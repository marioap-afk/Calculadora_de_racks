using System.Collections.Generic;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    public class DynamicForkliftDefensePlanTests
    {
        [Fact]
        public void LengthAt_DefaultsEdgesToTwelveAndIntermediatePostsToThirtySix()
        {
            Assert.Equal(12.0, DynamicForkliftDefensePlan.At(null, 0, 4).ExitLength);
            Assert.Equal(36.0, DynamicForkliftDefensePlan.At(null, 1, 4).ExitLength);
            Assert.Equal(36.0, DynamicForkliftDefensePlan.At(null, 2, 4).EntranceLength);
            Assert.Equal(12.0, DynamicForkliftDefensePlan.At(null, 3, 4).EntranceLength);
            Assert.True(DynamicForkliftDefensePlan.At(null, 1, 4).DrawsExit);
            Assert.True(DynamicForkliftDefensePlan.At(null, 1, 4).DrawsEntrance);
        }

        [Fact]
        public void LengthAt_ExplicitZeroDisablesAndPositiveValueOverridesOnePost()
        {
            var values = new List<SafetyPostDefense>
            {
                new SafetyPostDefense { PostIndex = 0, ExitLength = 0.0, EntranceLength = 0.0 },
                new SafetyPostDefense { PostIndex = 1, ExitLength = 48.0, EntranceLength = 24.0 }
            };

            Assert.False(DynamicForkliftDefensePlan.At(values, 0, 3).DrawsExit);
            Assert.Equal(48.0, DynamicForkliftDefensePlan.At(values, 1, 3).ExitLength);
            Assert.Equal(24.0, DynamicForkliftDefensePlan.At(values, 1, 3).EntranceLength);
            Assert.True(DynamicForkliftDefensePlan.At(values, 1, 3).DrawsExit);
            Assert.True(DynamicForkliftDefensePlan.At(values, 1, 3).DrawsEntrance);
            Assert.Equal(12.0, DynamicForkliftDefensePlan.At(values, 2, 3).ExitLength);
        }
    }
}
