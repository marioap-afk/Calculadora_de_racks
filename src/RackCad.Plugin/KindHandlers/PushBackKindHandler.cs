using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using RackCad.Application.Bom;
using RackCad.Application.Catalogs;
using RackCad.Application.ComputedParameters;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.PushBack;

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
            if (project?.PushBackDesign == null)
            {
                return null;
            }

            var system = new PushBackResolver(catalog).Resolve(project.PushBackDesign);
            return PushBackBomBuilder.Build(system, catalog);
        }

        /// <summary>
        /// I-42 (A1C/H11) — el motivo por el que este Push Back no puede cotizarse, si lo hay. Lo decide
        /// <see cref="RackBomOutputGate"/> sobre los MISMOS diagnosticos que bloquean los botones del editor.
        ///
        /// <para>
        /// I-63 D-27 — la composicion de la puerta vive en UN solo sitio, <see cref="RackOutputVerdict"/>, que
        /// resuelve el diseno con el resolver de Push Back y toma <c>RackBomOutputGate.For(system).Reason</c>.
        /// Este metodo ya no compone nada: delega en la via del handler de esa funcion, que normaliza el catalogo
        /// nulo a un catalogo vacio y conserva el <i>fail-open</i> (ilegible o indeterminado: <c>null</c>).
        /// </para>
        /// </summary>
        public string OutputBlockedReason(RackEmbedDocument embed, RackCatalog catalog)
            => RackOutputVerdict.HandlerBlockedReason(Kind, embed?.Design, catalog);

        public RestampResult RestampDesign(string designJson, string newId, string copyName) => RestampResult.Success(designJson);
    }
}
