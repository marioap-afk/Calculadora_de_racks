using System;
using RackCad.Application.VentanaRackCad;

namespace RackCad.Plugin.VentanaRackCad
{
    /// <summary>
    /// T4 bom: the door of the RackCad window to the BOM of the drawing (page «BOM y lista de racks», BM-01 and BM-11). The read is a command
    /// of AutoCAD with no undo marker (RACKBOMLEER, in <c>RackInventarioCommands.Ventana.cs</c>), handed over with the same rules as the door
    /// to a rack (<see cref="Send"/>: an active document, no command in progress, the request kept before it is handed over and the focus
    /// back to the drawing). The command takes the request once and answers with its number; a request the window abandoned
    /// (<see cref="Abandon"/>) is dropped, so a late command does nothing. The door keeps the drawing of the request (its native key), and
    /// the command reads nothing in another one; the window checks, when the answer arrives, that the drawing active then is the one of the
    /// session of its request (correction 5 of the review), because this door cannot reach the sessions of the host (M-24). RACKLISTA and
    /// RACKBOMTOTAL read the drawing in their own command and hand the reading to the page (<see cref="AskBomPage"/>). This file holds no
    /// AutoCAD transaction and no lock.
    /// </summary>
    public sealed partial class RackCadRackDoorPort : IRackCadBomPort
    {
        /// <summary>A read of the BOM that waits for its command: the number and the drawing it is for.</summary>
        internal sealed record PendingBomRead(long RequestId, long Document);

        private static PendingBomRead _pendingBomRead;

        public event Action<RackBomReading> BomRead;

        public event Action<RackBomTab, RackBomReading> BomPageAsked;

        public RackCadClassicResult RequestBomRead(RackBomRequest request)
        {
            if (request == null)
                return RackCadClassicResult.Without(RackCadClassicStatus.Failed, "La petición no es válida.");

            return Send(RackBomReadingRules.CommandName + " ",
                document => _pendingBomRead = new PendingBomRead(request.RequestId, KeyOf(document)),
                () => _pendingBomRead = null);
        }

        /// <summary>The read of the BOM that waits, taken once by its command; null when there is none.</summary>
        internal static PendingBomRead TakePendingBomRead()
        {
            var pending = _pendingBomRead;
            _pendingBomRead = null;
            return pending;
        }

        // «Dejar de esperar» of a read of the BOM (called by Abandon).
        private static void AbandonBomRead(long requestId)
        {
            if (_pendingBomRead != null && _pendingBomRead.RequestId == requestId) _pendingBomRead = null;
        }

        internal void AnswerBomRead(RackBomReading reading) => Raise(() => BomRead?.Invoke(reading));

        /// <summary>RACKLISTA or RACKBOMTOTAL read the drawing in their own command: the window shows «BOM y lista de racks», on that tab, with it.</summary>
        internal void AskBomPage(RackBomTab tab, RackBomReading reading) => Raise(() => BomPageAsked?.Invoke(tab, reading));
    }
}
