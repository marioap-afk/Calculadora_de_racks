using Xunit;

namespace RackCad.Tests
{
    /// <summary>I-55 G3: current RACKEDITAR sibling preflights, before any geometry write.</summary>
    public sealed class RackEditPreflightCharacterizationTests
    {
        [Theory]
        [InlineData("RackPushBackCommands.cs", "RackEmbedDocument.KindPushBack", "IsValidPushBackDescriptor", "PreflightInnerSources", "RedrawInPlace")]
        [InlineData("RackCantileverCommands.cs", "RackEmbedDocument.KindCantilever", "IsValidCantileverDescriptor", "PreflightInnerSources", "RedefineBlock")]
        public void StrictReadersRejectWrongSiblingIdentityAndDescriptorBeforeWriting(
            string file, string kind, string descriptor, string inner, string firstWrite)
        {
            var source = I55ProductCharacterizationTestSupport.Code("src", "RackCad.Plugin", file);
            var body = I55ProductCharacterizationTestSupport.Body(source, file.Contains("PushBack") ? "internal static void EditPushBack(" : "internal static void EditCantilever(");
            Assert.Contains(kind, body);
            Assert.Contains("viewBlock.Embed.Id", body);
            Assert.Contains(descriptor, body);
            Assert.Contains(inner, body);
            Assert.True(body.IndexOf(kind, System.StringComparison.Ordinal) < body.IndexOf(firstWrite, System.StringComparison.Ordinal));
            Assert.True(body.IndexOf(descriptor, System.StringComparison.Ordinal) < body.IndexOf(firstWrite, System.StringComparison.Ordinal));
        }

        [Theory]
        [InlineData("RackDinamicoCommands.cs", "RackSystemKind.PalletFlow")]
        [InlineData("RackCabeceraCommands.cs", "RackSystemKind.Selective")]
        public void TolerantReadersStillAbortOnAnIncompatibleInnerProject(string file, string expectedKind)
        {
            var source = I55ProductCharacterizationTestSupport.Code("src", "RackCad.Plugin", file);
            Assert.Contains("PreflightInnerSources", source);
            Assert.Contains(expectedKind, source);
            Assert.Contains("if (preflight.Aborted)", source);
            Assert.Contains("return;", source);
        }

        [Fact]
        public void CurrentRedrawsOwnTheirTransactionsPerViewThroughHistoricalWrappers()
        {
            foreach (var file in new[] { "RackSelectivoCommands.cs", "RackDinamicoCommands.cs", "RackPushBackCommands.cs", "RackCabeceraCommands.cs", "RackCamaCommands.cs" })
                Assert.Contains("RedrawInPlace(", I55ProductCharacterizationTestSupport.Code("src", "RackCad.Plugin", file));
        }
    }
}
