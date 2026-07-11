using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using RackCad.Application.Geometry;
using RackCad.Application.Headers;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// Collapses a FLAT list of block instances into a structured <see cref="DynamicSystemPlan"/> (the ARRAY
    /// pattern): every run of IDENTICAL pieces becomes ONE nested block definition referenced at each position.
    /// AutoCAD sets a dynamic block's parameters per reference and re-evaluates the block each time — the dominant
    /// cost when inserting many pieces (see the autocad-insert-perf memory) — so N identical largueros/postes turn
    /// into 1 parameter-set + N cheap references instead of N parameter-sets. Pieces that occur only once, and text
    /// annotations, stay loose (a definition referenced once saves no re-evaluation). Geometry is preserved exactly:
    /// <see cref="DynamicSystemPlan.Flatten"/> reproduces the input as a multiset.
    /// </summary>
    public static class HeaderInstanceGrouper
    {
        public static DynamicSystemPlan Group(IReadOnlyList<HeaderBlockInstance> instances, string namePrefix)
        {
            var loose = new List<HeaderBlockInstance>();
            if (instances == null || instances.Count == 0)
            {
                return new DynamicSystemPlan(new List<HeaderGroup>(), loose);
            }

            // Bucket by signature, preserving first-seen order so the generated definition names are deterministic.
            var order = new List<string>();
            var bySignature = new Dictionary<string, List<HeaderBlockInstance>>(StringComparer.Ordinal);

            foreach (var instance in instances)
            {
                // Annotations are DBText (not blocks) and a blank-block piece can't be nested — keep them loose.
                if (instance.Role == HeaderBlockRole.Annotation || string.IsNullOrWhiteSpace(instance.BlockName))
                {
                    loose.Add(instance);
                    continue;
                }

                var signature = Signature(instance);
                if (!bySignature.TryGetValue(signature, out var bucket))
                {
                    bucket = new List<HeaderBlockInstance>();
                    bySignature[signature] = bucket;
                    order.Add(signature);
                }

                bucket.Add(instance);
            }

            var groups = new List<HeaderGroup>();
            var groupIndex = 0;
            foreach (var signature in order)
            {
                var bucket = bySignature[signature];
                if (bucket.Count < 2)
                {
                    loose.AddRange(bucket); // a nested definition referenced once costs a def and saves no re-eval
                    continue;
                }

                // The shared definition holds ONE copy of the piece with its connection anchor at the local origin;
                // its Insertion keeps the piece's own (Insertion − ConnectionAnchor) offset (e.g. a base plate's mate
                // point), so each placement — inserted at the piece's world connection anchor — lands exactly where the
                // flat instance did.
                var prototype = bucket[0];
                var local = new HeaderBlockInstance
                {
                    Role = prototype.Role,
                    PieceId = prototype.PieceId,
                    BlockName = prototype.BlockName,
                    View = prototype.View,
                    RotationRadians = prototype.RotationRadians,
                    MirroredX = prototype.MirroredX,
                    ConnectionAnchor = new Point2D(0.0, 0.0),
                    Insertion = new Point2D(
                        prototype.Insertion.X - prototype.ConnectionAnchor.X,
                        prototype.Insertion.Y - prototype.ConnectionAnchor.Y)
                };
                foreach (var pair in prototype.DynamicParameters)
                {
                    local.DynamicParameters[pair.Key] = pair.Value;
                }

                var placements = bucket
                    .Select(o => new HeaderPlacement(o.ConnectionAnchor.X, mirrored: false, insertionY: o.ConnectionAnchor.Y))
                    .ToList();

                groups.Add(new HeaderGroup(
                    namePrefix + "_" + prototype.Role + "_" + groupIndex.ToString(CultureInfo.InvariantCulture),
                    new[] { local },
                    placements));
                groupIndex++;
            }

            return new DynamicSystemPlan(groups, loose);
        }

        /// <summary>Identity of a piece for grouping: same role/piece/block/view/rotation/mirror, the same dynamic
        /// parameters, and the same (Insertion − ConnectionAnchor) offset. Two instances with the same signature are
        /// geometrically interchangeable up to their world position, so they can share one nested definition. Doubles
        /// are formatted round-trippably, so only bitwise-equal values merge — identical pieces group, near-misses do
        /// not, and <see cref="DynamicSystemPlan.Flatten"/> reproduces the input exactly.</summary>
        private static string Signature(HeaderBlockInstance instance)
        {
            var builder = new StringBuilder();
            builder.Append((int)instance.Role).Append('|')
                .Append(instance.BlockName).Append('|')
                .Append(instance.PieceId).Append('|')
                .Append(instance.View).Append('|')
                .Append(Key(instance.RotationRadians)).Append('|')
                .Append(instance.MirroredX ? '1' : '0').Append('|')
                .Append(Key(instance.Insertion.X - instance.ConnectionAnchor.X)).Append(',')
                .Append(Key(instance.Insertion.Y - instance.ConnectionAnchor.Y)).Append('|');

            foreach (var pair in instance.DynamicParameters.OrderBy(p => p.Key, StringComparer.Ordinal))
            {
                builder.Append(pair.Key).Append('=').Append(Key(pair.Value)).Append(';');
            }

            return builder.ToString();
        }

        private static string Key(double value) => value.ToString("R", CultureInfo.InvariantCulture);
    }
}
