# Redaccion para el repo publico: las rutas locales se sustituyen por marcadores (<scratchpad>, <worktree-de-I-64>, <checkout-principal>, <datos-locales-del-usuario>). El resto no cambia.
import hashlib
import json
import subprocess
import sys

sys.stdout.reconfigure(encoding="utf-8", errors="replace")
REPO = "<checkout-principal>"
HERE = sys.argv[1]
T2C = "docs/automation/evidence/I-64-pilot/F4B-PBT2C-WORKSPACE-CARRYOVER/coordinator/gate-contract.json"
T1C = "docs/automation/evidence/I-64-pilot/F4B-PBT1C-LOSSLESS-STATE-WINDOW/coordinator/gate-contract.json"


def show(rev, path):
    r = subprocess.run(["git", "-C", REPO, "show", f"{rev}:{path}"], capture_output=True, check=True)
    return r.stdout


raw = open(HERE + "/contract-dry-pbt3a.json", "rb").read()
print("dry sha256", hashlib.sha256(raw).hexdigest().upper(), "bytes", len(raw))
a = json.loads(raw.decode("utf-8"))
c = json.loads(show("05beabfd", T2C).decode("utf-8-sig"))
t1 = json.loads(show("05beabfd", T1C).decode("utf-8-sig"))

sa = {s["Id"]: s["Text"] for s in a["StopConditions"]}
sc = {s["Id"]: s["Text"] for s in c["StopConditions"]}
st1 = {s["Id"]: s["Text"] for s in t1["StopConditions"]}
std = ["S-02", "S-03", "S-04", "S-05", "S-07", "S-08", "S-12", "S-13", "S-14"] + [f"P-0{i}" for i in range(1, 9)]
for k in std:
    print(k, "== PB-T2c:", sa.get(k) == sc.get(k))
for n in ("C07", "C18", "C08", "C12", "C17", "C11", "C01", "C03", "C05", "C06", "C09", "C10"):
    x = sa.get("PBT3A-" + n)
    y = sc.get("PBT2C-" + n)
    z = st1.get("PBT1C-" + n)
    yy = y.replace("PBT2C", "PBT3A").replace("PB-T2c", "PB-T3a") if y else None
    zz = z.replace("PBT1C", "PBT3A").replace("PB-T1c", "PB-T3a") if z else None
    rel = []
    if x == yy:
        rel.append("== PB-T2c")
    elif yy and x and x.rstrip(".") == yy.rstrip("."):
        rel.append("== PB-T2c salvo el punto final")
    elif yy and x and yy in x:
        rel.append("contiene PB-T2c")
    if x == zz:
        rel.append("== PB-T1c")
    print("PBT3A-" + n, rel or "difiere de PB-T2c y PB-T1c")
    if n == "C07":
        i = x.find("except under F4B-E35")
        j = y.find("except under F4B-E35")
        print("   C07 cola RR igual:", x[i:].rstrip(".") == y[j:].rstrip("."))

print("EligibleCells == PB-T2c:", a["EligibleCells"] == c["EligibleCells"])
print("Schema/Unit/Gate/Routing/Corrections:", [a[k] == c[k] for k in ("Schema", "Unit", "Gate", "RoutingEnforcement", "CorrectionsAuthorized")])

fa = a["ForbiddenWriteScope"]
fc = c["ForbiddenWriteScope"]
ta = [p for p in fa if p.startswith("tests/")]
tc = [p for p in fc if p.startswith("tests/")]
print("tests/ forbidden PB-T3a:", len(ta), "PB-T2c:", len(tc))
print("  en PB-T2c y no en PB-T3a:", [p for p in tc if p not in ta])
print("  en PB-T3a y no en PB-T2c:", [p for p in ta if p not in tc])
print("  allowed tests of PB-T2c:", [p for p in c["AllowedWriteScope"] if p.startswith("tests/")])
ra = [p for p in fa if not p.startswith(("src/", "tests/"))]
rc = [p for p in fc if not p.startswith(("src/", "tests/"))]
print("root == PB-T2c:", ra == rc, len(ra))

rqa = {(r["Project"], r["Filter"]): r for r in a["RequiredTests"]}
rqc = {(r["Project"], r["Filter"]): r for r in c["RequiredTests"]}
for k, r in rqa.items():
    if k in rqc:
        if rqc[k]["MinSelected"] != r["MinSelected"]:
            print("MinSelected difiere:", k[1], "PB-T3a", r["MinSelected"], "PB-T2c", rqc[k]["MinSelected"])
    else:
        print("solo en PB-T3a:", k[1], r["MinSelected"], r["ExpectRed"])
for k, r in rqc.items():
    if k not in rqa:
        print("solo en PB-T2c:", k[1], r["MinSelected"], r["ExpectRed"])
print("PB-T2c Allowed:", c["AllowedWriteScope"])
print("PB-T2c ExpectedEvidence count:", len(c["ExpectedEvidence"]))
for e in c["ExpectedEvidence"]:
    if "Linea base" in e or "linea base" in e or "19 0" in e or "19 1" in e:
        print("  PB-T2c baseline:", e[:400])
