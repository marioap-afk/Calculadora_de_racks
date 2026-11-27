using System;
using System.Windows;
using System.Windows.Controls;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>T5 cm: «Editar aquí» of a cama of the list, on the page of the rack. Its data context is the view-model of the window.</summary>
    public partial class RackCadCamaOfferPanel : UserControl
    {
        public RackCadCamaOfferPanel()
        {
            InitializeComponent();
        }

        private void OnEditCamaHereClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is not RackCadWindowViewModel window) return;
            try
            {
                window.EditCamaHere();
            }
            catch (Exception ex)
            {
                window.ShowError(ex);
            }
        }
    }
}
