using System;
using RackCad.Application.Persistence;
using RackCad.Domain.Systems;
using RackCad.UI.Editor;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// STA tests for the REAL <see cref="RackCad.UI.RackFlowBedWindow"/> (cama de rodamiento, initiative I-24). The insert
    /// runs through the window's OWN button handler (a genuine WPF Click → <c>InsertInAutoCad_Click</c> → ReadConfig →
    /// session → typed payload → Close), NOT by calling <c>session.RequestInsert</c> directly. A cama has no
    /// view/section/update — its insert is always <c>view:null, section:-1, UpdateOnly:false</c>. The payload's
    /// <see cref="FlowBedConfiguration"/> is checked field by field against the fixture (not just non-null). The pure
    /// <c>LoadExisting</c>/<c>LoadForNew</c> identity tests are kept. Deterministic: no timing, no pixels.
    /// </summary>
    public sealed class FlowBedEditorWindowTests
    {
        // ---- Pure load identity + I-11 source exposure (kept) ----

        [Fact]
        public void LoadExisting_AdoptsDrawnGuidAndName()
        {
            var (id, name) = StaTestRunner.Run(() =>
            {
                var window = new RackFlowBedWindow(canInsertInAutoCad: true);
                window.LoadExisting(ValidConfig(), "GUID-CAMA", "Cama A");
                return (window.RackId, window.RackName);
            });

            Assert.Equal("GUID-CAMA", id);
            Assert.Equal("Cama A", name);
        }

        [Fact]
        public void LoadExisting_WithSourceDocument_ExposesItForI11Transport()
        {
            var (source, exposed) = StaTestRunner.Run(() =>
            {
                var window = new RackFlowBedWindow(canInsertInAutoCad: true);
                var sourceDoc = FlowBedDocument.FromDomain(ValidConfig());
                window.LoadExisting(ValidConfig(), "GUID-CAMA", "Cama A", sourceDoc);
                return (sourceDoc, window.SourceFlowBedToInsert);
            });

            Assert.NotNull(exposed);
            Assert.Same(source, exposed);
        }

        // ---- Real insert through the window's own button handler ----

        [Fact]
        public void NewBed_Insert_ViaButton_MintsGuid_AndBuildsTheRealPayload()
        {
            // The REAL "Insertar en AutoCAD" handler runs (ReadConfig → session): a fresh GUID is minted, the typed name is
            // captured, the request is a FlowBedInsertionRequest, the produced config matches the fixture field by field,
            // and the cama insert carries no view/section/update.
            var (requested, id, name, requestType, config, view, section, updateOnly) = StaTestRunner.Run(() =>
            {
                var window = new RackFlowBedWindow(canInsertInAutoCad: true);
                window.LoadForNew(ValidConfig(), "Cama plantilla");
                EditorWindowTestSupport.SetText(window, "NameBox", "Cama nueva");
                EditorWindowTestSupport.ClickNamed(window, "InsertButton");
                var request = window.Session.InsertionRequest as FlowBedInsertionRequest;
                return (window.InsertRequested, window.RackId, window.RackName, window.Session.InsertionRequest?.GetType().Name,
                    request?.FlowBed, window.Session.InsertView, window.Session.InsertSection, window.Session.UpdateOnly);
            });

            Assert.True(requested);
            Assert.True(Guid.TryParse(id, out _)); // fresh GUID minted by the real handler
            Assert.Equal("Cama nueva", name);       // the handler read NameBox.Text
            Assert.Equal(nameof(FlowBedInsertionRequest), requestType);
            Assert.Null(view);
            Assert.Equal(-1, section);
            Assert.False(updateOnly);
            AssertFixtureConfig(config);
        }

        [Fact]
        public void ExistingBed_Insert_ViaButton_KeepsGuidName_CarriesSourceDoc_AndBuildsTheRealConfig()
        {
            // The REAL insert handler on an existing bed: GUID + name preserved, no view/section/update, the source FlowBed
            // document (I-11) carried into the payload, and the produced config matches the fixture field by field.
            var (requested, id, name, requestType, sourcePreserved, config, view, section, updateOnly) = StaTestRunner.Run(() =>
            {
                var sourceDoc = FlowBedDocument.FromDomain(ValidConfig());
                var window = new RackFlowBedWindow(canInsertInAutoCad: true);
                window.LoadExisting(ValidConfig(), "GUID-CAMA", "Cama A", sourceDoc);
                EditorWindowTestSupport.ClickNamed(window, "InsertButton");
                var request = window.Session.InsertionRequest as FlowBedInsertionRequest;
                return (window.InsertRequested, window.RackId, window.RackName, window.Session.InsertionRequest?.GetType().Name,
                    request != null && ReferenceEquals(request.SourceDocument, sourceDoc), request?.FlowBed,
                    window.Session.InsertView, window.Session.InsertSection, window.Session.UpdateOnly);
            });

            Assert.True(requested);
            Assert.Equal("GUID-CAMA", id);
            Assert.Equal("Cama A", name);
            Assert.Equal(nameof(FlowBedInsertionRequest), requestType);
            Assert.True(sourcePreserved); // the source document flowed through the real handler into the payload (I-11)
            Assert.Null(view);
            Assert.Equal(-1, section);
            Assert.False(updateOnly);
            AssertFixtureConfig(config);
        }

        // ---- Helpers ----

        /// <summary>The config the real handler produced must match the fixture field by field, not just be non-null.</summary>
        private static void AssertFixtureConfig(FlowBedConfiguration config)
        {
            Assert.NotNull(config);
            Assert.Equal(FlowBedType.Dynamic, config.BedType);
            Assert.Equal(96.0, config.LaneDepth, 6);
            Assert.Equal(48.0, config.PalletDepth, 6);
            Assert.Equal(FlowBedDefaults.RollerId, config.RollerId);
            Assert.Null(config.RollerPitchOverride);
        }

        /// <summary>A ReadConfig-valid dynamic bed with explicit fixture values (lane depth, pallet depth, roller).</summary>
        private static FlowBedConfiguration ValidConfig()
            => new FlowBedConfiguration
            {
                BedType = FlowBedType.Dynamic,
                LaneDepth = 96.0,
                PalletDepth = 48.0,
                RollerId = FlowBedDefaults.RollerId
            };
    }
}
