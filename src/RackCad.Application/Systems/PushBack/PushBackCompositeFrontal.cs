using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;

namespace RackCad.Application.Systems.PushBack
{
    /// <summary>
    /// I-42 — los cortes FRONTALES de un rack compuesto. Un corte frontal es de UN lado: mira a uno de los dos
    /// pasillos. Por eso hay cuatro secciones utiles —entrada/salida y posterior de cada lado— y cada una se
    /// construye con el MISMO builder de un Push Back de un sentido sobre la sub-estructura de ese lado.
    ///
    /// <para>
    /// Lo que la composicion aporta son las dos correcciones que un rack de un sentido no necesita:
    /// <list type="bullet">
    /// <item>en el corte BAJO, la elevacion de una celda CORRIDA la gobierna el lado ALTO, asi que el contexto de
    /// elevaciones se arma leyendo la cama REAL de cada celda y no la del lado que se dibuja;</item>
    /// <item>en el corte POSTERIOR, una celda corrida NO tiene larguero en la linea interior del lado BAJO —la calle
    /// la atraviesa—, asi que esa celda no se materializa alli. Dibujarla seria inventar una pieza inexistente. En el
    /// lado ALTO si aparece, que es donde su larguero esta realmente.</item>
    /// </list>
    /// </para>
    /// <para>
    /// La retícula transversal es la misma para los dos lados, asi que los dos cortes caen en las mismas columnas:
    /// ni un poste ni una placa cambian de sitio por mirar el rack desde el otro pasillo.
    /// </para>
    /// </summary>
    public static class PushBackCompositeFrontal
    {
        public static HeaderRunPlan Build(
            PushBackSystem system, RackCatalog catalog, PushBackFrontalEnd end, PushBackSide side)
        {
            var view = system?.Composite?.Of(side);
            var local = view?.Local;
            if (local == null || !view.IsPresent)
            {
                return new HeaderRunPlan(new List<HeaderGroup>(), new List<HeaderBlockInstance>());
            }

            var runs = PushBackRuns.Resolve(system);
            var allowed = AllowedCells(view, runs, side, end);
            var context = EndContext(local, catalog, view, runs, side, end);

            // Se llama a la SOBRECARGA con inyecciones sobre el sistema LOCAL del lado, que no es compuesto: asi no
            // hay recursion y el corte lo construye el mismo builder de un solo sentido.
            return new PushBackSystemFrontalBuilder().BuildPlan(
                local,
                catalog,
                end,
                context,
                (frontIndex, level) => allowed.Contains((frontIndex, level)),
                HeaderHeightAtLocalPost(system, local, catalog, view, end));
        }

        /// <summary>
        /// Las celdas que este corte materializa, en INDICES LOCALES del lado. El corte bajo admite las camas cuyo
        /// extremo BAJO esta en este lado; el posterior, aquellas cuyo extremo ALTO lo esta.
        /// </summary>
        private static HashSet<(int Front, int Level)> AllowedCells(
            PushBackSideSystem view, PushBackRunSet runs, PushBackSide side, PushBackFrontalEnd end)
        {
            var allowed = new HashSet<(int Front, int Level)>();
            foreach (var run in runs.Runs)
            {
                var owns = end == PushBackFrontalEnd.EntradaSalida ? run.LowSide == side : run.HighSide == side;
                if (!owns)
                {
                    continue;
                }

                var local = LocalIndex(view, run.Slot);
                if (local >= 0)
                {
                    allowed.Add((local, run.Level - 1));
                }
            }

            return allowed;
        }

        /// <summary>
        /// El contexto de elevaciones del corte: para cada celda, la elevacion que tiene en la cama REAL, medida en
        /// el extremo que este corte muestra. Con topologias por lado coincide con la del propio lado; con una
        /// corrida sale de la cama compartida, que es la unica correcta porque la cama es una sola.
        ///
        /// <para>
        /// Los DOS extremos necesitan contexto. El bajo porque su elevacion puede venir del otro lado; el alto
        /// porque desde la decision final del dueño se DERIVA del bajo y ya no coincide con la del resolver.
        /// </para>
        /// </summary>
        private static RackLevelElevations EndContext(
            PushBackSystem local,
            RackCatalog catalog,
            PushBackSideSystem view,
            PushBackRunSet runs,
            PushBackSide side,
            PushBackFrontalEnd end)
        {
            var low = end == PushBackFrontalEnd.EntradaSalida;
            var fronts = local.Structure?.Fronts;
            if (fronts == null || fronts.Count == 0)
            {
                return null;
            }

            var byFront = new List<RackFrontLevelElevations>();
            for (var index = 0; index < fronts.Count; index++)
            {
                var front = fronts[index];
                var elevations = new Dictionary<int, double>();
                foreach (var run in runs.Runs.Where(candidate => (low ? candidate.LowSide : candidate.HighSide) == side))
                {
                    if (LocalIndex(view, run.Slot) != index)
                    {
                        continue;
                    }

                    var source = low
                        ? PushBackElevations.LowInsertions(run.Source, catalog, run.Front())
                        : PushBackElevations.HighInsertions(run.Source, catalog, run.Front());
                    if (source.TryGetValue(run.SourceLevel, out var insertion))
                    {
                        elevations[run.Level] = insertion;
                    }
                }

                byFront.Add(new RackFrontLevelElevations(
                    front.Index,
                    DynamicFrontActivation.EffectiveLoadLevels(front),
                    front.EndX - front.StartX,
                    elevations));
            }

            return RackLevelElevations.From(byFront, systemEnvelope: null);
        }

        /// <summary>
        /// I-42 (ronda 6B) — LA ALTURA DE LA LINEA FISICA, para el corte de un lado.
        ///
        /// <para>
        /// Este corte se construye sobre el sistema LOCAL del lado, que es un modelo de trabajo: sus frentes tienen
        /// sus propias alturas resueltas. Pero el poste que dibuja es la MISMA pieza fisica que el lateral dibuja y
        /// que el BOM compra, y esa pertenece a la estructura COMPUESTA. Con dos autoridades para la misma pieza el
        /// dueño medía una altura en el lateral y otra en la frontal.
        /// </para>
        /// <para>
        /// Aqui no se ajusta nada: se traduce la linea local a su linea COMPUESTA y se pregunta a la MISMA funcion
        /// que ya responde en el lateral y en el BOM. Sin lado presente —o sin traduccion— se devuelve 0 y quien
        /// llama conserva la altura local, que es el comportamiento anterior.
        /// </para>
        /// </summary>
        private static Func<int, double> HeaderHeightAtLocalPost(
            PushBackSystem composite, PushBackSystem local, RackCatalog catalog,
            PushBackSideSystem view, PushBackFrontalEnd end)
        {
            var localLines = local?.Structure?.Fronts?.Count ?? 0;
            var compositeLineByLocalLine = new int[localLines + 1];
            for (var index = 0; index < compositeLineByLocalLine.Length; index++)
            {
                compositeLineByLocalLine[index] = -1;
            }

            // Una ranura presente aporta DOS lineas: la de su izquierda y la de su derecha. En indices compuestos
            // son `slot` y `slot + 1`; en locales, `k` y `k + 1`.
            for (var slot = 0; slot < view.LocalIndexBySlot.Count; slot++)
            {
                var k = view.LocalIndexBySlot[slot];
                if (k < 0 || k + 1 >= compositeLineByLocalLine.Length)
                {
                    continue;
                }

                compositeLineByLocalLine[k] = slot;
                compositeLineByLocalLine[k + 1] = slot + 1;
            }

            // I-42 (ronda 6D) — el corte es de UN LADO, y ese lado ocupa solo su tramo de la profundidad. Se
            // pregunta DENTRO de ese tramo: sobre el rack entero, el extremo posterior de A caeria en la cabecera
            // del otro lado. Y el mapeo extremo->cabecera se invierte en B, porque su marco entra al reves: su
            // pasillo esta contra el extremo lejano del rack.
            var minX = Math.Min(view.OuterX, view.InnerX);
            var maxX = Math.Max(view.OuterX, view.InnerX);
            var lowFirst = view.OuterX <= view.InnerX;
            var compositeEnd = (end == PushBackFrontalEnd.Posterior) == lowFirst
                ? DynamicRackEnd.Entrance
                : DynamicRackEnd.Exit;
            return postIndex =>
            {
                if (postIndex < 0 || postIndex >= compositeLineByLocalLine.Length)
                {
                    return 0.0;
                }

                var line = compositeLineByLocalLine[postIndex];
                return line < 0
                    ? 0.0
                    : DynamicFrontGeometry.HeaderHeightAtPost(
                        composite.Structure, catalog, line, compositeEnd, minX, maxX);
            };
        }

        private static int LocalIndex(PushBackSideSystem view, int slot)
            => slot >= 0 && slot < view.LocalIndexBySlot.Count ? view.LocalIndexBySlot[slot] : -1;
    }
}
