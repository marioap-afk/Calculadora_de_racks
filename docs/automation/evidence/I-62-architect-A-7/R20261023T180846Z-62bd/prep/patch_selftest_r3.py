"""Deriva kit/selftest-post-review.py de la ronda 3 (decisiones §96) del de la ronda 2: docstring, constantes (r3/, la revisión de la ronda 2, §96 y
la §97 sintética, AUTOMATION_PLAN 16.7 L593-L595), lecturas del Paso 0, resultado base del esquema rackcad-a7r3-architect-review/0
(PriorFindingResolution, CompatibilityVerdicts A5/A6, sin OptionalDispositionConformance ni RelatedImpacts), los casos de coherencia y cobertura del
esquema de la ronda 3 (55-121j, con 69-73, 100b, 114h, 114i y 121i para la compatibilidad), los negativos del mecanismo de citas de §96, punto 6
(15, 93-93h, 109: rutas fuera del cierre, la ruta mal escrita de la ronda 2, `..`, barras invertidas, absolutas, otra grafía que el esquema admite y
la premisa con otra grafía o por su ruta absoluta; el mensaje nuevo con la referencia tal como se escribió y el motivo), F09 y X06 (el patrón de
referencias del esquema frente a las rutas del cierre). El resto de los casos (00-54, 126-153, G, O, L, X01-X05, F) no cambia salvo nombres,
textos de la ronda y la §96. Uso: python -I -B patch_selftest_r3.py <kit> <prefijo del SHA-256 de partida>
"""
import hashlib
import os
import sys

p = os.path.join(sys.argv[1], "selftest-post-review.py")
t = open(p, encoding="utf-8", newline="").read()
assert hashlib.sha256(t.encode("utf-8")).hexdigest().startswith(sys.argv[2]), "selftest-post-review.py de partida (el de la ronda 2)"

DOC = '''"""Self-test of post-review.py v7.0-a7r3, of transport_gate.py, of owner_gate.py (A-7 round 3), of pin_authorization.values and of
c3_analysis.analyze, run BEFORE the launch of the ONE formal review of ROUND 3 of amendment A-7, bounded to A62-A7R2-R1 and its consequences
(decisions §96, points 2, 3 and 6). Adapted from the self-test of the A-7 round-2 kit (R20261019T201030Z-d1bf, itself from round 1 with the
read-fidelity cases of phase 1): the same synthetic formats and the same transport, read, custody, launcher, gate and fidelity cases (00-55,
126-153, G00-G58, X01-X05, F00-F20), the specifics replaced by the round-3 closure (three run files besides the faithful copies: order.txt = the §96
body, a7-r2.md = the round-2 text, and prompt.md; the object is A-7 round 3; the direct read is AUTOMATION_PLAN 16.7 L593-L595) and the result
schema rackcad-a7r3-architect-review/0, with:
  * cases 56-121j for the coherence and coverage rules of the round-3 schema (PriorFindingResolution of A62-A7R2-R1, DeltaPurity with its
    Changes WITHIN_SCOPE | OUTSIDE_SCOPE, RegressionCheck of what round 2 found corrected or conforming, CompatibilityVerdicts A5 and A6 — the
    explicit verdict of decisions §96, point 6: 69-73, 81, 88, 89, 100b, 102, 111, 114h, 114i, 121i —, Materiality with its Triggers and
    A7Declared, OwnerReservedConsequence, QuestionAnswers, AGREED, CHANGES REQUIRED, REJECTED, BLOCKED, NOT_ACCREDITED, schema; 77-78: the round-2
    fields OptionalDispositionConformance and RelatedImpacts are rejected by the schema; 114b-114i are nested schema violations that only the
    auditor's in-process schema check detects, because the Test-Json of pwsh 7.6.7 validates only the first level of nesting), the direct read of
    AUTOMATION_PLAN L593-L595 and the references of the nested elements;
  * the reference mechanism of decisions §96, point 6 («Exigir referencias relativas válidas, presentes en el cierre y comprobadas contra el
    archivo real»): 15, 93-93h and 109 (a file outside the closure, the misspelled path of round 2 docs/initiatives/I-62-A7.md, `..`, backslashes,
    an absolute path into the clone, another spelling that the schema admits, a premise with another spelling or by its absolute path): the schema
    rejects what it can, and the auditor reports every one as non-canonical with the reference as written and why (outside the closure, lines out of
    range, or not the exact relative path); 93f shows the auditor's exactness rule on its own (schema valid); F09 a faithful copy cited; X06 the
    reference pattern of result.schema.json against every closure path, the premises and the negatives;
  * cases 150-151p for the preflight record of gate 11: the object blob and the agreed A-6 blob at the worktree HEAD and the authorization pinned in
    owner_gate.py; the kit is delivered with the authorization PENDING, so the audit cases use SYNTHETIC pins (a synthetic §97 appended to the
    clone's decisions, set on the auditor's module object, never on the file); case 151p checks that a kit with the authorization pending cannot be
    accredited;
  * cases O00-O30 (owner_gate.evaluate, pure, on decision texts built from the clone's decisions, which end at §96, plus a SYNTHETIC §97): the
    authorization pending (blocked with the single «authorization pending: …» reason), pinned on a section after §96 (passes), a later section
    (blocked), the object and agreed A-6 blobs and the inherited checks; O30 exercises pin_authorization.values; and the live probe L00
    (owner_gate.check on the real I-62 worktree, read-only git, with the module exactly as the kit holds it: pending, it must block with only the
    pending reason; pinned, it must pass with exactly the pinned section and no later section);
  * cases X01-X06: launch.py and the auditor agree (RUN_JSON_FIELDS, outcome_categories on a set of records, the frozen command, the environment
    prefixes and, X05, check 7 with the faithful copies: run_files_check); launch.py is read with ast, never imported (importing it would evaluate
    its frozen-command guard); X06, the reference pattern of the schema;
  * cases F00-F20 (read fidelity and faithful copies, decisions §94, point 5): flagged originals read on faithful lines (informational); the r1
    failure reproduced (the TRX original with its BOM silently dropped: DEGRADED, a reason); the TRX through its copy, cited by the original path
    (credited); the copy altered by one byte in the run, altered in delivery, absent, of another original, non-canonical, with the BOM dropped
    inside it, and the original's blob pin altered (each a reason, nothing credited); a long line through the copy with and without its appendix
    and truncated in the original (the TruncatedKnownLongLines rule); the direct read of AUTOMATION_PLAN through its copy; a copy cited as a
    reference; and, only with the fixture of a test clone (zz-fidelity-fixture/), bare CR, NUL, invalid UTF-8 and every fixture copy (skipped, with a
    PASS row that says so, when the closure has no fixture).
'''
a = t.index('"""Self-test of post-review.py v6.0-a7r2')
b = t.index("\nEnvironment. By default it audits against the kit's closure")
t = DOC + t[b + 1:]

COH_START = "    # ---- result schema and coherence (A-7 round 2: rackcad-a7r2-architect-review/0)\n"
COH_END = "    # ---- run.json of the launcher (the round-3 rules, decisions §84, point 4)\n"
COH = r'''    # ---- result schema and coherence (A-7 round 3: rackcad-a7r3-architect-review/0)
    def extra_field(res, r):
        res["Extra"] = 1
    add(variant("55-esquema-invalido", result_patch=extra_field, expected=["resultado: no valida contra result.schema.json"], schema_valid=False))

    def p(name, fn, expected=(), counts=None, schema_valid=True, coherent=False, run_hook=None, base=None, extra=None):
        add(variant(name, extra=extra, result_patch=lambda res, r: fn(res), expected=expected, counts=counts, schema_valid=schema_valid,
                    require_coherent=coherent, run_hook=run_hook, base=base))

    def cr(extra, verdict="CHANGES REQUIRED"):
        def f(res):
            changes_required(res, verdict)
            extra(res)
        return f

    def bl(extra):
        def f(res):
            blocked(res)
            extra(res)
        return f
    R, O, PF = "A62-A7R3-R1", "A62-A7R3-O1", "A62-A7R2-R1"
    DPC, CVA5, CVA6 = "DeltaPurity.Changes", "CompatibilityVerdicts.A5", "CompatibilityVerdicts.A6"
    SV = "resultado: no valida contra result.schema.json"
    NC = "no es un archivo del cierre ni una premisa del run"
    NE = "no es la ruta relativa exacta de un archivo del cierre ni el nombre exacto de una premisa del run"

    def link_elsewhere(res):
        """The REQUIRED disposed by a regression topic (so «REQUIRED sin disposición» does not fire)."""
        rgd(res)["T05_MAP_INVALID_ABSENT_NONCONSTRUCTIBLE"].update({"Status": "REGRESSION", "FindingIds": [R]})

    def open_unlinked(res):
        rrd(res)[PF]["FindingIds"] = []
        link_elsewhere(res)
    p("56-resolucion-abierta-sin-required", cr(open_unlinked), ["PriorFindingResolution[A62-A7R2-R1] PARTIALLY_CORRECTED sin el REQUIRED residual vinculado"])
    p("57-resolucion-corregida-con-required", cr(lambda res: rrd(res)[PF].update({"Status": "CORRECTED"})),
      ["PriorFindingResolution[A62-A7R2-R1] CORRECTED con un REQUIRED vinculado"])

    def not_corrected_unlinked(res):
        rrd(res)[PF].update({"Status": "NOT_CORRECTED", "FindingIds": []})
        link_elsewhere(res)
    p("58-resolucion-no-corregida-sin-required", cr(not_corrected_unlinked),
      ["PriorFindingResolution[A62-A7R2-R1] NOT_CORRECTED sin el REQUIRED residual vinculado"])
    p("59-regresion-sin-required", cr(lambda res: rgd(res)["T06_CLAUSEMAP_COVERAGE_RULE"].update({"Status": "REGRESSION"})),
      ["RegressionCheck[T06_CLAUSEMAP_COVERAGE_RULE] REGRESSION sin un REQUIRED vinculado"])
    p("60-sin-regresion-con-required", cr(lambda res: rgd(res)["R1_TRX_AND_RELAYFACTS_AS_CORRECTED"].update({"FindingIds": [R]})),
      ["RegressionCheck[R1_TRX_AND_RELAYFACTS_AS_CORRECTED] NO_REGRESSION con un REQUIRED vinculado"])
    p("61-cambio-fuera-sin-required", cr(lambda res: chd(res)["R2R1_REBASE_COHERENCE"].update({"Assessment": "OUTSIDE_SCOPE"})),
      [DPC + "[R2R1_REBASE_COHERENCE] OUTSIDE_SCOPE sin un REQUIRED vinculado", "DeltaPurity CONFIRMED con un cambio OUTSIDE_SCOPE"])
    p("62-cambio-dentro-con-required", cr(lambda res: chd(res)["DESCRIPTIVE_SECTIONS"].update({"FindingIds": [R]})),
      [DPC + "[DESCRIPTIVE_SECTIONS] WITHIN_SCOPE con un REQUIRED vinculado"])
    p("63-pureza-no-confirmada-sin-cambio-fuera", cr(lambda res: dp(res).update({"Status": "NOT_CONFIRMED", "FindingIds": [R]})),
      ["DeltaPurity NOT_CONFIRMED con todos los cambios WITHIN_SCOPE"])

    def nc_no_req(res):
        chd(res)["R2R1_MAINSHA_EVAL_FROM_REBASEMAP"].update({"Assessment": "OUTSIDE_SCOPE", "FindingIds": [R]})
        dp(res).update({"Status": "NOT_CONFIRMED"})
    p("64-pureza-no-confirmada-sin-required", cr(nc_no_req), ["DeltaPurity NOT_CONFIRMED sin un REQUIRED vinculado"])
    p("65-disparador-en-desacuerdo-sin-required", cr(lambda res: trd(res)["M-03"].update({"Assessment": "DISAGREE"})),
      ["Materiality.Triggers[M-03] DISAGREE sin un REQUIRED vinculado"])
    p("66-disparador-de-acuerdo-con-required", cr(lambda res: trd(res)["M-01"].update({"FindingIds": [R]})),
      ["Materiality.Triggers[M-01] AGREE con un REQUIRED vinculado"])
    p("67-declaracion-de-a7-mal-copiada", lambda res: trd(res)["M-08"].update({"A7Declared": "yes"}),
      ["con un veredicto, A7Declared distinto de lo que declara A-7 §5 (M-08)"])
    p("68-pregunta-no-satisfecha-sin-hallazgo", cr(lambda res: qad(res)["Q-A7R3-02"].update({"Outcome": "NOT_SATISFIED"})),
      ["QuestionAnswers[Q-A7R3-02] NOT_SATISFIED sin un hallazgo vinculado"])
    # the explicit verdict on A-5 and A-6 (decisions §96, point 6)
    p("69-compatibilidad-a5-incompatible-sin-required", cr(lambda res: cvd(res)["A5"].update({"Status": "INCOMPATIBLE"})),
      [CVA5 + " INCOMPATIBLE sin un REQUIRED vinculado"])
    p("70-compatibilidad-a6-compatible-con-required", cr(lambda res: cvd(res)["A6"].update({"FindingIds": [R]})),
      [CVA6 + " COMPATIBLE con un REQUIRED vinculado"])
    p("71-compatibilidad-incompatible-coherente-aceptada", cr(lambda res: cvd(res)["A6"].update({"Status": "INCOMPATIBLE", "FindingIds": [R]})),
      coherent=True)
    p("72-compatibilidad-sin-referencias", lambda res: cvd(res)["A5"].update({"ClauseRefs": []}), ["con un veredicto, " + CVA5 + " sin ClauseRefs"])
    p("73-compatibilidad-referencia-a-a5-leida-aceptada", lambda res: cvd(res)["A5"]["ClauseRefs"].append(ref(A5, 175, 199)), coherent=True,
      extra=lambda r: read_range(r, A5, 169, 221))
    p("74-decision-del-owner-sin-consecuencias", bl(lambda res: orc(res).update({"UnidentifiedConsequences": []})),
      ["OwnerDecisionRequired = true sin UnidentifiedConsequences"])
    p("75-sin-decision-del-owner-con-consecuencias", lambda res: orc(res).update({"UnidentifiedConsequences": ["x"]}),
      ["OwnerDecisionRequired = false con UnidentifiedConsequences"])
    p("76-blocked-sin-decision-del-owner", lambda res: not_agreed(res, BLOCKED), ["BLOCKED — OWNER DECISION sin OwnerDecisionRequired = true"])
    p("77-disposiciones-opcionales-de-la-ronda-2-rechazadas-por-el-esquema",
      lambda res: res.update({"OptionalDispositionConformance": [{"Round1Optional": "A62-A7R2-O1", "Status": "CONFORMS", "Rationale": "x",
                                                                   "ClauseRefs": [], "FindingIds": []}]}), [SV], schema_valid=False)
    p("78-impactos-relacionados-de-la-ronda-2-rechazados-por-el-esquema",
      lambda res: res.update({"RelatedImpacts": {"ChangedByRound2": False, "Note": "", "ClauseRefs": [], "FindingIds": []}}), [SV], schema_valid=False)
    p("79-ifagreed-no-nulo-sin-agreed", cr(lambda res: res["IfAgreed"].update({"SuitableForCoordinatorAgreement": True})),
      ["IfAgreed debe ir en null salvo con AGREED"])
    p("80-agreed-con-ifagreed-falso", lambda res: res["IfAgreed"].update({"MaterialityConfirmed": False}),
      ["AGREED exige cero REQUIRED, ninguna decisión del Owner pendiente e IfAgreed todo en true"])
    p("81-no-determinado-con-veredicto", cr(lambda res: (dp(res).update({"Status": "NOT_DETERMINED"}), cvd(res)["A6"].update({"Status": "NOT_DETERMINED"}))),
      ["con un veredicto, NOT_DETERMINED o null no se admiten (DeltaPurity.Status, CompatibilityVerdicts.A6.Status)"])
    p("82-decision-del-owner-nula-con-veredicto", cr(lambda res: orc(res).update({"OwnerDecisionRequired": None})),
      ["OwnerReservedConsequence.OwnerDecisionRequired no admite null"])
    p("83-paso0-falso-con-veredicto", lambda res: res["Step0"].update({"Decisions96DeclaresOrderBytesAndSha256": False}),
      ["las siete booleanas de Step0 van en true"])
    p("84-paso0-nulo-con-veredicto", lambda res: res["Step0"].update({"AP167DirectReadL593L595": None}), ["las siete booleanas de Step0 van en true"])
    p("85-objeto-otro-blob", lambda res: res.update({"ReviewedBlob": OTHER_A7_BLOB}), ["objeto revisado distinto del designado"])
    p("86-objeto-otro-commit", lambda res: res.update({"ReviewedCommit": PR.CLOSURE["Publication"]["Parent"]}), ["objeto revisado distinto del designado"])
    p("87-ids-de-invocacion-de-la-ronda-2", lambda res: res.update({"RunId": "R20261019T201030Z-d1bf"}), ["identificadores de la invocación distintos"])

    def no_refs(res):
        rrd(res)[PF]["ClauseRefs"] = []
        rgd(res)["T05_MAP_INVALID_ABSENT_NONCONSTRUCTIBLE"]["ClauseRefs"] = []
        chd(res)["R2R1_ABSENCE_AND_NON_ADMISSION"]["ClauseRefs"] = []
        qad(res)["Q-A7R3-03"]["ClauseRefs"] = []
        trd(res)["M-02"]["ClauseRefs"] = []
        for x in (mat(res), orc(res), dp(res), cvd(res)["A6"]):
            x["ClauseRefs"] = []
    p("88-elementos-sin-referencias", no_refs, ["con un veredicto, PriorFindingResolution[A62-A7R2-R1] sin ClauseRefs",
                                                "con un veredicto, RegressionCheck[T05_MAP_INVALID_ABSENT_NONCONSTRUCTIBLE] sin ClauseRefs",
                                                "con un veredicto, DeltaPurity.Changes[R2R1_ABSENCE_AND_NON_ADMISSION] sin ClauseRefs",
                                                "con un veredicto, QuestionAnswers[Q-A7R3-03] sin ClauseRefs",
                                                "con un veredicto, Materiality.Triggers[M-02] sin ClauseRefs",
                                                "con un veredicto, Materiality sin ClauseRefs",
                                                "con un veredicto, OwnerReservedConsequence sin ClauseRefs",
                                                "con un veredicto, DeltaPurity sin ClauseRefs",
                                                "con un veredicto, " + CVA6 + " sin ClauseRefs"])

    def fid_missing(res):
        rrd(res)[PF]["FindingIds"] = ["A62-A7R3-O7"]
        rgd(res)["T07_MULTI_CATEGORY_ENTRIES"]["FindingIds"] = ["A62-A7R3-O9"]
        chd(res)["PROCEDURE_PIECES_ROUND3"]["FindingIds"] = ["A62-A7R3-O8"]
        qad(res)["Q-A7R3-01"]["FindingIds"] = ["A62-A7R3-O6"]
        trd(res)["M-04"]["FindingIds"] = ["A62-A7R3-O5"]
        dp(res)["FindingIds"] = ["A62-A7R3-O4"]
        cvd(res)["A5"]["FindingIds"] = ["A62-A7R3-O3"]
    p("89-findingids-inexistentes", fid_missing, ["PriorFindingResolution[A62-A7R2-R1] cita en FindingIds hallazgos que no existen",
                                                  "RegressionCheck[T07_MULTI_CATEGORY_ENTRIES] cita en FindingIds hallazgos que no existen",
                                                  DPC + "[PROCEDURE_PIECES_ROUND3] cita en FindingIds hallazgos que no existen",
                                                  "QuestionAnswers[Q-A7R3-01] cita en FindingIds hallazgos que no existen",
                                                  "Materiality.Triggers[M-04] cita en FindingIds hallazgos que no existen",
                                                  "DeltaPurity cita en FindingIds hallazgos que no existen",
                                                  CVA5 + " cita en FindingIds hallazgos que no existen"])
    p("90-ids-de-hallazgo-repetidos", lambda res: res.update({"OptionalFindings": [opt_finding(), opt_finding()]}),
      ["identificadores de hallazgo repetidos"])
    p("91-required-sin-invariante-ni-contraejemplo", cr(lambda res: res["RequiredFindings"][0].update({"ViolatedInvariant": " ", "Counterexample": ""})),
      ["A62-A7R3-R1 REQUIRED sin ViolatedInvariant ni Counterexample"])
    p("92-required-sin-evidencia", cr(lambda res: res["RequiredFindings"][0].update({"Evidence": []})), [SV], schema_valid=False)
    # ---- the reference mechanism (decisions §96, point 6): relative, exact, present in the closure, checked against the real file
    a5r = closure_range(A7, "## 5. ")[0]
    p("93-evidencia-fuera-del-cierre", cr(lambda res: res["RequiredFindings"][0].update({"Evidence": ["docs/HANDOFF.md:1"]})),
      [SV, "referencia no canónica en RequiredFindings[A62-A7R3-R1] (docs/HANDOFF.md:1: %s)" % NC], schema_valid=False)
    p("93b-referencia-mal-escrita-de-la-ronda-2", cr(lambda res: res["RequiredFindings"][0].update({"Evidence": ["docs/initiatives/I-62-A7.md:120"]})),
      [SV, "referencia no canónica en RequiredFindings[A62-A7R3-R1] (docs/initiatives/I-62-A7.md:120: %s)" % NC], schema_valid=False)
    dd = EVID + "/../r3/summary.json:3"
    p("93c-referencia-con-punto-punto", lambda res: mat(res)["ClauseRefs"].append(dd),
      [SV, "referencia no canónica en Materiality (%s: %s)" % (dd, NE)], schema_valid=False)
    p("93d-referencia-con-barras-invertidas", lambda res: mat(res)["ClauseRefs"].append(A7.replace("/", "\\") + ":%d" % a5r),
      [SV, "referencia no canónica en Materiality (%s:%d: %s)" % (A7, a5r, NE)], schema_valid=False)
    p("93e-referencia-con-punto", lambda res: mat(res)["ClauseRefs"].append(EVID + "/./summary.json:3"),
      [SV, "referencia no canónica en Materiality (%s/./summary.json:3: %s)" % (EVID, NE)], schema_valid=False)
    other = SUMMARY.replace("summary.json", "SUMMARY.json")
    p("93f-otra-grafia-admitida-por-el-esquema-rechazada-por-el-auditor", lambda res: mat(res)["ClauseRefs"].append(other + ":3"),
      ["referencia no canónica en Materiality (%s:3: %s)" % (other, NE)])
    p("93g-premisa-con-otra-grafia", lambda res: mat(res)["ClauseRefs"].append("ORDER.TXT:3"),
      [SV, "referencia no canónica en Materiality (ORDER.TXT:3: %s)" % NE], schema_valid=False)
    p("93h-premisa-por-su-ruta-absoluta", lambda res: mat(res)["ClauseRefs"].append(RUN + "\\order.txt:3"),
      [SV, "referencia no canónica en Materiality (%s/order.txt:3: %s)" % (PR.userless(RUN), NE)], schema_valid=False)
    p("94-evidencia-sobre-lineas-no-leidas", cr(lambda res: res["RequiredFindings"][0].update({"Evidence": [ref(A6, 10, 12)]})),
      ["referencia sobre líneas no entregadas fielmente en RequiredFindings[A62-A7R3-R1] (%s)" % ref(A6, 10, 12)])
    p("95-evidencia-opcional-no-leida", lambda res: res.update({"OptionalFindings": [dict(opt_finding(), Evidence=[ref(V14, 2408)])]}),
      ["referencia sobre líneas no entregadas fielmente en OptionalFindings[A62-A7R3-O1]"])
    p("96-required-sin-disposicion", cr(lambda res: res["RequiredFindings"].append(req_finding("A62-A7R3-R2"))),
      ["REQUIRED sin disposición: todo REQUIRED lo cita al menos un elemento evaluado (A62-A7R3-R2)"])

    def agreed_with(update):
        def f(res):
            res["RequiredFindings"] = [req_finding()]
            update(res)
        return f
    AG0 = "AGREED exige cero REQUIRED, ninguna decisión del Owner pendiente e IfAgreed todo en true"
    p("97-agreed-resolucion-no-corregida", agreed_with(lambda res: rrd(res)[PF].update({"Status": "NOT_CORRECTED", "FindingIds": [R]})),
      [AG0, "AGREED exige el hallazgo previo A62-A7R2-R1 en CORRECTED"])

    def ag_impure(res):
        chd(res)["S33_ROW_C_AND_S34_C6_NOTE"].update({"Assessment": "OUTSIDE_SCOPE", "FindingIds": [R]})
        dp(res).update({"Status": "NOT_CONFIRMED", "FindingIds": [R]})
    p("98-agreed-pureza-no-confirmada", agreed_with(ag_impure), [AG0, "AGREED exige DeltaPurity CONFIRMED con todos los cambios de la ronda 3 WITHIN_SCOPE"])
    p("99-agreed-regresion", agreed_with(lambda res: rgd(res)["T09_NO_UNAUTHORIZED_CHANGES"].update({"Status": "REGRESSION", "FindingIds": [R]})),
      [AG0, "AGREED exige todos los temas de RegressionCheck en NO_REGRESSION"])
    p("100-agreed-disparador-en-desacuerdo", agreed_with(lambda res: trd(res)["M-05"].update({"Assessment": "DISAGREE", "FindingIds": [R]})),
      [AG0, "AGREED exige los ocho disparadores de Materiality en AGREE"])
    p("100b-agreed-con-a5-incompatible", agreed_with(lambda res: cvd(res)["A5"].update({"Status": "INCOMPATIBLE", "FindingIds": [R]})),
      [AG0, "AGREED exige A-5 y A-6 COMPATIBLE"])

    def agreed_owner(res):
        orc(res).update({"OwnerDecisionRequired": True, "UnidentifiedConsequences": ["x"]})
    p("101-agreed-con-decision-del-owner", agreed_owner, [AG0])

    def cr_ok(res):
        res["OptionalFindings"] = [opt_finding()]
        rrd(res)[PF].update({"Status": "NOT_CORRECTED", "FindingIds": [R, O]})
        rgd(res)["T06_CLAUSEMAP_COVERAGE_RULE"].update({"FindingIds": [O]})
        chd(res)["R2R1_REBASE_COHERENCE"].update({"Assessment": "OUTSIDE_SCOPE", "FindingIds": [R]})
        dp(res).update({"Status": "NOT_CONFIRMED", "FindingIds": [R]})
        trd(res)["M-06"].update({"Assessment": "DISAGREE", "FindingIds": [R]})
        qad(res)["Q-A7R3-03"].update({"Outcome": "NOT_SATISFIED", "FindingIds": [O]})
        cvd(res)["A6"].update({"Status": "INCOMPATIBLE", "FindingIds": [R]})
        cvd(res)["A5"].update({"FindingIds": [O]})
    p("102-changes-required-coherente-aceptado", cr(cr_ok), coherent=True)
    p("103-rejected-coherente-aceptado", cr(lambda res: trd(res)["M-07"].update({"Assessment": "DISAGREE", "FindingIds": [R]}), "REJECTED"), coherent=True)
    p("104-rejected-sin-required", lambda res: not_agreed(res, "REJECTED"), ["REJECTED sin ningún REQUIRED"])
    p("105-blocked-coherente-aceptado", bl(lambda res: None), coherent=True)

    def ag_ok(res):
        res["OptionalFindings"] = [opt_finding()]
        qad(res)["Q-A7R3-01"].update({"Outcome": "NOT_SATISFIED", "FindingIds": [O]})
        rgd(res)["T07_MULTI_CATEGORY_ENTRIES"]["FindingIds"] = [O]
    p("106-agreed-con-un-opcional-aceptado", ag_ok, coherent=True)
    p("107-id-de-hallazgo-mal-formado", lambda res: res.update({"OptionalFindings": [dict(opt_finding(), FindingId="A62-A7R3-1")]}), [SV],
      schema_valid=False)

    def r2_space_fid(res):
        res["RequiredFindings"][0]["FindingId"] = "A62-A7R2-R2"
        rrd(res)[PF]["FindingIds"] = ["A62-A7R2-R2"]
    p("108-id-nuevo-con-el-espacio-de-la-ronda-2", cr(r2_space_fid), [SV], schema_valid=False)
    p("109-referencia-absoluta", lambda res: mat(res).update({"ClauseRefs": [absp(A7) + ":" + str(a5r)]}),
      [SV, "referencia no canónica en Materiality (%s:%d: %s)" % (PR.userless(absp(A7)), a5r, NE)], schema_valid=False)
    p("110-referencia-mal-formada", lambda res: mat(res).update({"ClauseRefs": ["order.txt"]}), ["referencia mal formada en Materiality", SV],
      schema_valid=False)
    p("111-esquema-estados-invalidos", lambda res: (rrd(res)[PF].update({"Status": "MAYBE"}), trd(res)["M-01"].update({"Assessment": "OK"}),
                                                    rgd(res)["T04_EFF_EFF1_B1_MAP_VALIDATION"].update({"Status": "IGNORED"}),
                                                    cvd(res)["A6"].update({"Status": "PARTIAL"})),
      [SV, "AGREED exige el hallazgo previo A62-A7R2-R1 en CORRECTED", "AGREED exige los ocho disparadores de Materiality en AGREE",
       "AGREED exige todos los temas de RegressionCheck en NO_REGRESSION", "AGREED exige A-5 y A-6 COMPATIBLE"], schema_valid=False)
    p("112-esquema-booleano-como-texto", lambda res: res["Step0"].update({"OrderReadWhole": "true"}), [SV, "las siete booleanas de Step0 van en true"],
      schema_valid=False)
    p("113-esquema-veredicto-inexistente", lambda res: not_agreed(res, "ACCEPTED"), [SV], schema_valid=False)
    # nested schema violations that Test-Json of pwsh 7.6.7 does not see (only the in-process check of the auditor does)
    p("114b-esquema-propiedad-sobrante-anidada", lambda res: res["Step0"].update({"Extra": True}), [SV], schema_valid=False)
    p("114c-esquema-campo-requerido-ausente-anidado", lambda res: rrd(res)[PF].pop("Rationale"), [SV], schema_valid=False)
    p("114d-esquema-texto-vacio-anidado", lambda res: mat(res).update({"Rationale": ""}), [SV], schema_valid=False)
    p("114e-esquema-elemento-de-lista-no-texto", lambda res: orc(res).update({"UnidentifiedConsequences": [1]}),
      [SV, "OwnerDecisionRequired = false con UnidentifiedConsequences"], schema_valid=False)
    p("114f-esquema-null-no-admitido-anidado", lambda res: orc(res).update({"Rationale": None}), [SV], schema_valid=False)
    p("114g-esquema-propiedad-sobrante-doble-anidada", lambda res: chd(res)["R2R1_REBASE_COHERENCE"].update({"X": 1}), [SV], schema_valid=False)
    p("114h-esquema-propiedad-sobrante-en-la-compatibilidad", lambda res: cvd(res)["A5"].update({"X": 1}), [SV], schema_valid=False)
    p("114i-esquema-compatibilidad-ausente", lambda res: res["CompatibilityVerdicts"].pop("A5"),
      [SV, "con un veredicto, " + CVA5 + " sin ClauseRefs", "con un veredicto, NOT_DETERMINED o null no se admiten (CompatibilityVerdicts.A5.Status)",
       "AGREED exige A-5 y A-6 COMPATIBLE"], schema_valid=False)

    # ---- coverage of the round-3 arrays
    def drop(path, field, value):
        def f(res):
            holder = res if len(path) == 1 else res[path[0]]
            holder[path[-1]] = [x for x in holder[path[-1]] if x[field] != value]
        return f

    def dup(path, field, value, replace):
        def f(res):
            holder = res if len(path) == 1 else res[path[0]]
            items_ = holder[path[-1]]
            src = next(x for x in items_ if x[field] == value)
            holder[path[-1]] = [copy.deepcopy(src) if x[field] == replace else x for x in items_]
        return f
    p("115-resolucion-ausente", drop(["PriorFindingResolution"], "PriorFinding", PF),
      ["PriorFindingResolution debe disponer cada id una vez (faltan ['A62-A7R2-R1']", "AGREED exige el hallazgo previo A62-A7R2-R1 en CORRECTED"])
    p("115b-resolucion-repetida", lambda res: res["PriorFindingResolution"].append(copy.deepcopy(res["PriorFindingResolution"][0])),
      ["PriorFindingResolution con identificadores repetidos", "PriorFindingResolution debe disponer cada id una vez",
       "AGREED exige el hallazgo previo A62-A7R2-R1 en CORRECTED"])
    p("116-tema-de-regresion-ausente", drop(["RegressionCheck"], "Topic", "R3_A62_RULE1_PRECISION"),
      ["RegressionCheck debe disponer cada id una vez (faltan ['R3_A62_RULE1_PRECISION']",
       "AGREED exige todos los temas de RegressionCheck en NO_REGRESSION"])
    p("116b-tema-de-regresion-repetido", dup(["RegressionCheck"], "Topic", "R2_INVALIDATOR_RULE_AND_PROCEDURE", "T05_MAP_INVALID_ABSENT_NONCONSTRUCTIBLE"),
      ["RegressionCheck con identificadores repetidos", "RegressionCheck debe disponer cada id una vez"])
    p("117-cambio-ausente", drop(["DeltaPurity", "Changes"], "Change", "PROCEDURE_PIECES_ROUND3"),
      ["DeltaPurity.Changes debe disponer cada id una vez (faltan ['PROCEDURE_PIECES_ROUND3']",
       "AGREED exige DeltaPurity CONFIRMED con todos los cambios de la ronda 3 WITHIN_SCOPE"])
    p("117b-cambio-repetido", dup(["DeltaPurity", "Changes"], "Change", "R2R1_REBASE_ROLE_REVERIFICATION_ONLY", "DESCRIPTIVE_SECTIONS"),
      ["DeltaPurity.Changes con identificadores repetidos", "DeltaPurity.Changes debe disponer cada id una vez"])
    p("118-disparador-ausente", drop(["Materiality", "Triggers"], "Trigger", "M-07"),
      ["Materiality.Triggers debe disponer cada id una vez (faltan ['M-07']", "AGREED exige los ocho disparadores de Materiality"])
    p("118b-disparador-repetido", dup(["Materiality", "Triggers"], "Trigger", "M-01", "M-06"),
      ["Materiality.Triggers con identificadores repetidos", "Materiality.Triggers debe disponer cada id una vez"])
    p("119-pregunta-ausente", drop(["QuestionAnswers"], "Question", "Q-A7R3-03"),
      ["QuestionAnswers debe disponer cada id una vez (faltan ['Q-A7R3-03']"])
    p("119b-pregunta-repetida", dup(["QuestionAnswers"], "Question", "Q-A7R3-01", "Q-A7R3-02"),
      ["QuestionAnswers con identificadores repetidos", "QuestionAnswers debe disponer cada id una vez"])

    def stop_with_dup(res):
        stop(res, None)
        res["RegressionCheck"] = [{"Topic": "T04_EFF_EFF1_B1_MAP_VALIDATION", "Status": "NO_REGRESSION", "Rationale": "x", "ClauseRefs": [],
                                   "FindingIds": []}] * 2
    p("120-not-accredited-con-ids-repetidos", stop_with_dup, ["RegressionCheck con identificadores repetidos"], base=stop_calls)

    # ---- direct read of AUTOMATION_PLAN 16.7 L593-L595 (round 3; the rule of v5.x) and the references of nested elements
    apd = ref(AP, *AP_DIRECT)
    p("121-sin-lectura-directa-de-ap", lambda res: None,
      ["lectura directa: %s no entregadas fielmente" % apd, "referencia sobre líneas no entregadas fielmente en DirectReads (%s)" % apd],
      base=lambda r: base_calls(r, direct=False))
    p("121b-cita-de-ap-leida-solo-en-a7", lambda res: mat(res)["ClauseRefs"].append(apd),
      ["lectura directa: %s no entregadas fielmente" % apd, "referencia sobre líneas no entregadas fielmente en DirectReads (%s)" % apd,
       "referencia sobre líneas no entregadas fielmente en Materiality (%s)" % apd], base=lambda r: base_calls(r, direct=False))
    p("121c-directreads-sin-ap", lambda res: res.update({"DirectReads": [{"Ref": sec_ref("### 3.4 "), "Purpose": "x"}]}),
      ["lectura directa: DirectReads sin una entrada que cubra"])
    p("121d-directreads-cubre-solo-593", lambda res: res.update({"DirectReads": [{"Ref": ref(AP, AP_DIRECT[0]), "Purpose": "x"}]}),
      ["lectura directa: DirectReads sin una entrada que cubra"])

    def cited_after_direct(res):
        mat(res)["ClauseRefs"].append(apd)
        rrd(res)[PF]["ClauseRefs"].append(apd)
        res["DirectReads"].append({"Ref": sec_ref("### 3.4 "), "Purpose": "x"})
    p("121e-cita-de-ap-tras-la-lectura-directa-aceptada", cited_after_direct, coherent=True)
    p("121f-referencia-anidada-no-leida", lambda res: trd(res)["M-02"]["ClauseRefs"].append(ref(A6, 10, 12)),
      ["referencia sobre líneas no entregadas fielmente en Materiality.Triggers[M-02] (%s)" % ref(A6, 10, 12)])
    p("121g-referencia-anidada-de-evidencia-leida", lambda res: chd(res)["R2R1_REBASE_COHERENCE"]["ClauseRefs"].append(preflight_ref()), coherent=True)
    p("121h-referencia-de-la-pureza-no-leida", lambda res: dp(res)["ClauseRefs"].append(ref(A6, 10, 12)),
      ["referencia sobre líneas no entregadas fielmente en DeltaPurity (%s)" % ref(A6, 10, 12)])
    p("121i-referencia-de-la-compatibilidad-no-leida", lambda res: cvd(res)["A6"]["ClauseRefs"].append(ref(A6, 10, 12)),
      ["referencia sobre líneas no entregadas fielmente en %s (%s)" % (CVA6, ref(A6, 10, 12))])
    p("121j-referencia-a-la-ronda-2-y-a-su-salida-aceptada",
      lambda res: rrd(res)[PF]["ClauseRefs"].extend(["a7-r2.md:197-200", ref(R2OUT, 3), ref(Q6, 3)]), coherent=True)

'''
a = t.index(COH_START)
b = t.index(COH_END)
t = t[:a] + COH + t[b:]

R = [
    ('EVID = "docs/automation/evidence/I-62-A7/r2"                 # el preflight de la ronda 2\n'
     'GRR, SUMMARY, PREFLIGHT_MD = EVID + "/a7-guards-result-run.json", EVID + "/summary.json", EVID + "/preflight.md"\n'
     'R1OUT = "docs/automation/evidence/I-62-architect-A-7/R20261019T010200Z-4a72/output.json"\n'
     'DISP = "docs/automation/evidence/I-62-A7/r2-dispositions/optional-dispositions.md"\n',
     'EVID = "docs/automation/evidence/I-62-A7/r3"                 # el preflight de la ronda 3\n'
     'GRR, SUMMARY, PREFLIGHT_MD = EVID + "/a7-guards-result-run.json", EVID + "/summary.json", EVID + "/preflight.md"\n'
     'Q6 = EVID + "/q6-compat.json"                                # la compatibilidad del preflight de la ronda 3 (A-5, A-6, …)\n'
     'R2OUT = "docs/automation/evidence/I-62-architect-A-7/R20261019T201030Z-d1bf/output.json"\n'),
    ('OTHER_A7_BLOB = "6c29747d61c5f689a195095ad3712e6d9dba62aa"   # la ronda 1 de A-7: nunca el objeto de esta revisión\n',
     'OTHER_A7_BLOB = "8a528133c7cccd83356e31f8fd79cd2ebb0cec15"   # la ronda 2 de A-7: nunca el objeto de esta revisión\n'),
    ('IFAGREED_KEYS = ("ScopeLimitedToR1R3AndConsequences", "Round1FindingsCorrected", "DeltaLimitedToR1R3", "NoRegressionOfRound1Conformities",\n'
     '                 "MaterialityConfirmed", "NoUnidentifiedOwnerReservedConsequence", "NoUnauthorizedChanges", "RelatedImpactsUnchangedOrDeclared",\n'
     '                 "SuitableForCoordinatorAgreement")\n',
     'IFAGREED_KEYS = ("ScopeLimitedToR2R1AndConsequences", "PriorFindingCorrected", "DeltaLimitedToR2R1", "NoRegressionOfPriorConformities",\n'
     '                 "CompatibleWithA5AndA6", "MaterialityConfirmed", "NoUnidentifiedOwnerReservedConsequence", "NoUnauthorizedChanges",\n'
     '                 "SuitableForCoordinatorAgreement")\n'),
    ('AP_READ = (608, 625)                          # the direct read of Step 0 (AUTOMATION_PLAN 16.8 L616-L617 inside)\n'
     'BASE_SECTION = 94                             # decisions §94: the last section of the clone; the authorization has to come after it\n'
     'SYNTH_AUTH_SECTION = 95                       # synthetic disposition after §94 (the kit is delivered with the authorization PENDING)\n',
     'AP_DIRECT = (593, 595)                        # AUTOMATION_PLAN 16.7, the reverification «después de escribir, sin conflictos» (L19 of A-7 §2)\n'
     'AP_READ = (585, 603)                          # the direct read of Step 0 (AUTOMATION_PLAN 16.7 L593-L595 inside; the prompt\'s offset and limit)\n'
     'BASE_SECTION = 96                             # decisions §96: the last section of the clone; the authorization has to come after it\n'
     'SYNTH_AUTH_SECTION = 97                       # synthetic disposition after §96 (the kit is delivered with the authorization PENDING)\n'),
    ('    """Lines of a SYNTHETIC Coordinator disposition after §94 (not a real disposition), appended to the clone\'s decisions by the self-test."""\n'
     '    return ["## %d. Disposición del Coordinator: autorización de la revisión formal de la ronda 2 de A-7 (selftest)" % number, "",\n',
     '    """Lines of a SYNTHETIC Coordinator disposition after §96 (not a real disposition), appended to the clone\'s decisions by the self-test."""\n'
     '    return ["## %d. Disposición del Coordinator: autorización de la revisión formal de la ronda 3 de A-7 (selftest)" % number, "",\n'),
    ('    """While the kit\'s owner_gate.py held the authorization pending, the audit cases would need a pinned authorization: SYNTHETIC pins (§94 appended\n',
     '    """While the kit\'s owner_gate.py held the authorization pending, the audit cases would need a pinned authorization: SYNTHETIC pins (§97 appended\n'),
    ('        PINS = {"Source": "SYNTHETIC (owner_gate.py del kit con la autorización pendiente; §94 sintética añadida a las decisiones del clon)",\n',
     '        PINS = {"Source": "SYNTHETIC (owner_gate.py del kit con la autorización pendiente; §97 sintética añadida a las decisiones del clon)",\n'),
    ('# ------------------------------------------------------------------ the A-7 round-2 reads and the base result\n',
     '# ------------------------------------------------------------------ the A-7 round-3 reads and the base result\n'),
    ('    read_range(r, DEC, *closure_range(DEC, "## 94. "))\n', '    read_range(r, DEC, *closure_range(DEC, "## 96. "))\n'),
    ('    read_range(r, R1OUT, 1, 60)\n', '    read_range(r, R2OUT, 1, 230)\n'),
    ('    r.call("Read", {"file_path": RUN + "\\\\a7-r1.md"}, numbered(lines_of(run="a7-r1.md")))\n',
     '    r.call("Read", {"file_path": RUN + "\\\\a7-r2.md"}, numbered(lines_of(run="a7-r2.md")))\n'),
    ('    r.call("Read", {"file_path": absp(DISP)}, numbered(lines_of(DISP)))\n', '    r.call("Read", {"file_path": absp(Q6)}, numbered(lines_of(Q6)))\n'),
    ('ROUND1_ROWS = {"A62-A7-R1": "§3.6, fila R1", "A62-A7-R2": "§3.6, fila R2", "A62-A7-R3": "§3.6, fila R3"}\n'
     'REG_ROWS = {"T04_EFF_EFF1_B1_MAP_VALIDATION": "§3.1, Más allá de R7-A", "T05_MAP_INVALID_ABSENT_NONCONSTRUCTIBLE": "A7-1, punto 5",\n',
     'PRIOR_ROWS = {"A62-A7R2-R1": "§3.7, fila A62-A7R2-R1"}\n'
     'REG_ROWS = {"R1_TRX_AND_RELAYFACTS_AS_CORRECTED": "§3.6, fila R1", "R2_INVALIDATOR_RULE_AND_PROCEDURE": "§3.6, fila R2",\n'
     '            "R3_A62_RULE1_PRECISION": "§3.6, fila R3",\n'
     '            "T04_EFF_EFF1_B1_MAP_VALIDATION": "§3.1, Más allá de R7-A", "T05_MAP_INVALID_ABSENT_NONCONSTRUCTIBLE": "A7-1, punto 5",\n'),
    ('        "Schema": "rackcad-a7r2-architect-review/0", "RunId": ids["RunId"], "InvocationId": ids["InvocationId"],\n',
     '        "Schema": "rackcad-a7r3-architect-review/0", "RunId": ids["RunId"], "InvocationId": ids["InvocationId"],\n'),
    ('        "DirectReads": [{"Ref": ref(AP, 616, 617), "Purpose": "x"}],\n', '        "DirectReads": [{"Ref": ref(AP, *AP_DIRECT), "Purpose": "x"}],\n'),
    ('        "Round1FindingResolution": [{"Round1Finding": f, "Status": "CORRECTED", "Rationale": "x", "ClauseRefs": [a7_ref(ROUND1_ROWS[f])], "FindingIds": []}\n'
     '                                    for f in PR.ROUND1],\n'
     '        "DeltaPurity": {"Status": "CONFIRMED",\n'
     '                        "Changes": [{"Change": k, "Assessment": "WITHIN_R1_R3", "Rationale": "x",\n'
     '                                     "ClauseRefs": [a7_ref("§3.6, tabla de correcciones de la ronda 2")], "FindingIds": []} for k in PR.CHANGES],\n'
     '                        "Rationale": "x", "ClauseRefs": [sec_ref("### 3.6 ")], "FindingIds": []},\n',
     '        "PriorFindingResolution": [{"PriorFinding": f, "Status": "CORRECTED", "Rationale": "x", "ClauseRefs": [a7_ref(PRIOR_ROWS[f])], "FindingIds": []}\n'
     '                                   for f in PR.ROUND1],\n'
     '        "DeltaPurity": {"Status": "CONFIRMED",\n'
     '                        "Changes": [{"Change": k, "Assessment": "WITHIN_SCOPE", "Rationale": "x",\n'
     '                                     "ClauseRefs": [a7_ref("§3.7, tabla de la corrección de la ronda 3")], "FindingIds": []} for k in PR.CHANGES],\n'
     '                        "Rationale": "x", "ClauseRefs": [sec_ref("### 3.7 ")], "FindingIds": []},\n'),
    ('                            for t in PR.REGRESSION],\n'
     '        "Materiality": {',
     '                            for t in PR.REGRESSION],\n'
     '        "CompatibilityVerdicts": {"A5": {"Status": "COMPATIBLE", "Rationale": "x", "ClauseRefs": [a7_ref("§6, A-5")], "FindingIds": []},\n'
     '                                  "A6": {"Status": "COMPATIBLE", "Rationale": "x", "ClauseRefs": [a7_ref("§6, A-6")], "FindingIds": []}},\n'
     '        "Materiality": {'),
    ('        "RelatedImpacts": {"ChangedByRound2": False, "Note": "", "ClauseRefs": [a7_ref("§3.5, lo que A-7 no hace")], "FindingIds": []},\n', ''),
    ('def rrd(res):\n    return {d["Round1Finding"]: d for d in res["Round1FindingResolution"]}\n',
     'def rrd(res):\n    return {d["PriorFinding"]: d for d in res["PriorFindingResolution"]}\n'),
    ('def odc(res):\n    return {d["Round1Optional"]: d for d in res.get("OptionalDispositionConformance", [])}\n',
     'def cvd(res):\n    return res["CompatibilityVerdicts"]\n'),
    ('def ri(res):\n    return res["RelatedImpacts"]\n\n\n'
     'def with_optionals(res, status="CONFORMS", ids=("A62-A7-O1", "A62-A7-O2", "A62-A7-O3")):\n'
     '    res["OptionalDispositionConformance"] = [{"Round1Optional": o, "Status": status, "Rationale": "x",\n'
     '                                              "ClauseRefs": [ref(DISP, 9 + int(o[-1]) - 1)], "FindingIds": []} for o in ids]\n'
     '    return res\n\n\n'
     'def req_finding(fid="A62-A7R2-R1", evidence=None):\n',
     'def req_finding(fid="A62-A7R3-R1", evidence=None):\n'),
    ('def opt_finding(fid="A62-A7R2-O1"):\n'
     '    return {"FindingId": fid, "Section": "§8", "Note": "x", "Evidence": [a7_ref("§8, Q-A7R2-01")], "SuggestedCorrection": "x"}\n',
     'def opt_finding(fid="A62-A7R3-O1"):\n'
     '    return {"FindingId": fid, "Section": "§8", "Note": "x", "Evidence": [a7_ref("§8, Q-A7R3-01")], "SuggestedCorrection": "x"}\n'),
    ('    """A coherent CHANGES REQUIRED (or REJECTED) skeleton: one REQUIRED that leaves A62-A7-R1 in PARTIALLY_CORRECTED."""\n'
     '    not_agreed(res, verdict)\n'
     '    res["RequiredFindings"] = [req_finding()]\n'
     '    rrd(res)["A62-A7-R1"].update({"Status": "PARTIALLY_CORRECTED", "FindingIds": ["A62-A7R2-R1"]})\n',
     '    """A coherent CHANGES REQUIRED (or REJECTED) skeleton: one REQUIRED that leaves A62-A7R2-R1 in PARTIALLY_CORRECTED."""\n'
     '    not_agreed(res, verdict)\n'
     '    res["RequiredFindings"] = [req_finding()]\n'
     '    rrd(res)["A62-A7R2-R1"].update({"Status": "PARTIALLY_CORRECTED", "FindingIds": ["A62-A7R3-R1"]})\n'),
    ('        r.call("Grep", {"pattern": "sin reintento", "path": RUN + "\\\\order.txt", "output_mode": "content", "-n": True}, "54:" + order[53])\n',
     '        n_o = next(i for i, l in enumerate(order) if "No ejecutar Codex" in l)\n'
     '        r.call("Grep", {"pattern": "No ejecutar Codex", "path": RUN + "\\\\order.txt", "output_mode": "content", "-n": True},\n'
     '               "%d:%s" % (n_o + 1, order[n_o]))\n'),
    ('        rrd(res)["A62-A7-R1"]["ClauseRefs"] += ["prompt.md:1", "order.txt:99"]\n'
     '    add(variant("15-referencia-a-prompt-o-fuera-de-rango", expected=["referencia no canónica en Round1FindingResolution[A62-A7-R1] (prompt.md:1)",\n'
     '                                                                     "referencia no canónica en Round1FindingResolution[A62-A7-R1] (order.txt:99)"],\n'
     '                result_patch=ref_bad))\n',
     '        rrd(res)["A62-A7R2-R1"]["ClauseRefs"] += ["prompt.md:1", "order.txt:99"]\n'
     '    add(variant("15-referencia-a-prompt-o-fuera-de-rango", expected=[\n'
     '        "resultado: no valida contra result.schema.json",\n'
     '        "referencia no canónica en PriorFindingResolution[A62-A7R2-R1] (prompt.md:1: no es un archivo del cierre ni una premisa del run)",\n'
     '        "referencia no canónica en PriorFindingResolution[A62-A7R2-R1] (order.txt:99: líneas fuera de rango del archivo)"],\n'
     '                result_patch=ref_bad, schema_valid=False))\n'),
    ('                    "Round1FindingResolution": [], "RegressionCheck": [], "QuestionAnswers": [],\n'
     '                    "DeltaPurity": dict(nds, Status="NOT_DETERMINED", Changes=[]),\n'
     '                    "Materiality": dict(nds, Triggers=[]),\n'
     '                    "OwnerReservedConsequence": dict(nds, OwnerDecisionRequired=None, UnidentifiedConsequences=[]),\n'
     '                    "RelatedImpacts": {"ChangedByRound2": None, "Note": "", "ClauseRefs": [], "FindingIds": []},\n',
     '                    "PriorFindingResolution": [], "RegressionCheck": [], "QuestionAnswers": [],\n'
     '                    "DeltaPurity": dict(nds, Status="NOT_DETERMINED", Changes=[]),\n'
     '                    "CompatibilityVerdicts": {"A5": dict(nds, Status="NOT_DETERMINED"), "A6": dict(nds, Status="NOT_DETERMINED")},\n'
     '                    "Materiality": dict(nds, Triggers=[]),\n'
     '                    "OwnerReservedConsequence": dict(nds, OwnerDecisionRequired=None, UnidentifiedConsequences=[]),\n'),
    ('        rgd(res)["T05_MAP_INVALID_ABSENT_NONCONSTRUCTIBLE"].update({"Status": "REGRESSION", "FindingIds": ["A62-A7R2-R1"]})\n'
     '    add(variant("17-agreed-con-un-required", result_patch=agreed_with_required, expected=["AGREED exige cero REQUIRED",\n'
     '                                                                                          "AGREED exige los cinco temas de RegressionCheck en NO_REGRESSION"]))\n',
     '        rgd(res)["T05_MAP_INVALID_ABSENT_NONCONSTRUCTIBLE"].update({"Status": "REGRESSION", "FindingIds": ["A62-A7R3-R1"]})\n'
     '    add(variant("17-agreed-con-un-required", result_patch=agreed_with_required, expected=["AGREED exige cero REQUIRED",\n'
     '                                                                                          "AGREED exige todos los temas de RegressionCheck en NO_REGRESSION"]))\n'),
    ('        trd(res)["M-08"].update({"Assessment": "DISAGREE", "FindingIds": ["A62-A7R2-O1"]})\n',
     '        trd(res)["M-08"].update({"Assessment": "DISAGREE", "FindingIds": ["A62-A7R3-O1"]})\n'),
    ('        rrd(res)["A62-A7-R1"]["ClauseRefs"] += ["order.txt:3-9", "order.txt:43", "a7-r1.md:121-125"]\n'
     '        mat(res)["ClauseRefs"].append("order.txt:1")\n'
     '    add(variant("25-referencias-a-order-y-a7-r1-aceptadas", result_patch=ref_run, require_coherent=True))\n',
     '        rrd(res)["A62-A7R2-R1"]["ClauseRefs"] += ["order.txt:3-9", "order.txt:43", "a7-r2.md:121-125"]\n'
     '        mat(res)["ClauseRefs"].append("order.txt:1")\n'
     '    add(variant("25-referencias-a-order-y-a7-r2-aceptadas", result_patch=ref_run, require_coherent=True))\n'),
    ('                          "lectura directa: %s no entregadas fielmente" % ref(AP, 616, 617)],\n',
     '                          "lectura directa: %s no entregadas fielmente" % ref(AP, *AP_DIRECT)],\n'),
    ('TRX_MAIN, TRX_UC = "docs/automation/evidence/I-62-A7/b1-like/main/fixture-tests.trx", "docs/automation/evidence/I-62-A7/b1-like/uc/fixture-tests.trx"\n',
     'TRX_MAIN, TRX_UC = EVID + "/b1-like/main/fixture-tests.trx", EVID + "/b1-like/uc/fixture-tests.trx"   # the round-3 B1-like case\n'),
    ('T03 = "A62-A7-R1"                              # the element the fidelity cases cite from\n',
     'T03 = "A62-A7R2-R1"                            # the element the fidelity cases cite from\n'),
    ('    not_delivered = "referencia sobre líneas no entregadas fielmente en Round1FindingResolution[%s] (%s)"\n',
     '    not_delivered = "referencia sobre líneas no entregadas fielmente en PriorFindingResolution[%s] (%s)"\n'),
    ('    # F08: the direct read of AUTOMATION_PLAN 16.8 L616-L617 through its faithful copy satisfies Step 0\n',
     '    # F08: the direct read of AUTOMATION_PLAN 16.7 L593-L595 through its faithful copy satisfies Step 0\n'),
    ('                 expected=["referencia no canónica en Round1FindingResolution[%s] (%s:1)" % (T03, trx_copy)]))\n',
     '                 expected=["resultado: no valida contra result.schema.json",\n'
     '                           "referencia no canónica en PriorFindingResolution[%s] (%s:1: no es un archivo del cierre ni una premisa del run)"\n'
     '                           % (T03, trx_copy)], schema_valid=False))\n'),
    ('    """Gate 11 of the round-2 kit (owner_gate.evaluate, pure): the clone\'s decisions end at §94 (the base section); the authorization is a FUTURE\n'
     '    disposition after §94, played here by a SYNTHETIC §95 appended to the clone\'s text (never a real disposition)."""\n',
     '    """Gate 11 of the round-3 kit (owner_gate.evaluate, pure): the clone\'s decisions end at §96 (the base section); the authorization is a FUTURE\n'
     '    disposition after §96, played here by a SYNTHETIC §97 appended to the clone\'s text (never a real disposition)."""\n'),
    ('    s94 = next(i for i, l in enumerate(dec_lines) if l.startswith("## %d. " % BASE_SECTION))\n'
     '    assert not any(l.startswith("## ") for l in dec_lines[s94 + 1:]), "§94 no es la última sección de las decisiones del clon"\n',
     '    s_base = next(i for i, l in enumerate(dec_lines) if l.startswith("## %d. " % BASE_SECTION))\n'
     '    assert not any(l.startswith("## ") for l in dec_lines[s_base + 1:]), "§96 no es la última sección de las decisiones del clon"\n'),
    ('    dec_base = "\\n".join(base_lines) + "\\n"              # decisiones del clon (terminan en §94): el texto de referencia de la compuerta\n',
     '    dec_base = "\\n".join(base_lines) + "\\n"              # decisiones del clon (terminan en §96): el texto de referencia de la compuerta\n'),
    ('    dec_auth = dec_base + syn_text                        # la HEAD del worktree con una disposición posterior a §94 (SINTÉTICA)\n',
     '    dec_auth = dec_base + syn_text                        # la HEAD del worktree con una disposición posterior a §96 (SINTÉTICA)\n'),
    ('    e = run_case("O00b-pendiente-con-una-seccion-posterior-a-94", dec_base', '    e = run_case("O00b-pendiente-con-una-seccion-posterior-a-96", dec_base'),
    ('    run_case("O01-autorizacion-posterior-a-94-fijada-pasa", dec_auth)\n', '    run_case("O01-autorizacion-posterior-a-96-fijada-pasa", dec_auth)\n'),
    ('    run_case("O10-fijada-no-posterior-a-94", dec_base,', '    run_case("O10-fijada-no-posterior-a-96", dec_base,'),
    ('    run_case("O12b-blob-fijado-de-la-ronda-1", dec_auth,', '    run_case("O12b-blob-fijado-de-la-ronda-2", dec_auth,'),
    ('    t = dec_auth.replace("suma 3 522 bytes", "suma 3 523 bytes")\n'
     '    assert t != dec_auth\n'
     '    run_case("O23-decision-94-cambiada-en-la-head", t, ["no crece solo por añadido"], dec_cl=dec_base)\n',
     '    t = dec_auth.replace("suma 3 198 bytes", "suma 3 199 bytes")\n'
     '    assert t != dec_auth\n'
     '    run_case("O23-decision-96-cambiada-en-la-head", t, ["no crece solo por añadido"], dec_cl=dec_base)\n'),
    ('    doc = {"Selftest": "post-review.py v6.0-a7r2 + transport_gate.py + owner_gate.py (A-7 ronda 2) + pin_authorization.values + c3_analysis.analyze + "\n'
     '                       "coherencia con launch.py",\n',
     '    doc = {"Selftest": "post-review.py v7.0-a7r3 + transport_gate.py + owner_gate.py (A-7 ronda 3) + pin_authorization.values + c3_analysis.analyze + "\n'
     '                       "coherencia con launch.py y patrón de referencias del esquema",\n'),
    ('        with_launch["Mismatches"] == [],\n'
     '        RunFiles=len(PR.RUN_FILES), Copies=len(PR.CLOSURE["FaithfulCopies"]), Consistent=ok_["Ok"], WithoutFaithfulCopies=no_fc["Mismatches"],\n'
     '        ManifestWithoutLast=no_man["Mismatches"], RunWithoutLast=no_copy["Ok"], RunWithLaunch=with_launch["Ok"])\n'
     '    return cases\n',
     '        with_launch["Mismatches"] == [],\n'
     '        RunFiles=len(PR.RUN_FILES), Copies=len(PR.CLOSURE["FaithfulCopies"]), Consistent=ok_["Ok"], WithoutFaithfulCopies=no_fc["Mismatches"],\n'
     '        ManifestWithoutLast=no_man["Mismatches"], RunWithoutLast=no_copy["Ok"], RunWithLaunch=with_launch["Ok"])\n'
     '    # X06 (decisions §96, point 6): the reference pattern of result.schema.json (the same in every Ref, Evidence and ClauseRefs) admits every\n'
     '    # closure path and the premise run files, and none of the negatives (the misspelled path of round 2, absolute, leading slash, backslashes,\n'
     '    # `..`, `.`, a faithful copy, prompt.md, the round-1 premise, a file outside the closure, no line, line 0, another spelling of a premise)\n'
     '    pats = []\n'
     '\n'
     '    def walk(o):\n'
     '        if isinstance(o, dict):\n'
     '            if isinstance(o.get("pattern"), str) and o["pattern"].endswith(":[1-9][0-9]*(-[1-9][0-9]*)?$"):\n'
     '                pats.append(o["pattern"])\n'
     '            for v in o.values():\n'
     '                walk(v)\n'
     '        elif isinstance(o, list):\n'
     '            for v in o:\n'
     '                walk(v)\n'
     '    walk(json.load(open(SCHEMA, encoding="utf-8")))\n'
     '    rx = re.compile(pats[0]) if pats else None\n'
     '    positives = [x["Path"] + ":1" for x in PR.CLOSURE["CanonicalInputs"]] + [n + ":3" for n in PR.CLOSURE["PremiseRunFiles"]]\n'
     '    negatives = ["docs/initiatives/I-62-A7.md:120", "/" + A7 + ":1", absp(A7) + ":1", A7.replace("/", "\\\\") + ":1", EVID + "/../r2/summary.json:3",\n'
     '                 EVID + "/./summary.json:3", PR.RUN_FILES[-1] + ":1", "prompt.md:1", "a7-r1.md:3", "docs/HANDOFF.md:1", A7, A7 + ":0", "ORDER.TXT:3"]\n'
     '    bad_pos = [x for x in positives if not (rx and rx.match(x))]\n'
     '    bad_neg = [x for x in negatives if rx and rx.match(x)]\n'
     '    row("X06-patron-de-referencias-del-esquema", bool(pats) and len(set(pats)) == 1 and len(pats) >= 3 and not bad_pos and not bad_neg,\n'
     '        Patterns=len(pats), Distinct=len(set(pats)), Positives=len(positives), PositivesRejected=bad_pos, NegativesAccepted=bad_neg)\n'
     '    return cases\n'),
]
for x, y in R:
    assert t.count(x) == 1, ("no único", t.count(x), x[:120])
    t = t.replace(x, y)
for bad in ("R1OUT", "DISP", "a7-r1.md\"", "Round1FindingResolution", "WITHIN_R1_R3", "Q-A7R2", "rackcad-a7r2",
            "odc(", "ri(res)", "with_optionals", "AP168", "Decisions94", "616, 617"):
    assert bad not in t, bad
open(p, "w", encoding="utf-8", newline="").write(t)
print("ok", hashlib.sha256(t.encode("utf-8")).hexdigest())
