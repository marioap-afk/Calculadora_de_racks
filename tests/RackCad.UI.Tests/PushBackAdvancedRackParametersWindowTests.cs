using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using RackCad.UI.Controls;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-35, segunda ronda del Owner — pruebas STA de la sección «Parámetros globales del rack» en la ventana REAL.
    ///
    /// Fijan lo que el Owner pidió: los cuatro ámbitos son del RACK y viven en su propia sección, SEPARADA de
    /// «Módulo seleccionado»; vacío significa el cálculo vigente; el refuerzo apagado deshabilita su altura con el
    /// motivo visible; y la restauración devuelve los cuatro al cálculo/default.
    /// </summary>
    public sealed class PushBackAdvancedRackParametersWindowTests
    {
        private static NumericField Field(RackPushBackSystemWindow w, string name) => (NumericField)w.FindName(name);
        private static CheckBox Check(RackPushBackSystemWindow w, string name) => (CheckBox)w.FindName(name);
        private static Button Btn(RackPushBackSystemWindow w, string name) => (Button)w.FindName(name);
        private static void Click(Button b) => b.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
        private static void Commit(FrameworkElement f) => f.RaiseEvent(new RoutedEventArgs(FrameworkElement.LostFocusEvent));

        private static RackPushBackSystemWindow Advanced()
        {
            var w = new RackPushBackSystemWindow(canInsertInAutoCad: true);
            w.LoadNew();
            w.Show();
            Check(w, "AdvancedModulesToggle").IsChecked = true;
            return w;
        }

        private static double PostHeight(RackPushBackSystemWindow w)
            => w.EditorStateForTest.WorkingBaseline.Structure.Modules
                .First(m => m.IsHeader && m.AssociatedFrameConfiguration != null)
                .AssociatedFrameConfiguration.Height;

        [Fact]
        public void TheFourScopes_LiveInTheirOwnSection_NotInTheSelectedModulePanel()
        {
            StaTestRunner.Run(() =>
            {
                var w = Advanced();
                try
                {
                    // Present, and none of them inside the per-module controls.
                    foreach (var name in new[]
                             {
                                 "RackHeaderHeightBox", "DerivedPostReinforcedCheck",
                                 "DerivedPostReinforcementHeightBox", "RackSeparatorCountBox", "RackSeparatorSpacingBox",
                                 "RestoreRackParametersButton"
                             })
                    {
                        Assert.NotNull(w.FindName(name));
                    }

                    var modulePanel = (StackPanel)w.FindName("ModuleHeaderPanel");
                    var inModulePanel = Descendants(modulePanel).OfType<FrameworkElement>()
                        .Select(element => element.Name)
                        .Where(name => !string.IsNullOrEmpty(name))
                        .ToList();
                    Assert.DoesNotContain("RackHeaderHeightBox", inModulePanel);
                    Assert.DoesNotContain("RackSeparatorCountBox", inModulePanel);
                    Assert.DoesNotContain("RackSeparatorSpacingBox", inModulePanel);
                    Assert.DoesNotContain("DerivedPostReinforcedCheck", inModulePanel);
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void TheReinforcementHeight_IsLabelledAndExplained_AsTheOwnerAsked()
        {
            StaTestRunner.Run(() =>
            {
                var w = Advanced();
                try
                {
                    var shell = (RackCad.UI.Shell.RackEditorVisualShell)w.Content;
                    var texts = Descendants(shell).OfType<TextBlock>().Select(t => t.Text).ToList();
                    Assert.Contains("Altura del refuerzo (in)", texts);
                    Assert.Contains("Vacio = refuerzo a toda la altura del poste derivado.", texts);
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void AManualHeaderHeight_ReachesTheResolvedRack()
        {
            StaTestRunner.Run(() =>
            {
                var w = Advanced();
                try
                {
                    var manual = PostHeight(w) + 24.0;
                    var box = Field(w, "RackHeaderHeightBox");
                    box.SetNumber(manual);
                    Commit(box);

                    Assert.Equal(manual, w.EditorStateForTest.WorkingBaseline.Structure.ManualHeaderHeightOverride);
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void ClearingTheManualHeight_ReturnsToTheStandingCalculation()
        {
            StaTestRunner.Run(() =>
            {
                var w = Advanced();
                try
                {
                    var box = Field(w, "RackHeaderHeightBox");
                    box.SetNumber(PostHeight(w) + 24.0);
                    Commit(box);
                    Assert.NotNull(w.EditorStateForTest.WorkingBaseline.Structure.ManualHeaderHeightOverride);

                    box.SetNumber(null);
                    Commit(box);

                    Assert.Null(w.EditorStateForTest.WorkingBaseline.Structure.ManualHeaderHeightOverride);
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void TurningTheReinforcementOff_DisablesItsHeight_WithTheReasonVisible()
        {
            StaTestRunner.Run(() =>
            {
                var w = Advanced();
                try
                {
                    var height = Field(w, "DerivedPostReinforcementHeightBox");
                    Assert.True(height.IsEnabled);

                    var check = Check(w, "DerivedPostReinforcedCheck");
                    check.IsChecked = false;
                    check.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));

                    Assert.False(height.IsEnabled);
                    Assert.Contains("refuerzo", height.ToolTip?.ToString() ?? string.Empty);
                    Assert.True(ToolTipService.GetShowOnDisabled(height));
                    Assert.False(w.EditorStateForTest.WorkingBaseline.Structure.DerivedPostReinforced);
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void APartialReinforcementHeight_ReachesTheResolvedRack()
        {
            StaTestRunner.Run(() =>
            {
                var w = Advanced();
                try
                {
                    var partial = PostHeight(w) / 2.0;
                    var box = Field(w, "DerivedPostReinforcementHeightBox");
                    box.SetNumber(partial);
                    Commit(box);

                    Assert.Equal(partial, w.EditorStateForTest.WorkingBaseline.Structure.DerivedPostReinforcementHeight);
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void AReinforcementTallerThanThePost_BlocksWithAVisibleError_AndDoesNotAdvanceTheRack()
        {
            StaTestRunner.Run(() =>
            {
                var w = Advanced();
                try
                {
                    var before = w.EditorStateForTest.WorkingBaseline;
                    var box = Field(w, "DerivedPostReinforcementHeightBox");
                    box.SetNumber(PostHeight(w) + 12.0);
                    Commit(box);

                    var status = (TextBlock)w.FindName("StatusText");
                    Assert.Contains("refuerzo", status.Text);
                    // Nothing was clamped and the baseline did not advance on an invalid model.
                    Assert.Same(before, w.EditorStateForTest.WorkingBaseline);
                    Assert.Equal(PostHeight(w) + 12.0, box.Value);
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void SeparatorCountAndSpacing_AreIndependent_AndEmptyMeansAutomatic()
        {
            StaTestRunner.Run(() =>
            {
                var w = Advanced();
                try
                {
                    var count = Field(w, "RackSeparatorCountBox");
                    count.SetNumber(5);
                    Commit(count);
                    Assert.Equal(5, w.EditorStateForTest.WorkingBaseline.Structure.SeparatorCountOverride);
                    Assert.Null(w.EditorStateForTest.WorkingBaseline.Structure.SeparatorSpacingOverride);

                    var spacing = Field(w, "RackSeparatorSpacingBox");
                    spacing.SetNumber(21.5);
                    Commit(spacing);
                    Assert.Equal(5, w.EditorStateForTest.WorkingBaseline.Structure.SeparatorCountOverride);
                    Assert.Equal(21.5, w.EditorStateForTest.WorkingBaseline.Structure.SeparatorSpacingOverride);

                    count.SetNumber(null);
                    Commit(count);
                    Assert.Null(w.EditorStateForTest.WorkingBaseline.Structure.SeparatorCountOverride);
                    Assert.Equal(21.5, w.EditorStateForTest.WorkingBaseline.Structure.SeparatorSpacingOverride);
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void RestoringTheRackParameters_ReturnsTheFourScopes_AndRepaintsThePanel()
        {
            StaTestRunner.Run(() =>
            {
                var w = Advanced();
                try
                {
                    Field(w, "RackHeaderHeightBox").SetNumber(PostHeight(w) + 24.0);
                    Commit(Field(w, "RackHeaderHeightBox"));
                    Field(w, "RackSeparatorCountBox").SetNumber(5);
                    Commit(Field(w, "RackSeparatorCountBox"));
                    Field(w, "RackSeparatorSpacingBox").SetNumber(21.5);
                    Commit(Field(w, "RackSeparatorSpacingBox"));
                    var check = Check(w, "DerivedPostReinforcedCheck");
                    check.IsChecked = false;
                    check.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));

                    Click(Btn(w, "RestoreRackParametersButton"));

                    Assert.Null(Field(w, "RackHeaderHeightBox").Value);
                    Assert.Null(Field(w, "RackSeparatorCountBox").Value);
                    Assert.Null(Field(w, "RackSeparatorSpacingBox").Value);
                    Assert.True(Check(w, "DerivedPostReinforcedCheck").IsChecked);

                    var structure = w.EditorStateForTest.WorkingBaseline.Structure;
                    Assert.Null(structure.ManualHeaderHeightOverride);
                    Assert.Null(structure.SeparatorCountOverride);
                    Assert.Null(structure.SeparatorSpacingOverride);
                    Assert.True(structure.DerivedPostReinforced);
                }
                finally { w.Close(); }
            });
        }

        private static System.Collections.Generic.IEnumerable<DependencyObject> Descendants(DependencyObject root)
        {
            if (root == null) yield break;
            var count = System.Windows.Media.VisualTreeHelper.GetChildrenCount(root);
            for (var i = 0; i < count; i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(root, i);
                yield return child;
                foreach (var descendant in Descendants(child)) yield return descendant;
            }
        }
    }
}
