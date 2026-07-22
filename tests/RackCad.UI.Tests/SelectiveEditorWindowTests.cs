using System;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using RackCad.UI.Editor;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// STA tests for the REAL <see cref="RackCad.UI.RackSelectiveWindow"/> (initiative I-24). Insert/update run through the
    /// window's OWN button handlers (a genuine WPF Click → <c>*_Click</c> → <c>RequestDraw</c> → ConfirmPendingCellEdits →
    /// BuildSystem → SetModel → session → typed payload → Close), NOT by calling <c>session.RequestInsert/RequestUpdate</c>
    /// directly. That exercises the window's real validation, model build and payload. <c>SelectiveEditorStateAdoptionTests</c>
    /// already locks the load→build GEOMETRY; here the orthogonal dimension is GUID/name/view/UpdateOnly conservation,
    /// the concrete request type and a real, non-null payload — driven by actual clicks. Deterministic: no timing, no pixels.
    /// </summary>
    public sealed class SelectiveEditorWindowTests
    {
        // Real catalog ids shipped next to the test binaries (mirrors SelectiveEditorStateAdoptionTests).
        private const string PostId = "POSTE_OMEGA_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA";
        private const string BeamId = "LARGUERO_ESCALON_CAL14_3_REMACHES";

        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        // ---- Pure load identity (kept: distinct from the real-handler tests) ----

        [Fact]
        public void LoadExisting_AdoptsDrawnGuidAndName()
        {
            // RACKEDITAR path: reopening a drawn selective rack adopts its GUID + name so a re-save keeps identity.
            var (id, name) = StaTestRunner.Run(() =>
            {
                var window = new RackSelectiveWindow(canInsertInAutoCad: true);
                window.LoadExisting(SelectivePalletDesignDocument.From(MinimalDesign(), "GUID-SEL", "Selectivo A"));
                return (window.RackId, window.RackName);
            });

            Assert.Equal("GUID-SEL", id);
            Assert.Equal("Selectivo A", name);
        }

        // ---- Real insert/update through the window's own button handlers ----

        [Fact]
        public void NewRack_InsertFrontal_ViaButton_MintsGuid_AndBuildsTheRealPayload()
        {
            // The REAL "Insertar frontal" handler runs (RequestDraw → BuildSystem → SetModel → session): a fresh GUID is
            // minted, the typed name is captured, the request is a SelectiveInsertionRequest for the frontal view, and the
            // payload's design+system are non-null and correspond (the system is the resolution of the design).
            var (requested, view, updateOnly, id, name, requestType, corresponds) = StaTestRunner.Run(() =>
            {
                var window = new RackSelectiveWindow(canInsertInAutoCad: true);
                window.LoadForNew(SelectivePalletDesignDocument.From(MinimalDesign(), "GUID-TEMPLATE", "Plantilla"));
                EditorWindowTestSupport.SetText(window, "NameBox", "Selectivo nuevo");
                EditorWindowTestSupport.ClickByContent(window, "Insertar frontal");
                return Capture(window);
            });

            Assert.True(requested);
            Assert.Equal("frontal", view);
            Assert.False(updateOnly);
            Assert.True(Guid.TryParse(id, out _)); // fresh GUID minted by the real handler (template opened as new)
            Assert.Equal("Selectivo nuevo", name);  // the handler read NameBox.Text
            Assert.Equal(nameof(SelectiveInsertionRequest), requestType);
            Assert.True(corresponds);                // design+system non-null and system == resolution(design)
        }

        [Fact]
        public void ExistingRack_Update_ViaButton_KeepsGuid_RedrawsInPlace()
        {
            // The REAL "Actualizar" handler on an existing rack: GUID preserved, in-place redraw (view null, UpdateOnly
            // true), typed SelectiveInsertionRequest, real payload built.
            var (requested, view, updateOnly, id, _, requestType, corresponds) = StaTestRunner.Run(() =>
            {
                var window = new RackSelectiveWindow(canInsertInAutoCad: true);
                window.LoadExisting(SelectivePalletDesignDocument.From(MinimalDesign(), "GUID-SEL", "Selectivo A"));
                EditorWindowTestSupport.ClickNamed(window, "UpdateButton");
                return Capture(window);
            });

            Assert.True(requested);
            Assert.True(updateOnly);
            Assert.Null(view);
            Assert.Equal("GUID-SEL", id);
            Assert.Equal(nameof(SelectiveInsertionRequest), requestType);
            Assert.True(corresponds);
        }

        [Fact]
        public void ExistingRack_InsertLateral_ViaButton_KeepsGuid_LinkedView()
        {
            // The REAL "Insertar lateral" handler on an existing rack: a linked lateral view keeps the GUID and carries the
            // normalized view; typed SelectiveInsertionRequest with a real payload.
            var (requested, view, updateOnly, id, _, requestType, corresponds) = StaTestRunner.Run(() =>
            {
                var window = new RackSelectiveWindow(canInsertInAutoCad: true);
                window.LoadExisting(SelectivePalletDesignDocument.From(MinimalDesign(), "GUID-SEL", "Selectivo A"));
                EditorWindowTestSupport.ClickNamed(window, "InsertLateralButton");
                return Capture(window);
            });

            Assert.True(requested);
            Assert.Equal("lateral", view);
            Assert.False(updateOnly);
            Assert.Equal("GUID-SEL", id);
            Assert.Equal(nameof(SelectiveInsertionRequest), requestType);
            Assert.True(corresponds);
        }

        // ---- Helpers ----

        private static (bool Requested, string View, bool UpdateOnly, string Id, string Name, string RequestType, bool Corresponds) Capture(RackSelectiveWindow window)
        {
            var system = window.SystemToInsert;
            var design = window.DesignToInsert;
            var corresponds = system != null && design != null && Corresponds(design, system);
            return (window.InsertRequested, window.InsertView, window.UpdateOnly, window.RackId, window.RackName,
                window.Session.InsertionRequest?.GetType().Name, corresponds);
        }

        /// <summary>The payload's system is the resolution of the payload's design (the model the window actually built).</summary>
        private static bool Corresponds(SelectivePalletDesign design, SelectiveRackSystem system)
        {
            var resolved = new SelectiveGeometryResolver().Resolve(design, Catalog);
            return resolved.Bays.Count == system.Bays.Count && Math.Abs(resolved.Height - system.Height) < 1e-6;
        }

        /// <summary>A minimal but valid single-fondo selective design (one bay, two levels, floor beam).</summary>
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
