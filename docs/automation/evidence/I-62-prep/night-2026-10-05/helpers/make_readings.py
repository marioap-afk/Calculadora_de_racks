"""EXPERIMENTAL — NOT AUTHORIZED FOR PRODUCTION. Normalized, auditable readings of the helpers over REAL I-63 artifacts.
Usage: python make_readings.py <repo> <trx dir> <out.json>   (TRX bytes stay outside the repository; only their SHA-256 and readings are kept)"""
import json
import os
import subprocess
import sys

import i62_helpers as H

REPO, TRX_DIR, OUT = sys.argv[1:4]
EV = "docs/automation/evidence/I-63-pilot/"
CASES = {
    "G1": ("G1-RACK-METRICS", "G1-RACK-METRICS-nc1/R20261002T145409Z-6c34", "G1-RACK-METRICS-nc2/R20261002T145411Z-2850"),
    "G2": ("G2-POPULATION", "G2-POPULATION-nc1/R20261002T173233Z-22f2", "G2-POPULATION-nc2/R20261002T173234Z-9502"),
    "G3": ("G3-RACK-BUILTINS", "G3-RACK-BUILTINS-nc1/R20261002T220928Z-5706", "G3-RACK-BUILTINS-nc2/R20261002T220929Z-1c05"),
    "G4": ("G4-PROJECT-SUMMARY", "G4-PROJECT-SUMMARY-nc1/R20261005T012438Z-27b5", "G4-PROJECT-SUMMARY-nc2/R20261005T012440Z-1e98"),
}


def show(p):
    return json.loads(subprocess.check_output(["git", "-C", REPO, "show", "HEAD:" + p]))


files = subprocess.check_output(["git", "-C", REPO, "ls-tree", "-r", "--full-tree", "--name-only", "HEAD", EV]).decode().split()
doc = {"Label": "EXPERIMENTAL — NOT AUTHORIZED FOR PRODUCTION", "Repo": "HEAD de la rama de I-62 (objetos de I-63 integrados en main)",
       "Gates": {}, "Trx": {}}
for gate, (unit, nc1, nc2) in CASES.items():
    handoffs = sorted(f for f in files if f.startswith(EV + unit + "/") and f.endswith("/worker-handoff.json"))
    delegations = sorted(f for f in files if f.startswith(EV + unit + "/") and f.endswith("/delegation.json"))
    real_h, real_d = show(handoffs[-1]), show(delegations[-1])
    mut_h, mut_d = show(EV + nc1 + "/input-mutated-worker-handoff.json"), show(EV + nc2 + "/input-mutated-delegation.json")
    p1_real, p1_nc1 = H.p1_identity(real_h, REPO, real_d), H.p1_identity(mut_h, REPO, real_d)
    p2_real = H.p2_scope(REPO, real_d["BaseSha"], real_h["CurrentSha"], real_d)
    p2_nc2 = H.p2_scope(REPO, mut_d["BaseSha"], real_h["CurrentSha"], mut_d)
    doc["Gates"][gate] = {
        "Handoff": handoffs[-1], "Delegation": delegations[-1], "Range": "%s..%s" % (real_d["BaseSha"][:8], real_h["CurrentSha"][:8]),
        "P1Real": {"Status": p1_real["Status"], "Failures": p1_real["Failures"]},
        "P1Nc1": {"Status": p1_nc1["Status"], "Failures": p1_nc1["Failures"], "MutatedCurrentSha": mut_h.get("CurrentSha")},
        "P2Real": {"Status": p2_real["Status"], "Paths": len(p2_real["Paths"]), "Failures": p2_real["Failures"], "Gaps": p2_real["Gaps"]},
        "P2Nc2": {"Status": p2_nc2["Status"], "Failures": p2_nc2["Failures"], "Gaps": p2_nc2["Gaps"]},
        "ControllerOutcomeInI63": "nc1/nc2 NO CUMPLIDOS en G2 y G4 según la evidencia de I-63 (DEV-G2-01, DEV-I63-G4-02/03); comparar con P1Nc1/P2Nc2",
    }
declared = H.declared_skip_methods(REPO, "55a66b3c412fa63a445dc1985977ad660ea72dd7", "tests/RackCad.UI.Tests")
for name in sorted(os.listdir(TRX_DIR)):
    r = H.p6_trx(os.path.join(TRX_DIR, name), declared if "ui" in name else None)
    doc["Trx"][name] = {k: r[k] for k in ("Sha256", "Bytes", "Counters", "Outcomes", "Passed", "Skipped", "UnexpectedNotExecuted", "Notes", "Failures", "Status")}
    if "DeclaredSkips" in r:
        doc["Trx"][name]["DeclaredSkips"] = {k: (len(v) if isinstance(v, list) and k == "Known" else v) for k, v in r["DeclaredSkips"].items()}
doc["Provenance"] = {
    "i63-ready05-ui.trx / i63-ready05-core.trx": "scratchpad de la sesión de I-63 (…/2d2cc72a-…/scratchpad/ready05/tests/), READY-05 sobre el Candidato 55a66b3c "
                                                 "(evidencia de I-63 §61.1: UI 1654, 1637 superadas y 17 omitidas). Identidad por procedencia: su SHA-256 no figura en la evidencia",
    "i63-f0-focal.trx / i63-f0r1-focal.trx": "identidad por SHA-256 registrado en la evidencia de I-63 (F5A9D461…, A1F5A8FA…)",
    "NotFound": "core.trx 00779B64… (evidencia de I-63 §46-47) no está en disco",
}
H.assert_no_verdict(doc["Gates"])
with open(OUT, "w", encoding="utf-8", newline="\n") as f:
    json.dump(doc, f, ensure_ascii=False, indent=1)
    f.write("\n")
for g, v in doc["Gates"].items():
    print(g, "P1", v["P1Real"]["Status"], "nc1", v["P1Nc1"]["Status"], "| P2", v["P2Real"]["Status"], v["P2Real"]["Paths"], "nc2", v["P2Nc2"]["Status"], v["P2Nc2"]["Failures"][:1])
for n, v in doc["Trx"].items():
    print(n, v["Status"], v["Counters"], "passed", v["Passed"], "skipped", v["Skipped"], v.get("DeclaredSkips"))
