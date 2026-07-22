using System;
using System.Globalization;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using RackCad.UI.Editor;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// STA boundary tests for the REAL <see cref="RackCad.UI.RackDynamicSystemWindow"/> — the wiring that the pure
    /// Application suites (<c>DynamicFrontMatrixTests</c>, <c>DynamicEditorDesignAssemblerTests</c>) cannot reach because
    /// it lives in the WPF window (net8.0-windows). Two things are locked here (initiative I-24):
    ///
    /// 1. The window's ADOPTION of the I-21 editor state round-trips: the design the window assembles from its own
    ///    controls, when reloaded through <c>LoadExisting</c>/<c>LoadDesignForNew</c> and reassembled, resolves to the
    ///    SAME drawing (a load→build fixpoint). If the window stopped reading the matrix/assembler faithfully, the two
    ///    resolved signatures would diverge and the test fails closed.
    /// 2. The identity round-trip through the real load entry points: <c>LoadExisting</c> adopts the drawn GUID (kept on
    ///    re-insert/update), while <c>LoadDesignForNew</c> leaves it unminted so a library template gets a FRESH GUID on
    ///    insert — and update nulls the view/section and sets <c>UpdateOnly</c>. These paths are not exercised by
    ///    <c>EditorShellAdoptionTests</c> (which calls <c>Identity.Adopt</c> directly) nor by the pure session tests.
    ///
    /// The window builds through its private <c>Recompose()</c>; the internal <c>BuildDesignForTest</c> seam (I-24) runs
    /// exactly that and hands back the design so the fixpoint can be measured. Deterministic: no timing, no pixels.
    /// </summary>
    public sealed class DynamicEditorWindowTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        // ---- 1. Load→build adoption (fixpoint) ----

        [Fact]
        public void DefaultDesign_BuildsAResolvableSystem()
        {
            // Guards the window's default build path: a freshly opened dynamic editor must assemble a design that resolves
            // into a real system (≥1 front, modules, positive length/height). Regression: a broken default or a
            // BuildDesignForTest/Recompose that returns a null/degenerate design.
            var (ok, fronts, modules, length, height) = StaTestRunner.Run(() =>
            {
                var window = new RackDynamicSystemWindow(canInsertInAutoCad: true);
                var design = window.BuildDesignForTest(out var built);
                var resolution = new DynamicRackSystemResolver(Catalog).Resolve(design);
                return (built, resolution.System.Fronts.Count, resolution.System.Modules.Count,
                    resolution.System.TotalLength, resolution.Height.HeaderHeight);
            });

            Assert.True(ok);
            Assert.True(fronts >= 1);
            Assert.True(modules > 0);
            Assert.True(length > 0.0);
            Assert.True(height > 0.0);
        }

        [Fact]
        public void LoadExisting_ThenRebuild_RoundTripsResolvedGeometry()
        {
            // The core I-21 adoption characterization at the WINDOW: build the default design, reload it as an existing
            // rack, rebuild through the window and assert the resolved drawing signature is unchanged. Written against the
            // current window it passes; if a later change made the window stop restoring the matrix/assembler faithfully
            // (e.g. dropping a front, a level or a header fondo on load), the two signatures diverge and this fails.
            var (sig0, sig1, ok0, ok1) = StaTestRunner.Run(() =>
            {
                var window = new RackDynamicSystemWindow(canInsertInAutoCad: true);
                var d0 = window.BuildDesignForTest(out var b0);
                var s0 = Signature(d0); // capture BEFORE reloading (d0 is the private design reference)

                window.LoadExisting(d0, "GUID-D24", "Din 24");
                var d1 = window.BuildDesignForTest(out var b1);
                var s1 = Signature(d1);
                return (s0, s1, b0, b1);
            });

            Assert.True(ok0);
            Assert.True(ok1);
            Assert.Equal(sig0, sig1);
        }

        // ---- 2. Identity round-trip through the real load entry points ----

        [Fact]
        public void LoadExisting_AdoptsDrawnGuidAndName()
        {
            // RACKEDITAR path: opening an existing rack must adopt its GUID + name so a re-save keeps identity.
            // Regression: LoadExisting failing to route the id/name through the session identity.
            var (id, name) = StaTestRunner.Run(() =>
            {
                var window = new RackDynamicSystemWindow(canInsertInAutoCad: true);
                var design = window.BuildDesignForTest(out _);
                window.LoadExisting(design, "GUID-EXIST", "Din existente");
                return (window.RackId, window.RackName);
            });

            Assert.Equal("GUID-EXIST", id);
            Assert.Equal("Din existente", name);
        }

        [Fact]
        public void LoadExisting_ThenUpdate_KeepsGuid_NullsViewAndSection_SetsUpdateOnly()
        {
            // "Actualizar" on a linked existing rack: the GUID is preserved and the request is an in-place redraw
            // (view null, section -1, UpdateOnly true). Regression: an update that re-mints the id or carries a view.
            var (id, view, section, updateOnly, requested) = StaTestRunner.Run(() =>
            {
                var window = new RackDynamicSystemWindow(canInsertInAutoCad: true);
                var design = window.BuildDesignForTest(out _);
                window.LoadExisting(design, "GUID-EXIST", "Din existente");
                window.Session.RequestUpdate(
                    ctx => new DynamicInsertionRequest(null, null, ctx.Id, ctx.Name, ctx.View, ctx.Section, null));
                return (window.RackId, window.InsertView, window.InsertSection, window.UpdateOnly, window.InsertRequested);
            });

            Assert.Equal("GUID-EXIST", id);
            Assert.Null(view);
            Assert.Equal(-1, section);
            Assert.True(updateOnly);
            Assert.True(requested);
        }

        [Fact]
        public void LoadExisting_ThenInsertEntranceFrontal_KeepsGuidAndCarriesSection()
        {
            // Inserting another linked view of an existing rack keeps its GUID and carries the frontal section
            // (1 = entrance). Regression: a re-insert that loses the GUID or drops the section normalization.
            var (id, view, section, updateOnly) = StaTestRunner.Run(() =>
            {
                var window = new RackDynamicSystemWindow(canInsertInAutoCad: true);
                var design = window.BuildDesignForTest(out _);
                window.LoadExisting(design, "GUID-EXIST", "Din existente");
                window.Session.RequestInsert("frontal", 1,
                    ctx => new DynamicInsertionRequest(null, null, ctx.Id, ctx.Name, ctx.View, ctx.Section, null));
                return (window.RackId, window.InsertView, window.InsertSection, window.UpdateOnly);
            });

            Assert.Equal("GUID-EXIST", id);
            Assert.Equal("frontal", view);
            Assert.Equal(1, section);
            Assert.False(updateOnly);
        }

        [Fact]
        public void LoadDesignForNew_LeavesNoId_ThenInsertMintsFreshGuid()
        {
            // Opening a library template as NEW: no id is adopted, so the first insert mints a fresh GUID (the template is
            // not the drawn rack). Regression: a library-opened design that reuses a stale id instead of a new one.
            var (idAfterLoad, idAfterInsert, view, section, updateOnly) = StaTestRunner.Run(() =>
            {
                var window = new RackDynamicSystemWindow(canInsertInAutoCad: true);
                var design = window.BuildDesignForTest(out _);
                window.LoadDesignForNew(design, "Din nueva");
                var afterLoad = window.RackId;
                window.Session.RequestInsert("lateral", -1,
                    ctx => new DynamicInsertionRequest(null, null, ctx.Id, ctx.Name, ctx.View, ctx.Section, null));
                return (afterLoad, window.RackId, window.InsertView, window.InsertSection, window.UpdateOnly);
            });

            Assert.Null(idAfterLoad);
            Assert.True(Guid.TryParse(idAfterInsert, out _)); // a fresh, real GUID was minted on insert
            Assert.Equal("lateral", view);
            Assert.Equal(-1, section);
            Assert.False(updateOnly);
        }

        /// <summary>Full resolved-drawing signature of a dynamic design: front count, module count, total length and
        /// header height. Two designs with the same signature draw the same rack skeleton.</summary>
        private static string Signature(DynamicRackDesign design)
        {
            var resolution = new DynamicRackSystemResolver(Catalog).Resolve(design);
            var system = resolution.System;
            return string.Format(
                CultureInfo.InvariantCulture,
                "F={0}|M={1}|L={2:R}|H={3:R}",
                system.Fronts.Count, system.Modules.Count, system.TotalLength, resolution.Height.HeaderHeight);
        }
    }
}
