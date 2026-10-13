using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using RackCad.Application.StructuralAnalysis;
using RackCad.Application.StructuralAnalysis.Providers.RamElements;

namespace RackCad.StructuralAnalysis.RamTool
{
    /// <summary>
    /// <c>compare</c> (D-11, D-13): reads <c>manifest.json</c> and <c>ram-readback.json</c> of the package folder and the two dumps with the
    /// strict readers, calls the pure comparator, prints the report and, when asked, writes it to a NEW file. The exit code tells PASS, FAIL,
    /// INCOMPLETE and a rejection apart. The report compares the transfer; it does not validate the design.
    ///
    /// <para>
    /// The tool adds three guards the comparator cannot have: the two dumps must come from independent models (the same file, or the same
    /// bytes, is rejected); a package marked as a fault injection is never approved; and the options that declare combinations unread
    /// need the written decision of the engineer. The report carries the fingerprints and the SHA-256 of the four files it was made from.
    /// </para>
    /// </summary>
    internal static class RamToolCompare
    {
        private static readonly string[] Values = { "--package", "--generated", "--manual", "--report", "--combinations-decision" };
        private static readonly string[] Flags = { "--generated-combinations-unread", "--manual-combinations-unread" };

        public static int Run(string[] args, TextWriter output, TextWriter error)
        {
            var arguments = RamToolArguments.Parse(args, Values, Flags);

            if (arguments.Error != null)
            {
                error.WriteLine(arguments.Error);
                error.Write(RamToolCommands.Usage);
                return RamToolExitCodes.InvalidUsage;
            }

            var missing = arguments.Missing("--package", "--generated", "--manual");

            if (missing != null)
            {
                error.WriteLine("The option " + missing + " is required.");
                error.Write(RamToolCommands.Usage);
                return RamToolExitCodes.InvalidUsage;
            }

            var decision = arguments.Value("--combinations-decision");
            var combinationsUnread = arguments.Flag("--generated-combinations-unread") || arguments.Flag("--manual-combinations-unread");

            if ((combinationsUnread || decision != null) && string.IsNullOrWhiteSpace(decision))
            {
                error.WriteLine(
                    "Declaring combinations unread needs the decision of the engineer: give --combinations-decision <text> with a text that is not blank.");
                error.Write(RamToolCommands.Usage);
                return RamToolExitCodes.InvalidUsage;
            }

            var reportPath = arguments.Value("--report") == null ? null : Path.GetFullPath(arguments.Value("--report"));

            if (reportPath != null && (File.Exists(reportPath) || Directory.Exists(reportPath)))
            {
                error.WriteLine("The report '" + reportPath + "' already exists; nothing is overwritten.");
                return RamToolExitCodes.OutputRefused;
            }

            var package = Path.GetFullPath(arguments.Value("--package"));
            var generatedPath = Path.GetFullPath(arguments.Value("--generated"));
            var manualPath = Path.GetFullPath(arguments.Value("--manual"));

            var manifestText = RamToolFiles.ReadTextWithBytes(Path.Combine(package, RamBuildPackage.ManifestFileName), out var manifestBytes, out var problem);

            if (manifestText == null)
            {
                error.WriteLine(problem);
                return RamToolExitCodes.InvalidUsage;
            }

            var readbackText = RamToolFiles.ReadTextWithBytes(Path.Combine(package, RamBuildManifest.ReadbackFileName), out var readbackBytes, out problem);

            if (readbackText == null)
            {
                error.WriteLine(problem);
                return RamToolExitCodes.InvalidUsage;
            }

            var generatedText = RamToolFiles.ReadTextWithBytes(generatedPath, out var generatedBytes, out problem);

            if (generatedText == null)
            {
                error.WriteLine(problem);
                return RamToolExitCodes.InvalidUsage;
            }

            var manualText = RamToolFiles.ReadTextWithBytes(manualPath, out var manualBytes, out problem);

            if (manualText == null)
            {
                error.WriteLine(problem);
                return RamToolExitCodes.InvalidUsage;
            }

            var hashes = new Hashes(Sha256(manifestBytes), Sha256(readbackBytes), Sha256(generatedBytes), Sha256(manualBytes));
            StructuralComparisonReport report;

            var injection = InjectionKind(manifestText);

            if (injection != null)
            {
                // A65-F4-04: a package marked as a fault injection is never approved; its read-back is evaluated only to say why.
                var injected = RamReadbackReader.Read(readbackText);

                if (injected.Outcome != StructuralExportOutcome.Valid)
                {
                    error.WriteLine("The read-back is not valid: " + RamToolFiles.Describe(injected.Diagnostics));
                    return RamToolExitCodes.InvalidUsage;
                }

                var reason = injected.Transfer.Passed
                    ? "The package is a fault injection (" + injection + "): a fault injection package cannot be approved, whatever its read-back says."
                    : "The package is a fault injection (" + injection + "): its read-back is not approved: "
                      + (injected.Transfer.Differences.Count == 0 ? "no detail" : string.Join("; ", injected.Transfer.Differences));

                report = StructuralReadbackComparison.Rejected(reason)
                    .WithInputs(hashes.Inputs(ManifestFingerprintOf(manifestText), injected.Transfer.AnalysisInputFingerprint));

                return Finish(report, decision, reportPath, output, error);
            }

            var manifest = RamBuildManifestReader.Read(manifestText);

            if (manifest.Outcome != StructuralExportOutcome.Valid)
            {
                error.WriteLine("The manifest is not valid: " + RamToolFiles.Describe(manifest.Diagnostics));
                return RamToolExitCodes.InvalidUsage;
            }

            var readback = RamReadbackReader.Read(readbackText);

            if (readback.Outcome != StructuralExportOutcome.Valid)
            {
                error.WriteLine("The read-back is not valid: " + RamToolFiles.Describe(readback.Diagnostics));
                return RamToolExitCodes.InvalidUsage;
            }

            var inputs = hashes.Inputs(manifest.Provenance.AnalysisInputFingerprint, readback.Transfer.AnalysisInputFingerprint);

            // A65-F4-03: the manual dump must come from an independent model, not from the generated one
            if (string.Equals(generatedPath, manualPath, StringComparison.Ordinal) || string.Equals(hashes.Generated, hashes.Manual, StringComparison.Ordinal))
            {
                report = StructuralReadbackComparison
                    .Rejected(
                        "The generated and the manual dump are the same input (the same path or the same SHA-256 " + hashes.Generated
                        + "): the manual dump must come from an independent model.")
                    .WithInputs(inputs);

                return Finish(report, decision, reportPath, output, error);
            }

            var generated = RamDumpReader.Read(generatedText);
            var manual = RamDumpReader.Read(manualText);

            var failedDump = Failed("generated", generated) ?? Failed("manual", manual);

            if (failedDump != null)
            {
                report = StructuralReadbackComparison.Rejected(failedDump);
            }
            else if (generated.Outcome != StructuralExportOutcome.Valid || manual.Outcome != StructuralExportOutcome.Valid)
            {
                error.WriteLine(
                    "A dump is not valid: " + RamToolFiles.Describe(generated.Diagnostics.Concat(manual.Diagnostics)));
                return RamToolExitCodes.InvalidUsage;
            }
            else
            {
                report = StructuralReadbackComparison.CompareReadbacks(
                    generated.Readback,
                    manual.Readback,
                    readback.Transfer,
                    manifest.Provenance.AnalysisInputFingerprint,
                    new StructuralComparisonOptions(
                        !arguments.Flag("--generated-combinations-unread"),
                        !arguments.Flag("--manual-combinations-unread")));
            }

            return Finish(report.WithInputs(inputs), decision, reportPath, output, error);
        }

        /// <summary>Writes the decision into the report, prints it, writes it to the new file if asked and gives the exit code of its outcome.</summary>
        private static int Finish(StructuralComparisonReport report, string decision, string reportPath, TextWriter output, TextWriter error)
        {
            if (decision != null)
            {
                report = report.WithCombinationsDecision(decision);
            }

            var json = report.ToJson();
            output.Write(json);

            if (reportPath != null && !RamToolFiles.TryCreate(reportPath, json, out var problem))
            {
                error.WriteLine(problem);
                return RamToolExitCodes.OutputRefused;
            }

            switch (report.Outcome)
            {
                case StructuralComparisonOutcome.Pass:
                    return RamToolExitCodes.Pass;
                case StructuralComparisonOutcome.Fail:
                    return RamToolExitCodes.Fail;
                case StructuralComparisonOutcome.Incomplete:
                    return RamToolExitCodes.Incomplete;
                default:
                    return RamToolExitCodes.Rejected;
            }
        }

        /// <summary>The reason when a dump is in a failed state (its own status), or <c>null</c>.</summary>
        private static string Failed(string which, RamDumpReadResult dump)
        {
            return dump.Diagnostics.Any(d => string.Equals(d.Code, RamDumpDiagnosticCodes.DumpFailed, StringComparison.Ordinal))
                ? "The " + which + " dump is in a failed state: " + RamToolFiles.Describe(dump.Diagnostics)
                : null;
        }

        /// <summary>The kind of fault when the manifest carries the mark of an injection (the product never writes it), or <c>null</c>.</summary>
        private static string InjectionKind(string manifestText)
        {
            try
            {
                using (var document = JsonDocument.Parse(manifestText))
                {
                    if (document.RootElement.ValueKind != JsonValueKind.Object || !document.RootElement.TryGetProperty(RamToolInjection.Marker, out var marker))
                    {
                        return null;
                    }

                    return marker.ValueKind == JsonValueKind.Object && marker.TryGetProperty("kind", out var kind) && kind.ValueKind == JsonValueKind.String
                        ? kind.GetString()
                        : "unknown kind";
                }
            }
            catch (JsonException)
            {
                // not a document: the strict reader of the manifest reports it
                return null;
            }
        }

        /// <summary>The fingerprint a manifest of an injection records, or <c>null</c> when it has none that looks like one.</summary>
        private static string ManifestFingerprintOf(string manifestText)
        {
            try
            {
                using (var document = JsonDocument.Parse(manifestText))
                {
                    return document.RootElement.TryGetProperty("analysisInputFingerprint", out var fingerprint) && fingerprint.ValueKind == JsonValueKind.String
                        ? fingerprint.GetString()
                        : null;
                }
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private static string Sha256(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

        private sealed class Hashes
        {
            private readonly string _manifest;
            private readonly string _readback;

            public Hashes(string manifest, string readback, string generated, string manual)
            {
                _manifest = manifest;
                _readback = readback;
                Generated = generated;
                Manual = manual;
            }

            public string Generated { get; }

            public string Manual { get; }

            public StructuralComparisonInputs Inputs(string manifestFingerprint, string readbackFingerprint) =>
                new StructuralComparisonInputs(manifestFingerprint, readbackFingerprint, _manifest, _readback, Generated, Manual);
        }
    }
}
