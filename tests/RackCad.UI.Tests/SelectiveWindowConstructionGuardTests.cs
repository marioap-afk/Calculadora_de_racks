using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-43, gate 8.6B (ARQ-43-04): ningún test construye <c>RackSelectiveWindow</c> por su cuenta.
    /// <para>
    /// El constructor público resuelve un <c>UserSettingsGateway</c> REAL, así que una construcción directa
    /// LEE <c>%APPDATA%\RackCad\settings.json</c> al abrir y lo ESCRIBE en cuanto el test toca los «Fondos
    /// destino». Eso hace dos daños a la vez: la suite pisa las preferencias de quien la ejecuta, y el
    /// resultado de un test pasa a depender de lo que otro test dejó escrito — exactamente la contaminación
    /// que hacía pasar <c>ACabeceraAimedAtAFondoWithoutThatPost…</c> por accidente.
    /// </para>
    /// <para>
    /// Toda construcción vive en <see cref="SelectiveWindowTestSupport"/>, que inyecta un gateway en memoria.
    /// La allowlist es ese único archivo: no se admite ninguna otra excepción.
    /// </para>
    /// </summary>
    public class SelectiveWindowConstructionGuardTests
    {
        /// <summary>El único archivo autorizado a construir la ventana.</summary>
        private const string Factory = "SelectiveWindowTestSupport.cs";

        private static readonly Regex Construction = new Regex(
            @"new\s+(RackCad\.UI\.Systems\.Selective\.)?RackSelectiveWindow\s*\(",
            RegexOptions.Compiled);

        private static DirectoryInfo RepoRoot()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "RackCad.sln")))
            {
                dir = dir.Parent;
            }

            Assert.True(dir != null, "No se localizó la raíz del repo (RackCad.sln).");
            return dir;
        }

        private static IEnumerable<string> TestSources()
        {
            var root = Path.Combine(RepoRoot().FullName, "tests", "RackCad.UI.Tests");
            Assert.True(Directory.Exists(root), root);

            return Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
                .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}")
                    && !p.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
                .OrderBy(p => p, StringComparer.Ordinal);
        }

        [Fact]
        public void OnlyTheFactoryConstructsTheSelectiveWindow()
        {
            var offenders = new List<string>();

            foreach (var path in TestSources())
            {
                var name = Path.GetFileName(path);
                if (string.Equals(name, Factory, StringComparison.Ordinal))
                {
                    continue; // la allowlist: aquí es donde se inyecta el gateway en memoria
                }

                var lines = File.ReadAllLines(path);
                for (var i = 0; i < lines.Length; i++)
                {
                    if (Construction.IsMatch(lines[i]))
                    {
                        offenders.Add($"{name}:{i + 1}: {lines[i].Trim()}");
                    }
                }
            }

            Assert.True(
                offenders.Count == 0,
                "Construir RackSelectiveWindow fuera de " + Factory + " usa el gateway REAL de settings: la suite "
                    + "pisa %APPDATA%\\RackCad\\settings.json y los tests se contaminan entre sí (I-43, ARQ-43-04). "
                    + "Usa SelectiveWindowTestSupport.Open(...). Construcciones directas encontradas ("
                    + offenders.Count + "):\n" + string.Join("\n", offenders));
        }

        /// <summary>
        /// El guard sería inútil si la allowlist no existiera o dejara de construir la ventana: en ese caso
        /// «cero infractores» sería cierto por vacío.
        /// </summary>
        [Fact]
        public void TheFactoryExists_AndIsWhereTheWindowIsBuilt()
        {
            var factory = TestSources().SingleOrDefault(p => string.Equals(Path.GetFileName(p), Factory, StringComparison.Ordinal));

            Assert.True(factory != null, Factory + " no existe: la allowlist del guard quedaría vacía y el guard pasaría por vacuidad.");
            Assert.Contains(File.ReadAllLines(factory), line => Construction.IsMatch(line));
        }
    }
}
