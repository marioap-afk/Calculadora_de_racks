"""PB-T4d (paquete v5.4: la de la v5.3 mas, sin correr pruebas, M-08 a M-11 de la recomprobacion 1 de CA-T4d y la reaplicacion de las piezas de la v5.4), precondicion 6: el resumen `pbt4d-precondition6.json` y su registro `compile-check.md`, desde las salidas de las corridas
(nada se corre aqui): la principal (<p6>, sola y con los 18 filtros de UI del contrato; CA-T4d M-07), la base con cobertura (<bc>: la suite de
Core y la union de los filtros de UI, la referencia de la cobertura ampliada y la base de los MinSelected), el control de las CT (<ct>), y las
lecturas: la cobertura ampliada (cov-p6-v54.txt, CA-T4d I-02), el censo de C13 (census-c13-v53.txt), los RequiredTests, los totales de UI y la
lista. compile-check.md lleva el resultado de cada clase que corre (Passed, Failed y Skipped) en cada rama y declara lo de la v5.2 (M-07). Comprueba
lo previsto y lo dice; no corrige nada. Escribe UTF-8 con LF.

Uso: python -I p6_json_pbt4d_v54.py <dir-compile> <p6> <bc> <ct> <lista.tsv> <reaplicacion-v54>
"""
import collections
import hashlib
import json
import os
import re
import sys
import xml.etree.ElementTree as ET

sys.dont_write_bytecode = True
CC, P6, BC, CT, LIST, RA = sys.argv[1:7]
PB4 = os.path.normpath(os.path.join(CC, '..', '..', 'pbt4'))
NS = {'t': 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}
REV = '6c2820bab4d8b41e3e229e23f3a1638ccbb04735'
SLOW = 'InvalidD_ABuilderThatCannotBuildTheOutgoingSystemIsInvalidWithTheTypedErrorAndNothingIsWritten'
CT01 = 'RackCad.Tests.PushBackRetirePilotGuardTests.CT_PBT4D_01_TheCodeOfThePilotThatThePanelNoLongerUsesIsRetiredAndWhatTheFullModelUsesStays'


def sha(path):
    return hashlib.sha256(open(path, 'rb').read()).hexdigest()


def text(path):
    return open(path, encoding='utf-8').read()


def driver(name):
    return text(os.path.join(CC, 'driver-%s.txt' % name))


def trx_stats(path):
    root = ET.parse(path).getroot()
    defs = {ut.get('id'): (ut.find('t:TestMethod', NS).get('className'), ut.find('t:TestMethod', NS).get('name'))
            for ut in root.iterfind('.//t:TestDefinitions/t:UnitTest', NS)}
    c = collections.Counter()
    failed, slow = [], None
    for r in root.iterfind('.//t:Results/t:UnitTestResult', NS):
        c[r.get('outcome')] += 1
        cls, name = defs[r.get('testId')]
        if r.get('outcome') == 'Failed':
            failed.append(cls + '.' + name)
        if name == SLOW and r.get('outcome') == 'Passed':
            h, m, s = r.get('duration').split(':')
            slow = round(int(h) * 3600 + int(m) * 60 + float(s), 1)
    return {'Total': sum(c.values()), 'Passed': c['Passed'], 'Failed': c['Failed'], 'Skipped': c['NotExecuted'], 'FailedTests': sorted(failed),
            'SlowTestSeconds': slow, 'Trx': sha(path)}


d6 = driver(P6)
out = {'Schema': 'rackcad-pbt4d-precondition6/v1', 'Package': 'v5.4', 'Task': 'F4B-PBT4-RETIRE-PILOT', 'Revision': REV}
out['Pieces'] = dict(sorted((m.group(2), m.group(1)) for m in re.finditer(r'^  pieza ([0-9a-f]{64}) (\S+)$', d6, re.M)))
for name in (LIST, 'pbt4d-red-v54.txt', 'pbt4d-green-v54.txt', 'pbt4d_test_edits_v54.py', 'v54/make_red_v54.py', 'v54/make_list_v54.py', 'v54/codeonly_v54.py', 'pbt4d_test_edits_v53.py', 'v53/make_red_v53.py', 'v53/make_green_v53.py', 'v53/make_list_v53.py', 'v53/make_run_eq_v53.py',
             'v53/unused_v53.py', 'v52/make_fullcopy_eq_v52.py', 'v52/retire_pbt4d_v52.py', 'v53/run/PushBackRetirePilotRunTests.cs',
             'v52/ct/PushBackRetirePilotGuardTests.cs', 'v52/ct/PushBackRetirePilotCensusGuardTests.cs', 'v52/try/PushBackRetirePilotPlanGuardTests.cs'):
    out['Pieces'][name] = sha(os.path.join(PB4, name))
lst = [l.split('\t') for l in text(os.path.join(PB4, LIST)).rstrip('\n').split('\n')[1:]]
out['List'] = {'File': LIST, 'Sha256': sha(os.path.join(PB4, LIST)), 'Rows': len(lst), 'WithAction': sum(1 for r in lst if r[6] != 'se queda'),
               'ByAction': dict(sorted(collections.Counter(r[6] for r in lst).items())),
               'ByGroupAndAction': {'%s | %s' % k: v for k, v in sorted(collections.Counter((r[1], r[6]) for r in lst).items())}}
D6 = os.path.join(CC, P6)
apply_red = text(os.path.join(D6, 'apply-red.txt'))
out['RedFiles'] = {m.group(3): {'kind': m.group(1), 'sha256': m.group(2)} for m in re.finditer(r'^(red|nuevo) ([0-9a-f]{64}) (\S+)$', apply_red, re.M)}
out['RedDeleted'] = re.findall(r'^borrado (\S+)', apply_red, re.M)
apply_green = text(os.path.join(D6, 'apply-green.txt'))
out['GreenFiles'] = {m.group(3): {'kind': m.group(1), 'sha256': m.group(2)} for m in re.finditer(r'^(\w+) ([0-9a-f]{64}) (\S+)$', apply_green, re.M)}
out['GreenDeleted'] = re.findall(r'^borrado (\S+)', apply_green, re.M)
builds = {m.group(1): {'rc': int(m.group(2)), 'errors': int(m.group(3)), 'csWarnings': int(m.group(4))}
          for m in re.finditer(r'^(r-core|r-ui|g-core|g-ui|g-plugin) rc=(\d+) errores=(\d+) avisos-CS=(\d+)', d6, re.M)}
out['Builds'] = builds
out['CsWarnings'] = sum(b['csWarnings'] for b in builds.values())
m0 = re.search(r'^procesos al empezar: (\d+) \(([\d:]+)\)', d6, re.M)
m1 = re.search(r'^procesos al acabar: (\d+) \(([\d:]+)\)', d6, re.M)
out['Alone'] = {'ProcessesAtStart': int(m0.group(1)), 'Start': m0.group(2), 'ProcessesAtEnd': int(m1.group(1)), 'End': m1.group(2)}
secs = {m.group(1): int(m.group(2)) for m in re.finditer(r'^core (core-\S+) rc=\d+ segundos=(\d+)', d6, re.M)}
out['Red'] = {'Core': trx_stats(os.path.join(D6, 'core-red.trx'))}
out['Green'] = {'Core': trx_stats(os.path.join(D6, 'core-green.trx'))}
out['GreenWithCoverage'] = {'Core': trx_stats(os.path.join(D6, 'core-green-cov.trx')), 'UI': trx_stats(os.path.join(D6, 'ui-green-cov.trx')),
                            'UIRepeats': [dict(trx_stats(os.path.join(D6, 'ui-green-cov-r-%d.trx' % i)), Json=sha(os.path.join(D6, 'ui-green-cov-r-%d.coverage.json' % i)))
                                          for i in (1, 2)]}
filters = [l for l in text(os.path.join(CC, 'ui_filters_pbt4d_v53.txt')).split('\n') if l]
assert len(filters) == 18
for branch in ('red', 'green'):
    ui = []
    for n, f in enumerate(filters, 1):
        p = os.path.join(D6, '%s-ui%02d.trx' % (branch, n))
        ui.append(dict({'n': n, 'Filter': f}, **(trx_stats(p) if os.path.exists(p) else {'Total': 0})))
        j = os.path.join(D6, '%s-ui%02d.coverage.json' % (branch, n))
        if branch == 'green':  # recomprobacion 1 de CA-T4d, M-08: la cobertura de cada filtro del GREEN, por su sha256
            ui[-1]['Json'] = sha(j)
    out['Red' if branch == 'red' else 'Green']['UI'] = ui
# M-08: todas las corridas de UI del GREEN que entran en la union de D-S16, con el sha256 de su TRX y de su JSON
out['GreenUiCoverage'] = {
    'Union': [{'Run': r, 'Trx': sha(os.path.join(D6, r + '.trx')), 'Json': sha(os.path.join(D6, r + '.coverage.json'))}
              for r in ('ui-green-cov', 'ui-green-cov-r-1', 'ui-green-cov-r-2')],
    'ByFilter': [{'n': u['n'], 'Filter': u['Filter'], 'Trx': u['Trx'], 'Json': u['Json']} for u in out['Green']['UI']]}
out['SuiteDurationSeconds'] = {'Red': secs.get('core-red'), 'Green': secs.get('core-green'), 'GreenWithCoverage': secs.get('core-green-cov')}
out['SlowTestSeconds'] = {'Red': out['Red']['Core']['SlowTestSeconds'], 'Green': out['Green']['Core']['SlowTestSeconds']}
dbc = driver(BC)
DB = os.path.join(CC, BC)
out['BaseCoverage'] = {'Copy': '/d/' + BC.split('-')[0], 'Driver': sha(os.path.join(CC, 'driver-%s.txt' % BC)),
                       'Core': trx_stats(os.path.join(DB, 'base-core-cov.trx')), 'UI': trx_stats(os.path.join(DB, 'base-ui-cov.trx')),
                       'CoreJson': sha(os.path.join(DB, 'base-core-cov.coverage.json')), 'UiJson': sha(os.path.join(DB, 'base-ui-cov.coverage.json')),
                       'Builds': re.findall(r'^(b-\w+ rc=\d+ errores=\d+ avisos-CS=\d+)', dbc, re.M),
                       'Alone': re.findall(r'^(procesos al (?:empezar|acabar): \d+)', dbc, re.M),
                       'UIRepeats': [dict(trx_stats(os.path.join(DB, 'base-ui-cov-r-%d.trx' % i)), Json=sha(os.path.join(DB, 'base-ui-cov-r-%d.coverage.json' % i)))
                                     for i in (1, 2)],
                       'UIRepeatsDriver': sha(os.path.join(CC, 'driver-uirep-r6w-r6x.txt')),
                       'UIByFilter': {'Driver': sha(os.path.join(CC, 'driver-uif-r6w.txt')),
                                      # M-08: el sha256 COMPLETO de la TRX y del JSON de cada uno de los 19 filtros de la base
                                      'Runs': [{'Run': r, 'Filter': f, 'Outcomes': o, 'Trx': sha(os.path.join(DB, r + '.trx')),
                                                'Json': sha(os.path.join(DB, r + '.coverage.json'))}
                                               for r, f, o in re.findall(r'^  ui (base-uif-\d\d) rc=\d+ (\S+) :: filas \d+ (\{[^}]*\})',
                                                                         text(os.path.join(CC, 'driver-uif-r6w.txt')), re.M)]}}
assert len(out['BaseCoverage']['UIByFilter']['Runs']) == 19
# la reaplicacion de las piezas de la v5.4 (solo texto: RED con comentarios, GREEN con su cabecera), sin pruebas
dra = driver(RA)
out['Reapply'] = {'Copy': '/d/' + RA.split('-')[0], 'Driver': sha(os.path.join(CC, 'driver-%s.txt' % RA)),
                  'Pieces': dict(sorted((m.group(2), m.group(1)) for m in re.finditer(r'^  pieza ([0-9a-f]{64}) (\S+)$', dra, re.M))),
                  'Builds': {m.group(1): {'rc': int(m.group(2)), 'errors': int(m.group(3)), 'csWarnings': int(m.group(4))}
                             for m in re.finditer(r'^(r-core|r-ui|g-core|g-ui|g-plugin) rc=(\d+) errores=(\d+) avisos-CS=(\d+)', dra, re.M)},
                  'TestsCodeChanged': re.search(r'cambia el codigo \(sin comentarios ni lineas en blanco\): (.*)$', dra, re.M).group(1),
                  'TestsTextChanged': re.search(r'cambia el texto: (.*)$', dra, re.M).group(1),
                  'SrcDifferences': int(re.search(r'^src/ frente a la v5\.3: (\d+) diferencias', dra, re.M).group(1))}
out['Noise'] = {'Driver': sha(os.path.join(CC, 'driver-noise-batch.txt')), 'Script': sha(os.path.join(CC, 'noise_v53.sh')),
                'Lines': 'src/RackCad.UI/Views/RackViewBatchDialogPresenter.cs:37-38',
                'Runs': re.findall(r'^(r6[wx]) corrida \d+ .*\n  golpes .*37:(\d+) 38:(\d+)', text(os.path.join(CC, 'driver-noise-batch.txt')), re.M)}
cov = text(os.path.join(CC, 'cov-p6-v54.txt'))
out['Coverage'] = {'Output': sha(os.path.join(CC, 'cov-p6-v54.txt')), 'Script': sha(os.path.join(CC, 'cov_p6_v54.py')), 'Summary': cov.split('\n')[2],
                   'Verdict': cov.split('\n')[3].split()[-1], 'Watched': cov.split('\n')[4],
                   'GreenCoreJson': sha(os.path.join(D6, 'core-green-cov.coverage.json')), 'GreenUiJson': sha(os.path.join(D6, 'ui-green-cov.coverage.json'))}
bajan = cov[cov.index('## Bajan (informativo'):]
out['Coverage']['Bajan'] = {'Summary': [l for l in bajan.split('\n') if l.startswith('entradas ')][0],
                            'Perdida': [l.strip() for l in bajan.split('\n') if l.startswith('  PERDIDA ')],
                            'Varia': [l.strip() for l in bajan.split('\n') if l.startswith('  VARIA ')],
                            'Estable': int(re.search(r'ESTABLE (\d+)', [l for l in bajan.split('\n') if l.startswith('entradas ')][0]).group(1)),
                            'EstableGroups': sum(1 for l in bajan.split('\n') if l.startswith('  ESTABLE '))}
census = text(os.path.join(CC, 'census-c13-v53.txt'))
out['Census'] = {'Verdict': 'PASA' if '\ncenso PASA\n' in census else 'CAE', 'Output': sha(os.path.join(CC, 'census-c13-v53.txt')),
                 'Script': sha(os.path.join(CC, 'census_c13_v52.py'))}
dct = driver(CT)
tests_ct = re.findall(r'^test (\S+) trx=(\w+) :: filas (\d+) (\{[^}]*\})', dct, re.M)
out['Control'] = {t[0]: {'Trx': t[1], 'Results': int(t[2]), 'Outcomes': t[3]} for t in tests_ct}


def outcome(stats, cls):
    return 'Failed' if any(f.startswith(cls + '.') for f in stats['FailedTests']) else 'Passed'


out['Rehearsal'] = {b: {'CT-PBT4D-01': outcome(out[b]['Core'], 'RackCad.Tests.PushBackRetirePilotGuardTests'),
                        'CT-PBT4D-02': outcome(out[b]['Core'], 'RackCad.Tests.PushBackRetirePilotCensusGuardTests')} for b in ('Red', 'Green')}
out['ClassResults'] = {b: sha(os.path.join(D6, 'clases-%s.txt' % b)) for b in ('red', 'green')}
req = text(os.path.join(CC, 'required-pbt4d.txt'))
out['RequiredTests'] = {'Tsv': sha(os.path.join(CC, 'required-pbt4d.tsv')), 'Summary': [l for l in req.split('\n') if l.startswith('heredados')][0],
                        'Overlap': [l for l in req.split('\n') if l.startswith('solapamientos')][0]}
out['UiTotals'] = {b: [l for l in text(os.path.join(CC, 'ui-totals-%s.txt' % b)).split('\n') if l.startswith('UI ')][0] for b in ('red', 'green')}
out['History'] = {
    '/d/r6a a /d/r6q': 'v5.2: medidas de la lista, de la necesidad y de la cobertura, y ensayos (pbt4d-precondition6-v52.json, History)',
    '/d/r6r': 'v5.2: la copia principal; no corrio sola (su cobertura se solapo con r6s, r6t y r6u) y sus 18 filtros de UI eran los de PB-T4c sin PushBackEditAllWindowTests (CA-T4d M-07)',
    '/d/r6s': 'v5.2: la base con cobertura de Core (Application)', '/d/r6t': 'v5.2: el control de las CT', '/d/r6u': 'v5.2: los 19 filtros de UI en la base',
    '/d/r6v': 'v5.3: ensayo 1 (IDE0051/IDE0052 solo en la copia): 17 privados sin uso tras la retirada (CA-T4d M-03)',
    '/d/r6x': 'v5.3: ensayo 2: la segunda vuelta de M-03 y la cobertura ampliada del borrador del RED'}
expected = {
    'red-only-ct01': out['Red']['Core']['FailedTests'] == [CT01],
    'green-0-failed': out['Green']['Core']['Failed'] == 0 and out['GreenWithCoverage']['Core']['Failed'] == 0 and out['GreenWithCoverage']['UI']['Failed'] == 0
                      and all(r['Failed'] == 0 for r in out['GreenWithCoverage']['UIRepeats'] + out['BaseCoverage']['UIRepeats']),
    'ui-0-failed': all(u.get('Failed', 0) == 0 for b in ('Red', 'Green') for u in out[b]['UI']) and all(u.get('Total', 0) > 0 for b in ('Red', 'Green') for u in out[b]['UI']),
    'cs-0': out['CsWarnings'] == 0 and all(b['errors'] == 0 for b in builds.values()) and len(builds) == 5,
    'same-totals': out['Red']['Core']['Total'] == out['Green']['Core']['Total'],
    'alone': out['Alone']['ProcessesAtStart'] == 0,
    'coverage': out['Coverage']['Verdict'] == 'PASA', 'census': out['Census']['Verdict'] == 'PASA',
    'rehearsal': out['Rehearsal'] == {'Red': {'CT-PBT4D-01': 'Failed', 'CT-PBT4D-02': 'Passed'}, 'Green': {'CT-PBT4D-01': 'Passed', 'CT-PBT4D-02': 'Passed'}},
    'control': out['Control'].get('base-cts', {}).get('Outcomes') == "{'Failed': 2}" and out['Control'].get('red-A', {}).get('Outcomes') == "{'Failed': 1, 'Passed': 1}",
    'no-overlap': out['RequiredTests']['Overlap'] == 'solapamientos: 0 []',
    'base-is-ci-baseline': (out['BaseCoverage']['Core']['Total'], out['BaseCoverage']['Core']['Passed'], out['BaseCoverage']['Core']['Failed'],
                            out['BaseCoverage']['Core']['Skipped']) == (19619, 19158, 0, 461) and out['BaseCoverage']['UI']['Failed'] == 0,
    'base-alone': out['BaseCoverage']['Alone'][:1] == ['procesos al empezar: 0'],
    'red-deleted-3': len(out['RedDeleted']) == 3,
    'bajan-sin-perdida': not out['Coverage']['Bajan']['Perdida'],
    'reapply-cs-0': len(out['Reapply']['Builds']) == 5 and all(b['errors'] == 0 and b['csWarnings'] == 0 for b in out['Reapply']['Builds'].values()),
    'reapply-code-equal': out['Reapply']['TestsCodeChanged'] == '[]' and out['Reapply']['SrcDifferences'] == 0,
}
out['Expected'] = expected
out['AllAsExpected'] = all(expected.values())
data = json.dumps(out, ensure_ascii=False, indent=2, sort_keys=False) + '\n'
with open(os.path.join(CC, 'pbt4d-precondition6.json'), 'w', encoding='utf-8', newline='\n') as f:
    f.write(data)

# compile-check.md
gb = out['BaseCoverage']
md = ['# PB-T4d, precondicion 6 (paquete v5.4): el registro', '',
      'Copias exportadas de `%s`, fuera de todo worktree; nada en el checkout principal salvo `git archive`. El resumen, `pbt4d-precondition6.json` '
      '(sha256 `%s`). Todo lo previsto: **%s**.' % (REV, hashlib.sha256(data.encode('utf-8')).hexdigest(), 'SI' if out['AllAsExpected'] else 'NO'), '',
      '## Las corridas', '',
      '| Copia | Que | Registro |', '|---|---|---|',
      '| `/d/%s` | **La principal, sola** (procesos al empezar: %d, a las %s; al acabar: %d, a las %s): el RED literal y el GREEN literal, la suite completa de Core en las dos ramas, '
      'los 18 filtros de UI del contrato de PB-T4d (`ui_filters_pbt4d_v53.txt`, los de `RequiredTests`), el Plugin, y la cobertura del GREEN (Core completa y la union de los filtros de UI, JSON) | `driver-%s.txt` (`%s`) |' % (
          P6.split('-')[0], out['Alone']['ProcessesAtStart'], out['Alone']['Start'], out['Alone']['ProcessesAtEnd'], out['Alone']['End'], P6, sha(os.path.join(CC, 'driver-%s.txt' % P6))),
      '| `/d/%s` | La base sin cambios, sola: la suite completa de Core y la union de los filtros de UI de PB-T4c y de PB-T4d, con cobertura (JSON de Domain, Application y UI): la referencia de la cobertura ampliada y la base de los MinSelected | `driver-%s.txt` (`%s`) |' % (
          BC.split('-')[0], BC, gb['Driver']),
      '| `/d/%s` | El control del ensayo de las CT: la base con solo las dos CT, y el RED con la retirada A | `driver-%s.txt` (`%s`) |' % (CT.split('-')[0], CT, sha(os.path.join(CC, 'driver-%s.txt' % CT))),
      '', '**Historia** (no valen para la emision): ' + '; '.join('`%s`, %s' % kv for kv in out['History'].items()) + '.', '',
      '**Lo de la v5.2, declarado (CA-T4d M-07):** la copia principal de la v5.2, `/d/r6r`, no corrio sola: el paso de cobertura de su GREEN (de las 04:04 a las 04:14) se solapo '
      'con `/d/r6s` (04:01 a 04:28), `/d/r6t` y `/d/r6u`, y su registro acaba con `procesos: 4`; sus resultados no cambian, pero sus duraciones (593 s y 8,1 s) son orientativas. '
      'Y sus 18 filtros de UI eran los de PB-T4c sin `PushBackEditAllWindowTests` (su n.o 11, `PushBackFieldValidationConcordanceTests`, con 0 resultados); '
      '`PushBackEditAllWindowTests` corrio en `/d/r6u`, en las dos ramas. En la v5.3, la copia principal corre sola y con los 18 filtros del contrato.', '',
      '## Los resultados', '',
      '- **Compilacion:** ' + ', '.join('%s rc=%d errores=%d avisos CS=%d' % (k, v['rc'], v['errors'], v['csWarnings']) for k, v in builds.items()) + '.',
      '- **Core, RED:** %d = %d + %d + %d (Passed + Failed + Skipped), %s s; cae: %s.' % (
          out['Red']['Core']['Total'], out['Red']['Core']['Passed'], out['Red']['Core']['Failed'], out['Red']['Core']['Skipped'], secs.get('core-red'), ', '.join(out['Red']['Core']['FailedTests']) or 'ninguna'),
      '- **Core, GREEN:** %d = %d + %d + %d, %s s; con cobertura, %s s y %d Failed.' % (
          out['Green']['Core']['Total'], out['Green']['Core']['Passed'], out['Green']['Core']['Failed'], out['Green']['Core']['Skipped'], secs.get('core-green'), secs.get('core-green-cov'), out['GreenWithCoverage']['Core']['Failed']),
      '- **La prueba lenta acotada** (`%s`): %s s en el RED y %s s en el GREEN.' % (SLOW, out['SlowTestSeconds']['Red'], out['SlowTestSeconds']['Green']),
      '- **UI, los 18 filtros:** ' + '; '.join('n.o %d %d/%d/%d' % (u['n'], u.get('Passed', 0), u.get('Failed', 0), u.get('Skipped', 0)) for u in out['Green']['UI']) + ' (GREEN; Passed/Failed/Skipped). En el RED: '
      + '; '.join('n.o %d %d/%d/%d' % (u['n'], u.get('Passed', 0), u.get('Failed', 0), u.get('Skipped', 0)) for u in out['Red']['UI']) + '.',
      '- **La union de los filtros de UI con cobertura:** base %d = %d + %d + %d; GREEN %d = %d + %d + %d.' % (
          gb['UI']['Total'], gb['UI']['Passed'], gb['UI']['Failed'], gb['UI']['Skipped'], out['GreenWithCoverage']['UI']['Total'], out['GreenWithCoverage']['UI']['Passed'],
          out['GreenWithCoverage']['UI']['Failed'], out['GreenWithCoverage']['UI']['Skipped']),
      '- **La base de Core:** %d = %d + %d + %d (la de la CI del cierre de PB-T4c: 19 619 = 19 158 + 0 + 461).' % (gb['Core']['Total'], gb['Core']['Passed'], gb['Core']['Failed'], gb['Core']['Skipped']),
      '- **Los totales de UI de la CI:** %s; %s.' % (out['UiTotals']['red'], out['UiTotals']['green']),
      '- **El ensayo de las CT de texto:** %s. **Su control:** %s.' % (json.dumps(out['Rehearsal'], ensure_ascii=False), json.dumps(out['Control'], ensure_ascii=False)),
      '- **El censo de C13:** %s (`census-c13-v53.txt`, `%s`).' % (out['Census']['Verdict'], out['Census']['Output']),
      '- **La cobertura ampliada de todo src/ que se queda** (`cov-p6-v54.txt`, `%s`): %s; **%s**; %s.' % (out['Coverage']['Output'], out['Coverage']['Summary'], out['Coverage']['Verdict'], out['Coverage']['Watched']),
      '- **Bajan (informativo, D-S16):** %s. **PERDIDA:** %s. **VARIA, declaradas** (una prueba que no cambia y una cobertura que varia entre corridas; '
      'no son caminos perdidos): %s. Las ESTABLE (%d) las sigue cubriendo el GREEN con Core, o con las tres corridas de todos los filtros juntos y al menos un filtro en su proceso (M-11 b); cada entrada nombra la corrida, no la prueba (M-11 a); el detalle, en `cov-p6-v54.txt`.' % (
          out['Coverage']['Bajan']['Summary'], '; '.join(out['Coverage']['Bajan']['Perdida']) or 'ninguna',
          '; '.join(out['Coverage']['Bajan']['Varia']) or 'ninguna', out['Coverage']['Bajan']['Estable']),
      '- **El ruido de la UI que motivo D-S16** (`%s`, dos getters que lee un binding de WPF desde CT-14 de `PushBackClassicCharacterizationOwnerTests`, que no cambia): '
      'con su filtro solo, en tres corridas (`driver-noise-batch.txt`): %s. En esta precondicion no baja (la unión de corridas lo cubre en los dos lados); se declara igual: '
      'no es un camino perdido.' % (out['Noise']['Lines'], '; '.join('%s 37:%s 38:%s' % r for r in out['Noise']['Runs'])),
      '- **Los RequiredTests** (`required-pbt4d.tsv`, `%s`): %s; %s.' % (out['RequiredTests']['Tsv'], out['RequiredTests']['Summary'], out['RequiredTests']['Overlap']),
      '- **La v5.4, solo de texto** (recomprobacion 1 de CA-T4d, M-08 a M-13): las piezas de la v5.4 (`pbt4d-red-v54.txt`, `%s`; `pbt4d-green-v54.txt`, `%s`) '
      'reaplicadas en `%s` (`driver-%s.txt`, `%s`): %s; el texto de tests/ cambia en %s y el codigo sin comentarios en %s; src/ frente a la v5.3: %d diferencias. '
      'No se repite ninguna corrida de pruebas: el JSON fija ahora el sha256 de los 3 + 18 JSON de UI del GREEN (`GreenUiCoverage`, `Green.UI[n].Json`) y el '
      'completo de los 19 por filtro de la base (`BaseCoverage.UIByFilter.Runs`; M-08), y la cobertura se lee con `cov_p6_v54.py` (M-11 b).' % (
          out['Pieces']['pbt4d-red-v54.txt'], out['Pieces']['pbt4d-green-v54.txt'], out['Reapply']['Copy'], RA, out['Reapply']['Driver'],
          ', '.join('%s rc=%d errores=%d avisos CS=%d' % (k, v['rc'], v['errors'], v['csWarnings']) for k, v in out['Reapply']['Builds'].items()),
          out['Reapply']['TestsTextChanged'], out['Reapply']['TestsCodeChanged'], out['Reapply']['SrcDifferences']),
      '- **La lista** (`%s`, `%s`): %d filas, %d con accion.' % (LIST, out['List']['Sha256'], out['List']['Rows'], out['List']['WithAction']),
      '- **Lo previsto:** ' + ', '.join('%s=%s' % kv for kv in expected.items()) + '.', '']
for branch in ('red', 'green'):
    md += ['## El resultado de cada clase que corre, %s (Core y los 18 filtros de UI; `clases-%s.txt`, `%s`)' % (branch.upper(), branch, out['ClassResults'][branch]), '', '```text']
    md += text(os.path.join(D6, 'clases-%s.txt' % branch)).rstrip('\n').split('\n') + ['```', '']
with open(os.path.join(CC, 'compile-check.md'), 'w', encoding='utf-8', newline='\n') as f:
    f.write('\n'.join(md))
print('json', hashlib.sha256(data.encode('utf-8')).hexdigest(), 'todo previsto', out['AllAsExpected'], expected)
