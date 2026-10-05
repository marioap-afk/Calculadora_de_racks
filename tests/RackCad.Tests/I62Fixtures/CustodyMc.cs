#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Xunit;
using static RackCad.Tests.StateV2Samples;

namespace RackCad.Tests
{
    /// <summary>
    /// The shared steps of the C-15 reproducible controls: the custody of task T-01 written as real points on a disposable repository, and every
    /// invariant (file, pair and history) evaluated on the state and the tree of each point's own commit. Test data and helpers only.
    /// </summary>
    public static class CustodyMc
    {
        public const string RedFile = "tests/RackCad.Tests/EjemploTests.cs";
        public const string GreenFile = "src/Ejemplo.cs";

        // One memoizing Git view per repository (facts about full ids of existing objects never change).
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, GitProcessHistory> Gits =
            new System.Collections.Concurrent.ConcurrentDictionary<string, GitProcessHistory>(StringComparer.Ordinal);

        public static List<string> Ids(IEnumerable<StateViolation> v) => v.Select(x => x.Invariant + (x.Clause.Length > 0 ? "/" + x.Clause : string.Empty)).Distinct().ToList();

        /// <summary>Every invariant that applies to <paramref name="n"/> (and to the pair, when <paramref name="p"/> is given) at the commit <paramref name="head"/>.</summary>
        public static List<StateViolation> All(string repo, StatePoint? p, string? pCommit, StatePoint n, string nCommit, string head, StateV2Validator? validator = null)
        {
            var v = validator ?? new StateV2Validator();
            var git = Gits.GetOrAdd(repo, r => new GitProcessHistory(r));
            var all = new List<StateViolation>(v.ValidateFile(n));
            if (p != null)
            {
                all.AddRange(v.ValidatePair(p, n));
                all.AddRange(v.ValidatePairHistory(p, pCommit!, n, nCommit, git, CustodyRepo.StatePath));
            }

            all.AddRange(v.ValidateHistory(n, git, head));
            return all;
        }

        public static void Clean(List<StateViolation> violations, string at) => Assert.True(violations.Count == 0, at + ": " + string.Join("; ", violations.Take(5)));

        /// <summary>
        /// The window of task T-01 written as real points: BOOTSTRAP → QU (G0 acceptances) → Q0 → the Worker's RED and GREEN → Q7 VERIFIED → QU, each
        /// validated, with DS at each step of the window from its journal. Returns the repository and the points.
        /// </summary>
        public static (CustodyRepo Repo, List<(StatePoint Point, string Commit)> Points, string Red, string Green) Window1()
        {
            var r = new CustodyRepo();
            r.Subst[Sha] = r.Main0;
            r.Subst[DigitSha] = r.Main0;
            var h = r.Holder;
            var history = CustodyHistory();
            var points = new List<(StatePoint, string)>();
            for (var i = 0; i < 3; i++)
            {
                var k = r.WritePoint(h, history[i], "I-99: punto rv" + (i + 1), push: false);
                points.Add((r.Read(h, k), k));
            }

            var red = r.Commit(h, "T-01 RED", (RedFile, "rojo\n"));
            var green = r.Commit(h, "T-01 GREEN", (GreenFile, "verde\n"));
            r.Subst[Sha3] = red;
            r.Subst[Sha2] = green;
            for (var i = 3; i < 5; i++)
            {
                var k = r.WritePoint(h, history[i], "I-99: punto rv" + (i + 1), push: false);
                points.Add((r.Read(h, k), k));
            }

            r.Git(h, "push", "-q", "origin", "HEAD");
            return (r, points, red, green);
        }

        public static StatePoint With(StatePoint p, Action<YamlMap> change)
        {
            var s = Clone(p.State);
            change(s);
            return new StatePoint(s, p.Tree);
        }

        public static void Rv(YamlMap s, long rv) => ((YamlMap)s["custody"]!)["record_version"] = rv;
    }
}
