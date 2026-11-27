using System;
using System.Collections.Generic;
using RackCad.Application.Systems.Shared;
using RackCad.Application.VentanaRackCad;

namespace RackCad.Plugin.VentanaRackCad
{
    /// <summary>
    /// T5 pb (I-64, fifth batch): the door of a Push Back of the drawing (RACKPUSHBACKABRIR, RACKPUSHBACKAPLICAR, RACKPUSHBACKINSERTAR and
    /// RACKPUSHBACKNUEVO), with the rules of the other doors: the request waits BEFORE its command is handed over, it is kept with the
    /// drawing that was active, «Dejar de esperar» drops it, and the request that waits keeps its own list of views.
    /// </summary>
    public sealed partial class RackCadRackDoorPort : IRackCadPushBackDoorPort
    {
        internal const string PushBackReadCommand = "RACKPUSHBACKABRIR";
        internal const string PushBackWriteCommand = "RACKPUSHBACKAPLICAR";
        internal const string PushBackInsertCommand = "RACKPUSHBACKINSERTAR";
        internal const string PushBackNewCommand = "RACKPUSHBACKNUEVO";

        /// <summary>A write of a Push Back that waits for its command: the request (copies of the draft) and the drawing it is for.</summary>
        internal sealed record PendingPushBackWrite(PushBackDoorWrite Write, long Document)
        {
            public long RequestId => Write.RequestId;

            public string RackId => Write.RackId;
        }

        private static PendingRead _pendingPushBackRead;
        private static PendingPushBackWrite _pendingPushBackWrite;
        private static PendingPushBackWrite _pendingPushBackInsert;
        private static PendingPushBackWrite _pendingPushBackNew;

        public event Action<RackDesignRead> PushBackRead;

        public RackCadClassicResult RequestPushBackDesign(RackDoorRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.RackId))
                return RackCadClassicResult.Without(RackCadClassicStatus.Failed, "El rack no tiene una identidad válida.");

            return Send(PushBackReadCommand + " ",
                document => _pendingPushBackRead = new PendingRead(request.RequestId, request.RackId.Trim(), KeyOf(document)),
                () => _pendingPushBackRead = null);
        }

        public RackCadClassicResult RequestPushBackUpdate(PushBackDoorWrite write)
        {
            var refusal = PushBackDoorRules.RefusalOf(write);
            if (refusal != null) return RackCadClassicResult.Without(RackCadClassicStatus.Failed, refusal);

            // A new Push Back and «Insertar vista…» have their own commands, as in Dinámico and Selectivo.
            var command = write.NewRack ? PushBackNewCommand : write.Inserts ? PushBackInsertCommand : PushBackWriteCommand;
            return Send(command + " ",
                document => SetPushBackPending(write, new PendingPushBackWrite(
                    write with { RackId = write.RackId.Trim(), InsertViews = write.Inserts ? new List<RackViewAddress>(write.InsertViews) : null },
                    KeyOf(document))),
                () => SetPushBackPending(write, null));
        }

        private static void SetPushBackPending(PushBackDoorWrite write, PendingPushBackWrite pending)
        {
            if (write.NewRack) _pendingPushBackNew = pending;
            else if (write.Inserts) _pendingPushBackInsert = pending;
            else _pendingPushBackWrite = pending;
        }

        // One line of Abandon: «Dejar de esperar» forgets the Push Back request that waits.
        private static void AbandonPushBack(long requestId)
        {
            if (_pendingPushBackRead != null && _pendingPushBackRead.RequestId == requestId) _pendingPushBackRead = null;
            if (_pendingPushBackWrite != null && _pendingPushBackWrite.RequestId == requestId) _pendingPushBackWrite = null;
            if (_pendingPushBackInsert != null && _pendingPushBackInsert.RequestId == requestId) _pendingPushBackInsert = null;
            if (_pendingPushBackNew != null && _pendingPushBackNew.RequestId == requestId) _pendingPushBackNew = null;
        }

        internal static PendingRead TakePendingPushBackRead()
        {
            var pending = _pendingPushBackRead;
            _pendingPushBackRead = null;
            return pending;
        }

        internal static PendingPushBackWrite TakePendingPushBackWrite()
        {
            var pending = _pendingPushBackWrite;
            _pendingPushBackWrite = null;
            return pending;
        }

        internal static PendingPushBackWrite TakePendingPushBackInsert()
        {
            var pending = _pendingPushBackInsert;
            _pendingPushBackInsert = null;
            return pending;
        }

        internal static PendingPushBackWrite TakePendingPushBackNew()
        {
            var pending = _pendingPushBackNew;
            _pendingPushBackNew = null;
            return pending;
        }

        internal void AnswerPushBackRead(RackDesignRead read) => Raise(() => PushBackRead?.Invoke(read));
    }
}
