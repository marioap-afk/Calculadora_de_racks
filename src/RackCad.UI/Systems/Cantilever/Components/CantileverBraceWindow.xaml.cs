using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using RackCad.Application.StructuralSections;
using RackCad.Application.StructuralSections.Geometry;
using RackCad.Application.Systems.Cantilever;
using RackCad.Domain.Systems.Cantilever;
using RackCad.UI.Editor;

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

            // ADR-0029 D6, I-39C: sin linea resuelta no hay nada que insertar —el corte del tensor se DERIVA de los
            // agujeros de sus separadores—, asi que Insertar se apaga con el motivo a la vista. Antes quedaba
            // habilitado y pulsarlo solo escribia una linea en el diagnostico: una accion habilitada sin efecto, que
            // es lo que D6 llama la violacion mayor. `resolved` no cambia despues de construir, asi que se fija aqui.
            if (resolved == null)
            {
                InsertButton.IsEnabled = false;
                InsertButton.ToolTip =
                    "Resuelve primero la línea: el corte del tensor sale de los agujeros de sus separadores.";
            }
        }

        /// <summary>The accepted bracing design, or null when the user cancelled.</summary>
        public CantileverBracingDesign Result { get; private set; }

        /// <summary>The stand-alone insertion the user asked for, or null. Carries its OWN identity.</summary>
        public CantileverComponentInsertionRequest ComponentInsertion { get; private set; }


        internal CantileverBracingDesign Working => working;

        /// <summary>
        /// El plan que la previa dibuja: una vista de la línea, o la SECCIÓN del adaptador.
        ///
        /// La sección no entra por <c>BuildBrace</c> y no es un descuido: su cámara no es fija, la pone el
        /// eje de corte de la pieza. Pedirla por la puerta de las vistas de línea falla en cerrado a
        /// propósito.
        /// </summary>
        internal CantileverViewPlan CurrentPreviewPlan
        {
            get
            {
                if (resolved == null)
                {
                    return null;
                }

                return PreviewAdapterSectionRadio.IsChecked == true
                    ? CantileverViewPlanBuilder.BuildAdapterSection(resolved, 0, geometry)
                    : CantileverViewPlanBuilder.BuildBrace(resolved, SelectedView(), geometry);
            }
        }

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
                PreviewCanvas,
                CurrentPreviewPlan,
                PreviewAdapterSectionRadio.IsChecked == true
                    ? "Un tensor de perfil estructural se atornilla directo: no lleva adaptador que seccionar."
                    : "Resuelve primero la línea para ver su tensor.");

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
            CloseWith(true);
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Result = null;
            CloseWith(false);
        }


        /// <summary>
        /// Draws the brace ALONE, as a non-editable block with its own identity.
        ///
        /// ONE view: the plane of the brace, which is the frontal. The two diagonals of a panel share that plane
        /// by decision (ADR-0027, D6), so a second projection would add nothing and invent a drawing nobody uses.
        /// </summary>
        private void InsertComponent_Click(object sender, RoutedEventArgs e)
        {
            if (resolved == null)
            {
                DiagnosticsText.Text =
                    "Resuelve primero la línea: el corte del tensor sale de los agujeros de sus separadores.";
                return;
            }

            var plan = CantileverViewPlanBuilder.BuildBrace(resolved, CantileverViewKind.Frontal, geometry);

            if (plan.IsEmpty)
            {
                DiagnosticsText.Text = "El tensor no dibuja nada.";
                return;
            }

            ComponentInsertion = new CantileverComponentInsertionRequest(
                CantileverComponentKind.Brace, new[] { plan },
                working.BraceKind == CantileverBraceBodyKind.ColdRolledRound
                    ? "CR_" + (working.ColdRolled?.Diameter ?? 0.0).ToString("0.###", CultureInfo.InvariantCulture)
                    : CantileverColumnBaseEditorState.Designation(working.BraceSectionId));

            Result = working.DeepCopy();
            CloseWith(true);
        }

        private void Restore_Click(object sender, RoutedEventArgs e)
        {
            working = original.DeepCopy();
            LoadFromWorking();
        }

        /// <summary>
        /// Closes reporting <paramref name="result"/> as the dialog outcome.
        ///
        /// Setting <see cref="Window.DialogResult"/> THROWS when the window was not shown with
        /// <c>ShowDialog</c> — a caller that merely constructed it, or a test. The contract the caller reads is
        /// <c>Result</c> and the insertion request; the dialog flag is a convenience of the modal path, so it is
        /// set when it can be and skipped when it cannot, instead of turning a legitimate use into an exception.
        /// </summary>
        private void CloseWith(bool result)
        {
            try
            {
                DialogResult = result;
            }
            catch (InvalidOperationException)
            {
                // Not shown as a dialog: `Result` already carries the outcome.
            }

            Close();
        }

    }
}
