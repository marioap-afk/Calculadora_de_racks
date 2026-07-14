using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems;

namespace RackCad.Application.Persistence
{
    /// <summary>
    /// Minimum-content checks that tell a REAL design from a degenerate one — an empty/near-empty document that
    /// deserializes without error but would draw nothing (a header with height 0, a selective with no frentes, a
    /// cama with length 0…). The stores use these to reject junk with a clear message (or, for the tolerant flow-bed
    /// store, to treat it as absent) instead of silently producing a broken rack. Pure predicates: no exceptions, no I/O.
    /// </summary>
    public static class RackDesignValidation
    {
        public static bool IsUsableHeader(RackFrameConfiguration header)
            => header != null && header.Height > 0.0 && header.Depth > 0.0
               && header.LeftPost != null && header.RightPost != null;

        public static bool IsUsableSelective(SelectivePalletDesignDocument design)
            => design != null && design.Bays != null && design.Bays.Count > 0;

        // Only LaneDepth (the rail length) is required. PalletDepth is meaningful ONLY for Dynamic beds — Pushback beds
        // legitimately have PalletDepth 0, so requiring it would wrongly reject a whole valid bed sub-type.
        public static bool IsUsableFlowBed(FlowBedConfiguration bed)
            => bed != null && bed.LaneDepth > 0.0;

        // Only the profile id is required: it is null on a "{}" larguero and always set on a real one. Length/peralte
        // are NOT required — a real larguero could legitimately be mid-edit with a 0 there, and rejecting it would hide
        // it from the library.
        public static bool IsUsableLarguero(LargueroDesign larguero)
            => larguero != null && !string.IsNullOrWhiteSpace(larguero.BeamProfileId);

        public static bool IsUsableDynamic(DynamicRackSystem system)
            => system != null && system.Modules != null && system.Modules.Count > 0;
    }
}
