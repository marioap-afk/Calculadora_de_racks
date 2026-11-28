#nullable enable
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
    public sealed class TmpPbt4cTextCt01
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
            var handlers = Src.Count(code, @"\bvoid\s+OnBrowserFieldEditAllClick\s*\(");
            var body = Regex.Match(code, @"\bvoid\s+OnBrowserFieldEditAllClick\s*\([^)]*\)\s*=>\s*(.*?);\s*$", RegexOptions.Multiline);
            log.Add("code-behind OnBrowserFieldEditAllClick=" + handlers + " cuerpo=[" + (body.Success ? body.Groups[1].Value.Trim() : "null") + "]");
            Assert.Equal(string.Join("\n", new[]
            {
                "xaml BrowserFieldEditAllButton=1 en-el-panel-de-los-botones=True tras=BrowserFieldEditButton",
                "xaml Content=[Editar todo] AutomationProperties.Name=[Editar el rack completo en el editor]",
                "xaml Visibility=[{Binding Browser.Field.ShowEditAllButton, Converter={StaticResource FlagToVisibility}}] Click=[OnBrowserFieldEditAllClick]",
                "code-behind OnBrowserFieldEditAllClick=1 cuerpo=[Contained(() => _viewModel.Browser.Field.EditAll())]",
            }), string.Join("\n", log));
        }
    }

    public sealed class TmpPbt4cTextCt07
    {
        private static readonly string[] Forbidden = { "RackPushBackCommands", "RackMenuCommands", "EditorModules", "RackViewBatchExecution", "DrawAndPlace", "RedrawInPlace", "InsertRequested", "InsertionRequest" };

        private static readonly string[] Subjects = { "RackWorkspaceEditorWindow.cs", "FullEditorModal", "TryEditAll", "EditAll", "PushBackFullDraftLaunch.cs", "PushBackFullDraftStatePort.cs" };

        private static string CodeOrEmpty(string relative) => Src.Exists(relative) ? Src.Code(relative) : string.Empty;

        private static string? TryBody(string code, string signature) => Regex.IsMatch(code, signature) ? Src.BodyOf(code, signature) : null;

        /// <summary>The text without comments of each subject; a subject that does not exist (in the RED) is the empty text.</summary>
        private static string Subject(string name)
        {
            switch (name)
            {
                case "RackWorkspaceEditorWindow.cs": return CodeOrEmpty("src/RackCad.Plugin/Views/RackWorkspaceEditorWindow.cs");
                case "FullEditorModal": return TryBody(Src.Code("src/RackCad.Plugin/Workspace/WorkspaceController.cs"), @"\bclass\s+FullEditorModal\b[^{]*") ?? string.Empty;
                case "TryEditAll":
                    var host = Src.Code("src/RackCad.Application/Workspace/WorkspaceHostCore.cs");
                    return (TryBody(host, @"EditAllOutcome\s+IWorkspacePilotEditPort\.TryEditAll\s*\(") ?? string.Empty) + "\n"
                        + (TryBody(host, @"private\s+EditAllOutcome\s+EditAllOfPilot\s*\(") ?? string.Empty);
                case "EditAll": return TryBody(Src.Code("src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs"), @"public\s+void\s+EditAll\s*\(\s*\)") ?? string.Empty;
                case "PushBackFullDraftLaunch.cs": return CodeOrEmpty("src/RackCad.Application/Systems/PushBack/Draft/PushBackFullDraftLaunch.cs");
                case "PushBackFullDraftStatePort.cs": return CodeOrEmpty("src/RackCad.Application/Systems/PushBack/Draft/PushBackFullDraftStatePort.cs");
                default: throw new ArgumentException(name);
            }
        }

        private static List<string> NamesIn(string text) => Forbidden.Where(n => Regex.IsMatch(text, @"\b" + Regex.Escape(n) + @"\b")).ToList();

        [Fact]
        public void CT07()
        {
            var log = new List<string>();
            var texts = Subjects.ToDictionary(s => s, Subject);
            foreach (var s in Subjects) log.Add("sujeto " + s + " prohibidos=[" + string.Join(", ", NamesIn(texts[s])) + "]");
            foreach (var s in Subjects)
            {
                foreach (var n in Forbidden) log.Add("autoprueba-" + n + "-en-" + s + "=" + (NamesIn(texts[s] + "\n" + n + "();\n").Contains(n) ? "falla" : "pasa"));
            }

            Assert.Equal(string.Join("\n", new[]
            {
                "sujeto RackWorkspaceEditorWindow.cs prohibidos=[]",
                "sujeto FullEditorModal prohibidos=[]",
                "sujeto TryEditAll prohibidos=[]",
                "sujeto EditAll prohibidos=[]",
                "sujeto PushBackFullDraftLaunch.cs prohibidos=[]",
                "sujeto PushBackFullDraftStatePort.cs prohibidos=[]",
                "autoprueba-RackPushBackCommands-en-RackWorkspaceEditorWindow.cs=falla",
                "autoprueba-RackMenuCommands-en-RackWorkspaceEditorWindow.cs=falla",
                "autoprueba-EditorModules-en-RackWorkspaceEditorWindow.cs=falla",
                "autoprueba-RackViewBatchExecution-en-RackWorkspaceEditorWindow.cs=falla",
                "autoprueba-DrawAndPlace-en-RackWorkspaceEditorWindow.cs=falla",
                "autoprueba-RedrawInPlace-en-RackWorkspaceEditorWindow.cs=falla",
                "autoprueba-InsertRequested-en-RackWorkspaceEditorWindow.cs=falla",
                "autoprueba-InsertionRequest-en-RackWorkspaceEditorWindow.cs=falla",
                "autoprueba-RackPushBackCommands-en-FullEditorModal=falla",
                "autoprueba-RackMenuCommands-en-FullEditorModal=falla",
                "autoprueba-EditorModules-en-FullEditorModal=falla",
                "autoprueba-RackViewBatchExecution-en-FullEditorModal=falla",
                "autoprueba-DrawAndPlace-en-FullEditorModal=falla",
                "autoprueba-RedrawInPlace-en-FullEditorModal=falla",
                "autoprueba-InsertRequested-en-FullEditorModal=falla",
                "autoprueba-InsertionRequest-en-FullEditorModal=falla",
                "autoprueba-RackPushBackCommands-en-TryEditAll=falla",
                "autoprueba-RackMenuCommands-en-TryEditAll=falla",
                "autoprueba-EditorModules-en-TryEditAll=falla",
                "autoprueba-RackViewBatchExecution-en-TryEditAll=falla",
                "autoprueba-DrawAndPlace-en-TryEditAll=falla",
                "autoprueba-RedrawInPlace-en-TryEditAll=falla",
                "autoprueba-InsertRequested-en-TryEditAll=falla",
                "autoprueba-InsertionRequest-en-TryEditAll=falla",
                "autoprueba-RackPushBackCommands-en-EditAll=falla",
                "autoprueba-RackMenuCommands-en-EditAll=falla",
                "autoprueba-EditorModules-en-EditAll=falla",
                "autoprueba-RackViewBatchExecution-en-EditAll=falla",
                "autoprueba-DrawAndPlace-en-EditAll=falla",
                "autoprueba-RedrawInPlace-en-EditAll=falla",
                "autoprueba-InsertRequested-en-EditAll=falla",
                "autoprueba-InsertionRequest-en-EditAll=falla",
                "autoprueba-RackPushBackCommands-en-PushBackFullDraftLaunch.cs=falla",
                "autoprueba-RackMenuCommands-en-PushBackFullDraftLaunch.cs=falla",
                "autoprueba-EditorModules-en-PushBackFullDraftLaunch.cs=falla",
                "autoprueba-RackViewBatchExecution-en-PushBackFullDraftLaunch.cs=falla",
                "autoprueba-DrawAndPlace-en-PushBackFullDraftLaunch.cs=falla",
                "autoprueba-RedrawInPlace-en-PushBackFullDraftLaunch.cs=falla",
                "autoprueba-InsertRequested-en-PushBackFullDraftLaunch.cs=falla",
                "autoprueba-InsertionRequest-en-PushBackFullDraftLaunch.cs=falla",
                "autoprueba-RackPushBackCommands-en-PushBackFullDraftStatePort.cs=falla",
                "autoprueba-RackMenuCommands-en-PushBackFullDraftStatePort.cs=falla",
                "autoprueba-EditorModules-en-PushBackFullDraftStatePort.cs=falla",
                "autoprueba-RackViewBatchExecution-en-PushBackFullDraftStatePort.cs=falla",
                "autoprueba-DrawAndPlace-en-PushBackFullDraftStatePort.cs=falla",
                "autoprueba-RedrawInPlace-en-PushBackFullDraftStatePort.cs=falla",
                "autoprueba-InsertRequested-en-PushBackFullDraftStatePort.cs=falla",
                "autoprueba-InsertionRequest-en-PushBackFullDraftStatePort.cs=falla",
            }), string.Join("\n", log));
        }
    }

    public sealed class TmpPbt4cTextCt08
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
            var modal = TryBody(wco, @"\bclass\s+FullEditorModal\b[^{]*") ?? string.Empty;
            var implements = Regex.Match(wco, @"\bclass\s+FullEditorModal\s*:\s*(\w+)");
            log.Add("modal FullEditorModal=" + Src.Count(wco, @"\bclass\s+FullEditorModal\b")
                + " implementa=" + (implements.Success ? implements.Groups[1].Value : "null")
                + " using-EnterPanelModal=" + Src.Count(modal, @"using\s*\(\s*_controller\.EnterPanelModal\s*\(\s*\)\s*\)")
                + " _open=" + Src.Count(modal, @"\b_open\s*\(")
                + " RackCad.UI=" + Occurrences(modal, "RackCad.UI") + " PushBack=" + Occurrences(modal, "PushBack"));
            var path = "src/RackCad.Plugin/Views/RackWorkspaceEditorWindow.cs";
            var opener = Src.Exists(path) ? Src.Code(path) : string.Empty;
            const string Conversion = "var window = RackPushBackSystemWindow.ForDraft((PushBackDraftEditorLaunch)launch);";
            const string Show = "AcApplication.ShowModalWindow(window);";
            log.Add("abridor conversion=[" + Conversion + "] veces=" + Occurrences(opener, Conversion));
            log.Add("abridor mostrar=[" + Show + "] veces=" + Occurrences(opener, Show) + " Owner-asignado=" + Src.Count(opener, @"\bOwner\s*=(?!=)")
                + " RackCad.Application.Workspace=" + Occurrences(opener, "RackCad.Application.Workspace"));
            const string Wiring = "_host.SetFullEditHost(new FullEditorModal(this, RackWorkspaceEditorWindow.Show));";
            var wiring = wco.IndexOf(Wiring, StringComparison.Ordinal);
            var update = wco.IndexOf("_host.SetPilotUpdateHost(", StringComparison.Ordinal);
            log.Add("cableado [" + Wiring + "] veces=" + Occurrences(wco, Wiring) + " tras-SetPilotUpdateHost=" + (wiring >= 0 && update >= 0 && wiring > update));
            Assert.Equal(string.Join("\n", new[]
            {
                "modal FullEditorModal=1 implementa=IWorkspaceFullEditHost using-EnterPanelModal=1 _open=1 RackCad.UI=0 PushBack=0",
                "abridor conversion=[var window = RackPushBackSystemWindow.ForDraft((PushBackDraftEditorLaunch)launch);] veces=1",
                "abridor mostrar=[AcApplication.ShowModalWindow(window);] veces=1 Owner-asignado=0 RackCad.Application.Workspace=0",
                "cableado [_host.SetFullEditHost(new FullEditorModal(this, RackWorkspaceEditorWindow.Show));] veces=1 tras-SetPilotUpdateHost=True",
            }), string.Join("\n", log));
        }
    }

    public sealed class TmpPbt4cTextCt13
    {
        private const string ControllerPath = "src/RackCad.Plugin/Workspace/WorkspaceController.cs";

        // The 18 operations of the port, in the order of the table of the equivalent of PEG:155-163.
        private static readonly string[] Operations =
        {
            "TryBeginEdit", "TryCheckNow", "TryReload", "TryConfirmBase", "TryKeepMine", "TryDiscardAndReload", "GetField", "SetPendingText", "CommitField", "CrossBoundary", "DiscardRack", "DiscardDocument", "GetRacksAtRisk", "TakeLossNotices", "TakeCloseNotices", "RequestUpdate", "TakeUpdateProgress", "TryEditAll",
        };

        private static string Squash(string text) => Regex.Replace(text, @"\s+", string.Empty);

        private static string? TryBody(string code, string signature) => Regex.IsMatch(code, signature) ? Src.BodyOf(code, signature) : null;

    private static IEnumerable<string> ControllerViolations(string controller)
    {
        var notify = TryBody(controller, @"private\s+void\s+NotifyStateChanged\s*\(\s*\)");
        if (notify == null) yield return "NotifyStateChanged is missing";
        else
        {
            var enter = notify.IndexOf("_noticeDepth.Enter();", StringComparison.Ordinal);
            var tryAt = Regex.Match(notify, @"\btry\b").Index;
            if (enter < 0 || enter > tryAt) yield return "the depth of notices is not entered before the try";
            if (!Regex.IsMatch(Squash(notify), @"finally\{_noticeDepth\.Leave\(\);\}")) yield return "the depth of notices is not left in a finally";
            if (!notify.Contains("StateChanged?.Invoke();", StringComparison.Ordinal)) yield return "the notice is not raised inside";
            if (Regex.IsMatch(notify, @"Dispatcher|\+=|\bTimer\b|\bTask\b|\basync\b")) yield return "NotifyStateChanged schedules or subscribes";
        }

        var getter = TryBody(controller, @"public\s+IWorkspacePilotEditPort\s+PilotEdit\b");
        if (getter == null) yield return "PilotEdit is missing";
        else
        {
            var navigation = getter.IndexOf("Navigation", StringComparison.Ordinal);
            var ret = getter.IndexOf("return", StringComparison.Ordinal);
            if (navigation < 0 || ret < 0 || navigation > ret) yield return "the getter does not reach Navigation before it returns";
            if (!Regex.IsMatch(getter, @"return\s+_pilotEdit\s*;")) yield return "the getter does not return the adapter";
        }

        if (!Regex.IsMatch(controller, @"_pilotEdit\s*=\s*new\s+PilotEditAdapter\s*\(\s*new\s+WorkspacePilotEditGate\s*\(\s*_host\s*,\s*_noticeDepth\s*\)\s*\)\s*;"))
            yield return "the adapter is not built over the decorator of the host";

        var adapter = TryBody(controller, @"private\s+sealed\s+class\s+PilotEditAdapter\s*:\s*IWorkspacePilotEditPort\b[^{]*");
        if (adapter == null) yield return "the nested adapter is missing";
        else
        {
            foreach (var name in Operations)
            {
                var body = TryBody(adapter, @"public\s+[\w<>,.\s?]+?\s+" + name + @"\s*\(");
                if (body == null) yield return "the adapter lacks " + name;
                else if (!Regex.IsMatch(Squash(body), @"=>_gate\." + name + @"\([\w,]*\);$")) yield return "the adapter does not only delegate " + name;
            }
        }

        // The panel never receives the host core (M-8).
        if (Regex.IsMatch(controller, @"\bpublic\s+(?:readonly\s+)?WorkspaceHostCore\b")) yield return "the controller exposes the host core";
        if (Src.Count(controller, @"\bpublic\s+IWorkspacePilotEditPort\b") != 1) yield return "the controller does not expose exactly one edit port";

        // The limits of the controller (H-12): three +=, no -=, one event, nothing that schedules.
        if (Src.Count(controller, @"\+=") != 3) yield return "the controller has " + Src.Count(controller, @"\+=") + " += (expected 3)";
        if (Src.Count(controller, @"-=") != 0) yield return "the controller has a -=";
        if (Src.Count(controller, @"\bevent\b") != 1) yield return "the controller does not declare exactly one event";
        foreach (Match match in Regex.Matches(controller, @"\bDispatcher\b|\bTimer\b|\bTask\b|\basync\b")) yield return "the controller names " + match.Value;
    }

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
            Assert.Equal(string.Join("\n", new[]
            {
                "adaptador operaciones=18 violaciones=[]",
                "autoprueba-sin-reenvio-de-TryEditAll=falla",
                "autoprueba-reenvio-que-no-solo-delega=falla",
            }), string.Join("\n", log));
        }
    }

#nullable disable
    public sealed class TmpPbt4cTextCt15
    {
        private static string Root => PilotKit.RepoRoot().FullName;

        private const string DraftFolder = "src/RackCad.Application/Systems/PushBack/Draft/";

        private static readonly string[] DraftFiles =
        {
            DraftFolder + "PushBackFullDraftLaunch.cs", DraftFolder + "PushBackFullDraftStatePort.cs",
        };

        private static string Read(string relative) => File.ReadAllText(Path.Combine(Root, relative));

        // ------------------------------------------------------------------------------------------------- rules of G-L1

        // ------------------------------------------------------------------------------------------------- rules of G-L1

        private static readonly string[] Tokens =
        {
            "WorkingBaseline", "workingBaseline", "LastModuleReconciliation", "computation.System", "lastComputation.System",
            "session.System", "lastCompositeSystem", "SystemToInsert",
        };

        private static readonly HashSet<string> Rebind = new HashSet<string>(StringComparer.Ordinal)
        {
            "lastComputation", "lastCompositeSystem", "workingBaseline", "LastModuleReconciliation",
        };

        private const string Link = @"(?:\s*\??\.\s*\w+(?:\s*\([^()]*\))?|\s*\[[^\]]*\])";
        private const string Assign = @"\s*(?:\??\.\s*\w+|\[[^\]]*\])\s*(?:=(?![=>])|\+=|-=|\*=|/=|\+\+|--)";
        private const string Mutators = @"(?:Add|AddRange|Clear|Remove|RemoveAt|RemoveAll|RemoveRange|Insert|InsertRange|Sort|Reverse|RecalculatePositions)";
        private const string Call = @"\s*\??\.\s*" + Mutators + @"\s*\(";
        private const string Coll = @"(?:List|IList|ICollection|Collection|HashSet|Dictionary|IDictionary)";
        private const string CastC = @"\(\s*\(\s*" + Coll + @"\s*<[^>]*>\s*\)[^;]*?";
        private const string Named = @"\b(?:Refresh|RefreshPhysicalModel|\w+InPlace)\s*\([^;]*?";

        private static string TokenRegex(string token) => @"(?<![\w])" + Regex.Escape(token).Replace(@"\.", @"\s*\.\s*");

        private static IEnumerable<string> Hits(string text, IEnumerable<string> patterns)
            => patterns.SelectMany(pattern => Regex.Matches(text, pattern).Cast<Match>().Select(match => match.Value));

        private static List<string> Rule1(string text, IEnumerable<string> tokens)
            => tokens.SelectMany(token => Hits(text, new[] { TokenRegex(token) + Link + "*" + Assign, TokenRegex(token) + Link + "*" + Call })).ToList();

        private static List<string> Rule1b(string text, IEnumerable<string> tokens)
            => tokens.SelectMany(token => Hits(text, new[]
            {
                CastC + TokenRegex(token),
                TokenRegex(token) + @"[^;{}]*?\s+as\s+" + Coll + @"\s*<",
                TokenRegex(token) + @"[^;{}]*?\s+is\s+" + Coll + @"\s*<[^>]*>\s+\w+",
            })).ToList();

        private static List<string> Rule1c(string text, IEnumerable<string> tokens)
            => tokens.SelectMany(token => Hits(text, new[] { Named + TokenRegex(token) })).ToList();

        /// <summary>The text without comments, with the string and character literals emptied (as the source guards do).</summary>
        private static string Clean(string source)
        {
            source = Regex.Replace(source, "@\"(?:[^\"]|\"\")*\"", "\"\"");
            source = Regex.Replace(source, "\\$?\"(?:\\\\.|[^\"\\\\\\n])*\"", "\"\"");
            source = Regex.Replace(source, "'(?:\\\\.|[^'\\\\\\n])'", "''");
            source = Regex.Replace(source, @"/\*.*?\*/", " ", RegexOptions.Singleline);
            source = Regex.Replace(source, @"//[^\n]*", string.Empty);
            return source;
        }

        private static string Collapse(string text)
            => string.Join(" ", text.Split((char[])null, StringSplitOptions.RemoveEmptyEntries));

        /// <summary>The members of a class: the text at depth two and deeper, one entry per member.</summary>
        private static List<string> Members(string source)
        {
            var output = new List<string>();
            var depth = 0;
            var current = new List<string>();
            foreach (var line in source.Split('\n'))
            {
                var before = depth;
                depth += line.Count(c => c == '{') - line.Count(c => c == '}');
                if (before >= 2 || depth > 2) current.Add(line);
                if ((before > 2 && depth == 2) || (before == 2 && depth == 2 && line.Trim().EndsWith(";", StringComparison.Ordinal)))
                {
                    output.Add(Collapse(string.Join(" ", current)));
                    current = new List<string>();
                }
            }

            if (current.Count > 0) output.Add(Collapse(string.Join(" ", current)));
            return output;
        }

        private static int Rfind(string text, char value, int end) => end <= 0 ? -1 : text.LastIndexOf(value, end - 1);

        /// <summary>One level of alias: by assignment, <c>foreach</c>, <c>is</c> and lambda parameter.</summary>
        private static Dictionary<string, string> Aliases(string text, IEnumerable<string> tokens)
        {
            var tokenList = tokens.ToList();
            var alternatives = string.Join("|", tokenList.Select(TokenRegex));
            var found = new Dictionary<string, string>(StringComparer.Ordinal);

            foreach (Match match in Regex.Matches(text, @"(?:\bvar|[\w<>\[\]?,.]+)?\s+(\w+)\s*=(?![=>])\s*([^;{}]*)"))
            {
                var identifier = match.Groups[1].Value;
                if (!tokenList.Contains(identifier) && !Rebind.Contains(identifier) && Regex.IsMatch(match.Groups[2].Value, alternatives))
                {
                    if (!found.ContainsKey(identifier)) found[identifier] = "asignacion";
                }
            }

            foreach (Match match in Regex.Matches(text, @"foreach\s*\(\s*(?:var|[\w<>\[\]?,.]+)\s+(\w+)\s+in\s+([^)]*(?:\([^()]*\)[^)]*)*)\)"))
            {
                if (Regex.IsMatch(match.Groups[2].Value, alternatives) && !found.ContainsKey(match.Groups[1].Value)) found[match.Groups[1].Value] = "foreach";
            }

            foreach (Match match in Regex.Matches(text, "(?:" + alternatives + @")[^;{}]*?\s+is\s+[\w.]+(?:\s*<[^>]*>)?\s+(\w+)"))
            {
                if (!found.ContainsKey(match.Groups[1].Value)) found[match.Groups[1].Value] = "is";
            }

            foreach (Match match in Regex.Matches(text, @"(?:\(([^()]*)\)|\b(\w+))\s*=>"))
            {
                var start = Math.Max(Math.Max(Rfind(text, ';', match.Index), Rfind(text, '{', match.Index)), Rfind(text, '}', match.Index)) + 1;
                if (!Regex.IsMatch(text.Substring(start, match.Index - start), alternatives)) continue;
                var parameters = match.Groups[1].Success ? match.Groups[1].Value.Split(',') : new[] { match.Groups[2].Value };
                foreach (var raw in parameters)
                {
                    var trimmed = raw.Trim();
                    var name = trimmed.Length == 0 ? string.Empty : trimmed.Split((char[])null, StringSplitOptions.RemoveEmptyEntries).Last();
                    if (Regex.IsMatch(name, @"^\w+$") && !tokenList.Contains(name) && !found.ContainsKey(name)) found[name] = "lambda";
                }
            }

            return found;
        }

        /// <summary>Every match of the four rules over the text of one file.</summary>
        private static List<string> Violations(string source)
        {
            var cleaned = Clean(source);
            var flat = Collapse(cleaned);
            var result = new List<string>();
            result.AddRange(Rule1(flat, Tokens).Select(hit => "R1 " + hit));
            result.AddRange(Rule1b(flat, Tokens).Select(hit => "R1b " + hit));
            result.AddRange(Rule1c(flat, Tokens).Select(hit => "R1c " + hit));
            foreach (var member in Members(cleaned))
            {
                var aliases = Aliases(member, Tokens);
                if (aliases.Count > 0)
                {
                    // Rule 2 of D-PBT1-15: an alias counts as a token for rules 1 AND 1b (a mold over an alias is a mutation too).
                    result.AddRange(Rule1(member, aliases.Keys).Select(hit => "R2 " + hit));
                    result.AddRange(Rule1b(member, aliases.Keys).Select(hit => "R2b " + hit));
                }
            }

            return result;
        }

        /// <summary>An injected line, inside a member appended to the real text of a file (never written to disk).</summary>
        private static string Inject(string source, string line)
        {
            var classEnd = source.LastIndexOf('}', source.LastIndexOf('}') - 1);
            return source.Substring(0, classEnd) + "void Inyectado() { " + line + " }\n" + source.Substring(classEnd);
        }

        // ------------------------------------------------------------------------------------------------- the purity of the folder

        private static readonly string[] ForbiddenTerms =
        {
            "Autodesk", "RackCad.Plugin", "RackCad.UI", "System.Windows", "Transaction", "Database", "async", "await", "Task", "File.",
            "System.IO", "RackEmbedComposer", "new RackEmbedDocument",
        };

        /// <summary>No nullable directive, exactly one namespace and in block form, and none of the forbidden terms.</summary>
        private static List<string> Purity(string source)
        {
            var result = new List<string>();
            if (Regex.IsMatch(source, @"^\s*#nullable", RegexOptions.Multiline)) result.Add("#nullable");
            var namespaces = Regex.Matches(source, @"^\s*namespace\s", RegexOptions.Multiline).Count;
            var block = Regex.Matches(source, @"^namespace RackCad\.Application\.Systems\.PushBack\.Draft\s*\r?\n\{", RegexOptions.Multiline).Count;
            if (namespaces != 1 || block != 1) result.Add("namespace RackCad.Application.Systems.PushBack.Draft");
            result.AddRange(ForbiddenTerms.Where(term => source.Contains(term, StringComparison.Ordinal)));
            return result;
        }

        [Fact]
        public void CT15()
        {
            var log = new List<string>();
            var existing = DraftFiles.Where(path => File.Exists(Path.Combine(Root, path))).ToList();
            var texts = existing.Select(path => Read(path)).ToList();
            var violations = existing.Zip(texts, (path, text) => Violations(text).Select(v => path + ": " + v)).SelectMany(list => list).ToList();
            log.Add("g-l1 archivos=" + existing.Count + " reglas-sin-coincidencias=" + (existing.Count == DraftFiles.Length && violations.Count == 0));

            var selfTests = new (string Name, string Line)[]
            {
                ("a", "state.WorkingBaseline.Structure.Name = \"x\";"),
                ("b", "var b = state.WorkingBaseline; b.Structure.Modules.Clear();"),
                ("c", "((List<string>)side.LastModuleReconciliation.Preserved).Add(\"x\");"),
                ("e", "foreach (var module in workingBaseline.Structure.Modules) { module.Length = 1.0; }"),
                ("f1", "state.WorkingBaseline.Structure.Modules[0].Length = 1.0;"),
                ("f2", "lastComputation.System.Structure.Modules[0] = null;"),
                ("f3", "workingBaseline.Structure.Fronts[0].Levels.Clear();"),
                ("g1", "(side.LastModuleReconciliation.Preserved as List<string>).Add(\"x\");"),
                ("g2", "if (side.LastModuleReconciliation.Preserved is List<string> l) { l.Add(\"x\"); }"),
                ("h1", "state.WorkingBaseline.Structure.Modules.ToList().ForEach(m => m.Length = 1);"),
                ("h2", "state.WorkingBaseline.Structure.Modules.ToList().ForEach(m => { m.Length = 1; });"),
                ("i", "state.WorkingBaseline.Structure.RecalculatePositions();"),
                ("j", "builder.Refresh(state.WorkingBaseline.Structure);"),

                // An alias plus a mold. Only rule 2 applied to rule 1b sees them.
                ("k1", "var r = side.LastModuleReconciliation; (r.Preserved as List<string>).Add(\"x\");"),
                ("k2", "var p = side.LastModuleReconciliation.Preserved; ((List<string>)p).Add(\"x\");"),
            };

            // A self-test fails when the rules see the injected line in the text of EVERY file of the list.
            foreach (var test in selfTests)
            {
                var seen = texts.Count == DraftFiles.Length && texts.All(text => Violations(Inject(text, test.Line)).Count > 0);
                log.Add("autoprueba-" + test.Name + "=" + (seen ? "falla" : "pasa"));
            }

            var purity = existing.SelectMany(path => Purity(Read(path)).Select(v => path + ": " + v)).ToList();
            log.Add("pureza archivos=" + existing.Count + " violaciones=[" + string.Join(", ", purity) + "]");

            // The self-tests of the purity: each term injected in the text of EVERY file of the list makes the guard fail.
            log.Add("autoprueba-#nullable=" + (texts.Count == DraftFiles.Length && texts.All(text => Purity(text + "\n#nullable enable\n").Contains("#nullable")) ? "falla" : "pasa"));
            log.Add("autoprueba-namespace RackCad.Application.Systems.PushBack.Draft="
                + (texts.Count == DraftFiles.Length && texts.All(text => Purity(text + "\nnamespace Otro { }\n").Contains("namespace RackCad.Application.Systems.PushBack.Draft")) ? "falla" : "pasa"));
            foreach (var term in ForbiddenTerms)
            {
                log.Add("autoprueba-" + term + "=" + (texts.Count == DraftFiles.Length && texts.All(text => Purity(text + "\n" + term + "\n").Contains(term)) ? "falla" : "pasa"));
            }

            // The one member of the two that edits a state is ApplyToDraft: the file that declares it is the one the guard covers.
            var applying = existing.Where((path, index) => Regex.IsMatch(Clean(texts[index]), @"\bPushBackDraftApplyResult\s+ApplyToDraft\s*\(")).Select(path => Path.GetFileName(path)).ToList();
            log.Add("cubre ApplyToDraft=" + string.Join(",", applying));

            Assert.Equal(string.Join("\n", new[]
            {
                "g-l1 archivos=2 reglas-sin-coincidencias=True",
                "autoprueba-a=falla",
                "autoprueba-b=falla",
                "autoprueba-c=falla",
                "autoprueba-e=falla",
                "autoprueba-f1=falla",
                "autoprueba-f2=falla",
                "autoprueba-f3=falla",
                "autoprueba-g1=falla",
                "autoprueba-g2=falla",
                "autoprueba-h1=falla",
                "autoprueba-h2=falla",
                "autoprueba-i=falla",
                "autoprueba-j=falla",
                "autoprueba-k1=falla",
                "autoprueba-k2=falla",
                "pureza archivos=2 violaciones=[]",
                "autoprueba-#nullable=falla",
                "autoprueba-namespace RackCad.Application.Systems.PushBack.Draft=falla",
                "autoprueba-Autodesk=falla",
                "autoprueba-RackCad.Plugin=falla",
                "autoprueba-RackCad.UI=falla",
                "autoprueba-System.Windows=falla",
                "autoprueba-Transaction=falla",
                "autoprueba-Database=falla",
                "autoprueba-async=falla",
                "autoprueba-await=falla",
                "autoprueba-Task=falla",
                "autoprueba-File.=falla",
                "autoprueba-System.IO=falla",
                "autoprueba-RackEmbedComposer=falla",
                "autoprueba-new RackEmbedDocument=falla",
                "cubre ApplyToDraft=PushBackFullDraftStatePort.cs",
            }), string.Join("\n", log));
        }
    }
}
