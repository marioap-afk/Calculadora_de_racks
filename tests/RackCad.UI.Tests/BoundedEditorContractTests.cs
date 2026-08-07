using System;
using System.IO;
using System.Linq;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// El contrato NUEVO del arquetipo B, el que I-39C establece. Vive en una clase aparte de
    /// <see cref="BoundedEditorCharacterizationTests"/> a proposito: aquella conserva intacto el comportamiento
    /// anterior —incluidas, con <c>Skip</c>, las pruebas que este cambio deja obsoletas— de modo que la transicion
    /// base → ADR → contrato se lea entera en el historial y no como una prueba reescrita.
    ///
    /// <para>Autorizado por ADR-0029: D9 (contrato de tamano por arquetipo, el B no hereda los minimos del A),
    /// D11 (adoptar antes que abstraer) y D12 (la infraestructura compartida no conoce sistemas).</para>
    /// </summary>
    public sealed class BoundedEditorContractTests
    {
        private static DirectoryInfo RepoRoot()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "RackCad.sln"))) dir = dir.Parent;
            Assert.True(dir != null, "repo root (RackCad.sln) not found");
            return dir;
        }

        private static string ComponentDirectory()
            => Path.Combine(RepoRoot().FullName, "src", "RackCad.UI", "Systems", "Cantilever", "Components");

        // ---- 1. composicion: ningun shell del arquetipo B lleva nombre de sistema (D12) ----

        [Fact]
        public void LosCuatroXAMLNombranElShellNEUTRAL()
        {
            foreach (var file in Directory.GetFiles(ComponentDirectory(), "*.xaml"))
            {
                var xaml = File.ReadAllText(file);

                Assert.Contains("shell:RackBoundedEditorShell", xaml, StringComparison.Ordinal);
                Assert.Contains("xmlns:shell=\"clr-namespace:RackCad.UI.Shell\"", xaml, StringComparison.Ordinal);
                Assert.DoesNotContain("CantileverComponentEditorShell", xaml, StringComparison.Ordinal);
            }
        }

        [Fact]
        public void LaFachadaDeI39AYaNoExiste()
        {
            Assert.False(
                File.Exists(Path.Combine(ComponentDirectory(), "CantileverComponentEditorShell.cs")),
                "la fachada era un andamio con fecha de retiro escrita en su propio comentario");

            // Y no reaparece con otro nombre: nada en el ensamblado deriva del shell acotado.
            var derived = typeof(RackCad.UI.Shell.RackBoundedEditorShell).Assembly
                .GetTypes()
                .Where(type => typeof(RackCad.UI.Shell.RackBoundedEditorShell).IsAssignableFrom(type)
                    && type != typeof(RackCad.UI.Shell.RackBoundedEditorShell))
                .Select(type => type.FullName)
                .ToList();

            Assert.Empty(derived);
        }
    }
}
