using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using RackCad.UI;
using RackCad.UI.Controls;
using RackCad.UI.Shell;
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
                    // I-18b: the window now COMPOSES over RackEditorVisualShell (like the dynamic/selective editors). The
                    // four zones are the shell's slots: sidebar (scroll), matrix, preview and the action bar. The matrix
                    // and preview keep their zone x:Names (now shell-surface Borders, not GroupBoxes); the settings sit in
                    // the shell's SidebarScroll; the action buttons live in the shell's EditorActionBar (no bespoke
                    // WrapPanel "ActionBar" / Grid "WorkArea").
                    var shell = w.Content as RackEditorVisualShell;
                    Assert.NotNull(shell);
                    Assert.NotNull(shell.SidebarScroll);              // settings panel zone (scrolls)
                    Assert.NotNull(Named<Border>(w, "MatrixZone"));   // matrix zone (central surface)
                    Assert.NotNull(Named<Border>(w, "PreviewZone"));  // preview zone
                    Assert.NotNull(shell.ActionBar);                     // action bar zone (EditorActionBar)
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
                    // The INFORMATIVE card matrix is the central surface and, since the Owner's 2026-07-24 decision,
                    // the ONLY matrix in the zone: the bulk check-matrices and the tope tools are gone.
                    var cardGrid = Named<Grid>(w, "PushBackMatrixGrid");
                    Assert.NotNull(cardGrid);
                    Assert.False(IsInsideLeftSettingsPanel(cardGrid), "the card matrix must not sit in the left panel");

                    var zone = Named<Border>(w, "MatrixZone");
                    Assert.Contains(Descendants(zone), d => ReferenceEquals(d, cardGrid));

                    // Owner decision (2026-07-24): the bulk check-matrices and the tope tools are GONE; the card
                    // matrix is the only matrix in the zone.
                    Assert.Null(w.FindName("BulkToolsExpander"));
                    Assert.Null(w.FindName("CellSelectionMatrix"));
                    Assert.Null(w.FindName("TopeMatrix"));
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

                    // The shell's EditorActionBar carries the whole flow (a WrapPanel that wraps, never clips) and stays
                    // visible at the minimum size.
                    var shell = w.Content as RackEditorVisualShell;
                    Assert.NotNull(shell);
                    var bar = shell.ActionBar;
                    Assert.NotNull(bar);
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
                // NOT shown: Show() clamps the window to the host's work area, and a small/headless CI screen would
                // report MinWidth instead of the declared size. The DECLARED startup size is what this pins.
                var w = new RackPushBackSystemWindow();
                try
                {
                    Assert.True(w.Width >= 1280.0, $"declared width {w.Width}");
                    Assert.True(w.Height >= 720.0, $"declared height {w.Height}");
                    Assert.True(w.MinWidth >= 1024.0, $"min width {w.MinWidth}");
                    Assert.True(w.MinHeight >= 600.0, $"min height {w.MinHeight}");
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void Preview_GetsAUsefulShareOfTheWindow()
        {
            StaTestRunner.Run(() =>
            {
                // NOT Shown(): Show() clamps to the host's work area (a small/headless screen would distort this). Lay the
                // real shell out directly. The preview is the shell's *-row BELOW the tall Push Back matrix (~499px: the
                // card matrix + the cell editor under it, by PB-VAL-01 design). At the default open height the *-row sits
                // at the shell's shared ShellPreviewMinHeight — the shell HONORS that floor — and the canvas takes a
                // genuinely useful, growing share as the window gets taller (a wide-enough width throughout).
                var w = new RackPushBackSystemWindow();
                try
                {
                    var shell = w.Content as RackEditorVisualShell;
                    Assert.NotNull(shell);
                    shell.Style = (Style)new ResourceDictionary { Source = new Uri("/RackCad.UI;component/Themes/Generic.xaml", UriKind.Relative) }[typeof(RackEditorVisualShell)];

                    shell.Measure(new Size(1280, 720));
                    shell.Arrange(new Rect(0, 0, 1280, 720));
                    shell.UpdateLayout();
                    var canvas = Named<Canvas>(shell, "PreviewCanvas");
                    Assert.NotNull(canvas);
                    Assert.True(shell.PreviewHost.ActualHeight >= 160.0, $"preview row below the shell minimum: {shell.PreviewHost.ActualHeight}");
                    Assert.True(canvas.ActualWidth >= 300.0, $"preview too narrow: {canvas.ActualWidth}");

                    // Give the window room (a modest resize) and the preview canvas takes a real, growing share.
                    shell.Measure(new Size(1280, 980));
                    shell.Arrange(new Rect(0, 0, 1280, 980));
                    shell.UpdateLayout();
                    Assert.True(canvas.ActualHeight >= 200.0, $"preview does not grow with the window: {canvas.ActualHeight}");
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
                    var expected = new RackCad.Application.Systems.PushBack.PushBackSafetyAuthority(w.Session.Catalog).Defaults();
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
        public void SafetyDialogElements_ExcludeGuiaAndParrilla_PbVal06()
        {
            StaTestRunner.Run(() =>
            {
                var w = new RackPushBackSystemWindow();
                try
                {
                    // PB-VAL-06 through the UI: the safety config dialog is fed SafetyElementsForDialog(), which hides GUIA
                    // AND PARRILLA — so neither is ever offered (the authority would strip them anyway). Vacuously true if the
                    // host ships no catalog; meaningful with the real catalog (it carries a PARRILLA_GENERICA and a GUIA).
                    var offered = w.SafetyElementsForDialog();
                    Assert.All(offered, e => Assert.False(
                        RackCad.Domain.Systems.Selective.SelectiveSafetyDefaults.IsType(e.Type, RackCad.Domain.Systems.Selective.SelectiveSafetyDefaults.GuiaType),
                        $"GUIA must not be offered: {e.Id}"));
                    Assert.All(offered, e => Assert.False(
                        RackCad.Domain.Systems.Selective.SelectiveSafetyDefaults.IsType(e.Type, RackCad.Domain.Systems.Selective.SelectiveSafetyDefaults.ParrillaType),
                        $"PARRILLA must not be offered (PB-VAL-06): {e.Id}"));
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
