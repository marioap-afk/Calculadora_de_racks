using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Systems.Selective;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.Systems.Shared;
using Xunit;
using static RackCad.Tests.DimensionViewScenarios;

namespace RackCad.Tests
{
    /// <summary>
    /// I-50, T-06 y T-09 del Selectivo (G4) — la política asignada al <c>SelectiveRackSystem</c> YA resuelto, dibujada
    /// por el camino del Plugin: la frontal de CADA fondo sigue el bit Frontal a través de <c>FondoSystemView</c> (C-05),
    /// cada corte lateral sigue Lateral y la planta sigue Planta.
    ///
    /// <para>
    /// Una vista encendida es EXACTAMENTE el legacy (<c>DimensionViews = null</c>) con el mismo detalle: mismas cotas y
    /// mismas etiquetas. Una vista apagada no emite cotas y coloca sus etiquetas como las coloca <c>None</c> para esa
    /// vista. Las demás vistas no se enteran.
    /// </para>
    /// </summary>
    public class SelectiveDimensionViewsTests
    {
        public static IEnumerable<object[]> Cases()
            => from twoFondos in new[] { false, true }
               from level in ActiveLevels
               from policy in Policies
               select new object[] { twoFondos, level, policy };

        // ---- T-06 — cotas por tipo de vista ---------------------------------------------------------------------

        [Theory]
        [MemberData(nameof(Cases))]
        public void T06_EachViewKind_DrawsItsCotasOnlyWhenItsBitIsOn(bool twoFondos, DimensionDetail level, int policy)
        {
            var catalog = Catalog;
            var system = Selective(level, twoFondos, catalog);
            var legacy = SelectiveViews(system, catalog).ToDictionary(view => view.Key);

            system.DimensionViews = Policy(policy);
            var views = SelectiveViews(system, catalog);

            Assert.Equal(legacy.Keys.OrderBy(key => key), views.Select(view => view.Key).OrderBy(key => key));
            Assert.Equal(twoFondos ? 2 : 1, views.Count(view => view.Kind == DimensionViewKind.Frontal));
            Assert.Equal(4, views.Count(view => view.Kind == DimensionViewKind.Lateral));
            Assert.Single(views, view => view.Kind == DimensionViewKind.Planta);

            foreach (var view in views)
            {
                var before = legacy[view.Key].Instances;
                Assert.True(DimensionLegacySignature.DimensionCount(before) > 0, view.Key + ": el legacy de la vista tiene cotas");
                if (BitIsOn(policy, view.Kind))
                {
                    Assert.Equal(DimensionLegacySignature.Of(before), DimensionLegacySignature.Of(view.Instances));
                }
                else
                {
                    Assert.True(DimensionLegacySignature.DimensionCount(view.Instances) == 0, $"{view.Key} apagada con {policy}: emitió cotas");
                }
            }
        }

        /// <summary>
        /// C-05: la vista de un fondo lleva la política resuelta EXACTA —bits desconocidos y de signo incluidos—, que es lo
        /// que hace que la frontal de cada fondo obedezca al bit Frontal en el camino del Plugin. Una copia que la
        /// omitiera devolvería la frontal a legacy en silencio; una que la enmascarara perdería los bits que el valor
        /// presente tiene que conservar.
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData(0)]
        [InlineData(2)]
        [InlineData(5)]
        [InlineData(13)]
        [InlineData(-1)]
        [InlineData(-8)]
        public void T06_C05_FondoSystemView_CarriesTheResolvedPolicyExactly_ToEveryFondo(int? policy)
        {
            var catalog = Catalog;
            var system = Selective(DimensionDetail.Standard, twoFondos: true, catalog);
            system.DimensionViews = Policy(policy);

            for (var k = 0; k < SelectiveDepthLayout.Offsets(system).Count; k++)
            {
                var fondoView = SelectiveDepthLayout.FondoSystemView(system, k);
                Assert.Equal(policy, fondoView.DimensionViews.HasValue ? (int)fondoView.DimensionViews.Value : (int?)null);
            }
        }

        // ---- T-09 — etiquetas ----------------------------------------------------------------------------------

        [Theory]
        [MemberData(nameof(Cases))]
        public void T09_AnOffView_PlacesItsLabelsExactlyAsNone_AndAnOnViewKeepsItsLegacyLabels(bool twoFondos, DimensionDetail level, int policy)
        {
            var catalog = Catalog;
            var none = SelectiveViews(Selective(DimensionDetail.None, twoFondos, catalog), catalog).ToDictionary(view => view.Key);
            var system = Selective(level, twoFondos, catalog);
            var legacy = SelectiveViews(system, catalog).ToDictionary(view => view.Key);

            system.DimensionViews = Policy(policy);
            foreach (var view in SelectiveViews(system, catalog))
            {
                if (BitIsOn(policy, view.Kind))
                {
                    Assert.Equal(DimensionLegacySignature.LabelsOnly(legacy[view.Key].Instances), DimensionLegacySignature.LabelsOnly(view.Instances));
                }
                else
                {
                    Assert.Equal(DimensionLegacySignature.LabelsOnly(none[view.Key].Instances), DimensionLegacySignature.LabelsOnly(view.Instances));
                    Assert.Equal(DimensionLegacySignature.Of(none[view.Key].Instances), DimensionLegacySignature.Of(view.Instances));
                }
            }

            // El escenario ejerce el alcance: la frontal legacy coloca sus números de frente fuera de donde los pone None.
            Assert.Contains(legacy.Values, view => view.Kind == DimensionViewKind.Frontal
                && DimensionLegacySignature.LabelsOnly(view.Instances) != DimensionLegacySignature.LabelsOnly(none[view.Key].Instances));
        }
    }
}
