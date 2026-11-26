using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.VentanaRackCad;
using RackCad.Domain.Systems.Selective;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>One row of «Seguridad por poste» of a Selectivo: the levels of the desviador and the side of the bota, read only.</summary>
    public sealed class SelectivePostSafetyItem
    {
        internal SelectivePostSafetyItem(SelectivePostSafetyRow row)
        {
            Post = "P" + (row.PostIndex + 1).ToString(CultureInfo.InvariantCulture);
            DeflectorLevels = row.DeflectorLevels.Count == 0
                ? "—"
                : string.Join(", ", row.DeflectorLevels.Select(level => "N" + (level + 1).ToString(CultureInfo.InvariantCulture)));
            BootSide = string.IsNullOrEmpty(row.BootSide) ? "—" : row.BootSide;
        }

        public string Post { get; }

        public string DeflectorLevels { get; }

        public string BootSide { get; }
    }

    /// <summary>«Extremos y seguridad» of the Selectivo editor (entrega 2, part C): T/P on the selected cells, for the whole rack, and the table by post.</summary>
    public sealed partial class SelectiveEditorViewModel
    {
        private Func<SelectiveDraftEditor, IReadOnlyList<SelectiveSafetySelection>> _safetyEditor;

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
            }
        }

        public bool CanEditSafety => CanEdit && _safetyEditor != null;

        /// <summary>«Seguridad por poste», counted with the plan the BOM uses.</summary>
        public IReadOnlyList<SelectivePostSafetyItem> PostSafety
            => SelectiveSafetyView.PostRows(_editor).Select(row => new SelectivePostSafetyItem(row)).ToList();

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

        /// <summary>T on or off at the selected cells, for the whole rack; returns the reason when it was not written.</summary>
        public string SetTope(bool on) => SetSafetyCells(() => SelectiveSafetyView.WriteTope(_editor, SelectedPositions, on));

        /// <summary>P on or off at the selected cells, for the whole rack; returns the reason when it was not written.</summary>
        public string SetParrilla(bool on) => SetSafetyCells(() => SelectiveSafetyView.WriteParrilla(_editor, SelectedPositions, on));

        /// <summary>«Editar seguridad…»: the classic dialog; its result replaces the safety of the draft, which is recomposed.</summary>
        public void EditSafety()
        {
            if (!CanEditSafety)
            {
                Say(_safetyEditor == null
                    ? "El diálogo de seguridad no está disponible aquí: usa «Abrir en editor clásico»."
                    : WritingText, error: true);
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

        private string SetSafetyCells(Func<SelectiveDraftEdit> write)
        {
            var error = Apply(write);
            if (error != null) Say(error, error: true);
            OnSafetyChanged();
            return error;
        }

        private void OnSafetyChanged()
        {
            OnPropertyChanged(nameof(SelectionTope));
            OnPropertyChanged(nameof(SelectionParrilla));
            OnPropertyChanged(nameof(PostSafety));
        }
    }
}
