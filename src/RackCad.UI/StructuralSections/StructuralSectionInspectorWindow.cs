using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using RackCad.Application.StructuralSections;
using RackCad.Application.StructuralSections.Geometry;
using RackCad.UI.Controls;
using RackCad.UI.Shell;

namespace RackCad.UI.StructuralSections
{
    /// <summary>
    /// The minimum surface that lets a person SEE a catalogued section: pick one, give it a length, choose a
    /// view and check it (owner decision 22).
    ///
    /// It is NOT a member configurator and must not become one (decision 23). There is no role, no material,
    /// no loads, no levels, no arms, no connectors, no fabrication, no saving to a library, no persistence and
    /// no round-trip editing. Every one of those belongs to I-37.
    ///
    /// Built in code rather than XAML, like the other dialogs of the repository, and thin on purpose: all the
    /// logic lives in <see cref="StructuralSectionInspectorState"/>, which is testable without a dispatcher.
    /// </summary>
    public sealed class StructuralSectionInspectorWindow : Window
    {
        private readonly StructuralSectionInspectorState _state;
        private readonly ListBox _sections;
        private readonly TextBox _search;
        private readonly ComboBox _family;
        private readonly TextBox _length;
        private readonly ComboBox _view;
        private readonly ComboBox _detail;
        private readonly ComboBox _mode;
        private readonly TextBox _rotation;
        private readonly CheckBox _mirror;
        private readonly CheckBox _axis;
        private readonly CheckBox _envelope;
        private readonly StructuralSectionPreview _preview;
        private readonly TextBlock _summary;
        private readonly TextBlock _fidelity;
        private readonly TextBlock _authority;
        private readonly TextBlock _diagnostics;
        private Button _accept;
        private bool _loaded;

        public StructuralSectionInspectorWindow(StructuralSectionCatalog catalog)
        {
            _state = new StructuralSectionInspectorState(catalog);

            Title = "Secciones estructurales";

            // I-39C: el tamano sale de los tokens del ARQUETIPO B, no de cuatro numeros repetidos aqui. Una ventana
            // construida en codigo no aplica el estilo de ventana por DynamicResource como hacen las de XAML, asi que
            // lee los mismos tokens del diccionario compartido. Los valores son los que esta ventana ya usaba —de
            // ella salieron cuando I-39A escribio el contrato—, de modo que su tamano observable NO cambia.
            Width = ShellResources.Get("BoundedEditorInitialWidth", 1040.0);
            Height = ShellResources.Get("BoundedEditorInitialHeight", 680.0);
            MinWidth = ShellResources.Get("BoundedEditorMinWidth", 820.0);
            MinHeight = ShellResources.Get("BoundedEditorMinHeight", 520.0);
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            _search = new TextBox { Margin = new Thickness(0, 2, 0, 8) };
            _search.TextChanged += (_, __) => OnFilterChanged();

            _family = new ComboBox { Margin = new Thickness(0, 2, 0, 8) };
            _family.Items.Add(new ComboBoxItem { Content = "Todas las familias", Tag = null });

            foreach (var family in StructuralSectionFamilies.All)
            {
                _family.Items.Add(new ComboBoxItem
                {
                    Content = FamilyLabel(family),
                    Tag = family
                });
            }

            _family.SelectedIndex = 0;
            _family.SelectionChanged += (_, __) => OnFilterChanged();

            _sections = new ListBox { Height = 260, Margin = new Thickness(0, 2, 0, 8) };
            _sections.SelectionChanged += (_, __) => OnSelectionChanged();

            _length = new TextBox
            {
                Text = StructuralSectionInspectorState.DefaultLengthInches.ToString(CultureInfo.CurrentCulture),
                Margin = new Thickness(0, 2, 0, 8)
            };
            _length.TextChanged += (_, __) => OnLengthChanged();

            _view = EnumCombo(new[]
            {
                ("Seccion", SectionViewKind.CrossSection),
                ("Longitudinal X", SectionViewKind.LongitudinalX),
                ("Longitudinal Y", SectionViewKind.LongitudinalY),
                ("Isometrica", SectionViewKind.Isometric)
            });
            _view.SelectionChanged += (_, __) => { _state.View = Selected<SectionViewKind>(_view); Refresh(); };

            _detail = EnumCombo(new[]
            {
                ("Tabulada", SectionDetailLevel.Tabulated),
                ("Simplificada", SectionDetailLevel.Simplified)
            });
            _detail.SelectionChanged += (_, __) => { _state.Detail = Selected<SectionDetailLevel>(_detail); Refresh(); };

            _mode = EnumCombo(new[]
            {
                ("Wireframe", SectionRepresentationMode.Wireframe),
                ("Envolvente", SectionRepresentationMode.Envelope),
                ("Eje", SectionRepresentationMode.Axis)
            });
            _mode.SelectionChanged += (_, __) => { _state.Mode = Selected<SectionRepresentationMode>(_mode); Refresh(); };

            _rotation = new TextBox { Text = "0", Margin = new Thickness(0, 2, 0, 8) };
            _rotation.TextChanged += (_, __) => OnRotationChanged();

            _mirror = new CheckBox { Content = "Espejo", Margin = new Thickness(0, 2, 0, 4) };
            _mirror.Checked += (_, __) => { _state.Mirrored = true; Refresh(); };
            _mirror.Unchecked += (_, __) => { _state.Mirrored = false; Refresh(); };

            _axis = new CheckBox { Content = "Mostrar eje", Margin = new Thickness(0, 2, 0, 4) };
            _axis.Checked += (_, __) => { _state.ShowAxis = true; Refresh(); };
            _axis.Unchecked += (_, __) => { _state.ShowAxis = false; Refresh(); };

            _envelope = new CheckBox { Content = "Mostrar envolvente", Margin = new Thickness(0, 2, 0, 8) };
            _envelope.Checked += (_, __) => { _state.ShowEnvelope = true; Refresh(); };
            _envelope.Unchecked += (_, __) => { _state.ShowEnvelope = false; Refresh(); };

            _preview = new StructuralSectionPreview { Background = Brushes.Transparent, MinHeight = 320 };
            _summary = new TextBlock { TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 8, 0, 2) };
            _fidelity = new TextBlock { TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 2, 0, 2) };
            // The visual-derived warning is a first-class element, not a diagnostic bullet: it is the one
            // statement a user must not miss (ADR-0023 decision 22).
            _authority = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 2, 0, 2),
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Firebrick,
                Visibility = Visibility.Collapsed
            };
            _diagnostics = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 2, 0, 8),
                Foreground = PreviewPalette.Muted
            };

            Content = BuildLayout();

            // ADR-0029 D9, I-39C: el foco inicial es DETERMINISTA y declarado, y no recae en una accion destructiva
            // ni bloqueada. Sin declararlo, el foco caia donde el arbol visual del shell lo pusiera —y la plantilla
            // acopla la barra de acciones ARRIBA en el DockPanel, antes de la zona de parametros—. Aqui apunta a la
            // caja de busqueda, que es por donde empieza el trabajo de esta ventana.
            System.Windows.Input.FocusManager.SetFocusedElement(this, _search);

            Loaded += (_, __) => { _loaded = true; RefreshList(); };
        }

        /// <summary>What the caller materialises when the user accepts. Null when they cancelled.</summary>
        public StructuralSectionRepresentationPlan Result { get; private set; }

        /// <summary>The section the user accepted, for reporting.</summary>
        public StructuralSectionDefinition AcceptedSection { get; private set; }

        /// <summary>The inspector's state, exposed for tests and for a caller that wants to preset it.</summary>
        public StructuralSectionInspectorState State => _state;

        internal static string FamilyLabel(StructuralSectionFamily family)
        {
            switch (family)
            {
                case StructuralSectionFamily.W: return "W — ala ancha";
                case StructuralSectionFamily.HssRectangular: return "HSS rectangular y cuadrado";
                case StructuralSectionFamily.Channel: return "C — canal";
                case StructuralSectionFamily.Angle: return "L — angulo";
                case StructuralSectionFamily.S: return "S / IPS — viga estandar americana";
                default: return family.ToString();
            }
        }

        private UIElement BuildLayout()
        {
            var left = new StackPanel { Margin = new Thickness(12) };
            left.Children.Add(Label("Buscar designacion"));
            left.Children.Add(_search);
            left.Children.Add(Label("Familia"));
            left.Children.Add(_family);
            left.Children.Add(Label("Seccion"));
            left.Children.Add(_sections);
            left.Children.Add(Label("Longitud (pulgadas)"));
            left.Children.Add(_length);
            left.Children.Add(Label("Vista"));
            left.Children.Add(_view);
            left.Children.Add(Label("Detalle"));
            left.Children.Add(_detail);
            left.Children.Add(Label("Representacion"));
            left.Children.Add(_mode);
            left.Children.Add(Label("Rotacion (grados)"));
            left.Children.Add(_rotation);
            left.Children.Add(_mirror);
            left.Children.Add(_axis);
            left.Children.Add(_envelope);

            var info = new StackPanel();
            info.Children.Add(_summary);
            info.Children.Add(_fidelity);
            info.Children.Add(_authority);
            info.Children.Add(_diagnostics);

            var actions = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            // I-39C: primer consumidor productivo de EditorAction. Antes esta ventana armaba sus dos botones a mano
            // porque la fabrica comun no sabia declarar accion por defecto ni de cancelacion, y sustituirlos habria
            // borrado Enter y Escape en silencio. Ahora la descripcion lleva el rol de teclado, asi que el contrato
            // que la caracterizacion de I-39A fijo se conserva y ademas los botones dejan de tener chrome propio:
            // usan los estilos compartidos, como los de las otras cinco ventanas del arquetipo.
            _accept = EditorActions.Button(
                new EditorAction("Insertar", isPrimary: true, isDefault: true),
                (_, __) => Accept());

            var cancel = EditorActions.Button(
                new EditorAction("Cerrar", isCancel: true),
                (_, __) => { Result = null; DialogResult = false; });

            _accept.MinWidth = 110;
            cancel.MinWidth = 110;

            actions.Children.Add(_accept);
            actions.Children.Add(cancel);

            // I-39A: the inspector is the first non-Cantilever consumer of the bounded-editor shell (archetype B of
            // ADR-0029). Its own controls are hosted AS THEY ARE — same instances, same handlers, same order — so
            // nothing about search, filtering, families, defaults, geometry authority or the output contract moves.
            //
            // Three zones stay empty on purpose. `Header`: the inspector never had one. `SectionPicker`: its search
            // and its list are part of the capture panel, and splitting them out would invert the visual order the
            // characterization pins. `BomSummary`: the inspector produces no BOM and must not start producing one
            // (owner decision 23 of I-36D).
            //
            // What the shell changes is composition, and that IS the point: diagnostics and actions leave the
            // preview column and become bands that span the window, always outside the parameters scroll, so they
            // stay reachable however long the capture panel grows.
            return new RackBoundedEditorShell
            {
                Parameters = left,
                Preview = _preview,
                Diagnostics = info,
                Actions = actions
            };
        }

        private static TextBlock Label(string text) =>
            new TextBlock { Text = text, Margin = new Thickness(0, 6, 0, 0), FontWeight = FontWeights.SemiBold };

        private static ComboBox EnumCombo<T>((string Text, T Value)[] options) where T : struct
        {
            var combo = new ComboBox { Margin = new Thickness(0, 2, 0, 8) };

            foreach (var option in options)
            {
                combo.Items.Add(new ComboBoxItem { Content = option.Text, Tag = option.Value });
            }

            combo.SelectedIndex = 0;
            return combo;
        }

        private static T Selected<T>(ComboBox combo) where T : struct =>
            (T)((ComboBoxItem)combo.SelectedItem).Tag;

        private void OnFilterChanged()
        {
            if (!_loaded)
            {
                return;
            }

            _state.Search = _search.Text;
            _state.Family = (StructuralSectionFamily?)((ComboBoxItem)_family.SelectedItem)?.Tag;
            RefreshList();
        }

        private void RefreshList()
        {
            _state.EnsureSelectionIsVisible();
            _sections.Items.Clear();

            foreach (var section in _state.Matches())
            {
                _sections.Items.Add(new ListBoxItem { Content = section.DisplayName, Tag = section });
            }

            var index = _state.Matches().ToList().IndexOf(_state.Selected);
            _sections.SelectedIndex = index >= 0 ? index : (_sections.Items.Count > 0 ? 0 : -1);
            Refresh();
        }

        private void OnSelectionChanged()
        {
            if (_sections.SelectedItem is ListBoxItem item && item.Tag is StructuralSectionDefinition section)
            {
                _state.Selected = section;
            }

            Refresh();
        }

        private void OnLengthChanged()
        {
            var valid = _state.TrySetLength(_length.Text);
            _length.Foreground = valid ? SystemColors.ControlTextBrush : PreviewPalette.Warning;
            Refresh();
        }

        private void OnRotationChanged()
        {
            if (double.TryParse(_rotation.Text, NumberStyles.Float, CultureInfo.CurrentCulture, out var degrees) ||
                double.TryParse(_rotation.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out degrees))
            {
                if (!double.IsNaN(degrees) && !double.IsInfinity(degrees))
                {
                    _state.RotationDegrees = degrees;
                    _rotation.Foreground = SystemColors.ControlTextBrush;
                    Refresh();
                    return;
                }
            }

            _rotation.Foreground = PreviewPalette.Warning;
        }

        /// <summary>Rebuilds the plan and repaints. The single place the preview is refreshed.</summary>
        public void Refresh()
        {
            UpdateInsertAvailability();

            if (!_state.HasSelection)
            {
                _preview.Show(null);
                _summary.Text = "Sin seleccion.";
                _fidelity.Text = string.Empty;
                _authority.Text = string.Empty;
                _authority.Visibility = Visibility.Collapsed;
                _diagnostics.Text = string.Empty;
                return;
            }

            var plan = _state.BuildPlan();
            _preview.Show(plan);

            _summary.Text = _state.Selected.DisplayName + "  ·  " +
                            FamilyLabel(_state.Selected.Family) + "  ·  " +
                            _state.Length.ToString("0.###", CultureInfo.InvariantCulture) + " in\n" +
                            _state.WeightSummary();

            _fidelity.Text = _state.FidelitySummary();

            _authority.Text = _state.AuthoritySummary();
            _authority.Visibility = string.IsNullOrEmpty(_authority.Text)
                ? Visibility.Collapsed
                : Visibility.Visible;

            var diagnostics = _state.Diagnostics();
            _diagnostics.Text = diagnostics.Count == 0
                ? string.Empty
                : string.Join("\n", diagnostics.Select(d => "• " + d.Message));
        }

        /// <summary>
        /// I-39C paga la deuda que I-39A midió y dejó asignada al arquetipo: <c>Insertar</c> no se deshabilitaba
        /// nunca y sin selección era un no-op SILENCIOSO — ADR-0029 D6 llama a eso «una violación mayor», porque una
        /// acción habilitada sin efecto es peor que una bloqueada sin motivo. Ahora se apaga con el motivo visible.
        ///
        /// <para>La otra mitad de aquella medición —que una longitud o una rotación inválidas no bloquean— se revisó
        /// y NO se cambia: es correcta. D5 exige que una entrada inválida no sobrescriba en silencio un valor
        /// aplicado válido, y eso es justo lo que la ventana hace: el campo se pinta en ámbar y el estado conserva el
        /// último valor válido, así que lo que se inserta es un valor aplicado, no basura.</para>
        /// </summary>
        private void UpdateInsertAvailability()
        {
            if (_accept == null)
            {
                return;
            }

            _accept.IsEnabled = _state.HasSelection;
            _accept.ToolTip = _state.HasSelection
                ? null
                : "Elige una sección de la lista para poder insertarla.";
        }

        private void Accept()
        {
            if (!_state.HasSelection)
            {
                return;
            }

            Result = _state.BuildPlan();
            AcceptedSection = _state.Selected;
            DialogResult = true;
        }
    }
}
