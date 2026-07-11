using System.Collections.Generic;
using RackCad.Application.Geometry;

namespace RackCad.Application.Headers
{
    public enum HeaderBlockRole
    {
        BasePlate,
        Post,
        Horizontal,
        Diagonal,
        ClosingHorizontal,

        /// <summary>A separator beam linking two adjacent headers in a dynamic system.</summary>
        Separator,

        // ---- Roller bed (cama de rodamiento) ----
        /// <summary>The rail (riel) that runs the lane depth; its length is the LONGITUD parameter.</summary>
        Rail,
        /// <summary>A roller (rodillo) on the rail.</summary>
        Roller,
        /// <summary>A brake (freno) on a dynamic bed.</summary>
        Brake,
        /// <summary>The end stop (tope) at the discharge end of the rail.</summary>
        Stop,

        /// <summary>A load beam (larguero/viga) spanning a bay in a selective rack.</summary>
        Beam,

        /// <summary>A text label (frente/level number, rack name). Drawn as DBText at <see cref="HeaderBlockInstance.Insertion"/>.</summary>
        Annotation,

        /// <summary>A linear dimension between <see cref="HeaderBlockInstance.Insertion"/> (p1) and
        /// <see cref="HeaderBlockInstance.ConnectionAnchor"/> (p2), its dimension line offset by
        /// <see cref="HeaderBlockInstance.DimensionOffset"/>. Drawn as a RotatedDimension on the dimensions layer.</summary>
        Dimension
    }

    /// <summary>
    /// One block to insert in the lateral header: which block, where its origin goes, its rotation and
    /// mirror, and the dynamic-block parameters to set after insertion (e.g. LONGITUD, Distancia1).
    /// Pure data — no AutoCAD types — so the AutoCAD drawer is the only thing that touches the API.
    /// </summary>
    public sealed class HeaderBlockInstance
    {
        public HeaderBlockRole Role { get; set; }

        /// <summary>Catalog id of the piece this instance draws (post/plate/truss). Used to look up a
        /// human-readable name when reporting, e.g., a block that is still missing from the drawing.</summary>
        public string PieceId { get; set; }

        public string BlockName { get; set; }
        public string View { get; set; }

        /// <summary>World point where the block's own origin is inserted.</summary>
        public Point2D Insertion { get; set; }

        /// <summary>
        /// World point where the block's reference connection point lands (post origin / MONTAJE_POSTE /
        /// CELOSIA / diagonal start). Redundant with <see cref="Insertion"/> but explicit for clarity and tests.
        /// </summary>
        public Point2D ConnectionAnchor { get; set; }

        public double RotationRadians { get; set; }

        /// <summary>Insert with X scale -1 (the right post and its plate are mirrored).</summary>
        public bool MirroredX { get; set; }

        /// <summary>Dynamic-block parameters to set after insertion (name -&gt; value).</summary>
        public Dictionary<string, double> DynamicParameters { get; } = new Dictionary<string, double>();

        /// <summary>For <see cref="HeaderBlockRole.Annotation"/>: the text to draw. Null/empty for block instances.</summary>
        public string Text { get; set; }

        /// <summary>For an annotation: text height (in). 0 = the drawer's default. For a dimension: its text height.</summary>
        public double TextHeight { get; set; }

        /// <summary>For a <see cref="HeaderBlockRole.Dimension"/>: signed perpendicular distance from the measured
        /// segment (Insertion→ConnectionAnchor) to the dimension line. Its axis (horizontal vs vertical) is derived
        /// from the two points; the sign chooses the side (e.g. below/left of the geometry).</summary>
        public double DimensionOffset { get; set; }
    }
}
