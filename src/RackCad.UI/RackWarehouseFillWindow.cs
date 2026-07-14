using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RackCad.UI
{
    /// <summary>
    /// Asks how to auto-fill a warehouse site with a rack: which layer holds the site geometry (envelope polyline +
    /// columns), the aisles, back-to-back or not, the clearances, and whether to try the rotated orientation. Built in
    /// code (no XAML), like the other dialogs. On OK, <see cref="Result"/> holds the choice. Copies are always LINKED
    /// (one shared block; the BOM counts them) — an auto-fill of independent racks would clone hundreds of definitions.
    /// </summary>
    public sealed class RackWarehouseFillWindow : Window
    {
        private readonly TextBox layer;
        private readonly TextBox aisleRows;
        private readonly TextBox aisleCols;
        private readonly CheckBox backToBack;
        private readonly TextBox backGap;
        private readonly TextBox wallClearance;
        private readonly TextBox columnClearance;
        private readonly CheckBox tryRotated;
        private readonly TextBlock error;

        public sealed class FillResult
        {
            public string Layer;
            public double AisleBetweenRows;
            public double AisleBetweenCols;
            public bool BackToBack;
            public double BackGap;
            public double WallClearance;
            public double ColumnClearance;
            public bool TryRotated;
        }

        public FillResult Result { get; private set; }

        public RackWarehouseFillWindow(string rackName, double footprintDepth, double footprintWidth)
        {
            Title = "Rellenar almacén con racks";
            Width = 500;
            SizeToContent = SizeToContent.Height;
            MinWidth = 460;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            FontFamily = new FontFamily("Segoe UI");
            Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("/RackCad.UI;component/Themes/AppStyles.xaml", UriKind.Relative) });
            if (TryFindResource("WindowBackgroundBrush") is Brush background) Background = background;

            var root = new StackPanel { Margin = new Thickness(16) };

            var name = string.IsNullOrWhiteSpace(rackName) ? "el rack seleccionado" : "\"" + rackName.Trim() + "\"";
            root.Children.Add(new TextBlock
            {
                Text = "Rellena el área disponible con copias de " + name + ". El contorno de la nave debe ser una "
                     + "POLILÍNEA CERRADA en la capa indicada; lo demás en esa capa (círculos, rectángulos, bloques) "
                     + "se toma como columnas/obstáculos a librar. Los arcos se aproximan por sus vértices.",
                TextWrapping = TextWrapping.Wrap, FontSize = 11.5, Margin = new Thickness(0, 0, 0, 6)
            });

            root.Children.Add(new TextBlock
            {
                Text = string.Format(CultureInfo.InvariantCulture,
                    "Tamaño del rack en planta: fondo {0:0.##}\" × ancho {1:0.##}\". Copias enlazadas (editar una edita todas).",
                    footprintDepth, footprintWidth),
                TextWrapping = TextWrapping.Wrap, FontSize = 11, Opacity = 0.8, Margin = new Thickness(0, 0, 0, 12)
            });

            var grid = NewGrid(6);
            layer = AddRow(grid, 0, "Capa del sitio:", "RACKCAD_SITIO", 130);
            aisleRows = AddRow(grid, 1, "Pasillo entre filas (in):", "96");
            aisleCols = AddRow(grid, 2, "Pasillo entre columnas (in):", "96");
            wallClearance = AddRow(grid, 3, "Holgura a muros (in):", "0");
            columnClearance = AddRow(grid, 4, "Holgura a columnas (in):", "4");
            backGap = AddRow(grid, 5, "Hueco entre espaldas (in):", "6");
            root.Children.Add(grid);

            backToBack = new CheckBox
            {
                Content = "Filas espalda-con-espalda (pasillo solo entre pares)",
                IsChecked = false,
                Margin = new Thickness(0, 8, 0, 0),
                ToolTip = "Empareja las filas de dos en dos: comparten el hueco entre espaldas y el pasillo va solo entre pares (más densidad)."
            };
            root.Children.Add(backToBack);

            tryRotated = new CheckBox
            {
                Content = "Probar también la orientación girada 90° (usa la que quepa más)",
                IsChecked = true,
                Margin = new Thickness(0, 6, 0, 0),
                ToolTip = "Los pasillos siguen al rack al girar (el de picking siempre frente a las caras). "
                        + "Con espalda-con-espalda solo se prueba la orientación natural (rota el rack antes si quieres la otra)."
            };
            root.Children.Add(tryRotated);

            error = new TextBlock { FontSize = 11, Foreground = Brushes.Firebrick, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 8, 0, 0) };
            root.Children.Add(error);

            var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 14, 0, 0) };
            var ok = new Button { Style = TryFindResource("PrimaryButtonStyle") as Style, Content = "Calcular", Padding = new Thickness(16, 3, 16, 3), IsDefault = true, Margin = new Thickness(0, 0, 8, 0) };
            ok.Click += (s, e) => OnOk();
            var cancel = new Button { Style = TryFindResource("SecondaryButtonStyle") as Style, Content = "Cancelar", Padding = new Thickness(10, 3, 10, 3), IsCancel = true };
            buttons.Children.Add(ok);
            buttons.Children.Add(cancel);
            root.Children.Add(buttons);

            Content = root;
        }

        private static Grid NewGrid(int rowCount)
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            for (var r = 0; r < rowCount; r++) grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            return grid;
        }

        private static TextBox AddRow(Grid grid, int row, string label, string value, double width = 80)
        {
            var text = new TextBlock { Text = label, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 4, 8, 4) };
            Grid.SetRow(text, row);
            Grid.SetColumn(text, 0);
            grid.Children.Add(text);

            var box = new TextBox { Text = value, Width = width, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 4, 0, 4), HorizontalAlignment = HorizontalAlignment.Right };
            Grid.SetRow(box, row);
            Grid.SetColumn(box, 1);
            grid.Children.Add(box);
            return box;
        }

        private void OnOk()
        {
            var layerName = (layer.Text ?? string.Empty).Trim();
            if (layerName.Length == 0)
            {
                error.Text = "Indica la capa donde está el contorno del sitio.";
                return;
            }

            if (!TryParseLength(aisleRows.Text, out var aisleR))
            {
                error.Text = "Pasillo entre filas inválido: usa un número ≥ 0.";
                return;
            }

            if (!TryParseLength(aisleCols.Text, out var aisleC))
            {
                error.Text = "Pasillo entre columnas inválido: usa un número ≥ 0.";
                return;
            }

            if (!TryParseLength(wallClearance.Text, out var wall))
            {
                error.Text = "Holgura a muros inválida: usa un número ≥ 0.";
                return;
            }

            if (!TryParseLength(columnClearance.Text, out var column))
            {
                error.Text = "Holgura a columnas inválida: usa un número ≥ 0.";
                return;
            }

            var pairs = backToBack.IsChecked == true;
            var gap = 0.0;
            if (pairs && !TryParseLength(backGap.Text, out gap))
            {
                error.Text = "Hueco entre espaldas inválido: usa un número ≥ 0.";
                return;
            }

            Result = new FillResult
            {
                Layer = layerName,
                AisleBetweenRows = aisleR,
                AisleBetweenCols = aisleC,
                BackToBack = pairs,
                BackGap = gap,
                WallClearance = wall,
                ColumnClearance = column,
                TryRotated = tryRotated.IsChecked == true
            };
            DialogResult = true;
        }

        private static bool TryParseLength(string text, out double value)
            => double.TryParse((text ?? string.Empty).Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out value) && value >= 0.0;
    }
}
