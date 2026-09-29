using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using RackCad.Application.Views.Placement;
using RackCad.Plugin.Drawing;
using RackCad.Plugin.Systems.Shared;

namespace RackCad.Plugin.Views
{
    /// <summary>
    /// The caller-owned write scope of ONE RACKPROYECTAR operation: it owns the document lock and the ONE transaction
    /// (AUTH-15 owns neither). Every definition is created through <c>RackDefinitionCreator.CreateInTransaction</c> inside
    /// that transaction, every reference is appended here, and the only Commit of the operation is <see cref="Scope.Commit"/>.
    /// Disposing the scope without committing discards everything: no erase, purge or cleanup is written by hand.
    /// </summary>
    internal sealed class AutoCadProjectionWriteScopeFactory : IRackProjectionWriteScopeFactory
    {
        private readonly Document document;

        internal AutoCadProjectionWriteScopeFactory(Document document) => this.document = document;

        public IRackProjectionWriteScope Begin()
        {
            var database = document.Database;
            var documentLock = document.LockDocument();
            try
            {
                var transaction = database.TransactionManager.StartTransaction();
                return new Scope(database, documentLock, transaction);
            }
            catch
            {
                documentLock.Dispose();
                throw;
            }
        }

        private sealed class Scope : IRackProjectionWriteScope
        {
            private readonly Database database;
            private readonly DocumentLock documentLock;
            private readonly Transaction transaction;

            internal Scope(Database database, DocumentLock documentLock, Transaction transaction)
            {
                this.database = database;
                this.documentLock = documentLock;
                this.transaction = transaction;
            }

            public RackProjectionDefinitionResult CreateDefinition(RackProjectionPreparedView view)
            {
                RackDefinitionCreationResult result;
                switch (view.Family)
                {
                    case RackProjectionMaterializationFamily.HeaderRun:
                        result = RackDefinitionCreator.CreateInTransaction(
                            database, transaction, new LateralHeaderDrawer(), view.HeaderPlan, view.BaseName, view.Envelope);
                        break;
                    case RackProjectionMaterializationFamily.Cantilever:
                        result = RackDefinitionCreator.CreateInTransaction(
                            database, transaction, view.CantileverPlan, view.BaseName, view.Envelope);
                        break;
                    default:
                        return RackProjectionDefinitionResult.Failed(
                            RackProjectionWriteFailure.DefinitionCreationFailed, "familia sin creador de definicion");
                }

                if (result.IsSuccess)
                {
                    return RackProjectionDefinitionResult.Created(result.BlockName, result.DefinitionId);
                }

                // A family that observed an incomplete definition is a materialisation failure: the caller does not commit.
                if (result.Failure == RackDefinitionCreationFailure.MissingLibraryBlocks)
                {
                    return RackProjectionDefinitionResult.Failed(
                        RackProjectionWriteFailure.DefinitionIncomplete, result.Failure + ": " + result.Diagnostic);
                }

                return RackProjectionDefinitionResult.Failed(
                    RackProjectionWriteFailure.DefinitionCreationFailed, result.Failure + ": " + result.Diagnostic);
            }

            public RackProjectionReferenceResult PlaceReference(
                RackProjectionDefinitionResult definition, RackProjectedPlacement placement)
            {
                var definitionId = (ObjectId)definition.Handle;
                var modelSpace = (BlockTableRecord)transaction.GetObject(
                    SymbolUtilityServices.GetBlockModelSpaceId(database), OpenMode.ForWrite);

                // The presentation stays the current creation one (OD-8 A): only the position and the rotation are set.
                var reference = new BlockReference(
                    new Point3d(placement.Position.X, placement.Position.Y, placement.Position.Z), definitionId)
                {
                    Rotation = placement.RotationRadians,
                };
                modelSpace.AppendEntity(reference);
                transaction.AddNewlyCreatedDBObject(reference, true);
                return RackProjectionReferenceResult.Placed();
            }

            public void Commit() => transaction.Commit();

            public void Dispose()
            {
                transaction.Dispose();
                documentLock.Dispose();
            }
        }
    }
}
