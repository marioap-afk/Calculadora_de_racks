using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using RackCad.Application.CustomProperties;
using RackCad.Application.Persistence;
using RackCad.Plugin.KindHandlers;
using RackCad.Plugin.Systems.Shared;

namespace RackCad.Plugin
{
    /// <summary>What a Project read found: the accredited collection and the workspace a window presents.</summary>
    internal sealed class CustomPropertiesProjectRead
    {
        internal CustomPropertiesProjectRead(CustomPropertiesReadResult collection, CustomPropertiesWorkspace workspace)
        {
            Collection = collection;
            Workspace = workspace;
        }

        /// <summary>The snapshot an early preflight may look at. A write is never applied to it.</summary>
        internal CustomPropertiesReadResult Collection { get; }

        internal CustomPropertiesWorkspace Workspace { get; }
    }

    /// <summary>How a Project write ended. Nothing reached the drawing unless <see cref="IsWritten"/>.</summary>
    internal sealed class CustomPropertiesProjectExecution
    {
        private CustomPropertiesProjectExecution(
            bool isWritten,
            CustomPropertiesReadOutcome freshOutcome,
            CustomPropertiesDocument document,
            CustomPropertyId createdId,
            CustomPropertiesRejection rejection,
            string error)
        {
            IsWritten = isWritten;
            FreshOutcome = freshOutcome;
            Document = document;
            CreatedId = createdId;
            Rejection = rejection;
            Error = error;
        }

        internal bool IsWritten { get; }

        /// <summary>How the entry read inside the write transaction.</summary>
        internal CustomPropertiesReadOutcome FreshOutcome { get; }

        /// <summary>The collection now stored. Null when nothing was written.</summary>
        internal CustomPropertiesDocument Document { get; }

        /// <summary>The identity minted by a create. Empty otherwise.</summary>
        internal CustomPropertyId CreatedId { get; }

        /// <summary>Why nothing was written: a fresh read that is not writable, or the mutation's own refusal.</summary>
        internal CustomPropertiesRejection Rejection { get; }

        internal string Error { get; }

        internal static CustomPropertiesProjectExecution Written(
            CustomPropertiesReadOutcome freshOutcome, CustomPropertiesDocument document, CustomPropertyId createdId)
            => new CustomPropertiesProjectExecution(true, freshOutcome, document, createdId, CustomPropertiesRejection.None, null);

        internal static CustomPropertiesProjectExecution NotWritten(
            CustomPropertiesReadOutcome freshOutcome, CustomPropertiesRejection rejection, string error)
            => new CustomPropertiesProjectExecution(false, freshOutcome, null, default, rejection, error);
    }

    /// <summary>What a Rack read found for a picked definition.</summary>
    internal sealed class CustomPropertiesRackRead
    {
        private CustomPropertiesRackRead(
            RackCustomPropertiesSelection selection, RackCustomPropertiesAuthorityResult authority, CustomPropertiesWorkspace workspace)
        {
            Selection = selection;
            Authority = authority;
            Workspace = workspace;
        }

        /// <summary>The pure selection of the pick, to hand back to the executors on write.</summary>
        internal RackCustomPropertiesSelection Selection { get; }

        /// <summary>What Application decided. Null when the picked definition carries no RackCad payload: there is no rack.</summary>
        internal RackCustomPropertiesAuthorityResult Authority { get; }

        /// <summary>The workspace of the rack. Null when there is no rack.</summary>
        internal CustomPropertiesWorkspace Workspace { get; }

        internal bool HasRack => Authority != null;

        internal static CustomPropertiesRackRead Of(
            RackCustomPropertiesSelection selection, RackCustomPropertiesAuthorityResult authority, CustomPropertiesWorkspace workspace)
            => new CustomPropertiesRackRead(selection, authority, workspace);

        internal static CustomPropertiesRackRead WithoutRack(RackCustomPropertiesSelection selection)
            => new CustomPropertiesRackRead(selection, null, null);
    }

    /// <summary>
    /// The PHYSICAL edge of custom properties (I-54 D-07.5, D-09.1 and D-22 / ADR-0039 §8 and §9): it reads the drawing into
    /// the pure inputs Application needs, and it writes back exactly what Application decided. It decides nothing itself.
    ///
    /// <para>
    /// <b>Project.</b> One transaction re-reads the Named Objects Dictionary entry, has the store accredit it, requires a
    /// writable read, applies the intent by id, asks the write guard, serializes and writes, and confirms once. Any
    /// refusal leaves without confirming, so the drawing is untouched.
    /// </para>
    /// <para>
    /// <b>Rack.</b> One transaction scans every definition with a RackCad payload, unreadable ones included, and flattens
    /// the scan for Application; the handle → definition map stays here and no <see cref="ObjectId"/> crosses. The commit
    /// of Application re-evaluates everything on that fresh projection and returns a complete plan or a refusal: on a
    /// refusal nothing is written and nothing is confirmed; on a plan every payload is written with
    /// <see cref="RackBlockData.Write"/> and the transaction is confirmed once. A plan only exists once every payload is
    /// serialized, so an exception while preparing one (the residual F-14b, for instance) propagates before any write.
    /// </para>
    /// <para>
    /// It only touches Xrecords: no regeneration, no redefinition, no block import, no purge. The known-kind predicate is
    /// built here, from the handler registry, and is the only thing the edge knows about kinds. No command or window
    /// reaches these entry points yet; physical behavior is validated in AutoCAD, not by any suite (ADR-0003).
    /// </para>
    /// </summary>
    internal static class CustomPropertiesExecutor
    {
        /// <summary>The known-kind predicate (C-1), constructed at the edge: Application receives the function, never the registry.</summary>
        private static readonly Func<string, bool> IsKnownKind = kind => KindHandlerRegistry.Default.TryGetIgnoreCase(kind, out _);

        /// <summary>Reads the Project collection: NOD entry → store → workspace, in one read transaction.</summary>
        internal static CustomPropertiesProjectRead ReadProject(Document document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            return InDocumentTransaction.Run(document, transaction =>
            {
                var collection = new CustomPropertiesStore().Read(CustomPropertiesData.Read(transaction, document.Database));
                return new CustomPropertiesProjectRead(collection, CustomPropertiesWorkspace.ForProject(collection));
            });
        }

        /// <summary>Applies one intent, by id, to the Project collection as it is NOW in the drawing (D-07.5).</summary>
        internal static CustomPropertiesProjectExecution ExecuteProject(Document document, CustomPropertiesIntent intent)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (intent == null)
            {
                throw new ArgumentNullException(nameof(intent));
            }

            var database = document.Database;
            var store = new CustomPropertiesStore();

            using (document.LockDocument())
            using (var transaction = database.TransactionManager.StartTransaction())
            {
                // Re-read inside THIS transaction and let Application accredit it: what a window showed is never what a
                // change is applied to, nor what the guard is asked about.
                var fresh = store.Read(CustomPropertiesData.Read(transaction, database));

                if (!fresh.CanWrite)
                {
                    return CustomPropertiesProjectExecution.NotWritten(fresh.Outcome, CustomPropertiesRejection.NotWritable, fresh.Error);
                }

                var mutation = CustomPropertiesMutations.Apply(fresh, intent);

                if (!mutation.Succeeded)
                {
                    return CustomPropertiesProjectExecution.NotWritten(fresh.Outcome, mutation.Rejection, mutation.Error);
                }

                if (!CustomPropertiesWriteGuard.CanOverwrite(fresh, out var refusal))
                {
                    return CustomPropertiesProjectExecution.NotWritten(fresh.Outcome, CustomPropertiesRejection.NotWritable, refusal);
                }

                CustomPropertiesData.Write(transaction, database, store.Serialize(mutation.Document));
                transaction.Commit();

                return CustomPropertiesProjectExecution.Written(fresh.Outcome, mutation.Document, mutation.CreatedId);
            }
        }

        /// <summary>
        /// Reads the rack of a picked block definition: scan → flat projection → authority → workspace, in one read
        /// transaction.
        /// </summary>
        internal static CustomPropertiesRackRead ReadRack(Document document, ObjectId definitionId)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            return InDocumentTransaction.Run(document, transaction =>
            {
                var selection = SelectionOf(transaction, definitionId);
                var snapshot = ScanProjection(transaction, document.Database, out var definitions);

                // The authority's one precondition (G5-CLOSE, precision A): a pick that is not an external reference has
                // to be in the scan. A definition with no RackCad payload never is, and then there is no rack to evaluate.
                if (!selection.IsFromExternalReference && !definitions.ContainsKey(selection.DefinitionHandle))
                {
                    return CustomPropertiesRackRead.WithoutRack(selection);
                }

                var authority = RackCustomPropertiesAuthority.Evaluate(snapshot, selection, IsKnownKind);
                return CustomPropertiesRackRead.Of(selection, authority, CustomPropertiesWorkspace.ForRack(authority));
            });
        }

        /// <summary>
        /// The pure selection of a picked block definition (D-09.4): its handle, and whether the definition itself comes from
        /// an external reference — the scan never includes those, so the pick has to carry the fact.
        /// </summary>
        internal static RackCustomPropertiesSelection SelectionOf(Transaction transaction, ObjectId definitionId)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException(nameof(transaction));
            }

            var record = (BlockTableRecord)transaction.GetObject(definitionId, OpenMode.ForRead);
            return new RackCustomPropertiesSelection(definitionId.Handle.ToString(), record.IsFromExternalReference);
        }

        /// <summary>Create, rename, change value or delete, by id, on the rack as it is NOW in the drawing (D-22).</summary>
        internal static CustomPropertiesCommitResult ExecuteRack(
            Document document,
            RackCustomPropertiesSelection selection,
            RackCustomPropertiesDisplayedState displayed,
            CustomPropertiesIntent intent)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            var database = document.Database;

            using (document.LockDocument())
            using (var transaction = database.TransactionManager.StartTransaction())
            {
                var fresh = ScanProjection(transaction, database, out var definitions);
                var result = CustomPropertiesCommit.ForRack(fresh, selection, IsKnownKind, displayed, intent);

                if (!result.IsPlanned)
                {
                    return result;
                }

                WritePlan(transaction, definitions, result.Plan);
                transaction.Commit();
                return result;
            }
        }

        /// <summary>Unify from the source the user chose, on the rack as it is NOW in the drawing (D-09.10, D-22).</summary>
        internal static CustomPropertiesCommitResult ExecuteRackUnify(
            Document document, RackCustomPropertiesSelection selection, RackCustomPropertiesUnifyIntent intent)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            var database = document.Database;

            using (document.LockDocument())
            using (var transaction = database.TransactionManager.StartTransaction())
            {
                var fresh = ScanProjection(transaction, database, out var definitions);
                var result = CustomPropertiesCommit.ForRackUnify(fresh, selection, IsKnownKind, intent);

                if (!result.IsPlanned)
                {
                    return result;
                }

                WritePlan(transaction, definitions, result.Plan);
                transaction.Commit();
                return result;
            }
        }

        /// <summary>
        /// The scan and its flat projection, in the caller's transaction (D-09.1, D-22.2): every definition with a RackCad
        /// payload, unreadable ones included, with no filter by rack, placement or kind — membership is Application's.
        /// </summary>
        private static IReadOnlyList<RackCustomPropertiesDefinition> ScanProjection(
            Transaction transaction, Database database, out IReadOnlyDictionary<string, ObjectId> definitions)
        {
            var map = new Dictionary<string, ObjectId>(StringComparer.Ordinal);
            var projection = new List<RackCustomPropertiesDefinition>();

            foreach (var envelope in RackBlockFinder.ScanEnvelopes(transaction, database, includeReferenceCount: true))
            {
                var record = (BlockTableRecord)transaction.GetObject(envelope.DefinitionId, OpenMode.ForRead);
                var handle = envelope.DefinitionId.Handle.ToString();

                map.Add(handle, envelope.DefinitionId);
                projection.Add(new RackCustomPropertiesDefinition(
                    handle, envelope.BlockName, envelope.DirectReferenceCount > 0, record.IsDependent, envelope.Embed));
            }

            definitions = map;
            return projection;
        }

        /// <summary>
        /// Writes a plan Application already prepared in full. Every handle is resolved BEFORE the first write, so a plan that
        /// names a definition the scan did not find fails before anything changes.
        /// </summary>
        private static void WritePlan(
            Transaction transaction, IReadOnlyDictionary<string, ObjectId> definitions, IReadOnlyList<CustomPropertiesPlanEntry> plan)
        {
            var targets = new List<(ObjectId Definition, string Payload)>(plan.Count);

            foreach (var entry in plan)
            {
                if (!definitions.TryGetValue(entry.Handle, out var definitionId))
                {
                    throw new InvalidOperationException(
                        "El plan nombra una definición que el barrido no encontró (handle " + entry.Handle + "): no se escribe nada.");
                }

                targets.Add((definitionId, entry.Payload));
            }

            foreach (var target in targets)
            {
                RackBlockData.Write(transaction, target.Definition, target.Payload);
            }
        }
    }
}
