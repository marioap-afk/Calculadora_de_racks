using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using RackCad.Application.StructuralSections;
using RackCad.Application.StructuralSections.Geometry;
using RackCad.Application.Systems.Cantilever;
using RackCad.Domain.Systems.Cantilever;
using RackCad.UI.Editor;
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

        /// <summary>The stand-alone insertion the user asked for, or null. Carries its OWN identity.</summary>
        public CantileverComponentInsertionRequest ComponentInsertion { get; private set; }


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

        /// <summary>
        /// Elegir una sección re-deriva cuántas filas de troqueles lleva la placa por omisión.
        ///
        /// Es el único momento en que se pisa ese campo, y no una imposición: la cuenta es la de las filas que
        /// caben en la altura DEL PERFIL ELEGIDO, así que un perfil nuevo la cambia; después el usuario puede
        /// subirla o bajarla y nada se la vuelve a tocar. Escribirla en cada tecla borraría lo que él escribió.
        /// </summary>
        private void SectionChosen(object sender, string sectionId)
        {
            if (suppressSync)
            {
                return;
            }

            SuggestPunchCountFor(sectionId);
            Recompute();
        }

        private void SuggestPunchCountFor(string sectionId)
        {
            if (string.IsNullOrWhiteSpace(sectionId) || !catalogue.TryGetById(sectionId, out var section))
            {
                return;
            }

            var height = geometry.Get(section.SectionId, SectionDetailLevel.Tabulated).Bounds.Height;
            var margin = VerticalOffsetBox.Value ?? CantileverDefaults.ArmMountingPlateVerticalEndOffset;

            var count = CantileverArmPunchCountRule.For(
                height, margin, CantileverDefaults.RegularColumnPunchPitch);

            suppressSync = true;

            try
            {
                PunchCountBox.SetNumber(count, "0");
            }
            finally
            {
                suppressSync = false;
            }
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
                UpdateInsertAvailability();
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

            UpdateInsertAvailability();
            RenderPreview();
        }

        /// <summary>ADR-0029 D6, I-39C: <c>Insertar</c> materialises, so it must be OFF whenever it cannot produce
        /// anything — and say why. It used to stay on for an arm that does not resolve, and pressing it only wrote a
        /// line into diagnostics: an action enabled with no effect, which D6 calls the greater violation.</summary>
        private void UpdateInsertAvailability()
        {
            var resolves = assembly != null && !assembly.Diagnostics.Any(d => d.IsBlocking);

            InsertButton.IsEnabled = resolves;
            InsertButton.ToolTip = resolves
                ? "Dibuja la pieza sola, como bloque independiente y NO editable. No modifica la línea que estás editando."
                : "No se puede insertar un brazo que no se resolvió.";
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
            CloseWith(true);
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Result = null;
            CloseWith(false);
        }


        /// <summary>
        /// Draws the arm ALONE, as a non-editable block with its own identity.
        ///
        /// Its main view is the LATERAL — that is where an arm's cut, its slope and its plate read. The frontal
        /// and the planta are added only when they say something the lateral does not: a redundant projection is
        /// two blocks the user has to delete, and ADR-0027 D6 already refuses to draw what nobody needs.
        /// </summary>
        private void InsertComponent_Click(object sender, RoutedEventArgs e)
        {
            if (assembly == null || assembly.Diagnostics.Any(d => d.IsBlocking))
            {
                DiagnosticsText.Text = "No se puede insertar un brazo que no se resolvió.";
                return;
            }

            var lateral = CantileverViewPlanBuilder.BuildArm(assembly, CantileverViewKind.Lateral, geometry);
            var views = new System.Collections.Generic.List<CantileverViewPlan>();

            if (!lateral.IsEmpty)
            {
                views.Add(lateral);
            }

            foreach (var kind in new[] { CantileverViewKind.Frontal, CantileverViewKind.Planta })
            {
                var plan = CantileverViewPlanBuilder.BuildArm(assembly, kind, geometry);

                if (!plan.IsEmpty && views.All(v => v.Signature() != plan.Signature()))
                {
                    views.Add(plan);
                }
            }

            if (views.Count == 0)
            {
                DiagnosticsText.Text = "El brazo no dibuja nada en ninguna vista.";
                return;
            }

            ComponentInsertion = new CantileverComponentInsertionRequest(
                CantileverComponentKind.Arm, views,
                CantileverColumnBaseEditorState.Designation(working.Body?.SectionId));

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
