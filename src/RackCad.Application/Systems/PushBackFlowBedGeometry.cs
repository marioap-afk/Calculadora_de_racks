using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Geometry;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// The physical Push Back bed geometry for one level: the line between the TWO REAL contact points of the two end
    /// beams, both of them bolted to a valid troquel.
    ///
    /// PB-004 (I-32, Owner rule after round 1): the 7/16"-per-foot rise is a NOMINAL TARGET, not the literal final
    /// rise. The REAR beam is the anchor — it keeps the troquel elevation the resolver already gave it — and the
    /// ENTRANCE/EXIT beam is derived from it through the nominal slope and then snapped to its own nearest troquel.
    /// The resulting slope is whatever those two real contacts produce, within half a troquel step of the target.
    ///
    /// The previous version did the opposite (anchored the LOW end and pulled the rear beam OFF the grid so its edge
    /// touched a theoretical line); the Owner rejected it because a larguero must always be bolted to a troquel.
    /// </summary>
    public readonly struct PushBackFlowBedAxis
    {
        public PushBackFlowBedAxis(int levelNumber, Point2D exitMate, Point2D highMate, Point2D railLocalMate)
        {
            LevelNumber = levelNumber;
            ExitMate = exitMate;
            HighMate = highMate;
            RailLocalMate = railLocalMate;
        }

        public int LevelNumber { get; }

        /// <summary>Low-end mate: the IN/OUT beam's TROQUEL_CAMA (same physical point the dynamic bed uses).</summary>
        public Point2D ExitMate { get; }

        /// <summary>
        /// Contacto físico del extremo ALTO: la arista del larguero posterior que la geometría elige
        /// (<see cref="PushBackLoadBeamGeometry.RearBeamTangencyPointWorld"/>), sobre su elevación de troquel. Es el
        /// ANCLA — no se deriva de nada (PB-004, I-32).
        /// </summary>
        public Point2D HighMate { get; }

        /// <summary>
        /// El <c>TROQUEL_IN</c> local del riel: <b>LA autoridad de colocación</b> de la cama en el extremo de
        /// entrada/salida.
        ///
        /// El mate obligatorio de ese extremo es <c>LARGUERO_IN_OUT.TROQUEL_CAMA</c> con
        /// <c>RIEL_DE_CINTA_CALIBRE_12.TROQUEL_IN</c>: la cama se transforma hasta que ESTE punto cae sobre
        /// <see cref="ExitMate"/>. Es un mate físico —dos troqueles atornillados—, no una convención de dibujo.
        ///
        /// Que quede geometría del riel ANTES de este punto es lo ESPERADO, no un defecto: el riel empieza antes
        /// de su primer troquel de sujeción. Igual que es esperado que sobresalga del larguero posterior, porque su
        /// LONGITUD es el fondo estructural completo. Ninguna de las dos cosas se recorta
        /// (aclaración final del Owner, I-32).
        /// </summary>
        public Point2D RailLocalMate { get; }

        public double Rise => HighMate.Y - ExitMate.Y;
        public double Run => HighMate.X - ExitMate.X;
        public double Length => Math.Sqrt(Run * Run + Rise * Rise);
        public double AngleRadians => Math.Atan2(Rise, Run);

        /// <summary>
        /// Dónde acaba el ORIGEN del bloque del riel una vez su <see cref="RailLocalMate"/> queda atornillado sobre
        /// <see cref="ExitMate"/>: se retrocede ese mate a lo largo del eje. Queda ANTES del contacto, y así debe
        /// ser — el riel empieza antes de su primer troquel.
        ///
        /// Es la línea a la que son tangentes los soportes intermedios, que por eso siguen correctos sin tocarlos.
        /// </summary>
        public Point2D RailOrigin
        {
            get
            {
                var cos = Math.Cos(AngleRadians);
                var sin = Math.Sin(AngleRadians);
                return new Point2D(
                    ExitMate.X - RailLocalMate.X * cos + RailLocalMate.Y * sin,
                    ExitMate.Y - RailLocalMate.X * sin - RailLocalMate.Y * cos);
            }
        }

        /// <summary>Height of the rail ORIGIN line at a world X — the line every intermediate support is tangent to.</summary>
        public double RailOriginYAt(double worldX)
            => Math.Abs(Run) < 1e-9
                ? RailOrigin.Y
                : RailOrigin.Y + (worldX - RailOrigin.X) * Rise / Run;
    }

    /// <summary>
    /// Single source of truth for the Push Back bed line. The high end is the REAR beam's real contact (its resolved
    /// troquel elevation plus its <c>INICIO_DERECHO</c> datum); the low end is the ENTRANCE/EXIT beam's real contact,
    /// whose elevation <see cref="PushBackLoadBeamGeometry.LowBeamElevations"/> derives from the high one and snaps to
    /// the troquel grid. Both ends are therefore physical. Does not touch the dynamic bed geometry.
    /// </summary>
    public static class PushBackFlowBedGeometry
    {
        /// <summary>Push Back commercial bed length = the front's COMPLETE span, no 4" clearance (see <see cref="PushBackFlowBedLateralBuilder.ResolveBedLength"/>).</summary>
        public static double ResolveBedLength(PushBackSystem system, DynamicRackFront front = null)
            => PushBackFlowBedLateralBuilder.ResolveBedLength(system, front);

        public static IReadOnlyList<PushBackFlowBedAxis> Resolve(PushBackSystem system, RackCatalog catalog, DynamicRackFront front = null)
        {
            var result = new List<PushBackFlowBedAxis>();
            var structure = system?.Structure;
            if (structure == null || structure.TotalLength <= 0.0)
            {
                return result;
            }

            var railLocalMate = CatalogLookup.Local(catalog, FlowBedDefaults.RailId, FlowBedDefaults.RailInOutMatePoint, FlowBedDefaults.View);

            // UNA autoridad resuelve las dos elevaciones y los dos contactos; aquí no se recalcula nada.
            foreach (var cell in PushBackElevations.Resolve(system, catalog, front).Values.OrderBy(c => c.LevelNumber))
            {
                if (cell.RearContact.X - cell.LowContact.X <= 0.0)
                {
                    continue;
                }

                result.Add(new PushBackFlowBedAxis(cell.LevelNumber, cell.LowContact, cell.RearContact, railLocalMate));
            }

            return result;
        }
    }
}
