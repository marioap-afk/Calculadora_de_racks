"""PB-T4, paquete v1: mediciones de solo lectura sobre una revision (por defecto 7dfc7c21).

Uso: python -I measure_pbt4.py <checkout principal> <rev> <salida.txt>
Solo llama a `git -C <checkout> show|grep|ls-tree|log` (objetos git); no lee el disco del checkout ni ningun worktree, y escribe solo <salida.txt>.
"""
import re
import subprocess
import sys

repo, rev, out_path = sys.argv[1:4]
lines = []


def git(*args, ok=(0,)):
    p = subprocess.run(['git', '-C', repo] + list(args), capture_output=True)
    if p.returncode not in ok:
        raise SystemExit('git %s -> %d: %s' % (' '.join(args), p.returncode, p.stderr.decode('utf-8', 'replace')))
    return p.stdout.decode('utf-8', 'replace')


def show(path):
    return git('show', '%s:%s' % (rev, path))


def emit(s=''):
    lines.append(s)


def grep_n(pattern, paths, fixed=False):
    args = ['grep', '-n'] + (['-F'] if fixed else ['-E']) + ['-e', pattern, rev, '--'] + paths
    out = git(*args, ok=(0, 1))
    return [l[len(rev) + 1:] for l in out.splitlines()]


def section(title):
    emit('')
    emit('## ' + title)


WHC = 'src/RackCad.Application/Workspace/WorkspaceHostCore.cs'
FVM = 'src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs'
XAML = 'src/RackCad.UI/Workspace/WorkspacePanelView.xaml'
W = 'src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs'
T = 'tests/RackCad.Tests/'
TW = 'tests/RackCad.Tests/Workspace/'

emit('# measure_pbt4: salida (rev %s)' % rev)
emit(git('log', '-1', '--format=%H %s', rev).strip())
emit(git('log', '-1', '--format=%H %s', 'e3c293b1').strip())

section('M-1. El anfitrion: el tipo del borrador de Push Back y sus puntos de uso')
for pat in [r'_drafts\.Begin\(', r'Find<PushBackPilotDraftState>', r'Find<PushBackFullDraftState>', r'KeepMineOnCurrentVersion\(',
            r'PushBackPilotUpdateRun\.(Execute|ExecuteFull|Precheck)\s*\(', r'public\s+\w[\w<>?]*\s+TryUpdatePilot\s*\(',
            r'PushBackPilotFieldAdapter', r'NotePilotEdit\s*\(', r'SetModalRackCadActive\s*\(']:
    hits = grep_n(pat, [WHC])
    emit('- `%s` en WHC: %d -> %s' % (pat, len(hits), ', '.join(h.split(':')[1] for h in hits)))

section('M-2. Lo que nada del producto llama todavia')
for pat, paths in [(r'ForDraft\s*\(', ['src']), (r'PushBackDraftSessionPort', ['src']), (r'LaunchFor\s*\(', ['src']),
                   (r'EnterPanelModal\s*\(', ['src']), (r'ExecuteFull\s*\(', ['src']), (r'PrepareFull\s*\(', ['src']),
                   (r'PushBackFullComposition\.KeepOn', ['src']), (r'PushBackConflictClassifier\.Classify', ['src']),
                   (r':\s*[^{]*IPushBackPilotViewEraser', ['src']), (r'Editar todo', ['src'])]:
    hits = grep_n(pat, paths)
    emit('- `%s` en src: %d' % (pat, len(hits)))
    for h in hits[:8]:
        emit('    ' + h[:180])

section('M-3. El panel: botones, gestos y fronteras')
for pat, path in [(r'x:Name="BrowserField\w*Button"', XAML), (r'Content="Editar', XAML), (r'enum\s+PilotBoundaryKind', 'src/RackCad.Application/Workspace/IWorkspacePilotEditPort.cs'),
                  (r'public\s+void\s+\w+\s*\(\s*\)', FVM), (r'_port\.TryKeepMine|_port\.RequestUpdate|_port\.CrossBoundary', FVM)]:
    hits = grep_n(pat, [path])
    emit('- `%s` en %s: %d' % (pat, path.rsplit('/', 1)[-1], len(hits)))
    for h in hits[:20]:
        emit('    ' + h[:180])

section('M-4. La ventana en WORKSPACE: las anclas que PB-T4 usa')
for pat in [r'public\s+static\s+RackPushBackSystemWindow\s+ForDraft', r'private\s+RackPushBackSystemWindow\s*\(\s*PushBackDraftEditorLaunch',
            r'PendingWork\s*\(\s*\)', r'WorkspaceUnappliedQuestion', r'HasUnappliedDraftChanges\s*\(', r'OnClosing\s*\(', r'RefuseInWorkspace\s*\(\s*\)\s*$|private\s+bool\s+RefuseInWorkspace']:
    hits = grep_n(pat, [W])
    emit('- `%s`: %s' % (pat, ', '.join(h.split(':')[1] for h in hits)))

section('M-5. Las pruebas que fijan el piloto en el panel (archivos:recuento)')
for pat in [r'KeepPilotEditsOn', r'PushBackPilotUpdateRun\.Execute\b', r'Find<PushBackPilotDraftState>', r'PushBackPilotStateComparer',
            r'PushBackPilotFieldAdapter', r'OutOfPilotDiagnostic|CompositeOutOfPilotReason', r'NonPilotContentReason']:
    out = git('grep', '-c', '-E', '-e', pat, rev, '--', 'tests', ok=(0, 1))
    rows = sorted(((l[len(rev) + 1:].rsplit(':', 1)) for l in out.splitlines()), key=lambda r: -int(r[1]))
    emit('- `%s`: %d archivos' % (pat, len(rows)))
    for f, n in rows:
        emit('    %s:%s' % (f, n))

section('M-6. Las clases que construyen el anfitrion real, y los metodos de las candidatas')
out = git('grep', '-l', '-E', '-e', r'new WorkspaceHostCore\(', rev, '--', 'tests', ok=(0, 1))
emit('- `new WorkspaceHostCore(` en tests: ' + ', '.join(l[len(rev) + 1:].rsplit('/', 1)[-1] for l in out.splitlines()))
CANDIDATES = ['WorkspacePilotEditHostTests', 'WorkspaceEditIntentHostTests', 'WorkspacePilotReconciliationHostTests', 'WorkspacePilotUpdateHostTests',
              'WorkspacePilotUpdateHostRunTests', 'WorkspacePilotUpdateRequestTests', 'WorkspaceLifecycleHostTests', 'WorkspaceAssociativeBookkeepingTests',
              'WorkspaceCommandAdvisorTests', 'WorkspaceEditIntentGuardTests', 'WorkspaceLifecycleGuardTests', 'WorkspacePilotUpdateIntegrationGuardTests',
              'WorkspacePilotUpdateGuardTests', 'WorkspacePilotEditGuardTests', 'WorkspacePilotPrepareGuardTests']
total = 0
for c in CANDIDATES:
    text = show(TW + c + '.cs')
    n = len(re.findall(r'^\s*\[(?:Fact|Theory)\b', text, re.M))
    total += n
    emit('- %s: %d metodos, %d menciones de PushBack' % (c, n, len(re.findall(r'PushBack', text))))
for c in ['PushBackWritePathGuardTests', 'PushBackDraftWriteGuardTests']:
    text = show(T + c + '.cs')
    n = len(re.findall(r'^\s*\[(?:Fact|Theory)\b', text, re.M))
    total += n
    emit('- %s: %d metodos' % (c, n))
emit('- total de metodos en las clases candidatas: %d (cota superior: no todas fallan)' % total)

section('M-7. Las guardas que condicionan el diseno (anclas)')
ANCHORS = [
    (TW + 'WorkspacePilotSurfaceGuardTests.cs', r'EnterPanelModal\|ShowModalWindow|ShowModalWindow\|ShowDialog'),
    (TW + 'WorkspaceBrowserHostGuardTests.cs', r'Abrir en editor'),
    (TW + 'WorkspacePilotCloseSyncGuardTests.cs', r'Assert\.Equal\(33'),
    (TW + 'WorkspacePilotReconciliationGuardTests.cs', r'TheSixButtons =|TheFiveGestures ='),
    (TW + 'WorkspaceEditIntentGuardTests.cs', r'KeepPilotEditsOn'),
    (TW + 'WorkspaceBridgeSourceGuardTests.cs', r'ForbiddenCallNames =|ShowModalDialog\|ShowModelessDialog'),
    (TW + 'WorkspaceModelBoundaryTests.cs', r'PersistenceOrWriteAuthorities = '),
    (TW + 'WorkspacePilotUpdateIntegrationGuardTests.cs', r'PushBackPilotUpdateRun\\\\.Execute|runCallers|G-9: the pilot'),
    (T + 'PushBackWritePathGuardTests.cs', r'Find<|ExecuteFull|PushBackFullPrepareInput'),
    (T + 'PushBackDraftWriteGuardTests.cs', r'PushBackViewDispositions|WorkspaceHostCore'),
]
for path, pat in ANCHORS:
    hits = grep_n(pat, [path])
    emit('- %s `%s`: %s' % (path.rsplit('/', 1)[-1], pat, ', '.join(h.split(':')[1] for h in hits[:12])))

section('M-8. Los sustitutos de nc1 y nc2: como se ligan a la tarea')
for path, pat in [('docs/automation/evidence/I-64-pilot/F4B-DRAFTS/nc1-cascade-substitute/nc1_cascade_signal_substitute.py', r"^TASKS = |prefix = 'F4B-"),
                  ('docs/automation/evidence/I-64-pilot/F4B-DRAFTS/nc2-substitute-f4b-v2/nc2_exact_whitelist_substitute_f4b_v2.py', r'^TASKS = |F4B-<TAREA>-|^F4B_EXACT_EVIDENCES')]:
    for h in grep_n(pat, [path]):
        emit('    ' + h[:200])

section('M-9. Los TaskId propuestos (24 a 26 caracteres, prefijo F4B-PBT4-)')
for tid in ['F4B-PBT4-FULL-DRAFT-PANEL', 'F4B-PBT4-EDIT-ALL-WINDOW']:
    emit('- %s: %d caracteres; prefijo F4B-PBT4-: %s' % (tid, len(tid), tid.startswith('F4B-PBT4-')))

section('M-10. La linea base: el contrato de PB-T3c')
c3 = show('docs/automation/evidence/I-64-pilot/F4B-PBT3C-FULL-WRITE-ERASE/coordinator/gate-contract.json')
for key in ['"TaskId"', '"AuthorityRevision"', '"MainSha"']:
    m = re.search(key + r'\s*:\s*"([^"]+)"', c3)
    emit('- %s: %s' % (key, m.group(1) if m else 'no aparece'))
for m in re.finditer(r'[^"]*(?:19 1[0-9]{2}|2 45[0-9])[^"]*', c3):
    s = m.group(0).strip()
    if 'Core' in s or 'UI' in s:
        emit('    ' + s[:260])
emit('- tamano del contrato de PB-T3c: %d bytes' % len(c3.encode('utf-8')))

with open(out_path, 'w', encoding='utf-8', newline='\n') as f:
    f.write('\n'.join(lines) + '\n')
print('lineas', len(lines))
