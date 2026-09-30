using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using RackCad.Application;
using RackCad.Application.Catalogs;
using RackCad.Application.Diagnostics;
using RackCad.Application.Views.Placement;
using RackCad.Plugin.Systems.Shared;

namespace RackCad.Plugin.Drawing
{
    /// <summary>
    /// Shared AutoCAD block-placement infrastructure, extracted from <see cref="LateralHeaderDrawService"/>
    /// (I-16 F2) so every DrawService reuses ONE "place side": jig-drag a reference of an already-built
    /// definition, append it where the user clicks, report the outcome and missing pieces, and — when the user
    /// cancels — erase the leftover definition (and its private nested defs) so RACKEDITAR's GUID scan never
    /// finds a phantom view. The "write side" (definition create/redefine) stays in
    /// <see cref="RackCad.Plugin.Systems.Shared.SystemBlockWriter"/>; the geometry stays in <see cref="LateralHeaderDrawer"/>.
    /// Behaviour is byte-equivalent to the methods it was moved from.
    /// </summary>
    internal static class BlockPlacement
    {
        internal readonly struct JigPlacementResult
        {
            internal JigPlacementResult(PromptStatus status, ObjectId referenceId)
            {
                Status = status;
                ReferenceId = referenceId;
            }

            internal PromptStatus Status { get; }
            internal ObjectId ReferenceId { get; }
        }
        /// <summary>Jig-place an already-created block and report the outcome (missing blocks by display name).</summary>
        internal static HeaderPlacementResult PlaceAndReport(Document document, RackCatalog catalog, LateralHeaderBlockResult block)
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
                TryCleanupDefinition(document, block.DefinitionId);
                return HeaderPlacementResult.Failure(ex.Message);
            }
        }

        /// <summary>
        /// The PLACE side alone: jig-drag a reference of an already-created definition and, when the user cancels,
        /// erase the leftover definition. Returns the appended reference id (or <see cref="ObjectId.Null"/>).
        ///
        /// It exists for a system whose definition is NOT built from the block library — a Cantilever view is
        /// polylines, so it has no <c>LateralHeaderDrawOutcome</c> and nothing to report as "missing pieces", and
        /// <see cref="PlaceAndReport"/>'s header vocabulary would not describe it. The drag and the
        /// cancelled-insert cleanup are the same two methods either way: this is the seam, not a copy of them.
        /// </summary>
        internal static ObjectId PlaceDefinition(Document document, ObjectId definitionId, string prompt = null)
        {
            ObjectId placedId;
            try
            {
                placedId = PlaceBlockWithJig(document, definitionId, prompt);
            }
            catch
            {
                TryCleanupDefinition(document, definitionId);
                throw;
            }

            if (placedId.IsNull)
            {
                // Cancelled placement: the fresh definition — with the rack payload already embedded — would
                // linger as a phantom view that RACKEDITAR's GUID scan finds and redraws. Remove it.
                EraseUnreferencedDefinition(document, definitionId);
            }

            return placedId;
        }

        /// <summary>Jig only. G8 owns cancellation/exception cleanup so it can return a structured outcome.</summary>
        internal static ObjectId PlaceDefinitionWithoutCleanup(Document document, ObjectId definitionId, string prompt = null)
            => PlaceBlockWithJig(document, definitionId, prompt);

        /// <summary>ID18 placement seam that preserves OK, Cancel, None and Error instead of collapsing them to a null id.</summary>
        internal static JigPlacementResult PlaceDefinitionWithStatus(
            Document document, ObjectId definitionId, string prompt = null)
            => PlaceBlockWithJigStatus(document, definitionId, prompt, null);

        /// <summary>Mode-2 placement: prepared sibling writes share the first jig transaction after OK.</summary>
        internal static ObjectId PlaceDefinitionWithoutCleanup(
            Document document,
            ObjectId definitionId,
            Action<Transaction> beforeCommit,
            string prompt = null)
            => PlaceBlockWithJig(document, definitionId, prompt, beforeCommit);

        /// <summary>Best-effort orphan cleanup with an observable result for the G8 single-view seam.</summary>
        internal static RackSingleViewCleanupResult TryCleanupDefinition(Document document, ObjectId definitionId)
        {
            try
            {
                EraseUnreferencedDefinitionCore(document, definitionId);
                return new RackSingleViewCleanupResult(true);
            }
            catch (Exception ex)
            {
                RackLog.Exception("Limpiar definiciones tras insercion cancelada", ex);
                return new RackSingleViewCleanupResult(false, ex.Message);
            }
        }

        /// <summary>Append a reference to a block definition at a fixed point (no jig); returns the reference id.</summary>
        internal static ObjectId AppendReference(Document document, ObjectId blockDefinitionId, Point3d insertion)
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

        /// <summary>Erase a block DEFINITION nothing references (a cancelled insert's leftover), plus the private nested
        /// header defs it held — a grouped insert creates those, and erasing only the top-level def would orphan them in
        /// the block table. Best effort.</summary>
        private static void EraseUnreferencedDefinition(Document document, ObjectId definitionId)
        {
            TryCleanupDefinition(document, definitionId);
        }

        private static void EraseUnreferencedDefinitionCore(Document document, ObjectId definitionId)
        {
            if (definitionId.IsNull) return;

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

                LateralHeaderDrawer.PurgeUnreferenced(database, nestedDefs);
            }
        }

        /// <summary>Drag a reference of the block under the cursor; commit it where the user clicks. Returns the
        /// appended reference's id (or <see cref="ObjectId.Null"/> if the user cancelled) so the caller can tag it.</summary>
        private static ObjectId PlaceBlockWithJig(
            Document document,
            ObjectId blockDefinitionId,
            string prompt = null,
            Action<Transaction> beforeCommit = null)
        {
            return PlaceBlockWithJigStatus(document, blockDefinitionId, prompt, beforeCommit).ReferenceId;
        }

        private static JigPlacementResult PlaceBlockWithJigStatus(
            Document document,
            ObjectId blockDefinitionId,
            string prompt,
            Action<Transaction> beforeCommit)
        {
            var database = document.Database;
            var editor = document.Editor;

            using (document.LockDocument())
            using (var transaction = database.TransactionManager.StartTransaction())
            {
                var reference = new BlockReference(Point3d.Origin, blockDefinitionId);
                var jig = new HeaderInsertionJig(reference, prompt);
                var result = editor.Drag(jig);

                if (result.Status != PromptStatus.OK)
                {
                    reference.Dispose();
                    transaction.Commit(); // nothing added; the block definition remains for later reuse
                    return new JigPlacementResult(result.Status, ObjectId.Null);
                }

                beforeCommit?.Invoke(transaction);

                var modelSpace = (BlockTableRecord)transaction.GetObject(
                    SymbolUtilityServices.GetBlockModelSpaceId(database), OpenMode.ForWrite);
                modelSpace.AppendEntity(reference);
                transaction.AddNewlyCreatedDBObject(reference, true);
                var placedId = reference.ObjectId;
                transaction.Commit();
                return new JigPlacementResult(PromptStatus.OK, placedId);
            }
        }

        /// <summary>Entity jig that keeps the header block under the cursor until the user picks a point.</summary>
        private sealed class HeaderInsertionJig : EntityJig
        {
            private readonly string prompt;
            private Point3d position;

            /// <summary>A null <paramref name="prompt"/> keeps the historic cabecera wording, so every caller that
            /// existed before I-37D reads exactly what it always read.</summary>
            public HeaderInsertionJig(BlockReference reference, string prompt = null)
                : base(reference)
            {
                this.prompt = string.IsNullOrWhiteSpace(prompt) ? "\nPunto de insercion de la cabecera: " : prompt;
            }

            protected override SamplerStatus Sampler(JigPrompts prompts)
            {
                var options = new JigPromptPointOptions(prompt)
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
}
