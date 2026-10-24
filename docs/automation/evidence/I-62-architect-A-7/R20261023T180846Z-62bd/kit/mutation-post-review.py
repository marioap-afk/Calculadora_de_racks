"""Mutation test of the auditor v7.0-a7r3 (post-review.py), of transport_gate.py (unchanged from A-6 round 4 and round 3: the D-01 gate and the
explicit Windows decoding of decisions §84, point 5), of owner_gate.py (A-7 round 3: Owner line of §78, CLAUDE-CLI-I62, the round-3 A-7 and agreed
A-6 blobs at the worktree HEAD and a Coordinator authorization after §96, pinned with its number, heading, blob and text SHA-256 and no later
section; delivered PENDING, so the audit cases pin a synthetic §97) and of fidelity.py, for the ONE formal review of ROUND 3 of amendment A-7
(decisions §96, points 6 and 7): «cada regla del auditor tiene un caso que falla sin ella», checked mechanically. The harness of the A-7 round-2
kit (R20261019T201030Z-d1bf, itself the round-1 harness with the read-fidelity extension of phase 1), unchanged except its names; it honours
RACKCAD_A7_KITDATA like the self-test (by default, the real clone and run of the kit). This is the FULL battery (every site of the four modules),
run once at the end of the kit's preparation (decisions §88, point 4: the auditor, v7.0-a7r3 with the reference-exactness rule, and the result
schema changed).

A rule site is a line that appends a reason or a problem: `add(…)`, `c.append(…)` and `row["Problems"].append(…)` in post-review.py, and
`probs.append(…)` in the three modules (aggregating lines such as `for p in row["Problems"]: add(…)` are sites too). Each site is disabled in turn
on a copy of its module (that one call becomes a no-op) and the FULL case list of selftest-post-review.py (without the live probe L00) runs against
the mutant (the stand-in executables of the decoding cases G50-G58 run for every mutant); the mutant is KILLED when at least one case FAILS, or
when the mutant makes a case raise an exception (an observable difference from the baseline, which passes every case; recorded in KilledBy as
«EXCEPTION <class>: <message>» and counted in Totals.KilledByException). The unmutated baseline must pass every case first (an exception there is
not caught).
Speed: Test-Json, the clone's git queries, the reparse walk and the run-file bytes are memoized across mutants (case 12 still simulates its
tampering on top of the memo). They are deterministic for a fixed input, and the memo is bypassed while the junction of case 03 exists, so
every mutant is judged on the same inputs as the baseline.
Every temporary file lives under <kit>/../.tmp/st and is removed at the end (the self-test's guard against links applies).
Read-fidelity correction (decisions §94, point 5): the battery also covers fidelity.py (every `mark(` of the detector and every `probs.append(` of
verify and run_files_of). A fidelity.py mutant is judged first on the cases of selftest-fidelity.py and, only if they all pass, on the full case
list of selftest-post-review.py with the auditor's fidelity module replaced (one failing case kills; Stage names the stage). For the other
mutants fidelity.verify and fidelity.layout are memoized by content (MemoFid; never for a fidelity.py mutant). The baseline also requires every
case of selftest-fidelity.py.
Usage: python -B mutation-post-review.py <out.json>
"""
import hashlib
import importlib.util
import json
import os
import re
import sys

sys.dont_write_bytecode = True
HERE = os.path.dirname(os.path.abspath(__file__))
_spec = importlib.util.spec_from_file_location("selftest_post_review", os.path.join(HERE, "selftest-post-review.py"))
ST = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(ST)
_spec = importlib.util.spec_from_file_location("selftest_fidelity", os.path.join(HERE, "selftest-fidelity.py"))
SF = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(SF)
SITE_RX = {"post-review.py": re.compile(r'(?<![.\w])add\(|(?<![.\w])c\.append\(|row\["Problems"\]\.append\(|(?<![.\w])probs\.append\('),
           "transport_gate.py": re.compile(r'(?<![.\w])probs\.append\('),
           "owner_gate.py": re.compile(r'(?<![.\w])probs\.append\('),
           "fidelity.py": re.compile(r'(?<![.\w])mark\(|(?<![.\w])probs\.append\(')}
MEMO = {"schema": {}, "git": {}, "reparse": None, "run": {}}
SHARED = {"canon": {}, "universe": {}}
FID_MEMO = {"verify": {}, "layout": {}}


class MemoFid:
    """The kit's fidelity module (decisions §94, point 5) with verify() and layout() memoized by CONTENT (path and SHA-256 of the bytes): the
    cases that alter a copy change its content, so the memo never hides a difference. Never used for a fidelity.py mutant."""
    def __init__(self, m):
        self._m = m

    def __getattr__(self, k):
        return getattr(self._m, k)

    def verify(self, path, data, copy_bytes):
        k = (path, hashlib.sha256(data).digest(), None if copy_bytes is None else hashlib.sha256(copy_bytes).digest())
        if k not in FID_MEMO["verify"]:
            FID_MEMO["verify"][k] = self._m.verify(path, data, copy_bytes)
        return list(FID_MEMO["verify"][k])

    def layout(self, copy_bytes):
        k = hashlib.sha256(copy_bytes).digest()
        if k not in FID_MEMO["layout"]:
            FID_MEMO["layout"][k] = self._m.layout(copy_bytes)
        return FID_MEMO["layout"][k]


def sites(name):
    src = open(os.path.join(HERE, name), encoding="utf-8").read().split("\n")
    out = []
    for i, line in enumerate(src):
        s = line.strip()
        if s.startswith("#") or s.startswith("def "):
            continue
        m = SITE_RX[name].search(line)
        if m:
            out.append((i, m.start(), m.end()))
    return src, out


def mutant_source(src, i, a, b):
    lines = list(src)
    lines[i] = lines[i][:a] + "_noop(" + lines[i][b:]
    return "_noop = lambda *a, **k: None\n" + "\n".join(lines)


def memoize(pr):
    validate, git, reparse, run_bytes = pr.validate_schema, pr.clone_git, pr.clone_reparse, pr.read_run_bytes

    def v(result, schema):
        k = json.dumps(result, sort_keys=True, ensure_ascii=False)
        if k not in MEMO["schema"]:
            MEMO["schema"][k] = validate(result, schema)
        return MEMO["schema"][k]

    def g(*a):
        if ST.CLONE_MUTATED:
            return git(*a)
        if a not in MEMO["git"]:
            MEMO["git"][a] = git(*a)
        return MEMO["git"][a]

    def r():
        if ST.CLONE_MUTATED:
            return reparse()
        if MEMO["reparse"] is None:
            MEMO["reparse"] = reparse()
        return list(MEMO["reparse"])

    def b(name):
        if name not in MEMO["run"]:
            MEMO["run"][name] = run_bytes(name)
        return MEMO["run"][name]
    pr.validate_schema, pr.clone_git, pr.clone_reparse, pr.read_run_bytes = v, g, r, b
    pr.canon_cache, pr.universe_cache = SHARED["canon"], SHARED["universe"]
    if not getattr(pr, "FID_IS_MUTANT", False) and not isinstance(pr.FID, MemoFid):
        pr.FID = MemoFid(pr.FID)
    return pr


def judge(pr):
    rows = ST.run_cases(memoize(pr))
    return [c["Case"] for c in rows if c["Result"] != "PASS"], len(rows)


def judge_fidelity(path):
    """A fidelity.py mutant: first the cases of selftest-fidelity.py on the mutant module; only if all pass, the FULL case list of
    selftest-post-review.py with the auditor's fidelity module replaced by the mutant (one failing case kills, so the verdict is that of running
    everything). → (failing cases, stage)."""
    fm = SF.load(path)
    fails = ["selftest-fidelity: " + c["Case"] for c in SF.cases(fm) if c["Result"] != "PASS"]
    if fails:
        return fails, "selftest-fidelity"
    pr = ST.load_auditor()
    pr.FID, pr.FID_IS_MUTANT = fm, True
    return judge(pr)[0], "selftest-post-review"


def main(out_path):
    mut_dir = os.path.join(ST.TMPROOT, "mut")
    os.makedirs(mut_dir, exist_ok=True)
    failing, n_cases = judge(ST.load_auditor())
    fid_failing = [c["Case"] for c in SF.cases(SF.load()) if c["Result"] != "PASS"]
    doc = {"Mutation": "post-review.py v7.0-a7r3 + transport_gate.py + owner_gate.py (A-7 ronda 3) + fidelity.py (decisiones §94, punto 5): cada sitio de "
                       "regla desactivado debe hacer fallar al menos un caso del selftest",
           "Environment": "SYNTHETIC" if ST._CLOSURE.get("Synthetic") else "REAL (clon y run del kit)",
           "Baseline": {"Cases": n_cases, "Failing": failing, "FidelityUnitFailing": fid_failing, "AllPass": not failing and not fid_failing},
           "Modules": {}, "Mutants": []}
    for name in ("post-review.py", "transport_gate.py", "owner_gate.py", "fidelity.py"):
        src, found = sites(name)
        doc["Modules"][name] = {"Sha256": hashlib.sha256(open(os.path.join(HERE, name), "rb").read()).hexdigest(), "Sites": len(found)}
        for k, (i, a, b) in enumerate(found):
            path = os.path.join(mut_dir, "%s.m%03d.py" % (name.split(".")[0].replace("-", "_"), k))
            open(path, "w", encoding="utf-8", newline="\n").write(mutant_source(src, i, a, b))
            stage = "selftest-post-review"
            try:
                if name == "fidelity.py":
                    fails, stage = judge_fidelity(path)
                else:
                    if name == "post-review.py":
                        pr = ST.load_auditor(path)
                    elif name == "transport_gate.py":
                        pr = ST.load_auditor()
                        pr.TG = ST.load("transport_gate_mutant", path)
                    else:
                        pr = ST.load_auditor()
                        pr.OG = ST.load("owner_gate_mutant", path)
                    fails, _ = judge(pr)
            except Exception as e:   # the mutant made a case raise: observable difference, recorded as a kill (see the docstring)
                fails = ["EXCEPTION %s: %s" % (type(e).__name__, str(e)[:200])]
            doc["Mutants"].append({"Module": name, "Line": i + 1, "Code": src[i].strip()[:150], "Killed": bool(fails), "Stage": stage,
                                   "KilledBy": fails[:4], "KillingCases": len(fails)})
            print("%-18s %4d %-6s %s" % (name, i + 1, "KILLED" if fails else "ALIVE", (fails[:2] if fails else src[i].strip()[:80])))
            sys.stdout.flush()
            os.remove(path)
    for name in doc["Modules"]:
        doc["Modules"][name]["Killed"] = sum(1 for m in doc["Mutants"] if m["Module"] == name and m["Killed"])
    doc["Survivors"] = [m for m in doc["Mutants"] if not m["Killed"]]
    doc["Totals"] = {"Mutants": len(doc["Mutants"]), "Killed": sum(1 for m in doc["Mutants"] if m["Killed"]),
                     "KilledByException": sum(1 for m in doc["Mutants"] if any(str(k).startswith("EXCEPTION ") for k in m["KilledBy"]))}
    doc["AllKilled"] = doc["Baseline"]["AllPass"] and not doc["Survivors"]
    text = ST.USER_RX.sub("%USERPROFILE%", json.dumps(doc, ensure_ascii=False, indent=1))
    with open(out_path, "w", encoding="utf-8", newline="\n") as f:
        f.write(text + "\n")
    print(json.dumps({"Baseline": doc["Baseline"]["AllPass"], "Totals": doc["Totals"], "AllKilled": doc["AllKilled"]}))
    ST.safe_remove_tmproot()
    return 0 if doc["AllKilled"] else 1


if __name__ == "__main__":
    sys.stdout.reconfigure(errors="backslashreplace")
    sys.exit(main(sys.argv[1]))
