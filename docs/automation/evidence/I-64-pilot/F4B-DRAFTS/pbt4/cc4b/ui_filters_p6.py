"""PB-T4b, precondición 6: los filtros de UI de los RequiredTests de §6.2.1 de la v4.5, uno por línea (solo lectura). Sin la clase de las CT de UI
(PushBackFullPanelFieldTests: las CT no están en la copia) y con las dos filas de cada copia de UI (Red y Green) en un solo filtro de su espacio de
nombres, RackCad.UI.Tests.FullPanelCopies, que vale también en el modo de medida. Nunca la suite completa de UI.

Uso: python -I ui_filters_p6.py <paquete-v4.5.md> <salida.txt>"""
import re
import sys

text = open(sys.argv[1], encoding='utf-8').read()
start = text.index('\n#### 6.2.1 ')
end = text.index('\n#### 6.2.2 ', start)
out = []
for l in text[start:end].split('\n'):
    m = re.match(r'^\| \d+ \| `tests/RackCad\.UI\.Tests/RackCad\.UI\.Tests\.csproj` \| `(FullyQualifiedName~[\w.]+)` \|', l)
    if not m:
        continue
    f = m.group(1)
    if f.endswith('PushBackFullPanelFieldTests'):
        continue
    if '.FullPanelCopies.' in f:
        f = 'FullyQualifiedName~RackCad.UI.Tests.FullPanelCopies'
    if f not in out:
        out.append(f)
with open(sys.argv[2], 'w', encoding='utf-8', newline='\n') as fh:
    fh.write('\n'.join(out) + '\n')
print('\n'.join(out))
