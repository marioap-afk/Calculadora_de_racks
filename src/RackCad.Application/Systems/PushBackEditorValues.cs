using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// Parsed edit buffer of the Push Back editor's per-cell panel. It COMPOSES the shared dynamic buffer
    /// (<see cref="Dynamic"/>) — every structural/pallet/beam field Push Back reuses unchanged — and ADDS only the two
    /// Push-Back-specific inputs: the high-end (rear) beam PERALTE and whether the rear pallet-stop tope is active for the
    /// cell. It never restates a dynamic field, so <see cref="DynamicEditorValues"/> stays the single source of those
    /// values and the matrix's apply/scope logic is reused verbatim.
    /// </summary>
    public sealed class PushBackEditorValues
    {
        /// <summary>The shared dynamic edit buffer (pallet, levels, fondos, beams, length override, ...). Never null.</summary>
        public DynamicEditorValues Dynamic { get; set; } = new DynamicEditorValues();

        /// <summary>High-end (rear) beam PERALTE requested for the cell (in); normalized against the catalog at build.</summary>
        public double HighEndBeamPeralte { get; set; } = PushBackDefaults.HighEndBeamDefaultPeralte;

        /// <summary>Whether the rear pallet-stop tope is active for the cell (Push Back defaults to active).</summary>
        public bool RearTopeEnabled { get; set; } = true;
    }
}
