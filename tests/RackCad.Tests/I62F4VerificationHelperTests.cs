#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Nodes;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-62 F4 (slice F4-G): the deterministic helpers of the F4 opening order (decisions §43) inside the Controller's verification boundary —
    /// P1 identity from the structured delivery (16.9 #5), P2 scope against the concrete delegation (16.9 #7), P3 facts without verdicts, P5
    /// observation invalidation, cession fingerprint STOP and non-secret fingerprints (16.18, 16.19, README §13), P6 the normalized TRX reading (16.9
    /// #10). P1 and P2 run on real disposable Git repositories; P5 uses the real F2 preflight and synthetic values only; P6 uses synthetic TRX.
    /// </summary>
    public class I62F4VerificationHelperTests
    {
        private sealed class Repo : IDisposable
        {
            public readonly GitScratch G = new GitScratch();
            public readonly string Dir;
            public readonly GitProcessHistory Git;

            public Repo()
            {
                Dir = System.IO.Path.Combine(G.Root, "r");
                System.IO.Directory.CreateDirectory(Dir);
                G.Run(Dir, "init", "-q", "-b", "main");
                Git = new GitProcessHistory(Dir);
            }

            public string Commit(string message, params (string Path, string? Text)[] files)
            {
                foreach (var (path, text) in files)
                {
                    if (text == null)
                    {
                        G.Run(Dir, "rm", "-q", path);
                    }
                    else
                    {
                        G.Write(Dir, path, text);
                    }
                }

                return G.CommitAll(Dir, message);
            }

            public void Dispose() => G.Dispose();
        }

        private static JsonObject Obj(params (string K, JsonNode? V)[] fields)
        {
            var o = new JsonObject();
            foreach (var (k, v) in fields)
            {
                o[k] = v;
            }

            return o;
        }

        private static JsonArray Arr(params string[] items) => new JsonArray(items.Select(i => (JsonNode)i).ToArray());

        // ------------------------------------------------------------------ P1

        [Fact]
        public void I62_P1_IdentityComesFromTheStructuredCurrentShaAndNarrativeNeverRescuesIt()
        {
            using var r = new Repo();
            var b = r.Commit("base", ("src/a.cs", "a\n"));
            var red = r.Commit("red", ("tests/a.cs", "red\n"));
            var green = r.Commit("green", ("src/a.cs", "a2\n"));
            var delegation = Obj(("BaseSha", b));
            var ok = VerificationFacts.Identity(Obj(("CurrentSha", green), ("RedSha", red)), delegation, r.Git);
            Assert.Equal("PASS", ok.Status);
            Assert.True(ok.BaseShaIsAncestor);
            Assert.True(ok.RedShaBetween);

            // The negative of the order: an invalid structured CurrentSha with the real GREEN SHA in prose is an Identity FAIL.
            var narrative = Obj(("CurrentSha", "not-a-sha"), ("RedSha", red), ("Summary", "GREEN en " + green + "; todo verde"), ("Notes", Arr(green)));
            var failed = VerificationFacts.Identity(narrative, delegation, r.Git);
            Assert.Equal("FAIL", failed.Status);
            Assert.Equal("not-a-sha", failed.CurrentSha);
            Assert.Null(typeof(VerificationFacts).GetMethods().SelectMany(m => m.GetParameters()).FirstOrDefault(p => p.Name!.Contains("prompt", StringComparison.OrdinalIgnoreCase)
                                                                                                                  || p.Name!.Contains("narrative", StringComparison.OrdinalIgnoreCase)));

            // No normalization, no substitution: upper case, an absent commit, a non-commit object, a base off the chain, a RED outside it.
            Assert.Equal("FAIL", VerificationFacts.Identity(Obj(("CurrentSha", green.ToUpperInvariant())), delegation, r.Git).Status);
            Assert.Equal("FAIL", VerificationFacts.Identity(Obj(("CurrentSha", new string('a', 40))), delegation, r.Git).Status);
            var blob = r.G.Run(r.Dir, "rev-parse", green + ":src/a.cs");
            var notCommit = VerificationFacts.Identity(Obj(("CurrentSha", blob)), delegation, r.Git);
            Assert.Equal("blob", notCommit.ObjectType);
            Assert.Equal("FAIL", notCommit.Status);
            r.G.Run(r.Dir, "checkout", "-q", "--orphan", "side");
            var other = r.Commit("other root", ("x.txt", "x\n"));
            Assert.Contains("BaseSha is not an ancestor of CurrentSha", VerificationFacts.Identity(Obj(("CurrentSha", green)), Obj(("BaseSha", other)), r.Git).Failures);
            Assert.Contains("RedSha is not between BaseSha and CurrentSha", VerificationFacts.Identity(Obj(("CurrentSha", green), ("RedSha", other)), delegation, r.Git).Failures);
        }

        // ------------------------------------------------------------------ P2

        [Fact]
        public void I62_P2_TheDiffOfTheExactRangeAgainstTheDelegationScopesAndForbiddenWins()
        {
            using var r = new Repo();
            var b = r.Commit("base", ("src/a.cs", "a\n"), ("docs/old.md", "x\n"), ("src/move.cs", "m\n"));
            r.G.Run(r.Dir, "mv", "src/move.cs", "src/moved.cs");
            var c = r.Commit("work", ("src/a.cs", "a2\n"), ("tests/t.cs", "t\n"), ("docs/old.md", null));
            JsonObject Delegation(string[] allowed, params string[] forbidden) => Obj(("BaseSha", b), ("AllowedWriteScope", Arr(allowed)), ("ForbiddenWriteScope", Arr(forbidden)));

            var pass = VerificationFacts.Scope(b, c, Delegation(new[] { "src/", "tests/", "docs/old.md" }), r.Git);
            Assert.Equal("PASS", pass.Status);
            Assert.Equal(new[] { "docs/old.md", "src/a.cs", "src/move.cs", "src/moved.cs", "tests/t.cs" }, pass.Paths.Select(p => p.Path).OrderBy(p => p, StringComparer.Ordinal));
            Assert.Contains(pass.Paths, p => p.Path == "docs/old.md" && p.GitStatus == "D");
            Assert.Contains(pass.Paths, p => p.Path == "src/move.cs" && p.GitStatus == "D");

            // A deletion is a write; Forbidden wins over Allowed; a prefix needs the trailing slash.
            Assert.Contains("outside AllowedWriteScope: docs/old.md (D)", VerificationFacts.Scope(b, c, Delegation(new[] { "src/", "tests/" }), r.Git).Failures);
            var forbidden = VerificationFacts.Scope(b, c, Delegation(new[] { "src/", "tests/", "docs/old.md" }, "tests/"), r.Git);
            Assert.Equal("FAIL", forbidden.Status);
            Assert.Contains("in ForbiddenWriteScope: tests/t.cs", forbidden.Failures);
            Assert.Equal("FAIL", VerificationFacts.Scope(b, c, Delegation(new[] { "src", "tests/", "docs/old.md" }), r.Git).Status);

            // A match only ignoring case is a reported gap, never a pass.
            var cased = VerificationFacts.Scope(b, c, Delegation(new[] { "SRC/", "tests/", "docs/old.md" }), r.Git);
            Assert.Equal("FAIL", cased.Status);
            Assert.Contains(cased.Gaps, g => g.StartsWith("match only ignoring case", StringComparison.Ordinal));
        }

        [Fact]
        public void I62_P2_AnAbsentScopeAnUnrunComparisonOrAGlobIsNeverAPass()
        {
            using var r = new Repo();
            var b = r.Commit("base", ("src/a.cs", "a\n"));
            var c = r.Commit("work", ("src/a.cs", "a2\n"));
            Assert.Equal("NOT_EVALUATED", VerificationFacts.Scope(b, c, Obj(("BaseSha", b)), r.Git).Status);
            Assert.Equal("NOT_EVALUATED", VerificationFacts.Scope(b, c, Obj(("AllowedWriteScope", Arr())), r.Git).Status);
            Assert.Equal("NOT_EVALUATED", VerificationFacts.Scope(b, c, Obj(("AllowedWriteScope", Arr("src/*.cs"))), r.Git).Status);
            var unrun = VerificationFacts.Scope(new string('b', 40), c, Obj(("AllowedWriteScope", Arr("src/"))), r.Git);
            Assert.Equal("NOT_EVALUATED", unrun.Status);
            Assert.Contains("the comparison was not run", unrun.Failures);
            Assert.Equal("NOT_EVALUATED", VerificationFacts.Scope(null, c, Obj(("AllowedWriteScope", Arr("src/"))), r.Git).Status);
            Assert.Equal("NOT_EVALUATED", VerificationFacts.Scope(b, c, Obj(("AllowedWriteScope", Arr("src/"))), new SyntheticGitHistory()).Status);

            // A mode change is a write too.
            r.G.Run(r.Dir, "update-index", "--chmod=+x", "src/a.cs");
            r.G.Run(r.Dir, "commit", "-q", "-m", "mode");
            var mode = r.G.Run(r.Dir, "rev-parse", "HEAD");
            var row = VerificationFacts.Scope(c, mode, Obj(("AllowedWriteScope", Arr("src/"))), r.Git).Paths.Single();
            Assert.Equal(("src/a.cs", "M"), (row.Path, row.GitStatus));
            Assert.Equal("FAIL", VerificationFacts.Scope(c, mode, Obj(("AllowedWriteScope", Arr("tests/"))), r.Git).Status);
        }

        // ------------------------------------------------------------------ P3

        [Fact]
        public void I62_P3_TheHelpersProduceFactsAndNeverAVerdict()
        {
            using var r = new Repo();
            var b = r.Commit("base", ("src/a.cs", "a\n"));
            var c = r.Commit("work", ("src/a.cs", "a2\n"));
            var delegation = Obj(("BaseSha", b), ("AllowedWriteScope", Arr("src/")));
            var facts = VerificationFacts.Facts(Obj(("CurrentSha", c)), delegation, r.Git);
            Assert.Equal("FACTS", (string?)facts["Kind"]);
            Assert.Equal("PASS", (string?)facts["Identity"]!["Status"]);
            Assert.Equal("PASS", (string?)facts["Scope"]!["Status"]);
            Assert.Empty(VerificationFacts.VerdictProblems(facts));

            // Without an identity, the scope is not evaluated.
            Assert.Equal("NOT_EVALUATED", (string?)VerificationFacts.Facts(Obj(("CurrentSha", "x")), delegation, r.Git)["Scope"]!["Status"]);

            // Verdict fields and values anywhere are rejected, whatever their case.
            Assert.NotEmpty(VerificationFacts.VerdictProblems(Obj(("Classification", "VERIFIED"))));
            Assert.NotEmpty(VerificationFacts.VerdictProblems(Obj(("Nested", Obj(("Items", Arr("ok", "execution_verified")))))));
            Assert.NotEmpty(VerificationFacts.VerdictProblems(Obj(("Note", " gate pass "))));
            Assert.NotEmpty(VerificationFacts.VerdictProblems(Obj(("Disposition", null))));
        }

        // ------------------------------------------------------------------ P5

        private static JsonObject RealPreflight() =>
            (JsonObject)JsonNode.Parse(I62Repo.ReadText("docs/automation/evidence/I-62-F2/preflights/P20261002T224500Z-f201.json"))!;

        [Fact]
        public void I62_P5_AnObservationIsInvalidatedByEachFrozenInvalidatorAndOnlyByThem()
        {
            var accepted = RealPreflight();
            Assert.Equal("OBSERVED", J.S(accepted, "Host.HostInstanceState"));
            Assert.Equal("STILL_VALID", ConfigurationFacts.Compare(accepted, (JsonObject)accepted.DeepClone()).State);
            foreach (var key in ConfigurationFacts.Invalidators)
            {
                var observed = (JsonObject)accepted.DeepClone();
                var current = J.S(accepted, "Invalidators." + key);
                observed["Invalidators"]![key] = key == "AuthState" ? (current == "UNKNOWN" ? "AUTHENTICATED" : "UNKNOWN")
                    : key == "AdapterVersion" ? current + ".1" : current == new string('e', 64) ? new string('d', 64) : new string('e', 64);
                var cmp = ConfigurationFacts.Compare(accepted, observed);
                Assert.Equal("INVALIDATED", cmp.State);
                Assert.Equal(new[] { key }, cmp.Changed);
            }

            // README §13.3: a binding or the branch SHA never invalidates; an unobserved host instance is never inherited.
            var rebound = (JsonObject)accepted.DeepClone();
            rebound["PreflightId"] = "P20261005T000000Z-ffff";
            rebound["Requirements"] = new JsonArray();
            Assert.Equal("STILL_VALID", ConfigurationFacts.Compare(accepted, rebound).State);
            var unobserved = (JsonObject)accepted.DeepClone();
            unobserved["Host"]!["HostInstanceState"] = "UNOBSERVED";
            Assert.Equal("INVALIDATED", ConfigurationFacts.Compare(accepted, unobserved).State);

            // Never a silent new baseline: no operation of the helper accepts, adopts or resets.
            Assert.DoesNotContain(typeof(ConfigurationFacts).GetMethods(BindingFlags.Public | BindingFlags.Static),
                m => new[] { "Accept", "Adopt", "Baseline", "Reset", "Reconfigure" }.Any(w => m.Name.Contains(w, StringComparison.OrdinalIgnoreCase)));
        }

        [Fact]
        public void I62_P5_AFingerprintChangeDuringACessionIsAStop()
        {
            var a = new string('a', 64);
            Assert.Equal("MATCH", ConfigurationFacts.CessionGate(a, a));
            Assert.Equal("MATCH", ConfigurationFacts.CessionGate("NONE", "NONE"));
            Assert.Equal("STOP", ConfigurationFacts.CessionGate(a, new string('b', 64)));
            Assert.Equal("STOP", ConfigurationFacts.CessionGate(a, null));
            Assert.Equal("STOP", ConfigurationFacts.CessionGate("UNKNOWN", "UNKNOWN"));
        }

        [Fact]
        public void I62_P5_AFingerprintCarriesOnlyAHashAndSanitizedNamesNeverValuesOrSecrets()
        {
            Assert.Empty(ConfigurationFacts.FingerprintProblems((JsonObject)RealPreflight()["Fingerprint"]!));
            JsonObject Fp(string kind, string? sha, string[] names, JsonNode? decision = null) =>
                Obj(("Kind", kind), ("Sha256", sha), ("KeyNames", Arr(names)), ("AcceptanceDecisionRef", decision));
            var sha = new string('c', 64);
            Assert.Empty(ConfigurationFacts.FingerprintProblems(Fp("CONFIG_FILE", sha, new[] { "[features]", "[projects.<redactado>]", "model" })));
            Assert.Contains(ConfigurationFacts.FingerprintProblems(Fp("CONFIG_FILE", sha, new[] { "model=gpt" })), p => p.StartsWith("a key name carries a value", StringComparison.Ordinal));
            Assert.Contains(ConfigurationFacts.FingerprintProblems(Fp("CONFIG_FILE", sha, new[] { "token: " + new string('z', 20) })), p => p.Contains("secret pattern (KV)", StringComparison.Ordinal));
            Assert.Contains(ConfigurationFacts.FingerprintProblems(Fp("CONFIG_FILE", sha, new[] { "[projects.D:/repo]" })), p => p.Contains("unsanitized", StringComparison.Ordinal));
            Assert.NotEmpty(ConfigurationFacts.FingerprintProblems(Fp("CONFIG_FILE", null, new[] { "[features]" })));
            Assert.NotEmpty(ConfigurationFacts.FingerprintProblems(Fp("NONE", null, new string[0])));
            Assert.Empty(ConfigurationFacts.FingerprintProblems(Fp("NONE", null, new string[0], Obj(("Path", "docs/automation/decisions/I-62.md")))));
            Assert.NotEmpty(ConfigurationFacts.FingerprintProblems(Fp("UNVERIFIED", null, new[] { "[features]" })));
            Assert.Equal(new[] { "SK" }, ConfigurationFacts.SecretHits("sk-" + new string('Q', 20)));
        }

        // ------------------------------------------------------------------ P6

        private static byte[] Trx(IEnumerable<(string Name, string Outcome, string? Reason)> results, string counters)
        {
            var sb = new StringBuilder("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<TestRun xmlns=\"http://microsoft.com/schemas/VisualStudio/TeamTest/2010\">\n<Results>\n");
            foreach (var (name, outcome, reason) in results)
            {
                sb.Append("<UnitTestResult testName=\"" + name + "\" outcome=\"" + outcome + "\">");
                if (reason != null)
                {
                    sb.Append("<Output><ErrorInfo><Message>" + reason + "</Message></ErrorInfo></Output>");
                }

                sb.Append("</UnitTestResult>\n");
            }

            sb.Append("</Results>\n<ResultSummary outcome=\"Completed\"><Counters " + counters + " /></ResultSummary>\n</TestRun>\n");
            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        private static readonly (string, string, string?)[] Green =
            { ("N.A.One", "Passed", null), ("N.A.Two", "Passed", null), ("N.A.Three", "Passed", null), ("N.A.Later", "NotExecuted", "requiere AutoCAD") };

        [Fact]
        public void I62_P6_PassedFailedSkippedAndUnexpectedStatesAreDistinguished()
        {
            var ok = TrxReading.Read(Trx(Green, "total=\"4\" executed=\"3\" passed=\"3\" failed=\"0\" notExecuted=\"0\""));
            Assert.Equal("PASS", ok.Status);
            Assert.Equal((3, 0, 1), (ok.Passed, ok.Failed, ok.Skipped.Count));
            Assert.Equal("requiere AutoCAD", ok.Skipped.Single().Reason);
            Assert.Contains(ok.Notes, n => n.Contains("notExecuted = 0", StringComparison.Ordinal));

            var failed = TrxReading.Read(Trx(Green.Append(("N.A.Bad", "Failed", null)), "total=\"5\" passed=\"3\" failed=\"1\""));
            Assert.Contains(failed.Failures, f => f.StartsWith("failed tests", StringComparison.Ordinal));
            var unexplained = TrxReading.Read(Trx(Green.Append(("N.A.Silent", "NotExecuted", null)), "total=\"5\" passed=\"3\""));
            Assert.Contains(unexplained.Failures, f => f.StartsWith("NotExecuted without", StringComparison.Ordinal));
            foreach (var state in new[] { "Timeout", "Aborted", "Inconclusive", "Error" })
            {
                var odd = TrxReading.Read(Trx(Green.Append(("N.A.Odd", state, null)), "total=\"5\" passed=\"3\""));
                Assert.Equal("FAIL", odd.Status);
                Assert.Equal(new[] { "N.A.Odd: " + state }, odd.UnexpectedOutcomes);
                Assert.Equal(3, odd.Passed);
            }
        }

        [Fact]
        public void I62_P6_ContradictionsAndAnEmptySelectionFailAndSkipsAreNeverPassed()
        {
            Assert.Contains(TrxReading.Read(Trx(Green, "total=\"4\" passed=\"4\"")).Failures, f => f.StartsWith("contradiction: Counters.passed", StringComparison.Ordinal));
            Assert.Contains(TrxReading.Read(Trx(Green, "total=\"7\" passed=\"3\"")).Failures, f => f.StartsWith("contradiction: Counters.total", StringComparison.Ordinal));
            Assert.Contains(TrxReading.Read(Trx(new (string, string, string?)[0], "total=\"0\"")).Failures, f => f == "empty selection");
            var allSkipped = TrxReading.Read(Trx(new (string, string, string?)[] { ("N.A.X", "NotExecuted", "omitida") }, "total=\"1\" passed=\"0\""));
            Assert.Equal(0, allSkipped.Passed);
        }

        [Fact]
        public void I62_P6_SkipsAreCrossCheckedWithTheSourceDeclarationsWithoutAFixedCount()
        {
            const string source = "public class A {\n  [Fact(Skip = \"requiere AutoCAD\")]\n  public void Later() { }\n  [Fact]\n  public void One() { }\n"
                                  + "  [Theory(Skip = \"x\")]\n  [InlineData(1)]\n  public async Task Again(int i) { }\n}\n";
            var declared = TrxReading.DeclaredSkipMethods(new[] { source });
            Assert.Equal(new[] { "Again", "Later" }, declared.OrderBy(x => x, StringComparer.Ordinal));
            var known = TrxReading.Read(Trx(Green, "total=\"4\" passed=\"3\""), declared);
            Assert.Equal("PASS", known.Status);
            Assert.Contains(known.Notes, n => n.Contains("declared with Skip but not skipped in this TRX: Again", StringComparison.Ordinal));
            var undeclared = TrxReading.Read(Trx(Green, "total=\"4\" passed=\"3\""), new HashSet<string> { "Again" });
            Assert.Contains(undeclared.Failures, f => f.Contains("skips the source does not declare: Later", StringComparison.Ordinal));

            // The real UI suite declares its skips in source; the reader derives them, nothing is fixed.
            var ui = System.IO.Directory.GetFiles(I62Repo.FullPath("tests/RackCad.UI.Tests"), "*.cs", System.IO.SearchOption.AllDirectories)
                .Where(f => !f.Contains(System.IO.Path.DirectorySeparatorChar + "obj" + System.IO.Path.DirectorySeparatorChar, StringComparison.Ordinal))
                .Select(System.IO.File.ReadAllText);
            Assert.NotEmpty(TrxReading.DeclaredSkipMethods(ui));
        }
    }
}
