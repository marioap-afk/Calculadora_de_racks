using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.FlowBed;
using RackCad.Domain.Systems.Larguero;
using RackCad.Domain.Systems.PushBack;

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

        // A rack whose every front is EN BLANCO carries nothing (I-33). It is rejected here, never normalized: the check
        // is DynamicFrontActivation.HasActiveFront — the same canonical predicate the resolver throws on — so the two
        // boundaries cannot drift. Legacy documents carry no flag and load every front active, so they pass unchanged.
        public static bool IsUsableDynamic(DynamicRackSystem system)
            => system != null && system.Modules != null && system.Modules.Count > 0
               && DynamicFrontActivation.HasActiveFront(system.Fronts);

        public static bool IsUsableDynamic(DynamicRackDesign design, DynamicRackSystem system)
            => design != null
               && design.Pallet != null
               && design.Pallet.Depth > 0.0
               && design.PalletsDeep >= 2
               && DynamicFrontActivation.HasActiveFront(design.Fronts)
               && IsUsableDynamic(system);

        // Push Back reuses the dynamic structure; a library load carries only the editable design (no resolved system).
        // Aligned with PushBackResolver.Validate: real pallet dimensions, >= 2 fondos, >= 1 load level and — since
        // I-33 — at least one front that is not en blanco.
        public static bool IsUsablePushBack(PushBackDesign design)
            => design?.Structure?.Pallet != null
               && design.Structure.Pallet.Front > 0.0
               && design.Structure.Pallet.Depth > 0.0
               && design.Structure.Pallet.Height > 0.0
               && design.Structure.PalletsDeep >= 2
               && design.Structure.LoadLevels >= 1
               && DynamicFrontActivation.HasActiveFront(design.Structure.Fronts);
    }
}
