using System;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// «Más» of the Selectivo editor and its entry «Guardar en biblioteca…» (I-64 A-14, line D, fourth batch, part «ut», UT-04; prototype
    /// «utilidades»): the window takes the user to «Biblioteca de diseños» with a line for the name, without the dialog of Windows. What is
    /// saved is what the editor shows, the draft included (the last valid recomposition); nothing here writes the drawing or a file. «Más»
    /// opens its entries under the button, inline (no popup: O-D of the smoke of Selectivo), so the other parts can add theirs there.
    /// </summary>
    public sealed partial class SelectiveEditorViewModel
    {
        private bool _moreOpen;
        private Action<SelectiveEditorViewModel> _saveToLibrary;

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
        public Action<SelectiveEditorViewModel> SaveToLibraryAction
        {
            get => _saveToLibrary;
            set
            {
                _saveToLibrary = value;
                RaiseMore();
            }
        }

        public bool CanSaveToLibrary => string.IsNullOrEmpty(SaveToLibraryBlockedText);

        /// <summary>Why «Guardar en biblioteca…» cannot run now; empty when it can. Writing to AutoCAD does not stop it: the draft does not change meanwhile.</summary>
        public string SaveToLibraryBlockedText
        {
            get
            {
                if (_saveToLibrary == null) return "Aquí no se puede guardar en la biblioteca.";
                if (_loadError != null) return LoadFailedText;
                if (_segmentsPending) return "Aplica los tramos con «Aplicar tramos» o descártalos antes de guardar en la biblioteca.";
                if (_editor.Design == null) return "No hay un borrador válido que guardar.";
                if (!string.IsNullOrEmpty(_editor.LastError)) return _editor.LastError;
                return string.Empty;
            }
        }

        /// <summary>
        /// «Guardar en biblioteca…»: a value written and refused in a field stops it and is named (as «Restaurar layout…» and «Editar
        /// seguridad…», the frontier CommitPendingEditors of the classic save); otherwise the window shows the line of the save.
        /// </summary>
        public void SaveToLibrary()
        {
            var pending = PendingFieldError("No se puede guardar en la biblioteca");
            if (pending != null)
            {
                Say(pending, error: true);
                return;
            }

            if (!CanSaveToLibrary)
            {
                Say(SaveToLibraryBlockedText, error: true);
                return;
            }

            IsMoreOpen = false;
            _saveToLibrary(this);
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
