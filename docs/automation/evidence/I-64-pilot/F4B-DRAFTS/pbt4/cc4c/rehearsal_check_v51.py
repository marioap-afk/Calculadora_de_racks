"""PB-T4c (paquete v5.1), precondicion 6, paso 8: la lectura de la TRX del ensayo de las CT de texto (pbt4c-rehearsal-textct.cs). En el RED,
CT-07 y CT-15 (ExpectRed false) pasan y CT-01, 08 y 13 (ExpectRed true) caen por su Assert.Equal (el mensaje es el de Assert.Equal y la primera
linea del StackTrace esta bajo tests/); en el GREEN, las cinco pasan. Solo lee la TRX; escribe en UTF-8 con LF.

Uso: python -I rehearsal_check_v51.py <red|green> <trx> <salida.txt>
"""
import sys
import xml.etree.ElementTree as ET

sys.stdout.reconfigure(encoding='utf-8', newline='\n')
NS = {'t': 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}
branch, trx, out = sys.argv[1:4]
WANT = {'red': {'CT01': 'Failed', 'CT07': 'Passed', 'CT08': 'Failed', 'CT13': 'Failed', 'CT15': 'Passed'},
        'green': {'CT01': 'Passed', 'CT07': 'Passed', 'CT08': 'Passed', 'CT13': 'Passed', 'CT15': 'Passed'}}[branch]
root = ET.parse(trx).getroot()
defs = {ut.get('id'): ut.find('t:TestMethod', NS) for ut in root.iterfind('.//t:TestDefinitions/t:UnitTest', NS)}
seen, lines, ok = {}, [], True
for r in root.iterfind('.//t:Results/t:UnitTestResult', NS):
    tm = defs[r.get('testId')]
    name = tm.get('name')
    msg = r.find('t:Output/t:ErrorInfo/t:Message', NS)
    st = r.find('t:Output/t:ErrorInfo/t:StackTrace', NS)
    message = (msg.text or '') if msg is not None else ''
    stack = (st.text or '') if st is not None else ''
    first = next((l.strip() for l in stack.split('\n') if l.strip()), '')
    by_assert = message.startswith('Assert.Equal() Failure') and ('\\tests\\' in first or '/tests/' in first)
    seen[name] = (r.get('outcome'), by_assert)
    lines.append('%s %s %s%s' % (tm.get('className'), name, r.get('outcome'), (' por-asercion=%s' % by_assert) if r.get('outcome') == 'Failed' else ''))
    if r.get('outcome') == 'Failed':
        lines.append('  ' + first[:200])
        lines.extend('  | ' + l for l in message.replace('\r\n', '\n').split('\n')[:12])
for name, want in WANT.items():
    got = seen.get(name)
    good = got is not None and got[0] == want and (want != 'Failed' or got[1])
    ok = ok and good
    lines.append('esperado %s %s: %s' % (name, want, 'OK' if good else 'DISTINTO (%s)' % (got,)))
ok = ok and len(seen) == len(WANT)
lines.append('ensayo %s: %s' % (branch, 'PASA' if ok else 'PARA'))
with open(out, 'w', encoding='utf-8', newline='\n') as f:
    f.write('\n'.join(lines) + '\n')
print(lines[-1])
sys.exit(0 if ok else 1)
