using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// The page «Variables» in the view of the RackCad window (I-64 A-14, cuarta tanda, va): one view per page (one page per drawing), kept
    /// while the page lives, so the box with the focus is never replaced by a commit.
    /// </summary>
    public partial class RackCadWindowView
    {
        /// <summary>Visible when every flag is true (the page «Variables» is shown, and the window has its door).</summary>
        public static readonly IMultiValueConverter AllFlagsToVisibility = new AllFlagsConverter();

        private readonly Dictionary<VariablesPageViewModel, VariablesPageView> _variablesViews = new();

        /// <summary>The view of the page of the drawing of the window, or null.</summary>
        internal VariablesPageView VariablesView => VariablesPageHost.Content as VariablesPageView;

        private void ShowVariablesPage()
        {
            var page = _viewModel.VariablesPage;
            foreach (var gone in _variablesViews.Keys.Where(k => !ReferenceEquals(k, page) && !IsKept(k)).ToList()) _variablesViews.Remove(gone);
            if (page == null)
            {
                VariablesPageHost.Content = null;
                return;
            }

            if (!_variablesViews.TryGetValue(page, out var view))
            {
                view = new VariablesPageView(page);
                _variablesViews[page] = view;
            }

            if (!ReferenceEquals(VariablesPageHost.Content, view)) VariablesPageHost.Content = view;
        }

        // A page of another drawing keeps its view while it has changes (the window keeps its draft).
        private static bool IsKept(VariablesPageViewModel page) => page.IsDirty || page.IsWriting;

        private sealed class AllFlagsConverter : IMultiValueConverter
        {
            public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
                => values != null && values.All(v => v is bool flag && flag) ? Visibility.Visible : Visibility.Collapsed;

            public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotSupportedException();
        }
    }
}
