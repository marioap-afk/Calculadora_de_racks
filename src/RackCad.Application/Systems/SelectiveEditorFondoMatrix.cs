using System.Collections.Generic;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// A saved copy of ONE fondo's level matrix (doble profundidad: each back-to-back side edits its OWN levels): the
    /// bays (each a column of <see cref="SelectiveEditorCell"/>), the per-bay "larguero a piso" flag, the per-bay manual
    /// height override, the per-bay "medio frente" tramos, plus this fondo's own depth and its optional cabecera-fondo
    /// override. Extracted verbatim from the private <c>FondoMatrix</c> of <c>RackSelectiveWindow</c> (initiative I-20).
    /// The four parallel lists stay index-aligned by bay, exactly as the editor keeps them.
    /// </summary>
    public sealed class SelectiveEditorFondoMatrix
    {
        public List<List<SelectiveEditorCell>> Bays { get; } = new List<List<SelectiveEditorCell>>();
        public List<bool> FloorBeams { get; } = new List<bool>();
        public List<double?> BayHeights { get; } = new List<double?>();
        public List<List<SelectiveSegment>> BaySegments { get; } = new List<List<SelectiveSegment>>();

        /// <summary>This fondo's pallet depth (in). Defaults to the shared default, mirroring the editor's field default.</summary>
        public double Depth { get; set; } = SelectiveRackDefaults.DefaultPalletDepth;

        /// <summary>Custom cabecera fondo for this fondo; 0 = auto (the rule tarima − allowance).</summary>
        public double CabeceraOverride { get; set; }
    }
}
