# -*- coding: utf-8 -*-
# I-64 F4B PB-T2: mediciones de solo lectura por objetos git (sin tocar el arbol de trabajo de nadie).
# Uso: python -I measure_pbt2.py <ruta-del-checkout-principal> <sha>
# Lee SOLO con `git -C <checkout> show|ls-tree|grep|diff|rev-parse <sha>...`. No escribe nada: imprime en stdout.
import re
import subprocess
import sys

REPO = sys.argv[1]
SHA = sys.argv[2]


def git(*args):
    out = subprocess.run(["git", "-C", REPO] + list(args), capture_output=True)
    return out.stdout.decode("utf-8", errors="replace")


def show(path):
    return git("show", SHA + ":" + path).replace("\r\n", "\n")


def lines_of(text, pattern, flags=0):
    rx = re.compile(pattern, flags)
    return [i + 1 for i, line in enumerate(text.split("\n")) if rx.search(line)]


def member_span(text, signature):
    """Linea de la firma y linea de la llave que cierra su cuerpo (por conteo de llaves, sin comentarios)."""
    src = text.split("\n")
    start = None
    for i, line in enumerate(src):
        if signature in line:
            start = i
            break
    if start is None:
        return None
    depth = 0
    opened = False
    for j in range(start, len(src)):
        code = re.sub(r"//.*$", "", src[j])
        code = re.sub(r'"(\\.|[^"\\])*"', '""', code)
        for ch in code:
            if ch == "{":
                depth += 1
                opened = True
            elif ch == "}":
                depth -= 1
        if opened and depth == 0:
            return (start + 1, j + 1)
        if not opened and code.rstrip().endswith(";") and "=>" in src[start]:
            return (start + 1, j + 1)
    return (start + 1, None)


def section(title):
    print()
    print("== " + title)


W = "src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs"
WC = "src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.Composite.cs"
WX = "src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml"
SES = "src/RackCad.UI/Editor/RackEditorSession.cs"
RID = "src/RackCad.UI/Editor/RackEditorIdentity.cs"
RPC = "src/RackCad.Plugin/RackPushBackCommands.cs"
RMC = "src/RackCad.Plugin/RackMenuCommands.cs"
EM = "src/RackCad.UI/Editor/EditorModules.cs"
PES = "src/RackCad.Application/Systems/PushBack/PushBackEditableState.cs"
PESC = "src/RackCad.Application/Systems/PushBack/PushBackEditableStateComparer.cs"
POG = "src/RackCad.Application/Systems/PushBack/PushBackEditorOutputGate.cs"
DS = "src/RackCad.Application/Workspace/Drafts/DraftSession.cs"
DR = "src/RackCad.Application/Workspace/Drafts/DraftRegistry.cs"
PFA = "src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotFieldAdapter.cs"
WHC = "src/RackCad.Application/Workspace/WorkspaceHostCore.cs"
NFV = "src/RackCad.UI/Controls/NumericFieldValidation.cs"
TSG = "tests/RackCad.UI.Tests/PushBackClassicCharacterizationSourceGuardTests.cs"
TPG = "tests/RackCad.Tests/PushBackClassicCharacterizationPluginGuardTests.cs"
G10 = "tests/RackCad.Tests/Workspace/WorkspacePilotUpdateIntegrationGuardTests.cs"
DBT = "tests/RackCad.Tests/Workspace/WorkspaceDraftBoundaryTests.cs"
DIG = "tests/RackCad.Tests/Workspace/WorkspaceDraftPilotIntentGuardTests.cs"
RPT = "tests/RackCad.UI.Tests/PushBackRichPreviewTests.cs"
WCG = "tests/RackCad.UI.Tests/WindowCensusGuardTests.cs"
LSG = "tests/RackCad.UI.Tests/PushBackLibrarySaveGateTests.cs"
DWC = "tests/RackCad.UI.Tests/DialogWindowCharacterizationTests.cs"
EDC = "tests/RackCad.UI.Tests/EditorDiscardPromptCollection.cs"

section("A. Base")
print("rev-parse", SHA, "=", git("rev-parse", SHA).strip())
print("rev-parse origin/main =", git("rev-parse", "origin/main").strip())
print("log -1:", git("log", "-1", "--format=%h %s", SHA).strip())
print("diff --stat b5c09f0b", SHA, "-- src:")
print(git("diff", "--stat", "b5c09f0b", SHA, "--", "src").rstrip())
for p in (W, WC, WX, SES, RID, RPC, RMC, EM):
    d = git("diff", "--name-only", "b5c09f0b", SHA, "--", p).strip()
    print("diff b5c09f0b..%s -- %s: %s" % (SHA, p, "vacio" if not d else d))
for p in (W, WC, WX):
    print("wc -l %s = %d" % (p, len(show(p).rstrip("\n").split("\n"))))

section("B. W: miembros (firma, cierre)")
w = show(W)
for sig in [
    "private readonly PushBackCompositeEditorState composite =",
    "private PushBackEditorState moduleOwner =>",
    "private readonly RackEditorSession<PushBackDesign, PushBackSystem> session;",
    "private readonly bool canInsertInAutoCad;",
    "private bool isEditingExisting;",
    "private RackProject sourceProject;",
    "public RackPushBackSystemWindow()",
    "public RackPushBackSystemWindow(bool canInsertInAutoCad, Func<string> newIdFactory = null)",
    "internal RackProject BuildLibraryProjectForTest()",
    "public bool InsertRequested =>",
    "public RackInsertionRequest InsertionRequest =>",
    "public void LoadNew()",
    "public void LoadDesignForNew(",
    "public void LoadExisting(",
    "private void LoadFromModel(",
    "private void Recompute()",
    "private bool AllFieldsValid(out string error)",
    "private NumericField[] AllNumericFields()",
    "private PushBackEditorInputs ReadInputs()",
    "private void Name_Changed(",
    "private void ShowDimensionViews(",
    "private void Insert_Click(",
    "private void InsertViewAt(",
    "private void Restore_Click(",
    "private void Update_Click(",
    "private void InsertBatch_Click(",
    "private void RequestDraw(",
    "private void Close_Click(",
    "protected override void OnClosing(",
    "internal EditorPendingWork PendingWork()",
    "private void Bom_Click(",
    "private void SaveLibrary_Click(",
    "private bool OutputBlockedAfterRecompute(",
    "private void UpdateButtons()",
    "internal Func<SelectiveSafetySelection",
    "internal Func<SelectiveSafetyWindow, bool?> SafetyWindowDialog;",
    "internal Action<RackFrameConfiguratorWindow> HeaderConfiguratorPresenter",
    "internal Func<SafetyPerPostWindow, bool?> BootPerPostWindowDialog;",
]:
    print(repr(sig), "->", member_span(w, sig))

section("C. W: lineas exactas que fijan las guardas o el flujo")
for pat in [
    r"new RackBomWindow\(lastComputation\.Bom\) \{ Owner = this \}\.ShowDialog\(\);",
    r'UiSupport\.PromptSaveToLibrary\(this, NameBox\.Text, "sistema"\)',
    r"session\.Identity\.EnsureId\(\);",
    r"session\.Identity\.SetName\(NameBox\.Text\?\.Trim\(\)\);",
    r"var project = RackProject\.ForPushBack\(lastComputation\.Design\)\.WithSourceMetadataFrom\(sourceProject\);",
    r"new RackProjectStore\(\)\.Save\(project, path\);",
    r"if \(moduleOwner\.ModuleSession\.HasPendingChanges\) moduleOwner\.CommitModuleEdits\(\);",
    r"Views\.RackViewBatchDialogPresenter\.TryShow\(this, RackSystemKind\.PushBack, options, out var views\)",
    r"session\.RequestInsertViews\(views",
    r"session\.RequestUpdate\(",
    r"session\.RequestInsert\(",
    r"\bClose\(\);",
    r"session\.Identity\.Adopt\(",
    r"canInsertInAutoCad",
    r"isEditingExisting",
    r"UpdateButton\.",
    r"InsertBatchButton",
    r"EditorClosePolicy\.MayClose\(PendingWork\(\)\)",
    r"if \(!canInsertInAutoCad\)",
    r"if \(updateOnly && !isEditingExisting\)",
]:
    print(pat, "->", lines_of(w, pat))
print(".RequestUpdate( en W =", len(re.findall(r"\.RequestUpdate\s*\(", w)))
print("InsertRequested en W ->", lines_of(w, r"\bInsertRequested\b"))

section("D. WC")
wc = show(WC)
for sig in [
    "internal PushBackCompositeEditorState CompositeState =>",
    "private void InitializeCompositeSection()",
    "private void UpdateCompositeEnabled()",
    "private void UpdateSideBEmptyNotice()",
    "private void UpdateFrontalButtons(bool composed)",
    "private void LoadCompositeFromDesign(PushBackDesign design)",
    "private IReadOnlyList<PushBackCompositeDiagnostic> CompositeDiagnostics(",
    "private string CompositeStatusOr(",
    "private bool CompositeHasBlocking(",
]:
    print(repr(sig), "->", member_span(wc, sig))
print("Visibility en WC ->", lines_of(wc, r"\.Visibility\s*="))

section("E. WX: botones de la barra de acciones")
wx = show(WX)
for name in ["RestoreButton", "BomButton", "SaveLibraryButton", "UpdateButton", "InsertLateralButton",
             "InsertFrontalEntradaButton", "InsertFrontalPosteriorButton", "FrontalSideBox", "InsertPlantaButton",
             "InsertBatchButton", "InsertButton\"", "CloseButton", "AnnotationScaleBox"]:
    print(name, "->", lines_of(wx, 'x:Name="' + name.strip('"') + '"'))
print("AnnotationScaleBox reglas ->", [l for l in wx.split("\n") if "MinimumInclusive" in l and "False" in l][:3])

section("F. Sesion e identidad")
ses = show(SES)
for sig in ["public bool InsertRequested", "public RackInsertionRequest InsertionRequest", "public void RequestInsertViews(",
            "public void RequestUpdate(", "private void Complete(", "public event EventHandler InsertRequestedRaised;"]:
    print(repr(sig), "->", member_span(ses, sig))
rid = show(RID)
for sig in ["public string EnsureId()", "public void Adopt(string id, string name)", "public void SetName(string name)"]:
    print(repr(sig), "->", member_span(rid, sig))

section("G. Plugin y menu: quien lee la peticion clasica")
rpc = show(RPC)
for pat in [r"new RackPushBackSystemWindow\(", r"AcApplication\.ShowModalWindow\(window\);", r"window\.InsertionRequest",
            r"window\.InsertRequested", r"window\.UpdateOnly", r"window\.DesignToInsert", r"window\.SystemToInsert",
            r"system\.Name = "]:
    print("RPC", pat, "->", lines_of(rpc, pat))
rmc = show(RMC)
for pat in [r"case PushBackInsertionRequest pushBack:", r"pushBack\.System\.Name = pushBackName;", r"menu\.InsertionRequest"]:
    print("RMC", pat, "->", lines_of(rmc, pat))
em = show(EM)
for pat in [r"new RackPushBackSystemWindow\(context\.CanInsertInAutoCad\) \{ Owner = context\.Owner \};",
            r"window\.InsertRequested \? window\.InsertionRequest : null"]:
    print("EM", pat, "->", lines_of(em, pat))
print("constructores de la ventana en src:")
print(git("grep", "-n", "new RackPushBackSystemWindow(", SHA, "--", "src").rstrip())
print("lectores de InsertRequested / InsertionRequest de Push Back en src (sin W):")
print(git("grep", "-n", "-E", r"window\.(InsertRequested|InsertionRequest)", SHA, "--", "src/RackCad.Plugin",
          "src/RackCad.UI/Editor").rstrip())

section("H. Application: API de PB-T1b y nucleo de borradores")
pes = show(PES)
for sig in ["public PushBackEditableState()", "public PushBackCompositeEditorState Editor { get; }", "public string Name { get; set; }",
            "public PushBackEditorInputs LoadDesignEditor(", "public PushBackEditorInputs RestoreEditor(",
            "public void AdoptCarriers(", "public PushBackEditorInputs BuildInputs(", "public static PushBackEditableSupport SupportOf("]:
    print("PES", repr(sig), "->", member_span(pes, sig))
pesc = show(PESC)
for sig in ["public bool Equals(PushBackEditableState x, PushBackEditableState y)", "public static PushBackEditableState Copy(",
            "private static void CopyEditor("]:
    print("PESC", repr(sig), "->", member_span(pesc, sig))
print("PESC 'The target is a fresh state' ->", lines_of(pesc, r"The target is a fresh state"))
pog = show(POG)
for sig in ["public static IReadOnlyList<PushBackCompositeDiagnostic> Diagnostics(", "public static bool IsBlocked(",
            "public static string StatusOr("]:
    print("POG", repr(sig), "->", member_span(pog, sig))
ds = show(DS)
for sig in ["public TState InitialDraftState =>", "public TState CurrentDraftState =>", "public bool IsDirty =>",
            "public DraftEditResult SetCurrentDraftState(TState state)", "private bool CanEdit =>",
            "public sealed class DraftSession<TState> : IDraftEntry"]:
    print("DS", repr(sig), "->", member_span(ds, sig))
dr = show(DR)
print("DR Begin ->", member_span(dr, "public DraftBeginResult<TState> Begin<TState>("))
print("DS/DR nombran PushBack?", "PushBack" in ds or "PushBack" in dr)
whc = show(WHC)
print("WHC Begin con el copiador del piloto ->", lines_of(whc, r"PushBackPilotStateComparer\.(Instance|Copy)"))
print("WHC lineas", len(whc.split("\n")))

section("I. La validacion del campo de escala en dos sitios")
pfa = show(PFA)
print("PFA Validate ->", member_span(pfa, "public DraftFieldValidation Validate(string fieldKey, string text)"))
for pat in [r'EmptyMessage = "', r'NotANumberMessage = "', r'NotPositiveMessage = "', r"LocalizedNumberParser\.TryDouble", r"value <= 0\.0"]:
    print("PFA", pat, "->", lines_of(pfa, pat))
nfv = show(NFV)
for pat in [r'EmptyRequiredMessage = "', r'NotANumberMessage = "', r"LocalizedNumberParser\.TryDouble", r"MinimumMessage"]:
    print("NFV", pat, "->", lines_of(nfv, pat))

section("J. Guardas")
tsg = show(TSG)
for sig in ["private static string ClassicWindowSources()", "public void TheClassicWindowHasNoKeyboardHandlersNoTabOrderNoForcedFocusAndNoDialogResult()",
            "public void NoCodeAnywhereInSrcSubscribesToInsertRequestedRaised()", "public void TheBomWindowIsOwnedByTheWindow()",
            "public void TheSaveFileDialogIsOwnedByTheWindowThroughPromptSaveToLibrary()",
            "public void TheMenuHostOpensThePushBackWindowWithItsContextOwnerInBothPathsAndReturnsTheRequestOnlyWhenRequested()",
            "public void SaveToLibraryPromptsFirstThenMarksTheIdentityNamesItAndSavesTheLibraryProject()",
            "public void InsertBatchShowsItsDialogThenRequestsTheBatchAndClosesWithoutLookingAtCanInsertInAutoCad()",
            "public void TheHeaderConfiguratorIsOwnedByTheWindowCentersOnItsOwnerAndItsConfigurationIsAlwaysStaged()",
            "public void TheDefenseGridIsOwnedByTheWindow()", "public void TheTopeGridIsOwnedByTheWindow()",
            "public void TheBatchDialogIsShownThroughItsPresenterWithTheWindowAsOwner()"]:
    print("T-SG", repr(sig[:70]), "->", member_span(tsg, sig))
print("T-SG [Fact] =", len(re.findall(r"\[Fact\]", tsg)))
g10 = show(G10)
print("G-10 OwnRequestUpdateWindows ->", lines_of(g10, r"OwnRequestUpdateWindows ="))
print("G-10 G10Violations ->", member_span(g10, "private static IEnumerable<string> G10Violations("))
print("G-10 regex ->", lines_of(g10, r"\(\?<!IWorkspacePilotEditPort\)\\\.RequestUpdate"))
dbt = show(DBT)
print("Drafts: ExpectedFiles ->", lines_of(dbt, r"ExpectedFiles"))
print("Drafts: sistemas prohibidos ->", lines_of(dbt, r"PushBack\|Selectiv"))
print("Drafts: nombres de tipo prohibidos ->", lines_of(dbt, r"ForbiddenTypeNames = new"))
dig = show(DIG)
print("Drafts sin PushBack (DIG) ->", member_span(dig, "public void TheDraftCoreStillNamesNoSystem()"))
rpt = show(RPT)
print("RichPreview reflexion ->", member_span(rpt, "public void TheSimplifiedRenderer_IsNoLongerInTheDrawingPath()"))
wcg = show(WCG)
print("Censo de ventanas: Utilities ->", lines_of(wcg, r"private static readonly string\[\] Utilities"))
lsg = show(LSG)
print("LibrarySaveGate ->", lines_of(lsg, r"PromptSaveToLibrary|BuildLibraryProjectForTest|SaveLibraryButton"))
dwc = show(DWC)
print("DialogWindowCharacterization RackBomWindow ->", lines_of(dwc, r"RackBomWindow")[:12])
edc = show(EDC)
print("EditorDiscardPromptCollection ->", lines_of(edc, r"class EditorDiscardPromptCollection|Name ="))

section("K. Archivos de la clausura de T-SG (src/RackCad.UI/Systems/PushBack)")
print(git("ls-tree", "-r", "--name-only", SHA, "--", "src/RackCad.UI/Systems/PushBack").rstrip())

section("L. Prefijos libres para lo nuevo")
for token in ["PushBackEditorMode", "PushBackDraftEditor", "PushBackDraftStatePort", "PushBackDraftSessionPort", "ForDraft(",
              "ApplyToDraft", "PushBackWorkspaceEditor", "PushBackWorkspaceMode", "CT_PBT2", "BomWindowDialog",
              "LibraryPathDialog", "Aplicar al borrador"]:
    hits = git("grep", "-n", "-F", token, SHA, "--", "src", "tests").strip()
    print(repr(token), "->", "libre" if not hits else hits.split("\n")[:4])

section("M. Recuento estatico de pruebas por prefijo")
def count(prefix_dir, name_rx):
    files = [f for f in git("ls-tree", "-r", "--name-only", SHA, "--", prefix_dir).split("\n") if f.endswith(".cs")]
    total = 0
    detail = []
    for f in files:
        base = f.rsplit("/", 1)[-1]
        if not re.match(name_rx, base):
            continue
        t = show(f)
        n = len(re.findall(r"\[(Fact|Theory)\b", t))
        total += n
        detail.append((base, n))
    return total, detail
for d, rx in [("tests/RackCad.UI.Tests", r"PushBack"), ("tests/RackCad.UI.Tests", r"PushBackClassicCharacterization"),
              ("tests/RackCad.Tests", r"PushBackClassicCharacterization"), ("tests/RackCad.UI.Tests", r"PushBackStateNet"),
              ("tests/RackCad.Tests", r"PushBackEditableState"), ("tests/RackCad.UI.Tests", r"PushBackEditableState"),
              ("tests/RackCad.Tests", r"PushBackEditorOutputGate"), ("tests/RackCad.Tests", r"PushBackSharedReferenceGuard"),
              ("tests/RackCad.UI.Tests", r"PushBackDeclaredCorrection"), ("tests/RackCad.UI.Tests", r"PushBackFullEditorCharacterization"),
              ("tests/RackCad.Tests", r"PushBackFullEditorCharacterization"), ("tests/RackCad.UI.Tests", r"Cantilever"),
              ("tests/RackCad.UI.Tests", r"WindowCensusGuardTests"), ("tests/RackCad.UI.Tests", r"PushBackLibrarySaveGateTests"),
              ("tests/RackCad.UI.Tests", r"DialogWindowCharacterizationTests")]:
    total, detail = count(d, rx)
    print("%s %s* -> %d metodos en %d archivos" % (d, rx, total, len(detail)))
