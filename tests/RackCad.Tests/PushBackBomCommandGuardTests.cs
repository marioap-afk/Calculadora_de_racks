using System;
using System.IO;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-42 (A1C/H11) — GUARDAS DE ORIGEN DEL COMANDO DE BOM.
    ///
    /// <para>
    /// El Plugin referencia AutoCAD, asi que esta suite no puede cargarlo (ADR-0003) y CI no tiene AutoCAD donde
    /// ejecutarlo. Estas guardas leen el <c>.cs</c> del comando como TEXTO y fijan lo que solo existe ahi: que
    /// <c>RACKBOMTOTAL</c> consulta la puerta de salida antes de construir nada, que ABORTA el total cuando algun
    /// rack esta bloqueado —un total al que le falta un rack no puede parecer completo— y que un rack ilegible
    /// nunca se salta en silencio.
    /// </para>
    /// <para>
    /// La decision en si es pura y esta probada aparte (<see cref="PushBackOutputGateTests"/>); lo que aqui se pin
    /// es que el comando la CONSUME.
    /// </para>
    /// </summary>
    public class PushBackBomCommandGuardTests
    {
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

        private static string ReadSource(params string[] parts)
        {
            var path = Path.Combine(RepoRoot().FullName, Path.Combine(parts));
            Assert.True(File.Exists(path), "No existe el archivo: " + path);
            return File.ReadAllText(path);
        }

        private static string BomTotal =>
            ReadSource("src", "RackCad.Plugin", "RackInventarioCommands.BomTotal.cs");

        private static string PushBackHandler =>
            ReadSource("src", "RackCad.Plugin", "KindHandlers", "PushBackKindHandler.cs");

        [Fact]
        public void BlockingDiagnostic_PreventsRackBomTotalOutput()
        {
            var source = BomTotal;

            // Pregunta la puerta ANTES de construir ningun BOM...
            Assert.Contains("OutputBlockedReason", source, StringComparison.Ordinal);
            Assert.True(
                source.IndexOf("OutputBlockedReason", StringComparison.Ordinal)
                < source.IndexOf("BuildRackBom(handlers[i]", StringComparison.Ordinal),
                "la puerta se consulta antes de construir el BOM de ningun rack");

            // ...y ABORTA el total, como ya hacia con un kind sin handler.
            Assert.Contains("RackBomOutputGate.DescribeBlocked(blocked)", source, StringComparison.Ordinal);
            Assert.Contains("if (blocked.Count > 0)", source, StringComparison.Ordinal);
        }

        [Fact]
        public void BlockingDiagnostic_CommandWritesTheReason()
        {
            var source = BomTotal;
            var abort = source.IndexOf("if (blocked.Count > 0)", StringComparison.Ordinal);

            Assert.True(abort > 0);
            var block = source.Substring(abort, Math.Min(400, source.Length - abort));
            Assert.Contains("editor.WriteMessage", block, StringComparison.Ordinal);
            Assert.Contains("DescribeBlocked", block, StringComparison.Ordinal);
            Assert.Contains("return;", block, StringComparison.Ordinal);
        }

        [Fact]
        public void UnreadableRack_IsNeverSkippedSilentlyByTheCommand()
        {
            var source = BomTotal;
            var skip = source.IndexOf("if (bom == null)", StringComparison.Ordinal);

            Assert.True(skip > 0, "el comando sigue teniendo el salto por payload ilegible");
            var block = source.Substring(skip, Math.Min(500, source.Length - skip));
            Assert.Contains("DescribeUnreadable", block, StringComparison.Ordinal);
            Assert.Contains("editor.WriteMessage", block, StringComparison.Ordinal);
        }

        [Fact]
        public void ThePushBackHandler_ConsumesTheSharedGateAndNotASecondRule()
        {
            var source = PushBackHandler;

            Assert.Contains("RackBomOutputGate.For(system).Reason", source, StringComparison.Ordinal);
            // No hay una segunda regla de validez escrita a mano en el Plugin.
            Assert.DoesNotContain("IsInvalidForBom", source, StringComparison.Ordinal);
            Assert.DoesNotContain("RequiredBedLength", source, StringComparison.Ordinal);
        }
    }
}
