import sys, xml.etree.ElementTree as ET
ns={'t':'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}
r=ET.parse(sys.argv[1]).getroot()
for u in r.iter('{%s}UnitTestResult'%ns['t']):
    if u.get('outcome')=='Failed':
        print('==',u.get('testName'))
        m=u.find('.//t:ErrorInfo/t:Message',ns); st=u.find('.//t:ErrorInfo/t:StackTrace',ns)
        print((m.text or '')[:1500]); print((st.text or '')[:900])
