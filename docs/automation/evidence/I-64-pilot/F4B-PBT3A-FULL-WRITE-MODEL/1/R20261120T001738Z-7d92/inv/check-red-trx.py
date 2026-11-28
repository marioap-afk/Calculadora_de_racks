import re
import sys
import xml.etree.ElementTree as ET

# Criterio del RED sobre un TRX con ExpectRed: cada prueba seleccionada falla (Failed), por asercion (el mensaje no lleva un
# prefijo de tipo cualificado seguido de " : "), con la primera linea del StackTrace bajo tests/; ninguna NotExecuted ni Skipped.
NS = {"t": "http://microsoft.com/schemas/VisualStudio/TeamTest/2010"}


def check(path):
    root = ET.parse(path).getroot()
    results = root.findall(".//t:UnitTestResult", NS)
    counts = {"selected": len(results), "failed": 0, "assertion": 0, "exception": 0, "first_line_under_tests": 0, "not_failed": 0}
    problems = []
    for result in results:
        name = result.get("testName")
        outcome = result.get("outcome")
        if outcome != "Failed":
            counts["not_failed"] += 1
            problems.append(name + ": outcome " + str(outcome))
            continue
        counts["failed"] += 1
        message = result.findtext(".//t:ErrorInfo/t:Message", default="", namespaces=NS) or ""
        stack = result.findtext(".//t:ErrorInfo/t:StackTrace", default="", namespaces=NS) or ""
        if re.match(r"^\S+ : ", message.strip()):
            counts["exception"] += 1
            problems.append(name + ": excepcion " + message.strip().split("\n")[0][:120])
        else:
            counts["assertion"] += 1
        lines = [line.strip() for line in stack.splitlines() if line.strip()]
        first = lines[0] if lines else ""
        if re.search(r"[\\/]tests[\\/]", first):
            counts["first_line_under_tests"] += 1
        else:
            problems.append(name + ": primera linea del StackTrace fuera de tests/: " + first[:160])
    return counts, problems


total_ok = True
for path in sys.argv[1:]:
    counts, problems = check(path)
    ok = (counts["selected"] > 0 and counts["failed"] == counts["selected"] and counts["assertion"] == counts["selected"]
          and counts["first_line_under_tests"] == counts["selected"] and counts["not_failed"] == 0)
    total_ok = total_ok and ok
    print(path.replace("\\", "/").split("/")[-1], counts, "CRITERIO-RED=" + ("PASS" if ok else "FAIL"))
    for problem in problems:
        print("   ", problem)
sys.exit(0 if total_ok else 1)
