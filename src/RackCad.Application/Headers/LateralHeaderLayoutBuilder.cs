using System;
using System.Collections.Generic;
using RackCad.Application.Geometry;

namespace RackCad.Application.Headers
{
    /// <summary>
    /// Builds a lateral header as a sequence of block insertions anchored on connection points — NOT a
    /// free visual composition. The post is the geometric base; horizontals and diagonals hang off the
    /// post's troquel line. Pure and unit-testable: it computes positions, lengths and dynamic
    /// parameters; the AutoCAD drawer turns the plan into real InsertBlock calls.
    ///
    /// Lateral-view axes: X = depth direction (between posts), Y = height.
    /// </summary>
    public sealed class LateralHeaderLayoutBuilder
    {
        private const double Tolerance = 1e-6;

        public LateralHeaderLayout Build(LateralHeaderParameters p, HeaderConnectionGeometry g)
        {
            if (p == null) throw new ArgumentNullException(nameof(p));
            if (g == null) throw new ArgumentNullException(nameof(g));
            p.Validate();

            var instances = new List<HeaderBlockInstance>();

            // 1. Left post + base plate at X = 0; the post origin (0,0) mates onto the plate's MONTAJE_POSTE.
            var leftPostOrigin = new Point2D(0.0, 0.0);
            AddPostWithPlate(instances, p, g, leftPostOrigin, mirrored: false);

            // 2. Right post mirrored at the depth: its origin sits at X = Depth (posts are Depth apart).
            var rightPostOrigin = new Point2D(p.Depth, 0.0);
            AddPostWithPlate(instances, p, g, rightPostOrigin, mirrored: true);

            // 3. Celosía reference line from the post's TROQUEL_CELOSIA (right post mirrored).
            var xTroquelLeft = leftPostOrigin.X + g.TroquelCelosia.X;
            var xTroquelRight = rightPostOrigin.X - g.TroquelCelosia.X;
            var yTroquel0 = leftPostOrigin.Y + g.TroquelCelosia.Y;
            var horizontalLength = xTroquelRight - xTroquelLeft;

            // 4. Horizontal (panel) Y positions.
            var yFirst = TroquelY(yTroquel0, p.InicioCelosiaTroquel, p.PasoTroquel);
            var panelGaps = PanelCount(p.Height, yFirst, p.ClaroPanel);
            var horizontalYs = HorizontalYs(yFirst, panelGaps, p.ClaroPanel);

            // 5. Horizontals: each one's CELOSIA point lands on the troquel line at its Y.
            foreach (var y in horizontalYs)
            {
                instances.Add(MakeMember(p, g, HeaderBlockRole.Horizontal,
                    new Point2D(xTroquelLeft, y), rotation: 0.0, length: horizontalLength));
            }

            // 6. Diagonals: one per panel, offset in from each horizontal by N troqueles.
            var diagonalCount = 0;
            for (var k = 0; k < horizontalYs.Count - 1; k++)
            {
                instances.Add(MakeDiagonal(p, g, xTroquelLeft, xTroquelRight, horizontalYs[k], horizontalYs[k + 1]));
                diagonalCount++;
            }

            // 7. Closing horizontal when there is leftover clear at the top.
            var closingGap = ClosingGap(p, yFirst, panelGaps);
            if (closingGap > Tolerance)
            {
                var yClosing = yFirst + panelGaps * p.ClaroPanel + closingGap; // == Height when auto
                instances.Add(MakeMember(p, g, HeaderBlockRole.ClosingHorizontal,
                    new Point2D(xTroquelLeft, yClosing), rotation: 0.0, length: horizontalLength));
            }
            else
            {
                closingGap = 0.0;
            }

            return new LateralHeaderLayout(instances, horizontalLength, horizontalYs.Count, diagonalCount, closingGap);
        }

        // ---- Steps 1 & 2: insert a post with its base plate ----
        private static void AddPostWithPlate(ICollection<HeaderBlockInstance> instances, LateralHeaderParameters p,
            HeaderConnectionGeometry g, Point2D postOrigin, bool mirrored)
        {
            var sign = mirrored ? -1.0 : 1.0;

            // Plate first: insert it so its MONTAJE_POSTE coincides with the post origin.
            instances.Add(new HeaderBlockInstance
            {
                Role = HeaderBlockRole.BasePlate,
                BlockName = g.BasePlateBlock,
                View = p.View,
                MirroredX = mirrored,
                ConnectionAnchor = postOrigin,
                Insertion = new Point2D(postOrigin.X - sign * g.MontajePoste.X, postOrigin.Y - g.MontajePoste.Y)
            });

            // Post: its own origin is the reference; stretch LONGITUD to the header height.
            var post = new HeaderBlockInstance
            {
                Role = HeaderBlockRole.Post,
                BlockName = g.PostBlock,
                View = p.View,
                MirroredX = mirrored,
                ConnectionAnchor = postOrigin,
                Insertion = postOrigin
            };
            post.DynamicParameters[p.PostLengthParameter] = p.Height;
            instances.Add(post);
        }

        // ---- Step 4: troquel positions ----

        /// <summary>World Y of a 1-based troquel index along the celosía line.</summary>
        public static double TroquelY(double yTroquel0, int troquelIndex, double pasoTroquel)
            => yTroquel0 + (troquelIndex - 1) * pasoTroquel;

        /// <summary>Number of full panel gaps that fit between the first horizontal and the header top.</summary>
        public static int PanelCount(double height, double yFirst, double claroPanel)
        {
            var span = height - yFirst;
            if (span <= 0.0) return 0;
            return (int)Math.Floor((span + Tolerance) / claroPanel);
        }

        private static IReadOnlyList<double> HorizontalYs(double yFirst, int panelGaps, double claroPanel)
        {
            var ys = new List<double>();
            if (yFirst < 0.0) return ys;
            for (var k = 0; k <= panelGaps; k++) ys.Add(yFirst + k * claroPanel);
            return ys;
        }

        private static double ClosingGap(LateralHeaderParameters p, double yFirst, int panelGaps)
        {
            if (!p.AutoClosing) return Math.Max(0.0, p.ValorClaroTravesano);
            var yTopStandard = yFirst + panelGaps * p.ClaroPanel;
            return Math.Max(0.0, p.Height - yTopStandard);
        }

        // ---- Steps 5 & 7: horizontals (and closing) ----
        private static HeaderBlockInstance MakeMember(LateralHeaderParameters p, HeaderConnectionGeometry g,
            HeaderBlockRole role, Point2D connectionAnchor, double rotation, double length)
        {
            var insertion = Insertion(connectionAnchor, g.Celosia, rotation);
            var instance = new HeaderBlockInstance
            {
                Role = role,
                BlockName = g.HorizontalBlock,
                View = p.View,
                RotationRadians = rotation,
                ConnectionAnchor = connectionAnchor,
                Insertion = insertion
            };
            instance.DynamicParameters[p.MemberLengthParameter] = length;
            return instance;
        }

        // ---- Step 6: diagonals ----
        private static HeaderBlockInstance MakeDiagonal(LateralHeaderParameters p, HeaderConnectionGeometry g,
            double xLeft, double xRight, double yLower, double yUpper)
        {
            var start = new Point2D(xLeft, yLower + p.OffsetDiagonalInicioTroqueles * p.PasoTroquel);
            var end = new Point2D(xRight, yUpper - p.OffsetDiagonalFinTroqueles * p.PasoTroquel);

            var dx = end.X - start.X;
            var dy = end.Y - start.Y;
            var length = Math.Sqrt(dx * dx + dy * dy);
            var rotation = Math.Atan2(dy, dx);

            var instance = new HeaderBlockInstance
            {
                Role = HeaderBlockRole.Diagonal,
                BlockName = g.DiagonalBlock,
                View = p.View,
                RotationRadians = rotation,
                ConnectionAnchor = start,
                Insertion = Insertion(start, g.Celosia, rotation)
            };
            instance.DynamicParameters[p.MemberLengthParameter] = length;
            return instance;
        }

        /// <summary>Block origin so that its local CELOSIA point lands on the anchor, accounting for rotation.</summary>
        private static Point2D Insertion(Point2D anchor, Point2D localConnection, double rotation)
        {
            var rotated = Rotate(localConnection, rotation);
            return new Point2D(anchor.X - rotated.X, anchor.Y - rotated.Y);
        }

        private static Point2D Rotate(Point2D p, double radians)
        {
            var cos = Math.Cos(radians);
            var sin = Math.Sin(radians);
            return new Point2D(p.X * cos - p.Y * sin, p.X * sin + p.Y * cos);
        }
    }
}
