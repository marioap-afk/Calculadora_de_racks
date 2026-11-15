"""Comprueba, de forma independiente, que los literales de TraverseBaseline son los de c3ae1c97 (D-T4B-04 (1) y (4b)).
Uso (desde la raiz del worktree): python _check_traverse_literals.py
Compara con el espacio en blanco colapsado y SIN quitar comentarios: es mas estricto que la normalizacion de la guarda."""
import subprocess

base = subprocess.run(['git', 'show', 'c3ae1c97:src/RackCad.Plugin/Views/RackSiblingScan.cs'], capture_output=True, text=True, encoding='utf-8').stdout
base = base.replace('\r\n', '\n')
if base.startswith('﻿'):
    base = base[1:]
L = base.split('\n')
line = lambda a, b: L[a - 1:b]  # 1-based, inclusive

core = '\n'.join(line(103, 110) + line(115, 161) + line(166, 169))
assert core.count('document.Database.BlockTableId') == 1
core = core.replace('document.Database.BlockTableId', 'database.BlockTableId')
rest = '\n'.join(L[:97] + L[170:])

test = open('tests/RackCad.Tests/Workspace/WorkspacePilotUpdateGuardTests.cs', encoding='utf-8').read().replace('\r\n', '\n')
q = '"' * 3
i = test.index('public const string CoreLiteral = ' + q) + len('public const string CoreLiteral = ' + q)
core_lit = test[i:test.index(q, i)]
j = test.index('private const string RestRaw = ' + q) + len('private const string RestRaw = ' + q)
rest_lit = test[j:test.index(q, j)].replace('@NS@ ', 'namespace ')

ws = lambda s: ' '.join(s.split())
print('core-equal', ws(core) == ws(core_lit), len(ws(core)), len(ws(core_lit)))
print('rest-equal', ws(rest) == ws(rest_lit), len(ws(rest)), len(ws(rest_lit)))
print('line-98', repr(L[97][:80]))
print('line-170', repr(L[169][:80]))
print('line-171', repr(L[170][:80]))
print('line-111..114', [x.strip() for x in line(111, 114)])
print('line-162..165', [x.strip() for x in line(162, 165)])
if ws(rest) != ws(rest_lit):
    a, b = ws(rest), ws(rest_lit)
    k = next(n for n in range(min(len(a), len(b))) if a[n] != b[n]) if a[:min(len(a), len(b))] != b[:min(len(a), len(b))] else min(len(a), len(b))
    print('first-diff', k, repr(a[k - 60:k + 60]), '|||', repr(b[k - 60:k + 60]))
