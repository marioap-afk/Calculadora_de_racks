using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Geometry;
using RackCad.Application.StructuralSections.Geometry;

namespace RackCad.Application.Systems.Cantilever
{
    /// <summary>
    /// THE authority that dice DÓNDE se ata el arriostramiento longitudinal a una columna.
    ///
    /// Al ALMA, y no al patín. Decisión del dueño en la ronda 3 de I-37D, y es como se ata un cantilever real:
    /// el separador pasa <b>entre</b> los dos patines de la columna y topa contra el alma. De ese único cambio
    /// de plano salen las tres cosas que él reportaba mal —el separador descentrado en planta, su longitud
    /// equivocada y su aspecto de ir por fuera de la línea— porque las tres eran la misma: se acotaba contra
    /// la cara exterior del patín.
    ///
    /// El espesor del alma se MIDE SOBRE EL CONTORNO QUE SE VA A DIBUJAR y no se lee de una tabla. Es
    /// ADR-0024 D5, y aquí importa especialmente: si el separador topara contra un alma tabulada mientras el
    /// dibujo pinta otra, la pieza no cerraría con su propio plano. Medirlo también hace la regla NEUTRAL —no
    /// pregunta de qué familia es la sección, cosa que a Cantilever le está vedada— y la hace fallar sola en
    /// una sección que no tiene alma, porque allí no hay nada que medir.
    /// </summary>
    public static class CantileverBracingPlaneResolver
    {
        /// <summary>
        /// El plano Y en el que corre el arriostramiento: el plano medio de la columna.
        ///
        /// Se toma del centro de la caja de la sección y no de un cero literal porque la columna está colocada
        /// por su marco, y ese marco puede no tener su origen en el centroide — I-36B lo dejó dicho para la
        /// familia S. El centro de la caja es el plano del alma esté donde esté el origen.
        /// </summary>
        public static double WebPlaneY(CantileverEnvelope3D crossSection) =>
            (crossSection.MinY + crossSection.MaxY) / 2.0;

        /// <summary>
        /// Cuánta anchura del perfil ocupa una sección delgada por su MITAD, comparada con su anchura total.
        ///
        /// Por encima de esto, lo que se cortó no es un alma. Un tubo cortado por su mitad da dos paredes que
        /// entre las dos no llegan ni de lejos a este tercio, pero un macizo daría el perfil entero; el límite
        /// separa «tiene alma» de «es otra cosa» sin preguntarle a nadie de qué familia es.
        /// </summary>
        private const double WebWidthShare = 1.0 / 3.0;

        /// <summary>
        /// Medio espesor del alma: hasta dónde puede llegar el separador antes de topar con ella.
        ///
        /// FALLA EN CERRADO. Una sección sin alma —un tubo, un macizo— no tiene dónde recibir este
        /// arriostramiento, y devolver un número cualquiera dibujaría un separador atornillado al aire.
        /// </summary>
        public static bool TryWebHalfThickness(
            StructuralSectionGeometry geometry, double chordTolerance, out double halfThickness, out string reason)
        {
            halfThickness = 0.0;
            reason = null;

            if (geometry == null)
            {
                reason = "No hay geometria de columna con la que resolver el plano de arriostramiento.";
                return false;
            }

            var bounds = geometry.Bounds;
            var midY = (bounds.MinY + bounds.MaxY) / 2.0;

            // Los tramos de MATERIAL que la recta de media altura corta: el contorno exterior aporta los
            // suyos y cada hueco se los quita. En una W o una S queda uno solo, el alma; en un tubo quedan dos,
            // una pared a cada lado, y ninguno contiene el centro.
            var solid = Spans(geometry.OuterContour, midY, chordTolerance);
            var holes = geometry.AllContours().Skip(1)
                .SelectMany(c => Spans(c, midY, chordTolerance))
                .ToList();

            var material = Subtract(solid, holes);

            var web = material.FirstOrDefault(i => i.Min <= 0.0 && i.Max >= 0.0);

            if (web.Max - web.Min <= GeometryTolerance.Length)
            {
                reason =
                    "La columna no tiene alma en su plano medio: el arriostramiento longitudinal se atornilla " +
                    "al alma y aqui no hay donde atornillarlo.";
                return false;
            }

            var width = web.Max - web.Min;

            if (width > bounds.Width * WebWidthShare)
            {
                reason =
                    "Cortada por su mitad, la columna mide " + width.ToString("0.###") + " in de los " +
                    bounds.Width.ToString("0.###") + " in de su anchura: eso no es un alma, y el " +
                    "arriostramiento longitudinal se atornilla al alma.";
                return false;
            }

            halfThickness = width / 2.0;
            return true;
        }

        /// <summary>Los tramos en X que la recta <c>y = level</c> corta dentro de un contorno cerrado.</summary>
        private static List<(double Min, double Max)> Spans(
            ClosedContour2D contour, double level, double chordTolerance)
        {
            var points = contour.Flatten(chordTolerance).ToList();
            var crossings = new List<double>();

            for (var i = 0; i < points.Count; i++)
            {
                var a = points[i];
                var b = points[(i + 1) % points.Count];

                // Un lado que no cruza el nivel no aporta nada, y uno que lo toca de refilon se cuenta una
                // sola vez: se pide que el nivel este en [min, max) del lado.
                var low = Math.Min(a.Y, b.Y);
                var high = Math.Max(a.Y, b.Y);

                if (level < low || level >= high || high - low <= GeometryTolerance.Length)
                {
                    continue;
                }

                crossings.Add(a.X + ((level - a.Y) / (b.Y - a.Y) * (b.X - a.X)));
            }

            crossings.Sort();

            var spans = new List<(double Min, double Max)>();

            for (var i = 0; i + 1 < crossings.Count; i += 2)
            {
                spans.Add((crossings[i], crossings[i + 1]));
            }

            return spans;
        }

        /// <summary>Los tramos de <paramref name="solid"/> que ningún tramo de <paramref name="holes"/> tapa.</summary>
        private static List<(double Min, double Max)> Subtract(
            List<(double Min, double Max)> solid, List<(double Min, double Max)> holes)
        {
            var result = solid;

            foreach (var hole in holes)
            {
                var next = new List<(double Min, double Max)>();

                foreach (var piece in result)
                {
                    if (hole.Max <= piece.Min || hole.Min >= piece.Max)
                    {
                        next.Add(piece);
                        continue;
                    }

                    if (hole.Min - piece.Min > GeometryTolerance.Length)
                    {
                        next.Add((piece.Min, hole.Min));
                    }

                    if (piece.Max - hole.Max > GeometryTolerance.Length)
                    {
                        next.Add((hole.Max, piece.Max));
                    }
                }

                result = next;
            }

            return result;
        }
    }
}
