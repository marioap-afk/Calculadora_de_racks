using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.Selective;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// The I-43 Gate 3 contract in Application: the multi-selection of the visible matrix, the target-fondo axis and
    /// the multi-fondo write authority (<see cref="SelectiveEditorState.ApplyToTargets"/>) — which is what lets the
    /// window apply across fondos without ever looping over them. Pure: no WPF, no AutoCAD.
    /// </summary>
    public class SelectiveMultiSelectionTests
    {
        private const string BeamId = "BEAM-DEF";

        /// <summary>A state with <paramref name="perFondoLevels"/> shape: one entry per fondo, each entry the level
        /// count of every frente of that fondo. Fondo 0 is loaded as the working matrix, as the editor opens it.</summary>
        private static SelectiveEditorState StateWith(params int[][] perFondoLevels)
        {
            var state = new SelectiveEditorState { DefaultBeamId = BeamId };
            foreach (var fondo in perFondoLevels)
            {
                state.InitMatrix(fondo.Length, 1);
                for (var bay = 0; bay < fondo.Length; bay++)
                {
                    while (state.Bays[bay].Count < fondo[bay]) state.AddLevel(bay);
                    while (state.Bays[bay].Count > fondo[bay]) state.Bays[bay].RemoveAt(state.Bays[bay].Count - 1);
                }

                state.FondoMatrices.Add(state.SnapshotWorking(48.0, 0.0));
            }

            state.SelectedFondo = 0;
            state.LoadFondo(0);
            state.SetTargetFondos(new[] { 0 });
            return state;
        }

        /// <summary>Four fondos of 3 frentes x 3 niveles.</summary>
        private static SelectiveEditorState FourSquare()
            => StateWith(new[] { 3, 3, 3 }, new[] { 3, 3, 3 }, new[] { 3, 3, 3 }, new[] { 3, 3, 3 });

        private static SelectiveEditorCell Values(double frente)
            => new SelectiveEditorCell { Frente = frente, Alto = 71.0, PalletCount = 5, BeamId = "BEAM-X", BeamPeralte = 4.0 };

        /// <summary>The Frente of a cell in ANY fondo — the live matrix for the selected one, its slot for the rest.</summary>
        private static double FrenteAt(SelectiveEditorState state, int fondo, int front, int level)
            => state.CellAt(new SelectiveCellAddress(fondo, front, level)).Frente;

        private static (int Front, int Level)[] Positions(SelectiveEditorState state)
            => state.SelectedPositions().Select(p => (p.FrontIndex, p.LevelIndex)).ToArray();

        // ---- Click vs Ctrl+click ----

        [Fact]
        public void PlainClick_SelectsExactlyOneCell_AndMakesItPrimary()
        {
            var state = FourSquare();
            state.SelectCell(2, 1, extend: true); // build a multi-selection first
            state.SelectCell(0, 0, extend: true);

            state.SelectCell(1, 2, extend: false);

            Assert.Equal(new[] { (1, 2) }, Positions(state));
            Assert.Equal(1, state.SelBay);
            Assert.Equal(2, state.SelLevel);
        }

        [Fact]
        public void CtrlClick_AddsToTheSelection_AndMovesThePrimaryToTheNewCell()
        {
            var state = FourSquare();

            state.SelectCell(0, 0, extend: false);
            state.SelectCell(2, 2, extend: true);

            Assert.Equal(new[] { (0, 0), (2, 2) }, Positions(state));
            Assert.Equal(2, state.SelBay);
            Assert.Equal(2, state.SelLevel);
            Assert.True(state.IsSelected(0, 0));
            Assert.True(state.IsSelected(2, 2));
            Assert.False(state.IsSelected(1, 1));
        }

        [Fact]
        public void CtrlClick_OnASelectedCell_RemovesIt_AndReseatsThePrimaryWhenItWasTheOneRemoved()
        {
            var state = FourSquare();
            state.SelectCell(0, 1, extend: false);
            state.SelectCell(2, 0, extend: true); // primary is now (2,0)

            state.SelectCell(2, 0, extend: true); // toggle the primary off

            Assert.Equal(new[] { (0, 1) }, Positions(state));
            Assert.Equal(0, state.SelBay);
            Assert.Equal(1, state.SelLevel);
            Assert.True(state.IsSelected(state.SelBay, state.SelLevel)); // primary always inside the selection
        }

        [Fact]
        public void CtrlClick_NeverEmptiesTheSelection()
        {
            var state = FourSquare();
            state.SelectCell(1, 1, extend: false);

            state.SelectCell(1, 1, extend: true); // the only selected cell: removing it is refused

            Assert.Equal(new[] { (1, 1) }, Positions(state));
            Assert.Equal(1, state.SelectedCount);
        }

        [Fact]
        public void ANonContiguousSelection_IsKeptExactly_AndInCanonicalOrder()
        {
            var state = FourSquare();
            state.SelectCell(2, 2, extend: false);
            state.SelectCell(0, 0, extend: true);
            state.SelectCell(1, 1, extend: true);

            Assert.Equal(new[] { (0, 0), (1, 1), (2, 2) }, Positions(state));
            Assert.Equal(3, state.SelectedCount);
        }

        [Fact]
        public void ClickingOutsideTheMatrix_ChangesNothing()
        {
            var state = FourSquare();
            state.SelectCell(1, 1, extend: false);

            state.SelectCell(9, 0, extend: true);
            state.SelectCell(0, 9, extend: true);

            Assert.Equal(new[] { (1, 1) }, Positions(state));
        }

        // ---- Normalization on a topology / fondo change ----

        [Fact]
        public void ShrinkingTheMatrix_KeepsTheSurvivingPositions_AndNeverCreatesANewOne()
        {
            var state = FourSquare();
            state.SelectCell(0, 0, extend: false);
            state.SelectCell(2, 2, extend: true); // primary is (2,2)

            state.ResizeBays(2); // frente 2 disappears, taking the primary with it

            // (0,0) survives and is the WHOLE selection. The clamped primary is NOT added: it is a cell the user never
            // marked, and selecting it would silently widen the next bulk edit.
            Assert.Equal(new[] { (0, 0) }, Positions(state));
            Assert.Equal((0, 0), (state.SelBay, state.SelLevel));
        }

        [Fact]
        public void WhenThePrimaryVanishes_ItReseatsOnASurvivor_WithoutAddingAThirdPosition()
        {
            var state = FourSquare();
            state.SelectCell(0, 1, extend: false);
            state.SelectCell(1, 0, extend: true);
            state.SelectCell(2, 2, extend: true); // primary is (2,2), which is about to disappear

            state.ResizeBays(2);

            Assert.Equal(new[] { (0, 1), (1, 0) }, Positions(state)); // both survivors, and only them
            Assert.Equal((0, 1), (state.SelBay, state.SelLevel));    // deterministic: the first in canonical order
        }

        [Fact]
        public void OnlyWhenNothingSurvives_DoesTheClampedPrimaryBecomeTheSelection()
        {
            var state = FourSquare();
            state.SelectCell(2, 2, extend: false);

            state.ResizeBays(1); // the single selected position is gone; the primary is clamped into frente 0

            Assert.Equal((0, 2), (state.SelBay, state.SelLevel));
            Assert.Equal(new[] { (0, 2) }, Positions(state)); // fallback, so the selection is never empty
        }

        [Fact]
        public void SwitchingFondo_KeepsThePositionsThatStillExist_AndPrunesTheRest()
        {
            // Fondo 1 is a corner layout: one frente of two levels. Only (0,0) survives the switch.
            var state = StateWith(new[] { 3, 3, 3 }, new[] { 2 });
            state.SelectCell(0, 0, extend: false);
            state.SelectCell(2, 2, extend: true);

            state.SelectFondo(1);
            state.LoadFondo(1);

            // Same rule as a shrink: the survivor is the whole selection and the primary re-seats onto it.
            Assert.Equal(new[] { (0, 0) }, Positions(state));
            Assert.Equal((0, 0), (state.SelBay, state.SelLevel));
        }

        // ---- Legacy primary vs real multi-selection ----

        [Fact]
        public void ALegacyPrimaryAssignment_StillClampsExactlyAsItAlwaysDid()
        {
            // SelBay/SelLevel assigned directly is the imperative "the primary is now this cell". It cannot maintain
            // the selection invariant, so it MEANS a single-cell selection — and the historical clamp still governs
            // where the primary lands, instead of the surviving selection dragging it elsewhere.
            var state = FourSquare();
            state.SelectCell(0, 0, extend: false);
            state.SelectCell(1, 1, extend: true);

            state.SelBay = 9;
            state.SelLevel = 9;
            state.ClampSelection();

            Assert.Equal((2, 2), (state.SelBay, state.SelLevel)); // clamped to the last frente / last level
            Assert.Equal(new[] { (2, 2) }, Positions(state));     // the statement collapses the selection onto it
        }

        [Fact]
        public void TheLegacyFlagIsConsumed_SoTheNextNormalizeUsesTheSharedRuleAgain()
        {
            var state = FourSquare();
            state.SelBay = 9;
            state.ClampSelection();          // legacy statement: selection is {(2,0)}
            Assert.Equal(new[] { (2, 0) }, Positions(state));

            state.SelectCell(0, 0, extend: true); // back to the multi-selection path
            state.ResizeBays(2);                  // frente 2 disappears

            Assert.Equal(new[] { (0, 0) }, Positions(state)); // survivor kept, nothing invented
            Assert.Equal((0, 0), (state.SelBay, state.SelLevel));
        }

        [Fact]
        public void TheSelectionIsOne_NotOnePerFondo_SoItDoesNotAccumulateWhileNavigating()
        {
            var state = StateWith(new[] { 3, 3, 3 }, new[] { 3, 3, 3 });
            state.SelectCell(0, 0, extend: false);
            state.SelectCell(1, 1, extend: true);

            state.SelectFondo(1);
            state.LoadFondo(1);
            state.SelectCell(2, 2, extend: true); // add one while on fondo 1
            state.SelectFondo(0);
            state.LoadFondo(0);

            // Three positions total — not three plus the two "of fondo 0" kept aside.
            Assert.Equal(new[] { (0, 0), (1, 1), (2, 2) }, Positions(state));
        }

        // ---- Target fondos ----

        [Fact]
        public void TargetFondos_DefaultToTheFondoBeingEdited()
        {
            var state = FourSquare();
            Assert.Equal(new[] { 0 }, state.TargetFondos.Fondos);
        }

        [Fact]
        public void ChangingFondo_FollowsTheTarget_WhenItWasOnlyThePreviousFondo()
        {
            var state = FourSquare();

            state.SelectFondo(2);

            Assert.Equal(new[] { 2 }, state.TargetFondos.Fondos); // legacy feel: edits land on the fondo on screen
        }

        [Fact]
        public void ChangingFondo_PreservesAnExplicitMultiFondoChoice()
        {
            var state = FourSquare();
            state.SetTargetFondos(new[] { 1, 3 });

            state.SelectFondo(2);

            Assert.Equal(new[] { 1, 3 }, state.TargetFondos.Fondos);
        }

        [Fact]
        public void ReducingTheFondoCount_DropsTargetsThatNoLongerExist_AndNeverLeavesTheSetEmpty()
        {
            var state = FourSquare();
            state.SetTargetFondos(new[] { 1, 3 });

            state.FondoMatrices.RemoveAt(3);
            state.FondoMatrices.RemoveAt(2);
            state.SyncTargetFondos();
            Assert.Equal(new[] { 1 }, state.TargetFondos.Fondos);

            state.SetTargetFondos(new[] { 1 });
            state.FondoMatrices.RemoveAt(1);
            state.SyncTargetFondos();
            Assert.Equal(new[] { 0 }, state.TargetFondos.Fondos); // fallback to the current, valid fondo
        }

        [Fact]
        public void SetTargetFondos_RejectsNothing_ButNeverKeepsAnIndexTheRackDoesNotHave()
        {
            var state = FourSquare();

            state.SetTargetFondos(new[] { -1, 2, 9 });
            Assert.Equal(new[] { 2 }, state.TargetFondos.Fondos);

            state.SetTargetFondos(new int[0]);
            Assert.Equal(new[] { 0 }, state.TargetFondos.Fondos); // empty request falls back, never applies to nothing
        }

        [Theory]
        [InlineData("1+3", new[] { 0, 2 })]
        [InlineData("2-4", new[] { 1, 2, 3 })]
        [InlineData("1,3-4", new[] { 0, 2, 3 })]
        [InlineData("todos", new[] { 0, 1, 2, 3 })]
        [InlineData(" 3 1 ", new[] { 0, 2 })]
        [InlineData("2-2", new[] { 1 })]
        [InlineData("4-2", new[] { 1, 2, 3 })]
        public void TryParse_ReadsTheCompactSubsetNotation_OneBasedIn_ZeroBasedOut(string text, int[] expected)
        {
            Assert.True(SelectiveFondoTargets.TryParse(text, 4, out var targets, out var error));
            Assert.Null(error);
            Assert.Equal(expected, targets.Fondos);
        }

        [Theory]
        [InlineData("")]
        [InlineData("0")]
        [InlineData("5")]
        [InlineData("a")]
        [InlineData("1-2-3")]
        public void TryParse_RefusesWhatItCannotHonour_InsteadOfNarrowingSilently(string text)
        {
            Assert.False(SelectiveFondoTargets.TryParse(text, 4, out var targets, out var error));
            Assert.False(string.IsNullOrWhiteSpace(error));
            Assert.True(targets.IsEmpty);
        }

        // ---- The multi-fondo write authority ----

        [Fact]
        public void Targets1And3_AreTheOnlyFondosModified()
        {
            var state = FourSquare();
            state.SetTargetFondos(new[] { 1, 3 });

            var plan = state.ApplyToTargets(SelectiveApplyScope.All, Values(99.0));

            Assert.Equal(new[] { 1, 3 }, plan.Fondos);
            Assert.Equal(18, plan.Count);
            foreach (var fondo in new[] { 1, 3 })
            {
                for (var front = 0; front < 3; front++)
                {
                    for (var level = 0; level < 3; level++) Assert.Equal(99.0, FrenteAt(state, fondo, front, level));
                }
            }

            foreach (var fondo in new[] { 0, 2 })
            {
                for (var front = 0; front < 3; front++)
                {
                    for (var level = 0; level < 3; level++) Assert.Equal(42.0, FrenteAt(state, fondo, front, level));
                }
            }
        }

        [Fact]
        public void TheActiveFondoIsWrittenInTheLiveMatrix_AndTheOthersInTheirStoredOnes()
        {
            // The active fondo's slot is STALE while editing: writing fondo 0 into its slot instead of the working
            // matrix would be silently reverted by the next SaveWorkingToSelected.
            var state = FourSquare();
            state.SetTargetFondos(new[] { 0, 2 });

            state.ApplyToTargets(SelectiveApplyScope.All, Values(77.0));

            Assert.Equal(77.0, state.Bays[1][1].Frente);                       // live working matrix (fondo 0)
            Assert.Equal(77.0, state.FondoMatrices[2].Bays[1][1].Frente);      // stored matrix (fondo 2)
            Assert.Equal(42.0, state.FondoMatrices[1].Bays[1][1].Frente);      // untouched fondo

            state.SaveWorkingToSelected(48.0, 0.0);
            Assert.Equal(77.0, state.FondoMatrices[0].Bays[1][1].Frente);      // and it survives the commit
        }

        [Theory]
        [InlineData(SelectiveApplyScope.Cell, 2)]
        [InlineData(SelectiveApplyScope.Row, 6)]
        [InlineData(SelectiveApplyScope.Column, 6)]
        [InlineData(SelectiveApplyScope.All, 18)]
        public void EveryScope_MultipliesByTheTargetFondos(SelectiveApplyScope scope, int expected)
        {
            var state = FourSquare();
            state.SetTargetFondos(new[] { 1, 3 });
            state.SelectCell(1, 1, extend: false);

            var plan = state.ApplyToTargets(scope, Values(55.0));

            Assert.Equal(expected, plan.Count);
            Assert.Equal(new[] { 1, 3 }, plan.Fondos);
        }

        [Fact]
        public void Selected_WritesTheMarkedPositionsInEveryTargetFondo()
        {
            var state = FourSquare();
            state.SetTargetFondos(new[] { 1, 3 });
            state.SelectCell(0, 0, extend: false);
            state.SelectCell(2, 0, extend: true);
            state.SelectCell(1, 1, extend: true);

            var plan = state.ApplyToTargets(SelectiveApplyScope.Selected, Values(64.0));

            Assert.Equal(6, plan.Count);
            foreach (var fondo in new[] { 1, 3 })
            {
                Assert.Equal(64.0, FrenteAt(state, fondo, 0, 0));
                Assert.Equal(64.0, FrenteAt(state, fondo, 2, 0));
                Assert.Equal(64.0, FrenteAt(state, fondo, 1, 1));
                Assert.Equal(42.0, FrenteAt(state, fondo, 0, 1)); // not marked
            }

            Assert.Equal(42.0, FrenteAt(state, 0, 0, 0)); // the fondo on screen is not a target
        }

        [Fact]
        public void Selected_WithDivergentTopologies_AppliesWhereThePositionExistsAndOmitsOnlyThere()
        {
            // Fondo 1 is a corner layout with ONE frente of two levels: (2,0) does not exist there, but does in fondo 2.
            var state = StateWith(new[] { 3, 3, 3 }, new[] { 2 }, new[] { 3, 3, 3 });
            state.SetTargetFondos(new[] { 1, 2 });
            state.SelectCell(0, 0, extend: false);
            state.SelectCell(2, 0, extend: true);

            var plan = state.ApplyToTargets(SelectiveApplyScope.Selected, Values(31.0));

            Assert.Equal(3, plan.Count); // (0,0) in both fondos + (2,0) only in fondo 2
            Assert.Equal(31.0, FrenteAt(state, 1, 0, 0));
            Assert.Equal(31.0, FrenteAt(state, 2, 0, 0));
            Assert.Equal(31.0, FrenteAt(state, 2, 2, 0));
            Assert.Equal(new[] { new SelectiveCellAddress(1, 2, 0) }, plan.OmittedCells);
        }

        [Fact]
        public void AnApplyIsIdempotent()
        {
            var state = FourSquare();
            state.SetTargetFondos(new[] { 0, 1, 2, 3 });

            var first = state.ApplyToTargets(SelectiveApplyScope.All, Values(88.0));
            var snapshot = Signature(state);
            var second = state.ApplyToTargets(SelectiveApplyScope.All, Values(88.0));

            Assert.Equal(first.Count, second.Count);
            Assert.Equal(snapshot, Signature(state));
        }

        [Fact]
        public void AnApplyNeverChangesTheMatrixShapeNorTheBayFlags()
        {
            var state = FourSquare();
            state.SetTargetFondos(new[] { 0, 1 });
            state.FloorBeams[1] = true;
            state.BayHeights[0] = 123.0;
            state.BaySegments[2].Add(new SelectiveSegment { Length = 30.0, Loaded = true });

            state.ApplyToTargets(SelectiveApplyScope.All, Values(50.0));

            Assert.Equal(3, state.Bays.Count);
            Assert.All(state.Bays, column => Assert.Equal(3, column.Count));
            Assert.True(state.FloorBeams[1]);
            Assert.Equal(123.0, state.BayHeights[0]);
            Assert.Single(state.BaySegments[2]);
        }

        [Fact]
        public void LegacySingleFondo_BehavesExactlyAsBefore()
        {
            var state = StateWith(new[] { 3, 3, 3 });
            state.SelectCell(1, 1, extend: false);

            var plan = state.ApplyToTargets(SelectiveApplyScope.Cell, Values(66.0));

            Assert.Equal(new[] { 0 }, plan.Fondos);
            Assert.Equal(1, plan.Count);
            Assert.Equal(66.0, state.Bays[1][1].Frente);
            Assert.Equal(42.0, state.Bays[0][0].Frente);
        }

        [Fact]
        public void NeitherTheSelectionNorTheTargetFondos_ReachTheDesign()
        {
            // BuildDesign is the persistence boundary: if a runtime choice leaked into it, it would be written to the
            // DWG/library. The design must be identical whatever the selection and targets are.
            var plain = StateWith(new[] { 2, 2 }, new[] { 2, 2 });
            var chosen = StateWith(new[] { 2, 2 }, new[] { 2, 2 });
            chosen.SetTargetFondos(new[] { 0, 1 });
            chosen.SelectCell(0, 0, extend: false);
            chosen.SelectCell(1, 1, extend: true);

            Assert.Equal(DesignSignature(plain), DesignSignature(chosen));
        }

        private static string Signature(SelectiveEditorState state)
        {
            var parts = new List<string>();
            for (var fondo = 0; fondo < state.FondoMatrices.Count; fondo++)
            {
                var columns = fondo == state.SelectedFondo ? state.Bays : state.FondoMatrices[fondo].Bays;
                foreach (var column in columns)
                {
                    foreach (var cell in column)
                    {
                        parts.Add($"{fondo}:{cell.Frente}:{cell.Alto}:{cell.PalletCount}:{cell.BeamId}:{cell.BeamPeralte}:{cell.BeamLength}:{cell.Clear}");
                    }
                }
            }

            return string.Join("|", parts);
        }

        private static string DesignSignature(SelectiveEditorState state)
        {
            var design = state.BuildDesign(new SelectiveDesignInputs
            {
                PostId = "P",
                PostPeralte = 3.0,
                PalletTolerance = 4.0,
                VerticalClearance = 6.0,
                FloorBeamRise = 4.0,
                Fondo = 48.0,
                DepthCount = state.FondoMatrices.Count,
                WorkingDepth = 48.0,
                WorkingCabeceraOverride = 0.0,
                Separators = new List<double>()
            });

            var parts = new List<string> { design.Bays.Count.ToString(), design.ExtraFondoBays.Count.ToString() };
            foreach (var bay in design.Bays)
            {
                parts.AddRange(bay.Levels.Select(level => $"{level.Pallet.Frente}:{level.Pallet.Alto}:{level.PalletCount}"));
            }

            foreach (var extra in design.ExtraFondoBays)
            {
                foreach (var bay in extra)
                {
                    parts.AddRange(bay.Levels.Select(level => $"{level.Pallet.Frente}:{level.Pallet.Alto}:{level.PalletCount}"));
                }
            }

            return string.Join("|", parts);
        }
    }
}
