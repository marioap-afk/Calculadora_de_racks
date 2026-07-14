using RackCad.Application.Persistence;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>Schema versioning + minimum-content validation across the design stores.</summary>
    public class PersistenceValidationTests
    {
        [Fact]
        public void SelectiveStore_EmptyObject_Throws()
        {
            // "{}" deserializes without error but has no frentes → rejected instead of a blank rack.
            Assert.Throws<System.InvalidOperationException>(() => new SelectivePalletDesignStore().Deserialize("{}"));
        }

        [Fact]
        public void SelectiveStore_FutureSchemaVersion_Throws()
        {
            var ex = Assert.Throws<System.InvalidOperationException>(
                () => new SelectivePalletDesignStore().Deserialize("{\"schemaVersion\":\"99.0\"}"));
            Assert.Contains("más nueva", ex.Message);
        }

        [Fact]
        public void FlowBedStore_EmptyObject_IsTreatedAsAbsent()
        {
            // Tolerant contract: a degenerate {} bed (length 0) returns null so callers show "datos de cama invalidos".
            Assert.Null(new FlowBedConfigurationStore().Deserialize("{}"));
        }

        [Fact]
        public void FlowBedStore_PushbackWithZeroPalletDepth_IsAccepted()
        {
            // Pushback beds legitimately have PalletDepth 0 — only LaneDepth (rail length) signals a real bed, so a bed
            // with a lane but no pallet depth must NOT be rejected.
            var bed = new FlowBedConfigurationStore().Deserialize("{\"laneDepth\":120,\"palletDepth\":0}");
            Assert.NotNull(bed);
        }

        [Fact]
        public void SchemaGuard_RejectsHigherMajor_AcceptsSameLowerOrMissing()
        {
            SchemaGuard.CheckReadable("1.0", "2.0", "X");      // older major → ok
            SchemaGuard.CheckReadable("2.0", "2.0", "X");      // same major → ok
            SchemaGuard.CheckReadable("2.9", "2.0", "X");      // higher MINOR, same major → ok
            SchemaGuard.CheckReadable(null, "2.0", "X");       // missing → legacy → ok
            SchemaGuard.CheckReadable("garbage", "2.0", "X");  // unparseable → legacy → ok

            Assert.Throws<System.InvalidOperationException>(() => SchemaGuard.CheckReadable("3.0", "2.0", "X"));
        }
    }
}
