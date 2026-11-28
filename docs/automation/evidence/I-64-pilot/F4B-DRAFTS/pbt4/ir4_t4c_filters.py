"""CA-T4c items 4 and 5 (D-S8, RequiredTests): the 85 rows of the v5.0 table against the 77 of the emitted PB-T4b contract (number,
filter, MinSelected, ExpectRed), and the nine new filters against every test class of the test projects at the revision and the nine new
classes, by substring as FullyQualifiedName~ does (read-only).

Usage: python -I ir4_t4c_filters.py <repo> <rev> <package-v50.md> <pbt4b-gate-contract.json> <out.txt>
"""
import json
import re
import subprocess
import sys

repo, rev, pkg_p, contract_p, out_p = sys.argv[1:6]


def git(*a):
    return subprocess.run(["git", "-C", repo] + list(a), capture_output=True).stdout.decode("utf-8", "replace").replace("\r\n", "\n")


pkg = open(pkg_p, encoding="utf-8").read().replace("\r\n", "\n")
start = pkg.index("**`RequiredTests`** (76 heredados + 9 nuevos = 85)")
rows = [(int(n), p, f, m.strip().strip("*"), e) for n, p, f, m, e in
        re.findall(r"^\| (\d+) \| `([^`]+)` \| `FullyQualifiedName~([\w.]+)` \| ([^|]+) \| (true|false) \|$", pkg[start:start + 40000], re.M)]
c = json.load(open(contract_p, encoding="utf-8"))
old = [(k + 1, r["Project"], r["Filter"].replace("FullyQualifiedName~", ""), str(r["MinSelected"]), "true" if r["ExpectRed"] else "false")
       for k, r in enumerate(c["RequiredTests"])]
L = ["v5.0 rows %d; PB-T4b contract rows %d" % (len(rows), len(old))]
by_n = {r[0]: r for r in rows}
for n, p, f, m, e in old:
    r = by_n.get(n)
    if r is None:
        L.append("  PB-T4b #%d %s: not in v5.0 (MinSelected was %s)" % (n, f, m))
    elif (r[1], r[2]) != (p, f):
        L.append("  #%d DIFFERENT filter: %s -> %s" % (n, f, r[2]))
    elif r[3].replace(" ", "") != m or r[4] != "false":
        L.append("  #%d %s: MinSelected %s -> %s, ExpectRed %s -> %s" % (n, f, m, r[3], e, r[4]))
new = [r for r in rows if r[0] > len(old)]
L.append("new rows: %s" % [(r[0], r[2].split(".")[-1], r[3], r[4]) for r in new])
# classes at the revision
classes = set()
for proj in ("tests/RackCad.Tests", "tests/RackCad.UI.Tests"):
    ns = "RackCad.Tests." if proj.endswith("RackCad.Tests") else "RackCad.UI.Tests."
    for f in git("ls-tree", "-r", "--name-only", rev, "--", proj).split("\n"):
        if f.endswith(".cs"):
            for cl in re.findall(r"^\s*(?:public |internal )?(?:sealed |static |partial |abstract )*class (\w+)", git("show", "%s:%s" % (rev, f)), re.M):
                classes.add(ns + cl)
new_classes = set(r[2] for r in new)
L.append("classes at the revision: %d; new classes already there: %s" % (len(classes), sorted(new_classes & classes)))
allc = classes | new_classes
for r in new:
    sel = sorted(x for x in allc if r[2] in x + ".X")
    L.append("  new #%d %s selects: %s" % (r[0], r[2].split(".")[-1], [s.split(".")[-1] for s in sel]))
bad = []
for r in rows:
    if r[0] > len(old):
        continue
    for x in sorted(new_classes):
        if r[2] in x + ".X":
            bad.append((r[0], r[2], x))
L.append("inherited filters selecting a new class: %s" % bad)
open(out_p, "w", encoding="utf-8").write("\n".join(L) + "\n")
print("\n".join(L))
