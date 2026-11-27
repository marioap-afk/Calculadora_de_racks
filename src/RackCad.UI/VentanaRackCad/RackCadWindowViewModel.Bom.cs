using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using RackCad.Application.Bom;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.Selective;
using RackCad.Application.VentanaRackCad;
using RackCad.Application.Workspace;
using RackCad.UI.Workspace;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// T4 bom (I-64 A-14, line D; prototype «bom», BM-01 to BM-11): the page «BOM y lista de racks» of the window. Its read is a request of
    /// the door like the others (one request in flight for the whole window, numbered, with «Dejar de esperar»), answered by RACKBOMLEER; the
    /// page itself is <see cref="RackBomPageViewModel"/>. The window gives it the drafts of its editors (the BOM of a draft is computed here,
    /// without reading the drawing), the search and the filters of the side column, the page of a rack and its navigation.
    /// </summary>
    public sealed partial class RackCadWindowViewModel : IRackBomPageHost
    {
        private IRackCadBomPort _bomDoor;

        // «Ver en el dibujo» of the page makes the rack active to use its navigation: the window stays on the page of the BOM meanwhile.
        private bool _bomKeepsPage;

        /// <summary>The page «BOM y lista de racks».</summary>
        public RackBomPageViewModel Bom { get; private set; }

        /// <summary>«Ver su BOM» of the page of a rack (BM-10): there is an active rack.</summary>
        public bool CanShowBomOfRack => Panel.Browser.ActiveRackId != null;

        /// <summary>«Ver su BOM»: the page «BOM y lista de racks» with the active rack chosen (BM-10).</summary>
        public void ShowBomOfActiveRack()
        {
            Contained(() =>
            {
                var rack = Panel.Browser.ActiveRackId;
                if (rack == null) return;
                Bom.ShowRack(rack);
                ShowPage(RackCadWindowPage.Bom);
            });
        }

        // Called once by the constructor: the page, the door to the BOM, and what the page follows.
        private void WireBom()
        {
            _bomDoor = _door as IRackCadBomPort;
            Bom = new RackBomPageViewModel(this);
            if (_bomDoor != null)
            {
                _bomDoor.BomRead += OnBomRead;
                _bomDoor.BomPageAsked += OnBomPageAsked;
            }

            // The table follows the search, the filters and the marks of the side column (BM-09), and the drafts of the editors.
            RackList.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(RackListViewModel.Groups)) Contained(Bom.Refresh);
            };
            Panel.Browser.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(WorkspaceBrowserViewModel.ActiveRackId) || string.IsNullOrEmpty(e.PropertyName))
                    OnPropertyChanged(nameof(CanShowBomOfRack));
            };
        }

        // The window presented a snapshot (from Present): the page follows the drawing and the commands; it sends nothing on its own.
        private void PresentBom()
        {
            var port = Panel.BrowserSummaryPort;
            Bom.Present(
                Panel.PresentedSession,
                Panel.PresentedBlock,
                _inFlight != null && _inFlight.IsBom,
                session => port == null || IsAlive(port, session),
                Panel.PresentedDrawingChangeCount); // T6 st: only a command that changed the drawing makes the BOM possibly out of date
        }

        // The page was shown (from ShowPage, a gesture of the user): the first time for a drawing, it is read.
        private void BomPageShown() => Bom.Shown();

        // RACKLISTA or RACKBOMTOTAL read the drawing inside themselves (correction 4 of the review): the reading is for the drawing active
        // NOW, the one where the command runs, so the window pulls the state of the host first and gives the reading to that session. The page
        // is shown after it has its reading, so showing it sends nothing.
        private void OnBomPageAsked(RackBomTab tab, RackBomReading reading)
        {
            Contained(() =>
            {
                Refresh();
                var session = Panel.PresentedSession;
                if (session.HasValue && reading != null) Bom.Deliver(session.Value, reading, readByRunningCommand: true);
                Bom.Asked(tab);
                ShowPage(RackCadWindowPage.Bom);
            });
        }

        // RACKBOMLEER took the request (Started): what runs now is the read (correction 1 of the review).
        private void BomTaken(long requestId) => Bom.ReadingTaken(requestId);

        // The text of the wait once RACKBOMLEER took the request: its answer comes inside the same command.
        private const string BomTakenText = "AutoCAD está leyendo el dibujo para el BOM: la respuesta llega al terminar la lectura.";

        // The answer of AutoCAD to the read of the BOM: only the answer to the request that waits, for the session of the request.
        private void OnBomRead(RackBomReading reading)
        {
            Contained(() =>
            {
                var request = _inFlight;
                if (reading == null || request == null || !request.IsBom || reading.RequestId != request.Id) return;
                _inFlight = null;
                RaiseWaiting();

                // Correction 5 of the review: the door keeps the drawing of the request and RACKBOMLEER reads nothing in another one, but the
                // door cannot see the sessions of the host (M-24). The answer arrives inside the command, in the drawing that is active: the
                // window pulls the state of the host and keeps the reading only when that drawing is the session of the request.
                Refresh();
                if (Panel.PresentedSession != request.Session)
                {
                    Bom.ReadingStopped(abandoned: false,
                        "La lectura n.º " + request.Id.ToString(CultureInfo.InvariantCulture) + " no es del dibujo para el que se pidió (el dibujo activo cambió): no se usó. Pulsa «Calcular» en el dibujo que quieras.");
                    return;
                }

                Bom.Deliver(request.Session, reading);
            });
        }

        // «Dejar de esperar» of a read of the BOM (from StopWaiting).
        private void BomStopped() => Bom.ReadingStopped(abandoned: true);

        bool IRackBomPageHost.HasBomDoor => _bomDoor != null;

        RackCadClassicResult IRackBomPageHost.RequestBomRead()
        {
            if (_bomDoor == null) return RackCadClassicResult.Without(RackCadClassicStatus.Failed, "Aquí no se puede leer el dibujo.");
            var session = Panel.PresentedSession;
            if (!session.HasValue) return RackCadClassicResult.Without(RackCadClassicStatus.NoDocument, "No hay un dibujo en la ventana.");

            // One request of the door waits at a time, for the whole window: a read of the BOM waits for the one before.
            if (_inFlight != null) return RackCadClassicResult.Without(RackCadClassicStatus.Busy, "la ventana espera otra respuesta de AutoCAD");

            // The request waits BEFORE it is handed over: AutoCAD may run the command, and answer, during the call (M-22).
            var id = ++_lastRequestId;
            _inFlight = new DoorRequest(id, false, session.Value, null, null, null, IsBom: true);
            Bom.ReadingStarted(id, session.Value);
            RaiseWaiting();

            RackCadClassicResult result;
            try
            {
                result = _bomDoor.RequestBomRead(new RackBomRequest(id, session.Value))
                         ?? RackCadClassicResult.Without(RackCadClassicStatus.Failed, "sin respuesta.");
            }
            catch (Exception ex)
            {
                result = RackCadClassicResult.Without(RackCadClassicStatus.Failed, ex.Message);
            }

            if (result.IsStarted) return result;
            if (_inFlight != null && _inFlight.Id == id)
            {
                _inFlight = null;
                Bom.ReadingStopped(abandoned: false);
            }

            RaiseWaiting();
            return result;
        }

        void IRackBomPageHost.StopWaiting() => StopWaiting();

        void IRackBomPageHost.OpenRack(string rackId) => OpenRack(rackId);

        void IRackBomPageHost.ViewInDrawing(string rackId, bool nextCopy) => _ = ViewBomRackAsync(rackId, nextCopy);

        // «Ver en el dibujo» / «Siguiente copia» of the page: the navigation of the page of the rack (F2), for that rack, as many times as
        // wanted, without leaving the page. The rack becomes the active one (as a click in the list), so the side column marks it.
        private async Task ViewBomRackAsync(string rackId, bool next)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(rackId)) return;
                if (!string.Equals(Panel.Browser.ActiveRackId, rackId, StringComparison.OrdinalIgnoreCase))
                {
                    _bomKeepsPage = true;
                    try
                    {
                        Panel.Browser.OpenRack(rackId);
                    }
                    finally
                    {
                        _bomKeepsPage = false;
                    }

                    RackList.RefreshStates();
                }

                if (!string.Equals(Panel.Browser.ActiveRackId, rackId, StringComparison.OrdinalIgnoreCase))
                {
                    Bom.Say("No se pudo ir al rack: el rack abierto tiene texto sin aplicar. Termínalo o descártalo en su página y vuelve a intentarlo.", true);
                    return;
                }

                var navigation = Panel.Navigation;
                if (!navigation.CanNavigate)
                {
                    Bom.Say(string.IsNullOrWhiteSpace(navigation.BlockedReason) ? "Ahora no se puede navegar en el dibujo." : navigation.BlockedReason, true);
                    return;
                }

                if (next && navigation.CanNextCopy) navigation.NextCopy();
                await navigation.ViewAsync();
                var copy = navigation.CopyText;
                var result = navigation.ResultText;
                Bom.Say(string.IsNullOrWhiteSpace(result) ? copy : string.IsNullOrWhiteSpace(copy) ? result : result + " · " + copy, false);
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        BillOfMaterials IRackBomPageHost.DraftBomOf(SessionId session, string rackId)
        {
            if (string.IsNullOrWhiteSpace(rackId)) return null;
            if (PushBackDraftBom(session, rackId) is BillOfMaterials pushBack) return pushBack; // T5 pb

            // The BOM of a draft is the one of its handler, computed on copies here (SD-02 / BM-02: the BOM of the draft of the classic):
            // a Selectivo over a system resolved from a COPY of its last valid design (ResolveCopy: the BOM imposes on what it reads), a
            // Dinámico over the system of its snapshot for the write (also a copy).
            if (_selectiveDrafts.TryGetValue((session, rackId), out var selective) && !selective.Editor.IsNewRack && selective.Editor.IsDirty)
            {
                var system = selective.Editor.Draft.ResolveCopy();
                return system == null ? null : SelectiveBomBuilder.Build(system, selective.Editor.Draft.Catalog);
            }

            if (_dynamicDrafts.TryGetValue((session, rackId), out var dynamic) && !dynamic.Editor.IsNewRack && dynamic.Editor.IsDirty)
            {
                var snapshot = dynamic.Editor.Draft.SnapshotForWrite();
                return snapshot?.System == null ? null : SystemBomBuilder.Build(snapshot.System, dynamic.Editor.Draft.Catalog);
            }

            return CamaDraftBomOf(session, rackId) ?? CantileverDraftBom(session, rackId); // T5 cm, ct: a cama, the BOM of its handler on a copy (null otherwise)
        }

        bool IRackBomPageHost.HasUnwrittenDraft(SessionId session, string rackId)
        {
            if (string.IsNullOrWhiteSpace(rackId)) return false;
            if (HasUnwrittenPushBack(session, rackId)) return true; // T5 pb
            if (_selectiveDrafts.TryGetValue((session, rackId), out var selective)) return !selective.Editor.IsNewRack && selective.Editor.IsDirty;
            if (HasUnwrittenCama(session, rackId) is bool cama) return cama; // T5 cm
            return (_dynamicDrafts.TryGetValue((session, rackId), out var dynamic) && !dynamic.Editor.IsNewRack && dynamic.Editor.IsDirty)
                   || HasUnwrittenCantilever(session, rackId); // T5 ct
        }

        IReadOnlyList<(string RackId, string Name)> IRackBomPageHost.UnwrittenDraftsOf(SessionId session)
        {
            var drafts = _selectiveDrafts
                .Where(p => p.Key.Session == session && !p.Value.Editor.IsNewRack && p.Value.Editor.IsDirty)
                .Select(p => (p.Key.RackId, SelectiveNameOf(p.Value)))
                .Concat(_dynamicDrafts
                    .Where(p => p.Key.Session == session && !p.Value.Editor.IsNewRack && p.Value.Editor.IsDirty)
                    .Select(p => (p.Key.RackId, !string.IsNullOrWhiteSpace(p.Value.Editor.Name) ? p.Value.Editor.Name.Trim() : RackList.NameOf(p.Key.RackId))))
                .Concat(UnwrittenCamaDraftsOf(session)) // T5 cm
                .Concat(UnwrittenCantileverDrafts(session)) // T5 ct
                .Concat(UnwrittenPushBackDrafts(session)) // T5 pb
                .OrderBy(d => d.Item2, RackListViewModel.NaturalNameComparer.Instance)
                .ToList();
            return drafts;
        }

        // BM-09: the same search (by name, ignoring case and accents), system and «Solo con cambios sin escribir» as the side column.
        bool IRackBomPageHost.PassesSideFilter(string rackId, string name, string kind)
        {
            var search = (RackList.SearchText ?? string.Empty).Trim();
            var system = RackList.SelectedKind ?? string.Empty;
            if (system.Length > 0 && !string.Equals(system, (kind ?? string.Empty).Trim(), StringComparison.OrdinalIgnoreCase)) return false;
            if (search.Length > 0
                && CultureInfo.InvariantCulture.CompareInfo.IndexOf(name ?? string.Empty, search, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) < 0)
                return false;
            if (RackList.OnlyChanged && Panel.PresentedSession.HasValue && !((IRackBomPageHost)this).StateOf(Panel.PresentedSession.Value, rackId).Changed)
                return false;
            return true;
        }

        // The mark of a rack, as the side column shows it: the draft of an editor of the window first, then the mark of the list.
        (string Kind, bool Changed) IRackBomPageHost.StateOf(SessionId session, string rackId)
        {
            var own = DraftStateOf(session, rackId);
            if (own.HasValue) return (own.Value.Kind, own.Value.Changed);
            var item = RackList.Groups.SelectMany(g => g.Items).FirstOrDefault(i => string.Equals(i.RackId, rackId, StringComparison.OrdinalIgnoreCase));
            if (item == null) return ("Limpio", false);
            return (item.StatusKind, !string.Equals(item.StatusKind, "Limpio", StringComparison.Ordinal));
        }
    }
}
