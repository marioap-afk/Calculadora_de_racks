"""Genera kit/result.schema.json (rackcad-a7-architect-review/0) para la ÚNICA revisión formal de la enmienda A-7, acotada a A7-1 y a sus
consecuencias sobre el cierre de VERIFY de FX-02/F6 y la medición de A6-7 (decisiones §93).

Derivado de prep/gen_schema.py del kit acreditado de A-6 ronda 4 (rackcad-a6r4-architect-review/0, R20261014T180556Z-5e02): las mismas palabras
clave ya medidas con claude-cli (type, additionalProperties, required, properties, enum, pattern, minLength, minItems, items; sin $ref, const,
anyOf, maxLength ni maxItems) y los mismos campos de identidad, Step0 (siete booleanas, ahora las del Paso 0 de A-7), DirectReads, Reads,
hallazgos (forma del repositorio; ids A62-A7-R<n> / A62-A7-O<n>, como A62-A5-R<n> y A62-A2-R<n> en las primeras rondas), QuestionAnswers,
IfAgreed, KnownLimitations y RecommendedNextAction. En lugar de los campos de la ronda 4 de A-6, uno por exigencia de decisiones §93:
  * TopicVerdicts: los once temas obligatorios de §93, punto 3 (CONFORMS | DOES_NOT_CONFORM);
  * Materiality: M-01..M-08 con lo que A-7 §5 declara (A7Declared yes | no) y el dictamen (AGREE | DISAGREE) (§93, punto 3: «Dictaminar
    expresamente la materialidad»);
  * OwnerReservedConsequence: si existe alguna consecuencia OWNER-RESERVED no identificada (OwnerDecisionRequired y la lista) (§93, punto 3);
  * RelatedImpacts, campos SEPARADOS que no condicionan el veredicto de A-7 salvo que revelen un defecto de A-7 (RevealsA7Defect): la exclusión
    de la orden `contract` (D-1, §93, punto 4) y las propuestas de fallo cerrado FC-01, FC-04, FC-10, FC-12, FC-13, FC-16, FC-24 y FC-18 (§93,
    punto 5: si cambian los criterios de PASS, si excluyen casos válidos necesarios para C-24 o si se apoyan en la mera coincidencia con la
    implementación);
  * QuestionAnswers: Q-A7-01..06 (A-7 §8).
Veredictos: AGREED, CHANGES REQUIRED, REJECTED, BLOCKED — OWNER DECISION (LIFECYCLE §5) y NOT_ACCREDITED (la parada con fallo cerrado del Paso 0).
La unicidad, la cobertura exacta de ids y la coherencia las comprueba el auditor (post-review.py v5.5-a7).
Uso: python -I -B gen_schema.py <kit dir>
"""
import json
import os
import sys

REF = {"type": "string", "pattern": "^[A-Za-z0-9_.-]+(/[A-Za-z0-9_.-]+)*:[1-9][0-9]*(-[1-9][0-9]*)?$"}
TXT = {"type": "string", "minLength": 1}
STR = {"type": "string"}
NB = {"type": ["boolean", "null"]}
BOOL = {"type": "boolean"}
YN = {"enum": ["yes", "no"]}
HEX40N = {"type": ["string", "null"], "pattern": "^[0-9a-f]{40}$"}
FID = {"type": "string", "pattern": "^A62-A7-(R|O)[1-9][0-9]*$"}
TOPICS = ["T01_A71_VS_AUTHORITIES", "T02_CATEGORIES", "T03_PROVENANCE_REVISION_IDENTITY_FIDELITY", "T04_EFF_EFF1_B1_MAP_VALIDATION",
          "T05_MAP_INVALID_ABSENT_NONCONSTRUCTIBLE", "T06_CLAUSEMAP_COVERAGE_RULE", "T07_MULTI_CATEGORY_ENTRIES", "T08_COMPATIBILITY_A5_A6",
          "T09_NO_UNAUTHORIZED_CHANGES", "T10_APPLICABILITY_N4_NC1_N5_N7", "T11_NEGATIVES_AND_PREFLIGHT"]
TRIGGERS = ["M-0%d" % i for i in range(1, 9)]
FC_IDS = ["FC-01", "FC-04", "FC-10", "FC-12", "FC-13", "FC-16", "FC-24", "FC-18"]
QUESTIONS = ["Q-A7-0%d" % i for i in range(1, 7)]
STEP0 = ["V14DirectReadB9L2231L2233", "OrderReadWhole", "Decisions93DeclaresOrderBytesAndSha256", "Decisions93IdentifiesObjectAndAuthorizesReview",
         "Decisions90RecordsA6AgreedBlob", "GuardsRunModeBaseHeadResult", "PreflightSummaryCandidateAndTotals"]
IFAGREED = ["ScopeLimitedToA71AndConsequences", "A71CompatibleWithAuthorities", "AllMandatoryTopicsConform", "MaterialityConfirmed",
            "NoUnidentifiedOwnerReservedConsequence", "NoUnauthorizedChanges", "RelatedImpactsRevealNoA7Defect", "SuitableForCoordinatorAgreement"]
CONCERN = {"enum": ["NO_CONCERN", "CONCERNS_FOR_COORDINATOR"]}


def obj(props):
    return {"type": "object", "additionalProperties": False, "required": list(props), "properties": props}


def arr(items, **kw):
    return dict({"type": "array", "items": items}, **kw)


REFS, REFS1, FIDS, STRS = arr(REF), arr(REF, minItems=1), arr(FID), arr(STR)

schema = obj({
    "Schema": {"enum": ["rackcad-a7-architect-review/0"]},
    "RunId": {"type": "string", "pattern": "^R[0-9]{8}T[0-9]{6}Z-[0-9a-f]{4}$"},
    "InvocationId": {"type": "string", "pattern": "^I[0-9]{8}T[0-9]{6}Z-[0-9a-f]{4}$"},
    "LogicalReviewRequestId": {"type": "string", "pattern": "^L[0-9]{8}T[0-9]{6}Z-[0-9a-f]{4}$"},
    "AttemptSeq": {"type": "integer", "enum": [1]},
    "RequestedRole": {"enum": ["ARCHITECT"]},
    "Action": {"enum": ["REVIEW_DESIGN"]},
    "ReviewedUnit": {"enum": ["I-62"]},
    "ReviewedCommit": HEX40N,
    "ReviewedPath": {"enum": ["docs/initiatives/I-62-A-7.md"]},
    "ReviewedBlob": HEX40N,
    "ReviewerMode": {"enum": ["SAME-SESSION ROLE", "SEPARATE SESSION", "EXTERNAL HUMAN"]},
    "SamePersonAsAuthor": TXT,
    "ReviewerDeclaredIdentity": obj({"Runtime": STR, "Model": STR, "Effort": STR, "SessionOrThread": STR}),
    "InjectedContextDeclaration": obj({"State": {"enum": ["DECLARED", "NONE"]}, "Items": STRS}),
    "Step0": obj(dict([("Basis", {"enum": ["INVOKER_VERIFIED_CLONE"]})] + [(k, NB) for k in STEP0] + [("Note", STR)])),
    "DirectReads": arr(obj({"Ref": REF, "Purpose": TXT})),
    "Reads": STRS,
    "Verdict": {"enum": ["AGREED", "CHANGES REQUIRED", "REJECTED", "BLOCKED — OWNER DECISION", "NOT_ACCREDITED"]},
    "RequiredFindings": arr(obj({"FindingId": {"type": "string", "pattern": "^A62-A7-R[1-9][0-9]*$"}, "Section": TXT, "ViolatedInvariant": STR,
                                 "Counterexample": STR, "Evidence": REFS1, "WhyItMatters": TXT, "CorrectionRequired": TXT})),
    "OptionalFindings": arr(obj({"FindingId": {"type": "string", "pattern": "^A62-A7-O[1-9][0-9]*$"}, "Section": TXT, "Note": TXT,
                                 "Evidence": REFS1, "SuggestedCorrection": STR})),
    "TopicVerdicts": arr(obj({"Topic": {"enum": TOPICS}, "Status": {"enum": ["CONFORMS", "DOES_NOT_CONFORM"]}, "Rationale": TXT,
                              "ClauseRefs": REFS, "FindingIds": FIDS})),
    "Materiality": obj({
        "Triggers": arr(obj({"Trigger": {"enum": TRIGGERS}, "A7Declared": YN, "Assessment": {"enum": ["AGREE", "DISAGREE"]}, "Rationale": TXT,
                             "ClauseRefs": REFS, "FindingIds": FIDS})),
        "Rationale": TXT, "ClauseRefs": REFS, "FindingIds": FIDS}),
    "OwnerReservedConsequence": obj({"OwnerDecisionRequired": NB, "UnidentifiedConsequences": STRS, "Rationale": TXT, "ClauseRefs": REFS,
                                     "FindingIds": FIDS}),
    "RelatedImpacts": obj({
        "ContractExclusionD1": obj({"Status": {"enum": ["NO_CONCERN", "CONCERNS_FOR_COORDINATOR", "NOT_DETERMINED"]}, "RevealsA7Defect": NB,
                                    "Concerns": STRS, "Rationale": TXT, "ClauseRefs": REFS, "FindingIds": FIDS}),
        "FailClosedProposals": arr(obj({"Proposal": {"enum": FC_IDS}, "ChangesPassCriteria": BOOL, "ExcludesValidC24Cases": BOOL,
                                        "ReliesOnImplementationCoincidence": BOOL, "RevealsA7Defect": BOOL, "Status": CONCERN, "Rationale": TXT,
                                        "ClauseRefs": REFS, "FindingIds": FIDS})),
        "Note": STR}),
    "QuestionAnswers": arr(obj({"Question": {"enum": QUESTIONS}, "Outcome": {"enum": ["SATISFIED", "NOT_SATISFIED"]}, "Answer": TXT,
                                "ClauseRefs": REFS, "FindingIds": FIDS})),
    "IfAgreed": obj({k: NB for k in IFAGREED}),
    "KnownLimitations": STRS,
    "RecommendedNextAction": STR,
})

if __name__ == "__main__":
    out = os.path.join(sys.argv[1], "result.schema.json")
    with open(out, "w", encoding="utf-8", newline="\n") as f:
        f.write(json.dumps(schema, ensure_ascii=False, indent=1) + "\n")
    compact = json.dumps(schema, separators=(",", ":"), ensure_ascii=True)
    print(json.dumps({"Written": "kit/result.schema.json", "CompactChars": len(compact), "Ascii": all(ord(c) < 128 for c in compact),
                      "Keywords": sorted({k for k in ("type", "additionalProperties", "required", "properties", "enum", "pattern", "minLength",
                                                      "minItems", "items") if ('"%s"' % k) in compact})}))
