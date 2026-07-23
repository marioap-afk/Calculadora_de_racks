using System;
using System.IO;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// Source-characterization guards for the I-18b increment 4a Plugin wiring. The Plugin references AutoCAD, so
    /// RackCad.Tests must NOT load it (ADR-0003); these read the Plugin <c>.cs</c> as TEXT (no assembly loaded, no Autodesk
    /// dependency) and pin the wiring the pure suites cannot observe: the RACKPUSHBACK/RPB commands, the window + envelope +
    /// payload reuse, the three thin draw services, the single lateral-name authority and the typed host dispatch. Same
    /// pattern as <see cref="RackUnitsGuardSourceTests"/>.
    /// </summary>
    public class PushBackPluginSourceGuardTests
    {
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

        private static string ReadPluginSource(string relativePath)
        {
            var path = Path.Combine(RepoRoot().FullName, "src", "RackCad.Plugin", relativePath);
            Assert.True(File.Exists(path), $"Plugin source not found: {path}");
            return File.ReadAllText(path);
        }

        private static string Commands => ReadPluginSource("RackPushBackCommands.cs");
        private static string Menu => ReadPluginSource("RackMenuCommands.cs");
        private static string LateralService => ReadPluginSource(Path.Combine("Systems", "PushBackSystemDrawService.cs"));
        private static string FrontalService => ReadPluginSource(Path.Combine("Systems", "PushBackFrontalDrawService.cs"));
        private static string PlantaService => ReadPluginSource(Path.Combine("Systems", "PushBackPlantaDrawService.cs"));

        // ---- Commands + aliases ----

        [Fact]
        public void Commands_ExposeRackPushBackAndRpb_AndRpbDelegatesToTheMainCommand()
        {
            var src = Commands;
            Assert.Contains("[CommandMethod(\"RACKPUSHBACK\")]", src);
            Assert.Contains("[CommandMethod(\"RPB\")]", src);

            var rpb = src.IndexOf("[CommandMethod(\"RPB\")]", StringComparison.Ordinal);
            Assert.Contains("=> RackPushBack()", src.Substring(rpb, Math.Min(200, src.Length - rpb))); // RPB delegates
        }

        [Fact]
        public void Commands_UseTheWindow_Envelope_Payload_AndTheThreeDrawServices_NeverKindDynamic()
        {
            var src = Commands;
            Assert.Contains("new RackPushBackSystemWindow(", src);
            Assert.Contains("RackEmbedDocument.KindPushBack", src);
            Assert.Contains("RackProject.ForPushBack(", src);
            Assert.Contains("WithSourceMetadataFrom(innerSource)", src);
            Assert.DoesNotContain("KindDynamic", src);

            Assert.Contains("new PushBackSystemDrawService()", src);
            Assert.Contains("new PushBackFrontalDrawService()", src);
            Assert.Contains("new PushBackPlantaDrawService()", src);
        }

        [Fact]
        public void LateralNameSuffix_IsOwnedByTheService_NotTheCommand()
        {
            // Single authority for "- lateral N": the service adds it; the command passes only the BASE name.
            Assert.Contains("- lateral ", LateralService);
            Assert.DoesNotContain("- lateral ", Commands);
        }

        // ---- Typed host dispatch ----

        [Fact]
        public void Menu_HasTheTypedPushBackCase_DelegatingToDrawPushBackView_WithoutAKindSwitch()
        {
            var src = Menu;
            Assert.Contains("case PushBackInsertionRequest", src);

            var caseIdx = src.IndexOf("case PushBackInsertionRequest", StringComparison.Ordinal);
            Assert.Contains("RackPushBackCommands.DrawPushBackView", src.Substring(caseIdx, Math.Min(500, src.Length - caseIdx)));

            Assert.DoesNotContain("case RackSystemKind.PushBack", src);      // dispatch stays TYPED on the request
            Assert.DoesNotContain("switch (menu.InsertionRequest.Kind", src);
        }

        [Fact]
        public void Rackeditar_StillResolvesExclusivelyViaTheKindHandlerRegistry_NoPushBackEditBranch()
        {
            var src = Menu;
            Assert.Contains("KindHandlerDispatch.TryResolve", src); // RACKEDITAR dispatches through the kind-handler seam

            var editIdx = src.IndexOf("[CommandMethod(\"RACKEDITAR\")]", StringComparison.Ordinal);
            Assert.True(editIdx >= 0);
            var editSection = src.Substring(editIdx);
            Assert.DoesNotContain("RackPushBackCommands", editSection); // no direct Push Back branch inside RACKEDITAR (4a)
            Assert.DoesNotContain("DrawPushBackView", editSection);
        }

        // ---- Draw services: thin adapters over ViewBlockDraw, one builder each ----

        [Theory]
        [InlineData("Systems/PushBackSystemDrawService.cs")]
        [InlineData("Systems/PushBackFrontalDrawService.cs")]
        [InlineData("Systems/PushBackPlantaDrawService.cs")]
        public void EachDrawService_CallsViewBlockDraw_DrawAndPlace_AndRedrawInPlace(string file)
        {
            var src = ReadPluginSource(file.Replace('/', Path.DirectorySeparatorChar));
            Assert.Contains("ViewBlockDraw.DrawAndPlace(", src);
            Assert.Contains("ViewBlockDraw.RedrawInPlace(", src);
        }

        [Fact]
        public void DrawServices_ConsumeExactlyTheirPushBackBuilder()
        {
            Assert.Contains("PushBackSystemLateralBuilder", LateralService);
            Assert.Contains("PushBackSystemFrontalBuilder", FrontalService);
            Assert.Contains("PushBackSystemPlantaBuilder", PlantaService);
        }

        [Fact]
        public void DrawServices_HaveNoDynamicOrSelectiveBuilders_NorGeometryMath()
        {
            foreach (var src in new[] { LateralService, FrontalService, PlantaService })
            {
                Assert.DoesNotContain("DynamicSystemLateralBuilder", src);
                Assert.DoesNotContain("DynamicSystemFrontalBuilder", src);
                Assert.DoesNotContain("DynamicSystemPlantaBuilder", src);
                Assert.DoesNotContain("SelectiveSystem", src);
                Assert.DoesNotContain("Math.", src); // no geometry recomputation in the thin adapter — the plan is drawn verbatim
            }
        }

        [Fact]
        public void EachDrawService_ConsumesOnlyItsOwnBuilder_NotTheOtherTwoPushBackBuilders()
        {
            Assert.DoesNotContain("PushBackSystemFrontalBuilder", LateralService);
            Assert.DoesNotContain("PushBackSystemPlantaBuilder", LateralService);
            Assert.DoesNotContain("PushBackSystemLateralBuilder", FrontalService);
            Assert.DoesNotContain("PushBackSystemPlantaBuilder", FrontalService);
            Assert.DoesNotContain("PushBackSystemLateralBuilder", PlantaService);
            Assert.DoesNotContain("PushBackSystemFrontalBuilder", PlantaService);
        }
    }
}
