"""Pruebas negativas de kit/verify_clone.py (decisiones §88, punto 3: negativos de los componentes cambiados). Cada variante copia kit/ a
prep/neg-verify/<variante>/kit/ (fuera de kit/; ni el kit real, ni el clon, ni el run se tocan), altera UNA cosa en la copia y ejecuta el
verify_clone.py de la copia (que lee el clon y el run reales y escribe solo en la copia y en su .tmp/ex1). Se exige: código 1, AllChecks = false,
la causa esperada y el kit real sin cambio.
  N1-registro-alterado: un byte de la copia del registro de caracterización (la compuerta de la copia apunta a ella con su SHA-256): ExternalCopyOk
      false, MatchesGate true (la variante de A-6 ronda 4, recovery-s87/neg_verify_clone.py);
  N2-orden-del-kit-alterada: un byte de la copia de order.txt del kit: IdenticalToKitCopy false para order.txt;
  N3-evid-de-la-autoprueba-de-guardas: la copia de I-62-A7/ para la autoprueba de las guardas con preflight.md incluido (lista de excluidos sin
      «preflight.md»): CandidateAdded = 30 y la autoprueba distinta de la custodiada (WholeEqualsCustodied false), aunque PASS;
  N4-cabeza-de-las-guardas: el resultado `run` reproducido con la cabeza completa en lugar de la abreviada: WholeEqualsCustodied false (Head).
Uso: python -I -B prep/neg_verify_clone.py   (desde la carpeta que contiene kit/)
"""
import hashlib
import json
import os
import shutil
import stat
import subprocess
import sys
import time

sys.stdout.reconfigure(encoding="utf-8")
ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
KIT = os.path.join(ROOT, "kit")
SAND = os.path.join(ROOT, "prep", "neg-verify")
REC = "claude-cli-characterization.json"


def sha(b):
    return hashlib.sha256(b).hexdigest()


def kit_hashes():
    return {n: sha(open(os.path.join(KIT, n), "rb").read()) for n in sorted(os.listdir(KIT)) if os.path.isfile(os.path.join(KIT, n))}


def subst(path, old, new):
    t = open(path, encoding="utf-8").read()
    assert t.count(old) == 1, (path, old[:60])
    open(path, "w", encoding="utf-8", newline="\n").write(t.replace(old, new))


def n1(skit):
    b = bytearray(open(os.path.join(skit, REC), "rb").read())
    i = b.index(b"ELIGIBLE")
    b[i + 7] = ord("X")
    open(os.path.join(skit, REC), "wb").write(bytes(b))
    gate = json.load(open(os.path.join(skit, "transport-gate.json"), encoding="utf-8"))
    gate["Characterization"] = {"Path": os.path.join(skit, REC), "Sha256": sha(bytes(b))}
    with open(os.path.join(skit, "transport-gate.json"), "w", encoding="utf-8", newline="\n") as f:
        f.write(json.dumps(gate, ensure_ascii=False, indent=1) + "\n")
    return lambda d: (d["TransportCharacterization"]["ExternalCopyOk"] is False and d["TransportCharacterization"]["MatchesGate"] is True and
                      d["GuardsPreflight"]["Run"].get("WholeEqualsCustodied") is True)


def n2(skit):
    b = bytearray(open(os.path.join(skit, "order.txt"), "rb").read())
    b[0] = ord("i")
    open(os.path.join(skit, "order.txt"), "wb").write(bytes(b))
    return lambda d: d["Run"]["Records"]["order.txt"]["IdenticalToKitCopy"] is False and d["Run"]["Records"]["prompt.md"]["IdenticalToKitCopy"] is True


def n3(skit):
    subst(os.path.join(skit, "verify_clone.py"), '"summary.json", "preflight.md", "MANIFEST.sha256"]', '"summary.json", "MANIFEST.sha256"]')
    return lambda d: (d["GuardsPreflight"]["SelfTest"].get("CandidateAdded") == 30 and d["GuardsPreflight"]["SelfTest"].get("WholeEqualsCustodied") is False
                      and d["GuardsPreflight"]["Run"].get("WholeEqualsCustodied") is True)


def n4(skit):
    subst(os.path.join(skit, "verify_clone.py"), 'GUARDS_RUN_HEAD = "6fee8fd6"', 'GUARDS_RUN_HEAD = "6fee8fd6a96ce4ae45d116e34affee4ed1b1fd40"')
    return lambda d: d["GuardsPreflight"]["Run"].get("WholeEqualsCustodied") is False and "Head" in d["GuardsPreflight"]["Run"].get("DifferingTopLevelKeys", [])


before = kit_hashes()
assert not os.path.exists(SAND), "la carpeta de las pruebas negativas ya existe"
results = []
for name, fn in (("N1-registro-alterado", n1), ("N2-orden-del-kit-alterada", n2), ("N3-evid-de-la-autoprueba-de-guardas", n3),
                 ("N4-cabeza-de-las-guardas", n4)):
    skit = os.path.join(SAND, name.split("-")[0], "kit")
    os.makedirs(skit)
    for n in before:
        shutil.copy2(os.path.join(KIT, n), os.path.join(skit, n))
    gate = json.load(open(os.path.join(skit, "transport-gate.json"), encoding="utf-8"))   # la copia de la compuerta apunta a la copia del registro
    gate["Characterization"]["Path"] = os.path.join(skit, REC)                             # (mismo SHA-256): solo cambia lo que altera la variante
    with open(os.path.join(skit, "transport-gate.json"), "w", encoding="utf-8", newline="\n") as f:
        f.write(json.dumps(gate, ensure_ascii=False, indent=1) + "\n")
    cause = fn(skit)
    t0 = time.time()
    r = subprocess.run([sys.executable, "-I", "-B", "-X", "utf8", os.path.join(skit, "verify_clone.py")], capture_output=True, text=True,
                       encoding="utf-8", errors="replace")
    secs = round(time.time() - t0, 1)
    try:
        d = json.load(open(os.path.join(skit, "clone-verification.json"), encoding="utf-8"))
        ok_cause = bool(cause(d))
        all_checks = d["AllChecks"]
    except (OSError, ValueError, KeyError) as e:
        d, ok_cause, all_checks = {}, False, None
    results.append({"Case": name, "Seconds": secs, "ExitCode": r.returncode, "AllChecks": all_checks, "ExpectedCause": ok_cause,
                    "StdoutTail": r.stdout.strip()[-500:], "StderrTail": r.stderr.strip()[-300:],
                    "Result": "PASS" if r.returncode == 1 and all_checks is False and ok_cause else "FAIL"})
    print(name, results[-1]["Result"], secs, flush=True)
after = kit_hashes()
doc = {"Test": "verify_clone.py, pruebas negativas (copias alteradas del kit; kit real, clon y run sin cambio)", "Cases": results,
       "RealKitUnchanged": before == after, "Result": "PASS" if all(x["Result"] == "PASS" for x in results) and before == after else "FAIL"}
text = json.dumps(doc, ensure_ascii=False, indent=1).replace(os.environ["USERPROFILE"].replace("\\", "\\\\"), "%USERPROFILE%")
with open(os.path.join(ROOT, "prep", "neg-verify-result.json"), "w", encoding="utf-8", newline="\n") as f:
    f.write(text + "\n")
shutil.rmtree(SAND, onexc=lambda func, path, exc: (os.chmod(path, stat.S_IWRITE), func(path)))
print(json.dumps({"Result": doc["Result"], "RealKitUnchanged": doc["RealKitUnchanged"], "Cases": [(x["Case"], x["Result"]) for x in results]},
                 ensure_ascii=False))
sys.exit(0 if doc["Result"] == "PASS" else 1)
