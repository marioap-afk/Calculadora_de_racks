# Solo lectura: las anclas de PB-T1c (D-PBT1-13, D-PBT1-14 y sec 6.3 del paquete v2.7) sobre 2f5d3dec, leidas SOLO por
# objetos git (show, diff, ls-tree, rev-parse) desde un checkout que comparte el almacen de objetos. No escribe en el
# repositorio; solo escribe, junto a este script, la simulacion mecanica de D-PBT1-13 (carpeta sim/) para medir sobre ella
# los invariantes de PB-T1c.
# Uso: python measure_pbt1c_anchors.py <checkout-que-comparte-el-almacen>
import os
import re
import subprocess
import sys

REPO = sys.argv[1]
REV = "2f5d3dec"
REMEASURED = "b5c09f0b"
HERE = os.path.dirname(os.path.abspath(__file__))
PB = "src/RackCad.UI/Systems/PushBack/"
W = PB + "RackPushBackSystemWindow.xaml.cs"
WC = PB + "RackPushBackSystemWindow.Composite.cs"
WX = PB + "RackPushBackSystemWindow.xaml"
APP = "src/RackCad.Application/Systems/PushBack/"
DA = APP + "PushBackEditorDesignAssembler.cs"
EST = APP + "PushBackEditableState.cs"
GATE = APP + "PushBackEditorOutputGate.cs"
CMP = APP + "PushBackEditableStateComparer.cs"


def git(*args):
    return subprocess.run(["git", "-C", REPO] + list(args), capture_output=True)


def show(path, rev=REV):
    r = git("show", rev + ":" + path)
    if r.returncode != 0:
        return None
    return r.stdout.decode("utf-8-sig").replace("\r\n", "\n")


def L(text):
    return text.split("\n")


SRC = {"W": show(W), "WC": show(WC), "WX": show(WX), "DA": show(DA)}
LINES = {k: L(v) for k, v in SRC.items()}


def line(f, n):
    return LINES[f][n - 1] if 0 < n <= len(LINES[f]) else "<fuera>"


def h(title):
    print()
    print("## " + title)


# ----------------------------------------------------------------------------------------------------------------- (a)
h("(a) src/RackCad.UI entre b5c09f0b y 2f5d3dec")
print("diff --quiet rc =", git("diff", "--quiet", REMEASURED, REV, "--", "src/RackCad.UI").returncode)
for rev in (REMEASURED, REV):
    print("tree", rev, "src/RackCad.UI =", git("rev-parse", rev + ":src/RackCad.UI").stdout.decode().strip())
print("diff --name-status src:")
print(git("diff", "--name-status", REMEASURED, REV, "--", "src").stdout.decode().rstrip())
print("lineas W/WC/WX =", len(LINES["W"]) - 1, len(LINES["WC"]) - 1, len(LINES["WX"]) - 1)

# ----------------------------------------------------------------------------------------------------------------- (b)
h("(b) anclas de D-PBT1-13, D-PBT1-14 y sec 4/6.3")
# (ref, lo que dice el paquete, [(archivo, linea, subcadena esperada), ...])
A = [
    ("W:56-57", "campo `composite` (D-13 (1))", [("W", 56, "private readonly PushBackCompositeEditorState composite ="),
                                                 ("W", 57, "new PushBackCompositeEditorState(new PushBackEditorState(), new PushBackEditorState());")]),
    ("W:84", "`safetySelections`", [("W", 84, "private readonly List<SelectiveSafetySelection> safetySelections")]),
    ("W:101", "`generalPallet`", [("W", 101, "private PalletSpecification generalPallet")]),
    ("W:106-109", "`rackTolerance`, `rackPostPeralte`, `rackBeamDepth`, `rackAnnotationScale`",
     [("W", 106, "private double rackTolerance"), ("W", 107, "private double rackPostPeralte"),
      ("W", 108, "private double rackBeamDepth"), ("W", 109, "private double rackAnnotationScale")]),
    ("W:114", "`advanced`", [("W", 114, "private readonly PushBackEditorInputs advanced")]),
    ("W:380", "`firstLevelDatum`", [("W", 380, "private int? firstLevelDatum")]),
    ("W:1249", "`loadedDimensionViews`", [("W", 1249, "private DimensionViewVisibility? loadedDimensionViews;")]),
    ("W:1256", "`loadedDimensionStyle`", [("W", 1256, "private string loadedDimensionStyle;")]),
    ("W:1260", "`dimensionViewsTouched`", [("W", 1260, "private bool dimensionViewsTouched;")]),
    ("W:273-276", "LoadNew: la mitad del editor (-> `LoadNewEditor`)",
     [("W", 273, "composite.SetActiveSide("), ("W", 274, "composite.SetSideBPresent(false);"),
      ("W", 275, "composite.LoadComposite(null);"), ("W", 276, "var inputs = state.LoadNew();")]),
    ("W:286-287", "LoadNew: la seguridad por defecto (-> `SeedDefaultSafety`)",
     [("W", 286, "safetySelections.Clear();"), ("W", 287, "safetySelections.AddRange(new PushBackSafetyAuthority(catalog).Defaults());")]),
    ("W:299-302", "LoadDesignForNew: la mitad del editor (-> `LoadDesignEditor` + `UpdateCompositeEnabled();`)",
     [("W", 299, "composite.SetActiveSide("), ("W", 300, "composite.SetSideBPresent(false);"),
      ("W", 301, "var inputs = state.LoadFromDesign(SideAOnly(design), assembler.Resolver);"), ("W", 302, "LoadCompositeFromDesign(design);")]),
    ("W:316-319", "LoadExisting: la mitad del editor",
     [("W", 316, "composite.SetActiveSide("), ("W", 317, "composite.SetSideBPresent(false);"),
      ("W", 318, "var inputs = state.LoadFromDesign(SideAOnly(design), assembler.Resolver);"), ("W", 319, "LoadCompositeFromDesign(design);")]),
    ("WC:933", "`UpdateCompositeEnabled();` al final de `LoadCompositeFromDesign`", [("WC", 933, "UpdateCompositeEnabled();"), ("WC", 934, "}")]),
    ("W:3320-3327", "Restore_Click: la mitad del editor (-> `RestoreEditor` + `UpdateCompositeEnabled();`)",
     [("W", 3320, "composite.SetActiveSide("), ("W", 3321, "composite.SetSideBPresent(false);"),
      ("W", 3322, "var inputs = state.LoadFromDesign(SideAOnly(design), assembler.Resolver);"),
      ("W", 3327, "LoadCompositeFromDesign(design, preserveDormantIntent: true);"), ("W", 3328, "LoadFromModel(inputs, NameBox.Text);")]),
    ("W:385-440", "`LoadFromModel` (HT1-07: try bajo suppressSync W:387-437, Recompute W:439)",
     [("W", 385, "private void LoadFromModel(PushBackEditorInputs inputs, string rackName)"), ("W", 387, "suppressSync = true;"),
      ("W", 388, "try"), ("W", 437, "}"), ("W", 436, "suppressSync = false;"), ("W", 439, "Recompute();")]),
    ("W:391", "portador `firstLevelDatum`", [("W", 391, "firstLevelDatum = inputs.FirstLevelDatum;")]),
    ("W:393", "portador `generalPallet`", [("W", 393, "generalPallet = pallet;")]),
    ("W:398", "portador `rackTolerance`", [("W", 398, "rackTolerance = inputs.PalletTolerance > 0.0")]),
    ("W:401", "portador `rackPostPeralte`", [("W", 401, "rackPostPeralte = inputs.PostPeralte;")]),
    ("W:403", "portador `rackBeamDepth`", [("W", 403, "rackBeamDepth = inputs.BeamDepth > 0.0")]),
    ("W:406-411", "los seis avanzados", [("W", 406, "advanced.ManualHeaderHeightOverride = inputs."), ("W", 407, "advanced.DerivedPostReinforced = inputs."),
                                         ("W", 408, "advanced.DerivedPostReinforcementHeight = inputs."), ("W", 409, "advanced.DerivedPostHeight = inputs."),
                                         ("W", 410, "advanced.SeparatorCountOverride = inputs."), ("W", 411, "advanced.SeparatorSpacingOverride = inputs.")]),
    ("W:414", "portador `loadedDimensionStyle`", [("W", 414, "loadedDimensionStyle = options.DimensionStyle;")]),
    ("W:419", "portador `rackAnnotationScale`", [("W", 419, "rackAnnotationScale = options.AnnotationScale > 0.0")]),
    ("W:423-427", "portador `safetySelections`", [("W", 423, "safetySelections.Clear();"), ("W", 424, "foreach (var safety in inputs.SafetySelections"),
                                                  ("W", 426, "if (safety != null) safetySelections.Add(safety.DeepCopy());"), ("W", 427, "}")]),
    ("W:390", "siembra NameBox", [("W", 390, "NameBox.Text = rackName ?? string.Empty;")]),
    ("W:392", "\"siembra\" (local `pallet`)", [("W", 392, "var pallet = inputs.Pallet ?? new PalletSpecification(")]),
    ("W:394-397", "siembras DepthBox, WeightUnitBox, PalletsDeepBox, ToleranceBox",
     [("W", 394, "DepthBox.SetNumber(pallet.Depth);"), ("W", 395, "WeightUnitBox.SelectedItem ="), ("W", 396, "PalletsDeepBox.SetNumber(Math.Max(2, inputs.PalletsDeep));"),
      ("W", 397, "ToleranceBox.SetNumber(")]),
    ("W:399-400", "siembras PostBox, PostPeralteBox", [("W", 399, "PostBox.SelectedId ="), ("W", 400, "PostPeralteBox.SetNumber(inputs.PostPeralte);")]),
    ("W:402", "siembra BeamDepthBox", [("W", 402, "BeamDepthBox.SetNumber(")]),
    ("W:413", "\"siembra\" (local `options`)", [("W", 413, "var options = inputs.Annotations ?? new DynamicAnnotationOptions();")]),
    ("W:415-418", "siembras de las tres casillas y AnnotationScaleBox",
     [("W", 415, "NumberFrontsCheck.IsChecked = options.NumberFronts;"), ("W", 416, "NumberLevelsCheck.IsChecked = options.NumberLevels;"),
      ("W", 417, "DrawRackNameCheck.IsChecked = options.DrawRackName;"), ("W", 418, "AnnotationScaleBox.SetNumber(")]),
    ("W:420-421", "siembras DimensionsBox y ShowDimensionViews", [("W", 420, "DimensionsBox.SelectedIndex ="), ("W", 421, "ShowDimensionViews(options.DimensionViews);")]),
    ("W:432", "`LoadAdvancedRackParameters` despues", [("W", 432, "LoadAdvancedRackParameters();")]),
    ("W:838-840", "la `PalletSpecification` de ReadInputs (5o argumento = unidad)",
     [("W", 838, "generalPallet = new PalletSpecification("), ("W", 839, "Keep(DepthBox, generalPallet.Depth)"), ("W", 840, "generalPallet.Weight, generalPallet.WeightUnit);")]),
    ("W:852", "la expresion de la unidad", [("W", 852, 'WeightUnitBox.SelectedItem as string ?? "kg"),')]),
    ("W:841-844", "las cuatro de Keep", [("W", 841, "rackTolerance = Keep(ToleranceBox, rackTolerance);"), ("W", 842, "rackPostPeralte = Keep(PostPeralteBox, rackPostPeralte);"),
                                         ("W", 843, "rackBeamDepth = Keep(BeamDepthBox, rackBeamDepth);"), ("W", 844, "rackAnnotationScale = Keep(AnnotationScaleBox, rackAnnotationScale);")]),
    ("W:853", "PalletsDeep", [("W", 853, "PalletsDeep = IntVal(PalletsDeepBox, DynamicRackDefaults.DefaultPalletsDeep),")]),
    ("W:868", "PostCatalogId", [("W", 868, "PostCatalogId = PostBox.SelectedId,")]),
    ("W:823, W:826", "el respaldo de las vigas (`?? cell.…`)", [("W", 823, "CellInOutBeamBox.SelectedValue as string ?? cell.InOutBeamCatalogId,"),
                                                             ("W", 826, "CellIntermediateBeamBox.SelectedValue as string ?? cell.IntermediateBeamCatalogId,")]),
    ("DA:169-171", "el respaldo del poste en el ensamblador", [("DA", 169, "var postId = string.IsNullOrWhiteSpace(editorInputs.PostCatalogId)"),
                                                             ("DA", 170, "? catalog.Defaults?.Post"), ("DA", 171, ": editorInputs.PostCatalogId;")]),
    ("W:874-876", "las tres casillas", [("W", 874, "NumberFronts = NumberFrontsCheck.IsChecked == true,"), ("W", 875, "NumberLevels = NumberLevelsCheck.IsChecked == true,"),
                                        ("W", 876, "DrawRackName = DrawRackNameCheck.IsChecked == true,")]),
    ("W:878", "Dimensions", [("W", 878, "Dimensions = (DimensionDetail)Math.Min((int)DimensionDetail.Detailed, Math.Max(0, DimensionsBox.SelectedIndex)),")]),
    ("W:1295-1297", "las tres de cotas", [("W", 1295, "DimensionsFrontalCheck.IsChecked == true,"), ("W", 1296, "DimensionsLateralCheck.IsChecked == true,"),
                                          ("W", 1297, "DimensionsPlantaCheck.IsChecked == true);")]),
    ("W:1291-1297", "`SelectedDimensionViews`", [("W", 1291, "private DimensionViewVisibility? SelectedDimensionViews()"), ("W", 1297, ");")]),
    ("W:880", "unico uso de `SelectedDimensionViews`", [("W", 880, "DimensionViews = SelectedDimensionViews()")]),
    ("W:342-373", "`SideAOnly`", [("W", 342, "private static PushBackDesign SideAOnly(PushBackDesign design)"), ("W", 373, "}")]),
    ("WC:813-814", "`LoadCompositeFromDesign(design)`", [("WC", 813, "private void LoadCompositeFromDesign(PushBackDesign design)"),
                                                       ("WC", 814, "=> LoadCompositeFromDesign(design, preserveDormantIntent: false);")]),
    ("WC:827-934", "`LoadCompositeFromDesign(design, preserve…)`", [("WC", 827, "private void LoadCompositeFromDesign(PushBackDesign design, bool preserveDormantIntent)"),
                                                                    ("WC", 934, "}")]),
    ("W:279", "Adopt (LoadNew)", [("W", 279, "session.Identity.Adopt(null, null);")]),
    ("W:305", "Adopt (LoadDesignForNew)", [("W", 305, "session.Identity.Adopt(null, rackName);")]),
    ("W:322", "Adopt (LoadExisting)", [("W", 322, "session.Identity.Adopt(rackId, rackName);")]),
    ("W:1237", "SetName (Name_Changed)", [("W", 1237, "session.Identity.SetName(NameBox.Text?.Trim());")]),
    ("W:3352", "SetName (InsertBatch_Click)", [("W", 3352, "session.Identity.SetName(NameBox.Text?.Trim());")]),
    ("W:3405", "SetName (RequestDraw)", [("W", 3405, "session.Identity.SetName(NameBox.Text?.Trim());")]),
    ("W:3504", "SetName (SaveLibrary_Click)", [("W", 3504, "session.Identity.SetName(NameBox.Text?.Trim());")]),
    ("W:131-161", "D-14: los constructores", [("W", 131, "public RackPushBackSystemWindow()"), ("W", 136, "public RackPushBackSystemWindow(bool canInsertInAutoCad, Func<string> newIdFactory = null)"),
                                              ("W", 160, "LoadNew();"), ("W", 161, "}")]),
    ("W:270, :294, :311", "D-14: las firmas publicas", [("W", 270, "public void LoadNew()"),
                                                        ("W", 294, "public void LoadDesignForNew(PushBackDesign design, string rackName, RackProject sourceProject = null)"),
                                                        ("W", 311, "public void LoadExisting(PushBackDesign design, string rackId, string rackName, RackProject sourceProject = null)")]),
    ("W:165-193, WC:43", "sec 4: las costuras internas", [("W", 165, "internal RackEditorSession<PushBackDesign, PushBackSystem> Session => session;"),
                                                          ("W", 193, "internal IReadOnlyList<SelectiveSafetySelection> SafetySelections => safetySelections;"),
                                                          ("WC", 43, "internal PushBackCompositeEditorState CompositeState => composite;")]),
]
ndiff = 0
for ref, claim, checks in A:
    ok = all(sub in line(f, n) for f, n, sub in checks)
    ndiff += 0 if ok else 1
    shown = " / ".join("%s:%d `%s`" % (f, n, line(f, n).strip()[:110]) for f, n, sub in checks)
    print("| %s | %s | %s | %s |" % (ref, claim, shown, "PASS" if ok else "DIFF"))
    if not ok:
        for f, n, sub in checks:
            if sub not in line(f, n):
                print("   DIFF en %s:%d: se esperaba `%s`" % (f, n, sub))
print("anclas=%d diff=%d" % (len(A), ndiff))

# Que la ventana no tenga otra linea en el try de LoadFromModel fuera de las dos listas de D-13 (4)
listed_carrier = [391, 393, 398, 401, 403] + list(range(406, 412)) + [414, 419] + list(range(423, 428))
listed_seed = [390, 392, 394, 395, 396, 397, 399, 400, 402, 413, 415, 416, 417, 418, 420, 421]
print("try de LoadFromModel (W:389-433), lineas que no estan en ninguna de las dos listas de D-13 (4):")
for n in range(390, 433):
    if n not in listed_carrier and n not in listed_seed:
        print("   W:%d `%s`" % (n, line("W", n).strip()))
print("lineas de la lista de portadores que estan tambien en la de siembras:", sorted(set(listed_carrier) & set(listed_seed)))

# ShowDimensionViews escribe portadores
print("escrituras de portadores FUERA de LoadFromModel y ReadInputs (siguen por el reenvio):")
carriers = r"\b(generalPallet|rackTolerance|rackPostPeralte|rackBeamDepth|rackAnnotationScale|firstLevelDatum|loadedDimensionViews|loadedDimensionStyle|dimensionViewsTouched)\s*=(?!=)"
for f in ("W", "WC"):
    for i, t in enumerate(LINES[f], start=1):
        if re.search(carriers, t) and not t.strip().startswith("private") and not (f == "W" and (385 <= i <= 440 or 833 <= i <= 890)):
            print("   %s:%d `%s`" % (f, i, t.strip()))

# ----------------------------------------------------------------------------------------------------------------- (c)
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


def doc_above(lines_, start):
    a = start - 1
    while a >= 1 and lines_[a - 1].strip().startswith(("///", "//", "[")):
        a -= 1
    return (a + 1, start - 1) if a + 1 <= start - 1 else None


MEM = {"W": members(LINES["W"]), "WC": members(LINES["WC"])}
h("(c) INV-PBT1C-03: los miembros de la lista, con su rango en 2f5d3dec (heuristica de members.py)")
print("miembros W=%d WC=%d" % (len(MEM["W"]), len(MEM["WC"])))
LIST03 = [("W", n) for n in ("composite", "safetySelections", "generalPallet", "rackTolerance", "rackPostPeralte", "rackBeamDepth",
                             "rackAnnotationScale", "advanced", "firstLevelDatum", "loadedDimensionViews", "loadedDimensionStyle",
                             "dimensionViewsTouched", "LoadNew", "LoadDesignForNew", "LoadExisting", "SideAOnly", "LoadFromModel",
                             "ReadInputs", "Name_Changed", "SelectedDimensionViews", "Restore_Click", "InsertBatch_Click", "RequestDraw",
                             "SaveLibrary_Click")] + \
         [("WC", n) for n in ("LoadCompositeFromDesign", "CompositeDiagnostics", "CompositeStatusOr", "CompositeHasBlocking")]
for f, name in LIST03:
    hits = [(a, b) for (nm, a, b) in MEM[f] if nm == name]
    other = [(a, b) for (nm, a, b) in MEM["WC" if f == "W" else "W"] if nm == name]
    docs = [doc_above(LINES[f], a) for a, b in hits]
    print("| %s | %s | %s | %s |" % (name, f, ", ".join("%d-%d" % x for x in hits) or "NO EXISTE",
                                    ", ".join(("%d-%d" % d) if d else "-" for d in docs)) + (" (tambien en el otro: %s)" % other if other else ""))
for new in ("SyncName", "EditableStateForTest", "editable"):
    print("declaracion nueva %s: W=%s WC=%s" % (new, [x for x in MEM["W"] if x[0] == new], [x for x in MEM["WC"] if x[0] == new]))

# ----------------------------------------------------------------------------------------------------------------- (d)


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


def collapse(t):
    return re.sub(r"\s+", " ", t).strip()


def normalize(raw, xaml):
    lf = raw.replace("\r\n", "\n").replace("\r", "\n")
    return collapse(re.sub(r"<!--.*?-->", " ", lf, flags=re.S) if xaml else strip_cs_comments(lf))


def classic_sources(override=None):
    names = git("ls-tree", "-r", "--name-only", REV, "--", PB).stdout.decode().split("\n")
    names = sorted(n for n in names if n)
    texts = []
    for ext, xaml in ((".cs", False), (".xaml", True)):
        for n in names:
            if n.endswith(ext):
                raw = (override or {}).get(n) or show(n)
                texts.append(normalize(raw, xaml))
    return "\n".join(texts)


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
        "session.RequestInsertViews(views", "Close();"]
CT23_HAS = ["RackViewAddress.PushBackCut(RackCad.Application.Systems.Shared.RackPushBackEnd.EntradaSalida, RackCad.Application.Systems.Shared.RackPushBackSide.A)",
            "RackViewAddress.PushBackCut(RackCad.Application.Systems.Shared.RackPushBackEnd.Posterior, RackCad.Application.Systems.Shared.RackPushBackSide.B)",
            "RackViewAddress.Whole(DimensionViewKind.Planta)", "RackViewAddress.Post(cut.PostIndex)",
            "RackViewCodec.Encode(RackSystemKind.PushBack, ctx.Views[0])"]


def tsg(source):
    res = {}
    b = body(source, "private void SaveLibrary_Click(")
    idx = [b.find(x) for x in CT22]
    res["CT-22"] = all(i >= 0 for i in idx) and idx == sorted(idx) and len(set(idx)) == 5
    res["S-9"] = S9 in source
    res["S-7"] = S7 in source
    b = body(source, "private void InsertBatch_Click(")
    confirm, dialog, request = (b.find(x) for x in CT23[:3])
    close = b.rfind("Close();")
    res["CT-23"] = dialog >= 0 and request > dialog and close > request and 0 <= confirm < dialog \
        and all(x in b for x in CT23_HAS) and "canInsertInAutoCad" not in b
    res["CT-19"] = not any(t in source for t in ("KeyDown", "PreviewKeyDown", "InputBindings", "KeyBinding", "TabIndex", "IsTabStop",
                                                 "KeyboardNavigation", ".Focus(", "DialogResult"))
    return res


h("(d) INV-PBT1C-04: los literales de T-SG en W")
for label, lits in (("CT-22", CT22), ("S-9", [S9]), ("S-7", [S7]), ("CT-23", CT23 + CT23_HAS)):
    for lit in lits:
        hits = [i for i, t in enumerate(LINES["W"], start=1) if lit in t]
        hitsc = [i for i, t in enumerate(LINES["WC"], start=1) if lit in t]
        print("| %s | `%s` | W:%s | WC:%s |" % (label, lit[:100], hits, hitsc))
src_now = classic_sources()
print("T-SG sobre 2f5d3dec (texto normalizado de Systems/PushBack):", tsg(src_now))
print("orden CT-22 en W (lineas):", [min(i for i, t in enumerate(LINES["W"], start=1) if x in t and 3481 <= i <= 3515) for x in CT22])
print("orden CT-23 en W (lineas):", [max(i for i, t in enumerate(LINES["W"], start=1) if x in t and 3334 <= i <= 3361) for x in CT23])
setname = [(f, i, t.strip()) for f in ("W", "WC") for i, t in enumerate(LINES[f], start=1) if re.search(r"\.(SetName|Adopt)\s*\(", t)]
print("sitios SetName/Adopt en W y WC:", [(f, i) for f, i, _ in setname])
for f, i, t in setname:
    mem = [nm for nm, a, b in MEM[f] if a <= i <= b]
    print("   %s:%d %s `%s`" % (f, i, mem, t))
allpb = git("grep", "-n", "-E", r"Identity\.(SetName|Adopt|EnsureId)\s*\(", REV, "--", "src").stdout.decode().strip()
print("git grep Identity.(SetName|Adopt|EnsureId)( en src:")
print("   " + allpb.replace("\n", "\n   "))

# ----------------------------------------------------------------------------------------------------------------- (e)
h("(e) INV-PBT1C-05: cuentas y literales")
g10 = r"(?<!IWorkspacePilotEditPort)\.RequestUpdate\s*\("
print(".RequestUpdate( en W: crudo=%d sin-comentarios=%d lineas=%s" % (
    len(re.findall(g10, SRC["W"])), len(re.findall(g10, strip_cs_comments(SRC["W"]))),
    [i for i, t in enumerate(LINES["W"], start=1) if re.search(g10, t)]))
print(".RequestUpdate( en WC:", len(re.findall(g10, SRC["WC"])))
for label, lit, f in (("I-55", "PhysicalPostIndex(LateralSectionBox.SelectedItem)", "W"), ("I-55", "corte.PostIndex == section", "W"),
                      ("I-55 (ausente)", "Math.Max(0, LateralSectionBox.SelectedIndex)", "W"),
                      ("Fact3", "RequestStandardRestore()", "W"), ("Fact3 (ausente)", "forceRebuild:", "W"), ("Fact3 (ausente)", "forceRebuild)", "W"),
                      ("Fact3 (prosa)", "forceRebuild", "W"), ("Fact3", "StandardRestoreRequested", "DA"), ("Fact3", "RestoreAllModulesButton", "WX"),
                      ("Desviador", "desviadorLevelsPerPost: DesviadorLevelsPerPost()", "W"), ("Desviador", "showDesviadorSide: false", "W")):
    hits = [i for i, t in enumerate(LINES[f], start=1) if lit in t]
    print("| %s | `%s` | %s:%s |" % (label, lit, f, hits))
for f in ("W", "WC"):
    sn = [i for i, t in enumerate(LINES[f], start=1) if "SetNumber(" in t]
    print("SetNumber( en %s: %d lineas %s" % (f, len(sn), sn))
REMOVED_W = listed_carrier + list(range(342, 374)) + list(range(1291, 1298)) + list(range(846, 890)) + [273, 274, 275, 276, 286, 287, 299, 300, 301, 302, 316, 317, 318, 319] + list(range(3320, 3328))
REMOVED_WC = list(range(813, 815)) + list(range(827, 935)) + list(range(784, 787)) + list(range(790, 801)) + [806]
print("lineas que D-13 quita o sustituye y contienen SetNumber(: W=%s WC=%s" % (
    [n for n in REMOVED_W if "SetNumber(" in line("W", n)], [n for n in REMOVED_WC if "SetNumber(" in line("WC", n)]))
print("siembras de LoadFromModel (D-13 (4)):")
for n in listed_seed:
    print("   W:%d `%s`" % (n, line("W", n).strip()))

# ----------------------------------------------------------------------------------------------------------------- (f)
h("(f) INV-PBT1C-06: las costuras internas")
SEAM = re.compile(r"^\s+internal\s+(?:sealed\s+|static\s+)*(.+?)\s+(\w+)\s*(\(|\{|=>|;|$)")


def seams(lines_):
    out = []
    for i, t in enumerate(lines_, start=1):
        if re.match(r"^\s+internal ", t):
            m = SEAM.match(t)
            out.append((i, m.group(2) if m else "?", (m.group(1) if m else t.strip())))
    return out


SW, SWC = seams(LINES["W"]), seams(LINES["WC"])
for i, name, typ in SW:
    print("| W:%d | %s | %s |" % (i, name, typ))
for i, name, typ in SWC:
    print("| WC:%d | %s | %s |" % (i, name, typ))
oldW, oldWC = seams(L(show(W, REMEASURED))), seams(L(show(WC, REMEASURED)))
print("costuras: W=%d WC=%d total=%d; iguales a b5c09f0b: %s" % (len(SW), len(SWC), len(SW) + len(SWC), (SW, SWC) == (oldW, oldWC)))
print("EditableStateForTest en src/ y tests/:", git("grep", "-n", "-F", "EditableStateForTest", REV, "--", "src", "tests").stdout.decode().strip() or "ninguna")

# ----------------------------------------------------------------------------------------------------------------- (g)
h("(g) la API de Application que usa D-PBT1-13")
for path in (EST, GATE, CMP):
    t = show(path)
    print("== " + path.split("/")[-1])
    for i, ln in enumerate(L(t), start=1):
        if re.match(r"^\s{4,8}public\s", ln):
            print("   :%d %s" % (i, ln.strip()))
