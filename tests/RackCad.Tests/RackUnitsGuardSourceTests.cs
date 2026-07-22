using System;
using System.IO;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// Source-characterization guards for the units guardrail (initiative I-05, audit D4). The Plugin references
    /// AutoCAD, so RackCad.Tests must not LOAD it (ADR-0003); these read the Plugin <c>.cs</c> as TEXT (no assembly is
    /// loaded, no Autodesk dependency) and pin the wiring that the pure <see cref="RackCad.Application.Drawing.DrawingUnitsAdvisory"/>
    /// tests cannot observe: (1) the guard is the SINGLE reader of <c>INSUNITS</c> and never assigns it; (2) every
    /// authorized insertion path calls it exactly as often as it inserts; (3) an in-place update (RACKEDITAR
    /// "Actualizar") does NOT warn while inserting a NEW view does (gated by <c>!window.UpdateOnly</c>, BEFORE the first
    /// redraw); (4) RACKLAYOUT/RACKRELLENAR warn BEFORE their functional prompts; (5) the guard touches no geometry.
    /// Same pattern as <see cref="KindHandlerGuardSourceTests"/>.
    /// </summary>
    public class RackUnitsGuardSourceTests
    {
        private const string GuardCall = "RackUnitsGuard.WarnIfNotInches";

        private static DirectoryInfo RepoRoot()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "RackCad.sln")))
            {
                dir = dir.Parent;
            }

            Assert.True(dir != null, "Could not locate the repo root (RackCad.sln) from the test output directory.");
            return dir;
        }

        private static string ReadPluginSource(string fileName)
        {
            var path = Path.Combine(RepoRoot().FullName, "src", "RackCad.Plugin", fileName);
            Assert.True(File.Exists(path), $"Plugin source not found: {path}");
            return File.ReadAllText(path);
        }

        private static int Count(string src, string needle)
        {
            var n = 0;
            for (var i = src.IndexOf(needle, StringComparison.Ordinal); i >= 0; i = src.IndexOf(needle, i + needle.Length, StringComparison.Ordinal))
            {
                n++;
            }

            return n;
        }

        /// <summary>First index of <paramref name="needle"/> at or after <paramref name="from"/>; asserts it exists.</summary>
        private static int IndexFrom(string src, string needle, int from, string because)
        {
            var i = src.IndexOf(needle, from, StringComparison.Ordinal);
            Assert.True(i >= 0, because);
            return i;
        }

        // ---- (1) The guard is the single INSUNITS reader, reads-not-writes, and touches no geometry ----------------

        [Fact]
        public void Guard_ReadsInsunits_NeverAssignsIt_AndDelegatesTheDecisionToApplication()
        {
            var src = ReadPluginSource("RackUnitsGuard.cs");

            Assert.Contains(".Insunits", src);                                   // reads INSUNITS from the Database
            Assert.DoesNotContain("Insunits =", src);                           // never assigns / changes it
            Assert.DoesNotContain("Insunits=", src);
            Assert.Contains("UnitsValue.Inches", src);                          // inches is the quiet unit
            Assert.Contains("UnitsValue.Undefined", src);                       // unitless is treated as non-inches
            Assert.Contains("DrawingUnitsAdvisory.RequiresInsertionAdvisory", src); // decision delegated to Application
        }

        [Fact]
        public void Guard_TouchesNoGeometry_NoConversionOrCoordinateChange()
        {
            var src = ReadPluginSource("RackUnitsGuard.cs");

            // The guard only reads a system variable and prints a message: no scaling, no transform, no drawing.
            // (The literal "25.4" is intentionally NOT forbidden: the doc-comment cites it to explain the scale error.)
            foreach (var forbidden in new[] { "ScaleFactors", "TransformBy", "Scale3d", "AppendEntity", "RedrawInPlace", "DrawAndPlace", ".Position" })
            {
                Assert.DoesNotContain(forbidden, src);
            }
        }

        [Fact]
        public void Guard_IsTheOnlyPluginFileThatReadsInsunits()
        {
            var pluginDir = Path.Combine(RepoRoot().FullName, "src", "RackCad.Plugin");
            foreach (var file in Directory.EnumerateFiles(pluginDir, "*.cs", SearchOption.AllDirectories))
            {
                if (file.Replace('\\', '/').Contains("/obj/"))
                {
                    continue; // generated
                }

                var isGuard = Path.GetFileName(file).Equals("RackUnitsGuard.cs", StringComparison.Ordinal);
                var readsInsunits = File.ReadAllText(file).Contains("Insunits");
                Assert.False(readsInsunits && !isGuard, $"Only RackUnitsGuard.cs may read INSUNITS, but {Path.GetFileName(file)} does.");
            }
        }

        // ---- (2) Every authorized insertion path is connected, exactly as often as it inserts ----------------------

        [Theory]
        [InlineData("RackMenuCommands.cs", 1)]        // RACKCAD menu (one point covers the 4 insertion kinds)
        [InlineData("RackSelectivoCommands.cs", 2)]   // RACKSELECTIVO (new) + EditSelective (insert-new-view)
        [InlineData("RackDinamicoCommands.cs", 2)]    // RACKSISTEMADINAMICO (new) + EditDynamic (insert-new-view)
        [InlineData("RackCamaCommands.cs", 1)]        // QUICKCAMA (new); EditCama never inserts -> no guard
        [InlineData("RackCabeceraCommands.cs", 3)]    // RACKCABECERA + QUICKCABECERA (new) + EditCabecera (insert-new-view)
        [InlineData("RackLayoutCommands.cs", 1)]      // RACKLAYOUT
        [InlineData("RackLayoutCommands.Fill.cs", 1)] // RACKRELLENAR
        public void EachInsertionPath_CallsTheGuard_OncePerOperation_NoAliasDoubleWarn(string file, int expectedCalls)
        {
            // Exact count pins BOTH "one message per operation" and "aliases don't double-warn" (aliases delegate to
            // the target method, which holds the single call) — and, for the cama file, that EditCama is NOT wired.
            Assert.Equal(expectedCalls, Count(ReadPluginSource(file), GuardCall));
        }

        // ---- (3) Update-vs-insert: pure update is quiet; inserting a new view warns, before the first redraw --------

        [Theory]
        [InlineData("RackSelectivoCommands.cs", "void EditSelective")]
        [InlineData("RackDinamicoCommands.cs", "void EditDynamic")]
        [InlineData("RackCabeceraCommands.cs", "void EditCabecera")]
        public void EditInsert_IsGatedByUpdateOnly_AndWarnsBeforeTheFirstRedraw(string file, string editSignature)
        {
            var src = ReadPluginSource(file);
            var method = IndexFrom(src, editSignature, 0, $"{file} must define {editSignature}.");

            var gate = IndexFrom(src, "!window.UpdateOnly", method, "the edit-insert guard must be gated by !window.UpdateOnly.");
            var guard = IndexFrom(src, GuardCall, method, "the edit method must call the units guard.");
            var redraw = IndexFrom(src, ".RedrawInPlace(", method, "the edit method must redraw views in place.");

            Assert.True(gate < guard, "the guard must sit inside the !window.UpdateOnly branch (a pure update stays quiet).");
            Assert.True(guard < redraw, "the guard must warn BEFORE the first in-place redraw (before any DWG modification).");
        }

        [Fact]
        public void EditCama_IsAPureUpdate_AndNeverWarns()
        {
            var src = ReadPluginSource("RackCamaCommands.cs");
            var editCama = IndexFrom(src, "void EditCama", 0, "RackCamaCommands must define EditCama.");
            // No guard call anywhere from EditCama onward (QUICKCAMA's single call sits ABOVE this point).
            Assert.Equal(-1, src.IndexOf(GuardCall, editCama, StringComparison.Ordinal));
        }

        // ---- (4) RACKLAYOUT / RACKRELLENAR pass through the guard BEFORE their functional prompts -------------------

        [Theory]
        [InlineData("RackLayoutCommands.cs", "void RackLayout()")]
        [InlineData("RackLayoutCommands.Fill.cs", "void RackRellenar()")]
        public void LayoutAndFill_WarnBeforeTheirFirstFunctionalPrompt(string file, string signature)
        {
            var src = ReadPluginSource(file);
            var method = IndexFrom(src, signature, 0, $"{file} must define {signature}.");
            var guard = IndexFrom(src, GuardCall, method, "layout/fill must call the units guard.");
            var firstPrompt = IndexFrom(src, "PickRackBlock", method, "layout/fill starts by picking a rack (its first functional prompt).");
            Assert.True(guard < firstPrompt, "the units guard must run BEFORE the first functional prompt (PickRackBlock).");
        }

        // ---- (5) The menu warns once, before dispatching any of the four new insertions ----------------------------

        [Fact]
        public void Menu_Warns_BeforeDispatchingTheInsertion()
        {
            var src = ReadPluginSource("RackMenuCommands.cs");
            var menu = IndexFrom(src, "void RackCad()", 0, "RackMenuCommands must define RackCad().");
            var guard = IndexFrom(src, GuardCall, menu, "the menu must call the units guard.");
            var dispatch = IndexFrom(src, "switch (menu.InsertionRequest)", menu, "the menu dispatches by InsertionRequest.");
            Assert.True(guard < dispatch, "the menu must warn BEFORE dispatching the insertion (before the first DWG modification).");
        }
    }
}
