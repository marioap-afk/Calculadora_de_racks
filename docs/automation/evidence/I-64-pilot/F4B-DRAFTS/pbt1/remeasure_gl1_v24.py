# Solo lectura: la guarda G-L1 de la v2.4 (V23C-I01 de la comprobacion acotada de la v2.3), sobre los archivos de CT-PBT1B-14 y
# CT-PBT1C-04 que ya existen, y sus autopruebas en negativo, cada una sobre un texto neutro. No escribe nada.
# Reglas (texto sin comentarios, con las cadenas vaciadas y los espacios colapsados):
#   1  mutacion a traves de una ficha: asignacion o llamada mutadora al final de una cadena de eslabones .Miembro, .Metodo(args) o [indice];
#   1b molde de una expresion con ficha a una coleccion mutable: ((T<...>)...ficha...), ficha... as T<, ficha... is T<...> x;
#   1c una ficha pasada como argumento a un mutador conocido de Domain/Application (Refresh, RefreshPhysicalModel, ...InPlace);
#   2  alias de un nivel: var/tipo x = ...ficha...; x = ...ficha...; foreach (var x in ...ficha...); ficha... is T<...> x;
#      y el parametro de un lambda cuya sentencia contiene una ficha. El alias cuenta como ficha para la regla 1 en el resto del miembro.
# Uso: python -I remeasure_gl1_v24.py <raiz-del-worktree>
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
REBIND = {"lastComputation", "lastCompositeSystem", "workingBaseline", "LastModuleReconciliation"}
LINK = r"(?:\s*\??\.\s*\w+(?:\s*\([^()]*\))?|\s*\[[^\]]*\])"
ASSIGN = r"\s*(?:\??\.\s*\w+|\[[^\]]*\])\s*(?:=(?![=>])|\+=|-=|\*=|/=|\+\+|--)"
MUTATORS = r"(?:Add|AddRange|Clear|Remove|RemoveAt|RemoveAll|RemoveRange|Insert|InsertRange|Sort|Reverse|RecalculatePositions)"
CALL = r"\s*\??\.\s*" + MUTATORS + r"\s*\("
COLL = r"(?:List|IList|ICollection|Collection|HashSet|Dictionary|IDictionary)"
CAST_C = r"\(\s*\(\s*" + COLL + r"\s*<[^>]*>\s*\)[^;]*?"
NAMED = r"\b(?:Refresh|RefreshPhysicalModel|\w+InPlace)\s*\([^;]*?"


def tok_re(t):
    return r"(?<![\w])" + re.escape(t).replace(r"\.", r"\s*\.\s*")


def rule1(text, tokens):
    hits = []
    for t in tokens:
        for pat in (tok_re(t) + LINK + "*" + ASSIGN, tok_re(t) + LINK + "*" + CALL):
            hits += [(t, m.group(0)[:140]) for m in re.finditer(pat, text)]
    return hits


def rule1b(text, tokens):
    hits = []
    for t in tokens:
        for pat in (CAST_C + tok_re(t),
                    tok_re(t) + r"[^;{}]*?\s+as\s+" + COLL + r"\s*<",
                    tok_re(t) + r"[^;{}]*?\s+is\s+" + COLL + r"\s*<[^>]*>\s+\w+"):
            hits += [(t, m.group(0)[:140]) for m in re.finditer(pat, text)]
    return hits


def rule1c(text, tokens):
    hits = []
    for t in tokens:
        hits += [(t, m.group(0)[:140]) for m in re.finditer(NAMED + tok_re(t), text)]
    return hits


def clean(src):
    src = re.sub(r'@"(?:[^"]|"")*"', '""', src)
    src = re.sub(r'\$?"(?:\\.|[^"\\\n])*"', '""', src)
    src = re.sub(r"'(?:\\.|[^'\\\n])'", "''", src)
    src = re.sub(r"/\*.*?\*/", " ", src, flags=re.S)
    src = re.sub(r"//[^\n]*", "", src)
    return src


def members(src):
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


def aliases(text, tokens):
    """Alias de un nivel, por forma: asignacion, foreach, is, lambda."""
    tokalt = "|".join(tok_re(t) for t in tokens)
    found = {}
    for m in re.finditer(r"(?:\bvar|[\w<>\[\]?,.]+)?\s+(\w+)\s*=(?![=>])\s*([^;{}]*)", text):
        ident, expr = m.group(1), m.group(2)
        if ident not in tokens and ident not in REBIND and re.search(tokalt, expr):
            found.setdefault(ident, "asignacion")
    for m in re.finditer(r"foreach\s*\(\s*(?:var|[\w<>\[\]?,.]+)\s+(\w+)\s+in\s+([^)]*(?:\([^()]*\)[^)]*)*)\)", text):
        if re.search(tokalt, m.group(2)):
            found.setdefault(m.group(1), "foreach")
    for m in re.finditer(r"(?:" + tokalt + r")[^;{}]*?\s+is\s+[\w.]+(?:\s*<[^>]*>)?\s+(\w+)", text):
        found.setdefault(m.group(1), "is")
    for m in re.finditer(r"(?:\(([^()]*)\)|\b(\w+))\s*=>", text):
        start = max(text.rfind(";", 0, m.start()), text.rfind("{", 0, m.start()), text.rfind("}", 0, m.start())) + 1
        if not re.search(tokalt, text[start:m.start()]):
            continue
        params = m.group(1).split(",") if m.group(1) is not None else [m.group(2)]
        for p in params:
            p = p.strip().split()[-1] if p.strip() else ""
            if re.fullmatch(r"\w+", p or "") and p not in tokens:
                found.setdefault(p, "lambda")
    return found


def check(text, tokens):
    flat = " ".join(clean(text).split())
    r = {"1": rule1(flat, tokens), "1b": rule1b(flat, tokens), "1c": rule1c(flat, tokens), "2": []}
    return r


def scan(files, label):
    print("==", label, "archivos", len(files))
    tot = {"1": 0, "1b": 0, "1c": 0, "2": 0}
    kinds = {}
    nmem = 0
    for rel in files:
        path = os.path.join(root, rel)
        if not os.path.isfile(path):
            print("  NO EXISTE", rel)
            continue
        with open(path, encoding="utf-8-sig") as f:
            src = clean(f.read())
        flat = " ".join(src.split())
        for k, fn in (("1", rule1), ("1b", rule1b), ("1c", rule1c)):
            h = fn(flat, TOKENS)
            tot[k] += len(h)
            for x in h:
                print("  R" + k, rel, x)
        for mem in members(src):
            nmem += 1
            al = aliases(mem, TOKENS)
            for a, kind in al.items():
                kinds[kind] = kinds.get(kind, 0) + 1
                print("   alias", rel.split("/")[-1], kind, a)
            if al:
                h = rule1(mem, list(al))
                tot["2"] += len(h)
                for x in h:
                    print("  R2", rel, al, x)
    print("  miembros=%d alias=%s" % (nmem, kinds))
    print("  TOTAL regla1=%d regla1b=%d regla1c=%d regla2=%d" % (tot["1"], tot["1b"], tot["1c"], tot["2"]))


scan(FILES_B, "CT-PBT1B-14 (Application, archivos que existen)")
scan(FILES_C, "CT-PBT1C-04 (W y WC)")

print("== autopruebas en negativo (cada una sola, dentro de un miembro neutro)")
SELF = [
    ("a", 'state.WorkingBaseline.Structure.Name = "x";'),
    ("b", "var b = state.WorkingBaseline; b.Structure.Modules.Clear();"),
    ("c", '((List<string>)side.LastModuleReconciliation.Preserved).Add("x");'),
    ("d", 'lastComputation.System.Name = "x";'),
    ("e", "foreach (var module in workingBaseline.Structure.Modules) { module.Length = 1.0; }"),
    ("f1", "state.WorkingBaseline.Structure.Modules[0].Length = 1.0;"),
    ("f2", "lastComputation.System.Structure.Modules[0] = null;"),
    ("f3", "workingBaseline.Structure.Fronts[0].Levels.Clear();"),
    ("g1", '(side.LastModuleReconciliation.Preserved as List<string>).Add("x");'),
    ("g2", 'if (side.LastModuleReconciliation.Preserved is List<string> l) { l.Add("x"); }'),
    ("h1", "state.WorkingBaseline.Structure.Modules.ToList().ForEach(m => m.Length = 1);"),
    ("h2", "state.WorkingBaseline.Structure.Modules.ToList().ForEach(m => { m.Length = 1; });"),
    ("i", "state.WorkingBaseline.Structure.RecalculatePositions();"),
    ("j", "builder.Refresh(state.WorkingBaseline.Structure);"),
    # formas que la guarda NO ve (limite declarado): alias de dos niveles y un argumento con parentesis anidados en la cadena
    ("L1-alias2", "var s = state.WorkingBaseline; var t = s.Structure; t.Name = \"x\";"),
    ("L2-anidado", "state.WorkingBaseline.Structure.Modules.Where(m => Ok(m)).First().Length = 1;"),
    ("L3-otro-metodo", "Touch(state.WorkingBaseline);"),
]
for k, text in SELF:
    body = "class K { void M() { " + text + " } }"
    src = clean("namespace N\n{\n    class K\n    {\n        void M()\n        {\n            " + text + "\n        }\n    }\n}\n")
    flat = " ".join(src.split())
    r1, r1b, r1c = rule1(flat, TOKENS), rule1b(flat, TOKENS), rule1c(flat, TOKENS)
    r2 = 0
    for mem in members(src):
        al = aliases(mem, TOKENS)
        if al:
            r2 += len(rule1(mem, list(al)))
    total = len(r1) + len(r1b) + len(r1c) + r2
    print("  %-14s regla1=%d regla1b=%d regla1c=%d regla2=%d -> %s" % (k, len(r1), len(r1b), len(r1c), r2, "SALTA" if total else "NO SALTA"))
