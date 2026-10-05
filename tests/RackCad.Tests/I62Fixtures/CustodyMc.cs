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

        /// <summary>
        /// A new holder at <paramref name="rv"/> (QR by default; T12a's Q7 passes <paramref name="point"/>): the binding <paramref name="bindingId"/> HELD
        /// and ACCEPTED, with the Coordinator's designation appended to the decisions file. The designation names <paramref name="designated"/>
        /// (a session that is not the designated one carries another BindingId). Returns the state and the tree of the files to write (a synthetic
        /// point's own files plus the new ones; a point read from Git already has its files in the repository).
        /// </summary>
        public static (YamlMap State, InMemoryStateTree Tree) NewHolder(StatePoint from, long rv, string bindingId, string designated, string point = "QR")
        {
            var tree = from.Tree is InMemoryStateTree own ? own.Clone() : new InMemoryStateTree();
            var decisions = tree.Put("docs/automation/decisions/I-99.md", Decisions(G0Entry, "## Designación\n\n```text\nI62-PRINCIPAL-BINDING: " + designated
                + " ACCEPTED\nClaim-Id: " + ClaimId + "\n```"));
            var binding = tree.PutJson("docs/automation/evidence/I-99-agent/" + bindingId + "/binding.json", new JsonObject
            {
                ["Schema"] = "rackcad-binding/v1", ["BindingId"] = bindingId, ["UnitId"] = Unit, ["Scope"] = "UNIT", ["TaskId"] = null,
                ["Role"] = "PRINCIPAL_COORDINATOR", ["Acceptance"] = new JsonObject { ["State"] = "ACCEPTED", ["Basis"] = "INDIVIDUAL_DECISION" },
            });
            var preflight = tree.Put("docs/automation/evidence/I-99-agent/" + bindingId + "/preflight.json", "{\"Schema\": \"rackcad-preflight/v1\", \"Action\": \"CUSTODY\"}\n");
            var s = Clone(from.State);
            var custody = (YamlMap)s["custody"]!;
            custody["record_version"] = rv;
            custody["point"] = point;
            custody["point_kind"] = point == "QR" ? "ORDINARY" : null;
            custody["principal"] = M(("state", "HELD"), ("binding", binding), ("acceptance", M(("state", "ACCEPTED"), ("decision", Clone(decisions)))),
                ("preflight", preflight), ("designation", Clone(decisions)), ("since_record_version", rv));
            ((YamlMap)Y.M(s, "protocol.g0_acceptance")!)["decision"] = Clone(decisions);
            return (s, tree);
        }
    }
}
