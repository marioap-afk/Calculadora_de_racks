using RackCad.Domain.Systems;

namespace RackCad.Application.Persistence
{
    /// <summary>
    /// Serialization-friendly snapshot of a dynamic system: flat pallet fields, the number of
    /// pallets deep, the optional header depth override, and the header reused via the existing
    /// <see cref="RackFrameProjectDocument"/>. Derived data (layout, members) is rebuilt on load.
    /// </summary>
    public sealed class DynamicRackSystemDocument
    {
        public double PalletFront { get; set; }
        public double PalletDepth { get; set; }
        public double PalletHeight { get; set; }
        public double PalletWeight { get; set; }
        public string PalletWeightUnit { get; set; } = "kg";
        public int PalletsDeep { get; set; }
        public double? HeaderDepthOverride { get; set; }
        public RackFrameProjectDocument Header { get; set; }

        public static DynamicRackSystemDocument From(DynamicRackSystem system)
        {
            return new DynamicRackSystemDocument
            {
                PalletFront = system.Pallet?.Front ?? 0.0,
                PalletDepth = system.Pallet?.Depth ?? 0.0,
                PalletHeight = system.Pallet?.Height ?? 0.0,
                PalletWeight = system.Pallet?.Weight ?? 0.0,
                PalletWeightUnit = system.Pallet?.WeightUnit ?? "kg",
                PalletsDeep = system.PalletsDeep,
                HeaderDepthOverride = system.HeaderDepthOverride,
                Header = system.Header == null ? null : RackFrameProjectDocument.FromConfiguration(system.Header)
            };
        }

        public DynamicRackSystem ToDomain()
        {
            return new DynamicRackSystem
            {
                Kind = RackSystemKind.PalletFlow,
                Pallet = new PalletSpecification(
                    PalletFront,
                    PalletDepth,
                    PalletHeight,
                    PalletWeight,
                    string.IsNullOrWhiteSpace(PalletWeightUnit) ? "kg" : PalletWeightUnit),
                PalletsDeep = PalletsDeep,
                HeaderDepthOverride = HeaderDepthOverride,
                Header = Header?.ToConfiguration()
            };
        }
    }
}
