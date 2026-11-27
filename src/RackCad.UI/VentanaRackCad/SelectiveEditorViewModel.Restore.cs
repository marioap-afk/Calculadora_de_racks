using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.VentanaRackCad;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// One box of «Restaurar layout…» of a Selectivo (decision 8 of the Owner): its name with how many values it would remove («Largo de
    /// larguero (3)», or «(ninguno)», disabled), checked by default. An unchecked box keeps that personalisation.
    /// </summary>
    public sealed class SelectiveRestoreBoxItem : ObservableObject
    {
        private readonly Action _changed;
        private bool _isChecked = true;
        private int _count;
        private bool _isBlocked;

        internal SelectiveRestoreBoxItem(SelectiveLayoutRestoreBox box, string title, Action changed)
        {
            Box = box;
            Title = title;
            _changed = changed;
        }

        public SelectiveLayoutRestoreBox Box { get; }

        /// <summary>The name of the box, as decision 8 lists it.</summary>
        public string Title { get; }

        /// <summary>How many values the box would remove in the scope (for the own peraltes blocked by the scope, how many the rack has).</summary>
        public int Count => _count;

        /// <summary>«Largo de larguero (3)»; «Largo de larguero (ninguno)» when it has nothing to remove.</summary>
        public string Label => Title + (_count == 0 ? " (ninguno)" : " (" + _count.ToString(CultureInfo.InvariantCulture) + ")");

        /// <summary>Checked = restore it. Every box is checked when the panel opens, the disabled ones too (they remove nothing).</summary>
        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                if (SetProperty(ref _isChecked, value)) _changed?.Invoke();
            }
        }

        /// <summary>Only a box with something to remove can be changed; the own peraltes, only when the scope is the whole rack.</summary>
        public bool IsEnabled => _count > 0 && !_isBlocked;

        internal void Set(int count, bool blocked, bool? isChecked)
        {
            var wasBlocked = _isBlocked;
            _count = count;
            _isBlocked = blocked;
            if (blocked) _isChecked = false;
            else if (isChecked.HasValue) _isChecked = isChecked.Value;
            else if (wasBlocked) _isChecked = true;
            OnPropertyChanged(nameof(Count));
            OnPropertyChanged(nameof(Label));
            OnPropertyChanged(nameof(IsChecked));
            OnPropertyChanged(nameof(IsEnabled));
        }
    }

    /// <summary>
    /// «Restaurar layout…» of the Selectivo editor (entrega 2, P2; decision 8 of the Owner; spec <c>sprint\spec-restaurar.md</c>): the button
    /// of the head opens a panel inline under it, as the confirmation of Dinámico and the question of closing, with the six boxes, every one
    /// checked when it opens, how many values each one removes, «Ver lo que se pierde» and «Restaurar lo marcado» / «Cancelar». It goes to
    /// the draft (the drawing changes with «Actualizar»), and «Descartar» goes back to what was last read or written (a new Selectivo of
    /// «Nuevo», to the defaults).
    /// </summary>
    public sealed partial class SelectiveEditorViewModel
    {
        private const string NothingManualText = "No hay nada manual que quitar: el layout ya es el que sale de las tarimas.";
        private const string RestorePeralteBlockedText = "Los peraltes propios son de todo el rack: elige «Todos los fondos» para restaurarlos.";

        private bool _restoreOpen;
        private bool _restoreLossesExpanded;
        private IReadOnlyList<SelectiveRestoreBoxItem> _restoreBoxes;
        private IReadOnlyList<SelectiveLayoutRestoreLoss> _restoreAllLosses = Array.Empty<SelectiveLayoutRestoreLoss>();
        private IReadOnlyList<string> _restoreLosses = Array.Empty<string>();
        private string _restorePeralteNote = string.Empty;

        /// <summary>
        /// SUPUESTO PROVISIONAL (encargo de la entrega 2, P2; pregunta abierta al Owner, spec §10): «Restaurar layout…» trabaja sobre TODO el
        /// rack, como el draft aprobado, y no sobre el selector de fondos de la matriz; los peraltes propios se restauran entonces siempre.
        /// Con <c>true</c> sigue el selector («Fondo n» = solo ese fondo, y los peraltes propios solo con «Todos los fondos», regla I-43):
        /// cambiar el supuesto es cambiar este valor por defecto.
        /// </summary>
        internal bool RestoreFollowsFondoView { get; set; }

        // The fondos «Restaurar layout…» works on: null = every fondo (the supuesto), else the fondo of the view.
        private int? RestoreScope => RestoreFollowsFondoView ? _editor.FondoScope : null;

        /// <summary>The boxes of «Restaurar layout…», all checked by default; an unchecked box keeps that personalisation.</summary>
        public SelectiveLayoutRestoreOptions RestoreOptions { get; set; } = SelectiveLayoutRestoreOptions.All;

        /// <summary>The panel of «Restaurar layout…» is open under the head of the editor.</summary>
        public bool IsRestoreOpen => _restoreOpen;

        /// <summary>The six boxes of decision 8, in its order.</summary>
        public IReadOnlyList<SelectiveRestoreBoxItem> RestoreBoxes => _restoreBoxes ??= new[]
        {
            new SelectiveRestoreBoxItem(SelectiveLayoutRestoreBox.BeamLength, "Largo de larguero", OnRestoreBoxChanged),
            new SelectiveRestoreBoxItem(SelectiveLayoutRestoreBox.Clear, "Claro", OnRestoreBoxChanged),
            new SelectiveRestoreBoxItem(SelectiveLayoutRestoreBox.HalfFront, "Medio frente", OnRestoreBoxChanged),
            new SelectiveRestoreBoxItem(SelectiveLayoutRestoreBox.ManualHeight, "Altura manual", OnRestoreBoxChanged),
            new SelectiveRestoreBoxItem(SelectiveLayoutRestoreBox.PostPeralte, "Peraltes propios", OnRestoreBoxChanged),
            new SelectiveRestoreBoxItem(SelectiveLayoutRestoreBox.CustomCabecera, "Cabeceras personalizadas", OnRestoreBoxChanged),
        };

        /// <summary>Where it restores: «el rack» with one fondo, «todos los fondos», or «el fondo n» when it follows the selector.</summary>
        public string RestoreScopeText
        {
            get
            {
                if (_editor.FondoCount <= 1) return "el rack";
                return RestoreScope is int fondo ? "el fondo " + (fondo + 1).ToString(CultureInfo.InvariantCulture) : "todos los fondos";
            }
        }

        public string RestoreIntro => "Vuelve a la geometría que sale de las tarimas en " + RestoreScopeText + ". Marca lo que quieres restaurar:";

        /// <summary>«Ver lo que se pierde»: one line per value, only of the boxes checked.</summary>
        public IReadOnlyList<string> RestoreLosses => _restoreLosses;

        public bool HasRestoreLosses => _restoreLosses.Count > 0;

        /// <summary>«Ver lo que se pierde» is folded every time the panel opens.</summary>
        public bool IsRestoreLossesExpanded
        {
            get => _restoreLossesExpanded;
            set => SetProperty(ref _restoreLossesExpanded, value);
        }

        /// <summary>Why the box of the own peraltes is disabled (only when the scope is one fondo of several); empty otherwise.</summary>
        public string RestorePeralteNote => _restorePeralteNote;

        /// <summary>«Restaurar lo marcado» needs a checked box with something to remove.</summary>
        public bool CanConfirmRestore => _restoreOpen && CanEdit && RestoreBoxes.Any(b => b.IsChecked && b.IsEnabled);

        /// <summary>
        /// «Restaurar layout…»: opens the panel with every box checked and closes the question of closing (and the question closes the
        /// panel), as the approved draft does. With nothing manual in the scope the panel does not open and the editor says so. A value
        /// written and refused stops it first (the frontier of <c>CommitPendingEditors</c> of the classic editor).
        /// </summary>
        public void OpenRestore()
        {
            if (!CanEdit)
            {
                Say(BlockedEditText, error: true);
                return;
            }

            var pending = PendingFieldError();
            if (pending != null)
            {
                Say(pending, error: true);
                return;
            }

            var measured = MeasureRestore();
            if (measured.Losses.Count == 0 && !measured.Preview.PostPeraltesSkipped)
            {
                CloseRestore();
                Say(NothingManualText, error: false);
                return;
            }

            SetConfirmingClose(false);
            ShowRestore(measured, everyBoxChecked: true);
            _restoreOpen = true;
            IsRestoreLossesExpanded = false;
            Say(string.Empty, error: false);
            RaiseRestore();
        }

        /// <summary>«Restaurar lo marcado»: restores the boxes checked, closes the panel and says what it removed and what it kept.</summary>
        public void ConfirmRestore()
        {
            if (!_restoreOpen) return;
            if (!CanEdit)
            {
                Say(BlockedEditText, error: true);
                return;
            }

            if (!CanConfirmRestore) return;
            var pending = PendingFieldError();
            if (pending != null)
            {
                Say(pending, error: true);
                return;
            }

            bool On(SelectiveLayoutRestoreBox box) => RestoreBoxes.First(b => b.Box == box).IsChecked;
            RestoreOptions = new SelectiveLayoutRestoreOptions(
                On(SelectiveLayoutRestoreBox.BeamLength),
                On(SelectiveLayoutRestoreBox.Clear),
                On(SelectiveLayoutRestoreBox.HalfFront),
                On(SelectiveLayoutRestoreBox.ManualHeight),
                On(SelectiveLayoutRestoreBox.PostPeralte),
                On(SelectiveLayoutRestoreBox.CustomCabecera));
            CloseRestore();
            RestoreLayout();
        }

        /// <summary>«Cancelar»: the panel closes and the draft stays as it is.</summary>
        public void CancelRestore() => CloseRestore();

        /// <summary>
        /// «Restaurar»: the checked personalisations go back to automatic in the fondos of <see cref="RestoreScope"/>, with ONE recomposition
        /// for all of them, and the notice says how many values it removed, which, and what the unchecked boxes keep. A value written and
        /// refused in a field stops it, and the editor says which.
        /// </summary>
        public string RestoreLayout()
        {
            var pending = PendingFieldError();
            if (pending != null)
            {
                Say(pending, error: true);
                return pending;
            }

            SelectiveLayoutRestoreResult result = null;
            var options = RestoreOptions ?? SelectiveLayoutRestoreOptions.All;
            var error = Apply(() =>
            {
                result = _editor.WithinScope(RestoreScope, () => SelectiveLayoutRestore.Apply(_editor, options));
                return Done;
            });
            // A new Selectivo of «Nuevo» was never read nor written: its «Descartar» goes back to the defaults (M-4 of the review of E2).
            // T6 ui (point 8): a restore that leaves the draft as the drawing has it does not name «Descartar», which is then off.
            if (error == null && result != null) Say(result.NoticeFor(_isNewRack, CanDiscard, _isCopy), error: false); // T6 ui: and a copy
            return error;
        }

        // Every recomposition (Rebuild) refreshes the open panel: the counts and the lines follow the draft, the boxes keep what the user
        // chose. With nothing left to remove, or while the draft cannot be edited (AutoCAD writes it), the panel closes.
        private void RefreshRestore()
        {
            if (!_restoreOpen) return;
            if (!CanEdit)
            {
                CloseRestore();
                return;
            }

            var measured = MeasureRestore();
            if (measured.Losses.Count == 0 && !measured.Preview.PostPeraltesSkipped)
            {
                CloseRestore();
                return;
            }

            ShowRestore(measured, everyBoxChecked: false);
            RaiseRestore();
        }

        private void CloseRestore()
        {
            if (!_restoreOpen) return;
            _restoreOpen = false;
            RaiseRestore();
        }

        // What the boxes would remove (with every box checked) and the lines of «Ver lo que se pierde», in the scope of «Restaurar».
        private (SelectiveLayoutRestoreResult Preview, IReadOnlyList<SelectiveLayoutRestoreLoss> Losses) MeasureRestore()
            => _editor.WithinScope(RestoreScope, () => (SelectiveLayoutRestore.Preview(_editor, SelectiveLayoutRestoreOptions.All), SelectiveLayoutRestore.Losses(_editor)));

        private void ShowRestore((SelectiveLayoutRestoreResult Preview, IReadOnlyList<SelectiveLayoutRestoreLoss> Losses) measured, bool everyBoxChecked)
        {
            var blocked = measured.Preview.PostPeraltesSkipped;
            foreach (var box in RestoreBoxes)
            {
                if (box.Box == SelectiveLayoutRestoreBox.PostPeralte && blocked)
                    box.Set(Enumerable.Range(0, _editor.PostCount).Count(p => _editor.PostPeralteOverride(p).HasValue), blocked: true, isChecked: false);
                else
                    box.Set(measured.Preview.Count(box.Box), blocked: false, everyBoxChecked ? true : (bool?)null);
            }

            _restorePeralteNote = blocked ? RestorePeralteBlockedText : string.Empty;
            _restoreAllLosses = measured.Losses;
            _restoreLosses = CheckedLosses();
        }

        private void OnRestoreBoxChanged()
        {
            _restoreLosses = CheckedLosses();
            OnPropertyChanged(nameof(RestoreLosses));
            OnPropertyChanged(nameof(HasRestoreLosses));
            OnPropertyChanged(nameof(CanConfirmRestore));
        }

        private IReadOnlyList<string> CheckedLosses()
        {
            var on = RestoreBoxes.Where(b => b.IsChecked && b.IsEnabled).Select(b => b.Box).ToHashSet();
            return _restoreAllLosses.Where(loss => on.Contains(loss.Box)).Select(loss => loss.Text).ToList();
        }

        private void RaiseRestore()
        {
            OnPropertyChanged(nameof(IsRestoreOpen));
            OnPropertyChanged(nameof(RestoreScopeText));
            OnPropertyChanged(nameof(RestoreIntro));
            OnPropertyChanged(nameof(RestoreLosses));
            OnPropertyChanged(nameof(HasRestoreLosses));
            OnPropertyChanged(nameof(RestorePeralteNote));
            OnPropertyChanged(nameof(CanConfirmRestore));
        }

        // A field that holds a value written and refused (its error is shown under it): a recomposition would put the value of the draft back
        // in the box, so «Restaurar» stops and names it (spec §5: the frontier of CommitPendingEditors of the classic editor). «Editar
        // seguridad…» stops the same way, with its own lead (M-2 of the review of the entrega 2).
        private string PendingFieldError(string refused = "No se puede restaurar el layout")
        {
            var fields = _essentials
                .Concat(_fondoRows.SelectMany(row => row.Fields))
                .Concat(_annotationFields)
                .Concat(_cellFields)
                .Concat(_palletFields)
                .Concat(_beamFields)
                .Concat(_manualFields)
                .Concat(_frontFields)
                .Concat(_postRows.Select(row => row.Field))
                .Where(item => item != null)
                .Distinct();
            foreach (var item in fields)
            {
                var (label, error) = item switch
                {
                    DynamicFieldItem field => (field.Label, field.Error),
                    SelectiveChoiceItem choice => (choice.Label, choice.Error),
                    _ => (null, null),
                };
                if (!string.IsNullOrWhiteSpace(error))
                    return refused + ": «" + label + "» tiene un valor que no se aplicó (" + error.Trim().TrimEnd('.') + "). "
                           + "Corrígelo o vuelve a escribir el que tenía.";
            }

            return null;
        }
    }
}
