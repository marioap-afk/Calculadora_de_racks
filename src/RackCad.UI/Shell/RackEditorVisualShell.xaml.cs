using System.Windows;
using System.Windows.Controls;

namespace RackCad.UI.Shell
{
    /// <summary>
    /// The common editor visual shell (I-30). A COMPOSITION component — a <see cref="UserControl"/>, never a Window base
    /// class — that lays out the shared editor structure and exposes it as content slots the concrete editor fills:
    /// a neutral, optional sidebar header; a scrolling side panel; an OPTIONAL central matrix; a preview that takes the
    /// remaining space (an editor without a matrix just leaves that slot empty); an always-visible status band outside
    /// the scroll; and the four neutral action categories of <see cref="EditorActionBar"/>.
    ///
    /// The shell is agnostic to the system: it carries no system-kind coupling, branches on no rack system, and holds
    /// no geometry/BOM/persistence/insertion logic. Sizes, colors, spacing and typography come from the tokens in
    /// <c>Themes/AppStyles.xaml</c>. It does NOT own identity — the GUID lives in <c>RackEditorSession</c>, and the
    /// header slot is optional so a shell need not show it.
    /// </summary>
    public partial class RackEditorVisualShell : UserControl
    {
        public RackEditorVisualShell()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty SidebarHeaderProperty =
            DependencyProperty.Register(nameof(SidebarHeader), typeof(object), typeof(RackEditorVisualShell), new PropertyMetadata(null));

        public static readonly DependencyProperty SidePanelContentProperty =
            DependencyProperty.Register(nameof(SidePanelContent), typeof(object), typeof(RackEditorVisualShell), new PropertyMetadata(null));

        public static readonly DependencyProperty MatrixContentProperty =
            DependencyProperty.Register(nameof(MatrixContent), typeof(object), typeof(RackEditorVisualShell), new PropertyMetadata(null));

        public static readonly DependencyProperty PreviewContentProperty =
            DependencyProperty.Register(nameof(PreviewContent), typeof(object), typeof(RackEditorVisualShell), new PropertyMetadata(null));

        public static readonly DependencyProperty StatusContentProperty =
            DependencyProperty.Register(nameof(StatusContent), typeof(object), typeof(RackEditorVisualShell), new PropertyMetadata(null));

        public static readonly DependencyProperty LeadingActionsProperty =
            DependencyProperty.Register(nameof(LeadingActions), typeof(object), typeof(RackEditorVisualShell), new PropertyMetadata(null));

        public static readonly DependencyProperty SecondaryActionsProperty =
            DependencyProperty.Register(nameof(SecondaryActions), typeof(object), typeof(RackEditorVisualShell), new PropertyMetadata(null));

        public static readonly DependencyProperty PrimaryActionsProperty =
            DependencyProperty.Register(nameof(PrimaryActions), typeof(object), typeof(RackEditorVisualShell), new PropertyMetadata(null));

        public static readonly DependencyProperty TrailingActionsProperty =
            DependencyProperty.Register(nameof(TrailingActions), typeof(object), typeof(RackEditorVisualShell), new PropertyMetadata(null));

        /// <summary>Neutral, optional header of the side panel (the editor decides its content; the shell does not
        /// require showing a name or GUID).</summary>
        public object SidebarHeader { get => GetValue(SidebarHeaderProperty); set => SetValue(SidebarHeaderProperty, value); }

        /// <summary>The scrolling side panel: the editor's sections.</summary>
        public object SidePanelContent { get => GetValue(SidePanelContentProperty); set => SetValue(SidePanelContentProperty, value); }

        /// <summary>The central editing surface. OPTIONAL: empty collapses and the preview takes the space.</summary>
        public object MatrixContent { get => GetValue(MatrixContentProperty); set => SetValue(MatrixContentProperty, value); }

        /// <summary>The preview surface (with its own view selector, supplied by the editor).</summary>
        public object PreviewContent { get => GetValue(PreviewContentProperty); set => SetValue(PreviewContentProperty, value); }

        /// <summary>The status band; commonly an <see cref="EditorStatusPresenter"/>. Empty collapses.</summary>
        public object StatusContent { get => GetValue(StatusContentProperty); set => SetValue(StatusContentProperty, value); }

        public object LeadingActions { get => GetValue(LeadingActionsProperty); set => SetValue(LeadingActionsProperty, value); }
        public object SecondaryActions { get => GetValue(SecondaryActionsProperty); set => SetValue(SecondaryActionsProperty, value); }
        public object PrimaryActions { get => GetValue(PrimaryActionsProperty); set => SetValue(PrimaryActionsProperty, value); }
        public object TrailingActions { get => GetValue(TrailingActionsProperty); set => SetValue(TrailingActionsProperty, value); }
    }
}
