using System;
using System.Linq;
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

            var frontDesigns = design.Fronts.Where(front => front != null).ToList();
            var depthLayout = DynamicDepthGeometry.Resolve(frontDesigns, design.PalletsDeep);
            var inOutBeamId = string.IsNullOrWhiteSpace(design.InOutBeamCatalogId)
                ? DynamicRackDefaults.InOutBeamCatalogId
                : design.InOutBeamCatalogId;
            var beamDepth = DynamicLoadBeamGeometry.ResolveBeamDepth(catalog, inOutBeamId, design.BeamDepth);
            var resolvedLoadLevels = Math.Max(
                design.LoadLevels,
                design.Fronts.Where(front => front?.LoadLevels > 0)
                    .Select(front => front.LoadLevels.Value)
                    .DefaultIfEmpty(design.LoadLevels)
                    .Max());
            var postId = string.IsNullOrWhiteSpace(design.HeaderPostCatalogId)
                ? catalog.Defaults?.Post
                : design.HeaderPostCatalogId;
            var postPeralte = ResolvePostPeralte(design, postId);
            var troquelEntry = catalog.ConnectionLayout.FindConnectionLayout(
                postId,
                SelectiveRackDefaults.PostBeamPoint,
                "FRONTAL");
            var troquelGridBase = SelectivePostGeometry.Resolve(
                troquelEntry,
                new System.Collections.Generic.Dictionary<string, double>
                {
                    [SelectiveRackDefaults.PeralteParam] = postPeralte
                }).Y;
            var palletTolerance = design.PalletTolerance > 0.0
                ? design.PalletTolerance
                : DynamicRackDefaults.DefaultPalletTolerance;
            var resolvedFronts = DynamicFrontGeometry.Resolve(
                frontDesigns,
                design.Pallet.Front,
                palletTolerance,
                resolvedLoadLevels,
                design.PalletsDeep);
            for (var index = 0; index < resolvedFronts.Count; index++)
            {
                var source = index < frontDesigns.Count ? frontDesigns[index] : null;
                DynamicRackLevelGeometry.Resolve(design, source, resolvedFronts[index], catalog);
            }

            DynamicHeaderHeightResult height = null;
            foreach (var front in resolvedFronts)
            {
                var frontDepth = front.PalletsDeep * design.Pallet.Depth
                                 + 2.0 * DynamicRackDefaults.HeaderEndAllowance;
                var levels = DynamicLoadBeamGeometry.ResolveLevels(
                    front.Levels.ToList(),
                    front.FirstLevelHeight,
                    DynamicHeaderHeightCalculator.CalculateSlope(frontDepth),
                    troquelGridBase,
                    SelectiveRackDefaults.TroquelPaso);
                foreach (var level in levels)
                {
                    front.LoadBeamLevels.Add(level);
                }

                var frontHeight = DynamicHeaderHeightCalculator.CalculateResolved(front);
                front.Height = design.ManualHeaderHeightOverride ?? frontHeight.HeaderHeight;
                if (height == null || frontHeight.HeaderHeight > height.HeaderHeight)
                {
                    height = frontHeight;
                }
            }

            height ??= DynamicHeaderHeightCalculator.Calculate(
                design.Pallet.Height,
                resolvedLoadLevels,
                design.FirstLevelHeight,
                beamDepth,
                depthLayout.TotalPositions * design.Pallet.Depth
                + 2.0 * DynamicRackDefaults.HeaderEndAllowance);
            var resolvedHeight = design.ManualHeaderHeightOverride
                                 ?? resolvedFronts.Select(front => front.Height).DefaultIfEmpty(height.HeaderHeight).Max();

            DynamicRackSystem system;
            if (design.Modules.Count == 0 || design.Modules.Count != depthLayout.TotalPositions)
            {
                system = builder.BuildDefault(
                    CopyPallet(design.Pallet),
                    depthLayout,
                    RackFrameTemplateCatalog.Default,
                    postId,
                    resolvedHeight,
                    postPeralte);
            }
            else
            {
                system = new DynamicRackSystem
                {
                    Kind = RackSystemKind.PalletFlow,
                    Pallet = CopyPallet(design.Pallet),
                    PalletsDeep = depthLayout.TotalPositions,
                    BaseDepthStartPosition = depthLayout.BaseRange.StartPosition,
                    BasePalletsDeep = depthLayout.BaseRange.PalletsDeep,
                    PostPeralte = postPeralte
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
                            ? builder.BuildHeaderConfiguration(
                                RackFrameTemplateCatalog.Default,
                                postId,
                                resolvedHeight,
                                module.Length,
                                postPeralte)
                            : CloneHeader(moduleDesign.HeaderConfiguration);
                        module.AssociatedFrameConfiguration.PostPeralte = postPeralte;
                    }

                    system.Modules.Add(module);
                }

                builder.Refresh(system);
            }

            builder.ApplyPostPeralte(system, postPeralte);

            system.SeparatorCountOverride = design.SeparatorCountOverride;
            system.SeparatorSpacingOverride = design.SeparatorSpacingOverride;
            system.DerivedPostReinforced = design.DerivedPostReinforced;
            system.DerivedPostReinforcementHeight = design.DerivedPostReinforcementHeight;
            system.ManualHeaderHeightOverride = design.ManualHeaderHeightOverride;
            system.NumberFronts = design.NumberFronts;
            system.NumberLevels = design.NumberLevels;
            system.DrawRackName = design.DrawRackName;
            system.AnnotationScale = design.AnnotationScale > 0.0 ? design.AnnotationScale : 1.0;
            system.Dimensions = design.Dimensions;
            system.DimensionStyle = design.DimensionStyle;
            system.PalletTolerance = palletTolerance;
            system.InOutBeamCatalogId = inOutBeamId;
            system.InOutBeamDepth = beamDepth;
            foreach (var peralte in DynamicIntermediateBeamGeometry.ResolvePeraltes(
                         catalog,
                         design.IntermediateBeamDepths,
                         resolvedLoadLevels))
            {
                system.IntermediateBeamDepths.Add(peralte);
            }
            for (var index = 0; index < resolvedFronts.Count; index++)
            {
                var front = resolvedFronts[index];
                system.Fronts.Add(front);
            }
            DynamicDepthGeometry.ResolveCoordinates(system);
            var projectedLevels = resolvedFronts
                .OrderByDescending(front => front.LoadBeamLevels.Count)
                .ThenByDescending(front => front.EndX - front.StartX)
                .FirstOrDefault()?.LoadBeamLevels;
            foreach (var level in projectedLevels ?? Enumerable.Empty<DynamicLoadBeamLevel>())
            {
                system.LoadBeamLevels.Add(level);
            }

            foreach (var safety in design.SafetySelections)
            {
                if (safety != null)
                {
                    system.SafetySelections.Add(safety.DeepCopy());
                }
            }

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
                BeamDepth = system.InOutBeamDepth > 0.0 ? system.InOutBeamDepth : beamDepth,
                InOutBeamCatalogId = string.IsNullOrWhiteSpace(system.InOutBeamCatalogId)
                    ? DynamicRackDefaults.InOutBeamCatalogId
                    : system.InOutBeamCatalogId,
                HeaderPostCatalogId = headerPostCatalogId,
                PostPeralte = system.PostPeralte > 0.0
                    ? system.PostPeralte
                    : DynamicFrontGeometry.PostPeralte(system, catalog, headerPostCatalogId),
                SeparatorCountOverride = system.SeparatorCountOverride,
                SeparatorSpacingOverride = system.SeparatorSpacingOverride,
                DerivedPostReinforced = system.DerivedPostReinforced,
                DerivedPostReinforcementHeight = system.DerivedPostReinforcementHeight,
                ManualHeaderHeightOverride = system.ManualHeaderHeightOverride
            };

            design.NumberFronts = system.NumberFronts;
            design.NumberLevels = system.NumberLevels;
            design.DrawRackName = system.DrawRackName;
            design.AnnotationScale = system.AnnotationScale > 0.0 ? system.AnnotationScale : 1.0;
            design.Dimensions = system.Dimensions;
            design.DimensionStyle = system.DimensionStyle;

            foreach (var peralte in system.IntermediateBeamDepths)
            {
                design.IntermediateBeamDepths.Add(peralte);
            }

            design.PalletTolerance = system.PalletTolerance > 0.0
                ? system.PalletTolerance
                : DynamicRackDefaults.DefaultPalletTolerance;

            foreach (var front in system.Fronts)
            {
                if (front != null)
                {
                    var frontDesign = new DynamicRackFrontDesign
                    {
                        PalletCount = front.PalletCount,
                        LoadLevels = front.LoadLevels,
                        PalletsDeep = front.PalletsDeep,
                        DepthStartPosition = front.DepthStartPosition,
                        BeamLengthOverride = front.BeamLengthOverride,
                        FirstLevelHeight = front.FirstLevelHeight
                    };
                    foreach (var peralte in front.IntermediateBeamDepths)
                    {
                        frontDesign.IntermediateBeamDepths.Add(peralte);
                    }
                    foreach (var level in front.Levels)
                    {
                        frontDesign.Levels.Add(new DynamicRackLevelDesign
                        {
                            PalletFront = level.Pallet?.Front,
                            PalletHeight = level.Pallet?.Height,
                            PalletWeight = level.Pallet?.Weight,
                            ClearHeight = level.ClearHeight,
                            InOutBeamCatalogId = level.InOutBeamCatalogId,
                            InOutBeamDepth = level.InOutBeamDepth,
                            BeamLengthOverride = level.BeamLengthOverride,
                            IntermediateBeamCatalogId = level.IntermediateBeamCatalogId,
                            IntermediateBeamDepth = level.IntermediateBeamDepth
                        });
                    }
                    design.Fronts.Add(frontDesign);
                }
            }

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

            foreach (var safety in system.SafetySelections)
            {
                if (safety != null)
                {
                    design.SafetySelections.Add(safety.DeepCopy());
                }
            }

            return design;
        }

        private double ResolvePostPeralte(DynamicRackDesign design, string postId)
        {
            if (design.PostPeralte > 0.0)
            {
                return design.PostPeralte;
            }

            var persisted = design.Modules
                .Where(module => module?.IsHeader == true && module.HeaderConfiguration?.PostPeralte > 0.0)
                .Select(module => module.HeaderConfiguration.PostPeralte)
                .FirstOrDefault();
            if (persisted > 0.0)
            {
                return persisted;
            }

            var width = catalog.PostProfiles.FirstOrDefault(profile => string.Equals(
                profile?.Id,
                postId,
                StringComparison.OrdinalIgnoreCase))?.Width ?? 0.0;
            return width > 0.0 ? width : DynamicRackDefaults.DefaultPostPeralte;
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
            if (design.PostPeralte < 0.0)
                throw new ArgumentException("El peralte de poste no puede ser negativo.", nameof(design));
            if (design.ManualHeaderHeightOverride.HasValue && design.ManualHeaderHeightOverride.Value <= 0.0)
                throw new ArgumentException("La altura manual debe ser mayor que cero.", nameof(design));
            if (design.PalletTolerance <= 0.0)
                throw new ArgumentException("La holgura transversal debe ser mayor que cero.", nameof(design));
            foreach (var front in design.Fronts)
            {
                if (front == null || front.PalletCount < 1)
                    throw new ArgumentException("Cada frente requiere al menos una posición de tarima.", nameof(design));
                if (front.LoadLevels.HasValue && front.LoadLevels.Value < 1)
                    throw new ArgumentException("Cada frente requiere al menos un nivel de carga.", nameof(design));
                if (front.PalletsDeep.HasValue && front.PalletsDeep.Value < 2)
                    throw new ArgumentException("Cada frente requiere al menos 2 fondos.", nameof(design));
                if (front.DepthStartPosition.HasValue && front.DepthStartPosition.Value < 1)
                    throw new ArgumentException("La posición inicial de fondo debe ser >= 1.", nameof(design));
                if (front.BeamLengthOverride.HasValue && front.BeamLengthOverride.Value <= 0.0)
                    throw new ArgumentException("El largo manual del frente debe ser mayor que cero.", nameof(design));
                if (front.FirstLevelHeight.HasValue && front.FirstLevelHeight.Value < 0.0)
                    throw new ArgumentException("El inicio del primer larguero no puede ser negativo.", nameof(design));
                foreach (var level in front.Levels)
                {
                    if (level == null)
                        continue;
                    if (level.PalletFront.HasValue && level.PalletFront.Value <= 0.0
                        || level.PalletHeight.HasValue && level.PalletHeight.Value <= 0.0
                        || level.PalletWeight.HasValue && level.PalletWeight.Value < 0.0
                        || level.ClearHeight.HasValue && level.ClearHeight.Value < 0.0
                        || level.InOutBeamDepth.HasValue && level.InOutBeamDepth.Value <= 0.0
                        || level.IntermediateBeamDepth.HasValue && level.IntermediateBeamDepth.Value <= 0.0
                        || level.BeamLengthOverride.HasValue && level.BeamLengthOverride.Value <= 0.0)
                        throw new ArgumentException("Los datos de una celda dinámica son inválidos.", nameof(design));
                }
            }
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
