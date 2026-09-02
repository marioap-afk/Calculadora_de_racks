using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.PushBack;
using RackCad.UI.Controls;
using RackCad.UI.Systems.PushBack;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-42 (A1C, contrato del dueño) — EL LADO ACTIVO ES CONTEXTO DE INTERFAZ, NO AUTORIDAD.
    ///
    /// <para>
    /// H9: los modulos longitudinales —y con ellos las cabeceras personalizadas de I-40— son del RACK. Su dueño es
    /// UNO y no cambia con el selector de lado: antes, editar con el lado B seleccionado dejaba el commit en el
    /// estado de B, que el ensamblador no lee, y la edicion se perdia sin decir nada.
    /// </para>
    /// <para>
    /// H10: mostrar no es escribir. En «Ambos», un campo cuyos dos lados difieren viaja VACIO y cada lado conserva
    /// el suyo; solo un valor escrito por el usuario se aplica a los dos.
    /// </para>
    /// </summary>
    public sealed class PushBackEditorSideOwnershipTests
    {
        private static ComboBox Combo(RackPushBackSystemWindow w, string name) => (ComboBox)w.FindName(name);
        private static CheckBox Check(RackPushBackSystemWindow w, string name) => (CheckBox)w.FindName(name);
        private static NumericField Field(RackPushBackSystemWindow w, string name) => (NumericField)w.FindName(name);

        private static void LoseFocus(FrameworkElement element)
            => element.RaiseEvent(new RoutedEventArgs(UIElement.LostFocusEvent, element));

        /// <summary>Fuerza un recalculo con el MISMO control real que usa el usuario, sin cambiar nada.</summary>
        private static void Recompute(RackPushBackSystemWindow w)
        {
            var gap = Field(w, "GapBox");
            gap.SetNumber(w.CompositeState.Gap);
            LoseFocus(gap);
        }

        /// <summary>Un rack compuesto con los dos lados poblados, montado con los controles reales.</summary>
        private static RackPushBackSystemWindow Composite()
        {
            var w = new RackPushBackSystemWindow(canInsertInAutoCad: true);
            Check(w, "SideBPresentCheck").IsChecked = true;
            var matrix = w.CompositeState.Of(PushBackSide.B).Structure;
            for (var front = 0; front < matrix.Count; front++)
            {
                matrix.Fronts[front].IsActive = true;
            }

            Recompute(w);
            return w;
        }

        private static void EditSide(RackPushBackSystemWindow w, PushBackSideSelection selection)
            => Combo(w, "SideSelectorBox").SelectedIndex = (int)selection;

        /// <summary>El estado que la ventana usa como dueño de los modulos del rack.</summary>
        private static PushBackEditorState Owner(RackPushBackSystemWindow w) => w.CompositeState.SideA;

        private static double LengthOf(RackPushBackSystemWindow w, string moduleId)
            => w.LastComputation.System.Structure.Modules
                .Where(module => string.Equals(module.ModuleId, moduleId, StringComparison.Ordinal))
                .Select(module => module.Length)
                .DefaultIfEmpty(double.NaN)
                .First();

        // ---------------------------------------------------------------- H9

        [Fact]
        public void ModuleEdit_AuthoredWithSideBActive_ReachesTheRack()
        {
            // El nombre del contrato: la edicion authored con B activo llega al RACK. No «al lado B»: la secuencia
            // longitudinal es UNA, y el ensamblador la lee siempre del mismo sitio.
            var ok = StaTestRunner.Run(() =>
            {
                var w = Composite();
                EditSide(w, PushBackSideSelection.B);

                var target = Owner(w).ModuleSession.Modules.First(module => !module.IsHeader);
                var before = LengthOf(w, target.ModuleId);
                Owner(w).ModuleSession.SetLength(target.ModuleId, before + 7.0);
                Owner(w).CommitModuleEdits();
                Recompute(w);

                return Math.Abs(LengthOf(w, target.ModuleId) - (before + 7.0)) < 1e-6;
            });

            Assert.True(ok, "una edicion de modulo hecha con el lado B activo debe llegar al rack");
        }

        [Fact]
        public void ModuleCommit_NeverLandsOnTheSideThatDoesNotOwnTheModules()
        {
            var ok = StaTestRunner.Run(() =>
            {
                var w = Composite();
                EditSide(w, PushBackSideSelection.B);

                var target = Owner(w).ModuleSession.Modules.First(module => !module.IsHeader);
                Owner(w).ModuleSession.SetLength(target.ModuleId, target.Length + 5.0);
                Owner(w).CommitModuleEdits();

                // El commit vive en el dueño, nunca en el lado que solo se estaba mirando.
                return w.CompositeState.SideA.ModuleCommit != null
                       && w.CompositeState.SideB.ModuleCommit == null;
            });

            Assert.True(ok);
        }

        [Fact]
        public void ModuleBaseline_IsOwnedByOneStateAndNeverDuplicatedIntoSideB()
        {
            // La linea de modulos del rack —incluidos el hueco y la mitad de B, que I-40 tiene que poder
            // personalizar— vive en UN estado. El otro lado no recibe una copia, y quien la recibe no depende de
            // que lado estuviera seleccionado.
            var ok = StaTestRunner.Run(() =>
            {
                var w = Composite();
                var afterA = w.CompositeState.SideB.WorkingBaseline == null;

                EditSide(w, PushBackSideSelection.B);
                Recompute(w);
                var afterB = w.CompositeState.SideB.WorkingBaseline == null
                             && w.CompositeState.SideA.WorkingBaseline != null;

                EditSide(w, PushBackSideSelection.Both);
                Recompute(w);
                var afterBoth = w.CompositeState.SideB.WorkingBaseline == null;

                // Y el dueño sigue teniendo la secuencia COMPLETA del rack: el hueco y la mitad de B son suyos.
                var ids = Owner(w).ModuleSession.Modules.Select(module => module.ModuleId).ToList();
                return afterA && afterB && afterBoth
                       && ids.Any(id => id.StartsWith("B:", StringComparison.Ordinal))
                       && ids.Contains(PushBackCompositeStructure.GapModuleId);
            });

            Assert.True(ok);
        }

        [Fact]
        public void SwitchingActiveSide_DoesNotMoveHeaderOverridesBetweenSides()
        {
            var ok = StaTestRunner.Run(() =>
            {
                var w = Composite();
                var owner = Owner(w);
                var header = owner.ModuleSession.Modules.First(module => module.IsHeader);
                var copy = owner.ModuleSession.HeaderConfigurationCopy(header.ModuleId, 0);
                if (copy == null)
                {
                    return false;
                }

                owner.ModuleSession.ApplyHeaderConfigurationToInstances(
                    copy, new[] { header.ModuleId }, new[] { 0 });
                owner.CommitModuleEdits();
                Recompute(w);
                var overridesAfterEdit = w.LastComputation.System.Structure.HeaderLineOverrides.Count;

                // Cambiar de lado es mirar, no editar: los overrides del rack no se mueven ni se pierden.
                EditSide(w, PushBackSideSelection.B);
                Recompute(w);
                var afterB = w.LastComputation.System.Structure.HeaderLineOverrides.Count;

                EditSide(w, PushBackSideSelection.A);
                Recompute(w);
                var afterA = w.LastComputation.System.Structure.HeaderLineOverrides.Count;

                return overridesAfterEdit > 0 && afterB == overridesAfterEdit && afterA == overridesAfterEdit;
            });

            Assert.True(ok);
        }

        [Fact]
        public void ModuleIds_AreStableAcrossSideSwitches()
        {
            var ok = StaTestRunner.Run(() =>
            {
                var w = Composite();
                var before = Owner(w).ModuleSession.Modules.Select(module => module.ModuleId).ToList();

                EditSide(w, PushBackSideSelection.B);
                Recompute(w);
                var duringB = Owner(w).ModuleSession.Modules.Select(module => module.ModuleId).ToList();

                EditSide(w, PushBackSideSelection.A);
                Recompute(w);
                var backToA = Owner(w).ModuleSession.Modules.Select(module => module.ModuleId).ToList();

                return before.Count > 0
                       && before.SequenceEqual(duringB, StringComparer.Ordinal)
                       && before.SequenceEqual(backToA, StringComparer.Ordinal);
            });

            Assert.True(ok);
        }

        [Fact]
        public void ThePanelKeepsTheRackModules_WhileEditingSideB()
        {
            // Sintoma visible del defecto: con B activo el panel de modulos se quedaba sin lista, porque miraba la
            // sesion del lado B —que no tiene ninguna—.
            var ok = StaTestRunner.Run(() =>
            {
                var w = Composite();
                var withA = Combo(w, "ModuleBox").Items.Count;

                EditSide(w, PushBackSideSelection.B);
                Recompute(w);

                return withA > 0 && Combo(w, "ModuleBox").Items.Count == withA;
            });

            Assert.True(ok);
        }

        // ---------------------------------------------------------------- H10

        /// <summary>Escribe valores DISTINTOS en la celda seleccionada de cada lado.</summary>
        private static void MakeSidesDiffer(RackPushBackSystemWindow w)
        {
            var a = w.CompositeState.SideA.Structure.Fronts[0];
            var b = w.CompositeState.SideB.Structure.Fronts[0];
            a.Cells[0].PalletHeight = 60.0;
            a.Cells[0].PalletWeight = 1000.0;
            a.Cells[0].ClearHeight = 6.0;
            b.Cells[0].PalletHeight = 72.0;
            b.Cells[0].PalletWeight = 1500.0;
            b.Cells[0].ClearHeight = 9.0;
        }

        private static string CellOf(RackPushBackSystemWindow w, PushBackSide side)
        {
            var cell = w.CompositeState.Of(side).Structure.Fronts[0].Cells[0];
            return FormattableString.Invariant(
                $"{cell.PalletFront:0.####}|{cell.PalletHeight:0.####}|{cell.PalletWeight:0.####}|{cell.ClearHeight:0.####}");
        }

        [Fact]
        public void BothMode_RecomputeWithoutEditing_DoesNotMutateBCells()
        {
            var ok = StaTestRunner.Run(() =>
            {
                var w = Composite();
                MakeSidesDiffer(w);
                var before = CellOf(w, PushBackSide.B);

                EditSide(w, PushBackSideSelection.Both);
                Recompute(w);

                return CellOf(w, PushBackSide.B) == before;
            });

            Assert.True(ok, "seleccionar «Ambos» sin editar no puede reescribir el lado B");
        }

        [Fact]
        public void BothMode_RecomputeWithoutEditing_DoesNotMutateACells()
        {
            var ok = StaTestRunner.Run(() =>
            {
                var w = Composite();
                MakeSidesDiffer(w);
                var before = CellOf(w, PushBackSide.A);

                EditSide(w, PushBackSideSelection.Both);
                Recompute(w);

                return CellOf(w, PushBackSide.A) == before;
            });

            Assert.True(ok);
        }

        [Fact]
        public void BothMode_MixedValuesRemainDistinctUntilExplicitEdit()
        {
            var ok = StaTestRunner.Run(() =>
            {
                var w = Composite();
                MakeSidesDiffer(w);

                EditSide(w, PushBackSideSelection.Both);
                Recompute(w);
                Recompute(w);   // y un segundo refresco tampoco los iguala

                return CellOf(w, PushBackSide.A) != CellOf(w, PushBackSide.B);
            });

            Assert.True(ok);
        }

        [Fact]
        public void BothMode_RefreshDoesNotMarkUntouchedFieldsDirty()
        {
            // El campo MIXTO viaja vacio: eso es lo que significa «cada lado conserva el suyo», y es lo que impide
            // que un refresco escriba.
            var ok = StaTestRunner.Run(() =>
            {
                var w = Composite();
                MakeSidesDiffer(w);

                EditSide(w, PushBackSideSelection.Both);

                return !Field(w, "CellPalletHeightBox").Value.HasValue
                       && !Field(w, "CellPalletWeightBox").Value.HasValue
                       && !Field(w, "CellClearBox").Value.HasValue;
            });

            Assert.True(ok);
        }

        [Fact]
        public void BothMode_ExplicitCellEditAppliesToBothSides()
        {
            var ok = StaTestRunner.Run(() =>
            {
                var w = Composite();
                MakeSidesDiffer(w);
                EditSide(w, PushBackSideSelection.Both);

                var box = Field(w, "CellPalletHeightBox");
                box.SetNumber(66.0);
                LoseFocus(box);

                var a = w.CompositeState.SideA.Structure.Fronts[0].Cells[0];
                var b = w.CompositeState.SideB.Structure.Fronts[0].Cells[0];

                // El alto se aplica a los DOS; lo que nadie escribio sigue siendo de cada lado.
                return Math.Abs(a.PalletHeight - 66.0) < 1e-6
                       && Math.Abs(b.PalletHeight - 66.0) < 1e-6
                       && Math.Abs(a.PalletWeight - 1000.0) < 1e-6
                       && Math.Abs(b.PalletWeight - 1500.0) < 1e-6;
            });

            Assert.True(ok);
        }

        [Fact]
        public void EqualValues_AreNotTreatedAsMixed()
        {
            // Sin diferencia no hay estado mixto: el campo sigue mostrando el valor, como siempre.
            var ok = StaTestRunner.Run(() =>
            {
                var w = Composite();
                EditSide(w, PushBackSideSelection.Both);

                return Field(w, "CellPalletHeightBox").Value.HasValue;
            });

            Assert.True(ok);
        }

        // ---------------------------------------------------------------- H12 en la ventana

        [Fact]
        public void CompositeRecompute_ReinforcementValidationExceptionIsContained()
        {
            // La validacion REAL del refuerzo contra el poste derivado lanza; la ventana la declara y sigue viva.
            var ok = StaTestRunner.Run(() =>
            {
                var w = Composite();
                Check(w, "DerivedPostReinforcedCheck").IsChecked = true;
                var height = Field(w, "DerivedPostReinforcementHeightBox");
                height.SetNumber(100000.0);
                LoseFocus(height);

                // Ni excepcion ni modelo nuevo: llegar hasta aqui YA demuestra la contencion —la validacion real
                // lanza—, y ademas el editor lo declara: entradas invalidas y el motivo a la vista.
                var status = (System.Windows.Controls.TextBlock)w.FindName("StatusText");
                return !w.CurrentInputsAreValid
                       && status != null
                       && (status.Text ?? string.Empty).IndexOf("refuerzo", StringComparison.OrdinalIgnoreCase) >= 0;
            });

            Assert.True(ok);
        }
    }
}
