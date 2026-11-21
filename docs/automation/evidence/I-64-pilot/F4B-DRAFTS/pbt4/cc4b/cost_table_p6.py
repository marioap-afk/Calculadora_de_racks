"""PB-T4b, precondición 6 de la v4.8, paso 7: la tabla de un escenario de la sonda del coste en varias corridas (solo lectura): por corrida, `Begin`,
`cae-al-Primary` y `origen-es-Primary`, con la mediana y el p95 de t4a y de t4b, el límite de «sin regresión» de §6.2.1 (t4a + el mayor de 10 ms y el
30 % de t4a, sobre la mediana) y si se cumple.

Uso: python -I cost_table_p6.py <escenario> <salida.txt> <corrida.txt> [<corrida.txt> ...]"""
import re
import sys

sys.stdout.reconfigure(encoding='utf-8', errors='replace')
scen, out, runs = sys.argv[1], sys.argv[2], sys.argv[3:]
CASES = [('Begin', 'Begin'), ('cae-al-Primary', 'relectura cae-al-Primary (sin coincidencia)'), ('origen-es-Primary', 'relectura origen-es-Primary')]
RX = re.compile(r'^medida \| (.+?) \| (.+?) \| mediana=([\d.]+) ms \| p95=([\d.]+) ms \| n=(\d+)$')
lines = ['escenario: %s' % scen, '| Corrida | Caso | t4a mediana (p95) | t4b mediana (p95) | n | Limite | Cumple |', '|---|---|---|---|---|---|---|']
for k, path in enumerate(runs, 1):
    med = {}
    for l in open(path, encoding='utf-8').read().replace('\r\n', '\n').split('\n'):
        m = RX.match(l.strip())
        if m and m.group(1) == scen:
            med[m.group(2)] = (float(m.group(3)), float(m.group(4)), int(m.group(5)))
    for label, key in CASES:
        a, b = med['t4a ' + key], med['t4b ' + key]
        lim = a[0] + max(10.0, 0.3 * a[0])
        lines.append('| %d | %s | %.1f (%.1f) | %.1f (%.1f) | %d | %.1f | %s |' % (k, label, a[0], a[1], b[0], b[1], b[2], lim, 'si' if b[0] <= lim else 'NO'))
with open(out, 'w', encoding='utf-8', newline='\n') as f:
    f.write('\n'.join(lines) + '\n')
print('\n'.join(lines))
