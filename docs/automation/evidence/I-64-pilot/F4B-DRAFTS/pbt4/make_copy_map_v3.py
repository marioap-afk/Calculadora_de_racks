"""PB-T4b (paquete v4.6): el mapa de la copia v3, el v2 (pbt4b-copy-map-v2.tsv, que no se toca) con las ocho pruebas de D-S1 de la precondición 6
de la v4.5 pasadas de «mecanica» a «a mano», con el motivo R-D-S1-compila (no compilan sobre el modelo completo). Escribe pbt4b-copy-map-v3.tsv e
imprime su sha256 y sus recuentos. Uso: python -I make_copy_map_v3.py"""
import hashlib
import os
import sys

sys.stdout.reconfigure(encoding='utf-8', errors='replace')
HERE = os.path.dirname(os.path.abspath(__file__))
raw = open(os.path.join(HERE, 'pbt4b-copy-map-v2.tsv'), 'rb').read()
assert hashlib.sha256(raw).hexdigest() == '3ffadf5b1f19461564d2e3f2f1c3e829395b8ade2f6d43293742619ffe359e01'
lines = raw.decode('utf-8').split('\n')
MOTIVE = 'R-D-S1-compila'
EIGHT = {
    ('WorkspacePilotEditHostTests', 'DiscardRackNeedsNoComparerAndRemovesAnUndecidableDraftToo'),
    ('WorkspacePilotEditHostTests', 'AComparerThatThrowsMakesTheFieldUndecidableProtectedAndNotEditableAndGetFieldNeverThrows'),
    ('WorkspacePilotEditHostTests', 'TheRacksAtRiskAreOrderedOrdinallyAndAComparerThatThrowsMakesThemUncertainApart'),
    ('WorkspacePilotEditHostTests', 'TheFallbackByDraftIsKnownAndKeepsTheUncertainOnesApart'),
    ('WorkspacePilotEditHostTests', 'TheOuterCatchOfGetFieldAnswersTheViewInErrorWhenTheCopierOfTheStateAnswersNull'),
    ('WorkspacePilotEditHostTests', 'TheReasonOfAFieldThatCannotBeReadIsInSpanish'),
    ('WorkspacePilotPrepareTests', 'TheSnapshotIsBuiltOnlyByFromWithCopiesAndTheStatusBeforeAnAttemptWhileApplying'),
    ('WorkspacePilotPrepareTests', 'PrepareDoesNotPutTheDraftInConflictNorCleanDirtyNorTakeTheSlotOfTheUpdate'),
}
done = set()
out = [lines[0]]
for l in lines[1:]:
    if not l:
        out.append(l)
        continue
    c = l.split('\t')
    if (c[0], c[1]) in EIGHT:
        assert c[4] == 'mecanica', c
        c[4] = MOTIVE
        done.add((c[0], c[1]))
    out.append('\t'.join(c))
assert done == EIGHT, EIGHT - done
data = '\n'.join(out).encode('utf-8')
open(os.path.join(HERE, 'pbt4b-copy-map-v3.tsv'), 'wb').write(data)
rows = [l.split('\t') for l in out[1:] if l]
mec = sum(1 for r in rows if r[4] == 'mecanica')
print('pbt4b-copy-map-v3.tsv sha256 %s %d bytes; filas %d; mecanicas %d; a mano %d' % (hashlib.sha256(data).hexdigest(), len(data), len(rows), mec, len(rows) - mec))
