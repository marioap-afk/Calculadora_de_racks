using System;

namespace RackCad.Application.StructuralAnalysis.Providers.RamElements
{
    /// <summary>The fixed text of the dump script (SKELETON).</summary>
    public static class RamDumpScriptTemplate
    {
        public const string PayloadMarker = "@@PAYLOAD@@";

        public const string PayloadLinePrefix = "PAYLOAD = \"";

        public const string StatusOk = "DUMP OK";

        public const string StatusFailed = "DUMP FAILED";

        public const string DumpFileName = "ram-dump.json";

        public static string Text { get; } = string.Empty;

        public static string Render(string payloadJson)
        {
            throw new NotImplementedException();
        }
    }
}
