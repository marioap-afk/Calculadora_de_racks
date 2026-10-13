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
    /// I-65 F4-T6 — A65-F4-01. The nodes are paired by EUCLIDEAN distance (0.02 in, same epsilon), and the value of a load is compared
    /// relative to the value of the MANUAL model (and in absolute value, with a named constant, when that value is zero). No other
    /// tolerance of the Freeze changes.
    /// </summary>
    public class StructuralAnalysisComparisonHardeningMetricTests
    {
        private static StructuralComparisonReport CompareWithNodeShift(double dx, double dy, double dz)
        {
            var manual = M.Reference("M", reverse: true);
            var node = manual.Node("M3");
            node.X += dx;
            node.Y += dy;
            node.Z += dz;

            return S.Compare(manual: manual);
        }

        [Theory]
        [InlineData(0.015, 0.015, 0.0, false)]
        [InlineData(0.010, 0.010, 0.0, true)]
        [InlineData(0.0116, 0.0116, 0.0116, false)]
        [InlineData(0.0, 0.015, 0.015, false)]
        [InlineData(0.010, 0.0, 0.010, true)]
        [InlineData(0.011, 0.011, 0.011, true)]
        public void The_nodes_are_paired_by_euclidean_distance_not_by_the_largest_coordinate(double dx, double dy, double dz, bool passes)
        {
            var report = CompareWithNodeShift(dx, dy, dz);

            Assert.Equal(passes ? V.Pass : V.Fail, S.VerdictOf(report, R.NodeCoordinates));
            Assert.Equal(passes ? StructuralComparisonOutcome.Pass : StructuralComparisonOutcome.Fail, report.Outcome);
        }

        [Theory]
        [InlineData(0.015, 0.015, 0.0)]
        [InlineData(0.010, 0.010, 0.0)]
        [InlineData(0.0116, 0.0116, 0.0116)]
        public void The_delta_of_the_coordinates_row_is_the_euclidean_distance(double dx, double dy, double dz)
        {
            var report = CompareWithNodeShift(dx, dy, dz);

            Assert.Equal(Math.Sqrt((dx * dx) + (dy * dy) + (dz * dz)), report.Row(R.NodeCoordinates).MaxDelta.Value, 9);
        }

        [Fact]
        public void The_note_of_the_coordinates_row_says_euclidean_and_not_one_coordinate()
        {
            var note = S.Compare().Row(R.NodeCoordinates).Note;

            Assert.Contains("Euclidean", note, StringComparison.Ordinal);
            Assert.DoesNotContain("one coordinate", note, StringComparison.Ordinal);
        }

        [Fact]
        public void The_tolerance_of_the_nodes_and_its_epsilon_do_not_change()
        {
            Assert.Equal(0.02, StructuralReadbackComparison.NodePositionToleranceInches);

            // exactly on the edge on one axis passes (the epsilon absorbs the last bits); a hair beyond the epsilon fails
            Assert.Equal(V.Pass, S.VerdictOf(CompareWithNodeShift(0.02, 0, 0), R.NodeCoordinates));
            Assert.Equal(V.Fail, S.VerdictOf(CompareWithNodeShift(0.02 + 1e-8, 0, 0), R.NodeCoordinates));
        }

        [Fact]
        public void The_ambiguity_rules_do_not_change_with_the_new_distance()
        {
            var manual = M.Reference("M", reverse: true);
            manual.Nodes.Add(new M.NodeSpec { Id = "MX", X = 0.01, Y = 0.01, Z = 0 });

            var report = S.Compare(manual: manual);

            Assert.Equal(V.Fail, S.VerdictOf(report, R.NodeCoordinates));
            Assert.Contains(report.Row(R.NodeCoordinates).Differences, text => text.Contains("ambiguous", StringComparison.Ordinal));
        }

        private static StructuralComparisonReport CompareWithLoads(double generatedValue, double manualValue)
        {
            var generated = M.Reference();
            var manual = M.Reference("M", reverse: true);
            generated.Loads[0].Value = generatedValue;
            manual.Loads[0].Value = manualValue;

            return S.Compare(generated, manual);
        }

        [Theory]
        [InlineData(999.0, 1000.0, false)]
        [InlineData(1000.0, 999.0, true)]
        [InlineData(1000.0, 1000.9, true)]
        [InlineData(1000.0, 1001.1, false)]
        [InlineData(-999.0, -1000.0, false)]
        [InlineData(-1000.0, -999.0, true)]
        [InlineData(2000.0, 2001.9, true)]
        [InlineData(2000.0, 2002.1, false)]
        public void The_value_of_a_load_is_relative_to_the_manual_value(double manualValue, double generatedValue, bool passes)
        {
            var report = CompareWithLoads(generatedValue, manualValue);

            Assert.Equal(passes ? V.Pass : V.Fail, S.VerdictOf(report, R.Loads));
            Assert.Equal(passes ? StructuralComparisonOutcome.Pass : StructuralComparisonOutcome.Fail, report.Outcome);
        }

        [Fact]
        public void The_delta_of_the_loads_row_is_the_difference_over_the_manual_value()
        {
            var report = CompareWithLoads(1000.0, 999.0);

            Assert.Equal(100.0 * 1.0 / 999.0, report.Row(R.Loads).MaxDelta.Value, 9);
        }

        [Theory]
        [InlineData(0.0, 0.0, true)]
        [InlineData(0.0, 1e-7, true)]
        [InlineData(0.0, -5e-7, true)]
        [InlineData(0.0, 2e-6, false)]
        [InlineData(0.0, 0.001, false)]
        [InlineData(0.0, -250.0, false)]
        public void A_manual_value_of_zero_is_compared_in_absolute_value_against_one_millionth_of_a_pound(double manualValue, double generatedValue, bool passes)
        {
            var report = CompareWithLoads(generatedValue, manualValue);

            Assert.Equal(passes ? V.Pass : V.Fail, S.VerdictOf(report, R.Loads));
            Assert.True(double.IsFinite(report.Row(R.Loads).MaxDelta.Value), "Sin division entre cero.");
        }

        [Fact]
        public void A_generated_value_of_zero_against_a_manual_value_is_still_a_relative_difference()
        {
            var report = CompareWithLoads(0.0, 1000.0);

            Assert.Equal(V.Fail, S.VerdictOf(report, R.Loads));
            Assert.Equal(100.0, report.Row(R.Loads).MaxDelta.Value, 9);
        }

        [Fact]
        public void The_constant_of_the_zero_base_has_a_name_and_the_value_of_the_decision()
        {
            Assert.Equal(1e-6, StructuralReadbackComparison.ZeroLoadAbsoluteToleranceLbf);
        }

        [Fact]
        public void A_failing_zero_base_says_it_is_an_absolute_difference_in_pounds_force()
        {
            var report = CompareWithLoads(0.001, 0.0);
            var row = report.Row(R.Loads);

            Assert.Equal(V.Fail, row.Verdict);
            Assert.Contains(row.Differences, text => text.Contains("absolute difference", StringComparison.Ordinal) && text.Contains("lbf", StringComparison.Ordinal));
            Assert.Contains("0.000001", row.Note, StringComparison.Ordinal);
        }

        [Fact]
        public void The_other_tolerances_of_the_freeze_and_the_other_rows_do_not_change()
        {
            Assert.Equal(0.1, StructuralReadbackComparison.AxisToleranceDegrees);
            Assert.Equal(0.1, StructuralReadbackComparison.LoadValueTolerancePercent);
            Assert.Equal(0.02, StructuralReadbackComparison.LoadPositionToleranceInches);
            Assert.Equal("OTHER", StructuralReadbackComparison.GeneratedCaseCategory);

            var report = S.Compare();

            Assert.Equal(V.NotEvaluated, S.VerdictOf(report, R.SectionProperties));
            Assert.Equal(V.NotEvaluated, S.VerdictOf(report, R.MaterialProperties));
            Assert.Equal(V.Informative, S.VerdictOf(report, R.ManualCaseCategory));
            Assert.Equal(V.Informative, S.VerdictOf(report, R.SelfWeight));
        }
    }
}
