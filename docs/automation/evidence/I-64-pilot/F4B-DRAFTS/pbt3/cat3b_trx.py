import collections
import sys
import xml.etree.ElementTree as ET

# Uso: python -I cat3b_trx.py <core.trx>
# Cuenta, en el TRX de la suite completa de Core (corrida 3 de la sesion, esqueleto inerte con los tres nombres sobre eab27a9e), los
# resultados que selecciona cada filtro FullyQualifiedName~ de las RequiredTests de PB-T3b, por el nombre completo de su metodo
# (clase.metodo, como lo compara vstest), y su desenlace. No escribe nada.
sys.stdout.reconfigure(encoding="utf-8", errors="replace")
NS = "{http://microsoft.com/schemas/VisualStudio/TeamTest/2010}"
root = ET.parse(sys.argv[1]).getroot()
fqn = {}
for ut in root.iter(NS + "UnitTest"):
    tm = ut.find(NS + "TestMethod")
    fqn[ut.get("id")] = tm.get("className") + "." + tm.get("name")
FILTERS = ["RackCad.Tests.PushBackFullWrite", "RackCad.Tests.PushBackPilotUpdateEquivalenceGuardTests", "RackCad.Tests.PushBackWritePathGuardTests",
           "RackCad.Tests.Workspace", "RackCad.Tests.PushBackFullDraft", "RackCad.Tests.PushBackDraftWriteGuardTests"]
res = [(fqn.get(r.get("testId"), r.get("testName")), r.get("outcome")) for r in root.iter(NS + "UnitTestResult")]
print("resultados en el TRX:", len(res), dict(collections.Counter(o for _, o in res)))
for f in FILTERS:
    sel = [(n, o) for n, o in res if f in n]
    methods = set(n for n, _ in sel)
    print("%-58s resultados %5d  metodos %5d  %s" % (f, len(sel), len(methods), dict(collections.Counter(o for _, o in sel))))
