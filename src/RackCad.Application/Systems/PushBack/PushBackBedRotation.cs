using System;
using RackCad.Application.Geometry;

namespace RackCad.Application.Systems.PushBack
{
    /// <summary>
    /// La geometría ASIMÉTRICA de la cama Push Back, en funciones puras (aclaración final del Owner, I-32).
    ///
    /// La cama no usa la misma referencia física en sus dos extremos:
    /// <list type="bullet">
    /// <item><b>Entrada/Salida</b> — mate por <c>TROQUEL_IN</c>: el punto local <c>TROQUEL_IN</c> del riel cae
    /// exactamente sobre el <c>TROQUEL_CAMA</c> del larguero In/Out.</item>
    /// <item><b>Intermedios y posterior</b> — tangentes a la línea del <b>ORIGEN</b> del bloque.</item>
    /// </list>
    ///
    /// Origen y <c>TROQUEL_IN</c> pertenecen al mismo bloque rígido, así que sus dos líneas son <b>paralelas</b>:
    /// comparten rotación y pendiente, pero están separadas por la componente perpendicular del mate local. Con el
    /// bloque sin rotar esa separación es <c>TROQUEL_IN.localY</c>.
    ///
    /// <b>El defecto que corrige:</b> la versión anterior derivaba la rotación como
    /// <c>atan2(HighMate − ExitMate)</c>, es decir, tratando los dos contactos como si estuvieran en la MISMA
    /// recta. No lo están: <c>ExitMate</c> vive en la línea de <c>TROQUEL_IN</c> y <c>HighMate</c> en la del origen.
    /// Medido: con aquella rotación el contacto posterior quedaba <b>exactamente 1.25"</b> fuera de la línea del
    /// origen — justo la separación entre las dos paralelas.
    /// </summary>
    public static class PushBackBedRotation
    {
        /// <summary>
        /// La pendiente OBJETIVO: 7/16" por pie comercial, es decir <c>(7/16)/12 = 7/192</c> pulgadas por pulgada.
        /// Es un objetivo, no un invariante: la pendiente final es la mejor que permite la retícula de troqueles.
        /// </summary>
        public static double TargetSlope => 7.0 / 192.0;

        /// <summary>
        /// La rotación θ del bloque completo de la cama, resuelta para que el contacto posterior caiga sobre la
        /// línea del ORIGEN mientras el <c>TROQUEL_IN</c> se mantiene sobre el contacto bajo.
        ///
        /// <para><b>Derivación.</b> La colocación es rígida y lleva el mate local <c>m</c> sobre el contacto bajo:
        /// <c>Transform(p) = ExitMate + R(θ)·(p − m)</c>. El origen aterriza entonces en
        /// <c>O = ExitMate − R(θ)·m</c>, y la línea del origen es <c>O + t·u</c> con <c>u = (cos θ, sin θ)</c>.
        /// Imponer que <c>HighMate</c> esté sobre esa recta es anular su distancia perpendicular:</para>
        ///
        /// <code>
        /// d = HighMate − O = E + R(θ)·m,   E = HighMate − ExitMate
        /// d.X·sin θ − d.Y·cos θ = 0
        /// </code>
        ///
        /// <para>Al desarrollarlo, los términos en <c>m.X</c> se cancelan entre sí y queda una ecuación limpia en la
        /// que <b>solo interviene la componente perpendicular del mate</b>:</para>
        ///
        /// <code>
        /// E.X·sin θ − E.Y·cos θ = m.Y
        /// </code>
        ///
        /// <para>Que es la forma <c>A·sin θ + B·cos θ = C</c> con <c>A = E.X</c>, <c>B = −E.Y</c>, <c>C = m.Y</c>, y
        /// se resuelve como <c>θ = asin(m.Y / |E|) − atan2(−E.Y, E.X)</c>. Se toma la rama principal, que es la
        /// solución de ángulo pequeño — la única físicamente válida para una cama.</para>
        ///
        /// <para>Con <c>m.Y = 0</c> —mate sobre la propia línea del origen— degenera en <c>atan2(E.Y, E.X)</c>, la
        /// fórmula antigua: era el caso particular que se estaba aplicando siempre.</para>
        ///
        /// Devuelve <c>null</c> cuando no hay solución real (<c>|E| &lt; m.Y</c>): con los dos contactos más juntos
        /// que la separación entre las paralelas, ninguna rotación pone el posterior sobre la línea del origen.
        /// </summary>
        public static double? Solve(Point2D exitMate, Point2D rearContact, Point2D railLocalMate)
        {
            var ex = rearContact.X - exitMate.X;
            var ey = rearContact.Y - exitMate.Y;
            var distance = Math.Sqrt(ex * ex + ey * ey);
            if (distance <= 0.0 || Math.Abs(railLocalMate.Y) > distance)
            {
                return null;
            }

            return Math.Asin(railLocalMate.Y / distance) - Math.Atan2(-ey, ex);
        }

        /// <summary>
        /// La Y del contacto bajo que daría la pendiente objetivo EXACTA, sin retícula: la posición teórica continua
        /// contra la que se mide el desempate. Se despeja <c>E.Y</c> de la misma ecuación con <c>θ = atan(7/192)</c>.
        /// </summary>
        public static double TheoreticalExitY(double rearContactX, double rearContactY, double exitMateX, double railLocalMateY)
        {
            var theta = Math.Atan(TargetSlope);
            var ex = rearContactX - exitMateX;
            return rearContactY - (ex * Math.Tan(theta) - railLocalMateY / Math.Cos(theta));
        }

        /// <summary>Dónde aterriza el ORIGEN local del bloque cuando su mate se atornilla sobre el contacto bajo.</summary>
        public static Point2D OriginFor(Point2D exitMate, Point2D railLocalMate, double rotationRadians)
        {
            var cos = Math.Cos(rotationRadians);
            var sin = Math.Sin(rotationRadians);
            return new Point2D(
                exitMate.X - railLocalMate.X * cos + railLocalMate.Y * sin,
                exitMate.Y - railLocalMate.X * sin - railLocalMate.Y * cos);
        }

        /// <summary>Distancia PERPENDICULAR de un punto a la línea del origen. Cero = tangente.</summary>
        public static double PerpendicularDistanceToOriginLine(
            Point2D point, Point2D exitMate, Point2D railLocalMate, double rotationRadians)
        {
            var origin = OriginFor(exitMate, railLocalMate, rotationRadians);
            return (point.X - origin.X) * Math.Sin(rotationRadians)
                - (point.Y - origin.Y) * Math.Cos(rotationRadians);
        }

        /// <summary>Error de una rotación contra el objetivo, medido sobre la PENDIENTE y no sobre el ángulo.</summary>
        public static double SlopeError(double rotationRadians) => Math.Abs(Math.Tan(rotationRadians) - TargetSlope);
    }
}
