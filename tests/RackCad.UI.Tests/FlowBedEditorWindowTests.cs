using System;
using RackCad.Application.Persistence;
using RackCad.Domain.Systems;
using RackCad.UI.Editor;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// STA boundary tests for the REAL <see cref="RackCad.UI.RackFlowBedWindow"/> (cama de rodamiento) — the identity
    /// round-trip and I-11 source-metadata transport through its actual load paths (initiative I-24).
    /// <c>EditorShellAdoptionTests</c> already checks that a cama routes identity + insert through the shared session;
    /// this file locks the ORTHOGONAL dimension the shell test does not: <c>LoadExisting</c> (adopt the drawn GUID +
    /// carry the source FlowBed document for round-trip) vs <c>LoadForNew</c> (no id → fresh GUID on insert). A cama has
    /// no view/section/update — its insert is always <c>view:null, section:-1, UpdateOnly:false</c>. Deterministic.
    /// </summary>
    public sealed class FlowBedEditorWindowTests
    {
        [Fact]
        public void LoadExisting_AdoptsDrawnGuidAndName()
        {
            // RACKEDITAR path for a cama: reopening a drawn bed adopts its GUID + name so a re-save keeps identity.
            var (id, name) = StaTestRunner.Run(() =>
            {
                var window = new RackFlowBedWindow(canInsertInAutoCad: true);
                window.LoadExisting(new FlowBedConfiguration(), "GUID-CAMA", "Cama A");
                return (window.RackId, window.RackName);
            });

            Assert.Equal("GUID-CAMA", id);
            Assert.Equal("Cama A", name);
        }

        [Fact]
        public void LoadExisting_WithSourceDocument_ExposesItForI11Transport()
        {
            // The FlowBed document read from the embed must be exposed so a library→drawing insert / save-to-library
            // preserves its unknown JSON fields + schema version (I-11). Regression: the source metadata being dropped.
            var (source, exposed) = StaTestRunner.Run(() =>
            {
                var window = new RackFlowBedWindow(canInsertInAutoCad: true);
                var sourceDoc = FlowBedDocument.FromDomain(new FlowBedConfiguration());
                window.LoadExisting(new FlowBedConfiguration(), "GUID-CAMA", "Cama A", sourceDoc);
                return (sourceDoc, window.SourceFlowBedToInsert);
            });

            Assert.NotNull(exposed);
            Assert.Same(source, exposed);
        }

        [Fact]
        public void LoadForNew_LeavesNoId_ThenInsertMintsFreshGuid()
        {
            // A library template opens as a NEW bed: no id adopted, so the first insert mints a fresh GUID. A cama insert
            // has no view/section/update (the window exposes none; the session normalizes them). Regression: a
            // library-opened bed reusing a stale id, or a cama insert that somehow carries a view/update.
            var (idAfterLoad, idAfterInsert, view, section, updateOnly, requested, name) = StaTestRunner.Run(() =>
            {
                var window = new RackFlowBedWindow(canInsertInAutoCad: true);
                window.LoadForNew(new FlowBedConfiguration(), "Cama nueva");
                var afterLoad = window.RackId;
                window.Session.RequestInsert(view: null, section: -1,
                    ctx => new FlowBedInsertionRequest(new FlowBedConfiguration(), ctx.Id, ctx.Name, null));
                return (afterLoad, window.RackId, window.Session.InsertView, window.Session.InsertSection,
                    window.Session.UpdateOnly, window.InsertRequested, window.RackName);
            });

            Assert.Null(idAfterLoad);
            Assert.True(Guid.TryParse(idAfterInsert, out _)); // a fresh, real GUID was minted on insert
            Assert.Null(view);
            Assert.Equal(-1, section);
            Assert.False(updateOnly);
            Assert.True(requested);
            Assert.Equal("Cama nueva", name);
        }
    }
}
