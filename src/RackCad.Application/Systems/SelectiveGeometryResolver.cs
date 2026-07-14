using System;
using System.Collections.Generic;
using System.Linq;
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
            system.PalletDepth = design.PalletDepth;
            system.DepthCount = Math.Max(1, design.DepthCount);
            foreach (var separator in design.SeparatorLengths)
            {
                system.SeparatorLengths.Add(separator);
            }

            // Per-fondo depth: fondo 0 = PalletDepth; each extra fondo its own override (else inherits fondo 0).
            // Plus each fondo's optional custom cabecera-depth override (<=0 = derived by the rule downstream).
            var baseDepth = design.PalletDepth > 0.0 ? design.PalletDepth : SelectiveRackDefaults.DefaultPalletDepth;
            for (var k = 0; k < system.DepthCount; k++)
            {
                var over = k >= 1 && (k - 1) < design.ExtraFondoDepths.Count ? design.ExtraFondoDepths[k - 1] : 0.0;
                system.FondoDepths.Add(over > 0.0 ? over : baseDepth);
                system.FondoCabeceraOverrides.Add(k < design.CabeceraFondoOverrides.Count ? design.CabeceraFondoOverrides[k] : 0.0);
            }

            system.DrawBasePlate = design.DrawBasePlate;
            system.NumberFronts = design.NumberFronts;
            system.NumberLevels = design.NumberLevels;
            system.DrawRackName = design.DrawRackName;
            system.DrawPallets = design.DrawPallets;
            system.AnnotationScale = design.AnnotationScale > 0.0 ? design.AnnotationScale : 1.0;
            system.Dimensions = design.Dimensions;
            system.DimensionStyle = design.DimensionStyle;
            foreach (var safety in design.SafetySelections)
            {
                // Keep a selection if it counts (quantity), draws by default (a side), OR draws on some post (an override).
                var hasPostSide = safety?.PostSides != null && safety.PostSides.Any(p => p != null && p.Side != SafetySide.None);
                if (safety != null && (safety.Quantity > 0 || safety.Side != SafetySide.None || hasPostSide) && !string.IsNullOrWhiteSpace(safety.ElementId))
                {
                    var copy = new SelectiveSafetySelection { ElementId = safety.ElementId, Quantity = safety.Quantity, Side = safety.Side, TopeShared = safety.TopeShared, TopeSaque = safety.TopeSaque, TopeFrontal = safety.TopeFrontal, TopeFondo = safety.TopeFondo, ParrillaFrontal = safety.ParrillaFrontal, ParrillaLateral = safety.ParrillaLateral, ParrillaFrente = safety.ParrillaFrente };
                    foreach (var post in safety.PostSides)
                    {
                        if (post != null) copy.PostSides.Add(new SafetyPostSide { PostIndex = post.PostIndex, Side = post.Side });
                    }

                    foreach (var off in safety.TopeOffCells)
                    {
                        if (off != null) copy.TopeOffCells.Add(new SelectiveGridCell { Frente = off.Frente, Level = off.Level });
                    }

                    foreach (var off in safety.ParrillaOffCells)
                    {
                        if (off != null) copy.ParrillaOffCells.Add(new SelectiveGridCell { Frente = off.Frente, Level = off.Level });
                    }

                    system.SafetySelections.Add(copy);
                }
            }

            // Per-post PERALTE: each post uses its own override (design.PostPeraltes) or the run default. Sized to the
            // MASTER grid — the longest fondo (most frentes) has the most posts; a shorter fondo is a prefix of it.
            var fondoCountForSlots = Math.Max(1, design.DepthCount);
            var masterDesignCount = design.Bays.Count;
            for (var k = 1; k < fondoCountForSlots; k++)
            {
                masterDesignCount = Math.Max(masterDesignCount, BaysForFondo(design, k).Count);
            }

            var postSlots = masterDesignCount + 1;
            for (var i = 0; i < postSlots; i++)
            {
                var over = i < design.PostPeraltes.Count ? design.PostPeraltes[i] : 0.0;
                system.PostPeraltes.Add(over > 0.0 ? over : design.PostPeralte);
            }

            var paso = SelectiveRackDefaults.TroquelPaso;
            var tolerance = design.PalletTolerance;
            var clearance = design.VerticalClearance;

            // Troquel grid base: the Y of the first larguero troquel on the post.
            var troquel = catalog?.ConnectionLayout.FindConnectionLayout(
                design.PostId, SelectiveRackDefaults.PostBeamPoint, SelectiveRackDefaults.View);
            var gridBase = troquel?.LocalY ?? paso;

            // BeamProfileStartY is a linear catalog scan per bay and the run repeats 1-2 beam ids: memoize the
            // RESULT of the same lookup, local to this Resolve so a catalog reload can never serve stale values.
            // The comparer matches FindConnectionLayout's OrdinalIgnoreCase; blank ids keep the 0.0 fallback
            // (FindConnectionLayout returns null for them and the ?? 0.0 applies) without touching the Dictionary.
            var beamStartYById = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
            double CachedBeamStartY(string beamId)
            {
                if (string.IsNullOrWhiteSpace(beamId)) return 0.0;
                if (!beamStartYById.TryGetValue(beamId, out var startY))
                {
                    startY = BeamProfileStartY(catalog, beamId, SelectiveRackDefaults.View);
                    beamStartYById[beamId] = startY;
                }

                return startY;
            }

            // Each fondo (doble profundidad) resolves its OWN vertical levels AND its OWN frente count — a corner
            // layout can have, say, 3 frentes on one line and 6 on the next. Fondo 0 is the primary/front face.
            var bays0 = ResolveFondo(design.Bays, paso, tolerance, clearance, gridBase, design.FloorBeamRise, CachedBeamStartY);
            foreach (var bay in bays0)
            {
                system.Bays.Add(bay);
            }

            system.FondoBays.Add(bays0);

            var fondoCount = Math.Max(1, design.DepthCount);
            for (var k = 1; k < fondoCount; k++)
            {
                // Keep each fondo's OWN frente count (no padding/truncation). Its overlapping frentes are aligned below.
                system.FondoBays.Add(ResolveFondo(BaysForFondo(design, k), paso, tolerance, clearance, gridBase, design.FloorBeamRise, CachedBeamStartY));
            }

            // The LONGEST fondo (most frentes) is the width master: every fondo adopts its per-bay width for the frentes
            // they overlap, so the shared prefix's posts coincide (the fondos "join" there) and a longer fondo simply
            // extends past the shorter ones. Fondo 0's bays are the same objects as system.Bays, so this updates both.
            var master = system.FondoBays[0];
            foreach (var fondo in system.FondoBays)
            {
                if (fondo.Count > master.Count) master = fondo;
            }

            // Align each overlapping frente to a single shared width so the posts coincide. The MASTER (longest fondo)
            // governs — EXCEPT where its bay is an empty "column" (a frente with no levels, BeamLength 0): a 0 there
            // would collapse the grid and zero out shorter fondos' real frentes, so the widest real bay among the fondos
            // governs that index instead (the column just spans the shared width).
            for (var i = 0; i < master.Count; i++)
            {
                var width = master[i].BeamLength;
                var governing = master[i].GoverningBeamId;
                if (width <= 0.0)
                {
                    foreach (var fondo in system.FondoBays)
                    {
                        if (i < fondo.Count && fondo[i].BeamLength > width)
                        {
                            width = fondo[i].BeamLength;
                            governing = fondo[i].GoverningBeamId;
                        }
                    }
                }

                foreach (var fondo in system.FondoBays)
                {
                    if (i < fondo.Count)
                    {
                        fondo[i].BeamLength = width;
                        fondo[i].GoverningBeamId = governing;
                    }
                }
            }

            var overallHeight = 0.0;
            foreach (var fondo in system.FondoBays)
            {
                overallHeight = Math.Max(overallHeight, MaxHeight(fondo));
            }

            system.Height = overallHeight;

            // Per-post cabeceras span the MASTER grid (masterCount frentes -> masterCount+1 posts); pad with null so
            // absent ones fall back to the run default. Custom cabeceras are still authored on fondo 0's posts only.
            var postCount = master.Count + 1;
            for (var i = 0; i < postCount; i++)
            {
                system.PostCabeceras.Add(i < design.PostCabeceras.Count ? design.PostCabeceras[i] : null);
            }

            return system;
        }

        /// <summary>The design bays fondo <paramref name="k"/> uses: its own <see cref="SelectivePalletDesign.ExtraFondoBays"/>
        /// entry when present, else fondo 0's <see cref="SelectivePalletDesign.Bays"/> (the plain doble-profundidad case).</summary>
        private static IList<SelectiveBayDesign> BaysForFondo(SelectivePalletDesign design, int k)
        {
            if (k <= 0)
            {
                return design.Bays;
            }

            var index = k - 1;
            if (index < design.ExtraFondoBays.Count && design.ExtraFondoBays[index] != null && design.ExtraFondoBays[index].Count > 0)
            {
                return design.ExtraFondoBays[index];
            }

            return design.Bays;
        }

        private static double MaxHeight(IEnumerable<SelectiveBay> bays)
        {
            var height = 0.0;
            foreach (var bay in bays)
            {
                if (bay.Height > height)
                {
                    height = bay.Height;
                }
            }

            return height;
        }

        /// <summary>
        /// Resolve ONE fondo's design bays into placed bays (each level at its troquel Y, plus the per-bay height).
        /// The four derivation rules (see the class summary) run here; the caller then shares fondo 0's widths so the
        /// posts of every fondo align. A bay with no levels stays an empty frente (Height from its override, else 0).
        /// </summary>
        private static List<SelectiveBay> ResolveFondo(
            IList<SelectiveBayDesign> designBays, double paso, double tolerance, double clearance,
            double gridBase, double floorBeamRise, Func<string, double> cachedBeamStartY)
        {
            var bays = new List<SelectiveBay>();
            foreach (var bayDesign in designBays)
            {
                var bay = new SelectiveBay
                {
                    BeamLength = BayBeamLength(bayDesign, tolerance, out var governingBeamId),
                    GoverningBeamId = governingBeamId
                };
                // Copy the medio-frente tramos verbatim (free measures, not derived from pallets). Geometry validity
                // (do the specified tramos + intermediate posts fit the bay?) is resolved later against the SHARED
                // BeamLength in SelectiveMedioFrente — extra fondos adopt fondo 0's width, which isn't known here.
                foreach (var segment in bayDesign.Segments)
                {
                    bay.Segments.Add(new SelectiveSegment { Length = segment.Length, Loaded = segment.Loaded });
                }
                var levels = bayDesign.Levels;
                if (levels.Count == 0)
                {
                    bay.Height = WithOverride(0.0, bayDesign.HeightOverride);
                    bays.Add(bay);
                    continue;
                }

                // Vertical stack. Level 0 is the ground pallet on the floor (Y=0). With "larguero a piso" it also
                // gets a beam at the lowest troquel; without it (default) the ground carries no beam and the first
                // larguero snaps up onto the grid above the ground pallet. Upper levels then stack by separation.
                double y;
                int start;
                if (bayDesign.FloorBeam)
                {
                    // The floor larguero rises FloorBeamRise above the lowest troquel so its ménsula clears the base
                    // plate. The rise is user-entered, so snap it up to the troquel pitch — otherwise the floor beam
                    // AND every level stacked above it (separations are always multiples of paso) leave the grid.
                    y = gridBase + RoundUpToMultiple(floorBeamRise, paso);
                    AddBeam(bay, y, levels[0]);
                    start = 1;
                }
                else if (levels.Count == 1)
                {
                    // Only a ground pallet on the floor, no larguero: the post covers a third of it, measured from the floor.
                    SetFloorPallet(bay, levels[0]);
                    bay.Height = WithOverride(RoundUpToFoot(PalletAlto(levels[0]) / 3.0), bayDesign.HeightOverride);
                    bays.Add(bay);
                    continue;
                }
                else
                {
                    // Ground pallet on the floor: the first larguero only needs to clear the pallet + holgura above
                    // the FLOOR — there is no beam under it, so no peralte term — snapped up onto the grid. A manual
                    // clear override (the distance from the floor) replaces the pallet-derived clear.
                    SetFloorPallet(bay, levels[0]); // the bottom pallet rests on the floor, not a beam
                    var firstClear = levels[1].ClearOverride.HasValue && levels[1].ClearOverride.Value > 0.0
                        ? levels[1].ClearOverride.Value
                        : RoundUpToMultiple(PalletAlto(levels[0]) + clearance, 2.0);
                    y = SnapUp(firstClear, gridBase, paso);
                    AddBeam(bay, y, levels[1]);
                    start = 2;
                }

                for (var j = start; j < levels.Count; j++)
                {
                    y += SeparationFor(levels[j], PalletAlto(levels[j - 1]), clearance, paso);
                    AddBeam(bay, y, levels[j]);
                }

                // Height this bay needs. The top pallet rests on the beam's escalón (INICIO_PERFIL's Y above the
                // troquel), so the third-of-the-pallet coverage is measured from THAT surface, not the troquel.
                // A manual per-bay override replaces the computed height.
                var top = levels[levels.Count - 1];
                var loadSurface = y + cachedBeamStartY(top.BeamId);
                bay.Height = WithOverride(RoundUpToFoot(loadSurface + PalletAlto(top) / 3.0), bayDesign.HeightOverride);
                bays.Add(bay);
            }

            return bays;
        }

        /// <summary>The manual override if it is a positive number, else the auto value.</summary>
        private static double WithOverride(double auto, double? over)
            => over.HasValue && over.Value > 0.0 ? over.Value : auto;

        private static void AddBeam(SelectiveBay bay, double y, SelectiveCell cell)
            => bay.Levels.Add(new SelectiveLevel
            {
                Y = y,
                BeamId = cell.BeamId,
                BeamPeralte = cell.BeamPeralte,
                PalletFrente = cell.Pallet?.Frente ?? 0.0,
                PalletAlto = cell.Pallet?.Alto ?? 0.0,
                PalletCount = Math.Max(1, cell.PalletCount)
            });

        /// <summary>Record a pallet that rests on the FLOOR (no larguero under it) so the visual reference can draw it.</summary>
        private static void SetFloorPallet(SelectiveBay bay, SelectiveCell cell)
        {
            bay.FloorPalletFrente = cell.Pallet?.Frente ?? 0.0;
            bay.FloorPalletAlto = cell.Pallet?.Alto ?? 0.0;
            bay.FloorPalletCount = Math.Max(1, cell.PalletCount);
        }

        private static double PalletAlto(SelectiveCell cell) => cell.Pallet?.Alto ?? 0.0;

        /// <summary>The larguero's INICIO_PERFIL Y (the escalón height above the troquel where the pallet rests); 0 if unset.</summary>
        private static double BeamProfileStartY(RackCatalog catalog, string beamId, string view)
            => catalog?.ConnectionLayout.FindConnectionLayout(beamId, SelectiveRackDefaults.BeamProfileStartPoint, view)?.LocalY ?? 0.0;

        /// <summary>
        /// Bay beam LONGITUD = the longest level, where a level is either its manual override or the auto
        /// Frente*Count + Tolerance*(Count+1). All beams of a bay share one length (the post spacing).
        /// <paramref name="governingBeamId"/> reports WHICH level's beam set the length, so downstream
        /// geometry (post spacing) uses that beam's ménsula overhang, not an arbitrary level's.
        /// </summary>
        private static double BayBeamLength(SelectiveBayDesign bay, double tolerance, out string governingBeamId)
        {
            var max = 0.0;
            governingBeamId = null;
            foreach (var cell in bay.Levels)
            {
                var desired = cell.BeamLengthOverride.HasValue && cell.BeamLengthOverride.Value > 0.0
                    ? cell.BeamLengthOverride.Value
                    : AutoBeamLength(cell, tolerance);
                if (desired > max)
                {
                    max = desired;
                    governingBeamId = cell.BeamId;
                }
            }

            return max;
        }

        private static double AutoBeamLength(SelectiveCell cell, double tolerance)
        {
            var frente = cell.Pallet?.Frente ?? 0.0;
            var count = Math.Max(1, cell.PalletCount);
            return frente * count + tolerance * (count + 1);
        }

        /// <summary>Separation below a level: the manual clear override (snapped up to the troquel grid) if set, else the pallet-derived auto.</summary>
        private static double SeparationFor(SelectiveCell cell, double palletAltoBelow, double clearance, double paso)
        {
            if (cell.ClearOverride.HasValue && cell.ClearOverride.Value > 0.0)
            {
                return Math.Max(paso, RoundUpToMultiple(cell.ClearOverride.Value, paso));
            }

            return Separation(palletAltoBelow, clearance, cell.BeamPeralte, paso);
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
