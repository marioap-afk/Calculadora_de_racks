using System.Collections.Generic;
using System.Linq;
using RackCad.Domain.RackFrames;

namespace RackCad.UI
{
    internal sealed class RackFrameEngineeringPreviewLayout
    {
        private RackFrameEngineeringPreviewLayout()
        {
            Segments = new List<RackFrameEngineeringPreviewSegment>();
            HorizontalMembers = new List<FrameMember>();
        }

        public bool HasValidGeometry { get; private set; }
        public double Width { get; private set; }
        public double Height { get; private set; }
        public double TopY { get; private set; }
        public double BottomY { get; private set; }
        public double LeftX { get; private set; }
        public double RightX { get; private set; }
        public double FrameWidth => RightX - LeftX;
        public double BaseVisualLift { get; private set; }
        public double ConfiguredHeight { get; private set; }
        public double TargetHeight { get; private set; }
        public double Depth { get; private set; }
        public RackFrameEngineeringPreviewPost LeftPost { get; private set; }
        public RackFrameEngineeringPreviewPost RightPost { get; private set; }
        public RackFrameEngineeringPreviewPlate LeftPlate { get; private set; }
        public RackFrameEngineeringPreviewPlate RightPlate { get; private set; }
        public IList<RackFrameEngineeringPreviewSegment> Segments { get; private set; }
        public IList<FrameMember> HorizontalMembers { get; private set; }

        public static RackFrameEngineeringPreviewLayout Create(RackFrameConfiguratorViewModel viewModel, double width, double height)
        {
            var layout = new RackFrameEngineeringPreviewLayout
            {
                Width = width,
                Height = height
            };

            if (viewModel == null || width < 120.0 || height < 80.0)
            {
                return layout;
            }

            var configuredHeight = System.Math.Max(System.Math.Max(viewModel.ConfiguredHeight, viewModel.Height), 1.0);
            var compactWidth = width < 340.0;
            var topY = compactWidth ? 62.0 : 66.0;
            var bottomY = height - (compactWidth ? 88.0 : 92.0);
            var usableHeight = bottomY - topY;

            if (usableHeight < 80.0)
            {
                return layout;
            }

            var leftMargin = compactWidth ? 64.0 : 76.0;
            var labelReserve = compactWidth ? 86.0 : 128.0;
            var depthRatio = System.Math.Max(viewModel.Depth, 1.0) / configuredHeight;
            var maxFrameWidth = System.Math.Max(48.0, width - leftMargin - labelReserve);
            var minFrameWidth = System.Math.Min(58.0, maxFrameWidth);
            var frameWidth = System.Math.Clamp(usableHeight * depthRatio, minFrameWidth, System.Math.Min(165.0, maxFrameWidth));
            var leftX = leftMargin + System.Math.Max(0.0, (maxFrameWidth - frameWidth) * 0.38);
            var rightX = leftX + frameWidth;

            layout.HasValidGeometry = true;
            layout.TopY = topY;
            layout.BottomY = bottomY;
            layout.LeftX = leftX;
            layout.RightX = rightX;
            layout.BaseVisualLift = System.Math.Clamp(usableHeight * 0.035, 8.0, 18.0);
            layout.ConfiguredHeight = configuredHeight;
            layout.TargetHeight = viewModel.Height;
            layout.Depth = viewModel.Depth;
            layout.LeftPost = new RackFrameEngineeringPreviewPost(
                leftX,
                topY,
                bottomY,
                viewModel.LeftPostCatalogId,
                viewModel.LeftPostHasReinforcement,
                viewModel.LeftPostReinforcementCatalogId,
                true);
            layout.RightPost = new RackFrameEngineeringPreviewPost(
                rightX,
                topY,
                bottomY,
                viewModel.RightPostCatalogId,
                viewModel.RightPostHasReinforcement,
                viewModel.RightPostReinforcementCatalogId,
                false);
            layout.LeftPlate = new RackFrameEngineeringPreviewPlate(
                leftX,
                bottomY,
                viewModel.LeftPlateCatalogId,
                viewModel.LeftPlateConnectionPointId);
            layout.RightPlate = new RackFrameEngineeringPreviewPlate(
                rightX,
                bottomY,
                viewModel.RightPlateCatalogId,
                viewModel.RightPlateConnectionPointId);
            layout.HorizontalMembers = viewModel.Configuration.Members
                .Where(IsHorizontalMember)
                .ToList();

            foreach (var panel in viewModel.Configuration.BracingPanels.OrderBy(item => item.Index))
            {
                var sourceSegment = viewModel.BracingSegments.FirstOrDefault(item => item.Index == panel.Index);
                var startY = layout.ToPanelBoundaryY(panel.StartElevation);
                var endY = layout.ToPanelBoundaryY(panel.EndElevation);
                var top = System.Math.Min(startY, endY);
                var bottom = System.Math.Max(startY, endY);

                layout.Segments.Add(new RackFrameEngineeringPreviewSegment(
                    sourceSegment,
                    panel,
                    panel.Members,
                    panel.Index,
                    panel.StartElevation,
                    panel.EndElevation,
                    panel.ClearHeight,
                    panel.Arrangement,
                    panel.MountingFace,
                    panel.DiagonalProfileId,
                    panel.StartConnectionPointId,
                    panel.EndConnectionPointId,
                    startY,
                    endY,
                    top,
                    bottom,
                    (top + bottom) / 2.0,
                    sourceSegment != null && viewModel.SelectedBracingSegments.Contains(sourceSegment),
                    sourceSegment != null ? sourceSegment.IsModified : panel.IsException));
            }

            return layout;
        }

        public double ToY(double elevation)
        {
            return BottomY - (elevation / ConfiguredHeight * (BottomY - TopY));
        }

        public double ToPanelBoundaryY(double elevation)
        {
            return ToY(elevation) - GetBaseVisualLift(elevation);
        }

        public double ToMemberY(double elevation)
        {
            return ToPanelBoundaryY(elevation);
        }

        private double GetBaseVisualLift(double elevation)
        {
            return elevation <= 0.001 ? BaseVisualLift : 0.0;
        }

        private static bool IsHorizontalMember(FrameMember member)
        {
            return member != null &&
                (member.MemberType == FrameMemberType.LowerHorizontal ||
                 member.MemberType == FrameMemberType.UpperHorizontal ||
                 member.MemberType == FrameMemberType.IntermediateHorizontal ||
                 member.MemberType == FrameMemberType.AdditionalHorizontal);
        }
    }

    internal sealed class RackFrameEngineeringPreviewSegment
    {
        public RackFrameEngineeringPreviewSegment(
            BracingSegmentEditorRow source,
            BracingPanel panel,
            IEnumerable<FrameMember> members,
            int index,
            double startElevation,
            double endElevation,
            double clearHeight,
            BracingPattern pattern,
            FrameSide sideMode,
            string braceProfileId,
            string startConnectionPointId,
            string endConnectionPointId,
            double startY,
            double endY,
            double top,
            double bottom,
            double middleY,
            bool isSelected,
            bool isModified)
        {
            Source = source;
            Panel = panel;
            Members = members == null ? new List<FrameMember>() : members.ToList();
            Index = index;
            StartElevation = startElevation;
            EndElevation = endElevation;
            ClearHeight = clearHeight;
            Pattern = pattern;
            SideMode = sideMode;
            BraceProfileId = braceProfileId ?? string.Empty;
            StartConnectionPointId = startConnectionPointId ?? string.Empty;
            EndConnectionPointId = endConnectionPointId ?? string.Empty;
            StartY = startY;
            EndY = endY;
            Top = top;
            Bottom = bottom;
            MiddleY = middleY;
            IsSelected = isSelected;
            IsModified = isModified;
        }

        public BracingSegmentEditorRow Source { get; private set; }
        public BracingPanel Panel { get; private set; }
        public IList<FrameMember> Members { get; private set; }
        public int Index { get; private set; }
        public double StartElevation { get; private set; }
        public double EndElevation { get; private set; }
        public double ClearHeight { get; private set; }
        public BracingPattern Pattern { get; private set; }
        public FrameSide SideMode { get; private set; }
        public string BraceProfileId { get; private set; }
        public string StartConnectionPointId { get; private set; }
        public string EndConnectionPointId { get; private set; }
        public double StartY { get; private set; }
        public double EndY { get; private set; }
        public double Top { get; private set; }
        public double Bottom { get; private set; }
        public double MiddleY { get; private set; }
        public bool IsSelected { get; private set; }
        public bool IsModified { get; private set; }
    }

    internal sealed class RackFrameEngineeringPreviewPost
    {
        public RackFrameEngineeringPreviewPost(
            double x,
            double topY,
            double bottomY,
            string catalogId,
            bool hasReinforcement,
            string reinforcementCatalogId,
            bool reinforcementOnRightSide)
        {
            X = x;
            TopY = topY;
            BottomY = bottomY;
            CatalogId = catalogId ?? string.Empty;
            HasReinforcement = hasReinforcement;
            ReinforcementCatalogId = reinforcementCatalogId ?? string.Empty;
            ReinforcementOnRightSide = reinforcementOnRightSide;
        }

        public double X { get; private set; }
        public double TopY { get; private set; }
        public double BottomY { get; private set; }
        public string CatalogId { get; private set; }
        public bool HasReinforcement { get; private set; }
        public string ReinforcementCatalogId { get; private set; }
        public bool ReinforcementOnRightSide { get; private set; }
    }

    internal sealed class RackFrameEngineeringPreviewPlate
    {
        public RackFrameEngineeringPreviewPlate(double x, double bottomY, string catalogId, string connectionPointId)
        {
            X = x;
            BottomY = bottomY;
            CatalogId = catalogId ?? string.Empty;
            ConnectionPointId = connectionPointId ?? string.Empty;
        }

        public double X { get; private set; }
        public double BottomY { get; private set; }
        public string CatalogId { get; private set; }
        public string ConnectionPointId { get; private set; }
    }
}
