using System;
using RackCad.Application.StructuralSections;
using RackCad.Application.StructuralSections.Geometry;
using RackCad.Application.Workspace;

namespace RackCad.Application.VentanaRackCad
{
    /// <summary>
    /// T7 per (I-64, séptima tanda; the Owner, 2026-11-27: «Igual que la clásica»): the page «Perfil estructural» of the RackCad window asks
    /// AutoCAD to insert the section the user approved in its preview (RACKSECCIONINSERTAR). The request carries the section and its plan, the
    /// very objects the page built with the inspector of RACKSECCION; both are immutable, so nothing the user does afterwards reaches what
    /// AutoCAD inserts.
    /// </summary>
    public sealed record StructuralSectionInsertRequest(long RequestId, SessionId Session, StructuralSectionDefinition Section,
        StructuralSectionRepresentationPlan Plan);

    /// <summary>
    /// T7 per: what AutoCAD answered to the request of that number. <see cref="Message"/> is what the page says; <see cref="Report"/> is the
    /// report of the flow of RACKSECCION, verbatim, the same text the command line received (what was drawn, its weight, the warning of
    /// ADR-0023 and the fidelity); <see cref="UnitsWarned"/> is whether the advisory of the units (I-05) fired before the point.
    /// </summary>
    public sealed record StructuralSectionInserted(long RequestId, bool Inserted, bool Failed, string Message, string BlockName = null,
        string Report = null, bool UnitsWarned = false);

    /// <summary>
    /// T7 per: the door of the page «Perfil estructural», with the rules of the other doors of the window: ONE numbered request that waits
    /// BEFORE its command is handed over, only in the drawing that was active, «Dejar de esperar» (<see cref="IRackCadRackDoorPort.Abandon"/>)
    /// and <see cref="IRackCadRackDoorPort.RequestStarted"/> when the command takes it. The command asks the insertion point and draws the
    /// section with the materialization of RACKSECCION (one transaction, one regen), so one «U» undoes it; its answer comes with
    /// <see cref="StructuralSectionInserted"/>.
    /// </summary>
    public interface IRackCadStructuralSectionDoorPort : IRackCadRackDoorPort
    {
        RackCadClassicResult RequestStructuralSectionInsert(StructuralSectionInsertRequest request);

        event Action<StructuralSectionInserted> StructuralSectionInserted;
    }

    /// <summary>T7 per: the pure rules and the texts of the door of the page «Perfil estructural»; the command and the page only call them.</summary>
    public static class StructuralSectionDoorRules
    {
        /// <summary>The command of the door: it takes the request, asks the point and draws (one «U»).</summary>
        public const string InsertCommand = "RACKSECCIONINSERTAR";

        /// <summary>What the command line says when the command runs with no request (without accents, as the lines of the Plugin).</summary>
        public const string NoRequestText = "RACKSECCIONINSERTAR la usa la ventana RackCad: no hay ningun perfil pendiente de insertar.";

        public const string OtherDrawingText =
            "La orden corrió en otro dibujo que el de la ventana: no se insertó nada. Vuelve a ese dibujo y pulsa «Insertar en el dibujo…» otra vez.";

        public const string NothingToInsertText = "No hay un perfil que insertar: elige una sección de la lista.";

        public const string MismatchText = "El plan no es el de la sección elegida: no se insertó nada. Vuelve a pulsar «Insertar en el dibujo…».";

        public const string CancelledText = "No se insertó nada: se canceló el punto de inserción. El dibujo no cambió.";

        /// <summary>The advisory of the units (I-05), as the page says it; the command line keeps the words of RackUnitsGuard.</summary>
        public const string UnitsText =
            "Aviso de unidades: el dibujo no está en pulgadas (INSUNITS). RackCad dibuja en pulgadas y no convierte ni reescala nada, así que la "
            + "sección queda a escala de pulgadas.";

        /// <summary>The page waits for the command to take its request.</summary>
        public const string WaitingText = "Esperando a que AutoCAD tome la inserción del perfil.";

        /// <summary>The command took the request: AutoCAD asks the point (the prompt of RACKSECCION).</summary>
        public const string TakenText =
            "AutoCAD pide el punto de inserción de la sección en la línea de órdenes: indícalo en el dibujo o cancélalo con Esc. La ventana recibirá su respuesta.";

        public const string StoppedText = "Se dejó de esperar a AutoCAD: si la orden llega a correr, no insertará nada.";

        public const string DrawingClosedText = "Se cerró el dibujo de la inserción: no se insertó nada.";

        /// <summary>Why the request cannot be inserted; null when it can.</summary>
        public static string RefusalOf(StructuralSectionInsertRequest request)
        {
            if (request?.Section == null || request.Plan == null) return NothingToInsertText;
            return request.Plan.SectionId != request.Section.SectionId ? MismatchText : null;
        }

        /// <summary>
        /// The answer when the section was drawn: what it is and its block, that it is a loose section (no rack: not in the list and never
        /// reopened, as with RACKSECCION) and that one «U» undoes it.
        /// </summary>
        public static StructuralSectionInserted Inserted(long requestId, StructuralSectionDefinition section, string blockName, string report, bool unitsWarned) =>
            new StructuralSectionInserted(requestId, true, false,
                "AutoCAD insertó «" + (section?.DisplayName ?? string.Empty) + "» como el bloque «" + (blockName ?? string.Empty) + "». Es una sección "
                + "suelta, sin datos de rack: no sale en la lista de la ventana ni se reabre. Un «U» la deshace.",
                blockName, report, unitsWarned);

        /// <summary>The answer when the user cancelled the point: nothing was created.</summary>
        public static StructuralSectionInserted Cancelled(long requestId, string report, bool unitsWarned) =>
            new StructuralSectionInserted(requestId, false, false, CancelledText, null, report, unitsWarned);

        /// <summary>
        /// The answer when the drawing failed BEFORE the commit (the transaction was not committed) or before anything was created (the
        /// units advisory, the point): nothing reached the drawing, so «El dibujo no cambió» is true. A failure after the commit is
        /// <see cref="DrawnButFailed"/> (review M-1).
        /// </summary>
        public static StructuralSectionInserted Failed(long requestId, string error, string report, bool unitsWarned) =>
            new StructuralSectionInserted(requestId, false, true,
                "No se pudo dibujar la sección: " + (string.IsNullOrWhiteSpace(error) ? "sin detalle." : error.Trim()) + " El dibujo no cambió.",
                null, report, unitsWarned);

        /// <summary>
        /// T7 per (review M-1): the section WAS committed and what failed came after it (the regen). The section is in the drawing, so the
        /// answer says so and where to look, and never that the drawing did not change; one «U» undoes the insertion.
        /// </summary>
        public static StructuralSectionInserted DrawnButFailed(long requestId, StructuralSectionDefinition section, string blockName, string error,
            string report, bool unitsWarned) =>
            new StructuralSectionInserted(requestId, true, true,
                "AutoCAD insertó «" + (section?.DisplayName ?? string.Empty) + "» como el bloque «" + (blockName ?? string.Empty) + "», pero después falló: "
                + (string.IsNullOrWhiteSpace(error) ? "sin detalle" : error.Trim().TrimEnd('.')) + ". Revisa el dibujo: un «U» deshace la inserción.",
                blockName, report, unitsWarned);

        /// <summary>The answer when the command refused the request before asking anything (another drawing, nothing to insert).</summary>
        public static StructuralSectionInserted Refused(long requestId, string text) =>
            new StructuralSectionInserted(requestId, false, true, string.IsNullOrWhiteSpace(text) ? NothingToInsertText : text.Trim());
    }
}
