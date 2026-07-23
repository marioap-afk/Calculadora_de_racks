using System;
using System.IO;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// Source-characterization guards for the I-18b increment 4b round-trip wiring: the Push Back kind handler, its
    /// registration as the FIFTH embedded kind, the <c>EditPushBack</c> multiview edit, and the (unchanged) consumers that
    /// adopt Push Back automatically through the registry. RackCad.Tests must NOT load the Plugin (AutoCAD refs, ADR-0003),
    /// so these read the Plugin <c>.cs</c> as TEXT. Same pattern as <see cref="PushBackPluginSourceGuardTests"/>.
    /// </summary>
    public class PushBackRoundTripSourceGuardTests
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

        private static int Count(string src, string needle)
        {
            var count = 0;
            var i = 0;
            while ((i = src.IndexOf(needle, i, StringComparison.Ordinal)) >= 0)
            {
                count++;
                i += needle.Length;
            }

            return count;
        }

        private static string Handler => ReadPluginSource(Path.Combine("KindHandlers", "PushBackKindHandler.cs"));
        private static string Registry => ReadPluginSource(Path.Combine("KindHandlers", "KindHandlerRegistry.cs"));
        private static string Interface => ReadPluginSource(Path.Combine("KindHandlers", "IRackKindHandler.cs"));
        private static string Commands => ReadPluginSource("RackPushBackCommands.cs");
        private static string Menu => ReadPluginSource("RackMenuCommands.cs");
        private static string BomTotal => ReadPluginSource("RackInventarioCommands.BomTotal.cs");
        private static string Restamp => ReadPluginSource("RackEnvelopeRestamp.cs");

        // ---- Handler ----

        [Fact]
        public void Handler_ImplementsInterface_WithPushBackKindAndBomLabel()
        {
            Assert.Contains("PushBackKindHandler : IRackKindHandler", Handler);
            Assert.Contains("Kind => RackEmbedDocument.KindPushBack", Handler);
            Assert.Contains("BomLabel => \"Push Back\"", Handler);
        }

        [Fact]
        public void Handler_DelegatesEditToEditPushBack_NoInlineEditLogic()
        {
            Assert.Contains("RackPushBackCommands.EditPushBack(document, blockId, embed)", Handler);
        }

        [Fact]
        public void Handler_BuildBom_UsesStore_RequiresPushBackDesign_PushBackResolverAndBomBuilder()
        {
            Assert.Contains("new RackProjectStore().Deserialize(embed.Design)", Handler);
            Assert.Contains("project?.PushBackDesign == null", Handler);
            Assert.Contains("new PushBackResolver(catalog).Resolve(project.PushBackDesign)", Handler);
            Assert.Contains("PushBackBomBuilder.Build(system, catalog)", Handler);
        }

        [Fact]
        public void Handler_NeverUsesDynamicResolverOrSystemBomBuilder()
        {
            Assert.DoesNotContain("DynamicRackSystemResolver", Handler);
            Assert.DoesNotContain("SystemBomBuilder", Handler);
        }

        [Fact]
        public void Handler_RestampReturnsDesignJsonUnchanged()
        {
            Assert.Contains("RestampDesign(string designJson, string newId, string copyName) => designJson", Handler);
        }

        // ---- Registry ----

        [Fact]
        public void Registry_RegistersFiveHandlers_PushBackImmediatelyAfterDynamic_InExactOrder()
        {
            var order = new[]
            {
                "new SelectiveKindHandler()",
                "new DynamicKindHandler()",
                "new PushBackKindHandler()",
                "new CabeceraKindHandler()",
                "new CamaKindHandler()",
            };

            var previous = -1;
            foreach (var handler in order)
            {
                var index = Registry.IndexOf(handler, StringComparison.Ordinal);
                Assert.True(index > previous, $"registry handler out of canonical order or missing: {handler}");
                previous = index;
            }

            Assert.Equal(5, Count(Registry, "KindHandler()")); // exactly five constructions, no duplicates
            Assert.Equal(1, Count(Registry, "new PushBackKindHandler()"));
        }

        [Fact]
        public void Registry_CommentUpdatedToFiveKinds()
        {
            Assert.Contains("five embedded kinds", Registry);
            Assert.DoesNotContain("four embedded kinds", Registry);
        }

        [Fact]
        public void Registry_PreservesTheDispatchSurface()
        {
            Assert.Contains("KindDispatch<IRackKindHandler>", Registry); // explicit construction, no reflection
            Assert.Contains("TryGet(string kind", Registry);             // ordinal lookup
            Assert.Contains("TryGetIgnoreCase(string kind", Registry);   // ignore-case lookup
            Assert.Contains("TryResolveAll(", Registry);                 // preflight-all
            Assert.Contains("IReadOnlyList<IRackKindHandler> Handlers", Registry); // read-only collection
        }

        [Fact]
        public void Interface_IsNotModifiedForPushBack()
        {
            Assert.DoesNotContain("PushBack", Interface);
        }

        // ---- EditPushBack ----

        [Fact]
        public void EditPushBack_Exists_UsesWindowAndLoadExisting()
        {
            Assert.Contains("internal static void EditPushBack(", Commands);
            Assert.Contains("new RackPushBackSystemWindow(canInsertInAutoCad: true)", Commands);
            Assert.Contains("window.LoadExisting(project.PushBackDesign, embed.Id, embed.Name, project)", Commands);
        }

        [Fact]
        public void EditPushBack_FindsLinkedBlocks_AndInnerPreflightsBeforeAnyRedraw()
        {
            Assert.Contains("RackCommandSupport.FindRackBlocks(document, id)", Commands);
            Assert.Contains("RackCommandSupport.PreflightInnerSources(blocks, RackSystemKind.PushBack, project)", Commands);

            var preflight = Commands.IndexOf("PreflightInnerSources(blocks, RackSystemKind.PushBack", StringComparison.Ordinal);
            var firstRedraw = Commands.IndexOf("RedrawInPlace(", StringComparison.Ordinal);
            Assert.True(preflight >= 0 && firstRedraw >= 0 && preflight < firstRedraw,
                "inner-source preflight must precede the first RedrawInPlace");
        }

        [Fact]
        public void EditPushBack_ValidatesKindViewAndSectionBeforeModifying()
        {
            var firstRedraw = Commands.IndexOf("RedrawInPlace(", StringComparison.Ordinal);
            var kindCheck = Commands.IndexOf("RackEmbedDocument.KindPushBack, StringComparison.OrdinalIgnoreCase", StringComparison.Ordinal);
            var descriptorCall = Commands.IndexOf("IsValidPushBackDescriptor(viewBlock.Embed)", StringComparison.Ordinal);

            Assert.True(kindCheck >= 0 && kindCheck < firstRedraw, "envelope kind check must precede geometry");
            Assert.True(descriptorCall >= 0 && descriptorCall < firstRedraw, "view descriptor check must precede geometry");

            // the descriptor helper covers all three views + both frontal ends + the section rule
            Assert.Contains("RackEmbedDocument.ViewPlanta", Commands);
            Assert.Contains("RackEmbedDocument.ViewFrontal", Commands);
            Assert.Contains("RackEmbedDocument.ViewLateral", Commands);
            Assert.Contains("PushBackFrontalEnd.EntradaSalida", Commands);
            Assert.Contains("PushBackFrontalEnd.Posterior", Commands);
        }

        [Fact]
        public void EditPushBack_RedrawsWithTheThreeDrawServices_EveryRedrawRegenFalse()
        {
            Assert.Contains("new PushBackPlantaDrawService().RedrawInPlace(", Commands);
            Assert.Contains("new PushBackFrontalDrawService().RedrawInPlace(", Commands);
            Assert.Contains("new PushBackSystemDrawService().RedrawInPlace(", Commands);
            Assert.Equal(Count(Commands, "RedrawInPlace("), Count(Commands, "regen: false")); // every RedrawInPlace defers regen
        }

        [Fact]
        public void EditPushBack_RenamesViaSyncName_ErasesStale_AndDoesExactlyOneBatchRegen()
        {
            Assert.Contains("RackBlockRenamer.SyncName(", Commands);
            Assert.Contains("RackCommandSupport.EraseViewBlocks(document, staleViewBlocks)", Commands);
            Assert.Equal(1, Count(Commands, "document.Editor.Regen()")); // one explicit batch regen for the in-place work
        }

        [Fact]
        public void EditPushBack_UsesResolvedByBlockPerView_AndAdditionalViewCarriesEmbedAndProject()
        {
            Assert.Contains("preflight.ResolvedByBlock[viewBlock.BlockId]", Commands);
            Assert.Contains(
                "DrawPushBackView(window.InsertView, window.InsertSection, system, design, id, name, source: embed, innerSource: project)",
                Commands);
        }

        [Fact]
        public void EditPushBack_UsesNoDynamicOrSelectiveDrawServicesOrBuilders()
        {
            Assert.DoesNotContain("DynamicSystemDrawService", Commands);
            Assert.DoesNotContain("DynamicPlantaDrawService", Commands);
            Assert.DoesNotContain("DynamicFrontalDrawService", Commands);
            Assert.DoesNotContain("DynamicSystemLateralBuilder", Commands);
            Assert.DoesNotContain("SelectiveSystemDrawService", Commands);
            Assert.DoesNotContain("SelectiveLateralBuilder", Commands);
        }

        // ---- Consumers (unchanged; adopt Push Back through the registry only) ----

        [Fact]
        public void Rackeditar_ResolvesOnlyViaKindHandlerDispatch_NoPushBackEditBranch()
        {
            Assert.Contains("KindHandlerDispatch.TryResolve(editor, embed.Kind, out var handler)", Menu);

            // Scope to the RACKEDITAR method (the file's RACKCAD menu keeps the 4a PushBackInsertionRequest dispatch).
            var editIdx = Menu.IndexOf("[CommandMethod(\"RACKEDITAR\")]", StringComparison.Ordinal);
            Assert.True(editIdx >= 0);
            var editSection = Menu.Substring(editIdx);
            Assert.DoesNotContain("PushBack", editSection);
        }

        [Fact]
        public void Rackbomtotal_ResolvesAllViaKindHandlerDispatch_NoPushBackBranch()
        {
            Assert.Contains("KindHandlerDispatch.TryResolveAll(", BomTotal);
            Assert.DoesNotContain("PushBack", BomTotal);
        }

        [Fact]
        public void RackEnvelopeRestamp_ResolvesViaRegistryIgnoreCase_NoPushBackBranch()
        {
            Assert.Contains("KindHandlerRegistry.Default.TryGetIgnoreCase(", Restamp);
            Assert.DoesNotContain("PushBack", Restamp);
        }
    }
}
