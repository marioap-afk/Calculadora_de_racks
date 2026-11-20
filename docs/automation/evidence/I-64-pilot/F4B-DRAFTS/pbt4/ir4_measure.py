"""ir4_measure: mediciones de solo lectura del revisor independiente de PB-T4 v1.

Uso: python -I ir4_measure.py <checkout principal> <rev> <paquete.md> <salida.txt>
Solo llama a `git -C <checkout> show|grep|ls-tree` (objetos git). Lee el paquete (carpeta de la sesion) y escribe solo <salida.txt>.
"""
import re
import subprocess
import sys

repo, rev, package_path, out_path = sys.argv[1:5]
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


METHOD = re.compile(r'^\s*\[(?:Fact|Theory)\b', re.M)

# 1. El recuento de las 17 clases candidatas.
TW = 'tests/RackCad.Tests/Workspace/'
T = 'tests/RackCad.Tests/'
CANDIDATES = [TW + c + '.cs' for c in [
    'WorkspacePilotEditHostTests', 'WorkspaceEditIntentHostTests', 'WorkspacePilotReconciliationHostTests', 'WorkspacePilotUpdateHostTests',
    'WorkspacePilotUpdateHostRunTests', 'WorkspacePilotUpdateRequestTests', 'WorkspaceLifecycleHostTests', 'WorkspaceAssociativeBookkeepingTests',
    'WorkspaceCommandAdvisorTests', 'WorkspaceEditIntentGuardTests', 'WorkspaceLifecycleGuardTests', 'WorkspacePilotUpdateIntegrationGuardTests',
    'WorkspacePilotUpdateGuardTests', 'WorkspacePilotEditGuardTests', 'WorkspacePilotPrepareGuardTests']] + [T + 'PushBackWritePathGuardTests.cs', T + 'PushBackDraftWriteGuardTests.cs']
total = 0
theories = 0
emit('## 1. Las 17 clases candidatas')
for path in CANDIDATES:
    text = show(path)
    n = len(METHOD.findall(text))
    th = len(re.findall(r'^\s*\[Theory\b', text, re.M))
    total += n
    theories += th
    emit('- %s: %d metodos (%d Theory)' % (path.rsplit('/', 1)[-1], n, th))
emit('- total: %d metodos, %d Theory' % (total, theories))

# 2. Archivos de pruebas FUERA de las 17 que nombran lo que PB-T4a cambia.
emit('')
emit('## 2. Archivos de pruebas fuera de las 17 que nombran lo que PB-T4a cambia (token -> archivos)')
TOKENS = [
    r'RequestKeepMine', r'AskChoice', r'ConfirmChoice', r'KeepMineResponder', r'\bPilotRead\b', r'FromPilot\s*\(', r'EditIntentReadResult',
    r'RackPilotIntentAdapter', r'WorkspaceNavigationHost', r'PushBackPilotFieldAdapter', r'KeepPilotEditsOn', r'typeof\(IWorkspacePilotEditPort\)',
    r'IWorkspacePilotEditPort\.cs', r'WorkspacePilotFieldViewModel\.cs', r'WorkspacePanelView\.xaml', r'WorkspaceController\.cs', r'WorkspaceHostCore\.cs',
    r'Find<PushBackPilotDraftState>', r'PushBackPilotStateComparer',
]
cand_set = set(CANDIDATES)
outside = {}
for tok in TOKENS:
    out = git('grep', '-l', '-E', '-e', tok, rev, '--', 'tests', ok=(0, 1))
    files = sorted(l[len(rev) + 1:] for l in out.splitlines())
    files_out = [f for f in files if f not in cand_set]
    emit('- `%s`: %d archivos, %d fuera de las 17' % (tok, len(files), len(files_out)))
    for f in files_out:
        outside.setdefault(f, []).append(tok)
        emit('    ' + f)
emit('')
emit('### Union de archivos fuera de las 17, con sus metodos')
grand = 0
for f in sorted(outside):
    text = show(f)
    n = len(METHOD.findall(text))
    grand += n
    emit('- %s: %d metodos; tokens: %s' % (f, n, ', '.join(outside[f])))
emit('- total de metodos en esos archivos: %d' % grand)

# 3. Pruebas de UI del flujo de Conservar con el doble del puerto: metodos que llaman RequestKeepMine y luego ConfirmChoice.
emit('')
emit('## 3. UI: metodos de WorkspacePilotReconciliationTests que llaman RequestKeepMine (y ConfirmChoice)')
ui = show('tests/RackCad.UI.Tests/Workspace/WorkspacePilotReconciliationTests.cs')
parts = re.split(r'(?=^\s*\[(?:Fact|Theory)\b)', ui, flags=re.M)
count_keep = 0
count_keep_confirm = 0
for p in parts[1:]:
    m = re.search(r'public\s+void\s+(\w+)\s*\(', p)
    name = m.group(1) if m else '?'
    if 'RequestKeepMine' in p:
        count_keep += 1
        both = 'ConfirmChoice' in p
        if both:
            count_keep_confirm += 1
        emit('- %s: RequestKeepMine%s' % (name, ' + ConfirmChoice' if both else ''))
emit('- metodos con RequestKeepMine: %d; con ConfirmChoice tambien: %d' % (count_keep, count_keep_confirm))

# 4. El doble del puerto de la UI: implementa el puerto entero? (si un miembro nuevo con cuerpo por defecto no lo implementa)
emit('')
emit('## 4. WorkspacePilotEditPortDouble: cabecera y operaciones')
dbl = show('tests/RackCad.UI.Tests/Workspace/WorkspacePilotEditPortDouble.cs')
for l in dbl.splitlines():
    if re.search(r'class\s+WorkspacePilotEditPortDouble|public\s+\w[\w<>,? ]*\s+(TryKeepMine|TryBeginEdit|GetField)\s*\(|KeepMineOp\s*=', l):
        emit('    ' + l.strip()[:200])

# 5. El literal de ExpectedEvidence[0] del paquete frente a la lista cerrada del sustituto de nc2 v2.
emit('')
emit('## 5. ExpectedEvidence[0]: el paquete frente a F4B_EXACT_EVIDENCES')
pkg = open(package_path, encoding='utf-8').read()
m = re.search(r'> (antes del Worker: [^\n]+)', pkg)
lit = m.group(1) if m else None
nc2 = show('docs/automation/evidence/I-64-pilot/F4B-DRAFTS/nc2-substitute-f4b-v2/nc2_exact_whitelist_substitute_f4b_v2.py')
lits = re.findall(r"^\s*'((?:antes|Antes) del Worker:[^']*)',\s*$", nc2, re.M)
emit('- literales en la lista: %d' % len(lits))
emit('- el del paquete: %r' % lit)
emit('- igual byte a byte a alguno: %s' % (lit is not None and any(lit.encode('utf-8') == x.encode('utf-8') for x in lits)))
pbt3c = show('docs/automation/evidence/I-64-pilot/F4B-PBT3C-FULL-WRITE-ERASE/coordinator/gate-contract.json')
m3 = re.search(r'"ExpectedEvidence"\s*:\s*\[\s*"((?:[^"\\]|\\.)*)"', pbt3c)
emit('- ExpectedEvidence[0] de PB-T3c: %r' % (m3.group(1) if m3 else None))
emit('- igual al del paquete: %s' % (m3 is not None and lit is not None and m3.group(1) == lit))

# 6. Las anclas de las guardas que el diseno toca.
emit('')
emit('## 6. Anclas')
ANCH = [
    (TW + 'WorkspacePilotEditGuardTests.cs', r'ThePortDeclaresTheSeventeenOperations|typeof\(IWorkspacePilotEditPort\)\.GetMethods|the adapter of the pilot is not the one static field|private static readonly PushBackPilotFieldAdapter PilotFieldAdapter'),
    (TW + 'WorkspacePilotReconciliationGuardTests.cs', r'public void RequestKeepMine\(\) => AskChoice\(ChoiceKind\.KeepMine\);'),
    (TW + 'WorkspaceEditIntentGuardTests.cs', r'Assert\.Equal\(6, PilotReadAssignment|Assert\.Equal\(6, Src\.Count\(core, @"\\\.ReadIntent'),
    (TW + 'WorkspaceEditIntentHostTests.cs', r'TheHostCoreHasExactlyOneFieldOfSystems|GetFields\(BindingFlags\.Instance'),
    ('src/RackCad.Application/Workspace/WorkspaceHostCore.cs', r'PilotFieldResult IWorkspacePilotEditPort\.(SetPendingText|CommitField)|if \(Authorize\(origin\) != RequestDecision\.Accepted\)|_modalRackCadActive'),
    ('src/RackCad.Application/Workspace/WorkspaceEditIntent.cs', r'PilotRead|FromPilot'),
    ('src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs', r'ChooseOriginDefinition|RackPilotIntentAdapter\.(Read|LoadCatalog)|FromPilot'),
    ('src/RackCad.Plugin/Views/RackPilotIntentAdapter.cs', r'PushBackPilotIntentReader\.Read\(|new PushBackPilotIntentScan'),
    ('src/RackCad.Application/Workspace/IWorkspaceEditIntentReader.cs', r'ReadIntent\('),
    ('src/RackCad.Application/Workspace/IWorkspacePilotEditPort.cs', r'enum PilotFieldStatus|^\s*(Rejected|Applied|NotEditable|Busy|Invalid),?\s*$'),
]
for path, pat in ANCH:
    out = git('grep', '-n', '-E', '-e', pat, rev, '--', path, ok=(0, 1))
    hits = [l[len(rev) + 1:] for l in out.splitlines()]
    emit('- %s: %s' % (path.rsplit('/', 1)[-1], ', '.join(h.split(':')[1] for h in hits[:20]) or 'ninguna'))

with open(out_path, 'w', encoding='utf-8', newline='\n') as f:
    f.write('\n'.join(lines) + '\n')
print('lineas', len(lines))
