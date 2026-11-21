"""PB-T4c (paquete v5.0): la lista exacta de PB-T4c, `pbt4c-substitution-list-3.tsv`, medida por objetos git en el cierre de PB-T4b (6adff90d).
Es la lista que el Owner tomo como la que vio: las 2 de `owner-pbt4-decisions.md` (pregunta 4), las dos filas «PB-T4c» de
`pbt4-substitution-lists.tsv` (que quedan como historia), y CT-PBT4B-20, de `owner-pbt4c-list-3-decision.md`. Cada fila: la lista, el archivo,
el metodo, sus lineas (de su linea de atributo a su llave de cierre; el atributo es `[Fact]`) y la causa. UTF-8 con LF.

Uso: python -I make_list_pbt4c_3.py <repo> <salida.tsv>
"""
import re
import subprocess
import sys

REPO, OUT = sys.argv[1], sys.argv[2]
sys.stdout.reconfigure(encoding='utf-8', newline='\n')
REV = '6adff90d'
ROWS = [
    ('tests/RackCad.Tests/Workspace/WorkspacePilotEditGuardTests.cs',
     'ThePortDeclaresTheSeventeenOperationsWithTheEnumsAsOrderedPrefixesAndNamesNoTypeOfTheDraftsNorOfThePilot',
     'C-5: PEG:155-163, las 17 operaciones del puerto pasan a 18 (TryEditAll); owner-pbt4-decisions.md, pregunta 4'),
    ('tests/RackCad.Tests/PushBackSharedReferenceGuardTests.cs',
     'CT_PBT1B_15_TheWritersOfNameInThePluginRunAfterTheWindowCloses',
     'C-5: CT-PBT1B-15, archivos=14 pasa a archivos=15 con RackWorkspaceEditorWindow.cs; owner-pbt4-decisions.md, pregunta 4'),
    ('tests/RackCad.Tests/WorkspaceFullCopyGuardTests.cs',
     'CT_PBT4B_20_TheScriptOfSkipAndDuplicateGivesTheCopiesTheKitAndTheSkipsOfTheClosedList',
     'C-5: CT-PBT4B-20 (1), el Skip de PB-T4c en PEG:155 deja PEG distinto de su BaseSha; owner-pbt4c-list-3-decision.md'),
]


def member_range(lines, name):
    i = next(k for k, l in enumerate(lines) if re.search(r'\bpublic void ' + re.escape(name) + r'\(', l))
    a = i
    while a > 0 and lines[a - 1].strip().startswith('['):
        a -= 1
    depth, seen = 0, False
    for k in range(i, len(lines)):
        code = re.sub(r'"(?:[^"\\]|\\.)*"', '""', re.sub(r'//.*', '', lines[k]))
        for ch in code:
            if ch == '{':
                depth, seen = depth + 1, True
            elif ch == '}':
                depth -= 1
                if seen and depth == 0:
                    return a + 1, k + 1, lines[a].strip()
    raise SystemExit('sin cierre: ' + name)


out = ['lista\tarchivo\tmetodo\tlineas\tcausa']
for path, method, cause in ROWS:
    text = subprocess.run(['git', '-C', REPO, 'show', REV + ':' + path], capture_output=True, check=True).stdout.decode('utf-8-sig').replace('\r\n', '\n')
    a, e, attr = member_range(text.split('\n'), method)
    assert attr == '[Fact]', (path, attr)
    out.append('PB-T4c C-5\t%s\t%s\t%d-%d\t%s' % (path, method, a, e, cause))
    print('%s:%d-%d %s' % (path, a, e, method))
with open(OUT, 'w', encoding='utf-8', newline='\n') as f:
    f.write('\n'.join(out) + '\n')
print('filas=%d' % (len(out) - 1))
