# Ejecución delegada de agentes — procedimiento

Este documento es subordinado: no crea requisitos de evidencia, estados, gates, reglas Git ni decisiones del Owner. Ante un conflicto manda la autoridad del dominio y el conflicto se
eleva como STOP.

Las reglas viven en [AUTOMATION_PLAN](../../AUTOMATION_PLAN.md) §16 («Ejecución delegada bajo orden del Coordinator»); Git y el relevo entre sesiones, en
[WORKFLOW](../../WORKFLOW.md) §3; la evidencia, en [AGENTS](../../../AGENTS.md). Aquí solo hay **cómo**: órdenes, plantillas de archivos y escenarios para aplicar esas reglas. Origen:
Freeze de I-61 ([Proposal V9](../../initiatives/I-61-proposal-v9.md)).

| Documento | Para qué |
|---|---|
| [routing.md](routing.md) | Elegir clase, effort semántico, nivel y celda |
| [model-catalog.md](model-catalog.md) | Datos fechados de modelos y estado local por celda (no normativo) |
| [prompting-guide.md](prompting-guide.md) | Cómo escribir el delta de un prompt |
| [PROMPT_TEMPLATES §G](../../initiatives/PROMPT_TEMPLATES.md) | Contrato base y perfiles con los que se compone el prompt |
| `schemas/` | Los cinco esquemas `rackcad-*/v1` |

## 1. Participantes

- **Coordinator:** redacta `gate-contract.json`, acepta o rechaza el paquete (A1-A8), redacta `analysis.md` y declara GATE PASS según LIFECYCLE §7.
- **Controller (Codex CLI, solo lectura):** planifica (`CONTROLLER_PLANNING`) y verifica (`CONTROLLER_VERIFICATION`); escribe solo por `-o`.
- **Worker (subagente de la sesión, o proceso):** escribe en su alcance, hace commit y push, escribe su entrega y termina.
- **Sesión responsable:** lanza a los participantes, cede el worktree mientras trabajan, escribe los registros de relevo y los hechos remotos, y custodia.

## 2. Área transitoria

Todo el tráfico de una delegación vive bajo `artifacts/orchestration/`, que `.gitignore` ya ignora. Cada invocación tiene su propio directorio de `RunId`:

```text operativo
artifacts/orchestration/<unit>/<task>/<attempt>/<RunId>/
artifacts/orchestration/<unit>/<task>/<attempt>/<RunId>/gate-contract.json
artifacts/orchestration/<unit>/<task>/<attempt>/<RunId>/delegation.json
artifacts/orchestration/<unit>/<task>/<attempt>/<RunId>/prompt.md
artifacts/orchestration/<unit>/<task>/<attempt>/<RunId>/worker-handoff.json
artifacts/orchestration/<unit>/<task>/<attempt>/<RunId>/controller-verification.json
artifacts/orchestration/<unit>/<task>/<attempt>/<RunId>/relay-record.json
artifacts/orchestration/<unit>/<task>/<attempt>/<RunId>/analysis.md
artifacts/orchestration/<unit>/<task>/<attempt>/<RunId>/events.jsonl
artifacts/orchestration/<unit>/<task>/<attempt>/<RunId>/output.json
artifacts/orchestration/<unit>/<task>/<attempt>/<RunId>/ci-core/
artifacts/orchestration/<unit>/<task>/<attempt>/<RunId>/ci-ui/
artifacts/orchestration/<unit>/<task>/<attempt>/{WorkRunId}/worker-handoff.json
artifacts/orchestration/<unit>/<task>-ncN/<attempt>/<RunId>/
artifacts/orchestration/<unit>/session-rebase/<RunId>/
```

El `RunId` lo asigna la sesión, nunca el modelo:

```powershell
$runId = 'R{0}-{1}' -f (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ'), ('{0:x4}' -f (Get-Random -Maximum 65536))
```

En la delegación, `ExpectedHandoffPath` lleva el token literal `{WorkRunId}`; la sesión lo sustituye por el `RunId` de la invocación de trabajo al componer `prompt.md`.

## 3. Relevo

### 3.1 Salida (antes de cada invocación)

```powershell
$wt = (git rev-parse --show-toplevel)
git status --porcelain                        # vacío
git rev-parse HEAD
git ls-remote origin "refs/heads/$(git branch --show-current)"   # mismo SHA que HEAD
git fetch origin; git rev-parse origin/main
git log -1 --format=%B                        # el cuerpo lleva el resumen de estado
$cfg = Join-Path $env:USERPROFILE '.codex\config.toml'
(Get-FileHash -LiteralPath $cfg -Algorithm SHA256).Hash
Select-String -LiteralPath $cfg -Pattern '^\s*(\[[^\]]+\]|[A-Za-z0-9_.-]+\s*=)' | ForEach-Object { ($_.Line -split '=')[0].Trim() }   # solo nombres, sin valores
```

Si `HEAD` coincide con el remoto y el último commit lleva el resumen, no hace falta un commit nuevo.

### 3.2 Procesos vivos

Se ejecuta desde **PowerShell**, no desde Git Bash: bajo MSYS2 la cadena de `ParentProcessId` se rompe y los shells propios aparecen como no atribuibles (medido en la sonda
PR-1, desviación DEV-G2-01 de I-61).

```powershell
Get-CimInstance Win32_Process |
  Select-Object ProcessId, ParentProcessId, Name, CommandLine,
                @{ n = 'CreationDateUtc'; e = { $_.CreationDate.ToUniversalTime().ToString('o') } }
```

Clasificación de cada proceso para el registro (`Processes[].Classification`):

| Clase | Cuándo |
|---|---|
| `own-tree` | Es el proceso que ejecuta la comprobación o uno de sus ancestros por `ParentProcessId` |
| `owner-app` | `codex*.exe` con línea de órdenes legible que no contiene la ruta del worktree (se registran solo PID y nombre) |
| `nominal-exclusion` | `codex-windows-sandbox-service.exe` (línea de órdenes ilegible) |
| `build-server` | `VBCSCompiler.exe`; `MSBuild.exe` o `dotnet.exe` con `/nodemode`, `build-server` o `VBCSCompiler.dll`. Solo exime en la comparación de descendientes de un Worker subagente y en la de huérfanos; en la evaluación general, si su línea de órdenes contiene la ruta del worktree, es `participant` |
| `participant` | Su línea de órdenes contiene la ruta del worktree (con `\` o `/`) o `-C <worktree>` |
| `unattributable` | Línea de órdenes ilegible y nombre en la lista cerrada (`codex*.exe`, `claude.exe`, `node.exe`, `git.exe`, `pwsh.exe`, `powershell.exe`, `bash.exe`, `dotnet.exe`), salvo la exclusión nominal |

Para un Worker subagente, además, se listan siempre con dos clases más:

| Clase | Cuándo |
|---|---|
| `session-descendant` | Descendiente de la sesión que no cae en las clases anteriores; el `Exit` los registra y el `Entry` los compara |
| `orphan` | Proceso de la lista cerrada creado en la ventana de la cesión cuyo padre no existe en el `Entry`, o existe con una `CreationDate` posterior (PID reutilizado) |

`Processes[]` lista los procesos de estas clases; los demás procesos legibles sin la ruta del worktree no se registran. La regla y sus consecuencias están en AUTOMATION_PLAN 16.4;
en resumen, un `participant` ajeno, un `unattributable`, o en la entrada un `session-descendant` nuevo o un `orphan` vivo (salvo servidores de compilación) es STOP (P-02). Para un Worker subagente se compara la lista de descendientes de la sesión del `Exit` con la del `Entry`, y se
evalúan los huérfanos con `CreationDateUtc` dentro de la ventana de la cesión.

### 3.3 Cesión

Mientras el participante trabaja, la sesión no lee, no escribe y no ejecuta nada sobre el worktree, tampoco con sus subagentes. Registra `Cession.StartUtc` y `Cession.EndUtc`.

### 3.4 Entrada

Se repiten las comprobaciones de 3.1, se confirma que el participante y sus descendientes terminaron (3.2) y se compara el hash de `config.toml`. Si cambió, STOP P-01 y se
registran solo los nombres de claves que difieren.

## 4. Aceptación del paquete (A1-A8)

Antes de lanzar al Worker, el Coordinator evalúa las ocho comprobaciones sin cortocircuito y anota cada resultado en `Outcome.Acceptance`:

```powershell
$schemas = Join-Path $wt 'docs\automation\agent-execution\schemas'
Get-Content -Raw delegation.json | Test-Json -SchemaFile (Join-Path $schemas 'delegation.schema.json')   # A1
```

- **A2:** `Unit`, `Gate` y `TaskId` iguales a los del contrato; `RunId` = el de un registro de planificación `COMPLETED` y no usado por otra delegación aceptada.
- **A3-A5:** inclusión de alcances, invariantes, condiciones STOP, autoridades y pruebas, con la sintaxis de alcance de AUTOMATION_PLAN §16 (archivo exacto o prefijo terminado en `/`).
- **A6:** `AuthorityRevision`, `MainSha` (contra `git rev-parse origin/main` recién obtenido), `BaseSha` = `HEAD` = remoto, rama y worktree.
- **A7:** celda, modelo y effort entre las celdas elegibles del contrato.
- **A8:** `Attempt`, `AttemptsRemaining`, `MaxReworkLoops`, `RoutingEnforcement`, `ChainBaseSha`, `ChainRedSha`, `ChainRedFiles`, `ExpectedHandoffPath` y `Owner`.

La disposición resultante es la más grave de las fallidas, según la tabla de AUTOMATION_PLAN §16.

## 5. Invocación del Controller (Codex)

Elementos fijos de la receta: binario por ruta verificada; `-C` con el worktree de la unidad y ningún otro directorio; `-s read-only`; `-m` y `-c model_reasoning_effort=…` de la
celda elegida; `--output-schema`; `-o` al directorio del `RunId`; `--json`; sin `--ephemeral`; stdin cerrado; el `pwsh` del runtime de Codex delante en el `PATH` solo del
proceso hijo; tope de 600 s con terminación del árbol; hash de `config.toml` antes y después. El `service_tier` se hereda de la configuración del Owner y no se toca.

La orden exacta medida en la sonda PR-1 está en la evidencia de I-61 (§14). Plantilla:

```bash
codex_bin="$LOCALAPPDATA/OpenAI/Codex/bin/<version>/codex.exe"
run_dir="artifacts/orchestration/<unit>/<task>/<attempt>/<RunId>"
PATH="/c/Users/<usuario>/.cache/codex-runtimes/codex-primary-runtime/dependencies/native/powershell:$PATH" \
timeout --kill-after=10 600 "$codex_bin" exec -C "$wt" -s read-only -m "<modelo>" -c model_reasoning_effort="<valor>" \
  --output-schema "docs/automation/agent-execution/schemas/<esquema>.schema.json" -o "$run_dir/output.json" --json \
  "$(cat "$run_dir/prompt.md")" < /dev/null > "$run_dir/events.jsonl"
```

Tras la invocación: el `thread_id` del evento `thread.started` localiza el registro de sesión en `~/.codex/sessions/`; de él salen `session_meta.model` y `turn_context.model`/`effort`
(modelo y effort efectivos). Si el proceso sigue vivo al vencer el tope: `taskkill /PID <pid> /T /F` y se repite la comprobación de 3.2 hasta confirmar la muerte.

## 6. Invocación del Worker (subagente)

La sesión lanza el subagente con el modelo y el effort del paquete y el `prompt.md` compuesto, y espera la notificación de finalización (tope de 60 min). El modelo y el effort efectivos
salen de la transcripción del subagente (`"model"`, `"effort"`). La conducta al vencer el tope y la prohibición de subagentes que sobrevivan a su llamada están en AUTOMATION_PLAN
16.4.

## 7. Hechos remotos

```powershell
gh run list --branch <rama> --event push --commit <CurrentSha> --json databaseId,status,conclusion,headSha,headBranch,event
gh run view <run_id> --json event,headBranch,headSha,status,conclusion,jobs
gh run download <run_id> --name rackcad-core-test-diagnostics --dir <dir del RunId>\ci-core
gh run download <run_id> --name rackcad-ui-test-diagnostics --dir <dir del RunId>\ci-ui
```

Se registran en `RemoteFacts` (con el id de GitHub en `GhRunId`): `ref` (`refs/heads/` + `headBranch`), `event`, `head_sha`, la conclusión de cada job requerido y, de los TRX, seleccionadas, superadas, fallidas y los
nombres de las fallidas. Se espera hasta 90 min sin reinvocar a nadie; la disposición la fija la fila `Ci` de AUTOMATION_PLAN §16.

## 8. Coherencia de la verificación

Además de `Test-Json`, el relevo aplica esta comprobación mecánica a `controller-verification.json` (OBL-11):

```powershell
$v = Get-Content -Raw controller-verification.json | ConvertFrom-Json
$pairs = @{ 'EXECUTION_VERIFIED' = 'NONE'; 'EXECUTION_REWORK_REQUIRED' = 'REWORK' }
$okPair = ($pairs[$v.Classification] -eq $v.Disposition) -or
          ($v.Classification -eq 'EXECUTION_BLOCKED' -and $v.Disposition -in 'BLOCKED', 'STOP')
$order = 'Termination','Handoff','Authority','Contract','Identity','Remote','Scope','CleanTree','Ci','Tests','Trailer','Routing','FreeText','Denials'   # orden de 16.9
$notPass = @($order | Where-Object { $v.Checks.$_.Result -ne 'pass' } | ForEach-Object { [pscustomobject]@{ Name = $_ } })
$okVerified = ($v.Classification -eq 'EXECUTION_VERIFIED') -eq ($notPass.Count -eq 0)          # VERIFIED ⇔ 14 en pass
$okClass = if ($notPass.Count -eq 0) { $v.FailureClass -eq 'NONE' } else { $v.FailureClass -eq $notPass[0].Name -or $v.FailureClass -eq 'StopCondition' }
$okRed = @('Ci', 'Tests' | Where-Object { $v.Checks.$_.Result -eq 'pass' -and $v.Checks.$_.RedPart -eq 'fail' }).Count -eq 0
$okPair -and $okVerified -and $okClass -and $okRed          # False → salida inválida (INVALID_OUTPUT)
```

`FailureClass` debe ser la primera comprobación que no está en `pass` (o `StopCondition` para un STOP no ligado a una comprobación); la `Disposition` sigue la precedencia de
AUTOMATION_PLAN 16.9 sobre todas las fallidas.

## 9. Escenarios de conteo (OBL-08)

La regla es la de AUTOMATION_PLAN 16.8 (con 16.6, 16.7 y 16.11). Escenarios para comprobarla sobre un registro; son los diecinueve del Freeze de I-61 (Proposal V9 §9), con las
referencias reescritas:

| Escenario | Resultado esperado |
|---|---|
| Primera delegación de una tarea | `Attempt` = `attempts`; nada se incrementa; RED exigido |
| REWORK de clase X, contador X = 0, `attempts` < máximo y RED acreditado | commit con `attempts`+1, delegación con el nuevo `Attempt`, `ChainRedSha` y `ChainRedFiles`, contador X = 1 |
| RED de una corrección que solo desactiva el fix en `src/` | se acredita; `ChainRedFiles` incluye igualmente las pruebas de la cadena desde `ChainBaseSha` |
| GREEN que modifica pruebas de `RT` ∪ `ChainRedFiles` | `Tests` con `RedPart` = `fail`: REWORK (las pruebas modificadas no se vieron fallar) |
| Corrección tras un RED no acreditado, con RED que solo desactiva el fix y GREEN que modifica pruebas de la cadena | `RT` (desde `ChainBaseSha`) las incluye: `RedPart` = `fail`, REWORK |
| REWORK por `RedPart` = `fail` con un RED acreditado anterior | RED vigente = `null`; `ChainRedFiles` se conserva; la corrección exige RED |
| REWORK sin RED acreditado (p. ej. la corrida del `RedSha` no falló) | la corrección exige RED (corrección desactivada, luego GREEN); `ChainRedSha` = `null` |
| Entrega sin `RedSha` cuando exige RED | `Ci` y `Tests` con `RedPart` = `fail`: REWORK |
| Entrega con RED acreditado en la cadena cuyo diff toca `ChainRedFiles` | exige RED (caducado): sin RED nuevo, `RedPart` = `fail` y REWORK; con RED nuevo acreditado, nuevo `ChainRedSha` y `ChainRedFiles` ampliado |
| REWORK de clase Y ≠ X | `analysis.md` antes de la corrección; `attempts`+1 |
| REWORK de clase X con contador X = 3 | STOP |
| `attempts` = `max_attempts` y nuevo REWORK | STOP |
| STOP resuelto por el Coordinator con un cambio del trabajo | `analysis.md`; `attempts`+1 |
| STOP por avance de `main` | sin incremento; recuperación de AUTOMATION_PLAN 16.7; como máximo dos recuperaciones por tarea, la tercera → STOP |
| BLOCKED o fallo de transporte | sin incremento; reejecución con `RunId` nuevo, máximo dos por fase; la tercera → STOP |
| Una invocación excedería el tope de invocaciones del plan de gates de la unidad | no se lanza; STOP (P-07) |
| Cambio de modelo, rol o sesión | sin reinicio |
| Control negativo | ruta y registro propios, sin incremento |
| Trabajo directo de la sesión con CI roja (fuera de la ejecución delegada) | AUTOMATION_PLAN §8-9: la corrección incrementa `attempts` |

## 10. Controles negativos

Se ejecutan una sola vez (contando solo los de salida válida), y solo sobre las entradas de la verificación `EXECUTION_VERIFIED` del último `WorkRunId` de la cadena; si la cadena termina
sin VERIFIED, no se ejecutan. Los ejecuta el Controller Codex real con `CONTROLLER_VERIFICATION` y el mismo prompt, salvo la ruta de entrada y el `RunId` de la invocación. Cada uno usa una
ruta `<task>-ncN` y un registro propios, con copias de todas las entradas que conservan sus `TaskId`, `RunId`, `DelegationRunId` y `Attempt` internos salvo el campo mutado. No consumen
`attempts`.

**Oráculo relativo:** la comprobación mutada queda en `fail`, `FailureClass` es esa comprobación y todas las anteriores en el orden de AUTOMATION_PLAN 16.9 tienen el mismo `Result`
(`pass`) que en la verificación real. Una salida ausente o inválida es un fallo de transporte de la fase `CONTROL`, reejecutable con `RunId` nuevo hasta dos veces por control; agotado
ese tope, STOP (P-04) y control no superado.

- **nc1:** entrega con `CurrentSha` inexistente → `Identity`; `EXECUTION_BLOCKED/STOP`.
- **nc2:** delegación cuyo `AllowedWriteScope` excluye un archivo de `git diff --name-only BaseSha..CurrentSha` de la delegación verificada (el primero en orden lexicográfico,
  registrado; si una entrada de prefijo lo cubre, se sustituye por la enumeración de los demás archivos del diff) → `Scope`; `EXECUTION_BLOCKED/STOP`.
- **nc3:** entrega con un término de gate en `WorkCompleted` → `FreeText`; `EXECUTION_REWORK_REQUIRED/REWORK`; las comprobaciones posteriores, igual que en la real.
- **nc4** (aceptación, sin invocar; solo en la primera delegación de la cadena y antes de aceptar la real): copia de `delegation.json` que solo cambia `AllowedWriteScope`, más amplio que
  el contrato, evaluada con A1-A8 sin cortocircuito. Oráculo relativo: A3 en `fail` y las demás con el mismo resultado que en la aceptación de la delegación real; disposición STOP (P-03)
  registrada como `REJECTED_BEFORE_INVOCATION` en `artifacts/orchestration/<unit>/<task>-nc4/…`, confinada al control y sin contar para ningún tope. Si la real no pasa A6 por avance de
  `main`, nc4 se repite sobre la primera delegación de la cadena que lo pase.

## 11. Custodia

Antes de que el Coordinator registre una decisión, la sesión copia los JSON y MD que la respaldan a `docs/automation/evidence/<unit>-pilot/<task>/<RunId>/`, hace commit y anota para
cada uno el SHA-256 transitorio (procedencia) y el blob versionado (`git rev-parse <commit>:<ruta>`). Si difieren por fin de línea, se declara. Los eventos y registros de sesión no se
versionan: solo su SHA-256 y los campos extraídos.
