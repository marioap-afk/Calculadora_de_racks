# Solo lectura (comprobacion acotada de la v2.3 de PB-T1). No escribe nada.
# 1. El censo de asignaciones a Name en los .cs del Plugin que contienen "PushBack" (CT-PBT1B-15), con la regex del paquete.
# 2. Las reglas 1 y 1b de G-L1, tal como las escribe el paquete (D-PBT1-15), frente a formas de mutacion que no estan en
#    sus autopruebas: cadena con indexador, molde con "as", patron "is", metodo mutador del dominio.
# 3. En los archivos de CT-PBT1B-14 y CT-PBT1C-04, las apariciones de una ficha seguida de un indexador o de "as"/"is".
# Uso: python -I check_v23.py <raiz-del-worktree>
import os
import re
import sys

root = sys.argv[1]


def clean(src):
    src = re.sub(r'@"(?:[^"]|"")*"', '""', src)
    src = re.sub(r'\$?"(?:\\.|[^"\\\n])*"', '""', src)
    src = re.sub(r"'(?:\\.|[^'\\\n])'", "''", src)
    src = re.sub(r"/\*.*?\*/", " ", src, flags=re.S)
    src = re.sub(r"//[^\n]*", "", src)
    return src


# ---- 1. censo de Name
plug = os.path.join(root, "src", "RackCad.Plugin")
NAME = re.compile(r"(?<![\w.])\w+(?:\s*\.\s*\w+)*\s*\.\s*Name\s*=(?![=>])")
files = []
hits = []
for dp, _, fs in os.walk(plug):
    if os.sep + "bin" in dp or os.sep + "obj" in dp:
        continue
    for f in sorted(fs):
        if not f.endswith(".cs"):
            continue
        p = os.path.join(dp, f)
        with open(p, encoding="utf-8-sig") as fh:
            raw = fh.read()
        if "PushBack" not in raw:
            continue
        rel = os.path.relpath(p, root).replace(os.sep, "/")
        files.append(rel)
        for i, line in enumerate(clean(raw).split("\n"), 1):
            if NAME.search(line):
                hits.append("%s:%d  %s" % (rel, i, " ".join(line.split())))
print("== censo Name: archivos=%d asignaciones=%d" % (len(files), len(hits)))
for h in hits:
    print("  ", h)

# ---- 2. reglas del paquete frente a formas no ensayadas
TOKENS = ["WorkingBaseline", "workingBaseline", "LastModuleReconciliation", "computation.System", "lastComputation.System",
          "session.System", "lastCompositeSystem", "SystemToInsert"]


def tok(t):
    return re.escape(t).replace(r"\.", r"\s*\.\s*")


TAIL = r"(\s*\??\.\s*\w+(\s*\([^()]*\))?)*\s*\??\.\s*"
R1A = r"{0}" + TAIL + r"\w+\s*(=(?!=)|\+=|-=|\*=|/=|\+\+|--)"
R1B = r"{0}" + TAIL + r"(Add|AddRange|Clear|Remove|RemoveAt|RemoveAll|RemoveRange|Insert|InsertRange|Sort|Reverse)\s*\("
R1b = r"\(\s*\(\s*(List|IList|ICollection|Collection|HashSet|Dictionary|IDictionary)\s*<[^>]*>\s*\)[^;]*?{0}"


def rules(text):
    n1 = n1b = 0
    for t in TOKENS:
        if re.search(R1A.format(tok(t)), text) or re.search(R1B.format(tok(t)), text):
            n1 += 1
        if re.search(R1b.format(tok(t)), text):
            n1b += 1
    return n1, n1b


PROBES = {
    "indexador-asignacion": 'state.WorkingBaseline.Structure.Modules[0].Length = 1.0;',
    "indexador-sustitucion": 'lastComputation.System.Structure.Modules[0] = null;',
    "indexador-coleccion": 'workingBaseline.Structure.Fronts[0].Levels.Clear();',
    "molde-as": '(side.LastModuleReconciliation.Preserved as List<string>).Add("x");',
    "patron-is": 'if (side.LastModuleReconciliation.Preserved is List<string> l) { l.Add("x"); }',
    "metodo-dominio": 'state.WorkingBaseline.Structure.RecalculatePositions();',
    "control-a (autoprueba a)": 'state.WorkingBaseline.Structure.Name = "x";',
    "control-c (autoprueba c)": '((List<string>)side.LastModuleReconciliation.Preserved).Add("x");',
}
print("== reglas 1 y 1b del paquete frente a formas no ensayadas (fichas que casan)")
for k, v in PROBES.items():
    n1, n1b = rules(" ".join(clean(v).split()))
    print("  %-26s regla1=%d regla1b=%d" % (k, n1, n1b))

# ---- 3. apariciones hoy de una ficha con indexador o con as/is en los archivos de G-L1
APP = "src/RackCad.Application/Systems/PushBack/"
G = [APP + n for n in ("PushBackEditorState.cs", "PushBackEditorState.Load.cs", "PushBackCompositeEditorState.cs",
                       "PushBackCompositeEditorAssembler.cs", "PushBackEditorDesignAssembler.cs")] + [
    "src/RackCad.Application/Systems/Shared/RackModuleEditSession.cs",
    "src/RackCad.Application/Systems/Shared/RackModuleReconciliation.cs",
    "src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs",
    "src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.Composite.cs"]
for dp, _, fs in os.walk(os.path.join(root, "src", "RackCad.Application", "Workspace")):
    for f in sorted(fs):
        if f.endswith(".cs"):
            G.append(os.path.relpath(os.path.join(dp, f), root).replace(os.sep, "/"))
alt = "|".join(tok(t) for t in TOKENS)
IDX = re.compile(r"(?:" + alt + r")(?:\s*\??\.\s*\w+(?:\s*\([^()]*\))?)*\s*\[")
ASIS = re.compile(r"(?:" + alt + r")(?:\s*\??\.\s*\w+(?:\s*\([^()]*\))?)*\s+(?:as|is)\s+")
n = 0
for rel in G:
    p = os.path.join(root, rel)
    if not os.path.isfile(p):
        continue
    with open(p, encoding="utf-8-sig") as fh:
        text = clean(fh.read())
    for i, line in enumerate(text.split("\n"), 1):
        if IDX.search(line) or ASIS.search(line):
            n += 1
            print("  hoy %s:%d  %s" % (rel, i, " ".join(line.split())[:150]))
print("== archivos de G-L1 revisados=%d lineas con ficha+indexador o ficha+as/is=%d" % (len(G), n))
