"""PB-T4b (paquete v4.8): el mapa de la copia v5, el v4 (pbt4b-copy-map-v4.tsv, que no se toca) con D-S1 aplicada a dos copias mecánicas de WPP
que no pasan en el GREEN (la precondición 6 de la v4.7, /d/r4m; session-pbt4b-design-decisions.md): fijan que el piloto nunca borra una vista, y el
modelo completo borra la lateral obsoleta (D-PB-04, D-PBT3-09 (e) y D-PBT4-07). Pasan a «a mano» con el motivo R-D-S1-borra. La guarda de los
textos del anfitrión de la lista de 478 es de C-5 y no está en el mapa. Escribe pbt4b-copy-map-v5.tsv e imprime su sha256 y sus recuentos.
Uso: python -I make_copy_map_v5.py"""
import hashlib
import os
import sys

sys.stdout.reconfigure(encoding='utf-8', errors='replace')
HERE = os.path.dirname(os.path.abspath(__file__))
raw = open(os.path.join(HERE, 'pbt4b-copy-map-v4.tsv'), 'rb').read()
assert hashlib.sha256(raw).hexdigest() == '0c5abc6d00d9dfc9943812aae4c9fd31338c3b64f17cbff632f949378e9d9b1c'
lines = raw.decode('utf-8').split('\n')
CHANGE = {
    ('WorkspacePilotPrepareTests', 'InvalidE_AMutableMemberThatIsNotAvailableInTheOutgoingSystemOrWhoseEnvelopeDoesNotDecodeIsInvalidAndNeverErasesAView'): ('mecanica', 'R-D-S1-borra'),
    ('WorkspacePilotPrepareTests', 'ThePilotNeverErasesAViewAPhantomThatWasAlreadyInTheDrawingMakesTheUpdateInvalidAndIsNeverInThePlan'): ('mecanica', 'R-D-S1-borra'),
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
open(os.path.join(HERE, 'pbt4b-copy-map-v5.tsv'), 'wb').write(data)
rows = [l.split('\t') for l in out[1:] if l]
mec = sum(1 for r in rows if r[4] == 'mecanica')
assert (mec, len(rows) - mec) == (356, 72), (mec, len(rows) - mec)
print('pbt4b-copy-map-v5.tsv sha256 %s %d bytes; filas %d; mecanicas %d; a mano %d' % (hashlib.sha256(data).hexdigest(), len(data), len(rows), mec, len(rows) - mec))
