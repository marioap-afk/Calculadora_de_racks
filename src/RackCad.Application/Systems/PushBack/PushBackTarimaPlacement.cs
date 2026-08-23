using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Geometry;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.FlowBed;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;

namespace RackCad.Application.Systems.PushBack
{
    /// <summary>
    /// I-41 (PB-016) — LA colocacion de la TARIMA de Push Back, en las dos vistas donde el bloque
    /// <c>TARIMA_GENERICA</c> existe (LATERAL y FRONTAL). Reutiliza la regla de instancia que ya posee
    /// <see cref="SelectiveTarimaPlacement"/> —origen abajo-centro, <c>LONGITUD</c> horizontal, <c>ALTURA</c> el alto
    /// de la tarima— en vez de reescribirla, de modo que las tarimas del Selectivo y las de Push Back son la MISMA
    /// pieza dibujada igual.
    ///
    /// <para><b>Donde se apoya la carga (validacion manual del Owner).</b> La tarima NO descansa sobre la linea del
    /// ORIGEN del bloque de la cama —esa es la recta donde se atornilla el riel y a la que son tangentes los soportes
    /// intermedios— sino sobre los RODILLOS. La regla, en palabras del Owner, es:</para>
    /// <code>Y de apoyo = origen del rodillo + radio del rodillo</code>
    /// <para>
    /// que se aplica en el sistema LOCAL del bloque de la cama: los rodillos se insertan sobre el troquel del riel
    /// (<see cref="FlowBedDefaults.RailTopePoint"/>), asi que la superficie de apoyo es ese troquel mas el radio. Ver
    /// <see cref="SupportLocalY"/>.
    /// </para>
    ///
    /// <para>Lo que decide esta clase, y solo esta, es DONDE cae cada tarima de Push Back:</para>
    /// <list type="bullet">
    /// <item><b>LATERAL</b>: una tarima por POSICION de fondo de la celda —tantas como su fondo efectivo—, de ancho
    /// <c>Pallet.Depth</c>, repartidas a lo largo del riel de la celda. Se construyen en el sistema LOCAL de la cama y
    /// se llevan a mundo con la MISMA transformacion rigida que usa el propio bloque de la cama, y ademas llevan su
    /// ROTACION. Por eso quedan tangentes a los rodillos y siguen la pendiente en vez de dibujarse horizontales a
    /// alturas distintas, que es lo que producia el escalonado.</item>
    /// <item><b>FRONTAL</b>: una fila por celda, con una tarima por CALLE, centrada en su calle real. Las calles no
    /// estan repartidas con huecos iguales a lo largo del larguero: cada una mide BFR (frente de tarima + 2") y el
    /// larguero anade una holgura que se reparte a los dos extremos. La Y es la misma superficie de apoyo del lateral,
    /// evaluada en el extremo que ese corte muestra, de modo que las tres vistas no pueden discrepar.</item>
    /// </list>
    /// <para>
    /// Una celda dibuja tarima solo si <see cref="PushBackSystem.DrawPalletAt"/> lo dice. El default legacy es FALSE,
    /// asi que un rack anterior a I-41 no emite ni una sola instancia y su dibujo es el de siempre.
    /// </para>
    /// <para>
    /// Las tarimas son <see cref="HeaderBlockRole.Pallet"/>: una REFERENCIA VISUAL. El BOM de Push Back se construye
    /// desde el sistema resuelto y no desde los planes, asi que no hay ninguna via por la que puedan llegar a el.
    /// </para>
    /// </summary>
    public static class PushBackTarimaPlacement
    {
        /// <summary>La vista de los cortes frontales de Push Back (la misma constante que usan sus builders).</summary>
        public const string FrontalView = "FRONTAL";

        /// <summary>
        /// La Y LOCAL —en el sistema del bloque de la cama— sobre la que descansa la carga: el troquel del riel donde
        /// se insertan los rodillos (<see cref="FlowBedLateralBuilder"/> los coloca todos en esa Y) mas el RADIO del
        /// rodillo. Es la regla que dio el Owner al validar: <c>origen del rodillo + radio</c>.
        ///
        /// <para>
        /// Se lee del MISMO rodillo que arma la cama (<see cref="FlowBedDefaults.RollerId"/>, el que fija
        /// <see cref="PushBackFlowBedLateralBuilder.BuildLocalAssembly(PushBackSystem, RackCatalog, double)"/>), para
        /// que la superficie de apoyo no pueda divergir de los rodillos realmente dibujados.
        /// </para>
        /// </summary>
        public static double SupportLocalY(RackCatalog catalog)
        {
            var railTope = CatalogLookup.Local(
                catalog, FlowBedDefaults.RailId, FlowBedDefaults.RailTopePoint, FlowBedDefaults.View);
            var diameter = catalog?.FlowBedProfiles?.FirstOrDefault(entry => string.Equals(
                entry?.Id, FlowBedDefaults.RollerId, StringComparison.OrdinalIgnoreCase))?.Diameter ?? 0.0;
            return railTope.Y + Math.Max(0.0, diameter) / 2.0;
        }

        /// <summary>
        /// Lleva un punto del sistema LOCAL del bloque de la cama a mundo. Es EXACTAMENTE la colocacion rigida que
        /// aplica <see cref="PushBackFlowBedLateralBuilder"/> al montaje de riel y rodillos —el mate local del riel se
        /// atornilla sobre el contacto bajo y todo el bloque gira con la misma rotacion—, escrita aqui una sola vez
        /// para que la tarima no pueda separarse de los rodillos sobre los que se apoya.
        /// </summary>
        public static Point2D ToWorld(PushBackFlowBedAxis axis, Point2D local)
        {
            var cos = Math.Cos(axis.RotationRadians);
            var sin = Math.Sin(axis.RotationRadians);
            var dx = local.X - axis.RailLocalMate.X;
            var dy = local.Y - axis.RailLocalMate.Y;
            return new Point2D(
                axis.ExitMate.X + dx * cos - dy * sin,
                axis.ExitMate.Y + dx * sin + dy * cos);
        }

        /// <summary>
        /// La Y de la superficie de apoyo (la linea de los rodillos) en una X de mundo. Es paralela a la linea del
        /// ORIGEN que ya expone <see cref="PushBackFlowBedAxis.RailOriginYAt"/>, separada de ella por
        /// <see cref="SupportLocalY"/>. La consumen los cortes frontales, que necesitan la altura de apoyo en un
        /// extremo concreto de la calle.
        /// </summary>
        public static double SupportYAt(PushBackFlowBedAxis axis, double supportLocalY, double worldX)
        {
            var reference = ToWorld(axis, new Point2D(0.0, supportLocalY));
            return reference.Y + (worldX - reference.X) * Math.Tan(axis.RotationRadians);
        }

        /// <summary>Las tarimas de un frente en el corte LATERAL: una por posicion de fondo de cada celda que las pide.</summary>
        public static IReadOnlyList<HeaderBlockInstance> Lateral(
            PushBackSystem system, RackCatalog catalog, DynamicRackFront front, int levelCount = int.MaxValue)
        {
            var result = new List<HeaderBlockInstance>();
            var structure = system?.Structure;
            if (structure == null || front == null)
            {
                // Sin frente no hay celda a la que preguntar: el lateral no seccionado no dibuja tarimas, igual que no
                // dibuja ningun otro dato por celda.
                return result;
            }

            var block = catalog?.Blocks.FindBlock(SelectiveRackDefaults.PalletPieceId, DynamicRackDefaults.IntermediateBeamView)?.BlockName;
            if (string.IsNullOrWhiteSpace(block))
            {
                return result;   // sin bloque medido no se inventa una tarima
            }

            var frontIndex = PushBackCellDepth.FrontIndexOf(system, front);
            if (frontIndex < 0)
            {
                return result;
            }

            var supportLocalY = SupportLocalY(catalog);

            foreach (var axis in PushBackFlowBedGeometry.Resolve(system, catalog, front)
                         .Where(candidate => candidate.LevelNumber <= levelCount))
            {
                var level = axis.LevelNumber - 1;
                if (!system.DrawPalletAt(frontIndex, level))
                {
                    continue;
                }

                var cell = DynamicRackLevelGeometry.At(structure, front, axis.LevelNumber);
                var alto = cell?.Pallet?.Height ?? 0.0;
                var fondo = cell?.Pallet?.Depth ?? 0.0;
                if (alto <= 0.0 || fondo <= 0.0)
                {
                    continue;
                }

                var positions = PushBackCellDepth.Effective(system, front, axis.LevelNumber);
                // El riel de esta celda mide exactamente su longitud de cama; el reparto se hace SOBRE EL RIEL, en su
                // propio sistema local, que es lo que hace que las tarimas caigan sobre los rodillos y no cerca.
                var laneDepth = PushBackCellDepth.BedLength(system, front, axis.LevelNumber);
                if (positions <= 0 || laneDepth <= 0.0)
                {
                    continue;
                }

                var gap = Math.Max(0.0, (laneDepth - positions * fondo) / (positions + 1));
                for (var position = 0; position < positions; position++)
                {
                    var localLeft = gap * (position + 1) + fondo * position;
                    // La instancia se arma con la regla compartida en el sistema LOCAL, y solo despues se lleva a
                    // mundo con la colocacion rigida de la cama. Asi la tarima hereda tangencia y pendiente por
                    // construccion, en vez de recalcularlas.
                    var pallet = SelectiveTarimaPlacement.Pallet(
                        block, DynamicRackDefaults.IntermediateBeamView, localLeft, supportLocalY, fondo, alto);
                    var world = ToWorld(axis, pallet.Insertion);
                    pallet.Insertion = world;
                    pallet.ConnectionAnchor = world;
                    pallet.RotationRadians = axis.RotationRadians;
                    result.Add(pallet);
                }
            }

            return result;
        }

        /// <summary>
        /// La fila de tarimas de UNA celda en un corte FRONTAL: una tarima por CALLE, centrada en su calle real.
        ///
        /// <para>
        /// Las calles NO se reparten con huecos iguales a lo largo del larguero. Cada calle mide
        /// <paramref name="bfr"/> (el frente de la tarima mas la holgura de cama) y el larguero anade una holgura
        /// total —<c>beamLength − calles x BFR</c>— que se reparte por igual a los dos extremos. La tarima va
        /// centrada en su calle, que es donde fisicamente esta.
        /// </para>
        ///
        /// Devuelve una lista vacia si falta cualquier medida — nunca una tarima inventada.
        /// </summary>
        public static IReadOnlyList<HeaderBlockInstance> FrontalRow(
            RackCatalog catalog, int laneCount, double anchorX, double beamLength, double bfr,
            double bottomY, double frente, double alto)
        {
            var result = new List<HeaderBlockInstance>();
            var block = catalog?.Blocks.FindBlock(SelectiveRackDefaults.PalletPieceId, FrontalView)?.BlockName;
            if (string.IsNullOrWhiteSpace(block) || laneCount < 1 || beamLength <= 0.0 || frente <= 0.0 || alto <= 0.0)
            {
                return result;
            }

            var lane = bfr > 0.0 ? bfr : frente;
            // Holgura del larguero repartida a los dos extremos. Nunca negativa: un larguero mas corto que sus calles
            // (por un override manual de longitud) arranca las calles en el extremo en vez de salirse por la izquierda.
            var margin = Math.Max(0.0, (beamLength - laneCount * lane) / 2.0);

            for (var index = 0; index < laneCount; index++)
            {
                var laneCentre = anchorX + margin + (index + 0.5) * lane;
                result.Add(SelectiveTarimaPlacement.Pallet(
                    block, FrontalView, laneCentre - frente / 2.0, bottomY, frente, alto));
            }

            return result;
        }
    }
}
