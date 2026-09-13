using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.Shared;
using Xunit;
using static RackCad.Tests.DimensionViewScenarios;

namespace RackCad.Tests
{
    /// <summary>
    /// I-50, T-07 extremo a extremo (G5) — la política viaja por el camino PRODUCTIVO del Dinámico: diseño →
    /// <c>DynamicRackSystemResolver</c> (C-10) → sistema → builders y emisores de G4, y sobrevive el archivo del
    /// proyecto (C-11). La frontal de salida y la de entrada siguen el bit Frontal, el lateral y sus cortes Lateral y la
    /// planta Planta, exactamente como cuando G4 asignaba la política al sistema resuelto. Cada vista encendida se
    /// compara con el MISMO sistema sin política, así que un cambio ajeno al DTO no puede colarse en la comparación.
    /// </summary>
    public class DynamicDimensionViewsEndToEndTests
    {
        public static IEnumerable<object[]> Policies()
            => new[] { 1, 2, 4, 5, 0, 13, -8 }.Select(policy => new object[] { policy });

        private static void AssertViewsFollow(int policy, DynamicRackSystem system)
        {
            var catalog = Catalog;
            system.Name = RackName;
            var views = DynamicViews(system, catalog);

            var resolved = system.DimensionViews;
            system.DimensionViews = null;
            var legacy = DynamicViews(system, catalog).ToDictionary(view => view.Key);
            system.DimensionViews = resolved;

            Assert.Equal(new[] { "frontal-entrada", "frontal-salida" },
                views.Where(view => view.Kind == DimensionViewKind.Frontal).Select(view => view.Key).OrderBy(key => key));
            foreach (var view in views)
            {
                Assert.True(DimensionLegacySignature.DimensionCount(legacy[view.Key].Instances) > 0, view.Key + ": el legacy de la vista tiene cotas");
                if (BitIsOn(policy, view.Kind))
                {
                    Assert.Equal(DimensionLegacySignature.Of(legacy[view.Key].Instances), DimensionLegacySignature.Of(view.Instances));
                }
                else
                {
                    Assert.True(DimensionLegacySignature.DimensionCount(view.Instances) == 0, $"{view.Key} apagada con {policy}: emitió cotas");
                }
            }
        }

        [Theory]
        [MemberData(nameof(Policies))]
        public void T07_EndToEnd_ThePolicyOfTheDesign_ReachesEveryViewThroughTheResolver(int policy)
        {
            var catalog = Catalog;
            var design = DynamicDesign(DimensionDetail.Standard, catalog);
            design.DimensionViews = Policy(policy);

            var system = new DynamicRackSystemResolver(catalog).Resolve(design).System;

            Assert.Equal(policy, system.DimensionViews.HasValue ? (int)system.DimensionViews.Value : (int?)null);
            AssertViewsFollow(policy, system);
        }

        [Theory]
        [InlineData(2)]
        [InlineData(13)]
        [InlineData(-8)]
        public void T07_EndToEnd_ThePolicy_SurvivesTheProjectFile_AndStillDrivesTheViews(int policy)
        {
            var catalog = Catalog;
            var design = DynamicPersistedDesign(DimensionDetail.Standard, catalog);
            design.DimensionViews = Policy(policy);
            var store = new RackProjectStore();
            var reopened = store.Deserialize(store.Serialize(RackProject.ForDynamic(design)));

            var system = new DynamicRackSystemResolver(catalog).Resolve(reopened.DynamicDesign).System;

            Assert.Equal(policy, system.DimensionViews.HasValue ? (int)system.DimensionViews.Value : (int?)null);
            AssertViewsFollow(policy, system);
        }
    }
}
