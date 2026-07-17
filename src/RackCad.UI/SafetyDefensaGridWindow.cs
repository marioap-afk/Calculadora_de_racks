using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;

namespace RackCad.UI
{
    /// <summary>Dynamic forklift-defense editor: one transverse post per row, with physical end(s) and LONGITUD.</summary>
    public sealed class SafetyDefensaGridWindow : Window
    {
        private readonly List<Row> rows = new List<Row>();
        private readonly TextBlock error;
        private readonly int postCount;

        public IReadOnlyList<SafetyPostDefense> Result { get; private set; } = new List<SafetyPostDefense>();

        public SafetyDefensaGridWindow(
            string elementLabel,
            int postCount,
            IEnumerable<SafetyPostDefense> current)
        {
            this.postCount = Math.Max(1, postCount);
            Title = "Defensa de montacargas por poste";
            Width = 670;
            Height = 580;
            MinWidth = 600;
            MinHeight = 360;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri("/RackCad.UI;component/Themes/AppStyles.xaml", UriKind.Relative)
            });

            var root = new DockPanel { Margin = new Thickness(14) };
            var footer = new StackPanel { Margin = new Thickness(0, 10, 0, 0) };
            DockPanel.SetDock(footer, Dock.Bottom);
            error = new TextBlock
            {
                Foreground = System.Windows.Media.Brushes.Firebrick,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 6)
            };
            footer.Children.Add(error);
            var buttons = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            var ok = new Button
            {
                Content = "Aceptar",
                Style = TryFindResource("PrimaryButtonStyle") as Style,
                Padding = new Thickness(16, 3, 16, 3),
                Margin = new Thickness(0, 0, 8, 0),
                IsDefault = true
            };
            ok.Click += OnOk;
            var cancel = new Button
            {
                Content = "Cancelar",
                Style = TryFindResource("SecondaryButtonStyle") as Style,
                Padding = new Thickness(16, 3, 16, 3),
                IsCancel = true
            };
            buttons.Children.Add(ok);
            buttons.Children.Add(cancel);
            footer.Children.Add(buttons);
            root.Children.Add(footer);

            var content = new StackPanel();
            content.Children.Add(new TextBlock
            {
                Text = elementLabel ?? "Defensa de montacargas",
                FontSize = 15,
                FontWeight = FontWeights.SemiBold
            });
            content.Children.Add(new TextBlock
            {
                Text = "Cada extremo es independiente: puedes activar Salida, Entrada, ambos o ninguno y asignar una LONGITUD distinta a cada lado. Los valores predeterminados son 12\" por lado en orillas y 36\" por lado en postes intermedios.",
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 3, 0, 10)
            });

            var table = new Grid();
            table.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            table.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(70) });
            table.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(105) });
            table.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(70) });
            table.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(105) });
            AddHeader(table, "Poste", 0);
            AddHeader(table, "Salida", 1);
            AddHeader(table, "Long. salida", 2);
            AddHeader(table, "Entrada", 3);
            AddHeader(table, "Long. entrada", 4);

            var source = current?.Where(value => value != null).ToList() ?? new List<SafetyPostDefense>();
            for (var postIndex = 0; postIndex < this.postCount; postIndex++)
            {
                table.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                var setting = DynamicForkliftDefensePlan.At(source, postIndex, this.postCount);
                var defaults = DynamicForkliftDefensePlan.At(null, postIndex, this.postCount);
                var label = new TextBlock
                {
                    Text = "Poste " + (postIndex + 1).ToString(CultureInfo.InvariantCulture)
                           + (postIndex == 0 || postIndex == this.postCount - 1 ? " (orilla)" : " (intermedio)"),
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(2, 5, 8, 5)
                };
                var exit = new CheckBox
                {
                    IsChecked = setting.DrawsExit,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                var exitLength = new TextBox
                {
                    Text = (setting.DrawsExit ? setting.ExitLength : defaults.ExitLength)
                        .ToString("0.##", CultureInfo.InvariantCulture),
                    Margin = new Thickness(2, 3, 8, 3),
                    IsEnabled = setting.DrawsExit
                };
                var entrance = new CheckBox
                {
                    IsChecked = setting.DrawsEntrance,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                var entranceLength = new TextBox
                {
                    Text = (setting.DrawsEntrance ? setting.EntranceLength : defaults.EntranceLength)
                        .ToString("0.##", CultureInfo.InvariantCulture),
                    Margin = new Thickness(2, 3, 2, 3),
                    IsEnabled = setting.DrawsEntrance
                };
                exit.Checked += (_, __) => exitLength.IsEnabled = true;
                exit.Unchecked += (_, __) => exitLength.IsEnabled = false;
                entrance.Checked += (_, __) => entranceLength.IsEnabled = true;
                entrance.Unchecked += (_, __) => entranceLength.IsEnabled = false;

                Grid.SetRow(label, postIndex + 1);
                Grid.SetColumn(label, 0);
                Grid.SetRow(exit, postIndex + 1);
                Grid.SetColumn(exit, 1);
                Grid.SetRow(exitLength, postIndex + 1);
                Grid.SetColumn(exitLength, 2);
                Grid.SetRow(entrance, postIndex + 1);
                Grid.SetColumn(entrance, 3);
                Grid.SetRow(entranceLength, postIndex + 1);
                Grid.SetColumn(entranceLength, 4);
                table.Children.Add(label);
                table.Children.Add(exit);
                table.Children.Add(exitLength);
                table.Children.Add(entrance);
                table.Children.Add(entranceLength);
                rows.Add(new Row(postIndex, exit, exitLength, entrance, entranceLength));
            }

            content.Children.Add(table);
            root.Children.Add(new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Content = content
            });
            Content = root;
        }

        private static void AddHeader(Grid table, string text, int column)
        {
            if (table.RowDefinitions.Count == 0)
            {
                table.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }

            var label = new TextBlock
            {
                Text = text,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(2, 2, 8, 4)
            };
            Grid.SetRow(label, 0);
            Grid.SetColumn(label, column);
            table.Children.Add(label);
        }

        private void OnOk(object sender, RoutedEventArgs e)
        {
            var result = new List<SafetyPostDefense>();
            foreach (var row in rows)
            {
                var defaultSetting = DynamicForkliftDefensePlan.At(null, row.PostIndex, postCount);
                var exitLength = 0.0;
                if (row.Exit.IsChecked == true
                    && (!UiSupport.TryNum(row.ExitLength.Text, out exitLength) || exitLength <= 0.0))
                {
                    error.Text = "La longitud de salida del poste "
                                 + (row.PostIndex + 1).ToString(CultureInfo.InvariantCulture)
                                 + " debe ser mayor que cero.";
                    return;
                }

                var entranceLength = 0.0;
                if (row.Entrance.IsChecked == true
                    && (!UiSupport.TryNum(row.EntranceLength.Text, out entranceLength) || entranceLength <= 0.0))
                {
                    error.Text = "La longitud de entrada del poste "
                                 + (row.PostIndex + 1).ToString(CultureInfo.InvariantCulture)
                                 + " debe ser mayor que cero.";
                    return;
                }

                if (Math.Abs(exitLength - defaultSetting.ExitLength) > 1e-6
                    || Math.Abs(entranceLength - defaultSetting.EntranceLength) > 1e-6)
                {
                    result.Add(new SafetyPostDefense
                    {
                        PostIndex = row.PostIndex,
                        ExitLength = exitLength,
                        EntranceLength = entranceLength
                    });
                }
            }

            Result = result;
            DialogResult = true;
        }

        private sealed class Row
        {
            public Row(
                int postIndex,
                CheckBox exit,
                TextBox exitLength,
                CheckBox entrance,
                TextBox entranceLength)
            {
                PostIndex = postIndex;
                Exit = exit;
                ExitLength = exitLength;
                Entrance = entrance;
                EntranceLength = entranceLength;
            }

            public int PostIndex { get; }
            public CheckBox Exit { get; }
            public TextBox ExitLength { get; }
            public CheckBox Entrance { get; }
            public TextBox EntranceLength { get; }
        }
    }
}
