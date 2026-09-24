using System;
using System.IO;
using Xunit;

namespace RackCad.Tests
{
    public sealed class RackViewBatchDriverGuardTests
    {
        [Fact]
        public void Driver_delegates_transitions_to_G11_and_distinguishes_Autodesk_error()
        {
            var source = Read("src", "RackCad.Plugin", "Views", "RackViewBatchDriver.cs");
            var placement = Read("src", "RackCad.Plugin", "Views", "RackViewPlacement.cs");
            Assert.Contains("RackViewBatchPlan<TPrepared>.Execute", source);
            Assert.Contains("transactions.TopTransaction", source);
            Assert.Contains("RackSingleViewPlacementStatus.Stopped", source);
            Assert.Contains("PromptStatus.None", placement);
            Assert.Contains("PromptStatus.Cancel", placement);
            Assert.Contains("PROMPT_STATUS_", placement);
            Assert.DoesNotContain("enum RackViewBatchOutcome", source);
        }

        [Fact]
        public void I55_consumes_all_four_demonstrated_I58_comparators_without_local_canonicalization()
        {
            var source = Read("src", "RackCad.Plugin", "Views", "RackUnsupportedSiblingInsert.cs");
            Assert.Contains("RackAuthoredComparatorPorts.Dynamic()", source);
            Assert.Contains("RackAuthoredComparatorPorts.PushBack()", source);
            Assert.Contains("RackAuthoredComparatorPorts.Cantilever()", source);
            Assert.Contains("RackAuthoredComparatorPorts.Cabecera()", source);
            Assert.Contains("RackAuthoredComparisonOutcome.Single", source);
            Assert.DoesNotContain("Dynamic<object", source);
            Assert.DoesNotContain("JsonDocument", source);
            Assert.DoesNotContain("first", source.ToLowerInvariant());
        }

        [Fact]
        public void NonSelectiveBatchReusesTheAuthoredGateScanForRedrawMembership()
        {
            var gate = Read("src", "RackCad.Plugin", "Views", "RackUnsupportedSiblingInsert.cs");
            Assert.Equal(1, Count(gate, "RackSiblingScan.Capture("));
            Assert.Contains("snapshot.Membership.Members", gate);
            Assert.Equal(4, Count(string.Join("\n", new[]
            {
                Read("src", "RackCad.Plugin", "RackDinamicoCommands.cs"),
                Read("src", "RackCad.Plugin", "RackPushBackCommands.cs"),
                Read("src", "RackCad.Plugin", "RackCantileverCommands.cs"),
                Read("src", "RackCad.Plugin", "RackCabeceraCommands.cs")
            }), "authorized.Blocks"));
        }

        [Fact]
        public void Entry_paths_forward_ordered_Views_and_exclude_FlowBed()
        {
            var menu = Read("src", "RackCad.Plugin", "RackMenuCommands.cs");
            Assert.Contains("header.Views", menu);
            Assert.Contains("dynamic.Views", menu);
            Assert.Contains("selective.Views", menu);
            Assert.Contains("cantilever.Views", menu);
            Assert.Contains("pushBack.Views", menu);
            Assert.Equal(5, Count(menu, "RackViewBatchExecution.Run"));
            Assert.DoesNotContain("cama.Views", menu);
        }

        private static int Count(string source, string value)
            => source.Split(new[] { value }, StringSplitOptions.None).Length - 1;

        private static string Read(params string[] path)
        {
            var root = new DirectoryInfo(AppContext.BaseDirectory);
            while (root != null && !File.Exists(Path.Combine(root.FullName, "RackCad.sln"))) root = root.Parent;
            Assert.NotNull(root);
            return File.ReadAllText(Path.Combine(root.FullName, Path.Combine(path)));
        }
    }
}
