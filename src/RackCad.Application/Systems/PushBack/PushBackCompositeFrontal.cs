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
        /// <summary>
        /// I-42 (ronda 8B) — UN CORTE ES UN PLANO FISICO, y lo que muestra es EL APOYO de cada cama que coincide con
        /// el: su extremo BAJO, un apoyo INTERMEDIO, su extremo ALTO, o nada.
        ///
        /// <para>
        /// La vista ya no infiere el papel de su nombre. «Frontal» y «Posterior» dicen DONDE esta el plano; el papel
        /// lo decide <see cref="PushBackRunSupports"/> por cama. De ahi se sigue todo lo que el dueño pidio: la cara
        /// exterior del lado alto de una corrida muestra su ALTO —y su tope—, las dos lineas interiores muestran los
        /// INTERMEDIOS de la corrida que las atraviesa, y una cama que termina antes de una linea no aparece en ella.
        /// </para>
        /// <para>
        /// El corte se arma en TRES pasadas sobre el mismo builder de un solo sentido, una por papel, con las celdas
        /// que a cada uno le corresponden. El marco —postes, placas, cotas— lo aporta la primera; de las otras se
        /// toman solo sus piezas, para no dibujarlo tres veces.
        /// </para>
        /// </summary>
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
            var byRole = CellsByRole(system, runs, view, side, end);
            var builder = new PushBackSystemFrontalBuilder();
            var headerHeight = HeaderHeightAtLocalPost(system, local, catalog, view, end);

            // El BAJO lleva el marco del corte; los otros dos papeles aportan solo sus piezas.
            var low = builder.BuildPlan(
                local, catalog, PushBackFrontalEnd.EntradaSalida,
                EndContext(local, catalog, view, runs, side, PushBackFrontalEnd.EntradaSalida,
                    byRole[PushBackSupportRole.Low]),
                Only(byRole[PushBackSupportRole.Low]), headerHeight);

            var high = builder.BuildPlan(
                local, catalog, PushBackFrontalEnd.Posterior,
                EndContext(local, catalog, view, runs, side, PushBackFrontalEnd.Posterior,
                    byRole[PushBackSupportRole.High]),
                Only(byRole[PushBackSupportRole.High]), headerHeight);

            var middle = builder.BuildIntermediatePlan(
                local, catalog,
                IntermediateContext(system, local, catalog, view, runs, side, end,
                    byRole[PushBackSupportRole.Intermediate]),
                Only(byRole[PushBackSupportRole.Intermediate]), headerHeight);

            // La SEGURIDAD pertenece al pasillo: solo la lleva la cara exterior de un lado, que es donde hay
            // pasillo que proteger. La linea interior no lleva ninguna, como siempre.
            var frameKeeps = end == PushBackFrontalEnd.EntradaSalida
                ? (Func<HeaderBlockInstance, bool>)(_ => true)
                : instance => !PushBackPlanComposer.IsSafetyPiece(instance);

            // I-42 (A1B-D3): el marco local trae las tarimas que emitio su propia pasada. Se retiran enteras: la
            // pertenencia de una tarima a este plano la decide la cama fisica, no el lado que la construye.
            var frame = PushBackPalletProjection.Without(low);
            var groups = frame.Headers
                .Select(group => Filter(group, frameKeeps))
                .Where(group => group != null)
                .ToList();
            var loose = frame.LooseInstances.Where(frameKeeps).ToList();
            foreach (var plan in new[] { high, middle })
            {
                groups.AddRange(plan.Headers.Where(HoldsPieces));
                loose.AddRange(plan.LooseInstances.Where(IsPiece));
            }

            // Y las tarimas que este plano SI materializa: una fila por cama con apoyo aqui, con la altura de apoyo
            // de esa cama medida en este plano.
            loose.AddRange(PushBackPalletProjection.Instances(
                PushBackPalletProjection.Resolve(system, catalog, side, end), catalog));

            return new HeaderRunPlan(groups, loose);
        }

        /// <summary>El mismo grupo sin las instancias que <paramref name="keeps"/> rechaza, o null si queda vacio.</summary>
        private static HeaderGroup Filter(HeaderGroup group, Func<HeaderBlockInstance, bool> keeps)
        {
            if (group?.Instances == null)
            {
                return group;
            }

            var kept = group.Instances.Where(keeps).ToList();
            return kept.Count == group.Instances.Count
                ? group
                : (kept.Count == 0 ? null : new HeaderGroup(group.Name, kept, group.Placements));
        }

        /// <summary>Un grupo aporta piezas del corte (no marco) cuando alguna de sus instancias lo es.</summary>
        private static bool HoldsPieces(HeaderGroup group)
            => group?.Instances != null && group.Instances.Any(IsPiece);

        /// <summary>
        /// Las piezas que un papel aporta: sus largueros y sus topes. El marco lo pone la primera pasada.
        ///
        /// <para>
        /// I-42 (A1B-D3): las TARIMAS ya no viajan aqui. Cada pasada las emitia recorriendo otra vez las celdas del
        /// sistema local, de modo que un corte acumulaba la fila del extremo bajo y la del posterior de todas las
        /// celdas del lado, existiera o no una cama con apoyo en ese plano. Ahora las resuelve
        /// <see cref="PushBackPalletProjection"/> una sola vez, desde la cama fisica.
        /// </para>
        /// </summary>
        private static bool IsPiece(HeaderBlockInstance instance)
            => instance != null
               && (instance.Role == HeaderBlockRole.Beam
                   || instance.Role == HeaderBlockRole.Tope);

        private static Func<int, int, bool> Only(HashSet<(int Front, int Level)> cells)
            => (frontIndex, level) => cells.Contains((frontIndex, level));

        /// <summary>
        /// Las celdas de este corte AGRUPADAS POR PAPEL, en indices LOCALES del lado. Es la unica traduccion entre
        /// la autoridad de corte y el builder.
        /// </summary>
        private static Dictionary<PushBackSupportRole, HashSet<(int Front, int Level)>> CellsByRole(
            PushBackSystem system,
            PushBackRunSet runs,
            PushBackSideSystem view,
            PushBackSide side,
            PushBackFrontalEnd end)
        {
            var byRole = new Dictionary<PushBackSupportRole, HashSet<(int Front, int Level)>>
            {
                [PushBackSupportRole.Low] = new HashSet<(int, int)>(),
                [PushBackSupportRole.Intermediate] = new HashSet<(int, int)>(),
                [PushBackSupportRole.High] = new HashSet<(int, int)>(),
            };

            foreach (var run in runs.Runs)
            {
                var role = PushBackRunSupports.At(system, runs, run, side, end);
                if (role == PushBackSupportRole.None)
                {
                    continue;
                }

                var local = LocalIndex(view, run.Slot);
                if (local >= 0)
                {
                    byRole[role].Add((local, run.Level - 1));
                }
            }

            return byRole;
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
            PushBackFrontalEnd end,
            HashSet<(int Front, int Level)> cells = null)
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

                    // I-42 (ronda 8B): la elevacion se arma SOLO para las celdas que este papel materializa. Sin
                    // acotarlo, el corte bajo de un lado seguiria proponiendo la elevacion de una cama que en este
                    // plano no tiene su bajo.
                    if (cells != null && !cells.Contains((index, run.Level - 1)))
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
        /// I-42 (ronda 8B) — LA ELEVACION DE UN APOYO INTERMEDIO.
        ///
        /// <para>
        /// La cama es una rampa RECTA entre sus dos extremos, y las elevaciones de esos extremos ya las dan las dos
        /// autoridades que los cortes usan (<see cref="PushBackElevations.LowInsertions"/> y
        /// <see cref="PushBackElevations.HighInsertions"/>). La de un punto intermedio es, por tanto, la
        /// interpolacion entre ellas en la X de este plano — no una tercera regla de elevacion, sino la recta que
        /// las dos definen.
        /// </para>
        /// </summary>
        private static RackLevelElevations IntermediateContext(
            PushBackSystem system,
            PushBackSystem local,
            RackCatalog catalog,
            PushBackSideSystem view,
            PushBackRunSet runs,
            PushBackSide side,
            PushBackFrontalEnd end,
            HashSet<(int Front, int Level)> cells)
        {
            var fronts = local.Structure?.Fronts;
            if (fronts == null || fronts.Count == 0 || cells.Count == 0)
            {
                return null;
            }

            var cutX = PushBackRunSupports.CutX(system, side, end);
            var byFront = new List<RackFrontLevelElevations>();
            for (var index = 0; index < fronts.Count; index++)
            {
                var front = fronts[index];
                var elevations = new Dictionary<int, double>();
                foreach (var run in runs.Runs)
                {
                    if (LocalIndex(view, run.Slot) != index || !cells.Contains((index, run.Level - 1)))
                    {
                        continue;
                    }

                    var elevation = InterpolatedElevation(runs, run, catalog, cutX);
                    if (elevation.HasValue)
                    {
                        elevations[run.Level] = elevation.Value;
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

        /// <summary>La elevacion de la cama en la X del corte, sobre la recta que unen sus dos extremos.</summary>
        private static double? InterpolatedElevation(
            PushBackRunSet runs, PushBackRun run, RackCatalog catalog, double? cutX)
        {
            var boundaries = PushBackRunSupports.BoundariesOf(runs, run);
            if (!cutX.HasValue || boundaries == null)
            {
                return null;
            }

            var lowSource = PushBackElevations.LowInsertions(run.Source, catalog, run.Front());
            var highSource = PushBackElevations.HighInsertions(run.Source, catalog, run.Front());
            if (!lowSource.TryGetValue(run.SourceLevel, out var lowY)
                || !highSource.TryGetValue(run.SourceLevel, out var highY))
            {
                return null;   // sin los dos extremos medidos no hay recta: no se materializa nada
            }

            var (lowX, highX) = boundaries.Value;
            var span = highX - lowX;
            if (Math.Abs(span) <= PushBackRunSupports.Tolerance)
            {
                return lowY;
            }

            return lowY + (highY - lowY) * ((cutX.Value - lowX) / span);
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
