"""EXPERIMENTAL — NOT AUTHORIZED FOR PRODUCTION. I-62 night order §9: complete combined sequences for F4 preparation (not F4).

Runs five combined sequences (and their negatives) through the SAME symbolic engine as the A-1 harness (a1-counterexamples.py, imported read-only
from the review clone pinned at the A-1 object; the harness file is not modified). Variant 2 (Freeze + A-1 proposed) is the asserted oracle; variant 1
(Freeze literal, validator "V14") is recorded as information only and is never attributed to the amended variant. Expected verdicts and EXACT rule sets
were fixed before the first run (night order §11).

Usage: python combo_sequences.py <a1-counterexamples.py> <out.json>
"""
import copy
import hashlib
import importlib.util
import json
import sys

HARNESS, OUT = sys.argv[1:3]
spec = importlib.util.spec_from_file_location("a1h", HARNESS)
h = importlib.util.module_from_spec(spec)
spec.loader.exec_module(h)
st, req, att, entry, auth, val, obj, mod, snap, lin = h.st, h.req, h.att, h.entry, h.auth, h.val, h.obj, h.mod, h.snap, h.lin
AR, RV, NONE, DOC, IMPL, REFS0, REFS1 = h.AR, h.RV, h.NONE, h.DOC, h.IMPL, h.REFS0, h.REFS1
REFS2 = [{"commit": "a0pp", "path": h.DEC, "blob": "b-D"}, {"commit": "k0pp", "path": h.BND, "blob": "b-K"}]
ANC = {"x1", "x2", "x3", "y1", "a0", "k0", "c1"}
ANC_P = {"x1p", "x2", "x3", "y1", "a0p", "k0p", "c1p"}
# experiment-only decisions (added to the in-memory engine; nothing is written back)
h.DECISIONS["A2low"] = {"kind": "RLA", "continues": "ARL-10", "budget": {"review_rounds": 1, "logical_requests": 1, "architect_launches": 3}}
h.DECISIONS["A2eq"] = {"kind": "RLA", "continues": "ARL-10", "budget": {"review_rounds": 2, "logical_requests": 2, "architect_launches": 3}}
S = {}


def ing(i, loop, refs=REFS0):
    o = obj("x%d" % i)
    return req("L%d" % i, loop, o, "INGESTED", [att(1, "RESULT_INGESTED", "I%d" % i, o, 10 + i, refs, {"evaluated": o}, "VALID")])


# ---------------------------------------------------------------- C1: expiration + rebase (ARCHITECT_REVIEW)
e10_exp = entry("ARL-10", [auth("A1", "ENDED", "EXPIRED")], 1, 1, 1)
c1_exp = st("CORRECTING con A1 EXPIRED, escalada resuelta por CLOSE-10", 40, AR, "CORRECTING", "ARL-10", obj("x1"), "A1", val("A1", "ENDED", "EXPIRED"),
            [ing(1, "ARL-10")], [lin("LIN-1", "OPEN")], [e10_exp], anc=ANC, escalation=("COORDINATOR", "CLOSE-10"))
c1_rb = mod(c1_exp, "QU REBASE_RECONCILIATION (M1) con la vigencia EXPIRED", lambda t: (
    t.update(rv=41, kind="REBASE_RECONCILIATION", last_rebase="M1", history=["M1"], anc=set(ANC_P)), t["loop"].update(object=obj("x1p"))))
c1_closed = st("LOOP_CLOSED por CLOSE-10 tras el rebase", 42, requests=c1_exp["requests"], findings=c1_exp["findings"],
               entries=[dict(e10_exp, closed_at=42, closed_by="CLOSE-10")], anc=ANC_P, history=["M1"], last_rebase="M1")
l2 = req("L2", "ARL-43", obj("x1p"), "OPEN", [att(1, "BUDGET_RESERVED", "I2", obj("x1p"), 43, REFS1, open_findings=["LIN-1"], snapshot=snap(1, 1, 1))])
c1_new = mod(c1_closed, "apertura de ARL-43 con A2 sobre la imagen; hereda LIN-1", lambda t: (
    t.update(rv=43, validity=val("A2"), entries=t["entries"] + [entry("ARL-43", [auth("A2")], 1, 1, 1)]),
    t["loop"].update(type=AR, phase="REVIEW_PENDING", instance="ARL-43", object=obj("x1p"), authorization="A2"), t["requests"].append(copy.deepcopy(l2))))
S["c1-expiracion-y-rebase"] = ("expiración + rebase", "VALID", set(), [c1_exp, c1_rb, c1_closed, c1_new])
S["c1n-cierre-tras-rebase-sin-decision"] = ("expiración + rebase", "INVALID", {"A1-P07"},
                                            [c1_exp, c1_rb, mod(c1_closed, "LOOP_CLOSED sin decisión", lambda t: t["entries"][0].update(closed_by=None))])
S["c1n-rebase-sin-reconciliar-loop-object"] = ("expiración + rebase", "INVALID", {"I-H02"},
                                               [c1_exp, mod(c1_rb, "loop.object sin reconciliar", lambda t: t["loop"].update(object=obj("x1")))])

# ---------------------------------------------------------------- C2: REVIEWER with inherited BLOCKING + administrative close + new authority
Y = obj("y1", IMPL)
blk = [lin("LIN-R1", "OPEN", "BLOCKING", "REVIEWER", "R1")]
blk_closed = [lin("LIN-R1", "CLOSED", "BLOCKING", "REVIEWER", "R1")]
r1 = req("R1", None, Y, "INGESTED", [att(1, "RESULT_INGESTED", "IR1", Y, 51, REFS0, {"evaluated": Y}, "VALID")], authz="GC-1")
vx = val("GC-1", "ENDED", "EXHAUSTED")
rec1 = {"last_request": "R1", "authorization": "GC-1", "validity": vx, "closed_at": 61, "closed_by": "RCLOSE-R1"}
c2_p = st("REVIEWER EXHAUSTED con LIN-R1 BLOCKING abierto, escalada resuelta", 60, RV, "CORRECTING", None, Y, "GC-1", vx, [r1], blk, v14=(1, 1, 1),
          anc=ANC, escalation=("COORDINATOR", "RCLOSE-R1"))
c2_e = st("LOOP_CLOSED (E) por RCLOSE-R1; EXHAUSTED se conserva", 61, requests=[r1], findings=blk, v14=(1, 1, 1), anc=ANC, rclosures=[rec1])


def r2(state, attempt_state, result=None, outcome=None, authz="GC-2"):
    return req("R2", None, Y, state, [att(1, attempt_state, "IR2", Y, 62, REFS0, result, outcome, open_findings=["LIN-R1"])], authz=authz)


def rv_open(label, rv, phase, request, validity, findings, rclosures, authz="GC-2"):
    return st(label, rv, RV, phase, None, Y, authz, validity, [r1, request], findings, v14=(2, 2, 2), anc=ANC, rclosures=rclosures)


c2_new = rv_open("bucle REVIEWER nuevo con GC-2 (decisión custodiada); R2 hereda LIN-R1", 62, "REVIEW_PENDING", r2("OPEN", "BUDGET_RESERVED"), val("GC-2"), blk, [rec1])
c2_g = rv_open("R2/1 LAUNCHING", 63, "ARCHITECT_INVOKED", r2("OPEN", "LAUNCHING"), val("GC-2"), blk, [rec1])
c2_l = rv_open("R2/1 LAUNCHED", 64, "ARCHITECT_INVOKED", r2("OPEN", "LAUNCHED"), val("GC-2"), blk, [rec1])
c2_r = rv_open("R2/1 RESULT_RECEIVED", 65, "ARCHITECT_INVOKED", r2("OPEN", "RESULT_RECEIVED", {"evaluated": Y}), val("GC-2"), blk, [rec1])
VS2 = val("GC-2", "ENDED", "REVIEWER_SATISFIED")
c2_s = rv_open("REVIEWER_SATISFIED con GC-2: R2 VALID ingerido y LIN-R1 cerrado por su emisor", 66, "REVIEWER_SATISFIED",
               r2("INGESTED", "RESULT_INGESTED", {"evaluated": Y}, "VALID"), VS2, blk_closed, [rec1])
rec2 = {"last_request": "R2", "authorization": "GC-2", "validity": VS2, "closed_at": 67, "closed_by": None}
c2_c = st("LOOP_CLOSED (S) de GC-2", 67, requests=[r1, r2("INGESTED", "RESULT_INGESTED", {"evaluated": Y}, "VALID")], findings=blk_closed, v14=(2, 2, 2),
          anc=ANC, rclosures=[rec1, rec2])
S["c2-reviewer-blocking-heredado-cierre-E-y-nueva-autoridad"] = ("REVIEWER con BLOCKING heredado + cierre administrativo + nueva autorización", "VALID", set(),
                                                                 [c2_p, c2_e, c2_new, c2_g, c2_l, c2_r, c2_s, c2_c])
S["c2n-reapertura-con-la-autoridad-agotada"] = ("REVIEWER con BLOCKING heredado + cierre administrativo + nueva autorización", "INVALID", {"A1-R05"},
                                                [c2_p, c2_e, rv_open("bucle REVIEWER reabierto con GC-1 (EXHAUSTED)", 62, "REVIEW_PENDING",
                                                                     r2("OPEN", "BUDGET_RESERVED", authz="GC-1"), val("GC-1"), blk, [rec1], authz="GC-1")])
S["c2n-satisfecho-con-el-BLOCKING-heredado-abierto"] = ("REVIEWER con BLOCKING heredado + cierre administrativo + nueva autorización", "INVALID", {"A1-R02"},
                                                        [c2_r, mod(c2_s, "REVIEWER_SATISFIED con LIN-R1 abierto", lambda t: t.update(findings=copy.deepcopy(blk)))])

# ---------------------------------------------------------------- C3: two rebases + crash in LAUNCHING
o, op, opp = obj("c1"), obj("c1p"), obj("c1pp")
e1 = entry("ARL-10", [auth("A1")], 1, 1, 1)
c3_0 = st("ARCHITECT_INVOKED con L1/1 en LAUNCHING", 60, AR, "ARCHITECT_INVOKED", "ARL-10", o, "A1", val("A1"),
          [req("L1", "ARL-10", o, "OPEN", [att(1, "LAUNCHING", "I1", o, 55, REFS0)])], entries=[e1], anc={"c1", "a0", "k0"})
c3_1 = st("primer rebase (M1): LAUNCHING conserva su invocación", 61, AR, "ARCHITECT_INVOKED", "ARL-10", op, "A1", val("A1"),
          [req("L1", "ARL-10", op, "OPEN", [att(1, "LAUNCHING", "I1", o, 55, REFS0)])], entries=[e1], anc={"c1p", "a0p", "k0p"},
          kind="REBASE_RECONCILIATION", last_rebase="M1", history=["M1"])
c3_2 = mod(c3_1, "segundo rebase (M2): LAUNCHING sigue con su invocación original", lambda t: (
    t.update(rv=62, last_rebase="M2", history=["M1", "M2"], anc={"c1pp", "a0pp", "k0pp"}), t["loop"].update(object=opp), t["requests"][0].update(object=opp)))
c3_3 = mod(c3_2, "caída: no arrancó (prueba custodiada) → BUDGET_RESERVED replanificado sobre la imagen de la cadena", lambda t: (
    t.update(rv=63, kind="ORDINARY"), t["loop"].update(phase="REVIEW_PENDING"),
    t["requests"][0]["attempts"][0].update(state="BUDGET_RESERVED", inv="I1c", target=opp, refs=copy.deepcopy(REFS2), not_started="NS-1")))
c3_r1 = mod(c3_1, "caída resuelta tras el primer rebase: no arrancó → BUDGET_RESERVED replanificado sobre c1p", lambda t: (
    t.update(rv=62, kind="ORDINARY"), t["loop"].update(phase="REVIEW_PENDING"),
    t["requests"][0]["attempts"][0].update(state="BUDGET_RESERVED", inv="I1b", target=op, refs=copy.deepcopy(REFS1), not_started="NS-1")))
c3_r2 = mod(c3_r1, "segundo rebase (M2): el intento no lanzado se replanifica sobre c1pp", lambda t: (
    t.update(rv=63, kind="REBASE_RECONCILIATION", last_rebase="M2", history=["M1", "M2"], anc={"c1pp", "a0pp", "k0pp"}),
    t["loop"].update(object=opp), t["requests"][0].update(object=opp),
    t["requests"][0]["attempts"][0].update(inv="I1c", target=opp, refs=copy.deepcopy(REFS2))))
S["c3-dos-rebases-con-la-caida-resuelta-entre-ambos"] = ("dos rebases + caída en LAUNCHING", "VALID", set(), [c3_0, c3_1, c3_r1, c3_r2])
# A62-A1T-01 (Coordinator order after R20261005T073911Z-2dfe): formerly "c3-obs-segundo-rebase-con-LAUNCHING-pendiente-para-sin-publicar", INVALID {A1-P08}
S["c3-obs-segundo-rebase-con-LAUNCHING-pendiente-reconcilia-por-la-cadena"] = ("dos rebases + caída en LAUNCHING", "VALID", set(), [c3_0, c3_1, c3_2])
S["c3-obs-n-sin-M1-no-resuelve"] = ("dos rebases + caída en LAUNCHING", "INVALID", {"A1-P08"},
                                   [mod(c3_1, "rebase 1 con M1 sin el eslabón c1", lambda t: t.update(last_rebase="M1-sin-c1", history=["M1-sin-c1"])),
                                    mod(c3_2, "rebase 2 sin eslabón para c1", lambda t: t.update(history=["M1-sin-c1", "M2"]))])
S["c3n-segundo-rebase-reescribe-el-LAUNCHING"] = ("dos rebases + caída en LAUNCHING", "INVALID", {"A1-P09"},  # A62-A1T-01: no longer A1-P08
                                                  [c3_0, c3_1, mod(c3_2, "LAUNCHING reescrito en el segundo rebase",
                                                                   lambda t: t["requests"][0]["attempts"][0].update(target=op))])
# expectation corrected after the first run (declared in the README): A1-P15 applies only to replanned invocations; the stale Target is caught by I-H02 and V14-S18-target
S["c3n-segundo-rebase-sin-replanificar-el-no-lanzado"] = ("dos rebases + caída en LAUNCHING", "INVALID", {"I-H02", "V14-S18-target"},
                                                         [c3_0, c3_1, c3_r1, mod(c3_r2, "el intento conserva c1p tras M2", lambda t: t["requests"][0]["attempts"][0].update(
                                                             inv="I1b", target=op, refs=copy.deepcopy(REFS1)))])

# ---------------------------------------------------------------- C4: replacing authorization with caps below the consumption
e10_2 = entry("ARL-10", [auth("A1")], 2, 2, 2)
c4_0 = st("CORRECTING con A1 OPEN y 2/2/2 consumidos", 30, AR, "CORRECTING", "ARL-10", obj("x2"), "A1", val("A1"), [ing(1, "ARL-10"), ing(2, "ARL-10")],
          [lin("LIN-1", "OPEN")], [e10_2], anc=ANC)


def replaced(label, decision, rv=31):
    return mod(c4_0, label, lambda t: (t.update(rv=rv, validity=val(decision), entries=[entry("ARL-10", [auth("A1", "ENDED", "SUPERSEDED", decision), auth(decision)], 2, 2, 2)]),
                                        t["loop"].update(authorization=decision)))


S["c4-sustituta-con-topes-iguales-a-lo-consumido"] = ("autorización sustituta con límites menores a lo consumido", "VALID", set(),
                                                      [c4_0, replaced("A2eq sustituye a A1 con 2/2/3 (topes = consumo)", "A2eq")])
c4_eq = replaced("A2eq sustituye a A1 con 2/2/3", "A2eq")
l3 = req("L3", "ARL-10", obj("x3"), "OPEN", [att(1, "BUDGET_RESERVED", "I3", obj("x3"), 33, REFS0, open_findings=["LIN-1"], snapshot=snap(3, 3, 3))])
c4_over = mod(c4_eq, "reserva de una tercera versión por encima del tope", lambda t: (
    t.update(rv=33, entries=[entry("ARL-10", [auth("A1", "ENDED", "SUPERSEDED", "A2eq"), auth("A2eq")], 3, 3, 3)]),
    t["loop"].update(phase="REREVIEW_PENDING", object=obj("x3")), t["requests"].append(copy.deepcopy(l3))))
S["c4n-reserva-por-encima-del-tope-estrechado"] = ("autorización sustituta con límites menores a lo consumido", "INVALID", {"A1-F01", "A1-P01", "A1-P02"},
                                                   [c4_eq, c4_over])
S["c4n-sustituta-con-topes-menores-que-lo-consumido"] = ("autorización sustituta con límites menores a lo consumido", "INVALID", {"A1-F01"},
                                                         [c4_0, replaced("A2low sustituye a A1 con 1/1/3 (< consumo 2/2/2)", "A2low")])

# ---------------------------------------------------------------- C5: late response + cancelled attempt
x = obj("x1")


def c5(label, rv, phase, attempts, validity, entry_counts, auths):
    return st(label, rv, AR, phase, "ARL-10", x, "A1", validity, [req("L1", "ARL-10", x, "OPEN", attempts)], [lin("LIN-1", "OPEN")],
              [entry("ARL-10", auths, *entry_counts)], anc=ANC)


c5_0 = c5("L1/1 LAUNCHED", 70, "ARCHITECT_INVOKED", [att(1, "LAUNCHED", "I1", x, 69, REFS0)], val("A1"), (1, 1, 1), [auth("A1")])
c5_1 = c5("L1/1 LAUNCH_UNCERTAIN (cuenta como lanzado)", 71, "REVIEW_PENDING", [att(1, "LAUNCH_UNCERTAIN", "I1", x, 69, REFS0, outcome="UNCERTAIN")],
          val("A1"), (1, 1, 1), [auth("A1")])
c5_2 = c5("L1/2 BUDGET_RESERVED (reejecución de transporte)", 72, "REVIEW_PENDING",
          [att(1, "LAUNCH_UNCERTAIN", "I1", x, 69, REFS0, outcome="UNCERTAIN"), att(2, "BUDGET_RESERVED", "I1r", x, 72, REFS0, open_findings=["LIN-1"], snapshot=snap(1, 1, 2))], val("A1"), (1, 1, 2), [auth("A1")])
c5_3 = c5("A1 EXPIRED: L1/2 CANCELLED_BEFORE_LAUNCH", 73, "REVIEW_PENDING",
          [att(1, "LAUNCH_UNCERTAIN", "I1", x, 69, REFS0, outcome="UNCERTAIN"), att(2, "CANCELLED_BEFORE_LAUNCH", "I1r", x, 72, REFS0, open_findings=["LIN-1"], snapshot=snap(1, 1, 2))],
          val("A1", "ENDED", "EXPIRED"), (1, 1, 2), [auth("A1", "ENDED", "EXPIRED")])
c5_4 = mod(c5_3, "llega el resultado tardío de L1/1: se registra como evidencia sin ingerirse (V14 §20.6 caso B.5)", lambda t: t.update(rv=74))
S["c5-respuesta-tardia-e-intento-cancelado"] = ("respuesta tardía + intento cancelado", "VALID", set(), [c5_0, c5_1, c5_2, c5_3, c5_4])
S["c5n-resultado-tardio-ingerido"] = ("respuesta tardía + intento cancelado", "INVALID", {"A1-P09"},
                                      [c5_3, mod(c5_3, "L1/1 LAUNCH_UNCERTAIN → RESULT_RECEIVED", lambda t: (
                                          t.update(rv=74), t["requests"][0]["attempts"][0].update(state="RESULT_RECEIVED", result={"evaluated": x})))])
S["c5n-cancelado-relanzado"] = ("respuesta tardía + intento cancelado", "INVALID", {"A1-P09", "A1-P13"},
                                [c5_3, mod(c5_3, "L1/2 CANCELLED → LAUNCHING con la vigencia terminada", lambda t: (
                                    t.update(rv=74), t["loop"].update(phase="ARCHITECT_INVOKED"), t["requests"][0]["attempts"][1].update(state="LAUNCHING")))])


def main():
    results, ok = {}, True
    for name, (combo, expected, rules, trace) in S.items():
        got, got_rules, why = h.run(trace, "A1")
        lit, lit_rules, _ = h.run(trace, "V14")
        passed = got == expected and set(got_rules) == set(rules)
        ok = ok and passed
        results[name] = {"Combination": combo, "Points": len(trace), "FreezePlusA1": {"Expected": {"Verdict": expected, "Rules": sorted(rules)},
                                                                                    "Got": {"Verdict": got, "Rules": got_rules}, "Violations": why[:8]},
                         "FreezeLiteralInformative": {"Verdict": lit, "Rules": lit_rules}, "Result": "PASS" if passed else "FAIL"}
    blob = hashlib.sha1(b"blob %d\0" % len(open(HARNESS, "rb").read()) + open(HARNESS, "rb").read()).hexdigest()
    doc = {"Label": "EXPERIMENTAL — NOT AUTHORIZED FOR PRODUCTION", "Script": "combo_sequences.py",
           "Engine": {"Harness": "docs/automation/evidence/I-62-A1/a1-counterexamples.py", "GitBlobOfFileRead": blob},
           "Variants": {"Asserted": "Freeze + A-1 propuesta del mismo commit que el arnés leído (Engine.GitBlobOfFileRead)",
                        "Informative": "Freeze literal (validador V14), nunca atribuido a la variante ampliada"},
           "Sequences": results, "AllAsExpected": ok}
    with open(OUT, "w", encoding="utf-8", newline="\n") as f:
        json.dump(doc, f, ensure_ascii=False, indent=1)
        f.write("\n")
    for k, v in results.items():
        print("%-62s %s %s %s | V14 %s %s" % (k, v["Result"], v["FreezePlusA1"]["Got"]["Verdict"], v["FreezePlusA1"]["Got"]["Rules"],
                                             v["FreezeLiteralInformative"]["Verdict"], v["FreezeLiteralInformative"]["Rules"]))
    print("all", ok)
    return 0 if ok else 1


if __name__ == "__main__":
    sys.exit(main())
