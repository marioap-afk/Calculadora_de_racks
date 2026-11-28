using System;
using System.Windows;
using System.Windows.Controls;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>The view of the tab «Propiedades» of an editor of the window (cuarta tanda, va): it only passes the gestures of the user.</summary>
    public partial class RackPropertiesTabView : UserControl
    {
        public RackPropertiesTabView()
        {
            InitializeComponent();
        }

        private RackPropertiesTabViewModel Tab => DataContext as RackPropertiesTabViewModel;

        private void OnSourceChecked(object sender, RoutedEventArgs e)
            => Contained(() => Tab?.ChooseSource((sender as FrameworkElement)?.DataContext as RackPropertiesSourceItem));

        private void OnUnifyClick(object sender, RoutedEventArgs e) => Contained(() => Tab?.Unify());

        private static void Contained(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("RackPropertiesTabView: " + ex);
            }
        }
    }
}
