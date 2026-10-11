using System;
using System.Collections.Generic;

namespace RackCad.Application.StructuralAnalysis.Providers.RamElements
{
    /// <summary>The result of reading the read-back of a build (SKELETON).</summary>
    public sealed class RamReadbackReadResult
    {
        public StructuralExportOutcome Outcome { get; }

        public IReadOnlyList<StructuralDiagnostic> Diagnostics { get; }

        public StructuralTransferReadback Transfer { get; }
    }

    /// <summary>The strict reader of the read-back of a build (SKELETON).</summary>
    public static class RamReadbackReader
    {
        public const string Schema = "rackcad-ram-readback/v1";

        public const string StatusPassed = "READBACK OK";

        public static readonly string StatusFailed = "FAILED " + (char)0x2014 + " DISCARD MODEL";

        public static RamReadbackReadResult Read(string json)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>The result of reading a manifest (SKELETON).</summary>
    public sealed class RamManifestReadResult
    {
        public StructuralExportOutcome Outcome { get; }

        public IReadOnlyList<StructuralDiagnostic> Diagnostics { get; }

        public StructuralPackageProvenance Provenance { get; }
    }

    /// <summary>The strict reader of the manifest (SKELETON).</summary>
    public static class RamBuildManifestReader
    {
        public static RamManifestReadResult Read(string json)
        {
            throw new NotImplementedException();
        }
    }
}
