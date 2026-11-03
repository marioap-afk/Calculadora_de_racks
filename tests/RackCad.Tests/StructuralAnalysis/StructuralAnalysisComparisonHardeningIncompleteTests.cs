using System;
using System.Linq;
using System.Text.Json;
using RackCad.Application.StructuralAnalysis;
using Xunit;
using M = RackCad.Tests.StructuralAnalysisComparisonModel;
using R = RackCad.Application.StructuralAnalysis.StructuralComparisonRowIds;
using S = RackCad.Tests.StructuralAnalysisComparisonSupport;
using V = RackCad.Application.StructuralAnalysis.StructuralComparisonVerdict;

namespace RackCad.Tests
{
    /// <summary>
    /// I-65 F4-T6 — A65-F4-02 and the shape of the report that A65-F4-03 extends. A comparison whose combinations nobody could read is never a
    /// PASS: it is INCOMPLETE, unless a row fails (then it is a FAIL). The decision of the engineer and the inputs travel in the report
    /// only when the caller gives them, and a report without them is byte for byte the one of before.
    /// </summary>
    public class StructuralAnalysisComparisonHardeningIncompleteTests
    {
        private const string Sha = "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef";

        private static StructuralComparisonInputs Inputs() =>
            new StructuralComparisonInputs(M.Fingerprint, M.Fingerprint, Sha, Sha.Replace('0', 'a'), Sha.Replace('0', 'b'), Sha.Replace('0', 'c'));

        [Fact]
        public void The_new_outcome_is_added_at_the_end_and_the_existing_ones_keep_their_numbers()
        {
            Assert.Equal(0, (int)StructuralComparisonOutcome.Pass);
            Assert.Equal(1, (int)StructuralComparisonOutcome.Fail);
            Assert.Equal(2, (int)StructuralComparisonOutcome.Stale);
            Assert.Equal(3, (int)StructuralComparisonOutcome.Rejected);
            Assert.Equal(4, (int)StructuralComparisonOutcome.Incomplete);
        }

        [Theory]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(false, false)]
        public void Combinations_nobody_could_read_never_give_a_pass_and_no_failing_row_gives_incomplete(bool generatedRead, bool manualRead)
        {
            var report = S.Compare(options: new StructuralComparisonOptions(generatedRead, manualRead));

            Assert.Equal(StructuralComparisonOutcome.Incomplete, report.Outcome);
            Assert.Equal(V.NotEvaluated, S.VerdictOf(report, R.Combinations));
            Assert.Null(report.Reason);
        }

        [Fact]
        public void Everything_read_is_still_a_pass()
        {
            Assert.Equal(StructuralComparisonOutcome.Pass, S.Compare(options: new StructuralComparisonOptions(true, true)).Outcome);
            Assert.Equal(StructuralComparisonOutcome.Pass, S.Compare().Outcome);
        }

        [Fact]
        public void A_failing_row_keeps_the_fail_even_when_the_combinations_were_not_read()
        {
            var manual = M.Reference("M", reverse: true);
            manual.Member("MA1").Section = "HSS 5X5X1/4";

            var report = S.Compare(manual: manual, options: new StructuralComparisonOptions(manualCombinationsRead: false));

            Assert.Equal(StructuralComparisonOutcome.Fail, report.Outcome);
            Assert.Equal(V.NotEvaluated, S.VerdictOf(report, R.Combinations));
        }

        [Fact]
        public void An_incomplete_report_lists_every_row_with_its_verdict_and_the_combinations_stay_not_evaluated()
        {
            var report = S.Compare(options: new StructuralComparisonOptions(generatedCombinationsRead: false));

            Assert.Equal(14, report.Rows.Count);
            Assert.Equal(V.Pass, S.VerdictOf(report, R.NodeCoordinates));
            Assert.Equal(V.Pass, S.VerdictOf(report, R.Loads));
            Assert.Equal(V.NotEvaluated, S.VerdictOf(report, R.Combinations));
            Assert.Equal(V.NotEvaluated, S.VerdictOf(report, R.SectionProperties));
        }

        [Fact]
        public void An_incomplete_report_serializes_the_verdict_incomplete()
        {
            var report = S.Compare(options: new StructuralComparisonOptions(manualCombinationsRead: false));

            using (var document = JsonDocument.Parse(report.ToJson()))
            {
                Assert.Equal("INCOMPLETE", document.RootElement.GetProperty("verdict").GetString());
                Assert.Equal(14, document.RootElement.GetProperty("rows").GetArrayLength());
                Assert.Equal("NOT_EVALUATED", document.RootElement.GetProperty("rows").EnumerateArray().Single(r => r.GetProperty("id").GetString() == R.Combinations).GetProperty("verdict").GetString());
            }
        }

        // ---------------------------------------------------------------- the decision of the engineer

        [Fact]
        public void The_decision_is_written_in_the_report_after_the_reason()
        {
            var report = S.Compare(options: new StructuralComparisonOptions(manualCombinationsRead: false)).WithCombinationsDecision("The engineer checked the combinations by eye.");

            using (var document = JsonDocument.Parse(report.ToJson()))
            {
                Assert.Equal(
                    new[] { "schema", "verdict", "reason", "combinationsDecision", "tolerances", "rows" },
                    document.RootElement.EnumerateObject().Select(p => p.Name).ToArray());
                Assert.Equal("The engineer checked the combinations by eye.", document.RootElement.GetProperty("combinationsDecision").GetString());
                Assert.Equal("INCOMPLETE", document.RootElement.GetProperty("verdict").GetString());
            }
        }

        [Fact]
        public void A_report_without_a_decision_has_no_such_field()
        {
            using (var document = JsonDocument.Parse(S.Compare().ToJson()))
            {
                Assert.False(document.RootElement.TryGetProperty("combinationsDecision", out _));
            }
        }

        [Fact]
        public void The_decision_does_not_change_the_outcome_or_the_rows()
        {
            var report = S.Compare(options: new StructuralComparisonOptions(manualCombinationsRead: false));
            var decided = report.WithCombinationsDecision("decision");

            Assert.Equal(report.Outcome, decided.Outcome);
            Assert.Equal(report.Rows, decided.Rows);
            Assert.Equal("decision", decided.CombinationsDecision);
            Assert.Null(report.CombinationsDecision);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void A_decision_needs_a_text(string text)
        {
            Assert.ThrowsAny<ArgumentException>(() => S.Compare().WithCombinationsDecision(text));
        }

        // ---------------------------------------------------------------- the inputs

        [Fact]
        public void The_inputs_are_written_with_the_fingerprints_and_the_four_hashes_in_this_order()
        {
            var report = S.Compare().WithInputs(Inputs());

            using (var document = JsonDocument.Parse(report.ToJson()))
            {
                Assert.Equal(
                    new[] { "schema", "verdict", "reason", "inputs", "tolerances", "rows" },
                    document.RootElement.EnumerateObject().Select(p => p.Name).ToArray());

                var inputs = document.RootElement.GetProperty("inputs");

                Assert.Equal(
                    new[] { "manifestFingerprint", "readbackFingerprint", "manifestSha256", "readbackSha256", "generatedDumpSha256", "manualDumpSha256" },
                    inputs.EnumerateObject().Select(p => p.Name).ToArray());
                Assert.Equal(M.Fingerprint, inputs.GetProperty("manifestFingerprint").GetString());
                Assert.Equal(M.Fingerprint, inputs.GetProperty("readbackFingerprint").GetString());
                Assert.Equal(Sha, inputs.GetProperty("manifestSha256").GetString());
                Assert.Equal(Sha.Replace('0', 'a'), inputs.GetProperty("readbackSha256").GetString());
                Assert.Equal(Sha.Replace('0', 'b'), inputs.GetProperty("generatedDumpSha256").GetString());
                Assert.Equal(Sha.Replace('0', 'c'), inputs.GetProperty("manualDumpSha256").GetString());
            }
        }

        [Fact]
        public void A_report_without_inputs_is_byte_for_byte_the_one_of_before()
        {
            var report = S.Compare();
            var withInputs = report.WithInputs(Inputs()).ToJson();

            var start = withInputs.IndexOf("  \"inputs\": {\n", StringComparison.Ordinal);
            var end = withInputs.IndexOf("  },\n", start, StringComparison.Ordinal) + "  },\n".Length;

            Assert.True(start > 0 && end > start, "El informe con entradas debe tener el bloque.");
            Assert.Equal(report.ToJson(), withInputs.Remove(start, end - start));
            Assert.DoesNotContain("inputs", report.ToJson(), StringComparison.Ordinal);
        }

        [Fact]
        public void An_unreadable_fingerprint_is_written_as_null()
        {
            var inputs = new StructuralComparisonInputs(null, null, Sha, Sha, Sha, Sha);

            using (var document = JsonDocument.Parse(S.Compare().WithInputs(inputs).ToJson()))
            {
                Assert.Equal(JsonValueKind.Null, document.RootElement.GetProperty("inputs").GetProperty("manifestFingerprint").ValueKind);
                Assert.Equal(JsonValueKind.Null, document.RootElement.GetProperty("inputs").GetProperty("readbackFingerprint").ValueKind);
            }
        }

        [Theory]
        [InlineData("0123456789ABCDEF0123456789abcdef0123456789abcdef0123456789abcdef")]
        [InlineData("0123456789abcdef")]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("g123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef")]
        public void A_hash_is_sixty_four_lower_case_hexadecimal_characters(string hash)
        {
            Assert.Throws<ArgumentException>(() => new StructuralComparisonInputs(M.Fingerprint, M.Fingerprint, hash, Sha, Sha, Sha));
            Assert.Throws<ArgumentException>(() => new StructuralComparisonInputs(M.Fingerprint, M.Fingerprint, Sha, Sha, Sha, hash));
        }

        [Fact]
        public void The_inputs_and_the_decision_travel_in_a_rejected_report_too()
        {
            var report = StructuralReadbackComparison.Rejected("because").WithInputs(Inputs()).WithCombinationsDecision("decision");

            using (var document = JsonDocument.Parse(report.ToJson()))
            {
                Assert.Equal("REJECTED", document.RootElement.GetProperty("verdict").GetString());
                Assert.Equal(
                    new[] { "schema", "verdict", "reason", "combinationsDecision", "inputs", "tolerances", "rows" },
                    document.RootElement.EnumerateObject().Select(p => p.Name).ToArray());
            }
        }

        [Fact]
        public void The_inputs_are_required()
        {
            Assert.Throws<ArgumentNullException>(() => S.Compare().WithInputs(null));
        }
    }
}
