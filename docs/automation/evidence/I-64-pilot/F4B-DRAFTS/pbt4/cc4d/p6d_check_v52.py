"""PB-T4d (paquete v5.2, fase 2): la lectura de una TRX con el piloto retirado y las bajas quitadas: los totales y cada resultado que no pasa,
con su primera linea del StackTrace y de su mensaje, y, para los omitidos, si su motivo es de PB-T4b, de PB-T4c u otro. Lo que cae aqui cae fuera
de las bajas: es una fila nueva de la lista de PB-T4d. Solo lee la TRX; escribe en UTF-8 con LF.

Uso: python -I p6d_check_v52.py <trx> <salida.txt>
"""
import collections
import sys
import xml.etree.ElementTree as ET

sys.stdout.reconfigure(encoding='utf-8', newline='\n')
NS = {'t': 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}
trx, out = sys.argv[1:3]
root = ET.parse(trx).getroot()
defs = {}
for ut in root.iterfind('.//t:TestDefinitions/t:UnitTest', NS):
    tm = ut.find('t:TestMethod', NS)
    defs[ut.get('id')] = (tm.get('className'), tm.get('name'))
c = collections.Counter()
skips = collections.Counter()
failed = []
for r in root.iterfind('.//t:Results/t:UnitTestResult', NS):
    cls, name = defs[r.get('testId')]
    c[r.get('outcome')] += 1
    msg = r.find('t:Output/t:ErrorInfo/t:Message', NS)
    st = r.find('t:Output/t:ErrorInfo/t:StackTrace', NS)
    m = (msg.text or '') if msg is not None else ''
    s = (st.text or '') if st is not None else ''
    if r.get('outcome') == 'NotExecuted':
        text = m + ' ' + (r.get('testName') or '')
        skips['PB-T4c' if 'PB-T4c' in text else ('PB-T4b' if 'PB-T4b' in text else 'otro')] += 1
    elif r.get('outcome') != 'Passed':
        first_s = next((l.strip() for l in s.split('\n') if l.strip()), '')
        first_m = next((l.strip() for l in m.split('\n') if l.strip()), '')
        failed.append('%s %s.%s | %s | %s' % (r.get('outcome'), cls, r.get('testName') or name, first_s[:220], first_m[:220]))
lines = ['filas %d %s; omitidas por origen %s' % (sum(c.values()), dict(sorted(c.items())), dict(sorted(skips.items())))]
lines += ['  ' + f for f in sorted(failed)]
lines.append('no pasan: %d' % len(failed))
with open(out, 'w', encoding='utf-8', newline='\n') as f:
    f.write('\n'.join(lines) + '\n')
print(lines[0], '| no pasan', len(failed))
