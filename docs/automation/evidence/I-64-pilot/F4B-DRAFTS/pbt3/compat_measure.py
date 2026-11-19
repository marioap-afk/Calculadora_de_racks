# Redaccion para el repo publico: las rutas locales se sustituyen por marcadores (<scratchpad>, <worktree-de-I-64>, <checkout-principal>, <datos-locales-del-usuario>). El resto no cambia.
"""compat_measure.py - mediciones de solo lectura para compat-pbt3-vs-pbt2.md.
Lee solo por objetos git desde el checkout principal (git -C <checkout> show/ls-tree) y archivos de hs65/. No escribe nada.
Uso: python -I compat_measure.py
"""
import hashlib, json, os, re, subprocess, sys

MAIN = r'<checkout-principal>'
HERE = os.path.dirname(os.path.abspath(__file__))
HS = os.path.dirname(HERE)
PB1B_CLOSE = '95373af6116cf120bd0a8ba490833524e33c47da'
REVS = ['d8ccbcb8', '5a8366c1', '5ddeb3b4']
W = 'src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs'
APP = 'src/RackCad.Application/Systems/PushBack/'
DRAFT = APP + 'Draft/'


def git(*a):
    r = subprocess.run(['git', '-C', MAIN] + list(a), capture_output=True)
    assert r.returncode == 0, (a, r.stderr[-300:])
    return r.stdout


def files(rev):
    return [p for p in git('ls-tree', '-r', '-z', '--name-only', rev).decode('utf-8').split('\0') if p]


def cover(tracked, allowed, prefix):
    inside = [a for a in allowed if a.startswith(prefix)]
    if not inside:
        return [prefix]
    out = []
    names = sorted({p[len(prefix):].split('/')[0] + ('/' if '/' in p[len(prefix):] else '') for p in tracked if p.startswith(prefix)})
    for n in names:
        if n.endswith('/'):
            out += cover(tracked, allowed, prefix + n)
        elif prefix + n not in allowed:
            out.append(prefix + n)
    return out


# 1. sha256 de los paquetes
for name in ('I-64-f4b-pbt3-gate-contract-package-v4.md', 'I-64-f4b-pbt3-gate-contract-package-v41.md'):
    raw = open(os.path.join(HERE, name), 'rb').read()
    print('sha256', name, hashlib.sha256(raw).hexdigest(), 'bytes', len(raw), 'CRLF' if b'\r\n' in raw else 'LF')

# 2. el ForbiddenWriteScope de src/ de las tres subtareas (cover de [PKC-C]) en cada revision, y con el archivo del modo de PB-T2b
A_ALLOWED = [DRAFT + n for n in ('PushBackFullDraftState.cs', 'PushBackFullDraftStateComparer.cs', 'PushBackEditableStateSeat.cs',
                                 'PushBackFullDraftMaterializer.cs', 'PushBackFullDraftReader.cs', 'PushBackFullComposition.cs',
                                 'PushBackViewDisposition.cs', 'PushBackConflictClassifier.cs')]
B_ALLOWED = [APP + 'Pilot/PushBackPilotUpdatePlan.cs', APP + 'Pilot/PushBackPilotUpdateRun.cs', APP + 'Pilot/IPushBackPilotViewEraser.cs']
C_ALLOWED = ['src/RackCad.Plugin/Views/RackPilotUpdateWriter.cs', 'src/RackCad.Plugin/Views/RackViewBlockEraser.cs']
MODE = 'src/RackCad.UI/Systems/PushBack/PushBackEditorMode.cs'
for rev in REVS:
    t = [p for p in files(rev) if p.startswith('src/')]
    for label, extra in (('', []), (' +modo PB-T2b', [MODE])):
        tt = t + extra
        counts = []
        for name, allowed in (('A', A_ALLOWED), ('B', B_ALLOWED), ('C', C_ALLOWED)):
            counts.append('%s=%d' % (name, len(cover(tt, allowed, 'src/'))))
        print('cover(src/)', rev + label, ' '.join(counts))
    print('  archivos directos de', APP, 'en', rev, ':', sorted(p[len(APP):] for p in t if p.startswith(APP) and '/' not in p[len(APP):] and 'Draft' in p))

# WR real (el nombre del escritor del Plugin)
wr = [p for p in files('5ddeb3b4') if p.startswith('src/RackCad.Plugin/') and 'PilotUpdate' in p]
print('archivos del Plugin con PilotUpdate:', wr)

# 3. los bloques de W del generador (lineas del cierre de PB-T1b) y su desplazamiento en cada revision
W_BLOCKS = [(145, 146), (190, 190), (311, 311), (659, 704), (662, 670)]
w1b = git('show', PB1B_CLOSE + ':' + W).decode('utf-8').replace('\r\n', '\n').split('\n')
for rev in REVS:
    wr_ = git('show', rev + ':' + W).decode('utf-8').replace('\r\n', '\n').split('\n')
    out = []
    for a, b in W_BLOCKS:
        blk = w1b[a - 1:b]
        hits = [k for k in range(len(wr_) - len(blk) + 1) if wr_[k:k + len(blk)] == blk]
        out.append('W:%d-%d -> %s' % (a, b, ','.join('%d-%d' % (h + 1, h + len(blk)) for h in hits) or 'NO'))
    seam = [i + 1 for i, l in enumerate(wr_) if 'EditableStateForTest =>' in l]
    lfm = [i + 1 for i, l in enumerate(wr_) if 'private void LoadFromModel(' in l]
    print('W', rev, len(wr_), 'lineas |', ' | '.join(out), '| costura', seam, '| LoadFromModel', lfm)

# 4. el final de LoadFromModel (W:440 en d8ccbcb8, "termina con una pasada")
for rev in ('d8ccbcb8', '5ddeb3b4'):
    wr_ = git('show', rev + ':' + W).decode('utf-8').replace('\r\n', '\n').split('\n')
    s = [i for i, l in enumerate(wr_) if 'private void LoadFromModel(' in l][0]
    depth, opened, e = 0, False, None
    for i in range(s, len(wr_)):
        c = re.sub(r'"(?:\\.|[^"\\])*"', '""', wr_[i]).split('//')[0]
        depth += c.count('{') - c.count('}')
        opened = opened or '{' in c
        if opened and depth == 0:
            e = i
            break
    print('LoadFromModel', rev, 'W:%d-%d' % (s + 1, e + 1), 'ultima sentencia:', [l.strip() for l in wr_[s:e] if l.strip()][-1])

# 5. las clases de prueba de PB-T2 (alcances de los contratos en seco) frente al ForbiddenWriteScope de tests/ del contrato de PB-T1c
c2b = json.load(open(os.path.join(HS, 'pbt2', 'contract-dry-pbt2b.json'), encoding='utf-8'))
c2c = json.load(open(os.path.join(HS, 'pbt2', 'contract-dry-pbt2c.json'), encoding='utf-8'))
pt2a_tests = ['tests/RackCad.Tests/PushBackDraftStatePortTests.cs', 'tests/RackCad.Tests/PushBackDraftPortSharedReferenceGuardTests.cs']
pt2_tests = pt2a_tests + [p for p in c2b['AllowedWriteScope'] + c2c['AllowedWriteScope'] if p.startswith('tests/')]
print('clases de PB-T2 en tests/ (%d):' % len(pt2_tests), pt2_tests)
t1c = json.loads(git('show', '5ddeb3b4:docs/automation/evidence/I-64-pilot/F4B-PBT1C-LOSSLESS-STATE-WINDOW/coordinator/gate-contract.json').decode('utf-8'))
t1c_tests = [f for f in t1c['ForbiddenWriteScope'] + t1c['AllowedWriteScope'] if f.startswith('tests/')]
covered = [p for p in pt2_tests if any(p == f or (f.endswith('/') and p.startswith(f)) for f in t1c_tests)]
print('de ellas, cubiertas por las listas de tests/ que copia el generador de PB-T3a:', covered)
c2c_tests = [f for f in c2c['ForbiddenWriteScope'] if f.startswith('tests/')]
print('ForbiddenWriteScope de tests/ del seco de PB-T2c:', len(c2c_tests), 'entradas; Core', len([f for f in c2c_tests if f.startswith('tests/RackCad.Tests/')]),
      'UI', len([f for f in c2c_tests if f.startswith('tests/RackCad.UI.Tests/')]))
print('  de las clases de PB-T2, en el Forbidden de PB-T2c:', [p for p in pt2_tests if p in c2c_tests])
print('ForbiddenWriteScope de src/ del seco de PB-T2c:', len([f for f in c2c['ForbiddenWriteScope'] if f.startswith('src/')]))

# 6. los filtros de PB-T3a que podrian seleccionar clases de PB-T2 (prefijo de nombre completo, como count_tests_git.py)
filters = ['PushBackFullDraft', 'PushBackFullDraftWindowParityTests', 'PushBackDraftWriteGuardTests', 'Workspace', 'PushBackEditableState',
           'PushBackEditorOutputGate', 'PushBackSharedReferenceGuardTests', 'PushBackWindowSharedReferenceGuardTests', 'PushBackWindowOnStateSourceGuardTests',
           'PushBackWindowOnStateTests', 'PushBackClassicCharacterization', 'PushBackFullEditorCharacterization', 'PushBackStateNet', 'PushBackDeclaredCorrection',
           'PushBackFullWrite', 'PushBackPilotUpdateEquivalenceGuardTests', 'PushBackWritePathGuardTests', 'PushBackViewEraseGuardTests',
           'PushBackWritePathPluginGuardTests']
names = [os.path.basename(p)[:-3] for p in pt2_tests]
print('filtros de PB-T3 que seleccionan una clase de PB-T2:', [(f, n) for f in filters for n in names if n.startswith(f)])

# 7. los nombres nuevos de PB-T3 frente a los de PB-T2 (tipos y miembros)
pt2_names = ['PushBackDraftApplyStatus', 'PushBackDraftApplyResult', 'IPushBackDraftStatePort', 'PushBackDraftSessionPort', 'PushBackDraftEditorLaunch',
             'PushBackEditorMode', 'ApplyToDraft', 'LaunchFor']
pt3_names = ['PushBackFullDraftState', 'PushBackFullDraftStateComparer', 'PushBackEditableStateSeat', 'PushBackFullDraftMaterialization',
             'PushBackFullDraftMaterializer', 'PushBackFullIntentResult', 'PushBackFullDraftReader', 'PushBackFullWriteComposition', 'PushBackFullComposition',
             'PushBackViewDispositionKind', 'PushBackViewDispositionPlan', 'PushBackViewDispositions', 'PushBackChangeGroup', 'PushBackChangeClass',
             'PushBackChangeEntry', 'PushBackConflictClassification', 'PushBackConflictClassifier', 'PushBackFullPrepareInput', 'IPushBackPilotViewEraser',
             'RackViewBlockEraser', 'EraseUnit', 'EraseInTransaction']
print('nombres compartidos PB-T2/PB-T3:', sorted(set(pt2_names) & set(pt3_names)))
