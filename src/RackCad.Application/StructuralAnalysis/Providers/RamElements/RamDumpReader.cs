using System;
using System.Collections.Generic;

namespace RackCad.Application.StructuralAnalysis.Providers.RamElements
{
    /// <summary>The result of reading a dump (SKELETON).</summary>
    public sealed class RamDumpReadResult
    {
        public StructuralExportOutcome Outcome { get; }

        public IReadOnlyList<StructuralDiagnostic> Diagnostics { get; }

        public StructuralModelReadback Readback { get; }
    }

    /// <summary>The strict reader of the dump (SKELETON).</summary>
    public static class RamDumpReader
    {
        public const string Schema = "rackcad-ram-dump/v1";

        public static RamDumpReadResult Read(string json)
        {
            throw new NotImplementedException();
        }
    }
}
