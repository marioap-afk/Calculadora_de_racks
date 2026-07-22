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
    /// view/section/update — its insert is always <c>view:null, section:-1, UpdateOnly:false</c>. The pure
    /// <c>LoadExisting</c>/<c>LoadForNew</c> identity tests are kept because they cover the load wiring the shell-adoption
    /// test does not. Deterministic: no timing, no pixels.
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
            // captured, the request is a FlowBedInsertionRequest with a non-null config, and the cama insert carries no
            // view/section/update.
            var (requested, id, name, requestType, flowBedNonNull, view, section, updateOnly) = StaTestRunner.Run(() =>
            {
                var window = new RackFlowBedWindow(canInsertInAutoCad: true);
                window.LoadForNew(ValidConfig(), "Cama plantilla");
                EditorWindowTestSupport.SetText(window, "NameBox", "Cama nueva");
                EditorWindowTestSupport.ClickNamed(window, "InsertButton");
                var request = window.Session.InsertionRequest as FlowBedInsertionRequest;
                return (window.InsertRequested, window.RackId, window.RackName, window.Session.InsertionRequest?.GetType().Name,
                    request?.FlowBed != null, window.Session.InsertView, window.Session.InsertSection, window.Session.UpdateOnly);
            });

            Assert.True(requested);
            Assert.True(Guid.TryParse(id, out _)); // fresh GUID minted by the real handler
            Assert.Equal("Cama nueva", name);       // the handler read NameBox.Text
            Assert.Equal(nameof(FlowBedInsertionRequest), requestType);
            Assert.True(flowBedNonNull);            // ReadConfig produced a real config in the payload
            Assert.Null(view);
            Assert.Equal(-1, section);
            Assert.False(updateOnly);
        }

        [Fact]
        public void ExistingBed_Insert_ViaButton_KeepsGuid_AndCarriesSourceDocument()
        {
            // The REAL insert handler on an existing bed: the GUID is preserved and the source FlowBed document (I-11) is
            // carried into the payload — verified through the actual click, not a direct session call.
            var (id, requestType, sourcePreserved, flowBedNonNull) = StaTestRunner.Run(() =>
            {
                var sourceDoc = FlowBedDocument.FromDomain(ValidConfig());
                var window = new RackFlowBedWindow(canInsertInAutoCad: true);
                window.LoadExisting(ValidConfig(), "GUID-CAMA", "Cama A", sourceDoc);
                EditorWindowTestSupport.ClickNamed(window, "InsertButton");
                var request = window.Session.InsertionRequest as FlowBedInsertionRequest;
                return (window.RackId, window.Session.InsertionRequest?.GetType().Name,
                    request != null && ReferenceEquals(request.SourceDocument, sourceDoc), request?.FlowBed != null);
            });

            Assert.Equal("GUID-CAMA", id);
            Assert.Equal(nameof(FlowBedInsertionRequest), requestType);
            Assert.True(sourcePreserved); // the source document flowed through the real handler into the payload (I-11)
            Assert.True(flowBedNonNull);
        }

        /// <summary>A ReadConfig-valid dynamic bed (lane depth + pallet depth set; roller falls back to the default).</summary>
        private static FlowBedConfiguration ValidConfig()
            => new FlowBedConfiguration { BedType = FlowBedType.Dynamic, LaneDepth = 96.0, PalletDepth = 48.0 };
    }
}
