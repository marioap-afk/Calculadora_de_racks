using System;
using System.Collections.Generic;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Shared;
using RackCad.Application.VentanaRackCad;

namespace RackCad.Plugin.VentanaRackCad
{
    /// <summary>
    /// T4 cb: the door of a cabecera of its own (RACKCABECERAABRIR / RACKCABECERAAPLICAR), with the rules of the other doors: the request
    /// waits BEFORE its command is handed over, it is kept with the drawing that was active, «Dejar de esperar» drops it, and the copy that
    /// waits is the door's own. T7 cab: «Insertar vista…» (RACKCABECERAINSERTAR) and a new cabecera of «Nuevo» (RACKCABECERANUEVO) have
    /// their own commands and their own slots, as in the doors of the other systems, and the request that waits keeps its own list of views.
    /// </summary>
    public sealed partial class RackCadRackDoorPort : IRackCadCabeceraDoorPort
    {
        internal const string CabeceraReadCommand = "RACKCABECERAABRIR";
        internal const string CabeceraWriteCommand = "RACKCABECERAAPLICAR";
        internal const string CabeceraInsertCommand = "RACKCABECERAINSERTAR"; // T7 cab
        internal const string CabeceraNewCommand = "RACKCABECERANUEVO"; // T7 cab

        internal sealed record PendingCabeceraWrite(CabeceraDoorWrite Write, long Document)
        {
            public long RequestId => Write.RequestId;

            public string RackId => Write.RackId;
        }

        private static PendingRead _pendingCabeceraRead;
        private static PendingCabeceraWrite _pendingCabeceraWrite;
        private static PendingCabeceraWrite _pendingCabeceraInsert; // T7 cab
        private static PendingCabeceraWrite _pendingCabeceraNew; // T7 cab

        public event Action<CabeceraDesignRead> CabeceraRead;

        public RackCadClassicResult RequestCabeceraDesign(RackDoorRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.RackId))
                return RackCadClassicResult.Without(RackCadClassicStatus.Failed, "La cabecera no tiene una identidad válida.");

            return Send(CabeceraReadCommand + " ",
                document => _pendingCabeceraRead = new PendingRead(request.RequestId, request.RackId.Trim(), KeyOf(document)),
                () => _pendingCabeceraRead = null);
        }

        public RackCadClassicResult RequestCabeceraUpdate(CabeceraDoorWrite write)
        {
            var refusal = CabeceraDoorRules.RefusalOf(write); // T7 cab: the rule of the door, as Push Back
            if (refusal != null) return RackCadClassicResult.Without(RackCadClassicStatus.Failed, refusal);

            // T7 cab: a new cabecera and «Insertar vista…» have their own commands, as in the other doors.
            var command = write.NewRack ? CabeceraNewCommand : write.Inserts ? CabeceraInsertCommand : CabeceraWriteCommand;
            return Send(command + " ",
                document => SetCabeceraPending(write, new PendingCabeceraWrite(
                    write with
                    {
                        RackId = write.RackId.Trim(),
                        Cabecera = new RackFrameProjectStore().DeepCopy(write.Cabecera),
                        InsertViews = write.Inserts ? new List<RackViewAddress>(write.InsertViews) : null,
                    }, KeyOf(document))),
                () => SetCabeceraPending(write, null));
        }

        // T7 cab: the slot of each command.
        private static void SetCabeceraPending(CabeceraDoorWrite write, PendingCabeceraWrite pending)
        {
            if (write.NewRack) _pendingCabeceraNew = pending;
            else if (write.Inserts) _pendingCabeceraInsert = pending;
            else _pendingCabeceraWrite = pending;
        }

        // One line of Abandon: «Dejar de esperar» forgets the cabecera request that waits.
        private static void AbandonCabecera(long requestId)
        {
            if (_pendingCabeceraRead != null && _pendingCabeceraRead.RequestId == requestId) _pendingCabeceraRead = null;
            if (_pendingCabeceraWrite != null && _pendingCabeceraWrite.RequestId == requestId) _pendingCabeceraWrite = null;
            if (_pendingCabeceraInsert != null && _pendingCabeceraInsert.RequestId == requestId) _pendingCabeceraInsert = null; // T7 cab
            if (_pendingCabeceraNew != null && _pendingCabeceraNew.RequestId == requestId) _pendingCabeceraNew = null; // T7 cab
        }

        internal static PendingRead TakePendingCabeceraRead()
        {
            var pending = _pendingCabeceraRead;
            _pendingCabeceraRead = null;
            return pending;
        }

        internal static PendingCabeceraWrite TakePendingCabeceraWrite()
        {
            var pending = _pendingCabeceraWrite;
            _pendingCabeceraWrite = null;
            return pending;
        }

        internal static PendingCabeceraWrite TakePendingCabeceraInsert() // T7 cab
        {
            var pending = _pendingCabeceraInsert;
            _pendingCabeceraInsert = null;
            return pending;
        }

        internal static PendingCabeceraWrite TakePendingCabeceraNew() // T7 cab
        {
            var pending = _pendingCabeceraNew;
            _pendingCabeceraNew = null;
            return pending;
        }

        internal void AnswerCabeceraRead(CabeceraDesignRead read) => Raise(() => CabeceraRead?.Invoke(read));
    }
}
