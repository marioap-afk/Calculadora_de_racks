"""Decisiones §97, punto 4: «Comprobar expresamente que el archivo auxiliar histórico preflight_text_r2.py no interviene en la evaluación del objeto
publicado». Comprobaciones mecánicas, solo lectura de git en el worktree de I-62 (GIT_OPTIONAL_LOCKS=0) y del kit:
  1. no existe en I-62-A7/r3/ en la publicación (37ac6ca4), en el registro (b9bfae50) ni en la HEAD;
  2. el de I-62-A7/r2/ (evidencia de la ronda 2, en el cierre como tal) conserva el blob a773a873 desde la publicación de la ronda 2 (e90b444d);
  3. A-7 ronda 3 no lo nombra;
  4. en r3/ solo lo nombra r2lib.py, dentro de la función main() (el ejecutor de la ronda 2, que renderizaba preflight.md de la ronda 2), y
     run_a7r3.py, el ejecutor de la ronda 3, no llama a main() de r2lib (análisis con ast);
  5. el resultado `run` de las guardas sobre la publicación real, reproducido por verify_clone.py entero e igual al custodiado (PASS), no lo contiene:
     ninguno de los archivos añadidos por la publicación se llama así;
  6. su único efecto fue estar en el área del preflight cuando la autoprueba de las GUARDAS construyó su commit hipotético (EVID_REMOVED_BEFORE_PUBLICATION
     de verify_clone.py): una autoprueba de las guardas, no una evaluación del objeto.
Uso: python -I -B check_preflight_text_r2.py <kit> <salida.json>
"""
import ast
import json
import os
import subprocess
import sys

KIT, OUT = sys.argv[1], sys.argv[2]
WT = os.path.join(os.environ["USERPROFILE"], ".codex", "worktrees", "architecture-portabilidad-coordinador-principal")
ENV = dict(os.environ, GIT_OPTIONAL_LOCKS="0")
R3, R2, A7 = "docs/automation/evidence/I-62-A7/r3/", "docs/automation/evidence/I-62-A7/r2/", "docs/initiatives/I-62-A-7.md"
PUB, REG, R2PUB = "37ac6ca4a6f0a04268ac357764bb74f843eb3a08", "b9bfae50a82ba59c834658e54d7699662ee298bb", "e90b444d5d23288e0b5a59881820377bee0d7d09"
g = lambda *a: subprocess.run(["git", "-C", WT] + list(a), capture_output=True, env=ENV, check=True).stdout
head = g("rev-parse", "HEAD").decode().strip()
res = {}
res["1-AbsentFromR3"] = {c[:8]: [n for n in g("ls-tree", "-r", "--name-only", c, "--", R3).decode().split("\n") if n.endswith("preflight_text_r2.py")]
                         for c in (PUB, REG, head)}
res["2-R2CopyBlob"] = {c[:8]: g("rev-parse", "%s:%spreflight_text_r2.py" % (c, R2)).decode().strip() for c in (R2PUB, PUB, head)}
res["3-A7MentionsIt"] = g("show", "%s:%s" % (PUB, A7)).decode("utf-8").count("preflight_text")
mentions = [l for l in g("grep", "-n", "preflight_text_r2", PUB, "--", R3).decode("utf-8").split("\n") if l]
lib = g("show", "%s:%sr2lib.py" % (PUB, R3)).decode("utf-8")
tree = ast.parse(lib)
encl = []
for node in ast.walk(tree):
    if isinstance(node, ast.FunctionDef):
        seg = lib.split("\n")[node.lineno - 1:node.end_lineno]
        if any("preflight_text_r2" in s for s in seg):
            encl.append(node.name)
runner = ast.parse(g("show", "%s:%srun_a7r3.py" % (PUB, R3)).decode("utf-8"))
calls_main = [n.lineno for n in ast.walk(runner) if isinstance(n, ast.Call) and isinstance(n.func, ast.Attribute) and n.func.attr == "main" and
              isinstance(n.func.value, ast.Name) and n.func.value.id == "R2"]
res["4-References"] = {"Lines": [m.split(":", 2)[1] + ":" + m.split(":", 2)[2][:80] for m in mentions], "EnclosingFunctionsInR2lib": encl,
                       "Run_a7r3CallsR2Main": calls_main}
vc = json.load(open(os.path.join(KIT, "clone-verification.json"), encoding="utf-8"))
run = vc["GuardsPreflight"]["Run"]
added = [n for n in g("diff", "--name-only", "--diff-filter=A", PUB + "^", PUB).decode().split("\n") if n]
res["5-GuardsRunOnPublication"] = {"Result": run.get("Result"), "WholeEqualsCustodied": run.get("WholeEqualsCustodied"), "Base": run.get("Base"),
                                   "Head": run.get("Head"), "AddedByPublication": len(added),
                                   "AddedNamedPreflightText": [n for n in added if "preflight_text" in n]}
st = vc["GuardsPreflight"]["SelfTest"]
res["6-GuardsSelfTestOnly"] = {"EvidRemovedBeforePublication": st.get("EvidRemovedBeforePublication"), "WholeEqualsCustodied": st.get("WholeEqualsCustodied")}
ok = (all(v == [] for v in res["1-AbsentFromR3"].values()) and set(res["2-R2CopyBlob"].values()) == {"a773a8733574182eafd768b0a74d70ecc41179bc"} and
      res["3-A7MentionsIt"] == 0 and len(mentions) == 1 and encl == ["main"] and calls_main == [] and run.get("Result") == "PASS" and
      run.get("WholeEqualsCustodied") is True and not res["5-GuardsRunOnPublication"]["AddedNamedPreflightText"])
res["NonIntervention"] = ok
with open(OUT, "w", encoding="utf-8", newline="\n") as f:
    f.write(json.dumps(res, ensure_ascii=False, indent=1) + "\n")
print(json.dumps(res, ensure_ascii=False))
sys.exit(0 if ok else 1)
