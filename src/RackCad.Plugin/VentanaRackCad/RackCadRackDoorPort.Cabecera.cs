using System;
using RackCad.Application.Persistence;
using RackCad.Application.VentanaRackCad;

namespace RackCad.Plugin.VentanaRackCad
{
    /// <summary>
    /// T4 cb: the door of a cabecera of its own (RACKCABECERAABRIR / RACKCABECERAAPLICAR), with the rules of the other doors: the request
    /// waits BEFORE its command is handed over, it is kept with the drawing that was active, «Dejar de esperar» drops it, and the copy that
    /// waits is the door's own.
    /// </summary>
    public sealed partial class RackCadRackDoorPort : IRackCadCabeceraDoorPort
    {
        internal const string CabeceraReadCommand = "RACKCABECERAABRIR";
        internal const string CabeceraWriteCommand = "RACKCABECERAAPLICAR";

        internal sealed record PendingCabeceraWrite(CabeceraDoorWrite Write, long Document)
        {
            public long RequestId => Write.RequestId;

            public string RackId => Write.RackId;
        }

        private static PendingRead _pendingCabeceraRead;
        private static PendingCabeceraWrite _pendingCabeceraWrite;

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
            if (write == null || string.IsNullOrWhiteSpace(write.RackId) || write.Cabecera == null)
                return RackCadClassicResult.Without(RackCadClassicStatus.Failed, "No hay una cabecera válida que escribir.");

            return Send(CabeceraWriteCommand + " ",
                document => _pendingCabeceraWrite = new PendingCabeceraWrite(
                    write with { RackId = write.RackId.Trim(), Cabecera = new RackFrameProjectStore().DeepCopy(write.Cabecera) }, KeyOf(document)),
                () => _pendingCabeceraWrite = null);
        }

        // One line of Abandon: «Dejar de esperar» forgets the cabecera request that waits.
        private static void AbandonCabecera(long requestId)
        {
            if (_pendingCabeceraRead != null && _pendingCabeceraRead.RequestId == requestId) _pendingCabeceraRead = null;
            if (_pendingCabeceraWrite != null && _pendingCabeceraWrite.RequestId == requestId) _pendingCabeceraWrite = null;
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

        internal void AnswerCabeceraRead(CabeceraDesignRead read) => Raise(() => CabeceraRead?.Invoke(read));
    }
}
