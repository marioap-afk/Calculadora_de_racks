using Xunit;

namespace RackCad.Tests
{
    /// <summary>I-55 G3: current product payload composition. It intentionally characterizes Plugin call sites.</summary>
    public sealed class LegacyViewPayloadCompositionCharacterizationTests
    {
        [Theory]
        [InlineData("RackSelectivoCommands.cs", "BuildSelectivePayload(", "RackEmbedComposer.Compose(", "RackEmbedDocument.KindSelective")]
        [InlineData("RackDinamicoCommands.cs", "BuildDynamicPayload(", "RackEmbedComposer.Compose(", "RackEmbedDocument.KindDynamic")]
        [InlineData("RackPushBackCommands.cs", "BuildPushBackPayload(", "RackEmbedComposer.Compose(", "RackEmbedDocument.KindPushBack")]
        [InlineData("RackCantileverCommands.cs", "BuildCantileverPayload(", "RackEmbedComposer.Compose(", "RackEmbedDocument.KindCantilever")]
        [InlineData("RackCabeceraCommands.cs", "BuildCabeceraPayload(", "RackEmbedComposer.Compose(", "RackEmbedDocument.KindCabecera")]
        [InlineData("RackCamaCommands.cs", "BuildCamaPayload(", "RackEmbedComposer.Compose(", "RackEmbedDocument.KindCama")]
        public void EachProductReaderComposesOneTypedEnvelope(string file, string builder, string composer, string kind)
        {
            var source = I55ProductCharacterizationTestSupport.Code("src", "RackCad.Plugin", file);
            Assert.Contains(builder, source);
            Assert.Contains(composer, source);
            Assert.Contains(kind, source);
        }

        [Fact]
        public void SelectiveSerializesAuthoredOnceAndWrapsEverySiblingFromThatJson()
        {
            var source = I55ProductCharacterizationTestSupport.Code("src", "RackCad.Plugin", "RackSelectivoCommands.cs");
            var edit = I55ProductCharacterizationTestSupport.Body(source, "internal static void EditSelective(");
            Assert.Contains("var designJson = new SelectivePalletDesignStore().Serialize(reconciled.Authored)", edit);
            Assert.True(I55ProductCharacterizationTestSupport.Count(edit, "WrapSelectivePayload(designJson") >= 3);
            Assert.Equal(1, I55ProductCharacterizationTestSupport.Count(edit, "SelectivePalletDesignStore().Serialize"));
        }

        [Fact]
        public void EveryEnvelopeCarriesKindIdNameViewSectionAndDesign()
        {
            foreach (var file in new[] { "RackSelectivoCommands.cs", "RackDinamicoCommands.cs", "RackPushBackCommands.cs", "RackCantileverCommands.cs", "RackCabeceraCommands.cs", "RackCamaCommands.cs" })
            {
                var source = I55ProductCharacterizationTestSupport.Code("src", "RackCad.Plugin", file);
                Assert.Contains("RackEmbedComposer.Compose(", source);
                Assert.Contains("id", source);
                Assert.Contains("name", source);
                Assert.Contains("design", source, System.StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}
