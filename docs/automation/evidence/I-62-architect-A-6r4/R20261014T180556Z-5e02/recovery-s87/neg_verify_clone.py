"""Prueba negativa de verify_clone.py (decisiones §87/§88): una copia ALTERADA del registro de caracterización externo debe hacer fallar la
verificación. Copia kit/ a prep/s87/neg-verify/kit/ (fuera de kit/; el kit real no se toca), altera UN byte de la copia del registro, apunta la
compuerta de la copia a ese archivo con su SHA-256 (para que MatchesGate siga en true y solo fallen el SHA-256 y el blob fijados) y ejecuta el
verify_clone.py de la copia. Se exige: código 1, AllChecks = false, ExternalCopyOk = false, InKit = true, MatchesGate = true, Sha256 y Blob distintos
de los fijados; y que el kit real no cambie.
Uso: python -I -B prep/s87/neg_verify_clone.py   (desde la carpeta que contiene kit/)
"""
import hashlib
import json
import os
import shutil
import subprocess
import sys
import time

sys.stdout.reconfigure(encoding="utf-8")
ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
KIT = os.path.join(ROOT, "kit")
SAND = os.path.join(ROOT, "prep", "s87", "neg-verify")
SKIT = os.path.join(SAND, "kit")
REC = "claude-cli-characterization.json"


def sha(b):
    return hashlib.sha256(b).hexdigest()


def kit_hashes():
    return {n: sha(open(os.path.join(KIT, n), "rb").read()) for n in sorted(os.listdir(KIT)) if os.path.isfile(os.path.join(KIT, n))}


before = kit_hashes()
assert not os.path.exists(SAND), "la carpeta de la prueba negativa ya existe"
os.makedirs(SKIT)
for n in before:
    shutil.copy2(os.path.join(KIT, n), os.path.join(SKIT, n))
b = bytearray(open(os.path.join(SKIT, REC), "rb").read())
i = b.index(b"ELIGIBLE")
b[i + 7] = ord("X")                       # «ELIGIBLE» → «ELIGIBLX»: un byte
open(os.path.join(SKIT, REC), "wb").write(bytes(b))
gate = json.load(open(os.path.join(SKIT, "transport-gate.json"), encoding="utf-8"))
gate["Characterization"] = {"Path": os.path.join(SKIT, REC), "Sha256": sha(bytes(b))}
with open(os.path.join(SKIT, "transport-gate.json"), "w", encoding="utf-8", newline="\n") as f:
    f.write(json.dumps(gate, ensure_ascii=False, indent=1) + "\n")
t0 = time.time()
r = subprocess.run([sys.executable, "-I", "-B", "-X", "utf8", os.path.join(SKIT, "verify_clone.py")], capture_output=True, text=True,
                   encoding="utf-8", errors="replace")
secs = round(time.time() - t0, 1)
doc = json.load(open(os.path.join(SKIT, "clone-verification.json"), encoding="utf-8"))
tc = doc["TransportCharacterization"]
after = kit_hashes()
checks = {"ExitCode1": r.returncode == 1, "AllChecksFalse": doc["AllChecks"] is False, "ExternalCopyOkFalse": tc["ExternalCopyOk"] is False,
          "InKitTrue": tc["InKit"] is True, "MatchesGateTrue": tc["MatchesGate"] is True, "Sha256DiffersFromPinned": tc["Sha256"] != tc["PinnedSha256"],
          "BlobDiffersFromPinned": tc["Blob"] != tc["PinnedBlob"], "BlobAtCommitIsPinned": tc["BlobAtCommit"] == tc["PinnedBlob"],
          "GuardsStillPass": doc["GuardsPreflight"]["Run"].get("Result") == "PASS" and doc["GuardsPreflight"]["SelfTest"].get("Result") == "PASS",
          "RealKitUnchanged": before == after}
out = {"Test": "verify_clone negativa: copia alterada (1 byte) del registro externo", "Seconds": secs, "ExitCode": r.returncode,
       "Stdout": r.stdout.strip()[-600:], "StderrTail": r.stderr.strip()[-400:], "TransportCharacterization": tc, "Checks": checks,
       "Result": "PASS" if all(checks.values()) else "FAIL"}
with open(os.path.join(ROOT, "prep", "s87", "neg-verify-result.json"), "w", encoding="utf-8", newline="\n") as f:
    f.write(json.dumps(out, ensure_ascii=False, indent=1) + "\n")
print(json.dumps({k: out[k] for k in ("Seconds", "ExitCode", "Checks", "Result")}, ensure_ascii=False, indent=1))
sys.exit(0 if out["Result"] == "PASS" else 1)
