# Solo lectura: remedicion (v2.3) de lo que pide D2 (R-11 y R-12 del paquete v2.2).
#  1. La guarda G-L1 (D-PBT1-15, condicion 10): reglas 1 y 2 sobre los archivos de CT-PBT1B-14 y CT-PBT1C-04 que ya existen,
#     con el texto sin comentarios (y con los literales de cadena vaciados para no confundir "//" dentro de una cadena).
#     Regla 2 en dos formas: la de la v2.2 (var/tipo id = ...ficha...;  id = ...ficha...;) y la ampliada con el foreach.
#     Regla 1b (candidata): un molde a una coleccion mutable de una expresion con ficha.
#  2. Las autopruebas en negativo: cada linea inyectada (a)-(d), y la (c) en forma compilable con molde, hace saltar la guarda.
#  3. El censo de asignaciones a Name en los archivos del Plugin que nombran PushBack (CT-PBT1B-15).
# No escribe nada.
# Uso: python -I remeasure_d2.py <raiz-del-worktree>
import os
import re
import sys

root = sys.argv[1]
APP = "src/RackCad.Application/Systems/PushBack/"
FILES_B = [APP + n for n in ("PushBackEditorState.cs", "PushBackEditorState.Load.cs", "PushBackCompositeEditorState.cs",
                             "PushBackCompositeEditorAssembler.cs", "PushBackEditorDesignAssembler.cs")] + [
    "src/RackCad.Application/Systems/Shared/RackModuleEditSession.cs",
    "src/RackCad.Application/Systems/Shared/RackModuleReconciliation.cs"]
WS = "src/RackCad.Application/Workspace/"
for dp, _, fs in os.walk(os.path.join(root, WS)):
    if os.sep + "bin" in dp or os.sep + "obj" in dp:
        continue
    for f in sorted(fs):
        if f.endswith(".cs"):
            FILES_B.append(os.path.relpath(os.path.join(dp, f), root).replace(os.sep, "/"))
FILES_C = ["src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs",
           "src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.Composite.cs"]

TOKENS = ["WorkingBaseline", "workingBaseline", "LastModuleReconciliation", "computation.System", "lastComputation.System",
          "session.System", "lastCompositeSystem", "SystemToInsert"]
TAIL = r"(?:\s*\??\.\s*\w+(?:\s*\([^()]*\))?)*\s*\??\.\s*"
ASSIGN = r"\w+\s*(?:=(?!=)|\+=|-=|\*=|/=|\+\+|--)"
CALL = r"(?:Add|AddRange|Clear|Remove|RemoveAt|RemoveAll|RemoveRange|Insert|InsertRange|Sort|Reverse)\s*\("
CAST = r"\(\s*\(\s*(?:List|IList|ICollection|Collection|HashSet|Dictionary|IDictionary)\s*<[^>]*>\s*\)[^;]*?"


def tok_re(t):
    return r"(?<![\w])" + re.escape(t).replace(r"\.", r"\s*\.\s*")


def rule1(text, tokens):
    hits = []
    for t in tokens:
        for pat in (tok_re(t) + TAIL + ASSIGN, tok_re(t) + TAIL + CALL):
            for m in re.finditer(pat, text):
                hits.append((t, m.group(0)))
    return hits


def rule1b(text, tokens):
    hits = []
    for t in tokens:
        for m in re.finditer(CAST + tok_re(t), text):
            hits.append((t, m.group(0)[:160]))
    return hits


def clean(src):
    src = re.sub(r'@"(?:[^"]|"")*"', '""', src)
    src = re.sub(r'\$?"(?:\\.|[^"\\\n])*"', '""', src)
    src = re.sub(r"'(?:\\.|[^'\\\n])'", "''", src)
    src = re.sub(r"/\*.*?\*/", " ", src, flags=re.S)
    src = re.sub(r"//[^\n]*", "", src)
    return src


def members(src):
    """Trozos del texto limpio por miembro de primer nivel de la clase (profundidad 2), heuristico por llaves."""
    out, depth, cur = [], 0, []
    for line in src.split("\n"):
        before = depth
        depth += line.count("{") - line.count("}")
        if before >= 2 or depth > 2:
            cur.append(line)
        if before > 2 and depth == 2 or (before == 2 and depth == 2 and line.strip().endswith(";")):
            out.append(" ".join(" ".join(cur).split()))
            cur = []
    if cur:
        out.append(" ".join(" ".join(cur).split()))
    return out


def aliases(member_text, tokens, with_foreach):
    found = set()
    tokalt = "|".join(tok_re(t) for t in tokens)
    for m in re.finditer(r"(?:\bvar|[\w<>\[\]?,.]+)?\s+(\w+)\s*=(?![=>])\s*([^;{}]*)", member_text):
        ident, expr = m.group(1), m.group(2)
        if ident in tokens or ident in ("lastComputation", "lastCompositeSystem", "workingBaseline"):
            continue
        if re.search(tokalt, expr):
            found.add(ident)
    if with_foreach:
        for m in re.finditer(r"foreach\s*\(\s*(?:var|[\w<>\[\]?,.]+)\s+(\w+)\s+in\s+([^)]*(?:\([^()]*\)[^)]*)*)\)", member_text):
            if re.search(tokalt, m.group(2)):
                found.add(m.group(1))
    return sorted(found)


def scan(files, label, tokens):
    print("==", label, "archivos", len(files))
    total1 = total2 = total2f = total1b = 0
    nmembers = 0
    alias_list = []
    for rel in files:
        path = os.path.join(root, rel)
        if not os.path.isfile(path):
            print("  NO EXISTE", rel)
            continue
        with open(path, encoding="utf-8-sig") as f:
            src = clean(f.read())
        flat = " ".join(src.split())
        h1 = rule1(flat, tokens)
        h1b = rule1b(flat, tokens)
        total1 += len(h1)
        total1b += len(h1b)
        for h in h1:
            print("  R1", rel, h)
        for h in h1b:
            print("  R1b", rel, h)
        for mem in members(src):
            nmembers += 1
            al = aliases(mem, tokens, False)
            alf = aliases(mem, tokens, True)
            if alf:
                alias_list.append((rel.split("/")[-1], alf))
            if al:
                h2 = rule1(mem, al)
                total2 += len(h2)
                for h in h2:
                    print("  R2", rel, "alias", al, h)
            extra = [a for a in alf if a not in al]
            if extra:
                h2f = rule1(mem, extra)
                total2f += len(h2f)
                for h in h2f:
                    print("  R2-foreach", rel, "alias", extra, h)
    print("  miembros=%d alias-por-ficha=%d" % (nmembers, sum(len(a) for _, a in alias_list)))
    for f, a in alias_list:
        print("   alias", f, a)
    print("  TOTAL regla1=%d regla2=%d regla2-foreach=%d regla1b=%d" % (total1, total2, total2f, total1b))


scan(FILES_B, "CT-PBT1B-14 (Application, archivos que existen)", TOKENS)
scan(FILES_C, "CT-PBT1C-04 (W y WC)", TOKENS)

print("== autopruebas en negativo (cada una sola, sobre un texto neutro)")
SELF = {
    "a": "state.WorkingBaseline.Structure.Name = \"x\";",
    "b": "void M() { var b = state.WorkingBaseline; b.Structure.Modules.Clear(); }",
    "c-v22": "side.LastModuleReconciliation.Preserved.Add(\"x\");",
    "c-compilable": "((List<string>)side.LastModuleReconciliation.Preserved).Add(\"x\");",
    "d": "lastComputation.System.Name = \"x\";",
    "e-foreach": "void M() { foreach (var module in workingBaseline.Structure.Modules) { module.Length = 1.0; } }",
}
for k, text in SELF.items():
    flat = " ".join(clean(text).split())
    r1 = rule1(flat, TOKENS)
    r1b = rule1b(flat, TOKENS)
    r2 = []
    r2f = []
    for mem in [flat]:
        al = aliases(mem, TOKENS, False)
        alf = aliases(mem, TOKENS, True)
        r2 = rule1(mem, al) if al else []
        r2f = rule1(mem, [a for a in alf if a not in al]) if alf else []
    print("  %-13s regla1=%d regla2=%d regla2-foreach=%d regla1b=%d" % (k, len(r1), len(r2), len(r2f), len(r1b)))

print("== censo de Name en el Plugin (archivos que nombran PushBack)")
plug = os.path.join(root, "src", "RackCad.Plugin")
NAME = re.compile(r"(?<![\w.])\w+(?:\s*\.\s*\w+)*\s*\.\s*Name\s*=(?![=>])")
NARROW = re.compile(r"(?<![\w.])(system|\w+\.System)\s*\.\s*Name\s*=(?!=)")
pb_files, narrow_all = [], []
for dp, _, fs in os.walk(plug):
    if os.sep + "bin" in dp or os.sep + "obj" in dp:
        continue
    for f in sorted(fs):
        if not f.endswith(".cs"):
            continue
        p = os.path.join(dp, f)
        rel = os.path.relpath(p, root).replace(os.sep, "/")
        with open(p, encoding="utf-8-sig") as fh:
            raw = fh.read()
        lines = clean(raw).split("\n")
        for i, l in enumerate(lines, 1):
            if NARROW.search(l):
                narrow_all.append("%s:%d" % (rel, i))
        if "PushBack" not in raw:
            continue
        pb_files.append(rel)
        for i, l in enumerate(lines, 1):
            if NAME.search(l):
                print("  %s:%d\t%s" % (rel, i, " ".join(l.split())))
print("  archivos del Plugin que nombran PushBack:", len(pb_files))
print("  la regex estrecha de la v2.2 sobre TODO el Plugin:", len(narrow_all), narrow_all)
