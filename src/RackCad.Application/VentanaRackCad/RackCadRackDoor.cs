using RackCad.Domain.Systems.Dynamic;

namespace RackCad.Application.VentanaRackCad
{
    /// <summary>The window asks AutoCAD to read the design of ONE rack; the number ties the answer to this request.</summary>
    public sealed record RackDoorRequest(long RequestId, string RackId);

    /// <summary>
    /// The window asks AutoCAD to redraw ONE rack with a draft. The design and the system are COPIES taken when the request was made, so
    /// nothing the user does afterwards reaches what AutoCAD writes; the name is already trimmed, as the classic editor sends it.
    /// </summary>
    public sealed record RackDoorWrite(long RequestId, string RackId, string Name, DynamicRackDesign Design, DynamicRackSystem System);

    /// <summary>What AutoCAD read of ONE rack for the RackCad window, for the request of that number: its persisted design, or why there is none.</summary>
    public sealed record RackDesignRead(long RequestId, string RackId, bool Found, string Kind, string Name, string DesignJson, string Reason);

    /// <summary>What AutoCAD answered when it wrote the draft of a rack, for the request of that number: whether it wrote it and what it said.</summary>
    public sealed record RackDesignWritten(long RequestId, string RackId, bool Written, string Message);

    /// <summary>
    /// The door of the RackCad window to ONE rack of the drawing (I-64 A-14, line D): reading its design to edit it in the window, and
    /// writing the draft back. Both run as AutoCAD commands in the document's own command context, and the palette never locks the
    /// document for them; the focus goes back to the drawing once the command is handed over. Each request carries a number: an answer is
    /// for that request only, a request is for the drawing that was active when it was made, and a request the window stopped waiting for
    /// (<see cref="Abandon"/>) does nothing when its command runs. The answers arrive later, through the events, on the thread of AutoCAD.
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
}
