using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using RackCad.Application.StructuralAnalysis;
using RackCad.Application.StructuralAnalysis.Providers.RamElements;
using RackCad.StructuralAnalysis.RamTool;
using Xunit;
using Xunit.Abstractions;
using C = RackCad.Tests.StructuralAnalysisCantileverFixtures;
using D = RackCad.Tests.StructuralAnalysisRamElementsDumpSupport;
using F = RackCad.Tests.StructuralAnalysisRamElementsFixtures;
using H = RackCad.Tests.StructuralAnalysisComparisonHardeningSupport;
using R = RackCad.Application.StructuralAnalysis.StructuralComparisonRowIds;
using S = RackCad.Tests.StructuralAnalysisComparisonSupport;
using V = RackCad.Application.StructuralAnalysis.StructuralComparisonVerdict;

namespace RackCad.Tests
{
    /// <summary>
    /// I-65 F4-T6 — A65-F4-06, the general rehearsal. A manual dump of REF-CANT-01 written with the values of section 2 of the engineering
    /// package (coordinates to 4 decimals, column twist -90 in one variant and 270 in the other, load positions as percentages to 3
    /// decimals, other ids and another order than the generated model, and the default case CM without loads) against the dump of the
    /// generated model, which is coherent with the golden of F1 and F3, and its read-back. Both variants PASS; a twist of +90, a percentage
    /// of one decimal and an inverted member FAIL. The fixtures are synthetic: nothing here is a measurement of the analysis program.
    /// </summary>
    public sealed class StructuralAnalysisComparisonHardeningRehearsalTests : IDisposable
    {
        private const string Generated = "generated.dump.json";
        private const string Readback = "generated.readback.json";
        private const string VariantA = "manual-a.dump.json";
        private const string VariantB = "manual-b.dump.json";

        private readonly ITestOutputHelper _output;
        private readonly string _temp = Path.Combine(Path.GetTempPath(), "rackcad-t6-rehearsal-" + Guid.NewGuid().ToString("N"));

        public StructuralAnalysisComparisonHardeningRehearsalTests(ITestOutputHelper output)
        {
            _output = output;
            Directory.CreateDirectory(_temp);
        }

        public void Dispose()
        {
            try
            {
                Directory.Delete(_temp, true);
            }
            catch (IOException)
            {
            }
        }

        private static JsonObject Fixture(string name) =>
            (JsonObject)JsonNode.Parse(new UTF8Encoding(false, true).GetString(C.FixtureBytes("rehearsal/" + name)));

        private static StructuralComparisonReport Compare(JsonObject manual, StructuralComparisonOptions options = null)
        {
            var generated = RamDumpReader.Read(Fixture(Generated).ToJsonString());
            var read = RamDumpReader.Read(manual.ToJsonString());
            var readback = RamReadbackReader.Read(Fixture(Readback).ToJsonString());

            Assert.True(generated.Outcome == StructuralExportOutcome.Valid, "El volcado del generado no se lee: " + D.Codes(generated));
            Assert.True(read.Outcome == StructuralExportOutcome.Valid, "El volcado manual no se lee: " + D.Codes(read));
            Assert.True(readback.Outcome == StructuralExportOutcome.Valid, "La lectura de vuelta no se lee: " + string.Join(", ", readback.Diagnostics.Select(d => d.Code + "(" + d.Subject + ")")));

            return StructuralReadbackComparison.CompareReadbacks(
                generated.Readback, read.Readback, readback.Transfer, readback.Transfer.AnalysisInputFingerprint, options);
        }

        private static IEnumerable<JsonObject> Members(JsonObject dump) => ((JsonArray)dump["members"]).Select(m => (JsonObject)m);

        private static bool IsColumn(JsonObject member) => ((string)member["section"]).StartsWith("W ", StringComparison.Ordinal);

        private static string Describe(StructuralComparisonReport report) =>
            report.Outcome + ": " + string.Join("; ", report.Rows.Select(r => r.Id + "=" + r.Verdict + (r.DifferenceCount > 0 ? " [" + string.Join(" | ", r.Differences) + "]" : string.Empty)));

        // ---------------------------------------------------------------- the generated side is the golden

        [Fact]
        public void The_dump_of_the_generated_model_is_coherent_with_the_golden_of_the_translation()
        {
            var table = F.RefTable();
            var plan = RamElementsTranslator.Translate(F.ValidRefModel(table), table).Plan;
            var dump = Fixture(Generated);
            var nodes = ((JsonArray)dump["nodes"]).Select(n => (JsonObject)n).ToList();
            var members = Members(dump).ToList();
            var failures = new List<string>();

            void Check(string label, double actual, double expected)
            {
                if (Math.Abs(actual - expected) > 1e-9 * Math.Max(1.0, Math.Abs(expected)))
                {
                    failures.Add(label + ": " + actual + " frente a " + expected);
                }
            }

            Assert.Equal(plan.Nodes.Count, nodes.Count);
            Assert.Equal(plan.Members.Count, members.Count);

            foreach (var node in plan.Nodes)
            {
                var row = nodes.Single(n => (string)n["description"] == node.Id);
                var coordinates = ((JsonArray)row["coordinates"]).Select(c => (double)c).ToArray();

                Check(node.Id + " x", coordinates[0], node.Position.X);
                Check(node.Id + " y", coordinates[1], node.Position.Y);
                Check(node.Id + " z", coordinates[2], node.Position.Z);
                Assert.Equal(
                    new[] { node.Restraints.Dx, node.Restraints.Dy, node.Restraints.Dz, node.Restraints.Rx, node.Restraints.Ry, node.Restraints.Rz },
                    ((JsonArray)row["restraints"]).Select(f => (bool)f).ToArray());
            }

            foreach (var member in plan.Members)
            {
                var row = members.Single(m => (string)m["description"] == member.Id);
                var a = member.LocalAxes;

                Assert.Equal(member.SectionName, (string)row["section"]);
                Assert.Equal(member.MaterialName, (string)row["material"]);
                Check(member.Id + " theta", (double)row["rotation"], member.ThetaDegrees);

                var axes = new[] { ("l1", a.L1), ("l2", a.L2), ("l3", a.L3) };

                foreach (var axis in axes)
                {
                    var values = ((JsonArray)row[axis.Item1]).Select(v => (double)v).ToArray();
                    Check(member.Id + " " + axis.Item1 + " x", values[0], axis.Item2.X);
                    Check(member.Id + " " + axis.Item1 + " y", values[1], axis.Item2.Y);
                    Check(member.Id + " " + axis.Item1 + " z", values[2], axis.Item2.Z);
                }
            }

            foreach (var load in plan.Loads)
            {
                var member = members.Single(m => (string)m["description"] == load.MemberId);
                var group = ((JsonArray)((JsonArray)dump["loadCases"]).Select(c => (JsonObject)c).Single(c => (string)c["id"] == plan.LoadCaseId)["memberLoads"])
                    .Select(g => (JsonObject)g).Single(g => (int)g["member"] == (int)member["id"]);
                var entry = (JsonObject)((JsonArray)group["loads"])[0];

                Check(load.Id + " value", (double)entry["value"], load.ValuePoundsForce);
                Check(load.Id + " percent", (double)entry["distance"], load.PositionPercent);
                Assert.True((bool)entry["isPercentage"]);
            }

            Assert.True(failures.Count == 0, "El volcado del generado no es el golden:\n" + string.Join("\n", failures));
        }

        [Fact]
        public void The_read_back_of_the_fixture_is_approved_and_binds_the_dump_of_the_generated_model()
        {
            var readback = RamReadbackReader.Read(Fixture(Readback).ToJsonString());

            Assert.Equal(StructuralExportOutcome.Valid, readback.Outcome);
            Assert.True(readback.Transfer.Passed);
            Assert.Equal(8, readback.Transfer.Values.Nodes.Count);
            Assert.Equal(7, readback.Transfer.Values.Members.Count);
            Assert.Equal(3, readback.Transfer.Values.Loads.Count);
            Assert.Equal("REF1", readback.Transfer.Values.LoadCase.Id);
        }

        // ---------------------------------------------------------------- the manual side as the engineer would type it

        [Theory]
        [InlineData(VariantA, -90.0)]
        [InlineData(VariantB, 270.0)]
        public void The_manual_dump_written_with_the_values_of_the_engineering_package_passes(string variant, double twist)
        {
            var manual = Fixture(variant);
            var report = Compare(manual);

            Assert.True(report.Outcome == StructuralComparisonOutcome.Pass, Describe(report));

            foreach (var id in new[] { R.NodeCoordinates, R.Connectivity, R.LocalAxes, R.SectionName, R.MaterialName, R.MemberDefaults, R.ReleasesSupports, R.Loads, R.Combinations, R.CaseCategory })
            {
                Assert.Equal(V.Pass, S.VerdictOf(report, id));
            }

            Assert.All(Members(manual).Where(IsColumn), m => Assert.Equal(twist, (double)m["rotation"]));
            Assert.All(Members(manual).Where(m => !IsColumn(m)), m => Assert.Equal(0.0, (double)m["rotation"]));
        }

        [Theory]
        [InlineData(VariantA)]
        [InlineData(VariantB)]
        public void The_manual_dump_has_four_decimals_three_decimals_in_the_percentages_and_other_ids_and_order(string variant)
        {
            var manual = Fixture(variant);
            var generated = Fixture(Generated);

            foreach (var node in (JsonArray)manual["nodes"])
            {
                foreach (var coordinate in (JsonArray)node["coordinates"])
                {
                    var value = (double)coordinate;
                    Assert.True(Math.Abs((value * 10000.0) - Math.Round(value * 10000.0)) < 1e-6, "Mas de 4 decimales: " + value);
                }
            }

            foreach (var group in (JsonArray)((JsonArray)manual["loadCases"]).Select(c => (JsonObject)c).Single(c => (string)c["id"] == "REF1")["memberLoads"])
            {
                foreach (var load in (JsonArray)group["loads"])
                {
                    var value = (double)load["distance"];
                    Assert.True((bool)load["isPercentage"]);
                    Assert.True(Math.Abs((value * 1000.0) - Math.Round(value * 1000.0)) < 1e-6, "Mas de 3 decimales: " + value);
                }
            }

            var manualIds = ((JsonArray)manual["nodes"]).Select(n => (int)n["id"]).ToList();
            var generatedIds = ((JsonArray)generated["nodes"]).Select(n => (int)n["id"]).ToList();

            Assert.Empty(manualIds.Intersect(generatedIds));
            Assert.NotEqual(
                ((JsonArray)generated["nodes"]).Select(n => (string)n["description"]).ToList(),
                ((JsonArray)manual["nodes"]).Select(n => ((string)n["description"]).Substring("MAN.".Length)).ToList());
        }

        [Theory]
        [InlineData(VariantA, "DEAD")]
        [InlineData(VariantB, "")]
        public void The_default_case_without_loads_is_in_the_manual_dump_and_is_ignored(string variant, string category)
        {
            var manual = RamDumpReader.Read(Fixture(variant).ToJsonString());

            Assert.Equal(StructuralExportOutcome.Valid, manual.Outcome);

            var cm = manual.Readback.LoadCases.Single(c => c.Id == "CM");
            Assert.Empty(cm.Loads);
            Assert.Equal(category, cm.Category);
            Assert.Equal(V.Pass, S.VerdictOf(Compare(Fixture(variant)), R.Loads));
        }

        // ---------------------------------------------------------------- what must FAIL

        [Theory]
        [InlineData(VariantA)]
        [InlineData(VariantB)]
        public void A_twist_of_plus_ninety_in_the_columns_fails_the_local_axes(string variant)
        {
            var manual = Fixture(variant);

            foreach (var column in Members(manual).Where(IsColumn))
            {
                column["rotation"] = 90.0;
                column["l2"] = new JsonArray(0, 0, 1);
                column["l3"] = new JsonArray(1, 0, 0);
            }

            var report = Compare(manual);

            Assert.Equal(StructuralComparisonOutcome.Fail, report.Outcome);
            Assert.Equal(V.Fail, S.VerdictOf(report, R.LocalAxes));
            Assert.True(report.Row(R.LocalAxes).MaxDelta >= 179.0, "Los vectores de +90 son opuestos a los del generado.");
            Assert.Equal(V.Pass, S.VerdictOf(report, R.NodeCoordinates));
            Assert.Equal(V.Pass, S.VerdictOf(report, R.Loads));
        }

        [Theory]
        [InlineData(VariantA)]
        [InlineData(VariantB)]
        public void Load_positions_typed_with_one_decimal_fail_the_loads(string variant)
        {
            var manual = Fixture(variant);

            foreach (var caseJson in ((JsonArray)manual["loadCases"]).Select(c => (JsonObject)c).Where(c => (string)c["id"] == "REF1"))
            {
                foreach (var group in (JsonArray)caseJson["memberLoads"])
                {
                    foreach (var load in (JsonArray)group["loads"])
                    {
                        load["distance"] = Math.Round((double)load["distance"], 1);
                    }
                }
            }

            var report = Compare(manual);

            Assert.Equal(StructuralComparisonOutcome.Fail, report.Outcome);
            Assert.Equal(V.Fail, S.VerdictOf(report, R.Loads));
            Assert.Contains(report.Row(R.Loads).Differences, text => text.Contains("position", StringComparison.Ordinal));
            Assert.Equal(V.Pass, S.VerdictOf(report, R.LocalAxes));
        }

        [Theory]
        [InlineData(VariantA)]
        [InlineData(VariantB)]
        public void A_member_drawn_the_wrong_way_round_fails_the_connectivity(string variant)
        {
            var manual = Fixture(variant);
            var arm = Members(manual).First(m => !IsColumn(m));
            var nodeI = arm["nodeI"].DeepClone();
            arm["nodeI"] = arm["nodeJ"].DeepClone();
            arm["nodeJ"] = nodeI;

            var report = Compare(manual);

            Assert.Equal(StructuralComparisonOutcome.Fail, report.Outcome);
            Assert.Equal(V.Fail, S.VerdictOf(report, R.Connectivity));
            Assert.Contains(report.Row(R.Connectivity).Differences, text => text.Contains("inverted", StringComparison.Ordinal));
            Assert.Equal(V.Pass, S.VerdictOf(report, R.NodeCoordinates));
        }

        // ---------------------------------------------------------------- through the tool, from files

        private string Package(string name)
        {
            var folder = Path.Combine(_temp, name);
            var fingerprint = H.WritePackage(folder);
            var readback = Fixture(Readback);
            readback["analysisInputFingerprint"] = fingerprint;
            File.WriteAllText(Path.Combine(folder, RamBuildManifest.ReadbackFileName), readback.ToJsonString(), new UTF8Encoding(false));
            return folder;
        }

        private static string FixturePath(string name) =>
            Path.Combine(C.RepoRoot(), "tests", "RackCad.Tests", "StructuralAnalysis", "Fixtures", "rehearsal", name);

        private (int Code, string Output) Tool(params string[] args)
        {
            var output = new StringWriter();
            var code = RamToolCommands.Run(args, output, new StringWriter());

            return (code, output.ToString());
        }

        [Theory]
        [InlineData(VariantA)]
        [InlineData(VariantB)]
        public void The_tool_passes_the_rehearsal_from_files_and_the_report_carries_the_inputs(string variant)
        {
            var result = Tool("compare", "--package", Package("pkg"), "--generated", FixturePath(Generated), "--manual", FixturePath(variant));

            _output.WriteLine("REHEARSAL-PASS-REPORT " + variant + "\n" + result.Output);
            Assert.Equal(RamToolExitCodes.Pass, result.Code);

            using (var report = JsonDocument.Parse(result.Output))
            {
                Assert.Equal("PASS", report.RootElement.GetProperty("verdict").GetString());
                Assert.Equal(H.Sha256(File.ReadAllBytes(FixturePath(variant))), report.RootElement.GetProperty("inputs").GetProperty("manualDumpSha256").GetString());
                Assert.Equal(H.Sha256(File.ReadAllBytes(FixturePath(Generated))), report.RootElement.GetProperty("inputs").GetProperty("generatedDumpSha256").GetString());
            }
        }

        [Fact]
        public void The_tool_gives_incomplete_for_the_rehearsal_when_the_combinations_are_declared_unread()
        {
            var result = Tool(
                "compare", "--package", Package("pkg"), "--generated", FixturePath(Generated), "--manual", FixturePath(VariantA),
                "--manual-combinations-unread", "--combinations-decision", "Rehearsal: the combinations of the manual dump were not read.");

            _output.WriteLine("REHEARSAL-INCOMPLETE-REPORT\n" + result.Output);
            Assert.Equal(RamToolExitCodes.Incomplete, result.Code);

            using (var report = JsonDocument.Parse(result.Output))
            {
                Assert.Equal("INCOMPLETE", report.RootElement.GetProperty("verdict").GetString());
                Assert.Equal("Rehearsal: the combinations of the manual dump were not read.", report.RootElement.GetProperty("combinationsDecision").GetString());
            }
        }

        [Fact]
        public void The_tool_fails_the_rehearsal_with_a_twist_of_plus_ninety()
        {
            var manual = Fixture(VariantA);

            foreach (var column in Members(manual).Where(IsColumn))
            {
                column["rotation"] = 90.0;
                column["l2"] = new JsonArray(0, 0, 1);
                column["l3"] = new JsonArray(1, 0, 0);
            }

            var path = Path.Combine(_temp, "manual-plus90.json");
            File.WriteAllText(path, manual.ToJsonString(), new UTF8Encoding(false));

            var result = Tool("compare", "--package", Package("pkg"), "--generated", FixturePath(Generated), "--manual", path);

            Assert.Equal(RamToolExitCodes.Fail, result.Code);
            Assert.Contains("\"verdict\": \"FAIL\"", result.Output, StringComparison.Ordinal);
        }
    }
}
