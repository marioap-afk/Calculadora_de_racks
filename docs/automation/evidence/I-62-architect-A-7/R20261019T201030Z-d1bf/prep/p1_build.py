"""Datos del kit para el entorno de prueba de la FASE 1 (decisiones §94, punto 5): ejecuta el make_closure.py CORREGIDO del kit (importado como
módulo; sus funciones sin cambios) sobre el clon de prueba p1/clone (commit de fixture 3851370c sobre 5158e4bc) con el run de prueba p1/run, y
escribe en p1/kitdata lo que el self-test y la mutación toman con RACKCAD_A7_KITDATA: closure.json, prompt.md, las copias fieles y las copias de los
módulos de las compuertas y de fidelidad. El cierre es el de r1 (mismas listas y comprobaciones de identidad) más los 11 archivos del fixture
(zz-fidelity-fixture/, enteros), que solo existen en el clon de prueba. No toca el clon ni el run de r1, el worktree ni el repositorio.

Uso: python -B p1_build.py <kit> <p1>
"""
import hashlib
import importlib.util
import json
import os
import shutil
import subprocess
import sys

sys.dont_write_bytecode = True
KIT, P1 = os.path.abspath(sys.argv[1]), os.path.abspath(sys.argv[2])
CLONE, RUN, DATA = os.path.join(P1, "clone"), os.path.join(P1, "run"), os.path.join(P1, "kitdata")
FIXTURE_DIR = "zz-fidelity-fixture"
COPY_FROM_KIT = ["owner_gate.py", "transport_gate.py", "transport-gate.json", "pin_authorization.py", "fidelity.py", "claude-cli-characterization.json",
                 "order.txt", "c3_analysis.py"]

assert not os.path.exists(RUN) and not os.path.exists(DATA), "p1/run o p1/kitdata ya existen"
commit = subprocess.check_output(["git", "-C", CLONE, "rev-parse", "HEAD"]).decode().strip()
assert subprocess.check_output(["git", "-C", CLONE, "status", "--porcelain", "--ignored"]).decode().strip() == "", "clon de prueba no limpio"
os.makedirs(DATA)
os.makedirs(RUN)
for n in COPY_FROM_KIT:
    shutil.copyfile(os.path.join(KIT, n), os.path.join(DATA, n))
shutil.copyfile(os.path.join(KIT, "order.txt"), os.path.join(RUN, "order.txt"))
spec = importlib.util.spec_from_file_location("make_closure", os.path.join(KIT, "make_closure.py"))
MC = importlib.util.module_from_spec(spec)
spec.loader.exec_module(MC)
params = json.load(open(os.path.join(KIT, "kit-params.json"), encoding="utf-8"))
params.update({"Clone": CLONE, "Run": RUN, "CloneCommit": commit,
               "P1TestEnvironment": "clon de prueba de la fase 1 (decisiones §94, punto 5): 5158e4bc + commit de fixture %s" % commit[:8]})
with open(os.path.join(DATA, "kit-params.json"), "w", encoding="utf-8", newline="\n") as f:
    f.write(json.dumps(params, ensure_ascii=False, indent=1) + "\n")
fixture = sorted(n for n in subprocess.check_output(["git", "-C", CLONE, "ls-tree", "-r", "--name-only", commit, "--", FIXTURE_DIR + "/"])
                 .decode("utf-8").split("\n") if n)
assert len(fixture) == 11, fixture
MC.canonical.extend((p, "fixture de fidelidad de la fase 1 (solo en el clon de prueba; decisiones §94, punto 5): %s" % p.split("/")[-1], "ENTIRE",
                     MC.ranges_whole, "fixture de fidelidad, entero") for p in fixture)
copies = MC.build_kit(CLONE, commit, RUN, DATA, params, "copies")
for c in copies["Written"]:
    shutil.copyfile(os.path.join(DATA, c["Copy"]), os.path.join(RUN, c["Copy"]))
prompt = MC.build_kit(CLONE, commit, RUN, DATA, params, "render")
with open(os.path.join(DATA, "prompt.md"), "w", encoding="utf-8", newline="\n") as f:
    f.write(prompt)
shutil.copyfile(os.path.join(DATA, "prompt.md"), os.path.join(RUN, "prompt.md"))
summary = MC.build_kit(CLONE, commit, RUN, DATA, params, "closure")
sha = lambda p: hashlib.sha256(open(p, "rb").read()).hexdigest()
print(json.dumps({"Commit": commit, "Canonical": summary["canonical"], "Bytes": summary["bytes"], "Copies": len(copies["Written"]),
                  "Flagged": {c["Original"]: c["Flags"] for c in copies["Written"]},
                  "ClosureSha256": sha(os.path.join(DATA, "closure.json")), "PromptSha256": sha(os.path.join(DATA, "prompt.md")),
                  "RunListing": sorted(os.listdir(RUN))}, ensure_ascii=False, indent=1))
