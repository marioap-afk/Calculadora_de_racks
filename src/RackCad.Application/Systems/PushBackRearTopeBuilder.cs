using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Headers;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// The rear pallet-stop ("larguero tope") of a Push Back system: one <c>LARGUERO_ESCALON_TOPE_DE_3</c> (the Selective
    /// tope piece — NOT <c>POSTE_3_1_5_8_TOPE</c>) per front and load level at the HIGH (rear) end, active by default and
    /// deactivable through <see cref="PushBackRearTopeConfig.OffCells"/>. It uses the CANONICAL Selective tope rule
    /// (<see cref="SelectiveTopePlacement"/>): it rises above the rear larguero and snaps to the post's TROQUEL grid, with
    /// SAQUE and LONGITUD. Planta draws top-down and keeps the frente Y (no rise-and-snap). Counted on its own
    /// (<see cref="HeaderBlockRole.Tope"/>), one physical piece per active cell across the lateral/rear-frontal/planta.
    /// </summary>
    public sealed class PushBackRearTopeBuilder
    {
        public const string TopePieceId = "LARGUERO_ESCALON_TOPE_DE_3";

        /// <summary>
        /// Extra rise of the rear tope ABOVE the canonical Selective rise-and-snap (PB-VAL-03: the Owner measured the tope
        /// sitting exactly 4" too low in AutoCAD). It is exactly TWO <see cref="SelectiveRackDefaults.TroquelPaso"/> steps,
        /// so the tope stays on the very same TROQUEL grid the Selective snap lands on — the snap rule is preserved by
        /// construction, not bypassed. Elevation views only; PLANTA has no elevation.
        /// </summary>
        public const double ExtraRise = 2.0 * SelectiveRackDefaults.TroquelPaso;

        /// <summary>True when <paramref name="view"/> is the top view (which keeps the frente Y, no rise-and-snap).</summary>
        public static bool IsPlanta(string view) => string.Equals(view, "PLANTA", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// PB-VAL-02 — which way the rear tope FACES, derived from the system's LOAD SIDE, never from the rear beam's own
        /// mirror (the beam's mirror orients a BEAM profile, not the tope's step; inheriting it — or hardcoding the
        /// opposite — is what drew the tope inverted with respect to the post and the rear end).
        ///
        /// Push Back is LIFO: pallets enter and leave through the LOW end, which lies at DECREASING X from the rear beam,
        /// so the tope must stick out toward -X to retain them. The CANONICAL Selective convention fixes what that means:
        /// in <see cref="SelectiveSafetyPlacement.TopeSpots"/> the two topes of one central gap are
        /// <c>{AtFront=false, Mirror=false}</c> (its gap lies at +X) and <c>{AtFront=true, Mirror=true}</c> (its gap lies
        /// at -X) — i.e. a MIRRORED tope sticks out toward -X. Hence the elevations draw the rear tope mirrored.
        /// PLANTA is a top view where the tope lies ALONG the beam, so there it keeps the beam's plan orientation.
        /// </summary>
        public static bool Mirrored(string view, bool beamMirroredX)
            => IsPlanta(view) ? beamMirroredX : FacesLowEnd;

        /// <summary>
        /// Push Back's load side seen from the rear beam: the LOW end, at -X. Elevation topes face it (see
        /// <see cref="Mirrored"/>). A constant of the SYSTEM, not of the block or of the beam's mirror.
        /// </summary>
        public const bool FacesLowEnd = true;

        /// <summary>
        /// PB-VAL-02 — the rear tope's world anchor X in an ELEVATION seen along the depth (LATERAL). It is NOT the raw
        /// <c>placement.X</c> (the beam's insertion point): it is the beam's own REAL bed-contact connection point on the
        /// LOAD side, transformed by the beam's mirror. The rear beam exposes both edges of that contact face
        /// (<see cref="PushBackDefaults.HighEndBeamLeftBedMatePoint"/> / <see cref="PushBackDefaults.HighEndBeamRightBedMatePoint"/>,
        /// measured from the block); whichever of the two lands at the LOWER world X is the one facing the load, and that
        /// is where the stop must sit. Falls back to <paramref name="placementX"/> only when the catalog carries neither
        /// point (a piece with no measured contact face), never silently preferring one edge.
        /// </summary>
        public static double LateralAnchorX(RackCatalog catalog, string beamId, double placementX, bool beamMirroredX)
        {
            var left = catalog?.ConnectionLayout.FindConnectionLayout(
                beamId, PushBackDefaults.HighEndBeamLeftBedMatePoint, PushBackDefaults.HighEndBeamView);
            var right = catalog?.ConnectionLayout.FindConnectionLayout(
                beamId, PushBackDefaults.HighEndBeamRightBedMatePoint, PushBackDefaults.HighEndBeamView);
            if (left == null && right == null)
            {
                return placementX;
            }

            var anchor = double.MaxValue;
            if (left != null)
            {
                anchor = Math.Min(anchor, placementX + (beamMirroredX ? -left.LocalX : left.LocalX));
            }

            if (right != null)
            {
                anchor = Math.Min(anchor, placementX + (beamMirroredX ? -right.LocalX : right.LocalX));
            }

            return anchor;
        }

        /// <summary>The rear tope Y in an ELEVATION view: the canonical Selective rise-and-snap plus <see cref="ExtraRise"/>.</summary>
        public static double ElevationY(double troquelMateY, double largueroY)
            => SelectiveTopePlacement.SnapY(troquelMateY, largueroY, SelectiveRackDefaults.TroquelPaso) + ExtraRise;

        /// <summary>Rear topes in the LATERAL view (rise-and-snap above the rear beam of each active cell of the front).</summary>
        public IReadOnlyList<HeaderBlockInstance> BuildLateral(PushBackSystem system, RackCatalog catalog, int frontIndex, DynamicRackFront front = null)
            => Build(system, catalog, frontIndex, front, "LATERAL");

        /// <summary>Rear topes in <paramref name="view"/>. LATERAL/FRONTAL rise-and-snap; PLANTA keeps the frente Y.</summary>
        public IReadOnlyList<HeaderBlockInstance> Build(PushBackSystem system, RackCatalog catalog, int frontIndex, DynamicRackFront front, string view)
        {
            var result = new List<HeaderBlockInstance>();
            var structure = system?.Structure;
            if (structure == null)
            {
                return result;
            }

            var block = CatalogLookup.Block(catalog, TopePieceId, view);
            if (string.IsNullOrWhiteSpace(block))
            {
                return result;
            }

            var rearTope = system.RearTope ?? new PushBackRearTopeConfig();
            var saque = rearTope.Saque > 0.0 ? rearTope.Saque : PushBackDefaults.RearTopeSaque;
            var keepFrenteY = IsPlanta(view);
            var troquelMateY = keepFrenteY ? 0.0 : PostTroquelGridBase(structure, catalog);
            var highBeamId = string.IsNullOrWhiteSpace(system.HighEndBeamCatalogId)
                ? PushBackDefaults.HighEndBeamCatalogId
                : system.HighEndBeamCatalogId;
            // PB-VAL-02: only the LATERAL is seen along the depth, so only there does the load-side contact point of the
            // rear beam define the stop's X. The frontal and planta look ACROSS the beam, where the tope runs ALONG it and
            // shares the beam's transverse datum — the very datum its LONGITUD is measured from.
            var anchorAlongDepth = string.Equals(view, "LATERAL", StringComparison.OrdinalIgnoreCase);

            foreach (var placement in DynamicLoadBeamGeometry.Placements(structure, front).Where(placement => placement.IsEntrance))
            {
                var levelIndex = placement.LevelNumber - 1;
                if (!rearTope.At(frontIndex, levelIndex))
                {
                    continue; // this cell's rear tope is deactivated
                }

                var y = keepFrenteY
                    ? placement.Y
                    : ElevationY(troquelMateY, placement.Y);
                // Commercial LONGITUD = the corresponding transverse beam length (per front x level) + the allowance.
                var baseLength = front != null
                    ? PushBackLoadBeamGeometry.CellBeamLength(structure, front, placement.LevelNumber)
                    : placement.BeamLength;
                double? longitud = baseLength > 0.0
                    ? baseLength + SelectiveTopePlacement.LengthAllowance
                    : (double?)null;
                var x = anchorAlongDepth
                    ? LateralAnchorX(catalog, highBeamId, placement.X, placement.MirroredX)
                    : placement.X;
                result.Add(SelectiveTopePlacement.Tope(
                    TopePieceId, block, view, x, y, saque, longitud,
                    mirroredX: Mirrored(view, placement.MirroredX)));
            }

            return result;
        }

        /// <summary>The post's first TROQUEL_LARGUERO Y (resolved with the post peralte) — the tope snap grid base.</summary>
        private static double PostTroquelGridBase(DynamicRackSystem structure, RackCatalog catalog)
        {
            var postId = DynamicFrontGeometry.PostId(structure, catalog);
            var postPeralte = DynamicFrontGeometry.PostPeralte(structure, catalog, postId);
            var entry = catalog?.ConnectionLayout.FindConnectionLayout(postId, SelectiveRackDefaults.PostBeamPoint, SelectiveRackDefaults.View);
            return SelectivePostGeometry.Resolve(
                entry,
                new Dictionary<string, double> { [SelectiveRackDefaults.PeralteParam] = postPeralte }).Y;
        }
    }
}
