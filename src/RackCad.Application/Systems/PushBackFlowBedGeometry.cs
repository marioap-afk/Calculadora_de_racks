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
        public PushBackFlowBedAxis(
            int levelNumber, Point2D exitMate, Point2D highMate, Point2D railLocalMate, double rotationRadians)
        {
            LevelNumber = levelNumber;
            ExitMate = exitMate;
            HighMate = highMate;
            RailLocalMate = railLocalMate;
            RotationRadians = rotationRadians;
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

        /// <summary>
        /// La ROTACIÓN del bloque completo de la cama, entregada explícitamente por
        /// <see cref="PushBackElevations"/>. Todo el bloque —riel, tope, rodillos— y los intermedios comparten esta
        /// única rotación.
        ///
        /// <b>No se deriva de los dos contactos.</b> <see cref="ExitMate"/> vive en la línea de
        /// <see cref="RailLocalMate"/> y <see cref="HighMate"/> en la del ORIGEN: son rectas PARALELAS distintas, y
        /// tratarlas como una sola dejaba el contacto posterior fuera de su línea por la separación entre ambas
        /// (aclaración final del Owner, I-32).
        /// </summary>
        public double RotationRadians { get; }

        /// <summary>La pendiente resultante: <c>tan(θ)</c>. Es la que se compara con el objetivo de 7/192.</summary>
        public double Slope => Math.Tan(RotationRadians);

        /// <summary>
        /// Distancia, a lo largo de la línea del ORIGEN, desde <see cref="RailOrigin"/> hasta el contacto posterior.
        /// Es cuánto riel hay hasta ese contacto — <b>no</b> la longitud del riel, que es el fondo estructural
        /// completo y por eso sobresale por detrás.
        /// </summary>
        public double RearContactAlongOrigin
            => (HighMate.X - RailOrigin.X) * Math.Cos(RotationRadians)
                + (HighMate.Y - RailOrigin.Y) * Math.Sin(RotationRadians);

        /// <summary>
        /// Dónde acaba el ORIGEN del bloque del riel una vez su <see cref="RailLocalMate"/> queda atornillado sobre
        /// <see cref="ExitMate"/>: se retrocede ese mate a lo largo del eje. Queda ANTES del contacto, y así debe
        /// ser — el riel empieza antes de su primer troquel.
        ///
        /// Es la línea a la que son tangentes los soportes intermedios, que por eso siguen correctos sin tocarlos.
        /// </summary>
        public Point2D RailOrigin => PushBackBedRotation.OriginFor(ExitMate, RailLocalMate, RotationRadians);

        /// <summary>
        /// Altura de la línea del ORIGEN en una X de mundo — la recta a la que son tangentes los intermedios Y el
        /// larguero posterior. Usa la pendiente de la ROTACIÓN, no el desnivel entre contactos.
        /// </summary>
        public double RailOriginYAt(double worldX)
            => RailOrigin.Y + (worldX - RailOrigin.X) * Slope;
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

                result.Add(new PushBackFlowBedAxis(
                    cell.LevelNumber, cell.LowContact, cell.RearContact, railLocalMate, cell.RotationRadians));
            }

            return result;
        }
    }
}
