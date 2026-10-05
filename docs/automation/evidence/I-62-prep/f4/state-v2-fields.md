# `state/v2`: matriz de campos (preparación de F4; generada por `state-v2-fields.py`)

Fuente: Proposal V14 B.8 (blob `34ad80ea`). La columna A-1 describe la enmienda **propuesta** (no acordada). Lectores: FILE, PAIR, HIST, CLASSIFIER,
NEXT, EXIT, VERIFY, RECON, REBASE, ORCH (ver la cabecera del script).

| Campo | Tipo | Card. | Fuente | Escritor | Puntos | Invariantes | SHA | Rebase (literal) | A-1 | Inválido |
|---|---|---|---|---|---|---|---|---|---|---|
| `schema` | const rackcad-automation-state/v2 | 1 | B.8.1 | P@BOOTSTRAP (T20: adopción); T21 lo sustituye por /v1 | BOOTSTRAP | I-S01, I-P11 | — | — | — | /v2 en una unidad I61; /v2 → /v1 fuera de T21 |
| `automation_state.initiative` | string | 1 | B.8.1; AUTOMATION_PLAN §8 | P@BOOTSTRAP | BOOTSTRAP | I-S01, I-S02, I-P06, I-P11 | — | — | — | cambio tras el BOOTSTRAP (inmutable; = reclamo remoto) |
| `automation_state.branch` | string | 1 | B.8.1; AUTOMATION_PLAN §8 | P@BOOTSTRAP | BOOTSTRAP | I-S01, I-S02, I-P06, I-P11 | — | — | — | cambio tras el BOOTSTRAP (inmutable) |
| `automation_state.claim_id` | UUID | 1 | B.8.1; AUTOMATION_PLAN §8 | P@BOOTSTRAP | BOOTSTRAP | I-S01, I-S02, I-P06, I-P11 | — | — | — | cambio tras el BOOTSTRAP (inmutable; = protocol.basis.claim_id) |
| `automation_state.current_phase` | string | 1 | AUTOMATION_PLAN §8 | P@QU,Q7,QR,QH | Q7, QU, QH, QR | I-S02 | — | — | — | — |
| `automation_state.state` | enum claimed\|implementing\|validating\|ci-failed\|waiting\|review-ready\|integration-ready\|completed | 1 | AUTOMATION_PLAN §8 | P@QU,Q7,QR,QH | Q7, QU, QH, QR | I-S02 | — | — | — | valor fuera del enum (p. ej., blocked) |
| `automation_state.gate` | enum none\|owner-decision\|owner-validation\|autocad\|plugin-build\|ci\|dependency\|conflict\|permissions\|scope | 1 | AUTOMATION_PLAN §8 | P@QU,Q7,QR,QH | Q7, QU, QH, QR | I-S02 | — | — | — | — |
| `automation_state.attempts` | int ≥ 0 | 1 | B.8.1; 16.8; §9.3 | P@Q0 (kind CORRECTION: +1); fuera de la ejecución delegada, QU (§8/§9) | Q0, QU | I-S02, I-S05, I-S12, I-P05, I-P10 | — | — | — | decrece; sube en Q7, QH o QR; sube sin CorrectionLaunch en Q0 |
| `automation_state.next_action` | string (prosa de una línea) | 1 | AUTOMATION_PLAN §8; SM-03 | P@todo punto | BOOTSTRAP, Q0, Q7, QU, QH, QR | I-S02 | — | — | — | nombra un rol o una acción distintos de orchestration.next_action (SM-03, regla de F4) |
| `automation_state.last_evidence_commit` | SHA | 1 | AUTOMATION_PLAN §8; B.8.7 | P@QU,Q7,QR | Q7, QU, QR | I-S02, I-H02, I-P10 | sí | imagen si es un commit reescrito de la rama; sin cambio si es ancestro de main_before (StateFields) | — | commit propio del mismo punto |
| `protocol.set` | const rackcad-protocol/I62 | 1 | B.8.1; §14.1 | P@BOOTSTRAP | BOOTSTRAP | I-S01, I-P06 | — | — | — | I61 en un /v2; cambio |
| `protocol.effective_sha` | SHA (commit de main) | 1 | B.8.1; 16.14 | P@BOOTSTRAP | BOOTSTRAP | I-P06, I-H01 | sí | sin cambio: commit de main, no reescrito | — | cambio; SHA que no es I62_EFFECTIVE_SHA |
| `protocol.basis.claim_id` | UUID | 1 | B.8.1 | P@BOOTSTRAP | BOOTSTRAP | I-S01, I-P06 | — | — | — | ≠ automation_state.claim_id; figura en PRE |
| `protocol.basis.claim_commit` | SHA \| null | 1 | B.8.1; §8.6 | P@BOOTSTRAP | BOOTSTRAP | I-P06 | sí | sin cambio: hecho histórico del reclamo (I-P06) | — | autorreferencia al propio BOOTSTRAP |
| `protocol.basis.claim_parent_sha` | SHA | 1 | B.8.1 | P@BOOTSTRAP | BOOTSTRAP | I-P06, I-H01 | sí | sin cambio: hecho histórico (I-P06) | — | — |
| `protocol.basis.claim_parent_contains_effective` | bool | 1 | B.8.1 | P@BOOTSTRAP | BOOTSTRAP | I-P06, I-H01 | — | — | — | valor que contradice git merge-base --is-ancestor |
| `protocol.basis.adoption_at` | enum G0\|MID_INITIATIVE | 1 | B.8.1; §8.7 | P@BOOTSTRAP | BOOTSTRAP | I-P06 | — | — | — | — |
| `protocol.g0_acceptance.state` | enum PENDING\|ACCEPTED\|REJECTED | 1 | B.8.1; §8.6 (T0) | P@QU (T0, una sola vez) | QU | I-S01, I-S15, I-S16, I-P09 | — | — | — | segunda transición; cambio fuera de un QU; Q0 con PENDING |
| `protocol.g0_acceptance.decision` | StateRef \| null | 1 | B.8.1; §8.6 | P@QU (T0) | QU | I-S13, I-S16 | — | — | — | null con ACCEPTED/REJECTED; sin el marcador I62-CLASSIFICATION o sin el Claim-Id |
| `custody.record_version` | int ≥ 1 | 1 | B.8.1; §8.5 | P@todo punto (+1 exacto, CAS) | BOOTSTRAP, Q0, Q7, QU, QH, QR | I-P01 | — | — | — | salto o repetición |
| `custody.point` | enum BOOTSTRAP\|Q0\|Q7\|QU\|QH\|QR | 1 | B.8.1; §8.1 | P o N@todo punto | BOOTSTRAP, Q0, Q7, QU, QH, QR | I-S03, I-S04, I-S15, I-S17, I-P02 | — | — | — | transición no listada en I-P02 (p. ej., Q0 → QU, QH → Q0) |
| `custody.point_kind` | enum ORDINARY\|REBASE_RECONCILIATION \| null | 1 | B.8.1; §8.8; §8.9 | P@QU; N@QR | QU, QR | I-S17, I-P05, I-P10 | — | — | — | no nulo fuera de QU/QR; REBASE_RECONCILIATION sin last_rebase de este record_version |
| `custody.last_rebase` | {map: StateRef, main_before, main_after, branch_before, branch_after, record_version} \| null | 0..1 | B.8.1; B.8.7; §8.8 | P@QU REBASE_RECONCILIATION; P@Q7/QR que cierra una ventana con rebase; N@QR (T22) | Q7, QU, QR | I-S17, I-P10, I-P12 | sí | se fija en el propio punto de reconciliación | — | record_version ≠ el del punto que reconcilia; map ausente en el árbol |
| `custody.principal.state` | enum HELD\|RELEASED | 1 | B.8.1 | P@QH (RELEASED); N@QR (HELD) | QH, QR | I-S03, I-S04 | — | — | — | RELEASED fuera de QH |
| `custody.principal.binding` | StateRef (binding/v1, UNIT, PRINCIPAL_COORDINATOR) | 1 | B.8.1; B.5; §8.6 | P@BOOTSTRAP; P@QU (T0); N@QR; Q7 (T12a) | BOOTSTRAP, QU, Q7, QR | I-S13, I-S14, I-P07 | — | — | — | Acceptance.State ≠ principal.acceptance.state; cambio fuera de QR, Q7-TRANSFER o el QU de la aceptación |
| `custody.principal.acceptance.state` | enum PENDING\|ACCEPTED\|REJECTED | 1 | B.8.1; §8.6 | P@QU (una vez por titular); N@QR (entra ACCEPTED) | QU, QR, Q7 | I-S01, I-S14, I-S15, I-S16, I-P09 | — | — | — | dos transiciones para el mismo titular |
| `custody.principal.acceptance.decision` | StateRef \| null | 1 | B.8.1; §8.6 | P@QU; N@QR | QU, QR, Q7 | I-S13, I-S16 | — | — | — | null con ACCEPTED/REJECTED; sin el marcador I62-PRINCIPAL-BINDING (o la designación) |
| `custody.principal.preflight` | StateRef (preflight/v1, Action CUSTODY) | 1 | B.8.1; B.4 | P@BOOTSTRAP; N@QR | BOOTSTRAP, QR, Q7 | I-S13 | — | — | — | — |
| `custody.principal.designation` | StateRef \| null | 1 | B.8.1 | N@QR; Q7 (T12a) | QR, Q7 | I-S13, I-S14 | — | — | — | null con since_record_version > 1 |
| `custody.principal.since_record_version` | int ≥ 1 | 1 | B.8.1 | N@QR; Q7 (T12a) | QR, Q7 | I-S14 | — | — | — | — |
| `custody.window.seq` | int ≥ 0 | 1 | B.8.1; §8.2 | P@Q0 (+1) | Q0 | I-S07, I-P04 | — | — | — | Q0 que no suma 1; QU que lo cambia |
| `custody.window.state` | enum CLOSED\|OPENABLE | 1 | B.8.1; §8.1 | P@Q0 (OPENABLE); P@Q7/N@QR (CLOSED) | Q0, Q7, QR | I-S03 | — | — | — | OPENABLE fuera de Q0 |
| `custody.task_intent` | objeto \| null | 0..1 | B.8.1; B.8.3 | P@Q0 (fijado), Q7/QU/QH/QR (siguiente o null) | BOOTSTRAP, Q0, Q7, QU, QH, QR | I-S03, I-S05, I-S06, I-P10 | — | — | — | null en Q0 |
| `custody.task_intent.task_id` | string | 1 | B.8.1 | P@Q0 | Q0 | I-S05 | — | — | — | — |
| `custody.task_intent.attempt` | int ≥ 0 | 1 | B.8.1 | P@Q0 | Q0 | I-S05 | — | — | — | ≠ automation_state.attempts |
| `custody.task_intent.kind` | enum FIRST\|CORRECTION\|RERUN\|REISSUE | 1 | B.8.1 | P@Q0; REBASE_RECONCILIATION → REISSUE o null (I-P10) | Q0, QU | I-S05, I-P10 | — | — | — | — |
| `custody.task_intent.contract` | StateRef (gate-contract/v2) | 1 | B.8.1; §8.8 | P@Q0 | Q0, QU | I-S13 | — | contrato inmutable: con SHAs reescritos o MainSha obsoleto se reemite (REISSUE) | — | — |
| `custody.task_intent.continues_task_id` | string \| null | 1 | B.8.1; §9.3 | P@Q0 | Q0 | — | — | — | — | — |
| `custody.task_intent.planned_roles[]` | {role, binding: StateRef \| null} | 1..n | B.8.1 | P@Q0 | Q0 | I-S06 | — | — | — | role duplicado; sin EXECUTION_CONTROLLER |
| `custody.last_window` | objeto \| null | 0..1 | B.8.1; B.8.3 | P@Q7; N@QR (ABANDONED) | Q7, QR | I-S07, I-S08, I-P03, I-P12 | — | — | — | — |
| `custody.last_window.seq` | int ≥ 1 | 1 | B.8.1 | P@Q7; N@QR | Q7, QR | I-S07 | — | — | — | — |
| `custody.last_window.task_id / attempt` | string / int | 1 | B.8.1 | P@Q7; N@QR | Q7, QR | — | — | — | — | — |
| `custody.last_window.closure` | enum VERIFIED\|REWORK\|BLOCKED\|STOP\|NOT_ACCEPTED\|WITHDRAWN\|ABANDONED | 1 | B.8.1; 16.9 | P@Q7; N@QR (ABANDONED) | Q7, QR | I-S08 | — | — | — | — |
| `custody.last_window.closure_source` | enum CONTROLLER\|SESSION\|COORDINATOR | 1 | B.8.1 | P@Q7; N@QR | Q7, QR | — | — | — | — | — |
| `custody.last_window.delegation_run_id` | string \| "UNKNOWN" \| null | 1 | B.8.1; B.8.5 | P@Q7; N@QR | Q7, QR | I-S08 | — | — | — | no nulo con NOT_ACCEPTED o WITHDRAWN |
| `custody.last_window.verification` | StateRef \| null | 1 | B.8.1 | P@Q7 | Q7 | I-S08, I-S13 | — | — | — | null con VERIFIED |
| `custody.last_window.verified_sha` | SHA \| null | 1 | B.8.1; §8.4 | P@Q7 | Q7 | I-S08, I-H02, I-P10, I-P12 | sí | imagen (StateFields); resultado propio de una ventana con rebase: SHA de la rama rebasada (I-P12) | — | no nulo sin VERIFIED; SHA de su propio commit |
| `custody.last_window.journal` | StateRef \| null | 1 | B.8.1; §8.4 | P@Q7 | Q7, QR | I-S08, I-S13 | — | — | — | null fuera de ABANDONED |
| `custody.last_window.reconstruction` | StateRef \| null | 1 | B.8.1; B.8.5 | N@QR | QR | I-S08, I-S13 | — | — | — | no nulo ⇎ ABANDONED |
| `custody.chains[]` | objeto | 0..n | B.8.1; 16.6; 16.8 | P@Q7 | Q7, QR | I-S09, I-S10, I-P05 | — | — | — | task_id duplicado; QU que las cambia |
| `custody.chains.task_id` | string | 1 | B.8.1 | P@Q7 | Q7 | I-S09 | — | — | — | — |
| `custody.chains.state` | enum IN_COURSE\|VERIFIED\|ABANDONED | 1 | B.8.1; 16.6 | P@Q7; N@QR | Q7, QR | — | — | — | — | — |
| `custody.chains.chain_base_sha` | SHA | 1 | B.8.1; 16.8; §8.4 | P@Q7 (el Q0 de la primera ventana de la tarea) | Q7, QU, QR | I-H02, I-P10, I-P12 | sí | imagen (StateFields), de todas las cadenas sea cual sea su estado | — | SHA de su propio commit |
| `custody.chains.chain_red_sha` | SHA \| null | 1 | B.8.1; 16.8 | P@Q7 | Q7, QU, QR | I-S09, I-H02, I-P10, I-P12 | sí | imagen (StateFields); la corrida de CI del RED se sigue citando por el SHA original (CiRuns) | — | — |
| `custody.chains.chain_red_files[]` | ruta | 0..n | B.8.1; 16.8 | P@Q7 | Q7 | I-S09, I-P05 | — | durable (rutas); no se recalcula | — | decrece; vacío con chain_red_sha no nulo |
| `custody.chains.continues_task_id` | string \| null | 1 | B.8.1 | P@Q7 | Q7 | — | — | — | — | — |
| `custody.chains.correction_authorized_pending` | bool | 1 | B.8.1; G.1 (16.6) | P@Q7, QU | Q7, QU | — | — | — | — | — |
| `custody.unverified_commits[]` | {sha, task_id, window_seq, reason, superseded_by} | 0..n | B.8.1; B.8.5 (R-2); B.8.6 | N@QR (R-2); P@Q7 (superseded_by) | QR, Q7, QU | I-S10, I-H02, I-P10 | sí | sha → imagen; window_seq y superseded_by iguales (StateFields) | — | task_id fuera de chains; superseded_by sin ventana VERIFIED registrada |
| `counters.correction_launches[]` | {seq, task_id, failure_class, correction_of_run_id, attempts_after, record_version} | 0..n | B.8.1; 16.8; §9.3 | P@Q0 (kind CORRECTION) | Q0 | I-S05, I-S12, I-P05, I-P10 | — | — | — | seq no consecutivo; entrada borrada; attempts_after no creciente |
| `counters.blocked_reruns[]` | {task_id, phase, count} | 0..n | B.8.1; 16.8 (P-04) | P@Q7 (desde el diario); N@QR (B.8.5 f) | Q7, QR | I-S11, I-P05 | — | — | — | (task_id, phase) duplicado; count ∉ 0..2; decrece |
| `counters.rebase_recoveries[]` | {task_id, count, last_rebase_map: StateRef} | 0..n | B.8.1; 16.7; §8.8 | P@Q7, QU REBASE_RECONCILIATION | Q7, QU, QR | I-S11, I-P05 | — | last_rebase_map = StateRef nueva si el rebase pertenece a la tarea; count según 16.7 | — | — |
| `counters.invocations[]` | {scope, launched, uncertain, cap, cap_source: StateRef, reconstructed, reconstruction: StateRef \| null} | 0..n | B.8.1; 16.8 (P-07); B.8.5 e | P@Q7; N@QR | Q7, QR | I-S11, I-P05 | — | — | — | scope duplicado; reconstructed ⇎ reconstruction; valores menores que los probados |
| `execution_context` | mapa | 0..1 | B.8.1 | P (descriptivo) | BOOTSTRAP, Q0, Q7, QU, QH, QR | — | — | — | — | (ningún lector del protocolo lo consume) |
| `orchestration.loop.type` | enum NONE\|ARCHITECT_REVIEW\|EXECUTION\|REVIEWER | 1 | B.8.8; §20.5 | P@QU | QU, QR | I-S18 | — | — | FC-01: LOOP_CLOSED la devuelve a NONE | — |
| `orchestration.loop.phase` | enum §20.5 (NONE, REVIEW_PENDING, ARCHITECT_INVOKED, RESULT_INGESTED, CORRECTING, PUBLISHED, CI_VERIFIED, REREVIEW_PENDING, ARCHITECT_SATISFIED, ESCALATE_OWNER) o la fase de §8 (ejecución) | 1 | B.8.8; §20.5 | P@QU | QU, QR | I-S18, I-P13 | — | — | FC-01: transición LOOP_CLOSED {ARCHITECT_SATISFIED \| ESCALATE_OWNER resuelta \| vigencia EXHAUSTED} → NONE | NONE con type ≠ NONE; transición fuera de §20.5 |
| `orchestration.loop.object` | {commit, path, blob} \| null | 1 | B.8.8; §20.5; I-P13 | P@QU (NONE → REVIEW_PENDING; CORRECTING → PUBLISHED) | QU | I-S18, I-P13 | sí | LITERAL: fuera de StateFields e I-H02; I-P13 impide cambiarlo (FC-02) | FC-01: → null solo en LOOP_CLOSED. FC-02: commit en StateFields e I-H02; imagen en REBASE_RECONCILIATION con path y blob iguales | cambio en REREVIEW_PENDING → ARCHITECT_INVOKED (cambio tardío); null con bucle activo |
| `orchestration.loop.authorization` | StateRef \| null | 1 | B.8.8; §20.5; §20.5.1 | P@QU (al abrir) | QU | I-S13, I-S18 | — | — | FC-01: null en LOOP_CLOSED; un bucle nuevo exige una autorización nueva | — |
| `orchestration.loop.action_validity` | {authorization_id, state OPEN\|ENDED, ended_at, ended_utc, ended_reason ARCHITECT_SATISFIED\|EXHAUSTED\|EXPIRED\|REVOKED\|SUPERSEDED, ended_by} \| null | 1 | B.8.8; §20.5; §20.5.1 | P@QU (OPEN al abrir; ENDED una sola vez) | QU | I-S18, I-P13 | — | — | FC-01: null en LOOP_CLOSED; SUPERSEDED solo con el bucle abierto | ENDED → OPEN; dos fines para la misma autorización |
| `orchestration.next_action` | {role, action, target, unit, gate, task_id, required_inputs[], required_capabilities[], required_independence, invocation_permission, budget_remaining, expected_output, completion_condition, stop_conditions[], escalation_conditions[]} | 1 | B.8.8; §20.4 | P@todo punto | BOOTSTRAP, Q0, Q7, QU, QH, QR | I-S18 | — | — | — | no derivable de forma única (P-17); role OWNER/COORDINATOR con escalation NONE |
| `orchestration.review_requests[]` | objeto | 0..n | B.8.8; §20.6 | P@QU | QU, QR | I-S18, I-P13 | — | — | — | logical_review_request_id duplicado; dos OPEN en un bucle; solicitud borrada |
| `orchestration.review_requests.logical_review_request_id` | string ^L…$ | 1 | B.8.8; §20.6; B.2 | P@QU | QU | I-S18 | — | — | — | — |
| `orchestration.review_requests.authorization_id` | string | 1 | A-1 D1-2 (no congelado) | P@QU (al abrir; inmutable) | QU | — | — | — | FC-01: campo NUEVO; las igualdades de presupuesto se calculan por autorización | — |
| `orchestration.review_requests.object` | {commit, path, blob} | 1 | B.8.8; §20.6 | P@QU (= loop.object al abrir; inmutable) | QU | I-S18, I-P13 | sí | LITERAL: inmutable y fuera de StateFields (FC-02) | FC-02: commit de las solicitudes OPEN en StateFields; imagen con path y blob iguales; las terminales no se reescriben | — |
| `orchestration.review_requests.round` | int ≥ 1 | 1 | B.8.8; §20.6 | P@QU | QU | — | — | — | — | — |
| `orchestration.review_requests.state` | enum OPEN\|INGESTED\|EXHAUSTED\|CANCELLED | 1 | B.8.8; §20.6 | P@QU | QU | I-S18 | — | — | FC-01: LOOP_CLOSED exige ninguna OPEN | — |
| `orchestration.review_requests.attempts[]` | objeto | 1..n | B.8.8; §20.6 | P@QU | QU | I-S18, I-P13 | — | — | — | attempt_seq no consecutivo; dos no terminales; intento borrado |
| `…attempts.attempt_seq` | int ≥ 1 | 1 | B.8.8; §20.6 | P@QU | QU | I-S18 | — | — | — | — |
| `…attempts.invocation` | StateRef (role-invocation/v1 con Target) | 1 | B.8.8; §20.6; B.9 | P@QU (inmutable desde LAUNCHING) | QU | I-S13, I-P13 | sí | LITERAL: el Target.commit de un intento no lanzado queda obsoleto (FC-02) | FC-02: intento no lanzado → replanificado (invocation nueva, Target = imagen, misma reserva); LAUNCHING o posterior conserva su Target histórico | invocation nueva desde LAUNCHING |
| `…attempts.binding` | StateRef (binding/v1) | 1 | B.8.8; §20.6; B.5 | P@QU | QU | I-S13, I-S18 | — | — | — | materializado sin AuthorizationRef o con un criterio no SATISFIED |
| `…attempts.state` | enum INVOCATION_PLANNED\|BUDGET_RESERVED\|LAUNCHING\|LAUNCHED\|LAUNCH_UNCERTAIN\|RESULT_RECEIVED\|RESULT_INGESTED\|CANCELLED_BEFORE_LAUNCH | 1 | B.8.8; §20.6 | P@QU | QU | I-S18, I-P13 | — | — | FC-02: INVOCATION_PLANNED → INVOCATION_PLANNED (replanificado) solo en REBASE_RECONCILIATION | transición fuera de I-P13; cambio de un terminal |
| `…attempts.reserved_at` | int \| null | 1 | B.8.8; §20.6 | P@QU (primera BUDGET_RESERVED) | QU | I-S18, I-P13 | — | — | — | cambio tras fijarse |
| `…attempts.run_id` | string \| "UNKNOWN" \| null | 1 | B.8.8; §20.6 | P@QU (LAUNCHING) | QU | I-S18 | — | — | — | null en LAUNCHING o posterior |
| `…attempts.launch_evidence / not_started_evidence / result / runtime_evidence / read_audit / input_fidelity / premise_independence` | StateRef \| null | 1 | B.8.8; §20.6; §20.3 | P@QU | QU | I-S13, I-S18 | — | — | — | result no nulo en CANCELLED_BEFORE_LAUNCH; premise_independence null con DEGRADED_BOUNDED |
| `…attempts.output_state / fidelity_status / outcome / unaccredited[]` | enums de B.8.8 | 1 | B.8.8; §20.6; §20.3.3 | P@QU | QU | I-S18 | — | — | — | — |
| `…attempts.reserved_utc / launching_utc` | instante \| null | 1 | B.8.8; §20.6; §20.5.1 | P@QU | QU | I-P13 | — | — | — | acción nueva con instante > Until |
| `…attempts.ingested_at` | int \| null | 1 | B.8.8; §20.6 | P@QU | QU | I-S18, I-P13 | — | — | — | no nulo en CANCELLED_BEFORE_LAUNCH |
| `orchestration.findings[]` | {lineage_id, finding_ids[], issuer, opened_in, severity, class, affected_section, state, response, last_disposition_in, omitted_in[], closed_by, downgraded_by} | 0..n | B.8.8; §20.5.2 | P@QU (RESULT_INGESTED) | QU | I-S18, I-P13 | — | — | FC-01: sin cambio (de unidad; un bucle nuevo hereda los linajes abiertos) | cerrado sin resultado con autoridad; cerrado por reviewer-result; linaje borrado |
| `orchestration.budgets` | {review_rounds, logical_requests, architect_launches, transport_reruns[], correction_rounds, corrections_by_lineage[], caps} | 1 | B.8.8; §20.6 | P@QU (reserva; PUBLISHED) | QU | I-S18, I-P13 | — | sin cambio en la reconciliación (I-P10) | FC-01: pasa a budgets[] {authorization_id, …}, append-only, una entrada por autorización | contador que decrece o supera su tope; reinicio por cambio de proveedor |
| `orchestration.budgets[].authorization_id` | string | 1 | A-1 D1-1 (no congelado) | P@QU (NONE → REVIEW_PENDING con autorización nueva) | QU | — | — | — | FC-01: campo NUEVO; único; una autorización no se reutiliza | — |
| `orchestration.escalation` | {state NONE\|OWNER\|COORDINATOR, reason, required_decision} | 1 | B.8.8; §20.1 | P@QU | QU | I-S18 | — | — | FC-01: NONE en LOOP_CLOSED | NONE con next_action.role OWNER/COORDINATOR |
| `orchestration.autonomy_gaps[]` | StateRef | 0..n | B.8.8; §20.9 | P@QU | QU | I-S13 | — | — | — | — |

**Resumen (MEASURED):** 91 campos; 12 con SHA; 14 afectados por A-1.

- SHA en la lista literal de `StateFields`: `automation_state.last_evidence_commit`, `custody.last_window.verified_sha`, `custody.chains.chain_base_sha`, `custody.chains.chain_red_sha`, `custody.unverified_commits[]`.
- SHA **fuera** de esa lista: `protocol.effective_sha`, `protocol.basis.claim_commit`, `protocol.basis.claim_parent_sha`, `custody.last_rebase`, `orchestration.loop.object`, `orchestration.review_requests.object`, `…attempts.invocation`. Los de `protocol.*` son hechos históricos o commits de `main` (I-P06, §8.8); `last_rebase` se fija en el propio punto; los de `orchestration` son exactamente el hueco FC-02.
