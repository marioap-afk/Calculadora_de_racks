using System;
using System.Collections.Generic;
using RackCad.Application.Geometry;
using RackCad.Application.StructuralSections;
using RackCad.Application.StructuralSections.Geometry;
using RackCad.Domain.Systems.Cantilever;

namespace RackCad.Application.Systems.Cantilever
{
    /// <summary>
    /// Where one member of a paired body sits: whether its section is mirrored, and how far it moves along
    /// the body's transverse axis.
    ///
    /// Both values are in SECTION coordinates. The resolver turns the offset into a frame origin shifted
    /// along the frame's X axis, which is the same thing expressed in world terms.
    /// </summary>
    public readonly struct CantileverArmMemberPlacement
    {
        public CantileverArmMemberPlacement(bool mirrored, double transverseOffset)
        {
            GeometryTolerance.RequireFinite(transverseOffset, nameof(transverseOffset));
            Mirrored = mirrored;
            TransverseOffset = transverseOffset;
        }

        /// <summary>Whether the section is mirrored about its own Y axis before being placed.</summary>
        public bool Mirrored { get; }

        /// <summary>Shift along the section's local X, inches.</summary>
        public double TransverseOffset { get; }

        /// <summary>The member's X interval in placed section coordinates, given the section's own bounds.</summary>
        public (double Min, double Max) SpanX(Bounds2D bounds)
        {
            // Mirroring about Y sends x to -x, so the interval flips end for end. Y is untouched, which is
            // why a pair of channels shares one depth interval.
            var min = Mirrored ? -bounds.MaxX : bounds.MinX;
            var max = Mirrored ? -bounds.MinX : bounds.MaxX;
            return (min + TransverseOffset, max + TransverseOffset);
        }

        public override string ToString() =>
            (Mirrored ? "espejado" : "directo") + " dx=" +
            TransverseOffset.ToString("0.####", System.Globalization.CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// THE authority that turns a body arrangement into member placements.
    ///
    /// Everything here is derived from <c>StructuralSectionGeometry.Bounds</c>. Nothing reads <c>d</c>,
    /// <c>bf</c>, a concrete dimension type or the designation, and that is not a style rule: the contact
    /// between two channels has to land on the contour that will be DRAWN, and a tabulated width is a nominal
    /// number (ADR-0024, D5).
    ///
    /// <b>The canonical channel orientation was READ from I-36, not assumed from the name.</b>
    /// <c>ChannelSectionGeometryBuilder</c> documents and implements it: the BACK OF THE WEB faces −X and the
    /// flanges OPEN TOWARDS +X, with <c>d</c> along Y and <c>bf</c> along X. The contour is then translated
    /// onto the tabulated centroid, so in section coordinates:
    ///
    /// <list type="bullet">
    ///   <item><c>Bounds.MinX</c> is the BACK OF THE WEB — the closed side;</item>
    ///   <item><c>Bounds.MaxX</c> is the FLANGE TIP — the open side.</item>
    /// </list>
    ///
    /// Those two facts are the whole content of the paired placements below.
    /// </summary>
    public static class CantileverArmBodyArrangementResolver
    {
        /// <summary>Whether this arrangement has a placement rule. False for anything not declared.</summary>
        public static bool IsSupported(CantileverArmBodyArrangement arrangement) =>
            arrangement == CantileverArmBodyArrangement.Single ||
            arrangement == CantileverArmBodyArrangement.DoubleChannelFacing ||
            arrangement == CantileverArmBodyArrangement.DoubleChannelBackToBack;

        /// <summary>How many members this arrangement produces.</summary>
        public static int MemberCount(CantileverArmBodyArrangement arrangement)
        {
            switch (arrangement)
            {
                case CantileverArmBodyArrangement.Single:
                    return 1;
                case CantileverArmBodyArrangement.DoubleChannelFacing:
                case CantileverArmBodyArrangement.DoubleChannelBackToBack:
                    return 2;
                default:
                    throw Undefined(arrangement);
            }
        }

        /// <summary>Whether this arrangement is only meaningful on a channel.</summary>
        public static bool RequiresChannel(CantileverArmBodyArrangement arrangement)
        {
            switch (arrangement)
            {
                case CantileverArmBodyArrangement.Single:
                    return false;
                case CantileverArmBodyArrangement.DoubleChannelFacing:
                case CantileverArmBodyArrangement.DoubleChannelBackToBack:
                    return true;
                default:
                    throw Undefined(arrangement);
            }
        }

        /// <summary>
        /// The placements, in deterministic order.
        ///
        /// For both paired arrangements the two members end up symmetric about the body's central plane
        /// (<c>x = 0</c> in section coordinates), touching exactly there, with no gap and no overlap. The gap
        /// is zero because the arrangement puts the two contact faces on the same plane — not because a
        /// parameter says so; there is no such parameter (ADR-0025, D2).
        /// </summary>
        public static IReadOnlyList<CantileverArmMemberPlacement> Placements(
            CantileverArmBodyArrangement arrangement, StructuralSectionGeometry geometry)
        {
            if (geometry == null)
            {
                throw new ArgumentNullException(nameof(geometry));
            }

            var b = geometry.Bounds;

            switch (arrangement)
            {
                case CantileverArmBodyArrangement.Single:
                    // Centred on the body's central plane.
                    return new[] { new CantileverArmMemberPlacement(false, -b.Center.X) };

                case CantileverArmBodyArrangement.DoubleChannelFacing:
                    // Openings face each other: the FLANGE TIPS meet on the central plane. The first member
                    // keeps its own hand, so its tips (at MaxX) move to zero and its body falls on -X; the
                    // second is mirrored, so its tips land at -MaxX and move to zero from the other side.
                    return new[]
                    {
                        new CantileverArmMemberPlacement(false, -b.MaxX),
                        new CantileverArmMemberPlacement(true, b.MaxX)
                    };

                case CantileverArmBodyArrangement.DoubleChannelBackToBack:
                    // Backs of the webs meet on the central plane and the openings look outwards. Mirrored
                    // first: its web back (at MinX) becomes -MinX and moves to zero, leaving the body on -X.
                    return new[]
                    {
                        new CantileverArmMemberPlacement(true, b.MinX),
                        new CantileverArmMemberPlacement(false, -b.MinX)
                    };

                default:
                    throw Undefined(arrangement);
            }
        }

        /// <summary>The combined section extent of a placed body, in section coordinates.</summary>
        public static Bounds2D CombinedBounds(
            CantileverArmBodyArrangement arrangement, StructuralSectionGeometry geometry)
        {
            var b = geometry?.Bounds ?? throw new ArgumentNullException(nameof(geometry));
            var minX = double.PositiveInfinity;
            var maxX = double.NegativeInfinity;

            foreach (var placement in Placements(arrangement, geometry))
            {
                var span = placement.SpanX(b);
                if (span.Min < minX) minX = span.Min;
                if (span.Max > maxX) maxX = span.Max;
            }

            // Y is the same for every member: a mirror about Y does not move it.
            return new Bounds2D(minX, b.MinY, maxX, b.MaxY);
        }

        /// <summary>
        /// Whether the placements touch without overlapping, measured rather than asserted.
        ///
        /// For a single member there is nothing to check. For a pair, the two spans must share exactly one
        /// boundary: the maximum of the lower one equals the minimum of the upper one.
        /// </summary>
        public static bool TouchesWithoutOverlap(
            CantileverArmBodyArrangement arrangement,
            StructuralSectionGeometry geometry,
            double tolerance,
            out double gap,
            out double overlap)
        {
            var b = geometry?.Bounds ?? throw new ArgumentNullException(nameof(geometry));
            var placements = Placements(arrangement, geometry);
            gap = 0.0;
            overlap = 0.0;

            if (placements.Count < 2)
            {
                return true;
            }

            var first = placements[0].SpanX(b);
            var second = placements[1].SpanX(b);

            var lower = first.Min <= second.Min ? first : second;
            var upper = first.Min <= second.Min ? second : first;

            var delta = upper.Min - lower.Max;

            if (delta > tolerance)
            {
                gap = delta;
                return false;
            }

            if (delta < -tolerance)
            {
                overlap = -delta;
                return false;
            }

            return true;
        }

        private static ArgumentOutOfRangeException Undefined(CantileverArmBodyArrangement arrangement) =>
            new ArgumentOutOfRangeException(
                nameof(arrangement), arrangement,
                "El arreglo de cuerpo '" + arrangement +
                "' no tiene regla de colocacion. Anadir un arreglo exige escribirla aqui.");
    }
}
