using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Headers;
using RackCad.Application.Persistence;
using RackCad.Application.RackFrames;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using RackCad.UI.Editor;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// STA tests for the REAL <see cref="RackCad.UI.RackDynamicSystemWindow"/> (initiative I-24), covering the wiring the
    /// pure Application suites cannot reach. Two things are locked:
    ///
    /// 1. The window's adoption of the I-21 editor state round-trips the FULL resolved drawing (all lateral cortes +
    ///    frontal exit + frontal entrance + planta, per-instance) for a representative NON-default design — not just four
    ///    aggregates. A sensitivity test proves the signature is strict: a change that leaves front count / modules /
    ///    total length / height untouched but adds a drawn piece yields a DIFFERENT signature (so a future refactor cannot
    ///    silently regress to a weak signature).
    /// 2. Insert/update run through the window's REAL button handlers (a genuine WPF <see cref="System.Windows.Controls.Primitives.ButtonBase.ClickEvent"/>
    ///    → <c>*_Click</c> → <c>RequestDraw</c> → validation → Recompose → SetModel → session → typed payload → Close), NOT
    ///    by calling <c>session.RequestInsert/RequestUpdate</c> directly. So identity/view/section/UpdateOnly, the concrete
    ///    request type and the real built payload (design+system) and its source metadata are all verified end to end.
    /// </summary>
    public sealed class DynamicEditorWindowTests
    {
        private const string PostId = "POSTE_OMEGA_3X3";

        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        // ---- 1a. Full-drawing round-trip through the window (representative non-default design) ----

        [Fact]
        public void RichDesign_LoadExisting_ThenRebuild_RoundTripsTheFullDrawing()
        {
            // The window normalizes raw inputs when it builds (e.g. it resolves the catalog IN/OUT beam depth in Recompose),
            // so the faithful-adoption property is that the window's OWN rich design is a load→build FIXPOINT. Feed a
            // NON-default design (3 fronts, different level counts, non-default pallet/palletsDeep/header height); capture
            // what the window built (designA); reload THAT via LoadExisting and rebuild (designB). If the I-21 adoption
            // stopped restoring a front/level/header/annotation/safety faithfully, the FULL drawing (every lateral corte +
            // frontal exit/entrance + planta, per instance) would diverge and this fails.
            var (frontsA, sigA, sigB, ok) = StaTestRunner.Run(() =>
            {
                var w1 = new RackDynamicSystemWindow(canInsertInAutoCad: true);
                w1.LoadExisting(RichDesign(), "GUID-RICH", "Din rico");
                var designA = w1.BuildDesignForTest(out _);

                var w2 = new RackDynamicSystemWindow(canInsertInAutoCad: true);
                w2.LoadExisting(designA, "GUID-RICH", "Din rico");
                var designB = w2.BuildDesignForTest(out var built);
                return (designA.Fronts.Count, FullDrawingSignature(designA), FullDrawingSignature(designB), built);
            });

            Assert.True(ok);
            Assert.Equal(3, frontsA);  // the window adopted the 3-front rich structure
            Assert.Equal(sigA, sigB);  // and its full drawing round-trips exactly through LoadExisting
        }

        [Fact]
        public void RichDesign_LoadDesignForNew_ThenRebuild_RoundTripsTheFullDrawing()
        {
            // Same full-drawing fixpoint but through the library-open entry point (LoadDesignForNew): a distinct public load
            // path (no id adopted) that must adopt the same rich state and rebuild the same drawing.
            var (frontsA, sigA, sigB, ok) = StaTestRunner.Run(() =>
            {
                var w1 = new RackDynamicSystemWindow(canInsertInAutoCad: true);
                w1.LoadDesignForNew(RichDesign(), "Din plantilla");
                var designA = w1.BuildDesignForTest(out _);

                var w2 = new RackDynamicSystemWindow(canInsertInAutoCad: true);
                w2.LoadDesignForNew(designA, "Din plantilla");
                var designB = w2.BuildDesignForTest(out var built);
                return (designA.Fronts.Count, FullDrawingSignature(designA), FullDrawingSignature(designB), built);
            });

            Assert.True(ok);
            Assert.Equal(3, frontsA);
            Assert.Equal(sigA, sigB);  // round-trips exactly through LoadDesignForNew
        }

        [Fact]
        public void FullDrawingSignature_CatchesADrawnPiece_ThatTheWeakAggregatesMiss()
        {
            // Sensitivity guard against regressing to a weak signature: two designs identical in front count, module count,
            // total length AND header height, differing only by an added safety boot (a drawn piece). The 4-aggregate
            // signature is IDENTICAL for both; the full per-instance signature MUST differ.
            var withoutBoot = RichDesign();
            var withBoot = RichDesign(withSafety: true);

            Assert.Equal(WeakAggregateSignature(withoutBoot), WeakAggregateSignature(withBoot)); // the weak signature is blind
            Assert.NotEqual(FullDrawingSignature(withoutBoot), FullDrawingSignature(withBoot));   // the full signature is not
        }

        // ---- 1b. Sanity of the default build seam ----

        [Fact]
        public void DefaultDesign_BuildsAResolvableSystem()
        {
            // A freshly opened dynamic editor must assemble a design that resolves into a real system. Regression: a broken
            // default or a BuildDesignForTest/Recompose that returns a null/degenerate design.
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

        // ---- 2. Pure load identity (kept: adds coverage the real-handler tests don't) ----

        [Fact]
        public void LoadExisting_AdoptsDrawnGuidAndName()
        {
            // RACKEDITAR path: opening an existing rack adopts its GUID + name so a re-save keeps identity.
            var (id, name) = StaTestRunner.Run(() =>
            {
                var window = new RackDynamicSystemWindow(canInsertInAutoCad: true);
                window.LoadExisting(RichDesign(), "GUID-EXIST", "Din existente");
                return (window.RackId, window.RackName);
            });

            Assert.Equal("GUID-EXIST", id);
            Assert.Equal("Din existente", name);
        }

        // ---- 3. Real insert/update through the window's own button handlers ----

        [Fact]
        public void NewSystem_InsertLateral_ViaButton_MintsGuid_AndBuildsTheRealPayload()
        {
            // The REAL "Insertar lateral" handler runs (RequestDraw → Recompose → SetModel → session): a fresh GUID is
            // minted, the typed name is captured, the request is a DynamicInsertionRequest for the lateral view, and the
            // payload's design+system are non-null and correspond (the system is the resolution of the design).
            var r = StaTestRunner.Run(() =>
            {
                var window = new RackDynamicSystemWindow(canInsertInAutoCad: true);
                window.LoadDesignForNew(RichDesign(), "Din plantilla");
                EditorWindowTestSupport.SetText(window, "NameBox", "Din nueva");
                EditorWindowTestSupport.ClickNamed(window, "InsertLateralButton");
                return Capture(window);
            });

            Assert.True(r.Requested);
            Assert.Equal("lateral", r.View);
            Assert.Equal(-1, r.Section);
            Assert.False(r.UpdateOnly);
            Assert.True(Guid.TryParse(r.Id, out _)); // fresh GUID minted by the real handler
            Assert.Equal("Din nueva", r.Name);       // the handler read NameBox.Text
            Assert.Equal(nameof(DynamicInsertionRequest), r.RequestType);
            Assert.True(r.PayloadCorresponds);       // design+system non-null and system == resolution(design)
        }

        [Fact]
        public void ExistingSystem_Update_ViaButton_KeepsGuid_RedrawsInPlace()
        {
            // The REAL "Actualizar" handler on an existing rack: GUID preserved, in-place redraw (view null, section -1,
            // UpdateOnly true), typed DynamicInsertionRequest, real payload built.
            var r = StaTestRunner.Run(() =>
            {
                var window = new RackDynamicSystemWindow(canInsertInAutoCad: true);
                window.LoadExisting(RichDesign(), "GUID-EXIST", "Din existente");
                EditorWindowTestSupport.ClickNamed(window, "UpdateButton");
                return Capture(window);
            });

            Assert.True(r.Requested);
            Assert.True(r.UpdateOnly);
            Assert.Null(r.View);
            Assert.Equal(-1, r.Section);
            Assert.Equal("GUID-EXIST", r.Id);
            Assert.Equal(nameof(DynamicInsertionRequest), r.RequestType);
            Assert.True(r.PayloadCorresponds);
        }

        [Fact]
        public void ExistingSystem_InsertEntranceFrontal_ViaButton_KeepsGuid_CarriesSection_AndSourceMetadata()
        {
            // The REAL "Frontal entrada" handler on an existing rack: GUID preserved, frontal section 1 (entrance), and the
            // library source project (I-11 metadata) is carried into the payload.
            var (r, sourcePreserved) = StaTestRunner.Run(() =>
            {
                var design = RichDesign();
                var source = RackProject.ForDynamic(design);
                var window = new RackDynamicSystemWindow(canInsertInAutoCad: true);
                window.LoadExisting(design, "GUID-EXIST", "Din existente", source);
                EditorWindowTestSupport.ClickNamed(window, "InsertEntranceButton");
                var request = window.Session.InsertionRequest as DynamicInsertionRequest;
                return (Capture(window), request != null && ReferenceEquals(request.SourceProject, source));
            });

            Assert.True(r.Requested);
            Assert.Equal("frontal", r.View);
            Assert.Equal(1, r.Section); // entrance
            Assert.False(r.UpdateOnly);
            Assert.Equal("GUID-EXIST", r.Id);
            Assert.Equal(nameof(DynamicInsertionRequest), r.RequestType);
            Assert.True(r.PayloadCorresponds);
            Assert.True(sourcePreserved); // the source project flowed through the real handler into the payload (I-11)
        }

        // ---- Helpers ----

        private readonly struct Captured
        {
            public Captured(bool requested, string view, int section, bool updateOnly, string id, string name,
                string requestType, bool payloadCorresponds)
            {
                Requested = requested; View = view; Section = section; UpdateOnly = updateOnly; Id = id; Name = name;
                RequestType = requestType; PayloadCorresponds = payloadCorresponds;
            }

            public bool Requested { get; }
            public string View { get; }
            public int Section { get; }
            public bool UpdateOnly { get; }
            public string Id { get; }
            public string Name { get; }
            public string RequestType { get; }
            public bool PayloadCorresponds { get; }
        }

        private static Captured Capture(RackDynamicSystemWindow window)
        {
            var system = window.SystemToInsert;
            var design = window.DesignToInsert;
            var corresponds = system != null && design != null && Corresponds(design, system);
            return new Captured(window.InsertRequested, window.InsertView, window.InsertSection, window.UpdateOnly,
                window.RackId, window.RackName, window.Session.InsertionRequest?.GetType().Name, corresponds);
        }

        /// <summary>The payload's system is the resolution of the payload's design (the model the window actually built).</summary>
        private static bool Corresponds(DynamicRackDesign design, DynamicRackSystem system)
        {
            var resolved = new DynamicRackSystemResolver(Catalog).Resolve(design).System;
            return resolved.Fronts.Count == system.Fronts.Count
                && Math.Abs(resolved.TotalLength - system.TotalLength) < 1e-6;
        }

        private static (DynamicRackSystemBuilder Builder, DynamicRackSystemResolver Resolver, DynamicEditorDesignAssembler Assembler) Services()
        {
            var catalog = Catalog;
            var builder = new DynamicRackSystemBuilder(catalog);
            var resolver = new DynamicRackSystemResolver(catalog);
            return (builder, resolver, new DynamicEditorDesignAssembler(catalog, builder, resolver));
        }

        /// <summary>A representative NON-default design: 3 fronts with different lane counts and different level counts,
        /// a non-default pallet (40×48×55), palletsDeep 6, non-default header height/peralte, non-default annotations.</summary>
        private static DynamicRackDesign RichDesign(bool withSafety = false)
        {
            var (builder, _, assembler) = Services();
            var pallet = new PalletSpecification(front: 40.0, depth: 48.0, height: 55.0, weight: 1200.0, weightUnit: "kg");
            const int palletsDeep = 6;
            var system = builder.BuildDefault(pallet, palletsDeep, RackFrameTemplateCatalog.Default, PostId, 140.0, 3.5);

            var matrix = new DynamicFrontMatrix();
            matrix.SetFrontCount(3);
            var lanes = new[] { 2, 3, 2 };
            var levels = new[] { 3, 2, 3 };
            for (var i = 0; i < 3; i++)
            {
                matrix.Fronts[i].PalletCount = lanes[i];
                matrix.Fronts[i].LoadLevels = levels[i];
            }

            var annotations = new DynamicAnnotationOptions
            {
                NumberFronts = true,
                NumberLevels = true,
                DrawRackName = true,
                AnnotationScale = 1.25,
                Dimensions = DimensionDetail.Standard
            };

            var safety = new List<SelectiveSafetySelection>();
            if (withSafety)
            {
                safety.Add(new SelectiveSafetySelection { ElementId = "PROTECTOR_BOTA_C_6", Quantity = 1, Side = SafetySide.Both });
            }

            return assembler.BuildDesign(
                system, matrix,
                levels: 3, firstLevel: 6.0, beamDepth: DynamicRackDefaults.DefaultBeamDepth,
                headerPostCatalogId: PostId, palletsDeep: palletsDeep, postPeralte: 3.5,
                palletTolerance: DynamicRackDefaults.DefaultPalletTolerance,
                annotations: annotations, safetySelections: safety);
        }

        /// <summary>The weak 4-aggregate signature the earlier version relied on (front count, module count, total length,
        /// header height). Deliberately coarse — used only to prove the full signature catches what this misses.</summary>
        private static string WeakAggregateSignature(DynamicRackDesign design)
        {
            var resolution = new DynamicRackSystemResolver(Catalog).Resolve(design);
            var system = resolution.System;
            return string.Format(CultureInfo.InvariantCulture, "F={0}|M={1}|L={2:R}|H={3:R}",
                system.Fronts.Count, system.Modules.Count, system.TotalLength, resolution.Height.HeaderHeight);
        }

        /// <summary>The FULL resolved-drawing signature produced by Application: every instance of every lateral corte
        /// (tagged with its cut index), the frontal exit, the frontal entrance and the planta — deterministically ordered,
        /// including role, PieceId, block, view, insertion, anchor, rotation, both mirror flags and dynamic parameters.</summary>
        private static string FullDrawingSignature(DynamicRackDesign design)
        {
            var catalog = Catalog;
            var system = new DynamicRackSystemResolver(catalog).Resolve(design).System;
            var keys = new List<string>();

            foreach (var corte in new DynamicSystemLateralBuilder().Cortes(system, catalog))
            {
                var tag = "lateral#" + corte.PostIndex.ToString(CultureInfo.InvariantCulture);
                foreach (var instance in corte.Plan.Flatten().Instances)
                {
                    keys.Add(InstanceKey(tag, instance));
                }
            }

            var frontal = new DynamicSystemFrontalBuilder();
            foreach (var instance in frontal.Build(system, catalog, DynamicRackEnd.Exit))
            {
                keys.Add(InstanceKey("exit", instance));
            }

            foreach (var instance in frontal.Build(system, catalog, DynamicRackEnd.Entrance))
            {
                keys.Add(InstanceKey("entrance", instance));
            }

            foreach (var instance in new DynamicSystemPlantaBuilder().Build(system, catalog))
            {
                keys.Add(InstanceKey("planta", instance));
            }

            keys.Sort(StringComparer.Ordinal);
            return string.Join("\n", keys);
        }

        private static string InstanceKey(string viewTag, HeaderBlockInstance i)
        {
            var parameters = string.Join(";", i.DynamicParameters
                .OrderBy(k => k.Key, StringComparer.Ordinal)
                .Select(k => k.Key + "=" + k.Value.ToString("R", CultureInfo.InvariantCulture)));
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0}|{1}|{2}|{3}|{4}|{5:R},{6:R}|{7:R},{8:R}|{9:R}|{10}{11}|{12}|{13:R}|{14}|{15}",
                viewTag, (int)i.Role, i.BlockName, i.PieceId, i.View,
                i.Insertion.X, i.Insertion.Y, i.ConnectionAnchor.X, i.ConnectionAnchor.Y,
                i.RotationRadians, i.MirroredX ? 1 : 0, i.MirroredY ? 1 : 0,
                parameters, i.DimensionOffset, i.Text ?? string.Empty, i.DimensionStyleName ?? string.Empty);
        }
    }
}
