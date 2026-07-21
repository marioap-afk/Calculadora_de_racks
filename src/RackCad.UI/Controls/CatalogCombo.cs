using System.Collections.Generic;
using System.Windows.Controls;
using RackCad.Application.Catalogs;

namespace RackCad.UI.Controls
{
    /// <summary>
    /// A combo box wired for <see cref="CatalogOption"/>: it shows <see cref="CatalogOption.DisplayName"/> while the
    /// model keeps <see cref="CatalogOption.Id"/> (the <c>DisplayMemberPath="DisplayName" SelectedValuePath="Id"</c>
    /// idiom repeated across the windows). It fills straight from catalog entries via <see cref="UiSupport.ToOptions"/>
    /// and supports an "(auto)"/"(none)" sentinel, so the per-window "populate + inject placeholder + SelectedValue = id"
    /// boilerplate becomes one call.
    /// </summary>
    public class CatalogCombo : ComboBox
    {
        public CatalogCombo()
        {
            DisplayMemberPath = nameof(CatalogOption.DisplayName);
            SelectedValuePath = nameof(CatalogOption.Id);
        }

        /// <summary>The id of the selected option (its <see cref="System.Windows.Controls.Primitives.Selector.SelectedValue"/>),
        /// or null when nothing (or the sentinel) is selected.</summary>
        public string SelectedId
        {
            get => SelectedValue as string;
            set => SelectedValue = value;
        }

        /// <summary>Populates the combo with <paramref name="options"/> (optionally led by <paramref name="placeholder"/>)
        /// and selects <paramref name="selectedId"/>. When the id is absent the sentinel — or the first row — is left
        /// selected, matching the tolerant behavior of the current windows.</summary>
        public void SetOptions(IEnumerable<CatalogOption> options, string selectedId = null, CatalogOption placeholder = null)
        {
            var list = CatalogComboSelection.WithPlaceholder(placeholder, options);
            ItemsSource = list;

            var match = CatalogComboSelection.Resolve(list, selectedId);
            if (match != null)
            {
                SelectedItem = match;
            }
            else if (placeholder != null)
            {
                SelectedItem = placeholder;
            }
        }

        /// <summary>Populates the combo directly from catalog entries (distinct by id, ordered by label) via the
        /// shared <see cref="UiSupport.ToOptions"/> factory.</summary>
        public void SetCatalogEntries<T>(IEnumerable<T> entries, string selectedId = null, CatalogOption placeholder = null)
            where T : CatalogEntryBase
            => SetOptions(UiSupport.ToOptions(entries), selectedId, placeholder);
    }
}
