import sys, xml.etree.ElementTree as ET
ns={'t':'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}
r=ET.parse(sys.argv[1]).getroot()
for u in r.iter('{%s}UnitTestResult'%ns['t']):
    if u.get('outcome')=='Failed':
        m=u.find('.//t:ErrorInfo/t:Message',ns)
        msg=(m.text or '').strip().replace('\n',' | ')[:200] if m is not None else ''
        print(u.get('testName').split('.')[-1][:100],'=>',msg)
