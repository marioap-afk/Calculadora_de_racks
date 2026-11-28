"""Lista, del recorrido de PB-T4b (pbt4b-guard-walk.tsv), las pruebas A o B fuera de la lista de 470, por clase, con sus diferencias recortadas.
Uso: python -I flagged_walk_b.py [ancho]"""
import os, sys
sys.stdout.reconfigure(encoding='utf-8', errors='replace')
HERE = os.path.dirname(os.path.abspath(__file__))
width = int(sys.argv[1]) if len(sys.argv) > 1 else 260
rows = [l.rstrip('\n').split('\t') for l in open(os.path.join(HERE, 'pbt4b-guard-walk.tsv'), encoding='utf-8')][1:]
out = [r for r in rows if r[3] in ('A', 'B') and r[4] == '-']
last = None
for r in out:
    if r[0] != last:
        print('== ' + r[0].rsplit('/', 1)[-1])
        last = r[0]
    print(('  %s %s %s lee=%s sel=%s | %s' % (r[3], r[1], r[2], r[5], r[6], r[7]))[:width])
print('total fuera de la lista:', len(out))
