using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using RackCad.Application.Catalogs;
using RackCad.Domain.Systems;

namespace RackCad.UI
{
    /// <summary>
    /// Selection of SAFETY accessories for a selective rack. A DRAWABLE element (type BOTA) is chosen by SIDE
    /// (Ninguno / Izquierda / Derecha / Ambas) — it's drawn at every post's base plate on that side and counted from
    /// the drawing — with an optional "Por poste…" override for specific posts; a non-drawable element keeps a manual
    /// QUANTITY for the BOM. On OK, <see cref="Result"/> holds the selections. Built in code (no XAML), like the tramos
    /// dialog.
    /// </summary>
    public sealed class SelectiveSafetyWindow : Window
    {
        private static readonly string[] SideLabels = { "Ninguno", "Izquierda", "Derecha", "Ambas" };

        private readonly List<Row> rows = new List<Row>();
        private readonly TextBlock error;
        private readonly int postCount;

        private sealed class Row
        {
            public string Id;
            public string Label;
            public bool IsBota;                      // BOTA: general side on every frente
            public bool IsLateral;                   // LATERAL: per-post only (defaults to the orillas), replaces botas
            public bool IsTope;                      // TOPE: on/off (rear pallet stop at the central fondo)
            public ComboBox Side;                    // BOTA: default side
            public Button PerPost;                   // BOTA/LATERAL: "Por poste…"
            public List<SafetyPostSide> PostSides;   // BOTA/LATERAL: per-post overrides (working copy)
            public TextBox Quantity;                 // non-drawable
        }

        public IReadOnlyList<SelectiveSafetySelection> Result { get; private set; } = new List<SelectiveSafetySelection>();

        public SelectiveSafetyWindow(IReadOnlyList<SafetyElementCatalogEntry> elements, IEnumerable<SelectiveSafetySelection> current, int postCount)
        {
            this.postCount = Math.Max(1, postCount);
            elements ??= new List<SafetyElementCatalogEntry>();
            var currentById = new Dictionary<string, SelectiveSafetySelection>(StringComparer.OrdinalIgnoreCase);
            foreach (var selection in current ?? Enumerable.Empty<SelectiveSafetySelection>())
            {
                if (selection != null && !string.IsNullOrWhiteSpace(selection.ElementId)) currentById[selection.ElementId] = selection;
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
                Text = "La bota va en los postes EXTREMOS del sistema (no en cada fondo): Izquierda = frente (pasillo), Derecha "
                     + "= fondo, Ambos = los dos extremos. El lado general aplica a todos los frentes; usa \"Por poste…\" para "
                     + "personalizar cuáles llevan y en qué lado. El BOM se cuenta del dibujo. Los demás elementos usan una "
                     + "cantidad manual.",
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
                foreach (var element in elements)
                {
                    if (string.IsNullOrWhiteSpace(element?.Id)) continue;

                    if (!string.Equals(element.Type, lastType, StringComparison.OrdinalIgnoreCase))
                    {
                        lastType = element.Type;
                        list.Children.Add(new TextBlock { Text = GroupTitle(element.Type), FontWeight = FontWeights.SemiBold, Foreground = Brushes.Gray, FontSize = 11, Margin = new Thickness(0, 8, 0, 2) });
                    }

                    currentById.TryGetValue(element.Id, out var existing);
                    var isBota = string.Equals(element.Type, "BOTA", StringComparison.OrdinalIgnoreCase);
                    var isLateral = string.Equals(element.Type, "LATERAL", StringComparison.OrdinalIgnoreCase);
                    var isTope = string.Equals(element.Type, "TOPE", StringComparison.OrdinalIgnoreCase);
                    var row = new Row { Id = element.Id, Label = element.Label, IsBota = isBota, IsLateral = isLateral, IsTope = isTope };

                    var grid = new Grid { Margin = new Thickness(0, 2, 0, 2) };
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110) });
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                    var label = new TextBlock { Text = element.Label, VerticalAlignment = VerticalAlignment.Center, TextWrapping = TextWrapping.Wrap };
                    Grid.SetColumn(label, 0);
                    grid.Children.Add(label);

                    if (isTope)
                    {
                        // A tope is on/off for now (shared central, all levels); the shared/side + level config is a later phase.
                        var combo = new ComboBox { VerticalAlignment = VerticalAlignment.Center, ToolTip = "Larguero tope trasero: dibuja uno por larguero en el fondo central (lateral y planta). La frontal tiene su propio toggle." };
                        combo.Items.Add("No");
                        combo.Items.Add("Sí");
                        combo.SelectedIndex = existing != null && existing.Side != SafetySide.None ? 1 : 0;
                        Grid.SetColumn(combo, 1);
                        grid.Children.Add(combo);
                        row.Side = combo;
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

                    list.Children.Add(grid);
                    rows.Add(row);
                }
            }

            root.Children.Add(new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto, Content = list });
            Content = root;
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

            var dialog = new SafetyPerPostWindow(row.Label, postCount, defaultSide, current) { Owner = this };
            if (dialog.ShowDialog() != true)
            {
                return;
            }

            row.PostSides = dialog.Result.ToList();
            row.PerPost.Content = PerPostLabel(row.PostSides.Count);
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
                case "BOTA": return "Protectores de bota (base de poste)";
                case "LATERAL": return "Protectores laterales (extremos)";
                case "DESVIADOR": return "Desviadores";
                case "TOPE": return "Topes";
                case "TRASERA": return "Guardas traseras";
                case "DECK": return "Parrillas / deck";
                default: return string.IsNullOrWhiteSpace(type) ? "Otros" : type;
            }
        }

        private void OnOk()
        {
            var result = new List<SelectiveSafetySelection>();
            foreach (var row in rows)
            {
                if (row.IsTope)
                {
                    // On/off for now: "Sí" (index 1) enables the tope (stored as Both, meaning "drawn").
                    if (row.Side.SelectedIndex >= 1)
                    {
                        result.Add(new SelectiveSafetySelection { ElementId = row.Id, Side = SafetySide.Both, Quantity = 1 });
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

                    var selection = new SelectiveSafetySelection { ElementId = row.Id, Side = side, Quantity = 1 };
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

                if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var quantity) || quantity < 0)
                {
                    error.Text = "Cantidad inválida en '" + row.Id + "': usa un entero ≥ 0 (vacío = ninguno).";
                    return;
                }

                if (quantity > 0)
                {
                    result.Add(new SelectiveSafetySelection { ElementId = row.Id, Quantity = quantity, Side = SafetySide.None });
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
