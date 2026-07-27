using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Bom;
using RackCad.Application.Catalogs;
using RackCad.Application.Headers;
using RackCad.Application.Persistence;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-33, decisión del Owner — la FRONTERA compartida por dos frentes EN BLANCO no existe.
    ///
    /// <para>
    /// Un rack de N frentes tiene N+1 fronteras. Las dos exteriores siempre existen; una interior existe salvo que sus
    /// DOS frentes adyacentes estén en blanco, porque entonces no hay nada que sostener. Una corrida de N blancos
    /// conserva sólo sus dos fronteras exteriores y pierde sus N−1 interiores.
    /// </para>
    ///
    /// <para>
    /// Lo que desaparece es el ENSAMBLE físico. Los frentes lógicos siguen intactos: mismos índices, mismos claros,
    /// misma configuración dormida y — verificado aquí — mismas coordenadas X y mismo largo total.
    /// </para>
    /// </summary>
    public class BlankFrontBoundaryTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static DynamicRackDesign Design(int frontCount, params int[] blankFronts)
        {
            var design = new DynamicRackDesign
            {
                Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                PalletsDeep = 4,
                LoadLevels = 3,
                FirstLevelHeight = 6.0,
                BeamDepth = 4.0
            };
            for (var index = 0; index < frontCount; index++)
            {
                design.Fronts.Add(new DynamicRackFrontDesign
                {
                    PalletCount = 1 + (index % 2),
                    LoadLevels = 2 + (index % 3),
                    PalletsDeep = 4,
                    DepthStartPosition = 1,
                    IsActive = !blankFronts.Contains(index)
                });
            }

            return design;
        }

        private static DynamicRackSystem Resolve(DynamicRackDesign design)
            => new DynamicRackSystemResolver(Catalog).Resolve(design).System;

        private static DynamicRackSystem System(int frontCount, params int[] blankFronts)
            => Resolve(Design(frontCount, blankFronts));

        private static PushBackSystem PushBack(int frontCount, params int[] blankFronts)
            => new PushBackResolver(Catalog).Resolve(new PushBackDesign { Structure = Design(frontCount, blankFronts) });

        private static int[] Boundaries(DynamicRackSystem system)
            => DynamicFrontActivation.PresentBoundaries(system).ToArray();

        private static int PostCount(IEnumerable<HeaderBlockInstance> instances)
            => instances.Count(instance => instance.Role == HeaderBlockRole.Post);

        private static int PlateCount(IEnumerable<HeaderBlockInstance> instances)
            => instances.Count(instance => instance.Role == HeaderBlockRole.BasePlate);

        private static IReadOnlyList<HeaderBlockInstance> Frontal(DynamicRackSystem system)
            => new DynamicSystemFrontalBuilder().Build(system, Catalog, DynamicRackEnd.Exit);

        private static IReadOnlyList<HeaderBlockInstance> Planta(DynamicRackSystem system)
            => new DynamicSystemPlantaBuilder().Build(system, Catalog);

        private static int Quantity(BillOfMaterials bom, string category)
            => bom.Components.Where(component => component.Category == category).Sum(component => component.Quantity);

        /// <summary>Piece totals (Lines = components flattened × quantity): the real count of postes, placas, etc.</summary>
        private static int Pieces(BillOfMaterials bom, string category)
            => bom.Lines.Where(line => line.Category == category).Sum(line => line.Quantity);

        // ---- The authority itself ----------------------------------------------------------------------------

        [Fact]
        public void Authority_KeepsTheOuterEdges_AndDropsOnlyTheInteriorBoundariesOfABlankRun()
        {
            // 4 fronts => 5 boundaries. Nothing blank: all present.
            Assert.Equal(new[] { 0, 1, 2, 3, 4 }, Boundaries(System(4)));

            // A SINGLE blank front suppresses nothing: both of its boundaries touch an active front (or an edge).
            Assert.Equal(new[] { 0, 1, 2, 3, 4 }, Boundaries(System(4, 1)));

            // Two consecutive: the one BETWEEN them goes.
            Assert.Equal(new[] { 0, 1, 3, 4 }, Boundaries(System(4, 1, 2)));

            // Three consecutive: two go.
            Assert.Equal(new[] { 0, 1, 4 }, Boundaries(System(4, 1, 2, 3)));

            // Alternating: no two blanks are adjacent, so nothing goes.
            Assert.Equal(new[] { 0, 1, 2, 3, 4, 5 }, Boundaries(System(5, 0, 2, 4)));
        }

        [Fact]
        public void Authority_KeepsBothOuterEdgesEvenWhenTheRunTouchesThem()
        {
            // Blank run at the START: boundary 0 is the rack's edge and survives; boundary 1 is interior and goes.
            Assert.Equal(new[] { 0, 2, 3, 4 }, Boundaries(System(4, 0, 1)));

            // Blank run at the END: boundary 4 is the rack's edge and survives; boundary 3 goes.
            Assert.Equal(new[] { 0, 1, 2, 4 }, Boundaries(System(4, 2, 3)));

            // Two INDEPENDENT runs, each losing exactly one interior boundary.
            Assert.Equal(new[] { 0, 1, 3, 4, 6 }, Boundaries(System(6, 1, 2, 4, 5)));
        }

        [Fact]
        public void Authority_IsTheSameRuleForPushBack_WhichComposesTheSameStructure()
        {
            Assert.Equal(
                Boundaries(System(4, 1, 2)),
                DynamicFrontActivation.PresentBoundaries(PushBack(4, 1, 2).Structure).ToArray());
        }

        // ---- Geometry is untouched ---------------------------------------------------------------------------

        [Fact]
        public void SuppressingABoundary_ChangesNoWidthNoTotalLengthAndNoXPosition()
        {
            var active = System(4);
            var blank = System(4, 1, 2);

            var activeLayout = DynamicFrontGeometry.Compute(active, Catalog);
            var blankLayout = DynamicFrontGeometry.Compute(blank, Catalog);

            // The layout still carries EVERY boundary's X, including the suppressed one: nothing shifts.
            Assert.Equal(activeLayout.PostPositions.Count, blankLayout.PostPositions.Count);
            for (var post = 0; post < activeLayout.PostPositions.Count; post++)
            {
                Assert.Equal(activeLayout.PostPositions[post], blankLayout.PostPositions[post], 6);
            }

            Assert.Equal(active.TotalLength, blank.TotalLength, 6);
            for (var index = 0; index < active.Fronts.Count; index++)
            {
                Assert.Equal(active.Fronts[index].StartX, blank.Fronts[index].StartX, 6);
                Assert.Equal(active.Fronts[index].EndX, blank.Fronts[index].EndX, 6);
                Assert.Equal(active.Fronts[index].BeamLength, blank.Fronts[index].BeamLength, 6);
            }
        }

        [Fact]
        public void TheLogicalFrontsSurvive_WithTheirIndicesAndDormantConfiguration()
        {
            var system = System(4, 1, 2);

            Assert.Equal(4, system.Fronts.Count);
            Assert.Equal(new[] { 0, 1, 2, 3 }, system.Fronts.Select(front => front.Index).ToArray());
            Assert.Equal(new[] { true, false, false, true }, system.Fronts.Select(front => front.IsActive).ToArray());
            // The blank fronts keep their own dormant levels and lanes.
            Assert.Equal(3, system.Fronts[1].LoadLevels);
            Assert.Equal(3, system.Fronts[1].Levels.Count);
            Assert.Equal(1, system.Fronts[2].PalletCount);
        }

        // ---- Every view omits the SAME boundary ---------------------------------------------------------------

        [Fact]
        public void TheFrontalCut_DropsThePostAndPlateOfTheSuppressedBoundaryOnly()
        {
            var active = Frontal(System(4));
            var blank = Frontal(System(4, 1, 2));

            Assert.Equal(5, PostCount(active));
            Assert.Equal(4, PostCount(blank));
            Assert.Equal(PlateCount(active) - 1, PlateCount(blank));

            // And it is the RIGHT one: no post stands at boundary 2's X.
            var layout = DynamicFrontGeometry.Compute(System(4, 1, 2), Catalog);
            var suppressed = layout.PostPositions[2];
            Assert.DoesNotContain(
                blank.Where(instance => instance.Role == HeaderBlockRole.Post),
                post => Math.Abs(post.Insertion.X - suppressed) < 1e-6);
            // The surviving boundaries keep their exact X.
            foreach (var post in new[] { 0, 1, 3, 4 })
            {
                var x = layout.PostPositions[post];
                Assert.Contains(
                    blank.Where(instance => instance.Role == HeaderBlockRole.Post),
                    instance => Math.Abs(instance.Insertion.X - x) < 1e-6);
            }
        }

        [Fact]
        public void ThePlanta_DropsTheWholeTransverseLineOfTheSuppressedBoundary()
        {
            var active = Planta(System(4));
            var blank = Planta(System(4, 1, 2));
            var layout = DynamicFrontGeometry.Compute(System(4, 1, 2), Catalog);
            var suppressedY = layout.PostPositions[2];

            Assert.True(PostCount(active) > PostCount(blank));
            // Nothing at all is drawn on the suppressed post line: no post, no plate, no separator, no cabecera.
            Assert.DoesNotContain(
                blank,
                instance => Math.Abs(instance.Insertion.Y - suppressedY) < 1e-6
                            && (instance.Role == HeaderBlockRole.Post
                                || instance.Role == HeaderBlockRole.BasePlate
                                || instance.Role == HeaderBlockRole.Separator));
        }

        [Fact]
        public void TheLateralCortes_SkipTheSuppressedBoundary()
        {
            var cortes = new DynamicSystemLateralBuilder().Cortes(System(4, 1, 2), Catalog);

            // One section per EXISTING boundary, keeping the original post index so nothing is renumbered.
            Assert.Equal(new[] { 0, 1, 3, 4 }, cortes.Select(corte => corte.PostIndex).ToArray());
            Assert.Equal(4, cortes.Count);
            // A rack with no blank fronts keeps all five.
            Assert.Equal(5, new DynamicSystemLateralBuilder().Cortes(System(4), Catalog).Count);
        }

        [Fact]
        public void PushBack_OmitsTheSameBoundaryInEveryView()
        {
            var system = PushBack(4, 1, 2);
            var layout = DynamicFrontGeometry.Compute(system.Structure, Catalog);
            var suppressed = layout.PostPositions[2];

            var frontal = new PushBackSystemFrontalBuilder()
                .BuildPlan(system, Catalog, PushBackFrontalEnd.EntradaSalida).Flatten().Instances;
            Assert.DoesNotContain(
                frontal.Where(instance => instance.Role == HeaderBlockRole.Post),
                post => Math.Abs(post.Insertion.X - suppressed) < 1e-6);

            var cortes = new PushBackSystemLateralBuilder().Cortes(system, Catalog);
            Assert.Equal(new[] { 0, 1, 3, 4 }, cortes.Select(corte => corte.PostIndex).ToArray());

            var planta = new PushBackSystemPlantaBuilder().BuildPlan(system, Catalog).Flatten().Instances;
            Assert.DoesNotContain(
                planta.Where(instance => instance.Role == HeaderBlockRole.Post),
                post => Math.Abs(post.Insertion.Y - suppressed) < 1e-6);
        }

        // ---- BOM ----------------------------------------------------------------------------------------------

        [Fact]
        public void TheBom_DropsExactlyThePiecesOfTheSuppressedBoundary()
        {
            var active = SystemBomBuilder.Build(System(4), Catalog);
            var blank = SystemBomBuilder.Build(System(4, 1, 2), Catalog);

            // 5 fronteras ⇒ 4: se van EXACTAMENTE las piezas de una. La cabecera se cotiza una vez por frontera y
            // por módulo de cabecera (2 aquí), y el poste derivado reforzado una vez por frontera.
            Assert.Equal(10, Quantity(active, BomBuilder.Cabecera));
            Assert.Equal(8, Quantity(blank, BomBuilder.Cabecera));
            Assert.Equal(5, Quantity(active, SystemBomBuilder.ReinforcedPost));
            Assert.Equal(4, Quantity(blank, SystemBomBuilder.ReinforcedPost));
            Assert.Equal(40, Quantity(active, SystemBomBuilder.Separator));
            Assert.Equal(30, Quantity(blank, SystemBomBuilder.Separator));

            // Y en el total de PIEZAS bajan los postes y las placas de esa frontera.
            Assert.True(Pieces(active, BomBuilder.Post) > Pieces(blank, BomBuilder.Post));
            Assert.True(Pieces(active, BomBuilder.BasePlate) > Pieces(blank, BomBuilder.BasePlate));

            // Un ÚNICO frente en blanco no suprime frontera: esas cuatro cifras no se mueven.
            var single = SystemBomBuilder.Build(System(4, 1), Catalog);
            Assert.Equal(Quantity(active, BomBuilder.Cabecera), Quantity(single, BomBuilder.Cabecera));
            Assert.Equal(Quantity(active, SystemBomBuilder.ReinforcedPost), Quantity(single, SystemBomBuilder.ReinforcedPost));
            Assert.Equal(Quantity(active, SystemBomBuilder.Separator), Quantity(single, SystemBomBuilder.Separator));
            Assert.Equal(Pieces(active, BomBuilder.Post), Pieces(single, BomBuilder.Post));
            Assert.Equal(Pieces(active, BomBuilder.BasePlate), Pieces(single, BomBuilder.BasePlate));
        }

        [Fact]
        public void ThePushBackBom_DropsTheSameStructuralPieces()
        {
            var active = PushBackBomBuilder.Build(PushBack(4), Catalog);
            var blank = PushBackBomBuilder.Build(PushBack(4, 1, 2), Catalog);

            // Push Back reutiliza el BOM estructural del Dinámico, así que baja lo mismo y por la misma autoridad.
            Assert.Equal(
                Quantity(SystemBomBuilder.Build(System(4), Catalog), BomBuilder.Cabecera),
                Quantity(active, BomBuilder.Cabecera));
            Assert.True(Quantity(active, BomBuilder.Cabecera) > Quantity(blank, BomBuilder.Cabecera));
            Assert.True(Quantity(active, SystemBomBuilder.ReinforcedPost) > Quantity(blank, SystemBomBuilder.ReinforcedPost));
            Assert.True(Quantity(active, SystemBomBuilder.Separator) > Quantity(blank, SystemBomBuilder.Separator));
            Assert.True(Pieces(active, BomBuilder.Post) > Pieces(blank, BomBuilder.Post));
            Assert.True(Pieces(active, BomBuilder.BasePlate) > Pieces(blank, BomBuilder.BasePlate));

            var single = PushBackBomBuilder.Build(PushBack(4, 1), Catalog);
            Assert.Equal(Quantity(active, BomBuilder.Cabecera), Quantity(single, BomBuilder.Cabecera));
            Assert.Equal(Pieces(active, BomBuilder.Post), Pieces(single, BomBuilder.Post));
        }

        // ---- Reactivating restores everything ------------------------------------------------------------------

        [Fact]
        public void ReactivatingOneOfTheTwoBlankFronts_RestoresTheSharedBoundaryAndItsPieces()
        {
            var design = Design(4, 1, 2);
            Assert.Equal(new[] { 0, 1, 3, 4 }, Boundaries(Resolve(design)));

            design.Fronts[2].IsActive = true;

            var restored = Resolve(design);
            Assert.Equal(new[] { 0, 1, 2, 3, 4 }, Boundaries(restored));

            // Drawing and BOM come back to the state a rack with only front 1 blank has.
            var reference = System(4, 1);
            Assert.Equal(PostCount(Frontal(reference)), PostCount(Frontal(restored)));
            Assert.Equal(PlateCount(Frontal(reference)), PlateCount(Frontal(restored)));
            Assert.Equal(
                Quantity(SystemBomBuilder.Build(reference, Catalog), BomBuilder.Cabecera),
                Quantity(SystemBomBuilder.Build(restored, Catalog), BomBuilder.Cabecera));
            Assert.Equal(
                Pieces(SystemBomBuilder.Build(reference, Catalog), BomBuilder.Post),
                Pieces(SystemBomBuilder.Build(restored, Catalog), BomBuilder.Post));
            Assert.Equal(
                new DynamicSystemLateralBuilder().Cortes(reference, Catalog).Count,
                new DynamicSystemLateralBuilder().Cortes(restored, Catalog).Count);
        }

        // ---- Safety bolted to the missing post -----------------------------------------------------------------

        [Fact]
        public void TheSafetyOfTheMissingPost_IsNotDrawnButItsStoredCellsAreNeitherMovedNorErased()
        {
            var design = Design(4, 1, 2);
            var desviador = new SelectiveSafetySelection
            {
                ElementId = TestCatalogIds.Safety.Deviators.L4,
                Side = SafetySide.Both
            };
            desviador.DesviadorOffCells.Add(new SelectiveGridCell { Frente = 2, Level = 0 });   // the suppressed post
            desviador.DesviadorOffCells.Add(new SelectiveGridCell { Frente = 3, Level = 1 });   // a surviving one
            design.SafetySelections.Add(desviador);
            var system = Resolve(design);

            // The selection is carried through untouched: the cell still names post 2, not some other post.
            var stored = system.SafetySelections.Single().DesviadorOffCells
                .Select(cell => (cell.Frente, cell.Level))
                .OrderBy(pair => pair.Frente).ToArray();
            Assert.Equal(new[] { (2, 0), (3, 1) }, stored);

            // The dialog sees the suppressed post as a column with NO levels, so it is not selectable there...
            var perPost = DynamicFrontActivation.EffectiveLevelsPerPost(
                DynamicFrontActivation.EffectiveLevelsPerFront(system));
            Assert.Equal(0, perPost[2]);
            Assert.False(DynamicFrontActivation.BoundaryExists(system, 2));
            // ...and the dormancy rule keeps its stored cell across a dialog round trip.
            var merged = SafetyDormantCells.Merge(
                new[] { new SelectiveGridCell { Frente = 3, Level = 1 } },
                system.SafetySelections.Single().DesviadorOffCells,
                perPost);
            Assert.Contains(merged, cell => cell.Frente == 2 && cell.Level == 0);

            // Reactivating one of the two fronts brings the post back, and with it its cell, at the SAME index.
            design.Fronts[2].IsActive = true;
            var restored = Resolve(design);
            Assert.True(DynamicFrontActivation.BoundaryExists(restored, 2));
            Assert.True(DynamicFrontActivation.EffectiveLevelsPerPost(
                DynamicFrontActivation.EffectiveLevelsPerFront(restored))[2] > 0);
            Assert.Contains(
                restored.SafetySelections.Single().DesviadorOffCells,
                cell => cell.Frente == 2 && cell.Level == 0);
        }

        // ---- Round trip ---------------------------------------------------------------------------------------

        [Fact]
        public void TheSuppressedBoundarySurvivesAPersistenceRoundTrip()
        {
            var design = Design(4, 1, 2);
            var reloaded = DynamicRackSystemDocument.From(design).ToDesign();

            Assert.Equal(new[] { true, false, false, true }, reloaded.Fronts.Select(front => front.IsActive).ToArray());
            Assert.Equal(new[] { 0, 1, 3, 4 }, Boundaries(Resolve(reloaded)));

            // And through the resolved-system document, which RACKEDITAR reopens.
            var system = DynamicRackSystemDocument.From(Resolve(design)).ToDomain();
            Assert.Equal(new[] { 0, 1, 3, 4 }, Boundaries(system));
        }

        // ---- Racks with no blank fronts are untouched ----------------------------------------------------------

        [Fact]
        public void ARackWithoutBlankFronts_DrawsAndCostsExactlyAsBefore()
        {
            var system = System(4);

            Assert.Equal(5, Boundaries(system).Length);
            Assert.Equal(5, PostCount(Frontal(system)));
            Assert.Equal(5, new DynamicSystemLateralBuilder().Cortes(system, Catalog).Count);
            Assert.All(
                Enumerable.Range(0, 5),
                post => Assert.True(DynamicFrontActivation.BoundaryExists(system, post)));
        }
    }
}
