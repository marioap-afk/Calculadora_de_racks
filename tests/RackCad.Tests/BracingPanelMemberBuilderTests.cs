using System.Linq;
using RackCad.Application.RackFrames;
using RackCad.Domain.RackFrames;
using Xunit;

namespace RackCad.Tests
{
    public class BracingPanelMemberBuilderTests
    {
        private static readonly BracingPanelMemberBuilder Builder = new BracingPanelMemberBuilder();

        private static RackFrameConfiguration BuildStandardModel()
        {
            var configuration = new HardcodedStandardRackFrameService().CreateDefault();
            Builder.RefreshPhysicalModel(configuration);
            return configuration;
        }

        private static bool IsHorizontal(FrameMember member)
        {
            return member.MemberType == FrameMemberType.LowerHorizontal ||
                   member.MemberType == FrameMemberType.UpperHorizontal ||
                   member.MemberType == FrameMemberType.IntermediateHorizontal ||
                   member.MemberType == FrameMemberType.AdditionalHorizontal;
        }

        [Fact]
        public void RefreshPhysicalModel_StandardFrame_GeneratesOneMemberPerHorizontalPlusOneDiagonalPerBracedPanel()
        {
            var configuration = BuildStandardModel();

            // Standard now: 5 horizontals (3 standard + 2 closings) and 2 diagonals (the closing panels carry none).
            Assert.Equal(5, configuration.Members.Count(IsHorizontal));
            Assert.Equal(2, configuration.Members.Count(m => m.MemberType == FrameMemberType.DiagonalBrace));
            Assert.Equal(7, configuration.Members.Count);
        }

        [Fact]
        public void RefreshPhysicalModel_AttachesDiagonalsToBracedPanelsOnly()
        {
            var configuration = BuildStandardModel();
            var bracedPanels = configuration.BracingPanels.Where(p => p.Arrangement != BracingPattern.NoBracing).ToList();
            var closingPanels = configuration.BracingPanels.Where(p => p.Arrangement == BracingPattern.NoBracing).ToList();

            Assert.All(bracedPanels, panel => Assert.Single(panel.Members));
            Assert.All(closingPanels, panel => Assert.Empty(panel.Members));
            // Horizontals are not attached to any panel.
            Assert.DoesNotContain(configuration.BracingPanels.SelectMany(p => p.Members), IsHorizontal);
        }

        [Fact]
        public void RefreshPhysicalModel_ResolvesPanelElevationsFromHorizontals()
        {
            var configuration = BuildStandardModel();
            var panels = configuration.BracingPanels.OrderBy(p => p.Number).ToList();

            Assert.Equal((4.0, 48.0), (panels[0].StartElevation, panels[0].EndElevation));
            Assert.Equal((48.0, 92.0), (panels[1].StartElevation, panels[1].EndElevation));
            Assert.Equal((92.0, 110.0), (panels[2].StartElevation, panels[2].EndElevation));
            Assert.Equal((110.0, 128.0), (panels[3].StartElevation, panels[3].EndElevation));
        }

        [Fact]
        public void AutoAlternating_AlternatesDiagonalDirectionByPanelNumber()
        {
            var configuration = BuildStandardModel();
            var panels = configuration.BracingPanels.OrderBy(p => p.Number).ToList();

            // P1 (odd) rises to the right: start on the left post, end on the right post.
            var p1 = panels[0].Members.Single();
            Assert.Equal(PostSide.Left, p1.Start.PostSide);
            Assert.Equal(PostSide.Right, p1.End.PostSide);

            // P2 (even) rises to the left: start on the right post, end on the left post.
            var p2 = panels[1].Members.Single();
            Assert.Equal(PostSide.Right, p2.Start.PostSide);
            Assert.Equal(PostSide.Left, p2.End.PostSide);
        }

        [Fact]
        public void NoBracing_ProducesNoDiagonalsButKeepsHorizontals()
        {
            var configuration = new HardcodedStandardRackFrameService().CreateDefault();
            foreach (var panel in configuration.BracingPanels)
            {
                panel.Arrangement = BracingPattern.NoBracing;
            }

            Builder.RefreshPhysicalModel(configuration);

            Assert.DoesNotContain(configuration.Members, m => m.MemberType == FrameMemberType.DiagonalBrace);
            Assert.Equal(5, configuration.Members.Count(IsHorizontal));
            Assert.All(configuration.BracingPanels, panel => Assert.Empty(panel.Members));
        }

        [Fact]
        public void XBracing_ProducesTwoCrossingDiagonalsPerPanel()
        {
            var configuration = new HardcodedStandardRackFrameService().CreateDefault();
            var target = configuration.BracingPanels.First();
            target.Arrangement = BracingPattern.XBracing;

            Builder.RefreshPhysicalModel(configuration);

            Assert.Equal(2, target.Members.Count);
            // The two braces run in opposite directions.
            Assert.Contains(target.Members, m => m.Start.PostSide == PostSide.Left && m.End.PostSide == PostSide.Right);
            Assert.Contains(target.Members, m => m.Start.PostSide == PostSide.Right && m.End.PostSide == PostSide.Left);
        }

        [Fact]
        public void BothFaces_DuplicatesDiagonalsOnFrontAndBack()
        {
            var configuration = new HardcodedStandardRackFrameService().CreateDefault();
            var target = configuration.BracingPanels.First();
            target.MountingFace = FrameSide.Both;

            Builder.RefreshPhysicalModel(configuration);

            Assert.Equal(2, target.Members.Count);
            Assert.Contains(target.Members, m => m.MountingFace == FrameSide.Front);
            Assert.Contains(target.Members, m => m.MountingFace == FrameSide.Back);
        }

        [Fact]
        public void RefreshPhysicalModel_DuplicateHorizontalIds_DoesNotThrow()
        {
            var configuration = new HardcodedStandardRackFrameService().CreateDefault();
            // Force a duplicate Id; the core regeneration path must degrade gracefully, not throw.
            configuration.Horizontals[1].Id = configuration.Horizontals[0].Id;

            var exception = Record.Exception(() => Builder.RefreshPhysicalModel(configuration));

            Assert.Null(exception);
        }

        [Fact]
        public void Custom_ArrangementProducesNoDerivedDiagonals()
        {
            var configuration = new HardcodedStandardRackFrameService().CreateDefault();
            foreach (var panel in configuration.BracingPanels)
            {
                panel.Arrangement = BracingPattern.Custom;
            }

            Builder.RefreshPhysicalModel(configuration);

            Assert.DoesNotContain(configuration.Members, m => m.MemberType == FrameMemberType.DiagonalBrace);
        }
    }
}
