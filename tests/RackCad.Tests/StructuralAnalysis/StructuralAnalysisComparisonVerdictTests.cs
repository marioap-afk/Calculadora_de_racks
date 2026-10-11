using System;
using System.Linq;
using RackCad.Application.StructuralAnalysis;
using Xunit;
using M = RackCad.Tests.StructuralAnalysisComparisonModel;
using R = RackCad.Application.StructuralAnalysis.StructuralComparisonRowIds;
using S = RackCad.Tests.StructuralAnalysisComparisonSupport;
using V = RackCad.Application.StructuralAnalysis.StructuralComparisonVerdict;

namespace RackCad.Tests
{
    /// <summary>
    /// I-65 F4-T4 — INV-13 (D-13): the comparator on synthetic neutral readbacks built in the test. Equal -> PASS; one thing changed -> the
    /// row that reads it FAILS and the overall verdict is FAIL; a rejected read-back or a stale fingerprint approves nothing.
    /// </summary>
    public class StructuralAnalysisComparisonVerdictTests
    {
        [Fact]
        public void Equal_models_with_other_identities_and_order_pass()
        {
            var report = S.Compare();

            Assert.True(report.Outcome == StructuralComparisonOutcome.Pass, S.Describe(report));

            foreach (var id in new[] { R.NodeCoordinates, R.Connectivity, R.LocalAxes, R.SectionName, R.MaterialName, R.MemberDefaults, R.ReleasesSupports, R.Loads, R.Combinations, R.CaseCategory })
            {
                Assert.Equal(V.Pass, S.VerdictOf(report, id));
            }
        }

        [Fact]
        public void A_node_moved_one_millimeter_fails_the_coordinates()
        {
            var manual = M.Reference("M", reverse: true);
            manual.Node("M3").X += StructuralLength.FromMillimeters(1).Inches;

            var report = S.Compare(manual: manual);

            Assert.Equal(StructuralComparisonOutcome.Fail, report.Outcome);
            Assert.Equal(V.Fail, S.VerdictOf(report, R.NodeCoordinates));
        }

        [Theory]
        [InlineData(0.019, true)]
        [InlineData(0.021, false)]
        public void Node_tolerance_edge_is_two_hundredths_of_an_inch(double shift, bool passes)
        {
            var manual = M.Reference("M", reverse: true);
            manual.Node("M3").X += shift;

            var report = S.Compare(manual: manual);

            Assert.Equal(passes ? V.Pass : V.Fail, S.VerdictOf(report, R.NodeCoordinates));
            Assert.Equal(passes ? StructuralComparisonOutcome.Pass : StructuralComparisonOutcome.Fail, report.Outcome);
        }

        [Fact]
        public void A_changed_section_fails_the_section_row()
        {
            var manual = M.Reference("M", reverse: true);
            manual.Member("MA1").Section = "HSS 5X5X1/4";

            var report = S.Compare(manual: manual);

            Assert.Equal(StructuralComparisonOutcome.Fail, report.Outcome);
            Assert.Equal(V.Fail, S.VerdictOf(report, R.SectionName));
            Assert.Equal(V.Pass, S.VerdictOf(report, R.MaterialName));
        }

        [Fact]
        public void A_changed_material_fails_the_material_row()
        {
            var manual = M.Reference("M", reverse: true);
            manual.Member("MC1").Material = "A36";

            var report = S.Compare(manual: manual);

            Assert.Equal(V.Fail, S.VerdictOf(report, R.MaterialName));
            Assert.Equal(V.Pass, S.VerdictOf(report, R.SectionName));
        }

        [Fact]
        public void A_column_twisted_the_other_way_fails_the_local_axes()
        {
            var manual = M.Reference("M", reverse: true);
            var column = manual.Member("MC1");
            column.L2 = new double[] { -1, 0, 0 };
            column.L3 = new double[] { 0, -1, 0 };

            var report = S.Compare(manual: manual);

            Assert.Equal(StructuralComparisonOutcome.Fail, report.Outcome);
            Assert.Equal(V.Fail, S.VerdictOf(report, R.LocalAxes));
            Assert.True(report.Row(R.LocalAxes).MaxDelta >= 179.0, "El delta maximo debe ser el angulo de 180 grados.");
        }

        [Theory]
        [InlineData(0.09, true)]
        [InlineData(0.11, false)]
        public void Axis_tolerance_edge_is_a_tenth_of_a_degree(double degrees, bool passes)
        {
            var manual = M.Reference("M", reverse: true);
            var radians = degrees * Math.PI / 180.0;
            manual.Member("MA1").L1 = new[] { Math.Cos(radians), Math.Sin(radians), 0.0 };

            var report = S.Compare(manual: manual);

            Assert.Equal(passes ? V.Pass : V.Fail, S.VerdictOf(report, R.LocalAxes));
        }

        [Fact]
        public void A_load_a_thousand_times_larger_fails_the_loads()
        {
            var manual = M.Reference("M", reverse: true);
            manual.Loads[0].Value *= 1000;

            var report = S.Compare(manual: manual);

            Assert.Equal(StructuralComparisonOutcome.Fail, report.Outcome);
            Assert.Equal(V.Fail, S.VerdictOf(report, R.Loads));
        }

        [Theory]
        [InlineData(0.09, true)]
        [InlineData(0.11, false)]
        public void Load_value_tolerance_edge_is_a_tenth_of_a_percent(double percent, bool passes)
        {
            var manual = M.Reference("M", reverse: true);
            manual.Loads[0].Value *= 1 + percent / 100.0;

            var report = S.Compare(manual: manual);

            Assert.Equal(passes ? V.Pass : V.Fail, S.VerdictOf(report, R.Loads));
        }

        [Theory]
        [InlineData(0.019, true)]
        [InlineData(0.021, false)]
        public void Load_position_tolerance_edge_is_two_hundredths_of_an_inch(double shift, bool passes)
        {
            var manual = M.Reference("M", reverse: true);
            manual.Loads[0].Distance += shift;

            var report = S.Compare(manual: manual);

            Assert.Equal(passes ? V.Pass : V.Fail, S.VerdictOf(report, R.Loads));
        }

        [Fact]
        public void A_load_in_another_direction_fails_the_loads()
        {
            var manual = M.Reference("M", reverse: true);
            manual.Loads[0].Axis = StructuralReadbackLoadAxis.Local3;

            var report = S.Compare(manual: manual);

            Assert.Equal(V.Fail, S.VerdictOf(report, R.Loads));
        }

        [Fact]
        public void A_missing_load_fails_the_loads()
        {
            var manual = M.Reference("M", reverse: true);
            manual.Loads.Add(new M.LoadSpec { MemberId = "MA1", Axis = StructuralReadbackLoadAxis.GlobalZ, Value = -10, Distance = 40 });

            var report = S.Compare(manual: manual);

            Assert.Equal(V.Fail, S.VerdictOf(report, R.Loads));
        }

        [Fact]
        public void An_ambiguous_node_pairing_fails_and_is_never_resolved_in_silence()
        {
            var manual = M.Reference("M", reverse: true);
            manual.Nodes.Add(new M.NodeSpec { Id = "MX", X = 0.01, Y = 0, Z = 0 });

            var report = S.Compare(manual: manual);

            Assert.Equal(StructuralComparisonOutcome.Fail, report.Outcome);
            Assert.Equal(V.Fail, S.VerdictOf(report, R.NodeCoordinates));
            Assert.Contains(report.Row(R.NodeCoordinates).Differences, text => text.Contains("ambiguous", StringComparison.Ordinal));
        }

        [Fact]
        public void A_candidate_paired_twice_fails()
        {
            var generated = M.Reference();
            generated.Nodes.Add(new M.NodeSpec { Id = "9", X = 0.01, Y = 0, Z = 100 });

            var report = S.Compare(generated: generated);

            Assert.Equal(StructuralComparisonOutcome.Fail, report.Outcome);
            Assert.Equal(V.Fail, S.VerdictOf(report, R.NodeCoordinates));
            Assert.Contains(report.Row(R.NodeCoordinates).Differences, text => text.Contains("twice", StringComparison.Ordinal));
        }

        [Fact]
        public void A_member_with_its_nodes_inverted_fails_the_connectivity()
        {
            var manual = M.Reference("M", reverse: true);
            var arm = manual.Member("MA1");
            (arm.NodeI, arm.NodeJ) = (arm.NodeJ, arm.NodeI);

            var report = S.Compare(manual: manual);

            Assert.Equal(StructuralComparisonOutcome.Fail, report.Outcome);
            Assert.Equal(V.Fail, S.VerdictOf(report, R.Connectivity));
            Assert.Contains(report.Row(R.Connectivity).Differences, text => text.Contains("inverted", StringComparison.Ordinal));
        }

        [Fact]
        public void Members_are_paired_by_their_nodes_whatever_the_order_and_the_names()
        {
            var report = S.Compare(M.Reference("G"), M.Reference("Z", reverse: true));

            Assert.Equal(V.Pass, S.VerdictOf(report, R.Connectivity));
            Assert.Equal(StructuralComparisonOutcome.Pass, report.Outcome);
        }

        [Fact]
        public void A_member_missing_in_the_manual_model_fails_the_connectivity()
        {
            var manual = M.Reference("M", reverse: true);
            manual.Members.RemoveAll(m => m.Id == "MA1");
            manual.Loads.Clear();

            var report = S.Compare(manual: manual);

            Assert.Equal(V.Fail, S.VerdictOf(report, R.Connectivity));
        }

        [Fact]
        public void A_read_back_in_a_failed_state_rejects_and_approves_no_row()
        {
            var report = S.Compare(transfer: S.Failed("members: expected 2 and read 1"));

            Assert.Equal(StructuralComparisonOutcome.Rejected, report.Outcome);
            Assert.False(string.IsNullOrWhiteSpace(report.Reason));
            Assert.DoesNotContain(report.Rows, row => row.Verdict == V.Pass);
        }

        [Fact]
        public void A_rejection_by_a_gate_is_a_rejection_too()
        {
            var report = S.Compare(transfer: S.Failed("gate: the model is not empty"));

            Assert.Equal(StructuralComparisonOutcome.Rejected, report.Outcome);
            Assert.DoesNotContain(report.Rows, row => row.Verdict == V.Pass);
        }

        [Fact]
        public void Another_fingerprint_is_stale_and_approves_no_row()
        {
            var report = S.Compare(fingerprint: M.OtherFingerprint);

            Assert.Equal(StructuralComparisonOutcome.Stale, report.Outcome);
            Assert.DoesNotContain(report.Rows, row => row.Verdict == V.Pass);
        }

        [Fact]
        public void A_generated_category_other_than_OTHER_fails()
        {
            var generated = M.Reference();
            generated.Category = "DEAD";

            var report = S.Compare(generated: generated);

            Assert.Equal(StructuralComparisonOutcome.Fail, report.Outcome);
            Assert.Equal(V.Fail, S.VerdictOf(report, R.CaseCategory));
        }

        [Fact]
        public void A_manual_category_other_than_OTHER_is_only_informative()
        {
            var manual = M.Reference("M", reverse: true);
            manual.Category = "LIVE";

            var report = S.Compare(manual: manual);

            Assert.Equal(StructuralComparisonOutcome.Pass, report.Outcome);
            Assert.Equal(V.Informative, S.VerdictOf(report, R.ManualCaseCategory));
            Assert.Contains("LIVE", report.Row(R.ManualCaseCategory).Note, StringComparison.Ordinal);
        }

        [Fact]
        public void The_self_weight_of_the_loaded_cases_is_informative()
        {
            var generated = M.Reference();
            generated.SelfWeight = new double[] { 0, 0, -1 };

            var report = S.Compare(generated: generated);

            Assert.Equal(V.Informative, S.VerdictOf(report, R.SelfWeight));
            Assert.Equal(StructuralComparisonOutcome.Pass, report.Outcome);
        }

        [Fact]
        public void The_section_and_material_properties_are_not_evaluated_and_say_who_evaluates_them()
        {
            var report = S.Compare();

            foreach (var id in new[] { R.SectionProperties, R.MaterialProperties })
            {
                Assert.Equal(V.NotEvaluated, S.VerdictOf(report, id));
                Assert.Contains("signed", report.Row(id).Note, StringComparison.Ordinal);
            }
        }

        [Fact]
        public void Null_inputs_are_refused()
        {
            var model = M.Reference().Build();

            Assert.Throws<ArgumentNullException>(() => StructuralReadbackComparison.CompareReadbacks(null, model, S.Approved(), M.Fingerprint));
            Assert.Throws<ArgumentNullException>(() => StructuralReadbackComparison.CompareReadbacks(model, null, S.Approved(), M.Fingerprint));
            Assert.Throws<ArgumentNullException>(() => StructuralReadbackComparison.CompareReadbacks(model, model, null, M.Fingerprint));
        }

        [Fact]
        public void The_tolerances_are_the_ones_of_the_freeze()
        {
            Assert.Equal(0.02, StructuralReadbackComparison.NodePositionToleranceInches);
            Assert.Equal(0.1, StructuralReadbackComparison.AxisToleranceDegrees);
            Assert.Equal(0.1, StructuralReadbackComparison.LoadValueTolerancePercent);
            Assert.Equal(0.02, StructuralReadbackComparison.LoadPositionToleranceInches);
        }
    }
}
