"""Matriz de validación incremental (decisiones §88, punto 3) de la recuperación del transporte del kit R20261014T180556Z-5e02 (decisiones §87,
opción A): una fila por archivo de kit/ (y los tres del run) con SHA-256 anterior (kit fijado a §86, manifiesto cf71191e) → nuevo (kit vuelto a
congelar), dependencias, controles exigibles y, por control, REUSED (con la justificación) o RE-RUN (con el resultado). Escribe
prep/s87/validation-matrix.json y prep/s87/validation-matrix.md.
Uso: python -I -B prep/s87/make_matrix.py   (desde la carpeta que contiene kit/)
"""
import hashlib
import json
import os
import sys

sys.stdout.reconfigure(encoding="utf-8")
ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
KIT = os.path.join(ROOT, "kit")
S87 = os.path.join(ROOT, "prep", "s87")
OLD = json.load(open(os.path.join(S87, "pinned-kit", "kit-manifest.json"), encoding="utf-8"))
NEW = json.load(open(os.path.join(KIT, "kit-manifest.json"), encoding="utf-8"))


def sha(p):
    return hashlib.sha256(open(p, "rb").read()).hexdigest()


ST = "selftest-result.json 5ac8c720 (autoprueba COMPLETA, 297/297, 2026-10-16T19:27:32Z-20:41:33Z, HEAD del worktree 1c960c0a; terminada antes de §88)"
MU_OLD = "mutation-result.json 424c983e (191/191: post-review.py 133/133 sobre 4f46caa6, transport_gate.py 40/40 sobre e326b5b9, owner_gate.py 18/18 sobre d442608a; 2026-10-15)"
MU_PART = ("mutación parcial sobre el código actual, detenida por §88 (prep/s87/phase/mutation.raw, 20:41:33Z-23:56:49Z): post-review.py 133/133 "
           "muertos y transport_gate.py 19/40 muertos, 0 supervivientes; no escribió mutation-result.json")
VC = "verify_clone positiva: AllChecks true (19:20:18Z-19:27:05Z en 1c960c0a y 00:02:01Z-00:04:43Z en e83676bb); guardas run y self-test iguales enteras a las custodiadas (815/815)"
NEG = "verify_clone negativa (prep/s87/neg-verify-result.json): copia del registro con 1 byte alterado → código 1, AllChecks false, ExternalCopyOk false, MatchesGate true; kit real sin cambio; 179,5 s"
DRY = "launch.py --dry-run (00:10:06Z, 19,7 s): 1-3 y 6-10 Ok; 4 y 5 no se ejecutan en seco; BlockingChecks = [\"11-OwnerAndAuthorizationGate\"] solo por §88 (esperado, §88 punto 5)"
RP = "launch.py --record-probe (00:10:05Z, 1,4 s): código 0, RecordProbe.Ok y FrozenCommandGuard.Ok"


def R(control, status, detail):
    return {"Control": control, "Status": status, "Detail": detail}


ROWS = {
    "a6-r3.md": ("premisa del run (blob e62f27b8)", "blob e62f27b8 del worktree; copia del run",
                 [R("SHA-256 = manifiesto cf71191e", "RE-RUN", "igual"), R("verify_clone: blob Git y copia del run", "RE-RUN", VC),
                  R("launch 7-RunFiles", "RE-RUN", DRY)]),
    "c3_analysis.py": ("analizador C3 (solo lo usa la autoprueba)", "ninguna del transporte",
                       [R("SHA-256 = manifiesto cf71191e", "RE-RUN", "igual"), R("casos G00-G58", "RE-RUN", ST + ": 61/61")]),
    "claude-cli-characterization.json": ("NUEVO: copia del registro de caracterización de 2.1.299 (decisiones §87), fuera del clon",
                                         "blob 19b8a27e en 1c960c0a:docs/automation/evidence/I-62-claude-cli/R20261016T185242Z-char/; SHA-256 fijado en verify_clone.py y en transport-gate.json",
                                         [R("SHA-256 y blob = los del commit (worktree, solo lectura)", "RE-RUN", VC),
                                          R("negativo: copia alterada", "RE-RUN", NEG),
                                          R("transport_gate.py check", "RE-RUN", "Status MEASURED, Problems [] (0,17 s)"),
                                          R("launch 10-TransportGate", "RE-RUN", DRY)]),
    "clone-verification.json": ("salida de verify_clone.py", "verify_clone.py, closure.json, transport-gate.json, el registro, el clon, el run y el worktree",
                                [R("regenerado", "RE-RUN", VC)]),
    "closure.json": ("cierre; cambia SOLO Transport (Binary, AddDirStatus, ExpectedInit.claude_code_version, Characterization) y DeclaredRuntimeContext[0] (versión)",
                     "make_closure.py, clon 088b3227, kit-params.json",
                     [R("identity_checks de make_closure", "RE-RUN", "código 0 (62,5 s); resumen idéntico al de la congelación (337 archivos, 6 973 215 bytes, 129 476 líneas)"),
                      R("diff estructural frente a cf71191e", "RE-RUN", "claves de primer nivel distintas: Transport y DeclaredRuntimeContext; CanonicalInputs idénticos (decisiones 4e090ed4 con 45 rangos y evidencia fafd4392 con 60, sin cambio); 7 líneas (prep/diffs/s87/closure.json.diff)"),
                      R("launch 2-ClosureTransport", "RE-RUN", DRY), R("casos de auditoría del transporte", "RE-RUN", ST + ": 195/195")]),
    "finish_kit.py": ("procedimiento de la congelación original (no se ejecuta en la recuperación)", "ninguna en el lanzamiento",
                      [R("SHA-256 = manifiesto cf71191e", "RE-RUN", "igual (su paso 5 sigue describiendo el registro 20d0de2c del clon: texto histórico, no un control)")]),
    "kit-params.json": ("parámetros del clon", "commit del clon 088b3227 (sin cambio)",
                        [R("SHA-256 = manifiesto cf71191e", "RE-RUN", "igual"), R("verify_clone y launch 6-Clone", "RE-RUN", VC)]),
    "launch.py": ("lanzador; cambian SOLO las constantes del binario (BIN_DISPLAY, BIN, BIN_SHA, VERSION) y dos menciones de su docstring",
                  "closure.Transport.Binary; binario 2.1.299",
                  [R("X01-X04 (coherencia lanzador-auditor)", "RE-RUN", ST + ": 4/4"), R("sonda de registro", "RE-RUN", RP),
                   R("launch 2-ClosureTransport y 3-Binary (SHA-256 295b49ec, Authenticode Valid, Anthropic)", "RE-RUN", DRY),
                   R("mutación", "REUSED", "sin reglas de auditoría en launch.py; la lógica no cambia (prep/diffs/s87/launch.diff: 6 líneas de constantes/docstring)")]),
    "make_closure.py": ("generador del cierre; cambia SOLO el bloque de transporte y la versión de DeclaredRuntimeContext", "clon, kit-params.json",
                        [R("identity_checks y cierre", "RE-RUN", "código 0, 62,5 s"),
                         R("renderizado del prompt", "RE-RUN", "--render-prompt (62,1 s): prompt.md byte a byte igual (a8dc5773)")]),
    "make_manifest.py": ("congelación", "archivos de kit/ y del run", [R("SHA-256 = manifiesto cf71191e", "RE-RUN", "igual"), R("congelación", "RE-RUN", "kit-manifest.json 70d68119 (0,2 s)")]),
    "mutation-post-review.py": ("arnés de mutación", "selftest-post-review.py", [R("SHA-256 = manifiesto cf71191e", "RE-RUN", "igual")]),
    "mutation-result.json": ("resultado histórico de mutación", "post-review.py, transport_gate.py, owner_gate.py (sitios de regla)",
                             [R("mutación de post-review.py", "REUSED", MU_OLD + "; 4f46caa6 → 6c51758a difiere en UNA línea de docstring (prep/diffs/s87/post-review.diff): sitios de regla idénticos. Además " + MU_PART),
                              R("mutación de transport_gate.py", "REUSED", "transport_gate.py idéntico (e326b5b9): 40/40 de " + MU_OLD),
                              R("mutación de owner_gate.py", "REUSED", "d442608a → d90aa1b0 difiere SOLO en las tres constantes AUTH_* (prep/diffs/s87/owner_gate.vs-mutated-d442608a.diff); los sitios probs.append y la lógica son idénticos y los casos que matan cada sitio fijan su autorización explícitamente: 18/18 de " + MU_OLD)]),
    "order.txt": ("orden vigente (§85)", "decisiones §85 del clon", [R("SHA-256 = manifiesto cf71191e", "RE-RUN", "igual"), R("verify_clone MatchesDecisions", "RE-RUN", VC)]),
    "owner_gate.py": ("compuerta 11; cambian SOLO AUTH_SECTION, AUTH_HEADING y AUTH_TEXT_SHA (§86 → §87)", "decisiones de la HEAD del worktree; closure.ObjectBlob",
                      [R("pin_authorization.py n = 87", "RE-RUN", "Ready, línea 1717, blob como token, sin secciones posteriores en 1c960c0a (prep/s87/pin87.json)"),
                       R("casos O00-O30 y L00", "RE-RUN", ST + ": 37/37; L00 PINNED Ok en 1c960c0a"),
                       R("launch 11 en la HEAD e83676bb", "RE-RUN", DRY + "; la sección §87 fijada coincide (encabezado, blob, SHA-256 del texto)"),
                       R("mutación", "REUSED", "ver mutation-result.json (diff solo de las tres constantes)")]),
    "pin_authorization.py": ("cálculo de la fijación", "ninguna", [R("SHA-256 = manifiesto cf71191e", "RE-RUN", "igual"), R("O30", "RE-RUN", ST)]),
    "post-review.py": ("auditor v5.4-a6r4; cambia SOLO una línea de docstring (versión)", "closure.json, owner_gate.py, transport_gate.py, result.schema.json",
                       [R("casos de auditoría", "RE-RUN", ST + ": 195/195"), R("mutación", "REUSED", "ver mutation-result.json; y " + MU_PART)]),
    "prepare_clone.py": ("preparación del clon (no se ejecuta: el clon sigue en 088b3227)", "ninguna", [R("SHA-256 = manifiesto cf71191e", "RE-RUN", "igual")]),
    "prompt.md": ("prompt de la revisión", "plantilla, cierre (sin identificadores del transporte)",
                  [R("SHA-256 = manifiesto cf71191e", "RE-RUN", "igual"), R("re-renderizado", "RE-RUN", "byte a byte igual"), R("copia del run idéntica", "RE-RUN", VC)]),
    "prompt.template.md": ("plantilla", "ninguna", [R("SHA-256 = manifiesto cf71191e", "RE-RUN", "igual")]),
    "read_fidelity.py": ("fidelidad de lectura (auditor)", "ninguna del transporte", [R("SHA-256 = manifiesto cf71191e", "RE-RUN", "igual"), R("casos de auditoría", "RE-RUN", ST)]),
    "result.schema.json": ("contrato de salida", "ninguna del transporte", [R("SHA-256 = manifiesto cf71191e", "RE-RUN", "igual"), R("casos de esquema", "RE-RUN", ST)]),
    "selftest-post-review.py": ("autoprueba; cambian SOLO las versiones de los fixtures (2.1.293 → 2.1.299)", "closure.Transport (ExpectedInit, versión), owner_gate.py",
                                [R("autoprueba", "RE-RUN", ST)]),
    "selftest-result.json": ("salida de la autoprueba", "todos los módulos (Modules en el resultado = los del kit)", [R("regenerado", "RE-RUN", ST)]),
    "transport-gate.json": ("compuerta D-01 (init + set MEASURED sobre la copia del registro)", "closure.Transport, el registro",
                            [R("transport_gate.py check", "RE-RUN", "MEASURED, Problems []"), R("casos G00-G58", "RE-RUN", ST + ": 61/61"),
                             R("launch 10-TransportGate", "RE-RUN", DRY)]),
    "transport_gate.py": ("compuerta D-01 (byte a byte la de la ronda 3)", "ninguna",
                          [R("SHA-256 = manifiesto cf71191e", "RE-RUN", "igual"), R("casos G00-G58", "RE-RUN", ST + ": 61/61"),
                           R("mutación", "REUSED", "40/40 de " + MU_OLD + "; y " + MU_PART)]),
    "verify_clone.py": ("verificación del clon; cambia SOLO la comprobación del registro (copia en kit/ con SHA-256 y blob fijados, en lugar de dentro del clon)",
                        "el registro, transport-gate.json, el worktree (solo lectura)",
                        [R("positiva", "RE-RUN", VC), R("negativa", "RE-RUN", NEG)]),
}
rows = []
for n in sorted(set(OLD["Files"]) | set(NEW["Files"])):
    o, w = OLD["Files"].get(n, {}).get("Sha256"), NEW["Files"].get(n, {}).get("Sha256")
    assert w == sha(os.path.join(KIT, n)), n
    what, deps, ctl = ROWS[n]
    rows.append({"File": "kit/" + n, "OldSha256": o, "NewSha256": w, "Changed": o != w, "Role": what, "DependsOn": deps, "Controls": ctl})
rows.append({"File": "kit/kit-manifest.json", "OldSha256": sha(os.path.join(S87, "pinned-kit", "kit-manifest.json")), "NewSha256": sha(os.path.join(KIT, "kit-manifest.json")),
             "Changed": True, "Role": "congelación", "DependsOn": "todos los archivos de kit/ y del run",
             "Controls": [R("launch 1-KitIntegrity", "RE-RUN", DRY)]})
for n, r in NEW["RunFiles"].items():
    rows.append({"File": "run/" + n, "OldSha256": OLD["RunFiles"][n]["Sha256"], "NewSha256": r["Sha256"], "Changed": OLD["RunFiles"][n]["Sha256"] != r["Sha256"],
                 "Role": "archivo del run", "DependsOn": "copia del kit", "Controls": [R("idéntico a la copia del kit; launch 7-RunFiles", "RE-RUN", DRY)]})
timings = [
    ("Sustituciones exactas y fijación (prep/s87/adapt_transport_s87.py)", "segundos (no cronometrado)"),
    ("make_closure: identity_checks + closure.json", "62,5 s"), ("make_closure --render-prompt (comprobación de identidad del prompt)", "62,1 s"),
    ("transport_gate.py init + set MEASURED; check", "~1 s; 0,17 s"),
    ("verify_clone positiva", "6 min 47 s (19:20:18Z, con carga) y 2 min 42 s (00:02:01Z)"), ("verify_clone negativa", "179,5 s"),
    ("autoprueba COMPLETA (antes de §88; cubre G00-G58, O00-O30, L00, X01-X04 y los casos de auditoría)", "74 min 01 s (19:27:32Z-20:41:33Z)"),
    ("mutación completa, detenida por §88 en 152/191", "3 h 15 min 16 s (20:41:33Z-23:56:49Z); resultado no usado salvo como evidencia adicional"),
    ("make_manifest", "0,2 s"), ("launch.py --record-probe", "1,4 s"), ("launch.py --dry-run", "19,7 s"),
]
doc = {"Schema": "rackcad-incremental-validation-matrix (decisiones §88, punto 3)", "Kit": "R20261014T180556Z-5e02", "Authority": "decisiones §87 (opción A) y §88",
       "Baseline": "kit fijado a §86, kit-manifest.json cf71191e4eda620573f935f7d3a08c69423370dcd27407a937823578ff9ec9b9 (copia verificada: prep/s87/pinned-kit/)",
       "NewManifestSha256": sha(os.path.join(KIT, "kit-manifest.json")),
       "Summary": {"Files": len(rows), "Changed": sum(1 for r in rows if r["Changed"]), "Unchanged": sum(1 for r in rows if not r["Changed"]),
                   "ControlsReRun": sum(1 for r in rows for c in r["Controls"] if c["Status"] == "RE-RUN"),
                   "ControlsReused": sum(1 for r in rows for c in r["Controls"] if c["Status"] == "REUSED")},
       "Timings": [{"Step": a, "WallClock": b} for a, b in timings], "Rows": rows}
with open(os.path.join(S87, "validation-matrix.json"), "w", encoding="utf-8", newline="\n") as f:
    f.write(json.dumps(doc, ensure_ascii=False, indent=1) + "\n")
md = ["# Matriz de validación incremental: recuperación del transporte (decisiones §87, opción A; §88)", "",
      "Base: %s. Manifiesto nuevo: `%s`. %d filas: %d cambiadas, %d sin cambio; controles RE-RUN %d, REUSED %d." %
      (doc["Baseline"], doc["NewManifestSha256"], doc["Summary"]["Files"], doc["Summary"]["Changed"], doc["Summary"]["Unchanged"],
       doc["Summary"]["ControlsReRun"], doc["Summary"]["ControlsReused"]), "",
      "| Archivo | SHA-256 anterior → nuevo | Depende de | Controles |", "|---|---|---|---|"]
for r in rows:
    ch = ("`%s` → `%s`" % ((r["OldSha256"] or "—")[:12], r["NewSha256"][:12])) if r["Changed"] else "`%s` (igual)" % r["NewSha256"][:12]
    md.append("| %s | %s | %s | %s |" % (r["File"], ch, r["DependsOn"], "<br>".join("**%s** %s: %s" % (c["Status"], c["Control"], c["Detail"]) for c in r["Controls"])))
md += ["", "## Tiempos", ""] + ["- %s: %s" % (a, b) for a, b in timings] + [""]
with open(os.path.join(S87, "validation-matrix.md"), "w", encoding="utf-8", newline="\n") as f:
    f.write("\n".join(md))
print(json.dumps(doc["Summary"]), doc["NewManifestSha256"])
