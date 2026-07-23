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
        /// The rear tope's mirror. PB-VAL-02: it must NOT inherit the rear BEAM's mirror — doing so drew the tope inverted
        /// with respect to the post and the rear end, because the beam's mirror orients a BEAM profile, not the tope's step.
        /// In the elevation views (lateral / rear frontal) the rear tope faces the load side and is drawn UNMIRRORED; PLANTA
        /// is a top view where the tope lies along the beam, so there it keeps the beam's plan orientation.
        /// </summary>
        public static bool Mirrored(string view, bool beamMirroredX) => IsPlanta(view) && beamMirroredX;

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
                result.Add(SelectiveTopePlacement.Tope(
                    TopePieceId, block, view, placement.X, y, saque, longitud,
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
