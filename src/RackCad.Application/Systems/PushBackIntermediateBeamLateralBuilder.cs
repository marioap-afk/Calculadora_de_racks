using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Geometry;
using RackCad.Application.Headers;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// Places one intermediate support beam (<c>LARGUERO_ESCALON_INFINITO</c>) at every internal post and load level,
    /// tangent to the PUSH BACK bed axis (whose high mate is the rear TROQUEL_REDONDO beam, so the tangent line differs
    /// from the dynamic one). It REUSES the dynamic structural helpers (<see cref="DynamicIntermediateBeamGeometry.Supports"/>,
    /// peralte/beam-id lookups) but consumes <see cref="PushBackFlowBedGeometry"/> for the axis. No dynamic code is altered.
    /// </summary>
    public sealed class PushBackIntermediateBeamLateralBuilder
    {
        public const string GroupPrefix = "PB_LARGUERO_INTERMEDIO";

        public DynamicSystemPlan Build(PushBackSystem system, RackCatalog catalog, int postIndex = -1, int levelCount = int.MaxValue)
        {
            var flat = new List<HeaderBlockInstance>();
            var structure = system?.Structure;
            if (structure == null)
            {
                return HeaderInstanceGrouper.Group(flat, GroupPrefix);
            }

            var postId = structure.Modules
                .FirstOrDefault(module => module.IsHeader && module.AssociatedFrameConfiguration?.LeftPost != null)?
                .AssociatedFrameConfiguration.LeftPost.PostCatalogId;
            var finPoste = CatalogLookup.Local(catalog, postId, "FIN_POSTE", DynamicRackDefaults.IntermediateBeamView);

            var fronts = postIndex >= 0
                ? DynamicFrontGeometry.AdjacentFronts(structure, postIndex).OrderByDescending(front => front.PalletsDeep).ToList()
                : new List<DynamicRackFront> { null };
            var added = new HashSet<string>();
            foreach (var front in fronts)
            {
                var axes = PushBackFlowBedGeometry.Resolve(system, catalog, front)
                    .Where(axis => axis.LevelNumber <= levelCount)
                    .ToList();

                foreach (var support in DynamicIntermediateBeamGeometry.Supports(structure, finPoste, front))
                {
                    foreach (var axis in axes)
                    {
                        var beamId = postIndex >= 0
                            ? DynamicIntermediateBeamGeometry.BeamIdAtPost(structure, postIndex, axis.LevelNumber)
                            : structure.Fronts
                                .Where(candidate => candidate.LoadLevels >= axis.LevelNumber)
                                .OrderByDescending(candidate => DynamicIntermediateBeamGeometry.PeralteAt(candidate, axis.LevelNumber))
                                .Select(candidate => DynamicIntermediateBeamGeometry.BeamIdAt(candidate, axis.LevelNumber))
                                .FirstOrDefault() ?? DynamicRackDefaults.IntermediateBeamCatalogId;
                        var block = CatalogLookup.Block(catalog, beamId, DynamicRackDefaults.IntermediateBeamView);
                        var leftEntry = catalog?.ConnectionLayout.FindConnectionLayout(
                            beamId, DynamicRackDefaults.IntermediateBeamLeftBedMatePoint, DynamicRackDefaults.IntermediateBeamView);
                        var rightEntry = catalog?.ConnectionLayout.FindConnectionLayout(
                            beamId, DynamicRackDefaults.IntermediateBeamRightBedMatePoint, DynamicRackDefaults.IntermediateBeamView);
                        if (string.IsNullOrWhiteSpace(block) || leftEntry == null || rightEntry == null)
                        {
                            continue;
                        }

                        var leftMate = new Point2D(leftEntry.LocalX, leftEntry.LocalY);
                        var rightMate = new Point2D(rightEntry.LocalX, rightEntry.LocalY);
                        var mate = support.Mirrored ? rightMate : leftMate;
                        var key = support.PostAxisX.ToString("0.####", CultureInfo.InvariantCulture)
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
                                ? DynamicIntermediateBeamGeometry.PeralteAtPost(structure, postIndex, axis.LevelNumber)
                                : DynamicIntermediateBeamGeometry.PeralteAt(structure, axis.LevelNumber)));
                    }
                }
            }

            return HeaderInstanceGrouper.Group(flat, GroupPrefix);
        }

        private static HeaderBlockInstance Make(
            PushBackFlowBedAxis axis,
            double postAxisX,
            Point2D localBedMate,
            bool mirrored,
            string beamId,
            string block,
            double peralte)
        {
            var contactX = postAxisX + (mirrored ? -localBedMate.X : localBedMate.X);
            var insertion = new Point2D(postAxisX, axis.RailOriginYAt(contactX) - localBedMate.Y);

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
