using System;

namespace RackCad.Application.StructuralAnalysis.Providers.RamElements
{
    /// <summary>The fixed text of the build script (I-65 D-09 rules 1-10). RED skeleton: it compiles and does nothing yet.</summary>
    public static class RamBuildScriptTemplate
    {
        public const string PayloadMarker = "";

        public const string PayloadLinePrefix = "";

        public static string Text { get; } = string.Empty;

        public static string Render(string payloadJson) => throw new NotImplementedException();

        public static string Escape(string text) => throw new NotImplementedException();
    }
}
