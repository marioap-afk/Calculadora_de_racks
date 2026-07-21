using RackCad.UI.Controls;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>The parse+range rule behind every <see cref="NumericField"/>. No WPF, so it runs on the normal thread.</summary>
    public sealed class NumericFieldValidationTests
    {
        [Theory]
        [InlineData("96.5", 96.5)]
        [InlineData("96,5", 96.5)] // localized comma decimal, NEVER 965 (no grouping) — the HANDOFF invariant.
        [InlineData("  12  ", 12.0)]
        public void Validate_ParsesLocalizedNumber(string text, double expected)
        {
            var result = NumericFieldValidation.Validate(text, isOptional: false, integerOnly: false, minimum: null, maximum: null);

            Assert.Equal(NumericFieldStatus.Valid, result.Status);
            Assert.True(result.IsValid);
            Assert.Equal(expected, result.Value.Value, 6);
        }

        [Fact]
        public void Validate_EmptyOptional_IsValidWithNullValue()
        {
            var result = NumericFieldValidation.Validate("", isOptional: true, integerOnly: false, minimum: null, maximum: null);

            Assert.Equal(NumericFieldStatus.EmptyOptional, result.Status);
            Assert.True(result.IsValid);
            Assert.Null(result.Value);
        }

        [Fact]
        public void Validate_EmptyRequired_IsError()
        {
            var result = NumericFieldValidation.Validate("   ", isOptional: false, integerOnly: false, minimum: null, maximum: null);

            Assert.Equal(NumericFieldStatus.EmptyRequired, result.Status);
            Assert.True(result.HasError);
            Assert.Equal(NumericFieldValidation.EmptyRequiredMessage, result.Message);
        }

        [Fact]
        public void Validate_NotANumber_IsError()
        {
            var result = NumericFieldValidation.Validate("abc", isOptional: false, integerOnly: false, minimum: null, maximum: null);

            Assert.Equal(NumericFieldStatus.NotANumber, result.Status);
            Assert.True(result.HasError);
        }

        [Theory]
        [InlineData("3.5", true)]  // integer-only rejects a fraction
        [InlineData("3", false)]
        public void Validate_IntegerOnly_RejectsFractions(string text, bool hasError)
        {
            var result = NumericFieldValidation.Validate(text, isOptional: false, integerOnly: true, minimum: null, maximum: null);

            Assert.Equal(hasError, result.HasError);
        }

        [Fact]
        public void Validate_BelowInclusiveMinimum_IsError()
        {
            var result = NumericFieldValidation.Validate("-1", isOptional: false, integerOnly: false, minimum: 0, maximum: null);

            Assert.Equal(NumericFieldStatus.BelowMinimum, result.Status);
        }

        [Fact]
        public void Validate_AtInclusiveMinimum_IsValid()
        {
            var result = NumericFieldValidation.Validate("0", isOptional: false, integerOnly: false, minimum: 0, maximum: null);

            Assert.Equal(NumericFieldStatus.Valid, result.Status);
        }

        [Fact]
        public void Validate_AtExclusiveMinimum_IsError()
        {
            var result = NumericFieldValidation.Validate(
                "0", isOptional: false, integerOnly: false, minimum: 0, maximum: null, minimumInclusive: false);

            Assert.Equal(NumericFieldStatus.BelowMinimum, result.Status);
        }

        [Fact]
        public void Validate_AboveMaximum_IsError()
        {
            var result = NumericFieldValidation.Validate("11", isOptional: false, integerOnly: false, minimum: null, maximum: 10);

            Assert.Equal(NumericFieldStatus.AboveMaximum, result.Status);
            Assert.Equal(11.0, result.Value.Value, 6); // offending value echoed back
        }

        [Fact]
        public void Validate_WithinRange_IsValid()
        {
            var result = NumericFieldValidation.Validate("3", isOptional: false, integerOnly: false, minimum: 1, maximum: 5);

            Assert.Equal(NumericFieldStatus.Valid, result.Status);
            Assert.Equal(3.0, result.Value.Value, 6);
        }
    }
}
