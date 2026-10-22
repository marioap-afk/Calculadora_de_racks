"""Matriz de validación del kit de la RONDA 2 de A-7 (decisiones §88, puntos 3-5; §94, puntos 5 y 6): componente → SHA-256 en el kit lanzado de
la ronda 1 (base-launched/kit/, manifiesto c27db93e…) → en la fase 1 aceptada (phase1-kit/) → ahora → dependencias → por qué cambió → controles
re-ejecutados o reutilizados, con los resultados leídos de los archivos de resultado. Escribe prep/p3/validation-matrix-r2.json y .md.
Uso: python -I -B make_matrix_r2.py <kit-review2>
"""
import hashlib
import json
import os
import sys

ROOT = sys.argv[1]
KIT, BASE, P1 = (os.path.join(ROOT, *p) for p in (("kit",), ("base-launched", "kit"), ("phase1-kit",)))
PH = os.path.join(ROOT, "prep", "p2", "phase")
OUT = os.path.join(ROOT, "prep", "p3")


def sha(p):
    return hashlib.sha256(open(p, "rb").read()).hexdigest() if os.path.isfile(p) else None


def j(p):
    return json.load(open(p, encoding="utf-8")) if os.path.isfile(p) else None


st, sf, vc, mu = (j(os.path.join(KIT, n)) for n in ("selftest-result.json", "selftest-fidelity-result.json", "clone-verification.json",
                                                    "mutation-result.json"))
nv, rp, dr = j(os.path.join(ROOT, "prep", "p3", "neg-verify-result-r2.json")), j(os.path.join(ROOT, "record-probe.json")), j(os.path.join(ROOT, "launch-dry-run.json"))
tot = lambda d: ("%d/%d" % (d["Totals"]["Pass"], d["Totals"]["Cases"]) + ("" if d.get("AllPass") else " (AllPass false)")) if d else "no ejecutado"
mut = lambda m: ("%d/%d" % (mu["Modules"][m]["Killed"], mu["Modules"][m]["Sites"]) if mu and m in mu.get("Modules", {}) else "no ejecutado")
stt = (st or {}).get("Totals", {})
aud = "%s/%s" % (stt.get("AuditPass"), stt.get("AuditCases"))
gat = "%s/%s" % (stt.get("GatePass"), stt.get("GateCases"))
own = "%s/%s" % (stt.get("OwnerGatePass"), stt.get("OwnerGateCases"))
con = "%s/%s" % (stt.get("ConsistencyPass"), stt.get("ConsistencyCases"))
g11 = ((dr or {}).get("Checks") or {}).get("11-OwnerAndAuthorizationGate") or {}
dry = "BlockingChecks %s, Problems %s" % ((dr or {}).get("BlockingChecks"), len(g11.get("Problems") or [])) if dr else "no ejecutada"
probe = "RecordProbe.Ok %s" % ((rp or {}).get("RecordProbe") or {}).get("Ok") if rp else "no ejecutada"
SELF = "selftest-post-review %s (auditoría %s, compuerta D-01 %s, compuerta 11 %s, coherencia %s)" % (tot(st), aud, gat, own, con)
WHY = {
    "post-review.py": ("fidelity.py, owner_gate.py, transport_gate.py, closure.json, result.schema.json",
                       "fase 1 (v5.6-a7): verificación de cada copia fiel contra los bytes del original y crédito por la copia; fase 2 (v6.0-a7r2): "
                       "esquema y coherencia de la ronda 2 (Round1FindingResolution, DeltaPurity, RegressionCheck, OptionalDispositionConformance, "
                       "Materiality, OwnerReservedConsequence, RelatedImpacts.ChangedByRound2, QuestionAnswers, IfAgreed de nueve campos), lectura "
                       "directa AUTOMATION_PLAN L616-L617, registro de la compuerta 11 con §94 como base; sesión principal: docstring de la lectura directa (ninguna línea de código)",
                       "%s; mutación completa %s" % (SELF, mut("post-review.py"))),
    "fidelity.py": ("closure.json", "fase 1: detector y copia fiel RACKCAD-COPIA-FIEL/1; fase 2: clase UNMEASURED_CHAR (decisión (c)) y U+2013 en el "
                    "conjunto medido (34 puntos de código, r1-literals.json)",
                    "selftest-fidelity %s; casos F del selftest; verify_clone; mutación completa %s" % (tot(sf), mut("fidelity.py"))),
    "owner_gate.py": ("pin_authorization.py, decisiones del worktree", "objeto A-7 ronda 2 (8a528133), base §94, autorización PENDIENTE (AUTH_* = None)",
                      "selftest O00-O30 y L00 (%s); mutación completa %s; prueba en seco: %s" % (own, mut("owner_gate.py"), dry)),
    "transport_gate.py": ("claude-cli-characterization.json", "sin cambio", "selftest G00-G58 (%s); mutación completa %s" % (gat, mut("transport_gate.py"))),
    "launch.py": ("fidelity.py, owner_gate.py, transport_gate.py, closure.json, kit-manifest.json",
                  "fase 1: comprobación 7 con las copias fieles; fase 2: clon, run, session id, patrón D--r62-arch-a7r2*, premisa a7-r1.md "
                  "(blob 6c29747d) en la comprobación 7", "X01-X05 (%s); sonda de registro: %s; prueba en seco: %s" % (con, probe, dry)),
    "make_closure.py": ("fidelity.py, prompt.template.md, el clon", "fase 1: detector y copias; fase 2: cierre de la ronda 2 (246 archivos), "
                        "comprobaciones de identidad de la ronda 2, prompt de la ronda 2",
                        "construcción real (closure.json, prompt.md, 43 copias); verify_clone %s" % ((vc or {}).get("AllChecks"))),
    "verify_clone.py": ("fidelity.py, closure.json, kit-params.json, el clon, el worktree (solo lectura)",
                        "fase 1: run = RunFiles con copias verificadas; fase 2: objeto, commits, a7-r1.md y guardas de r2/",
                        "AllChecks %s; negativa de la ronda 2 (un byte de una copia): %s" % ((vc or {}).get("AllChecks"), (nv or {}).get("Pass"))),
    "prepare_clone.py": ("el directorio común de git", "precondiciones de la ronda 2 (catorce)", "ejecutado una vez (kit-params.json, todas en true)"),
    "make_manifest.py": ("fidelity.py, owner_gate.py, closure.json", "run con a7-r1.md y copias; autorización pendiente; mutación completa",
                         "congelación (kit-manifest.json)"),
    "finish_kit.py": ("todos", "pasos de la ronda 2 (sin fijación; mutación completa al final; prueba en seco bloqueada solo por la compuerta 11)",
                      "pasos ejecutados uno a uno con sus mismas órdenes"),
    "mutation-post-review.py": ("selftest-post-review.py, selftest-fidelity.py", "batería completa de cuatro módulos (incluye fidelity.py)",
                                "es el control: %s" % ("AllKilled %s, %d/%d" % (mu["AllKilled"], mu["Totals"]["Killed"], mu["Totals"]["Mutants"]) if mu else "no ejecutado")),
    "selftest-post-review.py": ("post-review.py y sus dependencias", "casos F00-F20, X05 (fase 1); casos de la ronda 2 (fase 2); sesión principal: expectativa del caso 48", "es el control: %s" % SELF),
    "selftest-fidelity.py": ("fidelity.py", "negativos del componente de fidelidad (fase 1) y de UNMEASURED_CHAR (fase 2)", "es el control: %s" % tot(sf)),
    "read_fidelity.py": ("fidelity.py", "fase 1: marcas del detector por llamada; surrogateescape (sin cambio en la fase 2)", "reutilizado de la fase 1"),
    "prompt.template.md": ("closure.json", "fase 1: copias fieles; fase 2: alcance R1-R3 de la ronda 2", "renderizado (prompt.md)"),
    "result.schema.json": ("prep/p2/gen_schema_r2.py", "esquema rackcad-a7r2-architect-review/0", "casos 56-121j del selftest; mutación completa"),
}
rows = []
for n in sorted(set(os.listdir(KIT)) | set(os.listdir(BASE)) | set(os.listdir(P1))):
    if n.startswith("fiel__"):
        continue
    sb, s1, sn = sha(os.path.join(BASE, n)), sha(os.path.join(P1, n)), sha(os.path.join(KIT, n))
    if not (sb or s1 or sn):
        continue
    status = "IDENTICAL" if sb == sn else ("NEW" if sb is None else ("REMOVED" if sn is None else "CHANGED"))
    deps, why, controls = WHY.get(n, ("-", "sin cambio" if status == "IDENTICAL" else "artefacto o dato regenerado para la ronda 2",
                                      "reutilizado por identidad" if status == "IDENTICAL" else "generado por su paso"))
    rows.append({"Component": n, "Status": status, "LaunchedSha256": sb, "Phase1Sha256": s1, "NowSha256": sn, "Dependencies": deps, "Why": why,
                 "Controls": controls})
doc = {"Matrix": "kit de la ronda 2 de A-7 frente al kit lanzado de la ronda 1 (manifiesto c27db93e…) y a la fase 1 aceptada", "Rows": rows}
with open(os.path.join(OUT, "validation-matrix-r2.json"), "w", encoding="utf-8", newline="\n") as f:
    f.write(json.dumps(doc, ensure_ascii=False, indent=1) + "\n")
md = ["| Componente | Estado | Lanzado | Fase 1 | Ahora | Dependencias | Por qué | Controles |", "|---|---|---|---|---|---|---|---|"]
for r in rows:
    md.append("| %s | %s | %s | %s | %s | %s | %s | %s |" % (r["Component"], r["Status"], (r["LaunchedSha256"] or "-")[:8], (r["Phase1Sha256"] or "-")[:8],
                                                       (r["NowSha256"] or "-")[:8], r["Dependencies"], r["Why"], r["Controls"]))
with open(os.path.join(OUT, "validation-matrix-r2.md"), "w", encoding="utf-8", newline="\n") as f:
    f.write("\n".join(md) + "\n")
print(json.dumps({"Rows": len(rows), "Changed": sum(1 for r in rows if r["Status"] != "IDENTICAL")}))
