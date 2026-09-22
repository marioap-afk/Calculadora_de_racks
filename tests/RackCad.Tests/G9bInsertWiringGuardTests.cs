using System;
using System.IO;
using Xunit;

namespace RackCad.Tests
{
    public class SingleScanInsertGuardTests
    {
        [Fact]
        public void Selective_port_has_one_physical_scan_and_reuses_its_snapshot()
        {
            var source = Source("src", "RackCad.Plugin", "RackSelectivoInsertIntegration.cs");
            Assert.Equal(1, Count(source, "RackSiblingScan.Capture("));
            Assert.Contains("snapshot.Membership", source);
            Assert.Contains("snapshot.Properties", source);
            Assert.Contains("snapshot.Envelopes", source);
            Assert.Contains("snapshot.Definitions", source);
        }

        [Theory]
        [InlineData("RackDinamicoCommands.cs")]
        [InlineData("RackPushBackCommands.cs")]
        [InlineData("RackCantileverCommands.cs")]
        [InlineData("RackCabeceraCommands.cs")]
        public void Unsupported_comparators_enter_the_one_scan_fail_closed_edge(string command)
            => Assert.Contains("RackUnsupportedSiblingInsert.Reject(", Source("src", "RackCad.Plugin", command));

        private static int Count(string text, string value)
        {
            var count = 0;
            for (var at = 0; (at = text.IndexOf(value, at, StringComparison.Ordinal)) >= 0; at += value.Length) count++;
            return count;
        }
        private static string Source(params string[] path) => File.ReadAllText(Path.Combine(Root(), Path.Combine(path)));
        private static string Root() => Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    }

    public class BlankIdPredicateScopeGuardTests
    {
        [Theory]
        [InlineData("RackSelectivoCommands.cs")]
        [InlineData("RackCabeceraCommands.cs")]
        public void Insertar_uses_whitespace_while_actualizar_keeps_empty_only(string command)
        {
            var source = File.ReadAllText(Path.Combine(Root(), "src", "RackCad.Plugin", command));
            Assert.Contains("window.UpdateOnly", source);
            Assert.Contains("string.IsNullOrEmpty(embed.Id)", source);
            Assert.Contains("string.IsNullOrWhiteSpace(embed.Id)", source);
        }
        private static string Root() => Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    }

    public class OrphanFirstJigTransactionGuardTests
    {
        [Fact]
        public void Mode_two_defers_erases_until_a_reference_was_placed()
        {
            var source = File.ReadAllText(Path.Combine(Root(), "src", "RackCad.Plugin", "RackSelectivoInsertIntegration.cs"));
            var placement = File.ReadAllText(Path.Combine(Root(), "src", "RackCad.Plugin", "Drawing", "BlockPlacement.cs"));
            var drag = placement.IndexOf("editor.Drag(jig)", StringComparison.Ordinal);
            var callback = placement.IndexOf("beforeCommit?.Invoke(transaction)", StringComparison.Ordinal);
            var append = placement.IndexOf("modelSpace.AppendEntity(reference)", callback, StringComparison.Ordinal);
            Assert.True(drag >= 0 && callback > drag && append > callback);
            Assert.Contains("deferredRedraw.RedrawUnits", source);
            Assert.Contains("deferredRedraw.EraseUnits", source);
            Assert.Contains("SiblingRedrawTransaction.ApplyInTransaction(transaction, deferredUnits)", source);
            Assert.DoesNotContain("AUTH-15", source.Replace("AUTH-15 is not integrated", string.Empty));
        }

        [Fact]
        public void Flow_bed_has_no_sibling_insert_wiring()
            => Assert.DoesNotContain("RackSiblingInsertRun", File.ReadAllText(
                Path.Combine(Root(), "src", "RackCad.Plugin", "RackCamaCommands.cs")));

        private static string Root() => Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    }
}
