"""Negativa de verify_clone.py del kit de la ronda 3 (decisiones §96, punto 6: validar por incrementos según §88; verify_clone.py cambió), sobre
el clon REAL (solo lectura) y una copia del run con UN byte alterado en una copia fiel (la del TRX de r2 con BOM).

Se copian a <p3>/vcneg/kit/ los archivos del kit que lee verify_clone.py (kit-params.json, closure.json, transport-gate.json, fidelity.py,
claude-cli-characterization.json y las copias del kit de los archivos del run) y verify_clone.py con SOLO la constante RUN sustituida por
<p3>/vcneg/run; kit-params.json de esa copia lleva el mismo cambio en "Run" (la aserción de verify_clone.py lo exige). <p3>/vcneg/run es una copia
byte a byte del run real con el byte 200 de la copia fiel del TRX de RedSha de r3/ invertido (XOR 0x01). Se exige: código 1, AllChecks false; esa copia con
FaithfulCopyVerified false, VerifyProblems no vacío, CopySha256Matches false e IdenticalToKitCopy false; todas las demás copias verificadas; y el
clon limpio al terminar. El kit real, el run real y el clon no se tocan (el clon se lee con git; las guardas escriben en <p3>/vcneg/.tmp/ex1).
Uso: python -I -B neg_verify_clone_r3.py <kit> <run> <dir> <salida.json>
"""
import hashlib
import json
import os
import shutil
import subprocess
import sys

KIT, RUN, P3, OUT = (os.path.abspath(a) for a in sys.argv[1:5])
TARGET = "fiel__docs__automation__evidence__I-62-A7__r3__inputs__fixture-tests-red.trx.txt"
CONST = 'CLONE, RUN = r"D:\\r62-arch-a7r3", r"D:\\r62-arch-a7r3-run"'
NEG = os.path.join(P3, "vcneg")
NKIT, NRUN = os.path.join(NEG, "kit"), os.path.join(NEG, "run")
if os.path.isdir(NEG):
    shutil.rmtree(NEG)
os.makedirs(NKIT)
closure = json.load(open(os.path.join(KIT, "closure.json"), encoding="utf-8"))
for n in ("closure.json", "transport-gate.json", "fidelity.py", "claude-cli-characterization.json") + tuple(closure["RunFiles"]):
    shutil.copyfile(os.path.join(KIT, n), os.path.join(NKIT, n))
params = json.load(open(os.path.join(KIT, "kit-params.json"), encoding="utf-8"))
params["Run"] = NRUN
with open(os.path.join(NKIT, "kit-params.json"), "w", encoding="utf-8", newline="\n") as f:
    f.write(json.dumps(params, ensure_ascii=False, indent=1) + "\n")
src = open(os.path.join(KIT, "verify_clone.py"), encoding="utf-8").read()
assert src.count(CONST) == 1, "constante CLONE, RUN no encontrada una sola vez"
with open(os.path.join(NKIT, "verify_clone.py"), "w", encoding="utf-8", newline="\n") as f:
    f.write(src.replace(CONST, 'CLONE, RUN = r"D:\\r62-arch-a7r3", r"%s"' % NRUN))
shutil.copytree(RUN, NRUN)
p = os.path.join(NRUN, TARGET)
b = bytearray(open(p, "rb").read())
b[200] ^= 0x01
open(p, "wb").write(bytes(b))
r = subprocess.run([sys.executable, "-B", os.path.join(NKIT, "verify_clone.py")], capture_output=True, text=True, encoding="utf-8", errors="replace")
doc = json.load(open(os.path.join(NKIT, "clone-verification.json"), encoding="utf-8"))
run = doc["Run"]["Records"]
t = run[TARGET]
others = [e["Copy"] for e in closure["FaithfulCopies"] if e["Copy"] != TARGET]
status = subprocess.run(["git", "-C", r"D:\r62-arch-a7r3", "status", "--porcelain", "--ignored"], capture_output=True, text=True).stdout
res = {"Negative": "verify_clone.py (ronda 3) con un byte alterado en la copia fiel %s" % TARGET,
       "ExitCode": r.returncode, "AllChecks": doc.get("AllChecks"),
       "Target": {k: t.get(k) for k in ("FaithfulCopyVerified", "VerifyProblems", "CopySha256Matches", "IdenticalToKitCopy", "OriginalBlobMatches")},
       "OtherCopiesVerified": sum(1 for n in others if run[n].get("FaithfulCopyVerified")), "OtherCopies": len(others),
       "GuardsRun": doc.get("GuardsRun") or (doc.get("GuardsPreflight") or {}).get("Run", {}).get("Result"),
       "CloneCleanAfter": status.strip() == "",
       "RealKitClosureSha256": hashlib.sha256(open(os.path.join(KIT, "closure.json"), "rb").read()).hexdigest()}
res["Pass"] = (r.returncode == 1 and res["AllChecks"] is False and t.get("FaithfulCopyVerified") is False and bool(t.get("VerifyProblems")) and
               t.get("CopySha256Matches") is False and t.get("IdenticalToKitCopy") is False and res["OtherCopiesVerified"] == len(others) and
               res["CloneCleanAfter"])
text = json.dumps(res, ensure_ascii=False, indent=1).replace(os.environ.get("USERPROFILE", "\x00"), "%USERPROFILE%")
with open(OUT, "w", encoding="utf-8", newline="\n") as f:
    f.write(text + "\n")
print(text)
shutil.rmtree(NEG)
sys.exit(0 if res["Pass"] else 1)
