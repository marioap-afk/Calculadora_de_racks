# Sondas con datos reales de la regla candidata de la ronda 3 (I-62, A-6 ronda 3, r1)

Banco de pruebas de la supervisión para la regla candidata de la ronda 3 sobre dónde se ejecuta el aceptante y cómo
se construye su solicitud. Aquí solo hay pruebas y resultados: este paquete no redacta ni edita el texto de la
enmienda. No se invocó ningún modelo ni CLI de modelo (ni Codex ni claude), no hubo red, y no se leyó `auth.json`,
credenciales ni `config.toml`. Las órdenes se ejecutaron en secuencia, nunca en paralelo, y todo Python se ejecutó
con `-I`.

## 1. Entorno exacto

| Elemento | Valor |
|---|---|
| Sistema | Windows 11 Pro 10.0.26300 (`Windows-11-10.0.26300-SP0`) |
| Git | `git version 2.54.0.windows.1` (`%USERPROFILE%\AppData\Local\Programs\Git\cmd\git.EXE`) |
| Python | 3.13.14 (CPython; alias de `WindowsApps`, `...\PythonSoftwareFoundation.Python.3.13_qbz5n2kfra8p0\python.exe`) |
| PowerShell 7 | 7.6.6 (`C:\Program Files\WindowsApps\Microsoft.PowerShell_7.6.6.0_x64__8wekyb3d8bbwe\pwsh.EXE`, el del PATH) |
| Instrumento fijado | `bmin_facts.py`, blob `9a829b0eeb1210238052b1feac06cfa8f7964aed`, SHA-256 `b03c3a19cef608386afd7c2e0618b884bfd7b7f6913e95d8ed70272d517a0200` |
| Aceptante fijado | `bmin_accept.py`, blob `434ef1fccb0d15762b30244b4a60928120698411`, SHA-256 `173514aa353f6add1b11ea73ef6da68bba752bacdca64baa0551a6a7be6d8bb0` |
| Biblioteca de la ronda 2 | `lib_r1r4.py`, copia byte a byte de `docs/automation/evidence/I-62-A6/r2/r1r4/lib_r1r4.py` (SHA-256 `c3995a1c…`, el mismo que lista el README de la ronda 2) |

Los bytes del instrumento y del aceptante se obtienen solo con
`git --no-replace-objects -C <worktree de I-62> cat-file blob <blob>`; el banco recalcula el SHA-1 de blob y se detiene
si no coincide (`lib_r1r4.ensure_pins`). Las copias quedan en `D:\r62-a6r3\r1\pin\`. El origen del fixture
(`D:/r62-fixture/fixture-origin.git`) solo se usa como fuente de `git clone -q --no-hardlinks -c core.autocrlf=false -b fx/u1`
y de `git ls-remote` para los hechos del relevo. Los clones nuevos son `D:\r62-a6r3\r1\c-t1`, `c-n4` y `c-nc1`; en cada uno
se hace P1 (escritura del objeto suelto, comprobada) y P2 con pwsh antes de la invocación VERIFY. El worktree de I-62 y
`D:\r62-a6r2` solo se leen.

Escenario FX-U1: BASE `1a4fc9c6`, RED `d30fb6a9`, HEADX `ae25b596` (CurrentSha de la entrega), MAIN = EFF `fbe25347`,
TIP `95bdc29d` (HEAD de `fx/u1`). El CurrentSha inexistente de nc1 es `deadbeef…deadbeef` (40 hex); el banco comprueba
que no es un objeto del clon.

## 2. Cómo reproducir

```powershell
python -I run_r3.py all      # o un grupo: t1 | zero | n4 | nc1 | mutants | recon | summary
```

`zero`, `mutants` y `recon` reutilizan el contexto de `t1` (`D:\r62-a6r3\r1\state-t1.json`). Los resultados son JSON
canónico ASCII (claves ordenadas, separadores `,` `:`, LF) saneados: el prefijo de USERPROFILE se escribe
`%USERPROFILE%`, el nombre de la cuenta y su forma corta se escriben `<usuario>`, las direcciones de correo `<email>`.
Los sobres del instrumento no se copian aquí (llevan direcciones en `CoAuthoredBy`); se citan por SHA-256 y quedan en
`D:\r62-a6r3\r1\s-*\env\`. `inputs/` contiene copias saneadas de las salidas sintéticas del Controller, de las
solicitudes clave y del registro sin ejecuciones del instrumento (ZERO), que no lleva sobres.

| Archivo | Contenido |
|---|---|
| `table_r3.py` | evaluador puro de la regla: `construct` (construcción mecánica de la solicitud) y `evaluate` (recalcula la construcción, la compara con la solicitud dada y aplica la tabla de aceptación) |
| `reconstruct_runs.py` | reconstrucción de referencia de `Runs` desde un registro de eventos en formato Codex (`--json`), estricta y a prueba de fallos |
| `lib_r3.py` | forma de ejecución declarada, registro en formato Codex, salidas sintéticas del Controller, relevo, ejecución del aceptante |
| `run_r3.py` | las sondas |
| `p1-zero.json`, `p2-n4.json`, `p3-nc1.json`, `p4-t1.json`, `p5-mutants.json`, `p6-reconstruct.json` | resultados por grupo |
| `summary.json` | esperado frente a observado y un booleano por caso; SHA-256 de cada archivo |

## 3. Qué se ejecutó

**Forma de ejecución.** Cada corrida del instrumento se ejecuta de verdad con pwsh 7.6.6, cwd = T:
`& '<git.exe>' --no-replace-objects cat-file blob 9a829b0e… | & '<python.exe>' -I - <orden> --repo T …`
(P3 con ejecutables explícitos). El banco lanza `pwsh -NoLogo -NoProfile -NonInteractive -Command <inner>` para no cargar
el perfil del usuario (`D:\PowerShell\profile.ps1`); el registro sintético escribe la orden como la registra Codex,
`"<pwsh>" -Command <inner>`, con la misma `<inner>`.

**Registro de sesión.** Cada invocación VERIFY simulada produce un registro JSONL con el formato medido en los registros
custodiados de Codex: `thread.started`, `turn.started`, `item.started`/`item.completed` con `command_execution`
(`id`, `type`, `command`, `aggregated_output`, `exit_code`, `status`, en ese orden), `agent_message` y `turn.completed`
con `usage`. `command` se renderiza con una réplica del renderizado de Codex (trozos entre comillas simples literales,
trozos entre comillas dobles con `\\` y `\"`, caracteres seguros sin comillas); la réplica reproduce byte a byte las 111
órdenes registradas en los 9 registros custodiados (caso REAL-logs). Como `<inner>` lleva `'` y `\`, Codex la escribiría
entre comillas dobles, no simples. `aggregated_output` = stderr + stdout como texto. Además de las corridas del
instrumento, cada sesión registra lecturas Git primarias (`git rev-parse HEAD`, …), que no cuentan como ejecuciones.

**Reconstrucción** (`reconstruct_runs.py`). (1) Completitud: cada línea es un objeto JSON, el archivo termina en LF, sin
líneas vacías, tipos de evento de una lista cerrada, exactamente un `thread.started` (primero), un `turn.started`
(segundo) y un `turn.completed` (último), y cada `item.started` tiene después su `item.completed` con el mismo `id` y la
misma `command`. (2) Ejecuciones del instrumento: `command_execution` completados cuya `command` contiene el blob fijado.
(3) argv: capa externa (renderizado de Codex) con exactamente 3 tokens `"<ruta absoluta>\pwsh.exe" -Command <inner>`;
capa interna (pwsh) con la gramática fija `& <git> --no-replace-objects cat-file blob <blob> | & <intérprete> -I - <orden> <args>`,
tokens separados por un espacio, `<git>`/`<intérprete>` como literales pwsh (comillas simples con `''`, o dobles sin `$`
ni `` ` ``) con ruta absoluta a `git.exe` / `python[3[.N]].exe`, argumentos como palabras `[A-Za-z0-9_./:=+-]` o literales;
cualquier otra cosa (`;`, `>`, `$`, `~`, `(`, no ASCII, salto de línea, segunda tubería) la hace no reconstruible.
(4) Sobre: exactamente una línea candidata en `aggregated_output` (empieza por `{` o menciona el esquema de hechos),
terminada en LF y no en CRLF, ASCII, objeto JSON con `Schema rackcad-bmin-facts/0-experimental` e idéntica a su
serialización canónica; el resto de líneas (ruido de stderr) se admite fuera de ella. (5) `exit_code` entero;
`completed` ⇔ 0, `failed` ⇔ ≠ 0; 0 ⇔ `Error` nulo en el sobre. Una salida ≠ 0 con sobre de error es una corrida con
error y se conserva en `Runs`. Cualquier problema hace el registro entero no reconstruible: no hay solicitud.

**Regla y tabla** (`table_r3.py`), como la formula el encargo. R4: `Verification` se incluye si y solo si (1) la salida
del Controller, la delegación y la entrega existen y se leen y (2) ninguna de Identity, Scope, CleanTree, Trailer y
FreeText está en `not_run`; si no, se omite y se declara `NotApplicable.R4` con la causa. R5/R6/R7: `NotApplicable` si y
solo si su condición de datos. R10: `RelayFacts` siempre. Aceptada si y solo si rc 0, `ACCEPT_FACTS`, R1 R2 R3 R9 R10
MATCH, R8 MANUAL, R4 MATCH con `Verification` incluida / NOT_APPLICABLE solo si la regla la omite, R5-R7 MATCH o
NOT_APPLICABLE solo por su condición, nunca `EXECUTION_VERIFIED` sin R4 MATCH, y la solicitud igual a la construcción
recalculada (y sus `Runs` iguales, en orden, argv y SHA-256 del sobre, a lo reconstruido del registro). Cero ejecuciones
del instrumento: sin solicitud ni aceptante.

**Salidas sintéticas del Controller.** Escritas por el banco y validadas contra `controller-verification.v2.schema.json`.
Cada resultado sale de los sobres reconstruidos o de una lectura Git primaria del registro; las comprobaciones sin
señal en el banco (Routing, Denials; Remote en T1/nc1) van en `not_run` con la disposición de su fila. El banco aplica
su propia lectura mecánica de AP 16.9 (`Coherence169`): `FailureClass` = primera en `fail`/`not_run`, VERIFIED ⇔ 14 en
pass, pares válidos, y máximo estricto de las disposiciones de fila sin contar los `not_run` sin entrada.

**Resultado global** (`summary.json`): 57 casos, 57 se cumplen. Ejecución completa desde un área de trabajo vacía el
2026-10-13 entre 14:51 y 15:01 UTC.

## 4. Sonda 4: regresión T1 (`p4-t1.json`)

Clon nuevo `c-t1`, P1 y P2 correctos, 13 órdenes del instrumento por la forma declarada (todas con salida 0, sin
stderr) y 5 lecturas Git primarias. El registro es completo; 13 ejecuciones del instrumento reconstruidas sin
problemas. Salida del Controller derivada con las reglas de la ronda 2 (mismos resultados: Termination not_run,
Handoff pass, Authority fail, Contract pass, Identity fail, Remote not_run, Scope fail, CleanTree pass, Ci/Tests
not_run, Trailer fail, Routing not_run, FreeText fail, Denials not_run; BLOCKED/STOP, FailureClass Termination;
`Coherence169` sin diferencias). Delegación, entrega, salida y relevo válidos contra sus esquemas.

| Caso | Esperado | Observado | ¿Se cumple? |
|---|---|---|---|
| T1-r3 | la regla incluye `Verification` (ningún `not_run` entre las cinco); ACCEPT_FACTS rc 0; R4 MATCH; ningún NOT_APPLICABLE; R8 MANUAL; tabla aceptada; igual que la ronda 2 | ACCEPT_FACTS rc 0; R1-R7, R9, R10 MATCH; R8 MANUAL; sin NOT_APPLICABLE. Decisión, rc, estados y filas R4 (Check, Controller, AcceptorExpects, Status) idénticos a `r4-t1.json` Test3 de la ronda 2. Cero escrituras en T y ningún proceso vivo tras el aceptante. Tabla aceptada, sin violaciones de construcción | sí |
| T1-r2-static | la solicitud T1 de la ronda 2 (`D:\r62-a6r2\r1r4\r4\w-t1\request.json`) y su salida del aceptante, evaluadas con `table_r3.py` sin reejecutar: construcción recalculada = incluir `Verification` | tabla aceptada, sin violaciones. Las copias custodiadas `inputs-t1/*` de la ronda 2 son idénticas byte a byte a esos archivos. `Runs` no se contrasta con un registro (la ronda 2 usó un JSONL propio) | sí |

## 5. Sonda 1: ZERO (`p1-zero.json`)

| Caso | Esperado | Observado | ¿Se cumple? |
|---|---|---|---|
| ZERO-log | registro de una VERIFY con solo las 5 lecturas Git primarias reales: completo, 0 ejecuciones del instrumento → sin solicitud ni aceptante | completo; 5 `command_execution`; 0 ejecuciones del instrumento | sí |
| ZERO-a | `Runs = []` con la construcción mecánica (`Verification` T1, `Closure`, `RelayFacts`; R5 y R7 NA por su condición de datos; R6 con datos de `AuthorityResolution`): REJECT con R3 y R9 MISMATCH | REJECT_FACTS rc 1. MISMATCH: **R3** («no data for this check and no NotApplicable declaration»: `check_r3` no tiene corridas y R3 no es dispensable) y **R9** (`MISSING_INPUT` «no tool envelope to check against the closure»). R1 R2 R4 R6 R10 MATCH, R5 R7 NOT_APPLICABLE, R8 MANUAL. Tabla: `ZERO_EXECUTIONS`, `RUNS_EMPTY` | sí |
| ZERO-b | `Runs = []`, NA en R4 R5 R6 R7 R10, sin `Closure`, `Verification` ni `RelayFacts`: REJECT | REJECT_FACTS. MISMATCH solo R3 y R9 (los dos «no data … and no NotApplicable declaration»); R4 R5 R6 R7 R10 NOT_APPLICABLE | sí |
| ZERO-c | ZERO-b más NA en R3 y R9 | REJECT_FACTS. R3 y R9 MISMATCH con «this check cannot be declared not applicable» | sí |

Conclusión medida: sin al menos una corrida, R3 y R9 no pueden quedar en MATCH ni declararse no aplicables, con
cualquier combinación de declaraciones. Una VERIFY sin ejecuciones del instrumento no puede producir ACCEPT_FACTS.

## 6. Sonda 2: N4, entrega ausente (`p2-n4.json`)

Clon `c-n4`, entradas de T1 sin `handoff.json` (borrado; SHA-256 previo registrado). 7 corridas reales que no dependen de
la entrega: `contract`, `hashes --rev MAIN --paths docs/AUTOMATION_PLAN.md src/Fixture.Lib/Calculator.cs`, `cleantree`,
`read-text` ×2, `read-json`, `clausemap` (el `Closure` no lista el archivo `H`). Salida del Controller: Termination
pass, Handoff fail, Authority fail, Contract pass, CleanTree pass, Identity/Remote/Scope/Ci/Tests/Trailer/FreeText
`not_run` sin entrada (sin disposición), Routing y Denials `not_run` del banco; `EXECUTION_BLOCKED`/`BLOCKED`,
FailureClass Handoff.

**Nota de coherencia.** `Coherence169` da `DispositionStrictMax = STOP`, porque Authority falla en el fixture (defectos
del mapa, fila STOP) y «STOP > BLOCKED > REWORK». La salida declara BLOCKED como pide el encargo y como dice el modo de
fallo «Entrega ausente → BLOCKED/BLOCKED». El aceptante no lee `Disposition` ni `Classification`, y en N4-c ni siquiera
lee la salida del Controller (`Verification` omitida), así que los resultados no dependen de esa elección.

| Caso | Esperado | Observado | ¿Se cumple? |
|---|---|---|---|
| N4-a | construcción de la ronda 2: `Verification` apunta a la entrega ausente → R4 MISMATCH (MISSING_INPUT) y REJECT | REJECT_FACTS rc 1; solo **R4** MISMATCH, `{"Error":"MISSING_INPUT","Detail":"Verification.Handoff.Path: unreadable"}` (falla la comprobación entera, sin filas); el resto MATCH. Tabla: `Verification` incluida aunque la regla la omite | sí |
| N4-b | `Verification` omitida sin `NotApplicable.R4` → R4 MISMATCH | REJECT_FACTS; solo R4 MISMATCH («no data for this check and no NotApplicable declaration») | sí |
| N4-c | construcción de la ronda 3: `Verification` omitida, `NotApplicable.R4` con la causa → ACCEPT_FACTS, tabla aceptada | ACCEPT_FACTS rc 0; R4 NOT_APPLICABLE («inputs absent or unreadable: Handoff; not_run in the Controller output: Identity,Scope,Trailer,FreeText»); R5 (hashes), R6 (clausemap) y R7 (contract, clausemap) MATCH. Tabla aceptada | sí |
| N4-d | N4-c más una ejecución con error (`identity --current deadbeef…`) en su propia sesión → REJECT, tabla no aceptada | la reconstrucción la conserva como corrida con error (`exit_code` 1, `status` failed, `Error.Code` BAD_REVISION). REJECT_FACTS: **R3** (Error no nulo; la reejecución sale ≠ 0), **R5** (sobre sin hechos) y **R9** (la revisión inexistente queda fuera del cierre) MISMATCH. Tabla no aceptada | sí |

## 7. Sonda 3: nc1, CurrentSha inexistente (`p3-nc1.json`)

Clon `c-nc1`, entrega con `CurrentSha = deadbeef…` (sigue siendo válida contra `worker-handoff`). 10 corridas reales
sobre entradas cuya comprobación primaria no falló: `contract`, `hashes` (TIP), `cleantree`, `read-text` ×2,
`read-json`, `trx`, `handoff`, `freetext`, `clausemap` (sin `identity`, `diff` ni `scope`). Salidas: Termination pass,
Handoff pass, Authority fail, Contract pass, Identity fail (lectura primaria `git cat-file -t` con rc ≠ 0), Ci/Tests
`not_run` sin entrada, Remote/Routing/Denials `not_run` del banco; `EXECUTION_BLOCKED`/`STOP`. FailureClass = Authority
(no Identity): el mapa del fixture tiene defectos y Authority (#3) precede a Identity (#5) en el orden fijo.
`Coherence169` sin diferencias en V1, V2 y V3.

| Caso | Esperado | Observado | ¿Se cumple? |
|---|---|---|---|
| V1-r3 | V1 (Identity fail, Scope fail, CleanTree pass, Trailer fail, FreeText fail): la ronda 3 incluye `Verification` → R4 MATCH, ACCEPT | ACCEPT_FACTS; R4 MATCH. Filas: Identity, Scope y Trailer «not pass» («revision is not an existing commit»), CleanTree pass, FreeText «not pass» («term GATE PASS in Evidence»), Authority «not pass» (14 defectos). Tabla aceptada | sí |
| V2-r3 | V2 (Scope y Trailer `not_run` sin entrada): la ronda 3 omite `Verification`, R4 NA → ACCEPT | ACCEPT_FACTS; R4 NOT_APPLICABLE («not_run in the Controller output: Scope,Trailer»). Tabla aceptada | sí |
| V2-r2 | documentar las filas R4 con la construcción de la ronda 2 | **ACCEPT_FACTS con R4 MATCH**: el aceptante compara `(Controller == "pass") == calculado`, así que un `not_run` cuenta como «not pass» y coincide cuando el aceptante calcula «not pass» (filas Scope y Trailer: Controller `not_run`, AcceptorExpects «not pass», MATCH). Tabla: no aceptada (`Verification` incluida aunque la regla la omite; R4 MATCH donde se exige NOT_APPLICABLE) | sí (documentado) |
| V3-r2 | V3 (CleanTree `not_run`, el aceptante calcula pass) con la ronda 2 → R4 MISMATCH, REJECT | REJECT_FACTS; solo R4 MISMATCH (fila CleanTree: Controller `not_run`, AcceptorExpects pass) | sí |
| V3-r3 | V3 con la ronda 3 → R4 NA, ACCEPT | ACCEPT_FACTS; R4 NOT_APPLICABLE («not_run in the Controller output: CleanTree»). Tabla aceptada. Sesión propia de 9 corridas, sin `cleantree` | sí |
| nc1-err | V1 más una ejecución sobre el CurrentSha inexistente → sobre de error → REJECT | corrida con error conservada (`exit_code` 1). REJECT_FACTS: R3, R5 y R9 MISMATCH; R4 MATCH. Tabla no aceptada | sí |

## 8. Sonda 5: mutantes de la construcción (`p5-mutants.json`)

Todos sobre la solicitud T1 de la ronda 3 (13 corridas reconstruidas).

| Caso | Mutación | Observado | Lo detecta | ¿Se cumple? |
|---|---|---|---|---|
| M1 | `NotApplicable.R4` con `Verification` incluida | REJECT_FACTS; R4 MISMATCH («declared not applicable but data was supplied»). Tabla: `R4_RULE` | aceptante y tabla | sí |
| M2 | `Verification` omitida (con `NotApplicable.R4`) aunque la regla la exige | **ACCEPT_FACTS** (R4 NOT_APPLICABLE). Tabla: `R4_RULE: Verification omitted although the rule requires it` → no aceptada | solo la tabla | sí |
| M3 | `NotApplicable.R5` con identity/diff/scope/hashes en `Runs` | REJECT_FACTS; R5 MISMATCH. Tabla: `R5_DATA` | aceptante y tabla | sí |
| M4 | `NotApplicable.R6` con clausemap en `Runs` | REJECT_FACTS; R6 MISMATCH. Tabla: `R6_DATA` | aceptante y tabla | sí |
| M5 | salida sintética `EXECUTION_VERIFIED` con Scope `not_run` (la regla omite `Verification`, R4 NA) | **ACCEPT_FACTS** (R4 NOT_APPLICABLE). Tabla: «EXECUTION_VERIFIED without R4 MATCH» → no aceptada | solo la tabla | sí |
| M5b | `EXECUTION_VERIFIED` con las 14 en pass; solicitud sin `Verification` y con `NotApplicable.R4` | **ACCEPT_FACTS**. Tabla: `R4_RULE` y «EXECUTION_VERIFIED without R4 MATCH» | solo la tabla | sí |
| M5c | la misma salida (14 en pass) con la construcción de la ronda 3 (`Verification` incluida) | REJECT_FACTS; R4 MISMATCH (Identity, Scope, Trailer, FreeText y Authority dicen pass y el aceptante calcula «not pass») | aceptante y tabla | sí |
| M6 | `Runs` sin una ejecución que el registro recoge (`read-text 73-73`): 12 frente a 13 | **ACCEPT_FACTS**. Tabla: `RUNS_VS_LOG: request has 12 runs, the session log records 13 instrument executions` | solo la tabla (con la reconstrucción de la sonda 6) | sí |

## 9. Sonda 6: reconstrucción desde eventos en formato Codex (`p6-reconstruct.json`)

Registros sintéticos construidos con las salidas reales de las 13 ejecuciones de T1 (y, en EXIT-err, con una ejecución
real más). La línea de ruido es la primera línea de stderr del registro custodiado v5, leída del archivo:
`profile.ps1: Cannot dot-source this command because it was defined in a different language mode. …` terminada en CRLF.

| Caso | Esperado | Observado | ¿Se cumple? |
|---|---|---|---|
| OK | 13 corridas, argv y sobres iguales a T1; mismo resultado del aceptante | reconstruible; argv y SHA-256 de los 13 sobres idénticos a T1; ACCEPT_FACTS con los mismos estados que T1 (= ronda 2); tabla aceptada | sí |
| NOISE | lo mismo con la línea de profile.ps1 delante de cada salida | reconstruible (1 línea de ruido por corrida); sobres idénticos; mismo resultado | sí |
| CRLF | sobre de identity terminado en CRLF → no reconstruible | «candidate line ends in CRLF» | sí |
| TRUNC | salida cortada a la mitad, sin LF → no reconstruible | «candidate line is not LF-terminated (truncated output)» | sí |
| TRUNC-LF | salida cortada a la mitad, con LF → no reconstruible | «candidate line is not JSON (truncated or interleaved)» | sí |
| TWO | dos sobres en una orden → no reconstruible | «2 candidate lines in aggregated_output (exactly 1 required)» | sí |
| F-noI | sin `-I` | «missing -I right after the interpreter» | sí |
| F-noNRO | sin `--no-replace-objects` | «missing --no-replace-objects» | sí |
| F-bare-P3r2 | forma P3 de la ronda 2 (`git … \| python -I - …`, nombres desnudos, sin `&`) | «does not start with the call operator &» | sí |
| F-bare-git | `& git …` | «git given by bare name or unquoted path» | sí |
| F-bare-py | `& python …` | «interpreter given by bare name or unquoted path» | sí |
| F-extra-stmt | `…; Write-Output fin` | «character ';' not allowed after a token …» | sí |
| F-lead-stmt | `Set-Location …; …` | «character ';' not allowed after a token …» | sí |
| F-redir | `… 2>&1` | «character '>' not allowed after a token …» | sí |
| F-redir-file | `… > D:/…/x.json` | «character '>' not allowed …» | sí |
| F-var | `& $g …` | «character '$' not allowed …» | sí |
| F-dq-expand | `& "$env:LOCALAPPDATA\…\git.exe"` | «double-quoted literal with expansion or escape» | sí |
| F-second-pipe | segunda tubería tras la herramienta | «extra pipeline or call operator after the tool command» | sí |
| F-outer-flags | `"<pwsh>" -NoProfile -Command …` | «not the form "<pwsh>" -Command <inner> (4 tokens)» | sí |
| F-cmd-shell | `cmd.exe /c <inner>` | «not the form … (3 tokens)» | sí |
| F-blob-mention | orden no instrumental que nombra el blob (`git --no-replace-objects cat-file -t <blob>`, tipo P2) | cuenta como ejecución del instrumento y hace el registro no reconstruible («does not start with the call operator &») | sí |
| P-dq (control positivo) | intérprete en literal pwsh entre comillas dobles sin expansiones → reconstruible | reconstruible; argv y sobres idénticos a OK | sí |
| INC-noturn | sin `turn.completed` | «not exactly one turn.completed as last event» | sí |
| INC-nocompleted | un `item.started` sin `item.completed` | «item item_6: item.started without item.completed» | sí |
| INC-cutlast | última línea cortada | «log does not end with LF», «line 40 is not a JSON object», sin `turn.completed` | sí |
| EXIT-mismatch | `exit_code` 1 con un sobre sin error | «exit_code 1 inconsistent with envelope Error null» | sí |
| EXIT-null | `exit_code` null | «exit_code not recorded» | sí |
| EXIT-status | `exit_code` 0 con `status` failed | «status 'failed' inconsistent with exit_code 0» | sí |
| EXIT-err | ejecución real con error añadida → corrida con error; el aceptante la rechaza | reconstruible, 1 corrida con error (pwsh salió con 1); REJECT_FACTS con R3, R5 y R9 MISMATCH; tabla no aceptada | sí |
| SWAP | argv de `read-text 1-1` con el sobre de `73-73` y viceversa | reconstruible; REJECT_FACTS con solo **R3** MISMATCH (la reejecución difiere) | sí |
| SWAP-cmd | identity ↔ diff | reconstruible; REJECT_FACTS con R3 y R5 MISMATCH | sí |
| GRAMMAR-units | literal con `''`: reconstruible; comillas tipográficas, doble espacio, salto de línea, ruta relativa, `..`, `~`, otro blob: no | como se esperaba | sí |
| REAL-logs | registros custodiados de OD-2 y OD-2b-PROBE: completos y 0 ejecuciones del instrumento | **9 registros** (el encargo hablaba de 7): los 9 completos, 0 ejecuciones del instrumento en cada uno (8, 6, 19, 6, 11, 4, 42, 9 y 6 `command_execution`). Además, el decodificador externo y el renderizador reproducen byte a byte las 111 órdenes registradas | sí |

## 10. Observaciones medidas que importan para la redacción

1. **Cero ejecuciones.** R3 y R9 no son dispensables y no tienen datos sin corridas; ninguna combinación de
   `NotApplicable`, `Closure` o `Verification` lleva a ACCEPT_FACTS (ZERO-a/b/c). Excluir del aceptante las
   invocaciones sin ejecuciones no esconde ningún ACCEPT posible.
2. **El aceptante trata `not_run` como «no pass».** Con la construcción de la ronda 2, un `not_run` coincide con lo que
   el aceptante calcula cuando este calcula «not pass» (V2-r2: ACCEPT con R4 MATCH) y falla cuando calcula pass (V3-r2:
   REJECT). El resultado de la ronda 2 ante `not_run` depende de los datos; la regla de la ronda 3 lo vuelve uniforme.
3. **Entrega ausente.** Con la construcción de la ronda 2, R4 falla entero por `MISSING_INPUT` (N4-a); omitir
   `Verification` sin declarar R4 también falla (N4-b). Solo la construcción de la ronda 3 llega a ACCEPT (N4-c).
4. **El aceptante no aplica la regla de construcción.** M2, M5, M5b y M6 dan ACCEPT_FACTS en el aceptante; solo
   `table_r3.py` (construcción recalculada, cláusula VERIFIED y recuento frente al registro) los rechaza. Sin el
   evaluador, una omisión indebida de `Verification`, un VERIFIED con R4 NA o una corrida suprimida pasarían.
5. **Corridas con error.** Se conservan en `Runs` y el aceptante las rechaza siempre (R3, y R5/R9 según la orden):
   N4-d, nc1-err, EXIT-err. pwsh devuelve **1** cuando la herramienta sale con 2 (`EXIT_ERR`), así que el `exit_code`
   del registro es el de pwsh y solo sirve como cero/no cero.
6. **SWAP lo detecta R3** (reejecución), incluso con el mismo comando (SWAP); con comandos distintos también R5.
7. **La identificación por el blob fijado es amplia**: una orden no instrumental que lo nombra (F-blob-mention) cuenta
   como ejecución y vuelve no reconstruible el registro entero.
8. **Renderizado de Codex.** La orden registrada es una cadena con comillas de estilo POSIX (simples literales, dobles
   con `\\`/`\"`), no comillas de pwsh: la réplica reproduce las 111 órdenes custodiadas. Con `'` y `\` en `<inner>`, la
   forma declarada aparece entre comillas dobles, no como `-Command '<inner>'`.
9. **Disposición de N4 con el fixture** (sección 6): el máximo estricto da STOP por Authority; la tabla de modos de
   fallo da BLOCKED/BLOCKED para «Entrega ausente».

## 11. Lo que no queda demostrado

- **Salidas del Controller sintéticas.** No hay Controller real: las escribe el banco desde los sobres y lecturas Git
  primarias, con su propia lectura de AP 16.9. ZERO reutiliza la salida de T1, que se derivó de 13 corridas.
  `AuthorityResolution.Mode/RevisionSha` son valores del banco (como en la ronda 2).
- **Registros sintéticos.** El formato (eventos, claves y su orden, renderizado de `command`, línea de profile.ps1) sale
  de los registros custodiados, pero los archivos los escribe el banco. No se sabe si Codex trunca o transforma un
  `aggregated_output` largo (el sobre de `clausemap` mide unos 44 KB; el mayor `aggregated_output` custodiado mide
  11 228 caracteres), en qué orden real mezcla stdout y stderr, ni cómo trata bytes no UTF-8 (en los custodiados
  aparecen U+FFFD).
- **Sin invocación real de Codex.** El pwsh del runtime de Codex (`%USERPROFILE%\.cache\codex-runtimes\…\pwsh.exe`, que
  según el registro v5 corre en un modo de lenguaje restringido) no se usó. No se probó que `&` y la tubería nativa
  conserven los bytes del instrumento en ese pwsh y ese modo.
- **La orden registrada no es exactamente la ejecutada.** El banco lanzó pwsh con `-NoLogo -NoProfile -NonInteractive`
  para no cargar `D:\PowerShell\profile.ps1`; el registro escribe `"<pwsh>" -Command <inner>`, como Codex. `<inner>` es la
  misma.
- **Identificación de ejecuciones.** Solo se cuentan las órdenes que contienen el blob fijado. Una ejecución del
  instrumento que no nombre el blob (desde una ruta, un archivo copiado o una variable) no se contaría ni se
  reconstruiría, y no se probó.
- **Gramática.** Se probaron las formas enumeradas; otras sintaxis de pwsh (`--%`, here-strings, `-EncodedCommand`,
  alias) quedan fuera por construcción, pero no se enumeraron una a una.
- **Ejecutables.** La gramática exige rutas absolutas a `git.exe` y `python[3[.N]].exe`, pero no las compara con los
  ejecutables declarados en la solicitud o el relevo; R2 solo compara la versión y el SHA-256 del intérprete del
  aceptante con los `Runtime` de los sobres.
- **La opción `--allow-absent` de `handoff`** (que daría `Present: false` en N4) no se ejercitó: N4 solo usa órdenes que
  no dependen de la entrega.
- **Hechos del relevo sintéticos** (sin participante real, `Fingerprint.Kind = NONE`; `config.toml` no se lee). La
  comprobación de procesos es la del banco de la ronda 2 (líneas de órdenes con T e hijos del PID del aceptante), no la
  clasificación completa de AP 16.4.
- **Un host, NTFS, una versión de git y de pwsh, un escenario (FX-U1).**

## 12. Archivos (SHA-256)

El SHA-256 de este README se da fuera de él.

| SHA-256 | Archivo |
|---|---|
| `cbbd5e6db64ccad9eb19bf1bae2291c8b5284409949f3e847b1e543b835a2d4b` | `inputs/m2-request.json` |
| `cfe342a08676a75f18a7c0702cc1ae6fe8a6c554296c516a1307a62968944180` | `inputs/m5-controller-output.json` |
| `9938dc6b0128c0e2b5dcdd1ba000f311b1345409d520058f6fa4de0e6b27cf11` | `inputs/n4-controller-output.json` |
| `e1cc4a897ec881ba06e3bf8c2b33473fea9d3e8aaeaa2a185b94ff64637187bc` | `inputs/n4-request-a-r2.json` |
| `898b970db2c9697644c473e355aad12213f54a27a041844e057776f1e3dc65f2` | `inputs/n4-request-c-r3.json` |
| `7fe3c9dd33dac87bc73a2b032f2f1b798184ba6c0957027c770106181e8fd8f4` | `inputs/nc1-V1-controller-output.json` |
| `257a4c0138e775103148b1ef884844fb92e2c323e6f127ac80263996d5851f63` | `inputs/nc1-V2-controller-output.json` |
| `c2491efc9b8ee9fd3b193df8349380e3b422c0589b145ae9764966c74801f4bd` | `inputs/nc1-V2-request-r2.json` |
| `e410db46449c22e8f5454d853be017b0234f1475863a80c5ed7a734859585203` | `inputs/nc1-V2-request-r3.json` |
| `c78c3172a0620eb4d62269b4a36ce2ea1e07d8d2cf8f3576be8b903b5358972c` | `inputs/nc1-V3-controller-output.json` |
| `0f6ebcebf02184a58f76c91fac28cd32caa872eb060c3983a5b4aecc0974b65c` | `inputs/nc1-V3-request-r3.json` |
| `38c3826f3ff4521549d01337c620f0360f08a8119bd2443070f9359cb79cd60f` | `inputs/t1-controller-output.json` |
| `2efd4c13965c3876b777aac34a0c6a3c6b204993856ca18a6be9e6b4982160da` | `inputs/t1-request-r3.json` |
| `918ee53ac07f886642b806f6f65ac96e10f830687137824a8ce380bb36a43107` | `inputs/zero-session-events.jsonl` |
| `c3995a1cd377822593837c6e34eff30425de61da73b1928d5a0460ce93bfa8c5` | `lib_r1r4.py` |
| `357a775123ec2bcb0dc9ae6323806d92e77716aaaa04a5bdc56b4fbe4701481c` | `lib_r3.py` |
| `732ad8509baa73bef4aa02e10311884fefcaf3aee86087770f712de12ded02a9` | `p1-zero.json` |
| `475a487ebac52594848068403200932a7f408f782f6ed0a6b759057bb1c5a35f` | `p2-n4.json` |
| `de1099efe8bbec151645196675f1a9fca7520f8447ef900c517b0c79266dfb13` | `p3-nc1.json` |
| `524838c8502bc31601918210676b177d4d7de47746cad62a79d7fd4690cb9f48` | `p4-t1.json` |
| `f9e731fc75e340e74d9e5a690f0d8ac7d654f0033605dba6c632fd96e6ebd6e5` | `p5-mutants.json` |
| `dbfc615c0e4be08f856870ffca0056504715d263923fae9cc0d2a0d54ea04c34` | `p6-reconstruct.json` |
| `5dc9f78ee40e0f54efc5d14b2ec84dc12dd4a6c1d63b66b0c07a9d2be33338da` | `reconstruct_runs.py` |
| `e37797aaa4a3fb818ea8b9e3c015f5be080157f4da9a07c9f21c7d9b1dbe8888` | `run_r3.py` |
| `074bbb31ce1990f152ac727191c8969ac52652339fa3aed87385de6a1e165db5` | `summary.json` |
| `ec4d9a8ec0f665f74660bf517526b0a1c097efe630feda1f83297c8661851c9b` | `table_r3.py` |

Las copias de `inputs/` son UTF-8 (las salidas del Controller llevan texto con acentos); los archivos de resultados
son ASCII.
