"""Autoprueba de apply_red_pbt4b.py y apply_green_pbt4b.py (con apply_green_pbt4a.py antes, porque 5aaacce3 no trae el GREEN de PB-T4a), sobre
una copia mínima en _selftest_b/ (solo los archivos que tocan, sacados por objetos git de 5aaacce3). La carpeta se borra al acabar.
Uso: python -I selftest_pbt4b_scripts.py <checkout>"""
import os, shutil, subprocess, sys
sys.stdout.reconfigure(encoding='utf-8', errors='replace')
HERE = os.path.dirname(os.path.abspath(__file__))
repo = sys.argv[1]
root = os.path.join(HERE, '_selftest_b')
if os.path.exists(root):
    shutil.rmtree(root)
files = ['src/RackCad.Application/Workspace/WorkspaceHostCore.cs', 'src/RackCad.Application/Workspace/WorkspaceEditIntent.cs',
         'src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs', 'src/RackCad.Plugin/Views/RackPilotIntentAdapter.cs',
         'src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs', 'src/RackCad.UI/Workspace/WorkspacePanelView.xaml',
         'src/RackCad.UI/Controls/NumericFieldValidation.cs']
for l in open(os.path.join(HERE, 'pbt4b-substitution-list-475.tsv'), encoding='utf-8').read().split('\n')[1:]:
    if l and not l.split('\t')[1].startswith('(PB-T4a)'):
        files.append(l.split('\t')[1])
for rel in sorted(set(files)):
    data = subprocess.run(['git', '-C', repo, 'show', '5aaacce3:' + rel], capture_output=True).stdout
    p = os.path.join(root, *rel.split('/'))
    os.makedirs(os.path.dirname(p), exist_ok=True)
    open(p, 'wb').write(data)
py = sys.executable


def run(*a):
    r = subprocess.run([py, '-I'] + list(a), capture_output=True)
    out = r.stdout.decode('utf-8', 'replace') + r.stderr.decode('utf-8', 'replace')
    return r.returncode, out


try:
    rc, out = run(os.path.join(HERE, 'apply_green_pbt4a.py'), root, '--repo', repo, '--rev', '5aaacce3')
    print('apply_green_pbt4a rc', rc, out.strip().split('\n')[-1])
    rc, out = run(os.path.join(HERE, 'apply_red_pbt4b.py'), root, '--repo', repo, '--rev', '5aaacce3', '--dry')
    print('apply_red_pbt4b --dry rc', rc, '|', out.strip().split('\n')[0][:120], '|', out.strip().split('\n')[-1])
    rc, out = run(os.path.join(HERE, 'apply_red_pbt4b.py'), root, '--repo', repo, '--rev', '5aaacce3')
    print('apply_red_pbt4b rc', rc, '|', out.strip().split('\n')[-1])
    skips = 0
    for rel in sorted(set(files)):
        if rel.startswith('tests/'):
            skips += open(os.path.join(root, *rel.split('/')), encoding='utf-8').read().count('(Skip = "I-64 F4B PB-T4b: ')
    print('Skip de PB-T4b escritos:', skips)
    rc, out = run(os.path.join(HERE, 'apply_red_pbt4b.py'), root, '--repo', repo, '--rev', '5aaacce3', '--dry')
    print('apply_red_pbt4b otra vez (debe parar) rc', rc, '|', out.strip().split('\n')[0][:100])
    rc, out = run(os.path.join(HERE, 'apply_green_pbt4b.py'), root, '--repo', repo, '--rev', '5aaacce3')
    print('apply_green_pbt4b rc', rc, '|', out.strip().split('\n')[-1])
    rc, out = run(os.path.join(HERE, 'apply_green_pbt4b.py'), root, '--repo', repo, '--rev', '5aaacce3', '--dry')
    print('apply_green_pbt4b sobre el GREEN (debe parar) rc', rc, '|', out.strip().split('\n')[0][:100])
finally:
    shutil.rmtree(root)
    print('borrada', os.path.exists(root) is False)
