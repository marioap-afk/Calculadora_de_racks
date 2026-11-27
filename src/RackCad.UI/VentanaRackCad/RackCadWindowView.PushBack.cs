using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// T5 pb: what the window shows of Push Back, built here so the shared XAML of the window does not change (six parts add their systems at
    /// the same time): the offer «Editar aquí» on the page of a Push Back (next to the offers of the other systems), «Push Back en esta
    /// ventana» in «Nuevo» (after «Dinámico en esta ventana»), and the host of the editor of a new Push Back. The page of the rack shows the
    /// editor of a Push Back when it is open (and the user did not ask for the panel), with one view per editor that the window keeps.
    /// </summary>
    public partial class RackCadWindowView
    {
        private readonly Dictionary<PushBackEditorViewModel, PushBackEditorView> _pushBackViews = new();

        /// <summary>«Editar aquí» of a Push Back.</summary>
        internal Button EditPushBackHereButton { get; private set; }

        internal Border PushBackOfferPanel { get; private set; }

        /// <summary>«Push Back en esta ventana» of «Nuevo».</summary>
        internal Button NewPushBackHereButton { get; private set; }

        /// <summary>The host of the editor of a new Push Back in «Nuevo».</summary>
        internal ContentControl NewPushBackRackHost { get; private set; }

        /// <summary>How many Push Back editor views the window keeps.</summary>
        internal int PushBackViewCount => _pushBackViews.Count;

        // One line of the constructor.
        private void BuildPushBackChrome()
        {
            var flag = (IValueConverter)FindResource("FlagToVisibility");

            EditPushBackHereButton = new Button { Content = "Editar aquí", Padding = new Thickness(12, 4, 12, 4), Margin = new Thickness(8, 0, 0, 0) };
            EditPushBackHereButton.Style = (Style)FindResource("PrimaryButtonStyle");
            AutomationProperties.SetName(EditPushBackHereButton, "Editar el Push Back en la ventana");
            EditPushBackHereButton.Click += (_, __) => Contained(() => _viewModel.EditPushBackHere());
            DockPanel.SetDock(EditPushBackHereButton, Dock.Right);
            var offerText = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap,
                VerticalAlignment = VerticalAlignment.Center,
                Text = "Este rack es Push Back. Puedes editarlo aquí, con el editor nuevo, sin bloquear AutoCAD. RACKEDITAR sigue siendo el editor clásico.",
            };
            offerText.SetResourceReference(TextBlock.ForegroundProperty, "ShellTextPrimaryBrush");
            var offer = new DockPanel();
            offer.Children.Add(EditPushBackHereButton);
            offer.Children.Add(offerText);
            PushBackOfferPanel = new Border { Child = offer };
            PushBackOfferPanel.Style = (Style)FindResource("Banner");
            PushBackOfferPanel.Margin = new Thickness(10, 8, 10, 0);
            DockPanel.SetDock(PushBackOfferPanel, Dock.Top);
            PushBackOfferPanel.SetBinding(VisibilityProperty, new Binding(nameof(RackCadWindowViewModel.ShowPushBackOffer)) { Converter = flag });
            RackPage.Children.Insert(RackPage.Children.IndexOf(ShowBomOfRackButton), PushBackOfferPanel);

            var title = new TextBlock { Text = "Push Back en esta ventana", FontWeight = FontWeights.SemiBold };
            var caption = new TextBlock
            {
                Text = "El editor nuevo, de uno o dos sentidos, sin bloquear AutoCAD. Se inserta con «Insertar en el dibujo…».",
                TextWrapping = TextWrapping.Wrap,
            };
            var content = new StackPanel();
            content.Children.Add(title);
            content.Children.Add(caption);
            NewPushBackHereButton = new Button
            {
                Content = content,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 0, 0, 10),
                Padding = new Thickness(12, 6, 12, 6),
            };
            NewPushBackHereButton.Style = (Style)FindResource("PrimaryButtonStyle");
            AutomationProperties.SetName(NewPushBackHereButton, "Nuevo Push Back en esta ventana");
            NewPushBackHereButton.SetBinding(IsEnabledProperty, new Binding(nameof(RackCadWindowViewModel.CanStartNewPushBack)));
            NewPushBackHereButton.Click += (_, __) => Contained(() => _viewModel.StartNewPushBack());
            if (NuevoPage.Content is Panel nuevo) nuevo.Children.Insert(nuevo.Children.IndexOf(NewDynamicHereButton) + 1, NewPushBackHereButton);

            NewPushBackRackHost = new ContentControl { Focusable = false };
            NewPushBackRackHost.SetBinding(VisibilityProperty, new Binding(nameof(RackCadWindowViewModel.ShowNewPushBackEditor)) { Converter = flag });
            if (NewSelectiveRackHost.Parent is Panel center) center.Children.Insert(center.Children.IndexOf(NewSelectiveRackHost) + 1, NewPushBackRackHost);
        }

        // One line of OnViewModelPropertyChanged.
        private void OnPushBackPropertyChanged(string propertyName)
        {
            if (propertyName == nameof(RackCadWindowViewModel.ActivePushBackEditor) || propertyName == nameof(RackCadWindowViewModel.PushBackEditors))
                ShowRackContent();
            if (propertyName == nameof(RackCadWindowViewModel.NewPushBackEditor)) ShowNewPushBackContent();
        }

        // True when the page shows the editor of the active Push Back; the views of editors the window no longer keeps are forgotten.
        private bool ShowPushBackContent()
        {
            var live = _viewModel.PushBackEditors;
            foreach (var gone in _pushBackViews.Keys.Where(editor => !live.Contains(editor)).ToList()) _pushBackViews.Remove(gone);
            var active = _viewModel.ActivePushBackEditor;
            if (active == null || _viewModel.ShowPanel) return false;
            var view = ViewOf(active);
            if (!ReferenceEquals(RackPageHost.Content, view)) RackPageHost.Content = view;
            return true;
        }

        // «Nuevo» hosts the editor of the new Push Back being made here; the view is the one the page of the rack shows once it is inserted.
        private void ShowNewPushBackContent()
        {
            var editor = _viewModel.NewPushBackEditor;
            if (editor == null)
            {
                NewPushBackRackHost.Content = null;
                return;
            }

            var view = ViewOf(editor);
            if (!ReferenceEquals(NewPushBackRackHost.Content, view)) NewPushBackRackHost.Content = view;
        }

        private PushBackEditorView ViewOf(PushBackEditorViewModel editor)
        {
            if (!_pushBackViews.TryGetValue(editor, out var view))
            {
                view = new PushBackEditorView(editor);
                _pushBackViews[editor] = view;
            }

            return view;
        }
    }
}
