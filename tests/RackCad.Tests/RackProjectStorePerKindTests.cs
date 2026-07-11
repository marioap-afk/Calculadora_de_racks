using RackCad.Application.Persistence;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>The library store round-trips the new project kinds: a selectivo design, a cama, and a larguero.</summary>
    public class RackProjectStorePerKindTests
    {
        private static RackProject RoundTrip(RackProject project)
        {
            var store = new RackProjectStore();
            return store.Deserialize(store.Serialize(project));
        }

        [Fact]
        public void RoundTrip_Larguero_PreservesFields()
        {
            var larguero = new LargueroDesign { Name = "L1", BeamProfileId = "BEAM_X", Peralte = 4.0, Length = 96.0, MensulaOverride = "MENSULA_Y" };

            var restored = RoundTrip(RackProject.ForLarguero(larguero));

            Assert.Equal(RackSystemKind.Larguero, restored.Kind);
            Assert.NotNull(restored.Larguero);
            Assert.Equal("BEAM_X", restored.Larguero.BeamProfileId);
            Assert.Equal(96.0, restored.Larguero.Length, 4);
            Assert.Equal("MENSULA_Y", restored.Larguero.MensulaOverride);
        }

        [Fact]
        public void RoundTrip_Cama_PreservesFields()
        {
            var config = new FlowBedConfiguration { BedType = FlowBedType.Pushback, LaneDepth = 120.0, PalletDepth = 48.0, RollerId = "ROLLER_Z" };

            var restored = RoundTrip(RackProject.ForCama(config));

            Assert.Equal(RackSystemKind.Cama, restored.Kind);
            Assert.NotNull(restored.FlowBed);
            Assert.Equal(FlowBedType.Pushback, restored.FlowBed.BedType);
            Assert.Equal(120.0, restored.FlowBed.LaneDepth, 4);
            Assert.Equal("ROLLER_Z", restored.FlowBed.RollerId);
        }

        [Fact]
        public void RoundTrip_SelectiveRack_PreservesIdentity()
        {
            var design = new SelectivePalletDesign { PostId = "POST_P", PalletDepth = 48.0 };
            design.Bays.Add(new SelectiveBayDesign());
            var document = SelectivePalletDesignDocument.From(design, "id-1", "Rack A");

            var restored = RoundTrip(RackProject.ForSelectiveRack(document));

            Assert.Equal(RackSystemKind.SelectiveRack, restored.Kind);
            Assert.NotNull(restored.SelectiveRack);
            Assert.Equal("id-1", restored.SelectiveRack.Id);
            Assert.Equal("Rack A", restored.SelectiveRack.Name);
            Assert.Equal("POST_P", restored.SelectiveRack.ToDomain().PostId);
        }
    }
}
