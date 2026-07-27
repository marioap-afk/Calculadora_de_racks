using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-23 (hallazgo E8) — la guarda que impide que los namespaces por sistema vuelvan a aplanarse.
    ///
    /// I-23 separó los tres <c>Systems</c> planos en <c>Selective</c>/<c>Dynamic</c>/<c>PushBack</c>/
    /// <c>FlowBed</c>/<c>Larguero</c>/<c>Shared</c>. Esa separación sobrevive solo si algo la COMPRUEBA: sin
    /// guarda, el archivo siguiente nace en la raíz plana o en el sistema equivocado y nadie se entera hasta
    /// que el namespace vuelve a mentir, que es exactamente el estado que E8 denunciaba.
    ///
    /// Se lee como TEXTO, igual que el resto de guardas por fuente del repo, porque debe ver los archivos de
    /// los CUATRO proyectos — incluido <c>RackCad.Plugin</c>, que estas pruebas no pueden referenciar (exige
    /// AutoCAD). El <c>.editorconfig</c> declara la misma regla para el IDE, pero no puede aplicarla en el
    /// build: el proyecto WPF compila vía un proyecto temporal (<c>RackCad.UI_&lt;hash&gt;_wpftmp</c>) y
    /// IDE0130 deriva de ese nombre el namespace esperado, produciendo advertencias falsas. Esta prueba es la
    /// autoridad real, y corre en CI.
    /// </summary>
    public class NamespaceFolderGuardTests
    {
        private static readonly Regex NamespaceLine =
            new Regex(@"^namespace\s+(?<ns>[A-Za-z0-9_.]+)\s*$", RegexOptions.Compiled);

        /// <summary>Los seis destinos por sistema. `Larguero` es un <c>RackSystemKind</c> real y también tiene
        /// el suyo: dejarlo suelto mantendría viva justo la raíz plana que I-23 elimina.</summary>
        private static readonly string[] SystemBuckets =
            { "Selective", "Dynamic", "PushBack", "FlowBed", "Larguero", "Shared" };

        private static DirectoryInfo RepoRoot()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "RackCad.sln")))
            {
                dir = dir.Parent;
            }

            Assert.True(dir != null, "No se localizó la raíz del repo (RackCad.sln) desde el directorio de salida.");
            return dir;
        }

        /// <summary>Todo .cs de produccion, con su ruta relativa a `src/` normalizada a '/'.</summary>
        private static IEnumerable<string> ProductionSources()
        {
            var src = Path.Combine(RepoRoot().FullName, "src");
            return Directory.EnumerateFiles(src, "*.cs", SearchOption.AllDirectories)
                .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}")
                    && !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
                .Select(path => Path.GetRelativePath(src, path).Replace(Path.DirectorySeparatorChar, '/'))
                .OrderBy(rel => rel, StringComparer.Ordinal);
        }

        private static string DeclaredNamespace(string relative)
        {
            var full = Path.Combine(RepoRoot().FullName, "src", relative.Replace('/', Path.DirectorySeparatorChar));
            foreach (var line in File.ReadLines(full))
            {
                var match = NamespaceLine.Match(line);
                if (match.Success)
                {
                    return match.Groups["ns"].Value;
                }
            }

            return null;
        }

        /// <summary>The namespace a file's folder demands: `src/<project>/<a>/<b>/F.cs` => `<project>.<a>.<b>`.</summary>
        private static string ExpectedNamespace(string relative)
        {
            var parts = relative.Split('/');
            return string.Join('.', parts.Take(parts.Length - 1));
        }

        [Fact]
        public void EveryProductionFile_DeclaresTheNamespaceItsFolderDemands()
        {
            var offenders = ProductionSources()
                .Select(rel => new { Rel = rel, Declared = DeclaredNamespace(rel), Expected = ExpectedNamespace(rel) })
                .Where(x => x.Declared != null && !string.Equals(x.Declared, x.Expected, StringComparison.Ordinal))
                .Select(x => $"{x.Rel}: declara '{x.Declared}', su carpeta exige '{x.Expected}'")
                .ToList();

            Assert.True(offenders.Count == 0, "Namespace y carpeta divergen:\n" + string.Join("\n", offenders));
        }

        [Fact]
        public void EveryProductionFile_DeclaresExactlyOneBlockScopedNamespace()
        {
            var offenders = new List<string>();
            foreach (var rel in ProductionSources())
            {
                var full = Path.Combine(RepoRoot().FullName, "src", rel.Replace('/', Path.DirectorySeparatorChar));
                var declarations = File.ReadLines(full)
                    .Where(line => line.StartsWith("namespace ", StringComparison.Ordinal))
                    .ToList();

                if (declarations.Count > 1)
                {
                    offenders.Add($"{rel}: {declarations.Count} namespaces en un archivo");
                }
                else if (declarations.Count == 1 && declarations[0].TrimEnd().EndsWith(";", StringComparison.Ordinal))
                {
                    // A file-scoped namespace would break the by-line check above.
                    offenders.Add($"{rel}: namespace file-scoped ('{declarations[0].Trim()}')");
                }
            }

            Assert.True(offenders.Count == 0, "Declaración de namespace no canónica:\n" + string.Join("\n", offenders));
        }

        [Fact]
        public void NoSystemFile_RemainsInTheFlatSystemsRoot()
        {
            var loose = ProductionSources()
                .Where(rel => Regex.IsMatch(rel, @"^RackCad\.[A-Za-z]+/Systems/[^/]+\.cs$"))
                .ToList();

            Assert.True(
                loose.Count == 0,
                "E8 volvería: hay archivos sueltos en la raíz plana de Systems, sin sistema asignado:\n"
                    + string.Join("\n", loose));
        }

        [Fact]
        public void EverySystemsSubfolder_IsOneOfTheSixDeclaredBuckets()
        {
            var strays = ProductionSources()
                .Select(rel => Regex.Match(rel, @"^RackCad\.[A-Za-z]+/Systems/(?<bucket>[^/]+)/"))
                .Where(m => m.Success)
                .Select(m => m.Groups["bucket"].Value)
                .Distinct(StringComparer.Ordinal)
                .Where(bucket => !SystemBuckets.Contains(bucket, StringComparer.Ordinal))
                .ToList();

            Assert.True(
                strays.Count == 0,
                "Un sistema nuevo necesita su fila en el contrato de I-23 antes que su carpeta. Encontrados: "
                    + string.Join(", ", strays));
        }

        /// <summary>
        /// El namespace fósil que E8 nombra no debe reaparecer. `DynamicSystemPlan` no era el plan del sistema
        /// dinámico sino el de corridas de cabecera, y lo consumían los cuatro sistemas; hoy es
        /// <c>Application.Drawing.HeaderRunPlan</c>.
        /// </summary>
        [Fact]
        public void TheDissolvedNamespacesAndTheFossilName_AreGone()
        {
            var dissolved = new[]
            {
                "RackCad.Application.Systems", "RackCad.Domain.Systems",
                "RackCad.Plugin.Systems", "RackCad.Application.Headers",
            };

            var offenders = new List<string>();
            foreach (var rel in ProductionSources())
            {
                var full = Path.Combine(RepoRoot().FullName, "src", rel.Replace('/', Path.DirectorySeparatorChar));
                var text = File.ReadAllText(full);

                foreach (var ns in dissolved)
                {
                    if (Regex.IsMatch(text, $@"^using {Regex.Escape(ns)};\s*$", RegexOptions.Multiline))
                    {
                        offenders.Add($"{rel}: using del namespace disuelto '{ns}'");
                    }
                }

                if (Regex.IsMatch(text, @"\bDynamicSystemPlan\b"))
                {
                    offenders.Add($"{rel}: nombre fósil 'DynamicSystemPlan' (hoy es Drawing.HeaderRunPlan)");
                }
            }

            Assert.True(offenders.Count == 0, string.Join("\n", offenders));
        }
    }
}
