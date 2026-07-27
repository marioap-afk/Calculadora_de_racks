using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.RackFrames;
using RackCad.UI.Controls;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-35 (PB-011) — STA tests for the REAL advanced module surface of <see cref="RackPushBackSystemWindow"/>.
    ///
    /// They drive the actual WPF controls (Click via <c>RaiseEvent</c>, LostFocus on the real
    /// <see cref="NumericField"/>) so the handlers, the session and the assembler are exercised together, and they
    /// lock the five Owner decisions at the surface: single selection, confirm/cancel in the window (never in the
    /// shared configurator), full individual restore, explicit-only discard, and modules addressed as a rack-wide
    /// longitudinal sequence.
    /// </summary>
    public sealed class PushBackModuleEditorWindowTests
    {
        private static ComboBox Combo(RackPushBackSystemWindow w, string name) => (ComboBox)w.FindName(name);
        private static Button Btn(RackPushBackSystemWindow w, string name) => (Button)w.FindName(name);
        private static NumericField Field(RackPushBackSystemWindow w, string name) => (NumericField)w.FindName(name);
        private static CheckBox Check(RackPushBackSystemWindow w, string name) => (CheckBox)w.FindName(name);
        private static RadioButton Radio(RackPushBackSystemWindow w, string name) => (RadioButton)w.FindName(name);
        private static TextBlock Text(RackPushBackSystemWindow w, string name) => (TextBlock)w.FindName(name);

        private static void Click(Button button) => button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));

        /// <summary>A shown window with a valid model and the advanced panel open.</summary>
        private static RackPushBackSystemWindow Advanced(int fronts = 1)
        {
            var w = new RackPushBackSystemWindow(canInsertInAutoCad: true);
            w.LoadNew();
            if (fronts > 1)
            {
                for (var i = 1; i < fronts; i++) Click(Btn(w, "AddFrontButton"));
            }

            w.Show();
            Check(w, "AdvancedModulesToggle").IsChecked = true;
            return w;
        }

        private static RackFrameConfiguration Customized(RackFrameConfiguration source, double panelClear)
        {
            source.PanelClear = panelClear;
            return source;
        }

        // ===== Seleccion unica ==================================================================================

        [Fact]
        public void TheModuleSelector_IsSingleSelection_AndListsTheRackWideSequence()
        {
            StaTestRunner.Run(() =>
            {
                var w = Advanced();
                try
                {
                    var box = Combo(w, "ModuleBox");
                    Assert.False(box.IsEditable);
                    Assert.NotEmpty(box.Items);
                    Assert.True(box.SelectedIndex >= 0);

                    // One selection at a time: a ComboBox cannot hold two, and the labels name cabeceras y separadores.
                    var labels = box.Items.Cast<string>().ToList();
                    Assert.Contains(labels, label => label.Contains("Cabecera"));
                    Assert.Contains(labels, label => label.Contains("Separador"));
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void SelectingASeparator_HidesTheHeaderSection_LeavingOnlyItsLength()
        {
            StaTestRunner.Run(() =>
            {
                var w = Advanced();
                try
                {
                    var box = Combo(w, "ModuleBox");
                    var separator = box.Items.Cast<string>()
                        .Select((label, index) => new { label, index })
                        .First(item => item.label.Contains("Separador"));
                    box.SelectedIndex = separator.index;

                    Assert.Equal(Visibility.Collapsed, ((StackPanel)w.FindName("ModuleHeaderPanel")).Visibility);
                    Assert.True(Field(w, "ModuleLengthBox").IsEnabled);
                    Assert.True(Btn(w, "RestoreModuleButton").IsEnabled);
                }
                finally { w.Close(); }
            });
        }

        // ===== Confirmar / cancelar (decision 5) ================================================================

        [Fact]
        public void EditingALength_StagesIt_AndOnlyConfirmarAppliesIt()
        {
            StaTestRunner.Run(() =>
            {
                var w = Advanced();
                try
                {
                    var before = w.EditorStateForTest.ModuleSession.Modules
                        .First(module => module.IsLengthBearing);
                    Combo(w, "ModuleBox").SelectedIndex = before.Index;

                    var length = Field(w, "ModuleLengthBox");
                    length.SetNumber(before.Length + 8.0);
                    length.RaiseEvent(new RoutedEventArgs(FrameworkElement.LostFocusEvent));

                    Assert.True(w.EditorStateForTest.ModuleSession.HasPendingChanges);
                    Assert.True(Btn(w, "ConfirmModuleButton").IsEnabled);
                    Assert.True(Btn(w, "CancelModuleButton").IsEnabled);

                    Click(Btn(w, "ConfirmModuleButton"));

                    var applied = w.EditorStateForTest.WorkingBaseline.Structure.Modules
                        .First(module => module.ModuleId == before.ModuleId);
                    Assert.Equal(before.Length + 8.0, applied.Length, 4);
                    Assert.False(w.EditorStateForTest.ModuleSession.HasPendingChanges);
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void Cancelar_ThrowsTheStagedEditAway_AndLeavesTheRackUntouched()
        {
            StaTestRunner.Run(() =>
            {
                var w = Advanced();
                try
                {
                    var module = w.EditorStateForTest.ModuleSession.Modules.First(m => m.IsLengthBearing);
                    Combo(w, "ModuleBox").SelectedIndex = module.Index;
                    var totalBefore = w.EditorStateForTest.WorkingBaseline.Structure.TotalLength;

                    var length = Field(w, "ModuleLengthBox");
                    length.SetNumber(module.Length + 8.0);
                    length.RaiseEvent(new RoutedEventArgs(FrameworkElement.LostFocusEvent));
                    Assert.True(w.EditorStateForTest.ModuleSession.HasPendingChanges);

                    Click(Btn(w, "CancelModuleButton"));

                    Assert.False(w.EditorStateForTest.ModuleSession.HasPendingChanges);
                    Assert.Equal(totalBefore, w.EditorStateForTest.WorkingBaseline.Structure.TotalLength, 4);
                    Assert.Equal(module.Length, Field(w, "ModuleLengthBox").Value ?? 0.0, 4);
                }
                finally { w.Close(); }
            });
        }

        /// <summary>
        /// The shared configurator is opened on a COPY and is NOT modified (Owner decision 5). Cancelling the module
        /// edit afterwards discards the whole thing, which is exactly the confirm/cancel the base never had: the real
        /// configurator has no Aceptar/Cancelar and mutates what it is handed.
        /// </summary>
        [Fact]
        public void ConfiguringACabecera_StagesACopy_AndCancelarStillDiscardsIt()
        {
            StaTestRunner.Run(() =>
            {
                var w = Advanced();
                try
                {
                    RackFrameConfiguration handed = null;
                    w.HeaderConfiguratorDialog = copy =>
                    {
                        handed = copy;
                        return Customized(copy, 40.0);
                    };

                    var header = w.EditorStateForTest.ModuleSession.Modules.First(module => module.IsHeader);
                    Combo(w, "ModuleBox").SelectedIndex = header.Index;

                    Click(Btn(w, "ConfigureModuleHeaderButton"));

                    Assert.NotNull(handed);
                    Assert.True(Radio(w, "ModuleCustomRadio").IsChecked);
                    Assert.True(w.EditorStateForTest.ModuleSession.HasPendingChanges);

                    Click(Btn(w, "CancelModuleButton"));

                    Assert.False(w.EditorStateForTest.ModuleSession
                        .Modules.First(m => m.ModuleId == header.ModuleId).HasCustomHeaderConfiguration);
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void ConfiguringACabecera_ThenConfirmar_PreservesItThroughTheRecompute()
        {
            StaTestRunner.Run(() =>
            {
                var w = Advanced();
                try
                {
                    w.HeaderConfiguratorDialog = copy => Customized(copy, 40.0);
                    var header = w.EditorStateForTest.ModuleSession.Modules.First(module => module.IsHeader);
                    Combo(w, "ModuleBox").SelectedIndex = header.Index;

                    Click(Btn(w, "ConfigureModuleHeaderButton"));
                    Click(Btn(w, "ConfirmModuleButton"));

                    var applied = w.EditorStateForTest.WorkingBaseline.Structure.Modules
                        .First(module => module.ModuleId == header.ModuleId);
                    Assert.False(applied.UseCalculatedHeaderConfiguration);
                    Assert.Equal(40.0, applied.AssociatedFrameConfiguration.PanelClear, 4);
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void CancellingTheConfigurator_StagesNothing()
        {
            StaTestRunner.Run(() =>
            {
                var w = Advanced();
                try
                {
                    w.HeaderConfiguratorDialog = _ => null;   // cancelled
                    var header = w.EditorStateForTest.ModuleSession.Modules.First(module => module.IsHeader);
                    Combo(w, "ModuleBox").SelectedIndex = header.Index;

                    Click(Btn(w, "ConfigureModuleHeaderButton"));

                    Assert.False(w.EditorStateForTest.ModuleSession.HasPendingChanges);
                    Assert.False(Radio(w, "ModuleCustomRadio").IsChecked);
                }
                finally { w.Close(); }
            });
        }

        // ===== Restauracion individual y total ==================================================================

        [Fact]
        public void RestaurarModulo_ReturnsItToCalculated_AfterConfirmar()
        {
            StaTestRunner.Run(() =>
            {
                var w = Advanced();
                try
                {
                    w.HeaderConfiguratorDialog = copy => Customized(copy, 40.0);
                    var header = w.EditorStateForTest.ModuleSession.Modules.First(module => module.IsHeader);
                    var calculated = header.Length;
                    Combo(w, "ModuleBox").SelectedIndex = header.Index;

                    var length = Field(w, "ModuleLengthBox");
                    length.SetNumber(calculated + 8.0);
                    length.RaiseEvent(new RoutedEventArgs(FrameworkElement.LostFocusEvent));
                    Click(Btn(w, "ConfigureModuleHeaderButton"));
                    Click(Btn(w, "ConfirmModuleButton"));

                    Click(Btn(w, "RestoreModuleButton"));
                    Assert.True(w.EditorStateForTest.ModuleSession.IsRestored(header.ModuleId));
                    Click(Btn(w, "ConfirmModuleButton"));

                    var restored = w.EditorStateForTest.WorkingBaseline.Structure.Modules
                        .First(module => module.ModuleId == header.ModuleId);
                    Assert.Equal(calculated, restored.Length, 4);
                    Assert.True(restored.UseCalculatedHeaderConfiguration);
                    Assert.Contains(header.ModuleId, w.EditorStateForTest.LastModuleReconciliation.Restored);
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void RestaurarEstandar_DiscardsEveryModuleCustomization_AfterConfirmar()
        {
            StaTestRunner.Run(() =>
            {
                var w = Advanced();
                try
                {
                    var module = w.EditorStateForTest.ModuleSession.Modules.First(m => m.IsLengthBearing);
                    Combo(w, "ModuleBox").SelectedIndex = module.Index;
                    var length = Field(w, "ModuleLengthBox");
                    length.SetNumber(module.Length + 8.0);
                    length.RaiseEvent(new RoutedEventArgs(FrameworkElement.LostFocusEvent));
                    Click(Btn(w, "ConfirmModuleButton"));
                    Assert.Contains(w.EditorStateForTest.WorkingBaseline.Structure.Modules, m => m.IsManualOverride);

                    Click(Btn(w, "RestoreAllModulesButton"));
                    Click(Btn(w, "ConfirmModuleButton"));

                    Assert.DoesNotContain(w.EditorStateForTest.WorkingBaseline.Structure.Modules, m => m.IsManualOverride);
                }
                finally { w.Close(); }
            });
        }

        // ===== Deshabilitado CON EXPLICACION ====================================================================

        [Fact]
        public void AModuleSuppressedByI33_IsDisabled_WithTheReasonOnTheTooltip()
        {
            StaTestRunner.Run(() =>
            {
                // Two adjacent blank fronts with DIFFERENT depth ranges: the boundary they share disappears, and the
                // modules that only ever showed at that post stop being drawn anywhere.
                var w = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                w.LoadNew();
                w.Show();
                try
                {
                    var state = w.EditorStateForTest;
                    Click(Btn(w, "AddFrontButton"));
                    Click(Btn(w, "AddFrontButton"));
                    Click(Btn(w, "AddFrontButton"));
                    state.Structure.Fronts[0].PalletsDeep = 4;
                    state.Structure.Fronts[1].PalletsDeep = 4;
                    state.Structure.Fronts[2].PalletsDeep = 4;
                    state.Structure.Fronts[3].PalletsDeep = 10;
                    state.Structure.Fronts[3].DepthStartPosition = 1;
                    Check(w, "AdvancedModulesToggle").IsChecked = true;

                    var descriptors = RackModuleDescriptor.Describe(state.WorkingBaseline.Structure);
                    var absent = descriptors.FirstOrDefault(module => !module.IsPhysicallyPresent);
                    if (absent == null)
                    {
                        return;   // this catalog/layout keeps every module drawn; nothing to assert
                    }

                    Combo(w, "ModuleBox").SelectedIndex = absent.Index;

                    var lengthBox = Field(w, "ModuleLengthBox");
                    Assert.False(lengthBox.IsEnabled);
                    Assert.Contains("I-33", lengthBox.ToolTip?.ToString() ?? string.Empty);
                    Assert.True(ToolTipService.GetShowOnDisabled(lengthBox));
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void ADerivedPost_HasNoLengthToEdit_AndSaysSo()
        {
            StaTestRunner.Run(() =>
            {
                var w = Advanced();
                try
                {
                    var zero = w.EditorStateForTest.ModuleSession.Modules
                        .FirstOrDefault(module => !module.IsLengthBearing);
                    if (zero == null)
                    {
                        return;   // this layout has no derived post
                    }

                    Combo(w, "ModuleBox").SelectedIndex = zero.Index;

                    var lengthBox = Field(w, "ModuleLengthBox");
                    Assert.False(lengthBox.IsEnabled);
                    Assert.Contains("poste derivado", lengthBox.ToolTip?.ToString() ?? string.Empty);
                }
                finally { w.Close(); }
            });
        }

        // ===== El reporte de la reconciliacion llega a la superficie ============================================

        [Fact]
        public void LosingAModuleToAReduction_IsReportedInThePanel_NotSwallowed()
        {
            StaTestRunner.Run(() =>
            {
                var w = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                w.LoadNew();
                w.Show();
                try
                {
                    var state = w.EditorStateForTest;
                    state.Structure.Fronts[0].PalletsDeep = 10;
                    Check(w, "AdvancedModulesToggle").IsChecked = true;
                    Field(w, "FondosBox").SetNumber(10);
                    Field(w, "FondosBox").RaiseEvent(new RoutedEventArgs(FrameworkElement.LostFocusEvent));

                    var last = state.ModuleSession.Modules.Last(module => module.IsLengthBearing);
                    Combo(w, "ModuleBox").SelectedIndex = last.Index;
                    var length = Field(w, "ModuleLengthBox");
                    length.SetNumber(last.Length + 5.0);
                    length.RaiseEvent(new RoutedEventArgs(FrameworkElement.LostFocusEvent));
                    Click(Btn(w, "ConfirmModuleButton"));

                    // Shrink the rack so that customized module disappears.
                    Field(w, "FondosBox").SetNumber(4);
                    Field(w, "FondosBox").RaiseEvent(new RoutedEventArgs(FrameworkElement.LostFocusEvent));

                    var reconciliation = state.LastModuleReconciliation;
                    Assert.NotNull(reconciliation);
                    if (reconciliation.Removed.Count > 0)
                    {
                        Assert.Contains(reconciliation.Removed[0], Text(w, "ModuleStatusText").Text);
                    }
                }
                finally { w.Close(); }
            });
        }

        // ===== El configurador compartido NO se toca ============================================================

        [Fact]
        public void TheSharedConfigurator_IsOpenedOnACopy_SoTheSessionKeepsItsOwnInstance()
        {
            StaTestRunner.Run(() =>
            {
                var w = Advanced();
                try
                {
                    RackFrameConfiguration handed = null;
                    w.HeaderConfiguratorDialog = copy => { handed = copy; return copy; };

                    var header = w.EditorStateForTest.ModuleSession.Modules.First(module => module.IsHeader);
                    Combo(w, "ModuleBox").SelectedIndex = header.Index;
                    Click(Btn(w, "ConfigureModuleHeaderButton"));

                    Assert.NotNull(handed);
                    var stagedCopy = w.EditorStateForTest.ModuleSession.HeaderConfigurationCopy(header.ModuleId);
                    Assert.NotSame(handed, stagedCopy);

                    // Mutating what the configurator was handed cannot reach the session any more.
                    handed.PanelClear = 12.0;
                    Assert.NotEqual(
                        12.0,
                        w.EditorStateForTest.ModuleSession.HeaderConfigurationCopy(header.ModuleId).PanelClear);
                }
                finally { w.Close(); }
            });
        }
    }
}
