using System;
using RackCad.Application.Geometry;

namespace RackCad.Application.Systems.Cantilever
{
    /// <summary>
    /// De qué extremo de qué diagonal es un adaptador: las cuatro manos posibles.
    ///
    /// No es decoración. Un ángulo tiene talón, y el talón mira a un sitio distinto en cada esquina del panel;
    /// dibujar los cuatro iguales es lo que hacía que la pieza pareciera girada al azar.
    /// </summary>
    public enum CantileverBraceAdapterHand
    {
        LowerLeft = 0,
        LowerRight = 1,
        UpperLeft = 2,
        UpperRight = 3
    }

    /// <summary>
    /// THE authority que orienta un adaptador de tensor.
    ///
    /// La orientación NO se declara en una tabla de cuatro casos escritos a mano: se DERIVA de la única cosa
    /// que la determina, que es hacia dónde queda el agujero del tensor respecto del agujero del separador. El
    /// ala que se atornilla al separador corre en el sentido en que el adaptador se aleja de su columna, y la
    /// que recibe la varilla sube o baja según la diagonal vaya hacia arriba o hacia abajo desde ese extremo.
    ///
    /// Derivarlo tiene dos consecuencias que una tabla no da. La primera es que los cuatro casos salen
    /// EXHAUSTIVOS por construcción, sin que nadie tenga que acordarse del cuarto. La segunda es que si un día
    /// una diagonal cambia de sentido, el adaptador la sigue solo, en vez de quedarse mirando a donde miraba
    /// la diagonal de antes.
    ///
    /// Es geometría de VISTA y de colocación, no de fabricación: aquí no hay preparación de bordes, ni
    /// tolerancias de armado, ni la soldadura del talón. Eso está declarado en el plan de representación.
    /// </summary>
    public static class CantileverBraceAdapterFrameResolver
    {
        /// <summary>
        /// Hacia dónde corre el ala que se atornilla al SEPARADOR, y hacia dónde la que recibe la VARILLA.
        ///
        /// Las dos salen de la misma resta: del centro del agujero del separador al centro del agujero de la
        /// varilla. Su componente en X dice de qué lado del panel está este extremo y su componente en Z si la
        /// diagonal sube o baja desde él.
        ///
        /// Una resta sin componente en X o sin componente en Z no describe ningún extremo de una diagonal —una
        /// diagonal que no avanza no es una diagonal— así que se rechaza en vez de inventarle un sentido.
        /// </summary>
        public static bool TryResolve(
            Point3D separatorHole,
            Point3D rodHole,
            out Vector3D alongSeparator,
            out Vector3D towardsRod,
            out CantileverBraceAdapterHand hand,
            out string reason)
        {
            alongSeparator = new Vector3D(0.0, 0.0, 0.0);
            towardsRod = new Vector3D(0.0, 0.0, 0.0);
            hand = CantileverBraceAdapterHand.LowerLeft;
            reason = null;

            var dx = rodHole.X - separatorHole.X;
            var dz = rodHole.Z - separatorHole.Z;

            if (Math.Abs(dx) <= GeometryTolerance.Length || Math.Abs(dz) <= GeometryTolerance.Length)
            {
                reason =
                    "El agujero de la varilla y el del separador no definen una diagonal: se separan " +
                    dx.ToString("0.####") + " in en X y " + dz.ToString("0.####") +
                    " in en Z, y un adaptador necesita las dos para saber hacia donde mira.";
                return false;
            }

            var right = dx > 0.0;
            var up = dz > 0.0;

            alongSeparator = new Vector3D(right ? 1.0 : -1.0, 0.0, 0.0);
            towardsRod = new Vector3D(0.0, 0.0, up ? 1.0 : -1.0);

            // La mano se nombra por el extremo del PANEL en el que está la pieza, que es como se lee un plano:
            // un adaptador cuyo tensor sube hacia la derecha está en la esquina de ABAJO a la izquierda.
            hand = up
                ? (right ? CantileverBraceAdapterHand.LowerLeft : CantileverBraceAdapterHand.LowerRight)
                : (right ? CantileverBraceAdapterHand.UpperLeft : CantileverBraceAdapterHand.UpperRight);

            return true;
        }
    }
}
