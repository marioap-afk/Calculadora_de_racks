using RackCad.Application.Geometry;
using RackCad.Application.Headers;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>Unit coverage for the pure SEPARADOR placement factory (I-22, E6): the separador-instance rule the
    /// lateral and planta builders now share.</summary>
    public class SelectiveSeparadorPlacementTests
    {
        [Fact]
        public void Separador_AnchorsOnTheConnectionPoint_OffsetsByTheMate_AndSpansTheGap()
        {
            var separador = SelectiveSeparadorPlacement.Separador(
                "SEPARADOR_PLANTA", "PLANTA", anchor: new Point2D(40.0, 12.0), mate: new Point2D(1.5, 0.5), gap: 18.0);

            Assert.Equal(HeaderBlockRole.Separator, separador.Role);
            Assert.Equal(DynamicRackDefaults.SeparatorCatalogId, separador.PieceId);
            Assert.Equal("SEPARADOR_PLANTA", separador.BlockName);
            Assert.Equal("PLANTA", separador.View);
            Assert.Equal(40.0, separador.ConnectionAnchor.X, 6);
            Assert.Equal(12.0, separador.ConnectionAnchor.Y, 6);
            Assert.Equal(38.5, separador.Insertion.X, 6);   // anchor.X - mate.X
            Assert.Equal(11.5, separador.Insertion.Y, 6);   // anchor.Y - mate.Y
            Assert.Equal(18.0, separador.DynamicParameters[SelectiveRackDefaults.LengthParam], 6);
        }
    }
}
