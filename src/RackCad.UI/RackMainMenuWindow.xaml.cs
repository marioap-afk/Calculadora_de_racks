using System;
using System.Windows;
using RackCad.Application.RackFrames;

namespace RackCad.UI
{
    /// <summary>
    /// Application entry point: the user picks which system to design. Each option opens an
    /// independent module. The header configurator and the dynamic system are separate windows;
    /// the dynamic module reuses the header factory, not this menu's header window.
    /// </summary>
    public partial class RackMainMenuWindow : Window
    {
        public RackMainMenuWindow()
        {
            InitializeComponent();
        }

        private void DesignHeader_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var configuration = new HardcodedStandardRackFrameService().CreateDefault();
                var window = new RackFrameConfiguratorWindow(configuration) { Owner = this };
                window.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "No se pudo abrir el configurador de cabeceras: " + ex.Message,
                    "RackCad", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DesignDynamic_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var window = new RackDynamicSystemWindow { Owner = this };
                window.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "No se pudo abrir el sistema dinamico: " + ex.Message,
                    "RackCad", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
