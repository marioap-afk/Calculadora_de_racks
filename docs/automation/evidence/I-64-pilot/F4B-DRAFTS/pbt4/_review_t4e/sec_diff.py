import sys, re, hashlib, difflib
sys.stdout.reconfigure(encoding="utf-8")
base = sys.argv[1]
a = open(base + r"\I-64-f4b-pbt4-gate-contract-package-v54.md", encoding="utf-8").read().split("\n")
b = open(base + r"\I-64-f4b-pbt4-gate-contract-package-v55.md", encoding="utf-8").read().split("\n")

def sections(lines):
    out = []
    cur = ("<pre>", 0)
    buf = []
    in_code = False
    for i, l in enumerate(lines):
        if l.startswith("```"):
            in_code = not in_code
        if not in_code and re.match(r"^#{1,4} ", l):
            out.append((cur[0], cur[1], buf))
            cur = (l.strip(), i + 1)
            buf = []
        buf.append(l)
    out.append((cur[0], cur[1], buf))
    return out

sa = sections(a)
sb = sections(b)
print("v54 sections", len(sa), "v55 sections", len(sb))
from collections import Counter
ca = Counter(t for t, _, _ in sa)
cb = Counter(t for t, _, _ in sb)
print("dup titles v54", [k for k, v in ca.items() if v > 1])
print("dup titles v55", [k for k, v in cb.items() if v > 1])
da = {}
for t, ln, buf in sa:
    da.setdefault(t, []).append((ln, buf))
db = {}
for t, ln, buf in sb:
    db.setdefault(t, []).append((ln, buf))
same = 0
changed = []
for t, lst in db.items():
    for k, (ln, buf) in enumerate(lst):
        if t in da and k < len(da[t]):
            if da[t][k][1] == buf:
                same += 1
            else:
                changed.append((t, ln, da[t][k][0]))
        else:
            print("NEW", ln, t[:120])
for t in da:
    if t not in db:
        print("REMOVED", t[:120])
print("same", same)
for t, ln, la in changed:
    print("CHANGED v55:%d v54:%d %s" % (ln, la, t[:120]))
