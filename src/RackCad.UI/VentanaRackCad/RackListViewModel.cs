using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.VentanaRackCad;
using RackCad.Application.Workspace;
using RackCad.UI.Workspace;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>One rack of the side column as the window shows it: a plain value with no event, no delegate and no visual element.</summary>
    public sealed record RackListItem(string RackId, string Name, string StatusText, string StatusKind, bool IsActive, bool IsSelected);

    /// <summary>One group of the side column: the racks of one system, with "shown of total" for that system.</summary>
    public sealed record RackListGroup(string KindLabel, string CountText, IReadOnlyList<RackListItem> Items);

    /// <summary>One option of the filter by system; an empty <see cref="Key"/> is "every system".</summary>
    public sealed record RackKindOption(string Key, string Label);

    /// <summary>
    /// The list of the racks of the drawing in the side column of the RackCad window (I-64 A-14, phase 2, C-02 to C-04): every rack of the
    /// drawing grouped by system, with the state of its draft, a search by name, a filter by system, "solo con cambios sin escribir" and
    /// "Seguir la selección de AutoCAD". It replaces the strip of tabs as the way to move between racks; the browser behind it keeps every
    /// rule of the tabs (one live surface, the boundaries of pending text, the drafts).
    ///
    /// It asks the summary port for the list only when it is presented or when the user asks to read it again; the host answers a
    /// presentation from memory or, when the list is not remembered, only while it drains, exactly as for the summary. The state of each
    /// rack is read from the memory of the edit port (never the drawing). It is synchronous and contains every failure: the list shows why
    /// it could not be read and the window keeps working.
    /// </summary>
    public sealed class RackListViewModel : ObservableObject
    {
        /// <summary>The order of the groups and the label of each Kind of the envelope; any other Kind goes last with its raw name.</summary>
        private static readonly IReadOnlyList<(string Kind, string Label)> KindOrder = new[]
        {
            ("selective", "Selectivo"),
            ("pushback", "Push Back"),
            ("dynamic", "Dinámico"),
            ("cantilever", "Cantilever"),
            ("cabecera", "Cabecera"),
            ("cama", "Cama"),
        };

        private const string AllKinds = "";

        private readonly IWorkspaceBrowserSummaryPort _port;
        private readonly IWorkspacePilotEditPort _pilotEdit;
        private readonly WorkspaceBrowserViewModel _browser;

        private SessionId? _session;
        private bool _canRead;
        private WorkspaceActionBlock _block = WorkspaceActionBlock.NotStarted;
        private SessionId? _listSession;
        private IReadOnlyList<BrowserRackListEntry> _racks = Array.Empty<BrowserRackListEntry>();
        private string _statusText = string.Empty;

        private string _searchText = string.Empty;
        private string _selectedKind = AllKinds;
        private bool _onlyChanged;

        private IReadOnlyList<RackListGroup> _groups = Array.Empty<RackListGroup>();
        private IReadOnlyList<RackKindOption> _kindOptions = new[] { new RackKindOption(AllKinds, "Todos los sistemas") };
        private string _shownText = string.Empty;
        private int _shownCount;
        private int _changedCount;

        /// <param name="port">The summary port that also offers the list; null leaves the list empty.</param>
        /// <param name="pilotEdit">The edit port, for the state of the draft of each rack; optional.</param>
        /// <param name="browser">The browser of the panel: opening a rack goes through it.</param>
        public RackListViewModel(IWorkspaceBrowserSummaryPort port, IWorkspacePilotEditPort pilotEdit, WorkspaceBrowserViewModel browser)
        {
            _port = port;
            _pilotEdit = pilotEdit;
            _browser = browser ?? throw new ArgumentNullException(nameof(browser));
        }

        /// <summary>Raised after the user opened a rack of the list, so the window shows the page of the rack.</summary>
        public event Action RackOpened;

        // ------------------------------------------------------------------ what the side column shows

        public IReadOnlyList<RackListGroup> Groups => _groups;

        public IReadOnlyList<RackKindOption> KindOptions => _kindOptions;

        /// <summary>Every rack of the drawing that was read, before any filter.</summary>
        public int RackCount => _racks.Count;

        /// <summary>The racks with changes that are not written yet (a protected draft).</summary>
        public int ChangedCount => _changedCount;

        public int ShownCount => _shownCount;

        /// <summary>"Mostrando N de M"; empty while there is no list.</summary>
        public string ShownText => _shownText;

        /// <summary>Why there is no list, or that it is being read; empty when the list is shown.</summary>
        public string StatusText => _statusText;

        /// <summary>The text of an empty result of the filter; empty while some rack is shown or there is no list.</summary>
        public string NoResultsText => _racks.Count > 0 && _shownCount == 0 ? "Ningún rack coincide con el filtro." : string.Empty;

        public string SearchText
        {
            get => _searchText;
            set
            {
                var next = value ?? string.Empty;
                if (string.Equals(_searchText, next, StringComparison.Ordinal)) return;
                _searchText = next;
                OnPropertyChanged(nameof(SearchText));
                Rebuild();
            }
        }

        /// <summary>The key of the system of the filter; empty is every system.</summary>
        public string SelectedKind
        {
            get => _selectedKind;
            set
            {
                var next = value ?? AllKinds;
                if (string.Equals(_selectedKind, next, StringComparison.Ordinal)) return;
                _selectedKind = next;
                OnPropertyChanged(nameof(SelectedKind));
                Rebuild();
            }
        }

        public bool OnlyChanged
        {
            get => _onlyChanged;
            set
            {
                if (_onlyChanged == value) return;
                _onlyChanged = value;
                OnPropertyChanged(nameof(OnlyChanged));
                Rebuild();
            }
        }

        /// <summary>"Seguir la selección de AutoCAD": the switch lives in the browser, which applies it to the selection.</summary>
        public bool FollowSelection
        {
            get => _browser.FollowSelection;
            set
            {
                if (_browser.FollowSelection == value) return;
                _browser.FollowSelection = value;
                OnPropertyChanged(nameof(FollowSelection));
                OnPropertyChanged(nameof(HasSelectionBox));
                OnPropertyChanged(nameof(SelectionBoxText));
            }
        }

        /// <summary>The rack selected in AutoCAD waits behind the active one: only while the selection is not followed.</summary>
        public bool HasSelectionBox =>
            !_browser.FollowSelection
            && _browser.SelectionRackId != null
            && !string.Equals(_browser.SelectionRackId, _browser.ActiveRackId, StringComparison.Ordinal);

        public string SelectionBoxText => HasSelectionBox ? "Seleccionado en AutoCAD: " + NameOf(_browser.SelectionRackId) : string.Empty;

        // ------------------------------------------------------------------ what the window tells it

        /// <summary>
        /// The window presented a snapshot: the list follows its session and, while the drawing can be read and nothing blocks the actions,
        /// asks the port for the list (the host answers from memory or, when it must read, only during a drain).
        /// </summary>
        public void Present(SessionId? session, bool canReadDrawing, WorkspaceActionBlock block)
        {
            _session = session;
            _canRead = canReadDrawing;
            _block = block;

            if (_listSession != session)
            {
                _listSession = null;
                _racks = Array.Empty<BrowserRackListEntry>();
            }

            Load(userRequested: false);
            Rebuild();
        }

        /// <summary>The state of a draft may have changed (an edit, an update, a discard): the marks and the counts are shown again.</summary>
        public void RefreshStates() => Rebuild();

        /// <summary>"Volver a leer la lista": the explicit read of the user, which reads the drawing again.</summary>
        public void Reload()
        {
            Load(userRequested: true);
            Rebuild();
        }

        /// <summary>Opens the rack in the browser and tells the window to show its page.</summary>
        public void Open(string rackId)
        {
            if (string.IsNullOrWhiteSpace(rackId)) return;
            _browser.OpenRack(rackId);
            Rebuild();
            RackOpened?.Invoke();
        }

        /// <summary>"Abrir" of the rack selected in AutoCAD while the selection is not followed.</summary>
        public void OpenSelected()
        {
            var selected = _browser.SelectionRackId;
            if (selected != null) Open(selected);
        }

        /// <summary>The name the list knows for a rack, or its id when the list does not have it.</summary>
        public string NameOf(string rackId)
        {
            var entry = _racks.FirstOrDefault(r => string.Equals(r.RackId, rackId, StringComparison.OrdinalIgnoreCase));
            return entry == null ? rackId : DisplayName(entry);
        }

        // ------------------------------------------------------------------ internals

        private void Load(bool userRequested)
        {
            if (_port == null)
            {
                SetStatus("La lista de racks no está disponible.");
                return;
            }

            if (!_session.HasValue)
            {
                SetStatus("No hay un dibujo abierto.");
                return;
            }

            if (!_canRead || _block != WorkspaceActionBlock.None)
            {
                if (_listSession == null) SetStatus("La lista se leerá cuando el dibujo esté libre.");
                return;
            }

            BrowserRackList list;
            try
            {
                list = _port.ReadRackList(new WorkspaceRequest(_session.Value), userRequested)
                    ?? BrowserRackList.Without(BrowserSummaryStatus.Failed, "sin respuesta.");
            }
            catch (Exception ex)
            {
                list = BrowserRackList.Without(BrowserSummaryStatus.Failed, ex.Message);
            }

            switch (list.Status)
            {
                case BrowserSummaryStatus.Ready:
                    _racks = list.Racks ?? Array.Empty<BrowserRackListEntry>();
                    _listSession = _session;
                    SetStatus(_racks.Count == 0 ? "Este dibujo no tiene racks de RackCad." : string.Empty);
                    break;
                case BrowserSummaryStatus.Busy:
                    if (_listSession == null) SetStatus("Leyendo la lista del dibujo…");
                    break;
                case BrowserSummaryStatus.Failed:
                    SetStatus("No se pudo leer la lista: " + (string.IsNullOrWhiteSpace(list.Reason) ? "sin detalle." : list.Reason));
                    break;
                default:
                    if (_listSession == null) SetStatus("La lista de racks no está disponible.");
                    break;
            }
        }

        private void SetStatus(string text)
        {
            if (string.Equals(_statusText, text, StringComparison.Ordinal)) return;
            _statusText = text;
            OnPropertyChanged(nameof(StatusText));
        }

        private void Rebuild()
        {
            var compare = CultureInfo.InvariantCulture.CompareInfo;
            var search = _searchText.Trim();
            var active = _browser.ActiveRackId;
            var selected = _browser.SelectionRackId;

            var states = new Dictionary<string, (string Text, string Kind, bool Changed)>(StringComparer.OrdinalIgnoreCase);
            var changed = 0;
            foreach (var rack in _racks)
            {
                var state = StateOf(rack.RackId);
                states[rack.RackId] = state;
                if (state.Changed) changed++;
            }

            var groups = new List<RackListGroup>();
            var shown = 0;
            foreach (var (key, label, racks) in GroupsInOrder())
            {
                var items = new List<RackListItem>();
                foreach (var rack in racks)
                {
                    var state = states[rack.RackId];
                    if (_selectedKind.Length > 0 && !string.Equals(_selectedKind, key, StringComparison.OrdinalIgnoreCase)) continue;
                    if (_onlyChanged && !state.Changed) continue;
                    var name = DisplayName(rack);
                    if (search.Length > 0
                        && compare.IndexOf(name, search, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) < 0) continue;

                    items.Add(new RackListItem(
                        rack.RackId,
                        name,
                        state.Text,
                        state.Kind,
                        string.Equals(rack.RackId, active, StringComparison.Ordinal),
                        string.Equals(rack.RackId, selected, StringComparison.Ordinal)));
                }

                shown += items.Count;
                if (items.Count > 0)
                {
                    groups.Add(new RackListGroup(
                        label,
                        items.Count.ToString(CultureInfo.InvariantCulture) + " de " + racks.Count.ToString(CultureInfo.InvariantCulture),
                        items));
                }
            }

            _groups = groups;
            _shownCount = shown;
            _changedCount = changed;
            _shownText = _racks.Count == 0
                ? string.Empty
                : "Mostrando " + shown.ToString(CultureInfo.InvariantCulture) + " de " + _racks.Count.ToString(CultureInfo.InvariantCulture);
            RebuildKindOptions();

            OnPropertyChanged(nameof(Groups));
            OnPropertyChanged(nameof(RackCount));
            OnPropertyChanged(nameof(ChangedCount));
            OnPropertyChanged(nameof(ShownCount));
            OnPropertyChanged(nameof(ShownText));
            OnPropertyChanged(nameof(NoResultsText));
            OnPropertyChanged(nameof(HasSelectionBox));
            OnPropertyChanged(nameof(SelectionBoxText));
            OnPropertyChanged(nameof(FollowSelection));
        }

        // The groups in the order of the window: the known systems first, in their order, then any other Kind by name.
        private IEnumerable<(string Key, string Label, List<BrowserRackListEntry> Racks)> GroupsInOrder()
        {
            var byKind = new Dictionary<string, List<BrowserRackListEntry>>(StringComparer.OrdinalIgnoreCase);
            foreach (var rack in _racks)
            {
                var kind = (rack.Kind ?? string.Empty).Trim();
                if (!byKind.TryGetValue(kind, out var list))
                {
                    list = new List<BrowserRackListEntry>();
                    byKind[kind] = list;
                }

                list.Add(rack);
            }

            foreach (var (kind, label) in KindOrder)
            {
                if (byKind.TryGetValue(kind, out var racks)) yield return (kind, label, Sorted(racks));
            }

            foreach (var kind in byKind.Keys.OrderBy(k => k, StringComparer.OrdinalIgnoreCase))
            {
                if (KindOrder.Any(known => string.Equals(known.Kind, kind, StringComparison.OrdinalIgnoreCase))) continue;
                yield return (kind, kind.Length == 0 ? "Sin tipo" : kind, Sorted(byKind[kind]));
            }
        }

        // Inside a group the racks go by name, with numbers in their natural order (PB 2 before PB 10), then by id.
        private static List<BrowserRackListEntry> Sorted(List<BrowserRackListEntry> racks) =>
            racks.OrderBy(r => DisplayName(r), NaturalNameComparer.Instance)
                 .ThenBy(r => r.RackId, StringComparer.OrdinalIgnoreCase)
                 .ToList();

        private void RebuildKindOptions()
        {
            var options = new List<RackKindOption> { new(AllKinds, "Todos los sistemas") };
            foreach (var (key, label, _) in GroupsInOrder()) options.Add(new RackKindOption(key, label));

            if (options.Count == _kindOptions.Count && options.SequenceEqual(_kindOptions)) return;
            _kindOptions = options;
            OnPropertyChanged(nameof(KindOptions));
        }

        // The state of the draft of the rack, from the memory of the edit port. "Changed" is a draft whose work would be lost.
        private (string Text, string Kind, bool Changed) StateOf(string rackId)
        {
            if (_pilotEdit == null || !_session.HasValue) return ("Sin borrador", "Limpio", false);

            PilotFieldView view;
            try
            {
                view = _pilotEdit.GetField(new WorkspaceRequest(_session.Value), rackId);
            }
            catch (Exception)
            {
                return ("Estado desconocido", "Desconocido", false);
            }

            if (!view.Exists || view.State == PilotFieldState.NotOpen) return ("Sin borrador", "Limpio", false);

            var stale = view.IsPossiblyStale ? " (posiblemente obsoleto)" : string.Empty;
            switch (view.State)
            {
                case PilotFieldState.Clean: return ("Limpio" + stale, view.IsPossiblyStale ? "Obsoleto" : "Limpio", view.IsProtected);
                case PilotFieldState.Dirty: return ("Modificado" + stale, view.IsPossiblyStale ? "Obsoleto" : "Modificado", true);
                case PilotFieldState.Conflict: return ("Conflicto", "Obsoleto", true);
                case PilotFieldState.Applying: return ("Aplicando", "Modificado", true);
                case PilotFieldState.Unknown: return ("Resultado desconocido", "Obsoleto", true);
                case PilotFieldState.Error: return ("Error", "Obsoleto", true);
                default: return ("Modificado", "Modificado", true);
            }
        }

        private static string DisplayName(BrowserRackListEntry rack) =>
            string.IsNullOrWhiteSpace(rack.Name) ? "(sin nombre)" : rack.Name.Trim();

        // Compares names with their digit runs as numbers, ignoring case: "PB 2" < "PB 10".
        private sealed class NaturalNameComparer : IComparer<string>
        {
            public static readonly NaturalNameComparer Instance = new();

            public int Compare(string x, string y)
            {
                x ??= string.Empty;
                y ??= string.Empty;
                int i = 0, j = 0;
                while (i < x.Length && j < y.Length)
                {
                    if (char.IsDigit(x[i]) && char.IsDigit(y[j]))
                    {
                        var startX = i;
                        var startY = j;
                        while (i < x.Length && char.IsDigit(x[i])) i++;
                        while (j < y.Length && char.IsDigit(y[j])) j++;
                        var numberX = x.Substring(startX, i - startX).TrimStart('0');
                        var numberY = y.Substring(startY, j - startY).TrimStart('0');
                        if (numberX.Length != numberY.Length) return numberX.Length.CompareTo(numberY.Length);
                        var byDigits = string.CompareOrdinal(numberX, numberY);
                        if (byDigits != 0) return byDigits;
                        continue;
                    }

                    var byChar = char.ToUpperInvariant(x[i]).CompareTo(char.ToUpperInvariant(y[j]));
                    if (byChar != 0) return byChar;
                    i++;
                    j++;
                }

                return (x.Length - i).CompareTo(y.Length - j);
            }
        }
    }
}
