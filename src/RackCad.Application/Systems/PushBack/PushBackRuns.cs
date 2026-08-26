using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Systems.Dynamic;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;

namespace RackCad.Application.Systems.PushBack
{
    /// <summary>
    /// I-42 — UNA CAMA FISICA del rack compuesto, con el marco en el que se resuelve.
    ///
    /// <para>
    /// Es la unidad de propiedad fisica del contenido de almacenamiento: una cama, un larguero bajo, un larguero
    /// alto, sus intermedios y, como mucho, un tope. Dos camas encontradas son DOS ejecuciones; una cama corrida es
    /// UNA, aunque atraviese los dos lados. Por eso el BOM no necesita deduplicar nada: cuenta ejecuciones.
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

        /// <summary>La celda compuesta resuelta a la que pertenece.</summary>
        public PushBackResolvedCell Cell { get; set; }

        /// <summary>La cama fisica concreta (su demanda, su capacidad y su motivo de bloqueo si lo tiene).</summary>
        public PushBackCellBed Bed { get; set; }

        /// <summary>El frente dentro del sistema fuente, o null si no existe.</summary>
        public DynamicRackFront Front()
        {
            var fronts = Source?.Structure?.Fronts;
            return fronts != null && SourceFrontIndex >= 0 && SourceFrontIndex < fronts.Count
                ? fronts[SourceFrontIndex]
                : null;
        }

        /// <summary>True cuando la cama es construible (su demanda cabe en la estructura efectiva).</summary>
        public bool IsValid => Bed == null || Bed.IsValid;
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
    /// <item><b>Corrida</b> — UNA cama sobre un sistema sintetico que ATRAVIESA la interfaz, en identidad si va
    /// A-&gt;B y reflejada si va B-&gt;A. Una sola longitud, una sola pendiente, un solo eje y un solo tope.</item>
    /// </list>
    /// </para>
    /// <para>
    /// El sistema sintetico de la corrida es una RECETA geometrica, no un rack: no materializa postes, cabeceras,
    /// placas ni separadores, no aparece en ninguna vista como estructura y no aporta ni una linea al BOM
    /// estructural. Solo sirve para que la cama, sus elevaciones, su rotacion, sus intermedios y su tope los resuelva
    /// el MISMO codigo ya validado de un Push Back de un sentido.
    /// </para>
    /// </summary>
    public sealed class PushBackRunSet
    {
        public PushBackRunSet(double mirrorAxis, IReadOnlyList<PushBackRun> runs)
        {
            MirrorAxis = mirrorAxis;
            Runs = runs ?? new List<PushBackRun>();
        }

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
                return new PushBackRunSet(0.0, runs);
            }

            var axis = PushBackMirror.AxisOf(system.Structure);
            // Un sistema sintetico por (sentido, DEMANDA): las corridas que piden la misma profundidad comparten
            // receta, asi que un rack normal construye una o dos y no una por celda.
            var corridas = new Dictionary<(PushBackRunDirection Direction, int Demand), PushBackSystem>();

            foreach (var cell in composite.Cells)
            {
                foreach (var bed in cell.Beds)
                {
                    if (bed == null)
                    {
                        continue;
                    }

                    if (cell.Topology == PushBackCellTopology.Corrida)
                    {
                        AddCorridaRun(runs, system, cell, bed, corridas);
                        continue;
                    }

                    AddSideRun(runs, system, cell, bed);
                }
            }

            return new PushBackRunSet(axis, runs);
        }

        private static void AddSideRun(
            List<PushBackRun> runs, PushBackSystem system, PushBackResolvedCell cell, PushBackCellBed bed)
        {
            var view = system.Composite.Of(bed.LowSide);
            if (view == null || !view.IsPresent || view.Local == null || view.Front(cell.FrontIndex) == null)
            {
                return;
            }

            runs.Add(new PushBackRun
            {
                Slot = cell.FrontIndex,
                Level = cell.LevelNumber,
                Topology = cell.Topology,
                LowSide = bed.LowSide,
                HighSide = bed.HighSide,
                Source = view.Local,
                SourceFrontIndex = cell.FrontIndex,
                SourceLevel = cell.LevelNumber,
                Reflected = bed.LowSide == PushBackSide.B,
                Cell = cell,
                Bed = bed
            });
        }

        private static void AddCorridaRun(
            List<PushBackRun> runs,
            PushBackSystem system,
            PushBackResolvedCell cell,
            PushBackCellBed bed,
            IDictionary<(PushBackRunDirection Direction, int Demand), PushBackSystem> corridas)
        {
            var forward = bed.HighSide == PushBackSide.B;
            var direction = forward ? PushBackRunDirection.AToB : PushBackRunDirection.BToA;
            var demand = Math.Max(PushBackCellDepth.MinimumPalletsDeep, bed.DemandPositions);
            var key = (direction, demand);
            if (!corridas.TryGetValue(key, out var corrida))
            {
                corrida = BuildCorrida(system, direction, demand);
                corridas[key] = corrida;
            }

            if (corrida == null)
            {
                return;
            }

            runs.Add(new PushBackRun
            {
                Slot = cell.FrontIndex,
                Level = cell.LevelNumber,
                Topology = cell.Topology,
                LowSide = bed.LowSide,
                HighSide = bed.HighSide,
                Source = corrida,
                SourceFrontIndex = cell.FrontIndex,
                SourceLevel = cell.LevelNumber,
                // A->B fluye hacia +X en coordenadas de rack: marco identidad. B->A fluye hacia -X, asi que se
                // resuelve en el marco espejado y el resultado se refleja de vuelta.
                Reflected = !forward,
                Cell = cell,
                Bed = bed
            });
        }

        /// <summary>
        /// La RECETA de las corridas de un sentido y una demanda: cada ranura pasa a ser UNA calle anclada en el
        /// extremo ALTO que ocupa exactamente <paramref name="demand"/> posiciones hacia el bajo.
        ///
        /// <para>
        /// La estructura sobrante NO se toca: puede existir porque otros niveles o frentes la necesitan, y una
        /// corrida corta simplemente no la usa. Nada de lo que hay aqui materializa estructura — postes, cabeceras,
        /// placas y separadores siguen viniendo UNA sola vez del rack.
        /// </para>
        /// <para>
        /// Las ELEVACIONES son las del lado BAJO —el pasillo por el que se carga—, porque desde la decision final
        /// del dueño el ancla vertical de una cama es su extremo bajo. Antes se tomaban del lado ALTO, que era la
        /// regla correcta cuando mandaba el alto: con los dos lados a alturas distintas, la cama corrida quedaba a
        /// la altura del lado contrario al que la carga y su desviador, que sigue los niveles del lado bajo, se
        /// quedaba muy por debajo de ella. Los PERALTES del larguero posterior siguen siendo los del lado ALTO: esa
        /// pieza esta fisicamente alli. La configuracion propia de cada lado no se toca ni se borra.
        /// </para>
        /// </summary>
        public static PushBackSystem BuildCorrida(PushBackSystem system, PushBackRunDirection direction, int demand)
        {
            var composite = system?.Composite;
            var structure = system?.Structure;
            if (composite == null || structure == null)
            {
                return null;
            }

            var forward = direction == PushBackRunDirection.AToB;
            var highSide = forward ? composite.SideB : composite.SideA;
            var lowSide = forward ? composite.SideA : composite.SideB;
            // El marco en el que el flujo avanza hacia +X: el del rack si va A->B, el espejado si va B->A.
            var frame = forward ? Clone(structure) : PushBackMirror.Structure(structure);
            var totalModules = frame.Modules.Count;

            // UNA sola autoridad de colocacion, y anclada en el extremo BAJO. El rango del frente ES el span que
            // resuelve PushBackBedSpan, y nadie lo retoca despues: ni una resta continua, ni un recorte posterior.
            var span = PushBackBedSpan.ResolveSpan(frame, demand);
            var start = 1;
            var modules = Math.Max(1, Math.Min(span.EndPosition, totalModules));

            var corrida = new PushBackSystem
            {
                Structure = frame,
                HighEndBeamCatalogId = system.HighEndBeamCatalogId,
                RearTope = highSide?.RearTope?.DeepCopy() ?? new PushBackRearTopeConfig()
            };

            for (var slot = 0; slot < frame.Fronts.Count; slot++)
            {
                var front = frame.Fronts[slot];
                // La calle corrida ARRANCA siempre en el extremo bajo —el poste exterior del lado por el que se
                // carga, que es su ancla longitudinal— y acaba en el apoyo que el span resolvio. Su longitud fisica
                // es la de ese apoyo, nunca la del rack entero.
                front.DepthStartPosition = start;
                front.PalletsDeep = modules;

                var highFront = highSide?.LocalFront(slot);
                // El marco de la corrida esta REFLEJADO cuando va B->A, asi que el frente del lado bajo hay que
                // pedirlo por su ranura, no por el indice local del marco.
                var lowFront = lowSide?.LocalFront(slot);
                var levels = highFront != null ? DynamicFrontActivation.EffectiveLoadLevels(highFront) : 0;
                if (highFront != null)
                {
                    // Las ELEVACIONES son las del lado BAJO: es el pasillo por el que se carga y, desde la decision
                    // final del dueño, el extremo bajo es el ancla vertical de la cama. Si ese lado no tuviera
                    // frente —no puede, porque es el que carga— se conserva el del alto antes que inventar uno.
                    var elevations = lowFront ?? highFront;
                    front.IsActive = highFront.IsActive;
                    front.LoadLevels = elevations.LoadLevels;
                    front.FirstLevelHeight = elevations.FirstLevelHeight;
                    front.LoadBeamLevels.Clear();
                    foreach (var level in elevations.LoadBeamLevels)
                    {
                        front.LoadBeamLevels.Add(level);
                    }

                    front.Levels.Clear();
                    foreach (var level in elevations.Levels)
                    {
                        front.Levels.Add(level);
                    }
                }
                else
                {
                    front.IsActive = false;
                }

                var resolved = new PushBackResolvedFront
                {
                    IsPresent = highFront != null,
                    DefaultPalletsDeep = demand
                };
                var highResolved = highSide?.Resolved(slot);
                for (var level = 0; level < levels; level++)
                {
                    resolved.HighEndBeamPeraltes.Add(
                        highResolved != null && level < highResolved.HighEndBeamPeraltes.Count
                            ? highResolved.HighEndBeamPeraltes[level]
                            : PushBackDefaults.HighEndBeamDefaultPeralte);
                    // El FONDO de la celda es su DEMANDA en posiciones de tarima, NO los modulos que su rango
                    // atraviesa: un hueco es un modulo que la cama cruza sin almacenar nada. Escribir aqui el conteo
                    // de modulos metia el hueco de vuelta en la demanda por la puerta de atras y repartia una tarima
                    // de mas a lo largo del riel, desplazando TODAS las posiciones — el defecto que el dueño ve como
                    // «la cama esta en el fondo equivocado».
                    resolved.PalletsDeep.Add(demand);
                    resolved.DrawPallets.Add(DrawsPallet(composite, slot, level));
                }

                corrida.HighEndBeams.Add(resolved);
            }

            // Las coordenadas salen del RANGO, como en cualquier frente del rack: el extremo bajo cae en la linea de
            // postes exterior y el alto sobre el apoyo que el span resolvio. No se retoca ninguna X despues.
            DynamicDepthGeometry.ResolveCoordinates(frame);
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
        /// Copia independiente de la estructura para el marco identidad: la corrida re-escribe los rangos de los
        /// frentes, y hacerlo sobre la estructura compartida corromperia la que dibuja el rack.
        /// </summary>
        private static DynamicRackSystem Clone(DynamicRackSystem source)
            => PushBackMirror.Structure(PushBackMirror.Structure(source));
    }
}
