using System;
using System.Windows;
using System.Windows.Controls;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>T5 cm: «Cama en esta ventana» of «Nuevo». Its data context is the view-model of the window.</summary>
    public partial class RackCadNewCamaButton : UserControl
    {
        public RackCadNewCamaButton()
        {
            InitializeComponent();
        }

        private void OnNewCamaHereClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is not RackCadWindowViewModel window) return;
            try
            {
                window.StartNewCama();
            }
            catch (Exception ex)
            {
                window.ShowError(ex);
            }
        }
    }
}
