"""Contrato de gate de F4B-PBT1B-LOSSLESS-STATE-TYPE (ASCII), desde el paquete del contrato de PB-T1 v2.6
(docs/initiatives/I-64-f4b-pbt1-gate-contract-package.md, sections 5.0, 5.3, 6.0 y 6.2) y el patron de make_pbt1a_contract.py.
El ForbiddenWriteScope de src/ sale del algoritmo de [PKC-C] (cover()): por cada entrada de src/ en HEAD se emite <dir>/ solo si
ninguna ruta permitida queda debajo; si queda alguna, se desciende; los archivos se emiten uno a uno. Los de tests/ y la raiz son
las listas curadas del section 6.0: en tests/RackCad.Tests/, la de PB-T0 (18 entradas, con sus tres clases de Core; medicion H-12);
en tests/RackCad.UI.Tests/, la de PB-T1a (la de [PKC2], las dos clases de PB-TC2 y Workspace/) mas las siete clases de PB-T1a;
la raiz, la de PB-T1a.
Antes de escribir, comprueba sobre HEAD: src igual al cierre de PB-T1a (5043b49a) y al GREEN de PB-TC2 (b5c09f0b) (section 6.0,
anclajes de PB-T1b); las anclas de ES, RMES, CS, DA y CA del registro P; y el texto exacto del esqueleto del section 5.3 del
paquete custodiado (en --dry acepta todavia la v2.4 en HEAD, cuyo esqueleto es el de la v2.6 sin ResolveWeightUnit; al escribir exige la
v2.6). Al escribir comprueba tambien que estan en HEAD las dos rutas de PBT1BC-M04 (check_v25_skeleton.py y make_pbt1b_contract-dry.txt),
que se custodian con la emision y no son autoridades.
Uso: python make_pbt1b_contract.py <IssuedUtc> <AuthorityRevision> <MainSha> [--dry] [--print]
Con --dry no escribe nada: comprueba e imprime los recuentos y los bytes (con --print, ademas, el ForbiddenWriteScope numerado).
Al custodiarlo, la ruta local del worktree se redacta como en PB-T0 ('<worktree-de-I-64>')."""
import json, os, re, sys, subprocess, hashlib, unicodedata
WT = '<worktree-de-I-64>'  # Redaccion para el repo publico: la ruta local del worktree
os.chdir(WT)
ISSUED, AR, MAIN = sys.argv[1], sys.argv[2], sys.argv[3]
TASK = 'F4B-PBT1B-LOSSLESS-STATE-TYPE'
OUT = 'artifacts/orchestration/I-64/%s/coordinator/gate-contract.json' % TASK
PILOT = 'docs/automation/evidence/I-64-pilot'
PB1A_CLOSE = '5043b49a2bbf707520296b353a204995b90a1d79'
PB1A_GREEN = '73d30982b0c0ce2b8c9213e626df0ab0d50b0475'
TC2_GREEN = 'b5c09f0bd741f67b7dd8311dce65c26e3d4cf3c4'
DRY = '--dry' in sys.argv
# las celdas vencen el 2026-12-29 (model-catalog.md:14, :51 y :97; precondicion 6 del paquete)
assert re.fullmatch(r'[0-9]{4}-[0-9][0-9]-[0-9][0-9]T[0-9][0-9]:[0-9][0-9]:[0-9][0-9]Z', ISSUED) and ISSUED[:10] < '2026-12-29', ISSUED


def git(*args):
    return subprocess.run(['git'] + list(args), capture_output=True)


def in_head(path):
    return git('cat-file', '-e', 'HEAD:' + path.rstrip('/')).returncode == 0


def show(path):
    return git('show', 'HEAD:' + path).stdout.decode('utf-8')


# Anclaje de PB-T1b (section 6.0): src no cambia desde el cierre de PB-T1a ni desde el GREEN de PB-TC2.
assert git('merge-base', '--is-ancestor', PB1A_CLOSE, 'HEAD').returncode == 0, 'HEAD no desciende del cierre de PB-T1a'
assert git('diff', '--quiet', PB1A_CLOSE, 'HEAD', '--', 'src').returncode == 0, 'src cambio desde el cierre de PB-T1a'
assert git('diff', '--quiet', TC2_GREEN, 'HEAD', '--', 'src').returncode == 0, 'src cambio desde el GREEN de PB-TC2'
assert git('diff', '--quiet', PB1A_GREEN, PB1A_CLOSE, '--', 'src', 'tests').returncode == 0

PBT1AC = PILOT + '/F4B-PBT1A-LOSSLESS-STATE-NET/coordinator/gate-contract.json'
PBT0C = PILOT + '/F4B-PBT0-CHARACTERIZATION/coordinator/gate-contract.json'
PBTCC = PILOT + '/F4B-PBTC-DECLARED-CORRECTIONS/coordinator/gate-contract.json'
PBTC2C = PILOT + '/F4B-PBTC2-DECLARED-CORRECTIONS/coordinator/gate-contract.json'
T1BC = PILOT + '/F4-T1B-PILOT-CHARACTERIZATION/coordinator/gate-contract.json'
pbt1a = json.loads(show(PBT1AC))
pbt0 = json.loads(show(PBT0C))
P_STOPS = [dict(s) for s in pbt1a['StopConditions'] if re.fullmatch(r'[SP]-\d\d', s['Id'])]
assert [s['Id'] for s in P_STOPS] == ['S-02', 'S-03', 'S-04', 'S-05', 'S-07', 'S-08', 'S-12', 'S-13', 'S-14', 'P-01', 'P-02', 'P-03', 'P-04', 'P-05', 'P-06', 'P-07', 'P-08']
assert P_STOPS[9]['Text'] == ('config.toml no longer matches the accepted Snapshot in force (today Snapshot I, '
                              'docs/automation/evidence/I-64-pilot/F4-ENVIRONMENT/snapshot-I/snapshot-i.json)')

UT, TR = 'tests/RackCad.UI.Tests/', 'tests/RackCad.Tests/'
APP = 'src/RackCad.Application/Systems/PushBack/'
ES = APP + 'PushBackEditorState.cs'
CS = APP + 'PushBackCompositeEditorState.cs'
DA = APP + 'PushBackEditorDesignAssembler.cs'
CA = APP + 'PushBackCompositeEditorAssembler.cs'
RMES = 'src/RackCad.Application/Systems/Shared/RackModuleEditSession.cs'
PRODUCT = [ES, RMES]
NEW_SRC = [APP + n for n in ('PushBackEditableState.cs', 'PushBackEditableStateComparer.cs', 'PushBackEditableStateOwnership.cs', 'PushBackEditorOutputGate.cs')]
NEW_TESTS = [TR + n for n in ('PushBackEditableStateLoadTests.cs', 'PushBackEditableStateCopyTests.cs', 'PushBackEditableStateOwnershipTests.cs',
                              'PushBackEditorOutputGateTests.cs', 'PushBackSharedReferenceGuardTests.cs')] + [UT + 'PushBackEditableStateParityTests.cs']
NEW = NEW_SRC + NEW_TESTS
ALLOWED = PRODUCT + NEW
assert len(ALLOWED) == 12 == len(set(ALLOWED))
assert all(os.path.isfile(p) and in_head(p) for p in PRODUCT), [p for p in PRODUCT if not (os.path.isfile(p) and in_head(p))]
assert all(not os.path.exists(p) and not in_head(p) for p in NEW), [p for p in NEW if os.path.exists(p) or in_head(p)]

# Las anclas de ES, RMES, CS, DA y CA (registro P del paquete v2.5; D-PBT1-02 a D-PBT1-11 y D-PBT1-15): (archivo, linea, texto contenido).
ANCHORS = [
    (ES, 65, 'private PushBackSystem workingBaseline;'),
    (ES, 71, 'public PushBackSystem WorkingBaseline => workingBaseline;'),
    (ES, 99, 'public void AdoptLoadedBaseline(PushBackSystem baseline)'),
    (ES, 105, 'LastModuleReconciliation = null;'),
    (ES, 110, 'private RackModuleEditSession moduleSession;'),
    (ES, 111, 'private RackModuleCommit moduleCommit;'),
    (ES, 112, 'private string moduleSignature;'),
    (ES, 124, 'public RackModuleEditSession ModuleSession'),
    (ES, 143, 'public RackModuleReconciliationResult LastModuleReconciliation { get; set; }'),
    (ES, 176, 'private void ReseedModuleSession()'),
    (CS, 58, 'public List<DynamicRackModuleDesign> DormantCompositeTail { get; }'),
    (CS, 61, 'public List<DynamicHeaderLineOverride> DormantTailLineOverrides { get; }'),
    (CS, 74, 'public void ParkDormantTail('),
    (CS, 116, 'public void MarkDormantTailConsumed()'),
    (CS, 145, 'public bool DormantTailConsumptionPending => dormantTailConsumed;'),
    (CS, 147, 'private bool dormantTailConsumed;'),
    (CS, 629, 'public IReadOnlyList<PushBackCompositeDiagnostic> IntentDiagnostics()'),
    (RMES, 16, 'public sealed class RackModuleCommit'),
    (RMES, 18, 'internal RackModuleCommit('),
    (RMES, 74, 'private readonly RackFrameProjectStore clone = new RackFrameProjectStore();'),
    (RMES, 82, 'private Dictionary<LineKey, RackFrameConfiguration> committedLines'),
    (RMES, 83, 'private Dictionary<LineKey, RackFrameConfiguration> workingLines'),
    (RMES, 732, 'private Dictionary<LineKey, RackFrameConfiguration> CopyLines('),
    (RMES, 744, 'private static bool AreEquivalentDerivedPosts('),
    (RMES, 750, 'private bool AreEquivalentLines('),
    (RMES, 818, 'private bool AreEquivalent('),
    (DA, 57, 'private static IReadOnlyList<DynamicRackModuleDesign> ModuleIntents('),
    (DA, 101, 'public void AcceptComputation(PushBackEditorState state, PushBackEditorComputation computation)'),
    (DA, 169, 'var postId = string.IsNullOrWhiteSpace(editorInputs.PostCatalogId)'),
    (DA, 176, 'var pallet = ClonePallet(editorInputs.Pallet);'),
    (DA, 213, '|| DynamicEditorDesignAssembler.MustRebuild(rebuildShape, pallet, depthLayout);'),
    (DA, 218, 'system = builder.BuildDefault(pallet, depthLayout, RackFrameTemplateCatalog.Default, postId, headerHeight, postPeralte);'),
    (DA, 273, 'system = CopyStructureSystem(baseline);'),
    (DA, 302, 'PushBackAdvancedRackParameters.Reset(editorInputs);'),
    (DA, 325, 'state.LastModuleReconciliation = reconciliation.Reconcile(intents, system, restoredIds);'),
    (DA, 557, 'system.Modules.Add(new DynamicRackModule'),
    (DA, 565, 'AssociatedFrameConfiguration = headerClone.DeepCopy(module.HeaderConfiguration),'),
    (CA, 180, 'private static void ParkTailOf(PushBackCompositeEditorState state)'),
    (CA, 241, 'state.ParkDormantTail(parked, lines);'),
]
_text = {p: show(p).replace('\r\n', '\n').split('\n') for p in {a[0] for a in ANCHORS}}
for path, line, text in ANCHORS:
    assert text in _text[path][line - 1].strip(), ('ancla', path, line, _text[path][line - 1].strip())


def ascii_fold(s):
    s = s.replace(chr(0xAB), '"').replace(chr(0xBB), '"').replace(chr(0x2014), '-').replace(chr(0x2013), '-').replace(chr(0x2192), '->')
    s = ''.join(c for c in unicodedata.normalize('NFKD', s) if not unicodedata.combining(c))
    assert all(ord(c) < 0x80 for c in s), [c for c in s if ord(c) >= 0x80]
    return s


PKG = 'docs/initiatives/I-64-f4b-pbt1-gate-contract-package.md'
MEAS = 'docs/initiatives/I-64-f4b-pbt1-measurements.md'
MARK = '**El esqueleto exacto:**'


def skeleton_of(text):
    lines = text.replace('\r\n', '\n').split('\n')
    assert lines.count(MARK) == 1, 'el marcador del esqueleto no es unico'
    out = []
    for l in lines[lines.index(MARK) + 1:]:
        if not l.startswith('- '):
            break
        out.append(l)
    return ascii_fold('\n'.join(out))


# El esqueleto exacto del section 5.3 del paquete v2.6, el mismo de la v2.5 (texto ASCII: sin tildes). La v2.4 es este texto sin C09_FRAG.
SKELETON = '\n'.join([
    '- `PushBackEditableState`: todos los miembros con sus valores iniciales; `LoadNewEditor`, `LoadDesignEditor`, `RestoreEditor`, `LoadNew`, '
    '`LoadDesign` y `Restore` devuelven `PushBackEditorInputs.NewDesign()` sin hacer nada mas; `AdoptCarriers` y `SeedDefaultSafety` no hacen nada; '
    '`BuildInputs` devuelve `PushBackEditorInputs.NewDesign()`; `ResolvePostCatalogId` devuelve `requested`; `ResolveWeightUnit` devuelve `requested`; '
    '`SupportOf` devuelve `Supported`; `SideAOnly` devuelve `design`;',
    '- `PushBackEditableStateComparer`: `Instance`; `Equals` = `ReferenceEquals(x, y)`; `GetHashCode` = 0; `Copy` = `new PushBackEditableState()`;',
    '- `PushBackEditableStateOwnership`: `Table` vacia; `Normalize` devuelve su argumento; `Of` devuelve `Unclassified`;',
    '- `PushBackEditorOutputGate`: `Diagnostics` vacia; `IsBlocked` = false; `StatusOr` = `fallback`;',
    '- ES `CopyAuthorityFrom` no hace nada; RMES `Copy()` = `RackModuleEditSession.Begin(Array.Empty<DynamicRackModuleDesign>())`; `SameEditAs` = false; '
    '`RackModuleCommit.Copy()` = `this`; `SameAs` = false.',
])
C09_FRAG = ' `ResolveWeightUnit` devuelve `requested`;'
assert SKELETON.count(C09_FRAG) == 1
_head_skel = skeleton_of(show(PKG))
SKELETON_IN_HEAD = 'v2.6' if _head_skel == SKELETON else ('v2.4' if _head_skel == SKELETON.replace(C09_FRAG, '') else None)
assert SKELETON_IN_HEAD is not None, ('el esqueleto de HEAD no es ni el de la v2.6 ni el de la v2.4', _head_skel)
SKELETON_FLAT = ' / '.join(l[2:] for l in SKELETON.replace('`', '').split('\n'))


def tracked(prefix):
    out = git('ls-tree', '-r', '--name-only', 'HEAD', '--', prefix).stdout.decode('utf-8').split('\n')
    return [p for p in out if p and p.startswith(prefix)]


def cover(prefix):
    """Prefijos y archivos minimos que cubren todo prefix salvo las rutas permitidas (algoritmo de [PKC-C])."""
    inside = [a for a in ALLOWED if a.startswith(prefix)]
    if not inside:
        return [prefix]
    out = []
    names = sorted({p[len(prefix):].split('/')[0] + ('/' if '/' in p[len(prefix):] else '') for p in tracked(prefix)})
    for n in names:
        if n.endswith('/'):
            out += cover(prefix + n)
        elif prefix + n not in ALLOWED:
            out.append(prefix + n)
    return out


SRCF = cover('src/')
for a in ALLOWED:
    assert not any(a == f or (f.endswith('/') and a.startswith(f)) for f in SRCF), a
for p in tracked('src/'):
    assert p in ALLOWED or any(p == f or (f.endswith('/') and p.startswith(f)) for f in SRCF), ('sin cubrir', p)
assert len(SRCF) == 117, len(SRCF)
assert CS in SRCF and DA in SRCF and CA in SRCF and APP + 'Pilot/' in SRCF and 'src/RackCad.Application/Workspace/' in SRCF
assert {'src/RackCad.Domain/', 'src/RackCad.Plugin/', 'src/RackCad.UI/'} <= set(SRCF)
# lo que INV-PBT1B-01 exige vacio cae entero en el ForbiddenWriteScope
for p in ('src/RackCad.UI/', 'src/RackCad.Plugin/', 'src/RackCad.Domain/', APP + 'Pilot/', 'src/RackCad.Application/Workspace/', CS):
    assert p in SRCF, p

# tests/RackCad.Tests/: la lista curada de PB-T0 (medicion H-12): sus 15 entradas prohibidas de Core y sus 3 clases de Core = 18.
CORE_T0 = [f for f in pbt0['ForbiddenWriteScope'] if f.startswith(TR)] + [f for f in pbt0['AllowedWriteScope'] if f.startswith(TR)]
assert len(CORE_T0) == 18 == len(set(CORE_T0)), len(CORE_T0)
# tests/RackCad.UI.Tests/: la de PB-T1a (53 archivos y Workspace/) mas sus siete clases nuevas.
T1A_UI = [f for f in pbt1a['ForbiddenWriteScope'] if f.startswith(UT)]
assert len(T1A_UI) == 54 and T1A_UI[-1] == UT + 'Workspace/', len(T1A_UI)
T1A_NEW = [f for f in pbt1a['AllowedWriteScope'] if f.startswith(UT)]
assert len(T1A_NEW) == 7 and all(f.startswith(UT + 'PushBackStateNet') for f in T1A_NEW), T1A_NEW
UI_T = T1A_UI[:-1] + T1A_NEW + [UT + 'Workspace/']
assert len(UI_T) == 61 == len(set(UI_T)), len(UI_T)
TESTF = CORE_T0 + UI_T
assert len(TESTF) == 79
assert not any(a == t or (t.endswith('/') and a.startswith(t)) for a in ALLOWED for t in TESTF)
ROOT = [f for f in pbt1a['ForbiddenWriteScope'] if not f.startswith(('src/', 'tests/'))]
assert ROOT == ['docs/', '.github/', 'eng/', 'tools/', 'assets/', 'deploy/', 'RackCad.sln', 'Directory.Build.props', 'Directory.Build.targets', 'global.json',
                'NuGet.Config', 'README.md', 'AGENTS.md', 'CLAUDE.md', '.editorconfig', '.gitattributes', '.gitignore'], ROOT
FORBIDDEN = SRCF + TESTF + ROOT
assert len(FORBIDDEN) == 213 == len(set(FORBIDDEN)), len(FORBIDDEN)
for f in FORBIDDEN:
    assert os.path.exists(f.rstrip('/')) and in_head(f), ('no existe', f)

PHASE = 'docs/initiatives/I-64-f4b-push-back-completo-package.md'
CO = PILOT + '/F4B-DRAFTS/coordinator/'
D1 = PILOT + '/F4B-DRAFTS/pbt1/'
SC = PILOT + '/F4B-DRAFTS/superseding-characterization.md'
CONTRACT_REL = PILOT + '/%s/coordinator/gate-contract.json' % TASK
NEWS_SRC = 'PushBackEditableState, PushBackEditableStateComparer, PushBackEditableStateOwnership y PushBackEditorOutputGate'
NEWS_TESTS = ('PushBackEditableStateLoadTests, PushBackEditableStateCopyTests, PushBackEditableStateOwnershipTests, PushBackEditorOutputGateTests y '
              'PushBackSharedReferenceGuardTests (' + TR + ') y PushBackEditableStateParityTests (' + UT + ')')
P = 'POSTE_OMEGA_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA'
INV = [
    'INV-PBT1B-01 (alcance): el diff BaseSha..CurrentSha cae dentro de las 12 rutas del AllowedWriteScope; en src/, solo las seis de producto (' + ES + ', '
    + RMES + ' y los cuatro nuevos de ' + APP + ': ' + NEWS_SRC + '); git diff BaseSha..CurrentSha -- src/RackCad.UI src/RackCad.Plugin src/RackCad.Domain '
    'src/RackCad.Application/Systems/PushBack/Pilot src/RackCad.Application/Workspace ' + CS + ' vacio. BaseSha es el commit de emision de este contrato, '
    'sobre el cierre de PB-T1a (5043b49a), con src igual al GREEN de PB-TC2 (b5c09f0b) y al de PB-T1a (73d30982).',
    'INV-PBT1B-02 (RED y GREEN): el RED es el esqueleto exacto del section 5.3 del paquete (' + SKELETON_FLAT + ') y las seis clases de prueba nuevas ('
    + NEWS_TESTS + '); el GREEN solo toca src/. Criterio mecanico del RED, sobre cada filtro con ExpectRed y con los resultados de la CI del RED: todos '
    'Failed; un Message sin el prefijo de tipo ^(?:[A-Za-z_][\\w`]*[.+])+[A-Za-z_][\\w`]* : ; la primera linea de StackTrace en un archivo bajo tests/; '
    'ninguno NotExecuted ni Skipped; y Failed == Selected >= el MinSelected de su filtro (13, 1 y 1). El filtro de las guardas de D2 '
    '(PushBackSharedReferenceGuardTests, CT-PBT1B-14 y 15) pasa en el RED y en el GREEN, y sus autopruebas en negativo fallan sobre el texto inyectado '
    '(INV-F4T3A-13). En el GREEN, todos en verde.',
    'INV-PBT1B-03 (la API): los miembros, las firmas y los valores iniciales son exactamente los de D-PBT1-02 a D-PBT1-11 del paquete, con D-PBT1-09b '
    '(ResolveWeightUnit, la regla de c-09); ningun otro miembro publico nuevo. Los ayudantes privados nuevos son solo LoadComposite en '
    'PushBackEditableState, CopyLiteral en la sesion y CopyModule en el commit, mas los privados del comparador que copian literal los del piloto '
    '(PushBackPilotStateComparer.cs:64-83 y :134-350).',
    'INV-PBT1B-04 (ningun campo nuevo): ningun campo de instancia nuevo en ' + CS + ', ' + ES + ', PushBackEditorInputs, PalletSpecification, '
    'DynamicAnnotationOptions, los tipos de seguridad ni ' + RMES + ' (el censo del piloto en verde y git diff -U0 sin declaraciones de campo de instancia '
    'anadidas en ES ni en RMES); ningun campo estatico nuevo tampoco: el RackFrameProjectStore de RackModuleCommit y el de PushBackEditableState son '
    'locales al metodo (N-M-06).',
    'INV-PBT1B-04b (espacio de nombres): cada archivo nuevo de src/ declara un solo namespace RackCad.Application.Systems.PushBack de bloque; las pruebas, '
    'namespace RackCad.Tests o RackCad.UI.Tests (NamespaceFolderGuardTests).',
    'INV-PBT1B-05 (M-06, solo sobre src/): ningun archivo nuevo de src/ contiene .RequestUpdate( ni nombra RackCad.UI, System.Windows, Autodesk, '
    'Workspace, Pilot ni RackSystemKind.',
    'INV-PBT1B-06 (paso 1, sobre BaseSha y antes del RED): una corrida temporal de UI, fuera de todo commit, que termina en Assert.Fail. (c-02) Con '
    'pb-01-base y su HeaderPostCatalogId cambiado, la linea "<caso> PostBox.SelectedId=<v> salida=<HeaderPostCatalogId de LastComputation.Design>", con '
    'P = ' + P + ': (a) P -> "a PostBox.SelectedId=P salida=P"; (b) P en minusculas -> "b PostBox.SelectedId=<null> salida=P" (rama ordinal) o '
    '"b PostBox.SelectedId=P salida=P" (rama sin mayusculas); (c) PB0_POSTE_INEXISTENTE -> "c PostBox.SelectedId=<null> salida=P"; (d) en blanco -> '
    '"d PostBox.SelectedId=P salida=P"; (e) tres espacios -> "e PostBox.SelectedId=P salida=P". (c-09) En una ventana nueva, Cargar(w, <el sobre de '
    'pb-01-base con Structure.PalletWeightUnit = ton>) (EditInner); despues WeightUnitBox.SelectedItem = lb; Restaurar (Click(w, RestoreButton)); y '
    'SetNumberR(w, DepthBox, 54); la linea "f carga=[<WeightUnitBox.SelectedItem tras cargar>] WeightUnitBox.SelectedItem=<v tras Restaurar> '
    'antes=<PalletWeightUnit de LastComputation.Design tras Restaurar, sin comillas> salida=<PalletWeightUnit de LastComputation.Design tras DepthBox=54, '
    'sin comillas>", que se comprueba a si misma como F-03 (PBT1BC-M01): carga=[kg] y antes=ton se esperan en las dos ramas. (f) -> '
    '"f carga=[kg] WeightUnitBox.SelectedItem=lb antes=ton salida=lb" (rama conserva) o "f carga=[kg] WeightUnitBox.SelectedItem=kg antes=ton salida=kg" '
    'o "f carga=[kg] WeightUnitBox.SelectedItem=<null> antes=ton salida=kg" (rama kg). Se transcribe en la entrega. La fila (b) fija la rama de la regla '
    'de c-02 (D-PBT1-09) y la fila (f) la de la regla de c-09 (D-PBT1-09b); el Worker declara las dos, las aplica y usa las lineas esperadas de esas ramas '
    'en CT-PBT1B-03 y CT-PBT1B-17; cualquier otra linea, tambien un carga o un antes distintos, es PBT1B-C14.',
    'INV-PBT1B-07 (las redes): T1B, PB-T0, PB-TC, PB-TC2, PB-T1a (PushBackStateNet), Cantilever, las guardas del section 4 del paquete y el censo del '
    'piloto, en verde en el GREEN; ninguna prueba existente se edita, se salta, se reactiva ni se borra.',
    'INV-PBT1B-07c (D2, F4B-E11; D-PBT1-15): CT-PBT1B-06, 07, 08, 14, 15 y 16 en verde en el GREEN; la clase D del censo de CT-PBT1B-06 son exactamente '
    'PushBackEditorState.workingBaseline y PushBackEditorState.LastModuleReconciliation (condicion 1); lo que una copia comparte con su original son solo '
    'los no nulos de esos cuatro miembros y lo que ellos alcanzan.',
    'INV-PBT1B-08 (las pruebas de UI): la clase nueva de UI, public sealed, en [Collection(EditorDiscardPromptCollection.Name)], con [Fact] que acaban en '
    'un Assert.Equal; Ventana(), la carga dentro del try y Cerrar en finally; ninguna ventana mostrada; el aviso de descarte sustituido, con el sufijo '
    '" descartes=<n>" en la ultima linea de cada ventana, leido tras cerrarla; sin teclado ni foco real; prohibidos un MessageBox real, el BOM, Guardar '
    'en biblioteca, el configurador real e Insertar; ninguna asercion dentro de una costura; ningun fixture nuevo en Fixtures/I64; los disenos '
    'fabricados (X-01, F-01 y los de CT-PBT1B-01, 12 y 17), solo en codigo.',
    'INV-PBT1B-09 (determinismo): el GUID fijo, la cultura invariante, los numeros con "R", el orden ordinal; ni hora ni aleatoriedad.',
    'INV-PBT1B-10 (las corridas locales, F4-E59): solo los filtros de RequiredTests en UI, nunca la suite completa de UI en local; la de Core, si; sin copias '
    'de arboles (MAX_PATH); las corridas temporales terminan en Assert.Fail(string.Join("\\n", lineas)), corren tambien dentro de '
    'EditorDiscardPrompt.Substitute (leccion IRC-M02 del cierre de PB-TC2), no entran en ningun commit y guardan TRX y log como artefactos; el perfil del '
    'usuario no cambia (INV-PBT0-06).',
    'INV-PBT1B-11 (la evidencia): toda ruta de worker-handoff.json es relativa y se comprueba que existe; TreeSha es el arbol real (git write-tree) o null; '
    'el Worker no escribe en docs/; una sustitucion se llamaria SUS3B-0n y es PBT1B-C02.',
    'INV-PBT1B-12 (reglas de F4B, [DP], D-PB-10 y sin AutoCAD): la planificacion segun PBT1B-C08; la verificacion con las tres comprobaciones '
    'deterministas de F4B-E15; los controles segun PBT1B-C11; antes de un intento 3, siempre el Owner (PBT1B-C12); la comprobacion fisica es de Smoke-PB.',
    'INV-PBT1B-14 (las dos referencias de D2, F4B-E11): fuera de PushBackEditorState.workingBaseline y PushBackEditorState.LastModuleReconciliation de cada '
    'lado, ninguna copia del estado comparte un objeto mutable con su original; ninguna linea anadida muta un PushBackSystem ni un '
    'RackModuleReconciliationResult adoptado (las reglas de G-L1 de la v2.4, D-PBT1-15, que dan 0 coincidencias sobre BaseSha en los archivos de '
    'CT-PBT1B-14, registro P).',
]
INV_IDS = ['01', '02', '03', '04', '04b', '05', '06', '07', '07c', '08', '09', '10', '11', '12', '14']
assert len(INV) == 15 and [i.split(' ')[0] for i in INV] == ['INV-PBT1B-' + n for n in INV_IDS]
EVID = [
    'Antes del Worker: gate-contract valido; delegacion del Controller valida; comparacion byte a byte, aceptacion A1-A8 y nc4 (primera delegacion de la '
    'cadena F4B-PBT1B); AllowedWriteScope de la delegacion = exactamente el del contrato; si hubo una replanificacion por PBT1B-C08, su registro.',
    'Paso 1, sobre BaseSha y antes del RED (INV-PBT1B-06): la corrida temporal de UI con Assert.Fail(string.Join("\\n", lineas)), con las filas (a) a (f), '
    'su TRX y su log como artefactos, y su transcripcion en la entrega con la rama de c-02 y la de c-09 declaradas.',
    'Commit RED y push propios antes del GREEN: el esqueleto exacto del section 5.3 del paquete y las seis clases de prueba nuevas; el diff del esqueleto; '
    'CI del RED con tests y ui-tests en failure y build-ui y build-plugin en success, sin blame-hang ni timeout; los TRX de cada filtro con ExpectRed cumplen '
    'el criterio de INV-PBT1B-02; la tabla CT -> la asercion que falla en el RED; y la salida de las autopruebas en negativo de las dos guardas de D2 '
    '(CT-PBT1B-14 y 15), que pasan en el RED.',
    'Commit GREEN y push: el diff dentro de las 12 rutas y, en src/, solo las seis de producto (INV-PBT1B-01); CI del GREEN 4/4 con las suites completas, '
    'con los conteos de la section 6.2 del paquete (UI 2437 resultados: 0 Failed, 21 Skipped y 2416 Passed; Core 19086 Passed) o la diferencia explicada '
    'por el diff.',
    'En local: build Release de la solucion, Plugin incluido y sin avisos nuevos; los filtros de RequiredTests sobre el RED y sobre el GREEN; tres corridas '
    'seguidas en verde de los filtros con ExpectRed sobre el GREEN; la prueba de la limpieza del perfil.',
    'worker-handoff.json conforme a su esquema con RED, GREEN, alcance real, trailer y conteos por filtro (Passed, Failed y Skipped); el mapa CT a metodo '
    '(CT-PBT1B-01 a 17); las ramas medidas de c-02 y de c-09; rutas relativas comprobadas; TreeSha real o null.',
    'Despues: Controller verification valida, con las tres comprobaciones deterministas de F4B-E15 (la busqueda de terminos de 16.10 por script sobre el '
    'texto completo de los siete campos libres; el alcance contado por script contra el AllowedWriteScope de la delegacion; y la evidencia de Identity con '
    'el CurrentSha de la entrega y el HEAD medido escritos literalmente); nc1, nc2 y nc3 segun README section 10, conservando cada salida ordinaria; un '
    'control que no da CONTROL PASS se resuelve SOLO como dice PBT1B-C11; cada uso se registra en F4B-DRAFTS/f4b-dispositions.md; la verificacion 14/14 '
    'sola no basta para cerrar.',
    'Sin AutoCAD: el estado se prueba en RackCad.Tests y la paridad con la ventana en UI.Tests con el producto real; la comprobacion fisica es de Smoke-PB.',
]
C11 = ('a negative control (nc1, nc2 or nc3) does not give CONTROL PASS: its ordinary output is kept as is and there are only two exits without returning to the Owner '
       '([DP] question 1, "Extender firmas exactas"): (1) nc2 with the EXACT signature of PD-F4-01 (source f4-dispositions.md:1103-1106: Scope in fail on the removed path; '
       'Contract in fail only by the exactness clause; EXECUTION_BLOCKED/STOP; applied literally at :1540: no check other than the real verification differs; the exactness '
       'clause includes Contract evidence citing the contrast of the narrowed AllowedWriteScope with the contract or with byte-fidelity-check.json, f4-dispositions.md:520, :562, '
       'and nothing else) -> substitute PBT1B-NC2-EXACT-WHITELIST-01, a copy of F4T1B-NC2-EXACT-WHITELIST-01 with ONLY the id and the scope changed, recorded as '
       'nc2 = SATISFIED_UNDER_PBT1B_EXACT_WHITELIST_SUBSTITUTE; (2) nc1 with the EXACT signature of PD-F4-04 (source owner-f4t3-authorization.md:255-265: false CurrentSha '
       'correctly detected; Identity = fail; Handoff also fails first only because the handoff itself contains the false SHA; outcome BLOCKED / STOP; all other intended checks '
       'match; as applied in F4, f4-dispositions.md:1657-1660, Ci in fail only as a consumer of the false CurrentSha is part of the signature, with Remote in pass) -> substitute '
       'PBT1B-NC1-IDENTITY-SIGNAL-01, a copy of F4T3C2-NC1-IDENTITY-SIGNAL-01 with ONLY the id and the scope changed, recorded as '
       'nc1 = SATISFIED_UNDER_PBT1B_NC1_IDENTITY_SIGNAL_SUBSTITUTE. Matching is decided never by check name: the control output is compared with the real verification check by '
       'check (the set of failing checks, the first failing check, Outcome and StopClass, and the evidence text); the only admitted differences are those listed for each signature '
       'in section 6.0 of the package; the session records the comparison table in f4b-dispositions.md. Any other signature or variation (PD-F4-05, PD-F4-06, nc2 with Ci or '
       'Trailer also in fail, any nc1 that does not reproduce the literal signature - including the PB-TC variation with Remote, Scope, Tests and Trailer also in fail, '
       'which the Owner accepted only once (F4B-E08), and the PB-TC2 case, where the substitute failed its property 5 and the Owner accepted nc1 only for PB-TC2 attempt 1 '
       '(F4B-E12) -, a substitute that does not give PASS, any failure of nc3, or an evidence text that cannot be assigned to an admitted difference) returns to the Owner '
       'before any substitute or acceptance; neither one-off acceptance extends to PB-T1b')
C17 = ('(F4B-E11, D2, IMPORTANT LIMIT) doing the task would need to MUTATE the resolved rack (PushBackSystem) or the module report (RackModuleReconciliationResult) '
       'inside the editing or draft lifetime, or to share by reference any other mutable object between a copy of the state and its original, or a measurement already '
       'finds such a mutation in the editing path: STOP and L-1 is reopened. The Owner text, literal: "This is NOT authorization for general shallow copying. It is '
       'exactly: two declared shared references. If later PB-T1/PB-T2/PB-T3 needs to mutate either: resolved rack; module report; inside the Workspace draft lifetime: '
       'STOP and reopen L-1. At that point the object must either: be cloned; be made immutable; or be replaced by a value snapshot appropriate to the draft model." '
       'In PB-T1b the copier shares by reference ONLY the non-null PushBackEditorState.workingBaseline and PushBackEditorState.LastModuleReconciliation of each side '
       '(D-PBT1-15, condition 1); anything else shared, or any added line that mutates those two objects, is this stop. The Worker chooses none of the three exits: '
       'the Owner does')
STOPS = P_STOPS + [
    {'Id': 'PBT1B-C01', 'Text': 'doing it right would require writing outside the AllowedWriteScope (ES and RMES, the four new Application files and the six new test files)'},
    {'Id': 'PBT1B-C02', 'Text': 'an existing test or helper would have to be edited, skipped, re-enabled or deleted, or InternalsVisibleTo, the packages, a .csproj or tests/RackCad.Tests/Fixtures/I64 would have to change; a guard of section 4 of the package, the pilot census or a PB-T0, PB-TC, PB-TC2 or PB-T1a test that does not stay green returns to the Owner (OB-T1B.3); a needed substitution (SUS3B-0n) is a package defect and returns to the session'},
    {'Id': 'PBT1B-C03', 'Text': 'an observable change of RACKEDITAR against the base appears (in PB-T1b the window, the pilot, the Plugin and the XAML do not change, and no existing method of ES, CS, DA, CA or RMES changes its behaviour): STOP and back to the Owner (D-16, OB-RE.1)'},
    {'Id': 'PBT1B-C04', 'Text': '(I-06) a method of a filter with ExpectRed passes on the skeleton of section 5.3 of the package, or fails by exception and not by assertion; it is a contract defect and the Worker changes neither the CT nor the skeleton'},
    {'Id': 'PBT1B-C05', 'Text': 'a CT would need AutoCAD, a real MessageBox, the real configurator, the BOM window, Guardar en biblioteca, Insertar, the real keyboard or focus, or a write to the user profile or to disk'},
    {'Id': 'PBT1B-C06', 'Text': 'the full CI of the GREEN fails outside the new tests, reproduces on a second run and is not attributable to the new tests (attributable is REWORK; not reproduced is a BLOCKED rerun within the 16.8 limits)'},
    {'Id': 'PBT1B-C07', 'Text': 'the facts of section 2 of the package or the anchors of register P do not hold on the base in a way that changes a CT or the design; or an expected line of a CT is not met on the GREEN; the Worker does not adjust the expectation'},
    {'Id': 'PBT1B-C08', 'Text': 'a planning output rejected ONLY by the literal byte-fidelity comparison of the collections before the Worker allows ONE replanning with a reinforced prompt, without consuming an attempt ([DP] question 2); a second planning fidelity rejection is STOP and returns to the Owner; any other planning rejection (A1-A8 or another cause) is STOP and returns to the Owner'},
    {'Id': 'PBT1B-C09', 'Text': 'a test would hang, leave a window, a modal or a dispatcher alive, write to real user profile data or depend on machine-specific paths'},
    {'Id': 'PBT1B-C10', 'Text': 'doing it right would require correcting something outside the scope: a T-2 loss, c-09 or OWNER-3 and OWNER-4 of F4B-E14 (reproduced, c-09 as an explicit rule), M-07, pb-21-cola, the WORKSPACE mode, the pilot, D-PB-07 or a PB-TC or PB-TC2 correction'},
    {'Id': 'PBT1B-C11', 'Text': C11},
    {'Id': 'PBT1B-C12', 'Text': 'a correction would be attempt 3 of 3: it returns to the Owner before it is issued (D-PB-10)'},
    {'Id': 'PBT1B-C13', 'Text': 'the implementation would need a new field in a type of the pilot census, to touch CS, or to change the behaviour of an existing method of ES, CS, DA, CA or RMES'},
    {'Id': 'PBT1B-C14', 'Text': 'in step 1 (INV-PBT1B-06) some salida of rows (a) to (e) is not P, or a row is none of its expected lines (in (b), none of the two branches of the c-02 rule; in (f), none of the two branches of the c-09 rule): STOP before the RED; the rule returns to the session'},
    {'Id': 'PBT1B-C15', 'Text': 'CT-PBT1B-01 or 02 gives a difference between the state and the window of today, or a loss in a supported rack, that does not come from the skeleton: STOP; it is a new fact and returns to the Owner by OB-06.5'},
    {'Id': 'PBT1B-C17', 'Text': C17},
]
assert [s['Id'] for s in STOPS[17:]] == ['PBT1B-C%02d' % n for n in range(1, 16)] + ['PBT1B-C17'] and len(STOPS) == 33
UIP, CP = UT + 'RackCad.UI.Tests.csproj', TR + 'RackCad.Tests.csproj'
REQ = [
    (CP, 'FullyQualifiedName~RackCad.Tests.PushBackEditableState', 13, True),
    (CP, 'FullyQualifiedName~RackCad.Tests.PushBackEditorOutputGate', 1, True),
    (UIP, 'FullyQualifiedName~RackCad.UI.Tests.PushBackEditableStateParityTests', 1, True),
    (CP, 'FullyQualifiedName~RackCad.Tests.PushBackSharedReferenceGuardTests', 2, False),
    (CP, 'FullyQualifiedName~RackCad.Tests.Workspace', 3271, False),
    (CP, 'FullyQualifiedName~RackCad.Tests.PushBackOutputGateTests', 8, False),
    (CP, 'FullyQualifiedName~RackCad.Tests.PushBackBlankFrontAuthorityTests', 9, False),
    (CP, 'FullyQualifiedName~RackCad.Tests.PushBackModuleEditorCharacterizationTests', 15, False),
    (CP, 'FullyQualifiedName~RackCad.Tests.RackModuleEditSessionTests', 25, False),
    (CP, 'FullyQualifiedName~RackCad.Tests.NamespaceFolderGuardTests', 7, False),
    (CP, 'FullyQualifiedName~RackCad.Tests.HeaderBatchContractTests', 17, False),
    (UIP, 'FullyQualifiedName~RackCad.UI.Tests.PushBackStateNet', 14, False),
    (UIP, 'FullyQualifiedName~RackCad.UI.Tests.PushBackClassicCharacterization', 55, False),
    (CP, 'FullyQualifiedName~RackCad.Tests.PushBackClassicCharacterization', 10, False),
    (UIP, 'FullyQualifiedName~RackCad.UI.Tests.PushBackFullEditorCharacterization', 16, False),
    (CP, 'FullyQualifiedName~RackCad.Tests.PushBackFullEditorCharacterization', 3, False),
    (UIP, 'FullyQualifiedName~RackCad.UI.Tests.PushBackDeclaredCorrectionRoundingTests', 3, False),
    (UIP, 'FullyQualifiedName~RackCad.UI.Tests.PushBackDeclaredCorrectionDimensionStyleTests', 2, False),
    (UIP, 'FullyQualifiedName~RackCad.UI.Tests.PushBackDeclaredCorrectionPilotParityTests', 2, False),
    (UIP, 'FullyQualifiedName~RackCad.UI.Tests.PushBackDeclaredCorrectionSideBNoticeTests', 2, False),
    (UIP, 'FullyQualifiedName~RackCad.UI.Tests.PushBackDeclaredCorrectionStandardRestoreTests', 1, False),
    (UIP, 'FullyQualifiedName~RackCad.UI.Tests.PushBackDeclaredCorrection2', 4, False),
    (UIP, 'FullyQualifiedName~RackCad.UI.Tests.Cantilever', 67, False),
]
assert len(REQ) == 23 == len({(r[0], r[1]) for r in REQ}) and [r for r in REQ if r[3]] == REQ[:3]
for proj, _, _, _ in REQ:
    assert in_head(proj), proj
# los prefijos nuevos estan libres y no son prefijo de ninguna clase existente; PushBackEditorOutputGate no es prefijo de PushBackOutputGateTests
for pref in ('PushBackEditableState', 'PushBackEditorOutputGate', 'PushBackSharedReferenceGuardTests'):
    assert not [p for p in tracked('tests/') if pref in p], ('el prefijo ya existe', pref)
# los filtros heredados de PB-T1a, con sus MinSelected (section 6.1 del paquete)
_t1a_req = {(r['Project'], r['Filter']): r['MinSelected'] for r in pbt1a['RequiredTests']}
for proj, f, m, _ in REQ[11:]:
    assert _t1a_req.get((proj, f)) == m, (f, m, _t1a_req.get((proj, f)))
cells = json.loads(json.dumps(pbt1a['EligibleCells']))
assert [c['Cell'] for c in cells] == ['anthropic/subagent/claude-sonnet-5-5', 'openai/process/gpt-6-luna']
assert cells == json.loads(show(PBTC2C))['EligibleCells'], 'las celdas difieren de las de PB-TC2'
UNIT_CHANGE = [
    (ES, 'PushBackEditorState: solo el metodo nuevo CopyAuthorityFrom (D-PBT1-07), sin campos nuevos'),
    (RMES, 'RackModuleEditSession y RackModuleCommit: solo los metodos nuevos Copy y SameEditAs, y Copy y SameAs (D-PBT1-07), sin campos nuevos'),
]
EXISTING = [
    (PHASE, 'paquete de la fase F4B v3.1: OB-01.1b, OB-03.1, OB-05.1, OB-06.5, OB-10.1, OB-T1B.1 a OB-T1B.3, OB-RE.1, H-PB-27, R-PB-01, R-PB-05 y R-PB-09; la fila de PB-T1 del section 5'),
    (CO + 'owner-f4b-disposition.md', 'archivo completo ([OD]): la fila de PB-T1, D-PB-05, D-PB-06 y D-PB-10'),
    (CO + 'owner-f4b-procedure-decisions.md', 'archivo completo ([DP], F4B-E03): preguntas 1 y 2 (firmas exactas de los sustitutos y la replanificacion unica)'),
    (CO + 'owner-pbt1-pre-decisions.md', 'archivo completo ([OPT1], F4B-E09): pregunta 2 (c-02 como regla explicita), pregunta 3 (pb-21-cola fuera de la garantia) y pregunta 4 (la autorizacion condicional de emision)'),
    (CO + 'owner-d1-d2-decisions.md', 'archivo completo ([OD2], F4B-E11): D2 con sus diez condiciones y su limite, y PB-T1 PLAN'),
    (CO + 'owner-pbt1a-decisions.md', 'archivo completo (F4B-E14): pregunta 2 (c-09 como regla explicita de PB-T1b, con su CT) y pregunta 3 (OWNER-3 y OWNER-4 tal cual); la precondicion 5 satisfecha'),
    (CO + 'owner-pbt1a-nc1-decisions.md', 'archivo completo (F4B-E15): las tres comprobaciones deterministas de la verificacion, que valen tambien para PB-T1b'),
    (CO + 'owner-pbtc-nc1-decision.md', 'la disposicion unica de nc1 en PB-TC (F4B-E08), que no se extiende a PB-T1b'),
    (CO + 'owner-pbtc2-nc1-decisions.md', 'la disposicion de nc1 en PB-TC2 solo para su intento 1 (F4B-E12), que no se extiende a PB-T1b'),
    (PILOT + '/F4B-DRAFTS/f4b-dispositions.md', 'archivo completo: F4B-E01 a F4B-E16'),
    (PILOT + '/F4B-DRAFTS/pbt1a/owner-return-pbt1a.md', 'la vuelta al Owner tras el Worker de PB-T1a: los hechos de c-09 al reconstruir (OWNER-2), OWNER-3 y OWNER-4'),
    (D1 + 'ob065-dossier.md', 'el expediente del analista: section 3 (los huecos de red) y section 4 (las guardas de T1B y de texto)'),
    (D1 + 'owner-return-l1.md', 'la vuelta L-1 al Owner, respondida por F4B-E11 (D2)'),
    (D1 + 'independent-review-v1.md', 'la revision independiente IRT1-1 de la v1'),
    (D1 + 'independent-review-v2-check.md', 'la comprobacion acotada de la v2'),
    (D1 + 'independent-review-v23-check.md', 'la comprobacion acotada de la remedicion (v2.3), aplicada en la v2.4'),
    (SC, 'el registro declarado ([SC]): PB-TC y PB-TC2 EJECUTADAS'),
    ('docs/initiatives/I-64-f4b-pbt0-gate-contract-package.md', 'el paquete de PB-T0: CT-PB0-01 (las 667 hojas de pb-21-cola), CT-PB0-17 (T-2: c-01 a c-16), M-01 y D-3'),
    (PBT0C, 'el contrato de PB-T0: la lista curada de tests/RackCad.Tests/ (medicion H-12)'),
    (PILOT + '/F4B-PBT0-CHARACTERIZATION/task-close-package.md', 'el cierre de PB-T0'),
    ('docs/initiatives/I-64-f4b-pbtc-gate-contract-package.md', 'el paquete de PB-TC v2.2 ([PKC]): el algoritmo del ForbiddenWriteScope ([PKC-C]) y CT-PBTC-06'),
    (PBTCC, 'el contrato de PB-TC'),
    ('docs/initiatives/I-64-f4b-pbtc2-gate-contract-package.md', 'el paquete de PB-TC2 v2 ([PKC2]): la celda del Controller (:655) y la forma de los alcances'),
    ('docs/initiatives/I-64-f4b-pbtc2-measurements.md', 'el registro de mediciones de PB-TC2'),
    (PBTC2C, 'el contrato de PB-TC2 ([PKC2-C]): las paradas estandar con P-01 y las celdas'),
    (PILOT + '/F4B-PBTC2-DECLARED-CORRECTIONS/task-close-package.md', 'el cierre de PB-TC2 sobre b5c09f0b, el producto de la BaseSha de PB-T1b'),
    (PBT1AC, 'el contrato de PB-T1a: las paradas estandar, las celdas, la raiz y la lista curada de tests/RackCad.UI.Tests/, y los filtros heredados'),
    (PILOT + '/F4B-PBT1A-LOSSLESS-STATE-NET/task-close-package.md', 'el cierre de PB-T1a sobre 73d30982 (F4B-E16): la red PushBackStateNet (CT-PBT1A-01 a 14) y CT-PBT1A-02, el oraculo de CT-PBT1B-17'),
    ('docs/initiatives/I-64-f4-t1b-gate-contract-package.md', 'el patron de T1B ([T1B]): la caracterizacion vigente de la ventana clasica (55 casos en UI y 10 en Core)'),
    ('docs/initiatives/I-64-f4-t2-gate-contract-package.md', 'el regimen del RED con esqueleto inerte de los tipos nuevos, que compila, no lanza y falla por asercion (:282) (PBT1BC-M03)'),
    ('docs/initiatives/I-64-f4-t3-gate-contract-package.md', 'INV-F4T3A-13 (:371-374): el esqueleto inerte; las guardas pueden pasar en el RED y discriminan con autopruebas (PBT1BC-M03)'),
    ('docs/initiatives/I-64-f4-t3c-gate-contract-package.md', 'la regla RED (:297): el esqueleto completo y compilable, que no lanza y no cambia el comportamiento (PBT1BC-M03)'),
    (T1BC, 'el contrato de T1B ([T1B-C])'),
    (PILOT + '/F4-DRAFTS/f4-dispositions.md', 'las fuentes de PD-F4-01 (:1103-1106, :1540, :520, :562) y PD-F4-04 (:1657-1660), F4-E56 (:1917-1928) y F4-E59'),
    (PILOT + '/F4-DRAFTS/coordinator/owner-f4t3-authorization.md', 'la firma literal de PD-F4-04 (:255-265)'),
    ('docs/automation/state/I-64.yml', 'estado vigente completo'),
    ('docs/automation/evidence/I-64-evidence.md', 'las secciones de F4B (231 y siguientes)'),
]
CUSTODY = [  # se custodian en el commit de emision de PB-T1b (section 6.0 del paquete, Custodia)
    (PKG, 'archivo completo (v2.6): section 2 (hechos), section 3 (D-PBT1-02 a D-PBT1-11 con D-PBT1-09b, la regla de c-09, y D-PBT1-15), section 5.0 y 5.3 (recetas, el esqueleto exacto y CT-PBT1B-01 a 17), section 6.0 y 6.2 (el contrato), section 8 (decisiones del Owner y los anclajes de PB-T1b) y section 10 (arrastres)'),
    (MEAS, 'el registro de mediciones del paquete, apartados A a P (P: los anclajes de PB-T1b sobre el cierre de PB-T1a, 5043b49a)'),
    (D1 + 'pbt1b-anchors-out.txt', 'la salida de los anclajes de PB-T1b (registro P): identidad, anclas, G-L1, recuentos y la linea base de la CI'),
    (D1 + 'independent-review-pbt1b-check.md', 'la comprobacion acotada de PB-T1b (section 6.0 del paquete)'),
]
assert len(CUSTODY) == 4 and len({p for p, _ in CUSTODY}) == 4 and not {p for p, _ in CUSTODY} & {p for p, _ in EXISTING}
# PBT1BC-M04: se custodian con la emision en F4B-DRAFTS/pbt1/, sostienen el registro P-7 y no son autoridades del contrato.
CUSTODY_EXTRA = [D1 + 'check_v25_skeleton.py', D1 + 'make_pbt1b_contract-dry.txt']
EXTERNAL = [
    ('docs/adr/0029-contrato-funcional-comun-de-ventanas-wpf.md', 'D12 y D13 (sin modelo paralelo; la caracterizacion no se edita para que pase)'),
    ('docs/AUTOMATION_PLAN.md', 'section 16.5, 16.8, 16.9, 16.10 y 16.11'),
    ('docs/automation/agent-execution/README.md', 'section 4 (A1-A8) y section 10 (nc1..nc4)'),
    ('docs/automation/agent-execution/routing.md', 'archivo completo'),
    ('docs/automation/agent-execution/model-catalog.md', 'celdas medidas y vigentes para Controller y Worker'),
]
for p, _ in EXISTING + EXTERNAL + UNIT_CHANGE:
    assert in_head(p), ('autoridad inexistente en HEAD', p)
c = {
    'Schema': 'rackcad-gate-contract/v1', 'Unit': 'I-64', 'Gate': 'F4B', 'TaskId': TASK,
    'Objective': ('Crear en Application el estado editable de Push Back sin perdidas fuera de la ventana (PushBackEditableState), con las cargas de LoadNew, '
                  'LoadExisting, LoadDesignForNew y Restaurar partidas en la de Application y AdoptCarriers, la salida BuildInputs, el copiador y el comparador '
                  'completos con modulos, cola aparcada y A/B (con la excepcion de dos referencias compartidas decidida por el Owner en F4B-E11, D2, y sus diez '
                  'condiciones de seguridad), el mapa de propiedad, las reglas explicitas de c-02 y de c-09 (F4B-E09 y F4B-E14), la exclusion de pb-21-cola y '
                  'el criterio de bloqueo de salida extraido, segun el paquete del contrato de PB-T1 v2.6, con los anclajes medidos sobre el cierre de PB-T1a '
                  '(5043b49a), y la lista cerrada CT-PBT1B-01..17. El RED es el esqueleto inerte y las pruebas nuevas, que fallan por asercion salvo las dos '
                  'guardas de D2; el GREEN implementa solo Application. Si hiciera falta mutar el rack resuelto o el informe de modulos dentro del tiempo de '
                  'vida de la edicion: STOP y se reabre L-1. La ventana, el piloto, el Plugin y el XAML no cambian; no se ejecuta AutoCAD.'),
    'AuthorityRevision': AR, 'MainSha': MAIN,
    'Authorities': [{'Path': p, 'Section': s, 'Class': 'UNIT_DOC'} for p, s in CUSTODY + EXISTING] + [
        {'Path': CONTRACT_REL, 'Section': 'este contrato', 'Class': 'UNIT_DOC'},
    ] + [{'Path': p, 'Section': s, 'Class': 'UNIT_CHANGE'} for p, s in UNIT_CHANGE]
      + [{'Path': p, 'Section': s, 'Class': 'EXTERNAL'} for p, s in EXTERNAL],
    'AllowedWriteScope': ALLOWED,
    'ForbiddenWriteScope': FORBIDDEN,
    'Invariants': INV,
    'RequiredTests': [{'Project': p, 'Filter': f, 'MinSelected': m, 'ExpectRed': e} for p, f, m, e in REQ],
    'ExpectedEvidence': EVID, 'StopConditions': STOPS, 'EligibleCells': cells, 'RoutingEnforcement': 'required', 'CorrectionsAuthorized': True,
    'IssuedBy': ('Sesion principal de I-64 (Claude) por las decisiones del Owner F4B-E09, con su autorizacion condicional de emision (pregunta 4) '
                 '(docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/owner-pbt1-pre-decisions.md), F4B-E11, D2 y PB-T1 PLAN '
                 '(docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/owner-d1-d2-decisions.md), y F4B-E14, con c-09 como regla explicita y la '
                 'precondicion 5 satisfecha, y F4B-E15, la verificacion reforzada (docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/'
                 'owner-pbt1a-decisions.md y owner-pbt1a-nc1-decisions.md): la emision de la segunda tarea de PB-T1 tras el cierre de PB-T1a (F4B-E16), '
                 'sus anclajes y su comprobacion acotada; las decisiones de procedimiento owner-f4b-procedure-decisions.md (F4B-E03); y el paquete del '
                 'contrato v2.6, con la comprobacion acotada de PB-T1b aplicada'),
    'IssuedUtc': ISSUED,
}
assert len(c['Authorities']) == 49, len(c['Authorities'])  # 46 de la v2.5 + los tres contratos de F4 con API nueva (PBT1BC-M03)
b = (json.dumps(c, indent=2, ensure_ascii=True) + '\n').encode('ascii')
assert all(x < 0x80 for x in b) and b'\\u' not in b, 'no ASCII'
assert not set(c['AllowedWriteScope']) & set(c['ForbiddenWriteScope'])
for a in c['AllowedWriteScope']:
    assert not any(a.startswith(f) for f in c['ForbiddenWriteScope'] if f.endswith('/')), a
TERMS = ['GATE PASS', 'GATE_PASS', 'Candidato', 'Candidate', 'FINAL_CANDIDATE_SHA', 'gate cerrado', 'gate closed', 'cierre del gate', 'rama integrada', 'integrada en main',
         'integrated into main', 'merged into main', 'lista para integrar', 'ready to merge', 'Owner Validation APROBADA', 'Owner Validation APPROVED']
for t in TERMS:
    assert not re.search(r'(?<![A-Za-z0-9_])' + re.escape(t) + r'(?![A-Za-z0-9_])', b.decode('ascii'), re.I), t
for bad in ('C:' + chr(92), 'C:' + '/', 'D:' + '/', 'Users', 'alej' + 'andra', 'App' + 'Data', 'scratchpad', '.co' + 'dex', 'hs' + '65'):
    assert bad not in b.decode('ascii'), bad
PKG_IN_HEAD = re.search(r', (v\d+(?:\.\d+)?)\s*$', show(PKG).split('\n')[2])
MEAS_HAS_P = '\n## P. ' in show(MEAS)
if DRY:
    custody_missing = [p for p, _ in CUSTODY if not in_head(p)]
    print('DRY allowed', len(ALLOWED), 'forbidden', len(FORBIDDEN), '(src', len(SRCF), 'tests', len(TESTF), 'root', len(ROOT), ') inv', len(INV), 'stops', len(STOPS),
          'required', len(REQ), 'evidence', len(EVID), 'authorities', len(c['Authorities']), 'anchors-ok', len(ANCHORS), 'skeleton-in-HEAD', SKELETON_IN_HEAD,
          'bytes', len(b), 'sha256', hashlib.sha256(b).hexdigest().upper())
    print('a custodiar en la emision (aun no en HEAD):', len(custody_missing), custody_missing)
    extra_missing = [p for p in CUSTODY_EXTRA if not in_head(p)]
    print('a custodiar tambien, sin ser autoridades (PBT1BC-M04; aun no en HEAD):', len(extra_missing), extra_missing)
    print('a sustituir en la emision: paquete en HEAD', PKG_IN_HEAD.group(1) if PKG_IN_HEAD else None, '(hace falta la v2.6); registro en HEAD con el apartado P:', MEAS_HAS_P)
    if '--print' in sys.argv:
        for i, f in enumerate(FORBIDDEN, 1):
            print(i, f)
    sys.exit(0)
for p, _ in CUSTODY:
    assert in_head(p), ('falta custodiar antes de escribir el contrato', p)
for p in CUSTODY_EXTRA:
    assert in_head(p), ('falta custodiar antes de escribir el contrato (PBT1BC-M04)', p)
assert SKELETON_IN_HEAD == 'v2.6', 'el paquete custodiado no tiene el esqueleto de la v2.6'
assert PKG_IN_HEAD and PKG_IN_HEAD.group(1) == 'v2.6', 'el paquete custodiado no es la v2.6'
assert MEAS_HAS_P, 'el registro custodiado no tiene el apartado P'
os.makedirs(os.path.dirname(OUT), exist_ok=True)
open(OUT, 'wb').write(b)
o = subprocess.run(['pwsh', '-NoProfile', '-Command', f"Get-Content -Raw '{OUT}' | Test-Json -SchemaFile 'docs/automation/agent-execution/schemas/gate-contract.schema.json'"], capture_output=True, text=True)
assert o.stdout.strip().endswith('True'), o.stderr[-300:]
print('schema True allowed', len(ALLOWED), 'forbidden', len(FORBIDDEN), 'inv', len(INV), 'stops', len(STOPS), 'authorities', len(c['Authorities']), 'sha256', hashlib.sha256(b).hexdigest().upper())
