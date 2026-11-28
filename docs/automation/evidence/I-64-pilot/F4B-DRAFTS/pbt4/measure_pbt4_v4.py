"""PB-T4, paquete v4: mediciones de solo lectura sobre una revision (por defecto 7f0d9aa3, el cierre de PB-T3c).

Uso: python -I measure_pbt4_v4.py <checkout principal> <rev> <salida.txt>
Solo llama a `git -C <checkout> show|grep|ls-tree|diff` (objetos git); escribe solo <salida.txt>.
A. Las senales por causa de cada metodo de las 9 clases: su cuerpo y, a un nivel, los ayudantes del mismo archivo que llama por su nombre.
B. Las pruebas propias del piloto que se retirarian (Q1 «Retirar»), por archivo y metodo, con un nivel de ayudantes.
C. Las pruebas que nombran NumericField o NumericFieldValidation.
D. El censo de CT-PBT1B-15 (archivos del Plugin cuyo codigo contiene PushBack) y lo que hay en las carpetas candidatas del abridor.
E. Extractos.
"""
import re
import subprocess
import sys

repo, rev, out_path = sys.argv[1:4]
out = []


def git(*args, ok=(0,)):
    p = subprocess.run(['git', '-C', repo] + list(args), capture_output=True)
    if p.returncode not in ok:
        raise SystemExit('git %s -> %d' % (' '.join(args), p.returncode))
    return p.stdout.decode('utf-8', 'replace').replace('\r\n', '\n')


def show(path):
    return git('show', '%s:%s' % (rev, path))


def strip_comments(code):
    code = re.sub(r'/\*.*?\*/', '', code, flags=re.S)
    return re.sub(r'//[^\n]*', '', code)


def members(code):
    """(nombre, es_prueba, cuerpo) de cada metodo con cuerpo de llaves o de flecha, delimitado por llaves equilibradas."""
    res = []
    sig = re.compile(r'(\[(?:Fact|Theory)[^\]]*\][^\n]*\n(?:\s*\[[^\]]*\]\s*\n)*)?\s*(?:(?:public|private|internal|protected|static|async|override|sealed)\s+)+[\w<>\[\],.?\s]+?\s+(\w+)\s*\([^;{]*?\)\s*(\{|=>)', re.S)
    for m in sig.finditer(code):
        name, kind = m.group(2), m.group(3)
        start = m.end()
        if kind == '{':
            depth, i = 1, start
            while i < len(code) and depth:
                depth += {'{': 1, '}': -1}.get(code[i], 0)
                i += 1
            body = code[start:i]
        else:
            j = code.find(';', start)
            body = code[start:j + 1]
        res.append((name, bool(m.group(1)), body))
    return res


CAUSES = {
    'C-1 tipo': r'Find<PushBackPilotDraftState>|DraftSession<PushBackPilotDraftState>|\bPushBackPilotDraftState\b|PushBackPilotStateComparer',
    'C-2 A/B': r'OutOfPilotDiagnostic|CompositeOutOfPilotReason',
    'C-3 conservar': r'\bTryKeepMine\b|KeepPilotEditsOn|TryKeepPilotEdit\b',
    'C-4 actualizar': r'PushBackPilotUpdateRun\.(?:Execute|Precheck)\b|NonPilotContentReason|TryUpdatePilot\b',
    'C-6 adaptador': r'PushBackPilotFieldAdapter',
    'C-8 campo': r'\bSetPendingText\b|\bCommitField\b|PilotBoundaryKind\.(?:Update|TabChange|RackChange)',
    'C-9 lectura fuera del kit': r'\bFromPilot\s*\(|PrepareKit\.Reread\s*\(',
}
CRX = {k: re.compile(v) for k, v in CAUSES.items()}
NINE = ['WorkspaceLifecycleHostTests', 'WorkspacePilotPrepareTests', 'WorkspaceEditIntentHostTests', 'WorkspacePilotEditHostTests',
        'WorkspaceCommandAdvisorTests', 'WorkspacePilotUpdateRequestTests', 'WorkspacePilotUpdateHostTests',
        'WorkspacePilotReconciliationHostTests', 'WorkspaceAssociativeBookkeepingTests']


def signals(code):
    ms = members(strip_comments(code))
    helpers = {n: b for n, t, b in ms if not t}
    rows = []
    for n, t, b in ms:
        if not t:
            continue
        called = [h for h in helpers if h != n and re.search(r'\b' + re.escape(h) + r'\s*[(<]', b)]
        text = b + '\n'.join(helpers[h] for h in called)
        rows.append((n, {k: bool(r.search(text)) for k, r in CRX.items()}, {k: bool(r.search(b)) for k, r in CRX.items()}))
    return rows


out.append('## A. Senales por metodo (cuerpo + ayudantes del mismo archivo, un nivel)')
tot = {'metodos': 0}
agg = {}
for cls in NINE:
    rows = signals(show('tests/RackCad.Tests/Workspace/%s.cs' % cls))
    n = len(rows)
    tot['metodos'] += n
    anyc = sum(1 for _, s, _ in rows if any(s.values()))
    anyc_no8 = sum(1 for _, s, _ in rows if any(v for k, v in s.items() if k != 'C-8 campo'))
    body_only = sum(1 for _, _, s in rows if any(v for k, v in s.items() if k != 'C-8 campo'))
    per = {k: sum(1 for _, s, _ in rows if s[k]) for k in CAUSES}
    out.append('- %s: %d metodos; con alguna senal %d; sin C-8 %d; solo por el cuerpo (sin C-8) %d; %s' % (
        cls, n, anyc, anyc_no8, body_only, ', '.join('%s=%d' % (k.split()[0], v) for k, v in per.items())))
    for key, val in (('con', anyc), ('sin8', anyc_no8), ('cuerpo', body_only)):
        agg[key] = agg.get(key, 0) + val
out.append('- TOTAL: %d metodos; con alguna senal %d; sin C-8 %d; solo por el cuerpo (sin C-8) %d' % (tot['metodos'], agg['con'], agg['sin8'], agg['cuerpo']))

# ------------------------------------------------------------------ B
out.append('')
out.append('## B. Las pruebas del codigo que se retira (Q1 «Retirar»)')
RETIRED = re.compile(r'PushBackPilotStateComparer|PushBackPilotFieldAdapter|KeepPilotEditsOn|ComposeForWrite|PushBackPilotUpdatePlanner\.Prepare\s*\(|PushBackPilotUpdateRun\.(?:Execute|Precheck)\s*\(')
files = [p for p in git('ls-tree', '-r', '--name-only', rev, 'tests').splitlines() if p.endswith('.cs')]
host_set = set(NINE)
total_b, outside_b = 0, 0
for path in files:
    code = show(path)
    if not RETIRED.search(code):
        continue
    ms = members(strip_comments(code))
    helpers = {n: b for n, t, b in ms if not t}
    hit = 0
    for n, t, b in ms:
        if not t:
            continue
        called = [h for h in helpers if h != n and re.search(r'\b' + re.escape(h) + r'\s*[(<]', b)]
        if RETIRED.search(b + '\n'.join(helpers[h] for h in called)):
            hit += 1
    if hit:
        cls = path.rsplit('/', 1)[-1][:-3]
        total_b += hit
        if cls not in host_set:
            outside_b += hit
        out.append('- %s: %d%s' % (path, hit, '' if cls not in host_set else ' (una de las 9)'))
out.append('- TOTAL: %d metodos; fuera de las 9 clases del anfitrion: %d' % (total_b, outside_b))

# ------------------------------------------------------------------ C
out.append('')
out.append('## C. Las pruebas que nombran NumericField o NumericFieldValidation')
res = git('grep', '-n', '-E', '-e', r'\bNumericField(Validation)?\b', rev, '--', 'tests', ok=(0, 1)).splitlines()
byfile = {}
for l in res:
    f = l[len(rev) + 1:].split(':', 1)[0]
    byfile[f] = byfile.get(f, 0) + 1
for f, n in sorted(byfile.items()):
    out.append('- %s: %d lineas' % (f, n))
res2 = git('grep', '-n', '-E', '-e', r'NumericFieldValidation\.cs|Controls/NumericField|"NumericField', rev, '--', 'tests', ok=(0, 1)).splitlines()
out.append('- las que leen el archivo fuente de NumericField o NumericFieldValidation:')
for l in res2[:20]:
    out.append('    ' + l[len(rev) + 1:][:200])

# ------------------------------------------------------------------ D
out.append('')
out.append('## D. El censo de CT-PBT1B-15 y las carpetas candidatas')
plugin = [p for p in git('ls-tree', '-r', '--name-only', rev, 'src/RackCad.Plugin').splitlines() if p.endswith('.cs')]
withpb = []
for p in plugin:
    code = re.sub(r'"(?:[^"\\]|\\.)*"', '""', strip_comments(show(p)))
    if 'PushBack' in code:
        withpb.append(p)
out.append('- archivos del Plugin cuyo codigo contiene PushBack: %d' % len(withpb))
for p in withpb:
    out.append('    ' + p)
out.append('- WorkspaceController.cs contiene PushBack: %s' % ('src/RackCad.Plugin/Workspace/WorkspaceController.cs' in withpb))
for folder in ['src/RackCad.Plugin/Systems/PushBack', 'src/RackCad.Plugin/Views', 'src/RackCad.Plugin/Systems/Shared', 'src/RackCad.Plugin/Drawing']:
    n = len([p for p in plugin if p.startswith(folder + '/')])
    out.append('- %s: %d archivos .cs' % (folder, n))
cens = git('grep', '-n', '-E', '-e', r'"src/RackCad\.Plugin/(Systems/PushBack|Views|Systems/Shared)/?"|"Systems", "PushBack"|PluginDirectory|SourceFiles\("src/RackCad\.Plugin"\)', rev, '--', 'tests', ok=(0, 1)).splitlines()
out.append('- guardas que barren el Plugin o esas carpetas (%d lineas):' % len(cens))
for l in cens[:40]:
    out.append('    ' + l[len(rev) + 1:][:200])


# ------------------------------------------------------------------ E
def excerpt(tag, path, a, b):
    out.append('')
    out.append('### %s — %s:%d-%d' % (tag, path, a, b))
    try:
        lines = show(path).split('\n')
    except SystemExit:
        out.append('    (no existe)')
        return
    for i in range(a - 1, min(b, len(lines))):
        if lines[i].strip():
            out.append('%5d %s' % (i + 1, lines[i].rstrip()[:200]))


out.append('')
out.append('## E. Extractos')
out.append(git('diff', '--stat', '7dfc7c21', rev, '--', 'src', 'tests').strip())
excerpt('B07-a la guarda de tipos de la UI', 'tests/RackCad.Tests/Workspace/WorkspaceHostSourceGuardTests.cs', 392, 424)
excerpt('B07-b los using fijados', 'tests/RackCad.Tests/Workspace/WorkspaceNavigationHostTests.cs', 975, 990)
excerpt('B07-c NumericFieldValidation', 'src/RackCad.UI/Controls/NumericFieldValidation.cs', 1, 60)
excerpt('M25 el estado editable: las dos referencias', 'src/RackCad.Application/Systems/PushBack/PushBackEditableState.cs', 36, 100)
excerpt('M27 la CI de Core', '.github/workflows/ci.yml', 30, 50)
dispo = show('docs/automation/evidence/I-64-pilot/F4B-DRAFTS/f4b-dispositions.md').split('\n')
idx = [i for i, l in enumerate(dispo) if l.startswith('## F4B-E48')]
if idx:
    out.append('')
    out.append('### M27 F4B-E48 — f4b-dispositions.md:%d-' % (idx[0] + 1))
    for i in range(idx[0], min(idx[0] + 60, len(dispo))):
        if dispo[i].strip() and ('OWNER' in dispo[i] or i < idx[0] + 6):
            out.append('%5d %s' % (i + 1, dispo[i][:220]))

with open(out_path, 'w', encoding='utf-8', newline='\n') as f:
    f.write('# measure_pbt4_v4: salida (rev %s)\n' % rev)
    f.write('\n'.join(out) + '\n')
print('lineas', len(out))
