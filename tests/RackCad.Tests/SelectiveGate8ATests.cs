using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.Selective;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-43 gate 8A: the redesign the Owner asked for. The elevation of the floor larguero becomes a DIRECT property
    /// of each frente (no run-wide value to inherit from), the frente scopes gain "Seleccionados", the number of
    /// frentes obeys the target fondos like everything else, and adding or removing a level projects the same way.
    /// The legacy contracts — the old run-wide elevation and the old manual height — keep old drawings intact.
    /// </summary>
    public class SelectiveGate8ATests
    {
        private const string PostId = TestCatalogIds.Profiles.Posts.Standard;
        private const string BeamId = TestCatalogIds.Profiles.Beams.SelectiveThreeRivet;

        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

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

        private static SelectiveDesignInputs Inputs(SelectiveEditorState state, double legacyGlobalRise = 4.0)
            => new SelectiveDesignInputs
            {
                PostId = PostId,
                PostPeralte = 3.0,
                PalletTolerance = 4.0,
                VerticalClearance = 6.0,
                FloorBeamRise = legacyGlobalRise,
                Fondo = 48.0,
                DepthCount = state.FondoMatrices.Count,
                WorkingDepth = 48.0,
                WorkingCabeceraOverride = 0.0,
                Separators = new List<double>()
            };

        private static int Frentes(SelectiveEditorState state, int fondo)
            => fondo == state.SelectedFondo ? state.Bays.Count : state.FondoMatrices[fondo].Bays.Count;

        private static double?[] RisesOf(SelectiveEditorState state, int fondo)
            => Enumerable.Range(0, Frentes(state, fondo)).Select(f => state.FloorBeamRiseOverrideAt(fondo, f)).ToArray();

        private static bool[] FloorsOf(SelectiveEditorState state, int fondo)
            => (fondo == state.SelectedFondo ? state.FloorBeams : state.FondoMatrices[fondo].FloorBeams).ToArray();

        private static int LevelsAt(SelectiveEditorState state, int fondo, int front)
            => (fondo == state.SelectedFondo ? state.Bays : state.FondoMatrices[fondo].Bays)[front].Count;

        // ---- B. The "Seleccionados" reach of a FRENTE property ----

        [Fact]
        public void SelectedFrontIndices_AreTheDistinctFrentesOfTheCellSelection()
        {
            var state = StateWith(3);
            state.SelectCell(1, 0, extend: false);
            state.SelectCell(1, 1, extend: true);
            state.SelectCell(2, 0, extend: true);

            Assert.Equal(new[] { 1, 2 }, state.SelectedFrontIndices()); // the levels do not matter to a frente property
        }

        [Fact]
        public void FrontScope_Selected_WritesThoseFrentesInEveryTargetFondo()
        {
            var state = StateWith(3, 3, 3);
            state.SetTargetFondos(new[] { 0, 2 });
            state.SelectCell(0, 0, extend: false);
            state.SelectCell(2, 1, extend: true);

            var result = state.ApplyFrontPropertiesToTargets(SelectiveFrontApplyScope.Selected, 0, true, 16.0);

            Assert.Equal(new[] { (0, 0), (0, 2), (2, 0), (2, 2) }, result.Applied);
            Assert.Equal(new double?[] { 16.0, null, 16.0 }, RisesOf(state, 0));
            Assert.Equal(new double?[] { null, null, null }, RisesOf(state, 1)); // not a target
            Assert.Equal(new[] { true, false, true }, FloorsOf(state, 2));
        }

        [Fact]
        public void FrontScope_Selected_SkipsAFrenteAShortFondoDoesNotHave()
        {
            var state = StateWith(3, 1, 3);
            state.SetTargetFondos(new[] { 0, 1, 2 });
            state.SelectCell(0, 0, extend: false);
            state.SelectCell(2, 0, extend: true);

            var result = state.ApplyFrontPropertiesToTargets(SelectiveFrontApplyScope.Selected, 0, true, 12.0);

            Assert.Equal(new[] { (0, 0), (0, 2), (1, 0), (2, 0), (2, 2) }, result.Applied);
            Assert.Single(RisesOf(state, 1)); // never padded to reach frente 2
        }

        [Fact]
        public void AFondoThatReachesNoSelectedFrente_IsReportedAsOmitted()
        {
            var state = StateWith(3, 1);
            state.SetTargetFondos(new[] { 0, 1 });
            state.SelectCell(2, 0, extend: false); // only frente 2, which fondo 1 does not have

            var result = state.ApplyFrontPropertiesToTargets(SelectiveFrontApplyScope.Selected, 2, true, 14.0);

            Assert.Equal(new[] { (0, 2) }, result.Applied);
            Assert.Equal(new[] { 1 }, result.OmittedFondos);
        }

        // ---- C. The elevation is direct: no global, and it survives "piso" off ----

        [Fact]
        public void TheElevationIsWrittenDirectly_AndOneFrenteDoesNotMoveAnother()
        {
            var state = StateWith(3);
            state.MaterializeFloorBeamRises(4.0);

            state.ApplyFrontPropertiesToTargets(SelectiveFrontApplyScope.Front, 1, true, 18.0);

            Assert.Equal(new double?[] { 4.0, 18.0, 4.0 }, RisesOf(state, 0));
        }

        [Fact]
        public void TurningPisoOffKeepsTheElevation_SoTurningItBackOnRecoversIt()
        {
            var state = StateWith(2);
            state.ApplyFrontPropertiesToTargets(SelectiveFrontApplyScope.Front, 0, true, 17.0);

            state.ApplyFrontPropertiesToTargets(SelectiveFrontApplyScope.Front, 0, false, 17.0);
            Assert.False(FloorsOf(state, 0)[0]);
            Assert.Equal(17.0, state.FloorBeamRiseOverrideAt(0, 0));

            // And with the beam off the elevation moves no geometry at all.
            var off = new SelectiveGeometryResolver().Resolve(state.BuildDesign(Inputs(state)), Catalog);
            var plain = StateWith(2);
            plain.ApplyFrontPropertiesToTargets(SelectiveFrontApplyScope.Front, 0, false, 4.0);
            var reference = new SelectiveGeometryResolver().Resolve(plain.BuildDesign(Inputs(plain)), Catalog);
            Assert.Equal(reference.Bays[0].Levels.Min(l => l.Y), off.Bays[0].Levels.Min(l => l.Y), 6);
        }

        [Fact]
        public void ApplyingToEveryFrenteOfEveryFondo_IsHowASingleValueIsSpreadNow()
        {
            // There is no global to change: the same value everywhere is an explicit "todos x todos".
            var state = StateWith(3, 3, 3);
            state.SetTargetFondos(new[] { 0, 1, 2 });

            state.ApplyFrontPropertiesToTargets(SelectiveFrontApplyScope.All, 0, true, 10.0);

            for (var fondo = 0; fondo < 3; fondo++)
            {
                Assert.All(RisesOf(state, fondo), rise => Assert.Equal(10.0, rise));
                Assert.All(FloorsOf(state, fondo), floor => Assert.True(floor));
            }
        }

        // ---- D. Legacy run-wide elevation ----

        [Fact]
        public void ALegacyDocument_ResolvesExactlyAsBefore_AndThenTheFrenteIsTheAuthority()
        {
            // A document written before gate 8A: a run-wide 7" and no per-frente value anywhere.
            var legacy = LegacyDesign(globalRise: 7.0);
            var before = new SelectiveGeometryResolver().Resolve(legacy, Catalog);

            var state = StateWith(2);
            state.FloorBeams[0] = true;
            state.FloorBeams[1] = true;           // the legacy design has "larguero a piso" on both frentes
            state.MaterializeFloorBeamRises(7.0); // what the editor does on load

            Assert.All(RisesOf(state, 0), rise => Assert.Equal(7.0, rise)); // materialized, not inherited
            var after = new SelectiveGeometryResolver().Resolve(state.BuildDesign(Inputs(state, legacyGlobalRise: 7.0)), Catalog);
            Assert.Equal(before.Bays[0].Levels.Min(l => l.Y), after.Bays[0].Levels.Min(l => l.Y), 6);

            // From here the frente governs: changing one moves only it, whatever the legacy field still says.
            state.ApplyFrontPropertiesToTargets(SelectiveFrontApplyScope.Front, 0, true, 15.0);
            var edited = new SelectiveGeometryResolver().Resolve(state.BuildDesign(Inputs(state, legacyGlobalRise: 7.0)), Catalog);
            Assert.NotEqual(edited.Bays[0].Levels.Min(l => l.Y), edited.Bays[1].Levels.Min(l => l.Y));
        }

        [Fact]
        public void TheDirectElevationsRoundTrip()
        {
            var state = StateWith(2, 2);
            state.SetTargetFondos(new[] { 0, 1 });
            state.ApplyFrontPropertiesToTargets(SelectiveFrontApplyScope.All, 0, true, 13.0);
            state.SetTargetFondos(new[] { 1 });
            state.ApplyFrontPropertiesToTargets(SelectiveFrontApplyScope.Front, 1, true, 21.0);

            var design = state.BuildDesign(Inputs(state));
            var restored = RoundTrip(design, out var id);

            Assert.Equal("GUID-G8A", id);
            Assert.Equal(13.0, restored.Bays[0].FloorBeamRiseOverride);
            Assert.Equal(13.0, restored.ExtraFondoBays[0][0].FloorBeamRiseOverride);
            Assert.Equal(21.0, restored.ExtraFondoBays[0][1].FloorBeamRiseOverride);
        }

        // ---- E. Legacy manual height ----

        [Fact]
        public void ALegacyHeightOverride_KeepsItsGeometryThroughARoundTrip()
        {
            var legacy = LegacyDesign(globalRise: 4.0);
            legacy.Bays[1].HeightOverride = 240.0;

            var before = new SelectiveGeometryResolver().Resolve(legacy, Catalog);
            var after = new SelectiveGeometryResolver().Resolve(RoundTrip(legacy, out _), Catalog);

            Assert.Equal(240.0, before.Bays[1].Height, 6); // the override still governs that frente
            Assert.Equal(before.Bays[1].Height, after.Bays[1].Height, 6);
            Assert.Equal(before.Height, after.Height, 6);
        }

        [Fact]
        public void TheEditorNeverCreatesAHeightOverride()
        {
            // Nothing in the new flow authors one: a design built from a fresh state has none.
            var state = StateWith(3, 3);
            state.SetTargetFondos(new[] { 0, 1 });
            state.ApplyFrontPropertiesToTargets(SelectiveFrontApplyScope.All, 0, true, 11.0);
            state.ApplyBayCountToTargets(4);

            var design = state.BuildDesign(Inputs(state));

            Assert.All(design.Bays, bay => Assert.Null(bay.HeightOverride));
            Assert.All(design.ExtraFondoBays.SelectMany(f => f), bay => Assert.Null(bay.HeightOverride));
        }

        // ---- F. The number of frentes obeys the target fondos ----

        [Fact]
        public void BayCount_ResizesOnlyTheTargetFondos_Independently()
        {
            var state = StateWith(2, 2, 2, 2);
            state.SetTargetFondos(new[] { 0, 2 });

            var result = state.ApplyBayCountToTargets(5);

            Assert.Equal(new[] { 0, 2 }, result.AppliedFondos);
            Assert.Equal(5, Frentes(state, 0));
            Assert.Equal(2, Frentes(state, 1));
            Assert.Equal(5, Frentes(state, 2));
            Assert.Equal(2, Frentes(state, 3));
        }

        [Fact]
        public void BayCount_ShrinkDropsTheTail_AndRegrowingDoesNotResurrectIt()
        {
            var state = StateWith(3, 3);
            state.SetTargetFondos(new[] { 0, 1 });
            state.ApplyFrontPropertiesToTargets(SelectiveFrontApplyScope.Front, 2, true, 19.0);
            state.ApplyCabeceraToTargets(3, new RackCad.Domain.RackFrames.RackFrameConfiguration { Height = 300.0 }, c => c);

            state.ApplyBayCountToTargets(1);
            Assert.Equal(1, Frentes(state, 0));
            Assert.Equal(1, Frentes(state, 1));

            state.ApplyBayCountToTargets(3);
            Assert.Equal(new double?[] { null, null, null }, RisesOf(state, 0).Select(r => r == 19.0 ? (double?)19.0 : null).ToArray());
            Assert.Null(state.CabeceraAt(0, 3));
            Assert.Null(state.CabeceraAt(1, 3));
        }

        [Fact]
        public void BayCount_KeepsEveryParallelListCoherent()
        {
            var state = StateWith(2, 2);
            state.SetTargetFondos(new[] { 0, 1 });

            state.ApplyBayCountToTargets(4);

            Assert.Equal(4, state.Bays.Count);
            Assert.Equal(4, state.FloorBeams.Count);
            Assert.Equal(4, state.BayHeights.Count);
            Assert.Equal(4, state.BaySegments.Count);
            Assert.Equal(4, state.FloorBeamRiseOverrides.Count);

            var stored = state.FondoMatrices[1];
            Assert.Equal(4, stored.Bays.Count);
            Assert.Equal(4, stored.FloorBeams.Count);
            Assert.Equal(4, stored.BayHeights.Count);
            Assert.Equal(4, stored.BaySegments.Count);
            Assert.Equal(4, stored.FloorBeamRiseOverrides.Count);
        }

        [Fact]
        public void BayCount_NeverCreatesAFondo()
        {
            var state = StateWith(2, 2);
            state.SetTargetFondos(new[] { 0, 1 });

            state.ApplyBayCountToTargets(3);

            Assert.Equal(2, state.FondoMatrices.Count); // the fondo count is a different control entirely
        }

        [Fact]
        public void BayCount_PrunesTheSelectionToWhatSurvived()
        {
            var state = StateWith(3);
            state.SelectCell(0, 0, extend: false);
            state.SelectCell(2, 1, extend: true);

            state.ApplyBayCountToTargets(1);

            Assert.All(state.SelectedPositions(), position => Assert.True(position.FrontIndex < 1));
            Assert.True(state.IsSelected(state.SelBay, state.SelLevel));
        }

        // ---- Levels project like every other frente-wide edit ----

        [Fact]
        public void AddingALevel_ReachesTheSameFrenteOfEveryTargetFondo()
        {
            var state = StateWith(2, 2, 2);
            state.SetTargetFondos(new[] { 0, 2 });

            state.ApplyLevelDeltaToTargets(SelectiveFrontApplyScope.Front, 1, +1);

            Assert.Equal(3, LevelsAt(state, 0, 1));
            Assert.Equal(2, LevelsAt(state, 1, 1)); // untouched
            Assert.Equal(3, LevelsAt(state, 2, 1));
            Assert.Equal(2, LevelsAt(state, 0, 0)); // other frentes untouched
        }

        [Fact]
        public void RemovingALevel_NeverEmptiesAFrente()
        {
            var state = StateWith(2, 2);
            state.SetTargetFondos(new[] { 0, 1 });

            state.ApplyLevelDeltaToTargets(SelectiveFrontApplyScope.Front, 0, -5);

            Assert.Equal(1, LevelsAt(state, 0, 0));
            Assert.Equal(1, LevelsAt(state, 1, 0));
        }

        [Fact]
        public void ALevelChange_SkipsATargetWithoutThatFrente()
        {
            var state = StateWith(3, 1);
            state.SetTargetFondos(new[] { 0, 1 });

            var result = state.ApplyLevelDeltaToTargets(SelectiveFrontApplyScope.Front, 2, +1);

            Assert.Equal(new[] { (0, 2) }, result.Applied);
            Assert.Equal(new[] { 1 }, result.OmittedFondos);
        }

        // ---- Helpers ----

        private static SelectivePalletDesign LegacyDesign(double globalRise)
        {
            var design = new SelectivePalletDesign
            {
                PostId = PostId,
                PostPeralte = 3.0,
                PalletTolerance = 4.0,
                VerticalClearance = 6.0,
                FloorBeamRise = globalRise,
                PalletDepth = 48.0,
                DepthCount = 1
            };

            for (var b = 0; b < 2; b++)
            {
                var bay = new SelectiveBayDesign { FloorBeam = true }; // no per-frente elevation at all
                for (var l = 0; l < 2; l++)
                {
                    bay.Levels.Add(new SelectiveCell
                    {
                        Pallet = new Tarima { Frente = 42.0, Alto = 48.0 },
                        PalletCount = 2,
                        BeamId = BeamId,
                        BeamPeralte = 4.0
                    });
                }

                design.Bays.Add(bay);
            }

            return design;
        }

        private static SelectivePalletDesign RoundTrip(SelectivePalletDesign design, out string id)
        {
            var store = new SelectivePalletDesignStore();
            var document = store.Deserialize(store.Serialize(SelectivePalletDesignDocument.From(design, "GUID-G8A", "Gate 8A")));
            id = document.Id;
            return document.ToDomain();
        }
    }
}
