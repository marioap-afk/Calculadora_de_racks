import sys, os, re
sys.stdout.reconfigure(encoding="utf-8")
P = sys.argv[1]   # pbt4
C = sys.argv[2]   # pbt4ecc/compile

def rd(p):
    return open(p, encoding="utf-8").read().replace("\r\n", "\n")

def expected(cls_text):
    out = []
    for m in re.finditer(r"Finish\(\s*log,\s*new\[\]\s*\{(.*?)\}\);", cls_text, re.S):
        out.append(re.findall(r'^\s*"((?:[^"\\]|\\.)*)",?\s*$', m.group(1), re.M))
    return out

core55 = [l for x in expected(rd(os.path.join(P, "v55", "ct", "PushBackEditAllReloadTests.cs"))) for l in x]
core56 = [l for x in expected(rd(os.path.join(P, "v56", "ct", "PushBackEditAllReloadTests.cs"))) for l in x]
ui56 = [l for x in expected(rd(os.path.join(P, "v56", "ct", "PushBackEditAllReloadFieldTests.cs"))) for l in x]
print("core55", len(core55), "core56", len(core56), "ui", len(ui56))
print("added in core56:", [l for l in core56 if l not in core55])
print("removed from core56:", [l for l in core55 if l not in core56])
print("order of 24 kept:", [l for l in core56 if l in core55] == core55)

def probe_lines(path):
    t = rd(path)
    return [l[2:] for l in t.split("\n") if l.startswith("  ") and not l.startswith("  msg:")]

for d in ("r76-tr", "r82-tr", "r83-p6"):
    for side in ("green", "red"):
        p = os.path.join(C, d, side + "-core-lineas.txt")
        if not os.path.exists(p):
            continue
        got = probe_lines(p)
        exp = core56
        # the probe prints its lines per CT; compare as multisets of expected
        missing = [l for l in exp if l not in got]
        print(d, side, "core probe lines", len(got), "expected lines missing:", missing[:5])
    p = os.path.join(C, d, "green-ui-lineas.txt")
    if os.path.exists(p):
        got = probe_lines(p)
        print(d, "green ui probe lines", len(got), "missing", [l for l in ui56 if l not in got])

pkg = rd(os.path.join(P, "I-64-f4b-pbt4-gate-contract-package-v56.md"))
s = pkg.index("### 5.7 PB-T4e")
e = pkg.index("**Las clases:**", s)
table = pkg[s:e]
for l in core56 + ui56:
    if ("`" + l + "`") not in table:
        print("NOT IN 5.7:", l)
print("5.7 has all", all(("`" + l + "`") in table for l in core56 + ui56))
