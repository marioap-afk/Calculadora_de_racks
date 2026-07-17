using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Geometry;
using RackCad.Application.Headers;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// Builds the top view of the whole pallet-flow system. X follows the flow/depth and Y follows the transverse
    /// run. The complete lateral structure is repeated at every front boundary; endpoint and intermediate beams run
    /// across each front. Load levels collapse onto one plan line and roller beds are deliberately omitted.
    /// </summary>
    public sealed class DynamicSystemPlantaBuilder
    {
        private const string View = "PLANTA";
        private readonly PlantaHeaderLayoutBuilder headerBuilder = new PlantaHeaderLayoutBuilder();
        private readonly DynamicSafetyMultiViewBuilder safetyBuilder = new DynamicSafetyMultiViewBuilder();

        public DynamicSystemPlan BuildPlan(DynamicRackSystem system, RackCatalog catalog)
        {
            var groups = new List<HeaderGroup>();
            var loose = new List<HeaderBlockInstance>();
            if (system == null || system.Fronts.Count == 0)
            {
                return new DynamicSystemPlan(groups, loose);
            }

            var layout = DynamicFrontGeometry.Compute(system, catalog);
            if (layout.PostPositions.Count == 0)
            {
                return new DynamicSystemPlan(groups, loose);
            }

            var postId = DynamicFrontGeometry.PostId(system, catalog);
            var plateId = DynamicFrontGeometry.PlateId(system, catalog);
            var postPeralte = DynamicFrontGeometry.PostPeralte(system, catalog, postId);
            var headerOrdinal = 0;

            // Each longitudinal cabecera is built once, then ARRAY-placed at every transverse post line.
            foreach (var module in system.Modules.Where(module => module.IsHeader
                && module.AssociatedFrameConfiguration != null))
            {
                var local = headerBuilder.Build(
                    module.AssociatedFrameConfiguration,
                    catalog,
                    new Point2D(0.0, 0.0),
                    postPeralte);
                var mirrored = headerOrdinal % 2 == 1;
                var insertionX = mirrored ? module.EndX : module.StartX;
                var placements = layout.PostPositions
                    .Select((y, postIndex) => new { Y = y, PostIndex = postIndex })
                    .Where(item => DynamicDepthGeometry.AtPost(system, item.PostIndex).Contains(module.Index + 1))
                    .Select(item => new HeaderPlacement(insertionX, mirrored, item.Y))
                    .ToList();
                if (placements.Count > 0)
                {
                    groups.Add(new HeaderGroup("DIN_PLANTA_CAB_" + (headerOrdinal + 1), local, placements));
                }
                headerOrdinal++;
            }

            AddSeparators(loose, system, catalog, layout.PostPositions, postId, postPeralte);
            AddDerivedPosts(loose, system, catalog, layout.PostPositions, postId, plateId, postPeralte);
            AddTransverseBeams(loose, system, catalog, layout, postId, postPeralte);
            safetyBuilder.AppendPlanta(loose, system, catalog, layout, plateId);
            DynamicViewDecorations.AppendPlanta(loose, system, layout);

            // The transverse pieces repeat heavily; group them independently while preserving the already grouped
            // cabeceras. This is the same shared-definition/ARRAY performance pattern as the mature selective planta.
            var groupedLoose = HeaderInstanceGrouper.Group(loose, "DIN_PLANTA_PIEZA");
            groups.AddRange(groupedLoose.Headers);
            return new DynamicSystemPlan(groups, groupedLoose.LooseInstances);
        }

        public IReadOnlyList<HeaderBlockInstance> Build(DynamicRackSystem system, RackCatalog catalog)
            => BuildPlan(system, catalog).Flatten().Instances;

        private static void AddSeparators(
            ICollection<HeaderBlockInstance> result,
            DynamicRackSystem system,
            RackCatalog catalog,
            IReadOnlyList<double> postYs,
            string postId,
            double postPeralte)
        {
            var block = CatalogLookup.Block(catalog, DynamicRackDefaults.SeparatorCatalogId, View);
            var mate = CatalogLookup.Local(
                catalog,
                DynamicRackDefaults.SeparatorCatalogId,
                DynamicRackDefaults.SeparatorMatePoint,
                View);
            var troquelEntry = catalog?.ConnectionLayout.FindConnectionLayout(
                postId,
                DynamicRackDefaults.SeparatorPostPoint,
                View);
            var troquel = SelectivePostGeometry.Resolve(troquelEntry, new Dictionary<string, double>
            {
                [SelectiveRackDefaults.PeralteParam] = postPeralte
            });

            foreach (var module in system.Modules.Where(module => module.Kind == DynamicRackModuleKind.Separator
                && module.Length > 0.0))
            {
                for (var postIndex = 0; postIndex < postYs.Count; postIndex++)
                {
                    if (!DynamicDepthGeometry.AtPost(system, postIndex).Contains(module.Index + 1))
                    {
                        continue;
                    }

                    var postY = postYs[postIndex];
                    var anchorX = module.Index + 1 == DynamicDepthGeometry.AtPost(system, postIndex).StartPosition
                        ? module.StartX + troquel.X
                        : module.StartX - troquel.X;
                    var anchor = new Point2D(anchorX, postY + troquel.Y);
                    var separator = new HeaderBlockInstance
                    {
                        Role = HeaderBlockRole.Separator,
                        PieceId = DynamicRackDefaults.SeparatorCatalogId,
                        BlockName = block,
                        View = View,
                        ConnectionAnchor = anchor,
                        Insertion = new Point2D(anchor.X - mate.X, anchor.Y - mate.Y)
                    };
                    separator.DynamicParameters[SelectiveRackDefaults.LengthParam] = module.Length;
                    result.Add(separator);
                }
            }
        }

        private static void AddDerivedPosts(
            ICollection<HeaderBlockInstance> result,
            DynamicRackSystem system,
            RackCatalog catalog,
            IReadOnlyList<double> postYs,
            string postId,
            string plateId,
            double postPeralte)
        {
            var postBlock = CatalogLookup.Block(catalog, postId, View);
            var plateBlock = CatalogLookup.Block(catalog, plateId, View);
            var plateMate = CatalogLookup.Local(catalog, plateId, SelectiveRackDefaults.PlateMatePoint, View);
            var finPoste = CatalogLookup.Local(catalog, postId, "FIN_POSTE", DynamicRackDefaults.IntermediateBeamView);
            var platePeralte = catalog?.BasePlates.FindBasePlate(plateId)?.StandardPeralte(postPeralte) ?? 0.0;

            foreach (var boundary in system.GetDerivedPostOffsets())
            {
                var placement = DynamicDerivedPostGeometry.Resolve(boundary, system.DerivedPostReinforced, finPoste);
                for (var postIndex = 0; postIndex < postYs.Count; postIndex++)
                {
                    var range = DynamicDepthGeometry.AtPost(system, postIndex);
                    var boundaryPosition = system.Modules
                        .Where(module => module.EndX <= boundary + 1e-6)
                        .Select(module => module.Index + 1)
                        .DefaultIfEmpty(0)
                        .Max();
                    if (!range.Contains(boundaryPosition) || !range.Contains(boundaryPosition + 1))
                    {
                        continue;
                    }

                    var postY = postYs[postIndex];
                    AddPost(result, postId, postBlock, plateId, plateBlock, plateMate,
                        new Point2D(placement.PrimaryOrigin.X, postY), postPeralte, platePeralte,
                        mirroredX: false, mirroredY: false);
                    if (placement.HasReinforcement)
                    {
                        // A reinforced derived post continues along the flow axis: both profiles belong to the same
                        // transverse front line. FIN_POSTE defines the primary origin before the module boundary;
                        // the reinforcement starts at that boundary and keeps the same block orientation.
                        AddPost(result, postId, postBlock, plateId, plateBlock, plateMate,
                            new Point2D(placement.ReinforcementOrigin.X, postY), postPeralte, platePeralte,
                            mirroredX: false, mirroredY: false);
                    }
                }
            }

            for (var postIndex = 0; postIndex < postYs.Count; postIndex++)
            {
                var range = DynamicDepthGeometry.AtPost(system, postIndex);
                foreach (var offset in DynamicDepthGeometry.BoundaryPostOffsets(system, range))
                {
                    AddPost(result, postId, postBlock, plateId, plateBlock, plateMate,
                        new Point2D(offset, postYs[postIndex]), postPeralte, platePeralte,
                        mirroredX: false, mirroredY: false);
                }
            }
        }

        private static void AddPost(
            ICollection<HeaderBlockInstance> result,
            string postId,
            string postBlock,
            string plateId,
            string plateBlock,
            Point2D plateMate,
            Point2D origin,
            double postPeralte,
            double platePeralte,
            bool mirroredX,
            bool mirroredY)
        {
            var post = new HeaderBlockInstance
            {
                Role = HeaderBlockRole.Post,
                PieceId = postId,
                BlockName = postBlock,
                View = View,
                MirroredX = mirroredX,
                MirroredY = mirroredY,
                ConnectionAnchor = origin,
                Insertion = origin
            };
            post.DynamicParameters[SelectiveRackDefaults.PeralteParam] = postPeralte;
            result.Add(post);

            if (string.IsNullOrWhiteSpace(plateId))
            {
                return;
            }

            var signX = mirroredX ? -1.0 : 1.0;
            var signY = mirroredY ? -1.0 : 1.0;
            var plate = new HeaderBlockInstance
            {
                Role = HeaderBlockRole.BasePlate,
                PieceId = plateId,
                BlockName = plateBlock,
                View = View,
                MirroredX = mirroredX,
                MirroredY = mirroredY,
                ConnectionAnchor = origin,
                Insertion = new Point2D(origin.X - signX * plateMate.X, origin.Y - signY * plateMate.Y)
            };
            if (platePeralte > 0.0)
            {
                plate.DynamicParameters[SelectiveRackDefaults.PeralteParam] = platePeralte;
            }

            result.Add(plate);
        }

        private static void AddTransverseBeams(
            ICollection<HeaderBlockInstance> result,
            DynamicRackSystem system,
            RackCatalog catalog,
            DynamicFrontLayout layout,
            string postId,
            double postPeralte)
        {
            var troquelEntry = catalog?.ConnectionLayout.FindConnectionLayout(
                postId,
                SelectiveRackDefaults.PostBeamPoint,
                View);
            var troquel = SelectivePostGeometry.Resolve(troquelEntry, new Dictionary<string, double>
            {
                [SelectiveRackDefaults.PeralteParam] = postPeralte
            });
            var finPoste = CatalogLookup.Local(catalog, postId, "FIN_POSTE", DynamicRackDefaults.IntermediateBeamView);

            for (var index = 0; index < system.Fronts.Count; index++)
            {
                var front = system.Fronts[index];
                var envelope = DynamicRackLevelGeometry.Envelope(system, front);
                var beamId = envelope.InOutBeamCatalogId;
                var beamBlock = CatalogLookup.Block(catalog, beamId, View);
                var beamY = layout.PostPositions[index] + troquel.Y;
                AddBeam(result, beamId, beamBlock, new Point2D(front.StartX + troquel.X, beamY), front.BeamLength, envelope.InOutBeamDepth, mirrored: false);
                AddBeam(result, beamId, beamBlock, new Point2D(front.EndX - troquel.X, beamY), front.BeamLength, envelope.InOutBeamDepth, mirrored: true);

                var intermediate = front.Levels
                    .OrderByDescending(level => level.IntermediateBeamDepth)
                    .FirstOrDefault() ?? DynamicRackLevelGeometry.At(system, front, 1);
                var intermediateBlock = CatalogLookup.Block(catalog, intermediate.IntermediateBeamCatalogId, View);
                foreach (var support in DynamicIntermediateBeamGeometry.Supports(system, finPoste, front))
                {
                    AddBeam(result, intermediate.IntermediateBeamCatalogId, intermediateBlock,
                        new Point2D(support.PostAxisX, beamY), front.BeamLength,
                        intermediate.IntermediateBeamDepth, support.Mirrored);
                }
            }
        }

        private static void AddBeam(
            ICollection<HeaderBlockInstance> result,
            string pieceId,
            string block,
            Point2D origin,
            double length,
            double peralte,
            bool mirrored)
        {
            var beam = new HeaderBlockInstance
            {
                Role = HeaderBlockRole.Beam,
                PieceId = pieceId,
                BlockName = block,
                View = View,
                MirroredX = mirrored,
                ConnectionAnchor = origin,
                Insertion = origin
            };
            beam.DynamicParameters[SelectiveRackDefaults.LengthParam] = length;
            beam.DynamicParameters[SelectiveRackDefaults.PeralteParam] = peralte;
            result.Add(beam);
        }
    }
}
