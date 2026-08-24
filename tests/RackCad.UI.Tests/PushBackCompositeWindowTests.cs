using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Shared;
using RackCad.UI.Controls;
using RackCad.UI.Systems.PushBack;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-42 (G6) — la seccion COMPUESTA de la ventana real: selector de lado, hueco, separador central, topologia por
    /// celda con sus cinco alcances y estructura efectiva con restauracion. Lo que se fija es que la ventana sigue
    /// siendo una capa fina sobre el modelo puro y que un rack de un solo sentido no cambia en nada.
    /// </summary>
    public sealed class PushBackCompositeWindowTests
    {
        private static ComboBox Combo(RackPushBackSystemWindow w, string name) => (ComboBox)w.FindName(name);
        private static CheckBox Check(RackPushBackSystemWindow w, string name) => (CheckBox)w.FindName(name);
        private static Button Btn(RackPushBackSystemWindow w, string name) => (Button)w.FindName(name);
        private static NumericField Field(RackPushBackSystemWindow w, string name) => (NumericField)w.FindName(name);

        private static void Click(ButtonBase button)
            => button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent, button));

        private static void LoseFocus(FrameworkElement element)
            => element.RaiseEvent(new RoutedEventArgs(UIElement.LostFocusEvent, element));

        private static StackPanel Section(RackPushBackSystemWindow w) => (StackPanel)w.FindName("CompositeSection");

        [Fact]
        public void ANewRack_OpensAsSingleSided_WithTheCompositeSectionCollapsed()
        {
            var ok = StaTestRunner.Run(() =>
            {
                var w = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                // COLAPSADA, no solo deshabilitada: con un rack de un sentido no debe ocupar espacio en la barra.
                return !w.CompositeState.SideBPresent
                       && w.CompositeState.ActiveSide == PushBackSide.A
                       && Check(w, "SideBPresentCheck").IsChecked != true
                       && Section(w).Visibility == Visibility.Collapsed
                       && Check(w, "SideBPresentCheck").Visibility == Visibility.Visible
                       && w.LastComputation.IsValid
                       && !w.LastComputation.System.IsComposite;
            });

            Assert.True(ok);
        }

        [Fact]
        public void DeclaringSideB_ShowsTheSection_AndProducesACompositeSystem()
        {
            var ok = StaTestRunner.Run(() =>
            {
                var w = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                Check(w, "SideBPresentCheck").IsChecked = true;

                return w.CompositeState.SideBPresent
                       && Section(w).Visibility == Visibility.Visible
                       && Combo(w, "SideSelectorBox").IsEnabled
                       && Field(w, "GapBox").IsEnabled
                       && Btn(w, "ApplyTopologyButton").IsEnabled
                       && w.LastComputation.IsValid
                       && w.LastComputation.System.IsComposite;
            });

            Assert.True(ok);
        }

        [Fact]
        public void TogglingTheCompositeOff_CollapsesTheSection_AndKeepsSideBDormant()
        {
            var ok = StaTestRunner.Run(() =>
            {
                var w = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                Check(w, "SideBPresentCheck").IsChecked = true;
                Combo(w, "SideSelectorBox").SelectedIndex = 1;
                w.CompositeState.SideB.SetFrontCount(3);
                Field(w, "GapBox").SetNumber(14.0);
                LoseFocus(Field(w, "GapBox"));

                Check(w, "SideBPresentCheck").IsChecked = false;
                var collapsed = Section(w).Visibility == Visibility.Collapsed
                                && !w.LastComputation.System.IsComposite;

                Check(w, "SideBPresentCheck").IsChecked = true;
                // Todo reaparece intacto: la configuracion del lado B quedo dormante, no se destruyo.
                return collapsed
                       && Section(w).Visibility == Visibility.Visible
                       && w.CompositeState.SideB.Structure.Count == 3
                       && Math.Abs(w.CompositeState.Gap - 14.0) < 1e-6
                       && w.LastComputation.System.IsComposite;
            });

            Assert.True(ok);
        }

        [Fact]
        public void ApplyingAnEmptyStructureField_IsNotARestore()
        {
            var ok = StaTestRunner.Run(() =>
            {
                var w = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                Check(w, "SideBPresentCheck").IsChecked = true;
                Field(w, "StructureOverrideBox").SetNumber(9.0);
                Click(Btn(w, "ApplyStructureButton"));
                var applied = w.CompositeState.StructureOverrideA;

                // Vaciar el campo y pulsar «Aplicar» NO restaura: restaurar es su propio boton.
                Field(w, "StructureOverrideBox").SetNumber(null);
                Click(Btn(w, "ApplyStructureButton"));
                var afterEmptyApply = w.CompositeState.StructureOverrideA;

                Click(Btn(w, "RestoreStructureButton"));
                return applied == 9 && afterEmptyApply == 9 && w.CompositeState.StructureOverrideA == null;
            });

            Assert.True(ok);
        }

        [Fact]
        public void TheSideSelector_SwitchesTheMatrixWithoutLosingTheOtherSide()
        {
            var ok = StaTestRunner.Run(() =>
            {
                var w = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                Check(w, "SideBPresentCheck").IsChecked = true;
                var sideA = w.State;

                Combo(w, "SideSelectorBox").SelectedIndex = 1;
                var sideB = w.State;

                Combo(w, "SideSelectorBox").SelectedIndex = 0;
                return !ReferenceEquals(sideA, sideB)
                       && ReferenceEquals(w.State, sideA)
                       && w.CompositeState.ActiveSide == PushBackSide.A;
            });

            Assert.True(ok);
        }

        [Fact]
        public void RetiringSideB_KeepsItsConfigurationDormant()
        {
            var ok = StaTestRunner.Run(() =>
            {
                var w = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                Check(w, "SideBPresentCheck").IsChecked = true;
                Combo(w, "SideSelectorBox").SelectedIndex = 1;
                w.CompositeState.SideB.SetFrontCount(3);

                Check(w, "SideBPresentCheck").IsChecked = false;
                var backToA = w.CompositeState.ActiveSide == PushBackSide.A && !w.LastComputation.System.IsComposite;

                Check(w, "SideBPresentCheck").IsChecked = true;
                return backToA && w.CompositeState.SideB.Structure.Count == 3;
            });

            Assert.True(ok);
        }

        [Fact]
        public void TheGapAndTheCentralSeparator_ReachTheResolvedSystem()
        {
            var ok = StaTestRunner.Run(() =>
            {
                var w = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                Check(w, "SideBPresentCheck").IsChecked = true;
                var before = w.LastComputation.System.Structure.TotalLength;

                Field(w, "GapBox").SetNumber(12.0);
                LoseFocus(Field(w, "GapBox"));
                var widened = w.LastComputation.System.Structure.TotalLength;

                Check(w, "CentralSeparatorCheck").IsChecked = true;
                return Math.Abs(widened - before - 12.0) < 1e-6
                       && w.LastComputation.System.Composite.CentralSeparator;
            });

            Assert.True(ok);
        }

        [Fact]
        public void ApplyingTopology_UsesTheSameFiveScopes_AndReachesTheSystem()
        {
            var ok = StaTestRunner.Run(() =>
            {
                var w = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                Check(w, "SideBPresentCheck").IsChecked = true;
                Combo(w, "CellTopologyBox").SelectedIndex = 3;    // Corrida
                Combo(w, "RunDirectionBox").SelectedIndex = 0;    // A -> B
                Combo(w, "TopologyScopeBox").SelectedIndex = 4;   // Todo
                Click(Btn(w, "ApplyTopologyButton"));

                var runs = PushBackRuns.Resolve(w.LastComputation.System);
                return runs.Runs.Count > 0
                       && runs.Runs.All(run => run.Topology == PushBackCellTopology.Corrida)
                       && runs.Runs.All(run => run.LowSide == PushBackSide.A && run.HighSide == PushBackSide.B);
            });

            Assert.True(ok);
        }

        [Fact]
        public void TheStructureOverride_AppliesAndRestores()
        {
            var ok = StaTestRunner.Run(() =>
            {
                var w = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                Check(w, "SideBPresentCheck").IsChecked = true;
                var proposed = w.LastComputation.System.Composite.SideA.ProposedStructure;

                Field(w, "StructureOverrideBox").SetNumber(proposed + 3);
                Click(Btn(w, "ApplyStructureButton"));
                var overridden = w.LastComputation.System.Composite.SideA.EffectiveStructure;

                Click(Btn(w, "RestoreStructureButton"));
                var restored = w.LastComputation.System.Composite.SideA;

                return overridden == proposed + 3
                       && restored.EffectiveStructure == restored.ProposedStructure
                       && !restored.StructureOverride.HasValue;
            });

            Assert.True(ok);
        }

        [Fact]
        public void ACompositeDesign_RoundTripsThroughTheWindow()
        {
            var design = new PushBackDesign
            {
                Structure = new DynamicRackDesign
                {
                    Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                    PalletsDeep = 4,
                    LoadLevels = 2,
                    FirstLevelHeight = 4.0,
                    BeamDepth = 4.0
                },
                SideB = new PushBackSideDesign { IsPresent = true, LoadLevels = 2, FirstLevelHeight = 4.0 },
                Composite = new PushBackCompositeDesign
                {
                    Gap = 8.0,
                    DefaultTopology = PushBackCellTopology.Encontradas
                }
            };
            design.Structure.Fronts.Add(new DynamicRackFrontDesign
            {
                PalletCount = 1, LoadLevels = 2, PalletsDeep = 4, DepthStartPosition = 1
            });
            design.Fronts.Add(new PushBackFrontConfig { DefaultPalletsDeep = 4 });
            design.SideB.Fronts.Add(new DynamicRackFrontDesign
            {
                PalletCount = 1, LoadLevels = 2, PalletsDeep = 4, DepthStartPosition = 1
            });
            design.SideB.FrontConfigs.Add(new PushBackFrontConfig { DefaultPalletsDeep = 4 });

            var ok = StaTestRunner.Run(() =>
            {
                var w = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                w.LoadExisting(design, Guid.NewGuid().ToString(), "PB compuesto");

                return w.CompositeState.SideBPresent
                       && Math.Abs(w.CompositeState.Gap - 8.0) < 1e-6
                       && w.LastComputation.IsValid
                       && w.LastComputation.System.IsComposite
                       && w.DesignToInsert == null;
            });

            Assert.True(ok);
        }

        [Fact]
        public void ALegacyDesign_OpensAsSingleSided_WithoutAskingForAnything()
        {
            var design = new PushBackDesign
            {
                Structure = new DynamicRackDesign
                {
                    Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                    PalletsDeep = 5,
                    LoadLevels = 3,
                    FirstLevelHeight = 6.0,
                    BeamDepth = 4.0
                }
            };
            design.Structure.Fronts.Add(new DynamicRackFrontDesign
            {
                PalletCount = 2, LoadLevels = 3, PalletsDeep = 5, DepthStartPosition = 1
            });
            design.Fronts.Add(new PushBackFrontConfig());

            var ok = StaTestRunner.Run(() =>
            {
                var w = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                w.LoadExisting(design, Guid.NewGuid().ToString(), "PB legacy");

                return !w.CompositeState.SideBPresent
                       && w.CompositeState.ActiveSide == PushBackSide.A
                       && w.LastComputation.IsValid
                       && !w.LastComputation.System.IsComposite
                       && w.LastComputation.Design.SideB == null;
            });

            Assert.True(ok);
        }
    }
}
