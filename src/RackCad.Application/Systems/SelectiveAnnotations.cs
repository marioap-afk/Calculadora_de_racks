using System.Globalization;
using RackCad.Application.Geometry;
using RackCad.Application.Headers;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// Builds the text-label instances for the selective annotation toggles (numerar frentes / niveles /
    /// colocar nombre de rack), shared by the frontal / planta / lateral builders so the text height and the
    /// role/shape live in one place. The AutoCAD drawer materializes each as a DBText on the annotations layer.
    /// </summary>
    internal static class SelectiveAnnotations
    {
        /// <summary>Number/label text height (in).</summary>
        public const double TextHeight = 6.0;

        /// <summary>Margin (in) between a label and the piece it annotates.</summary>
        public const double Margin = 4.0;

        /// <summary>The label text height (in) for a given annotation scale (scale &lt;= 0 falls back to 1).</summary>
        public static double TextHeightFor(double scale) => TextHeight * (scale > 0.0 ? scale : 1.0);

        public static HeaderBlockInstance Label(string text, string view, Point2D at, double height = TextHeight)
            => new HeaderBlockInstance
            {
                Role = HeaderBlockRole.Annotation,
                View = view,
                Text = text,
                TextHeight = height,
                Insertion = at,
                ConnectionAnchor = at
            };

        public static string Num(int oneBased) => oneBased.ToString(CultureInfo.InvariantCulture);
    }
}
