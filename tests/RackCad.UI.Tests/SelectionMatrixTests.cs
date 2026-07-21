using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using RackCad.UI.Controls;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>The <see cref="SelectionMatrix"/> control (STA): it builds from the model, syncs both ways, and
    /// updates a single cell without rebuilding the grid.</summary>
    public sealed class SelectionMatrixTests
    {
        private static int CheckBoxCount(SelectionMatrix matrix)
            => matrix.Children.OfType<CheckBox>().Count();

        [Fact]
        public void BuildsOneCheckBoxPerCell()
        {
            var count = StaTestRunner.Run(() =>
            {
                var matrix = new SelectionMatrix { Model = new SelectionMatrixModel(3, 2) };
                return CheckBoxCount(matrix);
            });

            Assert.Equal(6, count);
        }

        [Fact]
        public void ModelChange_UpdatesTheMatchingCheckBox()
        {
            var isChecked = StaTestRunner.Run(() =>
            {
                var model = new SelectionMatrixModel(3, 2);
                var matrix = new SelectionMatrix { Model = model };
                model.SetSelected(1, 1, false);
                return matrix.CellFor(1, 1).IsChecked;
            });

            Assert.False(isChecked);
        }

        [Fact]
        public void SetAll_RepaintsEveryCheckBox()
        {
            var anyChecked = StaTestRunner.Run(() =>
            {
                var model = new SelectionMatrixModel(3, 3);
                var matrix = new SelectionMatrix { Model = model };
                model.SetAll(false);
                return matrix.Children.OfType<CheckBox>().Any(cb => cb.IsChecked == true);
            });

            Assert.False(anyChecked);
        }

        [Fact]
        public void CheckBoxClick_UpdatesModel()
        {
            var offInModel = StaTestRunner.Run(() =>
            {
                var model = new SelectionMatrixModel(2, 2);
                var matrix = new SelectionMatrix { Model = model };
                var cell = matrix.CellFor(0, 0);
                cell.IsChecked = false;
                cell.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                return model[0, 0];
            });

            Assert.False(offInModel);
        }

        [Fact]
        public void SingleCellChange_DoesNotRebuild()
        {
            var (sameInstance, isChecked) = StaTestRunner.Run(() =>
            {
                var model = new SelectionMatrixModel(3, 2);
                var matrix = new SelectionMatrix { Model = model };
                var before = matrix.CellFor(0, 0);
                model.SetSelected(0, 0, false);
                var after = matrix.CellFor(0, 0);
                return (ReferenceEquals(before, after), after.IsChecked);
            });

            Assert.True(sameInstance); // the check box was updated in place, not recreated
            Assert.False(isChecked);
        }

        [Fact]
        public void InvertRows_DrawsHighestRowOnTop()
        {
            var (normalRow, invertedRow) = StaTestRunner.Run(() =>
            {
                var normal = new SelectionMatrix { Model = new SelectionMatrixModel(1, 2) };
                var inverted = new SelectionMatrix { Model = new SelectionMatrixModel(1, 2), InvertRows = true };
                return (Grid.GetRow(normal.CellFor(0, 0)), Grid.GetRow(inverted.CellFor(0, 0)));
            });

            Assert.Equal(1, normalRow);   // header row is 0; model row 0 sits just below
            Assert.Equal(2, invertedRow); // inverted: model row 0 pushed to the bottom
        }

        [Fact]
        public void ColumnHeaders_UseProvidedCaptions()
        {
            var text = StaTestRunner.Run(() =>
            {
                var matrix = new SelectionMatrix
                {
                    Model = new SelectionMatrixModel(2, 1),
                    ColumnHeaders = new[] { "F1", "F2" },
                };
                return matrix.Children.OfType<TextBlock>().Select(t => t.Text).ToArray();
            });

            Assert.Contains("F1", text);
            Assert.Contains("F2", text);
        }
    }
}
