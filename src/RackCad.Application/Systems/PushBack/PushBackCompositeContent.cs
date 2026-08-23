using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Domain.Systems.PushBack;

namespace RackCad.Application.Systems.PushBack
{
    /// <summary>El contenido de almacenamiento de un rack compuesto, ya en coordenadas de rack.</summary>
    public sealed class PushBackCompositeContentResult
    {
        public List<HeaderGroup> Headers { get; } = new List<HeaderGroup>();
        public List<HeaderBlockInstance> Loose { get; } = new List<HeaderBlockInstance>();
    }

    /// <summary>
    /// I-42 — el CONTENIDO de almacenamiento (largueros, camas, topes y tarimas) de un rack compuesto, construido
    /// cama a cama y llevado a coordenadas de rack.
    ///
    /// <para>
    /// La estructura fisica NO pasa por aqui: se dibuja UNA vez desde el sistema compuesto. Aqui solo se materializa
    /// lo que pertenece a una cama, y por eso ninguna pieza estructural puede duplicarse por el hecho de que el rack
    /// tenga dos sentidos.
    /// </para>
    /// <para>
    /// Cada cama se construye con los MISMOS builders de un Push Back de un sentido, en su propio marco, y el
    /// conjunto —riel, rodillos, tope de cama, tarima, largueros— se lleva al mundo con UNA sola reflexion rigida
    /// cuando su marco lo pide. Ni una pieza viaja por su cuenta.
    /// </para>
    /// </summary>
    public static class PushBackCompositeContent
    {
        /// <summary>
        /// El contenido LATERAL de las ranuras que <paramref name="includeSlot"/> acepta. <paramref name="levelCap"/>
        /// acota los niveles como hace el corte por poste.
        /// </summary>
        public static PushBackCompositeContentResult Lateral(
            PushBackSystem system,
            RackCatalog catalog,
            PushBackRunSet runs,
            Func<int, bool> includeSlot,
            int levelCap = int.MaxValue)
        {
            var result = new PushBackCompositeContentResult();
            if (system == null || runs == null)
            {
                return result;
            }

            var bedBuilder = new PushBackFlowBedLateralBuilder();
            var topeBuilder = new PushBackRearTopeBuilder();

            foreach (var batch in Batches(runs, includeSlot, levelCap))
            {
                var source = batch.Source;
                var front = batch.Front;
                var levels = batch.Levels;
                var loose = new List<HeaderBlockInstance>();
                loose.AddRange(PushBackLoadBeamGeometry.LowBeams(source, catalog, front, levels));
                loose.AddRange(PushBackLoadBeamGeometry.HighBeams(source, catalog, batch.FrontIndex, front, levels));
                loose.AddRange(topeBuilder.BuildLateral(source, catalog, batch.FrontIndex, front, levels));
                loose.AddRange(PushBackTarimaPlacement.Lateral(source, catalog, front, int.MaxValue, levels));
                var groups = bedBuilder.BuildLateralGroups(source, catalog, front, int.MaxValue, levels);

                if (batch.Reflected)
                {
                    result.Loose.AddRange(PushBackMirror.Instances(loose, runs.MirrorAxis));
                    result.Headers.AddRange(groups.Select(group =>
                        PushBackMirror.Group(group, runs.MirrorAxis, " B")));
                }
                else
                {
                    result.Loose.AddRange(loose);
                    result.Headers.AddRange(groups);
                }
            }

            return result;
        }

        /// <summary>
        /// Las camas agrupadas por (marco, frente): una llamada por grupo en vez de una por nivel, para no perder el
        /// patron ARRAY ni multiplicar definiciones anidadas.
        /// </summary>
        public static IReadOnlyList<PushBackRunBatch> Batches(
            PushBackRunSet runs, Func<int, bool> includeSlot, int levelCap = int.MaxValue)
        {
            var batches = new List<PushBackRunBatch>();
            foreach (var group in runs.Runs
                         .Where(run => run.Source != null && run.Front() != null)
                         .Where(run => includeSlot == null || includeSlot(run.Slot))
                         .Where(run => run.Level <= levelCap)
                         .GroupBy(run => (run.Source, run.SourceFrontIndex, run.Reflected)))
            {
                var first = group.First();
                batches.Add(new PushBackRunBatch(
                    first.Source,
                    first.SourceFrontIndex,
                    first.Front(),
                    first.Reflected,
                    group.Select(run => run.SourceLevel).Distinct().OrderBy(level => level).ToList(),
                    group.ToList()));
            }

            return batches;
        }
    }

    /// <summary>Las camas de un mismo (marco, frente): comparten builder y, si procede, definicion anidada.</summary>
    public sealed class PushBackRunBatch
    {
        public PushBackRunBatch(
            PushBackSystem source, int frontIndex, Domain.Systems.Dynamic.DynamicRackFront front, bool reflected,
            IReadOnlyList<int> levels, IReadOnlyList<PushBackRun> runs)
        {
            Source = source;
            FrontIndex = frontIndex;
            Front = front;
            Reflected = reflected;
            Levels = levels;
            Runs = runs;
        }

        public PushBackSystem Source { get; }
        public int FrontIndex { get; }
        public Domain.Systems.Dynamic.DynamicRackFront Front { get; }
        public bool Reflected { get; }
        public IReadOnlyList<int> Levels { get; }
        public IReadOnlyList<PushBackRun> Runs { get; }
    }
}
