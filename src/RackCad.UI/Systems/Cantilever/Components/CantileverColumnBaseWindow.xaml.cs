using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using RackCad.Application.StructuralSections;
using RackCad.Application.StructuralSections.Geometry;
using RackCad.Application.Systems.Cantilever;
using RackCad.Domain.Systems.Cantilever;
using RackCad.UI.Controls;
using RackCad.UI.Editor;

namespace RackCad.UI.Systems.Cantilever.Components
{
    /// <summary>
    /// The column–base configurator (I-37D ronda 2).
    ///
    /// It edits a COPY of the line's column–base template and hands it back only on «Aceptar»: cancelling cannot
    /// mutate the design it was opened from. The «la base sigue a la columna» rule is NOT here — it lives in
    /// <see cref="CantileverColumnBaseEditorState"/>, in Application, so the window only reads controls and asks.
    ///
    /// <para>Its preview is built by the SAME authority the line and the Plugin use
    /// (<see cref="CantileverViewPlanBuilder.BuildColumnBase"/>), so the picture here, the picture in the line
    /// and the entities in the drawing are one projection with three consumers.</para>
    /// </summary>
    public partial class CantileverColumnBaseWindow : Window
    {
        private const string NumberFormat = "0.###";

        /// <summary>A height used ONLY to draw the piece here. The real one is the line's, shared by its stations.</summary>
        private const double DefaultPreviewHeight = 96.0;

        private readonly StructuralSectionCatalog catalogue;
        private readonly StructuralSectionGeometryFactory geometry;
        private readonly ICantileverColumnBaseSectionPolicy policy;
        private readonly CantileverStationColumnBaseTemplateDesign original;

        private CantileverColumnBaseEditorState state;
        private CantileverColumnBaseAssembly assembly;
        private bool suppressSync;

        public CantileverColumnBaseWindow(
            CantileverStationColumnBaseTemplateDesign template,
            StructuralSectionCatalog catalogue,
            bool canInsertInAutoCad = false)
        {
            this.catalogue = catalogue ?? throw new ArgumentNullException(nameof(catalogue));
            geometry = new StructuralSectionGeometryFactory(catalogue);
            policy = CantileverCataloguePolicies.ColumnBase(catalogue);
            original = template?.DeepCopy() ?? new CantileverStationColumnBaseTemplateDesign();

            InitializeComponent();

            state = new CantileverColumnBaseEditorState(original, policy);

            // The eligible families come from the SYSTEM's policy, not from the picker: which families a
            // Cantilever column may be built from is I-37A's rule, declared in one place.
            var families = (policy as CantileverCatalogueColumnBasePolicy)?.AllowedFamilies
                           ?? new[] { StructuralSectionFamily.W };

            ColumnPicker.Load(catalogue, families);
            BasePicker.Load(catalogue, families);

            InsertButton.IsEnabled = canInsertInAutoCad;

            if (!canInsertInAutoCad)
            {
                InsertButton.ToolTip = "Disponible solo cuando la ventana se abre desde AutoCAD.";
            }

            LoadFromState();
        }

        /// <summary>The accepted template, or null when the user cancelled.</summary>
        public CantileverStationColumnBaseTemplateDesign Result { get; private set; }

        /// <summary>Set when the user asked to draw this component on its own.</summary>
        public bool InsertRequested { get; private set; }

        /// <summary>The stand-alone insertion the user asked for, or null. Carries its OWN identity.</summary>
        public CantileverComponentInsertionRequest ComponentInsertion { get; private set; }

        // ---- Test seams -------------------------------------------------------------------------------------

        internal CantileverColumnBaseEditorState State => state;

        internal CantileverColumnBaseAssembly Assembly => assembly;

        internal CantileverViewPlan CurrentPreviewPlan =>
            assembly == null ? null : CantileverViewPlanBuilder.BuildColumnBase(assembly, SelectedView(), geometry);

        // ---- Loading ------------------------------------------------------------------------------------------

        private void LoadFromState()
        {
            suppressSync = true;

            try
            {
                var template = state.Template;

                ColumnPlateThicknessBox.SetNumber(template.ColumnBottomPlate?.Thickness, NumberFormat);
                BaseLengthBox.SetNumber(template.Base?.Length, NumberFormat);
                BaseFrontPlateBox.SetNumber(template.Base?.FrontPlate?.Thickness, NumberFormat);
                BaseRearPlateBox.SetNumber(template.Base?.RearPlate?.Thickness, NumberFormat);
                BaseGussetBox.SetNumber(template.Base?.Gusset?.Thickness, NumberFormat);

                var punches = template.Connection?.Punches ?? new CantileverPunchParameters();
                PunchDiameterBox.SetNumber(punches.Diameter, NumberFormat);
                PunchHorizontalOffsetBox.SetNumber(punches.HorizontalEndOffset, NumberFormat);
                ConnectionPitchBox.SetNumber(punches.ConnectionPitch, NumberFormat);
                ConnectionPunchesAboveBaseBox.SetNumber(punches.ConnectionPunchesAboveBase, "0");
                RearPlateOffsetBox.SetNumber(punches.RearPlateVerticalEndOffset, NumberFormat);
                RegularPitchBox.SetNumber(punches.RegularColumnPitch, NumberFormat);
                BottomPlatePitchBox.SetNumber(punches.ColumnBottomPlatePitch, NumberFormat);
                BottomPlateEndOffsetBox.SetNumber(punches.ColumnBottomPlateEndOffset, NumberFormat);
                TopPunchOffsetBox.SetNumber(punches.ColumnTopPunchOffset, NumberFormat);

                if (!PreviewHeightBox.Value.HasValue)
                {
                    PreviewHeightBox.SetNumber(DefaultPreviewHeight, NumberFormat);
                }

                ColumnPicker.SelectedSectionId = template.ColumnSectionId;
                BasePicker.SelectedSectionId = template.Base?.SectionId;
                FollowColumnCheck.IsChecked = template.BaseFollowsColumn;
            }
            finally
            {
                suppressSync = false;
            }

            Recompute();
        }

        // ---- The section rules, all of them asked to Application ---------------------------------------------

        private void ColumnChosen(object sender, string sectionId)
        {
            if (suppressSync)
            {
                return;
            }

            state.SelectColumn(sectionId);
            SyncSectionControls();
            Recompute();
        }

        private void BaseChosen(object sender, string sectionId)
        {
            if (suppressSync)
            {
                return;
            }

            state.SelectBase(sectionId);
            SyncSectionControls();
            Recompute();
        }

        private void UseSameSection_Click(object sender, RoutedEventArgs e)
        {
            state.UseSameSectionAsColumn();
            SyncSectionControls();
            Recompute();
        }

        private void FollowColumn_Click(object sender, RoutedEventArgs e)
        {
            if (FollowColumnCheck.IsChecked == true)
            {
                // Turning it on APPLIES it: leaving the two disagreeing until the next column change is the
                // surprise the rule exists to remove.
                state.UseSameSectionAsColumn();
            }
            else
            {
                state.SelectBase(state.BaseSectionId);
            }

            SyncSectionControls();
            Recompute();
        }

        private void SyncSectionControls()
        {
            suppressSync = true;

            try
            {
                ColumnPicker.SelectedSectionId = state.ColumnSectionId;
                BasePicker.SelectedSectionId = state.BaseSectionId;
                FollowColumnCheck.IsChecked = state.BaseFollowsColumn;
                ColumnSectionText.Text = "Sección: " + (state.ColumnSectionId ?? "(sin elegir)");
                BaseSectionText.Text = "Sección: " + (state.BaseSectionId ?? "(sin elegir)");
            }
            finally
            {
                suppressSync = false;
            }
        }

        // ---- Recompute ----------------------------------------------------------------------------------------

        private void Input_Changed(object sender, RoutedEventArgs e)
        {
            if (suppressSync)
            {
                return;
            }

            Recompute();
        }

        private void Recompute()
        {
            if (!ReadInputs(out var fieldError))
            {
                assembly = null;
                DiagnosticsText.Text = fieldError;
                BomText.Text = string.Empty;
                AcceptButton.IsEnabled = false;
                RenderPreview();
                return;
            }

            SyncSectionControls();

            var height = PreviewHeightBox.Value ?? DefaultPreviewHeight;
            assembly = new CantileverColumnBaseResolver(catalogue, geometry, policy)
                .Resolve(state.Template.ToColumnBaseDesign(height));

            // The component's own problems are shown HERE and not saved up for the main window.
            var messages = state.Diagnostics.Concat(assembly.Diagnostics).ToList();

            DiagnosticsText.Text = messages.Count == 0
                ? "Sin diagnósticos."
                : string.Join("\n", messages.Select(d => (d.IsBlocking ? "⛔ " : "⚠ ") + d.Message));

            BomText.Text = assembly.IsBlocked
                ? "Sin receta: la pieza no se resolvió."
                : Recipe(assembly);

            AcceptButton.IsEnabled = true; // a blocked piece is still an intent the user may keep editing
            RenderPreview();
        }

        private static string Recipe(CantileverColumnBaseAssembly columnBase) => string.Format(
            CultureInfo.InvariantCulture,
            "Columna {0} · base {1} · {2} placas · {3} troqueles",
            columnBase.Column.Placement.SectionId.Value,
            columnBase.Base.Placement.SectionId.Value,
            new[] { columnBase.ColumnBottomPlate, columnBase.BaseFrontPlate, columnBase.BaseRearPlate }.Count(p => p != null),
            columnBase.AllPunches.Count);

        private bool ReadInputs(out string error)
        {
            error = null;

            var fields = new (NumericField Field, string Label)[]
            {
                (ColumnPlateThicknessBox, "Espesor de placa inferior"),
                (BaseLengthBox, "Longitud de base"),
                (BaseFrontPlateBox, "Espesor de placa frontal"),
                (BaseRearPlateBox, "Espesor de placa posterior"),
                (BaseGussetBox, "Espesor de cartabón"),
                (PunchDiameterBox, "Diámetro de troquel"),
                (PunchHorizontalOffsetBox, "Margen horizontal a extremo"),
                (ConnectionPitchBox, "Paso de conexión"),
                (ConnectionPunchesAboveBaseBox, "Troqueles de conexión sobre la base"),
                (RearPlateOffsetBox, "Margen vertical de placa posterior"),
                (RegularPitchBox, "Paso regular de columna"),
                (BottomPlatePitchBox, "Paso de placa inferior"),
                (BottomPlateEndOffsetBox, "Margen de extremo de placa inferior"),
                (TopPunchOffsetBox, "Margen del troquel superior"),
                (PreviewHeightBox, "Altura de la vista previa")
            };

            var broken = fields.Where(f => f.Field.HasError).ToList();

            if (broken.Count > 0)
            {
                error = broken.Count == 1
                    ? broken[0].Label + ": " + (broken[0].Field.ErrorMessage ?? "valor inválido.")
                    : "Faltan o son inválidos " + broken.Count + " campos: "
                      + string.Join(", ", broken.Select(f => f.Label)) + ".";

                return false;
            }

            var template = state.Template;
            (template.ColumnBottomPlate ??= new CantileverPlateDesign()).Thickness =
                Keep(ColumnPlateThicknessBox, template.ColumnBottomPlate.Thickness);

            var baseDesign = template.Base ??= new CantileverBaseDesign();
            baseDesign.Length = Keep(BaseLengthBox, baseDesign.Length);
            (baseDesign.FrontPlate ??= new CantileverPlateDesign()).Thickness =
                Keep(BaseFrontPlateBox, baseDesign.FrontPlate.Thickness);
            (baseDesign.RearPlate ??= new CantileverPlateDesign()).Thickness =
                Keep(BaseRearPlateBox, baseDesign.RearPlate.Thickness);
            (baseDesign.Gusset ??= new CantileverGussetDesign()).Thickness =
                Keep(BaseGussetBox, baseDesign.Gusset.Thickness);

            var punches = (template.Connection ??= new CantileverColumnBaseConnectionDesign()).Punches
                ??= new CantileverPunchParameters();

            punches.Diameter = Keep(PunchDiameterBox, punches.Diameter);
            punches.HorizontalEndOffset = Keep(PunchHorizontalOffsetBox, punches.HorizontalEndOffset);
            punches.ConnectionPitch = Keep(ConnectionPitchBox, punches.ConnectionPitch);
            punches.ConnectionPunchesAboveBase =
                (int)Math.Round(ConnectionPunchesAboveBaseBox.Value ?? punches.ConnectionPunchesAboveBase);
            punches.RearPlateVerticalEndOffset = Keep(RearPlateOffsetBox, punches.RearPlateVerticalEndOffset);
            punches.RegularColumnPitch = Keep(RegularPitchBox, punches.RegularColumnPitch);
            punches.ColumnBottomPlatePitch = Keep(BottomPlatePitchBox, punches.ColumnBottomPlatePitch);
            punches.ColumnBottomPlateEndOffset = BottomPlateEndOffsetBox.Value;
            punches.ColumnTopPunchOffset = TopPunchOffsetBox.Value;

            return true;
        }

        /// <summary>See <c>RackCantileverWindow.Keep</c>: a field the user did not touch must not change the design.</summary>
        private static double Keep(NumericField field, double current)
        {
            if (!field.Value.HasValue)
            {
                return current;
            }

            var shown = field.Value.Value.ToString(NumberFormat, CultureInfo.InvariantCulture);
            var stored = current.ToString(NumberFormat, CultureInfo.InvariantCulture);

            return string.Equals(shown, stored, StringComparison.Ordinal) ? current : field.Value.Value;
        }

        // ---- Preview -------------------------------------------------------------------------------------------

        private CantileverViewKind SelectedView()
        {
            if (PreviewLateralRadio.IsChecked == true)
            {
                return CantileverViewKind.Lateral;
            }

            return PreviewPlantaRadio.IsChecked == true
                ? CantileverViewKind.Planta
                : CantileverViewKind.Frontal;
        }

        private void RenderPreview() =>
            CantileverPreviewRenderer.Render(
                PreviewCanvas, CurrentPreviewPlan, "Corrige los datos: la pieza no se resolvió.");

        private void PreviewCanvas_SizeChanged(object sender, SizeChangedEventArgs e) => RenderPreview();

        private void PreviewView_Changed(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded)
            {
                return;
            }

            RenderPreview();
        }

        // ---- Actions ---------------------------------------------------------------------------------------------

        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            Result = state.Accept();
            CloseWith(true);
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Result = null; // cancelling mutates nothing: the state edited a copy
            CloseWith(false);
        }

        private void Restore_Click(object sender, RoutedEventArgs e)
        {
            state = new CantileverColumnBaseEditorState(original, policy);
            LoadFromState();
        }

        /// <summary>
        /// Draws the column–base ALONE, as a non-editable block with its own identity.
        ///
        /// The three views are built from the assembly the preview is already showing, so what reaches the
        /// drawing is the picture the user approved. It does not touch the line being edited: the accepted
        /// template still travels in <see cref="Result"/>, and the insertion is a separate request.
        /// </summary>
        private void InsertComponent_Click(object sender, RoutedEventArgs e)
        {
            if (assembly == null || assembly.IsBlocked)
            {
                DiagnosticsText.Text = "No se puede insertar una pieza que no se resolvió.";
                return;
            }

            var views = new[] { CantileverViewKind.Frontal, CantileverViewKind.Lateral, CantileverViewKind.Planta }
                .Select(v => CantileverViewPlanBuilder.BuildColumnBase(assembly, v, geometry))
                .Where(p => !p.IsEmpty)
                .ToList();

            if (views.Count == 0)
            {
                DiagnosticsText.Text = "La pieza no dibuja nada en ninguna vista.";
                return;
            }

            ComponentInsertion = new CantileverComponentInsertionRequest(
                CantileverComponentKind.ColumnBase, views,
                CantileverColumnBaseEditorState.Designation(state.ColumnSectionId));

            Result = state.Accept();
            InsertRequested = true;
            CloseWith(true);
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
