using System;
using System.Windows;
using System.Windows.Controls;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// «Nuevo › Larguero» of the RackCad window (part «lg», a draft in construction). It only passes the clicks of the user to its view-model
    /// and places the summary beside the sections when there is room and below them when there is not. A failure is contained and said in
    /// the message of the window.
    /// </summary>
    public partial class LargueroWorkbenchView : UserControl
    {
        /// <summary>Below this width of the page, the summary goes below the sections.</summary>
        internal const double SummaryBesideWidth = 700;

        public LargueroWorkbenchView()
        {
            InitializeComponent();
        }

        private LargueroWorkbenchViewModel Workbench => DataContext as LargueroWorkbenchViewModel;

        /// <summary>Whether the summary is below the sections (a narrow window).</summary>
        internal bool SummaryBelow { get; private set; }

        private void OnLayoutSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!e.WidthChanged) return;
            PlaceSummary(e.NewSize.Width < SummaryBesideWidth);
        }

        private void PlaceSummary(bool below)
        {
            if (SummaryBelow == below) return;
            SummaryBelow = below;
            if (below)
            {
                Grid.SetRow(LargueroSummaryPanel, 1);
                Grid.SetColumn(LargueroSummaryPanel, 0);
                LargueroSummaryPanel.Width = double.NaN;
                LargueroSummaryPanel.Margin = new Thickness(0, 10, 0, 0);
                SummaryRow.Height = GridLength.Auto;
            }
            else
            {
                Grid.SetRow(LargueroSummaryPanel, 0);
                Grid.SetColumn(LargueroSummaryPanel, 1);
                LargueroSummaryPanel.Width = 340;
                LargueroSummaryPanel.Margin = new Thickness(12, 0, 0, 0);
                SummaryRow.Height = new GridLength(0);
            }
        }

        private void OnHomeClick(object sender, RoutedEventArgs e) => Contained(w => w.GoHome());

        private void OnResetClick(object sender, RoutedEventArgs e) => Contained(w => w.ResetToDefaults());

        private void OnClassicClick(object sender, RoutedEventArgs e) => Contained(w => w.OpenClassicEditor());

        private void Contained(Action<LargueroWorkbenchViewModel> interaction)
        {
            var workbench = Workbench;
            if (workbench != null) workbench.Contained(() => interaction(workbench));
        }
    }
}
