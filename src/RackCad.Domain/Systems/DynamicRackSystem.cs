using RackCad.Domain.RackFrames;

namespace RackCad.Domain.Systems
{
    /// <summary>
    /// Aggregate (source of truth) for a dynamic (pallet flow) system. Holds the pallet spec, the
    /// number of pallets deep, and a single shared header configuration used at both ends
    /// (symmetric). The header depth is fully pallet-driven: <see cref="EffectiveHeaderDepth"/> =
    /// pallet depth + 6", which is exactly the length of the first/last module. Modules, posts and
    /// total length are derived (not stored) by the Application layer.
    /// </summary>
    public sealed class DynamicRackSystem
    {
        public RackSystemKind Kind { get; set; } = RackSystemKind.PalletFlow;
        public PalletSpecification Pallet { get; set; } = new PalletSpecification();
        public int PalletsDeep { get; set; }

        /// <summary>End frame reused as a component. Shared by the start and end (symmetric).</summary>
        public RackFrameConfiguration Header { get; set; }

        /// <summary>Header depth the system imposes: pallet depth + 6" (= the first/last module length).</summary>
        public double EffectiveHeaderDepth => (Pallet?.Depth ?? 0.0) + DynamicRackDefaults.HeaderEndAllowance;
    }
}
