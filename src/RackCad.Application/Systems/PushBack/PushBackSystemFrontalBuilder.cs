using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Geometry;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.Shared;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;

namespace RackCad.Application.Systems.PushBack
{
    /// <summary>The two Push Back frontal cuts. Push Back is LIFO, so both are at the SAME (low) aisle, but they show
    /// opposite ends of the lane.</summary>
    public enum PushBackFrontalEnd
    {
        /// <summary>The entrance/exit (low) cut: complete IN/OUT beams + the applicable safety (never a guide).</summary>
        EntradaSalida,

        /// <summary>The rear (high) cut: LARGUERO_ESCALON_TROQUEL_REDONDO beams + rear topes, no normal dynamic safety.</summary>
        Posterior
    }

    /// <summary>
    /// Frontal Push Back cuts by BLACK-BOX composition of <see cref="DynamicSystemFrontalBuilder"/>. It reuses the dynamic
    /// posts/plates/transverse structure but substitutes the beams and safety: <see cref="PushBackFrontalEnd.EntradaSalida"/>
    /// keeps the dynamic exit cut (its safety is already GUIA-free); <see cref="PushBackFrontalEnd.Posterior"/> takes the
    /// dynamic entrance cut, removes every IN/OUT beam and all dynamic safety, and adds one TROQUEL_REDONDO per cell plus a
    /// rear tope per active cell. Instances are identified by Role/PieceId/position, never by group name.
    /// </summary>
    public sealed class PushBackSystemFrontalBuilder
    {
        private const string View = "FRONTAL";
        private readonly DynamicSystemFrontalBuilder dynamicBuilder = new DynamicSystemFrontalBuilder();

        public HeaderRunPlan BuildPlan(PushBackSystem system, RackCatalog catalog, PushBackFrontalEnd end)
            => BuildPlan(system, catalog, end, PushBackSide.A);

        /// <summary>
        /// I-42 — el corte frontal de UN lado. Un corte frontal mira a uno de los dos pasillos, asi que en un rack
        /// compuesto hay cuatro secciones utiles. Un rack de un solo sentido ignora el parametro: solo tiene lado A.
        /// </summary>
        public HeaderRunPlan BuildPlan(
            PushBackSystem system, RackCatalog catalog, PushBackFrontalEnd end, PushBackSide side)
            => WithResolvedDefense(
                WithResolvedBoots(
                    system != null && system.IsComposite
                        ? PushBackCompositeFrontal.Build(system, catalog, end, side)
                        : BuildPlan(system, catalog, end, null, null),
                    system, catalog, end, side),
                system, catalog, end, side);

        /// <summary>
        /// I-42 (A1B-D2, contrato del dueño) — LA DEFENSA DE ESTE CORTE, tomada de la resolucion fisica del rack.
        ///
        /// <para>
        /// Un corte compuesto se arma sobre el sistema LOCAL de su lado, y ese local no lleva declaradas las caras:
        /// al preguntar por su pieza caia al id general de la seleccion, asi que el corte de entrada de B dibujaba
        /// defensa aunque el tipo de B fuera «Ninguno» —y al reves—. La pertenencia y el tipo los decide ahora
        /// <see cref="PushBackDefensePlan"/>, una sola vez, sobre el rack entero; el corte solo proyecta las piezas
        /// de su Side × PhysicalPost. Un rack de un solo sentido resuelve el mismo conjunto que ya dibujaba.
        /// </para>
        /// </summary>
        private static HeaderRunPlan WithResolvedDefense(
            HeaderRunPlan plan, PushBackSystem system, RackCatalog catalog,
            PushBackFrontalEnd end, PushBackSide side)
        {
            if (plan == null || system?.Structure == null || catalog == null)
            {
                return plan;
            }

            var stripped = PushBackDefensePlan.Without(plan, catalog);
            var loose = stripped.LooseInstances.ToList();
            var plateId = DynamicFrontGeometry.PlateId(system.Structure, catalog);
            foreach (var defense in PushBackDefensePlan.AtCut(system, catalog, end, side))
            {
                var instance = PushBackDefensePlan.Instance(defense, catalog, plateId, side);
                if (instance != null)
                {
                    loose.Add(instance);
                }
            }

            return new HeaderRunPlan(stripped.Headers.ToList(), loose);
        }

        /// <summary>
        /// I-42 (S1F, contrato del dueño) — LAS BOTAS DE ESTE CORTE, tomadas de la resolucion fisica del rack.
        ///
        /// <para>
        /// Un corte no vuelve a resolver la intencion del usuario ni reinterpreta su marco: se identifica por su
        /// LADO y su EXTREMO, y se queda con las piezas cuya identidad coincide. Las cuatro secciones de un rack
        /// compuesto muestran cuatro caras distintas —exterior A, interior A, interior B, exterior B—, y las que
        /// trajera el builder compartido se retiran: la autoridad es una sola.
        /// </para>
        /// </summary>
        private static HeaderRunPlan WithResolvedBoots(
            HeaderRunPlan plan, PushBackSystem system, RackCatalog catalog,
            PushBackFrontalEnd end, PushBackSide side)
        {
            if (plan == null || system?.Structure == null || catalog == null)
            {
                return plan;
            }

            var stripped = PushBackBootPlan.Without(plan, catalog);
            var groups = stripped.Headers.ToList();
            var loose = stripped.LooseInstances.ToList();

            var plateId = DynamicFrontGeometry.PlateId(system.Structure, catalog);
            var plateMate = string.IsNullOrWhiteSpace(plateId)
                ? new Point2D(0.0, 0.0)
                : CatalogLookup.Local(catalog, plateId, SelectiveRackDefaults.PlateMatePoint, View);
            foreach (var boot in PushBackBootPlan.AtCut(system, catalog, end, side))
            {
                var block = CatalogLookup.Block(catalog, boot.PieceId, View);
                if (string.IsNullOrWhiteSpace(block))
                {
                    continue;
                }

                loose.Add(PushBackBootPlan.Instance(
                    boot, block, View, new Point2D(boot.LineX - plateMate.X, -plateMate.Y)));
            }

            return new HeaderRunPlan(groups, loose);
        }

        /// <summary>
        /// La SECCION con la que el envoltorio del DWG direcciona un corte frontal. Un rack de un solo sentido usa 0
        /// y 1, que es exactamente lo que escribieron todas las versiones anteriores; el lado B anade 2 y 3. Por eso
        /// un documento antiguo sigue apuntando al mismo corte sin migrar nada.
        /// </summary>
        public static int EncodeSection(PushBackFrontalEnd end, PushBackSide side)
            => (int)end + (side == PushBackSide.B ? 2 : 0);

        /// <summary>El corte y el lado que una seccion direcciona. Una seccion fuera de rango cae en el corte bajo de A.</summary>
        public static (PushBackFrontalEnd End, PushBackSide Side) DecodeSection(int section)
        {
            if (section < 0 || section > 3)
            {
                return (PushBackFrontalEnd.EntradaSalida, PushBackSide.A);
            }

            return (
                (section % 2) == 1 ? PushBackFrontalEnd.Posterior : PushBackFrontalEnd.EntradaSalida,
                section >= 2 ? PushBackSide.B : PushBackSide.A);
        }

        /// <summary>Si la seccion direcciona un corte frontal valido.</summary>
        public static bool IsValidSection(int section) => section >= 0 && section <= 3;

        /// <summary>
        /// El mismo corte con dos inyecciones OPCIONALES que solo usa el rack compuesto de I-42 (con null en las dos
        /// el comportamiento es exactamente el anterior a la iniciativa):
        /// </summary>
        /// <param name="elevationsOverride">
        /// El contexto de elevaciones del extremo que este corte dibuja. Un rack compuesto lo necesita porque la
        /// elevacion de una celda corrida la gobierna la cama REAL, que puede pertenecer al otro lado. Con null se
        /// usa el contexto del propio sistema, que es lo que hace un Push Back de un solo sentido.
        /// </param>
        /// <param name="includeCell">
        /// Filtro (indiceDeFrente, nivel 0-based) de las celdas que este corte materializa. Un rack compuesto lo
        /// necesita porque una celda corrida NO tiene larguero en la linea interior del lado bajo: la calle la
        /// atraviesa. Dibujarlo seria inventar una pieza que no existe.
        /// </param>
        /// <param name="headerHeightAtPost">
        /// I-42 (ronda 6B) — la altura de la cabecera de cada linea, resuelta sobre la estructura COMPUESTA. Un
        /// rack compuesto construye este corte sobre el sistema local del lado, y el poste que ahi se dibuja es la
        /// MISMA pieza fisica que el lateral dibuja y que el BOM compra. Con <c>null</c> nada cambia.
        /// </param>
        public HeaderRunPlan BuildPlan(
            PushBackSystem system,
            RackCatalog catalog,
            PushBackFrontalEnd end,
            RackLevelElevations elevationsOverride,
            Func<int, int, bool> includeCell,
            Func<int, double> headerHeightAtPost = null)
        {
            var structure = system?.Structure;
            if (structure == null)
            {
                return new HeaderRunPlan(new List<HeaderGroup>(), new List<HeaderBlockInstance>());
            }

            if (end == PushBackFrontalEnd.EntradaSalida)
            {
                // Low cut: the dynamic exit frontal (structure is GUIA-free) already IS "IN/OUT + applicable safety".
                // PB-004 (I-32): el builder compartido recibe el contexto de elevaciones y coloca los largueros
                // IN/OUT YA en su elevación derivada, junto con el desviador bajo y las anotaciones. No hay
                // reasiento posterior: antes esta vista movía las piezas después, localizándolas por coordenada.
                var lowContext = elevationsOverride ?? PushBackElevations.Context(system, catalog);
                var low = dynamicBuilder
                    .Build(
                        structure,
                        catalog,
                        DynamicRackEnd.Exit,
                        lowContext,
                        OwnsDesviador(structure, includeCell),
                        OwnsBoundary(structure, includeCell),
                        headerHeightAtPost)
                    .ToList();
                if (includeCell != null)
                {
                    low = FilterCells(low, structure, catalog, lowContext, includeCell);
                }

                // I-41 (PB-016): las tarimas de las celdas que las piden, apoyadas sobre el contacto BAJO real de su
                // cama. Sin ninguna celda marcada no se agrega nada y el corte es el de siempre.
                low.AddRange(Tarimas(system, catalog, lowEnd: true));
                return HeaderInstanceGrouper.Group(low, "PB_FRONTAL_ENTRADA_SALIDA");
            }

            // El corte POSTERIOR tambien lleva contexto. Desde la decision final del dueño su larguero ya no es
            // el ancla: se DERIVA del bajo, asi que leer la elevacion del resolver aqui lo dibujaria en un troquel
            // distinto del que ocupa en el lateral — dos autoridades verticales para la MISMA pieza fisica.
            var highContext = elevationsOverride ?? PushBackElevations.HighContext(system, catalog);
            // El corte POSTERIOR sigue la MISMA regla de pertenencia de linea: tambien es de un lado.
            var entrance = dynamicBuilder.Build(
                structure,
                catalog,
                DynamicRackEnd.Entrance,
                highContext,
                ownsDesviador: null,
                ownsBoundary: OwnsBoundary(structure, includeCell),
                headerHeightAtPost: headerHeightAtPost);
            var layout = DynamicFrontGeometry.Compute(structure, catalog);
            var redondoId = string.IsNullOrWhiteSpace(system.HighEndBeamCatalogId)
                ? PushBackDefaults.HighEndBeamCatalogId
                : system.HighEndBeamCatalogId;
            var redondoBlock = CatalogLookup.Block(catalog, redondoId, View);
            var rearTope = system.RearTope ?? new PushBackRearTopeConfig();
            var topePieceId = PushBackRearTopeBuilder.ResolvePieceId(catalog, rearTope);   // PB-005
            var topeBlock = CatalogLookup.Block(catalog, topePieceId, View);
            var saque = rearTope.Saque > 0.0 ? rearTope.Saque : PushBackDefaults.RearTopeSaque;

            // Owner decision (2026-07-24, final): in the REAR FRONTAL the stop is anchored by the post's own TROQUEL_TOPE
            // in this view — never TROQUEL_LARGUERO, which places a beam. Both its X (which follows the post PERALTE) and
            // its Y (the elevation grid base) come from that single measured row; a missing row means no stop is emitted,
            // never a silent fallback to the insertion point.
            var postId = DynamicFrontGeometry.PostId(structure, catalog);
            var postPeralte = DynamicFrontGeometry.PostPeralte(structure, catalog, postId);
            var topeAnchor = PushBackRearTopeBuilder.PostAnchorLocal(catalog, postId, postPeralte, View);
            var troquelMateY = topeAnchor?.Y ?? 0.0;

            var result = new List<HeaderBlockInstance>();
            foreach (var instance in entrance)
            {
                if (PushBackPlanComposer.IsSafetyPiece(instance))
                {
                    continue; // no normal dynamic safety on the rear cut
                }

                if (PushBackPlanComposer.IsDynamicEndBeam(instance))
                {
                    var (frontIndex, level) = LocateCell(structure, catalog, layout, instance, highContext);
                    if (includeCell != null && level >= 0 && !includeCell(frontIndex, level))
                    {
                        continue;   // la celda no tiene larguero posterior en esta linea (I-42: la calle la atraviesa)
                    }

                    // Swap the IN/OUT for the rear TROQUEL_REDONDO, keeping the transverse LONGITUD, at the same column.
                    // PB-004 (I-32, regla del Owner tras el round 1): el posterior es el ANCLA y se queda en su troquel,
                    // así que esta vista y el corte lateral coinciden por construcción — sin desplazamiento que
                    // sincronizar (D14 de la matriz de AutoCAD del dueño).
                    var redondo = CloneAt(instance, redondoId, redondoBlock);
                    redondo.DynamicParameters[SelectiveRackDefaults.PeralteParam] = level >= 0
                        ? system.HighEndBeamPeralteAt(frontIndex, level)
                        : PushBackDefaults.HighEndBeamDefaultPeralte;
                    result.Add(redondo);

                    // Rear tope only for a MATCHED, active cell, placed by the canonical Selective rule (rise + snap).
                    if (!string.IsNullOrWhiteSpace(topeBlock) && level >= 0 && rearTope.Draws(frontIndex, level)
                        && topeAnchor.HasValue)
                    {
                        // Owner clarification (2026-07-25): the tope block mates by its ORIGIN, so its insertion sits on
                        // the POST's TROQUEL_TOPE — resolved from the post instance of this very plan, not from the
                        // beam's insertion (which is what left it on the wrong troquel). The post carries a COLUMN of
                        // stop holes every 2", so the X coincides exactly while the Y keeps the approved rise-and-snap
                        // (+4") measured from that same TROQUEL_TOPE.
                        var mate = PushBackRearTopeBuilder.PostMateWorld(
                            catalog, postId, postPeralte, View, entrance, instance.Insertion);
                        if (!mate.HasValue)
                        {
                            continue;   // no measured post point: no stop, never a raw fallback
                        }

                        var topeY = PushBackRearTopeBuilder.ElevationY(troquelMateY, instance.Insertion.Y);
                        var topeX = mate.Value.X;
                        double? longitud = instance.DynamicParameters.TryGetValue(SelectiveRackDefaults.LengthParam, out var beamLength)
                            ? beamLength + SelectiveTopePlacement.LengthAllowance
                            : (double?)null;
                        result.Add(SelectiveTopePlacement.Tope(
                            topePieceId, topeBlock, View,
                            topeX, topeY, saque, longitud,
                            mirroredX: PushBackRearTopeBuilder.Mirrored(View, instance.MirroredX)));
                    }

                    continue;
                }

                result.Add(instance); // keep posts/plates/decorations
            }

            // I-41 (PB-016): la misma tarima vista desde el otro extremo de la calle. Se apoya sobre el contacto
            // POSTERIOR real de la cama, que es el ancla de esta vista.
            result.AddRange(Tarimas(system, catalog, lowEnd: false));

            return HeaderInstanceGrouper.Group(result, "PB_FRONTAL_POSTERIOR");
        }

        /// <summary>
        /// I-41 (PB-016) — las filas de tarimas de un corte frontal: una por celda que las pide, con tantas tarimas
        /// como calles tiene el frente, repartidas sobre la LONGITUD de su larguero y apoyadas sobre el contacto real
        /// de la cama en el extremo que este corte muestra. Las medidas —columna, longitud, contacto— salen de las
        /// autoridades que ya existen (<see cref="DynamicFrontGeometry"/>, <see cref="PushBackLoadBeamGeometry"/> y
        /// <see cref="PushBackElevations"/>); aqui no se recalcula ninguna.
        /// </summary>
        private static IReadOnlyList<HeaderBlockInstance> Tarimas(PushBackSystem system, RackCatalog catalog, bool lowEnd)
        {
            var result = new List<HeaderBlockInstance>();
            var structure = system?.Structure;
            if (structure == null)
            {
                return result;
            }

            var layout = DynamicFrontGeometry.Compute(structure, catalog);
            var columns = Math.Min(layout.PostPositions.Count, layout.TroquelPositions.Count);
            var supportLocalY = PushBackTarimaPlacement.SupportLocalY(catalog);
            for (var frontIndex = 0; frontIndex < structure.Fronts.Count && frontIndex < columns; frontIndex++)
            {
                var front = structure.Fronts[frontIndex];
                var elevations = PushBackElevations.Resolve(system, catalog, front);
                var axes = PushBackFlowBedGeometry.Resolve(system, catalog, front);
                var anchorX = layout.PostPositions[frontIndex] + layout.TroquelPositions[frontIndex];
                for (var level = 0; level < DynamicFrontActivation.EffectiveLoadLevels(front); level++)
                {
                    if (!system.DrawPalletAt(frontIndex, level) || !elevations.TryGetValue(level + 1, out var elevation))
                    {
                        continue;
                    }

                    // La carga descansa sobre los RODILLOS, no sobre el contacto del larguero con el riel. La altura
                    // de apoyo se pide a la MISMA autoridad que la coloca en el lateral, evaluada en el extremo que
                    // este corte muestra, para que las tres vistas no puedan discrepar.
                    var axis = axes.FirstOrDefault(candidate => candidate.LevelNumber == level + 1);
                    if (axis.LevelNumber != level + 1)
                    {
                        continue;   // sin eje de cama no hay superficie de apoyo, y no se inventa una
                    }

                    var endX = lowEnd ? elevation.LowContact.X : elevation.RearContact.X;
                    var cell = DynamicRackLevelGeometry.At(structure, front, level + 1);
                    result.AddRange(PushBackTarimaPlacement.FrontalRow(
                        catalog,
                        Math.Max(1, front.PalletCount),
                        anchorX,
                        PushBackLoadBeamGeometry.CellBeamLength(structure, front, level + 1),
                        cell?.Bfr ?? 0.0,
                        PushBackTarimaPlacement.SupportYAt(axis, supportLocalY, endX),
                        cell?.Pallet?.Front ?? 0.0,
                        cell?.Pallet?.Height ?? 0.0));
                }
            }

            return result;
        }


        /// <summary>
        /// I-42 (correccion aislada 2C) — que LINEAS de cabecera pertenecen al lado que este corte representa.
        ///
        /// <para>
        /// La estructura fisica global es la UNION de lo que necesitan los dos lados, y la PLANTA la dibuja entera
        /// porque representa el rack. Un corte FRONTAL no: es de un lado. Si la primera ranura esta en blanco en A y
        /// solo B almacena ahi, la linea exterior existe en el rack pero el corte de A no la posee y dibujarla es
        /// inventar estructura propia.
        /// </para>
        /// <para>
        /// La regla no es nueva: es la MISMA continuidad de <c>BoundaryExists</c> —una linea sostiene algo si tiene
        /// un claro activo a izquierda o a derecha— evaluada sobre la activacion del LADO, que es exactamente lo que
        /// lleva su sub-estructura. Lo unico que decae es la excepcion de los bordes exteriores, y con razon: esos
        /// bordes siempre existen para el RACK, no para un lado. Sin filtro —cualquier rack de un solo sentido— se
        /// devuelve null y el corte es el de siempre.
        /// </para>
        /// </summary>
        private static Func<int, bool> OwnsBoundary(DynamicRackSystem structure, Func<int, int, bool> includeCell)
        {
            if (includeCell == null)
            {
                return null;
            }

            var activation = DynamicFrontActivation.FrontActivation(structure);
            return postIndex => DynamicFrontActivation.BoundaryBelongsTo(activation, postIndex);
        }

        /// <summary>
        /// I-42 — de que celdas es el DESVIADOR de este corte, en terminos de (poste, nivel).
        ///
        /// <para>
        /// Un desviador guia la tarima al ENTRAR: existe donde este corte tiene una cama que se carga por el. La
        /// pertenencia sale del MISMO conjunto de camas que gobierna los largueros —<paramref name="includeCell"/>,
        /// que el compuesto deriva de <c>PushBackRuns</c>—, no de una lectura por coordenada. Un poste es la
        /// FRONTERA de hasta dos claros, asi que lleva desviador si CUALQUIERA de los dos tiene cama en ese nivel:
        /// es la misma adyacencia con la que el builder compartido decide sus niveles y su existencia.
        /// </para>
        /// <para>
        /// Sin filtro —cualquier rack de un solo sentido— devuelve null y el corte es exactamente el de siempre.
        /// Antes la SEGURIDAD se saltaba el filtro entera, de modo que el pasillo de un lado sin cama —el lado alto
        /// de una corrida— dibujaba desviadores a la altura de SUS propios niveles, contradiciendo al lateral.
        /// </para>
        /// </summary>
        private static Func<int, int, bool> OwnsDesviador(DynamicRackSystem structure, Func<int, int, bool> includeCell)
        {
            if (includeCell == null)
            {
                return null;
            }

            var fronts = structure?.Fronts?.Count ?? 0;
            return (postIndex, levelIndex) =>
            {
                var left = postIndex - 1;
                return (left >= 0 && left < fronts && includeCell(left, levelIndex))
                    || (postIndex >= 0 && postIndex < fronts && includeCell(postIndex, levelIndex));
            };
        }

        /// <summary>
        /// I-42 — deja pasar solo las celdas que <paramref name="includeCell"/> acepta. Los largueros del corte BAJO
        /// se identifican por su COLUMNA (el frente) y por su elevacion contra la que el contexto acaba de imponer,
        /// que es exactamente donde el builder compartido los acaba de colocar. Lo que no es larguero pasa siempre:
        /// postes, placas y decoraciones son estructura y no pertenecen a una celda.
        /// </summary>
        private static List<HeaderBlockInstance> FilterCells(
            List<HeaderBlockInstance> instances,
            DynamicRackSystem structure,
            RackCatalog catalog,
            RackLevelElevations context,
            Func<int, int, bool> includeCell)
        {
            var layout = DynamicFrontGeometry.Compute(structure, catalog);
            var result = new List<HeaderBlockInstance>(instances.Count);
            foreach (var instance in instances)
            {
                if (!PushBackPlanComposer.IsDynamicEndBeam(instance))
                {
                    result.Add(instance);
                    continue;
                }

                var match = IdentifyCell(structure, layout, context, instance);

                // I-42 (ronda 8, V3) — FAIL-OPEN DECLARADO. Un larguero que no se puede atribuir a ninguna celda se
                // CONSERVA: es el comportamiento historico, y dejarlo caer borraria geometria legitima por un fallo
                // de correspondencia. No es una via silenciosa: <see cref="UnidentifiedEndBeams"/> la expone, y una
                // prueba de guardia comprueba que en el rack compuesto no se ejerce nunca.
                if (!match.Identified || includeCell(match.FrontIndex, match.Level))
                {
                    result.Add(instance);
                }
            }

            return result;
        }

        /// <summary>
        /// I-42 (ronda 8B) — EL CORTE DE UN APOYO INTERMEDIO: la cama solo ATRAVIESA este plano. Se dibuja
        /// exactamente el larguero intermedio de esa frontera, con su propia pieza y su propio peralte, y nada mas:
        /// ni bajo, ni alto, ni tope. El tope pertenece al alto y solo al alto.
        ///
        /// <para>
        /// Reutiliza el mismo recorrido que el corte posterior —el marco, las columnas y la LONGITUD transversal
        /// salen del builder dinamico— y solo SUSTITUYE la pieza, igual que el posterior sustituye el IN/OUT por el
        /// larguero redondo. No se fabrica ninguna geometria nueva.
        /// </para>
        /// </summary>
        internal HeaderRunPlan BuildIntermediatePlan(
            PushBackSystem system,
            RackCatalog catalog,
            RackLevelElevations context,
            Func<int, int, bool> includeCell,
            Func<int, double> headerHeightAtPost)
        {
            var structure = system?.Structure;
            if (structure == null)
            {
                return new HeaderRunPlan(new List<HeaderGroup>(), new List<HeaderBlockInstance>());
            }

            var instances = dynamicBuilder.Build(
                structure,
                catalog,
                DynamicRackEnd.Entrance,
                context,
                ownsDesviador: null,
                ownsBoundary: OwnsBoundary(structure, includeCell),
                headerHeightAtPost: headerHeightAtPost);
            var layout = DynamicFrontGeometry.Compute(structure, catalog);

            var result = new List<HeaderBlockInstance>();
            foreach (var instance in instances)
            {
                if (PushBackPlanComposer.IsSafetyPiece(instance))
                {
                    continue;
                }

                if (!PushBackPlanComposer.IsDynamicEndBeam(instance))
                {
                    result.Add(instance);   // marco: postes, placas y decoraciones
                    continue;
                }

                var (frontIndex, level) = LocateCell(structure, catalog, layout, instance, context);
                if (includeCell != null && level >= 0 && !includeCell(frontIndex, level))
                {
                    continue;
                }

                var front = frontIndex >= 0 && frontIndex < structure.Fronts.Count ? structure.Fronts[frontIndex] : null;
                var beamId = front != null && level >= 0
                    ? DynamicIntermediateBeamGeometry.BeamIdAt(front, level + 1)
                    : DynamicRackDefaults.IntermediateBeamCatalogId;
                var block = CatalogLookup.Block(catalog, beamId, View);
                if (string.IsNullOrWhiteSpace(block))
                {
                    continue;   // sin bloque medido no se materializa nada: nunca un sustituto silencioso
                }

                var beam = CloneAt(instance, beamId, block);
                if (front != null && level >= 0)
                {
                    beam.DynamicParameters[SelectiveRackDefaults.PeralteParam] =
                        DynamicIntermediateBeamGeometry.PeralteAt(front, level + 1);
                }

                result.Add(beam);
            }

            return HeaderInstanceGrouper.Group(result, "PB_FRONTAL_INTERMEDIO");
        }

        /// <summary>
        /// I-42 (ronda 8, V3) — LA CELDA de un larguero del corte bajo, o «no identificada».
        ///
        /// <para>
        /// La COLUMNA se resuelve por cercania y eso es legitimo: las columnas transversales teselan el eje, asi que
        /// la mas cercana es la unica posible y no hay identidad mejor disponible en una instancia ya dibujada. El
        /// NIVEL si exige coincidir con una elevacion esperada dentro de tolerancia; sin coincidencia no se inventa
        /// ninguna.
        /// </para>
        /// </summary>
        internal static FrontalCellMatch IdentifyCell(
            DynamicRackSystem structure,
            DynamicFrontLayout layout,
            RackLevelElevations context,
            HeaderBlockInstance instance)
        {
            var frontIndex = NearestColumn(structure, layout, instance.Insertion.X);
            var level = NearestLowLevel(structure, context, frontIndex, instance.Insertion.Y);
            return level < 0
                ? FrontalCellMatch.None
                : new FrontalCellMatch(frontIndex, level);
        }

        /// <summary>
        /// Cuantos largueros de <paramref name="instances"/> NO se pueden atribuir a una celda. Es el seam que hace
        /// medible el fail-open de <see cref="FilterCells"/>: una prueba de guardia lo mantiene en cero para el rack
        /// compuesto, que es el unico que filtra por celda.
        /// </summary>
        internal static int UnidentifiedEndBeams(
            DynamicRackSystem structure,
            RackCatalog catalog,
            RackLevelElevations context,
            IEnumerable<HeaderBlockInstance> instances)
        {
            if (structure == null || instances == null)
            {
                return 0;
            }

            var layout = DynamicFrontGeometry.Compute(structure, catalog);
            return instances
                .Where(PushBackPlanComposer.IsDynamicEndBeam)
                .Count(instance => !IdentifyCell(structure, layout, context, instance).Identified);
        }

        private static int NearestColumn(DynamicRackSystem structure, DynamicFrontLayout layout, double x)
        {
            var best = -1;
            var bestDistance = double.MaxValue;
            var columns = Math.Min(layout.PostPositions.Count, layout.TroquelPositions.Count);
            for (var index = 0; index < columns && index < structure.Fronts.Count; index++)
            {
                var distance = Math.Abs(layout.PostPositions[index] + layout.TroquelPositions[index] - x);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = index;
                }
            }

            return best;
        }

        private static int NearestLowLevel(
            DynamicRackSystem structure, RackLevelElevations context, int frontIndex, double y)
        {
            if (frontIndex < 0 || frontIndex >= structure.Fronts.Count)
            {
                return -1;
            }

            var front = structure.Fronts[frontIndex];
            var levels = DynamicFrontGeometry.LoadBeamLevels(structure, front);
            var best = -1;
            var bestDistance = double.MaxValue;
            for (var index = 0; index < levels.Count; index++)
            {
                var expected = context.OrPost(frontIndex, levels[index].LevelNumber, levels[index].ExitElevation);
                var distance = Math.Abs(expected - y);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = index;
                }
            }

            return bestDistance <= LevelMatchTolerance ? best : -1;
        }

        /// <summary>La celda a la que pertenece un larguero del corte, o <see cref="None"/> si no se pudo atribuir.</summary>
        internal readonly struct FrontalCellMatch
        {
            public FrontalCellMatch(int frontIndex, int level)
            {
                Identified = true;
                FrontIndex = frontIndex;
                Level = level;
            }

            public static FrontalCellMatch None => default;

            public bool Identified { get; }

            public int FrontIndex { get; }

            public int Level { get; }
        }

        private static HeaderBlockInstance CloneAt(HeaderBlockInstance source, string pieceId, string block)
        {
            var clone = new HeaderBlockInstance
            {
                Role = HeaderBlockRole.Beam,
                PieceId = pieceId,
                BlockName = block,
                View = View,
                Insertion = source.Insertion,
                ConnectionAnchor = source.ConnectionAnchor,
                RotationRadians = source.RotationRadians,
                MirroredX = source.MirroredX,
                MirroredY = source.MirroredY
            };
            // Copia TODOS los parámetros dinámicos, no solo LONGITUD: un parámetro que el bloque lleve y este clon
            // no copie desaparece del dibujo sin que nada lo delate. Solo la Y cambia después, en el llamador.
            foreach (var parameter in source.DynamicParameters)
            {
                clone.DynamicParameters[parameter.Key] = parameter.Value;
            }

            return clone;
        }

        /// <summary>Explicit tolerance (in) for matching a frontal beam's Y to a level's entrance elevation.</summary>
        private const double LevelMatchTolerance = 0.05;

        /// <summary>
        /// Recover (frontIndex, 0-based level) of a frontal IN/OUT beam. The front comes from its X column
        /// (post+troquel); the level comes from THAT FRONT'S OWN load-beam levels (never the global projection), matched
        /// by entrance elevation within <see cref="LevelMatchTolerance"/>. Returns level = -1 (no silent front-0/level-0
        /// fallback) when nothing matches, so the caller neither mislabels the peralte nor draws a wrong-cell tope.
        ///
        /// Solo la usa el corte POSTERIOR, cuyos largueros conservan la elevación del resolver: por eso puede
        /// reconocerlos por coordenada. El corte BAJO ya no pasa por aquí — su elevación se decide al colocarlo, no
        /// después (PB-004, I-32), y buscar por coordenada una pieza ya movida era precisamente el riesgo.
        /// </summary>
        private static (int FrontIndex, int Level) LocateCell(
            DynamicRackSystem system,
            RackCatalog catalog,
            DynamicFrontLayout layout,
            HeaderBlockInstance beam,
            RackLevelElevations elevations)
        {
            var frontIndex = -1;
            var bestX = double.MaxValue;
            for (var index = 0; index < Math.Min(layout.PostPositions.Count, layout.TroquelPositions.Count) && index < system.Fronts.Count; index++)
            {
                var columnX = layout.PostPositions[index] + layout.TroquelPositions[index];
                var distance = Math.Abs(columnX - beam.Insertion.X);
                if (distance < bestX)
                {
                    bestX = distance;
                    frontIndex = index;
                }
            }

            if (frontIndex < 0)
            {
                return (-1, -1);
            }

            // Use the identified FRONT'S OWN levels — a front may have a different FirstLevelHeight, level count or
            // vertical configuration than the global projection.
            var frontLevels = DynamicFrontGeometry.LoadBeamLevels(system, system.Fronts[frontIndex]);
            var level = -1;
            var bestY = double.MaxValue;
            for (var index = 0; index < frontLevels.Count; index++)
            {
                // Se busca contra la MISMA elevacion con la que la pieza se coloco. Desde la decision final del
                // dueño el larguero alto se DERIVA, asi que comparar contra la del resolver no encontraria ninguna
                // celda: ni tope, ni filtro de celdas, y en silencio.
                var levelY = elevations.OrFront(
                    system.Fronts[frontIndex].Index,
                    frontLevels[index].LevelNumber,
                    frontLevels[index].EntranceElevation);
                var distance = Math.Abs(levelY - beam.Insertion.Y);
                if (distance < bestY)
                {
                    bestY = distance;
                    level = index;
                }
            }

            return level >= 0 && bestY <= LevelMatchTolerance ? (frontIndex, level) : (frontIndex, -1);
        }
    }
}
