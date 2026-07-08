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
    /// (all beams of a bay share one length = the post spacing). The ground pallet's frente counts too.</item>
    /// <item><b>Separación</b>: from a pallet of height <c>alto</c> to the level above =
    /// roundUpTroquel( roundUpEven(alto + Clearance) + peralte(beam above) ). The clear opening is the rounded
    /// (alto + holgura); the beam that closes it adds its peralte; the result snaps up to the troquel grid.</item>
    /// <item><b>Piso</b>: level 0 is the ground. Without "larguero a piso" (default) it has NO beam — its pallet
    /// rests on the floor (Y=0) and the first larguero snaps up to the grid above it. With it, level 0 gets a
    /// beam at the lowest troquel and pallets stack from there.</item>
    /// <item><b>Altura de la cabecera</b>: roundUpFoot( topLevelY + topPalletAlto/3 ) — the post covers at least
    /// the bottom third of the top pallet, then rounds up to a whole foot. The tallest bay governs the run.</item>
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
                var levels = bayDesign.Levels;
                if (levels.Count == 0)
                {
                    system.Bays.Add(bay);
                    continue;
                }

                // Vertical stack. Level 0 is the ground pallet on the floor (Y=0). With "larguero a piso" it also
                // gets a beam at the lowest troquel; without it (default) the ground carries no beam and the first
                // larguero snaps up onto the grid above the ground pallet. Upper levels then stack by separation.
                double y;
                int start;
                if (bayDesign.FloorBeam)
                {
                    y = gridBase;
                    AddBeam(bay, y, levels[0]);
                    start = 1;
                }
                else if (levels.Count == 1)
                {
                    // Only a ground pallet, no larguero: the post still covers a third of that pallet.
                    height = Math.Max(height, RoundUpToFoot(PalletAlto(levels[0]) / 3.0));
                    system.Bays.Add(bay);
                    continue;
                }
                else
                {
                    y = SnapUp(RoundUpToMultiple(PalletAlto(levels[0]) + clearance, 2.0) + levels[1].BeamPeralte, gridBase, paso);
                    AddBeam(bay, y, levels[1]);
                    start = 2;
                }

                for (var j = start; j < levels.Count; j++)
                {
                    y += Separation(PalletAlto(levels[j - 1]), clearance, levels[j].BeamPeralte, paso);
                    AddBeam(bay, y, levels[j]);
                }

                // Post height from the top level (its beam Y + a third of its pallet); the run takes the tallest.
                height = Math.Max(height, RoundUpToFoot(y + PalletAlto(levels[levels.Count - 1]) / 3.0));
                system.Bays.Add(bay);
            }

            system.Height = height;
            return system;
        }

        private static void AddBeam(SelectiveBay bay, double y, SelectiveCell cell)
            => bay.Levels.Add(new SelectiveLevel { Y = y, BeamId = cell.BeamId, BeamPeralte = cell.BeamPeralte });

        private static double PalletAlto(SelectiveCell cell) => cell.Pallet?.Alto ?? 0.0;

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

        /// <summary>Smallest troquel of the grid (base + k*paso) that is ≥ <paramref name="value"/>.</summary>
        private static double SnapUp(double value, double gridBase, double paso)
            => gridBase + RoundUpToMultiple(value - gridBase, paso);

        /// <summary>Smallest multiple of <paramref name="m"/> that is ≥ <paramref name="x"/> (with a tiny epsilon so exact multiples don't jump).</summary>
        private static double RoundUpToMultiple(double x, double m) => Math.Ceiling(x / m - 1e-9) * m;

        private static double RoundUpToFoot(double x) => RoundUpToMultiple(x, FootInches);
    }
}
