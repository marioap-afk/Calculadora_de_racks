using System;
using RackCad.Application.VentanaRackCad;

namespace RackCad.Plugin.VentanaRackCad
{
    /// <summary>
    /// T5 cm (I-64, quinta tanda): the door of a cama de rodamiento (RACKCAMAABRIR / RACKCAMAAPLICAR / RACKCAMANUEVO), with the rules of the
    /// other doors: the request waits BEFORE its command is handed over, it is kept with the drawing that was active, «Dejar de esperar» drops
    /// it, and the copy of the bed that waits is the door's own.
    /// </summary>
    public sealed partial class RackCadRackDoorPort : IRackCadCamaDoorPort
    {
        internal const string CamaReadCommand = "RACKCAMAABRIR";
        internal const string CamaWriteCommand = "RACKCAMAAPLICAR";
        internal const string CamaNewCommand = "RACKCAMANUEVO";

        internal sealed record PendingCamaWrite(CamaDoorWrite Write, long Document)
        {
            public long RequestId => Write.RequestId;

            public string RackId => Write.RackId;
        }

        private static PendingRead _pendingCamaRead;
        private static PendingCamaWrite _pendingCamaWrite;
        private static PendingCamaWrite _pendingCamaNew;

        public event Action<CamaDesignRead> CamaRead;

        public RackCadClassicResult RequestCamaDesign(RackDoorRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.RackId))
                return RackCadClassicResult.Without(RackCadClassicStatus.Failed, "La cama no tiene una identidad válida.");

            return Send(CamaReadCommand + " ",
                document => _pendingCamaRead = new PendingRead(request.RequestId, request.RackId.Trim(), KeyOf(document)),
                () => _pendingCamaRead = null);
        }

        public RackCadClassicResult RequestCamaWrite(CamaDoorWrite write)
        {
            if (write == null || string.IsNullOrWhiteSpace(write.RackId) || write.Bed == null)
                return RackCadClassicResult.Without(RackCadClassicStatus.Failed, "No hay una cama válida que escribir.");

            // The copy that waits is the door's: nothing the window does afterwards reaches what AutoCAD writes.
            var copy = write with { RackId = write.RackId.Trim(), Bed = CamaDraftRules.Copy(write.Bed) };
            if (write.NewRack)
            {
                return Send(CamaNewCommand + " ",
                    document => _pendingCamaNew = new PendingCamaWrite(copy, KeyOf(document)),
                    () => _pendingCamaNew = null);
            }

            return Send(CamaWriteCommand + " ",
                document => _pendingCamaWrite = new PendingCamaWrite(copy, KeyOf(document)),
                () => _pendingCamaWrite = null);
        }

        // One line of Abandon: «Dejar de esperar» forgets the cama request that waits.
        private static void AbandonCama(long requestId)
        {
            if (_pendingCamaRead != null && _pendingCamaRead.RequestId == requestId) _pendingCamaRead = null;
            if (_pendingCamaWrite != null && _pendingCamaWrite.RequestId == requestId) _pendingCamaWrite = null;
            if (_pendingCamaNew != null && _pendingCamaNew.RequestId == requestId) _pendingCamaNew = null;
        }

        internal static PendingRead TakePendingCamaRead()
        {
            var pending = _pendingCamaRead;
            _pendingCamaRead = null;
            return pending;
        }

        internal static PendingCamaWrite TakePendingCamaWrite()
        {
            var pending = _pendingCamaWrite;
            _pendingCamaWrite = null;
            return pending;
        }

        internal static PendingCamaWrite TakePendingCamaNew()
        {
            var pending = _pendingCamaNew;
            _pendingCamaNew = null;
            return pending;
        }

        internal void AnswerCamaRead(CamaDesignRead read) => Raise(() => CamaRead?.Invoke(read));
    }
}
