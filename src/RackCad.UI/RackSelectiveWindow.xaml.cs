using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using RackCad.Application.Catalogs;
using RackCad.Application.Headers;
using RackCad.Application.Persistence;
using RackCad.Application.RackFrames;
using RackCad.Application.Systems;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems;

namespace RackCad.UI
{
    /// <summary>
    /// Advanced editor for a selective rack (FRONTAL view). The user edits a bays × levels MATRIX where each
    /// bay has its OWN number of levels and its own "larguero a piso" flag, and each cell carries its own pallet
    /// (frente/alto), count and larguero. <see cref="SelectiveGeometryResolver"/> derives the larguero lengths,
    /// the floor-referenced level Ys and the post height (tallest bay governs); <see cref="SelectiveFrontalBuilder"/>
    /// lays out the blocks. Click a cell to edit it, then apply the values to the cell / row / column / all.
    /// </summary>
    public partial class RackSelectiveWindow : Window
    {
        private static readonly Brush PostBrush = new SolidColorBrush(Color.FromRgb(0x3D, 0xC9, 0x86));
        private static readonly Brush PostFill = new SolidColorBrush(Color.FromArgb(0x30, 0x3D, 0xC9, 0x86));
        private static readonly Brush PostHiBrush = new SolidColorBrush(Color.FromRgb(0xFF, 0xC5, 0x3D));
        private static readonly Brush PostHiFill = new SolidColorBrush(Color.FromArgb(0x55, 0xFF, 0xC5, 0x3D));
        private static readonly Brush BeamBrush = new SolidColorBrush(Color.FromRgb(0xE0, 0x8A, 0x2B));
        private static readonly Brush BeamFill = new SolidColorBrush(Color.FromArgb(0x66, 0xE0, 0x8A, 0x2B));
        private static readonly Brush PlateFill = new SolidColorBrush(Color.FromRgb(0xB7, 0xC3, 0xCF));
        private static readonly Brush FloorStroke = new SolidColorBrush(Color.FromRgb(0x6A, 0x7B, 0x8A));
        private static readonly Brush LabelStroke = new SolidColorBrush(Color.FromRgb(0x9A, 0xA7, 0xB4));

        private static readonly Brush CellStroke = new SolidColorBrush(Color.FromRgb(0xD8, 0xDE, 0xE6));
        private static readonly Brush CellText = new SolidColorBrush(Color.FromRgb(0x1F, 0x29, 0x33));
        private static readonly Brush CellSelStroke = new SolidColorBrush(Color.FromRgb(0x2F, 0x6F, 0xED));
        private static readonly Brush CellSelFill = new SolidColorBrush(Color.FromRgb(0xDB, 0xEA, 0xFE));

        private readonly RackCatalog catalog;
        private readonly SelectiveFrontalBuilder builder = new SelectiveFrontalBuilder();
        private readonly SelectiveGeometryResolver resolver = new SelectiveGeometryResolver();
        private readonly bool canInsertInAutoCad;

        /// <summary>The design matrix: <c>bays[bay][level]</c>, level 0 = ground; each bay has its own length.</summary>
        private readonly List<List<Cell>> bays = new List<List<Cell>>();

        /// <summary>Per-bay "larguero a piso" flag, parallel to <see cref="bays"/>.</summary>
        private readonly List<bool> floorBeams = new List<bool>();

        /// <summary>Per-bay manual height override (in); null = auto. Parallel to <see cref="bays"/>.</summary>
        private readonly List<double?> bayHeights = new List<double?>();

        /// <summary>Optional per-post cabecera (frame); one entry per post (N frentes → N+1 posts), null = run default.</summary>
        private readonly List<RackFrameConfiguration> postCabeceras = new List<RackFrameConfiguration>();
        private readonly List<double> postPeraltes = new List<double>(); // per-post PERALTE override; 0 = inherit the global

        private string defaultBeamId;
        private int selBay;
        private int selLevel;
        private bool loadingCell;

        /// <summary>Identity of the rack currently edited: stable id (GUID) + client name. Empty for a brand-new rack.</summary>
        private string currentId;
        private string currentName;

        /// <summary>True when the window was opened on an EXISTING rack (RACKEDITAR). The lateral view can only be
        /// inserted then — it links to that rack's frontal; inserting it on a brand-new rack would orphan it.</summary>
        private bool isEditingExisting;

        private IReadOnlyList<HeaderBlockInstance> lastInstances;
        private SelectiveRackSystem lastSystem;

        private double mapScale;
        private double mapOffsetX;
        private double mapBottomY;
        private double mapMinX;

        public bool InsertRequested { get; private set; }

        public SelectiveRackSystem SystemToInsert { get; private set; }

        /// <summary>The design that produced <see cref="SystemToInsert"/> — embedded in the drawing for round-trip editing.</summary>
        public SelectivePalletDesign DesignToInsert { get; private set; }

        /// <summary>Stable id of the inserted rack (fresh GUID for a new rack, preserved when re-editing).</summary>
        public string RackId { get; private set; }

        /// <summary>Client-facing name of the inserted rack (may be empty).</summary>
        public string RackName { get; private set; }

        /// <summary>Which view the user asked to insert ("frontal"/"lateral"/"planta"); null when only updating.</summary>
        public string InsertView { get; private set; }

        /// <summary>True when the user chose "Actualizar" (redraw existing views in place, insert nothing).</summary>
        public bool UpdateOnly { get; private set; }

        public RackSelectiveWindow()
            : this(false)
        {
        }

        public RackSelectiveWindow(bool canInsertInAutoCad)
        {
            this.canInsertInAutoCad = canInsertInAutoCad;
            InitializeComponent();
            catalog = UiSupport.LoadCatalogSafe();

            PostBox.ItemsSource = UiSupport.ToOptions(catalog?.PostProfiles);
            PostBox.SelectedValue = catalog?.Defaults?.Post;
            if (PostBox.SelectedItem == null && PostBox.Items.Count > 0) PostBox.SelectedIndex = 0;

            CellBeamBox.ItemsSource = UiSupport.ToOptions(catalog?.BeamProfiles);
            if (CellBeamBox.Items.Count > 0) CellBeamBox.SelectedIndex = 0;
            defaultBeamId = CellBeamBox.SelectedValue as string;
            CellBeamBox.SelectionChanged += (s, e) => OnBeamChanged();

            InitMatrix(2, 4);
            LoadCellEditor();
            RenderMatrix();
            RefreshPostSelect();
            UpdateInsertButtons();
            Recompute();
        }

        /// <summary>
        /// Lateral/planta are views OF an existing system: enabled only when editing one via RACKEDITAR (and
        /// inside AutoCAD). A disabled button with the reason in its tooltip beats a rejection MessageBox.
        /// </summary>
        private void UpdateInsertButtons()
        {
            // "Actualizar" (redraw existing views in place) and adding a linked lateral/planta only make sense on an
            // existing rack, so they light up only when editing via RACKEDITAR (and inside AutoCAD). A new rack starts
            // with "Insertar frontal", which creates the first block.
            var enabled = isEditingExisting && canInsertInAutoCad;
            UpdateButton.IsEnabled = enabled;
            InsertLateralButton.IsEnabled = enabled;
            InsertPlantaButton.IsEnabled = enabled;

            if (!enabled)
            {
                var reason = !canInsertInAutoCad
                    ? "Disponible solo cuando la ventana se abre desde AutoCAD."
                    : "Primero inserta la vista frontal; luego selecciónala con RACKEDITAR y actualiza o agrega vistas desde ahí.";
                UpdateButton.ToolTip = reason;
                InsertLateralButton.ToolTip = reason;
                InsertPlantaButton.ToolTip = reason;
            }
        }

        // ---- Matrix model ----

        /// <summary>One editable matrix cell (a bay's level): its pallet, count and beam.</summary>
        private sealed class Cell
        {
            public double Frente = 40.0;
            public double Alto = 60.0;
            public int PalletCount = 2;
            public string BeamId;
            public double BeamPeralte = SelectiveRackDefaults.DefaultBeamPeralte;

            /// <summary>Optional manual overrides (null = auto): larguero length and the clear below this level.</summary>
            public double? BeamLength;
            public double? Clear;

            public bool HasOverride => BeamLength.HasValue || Clear.HasValue;

            public Cell Clone() => (Cell)MemberwiseClone();

            public void CopyFrom(Cell other)
            {
                Frente = other.Frente;
                Alto = other.Alto;
                PalletCount = other.PalletCount;
                BeamId = other.BeamId;
                BeamPeralte = other.BeamPeralte;
                BeamLength = other.BeamLength;
                Clear = other.Clear;
            }
        }

        private enum Scope { Cell, Row, Column, All }

        private Cell NewCell() => new Cell { BeamId = defaultBeamId, BeamPeralte = SelectiveRackDefaults.DefaultBeamPeralte };

        private void InitMatrix(int bayCount, int levelCount)
        {
            bays.Clear();
            floorBeams.Clear();
            bayHeights.Clear();
            for (var b = 0; b < bayCount; b++)
            {
                var column = new List<Cell>();
                for (var l = 0; l < levelCount; l++) column.Add(NewCell());
                bays.Add(column);
                floorBeams.Add(false);
                bayHeights.Add(null);
            }

            selBay = 0;
            selLevel = 0;
        }

        /// <summary>Grow/shrink the number of bays, preserving existing ones; a new bay clones the last (cells + floor flag).</summary>
        private void ResizeBays(int bayCount)
        {
            while (bays.Count < bayCount)
            {
                if (bays.Count > 0)
                {
                    bays.Add(bays[bays.Count - 1].Select(c => c.Clone()).ToList());
                    floorBeams.Add(floorBeams[floorBeams.Count - 1]);
                    bayHeights.Add(bayHeights[bayHeights.Count - 1]);
                }
                else
                {
                    bays.Add(new List<Cell> { NewCell() });
                    floorBeams.Add(false);
                    bayHeights.Add(null);
                }
            }

            while (bays.Count > bayCount)
            {
                bays.RemoveAt(bays.Count - 1);
                floorBeams.RemoveAt(floorBeams.Count - 1);
                bayHeights.RemoveAt(bayHeights.Count - 1);
            }

            ClampSelection();
        }

        private void AddLevel(int bay)
        {
            var column = bays[bay];
            column.Add(column.Count > 0 ? column[column.Count - 1].Clone() : NewCell());
            RenderMatrix();
            Recompute();
        }

        private void RemoveLevel(int bay)
        {
            var column = bays[bay];
            if (column.Count <= 1)
            {
                SetStatus("Cada frente necesita al menos un nivel.", true);
                return;
            }

            column.RemoveAt(column.Count - 1);
            ClampSelection();
            LoadCellEditor();
            RenderMatrix();
            Recompute();
        }

        private void SetFloor(int bay, bool value)
        {
            if (bay < 0 || bay >= floorBeams.Count) return;
            floorBeams[bay] = value;
            Recompute();
        }

        private void ClampSelection()
        {
            selBay = Math.Min(Math.Max(0, selBay), bays.Count - 1);
            var levelCount = selBay >= 0 && selBay < bays.Count ? bays[selBay].Count : 1;
            selLevel = Math.Min(Math.Max(0, selLevel), levelCount - 1);
        }

        private bool TryGetSelected(out Cell cell)
        {
            cell = null;
            if (selBay < 0 || selBay >= bays.Count) return false;
            var column = bays[selBay];
            if (selLevel < 0 || selLevel >= column.Count) return false;
            cell = column[selLevel];
            return true;
        }

        // ---- Matrix rendering ----

        private void RenderMatrix()
        {
            MatrixGrid.Children.Clear();
            MatrixGrid.RowDefinitions.Clear();
            MatrixGrid.ColumnDefinitions.Clear();

            var bayCount = bays.Count;
            if (bayCount == 0) return;
            var maxLevels = bays.Max(c => c.Count);
            if (maxLevels == 0) return;

            MatrixGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(64) });
            for (var b = 0; b < bayCount; b++)
                MatrixGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(96) });

            MatrixGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            for (var r = 0; r < maxLevels; r++)
                MatrixGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            AddToGrid(HeaderCell(string.Empty), 0, 0);
            for (var b = 0; b < bayCount; b++)
                AddToGrid(BayHeader(b), 0, b + 1);

            // Top display row = highest level; shorter bays leave the upper rows empty (aligned to the floor).
            for (var displayRow = 0; displayRow < maxLevels; displayRow++)
            {
                var level = maxLevels - 1 - displayRow;
                var gridRow = displayRow + 1;
                AddToGrid(HeaderCell("Nivel " + (level + 1)), gridRow, 0);

                for (var b = 0; b < bayCount; b++)
                {
                    if (level < bays[b].Count)
                    {
                        AddToGrid(CellUi(bays[b][level], b, level), gridRow, b + 1);
                    }
                }
            }
        }

        private void AddToGrid(UIElement element, int row, int column)
        {
            Grid.SetRow(element, row);
            Grid.SetColumn(element, column);
            MatrixGrid.Children.Add(element);
        }

        private static TextBlock HeaderCell(string text) => new TextBlock
        {
            Text = text,
            Foreground = LabelStroke,
            FontSize = 11,
            FontWeight = FontWeights.SemiBold,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(2)
        };

        private UIElement BayHeader(int bay)
        {
            var panel = new StackPanel { Margin = new Thickness(2, 2, 2, 6) };
            panel.Children.Add(new TextBlock
            {
                Text = "Frente " + (bay + 1),
                Foreground = LabelStroke,
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                HorizontalAlignment = HorizontalAlignment.Center
            });

            var levelRow = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 2, 0, 0) };
            var minus = SmallButton("−");
            minus.Click += (s, e) => RemoveLevel(bay);
            var count = new TextBlock
            {
                Text = bays[bay].Count.ToString(CultureInfo.InvariantCulture),
                Foreground = CellText,
                FontSize = 11,
                MinWidth = 16,
                Margin = new Thickness(5, 0, 5, 0),
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            var plus = SmallButton("+");
            plus.Click += (s, e) => AddLevel(bay);
            levelRow.Children.Add(minus);
            levelRow.Children.Add(count);
            levelRow.Children.Add(plus);
            panel.Children.Add(levelRow);

            var floor = new CheckBox
            {
                Content = "Piso",
                FontSize = 10.5,
                Foreground = LabelStroke,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 3, 0, 0),
                IsChecked = floorBeams[bay],
                ToolTip = "Larguero a piso: el nivel de piso lleva larguero."
            };
            floor.Checked += (s, e) => SetFloor(bay, true);
            floor.Unchecked += (s, e) => SetFloor(bay, false);
            panel.Children.Add(floor);

            var heightRow = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 3, 0, 0) };
            heightRow.Children.Add(new TextBlock { Text = "Alto", Foreground = LabelStroke, FontSize = 10.5, Margin = new Thickness(0, 0, 4, 0), VerticalAlignment = VerticalAlignment.Center });
            var heightBox = new TextBox
            {
                Width = 44,
                FontSize = 10.5,
                Text = bayHeights[bay].HasValue ? bayHeights[bay].Value.ToString("0.###", CultureInfo.InvariantCulture) : string.Empty,
                ToolTip = "Altura del frente (in). Vacío = auto. El poste toma el frente más alto que toca."
            };
            heightBox.LostFocus += (s, e) => SetBayHeight(bay, heightBox.Text);
            // e.Handled: without it Enter ALSO fires the window's default button (double Recompute; the matrix
            // rebuild steals focus and any validation message is wiped instantly).
            heightBox.KeyDown += (s, e) => { if (e.Key == Key.Enter) { SetBayHeight(bay, heightBox.Text); e.Handled = true; } };
            heightRow.Children.Add(heightBox);
            panel.Children.Add(heightRow);

            return panel;
        }

        private void SetBayHeight(int bay, string text)
        {
            if (bay < 0 || bay >= bayHeights.Count) return;
            if (!TryOptionalNum(text, out var value)) { SetStatus("Altura de frente inválida (vacío = auto).", true); return; }
            if (Nullable.Equals(bayHeights[bay], value)) return;
            bayHeights[bay] = value;
            Recompute();
        }

        // ---- Per-post cabeceras ----

        /// <summary>Keep the per-post cabecera list sized to N+1 (posts), preserving existing entries.</summary>
        private void SyncPostCabeceras()
        {
            var posts = bays.Count + 1;
            while (postCabeceras.Count < posts) postCabeceras.Add(null);
            while (postCabeceras.Count > posts) postCabeceras.RemoveAt(postCabeceras.Count - 1);
            while (postPeraltes.Count < posts) postPeraltes.Add(0.0);
            while (postPeraltes.Count > posts) postPeraltes.RemoveAt(postPeraltes.Count - 1);
        }

        /// <summary>Fill the post selector with "Poste 1..N+1", preserving the selection, then refresh its status.</summary>
        private void RefreshPostSelect()
        {
            SyncPostCabeceras();
            var previous = PostSelectBox.SelectedIndex;
            var items = new List<string>();
            for (var i = 0; i < postCabeceras.Count; i++) items.Add("Poste " + (i + 1).ToString(CultureInfo.InvariantCulture));
            PostSelectBox.ItemsSource = items;
            PostSelectBox.SelectedIndex = previous >= 0 && previous < items.Count ? previous : (items.Count > 0 ? 0 : -1);
            UpdatePostStatus();
        }

        private void PostSelect_Changed(object sender, SelectionChangedEventArgs e)
        {
            UpdatePostStatus();
            ShowPostPeralteOverride();
            DrawPreview(); // re-highlight the picked post
        }

        /// <summary>Show the selected post's peralte override in its box (empty when the post inherits the global).</summary>
        private void ShowPostPeralteOverride()
        {
            if (PostPeralteOverrideBox == null) return;
            var i = PostSelectBox.SelectedIndex;
            var over = i >= 0 && i < postPeraltes.Count ? postPeraltes[i] : 0.0;
            PostPeralteOverrideBox.Text = over > 0.0 ? over.ToString("0.###", CultureInfo.InvariantCulture) : string.Empty;
        }

        /// <summary>Store the per-post peralte override for the selected post; empty (or = global) means inherit.</summary>
        private void PostPeralteOverride_LostFocus(object sender, RoutedEventArgs e)
        {
            var i = PostSelectBox.SelectedIndex;
            if (i < 0 || i >= postPeraltes.Count) return;

            var text = PostPeralteOverrideBox.Text;
            double value;
            if (string.IsNullOrWhiteSpace(text))
            {
                value = 0.0; // empty → inherit the global peralte
            }
            else if (!UiSupport.TryNum(text, out value) || value <= 0.0)
            {
                SetStatus("Peralte de poste invalido (deja vacio para usar el peralte global).", true);
                ShowPostPeralteOverride();
                return;
            }

            // A value equal to the global is just the default → store as inherit (0) so it tracks the global if it changes.
            if (value > 0.0 && UiSupport.TryNum(PostPeralteBox.Text, out var global) && Math.Abs(value - global) < 1e-6)
            {
                value = 0.0;
            }

            postPeraltes[i] = value;
            ShowPostPeralteOverride();
            Recompute();
        }

        private void UpdatePostStatus()
        {
            if (PostCabeceraStatus == null) return;
            var i = PostSelectBox.SelectedIndex;
            var custom = i >= 0 && i < postCabeceras.Count && postCabeceras[i] != null;
            PostCabeceraStatus.Text = i < 0 ? string.Empty : (custom ? "Personalizada" : "Por defecto (del tramo)");
        }

        private void CustomizePost_Click(object sender, RoutedEventArgs e)
        {
            var i = PostSelectBox.SelectedIndex;
            if (i < 0 || i >= postCabeceras.Count) return;

            // Seed with the RESOLVED cabecera for THIS post: height = the post's resolved height (tallest adjacent
            // frente), fondo = the tramo's fondo (shared by every cabecera). So "Personalizar" opens the cabecera that
            // is actually in use (e.g. 312 in / 48 in), not a generic 132/42. A custom one keeps its structural edits.
            var resolvedHeight = ResolvedPostHeight(i);
            var fondo = ResolvedFondo();

            // Work on a CLONE and compare before/after: closing the configurator without editing is a real
            // CANCEL (before, the seed was mutated up-front and any close marked the post "Personalizada").
            var seed = postCabeceras[i] != null ? CloneCabecera(postCabeceras[i]) : BuildStandardPostCabecera(resolvedHeight, fondo);
            if (seed == null) return;
            if (resolvedHeight > 0.0) seed.Height = resolvedHeight;
            if (fondo > 0.0) seed.Depth = fondo;

            var store = new RackProjectStore();
            var before = store.Serialize(RackProject.ForSelective(seed));

            var window = new RackFrameConfiguratorWindow(seed, canInsertInAutoCad: false) { Owner = this };
            window.ShowDialog();

            var cfg = window.Configuration;
            if (cfg == null || store.Serialize(RackProject.ForSelective(cfg)) == before)
            {
                // Nothing was edited: leave the post exactly as it was (default stays default).
                UpdatePostStatus();
                return;
            }

            // Fondo is locked to the tramo — every cabecera of the rack shares it.
            if (fondo > 0.0) cfg.Depth = fondo;

            // Height comes from the system; the user MAY override it, but warn it can desynchronize the rack
            // (the frontal largueros are placed for the resolved height).
            if (resolvedHeight > 0.0 && Math.Abs(cfg.Height - resolvedHeight) > 0.5)
            {
                MessageBox.Show(
                    this,
                    "La altura de la cabecera (" + cfg.Height.ToString("0.##", CultureInfo.InvariantCulture)
                        + " in) difiere del alto resuelto del poste (" + resolvedHeight.ToString("0.##", CultureInfo.InvariantCulture)
                        + " in).\n\nEl sistema se puede desconfigurar: el frontal coloca los largueros para el alto resuelto, "
                        + "así que el corte lateral y el frontal pueden dejar de coincidir.",
                    "Altura de cabecera",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }

            postCabeceras[i] = cfg;
            UpdatePostStatus();
            Recompute();
        }

        /// <summary>Deep-clone a cabecera via the project store round-trip (the same serialization RACKEDITAR uses).</summary>
        private static RackFrameConfiguration CloneCabecera(RackFrameConfiguration configuration)
        {
            var store = new RackProjectStore();
            return store.Deserialize(store.Serialize(RackProject.ForSelective(configuration)))?.Header;
        }

        /// <summary>The resolved height of post <paramref name="i"/> (tallest adjacent frente); falls back to the run height.</summary>
        private double ResolvedPostHeight(int i)
        {
            if (lastSystem == null) return 0.0;
            var height = SelectivePostGeometry.PostHeight(lastSystem, i);
            return height > 0.0 ? height : lastSystem.Height;
        }

        /// <summary>The tramo's fondo (shared by every cabecera): the resolved pallet depth, or 48 in as a fallback.</summary>
        private double ResolvedFondo()
        {
            var fondo = lastSystem?.PalletDepth ?? 0.0;
            return fondo > 0.0 ? fondo : SelectiveRackDefaults.DefaultPalletDepth;
        }

        /// <summary>Build a standard cabecera at the given height/fondo using the run's post; the seed when a post has no custom one.</summary>
        private RackFrameConfiguration BuildStandardPostCabecera(double height, double fondo)
        {
            var template = RackFrameTemplateCatalog.FindStandardOrDefault();
            var postId = PostBox.SelectedValue as string;
            if (string.IsNullOrWhiteSpace(postId)) postId = lastSystem?.PostId;
            return new RackFrameConfigurationFactory(catalog).Build(
                template, postId,
                height > 0.0 ? height : template.DefaultHeight,
                fondo > 0.0 ? fondo : SelectiveRackDefaults.DefaultPalletDepth);
        }

        private void ResetPost_Click(object sender, RoutedEventArgs e)
        {
            var i = PostSelectBox.SelectedIndex;
            if (i < 0 || i >= postCabeceras.Count) return;
            postCabeceras[i] = null;
            if (i < postPeraltes.Count) postPeraltes[i] = 0.0; // back to the global peralte too
            UpdatePostStatus();
            ShowPostPeralteOverride();
            Recompute();
        }

        private static Button SmallButton(string text) => new Button
        {
            Content = text,
            Width = 20,
            Height = 18,
            Padding = new Thickness(0),
            FontSize = 12,
            Cursor = Cursors.Hand
        };

        private UIElement CellUi(Cell cell, int bay, int level)
        {
            var selected = bay == selBay && level == selLevel;
            var border = new Border
            {
                Margin = new Thickness(2),
                Background = selected ? CellSelFill : Brushes.White,
                BorderBrush = selected ? CellSelStroke : CellStroke,
                BorderThickness = new Thickness(selected ? 2 : 1),
                Padding = new Thickness(6, 4, 6, 4),
                Cursor = Cursors.Hand,
                Child = new TextBlock
                {
                    Text = string.Format(CultureInfo.InvariantCulture, "{0:0.#}×{1:0.#}\n×{2} · P{3:0.#}{4}",
                        cell.Frente, cell.Alto, cell.PalletCount, cell.BeamPeralte, cell.HasOverride ? " ✎" : string.Empty),
                    FontSize = 11,
                    Foreground = CellText,
                    TextAlignment = TextAlignment.Center
                }
            };

            border.MouseLeftButtonUp += (s, e) => SelectCell(bay, level);
            return border;
        }

        private void SelectCell(int bay, int level)
        {
            selBay = bay;
            selLevel = level;
            LoadCellEditor();
            RenderMatrix();
        }

        private void LoadCellEditor()
        {
            if (!TryGetSelected(out var cell)) return;

            loadingCell = true;
            CellHeader.Text = string.Format(CultureInfo.InvariantCulture, "Celda: Frente {0} · Nivel {1}", selBay + 1, selLevel + 1);
            FrenteBox.Text = cell.Frente.ToString("0.###", CultureInfo.InvariantCulture);
            AltoBox.Text = cell.Alto.ToString("0.###", CultureInfo.InvariantCulture);
            PalletCountBox.Text = cell.PalletCount.ToString(CultureInfo.InvariantCulture);
            BeamLenBox.Text = cell.BeamLength.HasValue ? cell.BeamLength.Value.ToString("0.###", CultureInfo.InvariantCulture) : string.Empty;
            ClearBox.Text = cell.Clear.HasValue ? cell.Clear.Value.ToString("0.###", CultureInfo.InvariantCulture) : string.Empty;
            CellBeamBox.SelectedValue = cell.BeamId;
            RefreshPeralteCombo(cell.BeamPeralte);
            loadingCell = false;
        }

        /// <summary>The beam changed in the cell editor: repopulate the allowed peraltes, keeping the current one if it still fits.</summary>
        private void OnBeamChanged()
        {
            if (loadingCell) return;
            var current = BeamPeralteCombo.SelectedItem as string;
            RefreshPeralteCombo(UiSupport.TryNum(current, out var v) ? v : (double?)null);
        }

        /// <summary>Fill the peralte combo with the selected larguero's allowed values; select <paramref name="keep"/> if present, else the first.</summary>
        private void RefreshPeralteCombo(double? keep)
        {
            var options = PeralteOptions(CellBeamBox.SelectedValue as string);
            BeamPeralteCombo.ItemsSource = options;

            var target = keep.HasValue ? keep.Value.ToString("0.###", CultureInfo.InvariantCulture) : null;
            if (target != null && options.Contains(target)) BeamPeralteCombo.SelectedItem = target;
            else if (options.Count > 0) BeamPeralteCombo.SelectedIndex = 0;
        }

        /// <summary>Allowed PERALTE values declared by a larguero (parsed by <see cref="PeralteList"/>), formatted for display.</summary>
        private List<string> PeralteOptions(string beamId)
        {
            var raw = catalog?.BeamProfiles.FirstOrDefault(b => string.Equals(b?.Id, beamId, StringComparison.OrdinalIgnoreCase))?.Peraltes;
            return PeralteList.Parse(raw)
                .Select(value => value.ToString("0.###", CultureInfo.InvariantCulture))
                .ToList();
        }

        // ---- Events ----

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            if (!TryInt(BayCountBox.Text, out var bayCount) || bayCount < 1)
            {
                SetStatus("Cantidad de frentes inválida.", true);
                return;
            }

            ResizeBays(bayCount);
            LoadCellEditor();
            RenderMatrix();
            RefreshPostSelect();
            Recompute();
        }

        private void ApplyCell_Click(object sender, RoutedEventArgs e) => ApplyScope(Scope.Cell);
        private void ApplyRow_Click(object sender, RoutedEventArgs e) => ApplyScope(Scope.Row);
        private void ApplyColumn_Click(object sender, RoutedEventArgs e) => ApplyScope(Scope.Column);
        private void ApplyAll_Click(object sender, RoutedEventArgs e) => ApplyScope(Scope.All);
        private void Close_Click(object sender, RoutedEventArgs e) => Close();

        private void ShowBom_Click(object sender, RoutedEventArgs e)
        {
            if (lastInstances == null || lastSystem == null)
            {
                SetStatus("Genera primero la geometría (revisa tarima/niveles).", true);
                return;
            }

            var bom = SelectiveBomBuilder.Build(lastInstances, catalog);
            new RackBomWindow(bom) { Owner = this }.ShowDialog();
        }

        private void ApplyScope(Scope scope)
        {
            if (!ReadCellEditor(out var values, out var error))
            {
                SetStatus(error, true);
                return;
            }

            for (var b = 0; b < bays.Count; b++)
            {
                for (var l = 0; l < bays[b].Count; l++)
                {
                    var inScope =
                        scope == Scope.All ||
                        (scope == Scope.Cell && b == selBay && l == selLevel) ||
                        (scope == Scope.Row && l == selLevel) ||
                        (scope == Scope.Column && b == selBay);

                    if (inScope) bays[b][l].CopyFrom(values);
                }
            }

            RenderMatrix();
            Recompute();
        }

        private void InsertFrontal_Click(object sender, RoutedEventArgs e) => RequestDraw(RackEmbedDocument.ViewFrontal, updateOnly: false);

        private void InsertLateral_Click(object sender, RoutedEventArgs e) => RequestDraw(RackEmbedDocument.ViewLateral, updateOnly: false);

        private void InsertPlanta_Click(object sender, RoutedEventArgs e) => RequestDraw(RackEmbedDocument.ViewPlanta, updateOnly: false);

        /// <summary>"Actualizar": redraw the rack's already-drawn views in place with the current edits, inserting nothing.</summary>
        private void UpdateExisting_Click(object sender, RoutedEventArgs e) => RequestDraw(view: null, updateOnly: true);

        /// <summary>
        /// Close the window asking AutoCAD to draw. <paramref name="updateOnly"/> = redraw existing views only (Actualizar);
        /// otherwise insert a new linked view-block of <paramref name="view"/> AND refresh the existing ones.
        /// </summary>
        private void RequestDraw(string view, bool updateOnly)
        {
            if (!canInsertInAutoCad)
            {
                SetStatus("El dibujo en AutoCAD solo está disponible cuando el selectivo se abre desde AutoCAD.", true);
                return;
            }

            // Updating, and adding a linked lateral/planta, only make sense on an existing system (a new rack has no GUID
            // to link to yet): insert the frontal first, then add the rest via RACKEDITAR.
            if (!isEditingExisting && (updateOnly || view == RackEmbedDocument.ViewLateral || view == RackEmbedDocument.ViewPlanta))
            {
                MessageBox.Show(
                    this,
                    "Primero inserta la vista frontal. Luego selecciónala con RACKEDITAR y desde ahí actualiza o agrega "
                        + "las demás vistas: así quedan ligadas al sistema (mismo GUID).",
                    updateOnly ? "Actualizar" : "Vista " + view,
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            var system = BuildSystem(out var design, out var error);
            if (system == null)
            {
                SetStatus(error, true);
                return;
            }

            currentName = NameBox.Text?.Trim();
            if (string.IsNullOrWhiteSpace(currentId)) currentId = Guid.NewGuid().ToString();

            InsertRequested = true;
            UpdateOnly = updateOnly;
            InsertView = updateOnly ? null : view;
            SystemToInsert = system;
            DesignToInsert = design;
            RackId = currentId;
            RackName = currentName;
            Close();
        }

        private void Recompute()
        {
            var system = BuildSystem(out var error);
            if (system == null)
            {
                lastSystem = null;
                lastInstances = null;
                SummaryText.Text = string.Empty;
                PreviewCanvas.Children.Clear();
                SetStatus(error, true);
                return;
            }

            lastSystem = system;
            lastInstances = builder.Build(system, catalog);
            UpdateSummary();
            SetStatus("Vista actualizada.", false);
            DrawPreview();
        }

        // ---- Reading inputs ----

        private bool ReadCellEditor(out Cell values, out string error)
        {
            values = null;
            error = null;
            if (!(CellBeamBox.SelectedValue is string beamId) || string.IsNullOrWhiteSpace(beamId)) { error = "Selecciona un larguero."; return false; }
            if (!UiSupport.TryNum(FrenteBox.Text, out var frente) || frente <= 0.0) { error = "Frente de tarima invalido."; return false; }
            if (!UiSupport.TryNum(AltoBox.Text, out var alto) || alto <= 0.0) { error = "Alto de tarima invalido."; return false; }
            if (!TryInt(PalletCountBox.Text, out var count) || count < 1) { error = "Tarimas por nivel invalido."; return false; }
            if (!(BeamPeralteCombo.SelectedItem is string peralteText) || !UiSupport.TryNum(peralteText, out var peralte) || peralte <= 0.0) { error = "Selecciona un peralte de larguero."; return false; }
            if (!TryOptionalNum(BeamLenBox.Text, out var beamLen)) { error = "Longitud de larguero invalida (deja vacio para auto)."; return false; }
            if (!TryOptionalNum(ClearBox.Text, out var clear)) { error = "Claro invalido (deja vacio para auto)."; return false; }

            values = new Cell { Frente = frente, Alto = alto, PalletCount = count, BeamId = beamId, BeamPeralte = peralte, BeamLength = beamLen, Clear = clear };
            return true;
        }

        /// <summary>Parse an optional positive number: empty/whitespace → null (auto); a valid &gt; 0 value → that; anything else → invalid.</summary>
        private static bool TryOptionalNum(string text, out double? value)
        {
            value = null;
            if (string.IsNullOrWhiteSpace(text)) return true;
            if (UiSupport.TryNum(text, out var v) && v > 0.0) { value = v; return true; }
            return false;
        }

        /// <summary>Builds the pallet-driven design from the current editor state (globals + matrix), or null + error.</summary>
        private SelectivePalletDesign BuildDesign(out string error)
        {
            error = null;
            if (!(PostBox.SelectedValue is string postId) || string.IsNullOrWhiteSpace(postId)) { error = "Selecciona un poste."; return null; }
            if (!UiSupport.TryNum(PostPeralteBox.Text, out var postPeralte) || postPeralte <= 0.0) { error = "Peralte de poste invalido."; return null; }
            if (!UiSupport.TryNum(ToleranceBox.Text, out var tolerance) || tolerance < 0.0) { error = "Tolerancia horizontal invalida."; return null; }
            if (!UiSupport.TryNum(ClearanceBox.Text, out var clearance) || clearance < 0.0) { error = "Holgura vertical invalida."; return null; }
            if (!UiSupport.TryNum(FloorRiseBox.Text, out var floorRise) || floorRise < 0.0) { error = "Elevacion de larguero a piso invalida."; return null; }
            if (!UiSupport.TryNum(FondoBox.Text, out var fondo) || fondo <= 0.0) { error = "Fondo de tarima invalido."; return null; }
            if (bays.Count == 0 || bays[0].Count == 0) { error = "Define frentes y niveles."; return null; }

            var design = new SelectivePalletDesign
            {
                PostId = postId,
                PostPeralte = postPeralte,
                PalletTolerance = tolerance,
                VerticalClearance = clearance,
                FloorBeamRise = floorRise,
                PalletDepth = fondo
            };

            for (var b = 0; b < bays.Count; b++)
            {
                var bay = new SelectiveBayDesign { FloorBeam = floorBeams[b], HeightOverride = bayHeights[b] };
                foreach (var cell in bays[b])
                {
                    bay.Levels.Add(new SelectiveCell
                    {
                        Pallet = new Tarima { Frente = cell.Frente, Alto = cell.Alto },
                        PalletCount = cell.PalletCount,
                        BeamId = cell.BeamId,
                        BeamPeralte = cell.BeamPeralte,
                        BeamLengthOverride = cell.BeamLength,
                        ClearOverride = cell.Clear
                    });
                }

                design.Bays.Add(bay);
            }

            SyncPostCabeceras();
            foreach (var cabecera in postCabeceras)
            {
                design.PostCabeceras.Add(cabecera);
            }

            foreach (var peralte in postPeraltes)
            {
                design.PostPeraltes.Add(peralte);
            }

            return design;
        }

        private SelectiveRackSystem BuildSystem(out string error) => BuildSystem(out _, out error);

        /// <summary>Design + resolved system in one pass — RequestInsert and Recompute share this (no duplicated resolve/validation).</summary>
        private SelectiveRackSystem BuildSystem(out SelectivePalletDesign design, out string error)
        {
            design = BuildDesign(out error);
            if (design == null) return null;

            var system = resolver.Resolve(design, catalog);
            if (system.Height <= 0.0)
            {
                error = "No se pudo derivar la geometría (revisa tarima/niveles).";
                design = null;
                return null;
            }

            return system;
        }

        /// <summary>Restore the whole editor (globals + matrix) from a saved design, then recompute.</summary>
        private void LoadDesign(SelectivePalletDesign design)
        {
            if (design == null || design.Bays.Count == 0) return;

            PostBox.SelectedValue = design.PostId;
            if (PostBox.SelectedItem == null && PostBox.Items.Count > 0) PostBox.SelectedIndex = 0;
            PostPeralteBox.Text = design.PostPeralte.ToString("0.###", CultureInfo.InvariantCulture);
            ToleranceBox.Text = design.PalletTolerance.ToString("0.###", CultureInfo.InvariantCulture);
            ClearanceBox.Text = design.VerticalClearance.ToString("0.###", CultureInfo.InvariantCulture);
            FloorRiseBox.Text = design.FloorBeamRise.ToString("0.###", CultureInfo.InvariantCulture);
            FondoBox.Text = (design.PalletDepth > 0.0 ? design.PalletDepth : SelectiveRackDefaults.DefaultPalletDepth).ToString("0.###", CultureInfo.InvariantCulture);

            bays.Clear();
            floorBeams.Clear();
            bayHeights.Clear();
            foreach (var bayDesign in design.Bays)
            {
                var column = new List<Cell>();
                foreach (var cell in bayDesign.Levels)
                {
                    column.Add(new Cell
                    {
                        Frente = cell.Pallet?.Frente ?? 40.0,
                        Alto = cell.Pallet?.Alto ?? 60.0,
                        PalletCount = cell.PalletCount,
                        BeamId = cell.BeamId ?? defaultBeamId,
                        BeamPeralte = cell.BeamPeralte,
                        BeamLength = cell.BeamLengthOverride,
                        Clear = cell.ClearOverride
                    });
                }

                if (column.Count == 0) column.Add(NewCell());
                bays.Add(column);
                floorBeams.Add(bayDesign.FloorBeam);
                bayHeights.Add(bayDesign.HeightOverride);
            }

            postCabeceras.Clear();
            var loadedFondo = design.PalletDepth > 0.0 ? design.PalletDepth : SelectiveRackDefaults.DefaultPalletDepth;
            foreach (var cabecera in design.PostCabeceras)
            {
                // Every cabecera of the rack shares the tramo's fondo — coerce it on load so a legacy/round-tripped
                // design can't carry a stale depth.
                if (cabecera != null && loadedFondo > 0.0) cabecera.Depth = loadedFondo;
                postCabeceras.Add(cabecera);
            }

            postPeraltes.Clear();
            foreach (var peralte in design.PostPeraltes)
            {
                postPeraltes.Add(peralte);
            }

            BayCountBox.Text = bays.Count.ToString(CultureInfo.InvariantCulture);
            selBay = 0;
            selLevel = 0;
            ClampSelection();
            LoadCellEditor();
            RenderMatrix();
            RefreshPostSelect();
            Recompute();
        }

        /// <summary>Open the editor pre-loaded with an existing rack (from an embedded/saved document), keeping its Id/Name.</summary>
        public void LoadExisting(SelectivePalletDesignDocument document)
        {
            if (document == null) return;
            currentId = document.Id;
            currentName = document.Name;
            isEditingExisting = true; // opened on an existing rack → "Actualizar" + linked lateral/planta become available
            UpdateInsertButtons();
            NameBox.Text = document.Name ?? string.Empty;
            LoadDesign(document.ToDomain());
        }

        private void UpdateSummary()
        {
            var posts = lastInstances.Count(i => i.Role == HeaderBlockRole.Post);
            var beams = lastInstances.Count(i => i.Role == HeaderBlockRole.Beam);

            var bay0 = lastSystem.Bays.Count > 0 ? lastSystem.Bays[0] : null;
            var beamLength = bay0?.BeamLength ?? 0.0;
            var separation = bay0 != null && bay0.Levels.Count > 1 ? bay0.Levels[1].Y - bay0.Levels[0].Y : 0.0;

            SummaryText.Text = string.Format(
                CultureInfo.InvariantCulture,
                "{0} frentes · {1} cabeceras · {2} largueros\nDerivado (frente 1): larguero {3:0.##}\" · sep. {4:0.##}\" · altura {5:0.##}\"",
                lastSystem.Bays.Count, posts, beams, beamLength, separation, lastSystem.Height);
        }

        // ---- Preview ----

        private void PreviewCanvas_SizeChanged(object sender, SizeChangedEventArgs e) => DrawPreview();

        private void DrawPreview()
        {
            PreviewCanvas.Children.Clear();

            if (lastInstances == null || lastSystem == null || lastSystem.Height <= 0.0)
            {
                return;
            }

            var postWidth = ProfileWidth(lastSystem.PostId);
            var height = lastSystem.Height;

            var xMin = -postWidth / 2.0;
            var xMax = xMin;
            foreach (var instance in lastInstances)
            {
                if (instance.Role == HeaderBlockRole.Post)
                {
                    xMax = Math.Max(xMax, instance.Insertion.X + postWidth / 2.0);
                }
                else if (instance.Role == HeaderBlockRole.Beam)
                {
                    var length = Param(instance, "LONGITUD");
                    xMax = Math.Max(xMax, instance.Insertion.X + length);
                }
            }

            var totalWidth = Math.Max(1.0, xMax - xMin);
            var availableWidth = PreviewCanvas.ActualWidth;
            var availableHeight = PreviewCanvas.ActualHeight;
            if (availableWidth < 20 || availableHeight < 20)
            {
                return;
            }

            const double horizontalMargin = 46.0;
            const double topMargin = 26.0;
            const double bottomMargin = 40.0;
            var usableWidth = Math.Max(1.0, availableWidth - 2 * horizontalMargin);
            var usableHeight = Math.Max(1.0, availableHeight - topMargin - bottomMargin);
            mapScale = Math.Min(usableWidth / totalWidth, usableHeight / height);
            if (mapScale <= 0.0)
            {
                return;
            }

            mapMinX = xMin;
            var drawWidth = totalWidth * mapScale;
            var drawHeight = height * mapScale;
            mapOffsetX = (availableWidth - drawWidth) / 2.0;
            mapBottomY = topMargin + (usableHeight - drawHeight) / 2.0 + drawHeight;

            AddCanvasLabel(mapOffsetX, Math.Max(4.0, mapBottomY - drawHeight - 22.0),
                "Ancho " + totalWidth.ToString("0.##", CultureInfo.InvariantCulture) + " in  ·  altura " + height.ToString("0.##", CultureInfo.InvariantCulture) + " in",
                LabelStroke, 12, 320.0);

            // Floor.
            AddLine(Map(xMin, 0), Map(xMax, 0), FloorStroke, 1.5);

            var selectedPost = PostSelectBox?.SelectedIndex ?? -1;
            var postIndex = 0;

            foreach (var instance in lastInstances)
            {
                switch (instance.Role)
                {
                    case HeaderBlockRole.Post:
                        var postH = Param(instance, "LONGITUD");
                        if (postH <= 0.0) postH = height;
                        var pTop = Map(instance.Insertion.X - postWidth / 2.0, postH);
                        var hi = postIndex == selectedPost;
                        AddRectangle(pTop.X, pTop.Y, postWidth * mapScale, postH * mapScale,
                            hi ? PostHiBrush : PostBrush, hi ? 3.0 : 1.6, hi ? PostHiFill : PostFill);
                        // Post number under the base (1-based) — matches "Cabecera por poste" and the "insertar lateral" prompt.
                        var numAt = Map(instance.Insertion.X, 0.0);
                        AddPostNumber(numAt.X, mapBottomY + 8.0, (postIndex + 1).ToString(CultureInfo.InvariantCulture), hi);
                        postIndex++;
                        break;
                    case HeaderBlockRole.Beam:
                        var length = Param(instance, "LONGITUD");
                        var peralte = Param(instance, "PERALTE");
                        var bTop = Map(instance.Insertion.X, instance.Insertion.Y + peralte / 2.0);
                        AddRectangle(bTop.X, bTop.Y, length * mapScale, Math.Max(2.0, peralte * mapScale), BeamBrush, 1.2, BeamFill);
                        break;
                    case HeaderBlockRole.BasePlate:
                        var plate = Map(instance.ConnectionAnchor.X - postWidth * 0.7, 0);
                        AddRectangle(plate.X, plate.Y, postWidth * 1.4 * mapScale, Math.Max(3.0, 0.3 * mapScale + 4.0), PlateFill, 1.0, PlateFill);
                        break;
                }
            }
        }

        private double ProfileWidth(string postId)
        {
            var width = catalog?.PostProfiles.FirstOrDefault(p => string.Equals(p?.Id, postId, StringComparison.OrdinalIgnoreCase))?.Width ?? 0.0;
            return width > 0.0 ? width : 3.0;
        }

        private static double Param(HeaderBlockInstance instance, string name)
            => instance.DynamicParameters.TryGetValue(name, out var value) ? value : 0.0;

        private Point Map(double x, double y) => new Point(mapOffsetX + (x - mapMinX) * mapScale, mapBottomY - y * mapScale);

        private PreviewCanvasPainter previewPainter;
        private PreviewCanvasPainter Painter => previewPainter ??= new PreviewCanvasPainter(PreviewCanvas);

        private void AddLine(Point a, Point b, Brush stroke, double thickness)
            => Painter.AddLine(a, b, stroke, thickness);

        private void AddRectangle(double left, double top, double width, double height, Brush stroke, double thickness, Brush fill)
            => Painter.AddRectangle(left, top, width, height, stroke, thickness, dash: null, fill: fill);

        /// <summary>A post's 1-based number, centered under its base; the selected post is highlighted.</summary>
        private void AddPostNumber(double centerX, double top, string text, bool highlighted)
        {
            var label = new TextBlock
            {
                Text = text,
                Foreground = highlighted ? PostHiBrush : LabelStroke,
                FontSize = 12.5,
                FontWeight = FontWeights.Bold,
                Width = 24.0,
                TextAlignment = TextAlignment.Center
            };
            Canvas.SetLeft(label, centerX - 12.0);
            Canvas.SetTop(label, top);
            PreviewCanvas.Children.Add(label);
        }

        private void AddCanvasLabel(double left, double top, string text, Brush brush, double size, double maxWidth)
        {
            var label = new TextBlock
            {
                Text = text, Foreground = brush, FontSize = size,
                FontWeight = FontWeights.SemiBold, TextTrimming = TextTrimming.CharacterEllipsis
            };
            if (maxWidth > 0.0) label.Width = maxWidth;
            Canvas.SetLeft(label, left);
            Canvas.SetTop(label, top);
            PreviewCanvas.Children.Add(label);
        }

        private void SetStatus(string message, bool isError)
        {
            // Shared status palette across the rack windows: red #B00020 error / green #2F855A ok.
            StatusText.Text = message ?? string.Empty;
            StatusText.Foreground = isError
                ? new SolidColorBrush(Color.FromRgb(0xB0, 0x00, 0x20))
                : new SolidColorBrush(Color.FromRgb(0x2F, 0x85, 0x5A));
        }

        private static bool TryInt(string text, out int value)
            => int.TryParse((text ?? string.Empty).Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
    }
}
