# FX-06 — estructura y campos de la evidencia de la corrida (SOLO SUPERVISIÓN; plantilla)

```text
Autoridades: V14 B.8.8 (StateRef de cada intento), B.9, B.10, §20.3.1-§20.3.3, §20.9, D.4, D.6, D.8; README §17 (custodia), §18 (bucles); práctica
             de la evidencia de F6 (FX-01, FX-04a, FX-U1-chain, OD-2: result.json por corrida, SHA-256 antes de comparar con un oráculo).
Reglas:      UTF-8 y LF; JSON con claves en el orden de esta tabla; SHA de Git de 40 hex y SHA-256 de 64 hex en minúsculas; instantes ISO-8601 con Z;
             desconocido = estado explícito o "UNKNOWN", nunca null (B.1); ningún valor de configuración, credencial ni transcripción en claro;
             ninguna referencia del plano real dentro del fixture (P-16).
```

## 1. Dos lados de la custodia

| Lado | Quién escribe | Qué | Dónde |
|---|---|---|---|
| fixture (plano c) | el Principal de FX-06 (y el Coordinator del fixture para órdenes, RLA y medición) | el estado `/v2` en cada punto, los artefactos citados por `StateRef`, el objeto X y sus versiones | rama `fx/u1` de `D:\r62-fixture\fixture-origin.git` (y `github`) |
| RackCad (plano a) | la sesión de supervisión | índice de la corrida, referencias exactas a la custodia del fixture (commit, ruta, blob), auditorías, comparación con el oráculo y propuesta de clasificación | `docs/automation/evidence/I-62-F6/FX-06/R<AAAAMMDDTHHMMSSZ>-fx06/` (`R:`) |

## 2. Custodia en el fixture (rutas relativas a la raíz de `fx/u1`; `<U>` = unidad)

| Ruta | Esquema o forma | Productor | Punto que la custodia |
|---|---|---|---|
| `docs/automation/decisions/<U>.md` (orden de FX-06, RLA, designación del titular) | bloques con marcadores de 16.28 | Coordinator del fixture | antes del paso 1 |
| `docs/automation/evidence/<U>-agent/measurements/<RunId>.json` | registro saneado de la re-medición de la celda (destino del `RunRef`; OQ-14) | Coordinator del fixture | antes del paso 1 |
| `docs/automation/evidence/<U>-agent/fx06-principal/<PreflightId>.json`, `.validation.json`, `<BindingId>.json` | `preflight/v1`, validación de README §13.2, `binding/v1` | Principal | commit de observación y propuesta; QR |
| `docs/initiatives/<U>-proposal.md` | X v1 y después X2 | Principal | commits propios (pasos 1 y 5a) |
| `docs/automation/evidence/<U>-agent/review/preflight/<PreflightId>.json` (+ `.validation.json`) | `preflight/v1` de la candidata (REVIEW_DESIGN) | Principal | QU de la reserva |
| `docs/automation/evidence/<U>-agent/review/<BindingId>.json` | `binding/v1` materializado (B y C) | Principal | QU de la reserva |
| `docs/automation/evidence/<U>-agent/review/<L>/<n>/invocation.json` | `role-invocation/v1` | Principal | QU de la reserva |
| `…/review/<L>/<n>/closure.json` | `input-closure/v1` | Principal | QU de la reserva |
| `…/review/<L>/<n>/prompt.md` | prompt renderizado (insumo `PROMPT` de la fidelidad) | Principal (adapter) | QU de la reserva |
| `…/review/<L>/<n>/input-fidelity-preflight.json` | `input-fidelity/v1`, `Kind` FIDELITY, `Phase` PREFLIGHT | Principal | QU de la reserva |
| `…/review/<L>/<n>/launch.json` | `relay-record/v2` del arranque (OQ-11) | Principal | QU LAUNCHED (variante A de la receta); con la variante L no hay QU LAUNCHED y el registro se custodia en el QU RESULT_RECEIVED, citado por `runtime_evidence` (§20.3.2) |
| `…/review/<L>/<n>/result.json` | `architect-review-result/v1` (salida `-o`, bytes exactos) | Architect; custodia el Principal | QU RESULT_RECEIVED |
| `…/review/<L>/<n>/runtime-evidence.json` | §3 de este documento | Principal | QU RESULT_RECEIVED |
| `…/review/<L>/<n>/read-audit.json` | §3 | Principal | QU RESULT_RECEIVED |
| `…/review/<L>/<n>/input-fidelity-postrun.json` (+ `span-map.json`, `premise-independence.json` si hay degradación) | `input-fidelity/v1` FIDELITY POSTRUN, SPAN_MAP, PREMISE_INDEPENDENCE | Principal | QU RESULT_RECEIVED |
| `…/review/<L>/<n>/autonomy-gap.json` | registro AUTONOMY_GAP (README §18.7; F4 `AutonomyGaps.Record`) | Principal | QU siguiente al relevo (si lo hubiera) |
| `…/review/responses/<LineageId>.md` | respuesta del Principal por linaje | Principal | QU CORRECTING o PUBLISHED (OQ-12) |
| `…/review/ci/<sha-de-X2>.json` | registro de CI-5 ([ci-checkpoints.md](ci-checkpoints.md) §3) | Principal | QU CI_VERIFIED |
| `docs/automation/state/<U>.yml` | `rackcad-automation-state/v2` | Principal | cada punto |

No se versionan (solo identidad y campos extraídos, como en la revisión de V14): `events.jsonl` de `--json`, el registro de sesión de Codex
(`~/.codex/sessions/…`), `stderr`, la transcripción de la sesión del Principal.

## 3. Campos de los registros que no tienen esquema propio

**`runtime-evidence.json`** (`RuntimeEvidenceRef`, §20.3.2). §20.3.2 dice que la identidad observada se custodia en el `relay-record/v2` del intento
(`Participant.Observation`) y que el estado la referencia como `RuntimeEvidenceRef`; mientras OQ-11 no decida cómo se construye un `relay-record/v2` válido
para una revisión, esta forma propia lleva además lo que el auditor necesita para probar el lanzamiento cuando no hay `launch_evidence` (variante L, o
LAUNCHING → RESULT_RECEIVED):

| Campo | Contenido |
|---|---|
| `RunId`, `InvocationId`, `LogicalReviewRequestId`, `AttemptSeq` | identidades del intento |
| `Role`, `AdapterId`, `Transport` | ARCHITECT, `codex-cli`, `external-process` |
| `LaunchedPid`, `LaunchCreationDateUtc` | proceso lanzado por el Principal (op. 7: PID + `CreationDate`) |
| `OutputPath`, `OutputSha256` | salida `-o` tal como la escribió el proceso; su SHA-256 = el del `result.json` custodiado |
| `ThreadId` | `session_meta.id` del registro de sesión (`ActorRef.InstanceId` de `codex-cli`) |
| `SessionLog` | `{FileName, Sha256, Lines}` (sin contenido) |
| `CliVersion`, `BinarySha256`, `AppVersion` | observados |
| `TurnContext[]` | `{Model, Effort, ApprovalPolicy, SandboxPolicy, Cwd}` de cada `turn_context` |
| `StartUtc`, `EndUtc`, `ExitCode`, `TurnCompleted` | del proceso y de los eventos |
| `Compactions[]` | `{Utc, BeforeCallIndex, Preserved}` (GAP-11); el resumen cifrado no se cita |
| `RuntimeDiagnostics[]` | `{CallIndex, Position (START \| END), Line}` (GAP-12) |
| `Usage`, `LimitSignals` | tokens y señales de límite (`rate_limit_reached_type`, `spend_control_reached`) |
| `Fingerprint` | `{Before, After}` SHA-256 de `config.toml`; distinto → P-01 |

**`read-audit.json`** (§20.3.1; `relay-record/v2.ReadAudit` resume `{Coverage, ReadsOutsideClosure}`):

| Campo | Contenido |
|---|---|
| `Coverage` | RECORDED \| PARTIAL \| EMPTY \| UNAVAILABLE (cobertura declarada: no capta lecturas internas del runtime) |
| `Calls[]` | `{Index, Command, Form (A \| B \| C \| GIT \| OTHER), Paths[], InClosure, DeliveredTruncated, Failed}` |
| `ReadsOutsideClosure[]` | rutas leídas fuera del cierre (P-22) |
| `LinesNeverDelivered[]` | `{Path, LineStart, LineEnd}` pedidas y nunca entregadas íntegras |

## 4. Índice de la corrida en RackCad (`R:`)

| Archivo | Campos | Cuándo |
|---|---|---|
| `README.md` | resumen: autoridad, unidad, commits, corridas, resultado propuesto, límites | al final |
| `prerequisites.json` | `{FX04aClassification, OD2d, OD2dProbe, FX02Q7, CoordinatorDispositions[]}`: referencias exactas (decisión, commit, archivo) | antes de abrir la sesión |
| `prelaunch.json` | `{ObservedUtc, ConfigToml {Sha256, KeyNameCount}, Binary {Label, Sha256, Unique}, App, Arch {Path, Head, Clean, Remotes, Processes}, PrincipalFolder {Path, Head, Remotes, Identity}, ClaudeAutomaticInputs [{Kind, Path, Sha256 \| State}], CodexAutomaticInputs [{Kind, Path, Sha256 \| State}]}` | justo antes de P7 y repetido antes del primer LAUNCHING |
| `prepublish-scan.json` | salida de `prepublish_scan.py` (`fx06-prepublish-scan-result/v1`: por archivo, SHA-256, líneas con token por índice y líneas con indicio) sobre la orden, la RLA, el registro de la medición y el texto del objeto, más la disposición de la supervisión para cada indicio | antes del push de P5 |
| `rla.json` | `{AuthorizationId, Commit, DecisionsBlob, BlockSha256, Fields {…}}`; `Fields` con las líneas `Clave: valor` del bloque | tras P5 |
| `chain.json` | `{Unit, Branch, Commits [{Sha, By, What, Point, RecordVersion, Ci {Run, Event, Ref, Jobs, Conclusion} \| null}]}` (forma de `FX-U1-chain/chain.json`) | al final |
| `validator/<prev>-<next>.json` | salida de `run_validator.ps1` (archivo, par, historia, historia del par, B1) | por cada par |
| `attempts/<L>-<n>.json` | `{RunId, ThreadId, Model, Effort, Sandbox, StartUtc, EndUtc, ExitCode, Outcome, FidelityStatus, Refs {Invocation, Binding, Closure, Prompt, FidelityPreflight, Launch, Result, RuntimeEvidence, ReadAudit, FidelityPostrun}}`, cada `Ref` = `{Commit, Path, Blob}` del fixture | tras cada intento |
| `results-hashes.json` | `{Results [{L, n, Path, Blob, Sha256}]}` | **antes** de cargar el oráculo, en un commit durable de RackCad con su CI (práctica de FX-04a) |
| `ci.json` | contraste independiente de CI-5 por la supervisión | tras CI_VERIFIED |
| `human-events.json` | `HumanEvents[]` del auditor (§6 de [message-bus-auditor.md](message-bus-auditor.md)) con la cobertura declarada | tras la corrida |
| `transport-audit-input.json`, `transport-audit-result.json` | entrada (`fx06-transport-audit-input/v2`) y salida del auditor (con `InputSha256`); los registros de prueba del lanzamiento salen de `launch_evidence` o `runtime_evidence` de cada intento | tras la corrida |
| `oracle-comparison.json` | `{OracleSha256, OracleVerifiedSha256, Fields [{Name, Oracle, Observed, Equal}], Result}` | después de `results-hashes.json` durable |
| `termination.json` | `{Session, Observations [{Utc, isRunning, lastActivityAt}], Basis}` | si la sesión termina |
| `classification-proposal.json` | `{Scenario: FX-06, Obligation: C-39, Steps [{Step 1..8, Done, Evidence}], OwnerAsMessageBus, PassEligibleOnTransport, IntermediateCoordinatorDecisions, AutonomyGaps, Proposed (PASS \| FAIL \| UNVERIFIED \| UNSUPPORTED), Causes[], OpenQuestions[]}` | al final; la clasificación la decide el Coordinator |

## 5. Orden de publicación (para que nada se ajuste a posteriori)

1. `prerequisites.json`, `prepublish-scan.json`, `prelaunch.json` y `rla.json` antes de abrir la sesión del Principal.
2. `results-hashes.json` durable antes de leer el oráculo.
3. `transport-audit-*` y `oracle-comparison.json` después; ninguno se reescribe tras ver el otro.
4. `classification-proposal.json` con las causas exactas; el resultado lo fija el Coordinator (D.4; decisiones §46).
