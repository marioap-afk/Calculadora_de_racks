"""Prueba de kit/verify_clone.py corregido (decisiones §94, punto 5) en el entorno de prueba de la fase 1: una copia del script con SOLO la línea
de las constantes CLONE y RUN sustituida por las rutas de prueba se ejecuta (intérprete -B, como el kit) desde un directorio de datos del kit:
  POS  p1/kitdata (cierre con 33 copias fieles) y p1/run: cada copia debe dar FaithfulCopyVerified y el listado del run = RunFiles del cierre;
  NEG  p1/vcneg/kitdata (copia de p1/kitdata con Run = p1/vcneg/run) y p1/vcneg/run (copia de p1/run con UN byte alterado en la copia del TRX
       de r1): esa copia debe dar FaithfulCopyVerified false (VerifyProblems no vacío y CopySha256Matches false) y las demás true.
AllChecks no se exige: en este entorno el session id del kit de r1 ya tiene transcripción (la revisión r1 se lanzó) y existen directorios de
proyecto D--r62-arch-a7* (el patrón del kit de r1), condiciones ambientales que se informan. Las guardas se reproducen como en el kit (--repo =
el worktree, solo lectura). Escribe solo bajo p1/.
Uso: python -I p1_verify_clone.py <kit> <p1> <salida.json>
"""
import hashlib
import json
import os
import shutil
import subprocess
import sys

KIT, P1, OUT = os.path.abspath(sys.argv[1]), os.path.abspath(sys.argv[2]), sys.argv[3]
TRX = "docs/automation/evidence/I-62-F6/kits/FX-02/a6m7-measure/inputs/fixture-tests.trx"
CONST = 'CLONE, RUN = r"D:\\r62-arch-a7", r"D:\\r62-arch-a7-run"'


ENV_TERMS = 'not project_dirs and doc["PriorTranscriptsForSessionId"]["Found"] == 0 and'


def patched(dst_dir, run_dir, env_neutral=False):
    src = open(os.path.join(KIT, "verify_clone.py"), encoding="utf-8").read()
    assert src.count(CONST) == 1 and src.count(ENV_TERMS) == 1, "línea de constantes o términos ambientales no encontrados"
    new = src.replace(CONST, 'CLONE, RUN = r"%s", r"%s"' % (os.path.join(P1, "clone"), run_dir))
    if env_neutral:   # SOLO los dos términos ambientales (sesión de r1 ya lanzada; directorios de proyecto de r1) se neutralizan
        new = new.replace(ENV_TERMS, "True and")
    p = os.path.join(dst_dir, "verify_clone.py")
    open(p, "w", encoding="utf-8", newline="\n").write(new)
    return p


def run(dst_dir, run_dir, env_neutral=False):
    p = patched(dst_dir, run_dir, env_neutral)
    r = subprocess.run([sys.executable, "-B", p], capture_output=True, text=True, encoding="utf-8", errors="replace",
                       env=dict(os.environ, GIT_OPTIONAL_LOCKS="0"))
    os.remove(p)
    doc = json.load(open(os.path.join(dst_dir, "clone-verification.json"), encoding="utf-8"))
    os.remove(os.path.join(dst_dir, "clone-verification.json"))
    return r.returncode, doc, r.stderr[-400:]


closure = json.load(open(os.path.join(P1, "kitdata", "closure.json"), encoding="utf-8"))
copies = [e["Copy"] for e in closure["FaithfulCopies"]]
trx_copy = next(e["Copy"] for e in closure["FaithfulCopies"] if e["Original"] == TRX)
rc1, pos, err1 = run(os.path.join(P1, "kitdata"), os.path.join(P1, "run"))
rc0, posn, err0 = run(os.path.join(P1, "kitdata"), os.path.join(P1, "run"), env_neutral=True)
neg_root = os.path.join(P1, "vcneg")
assert not os.path.exists(neg_root)
shutil.copytree(os.path.join(P1, "kitdata"), os.path.join(neg_root, "kitdata"))
shutil.copytree(os.path.join(P1, "run"), os.path.join(neg_root, "run"))
kp = os.path.join(neg_root, "kitdata", "kit-params.json")
params = json.load(open(kp, encoding="utf-8"))
params["Run"] = os.path.join(neg_root, "run")
open(kp, "w", encoding="utf-8", newline="\n").write(json.dumps(params, ensure_ascii=False, indent=1) + "\n")
cp = os.path.join(neg_root, "run", trx_copy)
b = bytearray(open(cp, "rb").read())
i = b.index(b"<TestRun ")
b[i] ^= 0x01
open(cp, "wb").write(bytes(b))
rc2, neg, err2 = run(os.path.join(neg_root, "kitdata"), os.path.join(neg_root, "run"))
shutil.rmtree(neg_root)
rec = lambda d: d["Run"]["Records"]
checks = {
    "PosRunListingIsRunFiles": pos["Run"]["Files"] == sorted(closure["RunFiles"]),
    "PosEveryCopyVerified": all(rec(pos)[c].get("FaithfulCopyVerified") is True and rec(pos)[c]["VerifyProblems"] == [] for c in copies) and len(copies) == 33,
    "PosRunFilesMatchKitAndClosure": all(v["IdenticalToKitCopy"] and v["MatchesClosure"] for v in rec(pos).values()),
    "PosGuardsReproduced": pos["GuardsPreflight"]["Run"].get("WholeEqualsCustodied") is True and pos["GuardsPreflight"]["SelfTest"].get("WholeEqualsCustodied") is True,
    "NegAlteredCopyNotVerified": rec(neg)[trx_copy]["FaithfulCopyVerified"] is False and bool(rec(neg)[trx_copy]["VerifyProblems"]) and
                                 rec(neg)[trx_copy]["CopySha256Matches"] is False and rec(neg)[trx_copy]["IdenticalToKitCopy"] is False,
    "NegOtherCopiesVerified": all(rec(neg)[c]["FaithfulCopyVerified"] for c in copies if c != trx_copy),
    "NegAllChecksFalse": neg["AllChecks"] is False,
    "PosAllChecksWithOnlyTheTwoEnvironmentalTermsNeutralized": posn["AllChecks"] is True,
}
env_notes = {"PosAllChecks": pos["AllChecks"], "PriorTranscriptsForSessionId": pos.get("PriorTranscriptsForSessionId"),
             "ClaudeProjectDirsForCloneOrRun": (pos.get("ClaudeProjectDirsForCloneOrRun") or {}).get("Found")}
out = {"Test": "verify_clone.py corregido (decisiones §94, punto 5) en el entorno de prueba de la fase 1",
       "VerifyCloneSha256": hashlib.sha256(open(os.path.join(KIT, "verify_clone.py"), "rb").read()).hexdigest(),
       "ReturnCodes": [rc1, rc2], "Checks": checks, "AllPass": all(checks.values()), "Environment": env_notes,
       "NegAlteredCopy": {"Copy": trx_copy, "Record": rec(neg)[trx_copy]}, "Stderr": [err1, err2]}
text = json.dumps(out, ensure_ascii=False, indent=1).replace(os.path.expanduser("~"), "%USERPROFILE%")
open(OUT, "w", encoding="utf-8", newline="\n").write(text + "\n")
for tag, d in (("pos", pos), ("pos-env-neutral", posn)):
    t = json.dumps(d, ensure_ascii=False, indent=1).replace(os.path.expanduser("~"), "%USERPROFILE%")
    open(OUT.replace(".json", "-%s.json" % tag), "w", encoding="utf-8", newline="\n").write(t + "\n")
print(json.dumps({"AllPass": out["AllPass"], "Checks": checks, "Environment": env_notes}, ensure_ascii=True)[:1500])
