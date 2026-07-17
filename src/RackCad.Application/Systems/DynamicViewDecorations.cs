using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Geometry;
using RackCad.Application.Headers;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// Text and linear dimensions shared by the dynamic rack's linked views. The physical builders stay responsible
    /// only for members; this class owns the annotation offsets so frontal/planta/lateral cannot drift apart.
    /// </summary>
    internal static class DynamicViewDecorations
    {
        private const double ChainGap = 14.0;
        private const double OverallGap = 22.0;
        private const double ElevationStep = 14.0;

        public static void AppendFrontal(
            ICollection<HeaderBlockInstance> target,
            DynamicRackSystem system,
            DynamicFrontLayout layout,
            DynamicRackEnd end,
            RackCatalog catalog)
        {
            if (target == null || system == null || layout?.PostPositions == null || layout.PostPositions.Count < 2)
            {
                return;
            }

            const string view = "FRONTAL";
            var scale = Scale(system);
            var textHeight = SelectiveAnnotations.TextHeightFor(scale);
            var height = DynamicFrontGeometry.Height(system);
            var levelYs = system.LoadBeamLevels
                .Select(level => end == DynamicRackEnd.Entrance ? level.EntranceElevation : level.ExitElevation)
                .ToList();

            AppendFrontalDimensions(target, system, layout, view, height, levelYs, textHeight, scale, catalog);

            var leftReach = FrontalLeftReach(system, levelYs.Count, scale);
            var bottomReach = BottomReach(system, scale);
            var labelGap = textHeight + SelectiveAnnotations.Margin;
            if (system.NumberFronts)
            {
                for (var i = 0; i < system.Fronts.Count && i + 1 < layout.PostPositions.Count; i++)
                {
                    var center = (layout.PostPositions[i] + layout.PostPositions[i + 1]) / 2.0;
                    target.Add(SelectiveAnnotations.Label(
                        SelectiveAnnotations.Num(i + 1),
                        view,
                        new Point2D(center, -bottomReach - labelGap),
                        textHeight));
                }
            }

            if (system.NumberLevels)
            {
                for (var i = 0; i < levelYs.Count; i++)
                {
                    target.Add(SelectiveAnnotations.Label(
                        SelectiveAnnotations.Num(i + 1),
                        view,
                        new Point2D(layout.PostPositions[0] - leftReach - labelGap, levelYs[i]),
                        textHeight));
                }
            }

            if (system.DrawRackName && !string.IsNullOrWhiteSpace(system.Name))
            {
                target.Add(SelectiveAnnotations.Label(
                    system.Name.Trim(),
                    view,
                    new Point2D(layout.PostPositions[0], height + labelGap),
                    textHeight * 1.5));
            }
        }

        public static void AppendPlanta(
            ICollection<HeaderBlockInstance> target,
            DynamicRackSystem system,
            DynamicFrontLayout layout)
        {
            if (target == null || system == null || layout?.PostPositions == null || layout.PostPositions.Count < 2)
            {
                return;
            }

            const string view = "PLANTA";
            var scale = Scale(system);
            var textHeight = SelectiveAnnotations.TextHeightFor(scale);
            var near = ChainGap * scale;
            var far = (ChainGap + OverallGap) * scale;
            var style = system.DimensionStyle;

            if (system.Dimensions != DimensionDetail.None)
            {
                var depthOffset = system.Dimensions == DimensionDetail.Minimal ? -near : -far;
                AddHorizontal(target, view, 0.0, system.TotalLength, layout.PostPositions[0], depthOffset, textHeight, style);

                var frontCount = system.Dimensions == DimensionDetail.Minimal ? Math.Min(1, system.Fronts.Count) : system.Fronts.Count;
                for (var i = 0; i < frontCount && i + 1 < layout.PostPositions.Count; i++)
                {
                    var pitch = layout.PostPositions[i + 1] - layout.PostPositions[i];
                    var start = layout.PostPositions[i] + (pitch - system.Fronts[i].BeamLength) / 2.0;
                    AddVertical(target, view, 0.0, start, start + system.Fronts[i].BeamLength, -near, textHeight, style);
                }

                if (system.Dimensions >= DimensionDetail.Standard)
                {
                    foreach (var module in system.Modules.Where(module => module.Length > 0.0))
                    {
                        AddHorizontal(target, view, module.StartX, module.EndX, layout.PostPositions[0], -near, textHeight, style);
                    }
                }
            }

            var labelGap = textHeight + SelectiveAnnotations.Margin;
            var leftReach = system.Dimensions == DimensionDetail.None ? 0.0 : near;
            if (system.NumberFronts)
            {
                for (var i = 0; i < system.Fronts.Count && i + 1 < layout.PostPositions.Count; i++)
                {
                    var center = (layout.PostPositions[i] + layout.PostPositions[i + 1]) / 2.0;
                    target.Add(SelectiveAnnotations.Label(
                        SelectiveAnnotations.Num(i + 1),
                        view,
                        new Point2D(-leftReach - labelGap, center),
                        textHeight));
                }
            }

            if (system.DrawRackName && !string.IsNullOrWhiteSpace(system.Name))
            {
                target.Add(SelectiveAnnotations.Label(
                    system.Name.Trim(),
                    view,
                    new Point2D(0.0, layout.TotalWidth + labelGap),
                    textHeight * 1.5));
            }
        }

        public static void AppendLateral(
            ICollection<HeaderBlockInstance> target,
            DynamicRackSystem system,
            double? sectionHeight = null,
            int levelCount = int.MaxValue,
            double sectionStartX = 0.0,
            double? sectionEndX = null)
        {
            if (target == null || system == null || system.TotalLength <= 0.0)
            {
                return;
            }

            const string view = "LATERAL";
            var scale = Scale(system);
            var textHeight = SelectiveAnnotations.TextHeightFor(scale);
            var near = ChainGap * scale;
            var far = (ChainGap + OverallGap) * scale;
            var height = sectionHeight ?? DynamicFrontGeometry.Height(system);
            var levelYs = system.LoadBeamLevels
                .Take(Math.Min(levelCount, system.LoadBeamLevels.Count))
                .Select(level => level.ExitElevation)
                .ToList();
            var style = system.DimensionStyle;
            var endX = sectionEndX ?? system.TotalLength;

            if (system.Dimensions != DimensionDetail.None && height > 0.0)
            {
                var outer = system.Dimensions == DimensionDetail.Minimal ? near : far;
                AddHorizontal(target, view, sectionStartX, endX, 0.0, -outer, textHeight, style);
                AddVertical(target, view, sectionStartX, 0.0, height, -outer, textHeight, style);

                if (system.Dimensions >= DimensionDetail.Standard)
                {
                    foreach (var module in system.Modules.Where(module => module.Length > 0.0
                                 && module.StartX >= sectionStartX - 1e-6
                                 && module.EndX <= endX + 1e-6))
                    {
                        AddHorizontal(target, view, module.StartX, module.EndX, 0.0, -near, textHeight, style);
                    }

                    var previous = 0.0;
                    foreach (var y in levelYs)
                    {
                        AddVertical(target, view, sectionStartX, previous, y, -near, textHeight, style);
                        previous = y;
                    }
                }

                if (system.Dimensions == DimensionDetail.Detailed)
                {
                    for (var i = 0; i < levelYs.Count; i++)
                    {
                        AddVertical(target, view, sectionStartX, 0.0, levelYs[i],
                            -(far + (i + 1) * ElevationStep * scale), textHeight, style);
                    }
                }
            }

            var leftReach = FrontalLeftReach(system, levelYs.Count, scale);
            var labelGap = textHeight + SelectiveAnnotations.Margin;
            if (system.NumberLevels)
            {
                for (var i = 0; i < levelYs.Count; i++)
                {
                    target.Add(SelectiveAnnotations.Label(
                        SelectiveAnnotations.Num(i + 1),
                        view,
                        new Point2D(sectionStartX - leftReach - labelGap, levelYs[i]),
                        textHeight));
                }
            }

            if (system.DrawRackName && !string.IsNullOrWhiteSpace(system.Name))
            {
                target.Add(SelectiveAnnotations.Label(
                    system.Name.Trim(),
                    view,
                    new Point2D(sectionStartX, height + labelGap),
                    textHeight * 1.5));
            }
        }

        private static void AppendFrontalDimensions(
            ICollection<HeaderBlockInstance> target,
            DynamicRackSystem system,
            DynamicFrontLayout layout,
            string view,
            double height,
            IReadOnlyList<double> levelYs,
            double textHeight,
            double scale,
            RackCatalog catalog)
        {
            if (system.Dimensions == DimensionDetail.None || height <= 0.0)
            {
                return;
            }

            var near = ChainGap * scale;
            var far = (ChainGap + OverallGap) * scale;
            var style = system.DimensionStyle;
            var outer = system.Dimensions == DimensionDetail.Minimal ? near : far;
            AddHorizontal(target, view, layout.PostPositions[0], layout.PostPositions[layout.PostPositions.Count - 1],
                0.0, -outer, textHeight, style);
            AddVertical(target, view, layout.PostPositions[0], 0.0, height, -outer, textHeight, style);

            if (system.Dimensions >= DimensionDetail.Standard)
            {
                for (var i = 0; i < system.Fronts.Count && i < layout.TroquelPositions.Count; i++)
                {
                    var configuration = DynamicRackLevelGeometry.Envelope(system, system.Fronts[i]);
                    var frontProfileStart = SelectivePostGeometry.Resolve(
                        catalog?.ConnectionLayout.FindConnectionLayout(
                            configuration.InOutBeamCatalogId,
                            SelectiveRackDefaults.BeamProfileStartPoint,
                            view),
                        new Dictionary<string, double>
                        {
                            [SelectiveRackDefaults.PeralteParam] = configuration.InOutBeamDepth
                        }).X;
                    var start = layout.PostPositions[i] + layout.TroquelPositions[i] + frontProfileStart;
                    AddHorizontal(target, view, start, start + system.Fronts[i].BeamLength,
                        0.0, -near, textHeight, style);
                }

                var previous = 0.0;
                foreach (var y in levelYs)
                {
                    AddVertical(target, view, layout.PostPositions[0], previous, y, -near, textHeight, style);
                    previous = y;
                }
            }

            if (system.Dimensions == DimensionDetail.Detailed)
            {
                for (var i = 0; i < levelYs.Count; i++)
                {
                    AddVertical(target, view, layout.PostPositions[0], 0.0, levelYs[i],
                        -(far + (i + 1) * ElevationStep * scale), textHeight, style);
                }
            }
        }

        private static double Scale(DynamicRackSystem system)
            => system.AnnotationScale > 0.0 ? system.AnnotationScale : 1.0;

        private static double BottomReach(DynamicRackSystem system, double scale)
            => system.Dimensions == DimensionDetail.None
                ? 0.0
                : system.Dimensions == DimensionDetail.Minimal
                    ? ChainGap * scale
                    : (ChainGap + OverallGap) * scale;

        private static double FrontalLeftReach(DynamicRackSystem system, int levels, double scale)
        {
            if (system.Dimensions == DimensionDetail.None)
            {
                return 0.0;
            }

            var reach = system.Dimensions == DimensionDetail.Minimal
                ? ChainGap * scale
                : (ChainGap + OverallGap) * scale;
            return system.Dimensions == DimensionDetail.Detailed
                ? reach + levels * ElevationStep * scale
                : reach;
        }

        private static void AddHorizontal(
            ICollection<HeaderBlockInstance> target,
            string view,
            double x1,
            double x2,
            double y,
            double offset,
            double height,
            string style)
        {
            if (Math.Abs(x2 - x1) > 1e-6)
            {
                target.Add(Dimension(view, new Point2D(x1, y), new Point2D(x2, y), offset, height, style));
            }
        }

        private static void AddVertical(
            ICollection<HeaderBlockInstance> target,
            string view,
            double x,
            double y1,
            double y2,
            double offset,
            double height,
            string style)
        {
            if (Math.Abs(y2 - y1) > 1e-6)
            {
                target.Add(Dimension(view, new Point2D(x, y1), new Point2D(x, y2), offset, height, style));
            }
        }

        private static HeaderBlockInstance Dimension(
            string view,
            Point2D p1,
            Point2D p2,
            double offset,
            double height,
            string style)
            => new HeaderBlockInstance
            {
                Role = HeaderBlockRole.Dimension,
                View = view,
                Insertion = p1,
                ConnectionAnchor = p2,
                DimensionOffset = offset,
                TextHeight = height,
                DimensionStyleName = style
            };
    }
}
