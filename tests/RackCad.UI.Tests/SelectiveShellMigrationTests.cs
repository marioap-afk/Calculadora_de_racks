using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.Selective;
using RackCad.UI;
using RackCad.UI.Editor;
using RackCad.UI.Shell;
using RackCad.UI.Systems.Dynamic;
using RackCad.UI.Systems.Selective;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-31 migration (ADR-0019 accepted): STA + structural tests proving the REAL
    /// <see cref="RackSelectiveWindow"/> is now COMPOSED over <see cref="RackEditorVisualShell"/> — exactly as the dynamic
    /// editor (I-30) — and that the migration is behavior-preserving. These are the visual-adoption counterpart to
    /// <see cref="SelectiveEditorWindowTests"/> / <see cref="SelectiveEditorStateAdoptionTests"/> (which already lock the
    /// insert/update identity through the real handlers and the resolved geometry across the I-20 state extraction). Here we
    /// assert the window's root IS the shell, every original control keeps its x:Name and lands in the correct neutral slot,
    /// no control was dropped, the gated draw actions keep their reason + show-on-disabled, the matrix / fondo selector /
    /// status / preview render inside their slots, single-cell selection + scope application still fire through the shell-
    /// hosted grid, and the shell actually lays the window out (sidebar scroll, preview draw, resize, shown-at-minimum with
    /// no clip or overlap). Adds the ONE real-handler gap: inserting the PLANTA view. Assertions are structural/semantic —
    /// never screenshots or pixel comparisons. Selective uses SINGLE-cell selection (no multiselection in main): the tests
    /// preserve that, they do NOT introduce Ctrl+click.
    /// </summary>
    public sealed class SelectiveShellMigrationTests
    {
        // Real catalog ids shipped next to the test binaries (mirrors SelectiveEditorWindowTests).
        private const string PostId = "POSTE_OMEGA_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA";
        private const string BeamId = "LARGUERO_ESCALON_CAL14_3_REMACHES";

        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        // Every x:Name defined in RackSelectiveWindow.xaml. The I-31 migration reparented these into the shell's slots
        // and none may be lost (the code-behind binds to all of them by name). I-43 gate 8A moved the frente properties
        // out of the matrix into the left panel, so "FloorRiseBox" (the run-wide elevation, a concept the Owner
        // rejected) is gone and the frente panel + the single "Fondos destino" selector are here instead.
        private static readonly string[] AllNamedControls =
        {
            "Shell", "NameBox", "DrawBasePlateCheck", "NumberFrontsCheck", "NumberLevelsCheck", "DrawRackNameCheck",
            "DrawPalletsCheck", "AnnotationScaleBox", "DimensionsBox", "DimStyleBox", "SafetyButton", "PostBox",
            "PostPeralteBox", "PostSelectBox", "CustomizePostButton", "PostPeralteOverrideBox", "PostCabeceraStatus",
            "BayCountBox", "ToleranceBox", "ClearanceBox", "FondoBox", "CabeceraFondoBox", "FondosBox",
            "FrontHeader", "FrontScopeBox", "FrontFloorBeamCheck", "ApplyFrontFloorBeamButton",
            "FrontRiseBox", "ApplyFrontRiseButton", "FrontLevelsBox", "ApplyFrontLevelsButton",
            "TargetFondosPanel", "TargetFondosButton", "TargetFondosPopup", "TargetFondosList",
            "SeparatorsSection", "SeparatorsHost", "CellHeader", "CellBeamBox", "FrenteBox", "PalletCountBox", "AltoBox",
            "BeamPeralteCombo", "BeamLenBox", "ClearBox", "SummaryText", "StatusText", "FondoSelectorPanel",
            "FondoSelectorBox", "MatrixGrid", "PreviewFrontalRadio", "PreviewLateralRadio", "PreviewHint", "PreviewCanvas",
            "UpdateButton", "InsertLateralButton", "InsertPlantaButton"
        };

        // The three draw actions that carry a show-on-disabled reason (they gate on the AutoCAD origin / editing state).
        private static readonly string[] GatedDrawButtons = { "UpdateButton", "InsertLateralButton", "InsertPlantaButton" };

        // ---- 1. the window is composed over the shell (no bespoke outer Grid left) ----

        [Fact]
        public void SelectiveWindow_RootContentIsTheVisualShell()
        {
            var (contentIsShell, sameAsField) = StaTestRunner.Run(() =>
            {
                var window = new RackSelectiveWindow(canInsertInAutoCad: true);
                return (window.Content is RackEditorVisualShell, ReferenceEquals(window.Content, window.Shell));
            });

            Assert.True(contentIsShell);   // the window's root visual is the shared shell, not the old 2×3 Grid
            Assert.True(sameAsField);       // and it is the x:Name="Shell" instance the code-behind/tests reference
        }

        // ---- 2. no control was dropped in the reparent ----

        [Fact]
        public void SelectiveWindow_PreservesEveryNamedControl()
        {
            var missing = StaTestRunner.Run(() =>
            {
                var window = new RackSelectiveWindow(canInsertInAutoCad: true);
                return AllNamedControls.Where(name => window.FindName(name) == null).ToArray();
            });

            Assert.Empty(missing); // all 45 named elements (+ Shell) still resolve in the window's name scope after migration
        }

        // ---- 3. each control lands in the correct neutral slot (once, in its zone) ----

        [Fact]
        public void SelectiveWindow_PlacesEachControlInItsShellSlot()
        {
            StaTestRunner.Run(() =>
            {
                var window = new RackSelectiveWindow(canInsertInAutoCad: true);
                var shell = window.Shell;

                // Sidebar: global settings + selected-cell editor live in the scrolling side panel.
                foreach (var name in new[] { "NameBox", "DrawBasePlateCheck", "PostBox", "BayCountBox", "ToleranceBox",
                    "FondoBox", "FondosBox", "SafetyButton", "CellHeader", "FrenteBox", "SeparatorsHost" })
                {
                    AssertInSlot(shell.SidePanelContent, window, name, "SidePanelContent");
                }

                // Matrix: the bay×level grid AND the fondo selector are the central editing surface.
                AssertInSlot(shell.MatrixContent, window, "MatrixGrid", "MatrixContent");
                AssertInSlot(shell.MatrixContent, window, "FondoSelectorPanel", "MatrixContent");
                AssertInSlot(shell.MatrixContent, window, "FondoSelectorBox", "MatrixContent");

                // Preview: the frontal/lateral canvas + its view selector.
                foreach (var name in new[] { "PreviewCanvas", "PreviewFrontalRadio", "PreviewLateralRadio", "PreviewHint" })
                {
                    AssertInSlot(shell.PreviewContent, window, name, "PreviewContent");
                }

                // Status: summary + status line, in the always-visible band (moved out of the sidebar scroll).
                AssertInSlot(shell.StatusContent, window, "SummaryText", "StatusContent");
                AssertInSlot(shell.StatusContent, window, "StatusText", "StatusContent");

                // Actions, by neutral category — order preserved: Actualizar · Lista · Guardar · frontal · lateral · planta · Cerrar.
                AssertInSlot(shell.LeadingActions, window, "UpdateButton", "LeadingActions");   // Leading = in-place update
                AssertContentButton(shell.SecondaryActions, "Lista de materiales");             // Secondary = BOM/library
                AssertContentButton(shell.SecondaryActions, "Guardar en biblioteca");
                AssertContentButton(shell.PrimaryActions, "Insertar frontal");                  // Primary = the three inserts
                AssertInSlot(shell.PrimaryActions, window, "InsertLateralButton", "PrimaryActions");
                AssertInSlot(shell.PrimaryActions, window, "InsertPlantaButton", "PrimaryActions");
                Assert.Equal("Cerrar", ((Button)shell.TrailingActions).Content);                // Trailing = close
            });
        }

        // ---- 4. disabled draw actions keep their reason and show-on-disabled ----

        [Fact]
        public void SelectiveWindow_DisabledDrawActions_KeepReasonAndShowOnDisabled()
        {
            StaTestRunner.Run(() =>
            {
                // Opened NOT from AutoCAD → every gated draw button is disabled, still shows its tooltip while disabled,
                // and carries the origin reason. The migration must not have dropped these (they moved into Leading/Primary).
                var window = new RackSelectiveWindow(canInsertInAutoCad: false);
                foreach (var name in GatedDrawButtons)
                {
                    var button = (Button)window.FindName(name);
                    Assert.False(button.IsEnabled, $"{name} must be disabled when not opened from AutoCAD");
                    Assert.True(ToolTipService.GetShowOnDisabled(button), $"{name} must keep ShowOnDisabled");
                    Assert.Equal("Disponible solo cuando la ventana se abre desde AutoCAD.", button.ToolTip);
                }

                // "Insertar frontal" (the new-rack entry point) is never gated and stays enabled with its own tooltip.
                var frontal = FindButtonByContent((DependencyObject)window.Shell.PrimaryActions, "Insertar frontal");
                Assert.NotNull(frontal);
                Assert.True(frontal.IsEnabled);
            });
        }

        [Fact]
        public void SelectiveWindow_DrawActionsEnabled_WhenEditingExistingFromAutoCad_WithTheirRealTooltips()
        {
            StaTestRunner.Run(() =>
            {
                // RACKEDITAR from AutoCAD on an existing rack → the three gated draws light up with their descriptive tooltips
                // restored (UpdateInsertButtons swaps the disabled reason back).
                var window = new RackSelectiveWindow(canInsertInAutoCad: true);
                window.LoadExisting(SelectivePalletDesignDocument.From(MinimalDesign(), "GUID-SEL", "Selectivo A"));

                var lateral = (Button)window.FindName("InsertLateralButton");
                Assert.True(lateral.IsEnabled);
                Assert.True(ToolTipService.GetShowOnDisabled(lateral));
                Assert.Equal("Inserta una VISTA LATERAL (un corte de poste) enlazada a este rack (mismo GUID) y refresca las demás vistas.", lateral.ToolTip);
                Assert.True(((Button)window.FindName("UpdateButton")).IsEnabled);
                Assert.True(((Button)window.FindName("InsertPlantaButton")).IsEnabled);
            });
        }

        // ---- 5. the matrix, fondo selector and status render inside their slots (recompute wiring intact) ----

        [Fact]
        public void SelectiveWindow_MatrixAndStatus_RenderInsideTheShellSlots()
        {
            StaTestRunner.Run(() =>
            {
                var window = new RackSelectiveWindow(canInsertInAutoCad: true);

                // The ctor runs RenderMatrix()+Recompute(): the matrix grid (in MatrixContent) is populated and the status
                // band (in StatusContent) shows the recomputed summary — proving the pipeline still targets the shell-hosted
                // controls.
                Assert.True(window.MatrixGrid.Children.Count > 0, "the matrix must render into the slot");
                Assert.True(window.MatrixGrid.ColumnDefinitions.Count >= 2, "matrix has a label column + a front column");
                Assert.False(string.IsNullOrWhiteSpace(window.SummaryText.Text), "recompute must fill the status summary");
            });
        }

        [Fact]
        public void SelectiveWindow_MatrixCellClick_ChangesSelection_ThroughTheShellHostedGrid()
        {
            StaTestRunner.Run(() =>
            {
                var window = new RackSelectiveWindow(canInsertInAutoCad: true);
                var cell = FirstSelectableCell(window.MatrixGrid);
                Assert.NotNull(cell);
                // The real SelectCell handler fires on MouseLeftButtonUp (selective's cell wiring) — post-migration it still
                // updates the single-cell selection header. No multiselection: one selected cell + scope application.
                cell.RaiseEvent(new MouseButtonEventArgs(InputManager.Current.PrimaryMouseDevice, 0, MouseButton.Left)
                {
                    RoutedEvent = UIElement.MouseLeftButtonUpEvent,
                    Source = cell
                });
                Assert.Contains("Celda: Frente", window.CellHeader.Text); // the header reflects the selected cell
            });
        }

        [Fact]
        public void SelectiveWindow_ApplyScopeAll_PropagatesThroughTheShellHostedControls()
        {
            StaTestRunner.Run(() =>
            {
                var window = new RackSelectiveWindow(canInsertInAutoCad: true);
                // Select a cell, type a distinctive frente, and apply to ALL cells via the real ApplyAll_Click handler.
                var cell = FirstSelectableCell(window.MatrixGrid);
                cell.RaiseEvent(new MouseButtonEventArgs(InputManager.Current.PrimaryMouseDevice, 0, MouseButton.Left)
                {
                    RoutedEvent = UIElement.MouseLeftButtonUpEvent,
                    Source = cell
                });
                window.FrenteBox.Text = "50";
                EditorWindowTestSupport.ClickByContent(window, "Todas");

                // Every cell label now reads the applied frente ("50×…") — the scope apply ran through the shell-hosted grid.
                var labels = CellBorders(window.MatrixGrid)
                    .Select(b => (b.Child as TextBlock)?.Text ?? string.Empty).ToList();
                Assert.NotEmpty(labels);
                Assert.All(labels, t => Assert.StartsWith("50", t));
            });
        }

        [Fact]
        public void SelectiveWindow_JaggedLevels_RenderInTheShellHostedMatrix()
        {
            StaTestRunner.Run(() =>
            {
                var window = new RackSelectiveWindow(canInsertInAutoCad: true);
                var bayCount = window.MatrixGrid.ColumnDefinitions.Count - 1; // minus the label column
                var cellsBefore = CellBorders(window.MatrixGrid).Count;       // default 2 bays × 4 levels = 8

                // Grow ONE bay through the frente panel (the ± buttons left the matrix in I-43 gate 8A): the matrix
                // becomes JAGGED — that bay has one more level than the rest, and the shorter bay leaves its top
                // display row empty. The grid renders maxLevels rows either way.
                window.EditorState.SelectCell(0, 0, extend: false);
                ((ComboBox)window.FindName("FrontScopeBox")).SelectedIndex = 0; // this frente only
                ((TextBox)window.FindName("FrontLevelsBox")).Text = "5";        // the default matrix has 4 levels
                EditorWindowTestSupport.ClickByContent(window, "Aplicar niveles");

                var cellsAfter = CellBorders(window.MatrixGrid).Count;
                var maxLevels = window.MatrixGrid.RowDefinitions.Count - 1;   // minus the header row
                Assert.Equal(cellsBefore + 1, cellsAfter);                    // exactly one new cell (the grown bay)
                Assert.True(cellsAfter < maxLevels * bayCount,               // strictly fewer than a full rectangle → jagged
                    $"expected jagged ({cellsAfter} cells) below the full {maxLevels}×{bayCount} rectangle");
            });
        }

        [Fact]
        public void SelectiveWindow_FondoSelector_AppearsForMultipleFondos_InTheMatrixSlot()
        {
            StaTestRunner.Run(() =>
            {
                var window = new RackSelectiveWindow(canInsertInAutoCad: true);
                Assert.Equal(Visibility.Collapsed, window.FondoSelectorPanel.Visibility); // single fondo → hidden

                // Two fondos (doble profundidad): the real Fondos_LostFocus rebuilds the per-fondo selector, which lives in the
                // MatrixContent slot.
                window.FondosBox.Text = "2";
                window.FondosBox.RaiseEvent(new RoutedEventArgs(UIElement.LostFocusEvent, window.FondosBox));

                Assert.Equal(Visibility.Visible, window.FondoSelectorPanel.Visibility);
                Assert.Equal(2, window.FondoSelectorBox.Items.Count); // Fondo 1 / Fondo 2, each with its own level matrix
            });
        }

        // ---- 6. previews draw inside the shell slot for both views ----

        [Fact]
        public void SelectiveWindow_PreviewFrontalAndLateral_DrawInsideTheShellSlot()
        {
            StaTestRunner.Run(() =>
            {
                var window = new RackSelectiveWindow(canInsertInAutoCad: true);
                LayoutShell(window, 1300, 760);

                // Frontal (default) draws.
                Assert.True(window.PreviewCanvas.ActualWidth > 0.0, "preview canvas must receive a real width from the shell");
                Assert.True(window.PreviewCanvas.Children.Count > 0, "frontal preview must draw once it has a size");

                // Switch to Lateral via the real radio handler: the same canvas redraws the lateral view.
                window.PreviewLateralRadio.IsChecked = true;
                window.Shell.UpdateLayout();
                Assert.True(window.PreviewCanvas.Children.Count > 0, "lateral preview must draw after switching view");
                Assert.Contains("Vista lateral", window.PreviewHint.Text); // the hint reflects the lateral view
            });
        }

        // ---- 7. inserts / update through the REAL handlers, now hosted in the shell's action slots ----

        [Fact]
        public void SelectiveWindow_InsertFrontal_ViaShellHostedButton_MintsGuid_AndRequestsFrontal()
        {
            var (requested, view, updateOnly, id, name, requestType, payload) = StaTestRunner.Run(() =>
            {
                var window = new RackSelectiveWindow(canInsertInAutoCad: true);
                window.LoadForNew(SelectivePalletDesignDocument.From(MinimalDesign(), "GUID-TEMPLATE", "Plantilla"));
                EditorWindowTestSupport.SetText(window, "NameBox", "Selectivo nuevo");
                EditorWindowTestSupport.ClickByContent(window, "Insertar frontal"); // the CTA in PrimaryActions
                return Capture(window);
            });

            Assert.True(requested);
            Assert.Equal("frontal", view);
            Assert.False(updateOnly);
            Assert.True(Guid.TryParse(id, out _)); // fresh GUID minted (template opened as new)
            Assert.Equal("Selectivo nuevo", name);
            Assert.Equal(nameof(SelectiveInsertionRequest), requestType);
            Assert.True(payload);
        }

        [Fact]
        public void SelectiveWindow_Update_ViaShellHostedButton_KeepsGuidAndName_RedrawsInPlace()
        {
            var (requested, view, updateOnly, id, name, requestType, payload) = StaTestRunner.Run(() =>
            {
                var window = new RackSelectiveWindow(canInsertInAutoCad: true);
                window.LoadExisting(SelectivePalletDesignDocument.From(MinimalDesign(), "GUID-SEL", "Selectivo A"));
                EditorWindowTestSupport.ClickNamed(window, "UpdateButton"); // now in the LeadingActions slot
                return Capture(window);
            });

            Assert.True(requested);
            Assert.True(updateOnly);
            Assert.Null(view);
            Assert.Equal("GUID-SEL", id);
            Assert.Equal("Selectivo A", name);
            Assert.Equal(nameof(SelectiveInsertionRequest), requestType);
            Assert.True(payload);
        }

        [Fact]
        public void SelectiveWindow_InsertPlanta_ViaShellHostedButton_KeepsGuidAndName_LinkedView()
        {
            // The ONE real-handler gap not covered by SelectiveEditorWindowTests (frontal/lateral/update): the PLANTA view.
            var (requested, view, updateOnly, id, name, requestType, payload, resolves) = StaTestRunner.Run(() =>
            {
                var window = new RackSelectiveWindow(canInsertInAutoCad: true);
                window.LoadExisting(SelectivePalletDesignDocument.From(MinimalDesign(), "GUID-SEL", "Selectivo A"));
                EditorWindowTestSupport.ClickNamed(window, "InsertPlantaButton"); // now in the PrimaryActions slot
                var cap = Capture(window);
                // Coherence: the payload's design round-trips to a system with the same height as the payload's system.
                var design = window.DesignToInsert;
                var system = window.SystemToInsert;
                var resolved = design != null && system != null
                    && Math.Abs(new SelectiveGeometryResolver().Resolve(design, Catalog).Height - system.Height) < 1e-6;
                return (cap.Requested, cap.View, cap.UpdateOnly, cap.Id, cap.Name, cap.RequestType, cap.Payload, resolved);
            });

            Assert.True(requested);
            Assert.Equal("planta", view);
            Assert.False(updateOnly);
            Assert.Equal("GUID-SEL", id);   // linked planta view keeps the rack identity
            Assert.Equal("Selectivo A", name);
            Assert.Equal(nameof(SelectiveInsertionRequest), requestType);
            Assert.True(payload);            // System + Design both present
            Assert.True(resolves);           // the planta payload's design resolves to the same system
        }

        // ---- 8. the shell lays the window out: sidebar scroll, preview draw, resize ----

        [Fact]
        public void SelectiveWindow_ShellLaysOut_SidebarScrolls_PreviewDraws_AndResizes()
        {
            StaTestRunner.Run(() =>
            {
                var window = new RackSelectiveWindow(canInsertInAutoCad: true);
                var shell = window.Shell;
                LayoutShell(window, 1300, 700);

                // Sidebar scroll: the selective side panel is wrapped in the shell's vertical-only scroll (no scroll of its own).
                var scroll = shell.SidebarScroll;
                Assert.NotNull(scroll);
                Assert.Equal(ScrollBarVisibility.Auto, scroll.VerticalScrollBarVisibility);
                Assert.Equal(ScrollBarVisibility.Disabled, scroll.HorizontalScrollBarVisibility);

                // The slots host exactly the window's content.
                Assert.Same(shell.MatrixContent, shell.MatrixHost.Content);
                Assert.Same(shell.PreviewContent, shell.PreviewHost.Content);
                Assert.Same(shell.StatusContent, shell.StatusHost.Content);

                // Preview draws when the canvas gets a real size, and stays drawn across a resize.
                Assert.True(window.PreviewCanvas.ActualWidth > 0.0, "preview canvas must receive a real width from the shell");
                Assert.True(window.PreviewCanvas.Children.Count > 0, "preview must draw once it has a size");
                shell.Measure(new Size(1500, 900));
                shell.Arrange(new Rect(0, 0, 1500, 900));
                shell.UpdateLayout();
                Assert.True(window.PreviewCanvas.ActualWidth > 0.0);
                Assert.True(window.PreviewCanvas.Children.Count > 0);
            });
        }

        // ---- the REAL window SHOWN at its minimum fits every zone in the client without overlap or preview clip ----

        [Fact]
        public void SelectiveWindow_ShownAtMinimum_FitsEveryZoneInClient_NoOverlapNoClip()
        {
            StaTestRunner.Run(() =>
            {
                var window = new RackSelectiveWindow(canInsertInAutoCad: true);
                window.WindowStartupLocation = WindowStartupLocation.Manual;
                window.Left = -10000; window.Top = -10000; window.ShowInTaskbar = false;
                window.Width = window.MinWidth;   // show at the contractual minimum (outer), from the shared tokens
                window.Height = window.MinHeight;
                try
                {
                    window.Show();
                    window.UpdateLayout();
                    var shell = window.Shell;
                    var clientW = shell.ActualWidth;
                    var clientH = shell.ActualHeight;
                    Assert.True(clientW > 0 && clientH > 0, "the shown window must have a real client area");
                    Assert.True(window.PreviewCanvas.ActualWidth > 0 && window.PreviewCanvas.ActualHeight > 0
                        && window.PreviewCanvas.Children.Count > 0, "the preview must draw at the shown minimum");

                    Rect BoundsIn(FrameworkElement fe)
                    {
                        var tl = fe.TransformToAncestor(shell).Transform(new Point(0, 0));
                        return new Rect(tl, new Size(fe.ActualWidth, fe.ActualHeight));
                    }
                    var preview = BoundsIn(window.PreviewCanvas);
                    var status = BoundsIn(shell.StatusHost);
                    var bar = BoundsIn(shell.ActionBar);
                    var sidebar = BoundsIn(shell.SidebarScroll);
                    var matrix = BoundsIn(shell.MatrixHost);

                    // (a) every zone is INSIDE the client (nothing cut off by the window edge).
                    foreach (var (zone, r) in new[] { ("preview", preview), ("status", status), ("action bar", bar),
                        ("sidebar", sidebar), ("matrix", matrix) })
                    {
                        Assert.True(r.Left >= -0.5 && r.Top >= -0.5 && r.Right <= clientW + 0.5 && r.Bottom <= clientH + 0.5,
                            $"{zone} {r} falls outside the client {clientW:0}x{clientH:0}");
                    }

                    // (b) preview does NOT overlap the status band, and status sits above the action bar — in LAYOUT coordinates
                    //     (ignore ClipToBounds), so the minimum genuinely fits WITHOUT relying on the clip defense.
                    Assert.True(preview.Bottom <= status.Top + 0.5,
                        $"preview (bottom {preview.Bottom:0}) overlaps the status band (top {status.Top:0}) at the shown minimum");
                    Assert.True(status.Bottom <= bar.Top + 0.5,
                        $"status (bottom {status.Bottom:0}) overlaps the action bar (top {bar.Top:0})");
                }
                finally
                {
                    window.Close();
                }
            });
        }

        // ---- the REAL window consumes the shared size contract, and nothing clips at the contractual minimum ----

        [Fact]
        public void SelectiveWindow_ConsumesTheSharedSizeContract_FromTokens()
        {
            var (minW, minH, initW, initH, wMinW, wMinH, wW, wH, hasStyle) = StaTestRunner.Run(() =>
            {
                var tokens = AppStyles();
                var window = new RackSelectiveWindow(canInsertInAutoCad: true);
                return ((double)tokens["ShellMinWidth"], (double)tokens["ShellMinHeight"],
                        (double)tokens["ShellInitialWidth"], (double)tokens["ShellInitialHeight"],
                        window.MinWidth, window.MinHeight, window.Width, window.Height,
                        window.Style != null);
            });

            Assert.True(hasStyle);        // styled by the shared EditorShellWindowStyle, not hardcoded
            Assert.Equal(minW, wMinW);    // minimum from ShellMinWidth token (no more MinWidth="1060")
            Assert.Equal(minH, wMinH);    // ShellMinHeight token (no more MinHeight="600")
            Assert.Equal(initW, wW);      // initial size from ShellInitialWidth token (no more Width="1300")
            Assert.Equal(initH, wH);      // ShellInitialHeight token (no more Height="740")
        }

        [Fact]
        public void SelectiveWindow_AtContractualMinimumSize_NothingIsClipped()
        {
            StaTestRunner.Run(() =>
            {
                var tokens = AppStyles();
                var minW = (double)tokens["ShellMinWidth"];
                var minH = (double)tokens["ShellMinHeight"];
                var sidebarWidth = (double)tokens["ShellSidebarWidth"];

                var window = new RackSelectiveWindow(canInsertInAutoCad: true);
                var shell = window.Shell;
                var generic = new ResourceDictionary { Source = new Uri("/RackCad.UI;component/Themes/Generic.xaml", UriKind.Relative) };
                shell.Style = (Style)generic[typeof(RackEditorVisualShell)];
                shell.Measure(new Size(minW, minH));
                shell.Arrange(new Rect(0, 0, minW, minH));
                shell.UpdateLayout();

                // Sidebar: vertical scroll ENGAGES at the minimum height; its viewport is bounded to the sidebar token width
                // (minus the scrollbar), horizontal scrolling disabled (content wraps within that width).
                var scroll = shell.SidebarScroll;
                Assert.NotNull(scroll);
                Assert.Equal(ScrollBarVisibility.Auto, scroll.VerticalScrollBarVisibility);
                Assert.Equal(ScrollBarVisibility.Disabled, scroll.HorizontalScrollBarVisibility);
                Assert.True(scroll.ScrollableHeight > 0.0, "the sidebar must scroll at the minimum height, not clip its content");
                Assert.InRange(scroll.ViewportWidth, sidebarWidth - 30.0, sidebarWidth + 0.5);

                // Matrix, preview and status each have real area inside the shell.
                Assert.True(shell.MatrixHost.ActualWidth > 0.0 && shell.MatrixHost.ActualHeight > 0.0, "matrix clipped");
                Assert.True(shell.PreviewHost.ActualWidth > 0.0 && shell.PreviewHost.ActualHeight > 0.0, "preview clipped");
                Assert.True(shell.StatusHost.ActualWidth > 0.0 && shell.StatusHost.ActualHeight > 0.0, "status clipped");
                Assert.True(window.PreviewCanvas.ActualWidth > 0.0 && window.PreviewCanvas.Children.Count > 0, "preview must draw at the minimum size");

                // The preview must not overlap the status band, and the work area is clip-bounded (defense for a shown client
                // tighter than the token).
                var previewBottom = window.PreviewCanvas.TransformToAncestor(shell).Transform(new Point(0, window.PreviewCanvas.ActualHeight)).Y;
                var statusTop = shell.StatusHost.TransformToAncestor(shell).Transform(new Point(0, 0)).Y;
                Assert.True(previewBottom <= statusTop + 0.5, $"preview (bottom {previewBottom:0}) overlaps the status band (top {statusTop:0})");
                Assert.True(shell.WorkArea.ClipToBounds, "the work area must clip so the preview stays inside its zone at tight client heights");

                // The FULL action bar: all seven actions are laid out (the WrapPanel wraps at the minimum, never clips) and every
                // button stays within the bar's width.
                var bar = shell.ActionBar;
                Assert.NotNull(bar);
                var buttons = VisualDescendants(bar).OfType<Button>().ToList();
                Assert.True(buttons.Count >= 7, $"the whole action bar must be present; got {buttons.Count} buttons");
                Assert.All(buttons, b => Assert.True(b.ActualWidth > 0.0 && b.ActualHeight > 0.0, $"action '{b.Content}' is clipped"));
                Assert.True(bar.ActualWidth <= shell.ActualWidth + 0.5, "the action bar overflows the shell width");
                foreach (var b in buttons)
                {
                    var topLeft = b.TransformToAncestor(bar).Transform(new Point(0, 0));
                    Assert.True(topLeft.X + b.ActualWidth <= bar.ActualWidth + 0.5, $"action '{b.Content}' extends past the bar");
                }
            });
        }

        // ---- 9. the dynamic editor is not regressed by the selective migration ----

        [Fact]
        public void DynamicWindow_StillComposedOverShell_NoRegression()
        {
            var stillShell = StaTestRunner.Run(() =>
            {
                var window = new RackDynamicSystemWindow(canInsertInAutoCad: true);
                return window.Content is RackEditorVisualShell && ReferenceEquals(window.Content, window.Shell);
            });
            Assert.True(stillShell); // I-31 touches only the selective XAML; the dynamic editor stays on the shell (I-30)
        }

        // ---- helpers ----

        private static ResourceDictionary AppStyles()
            => new ResourceDictionary { Source = new Uri("/RackCad.UI;component/Themes/AppStyles.xaml", UriKind.Relative) };

        /// <summary>Give the shell its REAL default template (Themes/Generic.xaml) and lay it out, as a shown window would.
        /// (A control disconnected from a shown HWND does not auto-resolve its theme style.)</summary>
        private static void LayoutShell(RackSelectiveWindow window, double w, double h)
        {
            var shell = window.Shell;
            var generic = new ResourceDictionary { Source = new Uri("/RackCad.UI;component/Themes/Generic.xaml", UriKind.Relative) };
            shell.Style = (Style)generic[typeof(RackEditorVisualShell)];
            shell.Measure(new Size(w, h));
            shell.Arrange(new Rect(0, 0, w, h));
            shell.UpdateLayout();
        }

        private static IEnumerable<DependencyObject> VisualDescendants(DependencyObject root)
        {
            var count = VisualTreeHelper.GetChildrenCount(root);
            for (var i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                yield return child;
                foreach (var nested in VisualDescendants(child)) yield return nested;
            }
        }

        /// <summary>Selective matrix cells are the Borders at row≥1, col≥1 (row 0 = bay headers, col 0 = level labels).</summary>
        private static IReadOnlyList<Border> CellBorders(Grid grid)
            => grid.Children.OfType<Border>().Where(b => Grid.GetRow(b) >= 1 && Grid.GetColumn(b) >= 1).ToList();

        private static Border FirstSelectableCell(Grid grid) => CellBorders(grid).FirstOrDefault();

        private static (bool Requested, string View, bool UpdateOnly, string Id, string Name, string RequestType, bool Payload) Capture(RackSelectiveWindow window)
        {
            var system = window.SystemToInsert;
            var design = window.DesignToInsert;
            return (window.InsertRequested, window.InsertView, window.UpdateOnly, window.RackId, window.RackName,
                window.Session.InsertionRequest?.GetType().Name, system != null && design != null);
        }

        private static void AssertInSlot(object slotContent, RackSelectiveWindow window, string name, string slotName)
        {
            var target = window.FindName(name) as DependencyObject;
            Assert.True(target != null, $"named control {name} not found");
            Assert.True(slotContent is DependencyObject, $"slot {slotName} has no content");
            Assert.True(IsInLogicalSubtree((DependencyObject)slotContent, target), $"{name} is not hosted inside the {slotName} slot");
        }

        private static void AssertContentButton(object slotContent, string content)
        {
            Assert.True(slotContent is DependencyObject, "action slot has no content");
            Assert.True(FindButtonByContent((DependencyObject)slotContent, content) != null, $"no button '{content}' in the action slot");
        }

        private static bool IsInLogicalSubtree(DependencyObject root, DependencyObject target)
        {
            if (ReferenceEquals(root, target)) return true;
            foreach (var child in LogicalTreeHelper.GetChildren(root))
            {
                if (child is DependencyObject node && IsInLogicalSubtree(node, target)) return true;
            }
            return false;
        }

        private static Button FindButtonByContent(DependencyObject root, string content)
        {
            if (root is Button b && (b.Content as string) == content) return b;
            foreach (var child in LogicalTreeHelper.GetChildren(root))
            {
                if (child is DependencyObject node)
                {
                    var found = FindButtonByContent(node, content);
                    if (found != null) return found;
                }
            }
            return null;
        }

        /// <summary>A minimal but valid single-fondo selective design (one bay, two levels, floor beam). Mirrors
        /// SelectiveEditorWindowTests so LoadExisting/LoadForNew build a real payload.</summary>
        private static SelectivePalletDesign MinimalDesign()
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
                DrawBasePlate = true
            };

            var bay = new SelectiveBayDesign { FloorBeam = true };
            for (var level = 0; level < 2; level++)
            {
                bay.Levels.Add(new SelectiveCell
                {
                    Pallet = new Tarima { Frente = 42.0, Alto = 60.0 },
                    PalletCount = 2,
                    BeamId = BeamId,
                    BeamPeralte = 4.0
                });
            }

            design.Bays.Add(bay);
            return design;
        }
    }
}
