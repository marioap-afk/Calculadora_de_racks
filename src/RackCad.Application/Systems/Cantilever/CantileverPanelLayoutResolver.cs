using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Geometry;
using RackCad.Domain.Systems.Cantilever;

namespace RackCad.Application.Systems.Cantilever
{
    /// <summary>
    /// THE authority sobre la LISTA EFECTIVA de tramos de una secuencia de arriostramiento.
    ///
    /// <para><b>Una sola entrada para lo que viene después.</b> Los dos modos —automático y avanzado— acaban
    /// produciendo la misma cosa: una lista de tramos contigua, de abajo arriba, cada uno con sus dos cotas y
    /// lo que lleva dentro. El resolver posterior lee ESA lista y nada más. Es lo que impide que «automático»
    /// y «avanzado» sean dos caminos distintos hasta el dibujo, que es como se llega a que uno de los dos
    /// coloque un separador que el otro no.</para>
    ///
    /// <para><b>La lista cubre el NÚCLEO, no la columna entera.</b> Empieza donde empieza el primer tramo y
    /// acaba donde acaba el último; lo que queda por debajo y por encima son los espacios externos, que no son
    /// tramos y no llevan separador. Si los externos fueran tramos, <c>Distinct(inicios + fines)</c> pondría
    /// un separador en el piso y otro en la punta de la columna, y eso NO es el producto: la regla estándar
    /// coloca <c>paneles + huecos + 1</c> separadores, y ese <c>+1</c> ya cuenta la frontera de arriba del
    /// último panel.</para>
    ///
    /// <para><b>Un vacío es un tramo.</b> No la ausencia de uno. Un hueco implícito —dos tramos que no se
    /// tocan— se rechaza, porque no hay forma de distinguirlo de un tramo que alguien olvidó escribir.</para>
    /// </summary>
    public static class CantileverPanelLayoutResolver
    {
        /// <summary>
        /// Con cuánta holgura dos cotas cuentan como la MISMA frontera.
        ///
        /// Es la misma tolerancia con la que el resolver de secuencia busca el separador inferior de un panel,
        /// y tiene que serlo: si aquí dos tramos se tocaran y allí no, la lista diría que son vecinos y el
        /// dibujo pondría dos separadores en sitios que se ven iguales.
        /// </summary>
        internal const double JoinTolerance = 1e-9;

        /// <summary>
        /// La lista efectiva bajo la REGLA ESTÁNDAR: bloques de a dos paneles desde abajo, hueco central entre
        /// bloques y el bloque incompleto arriba.
        ///
        /// Reproduce exactamente la secuencia que la regla del producto lleva construyendo desde ADR-0027 D4;
        /// lo único nuevo es que ahora sale como una LISTA DE TRAMOS en vez de como un recorrido que rellenaba
        /// slots y separadores a la vez.
        /// </summary>
        /// <param name="firstStart">Cota del primer tramo, o sea el alto del espacio externo de abajo.</param>
        public static IReadOnlyList<CantileverPanelSegmentDesign> Standard(
            int bracedPanelCount, double bracedPanelHeight, double centralEmptySpaceHeight, double firstStart)
        {
            if (bracedPanelCount < 1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(bracedPanelCount), bracedPanelCount, "Una cantidad de paneles debe ser positiva.");
            }

            var segments = new List<CantileverPanelSegmentDesign>();
            var z = firstStart;
            var placed = 0;

            while (placed < bracedPanelCount)
            {
                var inBlock = Math.Min(2, bracedPanelCount - placed);

                for (var k = 0; k < inBlock; k++)
                {
                    segments.Add(Segment(z, z + bracedPanelHeight, CantileverPanelBracingMode.CrossBraced));
                    z += bracedPanelHeight;
                    placed++;
                }

                if (placed < bracedPanelCount)
                {
                    segments.Add(Segment(z, z + centralEmptySpaceHeight, CantileverPanelBracingMode.None));
                    z += centralEmptySpaceHeight;
                }
            }

            return segments;
        }

        /// <summary>
        /// Comprueba una lista de tramos contra el piso y la punta de la columna.
        ///
        /// <para>Todo lo que el dueño enumeró, y cada cosa con su propio mensaje: una lista que falla por dos
        /// razones distintas tiene que decir las dos, porque arreglar una y volver a fallar por la otra es la
        /// forma más rápida de que alguien piense que el editor está roto.</para>
        /// </summary>
        public static void Validate(
            IReadOnlyList<CantileverPanelSegmentDesign> segments,
            double floorZ,
            double columnTopZ,
            ICollection<CantileverDiagnostic> diagnostics)
        {
            if (segments == null)
            {
                throw new ArgumentNullException(nameof(segments));
            }

            if (diagnostics == null)
            {
                throw new ArgumentNullException(nameof(diagnostics));
            }

            if (segments.Count == 0)
            {
                diagnostics.Add(CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.BracingAdvancedLayoutEmpty,
                    "El modo avanzado no declara ningun tramo. Una secuencia vacia no es un arriostramiento: " +
                    "si lo que se quiere es una columna sin arriostrar, eso es un tramo con la casilla de " +
                    "tensores apagada, no una lista sin tramos."));
                return;
            }

            for (var i = 0; i < segments.Count; i++)
            {
                var s = segments[i];

                if (s == null)
                {
                    diagnostics.Add(CantileverDiagnostic.Blocking(
                        CantileverDiagnostics.BracingAdvancedSegmentInvalid,
                        "El tramo " + (i + 1) + " no existe."));
                    return;
                }

                if (!GeometryTolerance.IsFinite(s.StartElevation) ||
                    !GeometryTolerance.IsFinite(s.EndElevation))
                {
                    diagnostics.Add(CantileverDiagnostic.Blocking(
                        CantileverDiagnostics.BracingAdvancedSegmentInvalid,
                        "El tramo " + (i + 1) + " tiene cotas que no son numeros finitos."));
                    continue;
                }

                // ALTURA CERO Y ALTURA NEGATIVA, dichas aparte. Un tramo de altura cero suele ser un descuido
                // de edicion —dos cotas iguales— y uno invertido suele ser un par de cotas escritas al reves.
                if (Math.Abs(s.Height) <= JoinTolerance)
                {
                    diagnostics.Add(CantileverDiagnostic.Blocking(
                        CantileverDiagnostics.BracingAdvancedSegmentNotAscending,
                        "El tramo " + (i + 1) + " tiene altura cero: empieza y acaba en " +
                        Format(s.StartElevation) + " in."));
                }
                else if (s.Height < 0.0)
                {
                    diagnostics.Add(CantileverDiagnostic.Blocking(
                        CantileverDiagnostics.BracingAdvancedSegmentNotAscending,
                        "El tramo " + (i + 1) + " va de " + Format(s.StartElevation) + " a " +
                        Format(s.EndElevation) + " in, o sea hacia abajo. La cota de arriba tiene que ser " +
                        "mayor que la de abajo."));
                }
            }

            if (diagnostics.Any(d => d.IsBlocking))
            {
                // Sin tramos sanos no tiene sentido hablar de continuidad: los mensajes que saldrian serian
                // consecuencia del primer defecto y no defectos propios.
                return;
            }

            for (var i = 1; i < segments.Count; i++)
            {
                var previous = segments[i - 1];
                var current = segments[i];
                var gap = current.StartElevation - previous.EndElevation;

                if (Math.Abs(gap) <= JoinTolerance)
                {
                    continue;
                }

                // HUECO y SOLAPE se dicen aparte porque el remedio es distinto: uno pide declarar el vacio y
                // el otro pide corregir una cota.
                diagnostics.Add(CantileverDiagnostic.Blocking(
                    gap > 0.0
                        ? CantileverDiagnostics.BracingAdvancedLayoutHasGap
                        : CantileverDiagnostics.BracingAdvancedLayoutOverlaps,
                    gap > 0.0
                        ? "Entre el tramo " + i + " y el " + (i + 1) + " quedan " + Format(gap) +
                          " in sin declarar. Un vacio es un tramo con los tensores apagados, no un salto " +
                          "entre dos cotas: un hueco implicito no se distingue de un tramo que se olvido."
                        : "El tramo " + (i + 1) + " empieza en " + Format(current.StartElevation) +
                          " in, por debajo de donde acaba el " + i + " (" + Format(previous.EndElevation) +
                          " in): se solapan " + Format(-gap) + " in."));
            }

            var first = segments[0];
            var last = segments[segments.Count - 1];

            if (first.StartElevation < floorZ - JoinTolerance)
            {
                diagnostics.Add(CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.BracingAdvancedLayoutBelowFloor,
                    "El primer tramo empieza en " + Format(first.StartElevation) + " in, por debajo del piso (" +
                    Format(floorZ) + " in)."));
            }

            if (last.EndElevation > columnTopZ + JoinTolerance)
            {
                diagnostics.Add(CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.BracingDoesNotFitTheColumn,
                    "El ultimo tramo acaba en " + Format(last.EndElevation) + " in y la columna mide " +
                    Format(columnTopZ) + " in. Sube la altura de columna o baja el ultimo tramo: los tramos " +
                    "NO se comprimen para caber."));
            }
        }

        /// <summary>
        /// Las cotas que llevan separador: las fronteras ÚNICAS de la lista.
        ///
        /// <c>Distinct</c> es toda la regla de «no dupliques separadores en fronteras compartidas»: el fin de
        /// un tramo y el principio del siguiente son la MISMA cota, y ahi va UN separador, no dos. Es también
        /// el contenido de <c>paneles + huecos + 1</c>, dicho de la forma que no depende de contar.
        /// </summary>
        public static IReadOnlyList<double> SeparatorElevationsOf(
            IReadOnlyList<CantileverPanelSegmentDesign> segments)
        {
            if (segments == null)
            {
                throw new ArgumentNullException(nameof(segments));
            }

            var elevations = new List<double>();

            foreach (var z in segments
                         .SelectMany(s => new[] { s.StartElevation, s.EndElevation })
                         .OrderBy(z => z))
            {
                if (elevations.Count == 0 || Math.Abs(elevations[elevations.Count - 1] - z) > JoinTolerance)
                {
                    elevations.Add(z);
                }
            }

            return elevations;
        }

        private static CantileverPanelSegmentDesign Segment(
            double start, double end, CantileverPanelBracingMode mode) =>
            new CantileverPanelSegmentDesign
            {
                StartElevation = start,
                EndElevation = end,
                BracingMode = mode
            };

        private static string Format(double value) =>
            value.ToString("0.####", CultureInfo.InvariantCulture);
    }
}
