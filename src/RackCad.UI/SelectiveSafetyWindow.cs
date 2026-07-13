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
    /// the drawing; a non-drawable element keeps a manual QUANTITY for the BOM. On OK, <see cref="Result"/> holds the
    /// selections. Built in code (no XAML), like the tramos dialog. Per-post customization is a later phase.
    /// </summary>
    public sealed class SelectiveSafetyWindow : Window
    {
        private static readonly string[] SideLabels = { "Ninguno", "Izquierda", "Derecha", "Ambas" };

        private readonly List<Row> rows = new List<Row>();
        private readonly TextBlock error;

        private sealed class Row
        {
            public string Id;
            public bool IsBota;
            public ComboBox Side;   // BOTA
            public TextBox Quantity; // non-drawable
        }

        public IReadOnlyList<SelectiveSafetySelection> Result { get; private set; } = new List<SelectiveSafetySelection>();

        public SelectiveSafetyWindow(IReadOnlyList<SafetyElementCatalogEntry> elements, IEnumerable<SelectiveSafetySelection> current)
        {
            elements ??= new List<SafetyElementCatalogEntry>();
            var currentById = new Dictionary<string, SelectiveSafetySelection>(StringComparer.OrdinalIgnoreCase);
            foreach (var selection in current ?? Enumerable.Empty<SelectiveSafetySelection>())
            {
                if (selection != null && !string.IsNullOrWhiteSpace(selection.ElementId)) currentById[selection.ElementId] = selection;
            }

            Title = "Elementos de seguridad";
            Width = 460;
            Height = 540;
            MinWidth = 380;
            MinHeight = 300;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            FontFamily = new FontFamily("Segoe UI");
            Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("/RackCad.UI;component/Themes/AppStyles.xaml", UriKind.Relative) });
            if (TryFindResource("WindowBackgroundBrush") is Brush background) Background = background;

            var root = new DockPanel { Margin = new Thickness(14) };

            var intro = new TextBlock
            {
                Text = "La bota se dibuja en la base de CADA poste según el lado (por ahora todos los postes; la opción por "
                     + "poste viene después). El BOM se cuenta del dibujo. Los demás elementos usan una cantidad manual.",
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

                    var grid = new Grid { Margin = new Thickness(0, 2, 0, 2) };
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110) });

                    var label = new TextBlock { Text = element.Label, VerticalAlignment = VerticalAlignment.Center, TextWrapping = TextWrapping.Wrap };
                    Grid.SetColumn(label, 0);
                    grid.Children.Add(label);

                    currentById.TryGetValue(element.Id, out var existing);
                    var isBota = string.Equals(element.Type, "BOTA", StringComparison.OrdinalIgnoreCase);
                    var row = new Row { Id = element.Id, IsBota = isBota };

                    if (isBota)
                    {
                        var combo = new ComboBox { VerticalAlignment = VerticalAlignment.Center, ToolTip = "Lado de la bota en cada poste (Ninguno = no lleva)." };
                        foreach (var side in SideLabels) combo.Items.Add(side);
                        combo.SelectedIndex = existing != null ? (int)existing.Side : (int)SafetySide.None;
                        Grid.SetColumn(combo, 1);
                        grid.Children.Add(combo);
                        row.Side = combo;
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
                if (row.IsBota)
                {
                    var side = (SafetySide)Math.Max(0, row.Side.SelectedIndex);
                    if (side != SafetySide.None)
                    {
                        result.Add(new SelectiveSafetySelection { ElementId = row.Id, Side = side, Quantity = 1 });
                    }

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
}
