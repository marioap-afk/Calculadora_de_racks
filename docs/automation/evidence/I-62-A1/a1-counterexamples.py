"""I-62 A-1 (FC-01, FC-02): exact counterexamples against the literal Freeze, and the same traces against the proposed delta.

F4 PREPARATION, not F4 production: this is not the state/v2 validator and implements no state machine. It encodes, for the few fields that FC-01 and
FC-02 touch, the literal text of Proposal V14 (commit 4c617e82, blob 34ad80ea) and the delta proposed in docs/initiatives/I-62-A-1.md, and runs
symbolic traces through both. A trace is a list of durable points; each pair (p, n) is checked like I-P13 / I-P05 / I-P10 check pairs, each point like
I-S18 / I-H02 check files. The expected verdict of each trace is written next to it; the checks never read it.

Literal clauses encoded (V14):
  §20.5 states and «loop.object cambia en un solo par de puntos durables: CORRECTING → PUBLISHED»; B.8.8 orchestration.loop.object, review_requests[].object
  («inmutable»), orchestration.budgets (one object per unit); I-S18 (architect_launches = intents with reserved_at ≠ null; no counter above its cap);
  I-P13 (counters never decrease; loop.object only in CORRECTING → PUBLISHED or NONE → REVIEW_PENDING; attempt transitions of §20.6); I-P05 (a
  REBASE_RECONCILIATION point changes exactly the SHA fields of StateFields); B.8.7 StateFields (closed list); I-H02 (ancestry of that same list).

Usage: python a1-counterexamples.py <output json>
"""
import copy
import json
import sys

CAPS = {"review_rounds": 3, "logical_requests": 3, "architect_launches": 9}
LITERAL_STATE_FIELDS = ["chains.chain_base_sha", "chains.chain_red_sha", "last_window.verified_sha", "unverified_commits.sha", "last_evidence_commit"]
REVIEW_TRANSITIONS = {  # §20.5 (only the edges used here) + the A-1 closing edge
    ("NONE", "REVIEW_PENDING"), ("REVIEW_PENDING", "ARCHITECT_INVOKED"), ("ARCHITECT_INVOKED", "RESULT_INGESTED"),
    ("RESULT_INGESTED", "ARCHITECT_SATISFIED"), ("RESULT_INGESTED", "CORRECTING"), ("CORRECTING", "PUBLISHED"), ("PUBLISHED", "CI_VERIFIED"),
    ("CI_VERIFIED", "REREVIEW_PENDING"), ("REREVIEW_PENDING", "ARCHITECT_INVOKED"), ("RESULT_INGESTED", "ESCALATE_OWNER"),
}
CLOSING_FROM = {"ARCHITECT_SATISFIED", "ESCALATE_OWNER", "EXHAUSTED"}
ATTEMPT_TRANSITIONS = {("INVOCATION_PLANNED", "BUDGET_RESERVED"), ("BUDGET_RESERVED", "LAUNCHING"), ("BUDGET_RESERVED", "BUDGET_RESERVED"),
                       ("LAUNCHING", "LAUNCHED"), ("LAUNCHED", "RESULT_RECEIVED"), ("RESULT_RECEIVED", "RESULT_INGESTED")}
TERMINAL = {"RESULT_INGESTED", "LAUNCH_UNCERTAIN", "CANCELLED_BEFORE_LAUNCH"}
NOT_LAUNCHED = {"INVOCATION_PLANNED", "BUDGET_RESERVED"}


# ------------------------------------------------------------------ helpers over the symbolic state
def attempts(state):
    for r in state["review_requests"]:
        for a in r["attempts"]:
            yield r, a


def reserved(state, auth=None):
    return sum(1 for r, a in attempts(state) if a["reserved_at"] is not None and (auth is None or r.get("authorization_id") == auth))


def budget_entries(state):
    b = state["budgets"]
    return b if isinstance(b, list) else [dict(b, authorization_id=None)]


def live_commit_fields(state, amended):
    """SHA fields that I-H02 checks (literal: B.8.7 list; amended: + loop object, OPEN request objects, Targets of not-launched attempts)."""
    out = {"last_evidence_commit": state["last_evidence_commit"]}
    if amended:
        if state["loop"]["object"] is not None:
            out["loop.object.commit"] = state["loop"]["object"]["commit"]
        for r in state["review_requests"]:
            if r["state"] == "OPEN":
                out["review_requests[%s].object.commit" % r["id"]] = r["object"]["commit"]
            for a in r["attempts"]:
                if a["state"] in NOT_LAUNCHED:
                    out["review_requests[%s].attempts[%d].Target.commit" % (r["id"], a["seq"])] = a["target"]["commit"]
    return out


# ------------------------------------------------------------------ file invariants (I-S18 subset, I-H02 subset)
def file_checks(state, amended):
    v = []
    for entry in budget_entries(state):
        auth = entry["authorization_id"]
        if entry["architect_launches"] != reserved(state, auth if amended else None):
            v.append("I-S18: architect_launches ≠ intentos reservados%s" % ("" if not amended else " de " + str(auth)))
        for k, cap in CAPS.items():
            if entry[k] > cap:
                v.append("I-S18: %s = %d supera su tope %d%s (P-18)" % (k, entry[k], cap, "" if auth is None else " en " + auth))
    if amended:
        ids = [e["authorization_id"] for e in budget_entries(state)]
        if len(ids) != len(set(ids)):
            v.append("A-1: budgets[] único por authorization_id")
        if any(r.get("authorization_id") is None for r in state["review_requests"]):
            v.append("A-1: review_requests[] sin authorization_id")
    for r, a in attempts(state):
        if a["state"] in NOT_LAUNCHED and a["target"] != r["object"]:
            v.append("§20.6: el Target del intento no lanzado %s/%d ≠ el objeto de su solicitud" % (r["id"], a["seq"]))
    for field, sha in live_commit_fields(state, amended).items():
        if sha not in state["head_ancestors"] and state["last_point"] != "Q0":
            v.append("I-H02: %s = %s no es ancestro de HEAD" % (field, sha))
    return v


# ------------------------------------------------------------------ pair invariants (I-P13, I-P05, §20.5)
def pair_checks(p, n, amended):
    v = []
    pl, nl = p["loop"], n["loop"]
    edge = (pl["phase"], nl["phase"])
    rebase = n["point_kind"] == "REBASE_RECONCILIATION"
    closing = amended and pl["phase"] in CLOSING_FROM and nl["phase"] == "NONE"
    if pl["phase"] != nl["phase"] and edge not in REVIEW_TRANSITIONS and not closing:
        v.append("§20.5: transición %s → %s no existe" % edge)
    if pl["object"] != nl["object"]:
        allowed = edge in {("CORRECTING", "PUBLISHED"), ("NONE", "REVIEW_PENDING")}
        if amended and closing and nl["object"] is None:
            allowed = True
        if amended and rebase and pl["object"] and nl["object"] and pl["phase"] == nl["phase"] \
                and (pl["object"]["path"], pl["object"]["blob"]) == (nl["object"]["path"], nl["object"]["blob"]) \
                and n["rebase_map"].get(pl["object"]["commit"]) == nl["object"]["commit"]:
            allowed = True
        if not allowed:
            v.append("I-P13: loop.object cambia en %s → %s" % edge)
    if closing:
        if any(a["state"] not in TERMINAL for _, a in attempts(p)) or any(r["state"] == "OPEN" for r in p["review_requests"]):
            v.append("A-1: LOOP_CLOSED con una solicitud OPEN o un intento no terminal")
        if nl["authorization"] is not None or n["action_validity"] is not None:
            v.append("A-1: LOOP_CLOSED deja autorización o vigencia")
        if p["action_validity"] is None or p["action_validity"]["state"] != "ENDED":
            v.append("A-1: LOOP_CLOSED sin vigencia de acción terminada")
    if amended and edge == ("NONE", "REVIEW_PENDING"):
        used = {e["authorization_id"] for e in budget_entries(p)}
        if nl["authorization"] in used:
            v.append("A-1: un bucle nuevo exige una ReviewLoopAuthorization nueva (reutiliza %s)" % nl["authorization"])
    # budgets never decrease and never reset (literal: one object; amended: per authorization, append-only)
    pe = {e["authorization_id"]: e for e in budget_entries(p)}
    ne = {e["authorization_id"]: e for e in budget_entries(n)}
    for auth, e in pe.items():
        if auth not in ne:
            v.append("I-P13: desaparece la entrada de presupuesto %s" % auth)
            continue
        for k in CAPS:
            if ne[auth][k] < e[k]:
                v.append("I-P13: %s decrece%s" % (k, "" if auth is None else " en " + auth))
        if amended and auth != nl["authorization"] and ne[auth] != e:
            v.append("A-1: cambia el presupuesto de una autorización cerrada (%s)" % auth)
    # review requests: append-only; object immutable (amended: OPEN requests take their image in a reconciliation)
    prs = {r["id"]: r for r in p["review_requests"]}
    for r in n["review_requests"]:
        q = prs.get(r["id"])
        if q is None:
            continue
        if q["object"] != r["object"]:
            ok = amended and rebase and q["state"] == "OPEN" and (q["object"]["path"], q["object"]["blob"]) == (r["object"]["path"], r["object"]["blob"]) \
                and n["rebase_map"].get(q["object"]["commit"]) == r["object"]["commit"]
            if not ok:
                v.append("B.8.8: review_requests[%s].object es inmutable" % r["id"])
        qa = {a["seq"]: a for a in q["attempts"]}
        for a in r["attempts"]:
            b = qa.get(a["seq"])
            if b is None:
                continue
            if b["state"] != a["state"] and (b["state"], a["state"]) not in ATTEMPT_TRANSITIONS:
                v.append("I-P13: intento %s/%d %s → %s" % (r["id"], a["seq"], b["state"], a["state"]))
            if b["target"] != a["target"]:
                # §20.6 / I-P13: only a not-launched attempt may be replanned, with a new InvocationId and inside the same reservation; a LAUNCHING or
                # later attempt keeps its Target (B.8.8: invocation immutable from LAUNCHING)
                if b["state"] not in NOT_LAUNCHED or a["state"] != b["state"]:
                    v.append("I-P13/B.8.8: Target de un intento lanzado o terminal reescrito (%s/%d, %s)" % (r["id"], a["seq"], b["state"]))
                elif a["invocation_id"] == b["invocation_id"] or a["reserved_at"] != b["reserved_at"]:
                    v.append("§20.6: replanificación sin InvocationId nuevo o fuera de la misma reserva (%s/%d)" % (r["id"], a["seq"]))
                elif amended and rebase and (n["rebase_map"].get(b["target"]["commit"]) != a["target"]["commit"]
                                             or (b["target"]["path"], b["target"]["blob"]) != (a["target"]["path"], a["target"]["blob"])):
                    v.append("A-1: el Target replanificado no es la imagen del original (%s/%d)" % (r["id"], a["seq"]))
            if b["state"] in TERMINAL and a != b:
                v.append("I-P13: un intento terminal cambia")
    if missing := [r for r in p["review_requests"] if r["id"] not in {x["id"] for x in n["review_requests"]}]:
        v.append("I-P13: desaparecen solicitudes %s" % [r["id"] for r in missing])
    # I-P05: what a reconciliation may change
    if rebase:
        if any(pe.get(a) != ne.get(a) for a in set(pe) | set(ne)):
            v.append("I-P05: la reconciliación cambia contadores")
        if pl["phase"] != nl["phase"]:
            v.append("I-P05: la reconciliación cambia la fase")
        if not amended:
            changed = []
            if pl["object"] != nl["object"]:
                changed.append("orchestration.loop.object.commit")
            if [r["object"] for r in p["review_requests"]] != [r["object"] for r in n["review_requests"]]:
                changed.append("orchestration.review_requests[].object.commit")
            if changed:
                v.append("I-P05: la reconciliación cambia campos fuera de StateFields %s" % changed)
    return v


def run(trace, amended):
    out = []
    for k, point in enumerate(trace):
        for x in file_checks(point, amended):
            out.append("punto %d (%s): %s" % (k, point["label"], x))
        if k:
            for x in pair_checks(trace[k - 1], point, amended):
                out.append("par %d→%d (%s): %s" % (k - 1, k, point["label"], x))
    return ("VALID" if not out else "INVALID"), out


# ------------------------------------------------------------------ symbolic states
def obj(commit, path="docs/initiatives/U-proposal-v1.md", blob="b-X1"):
    return {"commit": commit, "path": path, "blob": blob}


def state(label, phase, loop_obj, auth, requests, budgets, ancestors, point_kind="ORDINARY", rebase_map=None, validity="OPEN", last_point="QU"):
    return {"label": label, "loop": {"type": "NONE" if phase == "NONE" else "ARCHITECT_REVIEW", "phase": phase, "object": loop_obj, "authorization": auth},
            "action_validity": None if auth is None else {"authorization_id": auth, "state": validity}, "review_requests": requests, "budgets": budgets,
            "last_evidence_commit": "e1", "head_ancestors": set(ancestors), "point_kind": point_kind, "rebase_map": rebase_map or {}, "last_point": last_point}


def request(rid, o, st, atts, auth=None):
    r = {"id": rid, "object": o, "state": st, "attempts": atts}
    if auth is not None:
        r["authorization_id"] = auth
    return r


def attempt(seq, st, target, reserved_at, inv):
    return {"seq": seq, "state": st, "target": target, "reserved_at": reserved_at, "invocation_id": inv}


def literal_budget(rounds, requests, launches):
    return {"review_rounds": rounds, "logical_requests": requests, "architect_launches": launches}


def amended_budget(auth, rounds, requests, launches):
    return {"authorization_id": auth, "review_rounds": rounds, "logical_requests": requests, "architect_launches": launches}


def fc01_traces():
    """Design loop over three versions (X1, X2, X3) under A1 reaches ARCHITECT_SATISFIED; READY-06 then needs a second loop over the implementation Y."""
    anc = {"e1", "x1", "x2", "x3", "y1"}
    r3 = lambda auth=None: [request("L%d" % i, obj("x%d" % i, blob="b-X%d" % i), "INGESTED",
                                    [attempt(1, "RESULT_INGESTED", obj("x%d" % i, blob="b-X%d" % i), 10 + i, "I%d" % i)], auth) for i in (1, 2, 3)]
    yreq = lambda auth=None: request("L4", obj("y1", "src/impl.cs", "b-Y1"), "OPEN", [attempt(1, "BUDGET_RESERVED", obj("y1", "src/impl.cs", "b-Y1"), 20, "I4")], auth)
    sat_lit = state("ARCHITECT_SATISFIED de A1", "ARCHITECT_SATISFIED", obj("x3", blob="b-X3"), "A1", r3(), literal_budget(3, 3, 3), anc, validity="ENDED")
    traces = {}
    # literal 1: reopen directly over Y
    traces["fc01-literal-reabrir-sobre-Y"] = ("INVALID", False, [
        sat_lit, state("REVIEW_PENDING sobre Y (A2)", "REVIEW_PENDING", obj("y1", "src/impl.cs", "b-Y1"), "A2", r3() + [yreq()], literal_budget(4, 4, 4), anc)])
    # literal 2: close to NONE, then open over Y
    traces["fc01-literal-cerrar-y-abrir"] = ("INVALID", False, [
        sat_lit, state("NONE", "NONE", None, None, r3(), literal_budget(3, 3, 3), anc),
        state("REVIEW_PENDING sobre Y (A2)", "REVIEW_PENDING", obj("y1", "src/impl.cs", "b-Y1"), "A2", r3() + [yreq()], literal_budget(4, 4, 4), anc)])
    # amended: LOOP_CLOSED, then a new loop under a NEW authorization with its own budget entry; A1's entry stays as history
    sat_am = state("ARCHITECT_SATISFIED de A1", "ARCHITECT_SATISFIED", obj("x3", blob="b-X3"), "A1", r3("A1"), [amended_budget("A1", 3, 3, 3)], anc, validity="ENDED")
    closed = state("LOOP_CLOSED → NONE", "NONE", None, None, r3("A1"), [amended_budget("A1", 3, 3, 3)], anc)
    opened = state("REVIEW_PENDING sobre Y (A2)", "REVIEW_PENDING", obj("y1", "src/impl.cs", "b-Y1"), "A2", r3("A1") + [yreq("A2")],
                   [amended_budget("A1", 3, 3, 3), amended_budget("A2", 1, 1, 1)], anc)
    traces["fc01-enmendado-cerrar-y-abrir-con-A2"] = ("VALID", True, [sat_am, closed, opened])
    # amended negatives
    reuse = copy.deepcopy(opened); reuse["loop"]["authorization"] = "A1"; reuse["action_validity"]["authorization_id"] = "A1"
    reuse["review_requests"][-1]["authorization_id"] = "A1"; reuse["budgets"] = [amended_budget("A1", 4, 4, 4)]; reuse["label"] = "reabre con A1"
    traces["fc01-enmendado-reutiliza-A1"] = ("INVALID", True, [sat_am, closed, reuse])
    open_req = copy.deepcopy(sat_am); open_req["review_requests"][-1]["state"] = "OPEN"; open_req["label"] = "ARCHITECT_SATISFIED con L3 OPEN (sintético)"
    traces["fc01-enmendado-cierra-con-solicitud-abierta"] = ("INVALID", True, [open_req, closed])
    reset = copy.deepcopy(opened); reset["budgets"][0] = amended_budget("A1", 0, 0, 3); reset["label"] = "A2 abre y reinicia A1"
    traces["fc01-enmendado-reinicia-A1"] = ("INVALID", True, [sat_am, closed, reset])
    null_late = state("objeto null fuera del cierre", "REVIEW_PENDING", None, "A1", r3("A1"), [amended_budget("A1", 3, 3, 3)], anc)
    traces["fc01-enmendado-object-null-fuera-del-cierre"] = ("INVALID", True, [sat_am, null_late])
    return traces


def fc02_traces():
    """An out-of-window rebase during an active loop: X@c1 OPEN in L1 with a BUDGET_RESERVED attempt; the rebase maps c1 → c1'."""
    before_anc, after_anc = {"e1", "c1"}, {"e1", "c1p"}
    o, op = obj("c1"), obj("c1p")
    base = lambda auth=None: [request("L1", o, "OPEN", [attempt(1, "BUDGET_RESERVED", o, 5, "I1")], auth)]
    traces = {}
    p_lit = state("REVIEW_PENDING antes del rebase", "REVIEW_PENDING", o, "A1", base(), literal_budget(1, 1, 1), before_anc)
    # literal option 1: reconcile orchestration in the QU
    rec_lit = state("QU REBASE_RECONCILIATION (reconcilia)", "REVIEW_PENDING", op, "A1",
                    [request("L1", op, "OPEN", [attempt(1, "BUDGET_RESERVED", op, 5, "I1b")])], literal_budget(1, 1, 1), after_anc,
                    point_kind="REBASE_RECONCILIATION", rebase_map={"c1": "c1p"})
    traces["fc02-literal-reconciliar-orquestacion"] = ("INVALID", False, [p_lit, rec_lit])
    # literal option 2: leave them; the literal validator ACCEPTS a stale Target that is not on the branch (the hole)
    stale_lit = state("QU REBASE_RECONCILIATION (no reconcilia)", "REVIEW_PENDING", o, "A1", base(), literal_budget(1, 1, 1), after_anc,
                      point_kind="REBASE_RECONCILIATION", rebase_map={"c1": "c1p"})
    traces["fc02-literal-no-reconciliar-hueco"] = ("VALID", False, [p_lit, stale_lit])
    # amended: images with the same path/blob/phase/counters; the not-launched attempt replanned within the same reservation
    p_am = state("REVIEW_PENDING antes del rebase", "REVIEW_PENDING", o, "A1", base("A1"), [amended_budget("A1", 1, 1, 1)], before_anc)
    rec_am = state("QU REBASE_RECONCILIATION", "REVIEW_PENDING", op, "A1",
                   [request("L1", op, "OPEN", [attempt(1, "BUDGET_RESERVED", op, 5, "I1b")], "A1")], [amended_budget("A1", 1, 1, 1)], after_anc,
                   point_kind="REBASE_RECONCILIATION", rebase_map={"c1": "c1p"})
    traces["fc02-enmendado-reconcilia"] = ("VALID", True, [p_am, rec_am])
    stale_am = state("QU REBASE_RECONCILIATION (no reconcilia)", "REVIEW_PENDING", o, "A1", base("A1"), [amended_budget("A1", 1, 1, 1)], after_anc,
                     point_kind="REBASE_RECONCILIATION", rebase_map={"c1": "c1p"})
    traces["fc02-enmendado-no-reconciliar-lo-detecta-I-H02"] = ("INVALID", True, [p_am, stale_am])
    # a LAUNCHED attempt keeps its historical Target (and so does its OPEN request? no: the request object takes its image; the attempt keeps the original)
    launched = lambda tgt, rid_obj, inv: [request("L1", rid_obj, "OPEN", [attempt(1, "LAUNCHED", tgt, 5, inv)], "A1")]
    p_l = state("ARCHITECT_INVOKED antes del rebase", "ARCHITECT_INVOKED", o, "A1", launched(o, o, "I1"), [amended_budget("A1", 1, 1, 1)], before_anc)
    keep = state("QU REBASE_RECONCILIATION", "ARCHITECT_INVOKED", op, "A1", launched(o, op, "I1"), [amended_budget("A1", 1, 1, 1)], after_anc,
                 point_kind="REBASE_RECONCILIATION", rebase_map={"c1": "c1p"})
    traces["fc02-enmendado-LAUNCHED-conserva-su-Target"] = ("VALID", True, [p_l, keep])
    rewrite = state("QU REBASE_RECONCILIATION (reescribe un LAUNCHED)", "ARCHITECT_INVOKED", op, "A1", launched(op, op, "I1"), [amended_budget("A1", 1, 1, 1)],
                    after_anc, point_kind="REBASE_RECONCILIATION", rebase_map={"c1": "c1p"})
    traces["fc02-enmendado-reescribir-un-LAUNCHED"] = ("INVALID", True, [p_l, rewrite])
    other_blob = copy.deepcopy(rec_am); other_blob["loop"]["object"]["blob"] = "b-otro"; other_blob["label"] = "imagen con otro blob"
    traces["fc02-enmendado-imagen-con-otro-blob"] = ("INVALID", True, [p_am, other_blob])
    counters = copy.deepcopy(rec_am); counters["budgets"] = [amended_budget("A1", 1, 1, 2)]; counters["label"] = "la reconciliación cambia contadores"
    counters["review_requests"][0]["attempts"][0]["reserved_at"] = 5
    traces["fc02-enmendado-reconciliacion-cambia-contadores"] = ("INVALID", True, [p_am, counters])
    return traces


def main():
    results, ok = {}, True
    for name, (expected, amended, trace) in sorted({**fc01_traces(), **fc02_traces()}.items()):
        got, why = run(trace, amended)
        results[name] = {"Validator": "enmendado (A-1)" if amended else "literal (V14)", "Expected": expected, "Got": got, "Violations": why,
                         "Verdict": "PASS" if got == expected else "FAIL"}
        ok = ok and got == expected
    with open(sys.argv[1], "w", encoding="utf-8", newline="\n") as f:
        json.dump({"Script": "a1-counterexamples.py", "Freeze": {"commit": "4c617e82b32b6c810b68d75fc19472efed22b393", "blob": "34ad80ea1bfff144bfc5169f62920a4c904c1bfa"},
                   "Traces": results, "AllAsExpected": ok}, f, ensure_ascii=False, indent=1)
        f.write("\n")
    print(json.dumps({k: v["Got"] + (" ok" if v["Verdict"] == "PASS" else " MISMATCH") for k, v in results.items()}, ensure_ascii=False, indent=0))
    return 0 if ok else 1


if __name__ == "__main__":
    sys.exit(main())
