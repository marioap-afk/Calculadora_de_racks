using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using RackCad.Application.StructuralSections;
using RackCad.Application.StructuralSections.Geometry;
using RackCad.Application.Systems.Cantilever;
using RackCad.Domain.Systems.Cantilever;
using RackCad.UI.Controls;

namespace RackCad.UI.Systems.Cantilever.Components
{
    /// <summary>
    /// The arm configurator (I-37D ronda 2).
    ///
    /// It carries EXACTLY the parameters the main window used to show for the arm: this round MOVES them, it does
    /// not redefine the arm's geometry or its rules. It edits a copy and hands it back only on «Aceptar».
    ///
    /// <para>An arm cannot be resolved on its own — I-37B resolves it AGAINST a column, because where its
    /// mounting plate bolts depends on that column's punches. So the window is opened with the line's column–base
    /// as CONTEXT, resolves the preview against it, and never invents one.</para>
    /// </summary>
    public partial class CantileverArmWindow : Window
    {
        private const string NumberFormat = "0.###";
        private const double ContextColumnHeight = 96.0;

        private static readonly string[] Arrangements = { "Sencillo", "Canal doble enfrentada", "Canal doble espalda" };
        private static readonly string[] EndPlateModes = { "Ninguna", "Placa", "Placa con tope" };

        private readonly StructuralSectionCatalog catalogue;
        private readonly StructuralSectionGeometryFactory geometry;
        private readonly ICantileverArmSectionPolicy policy;
        private readonly CantileverArmTemplateDesign original;
        private readonly CantileverStationColumnBaseTemplateDesign columnContext;

        private CantileverArmTemplateDesign working;
        private CantileverArmAssembly assembly;
        private bool suppressSync;

        public CantileverArmWindow(
            CantileverArmTemplateDesign arm,
            CantileverStationColumnBaseTemplateDesign columnContext,
            StructuralSectionCatalog catalogue,
            string scopeDescription = null)
        {
            this.catalogue = catalogue ?? throw new ArgumentNullException(nameof(catalogue));
            this.columnContext = columnContext?.DeepCopy() ?? new CantileverStationColumnBaseTemplateDesign();
            geometry = new StructuralSectionGeometryFactory(catalogue);
            policy = CantileverCataloguePolicies.Arm(catalogue);
            original = arm?.DeepCopy() ?? new CantileverArmTemplateDesign();
            working = original.DeepCopy();

            InitializeComponent();

            ArrangementBox.ItemsSource = Arrangements;
            EndPlateModeBox.ItemsSource = EndPlateModes;

            // Any catalogued section may be a single body; the paired arrangements need a channel, and that is
            // the arrangement resolver's rule, restated at the boundary and not invented here.
            SectionPicker.Load(catalogue);

            HeaderText.Text = string.IsNullOrWhiteSpace(scopeDescription)
                ? "Brazo por omisión de la línea"
                : "Brazo — " + scopeDescription;

            ColumnContextText.Text = columnContext?.ColumnSectionId ?? "(la línea no tiene columna elegida)";

            LoadFromWorking();
        }

        /// <summary>The accepted arm, or null when the user cancelled.</summary>
        public CantileverArmTemplateDesign Result { get; private set; }

        internal CantileverArmTemplateDesign Working => working;

        internal CantileverArmAssembly Assembly => assembly;

        internal CantileverViewPlan CurrentPreviewPlan =>
            assembly == null ? null : CantileverViewPlanBuilder.BuildArm(assembly, SelectedView(), geometry);

        private void LoadFromWorking()
        {
            suppressSync = true;

            try
            {
                var body = working.Body ?? new CantileverArmBodyDesign();
                ArrangementBox.SelectedIndex = (int)body.Arrangement;
                SectionPicker.SelectedSectionId = body.SectionId;
                CutLengthBox.SetNumber(body.CutLength, NumberFormat);
                SlopeBox.SetNumber(body.SlopeRisePer12, NumberFormat);

                var plate = working.MountingPlate ?? new CantileverArmMountingPlateTemplateDesign();
                PlateThicknessBox.SetNumber(plate.Thickness, NumberFormat);
                PunchCountBox.SetNumber(plate.VerticalPunchCount, "0");
                VerticalOffsetBox.SetNumber(plate.VerticalEndOffset, NumberFormat);

                var end = working.EndPlate ?? new CantileverArmEndPlateDesign();
                EndPlateModeBox.SelectedIndex = (int)end.Mode;
                EndPlateThicknessBox.SetNumber(end.Thickness, NumberFormat);
                ExtraStopBox.SetNumber(end.ExtraStopHeight, NumberFormat);
            }
            finally
            {
                suppressSync = false;
            }

            Recompute();
        }

        private void SectionChosen(object sender, string sectionId)
        {
            if (suppressSync)
            {
                return;
            }

            Recompute();
        }

        private void Input_Changed(object sender, RoutedEventArgs e)
        {
            if (!suppressSync)
            {
                Recompute();
            }
        }

        private void Combo_Changed(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (!suppressSync)
            {
                Recompute();
            }
        }

        private void Recompute()
        {
            if (!ReadInputs(out var error))
            {
                assembly = null;
                DiagnosticsText.Text = error;
                BomText.Text = string.Empty;
                RenderPreview();
                return;
            }

            SectionText.Text = "Sección: " + (working.Body?.SectionId ?? "(sin elegir)");

            assembly = ResolveAgainstTheColumn();

            DiagnosticsText.Text = assembly == null
                ? "La línea no tiene una columna resuelta, así que el brazo no se puede previsualizar. " +
                  "Configura primero la columna y base."
                : assembly.Diagnostics.Count == 0
                    ? "Sin diagnósticos."
                    : string.Join("\n", assembly.Diagnostics.Select(d => (d.IsBlocking ? "⛔ " : "⚠ ") + d.Message));

            BomText.Text = assembly == null || assembly.Diagnostics.Any(d => d.IsBlocking)
                ? "Sin receta: el brazo no se resolvió."
                : string.Format(
                    CultureInfo.InvariantCulture,
                    "Brazo {0} · corte {1} in · {2} placas · {3} troqueles",
                    working.Body.SectionId,
                    working.Body.CutLength.ToString("0.##", CultureInfo.InvariantCulture),
                    assembly.Plates.Count,
                    assembly.MountingPunches.Count);

            RenderPreview();
        }

        /// <summary>
        /// Resolves the arm against the line's column. Null when that column is not resolvable — an arm has no
        /// geometry of its own to fall back on, and inventing a column would preview a piece nobody ordered.
        /// </summary>
        private CantileverArmAssembly ResolveAgainstTheColumn()
        {
            if (string.IsNullOrWhiteSpace(columnContext.ColumnSectionId))
            {
                return null;
            }

            var column = new CantileverColumnBaseResolver(
                    catalogue, geometry, CantileverCataloguePolicies.ColumnBase(catalogue))
                .Resolve(columnContext.ToColumnBaseDesign(ContextColumnHeight));

            if (column.IsBlocked)
            {
                return null;
            }

            var design = new CantileverArmDesign
            {
                Side = CantileverArmSide.PositiveY,
                Body = working.Body?.DeepCopy() ?? new CantileverArmBodyDesign(),
                MountingPlate = new CantileverArmMountingPlateDesign
                {
                    Thickness = working.MountingPlate?.Thickness ?? CantileverDefaults.PlateThickness,
                    VerticalPunchCount = working.MountingPlate?.VerticalPunchCount ?? CantileverDefaults.ArmVerticalPunchCount,
                    VerticalEndOffset = working.MountingPlate?.VerticalEndOffset,
                    LowerColumnPunchIndex = 0
                },
                EndPlate = working.EndPlate?.DeepCopy() ?? new CantileverArmEndPlateDesign()
            };

            return new CantileverArmResolver(catalogue, geometry, policy).Resolve(design, column, "PREVIEW");
        }

        private bool ReadInputs(out string error)
        {
            error = null;

            var fields = new (NumericField Field, string Label)[]
            {
                (CutLengthBox, "Longitud de corte"),
                (SlopeBox, "Pendiente"),
                (PlateThicknessBox, "Espesor de placa de montaje"),
                (PunchCountBox, "Troqueles verticales"),
                (VerticalOffsetBox, "Margen vertical"),
                (EndPlateThicknessBox, "Espesor de placa de extremo"),
                (ExtraStopBox, "Altura extra de tope")
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

            var body = working.Body ??= new CantileverArmBodyDesign();
            body.Arrangement = Enum.IsDefined(typeof(CantileverArmBodyArrangement), ArrangementBox.SelectedIndex)
                ? (CantileverArmBodyArrangement)ArrangementBox.SelectedIndex
                : CantileverArmBodyArrangement.Single;
            body.SectionId = SectionPicker.SelectedSectionId;
            body.CutLength = Keep(CutLengthBox, body.CutLength);
            body.SlopeRisePer12 = Keep(SlopeBox, body.SlopeRisePer12);

            var plate = working.MountingPlate ??= new CantileverArmMountingPlateTemplateDesign();
            plate.Thickness = Keep(PlateThicknessBox, plate.Thickness);
            plate.VerticalPunchCount = (int)Math.Round(PunchCountBox.Value ?? plate.VerticalPunchCount);
            plate.VerticalEndOffset = VerticalOffsetBox.Value;

            var end = working.EndPlate ??= new CantileverArmEndPlateDesign();
            end.Mode = Enum.IsDefined(typeof(CantileverArmEndPlateMode), EndPlateModeBox.SelectedIndex)
                ? (CantileverArmEndPlateMode)EndPlateModeBox.SelectedIndex
                : CantileverArmEndPlateMode.None;
            end.Thickness = Keep(EndPlateThicknessBox, end.Thickness);
            end.ExtraStopHeight = Keep(ExtraStopBox, end.ExtraStopHeight);

            return true;
        }

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
                PreviewCanvas, CurrentPreviewPlan, "El brazo no se pudo previsualizar.");

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
