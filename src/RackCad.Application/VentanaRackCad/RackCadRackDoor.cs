using RackCad.Application.Workspace;
using RackCad.Domain.Systems.Dynamic;

namespace RackCad.Application.VentanaRackCad
{
    /// <summary>The window asks AutoCAD to read the design of ONE rack of the drawing of the session; the number ties the answer to this request.</summary>
    public sealed record RackDoorRequest(long RequestId, SessionId Session, string RackId);

    /// <summary>
    /// The window asks AutoCAD to redraw ONE rack of the drawing of the session with a draft. The design and the system are COPIES taken when
    /// the request was made, so nothing the user does afterwards reaches what AutoCAD writes; the name is already trimmed, as the classic
    /// editor sends it.
    /// </summary>
    public sealed record RackDoorWrite(long RequestId, SessionId Session, string RackId, string Name, DynamicRackDesign Design, DynamicRackSystem System);

    /// <summary>What AutoCAD read of ONE rack for the RackCad window, for the request of that number: its persisted design, or why there is none.</summary>
    public sealed record RackDesignRead(long RequestId, string RackId, bool Found, string Kind, string Name, string DesignJson, string Reason);

    /// <summary>What AutoCAD answered when it wrote the draft of a rack, for the request of that number: whether it wrote it and what it said.</summary>
    public sealed record RackDesignWritten(long RequestId, string RackId, bool Written, string Message);

    /// <summary>
    /// The door of the RackCad window to ONE rack of the drawing (I-64 A-14, line D): reading its design to edit it in the window, and
    /// writing the draft back. Both run as AutoCAD commands in the document's own command context, and the palette never locks the
    /// document for them; the focus goes back to the drawing once the command is handed over. Each request carries a number and the session
    /// it was made for: an answer is for that request only, a command that runs in another drawing than the one that was active at the
    /// request does nothing, and a request the window stopped waiting for (<see cref="Abandon"/>) does nothing when its command runs. The answers arrive later (or, if AutoCAD runs
    /// the command at once, during the call), through the events, on the thread of AutoCAD.
    /// </summary>
    public interface IRackCadRackDoorPort
    {
        /// <summary>Asks AutoCAD to read the design of the rack; the answer arrives with <see cref="DesignRead"/>.</summary>
        RackCadClassicResult RequestDesign(RackDoorRequest request);

        /// <summary>Asks AutoCAD to redraw the rack with the draft (update only: no new view); the answer arrives with <see cref="DesignWritten"/>.</summary>
        RackCadClassicResult RequestDynamicUpdate(RackDoorWrite write);

        /// <summary>The window stopped waiting for the request: if its command has not run yet, it will do nothing when it runs.</summary>
        void Abandon(long requestId);

        event System.Action<RackDesignRead> DesignRead;

        event System.Action<RackDesignWritten> DesignWritten;
    }

    /// <summary>The rules of the door that need no AutoCAD, so they are tested where AutoCAD is not.</summary>
    public static class RackDoorRules
    {
        /// <summary>
        /// The answer of RACKDINAMICOAPLICAR from what the drawing half of RACKEDITAR did (I-08 of the third review): «written» only when
        /// at least one view was redrawn. Otherwise the draft stays not written, and the answer says whether the drawing still changed.
        /// </summary>
        public static RackDesignWritten AnswerForApply(long requestId, string rackId, int redrawn, int erased)
        {
            if (redrawn > 0)
            {
                var text = "AutoCAD redibujó " + Count(redrawn, "vista", "vistas") + " del rack con el borrador";
                if (erased > 0) text += " y borró " + Count(erased, "vista que ya no existe", "vistas que ya no existen");
                return new RackDesignWritten(requestId, rackId, true, text + "; la línea de órdenes dice cuáles.");
            }

            if (erased > 0)
            {
                return new RackDesignWritten(requestId, rackId, false,
                    "AutoCAD no redibujó ninguna vista del rack, pero borró " + Count(erased, "vista que ya no existe", "vistas que ya no existen")
                    + ": revisa el dibujo. El borrador sigue sin escribir.");
            }

            return new RackDesignWritten(requestId, rackId, false,
                "AutoCAD no redibujó ninguna vista del rack: el dibujo no cambió (la línea de órdenes dice por qué). El borrador sigue sin escribir.");
        }

        private static string Count(int count, string one, string many) =>
            count.ToString(System.Globalization.CultureInfo.InvariantCulture) + " " + (count == 1 ? one : many);
    }
}
