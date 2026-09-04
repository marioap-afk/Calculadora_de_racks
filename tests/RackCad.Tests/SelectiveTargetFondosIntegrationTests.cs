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
    /// I-43 gate 7: the closing pass. Every property that repeats per fondo or per frente is addressable through the
    /// ONE <c>TargetFondos</c> axis, with the inner authority each property really has, and the whole initiative holds
    /// together across topology changes, restore, save/load and resize. Pure: no WPF, no AutoCAD.
    /// </summary>
    public class SelectiveTargetFondosIntegrationTests
    {
        private const string PostId = TestCatalogIds.Profiles.Posts.Standard;
        private const string BeamId = TestCatalogIds.Profiles.Beams.SelectiveThreeRivet;

        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        /// <summary>A state of fondos with their own frente counts, each frente two levels. Fondo 0 is loaded.</summary>
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

        private static SelectiveDesignInputs Inputs(SelectiveEditorState state)
            => new SelectiveDesignInputs
            {
                PostId = PostId,
                PostPeralte = 3.0,
                PalletTolerance = 4.0,
                VerticalClearance = 6.0,
                FloorBeamRise = 4.0,
                Fondo = 48.0,
                DepthCount = state.FondoMatrices.Count,
                WorkingDepth = state.FondoMatrices[state.SelectedFondo].Depth,
                WorkingCabeceraOverride = state.FondoMatrices[state.SelectedFondo].CabeceraOverride,
                Separators = new List<double>()
            };

        private static RackFrameConfiguration Custom(double height)
            => new RackFrameConfigurationFactory(Catalog).Build(
                RackFrameTemplateCatalog.FindStandardOrDefault(), PostId, height, 42.0);

        private static RackFrameConfiguration DeepCopy(RackFrameConfiguration c)
            => new RackFrameProjectStore().DeepCopy(c);

        private static bool FloorBeamAt(SelectiveEditorState state, int fondo, int front)
            => fondo == state.SelectedFondo ? state.FloorBeams[front] : state.FondoMatrices[fondo].FloorBeams[front];

        private static double? HeightAt(SelectiveEditorState state, int fondo, int front)
            => fondo == state.SelectedFondo ? state.BayHeights[front] : state.FondoMatrices[fondo].BayHeights[front];

        private static List<SelectiveSegment> SegmentsAt(SelectiveEditorState state, int fondo, int front)
            => fondo == state.SelectedFondo ? state.BaySegments[front] : state.FondoMatrices[fondo].BaySegments[front];

        // ---- A. Frente-wide properties over TargetFondos ----

        [Fact]
        public void FloorBeam_ReachesTheFrenteOfEveryTargetFondo_AndOnlyThose()
        {
            var state = StateWith(3, 3, 3, 3);
            state.SetTargetFondos(new[] { 0, 2 });

            var result = state.ApplyFloorBeamToTargets(SelectiveFrontApplyScope.Front, 1, true);

            Assert.Equal(new[] { (0, 1), (2, 1) }, result.Applied);
            Assert.True(FloorBeamAt(state, 0, 1));
            Assert.False(FloorBeamAt(state, 1, 1));
            Assert.True(FloorBeamAt(state, 2, 1));
            Assert.False(FloorBeamAt(state, 3, 1));
            Assert.False(FloorBeamAt(state, 0, 0)); // other frentes untouched
        }

        [Fact]
        public void FloorBeam_All_ReachesEveryFrenteOfTheTargets()
        {
            var state = StateWith(2, 2);
            state.SetTargetFondos(new[] { 1 });

            state.ApplyFloorBeamToTargets(SelectiveFrontApplyScope.All, 0, true);

            Assert.True(FloorBeamAt(state, 1, 0));
            Assert.True(FloorBeamAt(state, 1, 1));
            Assert.False(FloorBeamAt(state, 0, 0));
        }

        [Fact]
        public void BayHeight_ReachesTheTargets_AndNullRestoresTheDerivedHeight()
        {
            var state = StateWith(2, 2, 2);
            state.SetTargetFondos(new[] { 0, 2 });

            state.ApplyBayHeightToTargets(SelectiveFrontApplyScope.Front, 1, 210.0);
            Assert.Equal(210.0, HeightAt(state, 0, 1));
            Assert.Null(HeightAt(state, 1, 1));
            Assert.Equal(210.0, HeightAt(state, 2, 1));

            state.SetTargetFondos(new[] { 2 });
            var restore = state.ApplyBayHeightToTargets(SelectiveFrontApplyScope.Front, 1, null);

            Assert.Equal(new[] { (2, 1) }, restore.Applied);
            Assert.Equal(210.0, HeightAt(state, 0, 1)); // untouched
            Assert.Null(HeightAt(state, 2, 1));
        }

        [Fact]
        public void Segments_AreProjectedOntoTheSameFrenteOfEveryTarget_WithNoAliasing()
        {
            var state = StateWith(3, 3, 3);
            state.SetTargetFondos(new[] { 0, 2 });
            var authored = new[]
            {
                new SelectiveSegment { Length = 40.0, Loaded = true },
                new SelectiveSegment { Length = 0.0, Loaded = false }
            };

            var result = state.ApplySegmentsToTargets(1, authored);

            Assert.Equal(new[] { (0, 1), (2, 1) }, result.Applied);
            Assert.Equal(2, SegmentsAt(state, 0, 1).Count);
            Assert.Equal(2, SegmentsAt(state, 2, 1).Count);
            Assert.Empty(SegmentsAt(state, 1, 1)); // not a target

            // Independent copies, and independent of the authored list too.
            Assert.NotSame(SegmentsAt(state, 0, 1), SegmentsAt(state, 2, 1));
            SegmentsAt(state, 0, 1)[0].Length = 99.0;
            Assert.Equal(40.0, SegmentsAt(state, 2, 1)[0].Length);
            Assert.Equal(40.0, authored[0].Length);
        }

        [Fact]
        public void EveryFrenteWideProperty_OmitsATargetWithoutThatFrente()
        {
            var state = StateWith(3, 1, 3); // fondo 1 has a single frente
            state.SetTargetFondos(new[] { 0, 1, 2 });

            var floor = state.ApplyFloorBeamToTargets(SelectiveFrontApplyScope.Front, 2, true);
            var height = state.ApplyBayHeightToTargets(SelectiveFrontApplyScope.Front, 2, 180.0);
            var segments = state.ApplySegmentsToTargets(2, new[] { new SelectiveSegment { Length = 30.0, Loaded = true } });

            foreach (var result in new[] { floor, height, segments })
            {
                Assert.Equal(new[] { (0, 2), (2, 2) }, result.Applied);
                Assert.Equal(new[] { 1 }, result.OmittedFondos);
            }

            Assert.Single(state.FondoMatrices[1].FloorBeams); // nothing padded, nothing clamped
        }

        // ---- B. Fondo-wide properties over TargetFondos ----

        [Fact]
        public void PalletDepth_LandsOnEveryTargetFondo_AndTheirCustomCabecerasAdoptTheNewDepth()
        {
            var state = StateWith(2, 2, 2);
            state.SetTargetFondos(new[] { 0, 1, 2 });
            state.ApplyCabeceraToTargets(0, Custom(300.0), DeepCopy);

            state.SetTargetFondos(new[] { 0, 2 });
            var result = state.ApplyPalletDepthToTargets(60.0);

            Assert.Equal(new[] { 0, 2 }, result.AppliedFondos);
            Assert.Equal(60.0, state.FondoMatrices[0].Depth);
            Assert.Equal(48.0, state.FondoMatrices[1].Depth); // untouched
            Assert.Equal(60.0, state.FondoMatrices[2].Depth);

            // The cabeceras of the touched fondos follow immediately (60 - 6), the untouched one keeps 42.
            Assert.Equal(54.0, state.CabeceraAt(0, 0).Depth, 4);
            Assert.Equal(42.0, state.CabeceraAt(1, 0).Depth, 4);
            Assert.Equal(54.0, state.CabeceraAt(2, 0).Depth, 4);
        }

        [Fact]
        public void CabeceraDepth_OverridesAndRestoresOverAnArbitrarySubset()
        {
            var state = StateWith(2, 2, 2);
            state.SetTargetFondos(new[] { 0, 1, 2 });
            state.ApplyCabeceraToTargets(0, Custom(300.0), DeepCopy);

            state.SetTargetFondos(new[] { 1 });
            state.ApplyCabeceraDepthToTargets(37.0);
            Assert.Equal(37.0, state.CabeceraAt(1, 0).Depth, 4);
            Assert.Equal(42.0, state.CabeceraAt(0, 0).Depth, 4); // outside the target

            var restore = state.ApplyCabeceraDepthToTargets(null);
            Assert.Equal(new[] { 1 }, restore.AppliedFondos);
            Assert.Equal(0.0, state.FondoMatrices[1].CabeceraOverride);
            Assert.Equal(42.0, state.CabeceraAt(1, 0).Depth, 4); // back to the derived rule
        }

        // ---- Scenario 1: an arbitrary subset, every property at once ----

        [Fact]
        public void Scenario1_ASubsetOfFondos_ReceivesEveryEditAndTheOthersReceiveNone()
        {
            var state = StateWith(3, 3, 3, 3);
            state.SetTargetFondos(new[] { 0, 2 });

            state.SelectCell(1, 1, extend: false);
            state.ApplyToTargets(SelectiveApplyScope.Cell, new SelectiveEditorCell { Frente = 77.0, Alto = 60.0, PalletCount = 3, BeamId = BeamId, BeamPeralte = 4.0 });
            state.ApplyFloorBeamToTargets(SelectiveFrontApplyScope.Front, 1, true);
            state.ApplyBayHeightToTargets(SelectiveFrontApplyScope.Front, 1, 205.0);
            state.ApplyFloorBeamRiseToTargets(SelectiveFrontApplyScope.Front, 1, 14.0);
            state.ApplyCabeceraToTargets(1, Custom(320.0), DeepCopy);
            state.ApplyPalletDepthToTargets(60.0);
            state.ApplyCabeceraDepthToTargets(50.0);

            foreach (var fondo in new[] { 0, 2 })
            {
                Assert.Equal(77.0, state.CellAt(new SelectiveCellAddress(fondo, 1, 1)).Frente);
                Assert.True(FloorBeamAt(state, fondo, 1));
                Assert.Equal(205.0, HeightAt(state, fondo, 1));
                Assert.Equal(14.0, state.FloorBeamRiseOverrideAt(fondo, 1));
                Assert.Equal(320.0, state.CabeceraAt(fondo, 1).Height, 4);
                Assert.Equal(60.0, state.FondoMatrices[fondo].Depth);
                Assert.Equal(50.0, state.FondoMatrices[fondo].CabeceraOverride);
            }

            foreach (var fondo in new[] { 1, 3 })
            {
                Assert.Equal(42.0, state.CellAt(new SelectiveCellAddress(fondo, 1, 1)).Frente);
                Assert.False(FloorBeamAt(state, fondo, 1));
                Assert.Null(HeightAt(state, fondo, 1));
                // Conserva su valor DIRECTO sembrado (4"), NO el 14" que recibieron los destinos: sigue probando
                // exactamente lo mismo, que a este fondo no llego la edicion (I-43, gate 8.6D, INV-12).
                Assert.Equal(SelectiveRackDefaults.DefaultFloorBeamRise, state.FloorBeamRiseOverrideAt(fondo, 1));
                Assert.Null(state.CabeceraAt(fondo, 1));
                Assert.Equal(48.0, state.FondoMatrices[fondo].Depth);
                Assert.Equal(0.0, state.FondoMatrices[fondo].CabeceraOverride);
            }
        }

        // ---- Scenario 2: a non-contiguous Selected over a divergent topology ----

        [Fact]
        public void Scenario2_ANonContiguousSelection_ProjectsOntoWhatExistsAndOmitsOnlyThere()
        {
            var state = StateWith(3, 3, 3);
            state.FondoMatrices[2].Bays.RemoveAt(2); // fondo 2 keeps two frentes
            state.SetTargetFondos(new[] { 1, 2 });

            state.SelectCell(0, 0, extend: false);
            state.SelectCell(2, 0, extend: true);
            state.SelectCell(1, 1, extend: true);

            var plan = state.ApplyToTargets(SelectiveApplyScope.Selected, new SelectiveEditorCell { Frente = 64.0, Alto = 60.0, PalletCount = 2, BeamId = BeamId, BeamPeralte = 4.0 });

            Assert.Equal(
                new[] { (1, 0, 0), (1, 1, 1), (1, 2, 0), (2, 0, 0), (2, 1, 1) },
                plan.Targets.Select(t => (t.FondoIndex, t.FrontIndex, t.LevelIndex)).ToArray());
            Assert.Equal(new[] { new SelectiveCellAddress(2, 2, 0) }, plan.OmittedCells);
            Assert.Equal(2, state.FondoMatrices[2].Bays.Count); // nothing was created to make it fit
            Assert.Equal(42.0, state.CellAt(new SelectiveCellAddress(0, 0, 0)).Frente); // fondo 0 is not a target
        }

        // ---- Scenario 3: save / load ----

        [Fact]
        public void Scenario3_AMixOfOperationsSurvivesSaveAndLoad_AndNoRuntimeChoiceIsPersisted()
        {
            var state = StateWith(3, 3, 2);
            state.SetTargetFondos(new[] { 0, 2 });
            state.SelectCell(1, 1, extend: false);
            state.SelectCell(0, 0, extend: true);
            state.ApplyToTargets(SelectiveApplyScope.Selected, new SelectiveEditorCell { Frente = 55.0, Alto = 60.0, PalletCount = 4, BeamId = BeamId, BeamPeralte = 4.0 });
            state.ApplyFloorBeamToTargets(SelectiveFrontApplyScope.Front, 0, true);
            state.ApplyBayHeightToTargets(SelectiveFrontApplyScope.Front, 1, 199.0);
            state.ApplyFloorBeamRiseToTargets(SelectiveFrontApplyScope.Front, 0, 12.0);
            state.ApplySegmentsToTargets(1, new[] { new SelectiveSegment { Length = 40.0, Loaded = true }, new SelectiveSegment { Length = 0.0, Loaded = false } });
            state.ApplyCabeceraToTargets(1, Custom(330.0), DeepCopy);
            state.ApplyPalletDepthToTargets(60.0);

            var design = state.BuildDesign(Inputs(state));
            var before = new SelectiveGeometryResolver().Resolve(design, Catalog);

            var store = new SelectivePalletDesignStore();
            var json = store.Serialize(SelectivePalletDesignDocument.From(design, "GUID-G7", "Gate 7"));
            var document = store.Deserialize(json);
            var after = new SelectiveGeometryResolver().Resolve(document.ToDomain(), Catalog);

            Assert.Equal("GUID-G7", document.Id);
            Assert.Equal(Signature(before), Signature(after));
            Assert.Equal(BomSignature(before), BomSignature(after));

            // Runtime-only: nothing about the selection or the targets reaches the JSON.
            Assert.DoesNotContain("TargetFondos", json);
            Assert.DoesNotContain("Selected", json);
            Assert.DoesNotContain("SelBay", json);
        }

        // ---- Scenario 4: resize, no resurrection ----

        [Fact]
        public void Scenario4_NothingDeletedComesBackAfterAShrinkAndRegrow()
        {
            var state = StateWith(3, 3);
            state.SetTargetFondos(new[] { 0, 1 });
            state.ApplyFloorBeamRiseToTargets(SelectiveFrontApplyScope.Front, 2, 15.0);
            state.ApplyBayHeightToTargets(SelectiveFrontApplyScope.Front, 2, 240.0);
            state.ApplyCabeceraToTargets(3, Custom(340.0), DeepCopy);
            state.SelectCell(2, 1, extend: false);

            // Fondo 0 (the live matrix) shrinks to one frente and grows back.
            state.ResizeBays(1);
            state.SyncPostCabeceras();
            state.ResizeBays(3);
            state.SyncPostCabeceras();

            // El frente que vuelve clona al superviviente (4"), NO resucita el 15" que el shrink borro.
            Assert.Equal(SelectiveRackDefaults.DefaultFloorBeamRise, state.FloorBeamRiseOverrideAt(0, 2));
            Assert.Null(HeightAt(state, 0, 2));
            Assert.Null(state.CabeceraAt(0, 3));
            Assert.Equal(3, state.BaySegments.Count);
            Assert.Equal(3, state.FloorBeams.Count);
            Assert.Equal(3, state.BayHeights.Count);
            Assert.Equal(3, state.FloorBeamRiseOverrides.Count);
            Assert.True(state.IsSelected(state.SelBay, state.SelLevel)); // the selection stayed coherent

            Assert.Equal(3, state.FondoMatrices[1].Bays.Count); // the other fondo kept its own frentes
            Assert.Equal(15.0, state.FondoMatrices[1].FloorBeamRiseOverrides[2]);
        }

        [Fact]
        public void Scenario4_ReducingTheFondoCount_PrunesTheTargetsAndFallsBackToTheVisibleFondo()
        {
            var state = StateWith(3, 3, 3);
            state.SetTargetFondos(new[] { 1, 2 });

            state.FondoMatrices.RemoveAt(2);
            state.FondoMatrices.RemoveAt(1);
            state.SyncTargetFondos();

            Assert.Equal(new[] { 0 }, state.TargetFondos.Fondos); // never empty
        }

        // ---- Scenario 5: the legacy single-fondo flow ----

        [Fact]
        public void Scenario5_ASingleFondoRack_ProducesTheSameDesignAsTheHistoricFlow()
        {
            // The historic flow: no targets touched, edits land on the only fondo there is.
            var state = StateWith(2);
            state.SelectCell(0, 0, extend: false);
            state.ApplyToTargets(SelectiveApplyScope.All, new SelectiveEditorCell { Frente = 44.0, Alto = 52.0, PalletCount = 3, BeamId = BeamId, BeamPeralte = 4.0 });
            state.ApplyFloorBeamToTargets(SelectiveFrontApplyScope.Front, 0, true);
            state.ApplyBayHeightToTargets(SelectiveFrontApplyScope.Front, 1, 190.0);

            var built = state.BuildDesign(Inputs(state));

            // The same design assembled by hand, exactly as before I-43 existed.
            var hand = new SelectivePalletDesign
            {
                PostId = PostId,
                PostPeralte = 3.0,
                PalletTolerance = 4.0,
                VerticalClearance = 6.0,
                FloorBeamRise = 4.0,
                PalletDepth = 48.0,
                DepthCount = 1
            };
            for (var b = 0; b < 2; b++)
            {
                var bay = new SelectiveBayDesign { FloorBeam = b == 0, HeightOverride = b == 1 ? 190.0 : (double?)null };
                for (var l = 0; l < 2; l++)
                {
                    bay.Levels.Add(new SelectiveCell
                    {
                        Pallet = new Tarima { Frente = 44.0, Alto = 52.0 },
                        PalletCount = 3,
                        BeamId = BeamId,
                        BeamPeralte = 4.0
                    });
                }

                hand.Bays.Add(bay);
            }

            var resolver = new SelectiveGeometryResolver();
            Assert.Equal(Signature(resolver.Resolve(hand, Catalog)), Signature(resolver.Resolve(built, Catalog)));
        }

        /// <summary>Every resolved Y and beam length of every fondo — the geometry two designs must share to be equal.</summary>
        private static string Signature(SelectiveRackSystem system)
        {
            var parts = new List<string> { system.Height.ToString("0.####") };
            for (var k = 0; k < SelectiveDepthLayout.Count(system); k++)
            {
                var bays = SelectiveDepthLayout.BaysOfFondo(system, k);
                for (var b = 0; b < bays.Count; b++)
                {
                    parts.Add($"{k}:{b}:{bays[b].BeamLength:0.####}:{bays[b].Height:0.####}:"
                        + string.Join(",", bays[b].Levels.Select(level => level.Y.ToString("0.####"))));
                }
            }

            return string.Join("|", parts);
        }

        private static string BomSignature(SelectiveRackSystem system)
            => string.Join("|", SelectiveBomBuilder.Build(system, Catalog).Lines
                .Select(line => line.Category + ":" + line.ProfileId + ":" + line.Description + ":" + line.Length.ToString("0.###") + ":" + line.Quantity)
                .OrderBy(text => text, System.StringComparer.Ordinal));
    }
}
