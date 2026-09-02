using System;
using System.Collections.Generic;
using RackCad.Application.Geometry;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.Systems.Dynamic;

namespace RackCad.Application.Systems.Dynamic
{
    /// <summary>
    /// I-42 (A1B-D4) — LA IDENTIDAD SEMANTICA de un larguero de extremo: el FRENTE al que pertenece, su NIVEL y el
    /// extremo del rack que lo muestra. Con ella se coloca la pieza y con ella —la misma— se vuelve a encontrar.
    ///
    /// <para>
    /// <see cref="FrontIdentity"/> es <see cref="DynamicRackFront.Index"/>, la identidad del frente, y NO su posicion
    /// en la coleccion: <see cref="FrontPosition"/> es esa posicion, y sirve solo para direccionar la columna y la
    /// celda. Confundirlas es lo que producia el defecto: un indice de coleccion se pasaba como si fuera el indice de
    /// un POSTE, y la elevacion resuelta salia entonces del frente vecino.
    /// </para>
    /// </summary>
    public readonly struct DynamicEndBeamKey
    {
        public DynamicEndBeamKey(
            int frontPosition, int frontIdentity, int levelNumber, int levelIndex,
            DynamicRackEnd end, double columnX, double elevation)
        {
            Matched = true;
            FrontPosition = frontPosition;
            FrontIdentity = frontIdentity;
            LevelNumber = levelNumber;
            LevelIndex = levelIndex;
            End = end;
            ColumnX = columnX;
            Elevation = elevation;
        }

        /// <summary>False cuando la pieza no pudo atribuirse a ninguna celda.</summary>
        public bool Matched { get; }

        /// <summary>Posicion del frente en la coleccion del sistema: donde cae su columna y como se nombra la celda.</summary>
        public int FrontPosition { get; }

        /// <summary>La IDENTIDAD del frente (<see cref="DynamicRackFront.Index"/>), con la que se resuelve su elevacion.</summary>
        public int FrontIdentity { get; }

        /// <summary>Nivel de carga, 1-based.</summary>
        public int LevelNumber { get; }

        /// <summary>El mismo nivel en base 0, que es como se nombran las celdas.</summary>
        public int LevelIndex { get; }

        /// <summary>El extremo del rack que esta pieza materializa.</summary>
        public DynamicRackEnd End { get; }

        /// <summary>X de la columna transversal donde se atornilla.</summary>
        public double ColumnX { get; }

        /// <summary>Y con la que se coloca —y con la que se busca—.</summary>
        public double Elevation { get; }

        /// <summary>La identidad legible: frente, nivel y extremo. Nunca coordenadas.</summary>
        public string Identity
            => Matched
                ? FormattableString.Invariant($"F{FrontIdentity}|N{LevelNumber}|{End}")
                : string.Empty;

        public static DynamicEndBeamKey None => default;
    }

    /// <summary>
    /// I-42 (A1B-D4, contrato del dueño) — LA AUTORIDAD DE IDENTIDAD de los largueros de extremo, compartida por
    /// quien los COLOCA y por quien los BUSCA.
    ///
    /// <para>
    /// <b>La regla.</b> Una pieza colocada con la identidad «frente F, nivel N, extremo E» se localiza despues con
    /// esa MISMA identidad. La columna se compara contra la X con la que se coloco —exacta, no la mas cercana— y la
    /// elevacion contra la que resuelve el contexto PARA ESE FRENTE, que es la que se uso al colocarla.
    /// </para>
    /// <para>
    /// <b>Lo que corrige.</b> El corte bajo buscaba el nivel con <c>OrPost(frontIndex)</c>: preguntaba por la linea
    /// de un POSTE pasandole la posicion de un FRENTE en su coleccion. <c>AtPost</c> devuelve la elevacion del frente
    /// GANADOR entre los adyacentes a ese poste, asi que con primeros niveles distintos por frente contestaba la del
    /// vecino. Medido en un rack de 3 ranuras con primer nivel 4/16/28 (lado A) y 10/22/34 (lado B): la busqueda
    /// devolvia 4.605 donde la pieza estaba en 16.605, y 16.605 donde estaba en 28.605 — 4 de los 6 largueros de cada
    /// lado quedaban SIN IDENTIFICAR y sobrevivian por el fail-open del filtro, apareciendo como largueros IN/OUT
    /// fantasma en cortes que no los llevan.
    /// </para>
    /// </summary>
    public static class DynamicEndBeamIdentity
    {
        /// <summary>Por debajo de esto, dos elevaciones o dos columnas son la MISMA. Es la tolerancia historica.</summary>
        public const double Tolerance = 0.05;

        /// <summary>La X de la columna de un frente: poste mas troquel, la misma expresion con la que se coloca.</summary>
        public static double ColumnX(DynamicFrontLayout layout, int frontPosition)
            => layout == null
               || frontPosition < 0
               || frontPosition >= layout.PostPositions.Count
               || frontPosition >= layout.TroquelPositions.Count
                ? double.NaN
                : layout.PostPositions[frontPosition] + layout.TroquelPositions[frontPosition];

        /// <summary>
        /// La elevacion de una celda en un extremo: la del contexto PARA SU FRENTE, o la que el resolver dio si el
        /// contexto no la describe. Es la unica lectura vertical que hacen el placement y la busqueda.
        /// </summary>
        public static double ElevationOf(
            RackLevelElevations context, DynamicRackFront front, DynamicLoadBeamLevel level, DynamicRackEnd end)
            => context.OrFront(
                front?.Index ?? -1,
                level?.LevelNumber ?? 0,
                end == DynamicRackEnd.Entrance
                    ? level?.EntranceElevation ?? 0.0
                    : level?.ExitElevation ?? 0.0);

        /// <summary>La identidad completa de la celda que un frente y un nivel materializan en ese extremo.</summary>
        public static DynamicEndBeamKey KeyOf(
            DynamicFrontLayout layout,
            RackLevelElevations context,
            DynamicRackFront front,
            int frontPosition,
            DynamicLoadBeamLevel level,
            int levelIndex,
            DynamicRackEnd end)
            => front == null || level == null
                ? DynamicEndBeamKey.None
                : new DynamicEndBeamKey(
                    frontPosition,
                    front.Index,
                    level.LevelNumber,
                    levelIndex,
                    end,
                    ColumnX(layout, frontPosition),
                    ElevationOf(context, front, level, end));

        /// <summary>Todas las identidades que un sistema materializa en un extremo, en orden de frente y nivel.</summary>
        public static IReadOnlyList<DynamicEndBeamKey> KeysOf(
            DynamicRackSystem system, DynamicFrontLayout layout, RackLevelElevations context, DynamicRackEnd end)
        {
            var result = new List<DynamicEndBeamKey>();
            if (system?.Fronts == null || layout == null)
            {
                return result;
            }

            for (var position = 0; position < system.Fronts.Count; position++)
            {
                var front = system.Fronts[position];
                var levels = DynamicFrontGeometry.LoadBeamLevels(system, front);
                for (var index = 0; index < levels.Count; index++)
                {
                    var key = KeyOf(layout, context, front, position, levels[index], index, end);
                    if (key.Matched)
                    {
                        result.Add(key);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// La celda de una pieza ya dibujada. Se compara contra la MISMA identidad con la que se coloco: la columna
        /// EXACTA de un frente y la elevacion resuelta de ESE frente. No hay «la mas cercana»: una pieza que no
        /// coincide con ninguna identidad no se atribuye, y quien pregunta decide que hacer con eso.
        /// </summary>
        public static DynamicEndBeamKey Match(
            DynamicRackSystem system,
            DynamicFrontLayout layout,
            RackLevelElevations context,
            Point2D insertion,
            DynamicRackEnd end)
        {
            foreach (var key in KeysOf(system, layout, context, end))
            {
                if (!double.IsNaN(key.ColumnX)
                    && Math.Abs(key.ColumnX - insertion.X) <= Tolerance
                    && Math.Abs(key.Elevation - insertion.Y) <= Tolerance)
                {
                    return key;
                }
            }

            return DynamicEndBeamKey.None;
        }

        /// <summary>
        /// La COLUMNA de una pieza ya dibujada, sin exigir que su nivel coincida. La consumen las rutas que aun
        /// necesitan la columna para mantener su comportamiento historico cuando el nivel no se identifica.
        /// </summary>
        public static int ColumnOf(DynamicRackSystem system, DynamicFrontLayout layout, double x)
        {
            if (system?.Fronts == null || layout == null)
            {
                return -1;
            }

            var columns = Math.Min(layout.PostPositions.Count, layout.TroquelPositions.Count);
            for (var position = 0; position < columns && position < system.Fronts.Count; position++)
            {
                if (Math.Abs(ColumnX(layout, position) - x) <= Tolerance)
                {
                    return position;
                }
            }

            return -1;
        }
    }
}
