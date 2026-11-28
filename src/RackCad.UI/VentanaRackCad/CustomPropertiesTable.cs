using System.Collections.ObjectModel;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// A draft of custom properties shown as a table (I-64 A-14, cuarta tanda, va): the project in «Variables» (VA-06) and, when it comes, a
    /// rack in the tab «Propiedades» of its editor (VA-07). The view (<see cref="CustomPropertiesTableView"/>) binds to these names and calls
    /// these gestures; the rules are those of <see cref="RackCad.Application.CustomProperties.CustomPropertiesBatch"/>, through the view-model.
    /// </summary>
    public interface ICustomPropertiesTable
    {
        ObservableCollection<CustomPropertyRowItem> Properties { get; }

        string PropertiesTitle { get; }

        string PropertiesNote { get; }

        string PropertiesHelp { get; }

        string PropertiesRemovedText { get; }

        string PropertiesCount { get; }

        bool CanAddProperty { get; }

        bool CanEditProperties { get; }

        bool HasNoProperties { get; }

        void AddProperty();

        void RemoveProperty(CustomPropertyRowItem row);

        /// <summary>A name or a value was left (or Enter in a name): the draft is judged again.</summary>
        void CommitProperties();
    }
}
