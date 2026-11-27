using System;
using System.Globalization;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Shared;
using RackCad.Application.Workspace;
using RackCad.Domain.Systems.FlowBed;

namespace RackCad.Application.VentanaRackCad
{
    /// <summary>
    /// A write of a cama de rodamiento from the editor of the window (I-64, quinta tanda, part cm): the number of the request, the session it
    /// was made for, the rack, its name (trimmed; blank keeps the one of the envelope, I-61) and a COPY of the bed. With
    /// <paramref name="NewRack"/> the cama is not in the drawing yet: AutoCAD inserts it with the jig, with the identity the window gave it
    /// and, with a blank name, the automatic one of I-60 (RACKCAMANUEVO); otherwise it redraws the block in place (RACKCAMAAPLICAR).
    /// <paramref name="Properties"/> is the draft of the tab «Propiedades» (VA-07), written after the redraw; <paramref name="SourceDesignJson"/>
    /// the document of a design of the library a new cama was opened from, so its unknown fields reach the drawing (I-11).
    /// </summary>
    public sealed record CamaDoorWrite(long RequestId, SessionId Session, string RackId, string Name, FlowBedConfiguration Bed, bool NewRack = false,
        RackPropertiesWrite Properties = null, string SourceDesignJson = null);

    /// <summary>
    /// What RACKCAMAABRIR read for «Editar aquí» of a cama: whether it opens here, its kind and name, the bed as its document
    /// (<see cref="FlowBedConfigurationStore"/>), the properties of the rack (VA-07) and, when it does not open, why.
    /// </summary>
    public sealed record CamaDesignRead(long RequestId, string RackId, bool Opens, string Kind, string Name, string DesignJson, string Reason,
        RackPropertiesRead Properties = null)
    {
        public static CamaDesignRead Refused(long requestId, string rackId, string kind, string name, string reason)
            => new CamaDesignRead(requestId, rackId, false, kind, name, null, reason);

        /// <summary>The document of the bed read, as a new object (with its unknown fields, I-11); null when it does not open or cannot be read.</summary>
        public FlowBedDocument Document() => Opens ? new FlowBedConfigurationStore().DeserializeDocument(DesignJson) : null;

        /// <summary>The bed read, as a new object; null when it does not open or cannot be read.</summary>
        public FlowBedConfiguration Bed() => Document()?.ToDomain();
    }

    /// <summary>The plan of RACKCAMAAPLICAR: it writes, with the document of the bed as the drawing has it (for its unknown fields), or it does not, and why.</summary>
    public sealed record CamaApplyPlan(bool CanWrite, FlowBedDocument Source, string Reason);

    /// <summary>
    /// The door of the window to a cama de rodamiento (I-64, quinta tanda, part cm), with the rules of the other doors: numbered requests,
    /// another drawing, «Dejar de esperar», <see cref="IRackCadRackDoorPort.RequestStarted"/>. RACKCAMAABRIR only reads (no undo marker, no
    /// history); RACKCAMAAPLICAR redraws the block of the cama in place with the drawing half of «Actualizar» of RACKEDITAR, so one «U»
    /// undoes it; RACKCAMANUEVO inserts a new cama with the dispatch of «Nuevo». The answers of the writes come with
    /// <see cref="IRackCadRackDoorPort.DesignWritten"/>.
    /// </summary>
    public interface IRackCadCamaDoorPort : IRackCadRackDoorPort
    {
        RackCadClassicResult RequestCamaDesign(RackDoorRequest request);

        RackCadClassicResult RequestCamaWrite(CamaDoorWrite write);

        event Action<CamaDesignRead> CamaRead;
    }

    /// <summary>The pure rules of the door of the cama, with their tests in Core; the commands only call them.</summary>
    public static class CamaDoorRules
    {
        /// <summary>The refusal of a view whose descriptor does not decode, the text of EditCama («RackCad: descriptor de vista de cama invalido.»).</summary>
        public const string InvalidViewText = "Descriptor de vista de cama inválido.";

        /// <summary>
        /// The answer of RACKCAMAABRIR for the envelope of the view of the cama, as RACKEDITAR decides it before its window (EditCama): the
        /// kind, then the descriptor of the view (point 5 of the review: <c>RackViewCodec.Decode(kind, view, section).HasAddress</c>, the
        /// decode of EditCama), then the design.
        /// </summary>
        public static CamaDesignRead AnswerForOpen(long requestId, string rackId, string kind, string name, string view, int section, string designJson,
            RackPropertiesRead properties = null)
            => AnswerForOpen(requestId, rackId, kind, name, RackViewCodec.Decode(kind, view, section), designJson, properties);

        // The same rule over the view as decoded (a seam for the tests: with the kind of a cama the codec of today always gives it an address,
        // coerced when the descriptor is not the canonical one, «CamaDescriptorIgnored»).
        internal static CamaDesignRead AnswerForOpen(long requestId, string rackId, string kind, string name, DecodedRackView decoded, string designJson,
            RackPropertiesRead properties)
        {
            if (!string.Equals(kind, RackEmbedDocument.KindCama, StringComparison.Ordinal))
                return CamaDesignRead.Refused(requestId, rackId, kind, name, "El rack no es una cama de rodamiento: este editor no la abre.");
            if (!decoded.HasAddress) return CamaDesignRead.Refused(requestId, rackId, kind, name, InvalidViewText);

            var document = new FlowBedConfigurationStore().DeserializeDocument(designJson);
            var bed = document?.ToDomain();
            if (bed == null || !RackDesignValidation.IsUsableFlowBed(bed))
                return CamaDesignRead.Refused(requestId, rackId, kind, name, "Datos de cama inválidos.");

            return new CamaDesignRead(requestId, rackId, true, kind, name, designJson, null, properties);
        }

        /// <summary>
        /// The plan of RACKCAMAAPLICAR: the rack must still be a readable cama with a view that decodes (point 5 of the review, as EditCama),
        /// and the window must send a usable one.
        /// </summary>
        public static CamaApplyPlan PlanApply(string kind, string view, int section, string designJson, CamaDoorWrite write)
            => PlanApply(kind, RackViewCodec.Decode(kind, view, section), designJson, write);

        internal static CamaApplyPlan PlanApply(string kind, DecodedRackView decoded, string designJson, CamaDoorWrite write)
        {
            const string NotReadable = "El rack del dibujo no es una cama legible: no se escribió nada.";
            if (write?.Bed == null || !RackDesignValidation.IsUsableFlowBed(write.Bed)) return new CamaApplyPlan(false, null, "No hay una cama válida que escribir.");
            if (!string.Equals(kind, RackEmbedDocument.KindCama, StringComparison.Ordinal)) return new CamaApplyPlan(false, null, NotReadable);
            if (!decoded.HasAddress) return new CamaApplyPlan(false, null, InvalidViewText + " No se escribió nada.");
            var source = new FlowBedConfigurationStore().DeserializeDocument(designJson);
            var bed = source?.ToDomain();
            return bed == null || !RackDesignValidation.IsUsableFlowBed(bed) ? new CamaApplyPlan(false, null, NotReadable) : new CamaApplyPlan(true, source, null);
        }

        /// <summary>
        /// The answer of RACKCAMAAPLICAR from what the drawing half of RACKEDITAR did: «written» only when the block was redrawn (I-08 of
        /// Dinámico). A cama is one block: its copies in the drawing change with it.
        /// </summary>
        public static RackDesignWritten AnswerForApply(long requestId, string rackId, bool redrawn, string error)
            => redrawn
                ? new RackDesignWritten(requestId, rackId, true, "AutoCAD redibujó la cama con el borrador: todas sus copias en el dibujo reflejan el cambio.")
                : new RackDesignWritten(requestId, rackId, false, "AutoCAD no redibujó la cama, así que el borrador sigue sin escribir."
                    + (string.IsNullOrWhiteSpace(error) ? " Revisa la línea de órdenes de AutoCAD." : " " + error.Trim()));

        /// <summary>
        /// The answer of RACKCAMANUEVO: written when a block with the identity of the new cama was placed, with the name the drawing gave it
        /// (the automatic one of I-60 when it had none); otherwise the draft stays in the window, not written. The list is not read again
        /// (D-10): the answer says how to see it there.
        /// </summary>
        public static RackDesignWritten AnswerForNewCama(long requestId, string rackId, int placed, string name)
        {
            if (placed > 0)
            {
                var named = string.IsNullOrWhiteSpace(name) ? "la cama nueva" : "la cama nueva «" + name.Trim() + "»";
                return new RackDesignWritten(requestId, rackId, true,
                    "AutoCAD insertó " + named + ". Para seguir editándola aquí, vuelve a leer la lista y ábrela.",
                    string.IsNullOrWhiteSpace(name) ? null : name.Trim());
            }

            return new RackDesignWritten(requestId, rackId, false,
                "AutoCAD no insertó la cama nueva: se canceló la colocación o no se pudo preparar. El borrador sigue en la ventana; revisa la línea de órdenes de AutoCAD.");
        }

        /// <summary>The identity is the one the window gave: a new cama is never inserted over a rack the drawing already has.</summary>
        public static string RefusalForNewCama(CamaDoorWrite write, int blocksWithThatIdentity)
        {
            if (write?.Bed == null || !RackDesignValidation.IsUsableFlowBed(write.Bed)) return "No hay una cama válida que insertar.";
            if (string.IsNullOrWhiteSpace(write.RackId)) return "La cama nueva no tiene una identidad válida.";
            return blocksWithThatIdentity > 0
                ? "Ya hay un rack con esa identidad en el dibujo (" + blocksWithThatIdentity.ToString(CultureInfo.InvariantCulture) + "): no se insertó nada."
                : null;
        }
    }
}
