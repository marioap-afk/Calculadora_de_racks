using Xunit;

namespace RackCad.Tests
{
    /// <summary>I-55 G3: green baselines for PR-1 and PR-2. G4/G5 will intentionally invert these assertions.</summary>
    public sealed class I55DeferredProductFixCharacterizationTests
    {
        [Fact]
        public void Pr1_PushBackPersistsTheSelectedPhysicalPostIndex()
        {
            var source = I55ProductCharacterizationTestSupport.Code("src", "RackCad.UI", "Systems", "PushBack", "RackPushBackSystemWindow.xaml.cs");
            Assert.Contains("PhysicalPostIndex(LateralSectionBox.SelectedItem)", source);
            Assert.Contains("corte.PostIndex == section", source);
            Assert.DoesNotContain("Math.Max(0, LateralSectionBox.SelectedIndex)", source);
        }

        [Fact]
        public void Pr2_CantileverPluginForwardsPlantaVisibilityAtBothPlanCalls()
        {
            var source = I55ProductCharacterizationTestSupport.Code("src", "RackCad.Plugin", "RackCantileverCommands.cs");
            Assert.Equal(2, I55ProductCharacterizationTestSupport.Count(source, "CantileverViewPlanBuilder.Build("));
            Assert.Equal(2, I55ProductCharacterizationTestSupport.Count(source, "design.PlantaVisibility"));
        }

        [Fact]
        public void Id19SourcePlacementInputsRemainReadOnlyFoundationFacts()
        {
            var source = I55ProductCharacterizationTestSupport.Code("src", "RackCad.Plugin", "RackDuplicarCommands.cs");
            Assert.Contains("Position", source);
            Assert.Contains("Rotation", source);
            Assert.Contains("Scale", source);
            Assert.Contains("new RackPhysicalReferenceSnapshot", source);
        }
    }
}
