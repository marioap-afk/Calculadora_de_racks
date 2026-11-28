using System;
using System.Linq;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// «Más» of the Dinámico editor and its entry «Guardar en biblioteca…» (I-64 A-14, line D, fourth batch, part «ut», UT-04: one label,
    /// «Guardar en biblioteca», also for the «Guardar proyecto» of the classic Dinámico): the window takes the user to «Biblioteca de diseños»
    /// with a line for the name, without the dialog of Windows. What is saved is what the editor shows, the draft included. As in the
    /// Selectivo editor, «Más» opens its entries under the button, inline.
    /// </summary>
    public sealed partial class DynamicEditorViewModel
    {
        private bool _moreOpen;
        private Action<DynamicEditorViewModel> _saveToLibrary;

        /// <summary>Whether the entries of «Más» are shown.</summary>
        public bool IsMoreOpen
        {
            get => _moreOpen;
            set
            {
                if (!SetProperty(ref _moreOpen, value)) return;
                RaiseMore();
            }
        }

        /// <summary>The state of «Más» for UI Automation.</summary>
        public string MoreItemStatus => _moreOpen ? "abierto" : "cerrado";

        public void ToggleMore() => IsMoreOpen = !IsMoreOpen;

        /// <summary>What «Guardar en biblioteca…» does with this editor (the window shows the page of the library); null when it is not offered.</summary>
        public Action<DynamicEditorViewModel> SaveToLibraryAction
        {
            get => _saveToLibrary;
            set
            {
                _saveToLibrary = value;
                RaiseMore();
            }
        }

        public bool CanSaveToLibrary => string.IsNullOrEmpty(SaveToLibraryBlockedText);

        /// <summary>Why «Guardar en biblioteca…» cannot run now; empty when it can.</summary>
        public string SaveToLibraryBlockedText
        {
            get
            {
                if (_saveToLibrary == null) return "Aquí no se puede guardar en la biblioteca.";
                if (_editor.Design == null) return "No hay un borrador válido que guardar.";
                if (!string.IsNullOrEmpty(_editor.LastError)) return _editor.LastError;
                return string.Empty;
            }
        }

        /// <summary>«Guardar en biblioteca…»: a value written and refused in a field stops it and is named; otherwise the window shows the line of the save.</summary>
        public void SaveToLibrary()
        {
            var refusal = SaveRefusal();
            if (refusal != null)
            {
                Say(refusal, error: true);
                return;
            }

            IsMoreOpen = false;
            _saveToLibrary(this);
        }

        /// <summary>
        /// Why the draft cannot be saved now, or null: a value written and refused in a field, then <see cref="SaveToLibraryBlockedText"/>.
        /// «Más» asks it, and the line of the library asks it again at «Guardar» and «Reemplazar» (review of part «ut», point 1).
        /// </summary>
        public string SaveRefusal()
        {
            var refused = new[] { _essentials, _postFields, _cellFields, _frontFields, _advancedFields, _bedFields }
                .SelectMany(fields => fields)
                .FirstOrDefault(field => field != null && !string.IsNullOrWhiteSpace(field.Error));
            if (refused != null)
                return "No se puede guardar en la biblioteca: «" + refused.Label + "» tiene un valor que no se aplicó (" + refused.Error.Trim().TrimEnd('.')
                       + "). Corrígelo o vuelve a escribir el que tenía.";
            var blocked = SaveToLibraryBlockedText;
            return string.IsNullOrEmpty(blocked) ? null : blocked;
        }

        private void RaiseMore()
        {
            OnPropertyChanged(nameof(IsMoreOpen));
            OnPropertyChanged(nameof(MoreItemStatus));
            OnPropertyChanged(nameof(CanSaveToLibrary));
            OnPropertyChanged(nameof(SaveToLibraryBlockedText));
        }
    }
}
