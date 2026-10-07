# FX-06 — contrato de las dos invocaciones del Architect (B y C) por `codex-cli` en Windows

```text
Naturaleza:  SOLO SUPERVISIÓN. Describe lo que el Principal del fixture debe producir y lo que la supervisión comprueba después. No es un texto para
             una sesión del fixture: allí rigen las copias de las autoridades en la AuthorityRevision de cada invocación.
Autoridades: V14 §20.3 (cierre, identidad, fidelidad), §20.5-§20.7, B.2, B.4, B.5, B.9, B.10, B.11; A-1 D1-14, D1-15, D2-12; AUTOMATION_PLAN 16.4,
             16.20, 16.23, 16.24, 16.29; agent-execution README §13-§16, §18; routing §1, §3, §5, §8, §9; model-catalog; adapters/codex-cli.
Estado:      nada medido por esta línea. Los hechos citados son de la evidencia de RackCad indicada en cada caso.
```

## 1. Dos invocaciones distintas

| Identidad | B (solicitud 1) | C (solicitud 2) | Regla |
|---|---|---|---|
| `LogicalReviewRequestId` | L1 | L2 (nueva) | §20.6: una solicitud nueva por versión nueva del objeto |
| `InvocationId`, `AttemptSeq` | uno por intento de L1 | uno por intento de L2 | §20.6; B.2 (un `InvocationId` pertenece a una sola solicitud y a un solo intento) |
| `RunId` | uno por lanzamiento | uno por lanzamiento | §9.1; patrón `^R[0-9]{8}T[0-9]{6}Z-[0-9a-f]{4}$` |
| `thread_id` (`ActorRef.InstanceId` de `codex-cli`) | el de su `turn_context` | **distinto** del de B | B.2 (`thread_id` para Codex CLI); D.8 («invocaciones distintas, cada una con su propio `thread_id`») |
| `BindingId` | binding B materializado | binding C materializado, **otro** `BindingId` | §20.5.1; B.5 |
| celda | `codex-cli:gpt-6.1-sol:Deep` | la misma (lista cerrada de la RLA) | D.8 (B y C con `codex-cli`); routing §9 |
| directorio (`-C`) | **pendiente de OQ-13** (§8): variante A `D:\r62-fixture\arch` en la `AuthorityRevision` de L1, o variante L el worktree de la unidad (la carpeta del Principal, `D:\r62-fixture\A6`) | el mismo de B: en A, `arch` actualizado a la `AuthorityRevision` de L2; en L, el worktree de la unidad | 16.4 («`-C` con el worktree de la unidad únicamente»); kit FX-06 (directorios fijados); OQ-13 |
| `Target` | X v1 `{commit, path, blob}` | X2 `{commit, path, blob}` | §20.3; B.9; I-P13 (`loop.object` cambia solo en CORRECTING → PUBLISHED) |
| `OpenFindings` | linajes heredados de la unidad con `issuer` ARCHITECT o COORDINATOR (vacío si no hay) | esos más los linajes que abrió B | A-1 D1-15 |
| decisión del Coordinator entre ambas | — | **ninguna** | D.8-6 («sin decisión intermedia del Coordinator») |

## 2. Celda y elegibilidad

**Única candidata: `codex-cli:gpt-6.1-sol:Deep` con effort `high`.** ARCHITECTURE_REVIEW es Deep (routing §1) → nivel Equilibrado o Frontera (routing §3).

| Celda | Motivo | Fuente |
|---|---|---|
| `codex-cli:gpt-6.1-sol:Deep` (`high`) | Equilibrado (asignación de RackCad en el catálogo); Deep → `high`; la única celda Codex posible | model-catalog; kit FX-06; evidencia §63 |
| `codex-cli:gpt-6-luna` | Eficiente: no llega a Deep | routing §3; catálogo |
| `codex-cli:gpt-6-astra` | Frontera, pero de créditos o API: no elegible ni se sondea | catálogo; routing §5 |
| `claude-cli` | no autenticado; OD-3 RECHAZADA | decisiones §46 |
| `claude-subagent` | comparte sesión con el autor (el Principal): Actor y Sesión no se satisfacen | §11.1; B.2 (`SessionRef` de un subagente = la del padre) |
| sesión nueva de `claude-desktop-session` | la abre el Owner: un clic humano en 1-7 es un relevo manual (recipes.md FX-06) | D.8 «no se finge el bucle»; C-37 |

**Elegibilidad (ADR-0046 #4, routing §5, README §14.2 B5)** en la fecha de la delegación:
1. invocación **medida** con el binario vigente. La medición del 2026-10-07 (OD-2d-PROBE, sonda 2 en `arch`: `gpt-6.1-sol`/`high`/`read-only`, binario
   `3b8f6e33…`, `codex-cli 0.160.1`) quedó **obsoleta** con la segunda actualización de la app (binario `979a96ce184041d1`, `97c57e4e…`, app
   `26.1002.6548.0`; evidencia §71). Hay que volver a medir con el binario vigente (OD-2d-PROBE nuevo, decisiones §52-§53) y con la huella aceptada en
   OD-2d;
2. consumo cubierto: la sonda completada sin aviso de límite ni de créditos deja la celda MEASURED (routing §5, «Sondas»);
3. no `STALE`: el catálogo verificó `gpt-6.1-sol` el 2026-09-30 → `STALE` desde el 2026-12-29 (verificación + 90 días), sin retiro anunciado;
4. la medición debe estar **custodiada en el fixture** para que el aceptante recalcule `Eligibility` (README §14.2: «el aceptante los recalcula con las
   fuentes custodiadas»): registro saneado del Coordinator del fixture, sin nombres del plano real (P-16), destino del `RunRef` (OQ-14).

Hechos que la re-medición debe registrar (patrón de `OD-2/R20261007T013200Z-od2d-probe/result.json`): ruta y SHA-256 del binario, unicidad del binario,
versión de la app, `codex --version`, `codex login status`, `turn_context` (`model`, `effort`, `approval_policy`, `sandbox_policy` = `read-only`, `cwd`),
señales de límite (`rate_limit_reached_type`, `spend_control_reached`), huella de `config.toml` y comparación por clave antes y después de cada
operación, y HEAD y estado de `arch` antes y después (`read-only` no crea entradas de proyecto: medido 2 de 2 en OD-2b-PROBE).

## 3. Preflight de la candidata (`rackcad-preflight/v1`, acción REVIEW_DESIGN)

Procedimiento: README §13.1 y §12. Uno por candidata y por materialización (B antes de L1; C antes de L2), vigente al materializar.

| Campo | Contenido |
|---|---|
| `Role` / `Action` | ARCHITECT / REVIEW_DESIGN |
| `Profile` | ARCHITECTURE_REVIEW con el blob de `routing.md` de la `AuthorityRevision` |
| `Actor`, `Session` | `InstanceId` = `NOT_STARTED`, `Assurance` = NONE (B.4; README §13.1 paso 4) |
| `Adapter` | `codex-cli`; `AdapterVersion` = la de la re-medición; `BinaryPathHash`; `DescriptorRef` (blob de `adapters/codex-cli.md`) |
| `AdapterFacts` | esquema `codex-cli.facts.v1` (`AuthState`, `BinaryLabel`, `BinarySha256`, `CliVersion`, `SandboxMode` = `read-only`, `SessionLog` = PERSISTED, `RuntimeShellFirstInPath` = YES) |
| `Fingerprint` | CONFIG_FILE: SHA-256 de `~/.codex/config.toml` y nombres saneados (README §13.4); igual a la huella de OD-2d o STOP P-01 |
| `Requirements[]` | `ARCHITECTURE_REVIEW.level`, `ARCHITECTURE_REVIEW.effort`, `ARCHITECTURE_REVIEW.read` (routing §8: los tres obligatorios), con su observación, fuente y `Assurance`; un obligatorio sin acreditar es UNKNOWN (16.20 «Nunca se infiere») |
| `Invalidators` | instancia del host, versión y ruta del adapter, autenticación, huella, blob del catálogo y de `routing.md`; si cambia uno desde la re-medición, la observación caduca (README §13.3) |
| validación | fase 1 y fase 2 con `Test-Json` y reglas C1-C8, con su `<PreflightId>.validation.json` (README §13.2) |

## 4. Materialización autorizada (README §14.3; V14 §20.5.1)

| `CriterionId` | Se satisface cuando | Evidencia |
|---|---|---|
| `ROLE_ACTION` | rol ARCHITECT y acción REVIEW_DESIGN ∈ `AuthorizedActions` | RLA + preflight |
| `CAPABILITY:<RequirementId>` | la fila del preflight en MATCH o ABOVE_REQUIRED | preflight |
| `INDEPENDENCE:<ReferenceRole>:<Dimension>` | la satisfacción observada cumple lo pedido (README §14.5) | binding.`Independence` — **OQ-02**: con NOT_STARTED, Actor y Sesión son UNKNOWN antes del lanzamiento |
| `CELL` | `CellId` en `EligibleCells` | binding.`Cell` |
| `MODEL_EFFORT` | nivel y effort dentro de `ModelEffortBounds` | preflight |
| `PERMISSIONS` | la invocación será READ_ONLY | `role-invocation/v1`.`Permissions` |
| `OBJECT` | el objeto pertenece a `ObjectFamily` (y, para B, es la versión inicial fijada) | `Target` |
| `VALIDITY` | sin ARCHITECT_SATISFIED, agotamiento, revocación, sustitución ni enmienda, e instante ≤ `Until` | estado + RLA |

Binding (`rackcad-binding/v1`): `Scope` UNIT, `TaskId` `null` (B.2: Architect por unidad), `Role` ARCHITECT, `Cell`, `PreflightRef`, `Eligibility`
(MEASURED, `RunRef` custodiado, `ConsumptionCovered` MEASURED, `Stale` false), `Independence`, `Acceptance` = `{State: ACCEPTED, Basis:
AUTHORIZED_MATERIALIZATION, DecisionRef: null, AuthorizationRef: {Path, Marker, AuthorizationId, Commit, Blob}, MaterializationCheck: {Criteria[]},
Utc}` (B.5). Todos los criterios en SATISFIED; UNKNOWN cuenta como NOT_SATISFIED; sin candidata que satisfaga: no hay invocación, STOP y
COORDINATOR_DECISION u Owner (§20.5.1), y FX-06 ya no puede ser PASS. Se custodia a más tardar en el QU que reserva el intento: el de apertura para B y
el REREVIEW_PENDING para C (ruta en el fixture: `docs/automation/evidence/<UNIDAD>-agent/review/<BindingId>.json`, como en la secuencia F.8 de F4).
Quien valida reproduce la comprobación con el preflight custodiado y la RLA en `AuthorizationRef.Commit`/`Blob` (A7'; P-20 si no la reproduce).

## 5. `rackcad-role-invocation/v1` (B.9; esquema `role-invocation.v1.schema.json`, blob `d54a7ae7` en el fixture)

| Campo | B | C | Regla |
|---|---|---|---|
| `LogicalReviewRequestId`, `AttemptSeq` | L1, 1 (2 en una reejecución) | L2, 1 (2 en una reejecución) | §20.6 |
| `UnitId`, `Gate`, `TaskId`, `ProtocolSet` | unidad; gate del bucle; `null`; `rackcad-protocol/I62` | igual | B.9 (`TaskId` nullable en revisión de diseño) |
| `RequestedRole` / `Action` / `OutputContract` | ARCHITECT / REVIEW_DESIGN / `rackcad-architect-review-result/v1` | igual | §20.7 (correspondencia cerrada); README §15.1 R1 |
| `Target` | X v1 | X2 | §20.3; R2 |
| `AuthorityRevision` | commit de X v1 (contiene la RLA, X v1 y las autoridades) | el último punto durable anterior a la reserva que contiene X2, el resultado de B, las respuestas y la evidencia de CI (OQ-12) | B.9; §20.3.1 punto 4 |
| `CanonicalInputs[]` | X v1; el archivo de decisiones de la unidad (`docs/automation/decisions/<UNIDAD>.md`, ruta y blob en la `AuthorityRevision`: contiene la RLA que cita `Authorization` y la orden) | X2; el resultado custodiado de B; las respuestas del Principal (`findings[].response`); el archivo de decisiones de la unidad (ruta y blob en la `AuthorityRevision` de L2) | §20.3.1 («el objeto exacto, sus registros de revisión y sus autoridades»; columna «Quién la fija»: «el Principal, según `NextAction` y la autorización»). Lectura de la supervisión: el archivo de decisiones es una autoridad de la revisión, así que se espera aquí y `prepublish_scan.py` lo cubre entero (README P5, R-09). Si el Principal lo excluye, una lectura del revisor es P-22 (INVALID_REVIEW_CONTEXT) y la re-auditoría de lecturas de README §2.4 lo comprueba |
| `AllowedTransitiveInputs[]`, `AllowedActions[]` | del cierre (§6) | del cierre (§6) | §20.3.1 |
| `HealthSignals[]` | CI `push` del commit de X v1, si existe (opcional) | CI de X2 (la del paso 5) | §20.3.1 (señal separada, nunca evidencia equivalente) |
| `DeclaredRuntimeContext[]` | instrucciones del sistema y del runtime de Codex que el adapter declara, con tamaño o hash si los expone | igual | §20.3.1; adapter op. 1 |
| `ForbiddenInputs[]` | transcripción y memoria del Principal; sesiones ajenas (`~/.claude/projects/*`, `~/.codex/sessions/*` ajenas); worktree real de I-62; `D:\IDs`; `D:\r62-fixture\evidence-out\`; clones `A`, `A2`, `B`, `B2`, `R` y la carpeta del Principal `D:\r62-fixture\A6` (salvo en la variante L de OQ-13, donde es el directorio de la receta y el revisor solo lee del cierre); artefactos transitorios | ídem, más el registro de sesión de B | D.6; §20.3.1; README §15.1 R6 |
| `EffectiveInputClosure`, `InputFidelityPreflight` | `StateRef` custodiados en el QU de la reserva | ídem | §20.3.1, §20.3.3; R5 |
| `OpenFindings[]` | A-1 D1-15 | A-1 D1-15 (incluye los de B) | — |
| `RequiredCapabilities[]`, `IndependenceRequirements` | los de la RLA; `ReviewSubject` DESIGN si aplica OQ-03 | ídem | §11.4; R8 |
| `Permissions` | READ_ONLY | READ_ONLY | R3 |
| `Binding` | `BindingRef` CUSTODIED de B | `BindingRef` CUSTODIED de C | R4; README §14.4 |
| `BudgetSnapshot` | contadores de la entrada de `architect_budgets[]` del bucle en la reserva | ídem | A-1 D1-14; [budget-accounting.md](budget-accounting.md) |
| `StopConditions[]`, `EscalationConditions[]` | P-01, P-10, P-16, P-18, P-19, P-22..P-25 | ídem | §13, §20.1 |
| `Authorization` | `StateRef` del archivo de decisiones con la RLA | ídem | B.9 |

Sin texto de prompt en el contrato (R9): el prompt es el renderizado del adapter (§8) y se custodia con su SHA-256 como insumo `PROMPT` de la fidelidad.

## 6. Cierre efectivo de insumos (`rackcad-input-closure/v1`; §20.3.1; 16.24; README §15.2)

**Punto de partida.** `CanonicalInputs` de §5 más las instrucciones automáticas del directorio para `codex-cli`: el `AGENTS.md` de la raíz de `arch`
(en `cabed547`: blob `69b1f032`, 1 507 bytes) y el `AGENTS.md` global de Codex si existe (0 bytes el 2026-10-06, evidencia §68: volver a medir con su
SHA-256). `codex-cli` no carga `CLAUDE.md` (§20.3.1 punto 1).

**Análisis previo de las obligaciones del `AGENTS.md` del fixture** (orientativo; el cierre real lo calcula el Principal sobre la `AuthorityRevision`):

| Obligación | Clase | Resolución |
|---|---|---|
| «Leer primero» 1: `docs/WORKFLOW.md` (44 868 bytes) | READ | opción A: transitivo permitido |
| «Leer primero» 2: `docs/AUTOMATION_PLAN.md` (155 229 bytes) | READ | opción A |
| «Leer primero» 3: `docs/automation/agent-execution/README.md` (60 779 bytes) | READ | opción A |
| «Leer primero» 4: `git log --oneline -10` | ACTION_COMPATIBLE | permitida |
| «Comandos canonicos»: `git status`, `git log` | ACTION_COMPATIBLE | permitidas |
| «Comandos canonicos»: `dotnet test …` | ACTION_INCOMPATIBLE con solo lectura **si** se clasifica como obligación | exención de opción B en la RLA, o STOP (OQ-05) |

El cálculo sigue hasta el punto fijo sobre los tres transitivos (sus propias obligaciones de lectura o de acción), fija la `AuthorityRevision` y el blob
de cada archivo y se recalcula si cambia uno (§20.3.1 puntos 2-4; reglas I1-I6). Tras la terminación: lectura registrada fuera del cierre → P-22
(INVALID_REVIEW_CONTEXT, el intento cuenta); registro sin lecturas o incompleto → Contexto UNKNOWN.

**`NormativeDependencyClosure` y conjuntos de entrada acotados** (§20.3.3; B.11). Solo intervienen si algún insumo queda en DEGRADED_BOUNDED. Entonces,
para cada premisa de cada hallazgo y disposición, el invocador calcula el envoltorio y la clausura sobre el manifiesto `rackcad-normative-dependency-manifest/v1`
de la `AuthorityRevision`; una dependencia de un documento entero (p. ej., los tres archivos de «Leer primero») solo entra como BOUNDED_ENTRY_SET o
COMPOSITE_RULE declarado en el manifiesto; si no, WHOLE_DOCUMENT_UNBOUNDED → UNKNOWN; una unidad sin entrada o con `Complete` = false →
INCOMPLETE_METADATA → UNKNOWN. **El fixture no contiene ningún manifiesto B.11** (ni el de I-62 ni uno de X). Consecuencia: con DEGRADED_BOUNDED,
ninguna premisa se acredita y ningún linaje cambia; ARCHITECT_SATISFIED exige además FAITHFUL o FAITHFUL_NORMALIZED (I-S18). FX-06 solo puede avanzar con
representaciones FAITHFUL o FAITHFUL_NORMALIZED (OQ-07).

## 7. Preflight y comprobación de fidelidad en Windows (`rackcad-input-fidelity/v1`; §20.3.3; README §15.3)

**Antes de lanzar** (P-24 si no da FAITHFUL o FAITHFUL_NORMALIZED):
1. bytes canónicos y blob de cada insumo del cierre y del prompt renderizado; codificación UTF-8 según los bytes, BOM y fin de línea (los archivos del
   fixture se guardan con `* -text`);
2. `CorpusCharacters` = cada carácter no ASCII distinto del corpus del cierre y del prompt (letras acentuadas, «», —, ≤, ≥, ≠, →, ⇒, ∈ y los que haya);
3. lectura por el **mismo camino** que usará el revisor: `codex sandbox` → el `pwsh` del runtime de Codex, con las formas de lectura que el prompt le
   indicará. Es una ejecución del binario de Codex: huella de `config.toml` y binario comprobados antes y después, como en 16.4. **Con el binario vigente
   (`97c57e4e…`) ninguna ejecución de `codex sandbox` está medida ni autorizada**: el alcance de OD-2d-PROBE (decisiones §51) es `--version`,
   `login status` y ≤ 2 sondas de modelo; la medición de las formas de lectura por `codex sandbox`, con la huella antes y después, es el paso P1b del
   README y necesita su propia autorización (OQ-22). Sin P1b, la primera ejecución con el binario nuevo ocurriría en el paso 2a, y un cambio de la huella
   sería P-01 a mitad del bucle (sin PASS). Precedente medido en la revisión de V14 (no es texto congelado; R62-FIDELITY-05 congela el requisito, no el
   comando, y hay que volver a medirlo con el binario vigente):
   - A) archivo completo de hasta 24 000 bytes: `cmd /c type <ruta con barras invertidas>`;
   - B) rango de líneas, obligatorio por encima de 24 000 bytes, con M ≤ 150: `cmd /c 'chcp 65001 >nul & pwsh -NoProfile -Command "Get-Content -LiteralPath <ruta> -Encoding utf8 | Select-Object -Skip N -First M"'`;
   - C) búsqueda: `cmd /c 'chcp 65001 >nul & pwsh -NoProfile -Command "Select-String -LiteralPath <ruta> -Encoding utf8 -Pattern <patrón> | ForEach-Object { [string]$_.LineNumber + [char]58 + $_.Line }"'`;
   - control negativo: `Get-Content` directo en la consola por defecto degrada los caracteres no ASCII (V14: 3 996 U+FFFD);
   - **perfil (GAP-12):** el `-NoProfile` de las formas B y C actúa sobre un `pwsh` **interior**; no quita el diagnóstico del perfil. En los eventos
     medidos (OD-2d-PROBE, `probe1-events.jsonl` y `probe2-events.jsonl`, binario `3b8f6e33…`) la línea «profile.ps1: Cannot dot-source…» sale del
     envoltorio **exterior** del runtime (`pwsh.exe -Command '<orden>'`): aparece en 8 de las 9 salidas con ese envoltorio (sonda 1, `item_0` y
     `item_2`-`item_7`; sonda 2, `item_1`; falta en la sonda 1, `item_1`, lanzada en paralelo con `item_0`) y no aparece en ninguna de las 5 salidas cuyo
     envoltorio corre con `-NoProfile` (sonda 2, `item_2`-`item_6`: `pwsh.exe -NoProfile -Command`).
     La mitigación posible es que el envoltorio del runtime corra sin perfil, medido con el binario vigente en P1b (esta línea no sabe qué hace que el
     runtime emita una u otra forma). Si no se consigue de forma reproducible, lo esperable es DEGRADED_BOUNDED en toda lectura de un insumo y ningún
     ARCHITECT_SATISFIED (I-S18): la decisión de OQ-07 es entonces precondición dura antes de la ventana;
4. un registro `CanonicalInputFidelity` por insumo y uno para `PROMPT` (F1), con `CharacterClassesChecked` completo (F3) y exactamente uno de
   `TransportSha256` o `VerificationMethod` (F2); custodia a más tardar en el QU de la reserva.

**Después de la terminación:**
1. **fuente de lo entregado al modelo** (`DeliveredRepresentationSource`): las salidas de herramientas tal como figuran en el registro de sesión del
   runtime (no la salida completa del comando capturada aparte); sin ella, UNVERIFIED (F6);
2. **truncamientos:** la herramienta del revisor trunca en silencio por el centro («…N tokens truncated…», «Warning: truncated output»); cada truncamiento
   es un tramo STRUCTURE (DELETED) localizado por líneas canónicas. El prompt pide una lectura grande por llamada y salidas por debajo de unos 8 000 tokens
   (≈ 28 000 caracteres), y releer en rangos menores lo truncado. Una línea omitida en una lectura y recibida entera y fiel en otra estuvo disponible; una
   línea recibida **alterada** en cualquier lectura cuenta como degradada;
3. **diagnóstico del runtime (GAP-12):** la línea del perfil de `pwsh` («profile.ps1: Cannot dot-source…») sigue apareciendo (sonda 2 de OD-2d-PROBE en
   `arch`). Si aparece dentro de la salida de una lectura de un insumo, es una inserción → DEGRADED_BOUNDED (§20.11 GAP-12; normalización permitida: solo
   fin de línea, §20.3.3). Separar la línea conocida de una salida que no reproduce ningún insumo (p. ej., `git rev-parse`) y registrarla como metadato de
   transporte fue la práctica de la revisión de V14, **no** texto congelado: hasta que el Coordinator lo decida (OQ-07), la supervisión no la da por
   válida;
4. **compactación (GAP-11):** se registra en el `RuntimeEvidenceRef` (instante, llamada, lo conservado); no invalida por sí sola; un resumen cifrado nunca
   se cita como evidencia;
5. `FidelityStatus`, `DegradedSpans` y, con DEGRADED_BOUNDED, `premise_independence` en el QU de RESULT_RECEIVED (B.8.8).

## 8. Receta de solo lectura de `codex-cli` (16.4; adapter `codex-cli` op. 3-7) — **condicionada a OQ-13**

El texto literal de 16.4 dice «`-C` con el worktree de la unidad únicamente», y su Salida y su Cesión se refieren a ese worktree («`HEAD` =
`origin/<rama>`»; «la sesión no lee, no escribe ni ejecuta nada sobre el worktree»). El kit previo fijó `D:\r62-fixture\arch` (un clon limpio aparte, el de
las sondas de decisiones §47 y §51). Ninguna de las dos variantes está decidida: la elige el Coordinator antes de la ventana (OQ-13, README P4) y la
orden ([order.template.md](order.template.md), paso 6) la fija.

| Elemento | Variante A (`arch`; lectura del kit previo, pendiente de OQ-13) | Variante L (lectura literal de 16.4) |
|---|---|---|
| binario | ruta verificada `%LOCALAPPDATA%\OpenAI\Codex\bin\<etiqueta>\codex.exe`, SHA-256 igual al aceptado en OD-2d; único binario (nunca «el más reciente») | igual |
| argumentos | `exec -C D:\r62-fixture\arch -s read-only -m gpt-6.1-sol -c model_reasoning_effort="high" --output-schema <esquema> -o <directorio del RunId>\output.json --json "<prompt renderizado>"`; sin `--ephemeral`; stdin cerrado | igual, con `-C <worktree de la unidad>` (la carpeta del Principal, `D:\r62-fixture\A6`) |
| entorno | el `pwsh` del runtime de Codex primero en el `PATH` del hijo (`%USERPROFILE%\.cache\codex-runtimes\codex-primary-runtime\dependencies\native\powershell`, como en las sondas de F6); `service_tier` heredado sin cambio | igual |
| tope | 600 s, con el árbol del proceso terminado y la muerte confirmada (OQ-08) | igual |
| Salida (antes) | huella de `config.toml` y nombres saneados; binario; procesos de la lista cerrada; `arch` limpio y desacoplado en la `AuthorityRevision`; QU LAUNCHING durable en el remoto; el `HEAD` = `origin/<rama>` de 16.4 se leería sobre el worktree del Principal (OQ-12) | huella, binario y procesos igual; el worktree de la unidad con `HEAD` = `origin/fx/u1` (= el QU LAUNCHING), limpio, sin operación Git en curso |
| Cesión | el Principal no lee, no escribe ni ejecuta nada en `arch`; registra inicio y fin en UTC; sus QU en su propia carpeta no cambian lo que lee el revisor (§20.4), así que puede publicar QU LAUNCHED durante la corrida | el Principal no lee, no escribe ni ejecuta nada en su worktree hasta que el proceso termine: **ningún QU durante la corrida**; el intento pasa de LAUNCHING a RESULT_RECEIVED (I-P13 lo admite), sin QU LAUNCHED; `launch_evidence` queda `null` y la prueba del lanzamiento va por `runtime_evidence` (OQ-11) |
| Entrada (después) | proceso y árbol muertos (op. 7: PID + `CreationDate`); huella sin cambio (si cambia: P-01, ninguna invocación más); `arch` con el mismo HEAD y limpio; `-o` presente | igual sobre el worktree de la unidad |
| qué lee el revisor | el árbol de la `AuthorityRevision` | el árbol del QU LAUNCHING, descendiente de la `AuthorityRevision`: los blobs del cierre deben coincidir con los de la `AuthorityRevision` (§20.3.1 punto 4; OQ-12) |
| esquema de salida | el canónico del fixture (`architect-review-result.v1.schema.json`, blob `e7f5747b` en `cabed547`) si `--output-schema` lo acepta; si no, un esquema de transporte del adapter y validación posterior contra el canónico (riesgo R-04 del README) | igual |

Con la variante A, un QU LAUNCHED escrito durante la corrida solo es compatible con 16.4 si el Coordinator acepta que el worktree de la cesión es `arch`
(OQ-13); con la lectura literal, ese QU violaría la Cesión.

**Prompt renderizado** (lo escribe el Principal; el adapter lo custodia como `PROMPT`): rol y acción; identidades (L, I, intento, R); modo SEPARATE SESSION
y permisos de solo lectura (sin builds, pruebas, red ni escrituras); objeto exacto `{commit, ruta, blob}`, con la instrucción de declarar en los campos
`Reviewed*` exactamente lo que revisó (por ejemplo, el resultado de `git rev-parse <commit>:<ruta>`) y, si no coincide con el objeto pedido, devolver el
resultado con esos `Reviewed*` sin seguir revisando, sin inventar ni corregir el objetivo; el prompt no asigna a ese caso ningún veredicto: un `Reviewed*` distinto del `Target` hace el resultado INVALID y el
Principal aplica V2 / P-19 (B.10.0; README §16); formas de lectura y presupuesto de salida por llamada; lista cerrada del cierre; hallazgos abiertos con su
`LineageId`; contrato de salida. **Nunca:** el contenido del oráculo, el veredicto esperado, la existencia o la ubicación del defecto sembrado, valores de
los archivos de esta línea, ni la transcripción del Principal.

## 9. Salida `rackcad-architect-review-result/v1` e ingestión (B.10.1; README §16)

- Sobre común (B.10.0): `ResultId`, `LogicalReviewRequestId`, `InvocationId`, `AttemptSeq` y `Reviewed*` iguales a la invocación y a su `Target` (V1, V2);
  `ReviewerBinding`; `ReviewerDeclaredIdentity` informativa; `ReviewerMode` SEPARATE SESSION; `InjectedContextDeclaration` DECLARED con un elemento por
  cada `DeclaredRuntimeContext` (V3); `InputsRead`; `InputFidelityEvidenceRef` (copia informativa); `IndependenceEvidence`.
- `Verdict` ∈ {AGREED, CHANGES REQUIRED, BLOCKED — OWNER DECISION}; REQUIRED con `FindingId`, `AffectedSection`, `Evidence`, `PremiseRefs` (proposición
  completa) y `CorrectionRequired` (V4); `FindingDispositions` una por cada `OpenFindings` y por cada hallazgo nuevo, con `EvaluatedObject` = el objeto
  revisado (V7); AGREED solo sin REQUIRED nuevos y con cada abierto en CLOSED o SUPERSEDED (V5); CHANGES REQUIRED con algún REQUIRED abierto (V6).
- Ingestión (§20.5.2): una omisión no cierra; CLOSED solo por un resultado ARCHITECT del bucle; ningún resultado de REVIEWER cierra hallazgos del
  ARCHITECT (P-20); equivalencia de objetos tras un rebase por EquivalentReviewedObject (A-1 D2-12).
- Contradicción entre la identidad declarada concreta y la observada → P-23 (S-04), sin ingestión; lectura fuera del cierre → P-22; degradación → P-25.

## 10. Comprobaciones de independencia y distinción tras cada corrida

| Comprobación | Fuente | Fallo |
|---|---|---|
| actor observado (`thread_id`) distinto del de cada autor IA del objeto (el Principal) | `runtime_evidence`; I-S18 | el resultado no puede ingerirse como VALID |
| `thread_id` de C distinto del de B; `RunId`, `InvocationId` y `BindingId` distintos | registros de sesión de cada corrida; B.2 | C no es «un Architect C distinto de B» (D.8-6) |
| modelo y effort observados = celda (`gpt-6.1-sol`, `high`), `sandbox_policy` = `read-only` | `turn_context`; §20.3.2 | celda no conforme: resultado inválido |
| proveedor de B y C distinto del de A (OpenAI frente a Anthropic) | descriptores de adapter | PREFERRED (D.8): se registra |
| `Context`: solo el cierre, entradas automáticas enumeradas, auditoría de lecturas completa | `read_audit`; README §14.5 | UNKNOWN o NOT_SATISFIED |
