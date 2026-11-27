using System;
using System.Collections.Generic;
using RackCad.Application.Systems.Shared;
using RackCad.Application.VentanaRackCad;

namespace RackCad.Plugin.VentanaRackCad
{
    /// <summary>
    /// T5 ct (I-64, fifth batch): the door of a Cantilever line (RACKCANTILEVERABRIR, RACKCANTILEVERAPLICAR, RACKCANTILEVERINSERTAR and
    /// RACKCANTILEVERNUEVO), with the rules of the other doors: the request waits BEFORE its command is handed over, it is kept with the drawing
    /// that was active, «Dejar de esperar» drops it, and the copies that wait are the door's own (the design copied again, the views copied).
    /// </summary>
    public sealed partial class RackCadRackDoorPort : IRackCadCantileverDoorPort
    {
        internal const string CantileverReadCommand = "RACKCANTILEVERABRIR";
        internal const string CantileverWriteCommand = "RACKCANTILEVERAPLICAR";
        internal const string CantileverInsertCommand = "RACKCANTILEVERINSERTAR";
        internal const string CantileverNewCommand = "RACKCANTILEVERNUEVO";

        internal sealed record PendingCantileverWrite(CantileverDoorWrite Write, long Document)
        {
            public long RequestId => Write.RequestId;

            public string RackId => Write.RackId;
        }

        private static PendingRead _pendingCantileverRead;
        private static PendingCantileverWrite _pendingCantileverWrite;
        private static PendingCantileverWrite _pendingCantileverInsert;
        private static PendingCantileverWrite _pendingCantileverNew;

        public event Action<CantileverDesignRead> CantileverRead;

        public RackCadClassicResult RequestCantileverDesign(RackDoorRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.RackId))
                return RackCadClassicResult.Without(RackCadClassicStatus.Failed, "La línea no tiene una identidad válida.");

            return Send(CantileverReadCommand + " ",
                document => _pendingCantileverRead = new PendingRead(request.RequestId, request.RackId.Trim(), KeyOf(document)),
                () => _pendingCantileverRead = null);
        }

        public RackCadClassicResult RequestCantileverUpdate(CantileverDoorWrite write)
        {
            var refusal = CantileverDoorRules.RefusalOf(write);
            if (refusal != null) return RackCadClassicResult.Without(RackCadClassicStatus.Failed, refusal);

            if (write.NewRack)
                return Send(CantileverNewCommand + " ", document => _pendingCantileverNew = Copy(write, document), () => _pendingCantileverNew = null);
            if (write.Inserts)
                return Send(CantileverInsertCommand + " ", document => _pendingCantileverInsert = Copy(write, document), () => _pendingCantileverInsert = null);
            return Send(CantileverWriteCommand + " ", document => _pendingCantileverWrite = Copy(write, document), () => _pendingCantileverWrite = null);
        }

        // The request as it waits: the identity trimmed, its own copy of the design and of the views to insert (the line is resolved from
        // the copy of the window and nobody edits it).
        private static PendingCantileverWrite Copy(CantileverDoorWrite write, Autodesk.AutoCAD.ApplicationServices.Document document) =>
            new PendingCantileverWrite(write with
            {
                RackId = write.RackId.Trim(),
                Design = write.Design.DeepCopy(),
                InsertViews = write.Inserts ? new List<RackViewAddress>(write.InsertViews) : null,
            }, KeyOf(document));

        // One line of Abandon: «Dejar de esperar» forgets the Cantilever request that waits.
        private static void AbandonCantilever(long requestId)
        {
            if (_pendingCantileverRead != null && _pendingCantileverRead.RequestId == requestId) _pendingCantileverRead = null;
            if (_pendingCantileverWrite != null && _pendingCantileverWrite.RequestId == requestId) _pendingCantileverWrite = null;
            if (_pendingCantileverInsert != null && _pendingCantileverInsert.RequestId == requestId) _pendingCantileverInsert = null;
            if (_pendingCantileverNew != null && _pendingCantileverNew.RequestId == requestId) _pendingCantileverNew = null;
        }

        internal static PendingRead TakePendingCantileverRead()
        {
            var pending = _pendingCantileverRead;
            _pendingCantileverRead = null;
            return pending;
        }

        internal static PendingCantileverWrite TakePendingCantileverWrite()
        {
            var pending = _pendingCantileverWrite;
            _pendingCantileverWrite = null;
            return pending;
        }

        internal static PendingCantileverWrite TakePendingCantileverInsert()
        {
            var pending = _pendingCantileverInsert;
            _pendingCantileverInsert = null;
            return pending;
        }

        internal static PendingCantileverWrite TakePendingCantileverNew()
        {
            var pending = _pendingCantileverNew;
            _pendingCantileverNew = null;
            return pending;
        }

        internal void AnswerCantileverRead(CantileverDesignRead read) => Raise(() => CantileverRead?.Invoke(read));
    }
}
