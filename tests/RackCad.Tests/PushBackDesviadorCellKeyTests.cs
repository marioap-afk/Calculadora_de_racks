using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Bom;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Persistence;
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
    /// PB-002, segunda mitad (I-32) — la celda del desviador significa <b>POSTE × NIVEL</b> en Push Back.
    ///
    /// La primera corrida hizo alcanzables las celdas que el dibujo ya colocaba (la rejilla ofrece el máximo de
    /// frentes adyacentes por poste), pero dejó el interruptor a medias: el corte lateral leía la off-cell por
    /// POSTE mientras el frontal, la planta y el BOM la leían por FRENTE, con un <c>Math.Min</c> al último frente.
    /// Consecuencia real: apagar el último poste no quitaba nada del BOM, y apagar el penúltimo apagaba DOS
    /// desviadores frontales.
    ///
    /// Escenario: dos frentes jagged [3, 1] ⇒ tres postes [3, 3, 1]. Se desactivan dos celdas que el
    /// <c>Math.Min</c> confunde: una del poste FINAL y una celda ALTA del poste INTERIOR.
    /// </summary>
    public class PushBackDesviadorCellKeyTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static string DesviadorId(RackCatalog catalog)
            => catalog.SafetyElements
                .First(entry => SelectiveSafetyDefaults.IsType(entry.Type, SelectiveSafetyDefaults.DesviadorType)).Id;

        /// <summary>Dos frentes con 3 y 1 niveles ⇒ postes [3, 3, 1] por la regla de frentes adyacentes.</summary>
        private static PushBackDesign JaggedDesign(RackCatalog catalog, params SelectiveGridCell[] offCells)
        {
            var design = new PushBackDesign
            {
                Structure = new DynamicRackDesign
                {
                    Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                    PalletsDeep = 4,
                    LoadLevels = 3,
                    FirstLevelHeight = 6.0,
                    BeamDepth = 4.0
                }
            };
            design.Structure.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 3, PalletsDeep = 4, DepthStartPosition = 1 });
            design.Structure.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 1, PalletsDeep = 4, DepthStartPosition = 1 });
            design.Fronts.Add(new PushBackFrontConfig());
            design.Fronts.Add(new PushBackFrontConfig());

            var selection = new SelectiveSafetySelection
            {
                ElementId = DesviadorId(catalog),
                Quantity = 1,
                Side = SafetySide.Left,
                DesviadorLongitud = SelectiveSafetyDefaults.DesviadorLongitud,
                DesviadorPrimerNivelAltura = SelectiveSafetyDefaults.DesviadorPrimerNivelAltura
            };
            foreach (var cell in offCells)
            {
                selection.DesviadorOffCells.Add(new SelectiveGridCell { Frente = cell.Frente, Level = cell.Level });
            }

            design.Structure.SafetySelections.Add(selection);
            return design;
        }

        private static PushBackSystem Resolve(RackCatalog catalog, params SelectiveGridCell[] offCells)
            => new PushBackResolver(catalog).Resolve(JaggedDesign(catalog, offCells));

        /// <summary>The post index each drawn desviador belongs to, recovered from its own coordinate.</summary>
        private static List<int> PostsOf(
            IEnumerable<HeaderBlockInstance> instances, string desviadorId, IReadOnlyList<double> postPositions, bool byY)
        {
            var result = new List<int>();
            foreach (var instance in instances)
            {
                if (!string.Equals(instance.PieceId, desviadorId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var coordinate = byY ? instance.Insertion.Y : instance.Insertion.X;
                var best = 0;
                for (var post = 1; post < postPositions.Count; post++)
                {
                    if (Math.Abs(postPositions[post] - coordinate) < Math.Abs(postPositions[best] - coordinate))
                    {
                        best = post;
                    }
                }

                result.Add(best);
            }

            return result;
        }

        private static int DesviadorCount(BillOfMaterials bom, string desviadorId)
            => bom.Components
                .Where(component => string.Equals(component.ProfileId, desviadorId, StringComparison.OrdinalIgnoreCase))
                .Sum(component => component.Quantity);

        // ---- The grid the dialog offers is the one the drawing uses ----

        [Fact]
        public void TheJaggedScenario_HasThreePostsOfThreeThreeAndOne()
        {
            var perPost = DynamicFrontGeometry.LoadLevelsPerPost(new[] { 3, 1 });
            Assert.Equal(new[] { 3, 3, 1 }, perPost.ToArray());
        }

        // ---- The two cells the Math.Min used to confuse ----

        /// <summary>
        /// Desactivar la ÚNICA celda del poste FINAL (2, 0) tiene que quitar exactamente un desviador en cada
        /// elevación y bajar el BOM. Con el <c>Math.Min</c> ese poste leía la fila del frente 1, así que apagaba la
        /// celda equivocada — o ninguna.
        /// </summary>
        [Fact]
        public void DisablingTheFinalPost_RemovesItFromEveryViewAndFromTheBom()
        {
            var catalog = Catalog;
            var desviadorId = DesviadorId(catalog);
            var full = Resolve(catalog);
            var off = Resolve(catalog, new SelectiveGridCell { Frente = 2, Level = 0 });

            var layout = DynamicFrontGeometry.Compute(full.Structure, catalog);
            var posts = layout.PostPositions;
            Assert.Equal(3, posts.Count);

            var frontal = new PushBackSystemFrontalBuilder();
            var planta = new PushBackSystemPlantaBuilder();

            var fullFrontal = PostsOf(frontal.BuildPlan(full, catalog, PushBackFrontalEnd.EntradaSalida).Flatten().Instances, desviadorId, posts, byY: false);
            var offFrontal = PostsOf(frontal.BuildPlan(off, catalog, PushBackFrontalEnd.EntradaSalida).Flatten().Instances, desviadorId, posts, byY: false);
            var fullPlanta = PostsOf(planta.BuildPlan(full, catalog).Flatten().Instances, desviadorId, posts, byY: true);
            var offPlanta = PostsOf(planta.BuildPlan(off, catalog).Flatten().Instances, desviadorId, posts, byY: true);

            // FRONTAL: the last post loses its only level; every other post keeps exactly what it had.
            Assert.Contains(2, fullFrontal);
            Assert.DoesNotContain(2, offFrontal);
            Assert.Equal(fullFrontal.Count(p => p != 2), offFrontal.Count);

            // PLANTA: that post had a single level, so its whole plan reference goes away.
            Assert.Contains(2, fullPlanta);
            Assert.DoesNotContain(2, offPlanta);

            // BOM: it is counted from the frontal projections, so the switch has to reach it.
            Assert.True(
                DesviadorCount(PushBackBomBuilder.Build(off, catalog), desviadorId)
                < DesviadorCount(PushBackBomBuilder.Build(full, catalog), desviadorId),
                "apagar el último poste no bajó el BOM");
        }

        /// <summary>
        /// Desactivar una celda del poste INTERIOR sólo puede afectar a ese poste.
        ///
        /// El <c>Math.Min</c> desvía exactamente UN poste — el último, que no tiene frente propio — y lo hace leer la
        /// fila del penúltimo. Por eso la fuga se mide así: apagar (1, 0) tenía que llevarse TAMBIÉN el desviador del
        /// poste 2, que comparte esa clave. Se comprueba además la celda ALTA (1, 2), alcanzable desde la primera
        /// mitad de PB-002, para que ambas mitades queden fijadas juntas.
        /// </summary>
        [Fact]
        public void DisablingACellOfTheInteriorPost_TouchesOnlyThatPost()
        {
            var catalog = Catalog;
            var desviadorId = DesviadorId(catalog);
            var full = Resolve(catalog);
            var frontal = new PushBackSystemFrontalBuilder();
            var posts = DynamicFrontGeometry.Compute(full.Structure, catalog).PostPositions;

            List<int> Frontal(PushBackSystem system)
                => PostsOf(frontal.BuildPlan(system, catalog, PushBackFrontalEnd.EntradaSalida).Flatten().Instances, desviadorId, posts, byY: false);

            var fullFrontal = Frontal(full);

            // (a) The BLEED: post 1 level 0 shares the collapsed key with post 2, whose only level is 0.
            var offLow = Frontal(Resolve(catalog, new SelectiveGridCell { Frente = 1, Level = 0 }));
            Assert.Equal(fullFrontal.Count - 1, offLow.Count);
            Assert.Equal(fullFrontal.Count(p => p == 1) - 1, offLow.Count(p => p == 1));
            Assert.Equal(fullFrontal.Count(p => p == 2), offLow.Count(p => p == 2));   // the final post keeps its piece
            Assert.Equal(fullFrontal.Count(p => p == 0), offLow.Count(p => p == 0));

            // (b) The HIGH cell, reachable since the first half of PB-002.
            var offHigh = Frontal(Resolve(catalog, new SelectiveGridCell { Frente = 1, Level = 2 }));
            Assert.Equal(fullFrontal.Count - 1, offHigh.Count);
            Assert.Equal(fullFrontal.Count(p => p == 1) - 1, offHigh.Count(p => p == 1));
            Assert.Equal(fullFrontal.Count(p => p == 0), offHigh.Count(p => p == 0));
            Assert.Equal(fullFrontal.Count(p => p == 2), offHigh.Count(p => p == 2));
        }

        /// <summary>
        /// La misma ausencia, vista por vista: lo que el corte lateral de un poste no dibuja, tampoco lo dibujan el
        /// frontal ni la planta. Es la coherencia que el interruptor prometía y no cumplía.
        /// </summary>
        [Fact]
        public void TheSameAbsence_ShowsInLateralFrontalAndPlanta()
        {
            var catalog = Catalog;
            var desviadorId = DesviadorId(catalog);
            var off = Resolve(
                catalog,
                new SelectiveGridCell { Frente = 2, Level = 0 },
                new SelectiveGridCell { Frente = 1, Level = 2 });

            var posts = DynamicFrontGeometry.Compute(off.Structure, catalog).PostPositions;
            var lateral = new PushBackSystemLateralBuilder();

            // LATERAL, per post section: post 2 has no desviador at all; post 1 lost its top level.
            var perPostLateral = Enumerable.Range(0, posts.Count)
                .Select(post => lateral.Build(off, catalog, post).Flatten().Instances
                    .Count(i => string.Equals(i.PieceId, desviadorId, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            var frontal = PostsOf(
                new PushBackSystemFrontalBuilder().BuildPlan(off, catalog, PushBackFrontalEnd.EntradaSalida).Flatten().Instances,
                desviadorId, posts, byY: false);

            Assert.Equal(0, perPostLateral[2]);
            Assert.DoesNotContain(2, frontal);

            // Post 1 keeps pieces in both views, and the SAME number of levels in each.
            Assert.True(perPostLateral[1] > 0);
            Assert.Equal(perPostLateral[1], frontal.Count(p => p == 1));
            Assert.Equal(perPostLateral[0], frontal.Count(p => p == 0));
        }

        // ---- Round-trip: the indices stay per POST ----

        [Fact]
        public void RoundTrip_KeepsTheExactPerPostIndices()
        {
            var catalog = Catalog;
            var design = JaggedDesign(
                catalog,
                new SelectiveGridCell { Frente = 2, Level = 0 },
                new SelectiveGridCell { Frente = 1, Level = 2 });

            var restored = PushBackDesignDocument.FromDomain(design).ToDomain();
            var selection = restored.Structure.SafetySelections.Single();
            var cells = selection.DesviadorOffCells
                .Select(cell => (cell.Frente, cell.Level))
                .OrderBy(cell => cell.Frente).ThenBy(cell => cell.Level)
                .ToList();

            Assert.Equal(new[] { (1, 2), (2, 0) }, cells);

            // And the resolved system keeps them too (the resolver deep-copies through the authority).
            var system = new PushBackResolver(catalog).Resolve(restored);
            var resolved = system.Structure.SafetySelections
                .Single(s => string.Equals(s.ElementId, DesviadorId(catalog), StringComparison.OrdinalIgnoreCase))
                .DesviadorOffCells
                .Select(cell => (cell.Frente, cell.Level))
                .OrderBy(cell => cell.Frente).ThenBy(cell => cell.Level)
                .ToList();
            Assert.Equal(new[] { (1, 2), (2, 0) }, resolved);
        }

        // ---- Isolation: the dynamic system keeps its own (front-indexed) reading ----

        [Fact]
        public void DynamicSelection_KeepsTheFrontIndexedReading()
        {
            // The capability is OPT-IN and derived: a selection that nobody marked reads exactly as before.
            var ordinary = new SelectiveSafetySelection { ElementId = "X" };
            Assert.False(ordinary.DesviadorCellsAreByPost);

            // 2 fronts => post 2 collapses onto front 1, which is the historical dynamic behaviour.
            Assert.Equal(1, SelectiveDesviadorPlan.CellKey(ordinary, postIndex: 2, frontCount: 2));
            Assert.Equal(0, SelectiveDesviadorPlan.CellKey(ordinary, postIndex: 0, frontCount: 2));

            var pushBack = new SelectiveSafetySelection { ElementId = "X", DesviadorCellsAreByPost = true };
            Assert.Equal(2, SelectiveDesviadorPlan.CellKey(pushBack, postIndex: 2, frontCount: 2));
        }

        [Fact]
        public void Authority_ReimposesThePerPostReading_AndDeepCopyCarriesIt()
        {
            var catalog = Catalog;
            var fromDisk = new SelectiveSafetySelection { ElementId = DesviadorId(catalog) };
            Assert.False(fromDisk.DesviadorCellsAreByPost);

            var authorized = new PushBackSafetyAuthority(catalog).Authorize(new[] { fromDisk });
            Assert.All(authorized, selection => Assert.True(selection.DesviadorCellsAreByPost));
            Assert.False(fromDisk.DesviadorCellsAreByPost);   // the source is never mutated

            Assert.True(new SelectiveSafetySelection { ElementId = "X", DesviadorCellsAreByPost = true }
                .DeepCopy().DesviadorCellsAreByPost);
        }
    }
}
