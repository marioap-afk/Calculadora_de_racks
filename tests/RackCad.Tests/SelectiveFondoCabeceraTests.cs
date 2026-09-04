using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.RackFrames;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems.Selective;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-43 Gate 4: a custom cabecera is an authority of <c>(FondoIndex, PostIndex)</c>, not of the post alone. These
    /// tests cover the model and its single resolver, the multi-fondo write (deep copy per target, omission where the
    /// post does not exist), the destructive resize policy, the additive persistence with its legacy meaning, and the
    /// consumers — lateral, planta and BOM. Pure: no WPF, no AutoCAD.
    /// </summary>
    public class SelectiveFondoCabeceraTests
    {
        private const string PostId = TestCatalogIds.Profiles.Posts.Standard;
        private const string BeamId = TestCatalogIds.Profiles.Beams.SelectiveThreeRivet;

        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        /// <summary>A recognizable custom cabecera: its height is the marker the drawing/BOM tests look for.</summary>
        private static RackFrameConfiguration Custom(double height)
            => new RackFrameConfigurationFactory(Catalog).Build(
                RackFrameTemplateCatalog.FindStandardOrDefault(), PostId, height, 42.0);

        private static SelectiveBayDesign Bay(int levels = 2)
        {
            var bay = new SelectiveBayDesign();
            for (var l = 0; l < levels; l++)
            {
                bay.Levels.Add(new SelectiveCell
                {
                    Pallet = new Tarima { Frente = 42.0, Alto = 48.0 },
                    PalletCount = 2,
                    BeamId = BeamId,
                    BeamPeralte = 4.0
                });
            }

            return bay;
        }

        /// <summary>A design of <paramref name="fondoFrentes"/> fondos, each with its own frente count.</summary>
        private static SelectivePalletDesign Design(params int[] fondoFrentes)
        {
            var design = new SelectivePalletDesign
            {
                PostId = PostId,
                PostPeralte = 3.0,
                PalletTolerance = 4.0,
                VerticalClearance = 6.0,
                FloorBeamRise = 4.0,
                PalletDepth = 48.0,
                DepthCount = fondoFrentes.Length
            };

            for (var b = 0; b < fondoFrentes[0]; b++) design.Bays.Add(Bay());
            for (var k = 1; k < fondoFrentes.Length; k++)
            {
                var bays = new List<SelectiveBayDesign>();
                for (var b = 0; b < fondoFrentes[k]; b++) bays.Add(Bay());
                design.ExtraFondoBays.Add(bays);
                design.ExtraFondoDepths.Add(0.0);
            }

            return design;
        }

        /// <summary>Store a custom cabecera at (fondo, post) on a DESIGN, growing the rows as the editor would.</summary>
        private static void SetCustom(SelectivePalletDesign design, int fondo, int post, RackFrameConfiguration configuration)
        {
            if (fondo == 0)
            {
                while (design.PostCabeceras.Count <= post) design.PostCabeceras.Add(null);
                design.PostCabeceras[post] = configuration;
                return;
            }

            while (design.ExtraFondoPostCabeceras.Count < fondo) design.ExtraFondoPostCabeceras.Add(new List<RackFrameConfiguration>());
            var row = design.ExtraFondoPostCabeceras[fondo - 1];
            while (row.Count <= post) row.Add(null);
            row[post] = configuration;
        }

        private static SelectiveRackSystem Resolve(SelectivePalletDesign design)
            => new SelectiveGeometryResolver().Resolve(design, Catalog);

        // ---- The single authority ----

        [Fact]
        public void TheAuthority_ReadsFondoZeroFromTheLegacyRow_AndTheOthersFromTheirOwn()
        {
            var design = Design(2, 2, 2);
            SetCustom(design, 0, 1, Custom(200.0));
            SetCustom(design, 2, 1, Custom(300.0));
            var system = Resolve(design);

            Assert.Equal(200.0, SelectiveCabeceraAuthority.EffectiveCustomAt(system, 0, 1).Height);
            Assert.Null(SelectiveCabeceraAuthority.EffectiveCustomAt(system, 1, 1));
            Assert.Equal(300.0, SelectiveCabeceraAuthority.EffectiveCustomAt(system, 2, 1).Height);
            Assert.Null(SelectiveCabeceraAuthority.EffectiveCustomAt(system, 0, 0)); // no custom at that post
        }

        [Fact]
        public void TheAuthority_TreatsAMissingRow_AShortRow_AndANullEntry_AllAsStandard()
        {
            var design = Design(2, 2);
            design.ExtraFondoPostCabeceras.Add(new List<RackFrameConfiguration> { null }); // short row + null entry
            var system = Resolve(design);

            Assert.Null(SelectiveCabeceraAuthority.CustomAt(system, 1, 0)); // null entry
            Assert.Null(SelectiveCabeceraAuthority.CustomAt(system, 1, 2)); // beyond the row
            Assert.Null(SelectiveCabeceraAuthority.CustomAt(system, 5, 0)); // no such fondo
            Assert.Null(SelectiveCabeceraAuthority.CustomAt(system, 1, -1));
        }

        [Fact]
        public void AShortFondo_NeverGainsACabeceraAtAPostItDoesNotReach()
        {
            // Fondo 1 has ONE frente, so it has posts 0 and 1 only. A stored row longer than that must not survive
            // resolution: a phantom cabecera at post 2 would be drawn and counted where the rack has no post.
            var design = Design(3, 1);
            design.ExtraFondoPostCabeceras.Add(new List<RackFrameConfiguration> { null, null, Custom(400.0) });
            var system = Resolve(design);

            Assert.Null(SelectiveCabeceraAuthority.EffectiveCustomAt(system, 1, 2));
        }

        // ---- The editor state: apply / reset over the target fondos ----

        private static SelectiveEditorState StateWith(params int[] fondoFrentes)
        {
            var state = new SelectiveEditorState { DefaultBeamId = BeamId };
            foreach (var frentes in fondoFrentes)
            {
                state.InitMatrix(frentes, 2);
                state.FondoMatrices.Add(state.SnapshotWorking(48.0, 0.0));
            }

            state.SelectedFondo = 0;
            state.LoadFondo(0);
            state.SetTargetFondos(new[] { 0 });
            state.SyncPostCabeceras();
            return state;
        }

        private static RackFrameConfiguration DeepCopy(RackFrameConfiguration c)
            => new RackFrameProjectStore().DeepCopy(c);

        [Fact]
        public void ApplyingToSeveralFondos_GivesEachAnIndependentCopy_NeverTheSameInstance()
        {
            var state = StateWith(3, 3, 3, 3);
            state.SetTargetFondos(new[] { 0, 1, 3 });
            var source = Custom(180.0);

            var result = state.ApplyCabeceraToTargets(1, source, DeepCopy);

            Assert.Equal(new[] { 0, 1, 3 }, result.AppliedFondos);
            var a = state.CabeceraAt(0, 1);
            var b = state.CabeceraAt(1, 1);
            var c = state.CabeceraAt(3, 1);
            Assert.NotNull(a);
            Assert.NotSame(source, a);
            Assert.NotSame(a, b);
            Assert.NotSame(b, c);

            // Editing one afterwards must not move the others — the aliasing this deep copy exists to prevent.
            a.Height = 999.0;
            Assert.Equal(180.0, b.Height);
            Assert.Equal(180.0, c.Height);
            Assert.Equal(180.0, source.Height);
        }

        [Fact]
        public void ATargetWhereThePostDoesNotExist_IsOmittedAndReported_NeverPadded()
        {
            // Fondo 1 has one frente: posts 0 and 1. Post 3 exists only in the three-frente fondos.
            var state = StateWith(3, 1, 3);
            state.SetTargetFondos(new[] { 0, 1, 2 });

            var result = state.ApplyCabeceraToTargets(3, Custom(150.0), DeepCopy);

            Assert.Equal(new[] { 0, 2 }, result.AppliedFondos);
            Assert.Equal(new[] { 1 }, result.OmittedFondos);
            Assert.Null(state.CabeceraAt(1, 3));
            Assert.Contains("no llega", result.Describe(reset: false));
        }

        [Fact]
        public void ResettingOverASubsetOfFondos_ClearsOnlyThoseCabeceras()
        {
            var state = StateWith(2, 2, 2);
            state.SetTargetFondos(new[] { 0, 1, 2 });
            state.ApplyCabeceraToTargets(1, Custom(160.0), DeepCopy);

            state.SetTargetFondos(new[] { 1 });
            var result = state.ApplyCabeceraToTargets(1, null, DeepCopy);

            Assert.Equal(new[] { 1 }, result.AppliedFondos);
            Assert.NotNull(state.CabeceraAt(0, 1));
            Assert.Null(state.CabeceraAt(1, 1));
            Assert.NotNull(state.CabeceraAt(2, 1));
        }

        [Fact]
        public void TheCabeceraAxis_NeverTouchesPostPeraltes_WhichStayGlobalByPost()
        {
            var state = StateWith(2, 2);
            state.SyncPostCabeceras();
            state.PostPeraltes[1] = 7.0;
            state.SetTargetFondos(new[] { 1 });

            state.ApplyCabeceraToTargets(1, Custom(170.0), DeepCopy);
            state.ApplyCabeceraToTargets(1, null, DeepCopy);

            Assert.Equal(7.0, state.PostPeraltes[1]); // a per-fondo cabecera operation cannot clear a global override
        }

        // ---- Resize: destructive, no resurrection ----

        [Fact]
        public void ReducingTheFondoCount_DropsTheCabecerasOfTheFondosThatDisappear()
        {
            var state = StateWith(2, 2, 2);
            state.SetTargetFondos(new[] { 0, 1, 2 });
            state.ApplyCabeceraToTargets(1, Custom(190.0), DeepCopy);

            state.FondoMatrices.RemoveAt(2);
            state.SyncPostCabeceras();

            Assert.Single(state.ExtraFondoPostCabeceras); // only fondo 1 remains
            Assert.NotNull(state.CabeceraAt(1, 1));
            Assert.Null(state.CabeceraAt(2, 1));
        }

        [Fact]
        public void ShrinkingAFondo_DropsTheOverridesOfPostsItNoLongerHas_AndRegrowingDoesNotReviveThem()
        {
            var state = StateWith(3, 3);
            state.SetTargetFondos(new[] { 1 });
            state.ApplyCabeceraToTargets(3, Custom(210.0), DeepCopy);
            Assert.NotNull(state.CabeceraAt(1, 3));

            // Fondo 1 shrinks to one frente: posts 2 and 3 stop existing.
            state.FondoMatrices[1].Bays.RemoveAt(2);
            state.FondoMatrices[1].Bays.RemoveAt(1);
            state.SyncPostCabeceras();
            Assert.Null(state.CabeceraAt(1, 3));

            // Growing back gives a STANDARD slot, not the old configuration.
            state.FondoMatrices[1].Bays.Add(state.FondoMatrices[1].Bays[0]);
            state.FondoMatrices[1].Bays.Add(state.FondoMatrices[1].Bays[0]);
            state.SyncPostCabeceras();
            Assert.Null(state.CabeceraAt(1, 3));
        }

        [Fact]
        public void FondoZeroKeepsTheMasterSizing_ButLosesEntriesBeyondItsOwnPosts()
        {
            var state = StateWith(3, 3);
            state.SetTargetFondos(new[] { 0 });
            state.ApplyCabeceraToTargets(3, Custom(220.0), DeepCopy);

            state.Bays.RemoveAt(2); // the working matrix IS fondo 0: it drops to two frentes
            state.SyncPostCabeceras();

            Assert.Null(state.CabeceraAt(0, 3)); // no phantom, and nothing to resurrect
        }

        // ---- Persistence: additive, with the legacy meaning preserved ----

        [Fact]
        public void RoundTrip_PreservesDifferentCustomsInFondos0And1And3()
        {
            var design = Design(2, 2, 2, 2);
            SetCustom(design, 0, 1, Custom(240.0));
            SetCustom(design, 1, 2, Custom(252.0));
            SetCustom(design, 3, 0, Custom(264.0));

            var restored = RoundTrip(design, out var id);

            Assert.Equal("GUID-CAB", id);
            Assert.Equal(240.0, restored.PostCabeceras[1].Height);
            Assert.Equal(252.0, restored.ExtraFondoPostCabeceras[0][2].Height);
            Assert.Equal(264.0, restored.ExtraFondoPostCabeceras[2][0].Height);
            Assert.Null(restored.ExtraFondoPostCabeceras[1].FirstOrDefault(c => c != null)); // fondo 2 stayed standard
        }

        [Fact]
        public void RoundTrip_ToleratesNullRowsAndShortRows()
        {
            var design = Design(2, 2, 2);
            design.ExtraFondoPostCabeceras.Add(null);
            design.ExtraFondoPostCabeceras.Add(new List<RackFrameConfiguration> { null, Custom(276.0) });

            var restored = RoundTrip(design, out _);

            Assert.Empty(restored.ExtraFondoPostCabeceras[0]);
            Assert.Equal(276.0, restored.ExtraFondoPostCabeceras[1][1].Height);
        }

        [Fact]
        public void AfterLoad_EachFondoHoldsAnIndependentConfiguration()
        {
            var shared = Custom(288.0);
            var design = Design(2, 2, 2);
            SetCustom(design, 1, 0, shared);
            SetCustom(design, 2, 0, shared); // the SAME instance on both fondos before saving

            var restored = RoundTrip(design, out _);

            var a = restored.ExtraFondoPostCabeceras[0][0];
            var b = restored.ExtraFondoPostCabeceras[1][0];
            Assert.NotSame(a, b);
            a.Height = 999.0;
            Assert.Equal(288.0, b.Height); // persistence rebuilt them separately
        }

        [Fact]
        public void ALegacyDocument_WithOnlyPostCabeceras_CustomizesFondoZeroAlone()
        {
            // Exactly the pre-I-43 shape: the new field is absent from the JSON.
            var design = Design(2, 2, 2);
            SetCustom(design, 0, 1, Custom(300.0));
            var document = SelectivePalletDesignDocument.From(design, "GUID-LEGACY", "Legacy");
            document.ExtraFondoPostCabeceras = null;
            var json = new SelectivePalletDesignStore().Serialize(document);

            var restored = new SelectivePalletDesignStore().Deserialize(json).ToDomain();

            Assert.Equal(300.0, restored.PostCabeceras[1].Height);
            Assert.Empty(restored.ExtraFondoPostCabeceras); // NOT propagated: fondos 1..N stay standard
            var system = Resolve(restored);
            Assert.Null(SelectiveCabeceraAuthority.EffectiveCustomAt(system, 1, 1));
            Assert.Null(SelectiveCabeceraAuthority.EffectiveCustomAt(system, 2, 1));
        }

        [Fact]
        public void ADesignWithNoCustomAtAll_DoesNotEvenWriteTheNewField()
        {
            var document = SelectivePalletDesignDocument.From(Design(2, 2), "GUID-PLAIN", "Plain");
            Assert.Null(document.ExtraFondoPostCabeceras); // no churn in the JSON of every existing rack
        }

        private static SelectivePalletDesign RoundTrip(SelectivePalletDesign design, out string id)
        {
            var store = new SelectivePalletDesignStore();
            var json = store.Serialize(SelectivePalletDesignDocument.From(design, "GUID-CAB", "Cabeceras"));
            var document = store.Deserialize(json);
            id = document.Id;
            return document.ToDomain();
        }

        // ---- Depth: the FONDO owns it, a custom cabecera never becomes a second authority ----

        /// <summary>A state of <paramref name="depths"/> fondos, each with its own pallet depth (so each has its own
        /// derived cabecera depth: tarima - 6").</summary>
        private static SelectiveEditorState StateWithDepths(params double[] depths)
        {
            var state = new SelectiveEditorState { DefaultBeamId = BeamId };
            foreach (var depth in depths)
            {
                state.InitMatrix(2, 2);
                state.FondoMatrices.Add(state.SnapshotWorking(depth, 0.0));
            }

            state.SelectedFondo = 0;
            state.LoadFondo(0);
            state.SetTargetFondos(new[] { 0 });
            state.SyncPostCabeceras();
            return state;
        }

        /// <summary>The editor inputs matching a state built by <see cref="StateWithDepths"/>.</summary>
        private static SelectiveDesignInputs Inputs(SelectiveEditorState state)
            => new SelectiveDesignInputs
            {
                PostId = PostId,
                PostPeralte = 3.0,
                PalletTolerance = 4.0,
                VerticalClearance = 6.0,
                FloorBeamRise = 4.0,
                Fondo = state.FondoMatrices[0].Depth,
                DepthCount = state.FondoMatrices.Count,
                WorkingDepth = state.FondoMatrices[state.SelectedFondo].Depth,
                WorkingCabeceraOverride = state.FondoMatrices[state.SelectedFondo].CabeceraOverride,
                Separators = new List<double>()
            };

        [Fact]
        public void CustomizingOnce_GivesEachTargetFondoItsOwnDepth_Immediately()
        {
            // Fondo 0 draws its cabecera at 42 (48 - 6) and fondo 1 at 54 (60 - 6). ONE customization aimed at both
            // must land with each fondo's own depth, not with the depth of the fondo it was authored on.
            var state = StateWithDepths(48.0, 60.0);
            state.SetTargetFondos(new[] { 0, 1 });

            var authored = Custom(400.0);
            authored.Depth = state.CabeceraDepthOfFondo(0); // as the editor seeds it, from the VISIBLE fondo
            state.ApplyCabeceraToTargets(1, authored, DeepCopy);

            Assert.Equal(42.0, state.CabeceraAt(0, 1).Depth, 4);
            Assert.Equal(54.0, state.CabeceraAt(1, 1).Depth, 4);
            Assert.Equal(400.0, state.CabeceraAt(0, 1).Height, 4); // the rest of the recipe is preserved
            Assert.Equal(400.0, state.CabeceraAt(1, 1).Height, 4);
            Assert.NotSame(state.CabeceraAt(0, 1), state.CabeceraAt(1, 1));
        }

        [Fact]
        public void ThoseDepthsSurviveBuildDesignAndResolve()
        {
            var state = StateWithDepths(48.0, 60.0);
            state.SetTargetFondos(new[] { 0, 1 });
            var authored = Custom(400.0);
            authored.Depth = state.CabeceraDepthOfFondo(0);
            state.ApplyCabeceraToTargets(1, authored, DeepCopy);

            var system = Resolve(state.BuildDesign(Inputs(state)));

            Assert.Equal(42.0, SelectiveCabeceraAuthority.EffectiveCustomAt(system, 0, 1).Depth, 4);
            Assert.Equal(54.0, SelectiveCabeceraAuthority.EffectiveCustomAt(system, 1, 1).Depth, 4);
        }

        [Fact]
        public void ThoseDepthsSurviveASaveAndLoad_SoTheDrawingDoesNotChange()
        {
            var state = StateWithDepths(48.0, 60.0);
            state.SetTargetFondos(new[] { 0, 1 });
            var authored = Custom(400.0);
            authored.Depth = state.CabeceraDepthOfFondo(0);
            state.ApplyCabeceraToTargets(1, authored, DeepCopy);

            var before = Resolve(state.BuildDesign(Inputs(state)));
            var beforeDepths = new[]
            {
                SelectiveCabeceraAuthority.EffectiveCustomAt(before, 0, 1).Depth,
                SelectiveCabeceraAuthority.EffectiveCustomAt(before, 1, 1).Depth
            };

            var after = Resolve(RoundTrip(state.BuildDesign(Inputs(state)), out _));
            var afterDepths = new[]
            {
                SelectiveCabeceraAuthority.EffectiveCustomAt(after, 0, 1).Depth,
                SelectiveCabeceraAuthority.EffectiveCustomAt(after, 1, 1).Depth
            };

            Assert.Equal(new[] { 42.0, 54.0 }, beforeDepths);
            Assert.Equal(beforeDepths, afterDepths);
            Assert.Equal(BomSignature(before), BomSignature(after)); // and the BOM is the same before and after
        }

        [Fact]
        public void ApplyingFromTheDeepFondoTowardsTheShallowOne_GivesTheSameResult()
        {
            var state = StateWithDepths(48.0, 60.0);
            state.SelectFondo(1);
            state.LoadFondo(1);
            state.SetTargetFondos(new[] { 0, 1 });

            var authored = Custom(400.0);
            authored.Depth = state.CabeceraDepthOfFondo(1); // authored on the DEEP fondo this time
            state.ApplyCabeceraToTargets(1, authored, DeepCopy);

            Assert.Equal(42.0, state.CabeceraAt(0, 1).Depth, 4);
            Assert.Equal(54.0, state.CabeceraAt(1, 1).Depth, 4);
        }

        [Fact]
        public void ChangingAFondosCabeceraOverride_MovesOnlyThatFondosCustomDepth()
        {
            var design = Design(2, 2);
            design.ExtraFondoDepths[0] = 60.0;
            SetCustom(design, 0, 1, Custom(410.0));
            SetCustom(design, 1, 1, Custom(410.0));

            // Fondo 1 gets an explicit cabecera override; fondo 0 keeps the rule.
            design.CabeceraFondoOverrides.Add(0.0);
            design.CabeceraFondoOverrides.Add(37.0);
            var system = Resolve(design);

            Assert.Equal(42.0, SelectiveCabeceraAuthority.EffectiveCustomAt(system, 0, 1).Depth, 4);
            Assert.Equal(37.0, SelectiveCabeceraAuthority.EffectiveCustomAt(system, 1, 1).Depth, 4);
        }

        [Fact]
        public void AFondoWithoutACustom_KeepsDerivingItsCabeceraDepthNormally()
        {
            var design = Design(2, 2);
            design.ExtraFondoDepths[0] = 60.0;
            SetCustom(design, 0, 1, Custom(420.0));
            var system = Resolve(design);

            Assert.Equal(42.0, SelectiveCabeceraAuthority.EffectiveCustomAt(system, 0, 1).Depth, 4);
            Assert.Null(SelectiveCabeceraAuthority.EffectiveCustomAt(system, 1, 1));
            Assert.Equal(54.0, SelectiveDepthLayout.CabeceraDepthOfFondo(system, 1), 4); // still the rule
        }

        [Fact]
        public void TheDepthRuleNeverAliasesTwoFondosOntoOneConfiguration()
        {
            var state = StateWithDepths(48.0, 60.0);
            state.SetTargetFondos(new[] { 0, 1 });
            state.ApplyCabeceraToTargets(1, Custom(430.0), DeepCopy);

            state.CabeceraAt(0, 1).Height = 999.0;

            Assert.Equal(430.0, state.CabeceraAt(1, 1).Height, 4);
            Assert.Equal(42.0, state.CabeceraAt(0, 1).Depth, 4);
            Assert.Equal(54.0, state.CabeceraAt(1, 1).Depth, 4);
        }

        [Fact]
        public void ALegacyDocumentKeepsItsExactDepthBehaviour()
        {
            // Only fondo 0 is customized, and its cabecera depth is the rule for fondo 0 — exactly what the drawing
            // showed before this axis existed.
            var design = Design(2, 2);
            design.ExtraFondoDepths[0] = 60.0;
            SetCustom(design, 0, 1, Custom(440.0));
            var document = SelectivePalletDesignDocument.From(design, "GUID-LEG-DEPTH", "Legacy");
            document.ExtraFondoPostCabeceras = null;
            var store = new SelectivePalletDesignStore();

            var restored = store.Deserialize(store.Serialize(document)).ToDomain();
            var system = Resolve(restored);

            Assert.Equal(42.0, SelectiveCabeceraAuthority.EffectiveCustomAt(system, 0, 1).Depth, 4);
            Assert.Null(SelectiveCabeceraAuthority.EffectiveCustomAt(system, 1, 1));
        }


        // ---- Consumers: lateral, planta, BOM ----

        /// <summary>The tallest point any instance of a corte reaches. A custom cabecera taller than the derived one
        /// pushes this up, which is how a fondo other than the anchor proves it drew ITS OWN configuration: the corte
        /// exposes only the anchor fondo's cabecera directly, the rest arrive as translated instances.</summary>
        private static double TallestInCorte(SelectiveRackSystem system, int postIndex)
        {
            var corte = new SelectiveLateralBuilder().Cortes(system, Catalog).First(c => c.PostIndex == postIndex);
            return corte.Largueros.Count == 0 ? 0.0 : corte.Largueros.Max(instance => instance.Insertion.Y);
        }

        [Fact]
        public void Lateral_DrawsTheCustomCabeceraOfFondoZero_WhileFondoOneStaysStandard()
        {
            var design = Design(2, 2);
            SetCustom(design, 0, 1, Custom(310.0));

            var corte = new SelectiveLateralBuilder().Cortes(Resolve(design), Catalog).First(c => c.PostIndex == 1);

            Assert.Equal(310.0, corte.Cabecera.Height, 4); // the anchor cabecera IS fondo 0's custom
        }

        [Fact]
        public void Lateral_DrawsTheCustomCabeceraOfFondoOne_WhileFondoZeroStaysStandard()
        {
            // Before I-43 this could not even be expressed: a custom on fondo 1 was impossible, so the lateral drew a
            // derived cabecera there. Fondo 0 still anchors the corte, so the custom shows up in the translated set.
            var plain = Resolve(Design(2, 2));
            var design = Design(2, 2);
            SetCustom(design, 1, 1, Custom(320.0));
            var customized = Resolve(design);

            var corte = new SelectiveLateralBuilder().Cortes(customized, Catalog).First(c => c.PostIndex == 1);

            Assert.NotEqual(320.0, corte.Cabecera.Height); // fondo 0 kept its derived cabecera
            Assert.True(TallestInCorte(customized, 1) > TallestInCorte(plain, 1) + 100.0);
        }

        [Fact]
        public void Lateral_DrawsDifferentCustomsInFondosOneAndThree()
        {
            var onlyOne = Design(2, 2, 2, 2);
            SetCustom(onlyOne, 1, 1, Custom(330.0));

            var both = Design(2, 2, 2, 2);
            SetCustom(both, 1, 1, Custom(330.0));
            SetCustom(both, 3, 1, Custom(360.0));

            // Adding the SECOND custom, on another fondo, must change the corte again: the two are independent.
            Assert.True(TallestInCorte(Resolve(both), 1) > TallestInCorte(Resolve(onlyOne), 1) + 20.0);
        }

        [Fact]
        public void Planta_NeverSharesACustomGroup_EvenBetweenIdenticalTwinsOnTwoFondos()
        {
            var plain = new SelectivePlantaBuilder().BuildPlan(Resolve(Design(2, 2, 2)), Catalog).Headers.Count;

            var design = Design(2, 2, 2);
            SetCustom(design, 1, 1, Custom(350.0));
            SetCustom(design, 2, 1, Custom(350.0)); // identical twins on two different fondos
            var withTwins = new SelectivePlantaBuilder().BuildPlan(Resolve(design), Catalog).Headers.Count;

            // TWO extra groups, not one: editing one cabecera afterwards must not move the other.
            Assert.Equal(plain + 2, withTwins);
        }

        [Fact]
        public void Planta_UsesFondoOnesOwnConfiguration_WithoutTouchingFondoZero()
        {
            var plain = new SelectivePlantaBuilder().BuildPlan(Resolve(Design(2, 2)), Catalog).Headers.Count;

            var design = Design(2, 2);
            SetCustom(design, 1, 0, Custom(355.0));

            Assert.Equal(plain + 1, new SelectivePlantaBuilder().BuildPlan(Resolve(design), Catalog).Headers.Count);
        }

        /// <summary>Every BOM line as a comparable string, so two racks can be compared exactly.</summary>
        private static string BomSignature(SelectiveRackSystem system)
            => string.Join("|", SelectiveBomBuilder.Build(system, Catalog).Lines
                .Select(line => line.Category + ":" + line.ProfileId + ":" + line.Description + ":" + line.Length.ToString("0.###") + ":" + line.Quantity)
                .OrderBy(text => text, System.StringComparer.Ordinal));

        [Fact]
        public void Bom_ChangesWhenAFondoOtherThanZeroGetsItsOwnCabecera()
        {
            var plain = Resolve(Design(2, 2));
            var design = Design(2, 2);
            SetCustom(design, 1, 1, Custom(365.0));

            Assert.NotEqual(BomSignature(plain), BomSignature(Resolve(design)));
        }

        [Fact]
        public void Bom_DoesNotChangeWhenTheCustomIsNotAnEffectiveOverride()
        {
            // A stored configuration without a usable height is not an override, so the BOM must count the derived
            // cabecera exactly as before — the recipe did not change.
            var plain = Resolve(Design(2, 2));
            var design = Design(2, 2);
            SetCustom(design, 1, 1, new RackFrameConfiguration()); // Height == 0

            Assert.Equal(BomSignature(plain), BomSignature(Resolve(design)));
        }

        [Fact]
        public void Bom_DoesNotCountAPhantomCabeceraForAPostAShortFondoDoesNotReach()
        {
            var plain = Resolve(Design(3, 1));
            var design = Design(3, 1);
            // Fondo 1 has ONE frente (posts 0 and 1); a stored row reaching post 2 must change nothing.
            design.ExtraFondoPostCabeceras.Add(new List<RackFrameConfiguration> { null, null, Custom(370.0) });

            Assert.Equal(BomSignature(plain), BomSignature(Resolve(design)));
        }

        [Fact]
        public void LateralAndPlantaAndBom_ObserveEachFondosOwnCabeceraDepth()
        {
            // The same customization on two fondos of different depth: the drawing and the BOM must see 42 and 54,
            // never twice the depth of the fondo it was authored on.
            var design = Design(2, 2);
            design.ExtraFondoDepths[0] = 60.0;
            SetCustom(design, 0, 1, Custom(450.0));
            SetCustom(design, 1, 1, Custom(450.0));
            var system = Resolve(design);

            // Reading through the authority is what every consumer does, and it is what imposes the depth.
            var corte = new SelectiveLateralBuilder().Cortes(system, Catalog).First(c => c.PostIndex == 1);
            Assert.Equal(42.0, corte.Cabecera.Depth, 4); // the anchor is fondo 0

            Assert.Equal(42.0, SelectiveCabeceraAuthority.EffectiveCustomAt(system, 0, 1).Depth, 4);
            Assert.Equal(54.0, SelectiveCabeceraAuthority.EffectiveCustomAt(system, 1, 1).Depth, 4);

            // Planta and BOM run over the same authority, so building them must not change those depths back.
            new SelectivePlantaBuilder().BuildPlan(system, Catalog);
            SelectiveBomBuilder.Build(system, Catalog);
            Assert.Equal(42.0, SelectiveCabeceraAuthority.EffectiveCustomAt(system, 0, 1).Depth, 4);
            Assert.Equal(54.0, SelectiveCabeceraAuthority.EffectiveCustomAt(system, 1, 1).Depth, 4);
        }

        [Fact]
        public void Frontal_RepresentsItsOwnFondo_NotTheMasterFondoZero()
        {
            // REESCRITO AL CONTRARIO (I-43, gate 8.6F, O-43-03). Este test afirmaba que el frontal colapsa el eje de
            // fondos y por eso NO podia tomar la cabecera de un fondo 1..N. La decision del dueno es la opuesta: cada
            // frontal Fk representa FISICAMENTE al fondo k, asi que dibuja la custom de (k, i). No hay superposicion
            // que temer — cada frontal es el de UN fondo — ni eleccion arbitraria: la vista es la de ese fondo.
            var design = Design(2, 2);
            SetCustom(design, 1, 1, Custom(380.0));
            var system = Resolve(design);

            var view = SelectiveDepthLayout.FondoSystemView(system, 1);

            Assert.Contains(view.PostCabeceras, c => c != null && c.Height == 380.0);
            // Y no se contagia: el fondo 0 sigue sin esa cabecera.
            Assert.DoesNotContain(
                SelectiveDepthLayout.FondoSystemView(system, 0).PostCabeceras,
                c => c != null && c.Height == 380.0);
        }
    }
}
