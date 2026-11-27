using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.StructuralSections;
using RackCad.Application.Systems.Larguero;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>One entry of a filter or of a list to choose from: what the view-model keeps (<see cref="Key"/>) and what the page says.</summary>
    /// <remarks>Immutable; it says <see cref="INotifyPropertyChanged"/> (never raised) so WPF binds to it without the descriptor of the
    /// property, which would keep every row of a list that is made again on each keystroke.</remarks>
    public sealed record LargueroChoiceOption(string Key, string Label) : INotifyPropertyChanged
    {
        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged { add { } remove { } }
    }

    /// <summary>
    /// One row of the list of sections of «Nuevo › Larguero»: the header of a type («Escalón · 6») or one section. The rows of a type follow its
    /// header (LG-12 of the prototype, accepted: grouped, in natural order). A header cannot be chosen.
    /// </summary>
    public sealed class LargueroSectionRow : INotifyPropertyChanged
    {
        // Immutable: never raised. Saying it keeps WPF off the descriptor of the property (the list is made again on each keystroke).
        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged { add { } remove { } }

        internal LargueroSectionRow(string header, int count)
        {
            IsHeader = true;
            Label = header;
            Detail = count == 1 ? "1 sección" : count.ToString(CultureInfo.InvariantCulture) + " secciones";
            AccessibleName = "Tipo " + header + ": " + Detail;
        }

        internal LargueroSectionRow(LargueroSectionOption option, string detail, int position, int total)
        {
            Option = option;
            Label = option.Name;
            Detail = option.CatalogLabel + (string.IsNullOrEmpty(detail) ? string.Empty : " · " + detail);
            AccessibleName = "Sección " + position.ToString(CultureInfo.InvariantCulture) + " de " + total.ToString(CultureInfo.InvariantCulture) + ": "
                             + option.Name + ", " + option.TypeLabel + ", " + option.CatalogLabel;
        }

        public bool IsHeader { get; }

        public bool IsSection => !IsHeader;

        public LargueroSectionOption Option { get; }

        public string Label { get; }

        public string Detail { get; }

        /// <summary>The name of UI Automation: unique, with its position among the sections of the list.</summary>
        public string AccessibleName { get; }
    }

    /// <summary>
    /// The page «Nuevo › Larguero» of the RackCad window (I-64 A-14, line D, quinta tanda, part «lg»): a DRAFT IN CONSTRUCTION (LG-03 of the
    /// Owner: «Déjalo WIP»). It makes a larguero from a section and a ménsula (LG-01): the section is chosen in ONE page with a filter by
    /// catalog and by type (LG-02), from the largueros of RackCad (secciones.csv) and the AISC sections, and is called by its type; then the
    /// peralte, the length to cut and the ménsula, and the summary with its bill of materials. It reads only the catalogs: nothing is saved,
    /// nothing is written to the drawing and no command leaves it, apart from «Abrir el editor clásico», a gesture of the user that the window
    /// runs as every classic card. The page is of the window, not of a drawing, and its work never vetoes the close of a drawing.
    /// </summary>
    public sealed class LargueroWorkbenchViewModel : ObservableObject
    {
        internal const string AllKey = "";
        internal const string RackCadKey = "rackcad";
        internal const string AiscKey = "aisc";
        internal const string DefaultLengthText = "96";

        /// <summary>What the page is, in its banner (LG-03).</summary>
        public const string WipTitle = "Borrador en construcción";

        public const string WipText =
            "Esta página arma un larguero con una sección y una ménsula, y muestra su resumen. Todavía no lo guarda en la biblioteca ni lo "
            + "dibuja: nada sale de aquí. Para guardar un larguero hoy, usa «Abrir el editor clásico».";

        /// <summary>What is missing to finish the page (for the Owner, and on the page itself).</summary>
        public static IReadOnlyList<string> PendingItems { get; } = new[]
        {
            "«Guardar en la biblioteca…»: el diseño del larguero solo guarda un larguero de RackCad (su id en secciones.csv); una sección AISC "
            + "necesita guardar también su catálogo y su id, un cambio del formato del diseño.",
            "Dibujarlo: no hay un bloque del larguero completo (el perfil y sus 2 ménsulas); falta decidir si se inserta como pieza suelta.",
            "La vista del larguero (el alzado con el peralte y la ménsula a escala) y «Exportar el BOM» (Excel y CSV).",
            "El nombre del larguero, con el automático («Larguero 96 in · P3»), y abrir aquí un larguero de la biblioteca (hoy abre el editor clásico).",
            "Que Selectivo y Dinámico elijan un larguero hecho aquí (otra iniciativa).",
            "Los nombres y los tipos de los largueros de RackCad salen de su nombre en secciones.csv; una columna «tipo» en el catálogo los fijaría.",
        };

        private readonly Func<RackCatalog> _rackCatalog;
        private readonly Func<StructuralSectionCatalog> _aiscCatalog;
        private readonly Action _goHome;
        private readonly Action _openClassic;

        private RackCatalog _catalog = new RackCatalog();
        private LargueroSectionCatalog _sections = LargueroSectionCatalog.Empty;

        private string _catalogFilter = AllKey;
        private string _typeFilter = AllKey;
        private string _search = string.Empty;
        private IReadOnlyList<LargueroChoiceOption> _catalogOptions = Array.Empty<LargueroChoiceOption>();
        private IReadOnlyList<LargueroChoiceOption> _typeOptions = Array.Empty<LargueroChoiceOption>();
        private IReadOnlyList<LargueroSectionRow> _rows = Array.Empty<LargueroSectionRow>();
        private int _shown;

        private LargueroSectionOption _section;
        private string _peralte;
        private string _length = DefaultLengthText;
        private string _mensula;
        private bool _mensulaChosen;
        private IReadOnlyList<LargueroChoiceOption> _mensulaOptions = Array.Empty<LargueroChoiceOption>();
        private LargueroDraftSummary _summary;

        /// <param name="rackCatalog">The catalog of RackCad (the largueros and the ménsulas).</param>
        /// <param name="aiscCatalog">The neutral catalog of structural sections; a failure is said on the page and only RackCad is offered.</param>
        /// <param name="goHome">«Nuevo» of the crumb.</param>
        /// <param name="openClassic">«Abrir el editor clásico»: the classic card «Larguero» (RACKNUEVO Larguero), through the window.</param>
        /// <param name="failed">Says a contained failure of a gesture (the message of the window); without it the failure is rethrown.</param>
        public LargueroWorkbenchViewModel(Func<RackCatalog> rackCatalog, Func<StructuralSectionCatalog> aiscCatalog, Action goHome = null, Action openClassic = null,
            Action<Exception> failed = null)
        {
            _rackCatalog = rackCatalog ?? (() => new RackCatalog());
            _aiscCatalog = aiscCatalog ?? (() => null);
            _goHome = goHome;
            _openClassic = openClassic;
            _failed = failed;
            Apply(Read());
            ResetToDefaults();
        }

        private readonly Action<Exception> _failed;

        /// <summary>Runs a gesture of the view: a failure is said in the message of the window, and the page keeps working.</summary>
        internal void Contained(Action gesture)
        {
            try
            {
                gesture();
            }
            catch (Exception ex) when (_failed != null)
            {
                _failed(ex);
            }
        }

        // ------------------------------------------------------------------ the catalogs

        /// <summary>What could not be read of the catalogs, said at the top of the page (dolor 7: a catalog that fails is never simulated).</summary>
        public string CatalogNotice => string.Join(" ", new[] { _readNotice, _lostNotice }.Where(n => n.Length > 0));

        public bool HasCatalogNotice => CatalogNotice.Length > 0;

        /// <summary>The sections the page offers.</summary>
        public LargueroSectionCatalog Sections => _sections;

        // The instances of the last reading that worked: a reading that fails on a reopening keeps them (the page keeps its work).
        private RackCatalog _lastGoodRack;
        private StructuralSectionCatalog _lastGoodAisc;
        private StructuralSectionCatalog _aisc;
        private string _readNotice = string.Empty;
        private string _lostNotice = string.Empty;

        /// <summary>
        /// Reads the two catalogs again (review of part «lg», point 1: «editar el CSV en Excel y volver a abrir» picks the edit up). The two
        /// providers cache by the signature of their files, so a reading with nothing changed gives the same instances and costs nothing: the
        /// page is rebuilt only when an instance (or what the page says of the catalogs) changed. The section chosen is kept by its catalog and
        /// its id; the filters, the search, the length, the peralte (when the section still admits it) and the ménsula (when it is still in the
        /// catalog) too. A section that is no longer in the catalog is said, and none is chosen in its place.
        /// </summary>
        public void Reload()
        {
            var read = Read();
            if (ReferenceEquals(read.Rack, _catalog) && ReferenceEquals(read.Aisc, _aisc) && read.Notice == _readNotice) return;

            var previous = _section;
            var hadSections = _sections.All.Count > 0;
            Apply(read);
            _lostNotice = string.Empty;
            // With nothing to choose before (catalogs that gave no section), the page starts as a new one: the default section. With nothing
            // chosen among sections that were there (one that left the catalog), none is chosen in silence.
            _section = previous != null ? _sections.Find(previous.Source, previous.Id) : hadSections ? null : _sections.Default;
            if (previous != null && _section == null)
                _lostNotice = "La sección elegida, «" + previous.Name + "», ya no está en el catálogo: elige otra.";
            RefreshTypes(raise: false);
            RefreshRows(raise: false);
            RefreshMensulaOptions(raise: false);
            if (_section == null || !PeralteOptions.Contains(_peralte)) _peralte = PeralteOptions.FirstOrDefault();
            if (_mensula != null && _mensulaOptions.All(o => !string.Equals(o.Key, _mensula, StringComparison.OrdinalIgnoreCase)))
                _mensula = null; // the ménsula chosen is no longer in the catalog: the summary asks for one
            if (_mensula == null && !_mensulaChosen) _mensula = DefaultMensulaOf(_section);
            RaiseCatalogs();
        }

        private (RackCatalog Rack, StructuralSectionCatalog Aisc, string Notice) Read()
        {
            var notices = new List<string>();
            RackCatalog rack;
            var rackRead = true;
            try
            {
                rack = _rackCatalog() ?? new RackCatalog();
                _lastGoodRack = rack;
            }
            catch (Exception ex)
            {
                rackRead = false;
                rack = _lastGoodRack ?? new RackCatalog();
                notices.Add(_lastGoodRack == null
                    ? "El catálogo de RackCad no se pudo leer (" + ex.Message.Trim() + ")."
                    : "El catálogo de RackCad no se pudo volver a leer (" + ex.Message.Trim() + "): la página sigue con la última lectura.");
            }

            StructuralSectionCatalog aisc;
            try
            {
                aisc = _aiscCatalog();
                if (aisc == null) notices.Add("El catálogo de perfiles AISC no está disponible: solo se ofrecen los largueros de RackCad.");
                else _lastGoodAisc = aisc;
            }
            catch (Exception ex)
            {
                aisc = _lastGoodAisc;
                notices.Add(_lastGoodAisc == null
                    ? "El catálogo de perfiles AISC no se pudo leer (" + ex.Message.Trim() + "): solo se ofrecen los largueros de RackCad."
                    : "El catálogo de perfiles AISC no se pudo volver a leer (" + ex.Message.Trim() + "): la página sigue con la última lectura.");
            }

            if (rackRead && rack.BeamProfiles.Count == 0) notices.Add("El catálogo de RackCad no trae largueros (secciones.csv, rol LARGUERO).");
            if (rackRead && rack.Mensulas.Count == 0) notices.Add("El catálogo de RackCad no trae ménsulas (mensulas.csv): no hay ménsula que elegir.");
            return (rack, aisc, string.Join(" ", notices));
        }

        private void Apply((RackCatalog Rack, StructuralSectionCatalog Aisc, string Notice) read)
        {
            _catalog = read.Rack;
            _aisc = read.Aisc;
            _readNotice = read.Notice;
            _sections = LargueroSectionCatalog.Build(_catalog, _aisc);
            _catalogOptions = new[]
            {
                new LargueroChoiceOption(AllKey, "Todos los catálogos (" + Count(_sections.CountOf(null)) + ")"),
                new LargueroChoiceOption(RackCadKey, LargueroSectionNaming.RackCadCatalog + " (" + Count(_sections.CountOf(LargueroSectionSource.RackCad)) + ")"),
                new LargueroChoiceOption(AiscKey, LargueroSectionNaming.AiscCatalog + " (" + Count(_sections.CountOf(LargueroSectionSource.Aisc)) + ")"),
            };
        }

        private void RaiseCatalogs()
        {
            OnPropertyChanged(nameof(CatalogNotice));
            OnPropertyChanged(nameof(HasCatalogNotice));
            OnPropertyChanged(nameof(Sections));
            OnPropertyChanged(nameof(CatalogOptions));
            OnPropertyChanged(nameof(SelectedCatalog));
            OnPropertyChanged(nameof(TypeOptions));
            OnPropertyChanged(nameof(SelectedType));
            RaiseRows();
            RaiseSection();
        }

        // ------------------------------------------------------------------ the filter of the sections (LG-02)

        /// <summary>«Catálogo»: all, the largueros of RackCad, or the AISC sections.</summary>
        public IReadOnlyList<LargueroChoiceOption> CatalogOptions => _catalogOptions;

        public string SelectedCatalog
        {
            get => _catalogFilter;
            set
            {
                // A null comes from the combo while its items change: it is not a choice of the user.
                if (value == null || value == _catalogFilter || _catalogOptions.All(o => o.Key != value)) return;
                _catalogFilter = value;
                OnPropertyChanged(nameof(SelectedCatalog));
                RefreshTypes();
                RefreshRows();
            }
        }

        /// <summary>«Tipo»: every type of the catalog chosen, with its count.</summary>
        public IReadOnlyList<LargueroChoiceOption> TypeOptions => _typeOptions;

        public string SelectedType
        {
            get => _typeFilter;
            set
            {
                if (value == null || value == _typeFilter || _typeOptions.All(o => o.Key != value)) return;
                _typeFilter = value;
                OnPropertyChanged(nameof(SelectedType));
                RefreshRows();
            }
        }

        /// <summary>«Buscar»: the name, the name in the catalog or the id, without case or accents.</summary>
        public string SearchText
        {
            get => _search;
            set
            {
                value ??= string.Empty;
                if (value == _search) return;
                _search = value;
                OnPropertyChanged(nameof(SearchText));
                RefreshRows();
            }
        }

        /// <summary>The list: a header per type, then its sections.</summary>
        public IReadOnlyList<LargueroSectionRow> Rows => _rows;

        public string ShownText => "Mostrando " + Count(_shown) + " de " + Count(_sections.All.Count) + (_sections.All.Count == 1 ? " sección" : " secciones");

        public string NoMatchText => _shown == 0 ? "Ninguna sección coincide con el filtro." : string.Empty;

        /// <summary>The row of the section chosen when the filter shows it; a header or a null from the list is not a choice.</summary>
        public LargueroSectionRow SelectedRow
        {
            get => _section == null ? null : _rows.FirstOrDefault(r => ReferenceEquals(r.Option, _section));
            set
            {
                if (value?.Option == null) return;
                ChooseSection(value.Option);
            }
        }

        /// <summary>The section chosen stays chosen when the filter hides it; the page says it.</summary>
        public string OutsideFilterText =>
            _section != null && SelectedRow == null ? "La sección elegida, «" + _section.Name + "», no coincide con el filtro: sigue elegida." : string.Empty;

        /// <summary>Chooses a section (the tests and the list).</summary>
        public void ChooseSection(LargueroSectionOption section)
        {
            if (section == null || ReferenceEquals(section, _section)) return;
            _section = section;
            // The peralte stays when the new section admits it; else its first one.
            var peraltes = PeralteOptions;
            if (_peralte == null || !peraltes.Contains(_peralte)) _peralte = peraltes.FirstOrDefault();
            // The ménsula follows the section until the user chooses one (LG-01: «sería elegir la ménsula»).
            RefreshMensulaOptions();
            if (!_mensulaChosen) _mensula = DefaultMensulaOf(section);
            RaiseSection();
        }

        // ------------------------------------------------------------------ the peralte, the length and the ménsula

        public LargueroSectionOption Section => _section;

        public bool HasPeralteChoice => _section?.HasPeralteChoice == true;

        public bool ShowFixedPeralte => _section != null && !_section.HasPeralteChoice;

        public IReadOnlyList<string> PeralteOptions =>
            _section == null ? Array.Empty<string>() : _section.Peraltes.Select(LargueroSectionNaming.Number).ToList();

        public string SelectedPeralte
        {
            get => _peralte;
            set
            {
                if (value == null || value == _peralte || !PeralteOptions.Contains(value)) return;
                _peralte = value;
                OnPropertyChanged(nameof(SelectedPeralte));
                RaiseSummary();
            }
        }

        public string PeralteHint
        {
            get
            {
                if (_section == null) return string.Empty;
                if (!_section.HasPeralteChoice && _section.Source == LargueroSectionSource.RackCad)
                    return LargueroDraftSummary.NoCatalogPeraltesText;
                if (!_section.HasPeralteChoice)
                    return _section.Depth.HasValue
                        ? LargueroSectionNaming.Number(_section.Depth.Value) + " in: el peralte de la sección, del catálogo AISC."
                        : "El catálogo no da el peralte de esta sección.";
                return _section.Peraltes.Count == 1
                    ? "Este larguero tiene un solo peralte en el catálogo."
                    : "Los peraltes de este larguero en el catálogo: " + string.Join(", ", PeralteOptions) + " in.";
            }
        }

        /// <summary>«Longitud a corte (in)»: the length of the profile, without the ménsulas.</summary>
        public string LengthText
        {
            get => _length;
            set
            {
                value ??= string.Empty;
                if (value == _length) return;
                _length = value;
                OnPropertyChanged(nameof(LengthText));
                OnPropertyChanged(nameof(LengthError));
                OnPropertyChanged(nameof(HasLengthError));
                RaiseSummary();
            }
        }

        /// <summary>A length that is not a number greater than 0 is marked in its field (dolor 6) and stops the larguero.</summary>
        public string LengthError => Length == null ? "Escribe un número mayor que 0 (pulgadas)." : string.Empty;

        public bool HasLengthError => Length == null;

        private double? Length => UiSupport.TryNum(_length, out var value) && value > 0.0 && !double.IsInfinity(value) ? value : (double?)null;

        /// <summary>«Ménsula»: the ménsulas of the catalog; the one of the larguero of the row is named so.</summary>
        public IReadOnlyList<LargueroChoiceOption> MensulaOptions => _mensulaOptions;

        public string SelectedMensula
        {
            get => _mensula;
            set
            {
                if (value == null || value == _mensula || _mensulaOptions.All(o => o.Key != value)) return;
                _mensula = value;
                _mensulaChosen = true;
                OnPropertyChanged(nameof(SelectedMensula));
                OnPropertyChanged(nameof(MensulaHint));
                RaiseSummary();
            }
        }

        public string MensulaHint
        {
            get
            {
                if (_section == null) return string.Empty;
                if (_mensula == null)
                    return _section.Source == LargueroSectionSource.Aisc
                        ? "Una sección AISC no trae ménsula en el catálogo: elígela."
                        : "Elige la ménsula.";
                var mensula = _catalog.Mensulas.FirstOrDefault(m => string.Equals(m?.Id, _mensula, StringComparison.OrdinalIgnoreCase));
                var text = LargueroDraftSummary.MensulaText(mensula) + ". Dos por larguero.";
                return string.Equals(_mensula, _section.DefaultMensulaId, StringComparison.OrdinalIgnoreCase)
                    ? text + " Es la de su larguero en el catálogo."
                    : text;
            }
        }

        // ------------------------------------------------------------------ the summary

        public LargueroDraftSummary Summary => _summary;

        public string ProposedName => string.IsNullOrEmpty(_summary?.ProposedName) ? "Larguero sin terminar" : _summary.ProposedName;

        public IReadOnlyList<LargueroSummaryLine> SummaryLines => _summary?.Lines ?? Array.Empty<LargueroSummaryLine>();

        public IReadOnlyList<LargueroBomPreviewLine> BomLines => _summary?.Bom ?? Array.Empty<LargueroBomPreviewLine>();

        public bool IsComplete => _summary?.IsComplete == true;

        /// <summary>What is missing, one per line; empty when the larguero is complete.</summary>
        public string MissingText => _summary == null || _summary.IsComplete ? string.Empty : "Falta: " + string.Join(" ", _summary.Missing);

        public string CompleteText => IsComplete
            ? "El larguero está completo. Esta página todavía no lo guarda ni lo dibuja (borrador en construcción)."
            : string.Empty;

        // ------------------------------------------------------------------ the gestures

        /// <summary>«Volver a los valores por defecto»: the ones of the classic editor (the first larguero, peralte 3, 96 in, its ménsula).</summary>
        public void ResetToDefaults()
        {
            _catalogFilter = _sections.CountOf(LargueroSectionSource.RackCad) > 0 ? RackCadKey : AllKey;
            _typeFilter = AllKey;
            _search = string.Empty;
            _length = DefaultLengthText;
            _mensulaChosen = false;
            _lostNotice = string.Empty;
            _section = null;
            _peralte = null;
            _mensula = null;
            RefreshTypes(raise: false);
            RefreshRows(raise: false);
            var first = _sections.Default;
            if (first != null)
            {
                _section = first;
                _peralte = PeralteOptions.FirstOrDefault();
                RefreshMensulaOptions(raise: false);
                _mensula = DefaultMensulaOf(first);
            }
            else
            {
                RefreshMensulaOptions(raise: false);
            }

            OnPropertyChanged(nameof(CatalogNotice));
            OnPropertyChanged(nameof(HasCatalogNotice));
            OnPropertyChanged(nameof(CatalogOptions));
            OnPropertyChanged(nameof(SelectedCatalog));
            OnPropertyChanged(nameof(TypeOptions));
            OnPropertyChanged(nameof(SelectedType));
            OnPropertyChanged(nameof(SearchText));
            OnPropertyChanged(nameof(LengthText));
            OnPropertyChanged(nameof(LengthError));
            OnPropertyChanged(nameof(HasLengthError));
            RaiseRows();
            RaiseSection();
        }

        /// <summary>«Nuevo» of the crumb.</summary>
        public void GoHome() => _goHome?.Invoke();

        /// <summary>«Abrir el editor clásico»: the classic editor of the larguero, the one that saves a larguero today.</summary>
        public void OpenClassicEditor() => _openClassic?.Invoke();

        // ------------------------------------------------------------------ internals

        private LargueroSectionSource? SourceFilter =>
            _catalogFilter == RackCadKey ? LargueroSectionSource.RackCad : _catalogFilter == AiscKey ? LargueroSectionSource.Aisc : (LargueroSectionSource?)null;

        private void RefreshTypes(bool raise = true)
        {
            var types = _sections.TypesOf(SourceFilter);
            var options = new List<LargueroChoiceOption> { new LargueroChoiceOption(AllKey, "Todos los tipos") };
            options.AddRange(types.Select(t => new LargueroChoiceOption(t.Label, t.Label + " (" + Count(t.Count) + ")")));
            _typeOptions = options;
            if (options.All(o => o.Key != _typeFilter)) _typeFilter = AllKey;
            if (!raise) return;
            OnPropertyChanged(nameof(TypeOptions));
            OnPropertyChanged(nameof(SelectedType));
        }

        private void RefreshRows(bool raise = true)
        {
            var shown = _sections.Filter(SourceFilter, _typeFilter, _search);
            var rows = new List<LargueroSectionRow>(shown.Count + 8);
            var position = 0;
            foreach (var group in shown.GroupBy(o => o.TypeLabel))
            {
                var items = group.ToList();
                rows.Add(new LargueroSectionRow(group.Key, items.Count));
                foreach (var option in items) rows.Add(new LargueroSectionRow(option, option.Detail(MensulaLabel), ++position, shown.Count));
            }

            _rows = rows;
            _shown = shown.Count;
            if (raise) RaiseRows();
        }

        private void RaiseRows()
        {
            OnPropertyChanged(nameof(Rows));
            OnPropertyChanged(nameof(ShownText));
            OnPropertyChanged(nameof(NoMatchText));
            OnPropertyChanged(nameof(SelectedRow));
            OnPropertyChanged(nameof(OutsideFilterText));
        }

        private void RefreshMensulaOptions(bool raise = true)
        {
            _mensulaOptions = _catalog.Mensulas
                .Where(m => m != null && !string.IsNullOrWhiteSpace(m.Id))
                .Select(m => new LargueroChoiceOption(m.Id, m.Label
                    + (_section != null && string.Equals(m.Id, _section.DefaultMensulaId, StringComparison.OrdinalIgnoreCase) ? " (la de su larguero)" : string.Empty)))
                .ToList();
            if (raise) OnPropertyChanged(nameof(MensulaOptions));
        }

        // The ménsula of the row of a larguero of RackCad, when the catalog has it; none for an AISC section.
        private string DefaultMensulaOf(LargueroSectionOption section) =>
            section?.DefaultMensulaId != null && _mensulaOptions.Any(o => string.Equals(o.Key, section.DefaultMensulaId, StringComparison.OrdinalIgnoreCase))
                ? _mensulaOptions.First(o => string.Equals(o.Key, section.DefaultMensulaId, StringComparison.OrdinalIgnoreCase)).Key
                : null;

        private string MensulaLabel(string id) =>
            _catalog.Mensulas.FirstOrDefault(m => string.Equals(m?.Id, id, StringComparison.OrdinalIgnoreCase))?.Label ?? id;

        private void RaiseSection()
        {
            OnPropertyChanged(nameof(Section));
            OnPropertyChanged(nameof(SelectedRow));
            OnPropertyChanged(nameof(OutsideFilterText));
            OnPropertyChanged(nameof(HasPeralteChoice));
            OnPropertyChanged(nameof(ShowFixedPeralte));
            OnPropertyChanged(nameof(PeralteOptions));
            OnPropertyChanged(nameof(SelectedPeralte));
            OnPropertyChanged(nameof(PeralteHint));
            OnPropertyChanged(nameof(MensulaOptions));
            OnPropertyChanged(nameof(SelectedMensula));
            OnPropertyChanged(nameof(MensulaHint));
            RaiseSummary();
        }

        private void RaiseSummary()
        {
            double? peralte = double.TryParse(_peralte, NumberStyles.Float, CultureInfo.InvariantCulture, out var p) ? p : (double?)null;
            _summary = LargueroDraftSummary.Of(_section, peralte, Length, _mensula, _catalog);
            OnPropertyChanged(nameof(Summary));
            OnPropertyChanged(nameof(ProposedName));
            OnPropertyChanged(nameof(SummaryLines));
            OnPropertyChanged(nameof(BomLines));
            OnPropertyChanged(nameof(IsComplete));
            OnPropertyChanged(nameof(MissingText));
            OnPropertyChanged(nameof(CompleteText));
        }

        private static string Count(int value) => value.ToString(CultureInfo.InvariantCulture);
    }
}
