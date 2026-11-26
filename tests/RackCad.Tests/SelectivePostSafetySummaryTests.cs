using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Bom;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.Selective;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// «Seguridad por poste» of a Selectivo in the RackCad window (I-64 A-14, line D, entrega 2): the table is counted with the projections
    /// of the BOM (<see cref="SelectivePostSafetySummary"/>), so every column adds up to its line of the BOM, and it says where the drawing
    /// puts each piece. Generic sample racks, no data of a client.
    /// </summary>
    public sealed class SelectivePostSafetySummaryTests
    {
        private const string PostId = TestCatalogIds.Profiles.Posts.Standard;
        private const string BeamId = TestCatalogIds.Profiles.Beams.SelectiveThreeRivet;
        private const string BotaId = TestCatalogIds.Safety.Boots.H3_16_18;
        private const string LateralId = TestCatalogIds.Safety.SideProtectors.H3_16_18;
        private const string DesviadorId = TestCatalogIds.Safety.Deviators.A3;

        private static readonly RackCatalog Catalog = JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static SelectiveBayDesign Bay(int levels, bool floorBeam, double frente = 42.0, double alto = 50.0)
        {
            var bay = new SelectiveBayDesign { FloorBeam = floorBeam, FloorBeamRiseOverride = 4.0 };
            for (var level = 0; level < levels; level++)
            {
                bay.Levels.Add(new SelectiveCell
                {
                    Pallet = new Tarima { Frente = frente, Alto = alto },
                    PalletCount = 2,
                    BeamId = BeamId,
                    BeamPeralte = 4.0,
                });
            }

            return bay;
        }

        private static SelectivePalletDesign Design(params SelectiveBayDesign[] bays)
        {
            var design = new SelectivePalletDesign
            {
                PostId = PostId,
                PostPeralte = 3.0,
                PalletTolerance = 4.0,
                VerticalClearance = 6.0,
                FloorBeamRise = 4.0,
                PalletDepth = 48.0,
                DepthCount = 1,
                DrawBasePlate = true,
            };
            foreach (var bay in bays) design.Bays.Add(bay);
            return design;
        }

        private static SelectiveSafetySelection Bota(SafetySide side = SafetySide.Both)
            => new SelectiveSafetySelection { ElementId = BotaId, Quantity = 1, Side = side };

        private static SelectiveSafetySelection Lateral(params (int Post, SafetySide Side)[] posts)
        {
            var lateral = new SelectiveSafetySelection { ElementId = LateralId, Quantity = 1, Side = SafetySide.None };
            foreach (var (post, side) in posts) lateral.PostSides.Add(new SafetyPostSide { PostIndex = post, Side = side });
            return lateral;
        }

        private static SelectiveSafetySelection Desviador(SafetySide side)
            => new SelectiveSafetySelection { ElementId = DesviadorId, Quantity = 1, Side = side, DesviadorLongitud = 18.0, DesviadorPrimerNivelAltura = 18.0 };

        private static SelectiveRackSystem Resolve(SelectivePalletDesign design, params SelectiveSafetySelection[] safety)
        {
            foreach (var selection in safety) design.SafetySelections.Add(selection);
            return new SelectiveGeometryResolver().Resolve(design, Catalog);
        }

        private static int Bom(SelectiveRackSystem system, Func<BomComponent, bool> line)
            => SelectiveBomBuilder.Build(system, Catalog).Components.Where(line).Sum(c => c.Quantity);

        private static int BomOf(SelectiveRackSystem system, string id)
            => Bom(system, c => string.Equals(c.ProfileId, id, StringComparison.OrdinalIgnoreCase));

        private static void AddsUpToTheBom(SelectiveRackSystem system, IReadOnlyList<SelectivePostSafetyRow> rows)
        {
            Assert.Equal(BomOf(system, BotaId), rows.Sum(r => r.Boots));
            Assert.Equal(BomOf(system, LateralId), rows.Sum(r => r.SideGuards));
            Assert.Equal(BomOf(system, DesviadorId), rows.Sum(r => r.Deflectors));
            Assert.Equal(Bom(system, c => c.Category == SelectiveBomBuilder.Separador && c.ProfileId == DynamicRackDefaults.SeparatorCatalogId),
                rows.Sum(r => r.Separators));
        }

        public static IEnumerable<object[]> Racks()
        {
            yield return new object[] { "un fondo", 1, false, false };
            yield return new object[] { "dos fondos", 2, false, false };
            yield return new object[] { "dos fondos en esquina", 2, true, false };
            yield return new object[] { "medio frente", 1, false, true };
            yield return new object[] { "dos fondos con medio frente", 2, false, true };
        }

        [Theory]
        [MemberData(nameof(Racks))]
        public void Every_column_adds_up_to_its_line_of_the_BOM(string rack, int fondos, bool corner, bool medioFrente)
        {
            var design = Design(Bay(3, floorBeam: true), Bay(2, floorBeam: false, frente: 44.0), Bay(3, floorBeam: false));
            if (medioFrente)
            {
                design.Bays[0].Segments.Add(new SelectiveSegment { Length = 30.0, Loaded = true });
                design.Bays[0].Segments.Add(new SelectiveSegment { Length = 0.0, Loaded = true });
            }

            if (fondos > 1)
            {
                design.DepthCount = 2;
                design.SeparatorLengths.Add(10.0);
                design.ExtraFondoDepths.Add(40.0);
                design.ExtraFondoBays.Add(corner
                    ? new List<SelectiveBayDesign> { Bay(3, floorBeam: true), Bay(4, floorBeam: false) }
                    : new List<SelectiveBayDesign> { Bay(3, floorBeam: true), Bay(2, floorBeam: false, frente: 44.0), Bay(3, floorBeam: false) });
            }

            var desviador = Desviador(SafetySide.Both);
            desviador.DesviadorOffCells.Add(new SelectiveGridCell { Frente = 1, Level = 1 });
            var system = Resolve(design, Bota(), Lateral((0, SafetySide.Left), (3, SafetySide.Both)), desviador);

            var rows = SelectivePostSafetySummary.Build(system, Catalog);

            Assert.True(rows.Sum(r => r.Boots) > 0, rack);
            Assert.True(rows.Sum(r => r.SideGuards) > 0, rack);
            Assert.True(rows.Sum(r => r.Deflectors) > 0, rack);
            Assert.Equal(fondos > 1, rows.Sum(r => r.Separators) > 0);
            AddsUpToTheBom(system, rows);
            Assert.Equal(4, rows.Count(r => !r.IsIntermediate)); // P1..P4 of the master run
            Assert.Equal(medioFrente, rows.Any(r => r.IsIntermediate));
        }

        [Fact]
        public void A_protector_lateral_replaces_the_botas_of_its_post_as_in_the_drawing()
        {
            var system = Resolve(Design(Bay(3, true), Bay(3, true), Bay(3, true)), Bota(), Lateral((0, SafetySide.Left), (3, SafetySide.Right)));

            var rows = SelectivePostSafetySummary.Build(system, Catalog);

            Assert.Equal(new[] { 0, 2, 2, 0 }, rows.Select(r => r.Boots));
            Assert.Equal(new[] { 1, 0, 0, 1 }, rows.Select(r => r.SideGuards));
            Assert.False(rows[0].BootNear || rows[0].BootFar); // a post without botas has no side
            Assert.True(rows[1].BootNear && rows[1].BootFar);
            Assert.True(rows[0].IsEndPost && rows[3].IsEndPost && !rows[1].IsEndPost);
            AddsUpToTheBom(system, rows);
        }

        [Fact]
        public void The_side_of_a_bota_is_the_face_it_protects_on_the_depth_axis_of_the_rack()
        {
            var bota = Bota();
            bota.PostSides.Add(new SafetyPostSide { PostIndex = 1, Side = SafetySide.Left });
            bota.PostSides.Add(new SafetyPostSide { PostIndex = 2, Side = SafetySide.Right });
            var system = Resolve(Design(Bay(3, true), Bay(3, true)), bota);

            var rows = SelectivePostSafetySummary.Build(system, Catalog);

            Assert.Equal(new[] { 2, 1, 1 }, rows.Select(r => r.Boots));
            Assert.True(rows[0].BootNear && rows[0].BootFar);
            Assert.True(rows[1].BootNear && !rows[1].BootFar); // «Izquierda»
            Assert.True(!rows[2].BootNear && rows[2].BootFar); // «Derecha»

            // «Izquierda» is the near face: the low end of the rack's own depth axis (the front post of fondo 1); «Derecha» the far one.
            var postXs = SelectiveDepthLayout.MasterGrid(system, Catalog).PostXs;
            var botas = new SelectivePlantaBuilder().Build(system, Catalog).Where(i => i.Role == HeaderBlockRole.Safety && i.PieceId == BotaId).ToList();
            HeaderBlockInstance At(int post) => Assert.Single(botas, b => Math.Abs(b.Insertion.Y - postXs[post]) < 12.0);
            var both = botas.Where(b => Math.Abs(b.Insertion.Y - postXs[0]) < 12.0).OrderBy(b => b.Insertion.X).ToList();
            Assert.Equal(2, both.Count);
            Assert.Equal(both[0].Insertion.X, At(1).Insertion.X, 6);
            Assert.Equal(both[1].Insertion.X, At(2).Insertion.X, 6);
            Assert.True(both[0].Insertion.X < both[1].Insertion.X);
            AddsUpToTheBom(system, rows);
        }

        [Fact]
        public void Desviadores_on_both_faces_list_each_level_once_and_count_both_pieces()
        {
            var both = Resolve(Design(Bay(3, floorBeam: true)), Desviador(SafetySide.Both));
            var left = Resolve(Design(Bay(3, floorBeam: true)), Desviador(SafetySide.Left));

            var rows = SelectivePostSafetySummary.Build(both, Catalog);
            Assert.All(rows, r => Assert.Equal(new[] { 0, 1, 2 }, r.DeflectorLevels));
            Assert.All(rows, r => Assert.Equal(6, r.Deflectors));
            Assert.All(SelectivePostSafetySummary.Build(left, Catalog), r => Assert.Equal(3, r.Deflectors));
            AddsUpToTheBom(both, rows);
        }

        [Fact]
        public void The_levels_of_the_desviador_are_its_load_rows_N1_being_the_floor_row()
        {
            // Without «Larguero a piso» the first load row still has its desviador (above the first troquel): N1, N2, N3 for three levels.
            var desviador = Desviador(SafetySide.Left);
            desviador.DesviadorOffCells.Add(new SelectiveGridCell { Frente = 0, Level = 1 });
            var system = Resolve(Design(Bay(3, floorBeam: false), Bay(3, floorBeam: false)), desviador);

            var rows = SelectivePostSafetySummary.Build(system, Catalog);

            Assert.Equal(new[] { 0, 2 }, rows[0].DeflectorLevels); // its N2 is off
            Assert.Equal(new[] { 0, 1, 2 }, rows[1].DeflectorLevels);
            AddsUpToTheBom(system, rows);
        }

        [Fact]
        public void An_intermediate_post_of_a_medio_frente_has_its_own_row_with_its_desviadores()
        {
            var design = Design(Bay(3, floorBeam: false), Bay(3, floorBeam: false));
            design.Bays[1].Segments.Add(new SelectiveSegment { Length = 30.0, Loaded = true });
            design.Bays[1].Segments.Add(new SelectiveSegment { Length = 0.0, Loaded = true });
            var system = Resolve(design, Desviador(SafetySide.Left), Bota());

            var rows = SelectivePostSafetySummary.Build(system, Catalog);

            Assert.Equal(4, rows.Count);
            var intermediate = Assert.Single(rows, r => r.IsIntermediate);
            Assert.Same(intermediate, rows[2]); // between P2 and P3, in the order of the run
            Assert.Equal(1, intermediate.IntermediateOfFront);
            Assert.Equal(1, intermediate.IntermediateOrdinal);
            Assert.Equal(1, intermediate.PostIndex);
            Assert.Equal(3, intermediate.Deflectors);
            Assert.Equal(0, intermediate.Boots); // the planta puts botas on the frame posts only
            Assert.Equal(new[] { 3, 3, 3 }, rows.Where(r => !r.IsIntermediate).Select(r => r.Deflectors));
            AddsUpToTheBom(system, rows);
        }

        [Fact]
        public void The_separadores_are_those_of_the_corte_of_each_post_and_a_post_one_fondo_does_not_reach_has_none()
        {
            var design = Design(Bay(3, true), Bay(3, true));
            design.DepthCount = 2;
            design.SeparatorLengths.Add(10.0);
            design.ExtraFondoBays.Add(new List<SelectiveBayDesign> { Bay(3, true) }); // the second fondo stops at P2
            var system = Resolve(design);

            var rows = SelectivePostSafetySummary.Build(system, Catalog);
            var cortes = new SelectiveLateralBuilder().Cortes(system, Catalog);

            Assert.Equal(3, rows.Count);
            for (var post = 0; post < rows.Count; post++)
            {
                var corte = cortes.Single(c => c.PostIndex == post);
                Assert.Equal(corte.Largueros.Count(i => i.Role == HeaderBlockRole.Separator), rows[post].Separators);
            }

            Assert.True(rows[0].Separators > 0 && rows[1].Separators > 0);
            Assert.Equal(0, rows[2].Separators);
            AddsUpToTheBom(system, rows);
        }

        [Fact]
        public void Without_a_system_or_a_catalog_there_is_no_table()
        {
            Assert.Empty(SelectivePostSafetySummary.Build(null, Catalog));
            Assert.Empty(SelectivePostSafetySummary.Build(new SelectiveRackSystem(), Catalog));
            Assert.Empty(SelectivePostSafetySummary.Build(Resolve(Design(Bay(2, true))), null));
        }
    }
}
