using RackCad.Application.Catalogs;
using RackCad.UI.Editor;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>The shared editor session that composes catalog + identity + coalesced recompute + the insert/update
    /// contract (I-15). Exercised with plain string design/system so the contract is verified without a real editor.</summary>
    public sealed class RackEditorSessionTests
    {
        private static RackEditorSession<string, string> NewSession(System.Action recompute = null)
            => new RackEditorSession<string, string>(new RackCatalog(), recompute, newIdFactory: () => "GID");

        [Fact]
        public void DefaultCatalog_IsNeverNull()
        {
            // UiSupport.LoadCatalogSafe falls back to an empty catalog, so a session always has a usable catalog.
            var session = new RackEditorSession<string, string>();

            Assert.NotNull(session.Catalog);
        }

        [Fact]
        public void RequestInsert_SetsContractEnsuresIdBuildsPayloadAndRaises()
        {
            var session = NewSession();
            session.SetModel("design-x", "system-y");
            session.Identity.SetName("Rack 9"); // the window passes NameBox.Text?.Trim(); the helper stores it verbatim

            var raised = 0;
            session.InsertRequestedRaised += (s, e) => raised++;

            RackInsertionContext<string, string> captured = default;
            session.RequestInsert("frontal", 3, ctx =>
            {
                captured = ctx;
                return new SelectiveInsertionRequest(null, null, ctx.Id, ctx.Name, ctx.View);
            });

            Assert.True(session.InsertRequested);
            Assert.False(session.UpdateOnly);
            Assert.Equal("frontal", session.InsertView);
            Assert.Equal(3, session.InsertSection);
            Assert.Equal("GID", session.Identity.Id);
            Assert.Equal(1, raised);

            // The build hook sees the ensured identity, the current model and the normalized view/section.
            Assert.Equal("GID", captured.Id);
            Assert.Equal("Rack 9", captured.Name);
            Assert.Equal("design-x", captured.Design);
            Assert.Equal("system-y", captured.System);
            Assert.Equal("frontal", captured.View);
            Assert.Equal(3, captured.Section);
            Assert.False(captured.UpdateOnly);

            var request = Assert.IsType<SelectiveInsertionRequest>(session.InsertionRequest);
            Assert.Equal("GID", request.RackId);
            Assert.Equal("frontal", request.View);
        }

        [Fact]
        public void RequestUpdate_NullsViewAndSection_KeepsExistingId()
        {
            var session = NewSession();
            session.Identity.Adopt("EXISTING", "Bodega");

            var raised = 0;
            session.InsertRequestedRaised += (s, e) => raised++;

            RackInsertionContext<string, string> captured = default;
            session.RequestUpdate(ctx =>
            {
                captured = ctx;
                return new SelectiveInsertionRequest(null, null, ctx.Id, ctx.Name, ctx.View);
            });

            Assert.True(session.InsertRequested);
            Assert.True(session.UpdateOnly);
            Assert.Null(session.InsertView);      // updateOnly ? null : view
            Assert.Equal(-1, session.InsertSection); // updateOnly ? -1 : section
            Assert.Equal("EXISTING", session.Identity.Id); // kept, not re-minted
            Assert.Null(captured.View);
            Assert.True(captured.UpdateOnly);
            Assert.Equal(1, raised);
        }

        [Fact]
        public void Recompute_DelegatesToTheInjectedAction()
        {
            var recomputes = 0;
            var session = NewSession(() => recomputes++);

            session.Recompute.Request();
            using (session.Recompute.Defer())
            {
                session.Recompute.Request();
                session.Recompute.Request();
            }

            Assert.Equal(2, recomputes); // one immediate + one coalesced from the scope
        }
    }
}
