"""I-62 F4 preparation: executable oracle of the state/v2 machine (Proposal V14 §8, §9.2, B.8.3, B.8.4, B.8.8; §20.5, §20.6), in two variants:
LITERAL (the Freeze) and A1 (Freeze + the PROPOSED amendment A-1, not agreed). Prototype for F4 (StateV2Validator, C-15/C-18/C-38); not production.

validate_file(state, variant)          → [(invariant, message)]     I-S01, I-S03..I-S09, I-S12, I-S14..I-S17 and the I-S18 subset of the loop
validate_pair(p, n, variant, ctx)      → [(invariant, message)]     I-P01, I-P02, I-P04..I-P07, I-P09, I-P10, I-P13 and the mutable-field rule of B.8.3
                                                                    (which fields each kind of durable point may change)
ctx: {"rebase_map": {original: image}, "decisions": set of markers present, "transfer": bool, "reconstruction": bool}
Invariants that need Git history (I-P03, I-P08, I-H01, I-H02) are out of a file oracle; I-H02 is modelled by `ancestors` in validate_history.
"""
import copy

POINTS = ["BOOTSTRAP", "Q0", "Q7", "QU", "QH", "QR"]
I_P02 = {"BOOTSTRAP": {"Q0", "QU", "QH", "QR"}, "Q0": {"Q7", "QR"}, "Q7": {"Q0", "QU", "QH", "QR"}, "QU": {"Q0", "QU", "QH", "QR"},
         "QH": {"QR"}, "QR": {"Q0", "QU", "QH", "QR"}}
LOOP_EDGES = {("NONE", "REVIEW_PENDING"), ("REVIEW_PENDING", "ARCHITECT_INVOKED"), ("ARCHITECT_INVOKED", "RESULT_INGESTED"),
              ("RESULT_INGESTED", "ARCHITECT_SATISFIED"), ("RESULT_INGESTED", "CORRECTING"), ("CORRECTING", "PUBLISHED"), ("PUBLISHED", "CI_VERIFIED"),
              ("CI_VERIFIED", "REREVIEW_PENDING"), ("REREVIEW_PENDING", "ARCHITECT_INVOKED"), ("RESULT_INGESTED", "ESCALATE_OWNER"),
              ("RESULT_INGESTED", "REREVIEW_PENDING"),  # INVALID → transport rerun inside the cap (§20.5)
              ("ESCALATE_OWNER", "REREVIEW_PENDING"),   # a second request on the same version after the Owner's decision (§20.6)
              # SM-05 (recovery edges implied by §20.6 and I-S18, absent from the §20.5 diagram): the attempt goes back to BUDGET_RESERVED (case B.1) or
              # closes LAUNCH_UNCERTAIN / CANCELLED_BEFORE_LAUNCH while the request stays OPEN, so the phase must return to a PENDING phase
              ("ARCHITECT_INVOKED", "REVIEW_PENDING"), ("ARCHITECT_INVOKED", "REREVIEW_PENDING")}
ATTEMPT_EDGES = {("INVOCATION_PLANNED", "BUDGET_RESERVED"), ("INVOCATION_PLANNED", "CANCELLED_BEFORE_LAUNCH"), ("BUDGET_RESERVED", "LAUNCHING"),
                 ("BUDGET_RESERVED", "CANCELLED_BEFORE_LAUNCH"), ("BUDGET_RESERVED", "BUDGET_RESERVED"), ("LAUNCHING", "LAUNCHED"),
                 ("LAUNCHING", "RESULT_RECEIVED"), ("LAUNCHING", "LAUNCH_UNCERTAIN"), ("LAUNCHING", "BUDGET_RESERVED"), ("LAUNCHING", "CANCELLED_BEFORE_LAUNCH"),
                 ("LAUNCHED", "RESULT_RECEIVED"), ("LAUNCHED", "LAUNCH_UNCERTAIN"), ("RESULT_RECEIVED", "RESULT_INGESTED")}
TERMINAL = {"RESULT_INGESTED", "LAUNCH_UNCERTAIN", "CANCELLED_BEFORE_LAUNCH"}
NOT_LAUNCHED = {"INVOCATION_PLANNED", "BUDGET_RESERVED"}
CAPS = {"review_rounds": 3, "logical_requests": 3, "architect_launches": 9}
REBASE_SHA_FIELDS = ["automation_state.last_evidence_commit", "custody.last_window.verified_sha"]


# ------------------------------------------------------------------ helpers
def get(s, path, default=None):
    cur = s
    for part in path.split("."):
        if not isinstance(cur, dict) or part not in cur:
            return default
        cur = cur[part]
    return cur


def flatten(v, prefix=""):
    out = {}
    if isinstance(v, dict):
        for k, x in v.items():
            out.update(flatten(x, prefix + k + "."))
    elif isinstance(v, list):
        out[prefix[:-1]] = v
    else:
        out[prefix[:-1]] = v
    return out


def changed(p, n):
    fp, fn = flatten(p), flatten(n)
    return sorted(k for k in set(fp) | set(fn) if fp.get(k, "<absent>") != fn.get(k, "<absent>"))


def starts(path, prefixes):
    return any(path == x or path.startswith(x + ".") for x in prefixes)


# ------------------------------------------------------------------ mutable fields per kind of durable point (B.8.3, §8.1, I-P05)
COMMON = ["custody.record_version", "custody.point", "custody.point_kind", "automation_state.current_phase", "automation_state.state",
          "automation_state.gate", "automation_state.next_action", "orchestration.next_action"]
MUTABLE = {
    "Q0": COMMON + ["custody.window", "custody.task_intent", "automation_state.attempts", "counters.correction_launches"],
    "Q7": COMMON + ["custody.window.state", "custody.task_intent", "custody.last_window", "custody.chains", "custody.unverified_commits",
                    "counters.blocked_reruns", "counters.rebase_recoveries", "counters.invocations", "custody.principal", "custody.last_rebase",
                    "automation_state.last_evidence_commit"],
    "QU": COMMON + ["automation_state.last_evidence_commit", "custody.task_intent", "protocol.g0_acceptance", "custody.principal.acceptance",
                    "custody.principal.binding", "custody.principal.preflight", "orchestration"],
    "QH": COMMON + ["custody.principal.state", "custody.task_intent"],
    "QR": COMMON + ["custody.principal", "custody.task_intent", "custody.window.state", "custody.last_window", "custody.unverified_commits",
                    "counters.invocations", "counters.blocked_reruns", "automation_state.last_evidence_commit"],
}
# QU outside delegated execution may change attempts (§8/§9): kept out on purpose; a QU that changes attempts is flagged I-P05 in a delegated unit.


# ------------------------------------------------------------------ file invariants
def validate_file(s, variant="LITERAL"):
    v = []
    a = lambda inv, cond, msg: (not cond) and v.append((inv, msg))
    point = get(s, "custody.point")
    a("I-S01", s.get("schema") == "rackcad-automation-state/v2" and get(s, "protocol.set") == "rackcad-protocol/I62", "schema/protocol")
    a("I-S01", get(s, "protocol.basis.claim_id") == get(s, "automation_state.claim_id"), "claim_id")
    if point == "BOOTSTRAP":
        a("I-S01", get(s, "protocol.g0_acceptance.state") == "PENDING" and get(s, "custody.principal.acceptance.state") == "PENDING", "BOOTSTRAP sin PENDING")
    a("I-S03", (point == "Q0") == (get(s, "custody.window.state") == "OPENABLE"), "Q0 ⇔ OPENABLE")
    if point == "Q0":
        a("I-S03", get(s, "custody.task_intent") is not None and get(s, "custody.principal.state") == "HELD", "Q0 sin intención o sin HELD")
    a("I-S04", (get(s, "custody.principal.state") == "RELEASED") == (point == "QH"), "RELEASED ⇔ QH")
    ti = get(s, "custody.task_intent")
    if ti:
        a("I-S05", ti.get("attempt") == get(s, "automation_state.attempts"), "task_intent.attempt ≠ attempts")
        if ti.get("kind") == "CORRECTION" and point == "Q0":
            cl = get(s, "counters.correction_launches") or []
            a("I-S05", any(e["record_version"] == get(s, "custody.record_version") and e["attempts_after"] == get(s, "automation_state.attempts") for e in cl),
              "corrección sin CorrectionLaunch en el mismo punto")
        roles = [r["role"] for r in ti.get("planned_roles", [])]
        a("I-S06", len(roles) == len(set(roles)) and "EXECUTION_CONTROLLER" in roles, "planned_roles")
    seq, lw = get(s, "custody.window.seq"), get(s, "custody.last_window")
    if point != "Q0":
        a("I-S07", (lw is None) == (seq == 0) and (lw is None or lw["seq"] == seq), "last_window/window.seq fuera de Q0")
    else:
        a("I-S07", (lw is None) == (seq == 1) and (lw is None or lw["seq"] == seq - 1), "last_window/window.seq en Q0")
    if lw:
        a("I-S08", lw["closure"] != "VERIFIED" or (lw["verification"] and lw["verified_sha"]), "VERIFIED sin verificación")
        a("I-S08", lw["closure"] not in ("NOT_ACCEPTED", "WITHDRAWN") or lw["delegation_run_id"] is None, "run id en NOT_ACCEPTED/WITHDRAWN")
        a("I-S08", (lw["closure"] == "ABANDONED") == (lw["reconstruction"] is not None), "ABANDONED ⇔ reconstruction")
    chains = get(s, "custody.chains") or []
    a("I-S09", len({c["task_id"] for c in chains}) == len(chains), "chains duplicadas")
    a("I-S09", all(c["chain_red_sha"] is None or c["chain_red_files"] for c in chains), "RED sin archivos")
    cl = get(s, "counters.correction_launches") or []
    a("I-S12", [e["seq"] for e in cl] == list(range(1, len(cl) + 1)), "seq no consecutivo")
    a("I-S12", all(cl[i]["attempts_after"] < cl[i + 1]["attempts_after"] for i in range(len(cl) - 1)) and all(e["attempts_after"] <= get(s, "automation_state.attempts") for e in cl),
      "attempts_after")
    a("I-S14", (get(s, "custody.principal.designation") is None) == (get(s, "custody.principal.since_record_version") == 1), "designation ⇔ since 1")
    if point == "Q0":
        a("I-S15", get(s, "protocol.g0_acceptance.state") == "ACCEPTED" and get(s, "custody.principal.acceptance.state") == "ACCEPTED", "Q0 sin aceptaciones")
    for path in ("protocol.g0_acceptance", "custody.principal.acceptance"):
        a("I-S16", (get(s, path + ".decision") is None) == (get(s, path + ".state") == "PENDING"), path + ": decision ⇔ PENDING")
    pk = get(s, "custody.point_kind")
    a("I-S17", (pk is not None) == (point in ("QU", "QR")), "point_kind")
    if pk == "REBASE_RECONCILIATION":
        lr = get(s, "custody.last_rebase")
        a("I-S17", lr is not None and lr["record_version"] == get(s, "custody.record_version"), "last_rebase del punto")
    v += validate_orchestration_file(s, variant)
    return v


def budget_entries(o, variant):
    b = o["budgets"]
    if variant == "A1":
        return b
    return [dict(b, authorization_id=None)]


def validate_orchestration_file(s, variant):
    o, v = s.get("orchestration"), []
    if o is None:
        return v
    a = lambda cond, msg: (not cond) and v.append(("I-S18", msg))
    reqs = o["review_requests"]
    loop = o["loop"]
    a((loop["phase"] == "NONE") == (loop["type"] == "NONE"), "phase NONE ⇔ type NONE")
    a(loop["type"] == "NONE" or loop["object"] is not None, "bucle activo sin object")
    a(loop["type"] != "NONE" or loop["object"] is None, "object sin bucle activo")
    opens = [r for r in reqs if r["state"] == "OPEN"]
    a(len(opens) <= 1, "más de una solicitud OPEN")
    for r in reqs:
        nt = [x for x in r["attempts"] if x["state"] not in TERMINAL]
        a(len(nt) <= 1, "más de un intento no terminal")
        a([x["attempt_seq"] for x in r["attempts"]] == list(range(1, len(r["attempts"]) + 1)), "attempt_seq")
        for x in r["attempts"]:
            a(x["state"] not in ("LAUNCHING", "LAUNCHED", "LAUNCH_UNCERTAIN", "RESULT_RECEIVED", "RESULT_INGESTED") or x["run_id"] is not None,
              "run_id nulo desde LAUNCHING")
            a(x["state"] != "CANCELLED_BEFORE_LAUNCH" or (x["result"] is None and x["ingested_at"] is None), "cancelado con resultado")
    if loop["phase"] in ("REVIEW_PENDING", "REREVIEW_PENDING") and opens:
        a(all(x["state"] in NOT_LAUNCHED for x in opens[0]["attempts"] if x["state"] not in TERMINAL), "fase PENDING con intento lanzado")
    if loop["phase"] == "ARCHITECT_INVOKED" and opens:
        a(any(x["state"] in ("LAUNCHING", "LAUNCHED", "RESULT_RECEIVED") for x in opens[0]["attempts"]), "ARCHITECT_INVOKED sin intento lanzado")
    av = loop.get("action_validity")
    if av and av["state"] == "ENDED":
        a(not any(x["state"] in NOT_LAUNCHED for r in reqs for x in r["attempts"]), "vigencia terminada con intentos sin lanzar")
    for e in budget_entries(o, variant):
        rs = [r for r in reqs if variant != "A1" or r.get("authorization_id") == e["authorization_id"]]
        a(e["review_rounds"] <= e["logical_requests"], "review_rounds > logical_requests")
        a(e["architect_launches"] == sum(1 for r in rs for x in r["attempts"] if x["reserved_at"] is not None), "architect_launches ≠ reservados")
        for k, cap in CAPS.items():
            a(e[k] <= cap, "%s supera su tope (P-18)" % k)
    if variant == "A1":
        ids = [e["authorization_id"] for e in o["budgets"]]
        a(len(ids) == len(set(ids)), "budgets[] duplicado")
        a(all(r.get("authorization_id") in ids for r in reqs), "solicitud sin entrada de presupuesto")
        if av:
            a(av["authorization_id"] in ids, "autorización activa sin entrada")
    a((o["escalation"]["state"] == "NONE") == (o["next_action"]["role"] not in ("OWNER", "COORDINATOR")), "escalation ⇔ next_action.role")
    return v


# ------------------------------------------------------------------ pair invariants
def validate_pair(p, n, variant="LITERAL", ctx=None):
    ctx = ctx or {}
    v = []
    a = lambda inv, cond, msg: (not cond) and v.append((inv, msg))
    pp, np_ = get(p, "custody.point"), get(n, "custody.point")
    rebase = get(n, "custody.point_kind") == "REBASE_RECONCILIATION"
    rmap = ctx.get("rebase_map", {})
    a("I-P01", get(n, "custody.record_version") == get(p, "custody.record_version") + 1, "record_version + 1")
    a("I-P02", np_ in I_P02.get(pp, set()), "%s → %s" % (pp, np_))
    if np_ == "Q0":
        a("I-P04", get(n, "custody.window.seq") == get(p, "custody.window.seq") + 1, "Q0 sin window.seq + 1")
    for path in ("protocol.set", "protocol.effective_sha", "protocol.basis", "automation_state.initiative", "automation_state.branch", "automation_state.claim_id"):
        a("I-P06", get(p, path) == get(n, path), path + " es inmutable")
    # I-P05: attempts and counters
    pa, na = get(p, "automation_state.attempts"), get(n, "automation_state.attempts")
    kind = (get(n, "custody.task_intent") or {}).get("kind")
    a("I-P05", na >= pa, "attempts decrece")
    if na != pa:
        a("I-P05", np_ == "Q0" and kind == "CORRECTION" and na == pa + 1, "attempts cambia fuera de un Q0 CORRECTION")
    pcl, ncl = get(p, "counters.correction_launches") or [], get(n, "counters.correction_launches") or []
    a("I-P05", ncl[:len(pcl)] == pcl, "correction_launches no es append-only")
    for c in (get(p, "custody.chains") or []):
        m = next((x for x in get(n, "custody.chains") or [] if x["task_id"] == c["task_id"]), None)
        a("I-P05", m is not None and set(c["chain_red_files"]) <= set(m["chain_red_files"]), "chain_red_files decrece o cadena borrada")
    for key in ("blocked_reruns", "rebase_recoveries", "invocations"):
        for e in get(p, "counters." + key) or []:
            idk = {"blocked_reruns": ("task_id", "phase"), "rebase_recoveries": ("task_id",), "invocations": ("scope",)}[key]
            m = next((x for x in get(n, "counters." + key) or [] if all(x[k] == e[k] for k in idk)), None)
            vals = {"blocked_reruns": ["count"], "rebase_recoveries": ["count"], "invocations": ["launched", "uncertain"]}[key]
            a("I-P05", m is not None and all(m[k] >= e[k] for k in vals), "contador %s decrece" % key)
    # mutable-field rule (B.8.3 / §8.1); REBASE_RECONCILIATION = I-P05 exception + I-P10
    diff = changed(p, n)
    if rebase:
        allowed = ["custody.record_version", "custody.point", "custody.point_kind", "custody.last_rebase", "custody.task_intent",
                   "automation_state.last_evidence_commit", "custody.last_window.verified_sha", "custody.chains", "custody.unverified_commits",
                   "counters.rebase_recoveries"]
        if np_ == "QR":
            allowed += ["custody.principal"]
        if variant == "A1":
            allowed += ["orchestration.loop.object.commit", "orchestration.review_requests"]
        bad = [d for d in diff if not starts(d, allowed)]
        a("I-P05", not bad, "la reconciliación cambia campos fuera de StateFields: %s" % bad[:4])
        for path in REBASE_SHA_FIELDS:
            o_, n_ = get(p, path), get(n, path)
            a("I-P10", o_ == n_ or rmap.get(o_) == n_, path + " no es la imagen")
        for c in get(p, "custody.chains") or []:
            m = next((x for x in get(n, "custody.chains") or [] if x["task_id"] == c["task_id"]), {})
            for f in ("chain_base_sha", "chain_red_sha"):
                a("I-P10", c[f] == m.get(f) or rmap.get(c[f]) == m.get(f), "chains.%s no es la imagen" % f)
            a("I-P10", c["chain_red_files"] == m.get("chain_red_files") and c["state"] == m.get("state"), "la reconciliación cambia la cadena")
        if variant == "A1" and get(p, "orchestration") is not None:
            v += _a1_rebase_orchestration(p, n, rmap)
    elif np_ in MUTABLE:
        bad = [d for d in diff if not starts(d, MUTABLE[np_])]
        if np_ == "QU" and get(n, "custody.point_kind") == "ORDINARY":
            bad += [d for d in diff if starts(d, ["custody.window", "custody.last_window", "custody.chains", "counters"])]
        a("I-P05", not bad, "%s cambia campos que no le corresponden: %s" % (np_, sorted(set(bad))[:4]))
    # I-P07 principal binding
    if get(p, "custody.principal.binding") != get(n, "custody.principal.binding"):
        ok = np_ == "QR" or (np_ == "Q7" and ctx.get("transfer")) or (np_ == "QU" and get(p, "custody.principal.acceptance.state") == "PENDING")
        a("I-P07", ok, "principal.binding cambia fuera de QR, Q7-TRANSFER o la aceptación")
    # I-P09 acceptances
    for path in ("protocol.g0_acceptance.state", "custody.principal.acceptance.state"):
        o_, n_ = get(p, path), get(n, path)
        if o_ != n_:
            new_holder = path.startswith("custody") and (np_ == "QR" or (np_ == "Q7" and ctx.get("transfer")))
            a("I-P09", (o_ == "PENDING" and np_ == "QU") or new_holder, path + " cambia de forma no permitida")
            if path == "protocol.g0_acceptance.state" and n_ == "ACCEPTED":
                a("I-P09", {"I62-CLASSIFICATION: I62", "I62-DELEGATED-EXECUTION: I62_DELEGATED"} <= ctx.get("decisions", set()), "G0 sin marcadores")
            if path == "custody.principal.acceptance.state" and np_ == "QU" and n_ == "ACCEPTED":
                a("I-P09", any(m.startswith("I62-PRINCIPAL-BINDING") for m in ctx.get("decisions", set())), "binding del Principal sin marcador")
    if get(p, "orchestration") is not None and get(n, "orchestration") is not None:
        v += validate_orchestration_pair(p, n, variant, rebase, rmap)
    return v


def _a1_rebase_orchestration(p, n, rmap):
    v = []
    lo, ln = get(p, "orchestration.loop.object"), get(n, "orchestration.loop.object")
    if lo != ln:
        ok = lo and ln and (lo["path"], lo["blob"]) == (ln["path"], ln["blob"]) and rmap.get(lo["commit"]) == ln["commit"]
        (not ok) and v.append(("A1-D2-4", "loop.object no es la imagen con path y blob iguales"))
    return v


def validate_orchestration_pair(p, n, variant, rebase, rmap):
    v = []
    a = lambda inv, cond, msg: (not cond) and v.append((inv, msg))
    op, on = p["orchestration"], n["orchestration"]
    lp, ln = op["loop"], on["loop"]
    edge = (lp["phase"], ln["phase"])
    closing = variant == "A1" and ln["phase"] == "NONE" and lp["phase"] != "NONE"
    if lp["phase"] != ln["phase"]:
        a("I-P13", edge in LOOP_EDGES or closing, "fase %s → %s" % edge)
    if closing:
        ok_from = lp["phase"] in ("ARCHITECT_SATISFIED", "ESCALATE_OWNER") or (lp.get("action_validity") or {}).get("ended_reason") == "EXHAUSTED"
        a("A1-D1-5", ok_from, "LOOP_CLOSED desde una fase no admitida")
        a("A1-D1-5", not any(r["state"] == "OPEN" for r in op["review_requests"]) and all(x["state"] in TERMINAL for r in op["review_requests"] for x in r["attempts"]),
          "LOOP_CLOSED con solicitud OPEN o intento no terminal")
        a("A1-D1-5", (lp.get("action_validity") or {}).get("state") == "ENDED", "LOOP_CLOSED sin vigencia terminada")
        a("A1-D1-5", ln["object"] is None and ln["authorization"] is None and ln.get("action_validity") is None and on["escalation"]["state"] == "NONE",
          "LOOP_CLOSED no deja el bucle en blanco")
        a("A1-D1-5", op["budgets"] == on["budgets"] and op["review_requests"] == on["review_requests"] and op["findings"] == on["findings"],
          "LOOP_CLOSED cambia presupuestos, solicitudes o linajes")
    if lp["object"] != ln["object"]:
        ok = edge in (("CORRECTING", "PUBLISHED"), ("NONE", "REVIEW_PENDING")) or (closing and ln["object"] is None)
        if variant == "A1" and rebase:
            ok = ok or (lp["object"] and ln["object"] and (lp["object"]["path"], lp["object"]["blob"]) == (ln["object"]["path"], ln["object"]["blob"])
                        and rmap.get(lp["object"]["commit"]) == ln["object"]["commit"])
        a("I-P13", ok, "loop.object cambia en %s → %s" % edge)
    if edge == ("CORRECTING", "PUBLISHED"):
        pe, ne = budget_entries(op, variant), budget_entries(on, variant)
        a("I-P13", sum(e["correction_rounds"] for e in ne) == sum(e["correction_rounds"] for e in pe) + 1, "PUBLISHED sin correction_rounds + 1")
    # budgets (A-1: reuse and duplicates first, so that the reason is exact)
    if variant == "A1":
        nids = [e["authorization_id"] for e in on["budgets"]]
        a("A1-D1-1", len(nids) == len(set(nids)), "budgets[] con un authorization_id duplicado")
        if edge == ("NONE", "REVIEW_PENDING"):
            a("A1-D1-4", (ln.get("action_validity") or {}).get("authorization_id") not in {e["authorization_id"] for e in op["budgets"]},
              "bucle nuevo con una autorización ya usada")
    pe = {e["authorization_id"]: e for e in budget_entries(op, variant)}
    ne = {e["authorization_id"]: e for e in budget_entries(on, variant)}
    for k, e in pe.items():
        a("I-P13", k in ne, "desaparece una entrada de presupuesto")
        if k in ne:
            for c in list(CAPS) + ["correction_rounds"]:
                a("I-P13", ne[k][c] >= e[c], "%s decrece" % c)
            if variant == "A1" and k != (ln.get("action_validity") or {}).get("authorization_id"):
                a("A1-D1-4", ne[k] == e, "cambia la entrada de una autorización cerrada")
    if variant == "A1":
        new = set(ne) - set(pe)
        a("A1-D1-4", not new or (edge == ("NONE", "REVIEW_PENDING") and new == {(ln.get("action_validity") or {}).get("authorization_id")}),
          "entrada de presupuesto nueva fuera de la apertura")
    # requests and attempts
    nrs = {r["logical_review_request_id"]: r for r in on["review_requests"]}
    newly_reserved = 0
    for r in op["review_requests"]:
        m = nrs.get(r["logical_review_request_id"])
        a("I-P13", m is not None, "desaparece una solicitud")
        if m is None:
            continue
        if r["object"] != m["object"]:
            ok = variant == "A1" and rebase and r["state"] == "OPEN" and (r["object"]["path"], r["object"]["blob"]) == (m["object"]["path"], m["object"]["blob"]) \
                and rmap.get(r["object"]["commit"]) == m["object"]["commit"]
            a("I-P13", ok, "review_requests.object es inmutable")
        ma = {x["attempt_seq"]: x for x in m["attempts"]}
        for x in r["attempts"]:
            y = ma.get(x["attempt_seq"])
            a("I-P13", y is not None, "desaparece un intento")
            if y is None:
                continue
            if x["state"] != y["state"] or x["invocation"] != y["invocation"]:
                edge_a = (x["state"], y["state"])
                replan_planned = variant == "A1" and rebase and edge_a == ("INVOCATION_PLANNED", "INVOCATION_PLANNED")
                a("I-P13", edge_a in ATTEMPT_EDGES or replan_planned, "intento %s → %s" % edge_a)
                if x["invocation"] != y["invocation"]:
                    a("I-P13", x["state"] in NOT_LAUNCHED and y["state"] == x["state"], "invocation nueva desde LAUNCHING")
                    if rebase and variant == "A1":
                        a("A1-D2-2", y["target_commit"] == rmap.get(x["target_commit"]) and y["reserved_at"] == x["reserved_at"], "replanificación sin imagen o fuera de la reserva")
            if x["target_commit"] != y["target_commit"]:
                a("I-P13", x["state"] in NOT_LAUNCHED and x["invocation"] != y["invocation"], "Target de un intento lanzado o terminal reescrito")
            a("I-P13", x["state"] not in TERMINAL or x == y, "un intento terminal cambia")
            a("I-P13", x["reserved_at"] is None or y["reserved_at"] == x["reserved_at"], "reserved_at cambia")
            if x["reserved_at"] is None and y["reserved_at"] is not None:
                newly_reserved += 1
            if y["state"] == "RESULT_INGESTED" and x["state"] != "RESULT_INGESTED":
                a("I-P13", x["state"] == "RESULT_RECEIVED", "ingestión sin custodia previa")
    pr_ids = {r["logical_review_request_id"] for r in op["review_requests"]}
    for r in on["review_requests"]:
        if r["logical_review_request_id"] not in pr_ids:
            newly_reserved += sum(1 for x in r["attempts"] if x["reserved_at"] is not None)
    plaunch = sum(e["architect_launches"] for e in pe.values())
    nlaunch = sum(e["architect_launches"] for e in ne.values())
    a("I-P13", nlaunch - plaunch == newly_reserved, "architect_launches no crece exactamente con las reservas nuevas")
    if rebase:
        a("I-P10", lp["phase"] == ln["phase"] and op["findings"] == on["findings"] and op["budgets"] == on["budgets"], "la reconciliación cambia fase, linajes o presupuestos")
    # action validity
    av_p, av_n = lp.get("action_validity"), ln.get("action_validity")
    if av_p and av_n and av_p["authorization_id"] == av_n["authorization_id"]:
        a("I-P13", not (av_p["state"] == "ENDED" and av_n["state"] == "OPEN"), "vigencia ENDED → OPEN")
    if newly_reserved:
        a("I-P13", av_n is not None and av_n["state"] == "OPEN", "reserva nueva con la vigencia terminada")
    return v


def validate_history(state, ancestors, variant="LITERAL"):
    """I-H02 outside an active window: every live branch SHA of the state is an ancestor of HEAD (A-1 adds the live orchestration commits)."""
    if get(state, "custody.point") == "Q0":
        return []
    shas = {"automation_state.last_evidence_commit": get(state, "automation_state.last_evidence_commit"),
            "custody.last_window.verified_sha": get(state, "custody.last_window.verified_sha")}
    for c in get(state, "custody.chains") or []:
        shas["chains[%s].chain_base_sha" % c["task_id"]] = c["chain_base_sha"]
        if c["chain_red_sha"]:
            shas["chains[%s].chain_red_sha" % c["task_id"]] = c["chain_red_sha"]
    o = state.get("orchestration")
    if variant == "A1" and o:
        if o["loop"]["object"]:
            shas["orchestration.loop.object.commit"] = o["loop"]["object"]["commit"]
        for r in o["review_requests"]:
            if r["state"] == "OPEN":
                shas["review_requests[%s].object.commit" % r["logical_review_request_id"]] = r["object"]["commit"]
            for x in r["attempts"]:
                if x["state"] in NOT_LAUNCHED:
                    shas["review_requests[%s].attempts[%d].Target.commit" % (r["logical_review_request_id"], x["attempt_seq"])] = x["target_commit"]
    return [("I-H02", "%s = %s no es ancestro de HEAD" % (k, s)) for k, s in shas.items() if s and s not in ancestors]
