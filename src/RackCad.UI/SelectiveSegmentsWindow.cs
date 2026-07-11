using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using RackCad.Domain.Systems;

namespace RackCad.UI
{
    /// <summary>
    /// Editor for a frente's "medio frente" tramos: partition the bay into N tramos with N-1 intermediate posts, the
    /// LAST tramo's length CALCULATED (the remainder). Each row is one tramo — a larguero length and a "con largueros"
    /// checkbox (so you can tie one side, the other, or both). Fewer than 2 tramos means a normal full-width bay.
    /// On OK, <see cref="Result"/> holds the edited tramos (empty = full bay). Built entirely in code (no XAML) to
    /// match the editor's code-driven matrix.
    /// </summary>
    public sealed class SelectiveSegmentsWindow : Window
    {
        private readonly List<SelectiveSegment> working;
        private readonly double fullWidth; // resolved full bay length (in); 0 = unknown (no live "resto" hint)
        private readonly StackPanel rowsPanel;
        private readonly TextBlock hint;
        private readonly List<TextBox> lengthBoxes = new List<TextBox>();
        private readonly List<CheckBox> loadedChecks = new List<CheckBox>();

        /// <summary>The edited tramos once the dialog closes with OK. Fewer than 2 = the bay is NOT split (full bay).</summary>
        public IReadOnlyList<SelectiveSegment> Result { get; private set; } = new List<SelectiveSegment>();

        public SelectiveSegmentsWindow(int frenteNumber, IEnumerable<SelectiveSegment> initial, double fullWidth)
        {
            this.fullWidth = fullWidth > 0.0 ? fullWidth : 0.0;
            working = (initial ?? Enumerable.Empty<SelectiveSegment>())
                .Select(s => new SelectiveSegment { Length = s.Length, Loaded = s.Loaded })
                .ToList();

            // Opening on a bay with no tramos yet: seed the classic ½frente (a loaded left tramo + an empty calculated
            // remainder) so the user sees the structure and just fills in a length. "Sin medio frente" backs out.
            if (working.Count == 0)
            {
                working.Add(new SelectiveSegment { Length = 0.0, Loaded = true });
                working.Add(new SelectiveSegment { Length = 0.0, Loaded = false });
            }

            Title = "Tramos del frente " + frenteNumber.ToString(CultureInfo.InvariantCulture);
            Width = 400;
            SizeToContent = SizeToContent.Height;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.NoResize;
            FontFamily = new FontFamily("Segoe UI");

            // Same look as every other RackCad window (this one is code-built, so merge the shared styles by hand).
            Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("/RackCad.UI;component/Themes/AppStyles.xaml", UriKind.Relative) });
            if (TryFindResource("WindowBackgroundBrush") is Brush background) Background = background;

            var root = new StackPanel { Margin = new Thickness(14) };

            root.Children.Add(new TextBlock
            {
                Text = "Parte el frente en tramos con postes intermedios. El ÚLTIMO tramo se calcula (lo que sobra). "
                     + "Marca “con largueros” los tramos que cargan — un lado, el otro, o ambos.",
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 10),
                FontSize = 11.5
            });

            rowsPanel = new StackPanel();
            root.Children.Add(rowsPanel);

            var addBtn = new Button
            {
                Style = TryFindResource("SecondaryButtonStyle") as Style,
                Content = "+ Agregar tramo",
                Margin = new Thickness(0, 8, 0, 0),
                Padding = new Thickness(10, 3, 10, 3),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            addBtn.Click += (s, e) =>
            {
                CommitFromControls();
                working.Add(new SelectiveSegment { Length = 0.0, Loaded = true });
                RenderRows();
            };
            root.Children.Add(addBtn);

            hint = new TextBlock
            {
                FontSize = 11,
                Margin = new Thickness(0, 8, 0, 0),
                TextWrapping = TextWrapping.Wrap,
                Foreground = Brushes.Gray
            };
            root.Children.Add(hint);

            var buttons = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 14, 0, 0)
            };
            var clearBtn = new Button { Style = TryFindResource("SecondaryButtonStyle") as Style, Content = "Sin medio frente", Padding = new Thickness(10, 3, 10, 3), Margin = new Thickness(0, 0, 8, 0) };
            clearBtn.Click += (s, e) =>
            {
                Result = new List<SelectiveSegment>();
                DialogResult = true;
            };
            var okBtn = new Button { Style = TryFindResource("PrimaryButtonStyle") as Style, Content = "Aceptar", Padding = new Thickness(16, 3, 16, 3), IsDefault = true, Margin = new Thickness(0, 0, 8, 0) };
            okBtn.Click += (s, e) => OnOk();
            var cancelBtn = new Button { Style = TryFindResource("SecondaryButtonStyle") as Style, Content = "Cancelar", Padding = new Thickness(10, 3, 10, 3), IsCancel = true };
            buttons.Children.Add(clearBtn);
            buttons.Children.Add(okBtn);
            buttons.Children.Add(cancelBtn);
            root.Children.Add(buttons);

            Content = root;
            RenderRows();
        }

        private void RenderRows()
        {
            rowsPanel.Children.Clear();
            lengthBoxes.Clear();
            loadedChecks.Clear();

            for (var i = 0; i < working.Count; i++)
            {
                var index = i; // capture a per-row copy (the for-loop variable is shared across closures)
                var isLast = index == working.Count - 1;

                var row = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2, 0, 2) };

                row.Children.Add(new TextBlock
                {
                    Text = "Tramo " + (index + 1).ToString(CultureInfo.InvariantCulture),
                    Width = 66,
                    VerticalAlignment = VerticalAlignment.Center
                });

                var lenBox = new TextBox { Width = 80, VerticalAlignment = VerticalAlignment.Center };
                if (isLast)
                {
                    lenBox.IsEnabled = false;
                    lenBox.Text = "(resto)";
                    lenBox.ToolTip = "El último tramo se calcula: lo que sobra del frente tras los tramos anteriores.";
                }
                else
                {
                    lenBox.Text = working[index].Length > 0.0
                        ? working[index].Length.ToString("0.###", CultureInfo.InvariantCulture)
                        : string.Empty;
                    lenBox.ToolTip = "Longitud del larguero de este tramo (in).";
                    lenBox.TextChanged += (s, e) => UpdateHint();
                }

                row.Children.Add(lenBox);
                lengthBoxes.Add(lenBox);

                var chk = new CheckBox
                {
                    Content = "con largueros",
                    IsChecked = working[index].Loaded,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(10, 0, 0, 0)
                };
                row.Children.Add(chk);
                loadedChecks.Add(chk);

                var del = new Button
                {
                    Content = "✕",
                    Width = 24,
                    Margin = new Thickness(10, 0, 0, 0),
                    ToolTip = "Quitar este tramo."
                };
                del.Click += (s, e) =>
                {
                    CommitFromControls();
                    if (index < working.Count) working.RemoveAt(index);
                    RenderRows();
                };
                row.Children.Add(del);

                rowsPanel.Children.Add(row);
            }

            UpdateHint();
        }

        /// <summary>Read the current textbox lengths + checkbox flags back into <see cref="working"/> (before add/remove/OK).</summary>
        private void CommitFromControls()
        {
            for (var i = 0; i < working.Count; i++)
            {
                if (i < loadedChecks.Count)
                {
                    working[i].Loaded = loadedChecks[i].IsChecked == true;
                }

                if (i == working.Count - 1)
                {
                    continue; // the last tramo's length is calculated — ignore its (disabled) box
                }

                if (i < lengthBoxes.Count && UiSupport.TryNum(lengthBoxes[i].Text, out var v) && v > 0.0)
                {
                    working[i].Length = v;
                }
                else if (i < lengthBoxes.Count)
                {
                    working[i].Length = 0.0;
                }
            }
        }

        private void UpdateHint()
        {
            if (working.Count < 2)
            {
                hint.Text = "Agrega al menos 2 tramos para partir el frente (o “Sin medio frente” para dejarlo completo).";
                hint.Foreground = Brushes.Gray;
                return;
            }

            var sum = 0.0;
            for (var i = 0; i < lengthBoxes.Count - 1; i++) // every tramo except the last (calculated)
            {
                if (UiSupport.TryNum(lengthBoxes[i].Text, out var v) && v > 0.0) sum += v;
            }

            if (fullWidth <= 0.0)
            {
                hint.Text = "El último tramo se calcula (lo que sobra del frente).";
                hint.Foreground = Brushes.Gray;
                return;
            }

            var rest = fullWidth - sum; // rough: ignores the little each intermediate post consumes, so the real resto is a bit smaller
            if (rest <= 0.0)
            {
                hint.Text = string.Format(CultureInfo.InvariantCulture,
                    "Los tramos ({0:0.#} in) igualan o superan el frente completo ({1:0.#} in). No cabe.", sum, fullWidth);
                hint.Foreground = Brushes.Firebrick;
            }
            else
            {
                hint.Text = string.Format(CultureInfo.InvariantCulture,
                    "Último tramo (calculado) ≈ {0:0.#} in del frente de {1:0.#} in.", rest, fullWidth);
                hint.Foreground = Brushes.Gray;
            }
        }

        private void OnOk()
        {
            CommitFromControls();

            if (working.Count < 2)
            {
                Result = new List<SelectiveSegment>(); // not a split
                DialogResult = true;
                return;
            }

            for (var i = 0; i < working.Count - 1; i++)
            {
                if (working[i].Length <= 0.0)
                {
                    hint.Text = "El tramo " + (i + 1).ToString(CultureInfo.InvariantCulture) + " necesita una longitud mayor que 0 (el último se calcula solo).";
                    hint.Foreground = Brushes.Firebrick;
                    return;
                }
            }

            if (fullWidth > 0.0)
            {
                var specified = working.Take(working.Count - 1).Sum(s => s.Length);
                if (specified >= fullWidth)
                {
                    hint.Text = string.Format(CultureInfo.InvariantCulture,
                        "Los tramos ({0:0.#} in) no pueden igualar ni superar el frente completo ({1:0.#} in).", specified, fullWidth);
                    hint.Foreground = Brushes.Firebrick;
                    return;
                }
            }

            Result = working.Select(s => new SelectiveSegment { Length = s.Length, Loaded = s.Loaded }).ToList();
            DialogResult = true;
        }
    }
}
