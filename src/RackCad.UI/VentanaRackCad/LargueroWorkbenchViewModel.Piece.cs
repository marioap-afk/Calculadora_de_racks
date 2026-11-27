using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.StructuralSections.Geometry;
using RackCad.Application.Systems.Larguero;
using RackCad.Application.VentanaRackCad;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// T6 lg (I-64, sexta tanda): what the page «Larguero» asks of the window. «Insertar en el dibujo…» goes through the numbered door of the
    /// window (RACKLARGUEROINSERTAR, ONE request in flight for the whole window) and «Guardar en la biblioteca…» through the page of the
    /// library, as every editor of the window.
    /// </summary>
    public interface ILargueroWorkbenchHost
    {
        /// <summary>The window has the door that inserts a piece.</summary>
        bool HasInsertDoor { get; }

        /// <summary>There is a drawing to insert into.</summary>
        bool HasDrawing { get; }

        /// <summary>Another request of the window waits for AutoCAD (the door takes one at a time).</summary>
        bool OtherRequestWaiting { get; }

        /// <summary>
        /// Asks AutoCAD to insert the plan (a gesture of the user): null when it was handed over (the page waits for the answer), else why
        /// not. The window tells the page the number of the request BEFORE it hands it over (<see cref="LargueroWorkbenchViewModel.InsertWaiting"/>),
        /// because AutoCAD may answer during the call.
        /// </summary>
        string RequestInsert(LargueroPiecePlan plan);

        /// <summary>«Guardar en la biblioteca…»: the line of the save in the page of the library, with the name of the larguero.</summary>
        void SaveToLibrary(LargueroWorkbenchViewModel workbench);
    }

    /// <summary>
    /// T6 lg: the piece of the page «Larguero» (the Owner, points 2, 3 and 6): what «Insertar en el dibujo…» places — the larguero with its two
    /// ménsulas, or, with an AISC section, only the profile as RACKSECCION draws it, in the view and the detail chosen —, its plan (the one
    /// the view draws and AutoCAD inserts), the sketch of the view, the warning of ADR-0023 for the S family, the name of the larguero with the
    /// automatic one as a suggestion, and the state of the insertion.
    /// </summary>
    public sealed partial class LargueroWorkbenchViewModel
    {
        internal const string LargueroPieceKey = "larguero";
        internal const string ProfilePieceKey = "perfil";

        private readonly ILargueroWorkbenchHost _host;
        private LargueroPieceKind _pieceKind = LargueroPieceKind.Larguero;
        private SectionViewKind _view = SectionViewKind.CrossSection;
        private SectionDetailLevel _detail = SectionDetailLevel.Tabulated;
        private string _name = string.Empty;
        private LargueroPiecePlan _plan;
        private string _planRefusal = string.Empty;
        private LargueroPieceSketch _sketch = LargueroPieceSketch.Of(null);
        private StructuralSectionGeometryFactory _factory;
        private object _factoryFor;

        private long? _insertRequest;
        private bool _insertTaken;
        private string _insertStatus = string.Empty;
        private bool _insertFailed;

        /// <summary>The views of a bare profile, as RACKSECCION offers them.</summary>
        public static IReadOnlyList<LargueroChoiceOption> ViewOptions { get; } = new[]
        {
            new LargueroChoiceOption(nameof(SectionViewKind.CrossSection), "Sección"),
            new LargueroChoiceOption(nameof(SectionViewKind.LongitudinalX), "Longitudinal X"),
            new LargueroChoiceOption(nameof(SectionViewKind.LongitudinalY), "Longitudinal Y"),
            new LargueroChoiceOption(nameof(SectionViewKind.Isometric), "Isométrica"),
        };

        /// <summary>The detail of an AISC section, as RACKSECCION offers it.</summary>
        public static IReadOnlyList<LargueroChoiceOption> DetailOptions { get; } = new[]
        {
            new LargueroChoiceOption(nameof(SectionDetailLevel.Tabulated), "Tabulada"),
            new LargueroChoiceOption(nameof(SectionDetailLevel.Simplified), "Simplificada"),
        };

        // ------------------------------------------------------------------ the name (point 5)

        /// <summary>«Nombre del larguero»: what the user typed; blank takes the automatic one.</summary>
        public string NameText
        {
            get => _name;
            set
            {
                value ??= string.Empty;
                if (value == _name) return;
                _name = value;
                OnPropertyChanged(nameof(NameText));
                RaisePiece();
            }
        }

        /// <summary>The name the larguero takes: the one typed, else the automatic one (empty while it is not complete).</summary>
        public string EffectiveName => _name.Trim().Length > 0 ? _name.Trim() : _summary?.ProposedName ?? string.Empty;

        /// <summary>The title of the summary: the name the larguero takes, or «Larguero sin terminar».</summary>
        public string DisplayName => EffectiveName.Length > 0 ? EffectiveName : "Larguero sin terminar";

        public string NameHint
        {
            get
            {
                var proposed = _summary?.ProposedName ?? string.Empty;
                if (_name.Trim().Length == 0)
                    return proposed.Length > 0 ? "Vacío: se llama «" + proposed + "», el nombre automático." : "Vacío: tomará el nombre automático cuando el larguero esté completo.";
                return proposed.Length > 0 && !string.Equals(proposed, _name.Trim(), StringComparison.Ordinal) ? "El automático sería «" + proposed + "»." : string.Empty;
            }
        }

        /// <summary>«Usar el nombre automático» is offered when there is one and the name typed is another.</summary>
        public bool CanUseProposedName => !string.IsNullOrEmpty(_summary?.ProposedName) && !string.Equals(_summary.ProposedName, _name.Trim(), StringComparison.Ordinal);

        /// <summary>«Usar el nombre automático»: the automatic name in the field.</summary>
        public void UseProposedName()
        {
            if (CanUseProposedName) NameText = _summary.ProposedName;
        }

        // ------------------------------------------------------------------ what is inserted (points 2 and 6)

        /// <summary>«El larguero, con sus 2 ménsulas»: always possible.</summary>
        public bool IsLargueroPiece
        {
            get => PieceKind == LargueroPieceKind.Larguero;
            set
            {
                if (value) ChoosePiece(LargueroPieceKind.Larguero);
            }
        }

        /// <summary>«Solo el perfil (como RACKSECCION)»: only with an AISC section.</summary>
        public bool IsProfilePiece
        {
            get => PieceKind == LargueroPieceKind.Profile;
            set
            {
                if (value) ChoosePiece(LargueroPieceKind.Profile);
            }
        }

        /// <summary>What is inserted: the bare profile only while an AISC section is chosen; otherwise the larguero.</summary>
        public LargueroPieceKind PieceKind => _pieceKind == LargueroPieceKind.Profile && CanChooseProfile ? LargueroPieceKind.Profile : LargueroPieceKind.Larguero;

        /// <summary>The bare profile is offered with an AISC section (the filter «Perfiles AISC» of the one page, point 6).</summary>
        public bool CanChooseProfile => _section?.Source == LargueroSectionSource.Aisc && _section.Aisc != null;

        public string ProfileHint => CanChooseProfile
            ? "Solo el perfil, sin ménsulas, con la geometría y las vistas de RACKSECCION (que sigue en su tarjeta «Perfil estructural»)."
            : "Solo un perfil AISC se inserta sin ménsulas: elige una sección de «Perfiles AISC».";

        public void ChoosePiece(LargueroPieceKind kind)
        {
            if (kind == LargueroPieceKind.Profile && !CanChooseProfile) return;
            if (_pieceKind == kind) return;
            _pieceKind = kind;
            RaisePiece();
        }

        /// <summary>The view of a bare profile (RACKSECCION: «Sección», «Longitudinal X», «Longitudinal Y», «Isométrica»).</summary>
        public string SelectedView
        {
            get => _view.ToString();
            set
            {
                if (value == null || !Enum.TryParse<SectionViewKind>(value, out var view) || ViewOptions.All(o => o.Key != value) || view == _view) return;
                _view = view;
                RaisePiece();
            }
        }

        /// <summary>The detail of an AISC section («Tabulada» or «Simplificada»).</summary>
        public string SelectedDetail
        {
            get => _detail.ToString();
            set
            {
                if (value == null || !Enum.TryParse<SectionDetailLevel>(value, out var detail) || DetailOptions.All(o => o.Key != value) || detail == _detail) return;
                _detail = detail;
                RaisePiece();
            }
        }

        public bool ShowViewChoice => PieceKind == LargueroPieceKind.Profile;

        public bool ShowDetailChoice => CanChooseProfile;

        // ------------------------------------------------------------------ the plan and its view (point 3)

        /// <summary>The plan of the piece (what the view draws and what AutoCAD inserts); null while something is missing.</summary>
        public LargueroPiecePlan Plan => _plan;

        /// <summary>Why there is no plan: what is still missing.</summary>
        public string PlanRefusal => _planRefusal;

        /// <summary>The sketch of the view: the plan to one scale, broken in the middle when it is long.</summary>
        public LargueroPieceSketch Sketch => _sketch;

        public bool HasSketch => _plan != null;

        /// <summary>What the view shows, under it.</summary>
        public string SketchCaption
        {
            get
            {
                if (_plan == null) return string.IsNullOrEmpty(_planRefusal) ? string.Empty : "Sin vista: " + _planRefusal;
                var cut = LargueroSectionNaming.Number(_plan.Length) + " in a corte";
                if (_plan.Kind == LargueroPieceKind.Profile)
                    return "Perfil «" + _plan.Name + "», vista " + (ViewOptions.FirstOrDefault(o => o.Key == _view.ToString())?.Label ?? _view.ToString()).ToLowerInvariant()
                           + ", " + cut + (_sketch.IsBroken ? ": los dos extremos a escala, el centro cortado." : ", a escala.");
                return "Alzado" + (_sketch.IsBroken ? ": los dos extremos a escala, el centro cortado" : " a escala") + ". Perfil de "
                       + LargueroSectionNaming.Number(_plan.Peralte) + " in de peralte y " + cut + "; ménsulas de "
                       + LargueroSectionNaming.Number(_plan.MensulaHeight) + " in de alto, al ras de arriba; el perfil empieza a "
                       + LargueroSectionNaming.Number(_plan.Overhang) + " in de la cara del poste ("
                       + LargueroSectionNaming.Number(_plan.OverallLength) + " in de punta a punta).";
            }
        }

        /// <summary>The warning of ADR-0023 for a geometry with a convention of RackCad (the S family): always, as RACKSECCION shows it.</summary>
        public string AuthorityText => _plan?.IsVisualDerived == true
            ? LargueroDoorRules.VisualDerivedWarning + " Dimensiones, área, peso y propiedades siguen siendo los que tabula AISC."
            : string.Empty;

        public bool HasAuthorityText => AuthorityText.Length > 0;

        // ------------------------------------------------------------------ «Insertar en el dibujo…» (point 2)

        /// <summary>«Insertar en el dibujo…» can be pressed now.</summary>
        public bool CanInsert => InsertBlocker == null;

        /// <summary>Why «Insertar en el dibujo…» is off; empty when it is on.</summary>
        public string InsertReason => InsertBlocker ?? string.Empty;

        private string InsertBlocker
        {
            get
            {
                if (_host == null || !_host.HasInsertDoor) return "Aquí no se puede escribir el dibujo.";
                if (_insertRequest.HasValue) return "Esperando a AutoCAD: indica el punto de inserción en el dibujo o cancela con Esc.";
                if (!_host.HasDrawing) return "Abre o crea un dibujo para insertar el " + (PieceKind == LargueroPieceKind.Profile ? "perfil." : "larguero.");
                if (_host.OtherRequestWaiting) return "La ventana espera a AutoCAD por otra petición: espera a que termine o pulsa «Dejar de esperar».";
                if (_plan == null) return "Falta: " + _planRefusal;
                return null;
            }
        }

        public string InsertAutomationName => PieceKind == LargueroPieceKind.Profile ? "Insertar el perfil en el dibujo" : "Insertar el larguero en el dibujo";

        public string InsertHint => PieceKind == LargueroPieceKind.Profile
            ? "Como RACKSECCION: AutoCAD pide el punto y deja el perfil como un bloque suelto. Un «U» lo deshace."
            : "Una pieza suelta (un bloque, no un rack): AutoCAD pide el punto, en la cara del poste y arriba del perfil. Un «U» lo deshace.";

        /// <summary>A request of the page waits for AutoCAD.</summary>
        public bool IsInserting => _insertRequest.HasValue;

        /// <summary>What the last insertion did, or what it waits for.</summary>
        public string InsertStatus => _insertStatus;

        public bool HasInsertStatus => _insertStatus.Length > 0;

        public bool InsertFailed => _insertFailed;

        /// <summary>«Insertar en el dibujo…»: a gesture of the user; the window hands the plan the page shows to AutoCAD.</summary>
        public void Insert()
        {
            var blocker = InsertBlocker;
            if (blocker != null)
            {
                SetInsertStatus(blocker, failed: true);
                return;
            }

            var refusal = _host.RequestInsert(_plan);
            if (refusal != null)
            {
                _insertRequest = null;
                _insertTaken = false;
                SetInsertStatus(refusal, failed: true);
            }
        }

        /// <summary>The window took the request with that number and is handing it over (before AutoCAD may answer).</summary>
        public void InsertWaiting(long requestId)
        {
            _insertRequest = requestId;
            _insertTaken = false;
            SetInsertStatus("Esperando a que AutoCAD tome la petición n.º " + requestId.ToString(CultureInfo.InvariantCulture) + " (RACKLARGUEROINSERTAR).", failed: false);
        }

        /// <summary>RACKLARGUEROINSERTAR took the request: AutoCAD asks for the point.</summary>
        public void InsertTaken(long requestId)
        {
            if (_insertRequest != requestId) return;
            _insertTaken = true;
            SetInsertStatus("AutoCAD pide el punto de inserción en la línea de órdenes: indícalo en el dibujo (Esc cancela).", failed: false);
        }

        /// <summary>The answer of AutoCAD, only for the request that waits.</summary>
        public void InsertAnswered(LargueroInserted answer)
        {
            if (answer == null || _insertRequest != answer.RequestId) return;
            _insertRequest = null;
            _insertTaken = false;
            SetInsertStatus(answer.Message ?? string.Empty, failed: !answer.Inserted);
        }

        /// <summary>«Dejar de esperar», or the drawing of the request was closed: the page does not wait any more.</summary>
        public void InsertStopped(string text)
        {
            if (!_insertRequest.HasValue) return;
            _insertRequest = null;
            _insertTaken = false;
            SetInsertStatus(text ?? string.Empty, failed: false);
        }

        /// <summary>Whether the command of the request already took it (then its answer will come).</summary>
        internal bool InsertWasTaken => _insertTaken;

        /// <summary>The window changed (a request, the drawing): «Insertar en el dibujo…» follows.</summary>
        public void HostChanged()
        {
            OnPropertyChanged(nameof(CanInsert));
            OnPropertyChanged(nameof(InsertReason));
        }

        private void SetInsertStatus(string text, bool failed)
        {
            _insertStatus = text ?? string.Empty;
            _insertFailed = failed;
            OnPropertyChanged(nameof(InsertStatus));
            OnPropertyChanged(nameof(HasInsertStatus));
            OnPropertyChanged(nameof(InsertFailed));
            OnPropertyChanged(nameof(IsInserting));
            HostChanged();
        }

        // ------------------------------------------------------------------ internals

        // ResetToDefaults: the name, the piece, its view and its detail, the notice of a design of the library.
        private void ResetPiece()
        {
            _name = string.Empty;
            _pieceKind = LargueroPieceKind.Larguero;
            _view = SectionViewKind.CrossSection;
            _detail = SectionDetailLevel.Tabulated;
            _loadedNotice = string.Empty;
            LibrarySource = null; // a new larguero carries no file of the library
            OnPropertyChanged(nameof(NameText));
            OnPropertyChanged(nameof(LoadedNotice));
            OnPropertyChanged(nameof(HasLoadedNotice));
        }

        // RaiseSummary: the plan of the piece and everything that follows it.
        private void RaisePiece()
        {
            var built = BuildPlan();
            _plan = built.Plan;
            _planRefusal = built.Refusal ?? string.Empty;
            _sketch = LargueroPieceSketch.Of(_plan);
            OnPropertyChanged(nameof(EffectiveName));
            OnPropertyChanged(nameof(DisplayName));
            OnPropertyChanged(nameof(NameHint));
            OnPropertyChanged(nameof(CanUseProposedName));
            OnPropertyChanged(nameof(PieceKind));
            OnPropertyChanged(nameof(IsLargueroPiece));
            OnPropertyChanged(nameof(IsProfilePiece));
            OnPropertyChanged(nameof(CanChooseProfile));
            OnPropertyChanged(nameof(ProfileHint));
            OnPropertyChanged(nameof(SelectedView));
            OnPropertyChanged(nameof(SelectedDetail));
            OnPropertyChanged(nameof(ShowViewChoice));
            OnPropertyChanged(nameof(ShowDetailChoice));
            OnPropertyChanged(nameof(Plan));
            OnPropertyChanged(nameof(PlanRefusal));
            OnPropertyChanged(nameof(Sketch));
            OnPropertyChanged(nameof(HasSketch));
            OnPropertyChanged(nameof(SketchCaption));
            OnPropertyChanged(nameof(AuthorityText));
            OnPropertyChanged(nameof(HasAuthorityText));
            OnPropertyChanged(nameof(InsertAutomationName));
            OnPropertyChanged(nameof(InsertHint));
            HostChanged();
            RaiseExport();
        }

        private (LargueroPiecePlan Plan, string Refusal) BuildPlan()
        {
            try
            {
                double? peralte = double.TryParse(_peralte, NumberStyles.Float, CultureInfo.InvariantCulture, out var p) ? p : (double?)null;
                var mensula = string.IsNullOrWhiteSpace(_mensula)
                    ? null
                    : _catalog.Mensulas.FirstOrDefault(m => string.Equals(m?.Id, _mensula, StringComparison.OrdinalIgnoreCase));
                return LargueroPiecePlanner.Build(PieceKind, _section, peralte, Length, mensula, _catalog, Factory, EffectiveName, _view, _detail);
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
            {
                // A section whose geometry cannot be built (its row is inconsistent) is said, never drawn.
                return (null, "La geometría de la sección no se pudo construir (" + ex.Message.Trim() + ").");
            }
        }

        // The geometry of RACKSECCION over the AISC catalog of the last reading (one factory per instance: it caches by section and detail).
        private StructuralSectionGeometryFactory Factory
        {
            get
            {
                if (_aisc == null) return null;
                if (!ReferenceEquals(_factoryFor, _aisc))
                {
                    _factory = new StructuralSectionGeometryFactory(_aisc);
                    _factoryFor = _aisc;
                }

                return _factory;
            }
        }
    }
}
