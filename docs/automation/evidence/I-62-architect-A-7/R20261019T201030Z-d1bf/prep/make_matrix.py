"""Matriz de validación de la FASE 1 (decisiones §94, punto 5; validación incremental de §88, puntos 3-4): componente → SHA-256 en el kit lanzado
→ SHA-256 ahora → por qué → controles re-ejecutados o reutilizados, con los resultados leídos de prep/phase/. Escribe prep/validation-matrix.json.
Uso: python -I make_matrix.py <kit-review2>
"""
import hashlib
import json
import os
import sys

ROOT = sys.argv[1]
KIT, BASE, PH = os.path.join(ROOT, "kit"), os.path.join(ROOT, "base-launched", "kit"), os.path.join(ROOT, "prep", "phase")


def sha(p):
    return hashlib.sha256(open(p, "rb").read()).hexdigest() if os.path.isfile(p) else None


def j(name):
    p = os.path.join(PH, name)
    return json.load(open(p, encoding="utf-8")) if os.path.isfile(p) else None


st, sf1, sf2 = j("selftest-p1.json"), j("selftest-fidelity-p1.json"), j("selftest-fidelity-r1env.json")
mu, scan, rf, vc, mm, rp, dr = (j("mutation-targeted-p1.json"), j("r1-scan.json"), j("rf-test/test-read-fidelity.json"), j("verify-clone-p1.json"),
                                j("test-make-manifest.json"), j("record-probe.json"), j("launch-dry-run.json"))
cases = {c["Case"]: c["Result"] for c in (st or {}).get("Cases", [])}
fcases = [c for c in cases if c.startswith("F")]
tot = lambda d: "%d/%d" % (d["Totals"]["Pass"], d["Totals"]["Cases"]) if d else "no ejecutado"
mut = lambda m: ("%d/%d" % (sum(1 for x in mu["Mutants"] if x["Module"] == m and x["Killed"]), sum(1 for x in mu["Mutants"] if x["Module"] == m))
                 if mu else "no ejecutado")
WHY = {
    "fidelity.py": ("NEW", "detector de lo que Read no entrega fielmente (BOM, UTF-8 inválido, NUL, CR suelto, controles, líneas > 1 900 unidades "
                           "UTF-16, U+FEFF interior, separadores Unicode, extensiones especiales), copia fiel RACKCAD-COPIA-FIEL/1 (codificación "
                           "canónica, decodificación, distribución de líneas, verificación) y run_files_of",
                    "selftest-fidelity %s (entorno p1) y %s (entorno r1); escaneo del cierre de r1; mutación dirigida %s" % (tot(sf1), tot(sf2), mut("fidelity.py"))),
    "selftest-fidelity.py": ("NEW", "negativos del componente de fidelidad (FD, FE, FV, FR, FN, FX)", "es el control"),
    "post-review.py": ("CHANGED", "v5.6-a7: verificación de cada copia fiel contra los bytes del original, crédito de las líneas del original por la "
                                  "copia, canon con surrogateescape, FlaggedOriginalReads; la regla DEGRADED intacta",
                       "selftest-post-review completo %s (F00-F20: %d casos); mutación dirigida %s" % (tot(st), len(fcases), mut("post-review.py"))),
    "selftest-post-review.py": ("CHANGED", "casos F00-F20 y X05", "es el control"),
    "launch.py": ("CHANGED", "comprobación 7 con las copias fieles (run_files_check, fidelity.run_files_of)",
                  "X05 %s; sonda de registro %s; prueba en seco: %s" % (cases.get("X05-comprobacion-7-del-lanzador-con-copias-fieles"),
                                                                      (rp or {}).get("RecordProbe", {}).get("Ok"), (dr or {}).get("Refused", "ver archivo"))),
    "make_closure.py": ("CHANGED", "detector sobre cada archivo del cierre, copias fieles (modo --faithful-copies), FaithfulCopies y RunFiles en el "
                                   "cierre, prompt con la lista de copias; un archivo no UTF-8 ya no detiene",
                        "construcción p1 (prep/p1_build.py: 201 archivos, 33 copias, prompt y cierre)"),
    "verify_clone.py": ("CHANGED", "run = RunFiles del cierre; cada copia verificada contra el original en el commit",
                        "prep/p1_verify_clone.py: %s" % ((vc or {}).get("Checks") if vc else "no ejecutado")),
    "make_manifest.py": ("CHANGED", "archivos del run con las copias", "prep/test_make_manifest_runfiles.py: %s" % ((mm or {}).get("AllPass"))),
    "finish_kit.py": ("CHANGED", "pasos 2b (copias) y 8b (selftest de fidelidad)", "sin ejecutar (fase 2: congelación)"),
    "read_fidelity.py": ("CHANGED", "marcas del detector por llamada; surrogateescape", "prep/test_read_fidelity.py: %s; el lanzado falla con UnicodeDecodeError"
                         % ((rf or {}).get("AllPass"))),
    "prompt.template.md": ("CHANGED", "sección de copias fieles; lecturas del Paso 0 por la copia si el archivo está marcado; reglas de referencia",
                           "renderizado en p1"),
    "mutation-targeted.py": ("CHANGED", "sitios dirigidos de la corrección", "es el control"),
    "mutation-post-review.py": ("CHANGED", "la batería completa incluye fidelity.py; memo de verify/layout por contenido", "usado por la mutación dirigida"),
}
rows = []
for n in sorted(set(os.listdir(KIT)) | set(os.listdir(BASE))):
    so, sn = sha(os.path.join(BASE, n)), sha(os.path.join(KIT, n))
    if n in WHY:
        status, why, ctl = WHY[n]
    elif so == sn:
        r1_artifacts = ("closure.json", "prompt.md", "order.txt", "kit-params.json", "clone-verification.json", "selftest-result.json",
                        "mutation-result.json", "mutation-targeted-result.json", "transport-gate.json", "kit-manifest.json")
        status, why, ctl = "IDENTICAL", "sin cambio", ("artefacto o dato de r1: se regenera en la fase 2" if n in r1_artifacts else
                                                       "reutilizado por identidad (selftest G00-G58 y O00-O30 re-ejecutados igualmente)")
    else:
        status, why, ctl = "CHANGED", "?", "?"
    rows.append({"Component": n, "Status": status, "LaunchedSha256": so, "NowSha256": sn, "Why": why, "Controls": ctl})
doc = {"Matrix": "fase 1 de la corrección de fidelidad (decisiones §94, punto 5) frente al kit lanzado (manifiesto c27db93e…)", "Rows": rows,
       "R1Scan": {k: scan[k] for k in ("Files", "Flagged", "ByClass", "AllCopiesRoundTrip", "AllBlobsMatchClosure")} if scan else None}
out = os.path.join(ROOT, "prep", "validation-matrix.json")
open(out, "w", encoding="utf-8", newline="\n").write(json.dumps(doc, ensure_ascii=False, indent=1) + "\n")
for r in rows:
    print("%-30s %-9s %s -> %s | %s" % (r["Component"], r["Status"], (r["LaunchedSha256"] or "-")[:8], (r["NowSha256"] or "-")[:8], r["Controls"][:110]))
