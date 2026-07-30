using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using RackCad.Application.StructuralSections;
using RackCad.Application.StructuralSections.Geometry;
using RackCad.Application.Systems.Cantilever;
using RackCad.Domain.Systems.Cantilever;

namespace RackCad.UI.Systems.Cantilever.Components
{
    /// <summary>
    /// The brace configurator (I-37D ronda 2).
    ///
    /// Two products with the same function and different anatomy: a bolted structural profile, and a cold-rolled
    /// rod that cannot be drilled and so needs an ADAPTER at each end (ADR-0027, D7). Both sets of fields are
    /// shown, and the one not in force is disabled rather than hidden — a field that vanishes leaves the user
    /// wondering whether the value survived. It did: it is dormant data, exactly as the design declares.
    /// </summary>
    public partial class CantileverBraceWindow : Window
    {
        private const string NumberFormat = "0.###";

        private static readonly string[] Kinds = { "Cold rolled (varilla)", "Estructural (perfil)" };

        private readonly StructuralSectionGeometryFactory geometry;
        private readonly CantileverBracingDesign original;
        private readonly CantileverBracePlan resolved;

        private CantileverBracingDesign working;
        private bool suppressSync;

        public CantileverBraceWindow(
            CantileverBracingDesign bracing,
            StructuralSectionCatalog catalogue,
            CantileverBracePlan resolved = null)
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

            KindBox.ItemsSource = Kinds;

            // Channel and angle: the families a structural brace may be, declared where the rule lives.
            SectionPicker.Load(
                catalogue, new[] { StructuralSectionFamily.Channel, StructuralSectionFamily.Angle });

            LoadFromWorking();
        }

        /// <summary>The accepted bracing design, or null when the user cancelled.</summary>
        public CantileverBracingDesign Result { get; private set; }

        internal CantileverBracingDesign Working => working;

        internal CantileverViewPlan CurrentPreviewPlan =>
            resolved == null ? null : CantileverViewPlanBuilder.BuildBrace(resolved, SelectedView(), geometry);

        private void LoadFromWorking()
        {
            suppressSync = true;

            try
            {
                KindBox.SelectedIndex = working.BraceKind == CantileverBraceBodyKind.StructuralSection ? 1 : 0;
                RodDiameterBox.SetNumber(working.ColdRolled?.Diameter, NumberFormat);
                SectionPicker.SelectedSectionId = working.BraceSectionId;
                SectionText.Text = "Sección: " + (working.BraceSectionId ?? "(sin elegir)");
            }
            finally
            {
                suppressSync = false;
            }

            Refresh();
        }

        private void Refresh()
        {
            var isColdRolled = working.BraceKind == CantileverBraceBodyKind.ColdRolledRound;

            RodDiameterBox.IsEnabled = isColdRolled;
            SectionPicker.IsEnabled = !isColdRolled;

            DiagnosticsText.Text = !isColdRolled && string.IsNullOrWhiteSpace(working.BraceSectionId)
                ? "⛔ Un tensor estructural sin sección se rechaza: no hay un id por omisión aprobado."
                : "Sin diagnósticos.";

            BomText.Text = isColdRolled
                ? "Receta: una varilla por tensor, 2 adaptadores y 2 cartabones calibre 10 por adaptador."
                : "Receta: un perfil por tensor, con un agujero de 9/16 in a 1.25 in de cada extremo.";

            DerivedText.Text = resolved == null
                ? "La línea todavía no tiene un tensor resuelto que medir."
                : string.Format(
                    CultureInfo.InvariantCulture,
                    "Derivado de la línea actual: cuerpo {0} in · {1} adaptadores",
                    resolved.BodyLength.ToString("0.###", CultureInfo.InvariantCulture),
                    resolved.Adapters.Count);

            RenderPreview();
        }

        private void Kind_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (suppressSync)
            {
                return;
            }

            working.BraceKind = KindBox.SelectedIndex == 1
                ? CantileverBraceBodyKind.StructuralSection
                : CantileverBraceBodyKind.ColdRolledRound;

            Refresh();
        }

        private void SectionChosen(object sender, string sectionId)
        {
            if (suppressSync)
            {
                return;
            }

            working.BraceSectionId = sectionId;
            SectionText.Text = "Sección: " + (sectionId ?? "(sin elegir)");
            Refresh();
        }

        private void Input_Changed(object sender, RoutedEventArgs e)
        {
            if (suppressSync || RodDiameterBox.HasError)
            {
                DiagnosticsText.Text = RodDiameterBox.HasError
                    ? "Diámetro de la varilla: " + (RodDiameterBox.ErrorMessage ?? "valor inválido.")
                    : DiagnosticsText.Text;

                return;
            }

            (working.ColdRolled ??= new CantileverColdRolledBraceDesign()).Diameter =
                RodDiameterBox.Value ?? working.ColdRolled.Diameter;

            Refresh();
        }

        private CantileverViewKind SelectedView() =>
            PreviewPlantaRadio.IsChecked == true ? CantileverViewKind.Planta : CantileverViewKind.Frontal;

        private void RenderPreview() =>
            CantileverPreviewRenderer.Render(
                PreviewCanvas, CurrentPreviewPlan, "Resuelve primero la línea para ver su tensor.");

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
