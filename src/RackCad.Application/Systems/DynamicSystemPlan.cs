using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Geometry;
using RackCad.Application.Headers;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// Structured plan for drawing a dynamic system: each distinct header is one nested block definition
    /// (its pieces in local coordinates) reused at every position where that header occurs; separators and
    /// derived posts are loose instances. The AutoCAD drawer turns the headers into nested block references
    /// (identical headers share a definition) and inserts the loose pieces directly.
    /// </summary>
    public sealed class DynamicSystemPlan
    {
        public DynamicSystemPlan(IReadOnlyList<HeaderGroup> headers, IReadOnlyList<HeaderBlockInstance> looseInstances)
        {
            Headers = headers ?? new List<HeaderGroup>();
            LooseInstances = looseInstances ?? new List<HeaderBlockInstance>();
        }

        /// <summary>Distinct header definitions and the run positions where each is placed.</summary>
        public IReadOnlyList<HeaderGroup> Headers { get; }

        /// <summary>Separators and derived posts (already in world coordinates).</summary>
        public IReadOnlyList<HeaderBlockInstance> LooseInstances { get; }

        /// <summary>
        /// Expand the plan into a single flat layout (headers replicated at every offset). Used for preview
        /// and tests; the drawer uses the structured form so identical headers share one block.
        /// </summary>
        public LateralHeaderLayout Flatten()
        {
            var all = new List<HeaderBlockInstance>();
            var placements = 0;

            foreach (var group in Headers)
            {
                foreach (var placement in group.Placements)
                {
                    foreach (var instance in group.Instances)
                    {
                        all.Add(PlacedClone(instance, placement));
                    }

                    placements++;
                }
            }

            all.AddRange(LooseInstances);
            return new LateralHeaderLayout(all, 0.0, placements, LooseInstances.Count, 0.0);
        }

        private static HeaderBlockInstance PlacedClone(HeaderBlockInstance source, HeaderPlacement placement)
        {
            // A mirrored placement flips around the insertion X (ScaleX = -1): a local X maps to InsertionX - X
            // and the piece's own mirror flag toggles. A normal placement just shifts by InsertionX.
            double WorldX(double localX) => placement.Mirrored ? placement.InsertionX - localX : placement.InsertionX + localX;

            var clone = new HeaderBlockInstance
            {
                Role = source.Role,
                PieceId = source.PieceId,
                BlockName = source.BlockName,
                View = source.View,
                RotationRadians = source.RotationRadians,
                MirroredX = placement.Mirrored ? !source.MirroredX : source.MirroredX,
                ConnectionAnchor = new Point2D(WorldX(source.ConnectionAnchor.X), source.ConnectionAnchor.Y),
                Insertion = new Point2D(WorldX(source.Insertion.X), source.Insertion.Y)
            };

            foreach (var pair in source.DynamicParameters)
            {
                clone.DynamicParameters[pair.Key] = pair.Value;
            }

            return clone;
        }
    }

    /// <summary>One placement of a header along the run: where its block reference goes and whether it is
    /// mirrored (every other header is mirrored so the celosía alternates direction along the line).</summary>
    public readonly struct HeaderPlacement
    {
        public HeaderPlacement(double insertionX, bool mirrored)
        {
            InsertionX = insertionX;
            Mirrored = mirrored;
        }

        /// <summary>X where the block reference is inserted (already accounts for the mirror flip).</summary>
        public double InsertionX { get; }

        public bool Mirrored { get; }
    }

    /// <summary>One distinct header definition plus the placements where it is used.</summary>
    public sealed class HeaderGroup
    {
        public HeaderGroup(string name, IReadOnlyList<HeaderBlockInstance> instances, IReadOnlyList<HeaderPlacement> placements)
        {
            Name = name;
            Instances = instances ?? new List<HeaderBlockInstance>();
            Placements = placements ?? new List<HeaderPlacement>();
        }

        /// <summary>Suggested AutoCAD block name for this header definition.</summary>
        public string Name { get; }

        /// <summary>The header's pieces in local coordinates (run X starts at 0).</summary>
        public IReadOnlyList<HeaderBlockInstance> Instances { get; }

        /// <summary>Where references to this header are placed (with mirror flags).</summary>
        public IReadOnlyList<HeaderPlacement> Placements { get; }
    }
}
