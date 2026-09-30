using System;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems.Shared;
using RackCad.UI.Editor;
using Xunit;

namespace RackCad.UI.Tests
{
    public sealed class EditorViewBatchRequestTests
    {
        [Fact]
        public void NewBatchEnsuresOneIdentityAndSharesItWithEveryView()
        {
            var calls = 0;
            var session = new RackEditorSession<string, string>(
                new RackCatalog(), newIdFactory: () => { calls++; return "BATCH-ID"; });
            session.SetModel("design", "system");
            var views = new[] { RackViewAddress.Fondo(1), RackViewAddress.Post(2), RackViewAddress.Whole(DimensionViewKind.Planta) };
            var raised = 0;
            session.InsertRequestedRaised += (_, __) => raised++;

            RackInsertionBatchContext<string, string> captured = default;
            session.RequestInsertViews(views, context =>
            {
                captured = context;
                return new SelectiveInsertionRequest(null, null, context.Id, context.Name, "frontal");
            });

            Assert.Equal(1, calls);
            Assert.Equal("BATCH-ID", captured.Id);
            Assert.Equal(views, captured.Views);
            Assert.Equal(views, session.InsertionRequest.Views);
            Assert.Equal(1, raised);
        }

        [Fact]
        public void ExistingBatchPreservesAdoptedIdentityWithoutMintingAnother()
        {
            var calls = 0;
            var session = new RackEditorSession<string, string>(
                new RackCatalog(), newIdFactory: () => { calls++; return "NEW"; });
            session.Identity.Adopt("EXISTING", "Rack");
            var views = new[] { RackViewAddress.Fondo(0), RackViewAddress.Post(0) };

            session.RequestInsertViews(views, context =>
                new SelectiveInsertionRequest(null, null, context.Id, context.Name, "frontal"));

            Assert.Equal(0, calls);
            Assert.Equal("EXISTING", ((SelectiveInsertionRequest)session.InsertionRequest).RackId);
            Assert.Equal(views, session.InsertionRequest.Views);
        }

        [Fact]
        public void HistoricalSingleViewRequestExposesOneViewAndKeepsItsFields()
        {
            var request = new DynamicInsertionRequest(
                null, null, "D-1", "Din", "frontal", 1, sourceProject: null);

            var view = Assert.Single(request.Views);
            Assert.Equal(RackViewAddress.FlowEnd(RackFlowEnd.Entrance), view);
            Assert.Equal("frontal", request.View);
            Assert.Equal(1, request.Section);
        }

        [Fact]
        public void EmptyBatchIsRejectedBeforeIdentityIsMinted()
        {
            var calls = 0;
            var session = new RackEditorSession<string, string>(
                new RackCatalog(), newIdFactory: () => { calls++; return "NEW"; });

            Assert.Throws<ArgumentException>(() => session.RequestInsertViews(
                Array.Empty<RackViewAddress>(),
                context => new SelectiveInsertionRequest(null, null, context.Id, context.Name, "frontal")));
            Assert.Equal(0, calls);
        }
    }
}
