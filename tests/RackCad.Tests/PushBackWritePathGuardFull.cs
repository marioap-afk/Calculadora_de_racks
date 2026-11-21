using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-64 F4B PB-T3b — the TEXT GUARDS of the write of the full model (CT-PBT3B-13, two [Fact]). They are GUARDS: they pass on the inert
    /// skeleton and on the implementation, and they discriminate through their negative self-tests, run over text injected in memory
    /// (never written).
    ///
    /// <para>
    /// The first one is G-L1 of D2 (CT-PBT1B-14 and CT-PBT3A-13) over the three files of PB-T3b: nothing in the plan, in the run or in the new
    /// interface mutates the resolved rack or the module report. The second one is the «sin conectar» of OB-05.5: no file of the product
    /// outside the plan, the run and the folder of the model names the input of the full PREPARE or calls the run of the full model, and the
    /// host of the panel keeps the draft of the pilot as the only draft that it finds.
    /// </para>
    /// </summary>
    public sealed class FullPanelCopyPushBackWritePathGuardRedTests
    {
        private static string Root => PilotKit.RepoRoot().FullName;

        private const string PilotFolder = "src/RackCad.Application/Systems/PushBack/Pilot/";
        private const string DraftFolder = "src/RackCad.Application/Systems/PushBack/Draft/";
        private const string PlanPath = PilotFolder + "PushBackPilotUpdatePlan.cs";
        private const string RunPath = PilotFolder + "PushBackPilotUpdateRun.cs";
        private const string HostPath = "src/RackCad.Application/Workspace/WorkspaceHostCore.cs";

        private static string Read(string relative) => File.ReadAllText(Path.Combine(Root, relative));

        private static bool InBuildFolder(string path)
            => path.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar)
               || path.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar);

        // ------------------------------------------------------------------------------------------------- CT-13 (ii)

        /// <summary>The plan, the run and the folder of the model: the only files that may name the full write (the plan and the run are
        /// the two files that PB-T3b generalizes, and the model lives in its own folder).</summary>
        private static bool MayNameTheFullWrite(string relative)
            => relative == PlanPath || relative == RunPath || relative == HostPath || relative.StartsWith(DraftFolder, StringComparison.Ordinal);

        private static readonly Regex FullWriteNames = new Regex(
            @"\bPushBackFullPrepareInput\b|\bPushBackPilotUpdateRun\s*\.\s*ExecuteFull\b|\bPrecheck\s*\(\s*[^;)]*\bDraftSession\s*<\s*PushBackFullDraftState\s*>",
            RegexOptions.CultureInvariant);

        private static List<string> NamingTheFullWrite(IEnumerable<(string Path, string Text)> files)
            => files.Where(file => !MayNameTheFullWrite(file.Path) && FullWriteNames.IsMatch(file.Text)).Select(file => file.Path).ToList();

        /// <summary>The generic arguments of every <c>Find&lt;</c> of the host: all of them have to be the draft of the pilot.</summary>
        private static List<string> FindArguments(string host)
            => Regex.Matches(host, @"\bFind\s*<\s*([^>]*?)\s*>").Cast<Match>().Select(match => match.Groups[1].Value).ToList();

        private static bool OnlyThePilotDraftIsFound(string host)
        {
            var found = FindArguments(host);
            return found.Count > 0 && found.All(argument => argument == "PushBackFullDraftState");
        }

        [Fact]
        public void CT_PBT3B_13_NoFileOutsideThePlanTheRunAndTheModelNamesTheFullWriteAndTheHostFindsOnlyTheDraftOfThePilot()
        {
            var log = new List<string>();
            var files = Directory.GetFiles(Path.Combine(Root, "src"), "*.cs", SearchOption.AllDirectories)
                .Where(path => !InBuildFolder(path))
                .Select(path => Path.GetRelativePath(Root, path).Replace(Path.DirectorySeparatorChar, '/'))
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToList();
            var texts = files.Select(path => (Path: path, Text: Read(path))).ToList();
            log.Add("sin-conectar archivos-revisados>0=" + (texts.Count(file => !MayNameTheFullWrite(file.Path)) > 0)
                + " coincidencias=[" + string.Join(", ", NamingTheFullWrite(texts)) + "]");

            var host = Read(HostPath);
            log.Add("host Find<PushBackFullDraftState>-es-el-unico-Find<=" + OnlyThePilotDraftIsFound(host) + " busquedas>0=" + (FindArguments(host).Count > 0));

            // The self-tests: each of the names injected in the host and in the support of the pilot makes the guard fail.
            var expected = new List<string>
            {
                "sin-conectar archivos-revisados>0=True coincidencias=[]",
                "host Find<PushBackFullDraftState>-es-el-unico-Find<=True busquedas>0=True",
            };
            var injections = new (string Name, string Line)[]
            {
                ("PushBackFullPrepareInput", "var input = PushBackFullPrepareInput.From(draft);"),
                ("PushBackPilotUpdateRun.ExecuteFull", "var outcome = PushBackPilotUpdateRun.ExecuteFull(draft, request, port);"),
                ("Precheck", "var check = PushBackPilotUpdateRun.Precheck(full as DraftSession<PushBackFullDraftState>);"),
            };
            // I-64 F4B PB-T4b: the host core runs the full write, so the self-tests inject the names in the support of the panel only.
            foreach (var destination in new[] { "WorkspacePilotSupport.cs" })
            {
                var path = "src/RackCad.Application/Workspace/" + destination;
                var real = Read(path);
                foreach (var injection in injections)
                {
                    var injected = new[] { (Path: path, Text: real + "\n" + injection.Line + "\n") };
                    log.Add("autoprueba-" + injection.Name + "-en-" + destination + "=" + (NamingTheFullWrite(injected).Count > 0 ? "falla" : "pasa"));
                    expected.Add("autoprueba-" + injection.Name + "-en-" + destination + "=falla");
                }
            }

            log.Add("autoprueba-Find<PushBackPilotDraftState>-en-WorkspaceHostCore.cs="
                + (OnlyThePilotDraftIsFound(host + "\n_drafts.Find<PushBackPilotDraftState>(session, rackId);\n") ? "pasa" : "falla"));
            log.Add("control-Find<PushBackFullDraftState>-en-WorkspaceHostCore.cs="
                + (OnlyThePilotDraftIsFound(host + "\n_drafts.Find<PushBackFullDraftState>(session, rackId);\n") ? "pasa" : "falla"));
            expected.Add("autoprueba-Find<PushBackPilotDraftState>-en-WorkspaceHostCore.cs=falla");
            expected.Add("control-Find<PushBackFullDraftState>-en-WorkspaceHostCore.cs=pasa");

            Assert.Equal(string.Join("\n", expected), string.Join("\n", log));
        }
    }
}
