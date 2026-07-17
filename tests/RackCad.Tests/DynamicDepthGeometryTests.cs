using System;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.Systems;
using RackCad.Application.Headers;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    public class DynamicDepthGeometryTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        [Fact]
        public void Resolve_ShortestFrontOwnsAllowancesAndLongerFrontContinuesThePattern()
        {
            var system = ResolveVariableDepthSystem();

            Assert.Equal(8, system.PalletsDeep);
            Assert.Equal(3, system.BaseDepthStartPosition);
            Assert.Equal(2, system.BasePalletsDeep);
            Assert.Equal(new[] { 48.0, 48.0, 54.0, 54.0, 48.0, 48.0, 48.0, 48.0 },
                system.Modules.Select(module => module.Length));
            Assert.Equal(new[] { true, false, true, true, false, true, false, true },
                system.Modules.Select(module => module.IsHeader));

            var longFront = system.Fronts[0];
            var shortFront = system.Fronts[1];
            Assert.Equal((1, 8), (longFront.DepthStartPosition, longFront.PalletsDeep));
            Assert.Equal((3, 2), (shortFront.DepthStartPosition, shortFront.PalletsDeep));
            Assert.Equal(396.0, longFront.EndX - longFront.StartX, 4);
            Assert.Equal(108.0, shortFront.EndX - shortFront.StartX, 4);
            Assert.True(longFront.LoadBeamLevels[0].EntranceElevation
                        > shortFront.LoadBeamLevels[0].EntranceElevation);
        }

        [Fact]
        public void Resolve_RejectsAFrontThatDoesNotContainTheShortestSharedStructure()
        {
            var design = Design();
            design.Fronts.Add(new DynamicRackFrontDesign { PalletsDeep = 2, DepthStartPosition = 3 });
            design.Fronts.Add(new DynamicRackFrontDesign { PalletsDeep = 3, DepthStartPosition = 1 });

            var error = Assert.Throws<ArgumentException>(() =>
                new DynamicRackSystemResolver(Catalog).Resolve(design));

            Assert.Contains("contener", error.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Document_RoundTripsPerFrontDepthAndStart_WithLegacyFallback()
        {
            var system = ResolveVariableDepthSystem();
            var document = DynamicRackSystemDocument.From(system);

            var restored = document.ToDesign();
            Assert.Equal(new[] { 8, 2 }, restored.Fronts.Select(front => front.PalletsDeep.GetValueOrDefault()));
            Assert.Equal(new[] { 1, 3 }, restored.Fronts.Select(front => front.DepthStartPosition.GetValueOrDefault()));

            document.Fronts[0].PalletsDeep = null;
            document.Fronts[0].DepthStartPosition = null;
            var legacy = document.ToDesign();
            Assert.Equal(document.PalletsDeep, legacy.Fronts[0].PalletsDeep);
            Assert.Equal(1, legacy.Fronts[0].DepthStartPosition);
        }

        [Fact]
        public void Bom_GroupsBedsByTheirOwnDepthInsteadOfTheSystemEnvelope()
        {
            var system = ResolveVariableDepthSystem();

            var beds = SystemBomBuilder.Build(system, Catalog).Components
                .Where(component => component.Category == SystemBomBuilder.Cama)
                .OrderBy(component => component.Length)
                .ToList();

            Assert.Equal(2, beds.Count);
            Assert.Equal(system.Fronts[1].EndX - system.Fronts[1].StartX - 4.0, beds[0].Length, 4);
            Assert.Equal(system.Fronts[0].EndX - system.Fronts[0].StartX - 4.0, beds[1].Length, 4);
            Assert.Equal(3, beds[0].Quantity);
            Assert.Equal(3, beds[1].Quantity);
        }

        [Fact]
        public void PlantaAndOuterLateral_UseThePhysicalEndpointsOfEachFront()
        {
            var system = ResolveVariableDepthSystem();
            var postId = DynamicFrontGeometry.PostId(system, Catalog);
            var peralte = DynamicFrontGeometry.PostPeralte(system, Catalog, postId);
            var troquel = SelectivePostGeometry.Resolve(
                Catalog.ConnectionLayout.FindConnectionLayout(
                    postId,
                    SelectiveRackDefaults.PostBeamPoint,
                    "PLANTA"),
                new System.Collections.Generic.Dictionary<string, double>
                {
                    [SelectiveRackDefaults.PeralteParam] = peralte
                });
            var plantaBeams = new DynamicSystemPlantaBuilder().Build(system, Catalog)
                .Where(instance => instance.PieceId == DynamicRackDefaults.InOutBeamCatalogId)
                .ToList();

            Assert.Equal(4, plantaBeams.Count);
            foreach (var front in system.Fronts)
            {
                Assert.Contains(plantaBeams, beam => Math.Abs(beam.Insertion.X - (front.StartX + troquel.X)) < 1e-4);
                Assert.Contains(plantaBeams, beam => Math.Abs(beam.Insertion.X - (front.EndX - troquel.X)) < 1e-4);
            }

            var shortFront = system.Fronts[1];
            var lateralBeams = new DynamicSystemLateralBuilder().Build(system, Catalog, postIndex: 2)
                .Flatten().Instances
                .Where(instance => instance.Role == HeaderBlockRole.Beam
                                   && instance.PieceId == DynamicRackDefaults.InOutBeamCatalogId)
                .ToList();
            Assert.Contains(lateralBeams, beam => Math.Abs(beam.Insertion.X - shortFront.StartX) < 1e-4);
            Assert.Contains(lateralBeams, beam => Math.Abs(beam.Insertion.X - shortFront.EndX) < 1e-4);
            Assert.DoesNotContain(lateralBeams, beam => Math.Abs(beam.Insertion.X) < 1e-4);
            Assert.DoesNotContain(lateralBeams, beam => Math.Abs(beam.Insertion.X - system.TotalLength) < 1e-4);
        }

        [Fact]
        public void SeparatorEndpoint_AddsAStandalonePostWithoutTurningItIntoAHeader()
        {
            var design = Design();
            design.Fronts.Add(new DynamicRackFrontDesign { PalletsDeep = 4, DepthStartPosition = 1 });
            design.Fronts.Add(new DynamicRackFrontDesign { PalletsDeep = 3, DepthStartPosition = 2 });
            design.Fronts.Add(new DynamicRackFrontDesign { PalletsDeep = 2, DepthStartPosition = 3 });
            var system = new DynamicRackSystemResolver(Catalog).Resolve(design).System;
            var separator = system.Modules[1];

            Assert.False(separator.IsHeader);
            var lateral = new DynamicSystemLateralBuilder().Build(system, Catalog, postIndex: 2).Flatten().Instances;
            Assert.Contains(lateral, instance => instance.Role == HeaderBlockRole.Post
                                                  && Math.Abs(instance.ConnectionAnchor.X - separator.StartX) < 1e-4);

            var layout = DynamicFrontGeometry.Compute(system, Catalog);
            var planta = new DynamicSystemPlantaBuilder().Build(system, Catalog);
            Assert.Contains(planta, instance => instance.Role == HeaderBlockRole.Post
                                                 && Math.Abs(instance.ConnectionAnchor.X - separator.StartX) < 1e-4
                                                 && Math.Abs(instance.ConnectionAnchor.Y - layout.PostPositions[2]) < 1e-4);

            var boundaryPosts = SystemBomBuilder.Build(system, Catalog).Components
                .Where(component => component.Category == SystemBomBuilder.DerivedPost)
                .Sum(component => component.Quantity);
            Assert.Equal(1, boundaryPosts);
        }

        private static DynamicRackSystem ResolveVariableDepthSystem()
        {
            var design = Design();
            design.Fronts.Add(new DynamicRackFrontDesign
            {
                PalletCount = 1,
                LoadLevels = 3,
                PalletsDeep = 8,
                DepthStartPosition = 1
            });
            design.Fronts.Add(new DynamicRackFrontDesign
            {
                PalletCount = 1,
                LoadLevels = 3,
                PalletsDeep = 2,
                DepthStartPosition = 3
            });
            return new DynamicRackSystemResolver(Catalog).Resolve(design).System;
        }

        private static DynamicRackDesign Design()
            => new DynamicRackDesign
            {
                Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                PalletsDeep = 8,
                LoadLevels = 3,
                FirstLevelHeight = 6.0,
                BeamDepth = 6.0,
                HeaderPostCatalogId = "POSTE_OMEGA_3X3"
            };
    }
}
