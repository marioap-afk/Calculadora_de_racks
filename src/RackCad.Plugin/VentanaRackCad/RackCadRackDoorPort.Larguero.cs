using System;
using RackCad.Application.VentanaRackCad;

namespace RackCad.Plugin.VentanaRackCad
{
    /// <summary>
    /// T6 lg (I-64, sexta tanda): the door of the page «Larguero» of the RackCad window (RACKLARGUEROINSERTAR), with the rules of the door to a
    /// rack (<see cref="Send"/>: an active document, no command in progress, the request kept before it is handed over and the focus back to the
    /// drawing). The command lives in <c>RackLargueroCommands.Ventana.cs</c>; it takes its request once and answers with its number, and a
    /// request the window abandoned (<see cref="Abandon"/>) is dropped, so a late command does nothing. This file holds no AutoCAD transaction
    /// and no lock.
    /// </summary>
    public sealed partial class RackCadRackDoorPort : IRackCadLargueroDoorPort
    {
        /// <summary>An insertion of the page that waits for its command: the request (its immutable plan) and the drawing it is for.</summary>
        internal sealed record PendingLargueroInsert(LargueroInsertRequest Request, long Document)
        {
            public long RequestId => Request.RequestId;
        }

        private static PendingLargueroInsert _pendingLargueroInsert;

        public event Action<LargueroInserted> LargueroInserted;

        public RackCadClassicResult RequestLargueroInsert(LargueroInsertRequest request)
        {
            var refusal = LargueroDoorRules.RefusalOf(request);
            if (refusal != null) return RackCadClassicResult.Without(RackCadClassicStatus.Failed, refusal);

            // The plan is immutable: the request itself is the copy that waits.
            return Send(LargueroDoorRules.InsertCommand + " ",
                document => _pendingLargueroInsert = new PendingLargueroInsert(request, KeyOf(document)),
                () => _pendingLargueroInsert = null);
        }

        /// <summary>The insertion of the page that waits, taken once by its command; null when there is none.</summary>
        internal static PendingLargueroInsert TakePendingLargueroInsert()
        {
            var pending = _pendingLargueroInsert;
            _pendingLargueroInsert = null;
            return pending;
        }

        // «Dejar de esperar» of a request of the page (called by Abandon).
        private static void AbandonLarguero(long requestId)
        {
            if (_pendingLargueroInsert != null && _pendingLargueroInsert.RequestId == requestId) _pendingLargueroInsert = null;
        }

        internal void AnswerLargueroInsert(LargueroInserted inserted) => Raise(() => LargueroInserted?.Invoke(inserted));
    }
}
