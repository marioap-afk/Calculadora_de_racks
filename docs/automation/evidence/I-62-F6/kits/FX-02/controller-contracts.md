# FX-02 — contratos de las invocaciones del Controller (`codex-cli`, solo lectura)

> **Preparación; solo supervisión.** Describe lo que A2 debe construir y lo que el Controller debe devolver, con los campos de los esquemas del
> conjunto I62 (copias byte a byte en el fixture). No contiene valores esperados de ningún resultado. Bloqueado por **F-A2** (A-2 AGREED: el tope
> congelado de sondas está agotado, decisiones §55 U-04), **F-OD-PROBE** (bloque nuevo de medición) y **F-OD-2D** (no hay invocación de `codex-cli`
> posible antes), por CD-26 (OQ-33: ninguna escritura de FX-02 en el origen antes de esa disposición; F-UP-FX04A quedó liberada por decisiones §55
> U-01c) y por CD-03, CD-04, CD-07, CD-09, CD-14, CD-19, CD-20, CD-22, CD-23, CD-27 y CD-28 (`frontiers.json`; salvo CD-07, todas son disposiciones
> del Coordinator de I-62, grupo b). CD-10 está **DECIDIDA EN PARTE** (decisiones §55: U-03, `-C` del Controller = `D:\r62-fixture\A2`; U-04, tope de
> sondas); los directorios de las sondas del bloque nuevo siguen pendientes (CD-28 / OQ-35 del README).

Autoridades: AP 16.4 (receta y relevo), 16.5/16.22 (A1'-A8'), 16.9/16.22 (14 comprobaciones), 16.18-16.21 (preflight, adapters, binding,
independencia), 16.23-16.24 (invocación de rol, cierre, fidelidad); V14 B.7, B.9; RAE §3, §5, §8, §10, §13-§15; routing §1, §3, §5, §8, §9; descriptor
`adapters/codex-cli.md` (`155f3469`); esquemas (blobs del worktree en `707b4daa`, iguales en el fixture): `role-invocation.v1` `d54a7ae7`,
`input-closure.v1` `6bdf7184`, `input-fidelity.v1` `e54990fc`, `delegation.v2` `dfdbe461`, `controller-verification.v2` `e1faa9ec`, `relay-record.v2`
`dca5b29c`, `binding.v1` `13501476`, `preflight.v1` `6a054079`.

## 1. Invocaciones cubiertas (todas EXECUTION_CONTROLLER, `Permissions` READ_ONLY)

| Invocación | `Action` | `OutputContract` (AP 16.23) | `Target` | `Phase` del registro | Cuenta en D.3 |
|---|---|---|---|---|---|
| planificación | PLAN | `rackcad-delegation/v2` | `null` (solo PLAN admite `null`) | PLANNING | 1 de 11 |
| verificación | VERIFY | `rackcad-controller-verification/v2` | `{commit: G, path, blob}` (V14 B.9; qué ruta, con un cambio de dos archivos: OQ-31, en CD-14 / `{CLOSURE_CUSTODY_RULE}`) | VERIFICATION | 1 de 11 |
| nc1, nc2, nc3, N4, N5, N6, N7a, N7b | VERIFY (mismo prompt, salvo la ruta de entrada y el `RunId`; RAE §10) | `rackcad-controller-verification/v2` | el de la verificación real (OQ-31) | CONTROL | 8 de 11 |

PLAN y VERIFY no son intercambiables: PLAN con `controller-verification/v2` o VERIFY con `delegation/v2` es INVALID (P-19; RAE §16, casos g-h).

**Binding de la verificación.** VERIFY usa el binding del Controller de verificación (perfil CONTROLLER_VERIFICATION), nuevo y transitorio dentro de
la ventana (V14 B.8.3, fila bindings; README S21b), no el de planificación. El contrato T1 solo tiene la entrada PLAN del rol (G1): qué requisitos e
independencia se le exigen es OQ-28 (CD-22); cuándo y cómo se produce y se acepta, `{IN_WINDOW_BINDING_RULE}` de la orden. Los controles con
invocación los ejecuta «el Controller Codex real con `CONTROLLER_VERIFICATION`» (RAE §10).

## 2. Lo que recibe cada invocación

### 2.1 `rackcad-role-invocation/v1` (todos los campos son obligatorios; `null` solo donde el esquema lo marca como «no aplica»)

| Campo | Contenido en FX-02 |
|---|---|
| `Schema` | `rackcad-role-invocation/v1` |
| `InvocationId` | `I<yyyyMMddTHHmmssZ>-<4 hex>`, uno por intento físico, distinto del `RunId` |
| `LogicalReviewRequestId`, `AttemptSeq` | `null` (no aplica fuera de un bucle de revisión) |
| `UnitId`, `Gate`, `TaskId`, `ProtocolSet` | `FX-U1`, `FX-U1-T1`, `T1`, `rackcad-protocol/I62` |
| `RequestedRole`, `Action` | EXECUTION_CONTROLLER; PLAN o VERIFY |
| `Target` | PLAN: `null`; VERIFY: `{commit, path, blob}` exactos (ruta con un cambio de dos archivos: OQ-31) |
| `AuthorityRevision` | la del contrato: `1746b404e57d69170ecfc084fd24fa3e699916d7` |
| `CanonicalInputs[]` | `{Path, Blob}` en la `AuthorityRevision` (V14 B.9): contrato, archivo de decisiones, estado y lo que la acción necesita |
| `AllowedTransitiveInputs[]` | `{Path, Blob, RequiredBy {Path, Section, Blob}}` del cierre (§2.2) |
| `AllowedActions[]` | `{Action, RequiredBy, Class (ACTION_COMPATIBLE \| EXEMPTED), ExemptionRef, ExemptionScope}`; `dotnet test` solo como EXEMPTED con la exención de CD-07 (OQ-17) |
| `HealthSignals[]` | `{Kind PUBLICATION_CI, RunRef, Sha}`; nunca sustituyen a una prueba (AP 16.24) |
| `DeclaredRuntimeContext[]` | `{Kind, Source, SizeOrSha256}` que declara el adapter |
| `ForbiddenInputs[]` | transcripción y memoria del autor, sesiones ajenas, worktrees reales, artefactos transitorios no enumerados (D.6) |
| `EffectiveInputClosure` | `StateRef` del `rackcad-input-closure/v1` (custodia según CD-14; OQ-18) |
| `InputFidelityPreflight` | `StateRef` del `rackcad-input-fidelity/v1` PREFLIGHT; sin FAITHFUL o FAITHFUL_NORMALIZED no se lanza (P-24) |
| `OpenFindings[]` | `[]` (ningún linaje abierto en la ejecución) |
| `RequiredCapabilities[]` | `{RequirementId, Required}` del perfil y la acción (§5) |
| `IndependenceRequirements` | `{Requirements [WORKER: Actor REQUIRED, Session REQUIRED, Context REQUIRED, Provider PREFERRED], ReviewSubject null}` (contrato T1; no es revisión mayor) |
| `Permissions` | READ_ONLY |
| `Binding` | `BindingRef` del binding aceptado del rol (`{UnitId, Scope TASK, TaskId T1, Role, BindingId, Sha256, Location}`) |
| `OutputContract` | §1 |
| `BudgetSnapshot` | `{ReviewRounds, LogicalRequests, ArchitectLaunches, TransportReruns, CorrectionRounds, CorrectionsByLineage}` (fuera de bucle: los de `budgets`) |
| `StopConditions[]`, `EscalationConditions[]` | por id: S-T1-01..03 del contrato y los STOP de 16.11, 16.16, 16.19-16.30 aplicables |
| `Authorization` | `StateRef` del contrato de gate custodiado (`628d89af`) |

Reglas de construcción R1-R9 (RAE §15.1): R1 pareja rol/acción → contrato de salida; R2 `Target` salvo PLAN; R3 READ_ONLY; R4 binding aceptado del mismo
rol (si no, P-10); R5 cierre y fidelidad custodiados y válidos; R6 nada prohibido en las entradas; R7 exenciones con referencia; R9 sin texto de prompt
(lo renderiza el adapter, AP 16.17).

### 2.2 `rackcad-input-closure/v1`

Campos: `Schema`, `AuthorityRevision`, `CanonicalInputs[] {Path, Blob}`, `AutomaticInstructions[] {Path, Blob, Adapter}`, `Obligations[] {SourcePath,
SourceSection, SourceBlob, Text, Class, Resolution, Target, Authority, Reason}`, `AllowedTransitiveInputs[]`, `AllowedActions[]`, `DeclaredRuntimeContext[]`,
`ForbiddenInputs[]`, `FixedPointIterations`. Cálculo de AP 16.24 hasta el punto fijo; reglas I1-I6 (RAE §15.2). En el fixture, las instrucciones
automáticas del directorio para `codex-cli` incluyen `AGENTS.md` del clon, cuyo «Leer primero» (WORKFLOW, AUTOMATION_PLAN, README de agent-execution)
son obligaciones READ, y cuyos «Comandos canonicos» incluyen `dotnet test` (ACTION_INCOMPATIBLE en `read-only`: sin exención, no hay lanzamiento; OQ-17).
Tras la terminación, una lectura fuera del cierre es P-22; un registro sin lecturas o incompleto deja Contexto UNKNOWN.

### 2.3 `rackcad-input-fidelity/v1`

Campos obligatorios de primer nivel (esquema `e54990fc`): `Schema` (`rackcad-input-fidelity/v1`), `Kind`, `InvocationId` (el de la
`role-invocation/v1` a la que acompaña), `Fidelity`, `SpanMap`, `PremiseIndependence`.
`Kind` FIDELITY, `Phase` PREFLIGHT antes de lanzar y POSTRUN tras la terminación: `Fidelity {Phase, AuthorityRevision, DeliveredRepresentationSource,
CorpusCharacters, Records[] {InputRef, GitBlob, CanonicalSha256, Encoding {Charset UTF-8, Bom, LineEnding, FinalNewline}, TransportRepresentation,
TransportSha256, VerificationMethod, CharacterClassesChecked[] {Character, CountCanonical, CountTransport}, FidelityStatus, DegradedSpans}}`; `SpanMap` y
`PremiseIndependence` en `null` salvo en sus `Kind`. Reglas F1-F7 (RAE §15.3): un registro por insumo y uno para `PROMPT`; PREFLIGHT no fiel → no se lanza
(P-24); POSTRUN sin fuente de lo entregado → UNVERIFIED. Antecedente de degradación con este adapter: GAP-09/GAP-12 (V14 §20.11).

### 2.4 `rackcad-relay-record/v2` de cada lanzamiento

`Schema`, `TaskId`, `RunId`, `Attempt`, `Phase`, `WindowSeq` (= 1), `PrevRelaySha256` (null solo en el primero de la ventana), `Host`, `Participant {Role,
BindingRef, Actor, Session, AdapterId, AdapterVersion, Transport external-process, Observation[]}`, `Exit {HeadSha, LsRemoteSha, OriginMainSha, CleanTree,
GitOperationInProgress, SummaryCommitSha, Fingerprint, Processes, OpenDelegations, DelegationStatus}`, `Cession {StartUtc, EndUtc, SessionOperated}`,
`Outcome {Kind, LaunchedPid, ExitCode, TerminalEvent, TreeKilled, DeathConfirmed, OutputPath, OutputSha256, OutputWrittenUtc, Acceptance}`, `Entry
{HeadSha, LsRemoteSha, CleanTree, Fingerprint, FingerprintChanged, Processes}`, `RemoteFacts`, `RebaseMap`, `RoleInvocation {LogicalReviewRequestId,
InvocationId, AttemptSeq}`, `ReadAudit {Coverage, ReadsOutsideClosure[]}`, `InputFidelity`, `Disposition`, `Notes`. `Outcome.Acceptance` lleva
`A1`..`A8` (pass/fail) en el registro de la planificación. `Participant.Observation[]` lleva modelo y effort solicitados y efectivos (`turn_context` del
registro de sesión, RUNTIME_OBSERVED).

## 3. Lo que devuelve cada invocación

### 3.1 PLAN → `rackcad-delegation/v2`

Campos: `Schema`, `TaskId`, `RunId` (= el de la invocación de planificación = `DelegationRunId`), `Initiative`, `Unit`, `Gate`, `Attempt`,
`AuthorityRevision`, `MainSha`, `BaseSha` (= el Q0), `ExpectedBranch`, `ExpectedWorktree`, `Owner {Kind session-internal \| external-process, Id
<Kind>:<DelegationRunId>}`, `TaskClass`, `Dimensions` (siete, Low/Medium/High), `Executor {Provider, Transport, Role, Cell, BindingRef}`, `Model`,
`Effort {Semantic, Provider}`, `PromptProfile`, `RoutingReason`, `ModelEscalationReason`, `RoutingEnforcement` (copia: `required`), `Authorities[]`,
`Objective`, `AllowedWriteScope`, `ForbiddenWriteScope`, `Invariants`, `AcceptanceCriteria`, `RequiredTests[]`, `ExpectedEvidence`, `StopConditions[]`,
`MaxReworkLoops` (3), `AttemptsRemaining`, `ChainBaseSha`, `ChainRedSha`, `ChainRedFiles`, `CorrectionOf`, `ExpectedHandoffPath`
(`artifacts/orchestration/…/{WorkRunId}/worker-handoff.json`), `IssuedBy`, `RoleRequirements[]` (copia del contrato, nunca menor), `SupersededCommits[]`
(copia exacta: `[]`).

Aceptación A1'-A8' (AP 16.22; RAE §4, §14.7), sin cortocircuito, la más grave de las fallidas: A1' esquemas (`delegation/v2`, `gate-contract/v2`,
`binding/v1` del ejecutor, hechos del adapter en dos fases); A2' `Unit`/`Gate`/`TaskId` y `RunId` de una planificación COMPLETED no usada + `BindingRef`
(RAE §14.4); A3'-A5' alcances, invariantes, STOP, autoridades y pruebas ⊇ contrato + G4/G5; A6' `AuthorityRevision`, `MainSha` = `origin/main` recién
obtenido, `BaseSha` = `HEAD` = remoto, rama y worktree + `ProtocolSet` (P-15); A7' binding del ejecutor aceptado y preflight vigente; A8' `Attempt`,
`AttemptsRemaining`, `MaxReworkLoops`, `RoutingEnforcement`, cadena, `ExpectedHandoffPath`, `Owner` + `CountersSnapshot` = autoridad. Antes, nc4 sobre
una copia (RAE §10). Quién decide A7' dentro de la ventana: OQ-03.

### 3.2 VERIFY → `rackcad-controller-verification/v2`

Campos: `Schema`, `TaskId`, `RunId`, `DelegationRunId`, `WorkRunId`, `Attempt`, `VerifiedSha`, `Verifier {Role EXECUTION_CONTROLLER, BindingRef, Actor}`,
`Checks` (14, cada uno `{Result pass|fail|not_run, Evidence}`; `Ci` y `Tests` además `RedPart pass|fail|not_applicable`), `AuthorityResolution[] {Path,
Section, Class, Mode NORMAL|COMPAT|COMPUESTA|ENTRY, RevisionSha, Parts[], EffectiveSha, ClauseMapBlob, UnitProtocol}`, `Classification`, `Disposition`,
`FailureClass`, `TriggeredStopConditions`, `Findings`, `RecommendedNextAction`.

Orden fijo de las 14 (AP 16.9): `Termination`, `Handoff`, `Authority`, `Contract`, `Identity`, `Remote`, `Scope`, `CleanTree`, `Ci`, `Tests`, `Trailer`,
`Routing`, `FreeText`, `Denials`. Deltas I62 (AP 16.22): `Termination` exige `Outcome` COMPLETED y la operación 7; `Ci` usa los jobs del `AGENTS.md` del
fixture (`fixture-build`, `fixture-tests`; RED = `fixture-tests` en failure); `Tests` + `Skipped` coherente; `Trailer` coherente con el proveedor y el
modelo del binding; `Contract` + `RoleRequirements`; `Routing` `required`: efectivo no observado al mínimo → fail (BLOCKED). `Scope` = pass solo con la
salida reproducible de `git diff --name-only BaseSha..CurrentSha` y la entrada de `AllowedWriteScope` que contiene cada ruta; si no, `not_run`
(RAE §14.7). Comprobación mecánica de coherencia del relevo (RAE §8): pares válidos `VERIFIED/NONE`, `REWORK_REQUIRED/REWORK`, `BLOCKED/BLOCKED`,
`BLOCKED/STOP`; VERIFIED ⇔ las 14 en pass; `FailureClass` = la primera no pass (o `StopCondition`); ningún `RedPart` fail con `Result` pass. Una salida
que no la cumple es `INVALID_OUTPUT` (transporte BLOCKED).

## 4. Receta `codex-cli` (AP 16.4 «Receta de Codex»; RAE §5; descriptor `codex-cli` operaciones 3-7)

Elementos fijos: binario por ruta verificada; `-C` con el worktree de la unidad y ningún otro; `-s read-only`; `-m` y `-c model_reasoning_effort=…` de la
celda del binding aceptado; `--output-schema`; `-o` al directorio del `RunId`; `--json`; **sin** `--ephemeral`; stdin cerrado; el `pwsh` del runtime de
Codex primero en el `PATH` solo del proceso hijo; tope de 600 s con terminación del árbol; huella antes y después; el `service_tier` del Owner se hereda sin
tocarlo. Plantilla del fixture (los `{…}` los fija la autorización de transporte de la orden O4; nada aquí elige la celda):

```bash
codex_bin="$LOCALAPPDATA/OpenAI/Codex/bin/{etiqueta autorizada}/codex.exe"
wt="D:/r62-fixture/A2"                                   # -C del Controller: decisiones §55, U-03 (CD-10, decidida en esto)
run_dir="artifacts/orchestration/FX-U1/T1/0/{RunId}"     # T1-<control>/0/{RunId} para los negativos
PATH="/c/Users/<usuario>/.cache/codex-runtimes/codex-primary-runtime/dependencies/native/powershell:$PATH" \
timeout --kill-after=10 600 "$codex_bin" exec -C "$wt" -s read-only -m "{modelo de la celda}" -c model_reasoning_effort="{effort de la celda}" \
  --output-schema "docs/automation/agent-execution/schemas/{delegation.v2|controller-verification.v2}.schema.json" -o "$run_dir/output.json" --json \
  "$(cat "$run_dir/prompt.md")" < /dev/null > "$run_dir/events.jsonl"
```

Después: el `thread_id` del evento `thread.started` localiza el registro de sesión propio de esa invocación; de él salen `session_meta.model` y
`turn_context.model`/`effort` (efectivos, RUNTIME_OBSERVED). Proceso vivo al vencer el tope: `taskkill /PID <pid> /T /F` y comprobación de procesos hasta
confirmar la muerte. Nunca `workspace-write` (crea una entrada de confianza: P-01; y no puede hacer commit: OD-4).

Celdas candidatas según el catálogo (hechos, no elección): `gpt-6-luna` (Eficiente; Balanced → `high`) y `gpt-6.1-sol` (Equilibrado) para
CONTROLLER_*; `gpt-6-astra` no elegible (créditos/API). Sus mediciones con el binario `3b8f6e33` están **obsoletas** con el binario vigente
(dec. §50; ev. §75): la elegibilidad exige una invocación medida con el binario autorizado (routing §5; AP 16.20).

**Invocación medida de la celda del Controller (decisiones §55).** Por U-02(b) (punto 9), las sondas de solo lectura autorizadas para el binario
vigente son las invocaciones medidas que exigen las celdas del Controller y del Architect. U-03 fija el `-C` del Controller (`D:\r62-fixture\A2`), no
el directorio de su sonda: ese directorio sigue pendiente (CD-28 / OQ-35 del README) y la sonda con `-C D:\r62-fixture\A2` es la **propuesta** de este
staging. El bloque nuevo solo existe con A-2 AGREED y con la autoridad que A-2 fije (en la candidata, §3.3 regla 3: una disposición del Coordinator
nombra el par exacto y el Owner autoriza el consumo), porque el tope congelado de sondas está agotado (U-04; A-2, Problema 2: como máximo dos sondas
`read-only` para el BinaryHash/AppVersion exactos, sin reinicio de contadores). Va antes de abrir A2 (README S11 y S04). Tras OD-2d **no** hay segunda medición mientras sigan
iguales todos los invalidadores: BinaryHash, AppVersion, huella, estado de autenticación, modelo, effort, blobs de catálogo y routing, e instancia del
host. Cualquier cambio deja obsoleta la observación. U-02(b) no dispone de dónde salen las filas RUNTIME_OBSERVED del preflight de A2 (OQ-25) ni cómo
entra la medición en el fixture (OQ-10).

## 5. Revalidación antes de cada invocación (Salida) y después (Entrada)

| Momento | Qué | Si falla |
|---|---|---|
| Salida | `HEAD` = `origin/fx/u1` por `git ls-remote`; el último commit lleva el resumen de estado (antes de la planificación, el Q0; antes de la verificación y de cada control, el GREEN G, cuyo cuerpo debe llevarlo: `worker-reviewer-contracts.md` §2.3); árbol limpio y ninguna operación Git en curso; `git fetch` y `origin/main` registrado; procesos (desde PowerShell, no desde Git Bash: RAE §3.2); una sola delegación abierta (`DS`); SHA-256 de `~/.codex/config.toml` y lista ordenada de nombres de secciones y claves (sin valores) | árbol sucio u operación en curso: no hay Salida (OQ-02); `DS` UNKNOWN: STOP |
| Salida | huella = la autorizada **y** SHA-256 del binario = el autorizado (y ruta = la autorizada) | P-01: STOP sin aceptar nada; ninguna invocación de Codex hasta la decisión del Owner |
| Salida | los demás invalidadores de decisiones §55 U-02(b) (versión de la app, estado de autenticación, modelo, effort, blobs de catálogo y routing, instancia del host) iguales a los de la medición de la celda | observación obsoleta: sin invocación medida vigente no hay elegibilidad (AP 16.20; P-10); con A-2, la recuperación es otro bloque de medición |
| Salida | P-07: `launched + uncertain + 1` ≤ tope | no se lanza |
| Cesión | A2 no lee, no escribe ni ejecuta nada sobre el worktree; `StartUtc`/`EndUtc` | P-08: salida inválida |
| Entrada | el proceso lanzado y su árbol muertos; mismas comprobaciones de la Salida; huella y binario iguales | cambio de huella: P-01 (P-11), con el diff de nombres de claves |
| Entrada | modelo y effort efectivos del registro de sesión = los del binding (`RoutingEnforcement` required) | `Routing` fail en la verificación (BLOCKED) |
| Entrada | contraste de autenticación, versión del binario y huella del preflight con los del Exit/Entry (RAE §13.3) | S-04 |

## 6. Reglas de STOP que rigen estas invocaciones

| Id | Condición (fuente) | Comportamiento |
|---|---|---|
| P-01 / P-11 | huella cambiada (AP 16.4, 16.11, 16.19) | STOP del transporte; ninguna invocación de Codex más hasta la decisión del Owner; se registra el diff de nombres de claves |
| P-10 | obligatorio del rol en UNKNOWN o BELOW_REQUIRED, o sin binding elegible (AP 16.16, 16.20) | no se vincula ni se invoca; no consume `attempts` |
| S-04 | evidencia contradictoria: discrepancia preflight/Exit/Entry, recálculo de un binding distinto de lo registrado, contador contradictorio (AP 16.11, 16.18, 16.27; RAE §14.2) | STOP y decisión del Coordinator |
| P-02 | participante vivo ajeno, proceso no atribuible o más de una delegación abierta (AP 16.4) | STOP |
| P-06 | aviso de límite de uso o de créditos en eventos o transcripción | STOP |
| P-07 | una invocación excedería el tope | no se lanza; STOP |
| P-08 | la sesión operó durante la cesión | salida inválida; STOP |
| P-14 | hechos del adapter inválidos, esquema ausente o blob distinto (AP 16.19) | no elegible |
| P-19 | salida de rol inválida o incompleta (AP 16.23) | no avanza; reejecución de transporte dentro del tope |
| P-22 / P-24 / P-25 | lectura fuera del cierre; fidelidad no acreditada antes; degradación observada después (AP 16.24) | según AP 16.24 |
| S-12 | el Controller pierde el contexto o no puede verificar la revisión de autoridad (AP 16.11) | STOP, `analysis.md`, decisión del Coordinator |

Todas estas invocaciones (salvo el Architect, §7) ocurren dentro de la ventana, donde el Coordinator del fixture no puede publicar la decisión que un
STOP exige (W-2; para A2, una escritura ajena sería T10(c)). Qué hace A2 ante un STOP dentro de la ventana (p. ej., cierre declarado y Q7 antes de
esperar) no está escrito: OQ-29 (CD-23, `{IN_WINDOW_STOP_RULE}`).

## 7. Architect (`codex-cli`, invocación propia): revisión del contrato de T1

Misma receta de §4 y misma revalidación de §5, con su propio `thread_id`. Su `-C` **no** está decidido: decisiones §55, U-03, fija el del Controller
de FX-02 (`D:\r62-fixture\A2`) y el del Architect de FX-06 (`D:\r62-fixture\arch`), pero no el de este Architect; con la receta literal sería `A2`, y su
bucle publica QU LAUNCHED durante la corrida (Cesión): OQ-34 (CD-27). Diferencias:

| Elemento | Contenido | Fuente |
|---|---|---|
| Autoridad | una `ReviewLoopAuthorization` del Coordinator en `FX-U1.md` (marcador `I62-REVIEW-LOOP-AUTHORIZATION: <AuthorizationId>`, `Role` ARCHITECT, `ContinuesLoopInstanceId` null, `ObjectFamily`, `CorrectionScope`, `Budget.<tope>` ≤ congelados, `Validity.Until`, `Claim-Id`, y los criterios de materialización de AP 16.20 y las exenciones de opción B) — **hoy no existe** (OQ-05, CD-05) | AP 16.28, 16.29; V14 §20.5 |
| Binding | dos bases admitidas (AP 16.20, paso 5: «aceptación: individual (A7', por el Coordinator) o materialización autorizada»; V14 B.5 `Acceptance`; RAE §14.2 B7-B9): **(1)** INDIVIDUAL_DECISION, con `DecisionRef` a un marcador de decisión del Coordinator en `FX-U1.md` (el texto literal depende de CD-03, OQ-03); **(2)** AUTHORIZED_MATERIALIZATION bajo la RLA, con todos los criterios en SATISFIED (`ROLE_ACTION`, `CAPABILITY:<id>`, `INDEPENDENCE:<ref>:<dim>`, `CELL`, `MODEL_EFFORT`, `PERMISSIONS`, `OBJECT`, `VALIDITY`) y `AuthorizationRef` resoluble por ResolveBranchRef desde el punto que custodia el binding (B8 admite esta base para ARCHITECT). La RLA es requisito del **bucle** (AP 16.29), no de la base del binding | AP 16.20; AP 16.29; V14 B.5; RAE §14.2-§14.3; A-1 D2-11 |
| Requisitos | `ARCHITECTURE_REVIEW.effort` (Deep), `.level` (Equilibrado o Frontera), `.read` (contrato T1; routing §1, §3, §8). Hecho del catálogo: `gpt-6-luna` (Eficiente) no llega; `gpt-6.1-sol` (Equilibrado) sí, con medición obsoleta por el binario nuevo; `gpt-6-astra` no elegible. `Eligibility.MeasuredInvocation.RunRef` sin identificadores reales: OQ-10 (CD-09) | routing; catálogo; kit FX-06 |
| Independencia | frente a AUTHOR (autor del contrato: el Coordinator del fixture, sesión de supervisión; `IssuedBy` del contrato): Actor, Sesión y Contexto REQUIRED; Proveedor PREFERRED. B.5 exige `ReferenceActor` (un `ActorRef`) y RAE §14.5 da Actor/Sesión SATISFIED solo con ambos `ActorRef`/`SessionRef` RUNTIME_OBSERVED. A2 no puede observar admisiblemente la sesión de supervisión (D.6) y escribir su `ActorRef` en el fixture violaría P-16: sin la disposición de CD-19 (OQ-24), Actor y Sesión quedan UNKNOWN → binding no aceptado (B6; AP 16.21) y el paso del Architect, UNVERIFIED con esa causa. Evaluación de un candidato no arrancado: OQ-04 | contrato T1; V14 B.5; RAE §14.2 B6, §14.5; AP 16.21; V14 D.6; AP 16.30 P-16 |
| Invocación | `RequestedRole` ARCHITECT, `Action` REVIEW_DESIGN, `Target` = `{commit d30fb6a9d26c0b5cb189a30b98ec97d0edcb7b99, path docs/automation/decisions/FX-U1-T1.gate-contract.json, blob 628d89af5f21e4e14cbe7c3c14f0dd8fb5a490e0}`, `LogicalReviewRequestId` y `AttemptSeq` no nulos, `BudgetSnapshot` de su entrada de `architect_budgets[]`, `OpenFindings` = linajes ARCHITECT/COORDINATOR abiertos, `OutputContract` `rackcad-architect-review-result/v1` | V14 B.9; A-1 D1-14, D1-15 |
| Puntos durables | QU ORDINARY por fase: REVIEW_PENDING (intento BUDGET_RESERVED, `ARL-<rv>`, entrada nueva de `architect_budgets[]`), LAUNCHING (antes de lanzar, en el remoto), LAUNCHED, RESULT_RECEIVED, RESULT_INGESTED; LOOP_CLOSED en un QU (sin decisión desde ARCHITECT_SATISFIED; con `I62-REVIEW-LOOP-CLOSE` en otro caso). **Fuera de la ventana** (README §3) | AP 16.29; RAE §18.1-§18.5; A-1 D1-1, D1-2, D1-7, D1-9 |
| Resultado | `Verdict` AGREED \| CHANGES REQUIRED \| BLOCKED — OWNER DECISION; `RequiredFindings[]`/`OptionalFindings[]` `{FindingId, LineageId, Source, AffectedSection, Evidence, PremiseRefs[], WhyItMatters, CorrectionRequired}`; `FindingDispositions[]`; `Downgrades[]`; `OwnerDecisions[]`; sobre común de B.10.0. Validación V1-V8, V11 (RAE §16) | V14 B.10.0-B.10.1 |
| CHANGES REQUIRED | el contrato es del Coordinator y está en el alcance prohibido de T1: la corrección es COORDINATOR_DECISION (no la hace A2) | V14 §20.1; contrato `ForbiddenWriteScope` |
| Topes | D.3: 1 lanzamiento dentro de los 11 de Codex (reejecuciones del pool); AP 16.29: congelados por instancia, la RLA puede bajarlos | V14 D.3; AP 16.29 |
