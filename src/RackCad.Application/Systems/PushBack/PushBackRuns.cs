using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;

namespace RackCad.Application.Systems.PushBack
{
    /// <summary>
    /// I-42 — UNA CAMA FISICA del rack compuesto, con el marco en el que se resuelve.
    ///
    /// <para>
    /// Es la unidad de propiedad fisica del contenido de almacenamiento: una cama, un larguero bajo, un larguero
    /// alto y, como mucho, un tope. Dos camas encontradas son DOS ejecuciones; una cama corrida es UNA, aunque
    /// atraviese los dos lados. Por eso el BOM no necesita deduplicar nada: cuenta ejecuciones.
    /// </para>
    /// </summary>
    public sealed class PushBackRun
    {
        /// <summary>Ranura transversal compartida (0-based).</summary>
        public int Slot { get; set; }

        /// <summary>Nivel de la celda compuesta, 1-based.</summary>
        public int Level { get; set; }

        public PushBackCellTopology Topology { get; set; }

        /// <summary>Lado del extremo BAJO (donde se carga y descarga).</summary>
        public PushBackSide LowSide { get; set; }

        /// <summary>Lado del extremo ALTO (donde va el tope). Coincide con el bajo salvo en una corrida.</summary>
        public PushBackSide HighSide { get; set; }

        /// <summary>El sistema Push Back en cuyo MARCO se resuelve esta cama (el flujo avanza siempre hacia +X en el).</summary>
        public PushBackSystem Source { get; set; }

        /// <summary>Indice del frente dentro de <see cref="Source"/>.</summary>
        public int SourceFrontIndex { get; set; }

        /// <summary>Nivel dentro de <see cref="Source"/>, 1-based.</summary>
        public int SourceLevel { get; set; }

        /// <summary>True cuando el resultado hay que REFLEJARLO para llevarlo a coordenadas de rack.</summary>
        public bool Reflected { get; set; }

        /// <summary>La celda compuesta resuelta a la que pertenece (capacidad geometrica y motivo de bloqueo).</summary>
        public PushBackResolvedCell Cell { get; set; }

        /// <summary>El frente dentro del sistema fuente, o null si no existe.</summary>
        public DynamicRackFront Front()
        {
            var fronts = Source?.Structure?.Fronts;
            return fronts != null && SourceFrontIndex >= 0 && SourceFrontIndex < fronts.Count
                ? fronts[SourceFrontIndex]
                : null;
        }

        /// <summary>True cuando la cama es construible (su demanda cabe en la estructura efectiva).</summary>
        public bool IsValid => Cell == null || Cell.IsValid;
    }

    /// <summary>
    /// I-42 — LA autoridad que enumera las camas fisicas de un Push Back compuesto y decide en que marco se resuelve
    /// cada una. Nadie mas decide «esta celda son una o dos camas»: el dibujo, el BOM y el editor preguntan aqui.
    ///
    /// <para>
    /// Los cuatro modos fisicos y su marco:
    /// <list type="bullet">
    /// <item><b>Solo A</b> — una cama en el marco local de A (identidad).</item>
    /// <item><b>Solo B</b> — una cama en el marco local de B, REFLEJADA al mundo.</item>
    /// <item><b>Encontradas</b> — DOS camas: la de A en identidad y la de B reflejada. Sus extremos ALTOS se miran
    /// en el centro y cada una admite su propio tope.</item>
    /// <item><b>Corrida</b> — UNA cama sobre un sistema sintetico que atraviesa A + hueco + B, en identidad si va
    /// A-&gt;B y reflejada si va B-&gt;A. Una sola longitud, una sola pendiente, un solo eje y un solo tope.</item>
    /// </list>
    /// </para>
    /// <para>
    /// El sistema sintetico de la corrida es un <see cref="PushBackSystem"/> corriente: por eso su cama, sus
    /// elevaciones, su rotacion y su tope los resuelve el MISMO codigo ya validado de un Push Back de un sentido, y
    /// no existe una segunda fisica para el caso nuevo.
    /// </para>
    /// </summary>
    public sealed class PushBackRunSet
    {
        public PushBackRunSet(
            PushBackSystem corridaForward, PushBackSystem corridaBackward, double mirrorAxis, IReadOnlyList<PushBackRun> runs)
        {
            CorridaForward = corridaForward;
            CorridaBackward = corridaBackward;
            MirrorAxis = mirrorAxis;
            Runs = runs ?? new List<PushBackRun>();
        }

        /// <summary>Sistema sintetico de las corridas A-&gt;B (marco identidad), o null si ninguna celda lo pide.</summary>
        public PushBackSystem CorridaForward { get; }

        /// <summary>Sistema sintetico de las corridas B-&gt;A (marco espejado), o null si ninguna celda lo pide.</summary>
        public PushBackSystem CorridaBackward { get; }

        /// <summary>El eje de reflexion: la longitud total del rack.</summary>
        public double MirrorAxis { get; }

        public IReadOnlyList<PushBackRun> Runs { get; }

        /// <summary>Las camas de una ranura, en orden de nivel.</summary>
        public IEnumerable<PushBackRun> OfSlot(int slot) => Runs.Where(run => run.Slot == slot);
    }

    /// <summary>Construye el conjunto de camas fisicas de un Push Back compuesto.</summary>
    public static class PushBackRuns
    {
        public static PushBackRunSet Resolve(PushBackSystem system)
        {
            var runs = new List<PushBackRun>();
            var composite = system?.Composite;
            if (composite == null)
            {
                return new PushBackRunSet(null, null, 0.0, runs);
            }

            var axis = PushBackMirror.AxisOf(system.Structure);
            var needsForward = composite.Cells.Any(cell =>
                cell.Topology == PushBackCellTopology.Corrida && cell.Direction == PushBackRunDirection.AToB);
            var needsBackward = composite.Cells.Any(cell =>
                cell.Topology == PushBackCellTopology.Corrida && cell.Direction == PushBackRunDirection.BToA);

            var forward = needsForward ? BuildCorrida(system, PushBackRunDirection.AToB) : null;
            var backward = needsBackward ? BuildCorrida(system, PushBackRunDirection.BToA) : null;

            foreach (var cell in composite.Cells)
            {
                switch (cell.Topology)
                {
                    case PushBackCellTopology.SoloA:
                        AddSideRun(runs, system, cell, PushBackSide.A);
                        break;
                    case PushBackCellTopology.SoloB:
                        AddSideRun(runs, system, cell, PushBackSide.B);
                        break;
                    case PushBackCellTopology.Encontradas:
                        AddSideRun(runs, system, cell, PushBackSide.A);
                        AddSideRun(runs, system, cell, PushBackSide.B);
                        break;
                    case PushBackCellTopology.Corrida:
                        AddCorridaRun(runs, system, cell, cell.Direction == PushBackRunDirection.AToB ? forward : backward);
                        break;
                }
            }

            return new PushBackRunSet(forward, backward, axis, runs);
        }

        private static void AddSideRun(
            List<PushBackRun> runs, PushBackSystem system, PushBackResolvedCell cell, PushBackSide side)
        {
            var view = system.Composite.Of(side);
            if (view == null || !view.IsPresent || view.Local == null)
            {
                return;
            }

            var localIndex = cell.FrontIndex >= 0 && cell.FrontIndex < view.LocalIndexBySlot.Count
                ? view.LocalIndexBySlot[cell.FrontIndex]
                : -1;
            if (localIndex < 0)
            {
                return;
            }

            runs.Add(new PushBackRun
            {
                Slot = cell.FrontIndex,
                Level = cell.LevelNumber,
                Topology = cell.Topology,
                LowSide = side,
                HighSide = side,
                Source = view.Local,
                SourceFrontIndex = localIndex,
                SourceLevel = cell.LevelNumber,
                Reflected = side == PushBackSide.B,
                Cell = cell
            });
        }

        private static void AddCorridaRun(
            List<PushBackRun> runs, PushBackSystem system, PushBackResolvedCell cell, PushBackSystem corrida)
        {
            if (corrida == null)
            {
                return;
            }

            var forward = cell.Direction == PushBackRunDirection.AToB;
            runs.Add(new PushBackRun
            {
                Slot = cell.FrontIndex,
                Level = cell.LevelNumber,
                Topology = cell.Topology,
                LowSide = forward ? PushBackSide.A : PushBackSide.B,
                HighSide = forward ? PushBackSide.B : PushBackSide.A,
                Source = corrida,
                SourceFrontIndex = cell.FrontIndex,
                SourceLevel = cell.LevelNumber,
                // A->B fluye hacia +X en coordenadas de rack: marco identidad. B->A fluye hacia -X, asi que se
                // resuelve en el marco espejado y el resultado se refleja de vuelta.
                Reflected = !forward,
                Cell = cell
            });
        }

        /// <summary>
        /// El sistema SINTETICO de las corridas en un sentido: cada ranura pasa a ser UNA calle que atraviesa todo el
        /// rack, con las elevaciones y los peraltes del lado ALTO — que es el que gobierna, porque su larguero es el
        /// ancla. La elevacion propia del lado BAJO no se toca ni se borra: sigue almacenada en su lado y vuelve a
        /// gobernar en cuanto la celda deje de ser corrida.
        /// </summary>
        public static PushBackSystem BuildCorrida(PushBackSystem system, PushBackRunDirection direction)
        {
            var composite = system?.Composite;
            var structure = system?.Structure;
            if (composite == null || structure == null)
            {
                return null;
            }

            var forward = direction == PushBackRunDirection.AToB;
            var highSide = forward ? composite.SideB : composite.SideA;
            // El marco en el que el flujo avanza hacia +X: el del rack si va A->B, el espejado si va B->A.
            var frame = forward ? CloneStructure(structure) : PushBackMirror.Structure(structure);
            var totalPositions = frame.Modules.Count;

            var corrida = new PushBackSystem
            {
                Structure = frame,
                HighEndBeamCatalogId = system.HighEndBeamCatalogId,
                RearTope = highSide?.RearTope?.DeepCopy() ?? new PushBackRearTopeConfig()
            };

            for (var slot = 0; slot < frame.Fronts.Count; slot++)
            {
                var front = frame.Fronts[slot];
                // La calle corrida ocupa TODA la secuencia: una sola longitud fisica, sin fondo intermedio.
                front.DepthStartPosition = 1;
                front.PalletsDeep = totalPositions;
                front.StartX = 0.0;
                front.EndX = frame.TotalLength;

                var highFront = highSide?.LocalFront(slot);
                var levels = highFront != null
                    ? RackCad.Application.Systems.Dynamic.DynamicFrontActivation.EffectiveLoadLevels(highFront)
                    : 0;
                if (highFront != null)
                {
                    // Las ELEVACIONES son las del lado ALTO: su larguero posterior es el ancla de la corrida.
                    front.LoadLevels = highFront.LoadLevels;
                    front.FirstLevelHeight = highFront.FirstLevelHeight;
                    front.LoadBeamLevels.Clear();
                    foreach (var level in highFront.LoadBeamLevels)
                    {
                        front.LoadBeamLevels.Add(level);
                    }

                    front.Levels.Clear();
                    foreach (var level in highFront.Levels)
                    {
                        front.Levels.Add(level);
                    }
                }

                var resolved = new PushBackResolvedFront
                {
                    IsPresent = highFront != null,
                    DefaultPalletsDeep = totalPositions
                };
                var highResolved = highSide?.Resolved(slot);
                for (var level = 0; level < levels; level++)
                {
                    resolved.HighEndBeamPeraltes.Add(
                        highResolved != null && level < highResolved.HighEndBeamPeraltes.Count
                            ? highResolved.HighEndBeamPeraltes[level]
                            : PushBackDefaults.HighEndBeamDefaultPeralte);
                    // La cama corrida NO se trunca: su fondo fisico es la secuencia completa.
                    resolved.PalletsDeep.Add(totalPositions);
                    resolved.DrawPallets.Add(DrawsPallet(composite, slot, level));
                }

                corrida.HighEndBeams.Add(resolved);
            }

            return corrida;
        }

        /// <summary>
        /// Una corrida dibuja tarimas si CUALQUIERA de los dos lados lo pide en esa celda: la calle es una sola y su
        /// referencia visual tambien. Las tarimas siguen fuera del BOM (I-41/PB-016).
        /// </summary>
        private static bool DrawsPallet(PushBackCompositeSystem composite, int slot, int level)
        {
            var a = composite.SideA?.Resolved(slot);
            var b = composite.SideB?.Resolved(slot);
            var fromA = a != null && a.IsPresent && level < a.DrawPallets.Count && a.DrawPallets[level];
            var fromB = b != null && b.IsPresent && level < b.DrawPallets.Count && b.DrawPallets[level];
            return fromA || fromB;
        }

        /// <summary>
        /// Copia superficial de la estructura para el marco identidad: la corrida re-escribe los rangos de los
        /// frentes, y hacerlo sobre la estructura compartida corromperia la que dibuja el rack.
        /// </summary>
        private static DynamicRackSystem CloneStructure(DynamicRackSystem source)
            => PushBackMirror.Structure(PushBackMirror.Structure(source));
    }
}
