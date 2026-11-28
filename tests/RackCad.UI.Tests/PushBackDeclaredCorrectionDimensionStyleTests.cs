using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Drawing;
using RackCad.UI.Systems.PushBack;
using Xunit;
using static RackCad.UI.Tests.PushBackDeclaredCorrectionSupport;
using static RackCad.UI.Tests.PushBackFullEditorCharacterizationSupport;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64 F4B PB-TC — la correccion declarada (b) de D-PB-06 en la ventana CLASICA de Push Back: el estilo de cotas que trae la
    /// carga viaja en cada pasada. CT-PBTC-04 lo fija en las cuatro cargas y tras el gesto de las casillas de cotas, y CT-PBTC-05
    /// lo lleva hasta cada cota que «Actualizar» de RACKEDITAR dibuja.
    ///
    /// <para>Toda ventana se crea con <see cref="PushBackFullEditorCharacterizationSupport.Ventana"/> y su carga va dentro del
    /// <c>try</c>; ninguna se muestra y no se usa el teclado ni el foco real. Un rack nuevo sigue sin estilo.</para>
    /// </summary>
    [Collection(EditorDiscardPromptCollection.Name)]
    public sealed class PushBackDeclaredCorrectionDimensionStyleTests
    {
        private static string Estilo(RackPushBackSystemWindow w)
            => w.LastComputation?.Design?.Structure?.DimensionStyle ?? "<null>";

        [Fact]
        public void CT_PBTC_04_TheLoadedDimensionStyleSurvivesTheLoadsAndTheGestures()
        {
            var log = new List<string>();
            Sta(() =>
            {
                // La construccion y el gesto de T19_H1.
                var named = Ventana();
                try
                {
                    named.LoadExisting(EstiloPb(), "GUID-PB-H1", "PB H1");
                    log.Add("ESTILO_PB carga=" + Estilo(named));
                    Check(named, "DimensionsLateralCheck").IsChecked = false;
                    log.Add("ESTILO_PB gesto DimensionsLateralCheck=" + Check(named, "DimensionsLateralCheck").IsChecked + " estilo=" + Estilo(named));
                }
                finally { Cerrar(named); }

                foreach (var fixture in new[] { "pb-06-seguridad", "pb-07-combinado" })
                {
                    var w = Ventana();
                    try
                    {
                        Cargar(w, Fixture(fixture));
                        log.Add(fixture + " carga=" + Estilo(w));
                    }
                    finally { Cerrar(w); }
                }

                var biblioteca = Ventana();
                try
                {
                    CargarBiblioteca(biblioteca, Fixture("pb-07-combinado"));
                    log.Add("pb-07-combinado biblioteca=" + Estilo(biblioteca));
                }
                finally { Cerrar(biblioteca); }

                var nuevo = Ventana();
                try
                {
                    log.Add("nuevo estilo=" + Estilo(nuevo));
                }
                finally { Cerrar(nuevo); }
            });

            Assert.Equal(
                new[]
                {
                    "ESTILO_PB carga=ESTILO_PB",
                    "ESTILO_PB gesto DimensionsLateralCheck=False estilo=ESTILO_PB",
                    "pb-06-seguridad carga=STANDARD-ISO",
                    "pb-07-combinado carga=SIMPLEX",
                    "pb-07-combinado biblioteca=SIMPLEX",
                    "nuevo estilo=<null>",
                },
                log);
        }

        /// <summary>Las cotas de la ultima computacion: las instancias de rol <c>Dimension</c> de los cinco planos del rack.</summary>
        private static IEnumerable<HeaderBlockInstance> Cotas(RackPushBackSystemWindow w)
        {
            var computation = w.LastComputation;
            var plans = new List<HeaderRunPlan> { computation.PlantaPlan, computation.FrontalEntradaSalida, computation.FrontalPosterior, computation.LateralPlan };
            plans.AddRange(computation.LateralCortes.Select(corte => corte.Plan));
            return plans
                .Where(plan => plan != null)
                .SelectMany(plan => plan.Flatten().Instances)
                .Where(instance => instance.Role == HeaderBlockRole.Dimension);
        }

        [Fact]
        public void CT_PBTC_05_UpdateInRackeditarCarriesTheNamedStyleToEveryDimension()
        {
            var log = new List<string>();
            Sta(() =>
            {
                foreach (var fixture in new[] { "pb-06-seguridad", "pb-07-combinado" })
                {
                    var w = Ventana();
                    try
                    {
                        Cargar(w, Fixture(fixture));
                        Assert.True(Btn(w, "UpdateButton").IsEnabled, StatusText(w));
                        Click(w, "UpdateButton");
                        var cotas = Cotas(w).ToList();
                        var estilos = cotas
                            .Select(cota => cota.DimensionStyleName ?? "<null>")
                            .Distinct()
                            .OrderBy(nombre => nombre, System.StringComparer.Ordinal);
                        log.Add(fixture + " peticion=" + w.InsertRequested + " updateOnly=" + w.UpdateOnly
                            + " diseno=" + (w.DesignToInsert.Structure.DimensionStyle ?? "<null>")
                            + " sistema=" + (w.SystemToInsert.Structure.DimensionStyle ?? "<null>")
                            + " cotas>0=" + (cotas.Count > 0)
                            + " estilos=[" + string.Join(",", estilos) + "]");
                    }
                    finally { Cerrar(w); }
                }
            });

            Assert.Equal(
                new[]
                {
                    "pb-06-seguridad peticion=True updateOnly=True diseno=STANDARD-ISO sistema=STANDARD-ISO cotas>0=True estilos=[STANDARD-ISO]",
                    "pb-07-combinado peticion=True updateOnly=True diseno=SIMPLEX sistema=SIMPLEX cotas>0=True estilos=[SIMPLEX]",
                },
                log);
        }
    }
}
