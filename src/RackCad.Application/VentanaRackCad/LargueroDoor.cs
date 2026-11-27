using System;
using System.Globalization;
using RackCad.Application.StructuralSections;
using RackCad.Application.StructuralSections.Geometry;
using RackCad.Application.Systems.Larguero;
using RackCad.Application.Workspace;

namespace RackCad.Application.VentanaRackCad
{
    /// <summary>
    /// T6 lg (I-64, sexta tanda): the window asks AutoCAD to insert ONE piece of the page «Larguero» in the drawing of the session
    /// (RACKLARGUEROINSERTAR): the larguero with its two ménsulas or a bare AISC profile, as a loose block (the Owner: «Pieza suelta»). The plan
    /// is the one the page drew, immutable, so nothing the user does afterwards reaches what AutoCAD inserts.
    /// </summary>
    public sealed record LargueroInsertRequest(long RequestId, SessionId Session, LargueroPiecePlan Plan);

    /// <summary>T6 lg: what AutoCAD answered to the request of that number: whether it inserted, what it says, and the name of the block.</summary>
    public sealed record LargueroInserted(long RequestId, bool Inserted, string Message, string BlockName = null);

    /// <summary>
    /// T6 lg: the door of the page «Larguero» of the RackCad window, with the rules of the other doors: a numbered request that waits BEFORE
    /// its command is handed over, only in the drawing that was active, «Dejar de esperar» (<see cref="IRackCadRackDoorPort.Abandon"/>) and
    /// <see cref="IRackCadRackDoorPort.RequestStarted"/> when the command takes it. The command asks the insertion point and places the block
    /// in ONE transaction, so one «U» undoes it; its answer comes with <see cref="LargueroInserted"/>.
    /// </summary>
    public interface IRackCadLargueroDoorPort : IRackCadRackDoorPort
    {
        RackCadClassicResult RequestLargueroInsert(LargueroInsertRequest request);

        event Action<LargueroInserted> LargueroInserted;
    }

    /// <summary>T6 lg: the pure rules and the texts of the door of the page «Larguero»; the command only calls them.</summary>
    public static class LargueroDoorRules
    {
        public const string InsertCommand = "RACKLARGUEROINSERTAR";

        public const string OtherDrawingText =
            "La orden corrió en otro dibujo que el de la ventana: no se insertó nada. Vuelve a ese dibujo y pulsa «Insertar en el dibujo…» otra vez.";

        /// <summary>The warning of a geometry with a declared convention of RackCad (the S family, ADR-0023, decision 22): always, with no switch.</summary>
        public const string VisualDerivedWarning =
            "Geometría visual derivada (ADR-0023): la inclinación del patín y el filete son una convención de RackCad, no un dato de AISC. Es "
            + "aproximada, no está garantizada por ningún fabricante y NO es apta para CNC ni fabricación.";

        /// <summary>Why the request cannot be inserted; null when it can.</summary>
        public static string RefusalOf(LargueroInsertRequest request)
        {
            if (request?.Plan == null || request.Plan.Curves.Count == 0) return "No hay una pieza completa que insertar.";
            if (request.Plan.Kind == LargueroPieceKind.Profile && request.Plan.SectionPlan == null) return "El perfil no tiene geometría que insertar.";
            return null;
        }

        /// <summary>What the command line asks: the point of the larguero, or of the profile.</summary>
        public static string PointPrompt(LargueroPiecePlan plan) =>
            plan?.Kind == LargueroPieceKind.Profile ? "\nPunto de insercion del perfil: " : "\nPunto de insercion del larguero (cara del poste, arriba): ";

        /// <summary>
        /// The answer when the block was placed: what it is, that it is a loose piece (not a rack, not in the list), the weight of an AISC
        /// profile and, for a geometry with a convention of RackCad, the warning of ADR-0023, unconditionally.
        /// </summary>
        public static LargueroInserted Inserted(long requestId, LargueroPiecePlan plan, string blockName)
        {
            var what = plan.Kind == LargueroPieceKind.Profile ? "el perfil «" + plan.Name + "»" : "el larguero «" + plan.Name + "»";
            var message = "AutoCAD insertó " + what + " como una pieza suelta (bloque «" + (blockName ?? string.Empty) + "»): no es un rack, así que no sale en la "
                          + "lista ni se edita desde ella. Un «U» lo deshace.";
            if (plan.Section != null)
                message += " Peso del perfil: " + StructuralSectionLabelFormatter.FormatWeight(plan.Section) + ", "
                           + StructuralSectionUnits.WeightInPounds(plan.Section, plan.Length).ToString("0.#", CultureInfo.InvariantCulture) + " lb a "
                           + Number(plan.Length) + " in.";
            if (plan.IsVisualDerived) message += " " + VisualDerivedWarning;
            return new LargueroInserted(requestId, true, message, blockName);
        }

        /// <summary>The answer when the user cancelled the point: nothing was created.</summary>
        public static LargueroInserted Cancelled(long requestId) =>
            new LargueroInserted(requestId, false, "No se insertó nada: se canceló el punto de inserción. El dibujo no cambió.");

        /// <summary>The answer when the insertion failed: the transaction was not committed, so nothing reached the drawing.</summary>
        public static LargueroInserted Failed(long requestId, string error) =>
            new LargueroInserted(requestId, false, "No se pudo insertar: " + (string.IsNullOrWhiteSpace(error) ? "sin detalle." : error.Trim())
                                                    + " El dibujo no cambió.");

        /// <summary>
        /// What the command line says after the insertion, as RACKSECCION says it (without accents, as its line does): the block, its length,
        /// the weight of an AISC profile, the warning of ADR-0023 FIRST and unconditional for a visually derived geometry, then the fidelity.
        /// </summary>
        public static string CommandLineText(LargueroPiecePlan plan, string blockName)
        {
            if (plan == null) return string.Empty;
            var text = plan.Kind == LargueroPieceKind.Profile
                ? "RackCad: " + plan.Name + " insertada como bloque '" + blockName + "'. Longitud " + Number(plan.Length) + " in, vista "
                  + plan.SectionPlan?.View + ", " + plan.Curves.Count.ToString(CultureInfo.InvariantCulture) + " curvas."
                : "RackCad: larguero '" + plan.Name + "' insertado como pieza suelta, bloque '" + blockName + "'. Longitud a corte " + Number(plan.Length)
                  + " in, " + Number(plan.OverallLength) + " in con sus 2 mensulas.";
            if (plan.Section != null)
                text += "\nPeso: " + StructuralSectionLabelFormatter.FormatWeightWithDesignation(plan.Section) + ", total "
                        + StructuralSectionUnits.WeightInPounds(plan.Section, plan.Length).ToString("0.#", CultureInfo.InvariantCulture) + " lb.";

            if (plan.IsVisualDerived)
            {
                text += "\nADVERTENCIA: geometria visual derivada (ADR-0023). La inclinacion del patin y el filete son una convencion de RackCad, no un "
                        + "dato de AISC: es aproximada, no esta garantizada por ningun fabricante y NO es apta para CNC ni fabricacion. Dimensiones, area, "
                        + "peso y propiedades siguen siendo los que tabula AISC.";
            }

            var sectionPlan = plan.SectionPlan;
            if (sectionPlan != null && sectionPlan.Fidelity != SectionFidelity.TabulatedComplete)
            {
                text += "\nFidelidad: " + sectionPlan.Fidelity + ".";
                foreach (var diagnostic in sectionPlan.Diagnostics) text += "\n  - " + diagnostic.Message;
            }

            return text;
        }

        private static string Number(double value) => value.ToString("0.###", CultureInfo.InvariantCulture);
    }
}
