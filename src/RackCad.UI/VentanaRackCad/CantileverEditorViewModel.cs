using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using RackCad.Application.Persistence;
using RackCad.Application.StructuralSections;
using RackCad.Application.Systems.Cantilever;
using RackCad.Application.VentanaRackCad;
using RackCad.Domain.Systems.Cantilever;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// T5 ct (I-64, fifth batch; prototype «cantilever», CT-01 to CT-12, accepted by the Owner on 2026-11-26): the editor of ONE Cantilever line in
    /// the RackCad window, with the tabs Línea, Brazos (the first one, decision 9 of Dinámico), Componentes, Arriostramiento and Propiedades
    /// (VA-07), over the draft <see cref="CantileverDraftEditor"/> (the recompute authority of the classic editor). Nothing is modal: the four
    /// configurators of the classic editor are the panels of «Componentes» (CT-03), the section is searched in a list inside the page (CT-04),
    /// the questions are inline, and the sequence of the bracing is a table that is a draft until «Aplicar secuencia» (CT-10).
    ///
    /// One way to confirm: what is written in a field goes to the draft when the field is left (or with Enter); a value the field refuses
    /// stays in it with its reason and the draft does not change. One scope: the selection of the matrix (CT-06: «Todo» writes the exception in
    /// every cell; the default arm is written in «Componentes» or in «Lo esencial»). A line that does not resolve yet says what it lacks
    /// («Para que la línea se resuelva», with «Ir a …», CT-05) and is not written. Nothing here writes the drawing: «Actualizar» and the
    /// insertions hand the editor to the window, and while AutoCAD writes nothing can be edited.
    /// </summary>
    public sealed partial class CantileverEditorViewModel : ObservableObject
    {
        public const int LineTab = 0;
        public const int ArmsTab = 1;
        public const int ComponentsTab = 2;
        public const int BracingTab = 3;

        private static readonly (string Value, string Label)[] FaceOptions = { ("Single", "Sencilla"), ("Double", "Doble") };
        private static readonly (string Value, string Label)[] SideOptions = { ("PositiveY", "+Y"), ("NegativeY", "−Y") };
        private static readonly (string Value, string Label)[] AutoManual = { ("Automatic", "Automática"), ("Manual", "Manual") };
        private static readonly (string Value, string Label)[] BraceOptions = { ("ColdRolledRound", "Varilla (cold rolled)"), ("StructuralSection", "Perfil (estructural)") };

        private readonly CantileverDraftEditor _draft;
        private readonly Action<CantileverEditorViewModel> _update;
        private bool _isNewRack;
        private bool _writing;
        private bool _writeTaken;
        private bool _dirty;
        private int _selectedTab = ArmsTab;
        private string _message = string.Empty;
        private bool _messageIsError;
        private bool _confirmingClose;
        private string _component = CantileverDraftEditor.ComponentColumn;
        private CantileverLineDesign _baseDesign;

        public CantileverEditorViewModel(CantileverDraftEditor draft, string rackId, Action<CantileverEditorViewModel> update, bool isNewRack = false)
        {
            _draft = draft ?? throw new ArgumentNullException(nameof(draft));
            RackId = rackId;
            _update = update;
            _isNewRack = isNewRack;
            ColumnChoices = StructuralSectionSearch.Choices(draft.Catalogue, draft.ColumnFamilies);
            ArmChoices = StructuralSectionSearch.Choices(draft.Catalogue, CantileverDraftEditor.ArmFamilies);
            SeparatorChoices = StructuralSectionSearch.Choices(draft.Catalogue, CantileverDraftEditor.SeparatorFamilies);
            BraceChoices = StructuralSectionSearch.Choices(draft.Catalogue, CantileverDraftEditor.BraceFamilies);
            ComponentTabs = new[]
            {
                new CantileverComponentTab(CantileverDraftEditor.ComponentColumn, "Columna y base"),
                new CantileverComponentTab(CantileverDraftEditor.ComponentArm, "Brazo por omisión"),
                new CantileverComponentTab(CantileverDraftEditor.ComponentSeparator, "Separador"),
                new CantileverComponentTab(CantileverDraftEditor.ComponentBrace, "Tensor"),
            };
            ReadBase();
            SelectFirstCell();
            Rebuild();
        }

        // ------------------------------------------------------------------ identity and state

        public CantileverDraftEditor Draft => _draft;

        public string RackId { get; }

        /// <summary>A new line of the window, not in the drawing yet: «Insertar en el dibujo…» places it (RACKCANTILEVERNUEVO).</summary>
        public bool IsNewRack => _isNewRack;

        public string Name
        {
            get => _draft.Name ?? string.Empty;
            set
            {
                if (_writing) return;
                var next = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
                if (string.Equals(next, _draft.Name, StringComparison.Ordinal)) return;
                var refused = _draft.SetName(next);
                if (refused != null) Say(refused, error: true);
                Rebuild();
                OnPropertyChanged(nameof(Name));
            }
        }

        public bool IsNameReadOnly => _writing;

        public string NamePlaceholder => _isNewRack ? "Automático al insertar (I-60)" : "Sin nombre";

        /// <summary>«Modificado»: the draft (or its tab «Propiedades», or a table of tramos not applied) differs from what was read or written.</summary>
        public bool IsDirty => _dirty;

        public bool IsWriting => _writing;

        public bool CanEdit => !_writing;

        public string StatusText => _writing ? "Escribiendo" : _isNewRack ? (_dirty ? "Nueva · modificada" : "Nueva, sin insertar") : _dirty ? "Modificado" : "Limpio";

        public string DiffText => _writing
            ? "AutoCAD está escribiendo…"
            : _isNewRack ? (_dirty ? "Con cambios desde los valores por defecto." : "Valores por defecto del editor clásico.")
            : _dirty ? (SegmentsPending ? "Cambios sin escribir · secuencia sin aplicar." : "Cambios sin escribir.") : "Sin cambios que escribir.";

        /// <summary>«Cantilever · sencilla, lado +Y · 3 estaciones · 3 niveles»: what the line is, under the name.</summary>
        public string SubLine
        {
            get
            {
                var t = _draft.Design.StationTopology ?? new CantileverLineStationTopologyDesign();
                var faces = t.FaceMode == CantileverStationFaceMode.Double ? "doble cara" : "sencilla, lado " + CantileverDraftEditor.SideLabel(t.SingleSide);
                return "Cantilever · " + faces + " · " + CantileverDraftEditor.Count(_draft.Design.StationCount, "estación", "estaciones") + " · "
                       + CantileverDraftEditor.Count(t.LevelCount, "nivel", "niveles")
                       + (_isNewRack ? " · nueva, todavía sin insertar; si no le pones nombre, toma el automático al insertarla (I-60)" : string.Empty);
            }
        }

        /// <summary>The line of the status band: the name, the state and what the line is.</summary>
        public string StatusLine
        {
            get
            {
                var parts = new List<string> { CantileverDraftEditor.Count(_draft.Design.StationCount, "estación", "estaciones") };
                var line = _draft.IsResolved ? _draft.Computation.Line : null;
                parts.Add(line == null ? "sin resolver" : "altura " + CantileverDraftEditor.Num(line.ColumnHeight) + " in");
                if (line != null)
                {
                    parts.Add(CantileverDraftEditor.Count(line.Separators.Count, "separador", "separadores"));
                    parts.Add(CantileverDraftEditor.Count(line.Braces.Count, "tensor", "tensores"));
                }

                return string.Join(" · ", parts);
            }
        }

        public string Message => _message;

        public bool MessageIsError => _messageIsError;

        public string WritingText => _writeTaken
            ? "AutoCAD está escribiendo la línea (por ejemplo, espera a que coloques las vistas). Termina o cancela la orden en AutoCAD: mientras tanto el editor no se puede editar."
            : "AutoCAD está escribiendo la línea: mientras tanto el editor no se puede editar.";

        public int SelectedTab
        {
            get => _selectedTab;
            set => SetProperty(ref _selectedTab, value);
        }

        public void Say(string text, bool error)
        {
            _message = text ?? string.Empty;
            _messageIsError = error && _message.Length > 0;
            OnPropertyChanged(nameof(Message));
            OnPropertyChanged(nameof(MessageIsError));
        }

        // ------------------------------------------------------------------ «Para que la línea se resuelva» (CT-05)

        public ObservableCollection<CantileverMissingItem> MissingItems { get; } = new();

        public bool HasMissing => MissingItems.Count > 0;

        public string MissingTitle => "Para que la línea se resuelva (" + CantileverDraftEditor.Count(MissingItems.Count, "cosa", "cosas") + ")";

        /// <summary>«Ir a …»: the tab (and the panel of «Componentes») where the missing value is written.</summary>
        public void GoTo(CantileverMissingItem item)
        {
            if (item == null) return;
            switch (item.Tab)
            {
                case CantileverDraftEditor.TabArms: SelectedTab = ArmsTab; break;
                case CantileverDraftEditor.TabComponents:
                    SelectedTab = ComponentsTab;
                    if (item.Component != null) SelectComponent(item.Component);
                    break;
                case CantileverDraftEditor.TabBracing: SelectedTab = BracingTab; break;
                default:
                    SelectedTab = LineTab;
                    if (item.Component == CantileverDraftEditor.ComponentLevels) LevelsOpen = true;
                    break;
            }
        }

        private bool _levelsOpen;

        /// <summary>The group «Niveles» of «Línea» is open: «Ir a «Línea»» of the clear opens it.</summary>
        public bool LevelsOpen
        {
            get => _levelsOpen;
            set => SetProperty(ref _levelsOpen, value);
        }

        // ------------------------------------------------------------------ the head: «Actualizar», «Descartar», «Cerrar el editor»

        public string PrimaryLabel => _isNewRack ? "Insertar en el dibujo…" : "Actualizar";

        public bool CanUpdate => string.IsNullOrEmpty(UpdateBlockedText);

        /// <summary>Why «Actualizar» (or «Insertar en el dibujo…» of a new line) is off; empty when it is on.</summary>
        public string UpdateBlockedText
        {
            get
            {
                if (_writing) return WritingText;
                var other = OtherRequestBlockedText;
                if (other.Length > 0) return other;
                if (SegmentsPending) return "Aplica o descarta la secuencia antes de escribir.";
                if (!_draft.IsResolved) return CantileverDraftEditor.Count(MissingItems.Count, "cosa falta", "cosas faltan") + " para que la línea se resuelva.";
                if (PropertiesBlockUpdate) return PropertiesTab.BlockedText;
                if (_isNewRack) return _insertNew == null ? "Aquí no se puede insertar la línea nueva." : string.Empty;
                if (_update == null) return "Aquí no se puede escribir el dibujo.";
                if (!_dirty) return "Sin cambios que escribir.";
                return string.Empty;
            }
        }

        /// <summary>«Actualizar», or «Insertar en el dibujo…» of a new line (which opens the panel of the views).</summary>
        public void Primary()
        {
            if (_isNewRack)
            {
                InsertView();
                return;
            }

            RequestWrite();
        }

        /// <summary>«Actualizar»: the window hands COPIES of the draft to AutoCAD (RACKCANTILEVERAPLICAR).</summary>
        public void RequestWrite()
        {
            if (!CanUpdate || _isNewRack)
            {
                if (!CanUpdate) Say(UpdateBlockedText, error: true);
                return;
            }

            _update?.Invoke(this);
        }

        public bool CanDiscard => !_writing && (_draft.IsDirty || PropertiesDirty || SegmentsPending);

        /// <summary>«Descartar»: back to what was read or last written (the defaults, or the library, for a new line).</summary>
        public void Discard()
        {
            if (!CanDiscard) return;
            _draft.Discard();
            _propertiesTab?.Reset();
            ResetSegmentRows();
            CloseLayoutQuestions();
            Say(_isNewRack ? "Cambios descartados: la línea nueva vuelve a como se abrió." : "Borrador descartado: el editor vuelve a lo último que se leyó o se escribió.", error: false);
            Rebuild();
        }

        public bool IsConfirmingClose => _confirmingClose;

        public string CloseQuestion => _isNewRack
            ? "La línea nueva tiene cambios y no está en el dibujo. Si cierras el editor, se pierde. ¿Descartarla y cerrar?"
            : "El borrador tiene cambios que no están en el dibujo. Si cierras el editor, se pierden. ¿Descartarlos y cerrar?";

        /// <summary>The window forgets this editor (it asked first when it had changes).</summary>
        public event Action<CantileverEditorViewModel> CloseRequested;

        /// <summary>«Cerrar el editor»: with changes it asks inline first.</summary>
        public void RequestClose()
        {
            if (_writing) return;
            if (_dirty)
            {
                SetConfirmingClose(true);
                return;
            }

            CloseRequested?.Invoke(this);
        }

        public void ConfirmClose()
        {
            if (_writing || !_confirmingClose) return;
            SetConfirmingClose(false);
            CloseRequested?.Invoke(this);
        }

        public void CancelClose() => SetConfirmingClose(false);

        private void SetConfirmingClose(bool value)
        {
            if (_confirmingClose == value) return;
            _confirmingClose = value;
            OnPropertyChanged(nameof(IsConfirmingClose));
        }

        // ------------------------------------------------------------------ what the window tells the editor while AutoCAD writes

        /// <summary>The window handed the write to AutoCAD: nothing can be edited until the answer arrives or the window stops waiting.</summary>
        public void BeginWriting()
        {
            _writing = true;
            _writeTaken = false;
            SetConfirmingClose(false);
            IsMoreOpen = false;
            _propertiesTab?.Lock(true);
            Rebuild();
        }

        /// <summary>The command took the request: its answer will come (the editor no longer says to press «Dejar de esperar»).</summary>
        public void WriteTaken()
        {
            _writeTaken = true;
            RaiseState();
        }

        public void EndWriting()
        {
            _writing = false;
            _writeTaken = false;
            _propertiesTab?.Lock(false);
            Rebuild();
        }

        /// <summary>AutoCAD wrote the draft: what was SENT is the base of «Limpio» now, and «Descartar» goes back to it.</summary>
        public void MarkWritten(string message, string sentJson)
        {
            _draft.MarkWritten(sentJson);
            ReadBase();
            Say(message, error: false);
            Rebuild();
        }

        /// <summary>A new line that AutoCAD placed is a line of the drawing now, with the name the drawing gave it (I-60).</summary>
        public void MarkInserted(string name)
        {
            _isNewRack = false;
            if (!string.IsNullOrWhiteSpace(name) && !string.Equals(name.Trim(), _draft.Name, StringComparison.Ordinal)) _draft.SetName(name);
            _draft.MarkWritten(_draft.DesignJson);
            ReadBase();
            OnPropertyChanged(nameof(IsNewRack));
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(PrimaryLabel));
            OnPropertyChanged(nameof(NamePlaceholder));
            OnPropertyChanged(nameof(ShowInsertView));
            OnPropertyChanged(nameof(CloseQuestion));
            Rebuild();
        }

        /// <summary>«Duplicar» of a draft: the copied design taken as an edit over the base this editor opened with.</summary>
        internal bool TakeDesignAsEdit(CantileverLineDesign design)
        {
            if (!_draft.TakeAsEdit(design)) return false;
            SelectFirstCell();
            Rebuild();
            return true;
        }

        // ------------------------------------------------------------------ tab «Línea»

        public ObservableCollection<object> StructureItems { get; } = new();

        public ObservableCollection<object> FaceItems { get; } = new();

        public ObservableCollection<object> PieceItems { get; } = new();

        public ObservableCollection<CantileverCalcItem> CalcItems { get; } = new();

        public ObservableCollection<object> LevelItems { get; } = new();

        public ObservableCollection<object> DrawingItems { get; } = new();

        public string LevelsSummary =>
            "Troquel del primer nivel: " + _draft.TextOf(CantileverLineField.FirstLevelPunch) + " · claro " + _draft.TextOf(CantileverLineField.ClearHeight)
            + " in · factor superior " + _draft.TextOf(CantileverLineField.TopClearFactor);

        public string DrawingSummary =>
            "En planta: " + (_draft.Design.PlantaVisibility?.ShowArms == true ? "con brazos" : "sin brazos") + " · "
            + (_draft.Design.PlantaVisibility?.ShowBraces == true ? "con tensores" : "sin tensores");

        // ------------------------------------------------------------------ tab «Componentes» (CT-03)

        public IReadOnlyList<CantileverComponentTab> ComponentTabs { get; }

        public string SelectedComponent => _component;

        public bool ShowColumnPanel => _component == CantileverDraftEditor.ComponentColumn;

        public bool ShowArmPanel => _component == CantileverDraftEditor.ComponentArm;

        public bool ShowSeparatorPanel => _component == CantileverDraftEditor.ComponentSeparator;

        public bool ShowBracePanel => _component == CantileverDraftEditor.ComponentBrace;

        public void SelectComponent(string key)
        {
            if (ComponentTabs.All(t => t.Key != key) || _component == key) return;
            _component = key;
            OnPropertyChanged(nameof(SelectedComponent));
            OnPropertyChanged(nameof(ShowColumnPanel));
            OnPropertyChanged(nameof(ShowArmPanel));
            OnPropertyChanged(nameof(ShowSeparatorPanel));
            OnPropertyChanged(nameof(ShowBracePanel));
            Rebuild();
        }

        public ObservableCollection<object> ColumnItems { get; } = new();

        public ObservableCollection<object> BaseItems { get; } = new();

        public ObservableCollection<object> BaseThicknessItems { get; } = new();

        public ObservableCollection<object> PunchItems { get; } = new();

        public ObservableCollection<object> DefaultArmBodyItems { get; } = new();

        public ObservableCollection<object> DefaultArmPlateItems { get; } = new();

        public ObservableCollection<object> DefaultArmEndItems { get; } = new();

        public ObservableCollection<object> SeparatorItems { get; } = new();

        public ObservableCollection<object> BraceItems { get; } = new();

        public ObservableCollection<CantileverRecipeItem> Recipe { get; } = new();

        public ObservableCollection<string> ComponentDiagnostics { get; } = new();

        public string ComponentTitle => ComponentTabs.First(t => t.Key == _component).Label;

        public string ComponentSubtitle
        {
            get
            {
                switch (_component)
                {
                    case CantileverDraftEditor.ComponentArm: return "Hoy: «Brazo (por omisión) › Configurar». El brazo de una celda se edita en el inspector de «Brazos».";
                    case CantileverDraftEditor.ComponentSeparator: return "Hoy: «Separador › Configurar».";
                    case CantileverDraftEditor.ComponentBrace: return "Hoy: «Tensor › Configurar».";
                    default: return "Hoy: «Columna y base › Configurar». La misma en todas las estaciones.";
                }
            }
        }

        public bool ComponentResolves => ComponentDiagnostics.Count == 0;

        public string ComponentOkText => "Se resuelve. Lo que cambias aquí va al borrador al salir del campo; «Descartar» lo deshace.";

        public string ColumnReferenceText => "Sección de columna (referencia): "
                                             + (_draft.Designation(_draft.Design.StationTopology?.ColumnBaseTemplate?.ColumnSectionId) ?? "la línea no tiene columna elegida") + ".";

        /// <summary>
        /// Declared (pregunta 8, «Insertar pieza suelta…» por RACKCANTILEVERPIEZA): not built in this delivery; the loose piece stays in the
        /// configurators of the classic editor (RCT), and this note says so.
        /// </summary>
        public string PieceNote => "«Insertar pieza suelta…» (RACKCANTILEVERPIEZA) llega en otra entrega: mientras tanto, la pieza suelta sigue en los configuradores del editor clásico (RCT).";

        public string SeparatorNote => "El corte no se escribe: se deriva de la distancia entre los agujeros de las dos placas de columna, más 1.25 in por extremo."
                                       + (SeparatorDerived is string derived ? " " + derived : string.Empty);

        public string BraceNote => _draft.Design.Bracing?.BraceKind == CantileverBraceBodyKind.StructuralSection
            ? "Un perfil por tensor, con un agujero de 9/16 in a 1.25 in de cada extremo. Sin valor por omisión: un tensor estructural sin sección se rechaza."
            : "Su longitud de corte se deriva de los adaptadores. No lleva rosca, tuercas ni tolerancias. El adaptador es un ángulo L2×2×3/16.";

        // «Derivado de la línea actual: corte … · n agujeros · elevación …» of the first separator, as the classic configurator says it.
        private string SeparatorDerived
        {
            get
            {
                var first = _draft.IsResolved ? _draft.Computation.Line.Separators.FirstOrDefault() : null;
                if (first == null) return null;
                return "Derivado de la línea actual (primer separador): corte " + CantileverDraftEditor.Num(first.CutLength) + " in · "
                       + CantileverDraftEditor.Count(first.Punches.Count, "agujero", "agujeros") + " · elevación " + CantileverDraftEditor.Num(first.ElevationZ) + " in.";
            }
        }

        // ------------------------------------------------------------------ rebuild

        // Everything the editor shows, refreshed IN PLACE after every write (the box with the focus is never replaced, D-06 of Dinámico).
        private void Rebuild()
        {
            _dirty = _draft.IsDirty || PropertiesDirty || SegmentsPending;
            var ro = _writing;
            var d = _draft.Design;
            var t = d.StationTopology ?? new CantileverLineStationTopologyDesign();
            var dbl = t.FaceMode == CantileverStationFaceMode.Double;
            var manualHeight = (t.ColumnHeight?.Mode ?? CantileverStationColumnHeightMode.Automatic) == CantileverStationColumnHeightMode.Manual;

            Sync(StructureItems, new object[]
            {
                Field(CantileverLineField.StationCount, "Estaciones (columnas)", "", "Mín. 2: con una no hay intervalo, y sin intervalo no hay arriostramiento.", ro),
                Field(CantileverLineField.Spacing, "Separación entre centros", "in", "De columna a columna; no es el largo del separador.", ro),
                Field(CantileverLineField.LevelCount, "Niveles", "", "Solo la cantidad; los mismos en todas las estaciones.", ro),
            });
            Sync(FaceItems, new object[]
            {
                Choice(CantileverLineField.FaceMode, "Caras", FaceOptions, dbl ? "Brazos en los dos lados, con las bases espejadas." : "Brazos en un solo lado.", ro),
                Choice(CantileverLineField.SingleSide, "Lado de los brazos", SideOptions, dbl ? "Con doble cara se usan los dos lados." : "El que lleva la base y los brazos.", ro || dbl,
                    dbl ? string.Empty : null),
                Choice(CantileverLineField.HeightMode, "Altura de columna", AutoManual,
                    manualHeight ? "La misma para todas las estaciones." : "La mayor de las mínimas de cada estación.", ro),
                Field(CantileverLineField.ManualHeight, "Altura manual", "in",
                    manualHeight ? "Bajo la mínima, la línea no se resuelve." : "Solo con altura manual; la que había se conserva.", ro || !manualHeight),
            });
            Sync(PieceItems, new object[]
            {
                Picker("ess.col", "Sección de columna", ColumnChoices, CantileverLineField.ColumnSection,
                    t.ColumnBaseTemplate?.BaseFollowsColumn != false ? "La base sigue a la columna." : "Base elegida a mano en «Componentes».", ro),
                Field(CantileverLineField.BaseLength, "Longitud de base", "in", "No hay valor por omisión aprobado.", ro, "ess.baseLen"),
                ArmPicker("ess.arm", "Brazo por omisión: sección", null, "Las celdas con excepción conservan la suya.", ro),
                ArmField("ess.cut", "Brazo por omisión: longitud de corte", "in", null, CantileverArmField.CutLength, "Sin placas: el largo que se pide.", ro),
            });
            SyncCalc();
            Sync(LevelItems, new object[]
            {
                Field(CantileverLineField.FirstLevelPunch, "Troquel del primer nivel", "", "Contado desde 1 (el clásico lo cuenta desde 0). Su cota en pulgadas, en otra entrega.", ro),
                Field(CantileverLineField.ClearHeight, "Claro solicitado entre niveles", "in", "Uno para toda la línea (CT-12).", ro),
                Field(CantileverLineField.TopClearFactor, "Factor de claro superior", "", "Parte del claro sobre el último nivel. Mín. 1/3.", ro),
            });
            Sync(DrawingItems, new object[]
            {
                Check(CantileverLineField.PlantaArms, "Brazos en la planta", ro),
                Check(CantileverLineField.PlantaBraces, "Tensores en la planta", ro),
            });

            RebuildComponents(ro);
            RebuildBracing(ro);
            RebuildArms(ro);

            var missing = _draft.Missing.Select(m => new CantileverMissingItem(m.Message, "Ir a " + Where(m), m.Tab, m.Component, "Ir a " + Where(m) + ": " + m.Message)).ToList();
            SyncRecords(MissingItems, missing);
            foreach (var tab in ComponentTabs) tab.HasError = missing.Any(m => m.Component == tab.Key);
            RaiseState();
        }

        private void RebuildComponents(bool ro)
        {
            var d = _draft.Design;
            var cb = d.StationTopology?.ColumnBaseTemplate ?? new CantileverStationColumnBaseTemplateDesign();
            var follows = cb.BaseFollowsColumn;
            Sync(ColumnItems, new object[]
            {
                Picker("col.section", "Sección de columna", ColumnChoices, CantileverLineField.ColumnSection, "Familia W.", ro),
                Field(CantileverLineField.ColumnBottomPlate, "Espesor de placa inferior", "in", null, ro),
            });
            Sync(BaseItems, new object[]
            {
                Check(CantileverLineField.BaseFollowsColumn, "La base sigue a la columna", ro,
                    follows ? "Usa la sección de la columna. Elegir otra la deja a mano." : "Marcarla aplica la sección de la columna."),
                Picker("col.base", "Sección de base", ColumnChoices, CantileverLineField.BaseSection, follows ? "Sigue a la columna." : "Elegida a mano.", ro),
                Field(CantileverLineField.BaseLength, "Longitud de base", "in", "No hay valor por omisión aprobado.", ro),
            });
            Sync(BaseThicknessItems, new object[]
            {
                Field(CantileverLineField.BaseFrontPlate, "Placa frontal", "in", null, ro),
                Field(CantileverLineField.BaseRearPlate, "Placa posterior", "in", null, ro),
                Field(CantileverLineField.BaseGusset, "Cartabón", "in", null, ro),
            });
            Sync(PunchItems, new object[]
            {
                Field(CantileverLineField.PunchDiameter, "Diámetro", "in", null, ro),
                Field(CantileverLineField.PunchHorizontalEndOffset, "Margen horizontal a extremo", "in", null, ro),
                Field(CantileverLineField.ConnectionPitch, "Paso de conexión", "in", null, ro),
                Field(CantileverLineField.ConnectionPunchesAboveBase, "Troqueles de conexión sobre la base", "", null, ro),
                Field(CantileverLineField.RearPlateVerticalEndOffset, "Margen vertical de placa posterior", "in", null, ro),
                Field(CantileverLineField.RegularColumnPitch, "Paso regular de columna", "in", null, ro),
                Field(CantileverLineField.ColumnBottomPlatePitch, "Paso de placa inferior", "in", null, ro),
            });

            var end = d.DefaultArmTemplate?.EndPlate?.Mode ?? CantileverArmEndPlateMode.None;
            Sync(DefaultArmBodyItems, new object[]
            {
                ArmPicker("def.section", "Sección del brazo", null, "Un canal doble exige una sección C.", ro),
                ArmField("def.cut", "Longitud de corte", "in", null, CantileverArmField.CutLength, "No incluye placas: es el número que se pide.", ro),
                ArmChoice("def.arr", "Arreglo", null, CantileverArmField.Arrangement, ArrangementOptions, "Los apareados exigen canal (C).", ro),
                ArmField("def.slope", "Pendiente (subida por 12 in)", "in/12", null, CantileverArmField.Slope, "Por omisión, 7/16 (0.4375).", ro),
            });
            Sync(DefaultArmPlateItems, new object[]
            {
                ArmField("def.plate", "Espesor", "in", null, CantileverArmField.PlateThickness, null, ro),
                ArmField("def.punches", "Troqueles verticales", "", null, CantileverArmField.VerticalPunches, "Mín. 2. Al cambiar la sección se vuelve a sugerir.", ro),
                ArmField("def.margin", "Margen vertical", "in", null, CantileverArmField.VerticalMargin, "Por omisión, 2 in.", ro),
            });
            Sync(DefaultArmEndItems, new object[]
            {
                ArmChoice("def.end", "Tipo", null, CantileverArmField.EndPlate, EndOptions, null, ro),
                ArmField("def.endT", "Espesor", "in", null, CantileverArmField.EndPlateThickness,
                    end == CantileverArmEndPlateMode.None ? "Sin placa de extremo." : null, ro || end == CantileverArmEndPlateMode.None),
                ArmField("def.stop", "Altura extra de tope", "in", null, CantileverArmField.ExtraStopHeight,
                    end != CantileverArmEndPlateMode.Stop ? "Solo con «Placa con tope»." : null, ro || end != CantileverArmEndPlateMode.Stop),
            });
            Sync(SeparatorItems, new object[]
            {
                Picker("sep.section", "Sección del separador", SeparatorChoices, CantileverLineField.SeparatorSection, "Por omisión, C4X4.5.", ro),
            });
            var cold = (d.Bracing?.BraceKind ?? CantileverBraceBodyKind.ColdRolledRound) == CantileverBraceBodyKind.ColdRolledRound;
            Sync(BraceItems, new object[]
            {
                Choice(CantileverLineField.BraceKind, "Tipo", BraceOptions, null, ro),
                cold
                    ? Field(CantileverLineField.BraceDiameter, "Diámetro de la varilla", "in", null, ro)
                    : Picker("tens.section", "Sección del tensor estructural", BraceChoices, CantileverLineField.BraceSection, "Canal (C) o ángulo (L).", ro),
            });

            foreach (var tab in ComponentTabs) tab.IsSelected = tab.Key == _component;
            SyncRecords(Recipe, RecipeOf(_component).ToList());
            var diagnostics = _draft.Missing.Where(m => m.Component == _component).Select(m => m.Message).ToList();
            if ((_component == CantileverDraftEditor.ComponentSeparator || _component == CantileverDraftEditor.ComponentBrace) && !_draft.IsResolved && diagnostics.Count == 0)
                diagnostics.Add("Su largo lo deriva la línea, y la línea no se resuelve todavía.");
            SyncRecords(ComponentDiagnostics, diagnostics);
        }

        private IEnumerable<CantileverRecipeItem> RecipeOf(string component)
        {
            var d = _draft.Design;
            var line = _draft.IsResolved ? _draft.Computation.Line : null;
            var cb = d.StationTopology?.ColumnBaseTemplate ?? new CantileverStationColumnBaseTemplateDesign();
            var matrix = _draft.Matrix;
            var sides = matrix.ActiveSides.Count;
            switch (component)
            {
                case CantileverDraftEditor.ComponentColumn:
                    yield return new CantileverRecipeItem("Columnas", d.StationCount + " × " + (_draft.Designation(cb.ColumnSectionId) ?? "sin sección")
                        + ", con placa inferior de " + CantileverDraftEditor.Num(cb.ColumnBottomPlate?.Thickness ?? 0) + " in");
                    yield return new CantileverRecipeItem("Bases", (d.StationCount * sides) + " × " + (_draft.Designation(cb.Base?.SectionId) ?? "sin sección")
                        + " de " + (cb.Base?.Length > 0 ? CantileverDraftEditor.Num(cb.Base.Length) : "—") + " in"
                        + (sides > 1 ? ", espejadas en el lado −Y" : string.Empty));
                    yield return new CantileverRecipeItem("Placas y cartabones", "los cuenta el BOM («Ver su BOM»).");
                    break;
                case CantileverDraftEditor.ComponentArm:
                    var cells = matrix.Cells.Count;
                    var overrides = matrix.OverrideCount;
                    var arm = d.DefaultArmTemplate ?? new CantileverArmTemplateDesign();
                    yield return new CantileverRecipeItem("Brazos con este brazo", (cells - overrides) + " de " + cells
                        + (overrides > 0 ? "; las " + overrides + " con excepción conservan la suya" : string.Empty));
                    yield return new CantileverRecipeItem("Placa de montaje", "Una por brazo, de " + CantileverDraftEditor.Num(arm.MountingPlate?.Thickness ?? 0) + " in, con "
                        + CantileverDraftEditor.Count(arm.MountingPlate?.VerticalPunchCount ?? 0, "troquel vertical", "troqueles verticales"));
                    yield return new CantileverRecipeItem("Placa de extremo", EndLabel(arm.EndPlate?.Mode ?? CantileverArmEndPlateMode.None));
                    break;
                case CantileverDraftEditor.ComponentSeparator:
                    var intervals = Math.Max(1, d.StationCount - 1);
                    yield return new CantileverRecipeItem("Por intervalo", line == null ? "salen de la secuencia; la línea no se resuelve todavía"
                        : CantileverDraftEditor.Count(line.Separators.Count / intervals, "separador", "separadores"));
                    yield return new CantileverRecipeItem("En la línea", line == null ? "—"
                        : CantileverDraftEditor.Count(line.Separators.Count, "separador", "separadores") + " " + (_draft.Designation(d.Bracing?.SeparatorSectionId) ?? string.Empty));
                    break;
                default:
                    var perInterval = Math.Max(1, d.StationCount - 1);
                    var cold = (d.Bracing?.BraceKind ?? CantileverBraceBodyKind.ColdRolledRound) == CantileverBraceBodyKind.ColdRolledRound;
                    yield return new CantileverRecipeItem("Por intervalo", line == null ? "salen de la secuencia; la línea no se resuelve todavía"
                        : CantileverDraftEditor.Count(line.Braces.Count / perInterval, "tensor", "tensores") + " (dos por panel arriostrado)");
                    yield return new CantileverRecipeItem("Cada tensor", cold
                        ? "Una varilla, 2 adaptadores y 2 cartabones calibre 10 por adaptador."
                        : "Un perfil, con un agujero de 9/16 in a 1.25 in de cada extremo.");
                    break;
            }
        }

        private void SyncCalc()
        {
            var line = _draft.IsResolved ? _draft.Computation.Line : null;
            var matrix = _draft.Matrix;
            var overrides = matrix.OverrideCount;
            var manual = _draft.Design.StationTopology?.ColumnHeight?.Mode == CantileverStationColumnHeightMode.Manual;
            var intervals = Math.Max(0, _draft.Design.StationCount - 1);
            var perInterval = line?.Intervals.FirstOrDefault()?.BracedPanels.Count;
            var calc = new List<CantileverCalcItem>
            {
                new("Altura común", line == null ? "—" : CantileverDraftEditor.Num(line.ColumnHeight) + " in",
                    manual ? "Manual, para todas las estaciones." : "La calcula la línea: la mayor de las mínimas."),
                new("Intervalos", CantileverDraftEditor.Int(intervals), "Entre estaciones vecinas."),
                new("Paneles arriostrados por intervalo", perInterval.HasValue ? CantileverDraftEditor.Int(perInterval.Value) : "—",
                    _draft.IsAdvanced ? "Tramos con tensores (avanzada)." : _draft.Design.Bracing?.PanelCountMode == CantileverBracedPanelCountMode.Manual
                        ? "Cantidad manual." : "Regla: máx(1, techo((H − 72) / 60))."),
                new("Separadores", line == null ? "—" : CantileverDraftEditor.Int(line.Separators.Count), "En toda la línea."),
                new("Tensores", line == null ? "—" : CantileverDraftEditor.Int(line.Braces.Count), "Dos por panel arriostrado (la X)."),
                new("Brazos", CantileverDraftEditor.Int(matrix.Cells.Count),
                    overrides > 0 ? CantileverDraftEditor.Count(overrides, "con excepción", "con excepción") + "." : "Todos con el brazo por omisión."),
            };
            SyncRecords(CalcItems, calc);
        }

        private void RaiseState()
        {
            foreach (var name in new[]
                     {
                         nameof(IsDirty), nameof(IsWriting), nameof(CanEdit), nameof(StatusText), nameof(DiffText), nameof(SubLine), nameof(StatusLine),
                         nameof(WritingText), nameof(CanUpdate), nameof(UpdateBlockedText), nameof(CanDiscard), nameof(IsNameReadOnly), nameof(HasMissing),
                         nameof(MissingTitle), nameof(LevelsSummary), nameof(DrawingSummary), nameof(ComponentTitle), nameof(ComponentSubtitle),
                         nameof(ComponentResolves), nameof(ColumnReferenceText), nameof(SeparatorNote), nameof(BraceNote), nameof(CanRestore),
                     })
                OnPropertyChanged(name);
            RaiseArms();
            RaiseBracing();
            RaiseWindowState();
        }

        // ------------------------------------------------------------------ the items of the fields

        private static readonly (string Value, string Label)[] ArrangementOptions =
            { ("Single", "Sencillo"), ("DoubleChannelFacing", "Canal doble enfrentada"), ("DoubleChannelBackToBack", "Canal doble espalda con espalda") };

        private static readonly (string Value, string Label)[] EndOptions = { ("None", "Ninguna"), ("Cap", "Placa"), ("Stop", "Placa con tope") };

        private static string EndLabel(CantileverArmEndPlateMode mode) =>
            mode == CantileverArmEndPlateMode.Stop ? "Placa con tope" : mode == CantileverArmEndPlateMode.Cap ? "Placa" : "Ninguna";

        internal IReadOnlyList<StructuralSectionChoice> ColumnChoices { get; }

        internal IReadOnlyList<StructuralSectionChoice> ArmChoices { get; }

        internal IReadOnlyList<StructuralSectionChoice> SeparatorChoices { get; }

        internal IReadOnlyList<StructuralSectionChoice> BraceChoices { get; }

        private DynamicFieldItem Field(CantileverLineField field, string label, string unit, string hint, bool readOnly, string key = null) =>
            new DynamicFieldItem(key ?? field.ToString(), label, unit, _draft.TextOf(field), hint, text => Commit(() => _draft.Write(field, text)), readOnly);

        private CantileverChoiceItem Choice(CantileverLineField field, string label, (string Value, string Label)[] options, string hint, bool readOnly,
            string selected = null) =>
            new CantileverChoiceItem(field.ToString(), label, options, selected ?? _draft.TextOf(field), hint,
                value => Commit(() => _draft.Write(field, value)), readOnly);

        private CabeceraCheckItem Check(CantileverLineField field, string label, bool readOnly, string hint = null) =>
            new CabeceraCheckItem(field.ToString(), hint == null ? label : label, string.Equals(_draft.TextOf(field), bool.TrueString, StringComparison.Ordinal),
                value => Commit(() => _draft.Write(field, value ? bool.TrueString : bool.FalseString)), readOnly);

        private CantileverSectionPickerItem Picker(string key, string label, IReadOnlyList<StructuralSectionChoice> choices, CantileverLineField field, string hint, bool readOnly)
        {
            var id = _draft.TextOf(field);
            return new CantileverSectionPickerItem(key, label, choices, id, _draft.Designation(id), hint, value => Commit(() => _draft.Write(field, value)), readOnly);
        }

        // An arm field: of the default arm (cells null) or of the arms in force in the selection.
        private DynamicFieldItem ArmField(string key, string label, string unit, IReadOnlyCollection<CantileverLineCell> cells, CantileverArmField field, string hint, bool readOnly)
        {
            var text = _draft.ArmText(cells, field);
            return new DynamicFieldItem(key, label, unit, text.Text, text.Varies ? "Varía" : hint, value => Commit(() => _draft.WriteArm(cells, field, value)), readOnly);
        }

        private CantileverChoiceItem ArmChoice(string key, string label, IReadOnlyCollection<CantileverLineCell> cells, CantileverArmField field,
            (string Value, string Label)[] options, string hint, bool readOnly)
        {
            var text = _draft.ArmText(cells, field);
            return new CantileverChoiceItem(key, label, options, text.Varies ? string.Empty : text.Text, text.Varies ? "Varía; elegir aquí lo iguala." : hint,
                value => Commit(() => _draft.WriteArm(cells, field, value)), readOnly);
        }

        private CantileverSectionPickerItem ArmPicker(string key, string label, IReadOnlyCollection<CantileverLineCell> cells, string hint, bool readOnly)
        {
            var text = _draft.ArmText(cells, CantileverArmField.Section);
            return new CantileverSectionPickerItem(key, label, ArmChoices, text.Varies ? string.Empty : text.Text,
                text.Varies ? "Varía" : _draft.Designation(text.Text), text.Varies ? "Varía: elegir aquí la iguala." : hint,
                value => Commit(() => _draft.WriteArm(cells, CantileverArmField.Section, value)), readOnly);
        }

        // The write of a field: refused (its reason stays in the field), or written and the editor rebuilt. Nothing is written while AutoCAD writes.
        private string Commit(Func<string> write)
        {
            if (_writing) return WritingText;
            var refused = write();
            if (refused != null) return refused;
            if (!string.IsNullOrEmpty(_draft.Notice)) Say(_draft.Notice, error: false);
            else if (_messageIsError) Say(string.Empty, error: false);
            Rebuild();
            return null;
        }

        private static string Where(CantileverMissing missing)
        {
            switch (missing.Tab)
            {
                case CantileverDraftEditor.TabArms: return "«Brazos»";
                case CantileverDraftEditor.TabBracing: return "«Arriostramiento»";
                case CantileverDraftEditor.TabLine when missing.Component == CantileverDraftEditor.ComponentLevels: return "«Línea» › «Niveles»";
                case CantileverDraftEditor.TabComponents:
                    var component = missing.Component switch
                    {
                        CantileverDraftEditor.ComponentArm => "Brazo por omisión",
                        CantileverDraftEditor.ComponentSeparator => "Separador",
                        CantileverDraftEditor.ComponentBrace => "Tensor",
                        _ => "Columna y base",
                    };
                    return "«Componentes» › «" + component + "»";
                default: return "«Línea»";
            }
        }

        // The base of the draft as a design, to mark the cells whose arm changed in the draft.
        private void ReadBase() => _baseDesign = _draft.BaselineDesign;

        // ------------------------------------------------------------------ in-place refresh of the lists

        // The items keep their identity by key: an item with the same key and type is refreshed in place; otherwise the list is replaced.
        private static void Sync(ObservableCollection<object> target, IReadOnlyList<object> next)
        {
            var same = target.Count == next.Count && target.Zip(next, (a, b) => a.GetType() == b.GetType() && KeyOf(a) == KeyOf(b)).All(x => x);
            if (!same)
            {
                target.Clear();
                foreach (var item in next) target.Add(item);
                return;
            }

            for (var i = 0; i < next.Count; i++)
            {
                switch (target[i])
                {
                    case DynamicFieldItem field: field.Refresh((DynamicFieldItem)next[i]); break;
                    case CantileverChoiceItem choice: choice.Refresh((CantileverChoiceItem)next[i]); break;
                    case CabeceraCheckItem check: check.Refresh((CabeceraCheckItem)next[i]); break;
                    case CantileverSectionPickerItem picker: picker.Refresh((CantileverSectionPickerItem)next[i]); break;
                }
            }
        }

        private static string KeyOf(object item) => item switch
        {
            DynamicFieldItem field => field.Key,
            CantileverChoiceItem choice => choice.Key,
            CabeceraCheckItem check => check.Key,
            CantileverSectionPickerItem picker => picker.Key,
            _ => string.Empty,
        };

        // Values (records, strings): replaced only when they differ.
        private static void SyncRecords<T>(ObservableCollection<T> target, IReadOnlyList<T> next)
        {
            if (target.Count == next.Count && target.SequenceEqual(next)) return;
            target.Clear();
            foreach (var item in next) target.Add(item);
        }
    }
}
