"""PB-T4 (I-64 F4B): arma la v5.3 del paquete desde la v5.2 (`I-64-f4b-pbt4-gate-contract-package-v52.md`, sha256 466ab3db...f13e), con las
correcciones de la comprobacion acotada CA-T4d (`independent-review-pbt4d-check.md`: I-01, I-02 y M-01 a M-07). Cada cambio es un reemplazo por
anclaje exacto, contado (R), o una seccion de PB-T4d entera sustituida por su fragmento de v53/frag/ (los de la v5.2 con los cambios de
v53/edit_frags_v53.py). Los valores medidos salen de la precondicion 6 de la v5.3 (pbt4dcc/compile/pbt4d-precondition6.json, que se comprueba:
`Package` v5.3, todo lo previsto y las piezas con el sha256 de hoy); el anexo V53-A, de las piezas. Escribe UTF-8 con LF.

Uso: python -I make_v53.py <repo>
"""
import collections
import hashlib
import importlib.util
import json
import os
import re
import sys

sys.dont_write_bytecode = True
HERE = os.path.dirname(os.path.abspath(__file__))
REPO = sys.argv[1]
REV = '6c2820bab4d8b41e3e229e23f3a1638ccbb04735'
SRC = os.path.join(HERE, 'I-64-f4b-pbt4-gate-contract-package-v52.md')
OUT = os.path.join(HERE, 'I-64-f4b-pbt4-gate-contract-package-v54.md')
CC = os.path.normpath(os.path.join(HERE, '..', 'pbt4dcc', 'compile'))
V53 = os.path.join(HERE, 'v53')
raw = open(SRC, 'rb').read()
assert hashlib.sha256(raw).hexdigest() == '466ab3dbbde0a59279001c5250bdbca85892e0929e7f81829558f92a5464f13e'
t = raw.decode('utf-8')
log = []


def sha(path):
    data = open(path, 'rb').read()
    assert b'\r' not in data, path
    return hashlib.sha256(data).hexdigest()


def load(name, path):
    spec = importlib.util.spec_from_file_location(name, path)
    mod = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(mod)
    return mod


def R(old, new, times=1, what=None):
    global t
    n = t.count(old)
    assert n == times, (what or old[:90], n, times)
    t = t.replace(old, new)
    log.append('reemplazo %s' % (what or old[:60].replace('\n', ' ')))


def line_starting(prefix):
    lines = [l for l in t.split('\n') if l.startswith(prefix)]
    assert len(lines) == 1, (prefix, len(lines))
    return lines[0]


def section(start, end, new, what):
    global t
    a = t.index(start)
    assert t.count(start) == 1, start
    b = t.index(end, a)
    t = t[:a] + new + t[b:]
    log.append('reemplazo %s' % what)


def frag(name):
    return open(os.path.join(HERE, 'v54', 'frag', name), encoding='utf-8').read()


# ------------------------------------------------------------------ la precondicion 6
p6 = json.load(open(os.path.join(CC, 'pbt4d-precondition6.json'), encoding='utf-8'))
assert p6['Package'] == 'v5.4' and p6['Revision'] == REV and p6['AllAsExpected'], p6['Expected']
for rel in ('pbt4d-red-v54.txt', 'pbt4d-green-v54.txt', 'red_pbt4d_v53.py', 'apply_red_pbt4d_v53.py', 'green_pbt4d_v53.py', 'apply_green_pbt4d_v53.py'):
    assert p6['Pieces'][rel] == sha(os.path.join(HERE, rel)), rel
LIST_FILE = p6['List']['File']
for rel in (LIST_FILE, 'pbt4d_test_edits_v54.py', 'v54/make_red_v54.py', 'v53/make_green_v53.py', 'v54/make_list_v54.py', 'v53/make_run_eq_v53.py',
            'v53/unused_v53.py', 'v52/make_fullcopy_eq_v52.py'):
    assert p6['Pieces'][rel] == sha(os.path.join(HERE, rel)), rel
P6_SHA = sha(os.path.join(CC, 'pbt4d-precondition6.json'))
CC_SHA = sha(os.path.join(CC, 'compile-check.md'))
LIST = os.path.join(HERE, LIST_FILE)
LIST_SHA = sha(LIST)
rows = [l.split('\t') for l in open(LIST, encoding='utf-8').read().rstrip('\n').split('\n')[1:]]
N_ACTION = sum(1 for r in rows if r[6] != 'se queda')
assert LIST_FILE == 'pbt4d-retire-list-%d.tsv' % N_ACTION
ed = load('pbt4d_test_edits_v54', os.path.join(HERE, 'pbt4d_test_edits_v54.py'))
rp = load('red_pbt4d_v53', os.path.join(HERE, 'red_pbt4d_v53.py'))
base_r, red_r, new_r, del_r = rp.red_full(REPO, REV, os.path.join(HERE, 'pbt4d-red-v54.txt'))
RED_EDITED = sorted(p for p in red_r if p not in new_r)
RED_NEW = ['tests/RackCad.Tests/PushBackRetirePilotGuardTests.cs', 'tests/RackCad.Tests/PushBackRetirePilotCensusGuardTests.cs',
           'tests/RackCad.Tests/PushBackRetirePilotRunTests.cs', 'tests/RackCad.Tests/PushBackRetirePilotFullCopyGuardTests.cs',
           'tests/RackCad.Tests/PushBackRetirePilotPlanGuardTests.cs']
assert len(RED_EDITED) == 23 and len(del_r) == 3 and sorted(new_r) == sorted(RED_NEW), (len(RED_EDITED), len(del_r), sorted(new_r))
NEWSHA = {p: hashlib.sha256(red_r[p].encode('utf-8')).hexdigest() for p in RED_NEW}
for p in RED_NEW:
    assert p6['RedFiles'][p]['sha256'] == NEWSHA[p], p
assert sorted(p6['RedDeleted']) == sorted(del_r)
run_text = red_r['tests/RackCad.Tests/PushBackRetirePilotRunTests.cs']
assert len(re.findall(r'^    public void (\w+)\(', run_text, re.M)) == 29
N_UNUSED = len(ed.UNUSED)

# ------------------------------------------------------------------ los valores
V = {'LIST_FILE': LIST_FILE, 'LIST_SHA': LIST_SHA, 'LIST_ROWS': str(len(rows)), 'N_ACTION': str(N_ACTION), 'N_UNUSED': str(N_UNUSED),
     'RED_SHA': p6['Pieces']['pbt4d-red-v54.txt'], 'GREEN_SHA': p6['Pieces']['pbt4d-green-v54.txt'], 'P6_SHA': P6_SHA, 'CC_SHA': CC_SHA}
V['P6_COPY'] = '`/d/%s`' % [k for k in ('r6y', 'r6z', 'r7a', 'r7b', 'r7c') if os.path.isdir(os.path.join(CC, k + '-p6'))][-1]
V['CT_COPY'] = '`/d/%s`' % [k for k in ('r6y', 'r6z', 'r7a', 'r7b', 'r7c') if os.path.isdir(os.path.join(CC, k + '-ct'))][-1]
for key in ('red_pbt4d_v53.py', 'apply_red_pbt4d_v53.py', 'green_pbt4d_v53.py', 'apply_green_pbt4d_v53.py', 'v54/make_red_v54.py', 'pbt4d_test_edits_v54.py',
            'v53/make_run_eq_v53.py', 'v52/make_fullcopy_eq_v52.py', 'v53/make_green_v53.py', 'v54/make_list_v54.py', 'v53/unused_v53.py'):
    V['SHA_' + key] = p6['Pieces'].get(key) or sha(os.path.join(HERE, key))
for k, name in (('SHA_O1', 'owner-pbt4d-list-decisions.md'), ('SHA_O2', 'owner-pbt4d-substitutes-decision.md'),
                ('SHA_O3', 'owner-pbt4d-run-coverage-decision.md'), ('SHA_O4', 'owner-pbt4d-deadcode-27-decisions.md'),
                ('SHA_O5', 'owner-pbt4d-cat4d-decisions.md'), ('SHA_S1', 'session-pbt4d-design-decisions.md'),
                ('SHA_REVIEW', 'independent-review-pbt4d-check.md')):
    V[k] = sha(os.path.join(HERE, name))
existing = sorted(RED_EDITED + list(del_r))
V['RED_EDITED'] = ', '.join('%d. `%s`%s' % (i, p, ' (se borra)' if p in del_r else '') for i, p in enumerate(existing, 7))
V['RED_NEW'] = ', '.join('%d. `%s`' % (i, p) for i, p in enumerate(RED_NEW, 33))
WHAT = {RED_NEW[0]: 'CT-PBT4D-01; **`ExpectRed` true**', RED_NEW[1]: 'CT-PBT4D-02; `ExpectRed` false',
        RED_NEW[2]: 'las 29 sustitutas de WPUR sobre el modelo completo, con `FullUpdateSetup` (`v53/make_run_eq_v53.py`): 27 de la corrida genérica sobre `ExecuteFull` y la n.º 48 y la n.º 49 de `PushBackPilotSystemRebuild` (CA-T4d I-01); 26 por el mapa de PB-T4b; a mano la n.º 10, la n.º 43 y la n.º 49 (solo su comentario); `ExpectRed` false',
        RED_NEW[3]: 'el equivalente de CT-PBT4B-20 de PB-T4c, copia literal con los `BaseSha`, los `Skip`, las `CopySha` y los recuentos que PB-T4d cambia (`make_fullcopy_eq_v52.py`, sobre los textos finales del RED de la v5.3); `ExpectRed` false',
        RED_NEW[4]: 'las dos guardas de `WorkspacePilotPrepareGuardTests` (D-S12), cada una con su cambio declarado; `ExpectRed` false'}
V['NEWFILES'] = '\n'.join('- `%s`, `%s`: %s' % (p, NEWSHA[p], WHAT[p]) for p in RED_NEW)
skip_lines = []
for path, method, reason in ed.SKIPS:
    base = base_r[path].split('\n')
    n = next(i for i, l in enumerate(base) if re.match(r'\s*public\s+void\s+' + re.escape(method) + r'\s*\(', l))
    assert base[n - 1].strip() == '[Fact]', (path, method)
    skip_lines.append('- `%s`:%d (`%s`): `[Fact(Skip = "%s")]`' % (path, n, method, reason))
V['SKIPS'] = '\n'.join(skip_lines)
req = [l.split('\t') for l in open(os.path.join(CC, 'required-pbt4d.tsv'), encoding='utf-8').read().rstrip('\n').split('\n')]
assert [r[2] for r in req[1:] if r[4] == 'true'] == ['FullyQualifiedName~RackCad.Tests.PushBackRetirePilotGuardTests'], 'solo CT-PBT4D-01 lleva ExpectRed'
V['REQ_N'] = '%d: %d heredados y %d nuevos' % (len(req) - 1, sum(1 for r in req[1:] if r[5].startswith('PB-T4c')), sum(1 for r in req[1:] if r[5].startswith('nuevo')))
V['REQ_TABLE'] = '\n'.join(['| n.º | Project | Filter | MinSelected | ExpectRed | Origen |', '|---|---|---|---|---|---|'] +
                           ['| %s | `%s` | `%s` | %s | %s | %s |' % (r[0], r[1], r[2], r[3], r[4], r[5].replace('PB-T4c n.o', 'PB-T4c n.º')) for r in req[1:]])
gone = re.findall(r'sale el heredado n\.o (\d+) de PB-T4c: (\S+) \((\d+); base (\d+), GREEN (\d+)\)', open(os.path.join(CC, 'required-pbt4d.txt'), encoding='utf-8').read())
V['REQ_GONE'] = ('salen los que quedan en 0: ' + ' y '.join('el n.º %s (`%s`, de %s a 0: %s resultados en la base y %s en el GREEN)' % g for g in gone) + '.') if gone else 'ninguno queda en 0.'
run_req = [r for r in req[1:] if r[2].endswith('PushBackRetirePilotRunTests')]
V['RUN_RESULTS'] = run_req[0][3]
cr, cg = p6['Red']['Core'], p6['Green']['Core']
ui_red, ui_green = p6['UiTotals']['red'], p6['UiTotals']['green']


def ci(core, ui_line, failed_note):
    m = re.match(r'UI \w+: (\d+) = (\d+) \+ (\d+) \+ (\d+)', ui_line)
    return 'Core %d resultados, con %d Passed, %d Failed%s y %d Skipped; UI %s, con %s Passed, %s Failed y %s Skipped' % (
        core['Total'], core['Passed'], core['Failed'], failed_note, core['Skipped'], m.group(1), m.group(2), m.group(3), m.group(4))


V['CI_GREEN'] = ci(cg, ui_green, '')
V['CI_RED'] = ci(cr, ui_red, ' (CT-PBT4D-01)')
cov_txt = open(os.path.join(CC, 'cov-p6-v54.txt'), encoding='utf-8').read()
V['COV_SUMMARY'] = p6['Coverage']['Summary'] + '; **' + p6['Coverage']['Verdict'] + '** (`cov-p6-v54.txt`); ' + p6['Coverage']['Watched']
V['M02'] = open(os.path.join(V53, 'm02.txt'), encoding='utf-8').read().strip()
# recomprobacion 1 de CA-T4d, M-08: las claves del JSON, en su orden; el generador lee RedDeleted (comparada con los DELETE) y no lee Noise
KEY_NOTE = {'RedDeleted': '`RedDeleted` (los 3 borrados; el generador la compara con los bloques DELETE de la pieza del RED)',
            'Noise': '`Noise` (informativo: el ruido de D-S16; el generador no lo lee)',
            'GreenUiCoverage': '`GreenUiCoverage` (las 3 corridas juntas y los 18 filtros del GREEN, con el sha256 de su TRX y de su JSON)',
            'Reapply': '`Reapply` (las piezas de la v5.4 reaplicadas en `/d/r70`)'}
meta = [k for k in p6 if k in ('Schema', 'Package', 'Task')]
keys = [k for k in p6 if k not in meta]
V['P6_KEYS'] = 'metadatos %s; y, en este orden, %s; `Green.UI[n]` lleva también `Json`, y `BaseCoverage.UIByFilter.Runs`, el sha256 completo de la TRX y del JSON de cada filtro' % (
    ', '.join('`%s`' % k for k in meta), ', '.join(KEY_NOTE.get(k, '`%s`' % k) for k in keys))
assert 'GreenUiCoverage' in p6 and 'RedDeleted' in p6 and 'Noise' in p6 and 'Json' in p6['Green']['UI'][0]
V['BAJAN_SHORT'] = p6['Coverage']['Bajan']['Summary'] + '; igual que con la regla de la v5.3'
bj = p6['Coverage']['Bajan']
assert not bj['Perdida'], bj['Perdida']
V['BAJAN'] = ('%s; **ninguna PERDIDA**; VARIA, declaradas (una prueba que no cambia y una cobertura que varía entre corridas, no caminos perdidos): %s; '
              'las ESTABLE (%d) las sigue cubriendo el GREEN con Core, o con las tres corridas de todos los filtros juntos y al menos un filtro en su proceso (`cov-p6-v54.txt`, «Bajan (informativo)»). '
              'Las 37-38 de `RackViewBatchDialogPresenter.cs`, el ruido que motivó D-S16, no bajan en esta corrida (la unión las cubre en los dos lados); se declaran igual en `compile-check.md`: no son un camino perdido') % (
    bj['Summary'], '; '.join('`%s`' % v for v in bj['Varia']) or 'ninguna', bj['Estable'])
gb = p6['BaseCoverage']
V['MEASURED'] = ('%s para los pasos 2 a 7 (el RED, el GREEN y la cobertura del GREEN; **sola**: %d procesos al empezar), `/d/r6w` para la referencia de la '
                 'cobertura (la base sin cambios, sola: la suite de Core, %d = %d + %d + %d, y la unión de los filtros de UI, %d = %d + %d + %d) y la base de los '
                 '`MinSelected`, y %s para el control del ensayo; el censo, por objetos git. Lo previsto, con cero `warning CS`:\n'
                 '- **Core:** en el RED, %d = %d + %d + %d, y solo cae CT-PBT4D-01, por aserción; en el GREEN, %d = %d + %d + %d;\n'
                 '- **UI,** los 18 filtros del contrato: 0 `Failed` en las dos ramas;\n'
                 '- **la duración de la suite de Core:** %s s en el RED y %s s en el GREEN (%s s con la cobertura); **la prueba lenta acotada:** %s s en el RED y %s s en el GREEN (la de PB-T4c tardaba unos 390 s sola, `/d/r6f`);\n'
                 '- **la cobertura ampliada:** %s;\n'
                 '- **el ensayo:** CT-01 cae en el RED y pasa en el GREEN; CT-02 pasa en las dos; **su control:** sobre la base con solo las dos CT, caen las dos; sobre el RED con la variante A en `src/`, cae CT-01 y pasa CT-02; **el censo:** %s;\n'
                 '- **los totales de UI de la CI** (la línea base menos lo que quitan las bajas de UI, medido en la unión de los filtros): %s y %s.\n\n'
                 '**Las corridas anteriores** son historia: las de la v5.2 (`/d/r6a` a `/d/r6u`; lo de `/d/r6r`, declarado en `compile-check.md`, CA-T4d M-07) y los dos ensayos de la v5.3 (`/d/r6v` y `/d/r6x`).') % (
    V['P6_COPY'], p6['Alone']['ProcessesAtStart'], gb['Core']['Total'], gb['Core']['Passed'], gb['Core']['Failed'], gb['Core']['Skipped'],
    gb['UI']['Total'], gb['UI']['Passed'], gb['UI']['Failed'], gb['UI']['Skipped'], V['CT_COPY'],
    cr['Total'], cr['Passed'], cr['Failed'], cr['Skipped'], cg['Total'], cg['Passed'], cg['Failed'], cg['Skipped'],
    p6['SuiteDurationSeconds']['Red'], p6['SuiteDurationSeconds']['Green'], p6['SuiteDurationSeconds']['GreenWithCoverage'],
    p6['SlowTestSeconds']['Red'], p6['SlowTestSeconds']['Green'], V['COV_SUMMARY'], p6['Census']['Verdict'], ui_red, ui_green)
counts = collections.Counter((r[1], r[6]) for r in rows)
V['LIST_TABLE'] = '\n'.join(['| Grupo | Acción | Filas |', '|---|---|---|'] + ['| %s | %s | %d |' % (g, a, n) for (g, a), n in sorted(counts.items())] +
                            ['| **Total** | | **%d** (%d con acción) |' % (len(rows), N_ACTION)])

# las copias de PB-T4c en el bloque literal de 6.3.1 (como en make_v52.py)
block = t[t.index('TaskId: F4B-PBT4-EDIT-ALL-WINDOW'):]
block = block[:block.index('```')]
blines = block.split('\n')


def copy_line(prefix):
    ls = [l for l in blines if l.startswith(prefix)]
    assert len(ls) == 1, prefix
    return ls[0].replace('PBT4C', 'PBT4D').replace('PB-T4c', 'PB-T4d')


V['INV_COMMON_A'] = '\n'.join(copy_line('INV-PBT4C-%02d.' % n) for n in (8, 9, 10, 11, 12))
V['INV_COMMON_B'] = copy_line('INV-PBT4C-14.')
for c in ('08', '11', '12', '17'):
    V['C' + c] = copy_line('PBT4C-C%s:' % c)
V['EV0'] = copy_line('[0] ')
V['EV7'] = copy_line('[7] ')
V['EV12'] = copy_line('[12] ')
ev8 = copy_line('[8] ')
for old, new in (('dentro de las cinco clases de las CT', 'dentro de las dos clases de las CT'),
                 ('En PB-T4d, ademas: RedSha..RedRepairSha no toca ningun archivo de src/ (el esqueleto), ni los cuatro equivalentes, ni los cuatro archivos de los Skip.',
                  'En PB-T4d, ademas: RedSha..RedRepairSha no toca ningun archivo de src/, ni los 26 archivos existentes de tests/ que el RED edita o borra, ni las tres clases nuevas que no son CT.')):
    assert ev8.count(old) == 1, old
    ev8 = ev8.replace(old, new)
V['EV8'] = ev8


def fill(text):
    for k, v in V.items():
        text = text.replace('@@' + k + '@@', v)
    left = re.findall(r'@@[A-Za-z0-9_./-]+@@', text)
    assert not left, left
    return text


# ------------------------------------------------------------------ 1. el titulo, la cabecera nueva y el registro v5.2 -> v5.3
TITLE = '# I-64 F4B — Paquete del contrato de PB-T4 («Editar todo» en el panel del Workspace), v5.2\n'
# la cabecera de la v5.3, tal como se publico (de la v5.3, por su texto), con lo que corrige M-10 (recomprobacion 1 de CA-T4d)
v53_raw = open(os.path.join(HERE, 'I-64-f4b-pbt4-gate-contract-package-v53.md'), 'rb').read()
assert hashlib.sha256(v53_raw).hexdigest() == 'ae338b2d2ae4cd25d0d78306dad2f7468df94e54d9159c7e1a63f7d06c3e9daf'
t53 = v53_raw.decode('utf-8')
HEAD53 = t53[t53.index('**Estado:** v5.3.'):t53.index('**Estado:** v5.2.')]
for old, new in (
        ('**Lo que cambia:** PB-T4d: D-PBT4-10, §4.4, §5.6, §6.4 (con §6.4.1 y §6.4.2), §8.3, §8.5, §11 y el anexo, que pasa a V53-A.',
         '**Lo que cambia:** PB-T4d: §0, §1, D-PBT4-10, §4.4, §5.6, §6.0, §6.4 (con §6.4.1 y §6.4.2), §7, §8.3, §8.5, §9, §11 y el anexo, que pasa a V53-A '
         '(§0, §1, §6.0, §7 y §9, nombrados desde la v5.4: recomprobación 1 de CA-T4d, M-10).'),
        ('| D-PBT4-10; §4.4; §5.6; §6.4; la lista; el anexo |', '| §0; §1; D-PBT4-10; §4.4; §5.6; §6.4; la lista; el anexo |'),
        ('Reescritos INV-PBT4D-06, PBT4D-C16, el paso 7 y «Lo que la medida no ve» de §4.4.', 'Reescritos INV-PBT4D-06, PBT4D-C16, el paso 7 y «Lo que la medida no ve» de §4.4 (§1: D-S16; §7: R-T4-34 y R-T4-38).'),
        ('| §4.4; §6.4; la lista (grupo `M-03`) |', '| §4.4; §6.0 (RR-2); §6.4; §7 (R-T4-36); la lista (grupo `M-03`) |'),
        ('| §6.4.1; `compile-check.md` |', '| §6.4.1; §7 (R-T4-37); `compile-check.md` |'),
        ('`Package` `v5.3` | §6.4.1; §11 |', '`Package` `v5.3` | §6.4.1; §9; §11 |')):
    assert HEAD53.count(old) == 1, old[:60]
    HEAD53 = HEAD53.replace(old, new)
R(TITLE + '\n**Estado:** v5.2.', TITLE.replace('v5.2', 'v5.4') + '\n' + fill(frag('head54.md')) + HEAD53 + '**Estado:** v5.2.', what='titulo, cabecera v5.4, registro v5.4 y cabecera de la v5.3 (M-10)')

# ------------------------------------------------------------------ 2. §0 y §1
row0 = line_starting('| **PB-T4d** | `F4B-PBT4-RETIRE-PILOT` (21) |')
R(row0, row0.replace('| **229** filas con acción de `pbt4d-retire-list-229.tsv` (borrar, reescribir, sustituir y acotar con registro), con las 27 sustitutas de la corrida (§4.4) |',
                     '| **%d** filas con acción de `%s` (borrar, reescribir, sustituir y acotar con registro), con las 29 sustitutas de WPUR y los ayudantes sin uso borrados (§4.4; v5.3) |' % (N_ACTION, LIST_FILE)),
  what='§0 la fila de PB-T4d')
row1 = line_starting('| **Las decisiones de diseño de la sesión para PB-T4d** (')
R(row1 + '\n', row1.replace('D-S11 a D-S15 (§8.5)', 'D-S11 a D-S15 y, tras CA-T4d, D-S16 (§8.5)') + '\n' +
  '| **La comprobación acotada de la v5.2** (`independent-review-pbt4d-check.md`, 2026-11-22) | EMITIBLE TRAS CORRECCIONES, B 0 / I 2 / M 7: la v5.3 corrige los nueve (registro v5.2 → v5.3) |\n'
  '| **Las dos decisiones del Owner tras CA-T4d** (`owner-pbt4d-cat4d-decisions.md`, 2026-11-22, la 14.ª) | «Sustituirlas» (la 48 y la 49) y «Sustituirlos siempre» (la regla de los huecos de la cobertura ampliada) (§8.5) |\n',
  what='§1 las autoridades nuevas')

# ------------------------------------------------------------------ 3. D-PBT4-10, §4.4 y §5.6
section('**Lo que retira PB-T4d** (v5.2;', '#### D-PBT4-11.', fill(frag('d10.md')), 'D-PBT4-10, lo que retira PB-T4d')
section('### 4.4 PB-T4d (V52-01 a V52-04)', '## 5. La red de pruebas', fill(frag('s44.md')) + '\n', '§4.4')
section('### 5.6 PB-T4d: la lista cerrada CT-PBT4D-01 y 02', '## 6. Los contratos propuestos', fill(frag('s56.md')) + '\n', '§5.6')

# ------------------------------------------------------------------ 4. §6.0
R('**En PB-T4d,** el RED edita 26 archivos existentes de `tests/` y añade cinco:',
  '**En PB-T4d,** el RED cambia 26 archivos existentes de `tests/` (edita 23 y, desde la v5.3, borra 3) y añade cinco:', what='§6.0 RR-2')

# ------------------------------------------------------------------ 5. §6.4
s64 = fill(frag('s64.md'))
lit = s64[s64.index('```text\nTaskId: F4B-PBT4-RETIRE-PILOT'):]
lit = lit[:lit.index('\n```\n') + 5]
assert all(ord(ch) < 0x80 for ch in lit), [ch for ch in lit if ord(ch) >= 0x80][:5]
section('### 6.4 PB-T4d — `F4B-PBT4-RETIRE-PILOT`', '## 7. Riesgos y lecciones aplicadas', s64 + '\n', '§6.4 entera')
R('11 bloques de solo comentarios de CA-T4d M-05 (a) |',
  '11 bloques de solo comentarios de CA-T4d M-05 (a); en la v5.4, `pbt4d-green-v54.txt` es esa pieza con solo su cabecera corregida (recomprobación 1 de CA-T4d, M-12 b) |',
  what='§6.4.2 la pieza del GREEN de la v5.4')
R('La genera `v54/make_red_v54.py`', 'La genera `v54/make_red_v54.py` (la de la v5.3 con las dos ediciones de comentarios de WPUR de M-12 a)', what='§6.4.2 la pieza del RED de la v5.4')

# ------------------------------------------------------------------ 6. §7, §8.3, §8.5, §9 y §11
r33 = line_starting('| **R-T4-33, código muerto tras la retirada** (V52-04)')
R(r33 + '\n', r33 + '\n' + '\n'.join([
    '| **R-T4-34, el universo de la cobertura** (CA-T4d I-02; V53-02) | Medir solo lo que una clase ejecutaba no ve a quien llama desde otro archivo ni lo que pierden las demás bajas: se compara toda la suite (Core y los filtros de UI) sobre todo `src/` que se queda |',
    '| **R-T4-35, un porqué que no se mide** (CA-T4d I-01; V53-01) | «Usa la corrida del piloto como vehículo» valía para 19 de las 21 bajas de WPUR; la 48 y la 49 probaban otra cosa. Un porqué de baja se comprueba con la cobertura ampliada |',
    '| **R-T4-36, código muerto en `tests/`** (CA-T4d M-03; V53-05) | Una retirada deja ayudantes sin uso y clases sin pruebas: se miden con IDE0051/IDE0052 en una copia y por texto, en vueltas hasta que no sale ninguno, y se borran con registro |',
    '| **R-T4-37, la copia principal, sola** (CA-T4d M-07; V53-09) | Una corrida solapada no cambia los resultados, pero sí las duraciones: el conductor comprueba al empezar que no hay otro `dotnet` ni `testhost` |',
    '| **R-T4-38, el ruido de la cobertura de UI** (V53-02) | Un binding de WPF da golpes que dependen del despachador (`RackViewBatchDialogPresenter.cs:37-38`: la base da 1, 0 y 1 en tres corridas de la misma prueba, y en la unión de los filtros el GREEN da 0 tres veces y 1 tres veces con su filtro solo, `noise-batch/`): la UI de cada rama es la unión de tres corridas de la unión de los filtros y de cada filtro en su proceso, como los corre el contrato |']) + '\n', what='§7 las lecciones de CA-T4d')
r83 = line_starting('| PB-T4d | La lista del alcance B fila a fila')
R(r83, '| PB-T4d | La lista del alcance B fila a fila (C-7, originales, apoyo, compilación, ejecución, M-04, la lenta, SUS-01, M-03 y los ayudantes editados), con las 29 sustitutas de WPUR, la rama muerta de `BuildCore` y, desde la v5.3, sus declaraciones y su tramo editado | El esbozo: 138 + 55 + 29; B medida: 235 bajas | **%d** con acción (%d filas) | **La elegida** (`owner-pbt4d-list-decisions.md`, pregunta 1, con `owner-pbt4d-substitutes-decision.md`, `owner-pbt4d-run-coverage-decision.md`, `owner-pbt4d-deadcode-27-decisions.md` y `owner-pbt4d-cat4d-decisions.md`); `%s`, sha256 `%s` (la de la v5.2, `pbt4d-retire-list-229.tsv`, `1943468ba9a9f342b74bb8ea2c57e1b664c7751b52344e7a712ea7bc2e151fad`, y el esbozo, `pbt4d-retire-list-138.tsv`, `d5467dc162e2cda8c539a829cc8f3a7c46260a119f70e840b34e6f78dde0387c`, quedan como historia) |' % (N_ACTION, len(rows), LIST_FILE, LIST_SHA),
  what='§8.3 la fila de PB-T4d')
r13 = line_starting('| **13.ª** (2026-11-22) | `owner-pbt4d-deadcode-27-decisions.md`')
R(r13 + '\n', r13 + '\n' + '| **14.ª** (2026-11-22) | `owner-pbt4d-cat4d-decisions.md` | «48 y 49»: «Sustituirlas (Recomendado)». «Huecos nuevos»: «Sustituirlos siempre (Recomendado)» | La 48 y la 49 de WPUR pasan a sustitutas sobre el modelo completo (29); la cobertura de la precondición 6 mira todo `src/` que se queda; todo camino que se queda y pierde su única prueba recibe una sustituta sin volver al Owner, salvo que una no se pueda escribir o salgan más de 10 nuevas sobre las 29; lo que cubra la UI se mide con sus filtros |\n',
  what='§8.5 la 14.ª tanda')
ds = line_starting('- **D-S11 a D-S15** (`session-pbt4d-design-decisions.md`')
R(ds + '\n', ds + '\n' + '- **Tras CA-T4d** (la sesión, en su orden para la v5.3; no son decisiones del Owner): M-01, el corpus de CT-PBT3B-06 (iii) pasa al modelo completo en su sitio, con la regla del Owner; M-02, la rama de `MirrorSelection`, medida con la cobertura de los filtros de UI; M-03, los ayudantes sin uso y las clases sin pruebas, borrados con registro; M-04, la lista fila a fila con sus declaraciones y su tramo editado; M-05, los comentarios del GREEN que describían el piloto como presente, y «cuatro condiciones»; M-06, el número de la clase de las sustitutas; M-07, la copia principal sola y con los 18 filtros del contrato.\n'
  '- **D-S16** (`session-pbt4d-design-decisions.md`, 2026-11-22; para PB-T4d, tras CA-T4d): la cobertura de la UI de cada lado es la unión de tres corridas de todos los filtros juntos y de una de cada filtro en su proceso (un binding de WPF da golpes que dependen de lo que corrió antes; R-T4-38), y el informe lista lo que baja (informativo); **con su nota tras la recomprobación 1 de CA-T4d** (M-11): cada entrada nombra la corrida, no la prueba, y ESTABLE solo por la UI exige también un filtro en su proceso.\n'
  '- **Una errata, declarada** (CA-T4d M-06): la pregunta 2 de `owner-pbt4d-deadcode-27-decisions.md` dice «se borran 22 en vez de 27»; eran **21** pruebas (48 − 27): con `StatusAfter`, 22 filas de WPUR salían del archivo, pero `StatusAfter` va copiado en la clase nueva. La v5.2 la aplicó como 21 sin declararla. En la v5.3, con la 48 y la 49 sustituidas, se borran 19 y una se queda en WPUR.\n', what='§8.5 lo de la sesion tras CA-T4d y la errata')
m9 = line_starting('- **Medidos en copias exportadas, para PB-T4d** (V52-06)')
R(m9, '- **Medidos en copias exportadas, para PB-T4d** (V52-06; V53-10): la necesidad de cada baja (`/d/r6h`), la cobertura (`/d/r6j`, `/d/r6k`), los ensayos (`/d/r6l`, `/d/r6n`, `/d/r6v` y `/d/r6x`) y la precondición 6 de la v5.3 (`/d/r6w`, %s y %s); las demás, historia. **Lo que la cobertura no ve:** la cobertura de datos, el Plugin y la UI fuera de los filtros (§4.4).' % (V['P6_COPY'], V['CT_COPY']), what='§9 lo medido')
o2 = line_starting('2. **Las respuestas del Owner:**')
R(o2, o2.replace('`owner-pbt4d-run-coverage-decision.md` y `owner-pbt4d-deadcode-27-decisions.md`, y `session-pbt4d-design-decisions.md`',
                 '`owner-pbt4d-run-coverage-decision.md`, `owner-pbt4d-deadcode-27-decisions.md` y `owner-pbt4d-cat4d-decisions.md`, y `session-pbt4d-design-decisions.md`'), what='§11.2')
c6 = line_starting('   - **PB-T4d** (los nueve pasos de §6.4.1;')
R(c6, ('   - **PB-T4d** (los nueve pasos de §6.4.1; `pbt4dcc/compile/compile-check.md`), sobre copias exportadas del cierre de PB-T4c (`6c2820ba`): el RED literal y el GREEN literal, sin ningún `warning CS`; **Core:** en el RED, %d = %d + %d + %d, y solo cae CT-PBT4D-01, por aserción; en el GREEN, %d = %d + %d + %d; los 18 filtros de UI del contrato, 0 `Failed`; la copia principal, sola; **la cobertura ampliada de todo `src/` que se queda, %s**; el ensayo de las CT y su control, y el censo de C13 (%s). Medido en la v5.3 (`pbt4d-precondition6.json`, `%s`). **Una prueba que falle fuera de la lista** vuelve al Owner.'
       % (cr['Total'], cr['Passed'], cr['Failed'], cr['Skipped'], cg['Total'], cg['Passed'], cg['Failed'], cg['Skipped'], p6['Coverage']['Verdict'], p6['Census']['Verdict'], P6_SHA)), what='§11.6')
l7 = line_starting('7. **La lista que se emite es la aprobada, o una parte:**')
R(l7, l7.replace('**PB-T4d, las 229 filas con acción de `pbt4d-retire-list-229.tsv`** (sha256 `1943468ba9a9f342b74bb8ea2c57e1b664c7751b52344e7a712ea7bc2e151fad`)',
                 '**PB-T4d, las %d filas con acción de `%s`** (sha256 `%s`)' % (N_ACTION, LIST_FILE, LIST_SHA)), what='§11.7')
R('esta versión (v5.2), los fragmentos y los guiones de `v52/` (con `v52/sub/`, el alto del relevo), la lista (`pbt4d-retire-list-229.tsv`),',
  'esta versión (v5.4), los fragmentos y los guiones de `v52/` (con `v52/sub/`, el alto del relevo), de `v53/` y de `v54/`, la comprobación acotada de la v5.2 (`independent-review-pbt4d-check.md`), la lista (`%s`; la de la v5.2, `pbt4d-retire-list-229.tsv`, como historia),' % LIST_FILE,
  what='§11 la version y la lista que se custodian')
R('los cuatro registros del Owner de las tandas 10.ª a 13.ª, `session-pbt4d-design-decisions.md`',
  'los cinco registros del Owner de las tandas 10.ª a 14.ª, `session-pbt4d-design-decisions.md` (D-S11 a D-S16)', what='§11 los registros que se custodian')

# ------------------------------------------------------------------ 7. el anexo V53-A, en lugar del V52-A
green_txt = open(os.path.join(HERE, 'pbt4d-green-v54.txt'), encoding='utf-8').read()
annex = ['## Anexo V54-A. El texto literal de PB-T4d: el GREEN, las ediciones con registro, los ayudantes sin uso, los `Skip` y los archivos nuevos (v5.4)', '',
         'Las bajas con registro no van aquí: son las filas «borrar con registro» y «sustituir con registro» de la lista, y su texto es el de la pieza del RED (`pbt4d-red-v54.txt`, sha256 `%s`), que las quita. Los anexos V52-A y V53-A de la v5.2 y la v5.3 quedan como historia.' % V['RED_SHA'], '',
         '### La pieza del GREEN, `pbt4d-green-v54.txt` (sha256 `%s`)' % V['GREEN_SHA'], '', '```text'] + green_txt.rstrip('\n').split('\n') + ['```', '',
         '### Las ediciones con registro (`pbt4d_test_edits_v54.py`, `EDITS`, en su orden, sobre el texto ya sin las bajas)', '']
for path, frm, to, new, why in ed.EDITS:
    annex += ['**`%s`:** %s.' % (path, why), '', ('Desde la línea que contiene el texto de abajo hasta la anterior a la que contiene `%s`:' % to.strip()) if to else 'El texto, una vez en el archivo:', '', '```text'] + frm.rstrip('\n').split('\n') + ['```', '', 'Pasa a:', '', '```text'] + (new.rstrip('\n').split('\n') if new else ['(nada)']) + ['```', '']
old_call, new_call, times = ed.STALE_CALL
annex += ['**`tests/RackCad.Tests/Workspace/WorkspaceDraftPilotIntentStaleBaseTests.cs`:** las %d llamadas `%s` pasan a `%s`.' % (times, old_call, new_call), '',
          '### Los ayudantes sin uso tras la retirada (CA-T4d M-03; `pbt4d_test_edits_v54.py`, `UNUSED` y `DELETE_FILES`)', '',
          'Después de las ediciones, `v53/unused_v53.py` quita cada uno (su declaración, sus `///`, sus atributos y el `//` pegado encima, y una línea en blanco de las dos que la rodean); los de `edicion` los quita la última edición de WPUR. Los tres archivos, en bloques DELETE.', '']
annex += ['- `%s`: `%s`%s (%s)' % (p, m, '' if not nth else ', la declaración n.º %d del archivo' % nth, how) for p, m, why, nth, how in ed.UNUSED]
annex += ['- `%s`: **se borra el archivo** (%s)' % (p, why) for p, why in ed.DELETE_FILES] + ['', '### Los `Skip`', ''] + skip_lines + ['']
for p in RED_NEW:
    assert '```' not in red_r[p], p
    annex += ['### `%s` (`NewFileSha` `%s`)' % (p, NEWSHA[p]), '', '```csharp'] + red_r[p].rstrip('\n').split('\n') + ['```', '']
assert '```' not in green_txt and all('```' not in (e[1] + e[3]) for e in ed.EDITS)
a = t.index('## Anexo V52-A.')
t = t[:a] + '\n'.join(annex).rstrip('\n') + '\n'
log.append('reemplazo el anexo V52-A por el V53-A')

# ------------------------------------------------------------------ las comprobaciones
assert t.count('## Anexo V54-A') == 1 and '## Anexo V52-A' not in t and t.count('### 6.4 PB-T4d — `F4B-PBT4-RETIRE-PILOT`') == 1
assert t.count('| **V53-02** |') == 1 and t.count('### 4.4 PB-T4d') == 1 and t.count('### 5.6 PB-T4d') == 1
assert not re.search(r'@@[A-Za-z0-9_./-]+@@', t)
for stale in ('pbt4d-red-v52.txt', 'pbt4d-green-v52.txt`; el anexo', 'cov_p6_v52.py', 'p6_pbt4d_v52.sh'):
    assert stale not in t[t.index('### 6.4 PB-T4d'):t.index('## 7. Riesgos')], stale
fences = [l for l in t.split('\n') if l.startswith('```')]
assert len(fences) % 2 == 0
data = t.encode('utf-8')
assert b'\r' not in data
with open(OUT, 'wb') as f:
    f.write(data)
out = os.path.join(HERE, 'v54', 'make_v54-out.txt')
log.append('v5.4 %s %d bytes; reemplazos %d' % (hashlib.sha256(data).hexdigest(), len(data), len([l for l in log if l.startswith('reemplazo')])))
with open(out, 'w', encoding='utf-8', newline='\n') as f:
    f.write('\n'.join(log) + '\n')
print(log[-1])
