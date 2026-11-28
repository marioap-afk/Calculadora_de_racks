using System;
using RackCad.Application.VentanaRackCad;

namespace RackCad.Plugin.VentanaRackCad
{
    /// <summary>
    /// T7 per (I-64, séptima tanda; the Owner, 2026-11-27: «Igual que la clásica»): the door of the page «Perfil estructural» of the RackCad
    /// window, with the rules of the door to a rack (<see cref="Send"/>: an active document, no command in progress, the request kept before it
    /// is handed over and the focus back to the drawing). The command (RACKSECCIONINSERTAR) lives in <c>RackSeccionCommands.Ventana.cs</c>; it
    /// takes its request once and answers with its number, and a request the window abandoned (<see cref="Abandon"/>) is dropped, so a late
    /// command does nothing. This file holds no AutoCAD transaction and no lock.
    /// </summary>
    public sealed partial class RackCadRackDoorPort : IRackCadStructuralSectionDoorPort
    {
        /// <summary>An insertion of the page that waits for its command: the request (its immutable section and plan) and the drawing it is for.</summary>
        internal sealed record PendingStructuralSectionInsert(StructuralSectionInsertRequest Request, long Document)
        {
            public long RequestId => Request.RequestId;
        }

        private static PendingStructuralSectionInsert _pendingStructuralSectionInsert;

        public event Action<StructuralSectionInserted> StructuralSectionInserted;

        public RackCadClassicResult RequestStructuralSectionInsert(StructuralSectionInsertRequest request)
        {
            var refusal = StructuralSectionDoorRules.RefusalOf(request);
            if (refusal != null) return RackCadClassicResult.Without(RackCadClassicStatus.Failed, refusal);

            // The section and the plan are immutable: the request itself is the copy that waits.
            return Send(StructuralSectionDoorRules.InsertCommand + " ",
                document => _pendingStructuralSectionInsert = new PendingStructuralSectionInsert(request, KeyOf(document)),
                () => _pendingStructuralSectionInsert = null);
        }

        /// <summary>The insertion of the page that waits, taken once by its command; null when there is none.</summary>
        internal static PendingStructuralSectionInsert TakePendingStructuralSectionInsert()
        {
            var pending = _pendingStructuralSectionInsert;
            _pendingStructuralSectionInsert = null;
            return pending;
        }

        // «Dejar de esperar» of the request of the page (called by Abandon).
        private static void AbandonStructuralSection(long requestId)
        {
            if (_pendingStructuralSectionInsert != null && _pendingStructuralSectionInsert.RequestId == requestId) _pendingStructuralSectionInsert = null;
        }

        internal void AnswerStructuralSectionInsert(StructuralSectionInserted inserted) => Raise(() => StructuralSectionInserted?.Invoke(inserted));
    }
}
