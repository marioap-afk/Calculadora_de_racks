using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-64 F4B PB-T3a — the TEXT GUARDS of the full draft model (CT-PBT3A-12 to 14). They are GUARDS: they pass on the
    /// skeleton and on the implementation, and they discriminate through their negative self-tests, run over text injected in
    /// memory (never written).
    ///
    /// <para>
    /// CT-PBT3A-12: the full model is not connected (OB-05.2, OB-05.5): no source file names its types outside of its own folder
    /// and of the two files that PB-T3b generalizes. CT-PBT3A-13: G-L1 of D2 (CT-PBT1B-14), extended to the new folder: nothing
    /// mutates the resolved rack or the module report. CT-PBT3A-14: the purity of the folder.
    /// </para>
    /// </summary>
    public sealed class PushBackDraftWriteGuardTests
    {
        private static string Root => PilotKit.RepoRoot().FullName;

        private const string DraftFolder = "src/RackCad.Application/Systems/PushBack/Draft/";

        private static readonly string[] DraftFiles =
        {
            DraftFolder + "PushBackConflictClassifier.cs", DraftFolder + "PushBackEditableStateSeat.cs", DraftFolder + "PushBackFullComposition.cs",
            DraftFolder + "PushBackFullDraftMaterializer.cs", DraftFolder + "PushBackFullDraftReader.cs", DraftFolder + "PushBackFullDraftState.cs",
            DraftFolder + "PushBackFullDraftStateComparer.cs", DraftFolder + "PushBackViewDisposition.cs",
        };

        private static string Read(string relative) => File.ReadAllText(Path.Combine(Root, relative));

        private static bool InBuildFolder(string path)
            => path.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar)
               || path.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar);

        // ------------------------------------------------------------------------------------------------- CT-12

        private static readonly string[] ConnectedNames =
        {
            "PushBackFullDraftState", "PushBackFullDraftReader", "PushBackFullComposition", "PushBackConflictClassifier", "PushBackViewDispositions",
        };

        /// <summary>The folder of the model and the two files that PB-T3b generalizes (the plan and the run of the update).</summary>
        private static bool MayNameTheModel(string relative)
            => relative.StartsWith(DraftFolder, StringComparison.Ordinal)
               || relative == "src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdatePlan.cs"
               || relative == "src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdateRun.cs";

        private static List<string> Naming(IEnumerable<(string Path, string Text)> files)
            => files.Where(file => !MayNameTheModel(file.Path))
                .SelectMany(file => ConnectedNames
                    .Where(name => Regex.IsMatch(file.Text, @"\b" + Regex.Escape(name) + @"\b"))
                    .Select(name => file.Path + ": " + name))
                .ToList();

        [Fact(Skip = "I-64 F4B PB-T4a: sustituida por CT-PBT4A-06 de PushBackFullReadGuardTests (registro declarado superseding-characterization.md; decision del Owner owner-pbt4-decisions.md, pregunta 4).")]
        public void CT_PBT3A_12_NoSourceFileOutsideTheFolderOfTheModelNamesItsTypes()
        {
            var log = new List<string>();
            var files = Directory.GetFiles(Path.Combine(Root, "src"), "*.cs", SearchOption.AllDirectories)
                .Where(path => !InBuildFolder(path))
                .Select(path => Path.GetRelativePath(Root, path).Replace(Path.DirectorySeparatorChar, '/'))
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToList();
            var texts = files.Select(path => (Path: path, Text: Read(path))).ToList();
            var found = Naming(texts);
            log.Add("sin-conectar archivos-revisados>0=" + (texts.Count(file => !MayNameTheModel(file.Path)) > 0) + " coincidencias=[" + string.Join(", ", found) + "]");

            // The self-tests: each name injected in the host and in the support of the pilot makes the guard fail.
            foreach (var destination in new[] { "WorkspaceHostCore.cs", "WorkspacePilotSupport.cs" })
            {
                var path = "src/RackCad.Application/Workspace/" + destination;
                var real = Read(path);
                foreach (var name in ConnectedNames)
                {
                    var injected = new[] { (Path: path, Text: real + "\n// " + name + " inyectado\n") };
                    log.Add("autoprueba-" + name + "-en-" + destination + "=" + (Naming(injected).Count > 0 ? "falla" : "pasa"));
                }
            }

            var expected = new List<string> { "sin-conectar archivos-revisados>0=True coincidencias=[]" };
            foreach (var destination in new[] { "WorkspaceHostCore.cs", "WorkspacePilotSupport.cs" })
            {
                foreach (var name in new[] { "PushBackFullDraftState", "PushBackFullDraftReader", "PushBackFullComposition", "PushBackConflictClassifier", "PushBackViewDispositions" })
                {
                    expected.Add("autoprueba-" + name + "-en-" + destination + "=falla");
                }
            }

            Assert.Equal(expected, log);
        }

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

        // ------------------------------------------------------------------------------------------------- CT-13

        [Fact]
        public void CT_PBT3A_13_NoLineOfTheFolderOfTheModelMutatesTheResolvedRackOrTheModuleReport()
        {
            var log = new List<string>();
            var existing = DraftFiles.Where(path => File.Exists(Path.Combine(Root, path))).ToList();
            var violations = existing.SelectMany(path => Violations(Read(path)).Select(v => path + ": " + v)).ToList();
            log.Add("reglas-sin-coincidencias=" + (existing.Count == DraftFiles.Length && violations.Count == 0));

            var real = Read(DraftFolder + "PushBackFullComposition.cs");
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

            foreach (var test in selfTests)
            {
                log.Add("autoprueba-" + test.Name + "=" + (Violations(Inject(real, test.Line)).Count > 0 ? "falla" : "pasa"));
            }

            Assert.Equal(
                new[]
                {
                    "reglas-sin-coincidencias=True",
                    "autoprueba-a=falla", "autoprueba-b=falla", "autoprueba-c=falla", "autoprueba-e=falla",
                    "autoprueba-f1=falla", "autoprueba-h1=falla", "autoprueba-i=falla", "autoprueba-j=falla",
                },
                log);
        }

        // ------------------------------------------------------------------------------------------------- CT-14

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
        public void CT_PBT3A_14_TheFolderOfTheModelHasNoAutoCadNoWindowsNoTransactionAndNoNullableDirective()
        {
            var log = new List<string>();
            var existing = DraftFiles.Where(path => File.Exists(Path.Combine(Root, path))).ToList();
            var violations = existing.SelectMany(path => Purity(Read(path)).Select(v => path + ": " + v)).ToList();
            log.Add("pureza archivos=" + existing.Count + " violaciones=[" + string.Join(", ", violations) + "]");

            var real = Read(DraftFolder + "PushBackFullComposition.cs");
            log.Add("autoprueba-#nullable=" + (Purity(real + "\n#nullable enable\n").Contains("#nullable") ? "falla" : "pasa"));
            log.Add("autoprueba-namespace RackCad.Application.Systems.PushBack.Draft="
                + (Purity(real + "\nnamespace Otro { }\n").Contains("namespace RackCad.Application.Systems.PushBack.Draft") ? "falla" : "pasa"));
            foreach (var term in ForbiddenTerms)
            {
                log.Add("autoprueba-" + term + "=" + (Purity(real + "\n" + term + "\n").Contains(term) ? "falla" : "pasa"));
            }

            Assert.Equal(
                new[]
                {
                    "pureza archivos=8 violaciones=[]",
                    "autoprueba-#nullable=falla",
                    "autoprueba-namespace RackCad.Application.Systems.PushBack.Draft=falla",
                    "autoprueba-Autodesk=falla", "autoprueba-RackCad.Plugin=falla", "autoprueba-RackCad.UI=falla", "autoprueba-System.Windows=falla",
                    "autoprueba-Transaction=falla", "autoprueba-Database=falla", "autoprueba-async=falla", "autoprueba-await=falla",
                    "autoprueba-Task=falla", "autoprueba-File.=falla", "autoprueba-System.IO=falla", "autoprueba-RackEmbedComposer=falla",
                    "autoprueba-new RackEmbedDocument=falla",
                },
                log);
        }
    }
}
