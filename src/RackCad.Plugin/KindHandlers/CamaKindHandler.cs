using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using RackCad.Application.Bom;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.FlowBed;

namespace RackCad.Plugin.KindHandlers
{
    /// <summary>Cama de rodamiento (flow bed) rack. Façade over the existing cama edit entry point and BOM pipeline;
    /// the bodies are moved verbatim from the former RACKEDITAR / RACKBOMTOTAL switches. The cama design carries no
    /// display identity of its own, so an independent copy's restamp is a no-op (the envelope name is enough).</summary>
    internal sealed class CamaKindHandler : IRackKindHandler
    {
        public string Kind => RackEmbedDocument.KindCama;

        public string BomLabel => "Cama";

        public void Edit(Document document, ObjectId blockId, RackEmbedDocument embed)
            => RackCamaCommands.EditCama(document, blockId, embed);

        /// <summary>
        /// I-47 G13 — el contrato compartido lleva ahora el registro de variables del dibujo. Este kind no tiene
        /// vinculos en ID22A, asi que lo IGNORA: resolver donde nada esta vinculado seria trabajo inventado para
        /// parecer uniforme.
        /// </summary>
        public BomBuildResult BuildBom(
            RackEmbedDocument embed, RackCatalog catalog, ProjectVariablesDocument projectVariables)
        {
            var bom = Build(embed, catalog);

            return bom == null
                ? BomBuildResult.UnreadablePayload(embed.Id, embed.Name, "El diseno embebido no se pudo interpretar.")
                : BomBuildResult.Success(bom);
        }

        private BillOfMaterials Build(RackEmbedDocument embed, RackCatalog catalog)
        {
            var config = new FlowBedConfigurationStore().Deserialize(embed.Design);
            if (config == null)
            {
                return null;
            }

            var instances = new FlowBedLateralBuilder().Build(config, catalog);
            return FlowBedBomBuilder.Build(instances, catalog);
        }

        /// <summary>La cama no publica diagnosticos bloqueantes propios: su salida no se filtra aqui (I-42/H11).</summary>
        public string OutputBlockedReason(RackEmbedDocument embed, RackCatalog catalog) => null;

        public RestampResult RestampDesign(string designJson, string newId, string copyName) => RestampResult.Success(designJson);
    }
}
