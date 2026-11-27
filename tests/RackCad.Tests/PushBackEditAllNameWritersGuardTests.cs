using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-64 F4B PB-T4c (owner-pbt4-decisions.md, pregunta 4; D-PBT4-15): the equivalent of CT-PBT1B-15 of PushBackSharedReferenceGuardTests, whose
    /// Skip is in the closed list of PB-T4c. It is that test with the members of its file that it reaches, literal, with one change: the census of
    /// the files of the Plugin that name PushBack has 15, the 14 of before and the opener of the full editor (RackWorkspaceEditorWindow.cs),
    /// which writes no Name; the seven assignments, the order and the self-tests do not change.
    /// </summary>
    public sealed class PushBackEditAllNameWritersGuardTests
    {
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

        private static string Root => PilotKit.RepoRoot().FullName;

        /// <summary>An injected line, inside a member appended to the real text of a file (never written to disk).</summary>
        private static string Inject(string source, string line)
        {
            var classEnd = source.LastIndexOf('}', source.LastIndexOf('}') - 1);
            return source.Substring(0, classEnd) + "void Inyectado() { " + line + " }\n" + source.Substring(classEnd);
        }

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

        [Fact]
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

            // T5 pb (I-64, quinta tanda): the census of the files of the Plugin that name PushBack grows from 15 to 19 with the door of Push Back
            // of the RackCad window (RackCadPushBackDoorCommands.cs, RackCadRackDoorPort.PushBack.cs, the line of Abandon in RackCadRackDoorPort.cs)
            // and its drawing half (RackPushBackWindowDrawing.cs). None of them writes a Name: the seven assignments and the order do not change.
            // T6 at (I-64, sexta tanda): 19 -> 21 with «Actualizar todo» (RackCadUpdateAllCommands.cs, which calls the same drawing half and
            // PushBackDoorRules.Named as RACKPUSHBACKAPLICAR, and RackCadRackDoorPort.UpdateAll.cs, which keeps the copy of the write). Neither
            // writes a Name: the seven assignments and the order do not change.
            Assert.Equal(
                new[]
                {
                    "archivos=21", // T5 pb; T6 at
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
