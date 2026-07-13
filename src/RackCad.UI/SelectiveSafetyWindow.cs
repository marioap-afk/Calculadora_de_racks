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
    /// Selection of SAFETY accessories for a selective rack (Fase 0: catalog + selection + BOM, drawing is a future
    /// phase). Lists the catalog's safety elements grouped by type; the user sets a QUANTITY per element (blank/0 = not
    /// included). On OK, <see cref="Result"/> holds the selections with quantity &gt; 0. Built in code (no XAML), like
    /// the tramos dialog.
    /// </summary>
    public sealed class SelectiveSafetyWindow : Window
    {
        private readonly List<(string Id, TextBox Box)> rows = new List<(string, TextBox)>();
        private readonly TextBlock error;

        public IReadOnlyList<SelectiveSafetySelection> Result { get; private set; } = new List<SelectiveSafetySelection>();

        public SelectiveSafetyWindow(IReadOnlyList<SafetyElementCatalogEntry> elements, IEnumerable<SelectiveSafetySelection> current)
        {
            elements ??= new List<SafetyElementCatalogEntry>();
            var currentById = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var selection in current ?? Enumerable.Empty<SelectiveSafetySelection>())
            {
                if (selection != null && !string.IsNullOrWhiteSpace(selection.ElementId)) currentById[selection.ElementId] = selection.Quantity;
            }

            Title = "Elementos de seguridad";
            Width = 430;
            Height = 540;
            MinWidth = 360;
            MinHeight = 300;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            FontFamily = new FontFamily("Segoe UI");
            Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("/RackCad.UI;component/Themes/AppStyles.xaml", UriKind.Relative) });
            if (TryFindResource("WindowBackgroundBrush") is Brush background) Background = background;

            var root = new DockPanel { Margin = new Thickness(14) };

            var intro = new TextBlock
            {
                Text = "Cantidad de cada elemento de seguridad para este rack (vacío o 0 = ninguno). Entran a la lista de "
                     + "materiales; dibujarlos en las vistas es una fase posterior (falta el bloque de AutoCAD de cada uno).",
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

                    var row = new Grid { Margin = new Thickness(0, 2, 0, 2) };
                    row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(70) });

                    var label = new TextBlock { Text = element.Label, VerticalAlignment = VerticalAlignment.Center, TextWrapping = TextWrapping.Wrap };
                    Grid.SetColumn(label, 0);
                    row.Children.Add(label);

                    var box = new TextBox
                    {
                        VerticalAlignment = VerticalAlignment.Center,
                        Text = currentById.TryGetValue(element.Id, out var q) && q > 0 ? q.ToString(CultureInfo.InvariantCulture) : string.Empty,
                        ToolTip = "Cantidad (vacío o 0 = no incluir)."
                    };
                    Grid.SetColumn(box, 1);
                    row.Children.Add(box);

                    list.Children.Add(row);
                    rows.Add((element.Id, box));
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
            foreach (var (id, box) in rows)
            {
                var text = (box.Text ?? string.Empty).Trim();
                if (text.Length == 0)
                {
                    continue;
                }

                if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var quantity) || quantity < 0)
                {
                    error.Text = "Cantidad inválida en '" + id + "': usa un entero ≥ 0 (vacío = ninguno).";
                    return;
                }

                if (quantity > 0)
                {
                    result.Add(new SelectiveSafetySelection { ElementId = id, Quantity = quantity });
                }
            }

            Result = result;
            DialogResult = true;
        }
    }
}
