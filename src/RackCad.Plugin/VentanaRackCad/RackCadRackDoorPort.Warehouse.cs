using System;
using RackCad.Application.VentanaRackCad;

namespace RackCad.Plugin.VentanaRackCad
{
    /// <summary>
    /// T5 al (I-64, prototype «almacen»): the door of the page «Almacén» of the RackCad window, with the rules of the door to a rack
    /// (<see cref="Send"/>: an active document, no command in progress, the request kept before it is handed over and the focus back to the
    /// drawing). The read (RACKALMACENLEER, no undo marker) and the write (RACKALMACENCOLOCAR, one «U») live in
    /// <c>RackLayoutCommands.Ventana.cs</c>; each takes its request once and answers with its number, and a request the window abandoned
    /// (<see cref="Abandon"/>) is dropped, so a late command does nothing. This file holds no AutoCAD transaction and no lock.
    /// </summary>
    public sealed partial class RackCadRackDoorPort : IRackCadWarehousePort
    {
        /// <summary>A read of the page that waits for its command: the request and the drawing it is for.</summary>
        internal sealed record PendingWarehouseRead(WarehouseReadRequest Request, long Document)
        {
            public long RequestId => Request.RequestId;
        }

        /// <summary>A placement of the page that waits for its command: the request (its own copy of the values) and the drawing it is for.</summary>
        internal sealed record PendingWarehousePlace(WarehousePlaceRequest Request, long Document)
        {
            public long RequestId => Request.RequestId;
        }

        private static PendingWarehouseRead _pendingWarehouseRead;
        private static PendingWarehousePlace _pendingWarehousePlace;

        public event Action<WarehouseReading> WarehouseRead;

        public event Action<WarehousePlaced> WarehousePlaced;

        public RackCadClassicResult RequestWarehouseRead(WarehouseReadRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.SeedRackId))
                return RackCadClassicResult.Without(RackCadClassicStatus.Failed, "Elige el rack semilla.");

            var copy = request with { SeedRackId = request.SeedRackId.Trim(), Layer = request.Layer?.Trim() };
            return Send(WarehouseLayoutRules.ReadCommand + " ",
                document => _pendingWarehouseRead = new PendingWarehouseRead(copy, KeyOf(document)),
                () => _pendingWarehouseRead = null);
        }

        public RackCadClassicResult RequestWarehousePlace(WarehousePlaceRequest request)
        {
            if (request?.Values == null || string.IsNullOrWhiteSpace(request.SeedRackId) || string.IsNullOrWhiteSpace(request.Fingerprint))
                return RackCadClassicResult.Without(RackCadClassicStatus.Failed, "No hay nada que colocar: vuelve a leer la planta.");

            // Records with values only: the copy is the request itself, trimmed; nothing the page does later reaches it.
            var copy = request with { SeedRackId = request.SeedRackId.Trim(), Layer = request.Layer?.Trim(), Values = request.Values with { } };
            return Send(WarehouseLayoutRules.PlaceCommand + " ",
                document => _pendingWarehousePlace = new PendingWarehousePlace(copy, KeyOf(document)),
                () => _pendingWarehousePlace = null);
        }

        /// <summary>The read of the page that waits, taken once by its command; null when there is none.</summary>
        internal static PendingWarehouseRead TakePendingWarehouseRead()
        {
            var pending = _pendingWarehouseRead;
            _pendingWarehouseRead = null;
            return pending;
        }

        /// <summary>The placement of the page that waits, taken once by its command; null when there is none.</summary>
        internal static PendingWarehousePlace TakePendingWarehousePlace()
        {
            var pending = _pendingWarehousePlace;
            _pendingWarehousePlace = null;
            return pending;
        }

        // «Dejar de esperar» of a request of the page (called by Abandon).
        private static void AbandonWarehouse(long requestId)
        {
            if (_pendingWarehouseRead != null && _pendingWarehouseRead.RequestId == requestId) _pendingWarehouseRead = null;
            if (_pendingWarehousePlace != null && _pendingWarehousePlace.RequestId == requestId) _pendingWarehousePlace = null;
        }

        internal void AnswerWarehouseRead(WarehouseReading reading) => Raise(() => WarehouseRead?.Invoke(reading));

        internal void AnswerWarehousePlace(WarehousePlaced placed) => Raise(() => WarehousePlaced?.Invoke(placed));

        public event Action<WarehousePageAsk> WarehousePageAsked;

        public event Action<WarehousePageAsk, WarehouseReading> WarehouseShortcutRead;

        /// <summary>T6 rl: RLY or RR ask the page «Almacén» what to read; the window answers in the ask, during the call.</summary>
        internal void AskWarehousePage(WarehousePageAsk ask) => Raise(() => WarehousePageAsked?.Invoke(ask));

        /// <summary>T6 rl: RLY or RR read the drawing inside themselves: the window shows the page, on their tab, with that reading.</summary>
        internal void AnswerWarehouseShortcut(WarehousePageAsk ask, WarehouseReading reading) => Raise(() => WarehouseShortcutRead?.Invoke(ask, reading));
    }
}
