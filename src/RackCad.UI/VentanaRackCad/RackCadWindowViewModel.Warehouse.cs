using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using RackCad.Application.Persistence;
using RackCad.Application.VentanaRackCad;
using RackCad.Application.Workspace;
using RackCad.UI.Workspace;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// T5 al (I-64 A-14, line D; prototype «almacen»): the section «Almacén» of the window (question 1: between «BOM y lista de racks» and
    /// «Biblioteca de diseños») and «Llevar al almacén…» of the page of a rack. Its read and its placement are requests of the door like the
    /// others: ONE in flight for the whole window, numbered, with «Dejar de esperar», the answer only for its request and its drawing, the
    /// answer during the call (M-22) and the prune of a closed drawing. The page itself is <see cref="WarehousePageViewModel"/>. Without a door
    /// that implements <see cref="IRackCadWarehousePort"/>, the page says it cannot read or write the drawing.
    /// </summary>
    public sealed partial class RackCadWindowViewModel : IWarehousePageHost
    {
        private const string WarehouseTakenText =
            "AutoCAD ya está con la petición de «Almacén»: termina o cancela la orden en AutoCAD; la ventana recibirá su respuesta.";

        private IRackCadWarehousePort _warehouseDoor;

        /// <summary>The page «Almacén».</summary>
        public WarehousePageViewModel Warehouse { get; private set; }

        public bool IsAlmacenPage => _page == RackCadWindowPage.Almacen;

        // AL-S15: a rack selected in AutoCAD while «Almacén» is shown does not take the window to its page: selecting the planta that is
        // the seed is part of the page (question 3). The side column still follows the selection.
        private bool WarehouseKeepsPage => IsAlmacenPage;

        // ------------------------------------------------------------------ «Llevar al almacén…» (question 1)

        /// <summary>The page of a rack offers «Llevar al almacén…»: there is an active rack and the door of the page.</summary>
        public bool ShowTakeToWarehouse => _warehouseDoor != null && Panel.Browser.ActiveRackId != null;

        /// <summary>The active rack has a planta (by the last reading of the list) and the page is not placing.</summary>
        public bool CanTakeToWarehouse => ShowTakeToWarehouse && ActiveRackHasPlanta && !(Warehouse?.IsWriting ?? false);

        public string TakeToWarehouseReason => ShowTakeToWarehouse && !ActiveRackHasPlanta
            ? "Sin vista en planta: insértala con «Insertar vista…» (o RACKEDITAR) para usar este rack como semilla."
            : string.Empty;

        private bool ActiveRackHasPlanta => HasPlanta(Panel.Browser.ActiveRackId);

        /// <summary>«Llevar al almacén…»: the section «Almacén» with the active rack as the seed, read now (a gesture of the user).</summary>
        public void TakeActiveRackToWarehouse()
        {
            Contained(() =>
            {
                var rack = Panel.Browser.ActiveRackId;
                if (rack == null || !CanTakeToWarehouse) return;
                Warehouse.TakeSeed(rack);
                if (IsAlmacenPage) Warehouse.Shown();
                else ShowPage(RackCadWindowPage.Almacen);
            });
        }

        // Called once by the constructor: the page, the door, and what the page follows.
        private void WireWarehouse()
        {
            _warehouseDoor = _door as IRackCadWarehousePort;
            Warehouse = new WarehousePageViewModel(this);
            if (_warehouseDoor != null)
            {
                _warehouseDoor.WarehouseRead += OnWarehouseRead;
                _warehouseDoor.WarehousePlaced += OnWarehousePlaced;
                _door.RequestStarted += OnWarehouseRequestStarted;
            }

            // The seeds follow the list, the drafts and the door; «Llevar al almacén…» follows the active rack.
            RackList.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(RackListViewModel.Groups) || string.IsNullOrEmpty(e.PropertyName))
                {
                    Contained(Warehouse.Refresh);
                    RaiseTakeToWarehouse();
                }
            };
            Panel.Browser.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(WorkspaceBrowserViewModel.ActiveRackId) || string.IsNullOrEmpty(e.PropertyName)) RaiseTakeToWarehouse();
            };
            PropertyChanged += OnWindowChangedForWarehouse;
        }

        private void OnWindowChangedForWarehouse(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IsWaitingForAutoCad)) Contained(Warehouse.Refresh); // another request of the window waits, or not
        }

        private void RaiseTakeToWarehouse()
        {
            OnPropertyChanged(nameof(ShowTakeToWarehouse));
            OnPropertyChanged(nameof(CanTakeToWarehouse));
            OnPropertyChanged(nameof(TakeToWarehouseReason));
        }

        // The window presented a snapshot (from Present): the page follows the drawing and the commands; it sends nothing on its own.
        private void PresentWarehouse()
        {
            var port = Panel.BrowserSummaryPort;
            Warehouse.Present(
                Panel.PresentedSession,
                Panel.PresentedBlock,
                _inFlight != null && _inFlight.IsWarehouse,
                session => port == null || IsAlive(port, session));
            RaiseTakeToWarehouse();
        }

        // A page was shown (from ShowPage, a gesture of the user): «Almacén» reads the planta the first time for a drawing.
        private void WarehousePageShown(RackCadWindowPage page)
        {
            OnPropertyChanged(nameof(IsAlmacenPage));
            if (page == RackCadWindowPage.Almacen) Contained(Warehouse.Shown);
        }

        // RACKALMACENLEER or RACKALMACENCOLOCAR took the request (Started): from now the command is the page's own.
        private void OnWarehouseRequestStarted(long requestId)
        {
            Contained(() =>
            {
                if (_inFlight == null || !_inFlight.IsWarehouse || _inFlight.Id != requestId) return;
                if (!_inFlight.Started) _inFlight = _inFlight with { Started = true };
                Warehouse.Taken(requestId);
                RaiseWaiting();
            });
        }

        // The answer to the read: only for the request that waits, and only when the drawing active now is the one of the request (M-24).
        private void OnWarehouseRead(WarehouseReading reading)
        {
            Contained(() =>
            {
                var request = _inFlight;
                if (reading == null || request == null || !request.IsWarehouse || request.IsWrite || reading.RequestId != request.Id) return;
                _inFlight = null;
                RaiseWaiting();
                Refresh();
                if (Panel.PresentedSession != request.Session)
                {
                    Warehouse.ReadingStopped("La lectura n.º " + request.Id.ToString(CultureInfo.InvariantCulture)
                        + " no es del dibujo para el que se pidió (el dibujo activo cambió): no se usó. Pulsa «Volver a leer» en el dibujo que quieras.");
                    return;
                }

                Warehouse.Deliver(request.Session, reading);
            });
        }

        // The answer to «Colocar»: only for the request that waits; the page takes the reading made in the same command.
        private void OnWarehousePlaced(WarehousePlaced placed)
        {
            Contained(() =>
            {
                var request = _inFlight;
                if (placed == null || request == null || !request.IsWarehouse || !request.IsWrite || placed.RequestId != request.Id) return;
                _inFlight = null;
                RaiseWaiting();
                Warehouse.Placed(request.Session, placed);
                RaiseTakeToWarehouse();
            });
        }

        // «Dejar de esperar» of a request of the page (from StopWaiting), or its drawing was closed (from PruneDeadSessions).
        private void WarehouseStopped(DoorRequest request)
        {
            if (request.IsWrite) Warehouse.PlacingStopped("Se dejó de esperar a AutoCAD: si la orden llega a correr, no colocará nada.", false);
            else Warehouse.ReadingStopped(null);
            RaiseTakeToWarehouse();
        }

        // ------------------------------------------------------------------ IWarehousePageHost

        bool IWarehousePageHost.HasWarehouseDoor => _warehouseDoor != null;

        bool IWarehousePageHost.OtherRequestWaiting =>
            _inFlight != null && !(_inFlight.IsWarehouse && _inFlight.Session == Panel.PresentedSession);

        string IWarehousePageHost.ActiveRackId => Panel.Browser.ActiveRackId;

        IReadOnlyList<WarehouseRackOption> IWarehousePageHost.RacksOfTheList()
        {
            var racks = new List<WarehouseRackOption>();
            foreach (var item in RackList.Entries) // T5 al: the racks of the list, before its filters
                racks.Add(new WarehouseRackOption(item.RackId, RackList.NameOf(item.RackId), KindLabelOf(item.Kind), HasPlantaView(item.Views)));
            return racks
                .OrderBy(r => KindRank(RackList.KindOf(r.RackId)))
                .ThenBy(r => r.Name, RackListViewModel.NaturalNameComparer.Instance)
                .ToList();
        }

        bool IWarehousePageHost.HasUnwrittenDraft(SessionId session, string rackId) => ((IRackBomPageHost)this).StateOf(session, rackId).Changed;

        string IWarehousePageHost.RequestRead(string seedRackId, string layer, string preferredHandle)
        {
            if (_warehouseDoor == null) return "Aquí no se puede leer el dibujo.";
            var session = Panel.PresentedSession;
            if (!session.HasValue) return "Abre o crea un dibujo para usar «Almacén».";
            if (_inFlight != null) return DoorBusyText;

            // The request waits BEFORE it is handed over: AutoCAD may run the command, and answer, during the call (M-22).
            var id = ++_lastRequestId;
            _inFlight = new DoorRequest(id, false, session.Value, null, null, null, IsWarehouse: true); // no rack: never the request of an editor
            Warehouse.ReadingStarted(id, session.Value);
            RaiseWaiting();

            RackCadClassicResult result;
            try
            {
                result = _warehouseDoor.RequestWarehouseRead(new WarehouseReadRequest(id, session.Value, seedRackId, layer, preferredHandle))
                         ?? RackCadClassicResult.Without(RackCadClassicStatus.Failed, "sin respuesta.");
            }
            catch (Exception ex)
            {
                result = RackCadClassicResult.Without(RackCadClassicStatus.Failed, ex.Message);
            }

            if (result.IsStarted) return null;
            if (_inFlight != null && _inFlight.Id == id) _inFlight = null;
            Warehouse.ReadingStopped(null);
            RaiseWaiting();
            return RefusalText(result);
        }

        string IWarehousePageHost.RequestPlace(Func<long, SessionId, WarehousePlaceRequest> build)
        {
            if (_warehouseDoor == null) return "Aquí no se puede escribir el dibujo.";
            var session = Panel.PresentedSession;
            if (!session.HasValue) return "Abre o crea un dibujo para usar «Almacén».";
            if (_inFlight != null) return DoorBusyText;

            var id = ++_lastRequestId;
            var request = build(id, session.Value);
            if (request == null) return "No hay nada que colocar.";
            // No rack in the request: the branches of the editors (StopWaiting, OnRequestStarted) look for their draft by its rack, and the seed
            // may be a rack whose editor is open in the window.
            _inFlight = new DoorRequest(id, true, session.Value, null, null, null, IsWarehouse: true);
            Warehouse.PlacingStarted(id, session.Value, request);
            RaiseWaiting();
            RaiseTakeToWarehouse();

            RackCadClassicResult result;
            try
            {
                result = _warehouseDoor.RequestWarehousePlace(request) ?? RackCadClassicResult.Without(RackCadClassicStatus.Failed, "sin respuesta.");
            }
            catch (Exception ex)
            {
                result = RackCadClassicResult.Without(RackCadClassicStatus.Failed, ex.Message);
            }

            if (result.IsStarted) return null;
            if (_inFlight != null && _inFlight.Id == id) _inFlight = null;
            Warehouse.PlacingStopped(null, false);
            RaiseWaiting();
            RaiseTakeToWarehouse();
            return RefusalText(result);
        }

        void IWarehousePageHost.StopWaiting() => StopWaiting();

        WarehouseLayoutValues IWarehousePageHost.LoadValues() => (_settings as IRackCadWarehouseSettings)?.WarehouseValues;

        void IWarehousePageHost.SaveValues(WarehouseLayoutValues values) => (_settings as IRackCadWarehouseSettings)?.SetWarehouseValues(values);

        // ------------------------------------------------------------------ the planta of a rack, by the list

        private bool HasPlanta(string rackId) => rackId != null && HasPlantaView(RackList.ViewKindsOf(rackId));

        private static bool HasPlantaView(IReadOnlyList<string> views) =>
            views != null && views.Any(v => string.Equals((v ?? string.Empty).Trim(), RackEmbedDocument.ViewPlanta, StringComparison.OrdinalIgnoreCase));

        private static string KindLabelOf(string kind)
        {
            var known = RackListViewModel.KindOrder.FirstOrDefault(k => string.Equals(k.Kind, kind, StringComparison.OrdinalIgnoreCase));
            return known.Label ?? (kind ?? string.Empty);
        }

        private static int KindRank(string kind)
        {
            for (var i = 0; i < RackListViewModel.KindOrder.Count; i++)
                if (string.Equals(RackListViewModel.KindOrder[i].Kind, kind, StringComparison.OrdinalIgnoreCase)) return i;
            return RackListViewModel.KindOrder.Count;
        }
    }
}
