"""PB-T4b, precondición 6 (v4.8): los filtros de UI de los RequiredTests de §6.2.1 de la v4.8, uno por línea (solo lectura). Sin la clase de las CT de UI
(PushBackFullPanelFieldTests: las CT no están en la copia) y con las filas de las copias de UI (D-S6: `RackCad.UI.Tests.FullPanelCopy<Raíz>RedTests`
y …GreenTests) en un solo filtro de su prefijo, `FullyQualifiedName~RackCad.UI.Tests.FullPanelCopy`, que vale también en el modo de medida. Nunca la
suite completa de UI. Es ui_filters_p6.py (el de la v4.5 a la v4.7, que no se toca) con el prefijo de D-S6.

Uso: python -I ui_filters_p6_v48.py <paquete-v4.8.md> <salida.txt>"""
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
    if f.startswith('FullyQualifiedName~RackCad.UI.Tests.FullPanelCopy'):
        f = 'FullyQualifiedName~RackCad.UI.Tests.FullPanelCopy'
    if f not in out:
        out.append(f)
with open(sys.argv[2], 'w', encoding='utf-8', newline='\n') as fh:
    fh.write('\n'.join(out) + '\n')
print('\n'.join(out))
