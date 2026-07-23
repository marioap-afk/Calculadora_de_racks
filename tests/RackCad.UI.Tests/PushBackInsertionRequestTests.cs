using RackCad.Application.Persistence;
using RackCad.Domain.Systems;
using RackCad.UI.Editor;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-18b increment 3a — the Push Back editor→host insertion contract. It freezes exactly what the request transports
    /// (kind, model references, identity, view, section, source project) and that a brand-new design (no library source)
    /// is accepted. Pure data: no WPF, no AutoCAD.
    /// </summary>
    public class PushBackInsertionRequestTests
    {
        [Fact]
        public void Carries_Kind_ModelReferences_Identity_View_Section_AndSourceProject()
        {
            var system = new PushBackSystem();
            var design = new PushBackDesign();
            var source = RackProject.ForPushBack(design);

            var request = new PushBackInsertionRequest(
                system, design, "GUID-1", "Rack A", RackEmbedDocument.ViewLateral, 2, source);

            Assert.Equal(RackSystemKind.PushBack, request.Kind);
            Assert.Same(system, request.System);
            Assert.Same(design, request.Design);
            Assert.Equal("GUID-1", request.RackId);
            Assert.Equal("Rack A", request.RackName);
            Assert.Equal(RackEmbedDocument.ViewLateral, request.View);
            Assert.Equal(2, request.Section);
            Assert.Same(source, request.SourceProject);
        }

        [Fact]
        public void IsARackInsertionRequest_SoTheHostDispatchesByKind()
        {
            var request = new PushBackInsertionRequest(
                new PushBackSystem(), new PushBackDesign(), "id", "name", RackEmbedDocument.ViewPlanta, -1, null);

            Assert.IsAssignableFrom<RackInsertionRequest>(request);
        }

        [Fact]
        public void AcceptsNullSourceProject_ForABrandNewDesign()
        {
            var request = new PushBackInsertionRequest(
                new PushBackSystem(), new PushBackDesign(), "id", "name", RackEmbedDocument.ViewFrontal, 0, null);

            Assert.Null(request.SourceProject);
            Assert.Equal(RackSystemKind.PushBack, request.Kind);
        }

        [Fact]
        public void UpdateShape_CarriesNullViewAndMinusOneSection()
        {
            // The in-place "Actualizar" request: no specific view, section -1.
            var request = new PushBackInsertionRequest(
                new PushBackSystem(), new PushBackDesign(), "GUID-2", "Rack B", null, -1, null);

            Assert.Null(request.View);
            Assert.Equal(-1, request.Section);
        }
    }
}
