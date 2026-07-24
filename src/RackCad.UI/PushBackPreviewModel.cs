using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using RackCad.Application.Catalogs;
using RackCad.Application.Geometry;
using RackCad.Application.Headers;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;

namespace RackCad.UI
{
    internal enum PushBackPreviewKind
    {
        /// <summary>A world-space segment (real position, orientation and length taken from the plan).</summary>
        Line,

        /// <summary>A world-space box (e.g. a reference pallet), bottom-left at (X1,Y1), top-right at (X2,Y2).</summary>
        Box,

        /// <summary>A piece whose drawable size the plan does not carry: a fixed-size marker at (X1,Y1).</summary>
        Marker
    }

    /// <summary>One semantic preview primitive: what it is (role + piece) and where it lies in WORLD coordinates.</summary>
    internal sealed class PushBackPreviewPrimitive
    {
        public HeaderBlockRole Role { get; set; }
        public string PieceId { get; set; }
        public PushBackPreviewKind Kind { get; set; }
        public double X1 { get; set; }
        public double Y1 { get; set; }
        public double X2 { get; set; }
        public double Y2 { get; set; }

        /// <summary>Emphasis for the main load-path members (end beams, bed) over secondary ones.</summary>
        public bool Thick { get; set; }
    }

    /// <summary>
    /// The technical preview of one <see cref="DynamicSystemPlan"/> as SEMANTIC primitives (PB-VAL-01 round 3): real
    /// lines/boxes derived exclusively from data the plan and catalog already carry — insertion, rotation, mirror,
    /// LONGITUD/PERALTE/SAQUE parameters and catalog mate points. It never recomputes slope, snap, lengths, SAQUE or bed
    /// geometry, and never mutates the plan. Pure (no WPF types): tests inspect primitives and signatures directly; the
    /// window projects and paints them.
    /// </summary>
    internal sealed class PushBackPreviewModel
    {
        private PushBackPreviewModel(IReadOnlyList<PushBackPreviewPrimitive> primitives)
        {
            Primitives = primitives;
            if (primitives.Count > 0)
            {
                MinX = primitives.Min(p => Math.Min(p.X1, p.X2));
                MaxX = primitives.Max(p => Math.Max(p.X1, p.X2));
                MinY = primitives.Min(p => Math.Min(p.Y1, p.Y2));
                MaxY = primitives.Max(p => Math.Max(p.Y1, p.Y2));
            }
        }

        public IReadOnlyList<PushBackPreviewPrimitive> Primitives { get; }
        public bool IsEmpty => Primitives.Count == 0;
        public double MinX { get; }
        public double MinY { get; }
        public double MaxX { get; }
        public double MaxY { get; }

        public IEnumerable<PushBackPreviewPrimitive> Lines => Primitives.Where(p => p.Kind == PushBackPreviewKind.Line);

        public IEnumerable<PushBackPreviewPrimitive> OfRole(HeaderBlockRole role) => Primitives.Where(p => p.Role == role);

        /// <summary>A stable content signature (roles, pieces, kinds and rounded coordinates) — two previews of different
        /// cortes/views produce different signatures without any pixel comparison.</summary>
        public string Signature()
        {
            var builder = new StringBuilder();
            foreach (var p in Primitives)
            {
                builder.Append(p.Role).Append('|').Append(p.PieceId).Append('|').Append(p.Kind).Append('|')
                    .Append(p.X1.ToString("0.###", CultureInfo.InvariantCulture)).Append(',')
                    .Append(p.Y1.ToString("0.###", CultureInfo.InvariantCulture)).Append(',')
                    .Append(p.X2.ToString("0.###", CultureInfo.InvariantCulture)).Append(',')
                    .Append(p.Y2.ToString("0.###", CultureInfo.InvariantCulture)).Append(';');
            }

            return builder.ToString();
        }

        /// <summary>
        /// Interpret <paramref name="plan"/> into primitives. <paramref name="fallbackBeamDepth"/> is the resolved
        /// system's IN/OUT depth, used ONLY for the low lateral beam whose instance carries no PERALTE parameter (the
        /// same fallback the dynamic editor's preview applies). A null/empty plan yields an empty model.
        /// </summary>
        public static PushBackPreviewModel Build(DynamicSystemPlan plan, RackCatalog catalog, string view, double fallbackBeamDepth)
        {
            var primitives = new List<PushBackPreviewPrimitive>();
            if (plan == null)
            {
                return new PushBackPreviewModel(primitives);
            }

            var instances = plan.Headers.SelectMany(group => group.Instances).Concat(plan.LooseInstances);
            foreach (var instance in instances)
            {
                if (instance == null
                    || instance.Role == HeaderBlockRole.Annotation
                    || instance.Role == HeaderBlockRole.Dimension)
                {
                    continue; // decorations: not part of the technical preview
                }

                var primitive = Interpret(instance, catalog, string.IsNullOrWhiteSpace(instance.View) ? view : instance.View, fallbackBeamDepth);
                if (primitive != null)
                {
                    primitives.Add(primitive);
                }
            }

            return new PushBackPreviewModel(primitives);
        }

        private static PushBackPreviewPrimitive Interpret(HeaderBlockInstance instance, RackCatalog catalog, string view, double fallbackBeamDepth)
        {
            var lateral = string.Equals(view, "LATERAL", StringComparison.OrdinalIgnoreCase);
            double? longitud = Param(instance, SelectiveRackDefaults.LengthParam);
            double? peralte = Param(instance, SelectiveRackDefaults.PeralteParam);
            double? saque = Param(instance, SelectiveSafetyDefaults.SaqueParam);
            double? altura = Param(instance, SelectiveRackDefaults.PalletAltoParam);

            switch (instance.Role)
            {
                case HeaderBlockRole.Post:
                    return longitud.HasValue ? Segment(instance, 0.0, longitud.Value, vertical: true) : Marker(instance);

                case HeaderBlockRole.Horizontal:
                case HeaderBlockRole.ClosingHorizontal:
                case HeaderBlockRole.Separator:
                    if (longitud.HasValue) return Segment(instance, longitud.Value, 0.0, vertical: false);
                    return AnchorLineOrMarker(instance);

                case HeaderBlockRole.Diagonal:
                    return AnchorLineOrMarker(instance);

                case HeaderBlockRole.BasePlate:
                    return peralte.HasValue ? Segment(instance, peralte.Value, 0.0, vertical: false) : Marker(instance);

                case HeaderBlockRole.Rail:
                {
                    if (!longitud.HasValue) return Marker(instance);
                    // The bed line: from the rail's IN/OUT mate to (LONGITUD, mateY) in LOCAL coordinates, transformed by
                    // the instance (rotation = the already-resolved lane slope). Same interpretation the dynamic preview uses.
                    var mate = CatalogLookup.Local(catalog, instance.PieceId, FlowBedDefaults.RailInOutMatePoint, FlowBedDefaults.View);
                    var start = LocalToWorld(instance, mate);
                    var end = LocalToWorld(instance, new Point2D(longitud.Value, mate.Y));
                    return Line(instance, start, end, thick: true);
                }

                case HeaderBlockRole.Roller:
                case HeaderBlockRole.Brake:
                case HeaderBlockRole.Stop:
                    return Marker(instance);

                case HeaderBlockRole.Beam:
                    if (lateral)
                    {
                        // Seen end-on: the visible dimension is the beam DEPTH (its own PERALTE, or the system's IN/OUT depth).
                        var depth = peralte ?? (fallbackBeamDepth > 0.0 ? fallbackBeamDepth : (double?)null);
                        return depth.HasValue ? Segment(instance, 0.0, depth.Value, vertical: true, thick: true) : Marker(instance);
                    }

                    return longitud.HasValue
                        ? Segment(instance, longitud.Value, 0.0, vertical: false, thick: true)
                        : peralte.HasValue
                            ? Segment(instance, 0.0, peralte.Value, vertical: true, thick: true)
                            : Marker(instance);

                case HeaderBlockRole.Tope:
                    if (longitud.HasValue) return Segment(instance, longitud.Value, 0.0, vertical: false);
                    return saque.HasValue ? Segment(instance, 0.0, saque.Value, vertical: true) : Marker(instance);

                case HeaderBlockRole.Safety:
                {
                    if (!longitud.HasValue) return Marker(instance);
                    var element = catalog?.SafetyElements?.FirstOrDefault(entry => string.Equals(
                        entry?.Id, instance.PieceId, StringComparison.OrdinalIgnoreCase));
                    var isDeviator = SelectiveSafetyDefaults.IsType(element?.Type, SelectiveSafetyDefaults.DesviadorType);
                    return isDeviator
                        ? Segment(instance, 0.0, longitud.Value, vertical: true)
                        : Segment(instance, longitud.Value, 0.0, vertical: false);
                }

                case HeaderBlockRole.Pallet:
                    if (longitud.HasValue && altura.HasValue)
                    {
                        return new PushBackPreviewPrimitive
                        {
                            Role = instance.Role,
                            PieceId = instance.PieceId,
                            Kind = PushBackPreviewKind.Box,
                            X1 = instance.Insertion.X,
                            Y1 = instance.Insertion.Y,
                            X2 = instance.Insertion.X + (instance.MirroredX ? -longitud.Value : longitud.Value),
                            Y2 = instance.Insertion.Y + altura.Value
                        };
                    }

                    return Marker(instance);

                default:
                    return Marker(instance);
            }
        }

        private static double? Param(HeaderBlockInstance instance, string key)
            => instance.DynamicParameters.TryGetValue(key, out var value) && value > 0.0 ? value : (double?)null;

        /// <summary>A segment of local length (<paramref name="dx"/>, <paramref name="dy"/>) from the insertion,
        /// transformed by the instance's mirror + rotation (never recomputed — only what the plan already encodes).</summary>
        private static PushBackPreviewPrimitive Segment(HeaderBlockInstance instance, double dx, double dy, bool vertical, bool thick = false)
        {
            _ = vertical; // direction is fully encoded in (dx, dy); kept for call-site readability
            var end = LocalToWorld(instance, new Point2D(dx, dy));
            return Line(instance, new Point2D(instance.Insertion.X, instance.Insertion.Y), end, thick);
        }

        private static PushBackPreviewPrimitive AnchorLineOrMarker(HeaderBlockInstance instance)
        {
            var distinct = Math.Abs(instance.ConnectionAnchor.X - instance.Insertion.X) > 1e-9
                           || Math.Abs(instance.ConnectionAnchor.Y - instance.Insertion.Y) > 1e-9;
            return distinct
                ? Line(instance, instance.Insertion, instance.ConnectionAnchor, thick: false)
                : Marker(instance);
        }

        private static PushBackPreviewPrimitive Line(HeaderBlockInstance instance, Point2D a, Point2D b, bool thick)
            => new PushBackPreviewPrimitive
            {
                Role = instance.Role,
                PieceId = instance.PieceId,
                Kind = PushBackPreviewKind.Line,
                X1 = a.X,
                Y1 = a.Y,
                X2 = b.X,
                Y2 = b.Y,
                Thick = thick
            };

        private static PushBackPreviewPrimitive Marker(HeaderBlockInstance instance)
            => new PushBackPreviewPrimitive
            {
                Role = instance.Role,
                PieceId = instance.PieceId,
                Kind = PushBackPreviewKind.Marker,
                X1 = instance.Insertion.X,
                Y1 = instance.Insertion.Y,
                X2 = instance.Insertion.X,
                Y2 = instance.Insertion.Y
            };

        /// <summary>Mirror → rotate → translate, exactly the transform the AutoCAD drawer applies to block geometry.</summary>
        private static Point2D LocalToWorld(HeaderBlockInstance instance, Point2D local)
        {
            var localX = instance.MirroredX ? -local.X : local.X;
            var localY = instance.MirroredY ? -local.Y : local.Y;
            var cos = Math.Cos(instance.RotationRadians);
            var sin = Math.Sin(instance.RotationRadians);
            return new Point2D(
                instance.Insertion.X + localX * cos - localY * sin,
                instance.Insertion.Y + localX * sin + localY * cos);
        }
    }
}
