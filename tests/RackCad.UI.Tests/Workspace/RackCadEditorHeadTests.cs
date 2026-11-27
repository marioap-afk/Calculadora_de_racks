#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using RackCad.Application.RackFrames;
using RackCad.Application.StructuralSections;
using RackCad.Application.VentanaRackCad;
using RackCad.Domain.RackFrames;
using RackCad.UI.VentanaRackCad;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64, sexta tanda, ui (point 9; O-2 of the smoke of the fourth batch): in an editor about 950 px wide, «Más» went to a second line of
    /// the head, alone or with «Cerrar el editor», while the other buttons stayed beside the name. The buttons of the head of every editor of
    /// the window are one group: beside the name and the state while they fit (as before), or all together on the next line. Corrections of
    /// the review (sobre 331f9f18): «Más» open does not move the group (its entries are a row of their own under the head), the label
    /// «Insertar en el dibujo…» of a draft keeps the group whole, and the editor of a cabecera groups its buttons too. Real WPF (STA).
    /// </summary>
    public class RackCadEditorHeadTests
    {
        // The width of the editor in the palette: 946 px with the palette at 1200 px (one line), 846 at 1100 and 696 at 950 (the side column).
        private static readonly double[] EditorWidths = { 1036.0, 950.0, 846.0, 696.0 };

        // Each width, as it opens and as a draft of «Nuevo» with «Más» open.
        public static IEnumerable<object[]> Widths() => EditorWidths.SelectMany(w => new[] { new object[] { w, false }, new object[] { w, true } });

        private static void Lay(FrameworkElement view, double width)
        {
            view.Measure(new Size(width, 860));
            view.Arrange(new Rect(0, 0, width, 860));
            view.UpdateLayout();
        }

        private static double Top(FrameworkElement element, FrameworkElement root) => element.TranslatePoint(new Point(0, 0), root).Y;

        // Every shown button of the head is on one line, and that line is the one of the name when the buttons fit beside it. With «Más»
        // open, the buttons stay where they are with it closed, and its entries are below all of them.
        private static void AssertOneGroup(FrameworkElement view, double width, FrameworkElement nameBox, Action? openMore, FrameworkElement? morePanel,
            params FrameworkElement[] buttons)
        {
            Lay(view, width);
            var shown = buttons.Where(b => b.Visibility == Visibility.Visible).ToList();
            var tops = shown.Select(b => Top(b, view)).ToList();
            Assert.True(tops.All(t => Math.Abs(t - tops[0]) < 1.0),
                "at " + width + " px: " + string.Join(", ", shown.Select(b => b.Name + " " + Top(b, view).ToString("0"))));
            if (width >= 946) Assert.True(Math.Abs(Top(nameBox, view) - tops[0]) < 20.0, "at " + width + " px the buttons left the line of the name");

            if (openMore == null || morePanel == null) return;
            openMore();
            Lay(view, width);
            Assert.Equal(Visibility.Visible, morePanel.Visibility);
            var open = shown.Select(b => Top(b, view)).ToList();
            Assert.True(open.Zip(tops, (a, b) => Math.Abs(a - b) < 1.0).All(same => same),
                "«Más» open moved the buttons at " + width + " px: " + string.Join(", ", shown.Select(b => b.Name + " " + Top(b, view).ToString("0"))));
            var lowest = shown.Max(b => Top(b, view) + b.ActualHeight);
            Assert.True(Top(morePanel, view) >= lowest - 1.0, "the entries of «Más» are not under the buttons at " + width + " px");
        }

        private static readonly Lazy<StructuralSectionCatalog> Sections = new(() =>
            new CsvStructuralSectionCatalogProvider(RackCad.Application.Catalogs.CatalogDirectory.Resolve()).Load());

        [Theory]
        [MemberData(nameof(Widths))]
        public void TheHeadOfSelectivo(double width, bool newDraftWithMoreOpen) => StaTestRunner.Run(() =>
        {
            var vm = newDraftWithMoreOpen
                ? SelectiveEditorViewModel.ForNewRack(SelectiveDraftEditor.CreateNew(UiSupport.LoadCatalogSafe()), "nuevo-1")
                : SelectiveWindowFixtures.SelectiveEditor();
            var view = new SelectiveEditorView(vm);
            AssertOneGroup(view, width, view.NameBox, newDraftWithMoreOpen ? vm.ToggleMore : null, view.MorePanel,
                view.UpdateButton, view.InsertViewButton, view.DiscardButton, view.RestoreButton, view.CloseEditorButton, view.MoreButton);
        });

        [Theory]
        [MemberData(nameof(Widths))]
        public void TheHeadOfDinamico(double width, bool newDraftWithMoreOpen) => StaTestRunner.Run(() =>
        {
            var vm = new DynamicEditorViewModel(DynamicDraftEditor.CreateNew(UiSupport.LoadCatalogSafe()), "DIN-1", isNewRack: newDraftWithMoreOpen);
            var view = new DynamicEditorView(vm);
            AssertOneGroup(view, width, view.NameBox, newDraftWithMoreOpen ? vm.ToggleMore : null, view.MorePanel,
                view.UpdateButton, view.InsertViewButton, view.DiscardButton, view.RestoreButton, view.CloseEditorButton, view.MoreButton);
        });

        [Theory]
        [MemberData(nameof(Widths))]
        public void TheHeadOfPushBack(double width, bool newDraftWithMoreOpen) => StaTestRunner.Run(() =>
        {
            var vm = new PushBackEditorViewModel(PushBackDraftEditor.CreateNew(UiSupport.LoadCatalogSafe()), "PB-1", isNewRack: newDraftWithMoreOpen);
            var view = new PushBackEditorView(vm);
            AssertOneGroup(view, width, view.NameBox, newDraftWithMoreOpen ? vm.ToggleMore : null, view.MorePanel,
                view.UpdateButton, view.InsertViewButton, view.DiscardButton, view.RestoreButton, view.CloseEditorButton, view.MoreButton);
        });

        [Theory]
        [MemberData(nameof(Widths))]
        public void TheHeadOfCama(double width, bool newDraftWithMoreOpen) => StaTestRunner.Run(() =>
        {
            var vm = new CamaEditorViewModel(CamaDraftEditor.CreateNew(UiSupport.LoadCatalogSafe()), "CAMA-1", isNewRack: newDraftWithMoreOpen);
            var view = new CamaEditorView(vm);
            AssertOneGroup(view, width, view.NameBox, newDraftWithMoreOpen ? vm.ToggleMore : null, view.MorePanel,
                view.PrimaryButton, view.DiscardButton, view.RestoreButton, view.CloseEditorButton, view.MoreButton);
        });

        [Theory]
        [MemberData(nameof(Widths))]
        public void TheHeadOfCantilever(double width, bool newDraftWithMoreOpen) => StaTestRunner.Run(() =>
        {
            var vm = new CantileverEditorViewModel(new CantileverDraftEditor(Sections.Value), "CAN-1", _ => { }, isNewRack: newDraftWithMoreOpen);
            var view = new CantileverEditorView(vm);
            AssertOneGroup(view, width, view.NameBox, newDraftWithMoreOpen ? vm.ToggleMore : null, view.MorePanel,
                view.UpdateButton, view.InsertNewButton, view.InsertViewButton, view.DiscardButton, view.RestoreButton, view.CloseEditorButton, view.MoreButton);
        });

        // Correction 6: the editor of a cabecera of its own («Cabecera N» of the list) had «Cerrar el editor» alone on a second line at about
        // 700 px. It has no «Más».
        [Theory]
        [InlineData(1036.0)]
        [InlineData(950.0)]
        [InlineData(846.0)]
        [InlineData(696.0)]
        public void TheHeadOfCabecera(double width) => StaTestRunner.Run(() =>
        {
            var opened = new RackFrameConfigurationFactory(UiSupport.LoadCatalogSafe())
                .Build(RackFrameTemplateCatalog.FindStandardOrDefault(), SelectiveWindowFixtures.PostId, 132.0, 42.0);
            var draft = new CabeceraDraftEditor(opened, UiSupport.LoadCatalogSafe(), RackFrameTemplateCatalog.All, CabeceraDraftContext.Standalone);
            var view = new CabeceraEditorView(CabeceraEditorViewModel.Standalone(draft, "CAB-1", _ => { }));
            AssertOneGroup(view, width, view.NameBox, null, null,
                view.UpdateButton, view.DiscardButton, view.RestoreButton, view.CloseEditorButton);
        });
    }
}
