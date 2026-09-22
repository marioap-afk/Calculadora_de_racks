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
    /// I-54 G4B — guardas ESTRUCTURALES del sobre (Proposal V5 D-19.3 y D-21: T-GRD-01, T-GRD-02 y T-GRD-03; ADR-0039 §6).
    ///
    /// <para>
    /// Son defensa secundaria: el criterio de aceptacion es el comportamiento (T-ENV-08, T-CPY-01 y las OV del Plugin). Leen
    /// texto enmascarado con <see cref="PluginSourceCode"/>, que blanquea comentarios y el contenido de los literales, y cada
    /// una se demuestra en rojo aqui mismo contra una variante con la violacion: una guarda que nunca falla no vigila nada.
    /// </para>
    /// </summary>
    public class CustomPropertiesEnvelopeGuardTests
    {
        // ================================================================ T-GRD-01 construccion del sobre

        [Fact]
        public void TGrd01_EnSrc_SoloRackEmbedComposerConstruyeUnSobre()
        {
            var sources = EnvelopeSourceGuards.ProductionSources();

            Assert.Empty(EnvelopeSourceGuards.ConstructionsOutsideComposer(sources));

            // La guarda ve lo que tiene que ver: el compositor si construye.
            var composer = Assert.Single(sources, file => file.Path == EnvelopeSourceGuards.ComposerPath);
            Assert.NotEmpty(EnvelopeSourceGuards.Constructions(composer.Code));
        }

        [Theory]
        [InlineData("class X { object M() { return new RackEmbedDocument(); } }")]
        [InlineData("class X { object M() { return new RackEmbedDocument { Id = null }; } }")]
        [InlineData("class X { object M() { return new  RackCad.Application.Persistence.RackEmbedDocument(); } }")]
        [InlineData("class X { object M() { return new global::RackCad.Application.Persistence.RackEmbedDocument(); } }")]
        [InlineData("class X { void M() { RackEmbedDocument sobre = new(); } }")]
        [InlineData("class X { RackEmbedDocument Sobre { get; set; } = new(); }")]
        [InlineData("class X { private static RackEmbedDocument Build() => new(); }")]
        [InlineData("class X { private static RackEmbedDocument Build(int a) { var b = a; return new(); } }")]
        public void TGrd01_LaGuardaDetectaUnaConstruccionFueraDelCompositor(string violation)
        {
            var sources = EnvelopeSourceGuards.ProductionSources()
                .Append(new EnvelopeSourceGuards.SourceFile("src/RackCad.Plugin/Nuevo.cs", PluginSourceCode.Mask(violation)))
                .ToList();

            Assert.NotEmpty(EnvelopeSourceGuards.ConstructionsOutsideComposer(sources));
        }

        [Fact]
        public void TGrd01_UnComentarioOUnLiteral_NoEsUnaConstruccion()
        {
            var text = "class X { // new RackEmbedDocument()\n string s = \"new RackEmbedDocument()\"; /* RackEmbedDocument e = new(); */ }";

            Assert.Empty(EnvelopeSourceGuards.Constructions(PluginSourceCode.Mask(text)));
        }

        // ================================================================ T-GRD-02 censo de Compose

        /// <summary>
        /// Las siete llamadas historicas de D-21 mas la composicion pura autorizada por I-55 G7, por archivo y con su
        /// clasificacion. Un llamador nuevo falla aqui y obliga a clasificar su origen: un rack existente compone con su sobre
        /// real; <c>Compose(null)</c> solo es de un rack nuevo o de una importacion de biblioteca.
        /// </summary>
        [Fact]
        public void TGrd02_CensoDeCompose_IncluyeD21YLaComposicionPuraDeG7_PorArchivo()
        {
            var calls = EnvelopeSourceGuards.ComposeCallsOutsideComposer(EnvelopeSourceGuards.ProductionSources());

            Assert.Equal(EnvelopeSourceGuards.ExpectedComposeCensus, calls);
        }

        /// <summary>
        /// Clasificacion de cada origen. Seis constructores de payload y el compositor puro de G7 reciben el sobre como
        /// PARAMETRO y lo pasan tal cual: el
        /// redibujo y la vista nueva les dan el sobre real, y solo el rack nuevo y la importacion les dan <c>null</c> (cinco por
        /// omision del parametro opcional; la cama, por su sobrecarga sin sobre). Esos llamadores quedan fuera del alcance de
        /// la guarda, como declara D-21: OV-03, OV-04, OV-07, OV-11 y OV-13. La propagacion de variables compone con
        /// <c>view.Embed</c>, el sobre leido de cada vista. Ninguna llamada a <c>Compose</c> pasa un <c>null</c> literal.
        /// </summary>
        [Fact]
        public void TGrd02_CadaOrigenEstaClasificado_YNingunaLlamadaPasaNullLiteral()
        {
            var sources = EnvelopeSourceGuards.ProductionSources();

            foreach (var call in EnvelopeSourceGuards.ComposeCallsOutsideComposer(sources))
            {
                Assert.NotEqual("null", call.FirstArgument);

                if (call.FirstArgument == "view.Embed")
                {
                    Assert.Equal("src/RackCad.Plugin/ProjectVariableMutationExecutor.cs", call.Path);
                    continue;
                }

                var file = Assert.Single(sources, source => source.Path == call.Path);
                var builder = Assert.Single(
                    PluginSourceCode.Members(file.Code),
                    member => member.Name == call.Member && Regex.IsMatch(member.Body, @"\bRackEmbedComposer\s*\.\s*Compose\s*\("));

                Assert.Contains(builder.Parameters, parameter => parameter.Type == "RackEmbedDocument" && parameter.Name == call.FirstArgument);
            }
        }

        [Fact]
        public void TGrd02_NadieImportaElCompositorConUsingStaticNiConAlias()
        {
            foreach (var file in EnvelopeSourceGuards.ProductionSources())
            {
                Assert.DoesNotMatch(@"\busing\s+static\s+[\w.]*RackEmbedComposer\s*;", file.Code);
                Assert.DoesNotMatch(@"\busing\s+\w+\s*=\s*[\w.]*RackEmbedComposer\s*;", file.Code);
            }
        }

        [Theory]
        [InlineData("class X { internal static object M() { return RackEmbedComposer.Compose(null, \"cama\", \"i\", \"n\", null, -1, \"{}\"); } }")]
        [InlineData("class X { internal static object M(RackEmbedDocument otro) { return RackEmbedComposer.Compose(otro, \"cama\", \"i\", \"n\", null, -1, \"{}\"); } }")]
        [InlineData("class X { public X() { RackEmbedComposer.Compose(null, \"cama\", \"i\", \"n\", null, -1, \"{}\"); } }")]
        [InlineData("class X { System.Func<object> f = () => RackEmbedComposer.Compose(null, \"cama\", \"i\", \"n\", null, -1, \"{}\"); }")]
        public void TGrd02_LaGuardaDetectaUnaLlamadaNueva(string violation)
        {
            var sources = EnvelopeSourceGuards.ProductionSources()
                .Append(new EnvelopeSourceGuards.SourceFile("src/RackCad.Plugin/Nuevo.cs", PluginSourceCode.Mask(violation)))
                .ToList();

            Assert.NotEqual(EnvelopeSourceGuards.ExpectedComposeCensus, EnvelopeSourceGuards.ComposeCallsOutsideComposer(sources));
        }

        // ================================================================ T-GRD-03 restamp estructural

        /// <summary>
        /// El restamp de copia independiente re-estampa el MISMO objeto que deserializa (D-11.5): en la sobrecarga con
        /// <see cref="Guid"/>, un solo <c>Deserialize(</c> asignado a un identificador y un solo <c>Serialize(</c> con ese
        /// identificador, sin construir un sobre y sin el compositor. Asi <c>CustomProperties</c> viaja intacto por la propiedad
        /// del store que fija T-CPY-01. No prueba comportamiento (el Plugin no carga en las suites) y convive con G-R5, que
        /// sigue en <c>SelectiveDuplicationFailClosedTests</c> sin cambios.
        /// </summary>
        [Fact]
        public void TGrd03_ElRestampSerializaElMismoObjetoQueDeserializa()
        {
            Assert.Empty(EnvelopeSourceGuards.RestampShapeViolations(EnvelopeSourceGuards.RestampCode()));
        }

        public static TheoryData<string, string, string> RestampMutations()
            => new TheoryData<string, string, string>
            {
                {
                    "compone en lugar de re-estampar",
                    "store.Serialize(embed)",
                    "store.Serialize(RackEmbedComposer.Compose(embed, embed.Kind, embed.Id, embed.Name, embed.View, embed.Section, embed.Design))"
                },
                { "construye un sobre nuevo", "embed.Id = newIdText;", "embed = new RackEmbedDocument();\n            embed.Id = newIdText;" },
                { "deserializa dos veces", "store.Serialize(embed)", "store.Serialize(store.Deserialize(payload))" },
                { "serializa otro objeto", "store.Serialize(embed)", "store.Serialize(otro)" },
            };

        /// <summary>Rojo demostrado sobre una COPIA en memoria del archivo, nunca sobre el archivo del Plugin.</summary>
        [Theory]
        [MemberData(nameof(RestampMutations))]
        public void TGrd03_LaGuardaDetectaUnRestampQueNoReutilizaElObjeto(string caso, string original, string replacement)
        {
            var source = File.ReadAllText(EnvelopeSourceGuards.RestampFile());
            var mutated = source.Replace(original, replacement);

            Assert.True(mutated != source, "La mutacion '" + caso + "' ya no encuentra su texto en RackEnvelopeRestamp.cs.");
            Assert.NotEmpty(EnvelopeSourceGuards.RestampShapeViolations(PluginSourceCode.Mask(mutated)));
        }
    }

    /// <summary>I-54 G4B — lectura de las fuentes de produccion para T-GRD-01..03 y T-ENV-12.</summary>
    internal static class EnvelopeSourceGuards
    {
        internal const string ComposerPath = "src/RackCad.Application/Persistence/RackEmbedComposer.cs";

        internal const string RestampPath = "src/RackCad.Plugin/RackEnvelopeRestamp.cs";

        internal sealed record SourceFile(string Path, string Code);

        internal sealed record ComposeCall(string Path, string Member, string FirstArgument);

        /// <summary>Las llamadas de D-21 y la composicion pura autorizada por I-55 G7, ordenadas por archivo.</summary>
        internal static readonly IReadOnlyList<ComposeCall> ExpectedComposeCensus = new[]
        {
            new ComposeCall("src/RackCad.Application/Views/Preparation/RackViewEnvelopeComposition.cs", "Compose", "source"),
            new ComposeCall("src/RackCad.Plugin/ProjectVariableMutationExecutor.cs", "Execute", "view.Embed"),
            new ComposeCall("src/RackCad.Plugin/RackCabeceraCommands.cs", "BuildCabeceraPayload", "source"),
            new ComposeCall("src/RackCad.Plugin/RackCamaCommands.cs", "BuildCamaPayload", "sourceEmbed"),
            new ComposeCall("src/RackCad.Plugin/RackCantileverCommands.cs", "BuildCantileverPayload", "source"),
            new ComposeCall("src/RackCad.Plugin/RackDinamicoCommands.cs", "BuildDynamicPayload", "source"),
            new ComposeCall("src/RackCad.Plugin/RackPushBackCommands.cs", "BuildPushBackPayload", "source"),
            new ComposeCall("src/RackCad.Plugin/RackSelectivoCommands.cs", "WrapSelectivePayload", "source"),
        };

        private static readonly Regex ExplicitConstruction = new Regex(
            @"\bnew\s+(?:global\s*::\s*)?(?:[A-Za-z_]\w*\s*\.\s*)*RackEmbedDocument\b");

        private static readonly Regex TargetTypedDeclaration = new Regex(
            @"\bRackEmbedDocument\s*\??\s+[A-Za-z_]\w*\s*(?:\{[^{}]*\}\s*)?=\s*new\s*\(");

        private static readonly Regex TargetTypedExpressionBody = new Regex(
            @"\bRackEmbedDocument\s*\??\s+[A-Za-z_]\w*\s*\([^()]*\)\s*=>\s*new\s*\(");

        private static readonly Regex MethodReturningEnvelope = new Regex(
            @"\bRackEmbedDocument\s*\??\s+[A-Za-z_]\w*\s*\([^()]*\)\s*\{");

        private static readonly Regex ComposeReference = new Regex(@"\bRackEmbedComposer\s*\.\s*Compose\b");

        /// <summary>Todo el C# de produccion bajo <c>src/</c>, enmascarado, con ruta relativa al repositorio.</summary>
        internal static IReadOnlyList<SourceFile> ProductionSources()
        {
            var root = RepoRoot().FullName;

            return Directory.GetFiles(Path.Combine(root, "src"), "*.cs", SearchOption.AllDirectories)
                .Where(path => !path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                    .Any(segment => segment == "bin" || segment == "obj"))
                .Select(path => new SourceFile(
                    Path.GetRelativePath(root, path).Replace('\\', '/'),
                    PluginSourceCode.Mask(File.ReadAllText(path))))
                .OrderBy(file => file.Path, StringComparer.Ordinal)
                .ToList();
        }

        internal static string RestampFile() => Path.Combine(RepoRoot().FullName, "src", "RackCad.Plugin", "RackEnvelopeRestamp.cs");

        internal static string RestampCode() => PluginSourceCode.Mask(File.ReadAllText(RestampFile()));

        /// <summary><c>new RackEmbedDocument</c> y el <c>new(</c> con tipo destino declarado <c>RackEmbedDocument</c>.</summary>
        internal static IReadOnlyList<string> Constructions(string code)
        {
            var found = ExplicitConstruction.Matches(code).Cast<Match>()
                .Concat(TargetTypedDeclaration.Matches(code).Cast<Match>())
                .Concat(TargetTypedExpressionBody.Matches(code).Cast<Match>())
                .Select(match => match.Value)
                .ToList();

            foreach (Match method in MethodReturningEnvelope.Matches(code))
            {
                var open = method.Index + method.Length - 1;
                var body = code.Substring(open, Closing(code, open) - open);

                found.AddRange(Regex.Matches(body, @"\breturn\s+new\s*\(").Cast<Match>().Select(match => method.Value + " " + match.Value));
            }

            return found;
        }

        internal static IReadOnlyList<string> ConstructionsOutsideComposer(IEnumerable<SourceFile> sources)
            => sources
                .Where(file => file.Path != ComposerPath)
                .SelectMany(file => Constructions(file.Code).Select(construction => file.Path + ": " + construction))
                .ToList();

        /// <summary>
        /// Cada referencia a <c>RackEmbedComposer.Compose</c> fuera del compositor, con el metodo que la contiene y su primer
        /// argumento. Una referencia que no es una llamada dentro de un metodo reconocible (un constructor, un inicializador,
        /// un grupo de metodos) aparece sin clasificar, para que el censo falle en vez de no verla.
        /// </summary>
        internal static IReadOnlyList<ComposeCall> ComposeCallsOutsideComposer(IEnumerable<SourceFile> sources)
        {
            var calls = new List<ComposeCall>();

            foreach (var file in sources.Where(source => source.Path != ComposerPath))
            {
                var references = ComposeReference.Matches(file.Code).Count;

                if (references == 0)
                {
                    continue;
                }

                var classified = 0;

                foreach (var member in PluginSourceCode.Members(file.Code))
                {
                    foreach (var call in PluginSourceCode.Calls(member.Body, "Compose"))
                    {
                        if (!Regex.IsMatch(member.Body.Substring(0, call.Start), @"\bRackEmbedComposer\s*\.\s*$"))
                        {
                            continue;
                        }

                        calls.Add(new ComposeCall(
                            file.Path,
                            member.Name,
                            Regex.Replace(call.Arguments.FirstOrDefault() ?? string.Empty, @"\s+", string.Empty)));
                        classified++;
                    }
                }

                for (var i = classified; i < references; i++)
                {
                    calls.Add(new ComposeCall(file.Path, "(sin clasificar)", "(sin clasificar)"));
                }
            }

            return calls.OrderBy(call => call.Path, StringComparer.Ordinal).ToList();
        }

        /// <summary>Lo que T-GRD-03 exige de la sobrecarga con Guid de <c>RestampEnvelope</c>, como lista de violaciones.</summary>
        internal static IReadOnlyList<string> RestampShapeViolations(string code)
        {
            var violations = new List<string>();

            if (Regex.IsMatch(code, @"\bRackEmbedComposer\b"))
            {
                violations.Add("el restamp usa RackEmbedComposer");
            }

            violations.AddRange(Constructions(code).Select(construction => "el restamp construye un sobre: " + construction));

            var overloads = PluginSourceCode.Members(code)
                .Where(member => member.Name == "RestampEnvelope"
                                 && member.Parameters.Any(parameter => parameter.Type == "Guid" || parameter.Type == "System.Guid"))
                .ToList();

            if (overloads.Count != 1)
            {
                violations.Add("sobrecargas de RestampEnvelope con Guid: " + overloads.Count);
                return violations;
            }

            var body = overloads[0].Body;
            var deserializations = Regex.Matches(body, @"\bDeserialize\s*\(").Count;

            if (deserializations != 1)
            {
                violations.Add("Deserialize( aparece " + deserializations + " veces");
            }

            var assignments = Regex.Matches(
                body, @"(?<![\w.])(?:var\s+|RackEmbedDocument\s+)?(?<target>[A-Za-z_]\w*)\s*=(?!=)\s*(?:[\w.]+\.)?Deserialize\s*\(");

            if (assignments.Count != 1)
            {
                violations.Add("Deserialize( no se asigna a un unico identificador");
                return violations;
            }

            var target = assignments[0].Groups["target"].Value;
            var serializations = PluginSourceCode.Calls(body, "Serialize");

            if (serializations.Count != 1)
            {
                violations.Add("Serialize( aparece " + serializations.Count + " veces");
            }
            else if (serializations[0].Arguments.Count != 1
                     || Regex.Replace(serializations[0].Arguments[0], @"\s+", string.Empty) != target)
            {
                violations.Add("Serialize( no recibe el objeto deserializado '" + target + "': " + serializations[0].ArgumentText);
            }

            return violations;
        }

        private static int Closing(string code, int open)
        {
            var depth = 0;

            for (var i = open; i < code.Length; i++)
            {
                if (code[i] == '{')
                {
                    depth++;
                }
                else if (code[i] == '}' && --depth == 0)
                {
                    return i + 1;
                }
            }

            return code.Length;
        }
    }
}
