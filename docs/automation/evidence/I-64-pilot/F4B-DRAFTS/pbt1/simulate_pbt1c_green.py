# Solo lectura del repositorio: aplica MECANICAMENTE D-PBT1-13 (paquete v2.7) a W y WC de 2f5d3dec, en memoria, y mide
# sobre el resultado los invariantes de PB-T1c (INV-PBT1C-03 a 06 y 07c), CT-PBT1C-03 y la guarda G-L1 (CT-PBT1C-04), y
# G-L1 sobre los archivos reales de CT-PBT1B-14 en 2f5d3dec. Es una LECTURA del diseno, no el producto: no compila nada.
# Dos variantes de la retirada de SideAOnly, SelectedDimensionViews y los dos LoadCompositeFromDesign:
#   A = se retira tambien su comentario de documentacion (lo natural);  B = solo el rango del miembro (el comentario queda).
# Escribe solo en sim/ junto a este script. Lee el repositorio solo con git show / ls-tree.
# Uso: python simulate_pbt1c_green.py <checkout-que-comparte-el-almacen>
import difflib
import os
import re
import subprocess
import sys

REPO = sys.argv[1]
REV = "2f5d3dec"
HERE = os.path.dirname(os.path.abspath(__file__))
PB = "src/RackCad.UI/Systems/PushBack/"
W = PB + "RackPushBackSystemWindow.xaml.cs"
WC = PB + "RackPushBackSystemWindow.Composite.cs"


def git(*args):
    return subprocess.run(["git", "-C", REPO] + list(args), capture_output=True)


def show(path):
    r = git("show", REV + ":" + path)
    return None if r.returncode else r.stdout.decode("utf-8-sig").replace("\r\n", "\n")


BASE = {"W": show(W).split("\n"), "WC": show(WC).split("\n")}

I8, I12, I16 = " " * 8, " " * 12, " " * 16


def fwd(t, name, prop):
    return I8 + "private %s %s { get => editable.%s; set => editable.%s = value; }" % (t, name, prop, prop)


def edits_w(variant):
    e = [
        ("rep", 56, 57, [I8 + "private readonly PushBackEditableState editable = new PushBackEditableState();",
                         I8 + "private PushBackCompositeEditorState composite => editable.Editor;"]),
        ("rep", 84, 84, [I8 + "private List<SelectiveSafetySelection> safetySelections => editable.SafetySelections;"]),
        ("rep", 101, 101, [fwd("PalletSpecification", "generalPallet", "Pallet")]),
        ("rep", 106, 109, [fwd("double", "rackTolerance", "PalletTolerance"), fwd("double", "rackPostPeralte", "PostPeralte"),
                           fwd("double", "rackBeamDepth", "BeamDepth"), fwd("double", "rackAnnotationScale", "AnnotationScale")]),
        ("rep", 114, 114, [I8 + "private PushBackEditorInputs advanced => editable.Advanced;"]),
        ("ins", 193, None, [I8 + "internal PushBackEditableState EditableStateForTest => editable;"]),
        ("rep", 273, 276, [I12 + "var inputs = editable.LoadNewEditor();"]),
        ("ins", 279, None, [I12 + "SyncName();"]),
        ("rep", 286, 287, [I12 + "editable.SeedDefaultSafety(catalog);"]),
        ("rep", 299, 302, [I12 + "var inputs = editable.LoadDesignEditor(design, assembler.Resolver);", I12 + "UpdateCompositeEnabled();"]),
        ("ins", 305, None, [I12 + "SyncName();"]),
        ("rep", 316, 319, [I12 + "var inputs = editable.LoadDesignEditor(design, assembler.Resolver);", I12 + "UpdateCompositeEnabled();"]),
        ("ins", 322, None, [I12 + "SyncName();"]),
        ("rep", 325, 373, []) if variant == "A" else ("rep", 342, 373, []),
        ("rep", 380, 380, [fwd("int?", "firstLevelDatum", "FirstLevelDatum")]),
        ("ins", 389, None, [I16 + "editable.AdoptCarriers(inputs, catalog);"]),
        ("rep", 391, 391, []), ("rep", 393, 393, []), ("rep", 398, 398, []), ("rep", 401, 401, []), ("rep", 403, 403, []),
        ("rep", 406, 411, []), ("rep", 414, 414, []), ("rep", 419, 419, []), ("rep", 423, 427, []),
        ("rep", 840, 840, [I16 + 'generalPallet.Weight, WeightUnitBox.SelectedItem as string ?? "kg");']),
        ("rep", 845, 889, [I12 + "editable.PalletsDeep = IntVal(PalletsDeepBox, DynamicRackDefaults.DefaultPalletsDeep);",
                           I12 + "editable.PostCatalogId = PostBox.SelectedId ?? editable.PostCatalogId;",
                           I12 + "editable.NumberFronts = NumberFrontsCheck.IsChecked == true;",
                           I12 + "editable.NumberLevels = NumberLevelsCheck.IsChecked == true;",
                           I12 + "editable.DrawRackName = DrawRackNameCheck.IsChecked == true;",
                           I12 + "editable.Dimensions = (DimensionDetail)Math.Min((int)DimensionDetail.Detailed, Math.Max(0, DimensionsBox.SelectedIndex));",
                           I12 + "editable.DimensionsFrontal = DimensionsFrontalCheck.IsChecked == true;",
                           I12 + "editable.DimensionsLateral = DimensionsLateralCheck.IsChecked == true;",
                           I12 + "editable.DimensionsPlanta = DimensionsPlantaCheck.IsChecked == true;",
                           I12 + "return editable.BuildInputs(assembler);"]),
        ("ins", 1237, None, [I12 + "SyncName();"]),
        ("ins", 1238, None, ["", I8 + "private void SyncName() => editable.Name = session.Identity.Name;"]),
        ("rep", 1249, 1249, [fwd("DimensionViewVisibility?", "loadedDimensionViews", "LoadedDimensionViews")]),
        ("rep", 1256, 1256, [fwd("string", "loadedDimensionStyle", "DimensionStyle")]),
        ("rep", 1260, 1260, [fwd("bool", "dimensionViewsTouched", "DimensionViewsTouched")]),
        ("rep", 1288, 1297, []) if variant == "A" else ("rep", 1291, 1297, []),
        ("rep", 3320, 3327, [I12 + "var inputs = editable.RestoreEditor(design, assembler.Resolver);", I12 + "UpdateCompositeEnabled();"]),
        ("ins", 3352, None, [I12 + "SyncName();"]),
        ("ins", 3405, None, [I12 + "SyncName();"]),
        ("ins", 3504, None, [I16 + "SyncName();"]),
    ]
    return e


def edits_wc(variant):
    e = [
        ("rep", 784, 786, [I12 + "=> PushBackEditorOutputGate.Diagnostics(composite, lastCompositeSystem);"]),
        ("rep", 790, 801, [I12 + "=> PushBackEditorOutputGate.StatusOr(composite, lastCompositeSystem, fallback);"]),
        ("rep", 806, 806, [I12 + "return PushBackEditorOutputGate.IsBlocked(composite, system);"]),
    ]
    e += [("rep", 811, 934, [])] if variant == "A" else [("rep", 813, 814, []), ("rep", 827, 934, [])]
    return e


def apply(base, edits):
    out = list(base)
    for kind, a, b, new in sorted(edits, key=lambda x: -x[1]):
        if kind == "rep":
            out[a - 1:b] = new
        else:
            out[a:a] = new
    return out


# ---- members (heuristica de members.py) ------------------------------------------------------------------------------
DECL = re.compile(
    r"^\s{4,8}(?:\[[^\]]*\]\s*)*"
    r"((?:public|private|internal|protected|static|readonly|override|virtual|sealed|async|partial|new|const|event)\s+)+"
    r"[\w<>\[\]\?,\.\s]+?\b(\w+)\s*(\(|\{|=|;|=>)")


def members(lines_):
    depth, results, current = 0, [], None
    for i, raw in enumerate(lines_, start=1):
        stripped = raw.strip()
        if depth == 2 and current is None:
            m = DECL.match(raw)
            if m and not stripped.startswith("//"):
                current = [m.group(2), i, i]
                if stripped.endswith(";") and "{" not in stripped:
                    results.append(tuple(current))
                    current = None
        s = re.sub(r'"(?:\\.|[^"\\])*"', '""', raw)
        s = re.sub(r"'(?:\\.|[^'\\])'", "''", s)
        s = s.split("//")[0]
        depth += s.count("{") - s.count("}")
        if current is not None:
            current[2] = i
            if depth == 2 and (stripped.endswith("}") or stripped.endswith(";")):
                results.append(tuple(current))
                current = None
    return results


LIST = {"W": {"composite", "safetySelections", "generalPallet", "rackTolerance", "rackPostPeralte", "rackBeamDepth", "rackAnnotationScale",
              "advanced", "firstLevelDatum", "loadedDimensionViews", "loadedDimensionStyle", "dimensionViewsTouched", "LoadNew",
              "LoadDesignForNew", "LoadExisting", "SideAOnly", "LoadFromModel", "ReadInputs", "Name_Changed", "SelectedDimensionViews",
              "Restore_Click", "InsertBatch_Click", "RequestDraw", "SaveLibrary_Click"},
        "WC": {"LoadCompositeFromDesign", "CompositeDiagnostics", "CompositeStatusOr", "CompositeHasBlocking"}}
NEW_DECL = re.compile(r"\b(SyncName\s*\(\s*\)\s*=>|EditableStateForTest\s*=>)")


def hunks(base, new):
    sm = difflib.SequenceMatcher(None, base, new, autojunk=False)
    return [(tag, i1 + 1, i2, new[j1:j2], base[i1:i2]) for tag, i1, i2, j1, j2 in sm.get_opcodes() if tag != "equal"]


def check03(f, hs, ranges, label):
    bad = []
    for tag, a1, a2, added, removed in hs:
        if tag == "insert":
            a = a1 - 1  # insercion despues de la linea a de la base
            inside = [nm for nm, s, e in ranges if s <= a < e]
            if inside:
                ok = all(nm in LIST[f] for nm in inside)
                where = inside
            else:
                ok = bool(NEW_DECL.search("\n".join(added)))
                where = ["entre miembros", "declaracion nueva" if ok else "NO es SyncName/EditableStateForTest"]
            if not ok:
                bad.append(("@@ -%d,0 +%d" % (a, len(added)), where))
        else:
            # cada linea cambiada o quitada tiene que estar en el rango de un miembro de la lista
            outside, foreign = [], set()
            for n in range(a1, a2 + 1):
                own = [nm for nm, s, e in ranges if s <= n <= e]
                if not own:
                    outside.append(n)
                foreign |= {nm for nm in own if nm not in LIST[f]}
            if outside or foreign:
                spans, start = [], None
                for n in outside + [None]:
                    if start is None:
                        start = prev = n
                    elif n is not None and n == prev + 1:
                        prev = n
                    else:
                        spans.append("%d-%d" % (start, prev) if start != prev else str(start))
                        start = prev = n
                bad.append(("@@ -%d,%d" % (a1, a2 - a1 + 1), {"fuera de todo miembro": [s for s in spans if s != "None"],
                                                                "miembros fuera de la lista": sorted(foreign)}))
    print("  INV-PBT1C-03 %s %s: hunks=%d fuera=%d %s" % (f, label, len(hs), len(bad), bad))
    return bad


# ---- normalizacion de T-SG y G-L1 ------------------------------------------------------------------------------------


def strip_cs_comments(text):
    out, i, n = [], 0, len(text)
    while i < n:
        c = text[i]
        nx = text[i + 1] if i + 1 < n else "\0"
        if c == "/" and nx == "/":
            while i < n and text[i] != "\n":
                i += 1
            continue
        if c == "/" and nx == "*":
            i += 2
            while i + 1 < n and not (text[i] == "*" and text[i + 1] == "/"):
                i += 1
            i += 2
            out.append(" ")
            continue
        if c == "@" and nx == '"':
            out.append('@"')
            i += 2
            while i < n:
                if text[i] == '"':
                    if i + 1 < n and text[i + 1] == '"':
                        out.append('""')
                        i += 2
                        continue
                    out.append('"')
                    i += 1
                    break
                out.append(text[i])
                i += 1
            continue
        if c in "\"'":
            out.append(c)
            i += 1
            while i < n:
                if text[i] == "\\" and i + 1 < n:
                    out.append(text[i] + text[i + 1])
                    i += 2
                    continue
                if text[i] == c:
                    out.append(c)
                    i += 1
                    break
                out.append(text[i])
                i += 1
            continue
        out.append(c)
        i += 1
    return "".join(out)


def normalize(raw, xaml):
    return re.sub(r"\s+", " ", re.sub(r"<!--.*?-->", " ", raw, flags=re.S) if xaml else strip_cs_comments(raw)).strip()


def classic(override):
    names = sorted(n for n in git("ls-tree", "-r", "--name-only", REV, "--", PB).stdout.decode().split("\n") if n)
    parts = [normalize(override.get(n) or show(n), False) for n in names if n.endswith(".cs")]
    parts += [normalize(show(n), True) for n in names if n.endswith(".xaml")]
    return "\n".join(parts)


def body(source, marker):
    at = source.find(marker)
    if at < 0:
        return None
    o = source.find("{", at)
    d = 0
    for i in range(o, len(source)):
        if source[i] == "{":
            d += 1
        elif source[i] == "}":
            d -= 1
            if d == 0:
                return source[o:i + 1]
    return None


CT22 = ["UiSupport.PromptSaveToLibrary(this,", "session.Identity.EnsureId();", "session.Identity.SetName(NameBox.Text?.Trim());",
        "var project = RackProject.ForPushBack(lastComputation.Design).WithSourceMetadataFrom(sourceProject);",
        "new RackProjectStore().Save(project, path);"]
S9 = 'UiSupport.PromptSaveToLibrary(this, NameBox.Text, "sistema")'
S7 = "new RackBomWindow(lastComputation.Bom) { Owner = this }.ShowDialog();"
CT23 = ["if (moduleOwner.ModuleSession.HasPendingChanges) moduleOwner.CommitModuleEdits();",
        "Views.RackViewBatchDialogPresenter.TryShow(this, RackSystemKind.PushBack, options, out var views)",
        "session.RequestInsertViews(views"]
CT23_HAS = ["RackViewAddress.Whole(DimensionViewKind.Planta)", "RackViewAddress.Post(cut.PostIndex)",
            "RackViewCodec.Encode(RackSystemKind.PushBack, ctx.Views[0])"]
CT19 = ("KeyDown", "PreviewKeyDown", "InputBindings", "KeyBinding", "TabIndex", "IsTabStop", "KeyboardNavigation", ".Focus(", "DialogResult")


def tsg(src):
    b = body(src, "private void SaveLibrary_Click(")
    idx = [b.find(x) for x in CT22]
    r = {"CT-22": all(i >= 0 for i in idx) and idx == sorted(idx), "S-9": S9 in src, "S-7": S7 in src}
    b = body(src, "private void InsertBatch_Click(")
    c, d, q = (b.find(x) for x in CT23)
    r["CT-23"] = d >= 0 and q > d and b.rfind("Close();") > q and 0 <= c < d and all(x in b for x in CT23_HAS) and "canInsertInAutoCad" not in b
    r["CT-19"] = not any(t in src for t in CT19)
    return r


TOKENS = ["WorkingBaseline", "workingBaseline", "LastModuleReconciliation", "computation.System", "lastComputation.System",
          "session.System", "lastCompositeSystem", "SystemToInsert"]
REBIND = {"lastComputation", "lastCompositeSystem", "workingBaseline", "LastModuleReconciliation"}
LINK = r"(?:\s*\??\.\s*\w+(?:\s*\([^()]*\))?|\s*\[[^\]]*\])"
ASSIGN = r"\s*(?:\??\.\s*\w+|\[[^\]]*\])\s*(?:=(?![=>])|\+=|-=|\*=|/=|\+\+|--)"
CALL = r"\s*\??\.\s*(?:Add|AddRange|Clear|Remove|RemoveAt|RemoveAll|RemoveRange|Insert|InsertRange|Sort|Reverse|RecalculatePositions)\s*\("
COLL = r"(?:List|IList|ICollection|Collection|HashSet|Dictionary|IDictionary)"


def tok(t):
    return r"(?<![\w])" + re.escape(t).replace(r"\.", r"\s*\.\s*")


def r1(text, toks):
    return [m.group(0)[:120] for t in toks for p in (tok(t) + LINK + "*" + ASSIGN, tok(t) + LINK + "*" + CALL) for m in re.finditer(p, text)]


def r1b(text, toks):
    pats = lambda t: (r"\(\s*\(\s*" + COLL + r"\s*<[^>]*>\s*\)[^;]*?" + tok(t), tok(t) + r"[^;{}]*?\s+as\s+" + COLL + r"\s*<",
                      tok(t) + r"[^;{}]*?\s+is\s+" + COLL + r"\s*<[^>]*>\s+\w+")
    return [m.group(0)[:120] for t in toks for p in pats(t) for m in re.finditer(p, text)]


def r1c(text, toks):
    return [m.group(0)[:120] for t in toks for m in re.finditer(r"\b(?:Refresh|RefreshPhysicalModel|\w+InPlace)\s*\([^;]*?" + tok(t), text)]


def clean(src):
    src = re.sub(r'@"(?:[^"]|"")*"', '""', src)
    src = re.sub(r'\$?"(?:\\.|[^"\\\n])*"', '""', src)
    src = re.sub(r"'(?:\\.|[^'\\\n])'", "''", src)
    src = re.sub(r"/\*.*?\*/", " ", src, flags=re.S)
    return re.sub(r"//[^\n]*", "", src)


def gl1_members(src):
    out, depth, cur = [], 0, []
    for ln in src.split("\n"):
        before = depth
        depth += ln.count("{") - ln.count("}")
        if before >= 2 or depth > 2:
            cur.append(ln)
        if before > 2 and depth == 2 or (before == 2 and depth == 2 and ln.strip().endswith(";")):
            out.append(" ".join(" ".join(cur).split()))
            cur = []
    if cur:
        out.append(" ".join(" ".join(cur).split()))
    return out


def aliases(text):
    alt = "|".join(tok(t) for t in TOKENS)
    found = {}
    for m in re.finditer(r"(?:\bvar|[\w<>\[\]?,.]+)?\s+(\w+)\s*=(?![=>])\s*([^;{}]*)", text):
        if m.group(1) not in TOKENS and m.group(1) not in REBIND and re.search(alt, m.group(2)):
            found.setdefault(m.group(1), "asignacion")
    for m in re.finditer(r"foreach\s*\(\s*(?:var|[\w<>\[\]?,.]+)\s+(\w+)\s+in\s+([^)]*(?:\([^()]*\)[^)]*)*)\)", text):
        if re.search(alt, m.group(2)):
            found.setdefault(m.group(1), "foreach")
    for m in re.finditer(r"(?:" + alt + r")[^;{}]*?\s+is\s+[\w.]+(?:\s*<[^>]*>)?\s+(\w+)", text):
        found.setdefault(m.group(1), "is")
    for m in re.finditer(r"(?:\(([^()]*)\)|\b(\w+))\s*=>", text):
        start = max(text.rfind(";", 0, m.start()), text.rfind("{", 0, m.start()), text.rfind("}", 0, m.start())) + 1
        if not re.search(alt, text[start:m.start()]):
            continue
        for p in (m.group(1).split(",") if m.group(1) is not None else [m.group(2)]):
            p = p.strip().split()[-1] if p.strip() else ""
            if re.fullmatch(r"\w+", p or "") and p not in TOKENS:
                found.setdefault(p, "lambda")
    return found


def gl1(text):
    src = clean(text)
    flat = " ".join(src.split())
    tot = {"1": r1(flat, TOKENS), "1b": r1b(flat, TOKENS), "1c": r1c(flat, TOKENS), "2(1)": [], "2(1b)": []}
    al_all = {}
    for mem in gl1_members(src):
        al = aliases(mem)
        al_all.update(al)
        if al:
            tot["2(1)"] += r1(mem, list(al))
            tot["2(1b)"] += r1b(mem, list(al))  # I01 de PB-T1b: la regla 1b tambien sobre los alias
    return tot, al_all


def gl1_report(label, texts):
    agg = {"1": 0, "1b": 0, "1c": 0, "2(1)": 0, "2(1b)": 0}
    al = {}
    for t in texts:
        r, a = gl1(t)
        for k in agg:
            agg[k] += len(r[k])
            for x in r[k]:
                print("     coincidencia", k, x)
        al.update(a)
    nmem = sum(len(gl1_members(clean(t))) for t in texts)
    print("  G-L1 %s: miembros=%d %s alias=%s" % (label, nmem, agg, sorted(al.items())))
    return agg


def selftests():
    SELF = [("a", 'state.WorkingBaseline.Structure.Name = "x";'), ("b", "var b = state.WorkingBaseline; b.Structure.Modules.Clear();"),
            ("c", '((List<string>)side.LastModuleReconciliation.Preserved).Add("x");'), ("d", 'lastComputation.System.Name = "x";'),
            ("e", "foreach (var module in workingBaseline.Structure.Modules) { module.Length = 1.0; }"),
            ("f1", "state.WorkingBaseline.Structure.Modules[0].Length = 1.0;"), ("f2", "lastComputation.System.Structure.Modules[0] = null;"),
            ("f3", "workingBaseline.Structure.Fronts[0].Levels.Clear();"), ("g1", '(side.LastModuleReconciliation.Preserved as List<string>).Add("x");'),
            ("g2", 'if (side.LastModuleReconciliation.Preserved is List<string> l) { l.Add("x"); }'),
            ("h1", "state.WorkingBaseline.Structure.Modules.ToList().ForEach(m => m.Length = 1);"),
            ("h2", "state.WorkingBaseline.Structure.Modules.ToList().ForEach(m => { m.Length = 1; });"),
            ("i", "state.WorkingBaseline.Structure.RecalculatePositions();"), ("j", "builder.Refresh(state.WorkingBaseline.Structure);"),
            ("k-alias+molde (I01)", 'var r = side.LastModuleReconciliation; (r.Preserved as List<string>).Add("x");'),
            ("k2-alias+molde-C", 'var r = lastCompositeSystem; ((List<string>)r.Structure.Modules).Clear();')]
    for k, line in SELF:
        src = "namespace N\n{\n    class K\n    {\n        void M()\n        {\n            " + line + "\n        }\n    }\n}\n"
        r, _ = gl1(src)
        n = {x: len(v) for x, v in r.items()}
        sin_1b_alias = n["1"] + n["1b"] + n["1c"] + n["2(1)"]
        print("  %-22s %s -> %s; sin aplicar 1b a los alias: %s" % (k, n, "SALTA" if sum(n.values()) else "NO SALTA",
                                                                    "SALTA" if sin_1b_alias else "NO SALTA"))


# ---- las mediciones ----------------------------------------------------------------------------------------------------
os.makedirs(os.path.join(HERE, "sim"), exist_ok=True)
MEMB = {f: members(BASE[f]) for f in ("W", "WC")}

for variant in ("A", "B"):
    print("\n== variante %s (%s)" % (variant, "retira tambien los comentarios de documentacion" if variant == "A" else "solo el rango del miembro"))
    SIM = {"W": apply(BASE["W"], edits_w(variant)), "WC": apply(BASE["WC"], edits_wc(variant))}
    for f in ("W", "WC"):
        with open(os.path.join(HERE, "sim", "%s-%s.cs.txt" % (f, variant)), "w", encoding="utf-8", newline="\n") as fh:
            fh.write("\n".join(SIM[f]))
    HS = {f: hunks(BASE[f], SIM[f]) for f in ("W", "WC")}
    exp = {"W": len(edits_w(variant)), "WC": len(edits_wc(variant))}
    print("  hunks de difflib W=%d WC=%d (ediciones previstas W=%d WC=%d)" % (len(HS["W"]), len(HS["WC"]), exp["W"], exp["WC"]))
    for f in ("W", "WC"):
        check03(f, HS[f], MEMB[f], "con los rangos de members.py")
    # rangos corregidos a mano: generalPallet = W:101 (members.py la absorbe en lastComputation 92-101)
    fixed = [m for m in MEMB["W"] if m[0] != "lastComputation"] + [("lastComputation", 92, 92), ("generalPallet", 101, 101)]
    check03("W", HS["W"], fixed, "con generalPallet=W:101")
    docs = fixed + [("SideAOnly", 325, 341), ("SelectedDimensionViews", 1288, 1290)]
    check03("W", HS["W"], docs, "con generalPallet=W:101 y la documentacion de los retirados")
    check03("WC", HS["WC"], MEMB["WC"] + [("LoadCompositeFromDesign", 811, 812), ("LoadCompositeFromDesign", 815, 826)],
            "con la documentacion de los retirados")
    if variant != "A":
        continue
    sw, swc = "\n".join(SIM["W"]), "\n".join(SIM["WC"])
    added = [l for f in ("W", "WC") for h in HS[f] for l in h[3]]
    removed = [l for f in ("W", "WC") for h in HS[f] for l in h[4]]
    print("  lineas anadidas=%d quitadas=%d" % (len(added), len(removed)))
    # INV-PBT1C-04
    print("  T-SG sobre la simulacion:", tsg(classic({W: sw, WC: swc})))
    sl = SIM["W"]
    after = [(i + 1, sl[i + 1].strip()) for i, t in enumerate(sl) if re.search(r"\.(SetName|Adopt)\s*\(", t)]
    print("  linea siguiente a cada SetName/Adopt:", after)
    # INV-PBT1C-05
    g10 = r"(?<!IWorkspacePilotEditPort)\.RequestUpdate\s*\("
    print("  .RequestUpdate( en W:", len(re.findall(g10, sw)))
    bad = [t for t in CT19 + ("forceRebuild",) if any(t in l for l in added)]
    print("  tokens prohibidos en lineas anadidas:", bad)
    for lit in ("PhysicalPostIndex(LateralSectionBox.SelectedItem)", "corte.PostIndex == section", "RequestStandardRestore()",
                "desviadorLevelsPerPost: DesviadorLevelsPerPost()", "showDesviadorSide: false"):
        print("  literal %-52s en W: %s" % (lit, lit in sw))
    print("  forceRebuild: / forceRebuild) en W:", "forceRebuild:" in sw, "forceRebuild)" in sw)
    print("  SetNumber( en lineas anadidas=%d quitadas=%d" % (sum("SetNumber(" in l for l in added), sum("SetNumber(" in l for l in removed)))
    lfm = [m for m in members(sl) if m[0] == "LoadFromModel"][0]
    lfm_lines = sl[lfm[1] - 1:lfm[2]]
    seeds = [390, 392, 394, 395, 396, 397, 399, 400, 402, 413, 415, 416, 417, 418, 420, 421]
    seq = [BASE["W"][n - 1] for n in seeds]
    pos = [lfm_lines.index(s) if s in lfm_lines else -1 for s in seq]
    print("  siembras de D-13 (4) en LoadFromModel, identicas y en orden:", all(p >= 0 for p in pos) and pos == sorted(pos))
    t = lfm_lines.index(I12 + "try")
    print("  primera sentencia del try de LoadFromModel:", lfm_lines[t + 2].strip())
    for call in ("editable.LoadDesignEditor(", "editable.RestoreEditor("):
        print("  linea siguiente a %s %s" % (call, [sl[i + 1].strip() for i, l in enumerate(sl) if call in l]))
    # INV-PBT1C-06
    seam = lambda ls: [re.sub(r"\s+", " ", l.strip()) for l in ls if re.match(r"^\s+internal ", l)]
    sb = seam(BASE["W"]) + seam(BASE["WC"])
    ss = seam(SIM["W"]) + seam(SIM["WC"])
    print("  costuras base=%d sim=%d; nuevas: %s; perdidas: %s" % (len(sb), len(ss), [x for x in ss if x not in sb], [x for x in sb if x not in ss]))
    # CT-PBT1C-03 sobre la simulacion y sobre la base (RED)
    names2 = ["safetySelections", "generalPallet", "rackTolerance", "rackPostPeralte", "rackBeamDepth", "rackAnnotationScale",
              "advanced", "firstLevelDatum", "loadedDimensionViews", "loadedDimensionStyle", "dimensionViewsTouched"]
    for label, tw, twc in (("sim", sw, swc), ("base", "\n".join(BASE["W"]), "\n".join(BASE["WC"]))):
        cw, cwc = strip_cs_comments(tw), strip_cs_comments(twc)
        both = cw + "\n" + cwc
        fields = [n for n in names2 if re.search(r"\b(?:private|internal|protected|public)\s+(?:readonly\s+)?[\w<>\[\]?,. ]+?\s+" + n + r"\s*(?:=(?!>)|;)", both)]
        chb = body(re.sub(r"\s+", " ", cwc), "private bool CompositeHasBlocking(") or ""
        lfmb = body(re.sub(r"\s+", " ", cw), "private void LoadFromModel(") or ""
        tryb = body(lfmb, "try") or ""
        print("  CT-PBT1C-03 (%s): campos=%s SideAOnly=%s LoadCompositeFromDesign=%s ParkDormantTail(=%s IsBlocked=%s AdoptCarriers-primera=%s BuildInputs=%s costura=%s" % (
            label, fields, "PushBackDesign SideAOnly(" in both, "LoadCompositeFromDesign" in both, "ParkDormantTail(" in both,
            "PushBackEditorOutputGate.IsBlocked(" in chb, tryb.startswith("{ editable.AdoptCarriers(inputs, catalog);"),
            "editable.BuildInputs(assembler)" in cw, "EditableStateForTest => editable;" in cw))
    # INV-PBT1C-07c
    name_writes = [l.strip() for l in added if re.search(r"(lastComputation|session|lastCompositeSystem|WorkingBaseline|workingBaseline)\s*\.\s*(System\s*\.\s*)?Name\s*=", l)]
    print("  lineas anadidas que asignan Name de una referencia de D2:", name_writes)
    # G-L1 (CT-PBT1C-04) sobre la base y la simulacion
    gl1_report("CT-PBT1C-04 base 2f5d3dec (W, WC)", ["\n".join(BASE["W"]), "\n".join(BASE["WC"])])
    gl1_report("CT-PBT1C-04 simulacion A (W, WC)", [sw, swc])

print("\n== G-L1 de CT-PBT1B-14 sobre 2f5d3dec (los 4 archivos nuevos, ES, EL, CS, CA, DA, RMES, RackModuleReconciliation.cs y Workspace/)")
APP = "src/RackCad.Application/Systems/PushBack/"
files = [APP + n for n in ("PushBackEditableState.cs", "PushBackEditableStateComparer.cs", "PushBackEditableStateOwnership.cs",
                           "PushBackEditorOutputGate.cs", "PushBackEditorState.cs", "PushBackEditorState.Load.cs",
                           "PushBackCompositeEditorState.cs", "PushBackCompositeEditorAssembler.cs", "PushBackEditorDesignAssembler.cs")]
files += ["src/RackCad.Application/Systems/Shared/RackModuleEditSession.cs", "src/RackCad.Application/Systems/Shared/RackModuleReconciliation.cs"]
files += sorted(n for n in git("ls-tree", "-r", "--name-only", REV, "--", "src/RackCad.Application/Workspace").stdout.decode().split("\n") if n.endswith(".cs"))
texts = [show(p) for p in files]
print("  archivos=%d (existen %d)" % (len(files), sum(t is not None for t in texts)))
gl1_report("CT-PBT1B-14", [t for t in texts if t is not None])
print("\n== autopruebas en negativo (cada una sola en un miembro neutro), con la regla 1b tambien sobre los alias")
selftests()
