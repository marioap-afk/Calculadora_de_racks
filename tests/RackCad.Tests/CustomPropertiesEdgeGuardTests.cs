using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;
using static RackCad.Tests.CustomPropertiesTestKit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-54 G6 — guardas ESTRUCTURALES del borde fisico de propiedades personalizadas (Proposal V5 D-19.3: T-GRD-04,
    /// T-GRD-05, T-GRD-06 y T-GRD-07; D-07, D-09.1 y D-22; ADR-0039).
    ///
    /// <para>
    /// Ninguna suite carga el Plugin (ADR-0003) y el CI no tiene AutoCAD, asi que el borde se fija por su FORMA: quien abre la
    /// transaccion, cuantas veces se confirma, que se escanea, que se escribe y a quien se le pregunta. Son defensa
    /// secundaria: el comportamiento fisico es de G9 (OV-01..OV-14). Cada guarda se demuestra en rojo aqui mismo, contra una
    /// variante del archivo real o contra un archivo inventado con la violacion: una guarda que nunca falla no vigila nada.
    /// </para>
    /// </summary>
    public class CustomPropertiesEdgeGuardTests
    {
        // ================================================================ T-GRD-04 ejecutores

        [Fact]
        public void TGrd04_ElBordeLeeYEscribeSinDecidir()
        {
            Assert.Empty(CustomPropertiesEdgeSources.ExecutorViolations(CustomPropertiesEdgeSources.ExecutorCode()));
        }

        [Fact]
        public void TGrd04_FueraDelBorde_NingunArchivoDelPluginConsultaLaAutoridadNiConfirmaEnApplication()
        {
            Assert.Empty(CustomPropertiesEdgeSources.PluginCallersOutsideTheEdge(CustomPropertiesEdgeSources.ProductionSources()));
        }

        public static TheoryData<string, string, string> ExecutorMutations() => new TheoryData<string, string, string>
        {
            { "regenera", "transaction.Commit();", "transaction.Commit(); document.Editor.Regen();" },
            { "confirma dos veces", "transaction.Commit();", "transaction.Commit(); transaction.Commit();" },
            { "confirma una negativa de Application", "if (!result.IsPlanned)", "if (!result.IsPlanned && transaction.Commit() == null)" },
            { "busca el rack con FindRackBlocks", "RackBlockFinder.ScanEnvelopes(transaction, database, includeReferenceCount: true)", "RackCommandSupport.FindRackBlocks(null, null)" },
            { "escanea sin contar referencias", "includeReferenceCount: true", "includeReferenceCount: false" },
            { "decide con la autoridad al escribir", "if (!result.IsPlanned)", "if (!result.IsPlanned || !RackCustomPropertiesAuthority.Evaluate(fresh, selection, IsKnownKind).IsWritable)" },
            { "usa el preflight", "if (!result.IsPlanned)", "if (!result.IsPlanned || !CustomPropertiesPreflight.ForRack(null, null).IsAccepted)" },
            { "mira un campo semantico del resultado", "if (!result.IsPlanned)", "if (!result.IsPlanned || result.FreshOutcome == null)" },
            { "mira el kind del sobre", "record.IsDependent, envelope.Embed)", "record.IsDependent, envelope.Embed?.Kind == null ? null : envelope.Embed)" },
            { "redefine un bloque", "WritePlan(transaction, definitions, result.Plan);", "WritePlan(transaction, definitions, result.Plan); SystemBlockWriter.RedefineSystemBlock(null);" },
            { "purga tras confirmar", "WritePlan(transaction, definitions, result.Plan);", "WritePlan(transaction, definitions, result.Plan); SystemBlockWriter.PurgeAfterCommit(database, null);" },
            { "importa bloques", "WritePlan(transaction, definitions, result.Plan);", "WritePlan(transaction, definitions, result.Plan); BlockPlacement.EnsureForPlan(null);" },
            { "purga lo huerfano", "WritePlan(transaction, definitions, result.Plan);", "WritePlan(transaction, definitions, result.Plan); BlockPlacement.PurgeUnreferenced(null);" },
            { "compone el sobre en el Plugin", "WritePlan(transaction, definitions, result.Plan);", "WritePlan(transaction, definitions, result.Plan); RackEmbedComposer.WithCustomProperties(null, null);" },
            { "escribe el proyecto desde un ejecutor de rack", "RackBlockData.Write(transaction, target.Definition, target.Payload);", "RackBlockData.Write(transaction, target.Definition, target.Payload); CustomPropertiesData.Write(transaction, null, target.Payload);" },
            { "escribe mientras resuelve el plan", "targets.Add((definitionId, entry.Payload));", "targets.Add((definitionId, entry.Payload)); RackBlockData.Write(transaction, definitionId, entry.Payload);" },
            { "una lectura escribe", "return CustomPropertiesRackRead.Of(", "RackBlockData.Write(transaction, ObjectId.Null, null); return CustomPropertiesRackRead.Of(" },
            { "una lectura abre otra transaccion", "var selection = SelectionOf(transaction, definitionId);", "var selection = SelectionOf(transaction, definitionId); document.Database.TransactionManager.StartTransaction();" },
            { "el predicado distingue mayusculas", "TryGetIgnoreCase(kind, out _)", "TryGet(kind, out _)" },
            { "el borde declara un comando", "internal static class CustomPropertiesExecutor", "[CommandMethod(\"RACKPRUEBA\")] internal static class CustomPropertiesExecutor" },
            { "el borde pide la seleccion", "var selection = SelectionOf(transaction, definitionId);", "var selection = SelectionOf(transaction, document.Editor.GetEntity(null).ObjectId);" },
        };

        [Theory]
        [MemberData(nameof(ExecutorMutations))]
        public void TGrd04_LaGuardaDetectaUnBordeQueSeSaleDelContrato(string caso, string original, string replacement)
        {
            var text = CustomPropertiesEdgeSources.ExecutorText();

            Assert.Contains(original, text);

            var violations = CustomPropertiesEdgeSources.ExecutorViolations(PluginSourceCode.Mask(text.Replace(original, replacement)));

            Assert.True(violations.Count > 0, "la guarda no detecta: " + caso);
        }

        [Theory]
        [InlineData("src/RackCad.Plugin/RackOtroCommands.cs", "class X { object M() => RackCustomPropertiesAuthority.Evaluate(null, null, null); }")]
        [InlineData("src/RackCad.Plugin/RackOtroCommands.cs", "class X { object M() => CustomPropertiesCommit.ForRack(null, null, null, null, null); }")]
        public void TGrd04_LaGuardaDetectaOtroArchivoDelPluginQueDecide(string path, string violation)
        {
            var sources = CustomPropertiesEdgeSources.ProductionSources()
                .Append(new CustomPropertiesEdgeSources.Source(path, violation))
                .ToList();

            Assert.NotEmpty(CustomPropertiesEdgeSources.PluginCallersOutsideTheEdge(sources));
        }

        // ================================================================ T-GRD-05 independencia de Proyecto

        [Fact]
        public void TGrd05_PropiedadesYProjectVariablesNoSeConocenEnNingunSentido()
        {
            var sources = CustomPropertiesEdgeSources.ProductionSources();

            // La barrida mira de verdad: los dos lados estan, y el borde fisico cuenta entre las propiedades.
            Assert.Contains(sources, source => source.Path == CustomPropertiesEdgeSources.DataPath);
            Assert.Contains(sources, source => source.Path == CustomPropertiesEdgeSources.ExecutorPath);
            Assert.True(sources.Count(source => CustomPropertiesEdgeSources.IsPropertyFile(source.Path)) >= 17, "la barrida apenas ve archivos de propiedades.");
            Assert.True(sources.Count(source => CustomPropertiesEdgeSources.IsProjectVariablesFile(source.Path)) >= 40, "la barrida apenas ve archivos de Project Variables.");
            Assert.Contains(sources, source => source.Path == "src/RackCad.Plugin/ProjectVariablesData.cs");

            Assert.Empty(CustomPropertiesEdgeSources.IndependenceViolations(sources));
        }

        [Theory]
        [InlineData("src/RackCad.Plugin/CustomPropertiesOtro.cs", "class X { object M() => ProjectVariablesRegistry.Read(null, null); }")]
        [InlineData("src/RackCad.Application/CustomProperties/RackCustomPropertiesOtro.cs", "using RackCad.Application.ProjectVariables; class X { }")]
        [InlineData("src/RackCad.Application/Persistence/CustomPropertiesOtro.cs", "class X { const string Key = \"RACKCAD_PROJECT\"; }")]
        [InlineData("src/RackCad.Plugin/ProjectVariablesOtro.cs", "class X { object M() => CustomPropertiesData.Read(null, null); }")]
        [InlineData("src/RackCad.Application/ProjectVariables/Otro.cs", "class X { const string Key = \"RACKCAD_CUSTOM_PROPERTIES\"; }")]
        [InlineData("src/RackCad.Plugin/RackVariablesCommands.cs", "class X { CustomPropertiesWorkspace Workspace; }")]
        public void TGrd05_LaGuardaDetectaUnCruce(string path, string violation)
        {
            var sources = CustomPropertiesEdgeSources.ProductionSources()
                .Where(source => source.Path != path)
                .Append(new CustomPropertiesEdgeSources.Source(path, violation))
                .ToList();

            Assert.NotEmpty(CustomPropertiesEdgeSources.IndependenceViolations(sources));
        }

        [Fact]
        public void TGrd05_NombrarAlOtroEnUnComentarioNoEsUnCruce()
        {
            var sources = new[]
            {
                new CustomPropertiesEdgeSources.Source(
                    "src/RackCad.Plugin/CustomPropertiesOtro.cs",
                    "class X {\n// como ProjectVariablesData y RACKCAD_PROJECT\n/* ProjectVariablesRegistry */ }"),
                new CustomPropertiesEdgeSources.Source(
                    "src/RackCad.Plugin/ProjectVariablesOtro.cs",
                    "class X {\n/// <see cref=\"CustomPropertiesData\"/> y RACKCAD_CUSTOM_PROPERTIES\n}"),
            };

            Assert.Empty(CustomPropertiesEdgeSources.IndependenceViolations(sources));
        }

        // ================================================================ T-GRD-06 capas

        [Fact]
        public void TGrd06_SoloElPluginConoceAutoCAD_YApplicationNoConoceElDibujo()
        {
            var sources = CustomPropertiesEdgeSources.ProductionSources();

            Assert.Contains(sources, source => source.Path.StartsWith("src/RackCad.Domain/", StringComparison.Ordinal));
            Assert.Contains(sources, source => source.Path.StartsWith("src/RackCad.UI/", StringComparison.Ordinal));
            Assert.True(sources.Count(source => CustomPropertiesEdgeSources.IsApplicationPropertyFile(source.Path)) >= 15, "la barrida apenas ve Application.");

            Assert.Empty(CustomPropertiesEdgeSources.LayerViolations(sources));
        }

        [Theory]
        [InlineData("src/RackCad.Domain/Otro.cs", "class X { CustomPropertyId Id; }")]
        [InlineData("src/RackCad.Domain/Otro.cs", "class X { const string Key = \"RACKCAD_CUSTOM_PROPERTIES\"; }")]
        [InlineData("src/RackCad.UI/Otro.cs", "class X { Autodesk.AutoCAD.DatabaseServices.ObjectId Id; }")]
        [InlineData("src/RackCad.Application/Persistence/Otro.cs", "using Autodesk.AutoCAD.Runtime; class X { }")]
        [InlineData("src/RackCad.Application/CustomProperties/CustomPropertiesOtro.cs", "class X { object M(ObjectId id) => id; }")]
        [InlineData("src/RackCad.Application/CustomProperties/RackCustomPropertiesOtro.cs", "class X { object M(Database database) => database; }")]
        [InlineData("src/RackCad.Application/Persistence/CustomPropertiesOtro.cs", "class X { object M(Transaction transaction) => transaction; }")]
        [InlineData("src/RackCad.Application/CustomProperties/CustomPropertiesOtro.cs", "class X { object M() => KindHandlerRegistry.Default; }")]
        public void TGrd06_LaGuardaDetectaUnaCapaQueConoceDeMas(string path, string violation)
        {
            var sources = CustomPropertiesEdgeSources.ProductionSources()
                .Append(new CustomPropertiesEdgeSources.Source(path, violation))
                .ToList();

            Assert.NotEmpty(CustomPropertiesEdgeSources.LayerViolations(sources));
        }

        // ================================================================ T-GRD-07 clave del NOD y nombres prohibidos

        [Fact]
        public void TGrd07_LaClaveDelNodEsUnaSola_Exacta_YVivaEnElLectorFisico()
        {
            Assert.Empty(CustomPropertiesEdgeSources.KeyViolations(CustomPropertiesEdgeSources.ProductionSources()));

            Assert.Matches(
                new Regex(@"\bpublic\s+const\s+string\s+DictKey\s*=\s*""RACKCAD_CUSTOM_PROPERTIES""\s*;"),
                CustomPropertiesEdgeSources.DataText());
        }

        [Theory]
        [InlineData("src/RackCad.Plugin/CustomPropertiesOtro.cs", "class X { const string Key = \"RACKCAD_CUSTOM_PROPERTIES\"; }")]
        [InlineData("src/RackCad.Plugin/Otro.cs", "class X { const string Key = \"RACKCAD_CUSTOM_PROPERTY\"; }")]
        [InlineData("src/RackCad.Plugin/Otro.cs", "class X { const string Key = \"rackcad_custom_properties\"; }")]
        [InlineData("src/RackCad.Plugin/Otro.cs", "class X { const string Key = \"RACKCAD-CUSTOM-PROPERTIES\"; }")]
        [InlineData("src/RackCad.Application/Persistence/Otro.cs", "class X { const string Key = \"RACKCAD_PROPIEDADES\"; }")]
        [InlineData("src/RackCad.Application/CustomProperties/CustomPropertiesOtro.cs", "class CustomPropertyValues { }")]
        [InlineData("src/RackCad.Plugin/CustomPropertiesOtro.cs", "class X { string rackPropertyId; }")]
        [InlineData("src/RackCad.Application/CustomProperties/RackCustomPropertiesOtro.cs", "class X { object RackPropertyReference; }")]
        public void TGrd07_LaGuardaDetectaOtraClaveOUnNombreProhibido(string path, string violation)
        {
            var sources = CustomPropertiesEdgeSources.ProductionSources()
                .Append(new CustomPropertiesEdgeSources.Source(path, violation))
                .ToList();

            Assert.NotEmpty(CustomPropertiesEdgeSources.KeyViolations(sources));
        }

        [Fact]
        public void TGrd07_LaGuardaDetectaLaClaveMovidaFueraDelLectorFisico()
        {
            var sources = CustomPropertiesEdgeSources.ProductionSources()
                .Select(source => source.Path == CustomPropertiesEdgeSources.DataPath
                    ? new CustomPropertiesEdgeSources.Source(
                        source.Path, source.Text.Replace("\"RACKCAD_CUSTOM_PROPERTIES\"", "OtraClase.Key"))
                    : source)
                .Append(new CustomPropertiesEdgeSources.Source(
                    "src/RackCad.Plugin/OtraClase.cs", "class OtraClase { public const string Key = \"RACKCAD_CUSTOM_PROPERTIES\"; }"))
                .ToList();

            Assert.NotEmpty(CustomPropertiesEdgeSources.KeyViolations(sources));
        }
    }

    /// <summary>
    /// I-54 G6 — lectura de las fuentes de produccion para T-PRJ-01..04 y T-GRD-04..07. Lee texto con los saltos de linea
    /// normalizados (el CI de Core corre en Linux y el worktree local convierte a CRLF) y lo enmascara con
    /// <see cref="PluginSourceCode"/>: los comentarios y el contenido de los literales no cuentan como codigo, y los
    /// literales se consultan aparte cuando lo que importa es una clave persistida.
    /// </summary>
    internal static class CustomPropertiesEdgeSources
    {
        internal const string DataPath = "src/RackCad.Plugin/CustomPropertiesData.cs";

        internal const string ExecutorPath = "src/RackCad.Plugin/CustomPropertiesExecutor.cs";

        /// <summary>La clave contractual del NOD (D-07.1, ADR-0039 §3).</summary>
        internal const string NodKey = "RACKCAD_CUSTOM_PROPERTIES";

        /// <summary>Los substrings de P-12 que romperian las guardas de conformidad de Project Variables (D-19.2).</summary>
        private static readonly string[] ForbiddenNameParts = { "PropertyValues", "rackProperty", "RackPropertyReference" };

        private static readonly string[] Executors = { "ExecuteProject", "ExecuteRack", "ExecuteRackUnify" };

        private static readonly string[] Reads = { "ReadProject", "ReadRack" };

        private static readonly Lazy<IReadOnlyList<Source>> Production = new Lazy<IReadOnlyList<Source>>(LoadProductionSources);

        /// <summary>Un archivo de produccion: ruta relativa al repositorio, texto, codigo enmascarado y literales.</summary>
        internal sealed class Source
        {
            internal Source(string path, string text)
            {
                Path = path;
                Text = text.Replace("\r\n", "\n");
                Code = PluginSourceCode.Mask(Text);
                Literals = PluginSourceCode.StringLiterals(Text);
            }

            internal string Path { get; }

            internal string Text { get; }

            internal string Code { get; }

            internal IReadOnlyList<string> Literals { get; }
        }

        /// <summary>
        /// Todo el C# de produccion bajo <c>src/</c>, sin lo generado. Se lee y se enmascara una vez por ejecucion: ninguna
        /// prueba lo modifica, y las variantes con violaciones se construyen aparte.
        /// </summary>
        internal static IReadOnlyList<Source> ProductionSources() => Production.Value;

        private static IReadOnlyList<Source> LoadProductionSources()
        {
            var root = RepoRoot().FullName;

            return Directory.GetFiles(System.IO.Path.Combine(root, "src"), "*.cs", SearchOption.AllDirectories)
                .Where(path => !path.Split(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar)
                    .Any(segment => segment == "bin" || segment == "obj"))
                .Select(path => new Source(
                    System.IO.Path.GetRelativePath(root, path).Replace('\\', '/'),
                    File.ReadAllText(path)))
                .OrderBy(source => source.Path, StringComparer.Ordinal)
                .ToList();
        }

        internal static string DataText() => ReadProduction(DataPath);

        internal static string ExecutorText() => ReadProduction(ExecutorPath);

        internal static string ExecutorCode() => PluginSourceCode.Mask(ExecutorText());

        /// <summary>Propiedades personalizadas en produccion: el nucleo, el store y la autoridad de Application, y el borde del Plugin.</summary>
        internal static bool IsPropertyFile(string path)
            => path.StartsWith("src/", StringComparison.Ordinal)
               && Regex.IsMatch(FileName(path), @"^(?:Rack)?CustomPropert\w*\.cs$");

        internal static bool IsApplicationPropertyFile(string path)
            => IsPropertyFile(path) && path.StartsWith("src/RackCad.Application/", StringComparison.Ordinal);

        /// <summary>Project Variables en produccion: su carpeta de Application y todo archivo que las nombra, comando incluido.</summary>
        internal static bool IsProjectVariablesFile(string path)
            => path.StartsWith("src/RackCad.Application/ProjectVariables/", StringComparison.Ordinal)
               || FileName(path).Contains("ProjectVariable", StringComparison.Ordinal)
               || FileName(path) == "RackVariablesCommands.cs";

        // ------------------------------------------------------------ T-GRD-04

        /// <summary>
        /// Lo que el borde hace mal, sobre el codigo enmascarado de <see cref="ExecutorPath"/>. Vacia si cumple D-07.5 y D-22:
        /// una transaccion y una confirmacion por ejecutor, despues de escribir y solo si se escribe; el barrido de
        /// <c>ScanEnvelopes</c> con referencias y nunca <c>FindRackBlocks</c>; ni regeneracion, ni redefinicion, ni
        /// importacion, ni purga; y ninguna decision propia: la autoridad solo se consulta al LEER, el preflight nunca, el
        /// kind solo a traves del predicado del registro, y del resultado de Application solo se mira si hay plan y cual es.
        /// </summary>
        internal static IReadOnlyList<string> ExecutorViolations(string code)
        {
            var violations = new List<string>();
            var members = PluginSourceCode.Members(code);

            foreach (var forbidden in new[]
                     {
                         "Regen", "ApplyRegen", "RedefineSystemBlock", "RedefineInTransaction", "EnsureForPlan", "EnsureBlocks",
                         "PurgeUnreferenced", "PurgeAfterCommit", "FindRackBlocks", "Compose", "WithCustomProperties", "TryGet",
                         "TryResolveAll", "KindHandlerDispatch", "GetEntity", "GetSelection", "GetKeywords", "ShowModalWindow",
                         "CustomPropertiesPreflight",
                     })
            {
                if (Regex.IsMatch(code, @"\b" + forbidden + @"\b"))
                {
                    violations.Add("el borde nombra " + forbidden);
                }
            }

            if (Regex.IsMatch(code, @"\[\s*CommandMethod\b"))
            {
                violations.Add("el borde declara un comando");
            }

            if (Regex.IsMatch(code, @"\.\s*Kind\b"))
            {
                violations.Add("el borde mira un kind: eso es de Application");
            }

            if (Count(code, @"\bKindHandlerRegistry\b") != 1
                || Count(code, @"\bKindHandlerRegistry\s*\.\s*Default\s*\.\s*TryGetIgnoreCase\s*\(") != 1)
            {
                violations.Add("el registro de handlers no aparece exactamente una vez, como predicado sin distinguir mayusculas");
            }

            foreach (Match call in Regex.Matches(code, @"\bScanEnvelopes\s*\("))
            {
                var scan = PluginSourceCode.Calls(code.Substring(call.Index), "ScanEnvelopes")[0];

                if (!scan.Arguments.Any(argument => Regex.IsMatch(argument, @"^\s*(?:includeReferenceCount\s*:\s*)?true\s*$")))
                {
                    violations.Add("el barrido no cuenta las referencias: " + scan.ArgumentText);
                }
            }

            var byName = new Dictionary<string, PluginSourceCode.Member>(StringComparer.Ordinal);

            foreach (var name in Executors.Concat(Reads).Concat(new[] { "WritePlan" }))
            {
                var found = members.Where(member => member.Name == name).ToList();

                if (found.Count != 1)
                {
                    violations.Add("el borde no tiene exactamente un miembro " + name);
                    continue;
                }

                byName[name] = found[0];
            }

            // La autoridad se consulta UNA vez, y es al leer el rack; el commit de Application, solo en los ejecutores de rack.
            if (Count(code, @"\bRackCustomPropertiesAuthority\b") != 1
                || (byName.TryGetValue("ReadRack", out var readRack)
                    && Count(readRack.Body, @"\bRackCustomPropertiesAuthority\s*\.\s*Evaluate\s*\(") != 1))
            {
                violations.Add("la autoridad no se consulta exactamente una vez, al leer el rack");
            }

            foreach (var name in Executors.Where(byName.ContainsKey))
            {
                var executor = byName[name];
                var reached = Reached(members, executor);
                var all = string.Join("\n", new[] { executor.Body }.Concat(reached.Select(member => member.Body)));

                if (Count(executor.Body, @"\bLockDocument\s*\(") != 1
                    || Count(executor.Body, @"\bStartTransaction\s*\(") != 1
                    || Count(executor.Body, @"\bCommit\s*\(") != 1)
                {
                    violations.Add(name + " no bloquea, abre y confirma exactamente una vez");
                }

                if (Regex.IsMatch(executor.Body, @"\bInDocumentTransaction\b")
                    || reached.Any(member => Regex.IsMatch(member.Body, @"\b(?:InDocumentTransaction|StartTransaction|LockDocument|Commit)\b")))
                {
                    violations.Add(name + " delega en otra transaccion o confirmacion");
                }

                var write = name == "ExecuteProject"
                    ? FirstIndex(executor.Body, @"\bCustomPropertiesData\s*\.\s*Write\s*\(")
                    : FirstIndex(executor.Body, @"\bWritePlan\s*\(");
                var commit = FirstIndex(executor.Body, @"\bCommit\s*\(");

                if (write < 0 || commit < 0 || commit < write)
                {
                    violations.Add(name + " no confirma despues de escribir");
                }

                if (name == "ExecuteProject")
                {
                    if (Regex.IsMatch(all, @"\b(?:ScanEnvelopes|RackBlockData|CustomPropertiesCommit|WritePlan)\b"))
                    {
                        violations.Add(name + " toca el alcance Rack");
                    }

                    continue;
                }

                var application = name == "ExecuteRack" ? "ForRack" : "ForRackUnify";
                var other = name == "ExecuteRack" ? "ForRackUnify" : "ForRack";
                var commitCall = FirstIndex(executor.Body, @"\bCustomPropertiesCommit\s*\.\s*" + application + @"\s*\(");

                if (Count(executor.Body, @"\bCustomPropertiesCommit\s*\.\s*" + application + @"\s*\(") != 1
                    || Regex.IsMatch(all, @"\b" + other + @"\s*\("))
                {
                    violations.Add(name + " no llama exactamente una vez a CustomPropertiesCommit." + application);
                }

                if (commitCall < 0 || write < commitCall)
                {
                    violations.Add(name + " escribe antes de que Application devuelva el plan");
                }

                if (Count(all, @"\bScanEnvelopes\s*\(") != 1)
                {
                    violations.Add(name + " no hace exactamente un barrido fresco con ScanEnvelopes");
                }

                if (Count(all, @"\bRackBlockData\s*\.\s*Write\s*\(") != 1
                    || Regex.IsMatch(all, @"\b(?:CustomPropertiesData|CustomPropertiesMutations|CustomPropertiesWriteGuard)\b"))
                {
                    violations.Add(name + " no escribe solo con RackBlockData.Write");
                }

                foreach (Match field in Regex.Matches(
                             all,
                             @"\.\s*(?<field>FreshOutcome|Refusal|MutationRejection|Error|Document|CreatedId|Members|UnifyOptions|CanonicalForm|Outcome|WriteVersion|RackId|Collection|IsWritable|Id|View|Section)\b"))
                {
                    violations.Add(name + " mira un campo semantico: " + field.Groups["field"].Value);
                }
            }

            if (byName.TryGetValue("WritePlan", out var writePlan))
            {
                // Todo el plan se resuelve antes de la primera escritura: ningun bucle que resuelve tambien escribe.
                foreach (var loop in Blocks(writePlan.Body, "foreach"))
                {
                    if (Regex.IsMatch(loop, @"\bTryGetValue\s*\(") && Regex.IsMatch(loop, @"\bWrite\s*\("))
                    {
                        violations.Add("WritePlan escribe antes de resolver todo el plan");
                    }
                }

                if (!Regex.IsMatch(writePlan.Body, @"\bTryGetValue\s*\(")
                    || FirstIndex(writePlan.Body, @"\bTryGetValue\s*\(") > FirstIndex(writePlan.Body, @"\bRackBlockData\s*\.\s*Write\s*\("))
                {
                    violations.Add("WritePlan no resuelve cada handle antes de escribir");
                }
            }

            foreach (var name in Reads.Where(byName.ContainsKey))
            {
                var read = byName[name];
                var reached = Reached(members, read);
                var all = string.Join("\n", new[] { read.Body }.Concat(reached.Select(member => member.Body)));

                if (Count(read.Body, @"\bInDocumentTransaction\s*\.\s*Run\s*\(") != 1
                    || Regex.IsMatch(all, @"\b(?:StartTransaction|LockDocument|Commit)\s*\("))
                {
                    violations.Add(name + " no lee en exactamente una transaccion");
                }

                if (Regex.IsMatch(all, @"\b(?:Write|WritePlan|UpgradeOpen)\s*\(|\bForWrite\b|\bCustomPropertiesCommit\b"))
                {
                    violations.Add(name + " escribe o confirma en Application");
                }

                if (name == "ReadProject")
                {
                    if (Count(read.Body, @"\bCustomPropertiesData\s*\.\s*Read\s*\(") != 1
                        || Count(read.Body, @"\bCustomPropertiesWorkspace\s*\.\s*ForProject\s*\(") != 1
                        || Regex.IsMatch(all, @"\bScanEnvelopes\b"))
                    {
                        violations.Add(name + " no lee el NOD hacia el workspace de Proyecto");
                    }
                }
                else if (Count(all, @"\bScanEnvelopes\s*\(") != 1
                         || Count(read.Body, @"\bCustomPropertiesWorkspace\s*\.\s*ForRack\s*\(") != 1
                         || Regex.IsMatch(all, @"\bCustomPropertiesData\b"))
                {
                    violations.Add(name + " no barre una vez hacia el workspace de Rack");
                }
            }

            return violations;
        }

        /// <summary>
        /// Fuera de <see cref="ExecutorPath"/>, ningun archivo del Plugin consulta la autoridad ni pide a Application un
        /// commit de rack: una segunda puerta de escritura no pasaria por el contrato del borde.
        /// </summary>
        internal static IReadOnlyList<string> PluginCallersOutsideTheEdge(IEnumerable<Source> sources)
            => sources
                .Where(source => source.Path.StartsWith("src/RackCad.Plugin/", StringComparison.Ordinal) && source.Path != ExecutorPath)
                .SelectMany(source => Regex.Matches(source.Code, @"\b(?:RackCustomPropertiesAuthority|CustomPropertiesCommit)\b")
                    .Select(match => source.Path + " nombra " + match.Value))
                .ToList();

        // ------------------------------------------------------------ T-PRJ-01

        /// <summary>
        /// Lo que el lector y el escritor fisicos hacen mal, sobre el texto de <see cref="DataPath"/>. Vacia si cumple D-07.1 y
        /// D-07.2: la clave contractual; lectura tri-estado sin un solo camino que lance por el contenido (nada de casts
        /// que lanzan, ningun <c>throw</c>, y solo la familia de excepciones de AutoCAD capturada, convertida en ilegible);
        /// los trozos de texto concatenados en orden; ninguna interpretacion del JSON; y la escritura como UN Xrecord
        /// directo en trozos de 255, solo de texto, que nunca borra ni sustituye una entrada que no sea Xrecord.
        /// </summary>
        internal static IReadOnlyList<string> ReaderViolations(string text)
        {
            var violations = new List<string>();
            var source = new Source(DataPath, text);
            var code = source.Code;
            var members = PluginSourceCode.Members(code);

            if (!Regex.IsMatch(source.Text, @"\bpublic\s+const\s+string\s+DictKey\s*=\s*""" + NodKey + @"""\s*;"))
            {
                violations.Add("la clave no es la constante contractual " + NodKey);
            }

            if (!Regex.IsMatch(source.Text, @"\bconst\s+int\s+ChunkSize\s*=\s*255\s*;"))
            {
                violations.Add("los trozos no son de 255 caracteres");
            }

            foreach (Match match in Regex.Matches(code, @"\bcatch\b\s*(?<clause>\([^)]*\))?"))
            {
                if (!Regex.IsMatch(match.Groups["clause"].Value, @"^\(\s*(?:global\s*::\s*)?Autodesk\s*\.\s*AutoCAD\s*\.\s*Runtime\s*\.\s*Exception\b"))
                {
                    violations.Add("captura fuera de la familia de AutoCAD: catch " + match.Groups["clause"].Value);
                }
            }

            if (Regex.IsMatch(code, @"\b(?:Json\w*|CustomPropertiesStore|Deserialize|Serialize)\b"))
            {
                violations.Add("el acceso fisico interpreta el texto");
            }

            foreach (var name in new[] { "Contains", "GetAt", "SetAt" })
            {
                foreach (var call in PluginSourceCode.Calls(code, name))
                {
                    if (call.Arguments.Count == 0 || call.Arguments[0].Trim() != "DictKey")
                    {
                        violations.Add(name + " sin la clave contractual: " + call.ArgumentText);
                    }
                }
            }

            var read = members.Where(member => member.Name == "Read").ToList();

            if (read.Count != 1 || !read[0].Parameters.Select(parameter => parameter.Type).SequenceEqual(new[] { "Transaction", "Database" }))
            {
                violations.Add("no hay exactamente un Read(Transaction, Database)");
            }
            else
            {
                var body = read[0].Body;

                if (Regex.IsMatch(body, @"\bthrow\b"))
                {
                    violations.Add("Read lanza");
                }

                if (Regex.IsMatch(body, @"\(\s*(?:DBDictionary|Xrecord|DBObject|ResultBuffer)\s*\)"))
                {
                    violations.Add("Read convierte con un cast que lanza");
                }

                if (!Regex.IsMatch(body, @"\bNamedObjectsDictionaryId\b") || !Regex.IsMatch(body, @"\bis\s+Xrecord\b"))
                {
                    violations.Add("Read no parte del NOD o no distingue una entrada que no es Xrecord");
                }

                if (Count(body, @"\bCustomPropertiesPayload\s*\.\s*Absent\s*\(") != 1
                    || Count(body, @"\bCustomPropertiesPayload\s*\.\s*Present\s*\(") != 1
                    || Count(body, @"\bCustomPropertiesPayload\s*\.\s*Unreadable\s*\(") < 5)
                {
                    violations.Add("Read no es tri-estado: un ausente, un presente y los cinco ilegibles");
                }

                if (!Regex.IsMatch(body, @"\bDxfCode\s*\.\s*Text\b")
                    || !Regex.IsMatch(body, @"\bAppend\s*\(")
                    || !Regex.IsMatch(body, @"\bPresent\s*\(\s*\w+\s*\.\s*ToString\s*\(\s*\)\s*\)"))
                {
                    violations.Add("Read no entrega los trozos de texto concatenados en orden");
                }

                var handler = Regex.Match(body, @"\bcatch\b");

                if (!handler.Success)
                {
                    violations.Add("Read no convierte un fallo de AutoCAD en ilegible");
                }
                else
                {
                    var block = Blocks(body.Substring(handler.Index), "catch").FirstOrDefault() ?? string.Empty;

                    if (!Regex.IsMatch(block, @"\breturn\s+CustomPropertiesPayload\s*\.\s*Unreadable\s*\("))
                    {
                        violations.Add("el fallo de AutoCAD no se devuelve como ilegible");
                    }
                }
            }

            var write = members.Where(member => member.Name == "Write").ToList();

            if (write.Count != 1 || !write[0].Parameters.Select(parameter => parameter.Type).SequenceEqual(new[] { "Transaction", "Database", "string" }))
            {
                violations.Add("no hay exactamente un Write(Transaction, Database, string)");
            }
            else
            {
                var body = write[0].Body;

                if (!Regex.IsMatch(body, @"\bnew\s+Xrecord\b") || Regex.IsMatch(body, @"\bnew\s+DBDictionary\b"))
                {
                    violations.Add("Write no guarda un Xrecord directo");
                }

                if (Count(body, @"\bSetAt\s*\(") != 1 || !Regex.IsMatch(body, @"\bAddNewlyCreatedDBObject\s*\("))
                {
                    violations.Add("Write no crea la entrada una sola vez");
                }

                if (Regex.IsMatch(body, @"\b(?:Erase|Remove)\s*\("))
                {
                    violations.Add("Write borra o sustituye una entrada");
                }

                if (!Regex.IsMatch(body, @"\bis\s+Xrecord\b") || !Regex.IsMatch(body, @"\bthrow\s+new\s+InvalidOperationException\b"))
                {
                    violations.Add("Write no rechaza una entrada que no es Xrecord");
                }

                if (!Regex.IsMatch(body, @"\bSubstring\s*\([^;]*\bMath\s*\.\s*Min\s*\(\s*ChunkSize\b") || !Regex.IsMatch(body, @"\+=\s*ChunkSize\b"))
                {
                    violations.Add("Write no trocea en ChunkSize");
                }

                var values = PluginSourceCode.Calls(body, "TypedValue");

                if (values.Count == 0 || values.Any(value => !Regex.IsMatch(value.Arguments[0], @"^\s*\(\s*int\s*\)\s*DxfCode\s*\.\s*Text\s*$")))
                {
                    violations.Add("Write escribe trozos que no son DxfCode.Text");
                }
            }

            return violations;
        }

        // ------------------------------------------------------------ T-PRJ-04

        /// <summary>
        /// Lo que el ejecutor de Proyecto hace mal (D-07.5). Vacia si, en una sola transaccion: relee el NOD, lo acredita el
        /// store, exige una lectura escribible, aplica el intent por id, consulta la guarda, serializa, escribe eso y confirma
        /// una vez, en ese orden; sin regenerar y sin tocar Project Variables ni el alcance Rack.
        /// </summary>
        internal static IReadOnlyList<string> ProjectExecutorViolations(string code)
        {
            var violations = new List<string>();
            var executor = PluginSourceCode.Members(code).Where(member => member.Name == "ExecuteProject").ToList();

            if (executor.Count != 1)
            {
                violations.Add("no hay exactamente un ExecuteProject");
                return violations;
            }

            var body = executor[0].Body;

            // Lo serializado puede ser el argumento de la escritura, que se evalua antes de escribir: la escritura cuenta
            // donde termina su sentencia.
            var steps = new[]
            {
                ("acreditar la relectura en el store", @"\.\s*Read\s*\(\s*CustomPropertiesData\s*\.\s*Read\s*\(", false),
                ("exigir una lectura escribible", @"\.\s*CanWrite\b", false),
                ("aplicar el intent", @"\bCustomPropertiesMutations\s*\.\s*Apply\s*\(", false),
                ("consultar la guarda", @"\bCustomPropertiesWriteGuard\s*\.\s*CanOverwrite\s*\(", false),
                ("serializar", @"\.\s*Serialize\s*\(", false),
                ("escribir", @"\bCustomPropertiesData\s*\.\s*Write\s*\([^;]*;", true),
                ("confirmar", @"\bCommit\s*\(", false),
            };

            var previous = -1;

            foreach (var (step, pattern, atEnd) in steps)
            {
                var matches = Regex.Matches(body, pattern);

                if (matches.Count != 1)
                {
                    violations.Add("ExecuteProject no tiene exactamente un paso: " + step);
                    continue;
                }

                var index = atEnd ? matches[0].Index + matches[0].Length : matches[0].Index;

                if (index < previous)
                {
                    violations.Add("ExecuteProject hace fuera de orden: " + step);
                }

                previous = Math.Max(previous, index);
            }

            if (!Regex.IsMatch(body, @"\bnew\s+CustomPropertiesStore\s*\("))
            {
                violations.Add("ExecuteProject no acredita con el store de Application");
            }

            var writes = PluginSourceCode.Calls(body, "Write");

            if (writes.Count != 1 || writes[0].Arguments.Count != 3 || !Regex.IsMatch(writes[0].Arguments[2], @"\bSerialize\s*\("))
            {
                violations.Add("ExecuteProject no escribe exactamente lo que serializa");
            }

            if (Count(body, @"\bLockDocument\s*\(") != 1 || Count(body, @"\bStartTransaction\s*\(") != 1)
            {
                violations.Add("ExecuteProject no trabaja en una sola transaccion");
            }

            if (Regex.IsMatch(body, @"\b(?:Regen|ApplyRegen|ScanEnvelopes|RackBlockData|InDocumentTransaction)\b|ProjectVariable"))
            {
                violations.Add("ExecuteProject hace algo mas que escribir la coleccion de Proyecto");
            }

            return violations;
        }

        // ------------------------------------------------------------ T-GRD-05

        internal static IReadOnlyList<string> IndependenceViolations(IEnumerable<Source> sources)
        {
            var violations = new List<string>();

            foreach (var source in sources)
            {
                if (IsPropertyFile(source.Path))
                {
                    if (source.Code.Contains("ProjectVariable", StringComparison.Ordinal))
                    {
                        violations.Add(source.Path + " nombra Project Variables");
                    }

                    if (source.Literals.Any(literal => literal.Contains("RACKCAD_PROJECT", StringComparison.Ordinal)))
                    {
                        violations.Add(source.Path + " nombra la clave RACKCAD_PROJECT");
                    }
                }

                if (IsProjectVariablesFile(source.Path))
                {
                    if (source.Code.Contains("CustomPropert", StringComparison.Ordinal))
                    {
                        violations.Add(source.Path + " nombra propiedades personalizadas");
                    }

                    if (source.Literals.Any(literal => literal.Contains(NodKey, StringComparison.Ordinal)))
                    {
                        violations.Add(source.Path + " nombra la clave " + NodKey);
                    }
                }
            }

            return violations;
        }

        // ------------------------------------------------------------ T-GRD-06

        internal static IReadOnlyList<string> LayerViolations(IEnumerable<Source> sources)
        {
            var violations = new List<string>();

            foreach (var source in sources)
            {
                var isDomain = source.Path.StartsWith("src/RackCad.Domain/", StringComparison.Ordinal);

                if (isDomain
                    && (source.Code.Contains("CustomPropert", StringComparison.Ordinal)
                        || source.Literals.Any(literal => literal.Contains(NodKey, StringComparison.Ordinal))))
                {
                    violations.Add(source.Path + ": el dominio conoce las propiedades personalizadas");
                }

                if ((isDomain
                     || source.Path.StartsWith("src/RackCad.Application/", StringComparison.Ordinal)
                     || source.Path.StartsWith("src/RackCad.UI/", StringComparison.Ordinal))
                    && Regex.IsMatch(source.Code, @"\bAutodesk\b"))
                {
                    violations.Add(source.Path + ": conoce AutoCAD fuera del Plugin");
                }

                if (IsApplicationPropertyFile(source.Path))
                {
                    foreach (Match match in Regex.Matches(source.Code, @"\b(?:ObjectId|Database|Transaction|KindHandlerRegistry)\b"))
                    {
                        violations.Add(source.Path + ": nombra " + match.Value);
                    }
                }
            }

            return violations;
        }

        // ------------------------------------------------------------ T-GRD-07

        internal static IReadOnlyList<string> KeyViolations(IEnumerable<Source> sources)
        {
            var violations = new List<string>();
            var declarations = new List<string>();

            foreach (var source in sources)
            {
                foreach (var literal in source.Literals)
                {
                    if (literal == NodKey)
                    {
                        declarations.Add(source.Path);
                    }
                    else if (LooksLikeTheKey(literal))
                    {
                        violations.Add(source.Path + ": otra grafia de la clave del NOD: " + literal);
                    }
                }

                if (IsPropertyFile(source.Path))
                {
                    var visible = source.Code + "\n" + string.Join("\n", source.Literals);

                    foreach (var part in ForbiddenNameParts.Where(part => visible.Contains(part, StringComparison.Ordinal)))
                    {
                        violations.Add(source.Path + ": contiene " + part);
                    }
                }
            }

            if (declarations.Count != 1 || declarations[0] != DataPath)
            {
                violations.Add(
                    "la clave del NOD no se declara exactamente una vez en " + DataPath + ": "
                    + (declarations.Count == 0 ? "ningun archivo" : string.Join(", ", declarations)));
            }

            return violations;
        }

        /// <summary>Un literal con forma de clave (sin espacios) que nombra RackCad y propiedades.</summary>
        private static bool LooksLikeTheKey(string literal)
        {
            var compact = literal.Trim();

            return Regex.IsMatch(compact, @"^[A-Za-z0-9_.\-]+$")
                   && compact.IndexOf("rackcad", StringComparison.OrdinalIgnoreCase) >= 0
                   && Regex.IsMatch(compact, "custom|propert|propiedad", RegexOptions.IgnoreCase);
        }

        // ------------------------------------------------------------ utilidades

        private static string ReadProduction(string relative)
        {
            var path = System.IO.Path.Combine(RepoRoot().FullName, relative);

            Assert.True(File.Exists(path), "No existe el archivo de produccion: " + relative);
            return File.ReadAllText(path).Replace("\r\n", "\n");
        }

        private static string FileName(string path) => path.Substring(path.LastIndexOf('/') + 1);

        private static int Count(string text, string pattern) => Regex.Matches(text, pattern).Count;

        private static int FirstIndex(string text, string pattern)
        {
            var match = Regex.Match(text, pattern);
            return match.Success ? match.Index : -1;
        }

        /// <summary>Los miembros que <paramref name="start"/> llama, directa o indirectamente, sin contarse a si mismo.</summary>
        private static IReadOnlyList<PluginSourceCode.Member> Reached(
            IReadOnlyList<PluginSourceCode.Member> members, PluginSourceCode.Member start)
        {
            var reached = new List<PluginSourceCode.Member>();
            var visited = new HashSet<PluginSourceCode.Member> { start };
            var pending = new Stack<string>();
            pending.Push(start.Body);

            while (pending.Count > 0)
            {
                var text = pending.Pop();

                foreach (var member in members)
                {
                    if (!visited.Contains(member) && PluginSourceCode.Calls(text, member.Name).Count > 0)
                    {
                        visited.Add(member);
                        reached.Add(member);
                        pending.Push(member.Body);
                    }
                }
            }

            return reached;
        }

        /// <summary>El bloque entre llaves de cada sentencia <paramref name="keyword"/> de <paramref name="code"/>.</summary>
        private static IEnumerable<string> Blocks(string code, string keyword)
        {
            foreach (Match match in Regex.Matches(code, @"\b" + keyword + @"\b"))
            {
                var open = code.IndexOf('{', match.Index);

                if (open < 0)
                {
                    yield break;
                }

                var depth = 0;

                for (var i = open; i < code.Length; i++)
                {
                    if (code[i] == '{')
                    {
                        depth++;
                    }
                    else if (code[i] == '}' && --depth == 0)
                    {
                        yield return code.Substring(open, i - open + 1);
                        break;
                    }
                }
            }
        }
    }
}
