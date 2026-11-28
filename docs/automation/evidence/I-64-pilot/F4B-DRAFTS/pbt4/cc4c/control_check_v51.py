"""PB-T4c (paquete v5.1), el control negativo del paso 8 (p6_control50_v51.sh): sobre las piezas de la v5.0, el ensayo de las CT de texto tiene
que ver CA-T4c I-01. Lo esperado: en el RED, CT-01, 08, 13 y 15 Failed y CT-07 Passed; en el GREEN, CT-15 Failed y las otras cuatro Passed. Cada
Failed, por su Assert.Equal (la primera linea del StackTrace, bajo tests/), y el de CT-15 con su primera diferencia en la linea de la pureza (una
violacion de un archivo de Draft/ donde se esperaba ninguna). Que la violacion es #nullable lo dice el texto de las piezas: es lo unico que las
dos piezas de la v5.0 tienen de distinto en esos archivos, ademas del ? de ProjectOf. Solo lee la TRX; escribe en UTF-8 con LF.

Uso: python -I control_check_v51.py <red|green> <trx> <salida.txt>
"""
import sys
import xml.etree.ElementTree as ET

sys.stdout.reconfigure(encoding='utf-8', newline='\n')
NS = {'t': 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}
branch, trx, out = sys.argv[1:4]
WANT = {'red': {'CT01': 'Failed', 'CT07': 'Passed', 'CT08': 'Failed', 'CT13': 'Failed', 'CT15': 'Failed'},
        'green': {'CT01': 'Passed', 'CT07': 'Passed', 'CT08': 'Passed', 'CT13': 'Passed', 'CT15': 'Failed'}}[branch]
root = ET.parse(trx).getroot()
defs = {ut.get('id'): ut.find('t:TestMethod', NS) for ut in root.iterfind('.//t:TestDefinitions/t:UnitTest', NS)}
seen, lines, ok = {}, [], True
for r in root.iterfind('.//t:Results/t:UnitTestResult', NS):
    tm = defs[r.get('testId')]
    name = tm.get('name')
    msg = r.find('t:Output/t:ErrorInfo/t:Message', NS)
    st = r.find('t:Output/t:ErrorInfo/t:StackTrace', NS)
    message = (msg.text or '') if msg is not None else ''
    first = next((l.strip() for l in ((st.text or '') if st is not None else '').split('\n') if l.strip()), '')
    by_assert = message.startswith('Assert.Equal() Failure') and ('\\tests\\' in first or '/tests/' in first)
    seen[name] = (r.get('outcome'), by_assert, message)
    lines.append('%s %s %s%s' % (tm.get('className'), name, r.get('outcome'), (' por-asercion=%s' % by_assert) if r.get('outcome') == 'Failed' else ''))
    if r.get('outcome') == 'Failed':
        lines.extend('  | ' + l for l in message.replace('\r\n', '\n').split('\n')[:8])
for name, want in WANT.items():
    got = seen.get(name)
    good = got is not None and got[0] == want and (want != 'Failed' or got[1])
    if name == 'CT15' and good:
        # Assert.Equal solo muestra el tramo de la primera diferencia: tiene que estar en la linea de la pureza (G-L1 y sus autopruebas, antes,
        # son iguales), con una violacion de un archivo de Draft/ donde se esperaba "violaciones=[]".
        actual = next((l for l in got[2].replace('\r\n', '\n').split('\n') if l.strip().startswith('Actual:')), '')
        good = 'violaciones=[src' in actual
        lines.append('  la primera diferencia de CT-15, en la pureza: %s (%s)' % (good, actual.strip()[:90]))
    ok = ok and good
    lines.append('esperado %s %s: %s' % (name, want, 'OK' if good else 'DISTINTO (%s)' % ((got[0], got[1]) if got else None,)))
ok = ok and len(seen) == len(WANT)
lines.append('control %s: %s' % (branch, 'VE I-01' if ok else 'NO VE I-01'))
with open(out, 'w', encoding='utf-8', newline='\n') as f:
    f.write('\n'.join(lines) + '\n')
print(lines[-1])
