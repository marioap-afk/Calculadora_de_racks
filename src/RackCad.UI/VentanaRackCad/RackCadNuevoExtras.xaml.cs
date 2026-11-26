using System;
using System.Windows;
using System.Windows.Controls;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>The end of the page «Nuevo» (part «ut»): it passes the clicks to the view-model of the window, contained.</summary>
    public partial class RackCadNuevoExtras : UserControl
    {
        public RackCadNuevoExtras()
        {
            InitializeComponent();
        }

        private RackCadWindowViewModel Window => DataContext as RackCadWindowViewModel;

        private void OnShowLibraryClick(object sender, RoutedEventArgs e) => Contained(window => window.ShowPage(RackCadWindowPage.Biblioteca));

        private void OnRecentClick(object sender, RoutedEventArgs e) => Contained(window =>
        {
            if ((sender as FrameworkElement)?.DataContext is RackCadLibraryRow row) window.ShowRecentDesign(row);
        });

        private void OnShowAllCommandsClick(object sender, RoutedEventArgs e) => Contained(window => window.ShowHelp());

        private void Contained(Action<RackCadWindowViewModel> interaction)
        {
            var window = Window;
            if (window == null) return;
            try
            {
                interaction(window);
            }
            catch (Exception ex)
            {
                window.ShowError(ex);
            }
        }
    }
}
