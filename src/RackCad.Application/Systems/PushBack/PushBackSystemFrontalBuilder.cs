using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Geometry;
using RackCad.Application.Systems.Dynamic;
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
                return HeaderInstanceGrouper.Group(
                    dynamicBuilder.Build(
                        structure, catalog, DynamicRackEnd.Exit, PushBackElevations.Context(system, catalog)),
                    "PB_FRONTAL_ENTRADA_SALIDA");
            }

            // El corte POSTERIOR no lleva override: su larguero es el ancla y conserva la elevación del resolver.
            var entrance = dynamicBuilder.Build(structure, catalog, DynamicRackEnd.Entrance);
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
                    var (frontIndex, level) = LocateCell(structure, catalog, layout, instance);

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
                    if (!string.IsNullOrWhiteSpace(topeBlock) && level >= 0 && rearTope.At(frontIndex, level)
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

            return HeaderInstanceGrouper.Group(result, "PB_FRONTAL_POSTERIOR");
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
            DynamicRackSystem system, RackCatalog catalog, DynamicFrontLayout layout, HeaderBlockInstance beam)
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
                var distance = Math.Abs(frontLevels[index].EntranceElevation - beam.Insertion.Y);
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
