using RackCad.Application.Persistence;
using RackCad.Domain.Systems.Shared;
using Xunit;
using static RackCad.Tests.DimensionViewScenarios;

namespace RackCad.Tests
{
    /// <summary>
    /// I-50, T-15 (G5, guarda) — <c>RACKDUPLICAR</c> re-estampa la identidad de la copia sin tocar la política. Archivo
    /// NUEVO a propósito: <c>SelectiveDuplicationFailClosedTests.cs</c> lo modifica I-51 en su rama.
    ///
    /// <para>
    /// El Selectivo re-estampa por <see cref="SelectiveAuthoredRestamp"/>, que hace ida y vuelta por el DOCUMENTO: un
    /// campo declarado sobrevive con su <c>int</c> exacto. No se toca <c>RackDuplicarCommands</c>,
    /// <c>RackEnvelopeRestamp</c>, ningún kind handler ni el Plugin.
    /// </para>
    /// </summary>
    public class DimensionViewsRestampTests
    {
        private static string SelectiveJson(int? policy)
        {
            var design = SelectiveDesign(DimensionDetail.Standard, twoFondos: true);
            design.DimensionViews = Policy(policy);
            return new SelectivePalletDesignStore().Serialize(SelectivePalletDesignDocument.From(design, "ORIGEN-ID", RackName));
        }

        [Theory]
        [InlineData(13)]
        [InlineData(-1)]
        [InlineData(-8)]
        public void T15_Selective_TheAuthoredRestamp_KeepsThePolicyExact(int policy)
        {
            var result = SelectiveAuthoredRestamp.Restamp(SelectiveJson(policy), "COPIA-ID", RackName + " copia");

            Assert.True(result.IsSuccess, result.Error);
            var copy = new SelectivePalletDesignStore().Deserialize(result.DesignJson);
            Assert.Equal("COPIA-ID", copy.Id);
            Assert.Equal(policy, copy.DimensionViews);
            Assert.Equal(policy, (int)copy.ToDomain().DimensionViews.Value);
        }

        [Fact]
        public void T15_Selective_TheAuthoredRestamp_KeepsALegacyRackLegacy()
        {
            var result = SelectiveAuthoredRestamp.Restamp(SelectiveJson(null), "COPIA-ID", RackName + " copia");

            Assert.True(result.IsSuccess, result.Error);
            Assert.DoesNotContain("DimensionViews", result.DesignJson, System.StringComparison.OrdinalIgnoreCase);
            Assert.Null(new SelectivePalletDesignStore().Deserialize(result.DesignJson).DimensionViews);
        }
    }
}
