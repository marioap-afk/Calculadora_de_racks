using System;
using System.IO;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// Source-characterization guards for the two blocking control-flow behaviours the pure <see cref="KindDispatch{T}"/>
    /// tests cannot observe directly: the Plugin references AutoCAD, so RackCad.Tests must not load it (ADR-0003).
    /// These read the Plugin <c>.cs</c> as TEXT (no assembly is loaded, no Autodesk dependency) and pin:
    /// (1) RACKBOMTOTAL preflights every placed rack and aborts the whole command on an unrecognized kind — never a
    /// partial BOM; (2) RACKLAYOUT gates on the kind UNCONDITIONALLY (linked AND independent) before opening the
    /// layout flow. The semantics of the preflight itself are covered purely by KindDispatchTests.TryResolveAll_*.
    /// </summary>
    public class KindHandlerGuardSourceTests
    {
        private static string ReadPluginSource(string fileName)
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "RackCad.sln")))
            {
                dir = dir.Parent;
            }

            Assert.True(dir != null, "Could not locate the repo root (RackCad.sln) from the test output directory.");
            var path = Path.Combine(dir.FullName, "src", "RackCad.Plugin", fileName);
            Assert.True(File.Exists(path), $"Plugin source not found: {path}");
            return File.ReadAllText(path);
        }

        [Fact]
        public void RackBomTotal_PreflightsEveryRack_AbortsOnUnrecognizedKind_NeverPartial()
        {
            var src = ReadPluginSource("RackInventarioCommands.BomTotal.cs");

            // Uses the batch preflight seam that resolves EVERY placed rack up front and aborts on the first miss...
            Assert.Contains("KindHandlerDispatch.TryResolveAll(", src);
            // ...so the old per-rack "resolve one, else continue" path is gone; the only skip left is the best-effort
            // null-BOM skip of a KNOWN handler whose payload is unreadable.
            Assert.DoesNotContain("TryResolve(editor, aggregate.Embed.Kind", src);
        }

        [Fact]
        public void RackLayout_GatesOnKindUnconditionally_BeforeOpeningTheFlow()
        {
            var src = ReadPluginSource("RackLayoutCommands.cs");

            var gate = src.IndexOf("KindHandlerDispatch.TryResolveIgnoreCase", StringComparison.Ordinal);
            Assert.True(gate >= 0, "RACKLAYOUT must gate on the kind handler.");

            var window = src.IndexOf("new RackWarehouseLayoutWindow", StringComparison.Ordinal);
            Assert.True(window >= 0, "RACKLAYOUT must open the layout window.");
            Assert.True(gate < window, "the kind gate must run BEFORE opening the layout window.");

            // The gate statement must NOT be conditioned on Independent — it applies to linked copies too.
            var lineStart = src.LastIndexOf('\n', gate);
            var gateLine = src.Substring(lineStart + 1, gate - lineStart - 1);
            Assert.DoesNotContain("Independent", gateLine);
        }
    }
}
