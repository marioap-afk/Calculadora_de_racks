"""PB-T4c (paquete v5.1), precondicion 6, paso 8: escribe el ensayo de las CT de texto sobre src/, `pbt4c-rehearsal-textct.cs`: cinco clases
temporales, una por CT (TmpPbt4cTextCt01, 07, 08, 13 y 15), cada una con un [Fact] que acaba en el Assert.Equal de sus lineas unidas por \n. Las
lineas esperadas son las de pbt4c_ct_table_v51.py, literales (las de §5.3). Lo que copian de las guardas existentes sale de los objetos git de la
base, literal:
- CT-15: G-L1 y la pureza de PushBackPanelDraftGuardTests.cs (CT-PBT4B-19, que copia CT-PBT2A-04 y CT-PBT3A-14) con sus dos archivos de Draft/ y
  la cobertura de ApplyToDraft;
- CT-13: ControllerViolations de WorkspacePilotEditGuardTests.cs (PEG:659-707), con las 18 operaciones (las del equivalente de PEG:155-163);
- CT-01, 07 y 08 se escriben desde la fila de §5.3 de cada una (lo que fija, sus sujetos y sus terminos).
Es un ensayo, no las CT del Worker: no entra en ningun commit, solo en las copias exportadas. Escribe con \n e imprime su sha256.

Uso: python -I make_rehearsal_v51.py <repo> <rev>
"""
import hashlib
import importlib.util
import os
import sys

sys.dont_write_bytecode = True
sys.stdout.reconfigure(encoding='utf-8', newline='\n')
PB4 = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
repo, rev = sys.argv[1:3]


def load(name):
    spec = importlib.util.spec_from_file_location(name, os.path.join(PB4, name + '.py'))
    mod = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(mod)
    return mod


mk = load('make_equivalents_pbt4c_v50')
ct = load('pbt4c_ct_table_v51')
CT = {c[0]: c for c in ct.CT}


def lit(s):
    return '"' + s.replace('\\', '\\\\').replace('"', '\\"') + '"'


def expected(n, indent='                '):
    lines = CT[n][2]
    return (',\n' + indent).join(lit(l) for l in lines)


def once(text, old, new):
    assert text.count(old) == 1, (old[:80], text.count(old))
    return text.replace(old, new, 1)


# ------------------------------------------------------------------ CT-15, desde PushBackPanelDraftGuardTests.cs (CT-PBT4B-19)
pdg = mk.show(repo, rev, 'tests/RackCad.Tests/PushBackPanelDraftGuardTests.cs')
start = pdg.index('        private static string Root => PilotKit.RepoRoot().FullName;\n')
mark = pdg.index('        // ------------------------------------------------------------------------------------------------- CT-19\n')
helpers = pdg[start:mark]
files_old = helpers[helpers.index('        private static readonly string[] DraftFiles =\n'):]
files_old = files_old[:files_old.index('        };\n') + len('        };\n')]
helpers = once(helpers, files_old,
               '        private static readonly string[] DraftFiles =\n        {\n'
               '            DraftFolder + "PushBackFullDraftLaunch.cs", DraftFolder + "PushBackFullDraftStatePort.cs",\n        };\n')
method = pdg[mark:]
method = method[method.index('        [Fact]\n'):]
method = method[:method.index('            Assert.Equal(\n')]
method = once(method, 'public void CT_PBT4B_19_TheThreeNewFilesOfTheFolderOfTheFullModelDoNotMutateTheResolvedRackAreProbablyPureAndCoverApply()', 'public void CT15()')
method = once(method, '@"\\bPushBackFullDraftState\\s+Apply\\s*\\("', '@"\\bPushBackDraftApplyResult\\s+ApplyToDraft\\s*\\("')
method = once(method, '"cubre Apply="', '"cubre ApplyToDraft="')
method = once(method, '// The one member of the three that edits a state is Apply', '// The one member of the two that edits a state is ApplyToDraft')
# Su original no tiene #nullable: la copia va en el mismo contexto (sin el, el compilador avisaria en el texto copiado).
CT15 = ('#nullable disable\n    public sealed class TmpPbt4cTextCt15\n    {\n' + helpers + method
        + '            Assert.Equal(string.Join("\\n", new[]\n            {\n                ' + expected('15') + ',\n            }), string.Join("\\n", log));\n'
        + '        }\n    }\n')

# ------------------------------------------------------------------ CT-13, desde WorkspacePilotEditGuardTests.cs (ControllerViolations)
peg = mk.show(repo, rev, 'tests/RackCad.Tests/Workspace/WorkspacePilotEditGuardTests.cs').split('\n')
cv = '\n'.join(mk.member(peg, r'^    private static IEnumerable<string> ControllerViolations\(string controller\)'))
cv = once(cv, 'foreach (var (name, _) in Operations)', 'foreach (var name in Operations)')
OPS = ['TryBeginEdit', 'TryCheckNow', 'TryReload', 'TryConfirmBase', 'TryKeepMine', 'TryDiscardAndReload', 'GetField', 'SetPendingText', 'CommitField',
       'CrossBoundary', 'DiscardRack', 'DiscardDocument', 'GetRacksAtRisk', 'TakeLossNotices', 'TakeCloseNotices', 'RequestUpdate', 'TakeUpdateProgress',
       'TryEditAll']
eq1 = mk.equivalents(repo, rev)[mk.E1]
for op in OPS:
    assert ('        ("%s", @"' % op) in eq1, op
assert eq1.count('        ("') - eq1.count('        ("PilotFieldState"') - eq1.count('        ("PilotBoundaryDecision"') - eq1.count('        ("PilotBoundaryKind"') >= len(OPS)
CT13 = '''    public sealed class TmpPbt4cTextCt13
    {
        private const string ControllerPath = "src/RackCad.Plugin/Workspace/WorkspaceController.cs";

        // The 18 operations of the port, in the order of the table of the equivalent of PEG:155-163.
        private static readonly string[] Operations =
        {
            ''' + ', '.join(lit(o) for o in OPS) + ''',
        };

        private static string Squash(string text) => Regex.Replace(text, @"\\s+", string.Empty);

        private static string? TryBody(string code, string signature) => Regex.IsMatch(code, signature) ? Src.BodyOf(code, signature) : null;

''' + cv + '''

        [Fact]
        public void CT13()
        {
            var log = new List<string>();
            var controller = Src.Code(ControllerPath);
            log.Add("adaptador operaciones=" + Operations.Length + " violaciones=[" + string.Join(", ", ControllerViolations(controller)) + "]");
            const string Forward = "public EditAllOutcome TryEditAll(EditIntentRequest request) => _gate.TryEditAll(request);";
            var present = controller.Contains(Forward, StringComparison.Ordinal);
            log.Add("autoprueba-sin-reenvio-de-TryEditAll="
                + (present && ControllerViolations(controller.Replace(Forward, string.Empty, StringComparison.Ordinal)).Any() ? "falla" : "pasa"));
            log.Add("autoprueba-reenvio-que-no-solo-delega="
                + (present && ControllerViolations(controller.Replace(Forward, "public EditAllOutcome TryEditAll(EditIntentRequest request) => _gate.TryEditAll(request) with { };", StringComparison.Ordinal)).Any() ? "falla" : "pasa"));
            Assert.Equal(string.Join("\\n", new[]
            {
                ''' + expected('13') + ''',
            }), string.Join("\\n", log));
        }
    }
'''

# ------------------------------------------------------------------ CT-01, 07 y 08, desde su fila de §5.3
CT01 = '''    public sealed class TmpPbt4cTextCt01
    {
        [Fact]
        public void CT01()
        {
            var log = new List<string>();
            XNamespace x = "http://schemas.microsoft.com/winfx/2006/xaml";
            var doc = XDocument.Parse(Src.Text("src/RackCad.UI/Workspace/WorkspacePanelView.xaml"));
            var buttons = doc.Descendants().Where(e => e.Name.LocalName == "Button").ToList();
            var all = buttons.Where(b => (string?)b.Attribute(x + "Name") == "BrowserFieldEditAllButton").ToList();
            var edit = buttons.FirstOrDefault(b => (string?)b.Attribute(x + "Name") == "BrowserFieldEditButton");
            var button = all.FirstOrDefault();
            var inPanel = button != null && edit != null && button.Parent == edit.Parent && button.Parent!.Name.LocalName == "WrapPanel";
            var previous = button?.ElementsBeforeSelf().LastOrDefault();
            log.Add("xaml BrowserFieldEditAllButton=" + all.Count + " en-el-panel-de-los-botones=" + inPanel
                + " tras=" + ((string?)previous?.Attribute(x + "Name") ?? "null"));
            string A(string name) => "[" + ((string?)button?.Attribute(name) ?? "null") + "]";
            log.Add("xaml Content=" + A("Content") + " AutomationProperties.Name=" + A("AutomationProperties.Name"));
            log.Add("xaml Visibility=" + A("Visibility") + " Click=" + A("Click"));
            var code = Src.Code("src/RackCad.UI/Workspace/WorkspacePanelView.xaml.cs");
            var handlers = Src.Count(code, @"\\bvoid\\s+OnBrowserFieldEditAllClick\\s*\\(");
            var body = Regex.Match(code, @"\\bvoid\\s+OnBrowserFieldEditAllClick\\s*\\([^)]*\\)\\s*=>\\s*(.*?);\\s*$", RegexOptions.Multiline);
            log.Add("code-behind OnBrowserFieldEditAllClick=" + handlers + " cuerpo=[" + (body.Success ? body.Groups[1].Value.Trim() : "null") + "]");
            Assert.Equal(string.Join("\\n", new[]
            {
                ''' + expected('01') + ''',
            }), string.Join("\\n", log));
        }
    }
'''

CT07 = '''    public sealed class TmpPbt4cTextCt07
    {
        private static readonly string[] Forbidden = { ''' + ', '.join(lit(n) for n in ct.FORBIDDEN) + ''' };

        private static readonly string[] Subjects = { ''' + ', '.join(lit(s) for s in ct.SUBJECTS) + ''' };

        private static string CodeOrEmpty(string relative) => Src.Exists(relative) ? Src.Code(relative) : string.Empty;

        private static string? TryBody(string code, string signature) => Regex.IsMatch(code, signature) ? Src.BodyOf(code, signature) : null;

        /// <summary>The text without comments of each subject; a subject that does not exist (in the RED) is the empty text.</summary>
        private static string Subject(string name)
        {
            switch (name)
            {
                case "RackWorkspaceEditorWindow.cs": return CodeOrEmpty("src/RackCad.Plugin/Views/RackWorkspaceEditorWindow.cs");
                case "FullEditorModal": return TryBody(Src.Code("src/RackCad.Plugin/Workspace/WorkspaceController.cs"), @"\\bclass\\s+FullEditorModal\\b[^{]*") ?? string.Empty;
                case "TryEditAll":
                    var host = Src.Code("src/RackCad.Application/Workspace/WorkspaceHostCore.cs");
                    return (TryBody(host, @"EditAllOutcome\\s+IWorkspacePilotEditPort\\.TryEditAll\\s*\\(") ?? string.Empty) + "\\n"
                        + (TryBody(host, @"private\\s+EditAllOutcome\\s+EditAllOfPilot\\s*\\(") ?? string.Empty);
                case "EditAll": return TryBody(Src.Code("src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs"), @"public\\s+void\\s+EditAll\\s*\\(\\s*\\)") ?? string.Empty;
                case "PushBackFullDraftLaunch.cs": return CodeOrEmpty("src/RackCad.Application/Systems/PushBack/Draft/PushBackFullDraftLaunch.cs");
                case "PushBackFullDraftStatePort.cs": return CodeOrEmpty("src/RackCad.Application/Systems/PushBack/Draft/PushBackFullDraftStatePort.cs");
                default: throw new ArgumentException(name);
            }
        }

        private static List<string> NamesIn(string text) => Forbidden.Where(n => Regex.IsMatch(text, @"\\b" + Regex.Escape(n) + @"\\b")).ToList();

        [Fact]
        public void CT07()
        {
            var log = new List<string>();
            var texts = Subjects.ToDictionary(s => s, Subject);
            foreach (var s in Subjects) log.Add("sujeto " + s + " prohibidos=[" + string.Join(", ", NamesIn(texts[s])) + "]");
            foreach (var s in Subjects)
            {
                foreach (var n in Forbidden) log.Add("autoprueba-" + n + "-en-" + s + "=" + (NamesIn(texts[s] + "\\n" + n + "();\\n").Contains(n) ? "falla" : "pasa"));
            }

            Assert.Equal(string.Join("\\n", new[]
            {
                ''' + expected('07') + ''',
            }), string.Join("\\n", log));
        }
    }
'''

CT08 = '''    public sealed class TmpPbt4cTextCt08
    {
        private static string? TryBody(string code, string signature) => Regex.IsMatch(code, signature) ? Src.BodyOf(code, signature) : null;

        private static int Occurrences(string text, string literal)
        {
            var n = 0;
            for (var i = text.IndexOf(literal, StringComparison.Ordinal); i >= 0; i = text.IndexOf(literal, i + literal.Length, StringComparison.Ordinal)) n++;
            return n;
        }

        [Fact]
        public void CT08()
        {
            var log = new List<string>();
            var wco = Src.Code("src/RackCad.Plugin/Workspace/WorkspaceController.cs");
            var modal = TryBody(wco, @"\\bclass\\s+FullEditorModal\\b[^{]*") ?? string.Empty;
            var implements = Regex.Match(wco, @"\\bclass\\s+FullEditorModal\\s*:\\s*(\\w+)");
            log.Add("modal FullEditorModal=" + Src.Count(wco, @"\\bclass\\s+FullEditorModal\\b")
                + " implementa=" + (implements.Success ? implements.Groups[1].Value : "null")
                + " using-EnterPanelModal=" + Src.Count(modal, @"using\\s*\\(\\s*_controller\\.EnterPanelModal\\s*\\(\\s*\\)\\s*\\)")
                + " _open=" + Src.Count(modal, @"\\b_open\\s*\\(")
                + " RackCad.UI=" + Occurrences(modal, "RackCad.UI") + " PushBack=" + Occurrences(modal, "PushBack"));
            var path = "src/RackCad.Plugin/Views/RackWorkspaceEditorWindow.cs";
            var opener = Src.Exists(path) ? Src.Code(path) : string.Empty;
            const string Conversion = "var window = RackPushBackSystemWindow.ForDraft((PushBackDraftEditorLaunch)launch);";
            const string Show = "AcApplication.ShowModalWindow(window);";
            log.Add("abridor conversion=[" + Conversion + "] veces=" + Occurrences(opener, Conversion));
            log.Add("abridor mostrar=[" + Show + "] veces=" + Occurrences(opener, Show) + " Owner-asignado=" + Src.Count(opener, @"\\bOwner\\s*=(?!=)")
                + " RackCad.Application.Workspace=" + Occurrences(opener, "RackCad.Application.Workspace"));
            const string Wiring = "_host.SetFullEditHost(new FullEditorModal(this, RackWorkspaceEditorWindow.Show));";
            var wiring = wco.IndexOf(Wiring, StringComparison.Ordinal);
            var update = wco.IndexOf("_host.SetPilotUpdateHost(", StringComparison.Ordinal);
            log.Add("cableado [" + Wiring + "] veces=" + Occurrences(wco, Wiring) + " tras-SetPilotUpdateHost=" + (wiring >= 0 && update >= 0 && wiring > update));
            Assert.Equal(string.Join("\\n", new[]
            {
                ''' + expected('08') + ''',
            }), string.Join("\\n", log));
        }
    }
'''

HEAD = '''#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Xunit;
using Src = RackCad.Tests.WorkspaceHostSource;

// PB-T4c (paquete v5.1), precondicion 6, paso 8: el ensayo de las CT de texto sobre src/ (CT-PBT4C-01, 07, 08, 13 y 15), con sus lineas
// esperadas literales de §5.3 (pbt4c_ct_table_v51.py). Clases temporales: no entran en ningun commit, solo en las copias exportadas. En el RED,
// CT-07 y CT-15 (ExpectRed false) pasan y CT-01, 08 y 13 (ExpectRed true) caen por su Assert.Equal; en el GREEN, las cinco pasan.
// Generado por make_rehearsal_v51.py desde los objetos git de la base.
namespace RackCad.Tests
{
'''
text = HEAD + '\n'.join([CT01, CT07, CT08, CT13, CT15]) + '}\n'
assert '\r' not in text
out = os.path.join(PB4, 'pbt4c-rehearsal-textct.cs')
open(out, 'wb').write(text.encode('utf-8'))
print(hashlib.sha256(text.encode('utf-8')).hexdigest(), 'pbt4c-rehearsal-textct.cs', text.count('\n'), 'lineas')
