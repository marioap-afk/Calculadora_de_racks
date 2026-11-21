"""PB-T4c (v5.0), fase 2: las filas CANDIDATAS de la lista de PB-T4c sobre el cierre de PB-T4b (6adff90d), por objetos git. No es la lista
emitible: la tercera fila (CT-PBT4B-20) esta fuera de la que vio el Owner (owner-pbt4-decisions.md, pregunta 4) y espera su decision. Las dos
primeras son las de `pbt4-substitution-lists.tsv` (sha256 f24c2dbc...), con su rango vuelto a medir.

Escribe solo la salida que se le da, en UTF-8 con LF. Uso: python -I candidate_list_pbt4c_v50.py <repo> <salida.tsv>
"""
import re
import subprocess
import sys

REPO, OUT = sys.argv[1], sys.argv[2]
sys.stdout.reconfigure(encoding='utf-8', newline='\n')
REV = '6adff90d'
ROWS = [
    ('PB-T4c C-5', 'tests/RackCad.Tests/Workspace/WorkspacePilotEditGuardTests.cs',
     'ThePortDeclaresTheSeventeenOperationsWithTheEnumsAsOrderedPrefixesAndNamesNoTypeOfTheDraftsNorOfThePilot',
     'C-5: PEG:155-163, las 17 operaciones del puerto pasan a 18 (TryEditAll); en la lista que vio el Owner'),
    ('PB-T4c C-5', 'tests/RackCad.Tests/PushBackSharedReferenceGuardTests.cs',
     'CT_PBT1B_15_TheWritersOfNameInThePluginRunAfterTheWindowCloses',
     'C-5: CT-PBT1B-15, archivos=14 pasa a archivos=15 con RackWorkspaceEditorWindow.cs; en la lista que vio el Owner'),
    ('PB-T4c C-5 (fuera de la lista vista)', 'tests/RackCad.Tests/WorkspaceFullCopyGuardTests.cs',
     'CT_PBT4B_20_TheScriptOfSkipAndDuplicateGivesTheCopiesTheKitAndTheSkipsOfTheClosedList',
     'C-5: CT-PBT4B-20 (1), PEG es uno de sus 21 originales (BaseSha 1ef3b59b..., skip=5) y SkipLine solo devuelve los Skip de PB-T4b: '
     'con el Skip de PB-T4c en PEG:155, original WorkspacePilotEditGuard igual-a-la-base=False; FUERA de la lista que vio el Owner'),
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
for lista, path, method, cause in ROWS:
    text = subprocess.run(['git', '-C', REPO, 'show', REV + ':' + path], capture_output=True, check=True).stdout.decode('utf-8-sig').replace('\r\n', '\n')
    a, e, attr = member_range(text.split('\n'), method)
    assert attr == '[Fact]', (path, attr)
    out.append('%s\t%s\t%s\t%d-%d\t%s' % (lista, path, method, a, e, cause))
    print('%s:%d-%d %s %s' % (path, a, e, attr, method))
with open(OUT, 'w', encoding='utf-8', newline='\n') as f:
    f.write('\n'.join(out) + '\n')
print('filas=%d' % (len(out) - 1))
