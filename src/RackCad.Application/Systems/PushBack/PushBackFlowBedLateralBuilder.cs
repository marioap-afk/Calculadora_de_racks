using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Geometry;
using RackCad.Application.Systems.FlowBed;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.FlowBed;
using RackCad.Domain.Systems.PushBack;

namespace RackCad.Application.Systems.PushBack
{
    /// <summary>
    /// Composes the existing roller-bed assembly into a Push Back lane. It REUSES <see cref="FlowBedLateralBuilder"/>
    /// verbatim (the rail/roller recipe is never duplicated) with two Push-Back-specific choices:
    /// <list type="bullet">
    /// <item><see cref="FlowBedType.Pushback"/> — rollers + the bed's own tope, NO brakes (frenos).</item>
    /// <item>the commercial bed length is the front's COMPLETE structural span (no 4" clearance) — unlike the dynamic
    /// bed, which subtracts <see cref="DynamicRackDefaults.FlowBedLengthClearance"/>.</item>
    /// </list>
    /// The dynamic system is NOT touched; the divergent rule (full-span length) lives here, in one place.
    /// </summary>
    public sealed class PushBackFlowBedLateralBuilder
    {
        private readonly FlowBedLateralBuilder flowBedBuilder = new FlowBedLateralBuilder();

        /// <summary>
        /// Push Back commercial bed length = the front's COMPLETE longitudinal span (its resolved cabeceras + separators),
        /// i.e. <c>front.EndX - front.StartX</c> (or the whole system when no front is given). NO 4" clearance is
        /// subtracted — that discount is a dynamic-only rule.
        /// </summary>
        public static double ResolveBedLength(PushBackSystem system, DynamicRackFront front = null)
        {
            if (system == null)
            {
                return 0.0;
            }

            var span = front != null && front.EndX > front.StartX
                ? front.EndX - front.StartX
                : system.TotalLength;

            return span > 0.0 ? span : 0.0;
        }

        /// <summary>
        /// El montaje local de rodillos de una calle Push Back, antes de que el builder lateral lo rote sobre el eje
        /// de la cama. <see cref="FlowBedType.Pushback"/> omite frenos.
        ///
        /// La LONGITUD del riel es SIEMPRE el <b>fondo estructural completo</b> (<see cref="ResolveBedLength"/>).
        /// Hay UNA sola longitud de cama y esta es: la misma que dibuja el riel, la que alimenta el BOM y la que
        /// mide la subida nominal de 7/16" por pie (aclaración final del Owner, I-32).
        /// </summary>
        public IReadOnlyList<HeaderBlockInstance> BuildLocalAssembly(
            PushBackSystem system,
            RackCatalog catalog,
            DynamicRackFront front = null)
            => BuildLocalAssembly(system, catalog, ResolveBedLength(system, front));

        /// <summary>
        /// El mismo montaje para una LONGITUD de cama ya resuelta. I-41 (PB-015): con fondos distintos por nivel la
        /// longitud deja de ser una propiedad del frente, asi que quien la conoce —la celda— la pasa hecha en vez de
        /// obligar a este builder a volver a decidirla.
        /// </summary>
        public IReadOnlyList<HeaderBlockInstance> BuildLocalAssembly(
            PushBackSystem system,
            RackCatalog catalog,
            double laneDepth)
        {
            if (system == null || laneDepth <= 0.0)
            {
                return new List<HeaderBlockInstance>();
            }

            return flowBedBuilder.Build(
                new FlowBedConfiguration
                {
                    BedType = FlowBedType.Pushback,
                    LaneDepth = laneDepth,
                    PalletDepth = system.Structure?.Pallet?.Depth ?? 0.0,
                    RollerId = FlowBedDefaults.RollerId
                },
                catalog);
        }

        /// <summary>
        /// The lateral Push Back bed as grouped <see cref="HeaderGroup"/>s: one shared nested definition (rigid-rotated
        /// onto the Push Back bed axis at the low mate/angle) referenced once per level. Same ARRAY/shared-definition
        /// pattern as the dynamic bed, but the axis is the Push Back one (high mate = rear TROQUEL_REDONDO).
        /// <para>
        /// I-41 (PB-015): dos niveles solo pueden COMPARTIR una definicion anidada si su cama mide lo mismo, asi que
        /// los niveles se agrupan por FONDO EFECTIVO y cada grupo aporta su propia definicion. Un rack sin overrides
        /// tiene un unico fondo y por tanto un unico grupo — exactamente el plan de siempre, sin coste de rendimiento
        /// nuevo; solo un rack escalonado paga una definicion por fondo distinto, que es el minimo posible.
        /// </para>
        /// </summary>
        public IReadOnlyList<HeaderGroup> BuildLateralGroups(
            PushBackSystem system, RackCatalog catalog, DynamicRackFront front = null, int levelCount = int.MaxValue)
        {
            var result = new List<HeaderGroup>();
            var axes = PushBackFlowBedGeometry.Resolve(system, catalog, front)
                .Where(axis => axis.LevelNumber <= levelCount)
                .ToList();
            if (axes.Count == 0)
            {
                return result;
            }

            var suffix = front == null ? string.Empty : " F" + (front.Index + 1);
            var groups = axes
                .GroupBy(axis => Math.Round(PushBackCellDepth.BedLength(system, front, axis.LevelNumber), 6))
                .OrderBy(group => group.Key)
                .ToList();

            foreach (var group in groups)
            {
                // La cama se atornilla por su TROQUEL_IN sobre el TROQUEL_CAMA del larguero de entrada/salida: ese es
                // el mate FÍSICO del extremo bajo, y por eso el pivote es RailLocalMate y no el origen del bloque.
                // Que sobre riel antes de ese punto y que sobresalga por detrás del larguero posterior es lo esperado
                // —su LONGITUD es el fondo estructural completo—, no una penetración que haya que recortar
                // (aclaración final del Owner, I-32).
                var localAssembly = BuildLocalAssembly(system, catalog, group.Key);
                if (localAssembly.Count == 0)
                {
                    continue;
                }

                var groupAxes = group.OrderBy(axis => axis.LevelNumber).ToList();
                var firstAxis = groupAxes[0];
                var definitionInstances = localAssembly
                    .Select(instance => RigidClone(instance, firstAxis.RailLocalMate, firstAxis.ExitMate, firstAxis.RotationRadians, -firstAxis.ExitMate.Y))
                    .ToList();
                var levelPlacements = groupAxes
                    .Select(axis => new HeaderPlacement(0.0, mirrored: false, insertionY: axis.ExitMate.Y))
                    .ToList();

                // El nombre solo lleva el fondo cuando hay MAS DE UNO: con uno solo el grupo conserva su nombre
                // historico y nada aguas abajo (bloques anidados, pruebas doradas) cambia.
                var name = groups.Count > 1
                    ? "Cama push back" + suffix + " D" + PushBackCellDepth.Effective(system, front, firstAxis.LevelNumber)
                    : "Cama push back" + suffix;
                result.Add(new HeaderGroup(name, definitionInstances, levelPlacements));
            }

            return result;
        }

        /// <summary>
        /// Fachada historica: la cama del frente cuando TODOS sus niveles comparten fondo. Devuelve el primer grupo, o
        /// null si no hay ninguno. Se conserva porque es la firma que consumen las pruebas y los llamadores anteriores
        /// a I-41; el lateral usa <see cref="BuildLateralGroups"/>, que es el que no pierde los fondos escalonados.
        /// </summary>
        public HeaderGroup BuildLateral(PushBackSystem system, RackCatalog catalog, DynamicRackFront front = null, int levelCount = int.MaxValue)
            => BuildLateralGroups(system, catalog, front, levelCount).FirstOrDefault();

        private static HeaderBlockInstance RigidClone(
            HeaderBlockInstance source,
            Point2D pivot,
            Point2D target,
            double angle,
            double localYOffset)
        {
            Point2D Transform(Point2D point)
            {
                var x = point.X - pivot.X;
                var y = point.Y - pivot.Y;
                var cos = Math.Cos(angle);
                var sin = Math.Sin(angle);
                return new Point2D(
                    target.X + x * cos - y * sin,
                    target.Y + x * sin + y * cos + localYOffset);
            }

            var clone = new HeaderBlockInstance
            {
                Role = source.Role,
                PieceId = source.PieceId,
                BlockName = source.BlockName,
                View = source.View,
                Insertion = Transform(source.Insertion),
                ConnectionAnchor = Transform(source.ConnectionAnchor),
                RotationRadians = source.RotationRadians + angle,
                MirroredX = source.MirroredX,
                MirroredY = source.MirroredY
            };
            foreach (var pair in source.DynamicParameters)
            {
                clone.DynamicParameters[pair.Key] = pair.Value;
            }

            return clone;
        }
    }
}
