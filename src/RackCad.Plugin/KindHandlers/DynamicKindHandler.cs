using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using RackCad.Application.Bom;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Dynamic;

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
            var project = new RackProjectStore().Deserialize(embed.Design);
            var system = project?.DynamicDesign == null
                ? project?.DynamicSystem
                : new DynamicRackSystemResolver(catalog).Resolve(project.DynamicDesign).System;
            return system == null ? null : SystemBomBuilder.Build(system, catalog);
        }

        /// <summary>El rack dinamico no publica diagnosticos bloqueantes propios: su salida no se filtra aqui (I-42/H11).</summary>
        public string OutputBlockedReason(RackEmbedDocument embed, RackCatalog catalog) => null;

        public string RestampDesign(string designJson, string newId, string copyName) => designJson;
    }
}
