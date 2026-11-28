"""La tabla CT -> clase.metodo y CT -> primera divergencia de la asercion que falla en el RED reparado. Solo lee los TRX y los archivos de las CT."""
import glob
import os
import re
import sys
import xml.etree.ElementTree as ET

HERE = os.path.dirname(os.path.abspath(__file__))
RUN = os.path.dirname(HERE)
W = RUN
for _ in range(6):
    W = os.path.dirname(W)
NS = {'t': 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}
FILES = ['tests/RackCad.Tests/PushBackFullPanelTests.cs', 'tests/RackCad.Tests/PushBackFullPanelGuardTests.cs',
         'tests/RackCad.Tests/PushBackFullPanelCensusGuardTests.cs', 'tests/RackCad.Tests/PushBackPanelDraftGuardTests.cs',
         'tests/RackCad.Tests/WorkspaceFullCopyGuardTests.cs', 'tests/RackCad.UI.Tests/PushBackFullPanelFieldTests.cs']


def main():
    sys.stdout.reconfigure(encoding='utf-8', errors='replace')
    mapping = {}
    for f in FILES:
        cls = f.split('/')[-1][:-3]
        text = open(os.path.join(W, *f.split('/')), encoding='utf-8').read()
        for m in re.finditer(r'public void (CT_PBT4B_(\d\d)_\w+)\(', text):
            mapping[m.group(2)] = (cls, m.group(1))
    print('== CT -> clase.metodo')
    for k in sorted(mapping):
        print('CT-PBT4B-%s %s.%s' % (k, mapping[k][0], mapping[k][1]))
    rows = {}

    def scan(trx):
        for r in ET.parse(trx).getroot().iter('{%s}UnitTestResult' % NS['t']):
            n = r.get('testName')
            m = re.search(r'\.(CT_PBT4B_(\d\d))_', n)
            if m and r.get('outcome') == 'Failed':
                msg = r.find('t:Output/t:ErrorInfo/t:Message', NS).text
                st = r.find('t:Output/t:ErrorInfo/t:StackTrace', NS).text
                first = [l.strip() for l in st.split('\n') if l.strip().startswith('at ')][0]
                loc = re.search(r'in .*?[\\/](tests[\\/].*)', first)
                e = re.search(r'Expected: (.*)', msg)
                a = re.search(r'Actual:   (.*)', msg)
                rows[m.group(2)] = (loc.group(1).replace('\\', '/') if loc else first, e.group(1)[:90] if e else '', a.group(1)[:90] if a else '')

    scan(os.path.join(RUN, 'tests', 'rr1', 'core-full.trx'))
    for t in sorted(glob.glob(os.path.join(RUN, 'tests', 'rr2', 'f*.trx'))):
        scan(t)
    print('== CT -> la asercion que falla en el RED reparado: la linea de la prueba bajo tests/ y la primera divergencia de las lineas esperadas')
    for k in sorted(rows):
        print('CT-PBT4B-%s | %s | esperado %s | actual %s' % (k, rows[k][0], rows[k][1], rows[k][2]))
    print('pasan en el RED (ExpectRed false): CT-PBT4B-13, CT-PBT4B-19, CT-PBT4B-20')


if __name__ == '__main__':
    main()
