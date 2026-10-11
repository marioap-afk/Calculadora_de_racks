using System;
using System.Collections.Generic;

namespace RackCad.Application.StructuralAnalysis.Providers.RamElements
{
    /// <summary>The payload of the dump script (SKELETON).</summary>
    public static class RamDumpPayload
    {
        public const string Schema = "rackcad-ram-dump-payload/v1";

        public static IReadOnlyList<string> CoreFunctions { get; } = new string[0];

        public static string Create(string adapterVersion, string dumpFolder)
        {
            throw new NotImplementedException();
        }
    }
}
