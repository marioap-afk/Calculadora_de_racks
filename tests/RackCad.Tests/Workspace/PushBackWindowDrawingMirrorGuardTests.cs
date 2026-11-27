using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// T5 pb, review of 24adfee7, point 5 (PB-S06): the drawing half of the window (<c>RackPushBackWindowDrawing.Apply</c>) is a COPY of the one
    /// of RACKEDITAR (<c>RackPushBackCommands.EditPushBack</c>), not an extraction, because the guards of F4B pin the body of EditPushBack. This
    /// guard ties the copy: from the preflight to the end, the two bodies are the same once comments and spaces go and the four declared
    /// differences are read the same way (the input of the window instead of the classic window, the returns of the counts, the prefix of the
    /// payload and the words of the final message). If somebody corrects one of the two, this fails until the other one follows.
    /// </summary>
    public sealed class PushBackWindowDrawingMirrorGuardTests
    {
        private const string Marker = "// --- PREFLIGHT";

        [Fact]
        public void TheWindowDrawsWithTheSameBodyAsRackEditarFromThePreflightToTheEnd()
        {
            var classic = Normalize(FromPreflight(Plugin("RackPushBackCommands.cs"), "internal static void EditPushBack("));
            var window = Normalize(FromPreflight(Plugin("RackPushBackWindowDrawing.cs"), "internal static (int Redrawn, int Erased) Apply("));
            Assert.True(window.EndsWith("return;", StringComparison.Ordinal), "the copy answers its counts at the end");
            window = window.Substring(0, window.Length - "return;".Length);
            Assert.Equal(classic, window);
        }

        [Fact]
        public void TheCopyChecksTheDescriptorOfAViewAsRackEditar()
        {
            Assert.Equal(
                Normalize(MethodBody(Plugin("RackPushBackCommands.cs"), "private static bool IsValidPushBackDescriptor(")),
                Normalize(MethodBody(Plugin("RackPushBackWindowDrawing.cs"), "private static bool IsValidPushBackDescriptor(")));
        }

        [Fact]
        public void TheGuardBitesWhenOnlyOneOfTheTwoChanges()
        {
            var source = Plugin("RackPushBackWindowDrawing.cs");
            var classic = Normalize(FromPreflight(Plugin("RackPushBackCommands.cs"), "internal static void EditPushBack("));
            foreach (var (from, to) in new[]
                     {
                         ("regen: false", "regen: true"),
                         ("RackCommandSupport.EraseViewBlocks(document, staleViewBlocks)", "0"),
                         ("RackUnitsGuard.WarnIfNotInches(document);", string.Empty),
                     })
            {
                Assert.Contains(from, source);
                var changed = Normalize(FromPreflight(source.Replace(from, to), "internal static (int Redrawn, int Erased) Apply("));
                Assert.NotEqual(classic, changed.Substring(0, Math.Max(0, changed.Length - "return;".Length)));
            }
        }

        // The body from the line of the marker of the preflight to the end of the method.
        private static string FromPreflight(string source, string signature)
        {
            var body = MethodBody(source, signature);
            var at = body.IndexOf(Marker, StringComparison.Ordinal);
            Assert.True(at >= 0, "the preflight is marked in " + signature);
            return body.Substring(at);
        }

        private static string MethodBody(string source, string signature)
        {
            var start = source.IndexOf(signature, StringComparison.Ordinal);
            Assert.True(start >= 0, signature);
            var open = source.IndexOf('{', start);
            var depth = 0;
            for (var i = open; i < source.Length; i++)
            {
                if (source[i] == '{') depth++;
                else if (source[i] == '}' && --depth == 0) return source.Substring(open + 1, i - open - 1);
            }

            throw new InvalidOperationException("Unbalanced body: " + signature);
        }

        // Comments and spaces go; the declared differences of the copy are read the same way.
        private static string Normalize(string body)
        {
            var text = Regex.Replace(body, @"//[^\r\n]*", string.Empty);
            text = Regex.Replace(text, @"\s+", string.Empty);
            text = text.Replace("window.UpdateOnly", "updateOnly")
                .Replace("window.InsertionRequest.Views", "insertViews")
                .Replace("RackPushBackCommands.BuildPushBackPayload(", "BuildPushBackPayload(")
                .Replace("actualizadodesdelaventanaRackCad;", "actualizado;");
            return Regex.Replace(text, @"return[^;]*;", "return;");
        }

        private static string Plugin(string file)
            => File.ReadAllText(Path.Combine(PilotKit.RepoRoot().FullName, "src", "RackCad.Plugin", file));
    }
}
