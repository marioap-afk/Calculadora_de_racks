"""Sonda: cuántas de la lista C-5 de PB-T4b nombran por texto algo retirado (el solape con «Retirarlo»), y la cifra
comparable a las «99 pruebas propias» de la pregunta 1 (todas las de fuera de las 9 clases que nombran lo retirado).

Uso: python -I probe_overlap.py <checkout> <rev> <pbt4-substitution-lists.tsv>
"""
import re
import sys
import importlib.util

spec = importlib.util.spec_from_file_location('lc', __file__.replace('probe_overlap.py', 'lists_core.py'))
lc = importlib.util.module_from_spec(spec)
spec.loader.exec_module(lc)
lc.REPO, lc.REV = sys.argv[1:3]
RETIRED = re.compile(r'PushBackPilotStateComparer|PushBackPilotFieldAdapter|KeepPilotEditsOn|ComposeForWrite|PushBackPilotUpdatePlanner\.Prepare\s*\(|PushBackPilotUpdateRun\.(?:Execute|Precheck)\s*\(')
rows = [l.split('\t') for l in open(sys.argv[3], encoding='utf-8').read().splitlines()[1:]]
cache = {}
overlap = []
for lst, path, name, _, _ in rows:
    if lst != 'PB-T4b C-5' or not path.startswith('tests/'):
        continue
    if path not in cache:
        cache[path] = {m['name']: t for m, t in lc.tests_with_text(path)}
    if RETIRED.search(cache[path].get(name, '')):
        overlap.append((path.rsplit('/', 1)[-1], name))
c7_own = sum(1 for r in rows if r[0] == 'PB-T4b C-7' and 'E-3' not in r[4] and 'E-5' not in r[4] and 'E-6' not in r[4])
print('C-5 que nombran lo retirado:', len(overlap))
for o in overlap:
    print('  ', *o)
print('C-7 propias (sin E-3, E-5 y E-6):', c7_own)
print('comparable a las 99 de Q1 (C-7 propias + solape con C-5):', c7_own + len(overlap))
