using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using RackCad.Application.StructuralAnalysis;
using RackCad.Application.StructuralAnalysis.Providers.RamElements;
using Xunit;
using Codes = RackCad.Application.StructuralAnalysis.Providers.RamElements.RamDumpDiagnosticCodes;
using D = RackCad.Tests.StructuralAnalysisRamElementsDumpSupport;
using H = RackCad.Tests.StructuralAnalysisComparisonHardeningSupport;
using M = RackCad.Tests.StructuralAnalysisComparisonModel;
using S = RackCad.Tests.StructuralAnalysisComparisonSupport;
using V = RackCad.Application.StructuralAnalysis.StructuralComparisonVerdict;

namespace RackCad.Tests
{
    /// <summary>
    /// I-65 F4-T6 — A65-F4-03 (binding). The reader of the read-back reads <c>values</c> strictly into a neutral type, and the comparison
    /// binds the dump of the GENERATED model to those values (ids and coordinates of the nodes, members with section and material, loads
    /// by member with value and distance, counts): a mismatch, or no values, is REJECTED with the first mismatch in the reason.
    /// </summary>
    public class StructuralAnalysisComparisonHardeningReadbackTests
    {
        private const string Binding = "The dump of the generated model does not agree with the values of the read-back of the build: ";

        private static RamReadbackReadResult Read(JsonObject values, string status = "READBACK OK") =>
            RamReadbackReader.Read(H.Readback(status, M.Fingerprint, new string[0], values).ToJsonString());

        // ---------------------------------------------------------------- the strict reader

        [Fact]
        public void The_values_the_build_writes_are_read_into_the_neutral_type()
        {
            var result = Read(H.SampleValues());

            Assert.Equal(StructuralExportOutcome.Valid, result.Outcome);
            Assert.NotNull(result.Transfer.Values);

            var values = result.Transfer.Values;
            Assert.Equal(2, values.Nodes.Count);
            Assert.Single(values.Members);
            Assert.Single(values.Loads);
            Assert.Equal("REF1", values.LoadCase.Id);
            Assert.Equal("OTHER", values.LoadCase.Category);
        }

        [Fact]
        public void The_values_come_back_in_the_neutral_frame_and_in_typed_quantities()
        {
            var values = Read(H.SampleValues()).Transfer.Values;
            var top = values.Nodes.Single(n => n.Id == "N2");

            // the frame of the program (Y up) goes back to the neutral frame (Z up): X = x, Y = minus z, Z = y
            Assert.Equal(0.0, top.Position.X.Inches);
            Assert.Equal(0.0, top.Position.Y.Inches);
            Assert.Equal(100.0, top.Position.Z.Inches);
            Assert.Equal("12", top.ModelId);
            Assert.Equal(new[] { true, false, false, false, true, true }, top.SupportFlags.ToArray());

            var member = values.Members.Single();
            Assert.Equal("M1", member.Id);
            Assert.Equal("21", member.ModelId);
            Assert.Equal("W 12X26", member.SectionName);
            Assert.Equal("A992 Gr50", member.MaterialName);

            var load = values.Loads.Single();
            Assert.Equal("M1", load.MemberId);
            Assert.Equal(0, load.Index);
            Assert.Equal(StructuralReadbackLoadAxis.GlobalZ, load.Axis);
            Assert.Equal(-1000.0, load.Value.PoundsForce);
            Assert.Equal(25.0, load.Distance);
            Assert.True(load.IsPercentage);
        }

        [Fact]
        public void Empty_lists_and_a_load_case_that_is_null_are_still_valid_and_give_empty_values()
        {
            var values = new JsonObject
            {
                ["nodes"] = new JsonArray(),
                ["members"] = new JsonArray(),
                ["loads"] = new JsonArray(),
                ["loadCase"] = null,
            };

            var result = Read(values);

            Assert.Equal(StructuralExportOutcome.Valid, result.Outcome);
            Assert.Empty(result.Transfer.Values.Nodes);
            Assert.Empty(result.Transfer.Values.Members);
            Assert.Empty(result.Transfer.Values.Loads);
            Assert.Null(result.Transfer.Values.LoadCase);
        }

        [Fact]
        public void A_description_that_is_null_is_accepted_because_the_program_may_report_none()
        {
            var values = H.SampleValues();
            values["members"].ObjectAt(0)["description"] = null;

            Assert.Equal(StructuralExportOutcome.Valid, Read(values).Outcome);
        }

        [Fact]
        public void A_failed_read_back_keeps_its_differences_and_needs_no_values()
        {
            var result = Read(new JsonObject(), RamReadbackReader.StatusFailed);

            Assert.Equal(StructuralExportOutcome.Valid, result.Outcome);
            Assert.False(result.Transfer.Passed);
            Assert.Null(result.Transfer.Values);
        }

        [Fact]
        public void The_constructor_without_values_stays_and_gives_no_values()
        {
            var transfer = new StructuralTransferReadback(true, M.Fingerprint, M.AdapterVersion, new string[0]);

            Assert.True(transfer.Passed);
            Assert.Null(transfer.Values);
        }

        private static readonly Dictionary<string, (Action<JsonObject> Change, string Code)> Variants = BuildVariants();

        public static IEnumerable<object[]> VariantNames() => Variants.Keys.Select(name => new object[] { name });

        private static JsonObject Node(JsonObject values) => values["nodes"].ObjectAt(0);

        private static JsonObject Member(JsonObject values) => values["members"].ObjectAt(0);

        private static JsonObject Load(JsonObject values) => values["loads"].ObjectAt(0);

        private static Dictionary<string, (Action<JsonObject>, string)> BuildVariants()
        {
            var all = new Dictionary<string, (Action<JsonObject>, string)>(StringComparer.Ordinal);

            all["clave desconocida en values"] = (v => v["extra"] = 1, Codes.ReadbackUnknownProperty);
            all["falta nodes"] = (v => v.Remove("nodes"), Codes.ReadbackMissingProperty);
            all["falta loadCase"] = (v => v.Remove("loadCase"), Codes.ReadbackMissingProperty);
            all["nodes no es una lista"] = (v => v["nodes"] = new JsonObject(), Codes.ReadbackWrongType);
            all["loadCase es una lista"] = (v => v["loadCase"] = new JsonArray(), Codes.ReadbackWrongType);
            all["clave desconocida en un nudo"] = (v => Node(v)["extra"] = 1, Codes.ReadbackUnknownProperty);
            all["falta ramId en un nudo"] = (v => Node(v).Remove("ramId"), Codes.ReadbackMissingProperty);
            all["id de un nudo numerico"] = (v => Node(v)["id"] = 1, Codes.ReadbackWrongType);
            all["id de un nudo en blanco"] = (v => Node(v)["id"] = " ", Codes.ReadbackValueInvalid);
            all["ramId de un nudo como texto"] = (v => Node(v)["ramId"] = "11", Codes.ReadbackWrongType);
            all["ramId de un nudo con fraccion"] = (v => Node(v)["ramId"] = 11.5, Codes.ReadbackValueInvalid);
            all["coordenadas con dos numeros"] = (v => Node(v)["coordinates"] = D.Numbers(0, 0), Codes.ReadbackValueInvalid);
            all["coordenada como texto"] = (v => Node(v)["coordinates"] = new JsonArray(0, "0", 0), Codes.ReadbackWrongType);
            all["restricciones con cinco banderas"] = (v => Node(v)["restraints"] = D.Bools(true, true, true, true, true), Codes.ReadbackValueInvalid);
            all["restriccion que no es booleana"] = (v => Node(v)["restraints"] = new JsonArray(true, true, true, true, true, 1), Codes.ReadbackWrongType);
            all["clave desconocida en un miembro"] = (v => Member(v)["extra"] = 1, Codes.ReadbackUnknownProperty);
            all["falta section en un miembro"] = (v => Member(v).Remove("section"), Codes.ReadbackMissingProperty);
            all["section numerica"] = (v => Member(v)["section"] = 1, Codes.ReadbackWrongType);
            all["material en blanco"] = (v => Member(v)["material"] = "", Codes.ReadbackValueInvalid);
            all["rotation como texto"] = (v => Member(v)["rotation"] = "0", Codes.ReadbackWrongType);
            all["punto cardinal fuera de rango"] = (v => Member(v)["cardinalPoint"] = 10, Codes.ReadbackValueInvalid);
            all["releases con nueve banderas"] = (v => Member(v)["releases"] = D.Bools(new bool[9]), Codes.ReadbackValueInvalid);
            all["l2 con dos numeros"] = (v => Member(v)["l2"] = D.Numbers(0, 1), Codes.ReadbackValueInvalid);
            all["description numerica"] = (v => Member(v)["description"] = 5, Codes.ReadbackWrongType);
            all["clave desconocida en una carga"] = (v => Load(v)["extra"] = 1, Codes.ReadbackUnknownProperty);
            all["falta percent en una carga"] = (v => Load(v).Remove("percent"), Codes.ReadbackMissingProperty);
            all["indice con fraccion"] = (v => Load(v)["index"] = 0.5, Codes.ReadbackValueInvalid);
            all["indice negativo"] = (v => Load(v)["index"] = -1, Codes.ReadbackValueInvalid);
            all["direccion desconocida"] = (v => Load(v)["direction"] = "Q", Codes.ReadbackValueInvalid);
            all["direccion numerica"] = (v => Load(v)["direction"] = 2, Codes.ReadbackWrongType);
            all["valor de carga como texto"] = (v => Load(v)["value"] = "-1000", Codes.ReadbackWrongType);
            all["isPercentage como texto"] = (v => Load(v)["isPercentage"] = "yes", Codes.ReadbackWrongType);
            all["clave desconocida en loadCase"] = (v => v["loadCase"]["extra"] = 1, Codes.ReadbackUnknownProperty);
            all["falta category en loadCase"] = (v => v["loadCase"].AsObject().Remove("category"), Codes.ReadbackMissingProperty);
            all["category numerica"] = (v => v["loadCase"]["category"] = 1, Codes.ReadbackWrongType);
            all["category en blanco"] = (v => v["loadCase"]["category"] = "", Codes.ReadbackValueInvalid);
            all["selfWeight con dos numeros"] = (v => v["loadCase"]["selfWeight"] = D.Numbers(0, 0), Codes.ReadbackValueInvalid);

            return all.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        }

        [Theory]
        [MemberData(nameof(VariantNames))]
        public void The_values_are_read_strictly_and_a_variant_is_never_approved(string name)
        {
            var (change, code) = Variants[name];
            var values = H.SampleValues();
            change(values);

            var result = Read(values);

            Assert.NotEqual(StructuralExportOutcome.Valid, result.Outcome);
            Assert.Null(result.Transfer);
            Assert.Contains(result.Diagnostics, d => d.Code == code && d.IsBlocking);
        }

        // ---------------------------------------------------------------- the binding of the dump of the generated model

        [Fact]
        public void Values_that_agree_with_the_dump_bind_and_the_comparison_goes_on()
        {
            var report = S.Compare();

            Assert.Equal(StructuralComparisonOutcome.Pass, report.Outcome);
            Assert.Null(report.Reason);
        }

        [Theory]
        [InlineData(StructuralAnalysisValuesMismatch.NodeCount, "nodes")]
        [InlineData(StructuralAnalysisValuesMismatch.NodeId, "calls it")]
        [InlineData(StructuralAnalysisValuesMismatch.NodeCoordinate, "the read-back (")]
        [InlineData(StructuralAnalysisValuesMismatch.NodeSupport, "support flags")]
        [InlineData(StructuralAnalysisValuesMismatch.MemberCount, "members")]
        [InlineData(StructuralAnalysisValuesMismatch.MemberId, "calls it")]
        [InlineData(StructuralAnalysisValuesMismatch.MemberSection, "section")]
        [InlineData(StructuralAnalysisValuesMismatch.MemberMaterial, "material")]
        [InlineData(StructuralAnalysisValuesMismatch.LoadCount, "loads")]
        [InlineData(StructuralAnalysisValuesMismatch.LoadValue, "lbf")]
        [InlineData(StructuralAnalysisValuesMismatch.LoadDistance, "in from node I")]
        [InlineData(StructuralAnalysisValuesMismatch.LoadDirection, "direction")]
        [InlineData(StructuralAnalysisValuesMismatch.LoadCaseId, "load case")]
        [InlineData(StructuralAnalysisValuesMismatch.LoadCaseCategory, "category")]
        [InlineData(StructuralAnalysisValuesMismatch.NoLoadCase, "no load case")]
        public void A_dump_that_does_not_agree_with_the_values_is_rejected_with_the_first_mismatch(StructuralAnalysisValuesMismatch mismatch, string keyword)
        {
            var report = S.Compare(mismatch: mismatch);

            Assert.Equal(StructuralComparisonOutcome.Rejected, report.Outcome);
            Assert.StartsWith(Binding, report.Reason, StringComparison.Ordinal);
            Assert.Contains(keyword, report.Reason, StringComparison.Ordinal);
            Assert.DoesNotContain(report.Rows, row => row.Verdict == V.Pass);
            Assert.Equal(14, report.Rows.Count);
        }

        [Fact]
        public void A_read_back_without_values_is_rejected_and_says_so()
        {
            var report = S.Compare(transfer: S.ApprovedWithoutValues());

            Assert.Equal(StructuralComparisonOutcome.Rejected, report.Outcome);
            Assert.StartsWith(Binding, report.Reason, StringComparison.Ordinal);
            Assert.Contains("no values", report.Reason, StringComparison.Ordinal);
        }

        [Fact]
        public void A_read_back_with_empty_values_does_not_bind_a_dump_that_has_nodes()
        {
            var empty = new StructuralTransferValues(
                new StructuralTransferNodeValue[0], new StructuralTransferMemberValue[0], new StructuralTransferLoadValue[0], null);
            var transfer = new StructuralTransferReadback(true, M.Fingerprint, M.AdapterVersion, new string[0], empty);

            var report = S.Compare(transfer: transfer);

            Assert.Equal(StructuralComparisonOutcome.Rejected, report.Outcome);
            Assert.Contains("0 nodes", report.Reason, StringComparison.Ordinal);
        }

        [Fact]
        public void A_difference_of_five_ten_millionths_of_an_inch_still_binds_and_two_millionths_does_not()
        {
            var generated = M.Reference().Build();

            Assert.Equal(StructuralComparisonOutcome.Pass, CompareWithNodeShift(generated, 5e-7).Outcome);
            Assert.Equal(StructuralComparisonOutcome.Rejected, CompareWithNodeShift(generated, 2e-6).Outcome);
        }

        private static StructuralComparisonReport CompareWithNodeShift(StructuralModelReadback generated, double shift)
        {
            var values = S.ValuesOf(generated);
            var nodes = values.Nodes.ToList();
            var first = nodes[0];
            nodes[0] = new StructuralTransferNodeValue(
                first.Id,
                first.ModelId,
                M.Position(first.Position.X.Inches + shift, first.Position.Y.Inches, first.Position.Z.Inches),
                first.SupportFlags);

            var transfer = new StructuralTransferReadback(
                true, M.Fingerprint, M.AdapterVersion, new string[0], new StructuralTransferValues(nodes, values.Members, values.Loads, values.LoadCase));

            return StructuralReadbackComparison.CompareReadbacks(generated, M.Reference("M", reverse: true).Build(), transfer, M.Fingerprint);
        }

        [Fact]
        public void A_percentage_becomes_a_distance_with_the_length_of_the_member()
        {
            // the arm A1 of the reference goes from (0,0,100) to (50,0,100): 50 in, and its load is at 25 in, that is 50 percent
            var generated = M.Reference().Build();
            var values = S.ValuesOf(generated);
            var load = values.Loads.Single();

            StructuralComparisonReport With(double percent)
            {
                var percentLoad = new StructuralTransferLoadValue(load.MemberId, load.Index, load.Axis, load.Value, percent, true);
                var transfer = new StructuralTransferReadback(
                    true, M.Fingerprint, M.AdapterVersion, new string[0], new StructuralTransferValues(values.Nodes, values.Members, new[] { percentLoad }, values.LoadCase));

                return StructuralReadbackComparison.CompareReadbacks(generated, M.Reference("M", reverse: true).Build(), transfer, M.Fingerprint);
            }

            Assert.Equal(StructuralComparisonOutcome.Pass, With(50.0).Outcome);

            var wrong = With(51.0);
            Assert.Equal(StructuralComparisonOutcome.Rejected, wrong.Outcome);
            Assert.Contains("in from node I", wrong.Reason, StringComparison.Ordinal);
        }

        [Fact]
        public void A_load_of_a_member_the_read_back_does_not_list_is_a_mismatch()
        {
            var generated = M.Reference().Build();
            var values = S.ValuesOf(generated);
            var load = values.Loads.Single();
            var orphan = new StructuralTransferLoadValue("NOBODY", load.Index, load.Axis, load.Value, load.Distance, false);
            var transfer = new StructuralTransferReadback(
                true, M.Fingerprint, M.AdapterVersion, new string[0], new StructuralTransferValues(values.Nodes, values.Members, new[] { orphan }, values.LoadCase));

            var report = StructuralReadbackComparison.CompareReadbacks(generated, M.Reference("M", reverse: true).Build(), transfer, M.Fingerprint);

            Assert.Equal(StructuralComparisonOutcome.Rejected, report.Outcome);
            Assert.Contains("no such member", report.Reason, StringComparison.Ordinal);
        }

        [Fact]
        public void A_dump_with_loads_in_another_case_than_the_one_the_read_back_lists_is_a_mismatch()
        {
            var generated = M.Reference();
            var readback = generated.Build();
            var values = S.ValuesOf(readback);

            var extra = new StructuralReadbackLoadCase(
                "OTHERCASE",
                "DEAD",
                new StructuralDirection(0, 0, 0),
                new[] { new StructuralReadbackPointLoad("A1", StructuralReadbackLoadAxis.GlobalZ, StructuralForce.FromPoundsForce(-5), StructuralLength.FromInches(10)) });
            var withExtra = new StructuralModelReadback(
                readback.AdapterVersion, readback.InitialUnitPair, readback.UnitsRestored, readback.Nodes, readback.Members, readback.LoadCases.Concat(new[] { extra }), readback.Combinations);
            var loads = values.Loads.Concat(new[] { new StructuralTransferLoadValue("A1", 1, StructuralReadbackLoadAxis.GlobalZ, StructuralForce.FromPoundsForce(-5), 10, false) });
            var transfer = new StructuralTransferReadback(
                true, M.Fingerprint, M.AdapterVersion, new string[0], new StructuralTransferValues(values.Nodes, values.Members, loads, values.LoadCase));

            var report = StructuralReadbackComparison.CompareReadbacks(withExtra, M.Reference("M", reverse: true).Build(), transfer, M.Fingerprint);

            Assert.Equal(StructuralComparisonOutcome.Rejected, report.Outcome);
            Assert.Contains("OTHERCASE", report.Reason, StringComparison.Ordinal);
        }

        [Fact]
        public void A_read_back_that_is_not_approved_or_is_stale_is_judged_before_the_values()
        {
            var failed = S.Compare(transfer: S.Failed("members: expected 2 and read 1"), mismatch: StructuralAnalysisValuesMismatch.NodeCoordinate);
            var stale = S.Compare(fingerprint: M.OtherFingerprint, mismatch: StructuralAnalysisValuesMismatch.NodeCoordinate);

            Assert.Equal(StructuralComparisonOutcome.Rejected, failed.Outcome);
            Assert.StartsWith("The read-back of the build is not approved", failed.Reason, StringComparison.Ordinal);
            Assert.Equal(StructuralComparisonOutcome.Stale, stale.Outcome);
        }

        [Fact]
        public void The_binding_does_not_look_at_the_manual_model()
        {
            var manual = M.Reference("M", reverse: true);
            manual.Node("M3").X += 5;

            var report = S.Compare(manual: manual);

            Assert.Equal(StructuralComparisonOutcome.Fail, report.Outcome);
            Assert.Null(report.Reason);
        }

        [Fact]
        public void The_neutral_type_and_the_comparator_name_no_analysis_program()
        {
            var types = new[]
            {
                typeof(StructuralTransferValues), typeof(StructuralTransferNodeValue), typeof(StructuralTransferMemberValue),
                typeof(StructuralTransferLoadValue), typeof(StructuralTransferCaseValue),
            };

            foreach (var type in types)
            {
                Assert.Equal("RackCad.Application.StructuralAnalysis", type.Namespace);
                Assert.DoesNotContain("Providers", type.FullName, StringComparison.Ordinal);
                Assert.DoesNotContain(type.GetProperties(), p => p.Name.StartsWith("Ram", StringComparison.Ordinal));
            }
        }
    }
}
