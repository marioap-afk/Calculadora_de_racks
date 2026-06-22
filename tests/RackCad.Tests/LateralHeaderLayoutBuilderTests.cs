using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Geometry;
using RackCad.Application.Headers;
using Xunit;

namespace RackCad.Tests
{
    public class LateralHeaderLayoutBuilderTests
    {
        private const double Tol = 1e-4;

        // Header 132" tall, 42" deep; troquel pitch 2", first horizontal at troquel 3, panels every 44".
        private static LateralHeaderParameters StandardParameters() => new LateralHeaderParameters
        {
            Height = 132.0,
            Depth = 42.0,
            PasoTroquel = 2.0,
            InicioCelosiaTroquel = 3,
            ClaroPanel = 44.0,
            OffsetDiagonalInicioTroqueles = 2,
            OffsetDiagonalFinTroqueles = 2,
            PostId = "PO",
            BasePlateId = "PL",
            TrussProfileId = "TR"
        };

        // Post TROQUEL_CELOSIA at local (2, 0); other connection points at the origin for clean numbers.
        private static HeaderConnectionGeometry StandardGeometry() => new HeaderConnectionGeometry
        {
            MontajePoste = new Point2D(0.0, 0.0),
            TroquelCelosia = new Point2D(2.0, 0.0),
            Celosia = new Point2D(0.0, 0.0),
            PostBlock = "PO_LAT",
            BasePlateBlock = "PL_LAT",
            HorizontalBlock = "TR_LAT",
            DiagonalBlock = "TR_LAT"
        };

        [Fact]
        public void Build_InsertsTwoPostsAndPlates_PostLongitudIsHeight()
        {
            var layout = new LateralHeaderLayoutBuilder().Build(StandardParameters(), StandardGeometry());

            Assert.Equal(2, layout.OfRole(HeaderBlockRole.Post).Count());
            Assert.Equal(2, layout.OfRole(HeaderBlockRole.BasePlate).Count());

            foreach (var post in layout.OfRole(HeaderBlockRole.Post))
            {
                Assert.Equal(132.0, post.DynamicParameters["LONGITUD"], 4);
            }
        }

        [Fact]
        public void Build_RightPost_IsMirroredAtDepth()
        {
            var layout = new LateralHeaderLayoutBuilder().Build(StandardParameters(), StandardGeometry());
            var posts = layout.OfRole(HeaderBlockRole.Post).ToList();

            var left = posts.Single(p => !p.MirroredX);
            var right = posts.Single(p => p.MirroredX);

            Assert.Equal(0.0, left.Insertion.X, 4);
            Assert.Equal(42.0, right.Insertion.X, 4); // second post at the depth
        }

        [Fact]
        public void Build_Horizontals_OnTroquelLine_SpanBetweenPosts()
        {
            var layout = new LateralHeaderLayoutBuilder().Build(StandardParameters(), StandardGeometry());

            // Span = Depth - 2 * troquel inset = 42 - 4 = 38.
            Assert.Equal(38.0, layout.HorizontalLength, 4);

            var horizontals = layout.OfRole(HeaderBlockRole.Horizontal).ToList();
            Assert.Equal(3, horizontals.Count); // at y = 4, 48, 92

            Assert.Equal(new[] { 4.0, 48.0, 92.0 }, horizontals.Select(h => Math.Round(h.ConnectionAnchor.Y, 4)));
            foreach (var h in horizontals)
            {
                Assert.Equal(2.0, h.ConnectionAnchor.X, 4);              // left troquel X
                Assert.Equal(38.0, h.DynamicParameters["Distancia1"], 4); // covers post-to-post
            }
        }

        [Fact]
        public void Build_ClosingHorizontal_FillsLeftoverTop()
        {
            var layout = new LateralHeaderLayoutBuilder().Build(StandardParameters(), StandardGeometry());

            // Top standard horizontal at 92; leftover up to 132 = 40.
            Assert.True(layout.HasClosingHorizontal);
            Assert.Equal(40.0, layout.ClosingGap, 4);

            var closing = Assert.Single(layout.OfRole(HeaderBlockRole.ClosingHorizontal));
            Assert.Equal(132.0, closing.ConnectionAnchor.Y, 4); // sits at the header top
            Assert.Equal(38.0, closing.DynamicParameters["Distancia1"], 4);
        }

        [Fact]
        public void Build_NoLeftover_AddsNoClosingHorizontal()
        {
            var p = StandardParameters();
            p.Height = 92.0; // first(4) + 2 panels(88) lands exactly on top

            var layout = new LateralHeaderLayoutBuilder().Build(p, StandardGeometry());

            Assert.False(layout.HasClosingHorizontal);
            Assert.Empty(layout.OfRole(HeaderBlockRole.ClosingHorizontal));
            Assert.Equal(0.0, layout.ClosingGap, 4);
        }

        [Fact]
        public void Build_Diagonals_OnePerPanel_OffsetByTroqueles()
        {
            var layout = new LateralHeaderLayoutBuilder().Build(StandardParameters(), StandardGeometry());
            var diagonals = layout.OfRole(HeaderBlockRole.Diagonal).ToList();

            Assert.Equal(2, diagonals.Count); // one per panel between the 3 horizontals

            // First panel: between y=4 and y=48. Start 2 troqueles up (+4), end 2 troqueles down (-4).
            var first = diagonals[0];
            Assert.Equal(2.0, first.ConnectionAnchor.X, 4);  // starts on left troquel
            Assert.Equal(8.0, first.ConnectionAnchor.Y, 4);  // 4 + 2*2

            var dx = 38.0;                  // post-to-post
            var dy = (48.0 - 4.0) - 4.0 - 4.0; // claro 44 minus both offsets (4+4) = 36
            Assert.Equal(Math.Sqrt(dx * dx + dy * dy), first.DynamicParameters["Distancia1"], 4);
            Assert.Equal(Math.Atan2(dy, dx), first.RotationRadians, 4);
        }

        [Fact]
        public void TroquelY_And_PanelCount_AreParametric()
        {
            Assert.Equal(4.0, LateralHeaderLayoutBuilder.TroquelY(0.0, 3, 2.0), 4); // (3-1)*2
            Assert.Equal(2, LateralHeaderLayoutBuilder.PanelCount(132.0, 4.0, 44.0));
        }

        [Fact]
        public void Build_InvalidParameters_Throws()
        {
            var p = StandardParameters();
            p.Depth = 0.0;
            Assert.Throws<ArgumentOutOfRangeException>(() => new LateralHeaderLayoutBuilder().Build(p, StandardGeometry()));
        }

        [Fact]
        public void Resolver_ReadsConnectionLayoutAndBlocks_FromCatalog()
        {
            var catalog = new RackCatalog
            {
                ConnectionLayout = new List<ConnectionLayoutEntry>
                {
                    new ConnectionLayoutEntry { PieceId = "PL", ConnectionPointId = "MONTAJE_POSTE", View = "LATERAL", LocalX = 1.0, LocalY = 2.0 },
                    new ConnectionLayoutEntry { PieceId = "PO", ConnectionPointId = "TROQUEL_CELOSIA", View = "LATERAL", LocalX = 2.3, LocalY = 1.5 }
                },
                Blocks = new List<BlockCatalogEntry>
                {
                    new BlockCatalogEntry { PieceId = "PO", View = "LATERAL", BlockName = "PO_LAT" },
                    new BlockCatalogEntry { PieceId = "PL", View = "LATERAL", BlockName = "PL_LAT" }
                }
            };

            var g = HeaderGeometryResolver.Resolve(catalog, new LateralHeaderParameters { PostId = "PO", BasePlateId = "PL", TrussProfileId = "TR" });

            Assert.Equal(2.3, g.TroquelCelosia.X, 4);
            Assert.Equal(1.5, g.TroquelCelosia.Y, 4);
            Assert.Equal(2.0, g.MontajePoste.Y, 4);
            Assert.Equal("PO_LAT", g.PostBlock);
            Assert.Equal("PL_LAT", g.BasePlateBlock);
        }
    }
}
