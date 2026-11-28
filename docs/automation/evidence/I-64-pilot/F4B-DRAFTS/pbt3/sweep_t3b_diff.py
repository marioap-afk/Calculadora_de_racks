"""Solo lectura: compara dos salidas de guard_sweep.py (la de la emision de PB-T3a, 18e4b783, y la del cierre de PB-T3a, c40eea4a) en el corpus de
los archivos de PB-T3b (PP, UR y la interfaz nueva: D-PBT3-09 y D-PBT3-10 del paquete). Da las coincidencias nuevas o perdidas por archivo de
prueba, sin el numero de linea (las guardas que no cambian conservan sus filas aunque se muevan), y las guardas nuevas que leen src/.
Uso: python -I sweep_t3b_diff.py <hits-antes.txt> <hits-despues.txt> <salida.txt> [<corpus>]   (la salida, en UTF-8)"""
import collections
import sys

before_path, after_path, out_path = sys.argv[1:4]
corpus = sys.argv[4] if len(sys.argv) > 4 else 'Pilot/PP + UR (lo nuevo de PB-T3b)'


def rows(path):
    with open(path, encoding='utf-8') as f:
        head = f.readline().strip()
        out, files = collections.Counter(), set()
        for line in f:
            parts = line.rstrip('\n').split(' | ')
            if len(parts) < 4:
                continue
            files.add(parts[0].rsplit(':', 1)[0])
            if parts[1] == corpus:
                out[(parts[0].rsplit(':', 1)[0], parts[2], ' | '.join(parts[3:]))] += 1
    return head, out, files


hb, b, fb = rows(before_path)
ha, a, fa = rows(after_path)
new, lost = a - b, b - a
R = ['corpus: ' + corpus,
     'antes: %s | filas del corpus: %d' % (hb, sum(b.values())),
     'despues: %s | filas del corpus: %d' % (ha, sum(a.values())),
     'guardas con coincidencias (cualquier corpus): antes %d, despues %d; nuevas: %s' % (len(fb), len(fa), ', '.join(sorted(fa - fb)) or 'ninguna'),
     'filas del corpus nuevas: %d | perdidas: %d' % (sum(new.values()), sum(lost.values()))]
for (f, lit, val), n in sorted(new.items()):
    R.append('  + %s | %s | %s (x%d)' % (f, lit, val, n))
for (f, lit, val), n in sorted(lost.items()):
    R.append('  - %s | %s | %s (x%d)' % (f, lit, val, n))
fc_b, fc_a = {k[0] for k in b}, {k[0] for k in a}
R.append('guardas con coincidencias en el corpus: antes %d, despues %d; nuevas: %s' % (len(fc_b), len(fc_a), ', '.join(sorted(fc_a - fc_b)) or 'ninguna'))
open(out_path, 'w', encoding='utf-8', newline='\n').write('\n'.join(R) + '\n')
print('\n'.join(R[:5]))
