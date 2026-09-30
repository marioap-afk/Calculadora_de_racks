using System;
using System.IO;
using Xunit;

namespace RackCad.Tests
{
    public sealed class RackViewPlacementGuardTests
    {
        [Fact]
        public void PluginAdapterHasSixTypedSentinelsAndUsesExistingMaterializers()
        {
            var source = Source("src/RackCad.Plugin/Views/RackViewPlacement.cs");
            foreach (var sentinel in new[] { "PlaceSelective", "PlaceDynamic", "PlacePushBack", "PlaceCantilever", "PlaceHeader", "PlaceFlowBed" })
                Assert.Contains(sentinel + "(", source, StringComparison.Ordinal);
            Assert.Contains("SystemBlockWriter.CreatePreparedBlock", source, StringComparison.Ordinal);
            Assert.Contains("CantileverViewMaterializer.CreateBlockDefinitionNamed", source, StringComparison.Ordinal);
            Assert.Contains("new RackEmbedStore().Serialize(product.Envelope)", source, StringComparison.Ordinal);
            Assert.Contains("product.Prepared.BaseName", source, StringComparison.Ordinal);
        }

        [Fact]
        public void SingleViewSeamDoesNotReResolveReprepareOrCreateIdentity()
        {
            var source = Source("src/RackCad.Plugin/Views/RackViewPlacement.cs");
            Assert.DoesNotContain(".Resolve(", source, StringComparison.Ordinal);
            Assert.DoesNotContain(".Prepare(", source, StringComparison.Ordinal);
            Assert.DoesNotContain("Guid.NewGuid", source, StringComparison.Ordinal);
            Assert.DoesNotContain("RackViewCodec", source, StringComparison.Ordinal);
            Assert.DoesNotContain("dynamic ", source, StringComparison.Ordinal);
            Assert.DoesNotContain("object ", source, StringComparison.Ordinal);
        }

        [Fact]
        public void PermissivePolicyIsNamedForSingleViewAndContainsNoId19BatchConcepts()
        {
            var application = Source("src/RackCad.Application/Views/Placement/RackSingleViewPlacement.cs");
            var plugin = Source("src/RackCad.Plugin/Views/RackViewPlacement.cs");
            Assert.Contains("ID17/ID18", application, StringComparison.Ordinal);
            Assert.Contains("ID19 never calls", plugin, StringComparison.Ordinal);
            Assert.DoesNotContain("Sibling", plugin, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("Batch", plugin, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("CommonTransform", plugin, StringComparison.Ordinal);
        }

        [Fact]
        public void CancellationAndExceptionsUseObservableCleanupSeam()
        {
            var placement = Source("src/RackCad.Plugin/Drawing/BlockPlacement.cs");
            var adapter = Source("src/RackCad.Plugin/Views/RackViewPlacement.cs");
            Assert.Contains("PlaceDefinitionWithoutCleanup", placement, StringComparison.Ordinal);
            Assert.Contains("TryCleanupDefinition", placement, StringComparison.Ordinal);
            Assert.Contains("TryCleanupDefinition(document, block.DefinitionId)", placement, StringComparison.Ordinal);
            Assert.Contains("BlockPlacement.TryCleanupDefinition", adapter, StringComparison.Ordinal);
        }

        private static string Source(string relative)
        {
            var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../"));
            return File.ReadAllText(Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar)));
        }
    }
}
