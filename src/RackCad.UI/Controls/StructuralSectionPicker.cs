using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RackCad.Application.StructuralSections;

namespace RackCad.UI.Controls
{
    /// <summary>
    /// A searchable structural-section picker: a text box, a family filter and a list.
    ///
    /// It exists because motivo 3 of the round-1 rejection was «los perfiles son difíciles de seleccionar». A
    /// plain <c>ComboBox</c> over 983 rows is a scroll bar, and a user who knows the piece is called
    /// <c>W10X33</c> has no way to say so.
    ///
    /// <para><b>What it does not do.</b> It never resolves a section from a fragment: the search FINDS a row and
    /// the row hands back an EXACT <see cref="StructuralSectionId"/> (ADR-0024, D1 and D6). It reads no CSV — the
    /// catalogue arrives loaded — it holds no policy, and it knows nothing about Cantilever: the eligible
    /// families come from whoever hosts it, because which families a system admits is that system's decision.</para>
    ///
    /// <para>The list is virtualized: 983 rows is more than enough to make a non-virtualized
    /// <c>ItemsControl</c> stutter on every keystroke.</para>
    /// </summary>
    public class StructuralSectionPicker : Control
    {
        /// <summary>The "no family filter" row. A sentinel object, so it cannot collide with a real family.</summary>
        private static readonly object AllFamilies = new object();

        private TextBox searchBox;
        private ComboBox familyBox;
        private ListBox list;
        private TextBlock emptyState;

        private IReadOnlyList<StructuralSectionChoice> allChoices = Array.Empty<StructuralSectionChoice>();
        private bool suppressSelection;

        static StructuralSectionPicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(StructuralSectionPicker), new FrameworkPropertyMetadata(typeof(StructuralSectionPicker)));
        }

        public static readonly DependencyProperty SelectedSectionIdProperty =
            DependencyProperty.Register(
                nameof(SelectedSectionId), typeof(string), typeof(StructuralSectionPicker),
                new FrameworkPropertyMetadata(
                    null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedSectionIdChanged));

        public static readonly DependencyProperty SearchTextProperty =
            DependencyProperty.Register(
                nameof(SearchText), typeof(string), typeof(StructuralSectionPicker),
                new PropertyMetadata(string.Empty, OnFilterChanged));

        /// <summary>The EXACT id of the selected section, or null when nothing is selected.</summary>
        public string SelectedSectionId
        {
            get => (string)GetValue(SelectedSectionIdProperty);
            set => SetValue(SelectedSectionIdProperty, value);
        }

        public string SearchText
        {
            get => (string)GetValue(SearchTextProperty);
            set => SetValue(SearchTextProperty, value);
        }

        /// <summary>Raised when the user picks a row. The argument is the exact id (never a fragment).</summary>
        public event EventHandler<string> SectionChosen;

        /// <summary>The rows currently offered, after the text and the family filter.</summary>
        public IReadOnlyList<StructuralSectionChoice> VisibleChoices { get; private set; } =
            Array.Empty<StructuralSectionChoice>();

        /// <summary>Every row the consumer allowed, before filtering.</summary>
        public IReadOnlyList<StructuralSectionChoice> AllChoices => allChoices;

        /// <summary>The family filter in force, or null for «Todas».</summary>
        public StructuralSectionFamily? Family { get; private set; }

        /// <summary>
        /// Loads the catalogue rows this picker offers, restricted to <paramref name="eligibleFamilies"/>.
        ///
        /// An empty catalogue is a legitimate state and says so on screen; it is not an exception, because a
        /// catalogue that failed to load has already been reported by whoever loaded it.
        /// </summary>
        public void Load(
            StructuralSectionCatalog catalogue,
            IEnumerable<StructuralSectionFamily> eligibleFamilies = null)
        {
            allChoices = catalogue == null
                ? Array.Empty<StructuralSectionChoice>()
                : StructuralSectionSearch.Choices(catalogue, eligibleFamilies);

            BuildFamilyFilter();
            Refilter();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            searchBox = GetTemplateChild("PART_Search") as TextBox;
            familyBox = GetTemplateChild("PART_Family") as ComboBox;
            list = GetTemplateChild("PART_List") as ListBox;
            emptyState = GetTemplateChild("PART_Empty") as TextBlock;

            if (searchBox != null)
            {
                searchBox.TextChanged += (_, __) => SearchText = searchBox.Text;
                searchBox.PreviewKeyDown += SearchKeyDown;
            }

            if (familyBox != null)
            {
                familyBox.SelectionChanged += FamilyChanged;
            }

            if (list != null)
            {
                list.SelectionChanged += ListSelectionChanged;
            }

            BuildFamilyFilter();
            Refilter();
        }

        private static void OnSelectedSectionIdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            => ((StructuralSectionPicker)d).SyncSelectionToList();

        private static void OnFilterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            => ((StructuralSectionPicker)d).Refilter();

        private void BuildFamilyFilter()
        {
            if (familyBox == null)
            {
                return;
            }

            var present = allChoices.Select(c => c.Family).Distinct().ToList();

            // Only the families this picker actually offers get a filter button: a «S» filter on a picker that
            // admits channels would promise a result that cannot exist.
            var items = new List<object> { AllFamilies };
            items.AddRange(StructuralSectionSearch.Families.Where(present.Contains).Cast<object>());

            suppressSelection = true;
            familyBox.ItemsSource = items;
            familyBox.SelectedIndex = 0;
            suppressSelection = false;

            Family = null;
        }

        private void FamilyChanged(object sender, SelectionChangedEventArgs e)
        {
            if (suppressSelection)
            {
                return;
            }

            Family = familyBox.SelectedItem is StructuralSectionFamily family ? family : (StructuralSectionFamily?)null;
            Refilter();
        }

        /// <summary>The label shown for a family row of the filter. «Todas» for the sentinel.</summary>
        public static string FamilyLabel(object item) =>
            item is StructuralSectionFamily family ? StructuralSectionSearch.Label(family) : "Todas";

        private void Refilter()
        {
            VisibleChoices = StructuralSectionSearch.Filter(allChoices, SearchText, Family);

            if (list != null)
            {
                suppressSelection = true;
                list.ItemsSource = VisibleChoices;
                suppressSelection = false;
                SyncSelectionToList();
            }

            if (emptyState != null)
            {
                emptyState.Visibility = VisibleChoices.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
                emptyState.Text = allChoices.Count == 0
                    ? "No hay secciones disponibles."
                    : "Ninguna sección coincide con la búsqueda.";
            }
        }

        /// <summary>
        /// Shows the current selection in the list when it is visible, and shows NOTHING when the filter hid it.
        ///
        /// It never clears <see cref="SelectedSectionId"/>: filtering is a way of looking, not a way of
        /// deciding, and a search that silently unselected the user's section would be a data change caused by
        /// typing.
        /// </summary>
        private void SyncSelectionToList()
        {
            if (list == null)
            {
                return;
            }

            var match = VisibleChoices.FirstOrDefault(
                c => string.Equals(c.Id.Value, SelectedSectionId, StringComparison.Ordinal));

            suppressSelection = true;
            list.SelectedItem = match;
            suppressSelection = false;
        }

        private void ListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (suppressSelection || !(list.SelectedItem is StructuralSectionChoice choice))
            {
                return;
            }

            SelectedSectionId = choice.Id.Value;
            SectionChosen?.Invoke(this, choice.Id.Value);
        }

        /// <summary>Down/Up from the search box move through the list without leaving the keyboard.</summary>
        private void SearchKeyDown(object sender, KeyEventArgs e)
        {
            if (list == null || VisibleChoices.Count == 0)
            {
                return;
            }

            if (e.Key == Key.Down)
            {
                Move(1);
                e.Handled = true;
            }
            else if (e.Key == Key.Up)
            {
                Move(-1);
                e.Handled = true;
            }
            else if (e.Key == Key.Enter && list.SelectedItem == null)
            {
                list.SelectedIndex = 0;
                e.Handled = true;
            }
        }

        private void Move(int delta)
        {
            var next = list.SelectedIndex + delta;

            if (next < 0)
            {
                next = 0;
            }

            if (next >= VisibleChoices.Count)
            {
                next = VisibleChoices.Count - 1;
            }

            list.SelectedIndex = next;
            list.ScrollIntoView(list.SelectedItem);
        }

        // ---- Test seams ---------------------------------------------------------------------------------

        internal TextBox SearchBox => searchBox;

        internal ComboBox FamilyBox => familyBox;

        internal ListBox List => list;

        internal TextBlock EmptyState => emptyState;

        /// <summary>Selects a family filter programmatically, as clicking its button would.</summary>
        internal void SetFamily(StructuralSectionFamily? family)
        {
            Family = family;
            Refilter();
        }
    }
}
