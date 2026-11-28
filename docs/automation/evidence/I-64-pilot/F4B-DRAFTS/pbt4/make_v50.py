"""Arma la v5.0 del paquete de PB-T4 desde la v4.9: PB-T4c entera y literal. Lee la v4.9 (por su sha256), los fragmentos de v50/ (f50-head.md,
f50-design.md, f50-43.md y f50-53.md), las CT como datos (pbt4c_ct_table_v50.py), las piezas del GREEN y del esqueleto, los Skip, los
equivalentes (generados por objetos git en 6adff90d con make_equivalents_pbt4c_v50.py), la lista de 4, los registros de la sesion y del Owner y el
resumen de la precondicion 6 (pbt4ccc/compile/pbt4c-precondition6.json), todos por su sha256. PB-T4a y PB-T4b (sus §5.1, §5.2, §6.1 y §6.2) no
cambian: se comprueba. El bloque literal del contrato de PB-T4c (§6.3.1) es ASCII: se comprueba. Escribe solo la v5.0.

Uso: python -I make_v50.py <repo>
"""
import hashlib
import importlib.util
import json
import os
import re
import sys

sys.stdout.reconfigure(encoding='utf-8', newline='\n')
sys.dont_write_bytecode = True
HERE = os.path.dirname(os.path.abspath(__file__))
REPO = sys.argv[1]
REV = '6adff90d'
V49 = os.path.join(HERE, 'I-64-f4b-pbt4-gate-contract-package-v49.md')
OUT = os.path.join(HERE, 'I-64-f4b-pbt4-gate-contract-package-v50.md')
CC = os.path.normpath(os.path.join(HERE, '..', 'pbt4ccc', 'compile'))
raw = open(V49, 'rb').read()
assert hashlib.sha256(raw).hexdigest() == '043f196d8452588530784eb04fd3cd3b63c3594d3e961df5e204f0109329c886'
t = raw.decode('utf-8')


def load(name):
    spec = importlib.util.spec_from_file_location(name, os.path.join(HERE, name + '.py'))
    mod = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(mod)
    return mod


def sha(path):
    data = open(path, 'rb').read()
    assert b'\r' not in data, path
    return hashlib.sha256(data).hexdigest()


def frag(name):
    return open(os.path.join(HERE, 'v50', name), encoding='utf-8').read()


ct = load('pbt4c_ct_table_v50')
gp = load('green_pbt4c_v50')
sk = load('pbt4c_skips_v50')
mk = load('make_equivalents_pbt4c_v50')
EQ = mk.equivalents(REPO, REV)
EQSHA = {p: hashlib.sha256(EQ[p].encode('utf-8')).hexdigest() for p in mk.PATHS}
assert EQSHA[mk.E1] == '1a2607d3a35ae8f9737fffb0eb32a4acd8100318fe83e5b0098d30428f57d250'
assert EQSHA[mk.E4] == '7c2458260ef301d7f05bfaff8e1e8c4a4a618f25fd7022705d55dee32dbe78d2'
S = {n: sha(os.path.join(HERE, n)) for n in (
    'pbt4c-green-v50.txt', 'pbt4c-skeleton-v50.txt', 'green_pbt4c_v50.py', 'pbt4c_skips_v50.py', 'make_equivalents_pbt4c_v50.py',
    'apply_red_pbt4c_v50.py', 'apply_green_pbt4c_v50.py', 'check_skips_pbt4c_v50.py', 'pbt4c_ct_table_v50.py', 'walk_guards_pbt4c_v50.py',
    'pbt4c-probe-editall.cs', 'pbt4c-probe-close-ui.cs', 'make_list_pbt4c_4.py', 'make_list_pbt4c_3.py', 'pbt4c-substitution-list-4.tsv',
    'pbt4c-substitution-list-3.tsv', 'session-pbt4c-design-decisions.md', 'owner-pbt4c-list-3-decision.md', 'owner-pbt4c-list-4-decision.md')}
assert S['pbt4c-green-v50.txt'] == '6b304a009d05ff586603ed4bbb9fa69b9973977bea09222c688ef80faa983e1b'
assert S['pbt4c-skeleton-v50.txt'] == '48e097a7a579bd311fdeb2f6b87ff57ec0ada157573b174ad3a02935e5ab816c'
assert S['pbt4c-substitution-list-4.tsv'] == '9bd4c17a0eda837c92a7a0b7b3562d4ab17f1608baf3ffa8efaf560d8b1f82de'
P6 = os.path.join(CC, 'pbt4c-precondition6.json')
P6SHA = sha(P6)
p6 = json.load(open(P6, encoding='utf-8'))
assert p6['Red']['Core']['Failed'] == 0 and p6['Green']['Core']['Failed'] == 0 and p6['D'] == 0
CCHECK = sha(os.path.join(CC, 'compile-check.md'))
WALK = open(os.path.join(HERE, 'v50', 'pbt4c-guard-walk-v50b-out.txt'), encoding='utf-8').read().split('\n')[0]
m = re.match(r'clases=(\d+) pruebas=(\d+) A=(\d+) B=(\d+) sin-diferencia=(\d+) no-leen=(\d+)', WALK)
WALK_TEXT = ('%s clases y %s pruebas sin `Skip` de las clases que nombran un archivo tocado o su carpeta; %s marcadas (A: un literal cambia de cuenta), '
             '%s sin diferencia y %s que no leen ninguno (`v50/pbt4c-guard-walk-v50b-out.txt`; la lectura por tipo de diferencia, '
             '`v50/walk-analysis-v50b-out.txt`)' % tuple('{:,}'.format(int(m.group(k))).replace(',', ' ') for k in (1, 2, 3, 5, 6)))
done = []


def R(old, new, times=1, what=None):
    global t
    n = t.count(old)
    assert n == times, (what or old[:90], n, times)
    t = t.replace(old, new)
    done.append(what or old[:60])


def SECTION(head, until, new, what):
    """Sustituye la seccion que empieza en la linea `head` hasta la linea `until` (sin incluirla)."""
    global t
    i = t.index('\n' + head) + 1
    j = t.index('\n' + until, i) + 1
    t = t[:i] + new + t[j:]
    done.append(what)


KEEP = []
for h_, u_ in (('### 5.1 PB-T4a', '### 5.2 PB-T4b'), ('### 5.2 PB-T4b', '### 5.3 PB-T4c'), ('### 6.1 PB-T4a', '### 6.2 PB-T4b'),
               ('### 6.2 PB-T4b', '### 6.3 PB-T4c')):
    i_ = t.index('\n' + h_) + 1
    KEEP.append(t[i_:t.index('\n' + u_, i_) + 1])

# ---------------------------------------------------------------- la cabecera
head = (frag('f50-head.md').replace('@@OWNER3@@', S['owner-pbt4c-list-3-decision.md']).replace('@@OWNER4@@', S['owner-pbt4c-list-4-decision.md'])
        .replace('@@LIST4@@', S['pbt4c-substitution-list-4.tsv']).replace('@@LIST3@@', S['pbt4c-substitution-list-3.tsv'])
        .replace('@@SESSIONC@@', S['session-pbt4c-design-decisions.md']))
assert '@@' not in head
TITLE = '# I-64 F4B — Paquete del contrato de PB-T4 («Editar todo» en el panel del Workspace), v4.9\n'
R(TITLE, TITLE.replace('v4.9', 'v5.0') + '\n' + head + '\n## La cabecera de la v4.9, sin cambio salvo lo que dice el registro v4.9 → v5.0\n',
  what='titulo y cabecera')
R('**Estado:** v4.9. Corrige la parte de', '**Estado de la v4.9:** Corrige la parte de')
R('**Lo que sale de la v4.9 (PB-T4b):**', '**Lo que salió de la v4.9 (PB-T4b):**')

# ---------------------------------------------------------------- §0 y §1
R('| **PB-T4c** | `F4B-PBT4-EDIT-ALL-WINDOW` (24) | «Editar todo», con el abridor en dos piezas | **2** (PEG:155-163 y CT-PBT1B-15) | Condicional |',
  '| **PB-T4c** | `F4B-PBT4-EDIT-ALL-WINDOW` (24) | «Editar todo», con el abridor en dos piezas | **4** (PEG:155-163, CT-PBT1B-15, CT-PBT4B-20 y CT-PBT2B-06), la '
  'lista que el Owner tomó como la que vio tras sus dos decisiones de PB-T4c (`owner-pbt4c-list-3-decision.md` y `owner-pbt4c-list-4-decision.md`) | '
  'Condicional, con esa lista, sobre el cierre de PB-T4b (`6adff90d`) |')
R('y en la séptima, sobre el coste de la lectura de PB-T4b, «Darlo por cumplido» (`owner-pbt4b-cost-decision.md`). La sesión decidió D-S1 a D-S3, D-S5 y D-S6 '
  '(`session-pbt4b-design-decisions.md`) y D-S4 (D-PBT4-21; §8.5).',
  'y en la séptima, sobre el coste de la lectura de PB-T4b, «Darlo por cumplido» (`owner-pbt4b-cost-decision.md`). Sobre PB-T4c, dos veces «Añadirla '
  'a la lista» (`owner-pbt4c-list-3-decision.md` y `owner-pbt4c-list-4-decision.md`). La sesión decidió D-S1 a D-S3, D-S5 y D-S6 '
  '(`session-pbt4b-design-decisions.md`), D-S4 (D-PBT4-21; §8.5) y, para PB-T4c, D-S7 a D-S10 (`session-pbt4c-design-decisions.md`).')
R('| «Darlo por cumplido»: el paso 7 de la precondición 6, con dos de tres corridas (§8.5) |\n',
  '| «Darlo por cumplido»: el paso 7 de la precondición 6, con dos de tres corridas (§8.5) |\n'
  '| **Las dos decisiones del Owner sobre la lista de PB-T4c** (`owner-pbt4c-list-3-decision.md` y `owner-pbt4c-list-4-decision.md`, 2026-11-21, '
  'la octava y la novena tandas) | «Añadirla a la lista», dos veces: **4** en PB-T4c (§8.5) |\n'
  '| **Las decisiones de diseño de la sesión para PB-T4c** (`session-pbt4c-design-decisions.md`, 2026-11-21) | D-S7 a D-S10 (§8.5) |\n'
  '| **El arrastre A-T2b-OWNER-01** (F4B-E38; `owner-pbt2b-close-decisions.md:36`) | «Cerrar» en WORKSPACE con una sola pregunta (D-PBT4-17) |\n')
R('- PB-T4c es casi aditiva, con dos sustituciones por letra;',
  '- PB-T4c es casi aditiva, con cuatro sustituciones con registro (la lista de 4; V50-02);')

# ---------------------------------------------------------------- D-PBT4-12 a 17, §4.3 y §5.3
SECTION('### PB-T4c\n\n#### D-PBT4-12.', '### Común\n', frag('f50-design.md'), 'D-PBT4-12 a 17')
SECTION('### 4.3 PB-T4c', '## 5. La red de pruebas', frag('f50-43.md').replace('@@LIST4@@', S['pbt4c-substitution-list-4.tsv']).replace('@@WALK@@', WALK_TEXT),
        '§4.3')
rows = ['| CT | Qué fija | Las líneas (literales del paquete) | Clase | `ExpectRed` |', '|---|---|---|---|---|']
for n, what, lines, cls, proj, red in ct.CT:
    assert ' | ' not in what, n
    rows.append('| **%s** | %s | %s | `%s`%s | %s |' % (n, what, '; '.join('`%s`' % l for l in lines), cls, ' (UI)' if proj == 'UI' else '',
                                                    'true' if red else '**false**'))
P6TXT = ('`pbt4ccc/compile/pbt4c-precondition6.json`, sha256 `%s`, y `compile-check.md`, `%s`' % (P6SHA, CCHECK))
s53 = frag('f50-53.md').replace('@@CTTABLE@@', '\n'.join(rows)).replace('@@P6@@', P6TXT)
assert '@@' not in s53
SECTION('### 5.3 PB-T4c', '### 5.4 La CI del RED', s53, '§5.3')

# ---------------------------------------------------------------- §6.0
R('- **PB-T4c:** WHC, `IWorkspacePilotEditPort.cs`, `WorkspacePilotSupport.cs`, WEI, FVM, PV, WCo y W;',
  '- **PB-T4c:** WHC, `IWorkspacePilotEditPort.cs`, `WorkspacePilotSupport.cs`, WEI, FVM, `WorkspacePanelView.xaml` y `WorkspacePanelView.xaml.cs`, '
  'WCo y `RackPushBackSystemWindow.xaml.cs` (W; `W.xaml` no cambia);')
R('en PB-T4c, ningún archivo de `src/` ni PEG ni `PushBackSharedReferenceGuardTests.cs`.',
  'en PB-T4c, ningún archivo de `src/`, ni los cuatro equivalentes, ni los cuatro archivos de los `Skip` (PEG, `PushBackSharedReferenceGuardTests.cs`, '
  '`WorkspaceFullCopyGuardTests.cs` y `PushBackWorkspaceApplyCloseTests.cs`). **Los equivalentes no admiten reparación** (RR-2): su texto es el del '
  'guion, con su `EquivalentSha` (§6.3.1), y no tienen fila en la tabla de abajo, que es la de las clases de las CT.')
OLD_RR3 = ('| PB-T4c | `PushBackDraftLaunchGuardTests` (CT-15) | **Líneas:** `g-l1 archivos=2 reglas-sin-coincidencias=True`, `pureza archivos=2 violaciones=[]`, '
           '`cubre ApplyToDraft=PushBackFullDraftStatePort.cs`, y las fichas y los términos de la fila de CT-PBT4A-08 |\n'
           '| PB-T4c | La de CT-08 | **Líneas:** `var window = RackPushBackSystemWindow.ForDraft((PushBackDraftEditorLaunch)launch);` y '
           '`AcApplication.ShowModalWindow(window);` |\n\nLas filas de PB-T4c, con sus líneas medidas, van en su contrato.')
NEW_RR3 = ('| PB-T4c | `PushBackEditAllTests` (CT-03, 04, 05, 10, 11 y 12) | **Líneas:** las de esas CT en §5.3, literales **Valores:** `2.5` y `3` |\n'
           '| PB-T4c | `PushBackEditAllGuardTests` (CT-01, 08 y 13) | **Líneas:** las de esas CT en §5.3, literales |\n'
           '| PB-T4c | `PushBackOpenerClassicPathGuardTests` (CT-07) | **Líneas:** las de esas CT en §5.3, literales |\n'
           '| PB-T4c | `PushBackDraftLaunchGuardTests` (CT-15) | **Líneas:** `g-l1 archivos=2 reglas-sin-coincidencias=True`, `pureza archivos=2 violaciones=[]`, '
           '`cubre ApplyToDraft=PushBackFullDraftStatePort.cs`, y las fichas y los términos de la fila de CT-PBT4A-08 |\n'
           '| PB-T4c | `PushBackEditAllWindowTests` (UI; CT-02, 06, 09 y 14) | **Líneas:** las de esas CT en §5.3, literales **Valores:** `4.5` |\n\n'
           '**Las filas de PB-T4c** (V50-06) son una por clase de las CT, con su clase y sus CT, en la forma de la de CT-19 de PB-T4b; las líneas de CT-08, '
           'entre ellas la conversión y la llamada del abridor, son de `PushBackEditAllGuardTests`.')
R(OLD_RR3, NEW_RR3, what='RR-3 de PB-T4c')
R('`ui-tests` en `success` (PB-T4a) o en `failure` (PB-T4b y PB-T4c), sin cuelgue.',
  '`ui-tests` en `success` (PB-T4a) o en `failure` (PB-T4b y PB-T4c; en PB-T4c, con las 4 CT de UI y el equivalente de CT-PBT2B-06), sin cuelgue.')

# ---------------------------------------------------------------- §6.3, §6.3.1 y §6.3.2
blk = t[t.index('\n#### 6.2.1 '):t.index('\n#### 6.2.2 ')]
code = blk[blk.index('```text\n') + 8:blk.index('\n```', blk.index('```text\n'))]
B = {}
for line in code.split('\n'):
    mm = re.match(r'^(PBT4B-C\d\d|\[\d+\]|INV-PBT4B-\d\d)[:. ]', line)
    if mm:
        B[mm.group(1)] = line


def fromb(key, newkey, pairs=()):
    line = B[key]
    line = newkey + line[len(key):]
    for old, new in pairs:
        assert line.count(old) >= 1, (key, old[:60])
        line = line.replace(old, new)
    return line


LIST_SHA = S['pbt4c-substitution-list-4.tsv']
SCOPE = [
    ('src/, existentes (UNIT_CHANGE, 9)', ['src/RackCad.Application/Workspace/WorkspaceHostCore.cs', 'src/RackCad.Application/Workspace/IWorkspacePilotEditPort.cs',
                                           'src/RackCad.Application/Workspace/WorkspacePilotSupport.cs', 'src/RackCad.Application/Workspace/WorkspaceEditIntent.cs',
                                           'src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs', 'src/RackCad.UI/Workspace/WorkspacePanelView.xaml',
                                           'src/RackCad.UI/Workspace/WorkspacePanelView.xaml.cs', 'src/RackCad.Plugin/Workspace/WorkspaceController.cs',
                                           'src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs']),
    ('src/, nuevos (4)', ['src/RackCad.Application/Workspace/IWorkspaceFullEditHost.cs', 'src/RackCad.Application/Systems/PushBack/Draft/PushBackFullDraftLaunch.cs',
                          'src/RackCad.Application/Systems/PushBack/Draft/PushBackFullDraftStatePort.cs', 'src/RackCad.Plugin/Views/RackWorkspaceEditorWindow.cs']),
    ('las clases de las CT (5)', ['tests/RackCad.Tests/PushBackEditAllTests.cs', 'tests/RackCad.Tests/PushBackEditAllGuardTests.cs',
                                  'tests/RackCad.Tests/PushBackOpenerClassicPathGuardTests.cs', 'tests/RackCad.Tests/PushBackDraftLaunchGuardTests.cs',
                                  'tests/RackCad.UI.Tests/PushBackEditAllWindowTests.cs']),
    ('los equivalentes (4), nuevos, que genera el guion', list(mk.PATHS)),
    ('solo para los Skip (4)', [s_[0] for s_ in sk.SKIPS]),
]
assert sorted(SCOPE[0][1]) == sorted(gp.EXISTING) and sorted(SCOPE[1][1]) == sorted(gp.NEW)
PATHS = [p for _, ps in SCOPE for p in ps]
assert len(PATHS) == 26 == len(set(PATHS))

C13 = ('PBT4C-C13: doing the task would require touching a file of src/ other than the 13 of the AllowedWriteScope; or adding a declaration of any '
       'visibility (type, constructor, method, property, field, constant, enum member, nested type or x:Name; an overload is one more) whose name is not '
       'in this closed list: in IWorkspacePilotEditPort.cs, the method TryEditAll of the interface, with its default body; in WorkspaceEditIntent.cs, the '
       'enum EditAllStatus with its seven members (NotAvailable, Waiting, Rejected, NotBegun, Refused, Closed, Failed) and the struct EditAllOutcome with '
       'its private constructor, its properties Status, Reason, Begin and Applied and its static methods NotAvailable, Waiting, Rejected, NotBegun, Refused, '
       'Closed and Failed; in WorkspaceHostCore.cs, the private constants EditAllNoEditorReason, EditAllPendingTextReason, EditAllBusyDraftReason and '
       'EditAllStateReason, the private field _fullEditHost, the public method SetFullEditHost, the explicit implementation IWorkspacePilotEditPort.TryEditAll '
       'and the private method EditAllOfPilot; in WorkspacePilotSupport.cs, in WorkspacePilotEditGate, the public method TryEditAll; in '
       'WorkspaceController.cs, in PilotEditAdapter, the public method TryEditAll, and the private nested class FullEditorModal with its private fields '
       '_controller and _open, its public constructor and its public method ShowModal; in WorkspacePilotFieldViewModel.cs, the private constants '
       'EditAllNotOpenedPrefix, EditAllAppliedText and EditAllNothingAppliedText, the property ShowEditAllButton, the method EditAll and the private '
       'methods EditAllNoticeOf and SentenceOf; in WorkspacePanelView.xaml, the Button BrowserFieldEditAllButton; in WorkspacePanelView.xaml.cs, the '
       'private method OnBrowserFieldEditAllClick; in RackPushBackSystemWindow.xaml.cs, the private constant WorkspaceStagedAndUnappliedQuestion; in '
       'IWorkspaceFullEditHost.cs, the interface IWorkspaceFullEditHost with its method ShowModal; in PushBackFullDraftLaunch.cs, the static class '
       'PushBackFullDraftLaunch with its method For and its private method ProjectOf; in PushBackFullDraftStatePort.cs, the class '
       'PushBackFullDraftStatePort with its private fields _draft and _applied, its constructor and its method ApplyToDraft; in '
       'RackWorkspaceEditorWindow.cs, the static class RackWorkspaceEditorWindow with its method Show. Prescribed changes that are not new declarations: '
       'WHC changes no existing body; FVM changes RaiseAll; PendingWork of the window gains its first if; the getter Navigation of WCo gains the call '
       'to SetFullEditHost. The Worker stops before adding anything else.')
OWN = [
    fromb('PBT4B-C01', 'PBT4C-C01'),
    'PBT4C-C02: it would require editing, skipping, re-enabling or deleting an existing test or helper, or touching InternalsVisibleTo, the packages, a '
    '.csproj or Fixtures/I64. No exception outside the closed list of PB-T4c: the Skip of each of the 4 tests of pbt4c-substitution-list-4.tsv (sha256 '
    + LIST_SHA + '), with its literal reason, in its attribute line, and nothing else in those 4 files. Any existing test that fails, outside that list, '
    'returns to the Owner.',
    'PBT4C-C03: an observable change of RACKEDITAR appears, or of the panel or of the window other than the declared ones (the Editar todo button and its '
    'gesture; the full editor opened from the panel, modal, over the full draft; Cerrar in WORKSPACE with one question when a staged module and unapplied '
    'changes would be lost together; the summary and Modificado after closing). STOP and return to the Owner.',
    fromb('PBT4B-C04', 'PBT4C-C04'),
    fromb('PBT4B-C05', 'PBT4C-C05'),
    fromb('PBT4B-C06', 'PBT4C-C06'),
    fromb('PBT4B-C07', 'PBT4C-C07', [
        ('(the self-tests of CT-PBT4B-12, CT-PBT4B-13, CT-PBT4B-15, CT-PBT4B-19, CT-PBT4B-20 and CT-PBT4B-21)',
         '(the self-tests of CT-PBT4C-07, CT-PBT4C-13 and CT-PBT4C-15)'),
        ('(one of its six classes; never the kit nor the copies, whose text is the package one)',
         '(one of its five classes; never the four equivalents, whose text is the package one)'),
        ('A copy or a hand-written equivalent of the list that fails in the GREEN is this stop too: the session issues a package version with its '
         'hand-written text (D-S1).',
         'An equivalent of the list that fails in the GREEN, or one of the three of Core that fails in the RED, is this stop too: the session issues a '
         'package version with its text.')]),
    fromb('PBT4B-C08', 'PBT4C-C08'),
    fromb('PBT4B-C09', 'PBT4C-C09'),
    'PBT4C-C10: doing the task would require what the scope of PB-T4c excludes: retiring or editing the code of the pilot (Pilot/); touching the classic '
    'editor, RACKEDITAR, the classic path or the menu; the window in CLASSIC; creating views; translating a state; a member of the edit port other than '
    'TryEditAll; a new value of PilotBoundaryKind; reading or writing the drawing from the window or after it closes; or touching a test of the list '
    'other than by its Skip.',
    fromb('PBT4B-C11', 'PBT4C-C11', [('PB-T4b', 'PB-T4c')]),
    fromb('PBT4B-C12', 'PBT4C-C12'),
    C13,
    fromb('PBT4B-C14', 'PBT4C-C14'),
    'PBT4C-C16: an existing test outside the closed list of substitutions of PB-T4c (the 4 of pbt4c-substitution-list-4.tsv) fails, including one that '
    'reads the text or the types in a way only seen when it runs. It returns to the Owner (owner-pbt4-decisions.md, question 4). An equivalent of the '
    'list that fails where the package does not expect it is not this stop: it is PBT4C-C07.',
    fromb('PBT4B-C17', 'PBT4C-C17'),
    fromb('PBT4B-C18', 'PBT4C-C18'),
]
INV = [
    'INV-PBT4C-01. Alcance: dentro de las 26 rutas. git diff BaseSha..CurrentSha -- src/RackCad.Domain vacio; en src/RackCad.Plugin solo '
    'Workspace/WorkspaceController.cs y Views/RackWorkspaceEditorWindow.cs; en src/RackCad.Application solo WHC, IWorkspacePilotEditPort.cs, '
    'WorkspacePilotSupport.cs, WEI, Workspace/IWorkspaceFullEditHost.cs y los dos archivos nuevos de Systems/PushBack/Draft/; en src/RackCad.UI solo '
    'FVM, Workspace/WorkspacePanelView.xaml, Workspace/WorkspacePanelView.xaml.cs y Systems/PushBack/RackPushBackSystemWindow.xaml.cs; ningun archivo de '
    'Pilot/; en tests/ solo las cinco clases de las CT, los cuatro equivalentes y las lineas de atributo con Skip de los cuatro archivos de la lista.',
    'INV-PBT4C-02. RED y GREEN: el RED son las cinco clases de las CT, los cuatro equivalentes byte a byte (su sha256, EquivalentSha), los cuatro Skip y '
    'el esqueleto exacto de src/ (pbt4c-skeleton-v50.txt: el puerto con TryEditAll y su cuerpo por defecto, WEI con EditAllStatus y EditAllOutcome, '
    'IWorkspaceFullEditHost.cs entero, WHC solo con SetFullEditHost sin efecto, FVM con ShowEditAllButton en false y EditAll sin efecto, y los dos '
    'archivos nuevos de Draft/ y el abridor, inertes; la compuerta, WCo, WorkspacePanelView.xaml, WorkspacePanelView.xaml.cs y '
    'RackPushBackSystemWindow.xaml.cs sin cambio); el GREEN solo toca src/, con el texto literal del paquete (pbt4c-green-v50.txt). En el RED, cada '
    'filtro con ExpectRed da todos Failed por asercion (la primera linea del StackTrace bajo tests/), con Failed == Selected, tambien el del '
    'equivalente de CT-PBT2B-06; los filtros con ExpectRed false pasan y sus autopruebas fallan sobre el texto inyectado. Con una reparacion del RED '
    '(F4B-E35, RR): RedSha..RedRepairSha solo toca las cinco clases de las CT y ningun archivo de src/, ni los cuatro equivalentes, ni los cuatro '
    'archivos de los Skip; el criterio se repite entero sobre RedRepairSha; y RedRepairSha..CurrentSha es el GREEN de arriba.',
    'INV-PBT4C-03. La API: exactamente la de D-PBT4-12 a D-PBT4-17, con el texto literal del GREEN del paquete; TryEditAll es el unico miembro nuevo '
    'del puerto, con su cuerpo por defecto, y sus dos reenvios explicitos, uno en la compuerta y otro en el adaptador del Plugin, y nada mas; ForDraft '
    'tiene un solo llamador en src/ (RackWorkspaceEditorWindow.Show) y EnterPanelModal() otro (FullEditorModal.ShowModal).',
    'INV-PBT4C-04. Cero escrituras: ninguno de los seis sujetos de CT-PBT4C-07 nombra LockDocument, una transaccion, Commit( ni una API de escritura del '
    'dibujo; la ventana solo entrega sus ediciones al puerto del lanzamiento; nada despues de cerrar.',
    'INV-PBT4C-05. La ventana: solo cambia PendingWork() y su constante WorkspaceStagedAndUnappliedQuestion, en WORKSPACE (en WORKSPACE, un modulo '
    'escenificado deja siempre cambios sin aplicar: D-S10); ni el camino clasico, ni RACKEDITAR, ni la ventana en CLASSIC cambian; '
    'RackPushBackSystemWindow.xaml no cambia.',
    'INV-PBT4C-06. K = 1 y los nombres: la marca de lectura envuelve TryEditAll, con el inicio del borrador y ShowModal; con el editor abierto, la tabla '
    'de D-PBT4-14 (CT-PBT4C-05); WHC conserva sus dos interfaces; los tipos del puerto y del soporte evitan Draft, Editor, Subscription y Tab, y ningun '
    'tipo de RackCad.Application.Workspace lleva Subscription, Editor ni Draft (D-S9).',
    fromb('INV-PBT4B-07', 'INV-PBT4C-07', [
        ('salvo las 478 de la lista cerrada (pbt4b-substitution-list-478.tsv, sha256 8b0fcd0ca775ce3a5e46ef2059f4b06e7ec7319883daf3cb37b1fe91290e9e8b)',
         'salvo las 4 de la lista cerrada (pbt4c-substitution-list-4.tsv, sha256 ' + LIST_SHA + ')'),
        ('de los 21 archivos de pruebas existentes', 'de los cuatro archivos de pruebas existentes')]),
    fromb('INV-PBT4B-08', 'INV-PBT4C-08'),
    fromb('INV-PBT4B-09', 'INV-PBT4C-09'),
    fromb('INV-PBT4B-10', 'INV-PBT4C-10'),
    fromb('INV-PBT4B-11', 'INV-PBT4C-11'),
    fromb('INV-PBT4B-12', 'INV-PBT4C-12'),
    fromb('INV-PBT4B-13', 'INV-PBT4C-13', [('PBT4B-C13', 'PBT4C-C13')]),
    fromb('INV-PBT4B-14', 'INV-PBT4C-14'),
]
EV = [
    B['[0]'], B['[1]'], B['[2]'],
    '[3] la tabla "CT -> la asercion que falla en el RED", con el equivalente de CT-PBT2B-06 (PushBackEditAllApplyCloseTests), que falla por asercion '
    'en el RED, y las autopruebas de CT-PBT4C-07, 13 y 15;',
    B['[4]'], B['[5]'],
    '[6] la entrega con el mapa CT -> metodo, las rutas relativas comprobadas, el sha256 de cada uno de los cuatro equivalentes (iguales a EquivalentSha) '
    'y el Trailer real de los commits RED, "RED repair" (si lo hay) y GREEN; con una reparacion del RED, ademas, RedRepairSha, la prueba y la linea '
    'corregidas, el error y por que no cambia ninguna linea esperada, con el diff, y RedSha, que sigue siendo el commit RED original (F4B-E35, RR-5);',
    B['[7]'],
    fromb('[8]', '[8]', [('dentro de las seis clases de las CT', 'dentro de las cinco clases de las CT'),
                         ('En PB-T4b, ademas: RedSha..RedRepairSha no toca ningun archivo de src/ (el esqueleto), ni el kit, ni las 21 copias, ni los 21 '
                          'archivos de los Skip.',
                          'En PB-T4c, ademas: RedSha..RedRepairSha no toca ningun archivo de src/ (el esqueleto), ni los cuatro equivalentes, ni los cuatro '
                          'archivos de los Skip.')]),
    B['[9]'], B['[10]'],
    '[11] el censo de declaraciones de WHC, IWorkspacePilotEditPort.cs, WorkspacePilotSupport.cs, WEI, FVM, WorkspacePanelView.xaml, '
    'WorkspacePanelView.xaml.cs, WCo y RackPushBackSystemWindow.xaml.cs, y de los cuatro archivos nuevos, en BaseSha y en CurrentSha, contadas una a una '
    'con su nombre (una sobrecarga es una declaracion mas): la diferencia es la lista cerrada de PBT4C-C13;',
    B['[12]'],
    '[13] la entrada "## PB-T4c" de superseding-characterization.md, escrita por la sesion en el commit de emision: la tabla de las 4 (PEG:155-163, '
    'CT-PBT1B-15 y CT-PBT4B-20, C-5, en Core; CT-PBT2B-06, C-3, en UI), el sha256 de la lista, los motivos literales de los Skip, el equivalente de cada '
    'una con su EquivalentSha y su cambio declarado, y las decisiones del Owner (owner-pbt4-decisions.md, pregunta 4; owner-pbt4c-list-3-decision.md; '
    'owner-pbt4c-list-4-decision.md); su tabla de ejecucion se completa al cerrar;',
    '[14] Linea base de la CI: la del cierre de PB-T4b (F4B-E53, 6adff90d): run 44903818806 sobre su GREEN c065080d, Core 19 605 resultados, con '
    '19 147 Passed y 458 Skipped, y UI 2 481, con 2 433 Passed y 48 Skipped. Previstos, con el total exacto de cada rama (F4B-E46): PB-T4c pone 4 Skip '
    '(3 en Core y 1 en UI), cada uno sobre una prueba que pasa a un solo resultado omitido, anade 4 equivalentes (3 en Core y 1 en UI), un [Fact] cada '
    'uno, y 15 CT (11 de Core y 4 de UI), sin ninguna Theory nueva. El GREEN: Core 19 619 resultados, con 19 158 Passed, 0 Failed y 461 Skipped; UI '
    '2 486, con 2 437 Passed, 0 Failed y 49 Skipped. El RED: Core 19 619, con 19 149 Passed, 9 Failed y 461 Skipped; UI 2 486, con 2 432 Passed, 5 '
    'Failed (las 4 CT de UI y el equivalente de CT-PBT2B-06) y 49 Skipped.',
    '[15] la comprobacion mecanica de los Skip, check_skips_pbt4c_v50.py, sobre RedSha y sobre CurrentSha frente a BaseSha: los cuatro archivos, '
    'iguales a su base con su Skip literal y nada mas (4 Skip).',
]
OBJECTIVE = (
    'Anadir Editar todo al panel del Workspace de Push Back sobre el borrador completo de PB-T4b: un boton y su gesto, que confirma el texto pendiente '
    'con CommitField antes de abrir (R-PB-10); TryEditAll en el puerto, con su cuerpo por defecto, en el anfitrion y en sus dos reenvios (la compuerta de '
    'avisos y el adaptador del panel del Plugin), que empieza el borrador si hace falta, mantiene la marca de lectura mientras la ventana esta abierta '
    '(K = 1) y abre la ventana de Push Back en modo WORKSPACE en dos piezas: el modal del panel, en una clase anidada del controlador del Plugin sobre '
    'IWorkspaceFullEditHost, y la ventana, sin dueno, en una clase nueva de Plugin/Views; el lanzamiento y el adaptador del puerto de Draft/, que '
    'conservan Origin y acaban la marca known clean; la guarda de texto de OB-01.3 sobre los seis sujetos del camino; Cerrar en WORKSPACE con una sola '
    'pregunta si hay a la vez un modulo escenificado y cambios sin aplicar (A-T2b-OWNER-01); y el resumen y Modificado al cerrar, segun el paquete del '
    'contrato de PB-T4 v5.0, las decisiones del Owner sobre PB-T4 (con las dos de la lista de PB-T4c), la lista cerrada CT-PBT4C-01..15 y la lista '
    'exacta de 4 sustituciones que el Owner tomo como la que vio: cada una con su Skip de motivo literal y su equivalente, literal del paquete. Solo se '
    'sustituyen con registro la guarda de las operaciones del puerto (PEG:155-163), el censo de archivos del Plugin con PushBack (CT-PBT1B-15), la '
    'guarda del guion de Saltar y duplicar (CT-PBT4B-20) y la pregunta de Cerrar con un modulo escenificado (CT-PBT2B-06). Ni el camino clasico, ni '
    'RACKEDITAR, ni la ventana en CLASSIC; cero escrituras en el DWG desde la ventana; no se ejecuta AutoCAD.')
CODE = '\n'.join(['TaskId: F4B-PBT4-EDIT-ALL-WINDOW', '', 'Objective:', OBJECTIVE, '', 'Invariants:'] + INV + ['', 'StopConditions (propias):'] + OWN
                 + ['', 'ExpectedEvidence:'] + EV)
assert all(ord(ch) < 0x80 for ch in CODE), [ch for ch in CODE if ord(ch) >= 0x80][:5]
assert len(OWN) == 17 and len(INV) == 14 and len(EV) == 16

# RequiredTests: los 77 heredados del contrato de PB-T4b, sin el n.o 43 y con ExpectRed false, y los 9 nuevos.
dump = open(os.path.join(HERE, 'v50', 'pbt4b-contract-dump-v50.txt'), encoding='utf-8').read().split('\n')
inh = []
for line in dump:
    c = line.split('\t')
    if len(c) == 5 and c[0].isdigit():
        inh.append((int(c[0]), c[1], c[2], int(c[3])))
assert len(inh) == 77
LOWER = {7: 1, 12: 2, 36: 1, 43: 1}
NEWROWS = [
    ('tests/RackCad.Tests/RackCad.Tests.csproj', 'FullyQualifiedName~RackCad.Tests.PushBackEditAllTests', 6, True),
    ('tests/RackCad.Tests/RackCad.Tests.csproj', 'FullyQualifiedName~RackCad.Tests.PushBackEditAllGuardTests', 3, True),
    ('tests/RackCad.Tests/RackCad.Tests.csproj', 'FullyQualifiedName~RackCad.Tests.PushBackOpenerClassicPathGuardTests', 1, False),
    ('tests/RackCad.Tests/RackCad.Tests.csproj', 'FullyQualifiedName~RackCad.Tests.PushBackDraftLaunchGuardTests', 1, False),
    ('tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj', 'FullyQualifiedName~RackCad.UI.Tests.PushBackEditAllWindowTests', 4, True),
    ('tests/RackCad.Tests/RackCad.Tests.csproj', 'FullyQualifiedName~RackCad.Tests.PushBackEditAllPortEquivalenceGuardTests', 1, False),
    ('tests/RackCad.Tests/RackCad.Tests.csproj', 'FullyQualifiedName~RackCad.Tests.PushBackEditAllNameWritersGuardTests', 1, False),
    ('tests/RackCad.Tests/RackCad.Tests.csproj', 'FullyQualifiedName~RackCad.Tests.PushBackEditAllFullCopyGuardTests', 1, False),
    ('tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj', 'FullyQualifiedName~RackCad.UI.Tests.PushBackEditAllApplyCloseTests', 1, True),
]
req = ['| # | Project | Filter | MinSelected | ExpectRed |', '|---|---|---|---|---|']
for n, proj, flt, ms in inh:
    if n == 43:
        continue
    v = ms - LOWER.get(n, 0)
    req.append('| %d | `%s` | `%s` | %s | false |' % (n, proj, flt, ('**%d**' % v) if n in LOWER else str(v)))
for k, (proj, flt, ms, red) in enumerate(NEWROWS, 78):
    req.append('| %d | `%s` | `%s` | %d | %s |' % (k, proj, flt, ms, 'true' if red else 'false'))
assert len(req) - 2 == 85

skips = ['**Los `Skip` literales** (`pbt4c_skips_v50.py`, sha256 `%s`; ASCII), cada uno en la línea de atributo `[Fact]` de su prueba en `6adff90d`:'
         % S['pbt4c_skips_v50.py'], '']
for s_ in sk.SKIPS:
    skips.append('- `%s`:%d (`%s`): `%s`' % (s_[0], s_[1], s_[2], sk.skip_line(s_).strip()))
EQLINES = ['**Los equivalentes, literales** (`make_equivalents_pbt4c_v50.py`, sha256 `%s`, desde los originales de `6adff90d`; su texto, en el anexo '
           'V50-A; `EquivalentSha`, del texto con `\\n`):' % S['make_equivalents_pbt4c_v50.py'], '']
EQDESC = {mk.E1: 'PEG:155-163, con la fila de `TryEditAll` al final de `Operations` (18); Core, `ExpectRed` false',
          mk.E2: 'CT-PBT1B-15, con «archivos=15»; Core, `ExpectRed` false',
          mk.E3: 'CT-PBT4B-20, con la clase renombrada y `Restore`, que además devuelve la línea exacta del `Skip` de PB-T4c de PEG:155; Core, `ExpectRed` false',
          mk.E4: 'CT-PBT2B-06, con la línea de la ventana 4 a la pregunta combinada; UI, **`ExpectRed` true** (cae en el RED, V50-18)'}
for p in mk.PATHS:
    EQLINES.append('- `%s`, `%s`: %s' % (p, EQSHA[p], EQDESC[p]))

AUTH_KEEP = list(range(0, 43)) + [65]
s63 = '\n'.join([
    '### 6.3 PB-T4c — `F4B-PBT4-EDIT-ALL-WINDOW` (entero; V50-03, V50-04, V50-09, V50-13 y V50-16)',
    '',
    '**Se emite por la autorización condicional** (`owner-pbt4-decisions.md`, pregunta 4) **con la lista exacta de 4** que el Owner tomó como la que vio '
    '(las 2 de la pregunta 4, CT-PBT4B-20 de `owner-pbt4c-list-3-decision.md` y CT-PBT2B-06 de `owner-pbt4c-list-4-decision.md`; '
    '`pbt4c-substitution-list-4.tsv`, sha256 `%s`), si se cumple el resto de la condición, **sobre el cierre de PB-T4b** (F4B-E53, `6adff90d`).' % LIST_SHA,
    '',
    '- **Objective** (ASCII): el de §6.3.1.',
    '- **`AllowedWriteScope`** (26), ruta a ruta, en este orden:',
] + ['  - **%s:** %s;' % (label, ', '.join('%d. `%s`' % (PATHS.index(p) + 1, p) for p in ps)) for label, ps in SCOPE] + [
    '  - **ningún archivo de `Pilot/`, ni NH, ni RIA, ni `RackPushBackSystemWindow.xaml`:** el GREEN literal no los cambia.',
    '- **`UNIT_CHANGE`:** las 9 existentes de `src/` (las de la primera fila; F4B-E25).',
    '- **`ForbiddenWriteScope`:** el método de [PKC-C] con el precedente de PB-T3c, PB-T4a y PB-T4b en `tests/` (la lista explícita del contrato anterior, '
    'menos las rutas de `tests/` de este alcance; un prefijo prohibido que cubre una de ellas se sustituye por la lista de sus archivos), más las clases '
    'de PB-T4b tras su cierre (sus seis clases de las CT, el kit y las 21 copias), salvo las de este alcance (`WorkspaceFullCopyGuardTests.cs`, solo para '
    'su `Skip`). **El número lo da la regla del generador sobre el alcance final en `6adff90d`** (`<medido por el generador>`).',
    '- **El RED** (un commit, con su push antes del GREEN):',
    '  - **el esqueleto exacto de `src/`** (`pbt4c-skeleton-v50.txt`, sha256 `%s`, que aplica `green_pbt4c_v50.py`): 4 existentes (el puerto, WEI, WHC y '
    'FVM) y 4 nuevos (`IWorkspaceFullEditHost.cs`, los dos de `Draft/` y el abridor), inertes; la compuerta, WCo, PV, `WorkspacePanelView.xaml.cs` y W no '
    'cambian;' % S['pbt4c-skeleton-v50.txt'],
    '  - **los 4 `Skip`** literales, en los 4 archivos de la lista, y nada más en esos archivos;',
    '  - **los 4 equivalentes,** byte a byte los que genera `make_equivalents_pbt4c_v50.py` (su sha256, `EquivalentSha`, va en el contrato);',
    '  - **las cinco clases de las CT.**',
    '  - **La herramienta:** `python -I apply_red_pbt4c_v50.py <dir-copia> --repo <checkout> --rev 6adff90d` escribe exactamente eso salvo las CT, tras '
    'comprobar que la copia es de la revisión; el Worker puede usarla sobre su worktree o copiar sus salidas. Imprime el sha256 de cada archivo.',
    '- **El GREEN:** solo `src/`, los 13 archivos del texto literal de `pbt4c-green-v50.txt` (sha256 `%s`; `apply_green_pbt4c_v50.py` lo escribe en una '
    'copia). `tests/` no cambia después del RED (salvo una reparación del RED, solo en las clases de las CT).' % S['pbt4c-green-v50.txt'],
    '- **`RequiredTests`:** la tabla de §6.3.1.',
    '- **Invariantes:** INV-PBT4C-01 a 06 (§6.3.1) y los comunes 07 a 14.',
    '- **`StopConditions`:** las 17 estándar y PBT4C-C01 a C14, C16, C17 y C18 (§6.3.1): 34.',
    '- **`ExpectedEvidence`:** §6.3.1.',
    '- **La línea base:** el cierre de PB-T4b (F4B-E53, `6adff90d`), con la CI de su GREEN (`44903818806` sobre `c065080d`).',
    '',
    '#### 6.3.1 Los textos literales del contrato de PB-T4c',
    '',
    'Para la comparación del generador, en ASCII, campo a campo. Todo está medido en `6adff90d`; lo que da el generador va como `<medido por el generador>`.',
    '',
    '```text', CODE, '```', '',
] + skips + [''] + EQLINES + [
    '',
    '**La precondición 6** (I-02; §11.6), que corre la sesión antes de emitir, sobre copias exportadas del cierre de PB-T4b, fuera de todo commit '
    '(`pbt4ccc/compile/`, que la emisión custodia en `F4B-DRAFTS/pbt4/cc4c/`; el resumen, `pbt4c-precondition6.json`, sha256 `%s`, y su registro, '
    '`compile-check.md`, `%s`):' % (P6SHA, CCHECK),
    '1. `git archive` de `6adff90d` a una carpeta nueva, fuera del worktree;',
    '2. `python -I apply_red_pbt4c_v50.py <copia> --repo <checkout> --rev 6adff90d`: el esqueleto, los 4 `Skip` y los 4 equivalentes, sin las CT;',
    '3. la compilación de Core y de UI en Debug y del Plugin en Release con `-o`, sin ningún `warning CS`;',
    '4. `check_skips_pbt4c_v50.py` sobre la copia: los 4 archivos, iguales a su base con su `Skip`; y los filtros (`filter_overlap_pbt4c_v50b.py`, D-S8);',
    '5. **el RED exacto, sin las CT:** la suite completa de Core, **19 608 = 19 147 + 0 + 461**, con las 3 de Core de la lista omitidas con su motivo y '
    'los 3 equivalentes de Core en verde; y los 18 filtros de UI (los 17 heredados y el del equivalente de UI), donde **solo cae el equivalente de '
    'CT-PBT2B-06, por aserción**, y CT-PBT2B-06 está omitida con su motivo (D = 0);',
    '6. **sobre la misma copia, el GREEN literal** (`apply_green_pbt4c_v50.py`): la suite completa de Core, **19 608 = 19 147 + 0 + 461**, y los 18 '
    'filtros de UI, **0 `Failed`**, sin ningún `warning CS`. **Una prueba que falle fuera de la lista de 4** vuelve al Owner; **un equivalente que no '
    'pase donde se espera** necesita una versión del paquete;',
    '7. **las sondas, sobre el GREEN,** como clases temporales que no entran en ningún commit y acaban en `Assert.Fail(texto)`, cada una con su filtro: '
    '`pbt4c-probe-editall.cs` (las líneas de CT-03, 04, 05 y 12) y `pbt4c-probe-close-ui.cs` (CT-09 y D-S10); y el coste de `IsDirty` con la sonda de '
    'PB-T4b (`pbt4bcc/compile/TmpIsDirtyCostProbe.cs`), en Release;',
    '8. el contrato lleva por sha256 las piezas del GREEN y del esqueleto, los 4 equivalentes (`EquivalentSha`) y la lista.',
    '',
    '**Lo medido** (`/d/r5g` para los pasos 2 a 6, `/d/r5f` para el 7 y `/d/r5h` para el coste; la vuelta parada, `/d/r5a` a `/d/r5e`, es historia, '
    'con la medida de D-S9 en `/d/r5e`): lo previsto, con D = 0, K = 4, E = 4 y cero `warning CS`. **La duración de la suite de Core:** %d s en el RED '
    'y %d s en el GREEN. **El coste de `IsDirty`:** el de PB-T4b, dentro del ruido (pb-20-ab, mediana limpia 9.64 ms frente a 9.35 ms).'
    % (p6['SuiteDurationSeconds']['core_red'], p6['SuiteDurationSeconds']['core_green']),
    '',
    '**`RequiredTests`** (76 heredados + 9 nuevos = 85). Los heredados son los 77 del contrato de PB-T4b (`F4B-PBT4-FULL-DRAFT-PANEL`, emitido), con su '
    'número, **todos con `ExpectRed` false** (las CT, las clases Red de la partición y sus filtros pasan desde el cierre de PB-T4b), y sus `MinSelected` '
    'bajan en una por cada prueba de la lista que seleccionan; **el n.º 43 queda en 0 y su fila sale** (V50-16). Los nuevos, del 78 al 86, por clase '
    'exacta (D-S8):',
    '',
] + req + [
    '',
    '- **Nota (no va al contrato).** `MinSelected` cuenta resultados no omitidos, como en el contrato de PB-T4b (medido en las TRX de `/d/r5g`: n.º 5, 85 '
    'resultados de 40 métodos). **Los cuatro que bajan:** n.º 7, `PushBackSharedReferenceGuardTests` (CT-PBT1B-15), de 2 a **1**; **n.º 12, '
    '`RackCad.Tests.Workspace`, que selecciona PEG y `WorkspaceFullCopyGuardTests`** (PEG:155-163 y CT-PBT4B-20; la duda del generador decía que '
    'PEG no caía en ningún filtro heredado, y no es así: el filtro es una subcadena del nombre completo `RackCad.Tests.WorkspacePilotEditGuardTests…`), '
    'de 2 718 a **2 716**; n.º 36, `PushBackWorkspaceApplyCloseTests` (CT-PBT2B-06), de 4 a **3**; y n.º 43, `WorkspaceFullCopyGuardTests`, de 1 a 0, '
    'así que sale. **La prueba de PEG de la lista no necesita filtro nuevo:** su equivalente tiene el suyo (n.º 83) y el n.º 12 sigue cubriendo las '
    'demás de PEG. Ningún filtro heredado selecciona una clase nueva, y cada filtro nuevo solo la suya (`filter_overlap_pbt4c_v50b.py`; '
    '`pbt4ccc/compile/out-redgreen-v50b-r5g/filter-overlap-v50b.txt`).',
    '',
    '#### 6.3.2 Lo que necesita el generador de PB-T4c (las dudas de la fase 1, `generator-doubts-pbt4c.md`)',
    '',
    '| Pieza | Dónde, en este paquete | Lo que comprueba el generador |',
    '|---|---|---|',
    '| **`TaskId`** | §6.3.1 | `F4B-PBT4-EDIT-ALL-WINDOW`, 24 caracteres, ≤ 26 |',
    '| **`Objective`** | §6.3.1 | ASCII, byte a byte; cita el paquete v5.0, CT-PBT4C-01..15 y las **4** sustituciones |',
    '| **`AllowedWriteScope`** | §6.3 | Las 26 rutas, en ese orden y con su número; ninguna de `Pilot/`; de `src/RackCad.Plugin/`, solo WCo y '
    '`Views/RackWorkspaceEditorWindow.cs` |',
    '| **`ForbiddenWriteScope`** | §6.0 y §6.3 | Su regla, sobre el alcance final en `6adff90d` (`<medido por el generador>`) |',
    '| **`UNIT_CHANGE`** | §6.0 y §6.3 | Las 9 existentes de `src/` |',
    '| **`Invariants`** | §6.3.1 | INV-PBT4C-01 a 14, literales |',
    '| **`StopConditions`** | §6.0 y §6.3.1 | Las 17 estándar del contrato de PB-T4b y PBT4C-C01 a C14, C16, C17 y C18: **34**. **C11** es `PBT4B-C11` con '
    '«PB-T4b» → «PB-T4c». **C02** no tiene excepción fuera de los `Skip` de la lista de 4 |',
    '| **`RequiredTests`** | §6.3.1 | Las 85 filas: los 76 heredados con su número y sus `MinSelected` (n.º 7: **1**; n.º 12: **2 716**; n.º 36: **3**; '
    'el n.º 43 sale), todos con `ExpectRed` false, y los 9 nuevos (n.º 78 a 86), por clase exacta (D-S8): 5 de las CT y 4 de los equivalentes |',
    '| **`ExpectedEvidence`** | §6.3.1 | 16; `[0]` byte a byte el de PB-T3c; `[13]`, la entrada «## PB-T4c» del registro declarado; `[14]`, con los '
    'totales de §5.3 y la base de PB-T4b; la cuenta de CT (**15**), de `[Fact]` (15), de `Skip` (**4**) y de filtros con `ExpectRed` true (**4**: 2 de '
    'Core y 2 de UI, con 14 resultados: 9 de Core y 5 de UI) |',
    '| **La lista** | `pbt4c-substitution-list-4.tsv` | sha256 `%s`; 4 filas (3 C-5 en Core y 1 C-3 en UI), cada una un `[Fact]` sin `Skip` en su rango '
    'en `6adff90d`; la genera `make_list_pbt4c_4.py` (`%s`). La de 3 (`%s`) y las filas «PB-T4c» de `pbt4-substitution-lists.tsv` quedan como historia |'
    % (LIST_SHA, S['make_list_pbt4c_4.py'], S['pbt4c-substitution-list-3.tsv']),
    '| **El esqueleto y el GREEN** | `pbt4c-skeleton-v50.txt` y `pbt4c-green-v50.txt`, con `green_pbt4c_v50.py` | **Por su texto en el paquete**, no como '
    'parche: dos piezas de texto sin escapes, con bloques `EDIT` (un `OLD` que aparece exactamente una vez y su `NEW`) y `NEWFILE`, y `FROMGREEN` en el '
    'esqueleto; el generador las aplica sobre `6adff90d` y comprueba que cada `OLD` está una vez. sha256: `%s`, `%s` y `%s`. Su texto, en el anexo V50-A |'
    % (S['pbt4c-skeleton-v50.txt'], S['pbt4c-green-v50.txt'], S['green_pbt4c_v50.py']),
    '| **Los `Skip`, los equivalentes y su comprobación** | §6.3.1 | Por sha256: `pbt4c_skips_v50.py` (`%s`), `make_equivalents_pbt4c_v50.py` (`%s`), '
    '`apply_red_pbt4c_v50.py` (`%s`), `apply_green_pbt4c_v50.py` (`%s`) y `check_skips_pbt4c_v50.py` (`%s`); los 4 `EquivalentSha` |'
    % (S['pbt4c_skips_v50.py'], S['make_equivalents_pbt4c_v50.py'], S['apply_red_pbt4c_v50.py'], S['apply_green_pbt4c_v50.py'],
       S['check_skips_pbt4c_v50.py']),
    '| **Las CT como datos** | `pbt4c_ct_table_v50.py` | sha256 `%s`: la tabla de §5.3, que lee la lectura RR-3 de la cadena. **Pasada sobre esta '
    'versión** (`pbt4ccc/pbt4c_rr3_reading.py`, `7585a87e…`, sin tocarla; `v50/check_rr3_v50.py` y su salida): 0 errores; las cinco clases de las CT, '
    'con sus líneas iguales a las de sus CT y nada pendiente; los cuatro equivalentes, sin fila (no admiten reparación) |' % S['pbt4c_ct_table_v50.py'],
    '| **La precondición 6** | `pbt4ccc/compile/pbt4c-precondition6.json` | Schema `rackcad-pbt4c-precondition6/v1`; `Revision` (`6adff90d…`), `List` '
    '(su sha256, K = 4: 3 de Core y 1 de UI), `Pieces` (el sha256 de cada pieza), `Skeleton`, `GreenFiles`, `SkipFiles` y `EquivalentSha` (el sha256 de '
    'cada archivo escrito), `Builds` y `CsWarnings` (0), `Red` y `Green` (los totales de Core y cada filtro de UI con su TRX), `D` (0), `E` (4), '
    '`SuiteDurationSeconds`, `InheritedCoreFiltersGreen` (los resultados y métodos de cada filtro heredado), `FilterOverlap`, `CheckSkips`, `Probes`, '
    '`IsDirtyCost` y `History` (la vuelta parada). sha256 `%s`; la corrida, en su `compile-check.md` (`%s`). La carpeta va a `F4B-DRAFTS/pbt4/cc4c/` |'
    % (P6SHA, CCHECK),
    '| **CA-T4c** | `independent-review-v1.md` | **`BOUNDED_SECTIONS`: la §22** (la comprobación acotada de PB-T4c sobre esta versión, la v5.0; en '
    '`6adff90d`, el informe acaba en la §21, la recomprobación de la v4.9) y, si hay recomprobación, la siguiente; el revisor las escribe tras leer el '
    'paquete. **La línea `BOUNDED_RESULT`:** como en PB-T4b (`make_pbt4b_contract.py:53-60`), la línea BOUNDED literal que el revisor propone en su '
    'sección, en ASCII (`EMITIBLE` o `EMITIBLE TRAS CORRECCIONES`, con «B n / I n / M n»), con la cola de la recomprobación si la hay; el generador la '
    'comprueba con `bounded_check` contra esas secciones. La emisión necesita 0 bloqueantes y 0 importantes sin resolver (pregunta 4) |',
    '| **La numeración** | — | F4B-E53 (el cierre de PB-T4b), F4B-E54 (la emisión) y las secciones 283 (X) y 284 (BASE) |',
    '',
    '**Las autoridades de PB-T4c** (la n.º y la ruta son las del contrato de PB-T4b emitido, con sus 72):',
    '- **Siguen, sin cambio:** las n.º 1 a 5, 7 a 27 y 30 a 35, y 38 a 42.',
    '- **Siguen, con su sección al día:**',
    '  - la n.º 0, el paquete, en la v5.0, con sus secciones de PB-T4c (D-PBT4-12 a 17, §4.3, §5.3, §6.0 y §6.3);',
    '  - la n.º 6, `superseding-characterization.md`, con su entrada «## PB-T4c» (`[13]`);',
    '  - la n.º 28, `f4b-dispositions.md`, hasta F4B-E53;',
    '  - la n.º 29, el contrato anterior: el de PB-T4b (`F4B-PBT4-FULL-DRAFT-PANEL/coordinator/gate-contract.json`), en lugar del de PB-T3c;',
    '  - la n.º 36, la base: el cierre de PB-T4b (`F4B-PBT4-FULL-DRAFT-PANEL/task-close-package.md`, F4B-E53, con la CI de la línea base, '
    '`44903818806`), en lugar del de PB-T3c;',
    '  - la n.º 37, «este contrato»: `F4B-PBT4-EDIT-ALL-WINDOW/coordinator/gate-contract.json`;',
    '  - la n.º 65, `independent-review-v1.md`, con su §22 (CA-T4c) y su recomprobación, si la hay.',
    '- **No son autoridades de PB-T4c:** las n.º 43 a 64 (las tandas, la lista de 478, el mapa, el kit, los guiones de la copia, su revisión y la '
    'precondición 6 de PB-T4b) y las n.º 66 a 71 (los `UNIT_CHANGE` de PB-T4b).',
    '- **Se añaden:**',
    '  - `owner-pbt4c-list-3-decision.md` y `owner-pbt4c-list-4-decision.md` (la octava y la novena tandas, sobre la lista de PB-T4c);',
    '  - `session-pbt4c-design-decisions.md` (D-S7 a D-S10);',
    '  - `owner-pbt2b-close-decisions.md` (F4B-E38; A-T2b-OWNER-01, :36);',
    '  - `owner-pbt4b-code-decisions.md` (F4B-E53; los vacíos de M-04 van a la lista de PB-T4d, no a esta);',
    '  - `pbt4c-substitution-list-4.tsv`, las piezas y los guiones de §6.3.2 y `pbt4c-precondition6.json` (UNIT_DOC);',
    '  - y los 9 `UNIT_CHANGE` de `src/` (el texto literal del GREEN de PB-T4c, `pbt4c-green-v50.txt`).',
    '',
    '',
])
SECTION('### 6.3 PB-T4c', '### 6.4 PB-T4d', s63, '§6.3')

# ---------------------------------------------------------------- §7, §8.3, §8.5, §9 y §11
R('| R-T4-13, el modal y el abridor | Dos piezas; CT-PBT4C-08 fija el literal de la conversión (M-30) |',
  '| R-T4-13, el modal y el abridor | Dos piezas; CT-PBT4C-08 fija el literal de la conversión (M-30) |\n'
  '| **R-T4-27, lo que el paseo no ve** (V50-12) | La reflexión sobre tipos y el comportamiento de la UI: las dos caídas de la v5.0 las vio la suite '
  'completa de Core y los filtros de UI de la precondición 6, no el paseo. Lo que vale es la suite completa |\n'
  '| **R-T4-28, un nombre fijado y nunca ejecutado** (D-S9) | `IWorkspaceFullEditorHost` estaba en el paquete desde la v4 y una guarda por reflexión lo '
  'prohibía; se midió al compilar y correr el GREEN literal |')
R('| PB-T4c | PEG:155-163 y CT-PBT1B-15 (C-5) | 2 | **2** | Autorización condicional |',
  '| PB-T4c | PEG:155-163, CT-PBT1B-15 y CT-PBT4B-20 (C-5, Core), y CT-PBT2B-06 (C-3, UI) | 2; tras la remedición, 3 y 4 | **4** | **Aprobada como la que '
  'vio** (`owner-pbt4c-list-3-decision.md` y `owner-pbt4c-list-4-decision.md`); `pbt4c-substitution-list-4.tsv`, sha256 `%s` (la de 3, `%s`, queda '
  'como historia) |' % (LIST_SHA, S['pbt4c-substitution-list-3.tsv']))
R('**Las decisiones de la sesión,** que no son del Owner:\n',
  '| **8.ª** (2026-11-21) | `owner-pbt4c-list-3-decision.md` | «Lista T4c»: «Añadirla a la lista (Recomendado)». «Otro diseño sin tocarla» y «No emitir '
  'PB-T4c» no se hacen | **La lista de PB-T4c pasa de 2 a 3** (CT-PBT4B-20), con su `Skip` y su equivalente, copia literal que además reconoce el `Skip` '
  'de PB-T4c (lo que fija para PB-T4c) |\n'
  '| **9.ª** (2026-11-21) | `owner-pbt4c-list-4-decision.md` | «Lista T4c»: «Añadirla a la lista (Recomendado)». «Preguntar distinto» y «No cambiar '
  '«Cerrar» ahora» no se hacen | **La lista de PB-T4c pasa de 3 a 4** (CT-PBT2B-06), '
  'con su `Skip` y su equivalente, copia literal con la línea de la ventana 4 a la pregunta nueva (lo que fija para PB-T4c) |\n\n'
  '**Las decisiones de la sesión,** que no son del Owner:\n'
  '- **D-S7 a D-S10** (`session-pbt4c-design-decisions.md`, 2026-11-21; para PB-T4c): **D-S7**, la costura de CT-09 es `HeaderConfiguratorDialog`; '
  '**D-S8**, los filtros nuevos, por clase exacta; **D-S9**, `IWorkspaceFullEditHost` (y `SetFullEditHost`, `_fullEditHost`), por '
  '`WorkspaceModelBoundaryTests:101`; **D-S10**, CT-09 cubre lo que se puede alcanzar (en WORKSPACE, un módulo escenificado deja siempre cambios sin '
  'aplicar). Ninguna cambia la lista ni el comportamiento visible decidido por el Owner;\n')
R('- **Quedan para la copia exportada:** K, D y E de PB-T4c;',
  '- **Medidos en la copia exportada, para PB-T4c** (V50-12): K = 4, D = 0 y E = 4 (`/d/r5g`), el coste de `IsDirty` (`/d/r5h`) y las líneas de las '
  'sondas (`/d/r5f`). **Lo que el paseo de guardas no ve:** la reflexión sobre tipos y el comportamiento de la UI; lo ve la suite completa de Core y '
  'los filtros de UI.\n- **Quedaban para la copia exportada:** K, D y E de PB-T4c;')
R('y `owner-pbt4b-cost-decision.md`), con su número F4B-E al custodiarlas;',
  'y `owner-pbt4b-cost-decision.md`; y, para PB-T4c, `owner-pbt4c-list-3-decision.md` y `owner-pbt4c-list-4-decision.md`), con su número F4B-E al '
  'custodiarlas;')
R('   - en PB-T4c, además: K, D y E, el coste de `IsDirty` y la duración de la suite.',
  '   - **PB-T4c** (los ocho pasos de §6.3.1; `pbt4ccc/compile/compile-check.md`), sobre copias exportadas del cierre de PB-T4b (`6adff90d`): '
  '`apply_red_pbt4c_v50.py`, la compilación sin ningún `warning CS`, `check_skips_pbt4c_v50.py` y los filtros; **el RED exacto,** sin las CT: **Core '
  '19 608 = 19 147 + 0 + 461** y los 18 filtros de UI, donde solo cae el equivalente de CT-PBT2B-06, por aserción; **el GREEN literal:** **Core '
  '19 608 = 19 147 + 0 + 461** y los 18 filtros, 0 `Failed`; las sondas de las líneas de las CT y el coste de `IsDirty`; **K = 4, D = 0 y E = 4**, y '
  'la duración de la suite. Medido (`pbt4c-precondition6.json`, `%s`). **Una prueba que falle fuera de la lista de 4** vuelve al Owner.' % P6SHA)
R('que el generador comprueba; PB-T4c, 2.',
  'que el generador comprueba; **PB-T4c, las 4 de `pbt4c-substitution-list-4.tsv`** (sha256 `%s`).' % LIST_SHA)
R('**Para PB-T4d:** el cierre de PB-T4c, su lista vuelta a medir y la respuesta del Owner.',
  '**En el commit de emisión de PB-T4c,** además: esta versión (v5.0), los fragmentos de `v50/`, las listas (`pbt4c-substitution-list-4.tsv` y la de 3), '
  'las piezas y los guiones de §6.3.2 y sus salidas, las sondas, la remedición (`v50/remeasure_pbt4c_v50.py` y su salida), el paseo de guardas, la '
  'corrida de la precondición 6 (`pbt4ccc/compile/`: `compile-check.md`, `pbt4c-precondition6.json`, sus guiones, sus registros y el sha256 de sus TRX '
  'y binarios, con la vuelta parada como historia y la medida de D-S9 de `v50/variant-rename/`), los registros del Owner de la octava y la novena '
  'tandas, `session-pbt4c-design-decisions.md` y la entrada «## PB-T4c» de `superseding-characterization.md`. **Para PB-T4d:** el cierre de PB-T4c, su '
  'lista vuelta a medir y la respuesta del Owner.')

# ---------------------------------------------------------------- el anexo V50-A
annex = ['## Anexo V50-A. El texto literal de PB-T4c: el GREEN, el esqueleto y los equivalentes', '',
         'Copiado por `make_v50.py`. **Lo que manda es el sha256 de cada pieza** (§6.3.2); este anexo es para leerlo sin generar nada.', '',
         '### La pieza del GREEN, `pbt4c-green-v50.txt` (sha256 `%s`)' % S['pbt4c-green-v50.txt'], '', '```text',
         open(os.path.join(HERE, 'pbt4c-green-v50.txt'), encoding='utf-8').read().rstrip('\n'), '```', '',
         '### La pieza del esqueleto, `pbt4c-skeleton-v50.txt` (sha256 `%s`)' % S['pbt4c-skeleton-v50.txt'], '', '```text',
         open(os.path.join(HERE, 'pbt4c-skeleton-v50.txt'), encoding='utf-8').read().rstrip('\n'), '```', '']
for p, title in ((mk.E1, 'PEG:155-163'), (mk.E2, 'CT-PBT1B-15'), (mk.E4, 'CT-PBT2B-06')):
    annex += ['### El equivalente de %s, `%s` (`EquivalentSha` `%s`)' % (title, p, EQSHA[p]), '', '```csharp', EQ[p].rstrip('\n'), '```', '']
annex += ['### El equivalente de CT-PBT4B-20, `%s` (`EquivalentSha` `%s`)' % (mk.E3, EQSHA[mk.E3]), '',
          'Es `tests/RackCad.Tests/WorkspaceFullCopyGuardTests.cs` de `6adff90d` entero (%d líneas), con dos cambios y nada más:'
          % mk.show(REPO, REV, mk.FCG).count('\n'), '',
          '1. la clase: `%s` → `%s`;' % (mk.FCG_CLASS_OLD.strip(), mk.FCG_CLASS_NEW.strip()), '', '2. `Restore`:', '', '```csharp',
          mk.FCG_RESTORE_OLD.rstrip('\n'), '```', '', 'pasa a:', '', '```csharp', mk.fcg_restore_new().rstrip('\n'), '```', '']
for part in annex:
    assert '```' not in part or part.startswith('```'), part[:40]
ANNEX = '\n'.join(annex)

# ---------------------------------------------------------------- comprobaciones y escritura
v = t.rstrip('\n') + '\n\n' + ANNEX + '\n'
for k_ in KEEP:
    assert v.count(k_) == 1, 'PB-T4a o PB-T4b cambiaron'
assert not re.search(r'@@[A-Z0-9]+@@', v)  # las piezas del anexo llevan sus marcas @@@@; ningun hueco de los fragmentos queda
data = v.encode('utf-8')
assert b'\r' not in data
open(OUT, 'wb').write(data)
print('v5.0', hashlib.sha256(data).hexdigest(), len(data), 'bytes; reemplazos', len(done))
print('lista', LIST_SHA, '| GREEN', S['pbt4c-green-v50.txt'], '| esqueleto', S['pbt4c-skeleton-v50.txt'], '| JSON', P6SHA, '| compile-check', CCHECK)
print('sesion', S['session-pbt4c-design-decisions.md'], '| CT', S['pbt4c_ct_table_v50.py'])
