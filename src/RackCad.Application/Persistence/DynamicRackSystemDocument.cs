using System.Collections.Generic;
using System.Linq;
using RackCad.Domain.Systems;

namespace RackCad.Application.Persistence
{
    /// <summary>
    /// Version-tolerant dynamic-design DTO. Legacy files contain only the resolved-system fields; nullable design
    /// inputs fall back to the historical UI defaults. Positions and physical members are always rebuilt on load.
    /// </summary>
    public sealed class DynamicRackSystemDocument
    {
        public double PalletFront { get; set; }
        public double PalletDepth { get; set; }
        public double PalletHeight { get; set; }
        public double PalletWeight { get; set; }
        public string PalletWeightUnit { get; set; } = "kg";
        public int PalletsDeep { get; set; }
        public int? LoadLevels { get; set; }
        public double? FirstLevelHeight { get; set; }
        public double? BeamDepth { get; set; }
        public string HeaderPostCatalogId { get; set; }
        public int? SeparatorCountOverride { get; set; }
        public double? SeparatorSpacingOverride { get; set; }
        public bool DerivedPostReinforced { get; set; } = true;
        public double? DerivedPostReinforcementHeight { get; set; }
        public double? ManualHeaderHeightOverride { get; set; }
        public List<DynamicRackModuleDocument> Modules { get; set; } = new List<DynamicRackModuleDocument>();

        public static DynamicRackSystemDocument From(DynamicRackSystem system)
        {
            var postId = system?.Modules?.FirstOrDefault(m => m.IsHeader)?.AssociatedFrameConfiguration?.LeftPost?.PostCatalogId;
            var document = new DynamicRackSystemDocument
            {
                PalletFront = system.Pallet?.Front ?? 0.0,
                PalletDepth = system.Pallet?.Depth ?? 0.0,
                PalletHeight = system.Pallet?.Height ?? 0.0,
                PalletWeight = system.Pallet?.Weight ?? 0.0,
                PalletWeightUnit = system.Pallet?.WeightUnit ?? "kg",
                PalletsDeep = system.PalletsDeep,
                LoadLevels = DynamicRackDefaults.DefaultLoadLevels,
                FirstLevelHeight = DynamicRackDefaults.DefaultFirstLevelHeight,
                BeamDepth = DynamicRackDefaults.DefaultBeamDepth,
                HeaderPostCatalogId = postId,
                SeparatorCountOverride = system.SeparatorCountOverride,
                SeparatorSpacingOverride = system.SeparatorSpacingOverride,
                DerivedPostReinforced = system.DerivedPostReinforced,
                DerivedPostReinforcementHeight = system.DerivedPostReinforcementHeight,
                ManualHeaderHeightOverride = system.ManualHeaderHeightOverride
            };

            foreach (var module in system.Modules)
            {
                document.Modules.Add(DynamicRackModuleDocument.From(module));
            }

            return document;
        }

        public static DynamicRackSystemDocument From(DynamicRackDesign design)
        {
            var document = new DynamicRackSystemDocument
            {
                PalletFront = design.Pallet?.Front ?? 0.0,
                PalletDepth = design.Pallet?.Depth ?? 0.0,
                PalletHeight = design.Pallet?.Height ?? 0.0,
                PalletWeight = design.Pallet?.Weight ?? 0.0,
                PalletWeightUnit = design.Pallet?.WeightUnit ?? "kg",
                PalletsDeep = design.PalletsDeep,
                LoadLevels = design.LoadLevels,
                FirstLevelHeight = design.FirstLevelHeight,
                BeamDepth = design.BeamDepth,
                HeaderPostCatalogId = design.HeaderPostCatalogId,
                SeparatorCountOverride = design.SeparatorCountOverride,
                SeparatorSpacingOverride = design.SeparatorSpacingOverride,
                DerivedPostReinforced = design.DerivedPostReinforced,
                DerivedPostReinforcementHeight = design.DerivedPostReinforcementHeight,
                ManualHeaderHeightOverride = design.ManualHeaderHeightOverride
            };

            foreach (var module in design.Modules)
            {
                document.Modules.Add(DynamicRackModuleDocument.From(module));
            }

            return document;
        }

        public DynamicRackDesign ToDesign()
        {
            var design = new DynamicRackDesign
            {
                Pallet = new PalletSpecification(
                    PalletFront,
                    PalletDepth,
                    PalletHeight,
                    PalletWeight,
                    string.IsNullOrWhiteSpace(PalletWeightUnit) ? "kg" : PalletWeightUnit),
                PalletsDeep = PalletsDeep,
                LoadLevels = LoadLevels ?? DynamicRackDefaults.DefaultLoadLevels,
                FirstLevelHeight = FirstLevelHeight ?? DynamicRackDefaults.DefaultFirstLevelHeight,
                BeamDepth = BeamDepth ?? DynamicRackDefaults.DefaultBeamDepth,
                HeaderPostCatalogId = HeaderPostCatalogId,
                SeparatorCountOverride = SeparatorCountOverride,
                SeparatorSpacingOverride = SeparatorSpacingOverride,
                DerivedPostReinforced = DerivedPostReinforced,
                DerivedPostReinforcementHeight = DerivedPostReinforcementHeight,
                ManualHeaderHeightOverride = ManualHeaderHeightOverride
            };

            foreach (var module in Modules ?? Enumerable.Empty<DynamicRackModuleDocument>())
            {
                design.Modules.Add(module.ToDesign());
            }

            return design;
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
                DerivedPostReinforcementHeight = DerivedPostReinforcementHeight,
                ManualHeaderHeightOverride = ManualHeaderHeightOverride
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
        public bool? UseCalculatedHeaderConfiguration { get; set; }
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
                UseCalculatedHeaderConfiguration = module.UseCalculatedHeaderConfiguration,
                Notes = module.Notes,
                Header = module.AssociatedFrameConfiguration == null
                    ? null
                    : RackFrameProjectDocument.FromConfiguration(module.AssociatedFrameConfiguration)
            };
        }

        public static DynamicRackModuleDocument From(DynamicRackModuleDesign module)
        {
            return new DynamicRackModuleDocument
            {
                ModuleId = module.ModuleId,
                Kind = module.Kind,
                Length = module.Length,
                IsCalculated = module.IsCalculated,
                IsManualOverride = module.IsManualOverride,
                UseCalculatedHeaderConfiguration = module.UseCalculatedHeaderConfiguration,
                Notes = module.Notes,
                Header = module.HeaderConfiguration == null
                    ? null
                    : RackFrameProjectDocument.FromConfiguration(module.HeaderConfiguration)
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
                // Legacy documents had no provenance flag, and an advanced cabecera edit did not necessarily set
                // IsManualOverride. Preserve every persisted header as custom; the user can explicitly restore the
                // calculated preset. Separators have no Header and keep the harmless calculated default.
                UseCalculatedHeaderConfiguration = UseCalculatedHeaderConfiguration ?? (Header == null),
                Notes = Notes,
                AssociatedFrameConfiguration = Header?.ToConfiguration()
            };
        }

        public DynamicRackModuleDesign ToDesign()
        {
            return new DynamicRackModuleDesign
            {
                ModuleId = ModuleId,
                Kind = Kind,
                Length = Length,
                IsCalculated = IsCalculated,
                IsManualOverride = IsManualOverride,
                UseCalculatedHeaderConfiguration = UseCalculatedHeaderConfiguration ?? (Header == null),
                Notes = Notes,
                HeaderConfiguration = Header?.ToConfiguration()
            };
        }
    }
}
