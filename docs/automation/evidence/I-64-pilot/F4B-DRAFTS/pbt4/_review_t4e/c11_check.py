import sys, os, json, re, difflib
sys.stdout.reconfigure(encoding="utf-8")
R = sys.argv[1]
P = sys.argv[2]
c = json.load(open(os.path.join(R, "pbt4d-contract.json"), encoding="utf-8"))
d11 = [s["Text"] for s in c["StopConditions"] if s["Id"] == "PBT4D-C11"][0]
p01d = [s["Text"] for s in c["StopConditions"] if s["Id"] == "P-01"][0]
pkg56 = open(os.path.join(P, "I-64-f4b-pbt4-gate-contract-package-v56.md"), encoding="utf-8").read()
pkg54 = open(os.path.join(P, "I-64-f4b-pbt4-gate-contract-package-v54.md"), encoding="utf-8").read()
m = re.search(r"^PBT4E-C11: (.*)$", pkg56, re.M)
e11 = m.group(1)
m4 = re.search(r"^PBT4D-C11: (.*)$", pkg54, re.M)
d11pkg = m4.group(1) if m4 else None
print("PBT4D-C11 contract == package v5.4:", d11pkg == d11, len(d11), len(d11pkg or ""))
print("P-01 of PB-T4d contract:", p01d)
m5 = re.search(r"^P-01: (.*)$", pkg56, re.M)
print("P-01 lines in v56:", [x[:200] for x in re.findall(r"^P-01: .*$", pkg56, re.M)])
print("P-01 lines in v54 pkg:", [x[:200] for x in re.findall(r"^P-01: .*$", pkg54, re.M)])
# word-level diff
a = re.findall(r"\S+|\s+", d11)
b = re.findall(r"\S+|\s+", e11)
sm = difflib.SequenceMatcher(None, a, b, autojunk=False)
for tag, i1, i2, j1, j2 in sm.get_opcodes():
    if tag != "equal":
        print("%s\n  D: %s\n  E: %s" % (tag, "".join(a[i1:i2])[:600], "".join(b[j1:j2])[:600]))
print("len D", len(d11), "len E", len(e11))
open(os.path.join(R, "c11_d.txt"), "w", encoding="utf-8").write(d11)
open(os.path.join(R, "c11_e.txt"), "w", encoding="utf-8").write(e11)
