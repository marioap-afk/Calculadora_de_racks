"""Paso B de make_closure.py (kit de la RONDA 4 de A-7; decisiones §98, punto 7) sobre la salida del paso A: alcance y descripciones de la ronda 4,
constantes fijadas (árboles, decisiones, opcionales, temas, cambios, preguntas, fuentes L1-L27 con los contratos del Controller, lectura directa de
RAE §10 L219-L226), rangos de A-7 (§3.5 con los residuos de O1 y O2; §3.6, §3.7 y §3.8 con la matriz), de AUTOMATION_PLAN (16.4, 16.7 L598, 16.9
L634-L635), de RAE y de decisiones (§97 y §98), la revisión de la ronda 3 (ACCREDITED) como evidencia previa y el renderizado con los marcadores de
la plantilla de la ronda 4. Uso: python -I -B patch_make_closure_r4.py
"""
import hashlib, os

PREP = os.path.dirname(os.path.abspath(__file__))
SH, KIT = os.path.join(PREP, 'shifted'), os.path.join(os.path.dirname(PREP), 'kit')
t = open(os.path.join(SH, 'make_closure.py'), encoding='utf-8', newline='').read()
R = []


def add(old, new, count=1):
    R.append((old, new, count))


# ---------------- docstring
add('y a sus consecuencias (decisiones §98,\npuntos 2, 3 y 6), en una celda', 'y a sus consecuencias (decisiones §98,\npuntos 2 y 7), en una celda')
add('(§98, punto 7: «Preparar el kit acotado, pero no lanzar una nueva\nrevisión formal sin disposición posterior»).',
    '(§98, punto 7: «Preparar el kit acotado, pero no ejecutar\nrevisión formal sin nueva disposición»).')
add('la lectura directa (AUTOMATION_PLAN 16.7, L593-L595)', 'la lectura directa (RAE §10, L219-L226: los controles negativos sobre las entradas de la verificación VERIFIED)')
add('''la
    tabla de §3.3, §3.7 y §3.8 (correcciones de las rondas 2 y 3, fila por fila), las viñetas de §3.1, §3.4 y §6, los párrafos de §3.5, las tablas
    de §4, §5, §8 (Q-A7-01..06, Q-A7R3-01..03 y Q-A7R4-01..03) y §11;''', '''la
    tabla de §3.3, §3.6, §3.7 y §3.8 (correcciones de las rondas 2, 3 y 4, fila por fila, y la matriz de modalidades), las viñetas de §3.1, §3.4
    y §6, los párrafos de §3.5 (con los residuos de A62-A7R3-O1 y O2), las tablas de §4, §5, §8 (Q-A7-01..06, Q-A7R2-01..03, Q-A7R3-01..03 y
    Q-A7R4-01..03) y §11;''')
add('''comprobado: q0-q8, el RebaseMap en q8, las
    guardas de la ronda 4, su autoprueba y su resultado `run`);''', '''comprobado: q0-q10, el RebaseMap de la reverificación
    en q8, los ocho controles sobre sus entradas en q9, las modalidades, O1 y O2 en q10, la matriz modalities.md, las guardas de la ronda 4, su
    autoprueba y su resultado `run`);''')
add('sin cambio desde la ronda 3: la evidencia de R1-R3 corregidos);', 'sin cambio desde la ronda 3: la evidencia de la reverificación misma);')
add('(output.json con A62-A7R3-R1 y A62-A7R3-O1..O4, audit.json', '(output.json con A62-A7R3-R1 y A62-A7R3-O1..O3, audit.json')
add('''(V14 §20.3.1, B.8.7 y B.9; AUTOMATION_PLAN 16.3, 16.6, 16.7, 16.8, 16.9, 16.13 y 16.24; RAE §10 y
    §15.2; A-6, reglas citadas; el esquema relay-record/v2)''', '''(V14 §20.3.1, B.8.7, B.9 y D.3; AUTOMATION_PLAN 16.3, 16.4, 16.6, 16.7, 16.8, 16.9, 16.13 y 16.24; RAE
    §10 y §15.2; los contratos del Controller de FX-02; A-6, reglas citadas; el esquema relay-record/v2)''')
add('(I-62-evidence.md, §192-§197 como orientación', '(I-62-evidence.md, §192-§203 como orientación')
add('''el preflight de la ronda 1 (I-62-A7/ fuera de r2/ y r3/) y las disposiciones
  propuestas de O1-O8 (r2-dispositions/); la revisión de la ronda 1;''', '''los preflights de las rondas 1 y 2 (I-62-A7/ fuera de r3/ y r4/) y las
  disposiciones propuestas de O1-O8 (r2-dispositions/); las revisiones de las rondas 1 y 2;''')
# ---------------- constantes
add('# preflight de la ronda 1 (FUERA del cierre) y, debajo, los de las rondas 2 y 3', '# preflights de las rondas 1 y 2 (FUERA del cierre) y, debajo, los de las rondas 3 y 4')
add('A7R4_Q6 = A7R4_DIR + "/q6-compat.json"', 'A7R4_Q6 = A7R4_DIR + "/q6-compat.json"\nA7R4_Q9, A7R4_MODALITIES = A7R4_DIR + "/q9-controls-on-rv.json", A7R4_DIR + "/modalities.md"')
add('''    A7R3_DIR: ("85f0a0997634ce11156097b2d47155718b94118d", "b432c40f8ca7b1e915dff531cb9a8cad6466bb0c"),
    A7R4_DIR: ("656af47e9d614f50211cf2bee70bc3eff48f3df2", "94a04af7c663eac44016b4011cdffead714a530a"),
    R2_REVIEW: ("a07efc3761539ede9ba168a6ea5e5a93e2288f5b", "3f912c84465468dda7625470c7fe8d605ae60e06"),''',
    '''    A7R3_DIR: ("656af47e9d614f50211cf2bee70bc3eff48f3df2", "b9bfae50a82ba59c834658e54d7699662ee298bb"),
    A7R4_DIR: ("e65b166b091e2a40f01a3512c55d6e0dbb40a5c2", "94a04af7c663eac44016b4011cdffead714a530a"),
    R2_REVIEW: ("f5869bff83f07dba13559d77d53761ffd9654bc0", "068d3368a8034277404bdb0aac28891907b2d156"),''')
add('DECISIONS_PREFIX_BLOB = "5e9bff9821fb03170785cd504336e6481fad6ba0"   # decisiones en 6b00b2bf (§97 registrada) y en 3f912c84: prefijo exacto de las del clon',
    'DECISIONS_PREFIX_BLOB = "05757d9d39a14cda6f2f4417b10d4a9145f3c71c"   # decisiones en 92aa4d0c (§97 registrada) y en 068d3368: prefijo exacto de las del clon')
add('DECISIONS_AT_PUBLICATION = "7d86b98480e7981fd315f1852ed55d21f4ae7587"', 'DECISIONS_AT_PUBLICATION = "a11371b052543e85aac62096ead6c39b98cdb157"')
add('PRIOR_OPTIONALS = ["A62-A7R3-O%d" % i for i in range(1, 5)]                                      # no se incorporan (A-7 §3.8; §98, punto 2)',
    'PRIOR_OPTIONALS = ["A62-A7R3-O%d" % i for i in range(1, 4)]                                      # residuos declarados (A-7 §3.5 y §3.8; §98, punto 5)')
add('''REGRESSION_TOPICS = ["R1_TRX_AND_RELAYFACTS_AS_CORRECTED", "R2_INVALIDATOR_RULE_AND_PROCEDURE", "R3_A62_RULE1_PRECISION",
                     "T04_EFF_EFF1_B1_MAP_VALIDATION", "T05_MAP_INVALID_ABSENT_NONCONSTRUCTIBLE", "T06_CLAUSEMAP_COVERAGE_RULE",
                     "T07_MULTI_CATEGORY_ENTRIES", "T09_NO_UNAUTHORIZED_CHANGES"]   # R1-R3 corregidos en la ronda 3 y los temas CONFORMS de la ronda 1''',
    '''REGRESSION_TOPICS = ["R2R1_REVERIFICATION_RECORD_AS_CORRECTED", "R2R1_NON_ADMISSION_ORDINARY_VERIFY_AND_MEASUREMENT", "Q_A7R3_02_DELIVERY_FAIL_CLOSED",
                     "Q_A7R3_03_CATEGORY_B_BASESHA", "R1_TRX_AND_RELAYFACTS_AS_CORRECTED", "R2_INVALIDATOR_RULE_AND_PROCEDURE", "R3_A62_RULE1_PRECISION",
                     "T04_EFF_EFF1_B1_MAP_VALIDATION", "T05_MAP_INVALID_ABSENT_NONCONSTRUCTIBLE", "T06_CLAUSEMAP_COVERAGE_RULE",
                     "T07_MULTI_CATEGORY_ENTRIES", "T09_NO_UNAUTHORIZED_CHANGES"]   # lo que la ronda 3 encontró corregido o conforme''')
add('''DELTA_CHANGES = ["R3R1_REBASE_ROLE_REVERIFICATION_ONLY", "R3R1_REBASE_PATH_AND_RUNID", "R3R1_REBASE_COHERENCE", "R3R1_MAINSHA_EVAL_FROM_REBASEMAP",
                 "R3R1_ABSENCE_AND_NON_ADMISSION", "S33_ROW_C_AND_S34_C6_NOTE", "DESCRIPTIVE_SECTIONS", "PROCEDURE_PIECES_ROUND3"]   # A-7 §3.8 y el preflight de la ronda 4''',
    '''DELTA_CHANGES = ["R3R1_REBASE_COPY_IN_REVERIFICATION_CONTROLS", "R3R1_CONTROL_IDENTIFIES_REVERIFICATION", "R3R1_CONTROL_LINK", "R3R1_RECORD_COPY_AND_PATH",
                 "R3R1_COPIES_AND_MUTATIONS", "R3R1_ANCHORED_COHERENCE_AND_MAINSHA_EVAL", "R3R1_ABSENCE_AND_NON_ADMISSION", "S33_S34_S4_WITH_O3",
                 "S35_RESIDUALS_O1_O2", "DESCRIPTIVE_SECTIONS", "PROCEDURE_PIECES_ROUND4"]   # A-7 §3.8 y el preflight de la ronda 4''')
add('R2_QUESTION_IDS = ["Q-A7R3-0%d" % i for i in range(1, 4)]', 'R2_QUESTION_IDS = ["Q-A7R2-0%d" % i for i in range(1, 4)]\nR3_QUESTION_IDS = ["Q-A7R3-0%d" % i for i in range(1, 4)]')
add('LIT_IDS = ["L%d" % i for i in range(1, 23)]', 'LIT_IDS = ["L%d" % i for i in range(1, 28)]')
add('LIT_SOURCES = {"V14": V14, "AP": AP, "RAE": RAE, "A-6": A6, "esquema": RR2}', 'LIT_SOURCES = {"V14": V14, "AP": AP, "RAE": RAE, "A-6": A6, "esquema": RR2, "contratos": CC}')
add('DIRECT_READ = {"Path": AP, "Start": 593, "End": 595, "Token": "**Después de escribir, sin conflictos:**"}   # AP 16.7 (L19; A7-1, punto 1 (c) y 2 (c))',
    'DIRECT_READ = {"Path": RAE, "Start": 219, "End": 226, "Token": "Se ejecutan una sola vez"}   # RAE §10 (L15 y L23; A7-1, punto 2 (c) de la ronda 4)')
# ---------------- rangos
add('''        para(L, s["3.5"], "**Residuo declarado", "§3.5, residuo declarado (clausemap)"),
        table_at(L, s["3.6"], lambda x: x.startswith("| Hallazgo de la ronda 1 |"), "§3.7, tabla de correcciones de la ronda 3")] + [
        row(L, s["3.6"], (lambda k: lambda x: x.startswith("| %s" % k))(k), "§3.7, fila %s" % k.rstrip(":"))
        for k in ("R1:", "R2:", "R3:", "O1:", "O7:", "O2, O3-O6, O8 |")] + [
        para(L, s["3.7"], "Cambio normativo solo por A62-A7R3-R1", "§3.8, alcance de la corrección de la ronda 4"),
        table_at(L, s["3.7"], lambda x: x.startswith("| Hallazgo de la ronda 3 |"), "§3.8, tabla de la corrección de la ronda 4"),
        row(L, s["3.7"], lambda x: x.startswith("| A62-A7R3-R1:"), "§3.8, fila A62-A7R3-R1")] + [''',
    '''        para(L, s["3.5"], "**Residuo declarado (lecturas de `clausemap`", "§3.5, residuo declarado (clausemap)"),
        para(L, s["3.5"], "**Residuo declarado (C1 del aceptante", "§3.5, residuo declarado (A62-A7R3-O1)"),
        para(L, s["3.5"], "**Residuo declarado (segunda recuperación", "§3.5, residuo declarado (A62-A7R3-O2)"),
        table_at(L, s["3.6"], lambda x: x.startswith("| Hallazgo de la ronda 1 |"), "§3.6, tabla de correcciones de la ronda 2")] + [
        row(L, s["3.6"], (lambda k: lambda x: x.startswith("| %s" % k))(k), "§3.6, fila %s" % k.rstrip(":"))
        for k in ("R1:", "R2:", "R3:", "O1:", "O7:", "O2, O3-O6, O8 |")] + [
        para(L, s["3.7"], "Cambio normativo solo por A62-A7R2-R1", "§3.7, alcance de la corrección de la ronda 3"),
        table_at(L, s["3.7"], lambda x: x.startswith("| Hallazgo de la ronda 2 |"), "§3.7, tabla de la corrección de la ronda 3"),
        row(L, s["3.7"], lambda x: x.startswith("| A62-A7R2-R1:"), "§3.7, fila A62-A7R2-R1"),
        para(L, s["3.8"], "Cambio normativo solo por A62-A7R3-R1", "§3.8, alcance de la corrección de la ronda 4"),
        table_at(L, s["3.8"], lambda x: x.startswith("| Hallazgo de la ronda 3 |"), "§3.8, tabla de la corrección de la ronda 4"),
        row(L, s["3.8"], lambda x: x.startswith("| A62-A7R3-R1:"), "§3.8, fila A62-A7R3-R1"),
        table_at(L, s["3.8"], lambda x: x.startswith("| Modalidad |"), "§3.8, matriz de modalidades (resumen)")] + [''')
add('        for q in R1_QUESTION_IDS + R2_QUESTION_IDS + QUESTION_IDS] + [', '        for q in R1_QUESTION_IDS + R2_QUESTION_IDS + R3_QUESTION_IDS + QUESTION_IDS] + [')
add('''        para(L, s["8."], "Respuestas de la revisión de la ronda 1", "§8, respuestas de la ronda 1"),
        para(L, s["8."], "Respuestas de la revisión de la ronda 3", "§8, respuestas de la ronda 3"),''',
    '''        para(L, s["8."], "Respuestas de la revisión de la ronda 1", "§8, respuestas de la ronda 1"),
        para(L, s["8."], "Respuestas de la revisión de la ronda 2", "§8, respuestas de la ronda 2"),
        para(L, s["8."], "Respuestas de la revisión de la ronda 3", "§8, respuestas de la ronda 3"),''')
add('''        cited(L, DIRECT_READ["Start"], DIRECT_READ["End"], DIRECT_READ["Token"],
              "16.7, reverificación después de escribir, sin conflictos (L593-L595): LECTURA DIRECTA OBLIGATORIA"),''',
    '''        cited(L, 593, 595, "**Después de escribir, sin conflictos:**", "16.7, reverificación después de escribir, sin conflictos (L593-L595)"),
        cited(L, 598, 598, "Como máximo **dos recuperaciones**", "16.7, como máximo dos recuperaciones por TaskId (L598)"),
        cited(L, 536, 537, "(4) Worker: commit RED", "16.4, orden de commits: la verificación, la reverificación y los controles (L536-L537)"),
        cited(L, 634, 635, "| 5 | `Identity` |", "16.9, Identity y Remote tras un rebase (L634-L635)"),''')
add('''        cited(L, 219, 228, "Se ejecutan una sola vez", "§10, controles negativos (L219-L228)"),''',
    '''        cited(L, 219, 228, "Se ejecutan una sola vez", "§10, controles negativos (L219-L228)"),
        cited(L, DIRECT_READ["Start"], DIRECT_READ["End"], DIRECT_READ["Token"],
              "§10, controles negativos: copias de las entradas de la verificación VERIFIED y oráculo relativo (L219-L226): LECTURA DIRECTA OBLIGATORIA"),''')
add('''        cited(L, 44, 45, "| `AuthorityRevision` |", "AuthorityRevision y CanonicalInputs del kit (L44-L45)")]''',
    '''        cited(L, 44, 45, "| `AuthorityRevision` |", "AuthorityRevision y CanonicalInputs del kit (L44-L45)")] + literal_ranges(CC)''')
add('''[src(n) for n in (90, 91, 92, 93, 94, 95, 96)] + \\
        rows_of(90, 7) + rows_of(91, 8) + rows_of(92, 9) + rows_of(93, 9) + rows_of(94, 9) + rows_of(95, 9) + rows_of(96, 9) + \\''',
    '''[src(n) for n in (90, 91, 92, 93, 94, 95, 96, 97, 98)] + \\
        rows_of(90, 7) + rows_of(91, 8) + rows_of(92, 9) + rows_of(93, 9) + rows_of(94, 9) + rows_of(95, 9) + rows_of(96, 9) + rows_of(97, 10) + \\
        rows_of(98, 9) + \\''')
add('''R2_OUTPUT_MEMBERS = ("RunId", "ReviewedBlob", "Verdict", "RequiredFindings", "OptionalFindings", "Round1FindingResolution", "DeltaPurity",
                     "RegressionCheck", "OptionalDispositionConformance", "Materiality", "OwnerReservedConsequence", "RelatedImpacts",
                     "QuestionAnswers", "IfAgreed")''',
    '''R2_OUTPUT_MEMBERS = ("RunId", "ReviewedBlob", "Verdict", "RequiredFindings", "OptionalFindings", "PriorFindingResolution", "DeltaPurity",
                     "RegressionCheck", "CompatibilityVerdicts", "Materiality", "OwnerReservedConsequence", "QuestionAnswers", "IfAgreed")''')
# ---------------- canónicos
add('D.3 (donde A7-1 se añade, tras A6-8) y D.6; filas L1-L5 y L22 de A-7 §2', 'D.3 (donde A7-1 se añade, tras A6-8; los ocho controles con invocación, L2441) y D.6; filas L1-L5, L22 y L27 de A-7 §2')
add('16.3 (AuthorityRevision, L490), 16.4, 16.5', '16.3 (AuthorityRevision, L490), 16.4 (orden de commits, L536-L537), 16.5')
add('reverificación después de escribir, sin conflictos, L593-L595, LECTURA DIRECTA OBLIGATORIA)', 'reverificación después de escribir, sin conflictos, L593-L595; como máximo dos recuperaciones, L598)')
add('16.9 (comprobaciones, filas L630-L643)', '16.9 (comprobaciones, filas L630-L643; Identity y Remote tras un rebase, L634-L635)')
add('filas L6-L9, L16 y "\n         "L18-L20 de A-7 §2"', 'filas L6-L9, L16, "\n         "L18-L20, L25 y L26 de A-7 §2"')
add('§10 (controles negativos, L219-L228: ruta propia y copias de las entradas; oráculo relativo; nc1)',
    '§10 (controles negativos, L219-L228: sobre las entradas de la verificación VERIFIED, con ruta y registro propios y copias de las entradas; '
    'oráculo relativo; L219-L226, LECTURA DIRECTA OBLIGATORIA; nc1)')
add('filas L10 y L15 de A-7 §2', 'filas L10, L15 y L23 de A-7 §2')
add('AuthorityRevision y CanonicalInputs del kit, L44-L45)", "ENTIRE", ranges_cc', 'AuthorityRevision y CanonicalInputs del kit, L44-L45); fila L24 de A-7 §2", "ENTIRE", ranges_cc')
add('R1-R3 aceptados; §97: la ', 'R1-R3 aceptados; §95-§96: la revisión de la ronda 2 y A62-A7R2-R1 aceptado; §97: la ')
add('y sus rondas 2 y 3: hechos', 'y sus rondas 2 a 4: hechos')
add('§192-§197 como orientación, con riesgo de anclaje declarado", "SECTIONS", ranges_evidence, "§192-§197 (orientación)"',
    '§192-§203 como orientación, con riesgo de anclaje declarado", "SECTIONS", ranges_evidence, "§192-§203 (orientación)"')
# ---------------- grupos del cierre
add('''"preflight dirigido y guardas de la ronda 4 (q0: entorno, reutilización y delta exacto; q1-q5: las fases de la ronda 3 con las "
                "piezas de la ronda 4; q6: compatibilidad con A-5, A-6, EFF/EFF^1, el invalidador, el Owner, los presupuestos y los oráculos; q7: "
                "guardas G1-G7 y su autoprueba; q8: el RebaseMap de la reverificación de 16.7; constructor, validador, formato de designación "
                "a7-designation/3 y derivación del aceptante; resultado `run`): %s"''',
    '''"preflight dirigido y guardas de la ronda 4 (q0: entorno, reutilización y delta exacto; q1-q5 y q8: las fases de las rondas 2 y "
                "3 con las piezas de la ronda 4, sin regresión; q6: compatibilidad con A-5, A-6, EFF/EFF^1, el invalidador, el Owner, los "
                "presupuestos y los oráculos; q7: guardas G1-G7 y su autoprueba; q9: los ocho controles negativos sobre las entradas de la "
                "reverificación; q10: modalidades, A62-A7R3-O1 y A62-A7R3-O2; modalities.md: la matriz de modalidades; constructor, validador, "
                "formato de designación a7-designation/4 y derivación del aceptante; resultado `run`): %s"''')
add('''"preflight dirigido y guardas de la ronda 3 (sin cambio; la evidencia de R1-R3 corregidos: TRX de CurrentSha y RedSha, RelayFacts, "
                "N4 y nc1, tipo B1, identidad del invalidador, compatibilidad; a7-designation/2; resultado `run`): %s"''',
    '''"preflight dirigido y guardas de la ronda 3 (sin cambio; la evidencia de la reverificación misma: el RebaseMap en q8, su "
                "coherencia, MainSha_eval, ausencia, sustitución y uso indebido; a7-designation/3; resultado `run`): %s"''')
add('''A62-A7R3-R1 y O1..O4, resolución de R1-R3, "
                         "pureza del delta, regresión, materialidad y preguntas"''', '''A62-A7R3-R1 y O1..O3, resolución de A62-A7R2-R1, "
                         "pureza del delta, regresión, compatibilidad, materialidad y preguntas"''')
add('"auditoría de la revisión de la ronda 3 (NOT_ACCREDITED por una referencia no canónica del revisor)"',
    '"auditoría de la revisión de la ronda 3 (ACCREDITED: referencias exactas y copias fieles verificadas)"')
add('"el preflight de la ronda 1 y r2-dispositions/ no entran en el cierre de la ronda 4"', '"los preflights de las rondas 1 y 2 y r2-dispositions/ no entran en el cierre de la ronda 4"')
# ---------------- identidad
add('"decisiones del clon no crece solo por añadido desde la registrada con §97 (blob 5e9bff98, commit 6b00b2bf)"',
    '"decisiones del clon no crece solo por añadido desde la registrada con §97 (blob 05757d9d, commit 92aa4d0c)"')
add('''(summ.get("Guards") or {}).get("Result") == "PASS" and (summ.get("Selftest") or {}).get("Result") == "PASS", \\
        "summary.json de la ronda 4 no es el de la candidata con todos los casos cumplidos (q8 incluido), guardas y autoprueba PASS"''',
    '''(summ.get("Guards") or {}).get("Result") == "PASS" and (summ.get("Selftest") or {}).get("Result") == "PASS" and \\
        (summ.get("Modalities") or {}).get("Total") is True, \\
        "summary.json de la ronda 4 no es el de la candidata con todos los casos cumplidos (q8 incluido), la matriz total, guardas y autoprueba PASS"''')
add('''(aud2.get("Accreditation") or {}).get("Status") == "NOT_ACCREDITED", "la revisión de la ronda 3 custodiada no es la de A62-A7R3-R1 y O1-O4"''',
    '''(aud2.get("Accreditation") or {}).get("Status") == "ACCREDITED", "la revisión de la ronda 3 custodiada no es la de A62-A7R3-R1 y O1-O3 (ACCREDITED)"''')
add('"Verdict": "CHANGES REQUIRED", "Accreditation": "NOT_ACCREDITED"}', '"Verdict": "CHANGES REQUIRED", "Accreditation": "ACCREDITED"}')
# ---------------- renderizado
add('''        "{{A7_S35_RES}}": range_text(ga["§3.5, residuo declarado (clausemap)"]), "{{A7_S36}}": sec("3.6"), "{{A7_S37}}": sec("3.7"),
        "{{A7_S37_SCOPE}}": range_text(ga["§3.8, alcance de la corrección de la ronda 4"]),
        "{{A7_S37_ROW}}": range_text(ga["§3.8, fila A62-A7R3-R1"]), "{{A7_S4_REBASE}}": range_text(ga["§4, fila del registro de rebase"]),''',
    '''        "{{A7_S35_RES}}": range_text(ga["§3.5, residuo declarado (clausemap)"]), "{{A7_S35_O1}}": range_text(ga["§3.5, residuo declarado (A62-A7R3-O1)"]),
        "{{A7_S35_O2}}": range_text(ga["§3.5, residuo declarado (A62-A7R3-O2)"]), "{{A7_S36}}": sec("3.6"), "{{A7_S37}}": sec("3.7"), "{{A7_S38}}": sec("3.8"),
        "{{A7_S37_ROW}}": range_text(ga["§3.7, fila A62-A7R2-R1"]), "{{A7_S38_SCOPE}}": range_text(ga["§3.8, alcance de la corrección de la ronda 4"]),
        "{{A7_S38_ROW}}": range_text(ga["§3.8, fila A62-A7R3-R1"]), "{{A7_S38_MATRIX}}": range_text(ga["§3.8, matriz de modalidades (resumen)"]),
        "{{A7_S4_REBASE}}": range_text(ga["§4, fila del registro de rebase"]),''')
add('"{{A7_QR3}}": "%s-%s" % (ga["§8, Q-A7R4-01"]["Start"], ga["§8, Q-A7R4-03"]["End"]),', '"{{A7_QR4}}": "%s-%s" % (ga["§8, Q-A7R4-01"]["Start"], ga["§8, Q-A7R4-03"]["End"]),')
add('''        "{{DEC96}}": range_text(d(96)),
        "{{DEC96_SOURCE}}": range_text(dec["§98, bytes y SHA-256 del cuerpo pegado"]),
        "{{DEC96_P1}}": range_text(dec["§98, punto 1"]), "{{DEC96_P2}}": range_text(dec["§98, punto 2"]), "{{DEC96_P3}}": range_text(dec["§98, punto 3"]),
        "{{DEC96_P6}}": range_text(dec["§98, punto 6"]), "{{DEC96_P7}}": range_text(dec["§98, punto 7"]), "{{DEC96_P9}}": range_text(dec["§98, punto 9"]),''',
    '''        "{{DEC98}}": range_text(d(98)),
        "{{DEC98_SOURCE}}": range_text(dec["§98, bytes y SHA-256 del cuerpo pegado"]),
        "{{DEC98_P1}}": range_text(dec["§98, punto 1"]), "{{DEC98_P2}}": range_text(dec["§98, punto 2"]), "{{DEC98_P4}}": range_text(dec["§98, punto 4"]),
        "{{DEC98_P7}}": range_text(dec["§98, punto 7"]), "{{DEC98_P8}}": range_text(dec["§98, punto 8"]), "{{DEC98_P9}}": range_text(dec["§98, punto 9"]),''')
add('''"{{R2OUT_REG}}": "`Round1FindingResolution`, L%s, y `RegressionCheck`, L%s" % (r2m("Round1FindingResolution"), r2m("RegressionCheck")),''',
    '''"{{R2OUT_REG}}": "`PriorFindingResolution`, L%s, y `RegressionCheck`, L%s" % (r2m("PriorFindingResolution"), r2m("RegressionCheck")),''')
add('''        "{{AP_DIRECT}}": "%d-%d" % (apd["Start"], apd["End"]),
        "{{AP_DIRECT_OFFSET}}": str(apd["Start"] - 8), "{{AP_DIRECT_LIMIT}}": str(apd["End"] - apd["Start"] + 17),''',
    '''        "{{DIRECT}}": "%d-%d" % (apd["Start"], apd["End"]),
        "{{DIRECT_OFFSET}}": str(apd["Start"] - 8), "{{DIRECT_LIMIT}}": str(apd["End"] - apd["Start"] + 17),''')
add('"{{READ_AP}}": read_path(AP, flagged),', '"{{READ_DIRECT}}": read_path(DIRECT_READ["Path"], flagged),')
add('(índice de rutas, decisiones §98, punto 6)', '(índice de rutas, decisiones §96, punto 6)')
# ---------------- closure.json
add('''    sources = {"order.txt": "cuerpo exacto pegado de la disposición §98 del Coordinator (3 740 bytes; SHA-256 adb11535…, el que declara decisiones §98 "
                            "en el commit): conserva la revisión de la ronda 3 y acepta A62-A7R3-R1 (punto 1), autoriza la cuarta candidata limitada "
                            "a la admisión del RebaseMap en la reverificación de 16.7 (punto 2) y a las superficies que dependen de ella (punto 3), las "
                            "pruebas (punto 4), la compatibilidad (punto 5), el kit con referencias comprobadas y el dictamen sobre A-5 y A-6 (punto 6), "
                            "la publicación sin revisión formal sin una disposición posterior (punto 7), los límites (punto 8) y la entrega (punto 9)",''',
    '''    sources = {"order.txt": "cuerpo exacto pegado de la disposición §98 del Coordinator (3 740 bytes; SHA-256 8b73975d…, el que declara decisiones §98 "
                            "en el commit): registra la revisión de la ronda 3 y acepta A62-A7R3-R1 (punto 1), autoriza la cuarta candidata, limitada "
                            "a admitir REBASE en los controles negativos que derivan de una reverificación válida de 16.7 (punto 2), la matriz de "
                            "modalidades (punto 3), el preflight dirigido (punto 4), los opcionales (punto 5), la eficiencia (punto 6), la publicación y "
                            "el kit acotado sin revisión formal sin una disposición nueva (punto 7), los límites (punto 8) y la entrega (punto 9)",''')
add('''"Question": "decisiones §98 (order.txt), puntos 2, 3 y 6: revisión formal independiente de la ronda 4 de A-7, acotada a A62-A7R3-R1 y a sus "''',
    '''"Question": "decisiones §98 (order.txt), puntos 2 y 7: revisión formal independiente de la ronda 4 de A-7, acotada a A62-A7R3-R1 y a sus "''')
add('''"consecuencias: la resolución de A62-A7R3-R1 (el registro de rebase y su RebaseMap en la categoría (c), solo en la reverificación "
                    "de 16.7: origen y ruta, identidad y blob, RunId, fidelidad, ausencia, sustitución u otra corrida, rechazo ante incoherencias, "
                    "MainSha_eval, no admisión en una VERIFY ordinaria); la pureza del delta (cambios normativos solo por A62-A7R3-R1; los descriptivos "
                    "de §3.8); la ausencia de regresión en lo que la ronda 3 encontró corregido o conforme; el dictamen expreso sobre la "
                    "compatibilidad con A-5 y con A-6 (punto 6); la reconfirmación de la materialidad y de las consecuencias OWNER-RESERVED; las "
                    "preguntas Q-A7R4-01..03. Los opcionales A62-A7R3-O1..O4 no se incorporan ni se revisan. AGREED solo con cero REQUIRED; sin "
                    "acreditación excepcional"''',
    '''"consecuencias: la resolución de A62-A7R3-R1 (la copia del registro de rebase en los controles negativos con invocación sobre las "
                    "entradas de una reverificación de 16.7: identidad de la reverificación, vínculo de RunId, tarea e intento, copia del registro y "
                    "del RebaseMap, MainSha_eval de la reverificación, copias sin sustituciones, mutaciones de cada control, rechazo de lo ajeno; "
                    "no admisión en la VERIFY ordinaria, sus controles y la medición); la pureza del delta (cambios normativos solo por A62-A7R3-R1; "
                    "los descriptivos de §3.8, con los residuos de O1 y O2 y la enumeración de O3); la ausencia de regresión en lo que la ronda 3 "
                    "encontró corregido o conforme; el dictamen sobre la compatibilidad con A-5 y con A-6 (punto 4); la reconfirmación de la "
                    "materialidad y de las consecuencias OWNER-RESERVED; las preguntas Q-A7R4-01..03. AGREED solo con cero REQUIRED; sin "
                    "acreditación excepcional"''')
add('(transport_gate.py byte a byte el de A-6 ronda 4 y de la ronda 4:', '(transport_gate.py byte a byte el de A-6 ronda 4 y el de las rondas 1 a 3 de A-7:')
add('el revisor contrasta lo que puede leer: AUTOMATION_PLAN L593-L595, order.txt', 'el revisor contrasta lo que puede leer: RAE §10 L219-L226, order.txt')
add('"decisiones §98, punto 6 («Exigir referencias relativas válidas', '"decisiones §96, punto 6 («Exigir referencias relativas válidas')
add('''"DirectRead": "AUTOMATION_PLAN 16.7 L593-L595 se leen con Read en docs/AUTOMATION_PLAN.md o en su copia fiel (Paso 0) y constan en "
                          "DirectReads",''',
    '''"DirectRead": "RAE §10 L219-L226 se leen con Read en docs/automation/agent-execution/README.md o en su copia fiel (Paso 0) y constan "
                          "en DirectReads",''')
add('''{"Source": "el preflight de la ronda 1 (docs/automation/evidence/I-62-A7/ fuera de r2/ y r3/) y las disposiciones propuestas de O1-O8 "''',
    '''{"Source": "los preflights de las rondas 1 y 2 (docs/automation/evidence/I-62-A7/ fuera de r3/ y r4/) y las disposiciones propuestas de O1-O8 "''')
add('''"Reason": "revisión acotada a A62-A7R3-R1 (decisiones §98, puntos 2 y 6); ni A-7 ni los preflights de las rondas 2 y 3 los citan, y la "
                       "conformidad de la ronda 1 que se comprueba sin regresión consta en la salida de la revisión de la ronda 3"},''',
    '''"Reason": "revisión acotada a A62-A7R3-R1 (decisiones §98, punto 2); los preflights de las rondas 3 y 4 no los citan, y lo que la "
                       "ronda 3 encontró corregido o conforme, que se comprueba sin regresión, consta en la salida de su revisión"},''')
add('{"Source": "la revisión de la ronda 1 (R20261019T010200Z-4a72); el kit,', '{"Source": "las revisiones de las rondas 1 y 2 (R20261019T010200Z-4a72 y R20261019T201030Z-d1bf); el kit,')
add('''con la resolución de R1-R3 de la "
                       "ronda 1; el resto''', '''con la resolución de A62-A7R2-R1; "
                       "el resto''')

for i, (old, new, count) in enumerate(R):
    n = t.count(old)
    if n != count:
        raise SystemExit('edición %d: %d apariciones (se esperaban %d): %r' % (i, n, count, old[:110]))
    t = t.replace(old, new)
b = t.encode('utf-8')
assert b'\r' not in b
open(os.path.join(KIT, 'make_closure.py'), 'wb').write(b)
print('make_closure.py', hashlib.sha256(b).hexdigest(), len(R), 'ediciones')
