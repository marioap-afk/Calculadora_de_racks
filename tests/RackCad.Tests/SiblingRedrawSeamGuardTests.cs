using System;
using System.IO;
using Xunit;

namespace RackCad.Tests
{
    public class RackSiblingRedrawUnitsTests
    {
        [Fact]
        public void AdapterOwnsOneTransactionAndOneCommit()
        {
            var source = Source("SiblingRedrawTransaction.cs");
            Assert.Equal(1, Count(source, "StartTransaction("));
            Assert.Equal(1, Count(source, "Commit()"));
            Assert.DoesNotContain("Resolve(", source);
            Assert.DoesNotContain("Prepare(", source);
            Assert.DoesNotContain("RackId", source);
            Assert.DoesNotContain("StartOpenCloseTransaction", source);
            Assert.DoesNotContain("LockDocument", source);
            Assert.Contains("using (var transaction", source);
        }

        [Fact]
        public void PostContainsOneRegenAndMutateContainsNone()
        {
            var source = Source("SiblingRedrawTransaction.cs");
            Assert.Equal(1, Count(source, "Regen()"));
            var mutate = source.Substring(source.IndexOf(" Mutate(", StringComparison.Ordinal), source.IndexOf(" Post(", StringComparison.Ordinal) - source.IndexOf(" Mutate(", StringComparison.Ordinal));
            Assert.DoesNotContain("Regen", mutate);
        }

        private static string Source(string name) => File.ReadAllText(Path.Combine(Root(), "src", "RackCad.Plugin", "Systems", "Shared", name));
        private static string Root() { var d = new DirectoryInfo(AppContext.BaseDirectory); while (d != null && !File.Exists(Path.Combine(d.FullName, "RackCad.sln"))) d = d.Parent; Assert.NotNull(d); return d.FullName; }
        private static int Count(string text, string value) => (text.Length - text.Replace(value, string.Empty).Length) / value.Length;
    }

    public class DebugFaultInjectionGuardTests
    {
        [Fact]
        public void DebugFaultInjectionIsCompileGuardedAndReleaseBranchHasNoEnvironmentRead()
        {
            var path = Path.Combine(Root(), "src", "RackCad.Plugin", "Systems", "Shared", "SiblingRedrawDebugFaultInjection.cs");
            var source = File.ReadAllText(path);
            Assert.Contains("#if DEBUG", source);
            Assert.Contains("RACKCAD_DEBUG_FAIL_SIBLING_REDRAW_UNIT", source);
            Assert.DoesNotContain("#else", source);
            Assert.EndsWith("#endif\n", source.Replace("\r\n", "\n"));

            var transaction = File.ReadAllText(Path.Combine(Root(), "src", "RackCad.Plugin", "Systems", "Shared", "SiblingRedrawTransaction.cs"));
            var call = transaction.IndexOf("SiblingRedrawDebugFaultInjection.ThrowIfRequested", StringComparison.Ordinal);
            var guard = transaction.LastIndexOf("#if DEBUG", call, StringComparison.Ordinal);
            var end = transaction.IndexOf("#endif", call, StringComparison.Ordinal);
            Assert.True(guard >= 0 && end > call);
        }
        private static string Root() { var d = new DirectoryInfo(AppContext.BaseDirectory); while (d != null && !File.Exists(Path.Combine(d.FullName, "RackCad.sln"))) d = d.Parent; Assert.NotNull(d); return d.FullName; }
    }

    public class SiblingRedrawSeamGuardTests
    {
        [Fact]
        public void SeamRemainsUnwiredFromCommands()
        {
            var root = Root();
            foreach (var file in Directory.GetFiles(Path.Combine(root, "src", "RackCad.Plugin"), "*Commands*.cs", SearchOption.AllDirectories))
            {
                Assert.DoesNotContain("SiblingRedrawTransaction", File.ReadAllText(file));
            }
        }

        [Fact]
        public void AdapterDoesNotOwnPolicyPlanningOrIdentity()
        {
            var root = Path.Combine(Root(), "src", "RackCad.Plugin", "Systems", "Shared");
            var source = File.ReadAllText(Path.Combine(root, "SiblingRedrawUnits.cs")) + File.ReadAllText(Path.Combine(root, "SiblingRedrawTransaction.cs"));
            Assert.DoesNotContain("IRackResolvePort", source);
            Assert.DoesNotContain("RackProductPrepare", source);
            Assert.DoesNotContain("Guid.NewGuid", source);
            Assert.DoesNotContain("RackId =", source);
        }
        private static string Root() { var d = new DirectoryInfo(AppContext.BaseDirectory); while (d != null && !File.Exists(Path.Combine(d.FullName, "RackCad.sln"))) d = d.Parent; Assert.NotNull(d); return d.FullName; }
    }
}
