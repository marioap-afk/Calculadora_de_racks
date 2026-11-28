using RackCad.Application.VentanaRackCad;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// The tab «Propiedades» at the end of the editor of Selectivo (I-64 A-14, cuarta tanda, va; VA-07): the custom properties of the rack are
    /// part of the draft of the editor (they make it «Modificado», «Descartar» drops them) and its «Actualizar» writes them after the redraw.
    /// </summary>
    public sealed partial class SelectiveEditorViewModel
    {
        /// <summary>The index of the tab «Propiedades», the last one of the editor.</summary>
        public const int PropertiesTabIndex = 4;

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

        /// <summary>The properties read with the rack by the door (RACKSELECTIVOABRIR).</summary>
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
            RefreshDirty();
            OnPropertyChanged(nameof(CanUpdate));
            OnPropertyChanged(nameof(UpdateBlockedText));
            if (_isNewRack) RaiseState(); // T6 pr: a wrong property stops «Insertar en el dibujo…» of a new rack (InsertViewBlockedText)
        }
    }
}
