using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Geometry;
using RackCad.Application.Headers;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// The rear pallet-stop ("larguero tope") of a Push Back system: one <c>LARGUERO_ESCALON_TOPE_DE_3</c> (the Selective
    /// tope piece — NOT <c>POSTE_3_1_5_8_TOPE</c>) per front and load level at the HIGH (rear) end, active by default and
    /// deactivable through <see cref="PushBackRearTopeConfig.OffCells"/>. It carries the SAQUE (stick-out) parameter and
    /// the transverse LONGITUD of the corresponding beam, and is counted on its own (<see cref="HeaderBlockRole.Tope"/>),
    /// projected consistently across the lateral, rear-frontal and planta views (its physical count is one per active cell).
    /// </summary>
    public sealed class PushBackRearTopeBuilder
    {
        public const string TopePieceId = "LARGUERO_ESCALON_TOPE_DE_3";

        /// <summary>Rear topes in the LATERAL view, at the high (rear) beam of each active cell of the front.</summary>
        public IReadOnlyList<HeaderBlockInstance> BuildLateral(PushBackSystem system, RackCatalog catalog, int frontIndex, DynamicRackFront front = null)
            => Build(system, catalog, frontIndex, front, "LATERAL");

        /// <summary>Rear topes in the given <paramref name="view"/>, positioned at the high (rear) beam of each active cell.</summary>
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

            foreach (var placement in DynamicLoadBeamGeometry.Placements(structure, front).Where(placement => placement.IsEntrance))
            {
                var levelIndex = placement.LevelNumber - 1;
                if (!rearTope.At(frontIndex, levelIndex))
                {
                    continue; // this cell's rear tope is deactivated
                }

                var origin = new Point2D(placement.X, placement.Y);
                var instance = new HeaderBlockInstance
                {
                    Role = HeaderBlockRole.Tope,
                    PieceId = TopePieceId,
                    BlockName = block,
                    View = view,
                    Insertion = origin,
                    ConnectionAnchor = origin,
                    MirroredX = placement.MirroredX
                };
                instance.DynamicParameters[SelectiveSafetyDefaults.SaqueParam] = saque;
                if (placement.BeamLength > 0.0)
                {
                    instance.DynamicParameters[SelectiveRackDefaults.LengthParam] = placement.BeamLength;
                }

                result.Add(instance);
            }

            return result;
        }
    }
}
