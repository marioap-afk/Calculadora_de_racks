using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Geometry;
using RackCad.Application.Headers;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// Builds the block plan for a whole dynamic (pallet flow) system in the lateral view: each header
    /// module is the lateral header (reused from <see cref="LateralHeaderLayoutBuilder"/>) shifted to its
    /// position along the run; each separator module gets a separator beam at every vertical level
    /// (<see cref="SeparatorLevelCalculator"/>), anchored on the adjacent post's TROQUEL_SEPARADOR; and a
    /// derived post (header post + plate, reinforced full height by default) is placed wherever two
    /// separators meet. Every resolved load level also receives one complete IN beam, one complete OUT beam and one
    /// complete roller bed composed by <see cref="DynamicFlowBedLateralBuilder"/>. Pure: returns a
    /// <see cref="DynamicSystemPlan"/> the AutoCAD drawer turns into a block. Headers, beds and IN/OUT beams use
    /// LATERAL blocks; separators use the FRONTAL block.
    /// </summary>
    public sealed class DynamicSystemLateralBuilder
    {
        private readonly LateralHeaderLayoutBuilder headerBuilder = new LateralHeaderLayoutBuilder();
        private readonly DynamicFlowBedLateralBuilder flowBedBuilder = new DynamicFlowBedLateralBuilder();
        private readonly DynamicIntermediateBeamLateralBuilder intermediateBeamBuilder = new DynamicIntermediateBeamLateralBuilder();
        private readonly DynamicSafetyLateralBuilder safetyBuilder = new DynamicSafetyLateralBuilder();

        public DynamicSystemPlan Build(DynamicRackSystem system, RackCatalog catalog)
            => BuildCore(system, catalog, -1);

        /// <summary>Builds the lateral section at one transverse post of the front grid.</summary>
        public DynamicSystemPlan Build(DynamicRackSystem system, RackCatalog catalog, int postIndex)
        {
            if (system == null || postIndex < 0 || postIndex > system.Fronts.Count)
            {
                return new DynamicSystemPlan(new List<HeaderGroup>(), new List<HeaderBlockInstance>());
            }

            return BuildCore(system, catalog, postIndex);
        }

        private DynamicSystemPlan BuildCore(DynamicRackSystem system, RackCatalog catalog, int postIndex)
        {
            if (system == null)
            {
                return new DynamicSystemPlan(new List<HeaderGroup>(), new List<HeaderBlockInstance>());
            }

            var sectioned = postIndex >= 0;
            var levelCount = sectioned
                ? DynamicFrontGeometry.LoadLevelsAtPost(system, postIndex)
                : system.LoadBeamLevels.Count;
            var sectionHeight = sectioned
                ? DynamicFrontGeometry.PostHeight(system, postIndex)
                : DynamicFrontGeometry.Height(system);
            var sectionRange = sectioned
                ? DynamicDepthGeometry.AtPost(system, postIndex)
                : new DynamicDepthRange(1, system.PalletsDeep);
            var context = Resolve(system, catalog, sectionHeight);
            var loose = new List<HeaderBlockInstance>();

            // Group identical headers so each distinct header becomes one shared block definition; record the
            // placements (with a mirror flag) where each is used. Consecutive headers alternate (mirror) so the
            // celosía direction alternates along the line.
            var groups = new Dictionary<string, HeaderGroupBuilder>();
            var order = new List<string>();
            var headerOrdinal = 0;

            foreach (var module in DynamicDepthGeometry.ModulesInRange(system, sectionRange))
            {
                if (module.IsHeader && module.AssociatedFrameConfiguration != null)
                {
                    var configuration = sectioned
                        ? DynamicFrontGeometry.HeaderConfigurationAtPost(system, module, catalog, postIndex)
                        : module.AssociatedFrameConfiguration;

                    // Build the header and key the group on the resulting geometry, so two headers share a
                    // definition only when their drawing is truly identical (any edit separates them).
                    var parameters = LateralHeaderParametersFactory.FromConfiguration(configuration);
                    var layout = headerBuilder.Build(configuration, parameters, catalog);
                    var signature = LayoutSignature(layout.Instances);

                    if (!groups.TryGetValue(signature, out var group))
                    {
                        group = new HeaderGroupBuilder(HeaderName(configuration), layout.Instances.ToList());
                        groups[signature] = group;
                        order.Add(signature);
                    }

                    // Every other header is mirrored; a mirrored reference is inserted at the module's far edge
                    // so it still fills [StartX, EndX] but flips the celosía.
                    var mirrored = headerOrdinal % 2 == 1;
                    var insertionX = mirrored ? module.StartX + module.Length : module.StartX;
                    group.Placements.Add(new HeaderPlacement(insertionX, mirrored));
                    headerOrdinal++;
                }
                else if (module.Kind == DynamicRackModuleKind.Separator && module.Length > 0.0 && context.SeparatorBlock != null)
                {
                    foreach (var level in context.Levels)
                    {
                        loose.Add(MakeSeparator(
                            context,
                            module.StartX,
                            module.Length,
                            level,
                            module.Index + 1 == sectionRange.StartPosition));
                    }
                }
            }

            // Derived posts: where two separators are consecutive there is a shared post (header post + plate),
            // reinforced full height by default.
            var rangeStartX = system.Modules.FirstOrDefault(module => module.Index + 1 == sectionRange.StartPosition)?.StartX ?? 0.0;
            var rangeEndX = system.Modules.FirstOrDefault(module => module.Index + 1 == sectionRange.EndPosition)?.EndX ?? system.TotalLength;
            foreach (var offset in system.GetDerivedPostOffsets().Where(offset => offset > rangeStartX && offset < rangeEndX))
            {
                AddDerivedPost(loose, context, offset, context.ReinforceDerivedPost);
            }
            foreach (var offset in DynamicDepthGeometry.BoundaryPostOffsets(system, sectionRange))
            {
                AddDerivedPost(loose, context, offset, reinforced: false);
            }

            var intermediateBeams = intermediateBeamBuilder.Build(system, catalog, postIndex, levelCount);
            loose.AddRange(intermediateBeams.LooseInstances);

            var placements = sectioned
                ? DynamicFrontGeometry.AdjacentFronts(system, postIndex)
                    .SelectMany(front => DynamicLoadBeamGeometry.Placements(system, front))
                : DynamicLoadBeamGeometry.Placements(system);
            foreach (var placement in placements
                         .Where(placement => placement.LevelNumber <= levelCount)
                         .GroupBy(placement => string.Join("|",
                             placement.LevelNumber,
                             placement.IsEntrance,
                             placement.X.ToString("0.####", CultureInfo.InvariantCulture),
                             placement.Y.ToString("0.####", CultureInfo.InvariantCulture),
                             placement.BeamCatalogId,
                             placement.BeamDepth.ToString("0.####", CultureInfo.InvariantCulture)))
                         .Select(group => group.First()))
            {
                var beam = MakeLoadBeam(catalog, placement);
                if (!string.IsNullOrWhiteSpace(beam.BlockName))
                {
                    loose.Add(beam);
                }
            }

            var headers = order.Select(signature => groups[signature].ToGroup()).ToList();
            headers.AddRange(intermediateBeams.Headers);
            IReadOnlyList<DynamicRackFront> bedFronts = sectioned
                ? DynamicFrontGeometry.AdjacentFronts(system, postIndex)
                : Array.Empty<DynamicRackFront>();
            if (bedFronts.Count == 0)
            {
                var flowBed = flowBedBuilder.Build(system, catalog, levelCount);
                if (flowBed != null)
                {
                    headers.Add(flowBed);
                }
            }
            else
            {
                foreach (var front in bedFronts
                             .GroupBy(front => string.Join("|", front.StartX, front.EndX, front.LoadLevels))
                             .Select(group => group.First()))
                {
                    var flowBed = flowBedBuilder.Build(system, catalog, front, Math.Min(levelCount, front.LoadLevels));
                    if (flowBed != null)
                    {
                        headers.Add(flowBed);
                    }
                }
            }

            // Endpoint safety uses the plate/post instances after every header placement has been transformed. This
            // keeps custom/mirrored cabeceras authoritative instead of reproducing their plate offsets a second time.
            var structuralPlan = new DynamicSystemPlan(headers, loose);
            loose.AddRange(safetyBuilder.Build(
                system,
                catalog,
                structuralPlan.Flatten().Instances,
                sectioned ? postIndex : 0,
                levelCount,
                rangeStartX,
                rangeEndX,
                sectioned ? DynamicFrontGeometry.AdjacentFronts(system, postIndex) : null));
            DynamicViewDecorations.AppendLateral(
                loose,
                system,
                sectionHeight,
                levelCount,
                rangeStartX,
                rangeEndX);

            return new DynamicSystemPlan(headers, loose);
        }

        public IReadOnlyList<DynamicLateralCorte> Cortes(DynamicRackSystem system, RackCatalog catalog)
        {
            var result = new List<DynamicLateralCorte>();
            var layout = DynamicFrontGeometry.Compute(system, catalog);
            for (var postIndex = 0; postIndex < layout.PostPositions.Count; postIndex++)
            {
                result.Add(new DynamicLateralCorte(
                    postIndex,
                    layout.PostPositions[postIndex],
                    Build(system, catalog, postIndex)));
            }

            return result;
        }

        /// <summary>
        /// Signature of a built header's geometry: two headers share a block definition only when this matches
        /// (so any edit that changes the drawing — position, block, rotation, mirror, dynamic length — separates
        /// them). Order-independent.
        /// </summary>
        private static string LayoutSignature(IReadOnlyList<HeaderBlockInstance> instances)
        {
            var parts = instances.Select(instance => string.Join("|",
                instance.Role,
                instance.BlockName,
                instance.View,
                instance.Insertion.X.ToString("0.###", CultureInfo.InvariantCulture),
                instance.Insertion.Y.ToString("0.###", CultureInfo.InvariantCulture),
                instance.RotationRadians.ToString("0.#####", CultureInfo.InvariantCulture),
                instance.MirroredX,
                string.Join(",", instance.DynamicParameters
                    .OrderBy(p => p.Key, StringComparer.Ordinal)
                    .Select(p => p.Key + "=" + p.Value.ToString("0.###", CultureInfo.InvariantCulture)))));

            return string.Join(";", parts.OrderBy(part => part, StringComparer.Ordinal));
        }

        private static string HeaderName(RackFrameConfiguration c)
        {
            return string.Format(CultureInfo.InvariantCulture, "Cabecera F{0:0.##} A{1:0.##}", c.Depth, c.Height);
        }

        private HeaderContext Resolve(DynamicRackSystem system, RackCatalog catalog, double height)
        {
            var context = new HeaderContext
            {
                SeparatorId = DynamicRackDefaults.SeparatorCatalogId,
                SeparatorBlock = Block(catalog, DynamicRackDefaults.SeparatorCatalogId, DynamicRackDefaults.SeparatorView),
                SeparatorMate = Local(catalog, DynamicRackDefaults.SeparatorCatalogId, DynamicRackDefaults.SeparatorMatePoint, DynamicRackDefaults.SeparatorView),
                Levels = Array.Empty<double>()
            };

            var headerModule = system.Modules.FirstOrDefault(m => m.IsHeader && m.AssociatedFrameConfiguration != null);
            if (headerModule == null)
            {
                return context;
            }

            var configuration = headerModule.AssociatedFrameConfiguration;
            context.Height = height > 0.0 ? height : configuration.Height;
            context.PostId = configuration.LeftPost?.PostCatalogId;
            context.PlateId = configuration.LeftBasePlate?.PlateCatalogId;
            var troquelSeparador = Local(catalog, context.PostId, DynamicRackDefaults.SeparatorPostPoint, "LATERAL");
            context.TroquelSeparadorX = troquelSeparador.X;
            context.Montaje = Local(catalog, context.PlateId, "MONTAJE_POSTE", "LATERAL");
            context.FinPoste = Local(catalog, context.PostId, "FIN_POSTE", "LATERAL");
            context.PostBlock = Block(catalog, context.PostId, "LATERAL");
            context.PlateBlock = Block(catalog, context.PlateId, "LATERAL");

            context.Levels = DynamicSeparatorGeometry.Levels(system, catalog, context.Height);

            context.ReinforceDerivedPost = system.DerivedPostReinforced;
            context.DerivedReinforcementHeight =
                system.DerivedPostReinforcementHeight.HasValue && system.DerivedPostReinforcementHeight.Value > 0.0
                    ? system.DerivedPostReinforcementHeight.Value
                    : context.Height;
            return context;
        }

        private static HeaderBlockInstance MakeSeparator(
            HeaderContext context,
            double moduleStartX,
            double moduleLength,
            double level,
            bool startsAtBoundaryPost)
        {
            // Anchor the separator's TROQUEL_CABECERA on the previous header's right-post TROQUEL_SEPARADOR
            // (that post is mirrored, so its troquel sits one offset inside the module start). Its length is
            // the separation between headers (the module length), as shown in the preview.
            var anchorX = startsAtBoundaryPost
                ? moduleStartX + context.TroquelSeparadorX
                : moduleStartX - context.TroquelSeparadorX;
            var length = moduleLength;
            var anchor = new Point2D(anchorX, level);

            var instance = new HeaderBlockInstance
            {
                Role = HeaderBlockRole.Separator,
                PieceId = context.SeparatorId,
                BlockName = context.SeparatorBlock,
                View = DynamicRackDefaults.SeparatorView,
                RotationRadians = 0.0,
                ConnectionAnchor = anchor,
                Insertion = new Point2D(anchor.X - context.SeparatorMate.X, anchor.Y - context.SeparatorMate.Y)
            };
            instance.DynamicParameters[SelectiveRackDefaults.LengthParam] = length;
            return instance;
        }

        private static HeaderBlockInstance MakeLoadBeam(RackCatalog catalog, DynamicLoadBeamPlacement placement)
        {
            var origin = new Point2D(placement.X, placement.Y);
            var beamId = string.IsNullOrWhiteSpace(placement.BeamCatalogId)
                ? DynamicRackDefaults.InOutBeamCatalogId
                : placement.BeamCatalogId;
            var result = new HeaderBlockInstance
            {
                Role = HeaderBlockRole.Beam,
                PieceId = beamId,
                BlockName = Block(catalog, beamId, DynamicRackDefaults.InOutBeamView),
                View = DynamicRackDefaults.InOutBeamView,
                ConnectionAnchor = origin,
                Insertion = origin,
                MirroredX = placement.MirroredX
            };
            return result;
        }

        private static void AddDerivedPost(
            ICollection<HeaderBlockInstance> instances,
            HeaderContext context,
            double offset,
            bool reinforced)
        {
            if (string.IsNullOrWhiteSpace(context.PostId) || context.Height <= 0.0)
            {
                return;
            }

            var placement = DynamicDerivedPostGeometry.Resolve(
                offset,
                reinforced,
                context.FinPoste);
            var origin = placement.PrimaryOrigin;

            // Base plate (same as the header's), mated at the post origin.
            instances.Add(new HeaderBlockInstance
            {
                Role = HeaderBlockRole.BasePlate,
                PieceId = context.PlateId,
                BlockName = context.PlateBlock,
                View = "LATERAL",
                ConnectionAnchor = origin,
                Insertion = new Point2D(origin.X - context.Montaje.X, origin.Y - context.Montaje.Y)
            });

            // The post itself, stretched to the header height.
            var post = new HeaderBlockInstance
            {
                Role = HeaderBlockRole.Post,
                PieceId = context.PostId,
                BlockName = context.PostBlock,
                View = "LATERAL",
                ConnectionAnchor = origin,
                Insertion = origin
            };
            post.DynamicParameters[SelectiveRackDefaults.LengthParam] = context.Height;
            instances.Add(post);

            // Optional reinforcement: a second post mated at FIN_POSTE (reinforced by default, full height).
            if (placement.HasReinforcement)
            {
                var reinforcementOrigin = placement.ReinforcementOrigin;
                var reinforcement = new HeaderBlockInstance
                {
                    Role = HeaderBlockRole.Post,
                    PieceId = context.PostId,
                    BlockName = context.PostBlock,
                    View = "LATERAL",
                    ConnectionAnchor = reinforcementOrigin,
                    Insertion = reinforcementOrigin
                };
                reinforcement.DynamicParameters[SelectiveRackDefaults.LengthParam] = context.DerivedReinforcementHeight;
                instances.Add(reinforcement);
            }
        }

        private static Point2D Local(RackCatalog catalog, string pieceId, string connectionPointId, string view)
            => CatalogLookup.Local(catalog, pieceId, connectionPointId, view);

        private static string Block(RackCatalog catalog, string pieceId, string view)
            => CatalogLookup.Block(catalog, pieceId, view);

        private sealed class HeaderGroupBuilder
        {
            public HeaderGroupBuilder(string name, IReadOnlyList<HeaderBlockInstance> instances)
            {
                Name = name;
                Instances = instances;
                Placements = new List<HeaderPlacement>();
            }

            public string Name { get; }
            public IReadOnlyList<HeaderBlockInstance> Instances { get; }
            public List<HeaderPlacement> Placements { get; }

            public HeaderGroup ToGroup() => new HeaderGroup(Name, Instances, Placements);
        }

        private sealed class HeaderContext
        {
            public double Height { get; set; }
            public string PostId { get; set; }
            public string PlateId { get; set; }
            public string PostBlock { get; set; }
            public string PlateBlock { get; set; }
            public Point2D Montaje { get; set; }
            public Point2D FinPoste { get; set; }
            public double TroquelSeparadorX { get; set; }

            public string SeparatorId { get; set; }
            public string SeparatorBlock { get; set; }
            public Point2D SeparatorMate { get; set; }
            public IReadOnlyList<double> Levels { get; set; }


            public bool ReinforceDerivedPost { get; set; } = true;
            public double DerivedReinforcementHeight { get; set; }
        }
    }
}
