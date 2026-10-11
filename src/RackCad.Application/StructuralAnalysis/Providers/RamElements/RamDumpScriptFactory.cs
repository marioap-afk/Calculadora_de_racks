using System;
using System.Collections.Generic;

namespace RackCad.Application.StructuralAnalysis.Providers.RamElements
{
    /// <summary>The dump script ready to be published (SKELETON).</summary>
    public sealed class RamDumpScript
    {
        public const string ScriptFileName = "rackcad-ram-dump.py";

        public string Text { get; }

        public string DumpFolder { get; }

        public StructuralPackageFile ToPackageFile()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>The result of creating the dump script (SKELETON).</summary>
    public sealed class RamDumpScriptResult
    {
        public StructuralExportOutcome Outcome { get; }

        public IReadOnlyList<StructuralDiagnostic> Diagnostics { get; }

        public RamDumpScript Script { get; }
    }

    /// <summary>Creates the dump script (SKELETON).</summary>
    public static class RamDumpScriptFactory
    {
        public static RamDumpScriptResult Create(string adapterVersion, string dumpFolder)
        {
            throw new NotImplementedException();
        }
    }
}
