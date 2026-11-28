"""PB-T4b, precondición 6: las filas de las `Theory` que mezclan resultados en una TRX del modo de medida (la parada de la partición), con su
resultado, la primera línea de su StackTrace y su mensaje; y el resumen de la partición que sí sale (sin escribirla). Solo lee.

Uso: python -I mixed_rows_p6.py <core.trx> <ui.trx> <salida.txt>"""
import os
import re
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import p6_check  # noqa: E402  (el lector de TRX de la precondición)

core, ui, out = sys.argv[1:4]
sys.stdout.reconfigure(encoding='utf-8', errors='replace')
rs = p6_check.results(core)[0] + p6_check.results(ui)[0]
per = {}
for r in rs:
    m = p6_check.CLASS.match(r['class'])
    if m:
        per.setdefault((m.group(1), r['method']), []).append(r)
lines = []
red = green = mixed = 0
red_rows = green_rows = 0
not_assert = []
for (stem, method), group in sorted(per.items()):
    outs = {r['outcome'] for r in group}
    if outs == {'Passed'}:
        green += 1
        green_rows += len(group)
    elif outs == {'Failed'}:
        red += 1
        red_rows += len(group)
        for r in group:
            if not re.search(r'[/\\]tests[/\\]', r['stack1']):
                not_assert.append('%s.%s %s' % (stem, r['test'][:120], r['stack1'][:160]))
    else:
        mixed += 1
        lines.append('== %sFull.%s: %d filas, %s' % (stem, method, len(group), {o: sum(1 for r in group if r['outcome'] == o) for o in outs}))
        for r in sorted(group, key=lambda x: (x['outcome'], x['test'])):
            lines.append('   %s %s' % (r['outcome'], r['test'][:220]))
            if r['outcome'] != 'Passed':
                lines.append('        %s' % r['stack1'][:200])
                lines.append('        %s' % ' '.join(r['message'].split())[:300])
lines.insert(0, 'pruebas de copia: %d; todas Passed %d (%d filas); todas Failed %d (%d filas); mezclan %d; fallos sin asercion en las Red: %d %s' % (
    len(per), green, green_rows, red, red_rows, mixed, len(not_assert), not_assert[:5]))
with open(out, 'w', encoding='utf-8', newline='\n') as f:
    f.write('\n'.join(lines) + '\n')
print('\n'.join(lines))
