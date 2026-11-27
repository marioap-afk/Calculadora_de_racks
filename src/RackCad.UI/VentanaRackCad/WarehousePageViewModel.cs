using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Layout;
using RackCad.Application.VentanaRackCad;
using RackCad.Application.Workspace;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>T5 al: what the page «Almacén» needs of the window: the door (through it), the racks of the side column, the drafts and the settings.</summary>
    internal interface IWarehousePageHost
    {
        /// <summary>The window has the door of the page (RACKALMACENLEER, RACKALMACENCOLOCAR).</summary>
        bool HasWarehouseDoor { get; }

        /// <summary>Another request of the window waits for AutoCAD (one at a time for the whole window).</summary>
        bool OtherRequestWaiting { get; }

        /// <summary>The racks of the side column, in its order, with whether the last reading of the list saw a planta.</summary>
        IReadOnlyList<WarehouseRackOption> RacksOfTheList();

        /// <summary>The rack whose page is open, or null.</summary>
        string ActiveRackId { get; }

        /// <summary>The rack has changes not written in an editor of the window (question 4).</summary>
        bool HasUnwrittenDraft(SessionId session, string rackId);

        /// <summary>Hands the read to the door; null when AutoCAD took it, or why not.</summary>
        string RequestRead(string seedRackId, string layer, string preferredHandle);

        /// <summary>Hands the placement to the door with the number and the drawing of the window; null when AutoCAD took it, or why not.</summary>
        string RequestPlace(Func<long, SessionId, WarehousePlaceRequest> build);

        void StopWaiting();

        /// <summary>The values saved last for this user; null when never saved (question 6).</summary>
        WarehouseLayoutValues LoadValues();

        void SaveValues(WarehouseLayoutValues values);
    }

    /// <summary>T5 al: a rack of the side column as the page «Almacén» sees it.</summary>
    public sealed record WarehouseRackOption(string RackId, string Name, string KindLabel, bool HasPlanta);

    /// <summary>T5 al: one rack of the list «Rack semilla».</summary>
    public sealed record WarehouseSeedOption(string RackId, string Label);

    /// <summary>T5 al: one figure of «Resultado».</summary>
    public sealed record WarehouseStatItem(string Label, string Value, string Hint)
    {
        public string AutomationName => Label + ": " + Value + " (" + Hint + ")";
    }

    /// <summary>T5 al: one line of «Resultado» («Error», «Warning» or «Info», for its colour).</summary>
    public sealed record WarehouseProblemItem(string Kind, string Text);

    /// <summary>T5 al: one entry of the legend of the scheme («Seed», «New», «Out», «Off», «Col»).</summary>
    public sealed record WarehouseLegendItem(string Kind, string Text);

    /// <summary>T5 al: one square of the scheme: its label, what it is, its tip, its size and the gap below it.</summary>
    public sealed record WarehouseSchemeCellItem(string Text, string Kind, string Tip, double Width, double Height, double GapBelow);

    /// <summary>T5 al: one row of the grid (A, B…), drawn as a column of squares with its column numbers going up; the gap after it is its aisle.</summary>
    public sealed record WarehouseSchemeStrip(string Label, IReadOnlyList<WarehouseSchemeCellItem> Cells, double GapAfter);

    /// <summary>T5 al: one number of the columns of the scheme (1, 2, 3… going up).</summary>
    public sealed record WarehouseSchemeNumber(string Text, double Height, double GapBelow);

    /// <summary>
    /// T5 al (I-64 A-14, line D; prototype «almacen», questions 1 to 11 accepted): the page «Almacén» of the RackCad window, RACKLAYOUT and
    /// RACKRELLENAR without a modal. Its tabs «Rejilla» and «Rellenar un área» share one set of values (question 11), remembered per user
    /// (question 6); the result and the scheme are computed here, at once, with the rules of today (<see cref="WarehouseLayoutRules"/>),
    /// and nothing touches the drawing until «Colocar» (question 2).
    /// <para>
    /// The planta of the seed and the site are read by the door (RACKALMACENLEER, no undo marker), ONLY after a gesture of the user: the
    /// first time the section is opened with a click for a drawing, «Llevar al almacén…» of the page of a rack, a new seed or layer chosen,
    /// «Volver a leer» and «Restablecer valores» when it changes the layer. «Colocar» is RACKALMACENCOLOCAR, which reads again and places
    /// nothing when the planta or the site changed (question 5); its answer brings the reading made after placing. Each drawing keeps its
    /// seed and its last reading; a command after the reading that is not the page's own makes it «Puede no estar al día». The page has
    /// nothing unwritten, so it never vetoes closing the drawing (question 6).
    /// </para>
    /// </summary>
    public sealed class WarehousePageViewModel : ObservableObject
    {
        private const int MaxSchemeRows = 30;
        private const int MaxSchemeCols = 24;

        private readonly IWarehousePageHost _host;
        private readonly Dictionary<SessionId, DrawingState> _drawings = new();

        private SessionId? _session;
        private (long Id, SessionId Session)? _reading;
        private Placing _placing;
        private bool _ownCommandPending;
        private bool _readOnShow;

        private WarehouseLayoutValues _values = WarehouseLayoutValues.Defaults;
        private bool _valuesLoaded;
        private WarehouseLayoutMode _mode = WarehouseLayoutMode.Grid;
        private bool _fitOpen;
        private bool _fitAsk;
        private string _message = string.Empty;
        private bool _messageIsError;

        private WarehouseLayoutAssessment _assessment;
        private IReadOnlyList<WarehouseSeedOption> _seedOptions = Array.Empty<WarehouseSeedOption>();
        private IReadOnlyList<string> _layerOptions = Array.Empty<string>();
        private IReadOnlyList<WarehouseStatItem> _stats = Array.Empty<WarehouseStatItem>();
        private IReadOnlyList<WarehouseProblemItem> _problems = Array.Empty<WarehouseProblemItem>();
        private IReadOnlyList<WarehouseLegendItem> _legend = Array.Empty<WarehouseLegendItem>();
        private IReadOnlyList<WarehouseSchemeStrip> _strips = Array.Empty<WarehouseSchemeStrip>();
        private IReadOnlyList<WarehouseSchemeNumber> _numbers = Array.Empty<WarehouseSchemeNumber>();
        private string _noPlantaText = string.Empty;
        private string _schemeName = string.Empty;
        private string _schemeNote = string.Empty;
        private string _noSchemeText = string.Empty;
        private double _numbersGap;

        private sealed class DrawingState
        {
            public string SeedRackId;
            public WarehouseReading Reading;
            public bool Stale;
        }

        private sealed record Placing(long Id, SessionId Session, WarehousePlaceRequest Request, bool Started);

        internal WarehousePageViewModel(IWarehousePageHost host)
        {
            _host = host ?? throw new ArgumentNullException(nameof(host));
            Recompute();
        }

        private DrawingState Current => _session.HasValue && _drawings.TryGetValue(_session.Value, out var state) ? state : null;

        private string SeedRackId => Current?.SeedRackId;

        /// <summary>The reading of the drawing for the seed chosen now (a reading of another seed is no reading of this one).</summary>
        internal WarehouseReading CurrentReading
        {
            get
            {
                var state = Current;
                if (state?.Reading == null || state.SeedRackId == null) return null;
                return string.Equals(state.Reading.SeedRackId, state.SeedRackId, StringComparison.OrdinalIgnoreCase) ? state.Reading : null;
            }
        }

        public bool IsReading => _reading.HasValue && _session.HasValue && _reading.Value.Session == _session.Value;

        public bool IsWriting => _placing != null && _session.HasValue && _placing.Session == _session.Value;

        public bool IsEditable => !IsWriting;

        // ------------------------------------------------------------------ the head of the page

        public string ChipText
        {
            get
            {
                if (!_session.HasValue) return "Sin dibujo";
                if (IsWriting) return "Escribiendo";
                if (IsReading) return "Leyendo el dibujo";
                var state = Current;
                if (state?.Reading == null) return "Sin leer";
                if (!state.Reading.IsRead) return "No se pudo leer";
                return state.Stale ? "Puede no estar al día" : "Lectura al día";
            }
        }

        /// <summary>«Limpio», «Modificado», «Escribiendo», «Error» or «Nada», for the colour of the chip.</summary>
        public string ChipKind
        {
            get
            {
                if (!_session.HasValue) return "Nada";
                if (IsWriting || IsReading) return "Escribiendo";
                var state = Current;
                if (state?.Reading == null) return "Nada";
                if (!state.Reading.IsRead) return "Error";
                return state.Stale ? "Modificado" : "Limpio";
            }
        }

        public string PlaceText => _assessment.CanPlace ? "Colocar " + WarehouseLayoutRules.Plural(_assessment.Count, "rack", "racks") : "Colocar";

        public bool CanPlace => PlaceBlockedText.Length == 0;

        /// <summary>Why «Colocar» is off, next to it; empty when it can place.</summary>
        public string PlaceBlockedText
        {
            get
            {
                if (!_session.HasValue) return "Abre o crea un dibujo para usar «Almacén».";
                if (!_host.HasWarehouseDoor) return "Aquí no se puede leer ni escribir el dibujo.";
                if (IsWriting) return "AutoCAD está escribiendo.";
                if (IsReading) return "AutoCAD está leyendo la planta.";
                if (_host.OtherRequestWaiting) return "La ventana espera otra respuesta de AutoCAD: espera a que termine.";
                if (SeedRackId == null) return "Elige el rack semilla.";
                return _assessment.BlockedReason ?? string.Empty;
            }
        }

        public bool CanRead => _session.HasValue && _host.HasWarehouseDoor && !IsWriting && !IsReading && !_host.OtherRequestWaiting && SeedRackId != null;

        public bool CanReset => !IsWriting && _values != WarehouseLayoutValues.Defaults;

        public bool ShowWriting => IsWriting;

        public string WritingText
        {
            get
            {
                var placing = _placing;
                if (placing == null || !IsWriting) return string.Empty;
                var request = placing.Request;
                var name = DisplayName(CurrentReading?.SeedName);
                var number = " (" + WarehouseLayoutRules.PlaceCommand + ", petición n.º " + placing.Id.ToString(CultureInfo.InvariantCulture) + ")";
                if (request.Mode == WarehouseLayoutMode.Fill)
                    return "AutoCAD coloca " + WarehouseLayoutRules.Plural(_assessment.Count, "rack enlazado", "racks enlazados") + " de " + name
                           + " y sus etiquetas en la capa " + WarehouseLayoutRules.LabelLayer + number
                           + ". Antes vuelve a leer la planta y el sitio: si cambiaron, no coloca nada.";
                var kind = request.Values.Independent ? "independiente" : "enlazado";
                return "AutoCAD coloca " + WarehouseLayoutRules.Plural(_assessment.Count, "rack " + kind, "racks " + kind + "s") + " de " + name + " y "
                       + WarehouseLayoutRules.Plural(_assessment.Count + 1, "etiqueta", "etiquetas") + " en la capa " + WarehouseLayoutRules.LabelLayer
                       + number + ". Antes vuelve a leer la planta: si cambió, no coloca nada.";
            }
        }

        /// <summary>«Dejar de esperar» of the writing banner: until the command takes the request.</summary>
        public bool CanStopWriting => IsWriting && !_placing.Started;

        public string WritingNote => IsWriting && _placing.Started
            ? "La orden ya corre en AutoCAD: se termina o se cancela allí, y la respuesta siempre llega."
            : string.Empty;

        public bool ShowFitQuestion => _fitAsk && !IsWriting && _mode == WarehouseLayoutMode.Grid && _assessment.Grid != null && _assessment.Grid.OutLabels.Count > 0;

        public string FitQuestionText
        {
            get
            {
                if (!ShowFitQuestion) return string.Empty;
                var grid = _assessment.Grid;
                return (grid.OutLabels.Count == 1 ? "Se sale" : "Se salen") + " del edificio de " + WarehouseLayoutRules.Number(grid.BuildingX) + " × "
                       + WarehouseLayoutRules.Number(grid.BuildingY) + " in: " + WarehouseLayoutRules.ListText(grid.OutLabels, 12)
                       + ". ¿Colocar la rejilla de todos modos?";
            }
        }

        public bool ShowStale => Current?.Reading != null && Current.Stale && !IsWriting;

        public string Message => _message;

        public bool MessageIsError => _messageIsError;

        public bool HasMessage => _message.Length > 0;

        // ------------------------------------------------------------------ the tabs

        public WarehouseLayoutMode Mode => _mode;

        public bool IsGridTab => _mode == WarehouseLayoutMode.Grid;

        public bool IsFillTab => _mode == WarehouseLayoutMode.Fill;

        public bool GridTabHasProblems => _assessment.GridHasErrors;

        public bool FillTabHasProblems => _assessment.FillHasErrors;

        public string TabNote => IsGridTab
            ? "Filas × columnas con sus pasillos, desde la planta semilla, con una etiqueta por rack."
            : "La rejilla más grande que cabe en el contorno de una capa, librando sus columnas. Prueba las dos orientaciones si se lo pides.";

        // ------------------------------------------------------------------ the seed

        public IReadOnlyList<WarehouseSeedOption> SeedOptions => _seedOptions;

        public string SelectedSeedId => SeedRackId;

        public string NoPlantaText => _noPlantaText;

        public string SeedLine
        {
            get
            {
                var reading = CurrentReading;
                if (SeedRackId == null) return string.Empty;
                if (reading == null) return WarehouseLayoutRules.NotReadText;
                if (!reading.IsRead || reading.Seed == null) return string.Empty;
                var seed = reading.Seed;
                return "Huella leída: " + WarehouseLayoutRules.Number(seed.FootprintX) + " × " + WarehouseLayoutRules.Number(seed.FootprintY)
                       + " in (a lo profundo × a lo largo). " + WarehouseLayoutRules.Plural(reading.PlantaCount, "planta", "plantas")
                       + " de este rack en el dibujo." + (seed.WasSelected ? " La semilla es la planta seleccionada en AutoCAD." : string.Empty);
            }
        }

        /// <summary>Why the reading has no seed (or could not read), in its place.</summary>
        public string SeedProblem
        {
            get
            {
                var reading = CurrentReading;
                if (reading == null) return string.Empty;
                if (!reading.IsRead) return reading.Failure;
                return reading.Seed == null ? reading.SeedProblem ?? string.Empty : string.Empty;
            }
        }

        public string DraftWarning
        {
            get
            {
                var rack = SeedRackId;
                if (rack == null || !_session.HasValue || !_host.HasUnwrittenDraft(_session.Value, rack)) return string.Empty;
                return WarehouseLayoutRules.Sentence(DisplayName(NameOfSeed())) + " tiene cambios sin escribir en la ventana: el layout copia lo que hay en el dibujo. Si quieres los cambios, escríbelos antes con «Actualizar» en su editor.";
            }
        }

        public string MultiWarning
        {
            get
            {
                var reading = CurrentReading;
                if (reading == null || !reading.IsRead || reading.PlantaCount <= 1) return string.Empty;
                return "Ya hay " + reading.PlantaCount.ToString(CultureInfo.InvariantCulture) + " plantas de " + DisplayName(NameOfSeed())
                       + " en el dibujo. La semilla es la que esté seleccionada en AutoCAD. Repetir el layout desde la misma semilla deja la rejilla nueva encima de la anterior.";
            }
        }

        public string InsideWarning => IsFillTab && _assessment.Fill?.SeedInside == true
            ? "La planta de " + DisplayName(NameOfSeed()) + " está dentro del área a rellenar: el relleno no la tiene en cuenta y una copia puede quedar encima. Muévela o bórrala después."
            : string.Empty;

        // ------------------------------------------------------------------ the values (two-way: each change is the user's)

        public string RowsText { get => _values.Rows; set => SetValues(_values with { Rows = value ?? string.Empty }); }

        public string ColsText { get => _values.Cols; set => SetValues(_values with { Cols = value ?? string.Empty }); }

        public string AisleRowsText { get => _values.AisleRows; set => SetValues(_values with { AisleRows = value ?? string.Empty }); }

        public string AisleColsText { get => _values.AisleCols; set => SetValues(_values with { AisleCols = value ?? string.Empty }); }

        public bool BackToBack { get => _values.BackToBack; set => SetValues(_values with { BackToBack = value }); }

        public string BackGapText { get => _values.BackGap; set => SetValues(_values with { BackGap = value ?? string.Empty }); }

        public string BuildingXText { get => _values.BuildingX; set => SetValues(_values with { BuildingX = value ?? string.Empty }); }

        public string BuildingYText { get => _values.BuildingY; set => SetValues(_values with { BuildingY = value ?? string.Empty }); }

        public string WallClearanceText { get => _values.WallClearance; set => SetValues(_values with { WallClearance = value ?? string.Empty }); }

        public string ColumnClearanceText { get => _values.ColumnClearance; set => SetValues(_values with { ColumnClearance = value ?? string.Empty }); }

        /// <summary>«Probar también la orientación girada 90°», shown off with back to back, which does not try it (dolor 7).</summary>
        public bool TryRotatedShown
        {
            get => _values.TryRotated && !_values.BackToBack;
            set
            {
                if (_values.BackToBack) return;
                SetValues(_values with { TryRotated = value });
            }
        }

        public bool IsIndependent => _values.Independent;

        public bool IsLinked => !_values.Independent;

        public bool BackGapEnabled => _values.BackToBack && IsEditable;

        public bool TryRotatedEnabled => !_values.BackToBack && IsEditable;

        public string RowsError => ErrorOf(WarehouseLayoutRules.FieldRows);

        public string ColsError => ErrorOf(WarehouseLayoutRules.FieldCols);

        public string AisleRowsError => ErrorOf(WarehouseLayoutRules.FieldAisleRows);

        public string AisleColsError => ErrorOf(WarehouseLayoutRules.FieldAisleCols);

        public string BackGapError => _values.BackToBack ? ErrorOf(WarehouseLayoutRules.FieldBackGap) : string.Empty;

        public string BuildingXError => ErrorOf(WarehouseLayoutRules.FieldBuildingX);

        public string BuildingYError => ErrorOf(WarehouseLayoutRules.FieldBuildingY);

        public string WallClearanceError => ErrorOf(WarehouseLayoutRules.FieldWallClearance);

        public string ColumnClearanceError => ErrorOf(WarehouseLayoutRules.FieldColumnClearance);

        public string GridError => _assessment.GridError ?? string.Empty;

        public string BackGapHint => _values.BackToBack ? "Lo comparten las dos filas de cada par." : "Solo con «Filas espalda con espalda».";

        public string RotatedHint => _values.BackToBack
            ? "Con espalda con espalda solo se prueba la orientación natural: gira la planta en el dibujo si quieres la otra."
            : "Al girar, los pasillos giran con el rack: el de picking siempre va frente a las caras. Si empatan, se queda la natural.";

        public string CopyText
        {
            get
            {
                if (IsFillTab) return "Siempre enlazadas: un solo bloque, editar una edita todas y el BOM las cuenta como copias del mismo rack.";
                if (!_values.Independent)
                    return "Enlazadas: un solo bloque, editar una edita todas y el BOM las cuenta como copias del mismo rack. Más ligero.";
                var name = string.IsNullOrWhiteSpace(NameOfSeed()) ? "Rack" : NameOfSeed().Trim();
                var samples = (_assessment.Grid?.Cells ?? Array.Empty<WarehouseSchemeCell>())
                    .Where(c => c.Kind != WarehouseCellKind.Seed).Take(2).Select(c => "«" + name + " " + c.Label + "»").ToList();
                var shown = samples.Count > 0 ? string.Join(", ", samples) + "…" : "«" + name + " B1»…";
                return "Independientes: cada copia es un rack nuevo, con su GUID y su nombre (" + shown + "), y solo tiene la vista en planta.";
            }
        }

        public bool FitOpen => _fitOpen || _assessment.Errors.ContainsKey(WarehouseLayoutRules.FieldBuildingX) || _assessment.Errors.ContainsKey(WarehouseLayoutRules.FieldBuildingY);

        public string FitSummary => _assessment.Grid != null && _assessment.Grid.FitChecked
            ? "Edificio " + WarehouseLayoutRules.Number(_assessment.Grid.BuildingX) + " × " + WarehouseLayoutRules.Number(_assessment.Grid.BuildingY) + " in"
            : "Sin verificar";

        // ------------------------------------------------------------------ the site

        public IReadOnlyList<string> LayerOptions => _layerOptions;

        public string SelectedLayer => _values.Layer;

        public string SiteLine
        {
            get
            {
                var site = WarehouseLayoutRules.SiteFor(CurrentReading, _values.Layer);
                if (site == null || !site.HasOutline) return string.Empty;
                var xs = site.Boundary.Select(p => p.X).ToList();
                var ys = site.Boundary.Select(p => p.Y).ToList();
                var obstacles = site.Obstacles?.Count ?? 0;
                return "Contorno: polilínea cerrada de " + WarehouseLayoutRules.Plural(site.Boundary.Count, "vértice", "vértices") + ", "
                       + WarehouseLayoutRules.Number(xs.Max() - xs.Min()) + " × " + WarehouseLayoutRules.Number(ys.Max() - ys.Min())
                       + " in (a lo profundo × a lo largo). " + WarehouseLayoutRules.Plural(obstacles, "obstáculo", "obstáculos") + ". "
                       + (site.IgnoredLarge > 0
                           ? WarehouseLayoutRules.Plural(site.IgnoredLarge, "objeto ignorado", "objetos ignorados") + " (casi tan grandes como el contorno: ¿contorno duplicado?)."
                           : "Ningún objeto ignorado.");
            }
        }

        public string SiteError
        {
            get
            {
                var reading = CurrentReading;
                if (reading == null || !reading.IsRead) return string.Empty;
                var site = WarehouseLayoutRules.SiteFor(reading, _values.Layer);
                if (site == null) return WarehouseLayoutRules.SiteNotReadText(_values.Layer);
                return site.HasOutline ? string.Empty : site.Problem ?? WarehouseLayoutRules.NoOutlineText;
            }
        }

        // ------------------------------------------------------------------ the result and the scheme

        public IReadOnlyList<WarehouseStatItem> Stats => _stats;

        public bool HasStats => _stats.Count > 0;

        public IReadOnlyList<WarehouseProblemItem> Problems => _problems;

        public bool HasProblems => _problems.Count > 0;

        public IReadOnlyList<WarehouseLegendItem> Legend => _legend;

        public IReadOnlyList<WarehouseSchemeStrip> SchemeStrips => _strips;

        public IReadOnlyList<WarehouseSchemeNumber> SchemeNumbers => _numbers;

        public double SchemeNumbersGap => _numbersGap;

        public bool HasScheme => _strips.Count > 0;

        public string SchemeName => _schemeName;

        public string SchemeNote => _schemeNote;

        public string NoSchemeText => _noSchemeText;

        // ------------------------------------------------------------------ gestures of the user

        /// <summary>A rack chosen in «Rack semilla»: its planta is read (a gesture).</summary>
        public void ChooseSeed(string rackId)
        {
            if (string.IsNullOrWhiteSpace(rackId) || IsWriting) return;
            var state = EnsureState();
            if (state == null || string.Equals(state.SeedRackId, rackId, StringComparison.OrdinalIgnoreCase)) return;
            state.SeedRackId = rackId.Trim();
            _fitAsk = false;
            Recompute();
            TryRead();
        }

        /// <summary>A layer chosen in «Capa del sitio»: its outline is read (a gesture, question 8).</summary>
        public void ChooseLayer(string layer)
        {
            var next = (layer ?? string.Empty).Trim();
            if (next.Length == 0 || IsWriting || string.Equals(next, _values.Layer, StringComparison.Ordinal)) return;
            SetValues(_values with { Layer = next });
            TryRead();
        }

        /// <summary>«Volver a leer» / «Volver a leer ahora».</summary>
        public void Read() => TryRead();

        public void ShowGrid() => SetMode(WarehouseLayoutMode.Grid);

        public void ShowFill() => SetMode(WarehouseLayoutMode.Fill);

        public void ChooseLinked() => SetValues(_values with { Independent = false });

        public void ChooseIndependent() => SetValues(_values with { Independent = true });

        public void ToggleFit()
        {
            _fitOpen = !FitOpen;
            RaiseAll();
        }

        /// <summary>«Restablecer valores»: the values of the classic windows; a layer that changes is read again (question 6).</summary>
        public void Reset()
        {
            if (IsWriting) return;
            var layerChanged = !string.Equals(_values.Layer, WarehouseLayoutValues.Defaults.Layer, StringComparison.Ordinal);
            _values = WarehouseLayoutValues.Defaults;
            _fitAsk = false;
            Save();
            Say("Valores restablecidos: los de las ventanas clásicas.", false);
            Recompute();
            if (layerChanged && SeedRackId != null && CanRead) TryRead(keepMessage: true);
        }

        /// <summary>«Colocar N racks»: a grid that leaves its building asks first, in line.</summary>
        public void Place()
        {
            if (!CanPlace) return;
            if (_mode == WarehouseLayoutMode.Grid && _assessment.Grid != null && _assessment.Grid.OutLabels.Count > 0)
            {
                _fitAsk = true;
                RaiseAll();
                return;
            }

            DoPlace(false);
        }

        /// <summary>«Colocar de todos modos».</summary>
        public void PlaceAnyway()
        {
            if (!ShowFitQuestion || !CanPlace) return;
            _fitAsk = false;
            DoPlace(true);
        }

        /// <summary>«Seguir ajustando»: the question goes, «Verificar encaje» opens.</summary>
        public void KeepAdjusting()
        {
            _fitAsk = false;
            _fitOpen = true;
            RaiseAll();
        }

        public void StopWaiting() => _host.StopWaiting();

        public void DismissMessage() => Say(string.Empty, false);

        /// <summary>A failure the view contained: it is said in the message of the page.</summary>
        public void ShowError(Exception exception)
        {
            var detail = string.IsNullOrWhiteSpace(exception?.Message) ? exception?.GetType().Name : exception.Message;
            Say("La página tuvo un fallo contenido: " + detail, true);
        }

        // ------------------------------------------------------------------ what the window tells it

        /// <summary>
        /// The page was shown by a gesture of the user (the section, «Llevar al almacén…»): the values are loaded the first time, the seed of
        /// the drawing is chosen, and the planta is read when the drawing has no reading of it (or «Llevar al almacén…» asked).
        /// </summary>
        internal void Shown()
        {
            LoadValues();
            var state = EnsureState();
            Recompute();
            if (state == null) return;
            if ((_readOnShow || CurrentReading == null) && state.SeedRackId != null && !IsReading && !IsWriting)
            {
                _readOnShow = false;
                TryRead();
            }

            _readOnShow = false;
        }

        /// <summary>«Llevar al almacén…» of the page of a rack: that rack is the seed, on «Rejilla», read when the page is shown.</summary>
        internal void TakeSeed(string rackId)
        {
            if (string.IsNullOrWhiteSpace(rackId)) return;
            var state = EnsureState(rackId);
            if (state == null) return;
            state.SeedRackId = rackId.Trim();
            _mode = WarehouseLayoutMode.Grid;
            _fitAsk = false;
            _readOnShow = true;
            Recompute();
        }

        /// <summary>The window presented a snapshot: the page follows the drawing and the commands, and sends nothing on its own.</summary>
        internal void Present(SessionId? session, WorkspaceActionBlock block, bool warehouseInFlight, Func<SessionId, bool> isAlive)
        {
            if (isAlive != null)
            {
                foreach (var dead in _drawings.Keys.Where(s => !SafeAlive(isAlive, s)).ToList()) _drawings.Remove(dead);
            }

            var changed = false;
            if (session != _session)
            {
                var previous = _session;
                _session = session;
                _fitAsk = false;
                _message = string.Empty;
                _messageIsError = false;
                if (previous.HasValue && Current?.Reading != null) Current.Stale = true; // coming back to a drawing: maybe not up to date
                changed = true;
            }

            // A command after the reading that is not the page's own (its read or its placement, from the moment the command took the
            // request until the drawing is at rest again) may have changed the drawing.
            var commandRuns = block == WorkspaceActionBlock.CommandActive || block == WorkspaceActionBlock.ModalActive;
            if (commandRuns)
            {
                var state = Current;
                if (!_ownCommandPending && state?.Reading != null && !state.Stale)
                {
                    state.Stale = true;
                    changed = true;
                }
            }
            else if (!warehouseInFlight)
            {
                _ownCommandPending = false;
            }

            if (changed)
            {
                EnsureState();
                Recompute();
            }
        }

        /// <summary>The list of the side column, the drafts or the door changed: the seeds and the reasons follow.</summary>
        internal void Refresh()
        {
            EnsureState(); // a list read again may offer a seed now (choosing one is no read: nothing is sent)
            Recompute();
        }

        internal void ReadingStarted(long requestId, SessionId session)
        {
            _reading = (requestId, session);
            RaiseAll();
        }

        /// <summary>The command took a request of the page (Started): until the drawing is at rest, a command is the page's own.</summary>
        internal void Taken(long requestId)
        {
            if (_reading.HasValue && _reading.Value.Id == requestId) _ownCommandPending = true;
            if (_placing != null && _placing.Id == requestId)
            {
                _ownCommandPending = true;
                _placing = _placing with { Started = true };
            }

            RaiseAll();
        }

        /// <summary>The read was not handed over, the window stopped waiting, or its answer was not for this drawing.</summary>
        internal void ReadingStopped(string message)
        {
            _reading = null;
            if (message != null) Say(message, true);
            Recompute();
        }

        internal void Deliver(SessionId session, WarehouseReading reading)
        {
            if (reading == null) return;
            _reading = null;
            _ownCommandPending = true; // the answer arrives inside RACKALMACENLEER, which still runs
            if (!_drawings.TryGetValue(session, out var state))
            {
                state = new DrawingState { SeedRackId = reading.SeedRackId };
                _drawings[session] = state;
            }

            state.Reading = reading;
            state.Stale = false;
            Recompute();
        }

        internal void PlacingStarted(long requestId, SessionId session, WarehousePlaceRequest request)
        {
            _placing = new Placing(requestId, session, request, false);
            _fitAsk = false;
            Say(string.Empty, false);
            RaiseAll();
        }

        /// <summary>The placement was not handed over, the window stopped waiting, or its drawing was closed.</summary>
        internal void PlacingStopped(string message, bool error)
        {
            _placing = null;
            if (message != null) Say(message, error);
            Recompute();
        }

        /// <summary>The answer of RACKALMACENCOLOCAR: what it said, and the reading it made in the same command.</summary>
        internal void Placed(SessionId session, WarehousePlaced placed)
        {
            if (placed == null) return;
            _placing = null;
            _ownCommandPending = true;
            if (placed.Reading != null && placed.Reading.IsRead && _drawings.TryGetValue(session, out var state)
                && string.Equals(placed.Reading.SeedRackId, state.SeedRackId, StringComparison.OrdinalIgnoreCase))
            {
                state.Reading = placed.Reading;
                state.Stale = false;
            }

            Say(placed.Message ?? string.Empty, !placed.Placed);
            Recompute();
        }

        /// <summary>A request of the page is in flight (read or placement), for the window.</summary>
        internal bool HasRequestInFlight => _reading.HasValue || _placing != null;

        internal WarehouseLayoutValues Values => _values;

        internal WarehouseLayoutAssessment Assessment => _assessment;

        // ------------------------------------------------------------------ internals

        private void TryRead(bool keepMessage = false)
        {
            if (!_session.HasValue)
            {
                Say("Abre o crea un dibujo para usar «Almacén».", true);
                return;
            }

            if (IsWriting || IsReading) return;
            var seed = SeedRackId;
            if (seed == null)
            {
                Say("Elige el rack semilla: los racks con planta del dibujo.", true);
                return;
            }

            if (!keepMessage) Say(string.Empty, false);
            var previous = Current?.Reading;
            var preferred = previous?.Seed != null && string.Equals(previous.SeedRackId, seed, StringComparison.OrdinalIgnoreCase)
                ? previous.Seed.ReferenceHandle
                : null;
            var refusal = _host.RequestRead(seed, _values.Layer, preferred);
            if (refusal != null) Say(refusal, true);
            Recompute();
        }

        private void DoPlace(bool anyway)
        {
            var reading = CurrentReading;
            if (reading?.Seed == null) return;
            var mode = _mode;
            var values = _values;
            var fingerprint = WarehouseLayoutRules.Fingerprint(reading, mode);
            Save();
            var refusal = _host.RequestPlace((id, session) => new WarehousePlaceRequest(
                id, session, mode, reading.SeedRackId, reading.Seed.ReferenceHandle, values.Layer, fingerprint, values, anyway));
            if (refusal != null) Say(refusal, true);
            Recompute();
        }

        private void SetMode(WarehouseLayoutMode mode)
        {
            if (_mode == mode) return;
            _mode = mode;
            _fitAsk = false;
            Recompute();
        }

        private void SetValues(WarehouseLayoutValues next)
        {
            if (next == null || IsWriting || next == _values) return;
            _values = next;
            _fitAsk = false;
            Save();
            Recompute();
        }

        private void LoadValues()
        {
            if (_valuesLoaded) return;
            _valuesLoaded = true;
            try
            {
                _values = _host.LoadValues() ?? WarehouseLayoutValues.Defaults;
            }
            catch (Exception)
            {
                _values = WarehouseLayoutValues.Defaults; // settings that cannot be read are the defaults, as the store does
            }
        }

        private void Save()
        {
            try
            {
                _host.SaveValues(_values);
            }
            catch (Exception)
            {
                // Best effort, as the settings of RackCad: a value that is not remembered is no reason to stop the page.
            }
        }

        // The state of the drawing of the window, with a seed when there is one to choose: the rack asked, the active rack with planta, or
        // the first rack with planta of the side column.
        private DrawingState EnsureState(string preferred = null)
        {
            if (!_session.HasValue) return null;
            if (!_drawings.TryGetValue(_session.Value, out var state))
            {
                state = new DrawingState();
                _drawings[_session.Value] = state;
            }

            if (state.SeedRackId == null)
            {
                var racks = SafeRacks();
                var active = preferred ?? _host.ActiveRackId;
                var withPlanta = racks.Where(r => r.HasPlanta).ToList();
                state.SeedRackId = withPlanta.FirstOrDefault(r => string.Equals(r.RackId, active, StringComparison.OrdinalIgnoreCase))?.RackId
                                   ?? withPlanta.FirstOrDefault()?.RackId;
            }

            return state;
        }

        private IReadOnlyList<WarehouseRackOption> SafeRacks()
        {
            try
            {
                return _host.RacksOfTheList() ?? Array.Empty<WarehouseRackOption>();
            }
            catch (Exception)
            {
                return Array.Empty<WarehouseRackOption>();
            }
        }

        private string NameOfSeed()
        {
            var reading = CurrentReading;
            if (!string.IsNullOrWhiteSpace(reading?.SeedName)) return reading.SeedName.Trim();
            var rack = SeedRackId;
            return SafeRacks().FirstOrDefault(r => string.Equals(r.RackId, rack, StringComparison.OrdinalIgnoreCase))?.Name;
        }

        private string ErrorOf(string field) => _assessment.Errors.TryGetValue(field, out var error) ? error : string.Empty;

        private void Recompute()
        {
            _assessment = WarehouseLayoutRules.Assess(CurrentReading, _values, _mode);
            BuildSeeds();
            BuildLayers();
            BuildResult();
            RaiseAll();
        }

        private void BuildSeeds()
        {
            var racks = SafeRacks();
            var seed = SeedRackId;
            var options = new List<WarehouseSeedOption>();
            foreach (var rack in racks.Where(r => r.HasPlanta))
            {
                var draft = _session.HasValue && _host.HasUnwrittenDraft(_session.Value, rack.RackId);
                options.Add(new WarehouseSeedOption(rack.RackId,
                    DisplayName(rack.Name) + (string.IsNullOrWhiteSpace(rack.KindLabel) ? string.Empty : " · " + rack.KindLabel)
                    + (draft ? " · con cambios sin escribir" : string.Empty)));
            }

            // A seed the list no longer offers (read again without it) stays in the list, by the name of its reading.
            if (seed != null && options.All(o => !string.Equals(o.RackId, seed, StringComparison.OrdinalIgnoreCase)))
                options.Insert(0, new WarehouseSeedOption(seed, DisplayName(NameOfSeed() ?? seed)));
            if (!options.SequenceEqual(_seedOptions)) _seedOptions = options; // the same list is kept: the view does not reset its choice

            var without = racks.Where(r => !r.HasPlanta).Select(r => DisplayName(r.Name)).ToList();
            if (racks.Count == 0) _noPlantaText = "La lista del dibujo no tiene racks: pulsa «Volver a leer la lista».";
            else if (without.Count > 0)
                _noPlantaText = "Sin planta, no se ofrecen: " + WarehouseLayoutRules.ListText(without, 4) + ". Inserta su planta con «Insertar vista…» para usarlos.";
            else _noPlantaText = "Solo se ofrecen los racks con planta en el dibujo (según la última lectura de la lista).";
        }

        private void BuildLayers()
        {
            var layers = new List<string>(CurrentReading?.Layers ?? Array.Empty<string>());
            if (!layers.Any(l => string.Equals(l, _values.Layer, StringComparison.OrdinalIgnoreCase))) layers.Add(_values.Layer);
            layers.Sort(StringComparer.CurrentCultureIgnoreCase);
            if (!layers.SequenceEqual(_layerOptions, StringComparer.Ordinal)) _layerOptions = layers;
        }

        private void BuildResult()
        {
            var stats = new List<WarehouseStatItem>();
            var problems = (_assessment.Problems ?? Array.Empty<WarehouseProblem>())
                .Select(p => new WarehouseProblemItem(p.Kind.ToString(), p.Text)).ToList();
            var legend = new List<WarehouseLegendItem>();
            _strips = Array.Empty<WarehouseSchemeStrip>();
            _numbers = Array.Empty<WarehouseSchemeNumber>();
            _schemeName = string.Empty;
            _schemeNote = string.Empty;
            _noSchemeText = string.Empty;
            var reading = CurrentReading;
            if (reading?.Seed == null) _noSchemeText = "Sin lectura de la planta no hay esquema.";

            var grid = _mode == WarehouseLayoutMode.Grid ? _assessment.Grid : null;
            if (grid != null)
            {
                var fit = grid.FitChecked
                    ? grid.OutLabels.Count > 0 ? WarehouseLayoutRules.Plural(grid.OutLabels.Count, "se sale", "se salen") : "Cabe"
                    : "Sin verificar";
                stats.Add(new WarehouseStatItem("Racks nuevos", grid.NewRacks.ToString(CultureInfo.InvariantCulture), "la semilla ya está en el dibujo"));
                stats.Add(new WarehouseStatItem("En la rejilla",
                    grid.Rows.ToString(CultureInfo.InvariantCulture) + " × " + grid.Cols.ToString(CultureInfo.InvariantCulture) + " = "
                    + (grid.Rows * grid.Cols).ToString(CultureInfo.InvariantCulture), "filas × columnas, con la semilla"));
                stats.Add(new WarehouseStatItem("Ocupa",
                    WarehouseLayoutRules.Number(grid.Plan.TotalX) + " × " + WarehouseLayoutRules.Number(grid.Plan.TotalY) + " in", "a lo profundo × a lo largo"));
                stats.Add(new WarehouseStatItem("Encaje", fit,
                    grid.FitChecked
                        ? "edificio de " + WarehouseLayoutRules.Number(grid.BuildingX) + " × " + WarehouseLayoutRules.Number(grid.BuildingY) + " in, solo bordes"
                        : "abre «Verificar encaje»"));
                legend.Add(new WarehouseLegendItem("Seed", "Semilla (ya está)"));
                legend.Add(new WarehouseLegendItem("New", "Rack nuevo"));
                legend.Add(new WarehouseLegendItem("Out", "Se sale del edificio"));
                if (grid.Rows <= MaxSchemeRows && grid.Cols <= MaxSchemeCols)
                {
                    BuildStrips(grid.Rows, grid.Cols, grid.FootprintX <= grid.FootprintY, grid.AisleX, grid.AisleY, grid.Paired, grid.Cells, KindOfGridCell, TipOfGridCell);
                    _schemeName = "Esquema de la rejilla: " + grid.Rows.ToString(CultureInfo.InvariantCulture) + " filas por "
                                  + grid.Cols.ToString(CultureInfo.InvariantCulture) + " columnas; A1 es la semilla y hay "
                                  + WarehouseLayoutRules.Plural(grid.NewRacks, "rack nuevo", "racks nuevos")
                                  + (grid.OutLabels.Count > 0 ? "; se salen del edificio " + WarehouseLayoutRules.ListText(grid.OutLabels, 12) : string.Empty) + ".";
                    _schemeNote = "X →: filas A, B, C… a lo profundo, separadas por el pasillo de picking. Y ↑: columnas 1, 2, 3… a lo largo de la hilera. A1 es la semilla: la rejilla crece desde ella.";
                }
                else
                {
                    _noSchemeText = "El esquema se dibuja hasta 30 filas × 24 columnas; esta rejilla tiene " + grid.Rows.ToString(CultureInfo.InvariantCulture)
                                    + " × " + grid.Cols.ToString(CultureInfo.InvariantCulture) + ". El recuento y los problemas valen igual.";
                }
            }

            var fill = _mode == WarehouseLayoutMode.Fill ? _assessment.Fill : null;
            if (fill != null)
            {
                var result = fill.Result;
                var rotated = result.Orientation == RackOrientation.Rotated;
                stats.Add(new WarehouseStatItem("Caben", fill.Kept.ToString(CultureInfo.InvariantCulture), "racks enlazados nuevos"));
                stats.Add(new WarehouseStatItem("Orientación", rotated ? "Girada 90°" : "Normal",
                    _values.BackToBack ? "solo la natural, por espalda con espalda" : _values.TryRotated ? "se usa la que quepa más" : "no se prueba la girada"));
                stats.Add(new WarehouseStatItem("Rejilla probada",
                    result.RowsTried.ToString(CultureInfo.InvariantCulture) + " × " + result.ColsTried.ToString(CultureInfo.InvariantCulture), "filas × columnas sobre el contorno"));
                stats.Add(new WarehouseStatItem("Descartadas",
                    result.OmittedOutside.ToString(CultureInfo.InvariantCulture) + " fuera · " + result.OmittedByObstacle.ToString(CultureInfo.InvariantCulture) + " en columnas",
                    "fuera del contorno o sobre columnas con su holgura"));
                legend.Add(new WarehouseLegendItem("New", "Rack nuevo"));
                legend.Add(new WarehouseLegendItem("Off", "Fuera del contorno"));
                legend.Add(new WarehouseLegendItem("Col", "Sobre una columna"));
                if (result.RowsTried > 0 && result.RowsTried <= MaxSchemeRows && result.ColsTried <= MaxSchemeCols)
                {
                    BuildStrips(result.RowsTried, result.ColsTried, result.FootprintX <= result.FootprintY, fill.AisleX, fill.AisleY, _values.BackToBack,
                        fill.Cells, KindOfFillCell, TipOfFillCell);
                    _schemeName = "Esquema del relleno: rejilla probada de " + result.RowsTried.ToString(CultureInfo.InvariantCulture) + " filas por "
                                  + result.ColsTried.ToString(CultureInfo.InvariantCulture) + " columnas, orientación " + (rotated ? "girada 90°" : "normal")
                                  + "; caben " + fill.Kept.ToString(CultureInfo.InvariantCulture) + ", " + result.OmittedOutside.ToString(CultureInfo.InvariantCulture)
                                  + " fuera del contorno y " + result.OmittedByObstacle.ToString(CultureInfo.InvariantCulture) + " sobre columnas.";
                    _schemeNote = "X →: filas A, B, C… a lo profundo, separadas por el pasillo de picking. Y ↑: columnas 1, 2, 3… a lo largo de la hilera. "
                                  + "La rejilla empieza en la esquina del contorno (más la holgura a muros); la planta semilla no forma parte del relleno."
                                  + (rotated ? " Orientación girada 90°: cada rack, con su fondo a lo largo." : string.Empty);
                }
                else if (result.RowsTried > 0)
                {
                    _noSchemeText = "El esquema se dibuja hasta 30 filas × 24 columnas; esta rejilla tiene " + result.RowsTried.ToString(CultureInfo.InvariantCulture)
                                    + " × " + result.ColsTried.ToString(CultureInfo.InvariantCulture) + ". El recuento y los problemas valen igual.";
                }
            }

            if (_mode == WarehouseLayoutMode.Fill && reading?.Seed != null && fill == null && _assessment.FillError == null)
            {
                var site = WarehouseLayoutRules.SiteFor(reading, _values.Layer);
                if (site != null && !site.HasOutline) _noSchemeText = "Sin contorno no hay relleno: elige otra capa o arregla el contorno en el dibujo.";
            }

            if (reading?.Seed != null && _strips.Count == 0 && _noSchemeText.Length == 0) _noSchemeText = "Corrige los valores marcados para ver el esquema.";
            _stats = stats;
            _problems = problems;
            _legend = legend;
        }

        // The rows of the grid left to right (X), each a column of its squares from the last column at the top to the first at the bottom (Y
        // going up), with the gap of its aisle after it, as the prototype draws it. Not to scale.
        private void BuildStrips(int rows, int cols, bool tall, double aisleX, double aisleY, bool paired, IReadOnlyList<WarehouseSchemeCell> cells,
            Func<WarehouseSchemeCell, string> kindOf, Func<WarehouseSchemeCell, string> tipOf)
        {
            var width = tall ? 28.0 : 44.0;
            var height = tall ? 44.0 : 26.0;
            var gapY = aisleY > 0.0 ? 10.0 : 2.0;
            var byCell = cells.ToDictionary(c => (c.Row, c.Col));
            var numbers = new List<WarehouseSchemeNumber>();
            for (var c = cols - 1; c >= 0; c--)
                numbers.Add(new WarehouseSchemeNumber((c + 1).ToString(CultureInfo.InvariantCulture), height, c == 0 ? 0.0 : gapY));
            var strips = new List<WarehouseSchemeStrip>();
            for (var r = 0; r < rows; r++)
            {
                var gapAfter = r == rows - 1 ? 0.0 : paired ? (r % 2 == 0 ? 3.0 : 18.0) : aisleX > 0.0 ? 18.0 : 2.0;
                var items = new List<WarehouseSchemeCellItem>();
                for (var c = cols - 1; c >= 0; c--)
                {
                    if (!byCell.TryGetValue((r, c), out var cell)) continue;
                    items.Add(new WarehouseSchemeCellItem(cell.Label, kindOf(cell), tipOf(cell), width, height, c == 0 ? 0.0 : gapY));
                }

                strips.Add(new WarehouseSchemeStrip(WarehouseGridPlanner.RowLetters(r), items, gapAfter));
            }

            _numbers = numbers;
            _numbersGap = gapY;
            _strips = strips;
        }

        private static string KindOfGridCell(WarehouseSchemeCell cell) =>
            cell.Kind == WarehouseCellKind.Seed ? "Seed" : cell.Kind == WarehouseCellKind.OutOfBuilding ? "Out" : "New";

        private static string TipOfGridCell(WarehouseSchemeCell cell) =>
            cell.Label + (cell.Kind == WarehouseCellKind.Seed ? ": semilla, ya está en el dibujo"
                : cell.Kind == WarehouseCellKind.OutOfBuilding ? ": se sale del edificio" : ": rack nuevo");

        private static string KindOfFillCell(WarehouseSchemeCell cell) =>
            cell.Kind == WarehouseCellKind.Outside ? "Off" : cell.Kind == WarehouseCellKind.OnColumn ? "Col" : "New";

        private static string TipOfFillCell(WarehouseSchemeCell cell) =>
            cell.Label + (cell.Kind == WarehouseCellKind.Outside ? ": fuera del contorno"
                : cell.Kind == WarehouseCellKind.OnColumn ? ": sobre una columna" : ": rack nuevo");

        private void Say(string text, bool error)
        {
            _message = text ?? string.Empty;
            _messageIsError = error && _message.Length > 0;
            OnPropertyChanged(nameof(Message));
            OnPropertyChanged(nameof(MessageIsError));
            OnPropertyChanged(nameof(HasMessage));
        }

        // Every binding of the page reads again (the values are typed by the user, so a field shows what it already has).
        private void RaiseAll() => OnPropertyChanged(string.Empty);

        private static string DisplayName(string name) => string.IsNullOrWhiteSpace(name) ? "el rack" : name.Trim();

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
    }
}
