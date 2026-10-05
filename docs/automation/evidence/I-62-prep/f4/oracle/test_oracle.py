"""Transition table of the state/v2 oracle, with mutations (I-62 F4 preparation).

Each transition is a concrete pair (p, n) built from a valid predecessor. The oracle must accept it in its variant, and each mutation must be rejected
with the expected first failing invariant. For every transition the result records: predecessor and successor points, the fields that changed (the
mutable set actually used), the counter deltas, the evidence references introduced and the authority (decision markers) required.

Usage: python test_oracle.py <out.json>
"""
import copy
import json
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import state_oracle as O  # noqa: E402

OUT = sys.argv[1]
H = lambda s: (s * 40)[:40]
REF = lambda name: {"path": "docs/automation/evidence/I-99-agent/" + name, "blob": H("b")}
CLAIM = "11111111-2222-3333-4444-555555555555"


def orch(variant):
    return {"loop": {"type": "NONE", "phase": "NONE", "object": None, "authorization": None, "action_validity": None},
            "next_action": {"role": "PRINCIPAL_COORDINATOR", "action": "CUSTODY"}, "review_requests": [], "findings": [],
            "budgets": [] if variant == "A1" else {"review_rounds": 0, "logical_requests": 0, "architect_launches": 0, "transport_reruns": [],
                                                   "correction_rounds": 0, "corrections_by_lineage": [], "caps": REF("caps.json")},
            "escalation": {"state": "NONE", "reason": None, "required_decision": None}, "autonomy_gaps": []}


def bootstrap(variant="LITERAL"):
    return {"schema": "rackcad-automation-state/v2",
            "automation_state": {"initiative": "I-99", "branch": "architecture/x", "claim_id": CLAIM, "current_phase": "G0", "state": "claimed", "gate": "none",
                                 "attempts": 0, "next_action": "G0", "last_evidence_commit": H("e")},
            "protocol": {"set": "rackcad-protocol/I62", "effective_sha": H("f"),
                         "basis": {"claim_id": CLAIM, "claim_commit": None, "claim_parent_sha": H("a"), "claim_parent_contains_effective": True, "adoption_at": "G0"},
                         "g0_acceptance": {"state": "PENDING", "decision": None}},
            "custody": {"record_version": 1, "point": "BOOTSTRAP", "point_kind": None, "last_rebase": None,
                        "principal": {"state": "HELD", "binding": REF("bootstrap/binding.json"), "acceptance": {"state": "PENDING", "decision": None},
                                      "preflight": REF("bootstrap/preflight.json"), "designation": None, "since_record_version": 1},
                        "window": {"seq": 0, "state": "CLOSED"}, "task_intent": None, "last_window": None, "chains": [], "unverified_commits": []},
            "counters": {"correction_launches": [], "blocked_reruns": [], "rebase_recoveries": [], "invocations": []},
            "orchestration": orch(variant)}


def step(p, point, kind=None, **edits):
    n = copy.deepcopy(p)
    n["custody"]["record_version"] += 1
    n["custody"]["point"] = point
    n["custody"]["point_kind"] = kind
    for path, val in edits.items():
        cur = n
        parts = path.split("__")
        for part in parts[:-1]:
            cur = cur[part]
        cur[parts[-1]] = val
    return n


G0_DECISIONS = {"I62-CLASSIFICATION: I62", "I62-DELEGATED-EXECUTION: I62_DELEGATED", "I62-PRINCIPAL-BINDING: B1 ACCEPTED"}


def accepted(variant="LITERAL"):
    b = bootstrap(variant)
    return step(b, "QU", "ORDINARY", protocol__g0_acceptance={"state": "ACCEPTED", "decision": REF("decisions.md")},
                custody__principal__acceptance={"state": "ACCEPTED", "decision": REF("decisions.md")}, custody__principal__binding=REF("g0/binding.json"))


def intent(kind="FIRST", attempt=0):
    return {"task_id": "T-01", "attempt": attempt, "kind": kind, "contract": REF("T-01/contract.json"), "continues_task_id": None,
            "planned_roles": [{"role": "EXECUTION_CONTROLLER", "binding": REF("ctrl.json")}, {"role": "WORKER", "binding": None}]}


def q0(p, kind="FIRST"):
    attempts = p["automation_state"]["attempts"] + (1 if kind == "CORRECTION" else 0)
    n = step(p, "Q0", None, custody__window={"seq": p["custody"]["window"]["seq"] + 1, "state": "OPENABLE"}, custody__task_intent=intent(kind, attempts))
    n["automation_state"]["attempts"] = attempts
    if kind == "CORRECTION":
        cl = n["counters"]["correction_launches"]
        cl.append({"seq": len(cl) + 1, "task_id": "T-01", "failure_class": "Tests", "correction_of_run_id": "R1", "attempts_after": attempts,
                   "record_version": n["custody"]["record_version"]})
    return n


def window(seq, closure, task="T-01", attempt=0, run="R-del-1"):
    return {"seq": seq, "task_id": task, "attempt": attempt, "closure": closure, "closure_source": "CONTROLLER" if closure in ("VERIFIED", "REWORK", "BLOCKED", "STOP") else
            ("COORDINATOR" if closure in ("ABANDONED", "WITHDRAWN") else "SESSION"),
            "delegation_run_id": None if closure in ("NOT_ACCEPTED", "WITHDRAWN") else ("UNKNOWN" if closure == "ABANDONED" else run),
            "verification": REF("verif.json") if closure == "VERIFIED" else None, "verified_sha": H("9") if closure == "VERIFIED" else None,
            "journal": None if closure == "ABANDONED" else REF("journal.json"), "reconstruction": REF("recon.json") if closure == "ABANDONED" else None}


def q7(p, closure="VERIFIED"):
    seq = p["custody"]["window"]["seq"]
    n = step(p, "Q7", None, custody__window={"seq": seq, "state": "CLOSED"}, custody__task_intent=None,
             custody__last_window=window(seq, closure, attempt=p["automation_state"]["attempts"]))
    if closure == "VERIFIED" and not n["custody"]["chains"]:
        n["custody"]["chains"] = [{"task_id": "T-01", "state": "VERIFIED", "chain_base_sha": H("q"), "chain_red_sha": H("r"), "chain_red_files": ["tests/X.cs"],
                                   "continues_task_id": None, "correction_authorized_pending": False}]
    return n


def mutate(n, fn):
    m = copy.deepcopy(n)
    fn(m)
    return m


def first(v):
    return v[0][0] if v else None


results, all_ok = [], True


def check(name, p, n, variant="LITERAL", ctx=None, expect=None, mutations=(), authority="—", evidence=None, hist=None):
    """expect=None → VALID; otherwise the first failing invariant id."""
    global all_ok
    v = O.validate_pair(p, n, variant, ctx) + O.validate_file(n, variant)
    if hist is not None:
        v += O.validate_history(n, hist, variant)
    got = first(v)
    ok = got == expect
    row = {"Transition": name, "Variant": variant, "From": O.get(p, "custody.point"), "To": O.get(n, "custody.point"),
           "Expected": expect or "VALID", "Got": got or "VALID", "Pass": ok, "Changed": O.changed(p, n),
           "CounterDeltas": {"attempts": n["automation_state"]["attempts"] - p["automation_state"]["attempts"],
                             "correction_launches": len(n["counters"]["correction_launches"]) - len(p["counters"]["correction_launches"]),
                             "architect_launches": sum(e["architect_launches"] for e in O.budget_entries(n["orchestration"], variant))
                             - sum(e["architect_launches"] for e in O.budget_entries(p["orchestration"], variant))},
           "Authority": authority, "Evidence": evidence or sorted({r["path"] for k, r in O.flatten(n).items() if isinstance(r, dict) and "path" in r}),
           "Violations": v[:4], "Mutations": []}
    for mname, fn, mexp, mctx in mutations:
        m = mutate(n, fn)
        mv = O.validate_pair(p, m, variant, mctx if mctx is not None else ctx) + O.validate_file(m, variant)
        if hist is not None:
            mv += O.validate_history(m, hist, variant)
        mgot = first(mv)
        mok = mgot == mexp
        row["Mutations"].append({"Mutation": mname, "Expected": mexp, "Got": mgot or "VALID", "Pass": mok})
        ok = ok and mok
    row["Pass"] = ok
    all_ok = all_ok and ok
    results.append(row)
    return n


def setp(path, val):
    def f(m):
        cur = m
        parts = path.split(".")
        for part in parts[:-1]:
            cur = cur[part]
        cur[parts[-1]] = val
    return f


# ================================================================== durable points (LITERAL)
b = bootstrap()
ok_file = O.validate_file(b)
results.append({"Transition": "BOOTSTRAP (archivo)", "Variant": "LITERAL", "Expected": "VALID", "Got": first(ok_file) or "VALID", "Pass": not ok_file})
all_ok = all_ok and not ok_file
qu0 = check("T0 BOOTSTRAP → QU (decisión de G0)", b, accepted(), ctx={"decisions": G0_DECISIONS}, authority="Coordinator: I62-CLASSIFICATION, I62-DELEGATED-EXECUTION, I62-PRINCIPAL-BINDING",
            mutations=[("sin marcadores", lambda m: None, "I-P09", {"decisions": set()}),
                       ("solo el marcador del binding (sin los de G0)", lambda m: None, "I-P09", {"decisions": {"I62-PRINCIPAL-BINDING: B1 ACCEPTED"}}),
                       ("cambia attempts", setp("automation_state.attempts", 1), "I-P05", None),
                       ("cambia protocol.basis", setp("protocol.basis.claim_parent_sha", H("z")), "I-P06", None),
                       ("record_version + 2", setp("custody.record_version", 3), "I-P01", None),
                       ("decision nula con ACCEPTED", setp("protocol.g0_acceptance.decision", None), "I-S16", None)])
check("BOOTSTRAP → Q0 (SM-01: inalcanzable)", b, q0(b), expect="I-S15")
o1 = check("Q0 FIRST (paso 1 de 16.4)", qu0, q0(qu0), authority="Principal (intención fijada por el Coordinator)",
           mutations=[("window.seq + 2", setp("custody.window.seq", 2), "I-P04", None),
                      ("cambia chains", setp("custody.chains", [{"task_id": "X", "state": "IN_COURSE", "chain_base_sha": H("1"), "chain_red_sha": None,
                                                                   "chain_red_files": [], "continues_task_id": None, "correction_authorized_pending": False}]), "I-P05", None),
                      ("attempts + 1 sin CORRECTION", lambda m: (setp("automation_state.attempts", 1)(m), setp("custody.task_intent.attempt", 1)(m)), "I-P05", None),
                      ("task_intent nula", setp("custody.task_intent", None), "I-S03", None)])
w1 = check("T7 Q0 → Q7 VERIFIED", o1, q7(o1, "VERIFIED"), authority="Principal (verificación del Controller)",
           mutations=[("VERIFIED sin verified_sha", setp("custody.last_window.verified_sha", None), "I-S08", None),
                      ("ventana sigue OPENABLE", setp("custody.window.state", "OPENABLE"), "I-S03", None)])
check("Q0 → QU (prohibido, W-2)", o1, step(o1, "QU", "ORDINARY", custody__window={"seq": 1, "state": "CLOSED"}), expect="I-P02")
check("T3' Q0 → Q7 NOT_ACCEPTED", o1, q7(o1, "NOT_ACCEPTED"), authority="Coordinator (A1'-A8' en fail)",
      mutations=[("run id con NOT_ACCEPTED", setp("custody.last_window.delegation_run_id", "R-x"), "I-S08", None)])
check("T18 Q0 → Q7 WITHDRAWN", o1, q7(o1, "WITHDRAWN"), authority="Coordinator (retirada)")
oc = check("Q0 CORRECTION (attempts + 1 y CorrectionLaunch)", w1, q0(w1, "CORRECTION"),
           mutations=[("sin CorrectionLaunch", setp("counters.correction_launches", []), "I-S05", None)])
t12b = step(o1, "QR", "ORDINARY", custody__window={"seq": 1, "state": "CLOSED"}, custody__task_intent=None, custody__last_window=window(1, "ABANDONED"))
t12b["custody"]["principal"].update({"binding": REF("N/binding.json"), "designation": REF("designation.md"), "since_record_version": t12b["custody"]["record_version"],
                                     "preflight": REF("N/preflight.json")})
t12b["custody"]["chains"] = []
check("T12b Q0 → QR ABANDONED (R-1)", o1, t12b, authority="Coordinator: designación de N; reconstrucción B.8.5",
      mutations=[("sin reconstrucción", setp("custody.last_window.reconstruction", None), "I-S08", None)])
t12a = q7(o1, "VERIFIED")
t12a["custody"]["principal"].update({"binding": REF("N/binding.json"), "designation": REF("designation.md"), "since_record_version": t12a["custody"]["record_version"]})
check("T12a Q0 → Q7 con TRANSFER", o1, t12a, ctx={"transfer": True}, authority="Coordinator: designación + terminación de P acreditada",
      mutations=[("sin registro TRANSFER", lambda m: None, "I-P07", {"transfer": False})])
qh = check("T17 Q7 → QH (liberación)", w1, step(w1, "QH", None, custody__principal={**w1["custody"]["principal"], "state": "RELEASED"}),
           mutations=[("QH con HELD", setp("custody.principal.state", "HELD"), "I-S04", None)])
check("QH → Q0 (prohibido)", qh, q0(qh), expect="I-P02")
t16 = step(qh, "QR", "ORDINARY")
t16["custody"]["principal"].update({"state": "HELD", "binding": REF("B/binding.json"), "designation": REF("designation-B.md"), "since_record_version": t16["custody"]["record_version"]})
check("T16 QH → QR ORDINARY (transferencia A→B)", qh, t16, authority="Coordinator: designación de B con aceptación de su binding",
      mutations=[("QR sin designación", setp("custody.principal.designation", None), "I-S14", None)])
check("QH → QU (prohibido)", qh, step(qh, "QU", "ORDINARY"), expect="I-P02")
RMAP = {H("q"): H("Q"), H("r"): H("R"), H("9"): H("8"), H("e"): H("E")}
t19 = step(w1, "QU", "REBASE_RECONCILIATION", custody__last_rebase={"map": REF("rebase/map.json"), "main_before": H("m"), "main_after": H("M"),
                                                                        "branch_before": H("x"), "branch_after": H("X"), "record_version": w1["custody"]["record_version"] + 1})
t19["custody"]["chains"][0].update({"chain_base_sha": H("Q"), "chain_red_sha": H("R")})
t19["custody"]["last_window"]["verified_sha"] = H("8")
t19["automation_state"]["last_evidence_commit"] = H("E")
check("T19 Q7 → QU REBASE_RECONCILIATION", w1, t19, ctx={"rebase_map": RMAP}, authority="Principal (RebaseMap acreditado; force-with-lease)",
      mutations=[("imagen equivocada", setp("custody.last_window.verified_sha", H("7")), "I-P10", None),
                 ("cambia la ventana", setp("custody.window.seq", 5), "I-P05", None),
                 ("sin last_rebase", setp("custody.last_rebase", None), "I-S17", None),
                 ("cambia attempts", lambda m: (setp("automation_state.attempts", 1)(m)), "I-P05", None)])
t22 = copy.deepcopy(t19)
t22["custody"]["point"] = "QR"
t22["custody"]["record_version"] = qh["custody"]["record_version"] + 1
t22["custody"]["last_rebase"]["record_version"] = t22["custody"]["record_version"]
t22["custody"]["principal"] = {**qh["custody"]["principal"], "state": "HELD", "binding": REF("D/binding.json"), "designation": REF("takeover.md"),
                               "since_record_version": t22["custody"]["record_version"]}
check("T22 QH → QR REBASE_RECONCILIATION (toma con rebase)", qh, t22, ctx={"rebase_map": RMAP}, authority="Coordinator: I62-REBASE-TAKEOVER + aceptación del binding")
# T13: two concurrent recoveries from the same predecessor; the second loses the CAS
r1 = step(qh, "QR", "ORDINARY")
r1["custody"]["principal"].update({"state": "HELD", "binding": REF("N1.json"), "designation": REF("d1.md"), "since_record_version": r1["custody"]["record_version"]})
r2 = copy.deepcopy(r1)
r2["custody"]["principal"].update({"binding": REF("N2.json"), "designation": REF("d2.md")})
check("T13 segunda recuperación concurrente (pierde el CAS)", r1, r2, expect="I-P01")


# T20 / T21: /v1 ↔ /v2 (I-P11)
def v1_transition(p_schema, n_state):
    if p_schema == "v1":
        return None if O.get(n_state, "custody.point") == "BOOTSTRAP" and O.get(n_state, "custody.record_version") == 1 else "I-P11"
    return None if (O.get(n_state, "custody.point") == "BOOTSTRAP" and O.get(n_state, "custody.window.seq") == 0
                    and O.get(n_state, "protocol.g0_acceptance.state") == "PENDING") else "I-P11"


adopt = bootstrap()
adopt["protocol"]["basis"]["adoption_at"] = "MID_INITIATIVE"
for name, got, exp in (("T20 /v1 DIRECT_ONLY → BOOTSTRAP de adopción", v1_transition("v1", adopt), None),
                       ("/v1 → Q0 directo (prohibido)", v1_transition("v1", q0(qu0)), "I-P11"),
                       ("T21 BOOTSTRAP (seq 0) → /v1 DIRECT_ONLY", v1_transition("v2", b), None),
                       ("T21 con ventana ya abierta (prohibido)", v1_transition("v2", w1), "I-P11")):
    results.append({"Transition": name, "Variant": "LITERAL", "Expected": exp or "VALID", "Got": got or "VALID", "Pass": got == exp})
    all_ok = all_ok and got == exp


# ================================================================== review loop: attempts and phases (both variants)
def loop_open(p, variant, auth="RLA-1", obj_commit="x1"):
    n = step(p, "QU", "ORDINARY")
    o = n["orchestration"]
    o["loop"] = {"type": "ARCHITECT_REVIEW", "phase": "REVIEW_PENDING", "object": {"commit": H(obj_commit[-1]), "path": "docs/p-v1.md", "blob": H("1")},
                 "authorization": REF("decisions.md"), "action_validity": {"authorization_id": auth, "state": "OPEN", "ended_at": None, "ended_utc": None,
                                                                           "ended_reason": None, "ended_by": None}}
    req = {"logical_review_request_id": "L%d" % (len(o["review_requests"]) + 1), "object": dict(o["loop"]["object"]), "round": 1, "state": "OPEN",
           "attempts": [{"attempt_seq": 1, "invocation": REF("inv-1.json"), "binding": REF("arch.json"), "state": "INVOCATION_PLANNED", "reserved_at": None,
                         "run_id": None, "result": None, "ingested_at": None, "target_commit": o["loop"]["object"]["commit"]}]}
    if variant == "A1":
        req["authorization_id"] = auth
        o["budgets"].append({"authorization_id": auth, "review_rounds": 0, "logical_requests": 0, "architect_launches": 0, "transport_reruns": [],
                             "correction_rounds": 0, "corrections_by_lineage": [], "caps": REF("caps.json")})
    o["review_requests"].append(req)
    o["next_action"] = {"role": "PRINCIPAL_COORDINATOR", "action": "RESERVE"}
    return n


def entry(o, variant, auth="RLA-1"):
    return o["budgets"] if variant != "A1" else next(e for e in o["budgets"] if e["authorization_id"] == auth)


def attempt_step(p, state, variant, auth="RLA-1", reserve=False, **fields):
    n = step(p, "QU", "ORDINARY")
    o = n["orchestration"]
    r = next(x for x in o["review_requests"] if x["state"] == "OPEN")
    a = r["attempts"][-1]
    a["state"] = state
    a.update(fields)
    if reserve:
        a["reserved_at"] = n["custody"]["record_version"]
        e = entry(o, variant, auth)
        e["architect_launches"] += 1
        e["logical_requests"] += 1
        e["review_rounds"] += 1
    return n


for variant in ("LITERAL", "A1"):
    lo = check("bucle: NONE → REVIEW_PENDING (abre)", qu0 if variant == "LITERAL" else accepted("A1"), loop_open(qu0 if variant == "LITERAL" else accepted("A1"), variant), variant,
               authority="Coordinator: ReviewLoopAuthorization (marcador I62-REVIEW-LOOP-AUTHORIZATION)")
    br = check("intento: INVOCATION_PLANNED → BUDGET_RESERVED", lo, attempt_step(lo, "BUDGET_RESERVED", variant, reserve=True), variant,
               mutations=[("sin architect_launches + 1", lambda m: entry(m["orchestration"], variant).__setitem__("architect_launches", 0), "I-P13", None),
                          ("reserva con la vigencia terminada", lambda m: m["orchestration"]["loop"]["action_validity"].__setitem__("state", "ENDED"), "I-P13", None)])
    check("intento: BUDGET_RESERVED → BUDGET_RESERVED (replanificado)", br, attempt_step(br, "BUDGET_RESERVED", variant, invocation=REF("inv-1b.json")), variant,
          mutations=[("reserved_at cambia", lambda m: m["orchestration"]["review_requests"][0]["attempts"][0].__setitem__("reserved_at", 99), "I-P13", None)])
    lg = attempt_step(br, "LAUNCHING", variant, run_id="R-1")
    lg["orchestration"]["loop"]["phase"] = "ARCHITECT_INVOKED"
    lg = check("intento: BUDGET_RESERVED → LAUNCHING (ARCHITECT_INVOKED)", br, lg, variant,
               mutations=[("run_id nulo", lambda m: m["orchestration"]["review_requests"][0]["attempts"][0].__setitem__("run_id", None), "I-S18", None),
                          ("objeto cambia (cambio tardío)", lambda m: m["orchestration"]["loop"].__setitem__("object", {"commit": H("7"), "path": "docs/p-v1.md", "blob": H("1")}),
                           "I-P13", None)])
    ld = check("intento: LAUNCHING → LAUNCHED", lg, attempt_step(lg, "LAUNCHED", variant), variant)
    rr = check("intento: LAUNCHED → RESULT_RECEIVED", ld, attempt_step(ld, "RESULT_RECEIVED", variant, result=REF("out.json")), variant)
    ri = attempt_step(rr, "RESULT_INGESTED", variant, ingested_at=rr["custody"]["record_version"] + 1)
    ri["orchestration"]["review_requests"][0]["state"] = "INGESTED"
    ri["orchestration"]["loop"]["phase"] = "RESULT_INGESTED"
    ri = check("intento: RESULT_RECEIVED → RESULT_INGESTED", rr, ri, variant,
               mutations=[("ingestión saltando RESULT_RECEIVED", lambda m: None, None, None)])
    cr = step(ri, "QU", "ORDINARY")
    cr["orchestration"]["loop"]["phase"] = "CORRECTING"
    cr = check("fase: RESULT_INGESTED → CORRECTING", ri, cr, variant)
    pb = step(cr, "QU", "ORDINARY")
    pb["orchestration"]["loop"]["phase"] = "PUBLISHED"
    pb["orchestration"]["loop"]["object"] = {"commit": H("2"), "path": "docs/p-v2.md", "blob": H("2")}
    entry(pb["orchestration"], variant)["correction_rounds"] += 1
    pb = check("fase: CORRECTING → PUBLISHED (único cambio de loop.object)", cr, pb, variant,
               mutations=[("sin correction_rounds + 1", lambda m: entry(m["orchestration"], variant).__setitem__("correction_rounds", 0), "I-P13", None)])
    sat = step(ri, "QU", "ORDINARY")
    sat["orchestration"]["loop"]["phase"] = "ARCHITECT_SATISFIED"
    sat["orchestration"]["loop"]["action_validity"].update({"state": "ENDED", "ended_reason": "ARCHITECT_SATISFIED", "ended_at": 9, "ended_utc": "t", "ended_by": REF("x")})
    sat = check("fase: RESULT_INGESTED → ARCHITECT_SATISFIED", ri, sat, variant)
    cb = attempt_step(br, "CANCELLED_BEFORE_LAUNCH", variant)
    cb["orchestration"]["review_requests"][0]["state"] = "CANCELLED"
    cb["orchestration"]["loop"]["action_validity"].update({"state": "ENDED", "ended_reason": "REVOKED"})
    check("intento: BUDGET_RESERVED → CANCELLED_BEFORE_LAUNCH (fin de vigencia)", br, cb, variant,
          mutations=[("cancelado con resultado", lambda m: m["orchestration"]["review_requests"][0]["attempts"][0].__setitem__("result", REF("late.json")), "I-S18", None)])
    lu = attempt_step(lg, "LAUNCH_UNCERTAIN", variant)
    lu["orchestration"]["loop"]["phase"] = "REVIEW_PENDING"
    lu = check("intento: LAUNCHING → LAUNCH_UNCERTAIN (fase vuelve a REVIEW_PENDING, SM-05)", lg, lu, variant,
               mutations=[("fase sigue en ARCHITECT_INVOKED", lambda m: m["orchestration"]["loop"].__setitem__("phase", "ARCHITECT_INVOKED"), "I-S18", None)])
    b1 = attempt_step(lg, "BUDGET_RESERVED", variant, run_id=None)
    b1["orchestration"]["loop"]["phase"] = "REVIEW_PENDING"
    check("intento: LAUNCHING → BUDGET_RESERVED (caso B.1, vigencia abierta; SM-05)", lg, b1, variant,
          mutations=[("fase sigue en ARCHITECT_INVOKED", lambda m: m["orchestration"]["loop"].__setitem__("phase", "ARCHITECT_INVOKED"), "I-S18", None)])
    check("intento terminal que cambia (prohibido)", lu, attempt_step(lu, "LAUNCHED", variant), variant, expect="I-P13")
    # caps: a fourth logical request in the same budget (P-18)
    cap = copy.deepcopy(br)
    e = entry(cap["orchestration"], variant)
    e.update({"review_rounds": 4, "logical_requests": 4})
    check("tope: cuarta solicitud lógica (P-18)", br, step(cap, "QU", "ORDINARY"), variant, expect="I-S18")
    # FC-01: closing the loop and opening a second one
    closed = step(sat, "QU", "ORDINARY")
    closed["orchestration"]["loop"] = {"type": "NONE", "phase": "NONE", "object": None, "authorization": None, "action_validity": None}
    if variant == "LITERAL":
        check("FC-01: ARCHITECT_SATISFIED → NONE (literal)", sat, closed, variant, expect="I-P13")
    else:
        closed = check("A-1 LOOP_CLOSED: ARCHITECT_SATISFIED → NONE", sat, closed, variant, authority="Principal (tras ARCHITECT_SATISFIED; ninguna decisión nueva)",
                       mutations=[("deja loop.object", lambda m: m["orchestration"]["loop"].__setitem__("object", {"commit": H("2"), "path": "p", "blob": H("2")}), "A1-D1-5", None)])
        second = loop_open(closed, variant, auth="RLA-2", obj_commit="y5")
        check("A-1: bucle nuevo con RLA-2 (presupuesto propio)", closed, second, variant, authority="Coordinator: ReviewLoopAuthorization nueva (RLA-2)")
        reuse = loop_open(closed, variant, auth="RLA-1", obj_commit="y5")
        check("A-1: bucle nuevo reutilizando RLA-1 (prohibido)", closed, reuse, variant, expect="A1-D1-1")
        reuse2 = copy.deepcopy(reuse)
        reuse2["orchestration"]["budgets"] = copy.deepcopy(closed["orchestration"]["budgets"])
        check("A-1: bucle nuevo con RLA-1 sobre su entrada antigua (prohibido)", closed, reuse2, variant, expect="A1-D1-4")
        opened_close = copy.deepcopy(sat)
        opened_close["orchestration"]["review_requests"][0]["state"] = "OPEN"
        c2 = step(opened_close, "QU", "ORDINARY")
        c2["orchestration"]["loop"] = {"type": "NONE", "phase": "NONE", "object": None, "authorization": None, "action_validity": None}
        check("A-1: LOOP_CLOSED con una solicitud OPEN (prohibido)", opened_close, c2, variant, expect="A1-D1-5")
    # FC-02: out-of-window rebase with the loop at REVIEW_PENDING and a reserved attempt
    RM = {H("1"): H("6")}
    pre = copy.deepcopy(br)
    pre["orchestration"]["loop"]["object"]["commit"] = H("1")
    pre["orchestration"]["review_requests"][0]["object"]["commit"] = H("1")
    pre["orchestration"]["review_requests"][0]["attempts"][0]["target_commit"] = H("1")
    rec = step(pre, "QU", "REBASE_RECONCILIATION", custody__last_rebase={"map": REF("rebase/map.json"), "main_before": H("m"), "main_after": H("M"),
                                                                          "branch_before": H("x"), "branch_after": H("X"), "record_version": pre["custody"]["record_version"] + 1})
    rec["orchestration"]["loop"]["object"]["commit"] = H("6")
    rec["orchestration"]["review_requests"][0]["object"]["commit"] = H("6")
    a0 = rec["orchestration"]["review_requests"][0]["attempts"][0]
    a0.update({"target_commit": H("6"), "invocation": REF("inv-replanned.json")})
    stale = step(pre, "QU", "REBASE_RECONCILIATION", custody__last_rebase=rec["custody"]["last_rebase"])
    anc = {H("6"), H("e")}
    if variant == "LITERAL":
        check("FC-02 literal: reconciliar la orquestación", pre, rec, variant, ctx={"rebase_map": RM}, expect="I-P05")
        check("FC-02 literal: no reconciliar (hueco: válido)", pre, stale, variant, ctx={"rebase_map": RM}, hist=anc)
    else:
        check("A-1: reconciliación de la orquestación", pre, rec, variant, ctx={"rebase_map": RM}, hist=anc,
              mutations=[("Target replanificado que no es la imagen", lambda m: m["orchestration"]["review_requests"][0]["attempts"][0].__setitem__("target_commit", H("5")),
                          "A1-D2-2", None),
                         ("loop.object con otro blob", lambda m: m["orchestration"]["loop"]["object"].__setitem__("blob", H("0")), "I-P05", None)])
        check("A-1: no reconciliar (I-H02 lo detecta)", pre, stale, variant, ctx={"rebase_map": RM}, hist=anc, expect="I-H02")

summary = {"Transitions": len(results), "Pass": sum(r["Pass"] for r in results),
           "Mutations": sum(len(r.get("Mutations", [])) for r in results), "MutationsDetected": sum(m["Pass"] for r in results for m in r.get("Mutations", [])),
           "AllPass": all_ok}
json.dump({"Summary": summary, "Results": results}, open(OUT, "w", encoding="utf-8", newline="\n"), ensure_ascii=False, indent=1)
print(json.dumps(summary))
for r in results:
    if not r["Pass"]:
        print("FAIL", r["Transition"], r["Variant"], r.get("Expected"), r.get("Got"), r.get("Violations", [])[:2],
              [m for m in r.get("Mutations", []) if not m["Pass"]])
sys.exit(0 if all_ok else 1)
