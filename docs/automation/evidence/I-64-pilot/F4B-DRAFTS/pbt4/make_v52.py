"""PB-T4 (I-64 F4B): arma la v5.2 del paquete desde la v5.1 (`I-64-f4b-pbt4-gate-contract-package-v51.md`, sha256 34ca0026...94c3), con PB-T4d
en su forma literal emitible. Cada cambio es un reemplazo por anclaje exacto, contado (R). Los fragmentos estan en v52/frag/ y v52/frag_56.md; los
valores medidos salen de la precondicion 6 (pbt4dcc/compile/pbt4d-precondition6.json, que se comprueba: todo lo previsto y las piezas con el sha256
de hoy); el anexo V52-A, de las piezas (el GREEN, las ediciones, los Skip y los archivos nuevos de la pieza del RED). Escribe UTF-8 con LF.

Uso: python -I make_v52.py <repo>
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
SRC = os.path.join(HERE, 'I-64-f4b-pbt4-gate-contract-package-v51.md')
OUT = os.path.join(HERE, 'I-64-f4b-pbt4-gate-contract-package-v52.md')
CC = os.path.normpath(os.path.join(HERE, '..', 'pbt4dcc', 'compile'))
V52 = os.path.join(HERE, 'v52')
raw = open(SRC, 'rb').read()
assert hashlib.sha256(raw).hexdigest() == '34ca002650780aeb1347ee2db7c4d7c285578e2e99634129ca006a6e835094c3'
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


def frag(name):
    return open(os.path.join(V52, name), encoding='utf-8').read()


# ------------------------------------------------------------------ la precondicion 6
p6 = json.load(open(os.path.join(CC, 'pbt4d-precondition6.json'), encoding='utf-8'))
assert p6['Package'] == 'v5.2' and p6['Revision'] == REV and p6['AllAsExpected'], p6['Expected']
PIECES = {'pbt4d-red-v52.txt': 'pbt4d-red-v52.txt', 'pbt4d-green-v52.txt': 'pbt4d-green-v52.txt', 'red_pbt4d_v52.py': 'red_pbt4d_v52.py',
          'apply_red_pbt4d_v52.py': 'apply_red_pbt4d_v52.py', 'green_pbt4d_v52.py': 'green_pbt4d_v52.py', 'apply_green_pbt4d_v52.py': 'apply_green_pbt4d_v52.py'}
for key, rel in PIECES.items():
    assert p6['Pieces'][key] == sha(os.path.join(HERE, rel)), key
for rel in ('pbt4d-retire-list-229.tsv', 'pbt4d_test_edits_v52.py', 'v52/make_red_v52.py', 'v52/make_green_v52.py', 'v52/make_list_v52.py',
            'v52/make_run_eq_v52.py', 'v52/make_fullcopy_eq_v52.py'):
    assert p6['Pieces'][rel] == sha(os.path.join(HERE, rel)), rel
P6_SHA = sha(os.path.join(CC, 'pbt4d-precondition6.json'))
CC_SHA = sha(os.path.join(CC, 'compile-check.md'))
LIST = os.path.join(HERE, 'pbt4d-retire-list-229.tsv')
LIST_SHA = sha(LIST)
rows = [l.split('\t') for l in open(LIST, encoding='utf-8').read().rstrip('\n').split('\n')[1:]]
assert len(rows) == 242 and sum(1 for r in rows if r[6] != 'se queda') == 229
ed = load('pbt4d_test_edits_v52', os.path.join(HERE, 'pbt4d_test_edits_v52.py'))
rp = load('red_pbt4d_v52', os.path.join(HERE, 'red_pbt4d_v52.py'))
base_r, red_r, new_r = rp.red(REPO, REV)
RED_EDITED = sorted(p for p in red_r if p not in new_r)
RED_NEW = ['tests/RackCad.Tests/PushBackRetirePilotGuardTests.cs', 'tests/RackCad.Tests/PushBackRetirePilotCensusGuardTests.cs',
           'tests/RackCad.Tests/PushBackRetirePilotRunTests.cs', 'tests/RackCad.Tests/PushBackRetirePilotFullCopyGuardTests.cs',
           'tests/RackCad.Tests/PushBackRetirePilotPlanGuardTests.cs']
assert len(RED_EDITED) == 26 and sorted(new_r) == sorted(RED_NEW), (len(RED_EDITED), sorted(new_r))
NEWSHA = {p: hashlib.sha256(red_r[p].encode('utf-8')).hexdigest() for p in RED_NEW}
for p in RED_NEW:
    assert p6['RedFiles'][p]['sha256'] == NEWSHA[p], p

# ------------------------------------------------------------------ los valores
V = {'LIST_SHA': LIST_SHA, 'RED_SHA': p6['Pieces']['pbt4d-red-v52.txt'], 'GREEN_SHA': p6['Pieces']['pbt4d-green-v52.txt'], 'P6_SHA': P6_SHA, 'CC_SHA': CC_SHA}
for key in ('red_pbt4d_v52.py', 'apply_red_pbt4d_v52.py', 'green_pbt4d_v52.py', 'apply_green_pbt4d_v52.py', 'v52/make_red_v52.py', 'pbt4d_test_edits_v52.py',
            'v52/make_run_eq_v52.py', 'v52/make_fullcopy_eq_v52.py', 'v52/make_green_v52.py', 'v52/make_list_v52.py'):
    V['SHA_' + key] = p6['Pieces'].get(key) or sha(os.path.join(HERE, key))
for k, name in (('SHA_O1', 'owner-pbt4d-list-decisions.md'), ('SHA_O2', 'owner-pbt4d-substitutes-decision.md'),
                ('SHA_O3', 'owner-pbt4d-run-coverage-decision.md'), ('SHA_O4', 'owner-pbt4d-deadcode-27-decisions.md'),
                ('SHA_S1', 'session-pbt4d-design-decisions.md')):
    V[k] = sha(os.path.join(HERE, name))
V['RED_EDITED'] = ', '.join('%d. `%s`' % (i, p) for i, p in enumerate(RED_EDITED, 7))
V['RED_NEW'] = ', '.join('%d. `%s`' % (i, p) for i, p in enumerate(RED_NEW, 33))
WHAT = {RED_NEW[0]: 'CT-PBT4D-01; **`ExpectRed` true**', RED_NEW[1]: 'CT-PBT4D-02; `ExpectRed` false',
        RED_NEW[2]: 'las 27 sustitutas de WPUR sobre `ExecuteFull`, con `FullUpdateSetup` (`make_run_eq_v52.py`): 25 por el mapa de PB-T4b; a mano la n.º 10 y la n.º 43; `ExpectRed` false',
        RED_NEW[3]: 'el equivalente de CT-PBT4B-20 de PB-T4c, copia literal con los `BaseSha`, los `Skip`, las `CopySha` y los recuentos que PB-T4d cambia (`make_fullcopy_eq_v52.py`); `ExpectRed` false',
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
assert len(req) - 1 == 88
V['REQ_N'] = '%d: %d heredados y %d nuevos' % (len(req) - 1, sum(1 for r in req[1:] if r[5].startswith('PB-T4c')), sum(1 for r in req[1:] if r[5].startswith('nuevo')))
V['REQ_TABLE'] = '\n'.join(['| n.º | Project | Filter | MinSelected | ExpectRed | Origen |', '|---|---|---|---|---|---|'] +
                           ['| %s | `%s` | `%s` | %s | %s | %s |' % (r[0], r[1], r[2], r[3], r[4], r[5].replace('PB-T4c n.o', 'PB-T4c n.º')) for r in req[1:]])
gone = re.findall(r'sale el heredado n\.o (\d+) de PB-T4c: (\S+) \((\d+); base (\d+), GREEN (\d+)\)', open(os.path.join(CC, 'required-pbt4d.txt'), encoding='utf-8').read())
V['REQ_GONE'] = ('salen los que quedan en 0: ' + ' y '.join('el n.º %s (`%s`, de %s a 0: %s resultados en la base y %s en el GREEN)' % g for g in gone) + '.') if gone else 'ninguno queda en 0.'
cr, cg = p6['Red']['Core'], p6['Green']['Core']
ui_red, ui_green = p6['UiTotals']['red'], p6['UiTotals']['green']


def ci(core, ui_line, failed_note):
    m = re.match(r'UI \w+: (\d+) = (\d+) \+ (\d+) \+ (\d+)', ui_line)
    return 'Core %d resultados, con %d Passed, %d Failed%s y %d Skipped; UI %s, con %s Passed, %s Failed y %s Skipped' % (
        core['Total'], core['Passed'], core['Failed'], failed_note, core['Skipped'], m.group(1), m.group(2), m.group(3), m.group(4))


V['CI_GREEN'] = ci(cg, ui_green, '')
V['CI_RED'] = ci(cr, ui_red, ' (CT-PBT4D-01)')
V['MEASURED'] = ('`/d/r6r` para los pasos 2 a 7 (el RED, el GREEN y la cobertura del GREEN), `/d/r6s` para la referencia de la cobertura (la base sin '
                 'cambios), `/d/r6t` para el control del ensayo y `/d/r6u` para los 19 filtros de UI de los `RequiredTests` en la base (y el de '
                 '`PushBackEditAllWindowTests` con el RED y con el GREEN); el censo, por objetos git. Lo previsto, con cero `warning CS`:\n'
                 '- **Core:** en el RED, %d = %d + %d + %d, y solo cae CT-PBT4D-01, por aserción; en el GREEN, %d = %d + %d + %d;\n'
                 '- **UI,** los 18 filtros: 0 `Failed` en las dos ramas;\n'
                 '- **la duración de la suite de Core:** %s s en el RED y %s s en el GREEN (%s s con la cobertura); **la prueba lenta acotada:** %s s en el RED y %s s en el GREEN (la de PB-T4c tardaba unos 390 s sola, `/d/r6f`);\n'
                 '- **la cobertura:** %s; **%s** (`cov-p6-v52.txt`);\n'
                 '- **el ensayo:** CT-01 cae en el RED y pasa en el GREEN; CT-02 pasa en las dos; **su control:** sobre la base con solo las dos CT, caen las dos; sobre el RED con la variante A en `src/`, cae CT-01 y pasa CT-02; **el censo:** %s;\n'
                 '- **los totales de UI de la CI** (la línea base menos lo que quitan las bajas de UI, medido en los 18 filtros): %s y %s.\n\n'
                 '**Las corridas anteriores** (`/d/r6a` a `/d/r6q`) son historia: medidas de la lista, de la necesidad y de la cobertura, dos ensayos con borradores del RED, y `/d/r6p` y `/d/r6q`, cortadas por el cierre de una sesión (`compile-check.md`).') % (
    cr['Total'], cr['Passed'], cr['Failed'], cr['Skipped'], cg['Total'], cg['Passed'], cg['Failed'], cg['Skipped'],
    p6['SuiteDurationSeconds']['Red'], p6['SuiteDurationSeconds']['Green'], p6['SuiteDurationSeconds']['GreenWithCoverage'],
    p6['SlowTestSeconds']['Red'], p6['SlowTestSeconds']['Green'], p6['Coverage']['Summary'], p6['Coverage']['Verdict'], p6['Census']['Verdict'], ui_red, ui_green)
counts = collections.Counter((r[1], r[6]) for r in rows)
V['LIST_TABLE'] = '\n'.join(['| Grupo | Acción | Filas |', '|---|---|---|'] + ['| %s | %s | %d |' % (g, a, n) for (g, a), n in sorted(counts.items())] +
                            ['| **Total** | | **%d** (%d con acción) |' % (len(rows), sum(1 for r in rows if r[6] != 'se queda'))])

# las copias de PB-T4c en el bloque literal de 6.3.1
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
                  'En PB-T4d, ademas: RedSha..RedRepairSha no toca ningun archivo de src/, ni los 26 archivos existentes de tests/ que el RED edita, ni las tres clases nuevas que no son CT.')):
    assert ev8.count(old) == 1, old
    ev8 = ev8.replace(old, new)
V['EV8'] = ev8


def fill(text):
    for k, v in V.items():
        text = text.replace('@@' + k + '@@', v)
    left = re.findall(r'@@[A-Za-z0-9_./-]+@@', text)
    assert not left, left
    return text


# ------------------------------------------------------------------ 1. el titulo, la cabecera y el registro
TITLE = '# I-64 F4B — Paquete del contrato de PB-T4 («Editar todo» en el panel del Workspace), v5.1\n'
R(TITLE + '\n', TITLE.replace('v5.1', 'v5.2') + '\n' + fill(frag('frag/head.md')), what='titulo y cabecera')
R('\n---\n\n## Registro de cambios v5.0 → v5.1\n', '\n---\n\n## Registro de cambios v5.0 → v5.1\n', what='el registro anterior se queda')

# ------------------------------------------------------------------ 2. §0 y §1
row0 = line_starting('| **PB-T4d** | `F4B-PBT4-RETIRE-PILOT` (21, propuesto) |')
R(row0 + '\n', '| **PB-T4d** | `F4B-PBT4-RETIRE-PILOT` (21) | Retira el código del piloto que el panel ya no usa, con el alcance B y la rama muerta de `BuildCore`; el RED lleva todo lo de `tests/` (D-S11) | **229** filas con acción de `pbt4d-retire-list-229.tsv` (borrar, reescribir, sustituir y acotar con registro), con las 27 sustitutas de la corrida (§4.4) | Condicional (`owner-pbt4d-list-decisions.md`, pregunta 4), con esa lista, sobre el cierre de PB-T4c (`6c2820ba`) |\n', what='§0 la fila de PB-T4d')
row1 = line_starting('| **Las decisiones de diseño de la sesión para PB-T4c** (')
R(row1 + '\n', row1 + '\n' + '\n'.join([
    '| **Las decisiones del Owner sobre la lista de PB-T4d** (`owner-pbt4d-list-decisions.md`, 2026-11-22, la 10.ª tanda) | «Alcance B», «Tarea propia después» (la recarga), «Acotarla en PB-T4d» (la prueba lenta) y «Sí, con condiciones» (§8.5) |',
    '| **La decisión del Owner sobre las sustitutas** (`owner-pbt4d-substitutes-decision.md`, 2026-11-22, la 11.ª) | «Sustitutas donde hace falta» (§8.5) |',
    '| **La decisión del Owner sobre la corrida genérica** (`owner-pbt4d-run-coverage-decision.md`, 2026-11-22, la 12.ª) | «Sustituir las 21» (§8.5) |',
    '| **Las dos decisiones del Owner sobre la rama muerta y las 27** (`owner-pbt4d-deadcode-27-decisions.md`, 2026-11-22, la 13.ª) | «Quitarla en PB-T4d» y «Sí, 27» (§8.5) |',
    '| **Las decisiones de diseño de la sesión para PB-T4d** (`session-pbt4d-design-decisions.md`, 2026-11-22) | D-S11 a D-S15 (§8.5) |']) + '\n', what='§1 las autoridades de PB-T4d')

# ------------------------------------------------------------------ 3. D-PBT4-10, §4.4, §5.4 y §5.6
a = t.index('**Lo que retira PB-T4d** (lo que el panel ya no usa):')
b = t.index('#### D-PBT4-11.', a)
t = t[:a] + frag('frag/d10.md') + t[b:]
log.append('reemplazo D-PBT4-10, lo que retira PB-T4d')
R('\n## 5. La red de pruebas\n', '\n' + fill(frag('frag/s44.md')) + '## 5. La red de pruebas\n', what='§4.4')
R('`ui-tests`: en failure en PB-T4b y PB-T4c; en success en PB-T4a, que no añade pruebas de UI.',
  '`ui-tests`: en failure en PB-T4b y PB-T4c; en success en PB-T4a, que no añade pruebas de UI, y en PB-T4d, que no añade ninguna CT de UI y cuyas bajas de UI no caen.', what='§5.4')
R('\n## 6. Los contratos propuestos\n', '\n' + frag('frag_56.md') + '\n## 6. Los contratos propuestos\n', what='§5.6')

# ------------------------------------------------------------------ 4. §6.0
R('- **PB-T4d:** los archivos de `Pilot/` que retira (§6.4), con su contrato.',
  '- **PB-T4d:** los seis archivos de `Pilot/` del alcance (§6.4): `PushBackPilotMaterializer.cs`, `PushBackPilotUpdatePlan.cs`, `PushBackPilotUpdateRun.cs` y `PushBackPilotDraftState.cs` (solo comentarios, D-S15), que cambian, y `PushBackPilotStateComparer.cs` y `PushBackPilotFieldAdapter.cs`, que se borran.', what='§6.0 UNIT_CHANGE')
R('y una reparación tampoco toca el kit ni las 21 copias, cuyo texto es el del paquete.',
  'y una reparación tampoco toca el kit ni las 21 copias, cuyo texto es el del paquete. **En PB-T4d,** el RED edita 26 archivos existentes de `tests/` y añade cinco: una reparación solo toca las dos clases de las CT (`PushBackRetirePilotGuardTests.cs` y `PushBackRetirePilotCensusGuardTests.cs`), nunca los 26 existentes ni las tres clases nuevas que no son CT, cuyo texto es el del paquete.', what='§6.0 RR-2')
R('`PushBackWorkspaceApplyCloseTests.cs`). **Los equivalentes no admiten reparación** (RR-2)',
  '`PushBackWorkspaceApplyCloseTests.cs`); en PB-T4d, ningún archivo de `src/`, ni los 26 archivos existentes de `tests/` que el RED edita, ni las tres clases nuevas que no son CT (`PushBackRetirePilotRunTests`, `PushBackRetirePilotFullCopyGuardTests` y `PushBackRetirePilotPlanGuardTests`). **Los equivalentes no admiten reparación** (RR-2)', what='§6.0 el Controller')
rr3 = line_starting('| PB-T4c | `PushBackEditAllWindowTests` (UI; CT-02, 06, 09 y 14) |')
R(rr3 + '\n', rr3 + '\n' + '| PB-T4d | `PushBackRetirePilotGuardTests` (CT-01) | **Líneas:** las 35 de CT-01 en §5.6, literales, y las expresiones e inyecciones de su tabla `Retired` |\n'
  '| PB-T4d | `PushBackRetirePilotCensusGuardTests` (CT-02) | **Líneas:** las 11 de CT-02 en §5.6, literales, y las expresiones e inyecciones de su tabla `RetiredUses` |\n', what='§6.0 RR-3')
p_rows = line_starting('**Las filas de PB-T4c** (V50-06) son una por clase de las CT')
R(p_rows + '\n', p_rows + '\n\n**Las filas de PB-T4d** (V52-07) son una por clase de las CT, como las de PB-T4c. Las tres clases nuevas que no son CT no tienen fila: no admiten reparación (RR-2).\n', what='§6.0 las filas de PB-T4d')
R('o en `failure` (PB-T4b y PB-T4c; en PB-T4c, con las 4 CT de UI y el equivalente de CT-PBT2B-06), sin cuelgue.',
  'o en `failure` (PB-T4b y PB-T4c; en PB-T4c, con las 4 CT de UI y el equivalente de CT-PBT2B-06), y en `success` en PB-T4d (sin CT de UI; sus bajas de UI no caen), sin cuelgue.', what='§6.0 la CI del RED')

# ------------------------------------------------------------------ 5. §6.4
a = t.index('### 6.4 PB-T4d — la retirada del piloto (esbozo; V42-01)')
b = t.index('## 7. Riesgos y lecciones aplicadas', a)
s64 = fill(frag('frag/s64.md'))
lit = s64[s64.index('```text\nTaskId: F4B-PBT4-RETIRE-PILOT'):]
lit = lit[:lit.index('\n```\n') + 5]
assert all(ord(ch) < 0x80 for ch in lit), [ch for ch in lit if ord(ch) >= 0x80][:5]
t = t[:a] + s64 + '\n' + t[b:]
log.append('reemplazo §6.4 entera')

# ------------------------------------------------------------------ 6. §7, §8.3, §8.5, §9 y §11
r29 = line_starting('| **R-T4-29, las CT no corren en la precondición 6**')
R(r29 + '\n', r29 + '\n' + '\n'.join([
    '| **R-T4-30, la lista por texto no es la necesaria** (V52-02) | Se mide desde cero bajas, compilando (`/d/r6h`): 34 de las 235 bajas de B no hacían falta, entre ellas 7 CT de PB-T3b |',
    '| **R-T4-31, compilar no basta** | Un nombre que se queda sin su campo se resuelve como otro (`Comparer.Equals` como `System.Collections.Comparer`): la prueba compila y cae, o pasa en falso. Se corre la suite completa |',
    '| **R-T4-32, «lo cubren otras» se mide, rama por rama** (V52-03) | Con coverlet: primero por líneas (`/d/r6j`, `/d/r6k`), y el relevo vio que el método prueba a prueba contaba ramas, no cuáles; la precondición 6 compara la base y el GREEN rama por rama, en JSON |',
    '| **R-T4-33, código muerto tras la retirada** (V52-04) | Una rama que solo alcanzaba lo retirado no la puede cubrir ninguna sustituta: el GREEN la quita (`allowErase`), por decisión del Owner |']) + '\n', what='§7')
for prefix in ('| PB-T4d | C-7: 71 por texto', '| PB-T4d | Las originales de las 9 clases', '| PB-T4d | Los miembros de apoyo'):
    l = line_starting(prefix)
    R(l + '\n', '', what='§8.3 fila vieja ' + prefix[10:30])
r83 = line_starting('| PB-T4c | PEG:155-163, CT-PBT1B-15 y CT-PBT4B-20')
R(r83 + '\n', r83 + '\n' + '| PB-T4d | La lista del alcance B fila a fila (C-7, originales, apoyo, compilación, ejecución, M-04, la lenta y SUS-01), con las 27 sustitutas de la corrida y la rama muerta de `BuildCore` | El esbozo: 138 + 55 + 29; B medida: 235 bajas | **229** con acción (242 filas) | **La elegida** (`owner-pbt4d-list-decisions.md`, pregunta 1, con `owner-pbt4d-substitutes-decision.md`, `owner-pbt4d-run-coverage-decision.md` y `owner-pbt4d-deadcode-27-decisions.md`); `pbt4d-retire-list-229.tsv`, sha256 `%s` (el esbozo, `pbt4d-retire-list-138.tsv`, `d5467dc162e2cda8c539a829cc8f3a7c46260a119f70e840b34e6f78dde0387c`, queda como historia) |\n' % LIST_SHA, what='§8.3 la fila de PB-T4d')
r9 = line_starting('| **9.ª** (2026-11-21) | `owner-pbt4c-list-4-decision.md`')
R(r9 + '\n', r9 + '\n' + '\n'.join([
    '| **10.ª** (2026-11-22) | `owner-pbt4d-list-decisions.md` | «Lista T4d»: «Alcance B (Recomendado)». «Recarga»: «Tarea propia después (Recomendado)». «Prueba lenta»: «Acotarla en PB-T4d (Recomendado)». «Emisión T4d»: «Sí, con condiciones (Recomendado)» | PB-T4d retira el alcance B; la recarga de «Editar todo» va a una tarea propia; la prueba lenta se acota (D-S14); la emisión es condicional (lo que fija para PB-T4d) |',
    '| **11.ª** (2026-11-22) | `owner-pbt4d-substitutes-decision.md` | «Las 138»: «Sustitutas donde hace falta (Recomendado)» | Se quedan 7 CT de PB-T3b y sus 6 ayudantes; lo que también prueba código que se queda conserva esa parte; lo demás se borra con registro |',
    '| **12.ª** (2026-11-22) | `owner-pbt4d-run-coverage-decision.md` | «Corrida»: «Sustituir las 21 (Recomendado)» | Las 21 de WPUR, equivalentes sobre `ExecuteFull` en `PushBackRetirePilotRunTests`; la precondición 6 mide otra vez la cobertura |',
    '| **13.ª** (2026-11-22) | `owner-pbt4d-deadcode-27-decisions.md` | «Rama muerta»: «Quitarla en PB-T4d (Recomendado)». «27 en vez de 21»: «Sí, 27 (Recomendado)» | El GREEN quita `allowErase` y lo que solo existe con él; las sustitutas pasan a 27 (y la 06, 07, 16, 19, 29 y 42) y se borran 21; la cobertura, rama por rama |']) + '\n', what='§8.5 las tandas de PB-T4d')
ds = line_starting('- **D-S7 a D-S10** (`session-pbt4c-design-decisions.md`')
R(ds + '\n', ds + '\n' + '- **D-S11 a D-S15** (`session-pbt4d-design-decisions.md`, 2026-11-22; para PB-T4d): **D-S11**, el RED lleva todo lo de `tests/`, sin esqueleto, y dos CT (CT-PBT4D-01 con `ExpectRed` true y CT-PBT4D-02 con false); **D-S12**, las tres que caen al ejecutar, con `Skip` y equivalente, prefijo `PushBackRetirePilot` y filtros por clase exacta; **D-S13**, los vacíos de M-04, en su sitio; **D-S14**, la prueba lenta, cada 97.º toque y el último; **D-S15**, los cuatro `cref` y el comentario, y `PushBackPilotDraftState.cs` en `UNIT_CHANGE`.\n', what='§8.5 D-S11 a D-S15')
m9 = line_starting('- **Medidos en la copia exportada, para PB-T4c**')
R(m9 + '\n', m9 + '\n' + '- **Medidos en copias exportadas, para PB-T4d** (V52-06): la necesidad de cada baja (`/d/r6h`), la cobertura (`/d/r6j`, `/d/r6k`), dos ensayos (`/d/r6l`, `/d/r6n`) y la precondición 6 (`/d/r6r`, `/d/r6s` y `/d/r6t`); las demás, historia. **Lo que la cobertura no ve:** las líneas de `Pilot/` que la base no ejercitaba por WPUR (§4.4).\n', what='§9 PB-T4d')
o2 = line_starting('2. **Las respuestas del Owner:**')
R(o2 + '\n', o2 + ' **Para PB-T4d:** `owner-pbt4d-list-decisions.md`, `owner-pbt4d-substitutes-decision.md`, `owner-pbt4d-run-coverage-decision.md` y `owner-pbt4d-deadcode-27-decisions.md`, y `session-pbt4d-design-decisions.md` (D-S11 a D-S15), con su número F4B-E al custodiarlas.\n', what='§11.2')
c6 = line_starting('   - **PB-T4c** (los nueve pasos de §6.3.1;')
R(c6 + '\n', c6 + '\n' + ('   - **PB-T4d** (los nueve pasos de §6.4.1; `pbt4dcc/compile/compile-check.md`), sobre copias exportadas del cierre de PB-T4c (`6c2820ba`): el RED literal y el GREEN literal, sin ningún `warning CS`; **Core:** en el RED, %d = %d + %d + %d, y solo cae CT-PBT4D-01, por aserción; en el GREEN, %d = %d + %d + %d; los 18 filtros de UI, 0 `Failed`; **la cobertura de la corrida genérica que se queda, rama por rama, PASA**; el ensayo de las CT y su control, y el censo de C13 (PASA). Medido en la v5.2 (`pbt4d-precondition6.json`, `%s`). **Una prueba que falle fuera de la lista** vuelve al Owner.\n'
                     % (cr['Total'], cr['Passed'], cr['Failed'], cr['Skipped'], cg['Total'], cg['Passed'], cg['Failed'], cg['Skipped'], P6_SHA)), what='§11.6')
R('**PB-T4d no se emite por la autorización condicional:** vuelve al Owner con su lista.',
  '**PB-T4d, las 229 filas con acción de `pbt4d-retire-list-229.tsv`** (sha256 `%s`), la elegida (`owner-pbt4d-list-decisions.md`, pregunta 1, con las sustitutas y la rama muerta de sus decisiones siguientes), que el generador comprueba; se emite por su autorización condicional (pregunta 4).' % LIST_SHA, what='§11.7')
R('**Para PB-T4d:** el cierre de PB-T4c, su lista vuelta a medir y la respuesta del Owner.',
  '**Para PB-T4d:** el cierre de PB-T4c (CERRADA: F4B-E55, `6c2820ba`), su lista vuelta a medir y las respuestas del Owner (cumplidas); **en el commit de emisión de PB-T4d,** además: esta versión (v5.2), los fragmentos y los guiones de `v52/` (con `v52/sub/`, el alto del relevo), la lista (`pbt4d-retire-list-229.tsv`), las dos piezas y sus módulos y aplicadores, la remedición, las medidas de la necesidad y de la cobertura, la corrida de la precondición 6 (`pbt4dcc/compile/`, en `F4B-DRAFTS/pbt4/cc4d/`: `compile-check.md`, `pbt4d-precondition6.json`, sus guiones, sus registros y el sha256 de sus TRX y de sus coberturas, y como historia las copias anteriores), los cuatro registros del Owner de las tandas 10.ª a 13.ª, `session-pbt4d-design-decisions.md` y la entrada «## PB-T4d» de `superseding-characterization.md`.', what='§11.9')

# ------------------------------------------------------------------ 7. el anexo V52-A
green_txt = open(os.path.join(HERE, 'pbt4d-green-v52.txt'), encoding='utf-8').read()
annex = ['', '## Anexo V52-A. El texto literal de PB-T4d: el GREEN, las ediciones con registro, los `Skip` y los archivos nuevos (v5.2)', '',
         'Las bajas con registro no van aquí: son las filas «borrar con registro» y «sustituir con registro» de la lista, y su texto es el de la pieza del RED (`pbt4d-red-v52.txt`, sha256 `%s`), que las quita.' % V['RED_SHA'], '',
         '### La pieza del GREEN, `pbt4d-green-v52.txt` (sha256 `%s`)' % V['GREEN_SHA'], '', '```text'] + green_txt.rstrip('\n').split('\n') + ['```', '',
         '### Las ediciones con registro (`pbt4d_test_edits_v52.py`, `EDITS`, en su orden, sobre el texto ya sin las bajas)', '']
for path, frm, to, new, why in ed.EDITS:
    annex += ['**`%s`:** %s.' % (path, why), '', ('Desde la línea que contiene el texto de abajo hasta la anterior a la que contiene `%s`:' % to.strip()) if to else 'El texto, una vez en el archivo:', '', '```text'] + frm.rstrip('\n').split('\n') + ['```', '', 'Pasa a:', '', '```text'] + (new.rstrip('\n').split('\n') if new else ['(nada)']) + ['```', '']
old_call, new_call, times = ed.STALE_CALL
annex += ['**`tests/RackCad.Tests/Workspace/WorkspaceDraftPilotIntentStaleBaseTests.cs`:** las %d llamadas `%s` pasan a `%s`.' % (times, old_call, new_call), '',
          '### Los `Skip`', ''] + skip_lines + ['']
for p in RED_NEW:
    assert '```' not in red_r[p], p
    annex += ['### `%s` (`NewFileSha` `%s`)' % (p, NEWSHA[p]), '', '```csharp'] + red_r[p].rstrip('\n').split('\n') + ['```', '']
assert '```' not in green_txt and all('```' not in (e[1] + e[3]) for e in ed.EDITS)
t = t.rstrip('\n') + '\n' + '\n'.join(annex).rstrip('\n') + '\n'

# ------------------------------------------------------------------ las comprobaciones
assert t.count('## Anexo V52-A') == 1 and t.count('### 6.4 PB-T4d — `F4B-PBT4-RETIRE-PILOT`') == 1 and '(esbozo; V42-01)' not in t
assert t.count('| **V52-04** |') == 1 and t.count('### 4.4 PB-T4d') == 1 and t.count('### 5.6 PB-T4d') == 1
assert not re.search(r'@@[A-Za-z0-9_./-]+@@', t)
fences = [l for l in t.split('\n') if l.startswith('```')]
assert len(fences) % 2 == 0
data = t.encode('utf-8')
assert b'\r' not in data
with open(OUT, 'wb') as f:
    f.write(data)
out = os.path.join(V52, 'make_v52-out.txt')
log.append('v5.2 %s %d bytes; reemplazos %d' % (hashlib.sha256(data).hexdigest(), len(data), len([l for l in log if l.startswith('reemplazo')])))
with open(out, 'w', encoding='utf-8', newline='\n') as f:
    f.write('\n'.join(log) + '\n')
print(log[-1])
