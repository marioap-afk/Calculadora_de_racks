import sys
import xml.etree.ElementTree as ET

ns = {"t": "http://microsoft.com/schemas/VisualStudio/TeamTest/2010"}
root = ET.parse(sys.argv[1]).getroot()
rows = []
for r in root.iter("{http://microsoft.com/schemas/VisualStudio/TeamTest/2010}UnitTestResult"):
    name = r.get("testName")
    outcome = r.get("outcome")
    msg = ""
    exc = ""
    err = r.find("t:Output/t:ErrorInfo", ns)
    if err is not None:
        m = err.find("t:Message", ns)
        msg = (m.text or "").strip().splitlines()[0] if m is not None and m.text else ""
    rows.append((outcome, name, msg))
mode = sys.argv[2] if len(sys.argv) > 2 else "failed"
counts = {}
for o, n, m in rows:
    counts[o] = counts.get(o, 0) + 1
print(counts)
if mode == "failed":
    for o, n, m in rows:
        if o != "Passed":
            print(o, "|", n, "|", m[:150])
