using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Persistence;
using RackCad.Application.VentanaRackCad;
using RackCad.Domain.Systems.PushBack;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// T5 pb: what the Push Back editor shares with the others, in a file of its own: «Más» › «Guardar en biblioteca…» (UT-04), the tab
    /// «Propiedades» (VA-07), the base of «Modificado» for «Duplicar» of the drafts of «Nuevo» (SD-06), and «Restaurar layout…» with its
    /// boxes (question 6 of the prototype).
    /// </summary>
    public sealed partial class PushBackEditorViewModel
    {
        // ------------------------------------------------------------------ «Más» › «Guardar en biblioteca…»

        private bool _moreOpen;
        private Action<PushBackEditorViewModel> _saveToLibrary;

        public bool IsMoreOpen
        {
            get => _moreOpen;
            set
            {
                if (!SetProperty(ref _moreOpen, value)) return;
                RaiseMore();
            }
        }

        public string MoreItemStatus => _moreOpen ? "abierto" : "cerrado";

        public void ToggleMore() => IsMoreOpen = !IsMoreOpen;

        /// <summary>What «Guardar en biblioteca…» does with this editor (the window shows the page of the library); null when it is not offered.</summary>
        public Action<PushBackEditorViewModel> SaveToLibraryAction
        {
            get => _saveToLibrary;
            set
            {
                _saveToLibrary = value;
                RaiseMore();
            }
        }

        public bool CanSaveToLibrary => string.IsNullOrEmpty(SaveToLibraryBlockedText);

        /// <summary>Why «Guardar en biblioteca…» cannot run now; empty when it can (the gate of the classic editor: a blocked rack is not saved).</summary>
        public string SaveToLibraryBlockedText
        {
            get
            {
                if (_saveToLibrary == null) return "Aquí no se puede guardar en la biblioteca.";
                if (_draft.Design == null) return "No hay un borrador válido que guardar.";
                if (!string.IsNullOrEmpty(_draft.LastError)) return _draft.LastError;
                if (_draft.IsOutputBlocked) return "No se puede guardar: " + _draft.OutputStatus;
                return string.Empty;
            }
        }

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

        /// <summary>Why the draft cannot be saved now, or null: a value written and refused in a field, then <see cref="SaveToLibraryBlockedText"/>.</summary>
        public string SaveRefusal()
        {
            var refused = new[] { _essentials, _twoWayFields, _postFields, _cellFields, _depthFields, _beamFields, _frontFields, _advancedFields, _topeFields }
                .SelectMany(fields => fields)
                .Concat(_modules.Where(m => m.LengthField != null).Select(m => m.LengthField))
                .Concat(_structure.Select(s => s.Override))
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

        // ------------------------------------------------------------------ «Propiedades» (VA-07)

        private RackPropertiesTabViewModel _propertiesTab;

        public RackPropertiesTabViewModel PropertiesTab
        {
            get
            {
                if (_propertiesTab != null) return _propertiesTab;
                _propertiesTab = new RackPropertiesTabViewModel();
                _propertiesTab.Changed += OnPropertiesTabChanged;
                return _propertiesTab;
            }
        }

        private bool PropertiesDirty => _propertiesTab != null && _propertiesTab.IsDirty;

        private bool PropertiesBlockUpdate => _propertiesTab != null && _propertiesTab.HasProblems;

        /// <summary>The properties read with the rack by the door (RACKPUSHBACKABRIR).</summary>
        public void LoadProperties(RackPropertiesRead read) => PropertiesTab.Load(read);

        /// <summary>What «Actualizar» writes in the properties of the rack, or null when the tab writes nothing.</summary>
        public RackPropertiesWrite PropertiesWrite() => _propertiesTab?.BuildWrite();

        /// <summary>The answer of a write: when the properties were written, the tab goes on from what the drawing holds now.</summary>
        public void PropertiesWritten(RackDesignWritten written)
        {
            _propertiesTab?.Answered(written); // T6 pr: also the answer of «Insertar en el dibujo…» of a new rack (VA-S9)
        }

        private void OnPropertiesTabChanged()
        {
            UpdateDirty();
            OnPropertyChanged(nameof(CanUpdate));
            OnPropertyChanged(nameof(UpdateBlockedText));
            if (_isNewRack) RaiseState(); // T6 pr: a wrong property stops «Insertar en el dibujo…» of a new rack (InsertViewBlockedText)
        }

        // ------------------------------------------------------------------ the drafts of «Nuevo» (SD-06)

        /// <summary>The base of «Modificado» and of «Descartar»: the design (its project JSON) this editor compares with.</summary>
        internal string BaselineDesignJson => _baselineJson;

        /// <summary>The draft takes <paramref name="design"/> as an edit over its base («Duplicar»): «Modificado» exactly when it differs.</summary>
        internal bool TakeDesignAsEdit(PushBackDesign design)
        {
            if (design == null || !CanEdit) return false;
            if (!_draft.LoadFrom(design, _draft.Name)) return false;
            _isCopy = true; // T6 ui (correction 1 of the review): only a copy of «Duplicar» takes a design as an edit
            NormalizeSelection();
            UpdateDirty();
            Rebuild();
            return true;
        }

        // T6 ui: a copy of «Duplicar»: its «Descartar» goes back to how its original opened, and its text says so.
        private bool _isCopy;

        // ------------------------------------------------------------------ «Restaurar layout…» (question 6)

        private bool _restoreOpen;
        private bool _lossOpen;
        private IReadOnlyList<PushBackRestoreBox> _restoreBoxes = Array.Empty<PushBackRestoreBox>();

        /// <summary>«Restaurar layout…» shows its boxes inline (nothing is given back until «Restaurar lo marcado»).</summary>
        public bool IsRestoreOpen => _restoreOpen;

        public IReadOnlyList<PushBackRestoreBox> RestoreBoxes => _restoreBoxes;

        public bool IsLossOpen
        {
            get => _lossOpen;
            set { if (SetProperty(ref _lossOpen, value)) OnPropertyChanged(nameof(RestoreLoss)); }
        }

        /// <summary>«Ver lo que se pierde»: what the boxes marked give back now.</summary>
        public IReadOnlyList<string> RestoreLoss
        {
            get
            {
                var loss = _draft.LayoutLoss(ChosenRestore());
                return loss.Count == 0 ? new[] { "Lo marcado ya está como el cálculo: no hay nada que restaurar." } : loss;
            }
        }

        public string RestoreIntro => "Elige qué partes del layout vuelven al cálculo. Las tarimas dibujadas no entran: son dibujo, no layout.";

        public bool CanRestoreChosen => CanEdit && _restoreOpen && !ChosenRestore().IsEmpty;

        /// <summary>«Restaurar layout…»: opens its boxes, all marked.</summary>
        public void OpenRestore()
        {
            if (!CanEdit)
            {
                Say(WritingText, error: true);
                return;
            }

            _restoreBoxes = new[]
            {
                Box("lengths", "Longitudes propias de módulo"),
                Box("headers", "Cabeceras personalizadas"),
                Box("params", "Parámetros del rack (altura de cabecera, poste derivado y refuerzo, separadores)"),
                Box("fondos", "Posiciones de fondo propias de la celda"),
                Box("beams", "Largo manual de larguero"),
                Box("structure", "Ajuste de estructura por lado"),
            };
            _restoreOpen = true;
            _lossOpen = false;
            SetConfirmingClose(false);
            RaiseRestore();
        }

        private PushBackRestoreBox Box(string key, string label)
        {
            var box = new PushBackRestoreBox(key, label);
            box.PropertyChanged += (_, __) => { OnPropertyChanged(nameof(CanRestoreChosen)); OnPropertyChanged(nameof(RestoreLoss)); };
            return box;
        }

        /// <summary>«Cancelar» of «Restaurar layout…».</summary>
        public void CloseRestore()
        {
            if (!_restoreOpen) return;
            _restoreOpen = false;
            _restoreBoxes = Array.Empty<PushBackRestoreBox>();
            RaiseRestore();
        }

        /// <summary>«Restaurar lo marcado»: to the draft; the drawing does not change until «Actualizar».</summary>
        public void RestoreChosen()
        {
            var chosen = ChosenRestore();
            if (!CanEdit || !_restoreOpen || chosen.IsEmpty) return;
            var lost = _draft.LayoutLoss(chosen);
            CloseRestore();
            var error = Apply(() => _draft.RestoreLayout(chosen), null);
            if (error == null)
                Say(lost.Count == 0 ? "Nada que restaurar: lo marcado ya estaba como el cálculo." : "Layout restaurado en el borrador: " + string.Join(", ", lost) + ".", error: false);
        }

        private PushBackLayoutRestore ChosenRestore()
        {
            bool On(string key) => _restoreBoxes.Any(b => b.Key == key && b.IsChecked);
            return new PushBackLayoutRestore(On("lengths"), On("headers"), On("params"), On("fondos"), On("beams"), On("structure"));
        }

        private void RaiseRestore()
        {
            OnPropertyChanged(nameof(IsRestoreOpen));
            OnPropertyChanged(nameof(RestoreBoxes));
            OnPropertyChanged(nameof(IsLossOpen));
            OnPropertyChanged(nameof(RestoreLoss));
            OnPropertyChanged(nameof(CanRestoreChosen));
        }
    }
}
