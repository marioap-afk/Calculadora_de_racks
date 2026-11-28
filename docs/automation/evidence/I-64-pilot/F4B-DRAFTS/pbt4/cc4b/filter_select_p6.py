"""PB-T4b, diagnóstico (solo lectura): lo que seleccionaría un filtro `FullyQualifiedName~<texto>` en una TRX de la suite completa (filas por
resultado, y cuántas son de las clases de las copias).

Uso: python -I filter_select_p6.py <trx> <texto> <salida.txt>"""
import collections
import importlib.util
import os
import re
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
sys.stdout.reconfigure(encoding='utf-8', errors='replace')
spec = importlib.util.spec_from_file_location('p6c', os.path.join(HERE, 'p6_check_v47.py'))
p6 = importlib.util.module_from_spec(spec)
spec.loader.exec_module(p6)
trx, needle, out = sys.argv[1:4]
copy_rx = re.compile(r'Full(Probe|Red|Green)Tests$')
rs, _ = p6.results(trx)
sel = [r for r in rs if needle in (r['class'] + '.' + r['method'])]
c = collections.Counter(r['outcome'] for r in sel)
cc = collections.Counter(r['outcome'] for r in sel if copy_rx.search(r['class']) and r['class'].rsplit('.', 1)[-1][:-len('Tests')].replace('FullRed', '').replace('FullGreen', '') in p6.STEMS)
line = '%s | FullyQualifiedName~%s | filas %d %s | de las copias: %d %s' % (os.path.basename(trx), needle, len(sel), dict(c), sum(cc.values()), dict(cc))
with open(out, 'a', encoding='utf-8', newline='\n') as f:
    f.write(line + '\n')
print(line)
