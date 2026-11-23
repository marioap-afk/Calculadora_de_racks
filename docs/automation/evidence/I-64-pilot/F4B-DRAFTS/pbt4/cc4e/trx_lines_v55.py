"""PB-T4e (paquete v5.5), precondicion 6: las lineas de una TRX. Por cada resultado: su clase, su prueba y su desenlace, y, para uno que no pasa, la
primera linea de su StackTrace y su mensaje entero (las lineas de una sonda, tras la marca SONDA). Sirve para la sonda de las CT y para la tabla
"CT -> la asercion que falla en el RED". Solo lee la TRX; escribe UTF-8 con LF.

Uso: python -I trx_lines_v55.py <trx> <salida.txt>
"""
import sys
import xml.etree.ElementTree as ET

sys.dont_write_bytecode = True
NS = {'t': 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}
trx, out = sys.argv[1:3]
root = ET.parse(trx).getroot()
defs = {}
for ut in root.iterfind('.//t:TestDefinitions/t:UnitTest', NS):
    tm = ut.find('t:TestMethod', NS)
    defs[ut.get('id')] = (tm.get('className'), tm.get('name'))
lines = []
for r in sorted(root.iterfind('.//t:Results/t:UnitTestResult', NS), key=lambda x: defs[x.get('testId')]):
    cls, name = defs[r.get('testId')]
    lines.append('== %s %s.%s' % (r.get('outcome'), cls, name))
    if r.get('outcome') == 'Passed':
        continue
    msg = r.find('t:Output/t:ErrorInfo/t:Message', NS)
    st = r.find('t:Output/t:ErrorInfo/t:StackTrace', NS)
    m = ((msg.text or '') if msg is not None else '').replace('\r\n', '\n')
    s = ((st.text or '') if st is not None else '').replace('\r\n', '\n')
    first = next((x.strip() for x in s.split('\n') if x.strip()), '')
    lines.append('stack: ' + first)
    if 'SONDA\n' in m:
        lines += ['  ' + x for x in m.split('SONDA\n', 1)[1].split('\n')]
    else:
        lines += ['  msg: ' + x for x in m.split('\n')]
with open(out, 'w', encoding='utf-8', newline='\n') as f:
    f.write('\n'.join(lines) + '\n')
print('%d resultados' % sum(1 for x in lines if x.startswith('== ')))
