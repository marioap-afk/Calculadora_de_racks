"""Revision v3 (solo lectura): SHA-256 de v2 y v3, diff de lineas y comparacion de AST nodo a nodo.
Uso: python -I -B diff_ast_check.py <v2.py> <v3.py>"""
import ast, difflib, hashlib, sys

v2p, v3p = sys.argv[1], sys.argv[2]
b2, b3 = open(v2p, 'rb').read(), open(v3p, 'rb').read()
print('SHA v2', hashlib.sha256(b2).hexdigest().upper())
print('SHA v3', hashlib.sha256(b3).hexdigest().upper())
print('bytes v2/v3', len(b2), len(b3), '| CRLF v2/v3', b2.count(b'\r\n'), b3.count(b'\r\n'), '| BOM', b2[:3] == b'\xef\xbb\xbf', b3[:3] == b'\xef\xbb\xbf')
print('no ASCII v2/v3:', sorted({c for c in b2.decode('utf-8') if ord(c) > 127}), sorted({c for c in b3.decode('utf-8') if ord(c) > 127}))
t2, t3 = b2.decode('utf-8'), b3.decode('utf-8')
l2, l3 = t2.splitlines(keepends=True), t3.splitlines(keepends=True)
sm = difflib.SequenceMatcher(a=l2, b=l3, autojunk=False)
print('--- bloques distintos (v2 lineas -> v3 lineas)')
for op, a1, a2, b1, b2_ in sm.get_opcodes():
    if op != 'equal':
        print(' ', op, 'v2 l.%d-%d' % (a1 + 1, a2), '-> v3 l.%d-%d' % (b1 + 1, b2_))
m2, m3 = ast.parse(t2), ast.parse(t3)
print('--- AST: nodos de nivel superior', len(m2.body), len(m3.body))
assert len(m2.body) == len(m3.body)
for i, (n2, n3) in enumerate(zip(m2.body, m3.body)):
    same = ast.dump(n2) == ast.dump(n3)
    name = getattr(n2, 'name', None) or type(n2).__name__
    if not same:
        print('  distinto: nodo %d (%s)' % (i, name))
# docstring del modulo
print('docstring distinto:', ast.get_docstring(m2) != ast.get_docstring(m3))
# dentro de evaluate: sentencias distintas
f2 = next(n for n in m2.body if isinstance(n, ast.FunctionDef) and n.name == 'evaluate')
f3 = next(n for n in m3.body if isinstance(n, ast.FunctionDef) and n.name == 'evaluate')
d2, d3 = [ast.dump(s) for s in f2.body], [ast.dump(s) for s in f3.body]
sm = difflib.SequenceMatcher(a=d2, b=d3, autojunk=False)
print('--- evaluate: sentencias v2/v3', len(d2), len(d3))
for op, a1, a2, b1, b2_ in sm.get_opcodes():
    if op != 'equal':
        print(' ', op)
        for s in f2.body[a1:a2]:
            print('    v2 l.%d: %s' % (s.lineno, ast.unparse(s).replace('\n', ' | ')[:300]))
        for s in f3.body[b1:b2_]:
            print('    v3 l.%d: %s' % (s.lineno, ast.unparse(s).replace('\n', ' | ')[:300]))
# usos de itertools y de los nombres que solo usaba el bloque quitado
for nm, t in (('v2', t2), ('v3', t3)):
    tree = ast.parse(t)
    uses = sorted({n.lineno for n in ast.walk(tree) if isinstance(n, ast.Name) and n.id == 'itertools'})
    print(nm, 'usos de itertools (lineas):', uses)
# variables que el resto de evaluate lee de las quitadas (cands, excl)
names_after = {n.id for s in f3.body for n in ast.walk(s) if isinstance(n, ast.Name)}
print('v3 evaluate lee cands/excl:', {'cands', 'excl'} & names_after)
