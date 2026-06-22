using System.Collections.Generic;
using System.Linq;
using RackCad.Domain.Systems;

namespace RackCad.Application.Persistence
{
    /// <summary>
    /// Serialization-friendly snapshot of a dynamic system: flat pallet fields, the number of
    /// pallets deep, and the editable module list. Each module carries its kind, length, override
    /// flag, notes and (for headers) its associated header configuration. Positions and members are
    /// rebuilt on load.
    /// </summary>
    public sealed class DynamicRackSystemDocument
    {
        public double PalletFront { get; set; }
        public double PalletDepth { get; set; }
        public double PalletHeight { get; set; }
        public double PalletWeight { get; set; }
        public string PalletWeightUnit { get; set; } = "kg";
        public int PalletsDeep { get; set; }
        public int? SeparatorCountOverride { get; set; }
        public double? SeparatorSpacingOverride { get; set; }
        public bool DerivedPostReinforced { get; set; } = true;
        public double? DerivedPostReinforcementHeight { get; set; }
        public List<DynamicRackModuleDocument> Modules { get; set; } = new List<DynamicRackModuleDocument>();

        public static DynamicRackSystemDocument From(DynamicRackSystem system)
        {
            var document = new DynamicRackSystemDocument
            {
                PalletFront = system.Pallet?.Front ?? 0.0,
                PalletDepth = system.Pallet?.Depth ?? 0.0,
                PalletHeight = system.Pallet?.Height ?? 0.0,
                PalletWeight = system.Pallet?.Weight ?? 0.0,
                PalletWeightUnit = system.Pallet?.WeightUnit ?? "kg",
                PalletsDeep = system.PalletsDeep,
                SeparatorCountOverride = system.SeparatorCountOverride,
                SeparatorSpacingOverride = system.SeparatorSpacingOverride,
                DerivedPostReinforced = system.DerivedPostReinforced,
                DerivedPostReinforcementHeight = system.DerivedPostReinforcementHeight
            };

            foreach (var module in system.Modules)
            {
                document.Modules.Add(DynamicRackModuleDocument.From(module));
            }

            return document;
        }

        public DynamicRackSystem ToDomain()
        {
            var system = new DynamicRackSystem
            {
                Kind = RackSystemKind.PalletFlow,
                Pallet = new PalletSpecification(
                    PalletFront,
                    PalletDepth,
                    PalletHeight,
                    PalletWeight,
                    string.IsNullOrWhiteSpace(PalletWeightUnit) ? "kg" : PalletWeightUnit),
                PalletsDeep = PalletsDeep,
                SeparatorCountOverride = SeparatorCountOverride,
                SeparatorSpacingOverride = SeparatorSpacingOverride,
                DerivedPostReinforced = DerivedPostReinforced,
                DerivedPostReinforcementHeight = DerivedPostReinforcementHeight
            };

            foreach (var module in Modules ?? Enumerable.Empty<DynamicRackModuleDocument>())
            {
                system.Modules.Add(module.ToDomain());
            }

            system.RecalculatePositions();
            return system;
        }
    }

    public sealed class DynamicRackModuleDocument
    {
        public string ModuleId { get; set; }
        public DynamicRackModuleKind Kind { get; set; }
        public double Length { get; set; }
        public bool IsCalculated { get; set; } = true;
        public bool IsManualOverride { get; set; }
        public string Notes { get; set; }
        public RackFrameProjectDocument Header { get; set; }

        public static DynamicRackModuleDocument From(DynamicRackModule module)
        {
            return new DynamicRackModuleDocument
            {
                ModuleId = module.ModuleId,
                Kind = module.Kind,
                Length = module.Length,
                IsCalculated = module.IsCalculated,
                IsManualOverride = module.IsManualOverride,
                Notes = module.Notes,
                Header = module.AssociatedFrameConfiguration == null
                    ? null
                    : RackFrameProjectDocument.FromConfiguration(module.AssociatedFrameConfiguration)
            };
        }

        public DynamicRackModule ToDomain()
        {
            return new DynamicRackModule
            {
                ModuleId = ModuleId,
                Kind = Kind,
                Length = Length,
                IsCalculated = IsCalculated,
                IsManualOverride = IsManualOverride,
                Notes = Notes,
                AssociatedFrameConfiguration = Header?.ToConfiguration()
            };
        }
    }
}
