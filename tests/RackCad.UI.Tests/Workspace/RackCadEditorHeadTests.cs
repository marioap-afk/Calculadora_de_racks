#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using RackCad.Application.StructuralSections;
using RackCad.Application.VentanaRackCad;
using RackCad.UI.VentanaRackCad;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64, sexta tanda, ui (point 9; O-2 of the smoke of the fourth batch): in an editor about 950 px wide, «Más» went to a second line of
    /// the head, alone or with «Cerrar el editor», while the other buttons stayed beside the name. The buttons of the head of every editor of
    /// the window are one group: beside the name and the state while they fit (as before), or all together on the next line. Real WPF (STA).
    /// </summary>
    public class RackCadEditorHeadTests
    {
        // The width of the editor in the palette: 946 px with the palette at 1200 px (one line), 846 at 1100 and 696 at 950 (the side column).
        public static IEnumerable<object[]> Widths() => new[] { 1036.0, 950.0, 846.0, 696.0 }.Select(w => new object[] { w });

        private static void Lay(FrameworkElement view, double width)
        {
            view.Measure(new Size(width, 860));
            view.Arrange(new Rect(0, 0, width, 860));
            view.UpdateLayout();
        }

        private static double Top(FrameworkElement element, FrameworkElement root) => element.TranslatePoint(new Point(0, 0), root).Y;

        // Every shown button of the head is on one line, and that line is the one of the name when the buttons fit beside it.
        private static void AssertOneGroup(FrameworkElement view, double width, FrameworkElement nameBox, params FrameworkElement[] buttons)
        {
            Lay(view, width);
            var shown = buttons.Where(b => b.Visibility == Visibility.Visible).ToList();
            var tops = shown.Select(b => Top(b, view)).ToList();
            Assert.True(tops.All(t => Math.Abs(t - tops[0]) < 1.0),
                "at " + width + " px: " + string.Join(", ", shown.Select(b => b.Name + " " + Top(b, view).ToString("0"))));
            if (width >= 946) Assert.True(Math.Abs(Top(nameBox, view) - tops[0]) < 20.0, "at " + width + " px the buttons left the line of the name");
        }

        private static readonly Lazy<StructuralSectionCatalog> Sections = new(() =>
            new CsvStructuralSectionCatalogProvider(RackCad.Application.Catalogs.CatalogDirectory.Resolve()).Load());

        [Theory]
        [MemberData(nameof(Widths))]
        public void TheHeadOfSelectivo(double width) => StaTestRunner.Run(() =>
        {
            var view = new SelectiveEditorView(SelectiveWindowFixtures.SelectiveEditor());
            AssertOneGroup(view, width, view.NameBox,
                view.UpdateButton, view.InsertViewButton, view.DiscardButton, view.RestoreButton, view.CloseEditorButton, view.MoreButton);
        });

        [Theory]
        [MemberData(nameof(Widths))]
        public void TheHeadOfDinamico(double width) => StaTestRunner.Run(() =>
        {
            var view = new DynamicEditorView(new DynamicEditorViewModel(DynamicDraftEditor.CreateNew(UiSupport.LoadCatalogSafe()), "DIN-1"));
            AssertOneGroup(view, width, view.NameBox,
                view.UpdateButton, view.InsertViewButton, view.DiscardButton, view.RestoreButton, view.CloseEditorButton, view.MoreButton);
        });

        [Theory]
        [MemberData(nameof(Widths))]
        public void TheHeadOfPushBack(double width) => StaTestRunner.Run(() =>
        {
            var view = new PushBackEditorView(new PushBackEditorViewModel(PushBackDraftEditor.CreateNew(UiSupport.LoadCatalogSafe()), "PB-1"));
            AssertOneGroup(view, width, view.NameBox,
                view.UpdateButton, view.InsertViewButton, view.DiscardButton, view.RestoreButton, view.CloseEditorButton, view.MoreButton);
        });

        [Theory]
        [MemberData(nameof(Widths))]
        public void TheHeadOfCama(double width) => StaTestRunner.Run(() =>
        {
            var view = new CamaEditorView(new CamaEditorViewModel(CamaDraftEditor.CreateNew(UiSupport.LoadCatalogSafe()), "CAMA-1"));
            AssertOneGroup(view, width, view.NameBox,
                view.PrimaryButton, view.DiscardButton, view.RestoreButton, view.CloseEditorButton, view.MoreButton);
        });

        [Theory]
        [MemberData(nameof(Widths))]
        public void TheHeadOfCantilever(double width) => StaTestRunner.Run(() =>
        {
            var view = new CantileverEditorView(new CantileverEditorViewModel(new CantileverDraftEditor(Sections.Value), "CAN-1", _ => { }));
            AssertOneGroup(view, width, view.NameBox,
                view.UpdateButton, view.InsertNewButton, view.InsertViewButton, view.DiscardButton, view.RestoreButton, view.CloseEditorButton, view.MoreButton);
        });
    }
}
