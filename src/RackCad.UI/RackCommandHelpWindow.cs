using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RackCad.UI
{
    /// <summary>
    /// In-app reference (opened by RACKAYUDA): every RackCad command with its short alias and a one-line summary,
    /// grouped. Read-only; built in code (no XAML) like the other dialogs. Data comes from
    /// <see cref="RackCommandReference"/> so it never drifts from the registered commands.
    /// </summary>
    public sealed class RackCommandHelpWindow : Window
    {
        public RackCommandHelpWindow()
        {
            Title = "RackCad — comandos y atajos";
            Width = 640;
            Height = 620;
            MinWidth = 520;
            MinHeight = 400;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            FontFamily = new FontFamily("Segoe UI");
            Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("/RackCad.UI;component/Themes/AppStyles.xaml", UriKind.Relative) });
            if (TryFindResource("WindowBackgroundBrush") is Brush background) Background = background;

            var chipBrush = (TryFindResource("AccentBrush") as Brush) ?? new SolidColorBrush(Color.FromRgb(0x2F, 0x6F, 0xED));
            var mutedBrush = (TryFindResource("SubtleTextBrush") as Brush) ?? new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x88));
            var textBrush = (TryFindResource("TextBrush") as Brush) ?? Foreground ?? Brushes.Black;

            var root = new DockPanel { Margin = new Thickness(16) };

            var intro = new TextBlock
            {
                Text = "Escribe el comando completo o su atajo en la línea de comandos de AutoCAD. Los atajos viajan con el "
                     + "plugin (no hay que editar acad.pgp). Si un atajo ya lo usa tu acad.pgp, el del PGP gana: usa el "
                     + "comando completo o cámbialo en la configuración.",
                TextWrapping = TextWrapping.Wrap, FontSize = 11.5, Margin = new Thickness(0, 0, 0, 12)
            };
            DockPanel.SetDock(intro, Dock.Top);
            root.Children.Add(intro);

            var close = new Button
            {
                Style = TryFindResource("PrimaryButtonStyle") as Style,
                Content = "Cerrar", Padding = new Thickness(16, 3, 16, 3), IsDefault = true, IsCancel = true,
                HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 12, 0, 0)
            };
            close.Click += (s, e) => Close();
            DockPanel.SetDock(close, Dock.Bottom);
            root.Children.Add(close);

            var list = new StackPanel();

            foreach (var group in RackCommandReference.Commands.GroupBy(command => command.Group))
            {
                list.Children.Add(new TextBlock
                {
                    Text = group.Key,
                    FontWeight = FontWeights.SemiBold, FontSize = 13, Foreground = textBrush,
                    Margin = new Thickness(0, 12, 0, 4)
                });

                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(190) }); // command
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(56) });  // alias chip
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // summary

                var row = 0;
                foreach (var command in group)
                {
                    grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                    var name = new TextBlock
                    {
                        Text = command.Command,
                        FontFamily = new FontFamily("Consolas"), FontSize = 12, Foreground = textBrush,
                        VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 3, 8, 3)
                    };
                    Grid.SetRow(name, row);
                    Grid.SetColumn(name, 0);
                    grid.Children.Add(name);

                    var chip = new Border
                    {
                        Background = chipBrush, CornerRadius = new CornerRadius(3),
                        Padding = new Thickness(6, 1, 6, 1), Margin = new Thickness(0, 3, 8, 3),
                        HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center,
                        Child = new TextBlock
                        {
                            Text = command.Alias,
                            FontFamily = new FontFamily("Consolas"), FontSize = 11, FontWeight = FontWeights.SemiBold,
                            Foreground = Brushes.White
                        }
                    };
                    Grid.SetRow(chip, row);
                    Grid.SetColumn(chip, 1);
                    grid.Children.Add(chip);

                    var summary = new TextBlock
                    {
                        Text = command.Summary,
                        TextWrapping = TextWrapping.Wrap, FontSize = 11.5, Foreground = mutedBrush,
                        VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 3, 0, 3)
                    };
                    Grid.SetRow(summary, row);
                    Grid.SetColumn(summary, 2);
                    grid.Children.Add(summary);

                    row++;
                }

                list.Children.Add(grid);
            }

            root.Children.Add(new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Content = list
            });

            Content = root;
        }
    }
}
