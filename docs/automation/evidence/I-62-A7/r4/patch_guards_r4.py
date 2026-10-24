# patch_guards_r4.py - derives the scope guards of the FOURTH candidate of I-62 A-7 (round 4; decisiones §98, puntos 4 y 7) from the round-3
# a7-guards.py (copied byte for byte from docs/automation/evidence/I-62-A7/r3/) by exact replacements and one replaced region (the mutants of the
# self-test). Unchanged conditions keep the round-3 code and its self-test.   python -I patch_guards_r4.py
import hashlib, os, sys

HERE = os.path.dirname(os.path.abspath(__file__))
p = os.path.join(HERE, 'a7-guards.py')
t = open(p, encoding='utf-8', newline='').read()
assert hashlib.sha1(b'blob %d\0' % len(t.encode('utf-8')) + t.encode('utf-8')).hexdigest() == '87da1c6190a4d53093d4943932b14ba0c5bf6f6f', 'not the round-3 guards'
R = []


def add(a, b, count=1):
    R.append((a, b, count))


CC = 'docs/automation/evidence/I-62-F6/kits/FX-02/controller-contracts.md'
add('''"""Guardas mecánicas del alcance de la TERCERA candidata de I-62 A-7 (ronda 3; decisiones §96, puntos 2, 3 y 7), derivadas de las de la''',
    '''"""Guardas mecánicas del alcance de la CUARTA candidata de I-62 A-7 (ronda 4; decisiones §98, puntos 4 y 7), derivadas de las de la ronda 3
(texto siguiente, con r4/ por r3/ y la ronda 3 como base): G1 modifica la enmienda, que en la base es la ronda 3 (blob 8bcccb2f…), y añade solo
bajo I-62-A7/r4/; G2 protege además la evidencia de la ronda 3 y la custodia de su revisión (R20261023T180846Z-62bd), fija los contratos del
Controller de FX-02 (fuente L24) y lee «Previous rounds» con las rondas 1 a 3; G3 cubre L1-L27 (L23-L27: RAE §10, los contratos del Controller de
FX-02, AP 16.4 y 16.9 y V14 D.3); G4 añade la copia del registro de rebase en los controles sobre las entradas de la reverificación en los puntos
1 (c), 2 (c) y 5 (c), §3.3, §3.4 (con A62-A7R3-O3), §3.5 (residuos de A62-A7R3-O1 y A62-A7R3-O2), §3.7, §3.8, §4, §5, §6 y §8, y da por superadas
las frases de la ronda 3 que la ronda 4 sustituye; G5 lee la cabecera de la ronda 4. La autoprueba mata un mutante por condición nueva o cambiada.

Texto de la ronda 3:
Guardas mecánicas del alcance de la TERCERA candidata de I-62 A-7 (ronda 3; decisiones §96, puntos 2, 3 y 7), derivadas de las de la''')
add("BASE = '5da2007f45160ced37b958a3a772c46ac64874ee'", "BASE = '2405a919b8ce42c8b0b971d97e5f20294059b1e9'")
add("EVID = 'docs/automation/evidence/I-62-A7/r3/'", "EVID = 'docs/automation/evidence/I-62-A7/r4/'")
add("PREV_BLOB = '8a528133c7cccd83356e31f8fd79cd2ebb0cec15'   # round 2 (the base holds it)",
    "PREV_BLOB = '8bcccb2fd2cea480f73ee5bf5a1583e0c9eaf1ff'   # round 3 (the base holds it)\nR2_BLOB = '8a528133c7cccd83356e31f8fd79cd2ebb0cec15'")
add("RR2 = 'docs/automation/agent-execution/schemas/relay-record.v2.schema.json'",
    "RR2 = 'docs/automation/agent-execution/schemas/relay-record.v2.schema.json'\nCC = '%s'" % CC)
add("          RAE: '592dcfd45e8743fd83b3423249fbd3c73e3d3055', RR2: 'dca5b29c1a260b679efc0fb249c73850925302f0'}",
    "          RAE: '592dcfd45e8743fd83b3423249fbd3c73e3d3055', RR2: 'dca5b29c1a260b679efc0fb249c73850925302f0',\n"
    "          CC: '90929ad1c49c028f41f58f654f8713bd0b35f27b'}")
add("SOURCES = {'V14': V14, 'AP': AP, 'RAE': RAE, 'A-6': A6, 'esquema': RR2}", "SOURCES = {'V14': V14, 'AP': AP, 'RAE': RAE, 'A-6': A6, 'esquema': RR2, 'contratos': CC}")
add("""           'L21': ['"REBASE",', '"RebaseMap": {', '"MainBeforeSha",', '"MainAfterSha",', '"PatchIdEqual"', '"ConflictPaths"']}""",
    """           'L21': ['"REBASE",', '"RebaseMap": {', '"MainBeforeSha",', '"MainAfterSha",', '"PatchIdEqual"', '"ConflictPaths"'],
           # round 4: the controls of the FX-02 kit (paraphrased list on the cited line)
           'L24': ['nc1, nc2, nc3, N4, N5, N6, N7a, N7b']}""")
# G1, G2, G3
add("out.append('G1-amendment-modified: %s %s (the round-3 commit modifies the published amendment)' % (st, p))",
    "out.append('G1-amendment-modified: %s %s (the round-4 commit modifies the published amendment)' % (st, p))")
add('''out.append('G1-only-allowed: %s %s outside "modify the amendment, add under I-62-A7/r3/"' % (st, p))''',
    '''out.append('G1-only-allowed: %s %s outside "modify the amendment, add under I-62-A7/r4/"' % (st, p))''')
add("out.append('G1-amendment-modified: the base does not hold round 2 (%s) at %s' % (PREV_BLOB[:8], A7))",
    "out.append('G1-amendment-modified: the base does not hold round 3 (%s) at %s' % (PREV_BLOB[:8], A7))")
add("    want = [p for p in PINNED if p not in (AP, RAE, RR2)]", "    want = [p for p in PINNED if p not in (AP, RAE, RR2, CC)]")
add(r'''    m = re.search(r'\nronda 2 = blob ([0-9a-f]{40}) \(commit ([0-9a-f]{7,40})\)', a7text)
    if not m1 or m1.group(1) != R1_BLOB or not m or m.group(1) != PREV_BLOB or base_t.get(A7) != m.group(1):
        out.append('G2-previous-round: the header does not declare rounds 1 and 2, with round 2 as the blob the base holds at %s' % A7)''',
    r'''    m2 = re.search(r'\nronda 2 = blob ([0-9a-f]{40}) \(commit ([0-9a-f]{7,40})\)', a7text)
    m = re.search(r'\nronda 3 = blob ([0-9a-f]{40}) \(commit ([0-9a-f]{7,40})\)', a7text)
    if not m1 or m1.group(1) != R1_BLOB or not m2 or m2.group(1) != R2_BLOB or not m or m.group(1) != PREV_BLOB or base_t.get(A7) != m.group(1):
        out.append('G2-previous-round: the header does not declare rounds 1 to 3, with round 3 as the blob the base holds at %s' % A7)''')
add("""    if ids != ['L%d' % i for i in range(1, 23)]:
        out.append('G3-rows: §2 does not list L1..L22 in order')""", """    if ids != ['L%d' % i for i in range(1, 28)]:
        out.append('G3-rows: §2 does not list L1..L27 in order')""")
# G4: phrases changed or added by round 4
add('''               'corrida de `RedSha`, hechos del relevo y, en la reverificación, registro de rebase), con un artefacto como máximo por papel, y registra la '
               'ruta y el blob de cada uno;''', '''               'corrida de `RedSha`, hechos del relevo y, en la reverificación y en sus controles, registro de rebase), con un artefacto como máximo por '
               'papel, y registra la ruta y el blob de cada uno;''')
add("""    ('rule2c-rebase-path', 'el registro de rebase va en el directorio del intento de la tarea (`artifacts/orchestration/<UnitId>/<TaskId>/<Attempt>/`), en '""",
    """    ('rule2c-rebase-path', 'el registro de rebase de la reverificación va en el directorio del intento de la tarea '
                           '(`artifacts/orchestration/<UnitId>/<TaskId>/<Attempt>/`), en '""")
add("""    ('rule2c-mse', '`MainSha_eval` de la reverificación es el `MainAfterSha` de ese `RebaseMap` (16.13), un commit del `-C` posterior a su `MainBeforeSha` '
                   'del que desciende `Target.commit`;'),""",
    """    ('rule2c-mse', '`MainSha_eval` de la reverificación es el `MainAfterSha` de ese `RebaseMap` (16.13), un commit del `-C` posterior a su `MainBeforeSha` '
                   'del que desciende `Target.commit`.'),
    # round 4 (A62-A7R3-R1): the control on the inputs of a reverification
    ('rule2c-control-source', 'Un control negativo con invocación que se ejecuta sobre las entradas de una reverificación (RAE §10) solo tiene cierre si su '
                              'designación identifica esa reverificación por su `RunId` y por el SHA-256 de su designación, que cumple este punto con su '
                              'registro de rebase presente, verificado y coherente con su propia entrega, su `Target` y su contrato;'),
    ('rule2c-control-link', 'si el control tiene el `UnitId`, el `TaskId`, el `Attempt`, la `AuthorityRevision` y el `Target` de esa reverificación y un '
                            '`RunId` propio;'),
    ('rule2c-control-copy', 'si su registro de rebase es la copia del de la reverificación, con el mismo blob y el mismo `RunId`, que la designación registra, '
                            'en el directorio del intento de la ruta del control (`artifacts/orchestration/<UnitId>/<TaskId>-<control>/<Attempt>/`), en un '
                            'directorio cuyo nombre es ese `RunId`;'),
    ('rule2c-control-copies', 'y si cada otro artefacto de (c), salvo los hechos del relevo, que son los del registro propio del control, es la copia del de '
                              'su papel en la reverificación, con el mismo blob y en la misma ruta (la delegación y la entrega, en la ruta del control), y solo '
                              'difiere el artefacto que lleva los campos que el control muta, y solo en ellos, o falta la entrega que el control deja ausente '
                              '(RAE §10; D.3).'),
    ('rule2c-control-mse', 'En ese control, la coherencia del registro es la de la reverificación, comprobada con las entradas de esta y no con la entrega '
                           'mutada o ausente del control, y `MainSha_eval` es el `MainAfterSha` de ese registro;'),""")
add("""    ('rule1c-rebase', 'y, solo en la reverificación de 16.7 sin conflictos, el registro de rebase de la sesión (`relay-record/v2` con `Phase: REBASE` y '
                      'su `RebaseMap`, 16.6, 16.7 y B.8.7), del que la reverificación toma las imágenes y `MainSha_eval` (16.13); en ella, la corrida de '
                      '`CurrentSha` es la de su imagen (`Ci` sobre `CurrentSha\\'`) y la de `RedSha`, la original (16.7).'),
    ('rule1c-measurement', 'La invocación de medición, que no produce una verificación, no tiene hechos del relevo (A6-7, regla 1), y ninguna invocación '
                           'distinta de la reverificación tiene registro de rebase;'),""",
    """    ('rule1c-rebase', 'y, solo en la reverificación de 16.7 sin conflictos y en cada control negativo con invocación que se ejecuta sobre sus entradas '
                      '(RAE §10), el registro de rebase de la sesión (`relay-record/v2` con `Phase: REBASE` y su `RebaseMap`, 16.6, 16.7 y B.8.7), del que '
                      'la reverificación toma las imágenes y `MainSha_eval` (16.13) y del que el control recibe una copia; en ellos, la corrida de '
                      '`CurrentSha` es la de su imagen (`Ci` sobre `CurrentSha\\'`) y la de `RedSha`, la original (16.7).'),
    ('rule1c-measurement', 'La invocación de medición, que no produce una verificación, no tiene hechos del relevo (A6-7, regla 1), y ninguna otra invocación '
                           '(la verificación ordinaria, sus controles y la medición) tiene registro de rebase;'),""")
add("""    ('rule5c-rebase-not-admitted', 'en cualquier otra invocación, un registro de rebase designado no se admite;'),""",
    """    ('rule5c-control', 'en un control negativo sobre las entradas de una reverificación, si la reverificación identificada, su registro de rebase, la copia '
                       'de ese registro o alguna de las copias del control no cumplen el punto 2 (c), no hay cierre ni lanzamiento, y nada los sustituye; la '
                       'entrega mutada (nc1) o ausente (N4) del control no impide fijar `MainSha_eval`;'),
    ('rule5c-rebase-not-admitted', 'en cualquier otra invocación (la verificación ordinaria, sus controles y la medición), un registro de rebase designado no '
                                   'se admite;'),""")
add("""                  'TRX de la corrida de `RedSha`, hechos del relevo), con un artefacto']""",
    """                  'TRX de la corrida de `RedSha`, hechos del relevo), con un artefacto',
                  # superseded by round 4 (decisiones §98): the round-3 phrases that the control on the inputs of a reverification replaces
                  'y ninguna invocación distinta de la reverificación tiene registro de rebase',
                  'y, solo en la reverificación de 16.7 sin conflictos, el registro de rebase', 'hechos del relevo y, en la reverificación, registro de rebase)',
                  'en cualquier otra invocación, un registro de rebase designado no se admite', 'el registro de rebase va en el directorio del intento de la tarea']""")
add("""                   'entrega): sin cierre y sin lanzamiento; en otra invocación, el registro de rebase no se admite; comprobar la ausencia no es P-22; un '""",
    """                   'entrega): sin cierre y sin lanzamiento; en un control sobre sus entradas, sin esa reverificación válida, sin la copia de su registro o '
                   'con una copia que no es la suya: sin cierre y sin lanzamiento (la entrega mutada o ausente del control no lo impide); en la '
                   'verificación ordinaria, sus controles y la medición, el registro de rebase no se admite; comprobar la ausencia no es P-22; un '""")
add("""                 'los tiene); solo en la reverificación de 16.7, el registro de rebase (`relay-record/v2`, `Phase: REBASE`, con su `RebaseMap`), fuente '
                 'de las imágenes y de `MainSha_eval`')""",
    """                 'los tiene); solo en la reverificación de 16.7 y en los controles sobre sus entradas, el registro de rebase (`relay-record/v2`, `Phase: '
                 'REBASE`, con su `RebaseMap`; en el control, su copia), fuente de las imágenes y de `MainSha_eval`')""")
add("""              ('s34-rebase', '### 3.4', '### 3.5', 'El registro de rebase de la reverificación, como los hechos del relevo, no recibe nombre en C6'),""",
    """              ('s34-rebase', '### 3.4', '### 3.5', 'El registro de rebase de la reverificación y su copia en los controles, como los hechos del relevo, no '
                                                  'reciben nombre en C6'),
              ('s34-o3', '### 3.4', '### 3.5', 'los hechos del relevo y, en la reverificación y en sus controles, el registro de rebase) también son excepciones'),
              ('s35-r3o1', '### 3.5', '### 3.6', '**Residuo declarado (C1 del aceptante en la reverificación y en sus controles; A62-A7R3-O1).**'),
              ('s35-r3o1', '### 3.5', '### 3.6', 'un rechazo falso, nunca una aceptación falsa, y la salida no se acepta (A6-7, regla 4)'),
              ('s35-r3o1', '### 3.5', '### 3.6', 'no la da por resuelta'),
              ('s35-r3o2', '### 3.5', '### 3.6', '**Residuo declarado (segunda recuperación de 16.7; A62-A7R3-O2).**'),
              ('s35-r3o2', '### 3.5', '### 3.6', 'la segunda reverificación y sus controles no tienen cierre ni lanzamiento'),
              ('s37-r4note', '### 3.7', '### 3.8', 'La ronda 4 corrige la frase final de esta corrección'),
              ('s38', '### 3.8', '## 4.', '### 3.8 Corrección de la ronda 4 (decisiones §98)'),
              ('s38', '### 3.8', '## 4.', '| A62-A7R3-R1: los controles negativos con invocación que RAE §10 ejecuta sobre las entradas de una reverificación'),
              ('s38-matrix', '### 3.8', '## 4.', '**Matriz de modalidades (decisiones §98, punto 3).**'),
              ('s38-matrix', '### 3.8', '## 4.', 'docs/automation/evidence/I-62-A7/r4/modalities.md'),
              ('s4-o3', '## 4.', '## 5.', 'como excepción a la clase de §20.3.1 (precisión de A6-2, regla 1), con su `RunId` y su coherencia'),""")
add("""              ('s5-rebase', '## 5. Materialidad', '## 6.', 'en la reverificación, sin un registro de rebase coherente no hay cierre ni lanzamiento'),
              ('s6-a5-rebase', '## 6.', '## 7.', 'El registro de rebase solo entra en una reverificación de 16.7, que A-5 no regula.'),""",
    """              ('s5-rebase', '## 5. Materialidad', '## 6.', 'en la reverificación, sin un registro de rebase coherente, y en sus controles, sin esa reverificación '
                                                         'válida y la copia de su registro, no hay cierre ni lanzamiento'),
              ('s6-a5-rebase', '## 6.', '## 7.', 'El registro de rebase solo entra en una reverificación de 16.7 y en los controles sobre sus entradas, que A-5 no '
                                                 'regula.'),
              ('q-r4', '## 8. Preguntas', '## 9.', '| Q-A7R4-01 |'), ('q-r4', '## 8. Preguntas', '## 9.', '| Q-A7R4-02 |'),
              ('q-r4', '## 8. Preguntas', '## 9.', '| Q-A7R4-03 |'),
              ('q-r3-answers', '## 8. Preguntas', '## 9.', 'Respuestas de la revisión de la ronda 3 (R20261023T180846Z-62bd)'),""")
add("""                  ('superseded-5', '## 5. Materialidad', '## 6.', 'sin lanzamiento en (a) y en el objeto de (b)')]""",
    """                  ('superseded-5', '## 5. Materialidad', '## 6.', 'sin lanzamiento en (a) y en el objeto de (b)'),
                  # round 4: the round-3 phrases outside A7-1 that round 4 replaces
                  ('superseded-34-r3', '### 3.4', '### 3.5', 'no recibe nombre en C6: el instrumento no lo lee'),
                  ('superseded-4-r3', '## 4.', '## 5.', 'en otra invocación, no admitido |'),
                  ('superseded-6-r3', '## 6.', '## 7.', 'solo entra en una reverificación de 16.7, que A-5')]""")
add("""STATE_TABLE = ('| Architect independiente | ronda 1: CHANGES REQUIRED (R20261019T010200Z-4a72), NOT_ACCREDITED; ronda 2: CHANGES REQUIRED '
               '(R20261019T201030Z-d1bf), NOT_ACCREDITED; ronda 3: NOT LAUNCHED')""",
    """STATE_TABLE = ('| Architect independiente | ronda 1: CHANGES REQUIRED (R20261019T010200Z-4a72), NOT_ACCREDITED; ronda 2: CHANGES REQUIRED '
               '(R20261019T201030Z-d1bf), NOT_ACCREDITED; ronda 3: CHANGES REQUIRED (R20261023T180846Z-62bd), ACCREDITED; ronda 4: NOT LAUNCHED')""")
# G5
add(r"""    for label, pat in (('state', r'\nState:\nPROPUESTA \(ronda 3\)\.'),
                       ('architect', r'\nArchitect review:\nronda 1: CHANGES REQUIRED, NOT_ACCREDITED \(R20261019T010200Z-4a72\)\. Ronda 2: CHANGES REQUIRED, '
                                     r'NOT_ACCREDITED \(R20261019T201030Z-d1bf\)\. Ronda 3:\s+NOT LAUNCHED\.'),""",
    r"""    for label, pat in (('state', r'\nState:\nPROPUESTA \(ronda 4\)\.'),
                       ('architect', r'\nArchitect review:\nronda 1: CHANGES REQUIRED, NOT_ACCREDITED \(R20261019T010200Z-4a72\)\. Ronda 2: CHANGES REQUIRED, '
                                     r'NOT_ACCREDITED \(R20261019T201030Z-d1bf\)\. Ronda 3:\s+CHANGES REQUIRED, ACCREDITED \(R20261023T180846Z-62bd\)\. '
                                     r'Ronda 4: NOT LAUNCHED\.'),""")
add(r"""('owner', r'\nOwner decision:\nno requerida'), ('amendment', r'Amendment: A-7 \(candidata, ronda 3;'),""",
    r"""('owner', r'\nOwner decision:\nno requerida'), ('amendment', r'Amendment: A-7 \(candidata, ronda 4;'),""")
add(r"""                                          r'(?:[^\n]*\n){2}ronda 2 = blob 8a528133c7cccd83356e31f8fd79cd2ebb0cec15 \(commit e90b444d\)')):""",
    r"""                                          r'(?:[^\n]*\n){2}ronda 2 = blob 8a528133c7cccd83356e31f8fd79cd2ebb0cec15 \(commit e90b444d\)[^\n]*\n'
                                          r'(?:[^\n]*\n){2}ronda 3 = blob 8bcccb2fd2cea480f73ee5bf5a1583e0c9eaf1ff \(commit 37ac6ca4\)')):""")
add("""        out.append('G5-state-table: §11 does not record rounds 1 and 2 CHANGES REQUIRED / NOT_ACCREDITED and round 3 NOT LAUNCHED')""",
    """        out.append('G5-state-table: §11 does not record rounds 1 and 2 CHANGES REQUIRED / NOT_ACCREDITED, round 3 CHANGES REQUIRED / ACCREDITED and round 4 '
                   'NOT LAUNCHED')""")
# the self-test: the mutants region is replaced (one killing mutant per condition NEW or CHANGED in round 4)
start = t.index('def mutants(a7, idents):')
end = t.index('def selftest(repo, base, cand, evid, tmp):')
NEW_MUTANTS = r'''def mutants(a7, idents):
    """One killing mutant per condition that is NEW or CHANGED in round 4 (the unchanged conditions keep the round-3 code and its
    self-test, I-62-A7/r3/q7-guards-selftest.json)."""
    t = a7.decode('utf-8')

    def rep(old, new, count=1):
        if old not in t:
            raise SystemExit('selftest: anchor not found for a mutant: ' + old[:60])
        return t.replace(old, new, count).encode('utf-8')
    return [
        ('G1', 'only-allowed', 'añade un archivo fuera de r4/ (docs/automation/evidence/I-62-A7/extra.txt)', None, {'add': 'docs/automation/evidence/I-62-A7/extra.txt'}),
        ('G1', 'amendment-modified', 'el commit no modifica la enmienda (la ronda 3 queda como está)', None, {'revert_a7': True}),
        ('G2', 'r3-evidence', 'cambia un archivo de la evidencia de la ronda 3 (I-62-A7/r3/preflight.md)', None, {'modify': 'docs/automation/evidence/I-62-A7/r3/preflight.md'}),
        ('G2', 'r3-review', 'cambia la custodia de la revisión de la ronda 3 (output.json)', None,
         {'modify': 'docs/automation/evidence/I-62-architect-A-7/R20261023T180846Z-62bd/output.json'}),
        ('G2', 'pinned', 'cambia los contratos del Controller de FX-02 (fuente L24)', None, {'modify': CC}),
        ('G2', 'previous-round', 'la cabecera declara otra ronda 3', rep('ronda 3 = blob 8bcccb2fd2cea480f73ee5bf5a1583e0c9eaf1ff (commit',
                                                                        'ronda 3 = blob 8bcccb2fd2cea480f73ee5bf5a1583e0c9eaf1f0 (commit'), {}),
        ('G3', 'rows', 'falta la fila L27', rep('| L27 |', '| L28 |'), {}),
        ('G3', 'L23', 'L23 con la cita alterada («último» → «primer»)', rep('del último `WorkRunId` de la cadena»', 'del primer `WorkRunId` de la cadena»'), {}),
        ('G3', 'L24', 'L24 cita otro blob de los contratos del Controller', rep('contratos del Controller de FX-02 (`90929ad1`)', 'contratos del Controller de FX-02 (`90929ad2`)'), {}),
        ('G3', 'L25', 'L25 sin la línea 537 (la cita queda fuera de las líneas citadas)', rep('| L25 | AP 16.4 | 536-537 |', '| L25 | AP 16.4 | 535 |'), {}),
        ('G3', 'L26', 'L26 con la cita alterada («igual» → «distinto»)', rep('con `patch-id` igual»', 'con `patch-id` distinto»'), {}),
        ('G3', 'L27', 'L27 con la cita alterada (N6 → BLOCKED)', rep('N6 trabajo sin commit → CleanTree REWORK', 'N6 trabajo sin commit → CleanTree BLOCKED'), {}),
        ('G4', 'rule1c-rebase', '1 (c) sin los controles sobre las entradas de la reverificación',
         rep('sin conflictos y en cada control negativo con invocación que se ejecuta sobre sus\n>    entradas (RAE §10), el registro', 'sin conflictos, el registro'), {}),
        ('G4', 'rule1c-measurement', '1 (c): cualquier otra invocación con registro de rebase',
         rep('y ninguna otra invocación (la verificación ordinaria, sus controles y la medición) tiene registro de rebase;',
             'y cualquier otra invocación puede tener registro de rebase;'), {}),
        ('G4', 'rule2c', '2 (c) sin los controles entre los que nombran el registro de rebase',
         rep('hechos del relevo y, en la reverificación y en sus\n>    controles, registro de rebase)', 'hechos del relevo y registro de\n>    rebase)'), {}),
        ('G4', 'rule2c-rebase-path', '2 (c): el registro de la reverificación fuera del directorio del intento',
         rep('registro de rebase de la reverificación va en el directorio del intento de la tarea', 'registro de rebase de la reverificación va en el directorio de la tarea'), {}),
        ('G4', 'rule2c-mse', '2 (c): la frase de `MainSha_eval` de la reverificación sin cerrar', rep('del que desciende `Target.commit`. Un control negativo', 'del que desciende `Target.commit`; un control negativo'), {}),
        ('G4', 'rule2c-control-source', '2 (c): la reverificación identificada sin el SHA-256 de su designación',
         rep('por su `RunId` y por el SHA-256 de su', 'por su `RunId` y por el nombre de su'), {}),
        ('G4', 'rule2c-control-link', '2 (c): el control sin el `Target` de la reverificación ni un `RunId` propio',
         rep('y el `Target` de esa reverificación y un `RunId` propio;', 'de esa reverificación;'), {}),
        ('G4', 'rule2c-control-copy', '2 (c): la copia del registro sin el mismo blob', rep('con el mismo blob y el mismo `RunId`, que la designación registra', 'con el mismo `RunId`, que la designación registra'), {}),
        ('G4', 'rule2c-control-copies', '2 (c): las copias sin la misma ruta', rep('con el mismo blob y en la misma ruta (la delegación y la entrega, en la ruta del control)', 'con el mismo blob'), {}),
        ('G4', 'rule2c-control-mse', '2 (c): la coherencia comprobada con la entrega del control',
         rep('comprobada con las entradas de esta y no con la entrega mutada o ausente del control', 'comprobada con la entrega del control'), {}),
        ('G4', 'rule5c-control', '5 (c): la entrega del control fija `MainSha_eval`',
         rep('la entrega mutada (nc1) o ausente (N4) del control no impide fijar `MainSha_eval`;', 'la entrega del control fija `MainSha_eval`;'), {}),
        ('G4', 'rule5c-rebase-not-admitted', '5 (c): un registro de rebase se admite en la verificación ordinaria, sus controles y la medición',
         rep('ordinaria, sus controles y la medición), un registro de rebase designado no se admite;', 'ordinaria, sus controles y la medición), un registro de rebase designado también se admite;'), {}),
        ('G4', 'superseded-r4', 'reinsertada la frase de la ronda 3 «en cualquier otra invocación, un registro de rebase designado no se admite»',
         rep('> 8. **límites:** esta regla', '> 8. **límites:** en cualquier otra invocación, un registro de rebase designado no se admite; esta regla'), {}),
        ('G4', 's34-rebase', '§3.4: el registro y su copia con nombre en C6', rep('El registro de rebase de la reverificación y su copia en los\n  controles, como los hechos del relevo, no reciben nombre en C6',
                                                                                'El registro de rebase recibe nombre en C6'), {}),
        ('G4', 's34-o3', '§3.4 sin el registro de rebase en la enumeración (A62-A7R3-O3)', rep('los hechos del relevo y, en la reverificación y en sus\n  controles, el registro de rebase) también son excepciones',
                                                                                              'los hechos del relevo) también son excepciones'), {}),
        ('G4', 's35-r3o1', '§3.5 sin el residuo de A62-A7R3-O1', rep('**Residuo declarado (C1 del aceptante en la reverificación y en sus controles; A62-A7R3-O1).**', '**Residuo (C1).**'), {}),
        ('G4', 's35-r3o2', '§3.5: la segunda reverificación con cierre', rep('la segunda reverificación y sus controles no\ntienen cierre ni lanzamiento', 'la segunda reverificación y sus controles\ntienen cierre'), {}),
        ('G4', 's37-r4note', '§3.7 sin la nota de la ronda 4', rep('La ronda 4 corrige la frase final de esta corrección', 'La ronda 4 no corrige esta corrección'), {}),
        ('G4', 's38', 'sin el título de §3.8', rep('### 3.8 Corrección de la ronda 4 (decisiones §98)', '### 3.8 Corrección'), {}),
        ('G4', 's38-matrix', '§3.8 sin la referencia a la matriz completa', rep('docs/automation/evidence/I-62-A7/r4/modalities.md', 'docs/automation/evidence/I-62-A7/r4/'), {}),
        ('G4', 's4-o3', '§4 sin la excepción a la clase de §20.3.1 (A62-A7R3-O3)', rep('como excepción a la clase de §20.3.1 (precisión de A6-2, regla 1), con su `RunId`', 'con su `RunId`'), {}),
        ('G4', 's5-rebase', 'motivo de M-04 sin los controles sobre la reverificación',
         rep('; en la reverificación, sin un registro de rebase coherente, y en sus controles, sin esa reverificación válida y la copia de su registro, no hay cierre ni lanzamiento)', ')'), {}),
        ('G4', 's6-a5-rebase', '§6 sin la nota de A-5', rep(' El registro de rebase solo entra en una reverificación de 16.7 y en los controles sobre sus entradas, que A-5 no regula.', ''), {}),
        ('G4', 'q-r4', 'sin la pregunta Q-A7R4-03', rep('| Q-A7R4-03 |', '| Q-A7R4-04 |'), {}),
        ('G4', 'q-r3-answers', 'sin las respuestas de la revisión de la ronda 3', rep('Respuestas de la revisión de la ronda 3 (R20261023T180846Z-62bd)', 'Respuestas de la ronda 3'), {}),
        ('G4', 'table-absence', 'la columna de ausencia de (c) en §3.3 sin el control sobre la reverificación',
         rep('; en un control sobre sus entradas, sin esa reverificación válida, sin la copia de su registro o con una copia que no es la suya: sin cierre y sin lanzamiento (la entrega mutada o ausente del control no lo impide)', ''), {}),
        ('G4', 'table-c-roles', 'la fila (c) de §3.3 sin la copia en los controles',
         rep(' y en los controles sobre sus entradas, el registro de rebase (`relay-record/v2`, `Phase: REBASE`, con su `RebaseMap`; en el control, su copia)',
             ', el registro de rebase (`relay-record/v2`, `Phase: REBASE`, con su `RebaseMap`)'), {}),
        ('G5', 'amendment', 'Amendment ronda 3', rep('Amendment: A-7 (candidata, ronda 4;', 'Amendment: A-7 (candidata, ronda 3;'), {}),
        ('G5', 'state', 'State PROPUESTA (ronda 3)', rep('PROPUESTA (ronda 4). No aplicada', 'PROPUESTA (ronda 3). No aplicada'), {}),
        ('G5', 'architect', 'Architect review sin el resultado de la ronda 3', rep('CHANGES REQUIRED, ACCREDITED (R20261023T180846Z-62bd). Ronda 4: NOT LAUNCHED.', 'NOT LAUNCHED.'), {}),
        ('G5', 'previous-round', 'Previous rounds sin la ronda 3 en su forma', rep('\nronda 3 = blob 8bcccb2f', '\nronda 3: blob 8bcccb2f'), {}),
        ('G5', 'state-table', '§11: Architect ronda 4 LAUNCHED', rep('ACCREDITED; ronda 4: NOT LAUNCHED', 'ACCREDITED; ronda 4: LAUNCHED'), {}),
        ('G7', 'amendment-scanned', 'una dirección de correo en la enmienda modificada', rep('## 9. Separabilidad', '## 9. Separabilidad ' + 'persona' + chr(64) + 'example.com'), {}),
    ]


EXPECT = dict(((g, c), 'G%s-%s' % (g[1:], c)) for g, c in [
    ('G1', 'only-allowed'), ('G1', 'amendment-modified'), ('G2', 'previous-round'), ('G2', 'pinned'), ('G3', 'rows'), ('G4', 'rule1c-rebase'),
    ('G4', 'rule1c-measurement'), ('G4', 'rule2c'), ('G4', 'rule2c-rebase-path'), ('G4', 'rule2c-mse'), ('G4', 'rule2c-control-source'),
    ('G4', 'rule2c-control-link'), ('G4', 'rule2c-control-copy'), ('G4', 'rule2c-control-copies'), ('G4', 'rule2c-control-mse'), ('G4', 'rule5c-control'),
    ('G4', 'rule5c-rebase-not-admitted'), ('G4', 's34-rebase'), ('G4', 's34-o3'), ('G4', 's35-r3o1'), ('G4', 's35-r3o2'), ('G4', 's37-r4note'), ('G4', 's38'),
    ('G4', 's38-matrix'), ('G4', 's4-o3'), ('G4', 's5-rebase'), ('G4', 's6-a5-rebase'), ('G4', 'q-r4'), ('G4', 'q-r3-answers'), ('G4', 'table-absence'),
    ('G4', 'table-c-roles'), ('G5', 'amendment'), ('G5', 'state'), ('G5', 'architect'), ('G5', 'previous-round'), ('G5', 'state-table')])
EXPECT.update({('G2', 'r3-evidence'): 'G2-protected(r1-evidence)', ('G2', 'r3-review'): 'G2-protected(surface)',
               ('G3', 'L23'): 'G3-literal: L23:', ('G3', 'L24'): 'G3-blob: L24', ('G3', 'L25'): 'G3-literal: L25:', ('G3', 'L26'): 'G3-literal: L26:',
               ('G3', 'L27'): 'G3-literal: L27:', ('G7', 'amendment-scanned'): 'e-mail address',
               ('G4', 'superseded-r4'): 'G4-superseded: A7-1 keeps the superseded phrase «en cualquier otra invocación, un registro de rebase designado no se admite»'})
CHANGED_R4 = sorted(set(c for g, c in EXPECT))
UNCHANGED = ['las condiciones de la ronda 3 que la ronda 4 no cambia (G3 L1-L22 y extra; G4 scope, nochange, quote, points, categories, location, '
             'table, rule1, rule1a, rule2a, rule2-repeat, rule1c-roles, rule1-a62r1, rule2c-coherence, rule5a, rule5c, rule5c-169, rule5c-undetermined, '
             'rule5c-rebase-absent, rule5c-absence-check, rule5c-p22, rule6-relay, rule7-*, superseded (rondas 1 a 3), residual, residual-coverage, '
             'superseded-35, q06, s31-o7, s34-a62r1, s34-a67, s35-o2, s36, s37, s4-rebase, s5-m04, q-r2, q-r3; G5 classification, owner, no-agreed; G6; G7 '
             'host-identity, users-path, e-mail; G2 header-claims, protected): código de la ronda 3 sin cambio y su autoprueba']


'''
t = t[:start] + NEW_MUTANTS + t[end:]
add("""            'ChangedInRound3': CHANGED_R3,
            'UnchangedReusedFrom': 'docs/automation/evidence/I-62-A7/r2/q7-guards-selftest.json (autoprueba de la ronda 2, 46 de 46)',""",
    """            'ChangedInRound4': CHANGED_R4,
            'UnchangedReusedFrom': 'docs/automation/evidence/I-62-A7/r3/q7-guards-selftest.json (autoprueba de la ronda 3, 38 de 38)',""")
for i, (old, new, count) in enumerate(R):
    n = t.count(old)
    if n != count:
        raise SystemExit('replacement %d: %d occurrences (want %d): %r' % (i, n, count, old[:100]))
    t = t.replace(old, new)
data = t.encode('utf-8')
assert b'\r' not in data
open(p, 'wb').write(data)
sys.stdout.write('a7-guards.py %s (%d replacements and the mutants region)\n' % (hashlib.sha256(data).hexdigest(), len(R)))
