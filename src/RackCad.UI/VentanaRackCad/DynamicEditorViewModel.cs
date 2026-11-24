using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.VentanaRackCad;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.Shared;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>One editable value of the Dinámico editor: its label, unit and text, the hint when the selection has several values, and its error.</summary>
    public sealed class DynamicFieldItem : ObservableObject
    {
        private readonly Func<string, string> _commit;
        private string _text;
        private string _error = string.Empty;
        private bool _committing;

        public DynamicFieldItem(string key, string label, string unit, string text, string hint, Func<string, string> commit)
        {
            Key = key;
            Label = label;
            Unit = unit ?? string.Empty;
            _text = text ?? string.Empty;
            Hint = hint ?? string.Empty;
            _commit = commit;
        }

        public string Key { get; }

        public string Label { get; }

        public string Unit { get; }

        /// <summary>«Varía» when the values of the selection differ and the box is empty; empty otherwise.</summary>
        public string Hint { get; }

        public string Text
        {
            get => _text;
            set => SetProperty(ref _text, value ?? string.Empty);
        }

        public string Error
        {
            get => _error;
            private set => SetProperty(ref _error, value ?? string.Empty);
        }

        /// <summary>The gesture of the field (Enter or leaving it): the text goes to the draft, or the field says why not.</summary>
        public void Commit()
        {
            if (_committing || _commit == null) return;
            _committing = true;
            try
            {
                Error = _commit(_text) ?? string.Empty;
            }
            finally
            {
                _committing = false;
            }
        }
    }

    /// <summary>One cell of the matrix: front × level, with one datum (the pallet, width × height) and its marks.</summary>
    public sealed record DynamicCellItem(int FrontIndex, int LevelIndex, string Text, bool Exists, bool IsSelected, bool IsPrimary, bool IsBlankFront);

    /// <summary>One row of the matrix: «N3» … «N1», top to bottom.</summary>
    public sealed record DynamicMatrixRow(int LevelIndex, string Label, IReadOnlyList<DynamicCellItem> Cells);

    /// <summary>One column head of the matrix: «F1» …, with its blank mark.</summary>
    public sealed record DynamicColumnHead(int FrontIndex, string Title, bool IsBlank);

    /// <summary>One computed value of the rack.</summary>
    public sealed record DynamicCalcItem(string Label, string Value);

    /// <summary>One module of the sequence, with Spanish labels.</summary>
    public sealed record DynamicModuleItem(string Id, string Kind, string Length, string Start, string Manual);

    /// <summary>
    /// The editor of ONE Dinámico rack in the RackCad window (I-64 A-14, line D): the design the Owner approved («versión despejada»), with
    /// the tabs Rack, Matriz and Avanzado, over the same recomposition as the classic editor (<see cref="DynamicDraftEditor"/>).
    ///
    /// One way to confirm: what the user writes in a field goes to the draft when the field is left (or with Enter) and the rack is
    /// recomposed; there are no «apply» buttons. One scope: the selection of the matrix (a click, Ctrl+click, Mayús+click, a column, a row or
    /// «Todo»). A value that cannot be recomposed is refused in its field and the draft stays as it was. The rack-wide fields of «Lo
    /// esencial» write every cell or front of the rack and say «Varía» when they differ. Nothing here writes the drawing: «Actualizar» hands
    /// the design to the host.
    /// </summary>
    public sealed class DynamicEditorViewModel : ObservableObject
    {
        private const string Varies = "Varía";

        private readonly DynamicDraftEditor _editor;
        private readonly Action<DynamicEditorViewModel> _update;
        private int _selectedTab = 1;
        private bool _dirty;
        private string _message = string.Empty;
        private bool _messageIsError;

        private IReadOnlyList<DynamicFieldItem> _essentials = Array.Empty<DynamicFieldItem>();
        private IReadOnlyList<DynamicFieldItem> _postFields = Array.Empty<DynamicFieldItem>();
        private IReadOnlyList<DynamicFieldItem> _cellFields = Array.Empty<DynamicFieldItem>();
        private IReadOnlyList<DynamicFieldItem> _frontFields = Array.Empty<DynamicFieldItem>();
        private IReadOnlyList<DynamicFieldItem> _advancedFields = Array.Empty<DynamicFieldItem>();
        private IReadOnlyList<DynamicCalcItem> _calculated = Array.Empty<DynamicCalcItem>();
        private IReadOnlyList<DynamicMatrixRow> _rows = Array.Empty<DynamicMatrixRow>();
        private IReadOnlyList<DynamicColumnHead> _columns = Array.Empty<DynamicColumnHead>();
        private IReadOnlyList<DynamicModuleItem> _modules = Array.Empty<DynamicModuleItem>();

        /// <param name="editor">The draft of the rack.</param>
        /// <param name="rackId">The identity of the rack in the drawing; null for a new rack.</param>
        /// <param name="update">What «Actualizar» does with this editor (the host writes the drawing); null when it is not offered.</param>
        public DynamicEditorViewModel(DynamicDraftEditor editor, string rackId, Action<DynamicEditorViewModel> update = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            RackId = rackId;
            _update = update;
            Rebuild();
        }

        // ------------------------------------------------------------------ what the editor shows

        public DynamicDraftEditor Draft => _editor;

        public string RackId { get; }

        public string Name
        {
            get => _editor.Name ?? string.Empty;
            set
            {
                var next = value ?? string.Empty;
                if (string.Equals(_editor.Name ?? string.Empty, next, StringComparison.Ordinal)) return;
                _editor.Name = next;
                if (_editor.System != null) _editor.System.Name = next.Trim();
                MarkDirty();
                OnPropertyChanged(nameof(Name));
            }
        }

        /// <summary>0 = Rack, 1 = Matriz (the first tab, as the Owner decided), 2 = Avanzado.</summary>
        public int SelectedTab
        {
            get => _selectedTab;
            set => SetProperty(ref _selectedTab, value);
        }

        public bool IsDirty => _dirty;

        public string StatusText => _dirty ? "Modificado" : "Limpio";

        public string Message => _message;

        public bool MessageIsError => _messageIsError;

        public bool CanUpdate => _update != null && _dirty && _editor.Design != null && string.IsNullOrEmpty(_editor.LastError);

        public string UpdateBlockedText
        {
            get
            {
                if (_update == null) return "Aquí no se puede escribir el dibujo.";
                if (!_dirty) return "Sin cambios que escribir.";
                if (!string.IsNullOrEmpty(_editor.LastError)) return _editor.LastError;
                return string.Empty;
            }
        }

        public IReadOnlyList<DynamicFieldItem> Essentials => _essentials;

        public IReadOnlyList<DynamicFieldItem> PostFields => _postFields;

        public IReadOnlyList<DynamicCalcItem> Calculated => _calculated;

        public IReadOnlyList<DynamicColumnHead> Columns => _columns;

        public IReadOnlyList<DynamicMatrixRow> Rows => _rows;

        public IReadOnlyList<DynamicFieldItem> CellFields => _cellFields;

        public IReadOnlyList<DynamicFieldItem> FrontFields => _frontFields;

        public IReadOnlyList<DynamicFieldItem> AdvancedFields => _advancedFields;

        public IReadOnlyList<DynamicModuleItem> Modules => _modules;

        public string InspectorTitle
        {
            get
            {
                var cells = _editor.Matrix.SelectedCells();
                if (cells.Count == 1) return "Celda F" + (cells[0].FrontIndex + 1) + " · N" + (cells[0].LevelIndex + 1);
                var fronts = cells.Select(c => c.FrontIndex).Distinct().Count();
                return cells.Count.ToString(CultureInfo.InvariantCulture) + " celdas en " + fronts.ToString(CultureInfo.InvariantCulture)
                    + (fronts == 1 ? " frente" : " frentes");
            }
        }

        public bool NumberFronts
        {
            get => _editor.Annotations.NumberFronts;
            set => SetAnnotation(() => _editor.Annotations.NumberFronts = value);
        }

        public bool NumberLevels
        {
            get => _editor.Annotations.NumberLevels;
            set => SetAnnotation(() => _editor.Annotations.NumberLevels = value);
        }

        public bool DrawRackName
        {
            get => _editor.Annotations.DrawRackName;
            set => SetAnnotation(() => _editor.Annotations.DrawRackName = value);
        }

        public bool DerivedPostReinforced
        {
            get => _editor.DerivedPostReinforced;
            set
            {
                if (_editor.DerivedPostReinforced == value) return;
                Apply(() => _editor.DerivedPostReinforced = value, null);
            }
        }

        // ------------------------------------------------------------------ the selection of the matrix (the one scope)

        /// <summary>A click on a cell: one cell; with Ctrl, add or remove it; with Mayús, the rectangle from the primary cell.</summary>
        public void ClickCell(int frontIndex, int levelIndex, bool control, bool shift)
        {
            var matrix = _editor.Matrix;
            if (frontIndex < 0 || frontIndex >= matrix.Count) return;

            if (shift)
            {
                var fromFront = matrix.SelectedFrontIndex;
                var fromLevel = matrix.SelectedLevelIndex;
                matrix.ToggleCell(fromFront, fromLevel, extendSelection: false);
                for (var f = Math.Min(fromFront, frontIndex); f <= Math.Max(fromFront, frontIndex); f++)
                    for (var l = Math.Min(fromLevel, levelIndex); l <= Math.Max(fromLevel, levelIndex); l++)
                        Add(f, l);
                // The clicked cell becomes the primary one, still selected.
                if (Exists(frontIndex, levelIndex) && matrix.IsSelected(frontIndex, levelIndex)) { matrix.ToggleCell(frontIndex, levelIndex, true); matrix.ToggleCell(frontIndex, levelIndex, true); }
            }
            else
            {
                matrix.ToggleCell(frontIndex, levelIndex, control);
            }

            matrix.NormalizeSelection();
            RebuildSelection();
        }

        /// <summary>«F1»…: every cell of that front.</summary>
        public void SelectFront(int frontIndex)
        {
            var matrix = _editor.Matrix;
            if (frontIndex < 0 || frontIndex >= matrix.Count) return;
            matrix.ToggleCell(frontIndex, 0, extendSelection: false);
            for (var l = 1; l < matrix.Fronts[frontIndex].LoadLevels; l++) Add(frontIndex, l);
            matrix.NormalizeSelection();
            RebuildSelection();
        }

        /// <summary>«N1»…: that level in every front that has it.</summary>
        public void SelectLevel(int levelIndex)
        {
            var matrix = _editor.Matrix;
            var first = true;
            for (var f = 0; f < matrix.Count; f++)
            {
                if (!Exists(f, levelIndex)) continue;
                if (first) { matrix.ToggleCell(f, levelIndex, extendSelection: false); first = false; }
                else Add(f, levelIndex);
            }

            matrix.NormalizeSelection();
            RebuildSelection();
        }

        /// <summary>«Todo»: every cell of the rack.</summary>
        public void SelectAll()
        {
            var matrix = _editor.Matrix;
            matrix.ToggleCell(0, 0, extendSelection: false);
            for (var f = 0; f < matrix.Count; f++)
                for (var l = 0; l < matrix.Fronts[f].LoadLevels; l++)
                    Add(f, l);
            matrix.NormalizeSelection();
            RebuildSelection();
        }

        // ------------------------------------------------------------------ the gestures of the rack

        /// <summary>«Frentes»: the count of fronts; new fronts copy the selected one, as in the classic editor.</summary>
        public string SetFrontCount(string text)
        {
            if (!UiSupport.TryInt(text, out var count) || count < 1) return "La cantidad de frentes debe ser un entero mayor o igual que 1.";
            return Apply(() => _editor.Matrix.SetFrontCount(count), null);
        }

        /// <summary>«En blanco» of a front: it keeps its structure and carries nothing. The last active front cannot be blank.</summary>
        public void SetFrontBlank(int frontIndex, bool blank)
        {
            string refusal = null;
            Apply(() =>
            {
                if (!_editor.Matrix.SetActive(frontIndex, !blank)) refusal = "Al menos un frente debe permanecer activo.";
            }, null);
            if (refusal != null) Say(refusal, error: true);
        }

        /// <summary>«Descartar»: the draft goes back to what was loaded (or to a new rack).</summary>
        public void Discard(DynamicRackDesign original, string name)
        {
            if (original != null) _editor.LoadFrom(original, name);
            else
            {
                var fresh = DynamicDraftEditor.CreateNew(_editor.Catalog);
                _editor.LoadFrom(fresh.Design, name);
            }

            _dirty = false;
            Say("Borrador descartado: se muestra lo que hay en el dibujo.", error: false);
            Rebuild();
        }

        /// <summary>«Restaurar layout»: rebuilds the module sequence from scratch; the window asks first, because custom cabeceras are lost.</summary>
        public void RestoreStandard()
        {
            if (_editor.Recompose(restoreStandard: true))
            {
                MarkDirty();
                Say(string.IsNullOrWhiteSpace(_editor.LastRebuildReport) ? "Layout restaurado." : _editor.LastRebuildReport, error: false);
            }
            else Say(_editor.LastError, error: true);

            Rebuild();
        }

        /// <summary>«Actualizar»: hands this editor to the host, which writes the drawing. Nothing happens when there is nothing to write.</summary>
        public void Update()
        {
            if (!CanUpdate)
            {
                Say(UpdateBlockedText, error: true);
                return;
            }

            _update(this);
        }

        /// <summary>The host wrote the draft: it is the drawing now.</summary>
        public void MarkWritten(string message)
        {
            _dirty = false;
            Say(message, error: false);
            RaiseState();
        }

        /// <summary>What the host or the view needs to say.</summary>
        public void Say(string text, bool error)
        {
            _message = text ?? string.Empty;
            _messageIsError = error && _message.Length > 0;
            OnPropertyChanged(nameof(Message));
            OnPropertyChanged(nameof(MessageIsError));
        }

        // ------------------------------------------------------------------ internals

        private bool Exists(int frontIndex, int levelIndex)
        {
            var matrix = _editor.Matrix;
            return frontIndex >= 0 && frontIndex < matrix.Count && levelIndex >= 0 && levelIndex < matrix.Fronts[frontIndex].LoadLevels;
        }

        private void Add(int frontIndex, int levelIndex)
        {
            if (!Exists(frontIndex, levelIndex) || _editor.Matrix.IsSelected(frontIndex, levelIndex)) return;
            _editor.Matrix.ToggleCell(frontIndex, levelIndex, extendSelection: true);
        }

        private void SetAnnotation(Action change)
        {
            Apply(change, null);
        }

        // Every change of the draft: a snapshot of the matrix, the change, the recomposition; a change that cannot be recomposed is undone
        // and its reason goes back to the field.
        private string Apply(Action change, string fieldError)
        {
            if (fieldError != null) return fieldError;

            var snapshot = _editor.Matrix.Snapshot();
            var state = CaptureScalars();
            change();
            if (!_editor.Recompose())
            {
                var reason = _editor.LastError;
                _editor.Matrix.Restore(snapshot);
                RestoreScalars(state);
                _editor.Recompose();
                Say(reason, error: true);
                Rebuild();
                return reason;
            }

            MarkDirty();
            Say(string.IsNullOrWhiteSpace(_editor.LastRebuildReport) ? string.Empty : _editor.LastRebuildReport, error: false);
            Rebuild();
            return null;
        }

        private (double Depth, double Peralte, string Post, double? Manual, int? SepCount, double? SepSpacing, bool Reinforced, double? Reinforcement, bool NumF, bool NumL, bool RackName, double Scale, DimensionDetail Dims) CaptureScalars()
        {
            var a = _editor.Annotations;
            return (_editor.PalletDepth, _editor.PostPeralte, _editor.PostCatalogId, _editor.ManualHeaderHeight, _editor.SeparatorCountOverride,
                _editor.SeparatorSpacingOverride, _editor.DerivedPostReinforced, _editor.DerivedPostReinforcementHeight,
                a.NumberFronts, a.NumberLevels, a.DrawRackName, a.AnnotationScale, a.Dimensions);
        }

        private void RestoreScalars((double Depth, double Peralte, string Post, double? Manual, int? SepCount, double? SepSpacing, bool Reinforced, double? Reinforcement, bool NumF, bool NumL, bool RackName, double Scale, DimensionDetail Dims) s)
        {
            _editor.PalletDepth = s.Depth;
            _editor.PostPeralte = s.Peralte;
            _editor.PostCatalogId = s.Post;
            _editor.ManualHeaderHeight = s.Manual;
            _editor.SeparatorCountOverride = s.SepCount;
            _editor.SeparatorSpacingOverride = s.SepSpacing;
            _editor.DerivedPostReinforced = s.Reinforced;
            _editor.DerivedPostReinforcementHeight = s.Reinforcement;
            _editor.Annotations.NumberFronts = s.NumF;
            _editor.Annotations.NumberLevels = s.NumL;
            _editor.Annotations.DrawRackName = s.RackName;
            _editor.Annotations.AnnotationScale = s.Scale;
            _editor.Annotations.Dimensions = s.Dims;
        }

        private void MarkDirty()
        {
            if (_dirty) return;
            _dirty = true;
            RaiseState();
        }

        private void RaiseState()
        {
            OnPropertyChanged(nameof(IsDirty));
            OnPropertyChanged(nameof(StatusText));
            OnPropertyChanged(nameof(CanUpdate));
            OnPropertyChanged(nameof(UpdateBlockedText));
        }

        private void Rebuild()
        {
            _essentials = BuildEssentials();
            _postFields = BuildPostFields();
            _advancedFields = BuildAdvancedFields();
            _calculated = BuildCalculated();
            _modules = BuildModules();
            OnPropertyChanged(nameof(Essentials));
            OnPropertyChanged(nameof(PostFields));
            OnPropertyChanged(nameof(AdvancedFields));
            OnPropertyChanged(nameof(Calculated));
            OnPropertyChanged(nameof(Modules));
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(NumberFronts));
            OnPropertyChanged(nameof(NumberLevels));
            OnPropertyChanged(nameof(DrawRackName));
            OnPropertyChanged(nameof(DerivedPostReinforced));
            RebuildSelection();
            RaiseState();
        }

        private void RebuildSelection()
        {
            var matrix = _editor.Matrix;
            var maxLevels = Math.Max(1, matrix.Count == 0 ? 1 : matrix.Fronts.Max(f => f.LoadLevels));
            var columns = new List<DynamicColumnHead>();
            for (var f = 0; f < matrix.Count; f++) columns.Add(new DynamicColumnHead(f, "F" + (f + 1), !matrix.IsActive(f)));

            var rows = new List<DynamicMatrixRow>();
            for (var l = maxLevels - 1; l >= 0; l--)
            {
                var cells = new List<DynamicCellItem>();
                for (var f = 0; f < matrix.Count; f++)
                {
                    var front = matrix.Fronts[f];
                    var exists = l < front.LoadLevels;
                    var text = "—";
                    if (exists && matrix.IsActive(f))
                    {
                        front.EnsureCellCount(front.LoadLevels);
                        var cell = front.Cells[l];
                        text = Num(cell.PalletFront) + "×" + Num(cell.PalletHeight);
                    }

                    cells.Add(new DynamicCellItem(
                        f, l, text, exists, exists && matrix.IsSelected(f, l),
                        exists && matrix.SelectedFrontIndex == f && matrix.SelectedLevelIndex == l,
                        !matrix.IsActive(f)));
                }

                rows.Add(new DynamicMatrixRow(l, "N" + (l + 1), cells));
            }

            _columns = columns;
            _rows = rows;
            _cellFields = BuildCellFields();
            _frontFields = BuildFrontFields();
            OnPropertyChanged(nameof(Columns));
            OnPropertyChanged(nameof(Rows));
            OnPropertyChanged(nameof(CellFields));
            OnPropertyChanged(nameof(FrontFields));
            OnPropertyChanged(nameof(InspectorTitle));
        }

        // «Lo esencial»: rack-wide values. A cell value writes every existing cell of the rack, a front value every front.
        private IReadOnlyList<DynamicFieldItem> BuildEssentials()
        {
            var cells = AllCells().ToList();
            var fronts = _editor.Matrix.Fronts.ToList();
            return new[]
            {
                Number("depth", "Profundidad de tarima", "in", new[] { _editor.PalletDepth }, v => v > 0.0, "Fondo inválido.", v => _editor.PalletDepth = v),
                CellNumber("front", "Ancho de tarima", "in", cells, c => c.PalletFront, (c, v) => c.PalletFront = v, v => v > 0.0),
                CellNumber("height", "Alto de tarima", "in", cells, c => c.PalletHeight, (c, v) => c.PalletHeight = v, v => v > 0.0),
                CellNumber("weight", "Peso de tarima", "kg", cells, c => c.PalletWeight, (c, v) => c.PalletWeight = v, v => v >= 0.0),
                new DynamicFieldItem("fronts", "Frentes", string.Empty, _editor.Matrix.Count.ToString(CultureInfo.InvariantCulture), string.Empty, SetFrontCount),
                FrontInt("positions", "Carriles por frente", fronts, f => f.PalletCount, (f, v) => f.PalletCount = v, 1),
                FrontInt("levels", "Niveles", fronts, f => f.LoadLevels, (f, v) => { f.LoadLevels = v; f.EnsureCellCount(v); }, 1),
                FrontInt("deep", "Posiciones de fondo", fronts, f => f.PalletsDeep, (f, v) => f.PalletsDeep = v, 2),
                FrontNumber("first", "Inicio del 1er larguero", "in", fronts, f => f.FirstLevelHeight, (f, v) => f.FirstLevelHeight = v, v => v >= 0.0),
                CellNumber("clear", "Claro libre", "in", cells, c => c.ClearHeight, (c, v) => c.ClearHeight = v, v => v >= 0.0),
            };
        }

        private IReadOnlyList<DynamicFieldItem> BuildPostFields() => new[]
        {
            Number("peralte", "Peralte de poste", "in", new[] { _editor.PostPeralte }, v => v > 0.0, "El peralte de poste debe ser un número mayor que cero.", v => _editor.PostPeralte = v),
            Number("scale", "Escala de texto (1 = 6\")", string.Empty, new[] { _editor.Annotations.AnnotationScale }, v => v > 0.0, "La escala de texto debe ser mayor que cero.", v => _editor.Annotations.AnnotationScale = v),
        };

        private IReadOnlyList<DynamicFieldItem> BuildAdvancedFields() => new[]
        {
            OptionalNumber("manual", "Altura manual de cabecera (vacío = calculada)", "in", _editor.ManualHeaderHeight, "Altura manual inválida (debe ser mayor que cero).", v => _editor.ManualHeaderHeight = v),
            OptionalInt("sepCount", "Separadores (cantidad; vacío = estándar)", _editor.SeparatorCountOverride, "Cantidad de separadores inválida (entero >= 1, o vacío para estándar).", v => _editor.SeparatorCountOverride = v),
            OptionalNumber("sepSpacing", "Separación de separadores (vacío = estándar)", "in", _editor.SeparatorSpacingOverride, "Separación de separadores inválida (deja vacío para estándar).", v => _editor.SeparatorSpacingOverride = v),
            OptionalNumber("reinforcement", "Longitud del refuerzo del poste derivado (vacío = altura completa)", "in", _editor.DerivedPostReinforcementHeight, "Altura de refuerzo del poste derivado inválida (deja vacío para altura completa).", v => _editor.DerivedPostReinforcementHeight = v),
        };

        // The inspector: the cell values of the selected cells.
        private IReadOnlyList<DynamicFieldItem> BuildCellFields()
        {
            var cells = SelectedCells().ToList();
            return new[]
            {
                CellNumber("cFront", "Ancho de tarima", "in", cells, c => c.PalletFront, (c, v) => c.PalletFront = v, v => v > 0.0),
                CellNumber("cHeight", "Alto de tarima", "in", cells, c => c.PalletHeight, (c, v) => c.PalletHeight = v, v => v > 0.0),
                CellNumber("cWeight", "Peso de tarima", "kg", cells, c => c.PalletWeight, (c, v) => c.PalletWeight = v, v => v >= 0.0),
                CellNumber("cClear", "Claro libre", "in", cells, c => c.ClearHeight, (c, v) => c.ClearHeight = v, v => v >= 0.0),
                CellOptional("cLength", "Largo manual IN/OUT (vacío = calculado)", "in", cells, c => c.BeamLengthOverride, (c, v) => c.BeamLengthOverride = v),
                CellNumber("cInOut", "Peralte IN/OUT", "in", cells, c => c.InOutBeamDepth, (c, v) => c.InOutBeamDepth = v, v => v > 0.0),
                CellNumber("cInter", "Peralte intermedio", "in", cells, c => c.IntermediateBeamDepth, (c, v) => c.IntermediateBeamDepth = v, v => v > 0.0),
            };
        }

        // The inspector: the front values of the fronts that the selection touches.
        private IReadOnlyList<DynamicFieldItem> BuildFrontFields()
        {
            var fronts = SelectedFronts().ToList();
            return new[]
            {
                FrontInt("fPositions", "Carriles", fronts, f => f.PalletCount, (f, v) => f.PalletCount = v, 1),
                FrontInt("fLevels", "Niveles", fronts, f => f.LoadLevels, (f, v) => { f.LoadLevels = v; f.EnsureCellCount(v); }, 1),
                FrontInt("fDeep", "Posiciones de fondo", fronts, f => f.PalletsDeep, (f, v) => f.PalletsDeep = v, 2),
                FrontInt("fStart", "Inicio en fondo (1 = salida)", fronts, f => f.DepthStartPosition, (f, v) => f.DepthStartPosition = v, 1),
                FrontNumber("fFirst", "Inicio del 1er larguero", "in", fronts, f => f.FirstLevelHeight, (f, v) => f.FirstLevelHeight = v, v => v >= 0.0),
            };
        }

        private IReadOnlyList<DynamicCalcItem> BuildCalculated()
        {
            var system = _editor.System;
            var derived = _editor.DerivedHeight;
            var items = new List<DynamicCalcItem>
            {
                new("Altura de cabecera", Num(_editor.HeaderHeight) + " in" + (_editor.ManualHeaderHeight.HasValue ? " (manual)" : string.Empty)),
                new("Pendiente", derived == null ? "—" : Num(derived.Slope) + " in"),
                new("Posiciones de fondo del rack", _editor.PalletsDeep.ToString(CultureInfo.InvariantCulture)),
                new("Niveles máximos", _editor.Matrix.MaxLoadLevels().ToString(CultureInfo.InvariantCulture)),
                new("Módulos", system == null ? "—" : system.Modules.Count.ToString(CultureInfo.InvariantCulture)),
            };
            return items;
        }

        private IReadOnlyList<DynamicModuleItem> BuildModules()
        {
            var system = _editor.System;
            if (system == null) return Array.Empty<DynamicModuleItem>();
            var items = new List<DynamicModuleItem>();
            var x = 0.0;
            for (var i = 0; i < system.Modules.Count; i++)
            {
                var module = system.Modules[i];
                items.Add(new DynamicModuleItem(
                    "M" + (i + 1),
                    module.IsHeader ? "Cabecera" : "Separador",
                    Num(module.Length),
                    Num(x),
                    module.IsManualOverride ? "Sí" : "No"));
                x += module.Length;
            }

            return items;
        }

        private IEnumerable<DynamicEditorCell> AllCells()
        {
            foreach (var front in _editor.Matrix.Fronts)
            {
                front.EnsureCellCount(front.LoadLevels);
                for (var l = 0; l < front.LoadLevels; l++) yield return front.Cells[l];
            }
        }

        private IEnumerable<DynamicEditorCell> SelectedCells()
        {
            foreach (var address in _editor.Matrix.SelectedCells())
            {
                var front = _editor.Matrix.Fronts[address.FrontIndex];
                front.EnsureCellCount(front.LoadLevels);
                if (address.LevelIndex < front.Cells.Count) yield return front.Cells[address.LevelIndex];
            }
        }

        private IEnumerable<DynamicEditorFront> SelectedFronts() =>
            _editor.Matrix.SelectedCells().Select(c => c.FrontIndex).Distinct().OrderBy(i => i).Select(i => _editor.Matrix.Fronts[i]);

        private DynamicFieldItem Number(string key, string label, string unit, IEnumerable<double> values, Func<double, bool> valid, string invalid, Action<double> set)
        {
            var list = values.ToList();
            return new DynamicFieldItem(key, label, unit, Same(list), Hint(list.Cast<object>()), text =>
            {
                if (!UiSupport.TryNum(text, out var value) || !valid(value)) return invalid;
                return Apply(() => set(value), null);
            });
        }

        private DynamicFieldItem OptionalNumber(string key, string label, string unit, double? current, string invalid, Action<double?> set) =>
            new(key, label, unit, current.HasValue ? Num(current.Value) : string.Empty, string.Empty, text =>
            {
                if (string.IsNullOrWhiteSpace(text)) return Apply(() => set(null), null);
                if (!UiSupport.TryNum(text, out var value) || value <= 0.0) return invalid;
                return Apply(() => set(value), null);
            });

        private DynamicFieldItem OptionalInt(string key, string label, int? current, string invalid, Action<int?> set) =>
            new(key, label, string.Empty, current.HasValue ? current.Value.ToString(CultureInfo.InvariantCulture) : string.Empty, string.Empty, text =>
            {
                if (string.IsNullOrWhiteSpace(text)) return Apply(() => set(null), null);
                if (!UiSupport.TryInt(text, out var value) || value < 1) return invalid;
                return Apply(() => set(value), null);
            });

        private DynamicFieldItem CellNumber(string key, string label, string unit, IReadOnlyList<DynamicEditorCell> cells, Func<DynamicEditorCell, double> get, Action<DynamicEditorCell, double> set, Func<double, bool> valid)
        {
            var values = cells.Select(get).ToList();
            return new DynamicFieldItem(key, label, unit, Same(values), Hint(values.Cast<object>()), text =>
            {
                if (!UiSupport.TryNum(text, out var value) || !valid(value)) return "Revisa los valores de tarima, claro y largueros de cada celda.";
                return Apply(() => { foreach (var cell in cells) set(cell, value); }, null);
            });
        }

        private DynamicFieldItem CellOptional(string key, string label, string unit, IReadOnlyList<DynamicEditorCell> cells, Func<DynamicEditorCell, double?> get, Action<DynamicEditorCell, double?> set)
        {
            var values = cells.Select(get).ToList();
            var same = values.Distinct().Count() <= 1;
            var text0 = same && values.Count > 0 && values[0].HasValue ? Num(values[0].Value) : string.Empty;
            return new DynamicFieldItem(key, label, unit, text0, same ? string.Empty : Varies, text =>
            {
                if (string.IsNullOrWhiteSpace(text)) return Apply(() => { foreach (var cell in cells) set(cell, null); }, null);
                if (!UiSupport.TryNum(text, out var value) || value <= 0.0) return "Revisa los valores de tarima, claro y largueros de cada celda.";
                return Apply(() => { foreach (var cell in cells) set(cell, value); }, null);
            });
        }

        private DynamicFieldItem FrontInt(string key, string label, IReadOnlyList<DynamicEditorFront> fronts, Func<DynamicEditorFront, int> get, Action<DynamicEditorFront, int> set, int minimum)
        {
            var values = fronts.Select(get).ToList();
            var same = values.Distinct().Count() <= 1;
            var text0 = same && values.Count > 0 ? values[0].ToString(CultureInfo.InvariantCulture) : string.Empty;
            return new DynamicFieldItem(key, label, string.Empty, text0, same ? string.Empty : Varies, text =>
            {
                if (!UiSupport.TryInt(text, out var value) || value < minimum)
                    return "Debe ser un entero mayor o igual que " + minimum.ToString(CultureInfo.InvariantCulture) + ".";
                return Apply(() => { foreach (var front in fronts) set(front, value); _editor.Matrix.NormalizeSelection(); }, null);
            });
        }

        private DynamicFieldItem FrontNumber(string key, string label, string unit, IReadOnlyList<DynamicEditorFront> fronts, Func<DynamicEditorFront, double> get, Action<DynamicEditorFront, double> set, Func<double, bool> valid)
        {
            var values = fronts.Select(get).ToList();
            return new DynamicFieldItem(key, label, unit, Same(values), Hint(values.Cast<object>()), text =>
            {
                if (!UiSupport.TryNum(text, out var value) || !valid(value)) return "El inicio del primer larguero debe ser mayor o igual que cero.";
                return Apply(() => { foreach (var front in fronts) set(front, value); }, null);
            });
        }

        private static string Same(IReadOnlyList<double> values) =>
            values.Count > 0 && values.All(v => Math.Abs(v - values[0]) < 1e-9) ? Num(values[0]) : string.Empty;

        private static string Hint(IEnumerable<object> values) => values.Distinct().Count() > 1 ? Varies : string.Empty;

        private static string Num(double value) => value.ToString("0.##", CultureInfo.InvariantCulture);
    }
}
