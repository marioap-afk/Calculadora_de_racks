"""I-62 / FX-02 — clon temporal de lectura del fixture para la medición única de claude-cli.
Crea <destino> (por defecto D:\\r62-fixture\\tmp-ccli-meas; el destino debe empezar por tmp-ccli- y no existir) desde
D:\\r62-fixture\\fixture-origin.git, rama fx/u1, con main = d30fb6a9 y nada más:
  git clone --no-local --no-checkout --single-branch --branch fx/u1 --no-tags -c core.autocrlf=false <origen> <destino>
  merge-base --is-ancestor d30fb6a9 HEAD → checkout -B main d30fb6a9 → branch -D fx/u1 → remote remove origin
  → reflog expire --expire=now --all → gc --prune=now
y verifica el resultado con la puerta G3 de ccli_common.gate_clone (HEAD, solo main, sin remoto, limpio con ignorados, blob del objeto, sin
.claude/, sin enlaces, solo la historia de HEAD, core.autocrlf=false). Solo lee del origen; nunca escribe en él ni en otro repositorio.
Uso: python -I -B make_clone.py [--target <ruta>]     → imprime el JSON de la verificación (código 0 si G3 = Ok)
"""
import importlib.util
import json
import os
import subprocess
import sys

PKG = os.path.dirname(os.path.abspath(__file__))
spec = importlib.util.spec_from_file_location("ccli_common", os.path.join(PKG, "ccli_common.py"))
C = importlib.util.module_from_spec(spec)
spec.loader.exec_module(C)
K = C.load_constants(PKG)


def run(*a):
    r = subprocess.run(list(a), capture_output=True, text=True)
    if r.returncode != 0:
        raise RuntimeError("%s → %d: %s" % (" ".join(a[:4]), r.returncode, r.stderr.strip()[:300]))
    return r.stdout


def main(argv):
    target = argv[argv.index("--target") + 1] if "--target" in argv else K["Clone"]["Path"]
    k = K["Clone"]
    if not os.path.basename(os.path.normpath(target)).lower().startswith(k["RequiredPrefix"]):
        print(json.dumps({"Refused": "el destino debe empezar por %s" % k["RequiredPrefix"]}, ensure_ascii=False))
        return 2
    if os.path.exists(target):
        print(json.dumps({"Refused": "el destino ya existe: %s" % target}, ensure_ascii=False))
        return 2
    run("git", "clone", "-q", "--no-local", "--no-checkout", "--single-branch", "--branch", k["Branch"], "--no-tags",
        "-c", "core.autocrlf=false", k["Origin"], target)
    g = lambda *a: run("git", "-C", target, *a)
    anc = subprocess.run(["git", "-C", target, "merge-base", "--is-ancestor", k["Commit"], "HEAD"]).returncode == 0
    if not anc:
        print(json.dumps({"Refused": "%s no es ancestro de %s en el origen" % (k["Commit"], k["Branch"]), "Target": target}, ensure_ascii=False))
        return 2
    g("checkout", "-q", "-B", "main", k["Commit"])
    g("branch", "-q", "-D", k["Branch"])
    g("remote", "remove", "origin")
    g("reflog", "expire", "--expire=now", "--all")
    g("gc", "-q", "--prune=now")
    v = C.gate_clone(target, k["Commit"], k["ObjectPath"], k["ObjectBlob"], k["RequiredPrefix"])
    v["Target"] = target
    print(json.dumps(v, ensure_ascii=False, indent=1))
    return 0 if v["Ok"] else 1


if __name__ == "__main__":
    sys.stdout.reconfigure(encoding="utf-8", errors="backslashreplace")
    sys.exit(main(sys.argv[1:]))
