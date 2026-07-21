using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using RackCad.Application.Bom;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;

namespace RackCad.Plugin.KindHandlers
{
    /// <summary>Cabecera (frame) rack. Façade over the existing cabecera edit entry point, header BOM builder and
    /// project store; the bodies are moved verbatim from the former RACKEDITAR / RACKBOMTOTAL / restamp switches.</summary>
    internal sealed class CabeceraKindHandler : IRackKindHandler
    {
        public string Kind => RackEmbedDocument.KindCabecera;

        public string BomLabel => "Cabecera";

        public void Edit(Document document, ObjectId blockId, RackEmbedDocument embed)
            => RackCabeceraCommands.EditCabecera(document, blockId, embed);

        public BillOfMaterials BuildBom(RackEmbedDocument embed, RackCatalog catalog)
        {
            var header = new RackProjectStore().Deserialize(embed.Design)?.Header;
            return header == null ? null : BomBuilder.Build(header, catalog);
        }

        public string RestampDesign(string designJson, string newId, string copyName)
        {
            var store = new RackProjectStore();
            var project = store.Deserialize(designJson);
            if (project?.Header == null)
            {
                return designJson;
            }

            project.Header.Name = copyName;
            return store.Serialize(project);
        }
    }
}
