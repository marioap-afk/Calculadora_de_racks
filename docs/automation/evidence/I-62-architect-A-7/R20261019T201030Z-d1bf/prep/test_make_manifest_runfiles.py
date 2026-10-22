"""Prueba de make_manifest.run_files() corregido (decisiones §94, punto 5) en el entorno de prueba de la fase 1: el módulo se carga desde una copia
temporal en p1/kitdata con SOLO la constante RUN sustituida por p1/run (KIT = p1/kitdata, que tiene closure.json, fidelity.py y las copias del kit
de los archivos del run); run_files() debe devolver order.txt, prompt.md y las 33 copias fieles, cada una idéntica a la copia del kit, con el
SHA-256 de RunFileHashes del cierre. No escribe el manifiesto. Uso: python -I test_make_manifest_runfiles.py <kit> <p1> <salida.json>
"""
import importlib.util
import json
import os
import sys

KIT, P1, OUT = os.path.abspath(sys.argv[1]), os.path.abspath(sys.argv[2]), sys.argv[3]
DATA, RUN = os.path.join(P1, "kitdata"), os.path.join(P1, "run")
src = open(os.path.join(KIT, "make_manifest.py"), encoding="utf-8").read()
const = 'RUN = r"D:\\r62-arch-a7-run"'
assert src.count(const) == 1
tmp = os.path.join(DATA, "make_manifest_p1test.py")
open(tmp, "w", encoding="utf-8", newline="\n").write(src.replace(const, 'RUN = r"%s"' % RUN))
try:
    spec = importlib.util.spec_from_file_location("mm", tmp)
    mm = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(mm)
    rf = mm.run_files()
finally:
    os.remove(tmp)
closure = json.load(open(os.path.join(DATA, "closure.json"), encoding="utf-8"))
checks = {"Names": list(rf) == closure["RunFiles"] and len(rf) == 35,
          "IdenticalToKitCopy": all(v["IdenticalToKitCopy"] for v in rf.values()),
          "Sha256MatchesClosure": all(v["Sha256"] == closure["RunFileHashes"][n]["Sha256"] for n, v in rf.items())}
doc = {"Test": "make_manifest.run_files() con copias fieles", "Checks": checks, "AllPass": all(checks.values()), "RunFiles": len(rf)}
open(OUT, "w", encoding="utf-8", newline="\n").write(json.dumps(doc, ensure_ascii=False, indent=1) + "\n")
print(json.dumps(doc))
