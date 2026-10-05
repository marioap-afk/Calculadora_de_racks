#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Xunit;
using static RackCad.Tests.CustodyMc;
using static RackCad.Tests.JournalSamples;
using static RackCad.Tests.StateV2Samples;

namespace RackCad.Tests
{
    /// <summary>
    /// I-62 F4 (slice F4-H), C-15 and C-16 (reproducible controls, run as guards) on disposable repositories with real SHAs: T12b from a Q0 whose journal
    /// is lost (Anexo F.2 and F.5: the designated successor reconstructs from Git only and closes the window ABANDONED with R-1 or R-2 and the conservative
    /// counters of B.8.5, never zero launches) and T12a (the journal is accessible and intact: a chained TRANSFER record and a Q7 that fixes the new
    /// holder, I-P07).
    /// </summary>
    public class I62F4CustodyReconstructionMcTests
    {
        private const string Designated = "B20261005T010101Z-ef56";
        private const string Trailer = "Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com>";
        private static readonly string[] Scope = { "src/", RedFile };

        private static bool Cell(string line) => line == Trailer;

        private static (CustodyRepo R, StatePoint Q0, string K3) UpToQ0()
        {
            var r = new CustodyRepo();
            r.Subst[Sha] = r.Main0;
            r.Subst[DigitSha] = r.Main0;
            var history = CustodyHistory();
            var k = string.Empty;
            for (var i = 0; i < 3; i++)
            {
                k = r.WritePoint(r.Holder, history[i], "I-99: punto rv" + (i + 1), push: false);
            }

            r.Git(r.Holder, "push", "-q", "origin", "HEAD");
            return (r, r.Read(r.Holder, k), k);
        }

        private static string WorkerCommit(CustodyRepo r, string path, string text, string message)
        {
            r.G.Write(r.Holder, path, text);
            r.G.Run(r.Holder, "add", "-A");
            r.G.Run(r.Holder, "commit", "-q", "-m", message + "\n\n" + Trailer);
            return r.Head(r.Holder);
        }

        /// <summary>The QR of B.8.5 by the designated successor (T12b): the window closed ABANDONED with the reconstruction record and the conservative counters.</summary>
        private static StatePoint Abandoned(StatePoint q0, string q0Commit, string remoteCase, string? red, params string[] unverified)
        {
            var (s, tree) = NewHolder(q0, 4, Designated, Designated);
            var custody = (YamlMap)s["custody"]!;
            var reconstruction = tree.PutJson("docs/automation/evidence/I-99-agent/T-01/w1/reconstruction.json", new JsonObject
            {
                ["Schema"] = "rackcad-relay-record/v2", ["TaskId"] = "T-01", ["Phase"] = "RECONSTRUCTION", ["WindowSeq"] = 1, ["Case"] = remoteCase,
            });
            custody["window"] = M(("seq", 1L), ("state", "CLOSED"));
            custody["task_intent"] = null;
            custody["last_window"] = M(("seq", 1L), ("task_id", "T-01"), ("attempt", 0L), ("closure", "ABANDONED"), ("closure_source", "SESSION"),
                ("delegation_run_id", "UNKNOWN"), ("verification", null), ("verified_sha", null), ("journal", null), ("reconstruction", reconstruction));
            if (red != null)
            {
                custody["chains"] = L(M(("task_id", "T-01"), ("state", "IN_COURSE"), ("chain_base_sha", q0Commit), ("chain_red_sha", red),
                    ("chain_red_files", L(RedFile)), ("continues_task_id", null), ("correction_authorized_pending", false)));
            }

            custody["unverified_commits"] = L(unverified.Select(u => (object?)M(("sha", u), ("task_id", "T-01"), ("window_seq", 1L), ("reason", "JOURNAL_UNAVAILABLE"),
                ("superseded_by", null))).ToArray());
            var min = DelegationJournal.ConservativeMinimum(remoteCase);
            ((YamlMap)s["counters"]!)["invocations"] = L(M(("scope", "F4"), ("launched", (long)min.Launched), ("uncertain", (long)min.Uncertain), ("cap", 12L),
                ("cap_source", tree.Put("docs/initiatives/I-99-contract.md", "# I-99\n\ncap: 12\n")), ("reconstructed", true), ("reconstruction", Clone(reconstruction))));
            return new StatePoint(s, tree);
        }

        [Fact]
        public void I62_C15_C16_T12b_AQ0WithoutJournalAndNoLaterCommitIsClosedAbandonedR1WithAnUncertainPlanning()
        {
            var (r, q0, k3) = UpToQ0();
            using var _r = r;
            var n = r.CleanClone("n");
            Assert.Equal("UNKNOWN", DelegationJournal.Derive(q0.State, null));
            Assert.Equal("R-1", DelegationJournal.ClassifyRemote(new GitProcessHistory(n), k3, r.Head(n), Scope, Cell));

            var k4 = r.WritePoint(n, Abandoned(r.Read(n, k3), k3, "R-1", null), "I-99: QR ABANDONED (R-1)");
            var qr = r.Read(n, k4);
            Clean(All(n, r.Read(n, k3), k3, qr, k4, k4), "QR R-1");
            Assert.Empty(DelegationJournal.ReconstructionProblems(q0.State, qr.State, "F4", "R-1"));

            // Never zero launches: a reconstruction that drops the uncertain PLANNING is below the conservative minimum.
            var zero = With(qr, s => ((YamlMap)Y.L(s, "counters.invocations")[0]!)["uncertain"] = 0L);
            Assert.NotEmpty(DelegationJournal.ReconstructionProblems(q0.State, zero.State, "F4", "R-1"));
        }

        [Fact]
        public void I62_C15_C16_T12b_AQ0WithoutJournalAndTheWorkersCommitsIsClosedAbandonedR2WithThemUnverified()
        {
            var (r, q0, k3) = UpToQ0();
            using var _r = r;
            var red = WorkerCommit(r, RedFile, "rojo\n", "T-01 RED");
            var green = WorkerCommit(r, GreenFile, "verde\n", "T-01 GREEN");
            r.Git(r.Holder, "push", "-q", "origin", "HEAD");

            // The holder vanishes with the journal (F.2: «durante 4, después de R» and «después de G, antes del handoff» without a journal).
            var n = r.CleanClone("n");
            Assert.Equal("R-2", DelegationJournal.ClassifyRemote(new GitProcessHistory(n), k3, green, Scope, Cell));
            var k4 = r.WritePoint(n, Abandoned(r.Read(n, k3), k3, "R-2", red, red, green), "I-99: QR ABANDONED (R-2)");
            var qr = r.Read(n, k4);
            Clean(All(n, r.Read(n, k3), k3, qr, k4, k4), "QR R-2");
            Assert.Empty(DelegationJournal.ReconstructionProblems(q0.State, qr.State, "F4", "R-2"));

            // The Worker's GREEN left out of unverified_commits: a commit of the window that no result of the closure accounts for (I-P03).
            r.Git(n, "reset", "-q", "--hard", green);
            var kx = r.WritePoint(n, Abandoned(r.Read(n, k3), k3, "R-2", red, red), "I-99: QR sin G", push: false);
            Assert.Contains("I-P03", Ids(All(n, r.Read(n, k3), k3, r.Read(n, kx), kx, kx)));
        }

        [Fact]
        public void I62_C15_T12a_WithAnIntactJournalTheTransferIsChainedAndTheClosingQ7FixesTheNewHolder()
        {
            var (r, q0, k3) = UpToQ0();
            using var _r = r;
            var red = WorkerCommit(r, RedFile, "rojo\n", "T-01 RED");
            var green = WorkerCommit(r, GreenFile, "verde\n", "T-01 GREEN");
            r.Subst[Sha3] = red;
            r.Subst[Sha2] = green;
            r.Git(r.Holder, "push", "-q", "origin", "HEAD");

            // P is orphaned inside the window; the journal is accessible and intact. N, designated, registers a chained TRANSFER and operates after it.
            var journal = Chain(Planning(Pass()), Work(), Record("TRANSFER", "COMPLETED", "ACCEPTED_OPEN", 1), Verification());
            Assert.Empty(DelegationJournal.ChainProblems(journal, 1));
            Assert.Empty(DelegationJournal.ExitProblems(q0.State, journal));
            Assert.Equal("ACCEPTED_OPEN", DelegationJournal.Derive(q0.State, journal.Take(3).ToList()));
            Assert.Equal("CLOSED_PENDING_CUSTODY", DelegationJournal.Derive(q0.State, journal));
            var forged = journal.Take(2).Concat(Chain(Record("TRANSFER", "COMPLETED", "ACCEPTED_OPEN", 1))).ToList();
            Assert.NotEmpty(DelegationJournal.ChainProblems(forged, 1));

            // The Q7 of N closes the window VERIFIED and fixes the new holder with its designation (I-P07 with the TRANSFER of the window).
            var n = r.CleanClone("n");
            var (s4, t4) = NewHolder(CustodyHistory()[3], 4, Designated, Designated, point: "Q7");
            var k4 = r.WritePoint(n, new StatePoint(s4, t4), "I-99: Q7 de N (T12a)");
            var p3 = r.Read(n, k3);
            var p4 = r.Read(n, k4);
            var git = new GitProcessHistory(n);
            var validator = new StateV2Validator();
            var all = new List<StateViolation>(validator.ValidateFile(p4));
            all.AddRange(validator.ValidatePair(p3, p4, new PairContext(TransferRecorded: true)));
            all.AddRange(validator.ValidatePairHistory(p3, k3, p4, k4, git, CustodyRepo.StatePath));
            all.AddRange(validator.ValidateHistory(p4, git, k4));
            Clean(all, "Q7 (T12a)");

            // Without a TRANSFER in the window, the holder changes outside a QR (I-P07).
            Assert.Contains("I-P07", Ids(validator.ValidatePair(p3, p4)));
        }
    }
}
