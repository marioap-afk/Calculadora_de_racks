import difflib
import hashlib
import json
import subprocess
import sys

# Uso: python -I cat3b_inspect.py <carpeta> <checkout> [<json del seco>]
# El contrato en seco de PB-T3b frente al de PB-T3a reemitido (485b5651) y emitido de nuevo en el cierre (c40eea4a).
sys.stdout.reconfigure(encoding="utf-8", errors="replace")
HERE, REPO = sys.argv[1], sys.argv[2]
DRY = sys.argv[3] if len(sys.argv) > 3 else "contract-dry-pbt3b.json"
T3A = "docs/automation/evidence/I-64-pilot/F4B-PBT3A-FULL-WRITE-MODEL/coordinator/gate-contract.json"


def show(rev, path):
    return subprocess.run(["git", "-C", REPO, "show", rev + ":" + path], capture_output=True, check=True).stdout


raw = open(HERE + "/" + DRY, "rb").read()
b = json.loads(raw.decode("utf-8"))
a = json.loads(show("485b5651", T3A).decode("utf-8-sig"))
print("dry", DRY, hashlib.sha256(raw).hexdigest().upper()[:16], len(raw), "| PB-T3a c40eea4a == 485b5651:", show("485b5651", T3A) == show("c40eea4a", T3A))
print("ASCII:", all(c < 128 for c in raw))
print("TaskId", b["TaskId"], len(b["TaskId"]), "| AuthRev", b["AuthorityRevision"][:8], "| Main", b["MainSha"][:8])
print("Objective:", b["Objective"][:200], "...")
print("Allowed", len(b["AllowedWriteScope"]))
for p in b["AllowedWriteScope"]:
    print("   ", p)
uc = [x for x in b["Authorities"] if x["Class"] == "UNIT_CHANGE"]
print("UNIT_CHANGE:", [(x["Path"], x["Section"][:80]) for x in uc])
print("Authorities", len(b["Authorities"]), "| clases:", sorted(set(x["Class"] for x in b["Authorities"])))
sa = {s["Id"]: s["Text"] for s in a["StopConditions"]}
sb = {s["Id"]: s["Text"] for s in b["StopConditions"]}
print("Stops", len(sb), list(sb))
for k, v in sb.items():
    ka = k.replace("PBT3B", "PBT3A")
    if ka in sa:
        va = sa[ka].replace("PBT3A", "PBT3B").replace("PB-T3a", "PB-T3b")
        if v == va:
            print("   ", k, "== PB-T3a")
        else:
            sm = difflib.SequenceMatcher(None, va, v, autojunk=False)
            ops = [(op, va[i1:i2][:160], v[j1:j2][:160]) for op, i1, i2, j1, j2 in sm.get_opcodes() if op != "equal"]
            print("   ", k, "difiere de PB-T3a en", len(ops), "tramos")
            for o in ops[:12]:
                print("        ", o)
    else:
        print("   ", k, "propia:", v[:300])
ia = a["Invariants"]
ib = b["Invariants"]
print("Invariants", len(ib))
for x in ib:
    n = x.split(". ")[0]
    na = n.replace("PBT3B", "PBT3A")
    same = [y for y in ia if y.split(". ")[0] == na]
    if same and same[0].split(". ", 1)[1] == x.split(". ", 1)[1]:
        print("   ", n, "== PB-T3a")
    else:
        print("   ", n, x[:260])
print("RequiredTests", len(b["RequiredTests"]))
ra = {(r["Project"], r["Filter"]): r for r in a["RequiredTests"]}
for r in b["RequiredTests"]:
    k = (r["Project"], r["Filter"])
    tag = "" if k not in ra else ("== PB-T3a" if ra[k]["MinSelected"] == r["MinSelected"] and ra[k]["ExpectRed"] == r["ExpectRed"] else "PB-T3a: %s/%s" % (ra[k]["MinSelected"], ra[k]["ExpectRed"]))
    print("   ", r["Project"].split("/")[1], r["Filter"].split("~")[1], r["MinSelected"], r["ExpectRed"], tag)
print("   PB-T3a y no PB-T3b:", [k[1].split("~")[1] for k in ra if k not in {(r["Project"], r["Filter"]) for r in b["RequiredTests"]}])
print("Evidence", len(b["ExpectedEvidence"]))
for k, e in enumerate(b["ExpectedEvidence"]):
    print("   [%d]" % k, e[:230])
print("EligibleCells == PB-T3a:", a["EligibleCells"] == b["EligibleCells"])
fa, fb = a["ForbiddenWriteScope"], b["ForbiddenWriteScope"]
print("Forbidden", len(fb), "| src", len([p for p in fb if p.startswith("src/")]), "| tests", len([p for p in fb if p.startswith("tests/")]))
print("   en PB-T3b y no en PB-T3a:", [p for p in fb if p not in fa])
print("   en PB-T3a y no en PB-T3b:", [p for p in fa if p not in fb])
print("   choque con Allowed:", [p for p in b["AllowedWriteScope"] if any(p == f or (f.endswith("/") and p.startswith(f)) for f in fb)])
print("IssuedBy:", b["IssuedBy"][:600])
for x in b["Authorities"]:
    if "<" in x["Section"] or "por rellenar" in x["Section"]:
        print("   marcador en autoridad:", x["Path"], x["Section"][:160])
