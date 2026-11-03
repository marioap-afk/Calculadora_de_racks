using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using RackCad.Application.StructuralAnalysis;
using RackCad.Application.StructuralAnalysis.Providers.RamElements;
using Xunit;
using D = RackCad.Tests.StructuralAnalysisRamElementsDumpSupport;
using M = RackCad.Tests.StructuralAnalysisComparisonModel;
using R = RackCad.Application.StructuralAnalysis.StructuralComparisonRowIds;
using S = RackCad.Tests.StructuralAnalysisComparisonSupport;
using V = RackCad.Application.StructuralAnalysis.StructuralComparisonVerdict;

namespace RackCad.Tests
{
    /// <summary>
    /// I-65 F4-T4 — the rows of the matrix of section 4 that the comparator evaluates (defaults, releases and supports, combinations, cases
    /// without loads) and the shape of the deterministic report <c>rackcad-structural-comparison/v1</c>.
    /// </summary>
    public class StructuralAnalysisComparisonRowTests
    {
        private static readonly string[] OrderedRows =
        {
            R.NodeCoordinates, R.Connectivity, R.LocalAxes, R.SectionName, R.MaterialName, R.SectionProperties, R.MaterialProperties,
            R.MemberDefaults, R.ReleasesSupports, R.Loads, R.Combinations, R.CaseCategory, R.ManualCaseCategory, R.SelfWeight,
        };

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        public void A_value_other_than_the_default_fails_the_member_defaults(int variant)
        {
            var manual = M.Reference("M", reverse: true);
            var arm = manual.Member("MA1");

            switch (variant)
            {
                case 0:
                    arm.CardinalPoint = 3;
                    break;
                case 1:
                    arm.OffsetI = new double[] { 1, 0, 0 };
                    break;
                case 2:
                    arm.Factors = new double[] { 1, 1, 0.5, 1 };
                    break;
                case 3:
                    arm.Axial = StructuralReadbackAxialBehavior.TensionOnly;
                    break;
                case 4:
                    arm.SpringJ = 5;
                    break;
                default:
                    arm.OffsetJ = new double[] { 0, 0, -0.5 };
                    break;
            }

            var report = S.Compare(manual: manual);

            Assert.Equal(StructuralComparisonOutcome.Fail, report.Outcome);
            Assert.Equal(V.Fail, S.VerdictOf(report, R.MemberDefaults));
        }

        [Fact]
        public void The_defaults_are_checked_on_the_generated_model_too()
        {
            var generated = M.Reference();
            generated.Member("C1").CardinalPoint = 5;

            var report = S.Compare(generated: generated);

            Assert.Equal(V.Fail, S.VerdictOf(report, R.MemberDefaults));
        }

        [Fact]
        public void A_release_that_differs_fails_the_releases_and_supports()
        {
            var manual = M.Reference("M", reverse: true);
            manual.Member("MA1").Releases[2] = true;

            var report = S.Compare(manual: manual);

            Assert.Equal(V.Fail, S.VerdictOf(report, R.ReleasesSupports));
        }

        [Fact]
        public void A_support_flag_that_differs_fails_the_releases_and_supports()
        {
            var manual = M.Reference("M", reverse: true);
            manual.Node("M2").Flags = new[] { true, false, false, false, false, false };

            var report = S.Compare(manual: manual);

            Assert.Equal(V.Fail, S.VerdictOf(report, R.ReleasesSupports));
        }

        [Fact]
        public void Without_combinations_in_both_the_row_passes()
        {
            Assert.Equal(V.Pass, S.VerdictOf(S.Compare(), R.Combinations));
        }

        [Fact]
        public void A_combination_in_one_model_fails()
        {
            var manual = M.Reference("M", reverse: true);
            manual.Combinations.Add(("COMBO1", new[] { ("REF1", 1.0) }));

            var report = S.Compare(manual: manual);

            Assert.Equal(StructuralComparisonOutcome.Fail, report.Outcome);
            Assert.Equal(V.Fail, S.VerdictOf(report, R.Combinations));
        }

        [Fact]
        public void A_combination_in_both_fails_because_none_is_expected()
        {
            var generated = M.Reference();
            var manual = M.Reference("M", reverse: true);
            generated.Combinations.Add(("COMBO1", new[] { ("REF1", 1.0) }));
            manual.Combinations.Add(("COMBO1", new[] { ("REF1", 1.0) }));

            Assert.Equal(V.Fail, S.VerdictOf(S.Compare(generated, manual), R.Combinations));
        }

        [Fact]
        public void Combinations_a_dump_could_not_read_are_not_evaluated()
        {
            var report = S.Compare(options: new StructuralComparisonOptions(manualCombinationsRead: false));

            Assert.Equal(V.NotEvaluated, S.VerdictOf(report, R.Combinations));
            Assert.False(string.IsNullOrWhiteSpace(report.Row(R.Combinations).Note));
            Assert.Equal(StructuralComparisonOutcome.Incomplete, report.Outcome);
        }

        [Fact]
        public void Cases_without_loads_are_ignored_in_both_models()
        {
            var generated = M.Reference();
            var manual = M.Reference("M", reverse: true);
            generated.EmptyCases = new List<string> { "CM", "EXTRA1" };
            manual.EmptyCases = new List<string>();

            var report = S.Compare(generated, manual);

            Assert.Equal(StructuralComparisonOutcome.Pass, report.Outcome);
            Assert.Equal(V.Pass, S.VerdictOf(report, R.Loads));
        }

        [Fact]
        public void The_same_load_given_as_a_percentage_and_as_a_distance_passes()
        {
            var asPercent = D.Valid();
            var asDistance = D.Valid();
            asDistance["loadCases"] = new JsonArray(D.Case("REF1", "OTHER", D.MemberLoads(21, D.Load("Y", -1000, 25, false))));

            var generated = D.Read(asPercent);
            var manual = D.Read(asDistance);
            Assert.NotNull(generated.Readback);
            Assert.NotNull(manual.Readback);

            var report = StructuralReadbackComparison.CompareReadbacks(generated.Readback, manual.Readback, S.Approved(), M.Fingerprint);

            Assert.Equal(StructuralComparisonOutcome.Pass, report.Outcome);
            Assert.Equal(V.Pass, S.VerdictOf(report, R.Loads));
        }

        [Fact]
        public void The_report_has_one_row_per_row_of_the_matrix_in_a_stable_order()
        {
            var report = S.Compare();

            Assert.Equal(OrderedRows, report.Rows.Select(row => row.Id).ToArray());
        }

        [Fact]
        public void The_report_is_deterministic_in_value_and_in_bytes()
        {
            var manual = M.Reference("M", reverse: true);
            manual.Node("M3").X += 0.019;

            var first = S.Compare(manual: manual).ToJson();
            var second = S.Compare(manual: manual).ToJson();

            Assert.Equal(first, second);
        }

        [Fact]
        public void The_report_does_not_depend_on_the_culture()
        {
            var manual = M.Reference("M", reverse: true);
            manual.Node("M3").X += 0.019;
            var previous = CultureInfo.CurrentCulture;

            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("de-DE");
                var german = S.Compare(manual: manual).ToJson();
                CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
                var invariant = S.Compare(manual: manual).ToJson();

                Assert.Equal(invariant, german);
                Assert.Contains("0.019", invariant, StringComparison.Ordinal);
            }
            finally
            {
                CultureInfo.CurrentCulture = previous;
            }
        }

        [Fact]
        public void The_serialized_report_uses_LF_and_carries_no_timestamp()
        {
            var json = S.Compare().ToJson();

            Assert.DoesNotContain("\r", json, StringComparison.Ordinal);
            Assert.EndsWith("\n", json, StringComparison.Ordinal);
            Assert.False(Regex.IsMatch(json, @"\d{4}-\d{2}-\d{2}"), "El informe no lleva fechas.");
        }

        [Fact]
        public void The_serialized_report_has_the_exact_shape_of_the_schema()
        {
            using (var document = JsonDocument.Parse(S.Compare().ToJson()))
            {
                var root = document.RootElement;

                Assert.Equal(
                    new[] { "schema", "verdict", "reason", "tolerances", "rows" },
                    root.EnumerateObject().Select(p => p.Name).ToArray());
                Assert.Equal("rackcad-structural-comparison/v1", root.GetProperty("schema").GetString());
                Assert.Equal("PASS", root.GetProperty("verdict").GetString());
                Assert.Equal(JsonValueKind.Null, root.GetProperty("reason").ValueKind);

                var rows = root.GetProperty("rows").EnumerateArray().ToList();
                Assert.Equal(OrderedRows, rows.Select(r => r.GetProperty("id").GetString()).ToArray());

                foreach (var row in rows)
                {
                    Assert.Equal(
                        new[] { "id", "name", "verdict", "maxDelta", "unit", "differenceCount", "differences", "note" },
                        row.EnumerateObject().Select(p => p.Name).ToArray());
                    Assert.Contains(row.GetProperty("verdict").GetString(), new[] { "PASS", "FAIL", "INFORMATIVE", "NOT_EVALUATED" });
                }
            }
        }

        [Fact]
        public void A_rejected_and_a_stale_report_serialize_their_verdict_and_reason()
        {
            using (var rejected = JsonDocument.Parse(S.Compare(transfer: S.Failed("gate: the model is not empty")).ToJson()))
            using (var stale = JsonDocument.Parse(S.Compare(fingerprint: M.OtherFingerprint).ToJson()))
            {
                Assert.Equal("REJECTED", rejected.RootElement.GetProperty("verdict").GetString());
                Assert.Equal("STALE", stale.RootElement.GetProperty("verdict").GetString());
                Assert.False(string.IsNullOrWhiteSpace(rejected.RootElement.GetProperty("reason").GetString()));
                Assert.False(string.IsNullOrWhiteSpace(stale.RootElement.GetProperty("reason").GetString()));
            }
        }

        [Fact]
        public void The_first_differences_are_five_in_the_order_of_the_model_and_the_count_is_complete()
        {
            var generated = Chain("G", "W 12X26");
            var manual = Chain("H", "W 14X30");

            var report = S.Compare(generated, manual);
            var row = report.Row(R.SectionName);

            Assert.Equal(V.Fail, row.Verdict);
            Assert.Equal(7, row.DifferenceCount);
            Assert.Equal(5, row.Differences.Count);

            for (var index = 0; index < 5; index++)
            {
                Assert.Contains("GM" + (index + 1).ToString(CultureInfo.InvariantCulture), row.Differences[index], StringComparison.Ordinal);
            }
        }

        [Fact]
        public void The_comparator_does_no_input_output_and_keeps_no_clock_or_state()
        {
            var file = StructuralAnalysisGuards.Files("src/RackCad.Application/StructuralAnalysis")
                .Single(f => f.Path.EndsWith("/StructuralReadbackComparison.cs", StringComparison.Ordinal));
            var code = StructuralAnalysisGuards.Code(file.Source);

            Assert.False(
                Regex.IsMatch(code, @"\bDateTime\b|\bFile\s*\.|\bDirectory\s*\.|\bStopwatch\b|\bRandom\b|\bGuid\b|\bEnvironment\b|\bConsole\b"),
                "El comparador es puro: sin E/S, reloj ni azar.");
            Assert.False(Regex.IsMatch(code, @"\bstatic\s+(?!readonly\b|class\b)[\w<>\[\]?,]+\s+\w+\s*(=[^>]|;)"), "Sin estado estatico mutable.");
        }

        /// <summary>Eight nodes in a row along X joined by seven members of the given section; one load on the first.</summary>
        private static M Chain(string prefix, string section)
        {
            var model = new M();

            for (var index = 0; index <= 7; index++)
            {
                model.Nodes.Add(new M.NodeSpec { Id = prefix + "N" + index, X = index * 40, Y = 0, Z = 0 });
            }

            for (var index = 1; index <= 7; index++)
            {
                model.Members.Add(new M.MemberSpec
                {
                    Id = prefix + "M" + index,
                    NodeI = prefix + "N" + (index - 1),
                    NodeJ = prefix + "N" + index,
                    Section = section,
                    L1 = new double[] { 1, 0, 0 },
                    L2 = new double[] { 0, 1, 0 },
                    L3 = new double[] { 0, 0, 1 },
                });
            }

            model.Loads.Add(new M.LoadSpec { MemberId = prefix + "M1", Axis = StructuralReadbackLoadAxis.GlobalZ, Value = -500, Distance = 10 });
            return model;
        }
    }
}
