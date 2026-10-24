"""Genera kit/result.schema.json (rackcad-a7r3-architect-review/0) para la ÚNICA revisión formal de la RONDA 3 de la enmienda A-7, acotada a
A62-A7R2-R1 y a sus consecuencias directas (decisiones §96, puntos 2 y 6).

Derivado de prep/p2/gen_schema_r2.py del kit de la ronda 2 (R20261019T201030Z-d1bf): las mismas palabras clave ya medidas con claude-cli (type,
additionalProperties, required, properties, enum, pattern, minLength, minItems, items; sin $ref, const, anyOf, maxLength ni maxItems) y los mismos
campos de identidad, DirectReads, Reads, Materiality, OwnerReservedConsequence, QuestionAnswers, IfAgreed, KnownLimitations y RecommendedNextAction.
Cambios de la ronda 3:
  * referencias (ClauseRefs, Evidence y DirectReads.Ref): decisiones §96, punto 6, «exigir referencias relativas válidas, presentes en el cierre»: el
    patrón ya no admite cualquier ruta bien formada (en la ronda 2 el revisor citó docs/initiatives/I-62-A7.md, relativa y bien formada pero
    inexistente; el auditor la mostró en forma absoluta), sino solo los documentos del cierre por su nombre exacto y los subárboles de evidencia del
    cierre, más order.txt y a7-r2.md, siempre relativos, con `/` y sin ningún segmento que empiece por punto (ni `.` ni `..`); la validación de
    StructuredOutput la rechaza al generar. La presencia de cada ruta en el cierre, la exactitud de la ruta escrita y las líneas entregadas
    fielmente las sigue comprobando el auditor contra el archivo real;
  * PriorFindingResolution: A62-A7R2-R1 (el REQUIRED de la ronda 2, decisiones §96, punto 1), CORRECTED | PARTIALLY_CORRECTED | NOT_CORRECTED;
  * DeltaPurity: los cambios de la ronda 3 (A-7 §3.7), WITHIN_SCOPE | OUTSIDE_SCOPE;
  * RegressionCheck: lo que la ronda 2 encontró corregido o conforme (R1 en lo corregido, R2, R3, T04, T05, T06, T07, T09);
  * CompatibilityVerdicts: dictamen expreso de la compatibilidad con A-5 y con A-6 (decisiones §96, punto 6), COMPATIBLE | INCOMPATIBLE |
    NOT_DETERMINED;
  * QuestionAnswers: Q-A7R3-01..03 (A-7 §8, preguntas de la ronda 3);
  * sin OptionalDispositionConformance ni RelatedImpacts (fuera del alcance de §96).
Veredictos: AGREED, CHANGES REQUIRED, REJECTED, BLOCKED — OWNER DECISION (LIFECYCLE §5) y NOT_ACCREDITED (la parada con fallo cerrado del Paso 0).
La unicidad, la cobertura exacta de ids y la coherencia las comprueba el auditor (post-review.py v7.0-a7r3).
Uso: python -I -B gen_schema_r3.py <kit dir>
"""
import json
import os
import sys

SEG = r"[A-Za-z0-9_@+-][A-Za-z0-9_.@+-]*"   # un segmento nunca empieza por punto: ni `.` ni `..` (decisiones §96, punto 6)
REF_PATH = (r"(?:docs/initiatives/I-62-(?:A-[1-7]|proposal-v14|consensus-freeze)\.md"
            r"|docs/(?:AUTOMATION_PLAN|INITIATIVE_LIFECYCLE)\.md"
            r"|docs/adr/0046-protocolo-de-ejecucion-delegada-de-agentes\.md"
            r"|docs/adr/0048-ejecucion-delegada-portable-roles-binding-y-autoverificacion\.md"
            r"|docs/automation/decisions/I-62\.md"
            r"|docs/automation/evidence/I-62-evidence\.md"
            r"|docs/automation/agent-execution/" + SEG + r"(?:/" + SEG + r")*"
            r"|docs/automation/evidence/I-62-(?:A6|A7|F6|architect-A-7)/" + SEG + r"(?:/" + SEG + r")*"
            r"|order\.txt|a7-r2\.md)")
REF = {"type": "string", "pattern": "^" + REF_PATH + ":[1-9][0-9]*(-[1-9][0-9]*)?$"}
TXT = {"type": "string", "minLength": 1}
STR = {"type": "string"}
NB = {"type": ["boolean", "null"]}
YN = {"enum": ["yes", "no"]}
HEX40N = {"type": ["string", "null"], "pattern": "^[0-9a-f]{40}$"}
FID = {"type": "string", "pattern": "^A62-A7R3-(R|O)[1-9][0-9]*$"}
PRIOR = ["A62-A7R2-R1"]
CHANGES = ["R2R1_REBASE_ROLE_REVERIFICATION_ONLY", "R2R1_REBASE_PATH_AND_RUNID", "R2R1_REBASE_COHERENCE", "R2R1_MAINSHA_EVAL_FROM_REBASEMAP",
           "R2R1_ABSENCE_AND_NON_ADMISSION", "S33_ROW_C_AND_S34_C6_NOTE", "DESCRIPTIVE_SECTIONS", "PROCEDURE_PIECES_ROUND3"]
REGRESSION = ["R1_TRX_AND_RELAYFACTS_AS_CORRECTED", "R2_INVALIDATOR_RULE_AND_PROCEDURE", "R3_A62_RULE1_PRECISION", "T04_EFF_EFF1_B1_MAP_VALIDATION",
              "T05_MAP_INVALID_ABSENT_NONCONSTRUCTIBLE", "T06_CLAUSEMAP_COVERAGE_RULE", "T07_MULTI_CATEGORY_ENTRIES", "T09_NO_UNAUTHORIZED_CHANGES"]
TRIGGERS = ["M-0%d" % i for i in range(1, 9)]
QUESTIONS = ["Q-A7R3-01", "Q-A7R3-02", "Q-A7R3-03"]
STEP0 = ["AP167DirectReadL593L595", "OrderReadWhole", "Decisions96DeclaresOrderBytesAndSha256", "Decisions96AcceptsR2R1AndAuthorizesThirdCandidate",
         "GuardsRunModeBaseHeadResult", "PreflightSummaryCandidateAndTotals", "Round2OutputIdentifiesRunAndFindings"]
IFAGREED = ["ScopeLimitedToR2R1AndConsequences", "PriorFindingCorrected", "DeltaLimitedToR2R1", "NoRegressionOfPriorConformities",
            "CompatibleWithA5AndA6", "MaterialityConfirmed", "NoUnidentifiedOwnerReservedConsequence", "NoUnauthorizedChanges",
            "SuitableForCoordinatorAgreement"]


def obj(props, optional=()):
    return {"type": "object", "additionalProperties": False, "required": [k for k in props if k not in optional], "properties": props}


def arr(items, **kw):
    return dict({"type": "array", "items": items}, **kw)


REFS, REFS1, FIDS, STRS = arr(REF), arr(REF, minItems=1), arr(FID), arr(STR)
COMPAT = obj({"Status": {"enum": ["COMPATIBLE", "INCOMPATIBLE", "NOT_DETERMINED"]}, "Rationale": TXT, "ClauseRefs": REFS, "FindingIds": FIDS})

schema = obj({
    "Schema": {"enum": ["rackcad-a7r3-architect-review/0"]},
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
    "RequiredFindings": arr(obj({"FindingId": {"type": "string", "pattern": "^A62-A7R3-R[1-9][0-9]*$"}, "Section": TXT, "ViolatedInvariant": STR,
                                 "Counterexample": STR, "Evidence": REFS1, "WhyItMatters": TXT, "CorrectionRequired": TXT})),
    "OptionalFindings": arr(obj({"FindingId": {"type": "string", "pattern": "^A62-A7R3-O[1-9][0-9]*$"}, "Section": TXT, "Note": TXT,
                                 "Evidence": REFS1, "SuggestedCorrection": STR})),
    "PriorFindingResolution": arr(obj({"PriorFinding": {"enum": PRIOR}, "Status": {"enum": ["CORRECTED", "PARTIALLY_CORRECTED", "NOT_CORRECTED"]},
                                       "Rationale": TXT, "ClauseRefs": REFS, "FindingIds": FIDS})),
    "DeltaPurity": obj({
        "Status": {"enum": ["CONFIRMED", "NOT_CONFIRMED", "NOT_DETERMINED"]},
        "Changes": arr(obj({"Change": {"enum": CHANGES}, "Assessment": {"enum": ["WITHIN_SCOPE", "OUTSIDE_SCOPE"]}, "Rationale": TXT,
                            "ClauseRefs": REFS, "FindingIds": FIDS})),
        "Rationale": TXT, "ClauseRefs": REFS, "FindingIds": FIDS}),
    "RegressionCheck": arr(obj({"Topic": {"enum": REGRESSION}, "Status": {"enum": ["NO_REGRESSION", "REGRESSION"]}, "Rationale": TXT,
                                "ClauseRefs": REFS, "FindingIds": FIDS})),
    "CompatibilityVerdicts": obj({"A5": COMPAT, "A6": COMPAT}),
    "Materiality": obj({
        "Triggers": arr(obj({"Trigger": {"enum": TRIGGERS}, "A7Declared": YN, "Assessment": {"enum": ["AGREE", "DISAGREE"]}, "Rationale": TXT,
                             "ClauseRefs": REFS, "FindingIds": FIDS})),
        "Rationale": TXT, "ClauseRefs": REFS, "FindingIds": FIDS}),
    "OwnerReservedConsequence": obj({"OwnerDecisionRequired": NB, "UnidentifiedConsequences": STRS, "Rationale": TXT, "ClauseRefs": REFS,
                                     "FindingIds": FIDS}),
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
                      "RefPatternChars": len(REF["pattern"]),
                      "Keywords": sorted({k for k in ("type", "additionalProperties", "required", "properties", "enum", "pattern", "minLength",
                                                      "minItems", "items") if ('"%s"' % k) in compact})}))
