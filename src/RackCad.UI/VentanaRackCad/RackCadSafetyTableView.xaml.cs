using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// The view of «Seguridad por poste» editable (T4 seg). It only passes the gestures to the view-model of the panel: a title of a column
    /// shows its family in the inspector, a level button turns its level on or off, and a length is committed with Enter or when it is left.
    /// Every failure is contained and said in the line of messages of the editor.
    /// </summary>
    public partial class RackCadSafetyTableView : UserControl
    {
        public RackCadSafetyTableView()
        {
            InitializeComponent();
        }

        private RackCadSafetyPanelViewModel Panel => DataContext as RackCadSafetyPanelViewModel;

        private void OnHeadClick(object sender, RoutedEventArgs e) => Contained(() =>
        {
            if ((sender as FrameworkElement)?.Tag is string type) Panel?.Pick(type);
        });

        private void OnLevelClick(object sender, RoutedEventArgs e) => Contained(() =>
        {
            if ((sender as FrameworkElement)?.DataContext is RackCadSafetyLevelItem level) level.Toggle();
        });

        private void OnFieldKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            e.Handled = true;
            Commit(sender);
        }

        private void OnFieldLostFocus(object sender, RoutedEventArgs e) => Commit(sender);

        private void Commit(object sender) => Contained(() =>
        {
            if ((sender as FrameworkElement)?.DataContext is DynamicFieldItem field) field.Commit();
        });

        private void Contained(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                Panel?.SayAction?.Invoke("El editor tuvo un fallo contenido: " + ex.Message, true);
            }
        }
    }
}
