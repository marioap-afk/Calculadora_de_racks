using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Headers;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>Pure frontal metrics consumed by the WPF preview. Keeping the post heights here prevents the
    /// schematic from drifting from the AutoCAD builder when adjacent fronts have different heights.</summary>
    public sealed class DynamicFrontalPreviewGeometry
    {
        public DynamicFrontalPreviewGeometry(DynamicFrontLayout layout, IReadOnlyList<double> postHeights)
        {
            Layout = layout;
            PostHeights = postHeights ?? Array.Empty<double>();
        }

        public DynamicFrontLayout Layout { get; }
        public IReadOnlyList<double> PostHeights { get; }
        public double Height => PostHeights.DefaultIfEmpty(0.0).Max();
    }

    /// <summary>Resolved lateral cut consumed by the preview: only the selected post's shared depth range and
    /// the exact block plan that AutoCAD receives belong to the section.</summary>
    public sealed class DynamicLateralPreviewGeometry
    {
        public DynamicLateralPreviewGeometry(
            int postIndex,
            DynamicDepthRange range,
            IReadOnlyList<DynamicRackModule> modules,
            double startX,
            double endX,
            double height,
            int loadLevels,
            DynamicSystemPlan plan)
        {
            PostIndex = postIndex;
            Range = range;
            Modules = modules ?? Array.Empty<DynamicRackModule>();
            StartX = startX;
            EndX = endX;
            Height = height;
            LoadLevels = loadLevels;
            Plan = plan;
        }

        public int PostIndex { get; }
        public DynamicDepthRange Range { get; }
        public IReadOnlyList<DynamicRackModule> Modules { get; }
        public double StartX { get; }
        public double EndX { get; }
        public double Length => Math.Max(0.0, EndX - StartX);
        public double Height { get; }
        public int LoadLevels { get; }
        public DynamicSystemPlan Plan { get; }
    }

    public static class DynamicSystemPreviewGeometry
    {
        public static DynamicFrontalPreviewGeometry Frontal(DynamicRackSystem system, RackCatalog catalog)
        {
            var layout = DynamicFrontGeometry.Compute(system, catalog);
            var heights = Enumerable.Range(0, layout.PostPositions.Count)
                .Select(index => DynamicFrontGeometry.PostHeight(system, index))
                .ToList();
            return new DynamicFrontalPreviewGeometry(layout, heights);
        }

        public static DynamicLateralPreviewGeometry Lateral(
            DynamicRackSystem system,
            RackCatalog catalog,
            int postIndex)
        {
            if (system == null || postIndex < 0 || postIndex > system.Fronts.Count)
            {
                return new DynamicLateralPreviewGeometry(
                    postIndex,
                    new DynamicDepthRange(1, 0),
                    Array.Empty<DynamicRackModule>(),
                    0.0,
                    0.0,
                    0.0,
                    0,
                    new DynamicSystemPlan(Array.Empty<HeaderGroup>(), Array.Empty<HeaderBlockInstance>()));
            }

            var range = DynamicDepthGeometry.AtPost(system, postIndex);
            var modules = DynamicDepthGeometry.ModulesInRange(system, range);
            var startX = modules.FirstOrDefault()?.StartX ?? 0.0;
            var endX = modules.LastOrDefault()?.EndX ?? startX;
            return new DynamicLateralPreviewGeometry(
                postIndex,
                range,
                modules,
                startX,
                endX,
                DynamicFrontGeometry.PostHeight(system, postIndex),
                DynamicFrontGeometry.LoadLevelsAtPost(system, postIndex),
                new DynamicSystemLateralBuilder().Build(system, catalog, postIndex));
        }
    }
}
