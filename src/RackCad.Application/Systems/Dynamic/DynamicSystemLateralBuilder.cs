using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Geometry;
using RackCad.Application.RackFrames;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.Selective;

namespace RackCad.Application.Systems.Dynamic
{
    /// <summary>
    /// Builds the block plan for a whole dynamic (pallet flow) system in the lateral view: each header
    /// module is the lateral header (reused from <see cref="LateralHeaderLayoutBuilder"/>) shifted to its
    /// position along the run; each separator module gets a separator beam at every vertical level
    /// (<see cref="SeparatorLevelCalculator"/>), anchored on the adjacent post's TROQUEL_SEPARADOR; and a
    /// derived post (header post + plate, reinforced full height by default) is placed wherever two
    /// separators meet. Every resolved load level also receives one complete IN beam, one complete OUT beam and one
    /// complete roller bed composed by <see cref="DynamicFlowBedLateralBuilder"/>. Pure: returns a
    /// <see cref="HeaderRunPlan"/> the AutoCAD drawer turns into a block. Headers, beds and IN/OUT beams use
    /// LATERAL blocks; separators use the FRONTAL block.
    /// </summary>
    public sealed class DynamicSystemLateralBuilder
    {
        private readonly LateralHeaderLayoutBuilder headerBuilder = new LateralHeaderLayoutBuilder();
        private readonly DynamicFlowBedLateralBuilder flowBedBuilder = new DynamicFlowBedLateralBuilder();
        private readonly DynamicIntermediateBeamLateralBuilder intermediateBeamBuilder = new DynamicIntermediateBeamLateralBuilder();
        private readonly DynamicSafetyLateralBuilder safetyBuilder = new DynamicSafetyLateralBuilder();

        public HeaderRunPlan Build(
            DynamicRackSystem system, RackCatalog catalog, RackLevelElevations elevations = null)
            => BuildCore(system, catalog, -1, elevations);

        /// <summary>Builds the lateral section at one transverse post of the front grid.</summary>
        /// <param name="elevations">
        /// Override OPCIONAL de elevaciones (PB-004, I-32). Con <c>null</c> el plan es byte-idéntico al de siempre.
        /// </param>
        public HeaderRunPlan Build(
            DynamicRackSystem system, RackCatalog catalog, int postIndex, RackLevelElevations elevations = null)
        {
            // I-33 (Owner): una frontera compartida por dos frentes EN BLANCO no existe, así que su sección está
            // vacía. El guardia va AQUÍ y no sólo en Cortes, para que el dibujo, el preview y cualquier consumidor
            // directo del corte lean la misma regla y no puedan divergir.
            if (system == null
                || postIndex < 0
                || postIndex > system.Fronts.Count
                || !DynamicFrontActivation.BoundaryExists(system, postIndex))
            {
                return new HeaderRunPlan(new List<HeaderGroup>(), new List<HeaderBlockInstance>());
            }

            return BuildCore(system, catalog, postIndex, elevations);
        }

        private HeaderRunPlan BuildCore(
            DynamicRackSystem system, RackCatalog catalog, int postIndex, RackLevelElevations elevations)
        {
            if (system == null)
            {
                return new HeaderRunPlan(new List<HeaderGroup>(), new List<HeaderBlockInstance>());
            }

            var sectioned = postIndex >= 0;
            var levelCount = sectioned
                ? DynamicFrontGeometry.LoadLevelsAtPost(system, postIndex)
                : system.LoadBeamLevels.Count;
            var sectionHeight = sectioned
                ? DynamicFrontGeometry.PostHeight(system, postIndex)
                : DynamicFrontGeometry.Height(system);
            // Un CORTE dibuja la estructura de SU linea, asi que respeta su cobertura: si un frente compuesto no
            // usa una parte de la profundidad, alli no hay cabecera que dibujar (I-42, error 4). El lateral general
            // sigue siendo la envolvente del rack entero.
            var sectionCoverage = sectioned
                ? DynamicDepthGeometry.CoverageAtPost(system, postIndex)
                : new DynamicDepthCoverage(new[] { new DynamicDepthRange(1, system.PalletsDeep) });
            var sectionRange = new DynamicDepthRange(
                sectionCoverage.StartPosition,
                sectionCoverage.EndPosition - sectionCoverage.StartPosition + 1);
            var context = Resolve(system, catalog, sectionHeight, postIndex);
            var loose = new List<HeaderBlockInstance>();

            // I-42 (ronda 6D) — el contexto de altura POR TRAMO DE PROFUNDIDAD. Separadores y postes derivados
            // cuelgan de la altura de la cabecera que tienen al lado, y en un rack compuesto esa altura cambia a la
            // mitad de la profundidad: la primera mitad es del lado A y la segunda del B. Sin zonas declaradas hay
            // un solo contexto y es el de siempre, asi que el Dinamico no cambia ni un bit.
            var contextsByHeight = new Dictionary<double, HeaderContext> { [context.Height] = context };
            HeaderContext ContextAt(double x)
            {
                if (system.HeaderHeightZones.Count == 0)
                {
                    return context;
                }

                // I-42 (A1B-D6): el lateral GENERAL tampoco puede usar una altura aplanada. No representa una linea,
                // asi que su altura en una profundidad es la ENVOLVENTE de esa zona; un corte si tiene linea y
                // pregunta por ella. Las dos leen la misma autoridad, y sin zonas ninguna de las dos cambia.
                var height = sectioned
                    ? DynamicFrontGeometry.PostHeightAt(system, postIndex, x)
                    : DynamicFrontGeometry.HeightAt(system, x);
                if (height <= 0.0 || Math.Abs(height - context.Height) < 1e-6)
                {
                    return context;
                }

                if (!contextsByHeight.TryGetValue(height, out var zoned))
                {
                    zoned = Resolve(system, catalog, height, postIndex);
                    contextsByHeight[height] = zoned;
                }

                return zoned;
            }

            // Group identical headers so each distinct header becomes one shared block definition; record the
            // placements (with a mirror flag) where each is used. Consecutive headers alternate (mirror) so the
            // celosía direction alternates along the line.
            var groups = new Dictionary<string, HeaderGroupBuilder>();
            var order = new List<string>();
            var headerOrdinal = 0;

            foreach (var module in DynamicDepthGeometry.ModulesInCoverage(system, sectionCoverage))
            {
                if (module.IsHeader && module.AssociatedFrameConfiguration != null)
                {
                    // I-42 (A3-H3R): las DOS vistas preguntan por la configuracion EFECTIVA de la cabecera; lo
                    // unico que cambia es la pregunta de altura —la linea en el corte, la envolvente de la zona en
                    // el general—. Tomar aqui el objeto asociado tal cual dibujaba la altura que tenia ANTES de que
                    // se resolviera la demanda por zona, de modo que la misma cabecera fisica salia con una altura
                    // en el lateral general y con otra en su corte y en el BOM.
                    var configuration = sectioned
                        ? DynamicFrontGeometry.HeaderConfigurationAtPost(system, module, catalog, postIndex)
                        : DynamicFrontGeometry.HeaderConfigurationAt(system, module, catalog);

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
                    var separatorContext = ContextAt(0.5 * (module.StartX + module.EndX));
                    foreach (var level in separatorContext.Levels)
                    {
                        loose.Add(MakeSeparator(
                            separatorContext,
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
                AddDerivedPost(loose, ContextAt(offset), offset, context.ReinforceDerivedPost);
            }
            foreach (var offset in DynamicDepthGeometry.BoundaryPostOffsets(system, sectionCoverage))
            {
                AddDerivedPost(loose, ContextAt(offset), offset, reinforced: false);
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
                // A blank front is physically at this post — its posts and header are drawn above — but it carries no
                // bed, so it is dropped here rather than from AdjacentFronts (I-33).
                foreach (var front in DynamicFrontActivation.Active(bedFronts)
                             .GroupBy(front => string.Join("|", front.StartX, front.EndX, front.LoadLevels))
                             .Select(group => group.First()))
                {
                    var flowBed = flowBedBuilder.Build(
                        system,
                        catalog,
                        front,
                        Math.Min(levelCount, DynamicFrontActivation.EffectiveLoadLevels(front)));
                    if (flowBed != null)
                    {
                        headers.Add(flowBed);
                    }
                }
            }

            // Endpoint safety uses the plate/post instances after every header placement has been transformed. This
            // keeps custom/mirrored cabeceras authoritative instead of reproducing their plate offsets a second time.
            var structuralPlan = new HeaderRunPlan(headers, loose);
            loose.AddRange(safetyBuilder.Build(
                system,
                catalog,
                structuralPlan.Flatten().Instances,
                sectioned ? postIndex : 0,
                levelCount,
                rangeStartX,
                rangeEndX,
                sectioned ? DynamicFrontGeometry.AdjacentFronts(system, postIndex) : null,
                elevations));
            DynamicViewDecorations.AppendLateral(
                loose,
                system,
                sectionHeight,
                levelCount,
                rangeStartX,
                rangeEndX,
                sectioned ? postIndex : -1,
                elevations);

            return new HeaderRunPlan(headers, loose);
        }

        public IReadOnlyList<DynamicLateralCorte> Cortes(DynamicRackSystem system, RackCatalog catalog)
        {
            var result = new List<DynamicLateralCorte>();
            var layout = DynamicFrontGeometry.Compute(system, catalog);
            for (var postIndex = 0; postIndex < layout.PostPositions.Count; postIndex++)
            {
                // I-33 (Owner): sin frontera física no hay corte que dibujar. Los cortes que sobreviven conservan su
                // ÍNDICE de poste original, así que nada se renumera.
                if (!DynamicFrontActivation.BoundaryExists(system, postIndex))
                {
                    continue;
                }

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

        private HeaderContext Resolve(DynamicRackSystem system, RackCatalog catalog, double height, int postIndex)
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

            // I-40 (Owner): la ALTURA del poste derivado, resuelta por la autoridad unica: primero la de ESTA linea,
            // luego la del rack, y si ninguna, la de la cabecera — que es lo que este poste heredaba antes de que el
            // campo existiera, de modo que un rack sin valores dibuja exactamente igual que siempre.
            context.DerivedPostHeight = DynamicFrontGeometry.DerivedPostHeightAtPost(system, postIndex, context.Height);
            // Refuerzo vacio = TODA la altura del poste derivado, que es lo que "a toda la altura" significa. Cuando
            // nadie fija la altura del poste, esa altura ES la de la cabecera, asi que el valor historico no cambia.
            context.DerivedReinforcementHeight =
                system.DerivedPostReinforcementHeight.HasValue && system.DerivedPostReinforcementHeight.Value > 0.0
                    ? system.DerivedPostReinforcementHeight.Value
                    : context.DerivedPostHeight;
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
            post.DynamicParameters[SelectiveRackDefaults.LengthParam] = context.DerivedPostHeight;
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

            /// <summary>I-40: LONGITUD efectiva del poste derivado; por defecto la altura de la cabecera.</summary>
            public double DerivedPostHeight { get; set; }
            public double DerivedReinforcementHeight { get; set; }
        }
    }
}
