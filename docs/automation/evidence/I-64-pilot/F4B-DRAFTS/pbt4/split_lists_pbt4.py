"""PB-T4 v4.2: separa de pbt4-substitution-lists.tsv la lista de PB-T4b (sin C-7, la que el Owner tomó como «la que vio») y
el esbozo de la lista de PB-T4d (la retirada: C-7, las 55 originales saltadas que dejarán de compilar y los 29 miembros de apoyo).

Uso: python -I split_lists_pbt4.py
Lee solo, en esta carpeta: pbt4-substitution-lists.tsv, pbt4-retire-compile.tsv y retire_compile_pbt4-out.txt.
Escribe solo, en esta carpeta: pbt4b-substitution-list.tsv y pbt4d-retire-list.tsv. Imprime sus sha256 y la regla de filtrado.
"""
import hashlib

here = __file__.replace('split_lists_pbt4.py', '')
src = open(here + 'pbt4-substitution-lists.tsv', encoding='utf-8').read()
lines = src.splitlines()
header, rows = lines[0], [l.split('\t') for l in lines[1:]]
B_LISTS = ('PB-T4b C-0', 'PB-T4b C-5', 'PB-T4b UI')
b_rows = [r for r in rows if r[0] in B_LISTS]
assert len(b_rows) == 470, len(b_rows)
assert len({(r[1], r[2]) for r in b_rows}) == 470
out_b = header + '\n' + '\n'.join('\t'.join(r) for r in b_rows) + '\n'
open(here + 'pbt4b-substitution-list.tsv', 'w', encoding='utf-8', newline='\n').write(out_b)

c7 = [r for r in rows if r[0] == 'PB-T4b C-7']
assert len(c7) == 139
ret = [l.split('\t') for l in open(here + 'pbt4-retire-compile.tsv', encoding='utf-8').read().splitlines()[1:]]
nine = [r for r in ret if r[3] == 'las 9 clases']
assert len(nine) == 55
out = open(here + 'retire_compile_pbt4-out.txt', encoding='utf-8').read().splitlines()
i = next(k for k, l in enumerate(out) if l.startswith('miembros de apoyo afectados'))
support = [l.strip().split(' ', 1) for l in out[i + 1:] if l.startswith('    ')]
assert len(support) == 29
d = ['lista\tarchivo\tmiembro\tlineas\taccion']
d += ['PB-T4d C-7\t%s\t%s\t%s\tsustituir con registro (%s)' % (r[1], r[2], r[3], r[4]) for r in c7]
d += ['PB-T4d original\t%s\t%s\t%s\tborrar con registro: original de las 9 clases, saltada en PB-T4b, que deja de compilar al retirar' % (r[0], r[1], r[2]) for r in nine]
d += ['PB-T4d apoyo\t%s\t%s\t-\tborrar o editar con registro: miembro de apoyo que nombra lo retirado' % (f, m) for f, m in support]
out_d = '\n'.join(d) + '\n'
open(here + 'pbt4d-retire-list.tsv', 'w', encoding='utf-8', newline='\n').write(out_d)
for name, text in (('pbt4-substitution-lists.tsv', src), ('pbt4b-substitution-list.tsv', out_b), ('pbt4d-retire-list.tsv', out_d)):
    print(name, hashlib.sha256(text.encode('utf-8')).hexdigest(), len(text.encode('utf-8')))
print('regla: de pbt4-substitution-lists.tsv, las filas cuya columna lista es PB-T4b C-0, PB-T4b C-5 o PB-T4b UI, en su orden, con la cabecera:', len(b_rows))
print('PB-T4d:', len(c7), len(nine), len(support))
