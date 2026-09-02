using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using RackCad.Application.Bom;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.StructuralSections;
using RackCad.Application.StructuralSections.Geometry;
using RackCad.Application.Systems.Cantilever;

namespace RackCad.Plugin.KindHandlers
{
    /// <summary>
    /// Cantilever LINE (I-37D). A thin façade like the other handlers: <see cref="Edit"/> forwards to the single
    /// Cantilever edit entry point, and <see cref="BuildBom"/> resolves the embedded design with the CANTILEVER
    /// resolver and BOM builder.
    ///
    /// The <paramref name="catalog"/> the interface hands it is the LEGACY product catalogue, which Cantilever does
    /// not consume: the line is built from the neutral section catalogue of I-36, so this handler loads that one
    /// itself. It is the one kind whose BOM depends on a catalogue the interface does not carry, and inventing a
    /// second parameter for every other kind to ignore would be worse than loading it here.
    /// </summary>
    internal sealed class CantileverKindHandler : IRackKindHandler
    {
        public string Kind => RackEmbedDocument.KindCantilever;

        public string BomLabel => "Cantilever";

        public void Edit(Document document, ObjectId blockId, RackEmbedDocument embed)
            => RackCantileverCommands.EditCantilever(document, blockId, embed);

        public BillOfMaterials BuildBom(RackEmbedDocument embed, RackCatalog catalog)
        {
            var project = new RackProjectStore().Deserialize(embed.Design);

            if (project?.CantileverLineDesign == null)
            {
                return null;
            }

            // FAIL CLOSED through the Plugin's single owner of that policy, as a best-effort skip: quoting a line
            // whose dimensions failed validation is worse than leaving it out of the total, and the interface's
            // null contract is exactly "this rack could not be turned into a BOM". There is no Editor to write to
            // from a BOM build, so the reason is not printed here — the caller reports the skipped rack.
            if (!StructuralSectionCatalogAccess.TryLoad(null, out var sections))
            {
                return null;
            }

            var line = new CantileverLineEditorAssembler(sections).Build(project.CantileverLineDesign);

            return line.IsValid ? line.Bom : null;
        }

        /// <summary>
        /// Re-stamps an INDEPENDENT copy's inner identity.
        ///
        /// Unlike Push Back, a Cantilever design carries its OWN identity — one GUID per LINE, shared by its three
        /// views — so a copy that kept it would be a second line claiming to be the first. The envelope GUID is
        /// re-stamped by <c>RackEnvelopeRestamp</c>; this is the inner half of the same move.
        /// </summary>
        /// <summary>El cantilever no publica diagnosticos bloqueantes propios: su salida no se filtra aqui (I-42/H11).</summary>
        public string OutputBlockedReason(RackEmbedDocument embed, RackCatalog catalog) => null;

        public string RestampDesign(string designJson, string newId, string copyName)
        {
            var store = new RackProjectStore();
            var project = store.Deserialize(designJson);

            if (project?.CantileverLineDesign == null)
            {
                return designJson; // not a Cantilever payload: leave it byte-for-byte intact
            }

            var design = project.CantileverLineDesign;

            // The envelope's id is a string GUID; a copy whose id does not parse still gets a NEW identity rather
            // than keeping the original's, because sharing one is the defect being prevented.
            design.Id = Guid.TryParse(newId, out var parsed) ? parsed : Guid.NewGuid();
            design.Name = copyName;

            return store.Serialize(RackProject.ForCantilever(design).WithSourceMetadataFrom(project));
        }
    }
}
