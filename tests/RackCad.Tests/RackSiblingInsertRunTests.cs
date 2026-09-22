using System;
using System.Collections.Generic;
using RackCad.Application.Views.Insertion;
using RackCad.Application.Views.Redraw;
using Xunit;

namespace RackCad.Tests
{
    public class RackSiblingInsertRunTests
    {
        [Fact]
        public void Successful_insert_uses_one_snapshot_for_both_gates_and_places_after_post()
        {
            var port = new Port();
            var result = RackSiblingInsertRun.Execute(port);

            Assert.Equal(RackSiblingInsertOutcome.PlacementApplied, result.Outcome);
            Assert.Equal(RackSiblingInsertOutcome.RedrawApplied, result.RedrawOutcome);
            Assert.Equal(1, port.ScanCalls);
            Assert.Same(port.Membership, port.PropertiesMembership);
            Assert.Same(port.Membership, port.AuthoredMembership);
            Assert.Equal(new[] { "scan", "variant", "properties", "authored", "prepare", "redraw-prepare", "mutate", "post", "top-tx", "place" }, port.Events);
            Assert.Equal(1, port.PrepareCalls);
        }

        [Fact]
        public void Variant_cancel_has_no_gate_prepare_mutation_or_placement()
        {
            var port = new Port { ChooseVariant = false };
            var result = RackSiblingInsertRun.Execute(port);
            Assert.Equal(RackSiblingInsertOutcome.VariantCancelled, result.Outcome);
            Assert.Equal(new[] { "scan", "variant" }, port.Events);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Rejected_product_gate_never_prepares_or_resolves(bool properties)
        {
            var port = new Port();
            if (properties) port.Properties = RackInsertGateResult.Reject("PROPERTIES_DIVERGENT");
            else port.Authored = RackInsertGateResult.Reject("AUTHORED_DIVERGENT");

            var result = RackSiblingInsertRun.Execute(port);
            Assert.Equal(RackSiblingInsertOutcome.PreflightFailed, result.Outcome);
            Assert.Equal(0, port.PrepareCalls);
            Assert.DoesNotContain("mutate", port.Events);
            Assert.DoesNotContain("place", port.Events);
        }

        [Fact]
        public void Prepare_failure_has_no_redraw_mutation_or_placement()
        {
            var port = new Port { PrepareSuccess = false };
            var result = RackSiblingInsertRun.Execute(port);
            Assert.Equal(RackSiblingInsertOutcome.PreflightFailed, result.Outcome);
            Assert.DoesNotContain("mutate", port.Events);
            Assert.DoesNotContain("place", port.Events);
        }

        [Fact]
        public void Locked_layer_from_redraw_prepare_fails_before_mutation()
        {
            var port = new Port { LockedLayer = true };
            var result = RackSiblingInsertRun.Execute(port);
            Assert.Equal(RackSiblingInsertOutcome.PreflightFailed, result.Outcome);
            Assert.Contains("LOCKED_LAYER", result.Diagnostic);
            Assert.DoesNotContain("mutate", port.Events);
        }

        [Fact]
        public void Discarded_atomic_redraw_never_places()
        {
            var port = new Port { DiscardMutation = true };
            var result = RackSiblingInsertRun.Execute(port);
            Assert.Equal(RackSiblingInsertOutcome.RedrawRolledBack, result.Outcome);
            Assert.DoesNotContain("post", port.Events);
            Assert.DoesNotContain("place", port.Events);
        }

        [Fact]
        public void Open_top_transaction_blocks_placement_after_committed_redraw()
        {
            var port = new Port { TopTransactionOpen = true };
            var result = RackSiblingInsertRun.Execute(port);
            Assert.Equal(RackSiblingInsertOutcome.PlacementBlocked, result.Outcome);
            Assert.Contains("post", port.Events);
            Assert.DoesNotContain("place", port.Events);
        }

        [Theory]
        [InlineData(RackInsertPlacementKind.Cancelled, RackSiblingInsertOutcome.PlacementCancelled)]
        [InlineData(RackInsertPlacementKind.Failed, RackSiblingInsertOutcome.PlacementFailed)]
        public void Placement_terminal_states_preserve_the_committed_redraw(
            RackInsertPlacementKind placement, RackSiblingInsertOutcome expected)
        {
            var port = new Port { Placement = placement };
            var result = RackSiblingInsertRun.Execute(port);
            Assert.Equal(expected, result.Outcome);
            Assert.Equal(RackSiblingInsertOutcome.RedrawApplied, result.RedrawOutcome);
        }

        [Fact]
        public void Orphan_only_is_deferred_to_mode_two_first_placement()
        {
            var port = new Port { OrphanOnly = true };
            var result = RackSiblingInsertRun.Execute(port);
            Assert.Equal(RackSiblingInsertOutcome.PlacementApplied, result.Outcome);
            Assert.Equal(RackSiblingRedrawDisposition.DeferToFirstPlacement, port.Deferred?.Disposition);
            Assert.DoesNotContain("mutate", port.Events);
            Assert.DoesNotContain("post", port.Events);
        }

        private sealed class Port : IRackSiblingInsertPort<string, string, string>, ISiblingRedrawPort<string>
        {
            internal readonly List<string> Events = new List<string>();
            internal int ScanCalls;
            internal int PrepareCalls;
            internal bool ChooseVariant = true;
            internal bool PrepareSuccess = true;
            internal bool LockedLayer;
            internal bool DiscardMutation;
            internal bool TopTransactionOpen;
            internal bool OrphanOnly;
            internal RackInsertPlacementKind Placement = RackInsertPlacementKind.Applied;
            internal RackInsertGateResult Properties = RackInsertGateResult.Proceed();
            internal RackInsertGateResult Authored = RackInsertGateResult.Proceed();
            internal RackSiblingMembershipSnapshot Membership;
            internal RackSiblingMembershipSnapshot PropertiesMembership;
            internal RackSiblingMembershipSnapshot AuthoredMembership;
            internal RackSiblingRedrawPlan<string> Deferred;

            public RackSiblingMembershipSnapshot ScanAndClassifyOnce()
            {
                Events.Add("scan");
                ScanCalls++;
                Membership = RackSiblingMembership.Classify(new[]
                {
                    new RackSiblingScanFact("D1", "frontal", true, false, true, "rack", "rack", OrphanOnly ? 0 : 1, 0, OrphanOnly)
                }, "rack", "rack", true);
                return Membership;
            }

            public bool TryChooseVariant(out string variant) { Events.Add("variant"); variant = "frontal"; return ChooseVariant; }
            public RackInsertGateResult CheckCustomProperties(RackSiblingMembershipSnapshot membership) { Events.Add("properties"); PropertiesMembership = membership; return Properties; }
            public RackInsertGateResult CheckAuthored(RackSiblingMembershipSnapshot membership) { Events.Add("authored"); AuthoredMembership = membership; return Authored; }
            public bool TryPrepare(string variant, RackSiblingMembershipSnapshot membership, out string prepared, out string diagnostic)
            { Events.Add("prepare"); PrepareCalls++; prepared = PrepareSuccess ? "prepared" : null; diagnostic = PrepareSuccess ? null : "PREPARE_FAILED"; return PrepareSuccess; }
            public ISiblingRedrawPort<string> CreateRedrawPort(string prepared) => this;
            public bool HasOpenTopTransaction { get { Events.Add("top-tx"); return TopTransactionOpen; } }
            public RackInsertPlacementResult Place(string prepared, RackSiblingRedrawPlan<string> deferredRedraw)
            {
                Events.Add("place"); Deferred = deferredRedraw;
                return Placement == RackInsertPlacementKind.Applied ? RackInsertPlacementResult.Applied()
                    : Placement == RackInsertPlacementKind.Cancelled ? RackInsertPlacementResult.Cancelled()
                    : RackInsertPlacementResult.Failed("placement failed");
            }
            public RackSiblingUnitPreparation<string> Prepare(RackSiblingMember member)
            { Events.Add("redraw-prepare"); return RackSiblingUnitPreparation<string>.Prepared(member.Fact.DefinitionKey, LockedLayer ? new[] { new RackSiblingLayerRequirement("LOCKED", true) } : null); }
            public RackSiblingMutationResult Mutate(IReadOnlyList<string> units)
            { Events.Add("mutate"); return DiscardMutation ? RackSiblingMutationResult.Discarded("D1", "boom") : RackSiblingMutationResult.Committed(); }
            public void Post(RackSiblingRedrawPlan<string> plan, RackSiblingMutationResult committed) => Events.Add("post");
        }
    }
}
