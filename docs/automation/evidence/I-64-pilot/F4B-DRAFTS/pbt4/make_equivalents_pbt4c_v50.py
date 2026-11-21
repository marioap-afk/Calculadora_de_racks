"""PB-T4c (paquete v5.0): los cuatro equivalentes de la lista de 4, generados de sus originales en 6adff90d por objetos git (`git show`), con un
cambio declarado cada uno y nada mas. Trabaja con los saltos de linea a \n. El cuarto (owner-pbt4c-list-4-decision.md):
- PushBackEditAllApplyCloseTests (UI; equivalente de CT-PBT2B-06): la prueba y los miembros de PushBackWorkspaceApplyCloseTests que alcanza
  (Sesion, Nueva y Cierre), literales, con las usings y el atributo de su coleccion; solo cambia la linea esperada de la ventana 4, a la pregunta
  nueva. Falla en el RED (la ventana del esqueleto no cambia) y pasa en el GREEN: su filtro va con ExpectRed true.

- PushBackEditAllPortEquivalenceGuardTests (equivalente de PEG:155-163): la prueba y los miembros de PEG que alcanza (PortPath, Port, TryBody,
  Operations, DraftOrPilotNames, OrderedEnums, EnumMembers y PortViolations), literales, con UNA fila mas en Operations: TryEditAll, con su
  cuerpo por defecto, al final (18 operaciones).
- PushBackEditAllNameWritersGuardTests (equivalente de CT-PBT1B-15): la prueba y los miembros de PushBackSharedReferenceGuardTests que alcanza
  (Clean, Collapse, Root, Inject, NameWriter, SourceFiles, NameStatement, Assignments, MethodText, WriterInsideDrawPushBackView y OrderOf),
  literales, con «archivos=14» -> «archivos=15» (los 14 y el abridor); lo demas, igual.
- PushBackEditAllFullCopyGuardTests (equivalente de CT-PBT4B-20; owner-pbt4c-list-3-decision.md): WorkspaceFullCopyGuardTests.cs entero, con
  la clase renombrada y Restore, que ademas devuelve a [Fact] la linea exacta del Skip de PB-T4c de PEG:155; sus lineas esperadas no cambian.

Modulo: equivalents(repo, rev) -> {ruta: texto}. Uso: python -I make_equivalents_pbt4c_v50.py <repo> <rev> <salida.txt> [<dir-copia>]
(con <dir-copia>, escribe ademas los tres archivos en la copia, con \n; nunca fuera de ella).
"""
import hashlib
import importlib.util
import os
import re
import subprocess
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
spec = importlib.util.spec_from_file_location('pbt4c_skips_v50', os.path.join(HERE, 'pbt4c_skips_v50.py'))
sk = importlib.util.module_from_spec(spec)
spec.loader.exec_module(sk)

PEG = 'tests/RackCad.Tests/Workspace/WorkspacePilotEditGuardTests.cs'
SRG = 'tests/RackCad.Tests/PushBackSharedReferenceGuardTests.cs'
FCG = 'tests/RackCad.Tests/WorkspaceFullCopyGuardTests.cs'
WAC = 'tests/RackCad.UI.Tests/PushBackWorkspaceApplyCloseTests.cs'
E1 = 'tests/RackCad.Tests/PushBackEditAllPortEquivalenceGuardTests.cs'
E2 = 'tests/RackCad.Tests/PushBackEditAllNameWritersGuardTests.cs'
E3 = 'tests/RackCad.Tests/PushBackEditAllFullCopyGuardTests.cs'
E4 = 'tests/RackCad.UI.Tests/PushBackEditAllApplyCloseTests.cs'
PATHS = [E1, E2, E3, E4]


def show(repo, rev, path):
    raw = subprocess.run(['git', '-C', repo, 'show', rev + ':' + path], capture_output=True, check=True).stdout
    return raw.decode('utf-8-sig').replace('\r\n', '\n')


def _code(line):
    """La linea sin literales de cadena ni de caracter ni comentario de linea, para contar llaves."""
    out, i, n = [], 0, len(line)
    while i < n:
        c = line[i]
        if c == '/' and i + 1 < n and line[i + 1] == '/':
            break
        if c == '@' and i + 1 < n and line[i + 1] == '"':
            i += 2
            while i < n:
                if line[i] == '"' and i + 1 < n and line[i + 1] == '"':
                    i += 2
                elif line[i] == '"':
                    i += 1
                    break
                else:
                    i += 1
            continue
        if c == '"':
            i += 1
            while i < n and line[i] != '"':
                i += 2 if line[i] == '\\' else 1
            i += 1
            continue
        if c == "'":
            j = line.find("'", i + 2 if i + 1 < n and line[i + 1] == '\\' else i + 1)
            i = (j + 1) if j > 0 else n
            continue
        out.append(c)
        i += 1
    return ''.join(out)


def member(lines, start_rx):
    """Las lineas de un miembro: sus lineas de /// y de atributo de encima y, desde su firma, hasta la llave que cierra la primera que abre o,
    sin llaves, hasta el primer ; a nivel cero."""
    hits = [i for i, l in enumerate(lines) if re.search(start_rx, l)]
    assert len(hits) == 1, (start_rx, len(hits))
    i = hits[0]
    a = i
    while a > 0 and (lines[a - 1].strip().startswith('///') or lines[a - 1].strip().startswith('[')):
        a -= 1
    depth, seen = 0, False
    for k in range(i, len(lines)):
        code = _code(lines[k])
        for ch in code:
            if ch == '{':
                depth += 1
                seen = True
            elif ch == '}':
                depth -= 1
        if seen and depth == 0:
            return lines[a:k + 1]
        if not seen and code.rstrip().endswith(';') and depth == 0:
            return lines[a:k + 1]
    raise AssertionError(('sin fin', start_rx))


def once(text, old, new):
    assert text.count(old) == 1, ('no esta exactamente una vez', old[:100], text.count(old))
    return text.replace(old, new, 1)


PEG_MEMBERS = [
    r'^    private const string PortPath = ',
    r'^    private static string Port\(\) => ',
    r'^    private static string\? TryBody\(string code, string signature\)',
    r'^    private static readonly \(string Name, string Interface\)\[\] Operations =',
    r'^    private static readonly Regex DraftOrPilotNames = ',
    r'^    private static readonly \(string Enum, string\[\] Prefix\)\[\] OrderedEnums =',
    r'^    private static IReadOnlyList<string>\? EnumMembers\(string code, string name\)',
    r'^    private static IEnumerable<string> PortViolations\(string port\)',
    r'^    public void ThePortDeclaresTheSeventeenOperationsWithTheEnumsAsOrderedPrefixesAndNamesNoTypeOfTheDraftsNorOfThePilot\(\)',
]
TRY_EDIT_ALL_ROW_OLD = ('        ("TakeUpdateProgress", @"EditUpdateProgress\\s+TakeUpdateProgress\\s*\\(\\s*WorkspaceRequest\\s+\\w+\\s*,\\s*string\\s+\\w+\\s*\\)'
                        '\\s*;"),\n')
TRY_EDIT_ALL_ROW_NEW = (TRY_EDIT_ALL_ROW_OLD
                        + '\n'
                        + '        // I-64 F4B PB-T4c (D-PBT4-13; owner-pbt4-decisions.md, pregunta 4): "Editar todo", with its default body, at the end of the port.\n'
                        + '        ("TryEditAll", @"EditAllOutcome\\s+TryEditAll\\s*\\(\\s*EditIntentRequest\\s+\\w+\\s*\\)\\s*=>\\s*EditAllOutcome\\.NotAvailable\\s*\\(\\s*null\\s*\\)'
                        '\\s*;"),\n')

E1_HEAD = '''#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RackCad.Application.Workspace;
using Xunit;
using Src = RackCad.Tests.WorkspaceHostSource;

namespace RackCad.Tests;

// I-64 F4B PB-T4c (owner-pbt4-decisions.md, pregunta 4; D-PBT4-13): the equivalent of WorkspacePilotEditGuardTests:155-163, whose Skip is in the
// closed list of PB-T4c. It is that test with the members of its file that it reaches, literal, and ONE more row in the table of the operations:
// TryEditAll, with its default body, at the end of the port, so the port declares eighteen operations and the reflection sees the eighteen.
public class PushBackEditAllPortEquivalenceGuardTests
{
'''

SRG_MEMBERS = [
    r'^        private static string Clean\(string source\)',
    r'^        private static string Collapse\(string text\)',
    r'^        private static string Root => PilotKit\.RepoRoot\(\)\.FullName;',
    r'^        private static string Inject\(string source, string line\)',
    r'^        private const string NameWriter = ',
    r'^        private static IEnumerable<string> SourceFiles\(string folder\)',
    r'^        private const string NameStatement = ',
    r'^        private static List<\(string File, string Text\)> Assignments\(',
    r'^        private static string MethodText\(string text, string signature\)',
    r'^        private static bool WriterInsideDrawPushBackView\(string commands\)',
    r'^        private static bool OrderOf\(string text, string method, string modal, string writer\)',
    r'^        public void CT_PBT1B_15_TheWritersOfNameInThePluginRunAfterTheWindowCloses\(\)',
]

E2_HEAD = '''using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-64 F4B PB-T4c (owner-pbt4-decisions.md, pregunta 4; D-PBT4-15): the equivalent of CT-PBT1B-15 of PushBackSharedReferenceGuardTests, whose
    /// Skip is in the closed list of PB-T4c. It is that test with the members of its file that it reaches, literal, with one change: the census of
    /// the files of the Plugin that name PushBack has 15, the 14 of before and the opener of the full editor (RackWorkspaceEditorWindow.cs),
    /// which writes no Name; the seven assignments, the order and the self-tests do not change.
    /// </summary>
    public sealed class PushBackEditAllNameWritersGuardTests
    {
'''

FCG_CLASS_OLD = '    public sealed class WorkspaceFullCopyGuardTests\n'
FCG_CLASS_NEW = '    public sealed class PushBackEditAllFullCopyGuardTests\n'
FCG_RESTORE_OLD = '        private static string Restore(string text) => SkipLine.Replace(text, "$1[$2]");\n'


def fcg_restore_new():
    peg = next(s for s in sk.SKIPS if s[0] == PEG)
    literal = sk.skip_line(peg).replace('\\', '\\\\').replace('"', '\\"')
    return ('        // I-64 F4B PB-T4c (owner-pbt4c-list-3-decision.md): the one Skip of PB-T4c in an original, the one of WorkspacePilotEditGuardTests:155,\n'
            '        // goes back to its [Fact] too, by its exact line; the count of the Skip of PB-T4b and the expected lines do not change.\n'
            '        private const string EditAllSkipLine = "' + literal + '";\n'
            '\n'
            '        private static string Restore(string text) => SkipLine.Replace(text, "$1[$2]").Replace(EditAllSkipLine + "\\n", "    [Fact]\\n");\n')


WAC_MEMBERS = [
    r'^        private sealed class Sesion$',
    r'^        private static Sesion Nueva\(List<string> log, PushBackEditableState estado, RackProject proyecto = null\)',
    r'^        private static bool Cierre\(List<string> log, PromptProbe avisos, Action<Sesion> gestos\)',
    r'^        public void CT_PBT2B_06_ClosingAsksOnlyForWhatWasNotApplied\(\)',
]
WAC_WINDOW4_OLD = '                "cerrar-con-modulo avisos=1 pregunta=" + PreguntaModulo + " cerrada=True",\n'
WAC_WINDOW4_NEW = ('                "cerrar-con-modulo avisos=1 pregunta=Hay un cambio de módulo sin confirmar y cambios sin aplicar al borrador; se perderán los '
                   'dos al cerrar. ¿Deseas continuar? cerrada=True",\n')
WAC_CLASS_LINES = ['    [Collection(EditorDiscardPromptCollection.Name)]', '    public sealed class PushBackWorkspaceApplyCloseTests']
E4_DOC = '''    /// <summary>
    /// I-64 F4B PB-T4c (owner-pbt4c-list-4-decision.md; D-PBT4-17, A-T2b-OWNER-01): the equivalent of CT-PBT2B-06 of
    /// PushBackWorkspaceApplyCloseTests, whose Skip is in the closed list of PB-T4c. It is that test with the members of its file that it
    /// reaches (Sesion, Nueva and Cierre), literal, with one change: the expected line of window 4, where a staged module and unapplied changes
    /// are pending together, is the one question that names both (WorkspaceStagedAndUnappliedQuestion of the window).
    /// </summary>
'''


def equivalent_e4(repo, rev):
    """La copia literal de CT-PBT2B-06 en su clase nueva: las usings, el espacio y el atributo de la coleccion del original, y los miembros
    que la prueba alcanza, en su orden; solo cambia la linea esperada de la ventana 4."""
    lines = show(repo, rev, WAC).split('\n')
    i = lines.index(WAC_CLASS_LINES[0])
    assert lines[i + 1] == WAC_CLASS_LINES[1], lines[i + 1]
    head_end = next(k for k in range(i) if lines[k].startswith('    /// <summary>'))
    head = '\n'.join(lines[:head_end]) + '\n'
    assert head.endswith('namespace RackCad.UI.Tests\n{\n'), head[-60:]
    parts = ['\n'.join(member(lines, rx)) for rx in WAC_MEMBERS]
    body = '\n\n'.join(parts) + '\n'
    body = once(body, WAC_WINDOW4_OLD, WAC_WINDOW4_NEW)
    return (head + E4_DOC + WAC_CLASS_LINES[0] + '\n' + '    public sealed class PushBackEditAllApplyCloseTests\n    {\n'
            + body + '    }\n}\n')


def equivalents(repo, rev):
    peg = show(repo, rev, PEG).split('\n')
    e1 = E1_HEAD
    parts = []
    for rx in PEG_MEMBERS:
        parts.append('\n'.join(member(peg, rx)))
    body = '\n\n'.join(parts) + '\n'
    body = once(body, TRY_EDIT_ALL_ROW_OLD, TRY_EDIT_ALL_ROW_NEW)
    e1 += body + '}\n'

    srg = show(repo, rev, SRG).split('\n')
    parts = ['\n'.join(member(srg, rx)) for rx in SRG_MEMBERS]
    body = '\n\n'.join(parts) + '\n'
    body = once(body, '                    "archivos=14",\n', '                    "archivos=15",\n')
    e2 = E2_HEAD + body + '    }\n}\n'

    fcg = show(repo, rev, FCG)
    e3 = once(fcg, FCG_CLASS_OLD, FCG_CLASS_NEW)
    e3 = once(e3, FCG_RESTORE_OLD, fcg_restore_new())
    return {E1: e1, E2: e2, E3: e3, E4: equivalent_e4(repo, rev)}


def sha(text):
    return hashlib.sha256(text.encode('utf-8')).hexdigest()


if __name__ == '__main__':
    sys.stdout.reconfigure(encoding='utf-8', newline='\n')
    repo, rev, out = sys.argv[1:4]
    copy = sys.argv[4] if len(sys.argv) > 4 else None
    eq = equivalents(repo, rev)
    lines = ['# PB-T4c v5.0: los equivalentes sobre %s (sha256 del texto con \\n, EquivalentSha)' % rev]
    for p in PATHS:
        lines.append('%s %s lineas=%d' % (sha(eq[p]), p, eq[p].count('\n')))
        if copy:
            target = os.path.join(copy, p)
            assert os.path.isdir(os.path.dirname(target)) and not os.path.exists(target), target
            with open(target, 'w', encoding='utf-8', newline='\n') as f:
                f.write(eq[p])
    with open(out, 'w', encoding='utf-8', newline='\n') as f:
        f.write('\n'.join(lines) + '\n')
    print('equivalentes=%d' % len(eq))
