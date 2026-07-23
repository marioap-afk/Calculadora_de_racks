using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using RackCad.Application.Persistence;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-18b — Push Back registration and persistence: the new sixth <see cref="RackSystemKind"/>, its
    /// <see cref="SystemDescriptor"/>, its wrapper slot and its I-11-preserving store round-trip. The five prior kinds
    /// keep their numeric values and wire behavior (frozen elsewhere by the characterization tests).
    /// </summary>
    public class PushBackRegistrationTests
    {
        private static PushBackDesign Design()
            => new PushBackDesign
            {
                Structure = new DynamicRackDesign
                {
                    Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                    PalletsDeep = 4,
                    LoadLevels = 3,
                    FirstLevelHeight = 6.0,
                    BeamDepth = 4.0
                }
            };

        [Fact]
        public void Enum_PriorValuesAreFrozen_AndPushBackIsTheNewFinalValue()
        {
            Assert.Equal(0, (int)RackSystemKind.Selective);
            Assert.Equal(1, (int)RackSystemKind.PalletFlow);
            Assert.Equal(2, (int)RackSystemKind.SelectiveRack);
            Assert.Equal(3, (int)RackSystemKind.Cama);
            Assert.Equal(4, (int)RackSystemKind.Larguero);
            Assert.Equal(5, (int)RackSystemKind.PushBack);   // new, at the end
            Assert.Equal(6, Enum.GetValues(typeof(RackSystemKind)).Length);
        }

        [Fact]
        public void Registry_PushBackDescriptor_HasTheApprovedLabelAndNoun_AndPersistenceOps()
        {
            var descriptor = SystemRegistry.Default.Get(RackSystemKind.PushBack);
            Assert.Equal("Push Back", descriptor.LibraryLabel);
            Assert.Equal("el sistema Push Back", descriptor.ValidationNoun);
            Assert.True(descriptor.SupportsPersistence);
        }

        [Fact]
        public void Store_RoundTrip_PushBack_PreservesDesignKindAndLabel()
        {
            var store = new RackProjectStore();
            var json = store.Serialize(RackProject.ForPushBack(Design()));

            using (var parsed = JsonDocument.Parse(json))
            {
                Assert.Equal("PushBack", parsed.RootElement.GetProperty("Kind").GetString());
                Assert.Equal(JsonValueKind.Object, parsed.RootElement.GetProperty("PushBack").ValueKind);
            }

            var project = store.Deserialize(json);
            Assert.Equal(RackSystemKind.PushBack, project.Kind);
            Assert.NotNull(project.PushBackDesign);
            Assert.Equal(4, project.PushBackDesign.Structure.PalletsDeep);
            Assert.True(SystemRegistry.Default.Get(project.Kind).IsUsable(project));
        }

        [Fact]
        public void Store_PushBack_PreservesUnknownWrapperAndPayloadFields_AndDoesNotDowngrade()
        {
            var store = new RackProjectStore();
            var initial = store.Serialize(RackProject.ForPushBack(Design()));

            var node = JsonNode.Parse(initial).AsObject();
            node["futureWrapperField"] = "keep-wrapper";                 // unknown wrapper key a newer build wrote
            var payload = node["PushBack"].AsObject();
            payload["futurePayloadField"] = "keep-payload";              // unknown payload key
            payload["SchemaVersion"] = "1.5";                            // future same-major minor

            var project = store.Deserialize(node.ToJsonString());
            var resaved = store.Serialize(project);

            Assert.Contains("futureWrapperField", resaved);
            Assert.Contains("keep-wrapper", resaved);
            Assert.Contains("futurePayloadField", resaved);
            Assert.Contains("keep-payload", resaved);
            using var parsed = JsonDocument.Parse(resaved);
            Assert.Equal("1.5", parsed.RootElement.GetProperty("PushBack").GetProperty("SchemaVersion").GetString()); // not downgraded
        }

        [Fact]
        public void Store_PushBack_IncompatibleMajor_IsRejected()
        {
            var store = new RackProjectStore();
            var node = JsonNode.Parse(store.Serialize(RackProject.ForPushBack(Design()))).AsObject();
            node["PushBack"].AsObject()["SchemaVersion"] = "9.0";        // a higher MAJOR this build cannot read

            Assert.ThrowsAny<Exception>(() => store.Deserialize(node.ToJsonString()));
        }

        [Fact]
        public void Store_PushBack_KindWithoutPayload_ThrowsClearError()
        {
            // A wrapper that names Kind PushBack but omits the payload is corrupt/truncated.
            var json = "{\"SchemaVersion\":\"2.0\",\"Kind\":\"PushBack\"}";
            var ex = Assert.Throws<InvalidOperationException>(() => new RackProjectStore().Deserialize(json));
            Assert.Contains("Push Back", ex.Message);
        }

        [Fact]
        public void Store_PushBack_SaveWithSourceHavingOtherSlots_DoesNotResurrectThem()
        {
            var store = new RackProjectStore();
            // A source document that (hypothetically) carried a FlowBed slot must not resurrect it when saving a PushBack.
            var source = RackProject.ForPushBack(Design())
                .WithSourceFlowBed(new FlowBedDocument { LaneDepth = 120.0 });
            var json = store.Serialize(source);

            using var parsed = JsonDocument.Parse(json);
            Assert.Equal(JsonValueKind.Object, parsed.RootElement.GetProperty("PushBack").ValueKind);
            Assert.Equal(JsonValueKind.Null, parsed.RootElement.GetProperty("FlowBed").ValueKind);   // NOT resurrected
        }
    }
}
