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

        /// <summary>PB-009 (I-32): the far end carries no automatic length in this system (see the constructor).</summary>
        private readonly bool lowEndOnly;

        /// <summary>PB-010 (I-32): each end can follow the automatic 12"/36" rule instead of a stored length.</summary>
        private readonly bool autoPerEnd;

        /// <summary>The two end names the error messages use, matching the column headers.</summary>
        private readonly string lowEndName;
        private readonly string highEndName;

        private const string AutoTooltip =
            "Sigue la regla (12\" en poste de orilla, 36\" en intermedio) y se recalcula al agregar o quitar frentes.";

        private const string RearAutoTooltip =
            "Automático en este extremo significa apagado: Push Back no lleva defensa atrás salvo que fijes una longitud.";

        public IReadOnlyList<SafetyPostDefense> Result { get; private set; } = new List<SafetyPostDefense>();

        /// <param name="lowEndOnly">
        /// PB-008 / PB-009 (I-32). FALSE (Selectivo, Dinámico) is the historical dialog: two symmetric ends named
        /// "Salida" and "Entrada", both with the automatic 12"/36".
        ///
        /// TRUE (Push Back) renames them to the ends that system actually has — the low end is "Entrada/Salida"
        /// (loading and unloading share it, because Push Back is LIFO) and the far one is "Posterior" — and turns the
        /// far end OFF by default: it gets no automatic length, so nothing is drawn there unless the user asks for it.
        /// </param>
        /// <param name="autoPerEnd">
        /// PB-010 (I-32). FALSE keeps the historical behaviour, where a stored record freezes both lengths and a post
        /// that was an edge keeps its 12" after the rack grows and it becomes an intermediate.
        ///
        /// TRUE offers an "Auto" box per end: an automatic end follows the 12"/36" rule and is RECOMPUTED whenever the
        /// post count changes; clearing it turns that end into an override the user owns.
        /// </param>
        public SafetyDefensaGridWindow(
            string elementLabel,
            int postCount,
            IEnumerable<SafetyPostDefense> current,
            bool lowEndOnly = false,
            bool autoPerEnd = false)
        {
            this.postCount = Math.Max(1, postCount);
            this.lowEndOnly = lowEndOnly;
            this.autoPerEnd = autoPerEnd;
            var lowLabel = lowEndOnly ? "Entrada/Salida" : "Salida";
            var highLabel = lowEndOnly ? "Posterior" : "Entrada";
            lowEndName = lowLabel.ToLowerInvariant();
            highEndName = highLabel.ToLowerInvariant();
            Title = "Defensa de montacargas por poste";
            Width = autoPerEnd ? 780 : 670;
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
                Text = "Cada extremo es independiente: puedes activar " + lowLabel + ", " + highLabel
                       + ", ambos o ninguno y asignar una LONGITUD distinta a cada lado."
                       + (lowEndOnly
                          ? " El automático de 12\" en orillas y 36\" en postes intermedios aplica solo a "
                            + lowLabel + "; el lado " + highLabel + " viene apagado."
                          : " Los valores predeterminados son 12\" por lado en orillas y 36\" por lado en postes intermedios.")
                       + (autoPerEnd
                          ? " Marca «Auto» para que ese extremo siga la regla y se recalcule al agregar o quitar frentes."
                          : string.Empty),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 3, 0, 10)
            });

            var table = new Grid();
            table.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            table.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(70) });
            table.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(105) });
            if (autoPerEnd) table.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(52) });
            table.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(70) });
            table.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(105) });
            if (autoPerEnd) table.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(52) });

            // Column layout: Poste | <low> | Long. <low> | [Auto] | <high> | Long. <high> | [Auto]
            var colExit = 1;
            var colExitLength = 2;
            var colExitAuto = autoPerEnd ? 3 : -1;
            var colEntrance = autoPerEnd ? 4 : 3;
            var colEntranceLength = autoPerEnd ? 5 : 4;
            var colEntranceAuto = autoPerEnd ? 6 : -1;
            AddHeader(table, "Poste", 0);
            AddHeader(table, lowLabel, colExit);
            AddHeader(table, "Long. " + lowLabel.ToLowerInvariant(), colExitLength);
            if (autoPerEnd) AddHeader(table, "Auto", colExitAuto);
            AddHeader(table, highLabel, colEntrance);
            AddHeader(table, "Long. " + highLabel.ToLowerInvariant(), colEntranceLength);
            if (autoPerEnd) AddHeader(table, "Auto", colEntranceAuto);

            var source = current?.Where(value => value != null).ToList() ?? new List<SafetyPostDefense>();
            for (var postIndex = 0; postIndex < this.postCount; postIndex++)
            {
                table.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                var setting = DynamicForkliftDefensePlan.At(source, postIndex, this.postCount, lowEndOnly);
                var defaults = DynamicForkliftDefensePlan.At(null, postIndex, this.postCount, lowEndOnly);
                var stored = source.FirstOrDefault(value => value.PostIndex == postIndex);
                // With no stored record the post is fully automatic at BOTH ends — which is exactly what "no record"
                // means to the plan. In a low-end-only system "automatic" at the far end resolves to 0, so the box
                // starts unchecked there while still being automatic.
                var exitAutoNow = stored == null || stored.ExitAuto;
                var entranceAutoNow = stored == null || stored.EntranceAuto;
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
                CheckBox exitAuto = null;
                CheckBox entranceAuto = null;
                if (autoPerEnd)
                {
                    // An AUTO end shows the value the rule produces and is not typed into: clearing the box is what
                    // turns that end into an override the user owns.
                    exitAuto = new CheckBox
                    {
                        IsChecked = exitAutoNow,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        ToolTip = AutoTooltip
                    };
                    entranceAuto = new CheckBox
                    {
                        IsChecked = entranceAutoNow,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        ToolTip = lowEndOnly ? RearAutoTooltip : AutoTooltip
                    };
                    exitLength.IsEnabled = exit.IsChecked == true && !exitAutoNow;
                    entranceLength.IsEnabled = entrance.IsChecked == true && !entranceAutoNow;
                    exitAuto.Checked += (_, __) =>
                    {
                        exitLength.IsEnabled = false;
                        exitLength.Text = defaults.ExitLength.ToString("0.##", CultureInfo.InvariantCulture);
                    };
                    exitAuto.Unchecked += (_, __) => exitLength.IsEnabled = exit.IsChecked == true;
                    entranceAuto.Checked += (_, __) =>
                    {
                        entranceLength.IsEnabled = false;
                        entranceLength.Text = defaults.EntranceLength.ToString("0.##", CultureInfo.InvariantCulture);
                    };
                    entranceAuto.Unchecked += (_, __) => entranceLength.IsEnabled = entrance.IsChecked == true;
                }

                exit.Checked += (_, __) => exitLength.IsEnabled = exitAuto?.IsChecked != true;
                exit.Unchecked += (_, __) => exitLength.IsEnabled = false;
                entrance.Checked += (_, __) => entranceLength.IsEnabled = entranceAuto?.IsChecked != true;
                entrance.Unchecked += (_, __) => entranceLength.IsEnabled = false;

                Grid.SetRow(label, postIndex + 1);
                Grid.SetColumn(label, 0);
                Grid.SetRow(exit, postIndex + 1);
                Grid.SetColumn(exit, colExit);
                Grid.SetRow(exitLength, postIndex + 1);
                Grid.SetColumn(exitLength, colExitLength);
                Grid.SetRow(entrance, postIndex + 1);
                Grid.SetColumn(entrance, colEntrance);
                Grid.SetRow(entranceLength, postIndex + 1);
                Grid.SetColumn(entranceLength, colEntranceLength);
                table.Children.Add(label);
                table.Children.Add(exit);
                table.Children.Add(exitLength);
                table.Children.Add(entrance);
                table.Children.Add(entranceLength);
                if (autoPerEnd)
                {
                    Grid.SetRow(exitAuto, postIndex + 1);
                    Grid.SetColumn(exitAuto, colExitAuto);
                    Grid.SetRow(entranceAuto, postIndex + 1);
                    Grid.SetColumn(entranceAuto, colEntranceAuto);
                    table.Children.Add(exitAuto);
                    table.Children.Add(entranceAuto);
                }

                rows.Add(new Row(postIndex, exit, exitLength, entrance, entranceLength, exitAuto, entranceAuto));
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

        /// <summary>Runs the real OK path without a modal window (test seam; the tests cannot set DialogResult).</summary>
        internal void BuildResultForTest() => OnOk(this, null);

        private void OnOk(object sender, RoutedEventArgs e)
        {
            var result = new List<SafetyPostDefense>();
            foreach (var row in rows)
            {
                var defaultSetting = DynamicForkliftDefensePlan.At(null, row.PostIndex, postCount, lowEndOnly);
                var exitAuto = row.ExitAuto?.IsChecked == true;
                var entranceAuto = row.EntranceAuto?.IsChecked == true;
                if (exitAuto && entranceAuto)
                {
                    continue;   // fully automatic == no record; the plan computes both ends from the current rack
                }

                var exitLength = 0.0;
                if (!exitAuto && row.Exit.IsChecked == true
                    && (!UiSupport.TryNum(row.ExitLength.Text, out exitLength) || exitLength <= 0.0))
                {
                    error.Text = "La longitud de " + lowEndName + " del poste "
                                 + (row.PostIndex + 1).ToString(CultureInfo.InvariantCulture)
                                 + " debe ser mayor que cero.";
                    return;
                }

                var entranceLength = 0.0;
                if (!entranceAuto && row.Entrance.IsChecked == true
                    && (!UiSupport.TryNum(row.EntranceLength.Text, out entranceLength) || entranceLength <= 0.0))
                {
                    error.Text = "La longitud de " + highEndName + " del poste "
                                 + (row.PostIndex + 1).ToString(CultureInfo.InvariantCulture)
                                 + " debe ser mayor que cero.";
                    return;
                }

                // PB-010 (I-32) — WHEN a record is stored.
                //
                // With the Auto boxes the answer is explicit and never inferred: both ends automatic was already
                // handled above (no record, so the plan computes them), and anything else means at least one end is
                // the user's, so it is ALWAYS stored — even when today's number happens to equal today's automatic
                // one. Comparing numbers to guess provenance is exactly what dropped a manual 12" on an edge post:
                // the edge default is also 12", so the dialog concluded "this is the default" and threw the record
                // away; the moment the rack grew, that post became intermediate and the length silently became 36".
                //
                // Without the Auto boxes (Selectivo/Dinámico) there is nothing to read the provenance from, so the
                // historical heuristic stays untouched: a row equal to its default stores nothing.
                var stores = autoPerEnd
                    || Math.Abs(exitLength - defaultSetting.ExitLength) > 1e-6
                    || Math.Abs(entranceLength - defaultSetting.EntranceLength) > 1e-6;
                if (stores)
                {
                    // An AUTO end stores no length (the plan computes it); the other end is an explicit override, and
                    // 0 is the explicit "no defence at this end".
                    result.Add(new SafetyPostDefense
                    {
                        PostIndex = row.PostIndex,
                        ExitLength = exitAuto ? 0.0 : exitLength,
                        EntranceLength = entranceAuto ? 0.0 : entranceLength,
                        ExitAuto = exitAuto,
                        EntranceAuto = entranceAuto
                    });
                }
            }

            Result = result;
            if (e != null)
            {
                DialogResult = true;
            }
        }

        private sealed class Row
        {
            public Row(
                int postIndex,
                CheckBox exit,
                TextBox exitLength,
                CheckBox entrance,
                TextBox entranceLength,
                CheckBox exitAuto = null,
                CheckBox entranceAuto = null)
            {
                PostIndex = postIndex;
                Exit = exit;
                ExitLength = exitLength;
                Entrance = entrance;
                EntranceLength = entranceLength;
                ExitAuto = exitAuto;
                EntranceAuto = entranceAuto;
            }

            public int PostIndex { get; }
            public CheckBox Exit { get; }
            public TextBox ExitLength { get; }
            public CheckBox Entrance { get; }
            public TextBox EntranceLength { get; }
            public CheckBox ExitAuto { get; }
            public CheckBox EntranceAuto { get; }
        }
    }
}
