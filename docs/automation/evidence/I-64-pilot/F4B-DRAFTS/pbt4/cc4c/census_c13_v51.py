"""PB-T4c (paquete v5.1), precondicion 6, paso 8: el censo de las declaraciones de PBT4C-C13 y de los nombres, por texto, sobre los 13 archivos
de src/ del GREEN literal y del esqueleto (green_pbt4c_v51.green y .skeleton, por objetos git de la base: no toca ninguna copia).

- Las declaraciones de cada archivo C# (sin comentarios y con las cadenas vaciadas), con su tipo contenedor: los tipos (class, struct,
  interface, enum, record), los miembros a nivel de tipo (metodos, constructores, propiedades, campos, constantes, implementaciones explicitas
  como Interfaz.Metodo; una sobrecarga es una mas) y los miembros de los enumerados; en XAML, cada x:Name. La diferencia GREEN - base tiene que
  ser, nombre a nombre y con su cuenta, la lista cerrada de PBT4C-C13, y la base - GREEN vacia; la del esqueleto, una parte de esa lista.
- Los nombres (WorkspaceModelBoundaryTests:83, :101 y :102 de 6adff90d, y PEG:1015 por el tipo del puerto y del soporte): ningun tipo nuevo de
  src/RackCad.Application/Workspace/ lleva Subscription, Editor, Draft ni Tab (salvo BrowserTab), y ningun tipo ni miembro publico nuevo de ahi
  lleva ProjectSummary, Metric, Provider, TotalRacks, Aggregat ni Bom.
Es un censo por texto, no un compilador: lo que no ve, lo dice la salida. Escribe en UTF-8 con LF.

Uso: python -I census_c13_v51.py <repo> <rev> <salida.txt>
"""
import collections
import importlib.util
import os
import re
import sys

sys.dont_write_bytecode = True
sys.stdout.reconfigure(encoding='utf-8', newline='\n')
S = os.path.dirname(os.path.abspath(__file__))
PB4 = os.path.normpath(os.path.join(S, '..', '..', 'pbt4'))
spec = importlib.util.spec_from_file_location('green_pbt4c_v51', os.path.join(PB4, 'green_pbt4c_v51.py'))
gp = importlib.util.module_from_spec(spec)
spec.loader.exec_module(gp)
repo, rev, out = sys.argv[1:4]

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

# La lista cerrada de PBT4C-C13 (v5.0:2398; la misma en la v5.1), como Contenedor.Nombre; "tipo:" marca un tipo.
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
KEYWORDS = {'if', 'for', 'foreach', 'while', 'switch', 'using', 'lock', 'catch', 'return', 'new', 'nameof', 'typeof', 'sizeof', 'default', 'base',
            'this', 'throw', 'when', 'where', 'get', 'set', 'init', 'add', 'remove', 'fixed', 'checked', 'unchecked'}


def clean(text):
    """Sin comentarios y con las cadenas y los caracteres vaciados (las guardas de texto hacen lo mismo)."""
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
    stack = []  # ('tipo', nombre, clase) | ('miembro',) | ('otro',)
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


def census_xaml(text):
    return collections.Counter('x:Name.' + n for n in re.findall(r'\bx:Name="(\w+)"', text))


def census(path, text):
    if text is None:
        return collections.Counter()
    return census_xaml(text) if path.endswith('.xaml') else census_cs(text)


base_g, green = gp.green(repo, rev)
base_s, red = gp.skeleton(repo, rev)
lines = ['# PBT4C-C13 por texto: GREEN (green_pbt4c_v51.green) y esqueleto (green_pbt4c_v51.skeleton) frente a la base %s' % rev]
ok = True
for path in C13:
    base = base_g.get(path) or base_s.get(path)
    b = census(path, base)
    g = census(path, green.get(path))
    r = census(path, red.get(path, base))
    added, removed = g - b, b - g
    want = collections.Counter(C13[path])
    radded = r - b
    same = added == want and not removed
    subset = not (radded - want) and not (b - r)
    ok = ok and same and subset
    lines.append('%s: nuevas=%d esperadas=%d igual=%s quitadas=%s | esqueleto nuevas=%d dentro-de-la-lista=%s quitadas=%s' % (
        path, sum(added.values()), sum(want.values()), same, sorted(removed.elements()), sum(radded.values()), not (radded - want),
        sorted((b - r).elements())))
    if not same:
        lines.append('  de mas: %s' % sorted((added - want).elements()))
        lines.append('  de menos: %s' % sorted((want - added).elements()))
    if radded - want:
        lines.append('  esqueleto, fuera de la lista: %s' % sorted((radded - want).elements()))
# Los nombres de RackCad.Application.Workspace.
names_bad = []
for path in C13:
    if not path.startswith('src/RackCad.Application/Workspace/'):
        continue
    base = base_g.get(path) or base_s.get(path)
    added = census(path, green.get(path)) - census(path, base)
    for key in added:
        short = key.split('.')[-1].replace('tipo:', '')
        if key.startswith('tipo:') and (re.search(r'Subscription|Editor|Draft', short) or (re.search('Tab', short) and not short.startswith('BrowserTab'))):
            names_bad.append(key)
        if re.search(r'ProjectSummary|Metric|Provider|TotalRacks|Aggregat|Bom\b', short):
            names_bad.append(key)
lines.append('nombres de RackCad.Application.Workspace: violaciones=%s' % names_bad)
ok = ok and not names_bad
# Las autopruebas del censo: una declaracion de mas inyectada en el texto del GREEN tiene que dejar el archivo distinto de la lista.
SELF = [
    ('constante', WHC, '        private const string EditAllStateReason', '        private const string Extra = "x";\n        private const string EditAllStateReason'),
    ('sobrecarga', FVM, '        public void EditAll()', '        public void EditAll(int extra) { }\n\n        public void EditAll()'),
    ('propiedad', WEI, '        public int Applied { get; }', '        public int Applied { get; }\n\n        public int Extra => 0;'),
    ('miembro-de-enum', WEI, '        Failed,', '        Failed,\n        Extra,'),
    ('tipo-anidado', WCO, '        private sealed class FullEditorModal', '        private sealed class Extra { }\n\n        private sealed class FullEditorModal'),
    ('x:Name', PV, 'x:Name="BrowserFieldEditAllButton"', 'x:Name="BrowserFieldEditAllButton" Tag="x"><Button.Tag><TextBlock x:Name="Extra"/></Button.Tag></Button><Button'),
    ('quitada', SPORT, '        private readonly Action _applied;\n', ''),
]
for label, path, old, new in SELF:
    text = green[path]
    if old not in text:
        lines.append('autoprueba-%s: el anclaje no esta (%s)' % (label, path))
        ok = False
        continue
    mutated = text.replace(old, new, 1)
    base = base_g.get(path) or base_s.get(path)
    c = census(path, mutated)
    same = (c - census(path, base)) == collections.Counter(C13[path]) and not (census(path, base) - c)
    lines.append('autoprueba-%s=%s' % (label, 'falla' if not same else 'pasa'))
    ok = ok and not same
lines.append('Lo que no ve: la visibilidad y la firma (solo el nombre y su cuenta), los miembros generados por el compilador, las declaraciones '
             'locales y las lambdas; la reflexion sobre el ensamblado la mide la suite completa de Core.')
lines.append('censo C13: %s' % ('PASA' if ok else 'PARA'))
with open(out, 'w', encoding='utf-8', newline='\n') as f:
    f.write('\n'.join(lines) + '\n')
print(lines[-1])
