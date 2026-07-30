using System;
using RackCad.Application.Geometry;
using RackCad.Application.StructuralSections.Geometry;

namespace RackCad.Application.Systems.Cantilever
{
    /// <summary>Where one bracing member sits: its frame, and whether its section is mirrored first.</summary>
    public readonly struct CantileverBracingPlacement
    {
        internal CantileverBracingPlacement(LocalFrame3D frame, bool mirrored)
        {
            Frame = frame;
            Mirrored = mirrored;
        }

        public LocalFrame3D Frame { get; }

        /// <summary>Whether the section is mirrored about its own Y before being placed.</summary>
        public bool Mirrored { get; }

        public override string ToString() => (Mirrored ? "espejado" : "directo") + " " + Frame.Origin;
    }

    /// <summary>
    /// THE frame authority of the bracing, alongside the one for the column and base (I-37A) and the one for
    /// the arm (I-37B).
    ///
    /// It exists for the reason those two do: a frame built inline in a resolver is a frame nobody can find
    /// again. The separator's orientation in particular is not obvious — it depends on which extreme of a
    /// channel's <c>Bounds</c> is the back of its web — and the day that convention is revisited, this is the
    /// one file that has to change.
    ///
    /// Everything here comes from <c>Bounds</c>. Nothing reads <c>d</c>, <c>bf</c> or <c>tw</c>: the contact
    /// between a separator and its plate lands on the contour that will be DRAWN, and a tabulated width is a
    /// nominal number (ADR-0024, D5).
    /// </summary>
    public static class CantileverBracingFrameResolver
    {
        /// <summary>
        /// A separator: it runs along +X with its depth vertical and the BACK OF ITS WEB pressed against the
        /// plate.
        ///
        /// <para><b>Orientation.</b> <c>AxisZ = X</c> is the run. <c>LocalFrame3D</c> fixes
        /// <c>AxisY = AxisZ × AxisX</c>, so asking for <c>AxisY = Z</c> — the section's depth pointing up —
        /// means passing <c>referenceX = Z × X = Y</c>. The section's local X, which for a channel is the flange
        /// direction, therefore maps to world Y.</para>
        ///
        /// <para><b>The mirror.</b> I-36's canonical channel puts the back of the web at <c>Bounds.MinX</c> and
        /// the flange tips at <c>Bounds.MaxX</c>. Placed unmirrored, the web would face the aisle and the flanges
        /// would open towards the column. Mirroring sends x to −x, so the web's back lands at the placed
        /// MAXIMUM and can be seated on the plate.</para>
        /// </summary>
        /// <param name="startX">World X of the separator's cut end at the interval's left.</param>
        /// <param name="webBackY">World Y the back of the web must sit on — the plate's far face.</param>
        /// <param name="middleZ">World Z of the separator's mid-depth, which is its punch line.</param>
        public static CantileverBracingPlacement Separator(
            double startX, double webBackY, double middleZ, StructuralSectionGeometry geometry)
        {
            if (geometry == null)
            {
                throw new ArgumentNullException(nameof(geometry));
            }

            GeometryTolerance.RequireFinite(startX, nameof(startX));
            GeometryTolerance.RequireFinite(webBackY, nameof(webBackY));
            GeometryTolerance.RequireFinite(middleZ, nameof(middleZ));

            var bounds = geometry.Bounds;

            // Mirroring sends the interval [MinX, MaxX] to [-MaxX, -MinX], so the web's back — MinX before the
            // mirror — is at -MinX after it. Seating it on the plate means shifting the origin by that much.
            var webBackAtOrigin = -bounds.MinX;

            var frame = LocalFrame3D.Create(
                new Point3D(startX, webBackY - webBackAtOrigin, middleZ - bounds.Center.Y),
                Vector3D.UnitX,
                Vector3D.UnitY);

            return new CantileverBracingPlacement(frame, true);
        }

        /// <summary>
        /// A structural brace: it runs along its own sloped axis, IN the bracing plane, centred on that axis.
        ///
        /// <para>Its depth is the horizontal normal to its own axis — world Y — because the diagonal lies in the
        /// bracing plane. <c>AxisY = Y</c> means <c>referenceX = Y × AxisZ</c>, written as the product so the
        /// derivation stays visible rather than as a literal that would only be right for one slope.</para>
        ///
        /// <para>The section is CENTRED on the axis, both ways: the axis of a diagonal is the line between its
        /// two bolts, and a section seated on one of its own edges would hang off the bolt line by half its
        /// depth.</para>
        /// </summary>
        public static CantileverBracingPlacement Brace(
            Point3D start, Vector3D direction, StructuralSectionGeometry geometry)
        {
            if (geometry == null)
            {
                throw new ArgumentNullException(nameof(geometry));
            }

            var axisZ = direction.Normalized();
            var referenceX = Vector3D.UnitY.Cross(axisZ);
            var bounds = geometry.Bounds;

            var origin = start +
                (referenceX * -bounds.Center.X) +
                (Vector3D.UnitY * -bounds.Center.Y);

            return new CantileverBracingPlacement(
                LocalFrame3D.Create(origin, axisZ, referenceX), false);
        }
    }
}
