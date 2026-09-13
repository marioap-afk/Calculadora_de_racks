using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.Systems.Shared;
using Xunit;
using static RackCad.Tests.DimensionViewScenarios;

namespace RackCad.Tests
{
    /// <summary>
    /// I-50, T-07 y T-09 del Dinámico (G4) — la política asignada al <c>DynamicRackSystem</c> YA resuelto: la frontal
    /// de salida y la de entrada comparten el bit Frontal, el lateral entero y todos sus cortes siguen Lateral y la
    /// planta sigue Planta.
    ///
    /// <para>
    /// Una vista encendida es EXACTAMENTE el legacy (<c>DimensionViews = null</c>) con el mismo detalle. Una vista
    /// apagada no emite cotas y coloca sus etiquetas como las coloca <c>None</c> para esa vista: en el Dinámico eso
    /// mueve los números de frente y de nivel de las frontales y del lateral, y los números de frente de la planta.
    /// </para>
    /// </summary>
    public class DynamicDimensionViewsTests
    {
        public static IEnumerable<object[]> Cases()
            => from level in ActiveLevels
               from policy in Policies
               select new object[] { level, policy };

        // ---- T-07 — cotas por tipo de vista ---------------------------------------------------------------------

        [Theory]
        [MemberData(nameof(Cases))]
        public void T07_EachViewKind_DrawsItsCotasOnlyWhenItsBitIsOn(DimensionDetail level, int policy)
        {
            var catalog = Catalog;
            var system = Dynamic(level, catalog);
            var legacy = DynamicViews(system, catalog).ToDictionary(view => view.Key);

            system.DimensionViews = Policy(policy);
            var views = DynamicViews(system, catalog);

            Assert.Equal(legacy.Keys.OrderBy(key => key), views.Select(view => view.Key).OrderBy(key => key));
            Assert.Equal(new[] { "frontal-entrada", "frontal-salida" },
                views.Where(view => view.Kind == DimensionViewKind.Frontal).Select(view => view.Key).OrderBy(key => key));
            Assert.Contains(views, view => view.Key == "lateral");
            Assert.True(views.Count(view => view.Key.StartsWith("lateral-corte")) >= 2, "el lateral tiene cortes");
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

        // ---- T-09 — etiquetas ----------------------------------------------------------------------------------

        [Theory]
        [MemberData(nameof(Cases))]
        public void T09_AnOffView_PlacesItsLabelsExactlyAsNone_AndAnOnViewKeepsItsLegacyLabels(DimensionDetail level, int policy)
        {
            var catalog = Catalog;
            var none = DynamicViews(Dynamic(DimensionDetail.None, catalog), catalog).ToDictionary(view => view.Key);
            var system = Dynamic(level, catalog);
            var legacy = DynamicViews(system, catalog).ToDictionary(view => view.Key);

            system.DimensionViews = Policy(policy);
            foreach (var view in DynamicViews(system, catalog))
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

            // El escenario ejerce el alcance en los tres tipos: con cotas, todas las etiquetas legacy se alejan de None.
            Assert.All(legacy.Values, view => Assert.NotEqual(
                DimensionLegacySignature.LabelsOnly(none[view.Key].Instances), DimensionLegacySignature.LabelsOnly(view.Instances)));
        }
    }
}
