using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using RackCad.Application.StructuralAnalysis;
using RackCad.Application.StructuralAnalysis.Providers.RamElements;
using RackCad.StructuralAnalysis.RamTool;
using Xunit;
using C = RackCad.Tests.StructuralAnalysisCantileverFixtures;
using D = RackCad.Tests.StructuralAnalysisRamElementsDumpSupport;
using I = RackCad.Tests.StructuralAnalysisRamElementsPackageInputs;
using S = RackCad.Tests.StructuralAnalysisComparisonSupport;

namespace RackCad.Tests
{
    /// <summary>
    /// I-65 F4-T4 — the console tool (D-11, CD-I65-F4-09) on temporary files: the exit codes, no overwriting, the dump script, the comparison
    /// and the fault injection with ONE semantic mutation. The tests call the classes of the tool, never a process, and run nothing external.
    /// </summary>
    public sealed class StructuralAnalysisComparisonToolTests : IDisposable
    {
        private const string EmbeddedDumpFolder = "C:\\RackCadDumps\\Dump-Tool-1";
        private const string EmbeddedInjectFolder = "C:\\RackCadInjected\\Case-1";

        private readonly string _temp = Path.Combine(Path.GetTempPath(), "rackcad-t4-" + Guid.NewGuid().ToString("N"));

        public StructuralAnalysisComparisonToolTests()
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

        private (int Code, string Output, string Error) Run(params string[] args)
        {
            var output = new StringWriter();
            var error = new StringWriter();
            var code = RamToolCommands.Run(args, output, error);

            return (code, output.ToString(), error.ToString());
        }

        private string Path_(string name) => Path.Combine(_temp, name);

        /// <summary>Writes a real package (manifest, script, loads) and a read-back with the given content into a folder of the temp area.</summary>
        private string WritePackage(string name, string readbackStatus = null, string readbackFingerprint = null, string[] differences = null)
        {
            var package = I.Package();
            var folder = Path_(name);
            Directory.CreateDirectory(folder);

            foreach (var file in package.Files)
            {
                File.WriteAllBytes(Path.Combine(folder, file.Name), file.GetContent());
            }

            var readback = new JsonObject
            {
                ["schema"] = RamReadbackReader.Schema,
                ["status"] = readbackStatus ?? RamReadbackReader.StatusPassed,
                ["adapterVersion"] = I.AdapterVersion,
                ["analysisInputFingerprint"] = readbackFingerprint ?? package.AnalysisInputFingerprint,
                ["differences"] = new JsonArray((differences ?? new string[0]).Select(d => (JsonNode)d).ToArray()),
                ["values"] = new JsonObject(),
            };

            File.WriteAllText(Path.Combine(folder, RamBuildManifest.ReadbackFileName), readback.ToJsonString(), new UTF8Encoding(false));
            return folder;
        }

        private string WriteDump(string name, JsonObject dump)
        {
            var path = Path_(name);
            File.WriteAllText(path, dump.ToJsonString(), new UTF8Encoding(false));
            return path;
        }

        private static string Sha256(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

        private static string FolderHash(string folder) =>
            string.Join("|", Directory.GetFiles(folder).OrderBy(f => f, StringComparer.Ordinal).Select(f => Path.GetFileName(f) + ":" + Sha256(File.ReadAllBytes(f))));

        // ---------------------------------------------------------------- usage and exit codes

        [Fact]
        public void The_exit_codes_are_distinct_and_pass_is_zero()
        {
            var codes = new[]
            {
                RamToolExitCodes.Pass, RamToolExitCodes.Fail, RamToolExitCodes.Rejected, RamToolExitCodes.InvalidUsage, RamToolExitCodes.OutputRefused,
            };

            Assert.Equal(0, RamToolExitCodes.Pass);
            Assert.Equal(codes.Length, codes.Distinct().Count());
        }

        [Fact]
        public void Without_arguments_the_tool_prints_its_usage_and_is_invalid_usage()
        {
            var result = Run();

            Assert.Equal(RamToolExitCodes.InvalidUsage, result.Code);

            foreach (var command in new[] { "dump-script", "compare", "inject" })
            {
                Assert.Contains(command, result.Error, StringComparison.Ordinal);
            }
        }

        [Fact]
        public void An_unknown_command_is_invalid_usage()
        {
            Assert.Equal(RamToolExitCodes.InvalidUsage, Run("analyze").Code);
        }

        // ---------------------------------------------------------------- dump-script

        [Fact]
        public void The_dump_script_is_published_in_a_new_folder_with_the_embedded_folder()
        {
            var folder = Path_("dump1");

            var result = Run("dump-script", "--out", folder, "--embed-folder", EmbeddedDumpFolder, "--adapter-version", I.AdapterVersion);

            Assert.Equal(RamToolExitCodes.Pass, result.Code);
            var script = File.ReadAllText(Path.Combine(folder, RamDumpScript.ScriptFileName), new UTF8Encoding(false, true));
            Assert.Equal(D.Script(I.AdapterVersion, EmbeddedDumpFolder), script);
            Assert.Single(Directory.GetFileSystemEntries(folder));
        }

        [Fact]
        public void The_dump_script_never_overwrites_an_existing_folder()
        {
            var folder = Path_("dump2");
            Directory.CreateDirectory(folder);
            File.WriteAllText(Path.Combine(folder, "keep.txt"), "original");

            var result = Run("dump-script", "--out", folder, "--embed-folder", EmbeddedDumpFolder, "--adapter-version", I.AdapterVersion);

            Assert.Equal(RamToolExitCodes.OutputRefused, result.Code);
            Assert.Equal("original", File.ReadAllText(Path.Combine(folder, "keep.txt")));
            Assert.Single(Directory.GetFileSystemEntries(folder));
        }

        [Fact]
        public void The_dump_script_refuses_an_embedded_folder_that_is_not_absolute_and_creates_nothing()
        {
            var folder = Path_("dump3");

            var result = Run("dump-script", "--out", folder, "--embed-folder", "relative\\folder", "--adapter-version", I.AdapterVersion);

            Assert.Equal(RamToolExitCodes.InvalidUsage, result.Code);
            Assert.False(Directory.Exists(folder));
        }

        [Fact]
        public void The_dump_script_without_a_version_is_invalid_usage()
        {
            Assert.Equal(RamToolExitCodes.InvalidUsage, Run("dump-script", "--out", Path_("dump4"), "--embed-folder", EmbeddedDumpFolder).Code);
        }

        // ---------------------------------------------------------------- compare

        [Fact]
        public void Two_equal_dumps_with_a_matching_package_pass_with_exit_code_zero()
        {
            var package = WritePackage("pkg-pass");
            var generated = WriteDump("generated.json", D.Valid());
            var manual = WriteDump("manual.json", D.Valid());

            var result = Run("compare", "--package", package, "--generated", generated, "--manual", manual);

            Assert.Equal(RamToolExitCodes.Pass, result.Code);
            using (var report = JsonDocument.Parse(result.Output))
            {
                Assert.Equal("PASS", report.RootElement.GetProperty("verdict").GetString());
            }
        }

        [Fact]
        public void Dumps_that_differ_fail_with_their_own_exit_code()
        {
            var package = WritePackage("pkg-fail");
            var changed = D.Valid();
            changed["members"].ObjectAt(0)["section"] = "W 14X30";

            var result = Run("compare", "--package", package, "--generated", WriteDump("g.json", D.Valid()), "--manual", WriteDump("m.json", changed));

            Assert.Equal(RamToolExitCodes.Fail, result.Code);
            using (var report = JsonDocument.Parse(result.Output))
            {
                Assert.Equal("FAIL", report.RootElement.GetProperty("verdict").GetString());
            }
        }

        [Fact]
        public void A_failed_read_back_is_a_rejection_with_its_own_exit_code()
        {
            var package = WritePackage("pkg-failed", RamReadbackReader.StatusFailed, differences: new[] { "members: expected 2 and read 1" });

            var result = Run("compare", "--package", package, "--generated", WriteDump("g.json", D.Valid()), "--manual", WriteDump("m.json", D.Valid()));

            Assert.Equal(RamToolExitCodes.Rejected, result.Code);
            Assert.Contains("REJECTED", result.Output, StringComparison.Ordinal);
        }

        [Fact]
        public void A_read_back_of_another_input_is_stale_and_rejected()
        {
            var package = WritePackage("pkg-stale", readbackFingerprint: StructuralAnalysisComparisonModel.OtherFingerprint);

            var result = Run("compare", "--package", package, "--generated", WriteDump("g.json", D.Valid()), "--manual", WriteDump("m.json", D.Valid()));

            Assert.Equal(RamToolExitCodes.Rejected, result.Code);
            Assert.Contains("STALE", result.Output, StringComparison.Ordinal);
        }

        [Fact]
        public void A_dump_in_a_failed_state_is_a_rejection()
        {
            var package = WritePackage("pkg-dumpfail");
            var failed = new JsonObject
            {
                ["schema"] = RamDumpReader.Schema,
                ["status"] = "FAILED",
                ["errors"] = new JsonArray("gate: the model is not empty"),
            };

            var result = Run("compare", "--package", package, "--generated", WriteDump("g.json", failed), "--manual", WriteDump("m.json", D.Valid()));

            Assert.Equal(RamToolExitCodes.Rejected, result.Code);
        }

        [Fact]
        public void The_report_file_is_new_and_equals_what_is_printed()
        {
            var package = WritePackage("pkg-report");
            var report = Path_("report.json");

            var result = Run("compare", "--package", package, "--generated", WriteDump("g.json", D.Valid()), "--manual", WriteDump("m.json", D.Valid()), "--report", report);

            Assert.Equal(RamToolExitCodes.Pass, result.Code);
            Assert.Equal(result.Output, File.ReadAllText(report));
        }

        [Fact]
        public void An_existing_report_file_is_never_overwritten()
        {
            var package = WritePackage("pkg-keep");
            var report = Path_("report-keep.json");
            File.WriteAllText(report, "original");

            var result = Run("compare", "--package", package, "--generated", WriteDump("g.json", D.Valid()), "--manual", WriteDump("m.json", D.Valid()), "--report", report);

            Assert.Equal(RamToolExitCodes.OutputRefused, result.Code);
            Assert.Equal("original", File.ReadAllText(report));
        }

        [Fact]
        public void Missing_arguments_and_missing_files_are_invalid_usage()
        {
            Assert.Equal(RamToolExitCodes.InvalidUsage, Run("compare", "--package", Path_("nothing")).Code);
            Assert.Equal(
                RamToolExitCodes.InvalidUsage,
                Run("compare", "--package", Path_("nothing"), "--generated", Path_("g.json"), "--manual", Path_("m.json")).Code);
        }

        // ---------------------------------------------------------------- inject

        [Theory]
        [InlineData("unknown-section", "$.members[0].section")]
        [InlineData("inverted-axes", "$.members[0].l2")]
        [InlineData("wrong-version", "$.capabilities.version")]
        [InlineData("missing-core-function", "$.capabilities.coreFunctions")]
        public void Each_injection_changes_one_semantic_value_of_the_decoded_payload_and_the_folder(string fault, string expectedPath)
        {
            var source = WritePackage("source-" + fault);
            var sourceHash = FolderHash(source);
            var target = Path_("injected-" + fault);

            var result = Run("inject", "--package", source, "--fault", fault, "--out", target, "--embed-folder", EmbeddedInjectFolder);

            Assert.True(result.Code == RamToolExitCodes.Pass, result.Error);
            Assert.Equal(sourceHash, FolderHash(source));

            var originalPayload = I.ExtractPayload(File.ReadAllText(Path.Combine(source, RamBuildPackage.ScriptFileName)));
            var injectedPayload = I.ExtractPayload(File.ReadAllText(Path.Combine(target, RamBuildPackage.ScriptFileName)));

            using (var original = JsonDocument.Parse(originalPayload))
            using (var injected = JsonDocument.Parse(injectedPayload))
            {
                var paths = S.Differences(original.RootElement, injected.RootElement)
                    .Select(path => Regex.Replace(path, @"\[\d+\]$", string.Empty))
                    .Distinct()
                    .OrderBy(path => path, StringComparer.Ordinal)
                    .ToList();

                Assert.Equal(new[] { "$.packageFolder", expectedPath }.OrderBy(p => p, StringComparer.Ordinal).ToList(), paths);
                Assert.Equal(EmbeddedInjectFolder, injected.RootElement.GetProperty("packageFolder").GetString());
            }
        }

        [Fact]
        public void An_unknown_section_keeps_the_format_with_a_space()
        {
            var source = WritePackage("source-space");
            var target = Path_("injected-space");

            Assert.Equal(RamToolExitCodes.Pass, Run("inject", "--package", source, "--fault", "unknown-section", "--out", target, "--embed-folder", EmbeddedInjectFolder).Code);

            using (var original = JsonDocument.Parse(I.ExtractPayload(File.ReadAllText(Path.Combine(source, RamBuildPackage.ScriptFileName)))))
            using (var injected = JsonDocument.Parse(I.ExtractPayload(File.ReadAllText(Path.Combine(target, RamBuildPackage.ScriptFileName)))))
            {
                var before = original.RootElement.GetProperty("members")[0].GetProperty("section").GetString();
                var after = injected.RootElement.GetProperty("members")[0].GetProperty("section").GetString();

                Assert.NotEqual(before, after);
                Assert.Contains(" ", after, StringComparison.Ordinal);
            }
        }

        [Fact]
        public void An_injected_package_is_a_new_folder_with_the_three_files_and_the_script_of_its_payload()
        {
            var source = WritePackage("source-files");
            var target = Path_("injected-files");

            Assert.Equal(RamToolExitCodes.Pass, Run("inject", "--package", source, "--fault", "wrong-version", "--out", target, "--embed-folder", EmbeddedInjectFolder).Code);

            Assert.Equal(
                new[] { RamBuildPackage.ManifestFileName, RamBuildPackage.ScriptFileName, RamBuildPackage.LoadFileName },
                Directory.GetFiles(target).Select(Path.GetFileName).OrderBy(n => n, StringComparer.Ordinal).ToArray());
            Assert.Equal(
                File.ReadAllBytes(Path.Combine(source, RamBuildPackage.LoadFileName)),
                File.ReadAllBytes(Path.Combine(target, RamBuildPackage.LoadFileName)));

            var script = File.ReadAllText(Path.Combine(target, RamBuildPackage.ScriptFileName));
            Assert.Equal(script, RamBuildScriptTemplate.Render(I.ExtractPayload(script)));
        }

        [Fact]
        public void The_manifest_of_an_injection_is_marked_with_the_mutation_and_the_hash_of_the_source()
        {
            var source = WritePackage("source-manifest");
            var target = Path_("injected-manifest");
            var sourceManifest = File.ReadAllBytes(Path.Combine(source, RamBuildPackage.ManifestFileName));

            Assert.Equal(RamToolExitCodes.Pass, Run("inject", "--package", source, "--fault", "inverted-axes", "--out", target, "--embed-folder", EmbeddedInjectFolder).Code);

            var injectedText = File.ReadAllText(Path.Combine(target, RamBuildPackage.ManifestFileName));
            Assert.DoesNotContain("\r", injectedText, StringComparison.Ordinal);

            using (var original = JsonDocument.Parse(sourceManifest))
            using (var injected = JsonDocument.Parse(injectedText))
            {
                var marker = injected.RootElement.GetProperty(RamToolInjection.Marker);
                Assert.Equal("inverted-axes", marker.GetProperty("kind").GetString());
                Assert.False(string.IsNullOrWhiteSpace(marker.GetProperty("mutation").GetString()));
                Assert.Equal(Sha256(sourceManifest), marker.GetProperty("sourcePackageSha256").GetString());
                Assert.Equal(EmbeddedInjectFolder, injected.RootElement.GetProperty("packageFolder").GetString());

                var script = File.ReadAllBytes(Path.Combine(target, RamBuildPackage.ScriptFileName));
                var scriptEntry = injected.RootElement.GetProperty("files").EnumerateArray().Single(f => f.GetProperty("name").GetString() == RamBuildPackage.ScriptFileName);
                Assert.Equal(Sha256(script), scriptEntry.GetProperty("sha256").GetString());

                var changed = S.Differences(original.RootElement, injected.RootElement).OrderBy(p => p, StringComparer.Ordinal).ToList();
                Assert.Equal(
                    new[] { "$." + RamToolInjection.Marker, "$.files[0].sha256", "$.packageFolder" }.OrderBy(p => p, StringComparer.Ordinal).ToArray(),
                    changed.ToArray());
            }
        }

        [Fact]
        public void The_product_cannot_read_the_manifest_of_an_injection()
        {
            var source = WritePackage("source-reader");
            var target = Path_("injected-reader");

            Assert.Equal(RamToolExitCodes.Pass, Run("inject", "--package", source, "--fault", "wrong-version", "--out", target, "--embed-folder", EmbeddedInjectFolder).Code);

            var read = RamBuildManifestReader.Read(File.ReadAllText(Path.Combine(target, RamBuildPackage.ManifestFileName)));

            Assert.NotEqual(StructuralExportOutcome.Valid, read.Outcome);
            Assert.Null(read.Provenance);
        }

        [Fact]
        public void An_injection_never_overwrites_an_existing_folder()
        {
            var source = WritePackage("source-exists");
            var target = Path_("injected-exists");
            Directory.CreateDirectory(target);
            File.WriteAllText(Path.Combine(target, "keep.txt"), "original");

            var result = Run("inject", "--package", source, "--fault", "wrong-version", "--out", target, "--embed-folder", EmbeddedInjectFolder);

            Assert.Equal(RamToolExitCodes.OutputRefused, result.Code);
            Assert.Equal("original", File.ReadAllText(Path.Combine(target, "keep.txt")));
            Assert.Single(Directory.GetFileSystemEntries(target));
        }

        [Fact]
        public void An_unknown_fault_or_an_invalid_package_is_invalid_usage_and_creates_nothing()
        {
            var source = WritePackage("source-invalid");
            var unknown = Path_("injected-unknown");
            var missing = Path_("injected-missing");

            Assert.Equal(RamToolExitCodes.InvalidUsage, Run("inject", "--package", source, "--fault", "explode", "--out", unknown, "--embed-folder", EmbeddedInjectFolder).Code);
            Assert.False(Directory.Exists(unknown));
            Assert.Equal(RamToolExitCodes.InvalidUsage, Run("inject", "--package", Path_("nothing"), "--fault", "wrong-version", "--out", missing, "--embed-folder", EmbeddedInjectFolder).Code);
            Assert.False(Directory.Exists(missing));
        }

        // ---------------------------------------------------------------- the tool is outside the product

        [Fact]
        public void The_product_does_not_contain_the_marker_of_an_injection()
        {
            var offenders = StructuralAnalysisGuards.Files("src").Where(f => f.Source.Contains(RamToolInjection.Marker, StringComparison.OrdinalIgnoreCase)).Select(f => f.Path).ToList();

            Assert.Empty(offenders);
        }

        [Fact]
        public void The_tool_is_an_executable_with_only_the_application_reference_and_no_package()
        {
            var project = File.ReadAllText(Path.Combine(C.RepoRoot(), "tools", "RackCad.StructuralAnalysis.RamTool", "RackCad.StructuralAnalysis.RamTool.csproj"));

            Assert.Contains("<OutputType>Exe</OutputType>", project, StringComparison.Ordinal);
            Assert.Contains("<TargetFramework>net8.0</TargetFramework>", project, StringComparison.Ordinal);
            Assert.DoesNotContain("<PackageReference", project, StringComparison.Ordinal);
            Assert.Single(Regex.Matches(project, "<ProjectReference "));
            Assert.Contains("RackCad.Application.csproj", project, StringComparison.Ordinal);
        }

        [Fact]
        public void The_tool_is_in_the_solution_under_tools_and_in_the_tests_project_but_not_in_the_plugin()
        {
            var root = C.RepoRoot();
            var solution = File.ReadAllText(Path.Combine(root, "RackCad.sln"));
            var tests = File.ReadAllText(Path.Combine(root, "tests", "RackCad.Tests", "RackCad.Tests.csproj"));
            var plugin = File.ReadAllText(Path.Combine(root, "src", "RackCad.Plugin", "RackCad.Plugin.csproj"));
            var guid = Regex.Match(solution, "\"RackCad\\.StructuralAnalysis\\.RamTool\", \"[^\"]+\", \"(\\{[0-9A-F-]+\\})\"");

            Assert.True(guid.Success, "La herramienta no esta en la solucion.");
            Assert.Matches("\\t" + Regex.Escape(guid.Groups[1].Value) + " = \\{AE66CB82-AE70-412D-B03B-38ACB3CA8D2F\\}", solution);
            Assert.Equal(1, Regex.Matches(tests, "RackCad\\.StructuralAnalysis\\.RamTool").Count / 2);
            Assert.DoesNotContain("StructuralAnalysis.RamTool", plugin, StringComparison.Ordinal);
        }
    }
}
