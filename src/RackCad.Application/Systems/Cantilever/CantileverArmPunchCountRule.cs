using System;

namespace RackCad.Application.Systems.Cantilever
{
    /// <summary>
    /// THE rule that says cuántas filas de troqueles lleva por omisión la placa de montaje de un brazo.
    ///
    /// Deja de ser un número fijo —eran dos, siempre, independientemente del perfil— y pasa a derivarse de la
    /// pieza: <b>los que quepan en la altura del perfil</b>, arrancando en el margen vertical aprobado y
    /// avanzando con el paso de la retícula, con un mínimo de dos.
    ///
    /// El paso es el de la retícula REGULAR de la columna y no otro, porque un troquel del brazo que no
    /// coincida con uno de la columna no es un troquel: es un agujero sin tornillo. Por eso la regla lo recibe
    /// en vez de elegirlo — quien la llama trae el paso que la columna resolvió.
    ///
    /// Es un DEFAULT, no una imposición: lo que calcula se escribe en
    /// <c>VerticalPunchCount</c>, que sigue siendo un parámetro que el usuario puede subir o bajar. El
    /// resolutor sigue rechazando por su cuenta un conteo que no quepa, así que esta regla propone y aquélla
    /// dispone.
    /// </summary>
    public static class CantileverArmPunchCountRule
    {
        /// <summary>Dos filas: una sola es una bisagra, no una conexión. Es un mínimo, no un valor al que ajustar.</summary>
        public const int Minimum = 2;

        /// <summary>
        /// Cuántas filas caben en <paramref name="profileHeight"/>.
        ///
        /// Se arranca en <paramref name="verticalEndOffset"/> —el margen— y se avanza de
        /// <paramref name="gridPitch"/> en <paramref name="gridPitch"/> mientras la fila siga cayendo dentro
        /// de la altura. Con un perfil de 10 in, margen 2 in y paso 4 in salen tres: en 2, 6 y 10.
        ///
        /// Una altura, un margen o un paso que no sean positivos y finitos no dan una cuenta menor: dan el
        /// mínimo. Devolver cero o uno sería proponer una conexión que el resolutor va a rechazar de todos
        /// modos, y el usuario vería el rechazo sin haber pedido nada.
        /// </summary>
        public static int For(double profileHeight, double verticalEndOffset, double gridPitch)
        {
            if (!IsUsable(profileHeight) || !IsUsable(gridPitch) ||
                double.IsNaN(verticalEndOffset) || double.IsInfinity(verticalEndOffset) ||
                verticalEndOffset < 0.0)
            {
                return Minimum;
            }

            var usable = profileHeight - verticalEndOffset;

            if (usable < 0.0)
            {
                return Minimum;
            }

            // +1 porque se cuentan FILAS y no huecos: la primera cae en el propio margen.
            var fit = (int)Math.Floor((usable / gridPitch) + Tolerance) + 1;

            return Math.Max(Minimum, fit);
        }

        /// <summary>
        /// Holgura al dividir, en pasos.
        ///
        /// Una fila que cae exactamente en el borde de la altura tiene que contar, y sin esto no cuenta: 8/4
        /// puede dar 1.9999999999999998 en punto flotante y el suelo se lo come. Es una milésima de paso, muy
        /// por debajo de cualquier diferencia que signifique algo.
        /// </summary>
        private const double Tolerance = 1e-9;

        private static bool IsUsable(double value) =>
            !double.IsNaN(value) && !double.IsInfinity(value) && value > 0.0;
    }
}
