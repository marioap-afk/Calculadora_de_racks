"""I-62 F4 preparation: machine-readable field matrix of `rackcad-automation-state/v2` (Proposal V14 B.8.1, B.8.3, B.8.4, B.8.7, B.8.8; §8; §20.4-§20.6).

Not the state/v2 schema and not production: a specification aid so that F4 can implement the schema, the file validator (C-18/C-38) and the pair
validator without re-deriving fields. Every row cites its frozen clause. A-1 columns describe the PROPOSED amendment (docs/initiatives/I-62-A-1.md),
which is not agreed.

Columns: Path, Type, Card, Source, Writer (actor@points where the value may change), Readers, Points (durable points where it changes), Invariants,
Sha (SHA-bearing), Rebase (literal V14 treatment in a REBASE_RECONCILIATION), A1 (proposed change), Invalid (examples the validator must reject).

Readers: FILE = file validator (I-S*, C-18/C-38); PAIR = pair validator (I-P*); HIST = invariants with history (I-H*, C-15); CLASSIFIER = E.2;
NEXT = NextAction derivation (§20.4); EXIT = Exit/Entry of 16.4; VERIFY = Controller verification (16.9); RECON = reconstruction (B.8.5);
REBASE = §8.8/§8.9; ORCH = orchestration loop (§20.5-§20.6).

Usage: python state-v2-fields.py <out.json> <out.md>
"""
import json
import sys

ALL = ["BOOTSTRAP", "Q0", "Q7", "QU", "QH", "QR"]
rows = []


def F(path, typ, card, src, writer, readers, points, inv, sha=False, rebase="—", a1="—", invalid=""):
    rows.append({"Path": path, "Type": typ, "Card": card, "Source": src, "Writer": writer, "Readers": readers, "Points": points, "Invariants": inv,
                 "Sha": sha, "Rebase": rebase, "A1": a1, "Invalid": invalid})


# ---------------------------------------------------------------- header and automation_state (AUTOMATION_PLAN §8, unchanged)
F("schema", "const rackcad-automation-state/v2", "1", "B.8.1", "P@BOOTSTRAP (T20: adopción); T21 lo sustituye por /v1", ["FILE", "CLASSIFIER"], ["BOOTSTRAP"],
  ["I-S01", "I-P11"], invalid="/v2 en una unidad I61; /v2 → /v1 fuera de T21")
for f, t, rule in [("initiative", "string", "inmutable; = reclamo remoto"), ("branch", "string", "inmutable"), ("claim_id", "UUID", "inmutable; = protocol.basis.claim_id")]:
    F("automation_state." + f, t, "1", "B.8.1; AUTOMATION_PLAN §8", "P@BOOTSTRAP", ["FILE", "PAIR", "CLASSIFIER"], ["BOOTSTRAP"], ["I-S01", "I-S02", "I-P06", "I-P11"],
      invalid="cambio tras el BOOTSTRAP (" + rule + ")")
F("automation_state.current_phase", "string", "1", "AUTOMATION_PLAN §8", "P@QU,Q7,QR,QH", ["FILE", "NEXT"], ["Q7", "QU", "QH", "QR"], ["I-S02"])
F("automation_state.state", "enum claimed|implementing|validating|ci-failed|waiting|review-ready|integration-ready|completed", "1", "AUTOMATION_PLAN §8",
  "P@QU,Q7,QR,QH", ["FILE"], ["Q7", "QU", "QH", "QR"], ["I-S02"], invalid="valor fuera del enum (p. ej., blocked)")
F("automation_state.gate", "enum none|owner-decision|owner-validation|autocad|plugin-build|ci|dependency|conflict|permissions|scope", "1", "AUTOMATION_PLAN §8",
  "P@QU,Q7,QR,QH", ["FILE"], ["Q7", "QU", "QH", "QR"], ["I-S02"])
F("automation_state.attempts", "int ≥ 0", "1", "B.8.1; 16.8; §9.3", "P@Q0 (kind CORRECTION: +1); fuera de la ejecución delegada, QU (§8/§9)", ["FILE", "PAIR", "VERIFY"],
  ["Q0", "QU"], ["I-S02", "I-S05", "I-S12", "I-P05", "I-P10"], invalid="decrece; sube en Q7, QH o QR; sube sin CorrectionLaunch en Q0")
F("automation_state.next_action", "string (prosa de una línea)", "1", "AUTOMATION_PLAN §8; SM-03", "P@todo punto", ["FILE"], ALL, ["I-S02"],
  invalid="nombra un rol o una acción distintos de orchestration.next_action (SM-03, regla de F4)")
F("automation_state.last_evidence_commit", "SHA", "1", "AUTOMATION_PLAN §8; B.8.7", "P@QU,Q7,QR", ["FILE", "HIST", "REBASE"], ["Q7", "QU", "QR"], ["I-S02", "I-H02", "I-P10"],
  sha=True, rebase="imagen si es un commit reescrito de la rama; sin cambio si es ancestro de main_before (StateFields)", invalid="commit propio del mismo punto")

# ---------------------------------------------------------------- protocol
F("protocol.set", "const rackcad-protocol/I62", "1", "B.8.1; §14.1", "P@BOOTSTRAP", ["FILE", "CLASSIFIER", "PAIR"], ["BOOTSTRAP"], ["I-S01", "I-P06"],
  invalid="I61 en un /v2; cambio")
F("protocol.effective_sha", "SHA (commit de main)", "1", "B.8.1; 16.14", "P@BOOTSTRAP", ["FILE", "CLASSIFIER", "HIST"], ["BOOTSTRAP"], ["I-P06", "I-H01"], sha=True,
  rebase="sin cambio: commit de main, no reescrito", invalid="cambio; SHA que no es I62_EFFECTIVE_SHA")
F("protocol.basis.claim_id", "UUID", "1", "B.8.1", "P@BOOTSTRAP", ["FILE", "CLASSIFIER"], ["BOOTSTRAP"], ["I-S01", "I-P06"], invalid="≠ automation_state.claim_id; figura en PRE")
F("protocol.basis.claim_commit", "SHA | null", "1", "B.8.1; §8.6", "P@BOOTSTRAP", ["FILE", "CLASSIFIER"], ["BOOTSTRAP"], ["I-P06"], sha=True,
  rebase="sin cambio: hecho histórico del reclamo (I-P06)", invalid="autorreferencia al propio BOOTSTRAP")
F("protocol.basis.claim_parent_sha", "SHA", "1", "B.8.1", "P@BOOTSTRAP", ["FILE", "CLASSIFIER", "HIST"], ["BOOTSTRAP"], ["I-P06", "I-H01"], sha=True,
  rebase="sin cambio: hecho histórico (I-P06)")
F("protocol.basis.claim_parent_contains_effective", "bool", "1", "B.8.1", "P@BOOTSTRAP", ["FILE", "CLASSIFIER", "HIST"], ["BOOTSTRAP"], ["I-P06", "I-H01"],
  invalid="valor que contradice git merge-base --is-ancestor")
F("protocol.basis.adoption_at", "enum G0|MID_INITIATIVE", "1", "B.8.1; §8.7", "P@BOOTSTRAP", ["FILE", "CLASSIFIER"], ["BOOTSTRAP"], ["I-P06"])
F("protocol.g0_acceptance.state", "enum PENDING|ACCEPTED|REJECTED", "1", "B.8.1; §8.6 (T0)", "P@QU (T0, una sola vez)", ["FILE", "PAIR", "CLASSIFIER"], ["QU"],
  ["I-S01", "I-S15", "I-S16", "I-P09"], invalid="segunda transición; cambio fuera de un QU; Q0 con PENDING")
F("protocol.g0_acceptance.decision", "StateRef | null", "1", "B.8.1; §8.6", "P@QU (T0)", ["FILE", "CLASSIFIER"], ["QU"], ["I-S13", "I-S16"],
  invalid="null con ACCEPTED/REJECTED; sin el marcador I62-CLASSIFICATION o sin el Claim-Id")

# ---------------------------------------------------------------- custody
F("custody.record_version", "int ≥ 1", "1", "B.8.1; §8.5", "P@todo punto (+1 exacto, CAS)", ["FILE", "PAIR"], ALL, ["I-P01"], invalid="salto o repetición")
F("custody.point", "enum BOOTSTRAP|Q0|Q7|QU|QH|QR", "1", "B.8.1; §8.1", "P o N@todo punto", ["FILE", "PAIR", "EXIT", "CLASSIFIER"], ALL,
  ["I-S03", "I-S04", "I-S15", "I-S17", "I-P02"], invalid="transición no listada en I-P02 (p. ej., Q0 → QU, QH → Q0)")
F("custody.point_kind", "enum ORDINARY|REBASE_RECONCILIATION | null", "1", "B.8.1; §8.8; §8.9", "P@QU; N@QR", ["FILE", "PAIR", "REBASE"], ["QU", "QR"], ["I-S17", "I-P05", "I-P10"],
  invalid="no nulo fuera de QU/QR; REBASE_RECONCILIATION sin last_rebase de este record_version")
F("custody.last_rebase", "{map: StateRef, main_before, main_after, branch_before, branch_after, record_version} | null", "0..1", "B.8.1; B.8.7; §8.8",
  "P@QU REBASE_RECONCILIATION; P@Q7/QR que cierra una ventana con rebase; N@QR (T22)", ["FILE", "PAIR", "REBASE", "HIST"], ["Q7", "QU", "QR"],
  ["I-S17", "I-P10", "I-P12"], sha=True, rebase="se fija en el propio punto de reconciliación", invalid="record_version ≠ el del punto que reconcilia; map ausente en el árbol")
F("custody.principal.state", "enum HELD|RELEASED", "1", "B.8.1", "P@QH (RELEASED); N@QR (HELD)", ["FILE", "EXIT"], ["QH", "QR"], ["I-S03", "I-S04"], invalid="RELEASED fuera de QH")
F("custody.principal.binding", "StateRef (binding/v1, UNIT, PRINCIPAL_COORDINATOR)", "1", "B.8.1; B.5; §8.6", "P@BOOTSTRAP; P@QU (T0); N@QR; Q7 (T12a)",
  ["FILE", "PAIR"], ["BOOTSTRAP", "QU", "Q7", "QR"], ["I-S13", "I-S14", "I-P07"], invalid="Acceptance.State ≠ principal.acceptance.state; cambio fuera de QR, Q7-TRANSFER o el QU de la aceptación")
F("custody.principal.acceptance.state", "enum PENDING|ACCEPTED|REJECTED", "1", "B.8.1; §8.6", "P@QU (una vez por titular); N@QR (entra ACCEPTED)", ["FILE", "PAIR"],
  ["QU", "QR", "Q7"], ["I-S01", "I-S14", "I-S15", "I-S16", "I-P09"], invalid="dos transiciones para el mismo titular")
F("custody.principal.acceptance.decision", "StateRef | null", "1", "B.8.1; §8.6", "P@QU; N@QR", ["FILE"], ["QU", "QR", "Q7"], ["I-S13", "I-S16"],
  invalid="null con ACCEPTED/REJECTED; sin el marcador I62-PRINCIPAL-BINDING (o la designación)")
F("custody.principal.preflight", "StateRef (preflight/v1, Action CUSTODY)", "1", "B.8.1; B.4", "P@BOOTSTRAP; N@QR", ["FILE"], ["BOOTSTRAP", "QR", "Q7"], ["I-S13"])
F("custody.principal.designation", "StateRef | null", "1", "B.8.1", "N@QR; Q7 (T12a)", ["FILE"], ["QR", "Q7"], ["I-S13", "I-S14"], invalid="null con since_record_version > 1")
F("custody.principal.since_record_version", "int ≥ 1", "1", "B.8.1", "N@QR; Q7 (T12a)", ["FILE"], ["QR", "Q7"], ["I-S14"])
F("custody.window.seq", "int ≥ 0", "1", "B.8.1; §8.2", "P@Q0 (+1)", ["FILE", "PAIR", "EXIT"], ["Q0"], ["I-S07", "I-P04"], invalid="Q0 que no suma 1; QU que lo cambia")
F("custody.window.state", "enum CLOSED|OPENABLE", "1", "B.8.1; §8.1", "P@Q0 (OPENABLE); P@Q7/N@QR (CLOSED)", ["FILE", "EXIT"], ["Q0", "Q7", "QR"], ["I-S03"],
  invalid="OPENABLE fuera de Q0")
F("custody.task_intent", "objeto | null", "0..1", "B.8.1; B.8.3", "P@Q0 (fijado), Q7/QU/QH/QR (siguiente o null)", ["FILE", "EXIT", "NEXT"], ALL, ["I-S03", "I-S05", "I-S06", "I-P10"],
  invalid="null en Q0")
F("custody.task_intent.task_id", "string", "1", "B.8.1", "P@Q0", ["FILE"], ["Q0"], ["I-S05"])
F("custody.task_intent.attempt", "int ≥ 0", "1", "B.8.1", "P@Q0", ["FILE"], ["Q0"], ["I-S05"], invalid="≠ automation_state.attempts")
F("custody.task_intent.kind", "enum FIRST|CORRECTION|RERUN|REISSUE", "1", "B.8.1", "P@Q0; REBASE_RECONCILIATION → REISSUE o null (I-P10)", ["FILE", "PAIR"], ["Q0", "QU"],
  ["I-S05", "I-P10"])
F("custody.task_intent.contract", "StateRef (gate-contract/v2)", "1", "B.8.1; §8.8", "P@Q0", ["FILE", "REBASE"], ["Q0", "QU"], ["I-S13"],
  rebase="contrato inmutable: con SHAs reescritos o MainSha obsoleto se reemite (REISSUE)")
F("custody.task_intent.continues_task_id", "string | null", "1", "B.8.1; §9.3", "P@Q0", ["FILE"], ["Q0"], [])
F("custody.task_intent.planned_roles[]", "{role, binding: StateRef | null}", "1..n", "B.8.1", "P@Q0", ["FILE", "EXIT"], ["Q0"], ["I-S06"],
  invalid="role duplicado; sin EXECUTION_CONTROLLER")
F("custody.last_window", "objeto | null", "0..1", "B.8.1; B.8.3", "P@Q7; N@QR (ABANDONED)", ["FILE", "PAIR", "NEXT"], ["Q7", "QR"], ["I-S07", "I-S08", "I-P03", "I-P12"])
F("custody.last_window.seq", "int ≥ 1", "1", "B.8.1", "P@Q7; N@QR", ["FILE"], ["Q7", "QR"], ["I-S07"])
F("custody.last_window.task_id / attempt", "string / int", "1", "B.8.1", "P@Q7; N@QR", ["FILE"], ["Q7", "QR"], [])
F("custody.last_window.closure", "enum VERIFIED|REWORK|BLOCKED|STOP|NOT_ACCEPTED|WITHDRAWN|ABANDONED", "1", "B.8.1; 16.9", "P@Q7; N@QR (ABANDONED)", ["FILE", "NEXT"],
  ["Q7", "QR"], ["I-S08"])
F("custody.last_window.closure_source", "enum CONTROLLER|SESSION|COORDINATOR", "1", "B.8.1", "P@Q7; N@QR", ["FILE"], ["Q7", "QR"], [])
F("custody.last_window.delegation_run_id", "string | \"UNKNOWN\" | null", "1", "B.8.1; B.8.5", "P@Q7; N@QR", ["FILE", "RECON"], ["Q7", "QR"], ["I-S08"],
  invalid="no nulo con NOT_ACCEPTED o WITHDRAWN")
F("custody.last_window.verification", "StateRef | null", "1", "B.8.1", "P@Q7", ["FILE"], ["Q7"], ["I-S08", "I-S13"], invalid="null con VERIFIED")
F("custody.last_window.verified_sha", "SHA | null", "1", "B.8.1; §8.4", "P@Q7", ["FILE", "HIST", "REBASE"], ["Q7"], ["I-S08", "I-H02", "I-P10", "I-P12"], sha=True,
  rebase="imagen (StateFields); resultado propio de una ventana con rebase: SHA de la rama rebasada (I-P12)", invalid="no nulo sin VERIFIED; SHA de su propio commit")
F("custody.last_window.journal", "StateRef | null", "1", "B.8.1; §8.4", "P@Q7", ["FILE"], ["Q7", "QR"], ["I-S08", "I-S13"], invalid="null fuera de ABANDONED")
F("custody.last_window.reconstruction", "StateRef | null", "1", "B.8.1; B.8.5", "N@QR", ["FILE", "RECON"], ["QR"], ["I-S08", "I-S13"], invalid="no nulo ⇎ ABANDONED")
F("custody.chains[]", "objeto", "0..n", "B.8.1; 16.6; 16.8", "P@Q7", ["FILE", "PAIR", "VERIFY"], ["Q7", "QR"], ["I-S09", "I-S10", "I-P05"], invalid="task_id duplicado; QU que las cambia")
F("custody.chains.task_id", "string", "1", "B.8.1", "P@Q7", ["FILE"], ["Q7"], ["I-S09"])
F("custody.chains.state", "enum IN_COURSE|VERIFIED|ABANDONED", "1", "B.8.1; 16.6", "P@Q7; N@QR", ["FILE", "NEXT"], ["Q7", "QR"], [])
F("custody.chains.chain_base_sha", "SHA", "1", "B.8.1; 16.8; §8.4", "P@Q7 (el Q0 de la primera ventana de la tarea)", ["FILE", "HIST", "VERIFY", "REBASE"], ["Q7", "QU", "QR"],
  ["I-H02", "I-P10", "I-P12"], sha=True, rebase="imagen (StateFields), de todas las cadenas sea cual sea su estado", invalid="SHA de su propio commit")
F("custody.chains.chain_red_sha", "SHA | null", "1", "B.8.1; 16.8", "P@Q7", ["FILE", "HIST", "VERIFY", "REBASE"], ["Q7", "QU", "QR"], ["I-S09", "I-H02", "I-P10", "I-P12"],
  sha=True, rebase="imagen (StateFields); la corrida de CI del RED se sigue citando por el SHA original (CiRuns)")
F("custody.chains.chain_red_files[]", "ruta", "0..n", "B.8.1; 16.8", "P@Q7", ["FILE", "PAIR", "VERIFY"], ["Q7"], ["I-S09", "I-P05"],
  rebase="durable (rutas); no se recalcula", invalid="decrece; vacío con chain_red_sha no nulo")
F("custody.chains.continues_task_id", "string | null", "1", "B.8.1", "P@Q7", ["FILE"], ["Q7"], [])
F("custody.chains.correction_authorized_pending", "bool", "1", "B.8.1; G.1 (16.6)", "P@Q7, QU", ["FILE", "NEXT"], ["Q7", "QU"], [])
F("custody.unverified_commits[]", "{sha, task_id, window_seq, reason, superseded_by}", "0..n", "B.8.1; B.8.5 (R-2); B.8.6", "N@QR (R-2); P@Q7 (superseded_by)",
  ["FILE", "VERIFY", "HIST", "REBASE"], ["QR", "Q7", "QU"], ["I-S10", "I-H02", "I-P10"], sha=True,
  rebase="sha → imagen; window_seq y superseded_by iguales (StateFields)", invalid="task_id fuera de chains; superseded_by sin ventana VERIFIED registrada")
F("counters.correction_launches[]", "{seq, task_id, failure_class, correction_of_run_id, attempts_after, record_version}", "0..n", "B.8.1; 16.8; §9.3",
  "P@Q0 (kind CORRECTION)", ["FILE", "PAIR", "VERIFY"], ["Q0"], ["I-S05", "I-S12", "I-P05", "I-P10"], invalid="seq no consecutivo; entrada borrada; attempts_after no creciente")
F("counters.blocked_reruns[]", "{task_id, phase, count}", "0..n", "B.8.1; 16.8 (P-04)", "P@Q7 (desde el diario); N@QR (B.8.5 f)", ["FILE", "PAIR"], ["Q7", "QR"],
  ["I-S11", "I-P05"], invalid="(task_id, phase) duplicado; count ∉ 0..2; decrece")
F("counters.rebase_recoveries[]", "{task_id, count, last_rebase_map: StateRef}", "0..n", "B.8.1; 16.7; §8.8", "P@Q7, QU REBASE_RECONCILIATION", ["FILE", "PAIR", "REBASE"],
  ["Q7", "QU", "QR"], ["I-S11", "I-P05"], rebase="last_rebase_map = StateRef nueva si el rebase pertenece a la tarea; count según 16.7")
F("counters.invocations[]", "{scope, launched, uncertain, cap, cap_source: StateRef, reconstructed, reconstruction: StateRef | null}", "0..n",
  "B.8.1; 16.8 (P-07); B.8.5 e", "P@Q7; N@QR", ["FILE", "PAIR", "RECON"], ["Q7", "QR"], ["I-S11", "I-P05"],
  invalid="scope duplicado; reconstructed ⇎ reconstruction; valores menores que los probados")
F("execution_context", "mapa", "0..1", "B.8.1", "P (descriptivo)", [], ALL, [], invalid="(ningún lector del protocolo lo consume)")

# ---------------------------------------------------------------- orchestration (B.8.8)
O = "B.8.8; §20.5"
F("orchestration.loop.type", "enum NONE|ARCHITECT_REVIEW|EXECUTION|REVIEWER", "1", O, "P@QU", ["FILE", "ORCH", "NEXT"], ["QU", "QR"], ["I-S18"],
  a1="FC-01: LOOP_CLOSED la devuelve a NONE")
F("orchestration.loop.phase", "enum §20.5 (NONE, REVIEW_PENDING, ARCHITECT_INVOKED, RESULT_INGESTED, CORRECTING, PUBLISHED, CI_VERIFIED, REREVIEW_PENDING, "
  "ARCHITECT_SATISFIED, ESCALATE_OWNER) o la fase de §8 (ejecución)", "1", O, "P@QU", ["FILE", "PAIR", "ORCH", "NEXT"], ["QU", "QR"], ["I-S18", "I-P13"],
  a1="FC-01: transición LOOP_CLOSED {ARCHITECT_SATISFIED | ESCALATE_OWNER resuelta | vigencia EXHAUSTED} → NONE", invalid="NONE con type ≠ NONE; transición fuera de §20.5")
F("orchestration.loop.object", "{commit, path, blob} | null", "1", O + "; I-P13", "P@QU (NONE → REVIEW_PENDING; CORRECTING → PUBLISHED)", ["FILE", "PAIR", "ORCH", "HIST"],
  ["QU"], ["I-S18", "I-P13"], sha=True, rebase="LITERAL: fuera de StateFields e I-H02; I-P13 impide cambiarlo (FC-02)",
  a1="FC-01: → null solo en LOOP_CLOSED. FC-02: commit en StateFields e I-H02; imagen en REBASE_RECONCILIATION con path y blob iguales",
  invalid="cambio en REREVIEW_PENDING → ARCHITECT_INVOKED (cambio tardío); null con bucle activo")
F("orchestration.loop.authorization", "StateRef | null", "1", O + "; §20.5.1", "P@QU (al abrir)", ["FILE", "ORCH"], ["QU"], ["I-S13", "I-S18"],
  a1="FC-01: null en LOOP_CLOSED; un bucle nuevo exige una autorización nueva")
F("orchestration.loop.action_validity", "{authorization_id, state OPEN|ENDED, ended_at, ended_utc, ended_reason ARCHITECT_SATISFIED|EXHAUSTED|EXPIRED|REVOKED|SUPERSEDED, ended_by} | null",
  "1", O + "; §20.5.1", "P@QU (OPEN al abrir; ENDED una sola vez)", ["FILE", "PAIR", "ORCH"], ["QU"], ["I-S18", "I-P13"],
  a1="FC-01: null en LOOP_CLOSED; SUPERSEDED solo con el bucle abierto", invalid="ENDED → OPEN; dos fines para la misma autorización")
F("orchestration.next_action", "{role, action, target, unit, gate, task_id, required_inputs[], required_capabilities[], required_independence, invocation_permission, "
  "budget_remaining, expected_output, completion_condition, stop_conditions[], escalation_conditions[]}", "1", "B.8.8; §20.4", "P@todo punto", ["FILE", "NEXT"], ALL,
  ["I-S18"], invalid="no derivable de forma única (P-17); role OWNER/COORDINATOR con escalation NONE")
R = "B.8.8; §20.6"
F("orchestration.review_requests[]", "objeto", "0..n", R, "P@QU", ["FILE", "PAIR", "ORCH"], ["QU", "QR"], ["I-S18", "I-P13"],
  invalid="logical_review_request_id duplicado; dos OPEN en un bucle; solicitud borrada")
F("orchestration.review_requests.logical_review_request_id", "string ^L…$", "1", R + "; B.2", "P@QU", ["FILE"], ["QU"], ["I-S18"])
F("orchestration.review_requests.authorization_id", "string", "1", "A-1 D1-2 (no congelado)", "P@QU (al abrir; inmutable)", ["FILE", "PAIR"], ["QU"], [],
  a1="FC-01: campo NUEVO; las igualdades de presupuesto se calculan por autorización")
F("orchestration.review_requests.object", "{commit, path, blob}", "1", R, "P@QU (= loop.object al abrir; inmutable)", ["FILE", "PAIR", "ORCH", "HIST"], ["QU"],
  ["I-S18", "I-P13"], sha=True, rebase="LITERAL: inmutable y fuera de StateFields (FC-02)",
  a1="FC-02: commit de las solicitudes OPEN en StateFields; imagen con path y blob iguales; las terminales no se reescriben")
F("orchestration.review_requests.round", "int ≥ 1", "1", R, "P@QU", ["FILE"], ["QU"], [])
F("orchestration.review_requests.state", "enum OPEN|INGESTED|EXHAUSTED|CANCELLED", "1", R, "P@QU", ["FILE", "PAIR", "ORCH"], ["QU"], ["I-S18"],
  a1="FC-01: LOOP_CLOSED exige ninguna OPEN")
A = "B.8.8; §20.6"
F("orchestration.review_requests.attempts[]", "objeto", "1..n", A, "P@QU", ["FILE", "PAIR", "ORCH"], ["QU"], ["I-S18", "I-P13"],
  invalid="attempt_seq no consecutivo; dos no terminales; intento borrado")
F("…attempts.attempt_seq", "int ≥ 1", "1", A, "P@QU", ["FILE"], ["QU"], ["I-S18"])
F("…attempts.invocation", "StateRef (role-invocation/v1 con Target)", "1", A + "; B.9", "P@QU (inmutable desde LAUNCHING)", ["FILE", "PAIR", "ORCH", "HIST"], ["QU"],
  ["I-S13", "I-P13"], sha=True, rebase="LITERAL: el Target.commit de un intento no lanzado queda obsoleto (FC-02)",
  a1="FC-02: intento no lanzado → replanificado (invocation nueva, Target = imagen, misma reserva); LAUNCHING o posterior conserva su Target histórico",
  invalid="invocation nueva desde LAUNCHING")
F("…attempts.binding", "StateRef (binding/v1)", "1", A + "; B.5", "P@QU", ["FILE", "ORCH"], ["QU"], ["I-S13", "I-S18"], invalid="materializado sin AuthorizationRef o con un criterio no SATISFIED")
F("…attempts.state", "enum INVOCATION_PLANNED|BUDGET_RESERVED|LAUNCHING|LAUNCHED|LAUNCH_UNCERTAIN|RESULT_RECEIVED|RESULT_INGESTED|CANCELLED_BEFORE_LAUNCH", "1", A,
  "P@QU", ["FILE", "PAIR", "ORCH", "NEXT"], ["QU"], ["I-S18", "I-P13"], a1="FC-02: INVOCATION_PLANNED → INVOCATION_PLANNED (replanificado) solo en REBASE_RECONCILIATION",
  invalid="transición fuera de I-P13; cambio de un terminal")
F("…attempts.reserved_at", "int | null", "1", A, "P@QU (primera BUDGET_RESERVED)", ["FILE", "PAIR"], ["QU"], ["I-S18", "I-P13"], invalid="cambio tras fijarse")
F("…attempts.run_id", "string | \"UNKNOWN\" | null", "1", A, "P@QU (LAUNCHING)", ["FILE"], ["QU"], ["I-S18"], invalid="null en LAUNCHING o posterior")
F("…attempts.launch_evidence / not_started_evidence / result / runtime_evidence / read_audit / input_fidelity / premise_independence", "StateRef | null",
  "1", A + "; §20.3", "P@QU", ["FILE", "ORCH"], ["QU"], ["I-S13", "I-S18"], invalid="result no nulo en CANCELLED_BEFORE_LAUNCH; premise_independence null con DEGRADED_BOUNDED")
F("…attempts.output_state / fidelity_status / outcome / unaccredited[]", "enums de B.8.8", "1", A + "; §20.3.3", "P@QU", ["FILE", "ORCH"], ["QU"], ["I-S18"])
F("…attempts.reserved_utc / launching_utc", "instante | null", "1", A + "; §20.5.1", "P@QU", ["FILE", "PAIR"], ["QU"], ["I-P13"], invalid="acción nueva con instante > Until")
F("…attempts.ingested_at", "int | null", "1", A, "P@QU", ["FILE"], ["QU"], ["I-S18", "I-P13"], invalid="no nulo en CANCELLED_BEFORE_LAUNCH")
F("orchestration.findings[]", "{lineage_id, finding_ids[], issuer, opened_in, severity, class, affected_section, state, response, last_disposition_in, omitted_in[], "
  "closed_by, downgraded_by}", "0..n", "B.8.8; §20.5.2", "P@QU (RESULT_INGESTED)", ["FILE", "PAIR", "ORCH"], ["QU"], ["I-S18", "I-P13"],
  a1="FC-01: sin cambio (de unidad; un bucle nuevo hereda los linajes abiertos)", invalid="cerrado sin resultado con autoridad; cerrado por reviewer-result; linaje borrado")
B = "B.8.8; §20.6"
F("orchestration.budgets", "{review_rounds, logical_requests, architect_launches, transport_reruns[], correction_rounds, corrections_by_lineage[], caps}", "1", B,
  "P@QU (reserva; PUBLISHED)", ["FILE", "PAIR", "ORCH"], ["QU"], ["I-S18", "I-P13"], rebase="sin cambio en la reconciliación (I-P10)",
  a1="FC-01: pasa a budgets[] {authorization_id, …}, append-only, una entrada por autorización", invalid="contador que decrece o supera su tope; reinicio por cambio de proveedor")
F("orchestration.budgets[].authorization_id", "string", "1", "A-1 D1-1 (no congelado)", "P@QU (NONE → REVIEW_PENDING con autorización nueva)", ["FILE", "PAIR"], ["QU"], [],
  a1="FC-01: campo NUEVO; único; una autorización no se reutiliza")
F("orchestration.escalation", "{state NONE|OWNER|COORDINATOR, reason, required_decision}", "1", "B.8.8; §20.1", "P@QU", ["FILE", "NEXT"], ["QU"], ["I-S18"],
  a1="FC-01: NONE en LOOP_CLOSED", invalid="NONE con next_action.role OWNER/COORDINATOR")
F("orchestration.autonomy_gaps[]", "StateRef", "0..n", "B.8.8; §20.9", "P@QU", ["FILE"], ["QU"], ["I-S13"])

out = {"Source": "Proposal V14 (commit 4c617e82, blob 34ad80ea) B.8.1, B.8.3, B.8.4, B.8.7, B.8.8; A-1 columns = proposal docs/initiatives/I-62-A-1.md (not agreed)",
       "Fields": rows,
       "Summary": {"Fields": len(rows), "ShaBearing": sum(r["Sha"] for r in rows), "A1Affected": sum(r["A1"] != "—" for r in rows),
                   "LiteralStateFields": [r["Path"] for r in rows if r["Sha"] and "(StateFields)" in r["Rebase"]],
                   "ShaOutsideLiteralStateFields": [r["Path"] for r in rows if r["Sha"] and "(StateFields)" not in r["Rebase"]]}}
with open(sys.argv[1], "w", encoding="utf-8", newline="\n") as f:
    json.dump(out, f, ensure_ascii=False, indent=1)
    f.write("\n")
md = ["# `state/v2`: matriz de campos (preparación de F4; generada por `state-v2-fields.py`)", "",
      "Fuente: Proposal V14 B.8 (blob `34ad80ea`). La columna A-1 describe la enmienda **propuesta** (no acordada). Lectores: FILE, PAIR, HIST, CLASSIFIER,",
      "NEXT, EXIT, VERIFY, RECON, REBASE, ORCH (ver la cabecera del script).", "",
      "| Campo | Tipo | Card. | Fuente | Escritor | Puntos | Invariantes | SHA | Rebase (literal) | A-1 | Inválido |", "|---|---|---|---|---|---|---|---|---|---|---|"]
esc = lambda s: str(s).replace("|", "\\|")
for r in rows:
    md.append("| `%s` | %s | %s | %s | %s | %s | %s | %s | %s | %s | %s |" % (esc(r["Path"]), esc(r["Type"]), r["Card"], esc(r["Source"]), esc(r["Writer"]),
              ", ".join(r["Points"]), ", ".join(r["Invariants"]) or "—", "sí" if r["Sha"] else "—", esc(r["Rebase"]), esc(r["A1"]), esc(r["Invalid"]) or "—"))
s = out["Summary"]
md += ["", "**Resumen (MEASURED):** %d campos; %d con SHA; %d afectados por A-1." % (s["Fields"], s["ShaBearing"], s["A1Affected"]), "",
       "- SHA en la lista literal de `StateFields`: " + ", ".join("`%s`" % x for x in s["LiteralStateFields"]) + ".",
       "- SHA **fuera** de esa lista: " + ", ".join("`%s`" % x for x in s["ShaOutsideLiteralStateFields"]) + ". Los de `protocol.*` son hechos históricos o commits de "
       "`main` (I-P06, §8.8); `last_rebase` se fija en el propio punto; los de `orchestration` son exactamente el hueco FC-02.", ""]
open(sys.argv[2], "w", encoding="utf-8", newline="\n").write("\n".join(md))
print(json.dumps(s, ensure_ascii=True))
