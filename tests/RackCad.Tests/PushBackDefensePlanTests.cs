using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-42 (A1B-D2, contrato del dueño) — LA DEFENSA DE MONTACARGAS ES UNA PIEZA FISICA de un lado y una linea de
    /// postes, y los cuatro cortes de un rack compuesto la PROYECTAN. Ninguno vuelve a decidir si existe, a quien
    /// pertenece o de que tipo es: eso lo resolvio <see cref="PushBackDefensePlan"/> una sola vez sobre el rack.
    /// </summary>
    public class PushBackDefensePlanTests
    {
        private const string RealDefense = "DEFENSA_MONTACARGAS";
        private const string SecondDefense = "DEFENSA_MONTACARGAS_B";

        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        /// <summary>
        /// El catalogo de fabrica trae UNA sola pieza de defensa, asi que un rack con dos tipos distintos —uno por
        /// pasillo— no se puede expresar con el. Este catalogo anade una SEGUNDA pieza que dibuja el mismo bloque:
        /// cambia el id, que es justo lo que la prueba mide, y no cambia nada mas.
        ///
        /// <para>
        /// Es una COPIA: el catalogo cargado se comparte entre pruebas —lo cachea el proveedor—, asi que mutarlo
        /// contaminaria a todas las demas.
        /// </para>
        /// </summary>
        private static RackCatalog TwoPieceCatalog()
        {
            var loaded = JsonRackCatalogProvider.FromBaseDirectory().Load();
            var source = loaded.SafetyElements.First(entry =>
                string.Equals(entry.Id, RealDefense, StringComparison.OrdinalIgnoreCase));
            return new RackCatalog
            {
                PostProfiles = loaded.PostProfiles,
                TrussProfiles = loaded.TrussProfiles,
                BasePlates = loaded.BasePlates,
                FlowBedProfiles = loaded.FlowBedProfiles,
                BeamProfiles = loaded.BeamProfiles,
                Mensulas = loaded.Mensulas,
                SpacerProfiles = loaded.SpacerProfiles,
                ConnectionPoints = loaded.ConnectionPoints,
                Views = loaded.Views,
                Defaults = loaded.Defaults,
                SafetyElements = loaded.SafetyElements.Concat(new[]
                {
                    new SafetyElementCatalogEntry
                    {
                        Id = SecondDefense,
                        DisplayName = source.DisplayName,
                        Description = source.Description,
                        Type = source.Type,
                        Units = source.Units,
                        WeightEach = source.WeightEach,
                    },
                }).ToList(),
                Blocks = loaded.Blocks.Concat(loaded.Blocks
                    .Where(block => string.Equals(block.PieceId, RealDefense, StringComparison.OrdinalIgnoreCase))
                    .Select(block => new BlockCatalogEntry
                    {
                        PieceId = SecondDefense,
                        View = block.View,
                        BlockName = block.BlockName,
                        Layer = block.Layer,
                        Color = block.Color,
                        Scale = block.Scale,
                        Rotation = block.Rotation,
                    })
                    .ToList()).ToList(),
                ConnectionLayout = loaded.ConnectionLayout.Concat(loaded.ConnectionLayout
                    .Where(entry => string.Equals(entry.PieceId, RealDefense, StringComparison.OrdinalIgnoreCase))
                    .Select(entry => new ConnectionLayoutEntry
                    {
                        PieceId = SecondDefense,
                        ConnectionPointId = entry.ConnectionPointId,
                        View = entry.View,
                        LocalX = entry.LocalX,
                        LocalY = entry.LocalY,
                    })
                    .ToList()).ToList(),
            };
        }

        private static PushBackDesign Design(
            string defenseA, string defenseB, double gap = 0.0, int slotsA = 2, int slotsB = 2,
            RackCatalog catalog = null)
        {
            var design = PushBackCompositeStructureTests.Composite(
                slotsA: slotsA, slotsB: slotsB, deepA: 4, deepB: 4, levelsA: 2, levelsB: 2, gap: gap);
            design.Composite.DefaultTopology = PushBackCellTopology.Encontradas;
            foreach (var selection in new PushBackSafetyAuthority(catalog ?? Catalog).Defaults())
            {
                design.Structure.SafetySelections.Add(selection);
            }

            design.DefensePieceId = defenseA;
            design.SideB.DefensePieceId = defenseB;
            return design;
        }

        private static PushBackSystem Resolve(
            string defenseA, string defenseB, double gap = 0.0, int slotsA = 2, int slotsB = 2,
            RackCatalog catalog = null)
            => new PushBackResolver(catalog ?? Catalog).Resolve(
                Design(defenseA, defenseB, gap, slotsA, slotsB, catalog));

        private static IReadOnlyList<HeaderBlockInstance> DefenseOf(HeaderRunPlan plan, RackCatalog catalog)
            => plan.Flatten().Instances
                .Where(instance => PushBackDefensePlan.IsDefense(instance, catalog))
                .ToList();

        private static double LengthOf(HeaderBlockInstance instance)
            => instance.DynamicParameters.TryGetValue(SelectiveRackDefaults.LengthParam, out var value)
                ? Convert.ToDouble(value)
                : 0.0;

        private static IReadOnlyList<HeaderBlockInstance> Cut(
            PushBackSystem system, RackCatalog catalog, PushBackFrontalEnd end, PushBackSide side)
            => DefenseOf(new PushBackSystemFrontalBuilder().BuildPlan(system, catalog, end, side), catalog);

        // ---------------------------------------------------------------- el defecto medido (B3)

        [Fact]
        public void CompositeCut_ASideType_BSideNone_ShowsNoBDefense()
        {
            var catalog = Catalog;
            var system = Resolve(RealDefense, PushBackDefaults.NonePieceId, catalog: catalog);

            Assert.NotEmpty(Cut(system, catalog, PushBackFrontalEnd.EntradaSalida, PushBackSide.A));
            Assert.Empty(Cut(system, catalog, PushBackFrontalEnd.EntradaSalida, PushBackSide.B));
        }

        [Fact]
        public void CompositeCut_ASideNone_BSideType_ShowsNoADefense()
        {
            var catalog = Catalog;
            var system = Resolve(PushBackDefaults.NonePieceId, RealDefense, catalog: catalog);

            Assert.Empty(Cut(system, catalog, PushBackFrontalEnd.EntradaSalida, PushBackSide.A));
            Assert.NotEmpty(Cut(system, catalog, PushBackFrontalEnd.EntradaSalida, PushBackSide.B));
        }

        [Fact]
        public void CompositeCut_UsesDefensePieceIdOfItsPhysicalSide()
        {
            var catalog = TwoPieceCatalog();
            var system = Resolve(RealDefense, SecondDefense, catalog: catalog);

            var cutA = Cut(system, catalog, PushBackFrontalEnd.EntradaSalida, PushBackSide.A);
            var cutB = Cut(system, catalog, PushBackFrontalEnd.EntradaSalida, PushBackSide.B);

            Assert.NotEmpty(cutA);
            Assert.NotEmpty(cutB);
            Assert.All(cutA, instance => Assert.Equal(RealDefense, instance.PieceId));
            Assert.All(cutB, instance => Assert.Equal(SecondDefense, instance.PieceId));
        }

        [Fact]
        public void ReflectedBSide_DoesNotBorrowDefenseTypeFromA()
        {
            var catalog = TwoPieceCatalog();
            var system = Resolve(RealDefense, SecondDefense, catalog: catalog);

            var resolved = PushBackDefensePlan.Resolve(system, catalog);

            Assert.All(
                resolved.Where(defense => defense.Side == PushBackSide.B),
                defense => Assert.Equal(SecondDefense, defense.PieceId));
            Assert.All(
                resolved.Where(defense => defense.Side == PushBackSide.A),
                defense => Assert.Equal(RealDefense, defense.PieceId));
            Assert.Contains(resolved, defense => defense.Side == PushBackSide.B);
        }

        // ---------------------------------------------------------------- proyeccion, no re-decision

        [Fact]
        public void CompositeCut_DefenseProjectionIsSubsetOfResolvedPhysicalSet()
        {
            var catalog = Catalog;
            var system = Resolve(RealDefense, RealDefense, catalog: catalog);
            var resolved = PushBackDefensePlan.Resolve(system, catalog);

            foreach (var side in new[] { PushBackSide.A, PushBackSide.B })
            {
                var mine = resolved.Where(defense => defense.Side == side).ToList();
                var cut = Cut(system, catalog, PushBackFrontalEnd.EntradaSalida, side);

                Assert.Equal(mine.Count, cut.Count);
                Assert.All(cut, instance => Assert.Contains(
                    mine, defense => string.Equals(defense.PieceId, instance.PieceId, StringComparison.OrdinalIgnoreCase)));
                foreach (var defense in mine)
                {
                    Assert.Contains(cut, instance => Math.Abs(LengthOf(instance) - defense.Length) < 1e-6);
                }
            }
        }

        [Fact]
        public void CompositeCut_AsymmetricPerPostDefenseIsPreserved()
        {
            var catalog = Catalog;
            var design = Design(RealDefense, RealDefense, slotsA: 3, slotsB: 3, catalog: catalog);
            var selection = design.Structure.SafetySelections.First(entry =>
                string.Equals(entry.ElementId, RealDefense, StringComparison.OrdinalIgnoreCase));

            // El poste 1 lleva defensa SOLO en el pasillo de A; el 2, solo en el de B.
            selection.DefensaPosts.Add(new SafetyPostDefense { PostIndex = 1, ExitLength = 36.0, EntranceLength = 0.0 });
            selection.DefensaPosts.Add(new SafetyPostDefense { PostIndex = 2, ExitLength = 0.0, EntranceLength = 36.0 });

            var system = new PushBackResolver(catalog).Resolve(design);
            var resolved = PushBackDefensePlan.Resolve(system, catalog);

            Assert.Contains(resolved, d => d.Side == PushBackSide.A && d.PostLine == 1);
            Assert.DoesNotContain(resolved, d => d.Side == PushBackSide.B && d.PostLine == 1);
            Assert.DoesNotContain(resolved, d => d.Side == PushBackSide.A && d.PostLine == 2);
            Assert.Contains(resolved, d => d.Side == PushBackSide.B && d.PostLine == 2);

            var cutA = Cut(system, catalog, PushBackFrontalEnd.EntradaSalida, PushBackSide.A);
            var cutB = Cut(system, catalog, PushBackFrontalEnd.EntradaSalida, PushBackSide.B);
            Assert.Equal(resolved.Count(d => d.Side == PushBackSide.A), cutA.Count);
            Assert.Equal(resolved.Count(d => d.Side == PushBackSide.B), cutB.Count);
        }

        [Fact]
        public void CompositeDefense_PlantCutsBomSharePhysicalAuthority()
        {
            var catalog = Catalog;
            var system = Resolve(RealDefense, PushBackDefaults.NonePieceId, catalog: catalog);
            var resolved = PushBackDefensePlan.Resolve(system, catalog);

            var plant = DefenseOf(new PushBackSystemPlantaBuilder().BuildPlan(system, catalog), catalog);
            var cuts = Cut(system, catalog, PushBackFrontalEnd.EntradaSalida, PushBackSide.A).Count
                       + Cut(system, catalog, PushBackFrontalEnd.EntradaSalida, PushBackSide.B).Count;
            var bom = PushBackBomBuilder.Build(system, catalog).Lines
                .Where(line => string.Equals(line.ProfileId, RealDefense, StringComparison.OrdinalIgnoreCase))
                .Sum(line => line.Quantity);

            Assert.NotEmpty(resolved);
            Assert.Equal(resolved.Count, plant.Count);
            Assert.Equal(resolved.Count, cuts);
            Assert.Equal(resolved.Count, bom);
        }

        [Fact]
        public void CompositeDefense_GapZeroDoesNotCollapseSideIdentity()
        {
            var catalog = Catalog;
            var system = Resolve(RealDefense, RealDefense, gap: 0.0, catalog: catalog);

            var resolved = PushBackDefensePlan.Resolve(system, catalog);

            Assert.Equal(resolved.Count, resolved.Select(defense => defense.Identity).Distinct().Count());
            Assert.Contains(resolved, defense => defense.Side == PushBackSide.A);
            Assert.Contains(resolved, defense => defense.Side == PushBackSide.B);
            foreach (var line in resolved.Select(defense => defense.PostLine).Distinct())
            {
                Assert.Equal(2, resolved.Count(defense => defense.PostLine == line));
            }
        }

        // ---------------------------------------------------------------- lo que NO cambia

        [Fact]
        public void PosteriorCut_CarriesNoDefense_BecauseItIsNotALoadingAisle()
        {
            var catalog = Catalog;
            var system = Resolve(RealDefense, RealDefense, catalog: catalog);

            Assert.Empty(Cut(system, catalog, PushBackFrontalEnd.Posterior, PushBackSide.A));
            Assert.Empty(Cut(system, catalog, PushBackFrontalEnd.Posterior, PushBackSide.B));
        }

        [Fact]
        public void SingleSidedRack_ResolvesOnlySideA_AndItsCutKeepsDrawingIt()
        {
            var catalog = Catalog;
            var design = new PushBackDesign
            {
                Structure = new DynamicRackDesign
                {
                    Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                    PalletsDeep = 4,
                    LoadLevels = 2,
                    FirstLevelHeight = 4.0,
                    BeamDepth = 4.0,
                },
            };
            design.Structure.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 2, PalletsDeep = 4 });
            design.Structure.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 2, PalletsDeep = 4 });
            foreach (var selection in new PushBackSafetyAuthority(catalog).Defaults())
            {
                design.Structure.SafetySelections.Add(selection);
            }

            design.DefensePieceId = RealDefense;
            var system = new PushBackResolver(catalog).Resolve(design);
            var resolved = PushBackDefensePlan.Resolve(system, catalog);

            Assert.False(system.IsComposite);
            Assert.NotEmpty(resolved);
            Assert.All(resolved, defense => Assert.Equal(PushBackSide.A, defense.Side));
            Assert.Equal(
                resolved.Count,
                Cut(system, catalog, PushBackFrontalEnd.EntradaSalida, PushBackSide.A).Count);
        }

        [Fact]
        public void ADeactivatedBoundary_CarriesNoDefenseOnEitherSide()
        {
            var catalog = Catalog;
            var design = Design(RealDefense, RealDefense, slotsA: 3, slotsB: 3, catalog: catalog);
            design.Structure.Fronts[1].IsActive = false;

            var system = new PushBackResolver(catalog).Resolve(design);
            var resolved = PushBackDefensePlan.Resolve(system, catalog);

            Assert.All(resolved, defense => Assert.True(
                DynamicFrontActivation.BoundaryExists(system.Structure, defense.PostLine)));
        }

        /// <summary>
        /// Un lado llega una ranura mas lejos que el otro: en esa ultima linea SOLO el lado largo tiene pasillo que
        /// proteger, y del otro no hay almacenamiento, asi que esa cara no existe. Las dos orientaciones importan:
        /// el extremo lejano ya se apagaba solo por longitud, pero el CERCANO no —ahi la unica cosa que impide
        /// inventar una defensa es preguntar si la cara existe—.
        /// </summary>
        [Theory]
        [InlineData(3, 2)]
        [InlineData(2, 3)]
        public void ALineWithoutStorageOnOneSide_CarriesNoDefenseThere(int slotsA, int slotsB)
        {
            var catalog = Catalog;
            var system = Resolve(RealDefense, RealDefense, slotsA: slotsA, slotsB: slotsB, catalog: catalog);
            var resolved = PushBackDefensePlan.Resolve(system, catalog);
            var present = slotsA > slotsB ? PushBackSide.A : PushBackSide.B;
            var absent = slotsA > slotsB ? PushBackSide.B : PushBackSide.A;
            var last = Math.Max(slotsA, slotsB);

            Assert.Contains(resolved, defense => defense.Side == present && defense.PostLine == last);
            Assert.DoesNotContain(resolved, defense => defense.Side == absent && defense.PostLine == last);
            Assert.Equal(
                resolved.Count(defense => defense.Side == PushBackSide.A),
                Cut(system, catalog, PushBackFrontalEnd.EntradaSalida, PushBackSide.A).Count);
            Assert.Equal(
                resolved.Count(defense => defense.Side == PushBackSide.B),
                Cut(system, catalog, PushBackFrontalEnd.EntradaSalida, PushBackSide.B).Count);
        }

        [Fact]
        public void NoneOnBothSides_ResolvesNothingAndDrawsNothing()
        {
            var catalog = Catalog;
            var none = PushBackDefaults.NonePieceId;
            var system = Resolve(none, none, catalog: catalog);

            Assert.Empty(PushBackDefensePlan.Resolve(system, catalog));
            Assert.Empty(Cut(system, catalog, PushBackFrontalEnd.EntradaSalida, PushBackSide.A));
            Assert.Empty(Cut(system, catalog, PushBackFrontalEnd.EntradaSalida, PushBackSide.B));
        }
    }
}
