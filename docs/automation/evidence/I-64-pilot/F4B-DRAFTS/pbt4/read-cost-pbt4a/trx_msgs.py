import sys
import sys
sys.stdout.reconfigure(encoding="utf-8")
import xml.etree.ElementTree as ET

NS = {'t': 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}
root = ET.fromstring(open(sys.argv[1], 'rb').read())
for r in root.findall('.//t:UnitTestResult', NS):
    if r.get('outcome') == 'Failed':
        msg = r.find('.//t:ErrorInfo/t:Message', NS)
        st = r.find('.//t:ErrorInfo/t:StackTrace', NS)
        print('###', r.get('testName').split('.')[-1][:40])
        print((msg.text or '').replace('/home/runner/work/Calculadora_de_racks/Calculadora_de_racks/', ''))
        s = (st.text or '').replace('/home/runner/work/Calculadora_de_racks/Calculadora_de_racks/', '')
        print('STACK:', s.strip().splitlines()[0][-120:] if s.strip() else '')
        print()
