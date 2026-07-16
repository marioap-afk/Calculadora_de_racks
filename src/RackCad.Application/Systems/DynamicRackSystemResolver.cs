using System;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.RackFrames;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>Resolved pallet-flow geometry plus the height calculation that produced its standard cabeceras.</summary>
    public sealed class DynamicRackResolution
    {
        public DynamicRackResolution(DynamicRackSystem system, DynamicHeaderHeightResult height)
        {
            System = system;
            Height = height;
        }

        public DynamicRackSystem System { get; }
        public DynamicHeaderHeightResult Height { get; }
    }

    /// <summary>
    /// Pure design-to-system boundary for pallet flow. UI, persistence, drawing and BOM can share the same resolved
    /// model without persisting calculated coordinates. Existing custom cabeceras are copied; calculated ones are
    /// regenerated from the current design inputs.
    /// </summary>
    public sealed class DynamicRackSystemResolver
    {
        private readonly RackCatalog catalog;
        private readonly DynamicRackSystemBuilder builder;

        public DynamicRackSystemResolver(RackCatalog catalog)
        {
            this.catalog = catalog ?? new RackCatalog();
            builder = new DynamicRackSystemBuilder(this.catalog);
        }

        public DynamicRackResolution Resolve(DynamicRackDesign design)
        {
            Validate(design);

            var totalDepth = design.PalletsDeep * design.Pallet.Depth
                             + 2.0 * DynamicRackDefaults.HeaderEndAllowance;
            var height = DynamicHeaderHeightCalculator.Calculate(
                design.Pallet.Height,
                design.LoadLevels,
                design.FirstLevelHeight,
                design.BeamDepth,
                totalDepth);
            var resolvedHeight = design.ManualHeaderHeightOverride ?? height.HeaderHeight;
            var postId = string.IsNullOrWhiteSpace(design.HeaderPostCatalogId)
                ? catalog.Defaults?.Post
                : design.HeaderPostCatalogId;

            DynamicRackSystem system;
            if (design.Modules.Count == 0)
            {
                system = builder.BuildDefault(
                    CopyPallet(design.Pallet),
                    design.PalletsDeep,
                    RackFrameTemplateCatalog.Default,
                    postId,
                    resolvedHeight);
            }
            else
            {
                system = new DynamicRackSystem
                {
                    Kind = RackSystemKind.PalletFlow,
                    Pallet = CopyPallet(design.Pallet),
                    PalletsDeep = design.PalletsDeep
                };

                foreach (var moduleDesign in design.Modules)
                {
                    if (moduleDesign == null || moduleDesign.Length < 0.0)
                    {
                        continue;
                    }

                    var module = new DynamicRackModule
                    {
                        ModuleId = moduleDesign.ModuleId,
                        Kind = moduleDesign.Kind,
                        Length = moduleDesign.Length,
                        IsCalculated = moduleDesign.IsCalculated,
                        IsManualOverride = moduleDesign.IsManualOverride,
                        UseCalculatedHeaderConfiguration = moduleDesign.UseCalculatedHeaderConfiguration,
                        Notes = moduleDesign.Notes
                    };

                    if (module.IsHeader)
                    {
                        module.AssociatedFrameConfiguration = moduleDesign.UseCalculatedHeaderConfiguration
                            || moduleDesign.HeaderConfiguration == null
                            ? builder.BuildHeaderConfiguration(RackFrameTemplateCatalog.Default, postId, resolvedHeight, module.Length)
                            : CloneHeader(moduleDesign.HeaderConfiguration);
                    }

                    system.Modules.Add(module);
                }

                builder.Refresh(system);
            }

            system.SeparatorCountOverride = design.SeparatorCountOverride;
            system.SeparatorSpacingOverride = design.SeparatorSpacingOverride;
            system.DerivedPostReinforced = design.DerivedPostReinforced;
            system.DerivedPostReinforcementHeight = design.DerivedPostReinforcementHeight;
            system.ManualHeaderHeightOverride = design.ManualHeaderHeightOverride;
            return new DynamicRackResolution(system, height);
        }

        /// <summary>
        /// Captures the editable intent of an existing resolved system. UI calls this after committing its fields; the
        /// returned design owns independent header configurations and can safely be persisted or resolved again.
        /// </summary>
        public DynamicRackDesign Snapshot(
            DynamicRackSystem system,
            int loadLevels,
            double firstLevelHeight,
            double beamDepth,
            string headerPostCatalogId)
        {
            if (system == null)
            {
                throw new ArgumentNullException(nameof(system));
            }

            var design = new DynamicRackDesign
            {
                Pallet = CopyPallet(system.Pallet),
                PalletsDeep = system.PalletsDeep,
                LoadLevels = loadLevels,
                FirstLevelHeight = firstLevelHeight,
                BeamDepth = beamDepth,
                HeaderPostCatalogId = headerPostCatalogId,
                SeparatorCountOverride = system.SeparatorCountOverride,
                SeparatorSpacingOverride = system.SeparatorSpacingOverride,
                DerivedPostReinforced = system.DerivedPostReinforced,
                DerivedPostReinforcementHeight = system.DerivedPostReinforcementHeight,
                ManualHeaderHeightOverride = system.ManualHeaderHeightOverride
            };

            foreach (var module in system.Modules)
            {
                design.Modules.Add(new DynamicRackModuleDesign
                {
                    ModuleId = module.ModuleId,
                    Kind = module.Kind,
                    Length = module.Length,
                    IsCalculated = module.IsCalculated,
                    IsManualOverride = module.IsManualOverride,
                    UseCalculatedHeaderConfiguration = module.UseCalculatedHeaderConfiguration,
                    HeaderConfiguration = CloneHeader(module.AssociatedFrameConfiguration),
                    Notes = module.Notes
                });
            }

            return design;
        }

        private static void Validate(DynamicRackDesign design)
        {
            if (design == null) throw new ArgumentNullException(nameof(design));
            if (design.Pallet == null) throw new ArgumentException("El diseño dinámico no tiene tarima.", nameof(design));
            if (design.Pallet.Front <= 0.0 || design.Pallet.Depth <= 0.0 || design.Pallet.Height <= 0.0)
                throw new ArgumentException("Las dimensiones de la tarima deben ser mayores que cero.", nameof(design));
            if (design.PalletsDeep < 2)
                throw new ArgumentException("Se requieren al menos 2 tarimas de fondo.", nameof(design));
            if (design.LoadLevels < 1)
                throw new ArgumentException("Se requiere al menos un nivel de carga.", nameof(design));
            if (design.FirstLevelHeight < 0.0 || design.BeamDepth < 0.0)
                throw new ArgumentException("Las medidas verticales no pueden ser negativas.", nameof(design));
            if (design.ManualHeaderHeightOverride.HasValue && design.ManualHeaderHeightOverride.Value <= 0.0)
                throw new ArgumentException("La altura manual debe ser mayor que cero.", nameof(design));
        }

        private static PalletSpecification CopyPallet(PalletSpecification pallet)
            => pallet == null
                ? new PalletSpecification()
                : new PalletSpecification(pallet.Front, pallet.Depth, pallet.Height, pallet.Weight, pallet.WeightUnit);

        private static RackCad.Domain.RackFrames.RackFrameConfiguration CloneHeader(
            RackCad.Domain.RackFrames.RackFrameConfiguration configuration)
            => configuration == null
                ? null
                : RackFrameProjectDocument.FromConfiguration(configuration).ToConfiguration();
    }
}
