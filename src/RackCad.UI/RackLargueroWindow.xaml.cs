using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.Settings;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;

namespace RackCad.UI
{
    /// <summary>
    /// Editor for a LARGUERO component (a beam = 1 profile + 2 ménsulas). VISUAL + BOM only for now (no AutoCAD block):
    /// pick a profile / peralte / length / ménsula, see a schematic + its bill of materials, and save it to the design
    /// library. No "Insertar" — it doesn't draw into the drawing yet.
    /// </summary>
    public partial class RackLargueroWindow : Window
    {
        // Frozen once — DrawPreview runs per keystroke/selection change and used to allocate fresh brushes each time.
        private static readonly Brush BeamBrush = UiSupport.FrozenBrush(Color.FromRgb(0x3D, 0xC9, 0x86));
        private static readonly Brush MensulaBrush = UiSupport.FrozenBrush(Color.FromRgb(0x2E, 0x9C, 0x66));
        private static readonly Brush LabelBrush = UiSupport.FrozenBrush(Color.FromRgb(0x61, 0x70, 0x80));

        private readonly RackCatalog catalog;
        private bool ready;

        /// <summary>The library project this larguero was opened from, if any, so a re-save preserves its unknown JSON
        /// metadata and schema version instead of stamping a fresh document (I-11). Null for a brand-new design.</summary>
        private RackProject sourceProject;

        private void SetStatus(string message, bool isError) => UiSupport.SetStatus(StatusText, message, isError);

        public RackLargueroWindow()
        {
            InitializeComponent();
            catalog = UiSupport.LoadCatalogSafe();

            ProfileBox.ItemsSource = UiSupport.ToOptions(catalog?.BeamProfiles);

            // Ménsula options: an "(auto)" entry (empty id = derived from the profile) + every catalog ménsula.
            var mensulas = new List<CatalogOption> { new CatalogOption(string.Empty, "(auto — según perfil)") };
            mensulas.AddRange(UiSupport.ToOptions(catalog?.Mensulas));
            MensulaBox.ItemsSource = mensulas;
            MensulaBox.SelectedIndex = 0;

            if (ProfileBox.Items.Count > 0) ProfileBox.SelectedIndex = 0;

            ready = true;
            RefreshPeraltes();
            DrawPreview();
        }

        private void Profile_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (!ready) return;
            RefreshPeraltes();
            DrawPreview();
        }

        private void Field_Changed(object sender, RoutedEventArgs e)
        {
            if (!ready) return;
            DrawPreview();
        }

        private void Preview_SizeChanged(object sender, SizeChangedEventArgs e) => DrawPreview();

        private void RefreshPeraltes()
        {
            var raw = catalog?.BeamProfiles
                .FirstOrDefault(b => string.Equals(b?.Id, ProfileBox.SelectedValue as string, StringComparison.OrdinalIgnoreCase))?.Peraltes;
            var options = PeralteList.Parse(raw).Select(v => v.ToString("0.###", CultureInfo.InvariantCulture)).ToList();
            PeralteBox.ItemsSource = options;
            if (options.Count > 0) PeralteBox.SelectedIndex = 0;
        }

        private LargueroDesign CurrentDesign()
        {
            UiSupport.TryNum(LengthBox.Text, out var length);
            double.TryParse(PeralteBox.SelectedItem as string, NumberStyles.Any, CultureInfo.InvariantCulture, out var peralte);
            var mensulaId = MensulaBox.SelectedValue as string;

            return new LargueroDesign
            {
                Name = NameBox.Text?.Trim() ?? string.Empty,
                BeamProfileId = ProfileBox.SelectedValue as string,
                Peralte = peralte,
                Length = length > 0.0 ? length : 0.0,
                MensulaOverride = string.IsNullOrWhiteSpace(mensulaId) ? null : mensulaId
            };
        }

        /// <summary>Preload the editor from a saved larguero (from the library). <paramref name="sourceProject"/> is the
        /// loaded library project; passing it lets a re-save preserve its unknown JSON metadata (I-11).</summary>
        public void LoadExisting(LargueroDesign design, RackProject sourceProject = null)
        {
            if (design == null) return;

            this.sourceProject = sourceProject;
            NameBox.Text = design.Name ?? string.Empty;
            ProfileBox.SelectedValue = design.BeamProfileId;
            RefreshPeraltes();
            var peralteStr = design.Peralte.ToString("0.###", CultureInfo.InvariantCulture);
            if (PeralteBox.Items.Contains(peralteStr)) PeralteBox.SelectedItem = peralteStr;
            LengthBox.Text = design.Length.ToString("0.###", CultureInfo.InvariantCulture);
            MensulaBox.SelectedValue = design.MensulaOverride ?? string.Empty;
            DrawPreview();
        }

        private void ViewBom_Click(object sender, RoutedEventArgs e)
        {
            var bom = LargueroBomBuilder.Build(CurrentDesign(), catalog);
            new RackBomWindow(bom) { Owner = this }.ShowDialog();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var design = CurrentDesign();
            if (string.IsNullOrWhiteSpace(design.BeamProfileId))
            {
                SetStatus("Elige un perfil de larguero primero.", true);
                return;
            }

            if (design.Length <= 0.0)
            {
                SetStatus("Longitud a corte inválida (debe ser mayor que 0).", true);
                return;
            }

            var path = UiSupport.PromptSaveToLibrary(this, design.Name, "larguero");
            if (path == null) return;

            try
            {
                new RackProjectStore().Save(RackProject.ForLarguero(design).WithSourceMetadataFrom(sourceProject), path);
                SetStatus("Larguero guardado: " + System.IO.Path.GetFileName(path), false);
            }
            catch (Exception ex)
            {
                SetStatus("No se pudo guardar: " + ex.Message, true);
            }
        }

        private void DrawPreview()
        {
            if (PreviewCanvas == null) return;
            PreviewCanvas.Children.Clear();

            var w = PreviewCanvas.ActualWidth;
            var h = PreviewCanvas.ActualHeight;
            if (w < 40 || h < 40) return;

            var design = CurrentDesign();

            const double margin = 54.0;
            const double beamH = 18.0;
            var y = h / 2.0;
            var x0 = margin;
            var x1 = Math.Max(margin + 40.0, w - margin);

            // Beam profile.
            var beam = new Rectangle { Width = x1 - x0, Height = beamH, Fill = BeamBrush, Stroke = MensulaBrush, StrokeThickness = 1.2 };
            Canvas.SetLeft(beam, x0);
            Canvas.SetTop(beam, y - beamH / 2.0);
            PreviewCanvas.Children.Add(beam);

            // A ménsula (bracket) hanging at each end.
            foreach (var mx in new[] { x0, x1 - 14.0 })
            {
                var mensula = new Rectangle { Width = 14.0, Height = 28.0, Fill = MensulaBrush };
                Canvas.SetLeft(mensula, mx);
                Canvas.SetTop(mensula, y + beamH / 2.0);
                PreviewCanvas.Children.Add(mensula);
            }

            AddLabel(design.Length > 0.0 ? design.Length.ToString("0.#", CultureInfo.InvariantCulture) + "\" a corte" : "(longitud)",
                (x0 + x1) / 2.0 - 40.0, y - beamH / 2.0 - 26.0, LabelBrush);
            if (design.Peralte > 0.0)
            {
                AddLabel("P" + design.Peralte.ToString("0.#", CultureInfo.InvariantCulture), x0, y - beamH / 2.0 - 26.0, LabelBrush);
            }

            AddLabel("2 ménsulas", (x0 + x1) / 2.0 - 34.0, y + beamH / 2.0 + 32.0, LabelBrush);
        }

        private void AddLabel(string text, double x, double y, Brush brush)
        {
            var label = new TextBlock { Text = text, Foreground = brush, FontSize = 12.0 };
            Canvas.SetLeft(label, x);
            Canvas.SetTop(label, y);
            PreviewCanvas.Children.Add(label);
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();
    }
}
