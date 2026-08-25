using System;

namespace RackCad.Application.Systems.Shared
{
    /// <summary>
    /// Desde DONDE se mide «Alto 1er nivel».
    ///
    /// <para>
    /// Es un dato del DOCUMENTO, no del sistema: dice como hay que leer el numero que el documento guarda. Por eso
    /// es aditivo y anulable — ausente significa <see cref="LegacyAbsolute"/>, que es como se guardo siempre— y por
    /// eso ningun archivo existente cambia de geometria al abrirse.
    /// </para>
    /// </summary>
    public enum RackFirstLevelDatumMode
    {
        /// <summary>
        /// La lectura HISTORICA: el numero es una elevacion ABSOLUTA que se ajusta al troquel mas cercano. Tiene dos
        /// problemas —«0» no significa nada fisico y el troquel mas cercano puede caer por debajo del piso—, pero es
        /// lo que todo documento anterior guarda y por tanto lo que hay que respetar al abrirlo.
        /// </summary>
        LegacyAbsolute = 0,

        /// <summary>
        /// La lectura del PRODUCTO (decision del dueño): el numero es un OFFSET sobre el troquel utilizable mas bajo
        /// del poste. «0» significa exactamente «el larguero en el troquel utilizable mas bajo», que es el cero real
        /// del producto, y el resto se mide desde ahi.
        /// </summary>
        LowestUsablePunch = 1
    }

    /// <summary>
    /// LA autoridad —neutral y compartida— de desde donde se mide la altura del primer nivel de carga.
    ///
    /// <para>
    /// Es neutral a proposito: no conoce ningun sistema. Recibe la retícula de troqueles del POSTE que se esta
    /// usando —su base y su paso, que salen del catalogo (<c>TROQUEL_LARGUERO</c>) y no de ninguna constante— y
    /// responde dos preguntas. Asi cada perfil de poste puede tener su propio troquel utilizable mas bajo sin que
    /// nadie codifique un numero.
    /// </para>
    /// <para>
    /// La comparten el sistema DINAMICO y el PUSH BACK porque el dato de usuario es literalmente el mismo —«a que
    /// altura arranca el primer nivel de carga»— y lo resuelve un solo sitio
    /// (<c>DynamicRackSystemResolver</c>). No se toca ningun sistema que use otro mate o hable de otra cosa: lo que
    /// se comparte es el CONCEPTO, no un helper por casualidad.
    /// </para>
    /// </summary>
    public static class RackFirstLevelDatum
    {
        /// <summary>
        /// El TROQUEL UTILIZABLE MAS BAJO del poste: el primer punto de la retícula que no queda por debajo del
        /// piso. Sale de la geometria del poste, no de una constante — cada perfil puede tener el suyo.
        ///
        /// <para>
        /// Sin retícula medida (paso no positivo, base no finita) no hay troquel que ofrecer y el datum es 0: es el
        /// caso de un catalogo incompleto, y ahi la lectura absoluta es la unica que no inventa nada.
        /// </para>
        /// </summary>
        public static double LowestUsablePunch(double gridBase, double pitch)
        {
            if (pitch <= 0.0 || double.IsNaN(gridBase) || double.IsInfinity(gridBase))
            {
                return 0.0;
            }

            // El primer troquel en la base o por encima de ella. Con base negativa sube; con base positiva se queda
            // donde esta, porque bajar seria salirse del poste.
            var steps = Math.Ceiling(-gridBase / pitch);
            var lowest = gridBase + steps * pitch;

            // Defensa contra el redondeo: un troquel que queda una milesima por debajo no es utilizable.
            return lowest < -1e-9 ? lowest + pitch : lowest;
        }

        /// <summary>
        /// La elevacion ABSOLUTA que representa <paramref name="firstLevelHeight"/> con el datum indicado, ANTES de
        /// ajustarla a la retícula. Quien ajusta sigue siendo el mismo de siempre; aqui solo se decide desde donde
        /// se cuenta.
        /// </summary>
        public static double RawElevation(
            double firstLevelHeight, RackFirstLevelDatumMode mode, double gridBase, double pitch)
        {
            var value = Math.Max(0.0, firstLevelHeight);
            return mode == RackFirstLevelDatumMode.LowestUsablePunch
                ? LowestUsablePunch(gridBase, pitch) + value
                : value;
        }

        /// <summary>
        /// La conversion INVERSA: que numero hay que guardar con el datum nuevo para que una elevacion fisica ya
        /// resuelta se conserve EXACTAMENTE. Es lo que permite migrar un documento sin moverlo ni una milesima —no
        /// se resta una constante, se mide la geometria real y se re-expresa.
        /// </summary>
        public static double ToLowestPunchOffset(double absoluteElevation, double gridBase, double pitch)
            => Math.Max(0.0, absoluteElevation - LowestUsablePunch(gridBase, pitch));
    }
}
