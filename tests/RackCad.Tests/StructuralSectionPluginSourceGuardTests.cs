using System;
using System.IO;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// Source guards for the I-36B AutoCAD side.
    ///
    /// The Plugin references AutoCAD, so this suite must NOT load it (ADR-0003) and CI has no AutoCAD to run it
    /// in. These read the Plugin <c>.cs</c> files as TEXT and pin the decisions that only exist there: the
    /// command, the fail-closed catalogue load, the single transaction, the internal block, and — above all —
    /// the promises the initiative makes about what the adapter must NEVER do. Same pattern as
    /// <see cref="PushBackPluginSourceGuardTests"/>.
    ///
    /// What a text guard cannot check is what the drawing LOOKS like; that is the owner's twelve-point AutoCAD
    /// checklist, and it is not replaced by anything here.
    /// </summary>
    public class StructuralSectionPluginSourceGuardTests
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

        private static string ReadSource(params string[] parts)
        {
            var path = Path.Combine(RepoRoot().FullName, Path.Combine(parts));
            Assert.True(File.Exists(path), "No existe el archivo: " + path);
            return File.ReadAllText(path);
        }

        private static string Command =>
            ReadSource("src", "RackCad.Plugin", "RackSeccionCommands.cs");

        private static string Materializer =>
            ReadSource("src", "RackCad.Plugin", "Drawing", "StructuralSections", "StructuralSectionMaterializer.cs");

        private static string HeaderDrawer =>
            ReadSource("src", "RackCad.Plugin", "Drawing", "LateralHeaderDrawer.cs");

        /// <summary>
        /// The file with its comments removed.
        ///
        /// The "must never mention X" guards below have to look at CODE. Both files EXPLAIN at length why they
        /// do not use <c>blocks-library.dwg</c> — that prose is the most useful part of the file, and a guard
        /// that forbade naming the thing it rejects would push the reasoning out of the source. Neither file has
        /// a string literal containing <c>//</c>, so stripping line and block comments is safe here.
        /// </summary>
        private static string CodeOnly(string source)
        {
            var withoutBlocks = System.Text.RegularExpressions.Regex.Replace(
                source, @"/\*.*?\*/", string.Empty, System.Text.RegularExpressions.RegexOptions.Singleline);

            return System.Text.RegularExpressions.Regex.Replace(
                withoutBlocks, @"//[^\n]*", string.Empty);
        }

        // ---- The command exists and behaves ------------------------------------------------------------

        [Fact]
        public void TheCommandIsRegisteredAsRackseccion()
        {
            Assert.Contains("[CommandMethod(\"RACKSECCION\")]", Command, StringComparison.Ordinal);
        }

        [Fact]
        public void AnInvalidCatalogueStopsTheCommandInsteadOfDrawing()
        {
            // Fail closed. The product catalogue degrades to empty on purpose; this one must not — drawing a
            // beam from dimensions that failed validation is worse than drawing nothing (I-36A F5).
            var source = Command;
            var handler = source.IndexOf("catch (StructuralSectionCatalogException", StringComparison.Ordinal);

            Assert.True(handler > 0, "El comando debe capturar StructuralSectionCatalogException.");

            var body = source.Substring(handler, Math.Min(400, source.Length - handler));
            Assert.Contains("no es valido", body, StringComparison.Ordinal);
            Assert.Contains("return;", body, StringComparison.Ordinal);
        }

        [Fact]
        public void TheCommandOpensTheInspectorAndInsertsExactlyTheAcceptedPlan()
        {
            var source = Command;

            Assert.Contains("new StructuralSectionInspectorWindow(catalog)", source, StringComparison.Ordinal);
            Assert.Contains("ShowModalWindow(window)", source, StringComparison.Ordinal);

            // Cancel leaves nothing behind, and the plan handed to AutoCAD is the very object the preview drew.
            Assert.Contains("if (window.Result == null)", source, StringComparison.Ordinal);
            Assert.Contains("StructuralSectionInsertService.Insert(document, window.Result)", source, StringComparison.Ordinal);
        }

        [Fact]
        public void TheCommandWarnsAboutTheDrawingUnits()
        {
            // The plan is in inches and nothing converts it, so the ADR-0005 advisory is the only thing between
            // the user and a drawing that silently means something else.
            Assert.Contains("RackUnitsGuard.WarnIfNotInches(document)", Command, StringComparison.Ordinal);
        }

        [Fact]
        public void TheInsertionPointIsAskedBeforeAnythingIsCreated()
        {
            var source = Command;
            var prompt = source.IndexOf("GetPoint(options)", StringComparison.Ordinal);
            var create = source.IndexOf("CreateBlockDefinition", StringComparison.Ordinal);

            Assert.True(prompt > 0 && create > 0, "Faltan la peticion de punto o la creacion del bloque.");
            Assert.True(prompt < create,
                "El punto se pide ANTES de crear nada: un jig cancelado despues de crear la definicion deja un " +
                "bloque fantasma que luego hay que limpiar.");
        }

        // ---- One transaction, or nothing ----------------------------------------------------------------

        [Fact]
        public void TheDefinitionAndTheReferenceCommitTogether()
        {
            var source = Command;
            var start = source.IndexOf("StartTransaction()", StringComparison.Ordinal);
            var definition = source.IndexOf("CreateBlockDefinition", StringComparison.Ordinal);
            var reference = source.IndexOf("InsertReference", StringComparison.Ordinal);
            var commit = source.IndexOf("transaction.Commit()", StringComparison.Ordinal);

            Assert.True(start > 0 && definition > start && reference > definition && commit > reference,
                "Definicion y referencia deben crearse dentro de la MISMA transaccion y confirmarse al final.");

            // Exactly one commit: a second one would mean a second, separately-visible step.
            Assert.Equal(commit, source.LastIndexOf("transaction.Commit()", StringComparison.Ordinal));
        }

        [Fact]
        public void AFailureLeavesNothingInTheDrawing()
        {
            var source = Command;
            var commit = source.IndexOf("transaction.Commit()", StringComparison.Ordinal);
            var handler = source.IndexOf("catch (System.Exception ex)", commit, StringComparison.Ordinal);

            Assert.True(handler > commit, "El insert debe tener su propio catch despues del commit.");

            var body = source.Substring(handler);
            Assert.Contains("StructuralSectionInsertOutcome.Failure", body, StringComparison.Ordinal);

            // No compensating cleanup, because there is nothing to clean: an uncommitted transaction never
            // reached the drawing. If someone ever adds an "undo what we drew" here, it means the shape of the
            // insert changed and this guard should be re-thought rather than deleted.
            Assert.DoesNotContain("Erase()", body, StringComparison.Ordinal);
        }

        [Fact]
        public void TheRegenGoesThroughTheOneCanonicalHelper()
        {
            Assert.Contains("SystemBlockWriter.ApplyRegen(document, regen: true)", Command, StringComparison.Ordinal);
            Assert.DoesNotContain("Editor.Regen()", CodeOnly(Command), StringComparison.Ordinal);
        }

        // ---- The block is INTERNAL ----------------------------------------------------------------------

        [Fact]
        public void TheBlockIsDefinedInThisDrawing()
        {
            var source = Materializer;

            Assert.Contains("new BlockTableRecord", source, StringComparison.Ordinal);
            Assert.Contains("blockTable.Add(definition)", source, StringComparison.Ordinal);
        }

        [Theory]
        [InlineData("blocks-library")]
        [InlineData("blocks.csv")]
        [InlineData("BlockLibraryImporter")]
        [InlineData("RackBlockFinder")]
        [InlineData("catalog.Blocks")]
        [InlineData("RackCatalog")]
        public void NothingInTheSectionAdapterDependsOnTheBlockLibrary(string forbidden)
        {
            // Owner decision: 983 sections cannot be 983 hand-drawn blocks. The whole point of a parametric
            // profile is that the drawing needs no library entry at all.
            Assert.DoesNotContain(forbidden, CodeOnly(Materializer), StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(forbidden, CodeOnly(Command), StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void TheBlockNameIsDerivedAndNeverResolved()
        {
            var source = Materializer;

            Assert.Contains("RACKCAD_SECCION_", source, StringComparison.Ordinal);

            // A name collision picks the next free name instead of redefining someone else's block. The name is
            // an OUTPUT: nothing is ever looked up by it, which is what "no library" means in practice.
            Assert.Contains("UniqueBlockName", source, StringComparison.Ordinal);
            Assert.Contains("blockTable.Has(", source, StringComparison.Ordinal);
            Assert.DoesNotContain("blockTable[", CodeOnly(source), StringComparison.Ordinal);
        }

        // ---- The adapter computes NOTHING ---------------------------------------------------------------

        [Theory]
        [InlineData("StructuralSectionGeometryFactory")]
        [InlineData("SectionDimensions")]
        [InlineData("GeometryBuilder")]
        [InlineData("PrismaticSectionInstance")]
        [InlineData("StructuralSectionPlanBuilder")]
        [InlineData("Math.")]
        public void TheAdapterNeverBuildsGeometryOfItsOwn(string forbidden)
        {
            // ADR-0022 §7: one generator. The adapter receives points and roles; if it ever reaches for a
            // dimension or a builder, the preview and the drawing have become two implementations that merely
            // agree today.
            Assert.DoesNotContain(forbidden, CodeOnly(Materializer), StringComparison.Ordinal);
        }

        [Fact]
        public void TheAdapterDrawsTheCurvesOfThePlanVerbatim()
        {
            var source = Materializer;

            Assert.Contains("foreach (var curve in plan.Curves)", source, StringComparison.Ordinal);
            Assert.Contains("curve.Points[i].X", source, StringComparison.Ordinal);
            Assert.Contains("polyline.Closed = curve.IsClosed", source, StringComparison.Ordinal);
        }

        [Fact]
        public void NothingIsScaledOnTheWayIntoTheDrawing()
        {
            // The plan is already in the internal unit (ADR-0005). A conversion here would be the silent
            // reinterpretation that ADR forbids.
            var source = Materializer;

            Assert.DoesNotContain("ScaleFactors", CodeOnly(source), StringComparison.Ordinal);
            Assert.DoesNotContain("25.4", CodeOnly(source), StringComparison.Ordinal);
            Assert.DoesNotContain("UnitsConversion", CodeOnly(source), StringComparison.Ordinal);
        }

        // ---- Styling -------------------------------------------------------------------------------------

        [Fact]
        public void ThePieceIsByblockOnLayerZero()
        {
            var source = Materializer;

            Assert.Contains("polyline.Layer = \"0\"", source, StringComparison.Ordinal);
            Assert.Contains("polyline.ColorIndex = 0", source, StringComparison.Ordinal);
            Assert.Contains("polyline.Linetype = \"ByBlock\"", source, StringComparison.Ordinal);
            Assert.Contains("LineWeight.ByBlock", source, StringComparison.Ordinal);
        }

        [Fact]
        public void TheAnnotationLayerIsTheSameOneTheRestOfRackcadUses()
        {
            // The literal is duplicated because the original is a private const in a file I-36B must not touch.
            // This guard is what keeps the duplication from drifting into two layers for one idea.
            Assert.Contains("\"RACKCAD_ANOTACIONES\"", Materializer, StringComparison.Ordinal);
            Assert.Contains("\"RACKCAD_ANOTACIONES\"", HeaderDrawer, StringComparison.Ordinal);

            Assert.Contains("LayerHelper.EnsureLayer(database, transaction, AnnotationLayer, 2)",
                Materializer, StringComparison.Ordinal);
        }

        [Fact]
        public void OnlyTheAxisAndTheEnvelopeAreTreatedAsAnnotations()
        {
            var source = Materializer;
            var marker = source.IndexOf("private static bool IsAnnotation", StringComparison.Ordinal);

            Assert.True(marker > 0, "Falta la regla que decide que es anotacion.");

            var body = source.Substring(marker, Math.Min(240, source.Length - marker));
            Assert.Contains("SectionCurveRole.Axis", body, StringComparison.Ordinal);
            Assert.Contains("SectionCurveRole.Envelope", body, StringComparison.Ordinal);
            Assert.DoesNotContain("OuterContour", body, StringComparison.Ordinal);
        }

        // ---- What this is NOT ----------------------------------------------------------------------------

        [Theory]
        [InlineData("Solid3d")]
        [InlineData("Region")]
        [InlineData("Extrude")]
        [InlineData("Sweep")]
        [InlineData("DynamicBlockReferenceProperty")]
        [InlineData("ApplyDynamicParameters")]
        public void TheRepresentationIsFlatGeometryAndNotADynamicBlock(string forbidden)
        {
            // Owner decisions 2 and 20: geometry is parametric IN CODE, and a wireframe — never a dynamic block
            // and never a 3D solid.
            Assert.DoesNotContain(forbidden, CodeOnly(Materializer), StringComparison.Ordinal);
            Assert.DoesNotContain(forbidden, CodeOnly(Command), StringComparison.Ordinal);
        }

        [Theory]
        [InlineData("RackBlockData")]
        [InlineData("RackEmbedDocument")]
        [InlineData("RackEmbedStore")]
        [InlineData("Guid")]
        [InlineData("RackSystemKind")]
        public void AnInsertedSectionIsNotARack(string forbidden)
        {
            // No payload, no GUID, no round-trip. It is geometry the user can measure and copy; turning it into
            // an editable member is I-37, in its own initiative.
            Assert.DoesNotContain(forbidden, CodeOnly(Materializer), StringComparison.Ordinal);
            Assert.DoesNotContain(forbidden, CodeOnly(Command), StringComparison.Ordinal);
        }

        [Theory]
        [InlineData("Cantilever")]
        [InlineData("StructuralMember")]
        [InlineData("Poste")]
        [InlineData("Larguero")]
        public void NothingHereReachesIntoI37(string forbidden)
        {
            Assert.DoesNotContain(forbidden, CodeOnly(Materializer), StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(forbidden, CodeOnly(Command), StringComparison.OrdinalIgnoreCase);
        }
    }
}
