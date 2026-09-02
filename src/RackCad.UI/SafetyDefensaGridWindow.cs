using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using RackCad.Application.Systems.Dynamic;
using RackCad.Domain.Systems.Selective;
using RackCad.UI.Controls;

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

        /// <summary>
        /// I-42 (ronda 7D) — la CARA que esta rejilla edita, cuando edita una sola. NULL es el dialogo historico de
        /// dos extremos, que es el que siguen abriendo el Selectivo, el Dinamico y un Push Back de un solo sentido.
        /// </summary>
        private readonly SafetyDefenseFace face;

        /// <summary>Los registros con los que se abrio: en modo de una cara, la OTRA cara se conserva de aqui.</summary>
        private readonly IReadOnlyList<SafetyPostDefense> incoming;

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
            bool autoPerEnd = false,
            SafetyDefenseFace face = null)
        {
            this.postCount = Math.Max(1, postCount);
            this.lowEndOnly = lowEndOnly;
            this.autoPerEnd = autoPerEnd;
            this.face = face;
            this.incoming = (current ?? Enumerable.Empty<SafetyPostDefense>())
                .Where(value => value != null).ToList();
            var lowLabel = lowEndOnly ? "Entrada/Salida" : "Salida";
            var highLabel = lowEndOnly ? "Posterior" : "Entrada";
            lowEndName = lowLabel.ToLowerInvariant();
            highEndName = highLabel.ToLowerInvariant();
            Title = face == null
                ? "Defensa de montacargas por poste"
                : "Defensa de montacargas por poste — " + face.Label;
            Width = autoPerEnd ? 780 : 670;
            Height = 580;
            MinWidth = 600;
            MinHeight = 360;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            // I-39D: la UNICA de los diez dialogos que no aplicaba el chrome compartido. Abria en blanco liso
            // mientras sus nueve hermanas abrian con el fondo #F4F6F9, y era una omision, no una decision: ni un
            // comentario ni una prueba la respaldaban. Adoptar la fuente comun la corrige, y ese es su unico delta
            // observable, medido: la tipografia ya resolvia a Segoe UI por ser la predeterminada del sistema.
            DialogWindowChrome.Apply(this);

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
                // I-42 (ronda 7D): con una cara declarada la rejilla habla de ESA cara y de nada mas. Sin ella, el
                // texto historico de los dos extremos, palabra por palabra.
                Text = face != null
                    ? "Esta rejilla decide la defensa de " + face.Label
                      + ", poste por poste. La otra cara del rack tiene su propia rejilla y no se toca desde aqui."
                      + " Un poste sin cara de ataque en este lado aparece deshabilitado."
                      + (autoPerEnd
                         ? " Marca «Auto» para que ese poste siga la regla de 12\" en orillas y 36\" en intermedios"
                           + " y se recalcule al agregar o quitar frentes."
                         : string.Empty)
                    : "Cada extremo es independiente: puedes activar " + lowLabel + ", " + highLabel
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

            if (face != null)
            {
                content.Children.Add(BuildFaceTable());
                content.Children.Add(new TextBlock { Height = 0 });
                root.Children.Add(new ScrollViewer
                {
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    Content = content
                });
                Content = root;
                return;
            }

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

                // I-42 (ronda 7C, defecto del dueño) — LA CASILLA ES EL ON/OFF, y tocarla saca a ese extremo del
                // automatico. Antes no: la casilla solo habilitaba el cuadro de longitud, el «Auto» seguia marcado y
                // el resultado se descartaba entero mas abajo, asi que apagar un poste desde la ventana real no
                // cambiaba nada en el rack. Que el «Auto» se desmarque solo deja la decision A LA VISTA en el mismo
                // gesto; quien la escribe es OnOk, que no depende de este manejador.
                exit.Checked += (_, __) =>
                {
                    if (exitAuto != null) { exitAuto.IsChecked = false; }
                    if (!UiSupport.TryNum(exitLength.Text, out var typed) || typed <= 0.0)
                    {
                        // Encender un extremo cuyo automatico es CERO —el lado alto de un Push Back— dejaba un cero
                        // en el cuadro y la unica respuesta posible era el error de validacion. Se propone la
                        // longitud que la regla da para este poste, que es la que el usuario venia a poner.
                        exitLength.Text = SeedLength(defaults).ToString("0.##", CultureInfo.InvariantCulture);
                    }

                    exitLength.IsEnabled = true;
                };
                exit.Unchecked += (_, __) =>
                {
                    if (exitAuto != null) { exitAuto.IsChecked = false; }
                    exitLength.IsEnabled = false;
                };
                entrance.Checked += (_, __) =>
                {
                    if (entranceAuto != null) { entranceAuto.IsChecked = false; }
                    if (!UiSupport.TryNum(entranceLength.Text, out var typed) || typed <= 0.0)
                    {
                        entranceLength.Text = SeedLength(defaults).ToString("0.##", CultureInfo.InvariantCulture);
                    }

                    entranceLength.IsEnabled = true;
                };
                entrance.Unchecked += (_, __) =>
                {
                    if (entranceAuto != null) { entranceAuto.IsChecked = false; }
                    entranceLength.IsEnabled = false;
                };

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

                rows.Add(new Row(postIndex, exit, exitLength, entrance, entranceLength, exitAuto, entranceAuto,
                    setting.DrawsExit, setting.DrawsEntrance));
            }

            content.Children.Add(table);
            root.Children.Add(new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Content = content
            });
            Content = root;
        }

        /// <summary>
        /// La longitud que se propone al ENCENDER un extremo que estaba apagado: la que la regla da para ese poste
        /// (12" en orilla, 36" en intermedio). En el extremo alto de un Push Back el automatico es cero —esa es la
        /// regla PB-009—, asi que se toma la del extremo bajo, que es la misma regla por poste.
        /// </summary>
        private static double SeedLength(DynamicForkliftDefenseSetting defaults)
            => defaults.ExitLength > 0.0
                ? defaults.ExitLength
                : (defaults.EntranceLength > 0.0 ? defaults.EntranceLength : DynamicForkliftDefensePlan.EdgeLength);

        /// <summary>
        /// I-42 (ronda 7D) — la rejilla de UNA cara: una fila por linea transversal y una sola pareja de columnas,
        /// la de esta cara. Un poste donde la cara no existe se muestra deshabilitado y dice por que, en vez de
        /// ofrecer una casilla que no puede materializar nada.
        /// </summary>
        private Grid BuildFaceTable()
        {
            var table = new Grid();
            table.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            table.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
            table.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(115) });
            if (autoPerEnd)
            {
                table.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(52) });
            }

            AddHeader(table, "Poste", 0);
            AddHeader(table, face.Label, 1);
            AddHeader(table, "Longitud", 2);
            if (autoPerEnd)
            {
                AddHeader(table, "Auto", 3);
            }

            for (var postIndex = 0; postIndex < postCount; postIndex++)
            {
                table.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                var applies = face.AppliesAt(postIndex);
                var stored = incoming.FirstOrDefault(value => value.PostIndex == postIndex);

                // La cara lejana solo tiene longitud automatica donde ES cara de carga: se resuelve con la misma
                // aplicabilidad que declara la seccion, que es la que usa el dibujo.
                var setting = DynamicForkliftDefensePlan.At(
                    incoming, postIndex, postCount, lowEndOnly, secondLoadFace: applies);
                var defaults = DynamicForkliftDefensePlan.At(
                    null, postIndex, postCount, lowEndOnly, secondLoadFace: applies);
                var drawn = face.IsFarEnd ? setting.DrawsEntrance : setting.DrawsExit;
                var length = face.IsFarEnd ? setting.EntranceLength : setting.ExitLength;
                var defaultLength = face.IsFarEnd ? defaults.EntranceLength : defaults.ExitLength;
                if (defaultLength <= 0.0)
                {
                    defaultLength = defaults.ExitLength > 0.0
                        ? defaults.ExitLength
                        : DynamicForkliftDefensePlan.EdgeLength;
                }

                var autoNow = stored == null
                    || (face.IsFarEnd ? stored.EntranceAuto : stored.ExitAuto);

                var label = new TextBlock
                {
                    Text = "Poste " + (postIndex + 1).ToString(CultureInfo.InvariantCulture)
                           + (postIndex == 0 || postIndex == postCount - 1 ? " (orilla)" : " (intermedio)")
                           + (applies ? string.Empty : " — sin cara de ataque en este lado"),
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(2, 5, 8, 5)
                };

                var check = new CheckBox
                {
                    IsChecked = applies && drawn,
                    IsEnabled = applies,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                var lengthBox = new TextBox
                {
                    Text = (drawn && length > 0.0 ? length : defaultLength)
                        .ToString("0.##", CultureInfo.InvariantCulture),
                    Margin = new Thickness(2, 3, 8, 3),
                    IsEnabled = applies && check.IsChecked == true && !autoNow
                };
                CheckBox auto = null;
                if (autoPerEnd)
                {
                    auto = new CheckBox
                    {
                        IsChecked = autoNow,
                        IsEnabled = applies,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        ToolTip = AutoTooltip
                    };
                    auto.Checked += (_, __) =>
                    {
                        lengthBox.IsEnabled = false;
                        lengthBox.Text = defaultLength.ToString("0.##", CultureInfo.InvariantCulture);
                    };
                    auto.Unchecked += (_, __) => lengthBox.IsEnabled = check.IsChecked == true;
                }

                // La CASILLA es el ON/OFF y saca a este poste del automatico, igual que en el dialogo de dos
                // extremos desde la ronda 7C.
                check.Checked += (_, __) =>
                {
                    if (auto != null) { auto.IsChecked = false; }
                    if (!UiSupport.TryNum(lengthBox.Text, out var typed) || typed <= 0.0)
                    {
                        lengthBox.Text = defaultLength.ToString("0.##", CultureInfo.InvariantCulture);
                    }

                    lengthBox.IsEnabled = true;
                };
                check.Unchecked += (_, __) =>
                {
                    if (auto != null) { auto.IsChecked = false; }
                    lengthBox.IsEnabled = false;
                };

                Grid.SetRow(label, postIndex + 1);
                Grid.SetColumn(label, 0);
                Grid.SetRow(check, postIndex + 1);
                Grid.SetColumn(check, 1);
                Grid.SetRow(lengthBox, postIndex + 1);
                Grid.SetColumn(lengthBox, 2);
                table.Children.Add(label);
                table.Children.Add(check);
                table.Children.Add(lengthBox);
                if (auto != null)
                {
                    Grid.SetRow(auto, postIndex + 1);
                    Grid.SetColumn(auto, 3);
                    table.Children.Add(auto);
                }

                rows.Add(new Row(postIndex, check, lengthBox, null, null, auto, null, applies && drawn, false));
            }

            return table;
        }

        /// <summary>El OK del modo de UNA cara: escribe SOLO su extremo y conserva el otro tal cual.</summary>
        private void BuildFaceResult()
        {
            var result = CopyRecords(incoming);

            // Una linea que ya no existe no deja fantasma: el dialogo historico tampoco la conserva, porque
            // reconstruye su resultado desde las filas que tiene. La identidad de las que SIGUEN existiendo no se
            // toca — no se compacta ningun indice.
            result.RemoveAll(record => record.PostIndex < 0 || record.PostIndex >= postCount);

            foreach (var row in rows)
            {
                if (row.Exit.IsEnabled == false)
                {
                    continue;   // sin cara de ataque: esta linea no decide nada en este lado
                }

                var auto = row.ExitAuto?.IsChecked == true && (row.Exit.IsChecked == true) == row.ExitWasDrawn;
                var on = row.Exit.IsChecked == true;
                var length = 0.0;
                if (!auto && on && (!UiSupport.TryNum(row.ExitLength.Text, out length) || length <= 0.0))
                {
                    error.Text = "La longitud de " + face.Label.ToLowerInvariant() + " del poste "
                                 + (row.PostIndex + 1).ToString(CultureInfo.InvariantCulture)
                                 + " debe ser mayor que cero.";
                    return;
                }

                var record = result.FirstOrDefault(value => value.PostIndex == row.PostIndex);
                if (record == null)
                {
                    record = new SafetyPostDefense
                    {
                        PostIndex = row.PostIndex,
                        ExitAuto = true,
                        EntranceAuto = true
                    };
                    result.Add(record);
                }

                SetEnd(record, face.IsFarEnd, auto ? 0.0 : (on ? length : 0.0), auto);
            }

            result.RemoveAll(record => record.ExitAuto && record.EntranceAuto);
            Result = result.OrderBy(record => record.PostIndex).ToList();
            Accepted = true;
        }

        /// <summary>True cuando el OK termino sin diagnostico (seam de prueba del modo de una cara).</summary>
        internal bool Accepted { get; private set; }

        /// <summary>Copia de los registros por poste: el resultado nunca comparte objetos con la entrada.</summary>
        private static List<SafetyPostDefense> CopyRecords(IEnumerable<SafetyPostDefense> records)
            => (records ?? Enumerable.Empty<SafetyPostDefense>())
                .Where(record => record != null)
                .Select(record => new SafetyPostDefense
                {
                    PostIndex = record.PostIndex,
                    ExitLength = record.ExitLength,
                    EntranceLength = record.EntranceLength,
                    ExitAuto = record.ExitAuto,
                    EntranceAuto = record.EntranceAuto
                })
                .ToList();

        /// <summary>Escribe UN extremo del registro y deja el otro exactamente como estaba.</summary>
        private static void SetEnd(SafetyPostDefense record, bool farEnd, double length, bool auto)
        {
            if (farEnd)
            {
                record.EntranceLength = length;
                record.EntranceAuto = auto;
            }
            else
            {
                record.ExitLength = length;
                record.ExitAuto = auto;
            }
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

        /// <summary>
        /// I-42 (ronda 7C) — la casilla ON/OFF del extremo BAJO de un poste (seam de prueba). La rejilla se
        /// construye en codigo y sus controles no llevan x:Name, asi que una prueba que quiera hacer EL GESTO DEL
        /// USUARIO —marcar o desmarcar esa casilla, con sus manejadores reales— necesita llegar a ella.
        /// </summary>
        internal CheckBox ExitCheckForTest(int postIndex) => rows[postIndex].Exit;

        /// <summary>La casilla ON/OFF del extremo ALTO de un poste (seam de prueba).</summary>
        internal CheckBox EntranceCheckForTest(int postIndex) => rows[postIndex].Entrance;

        /// <summary>La casilla «Auto» del extremo BAJO, o null cuando la rejilla no las ofrece (seam de prueba).</summary>
        internal CheckBox ExitAutoForTest(int postIndex) => rows[postIndex].ExitAuto;

        /// <summary>La casilla «Auto» del extremo ALTO, o null cuando la rejilla no las ofrece (seam de prueba).</summary>
        internal CheckBox EntranceAutoForTest(int postIndex) => rows[postIndex].EntranceAuto;

        private void OnOk(object sender, RoutedEventArgs e)
        {
            // I-39D: el diagnostico se limpia al REVALIDAR. Antes se escribia y no se borraba nunca, asi que un
            // aviso corregido seguia en pantalla acusando a un poste que ya estaba bien.
            error.Text = string.Empty;
            Accepted = false;

            if (face != null)
            {
                BuildFaceResult();
                if (Accepted && e != null)
                {
                    DialogResult = true;
                }

                return;
            }

            var result = new List<SafetyPostDefense>();
            foreach (var row in rows)
            {
                var defaultSetting = DynamicForkliftDefensePlan.At(null, row.PostIndex, postCount, lowEndOnly);
                // I-42 (ronda 7C) — un extremo cuya CASILLA cambio respecto de lo que la fila mostro ya no es
                // automatico, marque lo que marque «Auto»: el usuario acaba de decidir ese extremo. Sin esto, una
                // fila automatica cuyo ON/OFF se apagaba seguia siendo «todo automatico», se descartaba aqui mismo
                // y el rack no cambiaba — que es exactamente el defecto que el dueño reporto.
                var exitToggled = (row.Exit.IsChecked == true) != row.ExitWasDrawn;
                var entranceToggled = (row.Entrance.IsChecked == true) != row.EntranceWasDrawn;
                var exitAuto = row.ExitAuto?.IsChecked == true && !exitToggled;
                var entranceAuto = row.EntranceAuto?.IsChecked == true && !entranceToggled;
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
            Accepted = true;
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
                CheckBox entranceAuto = null,
                bool exitWasDrawn = false,
                bool entranceWasDrawn = false)
            {
                ExitWasDrawn = exitWasDrawn;
                EntranceWasDrawn = entranceWasDrawn;
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

            /// <summary>Lo que la fila MOSTRO al abrirse. Comparar contra esto es lo que distingue «el usuario tocó
            /// la casilla» de «la fila esta como la pinto la regla», sin adivinar por el numero.</summary>
            public bool ExitWasDrawn { get; }

            public bool EntranceWasDrawn { get; }
        }
    }
}
