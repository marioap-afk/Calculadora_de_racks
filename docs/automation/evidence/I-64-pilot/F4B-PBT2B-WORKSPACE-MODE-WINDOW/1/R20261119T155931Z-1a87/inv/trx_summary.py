import sys, re
import xml.etree.ElementTree as ET
ns = {"t": "http://microsoft.com/schemas/VisualStudio/TeamTest/2010"}
root = ET.parse(sys.argv[1]).getroot()
c = root.find("t:ResultSummary/t:Counters", ns).attrib
print("total=%s executed=%s passed=%s failed=%s notExecuted=%s timeout=%s" % (c.get("total"), c.get("executed"), c.get("passed"), c.get("failed"), c.get("notExecuted"), c.get("timeout")))
for r in root.iter("{%s}UnitTestResult" % ns["t"]):
    if r.get("outcome") != "Passed" and r.get("outcome") != "NotExecuted":
        msg = r.find("t:Output/t:ErrorInfo/t:Message", ns)
        st = r.find("t:Output/t:ErrorInfo/t:StackTrace", ns)
        first = (st.text or "").strip().splitlines()[0] if st is not None and st.text else ""
        print(r.get("outcome"), r.get("testName"), "| msg:", (msg.text or "")[:40].replace("\n", " "), "| stack:", first[-90:])
