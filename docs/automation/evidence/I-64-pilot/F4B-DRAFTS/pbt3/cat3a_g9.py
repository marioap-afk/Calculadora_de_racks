import re
import runpy
import sys

# Uso: python -I cat3a_g9.py <carpeta> <checkout> <rev> <paquete.md>
# Las guardas condicionales (G-9, G-7, el Transaction de Pilot/, la pureza de UP/UR) aplicadas sin control a los corpus del barrido:
# un termino que el control tambien tiene no basta para descartarlas, porque su condicion (nombrar el piloto, estar en Pilot/) es otra.
sys.stdout.reconfigure(encoding="utf-8", errors="replace")
HERE, REPO, REV, PKG = sys.argv[1:5]
sys.argv = ["cat3a_sweep.py", REPO, REV, PKG]
# reutiliza la construccion del corpus del barrido sin ejecutar el barrido: se lee el guion y se corta antes de la lista de pruebas
src = open(HERE + "/cat3a_sweep.py", encoding="utf-8").read()
cut = src.index("files = git(")
ns = {}
exec(compile(src[:cut], "corpus", "exec"), ns)
groups = ns["groups"]

G9 = {"RedrawInTransaction": r"RedrawInTransaction", "RedrawInPlace": r"RedrawInPlace", "DrawAndPlace": r"DrawAndPlace",
      "SystemBlockWriter": r"SystemBlockWriter", "RackBlockData.Write": r"RackBlockData\.Write", "ForWrite": r"\bForWrite\b",
      "UpgradeOpen": r"\bUpgradeOpen\b", "AppendEntity": r"\bAppendEntity\b", "Commit": r"\.Commit\s*\(", "StartTransaction": r"StartTransaction",
      "StartOpenCloseTransaction": r"StartOpenCloseTransaction", "SyncName": r"\bSyncName\b", "PurgeUnreferenced": r"PurgeUnreferenced",
      "EnsureForPlan": r"EnsureForPlan", "WblockCloneObjects": r"WblockCloneObjects", "SetAt": r"\bSetAt\s*\(", "Erase": r"\.Erase\s*\(",
      "SendStringToExecute": r"SendStringToExecute", "CommandMethod": r"CommandMethod", "LockDocument": r"LockDocument"}
NAMES_PILOT = r"PushBackPilot\w*|RackPilot\w*|RackCad\.Application\.Systems\.PushBack\.Pilot"
G7 = r"\.(?:PrepareUnits|BeginMutation|WriteUnit|Seal)\s*\("
POST = r"\.Post\s*\("
for g, text in groups.items():
    print("==", g, "| nombra el piloto:", bool(re.search(NAMES_PILOT, text)))
    for name, rx in G9.items():
        for m in re.finditer(rx, text):
            print("   G-9", name, "->", text[max(0, m.start() - 60):m.end() + 30].replace("\n", " | "))
    for m in re.finditer(G7, text):
        print("   G-7", "->", text[max(0, m.start() - 60):m.end() + 30].replace("\n", " | "))
    for m in re.finditer(POST, text):
        print("   G-7 .Post(", "->", text[max(0, m.start() - 60):m.end() + 30].replace("\n", " | "))
    for m in re.finditer(r"Transaction", text):
        print("   Pilot/ Transaction", "->", text[max(0, m.start() - 60):m.end() + 30].replace("\n", " | "))
