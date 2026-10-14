"""Mutation test of the auditor v5.3-a6r3 (post-review.py), of transport_gate.py (round 3: the D-01 gate and the explicit Windows decoding of
decisions §84, point 5) and of owner_gate.py (round 3: Owner line of §78, CLAUDE-CLI-I62, round-3 blob at the worktree HEAD, decisions §84 with its
pinned heading, blob and text SHA-256, and no later section) for the ONE formal review of round 3 of A-6 (decisions §84), run BEFORE the launch:
«cada regla del auditor tiene un caso que falla sin ella», checked mechanically. The harness of the round-2 kit, unchanged except its names; it
honours RACKCAD_A6R3_KITDATA like the self-test (by default, the real clone and run of the kit).

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
SITE_RX = {"post-review.py": re.compile(r'(?<![.\w])add\(|(?<![.\w])c\.append\(|row\["Problems"\]\.append\(|(?<![.\w])probs\.append\('),
           "transport_gate.py": re.compile(r'(?<![.\w])probs\.append\('),
           "owner_gate.py": re.compile(r'(?<![.\w])probs\.append\(')}
MEMO = {"schema": {}, "git": {}, "reparse": None, "run": {}}
SHARED = {"canon": {}, "universe": {}}


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
    return pr


def judge(pr):
    rows = ST.run_cases(memoize(pr))
    return [c["Case"] for c in rows if c["Result"] != "PASS"], len(rows)


def main(out_path):
    mut_dir = os.path.join(ST.TMPROOT, "mut")
    os.makedirs(mut_dir, exist_ok=True)
    failing, n_cases = judge(ST.load_auditor())
    doc = {"Mutation": "post-review.py v5.3-a6r3 + transport_gate.py + owner_gate.py (ronda 3): cada sitio de regla desactivado debe hacer fallar al "
                       "menos un caso del selftest",
           "Environment": "SYNTHETIC" if ST._CLOSURE.get("Synthetic") else "REAL (clon y run del kit)",
           "Baseline": {"Cases": n_cases, "Failing": failing, "AllPass": not failing}, "Modules": {}, "Mutants": []}
    for name in ("post-review.py", "transport_gate.py", "owner_gate.py"):
        src, found = sites(name)
        doc["Modules"][name] = {"Sha256": hashlib.sha256(open(os.path.join(HERE, name), "rb").read()).hexdigest(), "Sites": len(found)}
        for k, (i, a, b) in enumerate(found):
            path = os.path.join(mut_dir, "%s.m%03d.py" % (name.split(".")[0].replace("-", "_"), k))
            open(path, "w", encoding="utf-8", newline="\n").write(mutant_source(src, i, a, b))
            if name == "post-review.py":
                pr = ST.load_auditor(path)
            elif name == "transport_gate.py":
                pr = ST.load_auditor()
                pr.TG = ST.load("transport_gate_mutant", path)
            else:
                pr = ST.load_auditor()
                pr.OG = ST.load("owner_gate_mutant", path)
            try:
                fails, _ = judge(pr)
            except Exception as e:   # the mutant made a case raise: observable difference, recorded as a kill (see the docstring)
                fails = ["EXCEPTION %s: %s" % (type(e).__name__, str(e)[:200])]
            doc["Mutants"].append({"Module": name, "Line": i + 1, "Code": src[i].strip()[:150], "Killed": bool(fails), "KilledBy": fails[:4],
                                   "KillingCases": len(fails)})
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
