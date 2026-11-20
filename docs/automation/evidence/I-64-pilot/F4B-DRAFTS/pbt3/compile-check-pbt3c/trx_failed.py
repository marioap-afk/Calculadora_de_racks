"""Resume un TRX: total, pasadas, fallidas, omitidas y la lista de metodos fallidos (clase.metodo, sin los argumentos de la Theory)."""
import sys, collections
import xml.etree.ElementTree as ET
NS = '{http://microsoft.com/schemas/VisualStudio/TeamTest/2010}'
t = ET.parse(sys.argv[1]).getroot()
c = t.find(f'{NS}ResultSummary/{NS}Counters')
print('counters', {k: c.get(k) for k in ('total', 'executed', 'passed', 'failed', 'notExecuted')})
fails = collections.Counter()
for r in t.iter(f'{NS}UnitTestResult'):
    if r.get('outcome') == 'Failed':
        fails[r.get('testName').split('(')[0]] += 1
print('failed methods', len(fails), 'rows', sum(fails.values()))
for name, n in sorted(fails.items()):
    print('  FAIL', n, name)
