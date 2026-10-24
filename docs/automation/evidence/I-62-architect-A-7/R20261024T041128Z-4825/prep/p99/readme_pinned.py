"""Decisiones §99, puntos 2-4: pasa el README del kit al estado FIJADO en §99 (cabecera, sección «Fijación de §99», lanzamiento y tabla de
archivos regenerada desde el manifiesto fijado). Lee los resultados de prep/p99. Uso: python -I -B readme_pinned.py <kit-review4>
"""
import hashlib
import json
import os
import re
import sys

ROOT = sys.argv[1]
KIT, P = os.path.join(ROOT, "kit"), os.path.join(ROOT, "prep", "p99")
NL = "\n"
j = lambda *p: json.load(open(os.path.join(*p), encoding="utf-8"))
sha = lambda p: hashlib.sha256(open(p, "rb").read()).hexdigest()
num = lambda n: "{:,}".format(n).replace(",", " ")
man = j(KIT, "kit-manifest.json")
man_sha = sha(os.path.join(KIT, "kit-manifest.json"))
pin, tv = j(P, "pin.json"), j(P, "targeted-validation.json")
dz = j(P, "launch-dry-run-pinned.json")
rg, hc = j(P, "faithful-regeneration-pinned.json"), j(P, "host-checks-prelaunch.json")
a, b = tv["SelftestTargeted"], tv["MutationTargeted"]
assert tv["AllPass"] and rg["AllReproduced"] and dz["BlockingChecks"] == [] and man["AuthorizationState"].startswith("FIJADA: decisiones §99")
og_sha = man["Files"]["owner_gate.py"]["Sha256"]
log = open(os.path.join(P, "chain_p99.log"), encoding="utf-8").read()
R = [
    ('''Autorización:   PENDIENTE: owner_gate.py entrega AUTH_SECTION, AUTH_HEADING y AUTH_TEXT_SHA en None; la compuerta 11 bloquea con el único motivo
                «authorization pending» hasta que una disposición del Coordinator POSTERIOR a §98 que nombre esta revisión se fije y el kit se
                vuelva a congelar (abajo, «Lanzamiento»)''',
     '''Autorización:   FIJADA: decisiones §99, «%s», línea %d en la
                HEAD de96a7f5; AUTH_TEXT_SHA %s…; fijada con pin_authorization.py (prep/p99/). La versión PENDIENTE del kit está en la
                custodia de 736a3805 (manifiesto 46b9c465…); abajo, «Fijación de §99»''' % (pin["Values"]["AUTH_HEADING"][7:], pin["Checks"]["SectionLine"],
                                                                                           pin["Values"]["AUTH_TEXT_SHA"][:16])),
    ("                completo, SHA-256 del texto, una sola sección) y ninguna sección posterior. En el kit entregado: PENDIENTE",
     "                completo, SHA-256 del texto, una sola sección) y ninguna sección posterior. Fijada en §99 (prueba en seco: Ok)"),
    ('''Estado:         kit CONGELADO con la autorización PENDIENTE (kit/kit-manifest.json, SHA-256 46b9c465f9f10d30d38fed9477c7c87d9b62f6cf812cbfc74e8773833342a115, 73 archivos más el manifiesto); sonda de registro en Ok;
                prueba en seco bloqueada SOLO por la compuerta 11 con el motivo «authorization pending» (4 y 5 no se ejecutan en seco). NO lanzado''',
     '''Estado:         kit FIJADO en §99 y vuelto a congelar (kit/kit-manifest.json, SHA-256 %s, %d archivos más el manifiesto); sonda de
                registro en Ok; prueba en seco con BlockingChecks = [] (cero bloqueos; 4 y 5 no se ejecutan en seco)''' % (man_sha, len(man["Files"]))),
    ("## Lanzamiento (no ejecutado; exige una disposición nueva)\n\n1. Una disposición del Coordinator POSTERIOR a §98 que nombre esta revisión (RunId R20261024T041128Z-4825, objeto ebb3a87b) en las decisiones de la\n   rama; ninguna sección posterior a ella hasta lanzar y auditar.",
     "## Fijación de §99 (decisiones §99, puntos 2-4)\n\nPEND:PIN\n\n## Lanzamiento\n\n1. Una disposición del Coordinator POSTERIOR a §98 que nombre esta revisión (RunId R20261024T041128Z-4825, objeto ebb3a87b) en las decisiones de la\n   rama; ninguna sección posterior a ella hasta lanzar y auditar. **Hecho: §99.**"),
    ("   completo como token, ninguna sección posterior) y sustituir SOLO las tres líneas AUTH_* de `kit/owner_gate.py`.",
     "   completo como token, ninguna sección posterior) y sustituir SOLO las tres líneas AUTH_* de `kit/owner_gate.py`. **Hecho.**"),
    ("   de `owner_gate.py`, como en las rondas 2 y 3.", "   de `owner_gate.py`, como en las rondas 2 y 3. **Hecho.**"),
]
pin_text = NL.join([
    "- **Registro:** §99 por añadido en `de96a7f5` (línea %d; cuerpo pegado de 2 695 bytes, SHA-256 `0bf8aee4d1457d2877964e849846076b2cd045ef7efd1db86e7d0a20d83acc36`). Antes de"
    % pin["Checks"]["SectionLine"],
    "  fijar, el kit se cotejó con la custodia de `736a3805`: manifiesto `46b9c465…` igual y los 73 archivos con su SHA-256 (`prep/p99/kit-integrity-before-pin.txt`).",
    "- **Fijación:** `pin_authorization.py` sobre el texto de decisiones de `de96a7f5` (blob `f9aeab80`): Ready; una sola sección §99, posterior a §98,",
    "  con el blob completo de A-7 como token y ninguna sección posterior (`prep/p99/pin.json`). En `owner_gate.py` solo cambian las tres líneas AUTH_*",
    "  (`9d4a8f96` → `%s`; `prep/p99/owner_gate.pin.diff`, `prep/p99/apply_pin.py`)." % og_sha[:8],
    "- **Validación incremental (§99, punto 3)** (`prep/p99/targeted-validation.json`): casos de la compuerta 11 %d/%d; L00 sobre el worktree real en"
    % (a["OwnerPass"], a["OwnerCases"]),
    "  estado PINNED (%s); casos de auditoría que leen la autorización fijada (00, 150-151p, 152, 153) %d/%d; mutación dirigida de `owner_gate.py` fijado"
    % (a["L00"]["Result"], a["AuditPass"], len(a["AuditCases"])),
    "  %d/%d, juzgada sobre los casos de la compuerta 11. Sin repetir la autoprueba de 321 casos ni los 209 mutantes. La cabecera del manifiesto marca"
    % (b["Killed"], b["Sites"]),
    "  `owner_gate.py` como cambiado desde la autoprueba y la mutación (`ChangedSinceSelftestOrMutation`): es la fijación, validada aquí.",
    "- **Congelación y preflight (§99, puntos 2 y 4)** (`prep/p99/chain_p99.sh`, registro `prep/p99/chain_p99.log`): manifiesto `%s…` (FIJADA §99);"
    % man_sha[:16],
    "  sonda de registro con código 0 (`record-probe.json`; la del kit pendiente, `record-probe-pending.json`); **prueba en seco con `BlockingChecks` = []**",
    "  (`launch-dry-run.json`; la del kit pendiente, `launch-dry-run-pending.json`): compuerta 11 en Ok con §99 y sin secciones posteriores; binario",
    "  `claude-cli` 2.1.299 con su SHA-256 y Authenticode Valid de Anthropic; integridad del kit; clon limpio en `94a04af7`; run = kit = cierre =",
    "  manifiesto. %d/%d copias fieles reproducidas desde `94a04af7` (`prep/p99/faithful-regeneration-pinned.json`; la primera ejecución de ese paso"
    % (rg["Copies"], rg["CopiesInClosure"]),
    "  falló solo porque la ruta del clon de la orden quedó con un retorno de carro al escribir el script; corregida y repetida, código 0).",
    "- **Host** (`prep/p99/host-checks-prelaunch.json`, %s): checkout principal en `%s`, HEAD `%s`, %d líneas de estado; worktree de I-64 en"
    % (hc["UtcTime"], hc["MainCheckout"]["Branch"], hc["MainCheckout"]["Head"][:8], hc["MainCheckout"]["StatusLines"]),
    "  `%s` con %d líneas y ningún proceso vigilado; corriente conectada (batería al 100 %%, sin descarga); la sesión retiene la suspensión por"
    % (hc["I64"]["Head"], hc["I64"]["StatusLines"]),
    "  inactividad mientras corre el lanzamiento.",
])
rows = ["| Archivo | Bytes | SHA-256 |", "|---|---|---|"]
for n in sorted(man["Files"]):
    if not n.startswith("fiel__"):
        rows.append("| `kit/%s` | %s | `%s` |" % (n, num(man["Files"][n]["Bytes"]), man["Files"][n]["Sha256"]))
rows.append("| `kit/kit-manifest.json` | %s | `%s` |" % (num(os.path.getsize(os.path.join(KIT, "kit-manifest.json"))), man_sha))
for n in ("record-probe.json", "record-probe-pending.json", "launch-dry-run.json", "launch-dry-run-pending.json", "launch-dry-run-before-mutation.json"):
    rows.append("| `%s` | %s | `%s` |" % (n, num(os.path.getsize(os.path.join(ROOT, n))), sha(os.path.join(ROOT, n))))
p = os.path.join(ROOT, "README.md")
t = open(p, encoding="utf-8").read()
for x, y in R:
    assert t.count(x) == 1, x[:70]
    t = t.replace(x, y)
t = t.replace("PEND:PIN", pin_text)
m = re.search(r"\| Archivo \| Bytes \| SHA-256 \|\n\|---\|---\|---\|\n(?:\|.*\|\n)+", t)
assert m and t.count("| Archivo | Bytes | SHA-256 |") == 1
t = t[:m.start()] + NL.join(rows) + NL + t[m.end():]
assert "PEND:" not in t
open(p, "w", encoding="utf-8", newline="\n").write(t)
print("ok", hashlib.sha256(t.encode("utf-8")).hexdigest()[:16], len(t.encode("utf-8")))
