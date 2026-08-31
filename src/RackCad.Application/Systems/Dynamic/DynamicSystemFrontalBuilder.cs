using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Geometry;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.Selective;

namespace RackCad.Application.Systems.Dynamic
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

        public HeaderRunPlan BuildPlan(
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
        /// <param name="headerHeightAtPost">
        /// I-42 (ronda 6B) — la ALTURA de la cabecera de cada linea, cuando quien llama la resuelve sobre OTRO
        /// modelo. Un rack compuesto la necesita porque su corte frontal se construye sobre el sistema LOCAL del
        /// lado —un modelo de trabajo— mientras la pieza que se fabrica y que el lateral dibuja pertenece a la
        /// estructura COMPUESTA. Con <c>null</c> la altura sale de este mismo sistema, que es lo que hace el
        /// Dinamico y cualquier Push Back de un solo sentido.
        /// </param>
        public IReadOnlyList<HeaderBlockInstance> Build(
            DynamicRackSystem system,
            RackCatalog catalog,
            DynamicRackEnd end,
            RackLevelElevations elevations = null,
            Func<int, int, bool> ownsDesviador = null,
            Func<int, bool> ownsBoundary = null,
            Func<int, double> headerHeightAtPost = null)
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

                // I-42: un corte frontal es de UN LADO. La linea que solo el otro lado necesita existe en el rack
                // —y la planta la dibuja— pero este corte no la posee. Sin filtro no cambia nada.
                if (ownsBoundary != null && !ownsBoundary(postIndex))
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
                // I-40 (Owner) — esta vista es un CORTE, no una envolvente. La ronda 5 hizo que leyera la
                // configuracion de la cabecera (antes usaba PostHeight, un derivado que la ignoraba), pero tomando
                // la MAS ALTA de la linea; el Owner precisó que la frontal corta por la PRIMERA cabecera
                // longitudinal y la posterior por la ULTIMA. `end` elige cual, `postIndex` elige de que linea es el
                // poste: los dos ejes conviven sin colapsarse.
                // I-42 (ronda 6B) — UNA linea fisica, UNA altura. Cuando quien llama la resuelve sobre el modelo
                // que manda —la estructura compuesta— se consume esa; si no, la de este sistema. Nunca se ajusta
                // graficamente: es la misma propiedad, leida de la autoridad correcta.
                var resolvedHeight = headerHeightAtPost != null
                    ? headerHeightAtPost(postIndex)
                    : 0.0;
                post.DynamicParameters[SelectiveRackDefaults.LengthParam] = resolvedHeight > 0.0
                    ? resolvedHeight
                    : DynamicFrontGeometry.HeaderHeightAtPost(system, catalog, postIndex, end);
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
                var levels = DynamicFrontGeometry.LoadBeamLevels(system, front);
                for (var levelIndex = 0; levelIndex < levels.Count; levelIndex++)
                {
                    var level = levels[levelIndex];
                    var configuration = DynamicRackLevelGeometry.At(system, front, level.LevelNumber);
                    var beamId = configuration.InOutBeamCatalogId;
                    // El larguero se coloca YA en su elevación definitiva: se pregunta por FRENTE, que es a quien
                    // pertenece la pieza. Antes Push Back lo movía después, localizándolo por coordenada; ese segundo
                    // reasiento desaparece y con él el riesgo de aplicarlo dos veces o de no encontrarlo (PB-004).
                    // El override vale en LOS DOS extremos: el contexto describe uno u otro y quien lo pasa sabe
                    // cuál. Sin contexto —el Dinámico, siempre— se usa la elevación del resolver y nada cambia.
                    //
                    // I-42 (A1B-D4): la columna y la elevación son la IDENTIDAD de la pieza, y quien la busca
                    // despues pregunta a la misma autoridad. No hay dos formulas.
                    var key = DynamicEndBeamIdentity.KeyOf(layout, elevations, front, index, level, levelIndex, end);
                    var at = new Point2D(key.ColumnX, key.Elevation);
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

            safetyBuilder.AppendFrontal(
                instances, system, catalog, layout, plateId, end, elevations, ownsDesviador, ownsBoundary);
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
