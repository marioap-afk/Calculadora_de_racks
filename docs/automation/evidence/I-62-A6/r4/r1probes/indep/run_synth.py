"""Runs derive_indep.py on every vectors/syn-*.json, saves out/syn-*.json and checks them against
synvec/expect.json (exit code, Omitted, Commits, Refs), cross-checks RevPaths with an alternative git
query (cat-file --batch-check <c>:<p>), and checks Paths/Files of all_present exactly.

    python -I run_synth.py
"""

import hashlib
import json
import os
import subprocess
import sys

ROOT = os.path.dirname(os.path.abspath(__file__))


def load(p):
    with open(p, encoding="utf-8") as f:
        return json.load(f)


def revpaths_alt(vec, commits):
    E = load(vec["E"])
    pairs = sorted({(e["Path"], e["Blob"]) for k in ("CanonicalInputs", "AllowedTransitiveInputs", "AutomaticInstructions")
                    for e in E.get(k, [])})
    qs = [(c, p, b) for c in commits for (p, b) in pairs]
    if not qs:
        return set()
    out = subprocess.run(["git", "--no-replace-objects", "-C", vec["Repo"], "cat-file", "--batch-check"],
                         input="".join("%s:%s\n" % (c, p) for c, p, _ in qs).encode(), stdout=subprocess.PIPE).stdout
    got = set()
    for (c, p, b), ln in zip(qs, out.decode().splitlines()):
        f = ln.split()
        if len(f) == 3 and f[1] == "blob" and f[0] == b:
            got.add((c, p))
    return got


def main():
    exp = load(os.path.join(ROOT, "synvec", "expect.json"))
    os.makedirs(os.path.join(ROOT, "out"), exist_ok=True)
    fails = 0
    for name, e in exp["cases"].items():
        vpath = os.path.join(ROOT, "vectors", "syn-%s.json" % name)
        opath = os.path.join(ROOT, "out", "syn-%s.json" % name)
        if os.path.exists(opath):
            os.remove(opath)
        p = subprocess.run([sys.executable, "-I", os.path.join(ROOT, "derive_indep.py"), vpath, opath],
                           stdout=subprocess.PIPE, stderr=subprocess.PIPE)
        problems = []
        if p.returncode != e["exit"]:
            problems.append("exit %d != %d (%s)" % (p.returncode, e["exit"], p.stderr.decode().strip()))
        if p.returncode != 0:
            with open(opath + ".stderr.txt", "wb") as f:
                f.write(p.stderr)
        if p.returncode == 0 and e["exit"] == 0:
            out = load(opath)
            cl = out["Closure"]
            om = {r["Term"]: r["Cause"] for r in out["Omitted"]}
            if len(om) != len(out["Omitted"]):
                problems.append("duplicate Omitted terms")
            if om != e["omitted"]:
                problems.append("Omitted %s != expected %s" % (om, e["omitted"]))
            if sorted(cl["Commits"]) != e["commits"]:
                problems.append("Commits differ: extra %s missing %s" % (
                    sorted(set(cl["Commits"]) - set(e["commits"])), sorted(set(e["commits"]) - set(cl["Commits"]))))
            if sorted(cl["Refs"]) != e["refs"]:
                problems.append("Refs %s != %s" % (cl["Refs"], e["refs"]))
            vec = load(vpath)
            mine = {(r["Rev"], r["Path"]) for r in cl["RevPaths"]}
            if mine != revpaths_alt(vec, cl["Commits"]):
                problems.append("RevPaths differ from the alternative git query")
            for lst in ("Commits", "Refs", "Paths"):
                if len(set(cl[lst])) != len(cl[lst]):
                    problems.append("duplicates in " + lst)
            if name == "all_present":
                want_paths = {"docs/decisions/SYN-K.json", "src/a.txt", vec["Designation"]["D"], vec["Designation"]["H"],
                              "tests/t.trx", "AGENTS.md", "docs/map.json", "docs/w.md", "docs/read-target.md", "docs/",
                              "docs/map-file.md", "docs/auth-a.md", "docs/auth-b.md",
                              "artifacts/orchestration/SU/T1/0/RC001/output.json"}
                if set(cl["Paths"]) != want_paths:
                    problems.append("Paths differ: %s" % sorted(set(cl["Paths"]) ^ want_paths))

                def sha_file(f):
                    with open(f, "rb") as h:
                        return hashlib.sha256(h.read()).hexdigest()
                kb = [x["Blob"] for x in load(vec["E"])["CanonicalInputs"] if x["Path"] == vec["Designation"]["K"]][0]
                kbytes = subprocess.run(["git", "--no-replace-objects", "-C", vec["Repo"], "cat-file", "blob", kb],
                                        stdout=subprocess.PIPE).stdout
                want_files = {("CONTRACT", hashlib.sha256(kbytes).hexdigest()),
                              ("DELEG", sha_file(vec["Custody"][vec["Designation"]["D"]])),
                              ("H", sha_file(vec["Custody"][vec["Designation"]["H"]])),
                              ("TRX", sha_file(vec["Custody"]["tests/t.trx"]))}
                if {(f["Name"], f["Sha256"]) for f in cl["Files"]} != want_files:
                    problems.append("Files differ")
                if cl["Objects"] != [] or cl["Schema"] != "rackcad-bmin-closure/0-experimental":
                    problems.append("Objects/Schema")
        status = "PASS" if not problems else "FAIL"
        fails += bool(problems)
        print("%-22s %s %s" % (name, status, "; ".join(problems)))
    print("%d case(s) failed" % fails)
    return 1 if fails else 0


if __name__ == "__main__":
    sys.exit(main())
