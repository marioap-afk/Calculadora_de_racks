using System;
using RackCad.Domain.Systems.Dynamic;

namespace RackCad.Application.VentanaRackCad
{
    /// <summary>What AutoCAD read of ONE rack for the RackCad window: its persisted design, or why there is none.</summary>
    public sealed record RackDesignRead(string RackId, bool Found, string Kind, string Name, string DesignJson, string Reason);

    /// <summary>What AutoCAD answered when it wrote the draft of a rack: whether it wrote it and what it said.</summary>
    public sealed record RackDesignWritten(string RackId, bool Written, string Message);

    /// <summary>
    /// The door of the RackCad window to ONE rack of the drawing (I-64 A-14, line D): reading its design to edit it in the window, and
    /// writing the draft back. Both run as AutoCAD commands in the document's own command context: the read is a command without an undo
    /// marker, so it leaves nothing in the list of Deshacer, and the write is one command, so one «U» undoes it. The palette never locks
    /// the document for them. The answers arrive later, through the two events, on the thread of AutoCAD.
    /// </summary>
    public interface IRackCadRackDoorPort
    {
        /// <summary>Asks AutoCAD to read the design of the rack; the answer arrives with <see cref="DesignRead"/>.</summary>
        RackCadClassicResult RequestDesign(string rackId);

        /// <summary>Asks AutoCAD to redraw the rack with the draft (update only: no new view); the answer arrives with <see cref="DesignWritten"/>.</summary>
        RackCadClassicResult RequestDynamicUpdate(string rackId, string name, DynamicRackDesign design, DynamicRackSystem system);

        event Action<RackDesignRead> DesignRead;

        event Action<RackDesignWritten> DesignWritten;
    }
}
