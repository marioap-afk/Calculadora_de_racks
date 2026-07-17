using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Bom;
using RackCad.Application.Catalogs;
using RackCad.Application.Headers;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// Aggregates a whole dynamic system into a COMPONENT bill of materials. Header configurations repeat at every
    /// transverse post line; derived reinforced posts, separators, IN/OUT beams and intermediate beams are explicit
    /// structural components. Each front contributes one complete bed per lane and level without exposing its
    /// internal rail/roller recipe. Post-base safety follows planta while level-specific diverters follow both end
    /// cuts, so each family uses its complete physical projection without multiplying the BOM.
    /// </summary>
    public static class SystemBomBuilder
    {
        public const string Cama = "Cama";
        public const string InOutBeam = "Larguero IN/OUT";
        public const string IntermediateBeam = "Larguero intermedio";
        public const string Separator = "Separador";
        public const string DerivedPost = "Poste derivado";
        public const string ReinforcedPost = "Poste reforzado";

        public static BillOfMaterials Build(DynamicRackSystem system, RackCatalog catalog)
        {
            if (system == null)
            {
                return new BillOfMaterials(new List<BomComponent>());
            }

            var postLineCount = Math.Max(1, system.Fronts.Count + 1);
            var headerConfigurations = new List<RackFrameConfiguration>();
            for (var postIndex = 0; postIndex < postLineCount; postIndex++)
            {
                var range = DynamicDepthGeometry.AtPost(system, postIndex);
                foreach (var module in system.Modules.Where(module => range.Contains(module.Index + 1)
                             && module.IsHeader
                             && module.AssociatedFrameConfiguration != null))
                {
                    headerConfigurations.Add(DynamicFrontGeometry.HeaderConfigurationAtPost(
                        system,
                        module,
                        catalog,
                        postIndex));
                }
            }

            var components = BomBuilder.Components(headerConfigurations, catalog).ToList();
            AddDerivedPostComponents(components, system, catalog, postLineCount);
            AddSeparatorComponents(components, system, catalog, postLineCount);
            AddInOutBeamComponents(components, system, catalog);
            AddIntermediateBeamComponents(components, system, catalog);
            AddBedComponents(components, system);

            AddSafetyComponents(components, system, catalog);

            return new BillOfMaterials(components);
        }

        private static void AddSeparatorComponents(
            ICollection<BomComponent> components,
            DynamicRackSystem system,
            RackCatalog catalog,
            int postLineCount)
        {
            var modules = system.Modules.Where(module => module.Kind == DynamicRackModuleKind.Separator
                && module.Length > 0.0).ToList();
            if (modules.Count == 0)
            {
                return;
            }

            var quantities = new Dictionary<double, int>();
            for (var postIndex = 0; postIndex < postLineCount; postIndex++)
            {
                var levelCount = DynamicSeparatorGeometry.Levels(
                    system,
                    catalog,
                    DynamicFrontGeometry.PostHeight(system, postIndex)).Count;
                foreach (var module in modules)
                {
                    if (!DynamicDepthGeometry.AtPost(system, postIndex).Contains(module.Index + 1))
                    {
                        continue;
                    }

                    var length = Round(module.Length);
                    quantities[length] = quantities.TryGetValue(length, out var current)
                        ? current + levelCount
                        : levelCount;
                }
            }

            var profileId = DynamicRackDefaults.SeparatorCatalogId;
            var description = catalog?.DescribeId(profileId) ?? profileId;
            foreach (var group in quantities.Where(item => item.Value > 0).OrderBy(item => item.Key))
            {
                components.Add(new BomComponent
                {
                    Category = Separator,
                    ProfileId = profileId,
                    Description = description,
                    Length = group.Key,
                    Quantity = group.Value,
                    Pieces = new List<BomLine>
                    {
                        Piece(Separator, profileId, description, group.Key, 1)
                    }
                });
            }
        }

        private static void AddInOutBeamComponents(
            ICollection<BomComponent> components,
            DynamicRackSystem system,
            RackCatalog catalog)
        {
            if (system.Fronts.Count == 0 || system.LoadBeamLevels.Count == 0)
            {
                return;
            }

            var quantities = new Dictionary<(string ProfileId, double Length, double Peralte), int>();
            foreach (var front in system.Fronts.Where(front => front != null))
            {
                for (var level = 1; level <= Math.Max(1, front.LoadLevels); level++)
                {
                    var configuration = DynamicRackLevelGeometry.At(system, front, level);
                    var key = (
                        string.IsNullOrWhiteSpace(configuration.InOutBeamCatalogId)
                            ? DynamicRackDefaults.InOutBeamCatalogId
                            : configuration.InOutBeamCatalogId,
                        Round(front.BeamLength),
                        Round(configuration.InOutBeamDepth));
                    quantities[key] = quantities.TryGetValue(key, out var current) ? current + 2 : 2;
                }
            }

            foreach (var group in quantities
                         .OrderBy(item => item.Key.ProfileId, StringComparer.OrdinalIgnoreCase)
                         .ThenBy(item => item.Key.Length)
                         .ThenBy(item => item.Key.Peralte))
            {
                var profile = catalog?.BeamProfiles?.FirstOrDefault(entry => string.Equals(
                    entry?.Id,
                    group.Key.ProfileId,
                    StringComparison.OrdinalIgnoreCase));
                var description = string.Format(
                    CultureInfo.InvariantCulture,
                    "{0} · Peralte {1:0.##}\"",
                    profile?.Label ?? group.Key.ProfileId,
                    group.Key.Peralte);
                components.Add(new BomComponent
                {
                    Category = InOutBeam,
                    ProfileId = group.Key.ProfileId,
                    Description = description,
                    Length = group.Key.Length,
                    Quantity = group.Value,
                    Pieces = new List<BomLine>
                    {
                        Piece(InOutBeam, group.Key.ProfileId, description, group.Key.Length, 1)
                    }
                });
            }
        }

        private static void AddDerivedPostComponents(
            ICollection<BomComponent> components,
            DynamicRackSystem system,
            RackCatalog catalog,
            int postLineCount)
        {
            var offsets = system.GetDerivedPostOffsets();
            var postId = DynamicFrontGeometry.PostId(system, catalog);
            if (string.IsNullOrWhiteSpace(postId))
            {
                return;
            }

            var plateId = DynamicFrontGeometry.PlateId(system, catalog);
            var postDescription = catalog?.PostProfiles.FindProfile(postId)?.Label ?? postId;
            var plateDescription = catalog?.BasePlates.FindBasePlate(plateId)?.Label ?? plateId ?? string.Empty;
            var grouped = new Dictionary<(double Primary, double Reinforcement), int>();
            for (var postIndex = 0; postIndex < postLineCount; postIndex++)
            {
                var primaryHeight = Round(DynamicFrontGeometry.PostHeight(system, postIndex));
                var reinforcementHeight = system.DerivedPostReinforced
                    ? Round(system.DerivedPostReinforcementHeight.HasValue
                            && system.DerivedPostReinforcementHeight.Value > 0.0
                        ? system.DerivedPostReinforcementHeight.Value
                        : primaryHeight)
                    : 0.0;
                var range = DynamicDepthGeometry.AtPost(system, postIndex);
                var derivedCount = offsets.Count(offset =>
                {
                    var boundary = system.Modules
                        .Where(module => module.EndX <= offset + 1e-6)
                        .Select(module => module.Index + 1)
                        .DefaultIfEmpty(0)
                        .Max();
                    return range.Contains(boundary) && range.Contains(boundary + 1);
                });
                if (derivedCount > 0)
                {
                    var reinforcedKey = (primaryHeight, reinforcementHeight);
                    grouped[reinforcedKey] = grouped.TryGetValue(reinforcedKey, out var current)
                        ? current + derivedCount
                        : derivedCount;
                }

                var boundaryCount = DynamicDepthGeometry.BoundaryPostOffsets(system, range).Count;
                if (boundaryCount > 0)
                {
                    var boundaryKey = (primaryHeight, Reinforcement: 0.0);
                    grouped[boundaryKey] = grouped.TryGetValue(boundaryKey, out var current)
                        ? current + boundaryCount
                        : boundaryCount;
                }
            }

            foreach (var group in grouped.OrderBy(item => item.Key.Primary))
            {
                var reinforced = group.Key.Reinforcement > 0.0;
                var pieces = new List<BomLine>
                {
                    Piece(BomBuilder.Post, postId, postDescription, group.Key.Primary, 1)
                };
                if (reinforced)
                {
                    // It is a second physical post. The component category communicates that the logical support is
                    // reinforced while the flat piece BOM still totals both post profiles as posts.
                    pieces.Add(Piece(BomBuilder.Post, postId, postDescription, group.Key.Reinforcement, 1));
                }

                if (!string.IsNullOrWhiteSpace(plateId))
                {
                    pieces.Add(Piece(
                        BomBuilder.BasePlate,
                        plateId,
                        plateDescription,
                        0.0,
                        reinforced ? 2 : 1));
                }

                components.Add(new BomComponent
                {
                    Category = reinforced ? ReinforcedPost : DerivedPost,
                    ProfileId = postId,
                    Description = reinforced ? ReinforcedPost : DerivedPost,
                    Length = group.Key.Primary,
                    Quantity = group.Value,
                    Pieces = pieces
                });
            }
        }

        private static void AddIntermediateBeamComponents(
            ICollection<BomComponent> components,
            DynamicRackSystem system,
            RackCatalog catalog)
        {
            if (system.Fronts.Count == 0)
            {
                return;
            }

            var postId = DynamicFrontGeometry.PostId(system, catalog);
            var finPoste = CatalogLookup.Local(
                catalog,
                postId,
                "FIN_POSTE",
                DynamicRackDefaults.IntermediateBeamView);
            var grouped = new Dictionary<(string ProfileId, double Length, double Peralte), int>();
            foreach (var front in system.Fronts.Where(front => front != null))
            {
                var supportCount = DynamicIntermediateBeamGeometry.Supports(system, finPoste, front).Count;
                for (var level = 1; level <= Math.Max(1, front.LoadLevels); level++)
                {
                    var configuration = DynamicRackLevelGeometry.At(system, front, level);
                    var key = (
                        string.IsNullOrWhiteSpace(configuration.IntermediateBeamCatalogId)
                            ? DynamicRackDefaults.IntermediateBeamCatalogId
                            : configuration.IntermediateBeamCatalogId,
                        Round(front.BeamLength),
                        Round(configuration.IntermediateBeamDepth));
                    grouped[key] = grouped.TryGetValue(key, out var quantity)
                        ? quantity + supportCount
                        : supportCount;
                }
            }

            foreach (var group in grouped
                         .OrderBy(item => item.Key.ProfileId, StringComparer.OrdinalIgnoreCase)
                         .ThenBy(item => item.Key.Length)
                         .ThenBy(item => item.Key.Peralte))
            {
                var profile = catalog?.BeamProfiles?.FirstOrDefault(entry => string.Equals(
                    entry?.Id,
                    group.Key.ProfileId,
                    StringComparison.OrdinalIgnoreCase));
                var description = string.Format(
                    CultureInfo.InvariantCulture,
                    "{0} · Peralte {1:0.##}\"",
                    profile?.Label ?? group.Key.ProfileId,
                    group.Key.Peralte);
                components.Add(new BomComponent
                {
                    Category = IntermediateBeam,
                    ProfileId = group.Key.ProfileId,
                    Description = description,
                    Length = group.Key.Length,
                    Quantity = group.Value,
                    Pieces = new List<BomLine>
                    {
                        Piece(IntermediateBeam, group.Key.ProfileId, description, group.Key.Length, 1)
                    }
                });
            }
        }

        private static void AddBedComponents(
            ICollection<BomComponent> components,
            DynamicRackSystem system)
        {
            if (system.LoadBeamLevels.Count == 0)
            {
                return;
            }

            var beds = new Dictionary<(double Bfr, double Length), int>();
            if (system.Fronts.Count > 0)
            {
                foreach (var front in system.Fronts.Where(front => front != null))
                {
                    var length = Round(DynamicFlowBedGeometry.ResolveBedLength(system, front));
                    for (var level = 1; level <= Math.Max(1, front.LoadLevels); level++)
                    {
                        var configuration = DynamicRackLevelGeometry.At(system, front, level);
                        var key = (Round(configuration.Bfr), length);
                        beds[key] = beds.TryGetValue(key, out var current)
                            ? current + Math.Max(1, front.PalletCount)
                            : Math.Max(1, front.PalletCount);
                    }
                }
            }
            else
            {
                beds[(Round(DynamicFrontGeometry.Bfr(system.Pallet?.Front ?? 0.0)), Round(DynamicFlowBedGeometry.ResolveBedLength(system)))] =
                    system.LoadBeamLevels.Count * DynamicRackDefaults.DefaultPalletsWide;
            }

            foreach (var group in beds.OrderBy(item => item.Key.Bfr).ThenBy(item => item.Key.Length))
            {
                components.Add(new BomComponent
                {
                    Category = Cama,
                    ProfileId = string.Empty,
                    Description = string.Format(CultureInfo.InvariantCulture, "{0} · BFR {1:0.##}\"", Cama, group.Key.Bfr),
                    Length = group.Key.Length,
                    Quantity = group.Value,
                    Pieces = new List<BomLine>()
                });
            }
        }

        private static BomLine Piece(
            string category,
            string profileId,
            string description,
            double length,
            int quantity)
            => new BomLine
            {
                Category = category,
                ProfileId = profileId,
                Description = description,
                Length = length,
                Quantity = quantity
            };

        private static double Round(double value)
            => Math.Round(value, 4);

        private static void AddSafetyComponents(
            ICollection<BomComponent> components,
            DynamicRackSystem system,
            RackCatalog catalog)
        {
            if (catalog == null)
            {
                return;
            }

            var layout = DynamicFrontGeometry.Compute(system, catalog);
            var plateId = DynamicFrontGeometry.PlateId(system, catalog);
            var safetyBuilder = new DynamicSafetyMultiViewBuilder();

            // Planta is authoritative for post-base protection: one lateral spans the complete flow and therefore
            // replaces BOTH endpoint boots at its transverse post line. Counting the two frontal projections would
            // incorrectly retain the boot at the opposite cut. Desviadores remain authoritative in both frontal cuts
            // because planta deliberately collapses their repeated load levels to one visible reference.
            var planta = new List<HeaderBlockInstance>();
            safetyBuilder.AppendPlanta(planta, system, catalog, layout, plateId);
            var frontales = new List<HeaderBlockInstance>();
            safetyBuilder.AppendFrontal(frontales, system, catalog, layout, plateId, DynamicRackEnd.Exit);
            safetyBuilder.AppendFrontal(frontales, system, catalog, layout, plateId, DynamicRackEnd.Entrance);

            var drawn = planta
                .Where(instance => IsSafetyType(instance, catalog, SelectiveSafetyDefaults.BotaType)
                                   || IsSafetyType(instance, catalog, SelectiveSafetyDefaults.LateralType)
                                   || IsSafetyType(instance, catalog, SelectiveSafetyDefaults.DefensaType))
                .Concat(frontales.Where(instance => IsSafetyType(
                                                    instance,
                                                    catalog,
                                                    SelectiveSafetyDefaults.DesviadorType)
                                                || IsSafetyType(
                                                    instance,
                                                    catalog,
                                                    SelectiveSafetyDefaults.GuiaType)))
                .Select(instance => new
                {
                    Instance = instance,
                    DrawnLength = instance.DynamicParameters.TryGetValue(SelectiveRackDefaults.LengthParam, out var value)
                        ? value
                        : 0.0
                })
                .GroupBy(item => (item.Instance.PieceId, Length: System.Math.Round(item.DrawnLength, 4)));

            foreach (var group in drawn)
            {
                var element = catalog?.SafetyElements?.FirstOrDefault(entry => string.Equals(
                    entry?.Id,
                    group.Key.PieceId,
                    System.StringComparison.OrdinalIgnoreCase));
                var length = group.Key.Length;
                if (element != null
                    && SelectiveSafetyDefaults.IsType(element.Type, SelectiveSafetyDefaults.LateralType))
                {
                    length += SelectiveSafetyPlacement.LateralLengthAllowance;
                }

                var label = element?.Label ?? group.Key.PieceId;
                components.Add(new BomComponent
                {
                    Category = SelectiveBomBuilder.Safety,
                    ProfileId = group.Key.PieceId,
                    Description = label,
                    Length = length,
                    Quantity = group.Count(),
                    Pieces = new List<BomLine>
                    {
                        new BomLine
                        {
                            Category = SelectiveBomBuilder.Safety,
                            ProfileId = group.Key.PieceId,
                            Description = label,
                            Length = length,
                            Quantity = 1
                        }
                    }
                });
            }
        }

        private static bool IsSafetyType(
            HeaderBlockInstance instance,
            RackCatalog catalog,
            string type)
        {
            if (instance == null
                || instance.Role != HeaderBlockRole.Safety
                || string.IsNullOrWhiteSpace(instance.PieceId))
            {
                return false;
            }

            var element = catalog.SafetyElements.FirstOrDefault(entry => string.Equals(
                entry?.Id,
                instance.PieceId,
                StringComparison.OrdinalIgnoreCase));
            return element != null && SelectiveSafetyDefaults.IsType(element.Type, type);
        }
    }
}
