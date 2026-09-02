using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using RackCad.Application.Persistence;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.UI.Controls;
using RackCad.UI.Systems.PushBack;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-42 (A4-MOD-LIFECYCLE / N-1, contrato del dueño) — RACKEDITAR DEVUELVE EL RACK QUE SE GUARDO.
    ///
    /// <para>
    /// La secuencia persistida de un rack compuesto es la del RACK —<c>M* + GAP + B:*</c>—, y la ventana carga el
    /// lado A por su propio camino. Entregarle la secuencia ENTERA lo llevaba al resolver de un solo sentido, donde
    /// los modulos no cuadran con las posiciones y se reconstruye la receta estandar: medido, reabrir un documento
    /// con <c>M2 = 30"</c> devolvia <c>M2 = 48"</c> y la mitad B entera estandar.
    /// </para>
    ///
    /// <para>
    /// Ahora el lado A recibe SU cabeza y la cola queda aparcada en el estado compuesto, que es el mismo sitio
    /// donde espera un lado B dormido; el primer recalculo compuesto la devuelve a su lugar.
    /// </para>
    /// </summary>
    public class PushBackCompositeLoadExistingModulesTests
    {
        private static CheckBox Check(RackPushBackSystemWindow window, string name)
            => (CheckBox)window.FindName(name);

        private static NumericField Field(RackPushBackSystemWindow window, string name)
            => (NumericField)window.FindName(name);

        private static Button Button(RackPushBackSystemWindow window, string name)
            => (Button)window.FindName(name);

        private static void LoseFocus(FrameworkElement element)
            => element.RaiseEvent(new RoutedEventArgs(UIElement.LostFocusEvent, element));

        private static void Click(ButtonBase button)
            => button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent, button));

        /// <summary>Un compuesto con personalizaciones de I-40 en LOS DOS lados y un override por linea en B.</summary>
        private static RackPushBackSystemWindow Customized()
        {
            var window = new RackPushBackSystemWindow(canInsertInAutoCad: true);
            Check(window, "SideBPresentCheck").IsChecked = true;
            var matrix = window.CompositeState.Of(PushBackSide.B).Structure;
            for (var front = 0; front < matrix.Count; front++)
            {
                matrix.Fronts[front].IsActive = true;
            }

            var gap = Field(window, "GapBox");
            gap.SetNumber(54.0);
            LoseFocus(gap);

            var session = window.CompositeState.SideA.ModuleSession;
            Assert.True(session.SetLength("M2", 30.0));
            Assert.True(session.SetLength("B:M8", 55.0));
            var configuration = session.HeaderConfigurationCopy("B:M6", 1);
            Assert.NotNull(configuration);
            configuration.Height = 137.0;
            Assert.True(session.ApplyHeaderConfigurationToLine(configuration, 1, new[] { "B:M6" }).Applied);
            window.CompositeState.SideA.CommitModuleEdits();
            LoseFocus(gap);   // recalculo: la edicion se confirma y el baseline avanza
            return window;
        }

        private static PushBackDesign Persist(RackPushBackSystemWindow window)
        {
            var design = window.LastComputation.Design;
            Assert.NotNull(design);
            return PushBackDesignDocument.FromDomain(design).ToDomain();
        }

        private static double LengthOf(RackPushBackSystemWindow window, string moduleId)
            => window.LastComputation.System.Structure.Modules
                .Single(module => string.Equals(module.ModuleId, moduleId, StringComparison.Ordinal))
                .Length;

        private static double LineOverrideHeight(RackPushBackSystemWindow window, int postIndex, string moduleId)
            => window.LastComputation.System.Structure.HeaderLineOverrides
                .Single(line => line.PostIndex == postIndex
                    && string.Equals(line.ModuleId, moduleId, StringComparison.Ordinal))
                .Header.Height;

        private static void AssertCustomizationsAreThere(RackPushBackSystemWindow window)
        {
            Assert.Equal(30.0, LengthOf(window, "M2"), 6);
            Assert.Equal(55.0, LengthOf(window, "B:M8"), 6);
            Assert.Equal(137.0, LineOverrideHeight(window, 1, "B:M6"), 6);
            Assert.Contains(
                window.LastComputation.System.Structure.Modules,
                module => string.Equals(module.ModuleId, "GAP", StringComparison.Ordinal));
        }

        // ---------------------------------------------------------------- ida y vuelta por RACKEDITAR

        [Fact]
        public void CompositeModules_RackEditarRoundTripsPersistedAModuleAndBModuleCustomizations()
        {
            StaTestRunner.Run(() =>
            {
                var saved = Persist(Customized());

                // La ruta REAL de RACKEDITAR.
                var reopened = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                reopened.LoadExisting(saved, "GUID-A4-N1", "PB compuesto");
                LoseFocus(Field(reopened, "GapBox"));   // primer recalculo tras cargar

                AssertCustomizationsAreThere(reopened);
                Assert.Equal(54.0, reopened.CompositeState.Gap, 6);
            });
        }

        [Fact]
        public void CompositeModules_LoadExistingThenUpdateDoesNotStripCompositeCustomizations()
        {
            StaTestRunner.Run(() =>
            {
                var saved = Persist(Customized());

                var reopened = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                reopened.LoadExisting(saved, "GUID-A4-N1", "PB compuesto");
                LoseFocus(Field(reopened, "GapBox"));
                AssertCustomizationsAreThere(reopened);

                // Guardar de nuevo lo que la ventana tiene ahora y volver a abrirlo: exactitud semantica.
                var again = Persist(reopened);
                var third = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                third.LoadExisting(again, "GUID-A4-N1", "PB compuesto");
                LoseFocus(Field(third, "GapBox"));

                AssertCustomizationsAreThere(third);
            });
        }

        [Fact]
        public void CompositeModules_LegacySingleSidedDocumentStillLoadsAsBefore()
        {
            StaTestRunner.Run(() =>
            {
                // Un rack de un solo sentido no tiene cola: su carga es literalmente la de siempre.
                var window = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                var session = window.CompositeState.SideA.ModuleSession;
                Assert.True(session.SetLength("M2", 30.0));
                window.CompositeState.SideA.CommitModuleEdits();
                LoseFocus(Field(window, "GapBox"));
                var saved = Persist(window);
                Assert.False(saved.IsComposite);

                var reopened = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                reopened.LoadExisting(saved, "GUID-A4-LEGACY", "PB simple");
                LoseFocus(Field(reopened, "GapBox"));

                Assert.Equal(30.0, LengthOf(reopened, "M2"), 6);
                Assert.DoesNotContain(
                    reopened.LastComputation.System.Structure.Modules,
                    module => PushBackCompositeStructureIds.IsTail(module.ModuleId));
            });
        }
    }

    /// <summary>Las marcas de la cola compuesta, para las pruebas de la UI.</summary>
    internal static class PushBackCompositeStructureIds
    {
        public static bool IsTail(string moduleId)
            => RackCad.Application.Systems.PushBack.PushBackCompositeStructure.IsCompositeTailId(moduleId);
    }
}
