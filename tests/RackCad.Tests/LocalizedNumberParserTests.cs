using System.Globalization;
using RackCad.Application.Formatting;
using Xunit;

namespace RackCad.Tests
{
    public sealed class LocalizedNumberParserTests
    {
        [Theory]
        [InlineData("96.5", "es-MX", 96.5)]
        [InlineData("96,5", "es-MX", 96.5)]
        [InlineData("96.5", "es-ES", 96.5)]
        [InlineData("96,5", "en-US", 96.5)]
        public void TryDouble_AcceptsEitherDecimalSeparator(string text, string cultureName, double expected)
        {
            var parsed = LocalizedNumberParser.TryDouble(text, CultureInfo.GetCultureInfo(cultureName), out var value);

            Assert.True(parsed);
            Assert.Equal(expected, value, 6);
        }

        [Theory]
        [InlineData("1,234.5")]
        [InlineData("1.234,5")]
        [InlineData("NaN")]
        [InlineData("Infinity")]
        [InlineData("")]
        public void TryDouble_RejectsGroupedOrNonFiniteInput(string text)
        {
            Assert.False(LocalizedNumberParser.TryDouble(text, CultureInfo.GetCultureInfo("en-US"), out _));
        }
    }
}
