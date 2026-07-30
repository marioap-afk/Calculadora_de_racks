using System;
using System.Globalization;
using System.Windows;
using RackCad.Application.StructuralSections;
using RackCad.Application.StructuralSections.Geometry;
using RackCad.Application.Systems.Cantilever;
using RackCad.Domain.Systems.Cantilever;

namespace RackCad.UI.Systems.Cantilever.Components
{
    /// <summary>
    /// The separator configurator (I-37D ronda 2).
    ///
    /// A separator is the piece of an INTERVAL, and its cut length is derived from the holes of the two column
    /// plates — never typed (ADR-0027, D5). So this window edits exactly what a separator has of its own: its
    /// section. Everything else it shows is DERIVED and read-only, which is the honest shape for a piece whose
    /// dimensions are decided by where it bolts.
    ///
    /// <para>Its preview is the separator the LINE already resolved, when there is one: a separator with no
    /// interval to belong to has no length, and inventing one would preview a piece nobody ordered.</para>
    /// </summary>
    public partial class CantileverSeparatorWindow : Window
    {
        private readonly StructuralSectionGeometryFactory geometry;
        private readonly CantileverBracingDesign original;
        private readonly CantileverSeparatorPlan resolved;

        private CantileverBracingDesign working;

        public CantileverSeparatorWindow(
            CantileverBracingDesign bracing,
            StructuralSectionCatalog catalogue,
            CantileverSeparatorPlan resolved = null)
        {
            if (catalogue == null)
            {
                throw new ArgumentNullException(nameof(catalogue));
            }

            geometry = new StructuralSectionGeometryFactory(catalogue);
            original = bracing?.DeepCopy() ?? new CantileverBracingDesign();
            working = original.DeepCopy();
            this.resolved = resolved;

            InitializeComponent();

            // A separator is a channel. The restriction is the product's, declared where the default lives.
            SectionPicker.Load(catalogue, new[] { StructuralSectionFamily.Channel });

            LoadFromWorking();
        }

        /// <summary>The accepted bracing design, or null when the user cancelled.</summary>
        public CantileverBracingDesign Result { get; private set; }

        internal CantileverBracingDesign Working => working;

        internal CantileverViewPlan CurrentPreviewPlan =>
            resolved == null
                ? null
                : CantileverViewPlanBuilder.BuildSeparator(resolved, SelectedView(), geometry);

        private void LoadFromWorking()
        {
            SectionPicker.SelectedSectionId = working.SeparatorSectionId;
            SectionText.Text = "Sección: " + (working.SeparatorSectionId ?? "(sin elegir)");

            DerivedText.Text = resolved == null
                ? "La línea todavía no tiene un separador resuelto que medir."
                : string.Format(
                    CultureInfo.InvariantCulture,
                    "Derivado de la línea actual: corte {0} in · {1} agujeros · elevación {2} in",
                    resolved.CutLength.ToString("0.###", CultureInfo.InvariantCulture),
                    resolved.Punches.Count,
                    resolved.ElevationZ.ToString("0.##", CultureInfo.InvariantCulture));

            DiagnosticsText.Text = string.IsNullOrWhiteSpace(working.SeparatorSectionId)
                ? "⛔ Sin sección, la línea no se resuelve."
                : "Sin diagnósticos.";

            BomText.Text = resolved == null
                ? "Receta: un perfil por separador, más las dos placas de columna en las que atornilla."
                : "Receta: " + resolved.Member.Placement.SectionId.Value + " · corte " +
                  resolved.CutLength.ToString("0.###", CultureInfo.InvariantCulture) + " in.";

            RenderPreview();
        }

        private void SectionChosen(object sender, string sectionId)
        {
            working.SeparatorSectionId = sectionId;
            LoadFromWorking();
        }

        private CantileverViewKind SelectedView() =>
            PreviewPlantaRadio.IsChecked == true ? CantileverViewKind.Planta : CantileverViewKind.Frontal;

        private void RenderPreview() =>
            CantileverPreviewRenderer.Render(
                PreviewCanvas, CurrentPreviewPlan,
                "Resuelve primero la línea para ver su separador.");

        private void PreviewCanvas_SizeChanged(object sender, SizeChangedEventArgs e) => RenderPreview();

        private void PreviewView_Changed(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                RenderPreview();
            }
        }

        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            Result = working.DeepCopy();
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Result = null;
            DialogResult = false;
            Close();
        }

        private void Restore_Click(object sender, RoutedEventArgs e)
        {
            working = original.DeepCopy();
            LoadFromWorking();
        }
    }
}
