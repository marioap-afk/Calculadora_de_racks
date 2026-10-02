using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using RackCad.Application.ComputedParameters;
using RackCad.Application.Persistence;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-63 G2 — guardas de fuente. El Plugin referencia AutoCAD y esta suite no puede cargarlo (ADR-0003), asi que
    /// estas pruebas leen sus <c>.cs</c> como TEXTO, sin comentarios.
    ///
    /// <para>
    /// INV-33: UNA funcion, <see cref="DelegationViolations"/>, decide si el texto de un metodo delega en
    /// <c>RackOutputVerdict</c>; se aplica al <c>OutputBlockedReason</c> real y a un fixture con el texto literal del
    /// metodo en 819955d6. INV-35: UNA funcion, <see cref="MissingPresenceEvidence"/>, comprueba cada fila de D-26
    /// contra el <c>BuildBom</c>/<c>Build</c> del handler; el control positivo la aplica a un fixture sin la comprobacion.
    /// </para>
    /// </summary>
    public class ComputedParametersPopulationGuardTests
    {
        // El texto LITERAL de PushBackKindHandler.OutputBlockedReason en 819955d6 (PushBackKindHandler.cs:56-73).
        private const string LegacyOutputBlockedReason = @"
        public string OutputBlockedReason(RackEmbedDocument embed, RackCatalog catalog)
        {
            try
            {
                var project = new RackProjectStore().Deserialize(embed?.Design);
                if (project?.PushBackDesign == null)
                {
                    return null;   // ilegible: lo reporta el camino del BOM, no esta puerta
                }

                var system = new PushBackResolver(catalog).Resolve(project.PushBackDesign);
                return RackBomOutputGate.For(system).Reason;
            }
            catch (System.Exception)
            {
                return null;   // ilegible: mismo criterio
            }
        }";

        private static readonly Dictionary<string, string> HandlerFiles = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [RackEmbedDocument.KindSelective] = "SelectiveKindHandler.cs",
            [RackEmbedDocument.KindDynamic] = "DynamicKindHandler.cs",
            [RackEmbedDocument.KindPushBack] = "PushBackKindHandler.cs",
            [RackEmbedDocument.KindCantilever] = "CantileverKindHandler.cs",
            [RackEmbedDocument.KindCabecera] = "CabeceraKindHandler.cs",
            [RackEmbedDocument.KindCama] = "CamaKindHandler.cs",
        };

        // ---------------------------------------------------------------- utilidades de fuente

        private static DirectoryInfo RepoRoot()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "RackCad.sln")))
            {
                dir = dir.Parent;
            }

            Assert.True(dir != null, "No se encontro la raiz del repositorio (RackCad.sln).");
            return dir;
        }

        private static string ReadHandler(string file)
        {
            var path = Path.Combine(RepoRoot().FullName, "src", "RackCad.Plugin", "KindHandlers", file);
            Assert.True(File.Exists(path), "No existe el archivo: " + path);
            return File.ReadAllText(path);
        }

        private static string CodeOnly(string source)
        {
            var withoutBlocks = Regex.Replace(source, @"/\*.*?\*/", string.Empty, RegexOptions.Singleline);
            return Regex.Replace(withoutBlocks, @"//[^\n]*", string.Empty);
        }

        /// <summary>El texto (sin comentarios) de cada metodo declarado con ese nombre: cuerpo con llaves o con flecha.</summary>
        private static IReadOnlyList<string> MethodTexts(string source, string methodName)
        {
            var code = CodeOnly(source);
            var declaration = new Regex(
                @"\b(?:public|private|internal|protected)\s+(?:static\s+)?[\w<>\[\],.?]+\s+" + Regex.Escape(methodName) + @"\s*\(");
            var texts = new List<string>();

            foreach (Match match in declaration.Matches(code))
            {
                var index = match.Index + match.Length;
                var depth = 1;
                while (index < code.Length && depth > 0)
                {
                    depth += code[index] == '(' ? 1 : code[index] == ')' ? -1 : 0;
                    index++;
                }

                while (index < code.Length && char.IsWhiteSpace(code[index]))
                {
                    index++;
                }

                int end;
                if (index < code.Length && code[index] == '{')
                {
                    depth = 0;
                    end = index;
                    do
                    {
                        depth += code[end] == '{' ? 1 : code[end] == '}' ? -1 : 0;
                        end++;
                    }
                    while (end < code.Length && depth > 0);
                }
                else
                {
                    var semicolon = code.IndexOf(';', index);
                    end = semicolon < 0 ? code.Length : semicolon + 1;
                }

                texts.Add(code.Substring(match.Index, end - match.Index));
            }

            return texts;
        }

        // ---------------------------------------------------------------- INV-33: LA guarda

        /// <summary>
        /// La guarda UNICA de INV-33, <c>DelegatesToOutputVerdict(textoDelMetodo)</c>, sobre el codigo sin comentarios:
        /// devuelve las violaciones (vacio = delega). Exige que llame a <c>RackOutputVerdict</c> y a la correspondencia
        /// del handler, y que NO contenga <c>PushBackResolver</c> ni <c>RackBomOutputGate.For</c>.
        /// </summary>
        private static IReadOnlyList<string> DelegationViolations(string methodText)
        {
            var code = CodeOnly(methodText ?? string.Empty);
            var violations = new List<string>();

            if (!Regex.IsMatch(code, @"\bRackOutputVerdict\s*\.\s*HandlerBlockedReason\s*\("))
            {
                violations.Add("no llama a RackOutputVerdict.HandlerBlockedReason");
            }

            if (Regex.IsMatch(code, @"\bPushBackResolver\b"))
            {
                violations.Add("PushBackResolver");
            }

            if (Regex.IsMatch(code, @"\bRackBomOutputGate\s*\.\s*For\b"))
            {
                violations.Add("RackBomOutputGate.For");
            }

            return violations;
        }

        private static bool DelegatesToOutputVerdict(string methodText) => DelegationViolations(methodText).Count == 0;

        [Fact]
        public void INV33_LaMismaGuardaRechazaElTextoLiteralDe819955d6_YAceptaElOutputBlockedReasonActual()
        {
            // (b) Control: el texto LITERAL del metodo en 819955d6, por la MISMA funcion, es rechazado por la composicion antigua.
            var legacyTexts = MethodTexts("class Fixture {" + LegacyOutputBlockedReason + "}", "OutputBlockedReason");
            var legacy = Assert.Single(legacyTexts);
            Assert.False(DelegatesToOutputVerdict(legacy));
            var legacyViolations = DelegationViolations(legacy);
            Assert.Contains("PushBackResolver", legacyViolations);
            Assert.Contains("RackBomOutputGate.For", legacyViolations);
            Assert.Contains("no llama a RackOutputVerdict.HandlerBlockedReason", legacyViolations);

            // El mismo literal pasado directo (sin extraer) tambien es rechazado: el extractor no es lo que discrimina.
            Assert.False(DelegatesToOutputVerdict(LegacyOutputBlockedReason));

            // Un metodo que delega sin componer nada es aceptado por la misma funcion (la guarda no rechaza todo).
            const string delegating = @"
                public string OutputBlockedReason(RackEmbedDocument embed, RackCatalog catalog)
                    => RackOutputVerdict.HandlerBlockedReason(Kind, embed?.Design, catalog);";
            Assert.True(DelegatesToOutputVerdict(delegating));

            // Mencionar los nombres prohibidos SOLO en un comentario no cuenta (la guarda opera sin comentarios).
            const string commentOnly = @"
                public string OutputBlockedReason(RackEmbedDocument embed, RackCatalog catalog)
                    // antes: PushBackResolver y RackBomOutputGate.For(system)
                    => RackOutputVerdict.HandlerBlockedReason(Kind, embed?.Design, catalog);";
            Assert.True(DelegatesToOutputVerdict(commentOnly));

            // (a) El OutputBlockedReason ACTUAL del handler real: debe pasar. Antes del cambio autorizado, el metodo
            // vigente (819955d6) compone el solo y esta asercion falla.
            var current = Assert.Single(MethodTexts(ReadHandler("PushBackKindHandler.cs"), "OutputBlockedReason"));
            var currentViolations = DelegationViolations(current);

            Assert.True(
                currentViolations.Count == 0,
                "PushBackKindHandler.OutputBlockedReason no delega en RackOutputVerdict: " + string.Join(" | ", currentViolations));
        }

        // ---------------------------------------------------------------- INV-35: LA guarda

        /// <summary>
        /// La guarda UNICA de INV-35: de una fila de D-26, que falta en el <c>BuildBom</c> o <c>Build</c> del handler
        /// (sin comentarios): cada identificador de la comprobacion de presencia y la comparacion con nulo.
        /// </summary>
        private static IReadOnlyList<string> MissingPresenceEvidence(string handlerSource, IReadOnlyList<string> markers)
        {
            var text = string.Join("\n", MethodTexts(handlerSource, "BuildBom").Concat(MethodTexts(handlerSource, "Build")));
            var missing = new List<string>();

            if (text.Length == 0)
            {
                missing.Add("<sin BuildBom ni Build>");
                return missing;
            }

            foreach (var marker in markers)
            {
                if (!Regex.IsMatch(text, @"\b" + Regex.Escape(marker) + @"\b"))
                {
                    missing.Add(marker);
                }
            }

            if (!Regex.IsMatch(text, @"(==|!=)\s*null|\bis\s+(not\s+)?null\b"))
            {
                missing.Add("<comprobacion de nulo>");
            }

            return missing;
        }

        [Fact]
        public void INV35_LaProduccionDeclaraLasSeisFilasDeD26_EnElOrdenDeLosSeisKinds()
        {
            var rows = RackMetricDesignReader.PresenceRows;

            Assert.Equal(
                new[]
                {
                    RackEmbedDocument.KindSelective, RackEmbedDocument.KindDynamic, RackEmbedDocument.KindPushBack,
                    RackEmbedDocument.KindCantilever, RackEmbedDocument.KindCabecera, RackEmbedDocument.KindCama,
                },
                rows.Select(row => row.KindToken));

            Assert.Equal(
                new[]
                {
                    new[] { "SelectivePalletDesignStore" },
                    new[] { "DynamicDesign", "DynamicSystem" },
                    new[] { "PushBackDesign" },
                    new[] { "CantileverLineDesign" },
                    new[] { "Header" },
                    new[] { "FlowBedConfigurationStore" },
                },
                rows.Select(row => row.Markers.ToArray()));
        }

        [Fact]
        public void INV35_CadaFilaDeD26EstaEnElBuildBomDeSuHandler_YElMismoChequeoRechazaUnFixtureSinLaComprobacion()
        {
            var rows = RackMetricDesignReader.PresenceRows;
            Assert.Equal(HandlerFiles.Count, rows.Count);

            // (a) Los handlers reales: ninguna fila pierde su comprobacion de presencia.
            foreach (var row in rows)
            {
                Assert.True(HandlerFiles.TryGetValue(row.KindToken, out var file), "Sin handler para " + row.KindToken);
                var missing = MissingPresenceEvidence(ReadHandler(file), row.Markers);
                Assert.True(
                    missing.Count == 0,
                    "El handler " + file + " ya no contiene la comprobacion de D-26: " + string.Join(", ", missing));
            }

            // (b) Control con la MISMA funcion: un handler sin DynamicSystem ni comprobacion legacy.
            const string withoutSystem = @"
                internal sealed class FixtureDynamicHandler
                {
                    public object BuildBom(object embed, object catalog) => Build(embed, catalog);

                    private object Build(object embed, object catalog)
                    {
                        var project = new RackProjectStore().Deserialize(embed.Design);
                        var system = project?.DynamicDesign == null ? null : Resolve(project.DynamicDesign);
                        return system == null ? null : SystemBomBuilder.Build(system, catalog);
                    }
                }";
            var dynamicRow = rows.First(row => row.KindToken == RackEmbedDocument.KindDynamic);
            Assert.Equal(new[] { "DynamicSystem" }, MissingPresenceEvidence(withoutSystem, dynamicRow.Markers));

            // Un fixture sin ninguna comprobacion de nulo ni identificador de la fila.
            const string noCheck = @"
                internal sealed class FixtureHandler
                {
                    public object BuildBom(object embed, object catalog)
                    {
                        var bom = Build(embed, catalog);
                        return bom;
                    }

                    private object Build(object embed, object catalog) => Quote(embed.Design);
                }";
            var pushBackRow = rows.First(row => row.KindToken == RackEmbedDocument.KindPushBack);
            var missingEverything = MissingPresenceEvidence(noCheck, pushBackRow.Markers);
            Assert.Contains("PushBackDesign", missingEverything);
            Assert.Contains("<comprobacion de nulo>", missingEverything);

            // Mencionar la comprobacion SOLO en un comentario no cuenta.
            const string commentOnly = @"
                internal sealed class FixtureCommentHandler
                {
                    // project?.PushBackDesign == null
                    public object BuildBom(object embed, object catalog) => Quote(embed.Design);
                }";
            Assert.Contains("PushBackDesign", MissingPresenceEvidence(commentOnly, pushBackRow.Markers));
        }
    }
}
