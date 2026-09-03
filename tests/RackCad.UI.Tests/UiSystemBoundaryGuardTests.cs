using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using RackCad.Application.RackFrames;
using RackCad.UI.RackFrames;
using RackCad.UI.Systems.Dynamic;
using RackCad.UI.Systems.FlowBed;
using RackCad.UI.Systems.Larguero;
using RackCad.UI.Systems.PushBack;
using RackCad.UI.Systems.Selective;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-23 — la frontera por sistema DENTRO de <c>RackCad.UI</c>.
    ///
    /// Mover una ventana WPF no es mover un archivo: el <c>x:Class</c> del XAML, el namespace del code-behind
    /// y la URI de recurso que genera el compilador tienen que seguir coincidiendo, y ninguna de las tres cosas
    /// la vigila el compilador entera. Un <c>x:Class</c> mal puesto compila y falla al CONSTRUIR la ventana.
    ///
    /// Por eso esta guarda hace las tres comprobaciones: construye de verdad cada ventana migrada (lo que
    /// ejecuta <c>InitializeComponent</c> y resuelve su URI generada más el diccionario <c>AppStyles</c>),
    /// comprueba que cada <c>x:Class</c> corresponde a su carpeta, y comprueba que cada pack URI del código
    /// apunta a un recurso que existe.
    ///
    /// <c>NamespaceFolderGuardTests</c> (en RackCad.Tests) vigila la regla namespace↔carpeta como texto; esta
    /// vigila lo que solo se ve en tiempo de ejecución.
    /// </summary>
    public sealed class UiSystemBoundaryGuardTests
    {
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

        private static string UiRoot() => Path.Combine(RepoRoot().FullName, "src", "RackCad.UI");

        private static IEnumerable<string> UiFiles(string pattern)
            => Directory.EnumerateFiles(UiRoot(), pattern, SearchOption.AllDirectories)
                .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}")
                    && !p.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
                .OrderBy(p => p, StringComparer.Ordinal);

        // ---- 1. Las ventanas migradas se CONSTRUYEN de verdad ------------------------------------------------

        /// <summary>
        /// Construye cada ventana que I-23 movió. Si un <c>x:Class</c> o la URI generada quedaran mal, esto
        /// lanza; compilar no basta para detectarlo. La cabecera exige una configuración, así que se le pasa
        /// la estándar.
        /// </summary>
        [Fact]
        public void EveryMigratedWindow_ConstructsWithItsXamlInitialised()
        {
            var built = StaTestRunner.Run(() =>
            {
                var windows = new List<Window>
                {
                    SelectiveWindowTestSupport.Open(),
                    new RackDynamicSystemWindow(canInsertInAutoCad: false),
                    new RackPushBackSystemWindow(canInsertInAutoCad: false),
                    new RackFlowBedWindow(canInsertInAutoCad: false),
                    new RackLargueroWindow(),
                    new RackFrameConfiguratorWindow(
                        new HardcodedStandardRackFrameService().CreateDefault(), canInsertInAutoCad: false),
                };

                var names = new List<string>();
                foreach (var window in windows)
                {
                    // Content != null solo es cierto si InitializeComponent corrió y cargó el árbol del XAML.
                    Assert.True(
                        window.Content != null,
                        string.Concat(window.GetType().FullName, " no cargó su XAML."));
                    names.Add(window.GetType().FullName);
                    window.Close();
                }

                return names;
            });

            Assert.Equal(6, built.Count);
            Assert.All(built, name => Assert.StartsWith("RackCad.UI.", name, StringComparison.Ordinal));

            // Cada una vive ya en su frontera, no en la raíz plana.
            Assert.Contains("RackCad.UI.Systems.Selective.RackSelectiveWindow", built);
            Assert.Contains("RackCad.UI.Systems.Dynamic.RackDynamicSystemWindow", built);
            Assert.Contains("RackCad.UI.Systems.PushBack.RackPushBackSystemWindow", built);
            Assert.Contains("RackCad.UI.Systems.FlowBed.RackFlowBedWindow", built);
            Assert.Contains("RackCad.UI.Systems.Larguero.RackLargueroWindow", built);
            Assert.Contains("RackCad.UI.RackFrames.RackFrameConfiguratorWindow", built);
        }

        // ---- 2. x:Class corresponde a la carpeta y al code-behind --------------------------------------------

        [Fact]
        public void EveryXamlClass_MatchesItsFolderAndItsCodeBehindNamespace()
        {
            var offenders = new List<string>();
            foreach (var xaml in UiFiles("*.xaml"))
            {
                var text = File.ReadAllText(xaml);
                var match = Regex.Match(text, @"x:Class=""(?<fqn>[A-Za-z0-9_.]+)""");
                if (!match.Success)
                {
                    continue;                       // diccionarios de recursos (Themes) no llevan x:Class
                }

                var fqn = match.Groups["fqn"].Value;
                var relativeDir = Path.GetRelativePath(UiRoot(), Path.GetDirectoryName(xaml))
                    .Replace(Path.DirectorySeparatorChar, '.');
                var expectedNs = relativeDir == "." ? "RackCad.UI" : "RackCad.UI." + relativeDir;
                var expected = expectedNs + "." + Path.GetFileNameWithoutExtension(xaml);

                if (!string.Equals(fqn, expected, StringComparison.Ordinal))
                {
                    offenders.Add($"{Path.GetFileName(xaml)}: x:Class='{fqn}', su carpeta exige '{expected}'");
                    continue;
                }

                var codeBehind = xaml + ".cs";
                if (!File.Exists(codeBehind))
                {
                    offenders.Add($"{Path.GetFileName(xaml)}: no tiene code-behind junto al XAML");
                    continue;
                }

                var declared = File.ReadLines(codeBehind)
                    .FirstOrDefault(l => l.StartsWith("namespace ", StringComparison.Ordinal))
                    ?.Substring("namespace ".Length).Trim();

                if (!string.Equals(declared, expectedNs, StringComparison.Ordinal))
                {
                    offenders.Add($"{Path.GetFileName(codeBehind)}: declara '{declared}', el x:Class exige '{expectedNs}'");
                }
            }

            Assert.True(offenders.Count == 0, "x:Class y carpeta divergen:\n" + string.Join("\n", offenders));
        }

        // ---- 3. Los pack URI apuntan a recursos que existen --------------------------------------------------

        /// <summary>
        /// Las URI de recurso del repo son de forma absoluta (<c>/RackCad.UI;component/...</c>), que se resuelve
        /// contra el ensamblado y NO contra la carpeta del archivo que la escribe — por eso mover ventanas no
        /// las rompe. Esta prueba fija ese hecho: si alguien escribiera una URI relativa, la carpeta pasaría a
        /// importar y el siguiente movimiento la rompería en silencio.
        /// </summary>
        [Fact]
        public void EveryResourceUri_IsAssemblyRooted_AndPointsAtAFileThatExists()
        {
            var uri = new Regex(@"(?<uri>(pack://application:,,,)?/RackCad\.UI;component/(?<path>[A-Za-z0-9_./-]+\.xaml))");
            var offenders = new List<string>();

            foreach (var file in UiFiles("*.cs").Concat(UiFiles("*.xaml")))
            {
                foreach (Match m in uri.Matches(File.ReadAllText(file)))
                {
                    var target = Path.Combine(UiRoot(), m.Groups["path"].Value.Replace('/', Path.DirectorySeparatorChar));
                    if (!File.Exists(target))
                    {
                        offenders.Add($"{Path.GetFileName(file)}: '{m.Groups["uri"].Value}' no existe");
                    }
                }

                // Una Source relativa (sin ;component) ataría el recurso a la carpeta del consumidor.
                foreach (Match m in Regex.Matches(File.ReadAllText(file), @"Source=""(?<s>[^""]+\.xaml)"""))
                {
                    var value = m.Groups["s"].Value;
                    if (!value.Contains(";component/", StringComparison.Ordinal))
                    {
                        offenders.Add($"{Path.GetFileName(file)}: Source relativa '{value}' (usa /RackCad.UI;component/...)");
                    }
                }
            }

            Assert.True(offenders.Count == 0, "Recursos WPF mal referenciados:\n" + string.Join("\n", offenders));
        }
    }
}
