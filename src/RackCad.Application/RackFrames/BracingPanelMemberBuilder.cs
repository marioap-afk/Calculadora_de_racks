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

            var offsets = DiagonalOffsets.From(configuration);
            var members = BuildHorizontalMembers(configuration.Horizontals, configuration.Depth)
                .Concat(BuildPanelDiagonalMembers(configuration.BracingPanels, configuration.Depth, offsets))
                .ToList();

            configuration.Members.Clear();

            foreach (var panel in configuration.BracingPanels)
            {
                panel.Members.Clear();
            }

            // Index panels by id once instead of scanning the panel list per member (was O(members x panels)).
            var panelsById = configuration.BracingPanels
                .GroupBy(panel => panel.PanelId ?? string.Empty)
                .ToDictionary(group => group.Key, group => group.First());

            foreach (var member in members)
            {
                configuration.Members.Add(member);

                if (!IsHorizontalMember(member) &&
                    panelsById.TryGetValue(member.SourcePanelId ?? string.Empty, out var panel))
                {
                    panel.Members.Add(member);
                }
            }
        }

        private static void ResolvePanelElevations(RackFrameConfiguration configuration)
        {
            // Duplicate-tolerant: RefreshPhysicalModel is the core regeneration entry point, so a
            // stray duplicate Id (or two empty Ids) must degrade gracefully, not throw from ToDictionary.
            var horizontals = configuration.Horizontals
                .GroupBy(horizontal => horizontal.Id ?? string.Empty)
                .ToDictionary(group => group.Key, group => group.First());

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

        /// <summary>Troquel-derived diagonal setbacks/spacing (in inches), shared with the lateral drawer's rule.</summary>
        private readonly struct DiagonalOffsets
        {
            private DiagonalOffsets(double startOffset, double endOffset, double doubleSpacing)
            {
                StartOffset = startOffset;
                EndOffset = endOffset;
                DoubleSpacing = doubleSpacing;
            }

            public double StartOffset { get; }
            public double EndOffset { get; }
            public double DoubleSpacing { get; }

            public static DiagonalOffsets From(RackFrameConfiguration configuration)
            {
                var paso = configuration.PasoTroquel > 0.0 ? configuration.PasoTroquel : 2.0;
                return new DiagonalOffsets(
                    configuration.DiagonalStartOffsetTroqueles * paso,
                    configuration.DiagonalEndOffsetTroqueles * paso,
                    configuration.DiagonalDoubleSpacingTroqueles * paso);
            }
        }

        private static IEnumerable<FrameMember> BuildPanelDiagonalMembers(IEnumerable<BracingPanel> panels, double frameDepth, DiagonalOffsets offsets)
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
                    foreach (var member in BuildPanelDiagonalMembers(panel, face, frameDepth, offsets))
                    {
                        yield return member;
                    }
                }
            }
        }

        private static IEnumerable<FrameMember> BuildPanelDiagonalMembers(BracingPanel panel, FrameSide face, double frameDepth, DiagonalOffsets offsets)
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
                foreach (var member in CreateDoubleDiagonal(panel, face, direction, bottomElevation, topElevation, frameDepth, offsets))
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

        private static DiagonalDirection ResolveDirection(BracingPanel panel) => panel.ResolveDiagonalDirection();

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
            double frameDepth,
            DiagonalOffsets offsets)
        {
            // Match what the lateral drawer actually draws (shared rule, BracingDiagonalGeometry): two FULL-depth
            // diagonals offset VERTICALLY by the double spacing (V-style celosía), set back the start/end offsets from the
            // panel's horizontals. The preview used to offset them HORIZONTALLY by 0.14·fondo across the full panel height
            // — a different geometry than what got drawn, so the preview and the BOM length disagreed with the DWG.
            var e = BracingDiagonalGeometry.DoubleDiagonal(bottomElevation, topElevation, offsets.StartOffset, offsets.EndOffset, offsets.DoubleSpacing);

            // UpRight rises left(0.0)→right(1.0); UpLeft mirrors to right(1.0)→left(0.0). Both diagonals share it.
            var startRatio = direction == DiagonalDirection.UpLeft ? 1.0 : 0.0;
            var endRatio = direction == DiagonalDirection.UpLeft ? 0.0 : 1.0;
            var origin = panel.IsException ? FrameMemberOrigin.Exception : FrameMemberOrigin.Standard;

            yield return CreateMember(panel.PanelId, panel.Number, FrameMemberType.DiagonalBrace, face, panel.DiagonalProfileId,
                CreateCustomEnd(startRatio, e.LowerStart, panel.StartConnectionPointId),
                CreateCustomEnd(endRatio, e.LowerEnd, panel.EndConnectionPointId),
                frameDepth, panel.IsStandard, origin);

            yield return CreateMember(panel.PanelId, panel.Number, FrameMemberType.DiagonalBrace, face, panel.DiagonalProfileId,
                CreateCustomEnd(startRatio, e.UpperStart, panel.StartConnectionPointId),
                CreateCustomEnd(endRatio, e.UpperEnd, panel.EndConnectionPointId),
                frameDepth, panel.IsStandard, origin);
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
