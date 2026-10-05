#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;

namespace RackCad.Tests
{
    /// <summary>The counts a window's journal proves (Proposal V14 §9.3, B.8.3): launches, uncertain launches and rebase records.</summary>
    public sealed record JournalCounts(int Launched, int Uncertain, int Rebases);

    /// <summary>
    /// The chained journal of a delegation window (AUTOMATION_PLAN 16.25; Proposal V14 §8.4, B.8.2, B.8.3, B.8.5): the <c>relay-record/v2</c> records of
    /// window k in order, each with <c>WindowSeq</c> = k and <c>PrevRelaySha256</c> = the SHA-256 of the previous record's bytes (null for the first).
    /// It derives the delegation state <c>DS(L, J)</c>, the counts a Q7 takes from the journal and, without a journal, the conservative minimum of B.8.5.
    /// </summary>
    public static class DelegationJournal
    {
        public static string Sha256(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

        /// <summary>Why the chain of window <paramref name="windowSeq"/> is not intact (empty = intact).</summary>
        public static List<string> ChainProblems(IReadOnlyList<byte[]> journal, long windowSeq)
        {
            var problems = new List<string>();
            for (var i = 0; i < journal.Count; i++)
            {
                JsonObject? r;
                try
                {
                    r = JsonNode.Parse(Encoding.UTF8.GetString(journal[i])) as JsonObject;
                }
                catch (System.Text.Json.JsonException)
                {
                    r = null;
                }

                if (r == null || J.S(r, "Schema") != "rackcad-relay-record/v2")
                {
                    problems.Add("record " + i + " is not a rackcad-relay-record/v2");
                    continue;
                }

                if (r["WindowSeq"]?.GetValue<long>() != windowSeq)
                {
                    problems.Add("record " + i + " belongs to another window");
                }

                var prev = J.S(r, "PrevRelaySha256");
                var expected = i == 0 ? null : Sha256(journal[i - 1]);
                if (prev != expected)
                {
                    problems.Add("record " + i + " breaks the chain (PrevRelaySha256)");
                }
            }

            return problems;
        }

        private static List<JsonObject> Records(IReadOnlyList<byte[]> journal) =>
            journal.Select(b => (JsonObject)JsonNode.Parse(Encoding.UTF8.GetString(b))!).ToList();

        private static bool Accepted(JsonObject r) =>
            J.S(r, "Phase") == "PLANNING" && r["Outcome"]?["Acceptance"] is JsonObject a
            && Enumerable.Range(1, 8).All(i => J.S(a, "A" + i) == "pass");

        private static bool ValidVerification(JsonObject r) => J.S(r, "Phase") == "VERIFICATION" && J.S(r, "Outcome.Kind") == "COMPLETED";

        private static bool DeclaredClosure(JsonObject r) => J.S(r, "Disposition") is "BLOCKED" or "STOP";

        /// <summary>
        /// DS(L, J) of B.8.2 with <paramref name="last"/> = the last durable point and <paramref name="journal"/> = its window's journal (null =
        /// unavailable): NONE, PLANNED, ACCEPTED_OPEN, CLOSED_PENDING_CUSTODY or UNKNOWN.
        /// </summary>
        public static string Derive(YamlMap last, IReadOnlyList<byte[]>? journal)
        {
            if (Y.S(last, "custody.window.state") != "OPENABLE")
            {
                return Y.M(last, "custody.task_intent") == null ? "NONE" : "PLANNED";
            }

            if (journal == null || ChainProblems(journal, Y.N(last, "custody.window.seq") ?? -1).Count > 0)
            {
                return "UNKNOWN";
            }

            var records = Records(journal);
            var accepted = records.FindIndex(Accepted);
            if (accepted < 0)
            {
                return "PLANNED";
            }

            return records.Skip(accepted + 1).Any(r => ValidVerification(r) || DeclaredClosure(r)) ? "CLOSED_PENDING_CUSTODY" : "ACCEPTED_OPEN";
        }

        /// <summary>
        /// The Exit of each record states the DS of the journal before it (B.8.2: <c>Exit.OpenDelegations</c> = 1 exactly with ACCEPTED_OPEN; with
        /// UNKNOWN there is no valid Exit).
        /// </summary>
        public static List<string> ExitProblems(YamlMap last, IReadOnlyList<byte[]> journal)
        {
            var problems = new List<string>();
            var records = Records(journal);
            for (var i = 0; i < records.Count; i++)
            {
                var ds = Derive(last, journal.Take(i).ToList());
                var status = J.S(records[i], "Exit.DelegationStatus");
                var open = records[i]["Exit"]?["OpenDelegations"]?.GetValue<long>();
                if (ds == "UNKNOWN" || status != ds || open != (ds == "ACCEPTED_OPEN" ? 1 : 0))
                {
                    problems.Add("record " + i + ": Exit says " + status + "/" + open + ", the journal before it gives " + ds);
                }
            }

            return problems;
        }

        /// <summary>What the journal proves for the Q7 counters: every launch with a RunId counts, an uncertain one as uncertain (§9.3).</summary>
        public static JournalCounts Counts(IReadOnlyList<byte[]> journal)
        {
            var records = Records(journal);
            var kinds = records.Select(r => J.S(r, "Outcome.Kind")).ToList();
            return new JournalCounts(
                kinds.Count(k => k != null && k != "REJECTED_BEFORE_INVOCATION" && k != "LAUNCH_UNCERTAIN"),
                kinds.Count(k => k == "LAUNCH_UNCERTAIN"),
                records.Count(r => J.S(r, "Phase") == "REBASE"));
        }

        /// <summary>
        /// The Q7 of window k takes its counters from the journal (B.8.3): the launched and uncertain invocations of the window's scope grow exactly by
        /// what the journal proves, and the rebase recoveries by its REBASE records. A contradiction is S-04.
        /// </summary>
        public static List<string> Q7CounterProblems(YamlMap q0, YamlMap q7, IReadOnlyList<byte[]> journal, string scope)
        {
            var problems = new List<string>();
            var counts = Counts(journal);
            YamlMap? Inv(YamlMap s) => Y.L(s, "counters.invocations").Cast<YamlMap>().FirstOrDefault(x => Y.S(x, "scope") == scope);
            var before = Inv(q0);
            var after = Inv(q7);
            if (after == null)
            {
                problems.Add("S-04: the Q7 has no invocation counter for scope " + scope);
                return problems;
            }

            long Get(YamlMap? m, string k) => m == null ? 0 : Y.N(m, k) ?? 0;
            if (Get(after, "launched") - Get(before, "launched") != counts.Launched)
            {
                problems.Add("S-04: launched grows by " + (Get(after, "launched") - Get(before, "launched")) + ", the journal proves " + counts.Launched);
            }

            if (Get(after, "uncertain") - Get(before, "uncertain") != counts.Uncertain)
            {
                problems.Add("S-04: uncertain grows by " + (Get(after, "uncertain") - Get(before, "uncertain")) + ", the journal proves " + counts.Uncertain);
            }

            var recoveries = Y.L(q7, "counters.rebase_recoveries").Cast<YamlMap>().Sum(x => Y.N(x, "count") ?? 0)
                             - Y.L(q0, "counters.rebase_recoveries").Cast<YamlMap>().Sum(x => Y.N(x, "count") ?? 0);
            if (recoveries != counts.Rebases)
            {
                problems.Add("S-04: rebase recoveries grow by " + recoveries + ", the journal records " + counts.Rebases + " rebases");
            }

            return problems;
        }

        /// <summary>
        /// B.8.5 step 2.e without a journal: per phase in order (PLANNING, WORK, VERIFICATION, NEGATIVE), launched = what an admitted source proves (R-2
        /// proves PLANNING and WORK) and uncertain = 1 when the phase could have launched (it is PLANNING, or the previous phase is proven) and nothing
        /// proves or excludes it. R-3 has no reconstruction (STOP).
        /// </summary>
        public static JournalCounts ConservativeMinimum(string remoteCase)
        {
            var proven = remoteCase switch
            {
                "R-1" => new[] { false, false, false, false },
                "R-2" => new[] { true, true, false, false },
                _ => throw new ArgumentException("no reconstruction for " + remoteCase + " (T10(c), STOP)"),
            };
            var launched = proven.Count(p => p);
            var uncertain = Enumerable.Range(0, 4).Count(i => !proven[i] && (i == 0 || proven[i - 1]));
            return new JournalCounts(launched, uncertain, 0);
        }

        /// <summary>
        /// B.8.5 step 2.b: the commits of <c>q0..tip</c> on the remote after a Q0 whose journal is lost. R-1: none. R-2: only commits whose every path is
        /// inside the intention's <c>AllowedWriteScope</c> (prefix only with a trailing <c>/</c>) and whose message carries a trailer of a catalog cell
        /// (<paramref name="trailerOfCell"/>). R-3: anything else (T10(c), STOP). A commit whose facts cannot be read is R-3.
        /// </summary>
        public static string ClassifyRemote(IGitHistory git, string q0, string tip, IReadOnlyList<string> allowedWriteScope, Func<string, bool> trailerOfCell)
        {
            var commits = git.Range(q0, tip);
            if (commits.Count == 0)
            {
                return "R-1";
            }

            bool Inside(string path) => allowedWriteScope.Any(e => e.EndsWith('/') ? path.StartsWith(e, StringComparison.Ordinal) : path == e);
            foreach (var c in commits)
            {
                var paths = git.ChangedPaths(c);
                var message = git.Message(c);
                if (paths.Count == 0 || !paths.All(Inside) || message == null || !message.Split('\n').Any(l => trailerOfCell(l.Trim())))
                {
                    return "R-3";
                }
            }

            return "R-2";
        }

        /// <summary>A reconstructed QR may fix higher counters, never lower (B.8.5): the invocation counter grows at least by the conservative minimum.</summary>
        public static List<string> ReconstructionProblems(YamlMap q0, YamlMap qr, string scope, string remoteCase)
        {
            var problems = new List<string>();
            var min = ConservativeMinimum(remoteCase);
            YamlMap? Inv(YamlMap s) => Y.L(s, "counters.invocations").Cast<YamlMap>().FirstOrDefault(x => Y.S(x, "scope") == scope);
            var before = Inv(q0);
            var after = Inv(qr);
            long Get(YamlMap? m, string k) => m == null ? 0 : Y.N(m, k) ?? 0;
            if (after == null || Y.Get(after, "reconstructed") is not true)
            {
                problems.Add("the QR does not mark its invocation counter as reconstructed");
                return problems;
            }

            if (Get(after, "launched") - Get(before, "launched") < min.Launched || Get(after, "uncertain") - Get(before, "uncertain") < min.Uncertain)
            {
                problems.Add("reconstructed counters below the conservative minimum (" + remoteCase + ": launched ≥ " + min.Launched + ", uncertain ≥ " + min.Uncertain + ")");
            }

            return problems;
        }
    }
}
