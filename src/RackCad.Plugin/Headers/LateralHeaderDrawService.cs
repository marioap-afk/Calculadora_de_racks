using System;
using System.Collections.Generic;
using System.Globalization;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using RackCad.Application;
using RackCad.Application.Catalogs;
using RackCad.Application.Headers;
using RackCad.Application.Systems;
using RackCad.Domain.RackFrames;
using RackCad.Plugin.Systems;

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

        public HeaderPlacementResult DrawAndPlace(Document document, RackFrameConfiguration configuration, string payloadJson = null, string rackName = null, IReadOnlyList<HeaderBlockInstance> extraInstances = null)
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
                var layout = Merge(builder.Build(configuration, parameters, catalog), extraInstances);
                var blockName = string.IsNullOrWhiteSpace(rackName) ? BuildBlockName(catalog, configuration) : rackName.Trim();

                return PlaceLayout(document, catalog, layout, blockName, payloadJson);
            }
            catch (Exception ex)
            {
                return HeaderPlacementResult.Failure(ex.Message);
            }
        }

        /// <summary>Redraw an existing cabecera's block DEFINITION in place; every copy updates on regen. Extra
        /// instances (e.g. a selective corte's largueros) are drawn together with the cabecera. Pass
        /// <paramref name="regen"/> = false when redrawing several blocks in a loop and regen once at the end.</summary>
        public HeaderPlacementResult RedrawInPlace(Document document, ObjectId blockId, RackFrameConfiguration configuration, string payloadJson, IReadOnlyList<HeaderBlockInstance> extraInstances = null, bool regen = true)
        {
            if (document == null)
            {
                return HeaderPlacementResult.Failure("No hay un dibujo activo en AutoCAD.");
            }

            if (configuration == null || blockId.IsNull)
            {
                return HeaderPlacementResult.Failure("No hay cabecera para actualizar.");
            }

            try
            {
                var parameters = LateralHeaderParametersFactory.FromConfiguration(configuration);
                var catalog = LoadCatalog();
                var layout = Merge(builder.Build(configuration, parameters, catalog), extraInstances);
                var database = document.Database;

                LateralHeaderDrawOutcome outcome;
                IReadOnlyCollection<ObjectId> staleDefs;
                using (document.LockDocument())
                {
                    // ARRAY pattern: group identical pieces of the corte into nested defs referenced N times (same
                    // optimization as frontal/planta). The redefine creates fresh nested defs and purges the prior run's.
                    var plan = HeaderInstanceGrouper.Group(layout.Instances, ReadBlockName(database, blockId));
                    BlockLibraryImporter.EnsureForPlan(database, plan);

                    using (var transaction = database.TransactionManager.StartTransaction())
                    {
                        outcome = drawer.RedefineSystemBlock(database, transaction, blockId, plan, out staleDefs);
                        RackBlockData.Write(transaction, blockId, payloadJson);
                        transaction.Commit();
                    }

                    // Post-commit purge of the orphaned nested defs (Database.Purge on committed state; see the drawer note).
                    LateralHeaderDrawer.PurgeUnreferenced(database, staleDefs);

                    if (regen)
                    {
                        document.Editor.Regen();
                    }
                }

                // Report pieces skipped during the redraw too — an edit can lose blocks just like an insert.
                return new HeaderPlacementResult(true, true, null, DescribeMissing(catalog, outcome), outcome);
            }
            catch (Exception ex)
            {
                return HeaderPlacementResult.Failure(ex.Message);
            }
        }

        /// <summary>
        /// Draw one cabecera as its own block at a GIVEN point (no jig), embedding its payload on the definition.
        /// Used to lay out a selective run's lateral "cortes" (one cabecera block per post) in a single action.
        /// </summary>
        public HeaderPlacementResult DrawAt(Document document, RackFrameConfiguration configuration, Point3d insertion, string payloadJson = null, string rackName = null)
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
                var blockName = string.IsNullOrWhiteSpace(rackName) ? BuildBlockName(catalog, configuration) : rackName.Trim();

                var block = CreateBlock(document, layout, blockName, payloadJson);
                var placedId = AppendReference(document, block.DefinitionId, insertion);

                return new HeaderPlacementResult(true, !placedId.IsNull, block.BlockName, DescribeMissing(catalog, block.Outcome), block.Outcome)
                {
                    PlacedId = placedId
                };
            }
            catch (Exception ex)
            {
                return HeaderPlacementResult.Failure(ex.Message);
            }
        }

        /// <summary>Merge extra loose instances (e.g. a selective corte's largueros) into the cabecera layout so they
        /// draw as one block. Returns the original layout when there is nothing to add.</summary>
        private static LateralHeaderLayout Merge(LateralHeaderLayout layout, IReadOnlyList<HeaderBlockInstance> extra)
        {
            if (extra == null || extra.Count == 0)
            {
                return layout;
            }

            var all = new List<HeaderBlockInstance>(layout.Instances);
            all.AddRange(extra);
            return new LateralHeaderLayout(all, layout.HorizontalLength, layout.HorizontalCount, layout.DiagonalCount, layout.ClosingGap);
        }

        /// <summary>Append a reference to a block definition at a fixed point (no jig); returns the reference id.</summary>
        private static ObjectId AppendReference(Document document, ObjectId blockDefinitionId, Point3d insertion)
        {
            var database = document.Database;

            using (document.LockDocument())
            using (var transaction = database.TransactionManager.StartTransaction())
            {
                var modelSpace = (BlockTableRecord)transaction.GetObject(
                    SymbolUtilityServices.GetBlockModelSpaceId(database), OpenMode.ForWrite);
                var reference = new BlockReference(insertion, blockDefinitionId);
                modelSpace.AppendEntity(reference);
                transaction.AddNewlyCreatedDBObject(reference, true);
                var placedId = reference.ObjectId;
                transaction.Commit();
                return placedId;
            }
        }

        /// <summary>
        /// Turn an already-built plan into one AutoCAD block and let the user place it with the mouse.
        /// Shared by the single header and the whole dynamic system.
        /// </summary>
        public HeaderPlacementResult PlaceLayout(Document document, RackCatalog catalog, LateralHeaderLayout layout, string blockName, string payloadJson = null)
        {
            if (document == null)
            {
                return HeaderPlacementResult.Failure("No hay un dibujo activo en AutoCAD.");
            }

            try
            {
                var block = CreateBlock(document, layout, blockName, payloadJson);
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
                var placedId = PlaceBlockWithJig(document, block.DefinitionId);

                if (placedId.IsNull)
                {
                    // Cancelled placement: the fresh definition — with the rack payload already embedded —
                    // would linger as a phantom view that RACKEDITAR's GUID scan finds and redraws. Remove it.
                    EraseUnreferencedDefinition(document, block.DefinitionId);
                }

                return new HeaderPlacementResult(true, !placedId.IsNull, block.BlockName, DescribeMissing(catalog, block.Outcome), block.Outcome)
                {
                    PlacedId = placedId
                };
            }
            catch (Exception ex)
            {
                return HeaderPlacementResult.Failure(ex.Message);
            }
        }

        /// <summary>Erase a block DEFINITION nothing references (a cancelled insert's leftover), plus the private nested
        /// header defs it held — a grouped insert creates those, and erasing only the top-level def would orphan them in
        /// the block table. Best effort.</summary>
        private static void EraseUnreferencedDefinition(Document document, ObjectId definitionId)
        {
            if (definitionId.IsNull)
            {
                return;
            }

            try
            {
                var database = document.Database;
                var nestedDefs = new List<ObjectId>();

                using (document.LockDocument())
                {
                    using (var transaction = database.TransactionManager.StartTransaction())
                    {
                        var record = (BlockTableRecord)transaction.GetObject(definitionId, OpenMode.ForRead);
                        var references = record.GetBlockReferenceIds(directOnly: true, forceValidity: false);

                        if (references == null || references.Count == 0)
                        {
                            // Capture the nested header defs this block referenced BEFORE erasing it, so they can be
                            // purged too (a cancelled grouped insert would otherwise leave them unreferenced).
                            foreach (ObjectId id in record)
                            {
                                if (transaction.GetObject(id, OpenMode.ForRead) is BlockReference nested && !nested.BlockTableRecord.IsNull)
                                {
                                    nestedDefs.Add(nested.BlockTableRecord);
                                }
                            }

                            record.UpgradeOpen();
                            record.Erase();
                        }

                        transaction.Commit();
                    }

                    // Post-commit: with the top-level def's references erased, its private nested defs are now purgeable.
                    LateralHeaderDrawer.PurgeUnreferenced(database, nestedDefs);
                }
            }
            catch
            {
                // Best effort: a leftover definition is preferable to failing the whole command here.
            }
        }

        private LateralHeaderBlockResult CreateBlock(Document document, LateralHeaderLayout layout, string blockName, string payloadJson = null)
        {
            var database = document.Database;

            // ARRAY pattern: collapse identical pieces (largueros, postes…) into nested defs referenced N times, so N
            // pieces cost ONE dynamic-parameter evaluation instead of N (see the autocad-insert-perf memory). Singletons,
            // annotations and cotas stay loose. Matters most for a selective corte with many equal largueros per nivel.
            var plan = HeaderInstanceGrouper.Group(layout.Instances, blockName);

            using (document.LockDocument())
            {
                // Import any block definitions the drawing is missing (from the library DWG) before drawing.
                BlockLibraryImporter.EnsureForPlan(database, plan);

                using (var transaction = database.TransactionManager.StartTransaction())
                {
                    var result = drawer.CreateSystemBlock(database, transaction, plan, blockName);

                    // Payload on the DEFINITION so every reference/copy shares it and the cabecera can be reopened.
                    if (!string.IsNullOrEmpty(payloadJson))
                    {
                        RackBlockData.Write(transaction, result.DefinitionId, payloadJson);
                    }

                    transaction.Commit();

                    // CreateSystemBlock reports an EMPTY layout and an InsertedCount that also tallies the nested-def
                    // prototypes (proto + N refs), which would inflate the single cabecera's "N piezas". Rebuild the
                    // outcome with the REAL layout (for the horizontal/diagonal counts RackFrameCommands.Cabecera reads)
                    // and the real drawn-piece count (all pieces present = layout size; less any whose block was missing).
                    var drawnPieces = layout.Instances.Count - result.Outcome.MissingInstances.Count;
                    var outcome = new LateralHeaderDrawOutcome(layout, drawnPieces, result.Outcome.MissingInstances);
                    return new LateralHeaderBlockResult(result.DefinitionId, result.BlockName, outcome);
                }
            }
        }

        /// <summary>Best-effort read of a block definition's name (used as a stable nested-def prefix on redraw); "Corte" on failure.</summary>
        private static string ReadBlockName(Database database, ObjectId blockId)
        {
            try
            {
                using (var transaction = database.TransactionManager.StartOpenCloseTransaction())
                {
                    var name = ((BlockTableRecord)transaction.GetObject(blockId, OpenMode.ForRead)).Name;
                    transaction.Commit();
                    return string.IsNullOrWhiteSpace(name) ? "Corte" : name;
                }
            }
            catch
            {
                return "Corte";
            }
        }

        /// <summary>Drag a reference of the block under the cursor; commit it where the user clicks. Returns the
        /// appended reference's id (or <see cref="ObjectId.Null"/> if the user cancelled) so the caller can tag it.</summary>
        private static ObjectId PlaceBlockWithJig(Document document, ObjectId blockDefinitionId)
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
                    return ObjectId.Null;
                }

                var modelSpace = (BlockTableRecord)transaction.GetObject(
                    SymbolUtilityServices.GetBlockModelSpaceId(database), OpenMode.ForWrite);
                modelSpace.AppendEntity(reference);
                transaction.AddNewlyCreatedDBObject(reference, true);
                var placedId = reference.ObjectId;
                transaction.Commit();
                return placedId;
            }
        }

        private static string BuildBlockName(RackCatalog catalog, RackFrameConfiguration configuration)
        {
            var post = BlockNaming.NormalizeWhitespace(catalog.DescribeId(configuration.LeftPost?.PostCatalogId));

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

        /// <summary>Human-readable lines for the pieces a draw skipped (shared by every draw service).</summary>
        internal static string[] DescribeMissing(RackCatalog catalog, LateralHeaderDrawOutcome outcome)
        {
            var lines = new string[outcome.MissingInstances.Count];

            for (var index = 0; index < outcome.MissingInstances.Count; index++)
            {
                var instance = outcome.MissingInstances[index];
                var displayName = BlockNaming.NormalizeWhitespace(catalog.DescribeId(instance.PieceId));

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

        /// <summary>Id of the placed top-level block reference (<see cref="ObjectId.Null"/> if not placed) — used to tag it with the rack payload.</summary>
        public ObjectId PlacedId { get; set; }

        public bool HasMissingBlocks => MissingBlocks.Length > 0;

        public static HeaderPlacementResult Failure(string message) =>
            new HeaderPlacementResult(false, false, null, Array.Empty<string>(), null) { ErrorMessage = message };
    }
}
