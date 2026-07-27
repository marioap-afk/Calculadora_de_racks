using System;
using System.Collections.Generic;
using RackCad.Application.Catalogs;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// PB-004 (I-32, regla del Owner tras el rechazo del round 1) — la retícula de troqueles del poste a la que
    /// deben caer los DOS largueros de extremo.
    ///
    /// Es la MISMA retícula que usa el resolver compartido al colocar los largueros
    /// (<c>DynamicRackSystemResolver</c>: el punto <see cref="SelectiveRackDefaults.PostBeamPoint"/> del poste en la
    /// vista FRONTAL, resuelto con su peralte, y el paso <see cref="SelectiveRackDefaults.TroquelPaso"/>). Se expone
    /// aquí para que la geometría de Push Back pueda derivar el larguero de entrada/salida y volver a ajustarlo sin
    /// duplicar la base ni el paso — si esta retícula y la del resolver divergieran, un larguero quedaría fuera de
    /// troquel sin que nada lo delatara.
    ///
    /// NO confundir con la base que usa el tope posterior
    /// (<c>PushBackRearTopeBuilder.PostAnchorLocal</c>): aquella mide desde <c>TROQUEL_SEPARADOR</c> en LATERAL y es
    /// otra retícula, con otro propósito.
    /// </summary>
    public static class PushBackTroquelGrid
    {
        /// <summary>
        /// La base de la retícula: la Y del <see cref="SelectiveRackDefaults.PostBeamPoint"/> del poste en FRONTAL,
        /// resuelta con el peralte real. 0 cuando el catálogo no publica esa fila (mismo fallback que el resolver,
        /// para no divergir de él).
        /// </summary>
        public static double Base(DynamicRackSystem structure, RackCatalog catalog)
        {
            var postId = DynamicFrontGeometry.PostId(structure, catalog);
            var entry = catalog?.ConnectionLayout.FindConnectionLayout(
                postId, SelectiveRackDefaults.PostBeamPoint, "FRONTAL");
            if (entry == null)
            {
                return 0.0;
            }

            var peralte = DynamicFrontGeometry.PostPeralte(structure, catalog, postId);
            return SelectivePostGeometry.Resolve(
                entry,
                new Dictionary<string, double> { [SelectiveRackDefaults.PeralteParam] = peralte }).Y;
        }

        /// <summary>
        /// Ajusta <paramref name="value"/> al troquel válido más cercano de la retícula. Mismo redondeo que el
        /// resolver compartido (<c>MidpointRounding.AwayFromZero</c>), para que un valor a mitad de camino entre dos
        /// troqueles caiga siempre del mismo lado en todo el producto.
        /// </summary>
        public static double Snap(double value, double gridBase)
        {
            if (double.IsNaN(gridBase) || double.IsInfinity(gridBase))
            {
                return value;
            }

            var pitch = SelectiveRackDefaults.TroquelPaso;
            if (pitch <= 0.0)
            {
                return value;
            }

            var steps = Math.Round((value - gridBase) / pitch, MidpointRounding.AwayFromZero);
            return gridBase + steps * pitch;
        }
    }
}
