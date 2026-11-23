import sys, json, os, re
sys.stdout.reconfigure(encoding="utf-8")
R = sys.argv[1]
P = sys.argv[2]
c = json.load(open(os.path.join(R, "pbt4d-contract.json"), encoding="utf-8"))
ids = [s["Id"] for s in c["StopConditions"]]
print("pbt4d stops", len(ids), ids)
for s in c["StopConditions"]:
    if s["Id"] in ("PBT4D-C15", "PBT4D-C11", "PBT4D-C16"):
        print(s["Id"], s["Text"][:700])
print("invariants", len(c["Invariants"]), [i[:14] for i in c["Invariants"]] if isinstance(c["Invariants"][0], str) else [i.get("Id") for i in c["Invariants"]])
pkg = open(os.path.join(P, "I-64-f4b-pbt4-gate-contract-package-v55.md"), encoding="utf-8").read()
s = pkg.index("#### 6.5.1")
e = pkg.index("#### 6.5.2", s)
own = re.findall(r"^(PBT4E-C\d\d):", pkg[s:e], re.M)
inv = re.findall(r"^(INV-PBT4E-\d\d)\.", pkg[s:e], re.M)
print("pbt4e own stops", len(own), own)
print("pbt4e invariants", len(inv), inv)
