using System.Collections.Generic;

namespace RackCad.Domain.Systems.Dynamic
{
    /// <summary>
    /// I-42 (ronda 6D) — UN TRAMO DE PROFUNDIDAD con su propia altura de cabecera por linea transversal.
    ///
    /// <para>
    /// Una cabecera es una pieza de la retícula: existe en una LINEA transversal y en una POSICION longitudinal.
    /// Hasta aqui la altura se resolvia solo por linea, lo que basta en un rack de un solo sentido porque toda su
    /// profundidad sirve a las mismas camas. Un Push Back compuesto no: su estructura es A + hueco + B invertido, de
    /// modo que las cabeceras de la primera mitad son de A y las de la segunda son de B, y sus demandas son
    /// independientes.
    /// </para>
    /// <para>
    /// La zona es DELIBERADAMENTE neutral: un intervalo de X y una altura por linea. El Dinamico no declara ninguna
    /// —la lista vacia significa «la altura sale de la linea, como siempre»— y por eso ni el Dinamico ni un Push
    /// Back de un solo sentido cambian de dibujo.
    /// </para>
    /// </summary>
    public sealed class DynamicHeaderHeightZone
    {
        /// <summary>Arranque del tramo en el eje de PROFUNDIDAD (in), inclusive.</summary>
        public double StartX { get; set; }

        /// <summary>Final del tramo en el eje de PROFUNDIDAD (in), inclusive.</summary>
        public double EndX { get; set; }

        /// <summary>
        /// La altura de cabecera de este tramo, por INDICE DE LINEA transversal. Un valor de 0 o una linea fuera de
        /// la lista significan «esta zona no opina», y quien pregunta conserva la altura de la linea.
        /// </summary>
        public List<double> HeightByLine { get; } = new List<double>();
    }
}
