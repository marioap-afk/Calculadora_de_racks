using System.Collections.Generic;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    public sealed class SelectiveSafetyGridTests
    {
        [Fact]
        public void LevelCounts_UsesResolvedBeams_NotTheFloorPalletDesignRow()
        {
            var design = new SelectivePalletDesign();
            var bay = new SelectiveBayDesign { FloorBeam = false };
            bay.Levels.Add(Cell()); // floor pallet: no beam
            bay.Levels.Add(Cell());
            bay.Levels.Add(Cell());
            design.Bays.Add(bay);

            var system = new SelectiveGeometryResolver().Resolve(design, new RackCatalog());

            Assert.Equal(2, system.Bays[0].Levels.Count);
            Assert.Equal(2, Assert.Single(SelectiveSafetyGrid.LevelCounts(system)));
        }

        [Fact]
        public void LevelCounts_ExposesTheLargestRealGridAcrossFondos()
        {
            var system = new SelectiveRackSystem { DepthCount = 2 };
            var front = new List<SelectiveBay> { BayWithLevels(2), BayWithLevels(1) };
            var back = new List<SelectiveBay> { BayWithLevels(3) };
            foreach (var bay in front) system.Bays.Add(bay);
            system.FondoBays.Add(front);
            system.FondoBays.Add(back);

            Assert.Collection(
                SelectiveSafetyGrid.LevelCounts(system),
                count => Assert.Equal(3, count),
                count => Assert.Equal(1, count));
        }

        [Fact]
        public void AllCellsOff_IgnoresDuplicateAndOutOfRangeLegacyCells()
        {
            var off = new List<SelectiveGridCell>
            {
                new SelectiveGridCell { Frente = 0, Level = 0 },
                new SelectiveGridCell { Frente = 0, Level = 0 },
                new SelectiveGridCell { Frente = 20, Level = 20 }
            };

            Assert.False(SelectiveSafetyGrid.AllCellsOff(new[] { 2 }, off));
            off.Add(new SelectiveGridCell { Frente = 0, Level = 1 });
            Assert.True(SelectiveSafetyGrid.AllCellsOff(new[] { 2 }, off));
        }

        [Fact]
        public void DeepCopy_CarriesEverySafetyFlagWithoutSharingMutableLists()
        {
            var source = new SelectiveSafetySelection
            {
                ElementId = "PARRILLA",
                Quantity = 4,
                Side = SafetySide.Right,
                TopeShared = false,
                TopeFondo = 2,
                TopeSaque = 7.5,
                TopeFrontal = true,
                DesviadorLongitud = 20.0,
                DesviadorPrimerNivelAltura = 22.0,
                ParrillaFrontal = false,
                ParrillaLateral = true,
                ParrillaFrente = 60.0,
                ParrillaCantidad = 2
            };
            source.PostSides.Add(new SafetyPostSide { PostIndex = 3, Side = SafetySide.Left });
            source.TopeOffCells.Add(new SelectiveGridCell { Frente = 1, Level = 2 });
            source.DesviadorOffCells.Add(new SelectiveGridCell { Frente = 3, Level = 1 });
            source.ParrillaOffCells.Add(new SelectiveGridCell { Frente = 2, Level = 1 });

            var copy = source.DeepCopy();

            Assert.Equal(source.ElementId, copy.ElementId);
            Assert.Equal(source.Quantity, copy.Quantity);
            Assert.Equal(source.Side, copy.Side);
            Assert.Equal(source.TopeShared, copy.TopeShared);
            Assert.Equal(source.TopeFondo, copy.TopeFondo);
            Assert.Equal(source.TopeSaque, copy.TopeSaque);
            Assert.Equal(source.TopeFrontal, copy.TopeFrontal);
            Assert.Equal(source.DesviadorLongitud, copy.DesviadorLongitud);
            Assert.Equal(source.DesviadorPrimerNivelAltura, copy.DesviadorPrimerNivelAltura);
            Assert.Equal(source.ParrillaFrontal, copy.ParrillaFrontal);
            Assert.Equal(source.ParrillaLateral, copy.ParrillaLateral);
            Assert.Equal(source.ParrillaFrente, copy.ParrillaFrente);
            Assert.Equal(source.ParrillaCantidad, copy.ParrillaCantidad);
            Assert.NotSame(source.PostSides[0], copy.PostSides[0]);
            Assert.NotSame(source.TopeOffCells[0], copy.TopeOffCells[0]);
            Assert.NotSame(source.DesviadorOffCells[0], copy.DesviadorOffCells[0]);
            Assert.NotSame(source.ParrillaOffCells[0], copy.ParrillaOffCells[0]);

            copy.PostSides[0].PostIndex = 99;
            copy.TopeOffCells[0].Level = 99;
            copy.DesviadorOffCells[0].Level = 99;
            Assert.Equal(3, source.PostSides[0].PostIndex);
            Assert.Equal(2, source.TopeOffCells[0].Level);
            Assert.Equal(1, source.DesviadorOffCells[0].Level);
        }

        [Fact]
        public void SafetyType_MatchesLegacyDeckAsParrillaOnly()
        {
            Assert.True(SelectiveSafetyDefaults.IsType("DECK", SelectiveSafetyDefaults.ParrillaType));
            Assert.True(SelectiveSafetyDefaults.IsType("parrilla", SelectiveSafetyDefaults.DeckLegacyType));
            Assert.False(SelectiveSafetyDefaults.IsType("BOTA", SelectiveSafetyDefaults.ParrillaType));
        }

        private static SelectiveCell Cell() => new SelectiveCell
        {
            Pallet = new Tarima { Frente = 42.0, Alto = 60.0 },
            PalletCount = 2,
            BeamId = "LARGUERO",
            BeamPeralte = 4.0
        };

        private static SelectiveBay BayWithLevels(int count)
        {
            var bay = new SelectiveBay();
            for (var i = 0; i < count; i++) bay.Levels.Add(new SelectiveLevel());
            return bay;
        }
    }
}
