import sys, os
sys.stdout.reconfigure(encoding="utf-8")
P = sys.argv[1]
pkg = open(os.path.join(P, "I-64-f4b-pbt4-gate-contract-package-v55.md"), encoding="utf-8").read()
s = pkg.index("#### 6.5.1")
a = pkg.index("```text", s)
b = pkg.index("```", a + 7)
block = pkg[a + 8:b]
bad = [(i, ch) for i, ch in enumerate(block) if ord(ch) > 127]
print("6.5.1 block chars", len(block), "non-ascii", len(bad))
for i, ch in bad[:20]:
    print(repr(block[max(0, i - 40):i + 10]))
# the Objective line length and the TaskId
for line in block.split("\n"):
    if line.startswith("TaskId:"):
        t = line.split(":", 1)[1].strip()
        print("TaskId", t, len(t))
