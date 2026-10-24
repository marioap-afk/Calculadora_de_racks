"""Completa el README del kit de la ronda 3 (sesión principal, al terminar la cadena): sustituye los marcadores PEND:STATE, PEND:REGEN, PEND:PREP,
PEND:HOST y PEND:FILES con los resultados leídos de los archivos (autoprueba, fidelidad, verify_clone y su negativa, mutación completa, manifiesto,
sonda, pruebas en seco, reproducción de las copias fieles, host, privacidad y tabla de archivos). Uso: python -I -B finalize_readme_r3.py <kit-review3>
"""
import hashlib
import json
import os
import re
import sys

ROOT = sys.argv[1]
KIT, P = os.path.join(ROOT, "kit"), os.path.join(ROOT, "prep")
NL = "\n"


def j(*p):
    return json.load(open(os.path.join(ROOT, *p), encoding="utf-8"))


def sha(*p):
    return hashlib.sha256(open(os.path.join(ROOT, *p), "rb").read()).hexdigest()


def log(name):
    d = {}
    for line in open(os.path.join(P, name), encoding="utf-8"):
        m = re.match(r"(\S+) (START|END) (\S+)(?: rc=(\d+))?", line)
        if m:
            d.setdefault(m.group(3), {})[m.group(2)] = m.group(1)
            if m.group(4):
                d[m.group(3)]["rc"] = int(m.group(4))
    return d


st, sf, vc, mu = j("kit", "selftest-result.json"), j("kit", "selftest-fidelity-result.json"), j("kit", "clone-verification.json"), j("kit", "mutation-result.json")
nv, man, rg = j("prep", "neg-verify-result-r3.json"), j("kit", "kit-manifest.json"), j("prep", "faithful-regeneration.json")
dr, drb, rp = j("launch-dry-run.json"), j("launch-dry-run-before-full-mutation.json"), j("record-probe.json")
host, priv, kp = j("prep", "host-checks-final.json"), j("prep", "privacy-final.json"), j("kit", "kit-params.json")
ch = log("chain.log")
man_sha = sha("kit", "kit-manifest.json")
T, MT = st["Totals"], mu["Totals"]
gr, gs = vc["GuardsPreflight"]["Run"], vc["GuardsPreflight"]["SelfTest"]
assert st["AllPass"] and sf["AllPass"] and mu["AllKilled"] and nv["Pass"] and vc["AllChecks"] and rg["AllReproduced"] and priv["Clean"]
assert dr["BlockingChecks"] == ["11-OwnerAndAuthorizationGate"] and drb["BlockingChecks"] == ["11-OwnerAndAuthorizationGate"]
mods = ", ".join("`%s` %d/%d" % (n, r["Killed"], r["Sites"]) for n, r in mu["Modules"].items())

state = ("kit CONGELADO con la autorización PENDIENTE (kit/kit-manifest.json, SHA-256 %s, %d archivos más el manifiesto); sonda de registro en Ok;\n"
         "                prueba en seco bloqueada SOLO por la compuerta 11 con el motivo «authorization pending» (4 y 5 no se ejecutan en seco). NO lanzado"
         % (man_sha, len(man["Files"])))
regen = "AllReproduced: %d/%d copias, %s bytes" % (rg["Copies"], rg["CopiesInClosure"], "{:,}".format(sum(r["Bytes"] for r in rg["Rows"])).replace(",", " "))


def span(name):
    x = ch.get(name, {})
    return "%s → %s, código %s" % (x.get("START"), x.get("END"), x.get("rc"))


prep = NL.join([
    "1. **Identificadores** generados una vez (`prep/ids.txt`): RunId, InvocationId, LogicalReviewRequestId y SessionId (uuid4).",
    "2. **Esquema** `prep/gen_schema_r3.py` → `kit/result.schema.json` (SHA-256 `%s`; reproducible)." % sha("kit", "result.schema.json"),
    "3. **Componentes derivados** del kit de la ronda 2 por sustituciones exactas comprobadas (`prep/patch_*.py`, cada uno exige el SHA-256 de partida):",
    "   `owner_gate.py`, `post-review.py` (dos pasos), `launch.py`, `verify_clone.py`, `finish_kit.py`, `make_manifest.py`, `mutation-post-review.py`,",
    "   `selftest-post-review.py` y `make_closure.py` (docstring, identidad y renderizado por script; constantes y rangos con ediciones exactas: el diff",
    "   completo está en `prep/diffs/make_closure.py.diff`); `prepare_clone.py` y `prompt.template.md` reescritos para la ronda 3 (diffs en `prep/diffs/`).",
    "4. **Clon y run** (`kit/prepare_clone.py`, %s): las quince precondiciones en true (`kit/kit-params.json`)." % kp["PreparedAt"],
    "5. **Copias fieles y prompt** (`make_closure.py --faithful-copies` y `--render-prompt`): 42 copias; `prompt.md` de 110 404 bytes y 731 líneas",
    "   (SHA-256 `%s`), que muestra literalmente las 262 rutas del cierre (el renderizado falla si falta alguna)." % sha("kit", "prompt.md"),
    "6. **Cierre** (`make_closure.py`): `kit/closure.json` (SHA-256 `%s`), 262 archivos, comprobaciones de identidad de la ronda 3 en true." % sha("kit", "closure.json"),
    "7. **Compuerta D-01** (`transport_gate.py init` y `set MEASURED`): `kit/transport-gate.json` MEASURED sobre la caracterización `4ba6ba58…`.",
    "8. **verify_clone.py**: AllChecks; guardas de r3/ reproducidas: `run` %s, entero igual al custodiado; autoprueba %s, %d/%d mutantes muertos, Added %d"
    % (gr.get("Result"), gs.get("Result"), gs.get("MutantsKilled"), gs.get("Mutants"), gs.get("CandidateAdded")),
    "   con el conjunto que existía al ejecutarla (más `preflight_text_r2.py`; arriba, «Hallazgo»), entera igual a la custodiada.",
    "9. **Autoprueba** `selftest-post-review.py` (2026-10-23, 19:03Z-20:05Z): **%d/%d** (auditoría %d, compuerta de transporte %d, compuerta 11 %d con la sonda"
    % (T["Pass"], T["Cases"], T["AuditCases"], T["GateCases"], T["OwnerGateCases"]),
    "   viva L00 en estado PENDIENTE, consistencia %d con X06); clon limpio y run igual al kit al terminar. Pasó a la primera." % T["ConsistencyCases"],
    "10. **Cadena** `prep/chain_r3.sh` (registro `prep/chain.log`): autoprueba de fidelidad %d/%d (%s); negativa de verify_clone (%s; la copia alterada"
    % (sf["Totals"]["Pass"], sf["Totals"]["Cases"], span("s8b-selftest-fidelity"), span("s8c-neg-verify-clone")),
    "    no verifica, las otras %d sí, clon limpio: `prep/neg-verify-result-r3.json`); manifiesto, sonda y prueba en seco previa (bloqueada solo por la"
    % nv["OtherCopiesVerified"],
    "    compuerta 11: `launch-dry-run-before-full-mutation.json`); **mutación COMPLETA %d/%d** (%s; %d muertos por excepción; línea base %d casos; %s);"
    % (MT["Killed"], MT["Mutants"], mods, MT["KilledByException"], mu["Baseline"]["Cases"], span("s12-mutation")),
    "    manifiesto final, sonda (`record-probe.json`, RecordProbe y FrozenCommandGuard en Ok) y prueba en seco final (`launch-dry-run.json`): código 2,",
    "    `BlockingChecks` = [11-OwnerAndAuthorizationGate] con el único motivo pendiente; 1-3 y 6-10 en Ok; 4 y 5 no ejecutadas en seco.",
    "11. **Copias fieles reproducidas** desde `b9bfae50` (`prep/regen_faithful.py` → `prep/faithful-regeneration.json`): %s." % regen,
    "12. **Privacidad** (`prep/privacy_scan.py` → `prep/privacy-final.json`): %d archivos, 0 coincidencias fuera de las copias fieles exentas (texto del"
    % priv["FilesScanned"],
    "    repositorio, que no se custodia). Las líneas de órdenes de los procesos no se registran (solo el ejecutable): podrían nombrar archivos de terceros.",
])
hc = host
procs = "; ".join("%s (iniciado %s; sin mención de I-64 ni de un clon r62-arch-a7)" % (p["Name"], p["Started"][:19]) for p in hc["Processes"]) or "ninguno"
hostt = NL.join([
    "- `prep/host_checks_r3.py` (git -C con GIT_OPTIONAL_LOCKS=0; procesos por Win32_Process, solo el ejecutable) a las %s (`prep/host-checks-final.json`):" % hc["UtcTime"],
    "  checkout principal en `%s`, HEAD `%s`, %d líneas de estado, origin/main local `%s`; worktree de I-64 en `%s` con %d líneas de estado;"
    % (hc["MainCheckout"]["Branch"], hc["MainCheckout"]["Head"], hc["MainCheckout"]["StatusLines"], hc["MainCheckout"]["OriginMainLocal"], hc["I64"]["Head"],
       hc["I64"]["StatusLines"]),
    "  worktree de I-62 en `%s` con %d líneas; clon de la ronda 3 en `%s` con %d líneas (con ignorados); clon de la ronda 2 en `%s` con %d líneas;"
    % (hc["W"]["Head"][:8], hc["W"]["StatusLines"], hc["R3Clone"]["Head"][:8], hc["R3Clone"]["StatusLinesWithIgnored"], hc["R2Clone"]["Head"][:8],
       hc["R2Clone"]["StatusLinesWithIgnored"]),
    "  run de la ronda 2 con %d entradas y la última modificación el %s (no se tocó); claude-code instalados: %s; procesos vigilados: %s;"
    % (hc["R2Run"]["Entries"], hc["R2Run"]["NewestMtimeUtc"], ", ".join(hc["ClaudeCodeVersionsInstalled"]), procs),
    "  memoria libre %s GB, CPU %s %%." % (hc["Load"]["FreeRamGB"], hc["Load"]["CpuPercent"]),
    "- La preparación no tocó el worktree de I-64 ni el checkout principal (solo `rev-parse` y `status` con GIT_OPTIONAL_LOCKS=0). AutoCAD estaba abierto",
    "  por el Owner; esta preparación no lo lanzó ni lo usó.",
    "- **Energía:** el equipo estuvo conectado a la corriente; la sesión retuvo la suspensión por inactividad y la pantalla mientras corrían la autoprueba",
    "  y la cadena (peticiones de energía por proceso, `prep/hold_awake.py`, sin cambiar ajustes, liberadas al terminar). Sin suspensiones.",
])
rows = ["| Archivo | Bytes | SHA-256 |", "|---|---|---|"]
for n in sorted(man["Files"]):
    if n.startswith("fiel__"):
        continue
    rows.append("| `kit/%s` | %s | `%s` |" % (n, "{:,}".format(man["Files"][n]["Bytes"]).replace(",", " "), man["Files"][n]["Sha256"]))
rows.append("| `kit/kit-manifest.json` | %s | `%s` |" % ("{:,}".format(os.path.getsize(os.path.join(KIT, "kit-manifest.json"))).replace(",", " "), man_sha))
for n in ("record-probe.json", "launch-dry-run.json", "launch-dry-run-before-full-mutation.json"):
    rows.append("| `%s` | %s | `%s` |" % (n, "{:,}".format(os.path.getsize(os.path.join(ROOT, n))).replace(",", " "), sha(n)))
files = NL.join(["Los SHA-256 de todos los archivos de esta carpeta están en `MANIFEST.sha256` (la regla `-text` de `.gitattributes` conserva los bytes",
                 "exactos). Las 42 copias fieles `kit/fiel__*.txt` se custodian por reproducción (`prep/faithful-regeneration.json`); sus SHA-256 están en",
                 "`kit/kit-manifest.json` y en `FaithfulCopies` del cierre. Componentes del kit (sin las copias fieles):", ""] + rows +
                ["", "`prep/` contiene los scripts de derivación y de preparación, los diffs frente al kit de la ronda 2, las salidas crudas de la cadena y",
                 "los resultados de la negativa, la reproducción, el host y la privacidad. Ninguna transcripción, credencial ni archivo de autenticación."])
p = os.path.join(ROOT, "README.md")
t = open(p, encoding="utf-8").read()
for k, v in (("PEND:STATE", state), ("PEND:REGEN", regen), ("PEND:PREP", prep), ("PEND:HOST", hostt), ("PEND:FILES", files)):
    assert t.count(k) == 1, k
    t = t.replace(k, v)
assert "PEND:" not in t
open(p, "w", encoding="utf-8", newline="\n").write(t)
print("ok", hashlib.sha256(t.encode("utf-8")).hexdigest(), len(t.encode("utf-8")))
