using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using RackCad.UI.Controls;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>The <see cref="NumericField"/> control end to end (STA): text in → parsed value + error state out.</summary>
    public sealed class NumericFieldTests
    {
        /// <summary>A binding source for the border-provenance regression test.</summary>
        public sealed class BorderBrushSource
        {
            public Brush Border { get; set; }
        }

        [Fact]
        public void Text_ParsesLocalizedValue()
        {
            var (value, hasError) = StaTestRunner.Run(() =>
            {
                var field = new NumericField { Text = "96,5" };
                return (field.Value, field.HasError);
            });

            Assert.Equal(96.5, value.Value, 6);
            Assert.False(hasError);
        }

        [Fact]
        public void OptionalBlank_IsAutoWithNullValue()
        {
            var (status, value, hasError) = StaTestRunner.Run(() =>
            {
                var field = new NumericField { IsOptional = true, Text = "" };
                return (field.Status, field.Value, field.HasError);
            });

            Assert.Equal(NumericFieldStatus.EmptyOptional, status);
            Assert.Null(value);
            Assert.False(hasError);
        }

        [Fact]
        public void RequiredBlank_IsError()
        {
            var hasError = StaTestRunner.Run(() => new NumericField { IsOptional = false }.HasError);

            Assert.True(hasError);
        }

        [Fact]
        public void OutOfRange_SetsErrorBorder()
        {
            var (hasError, status, border) = StaTestRunner.Run(() =>
            {
                var field = new NumericField { Minimum = 1, Maximum = 5, Text = "10" };
                return (field.HasError, field.Status, field.BorderBrush as SolidColorBrush);
            });

            Assert.True(hasError);
            Assert.Equal(NumericFieldStatus.AboveMaximum, status);
            Assert.NotNull(border);
            Assert.Equal(Color.FromRgb(0xB0, 0x00, 0x20), border.Color); // status-error red
        }

        [Fact]
        public void RuleChange_Revalidates()
        {
            var hasErrorAfterWidening = StaTestRunner.Run(() =>
            {
                var field = new NumericField { Maximum = 5, Text = "10" };
                var before = field.HasError;
                field.Maximum = 20; // widening the range must clear the error without retyping
                return (before, field.HasError);
            });

            Assert.True(hasErrorAfterWidening.before);
            Assert.False(hasErrorAfterWidening.HasError);
        }

        [Fact]
        public void SetNumber_WritesTextAndValue()
        {
            var (text, value) = StaTestRunner.Run(() =>
            {
                var field = new NumericField();
                field.SetNumber(3.5);
                return (field.Text, field.Value);
            });

            Assert.False(string.IsNullOrEmpty(text));
            Assert.Equal(3.5, value.Value, 6);
        }

        [Fact]
        public void Validated_EventFires()
        {
            var fired = StaTestRunner.Run(() =>
            {
                var field = new NumericField();
                var count = 0;
                field.Validated += (_, __) => count++;
                field.Text = "7";
                return count;
            });

            Assert.True(fired >= 1);
        }

        [Fact]
        public void ValidErrorValid_RestoresConsumerLocalBorderBrush()
        {
            var (redDuringError, restoredExactBrush) = StaTestRunner.Run(() =>
            {
                var custom = new SolidColorBrush(Color.FromRgb(0x12, 0x34, 0x56));
                custom.Freeze();

                var field = new NumericField();
                field.BorderBrush = custom;   // consumer's own local brush, set after construction

                field.Text = "abc";           // → error: border turns red
                var isRed = (field.BorderBrush as SolidColorBrush)?.Color == Color.FromRgb(0xB0, 0x00, 0x20);

                field.Text = "12";            // → valid: the consumer's brush must come back, not ClearValue
                var restored = ReferenceEquals(field.BorderBrush, custom);

                return (isRed, restored);
            });

            Assert.True(redDuringError);
            Assert.True(restoredExactBrush);
        }

        [Fact]
        public void ValidErrorValid_RestoresConsumerBorderBinding()
        {
            var (bindingBefore, bindingAfter, valueIsSourceBrush) = StaTestRunner.Run(() =>
            {
                var source = new BorderBrushSource { Border = Brushes.Blue };
                var field = new NumericField { DataContext = source };
                field.SetBinding(Control.BorderBrushProperty, new Binding(nameof(BorderBrushSource.Border)));

                var before = BindingOperations.GetBindingExpression(field, Control.BorderBrushProperty) != null;

                field.Text = "abc"; // error detaches the binding (so red is never written to the source)
                field.Text = "12";  // valid must re-apply the consumer's binding

                var after = BindingOperations.GetBindingExpression(field, Control.BorderBrushProperty) != null;
                var isSourceBrush = ReferenceEquals(field.BorderBrush, Brushes.Blue);

                return (before, after, isSourceBrush);
            });

            Assert.True(bindingBefore);
            Assert.True(bindingAfter);          // binding conserved, not destroyed
            Assert.True(valueIsSourceBrush);    // and it resolves to the source value again
        }

        [Fact]
        public void ErrorState_DoesNotWriteRedToBindingSource()
        {
            var sourceStaysBlue = StaTestRunner.Run(() =>
            {
                var source = new BorderBrushSource { Border = Brushes.Blue };
                var field = new NumericField { DataContext = source };
                field.SetBinding(Control.BorderBrushProperty, new Binding(nameof(BorderBrushSource.Border)));

                field.Text = "abc"; // error shows red WITHOUT pushing it back to source.Border
                return ReferenceEquals(source.Border, Brushes.Blue);
            });

            Assert.True(sourceStaysBlue);
        }
    }
}
