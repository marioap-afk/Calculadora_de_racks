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
            var header = new RackProjectStore().Deserialize(embed.Design)?.Header;
            return header == null ? null : BomBuilder.Build(header, catalog);
        }

        /// <summary>La cabecera no publica diagnosticos bloqueantes propios: su salida no se filtra aqui (I-42/H11).</summary>
        public string OutputBlockedReason(RackEmbedDocument embed, RackCatalog catalog) => null;

        public RestampResult RestampDesign(string designJson, string newId, string copyName)
        {
            var store = new RackProjectStore();
            RackProject project;

            try
            {
                project = store.Deserialize(designJson);
            }
            catch (System.Exception ex)
            {
                // I-47 G14: fallar es un valor, no un JSON original devuelto como si nada hubiera pasado.
                return RestampResult.Failure(ex.Message);
            }

            if (project?.Header == null)
            {
                return RestampResult.Success(designJson);
            }

            project.Header.Name = copyName;
            return RestampResult.Success(store.Serialize(project));
        }
    }
}
