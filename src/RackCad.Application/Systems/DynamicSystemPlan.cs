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
                foreach (var offset in group.OffsetsX)
                {
                    foreach (var instance in group.Instances)
                    {
                        all.Add(ShiftedClone(instance, offset));
                    }

                    placements++;
                }
            }

            all.AddRange(LooseInstances);
            return new LateralHeaderLayout(all, 0.0, placements, LooseInstances.Count, 0.0);
        }

        private static HeaderBlockInstance ShiftedClone(HeaderBlockInstance source, double dx)
        {
            var clone = new HeaderBlockInstance
            {
                Role = source.Role,
                PieceId = source.PieceId,
                BlockName = source.BlockName,
                View = source.View,
                RotationRadians = source.RotationRadians,
                MirroredX = source.MirroredX,
                ConnectionAnchor = new Point2D(source.ConnectionAnchor.X + dx, source.ConnectionAnchor.Y),
                Insertion = new Point2D(source.Insertion.X + dx, source.Insertion.Y)
            };

            foreach (var pair in source.DynamicParameters)
            {
                clone.DynamicParameters[pair.Key] = pair.Value;
            }

            return clone;
        }
    }

    /// <summary>One distinct header definition plus the run positions (X) where it is placed.</summary>
    public sealed class HeaderGroup
    {
        public HeaderGroup(string name, IReadOnlyList<HeaderBlockInstance> instances, IReadOnlyList<double> offsetsX)
        {
            Name = name;
            Instances = instances ?? new List<HeaderBlockInstance>();
            OffsetsX = offsetsX ?? new List<double>();
        }

        /// <summary>Suggested AutoCAD block name for this header definition.</summary>
        public string Name { get; }

        /// <summary>The header's pieces in local coordinates (run X starts at 0).</summary>
        public IReadOnlyList<HeaderBlockInstance> Instances { get; }

        /// <summary>Run positions (StartX) where a reference to this header is placed.</summary>
        public IReadOnlyList<double> OffsetsX { get; }
    }
}
