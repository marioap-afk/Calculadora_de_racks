using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;
using Xunit;
using static RackCad.Tests.DimensionViewScenarios;

namespace RackCad.Tests
{
    /// <summary>
    /// I-50, T-06 extremo a extremo (G5) — la política viaja por el camino PRODUCTIVO: diseño →
    /// <c>SelectiveGeometryResolver</c> (C-04) → sistema → <c>FondoSystemView</c> (C-05) → builders y emisores de G4. Y
    /// sobrevive el documento embebido (C-06): diseño → DTO → JSON → DTO → diseño → resolver → vistas. No repite la tabla
    /// exhaustiva de G4: usa combinaciones que discriminan cada tipo y centinelas exactos.
    /// </summary>
    public class SelectiveDimensionViewsEndToEndTests
    {
        public static IEnumerable<object[]> Cases()
            => from twoFondos in new[] { false, true }
               from policy in new[] { 1, 2, 4, 6, 0, 13, -8 }
               select new object[] { twoFondos, policy };

        private static void AssertViewsFollow(int policy, SelectiveRackSystem system, bool twoFondos)
        {
            var catalog = Catalog;
            var legacy = SelectiveViews(Selective(DimensionDetail.Standard, twoFondos, catalog), catalog).ToDictionary(view => view.Key);
            system.Name = RackName;
            var views = SelectiveViews(system, catalog);

            Assert.Equal(legacy.Keys.OrderBy(key => key), views.Select(view => view.Key).OrderBy(key => key));
            foreach (var view in views)
            {
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
        [MemberData(nameof(Cases))]
        public void T06_EndToEnd_ThePolicyOfTheDesign_ReachesEveryViewThroughTheResolver(bool twoFondos, int policy)
        {
            var design = SelectiveDesign(DimensionDetail.Standard, twoFondos);
            design.DimensionViews = Policy(policy);

            var system = new SelectiveGeometryResolver().Resolve(design, Catalog);

            Assert.Equal(policy, system.DimensionViews.HasValue ? (int)system.DimensionViews.Value : (int?)null);
            AssertViewsFollow(policy, system, twoFondos);
        }

        [Theory]
        [InlineData(5)]
        [InlineData(2)]
        [InlineData(13)]
        [InlineData(-8)]
        public void T06_EndToEnd_ThePolicy_SurvivesTheEmbeddedDocument_AndStillDrivesTheViews(int policy)
        {
            var design = SelectiveDesign(DimensionDetail.Standard, twoFondos: true);
            design.DimensionViews = Policy(policy);
            var store = new SelectivePalletDesignStore();
            var reopened = store.Deserialize(store.Serialize(SelectivePalletDesignDocument.From(design, "I50-ID", RackName))).ToDomain();

            var system = new SelectiveGeometryResolver().Resolve(reopened, Catalog);

            Assert.Equal(policy, system.DimensionViews.HasValue ? (int)system.DimensionViews.Value : (int?)null);
            AssertViewsFollow(policy, system, twoFondos: true);
        }
    }
}
