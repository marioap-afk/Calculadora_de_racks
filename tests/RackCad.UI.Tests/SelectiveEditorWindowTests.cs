using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Headers;
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
    /// directly. <c>SelectiveEditorStateAdoptionTests</c> already locks the load→build GEOMETRY; here the orthogonal
    /// dimension is GUID/name/view/UpdateOnly conservation, the concrete request type and a STRICTLY corresponding payload:
    /// the full resolved drawing (frontal of every fondo + planta + lateral cortes) built from the payload's Design equals
    /// the one built directly from the payload's System. Deterministic: no timing, no pixels.
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
            // payload's design+system STRICTLY correspond (full drawing signature of design == of system).
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
            Assert.True(corresponds);                // full drawing signatures of design and system match
        }

        [Fact]
        public void ExistingRack_Update_ViaButton_KeepsGuidAndName_RedrawsInPlace()
        {
            // The REAL "Actualizar" handler on an existing rack: GUID + name preserved, in-place redraw (view null,
            // UpdateOnly true), typed SelectiveInsertionRequest, strictly corresponding payload.
            var (requested, view, updateOnly, id, name, requestType, corresponds) = StaTestRunner.Run(() =>
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
            Assert.Equal("Selectivo A", name);
            Assert.Equal(nameof(SelectiveInsertionRequest), requestType);
            Assert.True(corresponds);
        }

        [Fact]
        public void ExistingRack_InsertLateral_ViaButton_KeepsGuidAndName_LinkedView()
        {
            // The REAL "Insertar lateral" handler on an existing rack: a linked lateral view keeps the GUID + name and
            // carries the normalized view; typed SelectiveInsertionRequest with a strictly corresponding payload.
            var (requested, view, updateOnly, id, name, requestType, corresponds) = StaTestRunner.Run(() =>
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
            Assert.Equal("Selectivo A", name);
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

        /// <summary>Strict correspondence: the full resolved drawing built from the payload's Design equals the one built
        /// directly from the payload's System.</summary>
        private static bool Corresponds(SelectivePalletDesign design, SelectiveRackSystem system)
            => DrawingSignature(new SelectiveGeometryResolver().Resolve(design, Catalog)) == DrawingSignature(system);

        /// <summary>Full resolved-drawing signature of a selective system: frontal of every fondo + planta + lateral
        /// cortes, per instance (role, PieceId, block, view, insertion, anchor, rotation, both mirrors, dynamic params),
        /// plus the resolved height. Mirrors the pattern in SelectiveEditorStateAdoptionTests.</summary>
        private static string DrawingSignature(SelectiveRackSystem system)
        {
            var catalog = Catalog;
            var instances = new List<HeaderBlockInstance>();

            var fondoCount = SelectiveDepthLayout.Count(system);
            var frontal = new SelectiveFrontalBuilder();
            for (var fondo = 0; fondo < fondoCount; fondo++)
            {
                instances.AddRange(frontal.Build(SelectiveDepthLayout.FondoSystemView(system, fondo), catalog));
            }

            instances.AddRange(new SelectivePlantaBuilder().Build(system, catalog));
            instances.AddRange(new SelectiveLateralBuilder().Cortes(system, catalog).SelectMany(c => c.Largueros));

            // Compare the STRUCTURAL block instances only — the Annotation/Dimension decorations depend on the display
            // name the window sets on the system after resolving, so they are not reproducible from the design alone.
            var keys = instances
                .Where(i => i.Role != HeaderBlockRole.Annotation && i.Role != HeaderBlockRole.Dimension)
                .Select(InstanceKey)
                .OrderBy(s => s, StringComparer.Ordinal);
            return "H=" + system.Height.ToString("R", CultureInfo.InvariantCulture) + "\n" + string.Join("\n", keys);
        }

        private static string InstanceKey(HeaderBlockInstance i)
        {
            var parameters = string.Join(";", i.DynamicParameters
                .OrderBy(k => k.Key, StringComparer.Ordinal)
                .Select(k => k.Key + "=" + k.Value.ToString("R", CultureInfo.InvariantCulture)));
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0}|{1}|{2}|{3}|{4:R},{5:R}|{6:R},{7:R}|{8:R}|{9}{10}|{11}",
                (int)i.Role, i.BlockName, i.PieceId, i.View,
                i.Insertion.X, i.Insertion.Y, i.ConnectionAnchor.X, i.ConnectionAnchor.Y,
                i.RotationRadians, i.MirroredX ? 1 : 0, i.MirroredY ? 1 : 0, parameters);
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
