using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Geometry;
using RackCad.Application.Headers;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// Places one intermediate support beam at every internal post and load level; IN/OUT beams own the two ends.
    /// The block origin stays on the post's vertical axis, while the selected left/right contact is lifted to the
    /// rail block's ORIGIN line. The bracket's vertical slot is free from the post perforation grid.
    /// </summary>
    public sealed class DynamicIntermediateBeamLateralBuilder
    {
        public DynamicSystemPlan Build(
            DynamicRackSystem system,
            RackCatalog catalog,
            int postIndex = -1,
            int levelCount = int.MaxValue)
        {
            var flat = new List<HeaderBlockInstance>();
            if (system == null)
            {
                return HeaderInstanceGrouper.Group(flat, "DIN_LARGUERO_INTERMEDIO");
            }

            var postId = system.Modules
                .FirstOrDefault(module => module.IsHeader && module.AssociatedFrameConfiguration?.LeftPost != null)?
                .AssociatedFrameConfiguration.LeftPost.PostCatalogId;
            var finPoste = CatalogLookup.Local(catalog, postId, "FIN_POSTE", DynamicRackDefaults.IntermediateBeamView);

            var fronts = postIndex >= 0
                ? DynamicFrontGeometry.AdjacentFronts(system, postIndex)
                    .OrderByDescending(front => front.PalletsDeep)
                    .ToList()
                : new List<DynamicRackFront> { null };
            var added = new HashSet<string>();
            foreach (var front in fronts)
            {
                var axes = DynamicFlowBedGeometry.Resolve(system, catalog, front)
                    .Where(axis => axis.LevelNumber <= levelCount)
                    .ToList();

                foreach (var support in DynamicIntermediateBeamGeometry.Supports(system, finPoste, front))
                {
                    foreach (var axis in axes)
                    {
                        var beamId = postIndex >= 0
                            ? DynamicIntermediateBeamGeometry.BeamIdAtPost(system, postIndex, axis.LevelNumber)
                            : system.Fronts
                                .Where(candidate => candidate.LoadLevels >= axis.LevelNumber)
                                .OrderByDescending(candidate => DynamicIntermediateBeamGeometry.PeralteAt(candidate, axis.LevelNumber))
                                .Select(candidate => DynamicIntermediateBeamGeometry.BeamIdAt(candidate, axis.LevelNumber))
                                .FirstOrDefault() ?? DynamicRackDefaults.IntermediateBeamCatalogId;
                        var block = CatalogLookup.Block(catalog, beamId, DynamicRackDefaults.IntermediateBeamView);
                        var leftEntry = catalog?.ConnectionLayout.FindConnectionLayout(
                            beamId,
                            DynamicRackDefaults.IntermediateBeamLeftBedMatePoint,
                            DynamicRackDefaults.IntermediateBeamView);
                        var rightEntry = catalog?.ConnectionLayout.FindConnectionLayout(
                            beamId,
                            DynamicRackDefaults.IntermediateBeamRightBedMatePoint,
                            DynamicRackDefaults.IntermediateBeamView);
                        // The piece and both physical contacts form one catalog contract. Never draw a partial support.
                        if (string.IsNullOrWhiteSpace(block) || leftEntry == null || rightEntry == null)
                        {
                            continue;
                        }

                        var leftMate = new Point2D(leftEntry.LocalX, leftEntry.LocalY);
                        var rightMate = new Point2D(rightEntry.LocalX, rightEntry.LocalY);
                        var mate = support.Mirrored ? rightMate : leftMate;
                        var key = support.PostAxisX.ToString("0.####", System.Globalization.CultureInfo.InvariantCulture)
                                  + "|" + axis.LevelNumber + "|" + beamId;
                        if (!added.Add(key))
                        {
                            continue;
                        }

                        flat.Add(Make(
                            axis,
                            support.PostAxisX,
                            mate,
                            support.Mirrored,
                            beamId,
                            block,
                            postIndex >= 0
                                ? DynamicIntermediateBeamGeometry.PeralteAtPost(system, postIndex, axis.LevelNumber)
                                : DynamicIntermediateBeamGeometry.PeralteAt(system, axis.LevelNumber)));
                    }
                }
            }

            return HeaderInstanceGrouper.Group(flat, "DIN_LARGUERO_INTERMEDIO");
        }

        private static HeaderBlockInstance Make(
            DynamicFlowBedAxis axis,
            double postAxisX,
            Point2D localBedMate,
            bool mirrored,
            string beamId,
            string block,
            double peralte)
        {
            var contactX = postAxisX + (mirrored ? -localBedMate.X : localBedMate.X);
            var insertion = new Point2D(
                postAxisX,
                axis.RailOriginYAt(contactX) - localBedMate.Y);

            var result = new HeaderBlockInstance
            {
                Role = HeaderBlockRole.Beam,
                PieceId = beamId,
                BlockName = block,
                View = DynamicRackDefaults.IntermediateBeamView,
                Insertion = insertion,
                ConnectionAnchor = insertion,
                MirroredX = mirrored
            };
            result.DynamicParameters[SelectiveRackDefaults.PeralteParam] = peralte;
            return result;
        }
    }
}
