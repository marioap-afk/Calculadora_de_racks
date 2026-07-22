using System;
using RackCad.Application.Geometry;
using RackCad.Application.Headers;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// Pure placement helpers for the larguero TOPE (rear pallet stop), parameterized by view (I-22, E6). The frontal,
    /// lateral and planta builders used to copy-paste the rise-and-snap Y formula and each hand-build a near-identical
    /// <see cref="HeaderBlockRole.Tope"/> instance. Those two rules now live here:
    /// <list type="bullet">
    /// <item><see cref="SnapY"/> — rise ~<see cref="YOffset"/>" above the larguero, then land on the TROQUEL_TOPE grid
    /// (a whole number of pasos from the post mate). Used by the frontal and lateral; the planta draws top-down and
    /// keeps the frente Y.</item>
    /// <item><see cref="Tope"/> — one tope block instance with the SAQUE stick-out, an optional LONGITUD (the frontal
    /// and planta stretch it to the larguero span; the lateral is seen end-on and carries none), and the mirror for
    /// the spot facing the central gap.</item>
    /// </list>
    /// </summary>
    public static class SelectiveTopePlacement
    {
        /// <summary>A tope's nominal rise ABOVE its larguero level (then snapped to the TROQUEL_TOPE grid).</summary>
        public const double YOffset = SelectiveSafetyPlacement.TopeYOffset;

        /// <summary>A tope's LONGITUD = its larguero's length + this (inches).</summary>
        public const double LengthAllowance = SelectiveSafetyPlacement.TopeLengthAllowance;

        /// <summary>The effective SAQUE (stick-out): the selection's value if positive, else the domain default.</summary>
        public static double Saque(SelectiveSafetySelection selection)
            => selection != null && selection.TopeSaque > 0.0 ? selection.TopeSaque : SelectiveSafetyPlacement.DefaultSaque;

        /// <summary>The tope Y: rise ~<see cref="YOffset"/>" above <paramref name="largueroY"/>, then snap to the grid
        /// (a whole number of <paramref name="paso"/> from <paramref name="troquelMateY"/>).</summary>
        public static double SnapY(double troquelMateY, double largueroY, double paso)
            => troquelMateY + Math.Round((largueroY + YOffset - troquelMateY) / paso, MidpointRounding.AwayFromZero) * paso;

        /// <summary>One larguero-tope block instance at (<paramref name="x"/>, <paramref name="y"/>) with the SAQUE
        /// stick-out; <paramref name="longitud"/> sets the LONGITUD when present (frontal/planta), and
        /// <paramref name="mirroredX"/> mirrors the spot facing the central gap.</summary>
        public static HeaderBlockInstance Tope(
            string pieceId, string block, string view, double x, double y, double saque,
            double? longitud = null, bool mirroredX = false)
        {
            var at = new Point2D(x, y);
            var instance = new HeaderBlockInstance
            {
                Role = HeaderBlockRole.Tope,
                PieceId = pieceId,
                BlockName = block,
                View = view,
                MirroredX = mirroredX,
                Insertion = at,
                ConnectionAnchor = at
            };
            if (longitud.HasValue)
            {
                instance.DynamicParameters[SelectiveRackDefaults.LengthParam] = longitud.Value;
            }

            instance.DynamicParameters[SelectiveSafetyPlacement.SaqueParam] = saque;
            return instance;
        }
    }
}
