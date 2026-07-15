using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;

namespace RackCad.UI
{
    /// <summary>
    /// Selection of SAFETY accessories for a selective rack. Known families have specialized editors and placement
    /// rules; an unknown/future catalog family remains a manual BOM quantity. On OK, <see cref="Result"/> holds an
    /// isolated working copy. Built in code (no XAML), like the tramos dialog.
    /// </summary>
    public sealed class SelectiveSafetyWindow : Window
    {
        private static readonly string[] SideLabels = { "Ninguno", "Izquierda", "Derecha", "Ambas" };

        private readonly List<Row> rows = new List<Row>();
        private readonly TextBlock error;
        private readonly int postCount;
        private readonly int fondoCount;
        private readonly IReadOnlyList<int> levelsPerFrente;
        private readonly IReadOnlyList<SelectiveParrillaPlan.Cell> parrillaPlan; // resolved load rows; null = counts unavailable

        private sealed class Row
        {
            public string Id;
            public string Label;
            public ComboBox Variant;                 // mutually-exclusive ElementId within a catalog family
            public bool IsBota;                      // BOTA: general side on every frente
            public bool IsLateral;                   // LATERAL: per-post only (defaults to the orillas), replaces botas
            public bool IsTope;                      // TOPE: rear pallet stop at the central fondo (grid config)
            public bool IsParrilla;                  // PARRILLA: deck per (frente,level) grid, drawn frontal/lateral
            public ComboBox Side;                    // BOTA: default side
            public Button PerPost;                   // BOTA/LATERAL: "Por poste…"
            public List<SafetyPostSide> PostSides;   // BOTA/LATERAL: per-post overrides (working copy)
            public TextBox Quantity;                 // non-drawable

            // ---- TOPE working config (edited via the grid dialog) ----
            public bool TopeConfigured;
            public bool TopeShared = true;
            public SafetySide TopeSide = SafetySide.Both;
            public double TopeSaque = SelectiveSafetyDefaults.TopeSaque;
            public bool TopeFrontal;
            public int TopeFondo = -1;
            public List<SelectiveGridCell> TopeOffCells = new List<SelectiveGridCell>();
            public Button TopeButton;

            // ---- PARRILLA working config (edited via its grid dialog) ----
            public bool ParrillaConfigured;
            public bool ParrillaFrontal = true;
            public bool ParrillaLateral = true;
            public double ParrillaFrente; // 0 = one deck per tarima at the tarima's own frente
            public int ParrillaCantidad; // 0 = as many as fit
            public List<SelectiveGridCell> ParrillaOffCells = new List<SelectiveGridCell>();
            public Button ParrillaButton;
        }

        public IReadOnlyList<SelectiveSafetySelection> Result { get; private set; } = new List<SelectiveSafetySelection>();

        public SelectiveSafetyWindow(IReadOnlyList<SafetyElementCatalogEntry> elements, IEnumerable<SelectiveSafetySelection> current, int postCount, IReadOnlyList<int> levelsPerFrente = null, int fondoCount = 1, IReadOnlyList<SelectiveParrillaPlan.Cell> parrillaPlan = null)
        {
            this.postCount = Math.Max(1, postCount);
            this.fondoCount = Math.Max(1, fondoCount);
            this.levelsPerFrente = levelsPerFrente ?? new List<int>();
            this.parrillaPlan = parrillaPlan;
            elements ??= new List<SafetyElementCatalogEntry>();
            var currentSelections = (current ?? Enumerable.Empty<SelectiveSafetySelection>())
                .Where(s => s != null && !string.IsNullOrWhiteSpace(s.ElementId))
                .ToList();
            var currentById = new Dictionary<string, SelectiveSafetySelection>(StringComparer.OrdinalIgnoreCase);
            foreach (var selection in currentSelections)
            {
                currentById[selection.ElementId] = selection;
            }

            Title = "Elementos de seguridad";
            Width = 480;
            Height = 540;
            MinWidth = 400;
            MinHeight = 300;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            FontFamily = new FontFamily("Segoe UI");
            Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("/RackCad.UI;component/Themes/AppStyles.xaml", UriKind.Relative) });
            if (TryFindResource("WindowBackgroundBrush") is Brush background) Background = background;

            var root = new DockPanel { Margin = new Thickness(14) };

            var intro = new TextBlock
            {
                Text = "La bota se aplica a los postes del sistema según el lado general; usa \"Por poste…\" para apagarla o "
                     + "cambiar el lado en posiciones concretas. El protector lateral se configura por poste y reemplaza las "
                     + "botas de ese frente. Topes y parrillas se eligen por nivel de carga. El BOM se calcula del dibujo.",
                TextWrapping = TextWrapping.Wrap, FontSize = 11.5, Margin = new Thickness(0, 0, 0, 10)
            };
            DockPanel.SetDock(intro, Dock.Top);
            root.Children.Add(intro);

            var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 12, 0, 0) };
            var ok = new Button { Style = TryFindResource("PrimaryButtonStyle") as Style, Content = "Aceptar", Padding = new Thickness(16, 3, 16, 3), IsDefault = true, Margin = new Thickness(0, 0, 8, 0) };
            ok.Click += (s, e) => OnOk();
            var cancel = new Button { Style = TryFindResource("SecondaryButtonStyle") as Style, Content = "Cancelar", Padding = new Thickness(10, 3, 10, 3), IsCancel = true };
            buttons.Children.Add(ok);
            buttons.Children.Add(cancel);
            DockPanel.SetDock(buttons, Dock.Bottom);
            root.Children.Add(buttons);

            error = new TextBlock { FontSize = 11, Foreground = Brushes.Firebrick, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 8, 0, 0) };
            DockPanel.SetDock(error, Dock.Bottom);
            root.Children.Add(error);

            var list = new StackPanel();
            if (elements.Count == 0)
            {
                list.Children.Add(new TextBlock { Text = "No hay elementos de seguridad en el catálogo (seguridad.csv).", Foreground = Brushes.Gray, TextWrapping = TextWrapping.Wrap });
            }
            else
            {
                string lastType = null;
                var emittedExclusiveTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var element in elements)
                {
                    if (string.IsNullOrWhiteSpace(element?.Id)) continue;

                    var exclusiveFamily = SelectiveSafetyFamilies.IsExclusive(element.Type);
                    var normalizedType = (element.Type ?? string.Empty).Trim();
                    if (exclusiveFamily && !emittedExclusiveTypes.Add(normalizedType))
                    {
                        continue; // the first row owns the whole family's variant combo
                    }

                    if (!string.Equals(element.Type, lastType, StringComparison.OrdinalIgnoreCase))
                    {
                        lastType = element.Type;
                        list.Children.Add(new TextBlock { Text = GroupTitle(element.Type), FontWeight = FontWeights.SemiBold, Foreground = Brushes.Gray, FontSize = 11, Margin = new Thickness(0, 8, 0, 2) });
                    }

                    var variants = exclusiveFamily
                        ? SelectiveSafetyFamilies.VariantsOfType(elements, element.Type)
                        : new List<SafetyElementCatalogEntry> { element };
                    SelectiveSafetySelection existing;
                    if (exclusiveFamily)
                    {
                        existing = SelectiveSafetyFamilies.SelectedOfType(currentSelections, elements, element.Type);
                    }
                    else
                    {
                        currentById.TryGetValue(element.Id, out existing);
                    }

                    var rowElement = existing == null
                        ? element
                        : variants.FirstOrDefault(v => string.Equals(v.Id, existing.ElementId, StringComparison.OrdinalIgnoreCase)) ?? element;
                    var isBota = SelectiveSafetyDefaults.IsType(element.Type, SelectiveSafetyDefaults.BotaType);
                    var isLateral = SelectiveSafetyDefaults.IsType(element.Type, SelectiveSafetyDefaults.LateralType);
                    var isTope = SelectiveSafetyDefaults.IsType(element.Type, SelectiveSafetyDefaults.TopeType);
                    var isParrilla = SelectiveSafetyDefaults.IsType(element.Type, SelectiveSafetyDefaults.ParrillaType);
                    var row = new Row { Id = rowElement.Id, Label = rowElement.Label, IsBota = isBota, IsLateral = isLateral, IsTope = isTope, IsParrilla = isParrilla };

                    var grid = new Grid { Margin = new Thickness(0, 2, 0, 2) };
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110) });
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                    if (exclusiveFamily)
                    {
                        var variant = new ComboBox
                        {
                            VerticalAlignment = VerticalAlignment.Center,
                            ToolTip = "Elige una sola variante de esta familia (Ninguno = no incluir)."
                        };
                        variant.Items.Add(new CatalogOption(null, "Ninguno"));
                        foreach (var option in variants)
                        {
                            variant.Items.Add(new CatalogOption(option.Id, option.Label));
                        }

                        variant.SelectedIndex = 0;
                        if (existing != null)
                        {
                            for (var index = 1; index < variant.Items.Count; index++)
                            {
                                if (variant.Items[index] is CatalogOption option
                                    && string.Equals(option.Id, existing.ElementId, StringComparison.OrdinalIgnoreCase))
                                {
                                    variant.SelectedIndex = index;
                                    break;
                                }
                            }
                        }

                        Grid.SetColumn(variant, 0);
                        grid.Children.Add(variant);
                        row.Variant = variant;
                    }
                    else
                    {
                        var label = new TextBlock { Text = element.Label, VerticalAlignment = VerticalAlignment.Center, TextWrapping = TextWrapping.Wrap };
                        Grid.SetColumn(label, 0);
                        grid.Children.Add(label);
                    }

                    if (isTope)
                    {
                        if (existing != null)
                        {
                            row.TopeConfigured = true;
                            row.TopeShared = existing.TopeShared;
                            row.TopeSide = existing.Side == SafetySide.None ? SafetySide.Both : existing.Side;
                            row.TopeSaque = existing.TopeSaque > 0.0 ? existing.TopeSaque : SelectiveSafetyDefaults.TopeSaque;
                            row.TopeFrontal = existing.TopeFrontal;
                            row.TopeFondo = existing.TopeFondo;
                            row.TopeOffCells = existing.TopeOffCells?.Where(c => c != null).Select(c => new SelectiveGridCell { Frente = c.Frente, Level = c.Level }).ToList() ?? new List<SelectiveGridCell>();
                        }

                        var button = new Button
                        {
                            Style = TryFindResource("SecondaryButtonStyle") as Style,
                            Content = TopeLabel(row),
                            Padding = new Thickness(10, 3, 10, 3),
                            VerticalAlignment = VerticalAlignment.Center,
                            ToolTip = "Tope trasero: elige por frente y nivel, compartido o por fondo, y el saque. Se dibuja en lateral y planta."
                        };
                        button.Click += (s, e) => EditTope(row);
                        Grid.SetColumn(button, 1);
                        grid.Children.Add(button);
                        row.TopeButton = button;
                    }
                    else if (isParrilla)
                    {
                        if (existing != null)
                        {
                            row.ParrillaConfigured = true;
                            row.ParrillaFrontal = existing.ParrillaFrontal;
                            row.ParrillaLateral = existing.ParrillaLateral;
                            row.ParrillaFrente = existing.ParrillaFrente;
                            row.ParrillaCantidad = existing.ParrillaCantidad;
                            row.ParrillaOffCells = existing.ParrillaOffCells?.Where(c => c != null).Select(c => new SelectiveGridCell { Frente = c.Frente, Level = c.Level }).ToList() ?? new List<SelectiveGridCell>();
                        }

                        var button = new Button
                        {
                            Style = TryFindResource("SecondaryButtonStyle") as Style,
                            Content = ParrillaLabel(row),
                            Padding = new Thickness(10, 3, 10, 3),
                            VerticalAlignment = VerticalAlignment.Center,
                            ToolTip = "Parrilla / deck: elige por frente y nivel dónde va, y en qué vistas (frontal / lateral). Se apoya sobre los largueros; el BOM cuenta las seleccionadas."
                        };
                        button.Click += (s, e) => EditParrilla(row);
                        Grid.SetColumn(button, 1);
                        grid.Children.Add(button);
                        row.ParrillaButton = button;
                    }
                    else if (isBota || isLateral)
                    {
                        row.PostSides = existing?.PostSides?.Where(p => p != null).Select(p => new SafetyPostSide { PostIndex = p.PostIndex, Side = p.Side }).ToList()
                                        ?? new List<SafetyPostSide>();

                        if (isBota)
                        {
                            var combo = new ComboBox { VerticalAlignment = VerticalAlignment.Center, ToolTip = "Lado general de la bota (Ninguno = no lleva). Se puede afinar por poste." };
                            foreach (var side in SideLabels) combo.Items.Add(side);
                            combo.SelectedIndex = existing != null ? (int)existing.Side : (int)SafetySide.None;
                            Grid.SetColumn(combo, 1);
                            grid.Children.Add(combo);
                            row.Side = combo;
                        }

                        var perPost = new Button
                        {
                            Style = TryFindResource("SecondaryButtonStyle") as Style,
                            Content = PerPostLabel(row.PostSides.Count),
                            Padding = new Thickness(8, 2, 8, 2),
                            Margin = new Thickness(6, 0, 0, 0),
                            VerticalAlignment = VerticalAlignment.Center,
                            ToolTip = isLateral
                                ? "Elige en qué postes va el protector lateral y de qué lado queda la GUÍA de canal (Izquierda/Derecha = guía a un lado o al otro; Ambos = guía en los dos lados, para un frente-puente). Por defecto: primer y último frente, con la guía en lados opuestos. Reemplaza las botas de ese frente."
                                : "Elige por poste cuáles llevan bota y en qué lado (los no personalizados usan el lado general)."
                        };
                        perPost.Click += (s, e) => EditPerPost(row);
                        Grid.SetColumn(perPost, isBota ? 2 : 1);
                        grid.Children.Add(perPost);
                        row.PerPost = perPost;
                    }
                    else
                    {
                        var box = new TextBox
                        {
                            VerticalAlignment = VerticalAlignment.Center,
                            Text = existing != null && existing.Quantity > 0 ? existing.Quantity.ToString(CultureInfo.InvariantCulture) : string.Empty,
                            ToolTip = "Cantidad (vacío o 0 = no incluir)."
                        };
                        Grid.SetColumn(box, 1);
                        grid.Children.Add(box);
                        row.Quantity = box;
                    }

                    if (row.Variant != null)
                    {
                        row.Variant.SelectionChanged += (s, e) => RefreshVariantControls(row);
                        RefreshVariantControls(row);
                    }

                    list.Children.Add(grid);
                    rows.Add(row);
                }
            }

            root.Children.Add(new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto, Content = list });
            Content = root;
        }

        private static string SelectedElementId(Row row)
            => row?.Variant?.SelectedItem is CatalogOption option ? option.Id : row?.Id;

        private static string SelectedElementLabel(Row row)
            => row?.Variant?.SelectedItem is CatalogOption option && !string.IsNullOrWhiteSpace(option.Id)
                ? option.DisplayName
                : row?.Label;

        private static void RefreshVariantControls(Row row)
        {
            var enabled = !string.IsNullOrWhiteSpace(SelectedElementId(row));
            if (row.Side != null) row.Side.IsEnabled = enabled;
            if (row.PerPost != null) row.PerPost.IsEnabled = enabled;
            if (row.Quantity != null) row.Quantity.IsEnabled = enabled;
            if (row.TopeButton != null) row.TopeButton.IsEnabled = enabled;
            if (row.ParrillaButton != null) row.ParrillaButton.IsEnabled = enabled;
        }

        private static string PerPostLabel(int overrides)
            => overrides > 0 ? "Por poste (" + overrides.ToString(CultureInfo.InvariantCulture) + ")…" : "Por poste…";

        private void EditPerPost(Row row)
        {
            // A bota has a general side ("(por defecto)" in the per-post editor); a lateral has none, and defaults to the
            // orillas (first + last frente) the first time it's configured.
            var defaultSide = row.IsBota ? (SafetySide)Math.Max(0, row.Side.SelectedIndex) : SafetySide.None;
            var current = row.PostSides;
            if (row.IsLateral && (current == null || current.Count == 0))
            {
                current = OrillaDefaults(postCount);
            }

            var dialog = new SafetyPerPostWindow(SelectedElementLabel(row), postCount, defaultSide, current) { Owner = this };
            if (dialog.ShowDialog() != true)
            {
                return;
            }

            row.PostSides = dialog.Result.ToList();
            row.PerPost.Content = PerPostLabel(row.PostSides.Count);
        }

        private static string TopeLabel(Row row) => row.TopeConfigured ? "Configurado ✓…" : "Configurar…";

        private void EditTope(Row row)
        {
            var dialog = new SafetyTopeGridWindow(SelectedElementLabel(row), levelsPerFrente, row.TopeShared, row.TopeSide, row.TopeSaque, row.TopeFrontal, row.TopeOffCells, fondoCount, row.TopeFondo) { Owner = this };
            if (dialog.ShowDialog() != true)
            {
                return;
            }

            var r = dialog.Result;
            row.TopeConfigured = true;
            row.TopeShared = r.Shared;
            row.TopeSide = r.Side;
            row.TopeSaque = r.Saque;
            row.TopeFrontal = r.Frontal;
            row.TopeFondo = r.Fondo;
            row.TopeOffCells = r.OffCells;
            row.TopeButton.Content = TopeLabel(row);
        }

        private static string ParrillaLabel(Row row) => row.ParrillaConfigured ? "Configurado ✓…" : "Configurar…";

        private void EditParrilla(Row row)
        {
            var dialog = new SafetyParrillaGridWindow(row.Label, levelsPerFrente, row.ParrillaFrontal, row.ParrillaLateral, row.ParrillaFrente, row.ParrillaCantidad, row.ParrillaOffCells, parrillaPlan) { Owner = this };
            if (dialog.ShowDialog() != true)
            {
                return;
            }

            var r = dialog.Result;
            row.ParrillaConfigured = true;
            row.ParrillaFrontal = r.Frontal;
            row.ParrillaLateral = r.Lateral;
            row.ParrillaFrente = r.Frente;
            row.ParrillaCantidad = r.Cantidad;
            row.ParrillaOffCells = r.OffCells;
            row.ParrillaButton.Content = ParrillaLabel(row);
        }

        /// <summary>Default posts for a protector lateral: the two orillas (first and last frente), the guide on opposite
        /// sides — the first frente as-is (Left), the last mirrored (Right) — ONE block each. "Ambos" is for a bridge.</summary>
        private static List<SafetyPostSide> OrillaDefaults(int postCount)
        {
            var result = new List<SafetyPostSide> { new SafetyPostSide { PostIndex = 0, Side = SafetySide.Left } };
            if (postCount > 1)
            {
                result.Add(new SafetyPostSide { PostIndex = postCount - 1, Side = SafetySide.Right });
            }

            return result;
        }

        private static string GroupTitle(string type)
        {
            switch ((type ?? string.Empty).Trim().ToUpperInvariant())
            {
                case SelectiveSafetyDefaults.BotaType: return "Protectores de bota (base de poste)";
                case SelectiveSafetyDefaults.LateralType: return "Protectores laterales (extremos)";
                case "DESVIADOR": return "Desviadores";
                case SelectiveSafetyDefaults.TopeType: return "Topes";
                case "TRASERA": return "Guardas traseras";
                case SelectiveSafetyDefaults.ParrillaType:
                case SelectiveSafetyDefaults.DeckLegacyType: return "Parrillas / deck";
                default: return string.IsNullOrWhiteSpace(type) ? "Otros" : type;
            }
        }

        private void OnOk()
        {
            var result = new List<SelectiveSafetySelection>();
            foreach (var row in rows)
            {
                var elementId = SelectedElementId(row);
                if (string.IsNullOrWhiteSpace(elementId))
                {
                    continue;
                }

                if (row.IsTope)
                {
                    if (row.TopeConfigured)
                    {
                        // Disabled when every (frente,level) cell is off.
                        if (!SelectiveSafetyGrid.AllCellsOff(levelsPerFrente, row.TopeOffCells))
                        {
                            var selection = new SelectiveSafetySelection { ElementId = elementId, Side = row.TopeSide, Quantity = 1, TopeShared = row.TopeShared, TopeSaque = row.TopeSaque, TopeFrontal = row.TopeFrontal, TopeFondo = row.TopeFondo };
                            foreach (var c in row.TopeOffCells)
                            {
                                if (c != null) selection.TopeOffCells.Add(new SelectiveGridCell { Frente = c.Frente, Level = c.Level });
                            }

                            result.Add(selection);
                        }
                    }

                    continue;
                }

                if (row.IsParrilla)
                {
                    // The grid drives the BOM regardless of the view toggles (a deck counts even if you hide both views),
                    // so gate ONLY on being configured — the per-view toggles gate the drawing, not BOM membership.
                    if (row.ParrillaConfigured)
                    {
                        // Disabled when every (frente,level) cell is off.
                        if (!SelectiveSafetyGrid.AllCellsOff(levelsPerFrente, row.ParrillaOffCells))
                        {
                            // Side = Both makes EnabledOfType treat it as "drawn"; the per-view toggles gate the actual draw.
                            var selection = new SelectiveSafetySelection { ElementId = elementId, Side = SafetySide.Both, Quantity = 1, ParrillaFrontal = row.ParrillaFrontal, ParrillaLateral = row.ParrillaLateral, ParrillaFrente = row.ParrillaFrente, ParrillaCantidad = row.ParrillaCantidad };
                            foreach (var c in row.ParrillaOffCells)
                            {
                                if (c != null) selection.ParrillaOffCells.Add(new SelectiveGridCell { Frente = c.Frente, Level = c.Level });
                            }

                            result.Add(selection);
                        }
                    }

                    continue;
                }

                if (row.IsBota || row.IsLateral)
                {
                    // A bota has a general side; a lateral is per-post only (Side stays None).
                    var side = row.IsBota ? (SafetySide)Math.Max(0, row.Side.SelectedIndex) : SafetySide.None;
                    var overrides = row.PostSides ?? new List<SafetyPostSide>();
                    var drawsSomewhere = side != SafetySide.None || overrides.Any(p => p != null && p.Side != SafetySide.None);
                    if (!drawsSomewhere)
                    {
                        continue; // nothing draws for this element
                    }

                    var selection = new SelectiveSafetySelection { ElementId = elementId, Side = side, Quantity = 1 };
                    foreach (var post in overrides)
                    {
                        if (post != null && post.PostIndex >= 0) selection.PostSides.Add(new SafetyPostSide { PostIndex = post.PostIndex, Side = post.Side });
                    }

                    result.Add(selection);
                    continue;
                }

                var text = (row.Quantity.Text ?? string.Empty).Trim();
                if (text.Length == 0)
                {
                    continue;
                }

                if (!UiSupport.TryInt(text, out var quantity) || quantity < 0)
                {
                    error.Text = "Cantidad inválida en '" + elementId + "': usa un entero ≥ 0 (vacío = ninguno).";
                    return;
                }

                if (quantity > 0)
                {
                    result.Add(new SelectiveSafetySelection { ElementId = elementId, Quantity = quantity, Side = SafetySide.None });
                }
            }

            Result = result;
            DialogResult = true;
        }
    }

    /// <summary>
    /// Per-post side override editor for one bota: a row per post (1..N) with a side combo whose first option
    /// "(por defecto)" means "use the general side". Only posts set to an explicit side are returned as overrides.
    /// </summary>
    public sealed class SafetyPerPostWindow : Window
    {
        // Index 0 = "(por defecto)" (no override); 1..4 map to SafetySide None/Left/Right/Both (side = index-1).
        private static readonly string[] Options = { "(por defecto)", "Ninguno", "Izquierda", "Derecha", "Ambas" };

        private readonly List<ComboBox> combos = new List<ComboBox>();

        public IReadOnlyList<SafetyPostSide> Result { get; private set; } = new List<SafetyPostSide>();

        public SafetyPerPostWindow(string elementLabel, int postCount, SafetySide defaultSide, IEnumerable<SafetyPostSide> current)
        {
            postCount = Math.Max(1, postCount);
            var byPost = new Dictionary<int, SafetySide>();
            foreach (var post in current ?? Enumerable.Empty<SafetyPostSide>())
            {
                if (post != null && post.PostIndex >= 0) byPost[post.PostIndex] = post.Side;
            }

            Title = string.IsNullOrWhiteSpace(elementLabel) ? "Lado por poste" : elementLabel;
            Width = 340;
            Height = 460;
            MinWidth = 300;
            MinHeight = 260;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            FontFamily = new FontFamily("Segoe UI");
            Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("/RackCad.UI;component/Themes/AppStyles.xaml", UriKind.Relative) });
            if (TryFindResource("WindowBackgroundBrush") is Brush background) Background = background;

            var root = new DockPanel { Margin = new Thickness(14) };

            // A bota has a general side, so "(por defecto)" means "use it"; a lateral has none, so it means "no lleva".
            var perDefault = defaultSide == SafetySide.None
                ? "\"(por defecto)\" = no lleva."
                : "\"(por defecto)\" usa el lado general (" + SideName(defaultSide) + ").";
            var intro = new TextBlock
            {
                Text = (string.IsNullOrWhiteSpace(elementLabel) ? "Lado por poste" : elementLabel) + " — lado por poste. " + perDefault,
                TextWrapping = TextWrapping.Wrap, FontSize = 11.5, Margin = new Thickness(0, 0, 0, 10)
            };
            DockPanel.SetDock(intro, Dock.Top);
            root.Children.Add(intro);

            var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 12, 0, 0) };
            var reset = new Button { Style = TryFindResource("SecondaryButtonStyle") as Style, Content = "Todos por defecto", Padding = new Thickness(10, 3, 10, 3), Margin = new Thickness(0, 0, 8, 0) };
            reset.Click += (s, e) => { foreach (var c in combos) c.SelectedIndex = 0; };
            var ok = new Button { Style = TryFindResource("PrimaryButtonStyle") as Style, Content = "Aceptar", Padding = new Thickness(16, 3, 16, 3), IsDefault = true, Margin = new Thickness(0, 0, 8, 0) };
            ok.Click += (s, e) => OnOk();
            var cancel = new Button { Style = TryFindResource("SecondaryButtonStyle") as Style, Content = "Cancelar", Padding = new Thickness(10, 3, 10, 3), IsCancel = true };
            buttons.Children.Add(reset);
            buttons.Children.Add(ok);
            buttons.Children.Add(cancel);
            DockPanel.SetDock(buttons, Dock.Bottom);
            root.Children.Add(buttons);

            var list = new StackPanel();
            for (var i = 0; i < postCount; i++)
            {
                var grid = new Grid { Margin = new Thickness(0, 2, 0, 2) };
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(140) });

                var caption = i == 0 ? "Poste 1 (extremo)"
                    : i == postCount - 1 ? "Poste " + (i + 1).ToString(CultureInfo.InvariantCulture) + " (extremo)"
                    : "Poste " + (i + 1).ToString(CultureInfo.InvariantCulture);
                var label = new TextBlock { Text = caption, VerticalAlignment = VerticalAlignment.Center };
                Grid.SetColumn(label, 0);
                grid.Children.Add(label);

                var combo = new ComboBox { VerticalAlignment = VerticalAlignment.Center };
                foreach (var option in Options) combo.Items.Add(option);
                combo.SelectedIndex = byPost.TryGetValue(i, out var side) ? (int)side + 1 : 0;
                Grid.SetColumn(combo, 1);
                grid.Children.Add(combo);
                combos.Add(combo);

                list.Children.Add(grid);
            }

            root.Children.Add(new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto, Content = list });
            Content = root;
        }

        private static string SideName(SafetySide side)
        {
            switch (side)
            {
                case SafetySide.Left: return "Izquierda";
                case SafetySide.Right: return "Derecha";
                case SafetySide.Both: return "Ambas";
                default: return "Ninguno";
            }
        }

        private void OnOk()
        {
            var result = new List<SafetyPostSide>();
            for (var i = 0; i < combos.Count; i++)
            {
                var index = combos[i].SelectedIndex;
                if (index > 0) // 0 = "(por defecto)" → no override
                {
                    result.Add(new SafetyPostSide { PostIndex = i, Side = (SafetySide)(index - 1) });
                }
            }

            Result = result;
            DialogResult = true;
        }
    }
}
