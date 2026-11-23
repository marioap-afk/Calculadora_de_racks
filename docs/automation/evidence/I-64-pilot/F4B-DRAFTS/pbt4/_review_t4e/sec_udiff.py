import sys, re, difflib
sys.stdout.reconfigure(encoding="utf-8")
base = sys.argv[1]
want = sys.argv[2:]
a = open(base + r"\I-64-f4b-pbt4-gate-contract-package-v54.md", encoding="utf-8").read().split("\n")
b = open(base + r"\I-64-f4b-pbt4-gate-contract-package-v55.md", encoding="utf-8").read().split("\n")

def sections(lines):
    out = {}
    cur = "<pre>"
    buf = []
    in_code = False
    for l in lines:
        if l.startswith("```"):
            in_code = not in_code
        if not in_code and re.match(r"^#{1,4} ", l):
            out[cur] = buf
            cur = l.strip()
            buf = []
        buf.append(l)
    out[cur] = buf
    return out

sa, sb = sections(a), sections(b)
for t in sb:
    if any(t.startswith(w) for w in want) and t in sa:
        print("=" * 20, t[:100])
        for l in difflib.unified_diff(sa[t], sb[t], lineterm="", n=0):
            if l.startswith("---") or l.startswith("+++"):
                continue
            print(l[:1600])
