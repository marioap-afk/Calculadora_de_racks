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
using D = RackCad.Tests.StructuralAnalysisRamElementsDumpSupport;
using H = RackCad.Tests.StructuralAnalysisComparisonHardeningSupport;

namespace RackCad.Tests
{
    /// <summary>
    /// I-65 F4-T6 — the console tool after A65-F4-02, 03 and 04: combinations declared unread need the written decision and end INCOMPLETE
    /// (its own exit code); the two dumps must be independent; the report carries the fingerprints and the SHA-256 of the four inputs; the
    /// dump of the generated model must agree with the values of the read-back; and a package marked as a fault injection is never approved.
    /// The tests call the classes of the tool on temporary files and run nothing external.
    /// </summary>
    public sealed class StructuralAnalysisComparisonHardeningToolTests : IDisposable
    {
        private const string EmbeddedInjectFolder = "C:\\RackCadInjected\\Case-1";
        private const string GeneratedUnread = "--generated-combinations-unread";
        private const string ManualUnread = "--manual-combinations-unread";
        private const string Decision = "--combinations-decision";

        private readonly string _temp = Path.Combine(Path.GetTempPath(), "rackcad-t6-" + Guid.NewGuid().ToString("N"));

        public StructuralAnalysisComparisonHardeningToolTests()
        {
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

        private string Path_(string name) => Path.Combine(_temp, name);

        private (int Code, string Output, string Error) Run(params string[] args)
        {
            var output = new StringWriter();
            var error = new StringWriter();
            var code = RamToolCommands.Run(args, output, error);

            return (code, output.ToString(), error.ToString());
        }

        private (int Code, string Output, string Error) Compare(string package, string generated, string manual, params string[] extra) =>
            Run(new[] { "compare", "--package", package, "--generated", generated, "--manual", manual }.Concat(extra).ToArray());

        private string Package(string name, string status = null, string[] differences = null, JsonObject values = null)
        {
            var folder = Path_(name);
            H.WritePackage(folder, status, null, differences, values);
            return folder;
        }

        private string Dump(string name, JsonObject dump) => H.WriteDump(Path_(name), dump);

        private static JsonDocument Report(string output) => JsonDocument.Parse(output);

        // ---------------------------------------------------------------- A65-F4-02

        [Fact]
        public void The_incomplete_exit_code_is_distinct_from_every_other_one()
        {
            var codes = new[]
            {
                RamToolExitCodes.Pass, RamToolExitCodes.Fail, RamToolExitCodes.Rejected, RamToolExitCodes.InvalidUsage, RamToolExitCodes.OutputRefused,
                RamToolExitCodes.Incomplete,
            };

            Assert.Equal(codes.Length, codes.Distinct().Count());
        }

        [Theory]
        [InlineData(GeneratedUnread, null)]
        [InlineData(ManualUnread, null)]
        [InlineData(GeneratedUnread, "")]
        [InlineData(ManualUnread, "")]
        [InlineData(GeneratedUnread, "   ")]
        [InlineData(ManualUnread, "\t")]
        public void Declaring_combinations_unread_without_a_decision_with_a_text_is_invalid_usage(string flag, string decision)
        {
            var package = Package("pkg");
            var args = new List<string> { flag };

            if (decision != null)
            {
                args.Add(Decision);
                args.Add(decision);
            }

            var result = Compare(package, Dump("g.json", D.Valid()), Dump("m.json", H.ManualDump()), args.ToArray());

            Assert.Equal(RamToolExitCodes.InvalidUsage, result.Code);
            Assert.Equal(string.Empty, result.Output);
            Assert.Contains(Decision, result.Error, StringComparison.Ordinal);
        }

        [Theory]
        [InlineData(GeneratedUnread)]
        [InlineData(ManualUnread)]
        public void Combinations_declared_unread_with_a_decision_end_incomplete_with_their_own_code(string flag)
        {
            var package = Package("pkg");

            var result = Compare(package, Dump("g.json", D.Valid()), Dump("m.json", H.ManualDump()), flag, Decision, "The engineer checked them in the interface.");

            Assert.Equal(RamToolExitCodes.Incomplete, result.Code);

            using (var report = Report(result.Output))
            {
                var root = report.RootElement;

                Assert.Equal("INCOMPLETE", root.GetProperty("verdict").GetString());
                Assert.Equal("The engineer checked them in the interface.", root.GetProperty("combinationsDecision").GetString());
                Assert.Equal(14, root.GetProperty("rows").GetArrayLength());
                Assert.Equal(
                    "NOT_EVALUATED",
                    root.GetProperty("rows").EnumerateArray().Single(r => r.GetProperty("id").GetString() == "COMBINATIONS").GetProperty("verdict").GetString());
                Assert.Equal(
                    "PASS",
                    root.GetProperty("rows").EnumerateArray().Single(r => r.GetProperty("id").GetString() == "NODE-COORDINATES").GetProperty("verdict").GetString());
            }
        }

        [Fact]
        public void Both_options_with_one_decision_end_incomplete_too()
        {
            var package = Package("pkg");

            var result = Compare(package, Dump("g.json", D.Valid()), Dump("m.json", H.ManualDump()), GeneratedUnread, ManualUnread, Decision, "both unread");

            Assert.Equal(RamToolExitCodes.Incomplete, result.Code);
        }

        [Fact]
        public void A_failing_row_is_still_a_fail_when_the_combinations_were_declared_unread()
        {
            var package = Package("pkg");
            var changed = H.ManualDump();
            changed["members"].ObjectAt(0)["section"] = "W 14X30";

            var result = Compare(package, Dump("g.json", D.Valid()), Dump("m.json", changed), ManualUnread, Decision, "unread and different");

            Assert.Equal(RamToolExitCodes.Fail, result.Code);

            using (var report = Report(result.Output))
            {
                Assert.Equal("FAIL", report.RootElement.GetProperty("verdict").GetString());
                Assert.Equal("unread and different", report.RootElement.GetProperty("combinationsDecision").GetString());
            }
        }

        [Fact]
        public void Without_the_options_there_is_no_decision_field_and_the_pass_stays_a_pass()
        {
            var package = Package("pkg");

            var result = Compare(package, Dump("g.json", D.Valid()), Dump("m.json", H.ManualDump()));

            Assert.Equal(RamToolExitCodes.Pass, result.Code);

            using (var report = Report(result.Output))
            {
                Assert.False(report.RootElement.TryGetProperty("combinationsDecision", out _));
            }
        }

        [Fact]
        public void A_decision_given_without_the_options_is_written_in_the_report_and_changes_nothing_else()
        {
            var package = Package("pkg");

            var result = Compare(package, Dump("g.json", D.Valid()), Dump("m.json", H.ManualDump()), Decision, "noted");

            Assert.Equal(RamToolExitCodes.Pass, result.Code);

            using (var report = Report(result.Output))
            {
                Assert.Equal("PASS", report.RootElement.GetProperty("verdict").GetString());
                Assert.Equal("noted", report.RootElement.GetProperty("combinationsDecision").GetString());
            }
        }

        [Fact]
        public void The_usage_explains_the_decision_and_the_incomplete_code()
        {
            Assert.Contains(Decision, RamToolCommands.Usage, StringComparison.Ordinal);
            Assert.Contains("INCOMPLETE", RamToolCommands.Usage, StringComparison.Ordinal);
            Assert.Contains("5 INCOMPLETE", RamToolCommands.Usage, StringComparison.Ordinal);
        }

        [Fact]
        public void The_incomplete_report_written_to_a_file_equals_what_is_printed()
        {
            var package = Package("pkg");
            var file = Path_("report.json");

            var result = Compare(package, Dump("g.json", D.Valid()), Dump("m.json", H.ManualDump()), GeneratedUnread, Decision, "text", "--report", file);

            Assert.Equal(RamToolExitCodes.Incomplete, result.Code);
            Assert.Equal(result.Output, File.ReadAllText(file));
        }

        // ---------------------------------------------------------------- A65-F4-03

        [Fact]
        public void The_same_file_as_generated_and_manual_is_rejected()
        {
            var package = Package("pkg");
            var dump = Dump("same.json", D.Valid());

            var result = Compare(package, dump, dump);

            Assert.Equal(RamToolExitCodes.Rejected, result.Code);

            using (var report = Report(result.Output))
            {
                Assert.Equal("REJECTED", report.RootElement.GetProperty("verdict").GetString());
                Assert.Contains("same input", report.RootElement.GetProperty("reason").GetString(), StringComparison.Ordinal);
            }
        }

        [Fact]
        public void Two_files_with_the_same_bytes_as_generated_and_manual_are_rejected()
        {
            var package = Package("pkg");

            var result = Compare(package, Dump("a.json", D.Valid()), Dump("b.json", D.Valid()));

            Assert.Equal(RamToolExitCodes.Rejected, result.Code);

            using (var report = Report(result.Output))
            {
                Assert.Equal("REJECTED", report.RootElement.GetProperty("verdict").GetString());
                Assert.Contains("same input", report.RootElement.GetProperty("reason").GetString(), StringComparison.Ordinal);
                Assert.DoesNotContain(report.RootElement.GetProperty("rows").EnumerateArray(), row => row.GetProperty("verdict").GetString() == "PASS");
            }
        }

        [Fact]
        public void The_same_path_written_in_another_form_is_the_same_file()
        {
            var package = Package("pkg");
            var dump = Dump("same.json", D.Valid());
            var other = Path.Combine(_temp, "sub", "..", "same.json");

            Assert.Equal(RamToolExitCodes.Rejected, Compare(package, dump, other).Code);
        }

        [Fact]
        public void Independent_dumps_carry_the_inputs_with_their_fingerprints_and_their_lower_case_hashes()
        {
            var package = Package("pkg");
            var generated = Dump("g.json", D.Valid());
            var manual = Dump("m.json", H.ManualDump());

            var result = Compare(package, generated, manual);

            Assert.Equal(RamToolExitCodes.Pass, result.Code);

            using (var report = Report(result.Output))
            {
                var inputs = report.RootElement.GetProperty("inputs");
                var fingerprint = JsonDocument.Parse(File.ReadAllText(Path.Combine(package, RamBuildPackage.ManifestFileName))).RootElement.GetProperty("analysisInputFingerprint").GetString();

                Assert.Equal(fingerprint, inputs.GetProperty("manifestFingerprint").GetString());
                Assert.Equal(fingerprint, inputs.GetProperty("readbackFingerprint").GetString());
                Assert.Equal(H.Sha256(File.ReadAllBytes(Path.Combine(package, RamBuildPackage.ManifestFileName))), inputs.GetProperty("manifestSha256").GetString());
                Assert.Equal(H.Sha256(File.ReadAllBytes(Path.Combine(package, RamBuildManifest.ReadbackFileName))), inputs.GetProperty("readbackSha256").GetString());
                Assert.Equal(H.Sha256(File.ReadAllBytes(generated)), inputs.GetProperty("generatedDumpSha256").GetString());
                Assert.Equal(H.Sha256(File.ReadAllBytes(manual)), inputs.GetProperty("manualDumpSha256").GetString());

                foreach (var name in new[] { "manifestSha256", "readbackSha256", "generatedDumpSha256", "manualDumpSha256" })
                {
                    Assert.Matches("^[0-9a-f]{64}$", inputs.GetProperty(name).GetString());
                }
            }
        }

        [Fact]
        public void The_inputs_are_in_the_rejections_and_in_the_incomplete_report_too()
        {
            var failed = Compare(Package("pkg-failed", RamReadbackReader.StatusFailed, new[] { "members: expected 2 and read 1" }), Dump("g.json", D.Valid()), Dump("m.json", H.ManualDump()));
            var same = Compare(Package("pkg-same"), Dump("a.json", D.Valid()), Dump("b.json", D.Valid()));
            var incomplete = Compare(Package("pkg-inc"), Dump("g2.json", D.Valid()), Dump("m2.json", H.ManualDump()), ManualUnread, Decision, "text");

            foreach (var result in new[] { failed, same, incomplete })
            {
                using (var report = Report(result.Output))
                {
                    Assert.Equal(6, report.RootElement.GetProperty("inputs").EnumerateObject().Count());
                }
            }
        }

        [Fact]
        public void A_generated_dump_that_does_not_agree_with_the_values_of_the_read_back_is_rejected()
        {
            var package = Package("pkg");
            var moved = D.Valid();
            moved["nodes"].ObjectAt(1)["coordinates"] = D.Numbers(0, 100.5, 0);

            var result = Compare(package, Dump("g.json", moved), Dump("m.json", H.ManualDump()));

            Assert.Equal(RamToolExitCodes.Rejected, result.Code);

            using (var report = Report(result.Output))
            {
                Assert.Equal("REJECTED", report.RootElement.GetProperty("verdict").GetString());
                Assert.Contains("does not agree with the values of the read-back", report.RootElement.GetProperty("reason").GetString(), StringComparison.Ordinal);
            }
        }

        [Fact]
        public void A_read_back_with_empty_values_does_not_approve_a_generated_dump_that_has_a_model()
        {
            var empty = new JsonObject
            {
                ["nodes"] = new JsonArray(),
                ["members"] = new JsonArray(),
                ["loads"] = new JsonArray(),
                ["loadCase"] = null,
            };

            var result = Compare(Package("pkg", values: empty), Dump("g.json", D.Valid()), Dump("m.json", H.ManualDump()));

            Assert.Equal(RamToolExitCodes.Rejected, result.Code);
            Assert.Contains("REJECTED", result.Output, StringComparison.Ordinal);
        }

        [Fact]
        public void Values_with_an_unknown_key_are_an_invalid_read_back_and_invalid_usage()
        {
            var values = H.SampleValues();
            values["extra"] = 1;

            var result = Compare(Package("pkg", values: values), Dump("g.json", D.Valid()), Dump("m.json", H.ManualDump()));

            Assert.Equal(RamToolExitCodes.InvalidUsage, result.Code);
            Assert.Contains("read-back is not valid", result.Error, StringComparison.Ordinal);
        }

        // ---------------------------------------------------------------- A65-F4-04

        private string Injected(string fault, string status, string[] differences)
        {
            var source = Path_("source-" + fault);
            var fingerprint = H.WritePackage(source);
            var target = Path_("injected-" + fault);

            Assert.Equal(
                RamToolExitCodes.Pass,
                Run("inject", "--package", source, "--fault", fault, "--out", target, "--embed-folder", EmbeddedInjectFolder).Code);

            File.WriteAllText(
                Path.Combine(target, RamBuildManifest.ReadbackFileName),
                H.Readback(status, fingerprint, differences, H.SampleValues()).ToJsonString(),
                new UTF8Encoding(false));
            return target;
        }

        [Theory]
        [InlineData("unknown-section", "member MC1: section 'W 12 INEXISTENTE-99' is not in the database")]
        [InlineData("inverted-axes", "member MC1: local axis 2 (0.0, 0.0, -1.0)")]
        [InlineData("wrong-version", "gate: package version is '0.1.0', expected '999.0.0'")]
        [InlineData("missing-core-function", "gate: core function missing: rackcad_nonexistent_function")]
        public void A_fault_injection_package_with_a_failed_read_back_is_rejected_citing_the_injection(string fault, string difference)
        {
            var package = Injected(fault, RamReadbackReader.StatusFailed, new[] { difference });

            var result = Compare(package, Dump("g.json", D.Valid()), Dump("m.json", H.ManualDump()));

            Assert.Equal(RamToolExitCodes.Rejected, result.Code);

            using (var report = Report(result.Output))
            {
                var reason = report.RootElement.GetProperty("reason").GetString();

                Assert.Equal("REJECTED", report.RootElement.GetProperty("verdict").GetString());
                Assert.Contains("fault injection", reason, StringComparison.Ordinal);
                Assert.Contains(fault, reason, StringComparison.Ordinal);
                Assert.Contains(difference, reason, StringComparison.Ordinal);
                Assert.DoesNotContain(report.RootElement.GetProperty("rows").EnumerateArray(), row => row.GetProperty("verdict").GetString() == "PASS");
            }
        }

        [Theory]
        [InlineData("unknown-section")]
        [InlineData("inverted-axes")]
        [InlineData("wrong-version")]
        [InlineData("missing-core-function")]
        public void A_fault_injection_package_whose_read_back_did_not_fail_is_rejected_too_because_it_can_never_be_approved(string fault)
        {
            var package = Injected(fault, RamReadbackReader.StatusPassed, new string[0]);

            var result = Compare(package, Dump("g.json", D.Valid()), Dump("m.json", H.ManualDump()));

            Assert.Equal(RamToolExitCodes.Rejected, result.Code);

            using (var report = Report(result.Output))
            {
                var reason = report.RootElement.GetProperty("reason").GetString();

                Assert.Equal("REJECTED", report.RootElement.GetProperty("verdict").GetString());
                Assert.Contains("cannot be approved", reason, StringComparison.Ordinal);
                Assert.Contains(fault, reason, StringComparison.Ordinal);
            }
        }

        [Fact]
        public void The_report_of_an_injection_carries_the_inputs()
        {
            var package = Injected("wrong-version", RamReadbackReader.StatusFailed, new[] { "gate: package version is '0.1.0'" });
            var generated = Dump("g.json", D.Valid());

            var result = Compare(package, generated, Dump("m.json", H.ManualDump()));

            using (var report = Report(result.Output))
            {
                var inputs = report.RootElement.GetProperty("inputs");

                Assert.Equal(H.Sha256(File.ReadAllBytes(generated)), inputs.GetProperty("generatedDumpSha256").GetString());
                Assert.Equal(H.Sha256(File.ReadAllBytes(Path.Combine(package, RamBuildPackage.ManifestFileName))), inputs.GetProperty("manifestSha256").GetString());
                Assert.False(string.IsNullOrWhiteSpace(inputs.GetProperty("manifestFingerprint").GetString()));
            }
        }

        [Fact]
        public void A_fault_injection_package_with_an_unreadable_read_back_is_invalid_usage()
        {
            var package = Injected("wrong-version", RamReadbackReader.StatusFailed, new[] { "gate: x" });
            File.WriteAllText(Path.Combine(package, RamBuildManifest.ReadbackFileName), "{}", new UTF8Encoding(false));

            var result = Compare(package, Dump("g.json", D.Valid()), Dump("m.json", H.ManualDump()));

            Assert.Equal(RamToolExitCodes.InvalidUsage, result.Code);
        }

        [Fact]
        public void A_package_without_the_mark_is_not_taken_for_an_injection()
        {
            var result = Compare(Package("pkg"), Dump("g.json", D.Valid()), Dump("m.json", H.ManualDump()));

            Assert.Equal(RamToolExitCodes.Pass, result.Code);
            Assert.DoesNotContain("fault injection", result.Output, StringComparison.Ordinal);
        }

        [Fact]
        public void The_readers_of_the_product_do_not_know_the_mark_and_still_refuse_the_manifest_of_an_injection()
        {
            var package = Injected("wrong-version", RamReadbackReader.StatusFailed, new[] { "gate: x" });

            var read = RamBuildManifestReader.Read(File.ReadAllText(Path.Combine(package, RamBuildPackage.ManifestFileName)));

            Assert.NotEqual(StructuralExportOutcome.Valid, read.Outcome);
            Assert.Contains(read.Diagnostics, d => d.Code == RamDumpDiagnosticCodes.ManifestUnknownProperty);
        }

        [Fact]
        public void The_mark_is_recognised_only_in_the_tool_and_not_in_the_product()
        {
            var offenders = StructuralAnalysisGuards.Files("src").Where(f => f.Source.Contains(RamToolInjection.Marker, StringComparison.OrdinalIgnoreCase)).Select(f => f.Path).ToList();
            var tool = File.ReadAllText(Path.Combine(StructuralAnalysisCantileverFixtures.RepoRoot(), "tools", "RackCad.StructuralAnalysis.RamTool", "RamToolCompare.cs"));

            Assert.Empty(offenders);
            Assert.Contains("RamToolInjection.Marker", tool, StringComparison.Ordinal);
        }
    }
}
