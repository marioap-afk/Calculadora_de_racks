"""M-7: recorre cada sobre de un inventario RCDUMP y lista las claves que contienen 'Dimension' (recursivo,
también dentro de Design, que es JSON en cadena). Uso: python dimkeys.py <inventario>"""
import json, sys
from collections import Counter


def walk(o, path, out):
    if isinstance(o, dict):
        for k, v in o.items():
            p = path + '.' + k
            if 'imension' in k:
                out.append((p, v if not isinstance(v, (dict, list)) else type(v).__name__))
            if isinstance(v, str) and v[:1] in '{[':
                try:
                    walk(json.loads(v), p + '<json>', out)
                    continue
                except Exception:
                    pass
            walk(v, p, out)
    elif isinstance(o, list):
        for i, v in enumerate(o):
            walk(v, path + '[]', out)


L = open(sys.argv[1], encoding='utf-8', errors='replace').read().split('\n')
per_kind = Counter()
rows = []
for i, l in enumerate(L):
    if l.startswith('DEF\t'):
        name = l.split('\t')[1]
        try:
            e = json.loads(L[i + 1])
        except Exception as ex:
            rows.append((name, 'ERR ' + str(ex)[:60]))
            continue
        out = []
        walk(e, '', out)
        kind = e.get('Kind')
        per_kind[kind] += 1
        for p, v in out:
            rows.append((name + ' [' + str(kind) + ']', '%s = %r' % (p, v)))
print('sobres por Kind:', dict(per_kind))
c = Counter(v for _, v in rows)
for v, n in sorted(c.items()):
    print('%4d  %s' % (n, v))
