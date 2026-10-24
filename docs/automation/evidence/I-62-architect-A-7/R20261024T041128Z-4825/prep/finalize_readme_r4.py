"""Completa el README del kit de la ronda 4 (sesión principal, al terminar la cadena; derivado de finalize_readme_r3.py): sustituye los marcadores
PEND:* con los resultados leídos de los archivos (autoprueba, fidelidad, verify_clone y su negativa, mutación dirigida, manifiesto, sonda, pruebas
en seco, reproducción de las copias fieles, tabla de diffs, host, privacidad y tabla de archivos). Uso: python -I -B finalize_readme_r4.py <kit-review4>
"""
import collections
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


def num(n):
    return "{:,}".format(n).replace(",", " ")


def log(name):
    d = {}
    for line in open(os.path.join(P, name), encoding="utf-8"):
        m = re.match(r"(\S+) (START|END) (\S+)(?: rc=(\d+))?", line)
        if m:
            d.setdefault(m.group(3), {})[m.group(2)] = m.group(1)
            if m.group(4):
                d[m.group(3)]["rc"] = int(m.group(4))
    return d


st, sf, vc = j("kit", "selftest-result.json"), j("kit", "selftest-fidelity-result.json"), j("kit", "clone-verification.json")
tm, nv, man, rg = j("kit", "targeted-mutation-result.json"), j("prep", "neg-verify-result-r4.json"), j("kit", "kit-manifest.json"), j("prep", "faithful-regeneration.json")
dr, drb, rp = j("launch-dry-run.json"), j("launch-dry-run-before-mutation.json"), j("record-probe.json")
host, priv, kp, closure = j("prep", "host-checks-final.json"), j("prep", "privacy-final.json"), j("kit", "kit-params.json"), j("kit", "closure.json")
dt = j("prep", "diffs", "diff-table.json")
r1 = j("prep", "selftest-result-run1.json")
assert [c["Case"] for c in r1["Cases"] if c["Result"] != "PASS"] == ["F00-base-lecturas-de-originales-marcados-informativas", "O03-seccion-99-cualquiera-posterior"]
ch = log("chain.log")
man_sha = sha("kit", "kit-manifest.json")
T, og = st["Totals"], tm["OwnerGateTargeted"]
gr, gs = vc["GuardsPreflight"]["Run"], vc["GuardsPreflight"]["SelfTest"]
assert st["AllPass"] and sf["AllPass"] and tm["AllPass"] and nv["Pass"] and vc["AllChecks"] and rg["AllReproduced"] and priv["Clean"]
assert dr["BlockingChecks"] == ["11-OwnerAndAuthorizationGate"] and drb["BlockingChecks"] == ["11-OwnerAndAuthorizationGate"]
assert "MÓDULOS CAMBIADOS" not in man["Auditor"] and man["OwnerGateTested"]["ChangedSinceSelftestOrMutation"] is False


def span(name):
    x = ch.get(name, {})
    return "%s → %s, código %s" % (x.get("START"), x.get("END"), x.get("rc"))


b3 = dr["Checks"]["3-Binary"]
assert b3.get("Ok") is True and b3.get("Sha256") == "295b49ec092f31a7de9c6ff286d9d8f120a4f029b9ad9a5ac0b12c35cbf4df2b"
binary = "presente, con ese SHA-256 y firma Authenticode válida de Anthropic en la prueba en seco final, comprobación 3, %s" % ch["s11b-dry-run"]["END"][:10]
state = ("kit CONGELADO con la autorización PENDIENTE (kit/kit-manifest.json, SHA-256 %s, %d archivos más el manifiesto); sonda de registro en Ok;\n"
         "                prueba en seco bloqueada SOLO por la compuerta 11 con el motivo «authorization pending» (4 y 5 no se ejecutan en seco). NO lanzado"
         % (man_sha, len(man["Files"])))
identical = [r["File"] for r in dt if r["Status"] == "IDENTICAL"]
reuse = ("%d componentes byte a byte (%s); diffs del resto en `prep/diffs/`; autoprueba %d/%d y autoprueba de fidelidad %d/%d sobre los módulos "
         "finales; negativa de `verify_clone.py` (cambió); **mutación dirigida** en lugar de la batería completa (abajo): reglas con los mismos sitios "
         "que en la ronda 3 y `owner_gate.py` %d/%d; el auditor no se reconstruyó (v7.0-a7r4 = el código de reglas de v7.0-a7r3 con los datos de la "
         "ronda 4)" % (len(identical), ", ".join("`%s`" % n for n in identical), T["Pass"], T["Cases"], sf["Totals"]["Pass"], sf["Totals"]["Cases"],
                       og["Killed"], og["Sites"]))

# ---- fidelidad
fc = closure["FaithfulCopies"]
flag_n, only_n = collections.Counter(), collections.Counter()
for f in fc:
    for k in f["Flags"]:
        flag_n[k] += 1
        if len(f["Flags"]) == 1:
            only_n[k] += 1
total = sum(f["CopyBytes"] for f in fc)
big = max(fc, key=lambda f: f["CopyBytes"])
obj = next(f for f in fc if f["Original"] == closure["ObjectPath"])
fid = NL.join([
    "%d archivos marcados por `fidelity.py` (sin cambio) se leen SOLO por su copia fiel RACKCAD-COPIA-FIEL/1 en el run: %s; %s bytes en total, la mayor"
    % (len(fc), ", ".join("%d con `%s` (%d solo por ella)" % (flag_n[k], k, only_n[k]) for k, _ in flag_n.most_common()), num(total)),
    "`%s` (%s). **El objeto (A-7 ronda 4) SÍ está marcado** (`LONG_LINE` en L%s; en la ronda 3 no lo estaba): su copia `%s` (%s bytes, %d líneas)"
    % (os.path.basename(big["Original"]), num(big["CopyBytes"]), ", ".join(str(x) for x in obj["Flags"]["LONG_LINE"]), obj["Copy"], num(obj["CopyBytes"]),
       obj["CopyLines"]),
    "es la lectura fiel del objeto (abajo, riesgo 4). La premisa `a7-r3.md` no está marcada. Las copias no se custodian como archivos (sus rutas",
    "pasarían de MAX_PATH en el worktree): se reproducen byte a byte desde `94a04af7` con `kit/fidelity.py` (`prep/regen_faithful_r4.py` →",
    "`prep/faithful-regeneration.json`, AllReproduced: %d/%d copias, %s bytes)." % (rg["Copies"], rg["CopiesInClosure"], num(sum(r["Bytes"] for r in rg["Rows"]))),
])

# ---- tabla de cambios
WHAT = {
    "result.schema.json": "`rackcad-a7r4-architect-review/0` (`prep/gen_schema_r4.py`, reproducible): PriorFindingResolution de A62-A7R3-R1; DeltaPurity con los once cambios de §3.8; RegressionCheck de doce temas; CompatibilityVerdicts A5/A6; Q-A7R4-01..03; Step0 de la ronda 4 (lectura directa de RAE §10 L219-L226); premisa `a7-r3.md`; el mismo patrón de referencias",
    "post-review.py": "v7.0-a7r4 (`prep/patch_auditor_r4.py`): claves del Paso 0 de la ronda 4, mensajes de compatibilidad con §98, punto 4; el código de las reglas es el de v7.0-a7r3",
    "owner_gate.py": "objeto ebb3a87b, base §98, AUTH_* = None (PENDIENTE), RunId; funciones byte a byte (paso A sin ediciones)",
    "make_closure.py": "cierre de la ronda 4 (`prep/patch_make_closure_r4.py`, 68 ediciones): lectura directa RAE 219-226, árboles fijados r3/r4/revisión r3, literales L1-L27 y «contratos», rangos de §3.5-§3.8, §97 y §98",
    "prompt.template.md": "alcance A62-A7R3-R1, Paso 0, «Qué decidir» y «Resultado» de la ronda 4 (`prep/patch_prompt_r4.py`)",
    "selftest-post-review.py": "casos de la ronda 4 (`prep/patch_selftest_r4.py`): lectura directa de RAE en su original (F08), cambios y temas de la ronda 4; los de transporte, lectura, custodia, lanzador, compuertas y fidelidad, sin cambio salvo nombres",
    "make_manifest.py": "constantes de la ronda 4 y la cabecera que declara la mutación dirigida (`targeted-mutation-result.json`)",
    "finish_kit.py": "constantes de la ronda 4; el paso 12 ejecuta la mutación dirigida (`--full-mutation`, la batería completa)",
    "targeted-mutation.py": "la mutación dirigida de §98, punto 6 (arriba)",
    "targeted-mutation-result.json": "el resultado de la mutación dirigida",
    "mutation-post-review.py": "textos de la ronda 4; el arnés (sitios y mutante) no cambia: lo usa `targeted-mutation.py`",
    "verify_clone.py": "constantes de la ronda 4; reproduce las guardas de r4/ (`run` y su autoprueba, sin archivos borrados antes de publicar)",
    "a7-r3.md": "la premisa: la ronda 3 (blob 8bcccb2f) en lugar de la ronda 2",
    "a7-r2.md": "la premisa de la ronda 3, sustituida por `a7-r3.md`",
    "mutation-result.json": "la batería completa no se repite (mutación dirigida, arriba): su resultado de la ronda 3 (209/209) sigue en `000fd6eb`",
    "order.txt": "el cuerpo pegado de §98 (3 740 bytes) en lugar del de §96",
    "closure.json": "el cierre de la ronda 4 (321 archivos)",
    "prompt.md": "renderizado de `prompt.template.md` con el cierre de la ronda 4",
    "kit-params.json": "`prepare_clone.py` de la ronda 4 (clon en `94a04af7`)",
}
rows = ["| Componente | Ronda 3 → ronda 4 | Qué cambia |", "|---|---|---|"]
for r in dt:
    if r["Status"] == "IDENTICAL":
        continue
    st_ = "NUEVO" if r["Status"] == "NEW" else ("RETIRADO" if r["Status"] == "REMOVED" else "`%s` → `%s`" % (r["OldSha256"][:8], r["NewSha256"][:8]))
    if "Added" in r:
        st_ += " (+%d/−%d)" % (r["Added"], r["Removed"])
    w = WHAT.get(r["File"])
    if w is None:
        w = ("constantes de la ronda 4 (clon, run, sesión, premisa a7-r3.md, patrón de proyecto `D--r62-arch-a7r4*`, §98)"
             if r["File"].endswith(".py") else "generado para la ronda 4")
    rows.append("| `%s` | %s | %s |" % (r["File"], st_, w))
rows.insert(2, "| %s | IDÉNTICOS | — |" % ", ".join("`%s`" % n for n in identical))
diffs = NL.join(rows)

mut = NL.join([
    "Resultado (`kit/targeted-mutation-result.json`, %s): **AllPass %s**; identidad de las reglas %s (%s); `owner_gate.py` **%d/%d** muertos, línea"
    % (span("s12-mutation"), tm["AllPass"], tm["RuleIdentityOk"], ", ".join("`%s` %d/%d sitios%s" % (n, v["SitesRound4"], v["SitesRound3"], ", byte a byte" if v["ByteIdentical"] else "")
                                                                              for n, v in tm["RuleIdentity"].items()), og["Killed"], og["Sites"]),
    "base sin fallos.",
])

prep = NL.join([
    "1. **Identificadores** generados una vez (`prep/ids.txt`): RunId, InvocationId, LogicalReviewRequestId y SessionId (uuid4).",
    "2. **Esquema** `prep/gen_schema_r4.py` → `kit/result.schema.json` (SHA-256 `%s`; reproducible)." % sha("kit", "result.schema.json"),
    "3. **Componentes derivados** del kit de la ronda 3 (`base3/`) en dos pasos: `prep/shift_r4.py` (desplazamiento con marcadores, exige el SHA-256 de",
    "   partida de cada componente) y `prep/patch_small_r4.py`, `prep/patch_auditor_r4.py`, `prep/patch_make_closure_r4.py`, `prep/patch_prompt_r4.py` y",
    "   `prep/patch_selftest_r4.py` (ediciones exactas con su número de apariciones); `kit/targeted-mutation.py` nuevo; diffs en `prep/diffs/`.",
    "4. **Clon y run** (`kit/prepare_clone.py`, %s): las quince precondiciones en true (`kit/kit-params.json`)." % kp["PreparedAt"],
    "5. **Copias fieles y prompt** (`make_closure.py --faithful-copies` y `--render-prompt`): %d copias; `prompt.md` de %s bytes y %d líneas"
    % (len(fc), num(os.path.getsize(os.path.join(KIT, "prompt.md"))), open(os.path.join(KIT, "prompt.md"), "rb").read().count(b"\n")),
    "   (SHA-256 `%s`), que muestra literalmente las %d rutas del cierre (el renderizado falla si falta alguna)." % (sha("kit", "prompt.md"), len(closure["CanonicalInputs"])),
    "6. **Cierre** (`make_closure.py`): `kit/closure.json` (SHA-256 `%s`), %d archivos, comprobaciones de identidad de la ronda 4 en true."
    % (sha("kit", "closure.json"), len(closure["CanonicalInputs"])),
    "7. **Compuerta D-01** (`transport_gate.py init` y `set MEASURED`): `kit/transport-gate.json` MEASURED sobre la caracterización `4ba6ba58…`.",
    "8. **verify_clone.py**: AllChecks; guardas de r4/ reproducidas: `run` %s, entero igual al custodiado; autoprueba %s, %d/%d mutantes muertos, Added %d,"
    % (gr.get("Result"), gs.get("Result"), gs.get("MutantsKilled"), gs.get("Mutants"), gs.get("CandidateAdded")),
    "   entera igual a la custodiada (sin archivos borrados antes de publicar).",
    "9. **Autoprueba** `selftest-post-review.py` (PEND:STSPAN): **%d/%d** (auditoría %d, compuerta de transporte %d, compuerta 11 %d con la sonda"
    % (T["Pass"], T["Cases"], T["AuditCases"], T["GateCases"], T["OwnerGateCases"]),
    "   viva L00 en estado PENDIENTE, consistencia %d con X06); clon limpio y run igual al kit al terminar. La primera ejecución (2026-10-24T04:45:10Z →"
    % T["ConsistencyCases"],
    "   05:47:18Z, `prep/selftest-result-run1.json`) dio %d/%d con dos fallos de la derivación, no del auditor: `F00` esperaba leer el original marcado"
    % (r1["Totals"]["Pass"], r1["Totals"]["Cases"]),
    "   de AUTOMATION_PLAN (la lectura directa de la ronda 3) y no el objeto, que en la ronda 4 está marcado; `O03` usaba como «sección posterior» la",
    "   §99, que en la ronda 4 es la autorización sintética. Corregidos en `prep/patch_selftest_r4.py` (F00 con decisiones y A-7; O03 con §101; y los",
    "   nombres de cuatro casos que aún llevaban la sección base de la ronda 3), comprobados aparte con `prep/quick_check_fixes.py` (compuerta 11 39/39,",
    "   fidelidad 16/16) y repetida la autoprueba entera sobre el módulo final.",
    "10. **Cadena** `prep/chain_r4.sh` (registro `prep/chain.log`): autoprueba de fidelidad %d/%d (%s); negativa de verify_clone (%s; la copia alterada"
    % (sf["Totals"]["Pass"], sf["Totals"]["Cases"], span("s8b-selftest-fidelity"), span("s8c-neg-verify-clone")),
    "    no verifica, las otras %d sí, clon limpio: `prep/neg-verify-result-r4.json`); manifiesto, sonda y prueba en seco previa (bloqueada solo por la"
    % nv["OtherCopiesVerified"],
    "    compuerta 11: `launch-dry-run-before-mutation.json`); **mutación dirigida** (arriba; %s); manifiesto final, sonda (`record-probe.json`," % span("s12-mutation"),
    "    RecordProbe y FrozenCommandGuard en Ok) y prueba en seco final (`launch-dry-run.json`): código 2, `BlockingChecks` = [11-OwnerAndAuthorizationGate]",
    "    con el único motivo pendiente; 1-3 y 6-10 en Ok; 4 y 5 no ejecutadas en seco.",
    "11. **Copias fieles reproducidas** desde `94a04af7` (`prep/regen_faithful_r4.py` → `prep/faithful-regeneration.json`, %s): AllReproduced %d/%d."
    % (span("s13-regen"), rg["Copies"], rg["CopiesInClosure"]),
    "12. **Privacidad** (`prep/privacy_scan_r4.py` → `prep/privacy-final.json`): %d archivos, 0 coincidencias fuera de las copias fieles exentas (texto del"
    % priv["FilesScanned"],
    "    repositorio, que no se custodia). Las líneas de órdenes de los procesos no se registran (solo el ejecutable): podrían nombrar archivos de terceros.",
])
hc = host
procs = "; ".join("%s (iniciado %s; sin mención de I-64 ni de un clon r62-arch-a7)" % (p["Name"], p["Started"][:19]) for p in hc["Processes"]
                  if not p.get("MentionsI64") and not p.get("MentionsCloneR62ArchA7")) or "ninguno"
assert not any(p.get("MentionsI64") for p in hc["Processes"])
hostt = NL.join([
    "- `prep/host_checks_r4.py` (git -C con GIT_OPTIONAL_LOCKS=0; procesos por Win32_Process, solo el ejecutable) a las %s (`prep/host-checks-final.json`):" % hc["UtcTime"],
    "  checkout principal en `%s`, HEAD `%s`, %d líneas de estado, origin/main local `%s`; worktree de I-64 en `%s` con %d líneas de estado;"
    % (hc["MainCheckout"]["Branch"], hc["MainCheckout"]["Head"], hc["MainCheckout"]["StatusLines"], hc["MainCheckout"]["OriginMainLocal"], hc["I64"]["Head"],
       hc["I64"]["StatusLines"]),
    "  worktree de I-62 en `%s` con %d líneas; clon de la ronda 4 en `%s` con %d líneas (con ignorados); clon de la ronda 3 en `%s` con %d líneas;"
    % (hc["W"]["Head"][:8], hc["W"]["StatusLines"], hc["R4Clone"]["Head"][:8], hc["R4Clone"]["StatusLinesWithIgnored"], hc["R3Clone"]["Head"][:8],
       hc["R3Clone"]["StatusLinesWithIgnored"]),
    "  run de la ronda 3 con %d entradas y la última modificación el %s (no se tocó); claude-code instalados: %s; procesos vigilados: %s;"
    % (hc["R3Run"]["Entries"], hc["R3Run"]["NewestMtimeUtc"], ", ".join(hc["ClaudeCodeVersionsInstalled"]), procs),
    "  memoria libre %s GB, CPU %s %%." % (hc["Load"]["FreeRamGB"], hc["Load"]["CpuPercent"]),
    "- La preparación no tocó el worktree de I-64 ni el checkout principal (solo `rev-parse` y `status` con GIT_OPTIONAL_LOCKS=0).",
])
rows = ["| Archivo | Bytes | SHA-256 |", "|---|---|---|"]
for n in sorted(man["Files"]):
    if n.startswith("fiel__"):
        continue
    rows.append("| `kit/%s` | %s | `%s` |" % (n, num(man["Files"][n]["Bytes"]), man["Files"][n]["Sha256"]))
rows.append("| `kit/kit-manifest.json` | %s | `%s` |" % (num(os.path.getsize(os.path.join(KIT, "kit-manifest.json"))), man_sha))
for n in ("record-probe.json", "launch-dry-run.json", "launch-dry-run-before-mutation.json"):
    rows.append("| `%s` | %s | `%s` |" % (n, num(os.path.getsize(os.path.join(ROOT, n))), sha(n)))
files = NL.join(["Los SHA-256 de todos los archivos de esta carpeta están en `MANIFEST.sha256` (la regla `-text` de `.gitattributes` conserva los bytes",
                 "exactos). Las %d copias fieles `kit/fiel__*.txt` se custodian por reproducción (`prep/faithful-regeneration.json`); sus SHA-256 están en" % len(fc),
                 "`kit/kit-manifest.json` y en `FaithfulCopies` del cierre. Componentes del kit (sin las copias fieles):", ""] + rows +
                ["", "`prep/` contiene los scripts de derivación y de preparación, la salida del desplazamiento (`prep/shifted/`), los diffs frente al kit de",
                 "la ronda 3, las salidas crudas de la cadena y los resultados de la negativa, la reproducción, el host y la privacidad. `base3/` (el kit de",
                 "la ronda 3 de partida) no se vuelve a custodiar: está en `000fd6eb`. Ninguna transcripción, credencial ni archivo de autenticación."])
schema_chars = len(json.dumps(json.load(open(os.path.join(KIT, "result.schema.json"), encoding="utf-8")), separators=(",", ":"), ensure_ascii=True))
stspan = open(os.path.join(P, "selftest.start"), encoding="utf-8").read().split()
p = os.path.join(ROOT, "README.md")
t = open(p, encoding="utf-8").read()
prep = prep.replace("PEND:STSPAN", "%s → %s" % (stspan[0], stspan[-1]))
for k, v in (("PEND:STATE", state), ("PEND:BIN", binary), ("PEND:REUSE", reuse), ("PEND:FID", fid), ("PEND:DIFFS", diffs), ("PEND:MUT", mut),
             ("PEND:PREP", prep), ("PEND:SCHEMA", num(schema_chars)), ("PEND:HOST", hostt), ("PEND:FILES", files)):
    assert t.count(k) == 1, (k, t.count(k))
    t = t.replace(k, v)
assert "PEND:" not in t, re.findall(r"PEND:\w+", t)
open(p, "w", encoding="utf-8", newline="\n").write(t)
print("ok", hashlib.sha256(t.encode("utf-8")).hexdigest(), len(t.encode("utf-8")))
