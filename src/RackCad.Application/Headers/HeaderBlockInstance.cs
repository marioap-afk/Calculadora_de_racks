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
        Stop
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
    }
}
