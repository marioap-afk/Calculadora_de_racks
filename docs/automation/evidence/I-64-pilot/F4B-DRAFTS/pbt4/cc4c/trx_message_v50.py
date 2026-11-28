"""PB-T4c, precondicion 6 (paquete v5.0): el mensaje entero de cada resultado que falla de una TRX (las sondas acaban en Assert.Fail(texto)).
Solo lee la TRX; escribe la salida en UTF-8 con LF. Uso: python -I trx_message_v50.py <trx> <salida.txt>
"""
import sys
import xml.etree.ElementTree as ET

NS = {'t': 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}
trx, out = sys.argv[1], sys.argv[2]
root = ET.parse(trx).getroot()
lines = []
for r in root.iterfind('.//t:Results/t:UnitTestResult', NS):
    if r.get('outcome') != 'Failed':
        continue
    msg = r.find('t:Output/t:ErrorInfo/t:Message', NS)
    lines.append('## ' + r.get('testName'))
    text = (msg.text or '') if msg is not None else ''
    if text.startswith('Assert.Fail() Failure'):
        text = text.split('\n', 1)[1] if '\n' in text else ''
    lines.extend(text.replace('\r\n', '\n').split('\n'))
with open(out, 'w', encoding='utf-8', newline='\n') as f:
    f.write('\n'.join(lines) + '\n')
print('resultados=%d lineas=%d' % (sum(1 for l in lines if l.startswith('## ')), len(lines)))
