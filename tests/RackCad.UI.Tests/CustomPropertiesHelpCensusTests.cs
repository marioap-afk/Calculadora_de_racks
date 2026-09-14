using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-54 G7 — T-GRD-08 del lado de la ayuda (Proposal V5 D-18.7): la referencia de RACKAYUDA documenta `RACKPROPIEDADES`
    /// con su unico alias `RPR`, y el censo de la ayuda se fija por PARES comando/alias, no por un numero.
    ///
    /// <para>
    /// Hasta G7 la ayuda no tenia censo propio. Se mide al abrir G7 (14 pares en `5c16ae8`) y se fija con el par nuevo. No se
    /// exige documentar los comandos que la ayuda ya no traia al abrir G7 —eso seria alcance ajeno—, pero todo lo que la
    /// ayuda nombra tiene que existir como registro del Plugin.
    /// </para>
    /// </summary>
    public sealed class CustomPropertiesHelpCensusTests
    {
        /// <summary>El censo de APERTURA de la ayuda, medido en `5c16ae8`.</summary>
        private static readonly (string Command, string Alias)[] OpeningHelp =
        {
            ("RACKCAD", "RK"), ("RACKSELECTIVO", "RS"), ("RACKCABECERA", "RCB"), ("QUICKCABECERA", "QCB"),
            ("RACKSISTEMADINAMICO", "RSD"), ("QUICKCAMA", "QCM"), ("RACKCANTILEVER", "RCT"), ("RACKEDITAR", "RED"),
            ("RACKDUPLICAR", "RD"), ("RACKLAYOUT", "RLY"), ("RACKRELLENAR", "RR"), ("RACKLISTA", "RL"),
            ("RACKBOMTOTAL", "RB"), ("RACKAYUDA", "RA"),
        };

        private static readonly (string Command, string Alias) G7Help = ("RACKPROPIEDADES", "RPR");

        [Fact]
        public void TGrd08_ElCensoDeLaAyudaEsElDeAperturaMasRackPropiedades()
        {
            Assert.Equal(14, OpeningHelp.Length);
            Assert.Empty(HelpViolations(RackCommandReference.Commands.Select(info => (info.Command, info.Alias))));
        }

        [Fact]
        public void TGrd08_LaAyudaDocumentaRackPropiedadesConSuUnicoAlias()
        {
            var entry = Assert.Single(RackCommandReference.Commands, info => info.Command == "RACKPROPIEDADES");

            Assert.Equal("RPR", entry.Alias);
            Assert.Equal("Editar y copiar", entry.Group);
            Assert.Single(RackCommandReference.Commands, info => info.Alias == "RPR");
            Assert.Single(RackCommandReference.Commands, info => info.Command.Contains("PROPIEDADES", StringComparison.Ordinal));
        }

        [Fact]
        public void TGrd08_ElResumenCuentaLoQueHace_SinFormulasNiPlantillas()
        {
            var summary = RackCommandReference.Commands.Single(info => info.Command == "RACKPROPIEDADES").Summary;

            // Lo que la orden de G7 pide contar: propiedades estables, los dos alcances, los diagnosticos de solo lectura y la
            // unificacion cuando las vistas de un rack difieren.
            foreach (var required in new[] { "propiedades", "estables", "rack", "proyecto", "solo lectura", "unific", "vistas" })
            {
                Assert.Contains(required, summary, StringComparison.OrdinalIgnoreCase);
            }

            // Y lo que no se documenta: las propiedades no son formulas, expresiones, plantillas ni variables de proyecto.
            foreach (var forbidden in new[] { "fórmula", "formula", "expresi", "plantilla", "variable" })
            {
                Assert.DoesNotContain(forbidden, summary, StringComparison.OrdinalIgnoreCase);
            }
        }

        [Fact]
        public void TGrd08_CadaComandoYAliasDeLaAyudaEstaRegistradoEnElPlugin()
        {
            var registered = PluginCommandNames();

            // La barrida mira de verdad: el censo del Plugin abre con 33 nombres y G7 anade dos.
            Assert.Contains("RACKAYUDA", registered);
            Assert.True(registered.Count >= 35, "la barrida apenas ve registros del Plugin.");

            var unregistered = RackCommandReference.Commands
                .SelectMany(info => new[] { info.Command, info.Alias })
                .Where(name => !registered.Contains(name, StringComparer.Ordinal))
                .ToList();

            Assert.True(unregistered.Count == 0, "la ayuda nombra registros que el Plugin no tiene: " + string.Join(", ", unregistered));
        }

        public static TheoryData<string, string[]> HelpMutations() => new TheoryData<string, string[]>
        {
            { "falta la entrada", OpeningHelp.Select(Pair).ToArray() },
            { "otro alias", OpeningHelp.Select(Pair).Append("RACKPROPIEDADES/RPRO").ToArray() },
            { "otro nombre", OpeningHelp.Select(Pair).Append("RACKPROPERTIES/RPR").ToArray() },
            { "entrada duplicada", OpeningHelp.Select(Pair).Append(Pair(G7Help)).Append(Pair(G7Help)).ToArray() },
            { "una entrada de apertura desaparecida", OpeningHelp.Skip(1).Select(Pair).Append(Pair(G7Help)).ToArray() },
            { "una entrada de mas", OpeningHelp.Select(Pair).Append(Pair(G7Help)).Append("RACKPROPIEDADESPROYECTO/RPP").ToArray() },
        };

        [Theory]
        [MemberData(nameof(HelpMutations))]
        public void TGrd08_ElCensoDeLaAyudaDetectaCualquierDesviacion(string caso, string[] pairs)
        {
            var entries = pairs.Select(pair => pair.Split('/')).Select(parts => (parts[0], parts[1]));

            Assert.True(HelpViolations(entries).Count > 0, "el censo de la ayuda no detecta: " + caso);
        }

        private static string Pair((string Command, string Alias) entry) => entry.Command + "/" + entry.Alias;

        /// <summary>Vacia si la ayuda tiene exactamente los pares de apertura mas `RACKPROPIEDADES/RPR`, cada uno una vez.</summary>
        private static IReadOnlyList<string> HelpViolations(IEnumerable<(string Command, string Alias)> entries)
        {
            var violations = new List<string>();
            var expected = OpeningHelp.Append(G7Help).Select(Pair).ToList();
            var found = entries.Select(Pair).ToList();

            foreach (var group in found.GroupBy(pair => pair, StringComparer.Ordinal).Where(group => group.Count() > 1))
            {
                violations.Add("documentado " + group.Count() + " veces: " + group.Key);
            }

            violations.AddRange(expected.Except(found, StringComparer.Ordinal).Select(pair => "falta en la ayuda: " + pair));
            violations.AddRange(found.Except(expected, StringComparer.Ordinal).Select(pair => "sin clasificar en el censo de la ayuda: " + pair));
            return violations;
        }

        private static IReadOnlyList<string> PluginCommandNames()
        {
            var root = CustomPropertiesWindowTestKit.RepoRoot().FullName;

            return Directory.GetFiles(Path.Combine(root, "src", "RackCad.Plugin"), "*.cs", SearchOption.AllDirectories)
                .Where(path => !path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                    .Any(segment => segment == "bin" || segment == "obj"))
                .SelectMany(path => Regex.Matches(File.ReadAllText(path), @"\[CommandMethod\(""(?<name>[^""]*)""")
                    .Select(match => match.Groups["name"].Value))
                .ToList();
        }
    }
}
