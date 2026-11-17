"""Diferencia campo a campo entre el sobre de una definición en dos inventarios RCDUMP (Design se abre como JSON).
Uso: python envdiff.py <invA> <defA> <invB> <defB>"""
import json, sys


def env(path, name):
    L = open(path, encoding='utf-8', errors='replace').read().split('\n')
    for i, l in enumerate(L):
        if l.startswith('DEF\t') and l.split('\t')[1] == name:
            e = json.loads(L[i + 1])
            if isinstance(e.get('Design'), str):
                e['Design'] = json.loads(e['Design'])
            return e
    raise SystemExit('no está ' + name)


def walk(a, b, path, out):
    if isinstance(a, dict) and isinstance(b, dict):
        for k in sorted(set(a) | set(b)):
            if k not in a:
                out.append('+ %s%s = %s' % (path, '.' + k, short(b[k])))
            elif k not in b:
                out.append('- %s%s = %s' % (path, '.' + k, short(a[k])))
            else:
                walk(a[k], b[k], path + '.' + k, out)
    elif isinstance(a, list) and isinstance(b, list):
        if len(a) != len(b):
            out.append('~ %s: lista %d -> %d' % (path, len(a), len(b)))
        for i, (x, y) in enumerate(zip(a, b)):
            walk(x, y, '%s[%d]' % (path, i), out)
    elif a != b:
        out.append('~ %s: %s -> %s' % (path, short(a), short(b)))


def short(v):
    s = json.dumps(v, ensure_ascii=False)
    return s if len(s) < 120 else s[:117] + '...'


a = env(sys.argv[1], sys.argv[2])
b = env(sys.argv[3], sys.argv[4])
out = []
walk(a, b, '', out)
print(len(out), 'diferencias')
for o in out[:80]:
    print(o)
