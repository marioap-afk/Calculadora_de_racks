using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Systems.Shared;
using RackCad.Application.VentanaRackCad;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>The kind of a piece of the strip of the tramos (DG-11).</summary>
    public enum SelectiveSegmentStripKind
    {
        /// <summary>A tramo with largueros (blue).</summary>
        Loaded,

        /// <summary>A tramo with no largueros (striped).</summary>
        Empty,

        /// <summary>An intermediate post (grey).</summary>
        Post,
    }

    /// <summary>One piece of the strip of the tramos: a tramo, with its share of the front and its length, or an intermediate post.</summary>
    public sealed record SelectiveSegmentStripItem(SelectiveSegmentStripKind Kind, double Weight, string Text)
    {
        public bool IsLoaded => Kind == SelectiveSegmentStripKind.Loaded;

        public bool IsEmpty => Kind == SelectiveSegmentStripKind.Empty;

        public bool IsPost => Kind == SelectiveSegmentStripKind.Post;
    }

    /// <summary>
    /// The fourth batch of the window, part «ins» (I-64 A-14, line D; the prototype «dialogos», accepted by the Owner on 2026-11-26), in the
    /// Selectivo editor:
    /// <list type="bullet">
    /// <item>«Insertar vista…» opens the panel <see cref="RackViewInsertPanelViewModel"/> under the head of the editor instead of the classic
    /// dialog «Insertar varias vistas» as a modal of AutoCAD (DG-01, which replaces the provisional P3-01). The panel opens also when the
    /// views cannot be inserted now and says why (DG-05); only while AutoCAD writes this rack is the button off. It follows the draft
    /// (DG-08), and «Insertar N vistas» is the write of «Insertar vista…» as before (RACKSELECTIVOINSERTAR, or RACKSELECTIVONUEVO for a
    /// new Selectivo).</item>
    /// <item>The tramos of «medio frente» (DG-09 to DG-11): the «(resto)» discounts what the intermediate posts take, «Aplicar tramos» is
    /// off while the split does not fit, and a strip of the tramos, read only, sits over the table.</item>
    /// </list>
    /// </summary>
    public sealed partial class SelectiveEditorViewModel
    {
        private RackViewInsertPanelViewModel _insertPanel;
        private Func<IReadOnlyList<string>> _existingViewKinds;
        private Func<string> _otherRequestText;
        private string _segmentHint = string.Empty;

        // ------------------------------------------------------------------ «Insertar vista…»: the panel (DG-01 to DG-08)

        /// <summary>The panel «Insertar vista…», while it is open; null when it is closed.</summary>
        public RackViewInsertPanelViewModel InsertPanel => _insertPanel;

        public bool IsInsertPanelOpen => _insertPanel != null;

        /// <summary>«panel abierto» while the panel is open: the item status of the button «Insertar vista…» for UI Automation.</summary>
        public string InsertPanelStatus => IsInsertPanelOpen ? "panel abierto" : string.Empty;

        /// <summary>
        /// The button «Insertar vista…» (or «Insertar en el dibujo…») is on: the window gave the editor its insertion and its rack an
        /// identity, and AutoCAD is not writing it (DG-05: the panel opens also when the views cannot be inserted now).
        /// </summary>
        public bool CanOpenInsertPanel => string.IsNullOrEmpty(InsertPanelBlockedText);

        /// <summary>Why the button «Insertar vista…» is off; empty when it is on.</summary>
        public string InsertPanelBlockedText
        {
            get
            {
                var action = _isNewRack ? _insertNew : _insertViews;
                if (action == null || string.IsNullOrWhiteSpace(RackId))
                    return _isNewRack ? "Aquí no se puede insertar el rack nuevo." : "Aquí no se pueden insertar vistas del rack.";
                return _writing ? WritingText : string.Empty;
            }
        }

        /// <summary>The kinds of view the drawing already has for this rack, as the list of the window read them (DG-04); the window sets it.</summary>
        public Func<IReadOnlyList<string>> ExistingViewKinds
        {
            get => _existingViewKinds;
            set { _existingViewKinds = value; RefreshInsertPanel(); }
        }

        /// <summary>
        /// Why the door of the window cannot take a write now (another request waits for AutoCAD), or empty: the window sets it, and the panel
        /// says it before anything is asked (DG-05, dolor 9).
        /// </summary>
        public Func<string> OtherRequestText
        {
            get => _otherRequestText;
            set { _otherRequestText = value; RaiseState(); }
        }

        private string OtherRequestBlockedText => _otherRequestText?.Invoke() ?? string.Empty;

        /// <summary>The views «Insertar vista…» offers now: one frontal per fondo, one lateral per post with a cut, the plan.</summary>
        public IReadOnlyList<RackViewInsertOption> InsertOptions() => SelectiveViewChoice.Options(_editor.System, _editor.Catalog);

        // «Insertar vista…» (the button): the panel opens, with nothing marked for a rack of the drawing and with the first view of each group
        // for a new rack (DG-02). Asked again while it is open, it stays as it is. The inline question of «Cerrar el editor» goes.
        private void OpenInsertPanel()
        {
            if (_insertPanel != null) return;
            SetConfirmingClose(false);
            _insertPanel = new RackViewInsertPanelViewModel(RackSystemKind.SelectiveRack, InsertOptions(), InsertPanelContext(), InsertChosenViews,
                CloseInsertPanel);
            RaiseInsertPanel();
        }

        /// <summary>«Cancelar» of the panel: it closes and nothing is inserted.</summary>
        public void CloseInsertPanel()
        {
            if (_insertPanel == null) return;
            _insertPanel = null;
            RaiseInsertPanel();
        }

        /// <summary>
        /// The window or the editor changed something the panel shows (the draft, the state of the editor, the door): the panel follows it.
        /// While AutoCAD writes this rack the panel closes (the write took its views).
        /// </summary>
        public void RefreshInsertPanel()
        {
            if (_insertPanel == null) return;
            if (_writing)
            {
                CloseInsertPanel();
                return;
            }

            _insertPanel.Follow(InsertOptions(), InsertPanelContext());
        }

        // The state of the editor changed: the texts of the panel (why it cannot insert, whether there are changes), not its views.
        private void UpdateInsertPanel()
        {
            if (_insertPanel == null) return;
            if (_writing)
            {
                CloseInsertPanel();
                return;
            }

            _insertPanel.Update(InsertPanelContext());
        }

        /// <summary>The window: the door changed (another request waits for AutoCAD, or not); the button and the open panel say it.</summary>
        public void RefreshInsertState()
        {
            OnPropertyChanged(nameof(CanInsertView));
            OnPropertyChanged(nameof(InsertViewBlockedText));
            RaiseStateIns();
        }

        // From RaiseState: what follows the state of the editor (writing, changes, the door).
        private void RaiseStateIns()
        {
            OnPropertyChanged(nameof(CanOpenInsertPanel));
            OnPropertyChanged(nameof(InsertPanelBlockedText));
            OnPropertyChanged(nameof(CanApplySegments));
            UpdateInsertPanel();
        }

        // «Insertar N vistas»: the write of «Insertar vista…», or of «Insertar en el dibujo…» for a new Selectivo, with the views of the panel.
        private void InsertChosenViews(IReadOnlyList<RackViewAddress> views)
        {
            if (views == null || views.Count == 0) return;
            if (!CanInsertView)
            {
                Say(InsertViewBlockedText, error: true);
                return;
            }

            (_isNewRack ? _insertNew : _insertViews)(this, views);
        }

        private RackViewInsertPanelContext InsertPanelContext() => new RackViewInsertPanelContext(
            Name,
            _isNewRack,
            "Selectivo",
            _dirty,
            InsertViewBlockedText,
            _existingViewKinds?.Invoke(),
            new RackViewInsertGroupTexts(
                _editor.FondoCount > 1 ? "Una por fondo." : "Una por fondo; este rack tiene uno.",
                "Un corte por poste; puedes marcar varios.",
                "Todo el rack."));

        private void RaiseInsertPanel()
        {
            OnPropertyChanged(nameof(InsertPanel));
            OnPropertyChanged(nameof(IsInsertPanelOpen));
            OnPropertyChanged(nameof(InsertPanelStatus));
            OnPropertyChanged(nameof(CanOpenInsertPanel));
            OnPropertyChanged(nameof(InsertPanelBlockedText));
        }

        // ------------------------------------------------------------------ the tramos of «medio frente» (DG-09 to DG-11)

        /// <summary>
        /// The split written in the inspector, as the resolver would draw it in the fondos of the view (DG-10): the width of the front, what
        /// each intermediate post takes, and what is left for the last tramo. Unknown with no single front or no recomposition yet.
        /// </summary>
        public SelectiveSegmentFitResult SegmentFit
        {
            get
            {
                if (!SegmentFront.HasValue || _segmentRows.Count == 0) return new SelectiveSegmentFitResult(-1, 0.0, 0.0, 0, 0.0, 0.0);
                var lengths = _segmentRows.Where(r => !r.IsRest).Select(WrittenLength).ToList();
                return SelectiveSegmentFit.Of(_editor.System, _editor.Catalog, _editor.ScopeFondos, SegmentFront.Value, lengths, _segmentRows.Count);
            }
        }

        /// <summary>The split written does not fit (DG-10): what is missing, in red; empty when it fits or it is not known.</summary>
        public string SegmentFitText => _segmentRows.Count >= 2 ? SelectiveSegmentFit.RefusalOf(SegmentFit, _editor.FondoCount > 1) ?? string.Empty : string.Empty;

        /// <summary>
        /// «Aplicar tramos» (DG-09: the one explicit gesture): there are tramos written and not applied, at least two (one tramo is a whole
        /// front: «Sin medio frente»), and they fit the front with its intermediate posts (DG-10).
        /// </summary>
        public bool CanApplySegments => CanChangeSegments && _segmentsPending && _segmentRows.Count >= 2 && string.IsNullOrEmpty(SegmentFitText);

        /// <summary>What the last gesture of the tramos did (the prototype: the seed, a tramo added, the rest that changes, dolores 15 and 16).</summary>
        public string SegmentHint => _segmentHint;

        /// <summary>With two fondos or more: the fondos of the view where the tramos are written (dolor 12).</summary>
        public string SegmentWhereText
        {
            get
            {
                if (_editor.FondoCount < 2 || !SegmentFront.HasValue) return string.Empty;
                var front = SegmentFront.Value;
                var fondos = _editor.ScopeFondos.Where(k => _editor.Front(k, front) != null).Select(k => (k + 1).ToString(CultureInfo.InvariantCulture)).ToList();
                if (fondos.Count == 0) return string.Empty;
                var where = fondos.Count == 1 ? "del fondo " + fondos[0] : "de los fondos " + RackViewInsertPanelViewModel.JoinY(fondos);
                return "Se escribe en F" + (front + 1).ToString(CultureInfo.InvariantCulture) + " " + where + ": los fondos de la vista que lo tienen.";
            }
        }

        /// <summary>DG-10: the «(resto)» discounts what each intermediate post takes (the catalog: the troquel of the post and the ménsula).</summary>
        public string SegmentPostNote
        {
            get
            {
                if (_segmentRows.Count < 2) return string.Empty;
                var fit = SegmentFit;
                return fit.IsKnown && fit.PostOverhead > 0.0
                    ? "El «(resto)» descuenta lo que ocupa cada poste intermedio: ≈ " + Num(fit.PostOverhead)
                      + " in (el troquel del poste y el arranque de la ménsula del larguero, a cada lado)."
                    : "El «(resto)» descuenta lo que ocupa cada poste intermedio.";
            }
        }

        /// <summary>DG-11: the strip of the tramos over the table, read only: the tramos with largueros, the empty ones and the posts between.</summary>
        public IReadOnlyList<SelectiveSegmentStripItem> SegmentStrip
        {
            get
            {
                var fit = SegmentFit;
                if (_segmentRows.Count < 2 || !fit.IsKnown) return Array.Empty<SelectiveSegmentStripItem>();
                var items = new List<SelectiveSegmentStripItem>();
                for (var i = 0; i < _segmentRows.Count; i++)
                {
                    var row = _segmentRows[i];
                    if (i > 0) items.Add(new SelectiveSegmentStripItem(SelectiveSegmentStripKind.Post, 0.0, string.Empty));
                    var value = WrittenLength(row);
                    var length = row.IsRest ? Math.Max(0.0, fit.Remainder) : value;
                    var text = row.IsRest ? "≈ " + Num(Math.Max(0.0, fit.Remainder)) + " in" : value > 0.0 ? Num(length) + " in" : "¿?";
                    items.Add(new SelectiveSegmentStripItem(row.Loaded ? SelectiveSegmentStripKind.Loaded : SelectiveSegmentStripKind.Empty,
                        Math.Max(length, fit.Width / 40.0) / fit.Width, text));
                }

                return items;
            }
        }

        public bool HasSegmentStrip => SegmentStrip.Count > 0;

        /// <summary>The strip for UI Automation: «F2: tramo 1 de 44 in, con largueros; poste intermedio; tramo 2 (resto) de ≈ 48.5 in, sin largueros.».</summary>
        public string SegmentStripText
        {
            get
            {
                if (!HasSegmentStrip) return string.Empty;
                var fit = SegmentFit;
                var parts = _segmentRows.Select((row, i) =>
                {
                    var value = WrittenLength(row);
                    var length = row.IsRest ? " (resto) de ≈ " + Num(Math.Max(0.0, fit.Remainder)) + " in" : value > 0.0 ? " de " + Num(value) + " in" : " de longitud sin escribir";
                    return "tramo " + (i + 1).ToString(CultureInfo.InvariantCulture) + length + ", " + (row.Loaded ? "con largueros" : "sin largueros");
                });
                return "F" + (SegmentFront.Value + 1).ToString(CultureInfo.InvariantCulture) + ": " + string.Join("; poste intermedio; ", parts) + ".";
            }
        }

        /// <summary>The split written does not fit: the strip is drawn with the border of an error.</summary>
        public bool SegmentStripDoesNotFit => !string.IsNullOrEmpty(SegmentFitText);

        // The length written in a row of a tramo that is not the rest: a number > 0, else 0 (not written yet, or not a number).
        private static double WrittenLength(SelectiveSegmentRow row)
            => !row.IsRest && UiSupport.TryNum(row.LengthText, out var value) && value > 0.0 && !double.IsInfinity(value) ? value : 0.0;

        // The gestures of the tramos say what they did (the prototype, dolores 15 and 16); writing a length or «con largueros» clears it.
        private void SegmentGesture(string hint)
        {
            _segmentHint = hint ?? string.Empty;
            OnPropertyChanged(nameof(SegmentHint));
        }

        private void SegmentRemovedGesture(int index, int countBefore, string removedPreviousLength)
        {
            var hint = string.Empty;
            var wasLast = index == countBefore - 1;
            if (wasLast && index > 0 && !string.IsNullOrWhiteSpace(removedPreviousLength))
                hint = "El tramo " + index.ToString(CultureInfo.InvariantCulture) + " pasa a ser el resto: su longitud (" + removedPreviousLength.Trim() + " in) ya no cuenta.";
            var left = countBefore - 1;
            if (left == 1) hint = (hint.Length > 0 ? hint + " " : string.Empty) + "Con un solo tramo el frente es completo: agrega otro tramo, o usa «Sin medio frente».";
            if (left == 0) hint = "Sin tramos: para dejar el frente completo, «Sin medio frente»; para volver a lo de antes, «Descartar tramos».";
            SegmentGesture(hint);
        }

        // The strip, the fit and the notes follow every change of the rows, of the front and of the draft.
        private void RaiseSegmentsIns()
        {
            OnPropertyChanged(nameof(SegmentFit));
            OnPropertyChanged(nameof(SegmentFitText));
            OnPropertyChanged(nameof(CanApplySegments));
            OnPropertyChanged(nameof(SegmentWhereText));
            OnPropertyChanged(nameof(SegmentPostNote));
            OnPropertyChanged(nameof(SegmentStrip));
            OnPropertyChanged(nameof(HasSegmentStrip));
            OnPropertyChanged(nameof(SegmentStripText));
            OnPropertyChanged(nameof(SegmentStripDoesNotFit));
        }

        // «(resto) ≈ n in» of the last tramo (DG-10): the width of the front less the tramos written above it AND what the intermediate posts
        // take, as the resolver draws it; 0 when nothing is left (the split does not fit and the line in red says so).
        private string RestTextOf()
        {
            var fit = SegmentFit;
            return fit.IsKnown ? "(resto) ≈ " + Num(Math.Max(0.0, fit.Remainder)) + " in" : "(resto)";
        }
    }
}
