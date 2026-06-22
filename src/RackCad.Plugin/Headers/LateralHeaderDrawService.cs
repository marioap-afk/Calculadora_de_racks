using System;
using System.Globalization;
using System.Text;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using RackCad.Application.Catalogs;
using RackCad.Application.Headers;
using RackCad.Domain.RackFrames;

namespace RackCad.Plugin.Headers
{
    /// <summary>
    /// AutoCAD-side orchestration for drawing a lateral header: it loads the catalog, builds the pure plan
    /// from the editor model, turns it into a single AutoCAD block, and lets the user place that block with
    /// the mouse (jig). The geometry itself stays in the pure Application layer; this is the only place that
    /// touches the AutoCAD API. It must run after any modal window has closed so the editor is interactive.
    /// </summary>
    public sealed class LateralHeaderDrawService
    {
        private readonly LateralHeaderLayoutBuilder builder = new LateralHeaderLayoutBuilder();
        private readonly LateralHeaderDrawer drawer = new LateralHeaderDrawer();

        public HeaderPlacementResult DrawAndPlace(Document document, RackFrameConfiguration configuration)
        {
            if (document == null)
            {
                return HeaderPlacementResult.Failure("No hay un dibujo activo en AutoCAD.");
            }

            if (configuration == null)
            {
                return HeaderPlacementResult.Failure("No hay configuracion para dibujar.");
            }

            try
            {
                var parameters = LateralHeaderParametersFactory.FromConfiguration(configuration);
                var catalog = LoadCatalog();
                var layout = builder.Build(configuration, parameters, catalog);
                var blockName = BuildBlockName(catalog, configuration);

                return PlaceLayout(document, catalog, layout, blockName);
            }
            catch (Exception ex)
            {
                return HeaderPlacementResult.Failure(ex.Message);
            }
        }

        /// <summary>
        /// Turn an already-built plan into one AutoCAD block and let the user place it with the mouse.
        /// Shared by the single header and the whole dynamic system.
        /// </summary>
        public HeaderPlacementResult PlaceLayout(Document document, RackCatalog catalog, LateralHeaderLayout layout, string blockName)
        {
            if (document == null)
            {
                return HeaderPlacementResult.Failure("No hay un dibujo activo en AutoCAD.");
            }

            try
            {
                var block = CreateBlock(document, layout, blockName);
                return PlaceAndReport(document, catalog, block);
            }
            catch (Exception ex)
            {
                return HeaderPlacementResult.Failure(ex.Message);
            }
        }

        /// <summary>Jig-place an already-created block and report the outcome (missing blocks by display name).</summary>
        public HeaderPlacementResult PlaceAndReport(Document document, RackCatalog catalog, LateralHeaderBlockResult block)
        {
            if (document == null)
            {
                return HeaderPlacementResult.Failure("No hay un dibujo activo en AutoCAD.");
            }

            try
            {
                var placed = PlaceBlockWithJig(document, block.DefinitionId);
                return new HeaderPlacementResult(true, placed, block.BlockName, DescribeMissing(catalog, block.Outcome), block.Outcome);
            }
            catch (Exception ex)
            {
                return HeaderPlacementResult.Failure(ex.Message);
            }
        }

        private LateralHeaderBlockResult CreateBlock(Document document, LateralHeaderLayout layout, string blockName)
        {
            var database = document.Database;

            using (document.LockDocument())
            using (var transaction = database.TransactionManager.StartTransaction())
            {
                var result = drawer.CreateHeaderBlock(database, transaction, layout, blockName);
                transaction.Commit();
                return result;
            }
        }

        /// <summary>Drag a reference of the block under the cursor; commit it where the user clicks.</summary>
        private static bool PlaceBlockWithJig(Document document, ObjectId blockDefinitionId)
        {
            var database = document.Database;
            var editor = document.Editor;

            using (document.LockDocument())
            using (var transaction = database.TransactionManager.StartTransaction())
            {
                var reference = new BlockReference(Point3d.Origin, blockDefinitionId);
                var jig = new HeaderInsertionJig(reference);
                var result = editor.Drag(jig);

                if (result.Status != PromptStatus.OK)
                {
                    reference.Dispose();
                    transaction.Commit(); // nothing added; the block definition remains for later reuse
                    return false;
                }

                var modelSpace = (BlockTableRecord)transaction.GetObject(
                    SymbolUtilityServices.GetBlockModelSpaceId(database), OpenMode.ForWrite);
                modelSpace.AppendEntity(reference);
                transaction.AddNewlyCreatedDBObject(reference, true);
                transaction.Commit();
                return true;
            }
        }

        private static string BuildBlockName(RackCatalog catalog, RackFrameConfiguration configuration)
        {
            var post = NormalizeWhitespace(catalog.DescribeId(configuration.LeftPost?.PostCatalogId));

            if (string.IsNullOrWhiteSpace(post))
            {
                post = "cabecera";
            }

            return string.Format(
                CultureInfo.InvariantCulture,
                "Cabecera {0} - F{1:0.##} A{2:0.##}",
                post,
                configuration.Depth,
                configuration.Height);
        }

        private static string[] DescribeMissing(RackCatalog catalog, LateralHeaderDrawOutcome outcome)
        {
            var lines = new string[outcome.MissingInstances.Count];

            for (var index = 0; index < outcome.MissingInstances.Count; index++)
            {
                var instance = outcome.MissingInstances[index];
                var displayName = NormalizeWhitespace(catalog.DescribeId(instance.PieceId));

                if (string.IsNullOrWhiteSpace(displayName))
                {
                    displayName = instance.Role.ToString();
                }

                lines[index] = string.IsNullOrWhiteSpace(instance.BlockName)
                    ? displayName + " (sin bloque definido en blocks.csv para la vista " + instance.View + ")"
                    : displayName + " (falta el bloque '" + instance.BlockName + "' en el dibujo)";
            }

            return lines;
        }

        public static RackCatalog LoadCatalog()
        {
            try
            {
                return JsonRackCatalogProvider.FromBaseDirectory().Load();
            }
            catch
            {
                return new RackCatalog();
            }
        }

        /// <summary>Collapses internal newlines/tabs/repeated spaces so a CSV display name reads on one line.</summary>
        private static string NormalizeWhitespace(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            var builder = new StringBuilder(value.Length);
            var previousWasSpace = false;

            foreach (var character in value)
            {
                if (char.IsWhiteSpace(character))
                {
                    if (!previousWasSpace)
                    {
                        builder.Append(' ');
                        previousWasSpace = true;
                    }
                }
                else
                {
                    builder.Append(character);
                    previousWasSpace = false;
                }
            }

            return builder.ToString().Trim();
        }

        /// <summary>Entity jig that keeps the header block under the cursor until the user picks a point.</summary>
        private sealed class HeaderInsertionJig : EntityJig
        {
            private Point3d position;

            public HeaderInsertionJig(BlockReference reference)
                : base(reference)
            {
            }

            protected override SamplerStatus Sampler(JigPrompts prompts)
            {
                var options = new JigPromptPointOptions("\nPunto de insercion de la cabecera: ")
                {
                    UserInputControls = UserInputControls.Accept3dCoordinates | UserInputControls.NullResponseAccepted
                };

                var result = prompts.AcquirePoint(options);

                if (result.Status != PromptStatus.OK)
                {
                    return SamplerStatus.Cancel;
                }

                if (result.Value.IsEqualTo(position))
                {
                    return SamplerStatus.NoChange;
                }

                position = result.Value;
                return SamplerStatus.OK;
            }

            protected override bool Update()
            {
                ((BlockReference)Entity).Position = position;
                return true;
            }
        }
    }

    /// <summary>Plain-data result the command uses to report what happened to the user.</summary>
    public sealed class HeaderPlacementResult
    {
        public HeaderPlacementResult(bool success, bool placed, string blockName, string[] missingBlocks, LateralHeaderDrawOutcome outcome)
        {
            Success = success;
            Placed = placed;
            BlockName = blockName;
            MissingBlocks = missingBlocks ?? Array.Empty<string>();
            Outcome = outcome;
        }

        public bool Success { get; }
        public bool Placed { get; }
        public string BlockName { get; }
        public string[] MissingBlocks { get; }
        public LateralHeaderDrawOutcome Outcome { get; }
        public string ErrorMessage { get; private set; }

        public bool HasMissingBlocks => MissingBlocks.Length > 0;

        public static HeaderPlacementResult Failure(string message) =>
            new HeaderPlacementResult(false, false, null, Array.Empty<string>(), null) { ErrorMessage = message };
    }
}
