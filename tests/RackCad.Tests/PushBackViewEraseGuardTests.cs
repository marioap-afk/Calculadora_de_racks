using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-64 F4B PB-T3c — the TEXT GUARDS of the erasure of a view inside the one scope of MUTATE (CT-PBT3C-01 to CT-PBT3C-04, one [Fact]
    /// each). The Core test project cannot reference the Plugin (it needs AutoCAD), so the facade and the writer are read as text from the
    /// tree of the repository. Each guard ends in ONE Assert.Equal of its lines joined by a line feed, and the guards of CT-01 and CT-04
    /// are followed by their self-tests, run over text injected in memory (never written): a guard that never fails watches nothing.
    ///
    /// <para>
    /// The facade is <c>RackViewBlockEraser.EraseInTransaction</c>: caller-owned (no lock, no scope of its own, no confirmation and no
    /// tidy-up), with the erasure of the direct references and the capture of the nested definitions of the classic erasure. The writer
    /// uses it from <c>EraseUnit</c> of the scope of MUTATE, and PREPARE and the re-read of MUTATE cover the views to erase (D-PBT3-11).
    /// </para>
    /// </summary>
    public sealed class PushBackViewEraseGuardTests
    {
        private const string WriterPath = "src/RackCad.Plugin/Views/RackPilotUpdateWriter.cs";
        private const string EraserPath = "src/RackCad.Plugin/Views/RackViewBlockEraser.cs";
        private const string CommandsPath = "src/RackCad.Plugin/RackPushBackCommands.cs";

        private const string Signature = "internal static IReadOnlyCollection<ObjectId> EraseInTransaction(Transaction transaction, ObjectId definitionId)";
        private const string PrimitiveCall = "RackViewBlockEraser.EraseInTransaction(";

        // What the facade never has, even in a comment: the lock, a scope of its own, the confirmation, the tidy-up, the regeneration, the
        // rename and the name of the product that uses it (CT-PBT1B-15 counts the files of the Plugin that carry that name).
        private static readonly string[] ForbiddenTerms =
        {
            "LockDocument", "StartTransaction", "StartOpenCloseTransaction", ".Commit(", "Purge", "Regen", "SyncName", "PushBack",
        };

        // NamesThePilot of G-9 (IntegrationGuard:679). The package writes it in a table cell, with the pipes escaped:
        // PushBackPilot\w*\|RackPilot\w*\|RackCad\.Application\.Systems\.PushBack\.Pilot
        private static readonly Regex NamesThePilot = new Regex(@"PushBackPilot\w*|RackPilot\w*|RackCad\.Application\.Systems\.PushBack\.Pilot");

        // ------------------------------------------------------------------------------------------------- the helpers

        private static string Root => PilotKit.RepoRoot().FullName;

        private static string N(int value) => value.ToString(CultureInfo.InvariantCulture);

        /// <summary>The text of a file of the tree, or nothing when it is not there (a guard never throws: it fails by its assertion).</summary>
        private static string Read(string relative)
        {
            var full = Path.Combine(Root, relative.Replace('/', Path.DirectorySeparatorChar));
            return File.Exists(full) ? File.ReadAllText(full) : string.Empty;
        }

        private static string Code(string text)
            => Regex.Replace(Regex.Replace(text, @"/\*.*?\*/", string.Empty, RegexOptions.Singleline), @"//[^\r\n]*", string.Empty);

        private static string Flat(string text)
            => string.Join(" ", text.Split((char[])null, StringSplitOptions.RemoveEmptyEntries));

        private static int Count(string text, string literal)
        {
            var count = 0;
            var at = 0;
            while ((at = text.IndexOf(literal, at, StringComparison.Ordinal)) >= 0)
            {
                count++;
                at += literal.Length;
            }

            return count;
        }

        private static int CountOf(string text, string pattern) => Regex.Matches(text, pattern).Count;

        /// <summary>The text of a member from its signature to its closing brace, or nothing when the signature is not in the text.</summary>
        private static string BodyOf(string code, string signaturePattern)
        {
            var match = Regex.Match(code, signaturePattern);
            if (!match.Success) return string.Empty;
            var open = code.IndexOf('{', match.Index + match.Length - 1);
            if (open < 0) return string.Empty;
            var depth = 0;
            for (var index = open; index < code.Length; index++)
            {
                if (code[index] == '{') depth++;
                else if (code[index] == '}' && --depth == 0) return code.Substring(match.Index, index - match.Index + 1);
            }

            return string.Empty;
        }

        /// <summary>The steps found IN ORDER, each one after the one before it, until the first that is missing.</summary>
        private static List<string> OrderedSteps(string flatCode, IEnumerable<(string Name, string Pattern)> steps)
        {
            var found = new List<string>();
            var from = 0;
            foreach (var step in steps)
            {
                var match = Regex.Match(flatCode.Substring(from), step.Pattern);
                if (!match.Success) break;
                found.Add(step.Name);
                from += match.Index + match.Length;
            }

            return found;
        }

        private static string Show(IEnumerable<string> items) => "[" + string.Join(",", items) + "]";

        // ------------------------------------------------------------------------------------------------- CT-PBT3C-01

        // The body of the facade, in its order (D-PBT3-11): the definition for reading, the layout that throws, the nested definitions, the
        // direct references opened for writing and erased, and the definition upgraded and erased.
        private static readonly (string Name, string Pattern)[] EraserSteps =
        {
            ("GetObject-ForRead", @"\.GetObject\(\s*definitionId\s*,\s*OpenMode\.ForRead\s*\)"),
            ("IsLayout", @"\.IsLayout\b"),
            ("lanza", @"throw new InvalidOperationException\("),
            ("hijas", @"foreach\s*\(\s*ObjectId\s+\w+\s+in\s+definition\s*\)"),
            ("anidadas", @"\.Add\(\s*\w+\.BlockTableRecord\s*\)"),
            ("GetBlockReferenceIds", @"\.GetBlockReferenceIds\(\s*directOnly:\s*true\s*,\s*forceValidity:\s*true\s*\)"),
            ("ForWrite", @"OpenMode\.ForWrite"),
            ("Erase-de-cada-referencia", @"\.Erase\(\s*\)"),
            ("UpgradeOpen", @"\.UpgradeOpen\(\s*\)"),
            ("Erase-de-la-definicion", @"\bdefinition\.Erase\(\s*\)"),
            ("devuelve-las-anidadas", @"\breturn\s+\w+\s*;"),
        };

        /// <summary>The text of a facade that is as the package says. It is the control of the self-tests, and the mutants start from it.</summary>
        private const string Model =
            "#nullable enable\n"
            + "using System;\n"
            + "using System.Collections.Generic;\n"
            + "using Autodesk.AutoCAD.DatabaseServices;\n"
            + "\n"
            + "namespace RackCad.Plugin.Views\n"
            + "{\n"
            + "    internal static class RackViewBlockEraser\n"
            + "    {\n"
            + "        internal static IReadOnlyCollection<ObjectId> EraseInTransaction(Transaction transaction, ObjectId definitionId)\n"
            + "        {\n"
            + "            if (!(transaction.GetObject(definitionId, OpenMode.ForRead) is BlockTableRecord definition) || definition.IsLayout)\n"
            + "                throw new InvalidOperationException(\"La definición no es un bloque.\");\n"
            + "\n"
            + "            var nested = new HashSet<ObjectId>();\n"
            + "            foreach (ObjectId childId in definition)\n"
            + "            {\n"
            + "                if (transaction.GetObject(childId, OpenMode.ForRead) is BlockReference child && !child.BlockTableRecord.IsNull)\n"
            + "                    nested.Add(child.BlockTableRecord);\n"
            + "            }\n"
            + "\n"
            + "            foreach (ObjectId referenceId in definition.GetBlockReferenceIds(directOnly: true, forceValidity: true))\n"
            + "            {\n"
            + "                if (transaction.GetObject(referenceId, OpenMode.ForWrite) is BlockReference reference) reference.Erase();\n"
            + "            }\n"
            + "\n"
            + "            definition.UpgradeOpen();\n"
            + "            definition.Erase();\n"
            + "            return nested;\n"
            + "        }\n"
            + "    }\n"
            + "}\n";

        private static List<string> EraserProhibitions(string raw)
        {
            var found = ForbiddenTerms.Where(term => raw.Contains(term, StringComparison.Ordinal)).ToList();
            if (NamesThePilot.IsMatch(raw)) found.Add("NamesThePilot");
            return found;
        }

        private static List<string> EraserOrder(string raw) => OrderedSteps(Flat(Code(raw)), EraserSteps);

        private static string EraserShape(string raw)
        {
            var code = Code(raw);
            return "clase=" + N(CountOf(code, @"\binternal\s+static\s+class\s+RackViewBlockEraser\b"))
                + " metodo=" + N(CountOf(code, @"\bEraseInTransaction\s*\("))
                + " firma=" + N(Count(Flat(code), Signature))
                + " declaraciones=" + N(CountOf(code, @"\b(?:internal|private|public|protected)\b"));
        }

        [Fact]
        public void CT_PBT3C_01_TheFacadeErasesOneViewInTheScopeOfTheCallerAndNamesNothingOfTheLockTheScopeTheTidyUpOrThePilot()
        {
            var log = new List<string>();
            var expected = new List<string>();
            var real = Read(EraserPath);
            var order = string.Join(",", EraserSteps.Select(step => step.Name));

            log.Add("fachada existe=" + (real.Length > 0));
            expected.Add("fachada existe=True");
            log.Add("fachada " + EraserShape(real));
            expected.Add("fachada clase=1 metodo=1 firma=1 declaraciones=2");
            var code = Flat(Code(real));
            log.Add("fachada orden=" + Show(EraserOrder(real)));
            expected.Add("fachada orden=[" + order + "]");
            log.Add("fachada erase=" + N(CountOf(code, @"\.Erase\(\s*\)")) + " upgradeopen=" + N(CountOf(code, @"\.UpgradeOpen\(\s*\)"))
                + " referencias=" + N(CountOf(code, @"\.GetBlockReferenceIds\(")));
            expected.Add("fachada erase=2 upgradeopen=1 referencias=1");
            log.Add("fachada prohibiciones=" + Show(EraserProhibitions(real)));
            expected.Add("fachada prohibiciones=[]");

            // The control of the self-tests: a text that is as the package says passes both checks, and the mutants below do not.
            log.Add("control-modelo prohibiciones=" + Show(EraserProhibitions(Model)) + " orden=" + (EraserOrder(Model).Count == EraserSteps.Length ? "completo" : "roto")
                + " " + EraserShape(Model));
            expected.Add("control-modelo prohibiciones=[] orden=completo clase=1 metodo=1 firma=1 declaraciones=2");

            // One prohibition at a time, injected into the real text of the facade and into the model.
            var prohibitions = new (string Name, string Line)[]
            {
                ("LockDocument", "using (document.LockDocument()) { }"),
                ("StartTransaction", "var own = database.TransactionManager.StartTransaction();"),
                ("StartOpenCloseTransaction", "var reading = database.TransactionManager.StartOpenCloseTransaction();"),
                ("Commit", "transaction.Commit();"),
                ("Purge", "database.Purge(ids);"),
                ("Regen", "editor.Regen();"),
                ("SyncName", "RackBlockRenamer.SyncName(document, id, name);"),
                ("PushBack", "var text = \"PushBack\";"),
                ("NamesThePilot", "RackPilotUpdateWriter writer = null;"),
            };
            foreach (var prohibition in prohibitions)
            {
                foreach (var (target, text) in new[] { ("fachada", real), ("modelo", Model) })
                {
                    log.Add("autoprueba-" + prohibition.Name + "-en-" + target + "=" + (EraserProhibitions(text + "\n" + prohibition.Line + "\n").Count > 0 ? "falla" : "pasa"));
                    expected.Add("autoprueba-" + prohibition.Name + "-en-" + target + "=falla");
                }
            }

            // The comments count too: the facade does not even mention them.
            log.Add("autoprueba-comentario-en-modelo=" + (EraserProhibitions(Model + "\n// The tidy-up: Purge.\n").Count > 0 ? "falla" : "pasa"));
            expected.Add("autoprueba-comentario-en-modelo=falla");

            // The order and the content of the body: a step removed, changed or moved makes the order fail.
            var mutants = new (string Name, string Text)[]
            {
                ("sin-IsLayout", Model.Replace(" || definition.IsLayout", string.Empty, StringComparison.Ordinal)),
                ("sin-anidadas", Model.Replace("nested.Add(child.BlockTableRecord);", "nested.Clear();", StringComparison.Ordinal)),
                ("referencias-no-directas", Model.Replace("directOnly: true", "directOnly: false", StringComparison.Ordinal)),
                ("referencias-sin-ForWrite", Model.Replace("OpenMode.ForWrite", "OpenMode.ForRead", StringComparison.Ordinal)),
                ("sin-UpgradeOpen", Model.Replace("definition.UpgradeOpen();", string.Empty, StringComparison.Ordinal)),
                ("definicion-antes-de-las-referencias", Model.Replace("            definition.UpgradeOpen();\n            definition.Erase();\n", string.Empty, StringComparison.Ordinal)
                    .Replace("            foreach (ObjectId referenceId", "            definition.UpgradeOpen();\n            definition.Erase();\n            foreach (ObjectId referenceId", StringComparison.Ordinal)),
            };
            foreach (var mutant in mutants)
            {
                log.Add("autoprueba-orden-" + mutant.Name + "=" + (EraserOrder(mutant.Text).Count == EraserSteps.Length ? "pasa" : "falla"));
                expected.Add("autoprueba-orden-" + mutant.Name + "=falla");
            }

            Assert.Equal(string.Join("\n", expected), string.Join("\n", log));
        }

        // ------------------------------------------------------------------------------------------------- CT-PBT3C-02

        private static readonly (string Name, string Pattern)[] EraseUnitSteps =
        {
            ("EnsureOwned", Regex.Escape("EnsureOwned();")),
            ("busca-en-_erasures", @"\b_erasures\b"),
            ("lanza", @"throw new InvalidOperationException\("),
            ("_units++", Regex.Escape("_units++;")),
            ("FailUnitIfRequested", Regex.Escape("FailUnitIfRequested(_units);")),
            ("EraseInTransaction", Regex.Escape("RackViewBlockEraser.EraseInTransaction(_transaction,")),
            ("_staleDefinitions", @"\b_staleDefinitions\b"),
            ("resultado-vacio", @"new\s+PushBackPilotUnitResult\(\s*Array\.Empty<string>\(\)\s*\)"),
        };

        private const string EraseUnitSignature = @"public\s+PushBackPilotUnitResult\s+EraseUnit\s*\(\s*string\s+memberKey\s*\)";

        [Fact]
        public void CT_PBT3C_02_EraseUnitOfTheScopeOfMutateEnsuresTheScopeLooksTheKeyUpCountsTheUnitAndErasesThroughTheFacade()
        {
            var raw = Read(WriterPath);
            var code = Code(raw);
            var mutation = BodyOf(code, @"internal\s+sealed\s+class\s+RackPilotMutationScope\b");
            var body = BodyOf(mutation, EraseUnitSignature);
            var flat = Flat(body);
            var log = new List<string>
            {
                "escritor ambito-implementa-IPushBackPilotViewEraser=" + Regex.IsMatch(code, @"class\s+RackPilotMutationScope\s*:\s*IPushBackPilotMutationScope\s*,\s*IPushBackPilotViewEraser\b"),
                "escritor EraseUnit=" + N(CountOf(mutation, EraseUnitSignature)),
                "escritor EraseUnit-orden=" + Show(OrderedSteps(flat, EraseUnitSteps)),
                "escritor EraseUnit-llamadas-a-la-fachada=" + N(Count(flat, PrimitiveCall)),
                "escritor EraseUnit-sin-nombre-de-la-purga-ni-del-renombrado-ni-de-la-regeneracion-ni-de-la-confirmacion="
                    + !Regex.IsMatch(body, @"SyncName|Purge|Regen|\.Commit\s*\(|StartTransaction|StartOpenCloseTransaction|LockDocument|CaptureDrawing"),
                "escritor Conditional-DEBUG=" + N(CountOf(raw, @"\[Conditional\(""DEBUG""\)\]")),
            };
            var expected = new List<string>
            {
                "escritor ambito-implementa-IPushBackPilotViewEraser=True",
                "escritor EraseUnit=1",
                "escritor EraseUnit-orden=[" + string.Join(",", EraseUnitSteps.Select(step => step.Name)) + "]",
                "escritor EraseUnit-llamadas-a-la-fachada=1",
                "escritor EraseUnit-sin-nombre-de-la-purga-ni-del-renombrado-ni-de-la-regeneracion-ni-de-la-confirmacion=True",
                "escritor Conditional-DEBUG=2",
            };

            Assert.Equal(string.Join("\n", expected), string.Join("\n", log));
        }

        // ------------------------------------------------------------------------------------------------- CT-PBT3C-03

        [Fact]
        public void CT_PBT3C_03_PrepareResolvesTheKeysToEraseAndTheRereadOfMutateCoversThem()
        {
            var code = Code(Read(WriterPath));
            var update = BodyOf(code, @"internal\s+sealed\s+class\s+RackPilotUpdateScope\b");
            var mutation = BodyOf(code, @"internal\s+sealed\s+class\s+RackPilotMutationScope\b");
            var prepare = BodyOf(update, @"public\s+PushBackPilotImportResult\s+PrepareUnits\s*\(");
            var loop = BodyOf(prepare, @"foreach\s*\(\s*var\s+\w+\s+in\s+plan\.Erasures\s*\)");
            var begin = BodyOf(update, @"public\s+PushBackPilotMutationStart\s+BeginMutation\s*\(");
            var constructor = Regex.Match(mutation, @"internal\s+RackPilotMutationScope\s*\(([\s\S]*?)\)\s*\{");
            var reread = BodyOf(mutation, @"public\s+PushBackPilotScanValues\s+Reread\s*\(");
            var covered = Regex.Matches(reread, @"foreach\s*\(\s*var\s+pair\s+in\s+(_prepared|_erasures)\s*\)").Cast<Match>().Select(match => match.Groups[1].Value);

            var log = new List<string>
            {
                "update-scope _erasures-declarado=" + N(CountOf(update, @"private\s+readonly\s+Dictionary<string,\s*ObjectId>\s+_erasures\s*=\s*new\(StringComparer\.Ordinal\)\s*;")),
                "update-scope PrepareUnits recorre-plan.Erasures=" + N(CountOf(prepare, @"foreach\s*\(\s*var\s+\w+\s+in\s+plan\.Erasures\s*\)")),
                "update-scope PrepareUnits resuelve-en-_definitions-y-guarda-en-_erasures="
                    + (Regex.IsMatch(loop, @"_definitions\.TryGetValue\(\s*\w+\s*,\s*out\s+var\s+\w+\s*\)") && Regex.IsMatch(loop, @"_erasures\[\s*\w+\s*\]\s*=")),
                "update-scope PrepareUnits Failed-con-el-texto-de-hoy=" + N(CountOf(prepare, @"Failed\(\s*""La definición ""\s*\+\s*\w+(?:\.\w+)*\s*\+\s*"" ya no está en el dibujo\.""\s*\)")),
                "update-scope PrepareUnits PrepareRedraw-para-las-erasures=" + N(CountOf(loop, @"\bPrepareRedraw\s*\(")),
                "update-scope PrepareUnits PrepareRedraw-en-total=" + N(CountOf(prepare, @"\bPrepareRedraw\s*\(")),
                "update-scope BeginMutation pasa-_erasures=" + Regex.IsMatch(begin, @"new\s+RackPilotMutationScope\s*\([^;]*\b_erasures\b[^;]*\)\s*\)\s*;"),
                "mutation-scope constructor-recibe-erasures=" + (constructor.Success && Regex.IsMatch(constructor.Groups[1].Value, @"ObjectId>\s+erasures\b")),
                "mutation-scope _erasures-declarado=" + N(CountOf(mutation, @"private\s+readonly\s+IReadOnlyDictionary<string,\s*ObjectId>\s+_erasures\s*;")),
                "mutation-scope Reread matches-sobre=" + Show(covered) + " asignaciones=" + N(CountOf(reread, @"matches\[\s*pair\.Key\s*\]\s*=")),
            };
            var expected = new List<string>
            {
                "update-scope _erasures-declarado=1",
                "update-scope PrepareUnits recorre-plan.Erasures=1",
                "update-scope PrepareUnits resuelve-en-_definitions-y-guarda-en-_erasures=True",
                "update-scope PrepareUnits Failed-con-el-texto-de-hoy=2",
                "update-scope PrepareUnits PrepareRedraw-para-las-erasures=0",
                "update-scope PrepareUnits PrepareRedraw-en-total=3",
                "update-scope BeginMutation pasa-_erasures=True",
                "mutation-scope constructor-recibe-erasures=True",
                "mutation-scope _erasures-declarado=1",
                "mutation-scope Reread matches-sobre=[_prepared,_erasures] asignaciones=2",
            };

            Assert.Equal(string.Join("\n", expected), string.Join("\n", log));
        }

        // ------------------------------------------------------------------------------------------------- CT-PBT3C-04

        private static IEnumerable<string> SourceFiles()
            => Directory.GetFiles(Path.Combine(Root, "src"), "*.cs", SearchOption.AllDirectories)
                .Where(path => !path.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar)
                               && !path.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar))
                .Select(path => Path.GetRelativePath(Root, path).Replace(Path.DirectorySeparatorChar, '/'))
                .OrderBy(path => path, StringComparer.Ordinal);

        /// <summary>The census of the primitive: ONE call in all of src, and it is inside EraseUnit of the writer.</summary>
        private static List<string> CensusViolations(IReadOnlyList<(string Path, string Text)> files)
        {
            var violations = new List<string>();
            var calls = files.Select(file => (file.Path, Count: Count(Code(file.Text), PrimitiveCall))).Where(file => file.Count > 0).ToList();
            var total = calls.Sum(file => file.Count);
            if (total != 1) violations.Add("llamadas=" + N(total));
            foreach (var call in calls.Where(file => file.Path != WriterPath)) violations.Add("fuera-del-escritor=" + call.Path);

            var writer = files.Where(file => file.Path == WriterPath).Select(file => Code(file.Text)).FirstOrDefault() ?? string.Empty;
            var mutation = BodyOf(writer, @"internal\s+sealed\s+class\s+RackPilotMutationScope\b");
            var inside = Count(BodyOf(mutation, EraseUnitSignature), PrimitiveCall);
            if (Count(writer, PrimitiveCall) != inside) violations.Add("fuera-de-EraseUnit");
            return violations;
        }

        private static List<(string Path, string Text)> WithText(IReadOnlyList<(string Path, string Text)> files, string path, Func<string, string> change)
            => files.Select(file => file.Path == path ? (file.Path, change(file.Text)) : file).ToList();

        [Fact]
        public void CT_PBT3C_04_TheCensusOfTheFacadeHasOneCallInAllOfSrcAndItIsInsideEraseUnitOfTheWriter()
        {
            var files = SourceFiles().Select(path => (Path: path, Text: Read(path))).ToList();
            var calls = files.Select(file => (file.Path, Count: Count(Code(file.Text), PrimitiveCall))).Where(file => file.Count > 0).ToList();
            var log = new List<string>
            {
                "censo archivos-revisados>0=" + (files.Count > 0),
                "censo llamadas=" + N(calls.Sum(file => file.Count)) + " archivos=" + Show(calls.Select(file => file.Path)),
                "censo violaciones=" + Show(CensusViolations(files)),
            };
            var expected = new List<string>
            {
                "censo archivos-revisados>0=True",
                "censo llamadas=1 archivos=[" + WriterPath + "]",
                "censo violaciones=[]",
            };

            // The self-tests: a second call in the writer, one in the commands of the product and one in a new file all trip the census.
            const string injected = "\nvoid Inyectado(Transaction transaction, ObjectId id) { RackViewBlockEraser.EraseInTransaction(transaction, id); }\n";
            log.Add("autoprueba-segunda-llamada-en-el-escritor=" + (CensusViolations(WithText(files, WriterPath, text => text + injected)).Count > 0 ? "falla" : "pasa"));
            expected.Add("autoprueba-segunda-llamada-en-el-escritor=falla");
            log.Add("autoprueba-llamada-en-RPC=" + (CensusViolations(WithText(files, CommandsPath, text => text + injected)).Count > 0 ? "falla" : "pasa"));
            expected.Add("autoprueba-llamada-en-RPC=falla");
            var added = files.Concat(new[] { (Path: "src/RackCad.Plugin/Views/RackArchivoNuevo.cs", Text: "class Nuevo {" + injected + "}") }).ToList();
            log.Add("autoprueba-llamada-en-un-archivo-nuevo=" + (CensusViolations(added).Count > 0 ? "falla" : "pasa"));
            expected.Add("autoprueba-llamada-en-un-archivo-nuevo=falla");

            Assert.Equal(string.Join("\n", expected), string.Join("\n", log));
        }
    }
}
