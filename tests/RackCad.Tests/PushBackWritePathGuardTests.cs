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
    public sealed class PushBackWritePathGuardTests
    {
        private static string Root => PilotKit.RepoRoot().FullName;

        private const string PilotFolder = "src/RackCad.Application/Systems/PushBack/Pilot/";
        private const string DraftFolder = "src/RackCad.Application/Systems/PushBack/Draft/";
        private const string PlanPath = PilotFolder + "PushBackPilotUpdatePlan.cs";
        private const string RunPath = PilotFolder + "PushBackPilotUpdateRun.cs";
        private const string EraserPath = PilotFolder + "IPushBackPilotViewEraser.cs";
        private const string HostPath = "src/RackCad.Application/Workspace/WorkspaceHostCore.cs";

        private static readonly string[] WriteFiles = { PlanPath, RunPath, EraserPath };

        private static string Read(string relative) => File.ReadAllText(Path.Combine(Root, relative));

        private static bool InBuildFolder(string path)
            => path.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar)
               || path.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar);

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

        // ------------------------------------------------------------------------------------------------- CT-13 (i)

        [Fact]
        public void CT_PBT3B_13_NoLineOfThePlanTheRunOrTheNewInterfaceMutatesTheResolvedRackOrTheModuleReport()
        {
            var log = new List<string>();
            var existing = WriteFiles.Where(path => File.Exists(Path.Combine(Root, path))).ToList();
            var violations = existing.SelectMany(path => Violations(Read(path)).Select(v => path + ": " + v)).ToList();
            log.Add("reglas-sin-coincidencias=" + (existing.Count == WriteFiles.Length && violations.Count == 0));

            var selfTests = new (string Name, string Line)[]
            {
                ("a", "state.WorkingBaseline.Structure.Name = \"x\";"),
                ("b", "var b = state.WorkingBaseline; b.Structure.Modules.Clear();"),
                ("c", "((List<string>)side.LastModuleReconciliation.Preserved).Add(\"x\");"),
                ("e", "foreach (var module in workingBaseline.Structure.Modules) { module.Length = 1.0; }"),
                ("f1", "state.WorkingBaseline.Structure.Modules[0].Length = 1.0;"),
                ("h1", "state.WorkingBaseline.Structure.Modules.ToList().ForEach(m => m.Length = 1);"),
                ("i", "state.WorkingBaseline.Structure.RecalculatePositions();"),
                ("j", "builder.Refresh(state.WorkingBaseline.Structure);"),
            };
            var expected = new List<string> { "reglas-sin-coincidencias=True" };
            foreach (var path in WriteFiles)
            {
                var real = Read(path);
                var file = Path.GetFileName(path);
                foreach (var test in selfTests)
                {
                    log.Add("autoprueba-" + test.Name + "-en-" + file + "=" + (Violations(Inject(real, test.Line)).Count > 0 ? "falla" : "pasa"));
                    expected.Add("autoprueba-" + test.Name + "-en-" + file + "=falla");
                }
            }

            Assert.Equal(string.Join("\n", expected), string.Join("\n", log));
        }

        // ------------------------------------------------------------------------------------------------- CT-13 (ii)

        /// <summary>The plan, the run and the folder of the model: the only files that may name the full write (the plan and the run are
        /// the two files that PB-T3b generalizes, and the model lives in its own folder).</summary>
        private static bool MayNameTheFullWrite(string relative)
            => relative == PlanPath || relative == RunPath || relative.StartsWith(DraftFolder, StringComparison.Ordinal);

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
            return found.Count > 0 && found.All(argument => argument == "PushBackPilotDraftState");
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
            log.Add("host Find<PushBackPilotDraftState>-es-el-unico-Find<=" + OnlyThePilotDraftIsFound(host) + " busquedas>0=" + (FindArguments(host).Count > 0));

            // The self-tests: each of the names injected in the host and in the support of the pilot makes the guard fail.
            var expected = new List<string>
            {
                "sin-conectar archivos-revisados>0=True coincidencias=[]",
                "host Find<PushBackPilotDraftState>-es-el-unico-Find<=True busquedas>0=True",
            };
            var injections = new (string Name, string Line)[]
            {
                ("PushBackFullPrepareInput", "var input = PushBackFullPrepareInput.From(draft);"),
                ("PushBackPilotUpdateRun.ExecuteFull", "var outcome = PushBackPilotUpdateRun.ExecuteFull(draft, request, port);"),
                ("Precheck", "var check = PushBackPilotUpdateRun.Precheck(full as DraftSession<PushBackFullDraftState>);"),
            };
            foreach (var destination in new[] { "WorkspaceHostCore.cs", "WorkspacePilotSupport.cs" })
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

            log.Add("autoprueba-Find<PushBackFullDraftState>-en-WorkspaceHostCore.cs="
                + (OnlyThePilotDraftIsFound(host + "\n_drafts.Find<PushBackFullDraftState>(session, rackId);\n") ? "pasa" : "falla"));
            log.Add("control-Find<PushBackPilotDraftState>-en-WorkspaceHostCore.cs="
                + (OnlyThePilotDraftIsFound(host + "\n_drafts.Find<PushBackPilotDraftState>(session, rackId);\n") ? "pasa" : "falla"));
            expected.Add("autoprueba-Find<PushBackFullDraftState>-en-WorkspaceHostCore.cs=falla");
            expected.Add("control-Find<PushBackPilotDraftState>-en-WorkspaceHostCore.cs=pasa");

            Assert.Equal(string.Join("\n", expected), string.Join("\n", log));
        }
    }
}
