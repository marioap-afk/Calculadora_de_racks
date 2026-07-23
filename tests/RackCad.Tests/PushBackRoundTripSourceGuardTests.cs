using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
        private static string Duplicar => ReadPluginSource("RackDuplicarCommands.cs");
        private static string Layout => ReadPluginSource("RackLayoutCommands.cs");

        /// <summary>Every <c>[CommandMethod("NAME")]</c> declared anywhere in the Plugin (excluding bin/obj).</summary>
        private static IReadOnlyList<string> AllCommandMethods()
        {
            var pluginDir = Path.Combine(RepoRoot().FullName, "src", "RackCad.Plugin");
            var names = new List<string>();
            foreach (var file in Directory.EnumerateFiles(pluginDir, "*.cs", SearchOption.AllDirectories))
            {
                if (file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
                    || file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                foreach (Match match in Regex.Matches(File.ReadAllText(file), "\\[CommandMethod\\(\"([^\"]+)\""))
                {
                    names.Add(match.Groups[1].Value);
                }
            }

            return names;
        }

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

        [Fact]
        public void CopyAndLayout_AcceptPushBackViaIgnoreCaseLookup_WithNoPerKindBranch()
        {
            foreach (var src in new[] { Duplicar, Layout })
            {
                Assert.Contains("KindHandlerDispatch.TryResolveIgnoreCase(editor, embed.Kind, out _)", src);
                Assert.DoesNotContain("KindPushBack", src);        // no hard-coded per-kind arm
                Assert.DoesNotContain("PushBackKindHandler", src);
            }
        }

        // ---- Increment 5a: end-to-end chain closure ----

        [Fact]
        public void EditPushBack_RegenAndFinalMessage_ShareOneChangedInPlaceAuthority_IncludingErasedCuts()
        {
            // An edit that ONLY erased stale cuts still changed the drawing: the regen and the success message must be
            // driven by the SAME expression, and erasedPhantoms must be part of it.
            Assert.Contains("var changedInPlace = updatedLateral + updatedFrontal + updatedPlanta + erasedPhantoms > 0;", Commands);
            Assert.Contains("if (changedInPlace)", Commands);              // the single explicit regen
            Assert.Contains("editor.WriteMessage(changedInPlace", Commands); // the final report
            Assert.Equal(3, Count(Commands, "changedInPlace"));             // declaration + regen gate + message gate
            Assert.Equal(1, Count(Commands, "document.Editor.Regen()"));
        }

        [Fact]
        public void CommandMethodNames_AreUnique_SoRackpushbackAndRpbCannotCollide()
        {
            var all = AllCommandMethods();
            Assert.NotEmpty(all);

            var duplicates = all
                .GroupBy(name => name, StringComparer.OrdinalIgnoreCase)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .ToList();
            Assert.True(duplicates.Count == 0, "duplicate [CommandMethod] names: " + string.Join(", ", duplicates));

            Assert.Equal(1, all.Count(n => string.Equals(n, "RACKPUSHBACK", StringComparison.OrdinalIgnoreCase)));
            Assert.Equal(1, all.Count(n => string.Equals(n, "RPB", StringComparison.OrdinalIgnoreCase)));
        }

        [Fact]
        public void MenuAndDirectCommand_ConvergeOnTheSameDrawPath()
        {
            // The direct RACKPUSHBACK command body ends in DrawPushBackView...
            var start = Commands.IndexOf("public void RackPushBack()", StringComparison.Ordinal);
            var end = Commands.IndexOf("internal static void DrawPushBackView(", StringComparison.Ordinal);
            Assert.True(start >= 0 && end > start, "could not isolate the RACKPUSHBACK command body");
            Assert.Contains("DrawPushBackView(", Commands.Substring(start, end - start));

            // ...and so does the RACKCAD menu's typed case: one draw path, not two.
            Assert.Contains("RackPushBackCommands.DrawPushBackView", Menu);
        }
    }
}
