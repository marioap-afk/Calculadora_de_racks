using System;
using System.Collections.Generic;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;
using Xunit;
using static RackCad.Tests.DimensionViewScenarios;

namespace RackCad.Tests
{
    /// <summary>
    /// I-50, T-11 (G5) — el DTO del Selectivo (C-06) persiste la política EXACTA. <c>null</c> no se escribe y sigue
    /// siendo legacy; cualquier <c>int</c> presente —incluidos <c>13</c>, <c>-1</c> y <c>-8</c>— vuelve igual en el
    /// número escrito y en el dominio, y ninguno vuelve como null. Un JSON no entero o fuera de Int32 sigue fallando
    /// la lectura como siempre. <c>WithDesign</c> y la exportación a biblioteca lo conservan por el <c>From</c> que ya
    /// usan, sin cambiarlos. El JSON byte a byte de un rack legacy lo fija <see cref="DimensionViewsLegacyJsonTests"/>.
    /// </summary>
    public class SelectiveDimensionViewsDocumentTests
    {
        private static readonly IReadOnlyList<int> PresentValues = new[] { 0, 1, 5, 7, 13, -1, -8 };

        private static SelectivePalletDesign Design(int? policy)
        {
            var design = SelectiveDesign(DimensionDetail.Standard, twoFondos: true);
            design.DimensionViews = Policy(policy);
            return design;
        }

        private static SelectivePalletDesignDocument Document(int? policy)
            => SelectivePalletDesignDocument.From(Design(policy), "I50-ID", RackName);

        [Fact]
        public void T11_Null_IsNotWritten_AndAPresentValueChangesTheJsonOnlyByItsOwnField()
        {
            var store = new SelectivePalletDesignStore();
            var legacy = store.Serialize(Document(null));
            Assert.DoesNotContain("DimensionViews", legacy, StringComparison.OrdinalIgnoreCase);

            foreach (var value in PresentValues)
            {
                var present = store.Serialize(Document(value));
                var member = FormattableString.Invariant($"\"DimensionViews\":{value},");
                Assert.Contains(member, present);
                Assert.Equal(legacy, present.Replace(member, string.Empty));
            }
        }

        [Fact]
        public void T11_EveryPresentValue_RoundTripsExactly_InTheWrittenNumberAndInTheDomain()
        {
            var store = new SelectivePalletDesignStore();
            foreach (var value in PresentValues)
            {
                var document = Document(value);
                Assert.Equal(value, document.DimensionViews);

                var reread = store.Deserialize(store.Serialize(document));
                Assert.Equal(value, reread.DimensionViews);

                var domain = reread.ToDomain();
                Assert.True(domain.DimensionViews.HasValue, $"{value} volvió como null");
                Assert.Equal(value, (int)domain.DimensionViews.Value);
            }
        }

        [Fact]
        public void T11_Null_StaysNull_InBothDirections_AndAFieldlessJsonLoadsAsLegacy()
        {
            var store = new SelectivePalletDesignStore();
            var document = Document(null);
            Assert.Null(document.DimensionViews);

            var json = store.Serialize(document);
            Assert.DoesNotContain("DimensionViews", json, StringComparison.OrdinalIgnoreCase);

            var reread = store.Deserialize(json);
            Assert.Null(reread.DimensionViews);
            Assert.Null(reread.ToDomain().DimensionViews);
        }

        [Theory]
        [InlineData("1.5")]
        [InlineData("\"13\"")]
        [InlineData("true")]
        [InlineData("2147483648")]
        [InlineData("-2147483649")]
        public void T11_ANonIntegerOrOutOfRangeValue_KeepsFailingToRead(string raw)
        {
            var store = new SelectivePalletDesignStore();
            var json = store.Serialize(Document(13)).Replace("\"DimensionViews\":13", "\"DimensionViews\":" + raw);
            Assert.Contains("\"DimensionViews\":" + raw, json);

            Assert.Throws<InvalidOperationException>(() => store.Deserialize(json));
        }

        [Fact]
        public void T11_WithDesign_CarriesThePolicy_ThroughTheFromItAlreadyUses()
        {
            var store = new SelectivePalletDesignStore();
            foreach (var value in new int?[] { null, 13, -1, -8 })
            {
                var loaded = store.Deserialize(store.Serialize(Document(value)));
                Assert.Equal(value, loaded.WithDesign(loaded.ToDomain()).DimensionViews);
            }

            var legacyLoaded = store.Deserialize(store.Serialize(Document(null)));
            Assert.Equal(-8, legacyLoaded.WithDesign(Design(-8)).DimensionViews);
            Assert.Null(store.Deserialize(store.Serialize(Document(13))).WithDesign(Design(null)).DimensionViews);
        }

        [Fact]
        public void T11_TheLibraryExport_CarriesThePolicy_ThroughTheFromItAlreadyUses()
        {
            foreach (var value in new int?[] { null, 13, -1, -8 })
            {
                Assert.Equal(value, SelectiveLibraryExport.FromEffective(Design(value), "I50-ID", RackName).DimensionViews);

                var exported = SelectiveLibraryExport.Materialize(Document(value), null);
                Assert.True(exported.IsSuccess, exported.Error);
                Assert.Equal(value, exported.Document.DimensionViews);
            }
        }
    }
}
