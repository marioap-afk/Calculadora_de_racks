using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Xunit;
using static RackCad.Tests.ExpressionSemanticTestSupport;

namespace RackCad.Tests
{
    /// <summary>
    /// I-63 G4 / INV-29 (b) - el resumen es NEUTRAL: el proyecto de Application no declara referencias de AutoCAD ni de WPF.
    /// UNA sola funcion, <see cref="ForbiddenReferenceFamilies"/>, recorre el cierre de referencias declaradas de un
    /// <c>.csproj</c> segun A-1.1 y las clasifica en WPF o AutoCAD. El objetivo es <c>RackCad.Application.csproj</c> (ninguna);
    /// los controles positivos usan LA MISMA funcion sobre <c>RackCad.UI.csproj</c> (WPF) y <c>RackCad.Plugin.csproj</c>
    /// (AutoCAD). Se usan los proyectos porque el job de Core no compila UI ni Plugin. Solo se LEEN los <c>.csproj</c>.
    /// </summary>
    public class ComputedParametersSummaryGuardTests
    {
        private const string AutoCad = "AutoCAD";
        private const string Wpf = "WPF";

        private static readonly string[] AutoCadAssemblies = { "AcCoreMgd", "AcDbMgd", "AcMgd" };
        private static readonly string[] WpfAssemblies = { "PresentationFramework", "PresentationCore", "WindowsBase" };

        /// <summary>
        /// A-1.1: AutoCAD por <c>PackageReference Include="AutoCAD.NET"</c> o <c>Reference</c> a AcCoreMgd, AcDbMgd o AcMgd;
        /// WPF por <c>UseWPF</c> = true o <c>Reference</c> a PresentationFramework, PresentationCore o WindowsBase. Toma la
        /// UNION de todos los <c>ItemGroup</c> y <c>PropertyGroup</c> SIN evaluar ningun <c>Condition</c>, sigue los
        /// <c>ProjectReference</c> recursivamente y NO inspecciona dependencias transitivas de los paquetes.
        /// </summary>
        private static IReadOnlyList<string> ForbiddenReferenceFamilies(string csprojPath)
        {
            var families = new SortedSet<string>(StringComparer.Ordinal);
            Visit(Path.GetFullPath(csprojPath), new HashSet<string>(StringComparer.OrdinalIgnoreCase), families);
            return families.ToList();
        }

        private static void Visit(string path, HashSet<string> visited, SortedSet<string> families)
        {
            if (!visited.Add(path))
            {
                return;
            }

            var directory = Path.GetDirectoryName(path);
            foreach (var element in XDocument.Load(path).Descendants())
            {
                var include = ((string)element.Attribute("Include") ?? (string)element.Attribute("Update") ?? string.Empty).Trim();

                switch (element.Name.LocalName)
                {
                    case "PackageReference":
                        if (string.Equals(include, "AutoCAD.NET", StringComparison.OrdinalIgnoreCase))
                        {
                            families.Add(AutoCad);
                        }

                        break;

                    case "Reference":
                        var assembly = include.Split(',')[0].Trim();
                        if (AutoCadAssemblies.Contains(assembly, StringComparer.OrdinalIgnoreCase))
                        {
                            families.Add(AutoCad);
                        }

                        if (WpfAssemblies.Contains(assembly, StringComparer.OrdinalIgnoreCase))
                        {
                            families.Add(Wpf);
                        }

                        break;

                    case "UseWPF":
                        if (string.Equals(element.Value.Trim(), "true", StringComparison.OrdinalIgnoreCase))
                        {
                            families.Add(Wpf);
                        }

                        break;

                    case "ProjectReference":
                        if (include.Length > 0)
                        {
                            Visit(
                                Path.GetFullPath(Path.Combine(directory, include.Replace('\\', Path.DirectorySeparatorChar))),
                                visited,
                                families);
                        }

                        break;
                }
            }
        }

        private static string Project(string relative)
            => Path.Combine(RepoRoot().FullName, relative.Replace('/', Path.DirectorySeparatorChar));

        [Fact]
        public void INV29_ApplicationNoDeclaraFamiliasProhibidas_YLosControlesPositivosSiLasReconocenConLaMismaFuncion()
        {
            // Objetivo: ninguna familia en el cierre de referencias de Application.
            Assert.Empty(ForbiddenReferenceFamilies(Project("src/RackCad.Application/RackCad.Application.csproj")));

            // Controles positivos con LA MISMA funcion: si no reconociera las familias, estos fallarian.
            Assert.Equal(new[] { Wpf }, ForbiddenReferenceFamilies(Project("src/RackCad.UI/RackCad.UI.csproj")));

            var plugin = ForbiddenReferenceFamilies(Project("src/RackCad.Plugin/RackCad.Plugin.csproj"));
            Assert.Contains(AutoCad, plugin);
            Assert.Contains(Wpf, plugin);
        }

        [Fact]
        public void INV29_LaMismaFuncionDiscrimina_LasFormasDeA11_YElCierreDeProjectReference()
        {
            var directory = Path.Combine(Path.GetTempPath(), "rackcad-g4-guard-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);

            try
            {
                string Write(string name, string body)
                {
                    var path = Path.Combine(directory, name);
                    File.WriteAllText(path, "<Project Sdk=\"Microsoft.NET.Sdk\">" + body + "</Project>");
                    return path;
                }

                // Sin nada prohibido: ni un paquete cualquiera (las transitivas no se inspeccionan).
                Assert.Empty(ForbiddenReferenceFamilies(Write(
                    "clean.csproj", "<ItemGroup><PackageReference Include=\"Newtonsoft.Json\" Version=\"13.0.3\" /></ItemGroup>")));

                // AutoCAD por PackageReference, aunque su ItemGroup lleve una Condition falsa (no se evalua).
                Assert.Equal(new[] { AutoCad }, ForbiddenReferenceFamilies(Write(
                    "package.csproj",
                    "<ItemGroup Condition=\"'$(Flag)' == 'true'\"><PackageReference Include=\"AutoCAD.NET\" Version=\"[25.0.1]\" /></ItemGroup>")));

                // AutoCAD por Reference, en cada una de las tres ensamblados.
                foreach (var assembly in new[] { "AcCoreMgd", "AcDbMgd", "AcMgd" })
                {
                    Assert.Equal(new[] { AutoCad }, ForbiddenReferenceFamilies(Write(
                        assembly + ".csproj",
                        "<ItemGroup Condition=\"'$(Flag)' != 'true'\"><Reference Include=\"" + assembly + "\"><HintPath>x</HintPath></Reference></ItemGroup>")));
                }

                // WPF por UseWPF (en un PropertyGroup condicionado) y por cada Reference de WPF.
                Assert.Equal(new[] { Wpf }, ForbiddenReferenceFamilies(Write(
                    "usewpf.csproj", "<PropertyGroup Condition=\"'$(Flag)' == 'x'\"><UseWPF>true</UseWPF></PropertyGroup>")));
                Assert.Empty(ForbiddenReferenceFamilies(Write(
                    "nowpf.csproj", "<PropertyGroup><UseWPF>false</UseWPF></PropertyGroup>")));

                foreach (var assembly in new[] { "PresentationFramework", "PresentationCore", "WindowsBase" })
                {
                    Assert.Equal(new[] { Wpf }, ForbiddenReferenceFamilies(Write(
                        assembly + ".csproj", "<ItemGroup><Reference Include=\"" + assembly + ", Version=4.0.0.0\" /></ItemGroup>")));
                }

                // El cierre de referencias: ProjectReference recursivo (con un ciclo que no cuelga).
                Write("leaf.csproj", "<PropertyGroup><UseWPF>true</UseWPF></PropertyGroup><ItemGroup><ProjectReference Include=\"root.csproj\" /></ItemGroup>");
                Write("middle.csproj", "<ItemGroup><ProjectReference Include=\"leaf.csproj\" /></ItemGroup>");
                var root = Write("root.csproj", "<ItemGroup><ProjectReference Include=\"middle.csproj\" /></ItemGroup>");
                Assert.Equal(new[] { Wpf }, ForbiddenReferenceFamilies(root));
            }
            finally
            {
                Directory.Delete(directory, recursive: true);
            }
        }
    }
}
