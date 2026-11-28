"""PB-T4c (v5.0), fase 1: la remedicion de los anclajes de la v4.9 (D-PBT4-12 a 17, §4.3, §5.3 y §6.3) sobre el cierre de PB-T4b.

Solo objetos git (`git show`, `git ls-tree`, `git diff --name-only`); no escribe nada salvo su salida (stdout). Para cada anclaje lo busca
por su contenido (no por su numero de linea) en tres revisiones:
  A = 7f0d9aa3, el cierre de PB-T3c, donde la v4.x midio los anclajes de PB-T4c;
  B = 78a7cef3, el cierre de PB-T4a, la base de PB-T4b;
  C = 6adff90d, el cierre de PB-T4b (F4B-E53), la base de PB-T4c.
y da sus lineas en A y en C, y si su texto es igual (sha256 del texto del anclaje con los saltos a \n). Un anclaje de clase 'member' va de su
primera linea de atributo (o de su firma, si no tiene) a la llave que cierra la primera que abre; 'line', las lineas que casan; 'block', de la
linea que casa a la que casa con su fin.

Uso: python -I remeasure_pbt4c_v50.py <repo>
"""
import hashlib
import re
import subprocess
import sys

REPO = sys.argv[1]
# La salida estandar, en UTF-8 con LF (la custodia rechaza cp1252 y CRLF).
sys.stdout.reconfigure(encoding='utf-8', newline='\n')
A, B, C ='7f0d9aa3', '78a7cef3', '6adff90d'

PEG = 'tests/RackCad.Tests/Workspace/WorkspacePilotEditGuardTests.cs'
EIG = 'tests/RackCad.Tests/Workspace/WorkspaceEditIntentGuardTests.cs'
EIH = 'tests/RackCad.Tests/Workspace/WorkspaceEditIntentHostTests.cs'
URT = 'tests/RackCad.Tests/Workspace/WorkspacePilotUpdateRequestTests.cs'
IG = 'tests/RackCad.Tests/Workspace/WorkspacePilotUpdateIntegrationGuardTests.cs'
WHS = 'tests/RackCad.Tests/Workspace/WorkspaceHostSourceGuardTests.cs'
WNH = 'tests/RackCad.Tests/Workspace/WorkspaceNavigationHostTests.cs'
WBS = 'tests/RackCad.Tests/Workspace/WorkspaceBridgeSourceGuardTests.cs'
WPS = 'tests/RackCad.Tests/Workspace/WorkspacePilotSurfaceGuardTests.cs'
SRG = 'tests/RackCad.Tests/PushBackSharedReferenceGuardTests.cs'
MSG = 'tests/RackCad.Tests/PushBackWorkspaceModeSourceGuardTests.cs'
CCP = 'tests/RackCad.Tests/PushBackClassicCharacterizationPluginGuardTests.cs'
HAW = 'tests/RackCad.UI.Tests/PushBackHeaderAuthorityWindowTests.cs'
WHC = 'src/RackCad.Application/Workspace/WorkspaceHostCore.cs'
PORT = 'src/RackCad.Application/Workspace/IWorkspacePilotEditPort.cs'
SUP = 'src/RackCad.Application/Workspace/WorkspacePilotSupport.cs'
WEI = 'src/RackCad.Application/Workspace/WorkspaceEditIntent.cs'
FVM = 'src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs'
PVX = 'src/RackCad.UI/Workspace/WorkspacePanelView.xaml'
PVC = 'src/RackCad.UI/Workspace/WorkspacePanelView.xaml.cs'
WCO = 'src/RackCad.Plugin/Workspace/WorkspaceController.cs'
W = 'src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs'
LAUNCH = 'src/RackCad.Application/Systems/PushBack/PushBackDraftEditorLaunch.cs'
SPORT = 'src/RackCad.Application/Systems/PushBack/PushBackDraftStatePort.cs'
CI = '.github/workflows/ci.yml'

# (id, lo que cita la v4.9, archivo, clase, expresion de inicio, expresion de fin para 'block')
ANCHORS = [
    # §4.3
    ('G01', 'PEG:155-163 (17 nombres; la lista)', PEG, 'member', r'public void ThePortDeclaresTheSeventeenOperations', None),
    ('G02', 'CT-PBT1B-15, PushBackSharedReferenceGuardTests:306-403 (la lista)', SRG, 'member', r'public void CT_PBT1B_15_', None),
    ('G02b', 'CT-PBT1B-15, «archivos=14» (:381 en D-PBT4-15)', SRG, 'line', r'"archivos=14",', None),
    ('G03', 'WorkspaceHostSourceGuardTests:398-420', WHS, 'member', r'public void NoHostFileUsesTheInternalAutoCadNamespaceOrAnyUiType', None),
    ('G04', 'EIG:1011-1022 (9 archivos)', EIG, 'member', r'public void TheFoldersKeepTheirFilesAndTheTwoNewFilesOfTheApplicationAreExactFiles', None),
    ('G05', 'WorkspaceNavigationHostTests:714-723 (9 archivos)', WNH, 'member', r'public void EveryNewPluginFileIsAnExactFileOfTheFolderAndTheFolderHoldsNineFiles', None),
    ('G06', 'WorkspaceBridgeSourceGuardTests:364-367', WBS, 'member', r'public void NoExistingPluginCommandFileReferencesTheWorkspaceBridge', None),
    ('G07', 'WorkspaceBridgeSourceGuardTests:139-142 (los nombres prohibidos)', WBS, 'block', r'private const string ForbiddenCallNames', r';\s*$'),
    ('G08', 'G-9 (IG; las cuentas de NH y RIA)', IG, 'member', r'\["src/RackCad\.Plugin/Workspace/WorkspaceNavigationHost\.cs"\] = new\(\) \{ \["LockDocument"\] = 2 \}', None),
    ('G09', 'EIG:1150-1190 (el cableado de SetPilotUpdateHost)', EIG, 'member', r'private static IReadOnlyList<string> UpdateHostSetterViolations', None),
    ('G09b', 'EIG:1150-1190 (la prueba del cableado)', EIG, 'member', r'public void TheControllerAssignsTheExecutorOfTheUpdateInTheNavigationGetter', None),
    ('G09c', 'EIG:1150-1190 (su autoprueba)', EIG, 'member', r'public void TheSetterCensusTripsOnACallInTheConstructor', None),
    ('G10', 'CT-PBT2B-08 (PushBackWorkspaceModeSourceGuardTests)', MSG, 'member', r'public void CT_PBT2B_08_', None),
    ('G10b', 'CT-PBT2B-08, R10 (:271-274)', MSG, 'line', r'// R10: los once literales', None),
    ('G11', 'URT:1107-1109 (los 7 nombres de PilotBoundaryKind)', URT, 'block', r'new\[\] \{ "TabChange", "RackChange", "DocumentChange"', r'Enum\.GetNames<PilotBoundaryKind>\(\)\);'),
    ('G12', 'EIH:1066 (los dos metodos con «Update»)', EIH, 'line', r'methodNames\.Where\(n => n\.Contains\("Update", StringComparison\.Ordinal\)\)', None),
    ('G13', 'PEG:380-385 (dos interfaces de WHC)', PEG, 'member', r'public void TheHostImplementsThePortExplicitlyAndNeverReadsOutsideTheThreeReads', None),
    ('G14', 'EIH:1088 (un campo de Systems en WHC)', EIH, 'line', r'var systems = fields\.Where\(f => Flatten\(f\.Type\)', None),
    # D-PBT4-12 a 17 (y los de la v3 a los que remiten)
    ('D01', 'PV:218-254 (el WrapPanel de los botones del campo)', PVX, 'xmlwrap', r'x:Name="BrowserFieldEditButton"', None),
    ('D01b', 'PV: BrowserFieldEditButton', PVX, 'line', r'x:Name="BrowserFieldEditButton"', None),
    ('D02', 'WHC:1149 («the same two-phase step as a blocking boundary»)', WHC, 'line', r'the same two-phase step as a blocking boundary', None),
    ('D03', 'WCo:52 (new PilotEditAdapter(new WorkspacePilotEditGate(_host, _noticeDepth)))', WCO, 'line', r'new PilotEditAdapter\(new WorkspacePilotEditGate\(_host, _noticeDepth\)\)', None),
    ('D04', 'WorkspacePilotSupport.cs:153- (la compuerta)', SUP, 'member', r'public sealed class WorkspacePilotEditGate : IWorkspacePilotEditPort', None),
    ('D05', 'WCo:283-327 (PilotEditAdapter)', WCO, 'member', r'private sealed class PilotEditAdapter : IWorkspacePilotEditPort', None),
    ('D06', 'PEG:683-695 (ControllerViolations)', PEG, 'member', r'private static IEnumerable<string> ControllerViolations\(string controller\)', None),
    ('D07', 'WHC:1127 (SetPendingText mira la marca)', WHC, 'member', r'PilotFieldResult IWorkspacePilotEditPort\.SetPendingText\(', None),
    ('D07b', 'WHC:1155 (CommitField mira la marca)', WHC, 'member', r'PilotFieldResult IWorkspacePilotEditPort\.CommitField\(', None),
    ('D07c', 'WHC:1208 (CrossBoundary mira la marca)', WHC, 'member', r'PilotBoundaryResult IWorkspacePilotEditPort\.CrossBoundary\(', None),
    ('D07d', 'WHC:1243 (DiscardRack mira la marca)', WHC, 'member', r'EditDiscardOutcome IWorkspacePilotEditPort\.DiscardRack\(', None),
    ('D07e', 'WHC:1270 (DiscardDocument mira la marca)', WHC, 'member', r'EditDiscardOutcome IWorkspacePilotEditPort\.DiscardDocument\(', None),
    ('D08', 'WHC:1022 (TryBeginEdit, la marca como envoltorio)', WHC, 'member', r'EditIntentOutcome IWorkspacePilotEditPort\.TryBeginEdit\(', None),
    ('D08b', 'WHC: TryKeepMine del puerto (el envoltorio de TryKeepPilotEdit)', WHC, 'member', r'EditIntentOutcome IWorkspacePilotEditPort\.TryKeepMine\(', None),
    ('D08c', 'WHC: TryKeepPilotEdit (las reglas que copia TryEditAll, I-09)', WHC, 'member', r'public EditIntentOutcome TryKeepPilotEdit\(EditIntentRequest request\)', None),
    ('D09', 'WHC:1388 y :1392 (RequestUpdate: la marca y despues PassGate)', WHC, 'member', r'EditUpdateStart IWorkspacePilotEditPort\.RequestUpdate\(', None),
    ('D09b', 'WHC:1939 (PassGate)', WHC, 'member', r'private EditGate PassGate\(SessionId session, out string reason\)', None),
    ('D09c', 'WHC:996 (UpdateGate)', WHC, 'member', r'private EditGate UpdateGate\(SessionId session, out string reason, out long documentNativeId\)', None),
    ('D09d', 'WHC: UpdateInFlightForRequest', WHC, 'line', r'private bool UpdateInFlightForRequest\(EditIntentRequest request\)', None),
    ('D09e', 'WHC: NotePilotEdit', WHC, 'line', r'public void NotePilotEdit\(SessionId session\)', None),
    ('D09f', 'WHC: SetPilotUpdateHost (WCo:110 lo llama)', WHC, 'line', r'public void SetPilotUpdateHost\(IPushBackPilotUpdateHost host\)', None),
    ('D09g', 'WHC: el campo _updateHost (EIH:1088)', WHC, 'line', r'private IPushBackPilotUpdateHost\? _updateHost;', None),
    ('D10', 'PEG:778-781 (el modal, solo por EnterPanelModal)', PEG, 'member', r'public void TheModalFlagIsDrivenOnlyByTheLiteralForwardAndByEnterPanelModalOverTheScope', None),
    ('D10b', 'WCo:141 (EnterPanelModal)', WCO, 'member', r'public IDisposable EnterPanelModal\(\)', None),
    ('D10c', 'WCo:110 (SetPilotUpdateHost en el getter de la navegacion)', WCO, 'line', r'_host\.SetPilotUpdateHost\(new RackPilotUpdateCommandContext\(\)\);', None),
    ('D11', 'EIH:1039-1046 (la excepcion de portador de TryUpdatePilot)', EIH, 'line', r'var update = typeof\(WorkspaceHostCore\)\.GetMethod\("TryUpdatePilot"', None),
    ('D12', 'W:3427 (PendingWork)', W, 'member', r'internal EditorPendingWork PendingWork\(\)', None),
    ('D12b', 'W: WorkspaceUnappliedQuestion', W, 'line', r'private const string WorkspaceUnappliedQuestion', None),
    ('D12c', 'W: ForDraft (el abridor lo llama)', W, 'member', r'public static RackPushBackSystemWindow ForDraft\(PushBackDraftEditorLaunch launch\)', None),
    ('D12d', 'W: HeaderConfiguratorPresenter (la costura de CT-09)', W, 'line', r'internal Action<RackFrameConfiguratorWindow> HeaderConfiguratorPresenter', None),
    ('D12e', 'W: HeaderConfiguratorDialog (la otra costura, sin construir el configurador)', W, 'line', r'internal Func<RackFrameConfiguration, RackFrameConfiguration> HeaderConfiguratorDialog', None),
    ('D13', 'PushBackHeaderAuthorityWindowTests.cs:99-106 (la costura en uso)', HAW, 'line', r'w\.HeaderConfiguratorPresenter = QuickConfigAt\(custom\);', None),
    ('D14', 'ci.yml:213-222 (el MessageBox real en el STA de la UI)', CI, 'line', r'MessageBox', None),
    ('D15', 'PushBackClassicCharacterizationPluginGuardTests.cs:196-209 (el abridor clasico, sin Owner)', CCP, 'member', r'public void RackPushBackShowsTheWindowModallyWithoutAnOwnerAndReadsOnlyTheViewAndSectionOfTheRequest', None),
    ('D16', 'WorkspacePilotSurfaceGuardTests:341 (LimitViolations)', WPS, 'member', r'private static IEnumerable<string> LimitViolations\(IEnumerable<\(string Path, string Code\)> files\)', None),
    ('D17', 'PEG:44-65 (la tabla de las 17 operaciones)', PEG, 'block', r'private static readonly \(string Name, string Interface\)\[\] Operations =', r'^\s*\};'),
    ('D17b', 'PEG:68 (las seis de lectura)', PEG, 'line', r'private static readonly string\[\] ReadOperations =', None),
    ('D18', 'IWorkspacePilotEditPort.cs:143-157 (PilotFieldStatus)', PORT, 'member', r'public enum PilotFieldStatus', None),
    ('D18b', 'IWorkspacePilotEditPort.cs: la interfaz', PORT, 'member', r'public interface IWorkspacePilotEditPort', None),
    ('D19', 'PushBackDraftEditorLaunch (lo que lleva el lanzamiento)', LAUNCH, 'member', r'class PushBackDraftEditorLaunch', None),
    ('D19b', 'IPushBackDraftStatePort (el puerto de la ventana)', SPORT, 'member', r'interface IPushBackDraftStatePort', None),
]

# Los simbolos que la v4.9 nombra: cuantos archivos de src/ y de tests/ los nombran en C (los nuevos de PB-T4c tienen que dar 0).
SYMBOLS = ['TryEditAll', 'EditAllOutcome', 'IWorkspaceFullEditorHost', 'SetFullEditorHost', 'FullEditorModal', 'RackWorkspaceEditorWindow',
           'PushBackFullDraftLaunch', 'PushBackFullDraftStatePort', 'WorkspaceStagedAndUnappliedQuestion', 'BrowserFieldEditAllButton',
           'OnBrowserFieldEditAllClick', 'ShowEditAllButton', 'EditAllPendingTextReason', 'PushBackEditAll', 'PushBackOpenerClassicPathGuardTests',
           'PushBackDraftLaunchGuardTests', 'PushBackEditAllWindowTests', 'PushBackEditAllPortEquivalenceGuardTests', 'PushBackEditAllGuardTests',
           'PushBackEditAllTests', 'HeaderConfiguratorPresenter', 'HeaderConfiguratorDialog', 'ConfigureModuleHeaderButton', 'PushBackDraftSessionPort',
           'LaunchFor', 'PilotEraseScopeDouble', 'PilotUpdateScopeDouble', 'WorkspacePilotEditPortDouble', 'PushBackDraftEditorLaunch',
           'IPushBackDraftStatePort', 'RackPushBackSystemWindow.ForDraft(']

# Los archivos que PB-T4c cambia (§6.3, AllowedWriteScope de la v4.9) y los de la lista.
TOUCHED = [WHC, PORT, SUP, WEI, FVM, PVX, PVC, WCO, W, PEG, SRG]


def git(*args):
    return subprocess.run(['git', '-C', REPO] + list(args), capture_output=True, check=True).stdout


def show(rev, path):
    try:
        return git('show', rev + ':' + path).decode('utf-8-sig').replace('\r\n', '\n')
    except subprocess.CalledProcessError:
        return None


def member_end(lines, start):
    depth, seen = 0, False
    for i in range(start, len(lines)):
        code = re.sub(r'"(?:[^"\\]|\\.)*"', '""', re.sub(r'//.*', '', lines[i]))
        for ch in code:
            if ch == '{':
                depth += 1
                seen = True
            elif ch == '}':
                depth -= 1
                if seen and depth == 0:
                    return i
        if not seen and code.rstrip().endswith(';') and i > start:
            return i
    return len(lines) - 1


def enclosing_member_start(lines, i):
    # Para un anclaje dentro de un miembro (G08): la firma del miembro que lo contiene, la ultima linea anterior con nivel de clase.
    sig = re.compile(r'^\s*(?:\[|(?:public|private|internal|protected|static)\b)')
    j = i
    while j >= 0:
        if sig.match(lines[j]) and not lines[j].startswith(' ' * 12):
            return j
        j -= 1
    return i


def locate(rev, path, kind, start_rx, end_rx, aid):
    text = show(rev, path)
    if text is None:
        return None
    lines = text.split('\n')
    hits = [i for i, l in enumerate(lines) if re.search(start_rx, l)]
    if not hits:
        return ('sin-coincidencia', None, None)
    i = hits[0]
    if kind == 'line':
        return ([h + 1 for h in hits], '\n'.join(lines[h] for h in hits), len(hits))
    if kind == 'xmlwrap':
        # El WrapPanel que contiene la linea que casa: el ultimo <WrapPanel antes y el primer </WrapPanel> despues.
        a = max(k for k in range(0, i + 1) if '<WrapPanel' in lines[k])
        e = min(k for k in range(i, len(lines)) if '</WrapPanel>' in lines[k])
        return ((a + 1, e + 1), '\n'.join(lines[a:e + 1]), len(hits))
    if kind == 'block':
        j = i
        while j < len(lines) and not re.search(end_rx, lines[j]):
            j += 1
        return ((i + 1, j + 1), '\n'.join(lines[i:j + 1]), len(hits))
    if aid == 'G08':
        i = enclosing_member_start(lines, i)
    a = i
    while a > 0 and lines[a - 1].strip().startswith('['):
        a -= 1
    e = member_end(lines, i)
    return ((a + 1, e + 1), '\n'.join(lines[a:e + 1]), len(hits))


def sha(s):
    return hashlib.sha256(s.encode('utf-8')).hexdigest()[:12] if s is not None else '-'


def fmt(pos):
    if pos is None:
        return 'sin-archivo'
    if isinstance(pos, str):
        return pos
    if isinstance(pos, list):
        return ','.join(str(p) for p in pos)
    return '%d-%d' % pos


print('# Remedicion de PB-T4c (v5.0, fase 1), por objetos git')
print('# A=%s (donde midio la v4.x)  B=%s (base de PB-T4b)  C=%s (cierre de PB-T4b, base de PB-T4c)' % (A, B, C))
print()
print('## 1. Los anclajes')
print('id\tcita de la v4.9\tarchivo\tlineas en A\tlineas en C\tcoincidencias en C\ttexto igual A=C\ttexto igual B=C')
for aid, cite, path, kind, srx, erx in ANCHORS:
    ra = locate(A, path, kind, srx, erx, aid)
    rb = locate(B, path, kind, srx, erx, aid)
    rc = locate(C, path, kind, srx, erx, aid)
    ta = ra[1] if ra else None
    tb = rb[1] if rb else None
    tc = rc[1] if rc else None
    print('%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s' % (aid, cite, path.split('/')[-1], fmt(ra[0] if ra else None), fmt(rc[0] if rc else None),
                                            rc[2] if rc else '-', (ta == tc) if (ta is not None and tc is not None) else '-',
                                            (tb == tc) if (tb is not None and tc is not None) else '-'))

print()
print('## 2. Los archivos que toca PB-T4c (AllowedWriteScope de la v4.9, y los dos de la lista): cambiados de A a C y de B a C')
for path in TOUCHED:
    ab = git('diff', '--name-only', A, C, '--', path).decode().strip() != ''
    bc = git('diff', '--name-only', B, C, '--', path).decode().strip() != ''
    tc = show(C, path)
    print('%s\tcambia A->C=%s\tcambia B->C=%s\tlineas en C=%s' % (path, ab, bc, len(tc.split('\n')) if tc is not None else 'sin-archivo'))

print()
print('## 3. Los simbolos de la v4.9: los archivos de src/ y tests/ que los nombran en C')
files = [p for p in git('ls-tree', '-r', '--name-only', C).decode().split('\n') if (p.startswith('src/') or p.startswith('tests/')) and (p.endswith('.cs') or p.endswith('.xaml'))]
texts = {}
out = git('cat-file', '--batch', input=None) if False else None
for p in files:
    texts[p] = None
# cat-file --batch: una sola llamada
req = ''.join('%s:%s\n' % (C, p) for p in files).encode()
res = subprocess.run(['git', '-C', REPO, 'cat-file', '--batch'], input=req, capture_output=True, check=True).stdout
pos = 0
for p in files:
    nl = res.index(b'\n', pos)
    header = res[pos:nl].split()
    size = int(header[2])
    body = res[nl + 1:nl + 1 + size]
    texts[p] = body.decode('utf-8-sig', errors='replace')
    pos = nl + 1 + size + 1
for s in SYMBOLS:
    hit = sorted(p for p, t in texts.items() if s in t)
    print('%s\t%d\t%s' % (s, len(hit), '; '.join(h.split('/')[-1] for h in hit[:8]) + (' ...' if len(hit) > 8 else '')))

print()
print('## 4. Lo que PB-T4b anadio que fija archivos que PB-T4c cambia')
added = [p for p in git('diff', '--name-only', '--diff-filter=A', B, C, '--', 'tests').decode().split('\n') if p]
names = {WHC: 'WorkspaceHostCore', PORT: 'IWorkspacePilotEditPort', SUP: 'WorkspacePilotSupport', WEI: 'WorkspaceEditIntent',
         FVM: 'WorkspacePilotFieldViewModel', PVX: 'WorkspacePanelView', WCO: 'WorkspaceController', W: 'RackPushBackSystemWindow',
         PEG: 'WorkspacePilotEditGuardTests', SRG: 'PushBackSharedReferenceGuardTests'}
print('archivos de pruebas nuevos de PB-T4b: %d' % len(added))
for p in sorted(added):
    t = texts.get(p) or show(C, p) or ''
    hit = [v for k, v in names.items() if v in t]
    if hit:
        print('%s\tnombra: %s' % (p, ', '.join(hit)))

print()
print('## 5. CT-PBT4B-20: los 21 originales que fija por su BaseSha, y si PEG y PushBackSharedReferenceGuardTests estan entre ellos')
g = texts['tests/RackCad.Tests/WorkspaceFullCopyGuardTests.cs']
origins = re.findall(r'\("(tests/[^"]+)",\s*"([0-9a-f]{64})", (\d+)\)', g)
print('originales: %d' % len(origins))
for path, base, skips in origins:
    if path in (PEG, SRG):
        print('%s\tBaseSha=%s\tSkip=%s' % (path, base, skips))
print('PushBackSharedReferenceGuardTests entre ellos: %s' % any(p == SRG for p, _, _ in origins))
m = re.search(r'private static readonly Regex SkipLine = new Regex\(\s*(@"[^\n]*")', g)
print('SkipLine (la expresion que devuelve un Skip a [Fact] o [Theory]): %s' % (m.group(1) if m else 'sin-coincidencia'))
peg_c = show(C, PEG)
peg_b = show(B, PEG)
restored = re.sub(r'^([ \t]*)\[(Fact|Theory)\(Skip = "I-64 F4B PB-T4b: [^"]*"\)\]', r'\1[\2]', peg_c, flags=re.M)
print('PEG en C, con sus Skip de PB-T4b devueltos, es su BaseSha: %s' % (hashlib.sha256(restored.encode('utf-8')).hexdigest() == dict((p, b) for p, b, _ in origins)[PEG]))
print('PEG en C, con sus Skip de PB-T4b devueltos, es PEG en B: %s' % (restored == peg_b))
print('Skip de PB-T4b en PEG en C: %d' % len(re.findall(r'^[ \t]*\[(?:Fact|Theory)\(Skip = "I-64 F4B PB-T4b: ', peg_c, flags=re.M)))
