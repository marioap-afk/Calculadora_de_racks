using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Systems.Selective;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// Pure unit tests for the I-43 contract <c>fondos objetivo x alcance interno -&gt; celdas del Selectivo</c>:
    /// <see cref="SelectiveFondoTargets"/> (the arbitrary set of target fondos), <see cref="SelectiveTopology"/> (the
    /// ragged shape targets are resolved against) and <see cref="SelectiveTargetResolver"/> (the pure, deterministic
    /// rule). They pin the four properties the contract promises — independence of the two axes, canonical order,
    /// omit-never-clamp, and a complete plan produced before anything mutates. No WPF, no AutoCAD, no catalog.
    /// </summary>
    public class SelectiveTargetResolverTests
    {
        /// <summary>A topology from raw counts: <c>Topology(new[]{3,3,3}, new[]{2})</c> is fondo 0 with three frentes
        /// of three levels and fondo 1 with a single frente of two.</summary>
        private static SelectiveTopology Topology(params int[][] perFondo)
            => SelectiveTopology.FromLevelCounts(perFondo);

        /// <summary>Four fondos of the same 3 x 3 shape — the fixture for the fondo axis on its own.</summary>
        private static SelectiveTopology FourSquareFondos()
            => Topology(new[] { 3, 3, 3 }, new[] { 3, 3, 3 }, new[] { 3, 3, 3 }, new[] { 3, 3, 3 });

        /// <summary>
        /// Four fondos with DIVERGENT topologies, the shape the Selectivo actually admits:
        /// fondo 0 = the master grid (3 frentes x 3 niveles); fondo 1 = a corner layout with ONE frente of 2 niveles;
        /// fondo 2 = ragged, with a tall frente, an EMPTY frente (a building column) and a short one; fondo 3 = a
        /// fondo with no frente at all.
        /// </summary>
        private static SelectiveTopology DivergentFondos()
            => Topology(new[] { 3, 3, 3 }, new[] { 2 }, new[] { 4, 0, 1 }, new int[0]);

        private static SelectiveCellAddress At(int fondo, int front, int level)
            => new SelectiveCellAddress(fondo, front, level);

        /// <summary>A position of the VISIBLE matrix — what a selection is made of. It carries no fondo.</summary>
        private static SelectiveMatrixPosition Pos(int front, int level)
            => new SelectiveMatrixPosition(front, level);

        private static (int Fondo, int Front, int Level)[] Tuples(SelectiveTargetPlan plan)
            => plan.Targets.Select(t => (t.FondoIndex, t.FrontIndex, t.LevelIndex)).ToArray();

        // ---- SelectiveCellAddress: the three-axis identity ----

        [Fact]
        public void CellAddress_SameIndicesInDifferentFondos_AreDifferentCells()
        {
            Assert.NotEqual(At(0, 1, 2), At(1, 1, 2));
            Assert.Equal(At(1, 1, 2), At(1, 1, 2));
            Assert.Equal(At(1, 1, 2).GetHashCode(), At(1, 1, 2).GetHashCode());
        }

        [Fact]
        public void CellAddress_Orders_ByFondoThenFrenteThenNivel()
        {
            var shuffled = new[] { At(1, 0, 0), At(0, 2, 0), At(0, 0, 5), At(0, 0, 1), At(0, 2, 0) };
            var sorted = shuffled.OrderBy(a => a).ToArray();

            Assert.Equal(
                new[] { At(0, 0, 1), At(0, 0, 5), At(0, 2, 0), At(0, 2, 0), At(1, 0, 0) },
                sorted);
        }

        // ---- SelectiveFondoTargets: an arbitrary set of fondos ----

        [Fact]
        public void FondoTargets_AreDistinctAndAscending_WhateverOrderTheyArriveIn()
        {
            var targets = SelectiveFondoTargets.Of(3, 1, 3, 1);

            Assert.Equal(new[] { 1, 3 }, targets.Fondos);
            Assert.Equal(2, targets.Count);
            Assert.True(targets.Contains(3));
            Assert.False(targets.Contains(0));
        }

        [Fact]
        public void FondoTargets_KeepsFondosTheRackDoesNotHave_SoTheResolverCanReportThem()
        {
            // Validity is a question about a topology, not about the set: dropping it here would make the omission
            // invisible to the caller.
            Assert.Equal(new[] { -1, 9 }, SelectiveFondoTargets.Of(9, -1).Fondos);
        }

        [Fact]
        public void FondoTargets_None_And_All_AreTheTwoExplicitExtremes()
        {
            Assert.True(SelectiveFondoTargets.None.IsEmpty);
            Assert.Equal(new[] { 0, 1, 2, 3 }, SelectiveFondoTargets.All(FourSquareFondos()).Fondos);
            Assert.True(SelectiveFondoTargets.All(SelectiveTopology.Empty).IsEmpty);
            Assert.True(SelectiveFondoTargets.Of((IEnumerable<int>)null).IsEmpty);
        }

        // ---- SelectiveTopology: the ragged shape ----

        [Fact]
        public void Topology_ReportsEachAxisIndependently_AndAnEmptyFrenteHoldsNoCell()
        {
            var topology = DivergentFondos();

            Assert.Equal(4, topology.FondoCount);
            Assert.Equal(3, topology.FrontCount(0));
            Assert.Equal(1, topology.FrontCount(1));
            Assert.Equal(0, topology.FrontCount(3));
            Assert.Equal(4, topology.LevelCount(2, 0));
            Assert.Equal(0, topology.LevelCount(2, 1));

            Assert.True(topology.HasFront(2, 1));          // the frente exists...
            Assert.False(topology.HasCell(At(2, 1, 0)));   // ...and holds nothing. No scope may invent its level 0.
            Assert.False(topology.HasFondo(4));
            Assert.False(topology.HasCell(At(0, 0, -1)));
        }

        // ---- The fondo axis: an arbitrary target set, and no cross-talk ----

        [Fact]
        public void FourFondos_TargetsOneAndThree_ReachExactlyThoseTwo()
        {
            var plan = SelectiveTargetResolver.Resolve(
                FourSquareFondos(), SelectiveFondoTargets.Of(1, 3), SelectiveApplyScope.All, At(0, 0, 0));

            Assert.Equal(new[] { 1, 3 }, plan.Fondos);
            Assert.Equal(18, plan.Count); // 2 fondos x 3 frentes x 3 niveles
            Assert.All(plan.Targets, target => Assert.True(target.FondoIndex == 1 || target.FondoIndex == 3));
            Assert.Empty(plan.OmittedFondos);
        }

        [Fact]
        public void TheAnchorsFondo_IsNeverIncludedByItself()
        {
            // The anchor lends frente/nivel coordinates ONLY. Its own fondo is not a target unless it was named.
            var plan = SelectiveTargetResolver.Resolve(
                FourSquareFondos(), SelectiveFondoTargets.Of(1, 3), SelectiveApplyScope.Cell, At(0, 2, 1));

            Assert.Equal(new[] { (1, 2, 1), (3, 2, 1) }, Tuples(plan));
            Assert.DoesNotContain(plan.Targets, target => target.FondoIndex == 0);
        }

        [Fact]
        public void NoCrossTalk_UntargetedFondosNeverAppear_InAnyScope()
        {
            var topology = FourSquareFondos();
            var targets = SelectiveFondoTargets.Of(1, 3);
            var scopes = new[]
            {
                SelectiveApplyScope.Cell, SelectiveApplyScope.Row,
                SelectiveApplyScope.Column, SelectiveApplyScope.All
            };

            foreach (var scope in scopes)
            {
                var plan = SelectiveTargetResolver.Resolve(topology, targets, scope, At(0, 1, 1));

                Assert.NotEmpty(plan.Targets);
                Assert.DoesNotContain(plan.Targets, target => target.FondoIndex == 0 || target.FondoIndex == 2);
            }
        }

        [Fact]
        public void SelectedPositions_AreProjectedOntoEveryTargetFondo()
        {
            // The mandatory example: three positions marked on the visible matrix, aimed at fondos 1 and 3. Each
            // position resolves ONCE PER TARGET FONDO — the selection is a set of coordinates, not a set of cells.
            var plan = SelectiveTargetResolver.Resolve(
                FourSquareFondos(), SelectiveFondoTargets.Of(1, 3), SelectiveApplyScope.Selected, At(0, 0, 0),
                new[] { Pos(0, 0), Pos(2, 0), Pos(1, 1) });

            Assert.Equal(
                new[] { (1, 0, 0), (1, 1, 1), (1, 2, 0), (3, 0, 0), (3, 1, 1), (3, 2, 0) },
                Tuples(plan));
            Assert.Equal(new[] { 1, 3 }, plan.Fondos);
            Assert.Empty(plan.OmittedCells);
        }

        [Fact]
        public void ASelectionMadeWhileLookingAtFondo0_DoesNotTouchFondo0WhenItIsNotATarget()
        {
            // Where the selection was PICKED is irrelevant: only the target set decides where it lands. Fondo 0 and
            // fondo 2 exist and hold those very coordinates, and neither may receive anything.
            var plan = SelectiveTargetResolver.Resolve(
                FourSquareFondos(), SelectiveFondoTargets.Of(1, 3), SelectiveApplyScope.Selected, At(0, 1, 1),
                new[] { Pos(0, 0), Pos(1, 1) });

            Assert.DoesNotContain(plan.Targets, target => target.FondoIndex == 0 || target.FondoIndex == 2);
            Assert.DoesNotContain(plan.OmittedCells, cell => cell.FondoIndex == 0 || cell.FondoIndex == 2);
            Assert.Equal(4, plan.Count); // 2 positions x 2 fondos
        }

        [Fact]
        public void ASelectionNeverAccumulatesPerFondo_TheSameSetResolvesTheSameWhicheverFondoIsBeingEdited()
        {
            // The anchor is the only thing that says which fondo is on screen, and Selected ignores it entirely.
            var topology = FourSquareFondos();
            var targets = SelectiveFondoTargets.Of(1, 3);
            var positions = new[] { Pos(0, 0), Pos(2, 2) };

            var fromFondo0 = SelectiveTargetResolver.Resolve(topology, targets, SelectiveApplyScope.Selected, At(0, 0, 0), positions);
            var fromFondo2 = SelectiveTargetResolver.Resolve(topology, targets, SelectiveApplyScope.Selected, At(2, 1, 1), positions);

            Assert.Equal(Tuples(fromFondo0), Tuples(fromFondo2));
        }

        // ---- The inner axis: Cell / Level (Row) / Front (Column) / All ----

        [Fact]
        public void Scope_Cell_ReachesOneCellPerTargetFondo()
        {
            var plan = SelectiveTargetResolver.Resolve(
                FourSquareFondos(), SelectiveFondoTargets.Of(0, 2), SelectiveApplyScope.Cell, At(0, 1, 2));

            Assert.Equal(new[] { (0, 1, 2), (2, 1, 2) }, Tuples(plan));
        }

        [Fact]
        public void Scope_Row_IsTheLevelScope_EveryFrenteAtTheAnchorLevel()
        {
            var plan = SelectiveTargetResolver.Resolve(
                FourSquareFondos(), SelectiveFondoTargets.Single(2), SelectiveApplyScope.Row, At(0, 0, 1));

            Assert.Equal(new[] { (2, 0, 1), (2, 1, 1), (2, 2, 1) }, Tuples(plan));
        }

        [Fact]
        public void Scope_Column_IsTheFrontScope_EveryLevelOfTheAnchorFrente()
        {
            var plan = SelectiveTargetResolver.Resolve(
                FourSquareFondos(), SelectiveFondoTargets.Single(2), SelectiveApplyScope.Column, At(0, 1, 0));

            Assert.Equal(new[] { (2, 1, 0), (2, 1, 1), (2, 1, 2) }, Tuples(plan));
        }

        [Fact]
        public void Scope_All_CoversTheWholeTargetFondo_AndIgnoresTheAnchor()
        {
            var missingAnchor = At(3, 9, 9); // All needs no anchor: a nonexistent one changes nothing
            var plan = SelectiveTargetResolver.Resolve(
                FourSquareFondos(), SelectiveFondoTargets.Single(1), SelectiveApplyScope.All, missingAnchor);

            Assert.False(plan.AnchorMissing);
            Assert.Equal(9, plan.Count);
            Assert.Equal(new[] { 1 }, plan.Fondos);
        }

        // ---- Divergent topologies: omit, never clamp, never create ----

        [Fact]
        public void DivergentTopologies_Row_SkipsEveryFrenteThatHasNoSuchLevel()
        {
            // Anchor level 2. fondo 0: three frentes of 3 -> all have it. fondo 1: one frente of 2 -> none.
            // fondo 2: frente 0 has 4 -> yes; frente 1 is empty -> no; frente 2 has 1 -> no. fondo 3: no frentes.
            var plan = SelectiveTargetResolver.Resolve(
                DivergentFondos(), SelectiveFondoTargets.Of(0, 1, 2, 3), SelectiveApplyScope.Row, At(0, 0, 2));

            Assert.Equal(new[] { (0, 0, 2), (0, 1, 2), (0, 2, 2), (2, 0, 2) }, Tuples(plan));
            Assert.Equal(new[] { 0, 2 }, plan.Fondos);
        }

        [Fact]
        public void DivergentTopologies_Column_SkipsFondosWithoutThatFrente_AndTheEmptyOne()
        {
            // Anchor frente 1. fondo 0 has it with 3 levels; fondo 1 has no frente 1; fondo 2's frente 1 is EMPTY.
            var plan = SelectiveTargetResolver.Resolve(
                DivergentFondos(), SelectiveFondoTargets.Of(0, 1, 2), SelectiveApplyScope.Column, At(0, 1, 0));

            Assert.Equal(new[] { (0, 1, 0), (0, 1, 1), (0, 1, 2) }, Tuples(plan));
        }

        [Fact]
        public void DivergentTopologies_Cell_OmitsMissingIndices_ItNeverClampsThemToTheNearestValidOne()
        {
            // Anchor (frente 2, nivel 2). fondo 1 has no frente 2; fondo 2's frente 2 has ONE level, so nivel 2 does
            // not exist there and must NOT become nivel 0; fondo 3 has no frente at all.
            var plan = SelectiveTargetResolver.Resolve(
                DivergentFondos(), SelectiveFondoTargets.Of(0, 1, 2, 3), SelectiveApplyScope.Cell, At(0, 2, 2));

            Assert.Equal(new[] { (0, 2, 2) }, Tuples(plan));
        }

        [Fact]
        public void DivergentTopologies_All_NeverCreatesACellForAnEmptyFrente()
        {
            var plan = SelectiveTargetResolver.Resolve(
                DivergentFondos(), SelectiveFondoTargets.Of(2, 3), SelectiveApplyScope.All, At(2, 0, 0));

            // fondo 2 = 4 + 0 + 1 cells; fondo 3 = none. A frente with zero levels contributes nothing.
            Assert.Equal(5, plan.Count);
            Assert.DoesNotContain(plan.Targets, target => target.FrontIndex == 1);
            Assert.Equal(new[] { 2 }, plan.Fondos);
        }

        [Fact]
        public void AnchorInAShorterFondo_ThanTheTargets_StillLendsItsCoordinates()
        {
            // The anchor is validated against ITS OWN fondo (1, frente 0, nivel 1 exists) and then reused as a pair
            // of coordinates in fondo 0, where it also exists.
            var plan = SelectiveTargetResolver.Resolve(
                DivergentFondos(), SelectiveFondoTargets.Of(0), SelectiveApplyScope.Cell, At(1, 0, 1));

            Assert.Equal(new[] { (0, 0, 1) }, Tuples(plan));
        }

        // ---- Determinism ----

        [Fact]
        public void Targets_AreAlwaysInCanonicalOrder_FondoThenFrenteThenNivel()
        {
            var plan = SelectiveTargetResolver.Resolve(
                DivergentFondos(), SelectiveFondoTargets.Of(2, 0), SelectiveApplyScope.All, At(0, 0, 0));

            for (var index = 1; index < plan.Targets.Count; index++)
            {
                Assert.True(plan.Targets[index - 1].CompareTo(plan.Targets[index]) < 0);
            }
        }

        [Fact]
        public void Selected_IsSortedAndDeduplicated_SoAnUnorderedSelectionGivesOneStableAnswer()
        {
            var topology = FourSquareFondos();
            var targets = SelectiveFondoTargets.Of(0, 1);
            var scrambled = new[] { Pos(2, 0), Pos(0, 2), Pos(2, 0), Pos(0, 0) };
            var reversed = scrambled.Reverse().ToArray();

            var first = SelectiveTargetResolver.Resolve(topology, targets, SelectiveApplyScope.Selected, At(0, 0, 0), scrambled);
            var second = SelectiveTargetResolver.Resolve(topology, targets, SelectiveApplyScope.Selected, At(0, 0, 0), reversed);

            // Three distinct positions x two fondos, in canonical order, with the duplicate collapsed once — not once
            // per fondo.
            Assert.Equal(
                new[] { (0, 0, 0), (0, 0, 2), (0, 2, 0), (1, 0, 0), (1, 0, 2), (1, 2, 0) },
                Tuples(first));
            Assert.Equal(Tuples(first), Tuples(second));
        }

        [Fact]
        public void ResolvingTwice_WithTheSameInputs_GivesTheSamePlan()
        {
            var topology = DivergentFondos();
            var targets = SelectiveFondoTargets.Of(0, 2);

            var first = SelectiveTargetResolver.Resolve(topology, targets, SelectiveApplyScope.Row, At(0, 0, 0));
            var second = SelectiveTargetResolver.Resolve(topology, targets, SelectiveApplyScope.Row, At(0, 0, 0));

            Assert.Equal(Tuples(first), Tuples(second));
            Assert.Equal(first.Fondos, second.Fondos);
        }

        // ---- Empty and invalid selections ----

        [Fact]
        public void NoTargetFondo_ReachesNothing_InEveryScope()
        {
            foreach (var scope in new[]
                     {
                         SelectiveApplyScope.Cell, SelectiveApplyScope.Row, SelectiveApplyScope.Column,
                         SelectiveApplyScope.All, SelectiveApplyScope.Selected
                     })
            {
                var plan = SelectiveTargetResolver.Resolve(
                    FourSquareFondos(), SelectiveFondoTargets.None, scope, At(0, 0, 0), new[] { Pos(0, 0) });

                Assert.True(plan.IsEmpty);
                Assert.Empty(plan.Fondos);
                Assert.Empty(plan.OmittedCells); // no target fondo means nothing to project onto, not a failed projection
            }
        }

        [Fact]
        public void TargetFondosTheRackDoesNotHave_AreOmittedAndReported()
        {
            var plan = SelectiveTargetResolver.Resolve(
                FourSquareFondos(), SelectiveFondoTargets.Of(-1, 1, 9), SelectiveApplyScope.All, At(0, 0, 0));

            Assert.Equal(new[] { 1 }, plan.Fondos);
            Assert.Equal(new[] { -1, 9 }, plan.OmittedFondos);
            Assert.Equal(9, plan.Count);
        }

        [Fact]
        public void AnchorThatDoesNotExist_RefusesTheWholeOperation_ForTheScopesThatNeedIt()
        {
            foreach (var scope in new[] { SelectiveApplyScope.Cell, SelectiveApplyScope.Row, SelectiveApplyScope.Column })
            {
                var plan = SelectiveTargetResolver.Resolve(
                    FourSquareFondos(), SelectiveFondoTargets.Of(1, 3), scope, At(0, 0, 7));

                Assert.True(plan.AnchorMissing);
                Assert.True(plan.IsEmpty);
                Assert.Contains("origen", plan.Describe());
            }
        }

        [Fact]
        public void EmptyTopology_And_NullArguments_ResolveToAnEmptyPlanInsteadOfThrowing()
        {
            var empty = SelectiveTargetResolver.Resolve(
                SelectiveTopology.Empty, SelectiveFondoTargets.Of(0), SelectiveApplyScope.All, At(0, 0, 0));
            Assert.True(empty.IsEmpty);
            Assert.Equal(new[] { 0 }, empty.OmittedFondos);

            var nulls = SelectiveTargetResolver.Resolve(null, null, SelectiveApplyScope.All, At(0, 0, 0));
            Assert.True(nulls.IsEmpty);
            Assert.Empty(nulls.OmittedFondos);

            var nullSelection = SelectiveTargetResolver.Resolve(
                FourSquareFondos(), SelectiveFondoTargets.All(FourSquareFondos()), SelectiveApplyScope.Selected, At(0, 0, 0));
            Assert.True(nullSelection.IsEmpty);
            Assert.Empty(nullSelection.OmittedCells);
        }

        [Fact]
        public void DivergentTopologies_TheSamePositionAppliesInOneTargetAndIsOmittedInAnother()
        {
            // Position (frente 1, nivel 0) exists in fondo 0 and NOT in fondo 2, whose frente 1 is empty. Only that
            // INSTANCE is dropped: the position still applies in fondo 0, and (frente 0, nivel 2) applies in both.
            var plan = SelectiveTargetResolver.Resolve(
                DivergentFondos(), SelectiveFondoTargets.Of(0, 2), SelectiveApplyScope.Selected, At(0, 0, 0),
                new[] { Pos(0, 2), Pos(1, 0) });

            Assert.Equal(new[] { (0, 0, 2), (0, 1, 0), (2, 0, 2) }, Tuples(plan));
            Assert.Equal(new[] { At(2, 1, 0) }, plan.OmittedCells);
            Assert.Equal(new[] { 0, 2 }, plan.Fondos);
        }

        [Fact]
        public void SelectedPositionsThatExistNowhere_AreOmittedPerInstance_NeverClampedNorCreated()
        {
            // Nivel 9 and nivel 5 exist in no frente of fondo 0, and frente 9 exists in no fondo at all. Nothing is
            // pulled down to the top level, nothing is pushed to the last frente, and no cell is invented.
            var plan = SelectiveTargetResolver.Resolve(
                DivergentFondos(), SelectiveFondoTargets.Of(0, 2), SelectiveApplyScope.Selected, At(0, 0, 0),
                new[] { Pos(0, 9), Pos(2, 5), Pos(9, 0) });

            Assert.True(plan.IsEmpty);
            Assert.Equal(
                new[] { At(0, 0, 9), At(0, 2, 5), At(0, 9, 0), At(2, 0, 9), At(2, 2, 5), At(2, 9, 0) },
                plan.OmittedCells);

            // Every omitted entry keeps the coordinates that were REQUESTED: none was moved to a valid neighbour.
            var requested = new[] { Pos(0, 9), Pos(2, 5), Pos(9, 0) };
            Assert.All(plan.OmittedCells, cell => Assert.Contains(cell.Position, requested));
        }

        [Fact]
        public void AnEmptySelection_ReachesNothingAndOmitsNothing()
        {
            var topology = FourSquareFondos();
            var targets = SelectiveFondoTargets.Of(1, 3);

            var empty = SelectiveTargetResolver.Resolve(
                topology, targets, SelectiveApplyScope.Selected, At(0, 0, 0), new SelectiveMatrixPosition[0]);
            var missing = SelectiveTargetResolver.Resolve(
                topology, targets, SelectiveApplyScope.Selected, At(0, 0, 0));

            foreach (var plan in new[] { empty, missing })
            {
                Assert.True(plan.IsEmpty);
                Assert.Empty(plan.Fondos);
                Assert.Empty(plan.OmittedCells);
                Assert.Empty(plan.OmittedFondos);
                Assert.False(plan.AnchorMissing);
            }
        }

        // ---- The plan is complete BEFORE anything mutates ----

        [Fact]
        public void ThePlan_NamesEveryFondoToRecomputeExactlyOnce()
        {
            var plan = SelectiveTargetResolver.Resolve(
                FourSquareFondos(), SelectiveFondoTargets.Of(3, 1, 3), SelectiveApplyScope.Row, At(0, 0, 0));

            // 6 cells across two fondos, but only TWO recomputes — and the list is distinct and ascending.
            Assert.Equal(6, plan.Count);
            Assert.Equal(new[] { 1, 3 }, plan.Fondos);
        }

        [Fact]
        public void EveryTargetOfAPlan_ExistsInTheTopologyItWasResolvedAgainst()
        {
            var topology = DivergentFondos();
            var plan = SelectiveTargetResolver.Resolve(
                topology, SelectiveFondoTargets.All(topology), SelectiveApplyScope.All, At(0, 0, 0));

            Assert.NotEmpty(plan.Targets);
            Assert.All(plan.Targets, target => Assert.True(topology.HasCell(target)));
            Assert.Equal(plan.Targets.Count, plan.Targets.Distinct().Count());
        }

        [Fact]
        public void Describe_SaysWhatWasReachedAndWhatWasLeftOut()
        {
            var reached = SelectiveTargetResolver.Resolve(
                FourSquareFondos(), SelectiveFondoTargets.Of(1, 3, 9), SelectiveApplyScope.Cell, At(0, 0, 0));
            Assert.Contains("2 celdas", reached.Describe());
            Assert.Contains("fondos 1, 3", reached.Describe());
            Assert.Contains("9", reached.Describe());

            var nothing = SelectiveTargetResolver.Resolve(
                FourSquareFondos(), SelectiveFondoTargets.None, SelectiveApplyScope.All, At(0, 0, 0));
            Assert.Contains("no se aplico nada", nothing.Describe());
        }
    }
}
