using System;
using System.Collections.Generic;
using System.Linq;

namespace RackCad.Application.Systems.Shared
{
    /// <summary>
    /// Las elevaciones de UN frente, más los dos datos que necesita la regla de proyección: cuántos niveles tiene y
    /// qué profundidad ocupa. Nada más — es una entrada de datos, no un modelo.
    /// </summary>
    public readonly struct RackFrontLevelElevations
    {
        private static readonly IReadOnlyDictionary<int, double> Empty = new Dictionary<int, double>();

        public RackFrontLevelElevations(
            int frontIndex, int levelCount, double depth, IReadOnlyDictionary<int, double> elevations)
        {
            FrontIndex = frontIndex;
            LevelCount = levelCount;
            Depth = depth;
            Elevations = elevations ?? Empty;
        }

        /// <summary>Índice del frente, tal como lo numera el sistema resuelto.</summary>
        public int FrontIndex { get; }

        /// <summary>Cuántos niveles de carga tiene el frente. Primer criterio de la regla de proyección.</summary>
        public int LevelCount { get; }

        /// <summary>Profundidad que ocupa el frente. Desempate de la regla de proyección.</summary>
        public double Depth { get; }

        /// <summary>Elevación por NÚMERO de nivel (1..n), no por índice.</summary>
        public IReadOnlyDictionary<int, double> Elevations { get; }
    }

    /// <summary>
    /// Elevaciones de nivel resueltas por frente, para que una vista compartida pueda dibujar sobre elevaciones que
    /// NO son las del resolver sin que el builder tenga que saber de qué sistema vienen.
    ///
    /// Es deliberadamente NEUTRAL: ni nombres ni tipos de ningún sistema concreto. Un builder compartido lo recibe
    /// como último parámetro OPCIONAL y, cuando vale <c>null</c>, no cambia absolutamente nada — cada consulta
    /// devuelve el <c>fallback</c> que el llamador ya usaba. Es la única forma de añadir un override sin arriesgar
    /// el comportamiento histórico de los sistemas que no lo pasan.
    ///
    /// Es INMUTABLE: se construye una vez, con todos los frentes, y a partir de ahí solo responde preguntas. Las
    /// cuatro consultas son explícitas porque cada vista mira a un ámbito distinto y confundirlos es justo lo que hay
    /// que evitar:
    ///
    /// <list type="bullet">
    /// <item><see cref="AtFront"/> — un frente concreto. Lo usa quien dibuja una pieza que pertenece a ese frente.</item>
    /// <item><see cref="AtPost"/> — un poste, resuelto entre sus frentes ADYACENTES. Lo usa quien dibuja en la línea
    /// de un poste, que en un rack jagged puede tener a cada lado frentes distintos.</item>
    /// <item><see cref="AtProjectedSystem"/> — el frente que ORIGINÓ la lista de niveles proyectada del sistema. Lo
    /// usa quien recorre esa lista.</item>
    /// <item><see cref="AtSystemEnvelope"/> — la ENVOLVENTE: el rack entero, sin frente. Lo usa quien dibuja el
    /// conjunto y por tanto ocupa la profundidad completa.</item>
    /// </list>
    ///
    /// <see cref="AtPost"/> y <see cref="AtProjectedSystem"/> eligen frente con la MISMA regla con la que el
    /// resolver proyecta sus niveles —mayor cantidad de niveles y, en empate, mayor profundidad— aplicada al ámbito
    /// de cada una. Si aquí se usara otra regla, el dibujo y el modelo hablarían de niveles distintos.
    ///
    /// <see cref="AtSystemEnvelope"/> NO elige frente, y por eso es un ámbito aparte y no una variante del anterior:
    /// proyección y envolvente pueden ser cosas distintas. Con un frente que gana por NIVELES y otro que es más
    /// PROFUNDO, la lista proyectada viene del primero mientras que el dibujo del conjunto ocupa el fondo del
    /// segundo, y las dos elevaciones caen en troqueles distintos. Por eso el mapa de envolvente llega EXPLÍCITO,
    /// calculado por quien sabe resolver el sistema completo, en vez de deducirse aquí escogiendo un frente.
    /// </summary>
    public sealed class RackLevelElevations
    {
        private static readonly IReadOnlyDictionary<int, double> NoEnvelope = new Dictionary<int, double>();

        private readonly IReadOnlyDictionary<int, IReadOnlyDictionary<int, double>> byFront;
        private readonly IReadOnlyDictionary<int, int> winnerByPost;
        private readonly IReadOnlyDictionary<int, double> envelope;
        private readonly int projectedFront;

        private RackLevelElevations(
            IReadOnlyDictionary<int, IReadOnlyDictionary<int, double>> byFront,
            IReadOnlyDictionary<int, int> winnerByPost,
            IReadOnlyDictionary<int, double> envelope,
            int projectedFront)
        {
            this.byFront = byFront;
            this.winnerByPost = winnerByPost;
            this.envelope = envelope;
            this.projectedFront = projectedFront;
        }

        /// <summary>
        /// El contexto del OTRO extremo, cuando el sistema que lo construyo tiene dos elevaciones por nivel.
        ///
        /// <para>
        /// Casi todas las vistas se dibujan por EXTREMO —el llamador ya sabe cual y pasa el contexto que toca—, pero
        /// el corte LATERAL dibuja los dos a la vez: el desviador de la izquierda cuelga del larguero bajo y el de
        /// la derecha del alto. Sin este acompañante ese segundo desviador tendria que leer la elevacion del
        /// resolver, que desde la inversion vertical de I-42 ya no es la del larguero alto.
        /// </para>
        /// <para>
        /// Null es lo normal: un sistema con una sola elevacion por nivel —el Dinamico— no lo rellena y nada cambia.
        /// </para>
        /// </summary>
        public RackLevelElevations HighEnd { get; private set; }

        /// <summary>
        /// El MISMO contexto con su acompañante del extremo alto. Devuelve una instancia nueva: el contexto es
        /// inmutable para quien lo consume.
        /// </summary>
        public RackLevelElevations WithHighEnd(RackLevelElevations highEnd)
            => new RackLevelElevations(byFront, winnerByPost, envelope, projectedFront) { HighEnd = highEnd };

        /// <summary>
        /// Construye el contexto a partir de las elevaciones de cada frente. Un rack de N frentes tiene N+1 postes:
        /// el poste <c>i</c> ve los frentes <c>i-1</c> e <c>i</c>, recortados a los extremos. Los ganadores por
        /// poste y el de la proyección se resuelven AQUÍ, una sola vez, para que ninguna consulta pueda derivar en
        /// una regla distinta más tarde.
        /// </summary>
        /// <param name="systemEnvelope">
        /// Las elevaciones de la ENVOLVENTE, por número de nivel. Llega explícita porque no se puede deducir de los
        /// frentes: el rack entero ocupa la profundidad completa, que no es la de ninguno de ellos. Puede ser null.
        /// </param>
        public static RackLevelElevations From(
            IEnumerable<RackFrontLevelElevations> fronts,
            IReadOnlyDictionary<int, double> systemEnvelope = null)
        {
            var envelopeMap = systemEnvelope != null && systemEnvelope.Count > 0
                ? (IReadOnlyDictionary<int, double>)new Dictionary<int, double>(
                    systemEnvelope.ToDictionary(entry => entry.Key, entry => entry.Value))
                : NoEnvelope;

            var ordered = (fronts ?? Enumerable.Empty<RackFrontLevelElevations>())
                .Where(front => front.Elevations != null && front.Elevations.Count > 0)
                .GroupBy(front => front.FrontIndex)
                .Select(group => group.First())
                .OrderBy(front => front.FrontIndex)
                .ToList();
            if (ordered.Count == 0 && envelopeMap.Count == 0)
            {
                return null;   // sin datos no hay override: el llamador se queda con su fallback
            }

            if (ordered.Count == 0)
            {
                // Solo envolvente: es un contexto legítimo, con las consultas por frente en fallback.
                return new RackLevelElevations(
                    new Dictionary<int, IReadOnlyDictionary<int, double>>(),
                    new Dictionary<int, int>(),
                    envelopeMap,
                    -1);
            }

            var map = ordered.ToDictionary(
                front => front.FrontIndex,
                front => (IReadOnlyDictionary<int, double>)new Dictionary<int, double>(
                    front.Elevations.ToDictionary(entry => entry.Key, entry => entry.Value)));

            // Un rack de N frentes tiene N+1 postes. Los índices de frente pueden no ser contiguos si algún frente
            // no aportó elevaciones, así que el rango de postes se toma del mayor índice presente.
            var lastFront = ordered[ordered.Count - 1].FrontIndex;
            var posts = new Dictionary<int, int>();
            for (var postIndex = 0; postIndex <= lastFront + 1; postIndex++)
            {
                var adjacent = ordered
                    .Where(front => front.FrontIndex == postIndex - 1 || front.FrontIndex == postIndex)
                    .ToList();
                if (adjacent.Count > 0)
                {
                    posts[postIndex] = Winner(adjacent);
                }
            }

            return new RackLevelElevations(map, posts, envelopeMap, Winner(ordered));
        }

        /// <summary>
        /// La regla del resolver: gana el frente con MÁS niveles y, en empate, el de mayor PROFUNDIDAD. El orden de
        /// entrada es por índice de frente y la ordenación es estable, así que un empate en los dos criterios lo
        /// resuelve el frente de menor índice — igual que el <c>FirstOrDefault</c> del resolver.
        /// </summary>
        private static int Winner(IReadOnlyList<RackFrontLevelElevations> fronts)
            => fronts
                .OrderByDescending(front => front.LevelCount)
                .ThenByDescending(front => front.Depth)
                .First()
                .FrontIndex;

        /// <summary>La elevación de un nivel en un frente concreto, o <paramref name="fallback"/> si no la hay.</summary>
        public double AtFront(int frontIndex, int levelNumber, double fallback)
            => byFront.TryGetValue(frontIndex, out var levels) && levels.TryGetValue(levelNumber, out var elevation)
                ? elevation
                : fallback;

        /// <summary>
        /// La elevación de un nivel en la línea de un poste: la del frente que gana entre los ADYACENTES a ese
        /// poste. Es la consulta de todo lo que se dibuja sobre un poste y no sobre un frente.
        /// </summary>
        public double AtPost(int postIndex, int levelNumber, double fallback)
            => winnerByPost.TryGetValue(postIndex, out var frontIndex)
                ? AtFront(frontIndex, levelNumber, fallback)
                : fallback;

        /// <summary>
        /// La elevación de un nivel en la PROYECCIÓN del sistema: la del frente que gana entre TODOS, es decir el
        /// que originó la lista de niveles proyectada. Es la consulta de quien recorre esa lista.
        ///
        /// No confundir con <see cref="AtSystemEnvelope"/>: esta consulta significa exactamente «el frente
        /// proyectado» y no debe sobrecargarse para significar «el rack entero».
        /// </summary>
        public double AtProjectedSystem(int levelNumber, double fallback)
            => AtFront(projectedFront, levelNumber, fallback);

        /// <summary>
        /// La elevación de un nivel en la ENVOLVENTE del sistema: el rack entero, sin frente. Es la consulta de
        /// quien dibuja el conjunto y por tanto ocupa la profundidad completa — no la de ningún frente.
        ///
        /// Su mapa llega EXPLÍCITO al construir el contexto; aquí no se deduce escogiendo un frente, porque ningún
        /// frente representa la envolvente.
        /// </summary>
        public double AtSystemEnvelope(int levelNumber, double fallback)
            => envelope.TryGetValue(levelNumber, out var elevation) ? elevation : fallback;
    }

    /// <summary>
    /// Azúcar para leer un contexto que puede ser <c>null</c> sin repetir la comprobación en cada punto de uso. Con
    /// <c>null</c> devuelven el fallback, que es exactamente el valor que el llamador usaba antes de existir esto.
    /// </summary>
    public static class RackLevelElevationsExtensions
    {
        public static double OrFront(this RackLevelElevations elevations, int frontIndex, int levelNumber, double fallback)
            => elevations?.AtFront(frontIndex, levelNumber, fallback) ?? fallback;

        public static double OrPost(this RackLevelElevations elevations, int postIndex, int levelNumber, double fallback)
            => elevations?.AtPost(postIndex, levelNumber, fallback) ?? fallback;

        public static double OrProjectedSystem(this RackLevelElevations elevations, int levelNumber, double fallback)
            => elevations?.AtProjectedSystem(levelNumber, fallback) ?? fallback;

        public static double OrSystemEnvelope(this RackLevelElevations elevations, int levelNumber, double fallback)
            => elevations?.AtSystemEnvelope(levelNumber, fallback) ?? fallback;
    }
}
