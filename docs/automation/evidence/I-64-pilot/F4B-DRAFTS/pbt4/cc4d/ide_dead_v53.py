"""PB-T4d (paquete v5.3): lee los avisos IDE0051 e IDE0052 (miembro privado sin uso o sin leer) de los registros de compilacion de un ensayo
(t53_trial_v53.sh: b-* la base, r-* el RED, g-* el GREEN) y escribe los de cada rama y los nuevos frente a la base, por archivo y miembro, sin
linea ni columna (cambian con las bajas). Solo para CA-T4d M-03; no es del contrato. Solo lee. UTF-8 con LF.

Uso: python -I ide_dead_v53.py <dir-ensayo> <salida.txt>
"""
import os
import re
import sys

D, OUT = sys.argv[1:3]
RX = re.compile(r'^(?P<file>.*?\.cs)\(\d+,\d+\): warning (?P<rule>IDE005[12]): (?P<msg>.*?)(?: \[[^\]]*\])?$')


def read(tag):
    found = set()
    for kind in ('core', 'ui'):
        p = os.path.join(D, '%s-%s.log' % (tag, kind))
        if not os.path.exists(p):
            continue
        for line in open(p, encoding='utf-8', errors='replace'):
            m = RX.match(line.strip())
            if not m:
                continue
            f = m.group('file').replace('\\', '/')
            i = max(f.find('/src/'), f.find('/tests/'))
            found.add((f[i + 1:] if i >= 0 else f, m.group('rule'), m.group('msg')))
    return found


b, r, g = read('b'), read('r'), read('g')
out = ['# IDE0051/IDE0052 de %s: base %d, RED %d, GREEN %d' % (os.path.basename(D), len(b), len(r), len(g))]
for title, items in (('nuevos en el RED frente a la base', r - b), ('nuevos en el GREEN frente al RED', g - r),
                     ('los que se van en el RED', b - r), ('la base', b)):
    out.append('')
    out.append('## %s (%d)' % (title, len(items)))
    out += ['  %s | %s | %s' % x for x in sorted(items)]
with open(OUT, 'w', encoding='utf-8', newline='\n') as f:
    f.write('\n'.join(out) + '\n')
print('\n'.join(out[:1]))
