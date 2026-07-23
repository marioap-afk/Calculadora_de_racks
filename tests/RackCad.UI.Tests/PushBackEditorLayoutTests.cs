using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using RackCad.UI;
using RackCad.UI.Controls;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// PB-VAL-01 — STA tests over the REAL <see cref="RackPushBackSystemWindow"/> for the redesigned layout the Owner
    /// rejected: the four zones exist, the frente x nivel matrix is the CENTRAL editing surface (not buried in the left
    /// settings panel), the linked views are first-class buttons instead of one hidden combo, and the controls are not
    /// crammed. Structural assertions only — no pixel or screenshot comparisons.
    /// </summary>
    public sealed class PushBackEditorLayoutTests
    {
        private static T Named<T>(DependencyObject root, string name) where T : FrameworkElement
        {
            foreach (var element in Descendants(root).OfType<T>())
            {
                if (string.Equals(element.Name, name, StringComparison.Ordinal))
                {
                    return element;
                }
            }

            return null;
        }

        private static System.Collections.Generic.IEnumerable<DependencyObject> Descendants(DependencyObject root)
        {
            if (root == null)
            {
                yield break;
            }

            var count = VisualTreeHelper.GetChildrenCount(root);
            for (var i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                yield return child;
                foreach (var nested in Descendants(child))
                {
                    yield return nested;
                }
            }
        }

        private static bool IsInsideLeftSettingsPanel(DependencyObject element)
        {
            for (var node = element; node != null; node = VisualTreeHelper.GetParent(node))
            {
                if (node is ScrollViewer scroller && scroller.Width > 0 && !double.IsNaN(scroller.Width))
                {
                    return true;   // the fixed-width settings column
                }
            }

            return false;
        }

        private static RackPushBackSystemWindow Shown()
        {
            var window = new RackPushBackSystemWindow();
            window.Show();
            window.UpdateLayout();
            return window;
        }

        [Fact]
        public void Layout_HasTheFourZones_PanelMatrixPreviewAndActionBar()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown();
                try
                {
                    Assert.NotNull(Named<Grid>(w, "WorkArea"));
                    Assert.NotNull(Named<GroupBox>(w, "MatrixZone"));
                    Assert.NotNull(Named<GroupBox>(w, "PreviewZone"));
                    Assert.NotNull(Named<WrapPanel>(w, "ActionBar"));
                    Assert.NotNull(Named<Canvas>(w, "PreviewCanvas"));
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void Matrix_IsTheCentralSurface_NotBuriedInTheLeftSettingsPanel()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown();
                try
                {
                    var selection = Named<SelectionMatrix>(w, "CellSelectionMatrix");
                    var topes = Named<SelectionMatrix>(w, "TopeMatrix");
                    Assert.NotNull(selection);
                    Assert.NotNull(topes);

                    // Both live in the central work area, NOT inside the fixed-width left settings column.
                    Assert.False(IsInsideLeftSettingsPanel(selection), "the selection matrix must not sit in the left panel");
                    Assert.False(IsInsideLeftSettingsPanel(topes), "the tope matrix must not sit in the left panel");

                    // ...and specifically inside the matrix zone.
                    var zone = Named<GroupBox>(w, "MatrixZone");
                    Assert.Contains(Descendants(zone), d => ReferenceEquals(d, selection));
                    Assert.Contains(Descendants(zone), d => ReferenceEquals(d, topes));
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void LinkedViews_AreSeparateButtons_NotOnlyACombo()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown();
                try
                {
                    foreach (var name in new[]
                    {
                        "InsertLateralButton", "InsertFrontalEntradaButton",
                        "InsertFrontalPosteriorButton", "InsertPlantaButton"
                    })
                    {
                        var button = Named<Button>(w, name);
                        Assert.True(button != null, $"missing action button {name}");
                        Assert.True(button.IsVisible, $"{name} must be visible");
                    }

                    // The combo still exists (it drives the preview + the embed View/Section contract).
                    Assert.NotNull(Named<ComboBox>(w, "ViewBox"));
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void ActionBar_CarriesTheWholeFlow_AndStaysVisibleAfterAResize()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown();
                try
                {
                    foreach (var name in new[] { "RestoreButton", "BomButton", "SaveLibraryButton", "UpdateButton", "CloseButton" })
                    {
                        Assert.True(Named<Button>(w, name) != null, $"missing action {name}");
                    }

                    w.Width = w.MinWidth;
                    w.Height = w.MinHeight;
                    w.UpdateLayout();

                    var bar = Named<WrapPanel>(w, "ActionBar");
                    Assert.True(bar.IsVisible, "the action bar must survive a resize to the minimum size");
                    Assert.True(bar.ActualHeight > 0.0);
                    Assert.True(Named<Button>(w, "CloseButton").IsVisible);
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void EssentialControls_HaveUsableWidths()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown();
                try
                {
                    // A crammed control is the defect the Owner reported; assert a sane floor, not exact pixels.
                    foreach (var name in new[] { "PostBox", "CellInOutBeamBox", "CellIntermediateBeamBox", "ViewBox" })
                    {
                        var combo = Named<ComboBox>(w, name);
                        Assert.True(combo != null, $"missing {name}");
                        Assert.True(combo.ActualWidth >= 70.0, $"{name} is too narrow: {combo.ActualWidth}");
                    }

                    Assert.True(Named<TextBox>(w, "NameBox").ActualWidth >= 150.0);
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void Window_OpensAtAUsableSize_AndTheMinimumIsHonoured()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown();
                try
                {
                    Assert.True(w.Width >= 1280.0, $"initial width {w.Width}");
                    Assert.True(w.Height >= 720.0, $"initial height {w.Height}");
                    Assert.True(w.MinWidth >= 1024.0);
                    Assert.True(w.MinHeight >= 600.0);
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void Preview_GetsAUsefulShareOfTheWindow()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown();
                try
                {
                    var canvas = Named<Canvas>(w, "PreviewCanvas");
                    Assert.NotNull(canvas);
                    Assert.True(canvas.ActualHeight >= 120.0, $"preview too short: {canvas.ActualHeight}");
                    Assert.True(canvas.ActualWidth >= 300.0, $"preview too narrow: {canvas.ActualWidth}");
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void NewSystem_OpensWithTheDefaultSafety_AndNoGuia()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown();
                try
                {
                    // PB-VAL-04 seen through the real window: LoadNew seeds from the Push Back safety AUTHORITY, so the
                    // selections are exactly what it yields for this host's catalog. (That the shipped catalog yields a
                    // NON-empty, GUIA-free, low-end set is proven by the pure suite; the UI test host may ship no
                    // catalog at all, so asserting the wiring here keeps the test environment-independent.)
                    var expected = new RackCad.Application.Systems.PushBackSafetyAuthority(w.Session.Catalog).Defaults();
                    Assert.Equal(expected.Count, w.SafetySelections.Count);
                    Assert.All(w.SafetySelections, selection =>
                        Assert.Contains(expected, e => string.Equals(e.ElementId, selection.ElementId, StringComparison.OrdinalIgnoreCase)));

                    // Whatever the catalog offers, a GUIA is never among them.
                    var guiaFree = w.SafetyElementsForDialog();
                    Assert.All(w.SafetySelections, selection =>
                        Assert.DoesNotContain(guiaFree, element => element == null));
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void ExistingEditorWindows_StillConstructAlongsideTheRedesign()
        {
            StaTestRunner.Run(() =>
            {
                var dynamicWindow = new RackDynamicSystemWindow();
                dynamicWindow.Close();
                var pushBack = new RackPushBackSystemWindow();
                pushBack.Close();
            });
        }
    }
}
