#nullable enable
using System.Linq;
using RackCad.Application.VentanaRackCad;
using RackCad.Domain.Systems.Selective;
using RackCad.UI.VentanaRackCad;
using Xunit;
using static RackCad.UI.Tests.SelectiveWindowFixtures;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// T6 bom (PP-S10): a «medio frente» that fits can stop fitting by a write that is not of its tramos, and the resolver then draws the
    /// whole front with no notice. The fifth batch refused it only for the peralte of the rack («Aplicar a todos» and «Lo esencial»); now
    /// every write of the editor of the window refuses it, with the front and the reason in the field, and nothing of it stays. The rack:
    /// <see cref="SelectiveWindowFixtures.Structure"/> with F2 split in two tramos and 0.6 in left for the last one.
    /// </summary>
    public sealed class SelectiveSplitGuardWindowTests
    {
        private const string Refusal = "No se aplicó el cambio: el medio frente de F2 dejaría de caber. No cabe: ";
        private const string Tail = " Ajusta antes sus tramos en «Matriz» (inspector del frente).";

        private static SelectivePalletDesign SplitF2(double rest)
        {
            var probe = SelectiveEditor();
            var fit = SelectiveSegmentFit.Of(probe.Draft.System!, probe.Draft.Catalog, new[] { 0 }, 1, new[] { 10.0 }, 2);
            var design = Structure();
            design.Bays[1].Segments.Add(new SelectiveSegment { Length = fit.Width - fit.PostOverhead - rest, Loaded = true });
            design.Bays[1].Segments.Add(new SelectiveSegment { Length = 0.0, Loaded = true });
            return design;
        }

        private static SelectiveSegmentFitResult FitOfF2(SelectiveEditorViewModel vm)
        {
            var segments = vm.Draft.Front(0, 1)!.Segments;
            return SelectiveSegmentFit.Of(vm.Draft.System!, vm.Draft.Catalog, new[] { 0 }, 1, segments.Take(1).Select(s => s.Length).ToList(), 2);
        }

        private static void AssertRefusedAndNothingStays(SelectiveEditorViewModel vm, string before, string? error)
        {
            Assert.NotNull(error);
            Assert.StartsWith(Refusal, error);
            Assert.EndsWith(Tail, error);
            Assert.Equal(before, Json(vm.Draft.Design));
            Assert.False(vm.IsDirty);
            Assert.True(vm.Draft.Front(0, 1)!.IsSplit);
            Assert.Equal(0.6, FitOfF2(vm).Remainder, 6); // the system is the one of before
        }

        [Fact]
        public void TheOwnPeralteOfThePostOfASplitFrontThatLeavesItWithNoRoomIsRefusedInItsRow()
        {
            // The troquel of the larguero slides with the peralte of the post: P2 (the left post of F2) from 3 to 4 in takes 1 in more.
            var vm = SelectiveEditor(SplitF2(rest: 0.6));
            var before = Json(vm.Draft.Design);

            var row = vm.PostRows[1].Field;
            Write(row, "4");

            AssertRefusedAndNothingStays(vm, before, row.Error);
            Assert.Null(vm.Draft.PostPeralteOverride(1));

            // A peralte the split still fits is applied: 0.1 in left.
            Write(vm.PostRows[1].Field, "3.5");
            Assert.True(string.IsNullOrEmpty(vm.PostRows[1].Field.Error), vm.PostRows[1].Field.Error);
            Assert.Equal(3.5, vm.Draft.PostPeralteOverride(1));
            Assert.Equal(0.1, FitOfF2(vm).Remainder, 6);
            Assert.True(vm.IsDirty);
        }

        [Fact]
        public void ANarrowerPalletOrALessHorizontalHolguraThatLeavesASplitWithNoRoomIsRefusedInTheField()
        {
            var vm = SelectiveEditor(SplitF2(rest: 0.6));
            var before = Json(vm.Draft.Design);

            // «Ancho de tarima» of the cells of F2: the front gets narrower.
            vm.SelectFront(1);
            var front = Field(vm.CellFields, "cFront");
            Write(front, "43");
            AssertRefusedAndNothingStays(vm, before, front.Error);
            Assert.Equal(44.0, vm.Draft.Cell(0, 1, 0)!.Frente);

            // «Holgura horizontal» of the rack: every front gets narrower.
            var holgura = Field(vm.Essentials, "tolerance");
            Write(holgura, "3");
            AssertRefusedAndNothingStays(vm, before, holgura.Error);
            Assert.Equal(4.0, vm.Draft.PalletTolerance);

            // A wider pallet is no problem: the front gets wider and the last tramo longer.
            Write(Field(vm.CellFields, "cFront"), "45");
            Assert.True(FitOfF2(vm).Remainder > 0.6);
            Assert.True(vm.IsDirty);
        }

        [Fact]
        public void ASplitThatDidNotFitBeforeIsNoReasonAndNewTramosAreCheckedWhereTheyAreWritten()
        {
            // An old drawing whose F2 never fitted (drawn whole): a change elsewhere is not refused because of it (PP-S8).
            var design = SplitF2(rest: 0.6);
            design.Bays[1].Segments[0].Length += 5.0;
            var vm = SelectiveEditor(design);
            Assert.False(FitOfF2(vm).Fits);

            var row = vm.PostRows[1].Field;
            Write(row, "4");
            Assert.True(string.IsNullOrEmpty(row.Error), row.Error);
            Assert.Equal(4.0, vm.Draft.PostPeralteOverride(1));

            // A split that fits and is taken away («Sin medio frente»), or dropped with its front, is no split broken by the write.
            var other = SelectiveEditor(SplitF2(rest: 0.6));
            other.SelectFront(1);
            other.ClearSegments();
            Assert.False(other.Draft.Front(0, 1)!.IsSplit);
            var dropped = SelectiveEditor(SplitF2(rest: 0.6));
            var fronts = Field(dropped.Essentials, "fronts");
            Write(fronts, "1");
            Assert.True(string.IsNullOrEmpty(fronts.Error), fronts.Error);
            Assert.Equal(1, dropped.Draft.FrontCount(0));
        }
    }
}
