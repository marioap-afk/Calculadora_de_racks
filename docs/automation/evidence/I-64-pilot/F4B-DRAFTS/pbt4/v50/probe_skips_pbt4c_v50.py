"""PB-T4c (v5.0), fase 2: la sonda de la lista. Pone en una copia EXPORTADA de 6adff90d solo los dos `Skip` de la lista que vio el Owner
(owner-pbt4-decisions.md, pregunta 4: PEG:155-163 y CT-PBT1B-15), sin GREEN, sin CT y sin equivalentes, para medir con la suite completa de Core
que pruebas reaccionan a los dos `Skip` por si solos. Los motivos son PROVISIONALES (los de la sonda); los literales los fija la v5.0.

Escribe solo en la copia que se le da, y nunca fuera de ella; comprueba que cada linea es el `[Fact]` medido antes de tocarla.
Uso: python -I probe_skips_pbt4c_v50.py <copia>
"""
import os
import sys

COPY = sys.argv[1]
sys.stdout.reconfigure(encoding='utf-8', newline='\n')
assert os.path.isdir(os.path.join(COPY, 'tests')), COPY
EDITS = [
    ('tests/RackCad.Tests/Workspace/WorkspacePilotEditGuardTests.cs', 155, '    [Fact]',
     'ThePortDeclaresTheSeventeenOperationsWithTheEnumsAsOrderedPrefixesAndNamesNoTypeOfTheDraftsNorOfThePilot',
     '    [Fact(Skip = "I-64 F4B PB-T4c: sonda de la v5.0 (motivo provisional): las 17 operaciones del puerto pasan a 18 (TryEditAll)")]'),
    ('tests/RackCad.Tests/PushBackSharedReferenceGuardTests.cs', 306, '        [Fact]',
     'CT_PBT1B_15_TheWritersOfNameInThePluginRunAfterTheWindowCloses',
     '        [Fact(Skip = "I-64 F4B PB-T4c: sonda de la v5.0 (motivo provisional): archivos=14 pasa a archivos=15 con RackWorkspaceEditorWindow.cs")]'),
]
for rel, line, before, method, after in EDITS:
    path = os.path.join(COPY, rel)
    raw = open(path, 'rb').read()
    crlf = b'\r\n' in raw
    text = raw.decode('utf-8-sig' if raw.startswith(b'\xef\xbb\xbf') else 'utf-8')
    lines = text.split('\r\n' if crlf else '\n')
    assert lines[line - 1] == before, (rel, line, lines[line - 1])
    assert ('public void ' + method + '(') in lines[line], (rel, line + 1, lines[line])
    lines[line - 1] = after
    out = ('\r\n' if crlf else '\n').join(lines)
    data = out.encode('utf-8')
    if raw.startswith(b'\xef\xbb\xbf'):
        data = b'\xef\xbb\xbf' + data
    open(path, 'wb').write(data)
    print('skip %s:%d %s (crlf=%s)' % (rel, line, method, crlf))
print('sonda: 2 Skip puestos')
