using RackCad.Application.Headers;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>Unit coverage for the pure TOPE placement helpers (I-22, E6): the rise-and-snap Y formula and the tope
    /// instance factory the frontal, lateral and planta builders now share instead of copy-pasting.</summary>
    public class SelectiveTopePlacementTests
    {
        [Fact]
        public void SnapY_RisesAboveTheLarguero_ThenLandsOnTheGrid()
        {
            // troquelMateY 10, larguero 50, paso 2, YOffset 8: 10 + round((50+8-10)/2)*2 = 10 + 24*2 = 58.
            Assert.Equal(58.0, SelectiveTopePlacement.SnapY(10.0, 50.0, 2.0), 6);
            // larguero 51: (51+8-10)/2 = 24.5 -> AwayFromZero 25 -> 10 + 50 = 60.
            Assert.Equal(60.0, SelectiveTopePlacement.SnapY(10.0, 51.0, 2.0), 6);
        }

        [Fact]
        public void Saque_UsesTheSelectionWhenPositive_ElseTheDomainDefault()
        {
            Assert.Equal(5.0, SelectiveTopePlacement.Saque(new SelectiveSafetySelection { TopeSaque = 5.0 }), 6);
            Assert.Equal(SelectiveSafetyDefaults.TopeSaque, SelectiveTopePlacement.Saque(new SelectiveSafetySelection { TopeSaque = 0.0 }), 6);
            Assert.Equal(SelectiveSafetyDefaults.TopeSaque, SelectiveTopePlacement.Saque(null), 6);
        }

        [Fact]
        public void Tope_WithLongitud_SetsBothParams_AndHonoursTheMirror()
        {
            var tope = SelectiveTopePlacement.Tope("POSTE_TOPE", "POSTE_TOPE_PLANTA", "PLANTA", x: 7.0, y: 12.0, saque: 3.0, longitud: 92.25, mirroredX: true);

            Assert.Equal(HeaderBlockRole.Tope, tope.Role);
            Assert.Equal("PLANTA", tope.View);
            Assert.True(tope.MirroredX);
            Assert.Equal(7.0, tope.Insertion.X, 6);
            Assert.Equal(12.0, tope.Insertion.Y, 6);
            Assert.Equal(92.25, tope.DynamicParameters[SelectiveRackDefaults.LengthParam], 6);
            Assert.Equal(3.0, tope.DynamicParameters[SelectiveSafetyDefaults.SaqueParam], 6);
        }

        [Fact]
        public void Tope_WithoutLongitud_CarriesOnlySaque_LikeTheLateral()
        {
            var tope = SelectiveTopePlacement.Tope("POSTE_TOPE", "POSTE_TOPE_LATERAL", "LATERAL", x: 41.0, y: 60.0, saque: 5.0);

            Assert.False(tope.MirroredX);
            Assert.False(tope.DynamicParameters.ContainsKey(SelectiveRackDefaults.LengthParam));
            Assert.Equal(5.0, tope.DynamicParameters[SelectiveSafetyDefaults.SaqueParam], 6);
        }
    }
}
