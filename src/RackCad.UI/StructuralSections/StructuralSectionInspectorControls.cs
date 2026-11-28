using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using RackCad.Application.StructuralSections;
using RackCad.Application.StructuralSections.Geometry;
using RackCad.UI.Controls;

namespace RackCad.UI.StructuralSections
{
    /// <summary>
    /// The controls of the section inspector, built in code over ONE <see cref="StructuralSectionInspectorState"/>, for the two surfaces that
    /// show it: the classic modal window «Secciones estructurales» (<see cref="StructuralSectionInspectorWindow"/>, RACKSECCION's flow, which
    /// RACKMENU's «Generar perfil estructural» runs) and the page «Perfil estructural» of the RackCad window (T7 per, I-64, séptima tanda; the
    /// Owner, 2026-11-27: «Igual que la clásica»).
    ///
    /// <para>They were EXTRACTED from the window, not copied: the same controls, in the same order, with the same labels, defaults and
    /// handlers, the same preview and the same lines of weight, fidelity, authority and diagnostics. A second copy would agree today and drift
    /// at the first correction; with one, the page cannot show something the classic does not, nor the other way round. All the logic still
    /// lives in the state, which is testable without a dispatcher; this class only wires the controls to it and repaints.</para>
    ///
    /// <para>The host places <see cref="Parameters"/>, <see cref="Preview"/> and <see cref="Information"/> where its layout wants them and owns
    /// the action («Insertar»); <see cref="Refreshing"/> tells it, at every repaint, that the selection may have changed. The list is filled by
    /// <see cref="Activate"/>: the window calls it when it is loaded, the page when it is shown, so nothing is built for a surface nobody
    /// opened. Until then the filters do nothing, as in the window before its <c>Loaded</c>.</para>
    /// </summary>
    public sealed class StructuralSectionInspectorControls
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
        private bool _active;

        /// <summary>
        /// Builds the controls over <paramref name="state"/>, showing the values it already has (a fresh state shows the defaults of the
        /// classic window: no search, every family, 120 in, «Seccion», «Tabulada», «Wireframe», 0 degrees and nothing checked).
        /// </summary>
        public StructuralSectionInspectorControls(StructuralSectionInspectorState state)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));

            _search = new TextBox { Text = _state.Search, Margin = new Thickness(0, 2, 0, 8) };
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

            _family.SelectedIndex = IndexOfTag(_family, _state.Family);
            _family.SelectionChanged += (_, __) => OnFilterChanged();

            _sections = new ListBox { Height = 260, Margin = new Thickness(0, 2, 0, 8) };
            _sections.SelectionChanged += (_, __) => OnSelectionChanged();

            _length = new TextBox
            {
                Text = _state.Length.ToString(CultureInfo.CurrentCulture),
                Margin = new Thickness(0, 2, 0, 8)
            };
            _length.TextChanged += (_, __) => OnLengthChanged();

            _view = EnumCombo(new[]
            {
                ("Seccion", SectionViewKind.CrossSection),
                ("Longitudinal X", SectionViewKind.LongitudinalX),
                ("Longitudinal Y", SectionViewKind.LongitudinalY),
                ("Isometrica", SectionViewKind.Isometric)
            }, _state.View);
            _view.SelectionChanged += (_, __) => { _state.View = Selected<SectionViewKind>(_view); Refresh(); };

            _detail = EnumCombo(new[]
            {
                ("Tabulada", SectionDetailLevel.Tabulated),
                ("Simplificada", SectionDetailLevel.Simplified)
            }, _state.Detail);
            _detail.SelectionChanged += (_, __) => { _state.Detail = Selected<SectionDetailLevel>(_detail); Refresh(); };

            _mode = EnumCombo(new[]
            {
                ("Wireframe", SectionRepresentationMode.Wireframe),
                ("Envolvente", SectionRepresentationMode.Envelope),
                ("Eje", SectionRepresentationMode.Axis)
            }, _state.Mode);
            _mode.SelectionChanged += (_, __) => { _state.Mode = Selected<SectionRepresentationMode>(_mode); Refresh(); };

            _rotation = new TextBox { Text = _state.RotationDegrees.ToString(CultureInfo.CurrentCulture), Margin = new Thickness(0, 2, 0, 8) };
            _rotation.TextChanged += (_, __) => OnRotationChanged();

            _mirror = new CheckBox { Content = "Espejo", Margin = new Thickness(0, 2, 0, 4), IsChecked = _state.Mirrored };
            _mirror.Checked += (_, __) => { _state.Mirrored = true; Refresh(); };
            _mirror.Unchecked += (_, __) => { _state.Mirrored = false; Refresh(); };

            _axis = new CheckBox { Content = "Mostrar eje", Margin = new Thickness(0, 2, 0, 4), IsChecked = _state.ShowAxis };
            _axis.Checked += (_, __) => { _state.ShowAxis = true; Refresh(); };
            _axis.Unchecked += (_, __) => { _state.ShowAxis = false; Refresh(); };

            _envelope = new CheckBox { Content = "Mostrar envolvente", Margin = new Thickness(0, 2, 0, 8), IsChecked = _state.ShowEnvelope };
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

            Parameters = BuildParameters();
            Information = BuildInformation();
        }

        /// <summary>The state the controls write to and the plan is built from.</summary>
        public StructuralSectionInspectorState State => _state;

        /// <summary>The capture panel: search, family, list, length, view, detail, representation, rotation and the three checks, in this order.</summary>
        public StackPanel Parameters { get; }

        /// <summary>The preview: the renderer of the plan, never a generator (ADR-0022 §7).</summary>
        public StructuralSectionPreview Preview => _preview;

        /// <summary>The lines under the preview: summary and weight, fidelity, the authority warning and the diagnostics.</summary>
        public StackPanel Information { get; }

        /// <summary>The search box, where the work of the inspector starts (the initial focus of the window).</summary>
        public TextBox SearchBox => _search;

        /// <summary>The list has been filled (<see cref="Activate"/>).</summary>
        public bool IsActive => _active;

        /// <summary>Raised first at every repaint: the selection may have changed, so the host updates its action.</summary>
        public event Action Refreshing;

        /// <summary>Fills the list from the state and repaints; from then on the filters narrow it. Calling it again refills it.</summary>
        public void Activate()
        {
            _active = true;
            RefreshList();
        }

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

        private StackPanel BuildParameters()
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
            return left;
        }

        private StackPanel BuildInformation()
        {
            var info = new StackPanel();
            info.Children.Add(_summary);
            info.Children.Add(_fidelity);
            info.Children.Add(_authority);
            info.Children.Add(_diagnostics);
            return info;
        }

        private static TextBlock Label(string text) =>
            new TextBlock { Text = text, Margin = new Thickness(0, 6, 0, 0), FontWeight = FontWeights.SemiBold };

        private static ComboBox EnumCombo<T>((string Text, T Value)[] options, T selected) where T : struct
        {
            var combo = new ComboBox { Margin = new Thickness(0, 2, 0, 8) };

            foreach (var option in options)
            {
                combo.Items.Add(new ComboBoxItem { Content = option.Text, Tag = option.Value });
            }

            combo.SelectedIndex = Math.Max(0, IndexOfTag(combo, selected));
            return combo;
        }

        // The item whose tag is the value the state has; 0 («Todas las familias», or the first option) when none is.
        private static int IndexOfTag(ComboBox combo, object value)
        {
            for (var i = 0; i < combo.Items.Count; i++)
            {
                if (Equals(((ComboBoxItem)combo.Items[i]).Tag, value))
                {
                    return i;
                }
            }

            return 0;
        }

        private static T Selected<T>(ComboBox combo) where T : struct =>
            (T)((ComboBoxItem)combo.SelectedItem).Tag;

        private void OnFilterChanged()
        {
            if (!_active)
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
            Refreshing?.Invoke();

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
    }
}
