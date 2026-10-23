# patch_guards_r3.py - derives the scope guards of the THIRD candidate of I-62 A-7 (round 3; decisiones §96, puntos 2, 3 y 7) from the round-2
# a7-guards.py by exact replacements and one replaced region (the mutants of the self-test). Unchanged conditions keep the round-2 code.
import hashlib, os
HERE = os.path.dirname(os.path.abspath(__file__))
p = os.path.join(HERE, 'a7-guards.py')
t = open(p, encoding='utf-8', newline='').read()
assert hashlib.sha256(t.encode('utf-8')).hexdigest().startswith(''), 'source'
R = []
add = lambda a, b: R.append((a, b))
RR2 = 'docs/automation/agent-execution/schemas/relay-record.v2.schema.json'

add('''"""Guardas mecánicas del alcance de la SEGUNDA candidata de I-62 A-7 (ronda 2; decisiones §94, puntos 2, 3 y 6).''',
    '''"""Guardas mecánicas del alcance de la TERCERA candidata de I-62 A-7 (ronda 3; decisiones §96, puntos 2, 3 y 7), derivadas de las de la
ronda 2 (texto siguiente, con r3/ por r2/ y la ronda 2 como base): G1 modifica la enmienda, que en la base es la ronda 2 (blob 8a528133…), y
añade solo bajo I-62-A7/r3/; G2 protege además la evidencia de la ronda 2 y la custodia de su revisión, y lee «Previous rounds»; G3 cubre
L1-L22 (L18-L22: AP 16.6, 16.7 y 16.13, el esquema relay-record/v2 y V14 B.8.7); G4 añade el registro de rebase (séptimo papel, solo en la
reverificación) en los puntos 1 (c), 2 (c) y 5 (c), §3.3, §3.4, §3.7, §4, §5, §6 y las preguntas de la ronda 3, y da por superadas las
frases de la ronda 2 que la ronda 3 sustituye; G5 lee la cabecera de la ronda 3. La autoprueba mata un mutante por condición nueva o cambiada.

Texto de la ronda 2:''')
add('''BASE = 'b9334f22204c95872e7defe41bbdaaf849097585\'''', '''BASE = '5da2007f45160ced37b958a3a772c46ac64874ee\'''')
add('''EVID = 'docs/automation/evidence/I-62-A7/r2/\'''', '''EVID = 'docs/automation/evidence/I-62-A7/r3/\'''')
add('''PREV_BLOB = '6c29747d61c5f689a195095ad3712e6d9dba62aa\'''', '''PREV_BLOB = '8a528133c7cccd83356e31f8fd79cd2ebb0cec15'   # round 2 (the base holds it)
R1_BLOB = '6c29747d61c5f689a195095ad3712e6d9dba62aa\'''')
add('''A6 = 'docs/initiatives/I-62-A-6.md\'''', '''A6 = 'docs/initiatives/I-62-A-6.md'
RR2 = '%s\'''' % RR2)
add('''          RAE: '592dcfd45e8743fd83b3423249fbd3c73e3d3055'}''', '''          RAE: '592dcfd45e8743fd83b3423249fbd3c73e3d3055', RR2: 'dca5b29c1a260b679efc0fb249c73850925302f0'}''')
add('''SOURCES = {'V14': V14, 'AP': AP, 'RAE': RAE, 'A-6': A6}''', '''SOURCES = {'V14': V14, 'AP': AP, 'RAE': RAE, 'A-6': A6, 'esquema': RR2}''')
add('''           'L17': ['R1, R2, R3, R9 y R10 están en `MATCH`']}''',
    '''           'L17': ['R1, R2, R3, R9 y R10 están en `MATCH`'],
           # round 3: the relay-record/v2 schema (Phase REBASE and the RebaseMap fields; JSON lines, so anchors and no quotes)
           'L21': ['"REBASE",', '"RebaseMap": {', '"MainBeforeSha",', '"MainAfterSha",', '"PatchIdEqual"', '"ConflictPaths"']}''')
# G4 phrases changed or added by round 3
add('''    ('rule2c', 'la designación nombra el papel de cada artefacto (contrato, delegación, entrega, TRX de la corrida de `CurrentSha`, TRX de la '
               'corrida de `RedSha`, hechos del relevo), con un artefacto como máximo por papel, y registra la ruta y el blob de cada uno; la delegación '
               'y la entrega van bajo `artifacts/orchestration/`, en la ruta de la tarea o del control (RAE §10), en un directorio cuyo nombre es el '
               '`RunId` de sus propios bytes;'),''',
    '''    ('rule2c', 'la designación nombra el papel de cada artefacto (contrato, delegación, entrega, TRX de la corrida de `CurrentSha`, TRX de la '
               'corrida de `RedSha`, hechos del relevo y, en la reverificación, registro de rebase), con un artefacto como máximo por papel, y registra la '
               'ruta y el blob de cada uno; la delegación y la entrega van bajo `artifacts/orchestration/`, en la ruta de la tarea o del control (RAE §10), '
               'en un directorio cuyo nombre es el `RunId` de sus propios bytes;'),
    ('rule2c-rebase-path', 'el registro de rebase va en el directorio del intento de la tarea (`artifacts/orchestration/<UnitId>/<TaskId>/<Attempt>/`), en '
                           'un directorio cuyo nombre es su propio `RunId` (16.6), distinto del de la delegación, el de la entrega y el de la '
                           'reverificación, y la designación registra también ese `RunId`.'),
    ('rule2c-coherence', 'El registro de rebase es coherente si es un `relay-record/v2` con `Phase` = `REBASE`, ese `RunId` y el `TaskId` y el `Attempt` '
                         'de la invocación, y su `RebaseMap` no tiene rutas en conflicto ni commits sin imagen, su `MainBeforeSha` es `K.MainSha`, su '
                         '`AuthorityRevision` es la del contrato con la igualdad de 16.7 registrada, su `BaseSha` es el de la delegación y asigna al '
                         '`CurrentSha` de la entrega la imagen `Target.commit` con igualdad de `patch-id`;'),
    ('rule2c-mse', '`MainSha_eval` de la reverificación es el `MainAfterSha` de ese `RebaseMap` (16.13), que tiene que ser un commit del `-C`;'),''')
add('''                     'de la corrida de `RedSha`—; y los hechos del relevo de la sesión (`RelayFacts`) que alimentan `Termination`, `Remote`, `Ci`, `Routing` '
                     'y `Denials`, cuyo SHA-256 cita la verificación;'),
    ('rule1c-measurement', 'la invocación de medición, que no produce una verificación, no tiene hechos del relevo (A6-7, regla 1);'),''',
    '''                     'de la corrida de `RedSha`—; los hechos del relevo de la sesión (`RelayFacts`) que alimentan `Termination`, `Remote`, `Ci`, `Routing` '
                     'y `Denials`, cuyo SHA-256 cita la verificación;'),
    ('rule1c-rebase', 'y, solo en la reverificación de 16.7 sin conflictos, el registro de rebase de la sesión (`relay-record/v2` con `Phase: REBASE` y '
                      'su `RebaseMap`, 16.6, 16.7 y B.8.7), del que la reverificación toma las imágenes y `MainSha_eval` (16.13); en ella, la corrida de '
                      '`CurrentSha` es la de su imagen (`Ci` sobre `CurrentSha\\'`) y la de `RedSha`, la original (16.7).'),
    ('rule1c-measurement', 'La invocación de medición, que no produce una verificación, no tiene hechos del relevo (A6-7, regla 1), y ninguna invocación '
                           'distinta de la reverificación tiene registro de rebase;'),''')
add('''    ('rule5c-absence-check', 'comprobar que un artefacto designado falta, en la ruta que fija la designación, no es una lectura fuera del cierre ni aporta '
                             'contenido.'),''',
    '''    ('rule5c-rebase-absent', 'en la reverificación, sin un registro de rebase presente, con el blob y el `RunId` registrados, legible y coherente (punto 2 '
                             '(c)), o sin la entrega con la que se comprueba esa coherencia, `MainSha_eval` no se puede fijar ni (a) calcularse: no hay '
                             'cierre ni lanzamiento, y nada lo sustituye (ni otro registro, ni otra corrida, ni el `MainSha` del contrato);'),
    ('rule5c-rebase-not-admitted', 'en cualquier otra invocación, un registro de rebase designado no se admite;'),
    ('rule5c-absence-check', 'comprobar que un artefacto designado falta, en la ruta que fija la designación, no es una lectura fuera del cierre ni aporta '
                             'contenido.'),''')
add('''                  'exige RED (16.9, `Ci` y `Tests`)']''',
    '''                  'exige RED (16.9, `Ci` y `Tests`)',
                  # superseded by round 3 (decisiones §96): the round-2 phrases that the seventh role replaces
                  '`RedSha`—; y los hechos del relevo de la sesión', 'no tiene hechos del relevo (A6-7, regla 1);',
                  'TRX de la corrida de `RedSha`, hechos del relevo), con un artefacto']''')
add('''TABLE_C_ABSENCE = ('contrato o delegación ausentes: sin cierre y sin lanzamiento; entrega, TRX o relevo ausentes: sin entrada, causa registrada, sin '
                   'sustitución, clasificación de 16.9; comprobar la ausencia no es P-22; un artefacto no designado, de otro papel o con otro blob no '
                   'entra (su lectura es P-22)')
TABLE_C_ROLES = ('TRX de la corrida de `CurrentSha` y, si la entrega exige RED, de la de `RedSha`; hechos del relevo (`RelayFacts`; la medición no '
                 'los tiene)')''',
    '''TABLE_C_ABSENCE = ('contrato o delegación ausentes: sin cierre y sin lanzamiento; entrega, TRX o relevo ausentes: sin entrada, causa registrada, sin '
                   'sustitución, clasificación de 16.9; en la reverificación, sin un registro de rebase presente, verificado y coherente (o sin la '
                   'entrega): sin cierre y sin lanzamiento; en otra invocación, el registro de rebase no se admite; comprobar la ausencia no es P-22; un '
                   'artefacto no designado, de otro papel, de otra corrida o con otro blob no entra (su lectura es P-22)')
TABLE_C_ROLES = ('TRX de la corrida de `CurrentSha` y, si la entrega exige RED, de la de `RedSha`; hechos del relevo (`RelayFacts`; la medición no '
                 'los tiene); solo en la reverificación de 16.7, el registro de rebase (`relay-record/v2`, `Phase: REBASE`, con su `RebaseMap`), fuente '
                 'de las imágenes y de `MainSha_eval`')''')
add('''              ('s36', '### 3.6', '## 4.', '### 3.6 Correcciones de la ronda 2 (decisiones §94)'),''',
    '''              ('s36', '### 3.6', '### 3.7', '### 3.6 Correcciones de la ronda 2 (decisiones §94)'),
              ('s37', '### 3.7', '## 4.', '### 3.7 Corrección de la ronda 3 (decisiones §96)'),
              ('s37', '### 3.7', '## 4.', '| A62-A7R2-R1: (c) no admite el `RebaseMap` de la reverificación de 16.7'),
              ('s37', '### 3.7', '## 4.', 'Los opcionales A62-A7R2-O1..O4 de la revisión de la ronda 2 no se incorporan'),
              ('s34-rebase', '### 3.4', '### 3.5', 'El registro de rebase de la reverificación, como los hechos del relevo, no recibe nombre en C6'),
              ('s4-rebase', '## 4.', '## 5.', '| Registro de rebase de la reverificación (`RebaseMap`) |'),
              ('s5-rebase', '## 5. Materialidad', '## 6.', 'en la reverificación, sin un registro de rebase coherente no hay cierre ni lanzamiento'),
              ('s6-a5-rebase', '## 6.', '## 7.', 'El registro de rebase solo entra en una reverificación de 16.7, que A-5 no regula.'),
              ('q-r3', '## 8. Preguntas', '## 9.', '| Q-A7R3-01 |'), ('q-r3', '## 8. Preguntas', '## 9.', '| Q-A7R3-02 |'),
              ('q-r3', '## 8. Preguntas', '## 9.', '| Q-A7R3-03 |'),''')
add('''              ('a62r1-declared', 'Previous amendments:', 'Previous round:', 'ámbito precisa A6-2, regla 1'),''',
    '''              ('a62r1-declared', 'Previous amendments:', 'Previous rounds:', 'ámbito precisa A6-2, regla 1'),''')
add('''STATE_TABLE = '| Architect independiente | ronda 1: CHANGES REQUIRED (R20261019T010200Z-4a72), NOT_ACCREDITED; ronda 2: NOT LAUNCHED\'''',
    '''STATE_TABLE = ('| Architect independiente | ronda 1: CHANGES REQUIRED (R20261019T010200Z-4a72), NOT_ACCREDITED; ronda 2: CHANGES REQUIRED '
               '(R20261019T201030Z-d1bf), NOT_ACCREDITED; ronda 3: NOT LAUNCHED')''')
# G1, G2, G3, G5
add('''                out.append('G1-amendment-modified: %s %s (the round-2 commit modifies the published amendment)' % (st, p))
        elif not (p.startswith(EVID) and st == 'A'):
            out.append('G1-only-allowed: %s %s outside "modify the amendment, add under I-62-A7/r2/"' % (st, p))''',
    '''                out.append('G1-amendment-modified: %s %s (the round-3 commit modifies the published amendment)' % (st, p))
        elif not (p.startswith(EVID) and st == 'A'):
            out.append('G1-only-allowed: %s %s outside "modify the amendment, add under I-62-A7/r3/"' % (st, p))''')
add('''        out.append('G1-amendment-modified: the base does not hold round 1 (%s) at %s' % (PREV_BLOB[:8], A7))''',
    '''        out.append('G1-amendment-modified: the base does not hold round 2 (%s) at %s' % (PREV_BLOB[:8], A7))''')
add('''    want = [p for p in PINNED if p not in (AP, RAE)]''', '''    want = [p for p in PINNED if p not in (AP, RAE, RR2)]''')
add('''    m = re.search(r'\\nPrevious round:\\nronda 1 = blob ([0-9a-f]{40}) \\(commit ([0-9a-f]{7,40})\\)', a7text)
    if not m or m.group(1) != PREV_BLOB or base_t.get(A7) != m.group(1):
        out.append('G2-previous-round: the header does not declare round 1 as the blob the base holds at %s' % A7)''',
    '''    m1 = re.search(r'\\nPrevious rounds:\\nronda 1 = blob ([0-9a-f]{40}) \\(commit ([0-9a-f]{7,40})\\)', a7text)
    m = re.search(r'\\nronda 2 = blob ([0-9a-f]{40}) \\(commit ([0-9a-f]{7,40})\\)', a7text)
    if not m1 or m1.group(1) != R1_BLOB or not m or m.group(1) != PREV_BLOB or base_t.get(A7) != m.group(1):
        out.append('G2-previous-round: the header does not declare rounds 1 and 2, with round 2 as the blob the base holds at %s' % A7)''')
add('''    if ids != ['L%d' % i for i in range(1, 18)]:
        out.append('G3-rows: §2 does not list L1..L17 in order')''',
    '''    if ids != ['L%d' % i for i in range(1, 23)]:
        out.append('G3-rows: §2 does not list L1..L22 in order')''')
add('''    for label, pat in (('state', r'\\nState:\\nPROPUESTA \\(ronda 2\\)\\.'),
                       ('architect', r'\\nArchitect review:\\nronda 1: CHANGES REQUIRED, NOT_ACCREDITED \\(R20261019T010200Z-4a72\\)\\. Ronda 2: NOT LAUNCHED\\.'),
                       ('classification', r'\\nClassification: MATERIAL POST-FREEZE AMENDMENT, candidata'),
                       ('owner', r'\\nOwner decision:\\nno requerida'), ('amendment', r'Amendment: A-7 \\(candidata, ronda 2;'),
                       ('previous-round', r'\\nPrevious round:\\nronda 1 = blob 6c29747d61c5f689a195095ad3712e6d9dba62aa \\(commit 6fee8fd6\\)')):''',
    '''    for label, pat in (('state', r'\\nState:\\nPROPUESTA \\(ronda 3\\)\\.'),
                       ('architect', r'\\nArchitect review:\\nronda 1: CHANGES REQUIRED, NOT_ACCREDITED \\(R20261019T010200Z-4a72\\)\\. Ronda 2: CHANGES REQUIRED, '
                                     r'NOT_ACCREDITED \\(R20261019T201030Z-d1bf\\)\\. Ronda 3:\\s+NOT LAUNCHED\\.'),
                       ('classification', r'\\nClassification: MATERIAL POST-FREEZE AMENDMENT, candidata'),
                       ('owner', r'\\nOwner decision:\\nno requerida'), ('amendment', r'Amendment: A-7 \\(candidata, ronda 3;'),
                       ('previous-round', r'\\nPrevious rounds:\\nronda 1 = blob 6c29747d61c5f689a195095ad3712e6d9dba62aa \\(commit 6fee8fd6\\)[^\\n]*\\n'
                                          r'(?:[^\\n]*\\n){2}ronda 2 = blob 8a528133c7cccd83356e31f8fd79cd2ebb0cec15 \\(commit e90b444d\\)')):''')
add('''        out.append('G5-state-table: §11 does not record round 1 CHANGES REQUIRED / NOT_ACCREDITED and round 2 NOT LAUNCHED')''',
    '''        out.append('G5-state-table: §11 does not record rounds 1 and 2 CHANGES REQUIRED / NOT_ACCREDITED and round 3 NOT LAUNCHED')''')
add('''    want = [(p, head_t[p]) for st, p in diff if (st == 'A' or p == A7) and p in head_t]   # round 2: the amendment is modified, G7 reads it too''',
    '''    want = [(p, head_t[p]) for st, p in diff if (st == 'A' or p == A7) and p in head_t]   # rounds 2 and 3: the amendment is modified, G7 reads it too''')
for a, b in R:
    assert t.count(a) == 1, ('not unique', t.count(a), a[:110])
    t = t.replace(a, b)

# the mutants of the self-test: one per NEW or CHANGED condition of round 3 (region replaced)
start = t.index('    return [\n        (\'G1\', \'only-allowed\',')
end = t.index('def selftest(')
NEW = r"""    return [
        ('G1', 'only-allowed', 'añade un archivo fuera de r3/ (docs/automation/evidence/I-62-A7/extra.txt)', None, {'add': 'docs/automation/evidence/I-62-A7/extra.txt'}),
        ('G1', 'amendment-modified', 'el commit no modifica la enmienda (la ronda 2 queda como está)', None, {'revert_a7': True}),
        ('G2', 'r2-evidence', 'cambia un archivo de la evidencia de la ronda 2 (I-62-A7/r2/preflight.md)', None, {'modify': 'docs/automation/evidence/I-62-A7/r2/preflight.md'}),
        ('G2', 'r2-review', 'cambia la custodia de la revisión de la ronda 2 (output.json)', None,
         {'modify': 'docs/automation/evidence/I-62-architect-A-7/R20261019T201030Z-d1bf/output.json'}),
        ('G2', 'previous-round', 'la cabecera declara otra ronda 2', rep('ronda 2 = blob 8a528133c7cccd83356e31f8fd79cd2ebb0cec15 (commit',
                                                                        'ronda 2 = blob 8a528133c7cccd83356e31f8fd79cd2ebb0cec16 (commit'), {}),
        ('G3', 'rows', 'falta la fila L22', rep('| L22 |', '| L23 |'), {}),
        ('G3', 'L18', 'L18 con una palabra cambiada en la cita', rep('«Un registro de rebase recibe un `RunId` propio y vive en el directorio del intento.»',
                                                                     '«Un registro de rebase recibe un `RunId` propio y vive en el directorio de la tarea.»'), {}),
        ('G3', 'L19', 'L19 sin las líneas 593-595 (la cita de la reverificación queda fuera de las líneas citadas)',
         rep('| L19 | AP 16.7 | 584-588, 593-595 |', '| L19 | AP 16.7 | 584-588 |'), {}),
        ('G3', 'L20', 'L20 con la cita alterada («sin conflictos» → «con conflictos»)',
         rep('salvo en una reverificación de 16.7 sin conflictos, donde es el `MainSha` nuevo del `RebaseMap`»',
             'salvo en una reverificación de 16.7 con conflictos, donde es el `MainSha` nuevo del `RebaseMap`»'), {}),
        ('G3', 'L21', 'L21 cita otro blob del esquema relay-record/v2', rep('esquema `relay-record/v2` (`dca5b29c`)', 'esquema `relay-record/v2` (`dca5b29d`)'), {}),
        ('G3', 'L22', 'L22 con la cita alterada («del diario» → «de la sesión»)',
         rep('«en el `RebaseMap` del `relay-record/v2` del diario»', '«en el `RebaseMap` del `relay-record/v2` de la sesión»'), {}),
        ('G4', 'rule1c-roles', '(c) vuelve a «—; y los hechos del relevo» (texto de la ronda 2)',
         rep('`RedSha`—; los hechos del relevo de la sesión', '`RedSha`—; y unos hechos del relevo de la sesión'), {}),
        ('G4', 'rule1c-rebase', '(c) sin «solo en la reverificación de 16.7 sin conflictos»',
         rep('y, solo en la reverificación de 16.7 sin conflictos, el registro de rebase', 'y, en la verificación, el registro de rebase'), {}),
        ('G4', 'rule1c-measurement', '(c): la medición o la verificación con registro de rebase',
         rep('y ninguna invocación distinta de la reverificación tiene registro de rebase;', 'y cualquier invocación puede tener registro de rebase;'), {}),
        ('G4', 'rule2c', '2 (c) sin el registro de rebase entre los papeles', rep('hechos del relevo y, en la reverificación, registro de\n>    rebase), con un artefacto',
                                                                            'hechos del relevo, registro de\n>    rebase), con un artefacto'), {}),
        ('G4', 'rule2c-rebase-path', '2 (c): el registro de rebase sin su propio `RunId` como directorio',
         rep('en un directorio cuyo\n>    nombre es su propio `RunId` (16.6)', 'en un directorio cuyo\n>    nombre es el `RunId` de la reverificación (16.6)'), {}),
        ('G4', 'rule2c-coherence', '2 (c): la coherencia sin el `MainBeforeSha` = `K.MainSha`', rep('su `MainBeforeSha` es `K.MainSha`', 'su `MainBeforeSha` es cualquiera'), {}),
        ('G4', 'rule2c-mse', '2 (c): `MainSha_eval` deja de ser el `MainAfterSha` del registro',
         rep('`MainSha_eval` de la reverificación es el `MainAfterSha` de ese `RebaseMap` (16.13)', '`MainSha_eval` de la reverificación es `K.MainSha` (16.13)'), {}),
        ('G4', 'rule5c-rebase-absent', '5 (c): sin registro de rebase la reverificación sigue con `K.MainSha`',
         rep('`MainSha_eval` no se puede fijar ni (a) calcularse: no hay cierre ni lanzamiento, y nada lo sustituye (ni otro registro, ni otra corrida, ni el\n>    `MainSha` del contrato);',
             '`MainSha_eval` es el `MainSha` del contrato;'), {}),
        ('G4', 'rule5c-rebase-not-admitted', '5 (c): un registro de rebase se admite en cualquier invocación',
         rep('en cualquier otra invocación, un registro de rebase designado no se admite;', 'en cualquier otra invocación, un registro de rebase designado también se admite;'), {}),
        ('G4', 'superseded-r3', 'reinsertada la frase de la ronda 2 «no tiene hechos del relevo (A6-7, regla 1);»',
         rep('> 8. **límites:** esta regla', '> 8. **límites:** la medición no tiene hechos del relevo (A6-7, regla 1); esta regla'), {}),
        ('G4', 's36', '§3.6 deja de cerrar antes de §3.7 (sin el título de §3.6)', rep('### 3.6 Correcciones de la ronda 2 (decisiones §94)', '### 3.6 Correcciones'), {}),
        ('G4', 's37', 'sin §3.7', rep('### 3.7 Corrección de la ronda 3 (decisiones §96)', '### 3.7 Corrección'), {}),
        ('G4', 's34-rebase', '§3.4 sin el registro de rebase fuera de C6', rep('El registro de rebase de la reverificación, como los hechos del\n  relevo, no recibe nombre en C6',
                                                                              'El registro de rebase de la reverificación recibe nombre en C6'), {}),
        ('G4', 's4-rebase', '§4 sin la fila del registro de rebase', rep('| Registro de rebase de la reverificación (`RebaseMap`) |', '| Registro de rebase |'), {}),
        ('G4', 's5-rebase', 'motivo de M-04 sin la ausencia del registro de rebase', rep('; en la reverificación, sin un registro de rebase coherente no hay cierre ni lanzamiento)', ')'), {}),
        ('G4', 's6-a5-rebase', '§6 sin la nota de A-5 sobre la reverificación', rep(' El registro de rebase solo entra en una reverificación de 16.7, que A-5 no regula.', ''), {}),
        ('G4', 'q-r3', 'sin la pregunta Q-A7R3-03', rep('| Q-A7R3-03 |', '| Q-A7R3-04 |'), {}),
        ('G4', 'table-absence', 'la columna de ausencia de (c) en §3.3 sin la reverificación', rep('; en la reverificación, sin un registro de rebase presente, verificado y coherente (o sin la entrega): sin cierre y sin lanzamiento', ''), {}),
        ('G4', 'table-c-roles', 'la fila (c) de §3.3 sin el registro de rebase', rep('; solo en la reverificación de 16.7, el registro de rebase (`relay-record/v2`, `Phase: REBASE`, con su `RebaseMap`), fuente de las imágenes y de `MainSha_eval`', ''), {}),
        ('G4', 'a62r1-declared', 'la cabecera sin la precisión de A6-2, regla 1 (sección acotada por «Previous rounds»)', rep('ámbito precisa A6-2, regla 1 (además', 'ámbito precisa A6-2 (además'), {}),
        ('G5', 'amendment', 'Amendment ronda 2', rep('Amendment: A-7 (candidata, ronda 3;', 'Amendment: A-7 (candidata, ronda 2;'), {}),
        ('G5', 'state', 'State PROPUESTA (ronda 2)', rep('PROPUESTA (ronda 3). No aplicada', 'PROPUESTA (ronda 2). No aplicada'), {}),
        ('G5', 'architect', 'Architect review sin la ronda 2', rep('Ronda 2: CHANGES REQUIRED, NOT_ACCREDITED (R20261019T201030Z-d1bf). Ronda 3:', 'Ronda 3:'), {}),
        ('G5', 'previous-round', 'sin el bloque Previous rounds', rep('\nPrevious rounds:\nronda 1 = blob', '\nPrevious:\nronda 1 = blob'), {}),
        ('G5', 'state-table', '§11: Architect ronda 3 LAUNCHED', rep('NOT_ACCREDITED; ronda 3: NOT LAUNCHED', 'NOT_ACCREDITED; ronda 3: LAUNCHED'), {}),
        ('G7', 'amendment-scanned', 'una dirección de correo en la enmienda modificada', rep('## 9. Separabilidad', '## 9. Separabilidad ' + 'persona' + chr(64) + 'example.com'), {}),
    ]


EXPECT = dict(((g, c), 'G%s-%s' % (g[1:], c)) for g, c in [
    ('G1', 'only-allowed'), ('G1', 'amendment-modified'), ('G2', 'previous-round'), ('G3', 'rows'), ('G4', 'rule1c-roles'), ('G4', 'rule1c-rebase'),
    ('G4', 'rule1c-measurement'), ('G4', 'rule2c'), ('G4', 'rule2c-rebase-path'), ('G4', 'rule2c-coherence'), ('G4', 'rule2c-mse'),
    ('G4', 'rule5c-rebase-absent'), ('G4', 'rule5c-rebase-not-admitted'), ('G4', 's36'), ('G4', 's37'), ('G4', 's34-rebase'), ('G4', 's4-rebase'),
    ('G4', 's5-rebase'), ('G4', 's6-a5-rebase'), ('G4', 'q-r3'), ('G4', 'table-absence'), ('G4', 'table-c-roles'), ('G4', 'a62r1-declared'),
    ('G5', 'amendment'), ('G5', 'state'), ('G5', 'architect'), ('G5', 'previous-round'), ('G5', 'state-table')])
EXPECT.update({('G2', 'r2-evidence'): 'G2-protected(r1-evidence)', ('G2', 'r2-review'): 'G2-protected(surface)',
               ('G3', 'L18'): 'G3-literal: L18:', ('G3', 'L19'): 'G3-literal: L19:', ('G3', 'L20'): 'G3-literal: L20:', ('G3', 'L21'): 'G3-blob: L21',
               ('G3', 'L22'): 'G3-literal: L22:', ('G7', 'amendment-scanned'): 'e-mail address',
               ('G4', 'superseded-r3'): 'G4-superseded: A7-1 keeps the superseded phrase «no tiene hechos del relevo (A6-7, regla 1);»'})
CHANGED_R3 = sorted(set(c for g, c in EXPECT))
UNCHANGED = ['las condiciones de la ronda 2 que la ronda 3 no cambia (G3 L1-L17 y extra; G4 scope, nochange, quote, points, categories, location, '
             'table, rule1, rule1a, rule2a, rule2-repeat, rule1-a62r1, rule5a, rule5c, rule5c-169, rule5c-undetermined, rule5c-absence-check, '
             'rule5c-p22, rule6-relay, rule7-*, superseded (rondas 1 y 2), residual, residual-coverage, superseded-35, q06, s31-o7, s34-a62r1, '
             's34-a67, s35-o2, s5-m04, q-r2; G5 classification, owner, no-agreed; G6; G7 host-identity, users-path, e-mail; G2 pinned, '
             'header-claims, protected): código de la ronda 2 sin cambio y su autoprueba']


"""
t = t[:start] + NEW + t[end:]
# selftest record
for a, b in (("""            'ChangedAfterFixA7r2b': CHANGED_R2B, 'ChangedAfterFixA7r2c': CHANGED_R2C,
            'UnchangedReusedFrom': 'docs/automation/evidence/I-62-A7/p4-guards-selftest.json (SHA-256 7a670e4945fece2c6a54d24d7eadc052d1c721c5693aabac8b91c49ecd9c6732)',""",
              """            'ChangedInRound3': CHANGED_R3,
            'UnchangedReusedFrom': 'docs/automation/evidence/I-62-A7/r2/q7-guards-selftest.json (autoprueba de la ronda 2, 46 de 46)',"""),
             ('''    """One killing mutant per condition that is NEW or CHANGED in round 2 (the unchanged conditions keep the round-1 code and its
    self-test, I-62-A7/p4-guards-selftest.json)."""''',
              '''    """One killing mutant per condition that is NEW or CHANGED in round 3 (the unchanged conditions keep the round-2 code and its
    self-test, I-62-A7/r2/q7-guards-selftest.json)."""''')):
    assert t.count(a) == 1, a[:80]
    t = t.replace(a, b)
open(p, 'w', encoding='utf-8', newline='').write(t)
print('ok', hashlib.sha256(t.encode('utf-8')).hexdigest()[:12])
