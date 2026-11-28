using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-64 F4B PB-T4b — the census of the types of the full model (CT-PBT4B-13). It is a GUARD: it passes on the skeleton and on the
    /// implementation, and it discriminates through its negative self-tests, run over text injected in memory (never written). It replaces
    /// CT-PBT4A-06: besides the folder of the model, the plan and the run of the update and the adapter of the Plugin, the host of the
    /// workspace may now name the five types, because its draft is the full one.
    /// </summary>
    public sealed class PushBackFullPanelCensusGuardTests
    {
        private static string Root => PilotKit.RepoRoot().FullName;

        private const string DraftFolder = "src/RackCad.Application/Systems/PushBack/Draft/";

        private static readonly string[] ConnectedNames =
        {
            "PushBackFullDraftState", "PushBackFullDraftReader", "PushBackFullComposition", "PushBackConflictClassifier", "PushBackViewDispositions",
        };

        private static bool InBuildFolder(string path)
            => path.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar)
               || path.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar);

        /// <summary>The folder of the model, the plan and the run of the update (PB-T3b), the adapter of the Plugin and, from PB-T4b, the host.</summary>
        private static bool MayNameTheModel(string relative)
            => relative.StartsWith(DraftFolder, StringComparison.Ordinal)
               || relative == "src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdatePlan.cs"
               || relative == "src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdateRun.cs"
               || relative == "src/RackCad.Plugin/Views/RackPilotIntentAdapter.cs"
               || relative == "src/RackCad.Application/Workspace/WorkspaceHostCore.cs";

        private static List<string> Naming(IEnumerable<(string Path, string Text)> files)
            => files.Where(file => !MayNameTheModel(file.Path))
                .SelectMany(file => ConnectedNames
                    .Where(name => Regex.IsMatch(file.Text, @"\b" + Regex.Escape(name) + @"\b"))
                    .Select(name => file.Path + ": " + name))
                .ToList();

        [Fact]
        public void CT_PBT4B_13_OnlyTheFolderOfTheModelThePlanTheRunTheAdapterOfThePluginAndTheHostNameItsTypes()
        {
            var log = new List<string>();
            var files = Directory.GetFiles(Path.Combine(Root, "src"), "*.cs", SearchOption.AllDirectories)
                .Where(path => !InBuildFolder(path))
                .Select(path => Path.GetRelativePath(Root, path).Replace(Path.DirectorySeparatorChar, '/'))
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToList();
            var texts = files.Select(path => (Path: path, Text: File.ReadAllText(Path.Combine(Root, path)))).ToList();
            var found = Naming(texts);
            log.Add("sin-conectar archivos-revisados>0=" + (texts.Count(file => !MayNameTheModel(file.Path)) > 0) + " coincidencias=[" + string.Join(", ", found) + "]");

            // The self-tests: each name injected in the support of the pilot and in the intention of the host makes the guard fail.
            foreach (var destination in new[] { "WorkspacePilotSupport.cs", "WorkspaceEditIntent.cs" })
            {
                var path = "src/RackCad.Application/Workspace/" + destination;
                var real = File.ReadAllText(Path.Combine(Root, path));
                foreach (var name in ConnectedNames)
                {
                    var injected = new[] { (Path: path, Text: real + "\n// " + name + " inyectado\n") };
                    log.Add("autoprueba-" + name + "-en-" + destination + "=" + (Naming(injected).Count > 0 ? "falla" : "pasa"));
                }
            }

            var expected = new List<string> { "sin-conectar archivos-revisados>0=True coincidencias=[]" };
            foreach (var destination in new[] { "WorkspacePilotSupport.cs", "WorkspaceEditIntent.cs" })
            {
                foreach (var name in new[] { "PushBackFullDraftState", "PushBackFullDraftReader", "PushBackFullComposition", "PushBackConflictClassifier", "PushBackViewDispositions" })
                {
                    expected.Add("autoprueba-" + name + "-en-" + destination + "=falla");
                }
            }

            Assert.Equal(string.Join("\n", expected), string.Join("\n", log));
        }
    }
}
