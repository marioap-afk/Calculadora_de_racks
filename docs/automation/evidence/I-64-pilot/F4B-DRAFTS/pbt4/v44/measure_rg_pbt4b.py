"""PB-T4b (D-S2): RG (WorkspacePilotReconciliationGuardTests) :104, :128 y :132 sobre el GREEN literal de PB-T4b, emulando ReaderViolations
(RG:69-100) con las mismas reglas: Src.Code (sin comentarios), Src.Count (Regex.Matches), Src.BodyOf (la primera llave tras la firma).
Lee los .cs de src/RackCad.UI/ de 5aaacce3 por objetos git y sustituye FVM por el del GREEN (green_pbt4b.py). Uso: python -I measure_rg_pbt4b.py"""
import importlib.util, os, re, subprocess, sys
sys.stdout.reconfigure(encoding='utf-8', errors='replace')
HERE = os.path.dirname(os.path.abspath(__file__))
P4 = os.path.dirname(HERE)
REPO = r'D:' + chr(92) + 'Documentos' + chr(92) + 'Codex' + chr(92) + 'Calculadora de racks'
REV = '5aaacce3'
spec = importlib.util.spec_from_file_location('gb', os.path.join(P4, 'green_pbt4b.py'))
gb = importlib.util.module_from_spec(spec)
spec.loader.exec_module(gb)
base, green = gb.green(REPO, REV)
FIELD = 'src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs'
BROWSER = 'src/RackCad.UI/Workspace/WorkspaceBrowserViewModel.cs'


def code_only(t):
    return re.sub(r'//[^\r\n]*', '', re.sub(r'/\*.*?\*/', '', t, flags=re.S))


def body_of(code, sig):
    m = re.search(sig, code)
    assert m, sig
    o = code.find('{', m.end() - 1)
    a = code.find('=>', m.end() - 1)
    if a >= 0 and (o < 0 or a < o):
        return code[m.start():code.find(';', a) + 1]
    d = 0
    for i in range(o, len(code)):
        if code[i] == '{':
            d += 1
        elif code[i] == '}':
            d -= 1
            if d == 0:
                return code[m.start():i + 1]


FOUR = [('TryConfirmBase', 'CheckNow'), ('TryCheckNow', 'CheckNow'), ('TryKeepMine', 'ConfirmChoice'), ('TryDiscardAndReload', 'ConfirmChoice')]


def violations(files):
    out = []
    for token, body in FOUR:
        pat = r'\b' + token + r'\b'
        for p, c in files:
            if p != FIELD and len(re.findall(pat, c)) > 0:
                out.append(p + ' names ' + token)
        field = [c for p, c in files if p == FIELD]
        if len(field) != 1:
            out.append('no field')
            continue
        inv = r'\b_port\s*\.\s*' + token + r'\s*\(\s*request\s*\)'
        fc = field[0]
        if len(re.findall(pat, fc)) != 1: out.append('the field does not name %s exactly once (%d)' % (token, len(re.findall(pat, fc))))
        if len(re.findall(inv, fc)) != 1: out.append('the field does not invoke _port.%s(request) exactly once' % token)
        if not re.search(r'public\s+void\s+' + body + r'\s*\(\s*\)', fc):
            out.append('no ' + body)
        else:
            g = body_of(fc, r'public\s+void\s+' + body + r'\s*\(\s*\)')
            if len(re.findall(pat, g)) != 1: out.append(token + ' is not inside ' + body)
            if len(re.findall(inv, g)) != 1: out.append('_port.%s(request) is not invoked inside %s' % (token, body))
    return out


ls = subprocess.run(['git', '-C', REPO, 'ls-tree', '-r', '--name-only', REV, '--', 'src/RackCad.UI/'], capture_output=True).stdout.decode('utf-8').split('\n')
ui = [p for p in ls if p.endswith('.cs') and '/obj/' not in p and '/bin/' not in p]
files_base = []
for p in sorted(ui):
    t = base[p] if p in base else subprocess.run(['git', '-C', REPO, 'show', REV + ':' + p], capture_output=True).stdout.decode('utf-8-sig')
    files_base.append((p, code_only(t.replace('\r\n', '\n'))))
files_green = [(p, code_only(green[p]) if p in green else c) for p, c in files_base]
fb = dict(files_base)[FIELD]
fg = dict(files_green)[FIELD]
print('RG:104 ReaderViolations base =', violations(files_base))
print('RG:104 ReaderViolations green =', violations(files_green))
snips = {
    'RG:116 RequestKeepMine': 'public void RequestKeepMine() => AskChoice(ChoiceKind.KeepMine);',
    'RG:117 PilotFieldView view;': 'PilotFieldView view;',
    'RG:120 SurfaceChangedText': 'private const string SurfaceChangedText',
    'RG:132 choice': 'var outcome = kind == ChoiceKind.KeepMine ? _port.TryKeepMine(request) : _port.TryDiscardAndReload(request);',
    'RG:133 confirm': 'var outcome = _port.TryConfirmBase(request);',
    'RG:134 check': 'var outcome = _port.TryCheckNow(request);',
}
for k, s in snips.items():
    print('%-28s base=%d green=%d' % (k, fb.count(s), fg.count(s)))
# RG:132-145 sobre el GREEN: la mutacion del grupo de metodos dentro de ConfirmChoice
keep = fg.replace(snips['RG:132 choice'], '_keep = _port.TryKeepMine; var outcome = kind == ChoiceKind.KeepMine ? _keep(request) : _port.TryDiscardAndReload(request);')
print('RG:139 TryKeepMine en ConfirmChoice tras la mutacion =', len(re.findall(r'\bTryKeepMine\b', body_of(keep, r'public\s+void\s+ConfirmChoice\s*\(\s*\)'))))
print('RG:140 violaciones tras la mutacion (no vacia) =', bool(violations([(p, keep if p == FIELD else c) for p, c in files_green])))
mut116 = fg.replace(snips['RG:116 RequestKeepMine'], 'public void RequestKeepMine() { _port.TryKeepMine(null); }')
print('RG:116 violaciones tras la mutacion (no vacia) =', bool(violations([(p, mut116 if p == FIELD else c) for p, c in files_green])))
