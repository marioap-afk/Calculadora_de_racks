using System;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using RackCad.Application.StructuralAnalysis;
using RackCad.Application.StructuralAnalysis.Providers.RamElements;
using Xunit;
using C = RackCad.Tests.StructuralAnalysisCantileverFixtures;
using Codes = RackCad.Application.StructuralAnalysis.Providers.RamElements.RamDumpDiagnosticCodes;
using D = RackCad.Tests.StructuralAnalysisRamElementsDumpSupport;
using H = RackCad.Tests.StructuralAnalysisComparisonHardeningSupport;
using M = RackCad.Tests.StructuralAnalysisComparisonModel;
using S = RackCad.Tests.StructuralAnalysisComparisonSupport;

namespace RackCad.Tests
{
    /// <summary>
    /// I-65 F4-T6 — A65-F4-05 (decision of the Owner; the semantics of the program for an empty category are NOT MEASURED, CD-I65-F4-34).
    /// The category of a case is Invalid when it is absent, null or only blanks; an exactly empty category is accepted ONLY on a case without
    /// loads (the cases the comparison ignores) and is Invalid, naming the case, on a case with loads. Nothing else about the program is assumed.
    /// </summary>
    public class StructuralAnalysisComparisonHardeningDumpReaderTests
    {
        /// <summary>The sample dump with a second case without loads, whose category is the given one.</summary>
        private static JsonObject WithEmptyCase(string category)
        {
            var dump = D.Valid();
            ((JsonArray)dump["loadCases"]).Add(D.Case("CM", category));
            return dump;
        }

        private static JsonObject FirstCase(JsonObject dump) => dump["loadCases"].ObjectAt(0);

        [Fact]
        public void An_absent_category_is_invalid()
        {
            var dump = D.Valid();
            FirstCase(dump).Remove("category");

            var result = D.Read(dump);

            Assert.NotEqual(StructuralExportOutcome.Valid, result.Outcome);
            Assert.Null(result.Readback);
            Assert.Contains(result.Diagnostics, d => d.Code == Codes.DumpMissingProperty && d.IsBlocking);
        }

        [Fact]
        public void A_null_category_is_invalid()
        {
            var dump = D.Valid();
            FirstCase(dump)["category"] = null;

            var result = D.Read(dump);

            Assert.NotEqual(StructuralExportOutcome.Valid, result.Outcome);
            Assert.Null(result.Readback);
            Assert.Contains(result.Diagnostics, d => d.Code == Codes.DumpWrongType && d.IsBlocking);
        }

        [Theory]
        [InlineData(" ")]
        [InlineData("   ")]
        [InlineData("\t")]
        public void A_category_of_only_blanks_is_ambiguous_and_invalid_even_without_loads(string category)
        {
            var result = D.Read(WithEmptyCase(category));

            Assert.NotEqual(StructuralExportOutcome.Valid, result.Outcome);
            Assert.Null(result.Readback);
            Assert.Contains(result.Diagnostics, d => d.Code == Codes.DumpValueInvalid && d.IsBlocking && d.Subject.Contains("category", StringComparison.Ordinal));
        }

        [Fact]
        public void A_category_of_only_blanks_is_invalid_with_loads_too()
        {
            var dump = D.Valid();
            FirstCase(dump)["category"] = " ";

            var result = D.Read(dump);

            Assert.NotEqual(StructuralExportOutcome.Valid, result.Outcome);
            Assert.Null(result.Readback);
        }

        [Fact]
        public void An_exactly_empty_category_on_a_case_without_loads_is_valid()
        {
            var result = D.Read(WithEmptyCase(string.Empty));

            Assert.Equal(StructuralExportOutcome.Valid, result.Outcome);
            Assert.Empty(result.Diagnostics);

            var empty = result.Readback.LoadCases.Single(c => c.Id == "CM");
            Assert.Equal(string.Empty, empty.Category);
            Assert.Empty(empty.Loads);
            Assert.Equal("OTHER", result.Readback.LoadCases.Single(c => c.Id == "REF1").Category);
        }

        [Fact]
        public void An_empty_category_on_a_case_whose_groups_have_no_loads_is_valid_too()
        {
            var dump = D.Valid();
            ((JsonArray)dump["loadCases"]).Add(D.Case("CM", string.Empty, D.MemberLoads(21)));

            var result = D.Read(dump);

            Assert.Equal(StructuralExportOutcome.Valid, result.Outcome);
            Assert.Empty(result.Readback.LoadCases.Single(c => c.Id == "CM").Loads);
        }

        [Fact]
        public void An_empty_category_on_a_case_with_loads_is_invalid_and_the_diagnostic_names_the_case()
        {
            var dump = D.Valid();
            FirstCase(dump)["category"] = string.Empty;

            var result = D.Read(dump);

            Assert.NotEqual(StructuralExportOutcome.Valid, result.Outcome);
            Assert.Null(result.Readback);
            Assert.Contains(
                result.Diagnostics,
                d => d.Code == Codes.DumpValueInvalid && d.IsBlocking && d.Subject.Contains("REF1", StringComparison.Ordinal)
                     && d.Subject.Contains("category", StringComparison.Ordinal));
        }

        [Fact]
        public void A_second_case_with_loads_and_an_empty_category_is_named_too()
        {
            var dump = D.Valid();
            ((JsonArray)dump["loadCases"]).Add(D.Case("EXTRA", string.Empty, D.MemberLoads(21, D.Load("Y", -5, 10, true))));

            var result = D.Read(dump);

            Assert.NotEqual(StructuralExportOutcome.Valid, result.Outcome);
            Assert.Contains(result.Diagnostics, d => d.Subject.Contains("EXTRA", StringComparison.Ordinal));
            Assert.DoesNotContain(result.Diagnostics, d => d.Subject.Contains("REF1", StringComparison.Ordinal));
        }

        [Fact]
        public void A_non_empty_category_on_a_case_without_loads_is_unchanged()
        {
            var result = D.Read(WithEmptyCase("DEAD"));

            Assert.Equal(StructuralExportOutcome.Valid, result.Outcome);
            Assert.Equal("DEAD", result.Readback.LoadCases.Single(c => c.Id == "CM").Category);
        }

        [Fact]
        public void A_case_without_loads_and_with_an_empty_category_is_ignored_by_the_comparison()
        {
            var generated = D.Read(WithEmptyCase(string.Empty)).Readback;
            var manualDump = H.ManualDump();
            ((JsonArray)manualDump["loadCases"]).Add(D.Case("CM", string.Empty));
            var manual = D.Read(manualDump).Readback;
            var values = S.ValuesOf(generated);
            var transfer = new StructuralTransferReadback(true, M.Fingerprint, M.AdapterVersion, new string[0], values);

            var report = StructuralReadbackComparison.CompareReadbacks(generated, manual, transfer, M.Fingerprint);

            Assert.Equal(StructuralComparisonOutcome.Pass, report.Outcome);
            Assert.Equal(StructuralComparisonVerdict.Pass, S.VerdictOf(report, StructuralComparisonRowIds.Loads));
        }

        [Fact]
        public void The_reader_records_that_the_semantics_of_an_empty_category_are_not_measured()
        {
            var source = File.ReadAllText(
                Path.Combine(C.RepoRoot(), "src", "RackCad.Application", "StructuralAnalysis", "Providers", "RamElements", "RamDumpReader.cs"));

            Assert.Contains("NOT MEASURED", source, StringComparison.Ordinal);
            Assert.Contains("CD-I65-F4-34", source, StringComparison.Ordinal);
        }

        [Fact]
        public void The_neutral_case_without_a_category_is_created_only_without_loads()
        {
            var empty = StructuralReadbackLoadCase.WithoutLoadsAndCategory("CM", new StructuralDirection(0, 0, 0));

            Assert.Equal(string.Empty, empty.Category);
            Assert.Empty(empty.Loads);
            Assert.Throws<ArgumentException>(() => new StructuralReadbackLoadCase("X", string.Empty, new StructuralDirection(0, 0, 0), new StructuralReadbackPointLoad[0]));
            Assert.Throws<ArgumentException>(() => StructuralReadbackLoadCase.WithoutLoadsAndCategory(" ", new StructuralDirection(0, 0, 0)));
        }
    }
}
