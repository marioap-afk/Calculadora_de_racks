using System.Windows;
using System.Windows.Controls;

namespace RackCad.UI.Systems.Cantilever.Components
{
    /// <summary>
    /// The common visual shell of the FOUR Cantilever component editors: column–base, arm, separator and brace.
    ///
    /// It exists because motivo 1 and motivo 6 of the round-1 rejection were that the main window was saturated
    /// and that its architecture did not reflect the real configuration flow. Splitting the product into one
    /// window per component only helps if the four windows are the same window: four hand-written layouts would
    /// drift on the first correction, and the user would learn four dialogs instead of one.
    ///
    /// <para>A LOOKLESS, TEMPLATED control — like <c>RackEditorVisualShell</c> and for the same reason: named
    /// content injected into its slots registers in the CONSUMING window's name scope, which a UserControl would
    /// forbid (MC3093). It is deliberately lighter than the editor shell: a component editor has no matrix and no
    /// side panel, it has parameters, a picker, a preview, its diagnostics and its recipe.</para>
    ///
    /// <para>It knows no geometry, no AutoCAD and no component: every slot is <c>object</c>, and which fields a
    /// component has is that component's business.</para>
    /// </summary>
    public class CantileverComponentEditorShell : Control
    {
        static CantileverComponentEditorShell()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(CantileverComponentEditorShell),
                new FrameworkPropertyMetadata(typeof(CantileverComponentEditorShell)));
        }

        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register(nameof(Header), typeof(object), typeof(CantileverComponentEditorShell), new PropertyMetadata(null));

        public static readonly DependencyProperty ParametersProperty =
            DependencyProperty.Register(nameof(Parameters), typeof(object), typeof(CantileverComponentEditorShell), new PropertyMetadata(null));

        public static readonly DependencyProperty SectionPickerProperty =
            DependencyProperty.Register(nameof(SectionPicker), typeof(object), typeof(CantileverComponentEditorShell), new PropertyMetadata(null));

        public static readonly DependencyProperty PreviewProperty =
            DependencyProperty.Register(nameof(Preview), typeof(object), typeof(CantileverComponentEditorShell), new PropertyMetadata(null));

        public static readonly DependencyProperty DiagnosticsProperty =
            DependencyProperty.Register(nameof(Diagnostics), typeof(object), typeof(CantileverComponentEditorShell), new PropertyMetadata(null));

        public static readonly DependencyProperty BomSummaryProperty =
            DependencyProperty.Register(nameof(BomSummary), typeof(object), typeof(CantileverComponentEditorShell), new PropertyMetadata(null));

        public static readonly DependencyProperty ActionsProperty =
            DependencyProperty.Register(nameof(Actions), typeof(object), typeof(CantileverComponentEditorShell), new PropertyMetadata(null));

        /// <summary>What the user is editing, in one line. Optional.</summary>
        public object Header { get => GetValue(HeaderProperty); set => SetValue(HeaderProperty, value); }

        /// <summary>The component's own fields. The ONE part that differs between the four editors.</summary>
        public object Parameters { get => GetValue(ParametersProperty); set => SetValue(ParametersProperty, value); }

        /// <summary>Its searchable section picker (or pickers).</summary>
        public object SectionPicker { get => GetValue(SectionPickerProperty); set => SetValue(SectionPickerProperty, value); }

        /// <summary>The component's own preview, which takes the space the parameters do not.</summary>
        public object Preview { get => GetValue(PreviewProperty); set => SetValue(PreviewProperty, value); }

        /// <summary>
        /// The component's diagnostics, shown HERE and not saved up for the main window.
        ///
        /// Hiding a component's own problems until the user goes back is regression 12 of this round: the place
        /// to say that a section is not eligible is the window where it was chosen.
        /// </summary>
        public object Diagnostics { get => GetValue(DiagnosticsProperty); set => SetValue(DiagnosticsProperty, value); }

        /// <summary>Its recipe: what this component is made of, as the BOM will count it.</summary>
        public object BomSummary { get => GetValue(BomSummaryProperty); set => SetValue(BomSummaryProperty, value); }

        /// <summary>Accept, cancel, restore and the independent insertion.</summary>
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
