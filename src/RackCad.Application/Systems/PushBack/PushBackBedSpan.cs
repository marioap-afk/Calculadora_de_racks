using System;
using System.Linq;
using RackCad.Domain.Systems.Dynamic;

namespace RackCad.Application.Systems.PushBack
{
    /// <summary>
    /// I-42 — LA autoridad de la capacidad geometrica de una cama. Separa explicitamente dos magnitudes que hasta
    /// ahora se confundian en una sola (el fondo del frente):
    ///
    /// <list type="bullet">
    /// <item><b>RequiredBedLength</b> — la longitud MINIMA que exige la demanda de fondos de la celda. Se mide sobre
    /// la secuencia REAL de modulos, no como una suma rigida de fondos: el primer modulo lleva su holgura de extremo
    /// y los demas la profundidad de tarima, y eso ya no se vuelve a derivar en ningun otro sitio.</item>
    /// <item><b>AvailableBedSpan</b> — la longitud FISICAMENTE disponible entre los apoyos de la estructura efectiva.
    /// Un gap positivo la aumenta de verdad; un override manual de estructura tambien.</item>
    /// </list>
    ///
    /// <para>
    /// La unica regla de validez es <c>RequiredBedLength &lt;= AvailableBedSpan</c>. No se trunca la cama, no se
    /// aumentan los fondos y no se inventa estructura: una celda que no cabe se declara imposible y se dice por que.
    /// Y no se codifica ninguna suma de fondos concreta — el caso «8 + 5» no es «13»: es una longitud contra otra, y
    /// por eso un gap puede volver valida una cama que sin el no cabria.
    /// </para>
    /// </summary>
    public static class PushBackBedSpan
    {
        /// <summary>Tolerancia de comparacion (in). Por debajo de una milesima de pulgada no hay diferencia fisica.</summary>
        public const double Tolerance = 1e-3;

        /// <summary>
        /// La longitud de las <paramref name="positions"/> PRIMERAS posiciones de una sub-estructura, medida sobre
        /// sus modulos reales. Una demanda que rebasa la secuencia se extrapola con la profundidad de tarima: es la
        /// unica forma de medir cuanto le FALTA a una celda que no cabe sin inventarle estructura.
        /// </summary>
        public static double SpanOfFirstPositions(DynamicRackSystem local, int positions)
        {
            if (local == null || positions <= 0)
            {
                return 0.0;
            }

            var modules = local.Modules.ToList();
            var span = 0.0;
            for (var index = 0; index < Math.Min(positions, modules.Count); index++)
            {
                span += modules[index].Length;
            }

            if (positions > modules.Count)
            {
                var palletDepth = Math.Max(0.0, local.Pallet?.Depth ?? 0.0);
                span += (positions - modules.Count) * palletDepth;
            }

            return span;
        }

        /// <summary>
        /// La longitud EXIGIDA por la demanda de una celda de un lado: sus fondos efectivos medidos desde el extremo
        /// exterior de ese lado.
        /// </summary>
        public static double Required(DynamicRackSystem local, int effectiveDeep)
            => SpanOfFirstPositions(local, effectiveDeep);

        /// <summary>
        /// La longitud DISPONIBLE de una ranura en un lado: lo que da su estructura efectiva desde el extremo
        /// exterior. Es exactamente el tramo del frente proyectado, asi que la cama nunca se compara contra una
        /// longitud que la estructura no tenga.
        /// </summary>
        public static double Available(DynamicRackSystem local, int slotStructure)
            => SpanOfFirstPositions(local, slotStructure);

        /// <summary>True cuando la celda cabe fisicamente. Nunca corrige: solo responde.</summary>
        public static bool Fits(double required, double available) => required <= available + Tolerance;

        /// <summary>
        /// El motivo, en el idioma del usuario, por el que una celda no cabe. Null cuando cabe. Se declara la
        /// magnitud que falta para que el usuario pueda decidir: mas estructura, mas gap o menos fondo.
        /// </summary>
        public static string DisabledReason(double required, double available)
            => Fits(required, available)
                ? null
                : string.Format(
                    System.Globalization.CultureInfo.CurrentCulture,
                    "La cama necesita {0:0.##}\" y la estructura efectiva solo ofrece {1:0.##}\". "
                    + "Aumenta la estructura del lado, el gap o reduce el fondo de la celda.",
                    required,
                    available);
    }
}
