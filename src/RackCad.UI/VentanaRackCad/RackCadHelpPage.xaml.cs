using System;
using System.Windows;
using System.Windows.Controls;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// The page «Órdenes y atajos» of the RackCad window (part «ut»). It passes «Ejecutar» and «En la ventana» of a row to the view-model of
    /// the help; every failure is contained and said in the message of the window.
    /// </summary>
    public partial class RackCadHelpPage : UserControl
    {
        public RackCadHelpPage()
        {
            InitializeComponent();
        }

        private RackCadWindowViewModel Window => DataContext as RackCadWindowViewModel;

        private void OnRunClick(object sender, RoutedEventArgs e) => Contained(window =>
        {
            if ((sender as FrameworkElement)?.DataContext is RackCadHelpRow row) window.Help.Run(row);
        });

        private void OnWindowClick(object sender, RoutedEventArgs e) => Contained(window =>
        {
            if ((sender as FrameworkElement)?.DataContext is RackCadHelpRow row) window.Help.GoTo(row);
        });

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
