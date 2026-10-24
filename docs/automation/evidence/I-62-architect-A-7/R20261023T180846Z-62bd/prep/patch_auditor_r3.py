"""Deriva kit/post-review.py v7.0-a7r3 (decisiones §96) del auditor v6.0-a7r2 por sustituciones exactas. Cambian: la cabecera y AUDITOR_VERSION;
las listas que lee del cierre (PriorFindings en lugar de Round1Findings; sin opcionales); Step0 de la ronda 3; los arreglos del esquema nuevo
(PriorFindingResolution; sin OptionalDispositionConformance ni RelatedImpacts; CompatibilityVerdicts.A5 y .A6 como elementos con ClauseRefs); la
coherencia (hallazgo previo, WITHIN_SCOPE/OUTSIDE_SCOPE, dictámenes de compatibilidad); los mensajes de la compuerta 11 (§96); y el mensaje de una
referencia no canónica, que muestra la referencia tal como se escribió y por qué no lo es (decisiones §96, punto 6). Las reglas de lanzamiento,
transporte, stream-json, transcripción, herramientas, lectura, fidelidad, copias fieles, referencias (fondo), lectura directa, custodia, clon,
run.json, FrozenCommandGuard, RecordProbe y Environment no cambian. Uso: python -I -B patch_auditor_r3.py <kit>
"""
import hashlib
import os
import sys

p = os.path.join(sys.argv[1], "post-review.py")
t = open(p, encoding="utf-8", newline="").read()
assert hashlib.sha256(t.encode("utf-8")).hexdigest().startswith("f5cdef1f"), "auditor de partida (v6.0-a7r2, f5cdef1f)"
R = []
add = lambda a, b: R.append((a, b))
add('''"""I-62, decisiones §94: post-review audit v6.0-a7r2 of ONE claude-cli run of the ONE FORMAL REVIEW of ROUND 2 of amendment A-7
(docs/initiatives/I-62-A-7.md, blob 8a528133… pinned in the closure and in owner_gate.py, PROPUESTA round 2), bounded to A62-A7-R1..R3 and their
consequences (decisions §94, point 6), by an independent ARCHITECT. Declared and self-tested BEFORE the launch (Owner CLAUDE-CLI-I62 = A; the launch
also requires gate 11, owner_gate.py: the Owner line of §78, the A-7 blob and the agreed A-6 blob at the worktree HEAD and a Coordinator disposition
AFTER §94 naming this review, pinned in owner_gate.py with its number, heading, full blob and text SHA-256, and no later section; the kit is
delivered with that authorization PENDING); the principal session runs it afterwards, never before, on the complete run, and the auditor is not
corrected after the run.
''', '''"""I-62, decisiones §96: post-review audit v7.0-a7r3 of ONE claude-cli run of the ONE FORMAL REVIEW of ROUND 3 of amendment A-7
(docs/initiatives/I-62-A-7.md, blob 8bcccb2f… pinned in the closure and in owner_gate.py, PROPUESTA round 3), bounded to A62-A7R2-R1 and its direct
consequences (decisions §96, points 2 and 6), by an independent ARCHITECT. Declared and self-tested BEFORE the launch (Owner CLAUDE-CLI-I62 = A; the
launch also requires gate 11, owner_gate.py: the Owner line of §78, the A-7 blob and the agreed A-6 blob at the worktree HEAD and a Coordinator
disposition AFTER §96 naming this review, pinned in owner_gate.py with its number, heading, full blob and text SHA-256, and no later section; the kit
is delivered with that authorization PENDING); the principal session runs it afterwards, never before, on the complete run, and the auditor is not
corrected after the run.

v7.0-a7r3 = the v6.0-a7r2 auditor (text below) with: the round-3 schema rackcad-a7r3-architect-review/0 (PriorFindingResolution of A62-A7R2-R1;
DeltaPurity WITHIN_SCOPE | OUTSIDE_SCOPE; RegressionCheck of what round 2 found corrected or conforming; CompatibilityVerdicts A5 and A6, the
explicit verdict on A-5 and A-6 of decisions §96, point 6; Q-A7R3-01..03; no OptionalDispositionConformance and no RelatedImpacts); the closure lists
PriorFindings, RegressionTopics, DeltaChanges, QuestionIds, MaterialityA7 and DirectRead (AUTOMATION_PLAN 16.7 L593-L595: the reverification with the
RebaseMap as input); the round-3 Step 0; gate 11 after §96; and the message of a non-canonical reference, which shows the reference as written and
why it is not canonical (outside the closure, or lines out of range): in round 2 the reviewer cited docs/initiatives/I-62-A7.md, relative and
well formed but not a closure file, and the old message showed its absolute resolution. Coherence of the new fields: a compatibility verdict
INCOMPATIBLE => a linked REQUIRED, COMPATIBLE => none; with a verdict, no NOT_DETERMINED and ClauseRefs in both; AGREED => A-5 and A-6 COMPATIBLE,
A62-A7R2-R1 CORRECTED, every change WITHIN_SCOPE and every topic NO_REGRESSION. The run has three files besides the faithful copies (order.txt, the
§96 body, and a7-r2.md, the round-2 text, both premises; prompt.md). Every other rule is kept.

Text of v6.0-a7r2 (with round 2's numbers):
''')
add('''AUDITOR_VERSION = "v6.0-a7r2 (claude-cli; Read/Grep/Glob only; transport gate D-01; Owner and authorization gate 11 on a Coordinator disposition after §94 pinned in owner_gate.py (pending in the kit as delivered); run.json Outcome and PostRunErrors; frozen command and record probe; direct read of AUTOMATION_PLAN L616-L617; faithful copies RACKCAD-COPIA-FIEL/1 of the files Read cannot deliver faithfully, decisions §94, point 5; round-2 schema rackcad-a7r2-architect-review/0; declared and self-tested before the launch; decisions §94)"''',
    '''AUDITOR_VERSION = "v7.0-a7r3 (claude-cli; Read/Grep/Glob only; transport gate D-01; Owner and authorization gate 11 on a Coordinator disposition after §96 pinned in owner_gate.py (pending in the kit as delivered); run.json Outcome and PostRunErrors; frozen command and record probe; direct read of AUTOMATION_PLAN L593-L595; faithful copies RACKCAD-COPIA-FIEL/1 of the files Read cannot deliver faithfully; round-3 schema rackcad-a7r3-architect-review/0 with the explicit A-5 and A-6 compatibility verdicts; non-canonical references shown as written; declared and self-tested before the launch; decisions §96)"''')
add('''    ROUND1, OPTIONALS, QUESTIONS = list(CLOSURE["Round1Findings"]), list(CLOSURE["Round1Optionals"]), list(CLOSURE["QuestionIds"])''',
    '''    ROUND1, OPTIONALS, QUESTIONS = list(CLOSURE["PriorFindings"]), [], list(CLOSURE["QuestionIds"])   # round 3: A62-A7R2-R1; no optionals''')
add('''            add("preflight: compuerta 11 (%s) con la autorización pendiente (owner_gate.py del kit sin la disposición posterior a §94 fijada, o "''',
    '''            add("preflight: compuerta 11 (%s) con la autorización pendiente (owner_gate.py del kit sin la disposición posterior a §96 fijada, o "''')
add('''                "owner_gate.py (sección posterior a §94, encabezado, blob completo y SHA-256 del texto) o con una sección posterior a ella" % OG.CHECK)''',
    '''                "owner_gate.py (sección posterior a §96, encabezado, blob completo y SHA-256 del texto) o con una sección posterior a ella" % OG.CHECK)''')
add('''        shown = "%s:%s" % (rel if kind in ("CLOSURE", "RUN") else userless(rel), m.group(2) + ("-" + m.group(3) if m.group(3) else ""))
        ref_check.append({"Group": group, "Ref": shown, "Kind": kind, "Lines": [a, b], "InRange": in_range,
                          "ExcerptFound": (found if excerpt is not None else None), "DeliveredFaithfully": delivered_ok})
        if not in_range:
            add("referencia no canónica en %s (%s)" % (group, shown))''',
    '''        shown = "%s:%s" % (rel if kind in ("CLOSURE", "RUN") else userless(rel), m.group(2) + ("-" + m.group(3) if m.group(3) else ""))
        written = userless(str(ref))
        ref_check.append({"Group": group, "Ref": shown, "Written": written, "Kind": kind, "Lines": [a, b], "InRange": in_range,
                          "ExcerptFound": (found if excerpt is not None else None), "DeliveredFaithfully": delivered_ok})
        if not in_range:
            # v7.0-a7r3: the reference as written and why (decisions §96, point 6); the rule itself is unchanged
            why = "líneas fuera de rango del archivo" if kind == "CLOSURE" or (kind == "RUN" and rel in PREMISE_RUN) else \\
                "no es un archivo del cierre ni una premisa del run"
            add("referencia no canónica en %s (%s: %s)" % (group, written, why))''')
add('''STEP0_KEYS = ("AP168DirectReadL616L617", "OrderReadWhole", "Decisions94DeclaresOrderBytesAndSha256",
              "Decisions94AcceptsR1R3AndAuthorizesSecondCandidate", "GuardsRunModeBaseHeadResult", "PreflightSummaryCandidateAndTotals",
              "Round1OutputIdentifiesRunAndFindings")''',
    '''STEP0_KEYS = ("AP167DirectReadL593L595", "OrderReadWhole", "Decisions96DeclaresOrderBytesAndSha256",
              "Decisions96AcceptsR2R1AndAuthorizesThirdCandidate", "GuardsRunModeBaseHeadResult", "PreflightSummaryCandidateAndTotals",
              "Round2OutputIdentifiesRunAndFindings")''')
add('''ARRAYS = (("Round1FindingResolution", "Round1Finding"), ("RegressionCheck", "Topic"), ("QuestionAnswers", "Question"),
          ("OptionalDispositionConformance", "Round1Optional"))''',
    '''ARRAYS = (("PriorFindingResolution", "PriorFinding"), ("RegressionCheck", "Topic"), ("QuestionAnswers", "Question"))''')
add('''SINGLES = ("Materiality", "OwnerReservedConsequence", "DeltaPurity", "RelatedImpacts")
NESTED_SINGLES = ()''',
    '''SINGLES = ("Materiality", "OwnerReservedConsequence", "DeltaPurity")
NESTED_SINGLES = (("CompatibilityVerdicts", "A5"), ("CompatibilityVerdicts", "A6"))
COMPAT_KEYS = ("A5", "A6")''')
# coherence
add('''    rr, rg, qa = items(result.get("Round1FindingResolution")), items(result.get("RegressionCheck")), items(result.get("QuestionAnswers"))
    od = items(result.get("OptionalDispositionConformance"))''',
    '''    rr, rg, qa = items(result.get("PriorFindingResolution")), items(result.get("RegressionCheck")), items(result.get("QuestionAnswers"))
    od = []                                                                        # round 3: no optional dispositions
    cv = dct(result.get("CompatibilityVerdicts"))
    compat = [("CompatibilityVerdicts.%s" % k, dct(cv.get(k))) for k in COMPAT_KEYS]''')
add('''    orc, ri, ia, step0 = dct(result.get("OwnerReservedConsequence")), dct(result.get("RelatedImpacts")), dct(result.get("IfAgreed")), \\
        dct(result.get("Step0"))
    owner, changed = orc.get("OwnerDecisionRequired"), ri.get("ChangedByRound2")
    arrays = (("Round1FindingResolution", rr, "Round1Finding", ROUND1), ("RegressionCheck", rg, "Topic", REGRESSION),''',
    '''    orc, ia, step0 = dct(result.get("OwnerReservedConsequence")), dct(result.get("IfAgreed")), dct(result.get("Step0"))
    owner = orc.get("OwnerDecisionRequired")
    arrays = (("PriorFindingResolution", rr, "PriorFinding", ROUND1), ("RegressionCheck", rg, "Topic", REGRESSION),''')
add('''    singles = (("Materiality", mat), ("OwnerReservedConsequence", orc), ("DeltaPurity", dp), ("RelatedImpacts", ri))
    optional = [("OptionalDispositionConformance[%s]" % x.get("Round1Optional"), x) for x in od]''',
    '''    singles = (("Materiality", mat), ("OwnerReservedConsequence", orc), ("DeltaPurity", dp)) + tuple(compat)
    optional = []''')
add('''    for label, xs, key in [(a, b, k) for a, b, k, _ in arrays] + [("OptionalDispositionConformance", od, "Round1Optional")]:''',
    '''    for label, xs, key in [(a, b, k) for a, b, k, _ in arrays]:''')
add('''            c.append("Round1FindingResolution[%s] %s sin el REQUIRED residual vinculado" % (x.get("Round1Finding"), x.get("Status")))''',
    '''            c.append("PriorFindingResolution[%s] %s sin el REQUIRED residual vinculado" % (x.get("PriorFinding"), x.get("Status")))''')
add('''            c.append("Round1FindingResolution[%s] CORRECTED con un REQUIRED vinculado" % x.get("Round1Finding"))''',
    '''            c.append("PriorFindingResolution[%s] CORRECTED con un REQUIRED vinculado" % x.get("PriorFinding"))''')
add('''        if x.get("Assessment") == "OUTSIDE_R1_R3" and not has_req(x):
            c.append("DeltaPurity.Changes[%s] OUTSIDE_R1_R3 sin un REQUIRED vinculado (decisiones §94, punto 2)" % x.get("Change"))
        if x.get("Assessment") == "WITHIN_R1_R3" and has_req(x):
            c.append("DeltaPurity.Changes[%s] WITHIN_R1_R3 con un REQUIRED vinculado" % x.get("Change"))
    if dp.get("Status") == "CONFIRMED" and any(x.get("Assessment") == "OUTSIDE_R1_R3" for x in ch):
        c.append("DeltaPurity CONFIRMED con un cambio OUTSIDE_R1_R3")
    if dp.get("Status") == "NOT_CONFIRMED" and ch and all(x.get("Assessment") == "WITHIN_R1_R3" for x in ch):
        c.append("DeltaPurity NOT_CONFIRMED con todos los cambios WITHIN_R1_R3")''',
    '''        if x.get("Assessment") == "OUTSIDE_SCOPE" and not has_req(x):
            c.append("DeltaPurity.Changes[%s] OUTSIDE_SCOPE sin un REQUIRED vinculado (decisiones §96, punto 2)" % x.get("Change"))
        if x.get("Assessment") == "WITHIN_SCOPE" and has_req(x):
            c.append("DeltaPurity.Changes[%s] WITHIN_SCOPE con un REQUIRED vinculado" % x.get("Change"))
    if dp.get("Status") == "CONFIRMED" and any(x.get("Assessment") == "OUTSIDE_SCOPE" for x in ch):
        c.append("DeltaPurity CONFIRMED con un cambio OUTSIDE_SCOPE")
    if dp.get("Status") == "NOT_CONFIRMED" and ch and all(x.get("Assessment") == "WITHIN_SCOPE" for x in ch):
        c.append("DeltaPurity NOT_CONFIRMED con todos los cambios WITHIN_SCOPE")
    for label, x in compat:
        if x.get("Status") == "INCOMPATIBLE" and not has_req(x):
            c.append("%s INCOMPATIBLE sin un REQUIRED vinculado (decisiones §96, punto 6)" % label)
        if x.get("Status") == "COMPATIBLE" and has_req(x):
            c.append("%s COMPATIBLE con un REQUIRED vinculado" % label)''')
add('''    for x in od:
        if x.get("Status") == "DOES_NOT_CONFORM" and not linked(x):
            c.append("OptionalDispositionConformance[%s] DOES_NOT_CONFORM sin un hallazgo vinculado" % x.get("Round1Optional"))
''', '')
add('''    if changed is True and not str(ri.get("Note") or "").strip():
        c.append("RelatedImpacts.ChangedByRound2 = true sin Note")
''', '')
add('''        undetermined = [k for k, bad in (("DeltaPurity.Status", dp.get("Status") == NOT_DET), ("RelatedImpacts.ChangedByRound2", changed is None)) if bad]''',
    '''        undetermined = [k for k, bad in [("DeltaPurity.Status", dp.get("Status") == NOT_DET)] +
                        [("%s.Status" % label, x.get("Status") in (NOT_DET, None)) for label, x in compat] if bad]''')
add('''        c.append("AGREED exige A62-A7-R1, R2 y R3 en CORRECTED")
    if v == "AGREED" and (dp.get("Status") != "CONFIRMED" or len(ch) != len(CHANGES) or any(x.get("Assessment") != "WITHIN_R1_R3" for x in ch)):
        c.append("AGREED exige DeltaPurity CONFIRMED con los diez cambios WITHIN_R1_R3")
    if v == "AGREED" and (len(rg) != len(REGRESSION) or any(x.get("Status") != "NO_REGRESSION" for x in rg)):
        c.append("AGREED exige los cinco temas de RegressionCheck en NO_REGRESSION")''',
    '''        c.append("AGREED exige el hallazgo previo A62-A7R2-R1 en CORRECTED")
    if v == "AGREED" and (dp.get("Status") != "CONFIRMED" or len(ch) != len(CHANGES) or any(x.get("Assessment") != "WITHIN_SCOPE" for x in ch)):
        c.append("AGREED exige DeltaPurity CONFIRMED con todos los cambios de la ronda 3 WITHIN_SCOPE")
    if v == "AGREED" and (len(rg) != len(REGRESSION) or any(x.get("Status") != "NO_REGRESSION" for x in rg)):
        c.append("AGREED exige todos los temas de RegressionCheck en NO_REGRESSION")
    if v == "AGREED" and any(x.get("Status") != "COMPATIBLE" for _, x in compat):
        c.append("AGREED exige A-5 y A-6 COMPATIBLE (CompatibilityVerdicts, decisiones §96, punto 6)")''')
for a, b in R:
    assert t.count(a) == 1, ("no único", t.count(a), a[:100])
    t = t.replace(a, b)
open(p, "w", encoding="utf-8", newline="").write(t)
print("ok", hashlib.sha256(t.encode("utf-8")).hexdigest())
