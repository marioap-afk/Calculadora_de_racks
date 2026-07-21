using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using RackCad.Application.Bom;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.Systems;

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

        public BillOfMaterials BuildBom(RackEmbedDocument embed, RackCatalog catalog)
        {
            var config = new FlowBedConfigurationStore().Deserialize(embed.Design);
            if (config == null)
            {
                return null;
            }

            var instances = new FlowBedLateralBuilder().Build(config, catalog);
            return FlowBedBomBuilder.Build(instances, catalog);
        }

        public string RestampDesign(string designJson, string newId, string copyName) => designJson;
    }
}
