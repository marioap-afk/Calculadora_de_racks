using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Drawing;
using RackCad.Application.Geometry;
using RackCad.Application.Systems.PushBack;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-66 (I-50 H4): <see cref="HeaderRunPlan.Flatten"/> clones each grouped instance per placement and must keep the text and
    /// dimension fields for every role (Freeze F-1..F-5, obligations N-1..N-5 and N-7..N-10; N-6 is the existing suites). A mirrored
    /// placement is the nested reference with X scale -1, so a vertical dimension's line lands on the other side: its offset flips sign,
    /// as in <see cref="PushBackMirror.Instance"/>; a tie |dx| = |dy| is horizontal, as in the drawer. Expected values are literal, except
    /// N-9, which compares two Flatten calls with each other.
    /// </summary>
    public class I66PlacedCloneTests
    {
        private const double InsertionX = 300.0;

        private static HeaderBlockInstance Label() => new HeaderBlockInstance
        {
            Role = HeaderBlockRole.Annotation,
            Text = "Frente 2",
            TextHeight = 4.5,
            Insertion = new Point2D(12, 30),
            ConnectionAnchor = new Point2D(12, 30)
        };

        private static HeaderBlockInstance HorizontalDimension() => new HeaderBlockInstance
        {
            Role = HeaderBlockRole.Dimension,
            Text = "96",
            Insertion = new Point2D(0, -5),
            ConnectionAnchor = new Point2D(96, -5),
            DimensionOffset = -12.0,
            DimensionStyleName = "RACK-COTAS",
            TextHeight = 2.5
        };

        private static HeaderBlockInstance VerticalDimension() => new HeaderBlockInstance
        {
            Role = HeaderBlockRole.Dimension,
            Text = "144",
            Insertion = new Point2D(-4, 0),
            ConnectionAnchor = new Point2D(-4, 144),
            DimensionOffset = -10.0,
            DimensionStyleName = "RACK-COTAS",
            TextHeight = 2.5
        };

        private static HeaderBlockInstance Post()
        {
            var post = new HeaderBlockInstance
            {
                Role = HeaderBlockRole.Post,
                PieceId = "POSTE.3",
                BlockName = "POSTE_3",
                View = "lateral",
                RotationRadians = 0.25,
                MirroredY = true,
                Insertion = new Point2D(2, 1),
                ConnectionAnchor = new Point2D(0, 0)
            };
            post.DynamicParameters["LONGITUD"] = 144.0;
            return post;
        }

        /// <summary>The single flat clone of <paramref name="source"/> placed once (mirrored or not) inside a one-instance group.</summary>
        private static HeaderBlockInstance PlaceOnce(HeaderBlockInstance source, bool mirrored, double insertionY = 0.0)
        {
            var plan = new HeaderRunPlan(
                new[] { new HeaderGroup("G", new[] { source }, new[] { new HeaderPlacement(InsertionX, mirrored, insertionY) }) },
                new List<HeaderBlockInstance>());
            return Assert.Single(plan.Flatten().Instances);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void N1_Annotation_KeepsTextAndTextHeight(bool mirrored)
        {
            var source = Label();

            var clone = PlaceOnce(source, mirrored);

            Assert.Equal("Frente 2", clone.Text);
            Assert.Equal(4.5, clone.TextHeight);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void N2_HorizontalDimension_KeepsOffsetStyleAndTextHeight(bool mirrored)
        {
            var clone = PlaceOnce(HorizontalDimension(), mirrored);

            Assert.Equal(-12.0, clone.DimensionOffset); // the line stays below: an X mirror does not move a Y offset
            Assert.Equal("96", clone.Text);
            Assert.Equal("RACK-COTAS", clone.DimensionStyleName);
            Assert.Equal(2.5, clone.TextHeight);
        }

        [Theory]
        [InlineData(false, -10.0)]
        [InlineData(true, 10.0)]
        public void N3_VerticalDimension_FlipsOffsetOnlyWhenMirrored(bool mirrored, double expectedOffset)
        {
            var clone = PlaceOnce(VerticalDimension(), mirrored);

            Assert.Equal(expectedOffset, clone.DimensionOffset);
            Assert.Equal("144", clone.Text);
            Assert.Equal("RACK-COTAS", clone.DimensionStyleName);
            Assert.Equal(2.5, clone.TextHeight);
        }

        public static IEnumerable<object[]> Dimensions()
        {
            yield return new object[] { "vertical" };
            yield return new object[] { "horizontal" };
        }

        [Theory]
        [MemberData(nameof(Dimensions))]
        public void N4_MirroredDimension_EqualsThePushBackReflection(string axis)
        {
            var source = axis == "vertical" ? VerticalDimension() : HorizontalDimension();

            var clone = PlaceOnce(source, mirrored: true);
            var reflected = PushBackMirror.Instance(source, InsertionX); // x -> InsertionX - x, the same reflection as the placement

            Assert.Equal(reflected.Insertion, clone.Insertion);
            Assert.Equal(reflected.ConnectionAnchor, clone.ConnectionAnchor);
            Assert.Equal(reflected.Text, clone.Text);
            Assert.Equal(reflected.TextHeight, clone.TextHeight);
            Assert.Equal(reflected.DimensionOffset, clone.DimensionOffset);
            Assert.Equal(reflected.DimensionStyleName, clone.DimensionStyleName);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void N5_BlockPiece_KeepsTheBaseFieldsAndDefaultTextFields(bool mirrored)
        {
            var source = Post();

            var clone = PlaceOnce(source, mirrored, insertionY: 7.0);

            Assert.Equal(HeaderBlockRole.Post, clone.Role);
            Assert.Equal("POSTE.3", clone.PieceId);
            Assert.Equal("POSTE_3", clone.BlockName);
            Assert.Equal("lateral", clone.View);
            Assert.Equal(0.25, clone.RotationRadians);
            Assert.Equal(mirrored, clone.MirroredX);
            Assert.True(clone.MirroredY);
            Assert.Equal(new Point2D(mirrored ? InsertionX - 2 : InsertionX + 2, 8), clone.Insertion);
            Assert.Equal(new Point2D(InsertionX, 7), clone.ConnectionAnchor);
            Assert.Equal(144.0, clone.DynamicParameters["LONGITUD"]);
            Assert.Null(clone.Text);
            Assert.Equal(0.0, clone.TextHeight);
            Assert.Equal(0.0, clone.DimensionOffset);
            Assert.Null(clone.DimensionStyleName);
            Assert.NotSame(source, clone);
        }

        [Theory]
        [InlineData(8.0, 8.0)]   // |dx| = |dy|: the drawer's tie is horizontal, so the offset is kept
        [InlineData(0.0, 0.0)]   // zero-length segment: also a tie
        public void N7_TieDimension_KeepsOffsetWhenMirrored(double dx, double dy)
        {
            var source = new HeaderBlockInstance
            {
                Role = HeaderBlockRole.Dimension,
                Insertion = new Point2D(5, 5),
                ConnectionAnchor = new Point2D(5 + dx, 5 + dy),
                DimensionOffset = -6.0,
                DimensionStyleName = "RACK-COTAS",
                TextHeight = 2.5
            };

            var clone = PlaceOnce(source, mirrored: true);

            Assert.Equal(-6.0, clone.DimensionOffset);
            Assert.Equal("RACK-COTAS", clone.DimensionStyleName);
        }

        public static IEnumerable<object[]> NonDimensionRoles()
        {
            yield return new object[] { HeaderBlockRole.Annotation, null };
            yield return new object[] { HeaderBlockRole.Post, "POSTE_3" };
        }

        [Theory]
        [MemberData(nameof(NonDimensionRoles))]
        public void N8_NonDimension_CopiesTheFourFieldsWithoutFlippingTheOffset(HeaderBlockRole role, string blockName)
        {
            var source = new HeaderBlockInstance
            {
                Role = role,
                BlockName = blockName,
                Insertion = new Point2D(1, 0),
                ConnectionAnchor = new Point2D(1, 50), // vertical points: only a Dimension flips its offset
                Text = "A",
                TextHeight = 3.5,
                DimensionOffset = -7.0,
                DimensionStyleName = "RACK-COTAS"
            };

            var clone = PlaceOnce(source, mirrored: true);

            Assert.Equal("A", clone.Text);
            Assert.Equal(3.5, clone.TextHeight);
            Assert.Equal(-7.0, clone.DimensionOffset);
            Assert.Equal("RACK-COTAS", clone.DimensionStyleName);
        }

        [Fact]
        public void N9_Flatten_LeavesTheSourceUntouchedAndRepeats()
        {
            var source = VerticalDimension();
            var plan = new HeaderRunPlan(
                new[] { new HeaderGroup("G", new[] { source }, new[] { new HeaderPlacement(0, false), new HeaderPlacement(InsertionX, true) }) },
                new List<HeaderBlockInstance>());

            var first = plan.Flatten().Instances.Select(i => (i.DimensionOffset, i.DimensionStyleName, i.TextHeight)).ToList();
            var second = plan.Flatten().Instances.Select(i => (i.DimensionOffset, i.DimensionStyleName, i.TextHeight)).ToList();

            Assert.Equal(-10.0, source.DimensionOffset);
            Assert.Equal("RACK-COTAS", source.DimensionStyleName);
            Assert.Equal(2.5, source.TextHeight);
            Assert.Equal(first, second);
        }

        public static IEnumerable<object[]> Decorations()
        {
            yield return new object[] { HeaderBlockRole.Annotation };
            yield return new object[] { HeaderBlockRole.Dimension };
        }

        [Theory]
        [MemberData(nameof(Decorations))]
        public void N10_MirroredDecoration_TogglesMirrorAndKeepsRotation(HeaderBlockRole role)
        {
            var source = role == HeaderBlockRole.Annotation ? Label() : HorizontalDimension();
            source.RotationRadians = 0.3;

            var clone = PlaceOnce(source, mirrored: true);

            Assert.True(clone.MirroredX);              // F-3: the mirror flag toggles for every role, decorations too
            Assert.Equal(0.3, clone.RotationRadians);  // F-3: the base keeps the rotation; I-66 does not change it
        }

        [Fact]
        public void N5_LooseInstances_AreAppendedByReference()
        {
            var loose = Label();
            var plan = new HeaderRunPlan(
                new[] { new HeaderGroup("G", new[] { Post() }, new[] { new HeaderPlacement(0, false), new HeaderPlacement(50, true) }) },
                new[] { loose });

            var flat = plan.Flatten().Instances;

            Assert.Equal(3, flat.Count);
            Assert.Same(loose, flat.Last());
        }
    }
}
