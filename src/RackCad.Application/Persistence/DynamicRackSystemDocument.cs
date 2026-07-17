using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Systems;
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
        public List<double> IntermediateBeamDepths { get; set; }
        public double? PalletTolerance { get; set; }
        public string InOutBeamCatalogId { get; set; }
        public string HeaderPostCatalogId { get; set; }
        public double? PostPeralte { get; set; }
        public int? SeparatorCountOverride { get; set; }
        public double? SeparatorSpacingOverride { get; set; }
        public bool DerivedPostReinforced { get; set; } = true;
        public double? DerivedPostReinforcementHeight { get; set; }
        public double? ManualHeaderHeightOverride { get; set; }
        public bool? NumberFronts { get; set; }
        public bool? NumberLevels { get; set; }
        public bool? DrawRackName { get; set; }
        public double? AnnotationScale { get; set; }
        public int? Dimensions { get; set; }
        public string DimensionStyle { get; set; }
        public List<SafetySelectionDocument> SafetySelections { get; set; }
        public List<DynamicRackFrontDocument> Fronts { get; set; }
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
                LoadLevels = system.LoadBeamLevels.Count > 0
                    ? system.LoadBeamLevels.Count
                    : system.Fronts.Where(front => front != null)
                        .Select(front => front.LoadLevels)
                        .DefaultIfEmpty(DynamicRackDefaults.DefaultLoadLevels)
                        .Max(),
                FirstLevelHeight = DynamicRackDefaults.DefaultFirstLevelHeight,
                BeamDepth = system.InOutBeamDepth > 0.0 ? system.InOutBeamDepth : DynamicRackDefaults.DefaultBeamDepth,
                IntermediateBeamDepths = system.IntermediateBeamDepths.Where(value => value > 0.0).ToList(),
                PalletTolerance = system.PalletTolerance > 0.0
                    ? system.PalletTolerance
                    : DynamicRackDefaults.DefaultPalletTolerance,
                InOutBeamCatalogId = string.IsNullOrWhiteSpace(system.InOutBeamCatalogId)
                    ? DynamicRackDefaults.InOutBeamCatalogId
                    : system.InOutBeamCatalogId,
                HeaderPostCatalogId = postId,
                PostPeralte = system.PostPeralte > 0.0
                    ? system.PostPeralte
                    : system.Modules.FirstOrDefault(module => module.IsHeader
                        && module.AssociatedFrameConfiguration?.PostPeralte > 0.0)?
                        .AssociatedFrameConfiguration.PostPeralte,
                SeparatorCountOverride = system.SeparatorCountOverride,
                SeparatorSpacingOverride = system.SeparatorSpacingOverride,
                DerivedPostReinforced = system.DerivedPostReinforced,
                DerivedPostReinforcementHeight = system.DerivedPostReinforcementHeight,
                ManualHeaderHeightOverride = system.ManualHeaderHeightOverride,
                NumberFronts = system.NumberFronts,
                NumberLevels = system.NumberLevels,
                DrawRackName = system.DrawRackName,
                AnnotationScale = system.AnnotationScale,
                Dimensions = (int)system.Dimensions,
                DimensionStyle = system.DimensionStyle,
                SafetySelections = system.SafetySelections.Where(s => s != null).Select(SafetySelectionDocument.From).ToList()
            };

            document.Fronts = system.Fronts
                .Where(front => front != null)
                .Select(DynamicRackFrontDocument.From)
                .ToList();

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
                IntermediateBeamDepths = design.IntermediateBeamDepths.Where(value => value > 0.0).ToList(),
                PalletTolerance = design.PalletTolerance,
                InOutBeamCatalogId = design.InOutBeamCatalogId,
                HeaderPostCatalogId = design.HeaderPostCatalogId,
                PostPeralte = design.PostPeralte > 0.0 ? design.PostPeralte : (double?)null,
                SeparatorCountOverride = design.SeparatorCountOverride,
                SeparatorSpacingOverride = design.SeparatorSpacingOverride,
                DerivedPostReinforced = design.DerivedPostReinforced,
                DerivedPostReinforcementHeight = design.DerivedPostReinforcementHeight,
                ManualHeaderHeightOverride = design.ManualHeaderHeightOverride,
                NumberFronts = design.NumberFronts,
                NumberLevels = design.NumberLevels,
                DrawRackName = design.DrawRackName,
                AnnotationScale = design.AnnotationScale,
                Dimensions = (int)design.Dimensions,
                DimensionStyle = design.DimensionStyle,
                SafetySelections = design.SafetySelections.Where(s => s != null).Select(SafetySelectionDocument.From).ToList()
            };

            document.Fronts = design.Fronts
                .Where(front => front != null)
                .Select(front =>
                {
                    var saved = DynamicRackFrontDocument.From(front);
                    saved.Bfr = DynamicFrontGeometry.Bfr(design.Pallet?.Front ?? 0.0);
                    return saved;
                })
                .ToList();

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
                BeamDepth = BeamDepth ?? DynamicRackDefaults.LegacyDefaultBeamDepth,
                PalletTolerance = PalletTolerance ?? DynamicRackDefaults.DefaultPalletTolerance,
                InOutBeamCatalogId = string.IsNullOrWhiteSpace(InOutBeamCatalogId)
                    ? DynamicRackDefaults.InOutBeamCatalogId
                    : InOutBeamCatalogId,
                HeaderPostCatalogId = HeaderPostCatalogId,
                PostPeralte = PostPeralte ?? 0.0,
                SeparatorCountOverride = SeparatorCountOverride,
                SeparatorSpacingOverride = SeparatorSpacingOverride,
                DerivedPostReinforced = DerivedPostReinforced,
                DerivedPostReinforcementHeight = DerivedPostReinforcementHeight,
                ManualHeaderHeightOverride = ManualHeaderHeightOverride,
                NumberFronts = NumberFronts ?? false,
                NumberLevels = NumberLevels ?? false,
                DrawRackName = DrawRackName ?? false,
                AnnotationScale = AnnotationScale.HasValue && AnnotationScale.Value > 0.0 ? AnnotationScale.Value : 1.0,
                Dimensions = ValidDimensions(Dimensions),
                DimensionStyle = DimensionStyle
            };

            foreach (var module in Modules ?? Enumerable.Empty<DynamicRackModuleDocument>())
            {
                design.Modules.Add(module.ToDesign());
            }

            foreach (var peralte in IntermediateBeamDepths ?? Enumerable.Empty<double>())
            {
                if (peralte > 0.0)
                {
                    design.IntermediateBeamDepths.Add(peralte);
                }
            }

            foreach (var front in Fronts ?? Enumerable.Empty<DynamicRackFrontDocument>())
            {
                if (front != null)
                {
                    design.Fronts.Add(front.ToDesign(design.LoadLevels, design.PalletsDeep));
                }
            }

            if (design.Fronts.Count == 0)
            {
                design.Fronts.Add(new DynamicRackFrontDesign
                {
                    PalletCount = DynamicRackDefaults.DefaultPalletsWide
                });
            }

            foreach (var safety in SafetySelections ?? Enumerable.Empty<SafetySelectionDocument>())
            {
                if (safety != null && !string.IsNullOrWhiteSpace(safety.ElementId))
                {
                    design.SafetySelections.Add(safety.ToDomain());
                }
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
                PostPeralte = PostPeralte ?? 0.0,
                InOutBeamCatalogId = string.IsNullOrWhiteSpace(InOutBeamCatalogId)
                    ? DynamicRackDefaults.InOutBeamCatalogId
                    : InOutBeamCatalogId,
                InOutBeamDepth = BeamDepth ?? DynamicRackDefaults.LegacyDefaultBeamDepth,
                PalletTolerance = PalletTolerance ?? DynamicRackDefaults.DefaultPalletTolerance,
                SeparatorCountOverride = SeparatorCountOverride,
                SeparatorSpacingOverride = SeparatorSpacingOverride,
                DerivedPostReinforced = DerivedPostReinforced,
                DerivedPostReinforcementHeight = DerivedPostReinforcementHeight,
                ManualHeaderHeightOverride = ManualHeaderHeightOverride,
                NumberFronts = NumberFronts ?? false,
                NumberLevels = NumberLevels ?? false,
                DrawRackName = DrawRackName ?? false,
                AnnotationScale = AnnotationScale.HasValue && AnnotationScale.Value > 0.0 ? AnnotationScale.Value : 1.0,
                Dimensions = ValidDimensions(Dimensions),
                DimensionStyle = DimensionStyle
            };

            foreach (var module in Modules ?? Enumerable.Empty<DynamicRackModuleDocument>())
            {
                system.Modules.Add(module.ToDomain());
            }

            if (system.PostPeralte <= 0.0)
            {
                system.PostPeralte = system.Modules
                    .Where(module => module.IsHeader && module.AssociatedFrameConfiguration?.PostPeralte > 0.0)
                    .Select(module => module.AssociatedFrameConfiguration.PostPeralte)
                    .FirstOrDefault();
            }

            foreach (var peralte in IntermediateBeamDepths ?? Enumerable.Empty<double>())
            {
                if (peralte > 0.0)
                {
                    system.IntermediateBeamDepths.Add(peralte);
                }
            }

            foreach (var front in Fronts ?? Enumerable.Empty<DynamicRackFrontDocument>())
            {
                if (front != null)
                {
                    var resolved = front.ToDomain(
                        PalletFront,
                        PalletDepth,
                        PalletHeight,
                        PalletWeight,
                        PalletWeightUnit,
                        system.PalletTolerance,
                        FirstLevelHeight ?? DynamicRackDefaults.DefaultFirstLevelHeight,
                        system.InOutBeamCatalogId,
                        system.InOutBeamDepth,
                        LoadLevels ?? DynamicRackDefaults.DefaultLoadLevels,
                        PalletsDeep);
                    if (resolved.IntermediateBeamDepths.Count == 0)
                    {
                        foreach (var peralte in system.IntermediateBeamDepths.Take(resolved.LoadLevels))
                        {
                            resolved.IntermediateBeamDepths.Add(peralte);
                        }
                    }
                    resolved.Index = system.Fronts.Count;
                    system.Fronts.Add(resolved);
                }
            }

            if (system.Fronts.Count == 0)
            {
                var legacy = DynamicRackFrontDocument.Legacy(
                    PalletFront,
                    system.PalletTolerance,
                    LoadLevels ?? DynamicRackDefaults.DefaultLoadLevels,
                    PalletsDeep);
                foreach (var peralte in system.IntermediateBeamDepths.Take(legacy.LoadLevels))
                {
                    legacy.IntermediateBeamDepths.Add(peralte);
                }
                legacy.Index = 0;
                system.Fronts.Add(legacy);
            }

            foreach (var safety in SafetySelections ?? Enumerable.Empty<SafetySelectionDocument>())
            {
                if (safety != null && !string.IsNullOrWhiteSpace(safety.ElementId))
                {
                    system.SafetySelections.Add(safety.ToDomain());
                }
            }

            system.RecalculatePositions();
            var depthLayout = DynamicDepthGeometry.Resolve(system);
            system.PalletsDeep = depthLayout.TotalPositions;
            system.BaseDepthStartPosition = depthLayout.BaseRange.StartPosition;
            system.BasePalletsDeep = depthLayout.BaseRange.PalletsDeep;
            DynamicDepthGeometry.ResolveCoordinates(system);
            return system;
        }

        private static DimensionDetail ValidDimensions(int? value)
            => value.HasValue && value.Value >= (int)DimensionDetail.None && value.Value <= (int)DimensionDetail.Detailed
                ? (DimensionDetail)value.Value
                : DimensionDetail.None;
    }

    public sealed class DynamicRackFrontDocument
    {
        public int PalletCount { get; set; } = DynamicRackDefaults.DefaultPalletsWide;
        public int? LoadLevels { get; set; }
        public int? PalletsDeep { get; set; }
        public int? DepthStartPosition { get; set; }
        public double? BeamLengthOverride { get; set; }
        public double? FirstLevelHeight { get; set; }
        public double? Bfr { get; set; }
        public List<double> IntermediateBeamDepths { get; set; }
        public List<DynamicRackLevelDocument> Levels { get; set; }

        public static DynamicRackFrontDocument From(DynamicRackFrontDesign front)
            => new DynamicRackFrontDocument
            {
                PalletCount = front.PalletCount,
                LoadLevels = front.LoadLevels,
                PalletsDeep = front.PalletsDeep,
                DepthStartPosition = front.DepthStartPosition,
                BeamLengthOverride = front.BeamLengthOverride,
                FirstLevelHeight = front.FirstLevelHeight,
                IntermediateBeamDepths = front.IntermediateBeamDepths.Where(value => value > 0.0).ToList(),
                Levels = front.Levels.Where(level => level != null).Select(DynamicRackLevelDocument.From).ToList()
            };

        public static DynamicRackFrontDocument From(DynamicRackFront front)
            => new DynamicRackFrontDocument
            {
                PalletCount = front.PalletCount,
                LoadLevels = front.LoadLevels,
                PalletsDeep = front.PalletsDeep,
                DepthStartPosition = front.DepthStartPosition,
                BeamLengthOverride = front.BeamLengthOverride,
                FirstLevelHeight = front.FirstLevelHeight,
                Bfr = front.Bfr > 0.0 ? front.Bfr : (double?)null,
                IntermediateBeamDepths = front.IntermediateBeamDepths.Where(value => value > 0.0).ToList(),
                Levels = front.Levels.Where(level => level != null).Select(DynamicRackLevelDocument.From).ToList()
            };

        public DynamicRackFrontDesign ToDesign(int defaultLoadLevels, int defaultPalletsDeep)
        {
            var result = new DynamicRackFrontDesign
            {
                PalletCount = PalletCount > 0 ? PalletCount : DynamicRackDefaults.DefaultPalletsWide,
                LoadLevels = LoadLevels.HasValue && LoadLevels.Value > 0
                    ? LoadLevels.Value
                    : System.Math.Max(1, defaultLoadLevels),
                PalletsDeep = PalletsDeep.HasValue && PalletsDeep.Value >= 2
                    ? PalletsDeep.Value
                    : System.Math.Max(2, defaultPalletsDeep),
                DepthStartPosition = DepthStartPosition.HasValue && DepthStartPosition.Value > 0
                    ? DepthStartPosition.Value
                    : 1,
                BeamLengthOverride = BeamLengthOverride,
                FirstLevelHeight = FirstLevelHeight
            };
            foreach (var peralte in IntermediateBeamDepths ?? Enumerable.Empty<double>())
            {
                if (peralte > 0.0)
                {
                    result.IntermediateBeamDepths.Add(peralte);
                }
            }

            foreach (var level in Levels ?? Enumerable.Empty<DynamicRackLevelDocument>())
            {
                if (level != null)
                {
                    result.Levels.Add(level.ToDesign());
                }
            }

            return result;
        }

        public DynamicRackFront ToDomain(
            double palletFront,
            double palletDepth,
            double palletHeight,
            double palletWeight,
            string palletWeightUnit,
            double tolerance,
            double defaultFirstLevelHeight,
            string defaultInOutBeamCatalogId,
            double defaultInOutBeamDepth,
            int defaultLoadLevels,
            int defaultPalletsDeep)
        {
            var count = PalletCount > 0 ? PalletCount : DynamicRackDefaults.DefaultPalletsWide;
            var result = new DynamicRackFront
            {
                PalletCount = count,
                LoadLevels = LoadLevels.HasValue && LoadLevels.Value > 0
                    ? LoadLevels.Value
                    : System.Math.Max(1, defaultLoadLevels),
                PalletsDeep = PalletsDeep.HasValue && PalletsDeep.Value >= 2
                    ? PalletsDeep.Value
                    : System.Math.Max(2, defaultPalletsDeep),
                DepthStartPosition = DepthStartPosition.HasValue && DepthStartPosition.Value > 0
                    ? DepthStartPosition.Value
                    : 1,
                BeamLengthOverride = BeamLengthOverride,
                FirstLevelHeight = FirstLevelHeight ?? defaultFirstLevelHeight,
                Bfr = Bfr.HasValue && Bfr.Value > 0.0 ? Bfr.Value : DynamicFrontGeometry.Bfr(palletFront),
                BeamLength = BeamLengthOverride.HasValue && BeamLengthOverride.Value > 0.0
                    ? BeamLengthOverride.Value
                    : DynamicFrontGeometry.AutoBeamLength(palletFront, count, tolerance)
            };
            foreach (var peralte in IntermediateBeamDepths ?? Enumerable.Empty<double>())
            {
                if (peralte > 0.0)
                {
                    result.IntermediateBeamDepths.Add(peralte);
                }
            }

            var levelIndex = 0;
            foreach (var level in Levels ?? Enumerable.Empty<DynamicRackLevelDocument>())
            {
                if (level == null || levelIndex >= result.LoadLevels)
                {
                    continue;
                }

                result.Levels.Add(level.ToDomain(
                    levelIndex + 1,
                    palletFront,
                    palletDepth,
                    palletHeight,
                    palletWeight,
                    palletWeightUnit,
                    defaultInOutBeamCatalogId,
                    defaultInOutBeamDepth,
                    result.IntermediateBeamDepths.ElementAtOrDefault(levelIndex),
                    count,
                    tolerance));
                levelIndex++;
            }

            if (result.Levels.Count > 0)
            {
                result.Bfr = result.Levels.Max(level => level.Bfr);
                result.BeamLength = result.Levels.Max(level => level.BeamLength);
                result.IntermediateBeamDepths.Clear();
                foreach (var level in result.Levels)
                {
                    result.IntermediateBeamDepths.Add(level.IntermediateBeamDepth);
                }
            }

            return result;
        }

        public static DynamicRackFront Legacy(
            double palletFront,
            double tolerance,
            int defaultLoadLevels,
            int defaultPalletsDeep)
            => new DynamicRackFrontDocument().ToDomain(
                palletFront,
                0.0,
                0.0,
                0.0,
                "kg",
                tolerance,
                DynamicRackDefaults.DefaultFirstLevelHeight,
                DynamicRackDefaults.InOutBeamCatalogId,
                DynamicRackDefaults.LegacyDefaultBeamDepth,
                defaultLoadLevels,
                defaultPalletsDeep);
    }

    /// <summary>Version-tolerant editable values of one front x level cell.</summary>
    public sealed class DynamicRackLevelDocument
    {
        public double? PalletFront { get; set; }
        public double? PalletHeight { get; set; }
        public double? PalletWeight { get; set; }
        public double? ClearHeight { get; set; }
        public string InOutBeamCatalogId { get; set; }
        public double? InOutBeamDepth { get; set; }
        public double? BeamLengthOverride { get; set; }
        public string IntermediateBeamCatalogId { get; set; }
        public double? IntermediateBeamDepth { get; set; }

        public static DynamicRackLevelDocument From(DynamicRackLevelDesign level)
            => new DynamicRackLevelDocument
            {
                PalletFront = level.PalletFront,
                PalletHeight = level.PalletHeight,
                PalletWeight = level.PalletWeight,
                ClearHeight = level.ClearHeight,
                InOutBeamCatalogId = level.InOutBeamCatalogId,
                InOutBeamDepth = level.InOutBeamDepth,
                BeamLengthOverride = level.BeamLengthOverride,
                IntermediateBeamCatalogId = level.IntermediateBeamCatalogId,
                IntermediateBeamDepth = level.IntermediateBeamDepth
            };

        public static DynamicRackLevelDocument From(DynamicRackLevel level)
            => new DynamicRackLevelDocument
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
            };

        public DynamicRackLevelDesign ToDesign()
            => new DynamicRackLevelDesign
            {
                PalletFront = PalletFront,
                PalletHeight = PalletHeight,
                PalletWeight = PalletWeight,
                ClearHeight = ClearHeight,
                InOutBeamCatalogId = InOutBeamCatalogId,
                InOutBeamDepth = InOutBeamDepth,
                BeamLengthOverride = BeamLengthOverride,
                IntermediateBeamCatalogId = IntermediateBeamCatalogId,
                IntermediateBeamDepth = IntermediateBeamDepth
            };

        public DynamicRackLevel ToDomain(
            int levelNumber,
            double palletFront,
            double palletDepth,
            double palletHeight,
            double palletWeight,
            string palletWeightUnit,
            string defaultInOutBeamCatalogId,
            double defaultInOutBeamDepth,
            double defaultIntermediateBeamDepth,
            int palletCount,
            double tolerance)
        {
            var front = PalletFront.HasValue && PalletFront.Value > 0.0 ? PalletFront.Value : palletFront;
            var manual = BeamLengthOverride.HasValue && BeamLengthOverride.Value > 0.0 ? BeamLengthOverride : null;
            return new DynamicRackLevel
            {
                LevelNumber = levelNumber,
                Pallet = new PalletSpecification(
                    front,
                    palletDepth,
                    PalletHeight.HasValue && PalletHeight.Value > 0.0 ? PalletHeight.Value : palletHeight,
                    PalletWeight.HasValue && PalletWeight.Value >= 0.0 ? PalletWeight.Value : palletWeight,
                    string.IsNullOrWhiteSpace(palletWeightUnit) ? "kg" : palletWeightUnit),
                ClearHeight = ClearHeight.HasValue && ClearHeight.Value >= 0.0
                    ? ClearHeight.Value
                    : DynamicRackDefaults.DefaultClearHeight,
                InOutBeamCatalogId = string.IsNullOrWhiteSpace(InOutBeamCatalogId)
                    ? defaultInOutBeamCatalogId
                    : InOutBeamCatalogId,
                InOutBeamDepth = InOutBeamDepth.HasValue && InOutBeamDepth.Value > 0.0
                    ? InOutBeamDepth.Value
                    : defaultInOutBeamDepth,
                BeamLengthOverride = manual,
                Bfr = DynamicFrontGeometry.Bfr(front),
                BeamLength = manual ?? DynamicFrontGeometry.AutoBeamLength(front, palletCount, tolerance),
                IntermediateBeamCatalogId = string.IsNullOrWhiteSpace(IntermediateBeamCatalogId)
                    ? DynamicRackDefaults.IntermediateBeamCatalogId
                    : IntermediateBeamCatalogId,
                IntermediateBeamDepth = IntermediateBeamDepth.HasValue && IntermediateBeamDepth.Value > 0.0
                    ? IntermediateBeamDepth.Value
                    : defaultIntermediateBeamDepth > 0.0
                        ? defaultIntermediateBeamDepth
                        : DynamicRackDefaults.DefaultIntermediateBeamDepth
            };
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
