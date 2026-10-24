"""Paso B de selftest-post-review.py (kit de la RONDA 4 de A-7; decisiones §98, punto 7) sobre la salida del paso A: la lectura directa de la ronda 4
(RAE §10 L219-L226, tomada del cierre: DirectRead), los nombres de los once cambios y de los doce temas de la ronda 4 en los casos, las filas de A-7 de
REG_ROWS, la sección §98 del Paso 0 sintético, el caso F08 sobre RAE (que en la ronda 4 no tiene copia fiel: la lectura directa en el original) y la
docstring. Los casos de transporte, lectura, custodia, lanzador, compuertas y fidelidad no cambian salvo nombres. Uso: python -I -B patch_selftest_r4.py
"""
import hashlib, os

PREP = os.path.dirname(os.path.abspath(__file__))
SH, KIT = os.path.join(PREP, 'shifted'), os.path.join(os.path.dirname(PREP), 'kit')
t = open(os.path.join(SH, 'selftest-post-review.py'), encoding='utf-8', newline='').read()
R = []


def add(old, new, count=1):
    R.append((old, new, count))


# ---- docstring
add('(decisions §98, points 2, 3 and 6).', '(decisions §98, points 2 and 7).')
add('the direct read is AUTOMATION_PLAN 16.7 L593-L595) and the result', 'the direct read is RAE §10 L219-L226) and the result')
add('explicit verdict of decisions §98, point 6:', 'verdict of decisions §98, point 4:')
add('the direct read of\n    AUTOMATION_PLAN L593-L595 and the references', 'the direct read of\n    RAE §10 L219-L226 and the references')
add('  * the reference mechanism of decisions §98, point 6 («Exigir', '  * the reference mechanism of decisions §96, point 6 («Exigir')
add('the misspelled path of round 3 docs/initiatives/I-62-A7.md', 'the misspelled path of round 2 docs/initiatives/I-62-A7.md')
add('the direct read of AUTOMATION_PLAN through its copy;', 'the direct read of RAE §10 on its original (RAE has no faithful copy in round 4);')
add('clone D:\\\\r62-arch-a7r3 and run D:\\\\r62-arch-a7r3-run.', 'clone D:\\\\r62-arch-a7r4 and run D:\\\\r62-arch-a7r4-run.')
# ---- constantes
add('OTHER_A6_BLOB = "e62f27b8b1329f1b9cfa69a707feb31d22bad1d1"   # la A-6 de la ronda 4: nunca la acordada',
    'OTHER_A6_BLOB = "e62f27b8b1329f1b9cfa69a707feb31d22bad1d1"   # la A-6 de la ronda 3: nunca la acordada')
add('''AP_DIRECT = (593, 595)                        # AUTOMATION_PLAN 16.7, the reverification «después de escribir, sin conflictos» (L19 of A-7 §2)
AP_READ = (585, 603)                          # the direct read of Step 0 (AUTOMATION_PLAN 16.7 L593-L595 inside; the prompt's offset and limit)''',
    '''DIRECT_FILE = _CLOSURE["DirectRead"]["Path"]  # round 4: RAE §10 (docs/automation/agent-execution/README.md; L15 and L23 of A-7 §2)
AP_DIRECT = (_CLOSURE["DirectRead"]["Start"], _CLOSURE["DirectRead"]["End"])   # L219-L226: the controls on the inputs of the VERIFIED verification
AP_READ = (AP_DIRECT[0] - 8, AP_DIRECT[1] + 8)   # the direct read of Step 0 (the prompt's offset and limit, with L219-L226 inside)''')
add('''REG_ROWS = {"R1_TRX_AND_RELAYFACTS_AS_CORRECTED": "§3.7, fila R1", "R2_INVALIDATOR_RULE_AND_PROCEDURE": "§3.7, fila R2",
            "R3_A62_RULE1_PRECISION": "§3.7, fila R3",''',
    '''REG_ROWS = {"R2R1_REVERIFICATION_RECORD_AS_CORRECTED": "§3.7, fila A62-A7R2-R1",
            "R2R1_NON_ADMISSION_ORDINARY_VERIFY_AND_MEASUREMENT": "§4, fila del registro de rebase",
            "Q_A7R3_02_DELIVERY_FAIL_CLOSED": "§8, Q-A7R3-02", "Q_A7R3_03_CATEGORY_B_BASESHA": "§8, Q-A7R3-03",
            "R1_TRX_AND_RELAYFACTS_AS_CORRECTED": "§3.6, fila R1", "R2_INVALIDATOR_RULE_AND_PROCEDURE": "§3.6, fila R2",
            "R3_A62_RULE1_PRECISION": "§3.6, fila R3",''')
add('"Rationale": "x", "ClauseRefs": [sec_ref("### 3.7 ")], "FindingIds": []},', '"Rationale": "x", "ClauseRefs": [sec_ref("### 3.8 ")], "FindingIds": []},')
# ---- la lectura directa (RAE §10) y el Paso 0 sintético con §98
add('    read_range(r, AP, *AP_READ)\n', '    read_range(r, DIRECT_FILE, *AP_READ)\n')
add("ref(AP, *AP_DIRECT)", "ref(DIRECT_FILE, *AP_DIRECT)", 3)
add('ref(AP, AP_DIRECT[0])', 'ref(DIRECT_FILE, AP_DIRECT[0])')
add('    read_range(r, DEC, *closure_range(DEC, "## 96. "))', '    read_range(r, DEC, *closure_range(DEC, "## 98. "))')
add('    # ---- direct read of AUTOMATION_PLAN 16.7 L593-L595 (round 4; the rule of v5.x) and the references of nested elements',
    '    # ---- direct read of RAE §10 L219-L226 (round 4; the rule of v5.x) and the references of nested elements')
add('p("121-sin-lectura-directa-de-ap"', 'p("121-sin-lectura-directa-de-rae"')
add('p("121b-cita-de-ap-leida-solo-en-a7"', 'p("121b-cita-de-rae-leida-solo-en-a7"')
add('p("121c-directreads-sin-ap"', 'p("121c-directreads-sin-rae"')
add('p("121d-directreads-cubre-solo-593"', 'p("121d-directreads-cubre-solo-la-primera-linea"')
add('p("121e-cita-de-ap-tras-la-lectura-directa-aceptada"', 'p("121e-cita-de-rae-tras-la-lectura-directa-aceptada"')
add('"121j-referencia-a-la-ronda-2-y-a-su-salida-aceptada"', '"121j-referencia-a-la-ronda-3-y-a-su-salida-aceptada"')
add('''    # F08: the direct read of AUTOMATION_PLAN 16.7 L593-L595 through its faithful copy satisfies Step 0
    def direct_by_copy(r):
        r.header()
        read_copy(r, AP, *AP_READ)''',
    '''    # F08 (round 4): the direct read of RAE §10 L219-L226, which has no faithful copy, on its original satisfies Step 0 (in round 3 it was
    # AUTOMATION_PLAN 16.7 through its faithful copy)
    def direct_by_copy(r):
        r.header()
        read_range(r, DIRECT_FILE, *AP_READ)''')
add('''    add(fvariant("F08-lectura-directa-por-la-copia-de-automation-plan", base=direct_by_copy, require_coherent=True,
                 check=lambda out: (out["DirectRead"]["DeliveredByRead"] is True and
                                    AP not in [x["Path"] for x in out["Fidelity"]["FlaggedOriginalReads"]], out["DirectRead"])))''',
    '''    add(fvariant("F08-lectura-directa-de-rae-en-su-original", base=direct_by_copy, require_coherent=True,
                 check=lambda out: (out["DirectRead"]["DeliveredByRead"] is True and
                                    DIRECT_FILE not in [x["Path"] for x in out["Fidelity"]["FlaggedOriginalReads"]], out["DirectRead"])))''')
# ---- nombres de los cambios y del Paso 0 de la ronda 4
add('R3R1_REBASE_COHERENCE', 'R3R1_ANCHORED_COHERENCE_AND_MAINSHA_EVAL', 5)
add('R3R1_MAINSHA_EVAL_FROM_REBASEMAP', 'R3R1_CONTROL_LINK', 1)
add('PROCEDURE_PIECES_ROUND3', 'PROCEDURE_PIECES_ROUND4', 4)
add('S33_ROW_C_AND_S34_C6_NOTE', 'S33_S34_S4_WITH_O3', 1)
add('R3R1_REBASE_ROLE_REVERIFICATION_ONLY', 'R3R1_REBASE_COPY_IN_REVERIFICATION_CONTROLS', 1)
add('"AP167DirectReadL593L595"', '"RAE10DirectReadL219L226"', 1)
add('    # the explicit verdict on A-5 and A-6 (decisions §98, point 6)', '    # the verdict on A-5 and A-6 (decisions §98, point 4)')
add('    # ---- the reference mechanism (decisions §98, point 6):', '    # ---- the reference mechanism (decisions §96, point 6):')
add('    # X06 (decisions §98, point 6):', '    # X06 (decisions §96, point 6):')
# ---- F00 (round 4): the object A-7 round 4 is flagged (LONG_LINE at L237, 1 941 UTF-16 units, under Read's 2 000) and the base flow reads it whole;
#      AUTOMATION_PLAN is no longer read (the direct read is RAE §10, not flagged)
add('''    # F00: the base flow reads flagged ORIGINALS (V14, decisions) only on lines Read delivers faithfully: accredited, recorded as informational
    add(fvariant("F00-base-lecturas-de-originales-marcados-informativas", require_coherent=True,
                 check=lambda out: (sorted({x["Path"] for x in out["Fidelity"]["FlaggedOriginalReads"]}) == sorted([DEC, AP]) and''',
    '''    # F00: the base flow reads flagged ORIGINALS (decisions and, in round 4, the object A-7, flagged LONG_LINE at L237 with 1 941 UTF-16 units, under
    # Read's 2 000) only on lines Read delivers faithfully: accredited, recorded as informational
    add(fvariant("F00-base-lecturas-de-originales-marcados-informativas", require_coherent=True,
                 check=lambda out: (sorted({x["Path"] for x in out["Fidelity"]["FlaggedOriginalReads"]}) == sorted([DEC, A7]) and''')
# ---- the owner-gate cases: «any later section» is §101 (round 3: §99 after the synthetic §97; round 4: the synthetic authorization is §99), and the
#      case names that still carried the base section of round 3 (96)
add(r'''    run_case("O03-seccion-99-cualquiera-posterior", dec_auth + "\n## 99. Nota (selftest)\n", [LATER])''',
    r'''    run_case("O03-seccion-101-cualquiera-posterior", dec_auth + "\n## 101. Nota (selftest)\n", [LATER])''')
add('"O00b-pendiente-con-una-seccion-posterior-a-96"', '"O00b-pendiente-con-una-seccion-posterior-a-98"')
add('"O01-autorizacion-posterior-a-96-fijada-pasa"', '"O01-autorizacion-posterior-a-98-fijada-pasa"')
add('"O10-fijada-no-posterior-a-96"', '"O10-fijada-no-posterior-a-98"')
add('"O23-decision-96-cambiada-en-la-head"', '"O23-decision-98-cambiada-en-la-head"')

for i, (old, new, count) in enumerate(R):
    n = t.count(old)
    if n != count:
        raise SystemExit('edición %d: %d apariciones (se esperaban %d): %r' % (i, n, count, old[:100]))
    t = t.replace(old, new)
b = t.encode('utf-8')
assert b'\r' not in b
open(os.path.join(KIT, 'selftest-post-review.py'), 'wb').write(b)
print('selftest-post-review.py', hashlib.sha256(b).hexdigest(), len(R), 'ediciones')
