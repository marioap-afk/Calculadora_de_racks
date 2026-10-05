"""I-62 A-1 (FC-01, FC-02), corrected version (2026-10-05): exact counterexamples against the literal Freeze and against the corrected delta.

F4 PREPARATION, not F4 production: this is not the state/v2 validator and implements no state machine. It encodes, for the fields that FC-01 and FC-02
touch, the literal text of Proposal V14 (commit 4c617e82, blob 34ad80ea) and the corrected delta of docs/initiatives/I-62-A-1.md, and runs symbolic
traces through both. A trace is a list of durable points; each pair (p, n) is checked like I-P13 / I-P05 / I-P10 check pairs, each point like
I-S18 / I-H02 check files.

Every trace declares its expected verdict AND, for INVALID, the EXACT set of rule ids it must violate. A trace passes only if both match, so no negative
trace can pass by an incidental failure (Coordinator disposition of A62-A1-O5). Each trace is tagged with the findings it covers; the result lists the
coverage of A62-A1-01..06 and of the accepted OPTIONAL O1..O5.

Modelled (O5): loop.type (ARCHITECT_REVIEW, REVIEWER, NONE), loop.instance_id, escalation, findings (lineages), action validity with replacement,
continuation, EXPIRED and REVOKED, the per-loop budget entries with their authorization history, review requests and attempts with invocation, Target,
reservation, branch-local references, result and outcome, the RebaseMap chain (custody.rebase_history), ResolveBranchRef and EquivalentReviewedObject,
and (OBS-A1-01, decisions §39) the REVIEWER completion REVIEWER_SATISFIED, its two-path LOOP_CLOSED, reviewer_closures[] and the loop-type guard (R1..R10);
and (decisions §41, formal review R20261005T044948Z-ac67) the closure (E) after EXHAUSTED or with a revoking decision, the REVIEWER opening without
resurrecting an ended authority, the per-type rebase of loop.object (REVIEWER and EXECUTION), OpenFindings by reviewer authority, every window map in
rebase_history and the positive-only operational satisfaction; and (A62-A1A-01, author correction) an inherited REVIEWER BLOCKING lineage
blocks REVIEWER_SATISFIED; and (A62-A1S-01..02, formal review R20261005T063359Z-ac67-86e3) the REVIEWER loop membership by opening
and the REVIEWER correction cycle CORRECTING -> PUBLISHED with a new request on the corrected object.
The two phase edges ARCHITECT_INVOKED -> REVIEW_PENDING / REREVIEW_PENDING are SM-05 (non-material, freeze-issues.md), not part of A-1.

Usage: python a1-counterexamples.py <output json>
"""
import copy
import json
import sys

AR, RV, NONE = "ARCHITECT_REVIEW", "REVIEWER", "NONE"
FROZEN = {"review_rounds": 3, "logical_requests": 3, "architect_launches": 9}
COUNTERS = ("review_rounds", "logical_requests", "architect_launches")
EDGES = {("NONE", "REVIEW_PENDING"), ("REVIEW_PENDING", "ARCHITECT_INVOKED"), ("ARCHITECT_INVOKED", "RESULT_INGESTED"),
         ("ARCHITECT_INVOKED", "ARCHITECT_SATISFIED"), ("ARCHITECT_INVOKED", "CORRECTING"), ("ARCHITECT_INVOKED", "ESCALATE_OWNER"),
         ("RESULT_INGESTED", "ARCHITECT_SATISFIED"), ("RESULT_INGESTED", "CORRECTING"), ("RESULT_INGESTED", "ESCALATE_OWNER"),
         ("CORRECTING", "PUBLISHED"), ("PUBLISHED", "CI_VERIFIED"), ("CI_VERIFIED", "REREVIEW_PENDING"), ("REREVIEW_PENDING", "ARCHITECT_INVOKED"),
         ("ARCHITECT_INVOKED", "REVIEW_PENDING"), ("ARCHITECT_INVOKED", "REREVIEW_PENDING"),  # SM-05
         ("ARCHITECT_INVOKED", "REVIEWER_SATISFIED"), ("RESULT_INGESTED", "REVIEWER_SATISFIED")}  # D1-16 (REVIEWER only)
ATTEMPT_EDGES_V14 = {("INVOCATION_PLANNED", "BUDGET_RESERVED"), ("INVOCATION_PLANNED", "CANCELLED_BEFORE_LAUNCH"), ("BUDGET_RESERVED", "LAUNCHING"),
                     ("BUDGET_RESERVED", "CANCELLED_BEFORE_LAUNCH"), ("BUDGET_RESERVED", "BUDGET_RESERVED"), ("LAUNCHING", "LAUNCHED"),
                     ("LAUNCHING", "RESULT_RECEIVED"), ("LAUNCHING", "LAUNCH_UNCERTAIN"), ("LAUNCHING", "BUDGET_RESERVED"),
                     ("LAUNCHING", "CANCELLED_BEFORE_LAUNCH"), ("LAUNCHED", "RESULT_RECEIVED"), ("LAUNCHED", "LAUNCH_UNCERTAIN"),
                     ("RESULT_RECEIVED", "RESULT_INGESTED")}
ATTEMPT_EDGES_A1 = ATTEMPT_EDGES_V14 | {("INVOCATION_PLANNED", "INVOCATION_PLANNED")}  # D2-6: only in REBASE_RECONCILIATION
TERMINAL = {"RESULT_INGESTED", "LAUNCH_UNCERTAIN", "CANCELLED_BEFORE_LAUNCH"}
NOT_LAUNCHED = {"INVOCATION_PLANNED", "BUDGET_RESERVED"}
CLOSABLE_REASONS = {"ARCHITECT_SATISFIED", "EXHAUSTED", "EXPIRED", "REVOKED"}

RULES = {
    "A1-F01": "D1-5 · I-S18: contadores de cada entrada de architect_budgets[] (architect_launches = intentos reservados de sus solicitudes; review_rounds ≤ logical_requests; ningún contador sobre su tope)",
    "A1-F02": "D1-2 · topes efectivos = mínimo entre las constantes congeladas y el Budget de cada autorización de la entrada",
    "A1-F03": "D1-5 · un bucle ARCHITECT_REVIEW activo tiene instance_id, su entrada abierta y su última autorización = action_validity",
    "A1-F04": "D1-5 · como máximo una entrada abierta, la del bucle activo",
    "A1-F05": "D1-5 · toda solicitud con loop_instance_id tiene su entrada",
    "A1-F06": "D1-13 · un bucle REVIEWER o EXECUTION no tiene instance_id ni solicitudes abiertas con loop_instance_id",
    "A1-F07": "D1-4 · el objeto budgets de V14 cuenta solo las solicitudes con loop_instance_id = null",
    "A1-F09": "D2-9 · custody.rebase_history[] termina en last_rebase.map",
    "A1-F10": "D2-10/D2-11 · toda referencia de rama de una invocación resuelve por ResolveBranchRef",
    "A1-P01": "§20.5 + D1-9 (+ SM-05) · la transición de fase existe",
    "A1-P02": "D1-10/D2-4 · loop.object cambia solo en CORRECTING → PUBLISHED, en la apertura, a null solo en LOOP_CLOSED de ARCHITECT_REVIEW y a su imagen probada en REBASE_RECONCILIATION",
    "A1-P03": "D1-1 · instance_id = ARL-<record_version> en la apertura, inmutable con el bucle abierto, null tras LOOP_CLOSED",
    "A1-P04": "D1-7 · entrada nueva solo en la apertura: exactamente una, con una ReviewLoopAuthorization nueva sin ContinuesLoopInstanceId y nunca usada",
    "A1-P05": "D1-8 · sustitución o continuación dentro del bucle abierto: misma entrada, sin reinicio, SUPERSEDED solo desde OPEN, EXPIRED/REVOKED intactos",
    "A1-P06": "D1-6 · las entradas no desaparecen, sus contadores no decrecen, una entrada cerrada no cambia, los topes no suben y un registro de autorización terminado no cambia",
    "A1-P07": "D1-9/D1-13 · LOOP_CLOSED de ARCHITECT_REVIEW solo sin trabajo vivo, con la escalada resuelta, la vigencia terminada (o revocada por la decisión de cierre) y la decisión exigida; ninguna variante de LOOP_CLOSED se aplica a EXECUTION",
    "A1-R01": "D1-16 · ARCHITECT_SATISFIED (fase o fin de vigencia) solo con ARCHITECT_REVIEW y REVIEWER_SATISFIED solo con REVIEWER",
    "A1-R02": "D1-17 · REVIEWER_SATISFIED solo con un resultado de REVIEWER VALID ingerido en la última solicitud del bucle, sin intentos no terminales, sin linajes BLOCKING de REVIEWER abiertos en la unidad (también los heredados, A62-A1A-01) y con la vigencia terminada por REVIEWER_SATISFIED",
    "A1-R03": "D1-18 · LOOP_CLOSED de REVIEWER: desde REVIEWER_SATISFIED sin decisión, o (E) tras EXHAUSTED/EXPIRED/REVOKED (o con la vigencia OPEN revocada por la misma decisión) con la decisión I62-REVIEWER-LOOP-CLOSE; sin trabajo vivo, escalada resuelta, nada borrado ni reiniciado y el registro de cierre con el motivo real",
    "A1-R04": "D1-19 · reviewer_closures[] es append-only y crece exactamente en uno en cada LOOP_CLOSED de REVIEWER, y en ningún otro par",
    "V14-P20-reviewer": "V14 §20.5.2/§20.7 · un resultado de REVIEWER nunca cierra ni rebaja un linaje del ARCHITECT (P-20)",
    "A1-R05": "D1-20 · un bucle REVIEWER se abre con una autoridad OPEN, la de loop.authorization, que no figura en ningún registro de reviewer_closures[]",
    "A1-P18": "V14 B.8.8 (review_requests.object) · una solicitud nueva se abre con object = loop.object del punto en que se abre (A62-A1S-02)",
    "A1-R06": "D1-17/D1-19 · el requisito operativo de una autoridad REVIEWER solo está satisfecho con evidencia REVIEWER_SATISFIED de esa autoridad",
    "A1-P08": "D2-2/D2-5 · la reconciliación lleva cada objeto vivo a su imagen probada (mismo path y blob) y exige imagen en el mapa para el Target de un intento en LAUNCHING",
    "A1-P09": "D2-6 · transiciones de intento; un intento en LAUNCHING o posterior nunca cambia su invocación en sitio; B.1 tras un rebase replanifica sobre la imagen",
    "A1-P10": "D2-8 · la reconciliación no cambia fase, presupuestos, linajes, estados de intento ni reserved_at",
    "A1-P11": "D2-9 · rebase_history recibe exactamente el mapa nuevo en cada reconciliación, todos los mapas de la ventana en orden al cerrarla, y no cambia en otro caso",
    "A1-P12": "D2-12 · un resultado solo cierra linajes y solo es VALID si su EvaluatedObject es EquivalentReviewedObject del objeto de su solicitud",
    "A1-P13": "V14 I-P13 · una acción nueva (reserva, LAUNCHING) exige la vigencia de acción OPEN",
    "A1-P14": "D1-3 · loop_instance_id de una solicitud es inmutable",
    "A1-P15": "D2-2/D2-11 · una invocación nueva o replanificada tras un rebase reconstruye sus referencias de rama sobre las imágenes",
    "A1-P16": "D1-15 · el OpenFindings de una reserva es exactamente el conjunto de linajes abiertos que su revisor debe disponer: ARCHITECT y COORDINATOR para el Architect, REVIEWER para el REVIEWER, también los heredados",
    "A1-P17": "D1-14 · el BudgetSnapshot de una reserva ARCHITECT copia la entrada de su loop_instance_id, no una cuenta por autorización",
    "I-H02": "D2-3 · todo SHA de rama vivo del estado (incluidos loop.object, objetos OPEN y Targets no lanzados) es ancestro de HEAD fuera de una ventana",
    "V14-S18-target": "V14 §20.6 · el Target de un intento no lanzado es el objeto de su solicitud",
    "V14-S18-vigencia": "V14 I-S18 · con la vigencia terminada no hay intentos en INVOCATION_PLANNED ni en BUDGET_RESERVED",
    "V14-§20.5": "V14 literal · la transición de fase existe (sin vuelta a NONE)",
    "V14-I-P13-object": "V14 literal · loop.object cambia solo en CORRECTING → PUBLISHED o NONE → REVIEW_PENDING",
    "V14-B.8.8-object": "V14 literal · review_requests[].object es inmutable",
    "V14-I-P05": "V14 literal · la reconciliación cambia solo los campos SHA de StateFields",
    "V14-I-S18": "V14 literal · un solo objeto budgets por unidad; ningún contador sobre su tope",
    "V14-I-S18-ancestro": "V14 literal · AuthorizationRef y BindingRef CUSTODIED con Commit ancestro del punto",
}


# ------------------------------------------------------------------ helpers
def entry_of(s, iid):
    return next((e for e in s["entries"] if e["id"] == iid), None)


def reserved(s, pred):
    return sum(1 for r in s["requests"] if pred(r) for a in r["attempts"] if a["reserved_at"] is not None)


def caps_of(s, e):
    caps = dict(FROZEN)
    for a in e["auths"]:
        for k, v in s["decisions"][a["auth"]]["budget"].items():
            caps[k] = min(caps[k], v)
    return caps


def chain(c, s):
    """Image of commit c through the custodied RebaseMap chain (D2-10, without the final HEAD and blob checks); None if unproven."""
    hist, maps = s["history"], s["maps"]
    first = next((i for i, m in enumerate(hist) if c in maps[m]["commits"]), None)
    if first is None:
        return None
    cur = maps[hist[first]]["commits"][c]
    for m in hist[first + 1:]:
        if cur in maps[m]["commits"]:
            cur = maps[m]["commits"][cur]
        elif cur not in maps[m]["main_before"]:
            return None  # a step of the chain is missing: never guessed
    return cur


def resolve(ref, s):
    """ResolveBranchRef (D2-10): RESOLVED commit or None."""
    c = ref["commit"]
    if c in s["anc"] and s["blobs"].get((c, ref["path"])) == ref["blob"]:
        return c
    img = chain(c, s)
    if img is not None and img in s["anc"] and s["blobs"].get((img, ref["path"])) == ref["blob"]:
        return img
    return None


def equivalent(a, b, s):
    """EquivalentReviewedObject (D2-12)."""
    if (a["path"], a["blob"]) != (b["path"], b["blob"]):
        return False
    return a["commit"] == b["commit"] or chain(a["commit"], s) == b["commit"] or chain(b["commit"], s) == a["commit"]


def rv_loop_requests(s):
    """D1-17 (A62-A1S-01): requests of the active REVIEWER loop = loop_instance_id null, opened after the pair that opened the loop (i.e. after the
    last REVIEWER closure), under any authority that governed it; never identified by the current authority."""
    last = max([c["closed_at"] for c in s["rclosures"]] or [-1])

    def opened(r):
        rs = [a["reserved_at"] for a in r["attempts"] if a["reserved_at"] is not None]
        return min(rs) if rs else float("inf")
    return [r for r in s["requests"] if r["loop"] is None and opened(r) > last]


def open_validity(s):
    return s["validity"] is not None and s["validity"]["state"] == "OPEN"


def mapped(s):
    return s["maps"][s["last_rebase"]]["commits"] if s["last_rebase"] else {}


# ------------------------------------------------------------------ corrected A-1: file checks
def file_a1(s):
    v = []
    add = lambda rule, msg: v.append((rule, msg))
    for e in s["entries"]:
        al = reserved(s, lambda r, i=e["id"]: r["loop"] == i)
        if e["architect_launches"] != al or e["review_rounds"] > e["logical_requests"] or any(e[k] > e["caps"][k] for k in COUNTERS):
            add("A1-F01", "contadores de %s incoherentes o sobre su tope (P-18)" % e["id"])
        if e["caps"] != caps_of(s, e):
            add("A1-F02", "topes de %s ≠ mínimo de las autorizaciones %s" % (e["id"], caps_of(s, e)))
    lp = s["loop"]
    if lp["type"] == AR:
        e = entry_of(s, lp["instance"])
        val = s["validity"]
        last = e["auths"][-1] if e else None
        if e is None or e["closed_at"] is not None or val is None or last is None or \
                (last["auth"], last["state"], last["reason"]) != (val["auth"], val["state"], val["reason"]):
            add("A1-F03", "bucle activo sin su entrada abierta coherente con action_validity")
    for e in s["entries"]:
        if e["closed_at"] is None and not (lp["type"] == AR and e["id"] == lp["instance"]):
            add("A1-F04", "entrada abierta %s que no es la del bucle activo" % e["id"])
    for r in s["requests"]:
        if r["loop"] is not None and entry_of(s, r["loop"]) is None:
            add("A1-F05", "solicitud %s sin entrada %s" % (r["id"], r["loop"]))
    if lp["type"] in (RV, "EXECUTION") and (lp["instance"] is not None or any(r["state"] == "OPEN" and r["loop"] is not None for r in s["requests"])):
        add("A1-F06", "bucle %s con identidad de bucle del Architect" % lp["type"])
    for a in s.get("satisfied", []):
        ok = (lp["type"] == RV and lp["phase"] == "REVIEWER_SATISFIED" and lp["authorization"] == a) or \
             any(r["authorization"] == a and (r["validity"] or {}).get("reason") == "REVIEWER_SATISFIED" for r in s["rclosures"])
        if not ok:
            add("A1-R06", "requisito de %s dado por satisfecho sin evidencia REVIEWER_SATISFIED" % a)
    val_reason = s["validity"]["reason"] if s["validity"] else None
    if ((lp["phase"] == "ARCHITECT_SATISFIED" or val_reason == "ARCHITECT_SATISFIED") and lp["type"] != AR) or \
            ((lp["phase"] == "REVIEWER_SATISFIED" or val_reason == "REVIEWER_SATISFIED") and lp["type"] != RV):
        add("A1-R01", "fase o fin de vigencia SATISFIED de otro tipo de bucle (%s)" % lp["type"])
    al14 = reserved(s, lambda r: r["loop"] is None)
    if s["v14"]["architect_launches"] != al14 or any(s["v14"][k] > FROZEN[k] for k in COUNTERS):
        add("A1-F07", "el objeto budgets de V14 no cuenta solo las solicitudes sin loop_instance_id")
    for r in s["requests"]:
        for a in r["attempts"]:
            if a["state"] in NOT_LAUNCHED and a["target"] != r["object"]:
                add("V14-S18-target", "Target de %s/%d ≠ objeto de su solicitud" % (r["id"], a["seq"]))
            if a["state"] in NOT_LAUNCHED and s["validity"] is not None and s["validity"]["state"] == "ENDED":
                add("V14-S18-vigencia", "intento %s/%d no lanzado con la vigencia terminada" % (r["id"], a["seq"]))
            for ref in a["refs"]:
                if resolve(ref, s) is None:
                    add("A1-F10", "referencia %s@%s de %s/%d no resuelve" % (ref["path"], ref["commit"], r["id"], a["seq"]))
    if s["last_point"] != "Q0":
        live = []
        if lp["object"] is not None:
            live.append(("loop.object", lp["object"]["commit"]))
        for r in s["requests"]:
            if r["state"] == "OPEN":
                live.append(("review_requests[%s].object" % r["id"], r["object"]["commit"]))
            live += [("%s/%d.Target" % (r["id"], a["seq"]), a["target"]["commit"]) for a in r["attempts"] if a["state"] in NOT_LAUNCHED]
        for field, c in live:
            if c not in s["anc"]:
                add("I-H02", "%s = %s no es ancestro de HEAD" % (field, c))
    if s["last_rebase"] is not None and (not s["history"] or s["history"][-1] != s["last_rebase"]):
        add("A1-F09", "rebase_history no termina en last_rebase")
    return v


# ------------------------------------------------------------------ corrected A-1: pair checks
def new_reservation(v, a, r, n):
    if not open_validity(n):
        v.append(("A1-P13", "reserva de %s/%d sin vigencia OPEN" % (r["id"], a["seq"])))
    issuers = ("REVIEWER",) if r["loop"] is None else ("ARCHITECT", "COORDINATOR")
    required = {f["lineage"] for f in n["findings"] if f["state"] in ("OPEN", "STILL_OPEN") and f["issuer"] in issuers}
    if set(a["open_findings"]) != required:
        v.append(("A1-P16", "OpenFindings de %s/%d = %s, debe ser %s" % (r["id"], a["seq"], sorted(a["open_findings"]), sorted(required))))
    if r["loop"] is None:
        return
    e = entry_of(n, r["loop"])
    if e is not None and a["snapshot"] != {k: e[k] for k in COUNTERS}:
        v.append(("A1-P17", "BudgetSnapshot de %s/%d ≠ entrada %s" % (r["id"], a["seq"], e["id"])))


def pair_a1(p, n):
    v = []
    add = lambda rule, msg: v.append((rule, msg))
    pl, nl = p["loop"], n["loop"]
    edge = (pl["phase"], nl["phase"])
    rebase = n["kind"] == "REBASE_RECONCILIATION"
    closing = pl["type"] == AR and nl["type"] == NONE
    opening = pl["type"] == NONE and nl["type"] == AR
    same = pl["type"] == AR and nl["type"] == AR
    rv_closing = pl["type"] == RV and nl["type"] == NONE
    mp = mapped(n)
    if pl["phase"] != nl["phase"] and edge not in EDGES and not closing and not rv_closing:
        add("A1-P01", "transición %s → %s no existe" % edge)
    if pl["type"] not in (NONE, AR, RV) and nl["type"] == NONE:
        add("A1-P07", "LOOP_CLOSED no se aplica a un bucle %s" % pl["type"])
    if pl["object"] != nl["object"]:
        ok = (edge == ("CORRECTING", "PUBLISHED") and nl["object"] is not None) or \
             (pl["type"] == NONE and pl["object"] is None and nl["object"] is not None and edge == ("NONE", "REVIEW_PENDING")) or \
             ((closing or rv_closing) and nl["object"] is None)
        if rebase and pl["object"] and nl["object"] and pl["phase"] == nl["phase"] and \
                (pl["object"]["path"], pl["object"]["blob"]) == (nl["object"]["path"], nl["object"]["blob"]) and mp.get(pl["object"]["commit"]) == nl["object"]["commit"]:
            ok = True
        if not ok:
            add("A1-P02", "loop.object cambia en %s → %s" % edge)
    if (opening and nl["instance"] != "ARL-%d" % n["rv"]) or (same and nl["instance"] != pl["instance"]) or (closing and nl["instance"] is not None):
        add("A1-P03", "instance_id incorrecto")
    pe = {e["id"]: e for e in p["entries"]}
    ne = {e["id"]: e for e in n["entries"]}
    used = {a["auth"] for e in p["entries"] for a in e["auths"]}
    new_ids = set(ne) - set(pe)
    if opening:
        a_id = n["validity"]["auth"] if n["validity"] else None
        d = n["decisions"].get(a_id, {})
        e = ne.get(nl["instance"])
        if new_ids != {nl["instance"]} or e is None or e["auths"] != [auth(a_id)] or d.get("kind") != "RLA" or d.get("continues") is not None \
                or a_id in used or nl["authorization"] != a_id:
            add("A1-P04", "apertura sin exactamente una entrada nueva con una autorización nueva no usada")
    elif new_ids:
        add("A1-P04", "entrada nueva %s fuera de la apertura" % sorted(new_ids))
    if same and p["validity"] and n["validity"] and n["validity"]["auth"] != p["validity"]["auth"]:
        a2 = n["validity"]["auth"]
        d = n["decisions"].get(a2, {})
        ep, en = pe.get(pl["instance"]), ne.get(nl["instance"])
        bad = d.get("kind") != "RLA" or d.get("continues") != pl["instance"] or a2 in used or ep is None or en is None
        if not bad:
            last = ep["auths"][-1]
            if last["state"] == "OPEN":
                prev = dict(last, state="ENDED", reason="SUPERSEDED", by=a2)
            elif last["reason"] in ("EXPIRED", "REVOKED"):
                prev = last
            else:
                prev, bad = None, True
            if prev is not None and en["auths"] != ep["auths"][:-1] + [prev, auth(a2)]:
                bad = True
            if any(en[k] != ep[k] for k in COUNTERS):
                bad = True
        if pl["phase"] != nl["phase"] or pl["object"] != nl["object"]:
            bad = True
        if bad:
            add("A1-P05", "sustitución o continuación inválida dentro del bucle %s" % pl["instance"])
    for iid, e in pe.items():
        f = ne.get(iid)
        if f is None:
            add("A1-P06", "desaparece la entrada %s" % iid)
            continue
        active = pl["type"] == AR and pl["instance"] == iid
        if any(f[k] < e[k] for k in COUNTERS) or (not active and f != e) or any(f["caps"][k] > e["caps"][k] for k in COUNTERS):
            add("A1-P06", "la entrada %s decrece, sube sus topes o cambia cerrada" % iid)
        for i, a in enumerate(e["auths"]):
            g = f["auths"][i] if i < len(f["auths"]) else None
            if g is None or (a["state"] == "ENDED" and g != a) or g["auth"] != a["auth"]:
                add("A1-P06", "cambia un registro de autorización de %s" % iid)
    if any(n["v14"][k] < p["v14"][k] for k in COUNTERS):
        add("A1-P06", "decrece un contador de budgets (V14)")
    p_ids = {r["id"] for r in p["requests"]}
    for r in n["requests"]:
        if r["id"] not in p_ids and r["object"] != nl["object"]:
            add("A1-P18", "solicitud nueva %s abierta sobre un objeto distinto de loop.object" % r["id"])
    if nl["type"] == RV and nl["phase"] == "REVIEWER_SATISFIED" and pl["phase"] != "REVIEWER_SATISFIED":
        lr = rv_loop_requests(n)
        ids = {r["id"] for r in lr}
        ok = bool(lr) and lr[-1]["state"] == "INGESTED" and any(a["state"] == "RESULT_INGESTED" and a["outcome"] == "VALID" for a in lr[-1]["attempts"])
        ok = ok and all(a["state"] in TERMINAL for r in lr for a in r["attempts"])
        ok = ok and not any(f["issuer"] == "REVIEWER" and f["severity"] == "BLOCKING" and f["state"] in ("OPEN", "STILL_OPEN")
                            for f in n["findings"])  # D1-17 (3): de la unidad, también heredados (A62-A1A-01)
        ok = ok and n["validity"] == val(nl["authorization"], "ENDED", "REVIEWER_SATISFIED")
        if not ok:
            add("A1-R02", "REVIEWER_SATISFIED sin sus condiciones")
    if pl["type"] == NONE and nl["type"] == RV:
        nv = n["validity"]
        ended_ids = {(r["validity"] or {}).get("auth") for r in p["rclosures"]}
        if not nv or nv["state"] != "OPEN" or nv["auth"] != nl["authorization"] or nv["auth"] in ended_ids:
            add("A1-R05", "bucle REVIEWER abierto con una autoridad terminada o distinta de loop.authorization")
    if rv_closing:
        lr = rv_loop_requests(p)
        ids = {r["id"] for r in lr}
        bad = not lr or any(r["state"] == "OPEN" for r in lr) or any(a["state"] not in TERMINAL for r in lr for a in r["attempts"])
        bad = bad or (p["escalation"]["state"] != NONE and not p["escalation"]["resolved_by"]) or n["escalation"]["state"] != NONE
        vp, decision = p["validity"], None
        if vp and vp["state"] == "ENDED" and vp["reason"] == "REVIEWER_SATISFIED" and pl["phase"] == "REVIEWER_SATISFIED":
            bad = bad or any(f["issuer"] == "REVIEWER" and f["severity"] == "BLOCKING" and f["state"] in ("OPEN", "STILL_OPEN")
                             for f in p["findings"])  # D1-18 (S) con D1-17 (3)
        elif vp and vp["state"] == "ENDED" and vp["reason"] in ("EXHAUSTED", "EXPIRED", "REVOKED"):
            decision = n["rclosures"][-1]["closed_by"] if n["rclosures"] else None
            dd = n["decisions"].get(decision, {})
            bad = bad or dd.get("kind") != "REVIEWER_CLOSE" or not lr or dd.get("request") != lr[-1]["id"]
        elif vp and vp["state"] == "OPEN":
            decision = n["rclosures"][-1]["closed_by"] if n["rclosures"] else None
            dd = n["decisions"].get(decision, {})
            bad = bad or dd.get("kind") != "REVIEWER_CLOSE" or not dd.get("revokes") or not lr or dd.get("request") != lr[-1]["id"]
            vp = dict(vp, state="ENDED", reason="REVOKED", by=decision)
        else:
            bad = True
        bad = bad or nl["authorization"] is not None or n["validity"] is not None or nl["object"] is not None or nl["phase"] != NONE
        bad = bad or p["requests"] != n["requests"] or p["findings"] != n["findings"] or p["v14"] != n["v14"]
        rec = {"last_request": lr[-1]["id"] if lr else None, "authorization": pl["authorization"], "validity": vp, "closed_at": n["rv"], "closed_by": decision}
        bad = bad or not n["rclosures"] or n["rclosures"][-1] != rec
        if bad:
            add("A1-R03", "LOOP_CLOSED de REVIEWER sin sus condiciones")
    if n["rclosures"][:len(p["rclosures"])] != p["rclosures"] or len(n["rclosures"]) != len(p["rclosures"]) + (1 if rv_closing else 0):
        add("A1-R04", "reviewer_closures cambia fuera de un cierre de REVIEWER o pierde historia")
    if closing:
        iid = pl["instance"]
        ep, en = pe.get(iid), ne.get(iid)
        reqs = [r for r in p["requests"] if r["loop"] == iid]
        bad = any(r["state"] == "OPEN" for r in reqs) or any(a["state"] not in TERMINAL for r in reqs for a in r["attempts"])
        bad = bad or (p["escalation"]["state"] != NONE and not p["escalation"]["resolved_by"]) or n["escalation"]["state"] != NONE
        bad = bad or en is None or ep is None or en["closed_at"] != n["rv"]
        vp = p["validity"]
        need = True
        if not bad and vp and vp["state"] == "ENDED" and vp["reason"] in CLOSABLE_REASONS:
            need = vp["reason"] != "ARCHITECT_SATISFIED"
            bad = en["auths"] != ep["auths"]
        elif not bad and vp and vp["state"] == "OPEN":
            dd = n["decisions"].get(en["closed_by"], {})
            bad = not dd.get("revokes") or en["auths"] != ep["auths"][:-1] + [dict(ep["auths"][-1], state="ENDED", reason="REVOKED", by=en["closed_by"])]
        else:
            bad = True
        if not bad and need:
            dd = n["decisions"].get(en["closed_by"], {})
            bad = dd.get("kind") != "CLOSE" or dd.get("loop") != iid
        bad = bad or nl["authorization"] is not None or n["validity"] is not None or nl["object"] is not None or nl["phase"] != NONE
        bad = bad or p["requests"] != n["requests"] or p["findings"] != n["findings"] or (en is not None and ep is not None and any(en[k] != ep[k] for k in COUNTERS))
        if bad:
            add("A1-P07", "LOOP_CLOSED de %s sin sus condiciones" % iid)
    prs = {r["id"]: r for r in p["requests"]}
    pf = {f["lineage"]: f for f in p["findings"]}
    for r in n["requests"]:
        q = prs.get(r["id"])
        if q is None:
            for a in r["attempts"]:
                if a["reserved_at"] is not None:
                    new_reservation(v, a, r, n)
            continue
        if q["loop"] != r["loop"]:
            add("A1-P14", "cambia loop_instance_id de %s" % r["id"])
        if q["object"] != r["object"]:
            if not (rebase and q["state"] == "OPEN" and (q["object"]["path"], q["object"]["blob"]) == (r["object"]["path"], r["object"]["blob"])
                    and mp.get(q["object"]["commit"]) == r["object"]["commit"]):
                add("A1-P08", "objeto de %s cambia sin imagen probada" % r["id"])
        qa = {a["seq"]: a for a in q["attempts"]}
        for a in r["attempts"]:
            b = qa.get(a["seq"])
            if b is None:
                if a["reserved_at"] is not None:
                    new_reservation(v, a, r, n)
                continue
            tr = (b["state"], a["state"])
            if (b["state"] != a["state"] and tr not in ATTEMPT_EDGES_A1) or (tr == ("INVOCATION_PLANNED", "INVOCATION_PLANNED") and not rebase) \
                    or (b["state"] in TERMINAL and a != b):
                add("A1-P09", "intento %s/%d %s → %s" % (r["id"], a["seq"], b["state"], a["state"]))
            if b["target"] != a["target"] or b["inv"] != a["inv"]:
                if b["state"] in NOT_LAUNCHED and a["state"] == b["state"]:
                    if a["inv"] == b["inv"] or a["reserved_at"] != b["reserved_at"]:
                        add("A1-P09", "replanificación de %s/%d sin InvocationId nuevo o fuera de su reserva" % (r["id"], a["seq"]))
                    if rebase and (mp.get(b["target"]["commit"]) != a["target"]["commit"] or
                                   (b["target"]["path"], b["target"]["blob"]) != (a["target"]["path"], a["target"]["blob"])):
                        add("A1-P08", "Target replanificado de %s/%d no es la imagen probada" % (r["id"], a["seq"]))
                    if rebase and any(x["commit"] not in n["anc"] for x in a["refs"]):
                        add("A1-P15", "invocación replanificada de %s/%d con referencias originales" % (r["id"], a["seq"]))
                elif tr == ("LAUNCHING", "BUDGET_RESERVED"):
                    img = b["target"]["commit"] if b["target"]["commit"] in n["anc"] else chain(b["target"]["commit"], n)
                    if a["inv"] == b["inv"] or a["reserved_at"] != b["reserved_at"] or a["target"] != dict(b["target"], commit=img):
                        add("A1-P09", "B.1 de %s/%d sin invocación nueva sobre la imagen" % (r["id"], a["seq"]))
                    if any(x["commit"] not in n["anc"] for x in a["refs"]):
                        add("A1-P15", "invocación replanificada de %s/%d con referencias originales" % (r["id"], a["seq"]))
                else:
                    add("A1-P09", "invocación o Target de %s/%d (%s) reescritos en sitio" % (r["id"], a["seq"], b["state"]))
            elif tr == ("LAUNCHING", "BUDGET_RESERVED") and b["target"]["commit"] not in n["anc"]:
                add("A1-P09", "B.1 de %s/%d tras un rebase sin invocación nueva sobre la imagen" % (r["id"], a["seq"]))
            if rebase and b["state"] == "LAUNCHING" and b["target"]["commit"] not in n["anc"] and b["target"]["commit"] not in mp \
                    and b["target"]["commit"] not in n["maps"][n["last_rebase"]]["main_before"]:
                add("A1-P08", "Target de LAUNCHING %s/%d sin imagen en el mapa" % (r["id"], a["seq"]))
            if rebase and (a["state"] != b["state"] or a["reserved_at"] != b["reserved_at"]):
                add("A1-P10", "la reconciliación cambia el intento %s/%d" % (r["id"], a["seq"]))
            if b["reserved_at"] is None and a["reserved_at"] is not None:
                new_reservation(v, a, r, n)
            if a["state"] == "LAUNCHING" and b["state"] != "LAUNCHING" and not open_validity(n):
                add("A1-P13", "LAUNCHING de %s/%d sin vigencia OPEN" % (r["id"], a["seq"]))
            if a["state"] == "RESULT_INGESTED" and b["state"] != "RESULT_INGESTED" and r["loop"] is None and \
                    any(f["issuer"] == "ARCHITECT" and f["state"] == "CLOSED" and pf.get(f["lineage"], {}).get("state") != "CLOSED" for f in n["findings"]):
                add("V14-P20-reviewer", "un resultado de REVIEWER cierra un linaje del ARCHITECT (P-20)")
            if a["state"] == "RESULT_INGESTED" and b["state"] != "RESULT_INGESTED":
                eq = equivalent(a["result"]["evaluated"], r["object"], n)
                closed_now = [f for f in n["findings"] if f["state"] == "CLOSED" and pf.get(f["lineage"], {}).get("state") != "CLOSED"]
                if (a["outcome"] == "VALID" and not eq) or (closed_now and (not eq or a["outcome"] != "VALID")):
                    add("A1-P12", "resultado de %s/%d ingerido sin objeto equivalente" % (r["id"], a["seq"]))
    if rebase and (pl["phase"] != nl["phase"] or p["findings"] != n["findings"] or p["v14"] != n["v14"]
                   or any(pe[i][k] != ne.get(i, pe[i])[k] for i in pe for k in COUNTERS)):
        add("A1-P10", "la reconciliación cambia fase, presupuestos o linajes")
    if n["last_rebase"] != p["last_rebase"]:
        if n["kind"] == "WINDOW_CLOSE":
            wm = n.get("window_maps") or []
            if not wm or n["history"] != p["history"] + wm or n["last_rebase"] != wm[-1]:
                add("A1-P11", "el cierre de la ventana no custodia todos sus mapas en orden")
        elif not rebase or n["history"] != p["history"] + [n["last_rebase"]]:
            add("A1-P11", "rebase_history no recibe exactamente el mapa nuevo")
    elif n["history"] != p["history"]:
        add("A1-P11", "rebase_history cambia sin reconciliación")
    return v


# ------------------------------------------------------------------ literal V14 (the Freeze as written)
def file_v14(s):
    v = []
    al = reserved(s, lambda r: True)
    if s["v14"]["architect_launches"] != al or any(s["v14"][k] > FROZEN[k] for k in COUNTERS):
        v.append(("V14-I-S18", "budgets único de la unidad: incoherente o sobre su tope (P-18)"))
    for r in s["requests"]:
        for a in r["attempts"]:
            for ref in a["refs"]:
                if ref["commit"] not in s["anc"] or s["blobs"].get((ref["commit"], ref["path"])) != ref["blob"]:
                    v.append(("V14-I-S18-ancestro", "%s@%s no es ancestro del punto" % (ref["path"], ref["commit"])))
    return v


def pair_v14(p, n):
    v = []
    pl, nl = p["loop"], n["loop"]
    edge = (pl["phase"], nl["phase"])
    if pl["phase"] != nl["phase"] and edge not in EDGES:
        v.append(("V14-§20.5", "transición %s → %s no existe" % edge))
    if pl["object"] != nl["object"] and edge not in {("CORRECTING", "PUBLISHED"), ("NONE", "REVIEW_PENDING")}:
        v.append(("V14-I-P13-object", "loop.object cambia en %s → %s" % edge))
    prs = {r["id"]: r for r in p["requests"]}
    changed_orch = pl["object"] != nl["object"]
    for r in n["requests"]:
        q = prs.get(r["id"])
        if q is None:
            continue
        if q["object"] != r["object"]:
            v.append(("V14-B.8.8-object", "review_requests[%s].object es inmutable" % r["id"]))
            changed_orch = True
        if any(a != b for a, b in zip(q["attempts"], r["attempts"])):
            changed_orch = True
    if n["kind"] == "REBASE_RECONCILIATION" and changed_orch:
        v.append(("V14-I-P05", "la reconciliación cambia campos fuera de StateFields"))
    if any(n["v14"][k] < p["v14"][k] for k in COUNTERS):
        v.append(("V14-I-P13-contadores", "un contador decrece"))
    return v


def run(trace, validator):
    ff, pp = (file_a1, pair_a1) if validator == "A1" else (file_v14, pair_v14)
    out = []
    for k, point in enumerate(trace):
        out += [(rule, "punto %d (%s): %s" % (k, point["label"], msg)) for rule, msg in ff(point)]
        if k:
            out += [(rule, "par %d→%d (%s): %s" % (k - 1, k, point["label"], msg)) for rule, msg in pp(trace[k - 1], point)]
    return ("VALID" if not out else "INVALID"), sorted({r for r, _ in out}), [m for _, m in out]


# ------------------------------------------------------------------ symbolic world
DOC, DEC, BND, IMPL = "docs/initiatives/U-proposal.md", "docs/automation/decisions/U.md", "docs/automation/evidence/U-agent/bindings/B1.json", "src/impl.cs"
BLOBS = {}
for c, path, blob in [("x1", DOC, "b-X1"), ("x1p", DOC, "b-X1"), ("x1pp", DOC, "b-X1"), ("x2", DOC, "b-X2"), ("x3", DOC, "b-X3"), ("y1", IMPL, "b-Y1"),
                      ("c1", DOC, "b-C"), ("c1p", DOC, "b-C"), ("c1pp", DOC, "b-C"),
                      ("a0", DEC, "b-D"), ("a0p", DEC, "b-D"), ("a0pp", DEC, "b-D"), ("k0", BND, "b-K"), ("k0p", BND, "b-K"), ("k0pp", BND, "b-K"),
                      ("y1p", IMPL, "b-Y1"), ("y2", IMPL, "b-Y2")]:
    BLOBS[(c, path)] = blob
MAPS = {"M1": {"commits": {"c1": "c1p", "a0": "a0p", "k0": "k0p", "x1": "x1p"}, "main_before": {"m0"}},
        "M2": {"commits": {"c1p": "c1pp", "a0p": "a0pp", "k0p": "k0pp", "x1p": "x1pp"}, "main_before": {"m0", "m1"}},
        "M1-sin-c1": {"commits": {"a0": "a0p", "k0": "k0p", "x1": "x1p"}, "main_before": {"m0"}},
        "MY": {"commits": {"y1": "y1p", "a0": "a0p", "k0": "k0p"}, "main_before": {"m0"}}}
DECISIONS = {"A1": {"kind": "RLA", "continues": None, "budget": dict(FROZEN)},
             "A2": {"kind": "RLA", "continues": None, "budget": dict(FROZEN)},
             "A2c": {"kind": "RLA", "continues": "ARL-10", "budget": {"review_rounds": 2, "logical_requests": 2, "architect_launches": 6}},
             "A3up": {"kind": "RLA", "continues": "ARL-10", "budget": dict(FROZEN)},
             "CLOSE-10": {"kind": "CLOSE", "loop": "ARL-10", "revokes": False},
             "CLOSE-10-REV": {"kind": "CLOSE", "loop": "ARL-10", "revokes": True},
             "GC-1": {"kind": "GATE_CONTRACT", "budget": {}},
             "RCLOSE-R1": {"kind": "REVIEWER_CLOSE", "request": "R1"},
             "RCLOSE-R1-REV": {"kind": "REVIEWER_CLOSE", "request": "R1", "revokes": True},
             "GC-2": {"kind": "GATE_CONTRACT", "budget": {}},
             "GC-1b": {"kind": "GATE_CONTRACT", "budget": {}}}
REFS0 = [{"commit": "a0", "path": DEC, "blob": "b-D"}, {"commit": "k0", "path": BND, "blob": "b-K"}]
REFS1 = [{"commit": "a0p", "path": DEC, "blob": "b-D"}, {"commit": "k0p", "path": BND, "blob": "b-K"}]


def obj(c, path=DOC, blob=None):
    return {"commit": c, "path": path, "blob": blob or BLOBS[(c, path)]}


def auth(a, state="OPEN", reason=None, by=None):
    return {"auth": a, "state": state, "reason": reason, "by": by}


def entry(iid, auths, rr, lr, al, closed_at=None, closed_by=None):
    e = {"id": iid, "auths": auths, "review_rounds": rr, "logical_requests": lr, "architect_launches": al, "closed_at": closed_at, "closed_by": closed_by}
    e["caps"] = caps_of({"decisions": DECISIONS}, e)
    return e


def att(seq, state, inv, target, reserved_at, refs=(), result=None, outcome=None, open_findings=(), snapshot=None):
    return {"seq": seq, "state": state, "inv": inv, "target": target, "reserved_at": reserved_at, "refs": copy.deepcopy(list(refs)),
            "result": result, "outcome": outcome, "open_findings": list(open_findings), "snapshot": snapshot}


def req(rid, loop, o, state, attempts, authz=None):
    return {"id": rid, "loop": loop, "object": o, "state": state, "attempts": attempts, "authz": authz}


def lin(lid, state, severity="REQUIRED", issuer="ARCHITECT", request=None):
    return {"lineage": lid, "severity": severity, "issuer": issuer, "state": state, "request": request}


def snap(rr, lr, al):
    return {"review_rounds": rr, "logical_requests": lr, "architect_launches": al}


def st(label, rv, ltype=NONE, phase="NONE", instance=None, o=None, authz=None, validity=None, requests=(), findings=(), entries=(), v14=(0, 0, 0),
       anc=(), kind="ORDINARY", last_rebase=None, history=(), escalation=(NONE, None), blobs=None, rclosures=(), satisfied=(), window_maps=None):
    return {"label": label, "rv": rv, "kind": kind, "last_point": "QU",
            "loop": {"type": ltype, "phase": phase, "instance": instance, "object": o, "authorization": authz},
            "validity": validity, "escalation": {"state": escalation[0], "resolved_by": escalation[1]},
            "requests": copy.deepcopy(list(requests)), "findings": copy.deepcopy(list(findings)), "entries": copy.deepcopy(list(entries)),
            "v14": snap(*v14), "history": list(history), "last_rebase": last_rebase, "anc": set(anc), "blobs": dict(BLOBS if blobs is None else blobs),
            "decisions": DECISIONS, "maps": MAPS, "rclosures": copy.deepcopy(list(rclosures)), "satisfied": list(satisfied),
            "window_maps": list(window_maps) if window_maps else None}


def val(a, state="OPEN", reason=None, by=None):
    return {"auth": a, "state": state, "reason": reason, "by": by}


def mod(s, label, fn):
    t = copy.deepcopy(s)
    t["label"] = label
    fn(t)
    return t


# ------------------------------------------------------------------ traces
def traces():
    T = {}
    ANC = {"x1", "x2", "x3", "y1", "a0", "k0", "c1"}

    def ing(i, loop):
        o = obj("x%d" % i)
        return req("L%d" % i, loop, o, "INGESTED", [att(1, "RESULT_INGESTED", "I%d" % i, o, 10 + i, REFS0, {"evaluated": o}, "VALID")])

    # ---- FC-01 literal (the defect in the Freeze)
    sat_lit = st("ARCHITECT_SATISFIED (V14)", 20, AR, "ARCHITECT_SATISFIED", None, obj("x3"), "A1", val("A1", "ENDED", "ARCHITECT_SATISFIED"),
                 [ing(i, None) for i in (1, 2, 3)], [lin("LIN-1", "CLOSED")], v14=(3, 3, 3), anc=ANC)
    y_lit = req("L4", None, obj("y1", IMPL), "OPEN", [att(1, "BUDGET_RESERVED", "I4", obj("y1", IMPL), 22, REFS0)])
    reopen_lit = st("REVIEW_PENDING sobre Y (A2)", 22, AR, "REVIEW_PENDING", None, obj("y1", IMPL), "A2", val("A2"),
                    [ing(i, None) for i in (1, 2, 3)] + [y_lit], [lin("LIN-1", "CLOSED")], v14=(4, 4, 4), anc=ANC)
    none_lit = st("NONE", 21, findings=[lin("LIN-1", "CLOSED")], requests=[ing(i, None) for i in (1, 2, 3)], v14=(3, 3, 3), anc=ANC)
    T["fc01-literal-reabrir-sobre-Y"] = ("V14", ["FC-01"], "INVALID", {"V14-§20.5", "V14-I-P13-object", "V14-I-S18"}, [sat_lit, reopen_lit])
    T["fc01-literal-cerrar-y-abrir"] = ("V14", ["FC-01"], "INVALID", {"V14-§20.5", "V14-I-P13-object", "V14-I-S18"}, [sat_lit, none_lit, reopen_lit])

    # ---- FC-01 corrected: close and open a new loop; negatives
    e10_sat = entry("ARL-10", [auth("A1", "ENDED", "ARCHITECT_SATISFIED")], 3, 3, 3)
    sat = st("ARCHITECT_SATISFIED de ARL-10", 20, AR, "ARCHITECT_SATISFIED", "ARL-10", obj("x3"), "A1", val("A1", "ENDED", "ARCHITECT_SATISFIED"),
             [ing(i, "ARL-10") for i in (1, 2, 3)], [lin("LIN-1", "CLOSED")], [e10_sat], anc=ANC)
    closed = st("LOOP_CLOSED → NONE", 21, requests=sat["requests"], findings=sat["findings"], entries=[dict(e10_sat, closed_at=21)], anc=ANC)
    y = req("L4", "ARL-22", obj("y1", IMPL), "OPEN", [att(1, "BUDGET_RESERVED", "I4", obj("y1", IMPL), 22, REFS0, snapshot=snap(1, 1, 1))])
    opened = st("apertura de ARL-22 con A2", 22, AR, "REVIEW_PENDING", "ARL-22", obj("y1", IMPL), "A2", val("A2"), sat["requests"] + [y], sat["findings"],
                [dict(e10_sat, closed_at=21), entry("ARL-22", [auth("A2")], 1, 1, 1)], anc=ANC)
    T["fc01-enmendado-cerrar-y-abrir-con-A2"] = ("A1", ["A62-A1-02", "A62-A1-O1"], "VALID", set(), [sat, closed, opened])

    def reuse_fn(t):
        t["loop"]["authorization"] = "A1"
        t["validity"] = val("A1")
        t["entries"][1] = entry("ARL-22", [auth("A1")], 1, 1, 1)
    T["fc01-enmendado-reutiliza-A1"] = ("A1", ["A62-A1-O1"], "INVALID", {"A1-P04"}, [sat, closed, mod(opened, "reabre con A1", reuse_fn)])
    T["fc01-enmendado-cierra-con-solicitud-abierta"] = ("A1", ["A62-A1-02"], "INVALID", {"A1-P07"},
                                                        [mod(sat, "L3 OPEN (sintético)", lambda t: t["requests"][2].update(state="OPEN")), closed])

    def reset_fn(t):
        t["entries"][0].update(review_rounds=0, logical_requests=0)
    T["fc01-enmendado-reinicia-A1"] = ("A1", ["A62-A1-01"], "INVALID", {"A1-P06"}, [sat, closed, mod(opened, "A2 abre y reinicia ARL-10", reset_fn)])

    e10 = entry("ARL-10", [auth("A1")], 1, 1, 1)
    corr = st("CORRECTING (A1 OPEN)", 30, AR, "CORRECTING", "ARL-10", obj("x1"), "A1", val("A1"), [ing(1, "ARL-10")], [lin("LIN-1", "OPEN")], [e10], anc=ANC)
    T["fc01-enmendado-object-null-fuera-del-cierre"] = ("A1", ["A62-A1-O5"], "INVALID", {"A1-P02"},
                                                        [corr, mod(corr, "PUBLISHED con loop.object = null", lambda t: (t["loop"].update(phase="PUBLISHED", object=None), t.update(rv=31)))])

    # ---- A62-A1-01: replacement inside an open loop
    e10_rep = entry("ARL-10", [auth("A1", "ENDED", "SUPERSEDED", "A2c"), auth("A2c")], 1, 1, 1)
    rep = st("A2c sustituye a A1 (continúa ARL-10)", 31, AR, "CORRECTING", "ARL-10", obj("x1"), "A2c", val("A2c"), [ing(1, "ARL-10")], [lin("LIN-1", "OPEN")],
             [e10_rep], anc=ANC)

    def rereview(prev, rv0, auth_id, entry_after, snapshot):
        pub = mod(prev, "PUBLISHED x2", lambda t: (t["loop"].update(phase="PUBLISHED", object=obj("x2")), t.update(rv=rv0)))
        civ = mod(pub, "CI_VERIFIED", lambda t: (t["loop"].update(phase="CI_VERIFIED"), t.update(rv=rv0 + 1)))
        l2 = req("L2", "ARL-10", obj("x2"), "OPEN", [att(1, "BUDGET_RESERVED", "I2", obj("x2"), rv0 + 2, REFS0, open_findings=["LIN-1"], snapshot=snapshot)])
        rr = mod(civ, "REREVIEW_PENDING: reserva en la misma entrada", lambda t: (t["loop"].update(phase="REREVIEW_PENDING"), t.update(rv=rv0 + 2),
                                                                                    t["requests"].append(l2), t.update(entries=[entry_after])))
        return [pub, civ, rr]
    e10_rep2 = entry("ARL-10", [auth("A1", "ENDED", "SUPERSEDED", "A2c"), auth("A2c")], 2, 2, 2)
    T["a62-a1-01-sustitucion-continua-la-misma-entrada"] = ("A1", ["A62-A1-01", "A62-A1-O2"], "VALID", set(),
                                                            [corr, rep] + rereview(rep, 32, "A2c", e10_rep2, snap(2, 2, 2)))
    T["a62-a1-o2-snapshot-por-autorizacion"] = ("A1", ["A62-A1-O2"], "INVALID", {"A1-P17"},
                                                [corr, rep] + rereview(rep, 32, "A2c", e10_rep2, snap(1, 1, 1)))

    def new_entry_fn(t):
        t["entries"] = [entry("ARL-10", [auth("A1", "ENDED", "SUPERSEDED", "A2c")], 1, 1, 1), entry("ARL-31", [auth("A2c")], 0, 0, 0)]
    T["a62-a1-01-sustitucion-crea-entrada-nueva"] = ("A1", ["A62-A1-01"], "INVALID", {"A1-F03", "A1-F04", "A1-P04", "A1-P05"},
                                                     [corr, mod(rep, "A2c abre otra entrada", new_entry_fn)])
    T["a62-a1-01-sustitucion-reinicia-contadores"] = ("A1", ["A62-A1-01"], "INVALID", {"A1-P05", "A1-P06"},
                                                      [corr, mod(rep, "A2c reinicia contadores", lambda t: t["entries"][0].update(review_rounds=0, logical_requests=0))])

    def up_fn(t):
        t.update(rv=32)
        t["loop"]["authorization"] = "A3up"
        t["validity"] = val("A3up")
        e = entry("ARL-10", [auth("A1", "ENDED", "SUPERSEDED", "A2c"), auth("A2c", "ENDED", "SUPERSEDED", "A3up"), auth("A3up")], 1, 1, 1)
        e["caps"] = dict(FROZEN)
        t["entries"] = [e]
    T["a62-a1-01-sustitucion-sube-topes"] = ("A1", ["A62-A1-01"], "INVALID", {"A1-F02", "A1-P06"}, [corr, rep, mod(rep, "A3up sube los topes", up_fn)])

    # ---- A62-A1-02: EXPIRED / REVOKED
    e10_exp = entry("ARL-10", [auth("A1", "ENDED", "EXPIRED")], 1, 1, 1)
    exp = st("CORRECTING con A1 EXPIRED", 40, AR, "CORRECTING", "ARL-10", obj("x1"), "A1", val("A1", "ENDED", "EXPIRED"), [ing(1, "ARL-10")],
             [lin("LIN-1", "OPEN")], [e10_exp], anc=ANC, escalation=("COORDINATOR", "CLOSE-10"))
    closed_exp = st("LOOP_CLOSED por CLOSE-10", 41, requests=exp["requests"], findings=exp["findings"],
                    entries=[dict(e10_exp, closed_at=41, closed_by="CLOSE-10")], anc=ANC)
    T["a62-a1-02-expirada-y-cerrada"] = ("A1", ["A62-A1-02", "A62-A1-O3", "A62-A1-O4"], "VALID", set(), [exp, closed_exp])
    e10_rev = entry("ARL-10", [auth("A1", "ENDED", "REVOKED", "REV-A1")], 1, 1, 1)
    rev = mod(exp, "CORRECTING con A1 REVOKED", lambda t: (t.update(validity=val("A1", "ENDED", "REVOKED", "REV-A1"), entries=[copy.deepcopy(e10_rev)])))
    T["a62-a1-02-revocada-y-cerrada"] = ("A1", ["A62-A1-02", "A62-A1-O4"], "VALID", set(),
                                         [rev, mod(closed_exp, "LOOP_CLOSED tras REVOKED", lambda t: t.update(entries=[dict(e10_rev, closed_at=41, closed_by="CLOSE-10")]))])
    open_close_p = mod(exp, "CORRECTING con A1 OPEN", lambda t: t.update(validity=val("A1"), entries=[copy.deepcopy(e10)], escalation={"state": NONE, "resolved_by": None}))
    open_close_n = mod(closed_exp, "LOOP_CLOSED que revoca A1", lambda t: t.update(entries=[dict(entry("ARL-10", [auth("A1", "ENDED", "REVOKED", "CLOSE-10-REV")], 1, 1, 1),
                                                                                                 closed_at=41, closed_by="CLOSE-10-REV")]))
    T["a62-a1-02-cierre-que-revoca-la-vigente"] = ("A1", ["A62-A1-02", "A62-A1-O4"], "VALID", set(), [open_close_p, open_close_n])
    T["a62-a1-02-cierre-sin-decision"] = ("A1", ["A62-A1-02"], "INVALID", {"A1-P07"},
                                          [exp, mod(closed_exp, "LOOP_CLOSED sin decisión", lambda t: t["entries"][0].update(closed_by=None))])
    T["a62-a1-02-cierre-con-escalada-sin-resolver"] = ("A1", ["A62-A1-02"], "INVALID", {"A1-P07"},
                                                       [mod(exp, "escalada al Owner sin resolver", lambda t: t.update(escalation={"state": "OWNER", "resolved_by": None})), closed_exp])
    exp_c = mod(exp, "CORRECTING con A1 EXPIRED (sin escalada)",lambda t: t.update(escalation={"state": NONE, "resolved_by": None}))
    e10_cont = entry("ARL-10", [auth("A1", "ENDED", "EXPIRED"), auth("A2c")], 1, 1, 1)
    cont = mod(exp_c, "A2c continúa ARL-10 tras EXPIRED", lambda t: (t.update(rv=41, validity=val("A2c"), entries=[copy.deepcopy(e10_cont)]),
                                                                     t["loop"].update(authorization="A2c")))
    T["a62-a1-02-expirada-y-continuada"] = ("A1", ["A62-A1-02"], "VALID", set(),
                                            [exp_c, cont] + rereview(cont, 42, "A2c", entry("ARL-10", [auth("A1", "ENDED", "EXPIRED"), auth("A2c")], 2, 2, 2), snap(2, 2, 2)))
    T["a62-a1-02-reescribe-EXPIRED-como-SUPERSEDED"] = ("A1", ["A62-A1-02"], "INVALID", {"A1-P05", "A1-P06"},
                                                        [exp_c, mod(cont, "EXPIRED reescrito", lambda t: t["entries"][0]["auths"][0].update(reason="SUPERSEDED", by="A2c"))])
    T["a62-a1-02-continuacion-crea-entrada"] = ("A1", ["A62-A1-02"], "INVALID", {"A1-F03", "A1-F04", "A1-P04", "A1-P05"},
                                                [exp_c, mod(cont, "A2c abre otra entrada", lambda t: t.update(entries=[copy.deepcopy(e10_exp), entry("ARL-41", [auth("A2c")], 0, 0, 0)]))])
    civ_exp = mod(exp_c, "CI_VERIFIED con A1 EXPIRED", lambda t: (t.update(rv=43), t["loop"].update(phase="CI_VERIFIED", object=obj("x2"))))
    l2x = req("L2", "ARL-10", obj("x2"), "OPEN", [att(1, "BUDGET_RESERVED", "I2", obj("x2"), 44, REFS0, open_findings=["LIN-1"], snapshot=snap(2, 2, 2))])
    act = mod(civ_exp, "reserva bajo A1 EXPIRED", lambda t: (t.update(rv=44, entries=[entry("ARL-10", [auth("A1", "ENDED", "EXPIRED")], 2, 2, 2)]),
                                                            t["loop"].update(phase="REREVIEW_PENDING"), t["requests"].append(l2x)))
    T["a62-a1-02-accion-nueva-con-vigencia-terminada"] = ("A1", ["A62-A1-02"], "INVALID", {"A1-P13", "V14-S18-vigencia"}, [civ_exp, act])

    # ---- A62-A1-03: loop.type scope
    r1 = req("R1", None, obj("y1", IMPL), "OPEN", [att(1, "BUDGET_RESERVED", "IR1", obj("y1", IMPL), 51, REFS0)], authz="GC-1")
    rev_none = st("NONE", 50, anc=ANC)
    rev_open = st("bucle REVIEWER autorizado por el contrato de gate GC-1", 51, RV, "REVIEW_PENDING", None, obj("y1", IMPL), "GC-1", val("GC-1"), [r1], v14=(1, 1, 1), anc=ANC)
    T["a62-a1-03-reviewer-autorizado-por-contrato-de-gate"] = ("A1", ["A62-A1-03", "OBS-A1-01", "OBS-A1-01/R6"], "VALID", set(), [rev_none, rev_open])
    T["a62-a1-03-reviewer-con-identidad-de-architect"] = ("A1", ["A62-A1-03", "OBS-A1-01", "OBS-A1-01/R5"], "INVALID", {"A1-F06"},
                                                          [rev_none, mod(rev_open, "REVIEWER con instance_id", lambda t: t["loop"].update(instance="ARL-51"))])
    l9 = req("L9", "ARL-51", obj("y1", IMPL), "OPEN", [att(1, "BUDGET_RESERVED", "I9", obj("y1", IMPL), 51, REFS0, snapshot=snap(1, 1, 1))])
    T["a62-a1-03-architect-sin-entrada"] = ("A1", ["A62-A1-03"], "INVALID", {"A1-F03", "A1-F05", "A1-P04"},
                                            [rev_none, st("ARCHITECT_REVIEW sin entrada", 51, AR, "REVIEW_PENDING", "ARL-51", obj("y1", IMPL), "A2", val("A2"), [l9], anc=ANC)])

    # ---- OBS-A1-01: REVIEWER completion and closure (R1..R10 of the Coordinator disposition, decisions §39)
    Y = obj("y1", IMPL)

    def rq(state, attempt_state, outcome=None, result=True):
        return req("R1", None, Y, state, [att(1, attempt_state, "IR1", Y, 51, REFS0, {"evaluated": Y} if result else None, outcome)], authz="GC-1")

    def rv_state(label, rv, phase, request, validity, findings=(), escalation=(NONE, None), v14=(1, 1, 1), rclosures=()):
        return st(label, rv, RV, phase, None, Y, "GC-1", validity, [request], findings, v14=v14, anc=ANC, escalation=escalation, rclosures=rclosures)

    def rv_closed(label, rv, request, findings, record, v14=(1, 1, 1), previous=()):
        return st(label, rv, requests=[request], findings=findings, v14=v14, anc=ANC, rclosures=list(previous) + [record])

    def rec(validity, closed_at, closed_by=None):
        return {"last_request": "R1", "authorization": "GC-1", "validity": validity, "closed_at": closed_at, "closed_by": closed_by}
    VS = val("GC-1", "ENDED", "REVIEWER_SATISFIED")
    rv_launch = rv_state("REVIEWER: R1/1 en LAUNCHING", 52, "ARCHITECT_INVOKED", rq("OPEN", "LAUNCHING", result=False), val("GC-1"))
    rv_recv = rv_state("REVIEWER: resultado recibido", 53, "ARCHITECT_INVOKED", rq("OPEN", "RESULT_RECEIVED"), val("GC-1"))

    def satisfied(findings=()):
        return rv_state("REVIEWER_SATISFIED (resultado VALID ingerido)", 54, "REVIEWER_SATISFIED", rq("INGESTED", "RESULT_INGESTED", "VALID"), VS, findings)
    sat_nf = satisfied()
    closed_nf = rv_closed("LOOP_CLOSED de REVIEWER", 55, rq("INGESTED", "RESULT_INGESTED", "VALID"), [], rec(VS, 55))
    T["a62-a1-obs01-r1-reviewer-NO_FINDINGS-se-cierra"] = ("A1", ["OBS-A1-01", "OBS-A1-01/R1"], "VALID", set(), [rev_none, rev_open, rv_launch, rv_recv, sat_nf, closed_nf])
    adv = [lin("LIN-R1", "OPEN", "ADVISORY", "REVIEWER", "R1")]
    T["a62-a1-obs01-r2-solo-ADVISORY-se-cierra"] = ("A1", ["OBS-A1-01", "OBS-A1-01/R2"], "VALID", set(),
                                                   [rv_recv, satisfied(adv), rv_closed("LOOP_CLOSED con un ADVISORY abierto", 55, rq("INGESTED", "RESULT_INGESTED", "VALID"), adv, rec(VS, 55))])
    blk = [lin("LIN-R1", "OPEN", "BLOCKING", "REVIEWER", "R1")]
    T["a62-a1-obs01-r3-BLOCKING-abierto-impide-REVIEWER_SATISFIED"] = ("A1", ["OBS-A1-01", "OBS-A1-01/R3"], "INVALID", {"A1-R02"}, [rv_recv, satisfied(blk)])
    T["a62-a1-obs01-r3-BLOCKING-abierto-impide-el-cierre"] = ("A1", ["OBS-A1-01", "OBS-A1-01/R3"], "INVALID", {"A1-R03"},
                                                             [satisfied(blk), rv_closed("LOOP_CLOSED con un BLOCKING abierto", 55, rq("INGESTED", "RESULT_INGESTED", "VALID"), blk, rec(VS, 55))])
    arch_lin = [lin("LIN-1", "OPEN")]
    T["a62-a1-obs01-r4-reviewer-cierra-un-linaje-del-architect"] = ("A1", ["OBS-A1-01", "OBS-A1-01/R4"], "INVALID", {"V14-P20-reviewer"},
                                                                   [rv_state("REVIEWER con LIN-1 del ARCHITECT abierto", 53, "ARCHITECT_INVOKED", rq("OPEN", "RESULT_RECEIVED"), val("GC-1"), arch_lin),
                                                                    satisfied([lin("LIN-1", "CLOSED")])])
    T["a62-a1-obs01-r5-reviewer-con-architect_budgets"] = ("A1", ["OBS-A1-01", "OBS-A1-01/R5"], "INVALID", {"A1-F04", "A1-P04"},
                                                          [rev_none, mod(rev_open, "REVIEWER con una entrada de architect_budgets", lambda t: t.update(entries=[entry("ARL-51", [auth("A2")], 0, 0, 0)]))])
    T["a62-a1-obs01-r1-NO_FINDINGS-no-es-ARCHITECT_SATISFIED"] = ("A1", ["OBS-A1-01", "OBS-A1-01/R1"], "INVALID", {"A1-R01"},
                                                                 [rv_recv, mod(sat_nf, "REVIEWER con ARCHITECT_SATISFIED", lambda t: (t["loop"].update(phase="ARCHITECT_SATISFIED"),
                                                                                                                                    t.update(validity=val("GC-1", "ENDED", "ARCHITECT_SATISFIED"))))])
    for reason in ("EXPIRED", "REVOKED"):
        vr = val("GC-1", "ENDED", reason, "REV-GC-1" if reason == "REVOKED" else None)
        p7 = rv_state("REVIEWER %s con un BLOCKING abierto, escalada resuelta" % reason, 60, "CORRECTING", rq("INGESTED", "RESULT_INGESTED", "VALID"), vr, blk,
                      escalation=("COORDINATOR", "RCLOSE-R1"))
        n7 = rv_closed("LOOP_CLOSED por RCLOSE-R1; %s se conserva" % reason, 61, rq("INGESTED", "RESULT_INGESTED", "VALID"), blk, rec(vr, 61, "RCLOSE-R1"))
        T["a62-a1-obs01-r7-%s-se-cierra-conservando-el-motivo" % reason.lower()] = ("A1", ["OBS-A1-01", "OBS-A1-01/R7"], "VALID", set(), [p7, n7])
        if reason == "EXPIRED":
            T["a62-a1-obs01-r7-EXPIRED-reescrito-como-SUPERSEDED"] = ("A1", ["OBS-A1-01", "OBS-A1-01/R7"], "INVALID", {"A1-R03"},
                                                                     [p7, mod(n7, "registro con SUPERSEDED", lambda t: t["rclosures"][-1]["validity"].update(reason="SUPERSEDED"))])
            T["a62-a1-obs01-r7-cierre-sin-decision"] = ("A1", ["OBS-A1-01", "OBS-A1-01/R7"], "INVALID", {"A1-R03"},
                                                       [p7, mod(n7, "sin RCLOSE-R1", lambda t: t["rclosures"][-1].update(closed_by=None))])
    T["a62-a1-obs01-r8-cierre-reinicia-budgets"] = ("A1", ["OBS-A1-01", "OBS-A1-01/R8"], "INVALID", {"A1-P06", "A1-R03"},
                                                   [sat_nf, mod(closed_nf, "cierre que reinicia budgets", lambda t: t.update(v14=snap(0, 0, 1)))])
    old_rec = rec(VS, 45)
    T["a62-a1-obs01-r8-cierre-borra-historia"] = ("A1", ["OBS-A1-01", "OBS-A1-01/R8"], "INVALID", {"A1-R04"},
                                                 [mod(sat_nf, "REVIEWER_SATISFIED con un cierre anterior", lambda t: t.update(rclosures=[old_rec])), closed_nf])
    l7 = req("L7", "ARL-56", obj("x1"), "OPEN", [att(1, "BUDGET_RESERVED", "I7", obj("x1"), 56, REFS0, snapshot=snap(1, 1, 1))])
    arch_after = mod(closed_nf, "apertura de ARL-56 tras el cierre del REVIEWER", lambda t: (
        t.update(rv=56, validity=val("A2"), entries=[entry("ARL-56", [auth("A2")], 1, 1, 1)]),
        t["loop"].update(type=AR, phase="REVIEW_PENDING", instance="ARL-56", object=obj("x1"), authorization="A2"), t["requests"].append(l7)))
    T["a62-a1-obs01-r9-architect-abre-tras-el-cierre-del-reviewer"] = ("A1", ["OBS-A1-01", "OBS-A1-01/R9"], "VALID", set(), [sat_nf, closed_nf, arch_after])
    T["a62-a1-obs01-r9-sin-cierre-no-se-abre-otro-bucle"] = ("A1", ["OBS-A1-01", "OBS-A1-01/R9"], "INVALID", {"A1-P01", "A1-P02", "A1-P04"},
                                                            [sat_nf, mod(arch_after, "apertura sin cerrar el REVIEWER", lambda t: t.update(rclosures=[]))])
    # ---- second formal review (decisions §41): A62-A1R-01..03 and the optional A62-A1R-O2..O5
    vx = val("GC-1", "ENDED", "EXHAUSTED")
    p1 = rv_state("REVIEWER agotado (EXHAUSTED) con un BLOCKING abierto, escalada resuelta", 60, "CORRECTING", rq("INGESTED", "RESULT_INGESTED", "VALID"), vx, blk,
                  escalation=("COORDINATOR", "RCLOSE-R1"))
    n1 = rv_closed("LOOP_CLOSED (E) tras EXHAUSTED por RCLOSE-R1", 61, rq("INGESTED", "RESULT_INGESTED", "VALID"), blk, rec(vx, 61, "RCLOSE-R1"))
    T["a62-a1r-01-exhausted-se-cierra-conservando-el-motivo"] = ("A1", ["A62-A1R-01", "OBS-A1-01"], "VALID", set(), [p1, n1])
    T["a62-a1r-01-exhausted-sin-decision"] = ("A1", ["A62-A1R-01"], "INVALID", {"A1-R03"}, [p1, mod(n1, "sin RCLOSE-R1", lambda t: t["rclosures"][-1].update(closed_by=None))])
    live = req("R2", None, Y, "OPEN", [att(1, "LAUNCHED", "IR2", Y, 59, REFS0)], authz="GC-1")
    T["a62-a1r-01-exhausted-con-intento-vivo"] = ("A1", ["A62-A1R-01"], "INVALID", {"A1-R03"},
                                                  [mod(p1, "EXHAUSTED con R2/1 LAUNCHED", lambda t: (t["requests"].append(copy.deepcopy(live)), t.update(v14=snap(2, 2, 2)))),
                                                   mod(n1, "cierre con R2/1 vivo", lambda t: (t["requests"].append(copy.deepcopy(live)), t.update(v14=snap(2, 2, 2)),
                                                                                              t["rclosures"][-1].update(last_request="R2")))])
    T["a62-a1r-01-registro-finge-REVIEWER_SATISFIED"] = ("A1", ["A62-A1R-01", "A62-A1R-O5"], "INVALID", {"A1-R03"},
                                                         [p1, mod(n1, "registro con REVIEWER_SATISFIED", lambda t: t["rclosures"][-1]["validity"].update(reason="REVIEWER_SATISFIED"))])
    Yp = obj("y1p", IMPL)
    rvb = st("REVIEWER antes del rebase (R1/1 BUDGET_RESERVED)", 80, RV, "REVIEW_PENDING", None, Y, "GC-1", val("GC-1"),
             [req("R1", None, Y, "OPEN", [att(1, "BUDGET_RESERVED", "IR1", Y, 79, REFS0)], authz="GC-1")], v14=(1, 1, 1), anc={"y1", "a0", "k0"})
    rva = st("QU REBASE_RECONCILIATION con bucle REVIEWER", 81, RV, "REVIEW_PENDING", None, Yp, "GC-1", val("GC-1"),
             [req("R1", None, Yp, "OPEN", [att(1, "BUDGET_RESERVED", "IR1b", Yp, 79, REFS1)], authz="GC-1")], v14=(1, 1, 1), anc={"y1p", "a0p", "k0p"},
             kind="REBASE_RECONCILIATION", last_rebase="MY", history=["MY"])
    T["a62-a1r-02-reviewer-rebase-reconcilia"] = ("A1", ["A62-A1R-02", "A62-A1R-O1"], "VALID", set(), [rvb, rva])
    T["a62-a1r-02-reviewer-rebase-loop-object-sin-reconciliar"] = ("A1", ["A62-A1R-02"], "INVALID", {"I-H02"},
                                                                  [rvb, mod(rva, "loop.object del REVIEWER sin reconciliar", lambda t: t["loop"].update(object=Y))])
    rvl = st("REVIEWER con R1/1 LAUNCHED", 82, RV, "ARCHITECT_INVOKED", None, Y, "GC-1", val("GC-1"),
             [req("R1", None, Y, "OPEN", [att(1, "LAUNCHED", "IR1", Y, 79, REFS0)], authz="GC-1")], v14=(1, 1, 1), anc={"y1", "a0", "k0"})
    rvl2 = mod(rvl, "QU REBASE_RECONCILIATION (LAUNCHED conserva su Target)", lambda t: (t.update(rv=83, kind="REBASE_RECONCILIATION", last_rebase="MY", history=["MY"],
                                                                                          anc={"y1p", "a0p", "k0p"}), t["loop"].update(object=Yp), t["requests"][0].update(object=Yp)))
    rvl3 = mod(rvl2, "RESULT_RECEIVED", lambda t: (t.update(rv=84, kind="ORDINARY"), t["requests"][0]["attempts"][0].update(state="RESULT_RECEIVED", result={"evaluated": Y})))
    rvl4 = mod(rvl3, "REVIEWER_SATISFIED: resultado del commit original ingerido frente a la imagen", lambda t: (
        t.update(rv=85, validity=val("GC-1", "ENDED", "REVIEWER_SATISFIED")), t["loop"].update(phase="REVIEWER_SATISFIED"), t["requests"][0].update(state="INGESTED"),
        t["requests"][0]["attempts"][0].update(state="RESULT_INGESTED", outcome="VALID")))
    T["a62-a1r-02-reviewer-ingesta-tras-rebase"] = ("A1", ["A62-A1R-02"], "VALID", set(), [rvl, rvl2, rvl3, rvl4])
    exo = st("bucle EXECUTION con loop.object no nulo", 86, "EXECUTION", "WINDOW_OPEN", None, obj("c1"), "GC-EXEC", val("GC-EXEC"), anc={"c1"})
    exr = mod(exo, "QU REBASE_RECONCILIATION con bucle EXECUTION", lambda t: (t.update(rv=87, kind="REBASE_RECONCILIATION", last_rebase="M1", history=["M1"], anc={"c1p"}),
                                                                             t["loop"].update(object=obj("c1p"))))
    T["a62-a1r-02-execution-rebase-reconcilia"] = ("A1", ["A62-A1R-02", "A62-A1R-O1"], "VALID", set(), [exo, exr])
    T["a62-a1r-02-execution-rebase-sin-reconciliar"] = ("A1", ["A62-A1R-02"], "INVALID", {"I-H02"},
                                                        [exo, mod(exr, "loop.object de EXECUTION sin reconciliar", lambda t: t["loop"].update(object=obj("c1")))])
    r2new = req("R2", None, Y, "OPEN", [att(1, "BUDGET_RESERVED", "IR2", Y, 56, REFS0)], authz="GC-1")
    reopen_s = mod(closed_nf, "bucle REVIEWER reabierto con GC-1 (terminada por REVIEWER_SATISFIED)", lambda t: (
        t.update(rv=56, validity=val("GC-1"), v14=snap(2, 2, 2)), t["loop"].update(type=RV, phase="REVIEW_PENDING", object=Y, authorization="GC-1"),
        t["requests"].append(copy.deepcopy(r2new))))
    T["a62-a1r-03-reapertura-tras-S"] = ("A1", ["A62-A1R-03"], "INVALID", {"A1-R05"}, [sat_nf, closed_nf, reopen_s])
    vrev = val("GC-1", "ENDED", "REVOKED", "REV-GC-1")
    pe = rv_state("REVIEWER REVOKED con un BLOCKING abierto, escalada resuelta", 60, "CORRECTING", rq("INGESTED", "RESULT_INGESTED", "VALID"), vrev, blk,
                  escalation=("COORDINATOR", "RCLOSE-R1"))
    ne = rv_closed("LOOP_CLOSED (E) tras REVOKED", 61, rq("INGESTED", "RESULT_INGESTED", "VALID"), blk, rec(vrev, 61, "RCLOSE-R1"))
    r2b = req("R2", None, Y, "OPEN", [att(1, "BUDGET_RESERVED", "IR2", Y, 62, REFS0, open_findings=["LIN-R1"])], authz="GC-1")
    reopen_e = mod(ne, "bucle REVIEWER reabierto con GC-1 revocada", lambda t: (
        t.update(rv=62, validity=val("GC-1"), v14=snap(2, 2, 2)), t["loop"].update(type=RV, phase="REVIEW_PENDING", object=Y, authorization="GC-1"),
        t["requests"].append(copy.deepcopy(r2b))))
    T["a62-a1r-03-reapertura-tras-E-revocada"] = ("A1", ["A62-A1R-03"], "INVALID", {"A1-R05"}, [pe, ne, reopen_e])
    ne_rb = mod(ne, "QU REBASE_RECONCILIATION con el bucle cerrado", lambda t: t.update(rv=62, kind="REBASE_RECONCILIATION", last_rebase="MY", history=["MY"],
                                                                                       anc={"y1p", "a0p", "k0p"}, blobs=dict(BLOBS)))
    reopen_rb = mod(ne_rb, "bucle REVIEWER reabierto con GC-1 tras el rebase", lambda t: (
        t.update(rv=63, kind="ORDINARY", validity=val("GC-1"), v14=snap(2, 2, 2)), t["loop"].update(type=RV, phase="REVIEW_PENDING", object=Yp, authorization="GC-1"),
        t["requests"].append(req("R2", None, Yp, "OPEN", [att(1, "BUDGET_RESERVED", "IR2", Yp, 63, REFS1, open_findings=["LIN-R1"])], authz="GC-1"))))
    T["a62-a1r-03-reapertura-tras-rebase"] = ("A1", ["A62-A1R-03"], "INVALID", {"A1-R05"}, [pe, ne, ne_rb, reopen_rb])
    r2c = req("R2", None, Y, "OPEN", [att(1, "BUDGET_RESERVED", "IR2", Y, 62, REFS0, open_findings=["LIN-R1"])], authz="GC-2")
    cont2 = mod(ne, "bucle REVIEWER nuevo con el contrato reemitido GC-2, hereda LIN-R1", lambda t: (
        t.update(rv=62, validity=val("GC-2"), v14=snap(2, 2, 2)), t["loop"].update(type=RV, phase="REVIEW_PENDING", object=Y, authorization="GC-2"),
        t["requests"].append(copy.deepcopy(r2c))))
    T["a62-a1r-03-nueva-autoridad-hereda-BLOCKING"] = ("A1", ["A62-A1R-03", "A62-A1R-O3"], "VALID", set(), [pe, ne, cont2])

    # ---- A62-A1A-01 (author correction, night order §3.A; found by the F4 combined sequences): an inherited BLOCKING blocks REVIEWER_SATISFIED
    def r2step(t, rv, phase, astate, result=None, outcome=None, rstate="OPEN", findings=None, validity=None):
        t.update(rv=rv)
        t["loop"].update(phase=phase)
        t["requests"][-1].update(state=rstate)
        t["requests"][-1]["attempts"][0].update(state=astate, result=result, outcome=outcome)
        if findings is not None:
            t.update(findings=copy.deepcopy(findings))
        if validity is not None:
            t.update(validity=validity)
    g2 = mod(cont2, "R2/1 LAUNCHING (GC-2)", lambda t: r2step(t, 63, "ARCHITECT_INVOKED", "LAUNCHING"))
    g3 = mod(g2, "R2/1 LAUNCHED", lambda t: r2step(t, 64, "ARCHITECT_INVOKED", "LAUNCHED"))
    g4 = mod(g3, "R2/1 RESULT_RECEIVED", lambda t: r2step(t, 65, "ARCHITECT_INVOKED", "RESULT_RECEIVED", {"evaluated": Y}))
    blk_c = [lin("LIN-R1", "CLOSED", "BLOCKING", "REVIEWER", "R1")]
    g5 = mod(g4, "REVIEWER_SATISFIED de GC-2: el LIN-R1 heredado lo cierra su emisor", lambda t: r2step(
        t, 66, "REVIEWER_SATISFIED", "RESULT_INGESTED", {"evaluated": Y}, "VALID", "INGESTED", blk_c, val("GC-2", "ENDED", "REVIEWER_SATISFIED")))
    T["a62-a1a-01-bucle-nuevo-cierra-el-BLOCKING-heredado-y-se-satisface"] = ("A1", ["A62-A1A-01"], "VALID", set(), [pe, ne, cont2, g2, g3, g4, g5])
    T["a62-a1a-01-satisfecho-con-el-BLOCKING-heredado-abierto"] = ("A1", ["A62-A1A-01"], "INVALID", {"A1-R02"},
                                                                   [g4, mod(g5, "REVIEWER_SATISFIED con el LIN-R1 heredado abierto", lambda t: t.update(findings=copy.deepcopy(blk)))])

    # ---- A62-A1S-01..02 (formal review R20261005T063359Z-86e3)
    T["a62-a1s-01-continuacion-satisfecha-con-STILL_OPEN-heredado"] = ("A1", ["A62-A1S-01"], "INVALID", {"A1-R02"},
        [g4, mod(g5, "REVIEWER_SATISFIED con LIN-R1 heredado en STILL_OPEN", lambda t: t.update(findings=[lin("LIN-R1", "STILL_OPEN", "BLOCKING", "REVIEWER", "R1")]))])
    r1live = req("R1", None, Y, "OPEN", [att(1, "LAUNCHED", "IR1", Y, 51, REFS0)], authz="GC-1")
    sub_p = st("REVIEWER GC-1 con R1/1 LAUNCHED", 52, RV, "ARCHITECT_INVOKED", None, Y, "GC-1", val("GC-1"), [r1live], v14=(1, 1, 1), anc=ANC)
    sub_n = mod(sub_p, "GC-1b sustituye a GC-1 dentro del bucle (decisión custodiada); R1 sigue LAUNCHED",
                lambda t: (t.update(rv=53, validity=val("GC-1b")), t["loop"].update(authorization="GC-1b")))
    r2s = req("R2", None, Y, "OPEN", [att(1, "BUDGET_RESERVED", "IR2s", Y, 54, REFS0)], authz="GC-1b")
    sub_m1 = mod(sub_n, "R2/1 BUDGET_RESERVED bajo GC-1b", lambda t: (t.update(rv=54, v14=snap(2, 2, 2)), t["requests"].append(copy.deepcopy(r2s))))
    sub_m2 = mod(sub_m1, "R2/1 LAUNCHING", lambda t: (t.update(rv=55), t["requests"][-1]["attempts"][0].update(state="LAUNCHING")))
    sub_m3 = mod(sub_m2, "R2/1 RESULT_RECEIVED", lambda t: (t.update(rv=56), t["requests"][-1]["attempts"][0].update(state="RESULT_RECEIVED", result={"evaluated": Y})))
    sub_sat = mod(sub_m3, "REVIEWER_SATISFIED de GC-1b con R1 de GC-1 todavía LAUNCHED", lambda t: (
        t.update(rv=57, validity=val("GC-1b", "ENDED", "REVIEWER_SATISFIED")), t["loop"].update(phase="REVIEWER_SATISFIED"),
        t["requests"][-1].update(state="INGESTED"), t["requests"][-1]["attempts"][0].update(state="RESULT_INGESTED", outcome="VALID")))
    T["a62-a1s-01-sustitucion-con-intento-vivo-de-la-autoridad-sustituida"] = ("A1", ["A62-A1S-01"], "INVALID", {"A1-R02"},
                                                                                [sub_n, sub_m1, sub_m2, sub_m3, sub_sat])
    Y2 = obj("y2", IMPL)
    blk1 = [lin("LIN-R1", "OPEN", "BLOCKING", "REVIEWER", "R1")]
    r1f = req("R1", None, Y, "INGESTED", [att(1, "RESULT_INGESTED", "IR1", Y, 51, REFS0, {"evaluated": Y}, "VALID")], authz="GC-1")
    cor0 = st("REVIEWER GC-1: R1 FINDINGS con LIN-R1 BLOCKING → CORRECTING", 60, RV, "CORRECTING", None, Y, "GC-1", val("GC-1"), [r1f], blk1,
              v14=(1, 1, 1), anc=ANC | {"y2"})
    cor1 = mod(cor0, "PUBLISHED: loop.object = Y2 (corrección)", lambda t: (t.update(rv=61), t["loop"].update(phase="PUBLISHED", object=Y2)))
    cor2 = mod(cor1, "CI_VERIFIED", lambda t: (t.update(rv=62), t["loop"].update(phase="CI_VERIFIED")))
    r2c2 = req("R2", None, Y2, "OPEN", [att(1, "BUDGET_RESERVED", "IR2c", Y2, 63, REFS0, open_findings=["LIN-R1"])], authz="GC-1")
    cor3 = mod(cor2, "REREVIEW_PENDING: R2 sobre Y2", lambda t: (t.update(rv=63, v14=snap(2, 2, 2)), t["loop"].update(phase="REREVIEW_PENDING"),
                                                             t["requests"].append(copy.deepcopy(r2c2))))

    def r2c_step(t, rv, phase, astate, result=None, outcome=None, rstate="OPEN"):
        t.update(rv=rv)
        t["loop"].update(phase=phase)
        t["requests"][-1].update(state=rstate)
        t["requests"][-1]["attempts"][0].update(state=astate, result=result, outcome=outcome)
    cor4 = mod(cor3, "R2/1 LAUNCHING", lambda t: r2c_step(t, 64, "ARCHITECT_INVOKED", "LAUNCHING"))
    cor5 = mod(cor4, "R2/1 RESULT_RECEIVED", lambda t: r2c_step(t, 65, "ARCHITECT_INVOKED", "RESULT_RECEIVED", {"evaluated": Y2}))
    cor6 = mod(cor5, "REVIEWER_SATISFIED: LIN-R1 cerrado frente a la corrección", lambda t: (
        r2c_step(t, 66, "REVIEWER_SATISFIED", "RESULT_INGESTED", {"evaluated": Y2}, "VALID", "INGESTED"),
        t.update(validity=val("GC-1", "ENDED", "REVIEWER_SATISFIED"), findings=[lin("LIN-R1", "CLOSED", "BLOCKING", "REVIEWER", "R1")])))
    T["a62-a1s-02-reviewer-corrige-y-re-revisa-la-correccion"] = ("A1", ["A62-A1S-02"], "VALID", set(), [cor0, cor1, cor2, cor3, cor4, cor5, cor6])
    T["a62-a1s-02-re-revision-sobre-el-objeto-anterior"] = ("A1", ["A62-A1S-02"], "INVALID", {"A1-P18"},
        [cor2, mod(cor3, "R2 abierta sobre Y (anterior a la corrección)", lambda t: (t["requests"][-1].update(object=Y),
                                                                                       t["requests"][-1]["attempts"][0].update(target=Y)))])
    T["a62-a1r-o3-reviewer-sin-su-linaje-heredado"] = ("A1", ["A62-A1R-O3"], "INVALID", {"A1-P16"},
                                                       [pe, ne, mod(cont2, "OpenFindings sin LIN-R1", lambda t: t["requests"][-1]["attempts"][0].update(open_findings=[]))])
    l8 = req("L8", "ARL-62", obj("x1"), "OPEN", [att(1, "BUDGET_RESERVED", "I8", obj("x1"), 62, REFS0, open_findings=["LIN-R1"], snapshot=snap(1, 1, 1))])
    arch_o3 = mod(ne, "ARCHITECT_REVIEW con un linaje del REVIEWER en OpenFindings", lambda t: (
        t.update(rv=62, validity=val("A2"), entries=[entry("ARL-62", [auth("A2")], 1, 1, 1)]),
        t["loop"].update(type=AR, phase="REVIEW_PENDING", instance="ARL-62", object=obj("x1"), authorization="A2"), t["requests"].append(copy.deepcopy(l8))))
    T["a62-a1r-o3-architect-con-linaje-del-reviewer"] = ("A1", ["A62-A1R-O3"], "INVALID", {"A1-P16"}, [pe, ne, arch_o3])
    po2 = rv_state("REVIEWER con la vigencia OPEN y un BLOCKING abierto", 60, "CORRECTING", rq("INGESTED", "RESULT_INGESTED", "VALID"), val("GC-1"), blk)
    no2 = rv_closed("LOOP_CLOSED (E) con la revocación en la decisión", 61, rq("INGESTED", "RESULT_INGESTED", "VALID"), blk,
                    rec(val("GC-1", "ENDED", "REVOKED", "RCLOSE-R1-REV"), 61, "RCLOSE-R1-REV"))
    T["a62-a1r-o2-cierre-E-con-revocacion-en-la-decision"] = ("A1", ["A62-A1R-O2"], "VALID", set(), [po2, no2])
    T["a62-a1r-o2-vigencia-OPEN-sin-revocacion"] = ("A1", ["A62-A1R-O2"], "INVALID", {"A1-R03"},
                                                    [po2, mod(no2, "decisión de cierre sin revocación", lambda t: (t["rclosures"][-1].update(closed_by="RCLOSE-R1"),
                                                                                                                     t["rclosures"][-1]["validity"].update(by="RCLOSE-R1")))])
    wp = st("ventana abierta (último punto Q0) con referencias en a0/k0", 90, requests=[ing(1, None)], v14=(1, 1, 1), anc={"x1", "a0", "k0"})
    wp["last_point"] = "Q0"
    wn = mod(wp, "Q7 que cierra la ventana con dos rebases (M1 y M2)", lambda t: t.update(rv=91, kind="WINDOW_CLOSE", last_point="Q7", last_rebase="M2",
                                                                                          history=["M1", "M2"], window_maps=["M1", "M2"], anc={"x1pp", "a0pp", "k0pp"}))
    T["a62-a1r-o4-ventana-con-dos-rebases"] = ("A1", ["A62-A1R-O4"], "VALID", set(), [wp, wn])
    T["a62-a1r-o4-ventana-solo-ultimo-mapa"] = ("A1", ["A62-A1R-O4"], "INVALID", {"A1-F10", "A1-P11"},
                                                [wp, mod(wn, "Q7 que custodia solo el último mapa", lambda t: t.update(history=["M2"]))])
    no_blk = rv_closed("LOOP_CLOSED (E) tras REVOKED antes de cualquier resultado; sin BLOCKING", 61, rq("INGESTED", "RESULT_INGESTED", "VALID"), [], rec(vrev, 61, "RCLOSE-R1"))
    T["a62-a1r-o5-satisfaccion-sin-evidencia"] = ("A1", ["A62-A1R-O5"], "INVALID", {"A1-R06"},
                                                  [mod(no_blk, "requisito de GC-1 dado por satisfecho sin REVIEWER_SATISFIED", lambda t: t.update(satisfied=["GC-1"]))])
    T["a62-a1r-o5-satisfaccion-con-evidencia"] = ("A1", ["A62-A1R-O5"], "VALID", set(),
                                                  [mod(closed_nf, "requisito de GC-1 satisfecho con el registro REVIEWER_SATISFIED", lambda t: t.update(satisfied=["GC-1"]))])
    ex = st("bucle EXECUTION (autoridad de §8 y §16)", 70, "EXECUTION", "WINDOW_OPEN", None, None, "GC-EXEC", val("GC-EXEC"), anc=ANC)
    T["a62-a1-obs01-r10-execution-sin-cambio"] = ("A1", ["OBS-A1-01", "OBS-A1-01/R10"], "VALID", set(), [ex])
    T["a62-a1-obs01-r10-execution-con-identidad-de-architect"] = ("A1", ["OBS-A1-01", "OBS-A1-01/R10"], "INVALID", {"A1-F06"},
                                                                 [mod(ex, "EXECUTION con instance_id", lambda t: t["loop"].update(instance="ARL-70"))])
    T["a62-a1-obs01-r10-LOOP_CLOSED-no-se-aplica-a-EXECUTION"] = ("A1", ["OBS-A1-01", "OBS-A1-01/R10"], "INVALID", {"A1-P01", "A1-P07"},
                                                                 [ex, st("NONE", 71, anc=ANC)])

    # ---- FC-02 literal
    o, op = obj("c1"), obj("c1p")
    p_lit = st("REVIEW_PENDING antes del rebase (V14)", 90, AR, "REVIEW_PENDING", None, o, "A1", val("A1"),
               [req("L1", None, o, "OPEN", [att(1, "BUDGET_RESERVED", "I1", o, 85)])], v14=(1, 1, 1), anc={"c1"})
    rec_lit = st("QU REBASE_RECONCILIATION (reconcilia)", 91, AR, "REVIEW_PENDING", None, op, "A1", val("A1"),
                 [req("L1", None, op, "OPEN", [att(1, "BUDGET_RESERVED", "I1b", op, 85)])], v14=(1, 1, 1), anc={"c1p"},
                 kind="REBASE_RECONCILIATION", last_rebase="M1", history=["M1"])
    stale_lit = mod(p_lit, "QU REBASE_RECONCILIATION (no reconcilia)", lambda t: t.update(rv=91, kind="REBASE_RECONCILIATION", last_rebase="M1",
                                                                                         history=["M1"], anc={"c1p"}))
    T["fc02-literal-reconciliar-orquestacion"] = ("V14", ["FC-02"], "INVALID", {"V14-I-P05", "V14-I-P13-object", "V14-B.8.8-object"}, [p_lit, rec_lit])
    T["fc02-literal-no-reconciliar-hueco"] = ("V14", ["FC-02"], "VALID", set(), [p_lit, stale_lit])

    # ---- FC-02 corrected: reconciliation of the orchestration
    e1 = entry("ARL-10", [auth("A1")], 1, 1, 1)
    p_am = st("REVIEW_PENDING antes del rebase", 90, AR, "REVIEW_PENDING", "ARL-10", o, "A1", val("A1"),
              [req("L1", "ARL-10", o, "OPEN", [att(1, "BUDGET_RESERVED", "I1", o, 85, REFS0)])], entries=[e1], anc={"c1", "a0", "k0"})
    rec_am = st("QU REBASE_RECONCILIATION", 91, AR, "REVIEW_PENDING", "ARL-10", op, "A1", val("A1"),
                [req("L1", "ARL-10", op, "OPEN", [att(1, "BUDGET_RESERVED", "I1b", op, 85, REFS1)])], entries=[e1], anc={"c1p", "a0p", "k0p"},
                kind="REBASE_RECONCILIATION", last_rebase="M1", history=["M1"])
    T["fc02-enmendado-reconcilia"] = ("A1", ["FC-02", "A62-A1-06"], "VALID", set(), [p_am, rec_am])
    stale_am = mod(p_am, "QU REBASE_RECONCILIATION (no reconcilia)", lambda t: t.update(rv=91, kind="REBASE_RECONCILIATION", last_rebase="M1",
                                                                                       history=["M1"], anc={"c1p", "a0p", "k0p"}))
    T["fc02-enmendado-no-reconciliar-lo-detecta-I-H02"] = ("A1", ["FC-02"], "INVALID", {"I-H02"}, [p_am, stale_am])
    T["fc02-enmendado-imagen-con-otro-blob"] = ("A1", ["FC-02"], "INVALID", {"A1-P02"},
                                                [p_am, mod(rec_am, "imagen con otro blob", lambda t: t["loop"].update(object=obj("c1p", DOC, "b-otro")))])
    T["fc02-enmendado-reconciliacion-cambia-contadores"] = ("A1", ["FC-02"], "INVALID", {"A1-F01", "A1-P10"},
                                                            [p_am, mod(rec_am, "la reconciliación cambia contadores", lambda t: t["entries"][0].update(architect_launches=2))])
    T["a62-a1-06-replanificada-con-referencias-originales"] = ("A1", ["A62-A1-06"], "INVALID", {"A1-P15"},
                                                               [p_am, mod(rec_am, "replanificada sin reconstruir referencias", lambda t: t["requests"][0]["attempts"][0].update(refs=copy.deepcopy(REFS0)))])

    # ---- A62-A1-04: LAUNCHING across a rebase
    inv0 = st("ARCHITECT_INVOKED con L1/1 en LAUNCHING", 60, AR, "ARCHITECT_INVOKED", "ARL-10", o, "A1", val("A1"),
              [req("L1", "ARL-10", o, "OPEN", [att(1, "LAUNCHING", "I1", o, 55, REFS0)])], entries=[e1], anc={"c1", "a0", "k0"})
    rb = st("QU REBASE_RECONCILIATION (LAUNCHING conserva su invocación)", 61, AR, "ARCHITECT_INVOKED", "ARL-10", op, "A1", val("A1"),
            [req("L1", "ARL-10", op, "OPEN", [att(1, "LAUNCHING", "I1", o, 55, REFS0)])], entries=[e1], anc={"c1p", "a0p", "k0p"},
            kind="REBASE_RECONCILIATION", last_rebase="M1", history=["M1"])
    started = mod(rb, "arrancó: LAUNCHED con el Target original", lambda t: (t.update(rv=62, kind="ORDINARY"), t["requests"][0]["attempts"][0].update(state="LAUNCHED")))
    T["a62-a1-04-LAUNCHING-tras-rebase-arranco"] = ("A1", ["A62-A1-04"], "VALID", set(), [inv0, rb, started])
    not_started = mod(rb, "no arrancó: BUDGET_RESERVED replanificado sobre la imagen",
                      lambda t: (t.update(rv=62, kind="ORDINARY"), t["loop"].update(phase="REVIEW_PENDING"),
                                 t["requests"][0]["attempts"][0].update(state="BUDGET_RESERVED", inv="I1b", target=op, refs=copy.deepcopy(REFS1))))
    T["a62-a1-04-LAUNCHING-tras-rebase-no-arranco"] = ("A1", ["A62-A1-04"], "VALID", set(), [inv0, rb, not_started])
    uncertain = mod(rb, "indeterminado: LAUNCH_UNCERTAIN con el Target original",
                    lambda t: (t.update(rv=62, kind="ORDINARY"), t["loop"].update(phase="REVIEW_PENDING"), t["requests"][0]["attempts"][0].update(state="LAUNCH_UNCERTAIN")))
    T["a62-a1-04-LAUNCHING-tras-rebase-incierto"] = ("A1", ["A62-A1-04"], "VALID", set(), [inv0, rb, uncertain])
    no_replan = mod(rb, "no arrancó sin replanificar", lambda t: (t.update(rv=62, kind="ORDINARY"), t["loop"].update(phase="REVIEW_PENDING"),
                                                                  t["requests"][0]["attempts"][0].update(state="BUDGET_RESERVED")))
    T["a62-a1-04-no-arranco-sin-replanificar"] = ("A1", ["A62-A1-04"], "INVALID", {"A1-P09", "I-H02", "V14-S18-target"}, [inv0, rb, no_replan])
    T["a62-a1-04-LAUNCHING-reescrito-en-la-reconciliacion"] = ("A1", ["A62-A1-04"], "INVALID", {"A1-P09"},
                                                               [inv0, mod(rb, "LAUNCHING reescrito", lambda t: t["requests"][0]["attempts"][0].update(target=op))])
    T["a62-a1-04-LAUNCHING-sin-imagen-en-el-mapa"] = ("A1", ["A62-A1-04"], "INVALID", {"A1-P02", "A1-P08"},
                                                      [inv0, mod(rb, "mapa sin c1", lambda t: t.update(last_rebase="M1-sin-c1", history=["M1-sin-c1"]))])

    # ---- A62-A1-05: reviewed-object equivalence through a rebase
    inv5 = st("ARCHITECT_INVOKED con L1/1 LAUNCHED", 70, AR, "ARCHITECT_INVOKED", "ARL-10", o, "A1", val("A1"),
              [req("L1", "ARL-10", o, "OPEN", [att(1, "LAUNCHED", "I1", o, 65, REFS0)])], [lin("LIN-1", "OPEN")], [e1], anc={"c1", "a0", "k0"})
    rb5 = st("QU REBASE_RECONCILIATION", 71, AR, "ARCHITECT_INVOKED", "ARL-10", op, "A1", val("A1"),
             [req("L1", "ARL-10", op, "OPEN", [att(1, "LAUNCHED", "I1", o, 65, REFS0)])], [lin("LIN-1", "OPEN")], [e1], anc={"c1p", "a0p", "k0p"},
             kind="REBASE_RECONCILIATION", last_rebase="M1", history=["M1"])
    T["fc02-enmendado-LAUNCHED-conserva-su-Target"] = ("A1", ["FC-02", "A62-A1-05"], "VALID", set(), [inv5, rb5])
    T["fc02-enmendado-reescribir-un-LAUNCHED"] = ("A1", ["FC-02"], "INVALID", {"A1-P09"},
                                                  [inv5, mod(rb5, "LAUNCHED reescrito", lambda t: t["requests"][0]["attempts"][0].update(target=op))])

    def ingest(evaluated):
        recv = mod(rb5, "RESULT_RECEIVED", lambda t: (t.update(rv=72, kind="ORDINARY"),
                                                      t["requests"][0]["attempts"][0].update(state="RESULT_RECEIVED", result={"evaluated": evaluated})))
        done = mod(recv, "ingestión AGREED: ARCHITECT_SATISFIED", lambda t: (
            t.update(rv=73, validity=val("A1", "ENDED", "ARCHITECT_SATISFIED"), entries=[entry("ARL-10", [auth("A1", "ENDED", "ARCHITECT_SATISFIED")], 1, 1, 1)],
                     findings=[lin("LIN-1", "CLOSED")]),
            t["loop"].update(phase="ARCHITECT_SATISFIED"), t["requests"][0].update(state="INGESTED"),
            t["requests"][0]["attempts"][0].update(state="RESULT_INGESTED", outcome="VALID")))
        return [inv5, rb5, recv, done]
    T["a62-a1-05-ingesta-de-un-LAUNCHED-tras-rebase"] = ("A1", ["A62-A1-05"], "VALID", set(), ingest(o))
    T["a62-a1-05-ingesta-con-otro-blob"] = ("A1", ["A62-A1-05"], "INVALID", {"A1-P12"}, ingest(obj("c1", DOC, "b-otro")))
    T["a62-a1-05-ingesta-con-imagen-no-probada"] = ("A1", ["A62-A1-05"], "INVALID", {"A1-P12"}, ingest(obj("c9", DOC, "b-C")))

    # ---- A62-A1-06: branch-local references after one and two rebases
    hist_att = [att(1, "RESULT_INGESTED", "I1", obj("x1"), 75, REFS0, {"evaluated": obj("x1")}, "VALID")]
    base6 = st("CORRECTING con referencias en a0/k0", 80, AR, "CORRECTING", "ARL-10", obj("x1"), "A1", val("A1"),
               [req("L1", "ARL-10", obj("x1"), "INGESTED", hist_att)], [lin("LIN-1", "OPEN")], [e1], anc={"x1", "a0", "k0"})
    rb6a = mod(base6, "primer rebase (M1)", lambda t: (t.update(rv=81, kind="REBASE_RECONCILIATION", last_rebase="M1", history=["M1"], anc={"x1p", "a0p", "k0p"}),
                                                       t["loop"].update(object=obj("x1p"))))
    rb6b = mod(rb6a, "segundo rebase (M2)", lambda t: (t.update(rv=82, last_rebase="M2", history=["M1", "M2"], anc={"x1pp", "a0pp", "k0pp"}),
                                                       t["loop"].update(object=obj("x1pp"))))
    T["a62-a1-06-referencias-tras-un-rebase"] = ("A1", ["A62-A1-06"], "VALID", set(), [base6, rb6a])
    T["a62-a1-06-referencias-tras-dos-rebases"] = ("A1", ["A62-A1-06"], "VALID", set(), [base6, rb6a, rb6b])
    clean = {k: v for k, v in BLOBS.items() if k[0] in {"x1pp", "a0pp", "k0pp"}}
    T["a62-a1-06-sucesor-en-clon-limpio"] = ("A1", ["A62-A1-06"], "VALID", set(), [mod(rb6b, "clon limpio: solo imágenes", lambda t: t.update(blobs=dict(clean)))])
    T["a62-a1-06-mapa-intermedio-ausente"] = ("A1", ["A62-A1-06"], "INVALID", {"A1-F10"}, [mod(rb6b, "sin M1 en la cadena", lambda t: t.update(history=["M2"]))])
    T["a62-a1-06-blob-cambiado"] = ("A1", ["A62-A1-06"], "INVALID", {"A1-F10"},
                                    [mod(rb6b, "decisiones con otro blob en la imagen", lambda t: t["blobs"].update({("a0pp", DEC): "b-otro"}))])
    lit6 = mod(rb6b, "referencias tras dos rebases (V14 literal)", lambda t: (t.update(v14=snap(1, 1, 1)), t["requests"][0].update(loop=None)))
    T["fc06-literal-referencias-tras-rebase"] = ("V14", ["A62-A1-06"], "INVALID", {"V14-I-S18-ancestro"}, [lit6])

    # ---- O3: inherited open lineages
    def new_loop(open_findings):
        l5 = req("L5", "ARL-42", obj("y1", IMPL), "OPEN", [att(1, "BUDGET_RESERVED", "I5", obj("y1", IMPL), 42, REFS0, open_findings=open_findings, snapshot=snap(1, 1, 1))])
        return mod(closed_exp, "bucle nuevo ARL-42 con A2", lambda t: (
            t.update(rv=42, validity=val("A2"), entries=t["entries"] + [entry("ARL-42", [auth("A2")], 1, 1, 1)]),
            t["loop"].update(type=AR, phase="REVIEW_PENDING", instance="ARL-42", object=obj("y1", IMPL), authorization="A2"), t["requests"].append(l5)))
    T["a62-a1-o3-bucle-nuevo-hereda-linajes-abiertos"] = ("A1", ["A62-A1-O3", "A62-A1-02"], "VALID", set(), [exp, closed_exp, new_loop(["LIN-1"])])
    T["a62-a1-o3-bucle-nuevo-sin-linajes-heredados"] = ("A1", ["A62-A1-O3"], "INVALID", {"A1-P16"}, [exp, closed_exp, new_loop([])])

    # ---- one specific negative for each remaining rule
    T["a62-a1-01-instance-id-distinto-del-QU-de-apertura"] = ("A1", ["A62-A1-01"], "INVALID", {"A1-P03"}, [closed, mod(opened, "apertura con ARL-99", lambda t: (
        t["loop"].update(instance="ARL-99"), t["requests"][3].update(loop="ARL-99"), t["entries"][1].update(id="ARL-99")))])
    T["a62-a1-01-solicitud-cambia-de-bucle"] = ("A1", ["A62-A1-01"], "INVALID", {"A1-F01", "A1-P14"},
                                                [closed, mod(opened, "L1 pasa a ARL-22", lambda t: t["requests"][0].update(loop="ARL-22"))])
    T["a62-a1-03-budgets-de-V14-cuenta-una-solicitud-del-architect"] = ("A1", ["A62-A1-03"], "INVALID", {"A1-F07"},
                                                                        [mod(opened, "budgets de V14 cuenta L4", lambda t: t.update(v14=snap(1, 1, 1)))])
    T["a62-a1-06-historia-de-mapas-sin-last_rebase-al-final"] = ("A1", ["A62-A1-06"], "INVALID", {"A1-F09"},
                                                                 [mod(rb6b, "last_rebase = M1 con historia [M1, M2]", lambda t: t.update(last_rebase="M1"))])
    T["a62-a1-06-historia-cambia-sin-reconciliacion"] = ("A1", ["A62-A1-06"], "INVALID", {"A1-P11"},
                                                         [corr, mod(corr, "historia con M1 sin reconciliación", lambda t: t.update(rv=31, history=["M1"]))])
    return T


def main():
    results, coverage, ok = {}, {}, True
    for name, (validator, tags, expected, rules, trace) in sorted(traces().items()):
        got, got_rules, why = run(trace, validator)
        passed = got == expected and set(got_rules) == set(rules)
        ok = ok and passed
        results[name] = {"Validator": "A-1 corregida" if validator == "A1" else "V14 literal", "Covers": tags,
                         "Expected": {"Verdict": expected, "Rules": sorted(rules)}, "Got": {"Verdict": got, "Rules": got_rules},
                         "Violations": why, "Verdict": "PASS" if passed else "FAIL"}
        for tag in tags:
            coverage.setdefault(tag, []).append(name)
    required = ["A62-A1-0%d" % i for i in range(1, 7)] + ["A62-A1-O%d" % i for i in range(1, 6)] + ["OBS-A1-01"] + ["OBS-A1-01/R%d" % i for i in range(1, 11)] \
        + ["A62-A1R-0%d" % i for i in range(1, 4)] + ["A62-A1R-O%d" % i for i in range(1, 6)] + ["A62-A1A-01", "A62-A1S-01", "A62-A1S-02"]
    missing = [t for t in required if t not in coverage]
    ok = ok and not missing
    with open(sys.argv[1], "w", encoding="utf-8", newline="\n") as f:
        json.dump({"Script": "a1-counterexamples.py", "Version": "A-1 corregida (2026-10-05), con OBS-A1-01, A62-A1R-01..03, A62-A1A-01 y A62-A1S-01..02",
                   "Freeze": {"commit": "4c617e82b32b6c810b68d75fc19472efed22b393", "blob": "34ad80ea1bfff144bfc5169f62920a4c904c1bfa"},
                   "Rules": RULES, "Coverage": {k: coverage[k] for k in sorted(coverage)}, "MissingCoverage": missing,
                   "Traces": results, "AllAsExpected": ok}, f, ensure_ascii=False, indent=1)
        f.write("\n")
    print(json.dumps({k: v["Got"]["Verdict"] + (" ok" if v["Verdict"] == "PASS" else " MISMATCH %s" % v["Got"]["Rules"]) for k, v in results.items()},
                     ensure_ascii=False, indent=0))
    print("traces", len(results), "all", ok, "missing", missing)
    return 0 if ok else 1


if __name__ == "__main__":
    sys.exit(main())
