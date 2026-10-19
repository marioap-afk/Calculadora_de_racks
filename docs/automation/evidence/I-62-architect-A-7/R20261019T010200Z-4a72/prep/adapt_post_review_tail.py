"""Sustituye en kit/post-review.py (tras adapt_post_review_head.py) los bloques de la coherencia del esquema de A-6 ronda 4 por los del esquema de
A-7 (rackcad-a7-architect-review/0): STEP0_KEYS, ARRAYS/NESTED/SINGLES y las funciones collect_refs y coherence. REF_RX, NOT_DET, BLOCKED,
LAUNCH_ENV_PREFIXES, RUN_JSON_FIELDS, outcome_categories e items quedan byte a byte. Cada bloque antiguo debe aparecer exactamente una vez.
Uso: python -I -B adapt_post_review_tail.py <kit dir>
"""
import sys

p = sys.argv[1] + "/post-review.py"
s = open(p, encoding="utf-8").read()

OLD_STEP0 = ('STEP0_KEYS = ("V14DirectReadL2333L2334", "OrderReadWhole", "Decisions85DeclaresOrderBytesAndSha256", "Decisions85RoundThreeDisposition",\n'
             '              "GuardsRunModeBaseHeadResult", "SelftestA6TextBlobMatchesObject", "Round3OutputIdentifiesRunAndFindings")\n')
NEW_STEP0 = ('STEP0_KEYS = ("V14DirectReadB9L2231L2233", "OrderReadWhole", "Decisions93DeclaresOrderBytesAndSha256",\n'
             '              "Decisions93IdentifiesObjectAndAuthorizesReview", "Decisions90RecordsA6AgreedBlob", "GuardsRunModeBaseHeadResult",\n'
             '              "PreflightSummaryCandidateAndTotals")\n')
OLD_ARR = ('ARRAYS = (("Round3FindingResolution", "Round3Finding"), ("NegativeCaseCoherence", "Case"), ("EvidenceAdequacy", "Aspect"),\n'
           '          ("OptionalDispositionConformance", "Round3Optional"), ("QuestionAnswers", "Question"))\n'
           'NESTED = (("ClosureRuleMechanical", "Coverage", "Case"), ("ClosureRuleMechanical", "SubRules", "SubRule"))\n'
           'SINGLES = ("ClosureRuleMechanical", "A69Compatibility", "DeltaPurity")\n')
NEW_ARR = ('ARRAYS = (("TopicVerdicts", "Topic"), ("QuestionAnswers", "Question"))\n'
           'NESTED = (("Materiality", "Triggers", "Trigger"), ("RelatedImpacts", "FailClosedProposals", "Proposal"))\n'
           'SINGLES = ("Materiality", "OwnerReservedConsequence")\n'
           'NESTED_SINGLES = (("RelatedImpacts", "ContractExclusionD1"),)\n'
           'FC_FLAGS = ("ChangesPassCriteria", "ExcludesValidC24Cases", "ReliesOnImplementationCoincidence")\n')
start = s.index("def collect_refs(result):\n")
end = s.index('if __name__ == "__main__":\n')
NEW_FUNCS = '''def dct(x):
    return x if isinstance(x, dict) else {}


def collect_refs(result):
    """→ [(group, ref, None)]: every Evidence of a finding and every ClauseRefs entry (no excerpts in this review)."""
    r = dct(result)
    out = []
    for key in ("RequiredFindings", "OptionalFindings"):
        for x in items(r.get(key)):
            out += [("%s[%s]" % (key, x.get("FindingId")), ref, None) for ref in (x.get("Evidence") or [])]
    for key, label in ARRAYS:
        for x in items(r.get(key)):
            out += [("%s[%s]" % (key, x.get(label)), ref, None) for ref in (x.get("ClauseRefs") or [])]
    for parent, key, label in NESTED:
        for x in items(dct(r.get(parent)).get(key)):
            out += [("%s.%s[%s]" % (parent, key, x.get(label)), ref, None) for ref in (x.get("ClauseRefs") or [])]
    out += [("DirectReads", x.get("Ref"), None) for x in items(r.get("DirectReads"))]
    for key in SINGLES:
        out += [(key, ref, None) for ref in (dct(r.get(key)).get("ClauseRefs") or [])]
    for parent, key in NESTED_SINGLES:
        out += [("%s.%s" % (parent, key), ref, None) for ref in (dct(dct(r.get(parent)).get(key)).get("ClauseRefs") or [])]
    return out


def coherence(result, c):
    v = result.get("Verdict")
    req, opt = items(result.get("RequiredFindings")), items(result.get("OptionalFindings"))
    req_ids, opt_ids = [f.get("FindingId") for f in req], [f.get("FindingId") for f in opt]
    all_ids = set(req_ids) | set(opt_ids)
    tv, qa = items(result.get("TopicVerdicts")), items(result.get("QuestionAnswers"))
    mat = dct(result.get("Materiality"))
    trig = items(mat.get("Triggers"))
    ri = dct(result.get("RelatedImpacts"))
    d1, fcp = dct(ri.get("ContractExclusionD1")), items(ri.get("FailClosedProposals"))
    orc, ia, step0 = dct(result.get("OwnerReservedConsequence")), dct(result.get("IfAgreed")), dct(result.get("Step0"))
    owner = orc.get("OwnerDecisionRequired")
    arrays = (("TopicVerdicts", tv, "Topic", TOPICS), ("QuestionAnswers", qa, "Question", QUESTIONS),
              ("Materiality.Triggers", trig, "Trigger", TRIGGERS), ("RelatedImpacts.FailClosedProposals", fcp, "Proposal", FCS))
    singles = (("Materiality", mat), ("OwnerReservedConsequence", orc), ("RelatedImpacts.ContractExclusionD1", d1))
    impacts = [("RelatedImpacts.ContractExclusionD1", d1)] + [("RelatedImpacts.FailClosedProposals[%s]" % x.get("Proposal"), x) for x in fcp]

    def linked(x):
        return x.get("FindingIds") or []

    def has_req(x):
        return any(i in req_ids for i in linked(x))

    def concerns(x):
        return any(x.get(k) is True for k in FC_FLAGS)
    if len(set(req_ids + opt_ids)) != len(req_ids + opt_ids):
        c.append("identificadores de hallazgo repetidos")
    for f in req:
        if not str(f.get("ViolatedInvariant", "")).strip() and not str(f.get("Counterexample", "")).strip():
            c.append("%s REQUIRED sin ViolatedInvariant ni Counterexample" % f.get("FindingId"))
    holders = [("%s[%s]" % (label, x.get(key)), x) for label, xs, key, _ in arrays for x in xs] + list(singles)
    for label, x in holders:
        if any(i not in all_ids for i in linked(x)):
            c.append("%s cita en FindingIds hallazgos que no existen" % label)
    for label, xs, key, _ in arrays:
        ids_ = [str(x.get(key)) for x in xs]
        if len(set(ids_)) != len(ids_):
            c.append("%s con identificadores repetidos" % label)
    for x in tv:
        if x.get("Status") == "DOES_NOT_CONFORM" and not has_req(x):
            c.append("TopicVerdicts[%s] DOES_NOT_CONFORM sin un REQUIRED vinculado (tema obligatorio de decisiones §93, punto 3)" % x.get("Topic"))
        if x.get("Status") == "CONFORMS" and has_req(x):
            c.append("TopicVerdicts[%s] CONFORMS con un REQUIRED vinculado" % x.get("Topic"))
    for x in trig:
        if x.get("Assessment") == "DISAGREE" and not has_req(x):
            c.append("Materiality.Triggers[%s] DISAGREE sin un REQUIRED vinculado" % x.get("Trigger"))
        if x.get("Assessment") == "AGREE" and has_req(x):
            c.append("Materiality.Triggers[%s] AGREE con un REQUIRED vinculado" % x.get("Trigger"))
    for x in qa:
        if x.get("Outcome") == "NOT_SATISFIED" and not linked(x):
            c.append("QuestionAnswers[%s] NOT_SATISFIED sin un hallazgo vinculado" % x.get("Question"))
    if owner is True and not orc.get("UnidentifiedConsequences"):
        c.append("OwnerDecisionRequired = true sin UnidentifiedConsequences")
    if owner is False and orc.get("UnidentifiedConsequences"):
        c.append("OwnerDecisionRequired = false con UnidentifiedConsequences")
    for label, x in impacts:
        if x.get("RevealsA7Defect") is True and not has_req(x):
            c.append("%s RevealsA7Defect = true sin un REQUIRED vinculado" % label)
        if x.get("RevealsA7Defect") is False and has_req(x):
            c.append("%s RevealsA7Defect = false con un REQUIRED vinculado (un impacto relacionado no condiciona el veredicto de A-7)" % label)
    if d1.get("Status") == "CONCERNS_FOR_COORDINATOR" and not d1.get("Concerns"):
        c.append("RelatedImpacts.ContractExclusionD1 CONCERNS_FOR_COORDINATOR sin Concerns")
    if d1.get("Status") == "NO_CONCERN" and d1.get("Concerns"):
        c.append("RelatedImpacts.ContractExclusionD1 NO_CONCERN con Concerns")
    for x in fcp:
        if x.get("Status") == "NO_CONCERN" and concerns(x):
            c.append("RelatedImpacts.FailClosedProposals[%s] NO_CONCERN con %s en true" % (x.get("Proposal"), "/".join(FC_FLAGS)))
        if x.get("Status") == "CONCERNS_FOR_COORDINATOR" and not concerns(x):
            c.append("RelatedImpacts.FailClosedProposals[%s] CONCERNS_FOR_COORDINATOR sin ninguna de %s en true" % (x.get("Proposal"), "/".join(FC_FLAGS)))
    if v != "NOT_ACCREDITED":
        if any(step0.get(k) is not True for k in STEP0_KEYS):
            c.append("con un veredicto, las siete booleanas de Step0 van en true")
        if result.get("ReviewedCommit") != REV or result.get("ReviewedPath") != OBJECT or result.get("ReviewedBlob") != OBJECT_BLOB:
            c.append("objeto revisado distinto del designado (commit, ruta o blob)")
        for label, xs, key, want in arrays:
            got_ = [str(x.get(key)) for x in xs]
            if sorted(got_) != sorted(want):
                c.append("%s debe disponer cada id una vez (faltan %s; sobran o repetidos %s)"
                         % (label, [w for w in want if w not in got_], sorted({g for g in got_ if g not in want or got_.count(g) > 1})))
        wrong_decl = [str(x.get("Trigger")) for x in trig if x.get("Trigger") in MAT_A7 and x.get("A7Declared") != MAT_A7[x.get("Trigger")]]
        if wrong_decl:
            c.append("con un veredicto, A7Declared distinto de lo que declara A-7 §5 (%s)" % ", ".join(wrong_decl))
        for label, x in holders:
            if not x.get("ClauseRefs"):
                c.append("con un veredicto, %s sin ClauseRefs" % label)
        undetermined = [k for k, bad in (("RelatedImpacts.ContractExclusionD1.Status", d1.get("Status") == NOT_DET),
                                         ("RelatedImpacts.ContractExclusionD1.RevealsA7Defect", d1.get("RevealsA7Defect") is None)) if bad]
        if undetermined:
            c.append("con un veredicto, NOT_DETERMINED o null no se admiten (%s)" % ", ".join(undetermined))
        if owner is None:
            c.append("con un veredicto, OwnerReservedConsequence.OwnerDecisionRequired no admite null")
        cited = set()
        for _, x in holders:
            cited |= set(linked(x))
        uncited = [i for i in req_ids if i not in cited]
        if uncited:
            c.append("REQUIRED sin disposición: todo REQUIRED lo cita al menos un elemento evaluado (%s)" % ", ".join(str(i) for i in uncited))
    if v == "AGREED" and (req or owner is not False or not ia or not all(ia.get(k) is True for k in ia)):
        c.append("AGREED exige cero REQUIRED, ninguna decisión del Owner pendiente e IfAgreed todo en true")
    if v == "AGREED" and (len(tv) != len(TOPICS) or any(x.get("Status") != "CONFORMS" for x in tv)):
        c.append("AGREED exige los once temas de TopicVerdicts en CONFORMS")
    if v == "AGREED" and (len(trig) != len(TRIGGERS) or any(x.get("Assessment") != "AGREE" for x in trig)):
        c.append("AGREED exige los ocho disparadores de Materiality en AGREE")
    if v == "AGREED" and (len(fcp) != len(FCS) or any(x.get("RevealsA7Defect") is not False for _, x in impacts)):
        c.append("AGREED exige RevealsA7Defect = false en RelatedImpacts (ContractExclusionD1 y las ocho propuestas)")
    if v != "AGREED" and any(x is not None for x in ia.values()):
        c.append("IfAgreed debe ir en null salvo con AGREED")
    if v in ("CHANGES REQUIRED", "REJECTED") and not req:
        c.append("%s sin ningún REQUIRED" % v)
    if v == BLOCKED and owner is not True:
        c.append("BLOCKED — OWNER DECISION sin OwnerDecisionRequired = true")
    if v == "NOT_ACCREDITED" and not result.get("KnownLimitations"):
        c.append("NOT_ACCREDITED sin el motivo en KnownLimitations")
    if any(result.get(k) != IDS[k] for k in ("RunId", "InvocationId", "LogicalReviewRequestId")):
        c.append("identificadores de la invocación distintos de los del cierre")


'''
for old, new in ((OLD_STEP0, NEW_STEP0), (OLD_ARR, NEW_ARR)):
    assert s.count(old) == 1, old[:40]
start = s.index("def collect_refs(result):\n")
end = s.index('if __name__ == "__main__":\n')
s = s[:start] + NEW_FUNCS + s[end:]
for old, new in ((OLD_STEP0, NEW_STEP0), (OLD_ARR, NEW_ARR)):
    s = s.replace(old, new)
open(p, "w", encoding="utf-8", newline="\n").write(s)
print("ok")
