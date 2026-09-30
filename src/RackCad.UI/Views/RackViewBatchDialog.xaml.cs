using System.Windows;
using System.Windows.Input;

namespace RackCad.UI.Views
{
    public partial class RackViewBatchDialog : Window
    {
        private readonly RackViewBatchDialogPresenter presenter;

        public RackViewBatchDialog(RackViewBatchDialogPresenter presenter)
        {
            InitializeComponent();
            this.presenter = presenter;
            DataContext = presenter;
        }

        private void Add_Click(object sender, RoutedEventArgs e) => presenter.Add(AvailableList.SelectedItem as RackViewBatchOption);
        private void Remove_Click(object sender, RoutedEventArgs e) => presenter.RemoveAt(SelectedList.SelectedIndex);
        private void Up_Click(object sender, RoutedEventArgs e) { var i = SelectedList.SelectedIndex; presenter.MoveUp(i); SelectedList.SelectedIndex = i - 1; }
        private void Down_Click(object sender, RoutedEventArgs e) { var i = SelectedList.SelectedIndex; presenter.MoveDown(i); SelectedList.SelectedIndex = i + 1; }
        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            if (presenter.Selected.Count == 0) return;
            DialogResult = true;
        }
    }
}
