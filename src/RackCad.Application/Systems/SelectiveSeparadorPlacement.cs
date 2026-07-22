using RackCad.Application.Geometry;
using RackCad.Application.Headers;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// Pure placement of the SEPARADOR (the beam that bridges the gap between two back-to-back fondos), parameterized
    /// by view (I-22, E6). The lateral and planta builders derive the reaching-fondo gap differently — the lateral
    /// stacks one per vertical level via <see cref="SeparatorLevelCalculator"/>, the planta draws one per frente line —
    /// but both built an IDENTICAL <see cref="HeaderBlockRole.Separator"/> instance: anchored on the (mirrored) back
    /// post's TROQUEL_SEPARADOR, offset to the block's mate, with LONGITUD = the gap. That instance rule now lives here.
    /// </summary>
    public static class SelectiveSeparadorPlacement
    {
        /// <summary>One separador block spanning <paramref name="gap"/>, its connection point landing on
        /// <paramref name="anchor"/> and its origin offset back by the block's <paramref name="mate"/>.</summary>
        public static HeaderBlockInstance Separador(string block, string view, Point2D anchor, Point2D mate, double gap)
        {
            var separador = new HeaderBlockInstance
            {
                Role = HeaderBlockRole.Separator,
                PieceId = DynamicRackDefaults.SeparatorCatalogId,
                BlockName = block,
                View = view,
                ConnectionAnchor = anchor,
                Insertion = new Point2D(anchor.X - mate.X, anchor.Y - mate.Y)
            };
            separador.DynamicParameters[SelectiveRackDefaults.LengthParam] = gap;
            return separador;
        }
    }
}
