using Xunit;

namespace RackCad.Tests
{
    public sealed class ProductPolicyIndependenceGuardTests
    {
        [Fact]
        public void FoundationDoesNotReferenceI55ProductPolicy()
        {
            foreach (var file in new[] { "RackViewAddress.cs", "RackViewCodec.cs", "RackViewAvailability.cs" })
            {
                var source = I55ProductCharacterizationTestSupport.Code(
                    "src", "RackCad.Application", "Systems", "Shared", file);
                Assert.DoesNotContain("RackCad.Application.Views.Policy", source);
                Assert.DoesNotContain("ProjectionCodecPolicy", source);
                Assert.DoesNotContain("RackViewExposure", source);
            }
        }

        [Fact]
        public void ProductPolicyConsumesCodecAndAvailabilityWithoutParsingThemAgain()
        {
            var codec = I55ProductCharacterizationTestSupport.Code(
                "src", "RackCad.Application", "Views", "Policy", "ProjectionCodecPolicy.cs");
            var availability = I55ProductCharacterizationTestSupport.Code(
                "src", "RackCad.Application", "Views", "Policy", "ProjectionAvailabilityPolicy.cs");

            Assert.Contains("RackViewCodec.Encode", codec);
            Assert.Contains("RackViewCodec.Decode", codec);
            Assert.DoesNotContain("string.Equals", codec);
            Assert.DoesNotContain("Math.Max", codec);
            Assert.Contains("RackViewAvailability.Evaluate", availability);
            Assert.DoesNotContain(".Contains(decoded.Address)", availability);
        }
    }
}
