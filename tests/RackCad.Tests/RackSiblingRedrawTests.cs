using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Views.Redraw;
using Xunit;

namespace RackCad.Tests
{
    public class RackSiblingMembershipTests
    {
        [Fact] public void SameRackDirectMemberIsRedraw() => Assert.Equal(RackSiblingMembershipKind.Redraw, Only(F("D", rackId: "R")));
        [Fact] public void XrefMemberIsReadOnly() => Assert.Equal(RackSiblingMembershipKind.ReadOnly, Only(F("D", xref: true, rackId: "R")));
        [Fact] public void UnreadableSameProbeBlocks() => Assert.Equal(RackSiblingMembershipKind.BlockingUnreadable, Only(F("D", readable: false, probeId: "R")));
        [Fact] public void UnreadableOtherProbeIsNotMember() => Assert.Equal(RackSiblingMembershipKind.NotMember, Only(F("D", readable: false, probeId: "Q")));
        [Fact] public void UnreadableWithoutProbeIsNotMember() => Assert.Equal(RackSiblingMembershipKind.NotMember, Only(F("D", readable: false)));

        [Fact]
        public void SelectedBlankSourceAlwaysParticipates()
        {
            Assert.Equal(RackSiblingMembershipKind.Redraw, Only(F("SOURCE", selected: true, rackId: "   ", refs: 1)));
        }

        [Fact]
        public void NestedReferencesDoNotMakeASurvivor()
        {
            var member = Snapshot(F("D", rackId: "R", nested: 4)).Members.Single();
            Assert.False(member.IsSurvivor);
        }

        [Fact]
        public void SurvivorRequiresDirectLayoutReference()
        {
            var member = Snapshot(F("D", rackId: "R", refs: 1)).Members.Single();
            Assert.True(member.IsSurvivor);
        }

        [Fact]
        public void MembershipOutputIsTheReusableAuthoritySet()
        {
            var snapshot = Snapshot(F("A", rackId: "R"), F("B", rackId: "R", erase: true), F("C", xref: true, rackId: "R"));
            Assert.Same(snapshot.MutableMembers, snapshot.CustomPropertiesGateMembers);
            Assert.Same(snapshot.MutableMembers, snapshot.AuthoredGateMembers);
            var attributable = snapshot.MutableMembers.Select(x => x.Fact.DefinitionKey);
            Assert.Equal(new[] { "A", "B" }, attributable);
        }

        [Fact]
        public void SelectiveOrHeaderOriginalWhitespaceIdentityCanBeAttributed()
        {
            var fact = F("D", rackId: "   ");
            var snapshot = RackSiblingMembership.Classify(new[] { fact }, "CURADO", "   ", true);
            Assert.Equal(RackSiblingMembershipKind.Redraw, snapshot.Members.Single().Kind);
        }

        [Fact]
        public void OriginalIdentityIsNotAttributedForOtherKinds()
        {
            var fact = F("D", rackId: "ORIGINAL");
            var snapshot = RackSiblingMembership.Classify(new[] { fact }, "CURADO", "ORIGINAL", false);
            Assert.Equal(RackSiblingMembershipKind.NotMember, snapshot.Members.Single().Kind);
        }

        [Fact]
        public void XrefWithAttributedOriginalIdentityIsReadOnly()
        {
            var fact = F("D", xref: true, rackId: "ORIGINAL");
            var snapshot = RackSiblingMembership.Classify(new[] { fact }, "CURADO", "ORIGINAL", true);
            Assert.Equal(RackSiblingMembershipKind.ReadOnly, snapshot.Members.Single().Kind);
        }

        internal static RackSiblingScanFact F(string key, bool selected = false, bool xref = false, bool readable = true,
            string rackId = null, string probeId = null, int refs = 0, int nested = 0, bool erase = false)
            => new RackSiblingScanFact(key, "Frontal", selected, xref, readable, rackId, probeId, refs, nested, erase);

        internal static RackSiblingMembershipSnapshot Snapshot(params RackSiblingScanFact[] facts)
            => RackSiblingMembership.Classify(facts, "R", null, false);

        private static RackSiblingMembershipKind Only(RackSiblingScanFact fact) => Snapshot(fact).Members.Single().Kind;
    }

    public class RackSiblingRedrawPlanTests
    {
        [Fact]
        public void PlanSeparatesAllCategoriesAndSurvivors()
        {
            var membership = RackSiblingMembershipTests.Snapshot(
                RackSiblingMembershipTests.F("R1", rackId: "R", refs: 1),
                RackSiblingMembershipTests.F("E1", rackId: "R", erase: true),
                RackSiblingMembershipTests.F("X1", xref: true, rackId: "R"),
                RackSiblingMembershipTests.F("U1", readable: false, probeId: "R"));
            var prepared = membership.Members.Where(x => x.Kind == RackSiblingMembershipKind.Redraw || x.Kind == RackSiblingMembershipKind.Erase)
                .Select(x => new RackSiblingPreparedUnit<string>(x, x.Fact.DefinitionKey)).ToArray();

            var plan = RackSiblingRedrawPlan<string>.Create(membership, prepared);

            Assert.Single(plan.RedrawUnits);
            Assert.Single(plan.EraseUnits);
            Assert.Single(plan.ReadOnly);
            Assert.Single(plan.Blocking);
            Assert.Single(plan.Survivors);
            Assert.Equal(RackSiblingRedrawDisposition.NoMutation, plan.Disposition);
        }

        [Fact]
        public void OrphanOnlyCaseIsDeferredForG9b()
        {
            var membership = RackSiblingMembershipTests.Snapshot(RackSiblingMembershipTests.F("E", rackId: "R", erase: true));
            var unit = new RackSiblingPreparedUnit<string>(membership.Members.Single(), "E");
            var plan = RackSiblingRedrawPlan<string>.Create(membership, new[] { unit });
            Assert.Equal(RackSiblingRedrawDisposition.DeferToFirstPlacement, plan.Disposition);
        }
    }

    public class RackSiblingRedrawRunTests
    {
        [Fact]
        public void ReadOnlyAndUnrelatedDefinitionsAreNeverPreparedForWrite()
        {
            var membership = RackSiblingMembershipTests.Snapshot(
                RackSiblingMembershipTests.F("R", rackId: "R", refs: 1),
                RackSiblingMembershipTests.F("X", xref: true, rackId: "R"),
                RackSiblingMembershipTests.F("OTHER", rackId: "Q"));
            var port = new FakePort();
            RackSiblingRedrawRun.Execute(membership, port);
            Assert.Equal(new[] { "R" }, port.PreparedDefinitions);
        }

        [Fact]
        public void EveryUnitIsPreparedBeforeMutateStarts()
        {
            var port = new FakePort();
            RackSiblingRedrawRun.Execute(Membership("D1", "D2", "D3"), port);
            Assert.Equal(3, port.PreparedDefinitions.Count);
            Assert.Equal(3, port.PreparedCountObservedByMutate);
        }

        [Fact]
        public void LockedLayerFailsBeforeMutate()
        {
            var port = new FakePort { LockedDefinition = "D2" };
            var result = RackSiblingRedrawRun.Execute(Membership("D1", "D2"), port);
            Assert.Equal(RackSiblingRedrawOutcome.PrepareFailed, result.Outcome);
            Assert.Equal(0, port.MutateCalls);
            Assert.Contains("D2", result.Diagnostic);
            Assert.Contains("LOCKED-D2", result.Diagnostic);
        }

        [Fact]
        public void NUnitsUseOneMutateOneTransactionOneCommitAndOnePostRegen()
        {
            var port = new FakePort();
            var result = RackSiblingRedrawRun.Execute(Membership("D1", "D2", "D3"), port);
            Assert.Equal(RackSiblingRedrawOutcome.Committed, result.Outcome);
            Assert.Equal(1, port.MutateCalls);
            Assert.Equal(1, port.TransactionCount);
            Assert.Equal(1, port.CommitCount);
            Assert.Equal(1, port.PostCalls);
            Assert.Equal(1, port.RegenCount);
        }

        [Fact]
        public void SecondUnitExceptionDiscardsEveryStagedChange()
        {
            var port = new FakePort { ThrowAt = 2 };
            var result = RackSiblingRedrawRun.Execute(Membership("D1", "D2"), port);
            Assert.Equal(RackSiblingRedrawOutcome.Discarded, result.Outcome);
            Assert.Empty(port.CommittedState);
            Assert.Equal(0, port.CommitCount);
            Assert.Equal(0, port.PostCalls);
        }

        [Fact]
        public void TypedFailureDiscardsAndDoesNotPost()
        {
            var port = new FakePort { FailAt = 2 };
            var result = RackSiblingRedrawRun.Execute(Membership("D1", "D2"), port);
            Assert.Equal(RackSiblingRedrawOutcome.Discarded, result.Outcome);
            Assert.Empty(port.CommittedState);
            Assert.Equal(0, port.PostCalls);
        }

        private static RackSiblingMembershipSnapshot Membership(params string[] definitions)
            => RackSiblingMembershipTests.Snapshot(definitions.Select(x => RackSiblingMembershipTests.F(x, rackId: "R", refs: 1)).ToArray());

        private sealed class FakePort : ISiblingRedrawPort<string>
        {
            internal string LockedDefinition;
            internal int ThrowAt;
            internal int FailAt;
            internal int MutateCalls;
            internal int TransactionCount;
            internal int CommitCount;
            internal int PostCalls;
            internal int RegenCount;
            internal int PreparedCountObservedByMutate;
            internal List<string> PreparedDefinitions { get; } = new List<string>();
            internal List<string> CommittedState { get; } = new List<string>();

            public RackSiblingUnitPreparation<string> Prepare(RackSiblingMember member)
            {
                var key = member.Fact.DefinitionKey;
                PreparedDefinitions.Add(key);
                var layers = new[] { new RackSiblingLayerRequirement("LOCKED-" + key, key == LockedDefinition) };
                return RackSiblingUnitPreparation<string>.Prepared(key, layers);
            }

            public RackSiblingMutationResult Mutate(IReadOnlyList<string> units)
            {
                MutateCalls++;
                PreparedCountObservedByMutate = PreparedDefinitions.Count;
                TransactionCount++;
                var staged = new List<string>(CommittedState);
                for (var i = 0; i < units.Count; i++)
                {
                    staged.Add(units[i]);
                    if (ThrowAt == i + 1) throw new InvalidOperationException("fault");
                    if (FailAt == i + 1) return RackSiblingMutationResult.Discarded(units[i], "typed failure");
                }

                CommittedState.Clear();
                CommittedState.AddRange(staged);
                CommitCount++;
                return RackSiblingMutationResult.Committed();
            }

            public void Post(RackSiblingRedrawPlan<string> plan, RackSiblingMutationResult committed)
            {
                PostCalls++;
                RegenCount++;
            }
        }
    }
}
