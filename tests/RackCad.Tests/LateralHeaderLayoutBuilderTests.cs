using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Headers;
using RackCad.Domain.RackFrames;
using Xunit;

namespace RackCad.Tests
{
    public class LateralHeaderLayoutBuilderTests
    {
        // Header 132" tall, 42" deep; first horizontal at 4", panels of 44"; troquel pitch 2".
        private static RackFrameConfiguration StandardConfiguration()
        {
            var configuration = new RackFrameConfiguration
            {
                Height = 132.0,
                Depth = 42.0,
                PasoTroquel = 2.0,
                DiagonalStartOffsetTroqueles = 2,
                DiagonalEndOffsetTroqueles = 2,
                DiagonalDoubleSpacingTroqueles = 1,
                HorizontalDoubleOffsetTroqueles = 1,
                LeftPost = new PostAssembly { PostCatalogId = "PO" },
                RightPost = new PostAssembly { PostCatalogId = "PO" },
                LeftBasePlate = new BasePlatePlacement { PlateCatalogId = "PL" },
                RightBasePlate = new BasePlatePlacement { PlateCatalogId = "PL" }
            };

            AddHorizontal(configuration, "H1", 1, 4.0);
            AddHorizontal(configuration, "H2", 2, 48.0);
            AddHorizontal(configuration, "H3", 3, 92.0);

            AddPanel(configuration, "P1", 1, "H1", "H2", BracingPattern.SingleDiagonal);
            AddPanel(configuration, "P2", 2, "H2", "H3", BracingPattern.SingleDiagonal);

            return configuration;
        }

        private static void AddHorizontal(RackFrameConfiguration configuration, string id, int number, double elevation, int quantity = 1)
        {
            configuration.Horizontals.Add(new FrameHorizontal
            {
                Id = id,
                Number = number,
                Elevation = elevation,
                ProfileId = "TR",
                Quantity = quantity
            });
        }

        private static void AddPanel(RackFrameConfiguration configuration, string id, int number, string lower, string upper, BracingPattern arrangement)
        {
            configuration.BracingPanels.Add(new BracingPanel
            {
                PanelId = id,
                Number = number,
                LowerHorizontalId = lower,
                UpperHorizontalId = upper,
                Arrangement = arrangement,
                DiagonalDirection = DiagonalDirection.AutoAlternating,
                DiagonalProfileId = "TR"
            });
        }

        // Post TROQUEL_CELOSIA at local (2, 0); other points at the origin for clean numbers.
        private static RackCatalog StandardCatalog() => new RackCatalog
        {
            ConnectionLayout = new List<ConnectionLayoutEntry>
            {
                new ConnectionLayoutEntry { PieceId = "PL", ConnectionPointId = "MONTAJE_POSTE", View = "LATERAL", LocalX = 0.0, LocalY = 0.0 },
                new ConnectionLayoutEntry { PieceId = "PO", ConnectionPointId = "TROQUEL_CELOSIA", View = "LATERAL", LocalX = 2.0, LocalY = 0.0 },
                new ConnectionLayoutEntry { PieceId = "PO", ConnectionPointId = "FIN_POSTE", View = "LATERAL", LocalX = 3.0, LocalY = 0.0 },
                new ConnectionLayoutEntry { PieceId = "TR", ConnectionPointId = "CELOSIA", View = "LATERAL", LocalX = 0.0, LocalY = 0.0 }
            },
            Blocks = new List<BlockCatalogEntry>
            {
                new BlockCatalogEntry { PieceId = "PO", View = "LATERAL", BlockName = "PO_LAT" },
                new BlockCatalogEntry { PieceId = "PL", View = "LATERAL", BlockName = "PL_LAT" },
                new BlockCatalogEntry { PieceId = "TR", View = "LATERAL", BlockName = "TR_LAT" }
            }
        };

        private static LateralHeaderLayout Build(RackFrameConfiguration configuration)
        {
            var parameters = LateralHeaderParametersFactory.FromConfiguration(configuration);
            return new LateralHeaderLayoutBuilder().Build(configuration, parameters, StandardCatalog());
        }

        [Fact]
        public void Build_InsertsTwoPostsAndPlates_PostLongitudIsHeight()
        {
            var layout = Build(StandardConfiguration());

            Assert.Equal(2, layout.OfRole(HeaderBlockRole.Post).Count());
            Assert.Equal(2, layout.OfRole(HeaderBlockRole.BasePlate).Count());

            foreach (var post in layout.OfRole(HeaderBlockRole.Post))
            {
                Assert.Equal(132.0, post.DynamicParameters["LONGITUD"], 4);
            }
        }

        [Fact]
        public void Build_AsymmetricSides_RightPostAndPlateUseTheirOwnIds_AndTheRightTroquel()
        {
            // The configurator edits post/plate PER SIDE: the right side must resolve its own catalog ids and
            // its own troquel inset — not silently inherit the left one's.
            var configuration = StandardConfiguration();
            configuration.RightPost.PostCatalogId = "PO2";
            configuration.RightBasePlate.PlateCatalogId = "PL2";

            var catalog = StandardCatalog();
            ((List<ConnectionLayoutEntry>)catalog.ConnectionLayout).AddRange(new[]
            {
                new ConnectionLayoutEntry { PieceId = "PO2", ConnectionPointId = "TROQUEL_CELOSIA", View = "LATERAL", LocalX = 3.0, LocalY = 0.0 },
                new ConnectionLayoutEntry { PieceId = "PO2", ConnectionPointId = "FIN_POSTE", View = "LATERAL", LocalX = 4.0, LocalY = 0.0 },
                new ConnectionLayoutEntry { PieceId = "PL2", ConnectionPointId = "MONTAJE_POSTE", View = "LATERAL", LocalX = 0.0, LocalY = 0.0 }
            });
            ((List<BlockCatalogEntry>)catalog.Blocks).AddRange(new[]
            {
                new BlockCatalogEntry { PieceId = "PO2", View = "LATERAL", BlockName = "PO2_LAT" },
                new BlockCatalogEntry { PieceId = "PL2", View = "LATERAL", BlockName = "PL2_LAT" }
            });

            var parameters = LateralHeaderParametersFactory.FromConfiguration(configuration);
            var layout = new LateralHeaderLayoutBuilder().Build(configuration, parameters, catalog);

            var posts = layout.OfRole(HeaderBlockRole.Post).OrderBy(p => p.Insertion.X).ToList();
            Assert.Equal("PO", posts.First().PieceId);
            Assert.Equal("PO2", posts.Last().PieceId);

            var plates = layout.OfRole(HeaderBlockRole.BasePlate).OrderBy(p => p.ConnectionAnchor.X).ToList();
            Assert.Equal("PL2", plates.Last().PieceId);

            // Horizontals span left troquel (0 + 2) to the RIGHT post's own troquel (42 - 3): length 37, not 38.
            var horizontal = layout.OfRole(HeaderBlockRole.Horizontal).First();
            Assert.Equal(37.0, horizontal.DynamicParameters["LONGITUD"], 4);
        }

        [Fact]
        public void PlatePeralteOverride_SetsThePlatePeralteParameter_OnlyWhenPresent()
        {
            var configuration = StandardConfiguration();
            configuration.LeftBasePlate.PeralteOverride = 5.0; // right stays derived (null)

            var plates = Build(configuration).OfRole(HeaderBlockRole.BasePlate).ToList();

            var withPeralte = plates.Where(p => p.DynamicParameters.ContainsKey("PERALTE")).ToList();
            Assert.Single(withPeralte);
            Assert.Equal(5.0, withPeralte[0].DynamicParameters["PERALTE"], 4);
        }

        [Fact]
        public void Build_RightPost_IsMirroredAtDepth()
        {
            var layout = Build(StandardConfiguration());
            var posts = layout.OfRole(HeaderBlockRole.Post).ToList();

            var left = posts.Single(p => !p.MirroredX);
            var right = posts.Single(p => p.MirroredX);

            Assert.Equal(0.0, left.Insertion.X, 4);
            Assert.Equal(42.0, right.Insertion.X, 4);
        }

        [Fact]
        public void Build_Horizontals_OnTroquelLine_SpanBetweenPosts_UsingLongitud()
        {
            var layout = Build(StandardConfiguration());

            Assert.Equal(38.0, layout.HorizontalLength, 4); // 42 - 2*2 troquel inset

            var horizontals = layout.OfRole(HeaderBlockRole.Horizontal).ToList();
            Assert.Equal(3, horizontals.Count);
            Assert.Equal(new[] { 4.0, 48.0, 92.0 }, horizontals.Select(h => Math.Round(h.ConnectionAnchor.Y, 4)));

            foreach (var horizontal in horizontals)
            {
                Assert.Equal(2.0, horizontal.ConnectionAnchor.X, 4);            // left troquel X
                Assert.Equal(38.0, horizontal.DynamicParameters["LONGITUD"], 4); // post-to-post, LONGITUD (not Distancia1)
            }
        }

        [Fact]
        public void Build_DoubleHorizontal_StacksExtraTravesanoOneTroquelUp()
        {
            var configuration = StandardConfiguration();
            configuration.Horizontals.First(h => h.Id == "H1").Quantity = 2;

            var layout = Build(configuration);
            var horizontals = layout.OfRole(HeaderBlockRole.Horizontal).Select(h => Math.Round(h.ConnectionAnchor.Y, 4)).ToList();

            Assert.Equal(4, horizontals.Count);          // H1 doubled
            Assert.Contains(4.0, horizontals);
            Assert.Contains(6.0, horizontals);           // second travesaño one troquel (2") above
        }

        [Fact]
        public void Build_DoubleHorizontalAtPanelBottom_DiagonalStartsAboveTheGroup()
        {
            var configuration = StandardConfiguration();
            configuration.Horizontals.First(h => h.Id == "H1").Quantity = 2; // bottom of panel P1

            var layout = Build(configuration);
            var firstDiagonal = layout.OfRole(HeaderBlockRole.Diagonal).First();

            // Without the double the start would be 4 + 4 = 8; the second travesaño sits at 6, so the start
            // must clear it by a full offset: (4 + 2) + 4 = 10.
            Assert.Equal(10.0, firstDiagonal.ConnectionAnchor.Y, 4);
        }

        [Fact]
        public void Build_SnapsHorizontalsToThePostTroquelGrid()
        {
            var configuration = StandardConfiguration();
            var catalog = new RackCatalog
            {
                ConnectionLayout = new List<ConnectionLayoutEntry>
                {
                    new ConnectionLayoutEntry { PieceId = "PL", ConnectionPointId = "MONTAJE_POSTE", View = "LATERAL", LocalX = 0.0, LocalY = 0.0 },
                    // Real post: the troquel grid starts at 1.5938, every 2".
                    new ConnectionLayoutEntry { PieceId = "PO", ConnectionPointId = "TROQUEL_CELOSIA", View = "LATERAL", LocalX = 2.0, LocalY = 1.5938 },
                    new ConnectionLayoutEntry { PieceId = "TR", ConnectionPointId = "CELOSIA", View = "LATERAL", LocalX = 0.0, LocalY = 0.0 }
                },
                Blocks = StandardCatalog().Blocks
            };

            var parameters = LateralHeaderParametersFactory.FromConfiguration(configuration);
            var layout = new LateralHeaderLayoutBuilder().Build(configuration, parameters, catalog);

            var first = layout.OfRole(HeaderBlockRole.Horizontal).OrderBy(h => h.ConnectionAnchor.Y).First();
            // Nominal 4" snaps down to the nearest troquel 3.5938 (the 0.4062" fix).
            Assert.Equal(3.5938, first.ConnectionAnchor.Y, 4);
        }

        [Fact]
        public void Build_Diagonals_OnePerPanel_Alternate()
        {
            var layout = Build(StandardConfiguration());
            var diagonals = layout.OfRole(HeaderBlockRole.Diagonal).ToList();

            Assert.Equal(2, diagonals.Count);

            // P1 (Number 1) up-right: starts on the LEFT troquel at y = 4 + 4 = 8.
            var first = diagonals[0];
            Assert.Equal(2.0, first.ConnectionAnchor.X, 4);
            Assert.Equal(8.0, first.ConnectionAnchor.Y, 4);

            // P2 (Number 2) up-left: starts on the RIGHT troquel at y = 48 + 4 = 52.
            var second = diagonals[1];
            Assert.Equal(40.0, second.ConnectionAnchor.X, 4);
            Assert.Equal(52.0, second.ConnectionAnchor.Y, 4);
        }

        [Fact]
        public void Build_NoBracingPanel_EmitsNoDiagonal()
        {
            var configuration = StandardConfiguration();
            configuration.BracingPanels.First(p => p.PanelId == "P2").Arrangement = BracingPattern.NoBracing;

            var layout = Build(configuration);

            Assert.Single(layout.OfRole(HeaderBlockRole.Diagonal));
        }

        [Fact]
        public void Build_DoubleDiagonal_EmitsTwoParallelOffsetDiagonals()
        {
            var configuration = StandardConfiguration();
            var panel = configuration.BracingPanels.First(p => p.PanelId == "P1");
            panel.Arrangement = BracingPattern.DoubleDiagonal;
            panel.DiagonalDirection = DiagonalDirection.UpRight;

            var layout = Build(configuration);
            var diagonals = layout.OfRole(HeaderBlockRole.Diagonal)
                .Where(d => Math.Abs(d.ConnectionAnchor.X - 2.0) < 1e-6) // P1 starts on the left troquel
                .OrderBy(d => d.ConnectionAnchor.Y)
                .ToList();

            Assert.Equal(2, diagonals.Count);
            // Lower diagonal starts at 4+4 = 8; upper one a troquel (2") higher at 4+6 = 10.
            Assert.Equal(8.0, diagonals[0].ConnectionAnchor.Y, 4);
            Assert.Equal(10.0, diagonals[1].ConnectionAnchor.Y, 4);

            // Parallel: same length and rotation.
            Assert.Equal(diagonals[0].DynamicParameters["LONGITUD"], diagonals[1].DynamicParameters["LONGITUD"], 4);
            Assert.Equal(diagonals[0].RotationRadians, diagonals[1].RotationRadians, 6);
        }

        [Fact]
        public void Build_TagsEachInstanceWithItsPieceId()
        {
            var layout = Build(StandardConfiguration());

            Assert.All(layout.OfRole(HeaderBlockRole.Post), post => Assert.Equal("PO", post.PieceId));
            Assert.All(layout.OfRole(HeaderBlockRole.BasePlate), plate => Assert.Equal("PL", plate.PieceId));
            Assert.All(layout.OfRole(HeaderBlockRole.Horizontal), h => Assert.Equal("TR", h.PieceId));
            Assert.All(layout.OfRole(HeaderBlockRole.Diagonal), d => Assert.Equal("TR", d.PieceId));
        }

        [Fact]
        public void Build_ResolvesBlockNamesFromCatalog()
        {
            var layout = Build(StandardConfiguration());

            Assert.All(layout.OfRole(HeaderBlockRole.Post), post => Assert.Equal("PO_LAT", post.BlockName));
            Assert.All(layout.OfRole(HeaderBlockRole.Horizontal), h => Assert.Equal("TR_LAT", h.BlockName));
        }

        [Fact]
        public void Build_LeftReinforcement_DrawsThirdPostAtFinPosteWithOwnLongitud()
        {
            var configuration = StandardConfiguration();
            configuration.LeftPost.HasReinforcement = true;
            configuration.LeftPost.ReinforcementCatalogId = "PO";
            configuration.LeftPost.ReinforcementHeight = 50.0;

            var layout = Build(configuration);
            var posts = layout.OfRole(HeaderBlockRole.Post).ToList();

            Assert.Equal(3, posts.Count); // left, right, left reinforcement
            var reinforcement = posts.Single(p => Math.Abs(p.Insertion.X - 3.0) < 1e-6); // mated at FIN_POSTE (X=3)
            Assert.False(reinforcement.MirroredX);
            Assert.Equal(50.0, reinforcement.DynamicParameters["LONGITUD"], 4); // its own height, not 132
        }

        [Fact]
        public void Build_ReinforcedZone_CelosiaUsesTheInnerReinforcementTroquel()
        {
            var configuration = StandardConfiguration();
            configuration.LeftPost.HasReinforcement = true;
            configuration.LeftPost.ReinforcementCatalogId = "PO";
            configuration.LeftPost.ReinforcementHeight = 50.0; // covers H1(4) and H2(48), not H3(92)

            var layout = Build(configuration);
            var horizontals = layout.OfRole(HeaderBlockRole.Horizontal).ToList();

            var reinforced = horizontals.Single(h => Math.Abs(h.ConnectionAnchor.Y - 4.0) < 1e-6);
            var plain = horizontals.Single(h => Math.Abs(h.ConnectionAnchor.Y - 92.0) < 1e-6);

            // In the reinforced zone the left end moves in to the reinforcement troquel (3 + 2 = 5); above it
            // returns to the post troquel (2). The right post is not reinforced, so the right end stays at 40.
            Assert.Equal(5.0, reinforced.ConnectionAnchor.X, 4);
            Assert.Equal(35.0, reinforced.DynamicParameters["LONGITUD"], 4); // 40 - 5
            Assert.Equal(2.0, plain.ConnectionAnchor.X, 4);
            Assert.Equal(38.0, plain.DynamicParameters["LONGITUD"], 4);      // 40 - 2
        }

        [Fact]
        public void Build_DiagonalCrossingReinforcementBoundary_StartsOnReinforcementEndsOnPost()
        {
            var configuration = StandardConfiguration();
            configuration.LeftPost.HasReinforcement = true;
            configuration.LeftPost.ReinforcementCatalogId = "PO";
            configuration.LeftPost.ReinforcementHeight = 50.0;

            var layout = Build(configuration);

            // P1 (number 1, up-right): start on the LEFT at y=8 (<=50 -> reinforcement troquel 5),
            // end on the RIGHT at y=44 (right post not reinforced -> 40).
            var first = layout.OfRole(HeaderBlockRole.Diagonal).First();
            Assert.Equal(5.0, first.ConnectionAnchor.X, 4);
            Assert.Equal(8.0, first.ConnectionAnchor.Y, 4);
        }

        [Fact]
        public void Build_NoReinforcement_LeavesCelosiaOnThePostTroquels()
        {
            var layout = Build(StandardConfiguration());

            Assert.Equal(2, layout.OfRole(HeaderBlockRole.Post).Count()); // no reinforcement post
            Assert.All(layout.OfRole(HeaderBlockRole.Horizontal), h => Assert.Equal(2.0, h.ConnectionAnchor.X, 4));
        }

        [Fact]
        public void Build_InvalidDepth_Throws()
        {
            var configuration = StandardConfiguration();
            configuration.Depth = 0.0;

            var parameters = LateralHeaderParametersFactory.FromConfiguration(configuration);
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new LateralHeaderLayoutBuilder().Build(configuration, parameters, StandardCatalog()));
        }
    }
}
