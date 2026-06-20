using RackCad.Domain.RackFrames;

namespace RackCad.Domain.Systems
{
    /// <summary>
    /// Aggregate (source of truth) for a dynamic (pallet flow) system. Holds the pallet spec, the
    /// number of pallets deep, and a single shared header configuration used at both ends
    /// (symmetric for now). The header's depth is driven by the system:
    /// <see cref="EffectiveHeaderDepth"/> = override, or pallet depth + 6". Modules and total
    /// length are derived (not stored) by the Application layer.
    /// </summary>
    public sealed class DynamicRackSystem
    {
        public RackSystemKind Kind { get; set; } = RackSystemKind.PalletFlow;
        public PalletSpecification Pallet { get; set; } = new PalletSpecification();
        public int PalletsDeep { get; set; }

        /// <summary>End frame reused as a component. Shared by the start and end (symmetric).</summary>
        public RackFrameConfiguration Header { get; set; }

        /// <summary>Optional manual override of the header depth; null means derive depth + 6".</summary>
        public double? HeaderDepthOverride { get; set; }

        /// <summary>Header depth the system imposes: the override when set, otherwise pallet depth + 6".</summary>
        public double EffectiveHeaderDepth =>
            HeaderDepthOverride ?? ((Pallet?.Depth ?? 0.0) + DynamicRackDefaults.HeaderEndAllowance);
    }
}
