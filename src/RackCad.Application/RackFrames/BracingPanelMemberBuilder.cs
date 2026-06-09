using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Domain.RackFrames;

namespace RackCad.Application.RackFrames
{
    public sealed class BracingPanelMemberBuilder
    {
        public void RefreshPhysicalModel(RackFrameConfiguration configuration)
        {
            if (configuration == null)
            {
                return;
            }

            ResolvePanelElevations(configuration);

            var members = BuildHorizontalMembers(configuration.Horizontals, configuration.Depth)
                .Concat(BuildPanelDiagonalMembers(configuration.BracingPanels, configuration.Depth))
                .ToList();

            configuration.Members.Clear();

            foreach (var panel in configuration.BracingPanels)
            {
                panel.Members.Clear();
            }

            foreach (var member in members)
            {
                configuration.Members.Add(member);

                if (!IsHorizontalMember(member))
                {
                    var panel = configuration.BracingPanels.FirstOrDefault(item => item.PanelId == member.SourcePanelId);

                    if (panel != null)
                    {
                        panel.Members.Add(member);
                    }
                }
            }
        }

        private static void ResolvePanelElevations(RackFrameConfiguration configuration)
        {
            var horizontals = configuration.Horizontals.ToDictionary(horizontal => horizontal.Id ?? string.Empty);

            foreach (var panel in configuration.BracingPanels)
            {
                if (!horizontals.TryGetValue(panel.LowerHorizontalId ?? string.Empty, out var lowerHorizontal) ||
                    !horizontals.TryGetValue(panel.UpperHorizontalId ?? string.Empty, out var upperHorizontal))
                {
                    continue;
                }

                panel.StartElevation = Math.Min(lowerHorizontal.Elevation, upperHorizontal.Elevation);
                panel.EndElevation = Math.Max(lowerHorizontal.Elevation, upperHorizontal.Elevation);
            }
        }

        private static IEnumerable<FrameMember> BuildHorizontalMembers(IEnumerable<FrameHorizontal> horizontals, double frameDepth)
        {
            if (horizontals == null)
            {
                yield break;
            }

            foreach (var horizontal in horizontals.OrderBy(item => item.Elevation).ThenBy(item => item.Number))
            {
                yield return CreateHorizontalMember(horizontal, frameDepth);
            }
        }

        private static FrameMember CreateHorizontalMember(FrameHorizontal horizontal, double frameDepth)
        {
            var memberType = GetHorizontalMemberType(horizontal);
            var start = CreateEnd(FrameMemberEndRole.LeftUpright, PostSide.Left, horizontal.Elevation, horizontal.Id);
            var end = CreateEnd(FrameMemberEndRole.RightUpright, PostSide.Right, horizontal.Elevation, horizontal.Id);
            var member = CreateMember(
                "Horizontal-" + horizontal.Number.ToString(CultureInfo.InvariantCulture),
                horizontal.Number,
                memberType,
                horizontal.MountingFace,
                NormalizeText(horizontal.ProfileId),
                start,
                end,
                frameDepth,
                horizontal.State == FrameComponentState.Standard,
                horizontal.State == FrameComponentState.Standard ? FrameMemberOrigin.Standard : FrameMemberOrigin.Manual);

            member.Quantity = Math.Max(1, horizontal.Quantity);
            member.PositionRatio = 0.0;
            return member;
        }

        private static FrameMemberType GetHorizontalMemberType(FrameHorizontal horizontal)
        {
            if (horizontal.Number <= 1 || horizontal.Elevation <= 0.001)
            {
                return FrameMemberType.LowerHorizontal;
            }

            if (horizontal.ProfileId != null &&
                horizontal.ProfileId.IndexOf("SUPERIOR", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return FrameMemberType.UpperHorizontal;
            }

            return FrameMemberType.IntermediateHorizontal;
        }

        private static IEnumerable<FrameMember> BuildPanelDiagonalMembers(IEnumerable<BracingPanel> panels, double frameDepth)
        {
            if (panels == null)
            {
                yield break;
            }

            foreach (var panel in panels.OrderBy(item => item.Number))
            {
                if (panel.Arrangement == BracingPattern.NoBracing ||
                    panel.Arrangement == BracingPattern.Custom)
                {
                    continue;
                }

                foreach (var face in GetPhysicalFaces(panel.MountingFace))
                {
                    foreach (var member in BuildPanelDiagonalMembers(panel, face, frameDepth))
                    {
                        yield return member;
                    }
                }
            }
        }

        private static IEnumerable<FrameMember> BuildPanelDiagonalMembers(BracingPanel panel, FrameSide face, double frameDepth)
        {
            var bottomElevation = Math.Min(panel.StartElevation, panel.EndElevation);
            var topElevation = Math.Max(panel.StartElevation, panel.EndElevation);
            var middleElevation = (bottomElevation + topElevation) / 2.0;
            var direction = ResolveDirection(panel);

            if (panel.Arrangement == BracingPattern.SingleDiagonal)
            {
                yield return CreateDiagonal(panel, face, direction, bottomElevation, topElevation, frameDepth);
                yield break;
            }

            if (panel.Arrangement == BracingPattern.DoubleDiagonal)
            {
                foreach (var member in CreateDoubleDiagonal(panel, face, direction, bottomElevation, topElevation, frameDepth))
                {
                    yield return member;
                }

                yield break;
            }

            if (panel.Arrangement == BracingPattern.XBracing)
            {
                yield return CreateDiagonal(panel, face, DiagonalDirection.UpRight, bottomElevation, topElevation, frameDepth);
                yield return CreateDiagonal(panel, face, DiagonalDirection.UpLeft, bottomElevation, topElevation, frameDepth);
                yield break;
            }

            if (panel.Arrangement == BracingPattern.KBracing)
            {
                foreach (var member in CreateKBracing(panel, face, direction, bottomElevation, middleElevation, topElevation, frameDepth))
                {
                    yield return member;
                }
            }
        }

        private static DiagonalDirection ResolveDirection(BracingPanel panel)
        {
            if (panel.DiagonalDirection == DiagonalDirection.UpRight ||
                panel.DiagonalDirection == DiagonalDirection.UpLeft)
            {
                return panel.DiagonalDirection;
            }

            return panel.Number % 2 == 0 ? DiagonalDirection.UpLeft : DiagonalDirection.UpRight;
        }

        private static FrameMember CreateDiagonal(
            BracingPanel panel,
            FrameSide face,
            DiagonalDirection direction,
            double bottomElevation,
            double topElevation,
            double frameDepth)
        {
            if (direction == DiagonalDirection.UpLeft)
            {
                return CreateMember(
                    panel.PanelId,
                    panel.Number,
                    FrameMemberType.DiagonalBrace,
                    face,
                    panel.DiagonalProfileId,
                    CreateEnd(FrameMemberEndRole.RightUpright, PostSide.Right, bottomElevation, panel.StartConnectionPointId),
                    CreateEnd(FrameMemberEndRole.LeftUpright, PostSide.Left, topElevation, panel.EndConnectionPointId),
                    frameDepth,
                    panel.IsStandard,
                    panel.IsException ? FrameMemberOrigin.Exception : FrameMemberOrigin.Standard);
            }

            return CreateMember(
                panel.PanelId,
                panel.Number,
                FrameMemberType.DiagonalBrace,
                face,
                panel.DiagonalProfileId,
                CreateEnd(FrameMemberEndRole.LeftUpright, PostSide.Left, bottomElevation, panel.StartConnectionPointId),
                CreateEnd(FrameMemberEndRole.RightUpright, PostSide.Right, topElevation, panel.EndConnectionPointId),
                frameDepth,
                panel.IsStandard,
                panel.IsException ? FrameMemberOrigin.Exception : FrameMemberOrigin.Standard);
        }

        private static IEnumerable<FrameMember> CreateDoubleDiagonal(
            BracingPanel panel,
            FrameSide face,
            DiagonalDirection direction,
            double bottomElevation,
            double topElevation,
            double frameDepth)
        {
            if (direction == DiagonalDirection.UpLeft)
            {
                yield return CreateMember(panel.PanelId, panel.Number, FrameMemberType.DiagonalBrace, face, panel.DiagonalProfileId, CreateCustomEnd(1.0, bottomElevation, panel.StartConnectionPointId), CreateCustomEnd(0.14, topElevation, panel.EndConnectionPointId), frameDepth, panel.IsStandard, panel.IsException ? FrameMemberOrigin.Exception : FrameMemberOrigin.Standard);
                yield return CreateMember(panel.PanelId, panel.Number, FrameMemberType.DiagonalBrace, face, panel.DiagonalProfileId, CreateCustomEnd(0.86, bottomElevation, panel.StartConnectionPointId), CreateCustomEnd(0.0, topElevation, panel.EndConnectionPointId), frameDepth, panel.IsStandard, panel.IsException ? FrameMemberOrigin.Exception : FrameMemberOrigin.Standard);
                yield break;
            }

            yield return CreateMember(panel.PanelId, panel.Number, FrameMemberType.DiagonalBrace, face, panel.DiagonalProfileId, CreateCustomEnd(0.0, bottomElevation, panel.StartConnectionPointId), CreateCustomEnd(0.86, topElevation, panel.EndConnectionPointId), frameDepth, panel.IsStandard, panel.IsException ? FrameMemberOrigin.Exception : FrameMemberOrigin.Standard);
            yield return CreateMember(panel.PanelId, panel.Number, FrameMemberType.DiagonalBrace, face, panel.DiagonalProfileId, CreateCustomEnd(0.14, bottomElevation, panel.StartConnectionPointId), CreateCustomEnd(1.0, topElevation, panel.EndConnectionPointId), frameDepth, panel.IsStandard, panel.IsException ? FrameMemberOrigin.Exception : FrameMemberOrigin.Standard);
        }

        private static IEnumerable<FrameMember> CreateKBracing(
            BracingPanel panel,
            FrameSide face,
            DiagonalDirection direction,
            double bottomElevation,
            double middleElevation,
            double topElevation,
            double frameDepth)
        {
            var centerPointId = "PanelCenter-" + panel.Number.ToString(CultureInfo.InvariantCulture);

            if (direction == DiagonalDirection.UpLeft)
            {
                yield return CreateMember(panel.PanelId, panel.Number, FrameMemberType.DiagonalBrace, face, panel.DiagonalProfileId, CreateEnd(FrameMemberEndRole.RightUpright, PostSide.Right, bottomElevation, panel.StartConnectionPointId), CreateEnd(FrameMemberEndRole.PanelCenter, PostSide.Right, middleElevation, centerPointId), frameDepth, panel.IsStandard, panel.IsException ? FrameMemberOrigin.Exception : FrameMemberOrigin.Standard);
                yield return CreateMember(panel.PanelId, panel.Number, FrameMemberType.DiagonalBrace, face, panel.DiagonalProfileId, CreateEnd(FrameMemberEndRole.RightUpright, PostSide.Right, topElevation, panel.EndConnectionPointId), CreateEnd(FrameMemberEndRole.PanelCenter, PostSide.Right, middleElevation, centerPointId), frameDepth, panel.IsStandard, panel.IsException ? FrameMemberOrigin.Exception : FrameMemberOrigin.Standard);
                yield return CreateMember(panel.PanelId, panel.Number, FrameMemberType.DiagonalBrace, face, panel.DiagonalProfileId, CreateEnd(FrameMemberEndRole.PanelCenter, PostSide.Left, middleElevation, centerPointId), CreateEnd(FrameMemberEndRole.LeftUpright, PostSide.Left, topElevation, panel.EndConnectionPointId), frameDepth, panel.IsStandard, panel.IsException ? FrameMemberOrigin.Exception : FrameMemberOrigin.Standard);
                yield return CreateMember(panel.PanelId, panel.Number, FrameMemberType.DiagonalBrace, face, panel.DiagonalProfileId, CreateEnd(FrameMemberEndRole.PanelCenter, PostSide.Left, middleElevation, centerPointId), CreateEnd(FrameMemberEndRole.LeftUpright, PostSide.Left, bottomElevation, panel.StartConnectionPointId), frameDepth, panel.IsStandard, panel.IsException ? FrameMemberOrigin.Exception : FrameMemberOrigin.Standard);
                yield break;
            }

            yield return CreateMember(panel.PanelId, panel.Number, FrameMemberType.DiagonalBrace, face, panel.DiagonalProfileId, CreateEnd(FrameMemberEndRole.LeftUpright, PostSide.Left, bottomElevation, panel.StartConnectionPointId), CreateEnd(FrameMemberEndRole.PanelCenter, PostSide.Left, middleElevation, centerPointId), frameDepth, panel.IsStandard, panel.IsException ? FrameMemberOrigin.Exception : FrameMemberOrigin.Standard);
            yield return CreateMember(panel.PanelId, panel.Number, FrameMemberType.DiagonalBrace, face, panel.DiagonalProfileId, CreateEnd(FrameMemberEndRole.LeftUpright, PostSide.Left, topElevation, panel.EndConnectionPointId), CreateEnd(FrameMemberEndRole.PanelCenter, PostSide.Left, middleElevation, centerPointId), frameDepth, panel.IsStandard, panel.IsException ? FrameMemberOrigin.Exception : FrameMemberOrigin.Standard);
            yield return CreateMember(panel.PanelId, panel.Number, FrameMemberType.DiagonalBrace, face, panel.DiagonalProfileId, CreateEnd(FrameMemberEndRole.PanelCenter, PostSide.Right, middleElevation, centerPointId), CreateEnd(FrameMemberEndRole.RightUpright, PostSide.Right, topElevation, panel.EndConnectionPointId), frameDepth, panel.IsStandard, panel.IsException ? FrameMemberOrigin.Exception : FrameMemberOrigin.Standard);
            yield return CreateMember(panel.PanelId, panel.Number, FrameMemberType.DiagonalBrace, face, panel.DiagonalProfileId, CreateEnd(FrameMemberEndRole.PanelCenter, PostSide.Right, middleElevation, centerPointId), CreateEnd(FrameMemberEndRole.RightUpright, PostSide.Right, bottomElevation, panel.StartConnectionPointId), frameDepth, panel.IsStandard, panel.IsException ? FrameMemberOrigin.Exception : FrameMemberOrigin.Standard);
        }

        private static IEnumerable<FrameSide> GetPhysicalFaces(FrameSide sideMode)
        {
            if (sideMode == FrameSide.Front)
            {
                yield return FrameSide.Front;
                yield break;
            }

            if (sideMode == FrameSide.Back)
            {
                yield return FrameSide.Back;
                yield break;
            }

            if (sideMode == FrameSide.Both)
            {
                yield return FrameSide.Front;
                yield return FrameSide.Back;
            }
        }

        private static FrameMember CreateMember(
            string sourcePanelId,
            int sourcePanelIndex,
            FrameMemberType memberType,
            FrameSide face,
            string profileId,
            FrameMemberEnd start,
            FrameMemberEnd end,
            double frameDepth,
            bool isStandard,
            FrameMemberOrigin origin)
        {
            var horizontalDistance = GetHorizontalDistance(start, end, Math.Max(frameDepth, 0.0));
            var verticalDistance = end.Elevation - start.Elevation;

            return new FrameMember
            {
                SourcePanelId = sourcePanelId,
                SourcePanelIndex = sourcePanelIndex,
                MemberType = memberType,
                CatalogId = NormalizeText(profileId),
                ProfileId = NormalizeText(profileId),
                Quantity = 1,
                PositionRatio = 0.0,
                MountingFace = face,
                Origin = origin,
                Start = start,
                End = end,
                Length = Math.Sqrt(horizontalDistance * horizontalDistance + verticalDistance * verticalDistance),
                Angle = Math.Atan2(verticalDistance, horizontalDistance <= 0.0 ? 0.0001 : horizontalDistance) * 180.0 / Math.PI,
                IsStandard = isStandard
            };
        }

        private static FrameMemberEnd CreateEnd(FrameMemberEndRole role, PostSide postSide, double elevation, string connectionPointId)
        {
            return new FrameMemberEnd
            {
                Role = role,
                PostSide = postSide,
                HorizontalPositionRatio = GetNormalizedHorizontalPosition(role),
                Elevation = elevation,
                ConnectionPointId = NormalizeText(connectionPointId)
            };
        }

        private static FrameMemberEnd CreateCustomEnd(double horizontalPositionRatio, double elevation, string connectionPointId)
        {
            return new FrameMemberEnd
            {
                Role = FrameMemberEndRole.Custom,
                HorizontalPositionRatio = Math.Clamp(horizontalPositionRatio, 0.0, 1.0),
                Elevation = elevation,
                ConnectionPointId = NormalizeText(connectionPointId)
            };
        }

        private static double GetHorizontalDistance(FrameMemberEnd start, FrameMemberEnd end, double frameDepth)
        {
            var startPosition = start == null ? 0.0 : start.HorizontalPositionRatio;
            var endPosition = end == null ? 0.0 : end.HorizontalPositionRatio;
            return Math.Abs(endPosition - startPosition) * frameDepth;
        }

        private static double GetNormalizedHorizontalPosition(FrameMemberEndRole role)
        {
            if (role == FrameMemberEndRole.LeftUpright)
            {
                return 0.0;
            }

            if (role == FrameMemberEndRole.RightUpright)
            {
                return 1.0;
            }

            if (role == FrameMemberEndRole.PanelCenter)
            {
                return 0.5;
            }

            return 0.0;
        }

        private static bool IsHorizontalMember(FrameMember member)
        {
            return member != null &&
                (member.MemberType == FrameMemberType.LowerHorizontal ||
                 member.MemberType == FrameMemberType.UpperHorizontal ||
                 member.MemberType == FrameMemberType.IntermediateHorizontal ||
                 member.MemberType == FrameMemberType.AdditionalHorizontal);
        }

        private static string NormalizeText(string value)
        {
            return value == null ? string.Empty : value.Trim();
        }
    }
}
