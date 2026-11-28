"""Comprobaciones de INV-PBT4D-01..14 (solo lectura). Uso (cwd = worktree): python -I check_inv.py <BaseSha> <RedSha> <TargetRef>
TargetRef: HEAD, o WORKTREE (el arbol de trabajo, antes del commit GREEN)."""
import glob
import hashlib
import os
import re
import subprocess
import sys

REL = 'artifacts/orchestration/I-64/F4B-PBT4-RETIRE-PILOT/1/R20261122T173325Z-c9e1'
BASE, RED, TARGET = sys.argv[1:4]
PILOT = 'src/RackCad.Application/Systems/PushBack/Pilot/'
SIX = [PILOT + n for n in ('PushBackPilotMaterializer.cs', 'PushBackPilotUpdatePlan.cs', 'PushBackPilotUpdateRun.cs',
                           'PushBackPilotDraftState.cs', 'PushBackPilotStateComparer.cs', 'PushBackPilotFieldAdapter.cs')]
GREEN_SHA = {
    PILOT + 'PushBackPilotDraftState.cs': 'a545dbffe26c25e147a0821470b00e7ea300bb77b8b4b1451cc2b7ed098d9f97',
    PILOT + 'PushBackPilotMaterializer.cs': '7b7f2fb46a7e036789eecc7382093a7b2a5f8bb023f3c4bf765c805aa67ee9cb',
    PILOT + 'PushBackPilotUpdatePlan.cs': 'f70febaaea02a6f84c8f6502f5b540e72fdb5197bbd02c4493800fe43938078f',
    PILOT + 'PushBackPilotUpdateRun.cs': '5c936519abbaf49f0b68fdc6484d8ea877a0370412af505f1bf3e13aa6d33b7b',
}
GREEN_DELETED = [PILOT + 'PushBackPilotStateComparer.cs', PILOT + 'PushBackPilotFieldAdapter.cs']
fails = []


def git(*a):
    return subprocess.run(['git'] + list(a), capture_output=True, check=True).stdout.decode('utf-8', 'replace')


def lines(s):
    return [x for x in s.split('\n') if x]


def text_at(ref, path):
    if ref == 'WORKTREE':
        if not os.path.exists(path):
            return None
        return open(path, 'rb').read().decode('utf-8-sig').replace('\r\n', '\n')
    r = subprocess.run(['git', 'show', ref + ':' + path], capture_output=True)
    return None if r.returncode else r.stdout.decode('utf-8-sig').replace('\r\n', '\n')


def check(name, ok, detail=''):
    print('%s %s %s' % ('OK  ' if ok else 'FALLA', name, detail))
    if not ok:
        fails.append(name)


sys.stdout.reconfigure(encoding='utf-8', newline='\n')
tgt = [] if TARGET == 'WORKTREE' else [TARGET]


def diffnames(a, *paths):
    return lines(git('diff', '--name-only', a, *tgt, '--', *paths))


# INV-01
d_red = diffnames(RED, 'src', 'tests')
print('git diff --name-only RedSha -- src tests:'); [print('  ' + x) for x in d_red]
check('INV-PBT4D-01 desde RedSha solo los seis de Pilot/, nada de tests/', sorted(d_red) == sorted(SIX))
check('INV-PBT4D-01 Domain/UI/Plugin vacio', diffnames(BASE, 'src/RackCad.Domain', 'src/RackCad.UI', 'src/RackCad.Plugin') == [])
check('INV-PBT4D-01 los cinco/seis archivos no cambiados del contrato', diffnames(BASE, 'src/RackCad.Domain', 'src/RackCad.Plugin/RackPushBackCommands.cs', 'src/RackCad.Plugin/RackCommandSupport.cs',
      'src/RackCad.Plugin/Views/RackPilotIntentAdapter.cs', 'src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs') == [])
d_base = diffnames(BASE, 'src', 'tests')
src_changed = sorted(x for x in d_base if x.startswith('src/'))
check('INV-PBT4D-01 en src/ solo los seis de Pilot/', src_changed == sorted(SIX), str(len(src_changed)))
status = lines(git('diff', '--name-status', BASE, *tgt, '--', 'src'))
check('INV-PBT4D-01 4 M y 2 D', sorted(s.split('\t')[0] for s in status) == ['D', 'D', 'M', 'M', 'M', 'M'], str(status))
tests_changed = [x for x in d_base if x.startswith('tests/')]
print('tests/ cambiados entre BaseSha y destino: %d' % len(tests_changed))

# RED literal (inputs/red-files.txt)
red_files = {}
for l in open(REL + '/inputs/red-files.txt', encoding='utf-8').read().split('\n'):
    if l.strip():
        a, b = l.split(' ', 1)
        red_files[b.strip()] = a
bad = 0
for p, h in red_files.items():
    t = text_at(TARGET, p)
    if h == 'BORRADO':
        if t is not None:
            bad += 1
            print('  existe y debia estar borrado: ' + p)
    else:
        if t is None or hashlib.sha256(t.encode('utf-8')).hexdigest() != h:
            bad += 1
            print('  no coincide: ' + p)
check('RED literal: %d archivos de tests/ con el sha256 de red-files.txt y los borrados ausentes (en el destino)' % len(red_files), bad == 0)
check('tests/ del destino == tests/ de RedSha (sin cambio desde el RED)', lines(git('diff', '--name-only', RED, *tgt, '--', 'tests')) == [])
check('tests/ que cambian desde BaseSha son exactamente los 31 del RED', sorted(tests_changed) == sorted(red_files), str(len(tests_changed)))

# GREEN literal
bad = 0
for p, h in GREEN_SHA.items():
    t = text_at(TARGET, p)
    if t is None or hashlib.sha256(t.encode('utf-8')).hexdigest() != h:
        bad += 1
        print('  GREEN no coincide: ' + p)
for p in GREEN_DELETED:
    if text_at(TARGET, p) is not None:
        bad += 1
        print('  debia estar borrado: ' + p)
check('GREEN literal: 4 archivos con su sha256 y 2 ausentes', bad == 0)

# allowErase
ga = subprocess.run(['git', 'grep', '-n', 'allowErase', TARGET if TARGET != 'WORKTREE' else '--', '--', 'src'] if TARGET != 'WORKTREE' else ['git', 'grep', '-n', 'allowErase', '--', 'src'], capture_output=True)
check('INV-PBT4D-13 allowErase no aparece en src/', ga.returncode == 1 and not ga.stdout)

# warning CS
tot = 0
for f in sorted(glob.glob(REL + '/**/*.log', recursive=True)) + sorted(glob.glob(REL + '/**/*.txt', recursive=True)):
    if os.path.basename(f).startswith('build-'):
        t = open(f, encoding='utf-8', errors='replace').read()
        n = len(re.findall(r'warning CS\d+', t))
        tot += n
        print('  warning CS %d  %s' % (n, f))
check('0 lineas "warning CS" en todos los registros de build', tot == 0)

# D2: lineas anadidas en src
patch = git('diff', '-U0', BASE, *tgt, '--', 'src')
added = [l[1:] for l in patch.split('\n') if l.startswith('+') and not l.startswith('+++')]
code_added = [l for l in added if l.strip() and not l.strip().startswith('//') and not l.strip().startswith('///')]
print('lineas anadidas en src/: %d (de ellas no comentario: %d)' % (len(added), len(code_added)))
for l in code_added:
    print('  +' + l)
mut = [l for l in code_added if re.search(r'PushBackSystem|RackModuleReconciliationResult', l)]
check('INV-PBT4D-14/C17 ninguna linea anadida nombra PushBackSystem ni RackModuleReconciliationResult', not mut, str(mut))
print('RESULTADO: %s' % ('TODO OK' if not fails else 'FALLAS: ' + '; '.join(fails)))
