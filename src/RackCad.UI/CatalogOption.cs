namespace RackCad.UI
{
    /// <summary>
    /// A selectable catalog entry for the combos: the user sees <see cref="DisplayName"/> while the model
    /// keeps the <see cref="Id"/> (bound via the combo's SelectedValue/SelectedValuePath). This is why the
    /// dropdowns show "Poste omega 3x3" instead of the raw catalog id.
    /// </summary>
    public sealed class CatalogOption
    {
        public CatalogOption(string id, string displayName)
        {
            Id = id;
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? id : displayName;
        }

        public string Id { get; }

        public string DisplayName { get; }

        public override string ToString() => DisplayName;
    }
}
