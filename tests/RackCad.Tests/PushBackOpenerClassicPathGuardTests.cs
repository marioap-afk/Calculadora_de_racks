using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;
using Src = RackCad.Tests.WorkspaceHostSource;

namespace RackCad.Tests
{
    /// <summary>
    /// I-64 F4B PB-T4c — the guard of OB-01.3 over the six subjects of the path of «Editar todo» (CT-PBT4C-07; D-PBT4-16): the window of the
    /// opener, the body of <c>FullEditorModal</c>, the bodies of <c>TryEditAll</c> and <c>EditAllOfPilot</c> of the host core, the body of
    /// <c>EditAll</c> of the field and the two files of the folder of the full model. In text without comments, none of them names the eight
    /// names of the classic path, of the insertion and of the drawing. The self-test injects each name in each subject (48). A subject that
    /// does not exist yet (in the RED) is its empty text, and the guard passes.
    /// </summary>
    public sealed class PushBackOpenerClassicPathGuardTests
    {
        private static readonly string[] Forbidden =
        {
            "RackPushBackCommands", "RackMenuCommands", "EditorModules", "RackViewBatchExecution", "DrawAndPlace", "RedrawInPlace", "InsertRequested", "InsertionRequest",
        };

        private static readonly string[] Subjects =
        {
            "RackWorkspaceEditorWindow.cs", "FullEditorModal", "TryEditAll", "EditAll", "PushBackFullDraftLaunch.cs", "PushBackFullDraftStatePort.cs",
        };

        private static string CodeOrEmpty(string relative) => Src.Exists(relative) ? Src.Code(relative) : string.Empty;

        private static string TryBody(string code, string signature) => Regex.IsMatch(code, signature) ? Src.BodyOf(code, signature) : null;

        /// <summary>The text without comments of each subject; a subject that does not exist (in the RED) is the empty text.</summary>
        private static string Subject(string name)
        {
            switch (name)
            {
                case "RackWorkspaceEditorWindow.cs":
                    return CodeOrEmpty("src/RackCad.Plugin/Views/RackWorkspaceEditorWindow.cs");
                case "FullEditorModal":
                    return TryBody(Src.Code("src/RackCad.Plugin/Workspace/WorkspaceController.cs"), @"\bclass\s+FullEditorModal\b[^{]*") ?? string.Empty;
                case "TryEditAll":
                    var host = Src.Code("src/RackCad.Application/Workspace/WorkspaceHostCore.cs");
                    return (TryBody(host, @"EditAllOutcome\s+IWorkspacePilotEditPort\.TryEditAll\s*\(") ?? string.Empty) + "\n"
                        + (TryBody(host, @"private\s+EditAllOutcome\s+EditAllOfPilot\s*\(") ?? string.Empty);
                case "EditAll":
                    return TryBody(Src.Code("src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs"), @"public\s+void\s+EditAll\s*\(\s*\)") ?? string.Empty;
                case "PushBackFullDraftLaunch.cs":
                    return CodeOrEmpty("src/RackCad.Application/Systems/PushBack/Draft/PushBackFullDraftLaunch.cs");
                case "PushBackFullDraftStatePort.cs":
                    return CodeOrEmpty("src/RackCad.Application/Systems/PushBack/Draft/PushBackFullDraftStatePort.cs");
                default:
                    throw new ArgumentException(name);
            }
        }

        private static List<string> NamesIn(string text) => Forbidden.Where(name => Regex.IsMatch(text, @"\b" + Regex.Escape(name) + @"\b")).ToList();

        [Fact]
        public void CT_PBT4C_07_NoneOfTheSixSubjectsOfThePathOfEditAllNamesTheClassicPathTheInsertionOrTheDrawing()
        {
            var log = new List<string>();
            var texts = Subjects.ToDictionary(subject => subject, Subject);
            foreach (var subject in Subjects) log.Add("sujeto " + subject + " prohibidos=[" + string.Join(", ", NamesIn(texts[subject])) + "]");
            foreach (var subject in Subjects)
            {
                foreach (var name in Forbidden)
                {
                    log.Add("autoprueba-" + name + "-en-" + subject + "=" + (NamesIn(texts[subject] + "\n" + name + "();\n").Contains(name) ? "falla" : "pasa"));
                }
            }

            Assert.Equal(
                string.Join(
                    "\n",
                    new[]
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
                    }),
                string.Join("\n", log));
        }
    }
}
