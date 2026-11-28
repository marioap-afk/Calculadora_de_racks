# Solo lectura: los anclajes de PB-T1b (precondicion 4 del paquete de PB-T1, section 6.0) sobre el cierre de PB-T1a (5043b49a).
# Lee HEAD con git show / git diff / git ls-tree, ejecuta en solo lectura remeasure_gl1_v24.py y count_tests.py (con -I) y consulta
# la CI con gh en solo lectura (run list, run view, run view --job --log; ninguna descarga). No escribe nada.
# Las rutas locales no se imprimen: el worktree sale como <worktree-de-I-64>.
# Uso: python -I measure_pbt1b_anchors.py <raiz-del-worktree>
import os
import re
import subprocess
import sys

sys.stdout.reconfigure(encoding="utf-8")
WT = sys.argv[1]
HERE = os.path.dirname(os.path.abspath(__file__))
PB1A_CLOSE = "5043b49a"
PB1A_EMIT = "a3b63ccc"
TC2_GREEN = "b5c09f0b"
TC2_CLOSE = "cd47523b"


def run(args, cwd=None):
    r = subprocess.run(args, cwd=cwd or WT, capture_output=True)
    return r.returncode, r.stdout.decode("utf-8", "replace"), r.stderr.decode("utf-8", "replace")


def redact(text):
    for p in (WT, WT.replace("\\", "/"), HERE, HERE.replace("\\", "/")):
        text = text.replace(p, "<redactado>")
    return text


def cmd(label, args, cwd=None, keep=None):
    print("$ " + label)
    rc, out, err = run(args, cwd)
    lines = out.splitlines()
    if keep is not None:
        lines = [l for l in lines if keep(l)]
    for l in lines:
        print("  " + redact(l))
    if not lines:
        print("  <vacio>")
    if rc != 0:
        print("  rc=%d %s" % (rc, redact(err.strip())[:300]))
    return rc, out


def show(path):
    return run(["git", "show", "HEAD:" + path])[1]


APP = "src/RackCad.Application/Systems/PushBack/"
F = {
    "ES": APP + "PushBackEditorState.cs",
    "EL": APP + "PushBackEditorState.Load.cs",
    "CS": APP + "PushBackCompositeEditorState.cs",
    "DA": APP + "PushBackEditorDesignAssembler.cs",
    "CA": APP + "PushBackCompositeEditorAssembler.cs",
    "PCS": APP + "PushBackCompositeStructure.cs",
    "PC": APP + "Pilot/PushBackPilotStateComparer.cs",
    "PM": APP + "Pilot/PushBackPilotMaterializer.cs",
    "RMES": "src/RackCad.Application/Systems/Shared/RackModuleEditSession.cs",
    "RMR": "src/RackCad.Application/Systems/Shared/RackModuleReconciliation.cs",
    "RFPS": "src/RackCad.Application/Persistence/RackFrameProjectStore.cs",
    "DDA": "src/RackCad.Application/Systems/Dynamic/DynamicEditorDesignAssembler.cs",
    "W": "src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs",
    "WC": "src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.Composite.cs",
    "UIS": "src/RackCad.UI/UiSupport.cs",
    "BOG": "src/RackCad.Application/Bom/RackBomOutputGate.cs",
    "PSYS": "src/RackCad.Domain/Systems/PushBack/PushBackSystem.cs",
    "DRS": "src/RackCad.Domain/Systems/Dynamic/DynamicRackSystem.cs",
    "DRSB": "src/RackCad.Application/Systems/Dynamic/DynamicRackSystemBuilder.cs",
    "BPMB": "src/RackCad.Application/RackFrames/BracingPanelMemberBuilder.cs",
    "PBDD": "src/RackCad.Application/Persistence/PushBackDesignDocument.cs",
    "WDPS": "tests/RackCad.Tests/Workspace/WorkspaceDraftPilotStateTests.cs",
    "WDPM": "tests/RackCad.Tests/Workspace/WorkspaceDraftPilotMaterializationTests.cs",
    "POG": "tests/RackCad.Tests/PushBackOutputGateTests.cs",
    "HBC": "tests/RackCad.Tests/HeaderBatchContractTests.cs",
    "PPT": "tests/RackCad.UI.Tests/PushBackDeclaredCorrectionPilotParityTests.cs",
    "SNRC": "tests/RackCad.UI.Tests/PushBackStateNetRackControlsTests.cs",
}
TEXT = {k: show(v).replace("\r\n", "\n").split("\n") for k, v in F.items()}

DECL = re.compile(
    r"^\s{4,8}(?:\[[^\]]*\]\s*)*"
    r"((?:public|private|internal|protected|static|readonly|override|virtual|sealed|async|partial|new|const|event)\s+)+"
    r"[\w<>\[\]\?,\.\s]+?\b(\w+)\s*(\(|\{|=|;|=>)")


def members(lines):
    """Rangos de miembros (heuristico de members.py: cuenta llaves a profundidad 2)."""
    depth, results, current = 0, [], None
    for i, raw in enumerate(lines, start=1):
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


MEMBERS = {k: members(v) for k, v in TEXT.items()}


def member_at(alias, line):
    hits = [m for m in MEMBERS[alias] if m[1] <= line <= m[2]]
    return ("%s(%d-%d)" % hits[-1]) if hits else "-"


# (id, alias, linea, texto que la linea contiene, cita del paquete)
ANCHORS = [
    # ES
    ("ES-65", "ES", 65, "private PushBackSystem workingBaseline;", "HT1-01; D-PBT1-15 cond. 1"),
    ("ES-71", "ES", 71, "public PushBackSystem WorkingBaseline => workingBaseline;", "D-PBT1-15 cond. 1"),
    ("ES-99", "ES", 99, "public void AdoptLoadedBaseline(PushBackSystem baseline)", "D-PBT1-15 (ES:99-106)"),
    ("ES-105", "ES", 105, "LastModuleReconciliation = null;", "D-PBT1-15 (ES:105)"),
    ("ES-110", "ES", 110, "private RackModuleEditSession moduleSession;", "HT1-01 (ES:110-112); INV-PBT1B-04"),
    ("ES-111", "ES", 111, "private RackModuleCommit moduleCommit;", "HT1-01"),
    ("ES-112", "ES", 112, "private string moduleSignature;", "HT1-01"),
    ("ES-124", "ES", 124, "public RackModuleEditSession ModuleSession", "D-PBT1-06 (ES:124-133)"),
    ("ES-143", "ES", 143, "public RackModuleReconciliationResult LastModuleReconciliation { get; set; }", "HT1-01; D-PBT1-15 cond. 1"),
    ("ES-176", "ES", 176, "private void ReseedModuleSession()", "D-PBT1-06 (ES:176-187)"),
    # CS
    ("CS-58", "CS", 58, "public List<DynamicRackModuleDesign> DormantCompositeTail { get; }", "HT1-01"),
    ("CS-61", "CS", 61, "public List<DynamicHeaderLineOverride> DormantTailLineOverrides { get; }", "HT1-01"),
    ("CS-74", "CS", 74, "public void ParkDormantTail(", "D-PBT1-05 paso 2"),
    ("CS-116", "CS", 116, "public void MarkDormantTailConsumed()", "D-PBT1-05 (CS:116-117)"),
    ("CS-145", "CS", 145, "public bool DormantTailConsumptionPending => dormantTailConsumed;", "D-PBT1-05"),
    ("CS-147", "CS", 147, "private bool dormantTailConsumed;", "HT1-01; CT-PBT1B-06 (T)"),
    ("CS-167", "CS", 167, "public PushBackSide ActiveSide { get; private set; }", "HT1-01"),
    ("CS-179", "CS", 179, "public PushBackSideSelection ActiveSelection { get; private set; }", "HT1-01"),
    ("CS-629", "CS", 629, "public IReadOnlyList<PushBackCompositeDiagnostic> IntentDiagnostics()", "HT1-19 (CS:629-631)"),
    # RMES
    ("RMES-16", "RMES", 16, "public sealed class RackModuleCommit", "D-PBT1-07 (RMES:16-50)"),
    ("RMES-18", "RMES", 18, "internal RackModuleCommit(", "D-PBT1-07 (RMES:18)"),
    ("RMES-74", "RMES", 74, "private readonly RackFrameProjectStore clone = new RackFrameProjectStore();", "D-PBT1-07 (RMES:74)"),
    ("RMES-82", "RMES", 82, "private Dictionary<LineKey, RackFrameConfiguration> committedLines", "CT-PBT1B-07 (RMES:82-83)"),
    ("RMES-83", "RMES", 83, "private Dictionary<LineKey, RackFrameConfiguration> workingLines", "CT-PBT1B-07 (RMES:82-83)"),
    ("RMES-91", "RMES", 91, "committed = baseline.Select(CopyIntent).ToList();", "D-PBT1-03 (M-3: CopyIntent privado)"),
    ("RMES-732", "RMES", 732, "private Dictionary<LineKey, RackFrameConfiguration> CopyLines(", "D-PBT1-07 (RMES:732-742)"),
    ("RMES-744", "RMES", 744, "private static bool AreEquivalentDerivedPosts(", "D-PBT1-07 (RMES:744-746)"),
    ("RMES-750", "RMES", 750, "private bool AreEquivalentLines(", "D-PBT1-07 (RMES:750-769)"),
    ("RMES-818", "RMES", 818, "private bool AreEquivalent(", "D-PBT1-06/07 (RMES:818-846)"),
    # DA
    ("DA-57", "DA", 57, "private static IReadOnlyList<DynamicRackModuleDesign> ModuleIntents(", "D-PBT1-06 (DA:57-81)"),
    ("DA-101", "DA", 101, "public void AcceptComputation(PushBackEditorState state, PushBackEditorComputation computation)", "D-PBT1-15 (DA:101-107)"),
    ("DA-155", "DA", 155, "state.NormalizePeraltes(pushResolver.AllowedHighEndPeraltes());", "HT1-10 (DA:155-159)"),
    ("DA-169", "DA", 169, "var postId = string.IsNullOrWhiteSpace(editorInputs.PostCatalogId)", "D-PBT1-09 (DA:169-171)"),
    ("DA-176", "DA", 176, "var pallet = ClonePallet(editorInputs.Pallet);", "D-PBT1-09b (c-09)"),
    ("DA-183", "DA", 183, "var depthLayout = DynamicDepthGeometry.Resolve(envelopeFronts, Math.Max(2, editorInputs.PalletsDeep));", "D-PBT1-09b (OWNER-3)"),
    ("DA-211", "DA", 211, "var mustRebuild = standardRestore", "D-PBT1-09b (DA:211-218)"),
    ("DA-213", "DA", 213, "|| DynamicEditorDesignAssembler.MustRebuild(rebuildShape, pallet, depthLayout);", "D-PBT1-09b"),
    ("DA-218", "DA", 218, "system = builder.BuildDefault(pallet, depthLayout, RackFrameTemplateCatalog.Default, postId, headerHeight, postPeralte);", "D-PBT1-09b"),
    ("DA-273", "DA", 273, "system = CopyStructureSystem(baseline);", "D-PBT1-09b"),
    ("DA-302", "DA", 302, "PushBackAdvancedRackParameters.Reset(editorInputs);", "D-PBT1-04 (DA:302)"),
    ("DA-325", "DA", 325, "state.LastModuleReconciliation = reconciliation.Reconcile(intents, system, restoredIds);", "D-PBT1-15 (DA:325)"),
    ("DA-557", "DA", 557, "system.Modules.Add(new DynamicRackModule", "D-PBT1-03/07 (DA:557-566)"),
    ("DA-565", "DA", 565, "AssociatedFrameConfiguration = headerClone.DeepCopy(module.HeaderConfiguration),", "D-PBT1-03/07"),
    ("DA-576", "DA", 576, ": new PalletSpecification(pallet.Front, pallet.Depth, pallet.Height, pallet.Weight, pallet.WeightUnit);", "D-PBT1-09b (ClonePallet)"),
    # CA y PCS
    ("CA-180", "CA", 180, "private static void ParkTailOf(PushBackCompositeEditorState state)", "HT1-12 (CA:180-242)"),
    ("CA-189", "CA", 189, "var lines = baseline.HeaderLineOverrides", "HT1-12 (CA:188-190)"),
    ("CA-202", "CA", 202, "var commit = state.SideA?.ModuleCommit;", "HT1-12 (CA:201-239)"),
    ("CA-241", "CA", 241, "state.ParkDormantTail(parked, lines);", "HT1-12"),
    ("PCS-777", "PCS", 777, "public static DynamicRackModuleDesign ToTailDesign(DynamicRackModule module)", "HT1-12 (PCS:777)"),
    ("PCS-809", "PCS", 809, "HeaderConfiguration = module.AssociatedFrameConfiguration,", "HT1-12 (PCS:800-811)"),
    # El piloto, el informe, el almacen y las dependencias de D-PBT1-09 a 11 y 15
    ("PC-37", "PC", 37, "public int GetHashCode(PushBackPilotDraftState obj)", "D-PBT1-06 (PC:37-49)"),
    ("PC-58", "PC", 58, "public static PushBackPilotDraftState Copy(PushBackPilotDraftState state)", "HT1-20 (PC:58-111)"),
    ("PC-64", "PC", 64, "private static PushBackCompositeEditorState CopyEditor(PushBackCompositeEditorState source)", "D-PBT1-05 (PC:64-83)"),
    ("PC-134", "PC", 134, "private static bool EditorsEqual(PushBackCompositeEditorState x, PushBackCompositeEditorState y)", "D-PBT1-06 (PC:134-249)"),
    ("PC-309", "PC", 309, "private static class StructuralEquality", "D-PBT1-06 (PC:309-350)"),
    ("PM-108", "PM", 108, "public const string DormantTailOutOfPilotReason", "D-PBT1-10; CT-PBT1B-04"),
    ("PM-160", "PM", 160, "editor.ClearDormantTail();", "D-PBT1-10 (PM:160-170)"),
    ("RMR-16", "RMR", 16, "public sealed class RackModuleReconciliationResult", "D-PBT1-15 cond. 1"),
    ("RMR-212", "RMR", 212, "return new RackModuleReconciliationResult(preserved, adapted, removed, incompatible, restored);", "D-PBT1-15"),
    ("RFPS-87", "RFPS", 87, "public RackFrameConfiguration DeepCopy(RackFrameConfiguration configuration)", "D-PBT1-03 (RFPS:87)"),
    ("UIS-124", "UIS", 124, "public static List<CatalogOption> ToOptions<T>(IEnumerable<T> entries) where T : CatalogEntryBase", "D-PBT1-09 (UiSupport.cs:124-133)"),
    ("BOG-49", "BOG", 49, "public static RackBomOutputVerdict For(PushBackSystem system)", "HT1-19 (RackBomOutputGate.cs:49-59)"),
    ("DDA-44", "DDA", 44, "public static bool MustRebuild(DynamicRackSystem system, PalletSpecification pallet, DynamicDepthLayout depthLayout)", "D-PBT1-09b"),
    ("DDA-50", "DDA", 50, "private static bool SamePallet(PalletSpecification a, PalletSpecification b)", "D-PBT1-09b (DDA:50-61)"),
    ("DDA-60", "DDA", 60, "&& Math.Abs(a.Weight - b.Weight) < 1e-6;", "D-PBT1-09b: SamePallet no mira la unidad"),
    ("DDA-65", "DDA", 65, "public void UpdateHeaderHeightInPlace(DynamicRackSystem system, double newHeight, string postId)", "G-L1 regla 1c"),
    ("DRS-146", "DRS", 146, "public void RecalculatePositions()", "G-L1 regla 1"),
    ("DRSB-137", "DRSB", 137, "public void Refresh(DynamicRackSystem system)", "G-L1 regla 1c"),
    ("BPMB-11", "BPMB", 11, "public void RefreshPhysicalModel(RackFrameConfiguration configuration)", "G-L1 regla 1c"),
    ("PSYS-143", "PSYS", 143, "set { if (Structure != null) { Structure.Name = value; } }", "D-PBT1-15 cond. 8"),
    ("PBDD-469", "PBDD", 469, "Gap = Gap.HasValue && Gap.Value > 0.0 ? Gap.Value : 0.0,", "CT-PBT1B-01 (N-M-04)"),
    # W y WC que reproduce PB-T1b (D-PBT1-03, 04, 09, 09b, 11)
    ("W-148", "W", 148, 'WeightUnitBox.ItemsSource = new[] { "kg", "lb" };', "D-PBT1-09b (c-09)"),
    ("W-149", "W", 149, "WeightUnitBox.SelectedIndex = 0;", "D-PBT1-09b (c-09)"),
    ("W-273", "W", 273, "composite.SetActiveSide(RackCad.Domain.Systems.PushBack.PushBackSide.A);", "D-PBT1-03 (W:273-276)"),
    ("W-286", "W", 286, "safetySelections.Clear();", "D-PBT1-03 (W:286-287)"),
    ("W-301", "W", 301, "var inputs = state.LoadFromDesign(SideAOnly(design), assembler.Resolver);", "D-PBT1-03 (W:299-302)"),
    ("W-318", "W", 318, "var inputs = state.LoadFromDesign(SideAOnly(design), assembler.Resolver);", "D-PBT1-03 (W:316-319)"),
    ("W-391", "W", 391, "firstLevelDatum = inputs.FirstLevelDatum;", "D-PBT1-03 (W:391-427)"),
    ("W-392", "W", 392, 'var pallet = inputs.Pallet ?? new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg");', "D-PBT1-03; c-09"),
    ("W-393", "W", 393, "generalPallet = pallet;", "D-PBT1-09b (c-09)"),
    ("W-395", "W", 395, 'WeightUnitBox.SelectedItem = string.IsNullOrWhiteSpace(pallet.WeightUnit) ? "kg" : pallet.WeightUnit;', "D-PBT1-09b (c-09)"),
    ("W-396", "W", 396, "PalletsDeepBox.SetNumber(Math.Max(2, inputs.PalletsDeep));", "D-PBT1-03; OWNER-3"),
    ("W-399", "W", 399, "PostBox.SelectedId = string.IsNullOrWhiteSpace(inputs.PostCatalogId) ? catalog?.Defaults?.Post : inputs.PostCatalogId;", "D-PBT1-09 (c-01, c-02)"),
    ("W-838", "W", 838, "generalPallet = new PalletSpecification(", "D-PBT1-09b (W:838-840)"),
    ("W-840", "W", 840, "generalPallet.Weight, generalPallet.WeightUnit);", "D-PBT1-09b: generalPallet conserva la cargada"),
    ("W-852", "W", 852, 'generalPallet.Weight, WeightUnitBox.SelectedItem as string ?? "kg"),', "D-PBT1-09b: la salida lleva la de la caja"),
    ("W-853", "W", 853, "PalletsDeep = IntVal(PalletsDeepBox, DynamicRackDefaults.DefaultPalletsDeep),", "D-PBT1-09b (OWNER-3)"),
    ("W-868", "W", 868, "PostCatalogId = PostBox.SelectedId,", "D-PBT1-09 (c-02)"),
    ("W-2682", "W", 2682, "ApplyBootSections(bootSectionA, bootSectionB);", "D-PBT1-09b (OWNER-4)"),
    ("W-3322", "W", 3322, "var inputs = state.LoadFromDesign(SideAOnly(design), assembler.Resolver);", "D-PBT1-03 (W:3320-3327)"),
    ("W-3327", "W", 3327, "LoadCompositeFromDesign(design, preserveDormantIntent: true);", "D-PBT1-03"),
    ("WC-783", "WC", 783, "private IReadOnlyList<PushBackCompositeDiagnostic> CompositeDiagnostics()", "D-PBT1-11 (WC:783-786)"),
    ("WC-789", "WC", 789, "private string CompositeStatusOr(string fallback)", "D-PBT1-11 (WC:789-801)"),
    ("WC-806", "WC", 806, "return CompositeDiagnostics().Any(diagnostic => diagnostic.IsBlocking);", "D-PBT1-11 (WC:806)"),
    ("WC-827", "WC", 827, "private void LoadCompositeFromDesign(PushBackDesign design, bool preserveDormantIntent)", "D-PBT1-03 (WC:827-934)"),
    ("WC-933", "WC", 933, "UpdateCompositeEnabled();", "D-PBT1-03 (WC:933)"),
    # Las pruebas que citan el esqueleto y los CT de PB-T1b
    ("WDPS-211", "WDPS", 211, "public static readonly IReadOnlyDictionary<string, PilotFieldClass> Table", "CT-PBT1B-06 (:211-369)"),
    ("WDPS-501", "WDPS", 501, "public static HashSet<object> AllObjects(object root)", "CT-PBT1B-07 (:501-529)"),
    ("WDPS-531", "WDPS", 531, "public static bool IsImmutable(Type type)", "CT-PBT1B-07 (:531-536)"),
    ("POG-41", "POG", 41, "var design = PushBackCompositeStructureTests.Composite(", "CT-PBT1B-01/12 (Blocking, :41-48)"),
    ("WDPM-706", "WDPM", 706, "public static readonly IReadOnlyDictionary<string, string[]> LegacyDeltas", "CT-PBT1B-02 (:706-735)"),
    ("PPT-20", "PPT", 20, "public void CT_PBTC_06_TheWindowAndThePilotWriteTheSameDesignForTheFourteenDesigns()", "CT-PBT1B-02 (:20-26)"),
    ("HBC-524", "HBC", 524, "var engineVerbs = new[]", "D-PBT1-07 (C-10, :524-537)"),
    ("SNRC-133", "SNRC", 133, "public void CT_PBT1A_02_TheWeightUnitTheLoadedTonUnitAndTheDeepPalletsBoxReachTheOutput()", "CT-PBT1B-17 (oraculo)"),
]

print("== P-0. Identidad")
cmd("git -C <worktree-de-I-64> rev-parse HEAD", ["git", "rev-parse", "HEAD"])
cmd("git -C <worktree-de-I-64> status --short", ["git", "status", "--short"])
cmd("git -C <worktree-de-I-64> log --oneline -6", ["git", "log", "--oneline", "-6"])
cmd("git -C <worktree-de-I-64> diff --stat %s %s -- src" % (PB1A_EMIT, PB1A_CLOSE), ["git", "diff", "--stat", PB1A_EMIT, PB1A_CLOSE, "--", "src"])
cmd("git -C <worktree-de-I-64> diff --stat %s HEAD -- src" % TC2_GREEN, ["git", "diff", "--stat", TC2_GREEN, "HEAD", "--", "src"])
cmd("git -C <worktree-de-I-64> diff --stat %s -- src   (el arbol de trabajo frente al cierre de PB-T1a)" % PB1A_CLOSE, ["git", "diff", "--stat", PB1A_CLOSE, "--", "src"])
cmd("git -C <worktree-de-I-64> diff --name-status %s HEAD -- tests" % TC2_GREEN, ["git", "diff", "--name-status", TC2_GREEN, "HEAD", "--", "tests"])
rc = run(["git", "diff", "--quiet", TC2_CLOSE, "HEAD", "--", "docs/automation/agent-execution"])[0]
print("$ git -C <worktree-de-I-64> diff --quiet %s HEAD -- docs/automation/agent-execution\n  rc=%d (%s)" % (TC2_CLOSE, rc, "sin cambios" if rc == 0 else "CON CAMBIOS"))
print("$ lineas (git show HEAD:<ruta>)")
for k in ("ES", "EL", "CS", "DA", "CA", "RMES", "W", "WC"):
    print("  %-5s %5d  %s" % (k, len(TEXT[k]) - (1 if TEXT[k] and TEXT[k][-1] == "" else 0), F[k]))

print("\n== P-1. Las anclas por miembro y linea (D-PBT1-02 a D-PBT1-11, D-PBT1-15, el esqueleto y los CT de la section 5.3)")
bad = 0
for ident, alias, line, text, cite in ANCHORS:
    lines = TEXT[alias]
    got = lines[line - 1].strip() if line <= len(lines) else "<fuera>"
    ok = text in got
    bad += 0 if ok else 1
    print("  %-4s %-9s %s:%d  %s  [%s]  | %s" % ("OK" if ok else "DIFF", ident, alias, line, member_at(alias, line), cite, got[:150]))
print("  anclas=%d diferencias=%d" % (len(ANCHORS), bad))

print("\n== P-2. c-09: la regla de hoy y su oraculo")
print("$ git show HEAD:%s | sed -n 195,205p   (las lineas esperadas de CT-PBT1A-02)" % F["SNRC"])
for i in range(195, 206):
    print("  %d: %s" % (i, TEXT["SNRC"][i - 1].rstrip()))
print("$ la unidad de las 17 fixtures de I64 (git ls-tree + git show; PalletWeightUnit)")
fx = [p for p in run(["git", "ls-tree", "-r", "--name-only", "HEAD", "--", "tests/RackCad.Tests/Fixtures/I64"])[1].split("\n") if p.endswith(".json")]
units = {}
for p in fx:
    t = show(p).replace("\\u0022", '"').replace('\\"', '"')
    for u in re.findall(r'PalletWeightUnit"\s*:\s*"([^"]*)"', t):
        units[u] = units.get(u, 0) + 1
print("  fixtures=%d unidades=%s" % (len(fx), dict(sorted(units.items()))))

print("\n== P-3. Las rutas de PB-T1b y la lista de archivos de G-L1 (CT-PBT1B-14)")
TR, UT = "tests/RackCad.Tests/", "tests/RackCad.UI.Tests/"
NEW = [APP + n for n in ("PushBackEditableState.cs", "PushBackEditableStateComparer.cs", "PushBackEditableStateOwnership.cs", "PushBackEditorOutputGate.cs")] + \
      [TR + n for n in ("PushBackEditableStateLoadTests.cs", "PushBackEditableStateCopyTests.cs", "PushBackEditableStateOwnershipTests.cs",
                        "PushBackEditorOutputGateTests.cs", "PushBackSharedReferenceGuardTests.cs")] + [UT + "PushBackEditableStateParityTests.cs"]
for p in [F["ES"], F["RMES"]] + NEW:
    exists = run(["git", "cat-file", "-e", "HEAD:" + p])[0] == 0
    print("  %-9s %s" % ("EXISTE" if exists else "NO-EXISTE", p))
ws = [p for p in run(["git", "ls-tree", "-r", "--name-only", "HEAD", "--", "src/RackCad.Application/Workspace/"])[1].split("\n") if p.endswith(".cs")]
print("  G-L1 (CT-PBT1B-14): 4 nuevos + ES, EL, CS, CA, DA, RMES, RackModuleReconciliation.cs + Workspace/ (%d .cs) = %d que existen" % (len(ws), 7 + len(ws)))
tracked_tests = run(["git", "ls-tree", "-r", "--name-only", "HEAD", "--", "tests"])[1].split("\n")
for pref in ("PushBackEditableState", "PushBackEditorOutputGate", "PushBackSharedReferenceGuardTests", "PushBackWindowSharedReferenceGuardTests", "PushBackWindowOnState"):
    print("  prefijo %-40s en tests/: %d" % (pref, len([p for p in tracked_tests if pref in p])))

print("\n== P-4. G-L1 de la v2.4 sobre los archivos de su lista (remeasure_gl1_v24.py)")
rc, out, err = run([sys.executable, "-I", os.path.join(HERE, "remeasure_gl1_v24.py"), WT], cwd=HERE)
print("$ python -I remeasure_gl1_v24.py <worktree-de-I-64>")
for l in out.splitlines():
    print("  " + redact(l))
if rc != 0:
    print("  rc=%d %s" % (rc, redact(err)[-300:]))

print("\n== P-5. Los recuentos estaticos de RequiredTests (count_tests.py)")
UI_PREFIXES = ["PushBackStateNet", "PushBackClassicCharacterization", "PushBackFullEditorCharacterization",
               "PushBackDeclaredCorrectionRoundingTests", "PushBackDeclaredCorrectionDimensionStyleTests", "PushBackDeclaredCorrectionPilotParityTests",
               "PushBackDeclaredCorrectionSideBNoticeTests", "PushBackDeclaredCorrectionStandardRestoreTests", "PushBackDeclaredCorrection2",
               "Cantilever", "PushBackEditableStateParityTests", "PushBackWindowOnState", "PushBack", "BlankFront", "WindowCensusGuardTests",
               "RichEditor", "Workspace", "PushBackRichPreviewTests"]
CORE_PREFIXES = ["PushBackEditableState", "PushBackEditorOutputGate", "PushBackSharedReferenceGuardTests", "PushBackWindowSharedReferenceGuardTests",
                 "PushBackWindowOnState", "Workspace", "PushBackOutputGateTests", "PushBackBlankFrontAuthorityTests",
                 "PushBackModuleEditorCharacterizationTests", "RackModuleEditSessionTests", "NamespaceFolderGuardTests", "HeaderBatchContractTests",
                 "PushBackClassicCharacterization", "PushBackFullEditorCharacterization", "I55DeferredProductFixCharacterizationTests", "PushBack"]
for proj, prefs in (("UI", UI_PREFIXES), ("Core", CORE_PREFIXES)):
    rc, out, err = run([sys.executable, "-I", os.path.join(HERE, "count_tests.py"), WT, proj] + prefs, cwd=HERE)
    print("$ python -I count_tests.py <worktree-de-I-64> %s %s" % (proj, " ".join(prefs)))
    for l in out.splitlines():
        print("  " + redact(l))
    if rc != 0:
        print("  rc=%d %s" % (rc, redact(err)[-300:]))

print("\n== P-6. La linea base de la CI (gh, solo lectura)")
BR = "architecture/workspace-persistente-rackcad"
rc, out = cmd("gh run list --branch %s --limit 5 --json databaseId,headSha,status,conclusion,event,createdAt,name" % BR,
              ["gh", "run", "list", "--branch", BR, "--limit", "5", "--json", "databaseId,headSha,status,conclusion,event,createdAt,name",
               "--jq", '.[] | [.databaseId, .headSha[0:8], .event, .status, .conclusion, .createdAt] | @tsv'])
first_green = None
for l in out.splitlines():
    parts = l.split("\t")
    if len(parts) >= 5 and parts[4] == "success":
        first_green = parts[0]
        break
print("  ultimo run verde: %s" % first_green)
if first_green:
    rc, jobs = cmd("gh run view %s --json jobs" % first_green,
                   ["gh", "run", "view", first_green, "--json", "jobs", "--jq", ".jobs[] | [.databaseId, .conclusion, .name] | @tsv"])
    for l in jobs.splitlines():
        jid, concl, name = (l.split("\t") + ["", "", ""])[:3]
        if "Tests" in name:
            cmd("gh run view --job %s --log | grep -E 'Passed!|Failed!'   (%s)" % (jid, name),
                ["gh", "run", "view", "--job", jid, "--log"], keep=lambda s: "Passed!" in s or "Failed!" in s)
