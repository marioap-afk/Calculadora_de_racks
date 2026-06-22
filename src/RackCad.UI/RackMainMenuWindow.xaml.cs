using System;
using System.Windows;
using RackCad.Application.RackFrames;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems;

namespace RackCad.UI
{
    /// <summary>
    /// Application entry point: the user picks which system to design. Each option opens an
    /// independent module. The header configurator and the dynamic system are separate windows;
    /// the dynamic module reuses the header factory, not this menu's header window.
    /// </summary>
    public partial class RackMainMenuWindow : Window
    {
        private readonly bool canInsertInAutoCad;

        /// <summary>Set when the user asked to insert the configured header; the host command draws it after
        /// every modal window (this menu included) has closed, so the placement jig has the editor free.</summary>
        public bool InsertRequested { get; private set; }

        public RackFrameConfiguration ConfigurationToInsert { get; private set; }

        public DynamicRackSystem DynamicSystemToInsert { get; private set; }

        public RackMainMenuWindow()
            : this(false)
        {
        }

        public RackMainMenuWindow(bool canInsertInAutoCad)
        {
            this.canInsertInAutoCad = canInsertInAutoCad;
            InitializeComponent();
        }

        private void DesignHeader_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var configuration = new HardcodedStandardRackFrameService().CreateDefault();
                var window = new RackFrameConfiguratorWindow(configuration, canInsertInAutoCad) { Owner = this };
                window.ShowDialog();

                if (window.InsertRequested)
                {
                    // Bubble the request up and close so the host command can run the placement jig.
                    InsertRequested = true;
                    ConfigurationToInsert = window.Configuration;
                    Close();
                }
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
                var window = new RackDynamicSystemWindow(canInsertInAutoCad) { Owner = this };
                window.ShowDialog();

                if (window.InsertRequested)
                {
                    InsertRequested = true;
                    DynamicSystemToInsert = window.SystemToInsert;
                    Close();
                }
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
