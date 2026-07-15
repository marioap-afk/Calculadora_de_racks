using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Headers;
using RackCad.Application.Persistence;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    public sealed class SelectiveDesviadorTests
    {
        private const string DesviadorId = "DESVIADOR_A_3";
        private const string PostId = "POSTE_OMEGA_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA";
        private const string BeamId = "LARGUERO_ESCALON_CAL14_3_REMACHES";
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static SelectivePalletDesign Design(
            bool medioFrente = false,
            bool floorBeam = false,
            string desviadorId = DesviadorId)
        {
            var design = new SelectivePalletDesign
            {
                PostId = PostId,
                PostPeralte = 3.0,
                PalletTolerance = 4.0,
                VerticalClearance = 6.0,
                PalletDepth = 48.0
            };
            var bay = new SelectiveBayDesign { FloorBeam = floorBeam };
            if (medioFrente)
            {
                bay.Segments.Add(new SelectiveSegment { Length = 30.0, Loaded = true });
                bay.Segments.Add(new SelectiveSegment { Length = 0.0, Loaded = true });
            }

            for (var i = 0; i < 3; i++)
            {
                bay.Levels.Add(new SelectiveCell
                {
                    Pallet = new Tarima { Frente = 40.0, Alto = 44.0 },
                    PalletCount = 2,
                    BeamId = BeamId,
                    BeamPeralte = 4.0
                });
            }

            design.Bays.Add(bay);
            design.SafetySelections.Add(new SelectiveSafetySelection
            {
                ElementId = desviadorId,
                Quantity = 1,
                Side = SafetySide.Both,
                DesviadorLongitud = 18.0,
                DesviadorPrimerNivelAltura = 18.0
            });
            return design;
        }

        private static SelectiveRackSystem Resolve(SelectivePalletDesign design)
            => new SelectiveGeometryResolver().Resolve(design, Catalog);

        [Theory]
        [InlineData("DESVIADOR_A_3")]
        [InlineData("DESVIADOR_A_4")]
        [InlineData("DESVIADOR_L_3")]
        [InlineData("DESVIADOR_L_3_5")]
        [InlineData("DESVIADOR_L_4")]
        [InlineData("DESVIADOR_L_4_5")]
        [InlineData("DESVIADOR_L_5")]
        public void Catalog_LoadsEveryVariantAsExclusiveDesviador_WithExactThreeViewBlocks(string desviadorId)
        {
            var entry = Assert.Single(Catalog.SafetyElements, e => e.Id == desviadorId);
            Assert.Equal(SelectiveSafetyDefaults.DesviadorType, entry.Type);
            Assert.True(SelectiveSafetyFamilies.IsExclusive(entry.Type));
            Assert.Equal(desviadorId + "_FRONTAL", Catalog.Blocks.FindBlock(desviadorId, "FRONTAL").BlockName);
            Assert.Equal(desviadorId + "_LATERAL", Catalog.Blocks.FindBlock(desviadorId, "LATERAL").BlockName);
            Assert.Equal(desviadorId + "_PLANTA", Catalog.Blocks.FindBlock(desviadorId, "PLANTA").BlockName);
        }

        [Theory]
        [InlineData("DESVIADOR_A_3")]
        [InlineData("DESVIADOR_A_4")]
        [InlineData("DESVIADOR_L_3")]
        [InlineData("DESVIADOR_L_3_5")]
        [InlineData("DESVIADOR_L_4")]
        [InlineData("DESVIADOR_L_4_5")]
        [InlineData("DESVIADOR_L_5")]
        public void EveryVariant_ReusesTheSamePlanDrawingAndBomRule(string desviadorId)
        {
            var system = Resolve(Design(desviadorId: desviadorId));

            Assert.Equal(12, SelectiveDesviadorPlan.Build(system, Catalog).PhysicalQuantity);

            var frontal = new SelectiveFrontalBuilder().Build(system, Catalog)
                .Where(i => i.Role == HeaderBlockRole.Safety && i.PieceId == desviadorId).ToList();
            var lateral = new SelectiveLateralBuilder().Cortes(system, Catalog)
                .SelectMany(c => c.Largueros)
                .Where(i => i.Role == HeaderBlockRole.Safety && i.PieceId == desviadorId).ToList();
            var planta = new SelectivePlantaBuilder().Build(system, Catalog)
                .Where(i => i.Role == HeaderBlockRole.Safety && i.PieceId == desviadorId).ToList();

            Assert.Equal(6, frontal.Count);
            Assert.Equal(12, lateral.Count);
            Assert.Equal(4, planta.Count);
            Assert.All(frontal, i => Assert.Equal(desviadorId + "_FRONTAL", i.BlockName));
            Assert.All(lateral, i => Assert.Equal(desviadorId + "_LATERAL", i.BlockName));
            Assert.All(planta, i => Assert.Equal(desviadorId + "_PLANTA", i.BlockName));

            var component = Assert.Single(
                SelectiveBomBuilder.Build(system, Catalog).Components,
                c => c.ProfileId == desviadorId);
            Assert.Equal(12, component.Quantity);
            Assert.Equal(18.0, component.Length, 4);
        }

        [Theory]
        [InlineData(8.0, false)]
        [InlineData(9.0, false)]
        [InlineData(10.0, true)]
        [InlineData(18.0, true)]
        [InlineData(19.0, false)]
        [InlineData(20.5, false)]
        public void Dimensions_AcceptOnlyEvenIntegralInchesAboveEight(double value, bool expected)
            => Assert.Equal(expected, SelectiveDesviadorPlan.IsValidEvenAbove8(value));

        [Fact]
        public void Plan_PlacesOnePerPostAndLoadLevel_OnBothMirroredAisleFaces()
        {
            var system = Resolve(Design());
            var plan = SelectiveDesviadorPlan.Build(system, Catalog);

            Assert.Equal(new[] { 3, 3 }, plan.LevelCounts);
            Assert.Equal(12, plan.PhysicalQuantity); // 2 posts × 3 load rows × 2 exterior faces
            Assert.Equal(6, plan.Spots.Count(s => s.Enabled && !s.Mirrored));
            Assert.Equal(6, plan.Spots.Count(s => s.Enabled && s.Mirrored));

            var selection = Assert.Single(system.SafetySelections);
            var first = Assert.Single(plan.Spots, s => s.PostIndex == 0 && s.Level == 0 && s.Face == SelectiveDesviadorPlan.AisleFace.Front);
            var upper = Assert.Single(plan.Spots, s => s.PostIndex == 0 && s.Level == 1 && s.Face == SelectiveDesviadorPlan.AisleFace.Front);
            var troquel = Catalog.ConnectionLayout.FindConnectionLayout(PostId, SelectiveRackDefaults.PostBeamPoint, SelectiveRackDefaults.View);
            var firstTroquelY = SelectivePostGeometry.Resolve(troquel, new Dictionary<string, double>
            {
                [SelectiveRackDefaults.PeralteParam] = system.PostPeralte
            }).Y;
            Assert.Equal(firstTroquelY + selection.DesviadorPrimerNivelAltura, first.Y, 4);
            Assert.Equal(system.Bays[0].Levels[0].Y - SelectiveDesviadorPlan.BeamYOffset, upper.Y, 4);
        }

        [Theory]
        [InlineData(SafetySide.Left, 6, 0, 2)]
        [InlineData(SafetySide.Right, 6, 6, 2)]
        [InlineData(SafetySide.Both, 12, 6, 4)]
        public void SideChoice_FiltersAisleFacesForDrawingAndBom(
            SafetySide side,
            int expectedPhysical,
            int expectedMirrored,
            int expectedPlanta)
        {
            var design = Design();
            Assert.Single(design.SafetySelections).Side = side;
            var system = Resolve(design);
            var plan = SelectiveDesviadorPlan.Build(system, Catalog);

            Assert.Equal(expectedPhysical, plan.PhysicalQuantity);
            Assert.Equal(expectedMirrored, plan.Spots.Count(s => s.Enabled && s.Mirrored));
            Assert.Equal(6, new SelectiveFrontalBuilder().Build(system, Catalog)
                .Count(i => i.Role == HeaderBlockRole.Safety && i.PieceId == DesviadorId));
            Assert.Equal(expectedPhysical, new SelectiveLateralBuilder().Cortes(system, Catalog)
                .SelectMany(c => c.Largueros).Count(i => i.Role == HeaderBlockRole.Safety && i.PieceId == DesviadorId));
            Assert.Equal(expectedPlanta, new SelectivePlantaBuilder().Build(system, Catalog)
                .Count(i => i.Role == HeaderBlockRole.Safety && i.PieceId == DesviadorId));
            Assert.Equal(expectedPhysical, Assert.Single(
                SelectiveBomBuilder.Build(system, Catalog).Components,
                c => c.ProfileId == DesviadorId).Quantity);
        }

        [Fact]
        public void Drawing_ProjectsTheSamePlanInThreeViews_AndBomKeepsPhysicalLevelCount()
        {
            var system = Resolve(Design());
            var frontal = new SelectiveFrontalBuilder().Build(system, Catalog)
                .Where(i => i.Role == HeaderBlockRole.Safety && i.PieceId == DesviadorId).ToList();
            var lateral = new SelectiveLateralBuilder().Cortes(system, Catalog)
                .SelectMany(c => c.Largueros).Where(i => i.Role == HeaderBlockRole.Safety && i.PieceId == DesviadorId).ToList();
            var planta = new SelectivePlantaBuilder().Build(system, Catalog)
                .Where(i => i.Role == HeaderBlockRole.Safety && i.PieceId == DesviadorId).ToList();

            Assert.Equal(6, frontal.Count); // the two depth faces overlap in frontal
            Assert.Equal(12, lateral.Count); // both physical faces × every level
            Assert.Equal(4, planta.Count); // levels overlap in plan, faces do not
            Assert.All(frontal, i => Assert.Equal(18.0, i.DynamicParameters[SelectiveRackDefaults.LengthParam], 4));
            Assert.All(lateral, i => Assert.Equal(DesviadorId + "_LATERAL", i.BlockName));
            Assert.All(planta, i => Assert.Equal(DesviadorId + "_PLANTA", i.BlockName));
            Assert.Equal(6, lateral.Count(i => i.MirroredX));
            Assert.Equal(2, planta.Count(i => i.MirroredX));

            var plan = SelectiveDesviadorPlan.Build(system, Catalog);
            Assert.All(planta, instance => Assert.Contains(
                plan.Spots,
                spot => spot.Enabled
                    && spot.Mirrored == instance.MirroredX
                    && Math.Abs(spot.DepthPostX - instance.Insertion.X) < 1e-4
                    && Math.Abs(spot.RunPostX - instance.Insertion.Y) < 1e-4));

            var component = Assert.Single(SelectiveBomBuilder.Build(system, Catalog).Components, c => c.ProfileId == DesviadorId);
            Assert.Equal(12, component.Quantity);
            Assert.Equal(18.0, component.Length, 4);
        }

        [Fact]
        public void Grid_DisablesBothFacesOfOnePostLevel_WithoutLosingOtherPlanLevels()
        {
            var design = Design();
            Assert.Single(design.SafetySelections).DesviadorOffCells.Add(new SelectiveGridCell { Frente = 0, Level = 1 });
            var plan = SelectiveDesviadorPlan.Build(Resolve(design), Catalog);

            Assert.Equal(10, plan.PhysicalQuantity);
            Assert.Equal(2, plan.Spots.Count(s => s.PostIndex == 0 && s.Level == 1 && !s.Enabled));
            Assert.False(SelectiveSafetyGrid.AllCellsOff(plan.LevelCounts, Assert.Single(design.SafetySelections).DesviadorOffCells));
        }

        [Fact]
        public void MedioFrente_IntermediatePostGetsEveryLevelAndBothAisleFaces()
        {
            var system = Resolve(Design(medioFrente: true));
            var plan = SelectiveDesviadorPlan.Build(system, Catalog);

            Assert.Equal(new[] { 3, 3, 3 }, plan.LevelCounts);
            Assert.Equal(18, plan.PhysicalQuantity);
            Assert.Equal(9, new SelectiveFrontalBuilder().Build(system, Catalog)
                .Count(i => i.Role == HeaderBlockRole.Safety && i.PieceId == DesviadorId));
            Assert.Equal(6, new SelectivePlantaBuilder().Build(system, Catalog)
                .Count(i => i.Role == HeaderBlockRole.Safety && i.PieceId == DesviadorId));
            Assert.Equal(18, Assert.Single(SelectiveBomBuilder.Build(system, Catalog).Components, c => c.ProfileId == DesviadorId).Quantity);
        }

        [Fact]
        public void FirstRow_UsesCustomFirstTroquelHeight_EvenWithFloorBeam()
        {
            var design = Design(floorBeam: true);
            Assert.Single(design.SafetySelections).DesviadorPrimerNivelAltura = 22.0;
            var system = Resolve(design);
            var plan = SelectiveDesviadorPlan.Build(system, Catalog);

            Assert.Equal(new[] { 3, 3 }, plan.LevelCounts);
            var first = Assert.Single(plan.Spots, s => s.PostIndex == 0 && s.Level == 0 && s.Face == SelectiveDesviadorPlan.AisleFace.Front);
            var troquel = Catalog.ConnectionLayout.FindConnectionLayout(PostId, SelectiveRackDefaults.PostBeamPoint, SelectiveRackDefaults.View);
            Assert.Equal(troquel.LocalY + 22.0, first.Y, 4);
            Assert.DoesNotContain(plan.Spots, s => Math.Abs(s.Y - (system.Bays[0].Levels[0].Y - SelectiveDesviadorPlan.BeamYOffset)) < 1e-4);
        }

        [Fact]
        public void ClearanceNoteRule_FlagsSelectedLevelsCloserThanLongitud()
        {
            var design = Design();
            Assert.Single(design.SafetySelections).DesviadorLongitud = 100.0;
            var plan = SelectiveDesviadorPlan.Build(Resolve(design), Catalog);

            Assert.NotEmpty(plan.ClearanceIssues);
            Assert.All(plan.ClearanceIssues, issue => Assert.True(issue.Clear < plan.Longitud));
        }

        [Fact]
        public void DeepCopyAndDocumentRoundTrip_PreserveDesviadorConfiguration_WithLegacyFallbacks()
        {
            var design = Design();
            var source = Assert.Single(design.SafetySelections);
            source.Side = SafetySide.Right;
            source.DesviadorLongitud = 20.0;
            source.DesviadorPrimerNivelAltura = 22.0;
            source.DesviadorOffCells.Add(new SelectiveGridCell { Frente = 1, Level = 2 });

            var copy = source.DeepCopy();
            Assert.Equal(SafetySide.Right, copy.Side);
            Assert.Equal(20.0, copy.DesviadorLongitud);
            Assert.Equal(22.0, copy.DesviadorPrimerNivelAltura);
            Assert.NotSame(source.DesviadorOffCells[0], copy.DesviadorOffCells[0]);

            var store = new SelectivePalletDesignStore();
            var restored = store.Deserialize(store.Serialize(SelectivePalletDesignDocument.From(design, "id", "Rack"))).ToDomain();
            var selection = Assert.Single(restored.SafetySelections);
            Assert.Equal(SafetySide.Right, selection.Side);
            Assert.Equal(20.0, selection.DesviadorLongitud);
            Assert.Equal(22.0, selection.DesviadorPrimerNivelAltura);
            Assert.Equal((1, 2), (Assert.Single(selection.DesviadorOffCells).Frente, Assert.Single(selection.DesviadorOffCells).Level));

            var legacy = new SelectivePalletDesignDocument
            {
                SafetySelections = new List<SafetySelectionDocument>
                {
                    new SafetySelectionDocument { ElementId = DesviadorId, Quantity = 1, Side = (int)SafetySide.Both }
                }
            }.ToDomain();
            Assert.Equal(SafetySide.Both, Assert.Single(legacy.SafetySelections).Side);
            Assert.Equal(SelectiveSafetyDefaults.DesviadorLongitud, Assert.Single(legacy.SafetySelections).DesviadorLongitud);
            Assert.Equal(SelectiveSafetyDefaults.DesviadorPrimerNivelAltura, Assert.Single(legacy.SafetySelections).DesviadorPrimerNivelAltura);
        }
    }
}
