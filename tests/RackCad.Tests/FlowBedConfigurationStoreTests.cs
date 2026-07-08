using RackCad.Application.Persistence;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>The roller-bed config round-trips through JSON (for the drawing embed).</summary>
    public class FlowBedConfigurationStoreTests
    {
        [Fact]
        public void RoundTrips_AllFields()
        {
            var config = new FlowBedConfiguration
            {
                BedType = FlowBedType.Pushback,
                LaneDepth = 96.0,
                PalletDepth = 48.0,
                RollerId = "ROLLER_X",
                RollerPitchOverride = 3.5
            };

            var store = new FlowBedConfigurationStore();
            var back = store.Deserialize(store.Serialize(config));

            Assert.Equal(FlowBedType.Pushback, back.BedType);
            Assert.Equal(96.0, back.LaneDepth, 4);
            Assert.Equal(48.0, back.PalletDepth, 4);
            Assert.Equal("ROLLER_X", back.RollerId);
            Assert.True(back.RollerPitchOverride.HasValue);
            Assert.Equal(3.5, back.RollerPitchOverride.Value, 4);
        }

        [Fact]
        public void Deserialize_Junk_ReturnsNull()
        {
            Assert.Null(new FlowBedConfigurationStore().Deserialize("no json"));
            Assert.Null(new FlowBedConfigurationStore().Deserialize(""));
        }
    }
}
