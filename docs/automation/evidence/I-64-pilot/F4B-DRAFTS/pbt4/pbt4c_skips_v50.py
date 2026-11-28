"""PB-T4c (paquete v5.0): los cuatro `Skip` literales de la lista de 4 (`pbt4c-substitution-list-4.tsv`; owner-pbt4-decisions.md, pregunta 4,
owner-pbt4c-list-3-decision.md y owner-pbt4c-list-4-decision.md), cada uno en la linea de atributo `[Fact]` de su prueba en 6adff90d, y nada mas
en esos cuatro archivos (tres de Core y uno de UI).

Modulo: SKIPS (ruta, linea del atributo, metodo, sangria, motivo, equivalente), skip_line(s) (la linea con su Skip), apply(text, path) (el
texto con \n y su Skip). Los motivos son ASCII.
"""
SKIPS = [
    ('tests/RackCad.Tests/Workspace/WorkspacePilotEditGuardTests.cs', 155,
     'ThePortDeclaresTheSeventeenOperationsWithTheEnumsAsOrderedPrefixesAndNamesNoTypeOfTheDraftsNorOfThePilot', '    ',
     'I-64 F4B PB-T4c: sustituida por su equivalente en PushBackEditAllPortEquivalenceGuardTests (registro declarado '
     'superseding-characterization.md; decision del Owner owner-pbt4-decisions.md, pregunta 4).',
     'PushBackEditAllPortEquivalenceGuardTests'),
    ('tests/RackCad.Tests/PushBackSharedReferenceGuardTests.cs', 306,
     'CT_PBT1B_15_TheWritersOfNameInThePluginRunAfterTheWindowCloses', '        ',
     'I-64 F4B PB-T4c: sustituida por su equivalente en PushBackEditAllNameWritersGuardTests (registro declarado '
     'superseding-characterization.md; decision del Owner owner-pbt4-decisions.md, pregunta 4).',
     'PushBackEditAllNameWritersGuardTests'),
    ('tests/RackCad.Tests/WorkspaceFullCopyGuardTests.cs', 568,
     'CT_PBT4B_20_TheScriptOfSkipAndDuplicateGivesTheCopiesTheKitAndTheSkipsOfTheClosedList', '        ',
     'I-64 F4B PB-T4c: sustituida por su equivalente en PushBackEditAllFullCopyGuardTests (registro declarado '
     'superseding-characterization.md; decision del Owner owner-pbt4c-list-3-decision.md).',
     'PushBackEditAllFullCopyGuardTests'),
    ('tests/RackCad.UI.Tests/PushBackWorkspaceApplyCloseTests.cs', 686,
     'CT_PBT2B_06_ClosingAsksOnlyForWhatWasNotApplied', '        ',
     'I-64 F4B PB-T4c: sustituida por su equivalente en PushBackEditAllApplyCloseTests (registro declarado '
     'superseding-characterization.md; decision del Owner owner-pbt4c-list-4-decision.md).',
     'PushBackEditAllApplyCloseTests'),
]

for _s in SKIPS:
    assert all(ord(c) < 128 for c in _s[4]), _s[0]


def skip_line(s):
    return s[3] + '[Fact(Skip = "' + s[4] + '")]'


def apply(text, path):
    """El texto (con \\n) de una de las tres rutas con su Skip en su linea de atributo, comprobada antes."""
    s = next(x for x in SKIPS if x[0] == path)
    lines = text.split('\n')
    assert lines[s[1] - 1] == s[3] + '[Fact]', (path, s[1], lines[s[1] - 1])
    assert ('public void ' + s[2] + '(') in lines[s[1]], (path, s[1] + 1)
    lines[s[1] - 1] = skip_line(s)
    return '\n'.join(lines)
