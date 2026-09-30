using System;
using System.IO;
using System.Linq;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-60, obligations N-6 and N-7 (source guards: the Plugin needs AutoCAD). Every creation path of a NEW logical rack asks for its
    /// name before it serializes the envelope and the inner design, with the gate of its family; no edit, duplication, layout or
    /// projection path does; and the drawing scan only reads.
    /// </summary>
    public class I60RackNameWiringGuardTests
    {
        [Fact]
        public void I60_N6_SelectiveCreationResolvesTheNameBeforeSerializing()
        {
            var body = Body(Code("src/RackCad.Plugin/RackSelectivoCommands.cs"), "internal static void DrawSelectiveView(");

            var resolve = body.IndexOf("RackNewRackName.Resolve(", StringComparison.Ordinal);
            Assert.True(resolve >= 0, "DrawSelectiveView must resolve the name of the new rack");
            Assert.Contains("RackSystemKind.SelectiveRack", body);
            Assert.True(resolve < body.IndexOf("SerializeSelectiveDesign(", StringComparison.Ordinal));
        }

        [Theory]
        [InlineData("src/RackCad.Plugin/RackDinamicoCommands.cs", "internal static void DrawDynamicView(", "RackSystemKind.PalletFlow", "system.Name = rackName;")]
        [InlineData("src/RackCad.Plugin/RackPushBackCommands.cs", "internal static void DrawPushBackView(", "RackSystemKind.PushBack", "system.Name = rackName;")]
        [InlineData("src/RackCad.Plugin/RackCantileverCommands.cs", "internal static void DrawCantileverView(", "RackSystemKind.Cantilever", "BuildCantileverPayload(")]
        public void I60_N6_TheSharedDrawPathsNameOnlyANewRack(string file, string signature, string kind, string firstUse)
        {
            var body = Body(Code(file), signature);

            var resolve = body.IndexOf("RackNewRackName.Resolve(", StringComparison.Ordinal);
            Assert.True(resolve >= 0, file + " must resolve the name of a new rack");
            Assert.Contains(kind, body);
            // The RACKEDITAR sibling insert passes its source envelope: only a new rack (no source) is named.
            Assert.Contains("source == null", body.Substring(0, resolve + 1).Substring(Math.Max(0, resolve - 200)));
            Assert.True(resolve < body.IndexOf(firstUse, StringComparison.Ordinal), "resolve before " + firstUse);
        }

        [Fact]
        public void I60_N6_CabeceraCreationResolvesWithTheTemplateRule()
        {
            var body = Body(Code("src/RackCad.Plugin/RackCabeceraCommands.cs"), "internal static void DrawAndPlace(RackFrameConfiguration configuration");

            var resolve = body.IndexOf("RackNewRackName.Resolve(", StringComparison.Ordinal);
            Assert.True(resolve >= 0);
            Assert.Contains("RackSystemKind.Selective", body);
            Assert.True(resolve < body.IndexOf("BuildCabeceraPayload(", StringComparison.Ordinal));
        }

        [Fact]
        public void I60_N6_BothCamaCreationPathsResolveTheName()
        {
            var quick = Body(Code("src/RackCad.Plugin/RackCamaCommands.cs"), "public void QuickCama(");
            Assert.True(quick.IndexOf("RackNewRackName.Resolve(", StringComparison.Ordinal) >= 0);
            Assert.True(quick.IndexOf("RackNewRackName.Resolve(", StringComparison.Ordinal) < quick.IndexOf("BuildCamaPayload(", StringComparison.Ordinal));

            var menu = Code("src/RackCad.Plugin/RackMenuCommands.cs");
            var cama = menu.Substring(menu.IndexOf("case FlowBedInsertionRequest cama:", StringComparison.Ordinal));
            cama = cama.Substring(0, cama.IndexOf("break;", StringComparison.Ordinal));
            Assert.Contains("RackNewRackName.Resolve(", cama);
            Assert.Contains("RackSystemKind.Cama", cama);
        }

        [Fact]
        public void I60_N9_TheCabeceraInnerNameIsTheAssignedNameBeforeTheEnvelope()
        {
            var body = Body(Code("src/RackCad.Plugin/RackCabeceraCommands.cs"), "internal static void DrawAndPlace(RackFrameConfiguration configuration");

            var assign = body.IndexOf("configuration.Name = ", StringComparison.Ordinal);
            Assert.True(assign >= 0, "the cabecera must carry its assigned name in its configuration (RACKEDITAR reloads it)");
            Assert.True(assign < body.IndexOf("BuildCabeceraPayload(", StringComparison.Ordinal));
        }

        [Fact]
        public void I60_N9_ACantileverNamedAutomaticallyCarriesTheNameInItsDesign()
        {
            var body = Body(Code("src/RackCad.Plugin/RackCantileverCommands.cs"), "internal static void DrawCantileverView(");

            var assign = body.IndexOf("design.Name = ", StringComparison.Ordinal);
            Assert.True(assign >= 0);
            Assert.True(assign < body.IndexOf("BuildCantileverPayload(", StringComparison.Ordinal));
        }

        [Fact]
        public void I60_N9_TheCamaUsesOneNameForTheEnvelopeAndTheBlock()
        {
            var menu = Code("src/RackCad.Plugin/RackMenuCommands.cs");
            var cama = menu.Substring(menu.IndexOf("case FlowBedInsertionRequest cama:", StringComparison.Ordinal));
            cama = cama.Substring(0, cama.IndexOf("break;", StringComparison.Ordinal));
            // The request name is read once (by the resolution); both uses take the same local name.
            Assert.Equal(1, System.Text.RegularExpressions.Regex.Matches(cama, @"cama\.RackName").Count);

            var quick = Body(Code("src/RackCad.Plugin/RackCamaCommands.cs"), "public void QuickCama(");
            Assert.DoesNotContain("BuildCamaPayload(config, System.Guid.NewGuid().ToString(), null)", quick);
            Assert.Matches(new System.Text.RegularExpressions.Regex(@"DrawAndPlace\(document, config, payload, \w+\)"), quick);
        }

        [Theory]
        [InlineData("src/RackCad.Plugin/RackDuplicarCommands.cs")]
        [InlineData("src/RackCad.Plugin/RackLayoutCommands.cs")]
        [InlineData("src/RackCad.Plugin/RackLayoutCommands.Fill.cs")]
        [InlineData("src/RackCad.Plugin/RackProyectarCommands.cs")]
        [InlineData("src/RackCad.Plugin/RackInventarioCommands.cs")]
        [InlineData("src/RackCad.Plugin/RackInventarioCommands.BomTotal.cs")]
        public void I60_N6_NoDuplicationLayoutProjectionOrReportNamesARack(string file)
        {
            if (!File.Exists(Path.Combine(Root(), file))) return;
            Assert.DoesNotContain("RackNewRackName", Code(file));
        }

        [Theory]
        [InlineData("src/RackCad.Plugin/RackSelectivoCommands.cs", "EditSelective(")]
        [InlineData("src/RackCad.Plugin/RackDinamicoCommands.cs", "EditDynamic(")]
        [InlineData("src/RackCad.Plugin/RackPushBackCommands.cs", "EditPushBack(")]
        [InlineData("src/RackCad.Plugin/RackCantileverCommands.cs", "EditCantilever(")]
        [InlineData("src/RackCad.Plugin/RackCabeceraCommands.cs", "EditCabecera(")]
        [InlineData("src/RackCad.Plugin/RackCamaCommands.cs", "EditCama(")]
        public void I60_N6_NoEditPathNamesALegacyRack(string file, string edit)
        {
            var body = Body(Code(file), edit);

            Assert.DoesNotContain("RackNewRackName", body);
        }

        [Fact]
        public void I60_N7_TheScanOnlyReadsAndUsesThePureAuthority()
        {
            var helper = Code("src/RackCad.Plugin/Systems/Shared/RackNewRackName.cs");

            Assert.Contains("RackBlockFinder.ScanEnvelopes(", helper);
            Assert.Contains("RackLogicalNameAllocator.IsUnassigned(", helper);
            Assert.Contains("RackLogicalNameAllocator.Next(", helper);
            Assert.DoesNotContain("OpenMode.ForWrite", helper);
            Assert.DoesNotContain("RackBlockData.Write", helper);
            Assert.DoesNotContain("\"Selectivo", helper);
        }

        private static string Root()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "RackCad.sln"))) dir = dir.Parent;
            Assert.NotNull(dir);
            return dir.FullName;
        }

        private static string Code(string relative)
            => string.Join("\n", File.ReadAllLines(Path.Combine(Root(), relative.Replace('/', Path.DirectorySeparatorChar)))
                .Select(line => line.Contains("//") ? line.Substring(0, line.IndexOf("//", StringComparison.Ordinal)) : line));

        /// <summary>The body of a method, by brace matching from its signature.</summary>
        private static string Body(string source, string signature)
        {
            var at = source.IndexOf(signature, StringComparison.Ordinal);
            Assert.True(at >= 0, "signature not found: " + signature);
            var open = source.IndexOf('{', at);
            var depth = 0;
            for (var i = open; i < source.Length; i++)
            {
                if (source[i] == '{') depth++;
                else if (source[i] == '}' && --depth == 0) return source.Substring(at, i - at + 1);
            }

            return source.Substring(at);
        }
    }
}
