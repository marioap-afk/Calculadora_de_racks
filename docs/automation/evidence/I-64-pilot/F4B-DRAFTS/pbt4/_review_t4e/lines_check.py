import sys, os, re
sys.stdout.reconfigure(encoding="utf-8")
P = sys.argv[1]   # pbt4
C = sys.argv[2]   # pbt4ecc/compile

def rd(p):
    return open(p, encoding="utf-8").read().replace("\r\n", "\n")

def expected(cls_text):
    # all string literals inside Finish( log, new[] { ... } )
    out = []
    for m in re.finditer(r"Finish\(\s*log,\s*new\[\]\s*\{(.*?)\}\);", cls_text, re.S):
        out.append(re.findall(r'^\s*"((?:[^"\\]|\\.)*)",?\s*$', m.group(1), re.M))
    return out

core = expected(rd(os.path.join(P, "v55", "ct", "PushBackEditAllReloadTests.cs")))
ui = expected(rd(os.path.join(P, "v55", "ct", "PushBackEditAllReloadFieldTests.cs")))
print("core finishes", [len(x) for x in core], "ui finishes", [len(x) for x in ui])
allexp = [l for x in core for l in x] + [l for x in ui for l in x]
print("total expected lines", len(allexp))

pkg = rd(os.path.join(P, "I-64-f4b-pbt4-gate-contract-package-v55.md"))
s = pkg.index("### 5.7 PB-T4e")
e = pkg.index("**Las clases:**", s)
table = pkg[s:e]
pk = re.findall(r"`([a-z][a-z-]+ [^`]*)`", table)
pk = [x for x in pk if "=" in x]
print("package lines in 5.7 table", len(pk))
missing = [l for l in allexp if l not in pk]
extra = [l for l in pk if l not in allexp]
print("class lines missing from package:", missing)
print("package lines not in classes:", extra)
for name in ("green-core-lineas.txt", "green-ui-lineas.txt", "red-core-lineas.txt", "red-ui-lineas.txt"):
    t = rd(os.path.join(C, "r71-tr", name))
    print("==", name, len(t.splitlines()))
    print(t[:3000])
