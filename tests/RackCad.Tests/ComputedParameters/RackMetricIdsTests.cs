using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RackCad.Application.ComputedParameters;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-63 G1 — D-02 (identidad MetricId) y D-05 (catalogo V1 cerrado). El oraculo es la tabla congelada del
    /// Freeze (Proposal V3 secciones 5 y 6), no el codigo productivo.
    /// </summary>
    public class ComputedParametersRackMetricIdsTests
    {
        private static readonly (MetricScope Scope, string Token)[] Frozen =
        {
            (MetricScope.Rack, "frentes"),
            (MetricScope.Rack, "frentesVacios"),
            (MetricScope.Project, "totalRacks"),
            (MetricScope.Project, "rackCount"),
            (MetricScope.Project, "totalFrentes"),
            (MetricScope.Project, "totalFrentesVacios"),
        };

        public static IEnumerable<object[]> FrozenPairs()
            => Frozen.Select(p => new object[] { p.Scope, p.Token });

        [Theory]
        [MemberData(nameof(FrozenPairs))]
        public void ScopeAndToken_MapToAMetricIdInTheProductiveCatalog(MetricScope scope, string token)
        {
            var id = new MetricId(scope, token);

            Assert.Contains(id, RackMetricIds.All);
            Assert.Single(RackMetricIds.All.Where(m => m == id));
        }

        [Fact]
        public void EveryProductiveMetricId_MapsBackToAFrozenPair()
        {
            foreach (var metric in RackMetricIds.All)
            {
                Assert.Contains(Frozen, p => p.Scope == metric.Scope && p.Token == metric.Token);
            }
        }

        [Fact]
        public void Catalog_IsClosedAtExactlySixDistinctMetricIds()
        {
            Assert.Equal(6, RackMetricIds.All.Count);
            Assert.Equal(6, RackMetricIds.All.Distinct().Count());

            var productive = RackMetricIds.All.Select(m => (m.Scope, m.Token)).ToHashSet();
            var frozen = Frozen.ToHashSet();
            Assert.True(productive.SetEquals(frozen));
        }

        [Fact]
        public void ProductiveTokens_FollowTheD02TokenRule()
        {
            var rule = new Regex("^[a-z][A-Za-z0-9]*$", RegexOptions.CultureInvariant);

            foreach (var metric in RackMetricIds.All)
            {
                Assert.Matches(rule, metric.Token);
            }
        }

        [Fact]
        public void TokenComparison_IsOrdinalAndCaseSensitive()
        {
            foreach (var metric in RackMetricIds.All)
            {
                var other = char.ToUpperInvariant(metric.Token[0]) + metric.Token.Substring(1);

                Assert.NotEqual(metric.Token, other);
                Assert.False(metric == new MetricId(metric.Scope, other));
                Assert.True(metric != new MetricId(metric.Scope, other));
                Assert.False(metric.Equals(new MetricId(metric.Scope, other)));
                Assert.False(metric.Equals(new MetricId(metric.Scope, metric.Token.ToUpperInvariant())));
            }
        }
    }
}
