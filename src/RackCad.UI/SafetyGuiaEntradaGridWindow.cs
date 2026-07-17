using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;

namespace RackCad.UI
{
    /// <summary>Chooses the dynamic-rack frente/level cells that receive the mirrored entrance-guide pair.</summary>
    public sealed class SafetyGuiaEntradaGridWindow : Window
    {
        private readonly CheckBox[][] cells;
        private readonly IReadOnlyList<int> levelsPerFront;

        public IReadOnlyList<SelectiveGridCell> Result { get; private set; } = new List<SelectiveGridCell>();

        public SafetyGuiaEntradaGridWindow(
            string elementLabel,
            IReadOnlyList<int> levelsPerFront,
            IEnumerable<SelectiveGridCell> offCells)
        {
            this.levelsPerFront = (levelsPerFront ?? Array.Empty<int>())
                .Select(count => Math.Max(1, count))
                .ToList();
            if (this.levelsPerFront.Count == 0)
            {
                this.levelsPerFront = new List<int> { 1 };
            }

            var off = SelectiveSafetyGrid.OffCellKeys(offCells);
            var maxLevels = this.levelsPerFront.Max();
            cells = new CheckBox[this.levelsPerFront.Count][];

            Title = string.IsNullOrWhiteSpace(elementLabel) ? "Guía de entrada" : elementLabel;
            Width = Math.Max(470, Math.Min(1000, 220 + this.levelsPerFront.Count * 54));
            Height = Math.Min(680, 245 + maxLevels * 30);
            MinWidth = 450;
            MinHeight = 300;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            FontFamily = new FontFamily("Segoe UI");
            Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri("/RackCad.UI;component/Themes/AppStyles.xaml", UriKind.Relative)
            });
            if (TryFindResource("WindowBackgroundBrush") is Brush background) Background = background;

            var root = new DockPanel { Margin = new Thickness(14) };
            var intro = new TextBlock
            {
                Text = "Marca los niveles de cada frente que llevan guía de entrada (todos por defecto). Cada celda "
                     + "coloca una pareja espejeada, 8\" arriba del larguero IN/OUT de entrada; LONGITUD se resuelve "
                     + "automáticamente con el tramo de cabecera correspondiente.",
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 10)
            };
            DockPanel.SetDock(intro, Dock.Top);
            root.Children.Add(intro);

            var buttons = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 12, 0, 0)
            };
            var all = Button("Todos", false);
            all.Click += (_, __) => SetAll(true);
            var none = Button("Ninguno", false);
            none.Click += (_, __) => SetAll(false);
            var ok = Button("Aceptar", true);
            ok.Click += (_, __) => Accept();
            var cancel = Button("Cancelar", false);
            cancel.IsCancel = true;
            buttons.Children.Add(all);
            buttons.Children.Add(none);
            buttons.Children.Add(ok);
            buttons.Children.Add(cancel);
            DockPanel.SetDock(buttons, Dock.Bottom);
            root.Children.Add(buttons);

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(75) });
            for (var front = 0; front < this.levelsPerFront.Count; front++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(52) });
            }

            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            for (var level = 0; level < maxLevels; level++)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }

            for (var front = 0; front < this.levelsPerFront.Count; front++)
            {
                cells[front] = new CheckBox[this.levelsPerFront[front]];
                var heading = new TextBlock
                {
                    Text = "F" + (front + 1).ToString(CultureInfo.InvariantCulture),
                    FontWeight = FontWeights.SemiBold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 4)
                };
                Grid.SetColumn(heading, front + 1);
                grid.Children.Add(heading);
            }

            for (var level = maxLevels - 1; level >= 0; level--)
            {
                var row = maxLevels - level;
                var label = new TextBlock
                {
                    Text = "Nivel " + (level + 1).ToString(CultureInfo.InvariantCulture),
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetRow(label, row);
                grid.Children.Add(label);
                for (var front = 0; front < this.levelsPerFront.Count; front++)
                {
                    if (level >= this.levelsPerFront[front]) continue;
                    var check = new CheckBox
                    {
                        IsChecked = !off.Contains((front, level)),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 3, 0, 3)
                    };
                    cells[front][level] = check;
                    Grid.SetRow(check, row);
                    Grid.SetColumn(check, front + 1);
                    grid.Children.Add(check);
                }
            }

            root.Children.Add(new ScrollViewer
            {
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Content = grid
            });
            Content = root;
        }

        private Button Button(string content, bool primary)
            => new Button
            {
                Content = content,
                Style = TryFindResource(primary ? "PrimaryButtonStyle" : "SecondaryButtonStyle") as Style,
                Padding = new Thickness(primary ? 16 : 10, 3, primary ? 16 : 10, 3),
                Margin = new Thickness(0, 0, 8, 0),
                IsDefault = primary
            };

        private void SetAll(bool value)
        {
            foreach (var front in cells)
            {
                foreach (var cell in front)
                {
                    cell.IsChecked = value;
                }
            }
        }

        private void Accept()
        {
            var off = new List<SelectiveGridCell>();
            for (var front = 0; front < cells.Length; front++)
            {
                for (var level = 0; level < cells[front].Length; level++)
                {
                    if (cells[front][level].IsChecked != true)
                    {
                        off.Add(new SelectiveGridCell { Frente = front, Level = level });
                    }
                }
            }

            Result = off;
            DialogResult = true;
        }
    }
}
