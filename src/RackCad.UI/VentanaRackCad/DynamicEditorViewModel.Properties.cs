using RackCad.Application.VentanaRackCad;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// The tab «Propiedades» at the end of the editor of Dinámico (I-64 A-14, cuarta tanda, va; VA-07), as in the editor of Selectivo: the
    /// custom properties of the rack are part of the draft and its «Actualizar» writes them after the redraw.
    /// </summary>
    public sealed partial class DynamicEditorViewModel
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

        /// <summary>The properties read with the rack by the door (RACKDINAMICOABRIR).</summary>
        public void LoadProperties(RackPropertiesRead read) => PropertiesTab.Load(read);

        /// <summary>What «Actualizar» writes in the properties of the rack, or null when the tab writes nothing.</summary>
        public RackPropertiesWrite PropertiesWrite() => _propertiesTab?.BuildWrite();

        /// <summary>The answer of a write: when the properties were written, the tab goes on from what the drawing holds now.</summary>
        public void PropertiesWritten(RackDesignWritten written)
        {
            if (written != null && written.Written && written.PropertiesError == null) _propertiesTab?.Written(written.Properties);
        }

        private void OnPropertiesTabChanged()
        {
            UpdateDirty();
            OnPropertyChanged(nameof(CanUpdate));
            OnPropertyChanged(nameof(UpdateBlockedText));
        }
    }
}
