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
            var keepFrenteY = string.Equals(view, "PLANTA", StringComparison.OrdinalIgnoreCase);
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
                    : SelectiveTopePlacement.SnapY(troquelMateY, placement.Y, SelectiveRackDefaults.TroquelPaso);
                // Commercial LONGITUD = the corresponding transverse beam length (per front x level) + the allowance.
                var baseLength = front != null
                    ? PushBackLoadBeamGeometry.CellBeamLength(structure, front, placement.LevelNumber)
                    : placement.BeamLength;
                double? longitud = baseLength > 0.0
                    ? baseLength + SelectiveTopePlacement.LengthAllowance
                    : (double?)null;
                result.Add(SelectiveTopePlacement.Tope(TopePieceId, block, view, placement.X, y, saque, longitud, mirroredX: placement.MirroredX));
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
