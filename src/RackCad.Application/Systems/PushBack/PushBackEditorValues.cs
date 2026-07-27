using RackCad.Application.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;

namespace RackCad.Application.Systems.PushBack
{
    /// <summary>
    /// Parsed edit buffer of the Push Back editor's per-cell panel. It COMPOSES the shared dynamic buffer
    /// (<see cref="Dynamic"/>) — every structural/pallet/beam field Push Back reuses unchanged — and ADDS only the one
    /// Push-Back-specific input: the high-end (rear) beam PERALTE. The rear pallet-stop tope is deliberately NOT here
    /// (Owner decision 2026-07-24): it is configured exclusively from Seguridad, so it must have no way to travel
    /// through a cell scope. It never restates a dynamic field, so <see cref="DynamicEditorValues"/> stays the single source of those
    /// values and the matrix's apply/scope logic is reused verbatim.
    /// </summary>
    public sealed class PushBackEditorValues
    {
        /// <summary>The shared dynamic edit buffer (pallet, levels, fondos, beams, length override, ...). Never null.</summary>
        public DynamicEditorValues Dynamic { get; set; } = new DynamicEditorValues();

        /// <summary>High-end (rear) beam PERALTE requested for the cell (in); normalized against the catalog at build.</summary>
        public double HighEndBeamPeralte { get; set; } = PushBackDefaults.HighEndBeamDefaultPeralte;

    }
}
