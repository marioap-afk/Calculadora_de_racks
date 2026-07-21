using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using RackCad.Application.Bom;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.Systems;

namespace RackCad.Plugin.KindHandlers
{
    /// <summary>Dynamic (modular) rack system. Façade over the existing dynamic edit entry point and BOM pipeline;
    /// the bodies are moved verbatim from the former RACKEDITAR / RACKBOMTOTAL switches. The dynamic design carries
    /// no display identity of its own, so an independent copy's restamp is a no-op (the envelope name is enough).</summary>
    internal sealed class DynamicKindHandler : IRackKindHandler
    {
        public string Kind => RackEmbedDocument.KindDynamic;

        public string BomLabel => "Dinámico";

        public void Edit(Document document, ObjectId blockId, RackEmbedDocument embed)
            => RackDinamicoCommands.EditDynamic(document, blockId, embed);

        public BillOfMaterials BuildBom(RackEmbedDocument embed, RackCatalog catalog)
        {
            var project = new RackProjectStore().Deserialize(embed.Design);
            var system = project?.DynamicDesign == null
                ? project?.DynamicSystem
                : new DynamicRackSystemResolver(catalog).Resolve(project.DynamicDesign).System;
            return system == null ? null : SystemBomBuilder.Build(system, catalog);
        }

        public string RestampDesign(string designJson, string newId, string copyName) => designJson;
    }
}
