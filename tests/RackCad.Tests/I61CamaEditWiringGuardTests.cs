#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-61 OBL-P2 (D-1a): source guard over the text of <c>EditCama</c> in RackCamaCommands.cs. No suite loads the Plugin (ADR-0003),
    /// so the wiring is fixed on the source. The resolved name must be computed once with
    /// <c>EditedRackNameResolver.X(window.RackName, embed.Name)</c> (edited first, envelope second) and that same value must reach
    /// the payload and <c>RackViewBaseName.LinkedBase</c> of SyncName; <c>window.RackName</c> must not stay as a direct argument
    /// anywhere else. Each condition is shown able to fail with an in-memory mutation (of a conforming fixture and of the real text).
    /// </summary>
    public class I61CamaEditWiringGuardTests
    {
        private const string CamaCommandsPath = "src/RackCad.Plugin/RackCamaCommands.cs";

        private const string ConformingBody = @"{
            var window = new RackFlowBedWindow(canInsertInAutoCad: true);
            window.LoadExisting(config, embed.Id, embed.Name, sourceDesign);
            var resolvedName = EditedRackNameResolver.Resolve(window.RackName, embed.Name);
            var result = new FlowBedDrawService().RedrawInPlace(
                document, blockId, window.FlowBedToInsert,
                BuildCamaPayload(window.FlowBedToInsert, window.RackId, resolvedName, embed, sourceDesign));
            if (result != null && result.Success)
            {
                RackBlockRenamer.SyncName(document, blockId, RackViewBaseName.LinkedBase(resolvedName));
            }
        }";

        // ---- The real text ----

        [Fact]
        public void EditCama_UsesTheResolvedName_InPayloadAndSyncName()
        {
            var violations = Analyze(EditCamaBody(ReadCamaCommands()));

            Assert.Empty(violations);
        }

        [Fact]
        public void RealText_PayloadSiteBackToWindowRackName_IsDetected()
        {
            var body = EditCamaBody(ReadCamaCommands());
            var mutated = Regex.Replace(body, @"(BuildCamaPayload\(window\.FlowBedToInsert,\s*window\.RackId,\s*)\w+(\s*,)", "$1window.RackName$2");

            Assert.NotEqual(body, mutated);
            var violations = Analyze(mutated);
            Assert.Contains("A", violations);
            Assert.Contains("C", violations);
        }

        [Fact]
        public void RealText_SyncNameSiteBackToWindowRackName_IsDetected()
        {
            var body = EditCamaBody(ReadCamaCommands());
            var mutated = Regex.Replace(body, @"(RackViewBaseName\.LinkedBase\()\w+(\))", "$1window.RackName$2");

            Assert.NotEqual(body, mutated);
            var violations = Analyze(mutated);
            Assert.Contains("B", violations);
            Assert.Contains("C", violations);
        }

        [Fact]
        public void RealText_InvertedResolverArguments_AreDetected()
        {
            var body = EditCamaBody(ReadCamaCommands());
            var mutated = Regex.Replace(body, @"(EditedRackNameResolver\s*\.\s*\w+\s*\()\s*window\.RackName\s*,\s*embed\.Name\s*(\))", "$1embed.Name, window.RackName$2");

            Assert.NotEqual(body, mutated);
            Assert.Contains("D", Analyze(mutated));
        }

        // ---- The guard able to fail: mutations of a conforming fixture ----

        [Fact]
        public void Fixture_Conforming_HasNoViolations()
        {
            Assert.Empty(Analyze(ConformingBody));
        }

        [Fact]
        public void Fixture_PayloadSiteWithWindowRackName_ViolatesA_AndC()
        {
            var mutated = ConformingBody.Replace("window.RackId, resolvedName,", "window.RackId, window.RackName,");

            Assert.NotEqual(ConformingBody, mutated);
            var violations = Analyze(mutated);
            Assert.Contains("A", violations);
            Assert.Contains("C", violations);
            Assert.DoesNotContain("B", violations);
        }

        [Fact]
        public void Fixture_SyncNameSiteWithWindowRackName_ViolatesB_AndC()
        {
            var mutated = ConformingBody.Replace("LinkedBase(resolvedName)", "LinkedBase(window.RackName)");

            Assert.NotEqual(ConformingBody, mutated);
            var violations = Analyze(mutated);
            Assert.Contains("B", violations);
            Assert.Contains("C", violations);
            Assert.DoesNotContain("A", violations);
        }

        [Fact]
        public void Fixture_SyncNameWithADifferentValueThanThePayload_ViolatesB()
        {
            var mutated = ConformingBody.Replace("LinkedBase(resolvedName)", "LinkedBase(embed.Name)");

            Assert.NotEqual(ConformingBody, mutated);
            Assert.Contains("B", Analyze(mutated));
        }

        [Fact]
        public void Fixture_InvertedResolverArguments_ViolateD()
        {
            var mutated = ConformingBody.Replace("Resolve(window.RackName, embed.Name)", "Resolve(embed.Name, window.RackName)");

            Assert.NotEqual(ConformingBody, mutated);
            Assert.Contains("D", Analyze(mutated));
        }

        [Fact]
        public void Fixture_WithoutTheResolverCall_ViolatesAandB()
        {
            var mutated = ConformingBody.Replace("var resolvedName = EditedRackNameResolver.Resolve(window.RackName, embed.Name);", string.Empty);

            Assert.NotEqual(ConformingBody, mutated);
            var violations = Analyze(mutated);
            Assert.Contains("A", violations);
            Assert.Contains("B", violations);
        }

        // ---- Analysis ----

        /// <summary>Violation codes: A payload arg is not the resolved value; B SyncName does not get that value via LinkedBase;
        /// C window.RackName stays as a direct argument; D editado/sobre arguments inverted.</summary>
        private static HashSet<string> Analyze(string body)
        {
            var violations = new HashSet<string>();

            var call = Regex.Match(body,
                @"(?:var|string)\s+(?<v>\w+)\s*=\s*EditedRackNameResolver\s*\.\s*\w+\s*\(\s*(?<first>[\w.]+)\s*,\s*(?<second>[\w.]+)\s*\)\s*;");

            string? resolved = null;
            var outside = body;
            if (call.Success)
            {
                resolved = call.Groups["v"].Value;
                if (call.Groups["first"].Value != "window.RackName" || call.Groups["second"].Value != "embed.Name")
                {
                    violations.Add("D");
                }

                outside = body.Remove(call.Index, call.Length);
            }

            var payload = Regex.Match(body, @"BuildCamaPayload\(\s*window\.FlowBedToInsert\s*,\s*window\.RackId\s*,\s*(?<arg>[\w.]+)\s*,");
            if (!payload.Success || resolved == null || payload.Groups["arg"].Value != resolved)
            {
                violations.Add("A");
            }

            var sync = Regex.Match(body, @"RackBlockRenamer\.SyncName\([^;]*?RackViewBaseName\.LinkedBase\(\s*(?<arg>[\w.]+)\s*\)");
            if (!sync.Success || resolved == null || sync.Groups["arg"].Value != resolved)
            {
                violations.Add("B");
            }

            if (outside.Contains("window.RackName"))
            {
                violations.Add("C");
            }

            return violations;
        }

        private static string EditCamaBody(string source)
        {
            var start = source.IndexOf("internal static void EditCama(", StringComparison.Ordinal);
            Assert.True(start >= 0, "EditCama not found in " + CamaCommandsPath);

            var open = source.IndexOf('{', start);
            Assert.True(open >= 0, "EditCama has no body.");

            var depth = 0;
            for (var i = open; i < source.Length; i++)
            {
                if (source[i] == '{')
                {
                    depth++;
                }
                else if (source[i] == '}')
                {
                    depth--;
                    if (depth == 0)
                    {
                        return source.Substring(open, i - open + 1);
                    }
                }
            }

            throw new InvalidOperationException("EditCama body is not balanced.");
        }

        private static string ReadCamaCommands()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "RackCad.sln")))
            {
                dir = dir.Parent;
            }

            Assert.NotNull(dir);
            return File.ReadAllText(Path.Combine(dir!.FullName, CamaCommandsPath.Replace('/', Path.DirectorySeparatorChar)));
        }
    }
}
