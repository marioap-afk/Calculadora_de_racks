"""Decisiones §95: actualiza el README del kit tras la fijación (sesión principal). Sustituye las líneas «Autorización» y «Estado» de la cabecera (la
anterior, PENDIENTE, sigue en la custodia de b23d71b8) y añade la sección «Fijación de §95 y nueva congelación», con los resultados leídos de los
archivos. No toca el resto. Uso: python -I -B readme_s95.py <kit-review2>
"""
import hashlib
import json
import os
import re
import sys

ROOT = sys.argv[1]
NL = "\n"
j = lambda *p: json.load(open(os.path.join(ROOT, *p), encoding="utf-8"))
sha = lambda *p: hashlib.sha256(open(os.path.join(ROOT, *p), "rb").read()).hexdigest()
man, st, mo, ic, dr, pin = (j("kit", "kit-manifest.json"), j("kit", "selftest-result.json"), j("prep", "p5", "mutation-owner-gate-pinned.json"),
                            j("prep", "p5", "integrity-checks.json"), j("launch-dry-run.json"), j("prep", "p5", "pin95", "pin.json"))
log = {}
for line in open(os.path.join(ROOT, "prep", "p5", "chain_p5.log"), encoding="utf-8"):
    m = re.match(r"(\S+) (START|END) (\S+)", line)
    if m:
        log.setdefault(m.group(3), {})[m.group(2)] = m.group(1)
assert st["AllPass"] and mo["AllKilled"] and ic["AllPass"] and dr["BlockingChecks"] == []
ms = sha("kit", "kit-manifest.json")
T = st["Totals"]
v = pin["Values"]
g = dr["Checks"]["11-OwnerAndAuthorizationGate"]
a = dr["AuthorizationSections"][0]
t = open(os.path.join(ROOT, "README.md"), encoding="utf-8").read()
old_auth = re.search(r"Autorización:   PENDIENTE\..*?Nada se fijó\n", t, re.S).group(0)
new_auth = ("Autorización:   FIJADA: decisiones §95, «" + v["AUTH_HEADING"][7:] + "»,\n"
            "                línea " + str(a["Line"]) + " en la HEAD 6b00b2bf; AUTH_TEXT_SHA " + v["AUTH_TEXT_SHA"][:16] + "…; fijada con pin_authorization.py\n"
            "                (prep/p5/pin95/). La versión PENDIENTE del kit está en la custodia de b23d71b8 (manifiesto 3499596d…)\n")
old_state = re.search(r"Estado:         kit preparado, probado y CONGELADO con la autorización PENDIENTE.*?ni red ni credenciales\n", t, re.S).group(0)
new_state = ("Estado:         kit FIJADO en §95, probado y vuelto a CONGELAR (kit/kit-manifest.json, SHA-256 " + ms + ");\n"
             "                sonda de registro en Ok; prueba en seco con BlockingChecks = [] (4 y 5 no se ejecutan en seco). Lanzamiento: abajo, «Fijación de §95»\n")
for o, n in ((old_auth, new_auth), (old_state, new_state)):
    assert t.count(o) == 1
    t = t.replace(o, n)
neg = ic["C_Rejection"]["Rows"]
cnt = lambda pfx: sum(1 for r in neg if r["Case"].startswith(pfx) and r["Rejected"])
SEC = """
## Fijación de §95 y nueva congelación (sesión principal; decisiones §95)

1. **Integridad y fidelidad antes de usar ninguna copia** (§95, punto 3; `prep/p5/integrity_checks.py` → `prep/p5/integrity-checks.json`, AllPass):
   - custodia en HEAD: los %(ne)d blobs de la carpeta custodiada coinciden con su `MANIFEST.sha256` (la corrección de CRLF de `b23d71b8`); el mismo
     verificador da %(n5d)d discrepancias en `5d5730ba` y detecta un SHA-256 alterado del manifiesto;
   - las 43 copias fieles, reproducidas desde `b432c40f` (`git cat-file blob` del clon y `fidelity.encode`): blob y SHA-256 del original = cierre;
     SHA-256 de la copia = cierre = manifiesto; regenerada = copia del kit = copia del run; `fidelity.verify` sin problemas;
   - rechazo mecánico (`fidelity.verify` del kit y `run_files_check` de `launch.py`): BOM eliminado %(c1)d/%(c1)d (las seis copias con BOM regeneradas sin
     él y el escape `\\xEF\\xBB\\xBF` borrado), normalización no permitida %(c2)d/%(c2)d (LF→CRLF y NFKC/NFD de la copia; original normalizado a CRLF),
     copia alterada %(c3)d/%(c3)d (un byte en tres posiciones), archivo ajeno al cierre %(c4)d/2 (copia de `docs/HANDOFF.md` y un archivo extra en el run;
     el run real se acepta); y los negativos del kit (FV02-FV05, FR01-FR06) en PASS.
2. **Registro y fijación:** §95 registrada solo por añadido en `6b00b2bf` (cuerpo pegado de 3 477 bytes, SHA-256 `2e2e36e7…`; decisiones del clon y de
   `b23d71b8` como prefijos exactos). `python -B kit/pin_authorization.py <texto de decisiones de 6b00b2bf> 95` → Ready (línea %(line)d, una sola
   sección, blob completo como token, sin secciones posteriores; `prep/p5/pin95/pin.json`; la copia temporal de decisiones, SHA-256 `9799381e…`, se
   borró porque contiene la identidad Git del host y se reproduce con `git show 6b00b2bf:docs/automation/decisions/I-62.md`).
   `prep/p5/pin95/apply_pin.py` sustituyó SOLO las líneas 56-58 de `owner_gate.py` (`3d420a9f` → `%(og)s`; `prep/p5/pin95/owner_gate.pin.diff`).
3. **Controles repetidos por la fijación** (`prep/p5/chain_p5.sh`, `chain_p5.log`): autoprueba COMPLETA con la autorización fijada (%(s8a)s →
   %(s8b)s) **%(tp)d/%(tc)d** (auditoría %(ap)d, compuerta de transporte %(gp)d, compuerta 11 %(op)d con la sonda viva L00 en estado PINNED y la compuerta
   real en Ok, coherencia %(xp)d); **mutación dirigida de `owner_gate.py` fijado %(mk)d/%(msites)d** con el arnés congelado (`prep/p5/mutation_owner_gate.py`,
   línea base %(bl)d casos; %(ma)s → %(mb)s). La batería completa 207/207 se ejecutó con `owner_gate.py` pendiente; entre las dos versiones solo cambian
   las tres constantes AUTH_*, ninguna línea de regla, así que sigue vigente para los demás módulos. Por eso el campo `Auditor` del manifiesto nombra
   `owner_gate.py` entre los módulos cambiados desde la batería y `OwnerGateTested.ChangedSinceSelftestOrMutation` es true (la mutación del manifiesto
   es la de `3d420a9f`); la dirigida cubre la versión fijada.
4. **Nueva congelación:** `make_manifest.py` → `kit-manifest.json` SHA-256 `%(ms)s` (AuthorizationState FIJADA §95). Con la orden congelada: sonda de
   registro con código 0 (`record-probe.json`) y prueba en seco (`launch-dry-run.json`) con **`BlockingChecks` = []**: compuerta 11 en Ok con
   AuthorizationSections = [§95, línea %(line)d, encabezado, blob y SHA-256 del texto conformes] y LaterSections = []; comprobación 3 con el binario de
   `claude-cli` 2.1.299, SHA-256 `295b49ec…`, Authenticode Valid y firmante Anthropic; comprobación 6 con el clon limpio en `b432c40f`. La prueba en
   seco y la sonda de la versión pendiente se conservan como `launch-dry-run-pending.json` y `record-probe-pending.json`.
5. **Dictámenes de §95, punto 6, en el esquema congelado** (§95, punto 4, solo permite cambiar las constantes de la autorización; el prompt y el esquema
   no cambian): cierre de R1-R3 → `Round1FindingResolution`; fidelidad de los artefactos con BOM → la resolución de R1 (A7-1, punto 5: fidelidad por
   los bytes) y `DeltaPurity`; integridad del cierre y categorías de entrada → R1 y `RegressionCheck` T05, T06 y T07; compatibilidad con A-5 y A-6 → R3,
   `DeltaPurity` (sin cambios en A-6), `Materiality` y T09; impacto sobre B1 → T04; riesgos residuales y hallazgos nuevos → `KnownLimitations` y los
   hallazgos `A62-A7R2-*`. El informe del resultado dirá cuáles quedan expresos y cuáles no.
6. **Lanzamiento:** tras la custodia de este kit, las comprobaciones finales del host (binario, clon, checkout principal, I-64, alimentación y
   retención de la suspensión con la pantalla encendida) y una sonda y una prueba en seco finales con cero bloqueos: UNA vez `python -B kit/launch.py`
   desde esta carpeta, tope 3 600 s, sin reintento, y el auditor v6.0-a7r2 con la orden de «Lanzamiento».
""" % dict(ne=ic["A_Custody"]["HeadEntries"], n5d=ic["A_Custody"]["Neg5d5730baMismatches"], c1=cnt("C1"), c2=cnt("C2"), c3=cnt("C3"), c4=cnt("C4-copia") + cnt("C4-archivo"),
           line=a["Line"], og=man["Files"]["owner_gate.py"]["Sha256"][:8], s8a=log["s8-selftest-pinned"]["START"], s8b=log["s8-selftest-pinned"]["END"],
           tp=T["Pass"], tc=T["Cases"], ap=T["AuditCases"], gp=T["GateCases"], op=T["OwnerGateCases"], xp=T["ConsistencyCases"], mk=mo["Module"]["Killed"],
           msites=mo["Module"]["Sites"], bl=mo["Baseline"]["Cases"], ma=log["m-owner-gate-pinned"]["START"], mb=log["m-owner-gate-pinned"]["END"], ms=ms)
t = t.rstrip(NL) + NL + SEC
with open(os.path.join(ROOT, "README.md"), "w", encoding="utf-8", newline="\n") as f:
    f.write(t)
print("ok README", hashlib.sha256(t.encode("utf-8")).hexdigest()[:16])
