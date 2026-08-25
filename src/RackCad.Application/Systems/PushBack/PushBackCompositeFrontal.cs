using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.Shared;
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
                (frontIndex, level) => allowed.Contains((frontIndex, level)));
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

        private static int LocalIndex(PushBackSideSystem view, int slot)
            => slot >= 0 && slot < view.LocalIndexBySlot.Count ? view.LocalIndexBySlot[slot] : -1;
    }
}
