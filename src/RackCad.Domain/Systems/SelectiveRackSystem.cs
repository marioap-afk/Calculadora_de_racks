using System.Collections.Generic;

namespace RackCad.Domain.Systems
{
    /// <summary>
    /// The RESOLVED geometry of a selective rack run in the FRONTAL view: a sequence of bays bounded by
    /// shared frames (cabeceras). N bays → N+1 cabeceras (each drawn as one post in frontal). Everything here
    /// is already computed (beam lengths, level Ys, height) — the pallet-driven rules live in the resolver
    /// (<c>SelectiveGeometryResolver</c>); this is just what the builder places. Phase 1: all frames share one
    /// height and post; each bay (and each level within it) can differ.
    /// </summary>
    public sealed class SelectiveRackSystem
    {
        /// <summary>Height of every cabecera/post (in). Derived, uniform for the whole run in Phase 1.</summary>
        public double Height { get; set; }

        /// <summary>Catalog id of the post used for the cabeceras.</summary>
        public string PostId { get; set; }

        /// <summary>Peralte of the post (drives the larguero troquel X via the parametric mate).</summary>
        public double PostPeralte { get; set; }

        /// <summary>The bays, left to right. N bays sit between N+1 cabeceras.</summary>
        public IList<SelectiveBay> Bays { get; } = new List<SelectiveBay>();
    }

    /// <summary>One resolved bay: its beam length (which governs post spacing) and its placed levels.</summary>
    public sealed class SelectiveBay
    {
        /// <summary>
        /// Beam length = LONGITUD (the profile "A corte"), per bay: it fixes the post spacing
        /// (post-to-post = BeamLength + 2*(troquelX + inicioPerfilX)). All levels of the bay share it.
        /// </summary>
        public double BeamLength { get; set; }

        /// <summary>The load levels of this bay, bottom to top, each with its own resolved Y and beam.</summary>
        public IList<SelectiveLevel> Levels { get; } = new List<SelectiveLevel>();
    }

    /// <summary>One resolved load level of a bay: a larguero at a fixed (already snapped) troquel Y.</summary>
    public sealed class SelectiveLevel
    {
        /// <summary>Troquel Y where the larguero sits (already snapped to the grid by the resolver).</summary>
        public double Y { get; set; }

        /// <summary>Catalog id of the beam (larguero) at this level.</summary>
        public string BeamId { get; set; }

        /// <summary>Beam peralte (block parameter) at this level.</summary>
        public double BeamPeralte { get; set; }
    }
}
