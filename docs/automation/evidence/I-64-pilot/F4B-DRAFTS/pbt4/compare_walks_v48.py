"""PB-T4b (v4.8): lo que cambia el paseo de guardas con la expresión FIELD corregida (walk_guards_pbt4b_v48.py) frente al de la v4.5, con las
mismas entradas: las pruebas que cambian de categoría y, sobre todo, las que pasan a marcadas (A o B) fuera de la lista, sin contar las copias ni
el kit. Una marcada nueva fuera de la lista es «otra guarda» que hay que leer (y, por la regla de la sesión, una parada).

Uso: python -I compare_walks_v48.py <walk-v45.tsv> <walk-v48.tsv> <lista.tsv> <salida.txt>
Solo lee las TSV; escribe solo su salida."""
import sys

v45, v48, list_file, out = sys.argv[1:5]
sys.stdout.reconfigure(encoding='utf-8', errors='replace')


def rows(path):
    return {(r[0], r[1]): r for r in (l.split('\t') for l in open(path, encoding='utf-8').read().split('\n')[1:] if l)}


listed = {(l.split('\t')[1], l.split('\t')[2]) for l in open(list_file, encoding='utf-8').read().split('\n')[1:] if l}
a, b = rows(v45), rows(v48)


def is_copy(path):
    return path.endswith('Full.cs') or path.endswith('WorkspaceFullReadKit.cs')


# las pruebas de la revisión (sin copias ni kit) tienen que ser las mismas en las dos; las copias pueden cambiar (otra versión de las copias)
orig_a, orig_b = {k for k in a if not is_copy(k[0])}, {k for k in b if not is_copy(k[0])}
assert orig_a == orig_b, ('las pruebas de la revision recorridas no son las mismas', len(orig_a ^ orig_b))
copies_only = sorted(set(a) ^ set(b))
a = {k: v for k, v in a.items() if k in b}
changed = sorted(k for k in a if a[k][3] != b[k][3] or a[k][7] != b[k][7])
new_flag = [k for k in changed if b[k][3] in ('A', 'B') and a[k][3] not in ('A', 'B')]
new_out = [k for k in new_flag if k not in listed and not is_copy(k[0])]
more_diffs = [k for k in changed if k not in new_flag]
lines = ['pruebas recorridas en las dos: %d (de la revision, las mismas: %d; solo en una, de las copias: %d)' % (len(a), len(orig_a), len(copies_only)),
         'cambian de categoria o de diferencias: %d; pasan a marcadas (A o B): %d; de ellas fuera de la lista %s y sin ser copia ni kit: %d' % (
             len(changed), len(new_flag), list_file.rsplit('/', 1)[-1], len(new_out))]
for k in new_flag:
    lines.append('  NUEVA %s %s.%s: %s -> %s | %s | %s' % ('fuera-de-la-lista' if k in new_out else ('en-la-lista' if k in listed else 'copia-o-kit'),
                                                       k[0].rsplit('/', 1)[-1], k[1], a[k][3], b[k][3], b[k][6], b[k][7][:400]))
for k in more_diffs:
    lines.append('  MAS-DIFERENCIAS %s.%s: %s -> %s | %s' % (k[0].rsplit('/', 1)[-1], k[1], a[k][3], b[k][3], b[k][7][:300]))
with open(out, 'w', encoding='utf-8', newline='\n') as fh:
    fh.write('\n'.join(lines) + '\n')
print('\n'.join(lines))
