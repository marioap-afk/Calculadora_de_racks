using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using RackCad.Application.Systems.Cantilever;
using RackCad.Application.VentanaRackCad;
using RackCad.Domain.Systems.Cantilever;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// The tab «Brazos» of the Cantilever editor (CT-01, CT-06; dolores 3, 4 and 15): a compact matrix station × level (× side), with ONE datum
    /// per cell, the cut length of its arm in inches, an «E» for an exception and a «T» for an end plate with stop; «Ver detalle en celdas» adds
    /// the section. A click, Ctrl+click and Mayús+click select; «E1…», «N1…» and «Todo» select a station, a level or the whole line; the
    /// segments Celda · Estación · Nivel · Lado · Todo widen the selection from its primary cell. The inspector edits the arm in force in the
    /// selection: one scope, the selection; a write makes it the exception of every cell selected (an exception equal to the default arm is
    /// not stored). «Volver al brazo por omisión» removes the exceptions of the selection.
    /// </summary>
    public sealed partial class CantileverEditorViewModel
    {
        private readonly List<CantileverLineCell> _selection = new();
        private CantileverLineCell? _primary;
        private CantileverLineCell? _anchor;
        private string _scope = "celda";
        private bool _showCellDetail;
        private readonly Dictionary<CantileverLineCell, CantileverCellItem> _cellItems = new();
        private IReadOnlyList<CantileverColumnHead> _columnHeads = Array.Empty<CantileverColumnHead>();
        private IReadOnlyList<CantileverMatrixRow> _rows = Array.Empty<CantileverMatrixRow>();
        private string _matrixShape = string.Empty;

        public IReadOnlyList<CantileverColumnHead> ColumnHeads => _columnHeads;

        public IReadOnlyList<CantileverMatrixRow> MatrixRows => _rows;

        public string MatrixTitle
        {
            get
            {
                var t = _draft.Design.StationTopology ?? new CantileverLineStationTopologyDesign();
                return t.FaceMode == CantileverStationFaceMode.Double
                    ? "Brazos: estación × nivel × lado"
                    : "Brazos: estación × nivel · lado " + CantileverDraftEditor.SideLabel(t.SingleSide);
            }
        }

        /// <summary>«Ver detalle en celdas»: the section of the arm under its cut length.</summary>
        public bool ShowCellDetail
        {
            get => _showCellDetail;
            set => SetProperty(ref _showCellDetail, value);
        }

        /// <summary>The selection of the matrix, in the order of the line (station, level, side).</summary>
        public IReadOnlyList<CantileverLineCell> Selection => _selection;

        public string Scope => _scope;

        private CantileverChoiceItem _scopeChoice;

        /// <summary>The segments of the scope: Celda (or «Celdas (n)») · Estación · Nivel · Lado (double faces only) · Todo.</summary>
        public CantileverChoiceItem ScopeChoice => _scopeChoice;

        public ObservableCollection<object> InspectorItems { get; } = new();

        public ObservableCollection<object> InspectorPlateItems { get; } = new();

        public ObservableCollection<object> InspectorEndItems { get; } = new();

        public string InspectorTitle
        {
            get
            {
                if (_selection.Count == 0) return "Ninguna celda seleccionada";
                switch (_scope)
                {
                    case "todo": return "Toda la línea";
                    case "estacion": return List(_selection.Select(c => c.StationIndex + 1), "Estación", "Estaciones");
                    case "nivel": return List(_selection.Select(c => c.LevelIndex + 1), "Nivel", "Niveles");
                    case "lado":
                        var sides = _selection.Select(c => c.Side).Distinct().Select(CantileverDraftEditor.SideLabel).ToList();
                        return (sides.Count == 1 ? "Lado " : "Lados ") + string.Join(" y ", sides);
                    default:
                        return _selection.Count == 1 ? CantileverDraftEditor.CellTitle(_selection[0]) : _selection.Count + " celdas";
                }
            }
        }

        public string TargetText
        {
            get
            {
                var matrix = _draft.Matrix;
                return "Destino: " + CantileverDraftEditor.Count(_selection.Count, "celda", "celdas") + " · " + _selection.Count(matrix.HasOverride) + " con excepción.";
            }
        }

        public string ExceptionText
        {
            get
            {
                var count = _selection.Count(_draft.Matrix.HasOverride);
                return count == 0 ? "La selección sigue el brazo por omisión." : count + " de " + CantileverDraftEditor.Count(_selection.Count, "celda", "celdas") + " con excepción.";
            }
        }

        public bool CanRestoreCells => !_writing && _selection.Any(_draft.Matrix.HasOverride);

        public string DefaultArmSummary => _draft.ArmSummary(_draft.Design.DefaultArmTemplate) + " · " + EndLabel(_draft.Design.DefaultArmTemplate?.EndPlate?.Mode ?? CantileverArmEndPlateMode.None);

        public string DefaultArmText
        {
            get
            {
                var matrix = _draft.Matrix;
                var cells = matrix.Cells.Count;
                return "Lo usan las celdas sin excepción: " + (cells - matrix.OverrideCount) + " de " + cells + ". Cambiarlo no toca las excepciones.";
            }
        }

        /// <summary>A click on a cell: alone; with Ctrl it adds or removes it; with Mayús, the rectangle from the anchor.</summary>
        public void ClickCell(CantileverLineCell cell, bool control, bool shift)
        {
            if (!_draft.Matrix.IsActive(cell)) return;
            if (shift && _anchor.HasValue)
            {
                var a = _anchor.Value;
                var rowA = RowIndexOf(a);
                var rowB = RowIndexOf(cell);
                var cells = _draft.Matrix.Cells.Where(c =>
                    Between(c.StationIndex, a.StationIndex, cell.StationIndex) && Between(RowIndexOf(c), rowA, rowB)).ToList();
                SetSelection(cells, "celda", cell, keepAnchor: true);
                return;
            }

            if (control)
            {
                var next = _selection.ToList();
                if (next.Contains(cell)) next.Remove(cell);
                else next.Add(cell);
                SetSelection(next, "celda", next.Contains(cell) ? cell : next.FirstOrDefault());
                return;
            }

            SetSelection(new[] { cell }, "celda", cell);
        }

        public void SelectStation(int stationIndex) =>
            SetSelection(_draft.Matrix.Cells.Where(c => c.StationIndex == stationIndex).ToList(), "estacion", null);

        public void SelectLevel(int levelIndex) =>
            SetSelection(_draft.Matrix.Cells.Where(c => c.LevelIndex == levelIndex).ToList(), "nivel", null);

        public void SelectAll() => SetSelection(_draft.Matrix.Cells.ToList(), "todo", null);

        /// <summary>A segment of the scope: the selection widens from its primary cell (CT-06: «Todo» is the whole line).</summary>
        public void SetScope(string scope)
        {
            if (!_primary.HasValue && _selection.Count == 0) return;
            var anchor = _primary ?? _selection[0];
            var matrix = _draft.Matrix;
            switch (scope)
            {
                case "estacion": SetSelection(matrix.InScope(CantileverLineApplyScope.Station, anchor).ToList(), scope, anchor); break;
                case "nivel": SetSelection(matrix.InScope(CantileverLineApplyScope.Level, anchor).ToList(), scope, anchor); break;
                case "lado": SetSelection(matrix.InScope(CantileverLineApplyScope.Side, anchor).ToList(), scope, anchor); break;
                case "todo": SetSelection(matrix.InScope(CantileverLineApplyScope.Line, anchor).ToList(), scope, anchor); break;
                default: SetSelection(new[] { anchor }, "celda", anchor); break;
            }
        }

        /// <summary>«Volver al brazo por omisión»: the cells of the selection lose their exception.</summary>
        public void RestoreCells()
        {
            if (!CanRestoreCells) return;
            var count = _selection.Count(_draft.Matrix.HasOverride);
            var refused = Commit(() => _draft.RestoreCells(_selection.ToList()));
            Say(refused ?? CantileverDraftEditor.Count(count, "celda vuelve", "celdas vuelven") + " al brazo por omisión.", refused != null);
        }

        /// <summary>«Cambiar el brazo por omisión»: «Componentes» › «Brazo por omisión».</summary>
        public void GoToDefaultArm()
        {
            SelectedTab = ComponentsTab;
            SelectComponent(CantileverDraftEditor.ComponentArm);
        }

        private void SetSelection(IReadOnlyList<CantileverLineCell> cells, string scope, CantileverLineCell? primary, bool keepAnchor = false)
        {
            var matrix = _draft.Matrix;
            var order = matrix.Cells.ToList();
            _selection.Clear();
            _selection.AddRange(cells.Where(matrix.IsActive).Distinct().OrderBy(c => order.IndexOf(c)));
            _primary = primary.HasValue && _selection.Contains(primary.Value) ? primary : _selection.Count > 0 ? _selection[0] : null;
            if (!keepAnchor) _anchor = _primary;
            _scope = scope;
            RebuildArms(_writing);
            RaiseArms();
        }

        private void SelectFirstCell()
        {
            var first = _draft.Matrix.Cells.FirstOrDefault();
            _selection.Clear();
            if (_draft.Matrix.Cells.Count > 0) _selection.Add(first);
            _primary = _selection.Count > 0 ? first : null;
            _anchor = _primary;
            _scope = "celda";
        }

        // The rows of the matrix from the top: N(top) +Y, N(top) −Y, … N1 −Y.
        private int RowIndexOf(CantileverLineCell cell)
        {
            var sides = _draft.Matrix.ActiveSides;
            return (_draft.Matrix.LevelCount - 1 - cell.LevelIndex) * sides.Count + sides.ToList().IndexOf(cell.Side);
        }

        private static bool Between(int value, int a, int b) => value >= Math.Min(a, b) && value <= Math.Max(a, b);

        private static string List(IEnumerable<int> numbers, string one, string many)
        {
            var list = numbers.Distinct().OrderBy(n => n).ToList();
            return (list.Count == 1 ? one : many) + " " + string.Join(", ", list);
        }

        private void RebuildArms(bool ro)
        {
            var matrix = _draft.Matrix;
            var shape = matrix.StationCount + "x" + matrix.LevelCount + "x" + string.Join(",", matrix.ActiveSides);

            // The selection follows the shape of the line: cells that no longer exist leave it (with none left, the first cell).
            var kept = _selection.Where(matrix.IsActive).ToList();
            if (kept.Count != _selection.Count)
            {
                _selection.Clear();
                _selection.AddRange(kept);
                if (_selection.Count == 0 && matrix.Cells.Count > 0) _selection.Add(matrix.Cells[0]);
                if (!_primary.HasValue || !_selection.Contains(_primary.Value)) _primary = _selection.Count > 0 ? _selection[0] : null;
                if (!_anchor.HasValue || !matrix.IsActive(_anchor.Value)) _anchor = _primary;
                if (_selection.Count <= 1) _scope = "celda";
            }

            if (shape != _matrixShape)
            {
                _matrixShape = shape;
                _cellItems.Clear();
                _columnHeads = Enumerable.Range(0, matrix.StationCount)
                    .Select(e => new CantileverColumnHead(e, "E" + (e + 1), "Estación " + (e + 1) + ": seleccionar la estación")).ToList();
                var rows = new List<CantileverMatrixRow>();
                var dbl = matrix.ActiveSides.Count > 1;
                for (var level = matrix.LevelCount - 1; level >= 0; level--)
                {
                    foreach (var side in matrix.ActiveSides)
                    {
                        var cells = new List<CantileverCellItem>();
                        for (var station = 0; station < matrix.StationCount; station++)
                        {
                            var cell = new CantileverLineCell(station, level, side);
                            var item = new CantileverCellItem(cell);
                            _cellItems[cell] = item;
                            cells.Add(item);
                        }

                        rows.Add(new CantileverMatrixRow(level, side, "N" + (level + 1) + (dbl ? " " + CantileverDraftEditor.SideLabel(side) : string.Empty),
                            "Nivel " + (level + 1) + (dbl ? " " + CantileverDraftEditor.SideLabel(side) : string.Empty) + ": seleccionar el nivel"
                            + (dbl ? " en los dos lados" : string.Empty), cells));
                    }
                }

                _rows = rows;
                OnPropertyChanged(nameof(ColumnHeads));
                OnPropertyChanged(nameof(MatrixRows));
            }

            var baseMatrix = _baseDesign == null ? null : new CantileverLineArmMatrix(_baseDesign);
            foreach (var pair in _cellItems)
            {
                var cell = pair.Key;
                var arm = matrix.Effective(cell);
                var cut = arm?.Body?.CutLength > 0 ? CantileverDraftEditor.Num(arm.Body.CutLength) : null;
                var section = _draft.Designation(arm?.Body?.SectionId);
                var exception = matrix.HasOverride(cell);
                var stop = arm?.EndPlate?.Mode == CantileverArmEndPlateMode.Stop;
                var missing = cut == null || section == null;
                var modified = baseMatrix != null && baseMatrix.IsActive(cell)
                               && !CantileverArmTemplateComparer.AreEqual(baseMatrix.Effective(cell), arm);
                var states = new List<string>();
                if (exception) states.Add("con excepción");
                if (modified) states.Add("modificada en el borrador");
                if (stop) states.Add("con placa de tope");
                if (missing) states.Add("le falta " + (section == null ? "la sección" : "la longitud"));
                var accessible = CantileverDraftEditor.CellTitle(cell) + ": brazo " + (section ?? "sin sección") + ", " + (cut == null ? "sin longitud" : cut + " in")
                                 + (states.Count > 0 ? "; " + string.Join(", ", states) : string.Empty);
                pair.Value.Set(cut ?? "—", section ?? "sin sección", _selection.Contains(cell), _primary.HasValue && _primary.Value.Equals(cell),
                    exception, stop, missing, modified, accessible);
            }

            var selection = _selection.ToList();
            var ends = selection.Select(c => matrix.Effective(c)?.EndPlate?.Mode ?? CantileverArmEndPlateMode.None).Distinct().ToList();
            var none = selection.Count == 0 || (ends.Count == 1 && ends[0] == CantileverArmEndPlateMode.None);
            var allStop = ends.Count == 1 && ends[0] == CantileverArmEndPlateMode.Stop;
            var empty = selection.Count == 0;
            Sync(InspectorItems, new object[]
            {
                ArmPicker("insp.section", "Sección", selection, "El brazo de la selección.", ro || empty),
                ArmChoice("insp.arr", "Arreglo", selection, CantileverArmField.Arrangement, ArrangementOptions, "Los apareados exigen canal (C).", ro || empty),
                ArmField("insp.cut", "Longitud de corte", "in", selection, CantileverArmField.CutLength, "Sin placas: el largo que se pide.", ro || empty),
                ArmField("insp.slope", "Pendiente", "in/12", selection, CantileverArmField.Slope, "Subida por 12 in de vuelo.", ro || empty),
            });
            Sync(InspectorPlateItems, new object[]
            {
                ArmField("insp.plate", "Placa de montaje: espesor", "in", selection, CantileverArmField.PlateThickness, null, ro || empty),
                ArmField("insp.punches", "Troqueles verticales", "", selection, CantileverArmField.VerticalPunches, "Mín. 2. Al cambiar la sección se vuelve a sugerir.", ro || empty),
                ArmField("insp.margin", "Margen vertical", "in", selection, CantileverArmField.VerticalMargin, "Por omisión, 2 in.", ro || empty),
            });
            Sync(InspectorEndItems, new object[]
            {
                ArmChoice("insp.end", "Placa de extremo: tipo", selection, CantileverArmField.EndPlate, EndOptions, null, ro || empty),
                ArmField("insp.endT", "Placa de extremo: espesor", "in", selection, CantileverArmField.EndPlateThickness, none ? "Sin placa de extremo." : null, ro || none),
                ArmField("insp.stop", "Altura extra de tope", "in", selection, CantileverArmField.ExtraStopHeight, allStop ? null : "Solo con «Placa con tope».", ro || !allStop),
            });
            var scopes = new List<(string Value, string Label)>
            {
                ("celda", _selection.Count > 1 && _scope == "celda" ? "Celdas (" + _selection.Count + ")" : "Celda"), ("estacion", "Estación"), ("nivel", "Nivel"),
            };
            if (matrix.ActiveSides.Count > 1) scopes.Add(("lado", "Lado"));
            scopes.Add(("todo", "Todo"));
            _scopeChoice = new CantileverChoiceItem("scope", "Alcance", scopes, _scope, null, value => { SetScope(value); return null; }, ro || empty);
            OnPropertyChanged(nameof(ScopeChoice));
        }

        private void RaiseArms()
        {
            foreach (var name in new[]
                     {
                         nameof(MatrixTitle), nameof(Selection), nameof(Scope), nameof(InspectorTitle), nameof(TargetText), nameof(ExceptionText),
                         nameof(CanRestoreCells), nameof(DefaultArmSummary), nameof(DefaultArmText), nameof(ShowLadoScope),
                     })
                OnPropertyChanged(name);
        }

        /// <summary>«Lado» is a scope only with double faces.</summary>
        public bool ShowLadoScope => _draft.Matrix.ActiveSides.Count > 1;
    }
}
