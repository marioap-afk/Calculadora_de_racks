using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// Characterizes the pure cell state extracted from the dynamic editor window (I-21): validity, the design projection,
    /// deep-clone independence, resolved-level import and the edit-buffer apply. These mirror the values the window used to
    /// hold inline, so the tests double as an equivalence net for the extraction.
    /// </summary>
    public class DynamicEditorCellTests
    {
        [Fact]
        public void Default_MatchesTheWindowsInitialCellValues()
        {
            var cell = DynamicEditorCell.Default();

            Assert.Equal(42.0, cell.PalletFront);
            Assert.Equal(60.0, cell.PalletHeight);
            Assert.Equal(1000.0, cell.PalletWeight);
            Assert.Equal(DynamicRackDefaults.DefaultClearHeight, cell.ClearHeight);
            Assert.Equal(DynamicRackDefaults.InOutBeamCatalogId, cell.InOutBeamCatalogId);
            Assert.Equal(DynamicRackDefaults.DefaultBeamDepth, cell.InOutBeamDepth);
            Assert.Null(cell.BeamLengthOverride);
            Assert.Equal(DynamicRackDefaults.IntermediateBeamCatalogId, cell.IntermediateBeamCatalogId);
            Assert.Equal(DynamicRackDefaults.DefaultIntermediateBeamDepth, cell.IntermediateBeamDepth);
            Assert.True(cell.IsValid);
        }

        [Theory]
        [InlineData(0.0, 60.0, 1000.0, 2.0, 3.0, null, false)]   // pallet front must be > 0
        [InlineData(42.0, 0.0, 1000.0, 2.0, 3.0, null, false)]   // pallet height must be > 0
        [InlineData(42.0, 60.0, -1.0, 2.0, 3.0, null, false)]    // weight must be >= 0
        [InlineData(42.0, 60.0, 1000.0, 0.0, 3.0, null, false)]  // in/out depth must be > 0
        [InlineData(42.0, 60.0, 1000.0, 2.0, 3.0, -1.0, false)]  // override, if set, must be > 0
        [InlineData(42.0, 60.0, 0.0, 2.0, 3.0, 120.0, true)]     // weight 0 and a positive override are valid
        public void IsValid_MatchesTheInlineRule(
            double front, double height, double weight, double inOut, double intermediate, double? beamOverride, bool expected)
        {
            var cell = new DynamicEditorCell
            {
                PalletFront = front,
                PalletHeight = height,
                PalletWeight = weight,
                InOutBeamDepth = inOut,
                IntermediateBeamDepth = intermediate,
                BeamLengthOverride = beamOverride
            };

            Assert.Equal(expected, cell.IsValid);
        }

        [Fact]
        public void ToDesign_CopiesEveryEditableField()
        {
            var cell = new DynamicEditorCell
            {
                PalletFront = 45.0,
                PalletHeight = 62.0,
                PalletWeight = 900.0,
                ClearHeight = 7.0,
                InOutBeamCatalogId = "IN",
                InOutBeamDepth = 4.0,
                BeamLengthOverride = 130.0,
                IntermediateBeamCatalogId = "MID",
                IntermediateBeamDepth = 3.5
            };

            var design = cell.ToDesign();

            Assert.Equal(45.0, design.PalletFront);
            Assert.Equal(62.0, design.PalletHeight);
            Assert.Equal(900.0, design.PalletWeight);
            Assert.Equal(7.0, design.ClearHeight);
            Assert.Equal("IN", design.InOutBeamCatalogId);
            Assert.Equal(4.0, design.InOutBeamDepth);
            Assert.Equal(130.0, design.BeamLengthOverride);
            Assert.Equal("MID", design.IntermediateBeamCatalogId);
            Assert.Equal(3.5, design.IntermediateBeamDepth);
        }

        [Fact]
        public void Clone_IsAnIndependentCopy()
        {
            var original = new DynamicEditorCell { PalletFront = 40.0, BeamLengthOverride = 100.0 };
            var clone = original.Clone();
            clone.PalletFront = 99.0;
            clone.BeamLengthOverride = null;

            Assert.Equal(40.0, original.PalletFront);
            Assert.Equal(100.0, original.BeamLengthOverride);
        }

        [Fact]
        public void From_NullLevel_ReturnsDefault()
        {
            var cell = DynamicEditorCell.From(null);
            Assert.Equal(42.0, cell.PalletFront);
        }

        [Fact]
        public void From_ResolvedLevel_TakesItsValues()
        {
            var level = new DynamicRackLevel
            {
                Pallet = new PalletSpecification(front: 48.0, depth: 50.0, height: 66.0, weight: 800.0, weightUnit: "kg"),
                ClearHeight = 9.0,
                InOutBeamCatalogId = "IN2",
                InOutBeamDepth = 4.5,
                BeamLengthOverride = 140.0,
                IntermediateBeamCatalogId = "MID2",
                IntermediateBeamDepth = 3.25
            };

            var cell = DynamicEditorCell.From(level);

            Assert.Equal(48.0, cell.PalletFront);
            Assert.Equal(66.0, cell.PalletHeight);
            Assert.Equal(800.0, cell.PalletWeight);
            Assert.Equal(9.0, cell.ClearHeight);
            Assert.Equal("IN2", cell.InOutBeamCatalogId);
            Assert.Equal(4.5, cell.InOutBeamDepth);
            Assert.Equal(140.0, cell.BeamLengthOverride);
            Assert.Equal("MID2", cell.IntermediateBeamCatalogId);
            Assert.Equal(3.25, cell.IntermediateBeamDepth);
        }

        [Fact]
        public void Apply_CopiesTheEditBufferOntoTheCell()
        {
            var cell = DynamicEditorCell.Default();
            var values = new DynamicEditorValues
            {
                PalletFront = 44.0,
                PalletHeight = 61.0,
                PalletWeight = 750.0,
                ClearHeight = 6.0,
                InOutBeamCatalogId = "A",
                InOutBeamDepth = 4.0,
                BeamLengthOverride = 111.0,
                IntermediateBeamCatalogId = "B",
                IntermediateBeamDepth = 2.75
            };

            cell.Apply(values);

            Assert.Equal(44.0, cell.PalletFront);
            Assert.Equal(61.0, cell.PalletHeight);
            Assert.Equal(750.0, cell.PalletWeight);
            Assert.Equal(6.0, cell.ClearHeight);
            Assert.Equal("A", cell.InOutBeamCatalogId);
            Assert.Equal(4.0, cell.InOutBeamDepth);
            Assert.Equal(111.0, cell.BeamLengthOverride);
            Assert.Equal("B", cell.IntermediateBeamCatalogId);
            Assert.Equal(2.75, cell.IntermediateBeamDepth);
        }
    }
}
