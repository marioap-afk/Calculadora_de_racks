"""Genera kit/result.schema.json (rackcad-a7r2-architect-review/0) para la ÚNICA revisión formal de la RONDA 2 de la enmienda A-7, acotada a
A62-A7-R1..R3 y a sus consecuencias (decisiones §94, punto 6).

Derivado de prep/gen_schema.py del kit de A-7 ronda 1 (rackcad-a7-architect-review/0, R20261019T010200Z-4a72): las mismas palabras clave ya
medidas con claude-cli (type, additionalProperties, required, properties, enum, pattern, minLength, minItems, items; sin $ref, const, anyOf,
maxLength ni maxItems) y los mismos campos de identidad, Step0 (siete booleanas, ahora las del Paso 0 de la ronda 2), DirectReads, Reads,
hallazgos (ids nuevos A62-A7R2-R<n> / A62-A7R2-O<n>, como A62-A6R4-R<n> en la ronda 4 de A-6), Materiality, OwnerReservedConsequence,
QuestionAnswers, IfAgreed, KnownLimitations y RecommendedNextAction. Campos de la ronda 2 (los de las rondas de A-6 para una ronda posterior):
  * Round1FindingResolution: A62-A7-R1, R2 y R3, cada uno CORRECTED | PARTIALLY_CORRECTED | NOT_CORRECTED (los dos últimos con el REQUIRED
    residual vinculado);
  * DeltaPurity: los cambios normativos solo por R1-R3; cada cambio de la ronda 2 (R1, R2, R3, los descriptivos O1, O7 y O2 y las cuatro
    precisiones posteriores: RED según 16.8, la medición sin hechos del relevo, la implementación de la derivación como pieza del invalidador, la
    exigencia de RED indeterminada) WITHIN_R1_R3 | OUTSIDE_R1_R3;
  * RegressionCheck: los temas que la ronda 1 encontró CONFORMS (T04, T05, T06, T07, T09), cada uno NO_REGRESSION | REGRESSION;
  * OptionalDispositionConformance (OPCIONAL): las disposiciones propuestas de A62-A7-O1..O8, CONFORMS | DOES_NOT_CONFORM | NOT_ASSESSED;
  * RelatedImpacts: solo si R1-R3 cambian D-1 o las propuestas FC evaluadas en la ronda 1 (ChangedByRound2 y Note);
  * QuestionAnswers: Q-A7R2-01..03 (A-7 §8, preguntas de la ronda 2).
Veredictos: AGREED, CHANGES REQUIRED, REJECTED, BLOCKED — OWNER DECISION (LIFECYCLE §5) y NOT_ACCREDITED (la parada con fallo cerrado del Paso 0).
La unicidad, la cobertura exacta de ids y la coherencia las comprueba el auditor (post-review.py v6.0-a7r2).
Uso: python -I -B gen_schema_r2.py <kit dir>
"""
import json
import os
import sys

REF = {"type": "string", "pattern": "^[A-Za-z0-9_.-]+(/[A-Za-z0-9_.-]+)*:[1-9][0-9]*(-[1-9][0-9]*)?$"}
TXT = {"type": "string", "minLength": 1}
STR = {"type": "string"}
NB = {"type": ["boolean", "null"]}
YN = {"enum": ["yes", "no"]}
HEX40N = {"type": ["string", "null"], "pattern": "^[0-9a-f]{40}$"}
FID = {"type": "string", "pattern": "^A62-A7R2-(R|O)[1-9][0-9]*$"}
ROUND1 = ["A62-A7-R1", "A62-A7-R2", "A62-A7-R3"]
OPTIONALS = ["A62-A7-O%d" % i for i in range(1, 9)]
CHANGES = ["R1_CATEGORY_C_ROLES", "R2_INVALIDATOR_RULE_AND_PROCEDURE", "R3_A62_RULE1_PRECISION", "O1_M04_MOTIVE", "O7_S31_MV6_MV7_CONTENT",
           "O2_S35_REVERIFICATION", "P1_RED_PER_16_8", "P2_MEASUREMENT_WITHOUT_RELAY_FACTS", "P3_DERIVATION_AS_INVALIDATOR_PIECE",
           "P4_RED_UNDETERMINED"]
REGRESSION = ["T04_EFF_EFF1_B1_MAP_VALIDATION", "T05_MAP_INVALID_ABSENT_NONCONSTRUCTIBLE", "T06_CLAUSEMAP_COVERAGE_RULE",
              "T07_MULTI_CATEGORY_ENTRIES", "T09_NO_UNAUTHORIZED_CHANGES"]
TRIGGERS = ["M-0%d" % i for i in range(1, 9)]
QUESTIONS = ["Q-A7R2-01", "Q-A7R2-02", "Q-A7R2-03"]
STEP0 = ["AP168DirectReadL616L617", "OrderReadWhole", "Decisions94DeclaresOrderBytesAndSha256", "Decisions94AcceptsR1R3AndAuthorizesSecondCandidate",
         "GuardsRunModeBaseHeadResult", "PreflightSummaryCandidateAndTotals", "Round1OutputIdentifiesRunAndFindings"]
IFAGREED = ["ScopeLimitedToR1R3AndConsequences", "Round1FindingsCorrected", "DeltaLimitedToR1R3", "NoRegressionOfRound1Conformities",
            "MaterialityConfirmed", "NoUnidentifiedOwnerReservedConsequence", "NoUnauthorizedChanges", "RelatedImpactsUnchangedOrDeclared",
            "SuitableForCoordinatorAgreement"]


def obj(props, optional=()):
    return {"type": "object", "additionalProperties": False, "required": [k for k in props if k not in optional], "properties": props}


def arr(items, **kw):
    return dict({"type": "array", "items": items}, **kw)


REFS, REFS1, FIDS, STRS = arr(REF), arr(REF, minItems=1), arr(FID), arr(STR)

schema = obj({
    "Schema": {"enum": ["rackcad-a7r2-architect-review/0"]},
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
    "Verdict": {"enum": ["AGREED", "CHANGES REQUIRED", "REJECTED", "BLOCKED " + chr(0x2014) + " OWNER DECISION", "NOT_ACCREDITED"]},
    "RequiredFindings": arr(obj({"FindingId": {"type": "string", "pattern": "^A62-A7R2-R[1-9][0-9]*$"}, "Section": TXT, "ViolatedInvariant": STR,
                                 "Counterexample": STR, "Evidence": REFS1, "WhyItMatters": TXT, "CorrectionRequired": TXT})),
    "OptionalFindings": arr(obj({"FindingId": {"type": "string", "pattern": "^A62-A7R2-O[1-9][0-9]*$"}, "Section": TXT, "Note": TXT,
                                 "Evidence": REFS1, "SuggestedCorrection": STR})),
    "Round1FindingResolution": arr(obj({"Round1Finding": {"enum": ROUND1}, "Status": {"enum": ["CORRECTED", "PARTIALLY_CORRECTED", "NOT_CORRECTED"]},
                                        "Rationale": TXT, "ClauseRefs": REFS, "FindingIds": FIDS})),
    "DeltaPurity": obj({
        "Status": {"enum": ["CONFIRMED", "NOT_CONFIRMED", "NOT_DETERMINED"]},
        "Changes": arr(obj({"Change": {"enum": CHANGES}, "Assessment": {"enum": ["WITHIN_R1_R3", "OUTSIDE_R1_R3"]}, "Rationale": TXT,
                            "ClauseRefs": REFS, "FindingIds": FIDS})),
        "Rationale": TXT, "ClauseRefs": REFS, "FindingIds": FIDS}),
    "RegressionCheck": arr(obj({"Topic": {"enum": REGRESSION}, "Status": {"enum": ["NO_REGRESSION", "REGRESSION"]}, "Rationale": TXT,
                                "ClauseRefs": REFS, "FindingIds": FIDS})),
    "OptionalDispositionConformance": arr(obj({"Round1Optional": {"enum": OPTIONALS}, "Status": {"enum": ["CONFORMS", "DOES_NOT_CONFORM", "NOT_ASSESSED"]},
                                               "Rationale": TXT, "ClauseRefs": REFS, "FindingIds": FIDS})),
    "Materiality": obj({
        "Triggers": arr(obj({"Trigger": {"enum": TRIGGERS}, "A7Declared": YN, "Assessment": {"enum": ["AGREE", "DISAGREE"]}, "Rationale": TXT,
                             "ClauseRefs": REFS, "FindingIds": FIDS})),
        "Rationale": TXT, "ClauseRefs": REFS, "FindingIds": FIDS}),
    "OwnerReservedConsequence": obj({"OwnerDecisionRequired": NB, "UnidentifiedConsequences": STRS, "Rationale": TXT, "ClauseRefs": REFS,
                                     "FindingIds": FIDS}),
    "RelatedImpacts": obj({"ChangedByRound2": NB, "Note": STR, "ClauseRefs": REFS, "FindingIds": FIDS}),
    "QuestionAnswers": arr(obj({"Question": {"enum": QUESTIONS}, "Outcome": {"enum": ["SATISFIED", "NOT_SATISFIED"]}, "Answer": TXT,
                                "ClauseRefs": REFS, "FindingIds": FIDS})),
    "IfAgreed": obj({k: NB for k in IFAGREED}),
    "KnownLimitations": STRS,
    "RecommendedNextAction": STR,
}, optional=("OptionalDispositionConformance",))

if __name__ == "__main__":
    out = os.path.join(sys.argv[1], "result.schema.json")
    with open(out, "w", encoding="utf-8", newline="\n") as f:
        f.write(json.dumps(schema, ensure_ascii=False, indent=1) + "\n")
    compact = json.dumps(schema, separators=(",", ":"), ensure_ascii=True)
    print(json.dumps({"Written": "kit/result.schema.json", "CompactChars": len(compact), "Ascii": all(ord(c) < 128 for c in compact),
                      "Keywords": sorted({k for k in ("type", "additionalProperties", "required", "properties", "enum", "pattern", "minLength",
                                                      "minItems", "items") if ('"%s"' % k) in compact})}))
