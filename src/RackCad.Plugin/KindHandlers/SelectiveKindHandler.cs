using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using RackCad.Application.Bom;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.Systems;

namespace RackCad.Plugin.KindHandlers
{
    /// <summary>Selective pallet rack. Façade over the existing selective edit entry point, BOM pipeline and design
    /// store; the bodies are moved verbatim from the former RACKEDITAR / RACKBOMTOTAL / restamp switches.</summary>
    internal sealed class SelectiveKindHandler : IRackKindHandler
    {
        public string Kind => RackEmbedDocument.KindSelective;

        public string BomLabel => "Selectivo";

        public void Edit(Document document, ObjectId blockId, RackEmbedDocument embed)
            => RackSelectivoCommands.EditSelective(document, blockId, embed);

        public BillOfMaterials BuildBom(RackEmbedDocument embed, RackCatalog catalog)
        {
            var design = new SelectivePalletDesignStore().Deserialize(embed.Design)?.ToDomain();
            if (design == null)
            {
                return null;
            }

            var system = new SelectiveGeometryResolver().Resolve(design, catalog);
            return SelectiveBomBuilder.Build(system, catalog);
        }

        public string RestampDesign(string designJson, string newId, string copyName)
        {
            var store = new SelectivePalletDesignStore();
            var design = store.Deserialize(designJson);
            design.Id = newId;
            design.Name = copyName;
            return store.Serialize(design);
        }
    }
}
