using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Geometry;
using RackCad.Domain.Systems.FlowBed;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;

namespace RackCad.UI.Preview
{
    /// <summary>
    /// The shared preview PARTS: how each kind of piece is drawn, given an already-resolved plan. Extracted from the
    /// dynamic editor's renderer (which now calls these instead of its own copies), so both editors draw a rail, a
    /// roller, a safety element or a load beam exactly the same way.
    ///
    /// Neutral by contract (ADR-0019 §3): no <c>RackSystemKind</c> and no branch per system — every method takes the
    /// instances it must draw and the catalog it must read mates from. Each editor decides WHICH plans to feed in.
    ///
    /// These parts CONSUME resolved geometry: they never recompute slope, snap, mates, lengths, SAQUE or peraltes, and
    /// they never mutate the plan or the instances they receive.
    /// </summary>
    internal static class EditorPreviewParts
    {
        /// <summary>
        /// Load beams of a given piece id, drawn as the vertical peralte segment on the beam's own axis. This is the
        /// dynamic renderer's <c>DrawLoadBeams</c>, unchanged: same filter, same 5.0 thickness, same colour.
        /// </summary>
        public static void LoadBeams(
            EditorPreviewSurface surface, IEnumerable<HeaderBlockInstance> instances, string beamId, double depth)
        {
            foreach (var placement in instances.Where(instance =>
                         instance.Role == HeaderBlockRole.Beam
                         && string.Equals(instance.PieceId, beamId, StringComparison.OrdinalIgnoreCase)))
            {
                surface.Line(
                    surface.Map(placement.Insertion.X, placement.Insertion.Y),
                    surface.Map(placement.Insertion.X, placement.Insertion.Y + depth),
                    EditorPreviewPalette.Horizontal,
                    5.0);
            }
        }

        /// <summary>
        /// Intermediate supports: the vertical slotted bracket on the post axis plus its short bed contact. The dynamic
        /// renderer's <c>DrawIntermediateBeams</c>, unchanged — including reading the left/right bed mates from the
        /// catalog and choosing by the instance's mirror.
        /// </summary>
        public static void IntermediateSupports(
            EditorPreviewSurface surface, IEnumerable<HeaderBlockInstance> instances, RackCatalog catalog,
            string beamId, string leftMatePoint, string rightMatePoint, string view)
        {
            var leftMate = catalog?.ConnectionLayout.FindConnectionLayout(beamId, leftMatePoint, view);
            var rightMate = catalog?.ConnectionLayout.FindConnectionLayout(beamId, rightMatePoint, view);
            if (leftMate == null || rightMate == null)
            {
                return;   // a missing mate is a missing contract: draw nothing rather than guess
            }

            foreach (var support in instances.Where(instance =>
                         instance.Role == HeaderBlockRole.Beam
                         && string.Equals(instance.PieceId, beamId, StringComparison.OrdinalIgnoreCase)))
            {
                var mate = support.MirroredX
                    ? new Point2D(rightMate.LocalX, rightMate.LocalY)
                    : new Point2D(leftMate.LocalX, leftMate.LocalY);
                var contact = EditorPreviewSurface.LocalToWorld(support, mate);
                var topOnPostAxis = new Point2D(support.Insertion.X, contact.Y);

                surface.Line(
                    surface.Map(support.Insertion.X, support.Insertion.Y),
                    surface.Map(topOnPostAxis.X, topOnPostAxis.Y),
                    EditorPreviewPalette.Horizontal, 3.0);
                surface.Line(
                    surface.Map(topOnPostAxis.X, topOnPostAxis.Y),
                    surface.Map(contact.X, contact.Y),
                    EditorPreviewPalette.Horizontal, 2.0);
            }
        }

        /// <summary>
        /// Safety elements: a protector lateral runs along its LONGITUD, a desviador rises by it, anything else is a
        /// small square marker. The dynamic renderer's <c>DrawSafety</c>, unchanged.
        /// </summary>
        public static void Safety(
            EditorPreviewSurface surface, IEnumerable<HeaderBlockInstance> instances, RackCatalog catalog)
        {
            foreach (var piece in instances.Where(instance => instance.Role == HeaderBlockRole.Safety))
            {
                var element = catalog?.SafetyElements?.FirstOrDefault(entry => string.Equals(
                    entry?.Id, piece.PieceId, StringComparison.OrdinalIgnoreCase));
                var at = surface.Map(piece.Insertion.X, piece.Insertion.Y);

                if (SelectiveSafetyDefaults.IsType(element?.Type, SelectiveSafetyDefaults.LateralType)
                    && piece.DynamicParameters.TryGetValue(SelectiveRackDefaults.LengthParam, out var guardLength))
                {
                    var endX = piece.MirroredX ? piece.Insertion.X - guardLength : piece.Insertion.X + guardLength;
                    surface.Line(at, surface.Map(endX, piece.Insertion.Y), EditorPreviewPalette.Safety, 4.2);
                }
                else if (SelectiveSafetyDefaults.IsType(element?.Type, SelectiveSafetyDefaults.DesviadorType)
                         && piece.DynamicParameters.TryGetValue(SelectiveRackDefaults.LengthParam, out var deviatorLength))
                {
                    surface.Line(at, surface.Map(piece.Insertion.X, piece.Insertion.Y + deviatorLength), EditorPreviewPalette.Safety, 3.2);
                }
                else
                {
                    const double size = 9.0;
                    surface.Rectangle(at.X - size / 2.0, at.Y - size, size, size, EditorPreviewPalette.Safety, 2.0);
                }
            }
        }

        /// <summary>
        /// Flow beds: each rail along its LONGITUD from its IN mate, plus rollers, brakes and end stops as short
        /// perpendicular ticks. The dynamic renderer's <c>DrawFlowBeds</c>, unchanged.
        /// </summary>
        public static void FlowBeds(
            EditorPreviewSurface surface, IEnumerable<HeaderBlockInstance> instances, RackCatalog catalog)
        {
            var list = instances as IReadOnlyList<HeaderBlockInstance> ?? instances.ToList();
            var railMate = catalog?.ConnectionLayout.FindConnectionLayout(
                FlowBedDefaults.RailId, FlowBedDefaults.RailInOutMatePoint, FlowBedDefaults.View);
            if (railMate != null)
            {
                foreach (var rail in list.Where(instance => instance.Role == HeaderBlockRole.Rail))
                {
                    if (!rail.DynamicParameters.TryGetValue("LONGITUD", out var length))
                    {
                        continue;
                    }

                    var start = EditorPreviewSurface.LocalToWorld(rail, new Point2D(railMate.LocalX, railMate.LocalY));
                    var end = EditorPreviewSurface.LocalToWorld(rail, new Point2D(length, railMate.LocalY));
                    surface.Line(
                        surface.Map(start.X, start.Y), surface.Map(end.X, end.Y),
                        EditorPreviewPalette.FlowBed, 3.2);
                }
            }

            foreach (var piece in list.Where(instance =>
                         instance.Role == HeaderBlockRole.Roller
                         || instance.Role == HeaderBlockRole.Brake
                         || instance.Role == HeaderBlockRole.Stop))
            {
                var center = surface.Map(piece.Insertion.X, piece.Insertion.Y);
                var half = piece.Role == HeaderBlockRole.Brake ? 4.5 : 3.0;
                var dx = -Math.Sin(piece.RotationRadians) * half;
                var dy = -Math.Cos(piece.RotationRadians) * half;
                var stroke = piece.Role == HeaderBlockRole.Brake
                    ? EditorPreviewPalette.Reinforcement
                    : piece.Role == HeaderBlockRole.Stop ? EditorPreviewPalette.Selection : EditorPreviewPalette.FlowBed;
                surface.Line(
                    new System.Windows.Point(center.X - dx, center.Y - dy),
                    new System.Windows.Point(center.X + dx, center.Y + dy),
                    stroke,
                    piece.Role == HeaderBlockRole.Brake ? 2.2 : 1.2);
            }
        }

        // ---- Parts driven purely by a plan's ROLES (what an editor without a bespoke frame model can draw) ----

        /// <summary>
        /// The structural frame of a plan, by role: posts and base plates as vertical/oriented members, horizontals,
        /// closing horizontals and diagonals along their LONGITUD, and separators. Each piece is drawn from its resolved
        /// insertion and its own LONGITUD/PERALTE parameters — never recomputed.
        /// </summary>
        public static void Structure(EditorPreviewSurface surface, IEnumerable<HeaderBlockInstance> instances, double postHeight)
        {
            foreach (var piece in instances)
            {
                switch (piece.Role)
                {
                    case HeaderBlockRole.Post:
                    {
                        // The post's own height, and — when the piece carries its PERALTE — its section width, so the
                        // upright reads as a member instead of a hairline. Both come from the resolved instance.
                        var tall = Height(piece, postHeight);
                        surface.WorldLine(
                            piece.Insertion.X, piece.Insertion.Y,
                            piece.Insertion.X, piece.Insertion.Y + tall,
                            EditorPreviewPalette.Post, 3.0);
                        if (piece.DynamicParameters.TryGetValue(SelectiveRackDefaults.PeralteParam, out var section) && section > 0.0)
                        {
                            var sign = piece.MirroredX ? -1.0 : 1.0;
                            surface.WorldLine(
                                piece.Insertion.X + sign * section, piece.Insertion.Y,
                                piece.Insertion.X + sign * section, piece.Insertion.Y + tall,
                                EditorPreviewPalette.Upright, 1.4);
                        }

                        break;
                    }

                    case HeaderBlockRole.BasePlate:
                        DrawSpan(surface, piece, EditorPreviewPalette.PlateFill, 4.0, fallbackLength: 6.0);
                        break;

                    case HeaderBlockRole.Horizontal:
                    case HeaderBlockRole.ClosingHorizontal:
                        DrawSpan(surface, piece, EditorPreviewPalette.Horizontal, 2.0, fallbackLength: 0.0);
                        break;

                    case HeaderBlockRole.Diagonal:
                        DrawSpan(surface, piece, EditorPreviewPalette.Diagonal, 1.6, fallbackLength: 0.0);
                        break;

                    case HeaderBlockRole.Separator:
                        DrawSpan(surface, piece, EditorPreviewPalette.Separator, 2.4, fallbackLength: 0.0);
                        break;
                }
            }
        }

        /// <summary>Load beams of ANY piece id, as their transverse LONGITUD span (used by the frontal/planta views).</summary>
        public static void BeamSpans(EditorPreviewSurface surface, IEnumerable<HeaderBlockInstance> instances, double thickness = 3.4)
        {
            foreach (var beam in instances.Where(instance => instance.Role == HeaderBlockRole.Beam))
            {
                DrawSpan(surface, beam, EditorPreviewPalette.LoadBeam, thickness, fallbackLength: 0.0);

                // Its PERALTE at the span's start, so the beam shows its section instead of a bare line. Read from the
                // instance — never recomputed.
                if (beam.DynamicParameters.TryGetValue(SelectiveRackDefaults.PeralteParam, out var peralte) && peralte > 0.0)
                {
                    surface.WorldLine(
                        beam.Insertion.X, beam.Insertion.Y,
                        beam.Insertion.X, beam.Insertion.Y + peralte,
                        EditorPreviewPalette.LoadBeam, 1.6);
                }
            }
        }

        /// <summary>Rear stops: their SAQUE stick-out plus, when present, their LONGITUD run along the beam.</summary>
        public static void Topes(EditorPreviewSurface surface, IEnumerable<HeaderBlockInstance> instances)
        {
            foreach (var tope in instances.Where(instance => instance.Role == HeaderBlockRole.Tope))
            {
                var sign = tope.MirroredX ? -1.0 : 1.0;
                if (tope.DynamicParameters.TryGetValue(SelectiveRackDefaults.LengthParam, out var longitud) && longitud > 0.0)
                {
                    surface.WorldLine(
                        tope.Insertion.X, tope.Insertion.Y,
                        tope.Insertion.X + sign * longitud, tope.Insertion.Y,
                        EditorPreviewPalette.Selection, 2.6);
                }

                var saque = tope.DynamicParameters.TryGetValue(SelectiveSafetyDefaults.SaqueParam, out var value) && value > 0.0
                    ? value
                    : 3.0;
                surface.WorldLine(
                    tope.Insertion.X, tope.Insertion.Y,
                    tope.Insertion.X, tope.Insertion.Y + saque,
                    EditorPreviewPalette.Selection, 3.0);
            }
        }

        /// <summary>Reference pallets: a dashed box per pallet, never part of the BOM.</summary>
        public static void Pallets(
            EditorPreviewSurface surface, IEnumerable<(double X, double Y, double Width, double Height)> boxes)
        {
            foreach (var box in boxes)
            {
                surface.WorldRectangle(
                    box.X, box.Y, box.X + box.Width, box.Y + box.Height,
                    EditorPreviewPalette.Pallet, 1.0, EditorPreviewSurface.Dash());
            }
        }

        /// <summary>The floor line across the drawn extent.</summary>
        public static void Floor(EditorPreviewSurface surface, double startX, double endX, double y = 0.0)
            => surface.WorldLine(startX, y, endX, y, EditorPreviewPalette.Floor, 1.4);

        private static double Height(HeaderBlockInstance piece, double fallback)
            => piece.DynamicParameters.TryGetValue("ALTURA", out var value) && value > 0.0 ? value : fallback;

        /// <summary>Draws a piece along its LONGITUD in its own orientation (mirror + rotation), from its insertion.</summary>
        private static void DrawSpan(
            EditorPreviewSurface surface, HeaderBlockInstance piece,
            System.Windows.Media.Brush stroke, double thickness, double fallbackLength)
        {
            var length = piece.DynamicParameters.TryGetValue(SelectiveRackDefaults.LengthParam, out var value) && value > 0.0
                ? value
                : fallbackLength;
            if (length <= 0.0)
            {
                return;
            }

            var end = EditorPreviewSurface.LocalToWorld(piece, new Point2D(length, 0.0));
            surface.Line(
                surface.Map(piece.Insertion.X, piece.Insertion.Y),
                surface.Map(end.X, end.Y),
                stroke, thickness);
        }
    }
}
