"""EXPERIMENTAL — NOT AUTHORIZED FOR PRODUCTION. Tests of i62_helpers.py against REAL I-63 artifacts (read-only from the repository history) and
fake files. Usage: set I62_REPO=<repo> [I62_TRX_UI=<I-63 READY-05 ui.trx>] [I62_TRX_FOCAL=<I-63 focal trx>]; python -m unittest test_i62_helpers -v"""
import json
import os
import re
import shutil
import subprocess
import tempfile
import unittest

import i62_helpers as H

REPO = os.environ.get("I62_REPO", ".")
TRX_UI = os.environ.get("I62_TRX_UI")
TRX_FOCAL = os.environ.get("I62_TRX_FOCAL", "").split(os.pathsep) if os.environ.get("I62_TRX_FOCAL") else []
EV = "docs/automation/evidence/I-63-pilot/"
GATES = {
    "G1": ("G1-RACK-METRICS-nc1/R20261002T145409Z-6c34", "G1-RACK-METRICS-nc2/R20261002T145411Z-2850"),
    "G2": ("G2-POPULATION-nc1/R20261002T173233Z-22f2", "G2-POPULATION-nc2/R20261002T173234Z-9502"),
    "G3": ("G3-RACK-BUILTINS-nc1/R20261002T220928Z-5706", "G3-RACK-BUILTINS-nc2/R20261002T220929Z-1c05"),
    "G4": ("G4-PROJECT-SUMMARY-nc1/R20261005T012438Z-27b5", "G4-PROJECT-SUMMARY-nc2/R20261005T012440Z-1e98"),
}


def show_json(path):
    return json.loads(subprocess.check_output(["git", "-C", REPO, "show", "HEAD:" + path]))


def show_text(path):
    return subprocess.check_output(["git", "-C", REPO, "show", "HEAD:" + path]).decode("utf-8")


def real_pair(gate):
    """The real (non-mutated) handoff and delegation of the gate, located from the nc directories' siblings."""
    files = subprocess.check_output(["git", "-C", REPO, "ls-tree", "-r", "--name-only", "HEAD", EV]).decode().split()
    unit = GATES[gate][0].split("-nc1")[0]
    handoffs = sorted(f for f in files if f.startswith(EV + unit + "/") and f.endswith("/worker-handoff.json"))
    delegations = sorted(f for f in files if f.startswith(EV + unit + "/") and f.endswith("/delegation.json"))
    return handoffs, delegations


class P1Identity(unittest.TestCase):
    def test_nc1_mutated_handoffs_fail_even_with_the_right_sha_in_the_prompt(self):
        for gate, (nc1, _) in GATES.items():
            mutated = show_json(EV + nc1 + "/input-mutated-worker-handoff.json")
            r = H.p1_identity(mutated, REPO)
            self.assertEqual(r["Status"], "FAIL", gate)
            self.assertIn("CurrentSha", " ".join(r["Failures"]), gate)

    def test_g4_nc1_prompt_carries_the_green_sha_and_it_is_ignored(self):
        prompt = show_text(EV + GATES["G4"][0] + "/prompt.md")
        self.assertIn("4f45f446", prompt)                       # the narrative that misled the Controller in I-63
        mutated = show_json(EV + GATES["G4"][0] + "/input-mutated-worker-handoff.json")
        self.assertEqual(H.p1_identity(mutated, REPO)["Status"], "FAIL")

    def test_real_g4_handoff_passes_with_base_and_red(self):
        h = show_json(EV + "G4-PROJECT-SUMMARY/R20261003T005720Z-2b14/worker-handoff.json")
        r = H.p1_identity(h, REPO)
        self.assertEqual(r["Status"], "PASS", r)
        self.assertTrue(r["Facts"]["BaseShaIsAncestor"])
        self.assertTrue(r["Facts"]["RedShaBetween"])

    def test_malformed_sha(self):
        self.assertEqual(H.p1_identity({"CurrentSha": "4F45F446"}, REPO)["Status"], "FAIL")
        self.assertEqual(H.p1_identity({}, REPO)["Status"], "FAIL")


class P2Scope(unittest.TestCase):
    def test_nc2_mutated_delegations_fail_on_the_real_range(self):
        results = {}
        for gate, (_, nc2) in GATES.items():
            d = show_json(EV + nc2 + "/input-mutated-delegation.json")
            handoffs, _ = real_pair(gate)
            h = show_json(handoffs[-1])
            r = H.p2_scope(REPO, d["BaseSha"], h["CurrentSha"], d)
            results[gate] = r["Status"]
        self.assertEqual(set(results.values()), {"FAIL"}, results)

    def test_real_g4_delegation_passes_and_the_mutation_names_the_excluded_file(self):
        h = show_json(EV + "G4-PROJECT-SUMMARY/R20261003T005720Z-2b14/worker-handoff.json")
        d = show_json(EV + "G4-PROJECT-SUMMARY/R20261003T005345Z-66f3/delegation.json")
        self.assertEqual(H.p2_scope(REPO, d["BaseSha"], h["CurrentSha"], d)["Status"], "PASS")
        m = show_json(EV + GATES["G4"][1] + "/input-mutated-delegation.json")
        r = H.p2_scope(REPO, m["BaseSha"], h["CurrentSha"], m)
        self.assertTrue(any("ProjectPopulation.cs" in f for f in r["Failures"]), r["Failures"])

    def test_synthetic_prefix_nested_rename_delete_and_not_executed(self):
        root = tempfile.mkdtemp(prefix="p2-")
        try:
            g = lambda *a: subprocess.run(["git", "-c", "core.autocrlf=false", "-C", root] + list(a), check=True, capture_output=True)
            g("init", "-q", "-b", "main"); g("config", "user.email", "h@x"); g("config", "user.name", "h")
            for rel, txt in {"src/App/A.cs": "a", "src/App/Sub/B.cs": "b", "src/Other/C.cs": "c", "tests/T.cs": "t"}.items():
                os.makedirs(os.path.dirname(os.path.join(root, rel)), exist_ok=True)
                with open(os.path.join(root, rel), "w") as fh:
                    fh.write(txt)
            g("add", "-A"); g("commit", "-q", "-m", "base")
            base = subprocess.check_output(["git", "-C", root, "rev-parse", "HEAD"], text=True).strip()
            with open(os.path.join(root, "src/App/Sub/B.cs"), "w") as fh:
                fh.write("b2")
            g("mv", "src/Other/C.cs", "src/App/C.cs")
            g("rm", "-q", "tests/T.cs")
            g("add", "-A"); g("commit", "-q", "-m", "work")
            cur = subprocess.check_output(["git", "-C", root, "rev-parse", "HEAD"], text=True).strip()
            ok = {"AllowedWriteScope": ["src/App/", "src/Other/", "tests/T.cs"], "ForbiddenWriteScope": []}
            self.assertEqual(H.p2_scope(root, base, cur, ok)["Status"], "PASS")
            no_other = {"AllowedWriteScope": ["src/App/", "tests/T.cs"], "ForbiddenWriteScope": []}
            r = H.p2_scope(root, base, cur, no_other)
            self.assertEqual(r["Status"], "FAIL")                       # the rename is a delete of src/Other/C.cs
            self.assertTrue(any("src/Other/C.cs (D)" in f for f in r["Failures"]), r["Failures"])
            forb = {"AllowedWriteScope": ["src/", "tests/"], "ForbiddenWriteScope": ["src/App/Sub/"]}
            self.assertEqual(H.p2_scope(root, base, cur, forb)["Status"], "FAIL")   # forbidden wins over a broader allowed prefix
            nested_exact = {"AllowedWriteScope": ["src/App"], "ForbiddenWriteScope": []}
            self.assertEqual(H.p2_scope(root, base, cur, nested_exact)["Status"], "FAIL")   # no trailing slash = exact file, not a prefix
            glob = {"AllowedWriteScope": ["src/**"], "ForbiddenWriteScope": []}
            self.assertEqual(H.p2_scope(root, base, cur, glob)["Status"], "NOT_EVALUATED")
            self.assertEqual(H.p2_scope(root, base, "0" * 40, ok)["Status"], "NOT_EVALUATED")   # comparison not executed
            case = {"AllowedWriteScope": ["src/app/", "src/other/", "tests/T.cs"], "ForbiddenWriteScope": []}
            rc = H.p2_scope(root, base, cur, case)
            self.assertEqual(rc["Status"], "FAIL")
            self.assertTrue(rc["Gaps"])                                  # reported as a gap, not reinterpreted
        finally:
            shutil.rmtree(root, ignore_errors=True)


class P3FactsNotVerdicts(unittest.TestCase):
    def test_real_g4_facts_have_no_verdict(self):
        h = show_json(EV + "G4-PROJECT-SUMMARY/R20261003T005720Z-2b14/worker-handoff.json")
        d = show_json(EV + "G4-PROJECT-SUMMARY/R20261003T005345Z-66f3/delegation.json")
        facts = H.p3_facts(REPO, h, d)
        self.assertEqual(facts["Kind"], "FACTS")
        self.assertTrue(H.assert_no_verdict(facts))

    def test_injected_verdict_is_rejected(self):
        for bad in ({"Classification": "x"}, {"a": {"b": ["EXECUTION_VERIFIED"]}}, {"VerifiedSha": "0" * 40}, {"x": "gate pass"}):
            with self.assertRaises(ValueError):
                H.assert_no_verdict(bad)


class P5ConfigGate(unittest.TestCase):
    def setUp(self):
        self.dir = tempfile.mkdtemp(prefix="p5-")
        self.cfg, self.bin = os.path.join(self.dir, "config.toml"), os.path.join(self.dir, "codex.exe")
        self._w(self.cfg, "model = \"fake\"\n")
        self._w(self.bin, b"FAKEBIN1")
        self.gate = H.ConfigGate({"Config": H.sha256_file(self.cfg), "Binary": H.sha256_file(self.bin), "DecisionRef": "decisiones §1"})

    def tearDown(self):
        shutil.rmtree(self.dir, ignore_errors=True)

    @staticmethod
    def _w(path, data, mode=None):
        with open(path, mode or ("wb" if isinstance(data, bytes) else "w")) as fh:
            fh.write(data)

    def test_fake_files_only(self):
        real = os.path.join(os.path.expanduser("~"), ".codex", "config.toml")
        self.assertNotEqual(os.path.abspath(self.cfg), os.path.abspath(real))
        self.assertTrue(os.path.abspath(self.cfg).startswith(os.path.abspath(tempfile.gettempdir())))

    def test_match_drift_stop_reobserve_accept_resume(self):
        self.assertTrue(self.gate.may_resume(self.gate.observe(self.cfg, self.bin)))
        self._w(self.cfg, "sandbox = \"x\"\n", "a")                         # invalidation
        o2 = self.gate.observe(self.cfg, self.bin)
        self.assertEqual(self.gate.evaluate(o2)["State"], "STOP")
        o3 = self.gate.observe(self.cfg, self.bin)                           # new preflight: an observation, not a baseline
        self.assertFalse(self.gate.may_resume(o3))
        self.assertFalse(self.gate.accept(o3, "porque sí", o3["Id"])["Accepted"])
        self.assertFalse(self.gate.accept(o3, "decisiones §9", o2["Id"])["Accepted"])   # decision about another observation
        self.assertTrue(self.gate.accept(o3, "decisiones §9", o3["Id"])["Accepted"])
        self.assertTrue(self.gate.may_resume(self.gate.observe(self.cfg, self.bin)))

    def test_binary_drift_also_stops(self):
        self._w(self.bin, b"FAKEBIN2")
        self.assertEqual(self.gate.evaluate(self.gate.observe(self.cfg, self.bin))["Drift"], ["Binary"])


@unittest.skipUnless(TRX_UI and os.path.exists(TRX_UI or ""), "I62_TRX_UI no disponible")
class P6Trx(unittest.TestCase):
    def test_real_i63_ui_trx(self):
        r = H.p6_trx(TRX_UI)
        self.assertEqual(r["Status"], "PASS", r["Failures"])
        self.assertGreater(r["Skipped"], 0)
        self.assertEqual(r["Passed"] + r["Skipped"], r["Counters"]["total"])
        self.assertLess(r["Passed"], r["Counters"]["total"])               # skips are never counted as passed
        self.assertTrue(any("convención del logger" in n for n in r["Notes"]))

    def test_real_i63_ui_trx_against_declared_skips_at_the_candidate(self):
        declared = H.declared_skip_methods(REPO, "55a66b3c412fa63a445dc1985977ad660ea72dd7", "tests/RackCad.UI.Tests")
        r = H.p6_trx(TRX_UI, declared)
        self.assertEqual(r["DeclaredSkips"]["NotDeclared"], [], r["DeclaredSkips"])
        self.assertEqual(len(r["DeclaredSkips"]["Known"]), r["Skipped"])

    def _mutate(self, fn):
        with open(TRX_UI, encoding="utf-8") as fh:
            text = fh.read()
        d = tempfile.mkdtemp(prefix="p6-")
        self.addCleanup(shutil.rmtree, d, True)
        p = os.path.join(d, "m.trx")
        with open(p, "w", encoding="utf-8") as fh:
            fh.write(fn(text))
        return p

    def test_unexpected_not_executed(self):
        p = self._mutate(lambda t: re.sub(r"(outcome=\"NotExecuted\"[^>]*>\s*<Output>\s*<ErrorInfo>\s*<Message>)[^<]*", r"\1", t, count=1))
        r = H.p6_trx(p)
        self.assertEqual(r["Status"], "FAIL")
        self.assertEqual(len(r["UnexpectedNotExecuted"]), 1)

    def test_contradictory_counters(self):
        p = self._mutate(lambda t: re.sub(r'passed="(\d+)"', lambda m: 'passed="%d"' % (int(m.group(1)) + 1), t, count=1))
        self.assertTrue(any("contradicción" in f for f in H.p6_trx(p)["Failures"]))

    def test_empty_selection(self):
        p = self._mutate(lambda t: re.sub(r"<Results>.*</Results>", "<Results></Results>", t, flags=re.S))
        self.assertIn("selección vacía", H.p6_trx(p)["Failures"])

    def test_focal_real_trx_if_available(self):
        for f in TRX_FOCAL:
            if os.path.exists(f):
                r = H.p6_trx(f)
                self.assertIn(r["Status"], ("PASS", "FAIL"))
                self.assertEqual(r["Passed"], r["Outcomes"].get("Passed", 0))


if __name__ == "__main__":
    unittest.main()
