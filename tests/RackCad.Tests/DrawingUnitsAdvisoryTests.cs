using RackCad.Application.Drawing;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// Pure behaviour of the units-guardrail decision (initiative I-05, audit D4): RackCad draws in inches, so an
    /// insertion into anything that is not inches — including a unitless drawing — warrants the non-blocking advisory.
    /// These run WITHOUT AutoCAD (the AutoCAD <c>INSUNITS</c> read and the <c>UnitsValue</c> mapping stay in the Plugin,
    /// ADR-0003/ADR-0005); the Plugin's <c>UnitsValue</c> → <see cref="DrawingUnits"/> mapping is pinned separately by
    /// the source-guards in <c>RackUnitsGuardSourceTests</c>.
    /// </summary>
    public class DrawingUnitsAdvisoryTests
    {
        [Fact]
        public void Inches_DoesNotRequireAdvisory()
        {
            Assert.False(DrawingUnitsAdvisory.RequiresInsertionAdvisory(DrawingUnits.Inches));
        }

        [Fact]
        public void Unitless_RequiresAdvisory()
        {
            // A unitless drawing is NOT an assumption that it is in inches: it must warn (requirement: "incluido Unitless").
            Assert.True(DrawingUnitsAdvisory.RequiresInsertionAdvisory(DrawingUnits.Unitless));
        }

        [Fact]
        public void OtherUnit_RequiresAdvisory()
        {
            // Millimetres/centimetres/metres/feet all classify as Other in the Plugin: they must warn.
            Assert.True(DrawingUnitsAdvisory.RequiresInsertionAdvisory(DrawingUnits.Other));
        }

        [Theory]
        [InlineData(DrawingUnits.Inches, false)]
        [InlineData(DrawingUnits.Unitless, true)]
        [InlineData(DrawingUnits.Other, true)]
        public void RequiresInsertionAdvisory_OnlyInchesIsQuiet(DrawingUnits units, bool expectedAdvisory)
        {
            Assert.Equal(expectedAdvisory, DrawingUnitsAdvisory.RequiresInsertionAdvisory(units));
        }
    }
}
