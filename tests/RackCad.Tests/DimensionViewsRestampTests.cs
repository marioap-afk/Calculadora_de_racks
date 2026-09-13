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
    /// <para>
    /// El Dinámico y Push Back no re-estampan su diseño: sus handlers del Plugin devuelven
    /// <c>RestampResult.Success(designJson)</c> y solo cambia el sobre. Esas líneas viven en el Plugin y estas pruebas
    /// NO las ejecutan; fijan el contrato de Application sobre el que se apoyan: el sobre (<see cref="RackEmbedStore"/>)
    /// conserva el JSON del proyecto byte a byte y <see cref="RackProjectStore"/> lee de él la política exacta.
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

        // ---- Dinámico y Push Back ------------------------------------------------------------------------------

        private static string DynamicProjectJson(int? policy)
        {
            var design = DynamicPersistedDesign(DimensionDetail.Standard, Catalog);
            design.DimensionViews = Policy(policy);
            return new RackProjectStore().Serialize(RackProject.ForDynamic(design));
        }

        private static string PushBackProjectJson(int? policy)
        {
            var design = PushBackSingleSidedDesign(DimensionDetail.Standard);
            design.Structure.DimensionViews = Policy(policy);
            return new RackProjectStore().Serialize(RackProject.ForPushBack(design));
        }

        /// <summary>
        /// La copia de un kind cuyo handler no re-estampa el diseño: identidad nueva en el SOBRE y el diseño que devuelve
        /// <c>RestampResult.Success</c>, escrita y releída con el mismo store del sobre.
        /// </summary>
        private static RackEmbedDocument CopyOf(string kind, string designJson)
        {
            var store = new RackEmbedStore();
            var source = store.Serialize(new RackEmbedDocument { Kind = kind, Id = "ORIGEN-ID", Name = RackName, Design = designJson });

            var envelope = store.Deserialize(source);
            envelope.Id = "COPIA-ID";
            envelope.Name = RackName + " copia";
            var design = RestampResult.Success(envelope.Design);
            Assert.True(design.IsSuccess, design.Error);
            envelope.Design = design.DesignJson;

            return store.Deserialize(store.Serialize(envelope));
        }

        [Theory]
        [InlineData(13)]
        [InlineData(-1)]
        [InlineData(-8)]
        public void T15_Dynamic_TheCopyKeepsTheProjectJsonByteForByte_AndThePolicyExact(int policy)
        {
            var json = DynamicProjectJson(policy);
            var copy = CopyOf(RackEmbedDocument.KindDynamic, json);

            Assert.Equal("COPIA-ID", copy.Id);
            Assert.Equal(json, copy.Design);
            var project = new RackProjectStore().Deserialize(copy.Design);
            Assert.Equal(policy, (int)project.DynamicDesign.DimensionViews.Value);
            Assert.Equal(policy, (int)project.DynamicSystem.DimensionViews.Value);
        }

        [Theory]
        [InlineData(13)]
        [InlineData(-1)]
        [InlineData(-8)]
        public void T15_PushBack_TheCopyKeepsTheProjectJsonByteForByte_AndThePolicyExact(int policy)
        {
            var json = PushBackProjectJson(policy);
            var copy = CopyOf(RackEmbedDocument.KindPushBack, json);

            Assert.Equal("COPIA-ID", copy.Id);
            Assert.Equal(json, copy.Design);
            Assert.Equal(policy, (int)new RackProjectStore().Deserialize(copy.Design).PushBackDesign.Structure.DimensionViews.Value);
        }

        [Fact]
        public void T15_DynamicAndPushBack_TheCopyOfALegacyRack_StaysLegacy()
        {
            var dynamic = CopyOf(RackEmbedDocument.KindDynamic, DynamicProjectJson(null));
            Assert.DoesNotContain("DimensionViews", dynamic.Design, System.StringComparison.OrdinalIgnoreCase);
            var dynamicProject = new RackProjectStore().Deserialize(dynamic.Design);
            Assert.Null(dynamicProject.DynamicDesign.DimensionViews);
            Assert.Null(dynamicProject.DynamicSystem.DimensionViews);

            var pushBack = CopyOf(RackEmbedDocument.KindPushBack, PushBackProjectJson(null));
            Assert.DoesNotContain("DimensionViews", pushBack.Design, System.StringComparison.OrdinalIgnoreCase);
            Assert.Null(new RackProjectStore().Deserialize(pushBack.Design).PushBackDesign.Structure.DimensionViews);
        }
    }
}
