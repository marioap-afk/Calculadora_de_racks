using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using RackCad.Application.Drawing;

namespace RackCad.Plugin.Systems.Shared
{
    /// <summary>
    /// Why <see cref="RackDefinitionCreator"/> refused or failed (AUTH-15). Every value other than <see cref="None"/>
    /// leaves whatever was already written inside the CALLER's transaction: the caller rolls back, AUTH-15 never
    /// cleans up and never commits.
    /// </summary>
    internal enum RackDefinitionCreationFailure
    {
        None,

        /// <summary>No database, or the transaction is null, disposed, or not the database's top transaction.
        /// Detected before any write.</summary>
        TransactionMismatch,

        /// <summary>No plan, or no drawer for the header-run family. Detected before any write.</summary>
        InvalidPlan,

        /// <summary>The requested block name is null or whitespace. Detected before any write.</summary>
        InvalidBlockName,

        /// <summary>The header-run drawer skipped pieces whose library block is not in the drawing. Never silently
        /// omitted: the missing instances are reported and the envelope is not written.</summary>
        MissingLibraryBlocks,

        /// <summary>The envelope is null, lacks Id/Kind/Name, or cannot be serialized. Detected before any write.</summary>
        InvalidEnvelope,

        /// <summary>The envelope read back from the new definition differs from the serialized envelope.</summary>
        EnvelopeWriteFailed,

        /// <summary>AutoCAD (or the family creator) threw while creating the definition or writing the envelope.</summary>
        WriteFailed,
    }

    /// <summary>
    /// The outcome of AUTH-15: on success, the ONE enveloped definition created inside the caller's transaction
    /// (its id and the actual name the family's uniqueness policy gave it); on failure, a typed reason.
    ///
    /// <para>
    /// Success does NOT mean committed — nothing here commits. The definition exists only inside the caller's
    /// transaction until the caller commits it, and disappears with everything else if the caller rolls back.
    /// </para>
    /// </summary>
    internal readonly struct RackDefinitionCreationResult
    {
        private static readonly IReadOnlyList<HeaderBlockInstance> NoMissing = new HeaderBlockInstance[0];

        private RackDefinitionCreationResult(
            bool isSuccess,
            ObjectId definitionId,
            string blockName,
            RackDefinitionCreationFailure failure,
            IReadOnlyList<HeaderBlockInstance> missingInstances,
            string diagnostic)
        {
            IsSuccess = isSuccess;
            DefinitionId = definitionId;
            BlockName = blockName;
            Failure = failure;
            MissingInstances = missingInstances ?? NoMissing;
            Diagnostic = diagnostic;
        }

        public bool IsSuccess { get; }

        /// <summary>The created, enveloped definition. <see cref="ObjectId.Null"/> on failure.</summary>
        public ObjectId DefinitionId { get; }

        /// <summary>The actual name after the family's uniqueness policy (it may carry a suffix). Null on failure.</summary>
        public string BlockName { get; }

        public RackDefinitionCreationFailure Failure { get; }

        /// <summary>The pieces the header-run drawer could not place. Empty unless
        /// <see cref="RackDefinitionCreationFailure.MissingLibraryBlocks"/>.</summary>
        public IReadOnlyList<HeaderBlockInstance> MissingInstances { get; }

        public string Diagnostic { get; }

        internal static RackDefinitionCreationResult Created(ObjectId definitionId, string blockName)
            => new RackDefinitionCreationResult(true, definitionId, blockName, RackDefinitionCreationFailure.None, null, null);

        internal static RackDefinitionCreationResult Failed(RackDefinitionCreationFailure failure, string diagnostic)
            => new RackDefinitionCreationResult(false, ObjectId.Null, null, failure, null, diagnostic);

        internal static RackDefinitionCreationResult Missing(IReadOnlyList<HeaderBlockInstance> missingInstances, string diagnostic)
            => new RackDefinitionCreationResult(
                false, ObjectId.Null, null, RackDefinitionCreationFailure.MissingLibraryBlocks, missingInstances, diagnostic);
    }
}
