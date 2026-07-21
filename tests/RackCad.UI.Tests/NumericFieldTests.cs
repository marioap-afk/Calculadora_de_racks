using System.Windows.Media;
using RackCad.UI.Controls;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>The <see cref="NumericField"/> control end to end (STA): text in → parsed value + error state out.</summary>
    public sealed class NumericFieldTests
    {
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
    }
}
