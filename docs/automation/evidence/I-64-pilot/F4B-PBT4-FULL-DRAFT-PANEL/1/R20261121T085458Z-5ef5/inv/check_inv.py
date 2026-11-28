"""Comprobaciones de solo lectura de PB-T4b (INV-PBT4B-01, 02, 07 y 13; D2): alcance, Skip, copias y kit, censo de declaraciones, 'warning CS'.
Todo por objetos git (`git diff`, `git show`) y por los registros de build de la carpeta de la corrida. No escribe nada salvo su salida estandar (UTF-8).
Uso: python -I check_inv.py <repo> <BaseSha> <RedSha> <CurrentSha> [<RedRepairSha>]"""
import collections
import hashlib
import json
import os
import re
import subprocess
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
RUN = os.path.dirname(HERE)
GEN = os.path.join(RUN, 'gen')

ALLOWED_SRC = {
    'src/RackCad.Application/Workspace/WorkspaceHostCore.cs',
    'src/RackCad.Application/Workspace/WorkspaceEditIntent.cs',
    'src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs',
    'src/RackCad.UI/Workspace/WorkspacePanelView.xaml',
    'src/RackCad.UI/Controls/NumericFieldValidation.cs',
    'src/RackCad.Plugin/Views/RackPilotIntentAdapter.cs',
    'src/RackCad.Application/Formatting/NumericTextRule.cs',
    'src/RackCad.Application/Systems/PushBack/Draft/PushBackAnnotationScaleRule.cs',
    'src/RackCad.Application/Systems/PushBack/Draft/PushBackFullFieldAdapter.cs',
    'src/RackCad.Application/Systems/PushBack/Draft/PushBackKeepMinePresentation.cs',
}
GREEN_FILES = set(ALLOWED_SRC)
CT_CLASSES = {
    'tests/RackCad.Tests/PushBackFullPanelTests.cs', 'tests/RackCad.Tests/PushBackFullPanelGuardTests.cs',
    'tests/RackCad.Tests/PushBackFullPanelCensusGuardTests.cs', 'tests/RackCad.Tests/PushBackPanelDraftGuardTests.cs',
    'tests/RackCad.Tests/WorkspaceFullCopyGuardTests.cs', 'tests/RackCad.UI.Tests/PushBackFullPanelFieldTests.cs',
}
KIT = 'tests/RackCad.Tests/Workspace/WorkspaceFullReadKit.cs'
NO_TOUCH = ['src/RackCad.Domain', 'src/RackCad.Application/Systems/PushBack/Pilot/', 'src/RackCad.Plugin/RackPushBackCommands.cs',
            'src/RackCad.Plugin/RackCommandSupport.cs', 'src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs']


def git(repo, *args):
    p = subprocess.run(['git', '-C', repo] + list(args), capture_output=True)
    if p.returncode:
        raise SystemExit('git %s -> %d: %s' % (' '.join(args), p.returncode, p.stderr.decode('utf-8', 'replace')[:300]))
    return p.stdout.decode('utf-8', 'replace')


def show(repo, rev, path):
    p = subprocess.run(['git', '-C', repo, 'show', '%s:%s' % (rev, path)], capture_output=True)
    return None if p.returncode else p.stdout.decode('utf-8-sig').replace('\r\n', '\n')


def names(repo, a, b, *paths):
    return [l for l in git(repo, 'diff', '--name-only', a, b, *(('--',) + paths if paths else ())).split('\n') if l]


def main():
    sys.stdout.reconfigure(encoding='utf-8', errors='replace')
    repo, base, red, cur = sys.argv[1:5]
    rr = sys.argv[5] if len(sys.argv) > 5 else None
    bad = 0

    def check(label, ok, detail=''):
        nonlocal bad
        print('%s %s %s' % ('OK  ' if ok else 'MAL ', label, detail))
        bad += 0 if ok else 1

    print('== INV-PBT4B-01: el alcance (BaseSha..CurrentSha)')
    allowed_tests = set(CT_CLASSES) | {KIT}
    changed = names(repo, base, cur)
    skip_files = set()
    copies_json = json.load(open(os.path.join(repo, 'docs', 'automation', 'evidence', 'I-64-pilot', 'F4B-DRAFTS', 'pbt4', 'pbt4b-precondition6.json'), encoding='utf-8'))
    copy_files = set(copies_json['CopySha'])
    # los 21 archivos de los Skip: los de la lista de 478 (por el guion de la comprobacion)
    listing = open(os.path.join(GEN, 'pbt4b-substitution-list-478.tsv'), encoding='utf-8').read().split('\n')[1:]
    for l in listing:
        if l:
            c = l.split('\t')
            skip_files.add(c[1][len('(PB-T4a) '):] if c[1].startswith('(PB-T4a) ') else c[1])
    allowed = ALLOWED_SRC | allowed_tests | copy_files | skip_files
    check('59 rutas', len(allowed) == 59, str(len(allowed)))
    outside = [p for p in changed if p not in allowed]
    check('todo lo cambiado esta dentro de las 59', not outside, str(outside[:5]))
    check('se cambia cada una de las 59', set(changed) == allowed, 'faltan: %s' % sorted(allowed - set(changed))[:5])
    for prefix in NO_TOUCH:
        touched = [p for p in changed if p.startswith(prefix)]
        check('sin cambios en %s' % prefix, not touched, str(touched))
    print('   FilesChanged = %d' % len(changed))

    print('== INV-PBT4B-02: RED y GREEN')
    red_tree = names(repo, base, red)
    check('el RED toca src/ solo con el esqueleto (6 archivos)',
          sorted(p for p in red_tree if p.startswith('src/')) == sorted([
              'src/RackCad.Application/Formatting/NumericTextRule.cs',
              'src/RackCad.Application/Systems/PushBack/Draft/PushBackAnnotationScaleRule.cs',
              'src/RackCad.Application/Systems/PushBack/Draft/PushBackFullFieldAdapter.cs',
              'src/RackCad.Application/Systems/PushBack/Draft/PushBackKeepMinePresentation.cs',
              'src/RackCad.Application/Workspace/WorkspaceEditIntent.cs',
              'src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs']))
    after_red = names(repo, rr or red, cur)
    check('despues del RED solo cambia src/ (ninguna prueba)', not [p for p in after_red if not p.startswith('src/')], str([p for p in after_red if not p.startswith('src/')][:5]))
    check('despues del RED solo los 10 archivos del GREEN', set(after_red) <= GREEN_FILES, str(sorted(set(after_red) - GREEN_FILES)))
    must = {'src/RackCad.Application/Workspace/WorkspaceHostCore.cs', 'src/RackCad.UI/Workspace/WorkspacePanelView.xaml',
            'src/RackCad.UI/Controls/NumericFieldValidation.cs', 'src/RackCad.Plugin/Views/RackPilotIntentAdapter.cs'}
    check('el GREEN toca WHC, PV, NumericFieldValidation.cs y RIA', must <= set(after_red))
    if rr:
        rr_files = names(repo, red, rr)
        check('RedSha..RedRepairSha solo las seis clases de las CT', set(rr_files) <= CT_CLASSES, str(rr_files))

    print('== INV-PBT4B-07: los Skip (check_skips_pbt4b_v48.py)')
    for rev in [red] + ([rr] if rr else []) + [cur]:
        p = subprocess.run([sys.executable, '-I', os.path.join(GEN, 'check_skips_pbt4b_v48.py'), repo, base, rev], capture_output=True)
        out = p.stdout.decode('utf-8', 'replace').strip().split('\n')
        check('Skip sobre %s' % rev[:8], p.returncode == 0, out[-1])

    print('== las copias y el kit')
    for rev in [red] + ([rr] if rr else []) + [cur]:
        wrong = []
        for rel, sha in sorted(copies_json['CopySha'].items()):
            text = show(repo, rev, rel)
            if text is None or hashlib.sha256(text.encode('utf-8')).hexdigest() != sha:
                wrong.append(rel)
        kit = show(repo, rev, KIT)
        kit_ok = kit is not None and hashlib.sha256(kit.encode('utf-8')).hexdigest() == 'b4ed29f92838e5761faf1afaba1f33e58ba82a56541900f52db8c5ecb6239b9a'
        check('21 copias iguales a su CopySha y el kit (%s)' % rev[:8], not wrong and kit_ok, str(wrong[:3]))

    print('== esqueleto: sha256 de los seis archivos del RED')
    sk = {
        'src/RackCad.Application/Formatting/NumericTextRule.cs': 'e78a6429013d0793c3f5ae213e6c39e51d7d2ab94bab1c154431a18831435b35',
        'src/RackCad.Application/Systems/PushBack/Draft/PushBackAnnotationScaleRule.cs': 'd7706f5298bd860f8a7ee0638e3cf9282dee2639419233b9e40a579abe805169',
        'src/RackCad.Application/Systems/PushBack/Draft/PushBackFullFieldAdapter.cs': 'eff6f487ba78f517b9886253b97d66aa3c13e813b6d6b613aca399b5777fc569',
        'src/RackCad.Application/Systems/PushBack/Draft/PushBackKeepMinePresentation.cs': '6a2d8d327ffd7aba0fc5a30cd24349fdad49a23cae8a8054d5dff76be1dfc49d',
        'src/RackCad.Application/Workspace/WorkspaceEditIntent.cs': '151b3437fd27d691b91563bc6fe8bbea4c71db5c90df0aaa69f19ec7f604a5d5',
        'src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs': '3cd599f7c13c70b642e64ea1461fe09c882c656b4a4dabbd225c0c9817a1b9eb',
    }
    for rel, sha in sk.items():
        text = show(repo, red, rel)
        check(rel.split('/')[-1], text is not None and hashlib.sha256(text.encode('utf-8')).hexdigest() == sha)

    print('== INV-PBT4B-13 y PBT4B-C13: el censo de declaraciones')
    p = subprocess.run([sys.executable, '-I', os.path.join(HERE, 'census.py'), repo, base, cur], capture_output=True)
    out = p.stdout.decode('utf-8', 'replace')
    print(out.rstrip())
    check('el censo es la lista cerrada', p.returncode == 0)

    print('== D2 (PBT4B-C17): lineas anadidas en src/ que nombran el rack resuelto o el informe de modulos')
    diff = git(repo, 'diff', '-U0', base, cur, '--', 'src')
    hits = [l for l in diff.split('\n') if l.startswith('+') and not l.startswith('+++')
            and re.search(r'WorkingBaseline|LastModuleReconciliation|ModuleReport|PushBackSystem\b', l)]
    check('ninguna linea anadida nombra WorkingBaseline, LastModuleReconciliation ni PushBackSystem', not hits, str(hits[:3]))

    print('== "warning CS<n>" en todos los registros de build de la corrida')
    total = 0
    for dirpath, _, files in os.walk(RUN):
        if os.sep + 'gen' in dirpath or os.sep + 'inv' in dirpath:
            continue
        for f in files:
            if f.startswith('build-') and f.endswith('.log'):
                text = open(os.path.join(dirpath, f), encoding='utf-8', errors='replace').read()
                n = len(re.findall(r'warning CS\d+', text))
                total += n
                print('   %s: %d' % (os.path.relpath(os.path.join(dirpath, f), RUN).replace(os.sep, '/'), n))
    check('0 lineas "warning CS"', total == 0, str(total))
    print('RESULTADO: %s' % ('TODO OK' if bad == 0 else '%d MAL' % bad))
    sys.exit(0 if bad == 0 else 1)


if __name__ == '__main__':
    main()
