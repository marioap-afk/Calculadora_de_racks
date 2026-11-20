"""Sonda: las pruebas de un archivo que nombran un ayudante (en su cuerpo o en un ayudante que llaman, a un nivel).

Uso: python -I probe_callers.py <checkout> <rev> <ruta> <ayudante> [<ayudante> ...]
"""
import re
import sys
import importlib.util

spec = importlib.util.spec_from_file_location('lc', __file__.replace('probe_callers.py', 'lists_core.py'))
lc = importlib.util.module_from_spec(spec)
spec.loader.exec_module(lc)
lc.REPO, lc.REV = sys.argv[1:3]
path = sys.argv[3]
for helper in sys.argv[4:]:
    rx = re.compile(r'\b' + re.escape(helper) + r'\b')
    print('==', helper)
    for m, text in lc.tests_with_text(path):
        if rx.search(text):
            print('  %d-%d %s' % (m['first'], m['last'], m['name']))
