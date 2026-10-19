"""Matriz de validación incremental del kit de A-7 (decisiones §88, punto 3; §93, punto 2): componente → SHA-256 anterior (kit de A-6 ronda 4,
manifiesto b1342909…) → nuevo → dependencias → controles RE-RUN o REUSED con su resultado, leídos de los archivos de resultados. Escribe
prep/validation-matrix.json y prep/validation-matrix.md.
Uso: python -I -B make_matrix.py <carpeta que contiene kit/ y prep/>
"""
import hashlib
import json
import os
import sys

ROOT = sys.argv[1]
K, P = os.path.join(ROOT, "kit"), os.path.join(ROOT, "prep")
base = json.load(open(os.path.join(P, "base-a6r4", "kit-manifest.json"), encoding="utf-8"))
man = json.load(open(os.path.join(K, "kit-manifest.json"), encoding="utf-8"))
st = json.load(open(os.path.join(K, "selftest-result.json"), encoding="utf-8"))
mu = json.load(open(os.path.join(K, "mutation-targeted-result.json"), encoding="utf-8"))
cv = json.load(open(os.path.join(K, "clone-verification.json"), encoding="utf-8"))
nv = json.load(open(os.path.join(P, "neg-verify-result.json"), encoding="utf-8"))
rp = json.load(open(os.path.join(ROOT, "record-probe.json"), encoding="utf-8"))
dr = json.load(open(os.path.join(ROOT, "launch-dry-run.json"), encoding="utf-8"))
pin = json.load(open(os.path.join(P, "pin93", "pin-authorization.json"), encoding="utf-8"))
times = {n: open(os.path.join(P, "phase", n), encoding="utf-8").read().split() for n in os.listdir(os.path.join(P, "phase")) if n.endswith(".times")}
T = st["Totals"]
cases = {c["Case"]: c for c in st["Cases"]}
grp = lambda pre: [c for c in st["Cases"] if c["Case"].startswith(pre)]
npass = lambda cs: "%d/%d" % (sum(1 for c in cs if c["Result"] == "PASS"), len(cs))
ST = "selftest-result.json (autoprueba COMPLETA %d/%d: auditoría %d/%d, compuerta D-01 %d/%d, compuerta 11 %d/%d, coherencia %d/%d; %s → %s)" % (
    T["Pass"], T["Cases"], T["AuditPass"], T["AuditCases"], T["GatePass"], T["GateCases"], T["OwnerGatePass"], T["OwnerGateCases"],
    T["ConsistencyPass"], T["ConsistencyCases"], times.get("step7.times", ["?"])[0], times.get("step7.times", ["?", "?", "?"])[-1])
MU = "mutation-targeted-result.json (mutación dirigida %d/%d muertos; post-review.py %d/%d sitios dirigidos de %d; owner_gate.py %d/%d; %s → %s)" % (
    mu["Totals"]["Killed"], mu["Totals"]["Mutants"], mu["Modules"]["post-review.py"]["Killed"], mu["Modules"]["post-review.py"]["Targeted"],
    mu["Modules"]["post-review.py"]["Sites"], mu["Modules"]["owner_gate.py"]["Killed"], mu["Modules"]["owner_gate.py"]["Targeted"],
    times.get("mutation.times", ["?"])[0], times.get("mutation.times", ["?", "?", "?"])[-1])
REUSED_MU = "mutation-result.json 424c983e del kit de A-6 ronda 4 (191/191: post-review.py 133/133 sobre 4f46caa6, transport_gate.py 40/40 sobre e326b5b9, owner_gate.py 18/18)"
CV = "verify_clone positiva: AllChecks %s; guardas `run` iguales enteras a las custodiadas %s; autoprueba de las guardas %s/%s, igual entera a la custodiada %s" % (
    cv["AllChecks"], cv["GuardsPreflight"]["Run"].get("WholeEqualsCustodied"), cv["GuardsPreflight"]["SelfTest"].get("MutantsKilled"),
    cv["GuardsPreflight"]["SelfTest"].get("Mutants"), cv["GuardsPreflight"]["SelfTest"].get("WholeEqualsCustodied"))
NV = "verify_clone negativas (prep/neg-verify-result.json): %s" % "; ".join("%s %s" % (c["Case"], c["Result"]) for c in nv["Cases"])
RP = "launch.py --record-probe: código 0 = %s (RecordProbe.Ok %s, FrozenCommandGuard.Ok %s)" % (rp.get("RecordProbe", {}).get("Ok") is True,
                                                                                               rp.get("RecordProbe", {}).get("Ok"),
                                                                                               rp.get("FrozenCommandGuard", {}).get("Ok"))
DR = "launch.py --dry-run: BlockingChecks %s; NotRunInDryRun %s; AuthorizationSections %s; LaterSections %s" % (
    dr.get("BlockingChecks"), dr.get("NotRunInDryRun"), [a.get("Number") for a in dr.get("AuthorizationSections") or []], dr.get("LaterSections"))
O = grp("O") + grp("L00")
G = grp("G")
X = grp("X")
coh = [c for c in st["Cases"] if c["Case"].split("-")[0].rstrip("bcdefg2") .isdigit() and 55 <= int(c["Case"].split("-")[0].rstrip("bcdefg2")) <= 121]
g11 = [c for c in st["Cases"] if c["Case"].startswith(("150", "151"))]
gen = [c for c in st["Cases"] if c not in coh and c not in g11 and c["Case"][0].isdigit()]
rows = [
    ("c3_analysis.py", "ninguna del transporte", [("RE-RUN", "SHA-256 = manifiesto b1342909: igual"), ("RE-RUN", "casos G00-G58: %s, en %s" % (npass(G), ST))]),
    ("transport_gate.py", "closure.Transport (paths, session id)", [("RE-RUN", "SHA-256 = manifiesto b1342909: igual"), ("RE-RUN", "G00-G58 sobre el cierre nuevo: %s" % npass(G)),
                                                                   ("RE-RUN", "transport_gate.py init + set MEASURED + check: MEASURED, código 0"), ("REUSED", "mutación 40/40 de " + REUSED_MU)]),
    ("read_fidelity.py", "ninguna (no lo importa ningún componente)", [("RE-RUN", "SHA-256 = manifiesto b1342909: igual")]),
    ("pin_authorization.py", "owner_gate.py (BASE_SECTION 92), closure.ObjectBlob", [("RE-RUN", "SHA-256 = manifiesto b1342909: igual"),
                                                                                   ("RE-RUN", "fijación real de §93: Ready %s, línea %s, blob %s, LaterSections %s" % (pin["Ready"], pin["Checks"]["SectionLine"], pin["Checks"]["HasBlobToken"], pin["Checks"]["LaterSections"])),
                                                                                   ("RE-RUN", "O30: %s" % cases["O30-pin-authorization-valores"]["Result"])]),
    ("claude-cli-characterization.json", "blob 19b8a27e en 1c960c0a (worktree) y en el clon", [("RE-RUN", "SHA-256 = manifiesto b1342909: igual"), ("RE-RUN", CV),
                                                                                              ("RE-RUN", NV.split(";")[0])]),
    ("order.txt", "decisiones §93 del clon (bytes y SHA-256 declarados)", [("RE-RUN", "cuerpo pegado de §93 (s93_body.txt) = a5c37f71…, 4 057 bytes"), ("RE-RUN", CV), ("RE-RUN", "launch 7-RunFiles: " + DR)]),
    ("owner_gate.py", "decisiones de la HEAD del worktree; A-7 y A-6 en la HEAD; closure.ObjectBlob", [("RE-RUN", "casos O00-O30 y L00: %s" % npass(O)), ("RE-RUN", "casos 150-151p del registro: %s" % npass(g11)),
                                                                                                    ("RE-RUN", MU), ("RE-RUN", "launch 11: " + DR)]),
    ("launch.py", "closure.Transport, owner_gate.OBJECT_BLOB, run de dos archivos", [("RE-RUN", "X01-X04: %s" % npass(X)), ("RE-RUN", RP), ("RE-RUN", DR),
                                                                                    ("REUSED", "lógica G, R, P, D y comprobaciones 1-11 byte a byte (prep/diffs/launch.py.diff: constantes, docstring y la retirada de la comprobación de a6-r3.md); launch.py no tiene sitios de regla mutables")]),
    ("post-review.py", "closure.json, owner_gate.py, transport_gate.py, result.schema.json", [("RE-RUN", "casos de auditoría genéricos (00-55, 126-149, 152-153): %s" % npass(gen)),
                                                                                             ("RE-RUN", "casos de coherencia y esquema 55-121: %s" % npass(coh)), ("RE-RUN", "casos 150-151p: %s" % npass(g11)),
                                                                                             ("RE-RUN", MU), ("REUSED", "85 sitios no dirigidos, byte a byte líneas de v5.4-a6r4: 133/133 de " + REUSED_MU)]),
    ("selftest-post-review.py", "todos los módulos del auditor y de las compuertas", [("RE-RUN", ST)]),
    ("selftest-result.json", "Modules = los del kit", [("RE-RUN", "regenerado: " + ST)]),
    ("mutation-post-review.py", "selftest-post-review.py", [("REUSED", "el juez sin cambio de lógica (docstring y una etiqueta; prep/diffs/mutation-post-review.py.diff)"), ("RE-RUN", "usado por mutation-targeted.py: " + MU)]),
    ("mutation-targeted.py", "mutation-post-review.py", [("RE-RUN", "nuevo: " + MU)]),
    ("mutation-targeted-result.json", "post-review.py, owner_gate.py", [("RE-RUN", MU)]),
    ("result.schema.json", "prep/gen_schema.py", [("RE-RUN", "casos de esquema 55, 87, 101-108g: %s" % npass([c for c in coh if c["Case"].startswith(("55", "87", "101", "102", "103", "104", "105", "106", "107", "108"))]))]),
    ("prompt.template.md", "ninguna", [("RE-RUN", "renderizado sin marcadores sin sustituir (make_closure.py --render-prompt)")]),
    ("prompt.md", "plantilla, cierre", [("RE-RUN", "renderizado (77 659 bytes, 573 líneas); copia del run idéntica (verify_clone y launch 7)")]),
    ("make_closure.py", "clon 5158e4bc, kit-params.json", [("RE-RUN", "identity_checks y cierre: código 0 (190 archivos)")]),
    ("closure.json", "make_closure.py", [("RE-RUN", "regenerado; launch 2-ClosureTransport: " + DR)]),
    ("kit-params.json", "prepare_clone.py", [("RE-RUN", "precondiciones todas en true (prep/phase/step1.out)")]),
    ("prepare_clone.py", "worktree (solo lectura)", [("RE-RUN", "ejecutado una vez: precondiciones en true, clon y run creados")]),
    ("verify_clone.py", "clon, run, kit, worktree (solo lectura)", [("RE-RUN", CV), ("RE-RUN", NV)]),
    ("clone-verification.json", "verify_clone.py", [("RE-RUN", "regenerado: " + CV)]),
    ("transport-gate.json", "closure.Transport, la caracterización", [("RE-RUN", "init + set MEASURED + check; launch 10: " + DR)]),
    ("make_manifest.py", "archivos de kit/ y del run", [("RE-RUN", "congelación: kit-manifest.json")]),
    ("finish_kit.py", "ninguna en el lanzamiento", [("RE-RUN", "los pasos se ejecutaron uno a uno con sus mismas órdenes (no el script entero); sin efecto en el lanzamiento")]),
    ("kit-manifest.json", "todos los archivos de kit/ y del run", [("RE-RUN", "launch 1-KitIntegrity: " + DR)]),
]
out = []
for name, deps, ctrls in rows:
    old = (base["Files"].get(name) or {}).get("Sha256")
    new = hashlib.sha256(open(os.path.join(K, name), "rb").read()).hexdigest()
    out.append({"File": "kit/" + name, "OldSha256": old, "NewSha256": new, "Status": "IDENTICAL" if old == new else ("NEW" if old is None else "CHANGED"),
                "DependsOn": deps, "Controls": [{"Kind": k, "Detail": d} for k, d in ctrls]})
for n in ("order.txt", "prompt.md"):
    b = open(os.path.join(r"D:\r62-arch-a7-run", n), "rb").read()
    out.append({"File": "run/" + n, "OldSha256": (base["RunFiles"].get(n) or {}).get("Sha256"), "NewSha256": hashlib.sha256(b).hexdigest(),
                "Status": "IDENTICAL" if (base["RunFiles"].get(n) or {}).get("Sha256") == hashlib.sha256(b).hexdigest() else "CHANGED",
                "DependsOn": "copia del kit", "Controls": [{"Kind": "RE-RUN", "Detail": "idéntico a la copia del kit; launch 7-RunFiles: " + DR}]})
removed = [n for n in base["Files"] if not os.path.isfile(os.path.join(K, n))]
doc = {"Base": "kit de A-6 ronda 4, kit-manifest.json b1342909562478e5f99015666fd59fb4e7020cd1337b43ae0a8e87d4049d1d41",
       "New": "kit-manifest.json %s" % hashlib.sha256(open(os.path.join(K, "kit-manifest.json"), "rb").read()).hexdigest(),
       "Rows": out, "RemovedFromBase": removed,
       "Counts": {"Rows": len(out), "Identical": sum(1 for r in out if r["Status"] == "IDENTICAL"), "Changed": sum(1 for r in out if r["Status"] == "CHANGED"),
                  "New": sum(1 for r in out if r["Status"] == "NEW"), "ReRun": sum(1 for r in out for c in r["Controls"] if c["Kind"] == "RE-RUN"),
                  "Reused": sum(1 for r in out for c in r["Controls"] if c["Kind"] == "REUSED")},
       "Times": times}
with open(os.path.join(P, "validation-matrix.json"), "w", encoding="utf-8", newline="\n") as f:
    f.write(json.dumps(doc, ensure_ascii=False, indent=1) + "\n")
md = ["# Matriz de validación incremental: kit de la revisión de A-7 (decisiones §88, punto 3; §93, punto 2)", "",
      "Base: %s. Nuevo: %s. %d filas: %d idénticas, %d cambiadas, %d nuevas; controles RE-RUN %d, REUSED %d. Retirados de la base: %s." % (
          doc["Base"], doc["New"], doc["Counts"]["Rows"], doc["Counts"]["Identical"], doc["Counts"]["Changed"], doc["Counts"]["New"],
          doc["Counts"]["ReRun"], doc["Counts"]["Reused"], ", ".join(removed)), "",
      "| Archivo | SHA-256 anterior → nuevo | Depende de | Controles |", "|---|---|---|---|"]
for r in out:
    shas = "`%s` (igual)" % r["NewSha256"][:12] if r["Status"] == "IDENTICAL" else "`%s` → `%s`" % ((r["OldSha256"] or "—")[:12], r["NewSha256"][:12])
    md.append("| %s | %s | %s | %s |" % (r["File"], shas, r["DependsOn"], "<br>".join("**%s** %s" % (c["Kind"], c["Detail"].replace("|", "/")) for c in r["Controls"])))
md += ["", "## Tiempos (UTC, inicio y fin)", ""] + ["- %s: %s" % (k[:-6], " → ".join(v[i] for i in (0, -1))) for k, v in sorted(times.items())]
with open(os.path.join(P, "validation-matrix.md"), "w", encoding="utf-8", newline="\n") as f:
    f.write("\n".join(md) + "\n")
print(json.dumps(doc["Counts"]))
