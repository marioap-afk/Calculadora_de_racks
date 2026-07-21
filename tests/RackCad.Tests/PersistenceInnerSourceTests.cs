using System.Text.Json;
using RackCad.Application.Persistence;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-11 (changes-required, 3ª ronda): the Persistence seams the Plugin/UI use to preserve an INNER design's metadata
    /// when it is reconstructed independently of a full source project — the cama DWG→library seam
    /// (<see cref="RackProject.WithSourceFlowBed"/>) and the safe inner-design read (<see cref="RackProjectStore.TryDeserialize"/> /
    /// <see cref="RackProjectStore.IsReadable"/>) that must NOT hide an incompatible MAJOR behind a fallback. These are the
    /// pure, testable cores; the Plugin edit-redraw and WPF window wiring that call them are covered by the AutoCAD matrix.
    /// </summary>
    public class PersistenceInnerSourceTests
    {
        // --- Cama DWG → library seam: preserve the FlowBedDocument even without a source RackProject ---

        [Fact]
        public void Cama_DwgToLibrary_WithSourceFlowBed_PreservesVersionAndUnknown_AndWritesEdit()
        {
            // A cama read from the DRAWING as a FlowBedDocument (1.6 + unknown), edited, then saved to the LIBRARY.
            var sourceDoc = new FlowBedConfigurationStore().DeserializeDocument(
                "{\"SchemaVersion\":\"1.6\",\"BedType\":\"Dynamic\",\"LaneDepth\":96,\"PalletDepth\":48,\"RollerId\":\"R\",\"futureBed\":\"keep\"}");
            Assert.NotNull(sourceDoc);

            var edited = new FlowBedConfiguration { BedType = FlowBedType.Pushback, LaneDepth = 200.0, PalletDepth = 0.0, RollerId = "ROLLER_NEW" };
            var back = new RackProjectStore().Serialize(RackProject.ForCama(edited).WithSourceFlowBed(sourceDoc));

            using var doc = JsonDocument.Parse(back);
            var flowBed = doc.RootElement.GetProperty("FlowBed");
            Assert.Equal("1.6", flowBed.GetProperty("SchemaVersion").GetString()); // not downgraded to 1.0
            Assert.True(flowBed.TryGetProperty("futureBed", out var kept));
            Assert.Equal("keep", kept.GetString());
            Assert.Equal(200.0, flowBed.GetProperty("LaneDepth").GetDouble(), 4); // the edit is written
            Assert.Equal("ROLLER_NEW", flowBed.GetProperty("RollerId").GetString());
        }

        [Fact]
        public void Cama_DwgToLibrary_WithoutSeam_DropsFlowBedMetadata()
        {
            // Contrast (documents the gap the seam closes): a fresh ForCama with no source stamps the current version
            // and carries no unknown fields — which is exactly why the DWG-edit path needs WithSourceFlowBed.
            var back = new RackProjectStore().Serialize(
                RackProject.ForCama(new FlowBedConfiguration { LaneDepth = 200.0, RollerId = "ROLLER_NEW" }));

            using var doc = JsonDocument.Parse(back);
            var flowBed = doc.RootElement.GetProperty("FlowBed");
            Assert.Equal("1.0", flowBed.GetProperty("SchemaVersion").GetString());
            Assert.False(flowBed.TryGetProperty("futureBed", out _));
        }

        // --- IsReadable: the non-throwing MAJOR gate that mirrors the store's wrapper-vs-header decision ---

        [Theory]
        [InlineData("{\"schemaVersion\":\"2.0\",\"kind\":\"Cama\"}", true)]   // wrapper, current major
        [InlineData("{\"schemaVersion\":\"2.9\",\"kind\":\"Cama\"}", true)]   // wrapper, same major newer minor
        [InlineData("{\"schemaVersion\":\"3.0\",\"kind\":\"Cama\"}", false)]  // wrapper, higher major
        [InlineData("{\"schemaVersion\":\"1.0\"}", true)]                     // bare header, current major (1.x)
        [InlineData("{\"schemaVersion\":\"2.0\"}", false)]                    // bare header, higher major than 1.x
        [InlineData("{\"kind\":\"Cama\"}", true)]                             // wrapper, no version = legacy
        [InlineData("not json", true)]                                       // malformed = benign
        [InlineData("", true)]                                               // empty = benign
        public void IsReadable_MirrorsWrapperVsHeaderMajorGate(string json, bool expected)
            => Assert.Equal(expected, new RackProjectStore().IsReadable(json));

        // --- TryDeserialize: distinguishes an incompatible MAJOR from a benign failure ---

        [Fact]
        public void TryDeserialize_ValidWrapper_ReturnsProject()
        {
            var json = new RackProjectStore().Serialize(RackProject.ForCama(
                new FlowBedConfiguration { LaneDepth = 96.0, RollerId = "R" }));

            var ok = new RackProjectStore().TryDeserialize(json, out var project, out var incompatibleMajor);

            Assert.True(ok);
            Assert.NotNull(project);
            Assert.Equal(RackSystemKind.Cama, project.Kind);
            Assert.False(incompatibleMajor);
        }

        [Fact]
        public void TryDeserialize_HigherMajorWrapper_FailsAndFlagsIncompatibleMajor()
        {
            // A sibling block whose inner design is a newer MAJOR: TryDeserialize fails AND flags it, so the caller does
            // NOT hide it behind the initiating project's (lower-major) metadata.
            var ok = new RackProjectStore().TryDeserialize(
                "{\"schemaVersion\":\"3.0\",\"kind\":\"Cama\",\"FlowBed\":{\"LaneDepth\":96}}",
                out var project, out var incompatibleMajor);

            Assert.False(ok);
            Assert.Null(project);
            Assert.True(incompatibleMajor);
        }

        [Fact]
        public void TryDeserialize_MalformedOrEmpty_FailsWithoutIncompatibleMajor()
        {
            var store = new RackProjectStore();

            Assert.False(store.TryDeserialize("{ not json", out _, out var m1));
            Assert.False(m1); // benign, not a MAJOR issue → a caller MAY fall back to the initiating project

            Assert.False(store.TryDeserialize("", out _, out var m2));
            Assert.False(m2);

            // A degenerate but same-major wrapper ({} → empty) fails to build but is NOT an incompatible major.
            Assert.False(store.TryDeserialize("{\"kind\":\"Cama\"}", out _, out var m3));
            Assert.False(m3);
        }
    }
}
