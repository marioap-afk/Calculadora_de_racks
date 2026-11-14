using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using RackCad.UI.Shell;
using RackCad.UI.Systems.PushBack;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// F4-T1B (I-64) — CARACTERIZACION de Enter, del foco inicial y de la tabulacion de la ventana CLASICA de Push
    /// Back (<see cref="RackPushBackSystemWindow"/>). CT-01, CT-10 y CT-11 del paquete v5.
    ///
    /// <para><b>Fija lo que HAY, no lo que ADR-0029 D7/D9 querrian</b> (D13): hoy ningun boton es <c>IsDefault</c>, el
    /// foco inicial se DECLARA en el XAML y no se fuerza desde el codigo, y nadie fija <c>TabIndex</c>. Esas rarezas
    /// quedan registradas como deuda (D-T1B-06) y NO se corrigen aqui.</para>
    ///
    /// <para>El foco y la tabulacion EFECTIVOS no entran en las pruebas: dependen de la activacion de la ventana y
    /// del host. Se fija lo declarado: la expresion de enlace del foco y el orden de las paradas de tabulacion DENTRO
    /// de cada zona del shell, mas el orden de las zonas en la plantilla aplicada.</para>
    /// </summary>
    [Collection(EditorDiscardPromptCollection.Name)]
    public sealed class PushBackClassicCharacterizationKeyboardTests
    {
        private const string EscapeKey = "\x1B";
        private const string EnterKey = "\r";

        private static RackPushBackSystemWindow Shown()
        {
            var window = new RackPushBackSystemWindow(canInsertInAutoCad: true)
            {
                WindowStartupLocation = WindowStartupLocation.Manual,
                ShowInTaskbar = false,
                ShowActivated = false,
                Left = -10000,
                Top = -10000,
            };
            window.Show();
            window.UpdateLayout();
            return window;
        }

        // ================================================================================================== CT-01

        [Fact]
        public void EnterOnTheAccessKeyRouteDoesNothingBecauseNoButtonIsDefault()
        {
            StaTestRunner.Run(() =>
            {
                var asked = 0;
                using (EditorDiscardPrompt.Substitute((question, owner) => { asked++; return true; }))
                {
                    var window = Shown();
                    var closed = false;
                    window.Closed += (s, e) => closed = true;
                    try
                    {
                        var source = PresentationSource.FromVisual(window);
                        Assert.NotNull(source);
                        Assert.True(window.IsVisible);

                        // INCUMPLIMIENTO CARACTERIZADO (ADR-0029 D7): Enter no activa ninguna accion por defecto, porque
                        // ningun boton es IsDefault, ni siquiera entre lo que genera la matriz.
                        Assert.DoesNotContain(EditorWindowTestSupport.FindAll<Button>(window), button => button.IsDefault);

                        var passes = window.RecomputePassesForTest;
                        AccessKeyManager.ProcessKey(source, EnterKey, false);

                        Assert.False(closed);
                        Assert.True(window.IsVisible);
                        Assert.Equal(0, asked);
                        Assert.Equal(passes, window.RecomputePassesForTest);
                        Assert.False(window.InsertRequested);
                        Assert.Null(window.InsertionRequest);

                        // Sonda privada de la prueba (ancla): por esta MISMA ruta, «\r» SI llega a un boton IsDefault.
                        var clicks = 0;
                        var probeButton = new Button { Content = "Sonda", IsDefault = true };
                        probeButton.Click += (s, e) => clicks++;
                        var probeWindow = new Window
                        {
                            Content = probeButton,
                            WindowStartupLocation = WindowStartupLocation.Manual,
                            ShowInTaskbar = false,
                            ShowActivated = false,
                            Left = -10000,
                            Top = -10000,
                            Width = 120,
                            Height = 120,
                        };
                        probeWindow.Show();
                        try
                        {
                            var probeSource = PresentationSource.FromVisual(probeWindow);
                            Assert.NotNull(probeSource);
                            AccessKeyManager.ProcessKey(probeSource, EnterKey, false);
                            Assert.Equal(1, clicks);
                        }
                        finally { probeWindow.Close(); }

                        // CONTROL POSITIVO de la ruta (medido sobre la base): sobre el MISMO PresentationSource, Escape
                        // llega a un boton IsCancel y, sin trabajo pendiente, cierra la ventana. Asi, que Enter no haga
                        // nada es la ausencia de IsDefault y no un fallo de la ruta.
                        AccessKeyManager.ProcessKey(source, EscapeKey, false);
                        Assert.True(closed);
                    }
                    finally
                    {
                        if (window.IsVisible)
                        {
                            window.Close();
                        }
                    }
                }
            });
        }

        // ================================================================================================== CT-10

        [Fact]
        public void InitialFocusIsDeclaredOnTheNameBoxAndNotForcedFromCode()
        {
            StaTestRunner.Run(() =>
            {
                var window = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                try
                {
                    var expression = BindingOperations.GetBindingExpression(window, FocusManager.FocusedElementProperty);
                    Assert.NotNull(expression);
                    Assert.Equal("NameBox", expression.ParentBinding.ElementName);

                    // Ancla: NameBox es un TextBox (un control con manejadores) y no una accion.
                    Assert.IsType<TextBox>(window.FindName("NameBox"));
                }
                finally { window.Close(); }
            });
        }

        [Fact]
        public void NameBoxTextIsEmptyOnLoadNewAndTheStoredNameOnLoadExisting()
        {
            StaTestRunner.Run(() =>
            {
                var window = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                try
                {
                    var nameBox = (TextBox)window.FindName("NameBox");
                    Assert.Equal(string.Empty, nameBox.Text);

                    window.LoadExisting(window.LastComputation.Design, "22222222-2222-2222-2222-222222222222", "Nombre guardado");

                    Assert.Equal("Nombre guardado", nameBox.Text);
                }
                finally { window.Close(); }
            });
        }

        // ================================================================================================== CT-11

        /// <summary>El orden de las paradas de tabulacion con nombre DENTRO de una zona, en el orden logico de la zona.</summary>
        private static IReadOnlyList<string> Stops(object slot)
        {
            var root = slot as DependencyObject;
            if (root == null)
            {
                return new string[0];
            }

            return EditorWindowTestSupport.Descendants(root)
                .OfType<Control>()
                .Where(control => !string.IsNullOrEmpty(control.Name) && control.IsTabStop && control.Focusable)
                .Select(control => control.Name)
                .ToList();
        }

        private static readonly string[] SideStops =
        {
            "NameBox", "NumberFrontsCheck", "NumberLevelsCheck", "DrawRackNameCheck", "AnnotationScaleBox", "DimensionsBox",
            "DimensionsFrontalCheck", "DimensionsLateralCheck", "DimensionsPlantaCheck", "FrontCountBox", "ToleranceBox",
            "BeamDepthBox", "PalletsDeepBox", "PostPeralteBox", "PostBox", "SideBPresentCheck", "SideSelectorBox", "GapBox",
            "CentralSeparatorCheck", "CellTopologyBox", "RunDirectionBox", "TopologyScopeBox", "ApplyTopologyButton",
            "StructureOverrideBox", "ApplyStructureButton", "RestoreStructureButton", "FrontBox", "PalletHeightBox",
            "WeightUnitBox", "DepthBox", "WeightBox", "PositionsBox", "FondosBox", "FirstLevelHeightBox", "LevelsBox",
            "DepthStartBox", "ApplyFrontDataButton", "ApplySelectedFrontDataButton", "ApplyAllFrontDataButton",
            "CellPalletFrontBox", "CellPalletWeightBox", "CellClearBox", "CellPalletHeightBox", "CellBeamLengthOverrideBox",
            "CellInOutBeamBox", "CellIntermediateBeamBox", "CellInOutPeralteBox", "RearPeralteBox",
            "CellIntermediatePeralteBox", "ApplyCellButton", "ApplySelectedButton", "ApplyLevelButton", "ApplyFrontButton",
            "ApplyAllButton", "CellFondoOverrideBox", "CellDrawPalletCheck", "CellPropertyScopeBox", "ApplyCellFondoButton",
            "RestoreCellFondoButton", "ApplyCellPalletButton", "RestoreCellPalletButton", "SafetyButton",
            "AdvancedModulesToggle", "RackHeaderHeightBox", "DerivedPostHeightBox", "DerivedPostReinforcedCheck",
            "DerivedPostReinforcementHeightBox", "RackSeparatorCountBox", "RackSeparatorSpacingBox",
            "RestoreRackParametersButton", "ModuleBox", "ModuleLengthBox", "ModuleCalculatedRadio", "ModuleCustomRadio",
            "ConfigureModuleHeaderButton", "CopyHeaderFromBox", "CopyHeaderFromButton", "HeaderTargetsThisButton",
            "HeaderTargetsAllButton", "HeaderLinesThisButton", "HeaderLinesAllButton", "ApplyHeaderSelectionButton",
            "DerivedPostLineHeightBox", "ApplyDerivedPostLinesButton", "ConfirmModuleButton", "CancelModuleButton",
            "RestoreModuleButton", "RestoreAllModulesButton",
        };

        private static bool IsAncestorOrSelf(DependencyObject ancestor, DependencyObject node)
        {
            for (var current = node; current != null; current = VisualTreeHelper.GetParent(current))
            {
                if (ReferenceEquals(current, ancestor))
                {
                    return true;
                }
            }

            return false;
        }

        [Fact]
        public void TabStopsKeepTheirOrderInsideEveryShellZoneAndNoLocalNavigationValueIsSet()
        {
            StaTestRunner.Run(() =>
            {
                var window = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                try
                {
                    var shell = (RackEditorVisualShell)window.FindName("Shell");
                    Assert.NotNull(shell);

                    // (1) El orden de las paradas DENTRO de cada zona, independiente de la composicion de archivos.
                    Assert.Equal(SideStops, Stops(shell.SidePanelContent));
                    Assert.Equal(new[] { "AddFrontButton", "RemoveFrontButton", "AddLevelButton", "RemoveLevelButton", "SelectedFrontBox", "SelectedLevelBox" }, Stops(shell.MatrixContent));
                    Assert.Equal(new[] { "ViewBox", "LateralSectionBox" }, Stops(shell.PreviewContent));
                    Assert.Empty(Stops(shell.StatusContent));
                    Assert.Equal(new[] { "RestoreButton" }, Stops(shell.LeadingActions));
                    Assert.Equal(new[] { "BomButton", "SaveLibraryButton" }, Stops(shell.SecondaryActions));
                    Assert.Equal(
                        new[] { "UpdateButton", "InsertLateralButton", "InsertFrontalEntradaButton", "InsertFrontalPosteriorButton", "FrontalSideBox", "InsertPlantaButton", "InsertBatchButton", "InsertButton" },
                        Stops(shell.PrimaryActions));
                    Assert.Equal(new[] { "CloseButton" }, Stops(shell.TrailingActions));

                    // (2) ANCLA (vive en Themes/Generic.xaml, fuera del alcance de los mutantes): el orden de las zonas en
                    // el DockPanel de la plantilla aplicada. Se aplica sin mostrar la ventana.
                    shell.ApplyTemplate();
                    var actionBar = (DependencyObject)shell.Template.FindName("PART_ActionBar", shell);
                    var statusHost = (DependencyObject)shell.Template.FindName("PART_StatusHost", shell);
                    var sidebarScroll = (DependencyObject)shell.Template.FindName("PART_SidebarScroll", shell);
                    var workArea = (DependencyObject)shell.Template.FindName("PART_WorkArea", shell);
                    Assert.NotNull(actionBar);
                    Assert.NotNull(statusHost);
                    Assert.NotNull(sidebarScroll);
                    Assert.NotNull(workArea);
                    var dock = Assert.IsType<DockPanel>(VisualTreeHelper.GetParent(actionBar));
                    var zones = dock.Children.Cast<UIElement>().ToList();
                    Assert.Equal(0, zones.FindIndex(zone => ReferenceEquals(zone, actionBar)));
                    Assert.Equal(1, zones.FindIndex(zone => ReferenceEquals(zone, statusHost)));
                    Assert.Equal(2, zones.FindIndex(zone => IsAncestorOrSelf(zone, sidebarScroll)));
                    Assert.Equal(3, zones.FindIndex(zone => ReferenceEquals(zone, workArea)));

                    // (3) ANCLA: nadie fija un valor LOCAL de TabIndex, IsTabStop ni KeyboardNavigation.
                    var local = new List<string>();
                    var navigation = new[]
                    {
                        KeyboardNavigation.TabIndexProperty, Control.IsTabStopProperty, KeyboardNavigation.TabNavigationProperty,
                        KeyboardNavigation.ControlTabNavigationProperty, KeyboardNavigation.DirectionalNavigationProperty,
                    };
                    foreach (var node in EditorWindowTestSupport.Descendants(window))
                    {
                        foreach (var property in navigation)
                        {
                            if (node.ReadLocalValue(property) != DependencyProperty.UnsetValue)
                            {
                                local.Add(node.GetType().Name + "#" + (node as FrameworkElement)?.Name + ":" + property.Name);
                            }
                        }
                    }

                    Assert.Empty(local);
                }
                finally { window.Close(); }
            });
        }
    }
}
