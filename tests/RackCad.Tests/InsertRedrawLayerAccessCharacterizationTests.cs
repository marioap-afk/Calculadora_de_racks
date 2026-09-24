using Xunit;

namespace RackCad.Tests
{
    /// <summary>I-55 G3 source baseline for the current layer/write boundary and cancellation ownership.</summary>
    public sealed class InsertRedrawLayerAccessCharacterizationTests
    {
        [Theory]
        [InlineData("Systems", "Shared", "SystemBlockWriter.cs")]
        [InlineData("Drawing", "LateralHeaderDrawService.cs")]
        public void HistoricalRedrawWrappersOwnLockTransactionAndCommit(params string[] path)
        {
            var parts = new string[path.Length + 2];
            parts[0] = "src"; parts[1] = "RackCad.Plugin"; path.CopyTo(parts, 2);
            var source = I55ProductCharacterizationTestSupport.Code(parts);
            Assert.Contains("LockDocument", source);
            Assert.Contains("StartTransaction", source);
            Assert.Contains("Commit()", source);
        }

        [Fact]
        public void CurrentRedrawOpensDefinitionsAndEntitiesForWriteWithoutALockedLayerPreflight()
        {
            var drawer = I55ProductCharacterizationTestSupport.Code("src", "RackCad.Plugin", "Drawing", "LateralHeaderDrawer.cs");
            Assert.Contains("tr.GetObject(blockId, OpenMode.ForWrite)", drawer);
            Assert.Contains("tr.GetObject(id, OpenMode.ForWrite)", drawer);
            Assert.Contains("tr.GetObject(referenceId, OpenMode.ForWrite)", drawer);
            Assert.DoesNotContain("IsLocked", drawer);
        }

        [Fact]
        public void PlacementCancellationReturnsBeforeCommitInTheCurrentJig()
        {
            var source = I55ProductCharacterizationTestSupport.Code("src", "RackCad.Plugin", "Drawing", "BlockPlacement.cs");
            Assert.Contains("if (result.Status != PromptStatus.OK)", source);
            Assert.Contains("reference.Dispose()", source);
            Assert.Contains("transaction.Commit()", source);
            Assert.Contains("new JigPlacementResult(result.Status, ObjectId.Null)", source);
            Assert.Contains("PlaceDefinitionWithStatus", source);
        }

        [Fact]
        public void ProductMessagesStillReportPlacementAndRedrawOutcomesAtCommandBoundary()
        {
            foreach (var file in new[] { "RackSelectivoCommands.cs", "RackDinamicoCommands.cs", "RackPushBackCommands.cs", "RackCantileverCommands.cs", "RackCabeceraCommands.cs", "RackCamaCommands.cs" })
                Assert.Contains("WriteMessage", I55ProductCharacterizationTestSupport.Code("src", "RackCad.Plugin", file));
        }
    }
}
