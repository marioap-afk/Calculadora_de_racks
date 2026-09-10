using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using RackCad.Application.Bom;
using RackCad.Application.Catalogs;
using RackCad.Application.Diagnostics;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Selective;

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

        /// <summary>
        /// authored + registro -> EFECTIVO -> geometria -> BOM (I-47 G13).
        ///
        /// <para>
        /// El paso nuevo es el del medio, y es el que faltaba: antes esto llamaba a <c>ToDomain()</c>, que es
        /// exactamente el caso SIN vinculo, asi que un rack vinculado se DIBUJABA con la variable y se COTIZABA
        /// con su literal congelado. Dos numeros para una misma propiedad, y nada fallando.
        /// </para>
        /// <para>
        /// El resolver corre aqui y UNA sola vez. No corre en el comando: si el total resolviera por su cuenta
        /// habria dos sitios que convierten un vinculo en un numero, que es la duplicacion que
        /// <see cref="SelectiveEffectiveDesignResolver"/> existe para impedir.
        /// </para>
        /// </summary>
        public BomBuildResult BuildBom(
            RackEmbedDocument embed, RackCatalog catalog, ProjectVariablesDocument projectVariables)
        {
            SelectivePalletDesignDocument authored;

            try
            {
                authored = new SelectivePalletDesignStore().Deserialize(embed.Design);
            }
            catch (System.Exception ex)
            {
                // El store SEÑALA lanzando; aqui se convierte en el estado tipado que le corresponde.
                RackLog.Exception("Leer el diseno selectivo para el BOM", ex);
                return BomBuildResult.UnreadablePayload(embed.Id, embed.Name, ex.Message);
            }

            if (authored == null)
            {
                return BomBuildResult.UnreadablePayload(
                    embed.Id, embed.Name, "El diseno embebido no se pudo interpretar.");
            }

            var resolution = new SelectiveEffectiveDesignResolver().Resolve(authored, projectVariables);

            if (!resolution.IsSuccess)
            {
                // El diseno se lee perfectamente: lo que falta es la VARIABLE. Degradarlo a payload ilegible
                // le quitaria al usuario la unica informacion que lleva a una reparacion.
                return BomBuildResult.BrokenProjectVariableReference(
                    embed.Id, embed.Name, resolution.PropertyId.Value, resolution.VariableId, resolution.Error);
            }

            var system = new SelectiveGeometryResolver().Resolve(resolution.Design, catalog);
            return BomBuildResult.Success(SelectiveBomBuilder.Build(system, catalog));
        }

        /// <summary>El rack selectivo no publica diagnosticos bloqueantes propios: su salida no se filtra aqui (I-42/H11).</summary>
        public string OutputBlockedReason(RackEmbedDocument embed, RackCatalog catalog) => null;

        /// <summary>
        /// I-47 G14 — la transformacion entera vive en Application, donde se puede demostrar que el vinculo, el
        /// literal congelado, la version y los campos desconocidos sobreviven a la copia.
        /// </summary>
        public RestampResult RestampDesign(string designJson, string newId, string copyName)
            => SelectiveAuthoredRestamp.Restamp(designJson, newId, copyName);
    }
}
