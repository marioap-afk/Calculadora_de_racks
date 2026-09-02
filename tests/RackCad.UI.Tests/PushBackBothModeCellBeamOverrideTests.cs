using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems.Dynamic;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.UI.Controls;
using RackCad.UI.Systems.PushBack;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-42 (A3-CELL, contrato del dueño) — «LARGO MANUAL» MIXTO EN «AMBOS LADOS»: MOSTRAR NO ES ESCRIBIR.
    ///
    /// <para>
    /// A1C ya vaciaba los campos de la celda cuyo valor difiere entre A y B, y la escritura resolvia ese hueco
    /// contra el lado que escribe. «Largo manual» se quedo a medio camino: el panel SI lo vaciaba, pero la lectura
    /// tomaba el control crudo, asi que el hueco viajaba como <c>null</c> a los dos lados. Bastaba SELECCIONAR
    /// «Ambos lados» —sin tocar el campo— para perder el override authored: medido, A = 100 y B sin override
    /// quedaban en null y null; A = 100 con B = 120 tambien.
    /// </para>
    ///
    /// <para>
    /// <c>null</c> es intencion: significa «sin override», no cero, no el valor del otro lado y no el ancho que la
    /// reticula compartida de A3-G1/G2 resuelve para la bahia. Que la geometria DERIVADA sea comun a los dos lados
    /// no autoriza a escribir la intencion de uno en el otro.
    /// </para>
    /// </summary>
    public class PushBackBothModeCellBeamOverrideTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static ComboBox Combo(RackPushBackSystemWindow window, string name)
            => (ComboBox)window.FindName(name);

        private static CheckBox Check(RackPushBackSystemWindow window, string name)
            => (CheckBox)window.FindName(name);

        private static NumericField Field(RackPushBackSystemWindow window, string name)
            => (NumericField)window.FindName(name);

        private static void LoseFocus(FrameworkElement element)
            => element.RaiseEvent(new RoutedEventArgs(UIElement.LostFocusEvent, element));

        /// <summary>El override AUTHORED de la celda seleccionada en un lado. Es la intencion, no la geometria.</summary>
        private static double? Authored(RackPushBackSystemWindow window, PushBackSide side)
        {
            var matrix = window.CompositeState.Of(side).Structure;
            var front = matrix.Fronts[matrix.SelectedFrontIndex];
            return front.Cells[matrix.SelectedLevelIndex].BeamLengthOverride;
        }

        private static double? AuthoredAt(RackPushBackSystemWindow window, PushBackSide side, int front, int level)
            => window.CompositeState.Of(side).Structure.Fronts[front].Cells[level].BeamLengthOverride;

        private static void SelectSide(RackPushBackSystemWindow window, int index)
            => Combo(window, "SideSelectorBox").SelectedIndex = index;

        private const int LadoA = 0;
        private const int LadoB = 1;
        private const int Ambos = 2;

        /// <summary>
        /// Un compuesto A + hueco + B con «Largo manual» AUTHORED por los controles reales: se elige el lado, se
        /// escribe (o se deja vacio) y se sale del campo. Nadie escribe en el estado por detras del panel.
        /// </summary>
        private static RackPushBackSystemWindow Composite(double? sideA, double? sideB)
        {
            var window = new RackPushBackSystemWindow(canInsertInAutoCad: true);
            Check(window, "SideBPresentCheck").IsChecked = true;
            var matrix = window.CompositeState.Of(PushBackSide.B).Structure;
            for (var front = 0; front < matrix.Count; front++)
            {
                matrix.Fronts[front].IsActive = true;
            }

            var gap = Field(window, "GapBox");
            gap.SetNumber(window.CompositeState.Gap);
            LoseFocus(gap);

            var box = Field(window, "CellBeamLengthOverrideBox");
            SelectSide(window, LadoA);
            box.SetNumber(sideA);
            LoseFocus(box);
            SelectSide(window, LadoB);
            box.SetNumber(sideB);
            LoseFocus(box);
            SelectSide(window, LadoA);
            return window;
        }

        /// <summary>Un recalculo mas, sin ninguna edicion: sale del campo del hueco, que no es de la celda.</summary>
        private static void RecomputeWithoutEditing(RackPushBackSystemWindow window)
            => LoseFocus(Field(window, "GapBox"));

        // ---------------------------------------------------------------- el estado mixto conserva cada lado

        [Fact]
        public void BothMode_CellBeamLengthOverride_MixedStatePreservesEachSide()
        {
            StaTestRunner.Run(() =>
            {
                var window = Composite(100.0, null);
                Assert.Equal(100.0, Field(window, "CellBeamLengthOverrideBox").Value);

                SelectSide(window, Ambos);

                Assert.Null(Field(window, "CellBeamLengthOverrideBox").Value); // mixto: el panel no miente
                Assert.Equal(100.0, Authored(window, PushBackSide.A));
                Assert.Null(Authored(window, PushBackSide.B));
            });
        }

        [Fact]
        public void BothMode_CellBeamLengthOverride_MixedStatePreservesSideB()
        {
            StaTestRunner.Run(() =>
            {
                // El caso inverso: nada puede estar cableado hacia A.
                var window = Composite(null, 120.0);

                SelectSide(window, Ambos);

                Assert.Null(Authored(window, PushBackSide.A));
                Assert.Equal(120.0, Authored(window, PushBackSide.B));
            });
        }

        [Fact]
        public void BothMode_CellBeamLengthOverride_DifferentValuesRemainDistinctUntilEdited()
        {
            StaTestRunner.Run(() =>
            {
                var window = Composite(100.0, 120.0);

                SelectSide(window, Ambos);

                Assert.Null(Field(window, "CellBeamLengthOverrideBox").Value);
                Assert.Equal(100.0, Authored(window, PushBackSide.A));
                Assert.Equal(120.0, Authored(window, PushBackSide.B));
            });
        }

        [Fact]
        public void BothMode_CellBeamLengthOverride_BothNullDoesNotMaterializeResolvedWidth()
        {
            StaTestRunner.Run(() =>
            {
                var window = Composite(null, null);

                SelectSide(window, Ambos);
                RecomputeWithoutEditing(window);

                // La bahia SI tiene un ancho resuelto —y es comun a los dos lados por A3-G1/G2—, pero nadie lo
                // declaro: escribirlo como override convertiria geometria derivada en intencion.
                var beamLength = window.LastComputation.System.Structure.Fronts[0].BeamLength;
                Assert.True(beamLength > 0.0);
                Assert.Null(Authored(window, PushBackSide.A));
                Assert.Null(Authored(window, PushBackSide.B));
            });
        }

        [Fact]
        public void BothMode_CellBeamLengthOverride_EqualValuesShowTheCommonValue()
        {
            StaTestRunner.Run(() =>
            {
                var window = Composite(100.0, 100.0);

                SelectSide(window, Ambos);

                // Iguales no es mixto: el panel puede decir el valor vigente, porque no miente al decirlo.
                Assert.Equal(100.0, Field(window, "CellBeamLengthOverrideBox").Value);
                RecomputeWithoutEditing(window);
                Assert.Equal(100.0, Authored(window, PushBackSide.A));
                Assert.Equal(100.0, Authored(window, PushBackSide.B));
            });
        }

        // ---------------------------------------------------------------- la edicion explicita sigue mandando

        [Fact]
        public void BothMode_CellBeamLengthOverride_ExplicitEditAppliesToBothSides()
        {
            StaTestRunner.Run(() =>
            {
                var window = Composite(100.0, null);
                SelectSide(window, Ambos);

                var box = Field(window, "CellBeamLengthOverrideBox");
                box.SetNumber(130.0);
                LoseFocus(box);

                // El campo no se vuelve de solo lectura: lo que el usuario escribe se aplica a los dos.
                Assert.Equal(130.0, Authored(window, PushBackSide.A));
                Assert.Equal(130.0, Authored(window, PushBackSide.B));
            });
        }

        [Fact]
        public void BothMode_CellBeamLengthOverride_ExplicitClearOfACommonValueStillClearsBothSides()
        {
            StaTestRunner.Run(() =>
            {
                // Sin estado mixto, un hueco SI es authored: borrar un override comun sigue borrandolo en los dos.
                var window = Composite(100.0, 100.0);
                SelectSide(window, Ambos);

                var box = Field(window, "CellBeamLengthOverrideBox");
                box.SetNumber(null);
                LoseFocus(box);

                Assert.Null(Authored(window, PushBackSide.A));
                Assert.Null(Authored(window, PushBackSide.B));
            });
        }

        // ---------------------------------------------------------------- sin edicion, nada se mueve

        [Fact]
        public void BothMode_CellBeamLengthOverride_RepeatedRecomputeWithoutEditIsPure()
        {
            StaTestRunner.Run(() =>
            {
                var window = Composite(100.0, null);
                SelectSide(window, Ambos);

                RecomputeWithoutEditing(window);
                RecomputeWithoutEditing(window);

                Assert.Equal(100.0, Authored(window, PushBackSide.A));
                Assert.Null(Authored(window, PushBackSide.B));
            });
        }

        [Fact]
        public void BothMode_CellBeamLengthOverride_ChangingTheSelectedCellDoesNotWriteOverrides()
        {
            StaTestRunner.Run(() =>
            {
                // Cambiar de celda confirma la anterior (SelectSingleCell -> CommitCurrentCell): esa confirmacion
                // tampoco puede convertir el hueco del estado mixto en un borrado.
                var window = Composite(100.0, null);
                SelectSide(window, Ambos);
                var levels = Combo(window, "SelectedLevelBox");
                Assert.True(levels.Items.Count >= 2); // la ruta existe: el rack nace con tres niveles

                levels.SelectedIndex = 1;

                Assert.Equal(100.0, AuthoredAt(window, PushBackSide.A, 0, 0));
                Assert.Null(AuthoredAt(window, PushBackSide.B, 0, 0));
            });
        }

        // ---------------------------------------------------------------- authored != derivado

        [Fact]
        public void BothMode_CellBeamLengthOverride_SharedPhysicalGridStillEnvelopesBothSides()
        {
            StaTestRunner.Run(() =>
            {
                var window = Composite(150.0, null);

                SelectSide(window, Ambos);
                RecomputeWithoutEditing(window);

                // La INTENCION sigue siendo de cada lado...
                Assert.Equal(150.0, Authored(window, PushBackSide.A));
                Assert.Null(Authored(window, PushBackSide.B));

                // ...y la GEOMETRIA derivada sigue siendo UNA sola (A3-G1/G2), envolviendo a los dos lados.
                var system = window.LastComputation.System;
                var composite = Posts(system.Structure);
                Assert.NotEmpty(composite);
                Assert.Equal(composite, Posts(system.Composite.Of(PushBackSide.A).Local.Structure));
                Assert.Equal(composite, Posts(system.Composite.Of(PushBackSide.B).Local.Structure));
                Assert.Equal(150.0, system.Structure.Fronts[0].BeamLength, 6);
            });
        }

        private static IReadOnlyList<double> Posts(DynamicRackSystem structure)
            => structure == null
                ? Array.Empty<double>()
                : DynamicFrontGeometry.Compute(structure, Catalog).PostPositions
                    .Select(x => Math.Round(x, 6))
                    .ToList();
    }
}
