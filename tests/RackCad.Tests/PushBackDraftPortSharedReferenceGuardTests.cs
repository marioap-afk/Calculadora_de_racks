using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-64 F4B PB-T2a — the guard of the two shared references of decision D2 of the Owner (F4B-E11), condition 10, over the
    /// two files of the port (CT-PBT2A-04): G-L1 of CT-PBT1B-14, copied. It is a GUARD: it passes on the skeleton and on the
    /// implementation, and it discriminates through its fifteen negative self-tests, run over text injected in memory (never
    /// written).
    ///
    /// <para>
    /// The two shared references are the resolved rack (<c>PushBackSystem</c>) and the module report
    /// (<c>RackModuleReconciliationResult</c>). This is NOT authorization for general shallow copying: if a task needs to
    /// MUTATE either inside the draft lifetime, it stops and L-1 is reopened.
    /// </para>
    /// </summary>
    public sealed class PushBackDraftPortSharedReferenceGuardTests
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
            return new[] { push + "PushBackDraftStatePort.cs", push + "PushBackDraftEditorLaunch.cs" };
        }

        /// <summary>An injected line, inside a member appended to the real text of a file (never written to disk).</summary>
        private static string Inject(string source, string line)
        {
            var classEnd = source.LastIndexOf('}', source.LastIndexOf('}') - 1);
            return source.Substring(0, classEnd) + "void Inyectado() { " + line + " }\n" + source.Substring(classEnd);
        }

        // ------------------------------------------------------------------------------------------------ CT-PBT2A-04

        [Fact]
        public void CT_PBT2A_04_NoFileOfThePortMutatesTheResolvedRackOrTheModuleReport()
        {
            var log = new List<string>();
            var files = GuardedFiles().ToList();
            var existing = files.Where(path => File.Exists(Path.Combine(Root, path))).ToList();
            var texts = existing.Select(path => File.ReadAllText(Path.Combine(Root, path))).ToList();
            var violations = existing.Zip(texts, (path, text) => Violations(text).Select(v => path + ": " + v)).SelectMany(list => list).ToList();
            log.Add("reglas-sin-coincidencias=" + (existing.Count == files.Count && violations.Count == 0));

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

            // A self-test fails when the rules see the injected line in the text of EVERY file of the port.
            foreach (var test in selfTests)
            {
                var seen = texts.Count == files.Count && texts.All(text => Violations(Inject(text, test.Line)).Count > 0);
                log.Add("autoprueba-" + test.Name + "=" + (seen ? "falla" : "pasa"));
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
    }
}
