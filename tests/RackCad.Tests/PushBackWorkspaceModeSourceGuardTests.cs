using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-64 F4B PB-T2b - CT-PBT2B-08, la guarda de texto «ningun camino de WORKSPACE llega a la escritura clasica» (D-PBT2-06 a
    /// D-PBT2-11, OB-01.3): once reglas sobre el codigo de la clausura <c>src/RackCad.UI/Systems/PushBack</c>, cada una con su
    /// autoprueba en negativo, inyectada en MEMORIA (nunca se escribe un archivo).
    ///
    /// <para>El texto es el de cada archivo sin comentarios, con los literales vaciados y los espacios colapsados, partido en miembros
    /// (<c>Clean</c>, <c>Collapse</c> y <c>Members</c> de la guarda G-L1 de PB-T1b, copiados). El nombre de un miembro es el
    /// identificador justo antes del primer <c>(</c> de su cabecera. Un miembro que no existe da <c>regla-R&lt;n&gt;=False</c> y no
    /// lanza. Esta clase NO ve el ayudante de la red de UI (es otro proyecto de pruebas).</para>
    ///
    /// <para>Declarado, lo que la guarda no ve: una escritura clasica llegada por un camino que no contenga ninguno de los tres
    /// anotadores, la reflexion y un miembro con la cabecera partida en lineas que <c>Members</c> no separa. Lo que si fija es que
    /// los dos unicos miembros que anotan una peticion clasica empiezan por la guarda de WORKSPACE, y que «Aplicar al borrador»
    /// no llega a ninguno de ellos.</para>
    /// </summary>
    public sealed class PushBackWorkspaceModeSourceGuardTests
    {
        private const string Folder = "src/RackCad.UI/Systems/PushBack";
        private const string WindowName = "RackPushBackSystemWindow.xaml.cs";
        private const string CompositeName = "RackPushBackSystemWindow.Composite.cs";

        // ------------------------------------------------------------------------------------------------ G-L1c (copiado)

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

        /// <summary>An injected line, inside a member appended to the real text of a file (never written to disk).</summary>
        private static string Inject(string source, string line)
        {
            var classEnd = source.LastIndexOf('}', source.LastIndexOf('}') - 1);
            return source.Substring(0, classEnd) + "void Inyectado() { " + line + " }\n" + source.Substring(classEnd);
        }

        // ------------------------------------------------------------------------------------------------ la clausura

        private static string Root => PilotKit.RepoRoot().FullName;

        /// <summary>Los <c>.cs</c> de la clausura, por su nombre de archivo: W, WC y los demas de <c>Systems/PushBack</c>.</summary>
        private static Dictionary<string, string> Closure()
        {
            var files = new Dictionary<string, string>(StringComparer.Ordinal);
            var folder = Path.Combine(Root, Folder.Replace('/', Path.DirectorySeparatorChar));
            foreach (var path in Directory.GetFiles(folder, "*.cs", SearchOption.AllDirectories)
                         .Where(item => !item.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar)
                                        && !item.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar))
                         .OrderBy(item => item, StringComparer.Ordinal))
            {
                files[Path.GetFileName(path)] = File.ReadAllText(path);
            }

            return files;
        }

        // ------------------------------------------------------------------------------------------------ los miembros

        /// <summary>
        /// El miembro cuya cabecera casa con <paramref name="signature"/>: desde la cabecera hasta el final de su cuerpo (la llave que
        /// cierra, o el punto y coma de un miembro con flecha). Si no existe, nulo: la regla da <c>False</c> y no lanza.
        /// </summary>
        private static string Member(List<string> members, string signature)
        {
            foreach (var member in members)
            {
                var head = Regex.Match(member, signature);
                if (!head.Success) continue;

                var text = member.Substring(head.Index);
                var open = text.IndexOf('(');
                if (open < 0) return text;
                var depth = 0;
                var index = open;
                for (; index < text.Length; index++)
                {
                    if (text[index] == '(') depth++;
                    else if (text[index] == ')' && --depth == 0) break;
                }

                var brace = text.IndexOf('{', Math.Min(index, text.Length - 1));
                var arrow = text.IndexOf("=>", Math.Min(index, text.Length - 1), StringComparison.Ordinal);
                if (brace < 0 || (arrow >= 0 && arrow < brace))
                {
                    var end = text.IndexOf(';', Math.Min(index, text.Length - 1));
                    return end < 0 ? text : text.Substring(0, end + 1);
                }

                var level = 0;
                for (var position = brace; position < text.Length; position++)
                {
                    if (text[position] == '{') level++;
                    else if (text[position] == '}' && --level == 0) return text.Substring(0, position + 1);
                }

                return text;
            }

            return null;
        }

        /// <summary>El cuerpo de un miembro: desde su primera llave. Nulo si el miembro no existe o no tiene cuerpo de bloque.</summary>
        private static string Body(string member)
        {
            if (member == null) return null;
            var open = member.IndexOf('{');
            return open < 0 ? null : member.Substring(open);
        }

        private static string Sig(string kind, string name) => @"\b(?:private|internal|public)\s+" + kind + @"\s+" + name + @"\s*\(";

        private static readonly HashSet<string> Keywords = new HashSet<string>(StringComparer.Ordinal)
        {
            "if", "for", "foreach", "while", "switch", "catch", "using", "lock", "fixed", "return", "new", "else", "when",
        };

        /// <summary>El nombre del miembro (el identificador de su cabecera) que contiene el texto de la posicion <paramref name="index"/>.</summary>
        private static string EnclosingName(string member, int index)
        {
            string name = null;
            foreach (Match match in Regex.Matches(member.Substring(0, index), @"(?:[\w<>\[\],.?]+\s+)+(\w+)\s*\([^()]*\)\s*\{"))
            {
                if (!Keywords.Contains(match.Groups[1].Value)) name = match.Groups[1].Value;
            }

            return name;
        }

        // ------------------------------------------------------------------------------------------------ las reglas

        private static readonly string[] RequestTokens = { "RequestUpdate", "RequestInsert", "RequestInsertViews" };

        private const string Guard = "{ if (RefuseInWorkspace()) return;";

        private const string LoadGuard =
            "{ if (mode == PushBackEditorMode.Workspace) throw new InvalidOperationException(WorkspaceLoadRefusalMessage);";

        private const string BomCall = "new RackBomWindow(lastComputation.Bom) { Owner = this }.ShowDialog();";

        /// <summary>Los once literales de D-PBT2-09, tal como los fija el paquete.</summary>
        private static readonly (string Name, string Literal)[] Constants =
        {
            ("WorkspaceApplyLabel", "Aplicar al borrador"),
            ("WorkspaceApplyToolTip", "Guarda los cambios en el borrador. El dibujo cambia con «Actualizar» del panel."),
            ("WorkspaceApplyBlockedToolTip", "Corrige el diagnóstico bloqueante: el rack no puede pasar al borrador así."),
            ("WorkspaceRefusalMessage", "En el editor del borrador no se dibuja: aplica al borrador y actualiza desde el panel."),
            ("WorkspaceLoadRefusalMessage", "La ventana del borrador solo carga el estado del borrador."),
            ("WorkspaceInvalidMessage", "Corrige los datos: no se puede aplicar al borrador un modelo inválido."),
            ("WorkspaceBlockedMessage", "Corrige los datos: no se puede aplicar al borrador un modelo bloqueado."),
            ("WorkspaceNothingToApplyMessage", "No hay cambios que aplicar al borrador."),
            ("WorkspaceAppliedMessage", "Cambios aplicados al borrador. El dibujo cambia con «Actualizar» del panel."),
            ("WorkspaceNotEditableMessage", "El borrador no admite cambios ahora: cierra el editor y revisa el panel."),
            ("WorkspaceUnappliedQuestion", "Hay cambios sin aplicar al borrador que se perderán al cerrar. ¿Deseas continuar?"),
        };

        /// <summary>Las once reglas sobre los archivos de la clausura (nombre de archivo, texto). Una regla sin su miembro es <c>False</c>.</summary>
        private static bool[] Rules(IReadOnlyDictionary<string, string> files)
        {
            var result = new bool[12];
            var window = files.TryGetValue(WindowName, out var found) ? found : string.Empty;
            var flat = Collapse(Clean(window));
            var windowMembers = Members(Clean(window));

            // R1 y R2: los dos miembros que anotan una peticion clasica empiezan por la guarda.
            var draw = Member(windowMembers, Sig("void", "RequestDraw"));
            var batch = Member(windowMembers, Sig("void", "InsertBatch_Click"));
            result[1] = Body(draw) != null && Body(draw).StartsWith(Guard, StringComparison.Ordinal);
            result[2] = Body(batch) != null && Body(batch).StartsWith(Guard, StringComparison.Ordinal);

            // R3: en TODA la clausura, los miembros que anotan una peticion clasica son exactamente esos dos, de W.
            var annotators = new List<string>();
            foreach (var file in files)
            {
                foreach (var member in Members(Clean(file.Value)))
                {
                    foreach (Match call in Regex.Matches(member, @"\.\s*(?:" + string.Join("|", RequestTokens) + @")\s*\("))
                    {
                        annotators.Add(file.Key + ":" + EnclosingName(member, call.Index));
                    }
                }
            }

            result[3] = annotators.Distinct().OrderBy(item => item, StringComparer.Ordinal)
                .SequenceEqual(new[] { WindowName + ":InsertBatch_Click", WindowName + ":RequestDraw" });

            // R4: «Aplicar al borrador» no llega a ninguna ruta clasica y entrega una copia al puerto, una vez.
            var apply = Member(windowMembers, Sig("void", "ApplyToDraft"));
            var forbidden = new[]
            {
                "RequestUpdate(", "RequestInsert", "RequestDraw(", "Close();", "SetModel(", "InsertionRequest", "InsertRequested", "ShowDialog",
                "SetName(", "SyncName(",
            };
            const string handOver = "draftPort.ApplyToDraft(PushBackEditableStateComparer.Copy(editable))";
            result[4] = apply != null && forbidden.All(token => !apply.Contains(token))
                        && Regex.Matches(apply, Regex.Escape(handOver)).Count == 1;

            // R5: el modo es un campo de solo lectura que asigna, una vez, el constructor privado de WORKSPACE.
            var constructor = Member(windowMembers, @"\bprivate\s+RackPushBackSystemWindow\s*\(\s*PushBackDraftEditorLaunch\s+\w+\s*\)");
            var assignments = Regex.Matches(flat, @"(?<![\w.])mode\s*=(?![=>])").Count;
            result[5] = Regex.Matches(flat, Regex.Escape("private readonly PushBackEditorMode mode;")).Count == 1
                        && assignments == 1
                        && constructor != null
                        && Regex.Matches(constructor, Regex.Escape("mode = PushBackEditorMode.Workspace;")).Count == 1;

            // R6: las dos firmas publicas de hoy, la factoria, y ningun parametro que sea un modo disfrazado de booleano.
            result[6] = flat.Contains("public RackPushBackSystemWindow() : this(false)")
                        && flat.Contains("public RackPushBackSystemWindow(bool canInsertInAutoCad, Func<string> newIdFactory = null)")
                        && flat.Contains("public static RackPushBackSystemWindow ForDraft(PushBackDraftEditorLaunch launch)")
                        && !Regex.IsMatch(
                            flat,
                            @"\b(?:PushBackEditorMode\s+\w+|bool\s+\w*(?:workspace|draft|borrador)\w*)\s*(?:[,)]|=(?!>))",
                            RegexOptions.IgnoreCase);

            // R7: la costura del BOM va antes de la linea S-7, y la de la biblioteca es el primer termino del ternario de S-9.
            var bom = Member(windowMembers, Sig("void", "Bom_Click"));
            var library = Member(windowMembers, Sig("void", "SaveLibrary_Click"));
            var seam = bom == null ? -1 : bom.IndexOf("if (BomDialog != null)", StringComparison.Ordinal);
            var s7 = bom == null ? -1 : bom.IndexOf(BomCall, StringComparison.Ordinal);
            result[7] = seam >= 0 && s7 > seam
                        && library != null
                        && library.Contains("var path = LibraryPathDialog != null ? LibraryPathDialog(this, NameBox.Text) :");

            // R8: las tres cargas publicas empiezan por la guarda de WORKSPACE.
            result[8] = new[] { "LoadNew", "LoadDesignForNew", "LoadExisting" }
                .All(name => Body(Member(windowMembers, Sig("void", name))) != null
                             && Body(Member(windowMembers, Sig("void", name))).StartsWith(LoadGuard, StringComparison.Ordinal));

            // R9: el cuerpo literal de «Actualizar».
            result[9] = Member(windowMembers, Sig("void", "Update_Click"))
                        == "private void Update_Click(object sender, RoutedEventArgs e) { if (mode == PushBackEditorMode.Workspace) { ApplyToDraft(); return; } RequestDraw(null, -1, updateOnly: true); }";

            // R10: los once literales, cada uno una vez y entero (el texto sin comentarios, con los literales vivos).
            var withLiterals = Collapse(window);
            result[10] = Constants.All(constant => Regex.Matches(
                withLiterals, Regex.Escape("private const string " + constant.Name + " = \"" + constant.Literal + "\";")).Count == 1);

            // R11: «Aplicar al estado de edicion» se llama solo desde RequestDraw y ApplyToDraft, y ni «Aplicar» ni su dirty validan por su cuenta.
            var callers = new List<string>();
            foreach (var member in Members(Clean(window)))
            {
                foreach (Match call in Regex.Matches(member, @"(?<!\bbool\s)\bApplyEditsToEditingState\s*\("))
                {
                    callers.Add(EnclosingName(member, call.Index));
                }
            }

            var dirty = Member(windowMembers, Sig("bool", "HasUnappliedDraftChanges"));
            var own = new[] { "AllFieldsValid(", "OutputBlockedAfterRecompute(", "currentInputsAreValid", "HasError" };
            result[11] = callers.Distinct().OrderBy(item => item, StringComparer.Ordinal).SequenceEqual(new[] { "ApplyToDraft", "RequestDraw" })
                         && apply != null && own.All(token => !apply.Contains(token))
                         && dirty != null && own.All(token => !dirty.Contains(token));

            return result;
        }

        // ------------------------------------------------------------------------------------------------ las autopruebas

        private static Dictionary<string, string> With(IReadOnlyDictionary<string, string> files, string name, Func<string, string> edit)
        {
            var copy = new Dictionary<string, string>(files, StringComparer.Ordinal);
            copy[name] = edit(files.TryGetValue(name, out var text) ? text : string.Empty);
            return copy;
        }

        private static string Replace(string text, string pattern, string replacement)
            => new Regex(pattern, RegexOptions.Singleline).Replace(text, replacement, 1);

        [Fact]
        public void CT_PBT2B_08_NoWorkspacePathReachesTheClassicWritePath()
        {
            var log = new List<string>();
            var files = Closure();
            var rules = Rules(files);
            for (var number = 1; number <= 11; number++)
            {
                log.Add("regla-R" + number + "=" + rules[number]);
            }

            // Autopruebas en negativo, sobre el texto real con un cambio en memoria. Cada una rompe SU regla.
            var selfTests = new (string Name, int Rule, Dictionary<string, string> Files)[]
            {
                ("R1", 1, With(files, WindowName, text => Replace(
                    text, @"(void\s+RequestDraw\s*\([^)]*\)\s*\{\s*)if\s*\(\s*RefuseInWorkspace\s*\(\s*\)\s*\)\s*return\s*;", "$1"))),
                ("R2", 2, With(files, WindowName, text => Replace(
                    text, @"(void\s+InsertBatch_Click\s*\([^)]*\)\s*\{\s*)if\s*\(\s*RefuseInWorkspace\s*\(\s*\)\s*\)\s*return\s*;", "$1"))),
                ("R3", 3, With(files, CompositeName, text => Inject(text, "session.RequestInsert(null, -1, ctx => null);"))),
                ("R4", 4, With(files, WindowName, text => Replace(text, @"(void\s+ApplyToDraft\s*\(\s*\)\s*\{)", "$1 SyncName();"))),
                ("R5", 5, With(files, WindowName, text => Replace(text, @"(void\s+UpdateButtons\s*\(\s*\)\s*\{)", "$1 mode = PushBackEditorMode.Classic;"))),
                ("R6", 6, With(files, WindowName, text => Replace(
                    text, @"Func<string>\s+newIdFactory\s*=\s*null\s*\)", "Func<string> newIdFactory = null, bool workspace = false)"))),
                ("R7", 7, With(files, WindowName, text => Replace(
                    text,
                    @"(if\s*\(\s*BomDialog\s*!=\s*null\s*\)\s*\{[^}]*\})(\s*)(new\s+RackBomWindow\s*\(\s*lastComputation\.Bom\s*\)\s*\{\s*Owner\s*=\s*this\s*\}\s*\.ShowDialog\s*\(\s*\)\s*;)",
                    "$3$2$1"))),
                ("R8", 8, With(files, WindowName, text => Replace(
                    text,
                    @"(void\s+LoadExisting\s*\([^)]*\)\s*\{\s*)if\s*\(\s*mode\s*==\s*PushBackEditorMode\.Workspace\s*\)\s*throw\s+new\s+InvalidOperationException\s*\(\s*WorkspaceLoadRefusalMessage\s*\)\s*;",
                    "$1"))),
                ("R9", 9, With(files, WindowName, text => Replace(
                    text, @"(void\s+Update_Click\s*\([^)]*\)\s*\{[^}]*?)ApplyToDraft\s*\(\s*\)\s*;", "$1RequestDraw(null, -1, updateOnly: true);"))),
                ("R10", 10, With(files, WindowName, text => Replace(
                    text, @"(WorkspaceApplyLabel\s*=\s*"")Aplicar al borrador("")", "$1Aplicar al borradorX$2"))),
                ("R11a", 11, With(files, WindowName, text => Replace(text, @"(void\s+ApplyToDraft\s*\(\s*\)\s*\{)", "$1 if (!currentInputsAreValid) return;"))),
                ("R11b", 11, With(files, WindowName, text => Replace(
                    text, @"if\s*\(\s*!\s*ApplyEditsToEditingState\s*\([^;]*?\)\s*\)\s*return\s*;", string.Empty))),
            };

            foreach (var test in selfTests)
            {
                log.Add("autoprueba-" + test.Name + "=" + (Rules(test.Files)[test.Rule] ? "pasa" : "falla"));
            }

            var expected = new List<string>();
            for (var number = 1; number <= 11; number++)
            {
                expected.Add("regla-R" + number + "=True");
            }

            foreach (var name in new[] { "R1", "R2", "R3", "R4", "R5", "R6", "R7", "R8", "R9", "R10", "R11a", "R11b" })
            {
                expected.Add("autoprueba-" + name + "=falla");
            }

            Assert.Equal(expected, log);
        }
    }
}
