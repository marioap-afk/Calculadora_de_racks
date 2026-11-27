using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using RackCad.Application.VentanaRackCad;
using RackCad.Domain.Systems.Cantilever;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// The tab «Arriostramiento» of the Cantilever editor and «Restaurar layout…» (CT-07, CT-10; dolor 17). The panels (automatic or manual
    /// count, the heights) are fields of the draft. The advanced sequence is a table that is a draft until «Aplicar secuencia», as the tramos of
    /// half a front of Selectivo: while it has rows not applied, «Actualizar» refuses and says why. Going back to «Automática» asks inline (the
    /// tramos are kept); going to «Avanzada» with tramos kept offers «Recuperar mis tramos» or «Empezar desde la regla».
    /// </summary>
    public sealed partial class CantileverEditorViewModel
    {
        private bool _askingToAutomatic;
        private bool _askingRecover;
        private bool _restoreOpen;
        private bool _lossOpen;
        private string _segmentError = string.Empty;
        private bool _rowsFromDesign = true;
        private CantileverChoiceItem _layoutChoice;

        public ObservableCollection<object> BracingItems { get; } = new();

        public CantileverChoiceItem LayoutChoice => _layoutChoice;

        public bool IsAdvanced => _draft.IsAdvanced;

        public string LayoutHint => IsAdvanced
            ? "Tú declaras cada tramo, de abajo arriba. La tabla es un borrador hasta «Aplicar secuencia»."
            : "La regla reparte los paneles en bloques de dos desde abajo, con el espacio central entre bloques.";

        public string DerivedText
        {
            get
            {
                if (!_draft.IsResolved) return "Derivado: la línea no se resuelve todavía.";
                var line = _draft.Computation.Line;
                var interval = line.Intervals.FirstOrDefault();
                if (interval == null) return "Derivado: la línea no tiene intervalos.";
                return "Derivado por intervalo: " + CantileverDraftEditor.Count(interval.BracedPanels.Count, "panel arriostrado", "paneles arriostrados") + " · "
                       + CantileverDraftEditor.Count(interval.Separators.Count, "separador", "separadores") + " · "
                       + CantileverDraftEditor.Count(interval.Braces.Count, "tensor", "tensores") + ".";
            }
        }

        public string PiecesText
        {
            get
            {
                var b = _draft.Design.Bracing ?? new CantileverBracingDesign();
                var brace = b.BraceKind == CantileverBraceBodyKind.ColdRolledRound
                    ? "de varilla Ø " + CantileverDraftEditor.Num(b.ColdRolled?.Diameter ?? 0) + " in"
                    : "estructural " + (_draft.Designation(b.BraceSectionId) ?? "sin sección");
                return "Separador " + (_draft.Designation(b.SeparatorSectionId) ?? "sin sección") + " · tensor " + brace + ".";
            }
        }

        /// <summary>«Cambiar en «Componentes»»: the separator and the brace are pieces of «Componentes».</summary>
        public void GoToSeparator()
        {
            SelectedTab = ComponentsTab;
            SelectComponent(CantileverDraftEditor.ComponentSeparator);
        }

        // ------------------------------------------------------------------ the diagram of the sequence

        public ObservableCollection<CantileverSequenceItem> SequenceItems { get; } = new();

        public string SequenceTop => "Punta de la columna · " + (_draft.IsResolved ? CantileverDraftEditor.Num(_draft.Computation.Line.ColumnHeight) + " in" : "sin resolver");

        public string SequenceMessage => _draft.IsResolved
            ? string.Empty
            : "La línea no se resuelve todavía: la altura y la secuencia las calcula la línea. Mira «Para que la línea se resuelva».";

        // ------------------------------------------------------------------ the layout and its inline questions

        public bool IsAskingToAutomatic => _askingToAutomatic;

        public string ToAutomaticText =>
            "La secuencia vuelve a la regla y tus " + CantileverDraftEditor.Count(_draft.SavedSegments.Count, "tramo", "tramos")
            + " dejan de mandar. Se conservan por si vuelves a «Avanzada», pero el dibujo y el BOM pasan a salir de la regla."
            + (SegmentsPending ? " Los cambios sin aplicar de la tabla se descartan." : string.Empty);

        public bool IsAskingRecover => _askingRecover;

        public string RecoverText =>
            "Tienes " + CantileverDraftEditor.Count(_draft.SavedSegments.Count, "tramo guardado", "tramos guardados")
            + " de la última vez que usaste «Avanzada». ¿Los recuperas o empiezas desde la secuencia de la regla?";

        private string ChooseLayout(string value)
        {
            if (_writing) return WritingText;
            if (value == "Advanced" && !IsAdvanced)
            {
                if (_draft.SavedSegments.Count > 0)
                {
                    SetLayoutQuestion(toAutomatic: false, recover: true);
                    return null;
                }

                return Commit(() => AfterLayout(_draft.ToAdvanced(recover: false)));
            }

            if (value == "Automatic" && IsAdvanced)
            {
                SetLayoutQuestion(toAutomatic: true, recover: false);
                return null;
            }

            return null;
        }

        /// <summary>«Volver a la regla»: the sequence goes back to the rule; the tramos are kept, and a table not applied is dropped.</summary>
        public void ConfirmToAutomatic()
        {
            if (!_askingToAutomatic) return;
            SetLayoutQuestion(false, false);
            var refused = Commit(() => AfterLayout(_draft.ToAutomatic()));
            if (refused != null) Say(refused, error: true);
        }

        /// <summary>«Recuperar mis tramos».</summary>
        public void Recover()
        {
            if (!_askingRecover) return;
            SetLayoutQuestion(false, false);
            var refused = Commit(() => AfterLayout(_draft.ToAdvanced(recover: true)));
            if (refused != null) Say(refused, error: true);
        }

        /// <summary>«Empezar desde la regla»: the sequence the rule draws now becomes the tramos (the classic «Materializar»).</summary>
        public void StartFromRule()
        {
            if (!_askingRecover) return;
            SetLayoutQuestion(false, false);
            var refused = Commit(() => AfterLayout(_draft.ToAdvanced(recover: false)));
            if (refused != null) Say(refused, error: true);
        }

        public void CancelLayoutQuestion() => SetLayoutQuestion(false, false);

        private string AfterLayout(string refused)
        {
            if (refused == null) ResetSegmentRows();
            return refused;
        }

        private void SetLayoutQuestion(bool toAutomatic, bool recover)
        {
            _askingToAutomatic = toAutomatic;
            _askingRecover = recover;
            OnPropertyChanged(nameof(IsAskingToAutomatic));
            OnPropertyChanged(nameof(IsAskingRecover));
            OnPropertyChanged(nameof(ToAutomaticText));
            OnPropertyChanged(nameof(RecoverText));
        }

        private void CloseLayoutQuestions() => SetLayoutQuestion(false, false);

        // ------------------------------------------------------------------ the table of tramos (CT-10)

        public ObservableCollection<CantileverSegmentRowItem> SegmentRows { get; } = new();

        /// <summary>The table has rows that are not the tramos of the draft: «Aplicar secuencia» or «Descartar cambios de la tabla».</summary>
        public bool SegmentsPending => IsAdvanced && !_rowsFromDesign;

        public string SegmentError => _segmentError;

        public bool CanEditSegments => !_writing && IsAdvanced;

        public bool CanApplySegments => !_writing && SegmentsPending;

        /// <summary>«Aplicar secuencia»: the rows become the tramos of the draft (the resolver says whether they are a valid bracing).</summary>
        public void ApplySegments()
        {
            if (!CanApplySegments) return;
            var rows = SegmentRows.Select(r => r.ToRow()).ToList();
            var refused = _writing ? WritingText : _draft.ApplySegments(rows);
            if (refused != null)
            {
                SetSegmentError(refused);
                return;
            }

            SetSegmentError(string.Empty);
            ResetSegmentRows();
            Say("Secuencia aplicada al borrador.", error: false);
            Rebuild();
        }

        /// <summary>«Descartar cambios de la tabla»: the table goes back to the tramos of the draft.</summary>
        public void DiscardSegments()
        {
            if (_writing || !SegmentsPending) return;
            ResetSegmentRows();
            SetSegmentError(string.Empty);
            Rebuild();
        }

        /// <summary>«Agregar tramo arriba»: a tramo of the height of a panel over the last one, with tensores.</summary>
        public void AddSegmentTop()
        {
            if (!CanEditSegments) return;
            var rows = SegmentRows.Select(r => r.ToRow()).ToList();
            var top = rows.Count == 0 ? 0.0 : CantileverDraftEditor.Keep(rows[rows.Count - 1].To, rows[rows.Count - 1].ToValue, out var t) ? t : 0.0;
            var height = _draft.Design.Bracing?.BracedPanelHeight ?? CantileverLineDefaults.BracedPanelHeight;
            rows.Add(new CantileverSegmentRow(CantileverDraftEditor.Num(top), CantileverDraftEditor.Num(top + height), true, top, top + height));
            SetRows(rows);
        }

        /// <summary>«Dividir»: the tramo in two halves with its bracing.</summary>
        public void SplitSegment(CantileverSegmentRowItem row)
        {
            if (!CanEditSegments || row == null) return;
            var rows = SegmentRows.Select(r => r.ToRow()).ToList();
            var i = row.Index;
            if (i < 0 || i >= rows.Count) return;
            if (!CantileverDraftEditor.Keep(rows[i].From, rows[i].FromValue, out var a) || !CantileverDraftEditor.Keep(rows[i].To, rows[i].ToValue, out var b) || !(b > a))
            {
                SetSegmentError("Tramo " + row.Number + ": escribe antes «Desde» y «Hasta» para dividirlo.");
                return;
            }

            var mid = a + (b - a) / 2.0;
            rows[i] = new CantileverSegmentRow(CantileverDraftEditor.Num(a), CantileverDraftEditor.Num(mid), rows[i].Braced, a, mid);
            rows.Insert(i + 1, new CantileverSegmentRow(CantileverDraftEditor.Num(mid), CantileverDraftEditor.Num(b), rows[i].Braced, mid, b));
            SetRows(rows);
        }

        /// <summary>«Quitar»: the tramo leaves the table (one tramo at least: a sequence without tramos is no bracing).</summary>
        public void RemoveSegment(CantileverSegmentRowItem row)
        {
            if (!CanEditSegments || row == null) return;
            if (SegmentRows.Count <= 1)
            {
                SetSegmentError("Es el único tramo. Una secuencia sin tramos no es un arriostramiento.");
                return;
            }

            var rows = SegmentRows.Select(r => r.ToRow()).ToList();
            rows.RemoveAt(row.Index);
            SetRows(rows);
        }

        private void SetRows(IReadOnlyList<CantileverSegmentRow> rows)
        {
            SetSegmentError(string.Empty);
            FillRows(rows);
            OnRowsChanged();
        }

        private void FillRows(IReadOnlyList<CantileverSegmentRow> rows)
        {
            SegmentRows.Clear();
            for (var i = 0; i < rows.Count; i++) SegmentRows.Add(new CantileverSegmentRowItem(i, rows[i], OnRowsChanged));
        }

        // A row was written: the table differs from the tramos of the draft until it is applied or discarded.
        private void OnRowsChanged()
        {
            var design = CantileverDraftEditor.RowsOf(_draft.SavedSegments);
            var rows = SegmentRows.Select(r => r.ToRow()).ToList();
            _rowsFromDesign = rows.Count == design.Count && rows.Zip(design, (a, b) => a == b).All(x => x);
            Rebuild();
        }

        private void ResetSegmentRows()
        {
            FillRows(CantileverDraftEditor.RowsOf(_draft.SavedSegments));
            _rowsFromDesign = true;
        }

        private void SetSegmentError(string text)
        {
            _segmentError = text ?? string.Empty;
            OnPropertyChanged(nameof(SegmentError));
        }

        private void RebuildBracing(bool ro)
        {
            var b = _draft.Design.Bracing ?? new CantileverBracingDesign();
            var manual = b.PanelCountMode == CantileverBracedPanelCountMode.Manual;
            Sync(BracingItems, new object[]
            {
                Choice(CantileverLineField.PanelCountMode, "Cantidad de paneles", AutoManual, manual ? "La que escribas." : "Regla: máx(1, techo((H − 72) / 60)).", ro),
                Field(CantileverLineField.ManualPanelCount, "Cantidad manual", "", manual ? null : "Solo con cantidad manual; la que había se conserva.", ro || !manual),
                Field(CantileverLineField.PanelHeight, "Altura de panel", "in", null, ro),
                Field(CantileverLineField.CentralGapHeight, "Altura de espacio central", "in", "Entre bloques de dos paneles.", ro),
            });
            _layoutChoice = new CantileverChoiceItem("layout", "Secuencia de paneles", AutoManual.Select(o => (o.Value == "Manual" ? "Advanced" : o.Value, o.Value == "Manual" ? "Avanzada" : o.Label)).ToList(),
                IsAdvanced ? "Advanced" : "Automatic", null, ChooseLayout, ro);
            if (_rowsFromDesign && SegmentRows.Count != _draft.SavedSegments.Count) ResetSegmentRows();

            var items = new List<CantileverSequenceItem>();
            var segments = _draft.IsResolved ? _draft.AutomaticSegments : Array.Empty<CantileverPanelSegmentDesign>();
            foreach (var segment in segments.Reverse())
            {
                var braced = segment.BracingMode != CantileverPanelBracingMode.None;
                items.Add(new CantileverSequenceItem(CantileverDraftEditor.Num(segment.StartElevation) + "–" + CantileverDraftEditor.Num(segment.EndElevation) + " in · "
                    + CantileverDraftEditor.Num(segment.EndElevation - segment.StartElevation) + " in " + (braced ? "con tensores" : "sin tensores"), braced));
            }

            SyncRecords(SequenceItems, items);
        }

        private void RaiseBracing()
        {
            foreach (var name in new[]
                     {
                         nameof(LayoutChoice), nameof(IsAdvanced), nameof(LayoutHint), nameof(DerivedText), nameof(PiecesText), nameof(SequenceTop),
                         nameof(SequenceMessage), nameof(SegmentsPending), nameof(CanEditSegments), nameof(CanApplySegments), nameof(ToAutomaticText),
                         nameof(RecoverText),
                     })
                OnPropertyChanged(name);
        }

        // ------------------------------------------------------------------ «Restaurar layout…» (CT-07)

        public bool CanRestore => !_writing;

        public bool IsRestoreOpen => _restoreOpen;

        public ObservableCollection<CantileverRestoreBoxItem> RestoreBoxes { get; } = new();

        public bool IsLossOpen => _lossOpen;

        public IReadOnlyList<string> LossLines => RestoreBoxes.Where(b => b.IsChecked && b.Count > 0).SelectMany(b => b.Losses).ToList();

        public string LossCountText => "Ver lo que se pierde (" + CantileverDraftEditor.Count(LossLines.Count, "valor", "valores") + ")";

        public bool CanRestoreMarked => !_writing && RestoreBoxes.Any(b => b.IsChecked && b.Count > 0);

        /// <summary>«Restaurar layout…»: the inline panel with its boxes, all marked (decision 8 of Selectivo).</summary>
        public void OpenRestore()
        {
            if (!CanRestore) return;
            RestoreBoxes.Clear();
            foreach (var box in _draft.RestoreBoxes()) RestoreBoxes.Add(new CantileverRestoreBoxItem(box.Key, box.Label, box.Count, box.Losses, RaiseRestore));
            _restoreOpen = true;
            _lossOpen = false;
            RaiseRestore();
        }

        public void ToggleLoss()
        {
            _lossOpen = !_lossOpen;
            RaiseRestore();
        }

        /// <summary>«Restaurar lo marcado».</summary>
        public void RestoreMarked()
        {
            if (!CanRestoreMarked) return;
            var keys = RestoreBoxes.Where(b => b.IsChecked && b.Count > 0).Select(b => b.Key).ToList();
            var refused = Commit(() => _draft.Restore(keys));
            if (refused != null)
            {
                Say(refused, error: true);
                return;
            }

            if (keys.Contains(CantileverDraftEditor.RestoreAdvanced)) ResetSegmentRows();
            _restoreOpen = false;
            // T6 ui (point 8): a restore that leaves the line as the drawing has it does not name «Descartar», which is then off.
            Say("Layout restaurado en el borrador: " + CantileverDraftEditor.Count(keys.Count, "casilla", "casillas") + "."
                + (CanDiscard ? " «Descartar» lo deshace." : string.Empty), error: false);
            Rebuild();
            RaiseRestore();
        }

        public void CancelRestore()
        {
            _restoreOpen = false;
            RaiseRestore();
        }

        private void RaiseRestore()
        {
            OnPropertyChanged(nameof(IsRestoreOpen));
            OnPropertyChanged(nameof(IsLossOpen));
            OnPropertyChanged(nameof(LossLines));
            OnPropertyChanged(nameof(LossCountText));
            OnPropertyChanged(nameof(CanRestoreMarked));
        }
    }
}
