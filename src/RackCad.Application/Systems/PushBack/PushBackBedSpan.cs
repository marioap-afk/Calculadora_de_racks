using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;

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

        /// <summary>
        /// La longitud de las <paramref name="positions"/> ULTIMAS posiciones de una secuencia, medida sobre sus
        /// modulos reales. Es lo que mide una cama CORRIDA: su ancla es el extremo ALTO —el ultimo de la secuencia en
        /// su marco— y desde ahi se desarrolla hacia el bajo exactamente lo que su demanda exige, sin obligacion de
        /// llegar al extremo exterior contrario.
        /// </summary>
        public static double SpanOfLastPositions(DynamicRackSystem frame, int positions)
        {
            if (frame == null || positions <= 0)
            {
                return 0.0;
            }

            var modules = frame.Modules.ToList();
            var span = 0.0;
            var covered = 0;
            var index = modules.Count - 1;
            for (; index >= 0 && covered < positions; index--)
            {
                span += modules[index].Length;
                // El HUECO se ATRAVIESA pero no aloja tarima: suma longitud a la cama y no cuenta como fondo. Contar
                // modulos en vez de fondos haria que un rack con hueco pareciera tener una posicion mas.
                if (modules[index].Kind != DynamicRackModuleKind.Gap)
                {
                    covered++;
                }
            }

            if (covered < positions)
            {
                // La demanda rebasa lo que la secuencia ofrece: se extrapola para poder decir CUANTO falta, sin
                // inventarle estructura al rack.
                var palletDepth = Math.Max(0.0, frame.Pallet?.Depth ?? 0.0);
                span += (positions - covered) * palletDepth;
            }

            return span;
        }

        /// <summary>
        /// Cuantos MODULOS ocupa, desde el extremo alto, una cama de <paramref name="positions"/> fondos. Incluye el
        /// hueco que atraviese, que suma longitud pero no aloja tarima.
        /// </summary>
        public static int ModulesForLastPositions(DynamicRackSystem frame, int positions)
        {
            var modules = frame?.Modules.ToList() ?? new List<DynamicRackModule>();
            var covered = 0;
            var used = 0;
            for (var index = modules.Count - 1; index >= 0 && covered < positions; index--)
            {
                used++;
                if (modules[index].Kind != DynamicRackModuleKind.Gap)
                {
                    covered++;
                }
            }

            return used;
        }

        /// <summary>
        /// La PRIMERA posicion (1-based) que ocupa una cama anclada en el extremo ALTO y de
        /// <paramref name="positions"/> fondos de demanda. Acotada a 1: una demanda mayor que la secuencia no
        /// desplaza el arranque fuera del rack — se declara imposible, que es otra cosa.
        /// </summary>
        public static int FirstPositionOfLast(DynamicRackSystem frame, int positions)
        {
            var total = frame?.Modules.Count ?? 0;
            var used = ModulesForLastPositions(frame, positions);
            return Math.Max(1, total - used + 1);
        }

        /// <summary>True cuando la celda cabe fisicamente. Nunca corrige: solo responde.</summary>
        public static bool Fits(double required, double available) => required <= available + Tolerance;

        /// <summary>
        /// El motivo, en el idioma del usuario, por el que una cama no cabe. Null cuando cabe. Declara el LADO, el
        /// frente, el nivel y las dos magnitudes reales, para que el usuario sepa exactamente que cama se queja y
        /// pueda decidir: mas estructura, mas gap o menos fondo.
        /// </summary>
        public static string DisabledReason(
            double required, double available, PushBackSide side, int frontIndex, int levelNumber)
            => Fits(required, available)
                ? null
                : string.Format(
                    System.Globalization.CultureInfo.CurrentCulture,
                    "Lado {0}, frente {1}, nivel {2}: la cama necesita {3:0.##}\" y la estructura efectiva solo "
                    + "ofrece {4:0.##}\". Aumenta la estructura de ese lado, el gap o reduce el fondo de la celda.",
                    side == PushBackSide.B ? "B" : "A",
                    frontIndex + 1,
                    levelNumber,
                    required,
                    available);
    }
}
