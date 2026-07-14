using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RackCad.UI
{
    /// <summary>
    /// Asks how to array a selected rack into a warehouse grid: rows × columns, the aisles, linked vs independent copies,
    /// optional back-to-back rows (paired, sharing a flue), and an optional fit check against a building envelope. Built
    /// in code (no XAML), like the other dialogs. On OK, <see cref="Result"/> holds the choice. (Orientation is inherited
    /// from the picked rack — rotate the seed before running the command to lay out rotated racks.)
    /// </summary>
    public sealed class RackWarehouseLayoutWindow : Window
    {
        private readonly TextBox rows;
        private readonly TextBox cols;
        private readonly TextBox aisleRows;
        private readonly TextBox aisleCols;
        private readonly CheckBox backToBack;
        private readonly TextBox backGap;
        private readonly CheckBox independent;
        private readonly TextBox buildingWidth;
        private readonly TextBox buildingDepth;
        private readonly TextBlock error;

        public sealed class LayoutResult
        {
            public int Rows;
            public int Cols;
            public double AisleBetweenRows;
            public double AisleBetweenCols;
            public bool Independent;
            public bool BackToBack;
            public double BackGap;
            // Optional fit check (bounds only): 0 = not requested.
            public double BuildingWidth;
            public double BuildingDepth;
        }

        public LayoutResult Result { get; private set; }

        public RackWarehouseLayoutWindow(string rackName, double footprintDepth, double footprintWidth)
        {
            Title = "Layout de almacén";
            Width = 480;
            SizeToContent = SizeToContent.Height;
            MinWidth = 440;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            FontFamily = new FontFamily("Segoe UI");
            Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("/RackCad.UI;component/Themes/AppStyles.xaml", UriKind.Relative) });
            if (TryFindResource("WindowBackgroundBrush") is Brush background) Background = background;

            var root = new StackPanel { Margin = new Thickness(16) };

            var name = string.IsNullOrWhiteSpace(rackName) ? "el rack seleccionado" : "\"" + rackName.Trim() + "\"";
            root.Children.Add(new TextBlock
            {
                Text = "Replica " + name + " en una rejilla de almacén. Las filas van a lo largo del fondo "
                     + "(separadas por el pasillo de picking) y las columnas a lo largo de la hilera.",
                TextWrapping = TextWrapping.Wrap, FontSize = 11.5, Margin = new Thickness(0, 0, 0, 6)
            });

            root.Children.Add(new TextBlock
            {
                Text = string.Format(CultureInfo.InvariantCulture,
                    "Tamaño del rack en planta: fondo {0:0.##}\" × ancho {1:0.##}\". (Para girarlos, rota el rack antes de correr el comando.)",
                    footprintDepth, footprintWidth),
                TextWrapping = TextWrapping.Wrap, FontSize = 11, Opacity = 0.8, Margin = new Thickness(0, 0, 0, 12)
            });

            var grid = NewGrid(4);
            rows = AddRow(grid, 0, "Filas (racks en paralelo):", "4");
            cols = AddRow(grid, 1, "Columnas (a lo largo de la hilera):", "1");
            aisleRows = AddRow(grid, 2, "Pasillo entre filas (in):", "96");
            aisleCols = AddRow(grid, 3, "Pasillo entre columnas (in):", "96");
            root.Children.Add(grid);

            // ---- Back-to-back ----
            backToBack = new CheckBox
            {
                Content = "Filas espalda-con-espalda (pasillo solo entre pares)",
                IsChecked = false,
                Margin = new Thickness(0, 10, 0, 0),
                ToolTip = "Empareja las filas de dos en dos: comparten un flue y el pasillo de picking va solo entre pares "
                        + "(más densidad). Nota (v1): el par queda al espaciado correcto pero con la misma orientación; "
                        + "aún no se espejea la fila pareja."
            };
            root.Children.Add(backToBack);

            var backGapGrid = NewGrid(1);
            backGap = AddRow(backGapGrid, 0, "Hueco entre espaldas (in):", "6");
            root.Children.Add(backGapGrid);

            // ---- Identity ----
            independent = new CheckBox
            {
                Content = "Racks independientes (cada copia editable por separado)",
                IsChecked = false,
                Margin = new Thickness(0, 8, 0, 0),
                ToolTip = "Desmarcado = copias enlazadas: un solo bloque, editar uno edita todas (más ligero). "
                        + "Marcado = cada copia con su propio GUID y nombre, editable por separado."
            };
            root.Children.Add(independent);

            // ---- Optional fit check ----
            root.Children.Add(new TextBlock
            {
                Text = "Verificar encaje (opcional): ¿cabe la rejilla en el edificio? Deja en blanco para omitir. "
                     + "La esquina del edificio se toma en el rack seleccionado.",
                TextWrapping = TextWrapping.Wrap, FontSize = 11, Opacity = 0.8, Margin = new Thickness(0, 14, 0, 4)
            });

            var fitGrid = NewGrid(2);
            buildingWidth = AddRow(fitGrid, 0, "Edificio a lo profundo — donde crecen las filas (in):", "");
            buildingDepth = AddRow(fitGrid, 1, "Edificio a lo largo — donde crecen las columnas (in):", "");
            root.Children.Add(fitGrid);

            error = new TextBlock { FontSize = 11, Foreground = Brushes.Firebrick, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 8, 0, 0) };
            root.Children.Add(error);

            var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 14, 0, 0) };
            var ok = new Button { Style = TryFindResource("PrimaryButtonStyle") as Style, Content = "Colocar", Padding = new Thickness(16, 3, 16, 3), IsDefault = true, Margin = new Thickness(0, 0, 8, 0) };
            ok.Click += (s, e) => OnOk();
            var cancel = new Button { Style = TryFindResource("SecondaryButtonStyle") as Style, Content = "Cancelar", Padding = new Thickness(10, 3, 10, 3), IsCancel = true };
            buttons.Children.Add(ok);
            buttons.Children.Add(cancel);
            root.Children.Add(buttons);

            Content = root;
        }

        private static Grid NewGrid(int rowCount)
        {
            var grid = new Grid { Margin = new Thickness(0, 0, 0, 0) };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
            for (var r = 0; r < rowCount; r++) grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            return grid;
        }

        private static TextBox AddRow(Grid grid, int row, string label, string value)
        {
            var text = new TextBlock { Text = label, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 4, 8, 4) };
            Grid.SetRow(text, row);
            Grid.SetColumn(text, 0);
            grid.Children.Add(text);

            var box = new TextBox { Text = value, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 4, 0, 4) };
            Grid.SetRow(box, row);
            Grid.SetColumn(box, 1);
            grid.Children.Add(box);
            return box;
        }

        private void OnOk()
        {
            if (!TryParseCount(rows.Text, out var rowCount))
            {
                error.Text = "Filas inválidas: usa un entero ≥ 1.";
                return;
            }

            if (!TryParseCount(cols.Text, out var colCount))
            {
                error.Text = "Columnas inválidas: usa un entero ≥ 1.";
                return;
            }

            if (rowCount == 1 && colCount == 1)
            {
                error.Text = "La rejilla debe tener más de un rack (aumenta filas o columnas).";
                return;
            }

            if ((long)rowCount * colCount > 1000)
            {
                error.Text = "La rejilla es demasiado grande (máx. 1000 racks). Reduce filas o columnas, o hazlo por partes.";
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

            var pairs = backToBack.IsChecked == true;
            var gap = 0.0;
            if (pairs && !TryParseLength(backGap.Text, out gap))
            {
                error.Text = "Hueco entre espaldas inválido: usa un número ≥ 0.";
                return;
            }

            // Optional fit check: either both building dimensions, or neither.
            var hasWidth = !string.IsNullOrWhiteSpace(buildingWidth.Text);
            var hasDepth = !string.IsNullOrWhiteSpace(buildingDepth.Text);
            var width = 0.0;
            var depth = 0.0;
            if ((hasWidth || hasDepth)
                && (!TryParsePositive(buildingWidth.Text, out width) || !TryParsePositive(buildingDepth.Text, out depth)))
            {
                error.Text = "Para verificar el encaje, escribe ancho y largo del edificio (números > 0), o deja ambos en blanco.";
                return;
            }

            Result = new LayoutResult
            {
                Rows = rowCount,
                Cols = colCount,
                AisleBetweenRows = aisleR,
                AisleBetweenCols = aisleC,
                Independent = independent.IsChecked == true,
                BackToBack = pairs,
                BackGap = gap,
                BuildingWidth = width,
                BuildingDepth = depth
            };
            DialogResult = true;
        }

        private static bool TryParseCount(string text, out int value)
            => int.TryParse((text ?? string.Empty).Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out value) && value >= 1;

        private static bool TryParseLength(string text, out double value)
            => double.TryParse((text ?? string.Empty).Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out value) && value >= 0.0;

        private static bool TryParsePositive(string text, out double value)
            => double.TryParse((text ?? string.Empty).Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out value) && value > 0.0;
    }
}
