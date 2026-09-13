using System.Collections.Generic;
using System.Linq;
using RackCad.Domain.Systems.Shared;
using Xunit;
using static RackCad.Tests.DimensionViewScenarios;

namespace RackCad.Tests
{
    /// <summary>
    /// I-50, T-10 (G4, guarda) — <c>Dimensions = None</c> SIEMPRE gana: con cualquier política, incluida la ausencia y
    /// los valores con bits desconocidos o de signo, ninguna vista emite una sola cota.
    ///
    /// <para>
    /// Cubre los sistemas a los que la política llega en este gate asignándola al sistema resuelto: el Selectivo (uno y
    /// dos fondos), el Dinámico y Push Back de UN sentido, cuyas vistas se construyen sobre su propia estructura. El
    /// compuesto A/B no se incluye: sus lados se dibujan sobre sub-estructuras a las que la política solo llega por la
    /// copia compartida C-15, que es de G6 (T-08).
    /// </para>
    /// </summary>
    public class DimensionViewsNoneWinsTests
    {
        public static IEnumerable<object[]> AnyPolicy()
            => new int?[] { null }.Concat(Policies.Select(policy => (int?)policy)).Select(policy => new object[] { policy });

        [Theory]
        [MemberData(nameof(AnyPolicy))]
        public void T10_Selective_NoneWins_WhateverThePolicy(int? policy)
        {
            var catalog = Catalog;
            foreach (var twoFondos in new[] { false, true })
            {
                var active = SelectiveViews(Selective(DimensionDetail.Detailed, twoFondos, catalog), catalog);
                Assert.All(active, view => Assert.True(DimensionLegacySignature.DimensionCount(view.Instances) > 0, view.Key));

                var system = Selective(DimensionDetail.None, twoFondos, catalog);
                system.DimensionViews = Policy(policy);
                Assert.All(SelectiveViews(system, catalog), view => Assert.Equal(0, DimensionLegacySignature.DimensionCount(view.Instances)));
            }
        }

        [Theory]
        [MemberData(nameof(AnyPolicy))]
        public void T10_Dynamic_NoneWins_WhateverThePolicy(int? policy)
        {
            var catalog = Catalog;
            var active = DynamicViews(Dynamic(DimensionDetail.Detailed, catalog), catalog);
            Assert.All(active, view => Assert.True(DimensionLegacySignature.DimensionCount(view.Instances) > 0, view.Key));

            var system = Dynamic(DimensionDetail.None, catalog);
            system.DimensionViews = Policy(policy);
            Assert.All(DynamicViews(system, catalog), view => Assert.Equal(0, DimensionLegacySignature.DimensionCount(view.Instances)));
        }

        [Theory]
        [MemberData(nameof(AnyPolicy))]
        public void T10_PushBackSingleSided_NoneWins_WhateverThePolicy(int? policy)
        {
            var catalog = Catalog;
            var active = PushBackSingleSidedViews(PushBackSingleSided(DimensionDetail.Detailed, catalog), catalog);
            Assert.All(active, view => Assert.True(DimensionLegacySignature.DimensionCount(view.Instances) > 0, view.Key));

            var system = PushBackSingleSided(DimensionDetail.None, catalog);
            Assert.False(system.IsComposite);
            system.Structure.DimensionViews = Policy(policy);
            Assert.All(PushBackSingleSidedViews(system, catalog), view => Assert.Equal(0, DimensionLegacySignature.DimensionCount(view.Instances)));
        }
    }
}
