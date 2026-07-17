using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.RackFrames;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>Shared transverse post grid for the dynamic frontal and planta views.</summary>
    public sealed class DynamicFrontLayout
    {
        public DynamicFrontLayout(IReadOnlyList<double> postPositions, IReadOnlyList<double> troquelPositions)
        {
            PostPositions = postPositions;
            TroquelPositions = troquelPositions;
        }

        public IReadOnlyList<double> PostPositions { get; }
        public IReadOnlyList<double> TroquelPositions { get; }
        public double TotalWidth => PostPositions.Count == 0 ? 0.0 : PostPositions[PostPositions.Count - 1];
    }

    /// <summary>
    /// Resolves dynamic front widths and their post grid. One lane has BFR = pallet front + 2 in; the complete
    /// IN/OUT cut is the sum of its lane BFR widths + 6 in, unless the front carries an explicit override. Post
    /// spacing then adds the catalog-driven hook/profile offsets at both ends of the IN/OUT beam.
    /// </summary>
    public static class DynamicFrontGeometry
    {
        public static double AutoBeamLength(double palletFront, int palletCount, double tolerance)
        {
            var count = Math.Max(1, palletCount);
            return Bfr(palletFront) * count + DynamicRackDefaults.InOutBeamLengthAllowance;
        }

        public static double Bfr(double palletFront)
            => Math.Max(0.0, palletFront) + DynamicRackDefaults.BfrAllowance;

        public static IReadOnlyList<DynamicRackFront> Resolve(
            IEnumerable<DynamicRackFrontDesign> designs,
            double palletFront,
            double tolerance,
            int defaultLoadLevels = DynamicRackDefaults.DefaultLoadLevels,
            int defaultPalletsDeep = DynamicRackDefaults.DefaultPalletsDeep)
        {
            var source = designs?.Where(front => front != null).ToList() ?? new List<DynamicRackFrontDesign>();
            if (source.Count == 0)
            {
                source.Add(new DynamicRackFrontDesign { PalletCount = DynamicRackDefaults.DefaultPalletsWide });
            }

            var result = new List<DynamicRackFront>(source.Count);
            for (var index = 0; index < source.Count; index++)
            {
                var design = source[index];
                var count = Math.Max(1, design.PalletCount);
                var beamLength = design.BeamLengthOverride.HasValue && design.BeamLengthOverride.Value > 0.0
                    ? design.BeamLengthOverride.Value
                    : AutoBeamLength(palletFront, count, tolerance);
                result.Add(new DynamicRackFront
                {
                    Index = index,
                    PalletCount = count,
                    LoadLevels = design.LoadLevels.HasValue && design.LoadLevels.Value > 0
                        ? design.LoadLevels.Value
                        : Math.Max(1, defaultLoadLevels),
                    PalletsDeep = design.PalletsDeep.HasValue && design.PalletsDeep.Value >= 2
                        ? design.PalletsDeep.Value
                        : Math.Max(2, defaultPalletsDeep),
                    DepthStartPosition = design.DepthStartPosition.HasValue && design.DepthStartPosition.Value > 0
                        ? design.DepthStartPosition.Value
                        : 1,
                    Bfr = Bfr(palletFront),
                    BeamLength = beamLength,
                    BeamLengthOverride = design.BeamLengthOverride
                });
            }

            return result;
        }

        public static IReadOnlyList<DynamicLoadBeamLevel> LoadBeamLevels(
            DynamicRackSystem system,
            DynamicRackFront front)
        {
            if (front?.LoadBeamLevels?.Count > 0)
            {
                return front.LoadBeamLevels.ToList();
            }

            if (system?.LoadBeamLevels == null)
            {
                return Array.Empty<DynamicLoadBeamLevel>();
            }

            return front == null
                ? system.LoadBeamLevels.ToList()
                : system.LoadBeamLevels.Take(Math.Max(1, front.LoadLevels)).ToList();
        }

        public static DynamicFrontLayout Compute(DynamicRackSystem system, RackCatalog catalog)
        {
            if (system == null || system.Fronts.Count == 0)
            {
                return new DynamicFrontLayout(Array.Empty<double>(), Array.Empty<double>());
            }

            var postId = PostId(system, catalog);
            var peralte = PostPeralte(system, catalog, postId);
            var parameters = new Dictionary<string, double> { [SelectiveRackDefaults.PeralteParam] = peralte };
            var troquelEntry = catalog?.ConnectionLayout.FindConnectionLayout(
                postId,
                SelectiveRackDefaults.PostBeamPoint,
                RackEmbedView.Frontal);
            var troquel = SelectivePostGeometry.Resolve(troquelEntry, parameters).X;
            var posts = new List<double> { 0.0 };
            var troqueles = new List<double> { troquel };
            foreach (var front in system.Fronts)
            {
                var envelope = DynamicRackLevelGeometry.Envelope(system, front);
                var inicioEntry = catalog?.ConnectionLayout.FindConnectionLayout(
                    envelope.InOutBeamCatalogId,
                    SelectiveRackDefaults.BeamProfileStartPoint,
                    RackEmbedView.Frontal);
                var inicio = SelectivePostGeometry.Resolve(inicioEntry, new Dictionary<string, double>
                {
                    [SelectiveRackDefaults.PeralteParam] = envelope.InOutBeamDepth
                }).X;
                posts.Add(posts[posts.Count - 1] + front.BeamLength + 2.0 * (troquel + inicio));
                troqueles.Add(troquel);
            }

            return new DynamicFrontLayout(posts, troqueles);
        }

        public static string PostId(DynamicRackSystem system, RackCatalog catalog)
            => system?.Modules.FirstOrDefault(module => module.IsHeader
                    && module.AssociatedFrameConfiguration?.LeftPost != null)?
                .AssociatedFrameConfiguration.LeftPost.PostCatalogId
               ?? catalog?.Defaults?.Post;

        public static string PlateId(DynamicRackSystem system, RackCatalog catalog)
            => system?.Modules.FirstOrDefault(module => module.IsHeader
                    && module.AssociatedFrameConfiguration?.LeftBasePlate != null)?
                .AssociatedFrameConfiguration.LeftBasePlate.PlateCatalogId
               ?? catalog?.Defaults?.BasePlate;

        public static double Height(DynamicRackSystem system)
            => system?.Modules.Where(module => module.IsHeader && module.AssociatedFrameConfiguration != null)
                   .Select(module => module.AssociatedFrameConfiguration.Height)
                   .DefaultIfEmpty(0.0)
                   .Max()
               ?? 0.0;

        /// <summary>Height of one transverse post: the tallest of the fronts that share it. This mirrors the
        /// selective-rack contract and prevents an unrelated taller front from inflating every post.</summary>
        public static double PostHeight(DynamicRackSystem system, int postIndex)
        {
            if (system == null || postIndex < 0 || postIndex > system.Fronts.Count)
            {
                return 0.0;
            }

            var height = 0.0;
            if (postIndex > 0)
            {
                height = Math.Max(height, system.Fronts[postIndex - 1]?.Height ?? 0.0);
            }

            if (postIndex < system.Fronts.Count)
            {
                height = Math.Max(height, system.Fronts[postIndex]?.Height ?? 0.0);
            }

            return height > 0.0 ? height : Height(system);
        }

        /// <summary>
        /// Resolves the header configuration physically present at one transverse post line. Calculated headers
        /// inherit the tallest adjacent front; manually edited headers remain authoritative. Lateral drawing and
        /// BOM consume this same rule so a tall front changes only its two adjacent header sections and quantities.
        /// </summary>
        public static RackFrameConfiguration HeaderConfigurationAtPost(
            DynamicRackSystem system,
            DynamicRackModule module,
            RackCatalog catalog,
            int postIndex)
        {
            var configuration = module?.AssociatedFrameConfiguration;
            if (configuration == null || !module.UseCalculatedHeaderConfiguration)
            {
                return configuration;
            }

            var height = PostHeight(system, postIndex);
            if (height <= 0.0)
            {
                return configuration;
            }

            var postId = configuration.LeftPost?.PostCatalogId ?? PostId(system, catalog);
            return new DynamicRackSystemBuilder(catalog).BuildHeaderConfiguration(
                RackFrameTemplateCatalog.Default,
                postId,
                height,
                module.Length,
                system.PostPeralte);
        }

        /// <summary>Number of load levels visible at one post section: the tallest adjacent front owns the cut.</summary>
        public static int LoadLevelsAtPost(DynamicRackSystem system, int postIndex)
        {
            if (system == null || postIndex < 0 || postIndex > system.Fronts.Count)
            {
                return 0;
            }

            var levels = 0;
            if (postIndex > 0)
            {
                levels = Math.Max(levels, system.Fronts[postIndex - 1]?.LoadLevels ?? 0);
            }

            if (postIndex < system.Fronts.Count)
            {
                levels = Math.Max(levels, system.Fronts[postIndex]?.LoadLevels ?? 0);
            }

            return levels > 0 ? levels : system.LoadBeamLevels.Count;
        }

        /// <summary>Fronts physically adjacent to one post, used by section-only level and peralte rules.</summary>
        public static IReadOnlyList<DynamicRackFront> AdjacentFronts(DynamicRackSystem system, int postIndex)
        {
            var result = new List<DynamicRackFront>();
            if (system == null || postIndex < 0 || postIndex > system.Fronts.Count)
            {
                return result;
            }

            if (postIndex > 0 && system.Fronts[postIndex - 1] != null)
            {
                result.Add(system.Fronts[postIndex - 1]);
            }

            if (postIndex < system.Fronts.Count && system.Fronts[postIndex] != null)
            {
                result.Add(system.Fronts[postIndex]);
            }

            return result;
        }

        public static double PostPeralte(DynamicRackSystem system, RackCatalog catalog, string postId = null)
        {
            if (system?.PostPeralte > 0.0)
            {
                return system.PostPeralte;
            }

            var configuration = system?.Modules.FirstOrDefault(module => module.IsHeader
                && module.AssociatedFrameConfiguration != null)?.AssociatedFrameConfiguration;
            if (configuration?.PostPeralte > 0.0)
            {
                return configuration.PostPeralte;
            }

            var resolvedPostId = string.IsNullOrWhiteSpace(postId) ? PostId(system, catalog) : postId;
            var width = catalog?.PostProfiles?.FirstOrDefault(profile => string.Equals(
                profile?.Id,
                resolvedPostId,
                StringComparison.OrdinalIgnoreCase))?.Width ?? 0.0;
            return width > 0.0 ? width : 3.0;
        }

        /// <summary>Internal view names without taking a persistence dependency from Domain.</summary>
        private static class RackEmbedView
        {
            public const string Frontal = "FRONTAL";
        }
    }
}
