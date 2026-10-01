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
| `build-server` | `VBCSCompiler.exe`; `MSBuild.exe` o `dotnet.exe` con `/nodemode`, `build-server` o `VBCSCompiler.dll` |
| `participant` | Su línea de órdenes contiene la ruta del worktree (con `\` o `/`) o `-C <worktree>` |
| `unattributable` | Línea de órdenes ilegible y nombre en la lista cerrada (`codex*.exe`, `claude.exe`, `node.exe`, `git.exe`, `pwsh.exe`, `powershell.exe`, `bash.exe`, `dotnet.exe`), salvo la exclusión nominal |

Un `participant` ajeno o un `unattributable` es STOP (P-02). Para un Worker subagente se compara la lista de descendientes de la sesión del `Exit` con la del `Entry`, y se
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
salen de la transcripción del subagente (`"model"`, `"effort"`). No se usan subagentes que sobrevivan a su llamada.

## 7. Hechos remotos

```powershell
gh run list --branch <rama> --event push --commit <CurrentSha> --json databaseId,status,conclusion,headSha,headBranch,event
gh run view <run_id> --json event,headBranch,headSha,status,conclusion,jobs
gh run download <run_id> --name rackcad-core-test-diagnostics --dir <dir del RunId>\ci-core
gh run download <run_id> --name rackcad-ui-test-diagnostics --dir <dir del RunId>\ci-ui
```

Se registran en `RemoteFacts`: `ref` (`refs/heads/` + `headBranch`), `event`, `head_sha`, la conclusión de cada job requerido y, de los TRX, seleccionadas, superadas, fallidas y los
nombres de las fallidas. Se espera hasta 90 min sin reinvocar a nadie; la disposición la fija la fila `Ci` de AUTOMATION_PLAN §16.

## 8. Coherencia de la verificación

Además de `Test-Json`, el relevo aplica esta comprobación mecánica a `controller-verification.json` (OBL-11):

```powershell
$v = Get-Content -Raw controller-verification.json | ConvertFrom-Json
$checks = $v.Checks.PSObject.Properties
$pairs = @{ 'EXECUTION_VERIFIED' = 'NONE'; 'EXECUTION_REWORK_REQUIRED' = 'REWORK' }
$okPair = ($pairs[$v.Classification] -eq $v.Disposition) -or
          ($v.Classification -eq 'EXECUTION_BLOCKED' -and $v.Disposition -in 'BLOCKED', 'STOP')
$okVerified = ($v.Classification -ne 'EXECUTION_VERIFIED') -or
              (@($checks | Where-Object { $_.Value.Result -ne 'pass' }).Count -eq 0 -and $v.FailureClass -eq 'NONE')
$okPair -and $okVerified          # False → salida inválida (INVALID_OUTPUT)
```

## 9. Escenarios de conteo (OBL-08)

La regla es la de AUTOMATION_PLAN §16. Escenarios para comprobarla sobre un registro:

| Escenario | Resultado esperado |
|---|---|
| Primera delegación | `Attempt` = `attempts`; nada se incrementa; la entrega exige RED |
| REWORK de clase X, contador 0, intentos disponibles | commit con `attempts`+1 antes de la delegación de corrección; contador X = 1 |
| REWORK de otra clase | `analysis.md` antes de la corrección; `attempts`+1 |
| REWORK de clase X con contador 3 | STOP |
| `attempts` = `max_attempts` y nuevo REWORK | STOP |
| BLOCKED o fallo de transporte | sin incremento; `RunId` nuevo; dos reejecuciones por fase, la tercera → STOP |
| STOP por avance de `main` | sin incremento; recuperación (máximo dos por tarea) |
| Invocación que superaría el tope | no se lanza; STOP P-07 |
| Control negativo | ruta y registro propios; sin incremento |

## 10. Controles negativos

Se ejecutan una vez sobre las entradas de la verificación `EXECUTION_VERIFIED` final, con el mismo prompt salvo la ruta de entrada y el `RunId`, en un directorio `<task>-ncN`:

- **nc1:** `CurrentSha` inexistente en la entrega → `Identity` en `fail`.
- **nc2:** se quita de `AllowedWriteScope` un archivo del diff verificado → `Scope` en `fail`.
- **nc3:** un término de gate en `WorkCompleted` → `FreeText` en `fail`.
- **nc4** (aceptación, sin invocar): `AllowedWriteScope` más amplio que el contrato → solo A3 en `fail`.

## 11. Custodia

Antes de que el Coordinator registre una decisión, la sesión copia los JSON y MD que la respaldan a `docs/automation/evidence/<unit>-pilot/<task>/<RunId>/`, hace commit y anota para
cada uno el SHA-256 transitorio (procedencia) y el blob versionado (`git rev-parse <commit>:<ruta>`). Si difieren por fin de línea, se declara. Los eventos y registros de sesión no se
versionan: solo su SHA-256 y los campos extraídos.
