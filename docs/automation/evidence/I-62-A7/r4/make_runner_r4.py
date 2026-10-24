# make_runner_r4.py - derives run_a7r4.py (the single entry point of the round-4 preflight of I-62 A-7; decisiones §98, puntos 3-5) from the
# round-3 runner run_a7r3.py (copied byte for byte from docs/automation/evidence/I-62-A7/r3/) by exact replacements; the new phases (q9, q10)
# and the coverage matrix live in r4phases.py. Unchanged code keeps the round-3 text.   python -I make_runner_r4.py
import hashlib, os, sys

HERE = os.path.dirname(os.path.abspath(__file__))
R3_RUNNER_SHA = None   # checked against the custodied round-3 file below
src = open(os.path.join(HERE, 'run_a7r3.py'), 'rb').read()
t = src.decode('utf-8')
R = []


def add(old, new, count=1):
    R.append((old, new, count))


add('''# run_a7r3.py - single entry point of the targeted preflight of the THIRD candidate of I-62 A-7 (round 3; decisiones §96, puntos 2-5;
# validacion incremental de §88).   python -I run_a7r3.py all --cand <I-62-A-7.md>
''', '''# run_a7r4.py - single entry point of the targeted preflight of the FOURTH candidate of I-62 A-7 (round 4; decisiones §98, puntos 3-5),
# derived from run_a7r3.py by make_runner_r4.py.   python -I run_a7r4.py all --cand <I-62-A-7.md>
# Phases: q0 environment, identities, reuse and the exact delta against round 3; q1-q5 the round-2 phases and q8 the round-3 phase (the
# RebaseMap of the 16.7 reverification), re-run with the round-4 pieces as the regression check of the cases that conformed before; q9 the
# eight negative controls on the inputs of the reverification (A62-A7R3-R1) and q10 the other modalities of the coverage matrix, A62-A7R3-O1
# and A62-A7R3-O2 (r4phases.py); q6 compatibility; q7 the round-4 scope guards; then modalities.md, summary.json, preflight.md, a privacy
# pass and MANIFEST.sha256. Round-3 text follows (with r4 for r3 in the work area and the pieces of round 4):
# run_a7r3.py - single entry point of the targeted preflight of the THIRD candidate of I-62 A-7 (round 3; decisiones §96, puntos 2-5;
# validacion incremental de §88).
''')
add('''BASE = '5da2007f45160ced37b958a3a772c46ac64874ee'
PREV_BLOB = '8a528133c7cccd83356e31f8fd79cd2ebb0cec15'     # round 2 (the base holds it)''',
    '''BASE = '2405a919b8ce42c8b0b971d97e5f20294059b1e9'
PREV_BLOB = '8bcccb2fd2cea480f73ee5bf5a1583e0c9eaf1ff'     # round 3 (the base holds it)''')
add('''R2_PIECES = {'make_eic_a7.py': '0e5f9305525f8d0e91d09127dc1a0d2238883a37b28d29b08cfaf98803584aad',
             'check_a7_closure.py': '59e26e118697bc8371e0496d8b44490a5105e74536c5e67ca56343c854bc96b1',
             'a7-designation-2.schema.json': 'ae33ad31dde5f38094d766b6a576b3f354ff2e27066fa791718a751c9bf306c0',
             'designate_a7.py': '759df4f0332b1e4557ab3460973323e3016e485d6cb92cb1793df56f01842ab9',
             'derive_r4.py': '3de12c54e3893b7c8689825f69bc8fd2a9cea767eaa6d0f77d5361f1ff29c426'}''',
    '''R3_PIECES = {'make_eic_a7.py': '8ebe617d87e19d0b703e3225184ec26090d8eb70181e65f272d0848ee1269f09',
             'check_a7_closure.py': '2afa162156ebb17aaeac0f5c3a5919efe23f87e486f2ca71b8475ef7565a0f50',
             'a7-designation-3.schema.json': 'a897f617a9ea19bdc4f6b35c6bcb44d36d864b186457e0d56752f1b1e0368e1b',
             'designate_a7.py': 'b0d4bf136e620ee5fa162f966f0ab28155ff8951ec36f1ce5bc2ae0142942222',
             'derive_r4.py': '3de12c54e3893b7c8689825f69bc8fd2a9cea767eaa6d0f77d5361f1ff29c426'}''')
add('R2_PIECES', 'R3_PIECES', 4)
add('''CAND = None
''', '''CAND = None
Q8CTX, Q9CTX = {}, {}    # round 4: the fixture of q8 and the controls of q9, shared with r4phases.py
''')
# ---- q0
add("cases.append(case('CAND', 'candidata A-7 ronda 3 leída por completo',", "cases.append(case('CAND', 'candidata A-7 ronda 4 leída por completo',")
add("cases.append(case('PREV', 'ronda 2 en la base (la que la ronda 3 corrige en el mismo archivo)',",
    "cases.append(case('PREV', 'ronda 3 en la base (la que la ronda 4 corrige en el mismo archivo)',")
add("'texto normalizado de cada punto y categoría de A7-1 frente a la ronda 2 (§96, puntos 2 y 3)',",
    "'texto normalizado de cada punto y categoría de A7-1 frente a la ronda 3 (§98, punto 2)',")
add('''ALLOWED_SECTIONS = ('HEADER', '## 2. Fuentes', '### 3.3 Categorías de entrada', '### 3.4 Relación con A-6', '### 3.6 Correcciones de la ronda 2',
                    '### 3.7 Corrección de la ronda 3', '## 4. Semántica', '## 5. Materialidad', '## 6. No cambia producción', '## 8. Preguntas',
                    '## 10. Evidencia', '## 11. Estado de revisión', '## Anexo')''',
    '''ALLOWED_SECTIONS = ('HEADER', '## 2. Fuentes', '### 3.3 Categorías de entrada', '### 3.4 Relación con A-6', '### 3.5 Lo que A-7 no hace',
                    '### 3.7 Corrección de la ronda 3', '### 3.8 Corrección de la ronda 4', '## 4. Semántica', '## 5. Materialidad',
                    '## 6. No cambia producción', '## 8. Preguntas', '## 10. Evidencia', '## 11. Estado de revisión', '## Anexo')''')
add("""'solo las secciones descriptivas que declara §3.7: cabecera, §2, §3.3, §3.4, '
                      '§3.7, §4, §5, §6, §8, §10, §11 y el Anexo'""",
    """'solo las secciones descriptivas que declara §3.8: cabecera, §2, §3.3, §3.4, '
                      '§3.5, §3.7, §3.8, §4, §5, §6, §8, §10, §11 y el Anexo'""")
add("""    cases.append(case('R3-ABSENT', 'ningún archivo de r3/ publicado en la base', 'I-62-A7/r3/ vacío', '%d archivos' % sum(1 for n in tree if '/r3/' in n),
                      not any('/r3/' in n for n in tree)))
    man = git(W, 'cat-file', 'blob', 'HEAD:docs/automation/evidence/I-62-A7/r2/MANIFEST.sha256')[1].decode('ascii')
    rows = [l.split('  ', 1) for l in man.splitlines() if l.strip()]
    bad = [r for h, r in rows if sha256(git(W, 'cat-file', 'blob', 'HEAD:docs/automation/evidence/I-62-A7/r2/' + r)[1]) != h]
    cases.append(case('R2-EVID', 'evidencia de la ronda 2 (I-62-A7/r2/) en la base frente a su MANIFEST.sha256', 'idéntica (la ronda 3 no la toca)',
                      '%d entradas; discrepancias: %s' % (len(rows), ', '.join(bad) or 'ninguna'), not bad and len(rows) >= 48))
    rv = 'docs/automation/evidence/I-62-architect-A-7/R20261019T201030Z-d1bf/'
    t1 = gtext(W, 'rev-parse', '3f912c84:' + rv)
    t2 = gtext(W, 'rev-parse', 'HEAD:' + rv)
    cases.append(case('R2-REVIEW', 'custodia de la revisión de la ronda 2 (output.json, audit.json y su evidencia) en la base', 'el árbol de 3f912c84, sin cambio (§96, punto 1)',
                      'árbol %s (en 3f912c84: %s)' % (t2[:12], t1[:12]), t1 == t2 and len(t1) == 40))
    for name in REUSED_SAME:
        got = sha256(rb(os.path.join(HERE, name)))
        want = sha256(git(W, 'cat-file', 'blob', 'HEAD:docs/automation/evidence/I-62-A7/r2/' + name)[1])
        cases.append(case('REUSE-' + name, 'copia byte a byte de la pieza de la ronda 2 (sin cambio en la ronda 3)', 'SHA-256 ' + want[:16], 'SHA-256 ' + got[:16], got == want))
    for name, r2sha in sorted(R3_PIECES.items()):
        cur = name.replace('-2.schema', '-3.schema')
        got = sha256(rb(os.path.join(HERE, cur)))
        same = got == r2sha
        cases.append(case('PIECE-' + cur, 'pieza del procedimiento (A7-1, punto 7): ronda 2 → ronda 3', 'cambia solo si depende del registro de rebase '
                          '(constructor, validador, formato, designate_a7); derive_r4.py sin cambio',""",
    """    cases.append(case('R4-ABSENT', 'ningún archivo de r4/ publicado en la base', 'I-62-A7/r4/ vacío', '%d archivos' % sum(1 for n in tree if '/r4/' in n),
                      not any('/r4/' in n for n in tree)))
    man = git(W, 'cat-file', 'blob', 'HEAD:docs/automation/evidence/I-62-A7/r3/MANIFEST.sha256')[1].decode('ascii')
    rows = [l.split('  ', 1) for l in man.splitlines() if l.strip()]
    bad = [r for h, r in rows if sha256(git(W, 'cat-file', 'blob', 'HEAD:docs/automation/evidence/I-62-A7/r3/' + r)[1]) != h]
    cases.append(case('R3-EVID', 'evidencia de la ronda 3 (I-62-A7/r3/) en la base frente a su MANIFEST.sha256', 'idéntica (la ronda 4 no la toca)',
                      '%d entradas; discrepancias: %s' % (len(rows), ', '.join(bad) or 'ninguna'), not bad and len(rows) >= 56))
    rv = 'docs/automation/evidence/I-62-architect-A-7/R20261023T180846Z-62bd/'
    t1 = gtext(W, 'rev-parse', '068d3368:' + rv)
    t2 = gtext(W, 'rev-parse', 'HEAD:' + rv)
    cases.append(case('R3-REVIEW', 'custodia de la revisión de la ronda 3 (output.json, audit.json y su evidencia) en la base', 'el árbol de 068d3368, sin cambio (§98, punto 1)',
                      'árbol %s (en 068d3368: %s)' % (t2[:12], t1[:12]), t1 == t2 and len(t1) == 40))
    for name in REUSED_SAME:
        got = sha256(rb(os.path.join(HERE, name)))
        want = sha256(git(W, 'cat-file', 'blob', 'HEAD:docs/automation/evidence/I-62-A7/r3/' + name)[1])
        cases.append(case('REUSE-' + name, 'copia byte a byte de la pieza de la ronda 3 (sin cambio en la ronda 4)', 'SHA-256 ' + want[:16], 'SHA-256 ' + got[:16], got == want))
    for name, r2sha in sorted(R3_PIECES.items()):
        cur = name.replace('-3.schema', '-4.schema')
        got = sha256(rb(os.path.join(HERE, cur)))
        same = got == r2sha
        cases.append(case('PIECE-' + cur, 'pieza del procedimiento (A7-1, punto 7): ronda 3 → ronda 4', 'cambia solo si depende del control sobre la reverificación '
                          '(constructor, validador, formato, designate_a7); derive_r4.py sin cambio',""")
add("""    cases.append(case('INPUTS', 'designaciones a7-designation/3 de las invocaciones del banco (esquema a7-designation-3.schema.json) y hechos del relevo '""",
    """    cases.append(case('INPUTS', 'designaciones a7-designation/4 de las invocaciones del banco (esquema a7-designation-4.schema.json) y hechos del relevo '""")
# ---- q8 (the round-3 phase, re-run): work area, designations directory, schema file, context for q9 and q10
add("    T8 = os.path.join(WORK, 'r3', 'rebase-repo')", "    T8 = os.path.join(WORK, 'r4', 'rebase-repo')")
add("    dp = R2.jdump(os.path.join(HERE, 'designations-r3', 'RV.json'), rv)", "    dp = R2.jdump(os.path.join(HERE, 'designations-r4', 'RV.json'), rv)")
add("    dschema = json.loads(rb(os.path.join(HERE, 'a7-designation-3.schema.json')).decode('utf-8'))",
    "    dschema = json.loads(rb(os.path.join(HERE, 'a7-designation-4.schema.json')).decode('utf-8'))")
add("""    return {'Phase': 'q8-rebasemap', 'Cases': cases, 'Fixture': dict((k, v) for k, v in fx.items() if k not in ('K', 'D', 'H', 'Image'))}""",
    """    Q8CTX.update({'T8': T8, 'rv': rv, 'fx': fx, 'fx_K': K, 'rec': rec, 'E': E, 'rep': rep, 'cl': cl, 'image': fx['Image'], 'out': out})
    return {'Phase': 'q8-rebasemap', 'Cases': cases, 'Fixture': dict((k, v) for k, v in fx.items() if k not in ('K', 'D', 'H', 'Image'))}""")
# ---- q6
add("'texto del punto 7 igual al de la ronda 2; piezas recalculadas '", "'texto del punto 7 igual al de la ronda 3; piezas recalculadas '")
add("'sin cambio y no reescritas por A-7 r3',", "'sin cambio y no reescritas por A-7 r4',")
add("'el caso tipo B1 (EFF^1 distinto) pasa con las piezas de la ronda 3;", "'el caso tipo B1 (EFF^1 distinto) pasa con las piezas de la ronda 4;")
add("""    cases.append(case('COMPAT-A5-R3', 'A-5 frente a la ronda 3 (decisiones §96, punto 5)', 'A-5 (blob ad94ceed) no regula la reverificación de 16.7 ni el RebaseMap; A-7 r3 lo declara '
                      'en §6', 'A-5 menciona RebaseMap: %s; reverificación: %s; nota de §6: %s' % ('RebaseMap' in a5, '16.7' in a5,
                                                                                                  'El registro de rebase solo entra en una reverificación de 16.7, que A-5 no regula.' in new),
                      'RebaseMap' not in a5 and 'El registro de rebase solo entra en una reverificación de 16.7, que A-5 no regula.' in new))""",
    """    a5note = 'El registro de rebase solo entra en una reverificación de 16.7 y en los controles sobre sus entradas, que A-5 no regula.'
    cases.append(case('COMPAT-A5-R4', 'A-5 frente a la ronda 4 (decisiones §98, punto 4)', 'A-5 (blob ad94ceed) no regula la reverificación de 16.7, sus controles ni el '
                      'RebaseMap; A-7 r4 lo declara en §6', 'A-5 menciona RebaseMap: %s; reverificación: %s; nota de §6: %s' % ('RebaseMap' in a5, '16.7' in a5, a5note in new),
                      'RebaseMap' not in a5 and a5note in new))""")
add("""    cases.append(case('COMPAT-A6-R3', 'A-6 frente a la ronda 3', 'A-6 (blob 4bc919dd) sin cambio; A6-9 y derive_r4.py sin cambio: el registro de rebase sin nombre en C6 y el '
                      'aceptante acepta la reverificación (RB-ACCEPTOR); la precisión de A6-2, reglas 1 y 3, sin cambio de texto',
                      'A-6 en la base: %s; RB-ACCEPTOR: %s; derive_r4 igual: %s; punto 1 (c) conserva la precisión de A6-2, regla 1: %s' % (
                          gtext(W, 'rev-parse', 'HEAD:docs/initiatives/I-62-A-6.md')[:8], q8c['RB-ACCEPTOR']['Cumple'],""",
    """    q9c = dict((c['Caso'], c) for c in R['q9']['Cases'])
    rvc_ok = all(q9c['RVC-%s' % c]['Cumple'] for c in ('NC1', 'NC2', 'NC3', 'N4', 'N5', 'N6', 'N7A', 'N7B')) and q9c['RVC-ACCEPTOR']['Cumple']
    cases.append(case('COMPAT-A6-R4', 'A-6 frente a la ronda 4', 'A-6 (blob 4bc919dd) sin cambio; A6-9 y derive_r4.py sin cambio: el registro de rebase y su copia en los '
                      'controles sin nombre en C6, y el aceptante acepta la reverificación (RB-ACCEPTOR) y un control sobre ella (RVC-ACCEPTOR); la precisión de '
                      'A6-2, reglas 1 y 3, sin cambio de texto',
                      'A-6 en la base: %s; RB-ACCEPTOR: %s; controles sobre RV y RVC-ACCEPTOR: %s; derive_r4 igual: %s; punto 1 (c) conserva la precisión de A6-2, regla 1: %s' % (
                          gtext(W, 'rev-parse', 'HEAD:docs/initiatives/I-62-A-6.md')[:8], q8c['RB-ACCEPTOR']['Cumple'], rvc_ok,""")
add("""                      gtext(W, 'rev-parse', 'HEAD:docs/initiatives/I-62-A-6.md') == '4bc919dd4768ab81abc74b70d3a4a3821c07feb5' and q8c['RB-ACCEPTOR']['Cumple'] and""",
    """                      gtext(W, 'rev-parse', 'HEAD:docs/initiatives/I-62-A-6.md') == '4bc919dd4768ab81abc74b70d3a4a3821c07feb5' and q8c['RB-ACCEPTOR']['Cumple'] and rvc_ok and""")
# ---- q7
add("    tmp = os.path.join(WORK, 'guards-tmp-r3')", "    tmp = os.path.join(WORK, 'guards-tmp-r4')")
add("'guardas G1-G7 sobre el commit hipotético (base %s + candidata r3 como modificación + este directorio bajo I-62-A7/r3/)' % BASE[:8],",
    "'guardas G1-G7 sobre el commit hipotético (base %s + candidata r4 como modificación + este directorio bajo I-62-A7/r4/)' % BASE[:8],")
add("'autoprueba: un mutante por condición nueva o cambiada en la ronda 3'", "'autoprueba: un mutante por condición nueva o cambiada en la ronda 4'")
# ---- report
add("""PHASE_TITLES = {'q0': 'Entorno, identidades, reutilización y delta exacto frente a la ronda 2', 'q1': 'TRX de CurrentSha y RedSha (fase de la ronda 2)',""",
    """PHASE_TITLES = {'q0': 'Entorno, identidades, reutilización y delta exacto frente a la ronda 3', 'q1': 'TRX de CurrentSha y RedSha (fase de la ronda 2)',""")
add("""                'q7': 'Guardas de alcance de la ronda 3', 'q8': 'RebaseMap de la reverificación de 16.7'}""",
    """                'q7': 'Guardas de alcance de la ronda 4', 'q8': 'RebaseMap de la reverificación de 16.7 (fase de la ronda 3)',
                'q9': 'Controles negativos sobre las entradas de la reverificación (A62-A7R3-R1)',
                'q10': 'Modalidades: instrumento, nc3 ordinario, reejecución, A62-A7R3-O1 y A62-A7R3-O2'}""")
add("""    L = ['# I-62 A-7 ronda 3 — preflight dirigido (decisiones §96)', '',
         'Candidata: `docs/initiatives/I-62-A-7.md`, blob `%s` (ronda 2: `%s`). Base: `%s`. Inicio: %s.' % (summary['Candidate']['Blob'], PREV_BLOB, BASE, summary['StartUtc']), '',
         'Las fases q1-q5 son las de la ronda 2 (`r2lib.py`, el ejecutor de la ronda 2 con el formato `a7-designation/3`), repetidas con las piezas de la ronda 3; '
         'sus descripciones conservan el texto de la ronda 2. q0, q6 (ampliada), q7 y q8 son de la ronda 3. Nunca ejecuta Codex, claude ni ningún CLI de modelos; '
         'sin red; solo lectura del worktree de I-62.', '',""",
    """    L = ['# I-62 A-7 ronda 4 — preflight dirigido (decisiones §98)', '',
         'Candidata: `docs/initiatives/I-62-A-7.md`, blob `%s` (ronda 3: `%s`). Base: `%s`. Inicio: %s.' % (summary['Candidate']['Blob'], PREV_BLOB, BASE, summary['StartUtc']), '',
         'Las fases q1-q5 (de la ronda 2, `r2lib.py` con el formato `a7-designation/4`) y q8 (de la ronda 3) se repiten con las piezas de la ronda 4 como prueba de '
         'no regresión; sus descripciones conservan el texto de su ronda. q0, q6, q7, q9 y q10 son de la ronda 4; la matriz de modalidades de decisiones §98, '
         'punto 3, está en `modalities.md`. Nunca ejecuta Codex, claude ni ningún CLI de modelos; sin red; solo lectura del worktree de I-62.', '',""")
add("""        sys.stderr.write('usage: python -I run_a7r3.py all --cand <I-62-A-7.md>\\n')""", """        sys.stderr.write('usage: python -I run_a7r4.py all --cand <I-62-A-7.md>\\n')""")
add("""        if re.match(r'q\\d-.*\\.json$', n) or n in ('summary.json', 'preflight.md', 'MANIFEST.sha256'):""",
    """        if re.match(r'q\\d+-.*\\.json$', n) or n in ('summary.json', 'preflight.md', 'MANIFEST.sha256', 'modalities.md', 'modalities.json'):""")
add("""    R['q8'] = q8()
    jdump(os.path.join(HERE, 'q8-rebasemap.json'), R['q8'])
""", """    R['q8'] = q8()
    jdump(os.path.join(HERE, 'q8-rebasemap.json'), R['q8'])
    P4 = _load('r4phases')
    P4.init(sys.modules[__name__])
    R['q9'] = P4.q9()
    jdump(os.path.join(HERE, 'q9-controls-on-rv.json'), R['q9'])
    R['q10'] = P4.q10(P)
    jdump(os.path.join(HERE, 'q10-modalities.json'), R['q10'])
""")
add("""    order = ['q0', 'q1', 'q2', 'q3', 'q4', 'q5', 'q6', 'q7', 'q8']""", """    order = ['q0', 'q1', 'q2', 'q3', 'q4', 'q5', 'q6', 'q7', 'q8', 'q9', 'q10']""")
add("""    summary = {'Schema': 'i62-a7r3-preflight-summary/1', 'Candidate': {'Path': A7P + ' (candidata ronda 3)', 'Blob': R['q0']['CandidateBlob']},""",
    """    mdtext, mdjson = P4.modalities(R)
    wb(os.path.join(HERE, 'modalities.md'), R1M.sanitize(mdtext).encode('utf-8'))
    jdump(os.path.join(HERE, 'modalities.json'), mdjson)
    summary = {'Schema': 'i62-a7r4-preflight-summary/1', 'Candidate': {'Path': A7P + ' (candidata ronda 4)', 'Blob': R['q0']['CandidateBlob']},""")
add("""               'ToolChanges': ['a7-designation/3: role REBASE (with its RunId) and Invocation.Kind REVERIFICATION; MainShaEval no longer designated',
                               'builder and validator: the rebase record of the 16.7 reverification, its coherence and MainSha_eval = MainAfterSha',
                               'designate_a7.py: REBASE without Name in C6, not admitted outside the reverification, path in the attempt directory'],""",
    """               'ToolChanges': ['a7-designation/4: Invocation.Kind REVERIFICATION_CONTROL with Source (Control, RunId, Designation, DesignationSha256)',
                               'builder and validator: a control on the inputs of a reverification admits the copy of its rebase record; coherence of the '
                               'identified reverification with its own inputs; copies, mutation and link checked; MainSha_eval = its MainAfterSha',
                               'designate_a7.py: REBASE admitted in REVERIFICATION_CONTROL, in the attempt directory of the control path'],
               'Modalities': {'Total': mdjson['Total'], 'Count': len(mdjson['Modalities'])},""")
add("""    sys.stdout.write(json.dumps({'Cases': summary['Cases'], 'Hold': summary['Hold'], 'NotHolding': summary['NotHolding'], 'Guards': summary['Guards']['Result'],
                                 'Selftest': summary['Selftest'], 'Privacy': priv['Result']}, ensure_ascii=False) + '\\n')""",
    """    sys.stdout.write(json.dumps({'Cases': summary['Cases'], 'Hold': summary['Hold'], 'NotHolding': summary['NotHolding'], 'Guards': summary['Guards']['Result'],
                                 'Selftest': summary['Selftest'], 'Privacy': priv['Result'], 'Modalities': summary['Modalities']}, ensure_ascii=False) + '\\n')""")
# q9 and q10 run before q6, which reads them
add("""    R['q6'] = q6(R)""", """    R['q6'] = q6(R)   # round 4: after q9 and q10 (COMPAT-A6-R4 reads q9)""")

for i, (old, new, count) in enumerate(R):
    n = t.count(old)
    if n != count:
        raise SystemExit('replacement %d: %d occurrences (want %d): %r' % (i, n, count, old[:100]))
    t = t.replace(old, new)
out = t.encode('utf-8')
assert b'\r' not in out
open(os.path.join(HERE, 'run_a7r4.py'), 'wb').write(out)
os.remove(os.path.join(HERE, 'run_a7r3.py'))
sys.stdout.write('run_a7r4.py %s (%d replacements)\n' % (hashlib.sha256(out).hexdigest(), len(R)))
