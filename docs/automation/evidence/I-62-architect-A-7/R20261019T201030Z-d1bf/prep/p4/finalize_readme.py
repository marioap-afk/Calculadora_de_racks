"""Completa el README del kit de la ronda 2 (sesión principal, al terminar la cadena): sustituye los cuatro marcadores «PEND:…» con los resultados
leídos de los archivos (autoprueba, fidelidad, verify_clone y su negativa, mutación completa, manifiesto, sonda, pruebas en seco, host, privacidad y
tabla de archivos), corrige el párrafo de independencia (la sesión principal terminó el kit), las filas de post-review.py y selftest-post-review.py
y el riesgo 10. Uso: python -I -B finalize_readme.py <kit-review2>
"""
import hashlib
import json
import os
import re
import subprocess
import sys

ROOT = sys.argv[1]
KIT, P4 = os.path.join(ROOT, "kit"), os.path.join(ROOT, "prep", "p4")
NL = "\n"


def j(*p):
    return json.load(open(os.path.join(ROOT, *p), encoding="utf-8"))


def sha(*p):
    return hashlib.sha256(open(os.path.join(ROOT, *p), "rb").read()).hexdigest()


def log(name):
    d = {}
    for line in open(os.path.join(P4, name), encoding="utf-8"):
        m = re.match(r"(\S+) (START|END) (\S+)(?: rc=(\d+))?", line)
        if m:
            d.setdefault(m.group(3), {})[m.group(2)] = m.group(1)
            if m.group(4):
                d[m.group(3)]["rc"] = int(m.group(4))
    return d


st, sf, vc, mu = j("kit", "selftest-result.json"), j("kit", "selftest-fidelity-result.json"), j("kit", "clone-verification.json"), j("kit", "mutation-result.json")
nv, man = j("prep", "p3", "neg-verify-result-r2.json"), j("kit", "kit-manifest.json")
dr, drb, rp = j("launch-dry-run.json"), j("launch-dry-run-before-full-mutation.json"), j("record-probe.json")
host, priv = j("prep", "p4", "host-checks-final.json"), j("prep", "p4", "privacy-final.json")
first = j("prep", "p4", "selftest-result.json")
c1, c2 = log("chain.log"), log("chain2.log")
man_sha = sha("kit", "kit-manifest.json")
assert st["AllPass"] and sf["AllPass"] and vc["AllChecks"] and nv["Pass"] and mu["AllKilled"], "resultado no conforme: no se completa el README"
assert c2["s12-mutation"]["rc"] == 0 and c2["s11b-check"]["rc"] == 0
T, MT = st["Totals"], mu["Totals"]
g11, g11b = dr["Checks"]["11-OwnerAndAuthorizationGate"], drb["Checks"]["11-OwnerAndAuthorizationGate"]
assert dr["BlockingChecks"] == ["11-OwnerAndAuthorizationGate"] == drb["BlockingChecks"]
others = sorted(k for k, v in dr["Checks"].items() if v.get("Ok") is True)
mods = ", ".join("`%s` %d/%d (`%s`)" % (n, r["Killed"], r["Sites"], r["Sha256"][:8]) for n, r in mu["Modules"].items())
f48 = [c for c in first["Cases"] if c["Result"] != "PASS"]
assert [c["Case"] for c in f48] == ["48-transcripcion-sin-asistente"]
t = open(os.path.join(ROOT, "README.md"), encoding="utf-8").read()

ESTADO = ("kit preparado, probado y CONGELADO con la autorización PENDIENTE (kit/kit-manifest.json, SHA-256 %s); sonda de registro en\n"
          "                Ok; prueba en seco bloqueada SOLO por la compuerta 11 con el motivo pendiente (4 y 5 no se ejecutan en seco). NO lanzado:\n"
          "                ningún claude.exe ni CLI de modelos ejecutado; ni red ni credenciales" % man_sha)

PASOS = """## Preparación ejecutada (2026-10-19 a 2026-10-21): resultados

Pasos de `kit/finish_kit.py`, ejecutados uno a uno con sus mismas órdenes; salidas y tiempos en `prep/p2/`, `prep/p3/` y `prep/p4/` (`chain.log`,
`chain2.log`, `wait-ac.log`):

1. `prepare_clone.py b432c40f… e90b444d… b432c40f…` (2026-10-19T20:17:45Z) → `D:\\r62-arch-a7r2` (HEAD separado en b432c40f; sin ramas ni remoto;
   reflog y gc) y `D:\\r62-arch-a7r2-run\\{order.txt,a7-r1.md}`; `kit/kit-params.json` (SHA-256 `%(params)s…`): las catorce precondiciones en true.
2. `make_closure.py --render-prompt` → `kit/prompt.md` (103 431 bytes, SHA-256 `%(prompt)s…`). 2b. `make_closure.py --faithful-copies` → 43 copias
   fieles. 3. copiados al run, idénticos. 4. `make_closure.py` → `kit/closure.json` (SHA-256 `%(closure)s…`): 246 archivos, 43 marcados (abajo, «Fidelidad»).
5. Autorización PENDIENTE: `owner_gate.py` con AUTH_* = None (SHA-256 `%(og)s…`); nada fijado.
6. Compuerta D-01: `transport_gate.py init` + `set MEASURED kit/claude-cli-characterization.json` (SHA-256 `4ba6ba58…`) → `kit/transport-gate.json`
   en MEASURED (SHA-256 `%(tg)s…`).
7. `verify_clone.py` (2026-10-19T21:21:09Z, 3 min 29 s) → **AllChecks = true** (`kit/clone-verification.json`, SHA-256 `%(vc)s…`): HEAD separado,
   limpio, %(wtf)d archivos, sin enlaces, blobs del cierre y de las dos rondas, a7-r1.md = blob 6c29747d, la revisión de la ronda 1 igual a su custodia,
   run = RunFiles con cada copia fiel verificada contra el original; guardas de la ronda 2 reproducidas desde el clon contra el worktree (solo
   lectura): **`run` b9334f22..e90b444d PASS, igual entera a la custodiada**, y **autoprueba 46/46, igual entera a la custodiada**.
   **Negativa de la ronda 2** (`prep/p3/neg_verify_clone_r2.py` → `prep/p3/neg-verify-result-r2.json`, 2026-10-20T16:02:28Z-16:04:33Z): un byte
   alterado en la copia fiel del TRX con BOM → código 1, AllChecks false, esa copia `FaithfulCopyVerified` false («la copia no reproduce los bytes del
   original»), las otras %(nvo)d verificadas, guardas `run` PASS, clon limpio: **PASS**.
8. **Autoprueba COMPLETA** sobre el clon y el run REALES (`kit/selftest-result.json`, SHA-256 `%(st)s…`; %(s8a)s → %(s8b)s): **%(tp)d/%(tc)d PASS**
   — auditoría %(ap)d/%(ac)d, compuerta de transporte %(gp)d/%(gc)d, compuerta 11 %(op)d/%(oc)d (O00-O30 con una §95 sintética y la sonda viva L00 en
   estado PENDIENTE: bloquea con el único motivo pendiente), coherencia lanzador-auditor %(xp)d/%(xc)d —, con el clon limpio y los 46 archivos del run
   idénticos a sus copias al terminar. Historia: la autoprueba del subagente (2026-10-19T21:55:00Z) se cortó sin resultado por una suspensión del host;
   la primera completa de la sesión principal (2026-10-20T14:55:49Z-16:00:19Z, `prep/p4/selftest-result.json`) dio **%(f1p)d/%(f1c)d**: el caso 48
   esperaba el motivo de la lectura directa de la ronda 1 (V14 L2231-L2233) y el auditor da, correctamente, el de la ronda 2 (AP L616-L617). Se corrigió
   esa expectativa (`prep/p4/fix_case48.py`; la misma cadena que esperan los casos 121 y 121b) y, aprovechando, la docstring del auditor que aún
   describía la lectura directa de la ronda 1 (`prep/p4/fix_auditor_doc.py`; ninguna línea de código); diffs en `prep/p4/fixes.diff`. La autoprueba
   se repitió entera (el host se suspendió de 16:14Z a 18:28Z y el resto corrió con batería; la duración lo incluye).
8b. `selftest-fidelity.py` (2026-10-19T22:10:23Z-22:10:51Z; `fidelity.py` y `selftest-fidelity.py` sin cambio desde entonces) → **%(fp)d/%(fc)d PASS**.
9. `make_manifest.py` (congelación previa a la batería) → `kit-manifest.json` con la mutación PENDIENTE.
10. `python -B kit/launch.py --record-probe` (%(s10)s) → código 0. 11. `python -B kit/launch.py --dry-run` (%(s11)s) → código 2: **`BlockingChecks` =
    [11-OwnerAndAuthorizationGate]** con `Problems` = [«authorization pending…»]; las demás comprobaciones ejecutadas en Ok; `NotRunInDryRun` =
    ['4-Version', '5-Auth'] (`launch-dry-run-before-full-mutation.json`).
12. **Batería COMPLETA de mutación** (`kit/mutation-result.json`, SHA-256 `%(mu)s…`): línea base %(bl)d casos (sin L00) y la autoprueba de fidelidad en
    PASS; **%(mk)d de %(mm)d mutantes muertos**, ninguno superviviente (%(mx)d por excepción): %(mods)s. Historia: el primer arranque (20:04:57Z) se detuvo
    a propósito a los 32 s porque el equipo iba con batería; `prep/p4/wait_ac_then_chain2.sh` la relanzó al detectar corriente (2026-10-20T20:17:28Z),
    pero el equipo entró en espera moderna por ahorro de batería a las 20:36Z e hibernó a las 21:11Z; el proceso se reanudó con el equipo el
    2026-10-21 hacia las 14:09Z, ya con corriente, volvió a pausarse con el equipo en espera moderna de 15:20Z a 16:52Z y, ya con batería, de 19:03Z
    del 2026-10-21 a 16:35Z del 2026-10-22, y terminó el %(s12b)s.
    Ningún cálculo depende del reloj; el clon siguió limpio.
    Después, 9-11 otra vez: manifiesto final (%(s9b)s; SHA-256 `%(man)s`), sonda con código 0 (%(s10b)s; `record-probe.json`) y prueba en seco
    (%(s11bb)s; `launch-dry-run.json`) con la misma condición: **`BlockingChecks` = [11-OwnerAndAuthorizationGate]** y el único motivo pendiente;
    en Ok: %(others)s; `AuthorizationSections` = [], `LaterSections` = []; compuerta 11: línea del Owner %(a6l)s, CLAUDE-CLI-I62 %(ccl)s, A-7 en la HEAD
    = 8a528133 (%(objm)s), A-6 = 4bc919dd (%(a6m)s).

## Validación incremental (decisiones §88; §94, punto 5)

Matriz completa en `prep/p3/validation-matrix-r2.md` y `.json` (componente → SHA-256 en el kit lanzado de la ronda 1 → fase 1 → ahora →
dependencias → por qué → controles, con los resultados leídos de sus archivos). Resumen:

- **Idénticos byte a byte al kit lanzado:** `transport_gate.py`, `c3_analysis.py`, `pin_authorization.py` y `claude-cli-characterization.json`; sus
  controles se repiten porque dependen del cierre nuevo (G00-G58, O30, verify_clone) y `transport_gate.py` vuelve a entrar en la batería completa.
- **Cambiados o nuevos:** `fidelity.py`, `read_fidelity.py`, `post-review.py`, `result.schema.json`, `owner_gate.py`, `launch.py`, `make_closure.py`,
  `verify_clone.py`, `prepare_clone.py`, `make_manifest.py`, `finish_kit.py`, `selftest-post-review.py`, `selftest-fidelity.py`,
  `mutation-post-review.py`, `prompt.template.md`, `order.txt`, `a7-r1.md` y los generados. Controles: autoprueba completa (%(tp)d), autoprueba de
  fidelidad (%(fc)d, con los negativos FD/FE/FV/FR/FN/FX), verify_clone positiva y su negativa de la ronda 2, **batería completa** de los cuatro módulos
  (%(mm)d mutantes; §88, punto 4: cambiaron reglas del auditor, el esquema, la compuerta 11 y la semántica de la comprobación 7 del lanzador) y los
  controles obligatorios de lanzamiento (manifiesto, sonda, prueba en seco).
- **Retirados:** `mutation-targeted.py` y sus resultados, y el `mutation-result.json` de la ronda 1 (`prep/p2/retired-r1-artifacts/`).
- **Cambios de la sesión principal después de la fase 2:** la expectativa del caso 48 (`selftest-post-review.py` `d188e9ee` → `%(stmod)s`) y la docstring
  de `post-review.py` (`e5626a7e` → `%(prmod)s`); ambos antes de la autoprueba completa y de la batería, que los ejercen tal como quedan.""" % dict(
    params=sha("kit", "kit-params.json")[:8], prompt=sha("kit", "prompt.md")[:8], closure=sha("kit", "closure.json")[:8], og=sha("kit", "owner_gate.py")[:8],
    tg=sha("kit", "transport-gate.json")[:8], vc=sha("kit", "clone-verification.json")[:8], wtf=vc["WorkTreeFiles"], nvo=nv["OtherCopiesVerified"],
    st=sha("kit", "selftest-result.json")[:8], s8a=c1["s8-selftest"]["START"], s8b=c1["s8-selftest"]["END"], tp=T["Pass"], tc=T["Cases"],
    ap=T["AuditPass"], ac=T["AuditCases"], gp=T["GatePass"], gc=T["GateCases"], op=T["OwnerGatePass"], oc=T["OwnerGateCases"], xp=T["ConsistencyPass"],
    xc=T["ConsistencyCases"], f1p=first["Totals"]["Pass"], f1c=first["Totals"]["Cases"], fp=sf["Totals"]["Pass"], fc=sf["Totals"]["Cases"],
    s10=c1["s10-record-probe"]["START"], s11=c1["s11-dry-run"]["START"], mu=sha("kit", "mutation-result.json")[:8], bl=mu["Baseline"]["Cases"],
    mk=MT["Killed"], mm=MT["Mutants"], mx=MT.get("KilledByException", 0), mods=mods, s12b=c2["s12-mutation"]["END"], s9b=c2["s9b-manifest"]["END"],
    man=man_sha, s10b=c2["s10b-record-probe"]["START"], s11bb=c2["s11b-dry-run"]["START"], others=", ".join(others),
    a6l=g11.get("A6LineNumbers"), ccl=g11.get("ClaudeCliLineNumbers"), objm=g11.get("ObjectBlobAtHeadMatches"), a6m=g11.get("A6AgreedBlobAtHeadMatches"),
    stmod=st["Modules"]["selftest-post-review.py"][:8], prmod=st["Modules"]["post-review.py"][:8])

procs = host["Processes"]
HOST = """## Host: I-64 y checkout principal (solo lectura)

- `prep/p3/host_checks_r2.py` (git -C con GIT_OPTIONAL_LOCKS=0; procesos por Win32_Process) a las %(utc)s (`prep/p4/host-checks-final.json`):
  checkout principal en `%(br)s`, HEAD `%(mh)s`, %(ms)d líneas de estado, origin/main local `%(om)s`; worktree de I-64 en `%(i64)s` con %(i64s)d líneas
  de estado; worktree de I-62 en `%(wh)s` con %(ws)d líneas; clon de la ronda 2 en `%(r2)s` con %(r2s)d líneas (con ignorados); clon de la ronda 1 en
  `%(r1)s` con %(r1s)d líneas; run de la ronda 1 con %(r1e)d entradas y la última modificación el %(r1m)s (no se tocó); %(pr)s; memoria libre %(ram)s GB,
  CPU %(cpu)s %%.
- La preparación no tocó el worktree de I-64 ni el checkout principal (solo `rev-parse` y `status` con GIT_OPTIONAL_LOCKS=0).
- **Energía:** el host se suspendió cinco veces durante la preparación: la noche del 2026-10-19; de 16:14Z a 18:28Z del 2026-10-20; de 20:36Z del
  2026-10-20 a 14:09Z del 2026-10-21 (ahorro de batería, con hibernación); de 15:20Z a 16:52Z del 2026-10-21, ya con corriente (espera moderna: una
  petición de energía por proceso sin ES_DISPLAY_REQUIRED no la impide); y de 19:03Z del 2026-10-21 (cambio a batería) a 16:35Z del 2026-10-22. Desde el 2026-10-20 la sesión retuvo la suspensión por inactividad mientras
  corrían la autoprueba y la mutación, y desde las 18:35Z del 2026-10-21 también la pantalla (peticiones de energía por proceso, sin cambiar ajustes,
  liberadas al terminar la cadena).""" % dict(
    utc=host["UtcTime"], br=host["MainCheckout"]["Branch"], mh=host["MainCheckout"]["Head"], ms=host["MainCheckout"]["StatusLines"],
    om=host["MainCheckout"]["OriginMainLocal"], i64=host["I64"]["Head"], i64s=host["I64"]["StatusLines"], wh=host["W"]["Head"][:8],
    ws=host["W"]["StatusLines"], r2=host["R2Clone"]["Head"][:8], r2s=host["R2Clone"]["StatusLinesWithIgnored"], r1=host["R1Clone"]["Head"][:8],
    r1s=host["R1Clone"]["StatusLinesWithIgnored"], r1e=host["R1Run"]["Entries"], r1m=host["R1Run"]["NewestMtimeUtc"],
    pr=("ningún proceso vigilado (acad.exe, accoreconsole.exe, dotnet.exe, testhost.exe, MSBuild.exe, VBCSCompiler.exe, claude.exe con -p) ni que nombre "
        "la ruta de I-64 o un clon r62-arch-a7*") if not procs else
       ("procesos vigilados: %s, que esta preparación no lanzó; ninguno nombra la ruta de I-64 ni un clon r62-arch-a7* en su línea de órdenes"
        % ", ".join("%s (iniciado %s)" % (p["Name"], p["Started"][:19]) for p in procs)),
    ram=host["Load"]["FreeRamGB"], cpu=host["Load"]["CpuPercent"])

tp = os.path.join(P4, "file-table.txt")
subprocess.run([sys.executable, "-I", "-B", os.path.join(ROOT, "prep", "p3", "file_table.py"), ROOT, tp], capture_output=True, check=True)
table = open(tp, encoding="utf-8").read()
assert table.count(NL) > 50, "tabla de archivos incompleta"
ARCHIVOS = """## Archivos y privacidad

`kit/` es el kit congelado (manifiesto: `kit/kit-manifest.json`). Junto a este README: `record-probe.json`, `launch-dry-run.json` y
`launch-dry-run-before-full-mutation.json` (salidas de la orden congelada, convertidas a UTF-8; bytes crudos en `prep/p4/`). `prep/` es la preparación
y no forma parte del kit: `p2/` (fase 2: fuentes, esquema, notas y salidas), `p3/` (diffs frente al kit lanzado y a la fase 1, matriz, negativa de
verify_clone, comprobaciones del host, privacidad, tabla de archivos), `p4/` (sesión principal: correcciones, cadenas, registros de tiempo y salidas
crudas) y los archivos de la fase 1 (`phase/`, `diffs/`, `p1_*.py`, `test_*.py`, `validation-matrix.json`). Fuera de la custodia: `base-launched/` (el
kit de la ronda 1, ya custodiado en e71400a2), `phase1-kit/` (sus SHA-256 están en `prep/p3/diffs/diff-table.json`), `prep/dev/` (ayudas de
desarrollo), `prep/p3/vcneg/` (temporal de la negativa) y las copias previas a las correcciones (`prep/p4/*.before-*.py`; los diffs bastan).

**Privacidad:** `prep/p3/privacy_scan.py` (patrones leídos del entorno y de la configuración global de git en tiempo de ejecución) sobre `kit/` y
`prep/` (`prep/p4/privacy-final.json`): %(files)d archivos, **%(hits)d coincidencias** fuera de las copias byte a byte del repositorio (`fiel__*.txt`,
`a7-r1.md`: %(ex)d, exentas porque conservan los bytes de archivos ya versionados); la custodia comprueba además este README y las salidas de primer
nivel. La única coincidencia previa, la ruta corta del perfil en `prep/p4/wait_ac_then_chain2.sh` (la del script de retención en el scratchpad), se
redactó antes de la custodia y el script se copió a `prep/p4/hold_awake.py`. Las rutas del perfil se escriben como %%USERPROFILE%%, %%LOCALAPPDATA%%,
%%APPDATA%% o %%TEMP%%.

```text
SHA-256                                                              bytes  archivo
%(table)s```""" % dict(files=priv["FilesScanned"], hits=len(priv["Hits"]), ex=len(priv["ExemptHits"]), table=table)
assert not priv["Hits"], "privacidad: hay coincidencias"

INDEP = """Las fases 1 y 2 las construyó un subagente de la sesión principal, que se detuvo dos veces por el vigilante de flujo; un segundo subagente las
revisó, escribió este README hasta «Lanzamiento» y se detuvo durante la autoprueba. Ninguno evaluó A-7 ni sus preflights: el prompt no contiene veredicto
esperado ni valoraciones; las definiciones de los estados del esquema son de forma. No leyeron transcripciones, credenciales ni archivos de autenticación.
Recibieron automáticamente el índice de memoria de usuario de la sesión (MEMORY.md); no se usó para el contenido del kit, y el revisor no lo recibe
(--safe-mode, otro proyecto de claude-cli, clon y run propios). **La sesión principal terminó el kit** (2026-10-20 y 2026-10-21): es la sesión que
preparó A-7 ronda 2, así que no es independiente del objeto; sus cambios en el kit se limitan a dos correcciones (la expectativa del caso 48 de la
autoprueba y una docstring del auditor, abajo, paso 8) y no tocan el prompt, el esquema, el cierre, las copias fieles ni ninguna regla del auditor; además
ejecutó los pasos 7-12 y completó las secciones «Preparación ejecutada», «Validación incremental», «Host» y «Archivos»."""
m = re.search(r"Este kit lo terminó un subagente.*?clon y run propios\)\.", t, re.S)
assert m
t = t.replace(m.group(0), INDEP)
for a, b in (("| `post-review.py` | CAMBIADO (`303321f2` → `e5626a7e`) |", "| `post-review.py` | CAMBIADO (`303321f2` → `%s`) |" % st["Modules"]["post-review.py"][:8]),
             ("FrozenCommandGuard, RecordProbe y Environment sin cambio |", "FrozenCommandGuard, RecordProbe y Environment sin cambio; sesión principal: docstring de la lectura directa (ninguna línea de código) |"),
             ("| `selftest-post-review.py` | CAMBIADO (`e6deb731` → `d188e9ee`) |", "| `selftest-post-review.py` | CAMBIADO (`e6deb731` → `%s`) |" % st["Modules"]["selftest-post-review.py"][:8]),
             ("O00-O30 con una §95 SINTÉTICA, L00 pendiente) |", "O00-O30 con una §95 SINTÉTICA, L00 pendiente); sesión principal: expectativa del caso 48 |"),
             ("10. **Rendimiento del host:** durante la autoprueba había procesos de otras aplicaciones del host (no de esta preparación) con uso alto de CPU; los\n    tiempos de abajo lo incluyen.",
              "10. **Rendimiento y energía del host:** durante la autoprueba hubo procesos de otras aplicaciones del host con uso alto de CPU, cinco suspensiones\n    del host y tramos con batería (abajo, «Host»); los tiempos lo incluyen. Ninguna suspensión cambió un resultado: los cálculos no dependen del reloj."),
             ("Estado:         «PEND:ESTADO»", "Estado:         " + ESTADO), ("«PEND:PASOS»", PASOS), ("«PEND:HOST»", HOST), ("«PEND:ARCHIVOS»", ARCHIVOS)):
    assert t.count(a) == 1, a[:80]
    t = t.replace(a, b)
assert "«PEND" not in t
with open(os.path.join(ROOT, "README.md"), "w", encoding="utf-8", newline="\n") as f:
    f.write(t.rstrip(NL) + NL)
print("ok README", hashlib.sha256((t.rstrip(NL) + NL).encode("utf-8")).hexdigest(), len(t))
