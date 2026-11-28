"""PB-T4c (F4B-PBT4-EDIT-ALL-WINDOW): comprobaciones de solo lectura del Worker sobre git diff BaseSha..<actual> -- src tests.

Uso: python -I check_inv.py <worktree> <BaseSha> <actual> <RedSha|-> <salida.txt> [--green]
  <actual> es un SHA (se leen los objetos git) o WORKTREE (se lee el disco y los archivos nuevos sin versionar).
Comprueba: INV-PBT4C-01 (alcance), los Skip, los equivalentes (EquivalentSha), el GREEN literal (con --green), el censo de declaraciones de
PBT4C-C13 (nombre a nombre y con su cuenta), los avisos CS de los registros de build y D2 (las lineas anadidas de src/ que nombran las dos
referencias compartidas). Escribe en UTF-8 con LF. Solo lee.
"""
import collections
import hashlib
import os
import re
import subprocess
import sys

sys.dont_write_bytecode = True
sys.stdout.reconfigure(encoding='utf-8', newline='\n')
repo, base, current, red, out = sys.argv[1:6]
green_mode = '--green' in sys.argv

RUN = 'artifacts/orchestration/I-64/F4B-PBT4-EDIT-ALL-WINDOW/1/R20261121T205731Z-41db'

WHC = 'src/RackCad.Application/Workspace/WorkspaceHostCore.cs'
PORT = 'src/RackCad.Application/Workspace/IWorkspacePilotEditPort.cs'
SUP = 'src/RackCad.Application/Workspace/WorkspacePilotSupport.cs'
WEI = 'src/RackCad.Application/Workspace/WorkspaceEditIntent.cs'
FVM = 'src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs'
PV = 'src/RackCad.UI/Workspace/WorkspacePanelView.xaml'
PVC = 'src/RackCad.UI/Workspace/WorkspacePanelView.xaml.cs'
WCO = 'src/RackCad.Plugin/Workspace/WorkspaceController.cs'
W = 'src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs'
HOST = 'src/RackCad.Application/Workspace/IWorkspaceFullEditHost.cs'
LAUNCH = 'src/RackCad.Application/Systems/PushBack/Draft/PushBackFullDraftLaunch.cs'
SPORT = 'src/RackCad.Application/Systems/PushBack/Draft/PushBackFullDraftStatePort.cs'
OPENER = 'src/RackCad.Plugin/Views/RackWorkspaceEditorWindow.cs'
SRC13 = [WHC, PORT, SUP, WEI, FVM, PV, PVC, WCO, W, HOST, LAUNCH, SPORT, OPENER]
SKELETON8 = [PORT, WEI, WHC, FVM, HOST, LAUNCH, SPORT, OPENER]

CT_FILES = ['tests/RackCad.Tests/PushBackEditAllTests.cs', 'tests/RackCad.Tests/PushBackEditAllGuardTests.cs',
            'tests/RackCad.Tests/PushBackOpenerClassicPathGuardTests.cs', 'tests/RackCad.Tests/PushBackDraftLaunchGuardTests.cs',
            'tests/RackCad.UI.Tests/PushBackEditAllWindowTests.cs']
EQUIVALENTS = {
    'tests/RackCad.Tests/PushBackEditAllPortEquivalenceGuardTests.cs': '1a2607d3a35ae8f9737fffb0eb32a4acd8100318fe83e5b0098d30428f57d250',
    'tests/RackCad.Tests/PushBackEditAllNameWritersGuardTests.cs': '3744992127be8b7e1d8d822a769b8eb2a6886789c73285c8436e5baec6bf7b6b',
    'tests/RackCad.Tests/PushBackEditAllFullCopyGuardTests.cs': '4619b6d310340f9c97a7d9f31bda1db852d2fcda905b91bb857510f7e6b7e6ce',
    'tests/RackCad.UI.Tests/PushBackEditAllApplyCloseTests.cs': '7c2458260ef301d7f05bfaff8e1e8c4a4a618f25fd7022705d55dee32dbe78d2',
}
SKIP_FILES = ['tests/RackCad.Tests/Workspace/WorkspacePilotEditGuardTests.cs', 'tests/RackCad.Tests/PushBackSharedReferenceGuardTests.cs',
              'tests/RackCad.Tests/WorkspaceFullCopyGuardTests.cs', 'tests/RackCad.UI.Tests/PushBackWorkspaceApplyCloseTests.cs']
GREEN_SHA = {
    LAUNCH: 'e93aa1d793588c5eb4d842db97a6ac55716bedaab607059046fdbecc79feb28b',
    SPORT: 'b08cf5c7437810427645efb5e2f4592c6c5032cea5abe27d01291f42a0e53b5a',
    HOST: '9d8aa1b5770c5705cc4daf38541bd01cc00df70652741547b9a83ef582d3dadf',
    PORT: 'b3a2622b490e2e787088680743662365bf9e4b0016cad7578703281b6b1c0863',
    WEI: 'b937f893360b39c5751b5b5da9fe3f976a76b256799fbf685a2dc4193f141e96',
    WHC: '3fcabacbc9a568cc2b777d4e8bd356b76f55273f674d0ab604177f36ffeb5fee',
    SUP: 'a86d1f72c7884d5870d9107df65d4e1b5d577f801cfc080db5cb7202bcffcd4d',
    OPENER: '12e7786a1e39cf9f7c3b5be4137baf93df6b262fac2683eb0d04775a5e1a9401',
    WCO: '9448d170263905e37c24035699cf69e8419c381add53c45c389cd415d6669539',
    W: '597fa16486e836a5dad62251367029d4c8939ae3fb11834ee26b3598a70a31b0',
    PV: '3fffd2cdd707bbf37ac903ff1d65d8040ddde091a319dc6c439255440f63a70b',
    PVC: 'b21633ea1b82507da4d69b0d119d958cde30480d0f023798d1bad24d5b0f4342',
    FVM: '417c36cd68267ecec960e89c4908a237d8c9d89093ab93aecebd3f80a862f845',
}
FORBIDDEN_PREFIXES = ['src/RackCad.Domain', 'src/RackCad.Application/Systems/PushBack/Pilot/', 'src/RackCad.Plugin/RackPushBackCommands.cs',
                      'src/RackCad.Plugin/RackCommandSupport.cs', 'src/RackCad.Plugin/Views/RackPilotIntentAdapter.cs',
                      'src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs']

SKIP_PREFIX = 'I-64 F4B PB-T4c: sustituida por su equivalente en '


def git(*args, check=True):
    return subprocess.run(['git', '-C', repo] + list(args), capture_output=True, check=check).stdout


def lf(raw):
    return raw.decode('utf-8-sig').replace('\r\n', '\n')


def text_at(rev, path):
    """El texto (LF) del archivo en una revision o en el disco (WORKTREE); None si no existe."""
    if rev == 'WORKTREE':
        full = os.path.join(repo, path)
        return lf(open(full, 'rb').read()) if os.path.exists(full) else None
    p = subprocess.run(['git', '-C', repo, 'show', rev + ':' + path], capture_output=True)
    return lf(p.stdout) if p.returncode == 0 else None


def sha(text):
    return hashlib.sha256(text.encode('utf-8')).hexdigest()


def changed(frm, to, pathspec):
    if to == 'WORKTREE':
        names = set(git('diff', '--name-only', frm, '--', *pathspec).decode('utf-8').split('\n'))
        names |= set(git('ls-files', '--others', '--exclude-standard', '--', *pathspec).decode('utf-8').split('\n'))
    else:
        names = set(git('diff', '--name-only', frm, to, '--', *pathspec).decode('utf-8').split('\n'))
    return sorted(n for n in names if n)


lines = []
ok = True


def report(label, good, detail=''):
    global ok
    ok = ok and good
    lines.append('%s: %s%s' % (label, 'PASA' if good else 'FALLA', (' | ' + detail) if detail else ''))


lines.append('# comprobaciones de solo lectura: base=%s actual=%s red=%s' % (base, current, red))

# ----------------------------------------------------------------------------------------------------------- INV-PBT4C-01
src_changed = changed(base, current, ['src'])
tests_changed = changed(base, current, ['tests'])
lines.append('src cambiados (%d): %s' % (len(src_changed), src_changed))
lines.append('tests cambiados (%d): %s' % (len(tests_changed), tests_changed))
expected_src = sorted(SRC13 if green_mode else SKELETON8)
expected_tests = sorted(CT_FILES + list(EQUIVALENTS) + SKIP_FILES)
report('INV-PBT4C-01 src == %s' % ('los 13 del GREEN' if green_mode else 'los 8 del esqueleto'), src_changed == expected_src,
       'de mas=%s de menos=%s' % (sorted(set(src_changed) - set(expected_src)), sorted(set(expected_src) - set(src_changed))))
report('INV-PBT4C-01 tests == 5 CT + 4 equivalentes + 4 de los Skip', tests_changed == expected_tests,
       'de mas=%s de menos=%s' % (sorted(set(tests_changed) - set(expected_tests)), sorted(set(expected_tests) - set(tests_changed))))
forb = [p for p in changed(base, current, ['src']) if any(p.startswith(f) for f in FORBIDDEN_PREFIXES)]
report('INV-PBT4C-01 Domain, Pilot/, RackPushBackCommands, RackCommandSupport, RackPilotIntentAdapter y WorkspaceNavigationHost sin cambio', not forb, str(forb))
plugin = [p for p in src_changed if p.startswith('src/RackCad.Plugin/')]
report('INV-PBT4C-01 en src/RackCad.Plugin solo WCo y el abridor',
       sorted(plugin) == sorted(p for p in (WCO, OPENER) if p in src_changed), str(plugin))
must = [SUP, PV, PVC, WCO, W]
if green_mode:
    report('INV-PBT4C-02 el GREEN toca al menos WPS, PV, PV.cs, WCo y W', all(p in src_changed for p in must))
if red != '-':
    since_red_src = changed(red, current, ['src'])
    since_red_tests = changed(red, current, ['tests'])
    lines.append('desde el RED: src=%s tests=%s' % (since_red_src, since_red_tests))
    report('INV-PBT4C-02 RedSha..actual: src == los 13 del GREEN que no estaban en el esqueleto o cambian, y tests vacio',
           (sorted(since_red_src) == sorted(SRC13) or sorted(since_red_src) == sorted(set(SRC13) - set([PORT, WEI, HOST]))) and not since_red_tests,
           'src=%d tests=%d' % (len(since_red_src), len(since_red_tests)))

# ----------------------------------------------------------------------------------------------------------- los Skip
for path in SKIP_FILES:
    a = text_at(base, path).split('\n')
    b = text_at(current, path).split('\n')
    diff = [(i, x, y) for i, (x, y) in enumerate(zip(a, b)) if x != y]
    same_len = len(a) == len(b)
    only_attr = same_len and len(diff) == 1 and diff[0][1].strip() == '[Fact]' and diff[0][2].strip().startswith('[Fact(Skip = "' + SKIP_PREFIX)
    report('Skip %s' % path, only_attr, 'lineas distintas=%d' % len(diff))

# ----------------------------------------------------------------------------------------------------------- los equivalentes
for path, want in EQUIVALENTS.items():
    t = text_at(current, path)
    report('equivalente %s' % path, t is not None and sha(t) == want, 'sha256=%s' % (sha(t) if t is not None else 'ausente'))

# ----------------------------------------------------------------------------------------------------------- el GREEN literal
if green_mode:
    for path, want in GREEN_SHA.items():
        t = text_at(current, path)
        report('GREEN literal %s' % path, t is not None and sha(t) == want, 'sha256=%s' % (sha(t) if t is not None else 'ausente'))

# ----------------------------------------------------------------------------------------------------------- el censo de PBT4C-C13
KEYWORDS = {'if', 'for', 'foreach', 'while', 'switch', 'using', 'lock', 'catch', 'return', 'new', 'nameof', 'typeof', 'sizeof', 'default', 'base',
            'this', 'throw', 'when', 'where', 'get', 'set', 'init', 'add', 'remove', 'fixed', 'checked', 'unchecked'}
C13 = {
    PORT: ['IWorkspacePilotEditPort.TryEditAll'],
    WEI: ['tipo:EditAllStatus'] + ['EditAllStatus.' + m for m in ('NotAvailable', 'Waiting', 'Rejected', 'NotBegun', 'Refused', 'Closed', 'Failed')]
         + ['tipo:EditAllOutcome', 'EditAllOutcome.EditAllOutcome'] + ['EditAllOutcome.' + m for m in ('Status', 'Reason', 'Begin', 'Applied')]
         + ['EditAllOutcome.' + m for m in ('NotAvailable', 'Waiting', 'Rejected', 'NotBegun', 'Refused', 'Closed', 'Failed')],
    WHC: ['WorkspaceHostCore.' + m for m in ('EditAllNoEditorReason', 'EditAllPendingTextReason', 'EditAllBusyDraftReason', 'EditAllStateReason',
                                             '_fullEditHost', 'SetFullEditHost', 'IWorkspacePilotEditPort.TryEditAll', 'EditAllOfPilot')],
    SUP: ['WorkspacePilotEditGate.TryEditAll'],
    WCO: ['PilotEditAdapter.TryEditAll', 'tipo:FullEditorModal', 'FullEditorModal._controller', 'FullEditorModal._open', 'FullEditorModal.FullEditorModal',
          'FullEditorModal.ShowModal'],
    FVM: ['WorkspacePilotFieldViewModel.' + m for m in ('EditAllNotOpenedPrefix', 'EditAllAppliedText', 'EditAllNothingAppliedText', 'ShowEditAllButton',
                                                       'EditAll', 'EditAllNoticeOf', 'SentenceOf')],
    PV: ['x:Name.BrowserFieldEditAllButton'],
    PVC: ['WorkspacePanelView.OnBrowserFieldEditAllClick'],
    W: ['RackPushBackSystemWindow.WorkspaceStagedAndUnappliedQuestion'],
    HOST: ['tipo:IWorkspaceFullEditHost', 'IWorkspaceFullEditHost.ShowModal'],
    LAUNCH: ['tipo:PushBackFullDraftLaunch', 'PushBackFullDraftLaunch.For', 'PushBackFullDraftLaunch.ProjectOf'],
    SPORT: ['tipo:PushBackFullDraftStatePort', 'PushBackFullDraftStatePort._draft', 'PushBackFullDraftStatePort._applied',
            'PushBackFullDraftStatePort.PushBackFullDraftStatePort', 'PushBackFullDraftStatePort.ApplyToDraft'],
    OPENER: ['tipo:RackWorkspaceEditorWindow', 'RackWorkspaceEditorWindow.Show'],
}


def clean(text):
    text = re.sub(r'@"(?:[^"]|"")*"', '""', text)
    text = re.sub(r'\$?"(?:\\.|[^"\\\n])*"', '""', text)
    text = re.sub(r"'(?:\\.|[^'\\\n])'", "''", text)
    text = re.sub(r'/\*.*?\*/', ' ', text, flags=re.S)
    text = re.sub(r'//[^\n]*', '', text)
    return text


def member_name(head):
    head = re.sub(r'^\s*(?:\[[^\]]*\]\s*)+', '', head).strip()
    if not head or head.split()[0] in KEYWORDS or head.startswith('#'):
        return None
    arrow = head.find('=>')
    sig = head if arrow < 0 else head[:arrow]
    paren = sig.find('(')
    eq = re.search(r'(?<![=!<>])=(?![=>])', sig)
    if paren >= 0 and (eq is None or paren < eq.start()):
        m = re.search(r'((?:\w+\.)?\w+)\s*(?:<[^()]*>)?\s*\($', sig[:paren + 1].rstrip())
        if not m:
            return None
        name = m.group(1)
        return None if name.split('.')[-1] in KEYWORDS else name
    if eq is not None:
        sig = sig[:eq.start()]
    m = re.search(r'(\w+)\s*$', sig)
    if not m or len(sig.split()) < 2:
        return None
    return m.group(1)


def census_cs(text):
    code = clean(text)
    found = []
    stack = []
    buf = ''
    for ch in code:
        if ch == '{':
            m = re.search(r'\b(class|struct|interface|enum|record)\s+(\w+)', buf)
            top = stack[-1] if stack else None
            if m and not re.search(r'\bnew\b|\bwhere\b[^{]*$', buf[:m.start()]):
                container = top[1] if top and top[0] == 'tipo' else None
                found.append(('tipo:' + m.group(2), container))
                stack.append(('tipo', m.group(2), m.group(1)))
            elif top and top[0] == 'tipo':
                name = member_name(buf)
                if name:
                    found.append((top[1] + '.' + name, top[1]))
                stack.append(('miembro',))
            else:
                stack.append(('otro',))
            buf = ''
        elif ch == '}':
            top = stack.pop() if stack else None
            if top and top[0] == 'tipo' and top[2] == 'enum':
                for item in buf.split(','):
                    item = re.sub(r'=.*$', '', item.strip(), flags=re.S).strip()
                    if re.fullmatch(r'\w+', item):
                        found.append((top[1] + '.' + item, top[1]))
            buf = ''
        elif ch == ';':
            top = stack[-1] if stack else None
            if top and top[0] == 'tipo' and top[2] != 'enum':
                name = member_name(buf)
                if name:
                    found.append((top[1] + '.' + name, top[1]))
            buf = ''
        else:
            buf += ch
    return collections.Counter(k for k, _ in found)


def census(path, text):
    if text is None:
        return collections.Counter()
    if path.endswith('.xaml'):
        return collections.Counter('x:Name.' + n for n in re.findall(r'\bx:Name="(\w+)"', text))
    return census_cs(text)


for path in SRC13:
    before = census(path, text_at(base, path))
    after = census(path, text_at(current, path))
    added, removed = after - before, before - after
    want = collections.Counter(C13[path])
    lines.append('censo %s: base=%d actual=%d' % (path, sum(before.values()), sum(after.values())))
    lines.append('  nuevas (%d): %s' % (sum(added.values()), sorted(added.elements())))
    lines.append('  quitadas (%d): %s' % (sum(removed.values()), sorted(removed.elements())))
    if green_mode:
        report('censo C13 %s: la diferencia es la lista cerrada' % path, added == want and not removed,
               'de mas=%s de menos=%s' % (sorted((added - want).elements()), sorted((want - added).elements())))
    else:
        report('censo C13 %s: la diferencia es una parte de la lista cerrada' % path, not (added - want) and not removed,
               'fuera de la lista=%s' % sorted((added - want).elements()))

# ----------------------------------------------------------------------------------------------------------- avisos CS
cs_total = 0
for root, _, files in os.walk(os.path.join(repo, RUN)):
    for f in sorted(files):
        if f.startswith('build-') and f.endswith('.log'):
            count = len(re.findall(r'warning CS\d+', open(os.path.join(root, f), 'rb').read().decode('utf-8', 'replace')))
            cs_total += count
            lines.append('warning CS en %s: %d' % (os.path.relpath(os.path.join(root, f), os.path.join(repo, RUN)).replace(os.sep, '/'), count))
report('0 lineas "warning CS" en todos los registros de build', cs_total == 0, 'total=%d' % cs_total)

# ----------------------------------------------------------------------------------------------------------- D2
added_lines = []
for path in src_changed:
    a = (text_at(base, path) or '').split('\n')
    b = (text_at(current, path) or '').split('\n')
    seen = collections.Counter(a)
    for line in b:
        if seen[line] > 0:
            seen[line] -= 1
        else:
            added_lines.append((path, line))
tokens = re.compile(r'WorkingBaseline|LastModuleReconciliation|RackModuleReconciliationResult|PushBackSystem\b|ReseedModuleSession|SetWorkingBaseline')
hits = [(p, l.strip()) for p, l in added_lines if tokens.search(re.sub(r'//.*$', '', l))]
lines.append('D2: lineas anadidas en src=%d; las que nombran las dos referencias compartidas o un PushBackSystem (sin comentarios): %d' % (len(added_lines), len(hits)))
for p, l in hits:
    lines.append('  %s: %s' % (p, l))
report('D2 ninguna linea anadida de src/ nombra las dos referencias compartidas ni un PushBackSystem', not hits)

lines.append('RESULTADO: %s' % ('PASA' if ok else 'FALLA'))
with open(out, 'w', encoding='utf-8', newline='\n') as f:
    f.write('\n'.join(lines) + '\n')
print(lines[-1])
sys.exit(0 if ok else 1)
