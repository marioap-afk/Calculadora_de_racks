"""Sonda: los miembros que detecta lists_pbt4.members en un archivo de una revision, con sus lineas.

Uso: python -I probe_members.py <checkout> <rev> <ruta> [desde] [hasta]
"""
import sys
import importlib.util

repo, rev, path = sys.argv[1:4]
lo = int(sys.argv[4]) if len(sys.argv) > 4 else 0
hi = int(sys.argv[5]) if len(sys.argv) > 5 else 10 ** 9
spec = importlib.util.spec_from_file_location('lp', __file__.replace('probe_members.py', 'lists_core.py'))
lp = importlib.util.module_from_spec(spec)
spec.loader.exec_module(lp)
lp.REPO, lp.REV = repo, rev
for m in lp.members(lp.blank_comments(lp.show(path))):
    if m['last'] >= lo and m['first'] <= hi:
        print('%5d-%5d %s %s' % (m['first'], m['last'], 'T' if m['test'] else 'h', m['name']))
