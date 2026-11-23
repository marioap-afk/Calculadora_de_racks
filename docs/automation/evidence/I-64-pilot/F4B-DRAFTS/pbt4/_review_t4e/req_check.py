import sys, json, re, os
sys.stdout.reconfigure(encoding="utf-8")
R = sys.argv[1]
P = sys.argv[2]
c = json.load(open(os.path.join(R, "pbt4d-contract.json"), encoding="utf-8"))
rt = c["RequiredTests"]
print("pbt4d RequiredTests", len(rt))
print("pbt4d ForbiddenWriteScope", len(c["ForbiddenWriteScope"]))
pkg = open(os.path.join(P, "I-64-f4b-pbt4-gate-contract-package-v55.md"), encoding="utf-8").read()
start = pkg.index("**`RequiredTests`** (90:")
end = pkg.index("#### 6.5.2", start)
rows = re.findall(r"^\| (\d+) \| `([^`]+)` \| `([^`]+)` \| (\d+) \| (true|false) \| ([^|]+) \|$", pkg[start:end], re.M)
print("package rows", len(rows))
bad = 0
for i, (n, proj, flt, ms, er, orig) in enumerate(rows[:88]):
    d = rt[int(n) - 1]
    same = d["Project"] == proj and d["Filter"] == flt
    if not same:
        bad += 1
        print("DIFF filter", n, proj, flt, d["Project"], d["Filter"])
    if int(ms) != d["MinSelected"]:
        print("MinSelected", n, flt, "v55", ms, "pbt4d", d["MinSelected"])
    if (er == "true") != d["ExpectRed"]:
        print("ExpectRed", n, flt, "v55", er, "pbt4d", d["ExpectRed"])
print("filter diffs", bad)
for r in rows[88:]:
    print("new", r)
# tsv
tsv = open(os.path.join(P, "..", "pbt4ecc", "compile", "required-pbt4e.tsv"), encoding="utf-8").read().splitlines()
print("tsv header", tsv[0][:300])
print("tsv rows", len(tsv) - 1)
for l in tsv[1:]:
    f = l.split("\t")
    if len(f) > 3 and (f[0] in ("84", "85", "86", "87", "88", "89", "90", "12", "15", "76")):
        print("tsv", l[:400])
