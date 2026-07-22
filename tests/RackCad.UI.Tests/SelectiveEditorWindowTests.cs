using System;
using RackCad.Application.Persistence;
using RackCad.Domain.Systems;
using RackCad.UI.Editor;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// STA boundary tests for the REAL <see cref="RackCad.UI.RackSelectiveWindow"/>: the identity round-trip through the
    /// window's actual load entry points (initiative I-24). <c>SelectiveEditorStateAdoptionTests</c> already locks the
    /// load→build GEOMETRY; this file locks the ORTHOGONAL dimension — GUID/name/view/UpdateOnly conservation across
    /// <c>LoadForNew</c> vs <c>LoadExisting</c> and insert vs update — which neither the pure session tests nor the shell
    /// adoption tests cover (those call <c>Identity.Adopt</c> directly, not the window's load paths).
    ///
    /// The window exposes its identity/insert contract as getters over the shared session; driving the session and the
    /// load methods and reading the public props verifies the wiring end to end. Deterministic: no timing, no pixels.
    /// </summary>
    public sealed class SelectiveEditorWindowTests
    {
        // Real catalog ids shipped next to the test binaries (mirrors SelectiveEditorStateAdoptionTests).
        private const string PostId = "POSTE_OMEGA_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA";
        private const string BeamId = "LARGUERO_ESCALON_CAL14_3_REMACHES";

        [Fact]
        public void LoadExisting_AdoptsDrawnGuidAndName()
        {
            // RACKEDITAR path: reopening a drawn selective rack must adopt its GUID + name so a re-save keeps identity.
            // Regression: LoadExisting not routing the embedded id/name through the session identity.
            var (id, name) = StaTestRunner.Run(() =>
            {
                var window = new RackSelectiveWindow(canInsertInAutoCad: true);
                window.LoadExisting(SelectivePalletDesignDocument.From(MinimalDesign(), "GUID-SEL", "Selectivo A"));
                return (window.RackId, window.RackName);
            });

            Assert.Equal("GUID-SEL", id);
            Assert.Equal("Selectivo A", name);
        }

        [Fact]
        public void LoadForNew_LeavesNoId_ThenInsertMintsFreshGuid()
        {
            // Opening a library template as NEW ignores any embedded id, so the first insert mints a FRESH GUID (the
            // template is not the drawn rack). Regression: a library-opened design that reuses the template's id.
            var (idAfterLoad, idAfterInsert, view, updateOnly, requested) = StaTestRunner.Run(() =>
            {
                var window = new RackSelectiveWindow(canInsertInAutoCad: true);
                // Even though the document carries "GUID-TEMPLATE", LoadForNew adopts a null id (fresh GUID on insert).
                window.LoadForNew(SelectivePalletDesignDocument.From(MinimalDesign(), "GUID-TEMPLATE", "Plantilla"));
                var afterLoad = window.RackId;
                window.Session.RequestInsert("frontal", -1,
                    ctx => new SelectiveInsertionRequest(null, null, ctx.Id, ctx.Name, ctx.View));
                return (afterLoad, window.RackId, window.InsertView, window.UpdateOnly, window.InsertRequested);
            });

            Assert.Null(idAfterLoad);
            Assert.True(Guid.TryParse(idAfterInsert, out _));  // a fresh, real GUID was minted on insert
            Assert.NotEqual("GUID-TEMPLATE", idAfterInsert);   // NOT the template's id
            Assert.Equal("frontal", view);
            Assert.False(updateOnly);
            Assert.True(requested);
        }

        [Fact]
        public void LoadExisting_ThenUpdate_KeepsGuid_NullsView_SetsUpdateOnly()
        {
            // "Actualizar" redraws every linked view in place: the GUID is preserved and no view is requested.
            // Regression: an update that re-mints the id or carries a view.
            var (id, view, updateOnly, requested) = StaTestRunner.Run(() =>
            {
                var window = new RackSelectiveWindow(canInsertInAutoCad: true);
                window.LoadExisting(SelectivePalletDesignDocument.From(MinimalDesign(), "GUID-SEL", "Selectivo A"));
                window.Session.RequestUpdate(
                    ctx => new SelectiveInsertionRequest(null, null, ctx.Id, ctx.Name, ctx.View));
                return (window.RackId, window.InsertView, window.UpdateOnly, window.InsertRequested);
            });

            Assert.Equal("GUID-SEL", id);
            Assert.Null(view);
            Assert.True(updateOnly);
            Assert.True(requested);
        }

        [Fact]
        public void LoadExisting_ThenInsertLateral_KeepsGuidAndView()
        {
            // Inserting a linked lateral of an existing rack keeps its GUID and carries the requested view.
            // Regression: a re-insert that loses the GUID or the normalized view.
            var (id, view, updateOnly) = StaTestRunner.Run(() =>
            {
                var window = new RackSelectiveWindow(canInsertInAutoCad: true);
                window.LoadExisting(SelectivePalletDesignDocument.From(MinimalDesign(), "GUID-SEL", "Selectivo A"));
                window.Session.RequestInsert("lateral", -1,
                    ctx => new SelectiveInsertionRequest(null, null, ctx.Id, ctx.Name, ctx.View));
                return (window.RackId, window.InsertView, window.UpdateOnly);
            });

            Assert.Equal("GUID-SEL", id);
            Assert.Equal("lateral", view);
            Assert.False(updateOnly);
        }

        /// <summary>A minimal but valid single-fondo selective design (one bay, two levels, floor beam) — enough to load
        /// into the window and exercise the identity/insert wiring.</summary>
        private static SelectivePalletDesign MinimalDesign()
        {
            var design = new SelectivePalletDesign
            {
                PostId = PostId,
                PostPeralte = 3.0,
                PalletTolerance = 4.0,
                VerticalClearance = 6.0,
                FloorBeamRise = 4.0,
                PalletDepth = 48.0,
                DepthCount = 1,
                DrawBasePlate = true
            };

            var bay = new SelectiveBayDesign { FloorBeam = true };
            for (var level = 0; level < 2; level++)
            {
                bay.Levels.Add(new SelectiveCell
                {
                    Pallet = new Tarima { Frente = 42.0, Alto = 60.0 },
                    PalletCount = 2,
                    BeamId = BeamId,
                    BeamPeralte = 4.0
                });
            }

            design.Bays.Add(bay);
            return design;
        }
    }
}
