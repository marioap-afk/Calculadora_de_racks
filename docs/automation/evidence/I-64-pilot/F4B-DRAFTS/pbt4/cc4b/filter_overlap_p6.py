"""PB-T4b, diagnóstico (solo lectura): qué filtros de los RequiredTests de §6.2.1 de un paquete, salvo los de las copias, seleccionarían las clases de
las copias (por `FullyQualifiedName~`, que es «contiene»), con cuatro esquemas de nombre: el de hoy (`…FullPanelCopies.<Raíz>Full<Parte>Tests`), el
espacio raíz con los nombres de hoy (`RackCad(.UI)?.Tests.<Raíz>Full<Parte>Tests`) y el espacio raíz con dos prefijos (`…Tests.FullCopy<Raíz><Parte>Tests` y `…Tests.FullPanelCopy<Raíz><Parte>Tests`).

Uso: python -I filter_overlap_p6.py <paquete.md> <salida.txt>"""
import re
import sys

sys.stdout.reconfigure(encoding='utf-8', errors='replace')
pkg, out = sys.argv[1:3]
text = open(pkg, encoding='utf-8').read()
start = text.index('\n#### 6.2.1 ')
end = text.index('\n#### 6.2.2 ', start)
rows = []
for l in text[start:end].split('\n'):
    m = re.match(r'^\| (\d+) \| `tests/(RackCad(?:\.UI)?\.Tests)/[^`]+` \| `FullyQualifiedName~([\w.]+)` \| ([^|]+) \| (\w+) \|', l)
    if m and '.FullPanelCopies.' not in m.group(3):
        rows.append((int(m.group(1)), m.group(2), m.group(3), m.group(4).strip(), m.group(5)))
CORE = ['WorkspaceLifecycleHost', 'WorkspacePilotPrepare', 'WorkspaceEditIntentHost', 'WorkspacePilotEditHost', 'WorkspaceCommandAdvisor',
        'WorkspacePilotUpdateRequest', 'WorkspacePilotUpdateHost', 'WorkspacePilotReconciliationHost', 'WorkspaceAssociativeBookkeeping',
        'PushBackWritePathGuard', 'WorkspacePilotUpdateIntegrationGuard', 'WorkspaceEditIntentGuard', 'WorkspacePilotEditGuard',
        'WorkspacePilotReconciliationGuard', 'WorkspaceLifecycleGuard', 'WorkspacePilotUpdateGuard', 'PushBackFullReadPath', 'PushBackFullReadGuard']
UI = ['WorkspacePilotReconciliation', 'WorkspacePilotReconciliationView']
schemes = {
    'hoy (FullPanelCopies)': lambda proj, s, p: '%s.FullPanelCopies.%sFull%sTests' % (proj, s, p),
    'raiz, nombres de hoy': lambda proj, s, p: '%s.%sFull%sTests' % (proj, s, p),
    'raiz, prefijo FullCopy': lambda proj, s, p: '%s.FullCopy%s%sTests' % (proj, s, p),
    'raiz, prefijo FullPanelCopy': lambda proj, s, p: '%s.FullPanelCopy%s%sTests' % (proj, s, p),
}
lines = ['filtros heredados y de las CT de §6.2.1 (sin los de las copias): %d' % len(rows)]
for name, fqn in schemes.items():
    hits = []
    for n, proj, flt, mins, red in rows:
        sel = [fqn(proj, s, p) for s in (CORE if proj == 'RackCad.Tests' else UI) for p in ('Red', 'Green') if flt in fqn(proj, s, p)]
        if sel:
            reds = sum(1 for c in sel if c.endswith('RedTests'))
            hits.append('  #%d %s (MinSelected %s, ExpectRed %s): %d clases de copia, %d Red' % (n, flt, mins, red, len(sel), reds))
    lines.append('%s: %d filtros seleccionan copias' % (name, len(hits)))
    lines.extend(hits)
with open(out, 'w', encoding='utf-8', newline='\n') as f:
    f.write('\n'.join(lines) + '\n')
print('\n'.join(lines))
