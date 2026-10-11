using System;
using System.Windows.Controls;

namespace RackCad.UI.Workspace
{
    /// <summary>Root control of the workspace panel (I-64 F1-T3B). Skeleton of the RED step.</summary>
    public partial class WorkspacePanelView : UserControl
    {
        public const string RootLabel = "Workspace";

        public WorkspacePanelView(WorkspacePanelViewModel viewModel)
        {
            ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            InitializeComponent();
        }

        public WorkspacePanelViewModel ViewModel { get; }
    }
}
