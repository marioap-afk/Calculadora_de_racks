using System;
using System.Windows;
using System.Windows.Controls;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// The page «Biblioteca de diseños» of the RackCad window (part «ut»). It only passes the clicks of the user to the view-model of the
    /// window and of its library, and places the inspector beside the list when there is room and below it when there is not. Every
    /// failure is contained and said in the message of the window.
    /// </summary>
    public partial class RackCadLibraryPage : UserControl
    {
        /// <summary>Below this width of the list and its inspector, the inspector goes below the list.</summary>
        internal const double InspectorBesideWidth = 620;

        public RackCadLibraryPage()
        {
            InitializeComponent();
        }

        private RackCadWindowViewModel Window => DataContext as RackCadWindowViewModel;

        /// <summary>Whether the inspector is below the list (a narrow window).</summary>
        internal bool InspectorBelow { get; private set; }

        private void OnLayoutSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!e.WidthChanged) return;
            PlaceInspector(e.NewSize.Width < InspectorBesideWidth);
        }

        private void PlaceInspector(bool below)
        {
            if (InspectorBelow == below) return;
            InspectorBelow = below;
            if (below)
            {
                Grid.SetRow(LibraryInspector, 1);
                Grid.SetColumn(LibraryInspector, 0);
                LibraryInspector.Width = double.NaN;
                LibraryInspector.Margin = new Thickness(0, 10, 0, 0);
                InspectorRow.Height = GridLength.Auto;
            }
            else
            {
                Grid.SetRow(LibraryInspector, 0);
                Grid.SetColumn(LibraryInspector, 1);
                LibraryInspector.Width = 300;
                LibraryInspector.Margin = new Thickness(10, 0, 0, 0);
                InspectorRow.Height = new GridLength(0);
            }
        }

        private void OnRereadClick(object sender, RoutedEventArgs e) => Contained(w => w.Library.Reread());

        private void OnChangeFolderClick(object sender, RoutedEventArgs e) => Contained(w => w.ShowPage(RackCadWindowPage.Configuracion));

        private void OnSaveClick(object sender, RoutedEventArgs e) => Contained(w =>
        {
            LibrarySaveNameBox.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
            w.Library.Save();
        });

        private void OnSaveCancelClick(object sender, RoutedEventArgs e) => Contained(w => w.Library.CancelSave());

        private void OnReplaceClick(object sender, RoutedEventArgs e) => Contained(w => w.Library.Replace());

        private void OnRenameClick(object sender, RoutedEventArgs e) => Contained(w => w.Library.RenameSave());

        private void OnRowClick(object sender, RoutedEventArgs e) => Contained(w =>
        {
            if ((sender as FrameworkElement)?.DataContext is RackCadLibraryRow row) w.Library.Select(row);
        });

        private void OnOpenClick(object sender, RoutedEventArgs e) => Contained(w => w.Library.OpenSelected());

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
