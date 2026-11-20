"""Autoprueba de apply_green_pbt4a.py sobre una copia mínima, dentro de esta carpeta (_apply_selftest/), que se borra al final.

Uso: python -I selftest_apply_green.py <checkout> <rev>
Arma la copia mínima con los cuatro archivos de la revisión (por `git show`, con CRLF a propósito para probar la normalización),
corre el guion en --dry y de verdad, comprueba lo escrito contra green_pbt4a.py, comprueba que una segunda corrida para (los
archivos ya no son los de la revisión) y que una copia con .git para. Imprime el resultado de cada paso.
"""
import importlib.util
import os
import shutil
import subprocess
import sys

sys.stdout.reconfigure(encoding='utf-8', errors='replace')
repo, rev = sys.argv[1:3]
here = os.path.dirname(os.path.abspath(__file__))
root = os.path.join(here, '_apply_selftest')
spec = importlib.util.spec_from_file_location('gp', os.path.join(here, 'green_pbt4a.py'))
gp = importlib.util.module_from_spec(spec)
spec.loader.exec_module(gp)
base, green = gp.green(repo, rev)
if os.path.exists(root):
    shutil.rmtree(root)
os.makedirs(os.path.join(root, 'tests'))
for rel, text in base.items():
    path = os.path.join(root, *rel.split('/'))
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, 'wb') as f:
        f.write(text.replace('\n', '\r\n').encode('utf-8'))
script = os.path.join(here, 'apply_green_pbt4a.py')


def run(*extra):
    p = subprocess.run([sys.executable, '-I', script, root, '--repo', repo, '--rev', rev] + list(extra), capture_output=True)
    return p.returncode, p.stdout.decode('utf-8', 'replace'), p.stderr.decode('utf-8', 'replace')


rc, out, err = run('--dry')
print('dry: rc=%d, %d líneas, nada escrito=%s' % (rc, len(out.splitlines()), not os.path.exists(os.path.join(root, *gp.DRAFT_P.split('/')))))
rc, out, err = run()
same = all(open(os.path.join(root, *rel.split('/')), 'rb').read() == text.encode('utf-8') for rel, text in green.items())
print('real: rc=%d, escritos=%d, iguales al GREEN y con LF=%s' % (rc, sum(1 for l in out.splitlines() if l.startswith('escrito')), same))
print(out.rstrip())
rc, out, err = run()
print('segunda corrida: rc=%d, para=%s' % (rc, 'PARA' in (out + err)))
os.makedirs(os.path.join(root, '.git'))
rc, out, err = run('--dry')
print('con .git: rc=%d, para=%s' % (rc, '.git' in (out + err)))
shutil.rmtree(root)
print('copia mínima borrada:', not os.path.exists(root))
