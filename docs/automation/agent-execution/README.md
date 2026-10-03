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
| `participant` | Su línea de órdenes contiene la ruta del worktree (sin distinguir mayúsculas, con `\` o `/`) o `-C <worktree>` |
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

## 12. Autoverificación del Principal y `CONFIGURATION_STATUS` (unidades I62)

Materializado por I-62 e **inactivo** hasta su vigencia ([AUTOMATION_PLAN](../../AUTOMATION_PLAN.md) 16.14): hasta entonces no se aplica a ninguna unidad.
Las reglas están en AUTOMATION_PLAN 16.15 (perfil por acción y autoverificación) y 16.16 (estados, agregado y disposición); aquí solo está cómo calcular
el agregado de una acción.

**Entrada:** una fila por requisito, con `RequirementId`, `Mandatory`, `Required` y `Observation` (`State` OBSERVED o NOT_OBSERVED, `Value`, `Source`,
`Assurance` y `ObservedUtc`). **Salida:** el `Status` y la `Contradiction` de cada fila, y `ConfigurationStatus`, `Causes` y `Disposition` (`ELIGIBLE`,
`NOT_ELIGIBLE` o `STOP`) de la acción.

1. Fija los requisitos obligatorios de la acción según el perfil (AUTOMATION_PLAN 16.15). Un obligatorio sin fila se añade con `State` NOT_OBSERVED.
2. Calcula el `Status` de cada fila:
   - dos observaciones del mismo requisito con valores distintos → `Contradiction` true y UNKNOWN;
   - `State` NOT_OBSERVED, `Assurance` por debajo de RUNTIME_OBSERVED u observación invalidada → UNKNOWN;
   - en otro caso, el valor observado frente al requerido, en la escala del requisito: menor → BELOW_REQUIRED; mayor → ABOVE_REQUIRED; igual → MATCH.
3. `ConfigurationStatus` = el agregado de AUTOMATION_PLAN 16.16 sobre las filas obligatorias. Las opcionales se registran y no lo alteran.
4. `Causes` = los requisitos obligatorios cuyo `Status` no es MATCH, más las contradicciones.
5. `Disposition`, según la tabla de AUTOMATION_PLAN 16.16:
   - `STOP` si hay una contradicción (S-04) o si el Principal está en BELOW_REQUIRED para la acción (P-09);
   - si no, `NOT_ELIGIBLE` (P-10) con UNKNOWN, o con otro rol en BELOW_REQUIRED. Se sale con una observación nueva, nunca con una reconfiguración
     automática ni un reset;
   - si no (MATCH o ABOVE_REQUIRED), `ELIGIBLE`: habilita solo la configuración, y los demás STOP siguen.
6. Cada acción se calcula aparte: un mismo Principal puede tener CUSTODY en UNKNOWN y RESUME_DECISION en MATCH.

La tabla siguiente es copia fila a fila de la tabla de ejemplos de la [Proposal V14](../../initiatives/I-62-proposal-v14.md) §4.2 (Freeze), que es la
fuente del oráculo. Sus referencias «§» remiten a esa Proposal; P-09 y P-10 son los de AUTOMATION_PLAN 16.16, S-04 el de 16.11 y P-15 el de la
Proposal V14 §13.

| Caso | Entrada | Agregado | Disposición |
|---|---|---|---|
| E1 | todo = requisito; RUNTIME_OBSERVED | MATCH | sigue |
| E2 | effort > requisito | ABOVE_REQUIRED | sigue y registra |
| E3 | effort < requisito | BELOW_REQUIRED | STOP P-09 (Principal) |
| E4 | effort sin fuente | UNKNOWN | P-10 |
| E5 | nivel < requisito; effort sin fuente | BELOW_REQUIRED; `Causes` = {nivel, effort} | STOP P-09 |
| E6 | dos observaciones RUNTIME_OBSERVED simultáneas y distintas | UNKNOWN; `Contradiction` | STOP S-04 |
| E7 | observación invalidada (§6) | UNKNOWN | revalidar |
| E8 | rebinding del Worker | Y evaluado desde cero | contadores intactos |
| E9 | cuota desconocida (opcional) | MATCH | registrada |
| E10 | el Owner rebaja un requisito | recálculo | lo no observado sigue UNKNOWN |
| E11 | requisito obligatorio omitido en el preflight | UNKNOWN (falta acreditar) | P-10; `Causes` lo nombra |
| E12 | Principal sin `repo-write` acreditado; lo demás en MATCH | CUSTODY: UNKNOWN; RESUME_DECISION: MATCH | no toma la custodia delegada (P-10); puede responder RESUME_DECISION |
| E13 | unidad DIRECT_ONLY; CUSTODY sin observar | CUSTODY: UNKNOWN | sin efecto sobre el trabajo directo; ningún contrato delegado (P-15) |

## 13. Observación de capacidad, preflight y saneamiento (unidades I62)

Materializado por I-62 e **inactivo** hasta su vigencia ([AUTOMATION_PLAN](../../AUTOMATION_PLAN.md) 16.14). Las reglas están en AUTOMATION_PLAN 16.18
(observación y validación) y 16.19 (adapters y huella); los hechos propios de cada runtime, en su descriptor ([adapters/](adapters/)). Aquí solo está
cómo producir, comprobar, invalidar y sanear un `rackcad-preflight/v1`.

### 13.1 Producción

1. **Ámbito:** unidad, rol, acción y perfil. Los `RequirementId` y sus valores requeridos salen de [routing.md](routing.md) §8, con su blob en `RoutingBlob`.
2. **Host:** `HostLabelHash` = SHA-256 del hostname en minúsculas; en Windows, `HostInstanceHash` = SHA-256 del `MachineGuid` en minúsculas
   (`HKLM:\SOFTWARE\Microsoft\Cryptography`), con `HostInstanceState` OBSERVED; sin él, `UNKNOWN` y UNOBSERVED. Nunca se registran en claro.
3. **Adapter:** `DescriptorRef` = ruta del descriptor y su blob en la `AuthorityRevision`; `AdapterVersion` y `BinaryPathHash` según el descriptor
   (`UNKNOWN` o `null` donde el descriptor lo indica).
4. **Actor y sesión:** la instancia observada; para una candidata sin sesión, `InstanceId` = `NOT_STARTED` con `Assurance` NONE.
5. **Hechos:** los de la tabla «Hechos» del descriptor, con `Facts.SchemaId` igual al de `SchemaRef`. Lo que exige invocar el runtime y no se invoca
   queda en su estado explícito (`UNKNOWN`, `NOT_OBSERVED`), nunca con un valor supuesto.
6. **Huella:** el `Kind` que declara el descriptor. CONFIG_FILE: `Sha256` del archivo y `KeyNames` saneados (§13.4); NONE: `AcceptanceDecisionRef` de la
   decisión del Coordinator; UNVERIFIED: sin hash ni nombres.
7. **Requisitos:** una fila por requisito del perfil y la acción, con su observación, fuente y nivel; `Status`, agregado, `Causes` y `Disposition` según §12.
8. **Invalidadores:** los valores observados de la instancia del host, la versión y la ruta del adapter, la autenticación, la huella, el blob de
   `model-catalog.md` y el de `routing.md`.

### 13.2 Validación y coherencia

Fase 1 y fase 2 con `Test-Json` (AUTOMATION_PLAN 16.18), y después estas reglas mecánicas. Cualquier fallo es P-14, salvo la contradicción, que es S-04:

```powershell
$core = Join-Path $schemas 'preflight.v1.schema.json'
Get-Content -Raw $preflight | Test-Json -SchemaFile $core                      # fase 1
$p = Get-Content -Raw $preflight | ConvertFrom-Json
$p.AdapterFacts.Facts | ConvertTo-Json -Depth 20 |
  Test-Json -SchemaFile (Join-Path $protocol $p.AdapterFacts.SchemaRef.Path)    # fase 2
```

| Regla | Comprueba |
|---|---|
| C1 | el descriptor `adapters/<AdapterId>.md` existe y su blob es `DescriptorRef.Blob` |
| C2 | `SchemaRef.Path` = `schemas/adapters/<AdapterId>.facts.v<n>.schema.json`, con la `<n>` que declara el descriptor, y su blob es `SchemaRef.Blob` |
| C3 | `Facts.SchemaId` = `SchemaRef.SchemaId` |
| C4 | huella: CONFIG_FILE con `Sha256`; NONE con `AcceptanceDecisionRef` y sin `Sha256`; UNVERIFIED sin `Sha256`, sin nombres y sin decisión |
| C5 | `RequirementId` único; están todos los obligatorios del perfil y la acción |
| C6 | `Status`, `ConfigurationStatus`, `Causes` y `Disposition` iguales a los que da §12 sobre las filas |
| C7 | `Value`, `Source` y `ObservedUtc` nulos solo con NOT_OBSERVED, y `ContradictionEvidence` solo con `Contradiction` true |
| C8 | `Invalidators.FingerprintSha256` = `Fingerprint.Sha256`, o `NONE` o `UNKNOWN` según el `Kind`; `Invalidators.AdapterVersion` y `BinaryPathHash` iguales a los de `Adapter` |

El productor registra en `<PreflightId>.validation.json` la versión de PowerShell, el resultado de cada fase y el de cada regla. Sin ese registro el
preflight no se acepta.

### 13.3 Invalidación y contraste

- Un preflight anterior deja de valer si cambia cualquiera de sus `Invalidators` frente a la observación actual. Sus requisitos pasan a UNKNOWN y hace
  falta una observación nueva; nunca una reconfiguración automática ni un reset.
- Cambiar o crear un binding, o avanzar el SHA de la rama, **no** invalida la observación: el binding la consume y no forma parte de los invalidadores.
- En cada relevo, el aceptante compara la autenticación, la versión del binario y la huella del preflight con las del Exit/Entry del `relay-record/v2`. Una
  discrepancia es S-04.

### 13.4 Saneamiento previo a la custodia

Antes de copiar un preflight, un `relay-record/v2` o su validación a la evidencia, la sesión los sanea con estas reglas:

| Campo | Regla |
|---|---|
| `Fingerprint.KeyNames` | un nombre con una ruta o una unidad (`\`, `/` o `:`) se sustituye por su sección con `<redactado>` (p. ej., `[projects.<redactado>]`), conservando el número de nombres |
| texto libre: `Observation.Value`, `ContradictionEvidence`, `Notes`, `Evidence`, `Diagnosis` y mensajes de error | cada coincidencia con un patrón de la tabla siguiente se sustituye por `<redactado:ID>` |
| cualquier otro campo | una coincidencia con un patrón **rechaza la custodia**: se corrige el productor, nunca se publica |

| ID | Patrón (expresión regular) |
|---|---|
| PEM | `-----BEGIN [A-Z ]*PRIVATE KEY-----` |
| JWT | `eyJ[A-Za-z0-9_-]{10,}\.[A-Za-z0-9_-]{10,}\.[A-Za-z0-9_-]*` |
| SK | `\bsk-[A-Za-z0-9_-]{16,}` |
| GH | `\b(gh[pousr]_[A-Za-z0-9]{20,}\|github_pat_[A-Za-z0-9_]{20,})` |
| AWS | `\bAKIA[0-9A-Z]{16}\b` |
| SLACK | `\bxox[abposr]-[A-Za-z0-9-]{10,}` |
| GAPI | `\bAIza[0-9A-Za-z_-]{35}\b` |
| BEARER | `(?i)\bbearer\s+[A-Za-z0-9._~+/-]{16,}=*` |
| KV | `(?i)\b(password\|passwd\|secret\|token\|api[_-]?key\|access[_-]?key\|client[_-]?secret)\b\s*[:=]\s*\S+` |
| URLCRED | `[a-z][a-z0-9+.-]*://[^/\s:@]+:[^/\s@]+@` |

Nunca se leen ni se registran valores de configuración ni credenciales: la huella solo lleva el hash y los nombres. Los esquemas I62 no tienen campos de
credenciales.

## 14. Binding, aceptación, independencia y comprobaciones (unidades I62)

Materializado por I-62 e **inactivo** hasta su vigencia ([AUTOMATION_PLAN](../../AUTOMATION_PLAN.md) 16.14). Las reglas están en AUTOMATION_PLAN 16.20
(binding y aceptación), 16.21 (independencia) y 16.22 (aceptación del paquete y comprobaciones). Aquí solo está cómo producir y comprobar los registros. Toda
comprobación es mecánica y falla cerrada: un dato ausente o UNKNOWN nunca se completa con un valor favorable.

### 14.1 Producción de un binding

1. **Requisitos:** los `RequirementId` del perfil y la acción ([routing.md](routing.md) §8) más los `Mandatory` del rol en el contrato de gate
   (`RoleRequirements[]`), que puede exigir más, nunca menos.
2. **Candidatas:** [routing.md](routing.md) §9.
3. **Observación:** un `rackcad-preflight/v1` vigente de la candidata para la misma unidad, rol y acción (§13). Sin él, no hay binding.
4. **Elegibilidad e independencia:** las reglas B4-B6 (§14.2).
5. **Registro:** `rackcad-binding/v1` con `Acceptance.State` = PENDING, salvo una materialización autorizada, que nace ACCEPTED (§14.3).
6. **Aceptación** individual (A7') o materializada (§14.3), y custodia a más tardar en el punto durable que reserva la invocación.

### 14.2 Coherencia del binding

Fase 1: `Test-Json` contra `schemas/binding.v1.schema.json`. Después estas reglas, todas sin cortocircuito. La disposición es la más grave de las fallidas:

| Regla | Comprueba | Si falla |
|---|---|---|
| B1 | el registro valida contra el esquema | rechazo |
| B2 | `Scope` = TASK con `TaskId` no nulo; `Scope` = UNIT con `TaskId` nulo | rechazo |
| B3 | `Cell.AdapterId` = `Actor.AdapterId` = `Session.AdapterId` = el adapter del preflight referenciado; `Cell.CellId` = `<AdapterId>:<modelo o ->:<EffortSemantic>` ([routing.md](routing.md) §9); `Cell.EffortSemantic` = el effort observado en el preflight; `Cell.CatalogBlob` = su `Invalidators.CatalogEntryBlob`; si el contrato limita las celdas del rol, `CellId` está entre sus `EligibleCells[].Cell` | rechazo (celda incompatible) |
| B4 | el preflight existe, es válido (§13.2), no está invalidado (§13.3), es de la misma unidad, rol y acción, tiene una fila por **cada** requisito de 1 y todas están en MATCH o ABOVE_REQUIRED | P-10 (un requisito omitido cuenta como no acreditado) |
| B5 | `Eligibility`: `MeasuredInvocation.State` = MEASURED con `RunRef` no nulo, `ConsumptionCovered` ≠ UNKNOWN y `Stale` = false. `RunRef` es nulo si y solo si NOT_MEASURED | P-10 |
| B6 | `Independence.Requirements` cubre cada referencia del contrato con cada dimensión ≥ la del contrato; `Satisfaction` es único por (`ReferenceRole`, `Dimension`) y tiene una fila por cada dimensión distinta de NOT_REQUIRED; toda dimensión REQUIRED está en SATISFIED (§14.5) | binding no aceptado; la operación dependiente se bloquea |
| B7 | combinaciones de `Acceptance`: PENDING ⇒ `Basis`, `DecisionRef`, `AuthorizationRef`, `MaterializationCheck` y `Utc` nulos; INDIVIDUAL_DECISION ⇒ `DecisionRef` no nulo y `AuthorizationRef` = `MaterializationCheck` = null; AUTHORIZED_MATERIALIZATION ⇒ `State` = ACCEPTED, `DecisionRef` = null, `AuthorizationRef` y `MaterializationCheck` no nulos | P-20 |
| B8 | AUTHORIZED_MATERIALIZATION solo con `Role` = ARCHITECT, o REVIEWER cuando el contrato de gate trae `RoleRequirements[].Materialization` para ese rol | P-20 |
| B9 | `DecisionRef` = una decisión registrada del Coordinator: el marcador existe en el archivo de decisiones de la unidad y la nombra; nunca un marcador fabricado | P-20 |
| B10 | mismo `BindingId` con contenido distinto: solo se admite la transición PENDING → ACCEPTED o REJECTED con el resto idéntico | S-04 |

`Eligibility`, `Independence.Satisfaction` y `Acceptance` son campos con autoridad: el aceptante los **recalcula** con las fuentes custodiadas, y una
diferencia con lo registrado es S-04.

### 14.3 Materialización autorizada

**Fuente de los criterios:** la autorización de materialización (para ARCHITECT, la `ReviewLoopAuthorization` en el archivo de decisiones; para REVIEWER,
`RoleRequirements[].Materialization` del contrato de gate), leída en el `Commit`/`Blob` de `AuthorizationRef`. Un criterio por campo:

| `CriterionId` | Se satisface cuando … |
|---|---|
| `ROLE_ACTION` | el rol del binding = `Role` y la acción ∈ `AuthorizedActions` |
| `CAPABILITY:<RequirementId>` | la fila del preflight está en MATCH o ABOVE_REQUIRED |
| `INDEPENDENCE:<ReferenceRole>:<Dimension>` | la satisfacción observada cumple la dimensión pedida (§14.5) |
| `CELL` | el `CellId` está en `EligibleCells.Cells` (modo LIST), o la celda cumple el criterio cerrado de ADR-0046 #4 (modo CRITERION, B5) |
| `MODEL_EFFORT` | nivel y effort dentro de `ModelEffortBounds` (los máximos nulos no limitan) |
| `PERMISSIONS` | la invocación será READ_ONLY |
| `OBJECT` | el objeto a revisar pertenece a `ObjectFamily` (unidad y patrón de rutas) |
| `VALIDITY` | la vigencia de acción no terminó: sin ARCHITECT_SATISFIED, agotamiento, revocación, sustitución ni enmienda posterior registrados, y el instante de la materialización ≤ `Until` |

Cada criterio se registra como `{CriterionId, Required, Observed, Result, Evidence}`. **UNKNOWN cuenta como NOT_SATISFIED.** Solo con todos en SATISFIED se
materializa. Si ninguna candidata satisface la autorización: sin invocación, STOP y COORDINATOR_DECISION, o ESCALATION_OWNER cuando lo que falta es
materia del Owner (autenticación, huella, compra).

**Reproducción (A7' y validador).** El aceptante relee la autorización en `AuthorizationRef.Commit`/`Blob` y comprueba que `Commit` es un ancestro del punto de
custodia del binding (nunca ese mismo commit), que el marcador `I62-REVIEW-LOOP-AUTHORIZATION: <AuthorizationId>` figura en ese blob y que cada criterio da
el mismo resultado con el preflight custodiado. Una diferencia, un `DecisionRef` no nulo o un `AuthorizationRef` ausente → binding inválido (P-20).

### 14.4 Referencias (A2')

Un `BindingRef` se acepta solo si cumple todas:
1. `UnitId` = la unidad del contrato;
2. con `Scope` = TASK, `TaskId` = la tarea del contrato; con UNIT, `TaskId` nulo;
3. resuelve a un binding cuyo contenido tiene el `Sha256` declarado y, si es CUSTODIED, `Commit` es un ancestro del punto que lo usa y `Blob` es el del archivo en
   ese commit;
4. no hay un binding posterior del mismo rol, unidad y ámbito (rebinding) que lo deje obsoleto;
5. un artefacto custodiado nunca usa un `BindingRef` TRANSIENT; en Q7, toda referencia TRANSIENT se resuelve a CUSTODIED por el manifiesto de custodia.

### 14.5 Evaluación de la independencia

**Referencia.** Los actores que escribieron el rango evaluado (`BaseSha..CurrentSha`) y, con `SupersededCommits`, también los de esos commits. Se evalúa
contra cada uno por separado.

**Por dimensión**, contra una referencia:
- **Actor:** SATISFIED si los `ActorRef` difieren y ambos tienen `Assurance` RUNTIME_OBSERVED o superior; NOT_SATISFIED si son iguales (aunque los `BindingId`
  difieran); UNKNOWN con `Assurance` NONE;
- **Sesión:** igual con `SessionRef`;
- **Contexto:** SATISFIED si las entradas son solo el cierre efectivo de insumos (§15.2) y las entradas automáticas están enumeradas; NOT_SATISFIED si el cierre
  incluye la transcripción, la memoria o el razonamiento de la referencia; UNKNOWN si el registro de lecturas falta o es incompleto;
- **Proveedor:** SATISFIED si los descriptores de adapter declaran proveedores distintos.

**Combinación:** por (referencia, dimensión), el máximo de lo que piden los disparadores aplicables (REQUIRED > PREFERRED > NOT_REQUIRED). Toda REQUIRED debe
quedar en SATISFIED; una PREFERRED no satisfecha se registra y no bloquea.

**Revisiones mayores de LIFECYCLE en una unidad I62** ([INITIATIVE_LIFECYCLE](../../INITIATIVE_LIFECYCLE.md) §5): el conjunto de referencia es
`ReviewSubject.Authors` (B.2), acotado a la unidad y al objeto:
- DESIGN: los autores de los commits que produjeron las versiones de la Proposal de la unidad o sus deltas aceptados;
- IMPLEMENTATION: los de `merge-base(base, Commit)..Commit`, restringidos a las rutas de cambio de la unidad;
- incluye cada titular Principal de cada sucesión o rebinding, cada Worker y cada autor humano, y el operador humano de cada sesión IA autora; la historia
  anterior a la unidad queda fuera.

| Modo del revisor | Actor | Sesión | Contexto | Proveedor |
|---|---|---|---|---|
| SEPARATE SESSION (revisor IA) | REQUIRED: su `ActorRef` difiere del de cada autor IA del conjunto | REQUIRED: su `SessionRef` difiere de cada sesión autora | REQUIRED: solo el cierre efectivo de insumos, con el contexto inyectado declarado antes de revisar y la auditoría de lecturas | PREFERRED |
| EXTERNAL HUMAN | REQUIRED: su `HumanReviewerRef` difiere de toda identidad humana de autor, incluido el operador que dirigió una sesión IA autora | REQUIRED: una `ReviewInstanceRef` propia de esa revisión | REQUIRED: los insumos canónicos revisados, declarados | no aplica |
| SAME-SESSION ROLE | no satisface el predicado de estas revisiones | — | — | — |

Un autor UNKNOWN dentro del conjunto, o un `Operator` = `"UNKNOWN"` frente a un revisor humano, deja la dimensión en UNKNOWN: no satisface. Un revisor
humano no lleva `ActorRef` ni `SessionRef` ficticios. La evidencia (`ReviewSubject`, referencias del revisor e insumos, y el estado de cada dimensión) va en la
cabecera del registro recuperable de la revisión.

**Casos de control** (C-13; esperado exacto):

| # | Caso | Esperado |
|---|---|---|
| 1 | dos disparadores simultáneos sobre la misma referencia | por dimensión, el máximo |
| 2 | mismo proveedor, contexto separado | Contexto SATISFIED, Proveedor NOT_SATISFIED |
| 3 | proveedor distinto, contexto compartido con la referencia | Proveedor SATISFIED, Contexto NOT_SATISFIED |
| 4 | misma instancia de actor con dos `BindingId` (Worker y verificador) | Actor NOT_SATISFIED → rechazo |
| 5 | PREFERRED no satisfecha frente a REQUIRED no satisfecha | registro frente a bloqueo |
| 6 | EXTERNAL HUMAN ajeno, `HumanReviewerRef` fuera del conjunto, insumos canónicos, sin `ActorRef` | aceptado |
| 7 | versión de dos titulares sucesivos; revisor igual al primero, no al actual | no satisfecho: el autor de la sucesión anterior está incluido |
| 8 | commit con autor no establecible dentro del conjunto | UNKNOWN, no satisfecho |
| 9 | verificación con `SupersededCommits` y Controller igual al Worker de esos commits | rechazo |
| 10 | commit anterior a la unidad hecho por la identidad del revisor | excluido del conjunto; no descalifica |
| 11 | el humano que operó una sesión IA autora actúa como revisor EXTERNAL HUMAN | rechazado |

### 14.6 Coherencia de `gate-contract/v2` y `delegation/v2`

| Regla | Comprueba |
|---|---|
| G1 | `RoleRequirements` es único por `Role`; `Materialization` no nula solo con `Role` = REVIEWER |
| G2 | `SupersededBaseSha` es nulo si y solo si `SupersededCommits` está vacío |
| G3 | `ProtocolSet` = el protocolo de la unidad (si no, P-15); un contrato `/v1` nunca se reemite como `/v2` ni se cita para acogerse a I62 |
| G4 | la delegación copia `RoleRequirements`: por rol, `Mandatory` ⊇ el del contrato y cada dimensión de `Independence` ≥ la del contrato |
| G5 | `SupersededCommits` de la delegación = el del contrato, exactamente |
| G6 | `Executor.BindingRef` cumple §14.4 y resuelve a un binding aceptado del rol del ejecutor; `Executor.Role` = el `Role` del binding |
| G7 | `Owner.Kind` = `session-internal` o `external-process` |

### 14.7 A1'-A8' y las comprobaciones de la verificación

**A1'-A8'** (AUTOMATION_PLAN 16.22), sin cortocircuito, sobre §4:
- **A1':** `Test-Json` de la delegación contra `schemas/delegation.v2.schema.json`, del contrato contra `gate-contract.v2` y del binding del ejecutor contra
  `binding.v1`; los hechos del adapter con las dos fases de §13.2;
- **A2':** lo de §4 + §14.4;
- **A3'-A5':** lo de §4 + G4 y G5;
- **A6':** lo de §4 + G3;
- **A7':** el binding del ejecutor está aceptado (§14.2 B1-B10; si es materializado, §14.3) y su preflight sigue vigente (§13.3);
- **A8':** lo de §4 + `CountersSnapshot` del binding = los contadores de la autoridad.

**Comprobaciones de la verificación.** Las 14 de AUTOMATION_PLAN 16.9 con los deltas de 16.22. Una comparación que no se ejecutó o no terminó es `not_run`, y
cuenta como fail. Para `Scope`:
1. la `Evidence` trae la salida reproducible de `git diff --name-only BaseSha..CurrentSha`;
2. por cada ruta, la entrada de `AllowedWriteScope` que la contiene (archivo exacto o prefijo terminado en `/`);
3. con `SupersededCommits`, además las rutas de esos commits frente al alcance acumulado.

Si falta 1 o 2, o la comparación auxiliar falló, `Scope` es `not_run`: nunca pass. El Coordinator repite la comparación antes de aceptar un VERIFIED y lo
rechaza si no coincide.

**Casos de cierre** (C-14; Proposal V14 Anexo G.2, filas que no necesitan `state/v2`):

| # | Caso | Esperado |
|---|---|---|
| 1 | misma entrega con dos verificaciones: la 1.ª BLOCKED/STOP S-04 resuelta sin cambio de trabajo; la 2.ª VERIFIED | `attempts` sin cambio; por clase sin cambio; BLOCKED/verificación = 1; invocaciones +2 |
| 2 | corrección lanzada sin entrega (el Worker cae) | `attempts` +1 y por clase(X) +1 en la corrección; después `Handoff` BLOCKED; reejecución = BLOCKED/WORK +1; por clase sin otro incremento |
| 3 | pérdida de contexto del Controller | S-12 → STOP; `analysis.md`; decisión del Coordinator; si reejecuta sin cambio, BLOCKED/fase +1; `attempts` sin cambio |
| 4 | `Routing` con efectivo ≠ solicitado | `required` → `Routing` fail → BLOCKED/BLOCKED; `advisory` → pass con la discrepancia anotada; puede ser VERIFIED |
| 5 | handoff de otra corrida, `Identity` pass | `FailureClass` = `Handoff`; REWORK_REQUIRED/REWORK |
| 6 | handoff de otra corrida, `Identity` fail | `FailureClass` = `Handoff` (primera en fail); STOP > REWORK → EXECUTION_BLOCKED/STOP |
| 7 | proceso terminado con turno fallido | `Outcome` FAILED_TURN → `Termination` fail → BLOCKED de transporte; sin veredicto de contenido |
| S | `Scope` = pass sin pertenencia por ruta, o con la comparación auxiliar fallida | `Scope` `not_run`, que cuenta como fail con la disposición de su fila (STOP): `EXECUTION_BLOCKED/STOP`; el VERIFIED se rechaza |

## 15. Invocación de rol, cierre de insumos y fidelidad (unidades I62)

Materializado por I-62 e **inactivo** hasta su vigencia. Reglas: AUTOMATION_PLAN 16.23 y 16.24.

### 15.1 Construcción de la `RoleInvocation`

Fase 1: `Test-Json` contra `schemas/role-invocation.v1.schema.json`. Después:

| Regla | Comprueba | Si falla |
|---|---|---|
| R1 | `OutputContract` = el de la pareja (`RequestedRole`, `Action`) en la correspondencia cerrada de AUTOMATION_PLAN 16.23; una pareja fuera de la tabla no se invoca | P-19 |
| R2 | `Target` no nulo salvo con `Action` = PLAN | rechazo |
| R3 | `Permissions` = READ_ONLY con ARCHITECT, EXECUTION_CONTROLLER y REVIEWER | rechazo |
| R4 | `Binding` cumple §14.4 y resuelve a un binding **aceptado** con el mismo `Role` = `RequestedRole`; sin binding elegible no hay invocación | P-10 |
| R5 | `EffectiveInputClosure` y `InputFidelityPreflight` resuelven a artefactos custodiados válidos (§15.2, §15.3) | rechazo (P-24 si es la fidelidad) |
| R6 | `ForbiddenInputs` no aparece en `CanonicalInputs` ni en `AllowedTransitiveInputs`: ni transcripción, ni memoria del autor, ni sesiones ajenas | rechazo |
| R7 | `AllowedActions` con `Class` = EXEMPTED trae `ExemptionRef` y `ExemptionScope` de esta invocación | rechazo |
| R8 | en una revisión mayor de LIFECYCLE, `IndependenceRequirements.ReviewSubject` no es nulo y el binding satisface su predicado (§14.5) | P-10 |
| R9 | la invocación no lleva texto de prompt: lo renderiza el adapter | rechazo |

### 15.2 Cierre efectivo de insumos

Se produce `rackcad-input-closure/v1` antes de lanzar, con el procedimiento de AUTOMATION_PLAN 16.24, y se custodia a más tardar en el punto durable que
reserva el intento. Las instrucciones automáticas de partida son las que declara el descriptor del adapter. Coherencia:

| Regla | Comprueba |
|---|---|
| I1 | READ ⇒ `Resolution` = INCLUDED y la ruta está en `AllowedTransitiveInputs` con su `RequiredBy` |
| I2 | ACTION_COMPATIBLE ⇒ ALLOWED y la acción está en `AllowedActions` |
| I3 | ACTION_INCOMPATIBLE ⇒ EXEMPTED con `Authority` no nula; sin exención, no hay lanzamiento (STOP, COORDINATOR_DECISION) |
| I4 | CONDITIONAL_NOT_TRIGGERED ⇒ NOT_TRIGGERED con `Reason`; una condición ambigua se clasifica READ |
| I5 | punto fijo: ningún archivo de `AllowedTransitiveInputs` tiene una obligación READ sin resolver |
| I6 | todo blob coincide con el de la `AuthorityRevision`; si cambia uno, el cierre se recalcula |

Tras la terminación, el invocador compara las lecturas registradas por el adapter con el cierre: una lectura fuera → P-22; sin registro, o incompleto →
Contexto UNKNOWN. La cobertura del registro se declara.

### 15.3 Preflight y comprobación de fidelidad

`rackcad-input-fidelity/v1` con `Kind` = FIDELITY (`Phase` PREFLIGHT antes de lanzar y POSTRUN tras la terminación), SPAN_MAP (tramos degradados) o
PREMISE_INDEPENDENCE (evidencia por premisa). Solo la sección del `Kind` es no nula. Coherencia:

| Regla | Comprueba |
|---|---|
| F1 | un registro por insumo del cierre y uno para `PROMPT` |
| F2 | exactamente uno de `TransportSha256` y `VerificationMethod` es no nulo |
| F3 | `CharacterClassesChecked` tiene una fila por cada carácter de `CorpusCharacters`, y FAITHFUL o FAITHFUL_NORMALIZED solo con `CountCanonical` = `CountTransport` en todas |
| F4 | `DegradedSpans` nulo con FAITHFUL o FAITHFUL_NORMALIZED, y no nulo con DEGRADED_BOUNDED |
| F5 | PREFLIGHT con algún registro distinto de FAITHFUL o FAITHFUL_NORMALIZED → no se lanza (P-24) |
| F6 | POSTRUN: la fuente de lo entregado al revisor está declarada (`DeliveredRepresentationSource`); sin ella, UNVERIFIED |
| F7 | PREMISE_INDEPENDENCE: una premisa es TRUE solo si todas sus unidades son visibles, ninguna solapa un tramo y `Resolutions` está vacío; con alguna resolución, UNKNOWN; con un solape, FALSE |

### 15.4 Validador del manifiesto de dependencias normativas

`rackcad-normative-dependency-manifest/v1` (`schemas/normative-dependency-manifest.v1.schema.json`). El de las superficies congeladas de I-62 es
[I-62-normative-dependency-manifest.json](../../initiatives/I-62-normative-dependency-manifest.json); lo genera de forma determinista el guion custodiado en
la evidencia de F3 y lo revisa el Architect como parte del objeto. El validador comprueba, sin cortocircuito:

| Regla | Comprueba |
|---|---|
| M1 | el manifiesto valida contra el esquema |
| M2 | cada `Source` es única por `Document` + `UnitId` |
| M3 | todo `Target` de tipo UNIT tiene su propia entrada (si no: entrada ausente) |
| M4 | toda unidad de un documento de la `AuthorityRevision` resuelve a exactamente un ancla de ese documento en su `Revision` (cero: destino colgante; más de una: referencia ambigua) |
| M5 | un `Target` de tipo PROPOSED es una fila de Proposal V14 §3.1 |
| M6 | un `Target` de tipo WHOLE_DOCUMENT trae un conjunto de entrada no vacío o una regla compuesta |
| M7 | exactamente una de `Unit`, `Proposed` y `WholeDocument` es no nula, la que indica `Kind` |
| M8 | cada `Document` está en `Scope` y cada `Revision` es la de la `AuthorityRevision` |

Una unidad sin entrada, o con `Complete` = false, tiene metadatos incompletos: la independencia de la premisa que la necesita es UNKNOWN. La clausura de una
corrida concreta (`NormativeDependencyClosure`) la calcula y la custodia el invocador.

## 16. Validación de los resultados por rol (unidades I62)

Materializado por I-62 e **inactivo** hasta su vigencia. Regla: AUTOMATION_PLAN 16.23. El resultado se valida contra el esquema de su `OutputContract`
(`schemas/architect-review-result.v1.schema.json` o `reviewer-result.v1.schema.json`) y después:

| Regla | Comprueba | Si falla |
|---|---|---|
| V1 | el esquema del resultado = el `OutputContract` de su invocación; `RequestedRole` y `Action` = los de la invocación y los del binding | P-19 |
| V2 | `InvocationId`, `ReviewedCommit`, `ReviewedPath` y `ReviewedBlob` = los de la invocación y su `Target` | P-19 |
| V3 | `InjectedContextDeclaration.State` = DECLARED, con un elemento por cada `DeclaredRuntimeContext` de la invocación (sin elementos solo si la invocación no declara ninguno); NONE_DECLARED es la falta de declaración. `IndependenceEvidence` presente | P-19 |
| V4 | ARCHITECT: cada REQUIRED trae `FindingId`, `AffectedSection`, `Evidence` y `CorrectionRequired` | P-19 |
| V5 | ARCHITECT: AGREED solo sin REQUIRED nuevos y con cada REQUIRED abierto en CLOSED o SUPERSEDED en `FindingDispositions` | P-19 |
| V6 | ARCHITECT: CHANGES REQUIRED con algún REQUIRED abierto; BLOCKED — OWNER DECISION con `OwnerDecisions` no vacío | P-19 |
| V7 | `FindingDispositions[].EvaluatedObject` = el objeto revisado | P-19 |
| V8 | `ReviewerDeclaredIdentity` no contradice la identidad observada por el invocador (una declaración UNKNOWN no contradice) | P-23 (S-04) |
| V9 | REVIEWER: sin veredicto de LIFECYCLE (el esquema no admite `Verdict`), y sus disposiciones solo afectan a linajes del REVIEWER | P-19 |
| V10 | un resultado de REVIEWER presentado para satisfacer al ARCHITECT o para cerrar un hallazgo del ARCHITECT | P-20 |
| V11 | ninguna lectura fuera del cierre (§15.2) y la fidelidad de §15.3 para lo afectado | P-22, P-25 |

**Casos de control** (C-30; esperado exacto): (a) binding de la misma sesión que el autor → rechazo (P-10); (b) insumos con la transcripción del autor →
rechazo (R6); (c) sin declaración de contexto inyectado → rechazo (P-19); (d) binding válido con el predicado de §14.5 → aceptado; (e) `OutputContract`
cruzado (REVIEWER con `architect-review-result/v1`, o ARCHITECT con `reviewer-result/v1`) → rechazo (P-19); (f) sin `EffectiveInputClosure` → rechazo;
(g) PLAN con `controller-verification/v2` y (h) VERIFY con `delegation/v2` → INVALID (P-19); (i) PLAN con `delegation/v2` y VERIFY con
`controller-verification/v2` → aceptadas.
