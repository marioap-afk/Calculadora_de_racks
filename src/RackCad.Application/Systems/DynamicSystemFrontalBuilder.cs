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
    /// Builds one transverse end cut of a pallet-flow system. Exit and entrance share the same front/post grid;
    /// only their resolved beam elevations differ. Because this is a cut, it intentionally draws no beds and no
    /// intermediate supports: the only beams are complete IN/OUT beams, one per front and load level.
    /// </summary>
    public sealed class DynamicSystemFrontalBuilder
    {
        private const string View = "FRONTAL";
        private readonly DynamicSafetyMultiViewBuilder safetyBuilder = new DynamicSafetyMultiViewBuilder();

        public DynamicSystemPlan BuildPlan(
            DynamicRackSystem system, RackCatalog catalog, DynamicRackEnd end,
            RackLevelElevations elevations = null)
            => HeaderInstanceGrouper.Group(
                Build(system, catalog, end, elevations),
                end == DynamicRackEnd.Entrance ? "DIN_FRONTAL_ENTRADA" : "DIN_FRONTAL_SALIDA");

        /// <param name="elevations">
        /// Override OPCIONAL de elevaciones (PB-004, I-32). Solo afecta al corte BAJO: ahí el larguero IN/OUT se
        /// coloca directamente en la elevación derivada de SU frente, sin reasientos posteriores. Con <c>null</c>
        /// el plan es byte-idéntico al de siempre.
        /// </param>
        public IReadOnlyList<HeaderBlockInstance> Build(
            DynamicRackSystem system,
            RackCatalog catalog,
            DynamicRackEnd end,
            RackLevelElevations elevations = null)
        {
            var instances = new List<HeaderBlockInstance>();
            if (system == null || system.Fronts.Count == 0 || system.LoadBeamLevels.Count == 0)
            {
                return instances;
            }

            var layout = DynamicFrontGeometry.Compute(system, catalog);
            if (layout.PostPositions.Count == 0)
            {
                return instances;
            }

            var postId = DynamicFrontGeometry.PostId(system, catalog);
            var plateId = DynamicFrontGeometry.PlateId(system, catalog);
            var postBlock = CatalogLookup.Block(catalog, postId, View);
            var plateBlock = CatalogLookup.Block(catalog, plateId, View);
            var postPeralte = DynamicFrontGeometry.PostPeralte(system, catalog, postId);
            var platePeralte = ResolvePlatePeralte(system, catalog, plateId, postPeralte);
            var plateMate = CatalogLookup.Local(catalog, plateId, SelectiveRackDefaults.PlateMatePoint, View);

            for (var postIndex = 0; postIndex < layout.PostPositions.Count; postIndex++)
            {
                // I-33 (Owner): la frontera compartida por dos frentes EN BLANCO no tiene poste ni placa. Se omite el
                // ENSAMBLE, no la coordenada: el resto de las fronteras conserva su X exacta.
                if (!DynamicFrontActivation.BoundaryExists(system, postIndex))
                {
                    continue;
                }

                var x = layout.PostPositions[postIndex];
                var origin = new Point2D(x, 0.0);
                var post = new HeaderBlockInstance
                {
                    Role = HeaderBlockRole.Post,
                    PieceId = postId,
                    BlockName = postBlock,
                    View = View,
                    ConnectionAnchor = origin,
                    Insertion = origin
                };
                post.DynamicParameters[SelectiveRackDefaults.LengthParam] = DynamicFrontGeometry.PostHeight(system, postIndex);
                post.DynamicParameters[SelectiveRackDefaults.PeralteParam] = postPeralte;
                instances.Add(post);

                if (!string.IsNullOrWhiteSpace(plateId))
                {
                    var plate = new HeaderBlockInstance
                    {
                        Role = HeaderBlockRole.BasePlate,
                        PieceId = plateId,
                        BlockName = plateBlock,
                        View = View,
                        ConnectionAnchor = origin,
                        Insertion = new Point2D(origin.X - plateMate.X, origin.Y - plateMate.Y)
                    };
                    if (platePeralte > 0.0)
                    {
                        plate.DynamicParameters[SelectiveRackDefaults.PeralteParam] = platePeralte;
                    }

                    instances.Add(plate);
                }
            }

            for (var index = 0; index < system.Fronts.Count; index++)
            {
                var front = system.Fronts[index];
                var beamX = layout.PostPositions[index] + layout.TroquelPositions[index];
                foreach (var level in DynamicFrontGeometry.LoadBeamLevels(system, front))
                {
                    var configuration = DynamicRackLevelGeometry.At(system, front, level.LevelNumber);
                    var beamId = configuration.InOutBeamCatalogId;
                    // El larguero bajo se coloca YA en su elevación definitiva: se pregunta por FRENTE, que es a
                    // quien pertenece la pieza. Antes Push Back lo movía después, localizándolo por coordenada; ese
                    // segundo reasiento desaparece y con él el riesgo de aplicarlo dos veces o de no encontrarlo
                    // (PB-004, I-32). El extremo ALTO es el ancla y no admite override.
                    var y = end == DynamicRackEnd.Entrance
                        ? level.EntranceElevation
                        : elevations.OrFront(front.Index, level.LevelNumber, level.ExitElevation);
                    var at = new Point2D(beamX, y);
                    var beam = new HeaderBlockInstance
                    {
                        Role = HeaderBlockRole.Beam,
                        PieceId = beamId,
                        BlockName = CatalogLookup.Block(catalog, beamId, View),
                        View = View,
                        ConnectionAnchor = at,
                        Insertion = at
                    };
                    beam.DynamicParameters[SelectiveRackDefaults.LengthParam] = front.BeamLength;
                    beam.DynamicParameters[SelectiveRackDefaults.PeralteParam] = configuration.InOutBeamDepth;
                    instances.Add(beam);
                }
            }

            safetyBuilder.AppendFrontal(instances, system, catalog, layout, plateId, end, elevations);
            DynamicViewDecorations.AppendFrontal(instances, system, layout, end, catalog, elevations);

            return instances;
        }

        private static double ResolvePlatePeralte(
            DynamicRackSystem system,
            RackCatalog catalog,
            string plateId,
            double postPeralte)
        {
            var configuration = system.Modules.FirstOrDefault(module => module.IsHeader
                && module.AssociatedFrameConfiguration != null)?.AssociatedFrameConfiguration;
            var manual = configuration?.LeftBasePlate?.PeralteOverride;
            return manual ?? catalog?.BasePlates.FindBasePlate(plateId)?.StandardPeralte(postPeralte) ?? 0.0;
        }
    }
}
