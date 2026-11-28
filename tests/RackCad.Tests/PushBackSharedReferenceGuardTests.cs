using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-64 F4B PB-T1b — the guards of the two shared references of decision D2 of the Owner (F4B-E11): CT-PBT1B-14 (G-L1,
    /// over the source of Application) and CT-PBT1B-15 (the writers of <c>Name</c> of the Plugin). They are GUARDS: they
    /// pass on the skeleton and on the implementation, and they discriminate through their negative self-tests, run over
    /// text injected in memory (never written).
    ///
    /// <para>
    /// The two shared references are the resolved rack (<c>PushBackSystem</c>) and the module report
    /// (<c>RackModuleReconciliationResult</c>). This is NOT authorization for general shallow copying: if a task needs to
    /// MUTATE either inside the editing lifetime, it stops and L-1 is reopened.
    /// </para>
    /// <para>
    /// What G-L1 does NOT see (declared limit): a mutation inside a method of another file that receives the reference by a
    /// path that is not rule 1c; an alias of two levels; <c>ref</c>/<c>out</c>, tuples, <c>switch</c> patterns, closures and
    /// local functions that capture a token without being a lambda parameter, query-syntax LINQ, <c>dynamic</c> and
    /// reflection; a chain with nested parentheses in the arguments of a link. The deep fingerprints of CT-PBT1B-16 cover the
    /// exercised paths.
    /// </para>
    /// </summary>
    public sealed class PushBackSharedReferenceGuardTests
    {
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

        // ------------------------------------------------------------------------------------------------ the files

        private static string Root => PilotKit.RepoRoot().FullName;

        private static IEnumerable<string> GuardedFiles()
        {
            const string push = "src/RackCad.Application/Systems/PushBack/";
            var files = new List<string>
            {
                push + "PushBackEditableState.cs", push + "PushBackEditableStateComparer.cs", push + "PushBackEditableStateOwnership.cs",
                push + "PushBackEditorOutputGate.cs", push + "PushBackEditorState.cs", push + "PushBackEditorState.Load.cs",
                push + "PushBackCompositeEditorState.cs", push + "PushBackCompositeEditorAssembler.cs", push + "PushBackEditorDesignAssembler.cs",
                "src/RackCad.Application/Systems/Shared/RackModuleEditSession.cs", "src/RackCad.Application/Systems/Shared/RackModuleReconciliation.cs",
            };

            var folder = Path.Combine(Root, "src", "RackCad.Application", "Workspace");
            if (Directory.Exists(folder))
            {
                files.AddRange(Directory.GetFiles(folder, "*.cs", SearchOption.AllDirectories)
                    .Where(path => !path.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar)
                                   && !path.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar))
                    .Select(path => Path.GetRelativePath(Root, path).Replace(Path.DirectorySeparatorChar, '/'))
                    .OrderBy(path => path, StringComparer.Ordinal));
            }

            return files;
        }

        /// <summary>An injected line, inside a member appended to the real text of a file (never written to disk).</summary>
        private static string Inject(string source, string line)
        {
            var classEnd = source.LastIndexOf('}', source.LastIndexOf('}') - 1);
            return source.Substring(0, classEnd) + "void Inyectado() { " + line + " }\n" + source.Substring(classEnd);
        }

        // ------------------------------------------------------------------------------------------------- CT-14

        [Fact]
        public void CT_PBT1B_14_NoLineOfTheEditingPathMutatesTheResolvedRackOrTheModuleReport()
        {
            var log = new List<string>();
            var files = GuardedFiles().ToList();
            var existing = files.Where(path => File.Exists(Path.Combine(Root, path))).ToList();
            var violations = existing.SelectMany(path => Violations(File.ReadAllText(Path.Combine(Root, path))).Select(v => path + ": " + v)).ToList();
            log.Add("reglas-sin-coincidencias=" + (existing.Count == files.Count && violations.Count == 0));

            var real = File.ReadAllText(Path.Combine(Root, "src/RackCad.Application/Systems/PushBack/PushBackEditorState.cs"));
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

                // I01 (the correction of attempt 2): an alias plus a mold. Only rule 2 applied to rule 1b sees them.
                ("k1", "var r = side.LastModuleReconciliation; (r.Preserved as List<string>).Add(\"x\");"),
                ("k2", "var p = side.LastModuleReconciliation.Preserved; ((List<string>)p).Add(\"x\");"),
            };

            foreach (var test in selfTests)
            {
                log.Add("autoprueba-" + test.Name + "=" + (Violations(Inject(real, test.Line)).Count > 0 ? "falla" : "pasa"));
            }

            Assert.Equal(
                new[]
                {
                    "reglas-sin-coincidencias=True",
                    "autoprueba-a=falla", "autoprueba-b=falla", "autoprueba-c=falla", "autoprueba-e=falla", "autoprueba-f1=falla",
                    "autoprueba-f2=falla", "autoprueba-f3=falla", "autoprueba-g1=falla", "autoprueba-g2=falla", "autoprueba-h1=falla",
                    "autoprueba-h2=falla", "autoprueba-i=falla", "autoprueba-j=falla", "autoprueba-k1=falla", "autoprueba-k2=falla",
                },
                log);
        }

        // ------------------------------------------------------------------------------------------------- CT-15

        private const string NameWriter = @"(?<![\w.])\w+(?:\s*\.\s*\w+)*\s*\.\s*Name\s*=(?![=>])";

        private static IEnumerable<string> SourceFiles(string folder)
            => Directory.GetFiles(Path.Combine(Root, folder), "*.cs", SearchOption.AllDirectories)
                .Where(path => !path.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar)
                               && !path.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar))
                .OrderBy(path => path, StringComparer.Ordinal);

        /// <summary>M07: the WHOLE statement of each assignment (up to its semicolon), so a change of its right-hand side shows.</summary>
        private const string NameStatement = NameWriter + @"[^;]*;";

        private static List<(string File, string Text)> Assignments(IReadOnlyDictionary<string, string> cleanedFiles)
            => cleanedFiles
                .SelectMany(file => Regex.Matches(file.Value, NameStatement).Cast<Match>().Select(match => (file.Key, Collapse(match.Value))))
                .ToList();

        /// <summary>The text of a method, from its signature to its closing brace (the text has no comments and its literals are emptied).</summary>
        private static string MethodText(string text, string signature)
        {
            var start = text.IndexOf(signature, StringComparison.Ordinal);
            if (start < 0) return string.Empty;
            var open = text.IndexOf('{', start);
            if (open < 0) return string.Empty;
            var depth = 0;
            for (var index = open; index < text.Length; index++)
            {
                if (text[index] == '{') depth++;
                else if (text[index] == '}' && --depth == 0) return text.Substring(start, index - start + 1);
            }

            return string.Empty;
        }

        /// <summary>M07: the first writer must be INSIDE <c>DrawPushBackView</c>, not merely after its declaration.</summary>
        private static bool WriterInsideDrawPushBackView(string commands)
            => MethodText(commands, "static void DrawPushBackView(").Contains("system.Name = rackName;");

        private static bool OrderOf(string text, string method, string modal, string writer)
        {
            var start = text.IndexOf(method, StringComparison.Ordinal);
            if (start < 0) return false;
            var shown = text.IndexOf(modal, start, StringComparison.Ordinal);
            var written = text.IndexOf(writer, start, StringComparison.Ordinal);
            return shown >= 0 && written >= 0 && shown < written;
        }

        [Fact(Skip = "I-64 F4B PB-T4c: sustituida por su equivalente en PushBackEditAllNameWritersGuardTests (registro declarado superseding-characterization.md; decision del Owner owner-pbt4-decisions.md, pregunta 4).")]
        public void CT_PBT1B_15_TheWritersOfNameInThePluginRunAfterTheWindowCloses()
        {
            var log = new List<string>();
            var cleaned = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var path in SourceFiles("src/RackCad.Plugin"))
            {
                var text = Clean(File.ReadAllText(path));
                if (text.Contains("PushBack")) cleaned[Path.GetFileName(path)] = text;
            }

            var assignments = Assignments(cleaned);
            log.Add("archivos=" + cleaned.Count);
            log.Add("asignaciones=" + assignments.Count);
            foreach (var assignment in assignments) log.Add(assignment.File + ": " + assignment.Text);

            var pushBack = assignments.Where(a => a.File == "RackPushBackCommands.cs" || a.Text.StartsWith("pushBack", StringComparison.Ordinal)).ToList();
            log.Add("escritores-pushback=" + pushBack.Count);

            var commands = cleaned.ContainsKey("RackPushBackCommands.cs") ? cleaned["RackPushBackCommands.cs"] : string.Empty;
            var menu = cleaned.ContainsKey("RackMenuCommands.cs") ? cleaned["RackMenuCommands.cs"] : string.Empty;

            // The call to DrawPushBackView is the first occurrence (the declaration comes later); the writer is inside it (M07).
            var modalFirst = commands.IndexOf("AcApplication.ShowModalWindow(window);", StringComparison.Ordinal);
            var call = commands.IndexOf("DrawPushBackView(", StringComparison.Ordinal);
            var declaration = commands.IndexOf("static void DrawPushBackView(", StringComparison.Ordinal);
            log.Add("orden=" + (modalFirst >= 0 && modalFirst < call && call < declaration && WriterInsideDrawPushBackView(commands)));
            log.Add("orden=" + OrderOf(commands, "static void EditPushBack(", "AcApplication.ShowModalWindow(window);", "system.Name = name;"));
            log.Add("orden=" + OrderOf(menu, "public void RackCad()", "AcApplication.ShowModalWindow(menu);", "pushBack.System.Name = pushBackName;"));

            var calls = 0;
            foreach (var path in SourceFiles("src"))
            {
                var text = Clean(File.ReadAllText(path));
                calls += Regex.Matches(text, @"\bDrawPushBackView\s*\(").Cast<Match>()
                    .Count(match => !Regex.IsMatch(text.Substring(Math.Max(0, match.Index - 12), Math.Min(12, match.Index)), @"void\s+$"));
            }

            log.Add("DrawPushBackView-llamadas=" + calls);

            // Self-tests, over the real text with an injected change: an eighth assignment, and the writer of EditPushBack moved
            // before its ShowModalWindow.
            var injected = new Dictionary<string, string>(cleaned, StringComparer.Ordinal)
            {
                ["RackPushBackCommands.cs"] = Inject(commands, "state.Editor.SideA.WorkingBaseline.Name = \"x\";"),
            };
            log.Add("autoprueba-octava=" + (Assignments(injected).Count == 7 ? "pasa" : "falla"));

            var editStart = commands.IndexOf("static void EditPushBack(", StringComparison.Ordinal);
            var moved = commands;
            if (editStart >= 0)
            {
                var writer = moved.IndexOf("system.Name = name;", editStart, StringComparison.Ordinal);
                var modal = moved.IndexOf("AcApplication.ShowModalWindow(window);", editStart, StringComparison.Ordinal);
                if (writer >= 0 && modal >= 0)
                {
                    moved = moved.Remove(writer, "system.Name = name;".Length).Insert(modal, "system.Name = name; ");
                }
            }

            log.Add("autoprueba-orden=" + (OrderOf(moved, "static void EditPushBack(", "AcApplication.ShowModalWindow(window);", "system.Name = name;") ? "pasa" : "falla"));

            // M07: the writer of DrawPushBackView moved out of the method (to just before its declaration) no longer counts as inside it.
            var outside = commands;
            if (declaration >= 0)
            {
                var inside = outside.IndexOf("system.Name = rackName;", declaration, StringComparison.Ordinal);
                if (inside >= 0)
                {
                    outside = outside.Remove(inside, "system.Name = rackName;".Length).Insert(declaration, "system.Name = rackName; ");
                }
            }

            log.Add("autoprueba-escritor-fuera-de-DrawPushBackView=" + (WriterInsideDrawPushBackView(outside) ? "pasa" : "falla"));

            Assert.Equal(
                new[]
                {
                    "archivos=14",
                    "asignaciones=7",
                    "RackMenuCommands.cs: header.Configuration.Name = Systems.Shared.RackNewRackName.Resolve( document, global::RackCad.Domain.Systems.Shared.RackSystemKind.Selective, header.Configuration.Name);",
                    "RackMenuCommands.cs: dynamic.System.Name = dynamicName;",
                    "RackMenuCommands.cs: selective.System.Name = selectiveName;",
                    "RackMenuCommands.cs: cantilever.Design.Name = cantileverName;",
                    "RackMenuCommands.cs: pushBack.System.Name = pushBackName;",
                    "RackPushBackCommands.cs: system.Name = rackName;",
                    "RackPushBackCommands.cs: system.Name = name;",
                    "escritores-pushback=3",
                    "orden=True",
                    "orden=True",
                    "orden=True",
                    "DrawPushBackView-llamadas=1",
                    "autoprueba-octava=falla",
                    "autoprueba-orden=falla",
                    "autoprueba-escritor-fuera-de-DrawPushBackView=falla",
                },
                log);
        }
    }
}
