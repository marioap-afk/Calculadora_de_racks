using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using RackCad.Application.Persistence;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.Shared;
using Xunit;
using static RackCad.Tests.DimensionViewScenarios;

namespace RackCad.Tests
{
    /// <summary>
    /// I-50, T-12 (G5) — el DTO del Dinámico (C-11) persiste la política EXACTA en sus CUATRO mapeos:
    /// <c>From(system)</c>, <c>From(design)</c>, <c>ToDesign</c> y <c>ToDomain</c>. <c>null</c> no se escribe y sigue
    /// siendo legacy; cualquier <c>int</c> presente —incluidos <c>13</c>, <c>-1</c> y <c>-8</c>— vuelve igual por
    /// <c>RackProjectStore</c>, también dentro del <c>Structure</c> de Push Back, que usa el mismo documento.
    ///
    /// <para>
    /// Nombra y verifica las dos conversiones INDIRECTAS que dependen de C-11 sin código propio:
    /// <c>RackProject.ForDynamic(system)</c> (<c>From(system).ToDesign()</c>) y <c>RackProject.ForDynamic(design)</c>
    /// (<c>From(design).ToDomain()</c>, la que usa <c>BuildDynamicPayload</c>). <c>RackProject</c> no se toca. El JSON
    /// byte a byte de un rack legacy lo fija <see cref="DimensionViewsLegacyJsonTests"/>.
    /// </para>
    /// </summary>
    public class DynamicDimensionViewsDocumentTests
    {
        private static readonly IReadOnlyList<int> PresentValues = new[] { 0, 1, 5, 7, 13, -1, -8 };
        private static readonly IReadOnlyList<int?> EveryValue = new int?[] { null, 0, 1, 5, 7, 13, -1, -8 };

        private static int? AsInt(DimensionViewVisibility? value) => value.HasValue ? (int)value.Value : (int?)null;

        private static DynamicRackDesign Design(int? policy)
        {
            var design = DynamicDesign(DimensionDetail.Standard, Catalog);
            design.DimensionViews = Policy(policy);
            return design;
        }

        private static DynamicRackSystem ResolvedSystem(int? policy)
        {
            var system = Dynamic(DimensionDetail.Standard, Catalog);
            system.DimensionViews = Policy(policy);
            return system;
        }

        [Fact]
        public void T12_TheFourMappings_KeepEveryValueExact()
        {
            foreach (var value in EveryValue)
            {
                Assert.Equal(value, DynamicRackSystemDocument.From(ResolvedSystem(value)).DimensionViews);   // From(system)
                Assert.Equal(value, DynamicRackSystemDocument.From(Design(value)).DimensionViews);           // From(design)

                var document = DynamicRackSystemDocument.From(Design(null));
                document.DimensionViews = value;
                Assert.Equal(value, AsInt(document.ToDesign().DimensionViews));                             // ToDesign
                Assert.Equal(value, AsInt(document.ToDomain().DimensionViews));                             // ToDomain
            }
        }

        [Fact]
        public void T12_Null_IsNotWritten_AndAPresentValueChangesTheProjectJsonOnlyByItsOwnLine()
        {
            var store = new RackProjectStore();
            var legacy = store.Serialize(RackProject.ForDynamic(Design(null)));
            Assert.DoesNotContain("DimensionViews", legacy, StringComparison.OrdinalIgnoreCase);

            foreach (var value in PresentValues)
            {
                var present = store.Serialize(RackProject.ForDynamic(Design(value)));
                var line = new Regex("\\r?\\n[ ]*\"DimensionViews\": " + value.ToString(CultureInfo.InvariantCulture) + ",");
                Assert.Single(line.Matches(present));
                Assert.Equal(legacy, line.Replace(present, string.Empty));
            }
        }

        [Fact]
        public void T12_TheProjectStore_RoundTripsEveryValue_IntoTheDesignAndTheSystem()
        {
            var store = new RackProjectStore();
            foreach (var value in EveryValue)
            {
                var persisted = DynamicPersistedDesign(DimensionDetail.Standard, Catalog);
                persisted.DimensionViews = Policy(value);
                var reopened = store.Deserialize(store.Serialize(RackProject.ForDynamic(persisted)));

                Assert.Equal(value, AsInt(reopened.DynamicDesign.DimensionViews));
                Assert.Equal(value, AsInt(reopened.DynamicSystem.DimensionViews));
            }
        }

        [Fact]
        public void T12_ThePushBackStructure_RoundTripsEveryValue_ThroughTheSameDocument()
        {
            var store = new RackProjectStore();
            foreach (var value in EveryValue)
            {
                var design = PushBackSingleSidedDesign(DimensionDetail.Standard);
                design.Structure.DimensionViews = Policy(value);

                var json = store.Serialize(RackProject.ForPushBack(design));
                if (value == null)
                {
                    Assert.DoesNotContain("DimensionViews", json, StringComparison.OrdinalIgnoreCase);
                }

                Assert.Equal(value, AsInt(store.Deserialize(json).PushBackDesign.Structure.DimensionViews));
            }
        }

        [Fact]
        public void T12_RackProjectForDynamic_TheTwoIndirectConversions_KeepThePolicyExact()
        {
            foreach (var value in new int?[] { null, 13, -1, -8 })
            {
                Assert.Equal(value, AsInt(RackProject.ForDynamic(ResolvedSystem(value)).DynamicDesign.DimensionViews));   // From(system).ToDesign()
                Assert.Equal(value, AsInt(RackProject.ForDynamic(Design(value)).DynamicSystem.DimensionViews));           // From(design).ToDomain()
            }
        }

        [Theory]
        [InlineData("1.5")]
        [InlineData("\"13\"")]
        [InlineData("true")]
        [InlineData("2147483648")]
        public void T12_ANonIntegerOrOutOfRangeValue_KeepsFailingToRead(string raw)
        {
            var store = new RackProjectStore();
            var json = store.Serialize(RackProject.ForDynamic(Design(13))).Replace("\"DimensionViews\": 13", "\"DimensionViews\": " + raw);
            Assert.Contains("\"DimensionViews\": " + raw, json);

            Assert.Throws<InvalidOperationException>(() => store.Deserialize(json));
        }
    }
}
