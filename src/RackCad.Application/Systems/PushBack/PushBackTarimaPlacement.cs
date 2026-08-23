using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.Dynamic;
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
    /// <para>Lo que decide esta clase, y solo esta, es DONDE cae cada tarima de Push Back:</para>
    /// <list type="bullet">
    /// <item><b>LATERAL</b>: una tarima por POSICION de fondo de la celda —tantas como su fondo efectivo—, cada una de
    /// ancho <c>Pallet.Depth</c> repartida a lo largo del tramo de la celda y apoyada sobre la LINEA DEL ORIGEN de su
    /// cama, que es la recta a la que ya son tangentes los soportes intermedios. Por eso las tarimas siguen la
    /// pendiente en vez de flotar sobre una horizontal.</item>
    /// <item><b>FRONTAL</b>: una fila por celda, con tantas tarimas como calles tiene el frente
    /// (<see cref="DynamicRackFront.PalletCount"/>), repartidas sobre la LONGITUD del larguero y apoyadas sobre el
    /// contacto real de la cama en ese extremo — el bajo en el corte de entrada/salida, el posterior en el corte
    /// posterior.</item>
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
                var span = PushBackCellDepth.BedLength(system, front, axis.LevelNumber);
                if (positions <= 0 || span <= 0.0)
                {
                    continue;
                }

                // Reparto uniforme del hueco sobrante, igual que la fila frontal del Selectivo, pero apoyando cada
                // tarima sobre la linea del origen de la cama EN SU PROPIA X: la cama sube, y una sola Y para toda la
                // calle dejaria las del fondo enterradas y las de la entrada flotando.
                var gap = Math.Max(0.0, (span - positions * fondo) / (positions + 1));
                for (var position = 0; position < positions; position++)
                {
                    var footprintLeftX = front.StartX + gap * (position + 1) + fondo * position;
                    result.Add(SelectiveTarimaPlacement.Pallet(
                        block,
                        DynamicRackDefaults.IntermediateBeamView,
                        footprintLeftX,
                        axis.RailOriginYAt(footprintLeftX + fondo / 2.0),
                        fondo,
                        alto));
                }
            }

            return result;
        }

        /// <summary>
        /// La fila de tarimas de UNA celda en un corte FRONTAL: <paramref name="laneCount"/> tarimas repartidas sobre
        /// <paramref name="span"/> desde <paramref name="anchorX"/>, apoyadas en <paramref name="bottomY"/>. Devuelve
        /// una lista vacia si la celda no dibuja tarima o si falta cualquier medida — nunca una tarima inventada.
        /// </summary>
        public static IReadOnlyList<HeaderBlockInstance> FrontalRow(
            RackCatalog catalog, int laneCount, double anchorX, double span, double bottomY, double frente, double alto)
        {
            var result = new List<HeaderBlockInstance>();
            var block = catalog?.Blocks.FindBlock(SelectiveRackDefaults.PalletPieceId, FrontalView)?.BlockName;
            if (string.IsNullOrWhiteSpace(block) || laneCount < 1 || span <= 0.0 || frente <= 0.0 || alto <= 0.0)
            {
                return result;
            }

            SelectiveTarimaPlacement.AppendRow(result, block, FrontalView, anchorX, span, bottomY, frente, alto, laneCount);
            return result;
        }

        /// <summary>La vista de los cortes frontales de Push Back (la misma constante que usan sus builders).</summary>
        public const string FrontalView = "FRONTAL";
    }
}
