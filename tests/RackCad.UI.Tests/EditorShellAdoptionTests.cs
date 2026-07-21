using RackCad.Application.RackFrames;
using RackCad.Domain.Systems;
using RackCad.UI.Editor;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// Integration tests (STA) that construct the REAL editor windows and verify they carry their catalog, identity and
    /// insert/update contract through the shared shell (I-15), and that the configurator drives its preview through the
    /// shared debouncer — i.e. the shell has real production consumers, not just isolated unit tests. If a window kept
    /// the old inline fields instead of the session, driving the session here would NOT move the window's public props,
    /// so these fail closed. The windows load the real catalogs shipped next to the test binaries.
    /// </summary>
    public sealed class EditorShellAdoptionTests
    {
        [Fact]
        public void FlowBedWindow_LoadExisting_RoutesIdentityThroughTheSession()
        {
            var (hasSession, sessionId, rackId, rackName, catalog) = StaTestRunner.Run(() =>
            {
                var window = new RackFlowBedWindow(canInsertInAutoCad: true);
                window.LoadExisting(new FlowBedConfiguration(), "GUID-F", "Cama F");
                return (window.Session != null, window.Session.Identity.Id, window.RackId, window.RackName, window.Session.Catalog != null);
            });

            Assert.True(hasSession);           // the window holds the shared session
            Assert.True(catalog);              // capability 1: the catalog comes from the session
            Assert.Equal("GUID-F", sessionId); // capability 2: LoadExisting routed the id through session.Identity.Adopt
            Assert.Equal("GUID-F", rackId);    // the public RackId reads from the session, not a separate field
            Assert.Equal("Cama F", rackName);
        }

        [Fact]
        public void FlowBedWindow_InsertContract_IsBackedByTheSession()
        {
            var (requested, payload, rackId) = StaTestRunner.Run(() =>
            {
                var window = new RackFlowBedWindow(canInsertInAutoCad: true);
                window.Session.Identity.Adopt("GUID-F2", "Cama");
                // Driving the session's insert contract must move the window's PUBLIC props (they are getters over it).
                window.Session.RequestInsert(view: null, section: -1,
                    ctx => new FlowBedInsertionRequest(new FlowBedConfiguration(), ctx.Id, ctx.Name, null));
                return (window.InsertRequested, window.FlowBedToInsert, window.RackId);
            });

            Assert.True(requested);           // capability 4: InsertRequested reads session.InsertRequested
            Assert.NotNull(payload);          // FlowBedToInsert reads the session's payload
            Assert.Equal("GUID-F2", rackId);
        }

        [Fact]
        public void SelectiveWindow_IdentityAndInsertContract_AreBackedByTheSession()
        {
            var (hasSession, rackId, requested, view, catalog) = StaTestRunner.Run(() =>
            {
                var window = new RackSelectiveWindow(canInsertInAutoCad: true);
                window.Session.Identity.Adopt("GUID-S", "Sel");
                window.Session.RequestInsert("frontal", -1,
                    ctx => new SelectiveInsertionRequest(null, null, ctx.Id, ctx.Name, ctx.View));
                return (window.Session != null, window.RackId, window.InsertRequested, window.InsertView, window.Session.Catalog != null);
            });

            Assert.True(hasSession);
            Assert.True(catalog);
            Assert.Equal("GUID-S", rackId);
            Assert.True(requested);
            Assert.Equal("frontal", view);    // InsertView reads the session's normalized view
        }

        [Fact]
        public void DynamicWindow_IdentityAndInsertContract_AreBackedByTheSession()
        {
            var (hasSession, rackId, requested, section, catalog) = StaTestRunner.Run(() =>
            {
                var window = new RackDynamicSystemWindow(canInsertInAutoCad: true);
                window.Session.Identity.Adopt("GUID-D", "Din");
                window.Session.RequestInsert("frontal", 1,
                    ctx => new DynamicInsertionRequest(null, null, ctx.Id, ctx.Name, ctx.View, ctx.Section, null));
                return (window.Session != null, window.RackId, window.InsertRequested, window.InsertSection, window.Session.Catalog != null);
            });

            Assert.True(hasSession);
            Assert.True(catalog);
            Assert.Equal("GUID-D", rackId);
            Assert.True(requested);
            Assert.Equal(1, section);         // InsertSection reads the session's normalized section
        }

        [Fact]
        public void ConfiguratorWindow_DrivesPreviewThroughTheSharedDebouncer()
        {
            var (hasDebouncer, queuedAfterSchedule) = StaTestRunner.Run(() =>
            {
                var config = new HardcodedStandardRackFrameService().CreateDefault();
                var window = new RackFrameConfiguratorWindow(config, canInsertInAutoCad: true);
                var before = window.PreviewDebouncer != null;
                window.PreviewDebouncer.Schedule(); // the window's preview coalescing IS this shared debouncer
                return (before, window.PreviewDebouncer.IsQueued);
            });

            Assert.True(hasDebouncer);          // capability 3: the configurator's preview coalescer is the shared one
            Assert.True(queuedAfterSchedule);
        }
    }
}
