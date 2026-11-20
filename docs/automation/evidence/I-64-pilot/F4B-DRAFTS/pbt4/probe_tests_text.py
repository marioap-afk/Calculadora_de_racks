"""Sonda: imprime, de una prueba (archivo:desde-hasta, en una revisión), las líneas que hacen consultas al texto
(Assert, Count, Regex, BodyOf, InOrder, Contains, IndexOf, Files, Code), para leer a mano las que señala el recorrido.

Uso: python -I probe_tests_text.py <checkout> <rev> <ruta>:<desde>-<hasta> [...]
"""
import re
import subprocess
import sys

repo, rev = sys.argv[1:3]
sys.stdout.reconfigure(encoding='utf-8', errors='replace')
KEY = re.compile(r'Assert|Count\(|Regex|BodyOf|InOrder|Contains|IndexOf|Files\(|Code\(|Text\(|Violations\(|Matches|private static|readonly|const ')
cache = {}
for spec in sys.argv[3:]:
    path, span = spec.rsplit(':', 1)
    a, b = (int(x) for x in span.split('-'))
    if path not in cache:
        p = subprocess.run(['git', '-C', repo, 'show', '%s:%s' % (rev, path)], capture_output=True)
        cache[path] = p.stdout.decode('utf-8').replace('\r\n', '\n').split('\n')
    print('== %s:%d-%d' % (path.rsplit('/', 1)[-1], a, b))
    for i in range(a - 1, min(b, len(cache[path]))):
        line = cache[path][i]
        if KEY.search(line) or line.strip().startswith('[') or 'public void' in line:
            print('%5d %s' % (i + 1, line.strip()[:230]))
