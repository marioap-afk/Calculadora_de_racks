"""PB-T4b (paquete v4.7): el mapa de la copia v4, el v3 (pbt4b-copy-map-v3.tsv, que no se toca) con D-S5: las dos `Theory` de C-0 mecánicas que se
parten por filas pasan a «a mano» con el motivo R-D-S5, y la de WPP que ya era a mano (D-S1) lleva los dos motivos. La de PEG es una guarda (C-5) y
no está en el mapa. Escribe pbt4b-copy-map-v4.tsv e imprime su sha256 y sus recuentos. Uso: python -I make_copy_map_v4.py"""
import hashlib
import os
import sys

sys.stdout.reconfigure(encoding='utf-8', errors='replace')
HERE = os.path.dirname(os.path.abspath(__file__))
raw = open(os.path.join(HERE, 'pbt4b-copy-map-v3.tsv'), 'rb').read()
assert hashlib.sha256(raw).hexdigest() == '11b26ab1ecbfbf4a289c1485c14a803529549b2e066e13588668763448390743'
lines = raw.decode('utf-8').split('\n')
CHANGE = {
    ('WorkspaceLifecycleHostTests', 'EachOperationThatCanChangeADraftEndsTheMarkSoTheSessionIsRetiredConservatively'): ('mecanica', 'R-D-S5'),
    ('WorkspacePilotPrepareTests', 'ConfirmAppliesOnlyToCleanDirtyAndConflictAndTheOthersReadNothing'): ('mecanica', 'R-D-S5'),
    ('WorkspacePilotPrepareTests', 'KeepAppliesToConflictAndToDirtyMarkedStaleApplyingWaitsAndTheOthersAreNotNeededAllWithoutReading'): ('R-D-S1', 'R-D-S1,R-D-S5'),
}
done = set()
out = [lines[0]]
for l in lines[1:]:
    if not l:
        out.append(l)
        continue
    c = l.split('\t')
    k = (c[0], c[1])
    if k in CHANGE:
        assert c[4] == CHANGE[k][0], c
        c[4] = CHANGE[k][1]
        done.add(k)
    out.append('\t'.join(c))
assert done == set(CHANGE), set(CHANGE) - done
data = '\n'.join(out).encode('utf-8')
open(os.path.join(HERE, 'pbt4b-copy-map-v4.tsv'), 'wb').write(data)
rows = [l.split('\t') for l in out[1:] if l]
mec = sum(1 for r in rows if r[4] == 'mecanica')
print('pbt4b-copy-map-v4.tsv sha256 %s %d bytes; filas %d; mecanicas %d; a mano %d' % (hashlib.sha256(data).hexdigest(), len(data), len(rows), mec, len(rows) - mec))
