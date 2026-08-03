using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-37D gate 4 — guards over the Plugin half of the Cantilever system, read as TEXT.
    ///
    /// The Plugin references AutoCAD, so this suite cannot load it (ADR-0003) and CI has no AutoCAD. What is
    /// pinned here are the properties that only exist as ORDER and OWNERSHIP in the source and that no unit test
    /// can observe without a running acad.exe: that the host never re-resolves a line the window already resolved,
    /// that the whole preflight runs before the first geometry is touched, that there is exactly one regen, that an
    /// edit never mints a new identity, and that a corrupt descriptor is never coerced into another view.
    /// </summary>
    public class CantileverPluginSourceGuardTests
    {
        private static DirectoryInfo RepoRoot()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);

            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "RackCad.sln")))
            {
                dir = dir.Parent;
            }

            Assert.True(dir != null, "No se encontro la raiz del repositorio (RackCad.sln).");
            return dir;
        }

        private static string PluginDirectory => Path.Combine(RepoRoot().FullName, "src", "RackCad.Plugin");

        private static string Read(params string[] parts)
        {
            var path = Path.Combine(PluginDirectory, Path.Combine(parts));
            Assert.True(File.Exists(path), "No existe el archivo: " + path);
            return CodeOnly(File.ReadAllText(path));
        }

        private static string CodeOnly(string source)
        {
            var withoutBlocks = Regex.Replace(source, @"/\*.*?\*/", string.Empty, RegexOptions.Singleline);
            return Regex.Replace(withoutBlocks, @"//[^\n]*", string.Empty);
        }

        private static int Count(string source, string needle)
        {
            var count = 0;
            var index = source.IndexOf(needle, StringComparison.Ordinal);

            while (index >= 0)
            {
                count++;
                index = source.IndexOf(needle, index + needle.Length, StringComparison.Ordinal);
            }

            return count;
        }

        private static string Commands => Read("RackCantileverCommands.cs");

        private static string Materializer => Read("Drawing", "Cantilever", "CantileverViewMaterializer.cs");

        private static string Handler => Read("KindHandlers", "CantileverKindHandler.cs");

        private static string Registry => Read("KindHandlers", "KindHandlerRegistry.cs");

        private static string Menu => Read("RackMenuCommands.cs");

        /// <summary>The body of <c>EditCantilever</c>, by brace matching — indices would be fragile as it grows.</summary>
        private static string EditBody()
        {
            var source = Commands;
            var start = source.IndexOf("internal static void EditCantilever(", StringComparison.Ordinal);

            Assert.True(start > 0, "No existe EditCantilever.");

            var open = source.IndexOf('{', start);
            var depth = 0;

            for (var i = open; i < source.Length; i++)
            {
                if (source[i] == '{') depth++;
                else if (source[i] == '}' && --depth == 0) return source.Substring(open, i - open + 1);
            }

            throw new InvalidOperationException("Llaves desbalanceadas en EditCantilever.");
        }

        // ---- 1. The host never recomputes what the editor already computed -------------------------------------

        [Fact]
        public void NoPluginFileResolvesACantileverLine()
        {
            // The window resolved it and handed the resolved line over in the request. A second resolution here
            // would load the catalogue again, at another moment, and could disagree with the picture the user
            // approved.
            var offenders = Directory.GetFiles(PluginDirectory, "*.cs", SearchOption.AllDirectories)
                .Where(p => !p.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar))
                .Where(p => !p.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar))
                .Where(p => Regex.IsMatch(CodeOnly(File.ReadAllText(p)), @"\bCantileverLineResolver\b"))
                .Select(Path.GetFileName)
                .ToArray();

            Assert.Empty(offenders);
        }

        [Fact]
        public void TheCommandDrawsTheLineItWasGiven()
        {
            var commands = Commands;

            Assert.Contains("CantileverLineAssembly line", commands, StringComparison.Ordinal);
            Assert.Contains("new RackCantileverWindow(canInsertInAutoCad: true)", commands, StringComparison.Ordinal);
            Assert.Contains("window.LoadExisting(", commands, StringComparison.Ordinal);
            Assert.Contains("window.DesignToInsert", commands, StringComparison.Ordinal);
            Assert.Contains("window.LineToInsert", commands, StringComparison.Ordinal);
        }

        [Fact]
        public void TheMaterialiserOnlyAdaptsPointsAndDecidesNoGeometry()
        {
            var materializer = Materializer;

            Assert.Contains("new Polyline(", materializer, StringComparison.Ordinal);
            Assert.DoesNotContain("CantileverViewPlanBuilder", materializer, StringComparison.Ordinal);
            Assert.DoesNotContain("StructuralSectionPlanBuilder", materializer, StringComparison.Ordinal);
            Assert.DoesNotContain("CantileverLineResolver", materializer, StringComparison.Ordinal);

            // In INCHES, unscaled: the plan is already in the drawing's own unit and converting here would be the
            // silent reinterpretation ADR-0005 forbids.
            Assert.DoesNotMatch(new Regex(@"\*\s*25\.4|/\s*25\.4"), materializer);
        }

        // ---- 2. The preflight runs before anything is touched --------------------------------------------------

        [Fact]
        public void TheWholePreflightRunsBeforeTheFirstRedefinition()
        {
            var body = EditBody();

            var kindCheck = body.IndexOf("RackEmbedDocument.KindCantilever", StringComparison.Ordinal);
            var innerCheck = body.IndexOf("PreflightInnerSources", StringComparison.Ordinal);
            var descriptorCheck = body.IndexOf("IsValidCantileverDescriptor", StringComparison.Ordinal);
            var firstWrite = body.IndexOf("RedefineBlock", StringComparison.Ordinal);
            var firstErase = body.IndexOf("EraseViewBlocks", StringComparison.Ordinal);

            Assert.True(kindCheck > 0 && innerCheck > 0 && descriptorCheck > 0 && firstWrite > 0);
            Assert.True(kindCheck < firstWrite, "La comprobacion de kind/GUID debe correr antes de redibujar.");
            Assert.True(innerCheck < firstWrite, "El preflight I-11 debe correr antes de redibujar.");
            Assert.True(descriptorCheck < firstWrite, "El descriptor debe validarse antes de redibujar.");
            Assert.True(firstWrite < firstErase, "Nada se borra antes de haber redibujado.");
        }

        [Fact]
        public void AnEditNeverMintsANewIdentity()
        {
            // The picked envelope's GUID is kept; the window's is only a fallback. A new GUID here would silently
            // split one line into two.
            Assert.DoesNotMatch(new Regex(@"\bGuid\.NewGuid\b"), Commands);
            Assert.Contains("string.IsNullOrWhiteSpace(embed.Id) ? window.RackId : embed.Id", EditBody(), StringComparison.Ordinal);
        }

        [Fact]
        public void ExactlyOneRegenForTheWholeBatch_AndItAgreesWithWhatIsReported()
        {
            var body = EditBody();

            Assert.Equal(1, Count(body, "document.Editor.Regen()"));
            Assert.Contains("if (changedInPlace)", body, StringComparison.Ordinal);

            // Erasing a stale lateral IS a change: the regen and the final message must never disagree.
            Assert.Contains("erasedPhantoms > 0", body, StringComparison.Ordinal);
        }

        [Fact]
        public void AStaleLateralIsNeverRedrawnAtAnotherStation_AndTheLastLinkIsNeverDeleted()
        {
            var body = EditBody();

            Assert.Contains("station >= line.Stations.Count", body, StringComparison.Ordinal);
            Assert.Contains("staleViewBlocks.Add(viewBlock.BlockId)", body, StringComparison.Ordinal);
            Assert.Contains("survivors > 0", body, StringComparison.Ordinal);
        }

        [Fact]
        public void ACorruptDescriptorIsNeverCoercedIntoAnotherView()
        {
            var commands = Commands;

            Assert.Contains("internal static bool IsValidCantileverDescriptor(", commands, StringComparison.Ordinal);

            // Frontal and planta are views of the whole LINE (section −1); only a lateral carries a station.
            Assert.Contains("kind == CantileverViewKind.Lateral", commands, StringComparison.Ordinal);
            Assert.Contains("embed.Section >= 0", commands, StringComparison.Ordinal);
            Assert.Contains("embed.Section == -1", commands, StringComparison.Ordinal);
        }

        [Fact]
        public void AnUnknownViewFailsVisiblyAndDrawsNothing()
        {
            var commands = Commands;

            Assert.Contains("vista Cantilever desconocida", commands, StringComparison.Ordinal);
            Assert.Contains("no se dibuja", commands, StringComparison.Ordinal);
        }

        // ---- 3. Identity and payload ---------------------------------------------------------------------------

        [Fact]
        public void ThePayloadGoesOnTheDefinition_SoEveryCopySharesIt()
        {
            var commands = Commands;

            Assert.Contains("RackBlockData.Write(transaction, definitionId, payload)", commands, StringComparison.Ordinal);
            Assert.Contains("RackBlockData.Write(transaction, viewBlock.BlockId, payload)", commands, StringComparison.Ordinal);
            Assert.Contains("RackEmbedDocument.KindCantilever", commands, StringComparison.Ordinal);
        }

        [Fact]
        public void TheEnvelopeAndTheInnerProjectAreIndependentBoundaries()
        {
            var commands = Commands;

            // I-11: the inner design is itself a RackProjectDocument, and the source metadata of BOTH levels rides
            // into the new embed.
            Assert.Contains("WithSourceMetadataFrom(innerSource)", commands, StringComparison.Ordinal);
            Assert.Contains("RackEmbedComposer.Compose(", commands, StringComparison.Ordinal);
        }

        [Fact]
        public void TheHandlerReStampsTheInnerIdentityOfAnIndependentCopy()
        {
            // Unlike Push Back, a Cantilever design carries its OWN GUID — one per LINE — so a copy that kept it
            // would be a second line claiming to be the first.
            var handler = Handler;

            Assert.Contains("design.Id =", handler, StringComparison.Ordinal);
            Assert.Contains("design.Name = copyName", handler, StringComparison.Ordinal);
            Assert.Contains("RackCantileverCommands.EditCantilever", handler, StringComparison.Ordinal);
        }

        [Fact]
        public void TheKindIsRegisteredAndTheMenuDispatchesIt()
        {
            Assert.Contains("new CantileverKindHandler()", Registry, StringComparison.Ordinal);
            Assert.Contains("case CantileverInsertionRequest cantilever:", Menu, StringComparison.Ordinal);
            Assert.Contains("RackCantileverCommands.DrawCantileverView(", Menu, StringComparison.Ordinal);
        }

        [Fact]
        public void TheCommandAndItsAliasExist()
        {
            var commands = Commands;

            Assert.Contains("[CommandMethod(\"RACKCANTILEVER\")]", commands, StringComparison.Ordinal);
            Assert.Contains("[CommandMethod(\"RCT\")]", commands, StringComparison.Ordinal);
        }

        [Fact]
        public void TheUnitsAdvisoryFiresOnceBeforeTheFirstBlock_AndAPureUpdateDoesNotWarn()
        {
            var body = EditBody();

            Assert.Contains("if (!window.UpdateOnly)", body, StringComparison.Ordinal);
            Assert.Contains("RackUnitsGuard.WarnIfNotInches(document)", body, StringComparison.Ordinal);
            Assert.True(
                body.IndexOf("RackUnitsGuard.WarnIfNotInches", StringComparison.Ordinal) <
                body.IndexOf("RedefineBlock", StringComparison.Ordinal));
        }

        // ---- 4. The catalogue is loaded fail closed, through the single owner ----------------------------------

        [Fact]
        public void TheCantileverPluginLoadsTheCatalogueThroughTheSingleOwner()
        {
            Assert.Contains("StructuralSectionCatalogAccess.TryLoad", Commands, StringComparison.Ordinal);
            Assert.Contains("StructuralSectionCatalogAccess.TryLoad", Handler, StringComparison.Ordinal);
            Assert.DoesNotContain("CsvStructuralSectionCatalogProvider", Commands, StringComparison.Ordinal);
            Assert.DoesNotContain("CsvStructuralSectionCatalogProvider", Handler, StringComparison.Ordinal);
        }

        // ---- 5. The UI half stays out of AutoCAD ---------------------------------------------------------------

        [Fact]
        public void TheCantileverEditorReferencesNoAutoCadType()
        {
            var folder = Path.Combine(RepoRoot().FullName, "src", "RackCad.UI", "Systems", "Cantilever");

            Assert.True(Directory.Exists(folder), "No existe la carpeta del editor Cantilever.");

            foreach (var file in Directory.GetFiles(folder, "*.cs", SearchOption.AllDirectories))
            {
                Assert.DoesNotMatch(new Regex(@"\bAutodesk\."), CodeOnly(File.ReadAllText(file)));
            }
        }
    }
}
