using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Headers;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;

namespace RackCad.UI.Preview
{
    /// <summary>
    /// The Push Back preview, drawn with the SHARED infrastructure (I-18b, decision 6). It owns no primitives, no palette
    /// and no transform of its own: it fits the plan's world extent onto an <see cref="EditorPreviewSurface"/> and then
    /// asks <see cref="EditorPreviewParts"/> — the very parts the dynamic editor draws with — to paint each family.
    ///
    /// It CONSUMES resolved geometry: the plan already carries the slope, the snap, the mates, the lengths, the SAQUE and
    /// the peraltes, so nothing here recomputes them and nothing here mutates the plan.
    ///
    /// What Push Back contributes is only what is specific to it: which plan belongs to which view, and which families a
    /// given view may show (the entrance/exit cut carries no rear stop; the rear cut carries no ordinary low-end safety).
    /// </summary>
    internal static class PushBackPreviewRenderer
    {
        /// <summary>Canvas margin around the fitted drawing.</summary>
        private const double Margin = 16.0;

        /// <summary>Draws <paramref name="plan"/> for <paramref name="view"/>; returns false when there is nothing drawable.</summary>
        public static bool Draw(
            EditorPreviewSurface surface,
            DynamicSystemPlan plan,
            RackCatalog catalog,
            string view,
            double lowBeamDepth,
            string lowBeamId,
            string rearBeamId,
            IEnumerable<(double X, double Y, double Width, double Height)> pallets = null)
        {
            surface.Clear();
            var instances = Flatten(plan);
            if (instances.Count == 0)
            {
                return false;
            }

            if (!FitTo(surface, instances))
            {
                return false;
            }

            var isPlanta = string.Equals(view, "PLANTA", StringComparison.OrdinalIgnoreCase);
            var isLateral = string.Equals(view, "LATERAL", StringComparison.OrdinalIgnoreCase);

            // 1. The frame: posts, base plates, horizontals, closing horizontals, diagonals and separators, each from its
            //    own resolved insertion and LONGITUD.
            EditorPreviewParts.Structure(surface, instances, PostHeight(instances));

            // 2. Load beams. Seen ALONG the depth (lateral) the beam shows its peralte on its own axis, exactly as the
            //    dynamic editor draws it; seen ACROSS (frontal/planta) it shows its transverse LONGITUD span.
            if (isLateral)
            {
                EditorPreviewParts.LoadBeams(surface, instances, lowBeamId, lowBeamDepth);
                EditorPreviewParts.LoadBeams(surface, instances, rearBeamId, lowBeamDepth);
                EditorPreviewParts.IntermediateSupports(
                    surface, instances, catalog,
                    DynamicRackDefaults.IntermediateBeamCatalogId,
                    DynamicRackDefaults.IntermediateBeamLeftBedMatePoint,
                    DynamicRackDefaults.IntermediateBeamRightBedMatePoint,
                    DynamicRackDefaults.IntermediateBeamView);
            }
            else
            {
                EditorPreviewParts.BeamSpans(surface, instances);
            }

            // 3. The bed: rails along their LONGITUD plus rollers, brakes and end stops.
            EditorPreviewParts.FlowBeds(surface, instances, catalog);

            // 4. The rear stops and the safety elements the plan actually carries. Push Back's own composition already
            //    decides which of them belong to each cut, so the renderer draws what it is given and invents nothing.
            EditorPreviewParts.Topes(surface, instances);
            EditorPreviewParts.Safety(surface, instances, catalog);

            // 5. Reference pallets (never in the BOM), and the floor line in the elevations.
            if (pallets != null)
            {
                EditorPreviewParts.Pallets(surface, pallets);
            }

            if (!isPlanta)
            {
                var xs = instances.Select(instance => instance.Insertion.X).ToList();
                EditorPreviewParts.Floor(surface, xs.Min(), xs.Max());
            }

            return true;
        }

        /// <summary>Every instance of a plan, grouped or loose, in draw order. Never mutates the plan.</summary>
        public static IReadOnlyList<HeaderBlockInstance> Flatten(DynamicSystemPlan plan)
        {
            if (plan == null)
            {
                return Array.Empty<HeaderBlockInstance>();
            }

            var flattened = plan.Flatten();
            return flattened?.Instances ?? (IReadOnlyList<HeaderBlockInstance>)Array.Empty<HeaderBlockInstance>();
        }

        /// <summary>
        /// Fits the plan's WORLD extent onto the surface. The extent spans every instance's insertion plus the reach of
        /// its own LONGITUD, so a view whose pieces run transversely (the frontals and the planta) gets real extent on
        /// both axes instead of collapsing onto a line.
        /// </summary>
        private static bool FitTo(EditorPreviewSurface surface, IReadOnlyList<HeaderBlockInstance> instances)
        {
            var minX = double.MaxValue;
            var maxX = double.MinValue;
            var minY = double.MaxValue;
            var maxY = double.MinValue;

            void Include(double x, double y)
            {
                minX = Math.Min(minX, x);
                maxX = Math.Max(maxX, x);
                minY = Math.Min(minY, y);
                maxY = Math.Max(maxY, y);
            }

            foreach (var instance in instances)
            {
                Include(instance.Insertion.X, instance.Insertion.Y);

                if (instance.DynamicParameters.TryGetValue(SelectiveRackDefaults.LengthParam, out var length) && length > 0.0)
                {
                    var end = EditorPreviewSurface.LocalToWorld(instance, new Application.Geometry.Point2D(length, 0.0));
                    Include(end.X, end.Y);
                }

                if (instance.DynamicParameters.TryGetValue(SelectiveRackDefaults.PeralteParam, out var peralte) && peralte > 0.0)
                {
                    Include(instance.Insertion.X, instance.Insertion.Y + peralte);
                }

                if (instance.DynamicParameters.TryGetValue("ALTURA", out var altura) && altura > 0.0)
                {
                    Include(instance.Insertion.X, instance.Insertion.Y + altura);
                }
            }

            if (minX > maxX || minY > maxY)
            {
                return false;
            }

            // A degenerate axis (everything on one line) still deserves a visible band rather than a division by zero.
            var width = Math.Max(maxX - minX, 1e-6);
            var height = Math.Max(maxY - minY, 1e-6);
            return surface.Fit(minX, width, height, Margin, Margin, Margin);
        }

        /// <summary>Fallback post height when a post carries no ALTURA: the tallest thing the plan reaches.</summary>
        private static double PostHeight(IReadOnlyList<HeaderBlockInstance> instances)
        {
            var top = instances.Select(instance => instance.Insertion.Y).DefaultIfEmpty(0.0).Max();
            var bottom = instances.Select(instance => instance.Insertion.Y).DefaultIfEmpty(0.0).Min();
            return Math.Max(1.0, top - bottom);
        }
    }
}
