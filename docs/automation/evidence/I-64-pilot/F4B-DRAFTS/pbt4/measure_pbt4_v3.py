"""PB-T4, paquete v3: mediciones de solo lectura sobre una revision (por defecto 7dfc7c21).

Uso: python -I measure_pbt4_v3.py <checkout principal> <rev> <salida.txt>
Solo llama a `git -C <checkout> show|grep|ls-tree` (objetos git); no lee el disco del checkout ni ningun worktree, y escribe solo <salida.txt>.
Partes:
  A. Las clases que usan el lector doble o el kit del piloto: metodos por clase, y una estimacion por texto de los que cumplen C-1 a C-7.
  B. Las hojas de los fixtures (para la Q2): rutas de hoja del diseno, con los indices normalizados.
  C. Extractos de las guardas y del codigo que citan los hallazgos I-12 a I-16, B-05 y M-16.
"""
import json
import re
import subprocess
import sys

repo, rev, out_path = sys.argv[1:4]
out = []


def git(*args, ok=(0,)):
    p = subprocess.run(['git', '-C', repo] + list(args), capture_output=True)
    if p.returncode not in ok:
        raise SystemExit('git %s -> %d: %s' % (' '.join(args), p.returncode, p.stderr.decode('utf-8', 'replace')))
    return p.stdout.decode('utf-8', 'replace').replace('\r\n', '\n')


def show(path):
    return git('show', '%s:%s' % (rev, path))


def excerpt(tag, path, a, b):
    out.append('')
    out.append('### %s — %s:%d-%d' % (tag, path, a, b))
    lines = show(path).split('\n')
    for i in range(a - 1, min(b, len(lines))):
        if lines[i].strip():
            out.append('%5d %s' % (i + 1, lines[i].rstrip()[:200]))


TW = 'tests/RackCad.Tests/Workspace/'

# ------------------------------------------------------------------ A
KIT_USE = re.compile(r'\bnew\s+EditIntentFakeReader\b|\bEditIntentFakeReader\b|\bPilotEditAuditedReader\b|\bEditIntentKit\.|\bFromPilot\s*\(|\bnew\s+WorkspaceHostRig\b')
# Las firmas por texto de las causas (una estimacion, no un recuento de fallos)
CAUSES = [
    ('C-1', re.compile(r'Find<PushBackPilotDraftState>|PushBackPilotDraftState\b|PushBackPilotStateComparer')),
    ('C-2', re.compile(r'OutOfPilotDiagnostic|CompositeOutOfPilotReason|pb-20-ab|pb-2\d-')),
    ('C-3', re.compile(r'TryKeepMine|KeepPilotEditsOn|RequestKeepMine|ConfirmChoice')),
    ('C-4', re.compile(r'PushBackPilotUpdateRun\.(Execute|Precheck)\b|NonPilotContentReason')),
    ('C-6', re.compile(r'PushBackPilotFieldAdapter')),
]
ATTR = re.compile(r'^\s*\[(Fact|Theory)\b', re.M)
files = [p for p in git('ls-tree', '-r', '--name-only', rev, 'tests').splitlines() if p.endswith('.cs')]
out.append('## A. Las clases del lector doble y del kit del piloto (C-0)')
total = 0
rows = []
for path in files:
    text = show(path)
    if not KIT_USE.search(text):
        continue
    starts = [m.start() for m in ATTR.finditer(text)]
    if not starts:
        out.append('- (sin pruebas) %s' % path)
        continue
    bodies = [text[s:(starts[i + 1] if i + 1 < len(starts) else len(text))] for i, s in enumerate(starts)]
    counts = {c: sum(1 for b in bodies if r.search(b)) for c, r in CAUSES}
    any_cause = sum(1 for b in bodies if any(r.search(b) for _, r in CAUSES))
    rows.append((path, len(bodies), counts, any_cause))
    total += len(bodies)
for path, n, counts, any_cause in sorted(rows, key=lambda r: -r[1]):
    out.append('- %s: %d metodos; por texto: %s; con alguna firma C-1..C-6: %d' % (
        path.rsplit('/', 1)[-1], n, ', '.join('%s=%d' % (k, v) for k, v in counts.items()), any_cause))
out.append('- TOTAL: %d metodos en %d archivos; con alguna firma: %d' % (total, len(rows), sum(r[3] for r in rows)))

# ------------------------------------------------------------------ B
out.append('')
out.append('## B. Las hojas del diseno de los fixtures (Q2)')


def leaves(node, path, acc):
    if isinstance(node, dict):
        for k, v in node.items():
            leaves(v, path + '.' + k, acc)
    elif isinstance(node, list):
        if not node:
            acc.add(path + '[]')
        for v in node:
            leaves(v, path + '[i]', acc)
    else:
        acc.add(path)


for fx in ['pb-01-base', 'pb-20-ab', 'pb-07-combinado']:
    p = 'tests/RackCad.Tests/Fixtures/I64/%s.json' % fx
    try:
        doc = json.loads(show(p))
    except SystemExit:
        out.append('- %s: no existe' % fx)
        continue
    acc = set()
    leaves(doc, '$', acc)
    firsts = sorted({re.sub(r'^\$\.([^.\[]+).*', r'\1', a) for a in acc})
    out.append('- %s: %d rutas de hoja distintas (indices normalizados); primer nivel: %s' % (fx, len(acc), ', '.join(firsts)))
    # el diseno puede venir como texto JSON dentro del sobre
    for key in ('Design', 'Payload', 'Project'):
        if isinstance(doc, dict) and isinstance(doc.get(key), str):
            try:
                inner = json.loads(doc[key])
                acc2 = set()
                leaves(inner, '$', acc2)
                out.append('    %s (texto JSON anidado): %d rutas de hoja' % (key, len(acc2)))
                inner_firsts = sorted({re.sub(r'^\$\.([^.\[]+).*', r'\1', a) for a in acc2})
                out.append('    primer nivel de %s: %s' % (key, ', '.join(inner_firsts)))
            except ValueError:
                pass

# ------------------------------------------------------------------ C
EX = [
    ('B05-a el adaptador del panel en el controlador', 'src/RackCad.Plugin/Workspace/WorkspaceController.cs', 40, 60),
    ('B05-b PilotEditAdapter', 'src/RackCad.Plugin/Workspace/WorkspaceController.cs', 280, 336),
    ('B05-c ControllerViolations', TW + 'WorkspacePilotEditGuardTests.cs', 676, 700),
    ('B05-d la compuerta de avisos', 'src/RackCad.Application/Workspace/WorkspacePilotSupport.cs', 150, 200),
    ('I12-a EIG comprobar ahora', TW + 'WorkspaceEditIntentGuardTests.cs', 555, 580),
    ('I12-b EIG reconciliacion', TW + 'WorkspaceEditIntentGuardTests.cs', 750, 775),
    ('I12-c recarga', TW + 'WorkspaceLifecycleGuardTests.cs', 636, 660),
    ('I13-a resultado de la corrida', 'src/RackCad.Application/Workspace/Drafts/DraftApplyOutcome.cs', 40, 75),
    ('I13-b el origen de POST en la corrida', 'src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdateRun.cs', 595, 650),
    ('I13-c EIG UpdateNeverReads', TW + 'WorkspaceEditIntentGuardTests.cs', 1116, 1130),
    ('I13-d el estado del piloto', 'src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotDraftState.cs', 1, 40),
    ('I14-a EIG Keep', TW + 'WorkspaceEditIntentGuardTests.cs', 660, 725),
    ('I14-b los once estados', TW + 'WorkspacePilotPrepareTests.cs', 1340, 1356),
    ('I14-c el XAML de la escala', 'tests/RackCad.UI.Tests/Workspace/WorkspaceDraftPilotValidationParityTests.cs', 76, 99),
    ('I14-d nueve archivos (EIG)', TW + 'WorkspaceEditIntentGuardTests.cs', 1005, 1030),
    ('I14-e nueve archivos (navegacion)', TW + 'WorkspaceNavigationHostTests.cs', 708, 726),
    ('I14-f los estados de la intencion', 'src/RackCad.Application/Workspace/WorkspaceEditIntent.cs', 120, 200),
    ('I15 RequestUpdate', 'src/RackCad.Application/Workspace/WorkspaceHostCore.cs', 1376, 1395),
    ('I16 D2: las condiciones', 'docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/owner-d1-d2-decisions.md', 89, 160),
    ('M16 la clave del origen en RIA', 'src/RackCad.Plugin/Views/RackPilotIntentAdapter.cs', 100, 110),
    ('A13-a la costura de escenificar', 'tests/RackCad.UI.Tests/PushBackHeaderAuthorityWindowTests.cs', 95, 110),
    ('A13-b el resultado de la intencion', 'src/RackCad.Application/Workspace/WorkspaceEditIntent.cs', 200, 300),
    ('Q2 la tabla de propiedad', 'src/RackCad.Application/Systems/PushBack/PushBackEditableStateOwnership.cs', 128, 180),
]
for tag, path, a, b in EX:
    try:
        excerpt(tag, path, a, b)
    except SystemExit as e:
        out.append('')
        out.append('### %s — %s: %s' % (tag, path, str(e)[:120]))

with open(out_path, 'w', encoding='utf-8', newline='\n') as f:
    f.write('# measure_pbt4_v3: salida (rev %s)\n' % rev)
    f.write('\n'.join(out) + '\n')
print('lineas', len(out))
