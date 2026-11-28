"""Cuenta por filtro del contrato (RequiredTests, en prompt.md) a partir de los TRX locales de una fase.

Uso: python -I filter_counts.py <RUN-dir> <fase: red|green> <salida.txt>
Core: la union de los TRX de las tres particiones de la suite completa; UI: el TRX de cada filtro (el que corrio ese filtro).
"""
import re
import sys
import xml.etree.ElementTree as ET

sys.stdout.reconfigure(encoding='utf-8', newline='\n')
run, phase, out = sys.argv[1:4]
NS = {'t': 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}

core_trx = {
    'red': ['tests/r2/core-ws.trx', 'tests/r2d/core-rest2.trx', 'tests/r2c/prep-green.trx'],
    'green': ['tests/g2/core-ws.trx', 'tests/g2/core-rest.trx', 'tests/g2/core-prep.trx'],
}[phase]
ui_dir = {'red': 'tests/r3', 'green': 'tests/g3'}[phase]


def load(path):
    return [(r.get('testName'), r.get('outcome')) for r in ET.parse(run + '/' + path).getroot().findall('.//t:UnitTestResult', NS)]


core = []
for p in core_trx:
    core.extend(load(p))

prompt = open(run + '/prompt.md', encoding='utf-8').read()
rows = re.findall(r'- (tests/[\w./]+\.csproj), filtro FullyQualifiedName~([\w.]+), minimo (\d+), RED esperado: (no|si)', prompt)
ui_filters = [l.strip() for l in open(run + '/tests/r3/ui-filters.txt', encoding='utf-8') if l.strip()]
lines = ['# fase %s: %d filtros del contrato' % (phase, len(rows))]
ok = True
for proj, flt, minimo, expect in rows:
    if 'UI.Tests' in proj:
        idx = ui_filters.index(flt) + 1 if flt in ui_filters else None
        results = load('%s/ui-%d.trx' % (ui_dir, idx)) if idx else []
        results = [r for r in results if flt in r[0]]
    else:
        results = [r for r in core if flt in r[0]]
    sel = len(results)
    passed = sum(1 for r in results if r[1] == 'Passed')
    failed = sum(1 for r in results if r[1] == 'Failed')
    skipped = sum(1 for r in results if r[1] == 'NotExecuted')
    minimo = int(minimo)
    if phase == 'green' or expect == 'no':
        good = sel > 0 and failed == 0 and passed >= minimo
    else:
        good = sel > 0 and failed == sel and sel >= minimo
    ok = ok and good
    lines.append('%s | %s | min=%d expectRed=%s | selected=%d passed=%d failed=%d skipped=%d | %s' % (
        'core' if 'UI.Tests' not in proj else 'ui', flt, minimo, expect, sel, passed, failed, skipped, 'OK' if good else 'FALLA'))
tot = lambda rs: (len(rs), sum(1 for r in rs if r[1] == 'Passed'), sum(1 for r in rs if r[1] == 'Failed'), sum(1 for r in rs if r[1] == 'NotExecuted'))
lines.append('totales de la suite completa de Core (3 particiones): total=%d passed=%d failed=%d skipped=%d' % tot(core))
lines.append('RESULTADO: %s' % ('OK' if ok else 'FALLA'))
open(out, 'w', encoding='utf-8', newline='\n').write('\n'.join(lines) + '\n')
print(lines[-2]); print(lines[-1])
