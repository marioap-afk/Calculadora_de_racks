import sys, re, os
sys.stdout.reconfigure(encoding="utf-8")
P = sys.argv[1]

def sections(path):
    lines = open(path, encoding="utf-8").read().split("\n")
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

s4 = sections(os.path.join(P, "I-64-f4b-pbt4-gate-contract-package-v54.md"))
s5 = sections(os.path.join(P, "I-64-f4b-pbt4-gate-contract-package-v55.md"))
s6 = sections(os.path.join(P, "I-64-f4b-pbt4-gate-contract-package-v56.md"))
same56 = [t for t in s6 if t in s5 and s5[t] == s6[t] and t != "<pre>"]
same456 = [t for t in same56 if t in s4 and s4[t] == s5[t]]
extra = [t for t in same56 if t not in same456]
print("same v55-v56 (titled):", len(same56))
print("same since v54:", len(same456))
print("extra:", len(extra))
for t in extra:
    print("  ", t[:120])
pre = s5["<pre>"] == s6["<pre>"]
print("preamble equal:", pre)
