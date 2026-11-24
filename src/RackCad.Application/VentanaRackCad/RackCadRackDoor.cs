using System.Collections.Generic;
using RackCad.Application.Systems.Shared;
using RackCad.Application.Workspace;
using RackCad.Domain.Systems.Dynamic;

namespace RackCad.Application.VentanaRackCad
{
    /// <summary>The window asks AutoCAD to read the design of ONE rack of the drawing of the session; the number ties the answer to this request.</summary>
    public sealed record RackDoorRequest(long RequestId, SessionId Session, string RackId);

    /// <summary>
    /// The window asks AutoCAD to redraw ONE rack of the drawing of the session with a draft. The design and the system are COPIES taken when
    /// the request was made, so nothing the user does afterwards reaches what AutoCAD writes; the name is already trimmed, as the classic
    /// editor sends it. With <paramref name="InsertViews"/> («Insertar vista…», step 4 of Dinámico; decision 10 of the prototype) AutoCAD
    /// first writes the draft in the views that exist and then inserts those views as linked copies of the rack (the same identity);
    /// without them it only updates. With <paramref name="NewRack"/> (a new Dinámico made in the window, step 4) the rack is not in the
    /// drawing yet: AutoCAD inserts it with those views, with the identity the window gave it and the automatic name of I-60.
    /// </summary>
    public sealed record RackDoorWrite(long RequestId, SessionId Session, string RackId, string Name, DynamicRackDesign Design, DynamicRackSystem System,
        IReadOnlyList<RackViewAddress> InsertViews = null, bool NewRack = false)
    {
        /// <summary>The write also inserts views.</summary>
        public bool Inserts => InsertViews != null && InsertViews.Count > 0;
    }

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
        /// at least one view was redrawn. Otherwise the draft stays not written, and the answer says whether views were erased. Declared
        /// (M-28 of the fourth review): a write where one view is redrawn and another fails counts as written, as it does for RACKEDITAR.
        /// </summary>
        public static RackDesignWritten AnswerForApply(long requestId, string rackId, int redrawn, int erased)
        {
            if (redrawn > 0)
            {
                var text = "AutoCAD redibujó " + Count(redrawn, "vista", "vistas") + " del rack con el borrador";
                if (erased > 0) text += " y borró " + Count(erased, "vista que ya no existe", "vistas que ya no existen");
                return new RackDesignWritten(requestId, rackId, true, text + "; la línea de órdenes da el recuento por tipo de vista.");
            }

            if (erased > 0)
            {
                return new RackDesignWritten(requestId, rackId, false,
                    "AutoCAD no redibujó ninguna vista del rack, pero borró " + Count(erased, "vista que ya no existe", "vistas que ya no existen")
                    + ": revisa el dibujo y la línea de órdenes. El borrador sigue sin escribir.");
            }

            return new RackDesignWritten(requestId, rackId, false,
                "AutoCAD no redibujó ninguna vista del rack, así que el borrador sigue sin escribir. Revisa la línea de órdenes de AutoCAD.");
        }

        /// <summary>
        /// The answer of RACKDINAMICOINSERTAR («Insertar vista…»): the draft is written when the views that exist were redrawn with it, as
        /// for RACKDINAMICOAPLICAR; then the answer says how many new views were inserted (none when the placement was cancelled). When no
        /// view was redrawn the draft stays not written, even if a view was inserted, so «Actualizar» can still write it in all of them.
        /// </summary>
        public static RackDesignWritten AnswerForInsert(long requestId, string rackId, int redrawn, int erased, int inserted)
        {
            var applied = AnswerForApply(requestId, rackId, redrawn, erased);
            if (!applied.Written)
            {
                return inserted > 0
                    ? new RackDesignWritten(requestId, rackId, false,
                        "AutoCAD insertó " + Count(inserted, "vista nueva", "vistas nuevas") + ", pero no redibujó las vistas que ya había, así que el "
                        + "borrador sigue sin escribir: usa «Actualizar» y revisa la línea de órdenes de AutoCAD.")
                    : applied;
            }

            var text = "AutoCAD redibujó " + Count(redrawn, "vista", "vistas") + " del rack con el borrador";
            if (erased > 0) text += ", borró " + Count(erased, "vista que ya no existe", "vistas que ya no existen");
            text += inserted > 0
                ? " e insertó " + Count(inserted, "vista nueva", "vistas nuevas") + "."
                : ", pero no insertó ninguna vista nueva: se canceló la colocación o no se pudo preparar. Revisa la línea de órdenes de AutoCAD.";
            return new RackDesignWritten(requestId, rackId, true, text);
        }

        /// <summary>
        /// The answer of RACKDINAMICONUEVO (a new Dinámico of the window): written when at least one view of the new rack was placed, with the
        /// name the drawing gave it; otherwise the draft stays in the window, not written.
        /// </summary>
        public static RackDesignWritten AnswerForNewRack(long requestId, string rackId, int placed, string name)
        {
            if (placed > 0)
            {
                var named = string.IsNullOrWhiteSpace(name) ? "el rack nuevo" : "el rack nuevo «" + name.Trim() + "»";
                return new RackDesignWritten(requestId, rackId, true,
                    "AutoCAD insertó " + named + " con " + Count(placed, "vista", "vistas") + ". Para seguir editándolo aquí, vuelve a leer la lista y ábrelo.");
            }

            return new RackDesignWritten(requestId, rackId, false,
                "AutoCAD no insertó el rack nuevo: se canceló la colocación o no se pudo preparar. El borrador sigue en la ventana; revisa la línea de órdenes de AutoCAD.");
        }

        private static string Count(int count, string one, string many) =>
            count.ToString(System.Globalization.CultureInfo.InvariantCulture) + " " + (count == 1 ? one : many);
    }
}
