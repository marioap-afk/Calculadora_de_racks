using System;
using RackCad.Application.Catalogs;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// Turns a pallet-driven <see cref="SelectivePalletDesign"/> into the RESOLVED
    /// <see cref="SelectiveRackSystem"/> the builder places. This is where the four derivation rules live:
    /// <list type="number">
    /// <item><b>Larguero</b>: LONGITUD = Frente*Count + Tolerance*(Count+1). Per bay the widest level governs
    /// (all beams of a bay share one length = the post spacing).</item>
    /// <item><b>Separación</b>: from a pallet of height <c>alto</c> to the level above =
    /// roundUpTroquel( roundUpEven(alto + Clearance) + peralte(beam above) ). The clear opening is the rounded
    /// (alto + holgura); the beam that closes it adds its peralte; the result snaps up to the troquel grid.</item>
    /// <item><b>Altura de la cabecera</b>: roundUpFoot( topLevelY + topPalletAlto/3 ) — the post covers at least
    /// the bottom third of the top pallet, then rounds up to a whole foot.</item>
    /// <item><b>Redondeos</b>: even (troquel pitch 2") upward, and foot (12") upward.</item>
    /// </list>
    /// Pure: no AutoCAD. The only catalog read is the troquel grid base (TROQUEL_LARGUERO.LocalY).
    /// </summary>
    public sealed class SelectiveGeometryResolver
    {
        /// <summary>Inches in a foot (post height rounds up to this).</summary>
        public const double FootInches = 12.0;

        public SelectiveRackSystem Resolve(SelectivePalletDesign design, RackCatalog catalog)
        {
            var system = new SelectiveRackSystem();
            if (design == null || design.Bays.Count == 0)
            {
                return system;
            }

            system.PostId = design.PostId;
            system.PostPeralte = design.PostPeralte;

            var paso = SelectiveRackDefaults.TroquelPaso;
            var tolerance = design.PalletTolerance;
            var clearance = design.VerticalClearance;

            // Troquel grid base: the Y of the first larguero troquel on the post.
            var troquel = catalog?.ConnectionLayout.FindConnectionLayout(
                design.PostId, SelectiveRackDefaults.PostBeamPoint, SelectiveRackDefaults.View);
            var gridBase = troquel?.LocalY ?? paso;

            var height = 0.0;
            foreach (var bayDesign in design.Bays)
            {
                var bay = new SelectiveBay { BeamLength = BayBeamLength(bayDesign, tolerance) };

                // Level Ys, bottom-up: first level snapped to the grid, then each separation stacked on.
                // Clamp to the grid base — there is no troquel below the lowest one, so the first larguero
                // can never sit under it (guards odd inputs from the per-cell editor / programmatic callers).
                var y = Math.Max(gridBase, Snap(design.FirstLevel, gridBase, paso));
                for (var j = 0; j < bayDesign.Levels.Count; j++)
                {
                    var cell = bayDesign.Levels[j];
                    if (j > 0)
                    {
                        var below = bayDesign.Levels[j - 1];
                        y += Separation(below.Pallet?.Alto ?? 0.0, clearance, cell.BeamPeralte, paso);
                    }

                    bay.Levels.Add(new SelectiveLevel { Y = y, BeamId = cell.BeamId, BeamPeralte = cell.BeamPeralte });
                }

                // Post height from this bay's top level; the run takes the tallest (Phase 1: uniform posts).
                if (bayDesign.Levels.Count > 0)
                {
                    var topAlto = bayDesign.Levels[bayDesign.Levels.Count - 1].Pallet?.Alto ?? 0.0;
                    var topY = bay.Levels[bay.Levels.Count - 1].Y;
                    height = Math.Max(height, RoundUpToFoot(topY + topAlto / 3.0));
                }

                system.Bays.Add(bay);
            }

            system.Height = height;
            return system;
        }

        /// <summary>Bay beam LONGITUD = the widest level's Frente*Count + Tolerance*(Count+1).</summary>
        private static double BayBeamLength(SelectiveBayDesign bay, double tolerance)
        {
            var max = 0.0;
            foreach (var cell in bay.Levels)
            {
                var frente = cell.Pallet?.Frente ?? 0.0;
                var count = Math.Max(1, cell.PalletCount);
                max = Math.Max(max, frente * count + tolerance * (count + 1));
            }

            return max;
        }

        /// <summary>
        /// Separation to the level above = roundUpTroquel( roundUpEven(alto + clearance) + beamPeralte ).
        /// Floored at one troquel pitch so levels never coincide/invert if a caller passes a degenerate pallet.
        /// </summary>
        private static double Separation(double palletAlto, double clearance, double beamPeralteAbove, double paso)
        {
            var claroLibre = RoundUpToMultiple(palletAlto + clearance, 2.0);
            return Math.Max(paso, RoundUpToMultiple(claroLibre + beamPeralteAbove, paso));
        }

        /// <summary>Snap a value to the nearest troquel of the grid (base + k*paso).</summary>
        private static double Snap(double value, double baseY, double paso)
            => baseY + Math.Round((value - baseY) / paso, MidpointRounding.AwayFromZero) * paso;

        /// <summary>Smallest multiple of <paramref name="m"/> that is ≥ <paramref name="x"/> (with a tiny epsilon so exact multiples don't jump).</summary>
        private static double RoundUpToMultiple(double x, double m) => Math.Ceiling(x / m - 1e-9) * m;

        private static double RoundUpToFoot(double x) => RoundUpToMultiple(x, FootInches);
    }
}
