#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using Xunit;
using static RackCad.Tests.JournalSamples;
using static RackCad.Tests.StateV2Samples;

namespace RackCad.Tests
{
    /// <summary>
    /// I-62 F4 (slice F4-G), C-15 (the derived delegation state at each step) and C-16 (counters): the chained journal of a window
    /// (<see cref="DelegationJournal"/>) — DS(L, J) of B.8.2 with every row of its table, a broken or foreign chain as UNKNOWN, the Exit of each record
    /// consistent with the journal before it, the Q7 counters taken exactly from the journal (a contradiction or an absent counter is S-04), an uncertain
    /// launch counted as uncertain, and the conservative minimum of B.8.5 that a reconstruction can raise but never lower.
    /// </summary>
    public class I62F4JournalCounterTests
    {
        private static YamlMap Q0() => Point(3).State;

        [Fact]
        public void I62_C15_TheDerivedDelegationStateFollowsEveryRowOfB82()
        {
            Assert.Equal("NONE", DelegationJournal.Derive(Point(5).State, null));
            var planned = Clone(Point(5).State);
            ((YamlMap)planned["custody"]!)["task_intent"] = Clone((YamlMap)Y.M(Q0(), "custody.task_intent")!);
            Assert.Equal("PLANNED", DelegationJournal.Derive(planned, null));
            Assert.Equal("UNKNOWN", DelegationJournal.Derive(Q0(), null));
            Assert.Equal("PLANNED", DelegationJournal.Derive(Q0(), new List<byte[]>()));
            Assert.Equal("PLANNED", DelegationJournal.Derive(Q0(), Chain(Planning(null))));
            Assert.Equal("PLANNED", DelegationJournal.Derive(Q0(), Chain(Planning(Pass(failing: 3)))));
            Assert.Equal("ACCEPTED_OPEN", DelegationJournal.Derive(Q0(), Chain(Planning(Pass()))));
            Assert.Equal("ACCEPTED_OPEN", DelegationJournal.Derive(Q0(), Chain(Planning(Pass()), Work())));
            Assert.Equal("CLOSED_PENDING_CUSTODY", DelegationJournal.Derive(Q0(), Chain(Planning(Pass()), Work(), Verification())));
            Assert.Equal("CLOSED_PENDING_CUSTODY", DelegationJournal.Derive(Q0(), Chain(Planning(Pass()), Work("NO_OUTPUT", "STOP"))));
        }

        [Fact]
        public void I62_C15_ABrokenForeignOrTamperedChainIsUnknownAndNeverAnOpenOrClosedDelegation()
        {
            var good = Chain(Planning(Pass()), Work(), Verification());
            Assert.Empty(DelegationJournal.ChainProblems(good, 1));

            var broken = Chain(Planning(Pass()), Work(), Verification());
            var r2 = (JsonObject)JsonNode.Parse(Encoding.UTF8.GetString(broken[1]))!;
            r2["PrevRelaySha256"] = new string('0', 64);
            broken[1] = Encoding.UTF8.GetBytes(r2.ToJsonString() + "\n");
            Assert.Equal("UNKNOWN", DelegationJournal.Derive(Q0(), broken));

            var tampered = Chain(Planning(Pass()), Work(), Verification());
            tampered[0] = Encoding.UTF8.GetBytes(Encoding.UTF8.GetString(tampered[0]).Replace("\"Attempt\":0", "\"Attempt\":1", StringComparison.Ordinal));
            Assert.Contains(DelegationJournal.ChainProblems(tampered, 1), p => p.Contains("breaks the chain", StringComparison.Ordinal));
            Assert.Equal("UNKNOWN", DelegationJournal.Derive(Q0(), tampered));

            Assert.Equal("UNKNOWN", DelegationJournal.Derive(Q0(), Chain(Planning(Pass()), Record("WORK", "COMPLETED", "ACCEPTED_OPEN", 1, window: 2))));
        }

        [Fact]
        public void I62_C15_TheExitOfEachRecordStatesTheJournalBeforeIt()
        {
            Assert.Empty(DelegationJournal.ExitProblems(Q0(), Chain(Planning(Pass()), Work(), Verification())));
            var lying = Chain(Planning(Pass()), Record("WORK", "COMPLETED", "PLANNED", 0), Verification());
            Assert.Contains(DelegationJournal.ExitProblems(Q0(), lying), p => p.StartsWith("record 1", StringComparison.Ordinal));
            var open = Chain(Planning(Pass()), Record("WORK", "COMPLETED", "ACCEPTED_OPEN", 0));
            Assert.NotEmpty(DelegationJournal.ExitProblems(Q0(), open));
        }

        private static YamlMap Q7(long launched, long uncertain, long recoveries = 0)
        {
            var q7 = Clone(Point(4).State);
            var inv = (YamlMap)Y.L(q7, "counters.invocations")[0]!;
            inv["launched"] = launched;
            inv["uncertain"] = uncertain;
            if (recoveries > 0)
            {
                ((YamlMap)q7["counters"]!)["rebase_recoveries"] = L(M(("task_id", "T-01"), ("count", recoveries), ("last_rebase_map", M(("path", WindowMapPath), ("blob", Sha2)))));
            }

            return q7;
        }

        [Fact]
        public void I62_C16_TheQ7CountersAreExactlyWhatTheJournalProves()
        {
            var journal = Chain(Planning(Pass()), Work(), Verification());
            Assert.Equal(new JournalCounts(3, 0, 0), DelegationJournal.Counts(journal));
            Assert.Empty(DelegationJournal.Q7CounterProblems(Q0(), Q7(3, 0), journal, "F4"));

            // A contradictory or an absent counter is S-04.
            Assert.Contains(DelegationJournal.Q7CounterProblems(Q0(), Q7(2, 0), journal, "F4"), p => p.StartsWith("S-04: launched", StringComparison.Ordinal));
            Assert.Contains(DelegationJournal.Q7CounterProblems(Q0(), Q7(3, 0), journal, "F5"), p => p.StartsWith("S-04: the Q7 has no invocation counter", StringComparison.Ordinal));

            // An uncertain launch counts as launched-uncertain, never as zero; a rejected one before invocation does not count.
            var uncertain = Chain(Planning(Pass()), Work("LAUNCH_UNCERTAIN"), Record("VERIFICATION", "REJECTED_BEFORE_INVOCATION", "ACCEPTED_OPEN", 1));
            Assert.Equal(new JournalCounts(1, 1, 0), DelegationJournal.Counts(uncertain));
            Assert.Empty(DelegationJournal.Q7CounterProblems(Q0(), Q7(1, 1), uncertain, "F4"));
            Assert.NotEmpty(DelegationJournal.Q7CounterProblems(Q0(), Q7(1, 0), uncertain, "F4"));

            // A rebase of 16.7 inside the window is a recovery.
            var rebased = Chain(Planning(Pass()), Work(), Record("REBASE", "COMPLETED", "ACCEPTED_OPEN", 1), Verification());
            Assert.NotEmpty(DelegationJournal.Q7CounterProblems(Q0(), Q7(4, 0), rebased, "F4"));
            Assert.Empty(DelegationJournal.Q7CounterProblems(Q0(), Q7(4, 0, recoveries: 1), rebased, "F4"));
        }

        [Fact]
        public void I62_C16_AReconstructionWithoutJournalIsConservativeAndCanOnlyRaiseTheCounters()
        {
            Assert.Equal(new JournalCounts(0, 1, 0), DelegationJournal.ConservativeMinimum("R-1"));
            Assert.Equal(new JournalCounts(2, 1, 0), DelegationJournal.ConservativeMinimum("R-2"));
            Assert.Throws<ArgumentException>(() => DelegationJournal.ConservativeMinimum("R-3"));

            YamlMap Qr(long launched, long uncertain, bool reconstructed)
            {
                var qr = Q7(launched, uncertain);
                ((YamlMap)Y.L(qr, "counters.invocations")[0]!)["reconstructed"] = reconstructed;
                return qr;
            }

            Assert.Empty(DelegationJournal.ReconstructionProblems(Q0(), Qr(2, 1, true), "F4", "R-2"));
            Assert.Empty(DelegationJournal.ReconstructionProblems(Q0(), Qr(3, 2, true), "F4", "R-2"));
            Assert.NotEmpty(DelegationJournal.ReconstructionProblems(Q0(), Qr(1, 1, true), "F4", "R-2"));
            Assert.NotEmpty(DelegationJournal.ReconstructionProblems(Q0(), Qr(2, 0, true), "F4", "R-2"));
            Assert.NotEmpty(DelegationJournal.ReconstructionProblems(Q0(), Qr(2, 1, false), "F4", "R-2"));
            Assert.Empty(DelegationJournal.ReconstructionProblems(Q0(), Qr(0, 1, true), "F4", "R-1"));
        }

        [Fact]
        public void I62_C16_CorrectionsCountLaunchesNotVerificationsAndAContinuationInheritsThem()
        {
            // T-01 has one correction of class Ci, launched in its Q0 (§9.3: the entry of counters.correction_launches in the same Q0).
            var s = Clone(Point(4).State);
            var launches = Y.L(s, "counters.correction_launches");
            YamlMap Launch(long seq, string task, string failureClass) => M(("seq", seq), ("task_id", task), ("failure_class", failureClass),
                ("correction_of_run_id", "R20261003T010101Z-ab12"), ("attempts_after", seq), ("record_version", 3L));
            launches.Add(Launch(1, "T-01", "Ci"));

            // Two verifications of the same delivery are two launches of the journal, never two corrections.
            var journal = Chain(Planning(Pass()), Work(), Verification(), Verification());
            Assert.Equal(4, DelegationJournal.Counts(journal).Launched);
            Assert.Equal(1, DelegationJournal.ClassLaunches(s, "T-01", "Ci"));
            Assert.Equal(0, DelegationJournal.ClassLaunches(s, "T-01", "Red"));

            // A correction launched and lost before its verification still counts (the window closes ABANDONED; the entry is durable).
            var lost = Clone(s);
            ((YamlMap)Y.M(lost, "custody.last_window")!)["closure"] = "ABANDONED";
            Assert.Equal(1, DelegationJournal.ClassLaunches(lost, "T-01", "Ci"));

            // T-02 continues T-01 (a new TaskId by the Coordinator's decision): it inherits the class count and its next correction is the second.
            var custody = (YamlMap)s["custody"]!;
            custody["task_intent"] = M(("task_id", "T-02"), ("attempt", 1L), ("kind", "CORRECTION"), ("contract", null), ("continues_task_id", "T-01"),
                ("planned_roles", L()));
            Assert.Equal(new[] { "T-02", "T-01" }, DelegationJournal.ContinuationLineage(s, "T-02"));
            Assert.Equal(1, DelegationJournal.ClassLaunches(s, "T-02", "Ci"));
            Assert.Empty(DelegationJournal.ContinuationProblems(s));
            launches.Add(Launch(2, "T-02", "Ci"));
            Assert.Equal(2, DelegationJournal.ClassLaunches(s, "T-02", "Ci"));
            Assert.Equal(1, DelegationJournal.ClassLaunches(s, "T-01", "Ci"));

            // Without ContinuesTaskId the same work would restart the class at zero; a continuation of itself, of a task without a chain, or in a cycle is S-04.
            var restarted = Clone(s);
            ((YamlMap)Y.M(restarted, "custody.task_intent")!)["continues_task_id"] = null;
            Assert.Equal(1, DelegationJournal.ClassLaunches(restarted, "T-02", "Ci"));
            var self = Clone(s);
            ((YamlMap)Y.M(self, "custody.task_intent")!)["continues_task_id"] = "T-02";
            Assert.Contains(DelegationJournal.ContinuationProblems(self), p => p.Contains("continues itself", StringComparison.Ordinal));
            var unknown = Clone(s);
            ((YamlMap)Y.M(unknown, "custody.task_intent")!)["continues_task_id"] = "T-09";
            Assert.Contains(DelegationJournal.ContinuationProblems(unknown), p => p.Contains("no durable chain", StringComparison.Ordinal));
            var cycle = Clone(s);
            ((YamlMap)Y.L(cycle, "custody.chains")[0]!)["continues_task_id"] = "T-02";
            Assert.Contains(DelegationJournal.ContinuationProblems(cycle), p => p.Contains("cycle", StringComparison.Ordinal));
        }
    }
}
