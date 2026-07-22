using RackCad.Domain.Systems;
using RackCad.UI.Editor;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>The typed insertion contract each editor module produces and the Plugin host dispatches by kind (I-15).
    /// These freeze the kind mapping and the fields the host reads for each system.</summary>
    public sealed class RackInsertionRequestTests
    {
        [Fact]
        public void Header_HasSelectiveKind_AndCarriesConfigAndSource()
        {
            var request = new HeaderInsertionRequest(configuration: null, sourceProject: null);

            Assert.Equal(RackSystemKind.Selective, request.Kind); // "Selective" is the historic cabecera kind
            Assert.IsAssignableFrom<RackInsertionRequest>(request);
        }

        [Fact]
        public void Dynamic_HasPalletFlowKind_AndCarriesViewSectionIdentity()
        {
            var request = new DynamicInsertionRequest(
                system: null, design: null, rackId: "D-1", rackName: "Din", view: "lateral", section: 2, sourceProject: null);

            Assert.Equal(RackSystemKind.PalletFlow, request.Kind);
            Assert.Equal("D-1", request.RackId);
            Assert.Equal("Din", request.RackName);
            Assert.Equal("lateral", request.View);
            Assert.Equal(2, request.Section);
        }

        [Fact]
        public void FlowBed_HasCamaKind_AndCarriesPayloadAndIdentity()
        {
            var flowBed = new FlowBedConfiguration();
            var request = new FlowBedInsertionRequest(flowBed, rackId: "C-9", rackName: "Cama 9", sourceDocument: null);

            Assert.Equal(RackSystemKind.Cama, request.Kind);
            Assert.Same(flowBed, request.FlowBed);
            Assert.Equal("C-9", request.RackId);
            Assert.Equal("Cama 9", request.RackName);
        }

        [Fact]
        public void Selective_HasSelectiveRackKind_AndCarriesViewAndIdentity()
        {
            var request = new SelectiveInsertionRequest(
                system: null, design: null, rackId: "S-3", rackName: "Sel", view: "frontal");

            Assert.Equal(RackSystemKind.SelectiveRack, request.Kind);
            Assert.Equal("S-3", request.RackId);
            Assert.Equal("Sel", request.RackName);
            Assert.Equal("frontal", request.View);
        }
    }
}
