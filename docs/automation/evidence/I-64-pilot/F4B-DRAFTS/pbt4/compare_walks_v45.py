"""PB-T4b (v4.5): compara los recorridos de las guardas. Las marcadas fuera de la lista en el recorrido entero de la v4.5 tienen que estar en las
108 de la v4.4 (leídas, en verde), en las del recorrido delta de D-S4 (leídas, en verde) o ser copias o el kit (las sustitutas). Lista las que no.

Uso: python -I compare_walks_v45.py <walk-v44.tsv> <walk-delta.tsv> <walk-full.tsv> <salida.txt>
Solo lee las TSV; escribe solo su salida."""
import sys

v44, delta, full, out = sys.argv[1:5]


def flagged(path):
    rows = [l.split('\t') for l in open(path, encoding='utf-8').read().split('\n')[1:] if l]
    return {(r[0], r[1]) for r in rows if r[3] in ('A', 'B') and r[4] == '-'}, rows


a, _ = flagged(v44)
d, _ = flagged(delta)
f, rows = flagged(full)
copies = {k for k in f if k[0].endswith('Full.cs') or k[0].endswith('WorkspaceFullReadKit.cs')}
rest = sorted(f - a - d - copies)
lines = ['v4.4: %d marcadas fuera de la lista; delta de D-S4: %d; v4.5 entero: %d (de ellas, %d en las copias o el kit)' % (len(a), len(d), len(f), len(copies)),
         'del recorrido entero, en las 108 de la v4.4: %d; en el delta: %d; en las dos: %d' % (len(f & a), len(f & d), len(f & a & d)),
         'de las 108 de la v4.4 que ya no salen marcadas fuera de la lista: %d (%s)' % (len(a - f), '; '.join('%s.%s' % (p.rsplit('/', 1)[-1], m) for p, m in sorted(a - f))),
         'marcadas que no estan en ninguno de los tres grupos: %d' % len(rest)]
lines += ['  %s.%s' % (p.rsplit('/', 1)[-1], m) for p, m in rest]
with open(out, 'w', encoding='utf-8', newline='\n') as fh:
    fh.write('\n'.join(lines) + '\n')
print('\n'.join(lines))
