using System.Collections.Generic;

namespace RackCad.Domain.Systems
{
    /// <summary>
    /// A selective rack run in the FRONTAL view: a sequence of bays bounded by shared frames (cabeceras).
    /// N bays → N+1 cabeceras (each drawn as one post in frontal). Phase 1: all frames share one height and
    /// post; each bay can differ (its beam, peralte, length and level configuration).
    /// </summary>
    public sealed class SelectiveRackSystem
    {
        /// <summary>Height of every cabecera/post (in). Same for the whole run in Phase 1.</summary>
        public double Height { get; set; }

        /// <summary>Catalog id of the post used for the cabeceras.</summary>
        public string PostId { get; set; }

        /// <summary>Peralte of the post (drives the larguero troquel X via the parametric mate).</summary>
        public double PostPeralte { get; set; }

        /// <summary>The bays, left to right. N bays sit between N+1 cabeceras.</summary>
        public IList<SelectiveBay> Bays { get; } = new List<SelectiveBay>();
    }

    /// <summary>One bay of a selective run: its beam (larguero) and the vertical level configuration.</summary>
    public sealed class SelectiveBay
    {
        /// <summary>Catalog id of the beam (larguero).</summary>
        public string BeamId { get; set; }

        /// <summary>Beam peralte (block parameter).</summary>
        public double BeamPeralte { get; set; }

        /// <summary>Beam length = LONGITUD (A corte + ménsula); the clear span basis.</summary>
        public double BeamLength { get; set; }

        /// <summary>Number of load levels (one beam per level).</summary>
        public int Levels { get; set; }

        /// <summary>Height of the first level (in). User input in Phase 1; snapped to the troquel grid.</summary>
        public double FirstLevel { get; set; }

        /// <summary>Troquel-to-troquel separation between levels (in); a multiple of the troquel pitch.</summary>
        public double Separation { get; set; }
    }
}
