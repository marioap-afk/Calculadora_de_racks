using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Xunit;
using Src = RackCad.Tests.WorkspaceHostSource;

namespace RackCad.Tests
{
    /// <summary>
    /// I-64 F4B PB-T4c — the guards of text of «Editar todo» over <c>src/</c> (CT-PBT4C-01, 08 and 13). Each one reads the sources as text,
    /// never the drawing and never a window, and ends in ONE <c>Assert.Equal</c> of its lines joined by <c>\n</c>: the button and its handler
    /// (CT-01), the two pieces of the opener and the wiring of the modal (CT-08) and the forwarding of the adapter of the Plugin (CT-13, with
    /// two negative self-tests run over text injected in memory, never written). A subject that does not exist yet (in the RED) is read as
    /// absent, and the guard fails by its assertion.
    /// </summary>
    public sealed class PushBackEditAllGuardTests
    {
        private const string ControllerPath = "src/RackCad.Plugin/Workspace/WorkspaceController.cs";

        // The 18 operations of the port, in the order of the table of the equivalent of PEG:155-163.
        private static readonly string[] Operations =
        {
            "TryBeginEdit", "TryCheckNow", "TryReload", "TryConfirmBase", "TryKeepMine", "TryDiscardAndReload", "GetField", "SetPendingText",
            "CommitField", "CrossBoundary", "DiscardRack", "DiscardDocument", "GetRacksAtRisk", "TakeLossNotices", "TakeCloseNotices",
            "RequestUpdate", "TakeUpdateProgress", "TryEditAll",
        };

        private static string Squash(string text) => Regex.Replace(text, @"\s+", string.Empty);

        private static string TryBody(string code, string signature) => Regex.IsMatch(code, signature) ? Src.BodyOf(code, signature) : null;

        private static int Occurrences(string text, string literal)
        {
            var count = 0;
            for (var at = text.IndexOf(literal, StringComparison.Ordinal); at >= 0; at = text.IndexOf(literal, at + literal.Length, StringComparison.Ordinal)) count++;
            return count;
        }

        // ------------------------------------------------------------------------------------------------- CT-01

        [Fact]
        public void CT_PBT4C_01_TheEditAllButtonIsInThePanelOfTheFieldAfterEditWithItsFourAttributesAndItsHandlerIsOneContainedCall()
        {
            var log = new List<string>();
            XNamespace x = "http://schemas.microsoft.com/winfx/2006/xaml";
            var doc = XDocument.Parse(Src.Text("src/RackCad.UI/Workspace/WorkspacePanelView.xaml"));
            var buttons = doc.Descendants().Where(e => e.Name.LocalName == "Button").ToList();
            var all = buttons.Where(b => (string)b.Attribute(x + "Name") == "BrowserFieldEditAllButton").ToList();
            var edit = buttons.FirstOrDefault(b => (string)b.Attribute(x + "Name") == "BrowserFieldEditButton");
            var button = all.FirstOrDefault();
            var inPanel = button != null && edit != null && button.Parent == edit.Parent && button.Parent.Name.LocalName == "WrapPanel";
            var previous = button == null ? null : button.ElementsBeforeSelf().LastOrDefault();
            log.Add("xaml BrowserFieldEditAllButton=" + all.Count + " en-el-panel-de-los-botones=" + inPanel
                + " tras=" + (previous == null ? "null" : (string)previous.Attribute(x + "Name") ?? "null"));
            string A(string name) => "[" + (button == null ? "null" : (string)button.Attribute(name) ?? "null") + "]";
            log.Add("xaml Content=" + A("Content") + " AutomationProperties.Name=" + A("AutomationProperties.Name"));
            log.Add("xaml Visibility=" + A("Visibility") + " Click=" + A("Click"));
            var code = Src.Code("src/RackCad.UI/Workspace/WorkspacePanelView.xaml.cs");
            var handlers = Src.Count(code, @"\bvoid\s+OnBrowserFieldEditAllClick\s*\(");
            var body = Regex.Match(code, @"\bvoid\s+OnBrowserFieldEditAllClick\s*\([^)]*\)\s*=>\s*(.*?);\s*$", RegexOptions.Multiline);
            log.Add("code-behind OnBrowserFieldEditAllClick=" + handlers + " cuerpo=[" + (body.Success ? body.Groups[1].Value.Trim() : "null") + "]");

            Assert.Equal(
                string.Join(
                    "\n",
                    new[]
                    {
                        "xaml BrowserFieldEditAllButton=1 en-el-panel-de-los-botones=True tras=BrowserFieldEditButton",
                        "xaml Content=[Editar todo] AutomationProperties.Name=[Editar el rack completo en el editor]",
                        "xaml Visibility=[{Binding Browser.Field.ShowEditAllButton, Converter={StaticResource FlagToVisibility}}] Click=[OnBrowserFieldEditAllClick]",
                        "code-behind OnBrowserFieldEditAllClick=1 cuerpo=[Contained(() => _viewModel.Browser.Field.EditAll())]",
                    }),
                string.Join("\n", log));
        }

        // ------------------------------------------------------------------------------------------------- CT-08

        [Fact]
        public void CT_PBT4C_08_TheTwoPiecesOfTheOpenerAndTheWiringOfTheModalAreTheOnesOfThePackage()
        {
            var log = new List<string>();
            var wco = Src.Code(ControllerPath);
            var modal = TryBody(wco, @"\bclass\s+FullEditorModal\b[^{]*") ?? string.Empty;
            var implements = Regex.Match(wco, @"\bclass\s+FullEditorModal\s*:\s*(\w+)");
            log.Add("modal FullEditorModal=" + Src.Count(wco, @"\bclass\s+FullEditorModal\b")
                + " implementa=" + (implements.Success ? implements.Groups[1].Value : "null")
                + " using-EnterPanelModal=" + Src.Count(modal, @"using\s*\(\s*_controller\.EnterPanelModal\s*\(\s*\)\s*\)")
                + " _open=" + Src.Count(modal, @"\b_open\s*\(")
                + " RackCad.UI=" + Occurrences(modal, "RackCad.UI") + " PushBack=" + Occurrences(modal, "PushBack"));
            const string OpenerPath = "src/RackCad.Plugin/Views/RackWorkspaceEditorWindow.cs";
            var opener = Src.Exists(OpenerPath) ? Src.Code(OpenerPath) : string.Empty;
            const string Conversion = "var window = RackPushBackSystemWindow.ForDraft((PushBackDraftEditorLaunch)launch);";
            const string Show = "AcApplication.ShowModalWindow(window);";
            log.Add("abridor conversion=[" + Conversion + "] veces=" + Occurrences(opener, Conversion));
            log.Add("abridor mostrar=[" + Show + "] veces=" + Occurrences(opener, Show) + " Owner-asignado=" + Src.Count(opener, @"\bOwner\s*=(?!=)")
                + " RackCad.Application.Workspace=" + Occurrences(opener, "RackCad.Application.Workspace"));
            const string Wiring = "_host.SetFullEditHost(new FullEditorModal(this, RackWorkspaceEditorWindow.Show));";
            var wiring = wco.IndexOf(Wiring, StringComparison.Ordinal);
            var update = wco.IndexOf("_host.SetPilotUpdateHost(", StringComparison.Ordinal);
            log.Add("cableado [" + Wiring + "] veces=" + Occurrences(wco, Wiring) + " tras-SetPilotUpdateHost=" + (wiring >= 0 && update >= 0 && wiring > update));

            Assert.Equal(
                string.Join(
                    "\n",
                    new[]
                    {
                        "modal FullEditorModal=1 implementa=IWorkspaceFullEditHost using-EnterPanelModal=1 _open=1 RackCad.UI=0 PushBack=0",
                        "abridor conversion=[var window = RackPushBackSystemWindow.ForDraft((PushBackDraftEditorLaunch)launch);] veces=1",
                        "abridor mostrar=[AcApplication.ShowModalWindow(window);] veces=1 Owner-asignado=0 RackCad.Application.Workspace=0",
                        "cableado [_host.SetFullEditHost(new FullEditorModal(this, RackWorkspaceEditorWindow.Show));] veces=1 tras-SetPilotUpdateHost=True",
                    }),
                string.Join("\n", log));
        }

        // ------------------------------------------------------------------------------------------------- CT-13

        // PEG:659-707, with the 18 operations: the adapter of the controller only delegates, and the controller keeps its limits.
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
        public void CT_PBT4C_13_TheAdapterOfThePluginForwardsEditAllAndItsGuardSeesAMissingForwardAndOneThatDoesMoreThanDelegate()
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

            Assert.Equal(
                string.Join(
                    "\n",
                    new[]
                    {
                        "adaptador operaciones=18 violaciones=[]",
                        "autoprueba-sin-reenvio-de-TryEditAll=falla",
                        "autoprueba-reenvio-que-no-solo-delega=falla",
                    }),
                string.Join("\n", log));
        }
    }
}
