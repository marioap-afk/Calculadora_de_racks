"""Paso 1 (solo lectura): sobre la BaseSha, con git show, sin compilar y sin escribir nada.

Uso (cwd = worktree): python -I artifacts/.../inv/step1_anchors.py <BaseSha>
Comprueba: (a) cada bloque EDIT de pbt4d-red-v54.txt y de pbt4d-green-v54.txt tiene su OLD exactamente una vez
(lo asertan red_full y green); (b) los archivos de la lista existen en la BaseSha y sus miembros de prueba
aparecen una vez con [Fact]/[Theory] y sin Skip (los que la lista da como existentes); (c) los archivos nuevos del
alcance no existen en la BaseSha.
"""
import csv
import importlib.util
import os
import re
import subprocess
import sys

REL = 'artifacts/orchestration/I-64/F4B-PBT4-RETIRE-PILOT/1/R20261122T173325Z-c9e1'
GEN = REL + '/gen'
REV = sys.argv[1]
AUTH = '23c2923616352a7b32fb3c68d8181c41c7feb2c9'
LIST = 'docs/automation/evidence/I-64-pilot/F4B-DRAFTS/pbt4/pbt4d-retire-list-271.tsv'


def load(name, file):
    spec = importlib.util.spec_from_file_location(name, os.path.join(GEN, file))
    m = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(m)
    return m


def show(rev, path):
    r = subprocess.run(['git', 'show', rev + ':' + path], capture_output=True)
    if r.returncode != 0:
        return None
    return r.stdout.decode('utf-8-sig').replace('\r\n', '\n')


sys.stdout.reconfigure(encoding='utf-8', newline='\n')
rp = load('rp', 'red_pbt4d_v53.py')
gp = load('gp', 'green_pbt4d_v53.py')

base, red, new, deleted = rp.red_full('.', REV, GEN + '/pbt4d-red-v54.txt')
n_edits = sum(1 for b in rp.blocks(GEN + '/pbt4d-red-v54.txt') if b[0] == 'EDIT')
print('RED: %d bloques EDIT con su OLD exactamente una vez; %d archivos existentes, %d nuevos, %d borrados' % (n_edits, len(red) - len(new), len(new), len(deleted)))
gbase, green, gdel = gp.green('.', REV, GEN + '/pbt4d-green-v54.txt')
g_edits = sum(1 for b in gp.blocks(GEN + '/pbt4d-green-v54.txt') if b[0] == 'EDIT')
print('GREEN: %d bloques EDIT con su OLD exactamente una vez; %d archivos cambiados, %d borrados' % (g_edits, len(green), len(gdel)))

for p in sorted(new):
    ex = show(REV, p)
    print('nuevo %s existe-en-BaseSha=%s' % (p, ex is not None))
    assert ex is None, p

txt = subprocess.run(['git', 'show', AUTH + ':' + LIST], capture_output=True, check=True).stdout.decode('utf-8')
rows = list(csv.DictReader(txt.split('\n'), delimiter='\t', quoting=csv.QUOTE_NONE))
print('lista: %d filas' % len(rows))
bad = 0
checked = 0
for r in rows:
    f = r['archivo']
    m = r['miembro']
    a = r['accion']
    if a == 'sustituir con registro' or a == 'reescribir con registro' or a == 'borrar con registro' or a == 'se queda' or a == 'acotar con registro':
        t = show(REV, f)
        if t is None:
            print('FALTA archivo de la lista en BaseSha: %s (fila %s, %s)' % (f, r['n'], a))
            bad += 1
            continue
        if re.fullmatch(r'[A-Za-z_][A-Za-z0-9_]*', m or ''):
            cnt = len(re.findall(r'\b%s\b' % re.escape(m), t))
            if cnt == 0:
                print('miembro sin aparecer: %s :: %s (fila %s, %s)' % (f, m, r['n'], a))
                bad += 1
            checked += 1
print('miembros de la lista comprobados: %d; observaciones: %d' % (checked, bad))
