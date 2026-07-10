using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Geometry;
using RackCad.Application.Headers;
using RackCad.Application.Systems;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>The roller-bed BOM aggregates the placed instances into rail / tope / rodillos / frenos.</summary>
    public class FlowBedBomBuilderTests
    {
        private static HeaderBlockInstance Piece(HeaderBlockRole role, string id)
            => new HeaderBlockInstance { Role = role, PieceId = id, Insertion = new Point2D(0, 0) };

        private static HeaderBlockInstance Rail(string id, double longitud)
        {
            var rail = Piece(HeaderBlockRole.Rail, id);
            rail.DynamicParameters["LONGITUD"] = longitud;
            return rail;
        }

        [Fact]
        public void Build_AggregatesRailStopRollersAndBrakes()
        {
            var instances = new List<HeaderBlockInstance>
            {
                Rail("RIEL", 240.0),
                Piece(HeaderBlockRole.Stop, "TOPE"),
                Piece(HeaderBlockRole.Roller, "ROD"),
                Piece(HeaderBlockRole.Roller, "ROD"),
                Piece(HeaderBlockRole.Roller, "ROD"),
                Piece(HeaderBlockRole.Brake, "FRENO")
            };

            var bom = FlowBedBomBuilder.Build(instances, null);

            var rail = bom.Lines.Single(l => l.Category == FlowBedBomBuilder.Rail);
            Assert.Equal(240.0, rail.Length, 2);
            Assert.Equal(1, rail.Quantity);
            Assert.Equal(1, bom.Lines.Single(l => l.Category == FlowBedBomBuilder.Stop).Quantity);
            Assert.Equal(3, bom.Lines.Single(l => l.Category == FlowBedBomBuilder.Roller).Quantity);
            Assert.Equal(1, bom.Lines.Single(l => l.Category == FlowBedBomBuilder.Brake).Quantity);
            Assert.Equal(6, bom.TotalPieces);
        }

        [Fact]
        public void Build_OrdersRailFirstThenTopeRollersBrakes()
        {
            var instances = new List<HeaderBlockInstance>
            {
                Piece(HeaderBlockRole.Brake, "FRENO"),
                Piece(HeaderBlockRole.Roller, "ROD"),
                Piece(HeaderBlockRole.Stop, "TOPE"),
                Rail("RIEL", 200.0)
            };

            var categories = FlowBedBomBuilder.Build(instances, null).Lines.Select(l => l.Category).ToList();

            Assert.Equal(
                new[] { FlowBedBomBuilder.Rail, FlowBedBomBuilder.Stop, FlowBedBomBuilder.Roller, FlowBedBomBuilder.Brake },
                categories);
        }
    }
}
