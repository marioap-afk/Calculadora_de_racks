using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Systems.Shared;
using RackCad.Application.Views.Batch;
using RackCad.Application.Views.Preparation;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    public sealed class RackViewBatchPlanTests
    {
        private static readonly RackViewAddress Front = RackViewAddress.Fondo(1);
        private static readonly RackViewAddress Lateral = RackViewAddress.Post(2);
        private static readonly RackViewAddress Planta = RackViewAddress.Whole(DimensionViewKind.Planta);

        [Fact]
        public void OrderedThreeViewBatch_PreparesAndPlacesInRequestedOrder()
        {
            var port = new Port();
            var plan = RackViewBatchPlan<string>.Execute(New(Front, Lateral, Planta), port);

            Assert.Equal(RackViewBatchOutcome.COMPLETED, plan.Outcome);
            Assert.Equal(new[] { Front, Lateral, Planta }, plan.Report.RequestedOrder);
            Assert.Equal(new[] { Front, Lateral, Planta }, plan.Report.PlacedOrder);
            Assert.Equal(new[] { Front, Lateral, Planta }, port.Prepared);
            Assert.Equal(new[] { Front, Lateral, Planta }, port.Placed);
            Assert.True(port.Calls.LastIndexOf("prepare") < port.Calls.IndexOf("place"));
            Assert.DoesNotContain("redraw", port.Calls);
            Assert.True(plan.IsTerminal);
        }

        [Fact]
        public void RejectsMoreThanOneLogicalRackBeforeAnyCallback()
        {
            var port = new Port();
            var request = new RackViewBatchRequest(
                RackProductSourceKind.NewRack,
                RackSystemKind.SelectiveRack,
                new[] { Item("R-1", Front), Item("R-2", Lateral) });

            var plan = RackViewBatchPlan<string>.Execute(request, port);

            Assert.Equal(RackViewBatchOutcome.PREFLIGHT_FAILED, plan.Outcome);
            Assert.Equal(RackViewBatchStopReason.MULTIPLE_RACK_IDENTITIES, plan.Report.StopReason);
            Assert.Empty(port.Calls);
        }

        [Fact]
        public void VariantCancelStopsBeforeGatePreparationRedrawAndPlacement()
        {
            var port = new Port { CancelVariantAt = 1 };
            var plan = RackViewBatchPlan<string>.Execute(Existing(Front, Lateral), port);

            Assert.Equal(RackViewBatchOutcome.VARIANT_CANCELLED, plan.Outcome);
            Assert.Equal(0, plan.Report.PlacedCount);
            Assert.DoesNotContain("gate", port.Calls);
            Assert.Empty(port.Prepared);
            Assert.Empty(port.Placed);
            Assert.Equal(RackViewBatchRedrawState.NOT_STARTED, plan.Report.RedrawState);
        }

        [Fact]
        public void SiblingGateFailureStopsBeforePreparation()
        {
            var port = new Port { GateAccepted = false };
            var plan = RackViewBatchPlan<string>.Execute(Existing(Front), port);

            Assert.Equal(RackViewBatchOutcome.SIBLING_GATE_FAILED, plan.Outcome);
            Assert.Empty(port.Prepared);
            Assert.Equal(0, port.RedrawCalls);
            Assert.Empty(port.Placed);
        }

        [Fact]
        public void PrepareFailurePreventsRedrawAndEveryPlacement()
        {
            var port = new Port { PrepareFailsAt = 1 };
            var plan = RackViewBatchPlan<string>.Execute(Existing(Front, Lateral, Planta), port);

            Assert.Equal(RackViewBatchOutcome.PREPARE_FAILED, plan.Outcome);
            Assert.Equal(new[] { Front, Lateral }, port.Prepared);
            Assert.Equal(0, port.RedrawCalls);
            Assert.Empty(port.Placed);
            Assert.True(plan.IsTerminal);
        }

        [Fact]
        public void RedrawPreflightFailureAndRollbackPreventPlacement()
        {
            var preflight = new Port { Redraw = RackViewBatchRedrawResult.PreflightFailed("LOCKED") };
            var rolledBack = new Port { Redraw = RackViewBatchRedrawResult.RolledBack("FAULT") };

            var first = RackViewBatchPlan<string>.Execute(Existing(Front), preflight);
            var second = RackViewBatchPlan<string>.Execute(Existing(Front), rolledBack);

            Assert.Equal(RackViewBatchOutcome.PREFLIGHT_FAILED, first.Outcome);
            Assert.Equal(RackViewBatchOutcome.REDRAW_ROLLED_BACK, second.Outcome);
            Assert.Empty(preflight.Placed);
            Assert.Empty(rolledBack.Placed);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void AppliedOrNotRequiredRedrawStartsQueueExactlyOnce(bool applied)
        {
            var port = new Port
            {
                Redraw = applied ? RackViewBatchRedrawResult.Applied() : RackViewBatchRedrawResult.NotRequired()
            };

            var plan = RackViewBatchPlan<string>.Execute(Existing(Front, Lateral), port);

            Assert.Equal(RackViewBatchOutcome.COMPLETED, plan.Outcome);
            Assert.Equal(applied ? RackViewBatchRedrawState.REDRAW_APPLIED : RackViewBatchRedrawState.REDRAW_NOT_REQUIRED,
                plan.Report.RedrawState);
            Assert.Equal(1, port.RedrawCalls);
            Assert.Equal(new[] { Front, Lateral }, port.Placed);
        }

        [Fact]
        public void CancelFirstPlacementPreservesAppliedRedrawAndPlacesNothing()
        {
            var port = new Port { CancelPlacementAt = 0 };
            var plan = RackViewBatchPlan<string>.Execute(Existing(Front, Lateral), port);

            Assert.Equal(RackViewBatchOutcome.PLACEMENT_CANCELLED_PARTIAL_BATCH, plan.Outcome);
            Assert.Equal(RackViewBatchRedrawState.REDRAW_APPLIED, plan.Report.RedrawState);
            Assert.Equal(0, plan.Report.PlacedCount);
            Assert.Equal(Front, plan.Report.CurrentAddress);
        }

        [Fact]
        public void CancelOrEnterLaterPreservesEarlierPlacementsAndStopsTheRest()
        {
            var cancel = new Port { CancelPlacementAt = 1 };
            var enter = new Port { StopPlacementAt = 1 };

            var cancelled = RackViewBatchPlan<string>.Execute(Existing(Front, Lateral, Planta), cancel);
            var stopped = RackViewBatchPlan<string>.Execute(Existing(Front, Lateral, Planta), enter);

            Assert.Equal(RackViewBatchOutcome.PLACEMENT_CANCELLED_PARTIAL_BATCH, cancelled.Outcome);
            Assert.Equal(RackViewBatchOutcome.PLACEMENT_CANCELLED_PARTIAL_BATCH, stopped.Outcome);
            Assert.Equal(new[] { Front }, cancelled.Report.PlacedOrder);
            Assert.Equal(new[] { Front }, stopped.Report.PlacedOrder);
            Assert.DoesNotContain(Planta, cancel.Placed);
            Assert.DoesNotContain(Planta, enter.Placed);
        }

        [Fact]
        public void PlacementExceptionProducesPartialFailureAndPreservesEarlierPlacements()
        {
            var port = new Port { ThrowPlacementAt = 1 };
            var plan = RackViewBatchPlan<string>.Execute(Existing(Front, Lateral, Planta), port);

            Assert.Equal(RackViewBatchOutcome.PLACEMENT_FAILED_PARTIAL_BATCH, plan.Outcome);
            Assert.Equal(new[] { Front }, plan.Report.PlacedOrder);
            Assert.Equal(Lateral, plan.Report.CurrentAddress);
            Assert.DoesNotContain(Planta, port.Placed);
        }

        [Fact]
        public void MissingRequiredBlockIsAWarningAndDoesNotAbort()
        {
            var port = new Port { WarningAt = 0 };
            var plan = RackViewBatchPlan<string>.Execute(New(Front), port);

            Assert.Equal(RackViewBatchOutcome.COMPLETED, plan.Outcome);
            var warning = Assert.Single(plan.Report.Warnings);
            Assert.Equal("MISSING_REQUIRED_BLOCK", warning.Code);
            Assert.Equal(1, plan.Report.PlacedCount);
        }

        [Fact]
        public void FlowBedMultiViewIsRejectedBeforeCallbacks()
        {
            var port = new Port();
            var request = new RackViewBatchRequest(
                RackProductSourceKind.NewRack,
                RackSystemKind.Cama,
                new[] { Item("C-1", RackViewAddress.Whole(DimensionViewKind.Lateral)), Item("C-1", Planta) });

            var plan = RackViewBatchPlan<string>.Execute(request, port);

            Assert.Equal(RackViewBatchOutcome.PREPARE_FAILED, plan.Outcome);
            Assert.Equal(RackViewBatchStopReason.FLOW_BED_MULTI_VIEW_REJECTED, plan.Report.StopReason);
            Assert.Empty(port.Calls);
        }

        [Fact]
        public void ExistingPartialRackNeedsNoHistoricalFirstViewAndPreservesRackId()
        {
            var port = new Port();
            var plan = RackViewBatchPlan<string>.Execute(Existing(Front, Lateral), port);

            Assert.Equal(RackViewBatchOutcome.COMPLETED, plan.Outcome);
            Assert.Equal("EXISTING", plan.Report.RackId);
            Assert.All(plan.RequestedViews, view => Assert.Equal("EXISTING", view.RackId));
        }

        [Fact]
        public void DuplicateRequestsRemainOrderedAndAreNotDeduplicated()
        {
            var port = new Port();
            var plan = RackViewBatchPlan<string>.Execute(New(Front, Front, Planta), port);

            Assert.Equal(RackViewBatchOutcome.COMPLETED, plan.Outcome);
            Assert.Equal(new[] { Front, Front, Planta }, plan.Report.PlacedOrder);
        }

        private static RackViewBatchRequest New(params RackViewAddress[] addresses)
            => Request(RackProductSourceKind.NewRack, "NEW", addresses);

        private static RackViewBatchRequest Existing(params RackViewAddress[] addresses)
            => Request(RackProductSourceKind.ExistingRack, "EXISTING", addresses);

        private static RackViewBatchRequest Request(RackProductSourceKind source, string id, RackViewAddress[] addresses)
            => new RackViewBatchRequest(source, RackSystemKind.SelectiveRack, addresses.Select(a => Item(id, a)).ToArray());

        private static RackViewBatchItem Item(string id, RackViewAddress address) => new RackViewBatchItem(id, address);

        private sealed class Port : IRackViewBatchPort<string>
        {
            private int variantIndex;
            private int prepareIndex;
            private int placementIndex;

            public List<string> Calls { get; } = new List<string>();
            public List<RackViewAddress> Prepared { get; } = new List<RackViewAddress>();
            public List<RackViewAddress> Placed { get; } = new List<RackViewAddress>();
            public int CancelVariantAt { get; set; } = -1;
            public int PrepareFailsAt { get; set; } = -1;
            public int CancelPlacementAt { get; set; } = -1;
            public int StopPlacementAt { get; set; } = -1;
            public int ThrowPlacementAt { get; set; } = -1;
            public int WarningAt { get; set; } = -1;
            public bool GateAccepted { get; set; } = true;
            public int RedrawCalls { get; private set; }
            public RackViewBatchRedrawResult Redraw { get; set; } = RackViewBatchRedrawResult.Applied();

            public bool TryAcceptVariant(RackViewBatchItem requested, out RackViewBatchItem accepted, out string diagnostic)
            {
                Calls.Add("variant");
                accepted = requested;
                diagnostic = null;
                return variantIndex++ != CancelVariantAt;
            }

            public RackViewBatchGateResult CheckSiblingGate(RackViewBatchRequest request)
            {
                Calls.Add("gate");
                return GateAccepted ? RackViewBatchGateResult.Accept() : RackViewBatchGateResult.Reject("DIVERGENT");
            }

            public RackViewBatchPreparation<string> Prepare(RackViewBatchItem item)
            {
                Calls.Add("prepare");
                Prepared.Add(item.Address);
                var index = prepareIndex++;
                if (index == PrepareFailsAt) return RackViewBatchPreparation<string>.Failed("UNAVAILABLE");
                var warnings = index == WarningAt
                    ? new[] { new RackViewBatchWarning("MISSING_REQUIRED_BLOCK", item.Address, "KEY") }
                    : Array.Empty<RackViewBatchWarning>();
                return RackViewBatchPreparation<string>.Success(item.Address.ToString(), warnings);
            }

            public RackViewBatchRedrawResult RedrawExisting(IReadOnlyList<string> prepared)
            {
                Calls.Add("redraw");
                RedrawCalls++;
                return Redraw;
            }

            public RackViewBatchPlacementResult Place(RackViewBatchItem item, string prepared)
            {
                Calls.Add("place");
                var index = placementIndex++;
                if (index == ThrowPlacementAt) throw new InvalidOperationException("DRAG_ERROR");
                if (index == CancelPlacementAt) return RackViewBatchPlacementResult.Cancelled();
                if (index == StopPlacementAt) return RackViewBatchPlacementResult.Stopped();
                Placed.Add(item.Address);
                return RackViewBatchPlacementResult.Placed();
            }
        }
    }
}
