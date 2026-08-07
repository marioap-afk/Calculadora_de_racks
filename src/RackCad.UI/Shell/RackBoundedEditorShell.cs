using System.Windows;
using System.Windows.Controls;

namespace RackCad.UI.Shell
{
    /// <summary>
    /// The common visual shell of the BOUNDED EDITOR archetype (archetype B of ADR-0029): a window with a limited
    /// functional scope, its own parameters, a preview, diagnostics and a bounded result or action.
    ///
    /// <para>It is deliberately NOT called a "component editor": its consumers are not necessarily persistent
    /// components nor BOM members. The four Cantilever component configurators are bounded editors; so is the
    /// structural section inspector, which edits nothing and owns no component.</para>
    ///
    /// <para>It exists because motivo 1 and motivo 6 of the I-37D round-1 rejection were that the main window was
    /// saturated and that its architecture did not reflect the real configuration flow. Splitting a product into
    /// one window per subject only helps if those windows are the same window: hand-written layouts drift on the
    /// first correction, and the user learns N dialogs instead of one.</para>
    ///
    /// <para>A LOOKLESS, TEMPLATED control — like <c>RackEditorVisualShell</c> and for the same reason: named
    /// content injected into its slots registers in the CONSUMING window's name scope, which a UserControl would
    /// forbid (MC3093). It is lighter than the rich editor shell: a bounded editor has no matrix and no side
    /// panel; it has parameters, a picker, a preview, its diagnostics and its recipe.</para>
    ///
    /// <para>It knows no geometry, no AutoCAD, no persistence and no system: every slot is <c>object</c>, and
    /// which fields a subject has is that subject's business (ADR-0029 D12).</para>
    /// </summary>
    public class RackBoundedEditorShell : Control
    {
        static RackBoundedEditorShell()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(RackBoundedEditorShell),
                new FrameworkPropertyMetadata(typeof(RackBoundedEditorShell)));
        }

        /// <summary>
        /// The exact moment the template is about to resolve its tokens: by now the tree exists, so a consumer that
        /// supplies them can be told apart from one that does not.
        ///
        /// <para>The shell carries its own spacing, colour and typography tokens instead of depending on the consumer
        /// having merged them. A <c>DynamicResource</c> with a plain string key searches the element tree and then
        /// <c>Application.Resources</c>; it does NOT fall back to the assembly theme dictionary, even though that is
        /// where this control's template came from. Without it a consumer that merges nothing — a window built in
        /// code, which has no reason to merge anything — renders the template with <c>ShellZoneSpacing</c> unresolved,
        /// i.e. a ZERO margin, and its action buttons end up flush against the window edges. That was the single
        /// defect of I-39A's round-1 manual validation.</para>
        ///
        /// <para>It is a FALLBACK, not shadowing, and the difference matters — the lesson I-39B paid for on the rich
        /// shell. Merging unconditionally, as I-39A did here, puts the control's dictionary ahead of the window's for
        /// the content the editor injects into the slots too, and that content resolves ANOTHER INSTANCE of the same
        /// style: same appearance, different object. Merging only when the token does NOT resolve leaves every XAML
        /// consumer resolving against its own dictionary, so nothing changes for the four Cantilever windows.</para>
        /// </summary>
        public override void OnApplyTemplate()
        {
            if (TryFindResource("ShellZoneSpacing") == null)
            {
                Resources.MergedDictionaries.Add(ShellResources.Shared);
            }

            base.OnApplyTemplate();
        }

        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register(nameof(Header), typeof(object), typeof(RackBoundedEditorShell), new PropertyMetadata(null));

        public static readonly DependencyProperty ParametersProperty =
            DependencyProperty.Register(nameof(Parameters), typeof(object), typeof(RackBoundedEditorShell), new PropertyMetadata(null));

        public static readonly DependencyProperty SectionPickerProperty =
            DependencyProperty.Register(nameof(SectionPicker), typeof(object), typeof(RackBoundedEditorShell), new PropertyMetadata(null));

        public static readonly DependencyProperty PreviewProperty =
            DependencyProperty.Register(nameof(Preview), typeof(object), typeof(RackBoundedEditorShell), new PropertyMetadata(null));

        public static readonly DependencyProperty DiagnosticsProperty =
            DependencyProperty.Register(nameof(Diagnostics), typeof(object), typeof(RackBoundedEditorShell), new PropertyMetadata(null));

        public static readonly DependencyProperty BomSummaryProperty =
            DependencyProperty.Register(nameof(BomSummary), typeof(object), typeof(RackBoundedEditorShell), new PropertyMetadata(null));

        public static readonly DependencyProperty ActionsProperty =
            DependencyProperty.Register(nameof(Actions), typeof(object), typeof(RackBoundedEditorShell), new PropertyMetadata(null));

        /// <summary>What the user is editing or inspecting, in one line. Optional.</summary>
        public object Header { get => GetValue(HeaderProperty); set => SetValue(HeaderProperty, value); }

        /// <summary>The subject's own fields. The ONE part that differs between bounded editors.</summary>
        public object Parameters { get => GetValue(ParametersProperty); set => SetValue(ParametersProperty, value); }

        /// <summary>Its searchable section picker (or pickers). Optional: a bounded editor that chooses nothing
        /// from a catalogue, or that keeps its own search inside <see cref="Parameters"/>, leaves it empty.</summary>
        public object SectionPicker { get => GetValue(SectionPickerProperty); set => SetValue(SectionPickerProperty, value); }

        /// <summary>The subject's own preview, which takes the space the parameters do not.</summary>
        public object Preview { get => GetValue(PreviewProperty); set => SetValue(PreviewProperty, value); }

        /// <summary>
        /// The subject's diagnostics, shown HERE and not saved up for a parent window.
        ///
        /// Hiding a subject's own problems until the user goes back is regression 12 of the I-37D round: the place
        /// to say that a section is not eligible is the window where it was chosen.
        /// </summary>
        public object Diagnostics { get => GetValue(DiagnosticsProperty); set => SetValue(DiagnosticsProperty, value); }

        /// <summary>Its recipe: what the subject is made of, as the BOM will count it. Optional — a bounded editor
        /// that produces no BOM leaves it empty.</summary>
        public object BomSummary { get => GetValue(BomSummaryProperty); set => SetValue(BomSummaryProperty, value); }

        /// <summary>Its actions. The shell does NOT decide which ones exist, what they mean or whether one of them
        /// is the default: that is the consuming window's contract (ADR-0029 D6).</summary>
        public object Actions { get => GetValue(ActionsProperty); set => SetValue(ActionsProperty, value); }

        // ---- Template-part test seams (valid after the template is applied) ----

        internal ContentPresenter HeaderHost => GetTemplateChild("PART_Header") as ContentPresenter;
        internal ContentPresenter ParametersHost => GetTemplateChild("PART_Parameters") as ContentPresenter;
        internal ContentPresenter PickerHost => GetTemplateChild("PART_Picker") as ContentPresenter;
        internal ContentPresenter PreviewHost => GetTemplateChild("PART_Preview") as ContentPresenter;
        internal ContentPresenter DiagnosticsHost => GetTemplateChild("PART_Diagnostics") as ContentPresenter;
        internal ContentPresenter BomHost => GetTemplateChild("PART_Bom") as ContentPresenter;
        internal ContentPresenter ActionsHost => GetTemplateChild("PART_Actions") as ContentPresenter;
    }
}
