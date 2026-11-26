using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using RackCad.Application.Bom;
using RackCad.Application.VentanaRackCad;
using RackCad.Application.Workspace;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>What the page needs of the window: the door, the drafts of its editors, the side column and the page of a rack.</summary>
    internal interface IRackBomPageHost
    {
        /// <summary>The window has the door that reads the BOM of the drawing.</summary>
        bool HasBomDoor { get; }

        /// <summary>Hands RACKBOMLEER to AutoCAD for the drawing of the window; Started, or why not (Busy also when another request waits).</summary>
        RackCadClassicResult RequestBomRead();

        /// <summary>«Dejar de esperar» of the window.</summary>
        void StopWaiting();

        /// <summary>«Abrir»: the page of the rack.</summary>
        void OpenRack(string rackId);

        /// <summary>«Ver en el dibujo» or «Siguiente copia», with the navigation of the page of the rack, staying on this page.</summary>
        void ViewInDrawing(string rackId, bool nextCopy);

        /// <summary>The BOM of the draft of the editor of the window of that rack, when it has changes not written; null otherwise.</summary>
        BillOfMaterials DraftBomOf(SessionId session, string rackId);

        /// <summary>The rack has a draft with changes not written in an editor of the window (cheap: no BOM is computed).</summary>
        bool HasUnwrittenDraft(SessionId session, string rackId);

        /// <summary>The racks of the drawing with changes not written in the editors of the window, in the order of the list.</summary>
        IReadOnlyList<(string RackId, string Name)> UnwrittenDraftsOf(SessionId session);

        /// <summary>The rack passes the search and the filters of the side column (BM-09).</summary>
        bool PassesSideFilter(string rackId, string name, string kind);

        /// <summary>The state of the rack in the side column: «Limpio», «Modificado» or «Obsoleto», and whether it has changes not written.</summary>
        (string Kind, bool Changed) StateOf(SessionId session, string rackId);
    }

    /// <summary>One rack of the table «Racks del dibujo»: plain values.</summary>
    public sealed record RackBomRowItem(
        string RackId,
        string Name,
        string Views,
        string Kind,
        string Copies,
        string Components,
        string ComponentsTip,
        string Pieces,
        string InDrawing,
        string Tag,
        string TagKind,
        string StatusKind,
        string AutomationName);

    /// <summary>One line of a warning of the page, with «Abrir» when it is a rack.</summary>
    public sealed record RackBomNoticeItem(string RackId, string Name, string Reason, bool CanOpen, string OpenLabel, string OpenAutomationName);

    /// <summary>One piece of a component (or of a flat BOM): per unit («c/u») and in the amount of its column.</summary>
    public sealed record RackBomPieceItem(string Category, string Description, string Profile, string Length, string Each, string Amount);

    /// <summary>One option of the filter by component of the total.</summary>
    public sealed record RackBomOption(string Key, string Label);

    /// <summary>One component of the BOM of a rack in the inspector: it unfolds to its pieces, several at once (dolor 9).</summary>
    public sealed class RackBomComponentItem : ObservableObject
    {
        private bool _isExpanded;

        public RackBomComponentItem(string category, string description, string length, string quantity, IReadOnlyList<RackBomPieceItem> pieces, int position, bool expanded)
        {
            Category = category;
            Description = description;
            Length = length;
            Quantity = quantity;
            Pieces = pieces ?? Array.Empty<RackBomPieceItem>();
            Position = position;
            _isExpanded = expanded;
        }

        public string Category { get; }

        public string Description { get; }

        public string Length { get; }

        public string Quantity { get; }

        public IReadOnlyList<RackBomPieceItem> Pieces { get; }

        /// <summary>The position of the component in the BOM (1, 2…), for its unique name.</summary>
        public int Position { get; }

        public bool HasPieces => Pieces.Count > 0;

        public string NoPiecesText => HasPieces ? string.Empty : "Sin piezas en el BOM: la cama es un componente cerrado. En el Excel sale solo como «Componente».";

        public string ToggleName => (IsExpanded ? "Plegar " : "Desplegar ") + "componente " + Position.ToString(CultureInfo.InvariantCulture) + ": " + Category + " " + Description;

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded == value) return;
                _isExpanded = value;
                OnPropertyChanged(nameof(IsExpanded));
                OnPropertyChanged(nameof(ToggleName));
                Toggled?.Invoke(this);
            }
        }

        internal event Action<RackBomComponentItem> Toggled;
    }

    /// <summary>One partida of the total of the drawing: where it comes from and its pieces (BM-08).</summary>
    public sealed class RackBomTotalItem : ObservableObject
    {
        private bool _isExpanded;

        public RackBomTotalItem(string key, string category, string description, bool joinsDescriptions, string joinNote, string length, string quantity,
            string racks, string from, IReadOnlyList<RackBomPieceItem> pieces, int position, bool expanded)
        {
            Key = key;
            Category = category;
            Description = description;
            JoinsDescriptions = joinsDescriptions;
            JoinNote = joinNote;
            Length = length;
            Quantity = quantity;
            Racks = racks;
            From = from;
            Pieces = pieces ?? Array.Empty<RackBomPieceItem>();
            Position = position;
            _isExpanded = expanded;
        }

        internal string Key { get; }

        public string Category { get; }

        public string Description { get; }

        /// <summary>The merge joined components with different descriptions (finding of I-68): «varias descripciones».</summary>
        public bool JoinsDescriptions { get; }

        public string JoinNote { get; }

        public string Length { get; }

        public string Quantity { get; }

        /// <summary>The racks it comes from (the column «Racks»).</summary>
        public string Racks { get; }

        /// <summary>«Sale de: …», rack by rack with its copies.</summary>
        public string From { get; }

        public IReadOnlyList<RackBomPieceItem> Pieces { get; }

        public int Position { get; }

        public bool HasPieces => Pieces.Count > 0;

        public string NoPiecesText => HasPieces ? string.Empty : "Sin piezas en el BOM: la cama es un componente cerrado.";

        public string ToggleName => (IsExpanded ? "Plegar " : "Desplegar ") + "partida " + Position.ToString(CultureInfo.InvariantCulture) + ": " + Category + " " + Description;

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded == value) return;
                _isExpanded = value;
                OnPropertyChanged(nameof(IsExpanded));
                OnPropertyChanged(nameof(ToggleName));
                Toggled?.Invoke(this);
            }
        }

        internal event Action<RackBomTotalItem> Toggled;
    }

    /// <summary>
    /// The page «BOM y lista de racks» of the RackCad window (I-64 A-14, line D, T4 bom; prototype «bom», BM-01 to BM-11): ONE page, with no
    /// modal window and without blocking AutoCAD, for what three modal windows did (RACKLISTA, RACKBOMTOTAL and the BOM of a rack).
    ///
    /// <para>
    /// The drawing is read by AutoCAD (RACKBOMLEER, a command with no undo marker, with its number and «Dejar de esperar»): the first time the
    /// page is shown for a drawing, on its own, and then only with «Calcular de nuevo»; after any command the page says it may be out of date,
    /// as the list of the side column (BM-01). The total and the files of the drawing count only what is written; the BOM of a rack with a
    /// draft in an editor of the window can be seen «Borrador del editor», computed here without reading the drawing (BM-02, the BOM of the
    /// draft of the classic Selectivo). The files go next to the drawing with an automatic name, and the notice offers «Abrir», «Mostrar en la
    /// carpeta» and «Guardar como…» (BM-03). What prevents the total is said rack by rack (BM-05). The table follows the side column (BM-09).
    /// </para>
    /// <para>
    /// The readings are kept per drawing (session): coming back to a drawing shows its last reading, said as possibly out of date.
    /// </para>
    /// </summary>
    public sealed class RackBomPageViewModel : ObservableObject
    {
        private static readonly NumberFormatInfo Numbers = new()
        {
            NumberGroupSeparator = ((char)0x00A0).ToString(),
            NumberDecimalSeparator = ".",
            NumberGroupSizes = new[] { 3 },
        };

        private readonly IRackBomPageHost _host;
        private readonly Dictionary<SessionId, DrawingState> _drawings = new();

        private SessionId? _session;
        private (long Id, SessionId Session)? _reading;
        private bool _ownCommandPending;

        private RackBomTab _tab = RackBomTab.Racks;
        private string _selectedRackId;
        private bool _showDraft;
        private string _sortKey = SortKind;
        private int _sortDirection = 1;
        private string _category = AllCategories;
        private readonly HashSet<string> _expandedComponents = new(StringComparer.Ordinal);
        private readonly HashSet<string> _expandedTotal = new(StringComparer.Ordinal);

        private IReadOnlyList<RackBomRowItem> _rows = Array.Empty<RackBomRowItem>();
        private IReadOnlyList<RackBomComponentItem> _components = Array.Empty<RackBomComponentItem>();
        private IReadOnlyList<RackBomPieceItem> _flatLines = Array.Empty<RackBomPieceItem>();
        private IReadOnlyList<RackBomTotalItem> _totalItems = Array.Empty<RackBomTotalItem>();
        private IReadOnlyList<RackBomOption> _categoryOptions = Array.Empty<RackBomOption>();
        private BillOfMaterials _inspected;
        private bool _inspectedFromDraft;

        private PendingExport _ask;
        private PendingExport _afterRead;
        private Exported _exported;
        private string _message = string.Empty;
        private bool _messageIsError;

        internal const string SortName = "name";
        internal const string SortKind = "kind";
        internal const string SortCopies = "copies";
        internal const string SortPieces = "pieces";
        private const string AllCategories = "";

        private sealed class DrawingState
        {
            public RackBomReading Reading;
            public DateTime ReadAt;
            public bool Stale;
            public DateTime StaleAt;
            public StaleCause Cause;
        }

        // Why the reading may be out of date: a command ran after it, the user came back to the drawing, or a read was not completed.
        private enum StaleCause
        {
            Command,
            Return,
            Stopped,
        }

        // An export the page holds: of the drawing (no rack) or of one rack, of the drawing or of the draft of its editor.
        private sealed record PendingExport(bool Excel, string RackId, bool Draft);

        // A file the page saved, with its bytes for «Guardar como…».
        private sealed record Exported(string Path, string FileName, string Detail, byte[] Content, string Extension, bool Failed);

        internal RackBomPageViewModel(IRackBomPageHost host)
        {
            _host = host ?? throw new ArgumentNullException(nameof(host));
        }

        /// <summary>What the page does with the files; the files of the machine by default. Tests give their own.</summary>
        internal IRackBomFiles Files { get; set; } = new RackBomFileShell();

        /// <summary>The clock of «Leído a las …»; the time of the machine by default.</summary>
        internal Func<DateTime> Clock { get; set; } = () => DateTime.Now;

        private DateTime Now => Clock();

        private DrawingState Current => _session.HasValue && _drawings.TryGetValue(_session.Value, out var state) ? state : null;

        private RackBomReading Reading => Current?.Reading;

        private bool IsReading => _reading.HasValue && _session.HasValue && _reading.Value.Session == _session.Value;

        // ------------------------------------------------------------------ the head of the page

        public string ChipText
        {
            get
            {
                if (!_session.HasValue) return "Sin dibujo";
                if (IsReading) return "Leyendo el dibujo";
                var reading = Reading;
                if (reading == null) return "Sin leer";
                if (!reading.IsRead) return "No se pudo leer";
                if (reading.Refusals.Count > 0) return "Sin total";
                return Current.Stale ? "Puede no estar al día" : "Al día";
            }
        }

        /// <summary>«Limpio», «Modificado», «Escribiendo» or «Error», for the colour of the chip, as the marks of the side column.</summary>
        public string ChipKind
        {
            get
            {
                if (!_session.HasValue) return "Nada";
                if (IsReading) return "Escribiendo";
                var reading = Reading;
                if (reading == null) return "Nada";
                if (!reading.IsRead || reading.Refusals.Count > 0) return "Error";
                return Current.Stale ? "Modificado" : "Limpio";
            }
        }

        public string ReadText
        {
            get
            {
                var state = Current;
                if (state?.Reading == null) return string.Empty;
                var text = "Leído a las " + RackBomClock.Time(state.ReadAt);
                if (state.Stale && state.Cause == StaleCause.Command) text += " · corrió una orden a las " + RackBomClock.Time(state.StaleAt);
                return text;
            }
        }

        public string SubText
        {
            get
            {
                var drawing = Reading?.DrawingName ?? "el dibujo";
                return "Cuenta lo que está escrito en " + drawing + ": los cambios sin escribir de los editores de la ventana no entran. Los dos «Exportar» "
                       + "de aquí son del dibujo completo; los de un rack están en su BOM, a la derecha.";
            }
        }

        public bool CanRecalculate => _host.HasBomDoor && _session.HasValue && !IsReading;

        /// <summary>«Calcular» for a drawing never read here, «Calcular de nuevo» once it has a reading.</summary>
        public string RecalculateLabel => Reading == null ? "Calcular" : "Calcular de nuevo";

        public string RecalculateAutomationName => Reading == null ? "Calcular el BOM del dibujo" : "Calcular de nuevo el BOM del dibujo";

        /// <summary>«Calcular de nuevo» is the main action when the BOM may be out of date, has no total or was never read.</summary>
        public bool RecalculateIsPrimary => CanRecalculate && (Reading == null || !Reading.IsRead || Reading.Refusals.Count > 0 || Current.Stale);

        public bool CanExportDrawing => !IsReading && Reading?.Total != null;

        public bool ExportDrawingIsPrimary => CanExportDrawing && !Current.Stale;

        public string ExportDrawingTip =>
            IsReading ? "Espera a que AutoCAD termine de leer."
            : Reading == null ? "Todavía no se leyó el dibujo."
            : Reading.Total == null ? "No hay total que exportar: corrige lo que dice el aviso."
            : "Del dibujo completo.";

        // ------------------------------------------------------------------ the banners

        public bool ShowReading => IsReading;

        public string ReadingText => IsReading
            ? "Se pidió la lectura n.º " + _reading.Value.Id.ToString(CultureInfo.InvariantCulture) + " con " + RackBomReadingRules.CommandName
              + ", una orden sin marca de Deshacer. AutoCAD sigue disponible; esta página conserva lo que tenía hasta que llegue la respuesta."
            : string.Empty;

        public bool ShowUnread => _session.HasValue && !IsReading && Reading == null;

        public string UnreadText
        {
            get
            {
                if (!ShowUnread) return string.Empty;
                if (!_host.HasBomDoor) return "Aquí no se puede leer el dibujo.";
                return "El dibujo todavía no se leyó para el BOM. Pulsa «Calcular»: AutoCAD lo lee sin bloquearse.";
            }
        }

        public bool HasFailure => Reading != null && !Reading.IsRead;

        public string FailureText => HasFailure ? Reading.Failure : string.Empty;

        public bool HasRefusals => Reading?.Refusals.Count > 0;

        public string RefusalHead
        {
            get
            {
                if (!HasRefusals) return string.Empty;
                var count = Reading.Refusals.Count;
                return (count == 1 ? "1 rack tiene un problema que impide cotizarlo." : count.ToString(CultureInfo.InvariantCulture) + " racks tienen problemas que impiden cotizarlos.")
                       + " Un total al que le falta un rack no puede parecer completo: no hay total ni exportación del dibujo hasta corregirlo.";
            }
        }

        public IReadOnlyList<RackBomNoticeItem> Refusals => Notices(Reading?.Refusals, "impide el total");

        public bool HasSkipped => Reading?.IsRead == true && Reading.Skipped.Count > 0;

        public string SkippedHead
        {
            get
            {
                if (!HasSkipped) return string.Empty;
                var count = Reading.Skipped.Count;
                return count == 1 ? "1 rack queda fuera del total" : count.ToString(CultureInfo.InvariantCulture) + " racks quedan fuera del total";
            }
        }

        public IReadOnlyList<RackBomNoticeItem> Skipped => HasSkipped ? Notices(Reading.Skipped, "fuera del total") : Array.Empty<RackBomNoticeItem>();

        public bool ShowStale => !IsReading && Current?.Reading != null && Current.Stale;

        public string StaleText
        {
            get
            {
                if (!ShowStale) return string.Empty;
                var state = Current;
                var at = RackBomClock.Time(state.ReadAt);
                switch (state.Cause)
                {
                    case StaleCause.Return:
                        return "Volviste a este dibujo: el BOM es de las " + at + " y desde entonces el dibujo pudo cambiar. Se vuelve a leer solo cuando lo pides, para no bloquear AutoCAD.";
                    case StaleCause.Stopped:
                        return "El BOM es de las " + at + " y una lectura posterior no se completó: el dibujo pudo cambiar entretanto. Se vuelve a leer solo cuando lo pides.";
                    default:
                        return "El BOM es de las " + at + " y a las " + RackBomClock.Time(state.StaleAt)
                               + " corrió una orden en el dibujo. Se vuelve a leer solo cuando lo pides, para no bloquear AutoCAD con cada orden.";
                }
            }
        }

        public bool HasAsk => _ask != null;

        public string AskTitle => _ask == null ? string.Empty : "¿Exportar el BOM de las " + RackBomClock.Time(Current?.ReadAt ?? Now) + "?";

        public string AskText => _ask == null ? string.Empty
            : "Desde esa lectura corrió una orden en el dibujo. Puedes leerlo otra vez antes de exportar o exportar lo que ya se leyó.";

        public string AskExportLabel => _ask == null ? string.Empty : "Exportar el de las " + RackBomClock.Time(Current?.ReadAt ?? Now);

        public bool HasExported => _exported != null && _ask == null;

        public bool ExportedFailed => _exported?.Failed == true;

        public string ExportedTitle => _exported == null ? string.Empty
            : _exported.Failed ? "No se pudo guardar «" + _exported.FileName + "»" : "Se guardó «" + _exported.FileName + "»";

        public string ExportedDetail => _exported?.Detail ?? string.Empty;

        public bool CanOpenExported => _exported != null && !_exported.Failed;

        public bool HasDrafts => _session.HasValue && Reading != null && DraftRacks().Count > 0;

        public string DraftsText
        {
            get
            {
                var drafts = HasDrafts ? DraftRacks() : new List<(string RackId, string Name)>();
                if (drafts.Count == 0) return string.Empty;
                var names = drafts.Select(d => d.Name).ToList();
                return (names.Count == 1 ? names[0] + " tiene" : string.Join(", ", names) + " tienen")
                       + " cambios sin escribir en el editor de la ventana: aquí cuenta lo escrito en el dibujo. Su borrador se ve en su BOM con «Borrador del editor».";
            }
        }

        /// <summary>What the last gesture of the page said (the navigation, a refusal); empty when there is nothing to say.</summary>
        public string Message => _message;

        public bool MessageIsError => _messageIsError;

        public bool HasMessage => _message.Length > 0;

        // ------------------------------------------------------------------ the figures of the drawing

        public string StatRacks => Reading?.Total == null ? "—" : N(Reading.Total.RackCount);

        public string StatRacksHint => Reading?.IsRead == true ? "de " + N(Reading.Racks.Count) + " en el dibujo" : string.Empty;

        public string StatCopies => Reading?.Total == null ? "—" : N(Reading.Total.TotalCopies);

        public string StatComponents => Reading?.Total == null ? "—" : N(Reading.Total.GrandTotal.TotalComponents);

        public string StatPieces => Reading?.Total == null ? "—" : N(Reading.Total.GrandTotal.TotalPieces);

        public bool ShowContent => Reading?.IsRead == true;

        // ------------------------------------------------------------------ the tabs

        public bool IsRacksTab => _tab == RackBomTab.Racks;

        public bool IsTotalTab => _tab == RackBomTab.Total;

        public string RacksTabLabel => "Racks del dibujo (" + N(Reading?.Racks.Count ?? 0) + ")";

        public void ShowRacksTab() => SetTab(RackBomTab.Racks);

        public void ShowTotalTab() => SetTab(RackBomTab.Total);

        // ------------------------------------------------------------------ «Racks del dibujo»

        public IReadOnlyList<RackBomRowItem> Rows => _rows;

        public string ShowText
        {
            get
            {
                var listed = Reading?.Racks.Count ?? 0;
                if (listed == 0) return Reading?.IsRead == true ? "No hay racks colocados en el dibujo." : string.Empty;
                return _rows.Count == listed
                    ? "Los " + N(listed) + " racks del dibujo."
                    : "Mostrando " + N(_rows.Count) + " de " + N(listed) + ", con el filtro de la columna lateral. El total sigue siendo el del dibujo completo.";
            }
        }

        public string NoRowsText => (Reading?.Racks.Count ?? 0) > 0 && _rows.Count == 0 ? "Ningún rack coincide con el filtro de la columna." : string.Empty;

        public string NameHeader => "Rack y vistas" + Arrow(SortName);

        public string KindHeader => "Sistema" + Arrow(SortKind);

        public string CopiesHeader => "Copias" + Arrow(SortCopies);

        public string PiecesHeader => "Piezas" + Arrow(SortPieces);

        /// <summary>The rack chosen in the table, by its id; the inspector shows its BOM.</summary>
        public string SelectedRackId
        {
            get => _selectedRackId;
            set => Select(value);
        }

        public void Select(string rackId)
        {
            if (string.Equals(_selectedRackId, rackId, StringComparison.OrdinalIgnoreCase)) return;
            _selectedRackId = string.IsNullOrWhiteSpace(rackId) ? null : rackId;
            _showDraft = false;
            OnPropertyChanged(nameof(SelectedRackId));
            RebuildInspector();
        }

        /// <summary>A click on a header: that column orders the table; a second click turns the order.</summary>
        public void SortBy(string key)
        {
            if (key != SortName && key != SortKind && key != SortCopies && key != SortPieces) return;
            if (_sortKey == key) _sortDirection = -_sortDirection;
            else
            {
                _sortKey = key;
                _sortDirection = key == SortCopies || key == SortPieces ? -1 : 1;
            }

            RebuildRows();
            OnPropertyChanged(nameof(NameHeader));
            OnPropertyChanged(nameof(KindHeader));
            OnPropertyChanged(nameof(CopiesHeader));
            OnPropertyChanged(nameof(PiecesHeader));
        }

        // ------------------------------------------------------------------ the inspector: the BOM of the rack

        private RackBomRack SelectedRack => Reading?.Racks.FirstOrDefault(r => string.Equals(r.RackId, _selectedRackId, StringComparison.OrdinalIgnoreCase));

        public bool HasSelection => SelectedRack != null;

        public string NoSelectionText => HasSelection || !ShowContent ? string.Empty : "Elige un rack de la tabla para ver su BOM.";

        public string InspectorTitle => SelectedRack == null ? string.Empty : "BOM de " + SelectedRack.Name;

        public string InspectorTag => SelectedRack == null ? string.Empty : TagOf(SelectedRack).Tag;

        public string InspectorTagKind => SelectedRack == null ? string.Empty : TagOf(SelectedRack).Kind;

        public string InspectorSub
        {
            get
            {
                var rack = SelectedRack;
                if (rack == null) return string.Empty;
                return rack.KindLabel + (string.IsNullOrWhiteSpace(rack.Views) ? string.Empty : " · " + rack.Views) + " · " + CopiesText(rack.Copies);
            }
        }

        public bool HasNextCopy => SelectedRack?.Copies > 1;

        /// <summary>The rack has a draft with changes not written in an editor of the window: «Escrito en el dibujo» or «Borrador del editor».</summary>
        public bool HasDraftChoice => SelectedRack != null && _session.HasValue && SafeHasDraft(_selectedRackId);

        public bool ShowsDraft
        {
            get => _showDraft && HasDraftChoice;
            set
            {
                var next = value && HasDraftChoice;
                if (_showDraft == next) return;
                _showDraft = next;
                OnPropertyChanged(nameof(ShowsDraft));
                OnPropertyChanged(nameof(ShowsDrawing));
                RebuildInspector();
            }
        }

        public bool ShowsDrawing => !ShowsDraft;

        public void ShowDrawingBom() => ShowsDraft = false;

        public void ShowDraftBom() => ShowsDraft = true;

        public string SourceNote => !HasDraftChoice ? string.Empty
            : ShowsDraft
                ? "Calculado en la ventana con el borrador del editor, sin leer el dibujo. No entra en el total."
                : "Lo escrito en el dibujo, como el total. El editor de la ventana tiene cambios sin escribir.";

        public string BlockedNote
        {
            get
            {
                var rack = SelectedRack;
                if (rack == null || ShowsDraft || !rack.BlocksTheTotal) return string.Empty;
                return "No se puede cotizar: " + rack.Reason + " Ábrelo, corrígelo y pulsa «Calcular de nuevo».";
            }
        }

        public string OutNote
        {
            get
            {
                var rack = SelectedRack;
                if (rack == null || ShowsDraft || !rack.IsOutOfTheTotal) return string.Empty;
                return rack.State == RackBomRackState.NoSingleDesign
                    ? "Queda fuera del total: " + rack.Reason + " No se muestra su BOM porque no se sabe cuál de sus vistas vale: ábrelo con RACKEDITAR desde la vista que quieras conservar."
                    : "Queda fuera del total: " + rack.Reason;
            }
        }

        public bool HasRackBom => _inspected != null;

        public bool IsTree => _inspected?.IsComponentBased == true;

        public bool IsFlat => _inspected != null && !_inspected.IsComponentBased;

        public string RackSummary
        {
            get
            {
                var rack = SelectedRack;
                if (rack == null || _inspected == null) return string.Empty;
                var pieces = _inspected.TotalPieces;
                var first = _inspected.IsComponentBased
                    ? N(_inspected.TotalComponents) + " componentes y " + N(pieces) + " piezas por copia"
                    : N(pieces) + " piezas por copia";
                if (ShowsDraft) return first + " · del borrador: no entra en el total.";
                var copies = Math.Max(1, rack.Copies);
                return first + (copies > 1 ? " · × " + N(copies) + " copias = " + N((long)pieces * copies) + " piezas en el dibujo." : " · 1 copia en el dibujo.");
            }
        }

        public string FlatNote => IsFlat ? "Pieza suelta: su BOM son sus piezas. En el total entra como un componente «" + SelectedRack?.KindLabel + "» por copia." : string.Empty;

        public IReadOnlyList<RackBomComponentItem> Components => _components;

        public IReadOnlyList<RackBomPieceItem> FlatLines => _flatLines;

        public bool CanExportRack => !IsReading && _inspected != null && (ShowsDraft || SelectedRack?.IsQuoted == true);

        public string ExportRackHint
        {
            get
            {
                var rack = SelectedRack;
                if (rack == null || _inspected == null) return string.Empty;
                return "Solo este rack" + (ShowsDraft ? ", del borrador" : string.Empty) + ": Excel de una hoja, como hoy (D-2 de I-68). El archivo va junto al dibujo con su nombre: «"
                       + RackBomFiles.RackFileName(Reading?.DrawingName, rack, Reading?.Racks, ShowsDraft, RackBomFiles.Excel) + "».";
            }
        }

        public void ExpandAll() => SetExpanded(true);

        public void CollapseAll() => SetExpanded(false);

        // ------------------------------------------------------------------ «BOM total»

        public bool HasTotal => Reading?.Total != null;

        public string NoTotalText
        {
            get
            {
                var reading = Reading;
                if (reading == null || !reading.IsRead || reading.Total != null) return string.Empty;
                if (reading.Refusals.Count > 0)
                    return "No hay total: corrige lo que dice el aviso de arriba y pulsa «Calcular de nuevo». El BOM de cada rack que sí se puede cotizar sigue en «Racks del dibujo».";
                return reading.Racks.Count == 0 ? "No hay racks colocados en el dibujo." : "No se pudo interpretar ningún rack del dibujo: no hay total.";
            }
        }

        public IReadOnlyList<RackBomOption> CategoryOptions => _categoryOptions;

        public string SelectedCategory
        {
            get => _category;
            set
            {
                var next = value ?? AllCategories;
                if (string.Equals(_category, next, StringComparison.Ordinal)) return;
                _category = next;
                OnPropertyChanged(nameof(SelectedCategory));
                RebuildTotal();
            }
        }

        public IReadOnlyList<RackBomTotalItem> TotalItems => _totalItems;

        public string TotalShownText
        {
            get
            {
                var all = Reading?.Total?.GrandTotal.Components.Count ?? 0;
                if (all == 0) return string.Empty;
                return _category.Length == 0 ? N(all) + " partidas" : N(_totalItems.Count) + " de " + N(all) + " partidas";
            }
        }

        public string TotalFooterText
        {
            get
            {
                var total = Reading?.Total;
                if (total == null) return string.Empty;
                return N(total.GrandTotal.TotalPieces) + " piezas en " + N(total.TotalCopies) + " copias de " + N(total.RackCount) + " racks";
            }
        }

        public string TotalFooterQuantity => Reading?.Total == null ? string.Empty : N(Reading.Total.GrandTotal.TotalComponents);

        public void ExpandAllTotal() => SetExpandedTotal(true);

        public void CollapseAllTotal() => SetExpandedTotal(false);

        // ------------------------------------------------------------------ the gestures

        /// <summary>«Calcular de nuevo»: asks AutoCAD to read the drawing (RACKBOMLEER).</summary>
        public void Recalculate()
        {
            _ask = null;
            _exported = null;
            _afterRead = null; // a plain «Calcular» never exports (correction 2 of the review)
            TryRead(userAsked: true);
            RaiseAll();
        }

        /// <summary>«Dejar de esperar».</summary>
        public void StopWaiting() => _host.StopWaiting();

        /// <summary>«Exportar Excel» / «Exportar CSV» of the drawing: the files of the whole drawing (BM-03, BM-04).</summary>
        public void ExportDrawing(bool excel)
        {
            if (!CanExportDrawing) return;
            var export = new PendingExport(excel, null, false);
            if (Current.Stale)
            {
                _ask = export;
                _exported = null;
                RaiseAll();
                return;
            }

            DoExport(export);
        }

        /// <summary>«Exportar Excel» / «Exportar CSV» of the rack of the inspector: what is seen, the drawing or the draft (BM-02).</summary>
        public void ExportRack(bool excel)
        {
            if (!CanExportRack) return;
            var export = new PendingExport(excel, SelectedRack.RackId, ShowsDraft);
            if (!export.Draft && Current.Stale)
            {
                _ask = export;
                _exported = null;
                RaiseAll();
                return;
            }

            DoExport(export);
        }

        /// <summary>«Calcular y exportar»: the file is made with the reading that answers this new request.</summary>
        public void AskRecalculateAndExport()
        {
            if (_ask == null) return;
            // The export waits BEFORE the read is handed over: AutoCAD may answer during the call (M-22), and then the reading exports at once.
            // A read that is not handed over leaves nothing armed (correction 2 of the review).
            _afterRead = _ask;
            _ask = null;
            if (!TryRead(userAsked: true)) _afterRead = null;
            RaiseAll();
        }

        /// <summary>«Exportar el de las HH:MM»: the file is made with what was read.</summary>
        public void AskExportAnyway()
        {
            if (_ask == null) return;
            var export = _ask;
            _ask = null;
            DoExport(export);
        }

        public void AskCancel()
        {
            _ask = null;
            RaiseAll();
        }

        public void OpenExported() => Contained(() =>
        {
            if (CanOpenExported) Files.Open(_exported.Path);
        });

        public void ShowExportedInFolder() => Contained(() =>
        {
            if (CanOpenExported) Files.ShowInFolder(_exported.Path);
        });

        /// <summary>«Guardar como…»: the dialog of the system, the only one of the page, and only when asked; the same file elsewhere.</summary>
        public void SaveExportedAs() => Contained(() =>
        {
            var exported = _exported;
            if (exported == null) return;
            var chosen = Files.SaveAs(exported.FileName, exported.Extension, FolderOf(exported.Path));
            if (string.IsNullOrWhiteSpace(chosen)) return;
            try
            {
                Files.Write(chosen, exported.Content);
                _exported = exported with { Path = chosen, FileName = System.IO.Path.GetFileName(chosen), Failed = false, Detail = "En la carpeta que elegiste. " + exported.Detail };
            }
            catch (Exception ex)
            {
                _exported = exported with { Path = chosen, FileName = System.IO.Path.GetFileName(chosen), Failed = true, Detail = "No se pudo escribir: " + ex.Message };
            }

            RaiseAll();
        });

        public void CloseExported()
        {
            _exported = null;
            RaiseAll();
        }

        /// <summary>«Abrir» of a rack of a warning or of the inspector: the page of the rack.</summary>
        public void OpenRack(string rackId)
        {
            if (!string.IsNullOrWhiteSpace(rackId)) _host.OpenRack(rackId);
        }

        public void OpenSelected() => OpenRack(SelectedRack?.RackId);

        /// <summary>«Ver en el dibujo»: the first copy of the rack chosen, with the navigation of its page; this page stays.</summary>
        public void ViewSelected()
        {
            if (SelectedRack != null) _host.ViewInDrawing(SelectedRack.RackId, nextCopy: false);
        }

        /// <summary>«Siguiente copia»: the next copy, as many times as wanted.</summary>
        public void NextCopySelected()
        {
            if (SelectedRack != null) _host.ViewInDrawing(SelectedRack.RackId, nextCopy: true);
        }

        /// <summary>«Ver su BOM» of the notice of the drafts: the first rack with changes not written.</summary>
        public void ShowFirstDraft()
        {
            var first = HasDrafts ? DraftRacks().FirstOrDefault() : default;
            if (first.RackId == null) return;
            ShowRack(first.RackId);
        }

        internal void Say(string text, bool error)
        {
            _message = text ?? string.Empty;
            _messageIsError = error && _message.Length > 0;
            OnPropertyChanged(nameof(Message));
            OnPropertyChanged(nameof(MessageIsError));
            OnPropertyChanged(nameof(HasMessage));
        }

        public void DismissMessage() => Say(string.Empty, false);

        // ------------------------------------------------------------------ what the window tells it

        /// <summary>
        /// The window presented a snapshot: the page follows its drawing, says the BOM may be out of date after a command that is not its own
        /// read (BM-01) and forgets the drawings that were closed. It never sends a read: only a gesture of the user does (correction 4).
        /// </summary>
        internal void Present(SessionId? session, WorkspaceActionBlock block, bool bomInFlight, Func<SessionId, bool> isAlive)
        {
            if (isAlive != null)
            {
                foreach (var dead in _drawings.Keys.Where(s => !SafeAlive(isAlive, s)).ToList()) _drawings.Remove(dead);
            }

            if (session != _session)
            {
                var previous = _session;
                _session = session;
                _ask = null;
                _afterRead = null;
                _exported = null;
                Say(string.Empty, false);
                if (previous.HasValue && Current?.Reading != null && !Current.Stale) MarkStale(Current, StaleCause.Return);

                // A drawing never read stays «Sin leer» with «Calcular»: changing drawing is no gesture, so nothing is sent (correction 4).
                RebuildData();
            }

            // Any command that ran in the drawing after the reading (a write of the window too) may have changed it. The read itself does not
            // count, and only it: RACKBOMLEER from the moment it says it took the request (Started) — or RACKLISTA / RACKBOMTOTAL, which read
            // inside themselves — until the drawing is at rest again after its answer (correction 1 of the review).
            var commandRuns = block == WorkspaceActionBlock.CommandActive || block == WorkspaceActionBlock.ModalActive;
            if (commandRuns)
            {
                var state = Current;
                if (!_ownCommandPending && state?.Reading != null && !state.Stale)
                {
                    MarkStale(state, StaleCause.Command);
                    RaiseAll();
                }
            }
            else if (!bomInFlight)
            {
                _ownCommandPending = false;
            }
        }

        private void MarkStale(DrawingState state, StaleCause cause)
        {
            state.Stale = true;
            state.StaleAt = Now;
            state.Cause = cause;
        }

        /// <summary>
        /// The page was shown by a gesture of the user (a section, «Ver su BOM»): the first time for a drawing, the drawing is read (BM-01).
        /// RACKLISTA and RACKBOMTOTAL deliver their reading before showing it, so they send nothing.
        /// </summary>
        internal void Shown()
        {
            RebuildData(); // the BOM of a draft follows what was edited meanwhile in its editor
            if (_session.HasValue && Reading == null && !IsReading) TryRead(userAsked: false);
            RaiseAll();
        }

        /// <summary>RACKLISTA or RACKBOMTOTAL ran (BM-11): their tab (their reading is delivered by the window).</summary>
        internal void Asked(RackBomTab tab)
        {
            SetTab(tab);
            RaiseAll();
        }

        /// <summary>«Ver su BOM» of the page of a rack (BM-10): the tab of the racks with that rack chosen, as written in the drawing.</summary>
        internal void ShowRack(string rackId)
        {
            SetTab(RackBomTab.Racks);
            _selectedRackId = rackId;
            _showDraft = false;
            OnPropertyChanged(nameof(SelectedRackId));
            RebuildInspector();
        }

        internal void ReadingStarted(long requestId, SessionId session)
        {
            _reading = (requestId, session);
            RaiseAll();
        }

        /// <summary>RACKBOMLEER took the request (Started): from now until the drawing is at rest after its answer, a command is the read itself.</summary>
        internal void ReadingTaken(long requestId)
        {
            if (_reading.HasValue && _reading.Value.Id == requestId) _ownCommandPending = true;
        }

        /// <summary>
        /// The request was not handed over, the window stopped waiting, or its answer was not for this drawing: the export that waited is
        /// dropped, nothing counts as the read any more, and the reading the drawing had may be out of date (correction 1 of the review).
        /// </summary>
        internal void ReadingStopped(bool abandoned, string message = null)
        {
            var request = _reading;
            _reading = null;
            _afterRead = null;
            _ownCommandPending = false;
            if (request.HasValue && _drawings.TryGetValue(request.Value.Session, out var state) && state.Reading != null && !state.Stale)
                MarkStale(state, StaleCause.Stopped);
            if (message != null) Say(message, true);
            else if (abandoned && request.HasValue)
                Say("Se dejó de esperar la lectura n.º " + request.Value.Id.ToString(CultureInfo.InvariantCulture) + " del BOM: si la orden llega a correr, no cambiará nada.", false);
            RebuildData();
        }

        /// <summary>
        /// The answer of AutoCAD to the read of that session, or (<paramref name="readByRunningCommand"/>) the reading RACKLISTA or RACKBOMTOTAL
        /// made inside themselves: that command still runs, and until the drawing is at rest it is the read, not a change.
        /// </summary>
        internal void Deliver(SessionId session, RackBomReading reading, bool readByRunningCommand = false)
        {
            if (reading == null) return;
            if (readByRunningCommand) _ownCommandPending = true;
            else _reading = null;
            if (!_drawings.TryGetValue(session, out var state))
            {
                state = new DrawingState();
                _drawings[session] = state;
            }

            state.Reading = reading;
            state.ReadAt = Now;
            state.Stale = false;
            state.Cause = StaleCause.Command;
            var after = readByRunningCommand ? null : _afterRead;
            if (!readByRunningCommand) _afterRead = null;
            if (session != _session)
            {
                RaiseAll();
                return;
            }

            RebuildData();
            if (after == null) return;

            if (after.RackId == null)
            {
                if (reading.Total != null) DoExport(after);
                else Say("No se exportó: la lectura nueva no tiene total del dibujo (lo dice el aviso).", true);
                return;
            }

            var rack = reading.Racks.FirstOrDefault(r => string.Equals(r.RackId, after.RackId, StringComparison.OrdinalIgnoreCase));
            if (rack?.IsQuoted == true)
            {
                _selectedRackId = rack.RackId;
                RebuildInspector();
                DoExport(after);
            }
            else
            {
                Say("No se exportó: en la lectura nueva el rack no tiene BOM que cotizar.", true);
            }
        }

        /// <summary>The side column, the drafts or the editors changed: the table, the inspector and the notices follow.</summary>
        internal void Refresh()
        {
            RebuildRows();

            // The BOM of a draft is computed again only when what the inspector shows changes source (a draft written or discarded).
            if (_inspectedFromDraft != ShowsDraft || (_inspected == null) != (SelectedRack == null)) RebuildInspector();
            else RaiseInspector();
            RaiseAll();
        }

        // ------------------------------------------------------------------ internals

        private DrawingState StateOfCurrent()
        {
            if (!_session.HasValue) return null;
            if (!_drawings.TryGetValue(_session.Value, out var state))
            {
                state = new DrawingState();
                _drawings[_session.Value] = state;
            }

            return state;
        }

        // Hands the read over, after a gesture of the user; Busy (AutoCAD or another request of the window) is said, and nothing is asked again
        // on its own.
        private bool TryRead(bool userAsked)
        {
            var state = StateOfCurrent();
            if (state == null || IsReading) return false;
            if (!_host.HasBomDoor)
            {
                if (userAsked) Say("Aquí no se puede leer el dibujo.", true);
                return false;
            }

            RackCadClassicResult result;
            try
            {
                result = _host.RequestBomRead() ?? RackCadClassicResult.Without(RackCadClassicStatus.Failed, "sin respuesta.");
            }
            catch (Exception ex)
            {
                result = RackCadClassicResult.Without(RackCadClassicStatus.Failed, ex.Message);
            }

            if (result.IsStarted)
            {
                if (userAsked) Say(string.Empty, false);
                return true;
            }

            switch (result.Status)
            {
                case RackCadClassicStatus.Busy:
                    // Nothing is asked again on its own (correction 4): the user asks again when AutoCAD is free.
                    Say("AutoCAD está ocupado (" + (string.IsNullOrWhiteSpace(result.Reason) ? "hay un comando en curso" : result.Reason.Trim().TrimEnd('.'))
                        + "): termina o cancela la orden en curso y pulsa «" + RecalculateLabel + "».", false);
                    break;
                case RackCadClassicStatus.NoDocument:
                    Say("Abre o crea un dibujo para leer su BOM.", true);
                    break;
                default:
                    Say("No se pudo leer el dibujo: " + (string.IsNullOrWhiteSpace(result.Reason) ? "sin detalle." : result.Reason), true);
                    break;
            }

            RaiseAll();
            return false;
        }

        private void DoExport(PendingExport export)
        {
            _ask = null;
            var reading = Reading;
            if (reading == null)
            {
                RaiseAll();
                return;
            }

            var extension = export.Excel ? RackBomFiles.Excel : RackBomFiles.Csv;
            string fileName;
            byte[] content;
            string detail;
            var at = RackBomClock.Time(Current.ReadAt);
            if (export.RackId == null)
            {
                if (reading.Total == null)
                {
                    RaiseAll();
                    return;
                }

                fileName = RackBomFiles.DrawingFileName(reading.DrawingName, extension);
                content = export.Excel ? ConsolidatedBomXlsxExporter.ToXlsx(reading.Total) : Text(ConsolidatedBomCsvExporter.ToCsv(reading.Total));
                detail = export.Excel
                    ? "Tres hojas: «Por rack» (tabla RackCadBOM, con «Cantidad total»), «Total del dibujo» y «Resumen» (tabla dinámica ResumenBOM). Del BOM de las " + at + "."
                    : "Un archivo con cada rack (cantidades de una copia y «Cantidad_total») y las filas «TOTAL DEL DIBUJO». Del BOM de las " + at + ".";
            }
            else
            {
                var rack = reading.Racks.FirstOrDefault(r => string.Equals(r.RackId, export.RackId, StringComparison.OrdinalIgnoreCase));
                var bom = export.Draft ? _host.DraftBomOf(_session.Value, export.RackId) : rack?.Bom;
                if (rack == null || bom == null)
                {
                    Say("No se exportó: el rack ya no tiene ese BOM.", true);
                    RaiseAll();
                    return;
                }

                fileName = RackBomFiles.RackFileName(reading.DrawingName, rack, reading.Racks, export.Draft, extension);
                content = export.Excel ? BomXlsxExporter.ToXlsx(bom) : Text(BomCsvExporter.ToCsv(bom));
                detail = (export.Excel ? "Una hoja «Lista de materiales», como hoy (D-2 de I-68)" : "Componentes y piezas, con la cantidad de una copia del rack")
                         + (export.Draft ? ", del borrador del editor (no está escrito en el dibujo)." : ". Del BOM de las " + at + ".");
            }

            var folder = RackBomFiles.FolderOf(reading.DrawingPath);
            var where = folder == null ? "En Documentos, porque el dibujo no está guardado todavía. " : "Junto al dibujo. ";
            var shared = folder == null;
            if (folder == null) folder = SafeFallback();
            var path = System.IO.Path.Combine(folder ?? string.Empty, fileName);

            // Next to a saved drawing, a file of the same name is the earlier export of the same BOM and is replaced. In the documents of the
            // user (a drawing never saved: every one is «Drawing1»), it may be someone else's: it is never replaced (correction 3, BM-S5).
            if (shared)
            {
                var baseName = fileName;
                var number = 1;
                while (SafeExists(path) && number < 1000)
                {
                    number++;
                    fileName = RackBomFiles.WithNumber(baseName, number);
                    path = System.IO.Path.Combine(folder ?? string.Empty, fileName);
                }

                if (number > 1) where += "Ya había un archivo con ese nombre: este se guardó como «" + fileName + "». ";
            }

            try
            {
                Files.Write(path, content);
                _exported = new Exported(path, fileName, where + detail, content, extension, false);
            }
            catch (Exception ex)
            {
                _exported = new Exported(path, fileName,
                    "No se pudo escribir en " + (folder ?? "la carpeta") + ": " + ex.Message + " Si el archivo está abierto en Excel, ciérralo, o usa «Guardar como…».",
                    content, extension, true);
            }

            RaiseAll();
        }

        private bool SafeExists(string path)
        {
            try
            {
                return Files.Exists(path);
            }
            catch (Exception)
            {
                return true; // unknown is taken: never replace what may be there
            }
        }

        private string SafeFallback()
        {
            try
            {
                return Files.FallbackFolder;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static string FolderOf(string path)
        {
            try
            {
                return System.IO.Path.GetDirectoryName(path);
            }
            catch (Exception)
            {
                return null;
            }
        }

        // The CSV as the classic windows write it (File.WriteAllText: UTF-8 without a mark).
        private static byte[] Text(string text) => new UTF8Encoding(false).GetBytes(text ?? string.Empty);

        private List<(string RackId, string Name)> DraftRacks()
        {
            if (!_session.HasValue || Reading == null) return new List<(string, string)>();
            var listed = new HashSet<string>(Reading.Racks.Select(r => r.RackId), StringComparer.OrdinalIgnoreCase);
            return (_host.UnwrittenDraftsOf(_session.Value) ?? Array.Empty<(string, string)>()).Where(d => listed.Contains(d.RackId)).ToList();
        }

        private bool SafeHasDraft(string rackId)
        {
            try
            {
                return _host.HasUnwrittenDraft(_session.Value, rackId);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private BillOfMaterials DraftOfSelected()
        {
            if (!_session.HasValue || _selectedRackId == null) return null;
            try
            {
                return _host.DraftBomOf(_session.Value, _selectedRackId);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private void SetTab(RackBomTab tab)
        {
            if (_tab == tab) return;
            _tab = tab;
            OnPropertyChanged(nameof(IsRacksTab));
            OnPropertyChanged(nameof(IsTotalTab));
        }

        private void RebuildData()
        {
            RebuildRows();
            RebuildInspector();
            RebuildTotal();
            RaiseAll();
        }

        private void RebuildRows()
        {
            var reading = Reading;
            var rows = new List<RackBomRowItem>();
            if (reading?.IsRead == true && _session.HasValue)
            {
                var shown = reading.Racks.Where(r => SafeFilter(r)).ToList();
                shown.Sort(Compare);
                var position = 0;
                foreach (var rack in shown)
                {
                    position++;
                    var (tag, tagKind) = TagOf(rack);
                    var state = SafeState(rack.RackId);
                    var quoted = rack.IsQuoted;
                    var pieces = quoted ? rack.Bom.TotalPieces : 0;
                    var components = !quoted ? "—" : rack.Bom.IsComponentBased ? N(rack.Bom.TotalComponents) : "pieza suelta";
                    rows.Add(new RackBomRowItem(
                        rack.RackId,
                        rack.Name,
                        rack.Views,
                        rack.KindLabel,
                        N(rack.Copies),
                        components,
                        quoted && !rack.Bom.IsComponentBased ? "Una cama o una cabecera sola no tiene componentes: entra al total como un componente por copia." : null,
                        quoted ? N(pieces) : "—",
                        quoted ? N((long)pieces * Math.Max(1, rack.Copies)) : "—",
                        tag,
                        tagKind,
                        rack.BlocksTheTotal ? "Obsoleto" : state.Kind,
                        // The name of the row says its position: two racks may have the same name («(sin nombre)»).
                        "Rack " + position.ToString(CultureInfo.InvariantCulture) + " de " + shown.Count.ToString(CultureInfo.InvariantCulture) + ": "
                        + rack.Name + (tag.Length > 0 ? ", " + tag.ToLowerInvariant() : string.Empty) + ": " + rack.KindLabel + ", " + CopiesText(rack.Copies)
                        + (quoted ? ", " + N(pieces) + " piezas por copia" : string.Empty)));
                }
            }

            _rows = rows;
            OnPropertyChanged(nameof(Rows));
            OnPropertyChanged(nameof(ShowText));
            OnPropertyChanged(nameof(NoRowsText));
        }

        // The order of the side column (the system, then the name in its natural order), or the one of a header.
        private int Compare(RackBomRack a, RackBomRack b)
        {
            int value;
            switch (_sortKey)
            {
                case SortName:
                    value = RackListViewModel.NaturalNameComparer.Instance.Compare(a.Name, b.Name);
                    break;
                case SortCopies:
                    value = a.Copies.CompareTo(b.Copies);
                    break;
                case SortPieces:
                    // A rack with no BOM («—») always goes last.
                    if (a.IsQuoted != b.IsQuoted) return a.IsQuoted ? -1 : 1;
                    value = a.IsQuoted ? a.Bom.TotalPieces.CompareTo(b.Bom.TotalPieces) : 0;
                    break;
                default:
                    value = RackBomReadingRules.KindRank(a.Kind).CompareTo(RackBomReadingRules.KindRank(b.Kind));
                    if (value == 0 && RackBomReadingRules.KindRank(a.Kind) == RackBomReadingRules.KindOrder.Count)
                        value = string.Compare(a.Kind, b.Kind, StringComparison.OrdinalIgnoreCase);
                    break;
            }

            if (value != 0) return value * _sortDirection;
            var byName = RackListViewModel.NaturalNameComparer.Instance.Compare(a.Name, b.Name);
            return byName != 0 ? byName : string.Compare(a.RackId, b.RackId, StringComparison.OrdinalIgnoreCase);
        }

        private void RebuildInspector()
        {
            foreach (var old in _components) old.Toggled -= OnComponentToggled;
            var rack = SelectedRack;
            BillOfMaterials draft = null;
            if (rack != null && _showDraft)
            {
                draft = DraftOfSelected();
                if (draft == null) _showDraft = false;
            }

            _inspectedFromDraft = draft != null;
            _inspected = rack == null ? null : draft ?? (rack.IsQuoted ? rack.Bom : null);

            var components = new List<RackBomComponentItem>();
            var lines = new List<RackBomPieceItem>();
            if (_inspected != null)
            {
                if (_inspected.IsComponentBased)
                {
                    var position = 0;
                    foreach (var component in _inspected.Components)
                    {
                        position++;
                        var key = ComponentKey(rack.RackId, position);
                        var pieces = component.Pieces.Select(p => new RackBomPieceItem(p.Category, p.Description, p.ProfileId, L(p.Length), N(p.Quantity),
                            N((long)p.Quantity * component.Quantity))).ToList();
                        var item = new RackBomComponentItem(component.Category, component.Description, component.Length > 0 ? L(component.Length) + " in" : string.Empty,
                            "× " + N(component.Quantity), pieces, position, _expandedComponents.Contains(key));
                        item.Toggled += OnComponentToggled;
                        components.Add(item);
                    }
                }
                else
                {
                    lines.AddRange(_inspected.Lines.Select(l => new RackBomPieceItem(l.Category, l.Description, l.ProfileId, L(l.Length), N(l.Quantity), N(l.Quantity))));
                }
            }

            _components = components;
            _flatLines = lines;
            RaiseInspector();
        }

        // The unfolded components are kept per rack and per source, so changing «Escrito en el dibujo» / «Borrador del editor» keeps each.
        private string ComponentKey(string rackId, int position) => rackId + (ShowsDraft ? ":b:" : ":d:") + position.ToString(CultureInfo.InvariantCulture);

        private void OnComponentToggled(RackBomComponentItem item)
        {
            var rack = SelectedRack;
            if (rack == null) return;
            var key = ComponentKey(rack.RackId, item.Position);
            if (item.IsExpanded) _expandedComponents.Add(key);
            else _expandedComponents.Remove(key);
        }

        private void SetExpanded(bool expanded)
        {
            foreach (var item in _components) item.IsExpanded = expanded;
        }

        private void RebuildTotal()
        {
            foreach (var old in _totalItems) old.Toggled -= OnTotalToggled;
            var total = Reading?.Total;
            var items = new List<RackBomTotalItem>();
            var options = new List<RackBomOption>();
            if (total != null)
            {
                var components = total.GrandTotal.Components;
                var provenance = ConsolidatedBomBuilder.Provenance(total);
                options.Add(new RackBomOption(AllCategories, "Todos (" + N(components.Count) + " partidas)"));
                foreach (var category in components.Select(c => c.Category ?? string.Empty).Distinct(StringComparer.Ordinal))
                    options.Add(new RackBomOption(category, category.Length == 0 ? "(sin categoría)" : category));
                if (options.All(o => !string.Equals(o.Key, _category, StringComparison.Ordinal))) _category = AllCategories;

                for (var i = 0; i < components.Count; i++)
                {
                    var component = components[i];
                    if (_category.Length > 0 && !string.Equals(component.Category ?? string.Empty, _category, StringComparison.Ordinal)) continue;
                    var origin = i < provenance.Count ? provenance[i] : null;
                    var key = (i + 1).ToString(CultureInfo.InvariantCulture) + "|" + component.Category + "|" + component.Description;
                    var names = origin == null ? new List<string>() : origin.Sources.Select(s => s.RackName).Distinct(StringComparer.Ordinal).ToList();
                    var from = origin == null ? string.Empty : string.Join(" · ", origin.Sources.Select(s =>
                        s.RackName + ": " + N(s.QuantityPerCopy) + (s.Copies > 1 ? " × " + N(s.Copies) + " copias = " + N((long)s.QuantityPerCopy * s.Copies) : string.Empty)));
                    var joins = origin?.JoinsDescriptions == true;
                    var joinNote = joins
                        ? "Esta partida junta descripciones distintas: " + string.Join(" y ", origin.Descriptions.Select(d => "«" + d + "»"))
                          + ". Hoy el total une por categoría, largo y piezas, sin mirar perfil ni descripción, y se queda con la primera (hallazgo de I-68). La suma no cambia."
                        : string.Empty;
                    var pieces = component.Pieces.Select(p => new RackBomPieceItem(p.Category, p.Description, p.ProfileId, L(p.Length), N(p.Quantity),
                        N((long)p.Quantity * component.Quantity))).ToList();
                    var item = new RackBomTotalItem(key, component.Category, component.Description, joins, joinNote, L(component.Length), N(component.Quantity),
                        string.Join(", ", names), "Sale de: " + from, pieces, i + 1, _expandedTotal.Contains(key));
                    item.Toggled += OnTotalToggled;
                    items.Add(item);
                }
            }

            _totalItems = items;
            _categoryOptions = options;
            OnPropertyChanged(nameof(TotalItems));
            OnPropertyChanged(nameof(CategoryOptions));
            OnPropertyChanged(nameof(SelectedCategory));
            OnPropertyChanged(nameof(TotalShownText));
        }

        private void OnTotalToggled(RackBomTotalItem item)
        {
            if (item.IsExpanded) _expandedTotal.Add(item.Key);
            else _expandedTotal.Remove(item.Key);
        }

        private void SetExpandedTotal(bool expanded)
        {
            foreach (var item in _totalItems) item.IsExpanded = expanded;
        }

        private (string Tag, string Kind) TagOf(RackBomRack rack)
        {
            if (rack.BlocksTheTotal) return ("Bloqueado", "Bloqueado");
            if (rack.IsOutOfTheTotal) return ("Fuera del total", "Fuera");
            return SafeState(rack.RackId).Changed ? ("Con cambios", "Cambios") : (string.Empty, string.Empty);
        }

        private (string Kind, bool Changed) SafeState(string rackId)
        {
            if (!_session.HasValue) return ("Limpio", false);
            try
            {
                return _host.StateOf(_session.Value, rackId);
            }
            catch (Exception)
            {
                return ("Limpio", false);
            }
        }

        private bool SafeFilter(RackBomRack rack)
        {
            try
            {
                return _host.PassesSideFilter(rack.RackId, rack.Name, rack.Kind);
            }
            catch (Exception)
            {
                return true;
            }
        }

        private static bool SafeAlive(Func<SessionId, bool> isAlive, SessionId session)
        {
            try
            {
                return isAlive(session);
            }
            catch (Exception)
            {
                return true; // unknown is not dead
            }
        }

        private IReadOnlyList<RackBomNoticeItem> Notices(IReadOnlyList<RackBomNotice> notices, string list)
        {
            if (notices == null || notices.Count == 0) return Array.Empty<RackBomNoticeItem>();
            return notices.Select((n, i) => new RackBomNoticeItem(n.RackId, n.Name, n.Reason, n.RackId != null, n.RackId == null ? string.Empty : "Abrir " + n.Name,
                n.RackId == null ? string.Empty : "Abrir " + n.Name + " (" + list + ", " + (i + 1).ToString(CultureInfo.InvariantCulture) + ")")).ToList();
        }

        private string Arrow(string key) => _sortKey != key ? string.Empty : _sortDirection > 0 ? " ▲" : " ▼";

        private static string CopiesText(int copies) => copies == 1 ? "1 copia" : N(copies) + " copias";

        private static string N(long value) => value.ToString("#,0", Numbers);

        private static string L(double value) => value > 0 ? value.ToString("0.##", CultureInfo.InvariantCulture) : "—";

        private void Contained(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                Say("No se pudo: " + ex.Message, true);
            }
        }

        private void RaiseInspector()
        {
            OnPropertyChanged(nameof(HasSelection));
            OnPropertyChanged(nameof(NoSelectionText));
            OnPropertyChanged(nameof(InspectorTitle));
            OnPropertyChanged(nameof(InspectorTag));
            OnPropertyChanged(nameof(InspectorTagKind));
            OnPropertyChanged(nameof(InspectorSub));
            OnPropertyChanged(nameof(HasNextCopy));
            OnPropertyChanged(nameof(HasDraftChoice));
            OnPropertyChanged(nameof(ShowsDraft));
            OnPropertyChanged(nameof(ShowsDrawing));
            OnPropertyChanged(nameof(SourceNote));
            OnPropertyChanged(nameof(BlockedNote));
            OnPropertyChanged(nameof(OutNote));
            OnPropertyChanged(nameof(HasRackBom));
            OnPropertyChanged(nameof(IsTree));
            OnPropertyChanged(nameof(IsFlat));
            OnPropertyChanged(nameof(RackSummary));
            OnPropertyChanged(nameof(FlatNote));
            OnPropertyChanged(nameof(Components));
            OnPropertyChanged(nameof(FlatLines));
            OnPropertyChanged(nameof(CanExportRack));
            OnPropertyChanged(nameof(ExportRackHint));
        }

        // Everything the head, the banners and the figures show.
        private void RaiseAll()
        {
            foreach (var name in new[]
            {
                nameof(ChipText), nameof(ChipKind), nameof(ReadText), nameof(SubText), nameof(CanRecalculate), nameof(RecalculateIsPrimary),
                nameof(RecalculateLabel), nameof(RecalculateAutomationName),
                nameof(CanExportDrawing), nameof(ExportDrawingIsPrimary), nameof(ExportDrawingTip), nameof(ShowReading), nameof(ReadingText),
                nameof(ShowUnread), nameof(UnreadText), nameof(HasFailure), nameof(FailureText), nameof(HasRefusals), nameof(RefusalHead),
                nameof(Refusals), nameof(HasSkipped), nameof(SkippedHead), nameof(Skipped), nameof(ShowStale), nameof(StaleText), nameof(HasAsk),
                nameof(AskTitle), nameof(AskText), nameof(AskExportLabel), nameof(HasExported), nameof(ExportedFailed), nameof(ExportedTitle),
                nameof(ExportedDetail), nameof(CanOpenExported), nameof(HasDrafts), nameof(DraftsText), nameof(StatRacks), nameof(StatRacksHint),
                nameof(StatCopies), nameof(StatComponents), nameof(StatPieces), nameof(ShowContent), nameof(RacksTabLabel), nameof(HasTotal),
                nameof(NoTotalText), nameof(TotalFooterText), nameof(TotalFooterQuantity), nameof(TotalShownText), nameof(ShowText), nameof(NoRowsText),
                nameof(CanExportRack), nameof(ExportRackHint),
            })
            {
                OnPropertyChanged(name);
            }
        }
    }
}
