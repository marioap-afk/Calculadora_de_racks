using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace RackCad.Application.StructuralAnalysis.Providers.RamElements
{
    /// <summary>The dump script ready to be published: its text and the folder it embeds.</summary>
    public sealed class RamDumpScript
    {
        public const string ScriptFileName = "rackcad-ram-dump.py";

        internal RamDumpScript(string text, string dumpFolder)
        {
            Text = text;
            DumpFolder = dumpFolder;
        }

        public string Text { get; }

        /// <summary>The folder the script embeds: the one the dump is written to.</summary>
        public string DumpFolder { get; }

        /// <summary>The script as the file to publish: UTF-8 without a byte order mark, under its fixed name.</summary>
        public StructuralPackageFile ToPackageFile() => new StructuralPackageFile(ScriptFileName, new UTF8Encoding(false).GetBytes(Text));
    }

    /// <summary>The result of creating the dump script: the script when, and only when, the outcome is Valid. Never a partial script.</summary>
    public sealed class RamDumpScriptResult
    {
        internal RamDumpScriptResult(StructuralExportOutcome outcome, IReadOnlyList<StructuralDiagnostic> diagnostics, RamDumpScript script)
        {
            Outcome = outcome;
            Diagnostics = diagnostics;
            Script = script;
        }

        public StructuralExportOutcome Outcome { get; }

        public IReadOnlyList<StructuralDiagnostic> Diagnostics { get; }

        public RamDumpScript Script { get; }
    }

    /// <summary>
    /// <c>Create(adapterVersion, dumpFolder)</c> (I-65 D-11): the fixed dump script for a folder. It returns text and never touches the disk (D-01):
    /// publishing is the job of the caller. The folder must obey the SAME rule as the folder of a package (absolute, normalised, WINDOWS) and
    /// the version of the adapter must not be blank; anything else is a blocking diagnostic and there is NO script.
    /// </summary>
    public static class RamDumpScriptFactory
    {
        public static RamDumpScriptResult Create(string adapterVersion, string dumpFolder)
        {
            var diagnostics = new List<StructuralDiagnostic>();

            if (string.IsNullOrWhiteSpace(adapterVersion))
            {
                diagnostics.Add(StructuralDiagnostic.Blocking(RamPackageDiagnosticCodes.AdapterVersionInvalid, StructuralExportOutcome.Invalid, "adapterVersion"));
            }

            if (!RamBuildPackageFactory.IsAbsoluteNormalisedFolder(dumpFolder))
            {
                diagnostics.Add(StructuralDiagnostic.Blocking(RamPackageDiagnosticCodes.FolderInvalid, StructuralExportOutcome.Invalid, "dumpFolder"));
            }

            var outcome = StructuralModelResult.Aggregate(diagnostics);

            if (outcome != StructuralExportOutcome.Valid)
            {
                return new RamDumpScriptResult(outcome, new ReadOnlyCollection<StructuralDiagnostic>(diagnostics), null);
            }

            var script = new RamDumpScript(RamDumpScriptTemplate.Render(RamDumpPayload.Create(adapterVersion, dumpFolder)), dumpFolder);

            return new RamDumpScriptResult(outcome, new ReadOnlyCollection<StructuralDiagnostic>(diagnostics), script);
        }
    }
}
