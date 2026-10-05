#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Xunit;
using static RackCad.Tests.CustodyMc;
using static RackCad.Tests.StateV2Samples;

namespace RackCad.Tests
{
    /// <summary>
    /// I-62 F4 (slice F4-G), C-15 and C-16 (reproducible controls, run as guards) on disposable repositories with real SHAs: a rebase of 16.7 inside a
    /// window reconciled by the closing Q7 (I-P12, A-1 D2-9) with the window starting at the image of its Q0; a takeover with an advance of
    /// <c>main</c> (T22) in a clean host, with one combined QR; the adoption of a DIRECT_ONLY unit (T20) and the return to <c>/v1</c> (T21); and the
    /// classification R-1/R-2/R-3 of the commits after a Q0 whose journal is lost (B.8.5).
    /// </summary>
    public class I62F4CustodyRebaseMcTests
    {
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

        private static (string MainBefore, string MainAfter, string Before, string After, Dictionary<string, string> Images) Rebase(CustodyRepo r, string repo, string mainRef)
        {
            var before = r.Head(repo);
            var mainBefore = r.Git(repo, "merge-base", mainRef, "HEAD");
            var mainAfter = r.Git(repo, "rev-parse", mainRef);
            r.Git(repo, "rebase", "-q", mainRef);
            var after = r.Head(repo);
            var draft = r.Map(repo, "R", mainBefore, mainAfter, before, after, Array.Empty<(string, string, string)>());
            var images = ((JsonArray)draft["Commits"]!).ToDictionary(c => J.S((JsonObject)c!, "OriginalSha")!, c => J.S((JsonObject)c!, "ImageSha")!, StringComparer.Ordinal);
            return (mainBefore, mainAfter, before, after, images);
        }

        private static IEnumerable<(string, string, string)> Fields(StatePoint p, Dictionary<string, string> images) =>
            StateV2Validator.StateFieldShas(p.State).Concat(StateV2Validator.LiveOrchestrationShas(p))
                .Select(f => (f.Field, f.Sha, images.TryGetValue(f.Sha, out var i) ? i : f.Sha));

        [Fact]
        public void I62_C15_ARebaseInsideAWindowIsReconciledByTheClosingQ7AndTheWindowStartsAtTheImageOfItsQ0()
        {
            var (r, q0, k3) = UpToQ0();
            using var _ = r;
            var h = r.Holder;
            var w1 = r.Commit(h, "T-01 RED", (RedFile, "rojo\n"));
            r.Git(h, "push", "-q", "origin", "HEAD");

            // main advances during the window; the session rebases under 16.7 (no QU inside the window) and the Worker continues on the images.
            r.Git(h, "checkout", "-q", "main");
            r.Commit(h, "main 1", ("docs/otra.md", "otra\n"));
            r.Git(h, "push", "-q", "origin", "main");
            r.Git(h, "checkout", "-q", "feature");
            var rb = Rebase(r, h, "main");
            var map = r.Map(h, "R20261003T020202Z-ef01", rb.MainBefore, rb.MainAfter, rb.Before, rb.After, Fields(q0, rb.Images));
            map["TaskId"] = "T-01";
            Assert.Empty(RebaseChain.PublicationProblems(new RebaseMapDoc(map), new GitProcessHistory(h)));
            r.Git(h, "push", "-q", "--force-with-lease=feature:" + rb.Before, "origin", "feature");
            var green = r.Commit(h, "T-01 GREEN", (GreenFile, "verde\n"));

            // The Q7 that closes the window reconciles it: last_rebase of its own record_version and the window's map in rebase_history.
            r.Subst[Sha3] = rb.Images[w1];
            r.Subst[Sha2] = green;
            var q7 = CustodyHistory()[3];
            var mapRef = ((InMemoryStateTree)q7.Tree).PutJson("docs/automation/evidence/I-99-agent/T-01/w1/rebase-map.json", map);
            var custody = (YamlMap)q7.State["custody"]!;
            custody["last_rebase"] = M(("map", mapRef), ("main_before", rb.MainBefore), ("main_after", rb.MainAfter), ("branch_before", rb.Before),
                ("branch_after", rb.After), ("record_version", 4L));
            custody["rebase_history"] = L(Clone(mapRef));
            var k4 = r.WritePoint(h, q7, "I-99: Q7 con rebase dentro de la ventana", push: false);
            var p4 = r.Read(h, k4);
            var git = new GitProcessHistory(h);
            var validator = new StateV2Validator();
            var all = new List<StateViolation>(validator.ValidateFile(p4));
            all.AddRange(validator.ValidatePair(q0, p4, new PairContext(WindowRebaseMaps: new[] { map })));
            all.AddRange(validator.ValidatePairHistory(q0, k3, p4, k4, git, CustodyRepo.StatePath));
            all.AddRange(validator.ValidateHistory(p4, git, k4));
            Clean(all, "Q7");

            // Without the window's map in the closing point, the reconciliation is missing (I-P12) and the Q0 is no longer an ancestor (I-P03).
            var unreconciled = With(p4, s => ((YamlMap)s["custody"]!)["last_rebase"] = null);
            Assert.Contains("I-P12", Ids(validator.ValidatePair(q0, unreconciled, new PairContext(WindowRebaseMaps: new[] { map }))));
            Assert.Contains("I-P03", Ids(validator.ValidatePairHistory(q0, k3, unreconciled, k4, git, CustodyRepo.StatePath)));
        }

        [Fact]
        public void I62_C15_ATakeoverWithAnAdvanceOfMainIsOneCombinedQrFromACleanHost()
        {
            var (r, pts, red, green) = Window1();
            using var _ = r;
            var h = r.Holder;

            // QH: the holder releases the unit.
            var (p5, _) = pts[4];
            var kh = r.WritePoint(h, With(p5, s =>
            {
                Rv(s, 6);
                ((YamlMap)s["custody"]!)["point"] = "QH";
                ((YamlMap)s["custody"]!)["point_kind"] = null;
                ((YamlMap)Y.M(s, "custody.principal")!)["state"] = "RELEASED";
            }), "I-99: QH");
            var ph = r.Read(h, kh);
            Clean(All(h, p5, pts[4].Commit, ph, kh, kh), "QH");

            // main advances; the designated successor works from a clean host and may only fetch, rebase, publish with lease, map and publish one QR.
            r.Git(h, "checkout", "-q", "main");
            r.Commit(h, "main 1", ("docs/otra.md", "otra\n"));
            r.Git(h, "push", "-q", "origin", "main");
            var n = r.CleanClone("designado");
            r.Git(n, "fetch", "-q", "origin", "main:refs/remotes/origin/main");
            var rb = Rebase(r, n, "origin/main");
            var phi = r.Read(n, rb.After);
            Assert.Contains("I-H02", Ids(new StateV2Validator().ValidateHistory(phi, new GitProcessHistory(n), rb.After)));
            var map = r.Map(n, "R20261005T020202Z-ef02", rb.MainBefore, rb.MainAfter, rb.Before, rb.After, Fields(phi, rb.Images));
            r.Git(n, "push", "-q", "--force-with-lease=feature:" + rb.Before, "origin", "feature");

            // The combined QR registers the new holder with its designation (I62-REBASE-TAKEOVER) and reconciles every SHA in the same point.
            const string binding = "B20261005T010101Z-ef56";
            var tree = new InMemoryStateTree();
            var decisions = tree.Put("docs/automation/decisions/I-99.md", Decisions(G0Entry, "## Toma con rebase\n\n```text\nI62-REBASE-TAKEOVER: " + binding
                + "\nI62-PRINCIPAL-BINDING: " + binding + " ACCEPTED\nClaim-Id: " + ClaimId + "\n```"));
            var bindingRef = tree.PutJson("docs/automation/evidence/I-99-agent/qr/binding.json", new JsonObject
            {
                ["Schema"] = "rackcad-binding/v1", ["BindingId"] = binding, ["UnitId"] = Unit, ["Scope"] = "UNIT", ["TaskId"] = null,
                ["Role"] = "PRINCIPAL_COORDINATOR", ["Acceptance"] = new JsonObject { ["State"] = "ACCEPTED", ["Basis"] = "INDIVIDUAL_DECISION" },
            });
            var preflight = tree.Put("docs/automation/evidence/I-99-agent/qr/preflight.json", "{\"Schema\": \"rackcad-preflight/v1\", \"Action\": \"CUSTODY\"}\n");
            var mapRef = tree.PutJson("docs/automation/evidence/I-99-agent/rebase/R20261005T020202Z-ef02/rebase-map.json", map);
            var s7 = Clone(phi.State);
            var c7 = (YamlMap)s7["custody"]!;
            c7["record_version"] = 7L;
            c7["point"] = "QR";
            c7["point_kind"] = "REBASE_RECONCILIATION";
            c7["last_rebase"] = M(("map", mapRef), ("main_before", rb.MainBefore), ("main_after", rb.MainAfter), ("branch_before", rb.Before),
                ("branch_after", rb.After), ("record_version", 7L));
            c7["rebase_history"] = L(Clone(mapRef));
            c7["principal"] = M(("state", "HELD"), ("binding", bindingRef), ("acceptance", M(("state", "ACCEPTED"), ("decision", Clone(decisions)))),
                ("preflight", preflight), ("designation", Clone(decisions)), ("since_record_version", 7L));
            ((YamlMap)Y.M(s7, "protocol.g0_acceptance")!)["decision"] = Clone(decisions);
            ((YamlMap)Y.L(s7, "custody.chains")[0]!)["chain_red_sha"] = rb.Images[red];
            ((YamlMap)c7["last_window"]!)["verified_sha"] = rb.Images[green];
            var k7 = r.WritePoint(n, new StatePoint(s7, tree), "I-99: QR REBASE_RECONCILIATION (toma con rebase)");
            var p7 = r.Read(n, k7);
            Clean(All(n, phi, rb.After, p7, k7, k7), "QR");

            // Without the designation markers the new holder's acceptance cites a decision that does not carry them (I-S16).
            var undesignated = r.WritePoint(n, new StatePoint(s7, CloneTreeWith(tree, "docs/automation/decisions/I-99.md", Decisions(G0Entry, "## Toma\n\nsin marcadores\n"))),
                "I-99: QR sin designación", push: false);
            Assert.Contains("I-S16", Ids(All(n, phi, rb.After, r.Read(n, undesignated), undesignated, undesignated)));
        }

        private static InMemoryStateTree CloneTreeWith(InMemoryStateTree tree, string path, string text)
        {
            var c = tree.Clone();
            c.Put(path, text);
            return c;
        }

        [Fact]
        public void I62_C15_TheAdoptionOfADirectOnlyUnitAndTheReturnToV1KeepTheNineFields()
        {
            using var r = new CustodyRepo();
            r.Subst[Sha] = r.Main0;
            r.Subst[DigitSha] = r.Main0;
            var h = r.Holder;
            var history = CustodyHistory();
            YamlMap V1(YamlMap v2) => M(("schema", "rackcad-automation-state/v1"), ("automation_state", Clone((YamlMap)v2["automation_state"]!)),
                ("execution_context", M(("mode", "manual"))));

            // T20: /v1 DIRECT_ONLY → BOOTSTRAP of adoption (MID_INITIATIVE) → QU with both acceptances → Q0.
            var adopted = history.Take(3).Select(p => With(p, s => ((YamlMap)Y.M(s, "protocol.basis")!)["adoption_at"] = "MID_INITIATIVE"))
                .Select(p => new StatePoint(p.State, ((InMemoryStateTree)history[0].Tree).Clone())).ToList();
            var v1 = V1(history[0].State);
            r.G.Write(h, CustodyRepo.StatePath, YamlSubset.Write(v1));
            r.G.CommitAll(h, "I-99: estado /v1 DIRECT_ONLY");
            Assert.Empty(Adoption.T20Problems(v1, adopted[0].State));
            Assert.NotEmpty(Adoption.T20Problems(v1, history[0].State));
            Assert.NotEmpty(Adoption.T20Problems(v1, Point(4).State));
            var trees = history.Take(3).ToList();
            var k = new List<string>();
            for (var i = 0; i < 3; i++)
            {
                var point = new StatePoint(Clone(adopted[i].State), trees[i].Tree);
                k.Add(r.WritePoint(h, point, "I-99: adopción rv" + (i + 1), push: false));
            }

            for (var i = 1; i < 3; i++)
            {
                Clean(All(h, r.Read(h, k[i - 1]), k[i - 1], r.Read(h, k[i]), k[i], k[i]), "adoption rv" + (i + 1));
            }

            // T21: a DIRECT_ONLY decision on the BOOTSTRAP returns to an ordinary /v1 with the nine fields; never from a QU or with fields changed.
            Assert.Empty(Adoption.T21Problems(adopted[0].State, V1(adopted[0].State)));
            Assert.NotEmpty(Adoption.T21Problems(adopted[1].State, V1(adopted[1].State)));
            var changed = V1(adopted[0].State);
            ((YamlMap)changed["automation_state"]!)["attempts"] = 3L;
            Assert.NotEmpty(Adoption.T21Problems(adopted[0].State, changed));
            Assert.NotEmpty(Adoption.T21Problems(adopted[0].State, adopted[0].State));
        }

        [Fact]
        public void I62_C16_TheCommitsAfterAQ0WithoutJournalAreClassifiedR1R2OrR3FromGitOnly()
        {
            var (r, _, k3) = UpToQ0();
            using var _r = r;
            var h = r.Holder;
            var git = new GitProcessHistory(h);
            var allowed = new[] { "src/", "tests/RackCad.Tests/EjemploTests.cs" };
            const string trailer = "Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com>";
            bool Cell(string line) => line == trailer;
            Assert.Equal("R-1", DelegationJournal.ClassifyRemote(git, k3, r.Head(h), allowed, Cell));

            r.G.Write(h, RedFile, "rojo\n");
            r.G.Run(h, "add", "-A");
            r.G.Run(h, "commit", "-q", "-m", "T-01 RED\n\n" + trailer);
            r.G.Write(h, GreenFile, "verde\n");
            r.G.Run(h, "add", "-A");
            r.G.Run(h, "commit", "-q", "-m", "T-01 GREEN\n\n" + trailer);
            var worker = r.Head(h);
            Assert.Equal("R-2", DelegationJournal.ClassifyRemote(git, k3, worker, allowed, Cell));

            // A commit outside the scope, or without the trailer of a catalog cell, is R-3 (T10(c), STOP).
            r.G.Write(h, "docs/ajeno.md", "ajeno\n");
            r.G.Run(h, "add", "-A");
            r.G.Run(h, "commit", "-q", "-m", "ajeno\n\n" + trailer);
            Assert.Equal("R-3", DelegationJournal.ClassifyRemote(git, k3, r.Head(h), allowed, Cell));
            r.G.Run(h, "reset", "-q", "--hard", worker);
            r.G.Write(h, GreenFile, "verde 2\n");
            r.G.Run(h, "add", "-A");
            r.G.Run(h, "commit", "-q", "-m", "sin trailer");
            Assert.Equal("R-3", DelegationJournal.ClassifyRemote(git, k3, r.Head(h), allowed, Cell));
            Assert.Throws<ArgumentException>(() => DelegationJournal.ConservativeMinimum("R-3"));
        }
    }
}
