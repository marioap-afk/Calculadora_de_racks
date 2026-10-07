# FX-02 — contratos del Worker y del Reviewer (`claude-subagent`)

> **Preparación; solo supervisión.** Lo que A2 entrega a cada subagente y lo que cada uno devuelve. No contiene valores esperados. Autoridades: contrato
> T1 (`fx/u1:docs/automation/decisions/FX-U1-T1.gate-contract.json`, blob `628d89af`); AP 16.4, 16.8, 16.9-16.10, 16.20-16.23, 16.25, 16.29; A-1
> D1-4, D1-10, D1-13, D1-16..D1-21; V14 B.6, B.9, B.10.0, B.10.2, §11.3, §20.7; RAE §6, §14-§16, §18.6; routing §1, §3, §5, §8; catálogo (sección
> Anthropic); descriptor `adapters/claude-subagent.md` (`e10926c0`); esquemas `worker-handoff` (`cc3c9cf7`) y `reviewer-result.v1` (`a74288eb`).

## 1. Requisitos e independencia del contrato T1 (`RoleRequirements`, copia literal resumida)

| Rol | Perfil / acción | `Mandatory` | Independencia exigida | Consecuencia para los subagentes |
|---|---|---|---|---|
| PRINCIPAL_COORDINATOR | PRINCIPAL_COORDINATION / CUSTODY | `.level`, `.effort`, `.remote-facts`, `.introspection`, `.repo-write` | — | A2 (sesión padre de los subagentes) |
| EXECUTION_CONTROLLER | CONTROLLER_PLANNING / PLAN | `.effort`, `.level`, `.read`, `.tool-use`, `.structured-output` | frente a **WORKER**: Actor REQUIRED, Sesión REQUIRED, Contexto REQUIRED, Proveedor PREFERRED | el Worker es la **referencia**: su `ActorRef` (id del subagente, de la transcripción) y su `SessionRef` (= la de A2) deben quedar RUNTIME_OBSERVED en su registro para que la verificación pueda evaluarlos (RAE §14.5); un Controller `codex-cli` tiene otro actor, otra sesión y otro proveedor; Contexto = solo su cierre. Cuándo se evalúa: OQ-04 |
| WORKER | ROUTINE_IMPLEMENTATION / IMPLEMENT | `.effort`, `.level`, `.tool-use`, `.write-commit-push` | — | §2 |
| ARCHITECT | ARCHITECTURE_REVIEW / REVIEW_DESIGN | `.effort`, `.level`, `.read` | frente a **AUTHOR**: Actor, Sesión y Contexto REQUIRED, Proveedor PREFERRED | no afecta a los subagentes |
| REVIEWER | — | — | — | **ausente del contrato**: OQ-06 |

Reglas transversales: un subagente comparte la sesión de su padre, así que nunca satisface Sesión frente a otro subagente de A2 ni frente a A2 (V14
§11.1; descriptor `claude-subagent`, «Límites»); dos `BindingId` con el mismo `ActorRef` son el mismo actor (AP 16.21). Acumulación de roles (V14 §2):
«WORKER + REVIEWER de su entrega: autorrevisión permitida, nunca independiente». En FX-02 el Reviewer es además un subagente **distinto** del Worker
porque V14 D.3 (topología A) lo fija así: «Reviewer (`claude-subagent` nuevo)»; no es una prohibición de AP 16.1 ni de 16.21.

## 2. Worker (`claude-subagent`; una llamada notificada, tope de 60 min)

### 2.1 Binding

- Requisitos: los cuatro `Mandatory` del contrato; `ROUTINE_IMPLEMENTATION.effort` = el effort de la delegación y `.level` = el nivel de routing §3
  para ese effort (routing §8).
- Elegibilidad: invocación medida, consumo cubierto y celda no `STALE` (routing §5). **Hechos del catálogo** (no elección; la elige la planificación,
  routing §4 y §9): `claude-sonnet-5-5` subagente × `read`/`tool-use`/`write-commit-push` × `medium` y `high` medido en la sonda U-04 (2026-10-01 UTC),
  verificado el 2026-09-30 (`STALE` desde el 2026-12-29), nivel Equilibrado; `claude-opus-5-5` subagente × `write-commit-push` no medido;
  `claude-haiku-4-5-20251001` `STALE` desde el 2026-10-01; `claude-fable-5-1` no medido.
- `Eligibility.MeasuredInvocation.RunRef`: la invocación medida de la celda (sonda U-04, piloto de G3) es evidencia de RackCad; citarla desde un binding
  del fixture plantea el mismo problema P-16 que la medición de `codex-cli` (OQ-10, CD-09, ampliadas a todos los bindings).
- Filas obligatorias del preflight de una candidata aún no invocada: sin invocación quedan NOT_OBSERVED → UNKNOWN → P-10 (RAE §13.1 #4-#5); fuente de
  esas observaciones y tope en que contaría una invocación de observación: OQ-25 (CD-20).
- Descriptor: operaciones 1-8 DISPONIBLE; operación 9 (huella) UNVERIFIED (OQ-20). Introspección por la transcripción (RUNTIME_OBSERVED); que el
  subagente aplique un modelo y un effort **solicitados** distintos de los heredados no está medido en el descriptor (el catálogo registra la sonda U-04 con
  el alias `sonnet`).
- Aceptación: la regla de CD-03 (OQ-03).

### 2.2 Invocación (`rackcad-role-invocation/v1`, `RequestedRole` WORKER, `Action` IMPLEMENT)

`Permissions` = WRITE_IN_SCOPE; `OutputContract` = `rackcad-worker-handoff/v1`; `Binding` = el aceptado; `Authorization` = el contrato custodiado;
`EffectiveInputClosure` e `InputFidelityPreflight` como en `controller-contracts.md` §2.2-§2.3 (custodia según CD-14; OQ-18). R2 exige `Target` no nulo
salvo en PLAN: qué objeto exacto es el `Target` de IMPLEMENT no está escrito (OQ-23). El prompt lo renderiza el adapter desde el contrato neutral y la
delegación aceptada (AP 16.17), con `ExpectedHandoffPath` sustituido por el `RunId` de esta invocación (AP 16.6).

### 2.3 Trabajo exigido (contrato T1 + AP 16.8 + AP 16.9)

| Elemento | Exigencia | Fuente |
|---|---|---|
| Objetivo | `Calculator.Subtract(int a, int b)` en `Fixture.Lib` con su prueba en `Fixture.Tests`: primero RED y después GREEN | contrato `Objective` |
| Alcance de escritura | solo `src/Fixture.Lib/Calculator.cs`, `tests/Fixture.Tests/CalculatorTests.cs` (y `docs/automation/evidence/FX-U1-agent/**`, véase OQ-14/OQ-15); nunca `ForbiddenWriteScope` (incluidos `.github/**`, los `.csproj`, `AGENTS.md`, `CLAUDE.md`, `docs/automation/decisions/**`, `docs/automation/agent-execution/**`) | contrato; AP 16.9 #7 |
| Estado | nunca escribe `docs/automation/state/FX-U1.yml` | AP 16.25 W-3; V14 I-P08 |
| Invariantes | `Calculator.Add` conserva firma y comportamiento; sin paquetes ni dependencias nuevas; P-16 | contrato `Invariants` |
| Commit RED | el «esqueleto» de la primera delegación: la prueba de `Subtract` (seleccionable con `FullyQualifiedName~Subtract`, al menos 1) y lo mínimo para que **compile y la prueba falle al ejecutarse**. Motivo: `RedPart` de `Tests` exige que las pruebas con `ExpectRed` estén «entre las fallidas» del RED; un RED que no compila no ejecuta ninguna prueba | AP 16.8 («El commit RED es el esqueleto…»); AP 16.9 #10; contrato `RequiredTests` |
| Commit GREEN | la implementación, sin tocar `RT` ∪ `ChainRedFiles` (en la primera delegación, `RT` = las pruebas cambiadas entre `ChainBaseSha` y `RedSha`): en T1, solo `src/Fixture.Lib/Calculator.cs` | AP 16.8; AP 16.9 #10 |
| Commits de solo evidencia | ninguno: un commit que solo toca `docs/automation/` se lee como commit de la sesión | AP 16.1; V14 B.8.6; validador I-P03 (OQ-14) |
| Publicación | push de RED y de GREEN a `origin` **y** a `github` (la CI corre en `github`; `Identity` mira `origin`); `Pushed` = true | AP 16.4 paso (4); V14 D.2; OQ-13 |
| Trailer | cada commit `BaseSha..CurrentSha` con el `Co-Authored-By` que declare la delegación, coherente con el proveedor y el modelo efectivos del binding | AP 16.9 #11; AP 16.22 |
| Identidad Git | autor y committer `fixture <fixture@example.invalid>` | `AGENTS.md` del fixture |
| Pruebas locales | si ejecuta `dotnet test`, los `bin/` y `obj/` resultantes no pueden dejar el árbol sucio (regla de CD-02; OQ-02) | AP 16.9 #8 |
| Texto libre | ningún término de AP 16.10 en `WorkCompleted`, `Evidence`, `UnexpectedFindings`, `KnownLimitations`, `Deviations`, `OpenQuestions`, `RecommendedNextAction` | AP 16.9 #13, 16.10 |
| Terminación | termina con su llamada (notificación de finalización); ningún descendiente nuevo ni huérfano vivo en la Entrada | AP 16.4 («Procesos»); RAE §3.2, §6 |

### 2.4 Salida: `rackcad-worker-handoff/v1` en `ExpectedHandoffPath`

Campos (todos obligatorios): `Schema`, `TaskId`, `RunId` (el de esta invocación), `DelegationRunId`, `Initiative`, `Gate`, `Attempt`, `BaseSha`, `RedSha`,
`CurrentSha`, `Branch`, `Worktree`, `Pushed`, `FilesChanged[]`, `WorkCompleted`, `TestsExecuted[] {RunRef, Command, Phase RED|GREEN|RELEVANT|MUTATION,
TreeSha, ResultsFile}`, `TestResults[] {RunRef, Selected, Passed, Failed, Skipped, FailedTests[]}`, `Evidence[]`, `UnexpectedFindings`,
`KnownLimitations`, `Deviations`, `OpenQuestions`, `WorkerStatus IMPLEMENTATION_COMPLETE|PARTIAL|BLOCKED`, `Disposition NONE|BLOCKED|STOP`,
`TriggeredStopConditions[]`, `RecommendedNextAction`, `Worker {Provider, ModelRequested, EffortRequested, Trailer}`. Correlaciones I62 (V14 B.6): `TaskId`,
`Attempt` y `DelegationRunId` → delegación; `RunId` → relevo de trabajo; SHAs, rama y worktree → `Identity`; `Worker.Provider` → descriptor del binding;
`ModelRequested`/`EffortRequested` → delegación; `Trailer` → binding. El efectivo va en `relay-record/v2.Participant.Observation`.

Reejecución: V14 D.3 da al Worker «+1 solo si hay REWORK (corrección, cuenta en `attempts`)», tope 2: ese +1 va tras un REWORK con corrección
autorizada (`CorrectionsAuthorized` true), en una ventana nueva cuyo Q0 es CORRECTION (`attempts` +1 y su `CorrectionLaunch`). Un fallo de transporte del
Worker es BLOCKED/WORK y, por AP 16.8, su reejecución (≤ 2 por fase) no toca `attempts` (AP 16.27; RAE §9). Si esa reejecución BLOCKED cabe en el tope de
2 de D.3, consume el hueco de la corrección o no está permitida **no está escrito**: OQ-26 (CD-21). Sin disposición, el P-07 de esa reejecución no es
comprobable y `{WORKER_RERUN_RULE}` se retira de la orden con su efecto registrado (README §4, punto 6); esto no es una regla nueva, sino la consecuencia
de no tener tope determinable.

## 3. Reviewer (`claude-subagent` nuevo; una llamada notificada, tope de 60 min)

**Estado: sin autoridad en el contrato T1** (OQ-06). Lo que sigue aplica solo si el Coordinator de I-62 la dispone (CD-06, grupo b) y la orden la
transcribe en `{REVIEWER_AUTHORITY}`.

### 3.1 Autoridad y binding

- Vía 1: `RoleRequirements[].Materialization` del REVIEWER en un contrato reemitido (`Role` REVIEWER, `AuthorizedActions` [REVIEW_CHANGE],
  `MinimumCapabilities`, `RequiredIndependence`, `EligibleCells {Mode LIST|CRITERION, Cells}`, `ModelEffortBounds`, `Permissions` READ_ONLY, `Budget
  {MaxReviewRounds, MaxLogicalRequests, MaxTransportRerunsPerRequest, MaxCorrectionsPerLineage}`, `ObjectFamily {Unit, PathPattern, InitialVersion,
  CorrectionScope}`, `Validity {FromRecordVersion, Until}`); binding AUTHORIZED_MATERIALIZATION con todos los criterios en SATISFIED (AP 16.20; RAE §14.3).
- Vía 2: aceptación individual (A7') con decisión del Coordinator (V14 §20.7), sujeta a OQ-03.
- Independencia: el contrato no fija ninguna; la regla de AP 16.21 que podría pedirlo es «Cableado rutinario acotado: REVIEWER (si se pide)», con todas
  las dimensiones NOT_REQUIRED. Un subagente nuevo tiene otro actor que el Worker, la misma sesión (la de A2) y el mismo proveedor; el Contexto se limita a su
  cierre (sin la transcripción del Worker).
- Perfil y requisitos: routing §1 no tiene fila de REVIEWER; los `RequirementId` los tendría que fijar la autoridad (OQ-06).

### 3.2 Invocación y bucle

`RequestedRole` REVIEWER, `Action` REVIEW_CHANGE, `Permissions` READ_ONLY, `OutputContract` `rackcad-reviewer-result/v1`, `Target` = `{commit G, path,
blob}` (una sola ruta: OQ-06), `LogicalReviewRequestId` y `AttemptSeq` no nulos (bucle de revisión), `BudgetSnapshot` desde `budgets` (A-1 D1-14),
`OpenFindings` = los linajes con `issuer` REVIEWER abiertos (A-1 D1-15). El bucle REVIEWER publica cada fase en un QU ORDINARY (RAE §18.6, §18.2-§18.4),
así que va **fuera** de la ventana (README §3). Identidad de la autoridad: `<path>@<blob>#REVIEWER` (A-1 D1-20); sin `loop.instance_id`; solicitudes con
`loop_instance_id` = `null`, que cuentan en `budgets` (A-1 D1-4).

### 3.3 Salida: `rackcad-reviewer-result/v1`

Campos: `Schema`, `ResultId`, `LogicalReviewRequestId`, `InvocationId`, `AttemptSeq`, `RequestedRole` (REVIEWER), `Action` (REVIEW_CHANGE),
`ReviewedUnit`, `ReviewedCommit`, `ReviewedPath`, `ReviewedBlob`, `ReviewerBinding`, `ReviewerDeclaredIdentity`, `ReviewerMode` (SAME-SESSION ROLE |
SEPARATE SESSION | EXTERNAL HUMAN), `InjectedContextDeclaration {State NONE_DECLARED|DECLARED, Items[]}`, `InputsRead[]`, `InputFidelityEvidenceRef`,
`IndependenceEvidence {Dimensions[] {ReferenceRole, Dimension, Result}, ReviewSubject}`, `KnownLimitations`, `RecommendedNextAction`, `Disposition
NO_FINDINGS|FINDINGS`, `Findings[] {FindingId, LineageId, Severity BLOCKING|ADVISORY, AffectedPath, Evidence, Recommendation}`,
`FindingDispositions[] {…, State OPEN|CLOSED|STILL_OPEN|SUPERSEDED, EvaluatedObject}`, `Recommendations[]`. Sin `Verdict` (el esquema no lo admite).

Validación V1-V11 (RAE §16): esquema = `OutputContract`; `InvocationId` y objeto = los de la invocación; declaración de contexto presente
(`NONE_DECLARED` es falta de declaración); identidad declarada sin contradicción con la observada (P-23); disposiciones solo de linajes REVIEWER; ninguna
lectura fuera del cierre (P-22). Un resultado de REVIEWER nunca satisface al ARCHITECT ni cierra un hallazgo suyo (P-20). REVIEWER_SATISFIED solo con las
cuatro condiciones de A-1 D1-17, publicado en el QU de la ingestión; LOOP_CLOSED (S) desde REVIEWER_SATISFIED o (E) con la decisión
`I62-REVIEWER-LOOP-CLOSE` (A-1 D1-18).

Topes (V14 D.3): Reviewer 1 + 1 reejecución = 2; total de subagentes de la ronda A ≤ 4 (Worker 2 + Reviewer 2).
