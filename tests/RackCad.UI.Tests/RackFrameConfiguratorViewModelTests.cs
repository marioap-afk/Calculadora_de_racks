using System;
using System.IO;
using System.Linq;
using RackCad.Application.RackFrames;
using RackCad.Domain.RackFrames;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// Unit tests for the header/cabecera <see cref="RackCad.UI.RackFrameConfiguratorViewModel"/> — the God-ViewModel that
    /// had ZERO tests (initiative I-24, hallazgo U3). It is UI-framework-independent (no Dispatcher) and recomputes its
    /// physical model SYNCHRONOUSLY after each edit, so these assert on <c>Configuration</c> right after the edit — no
    /// timing, no pixels, no global order. They lock the structural invariants (a frame of N horizontals has N-1 panels;
    /// the physical model is rebuilt on every edit), the bracing application, the BOM/persistence wiring and the negative
    /// guards. The pure geometry of members lives in <c>RackCad.Tests</c> (BracingPanelMemberBuilder) and is not
    /// re-tested here. Bodies run on the shared STA thread defensively; the VM works on any apartment.
    /// </summary>
    public sealed class RackFrameConfiguratorViewModelTests
    {
        private static RackFrameConfiguratorViewModel NewViewModel()
            => new RackFrameConfiguratorViewModel(new HardcodedStandardRackFrameService().CreateDefault());

        // ---- Construction + structural invariants ----

        [Fact]
        public void Default_BuildsConsistentPhysicalModel_WithOnePanelPerHorizontalGap()
        {
            // Guards the configurator's default: the standard cabecera must load into a self-consistent model whose panels
            // sit one-per-gap between consecutive horizontals (ARCHITECTURE §3.1). Regression: a broken default, a failed
            // normalize, or a physical model that never gets built.
            StaTestRunner.Run(() =>
            {
                var vm = NewViewModel();

                Assert.True(vm.Horizontals.Count >= 2);
                Assert.Equal(vm.Horizontals.Count - 1, vm.BracingSegments.Count); // N horizontals -> N-1 panels
                Assert.NotEmpty(vm.Configuration.Members);                          // physical model was built
                Assert.True(vm.IsModelConsistent);
                Assert.Empty(vm.ModelWarnings);
            });
        }

        [Fact]
        public void AddCommonSegment_AddsAHorizontalAndPanel_AndRebuildsTheModel()
        {
            // Adding a horizontal must grow the model by exactly one horizontal and one panel and rebuild the physical
            // members synchronously. Regression: an add that forgets to rebuild the panels or the members.
            StaTestRunner.Run(() =>
            {
                var vm = NewViewModel();
                var horizontals = vm.Horizontals.Count;
                var panels = vm.BracingSegments.Count;

                vm.AddCommonSegment(44.0);

                Assert.Equal(horizontals + 1, vm.Horizontals.Count);
                Assert.Equal(panels + 1, vm.BracingSegments.Count);
                Assert.Equal(vm.Horizontals.Count - 1, vm.BracingSegments.Count); // invariant survives the edit
                Assert.NotEmpty(vm.Configuration.Members);
                Assert.True(vm.IsModelConsistent);
            });
        }

        [Fact]
        public void SplitSelectedSegment_InsertsAHorizontalAtThePanelMidpoint()
        {
            // Splitting a panel inserts one horizontal at its midpoint, turning one panel into two. Regression: a split
            // that lands off-midpoint or fails to rebuild the panels.
            StaTestRunner.Run(() =>
            {
                var vm = NewViewModel();
                var panel = vm.BracingSegments[0];
                var midpoint = (panel.StartElevation + panel.EndElevation) / 2.0;
                var horizontals = vm.Horizontals.Count;

                vm.SelectedBracingSegment = panel;
                vm.SplitSelectedSegment();

                Assert.Equal(horizontals + 1, vm.Horizontals.Count);
                Assert.Contains(vm.Configuration.Horizontals, h => Math.Abs(h.Elevation - midpoint) < 0.01);
            });
        }

        [Fact]
        public void CombineSelectedSegments_RemovesOneSharedHorizontal()
        {
            // Combining a panel removes the shared horizontal, merging two panels into one. Regression: a combine that
            // removes the wrong horizontal or none.
            StaTestRunner.Run(() =>
            {
                var vm = NewViewModel();
                var horizontals = vm.Horizontals.Count;

                vm.SelectedBracingSegment = vm.BracingSegments[0];
                vm.CombineSelectedSegments();

                Assert.Equal(horizontals - 1, vm.Horizontals.Count);
                Assert.Equal(vm.Horizontals.Count - 1, vm.BracingSegments.Count); // invariant survives
            });
        }

        // ---- Bracing application flows through to the physical model ----

        [Fact]
        public void ApplyBracing_ToSelectedPanel_UpdatesPatternAndPhysicalModel()
        {
            // Applying a bracing arrangement to the selected panel must change its pattern AND be reflected in the rebuilt
            // physical members (double diagonal adds diagonal members that "no bracing" does not). Regression: the UI
            // pattern changing without the physical model following, or vice versa.
            StaTestRunner.Run(() =>
            {
                var vm = NewViewModel();
                vm.SelectedBracingSegment = vm.BracingSegments[0];

                vm.ApplyNoBracingToSelection();
                Assert.Equal(BracingPattern.NoBracing, vm.BracingSegments[0].Pattern);
                var membersWithoutBracing = vm.Configuration.Members.Count;

                vm.SelectedBracingSegment = vm.BracingSegments[0];
                vm.ApplyDoubleBracingToSelection();
                Assert.Equal(BracingPattern.DoubleDiagonal, vm.BracingSegments[0].Pattern);
                var membersWithDoubleBracing = vm.Configuration.Members.Count;

                Assert.True(membersWithDoubleBracing > membersWithoutBracing,
                    "double diagonal must add diagonal members to the rebuilt physical model");
            });
        }

        // ---- BOM wiring ----

        [Fact]
        public void BuildBom_ForTheDefault_ProducesLines()
        {
            // The configurator's BOM must reflect the current configuration. Regression: BuildBom wiring broken or the
            // configuration producing an empty bill.
            StaTestRunner.Run(() =>
            {
                var vm = NewViewModel();

                var bom = vm.BuildBom();

                Assert.NotNull(bom);
                Assert.NotEmpty(bom.Lines);
            });
        }

        // ---- Persistence round-trip through the VM's own load path ----

        [Fact]
        public void SaveThenLoadProject_RoundTripsAStructuralChange()
        {
            // A structural edit saved to disk and reopened through the VM's load path must survive: the reopened
            // configuration has the edited horizontal count, and it replaced the fresh default. Regression: the VM's
            // Save/Load (ReplaceConfigurationAndReload) dropping structure on reload.
            StaTestRunner.Run(() =>
            {
                var path = Path.Combine(Path.GetTempPath(), "rackcad-i24-" + Guid.NewGuid().ToString("N") + ".rackcad.json");
                try
                {
                    var source = NewViewModel();
                    source.AddCommonSegment(44.0); // structural change to carry across the round-trip
                    var expected = source.Configuration.Horizontals.Count;
                    source.SaveProjectTo(path);
                    Assert.True(File.Exists(path)); // save succeeded (SaveProjectTo swallows errors, so assert the file)

                    var target = NewViewModel();
                    var freshDefault = target.Configuration.Horizontals.Count;
                    Assert.NotEqual(expected, freshDefault); // the edit really changed the count (guards a vacuous test)

                    target.LoadProjectFrom(path);
                    Assert.Equal(expected, target.Configuration.Horizontals.Count);      // round-trip carried the edit
                    Assert.Equal(vmPanels(target), target.Horizontals.Count - 1);        // rows/panels rebuilt on load
                }
                finally
                {
                    if (File.Exists(path)) File.Delete(path);
                }
            });
        }

        private static int vmPanels(RackFrameConfiguratorViewModel vm) => vm.BracingSegments.Count;

        // ---- Negative / edge paths ----

        [Fact]
        public void DuplicateSelectedHorizontal_AtAnAlreadyOccupiedElevation_IsRejected()
        {
            // Adding a horizontal where one already exists is rejected (it would make a zero-height panel). The collision
            // is forced deterministically: duplicating the top horizontal creates one exactly PanelClear above it, so
            // duplicating the SAME top again targets that just-created elevation. Regression: the collision guard removed.
            StaTestRunner.Run(() =>
            {
                var vm = NewViewModel();
                var top = vm.Horizontals.OrderByDescending(h => h.Elevation).First();

                vm.SelectedHorizontal = top;
                vm.DuplicateSelectedHorizontal();              // adds one at top.Elevation + PanelClear (accepted)
                var countAfterFirst = vm.Horizontals.Count;

                vm.SelectedHorizontal = top;                    // re-select the same top
                vm.DuplicateSelectedHorizontal();              // targets the just-created elevation -> collision -> rejected

                Assert.Equal(countAfterFirst, vm.Horizontals.Count); // no horizontal was added the second time
                Assert.Equal("#B00020", vm.StatusBrush);             // the rejection status (red)
            });
        }

        [Fact]
        public void DeleteSelectedHorizontal_RefusesToDropBelowTwoHorizontals()
        {
            // At least two horizontals (floor + top) must remain. Deleting down to two, then once more, is refused.
            // Regression: a delete that removes the floor and produces an unusable frame.
            StaTestRunner.Run(() =>
            {
                var vm = NewViewModel();

                // Delete interior/top horizontals until exactly two remain.
                var guard = 0;
                while (vm.Horizontals.Count > 2 && guard++ < 50)
                {
                    vm.SelectedHorizontal = vm.Horizontals.OrderByDescending(h => h.Elevation).First();
                    vm.DeleteSelectedHorizontal();
                }

                Assert.Equal(2, vm.Horizontals.Count);

                vm.SelectedHorizontal = vm.Horizontals.OrderByDescending(h => h.Elevation).First();
                vm.DeleteSelectedHorizontal(); // one too many

                Assert.Equal(2, vm.Horizontals.Count);   // refused: still two
                Assert.Equal("#B00020", vm.StatusBrush); // red rejection status
            });
        }

        [Fact]
        public void SplitSelectedSegment_WithNoSelection_IsRejected()
        {
            // Splitting with no panel selected is a no-op with a red status, not a crash or a stray horizontal.
            // Regression: a missing null-guard on the selection.
            StaTestRunner.Run(() =>
            {
                var vm = NewViewModel();
                var horizontals = vm.Horizontals.Count;

                vm.SelectedBracingSegment = null;
                vm.SplitSelectedSegment();

                Assert.Equal(horizontals, vm.Horizontals.Count); // nothing added
                Assert.Equal("#B00020", vm.StatusBrush);
            });
        }

        [Fact]
        public void AddHorizontalSegment_AddsAHorizontalAndPanel_AndRebuildsTheModel()
        {
            // The alternate add entry point (opens a panel at the configured PanelClear above the top) must grow the model
            // by one horizontal and one panel and rebuild the physical members. Regression: AddHorizontalSegment not
            // rebuilding the panels/members.
            StaTestRunner.Run(() =>
            {
                var vm = NewViewModel();
                var horizontals = vm.Horizontals.Count;
                var panels = vm.BracingSegments.Count;

                vm.AddHorizontalSegment();

                Assert.Equal(horizontals + 1, vm.Horizontals.Count);
                Assert.Equal(panels + 1, vm.BracingSegments.Count);
                Assert.Equal(vm.Horizontals.Count - 1, vm.BracingSegments.Count); // invariant survives
                Assert.NotEmpty(vm.Configuration.Members);
                Assert.True(vm.IsModelConsistent);
            });
        }

        [Fact]
        public void SimpleDimensions_InvalidHeightOrDepth_AreRejectedAndIgnored()
        {
            // The quick-config height/depth parse guard: a non-positive or unparseable value is rejected with a red status
            // and the previously stored value is KEPT (the invalid parse is ignored). Regression: a bad dimension silently
            // overwriting the configured height/depth.
            StaTestRunner.Run(() =>
            {
                var vm = NewViewModel();

                vm.SimpleHeightText = "120";
                var height = vm.SimpleHeightText;
                vm.SimpleHeightText = "-5"; // invalid (non-positive)
                Assert.Equal("#B00020", vm.StatusBrush);
                Assert.Equal(height, vm.SimpleHeightText); // unchanged

                vm.SimpleDepthText = "48";
                var depth = vm.SimpleDepthText;
                vm.SimpleDepthText = "no-es-numero"; // invalid (unparseable)
                Assert.Equal("#B00020", vm.StatusBrush);
                Assert.Equal(depth, vm.SimpleDepthText); // unchanged
            });
        }

        [Fact]
        public void MultipleSelection_ApplyNoBracing_UpdatesEverySelectedPanel()
        {
            // Selecting several panels and applying an arrangement must touch EVERY selected panel (GetTargetSegments uses
            // the multi-selection set, not just SelectedBracingSegment). Regression: a bulk action that only affects one.
            StaTestRunner.Run(() =>
            {
                var vm = NewViewModel();
                Assert.True(vm.BracingSegments.Count >= 2);
                var first = vm.BracingSegments[0];
                var second = vm.BracingSegments[1];

                vm.SetSelectedSegments(new[] { first, second });
                vm.ApplyNoBracingToSelection();

                Assert.Equal(BracingPattern.NoBracing, first.Pattern);
                Assert.Equal(BracingPattern.NoBracing, second.Pattern);
            });
        }
    }
}
