using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using RackCad.Application.Headers;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using RackCad.UI;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-18b round 3 (PB-VAL-01 closure) — STA tests of the informative card matrix and SEMANTIC tests of the technical
    /// preview over the REAL <see cref="RackPushBackSystemWindow"/>. Preview assertions inspect
    /// <see cref="PushBackPreviewModel"/> primitives (role/piece/kind/coordinates) and signatures — never pixels, never a
    /// bare <c>Canvas.Children</c> count.
    /// </summary>
    public sealed class PushBackMatrixAndPreviewTests
    {
        private static PushBackDesign SampleDesign(int front0Levels = 3, int front1Levels = 2)
        {
            var design = new PushBackDesign
            {
                Structure = new DynamicRackDesign
                {
                    Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                    PalletsDeep = 6,
                    LoadLevels = Math.Max(front0Levels, front1Levels),
                    FirstLevelHeight = 6.0,
                    BeamDepth = 4.0
                }
            };
            design.Structure.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 2, LoadLevels = front0Levels, PalletsDeep = 6, DepthStartPosition = 1 });
            design.Structure.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = front1Levels, PalletsDeep = 4, DepthStartPosition = 3 });
            return design;
        }

        private static RackPushBackSystemWindow NewWindow()
        {
            var w = new RackPushBackSystemWindow();
            w.LoadDesignForNew(SampleDesign(), "PB-R3");
            return w;
        }

        private static Grid CardGrid(RackPushBackSystemWindow w) => (Grid)w.FindName("PushBackMatrixGrid");

        private static System.Collections.Generic.List<Border> Cards(RackPushBackSystemWindow w)
            => CardGrid(w).Children.OfType<Border>().ToList();

        private static Border CardAt(RackPushBackSystemWindow w, int frontIndex, int levelIndex)
        {
            var levels = w.State.Structure.MaxLoadLevels();
            var row = levels - levelIndex;
            return CardGrid(w).Children.OfType<Border>()
                .Single(b => Grid.GetRow(b) == row && Grid.GetColumn(b) == frontIndex + 1);
        }

        private static string CardTextAt(RackPushBackSystemWindow w, int frontIndex, int levelIndex)
            => ((TextBlock)CardAt(w, frontIndex, levelIndex).Child).Text;

        // ==================== Matrix ====================

        [Fact]
        public void Matrix_HasOneCardPerSlot_JaggedGhostsShowDashAndAreNotEditable()
        {
            StaTestRunner.Run(() =>
            {
                var w = NewWindow();
                try
                {
                    // 2 fronts x 3 padded levels = 6 slots: 5 real cells + 1 ghost (front 2, level 3).
                    Assert.Equal(6, Cards(w).Count);
                    Assert.Equal("—", CardTextAt(w, 1, 2));
                    Assert.NotEqual("—", CardTextAt(w, 0, 2));

                    // Clicking the ghost selects its FRONT (clamped level), never a phantom cell.
                    w.SelectMatrixCell(1, 2, false);
                    Assert.Equal(1, w.State.Structure.SelectedFrontIndex);
                    Assert.True(w.State.Structure.SelectedLevelIndex < 2, "ghost click must clamp to a real level");
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void Cards_ShowFondosPeraltesAndTope_FromTheAuthority()
        {
            StaTestRunner.Run(() =>
            {
                var w = NewWindow();
                try
                {
                    var text = CardTextAt(w, 0, 0);
                    Assert.Equal(PushBackMatrixCardModel.CardText(w.State, 0, 0), text);
                    Assert.Contains("×2", text);          // posiciones of front 1
                    Assert.Contains("6F", text);          // fondos
                    Assert.Contains("ini 1", text);       // DepthStartPosition
                    // The IN/OUT peralte shown is the RESOLVED cell value (the resolver may snap the requested depth
                    // to the catalog's allowed list), never a stale literal.
                    var inOut = w.State.Structure.Fronts[0].Cells[0].InOutBeamDepth;
                    Assert.Contains(string.Format(System.Globalization.CultureInfo.InvariantCulture, "IN/OUT {0:0.##}\"", inOut), text);
                    Assert.Contains("Post 3.5\"", text);  // rear peralte default
                    Assert.Contains("Tope ✔", text);      // tope active by default

                    var other = CardTextAt(w, 1, 0);
                    Assert.Contains("×1", other);
                    Assert.Contains("4F", other);
                    Assert.Contains("ini 3", other);      // the second front's own DepthStartPosition
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void PlainClick_OnTheRealCard_ReplacesSelectionAndLoadsTheCellEditor()
        {
            StaTestRunner.Run(() =>
            {
                var w = NewWindow();
                try
                {
                    var card = CardAt(w, 1, 1);
                    card.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
                    {
                        RoutedEvent = UIElement.MouseLeftButtonDownEvent
                    });

                    Assert.Equal(1, w.State.Structure.SelectedFrontIndex);
                    Assert.Equal(1, w.State.Structure.SelectedLevelIndex);
                    Assert.Equal(1, w.State.Structure.SelectedCellCount);   // plain click REPLACES the selection

                    // The cell editor followed the primary: the positions box shows front 2's PalletCount.
                    var positions = (RackCad.UI.Controls.NumericField)w.FindName("PositionsBox");
                    Assert.Equal(1.0, positions.Value);
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void ExtendClick_GrowsAndShrinksTheMultiSelection_NeverEmpty()
        {
            StaTestRunner.Run(() =>
            {
                var w = NewWindow();
                try
                {
                    w.SelectMatrixCell(0, 0, false);
                    w.SelectMatrixCell(1, 1, true);       // Ctrl+click: extend
                    Assert.Equal(2, w.State.Structure.SelectedCellCount);
                    Assert.True(w.State.Structure.IsSelected(0, 0));
                    Assert.True(w.State.Structure.IsSelected(1, 1));
                    Assert.Equal(1, w.State.Structure.SelectedFrontIndex); // last touched is primary

                    w.SelectMatrixCell(1, 1, true);       // extend-toggle removes it (others remain)
                    Assert.Equal(1, w.State.Structure.SelectedCellCount);
                    Assert.True(w.State.Structure.IsSelected(0, 0));

                    w.SelectMatrixCell(0, 0, true);       // extend-toggle on the LAST cell must never empty the selection
                    Assert.True(w.State.Structure.SelectedCellCount >= 1);
                    Assert.True(w.State.Structure.IsSelected(0, 0));
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void ApplyToSelection_ChangesExactlyTheSelectedCards()
        {
            StaTestRunner.Run(() =>
            {
                var w = NewWindow();
                try
                {
                    w.SelectMatrixCell(0, 0, false);
                    w.SelectMatrixCell(0, 1, true);
                    ((ComboBox)w.FindName("RearPeralteBox")).SelectedItem = 5.0;
                    EditorWindowTestSupport.ClickNamed(w, "ApplySelectedButton");

                    Assert.Equal(5.0, w.State.Cell(0, 0).HighEndBeamPeralte, 6);
                    Assert.Equal(5.0, w.State.Cell(0, 1).HighEndBeamPeralte, 6);
                    Assert.Equal(3.5, w.State.Cell(0, 2).HighEndBeamPeralte, 6);   // NOT selected: untouched
                    Assert.Equal(3.5, w.State.Cell(1, 0).HighEndBeamPeralte, 6);

                    Assert.Contains("Post 5\"", CardTextAt(w, 0, 0));
                    Assert.Contains("Post 5\"", CardTextAt(w, 0, 1));
                    Assert.Contains("Post 3.5\"", CardTextAt(w, 0, 2));
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void TogglingATope_UpdatesItsCardInPlace()
        {
            StaTestRunner.Run(() =>
            {
                var w = NewWindow();
                try
                {
                    Assert.Contains("Tope ✔", CardTextAt(w, 0, 0));
                    var check = (CheckBox)w.FindName("RearTopeActiveCheck");
                    check.IsChecked = false;
                    EditorWindowTestSupport.ClickNamed(w, "RearTopeActiveCheck");
                    Assert.False(w.State.Cell(0, 0).RearTopeEnabled);
                    Assert.Contains("Sin tope", CardTextAt(w, 0, 0));
                    Assert.Contains("Tope ✔", CardTextAt(w, 0, 1));   // only the touched card changed
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void GrowingAndShrinkingTheStructure_RebuildsTheCards()
        {
            StaTestRunner.Run(() =>
            {
                var w = NewWindow();
                try
                {
                    Assert.Equal(6, Cards(w).Count);

                    w.SelectMatrixCell(0, 0, false);
                    EditorWindowTestSupport.ClickNamed(w, "AddLevelButton");   // front 1: 3 -> 4 levels
                    Assert.Equal(8, Cards(w).Count);                            // 2 fronts x 4 padded levels
                    Assert.NotEqual("—", CardTextAt(w, 0, 3));
                    Assert.Equal("—", CardTextAt(w, 1, 3));

                    EditorWindowTestSupport.ClickNamed(w, "RemoveFrontButton"); // 2 -> 1 front
                    Assert.Equal(4, Cards(w).Count);
                }
                finally { w.Close(); }
            });
        }

        // ==================== Preview (semantic) ====================

        [Fact]
        public void Preview_Lateral_DrawsBedEndBeamsIntermediatesAndTopes_AsRealLines()
        {
            StaTestRunner.Run(() =>
            {
                var w = NewWindow();
                try
                {
                    ((ComboBox)w.FindName("ViewBox")).SelectedIndex = 0;
                    var model = w.CurrentPreviewModel;

                    Assert.False(model.IsEmpty);
                    Assert.True(model.Lines.Any(), "the technical preview must produce LINES, not isolated markers only");

                    Assert.Contains(model.Lines, p => p.Role == HeaderBlockRole.Rail);                                  // cama
                    Assert.Contains(model.Lines, p => p.Role == HeaderBlockRole.Beam
                        && string.Equals(p.PieceId, DynamicRackDefaults.InOutBeamCatalogId, StringComparison.OrdinalIgnoreCase));   // larguero bajo
                    Assert.Contains(model.Lines, p => p.Role == HeaderBlockRole.Beam
                        && string.Equals(p.PieceId, PushBackDefaults.HighEndBeamCatalogId, StringComparison.OrdinalIgnoreCase));    // larguero posterior
                    Assert.Contains(model.Lines, p => p.Role == HeaderBlockRole.Beam
                        && string.Equals(p.PieceId, DynamicRackDefaults.IntermediateBeamCatalogId, StringComparison.OrdinalIgnoreCase)); // intermedios
                    Assert.Contains(model.Lines, p => p.Role == HeaderBlockRole.Tope);                                  // topes

                    // The bed line is genuinely inclined: its two endpoints differ in BOTH axes (the resolved slope).
                    var bed = model.Lines.First(p => p.Role == HeaderBlockRole.Rail);
                    Assert.True(Math.Abs(bed.X2 - bed.X1) > 1.0 && Math.Abs(bed.Y2 - bed.Y1) > 0.01);
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void Preview_FrontalEntrada_HasBeamsAndDefaultSafety_ButNoRearTopes()
        {
            StaTestRunner.Run(() =>
            {
                var w = new RackPushBackSystemWindow();   // LoadNew: seeds the default safety (PB-VAL-04)
                try
                {
                    ((ComboBox)w.FindName("ViewBox")).SelectedIndex = 1;
                    var model = w.CurrentPreviewModel;

                    Assert.False(model.IsEmpty);
                    Assert.Contains(model.Primitives, p => p.Role == HeaderBlockRole.Beam);
                    Assert.Contains(model.Primitives, p => p.Role == HeaderBlockRole.Safety);      // low-end default safety
                    Assert.DoesNotContain(model.Primitives, p => p.Role == HeaderBlockRole.Tope);  // rear topes live on the OTHER end
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void Preview_FrontalPosterior_HasRearBeamAndTopes_ButNoNormalSafety()
        {
            StaTestRunner.Run(() =>
            {
                var w = new RackPushBackSystemWindow();
                try
                {
                    ((ComboBox)w.FindName("ViewBox")).SelectedIndex = 2;
                    var model = w.CurrentPreviewModel;

                    Assert.False(model.IsEmpty);
                    Assert.Contains(model.Primitives, p => p.Role == HeaderBlockRole.Beam
                        && string.Equals(p.PieceId, PushBackDefaults.HighEndBeamCatalogId, StringComparison.OrdinalIgnoreCase));
                    Assert.Contains(model.Primitives, p => p.Role == HeaderBlockRole.Tope);
                    Assert.DoesNotContain(model.Primitives, p => p.Role == HeaderBlockRole.Safety); // rear cut: no normal safety
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void Preview_Planta_RendersVisibleGeometry()
        {
            StaTestRunner.Run(() =>
            {
                var w = NewWindow();
                try
                {
                    ((ComboBox)w.FindName("ViewBox")).SelectedIndex = 3;
                    var model = w.CurrentPreviewModel;
                    Assert.False(model.IsEmpty);
                    Assert.True(model.Lines.Any());
                    Assert.True(model.MaxX > model.MinX);
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void Preview_ChangingTheCorte_ChangesTheSignature()
        {
            StaTestRunner.Run(() =>
            {
                var w = NewWindow();   // 2 fronts -> 3 lateral cortes
                try
                {
                    ((ComboBox)w.FindName("ViewBox")).SelectedIndex = 0;
                    var corteBox = (ComboBox)w.FindName("LateralSectionBox");
                    Assert.True(corteBox.Items.Count >= 2, "the fixture must offer several cortes");

                    corteBox.SelectedIndex = 0;
                    var first = w.CurrentPreviewModel.Signature();
                    corteBox.SelectedIndex = 1;
                    var second = w.CurrentPreviewModel.Signature();

                    Assert.NotEqual(first, second);
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void Preview_DeactivatedTope_DisappearsFromTheModel()
        {
            StaTestRunner.Run(() =>
            {
                var w = new RackPushBackSystemWindow();
                try
                {
                    ((ComboBox)w.FindName("ViewBox")).SelectedIndex = 2;   // frontal posterior shows every rear tope
                    var before = w.CurrentPreviewModel.OfRole(HeaderBlockRole.Tope).Count();
                    Assert.True(before > 0);

                    ((CheckBox)w.FindName("RearTopeActiveCheck")).IsChecked = false;
                    EditorWindowTestSupport.ClickNamed(w, "RearTopeActiveCheck");

                    var after = w.CurrentPreviewModel.OfRole(HeaderBlockRole.Tope).Count();
                    Assert.Equal(before - 1, after);
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void Preview_EmptyOrNullPlan_YieldsAnEmptyModelWithoutThrowing()
        {
            var none = PushBackPreviewModel.Build(null, null, "LATERAL", 4.0);
            Assert.True(none.IsEmpty);
            Assert.Equal(string.Empty, none.Signature());

            var empty = PushBackPreviewModel.Build(
                new DynamicSystemPlan(null, null), null, "FRONTAL", 4.0);
            Assert.True(empty.IsEmpty);
        }

        [Fact]
        public void Preview_Build_DoesNotMutateThePlan()
        {
            StaTestRunner.Run(() =>
            {
                var w = NewWindow();
                try
                {
                    var plan = w.CurrentPreviewPlan;
                    var flatBefore = plan.Flatten().Instances;
                    var countBefore = flatBefore.Count;
                    var paramsBefore = flatBefore.Sum(i => i.DynamicParameters.Count);

                    var first = w.CurrentPreviewModel.Signature();
                    var second = w.CurrentPreviewModel.Signature();   // building twice is idempotent

                    var flatAfter = plan.Flatten().Instances;
                    Assert.Equal(countBefore, flatAfter.Count);
                    Assert.Equal(paramsBefore, flatAfter.Sum(i => i.DynamicParameters.Count));
                    Assert.Equal(first, second);
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void PreviewCanvas_PaintsLines_NotOnlyRectangles()
        {
            StaTestRunner.Run(() =>
            {
                var w = NewWindow();
                try
                {
                    w.Show();
                    w.UpdateLayout();

                    var canvas = (Canvas)w.FindName("PreviewCanvas");
                    Assert.True(canvas.Children.OfType<System.Windows.Shapes.Line>().Any(),
                        "the painted preview must contain real Line shapes");

                    var legend = (TextBlock)w.FindName("PreviewLegend");
                    Assert.False(string.IsNullOrWhiteSpace(legend.Text));   // active view + color legend always visible
                    Assert.Contains("Lateral", legend.Text);
                }
                finally { w.Close(); }
            });
        }
    }
}
