using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// The table of a draft of custom properties (cuarta tanda, va), over an <see cref="ICustomPropertiesTable"/>. It only passes the gestures
    /// of the user: «Nueva propiedad», «Quitar», and leaving a box (or Enter in a name) judges the draft again. Every failure is contained.
    /// </summary>
    public partial class CustomPropertiesTableView : UserControl
    {
        /// <summary>The negation of a flag, for the read only of the boxes.</summary>
        public static readonly IValueConverter Not = new NotConverter();

        public CustomPropertiesTableView()
        {
            InitializeComponent();
        }

        private ICustomPropertiesTable Table => DataContext as ICustomPropertiesTable;

        private void OnAddClick(object sender, RoutedEventArgs e) => Contained(() => Table?.AddProperty());

        private void OnRemoveClick(object sender, RoutedEventArgs e)
            => Contained(() => Table?.RemoveProperty((sender as FrameworkElement)?.DataContext as CustomPropertyRowItem));

        private void OnBoxLostFocus(object sender, RoutedEventArgs e) => Contained(() => Table?.CommitProperties());

        private void OnNameKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) Contained(() => Table?.CommitProperties());
        }

        private static void Contained(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("CustomPropertiesTableView: " + ex);
            }
        }

        private sealed class NotConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => !(value is bool flag && flag);

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => !(value is bool flag && flag);
        }
    }
}
