"""PB-T4d (v5.2, fase de preguntas): de las 138 C-7 de pbt4d-retire-list-138.tsv, cuales nombran ademas el modelo completo (por texto del
metodo, sin cadenas ni comentarios), en 6c2820ba. Solo git show. Salida UTF-8 con LF.

Uso: python -I probe_mixed_v52.py <repo> <salida.txt>
"""
import collections
import importlib.util
import os
import re
import subprocess
import sys

sys.dont_write_bytecode = True
HERE = os.path.dirname(os.path.abspath(__file__))
spec = importlib.util.spec_from_file_location('lists_core', os.path.join(os.path.dirname(HERE), 'lists_core.py'))
lc = importlib.util.module_from_spec(spec)
spec.loader.exec_module(lc)
REPO, OUT = sys.argv[1:3]
REV = '6c2820bab4d8b41e3e229e23f3a1638ccbb04735'
REMEASURE = os.path.join(HERE, 'pbt4d-remeasure-v52.tsv')
FULL = re.compile(r'\b(?:PushBackFull\w*|PrepareFull|ExecuteFull|ForFullWrite|BuildFull|FullPrepareInput|PushBackFullComposition|PushBackFullDraftState\w*|PushBackFullDraftReader|PushBackFullIntentResult|PushBackAnnotationScaleRule)\b')
RETIRED = re.compile(r'\b(?:PushBackPilotStateComparer|PushBackPilotFieldAdapter|ComposeForWrite|KeepPilotEditsOn|PushBackPilotPrepareInput|PushBackPilotComposition|ComparePost)\b|PushBackPilotUpdatePlanner\s*\.\s*Prepare\s*\(|PushBackPilotUpdateRun\s*\.\s*(?:Execute|Precheck)\s*\(')


def show(path):
    return subprocess.run(['git', '-C', REPO, 'show', REV + ':' + path], capture_output=True, check=True).stdout.decode('utf-8-sig').replace('\r\n', '\n')


rows = [l.split('\t') for l in open(REMEASURE, encoding='utf-8').read().rstrip('\n').split('\n')]
head = rows[0]
out = ['# las 138 C-7: que nombran del modelo completo, en ' + REV, '# columnas de la remedicion: ' + ' | '.join(head)]
cache = {}
count = collections.Counter()
for r in rows[1:]:
    d = dict(zip(head, r))
    if 'C-7' not in d.get('lista', ''):
        continue
    path, member, lines = d['archivo'], d['miembro'], d.get('lineas-6c2820ba', d.get('lineas', ''))
    if path not in cache:
        cache[path] = show(path)
    text = cache[path].split('\n')
    a, b = [int(x) for x in lines.split('-')]
    body = lc.mask('\n'.join(text[a - 1:b]))
    full = sorted(set(m.group(0) for m in FULL.finditer(body)))
    ret = sorted(set(re.sub(r'\s+', '', m.group(0)) for m in RETIRED.finditer(body)))
    kind = 'mixta' if full else 'solo-piloto'
    count[kind] += 1
    out.append('%s\t%s\t%s\t%s\tcompleto=%s\tretirado=%s' % (kind, path.split('/')[-1], member, lines, ','.join(full), ','.join(ret)))
out.append('# total ' + ', '.join('%s=%d' % kv for kv in sorted(count.items())))
with open(OUT, 'w', encoding='utf-8', newline='\n') as f:
    f.write('\n'.join(out) + '\n')
print(out[-1])
