import sys, os, re
sys.stdout.reconfigure(encoding="utf-8")
P = sys.argv[1]
frag = open(os.path.join(P, "v56", "frag", "c11.txt"), encoding="utf-8").read().rstrip("\n")
pkg = open(os.path.join(P, "I-64-f4b-pbt4-gate-contract-package-v56.md"), encoding="utf-8").read()
lines = re.findall(r"^PBT4E-C11: .*$", pkg, re.M)
print("C11 lines in v56", len(lines), "frag == package line:", lines[0] == frag if lines else None)
s = pkg.index("#### 6.5.1")
a = pkg.index("```text", s)
b = pkg.index("```", a + 7)
block = pkg[a + 8:b]
print("6.5.1 block non-ascii:", sum(1 for ch in block if ord(ch) > 127))
own = re.findall(r"^(PBT4E-C\d\d|P-01):", block, re.M)
print("stop ids in block", own, len(own))
inv = re.findall(r"^(INV-PBT4E-\d\d)\.", block, re.M)
print("invariants", len(inv))
# v55 vs v56 RequiredTests table identical?
p55 = open(os.path.join(P, "I-64-f4b-pbt4-gate-contract-package-v55.md"), encoding="utf-8").read()
def table(t):
    st = t.index("**`RequiredTests`** (90:")
    en = t.index("#### 6.5.2", st)
    return re.findall(r"^\| \d+ \|.*$", t[st:en], re.M)
t55, t56 = table(p55), table(pkg)
print("RequiredTests rows", len(t55), len(t56), "identical:", t55 == t56)
