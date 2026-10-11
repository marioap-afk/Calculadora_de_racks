using System;
using System.IO;
using System.Linq;
using RackCad.Application.StructuralAnalysis;
using RackCad.Application.StructuralAnalysis.Providers.RamElements;

namespace RackCad.StructuralAnalysis.RamTool
{
    /// <summary>
    /// <c>compare</c> (D-11, D-13): reads <c>manifest.json</c> and <c>ram-readback.json</c> of the package folder and the two dumps with the
    /// strict readers, calls the pure comparator, prints the report and, when asked, writes it to a NEW file. The exit code tells PASS, FAIL
    /// and a rejection apart. The report compares the transfer; it does not validate the design.
    /// </summary>
    internal static class RamToolCompare
    {
        private static readonly string[] Values = { "--package", "--generated", "--manual", "--report" };
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

            var reportPath = arguments.Value("--report") == null ? null : Path.GetFullPath(arguments.Value("--report"));

            if (reportPath != null && (File.Exists(reportPath) || Directory.Exists(reportPath)))
            {
                error.WriteLine("The report '" + reportPath + "' already exists; nothing is overwritten.");
                return RamToolExitCodes.OutputRefused;
            }

            var package = Path.GetFullPath(arguments.Value("--package"));
            var manifestText = RamToolFiles.ReadText(Path.Combine(package, RamBuildPackage.ManifestFileName), out var problem);

            if (manifestText == null)
            {
                error.WriteLine(problem);
                return RamToolExitCodes.InvalidUsage;
            }

            var readbackText = RamToolFiles.ReadText(Path.Combine(package, RamBuildManifest.ReadbackFileName), out problem);

            if (readbackText == null)
            {
                error.WriteLine(problem);
                return RamToolExitCodes.InvalidUsage;
            }

            var generatedText = RamToolFiles.ReadText(Path.GetFullPath(arguments.Value("--generated")), out problem);

            if (generatedText == null)
            {
                error.WriteLine(problem);
                return RamToolExitCodes.InvalidUsage;
            }

            var manualText = RamToolFiles.ReadText(Path.GetFullPath(arguments.Value("--manual")), out problem);

            if (manualText == null)
            {
                error.WriteLine(problem);
                return RamToolExitCodes.InvalidUsage;
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

            var generated = RamDumpReader.Read(generatedText);
            var manual = RamDumpReader.Read(manualText);
            StructuralComparisonReport report;

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

            var json = report.ToJson();
            output.Write(json);

            if (reportPath != null && !RamToolFiles.TryCreate(reportPath, json, out problem))
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
    }
}
