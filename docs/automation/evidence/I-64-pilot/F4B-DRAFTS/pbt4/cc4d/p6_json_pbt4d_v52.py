"""PB-T4d (paquete v5.2), precondicion 6: el resumen `pbt4d-precondition6.json` y su registro `compile-check.md`, desde las salidas de las corridas
(nada se corre aqui): la principal (<p6>), la cobertura de la base (<bc>), el control de las CT (<ct>), y las lecturas: la cobertura rama por rama
(cov_p6_v52.py), el censo de C13, los RequiredTests, los totales de UI y la lista. compile-check.md lleva el resultado de cada clase que corre
(Passed, Failed y Skipped) en cada rama. Comprueba lo previsto y lo dice; no corrige nada. Escribe UTF-8 con LF.

Uso: python -I p6_json_pbt4d_v52.py <dir-compile> <p6> <bc> <ct>
"""
import collections
import hashlib
import json
import os
import re
import sys
import xml.etree.ElementTree as ET

sys.dont_write_bytecode = True
CC, P6, BC, CT = sys.argv[1:5]
PB4 = os.path.normpath(os.path.join(CC, '..', '..', 'pbt4'))
NS = {'t': 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}
REV = '6c2820bab4d8b41e3e229e23f3a1638ccbb04735'
SLOW = 'InvalidD_ABuilderThatCannotBuildTheOutgoingSystemIsInvalidWithTheTypedErrorAndNothingIsWritten'


def sha(path):
    data = open(path, 'rb').read()
    return hashlib.sha256(data).hexdigest()


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
out = {'Schema': 'rackcad-pbt4d-precondition6/v1', 'Package': 'v5.2', 'Task': 'F4B-PBT4-RETIRE-PILOT', 'Revision': REV}
out['Pieces'] = dict(sorted((m.group(2), m.group(1)) for m in re.finditer(r'^  pieza ([0-9a-f]{64}) (\S+)$', d6, re.M)))
for name in ('pbt4d-retire-list-229.tsv', 'pbt4d_test_edits_v52.py', 'v52/make_red_v52.py', 'v52/make_green_v52.py', 'v52/make_list_v52.py',
             'v52/make_run_eq_v52.py', 'v52/make_fullcopy_eq_v52.py', 'v52/retire_pbt4d_v52.py', 'v52/run/PushBackRetirePilotRunTests.cs',
             'v52/ct/PushBackRetirePilotGuardTests.cs', 'v52/ct/PushBackRetirePilotCensusGuardTests.cs', 'v52/try/PushBackRetirePilotPlanGuardTests.cs'):
    out['Pieces'][name] = sha(os.path.join(PB4, name))
lst = [l.split('\t') for l in text(os.path.join(PB4, 'pbt4d-retire-list-229.tsv')).rstrip('\n').split('\n')[1:]]
out['List'] = {'File': 'pbt4d-retire-list-229.tsv', 'Sha256': sha(os.path.join(PB4, 'pbt4d-retire-list-229.tsv')), 'Rows': len(lst),
               'WithAction': sum(1 for r in lst if r[6] != 'se queda'),
               'ByAction': dict(sorted(collections.Counter(r[6] for r in lst).items()))}
D6 = os.path.join(CC, P6)
apply_red = text(os.path.join(D6, 'apply-red.txt'))
out['RedFiles'] = {m.group(3): {'kind': m.group(1), 'sha256': m.group(2)} for m in re.finditer(r'^(red|nuevo) ([0-9a-f]{64}) (\S+)$', apply_red, re.M)}
apply_green = text(os.path.join(D6, 'apply-green.txt'))
out['GreenFiles'] = {m.group(3): {'kind': m.group(1), 'sha256': m.group(2)} for m in re.finditer(r'^(\w+) ([0-9a-f]{64}) (\S+)$', apply_green, re.M)}
out['GreenDeleted'] = re.findall(r'^borrado (\S+)', apply_green, re.M)
builds = {m.group(1): {'rc': int(m.group(2)), 'errors': int(m.group(3)), 'csWarnings': int(m.group(4))}
          for m in re.finditer(r'^(r-core|r-ui|g-core|g-ui|g-plugin) rc=(\d+) errores=(\d+) avisos-CS=(\d+)', d6, re.M)}
out['Builds'] = builds
out['CsWarnings'] = sum(b['csWarnings'] for b in builds.values())
secs = {m.group(1): int(m.group(2)) for m in re.finditer(r'^core (core-\S+) rc=\d+ segundos=(\d+)', d6, re.M)}
out['Red'] = {'Core': trx_stats(os.path.join(D6, 'core-red.trx'))}
out['Green'] = {'Core': trx_stats(os.path.join(D6, 'core-green.trx'))}
out['GreenWithCoverage'] = {'Core': trx_stats(os.path.join(D6, 'core-green-cov.trx'))}
filters = [l for l in text(os.path.join(CC, '..', '..', 'pbt4ccc', 'compile', 'ui_filters_pbt4c_v50b.txt')).split('\n') if l]
for branch in ('red', 'green'):
    ui = []
    for n, f in enumerate(filters, 1):
        p = os.path.join(D6, '%s-ui%02d.trx' % (branch, n))
        ui.append(dict({'n': n, 'Filter': f}, **(trx_stats(p) if os.path.exists(p) else {'Total': 0})))
    out['Red' if branch == 'red' else 'Green']['UI'] = ui
out['SuiteDurationSeconds'] = {'Red': secs.get('core-red'), 'Green': secs.get('core-green'), 'GreenWithCoverage': secs.get('core-green-cov')}
out['SlowTestSeconds'] = {'Red': out['Red']['Core']['SlowTestSeconds'], 'Green': out['Green']['Core']['SlowTestSeconds']}
dbc = driver(BC)
m = re.search(r'^cov base rc=(\d+) segundos=(\d+) json=(\w+) cobertura=(\w+) :: filas (\d+) (\{[^}]*\})', dbc, re.M)
out['BaseCoverage'] = {'Copy': '/d/' + BC.split('-')[0], 'Seconds': int(m.group(2)), 'Json': sha(os.path.join(CC, BC, 'base.coverage.json')),
                       'Results': int(m.group(5)), 'Outcomes': m.group(6)}
cov = text(os.path.join(CC, 'cov-p6-v52.txt'))
out['Coverage'] = {'Output': sha(os.path.join(CC, 'cov-p6-v52.txt')), 'Summary': cov.split('\n')[2], 'Verdict': cov.split('\n')[3].split()[-1],
                   'GreenJson': sha(os.path.join(D6, 'core-green-cov.coverage.json'))}
census = text(os.path.join(CC, 'census-c13-v52.txt'))
out['Census'] = {'Verdict': 'PASA' if '\ncenso PASA\n' in census else 'CAE', 'Output': sha(os.path.join(CC, 'census-c13-v52.txt')),
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
du = driver('r6u-u9')
out['UiRequiredFilters'] = {'Copy': '/d/r6u', 'Driver': sha(os.path.join(CC, 'driver-r6u-u9.txt')),
                            'Runs': {m.group(1): m.group(3) for m in re.finditer(r'^  ui (\S+) rc=\d+ (\S+) :: filas \d+ (\{[^}]*\})', du, re.M)},
                            'Builds': re.findall(r'^(b-\w+ rc=\d+ errores=\d+ avisos-CS=\d+)', du, re.M)}
out['History'] = {
    '/d/r6a': 'variante A, PARA (body_end)', '/d/r6b': 'variante A, PARA (CS0535)', '/d/r6c': 'variante A, PARA (ReadyPlan y el constructor)',
    '/d/r6d': 'variante A medida (10 CS8618, artefacto del bucle)', '/d/r6e': 'variante B medida: 0 errores y 0 avisos CS (la fuente del GREEN)',
    '/d/r6f': 'la prueba lenta sin acotar: 388,6 s y 394,6 s',
    '/d/r6g': 'necesidad, PARA (un error en [InlineData] no se llevaba a su metodo)', '/d/r6h': 'necesidad: 201 de 235 bajas de B hacen falta',
    '/d/r6i': 'cobertura, PARA (la configuracion por linea de ordenes)', '/d/r6j': 'cobertura por clases (Cobertura): las 436 lineas',
    '/d/r6k': 'cobertura prueba a prueba de WPUR (Cobertura)', '/d/r6l': 'ensayo rapido del borrador 1 del RED',
    '/d/r6m': 'ensayo rapido del borrador 2: faltaba NamedScan (4 CS0103), historia', '/d/r6n': 'ensayo rapido del borrador 3: RED y GREEN 0/0',
    '/d/r6p': 'cobertura de la base, cortada por el cierre de la sesion tras la compilacion: historia',
    '/d/r6q': 'control de las CT, completo pero en la sesion cerrada: repetido en otra copia, historia'}
expected = {
    'red-only-ct01': out['Red']['Core']['FailedTests'] == ['RackCad.Tests.PushBackRetirePilotGuardTests.CT_PBT4D_01_TheCodeOfThePilotThatThePanelNoLongerUsesIsRetiredAndWhatTheFullModelUsesStays'],
    'green-0-failed': out['Green']['Core']['Failed'] == 0 and out['GreenWithCoverage']['Core']['Failed'] == 0,
    'ui-0-failed': all(u.get('Failed', 0) == 0 for b in ('Red', 'Green') for u in out[b]['UI']),
    'cs-0': out['CsWarnings'] == 0 and all(b['errors'] == 0 for b in builds.values()) and len(builds) == 5,
    'same-totals': out['Red']['Core']['Total'] == out['Green']['Core']['Total'],
    'coverage': out['Coverage']['Verdict'] == 'PASA', 'census': out['Census']['Verdict'] == 'PASA',
    'rehearsal': out['Rehearsal'] == {'Red': {'CT-PBT4D-01': 'Failed', 'CT-PBT4D-02': 'Passed'}, 'Green': {'CT-PBT4D-01': 'Passed', 'CT-PBT4D-02': 'Passed'}},
    'control': out['Control'].get('base-cts', {}).get('Outcomes') == "{'Failed': 2}" and out['Control'].get('red-A', {}).get('Outcomes') == "{'Failed': 1, 'Passed': 1}",
    'no-overlap': out['RequiredTests']['Overlap'] == 'solapamientos: 0 []',
    'ui19-0-failed': all("'Failed'" not in v for v in out['UiRequiredFilters']['Runs'].values()) and len(out['UiRequiredFilters']['Runs']) == 21
                     and all(b.endswith('errores=0 avisos-CS=0') for b in out['UiRequiredFilters']['Builds']),
    'base-is-ci-baseline': out['BaseCoverage']['Results'] == 19619 and out['BaseCoverage']['Outcomes'] == "{'NotExecuted': 461, 'Passed': 19158}",
}
out['Expected'] = expected
out['AllAsExpected'] = all(expected.values())
data = json.dumps(out, ensure_ascii=False, indent=2, sort_keys=False) + '\n'
with open(os.path.join(CC, 'pbt4d-precondition6.json'), 'w', encoding='utf-8', newline='\n') as f:
    f.write(data)

# compile-check.md
md = ['# PB-T4d, precondicion 6 (paquete v5.2): el registro', '',
      'Copias exportadas de `%s`, fuera de todo worktree; nada en el checkout principal salvo `git archive`. El resumen, `pbt4d-precondition6.json` '
      '(sha256 `%s`). Todo lo previsto: **%s**.' % (REV, hashlib.sha256(data.encode('utf-8')).hexdigest(), 'SI' if out['AllAsExpected'] else 'NO'), '',
      '## Las corridas', '',
      '| Copia | Que | Registro |', '|---|---|---|',
      '| `/d/%s` | La principal: el RED literal y el GREEN literal, la suite completa de Core en las dos ramas, los 18 filtros de UI, el Plugin y la cobertura del GREEN (JSON) | `driver-%s.txt` (`%s`) |' % (P6.split('-')[0], P6, sha(os.path.join(CC, 'driver-%s.txt' % P6))),
      '| `/d/%s` | La cobertura de referencia de la base sin cambios (JSON), la suite completa de Core | `driver-%s.txt` (`%s`) |' % (BC.split('-')[0], BC, sha(os.path.join(CC, 'driver-%s.txt' % BC))),
      '| `/d/%s` | El control del ensayo de las CT: la base con solo las dos CT, y el RED con la retirada A | `driver-%s.txt` (`%s`) |' % (CT.split('-')[0], CT, sha(os.path.join(CC, 'driver-%s.txt' % CT))),
      '| `/d/r6u` | Los 19 filtros de UI de los RequiredTests en la base (la base de los MinSelected y de los totales de UI), y `PushBackEditAllWindowTests` con el RED y con el GREEN | `driver-r6u-u9.txt` (`%s`) |' % out['UiRequiredFilters']['Driver'],
      '', '**Historia** (no valen para la emision): ' + '; '.join('`%s`, %s' % kv for kv in out['History'].items()) + '.', '',
      '## Los resultados', '',
      '- **Compilacion:** ' + ', '.join('%s rc=%d errores=%d avisos CS=%d' % (k, v['rc'], v['errors'], v['csWarnings']) for k, v in builds.items()) + '.',
      '- **Core, RED:** %d = %d + %d + %d (Passed + Failed + Skipped), %s s; cae: %s.' % (
          out['Red']['Core']['Total'], out['Red']['Core']['Passed'], out['Red']['Core']['Failed'], out['Red']['Core']['Skipped'], secs.get('core-red'), ', '.join(out['Red']['Core']['FailedTests']) or 'ninguna'),
      '- **Core, GREEN:** %d = %d + %d + %d, %s s; con cobertura, %s s y %d Failed.' % (
          out['Green']['Core']['Total'], out['Green']['Core']['Passed'], out['Green']['Core']['Failed'], out['Green']['Core']['Skipped'], secs.get('core-green'), secs.get('core-green-cov'), out['GreenWithCoverage']['Core']['Failed']),
      '- **La prueba lenta acotada** (`%s`): %s s en el RED y %s s en el GREEN.' % (SLOW, out['SlowTestSeconds']['Red'], out['SlowTestSeconds']['Green']),
      '- **UI, los 18 filtros:** ' + '; '.join('n.o %d %d/%d/%d' % (u['n'], u.get('Passed', 0), u.get('Failed', 0), u.get('Skipped', 0)) for u in out['Green']['UI']) + ' (GREEN; Passed/Failed/Skipped). En el RED: ' + '; '.join('n.o %d %d/%d/%d' % (u['n'], u.get('Passed', 0), u.get('Failed', 0), u.get('Skipped', 0)) for u in out['Red']['UI']) + '.',
      '- **Los totales de UI de la CI:** %s; %s.' % (out['UiTotals']['red'], out['UiTotals']['green']),
      '- **El ensayo de las CT de texto:** %s. **Su control:** %s.' % (json.dumps(out['Rehearsal'], ensure_ascii=False), json.dumps(out['Control'], ensure_ascii=False)),
      '- **El censo de C13:** %s (`census-c13-v52.txt`, `%s`).' % (out['Census']['Verdict'], out['Census']['Output']),
      '- **La cobertura de la corrida generica que se queda** (`cov-p6-v52.txt`, `%s`): %s; **%s**.' % (out['Coverage']['Output'], out['Coverage']['Summary'], out['Coverage']['Verdict']),
      '- **Los RequiredTests** (`required-pbt4d.tsv`, `%s`): %s; %s.' % (out['RequiredTests']['Tsv'], out['RequiredTests']['Summary'], out['RequiredTests']['Overlap']),
      '- **Lo previsto:** ' + ', '.join('%s=%s' % kv for kv in expected.items()) + '.', '']
for branch in ('red', 'green'):
    md += ['## El resultado de cada clase que corre, %s (Core y los 18 filtros de UI; `clases-%s.txt`, `%s`)' % (branch.upper(), branch, out['ClassResults'][branch]), '', '```text']
    md += text(os.path.join(D6, 'clases-%s.txt' % branch)).rstrip('\n').split('\n') + ['```', '']
with open(os.path.join(CC, 'compile-check.md'), 'w', encoding='utf-8', newline='\n') as f:
    f.write('\n'.join(md))
print('json', hashlib.sha256(data.encode('utf-8')).hexdigest(), 'todo previsto', out['AllAsExpected'], expected)
