using System.Linq;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using RackCad.Domain.Systems.Shared;
using Xunit;
using static RackCad.Tests.DimensionViewScenarios;

namespace RackCad.Tests
{
    /// <summary>
    /// I-50, T-14 (G5, guarda) — la autoridad multivista del Selectivo, SIN tocarla. Compara el documento persistido
    /// incluyendo por defecto todo campo declarado, así que la política participa sola: hermanas que coinciden también
    /// en ella son UNA autoridad, y documentos idénticos salvo la política son DIVERGENTES. La comparación es de los
    /// NÚMEROS del documento: <c>13</c> y <c>5</c> dibujan las mismas vistas y aun así son autoridades distintas, igual
    /// que <c>-1</c> y <c>7</c>.
    /// </summary>
    public class SelectiveDimensionViewsAuthorityTests
    {
        private const string RackId = "I50-ID";

        private static SelectivePalletDesignDocument Document(int? policy)
        {
            var design = SelectiveDesign(DimensionDetail.Standard, twoFondos: true);
            design.DimensionViews = Policy(policy);
            return SelectivePalletDesignDocument.From(design, RackId, RackName);
        }

        private static AuthoredAuthorityResult Resolve(params SelectivePalletDesignDocument[] siblings)
            => SelectiveAuthoredAuthority.Resolve(
                RackId,
                siblings.Select((document, index) => ProjectVariableScanEntry.Selective("vista-" + index, RackId, document)).ToList());

        [Theory]
        [InlineData(null)]
        [InlineData(0)]
        [InlineData(7)]
        [InlineData(13)]
        [InlineData(-1)]
        [InlineData(-8)]
        public void T14_SiblingsThatAgreeAlsoOnThePolicy_AreOneAuthority(int? policy)
        {
            var store = new SelectivePalletDesignStore();
            var fromDisk = store.Deserialize(store.Serialize(Document(policy)));

            var result = Resolve(Document(policy), fromDisk, Document(policy));

            Assert.Equal(AuthoredAuthorityOutcome.Single, result.Outcome);
        }

        [Theory]
        [InlineData(null, 7)]
        [InlineData(0, null)]
        [InlineData(13, -8)]
        [InlineData(13, 5)]
        [InlineData(-1, 7)]
        [InlineData(-8, 0)]
        public void T14_DocumentsIdenticalExceptForThePolicy_AreDivergent(int? left, int? right)
        {
            var result = Resolve(Document(left), Document(right));

            Assert.Equal(AuthoredAuthorityOutcome.Divergent, result.Outcome);
        }
    }
}
