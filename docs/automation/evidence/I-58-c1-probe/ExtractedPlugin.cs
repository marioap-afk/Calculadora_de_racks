// Diagnostic reconstruction ONLY. Exact method text extracted from sources listed in README.
using RackCad.Application.Persistence;
using RackCad.Domain.Systems.Cantilever;
internal sealed class ExtractedPlugin
{
        internal static string BuildCantileverPayload(
            CantileverLineDesign design,
            string id,
            string name,
            string view = RackEmbedDocument.ViewFrontal,
            int section = -1,
            RackEmbedDocument source = null,
            RackProject innerSource = null)
        {
            if (design == null)
            {
                return null;
            }

            // The inner Design of a Cantilever block is itself a RackProjectDocument — a boundary INDEPENDENT of
            // the envelope (I-11). innerSource is the ALREADY-RESOLVED source project (null for a fresh one, or the
            // library/initiating project); WithSourceMetadataFrom preserves its unknown fields + non-downgraded
            // version. The RESOLVED line is deliberately not persisted: the document carries the intention.
            var designJson = new RackProjectStore().Serialize(
                RackProject.ForCantilever(design).WithSourceMetadataFrom(innerSource));

            var embed = RackEmbedComposer.Compose(
                source, RackEmbedDocument.KindCantilever, id, name,
                string.IsNullOrWhiteSpace(view) ? RackEmbedDocument.ViewFrontal : view, section, designJson);

            return new RackEmbedStore().Serialize(embed);
        }

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
                // I-47 G14: un diseno que no se puede leer NO se copia tal cual. La copia saldria con la
                // identidad vieja dentro y una nueva fuera.
                return RestampResult.Failure(ex.Message);
            }

            if (project?.CantileverLineDesign == null)
            {
                return RestampResult.Success(designJson); // not a Cantilever payload: leave it byte-for-byte intact
            }

            var design = project.CantileverLineDesign;

            // The envelope's id is a string GUID; a copy whose id does not parse still gets a NEW identity rather
            // than keeping the original's, because sharing one is the defect being prevented.
            design.Id = Guid.TryParse(newId, out var parsed) ? parsed : Guid.NewGuid();
            design.Name = copyName;

            return RestampResult.Success(
                store.Serialize(RackProject.ForCantilever(design).WithSourceMetadataFrom(project)));
        }

        public static RestampResult RestampEnvelope(string payload, string copyName, Guid newId)
        {
            if (newId == Guid.Empty)
            {
                return RestampResult.Failure("La copia necesita una identidad nueva y recibio un GUID vacio.");
            }

            var store = new RackEmbedStore();
            var embed = store.Deserialize(payload);

            if (embed == null)
            {
                // The drawing-wide scan tolerates an envelope it cannot read; an independent copy cannot be built on one.
                return RestampResult.Failure("Los datos del rack de origen no se pueden leer: no se crea la copia.");
            }

            var newIdText = newId.ToString();

            if (string.Equals(embed.Id, newIdText, StringComparison.OrdinalIgnoreCase))
            {
                return RestampResult.Failure("La identidad nueva coincide con la del rack de origen: no se crea la copia.");
            }

            embed.Id = newIdText;
            embed.Name = copyName;

            var design = RestampDesign(embed.Kind, embed.Design, newIdText, copyName);

            if (!design.IsSuccess)
            {
                // The envelope was already given a new identity in memory; it never reaches the drawing.
                return design;
            }

            embed.Design = design.DesignJson;
            return RestampResult.Success(store.Serialize(embed));
        }
    // Harness-only dispatch: no AutoCAD registry/host is exercised.
    private static RestampResult RestampDesign(string kind, string json, string id, string name)
    {
        if (kind != RackEmbedDocument.KindCantilever) throw new InvalidOperationException("Only Cantilever is in scope");
        return new ExtractedPlugin().RestampDesign(json, id, name);
    }
}
