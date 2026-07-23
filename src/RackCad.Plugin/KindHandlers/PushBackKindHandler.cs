using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using RackCad.Application.Bom;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.Systems;

namespace RackCad.Plugin.KindHandlers
{
    /// <summary>Push Back (I-18) rack system. Thin façade like the other handlers: <see cref="Edit"/> forwards to the single
    /// Push Back edit entry point, <see cref="BuildBom"/> resolves the embedded design with the PUSH BACK resolver + BOM
    /// builder (never the dynamic ones, even though Push Back composes the dynamic structure). The Push Back design carries
    /// no display identity of its own — the GUID and name live in the envelope — so an independent copy's restamp is a
    /// no-op: <see cref="RackEnvelopeRestamp"/> re-stamps the envelope, the inner JSON stays byte-for-byte intact.</summary>
    internal sealed class PushBackKindHandler : IRackKindHandler
    {
        public string Kind => RackEmbedDocument.KindPushBack;

        public string BomLabel => "Push Back";

        public void Edit(Document document, ObjectId blockId, RackEmbedDocument embed)
            => RackPushBackCommands.EditPushBack(document, blockId, embed);

        public BillOfMaterials BuildBom(RackEmbedDocument embed, RackCatalog catalog)
        {
            var project = new RackProjectStore().Deserialize(embed.Design);
            if (project?.PushBackDesign == null)
            {
                return null;
            }

            var system = new PushBackResolver(catalog).Resolve(project.PushBackDesign);
            return PushBackBomBuilder.Build(system, catalog);
        }

        public string RestampDesign(string designJson, string newId, string copyName) => designJson;
    }
}
