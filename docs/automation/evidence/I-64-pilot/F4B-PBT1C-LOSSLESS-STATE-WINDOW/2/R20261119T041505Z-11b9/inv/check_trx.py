#!/usr/bin/env python3
"""Read-only: summarize TRX files (counts, failed tests, INV-PBT1C-02 criterion for a RED filter).

usage: python3 check_trx.py <trx> [--red MIN]
"""
import re
import sys
import xml.etree.ElementTree as ET

NS = {"t": "http://microsoft.com/schemas/VisualStudio/TeamTest/2010"}
path = sys.argv[1]
red_min = None
if "--red" in sys.argv:
    red_min = int(sys.argv[sys.argv.index("--red") + 1])

root = ET.parse(path).getroot()
results = root.findall(".//t:UnitTestResult", NS)
counts = {}
failed = []
for r in results:
    outcome = r.get("outcome")
    counts[outcome] = counts.get(outcome, 0) + 1
    if outcome == "Failed":
        failed.append(r)
print("%s: total=%d %s" % (path.split("/")[-1], len(results), counts))
ok = True
for r in failed:
    name = r.get("testName")
    msg = r.find(".//t:ErrorInfo/t:Message", NS)
    stack = r.find(".//t:ErrorInfo/t:StackTrace", NS)
    msg_text = (msg.text or "") if msg is not None else ""
    stack_text = (stack.text or "") if stack is not None else ""
    first_stack = next((l.strip() for l in stack_text.split("\n") if l.strip()), "")
    print("  FAILED " + name)
    if red_min is not None:
        prefix = re.match(r"^(?:[A-Za-z_][\w`]*[.+])+[A-Za-z_][\w`]* : ", msg_text) is not None
        in_tests = re.search(r"\\tests\\|/tests/", first_stack) is not None
        assertion = msg_text.lstrip().startswith("Assert.")
        print("    prefijo-de-tipo=%s primera-linea-de-pila-bajo-tests=%s aserto=%s" % (prefix, in_tests, assertion))
        print("    primera-linea-de-pila: " + first_stack[:160])
        if prefix or not in_tests or not assertion:
            ok = False
if red_min is not None:
    selected = len(results)
    failed_n = len(failed)
    cond = failed_n == selected and selected >= red_min and counts.get("NotExecuted", 0) == 0 and counts.get("Skipped", 0) == 0 and counts.get("Passed", 0) == 0
    print("RED criterio: Failed(%d)==Selected(%d)>=%d, sin NotExecuted ni Skipped: %s ; por aserto: %s" % (failed_n, selected, red_min, cond, ok))
    sys.exit(0 if cond and ok else 1)
