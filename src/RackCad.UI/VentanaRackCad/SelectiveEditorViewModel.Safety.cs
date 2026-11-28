using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Systems.Selective;
using RackCad.Application.VentanaRackCad;
using RackCad.Domain.Systems.Selective;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// One row of «Seguridad por poste» of a Selectivo, read only, with the columns of the approved draft: Poste · Botas · Protector lateral
    /// · Desviadores (niveles) · Separadores entre fondos. An empty cell is «—», as in Dinámico.
    /// </summary>
    public sealed class SelectivePostSafetyItem
    {
        internal SelectivePostSafetyItem(string post, string boots, string sideGuards, string deflectors, string separators, bool isTotal = false)
        {
            Post = post;
            Boots = boots;
            SideGuards = sideGuards;
            Deflectors = deflectors;
            Separators = separators;
            IsTotal = isTotal;
        }

        internal static SelectivePostSafetyItem Of(SelectivePostSafetyRow row)
            => new SelectivePostSafetyItem(
                PostText(row),
                Dash(SelectiveSafetyView.BootSide(row)),
                row.SideGuards == 0 ? "—" : (row.IsEndPost ? "Sí, extremo" : "Sí") + (row.SideGuards > 1 ? " (" + N(row.SideGuards) + ")" : string.Empty),
                row.Deflectors == 0 ? "—" : Levels(row.DeflectorLevels) + " (" + N(row.Deflectors) + ")",
                row.Separators == 0 ? "—" : N(row.Separators));

        /// <summary>The «Total» row: the pieces of each column (the separadores, «—» when the rack has one fondo).</summary>
        internal static SelectivePostSafetyItem Total(IReadOnlyList<SelectivePostSafetyRow> rows, bool severalFondos)
            => new SelectivePostSafetyItem(
                "Total",
                N(rows.Sum(r => r.Boots)),
                N(rows.Sum(r => r.SideGuards)),
                N(rows.Sum(r => r.Deflectors)),
                severalFondos ? N(rows.Sum(r => r.Separators)) : "—",
                isTotal: true);

        public string Post { get; }

        /// <summary>The side of the bota: «Izquierda», «Derecha» or both (decision 13; the text for both is provisional).</summary>
        public string Boots { get; }

        public string SideGuards { get; }

        /// <summary>The levels of the desviadores («N1–N3») and, in brackets, the pieces (both faces count).</summary>
        public string Deflectors { get; }

        public string Separators { get; }

        /// <summary>The last row of the table, «Total» (the foot of the table of the approved draft): the view shows it in bold.</summary>
        public bool IsTotal { get; }

        /// <summary>«P1»… for a frame post; an intermediate post of a medio frente is named after its front.</summary>
        internal static string PostText(SelectivePostSafetyRow row)
            => row.IsIntermediate
                ? "F" + N(row.IntermediateOfFront.Value + 1) + " · intermedio " + N(row.IntermediateOrdinal)
                : "P" + N(row.PostIndex + 1);

        // «N1–N3, N5»: the zero-based load levels of the desviador plan, N1 being its first troquel row.
        internal static string Levels(IReadOnlyList<int> levels)
        {
            var runs = new List<string>();
            var ordered = levels.Distinct().OrderBy(l => l).ToList();
            for (var i = 0; i < ordered.Count; i++)
            {
                var start = ordered[i];
                while (i + 1 < ordered.Count && ordered[i + 1] == ordered[i] + 1) i++;
                runs.Add(start == ordered[i] ? "N" + N(start + 1) : "N" + N(start + 1) + "–N" + N(ordered[i] + 1));
            }

            return string.Join(", ", runs);
        }

        private static string Dash(string text) => string.IsNullOrEmpty(text) ? "—" : text;

        private static string N(int value) => value.ToString(CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// One card of «En las celdas: frente × nivel» of «Extremos y seguridad» (the approved draft): its letter (T, P), its title, what it does
    /// in the rack («En 5 de 6 celdas con larguero · 10 piezas.», the pieces read from the BOM), its configuration when a variant is chosen,
    /// and whether «En todas las celdas» / «En ninguna» can write. Refreshed in place, so the button under the mouse survives a write.
    /// </summary>
    public sealed class SelectiveSafetyFamilyCard : ObservableObject
    {
        private string _summary = string.Empty;
        private string _config = string.Empty;
        private string _variantName = string.Empty;
        private bool _canAll;
        private bool _canNone;

        internal SelectiveSafetyFamilyCard(string type, string letter, string title)
        {
            Type = type;
            Letter = letter;
            Title = title;
        }

        /// <summary>The family in the catalog (<see cref="SelectiveSafetyDefaults.TopeType"/> or <see cref="SelectiveSafetyDefaults.ParrillaType"/>).</summary>
        public string Type { get; }

        public string Letter { get; }

        public string Title { get; }

        public bool IsTope => string.Equals(Type, SelectiveSafetyDefaults.TopeType, StringComparison.Ordinal);

        public string Summary { get => _summary; private set => SetProperty(ref _summary, value); }

        /// <summary>The variant and its measures, as «Editar seguridad…» left them; empty when no variant is chosen.</summary>
        public string Config
        {
            get => _config;
            private set
            {
                if (SetProperty(ref _config, value)) OnPropertyChanged(nameof(HasConfig));
            }
        }

        public bool HasConfig => _config.Length > 0;

        /// <summary>«En todas las celdas» writes something: a variant is chosen, some larguero is off, and the draft can be edited.</summary>
        public bool CanAll { get => _canAll; private set => SetProperty(ref _canAll, value); }

        /// <summary>«En ninguna» writes something: a variant is chosen, some larguero is on, and the draft can be edited.</summary>
        public bool CanNone { get => _canNone; private set => SetProperty(ref _canNone, value); }

        /// <summary>The names of the two buttons for UI Automation: the same text in both cards, so the name carries the family.</summary>
        public string AllName => Title + " en todas las celdas";

        public string NoneName => Title + " en ninguna celda";

        /// <summary>T4 seg: the name of the head of the card, a button that shows its family in «Variante y medidas».</summary>
        public string PickName => Title + ": ver su variante y sus medidas";

        /// <summary>T4 seg: the variant chosen, as the head of the card of the prototype says it («Sin variante» when none).</summary>
        public string VariantName { get => _variantName; private set => SetProperty(ref _variantName, value ?? string.Empty); }

        internal void Set(string summary, string config, bool canAll, bool canNone, string variantName = "Sin variante")
        {
            Summary = summary ?? string.Empty;
            Config = config ?? string.Empty;
            CanAll = canAll;
            CanNone = canNone;
            VariantName = variantName;
        }
    }

    /// <summary>T (tope) and P (parrilla) of a cell of «Matriz» (decisions 4 and 5), for the whole rack: the badges of the approved draft.</summary>
    public sealed partial class SelectiveCellItem
    {
        private bool _showsTope;
        private bool _topeVaries;
        private bool _showsParrilla;
        private bool _parrillaVaries;

        /// <summary>The cell carries a tope in some fondo of the view (the badge «T»).</summary>
        public bool ShowsTope { get => _showsTope; private set => SetProperty(ref _showsTope, value); }

        /// <summary>The fondos of the view disagree on the tope of this cell (the badge in italics).</summary>
        public bool TopeVaries
        {
            get => _topeVaries;
            private set
            {
                if (SetProperty(ref _topeVaries, value)) OnPropertyChanged(nameof(TopeTip));
            }
        }

        public string TopeTip => _topeVaries ? "Tope: varía entre los fondos de la vista" : "Tope";

        /// <summary>The cell carries a parrilla in some fondo of the view (the badge «P»).</summary>
        public bool ShowsParrilla { get => _showsParrilla; private set => SetProperty(ref _showsParrilla, value); }

        public bool ParrillaVaries
        {
            get => _parrillaVaries;
            private set
            {
                if (SetProperty(ref _parrillaVaries, value)) OnPropertyChanged(nameof(ParrillaTip));
            }
        }

        public string ParrillaTip => _parrillaVaries ? "Parrilla: varía entre los fondos de la vista" : "Parrilla";

        internal void SetSafety(SelectiveSafetyMark tope, SelectiveSafetyMark parrilla)
        {
            ShowsTope = tope == SelectiveSafetyMark.On || tope == SelectiveSafetyMark.Varies;
            TopeVaries = tope == SelectiveSafetyMark.Varies;
            ShowsParrilla = parrilla == SelectiveSafetyMark.On || parrilla == SelectiveSafetyMark.Varies;
            ParrillaVaries = parrilla == SelectiveSafetyMark.Varies;
        }
    }

    /// <summary>
    /// «Extremos y seguridad» of the Selectivo editor (entrega 2): T/P on the selected cells, for the whole rack (the section «Seguridad de la
    /// celda» of the inspector and the marks of the matrix), the cards of the two families, «Extremos y fondos» and the table by post. T4 seg:
    /// the table is edited and the variant and the measures are in «Variante y medidas» (SelectiveEditorViewModel.Seg.cs); the window no longer
    /// sets <see cref="SafetyEditor"/>, the bridge to the classic dialog of D-09, which stays here unused until the classic editor goes.
    /// </summary>
    public sealed partial class SelectiveEditorViewModel
    {
        /// <summary>The index of the tab «Extremos y seguridad» (between Matriz and Avanzado, as in Dinámico).</summary>
        public const int SafetyTabIndex = 2;

        // S-20 / D-11 of the approved draft: the label of a box of «Seguridad de la celda», by its state.
        private const string TopeLabel = "Tope (T)";
        private const string TopeVariesLabel = "Tope (varía; marcar lo pone en todas)";
        private const string TopeUnchosenLabel = "Tope: sin variante elegida";
        private const string TopeOnFloorLabel = "Tope: no en el piso sin larguero";
        private const string ParrillaLabel = "Parrilla (P)";
        private const string ParrillaVariesLabel = "Parrilla (varía; marcar lo pone en todas)";
        private const string ParrillaUnchosenLabel = "Parrilla: sin variante elegida";
        private const string ParrillaOnFloorLabel = "Parrilla: no en el piso sin larguero";

        private const string NoDialogText = "El diálogo de seguridad no está disponible aquí: edita la seguridad con RACKEDITAR (el editor clásico).";

        private Func<SelectiveDraftEditor, IReadOnlyList<SelectiveSafetySelection>> _safetyEditor;

        // What the BOM counts of the safety (the table by post and the pieces of the cards), with the system it was counted on: a
        // recomposition gives a new system, and only then is it counted again, on a copy (the BOM imposes on the custom cabeceras it reads).
        private SelectiveRackSystem _safetyCountsOf;
        private bool _safetyCounted;
        private SelectiveSafetyCounts _safetyCounts = SelectiveSafetyCounts.None;
        private IReadOnlyList<SelectivePostSafetyItem> _postSafetyItems = Array.Empty<SelectivePostSafetyItem>();
        private IReadOnlyList<SelectivePostSafetyItem> _postSafetyTable = Array.Empty<SelectivePostSafetyItem>();
        private IReadOnlyList<string> _endLines = Array.Empty<string>();

        private SelectiveSafetyFamilyCard[] _familyCards;

        /// <summary>
        /// What «Editar seguridad…» opens: the safety dialog of the classic editor over copies of the safety of the draft (its result, or null
        /// when it was cancelled). The window sets it when it can show a modal over AutoCAD (D-09); without it the button is not offered.
        /// </summary>
        public Func<SelectiveDraftEditor, IReadOnlyList<SelectiveSafetySelection>> SafetyEditor
        {
            get => _safetyEditor;
            set
            {
                _safetyEditor = value;
                OnPropertyChanged(nameof(CanEditSafety));
                OnPropertyChanged(nameof(SafetyEditText));
            }
        }

        public bool CanEditSafety => CanEdit && _safetyEditor != null;

        /// <summary>What the line above «Editar seguridad…» says: what the dialog edits, or why it is not offered (as in Dinámico).</summary>
        public string SafetyEditText
        {
            get
            {
                if (_safetyEditor == null) return NoDialogText;
                if (!CanEdit) return BlockedEditText;
                return "Las botas, los protectores laterales, los desviadores, los topes y las parrillas, con su variante y sus medidas, se eligen en el diálogo de seguridad del editor clásico.";
            }
        }

        /// <summary>Test seam, as <see cref="CalculatedBuilds"/>: how many times the safety was counted with the BOM (the table and the cards).</summary>
        internal int SafetyCountBuilds { get; private set; }

        // ------------------------------------------------------------------ «Seguridad por poste»

        /// <summary>«Seguridad por poste», counted with the projections of the BOM over the last valid recomposition.</summary>
        public IReadOnlyList<SelectivePostSafetyItem> PostSafety
        {
            get
            {
                RefreshSafetyCounts();
                return _postSafetyItems;
            }
        }

        /// <summary>The «Total» row of «Seguridad por poste».</summary>
        public SelectivePostSafetyItem PostSafetyTotal => SelectivePostSafetyItem.Total(PostSafetyRows(), _editor.FondoCount > 1);

        /// <summary>The rows of the table of the view: <see cref="PostSafety"/> and, last, <see cref="PostSafetyTotal"/> (the foot of the draft).</summary>
        public IReadOnlyList<SelectivePostSafetyItem> PostSafetyTable
        {
            get
            {
                RefreshSafetyCounts();
                return _postSafetyTable;
            }
        }

        /// <summary>The summary above the table, as the draft says it: «8 botas · 2 protectores · 12 desviadores, en 5 postes.»</summary>
        public string SafetySummary
        {
            get
            {
                var rows = PostSafetyRows();
                var several = _editor.FondoCount > 1;
                return Plural(rows.Sum(r => r.Boots), "bota", "botas")
                       + " · " + Plural(rows.Sum(r => r.SideGuards), "protector", "protectores")
                       + " · " + Plural(rows.Sum(r => r.Deflectors), "desviador", "desviadores")
                       + (several ? " · " + Plural(rows.Sum(r => r.Separators), "separador", "separadores") : string.Empty)
                       + ", en " + Plural(rows.Count, "poste", "postes") + ".";
            }
        }

        // ------------------------------------------------------------------ the cards and «Extremos y fondos»

        /// <summary>«En las celdas: frente × nivel»: the cards of the topes (T) and the parrillas (P), refreshed in place.</summary>
        public IReadOnlyList<SelectiveSafetyFamilyCard> FamilyCards
        {
            get
            {
                RefreshFamilyCards();
                return _familyCards;
            }
        }

        /// <summary>
        /// «Extremos y fondos» (the approved draft): the protector lateral, the botas, the desviadores and the separadores between fondos, in
        /// one line each. What is placed is read from the table by post (counted as the BOM); the measures, from the safety of the draft.
        /// </summary>
        public IReadOnlyList<string> EndLines
        {
            get
            {
                RefreshSafetyCounts();
                return _endLines;
            }
        }

        /// <summary>«En todas las celdas» (true) / «En ninguna» (false) of a card.</summary>
        public string SetFamilyEverywhere(string type, bool on)
            => string.Equals(type, SelectiveSafetyDefaults.TopeType, StringComparison.Ordinal) ? SetTopeEverywhere(on) : SetParrillaEverywhere(on);

        // ------------------------------------------------------------------ «Seguridad de la celda» (the inspector)

        /// <summary>T at the selected cells (null when it varies or no tope is chosen).</summary>
        public bool? SelectionTope
        {
            get
            {
                var value = SelectiveSafetyView.ReadTope(_editor, SelectedPositions);
                return value.HasValue ? value.Value : (bool?)null;
            }
        }

        /// <summary>P at the selected cells (null when it varies or no parrilla is chosen).</summary>
        public bool? SelectionParrilla
        {
            get
            {
                var value = SelectiveSafetyView.ReadParrilla(_editor, SelectedPositions);
                return value.HasValue ? value.Value : (bool?)null;
            }
        }

        /// <summary>
        /// The box «Tope» of «Seguridad de la celda»: true when every larguero of the selection has it, false when none (or no tope is chosen,
        /// or the selection has no larguero), null when they differ. When it says «varía», a click puts the tope in every cell of the selection
        /// and never takes it out (S-20 / D-11, the approved draft: «varía; marcar lo pone en todas»): WPF turns a click on an indeterminate
        /// box into false, and that false means «all» here.
        /// </summary>
        public bool? SelectionTopeCheck
        {
            get => Box(SelectiveSafetyView.ReadTope(_editor, SelectedPositions));
            set => WriteBox(SelectionTopeCheck, value, SetTope);
        }

        /// <summary>The box «Parrilla» of «Seguridad de la celda», as <see cref="SelectionTopeCheck"/>.</summary>
        public bool? SelectionParrillaCheck
        {
            get => Box(SelectiveSafetyView.ReadParrilla(_editor, SelectedPositions));
            set => WriteBox(SelectionParrillaCheck, value, SetParrilla);
        }

        /// <summary>The label of the box «Tope» (the approved draft): it says why it cannot be marked, or that it varies and what a click does.</summary>
        public string SelectionTopeLabel
            => BoxLabel(SelectiveSafetyDefaults.TopeType, SelectiveSafetyView.ReadTope(_editor, SelectedPositions), TopeLabel, TopeVariesLabel, TopeUnchosenLabel, TopeOnFloorLabel);

        public string SelectionParrillaLabel
            => BoxLabel(SelectiveSafetyDefaults.ParrillaType, SelectiveSafetyView.ReadParrilla(_editor, SelectedPositions), ParrillaLabel, ParrillaVariesLabel, ParrillaUnchosenLabel, ParrillaOnFloorLabel);

        /// <summary>The box «Tope» can write: a tope is chosen, the selection has a larguero, and the draft can be edited.</summary>
        public bool CanEditSelectionTope => CanEdit && SelectiveSafetyView.ReadTope(_editor, SelectedPositions).Count > 0;

        public bool CanEditSelectionParrilla => CanEdit && SelectiveSafetyView.ReadParrilla(_editor, SelectedPositions).Count > 0;

        /// <summary>The note under the boxes (the draft's «safNote»), and what is left out when the selection has cells on the floor.</summary>
        public string SafetyNote
        {
            get
            {
                var note = "Van por frente y nivel en todo el rack" + (HasFondos ? ", no por fondo" : string.Empty)
                           + ", como hoy. Su variante y sus medidas, en «Extremos y seguridad».";
                var onFloor = _selection.Count(p => !SelectiveSafetyView.IsCell(_editor, p.FrontIndex, p.LevelIndex));
                return onFloor == 0
                    ? note
                    : note + " " + (onFloor == 1
                        ? "Una celda de la selección está en el piso, sin larguero: no lleva tope ni parrilla y se deja fuera."
                        : Plural(onFloor, "celda", "celdas") + " de la selección están en el piso, sin larguero: no llevan tope ni parrilla y se dejan fuera.");
            }
        }

        /// <summary>T on or off at the selected cells, for the whole rack; returns the reason when it was not written.</summary>
        public string SetTope(bool on) => SetSafetyCells(() => SelectiveSafetyView.WriteTope(_editor, SelectedPositions, on));

        /// <summary>P on or off at the selected cells, for the whole rack; returns the reason when it was not written.</summary>
        public string SetParrilla(bool on) => SetSafetyCells(() => SelectiveSafetyView.WriteParrilla(_editor, SelectedPositions, on));

        /// <summary>«En todas las celdas» (true) / «En ninguna» (false) of the tope card.</summary>
        public string SetTopeEverywhere(bool on) => SetSafetyCells(() => SelectiveSafetyView.WriteAll(_editor, SelectiveSafetyDefaults.TopeType, on));

        /// <summary>«En todas las celdas» (true) / «En ninguna» (false) of the parrilla card.</summary>
        public string SetParrillaEverywhere(bool on) => SetSafetyCells(() => SelectiveSafetyView.WriteAll(_editor, SelectiveSafetyDefaults.ParrillaType, on));

        /// <summary>
        /// «Editar seguridad…»: the classic dialog; its result replaces the safety of the draft, which is recomposed. A value written and
        /// refused in a field stops it first and the editor names the field, as the classic editor does (its <c>Safety_Click</c> starts with
        /// <c>CommitPendingEditors</c>): the recomposition of the result would put the value of the draft back in that box (M-2 of the review
        /// of the entrega 2).
        /// </summary>
        public void EditSafety()
        {
            if (!CanEditSafety)
            {
                Say(_safetyEditor == null ? NoDialogText : BlockedEditText, error: true);
                return;
            }

            var pending = PendingFieldError("No se puede abrir «Editar seguridad…»");
            if (pending != null)
            {
                Say(pending, error: true);
                return;
            }

            var result = _safetyEditor(_editor);
            if (result == null) return;
            Apply(() =>
            {
                _editor.ReplaceSafety(result);
                return Done;
            });
            OnSafetyChanged();
        }

        // ------------------------------------------------------------------ internals

        // The marks T and P of the matrix and of the baseline of «Modificada en el borrador», read once per refresh of the matrix.
        private SelectiveSafetyMarks SafetyMarks() => SelectiveSafetyView.Marks(_editor);

        // What a screen reader says of T and P of a cell (the approved draft: «con tope», «con parrilla»).
        private static IEnumerable<string> SafetyStates(SelectiveSafetyMark tope, SelectiveSafetyMark parrilla)
        {
            if (tope == SelectiveSafetyMark.On) yield return "con tope";
            else if (tope == SelectiveSafetyMark.Varies) yield return "el tope varía entre los fondos";
            if (parrilla == SelectiveSafetyMark.On) yield return "con parrilla";
            else if (parrilla == SelectiveSafetyMark.Varies) yield return "la parrilla varía entre los fondos";
        }

        private static bool? Box(SelectiveScopedValue<bool> value)
            => value.Count == 0 ? false : value.Varies ? (bool?)null : value.Value;

        // S-20: a click on «varía» (WPF gives false) puts the family in every cell; a click that changes nothing writes nothing.
        private void WriteBox(bool? shown, bool? value, Func<bool, string> write)
        {
            var next = shown.HasValue ? value : value.HasValue ? true : (bool?)null;
            if (next.HasValue && next != shown) write(next.Value);
            OnSafetyChanged();
        }

        private string BoxLabel(string type, SelectiveScopedValue<bool> value, string label, string varies, string unchosen, string onFloor)
        {
            if (SelectiveSafetyView.Selected(_editor, type) == null) return unchosen;
            if (value.Count == 0) return onFloor;
            return value.Varies ? varies : label;
        }

        private string SetSafetyCells(Func<SelectiveSafetyEdit> write)
        {
            SelectiveSafetyEdit outcome = null;
            var error = Apply(() => (outcome = write()).ToDraftEdit());
            if (error != null) Say(error, error: true);
            else if (outcome?.Notice != null) Say(outcome.Notice, error: false);
            OnSafetyChanged();
            return error;
        }

        private IReadOnlyList<SelectivePostSafetyRow> PostSafetyRows()
        {
            RefreshSafetyCounts();
            return _safetyCounts.Posts;
        }

        // Counted once per system: the table by post, its «Total», «Extremos y fondos» and the pieces of the cards.
        private void RefreshSafetyCounts()
        {
            if (_safetyCounted && ReferenceEquals(_safetyCountsOf, _editor.System)) return;
            _safetyCounts = SelectiveSafetyView.Count(_editor);
            _safetyCountsOf = _editor.System;
            _safetyCounted = true;
            SafetyCountBuilds++;
            _postSafetyItems = _safetyCounts.Posts.Select(SelectivePostSafetyItem.Of).ToList();
            _postSafetyTable = _postSafetyItems.Concat(new[] { SelectivePostSafetyItem.Total(_safetyCounts.Posts, _editor.FondoCount > 1) }).ToList();
            _endLines = BuildEndLines(_safetyCounts.Posts);
        }

        private void RefreshFamilyCards()
        {
            if (_familyCards == null)
            {
                _familyCards = new[]
                {
                    new SelectiveSafetyFamilyCard(SelectiveSafetyDefaults.TopeType, "T", "Topes"),
                    new SelectiveSafetyFamilyCard(SelectiveSafetyDefaults.ParrillaType, "P", "Parrillas"),
                };
            }

            RefreshSafetyCounts();
            var safety = _editor.CopySafety();
            foreach (var card in _familyCards)
            {
                var selection = SelectiveSafetyFamilies.SelectedOfType(safety, _editor.Catalog?.SafetyElements, card.Type);
                if (selection == null)
                {
                    card.Set("Sin variante elegida: no se dibujan.", string.Empty, false, false);
                    continue;
                }

                var (on, total) = SelectiveSafetyView.CellsOn(_editor, card.Type);
                var pieces = card.IsTope ? _safetyCounts.Topes : _safetyCounts.Parrillas;
                var summary = "En " + N(on) + " de " + N(total) + " celdas con larguero · " + Plural(pieces, "pieza", "piezas")
                              + (!card.IsTope && HasFondos ? ", en todos los fondos." : ".");
                var variant = SelectiveSafetyFamilies.VariantsOfType(_editor.Catalog?.SafetyElements, card.Type)
                    .FirstOrDefault(v => string.Equals(v.Id, selection.ElementId, StringComparison.OrdinalIgnoreCase))?.Label ?? selection.ElementId;
                card.Set(summary, card.IsTope ? TopeConfig(selection) : ParrillaConfig(selection), CanEdit && on < total, CanEdit && on > 0, variant);
            }
        }

        // The configuration of the tope card (the approved draft): «Saque 3 in · uno central, compartido entre fondos · en la frontal: no.» T4 seg:
        // the one line of the prototype (famCfg), shared with the inspector; with one fondo it says the side, which places a tope at the near
        // face, the far one or both (SelectiveSafetyPlacement.TopeSpots), where it said «en los postes posteriores» whatever the side was.
        private string TopeConfig(SelectiveSafetySelection selection) => RackCadSafetyPanelViewModel.TopeConfig(selection, _editor.FondoCount);

        // The configuration of the parrilla card (the approved draft): «Se dibujan en la frontal y la lateral · una por tarima, con su ancho · cantidad automática.»
        private static string ParrillaConfig(SelectiveSafetySelection selection)
        {
            var views = new List<string>();
            if (selection.ParrillaFrontal) views.Add("la frontal");
            if (selection.ParrillaLateral) views.Add("la lateral");
            var width = selection.ParrillaFrente > 0.0 ? "de " + Num(selection.ParrillaFrente) + " in de ancho" : "una por tarima, con su ancho";
            var count = selection.ParrillaCantidad > 0 ? N(selection.ParrillaCantidad) + " por nivel" : "cantidad automática";
            return "Se dibujan en " + (views.Count == 0 ? "ninguna vista" : string.Join(" y ", views)) + " · " + width + " · " + count + ".";
        }

        // «Extremos y fondos»: what each family places (from the table, counted as the BOM) and its measures (from the safety of the draft).
        private IReadOnlyList<string> BuildEndLines(IReadOnlyList<SelectivePostSafetyRow> rows)
        {
            var safety = _editor.CopySafety();
            SelectiveSafetySelection Chosen(string type) => SelectiveSafetyFamilies.SelectedOfType(safety, _editor.Catalog?.SafetyElements, type);
            var frames = rows.Where(r => !r.IsIntermediate).ToList();
            var lines = new List<string>();

            if (Chosen(SelectiveSafetyDefaults.LateralType) == null) lines.Add("Protector lateral: sin variante elegida.");
            else
            {
                var guarded = frames.Where(r => r.SideGuards > 0).ToList();
                if (guarded.Count == 0) lines.Add("Protector lateral: elegido, en ningún poste.");
                else if (frames.Count > 1 && guarded.Count == 2 && guarded[0] == frames[0] && guarded[1] == frames[frames.Count - 1])
                    lines.Add("Protector lateral: en los postes de los extremos, " + SelectivePostSafetyItem.PostText(guarded[0]) + " y " + SelectivePostSafetyItem.PostText(guarded[1]) + ".");
                else lines.Add("Protector lateral: en " + string.Join(", ", guarded.Select(SelectivePostSafetyItem.PostText)) + ".");
            }

            if (Chosen(SelectiveSafetyDefaults.BotaType) == null) lines.Add("Botas: sin variante elegida.");
            else
            {
                var booted = frames.Count(r => r.Boots > 0);
                lines.Add("Botas: en " + N(booted) + " de " + Plural(frames.Count, "poste", "postes")
                          + ", el lado de cada uno en «Seguridad por poste»; un poste puede tener la suya.");
            }

            var desviador = Chosen(SelectiveSafetyDefaults.DesviadorType);
            if (desviador == null) lines.Add("Desviadores: sin variante elegida.");
            else
            {
                var plan = SelectiveDesviadorPlan.Build(null, null, desviador); // with no system: only the measures it uses, validated
                lines.Add("Desviadores: longitud " + Num(plan.Longitud) + " in; el primero a " + Num(plan.FirstLevelHeight)
                          + " in sobre el primer troquel; por poste y nivel.");
            }

            var separators = _editor.Separators;
            lines.Add(_editor.FondoCount > 1 && separators.Count > 0
                ? "Separadores entre fondos: " + string.Join(" · ", separators.Select((s, k) => "fondo " + N(k + 1) + "–" + N(k + 2) + ", " + Num(s) + " in"))
                  + ". Se cambian en «Rack» › «Fondos»."
                : "Separadores: no hay, el rack tiene un solo fondo.");
            return lines;
        }

        private static string Plural(int count, string one, string many)
            => count.ToString(CultureInfo.InvariantCulture) + " " + (count == 1 ? one : many);

        private static string N(int value) => value.ToString(CultureInfo.InvariantCulture);

        // The tab «Extremos y seguridad» was shown: what it reads from the BOM is announced (and counted) now, as «Calculado» on Rack.
        private void OnSafetyTabShown() => RaiseSafetyTab();

        private void RaiseSafetyTab()
        {
            if (_familyCards != null) RefreshFamilyCards(); // the cards are refreshed in place, whoever holds them
            OnPropertyChanged(nameof(PostSafety));
            OnPropertyChanged(nameof(PostSafetyTotal));
            OnPropertyChanged(nameof(PostSafetyTable));
            OnPropertyChanged(nameof(SafetySummary));
            OnPropertyChanged(nameof(EndLines));
            OnPropertyChanged(nameof(FamilyCards));
            RefreshSafetyPanel(); // T4 seg: the editable table and the inspector (SelectiveEditorViewModel.Seg.cs)
        }

        // What can be edited changed (AutoCAD writes, or stopped writing): the buttons and boxes of the safety follow.
        private void RaiseSafetyState()
        {
            OnPropertyChanged(nameof(CanEditSafety));
            OnPropertyChanged(nameof(SafetyEditText));
            OnPropertyChanged(nameof(CanEditSelectionTope));
            OnPropertyChanged(nameof(CanEditSelectionParrilla));
            if (_selectedTab == SafetyTabIndex && _familyCards != null) RefreshFamilyCards();
            if (_selectedTab == SafetyTabIndex) RefreshSafetyPanel(); // T4 seg: what can be edited, in the table and the inspector
        }

        private void OnSafetyChanged()
        {
            OnPropertyChanged(nameof(SelectionTope));
            OnPropertyChanged(nameof(SelectionParrilla));
            OnPropertyChanged(nameof(SelectionTopeCheck));
            OnPropertyChanged(nameof(SelectionParrillaCheck));
            OnPropertyChanged(nameof(SelectionTopeLabel));
            OnPropertyChanged(nameof(SelectionParrillaLabel));
            OnPropertyChanged(nameof(SafetyNote));
            RaiseSafetyState();

            // What reads the BOM is announced only while its tab is shown (the cards, the table and «Extremos y fondos»): an edit in
            // «Matriz» counts nothing, and showing the tab counts it once for what the draft is then.
            if (_selectedTab == SafetyTabIndex) RaiseSafetyTab();
        }
    }
}
