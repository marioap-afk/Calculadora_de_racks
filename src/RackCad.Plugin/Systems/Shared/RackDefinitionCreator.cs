using System;
using Autodesk.AutoCAD.DatabaseServices;
using RackCad.Application.Drawing;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Cantilever;
using RackCad.Plugin.Drawing;
using RackCad.Plugin.Drawing.Cantilever;

namespace RackCad.Plugin.Systems.Shared
{
    /// <summary>
    /// AUTH-15 (I-52 owned, outside the shared view Foundation): create exactly ONE new RackCad block definition
    /// from an already-prepared plan and persist the already-composed envelope on it, INSIDE A TRANSACTION THE
    /// CALLER OWNS. It is the create-side sibling of <see cref="SystemBlockWriter.RedefineInTransaction"/>.
    ///
    /// <para>
    /// It does NOT commit, abort, open a transaction, take the document lock, import library blocks, regenerate
    /// or purge; it does NOT place a reference, choose a RackId, restamp or recompose the envelope, compute a
    /// transform or order anything. All of that belongs to the caller, before or after — which is what lets a
    /// caller put this definition, its reference and everything else of one operation under a SINGLE commit.
    /// One call creates one definition: there is no batch state here, so a caller's batch semantics stay the
    /// caller's.
    /// </para>
    /// <para>
    /// On failure nothing is cleaned up: whatever was written stays inside the caller's transaction and the
    /// caller rolls it back. Cleaning up here could only hide a half-written state from the one party that owns
    /// the transaction.
    /// </para>
    /// <para>
    /// Dispatch is by plan FAMILY, not by rack kind. Each family keeps its own drawer and its own block-name
    /// uniqueness policy (I-09: the two policies are deliberately NOT unified); the header-run family also
    /// creates, as the definition's content, one nested definition per distinct header — those carry no
    /// envelope.
    /// </para>
    /// </summary>
    internal static class RackDefinitionCreator
    {
        /// <summary>Header-run family: the system block of <paramref name="plan"/>, enveloped.</summary>
        internal static RackDefinitionCreationResult CreateInTransaction(
            Database database,
            Transaction transaction,
            LateralHeaderDrawer drawer,
            HeaderRunPlan plan,
            string requestedBlockName,
            RackEmbedDocument envelope)
        {
            var refusal = Precheck(database, transaction, plan == null || drawer == null, requestedBlockName, envelope, out var payloadJson);
            if (refusal.HasValue)
            {
                return refusal.Value;
            }

            try
            {
                var created = drawer.CreateSystemBlock(database, transaction, plan, requestedBlockName);

                // The family names the definition; AUTH-15 does not. But a "success" whose actual name is empty is not a usable
                // identity, whatever the family did to the requested name (RUN-2: "<>" came back as Success with an empty name).
                var unnamed = UnusableEffectiveName(created.BlockName);
                if (unnamed.HasValue)
                {
                    return unnamed.Value;
                }

                // A missing library block is omitted by the drawer and only reported. Here it is never silent:
                // the caller decides what it means, and the envelope is not written on an incomplete definition.
                if (created.Outcome.HasMissingBlocks)
                {
                    return RackDefinitionCreationResult.Missing(
                        created.Outcome.MissingInstances,
                        created.Outcome.MissingInstances.Count + " pieza(s) sin bloque en el dibujo");
                }

                return Envelope(transaction, created.DefinitionId, created.BlockName, payloadJson);
            }
            catch (System.Exception ex)
            {
                return RackDefinitionCreationResult.Failed(RackDefinitionCreationFailure.WriteFailed, ex.Message);
            }
        }

        /// <summary>Cantilever family: the view block of <paramref name="plan"/>, enveloped.</summary>
        internal static RackDefinitionCreationResult CreateInTransaction(
            Database database,
            Transaction transaction,
            CantileverViewPlan plan,
            string requestedBlockName,
            RackEmbedDocument envelope)
        {
            var refusal = Precheck(database, transaction, plan == null, requestedBlockName, envelope, out var payloadJson);
            if (refusal.HasValue)
            {
                return refusal.Value;
            }

            try
            {
                var definitionId = CantileverViewMaterializer.CreateBlockDefinitionNamed(
                    database, transaction, plan, requestedBlockName, out var blockName);

                var unnamed = UnusableEffectiveName(blockName);
                if (unnamed.HasValue)
                {
                    return unnamed.Value;
                }

                return Envelope(transaction, definitionId, blockName, payloadJson);
            }
            catch (System.Exception ex)
            {
                return RackDefinitionCreationResult.Failed(RackDefinitionCreationFailure.WriteFailed, ex.Message);
            }
        }

        /// <summary>
        /// POST-WRITE postcondition on AUTH-15's own result: the definition the family created must carry a non-empty actual name.
        /// Nothing is cleaned up here (whatever was written stays in the caller's transaction for its rollback) and no naming policy
        /// is applied or duplicated: the name is only READ, never derived. The requested name is validated as supplied (Precheck);
        /// what the family makes of it is the family's business, and this only refuses to call an unnamed definition a success.
        /// </summary>
        private static RackDefinitionCreationResult? UnusableEffectiveName(string effectiveName)
        {
            if (!string.IsNullOrWhiteSpace(effectiveName))
            {
                return null;
            }

            return RackDefinitionCreationResult.Failed(
                RackDefinitionCreationFailure.WriteFailed,
                "la familia devolvio un nombre efectivo vacio para la definicion; lo escrito queda en la transaccion del llamador para su rollback");
        }

        /// <summary>
        /// Every refusal that can be decided without writing, in the order the caller is most likely to get wrong.
        /// A refusal here guarantees nothing was written.
        /// </summary>
        private static RackDefinitionCreationResult? Precheck(
            Database database,
            Transaction transaction,
            bool invalidPlan,
            string requestedBlockName,
            RackEmbedDocument envelope,
            out string payloadJson)
        {
            payloadJson = null;

            // The caller's transaction must be the live top transaction of THIS live database: writing through any
            // other one would put the definition outside the scope the caller commits or rolls back.
            const string mismatch = "la transaccion no es la transaccion activa de la base de datos destino";

            if (database == null || database.IsDisposed || transaction == null || transaction.IsDisposed)
            {
                return RackDefinitionCreationResult.Failed(RackDefinitionCreationFailure.TransactionMismatch, mismatch);
            }

            // TopTransaction returns a NEW managed wrapper on every read, so wrapper identity never matches: the
            // identity that counts is the native transaction. An OpenCloseTransaction is never the top transaction
            // and stays unsupported on purpose.
            var top = database.TransactionManager.TopTransaction;

            if (top == null || top.UnmanagedObject != transaction.UnmanagedObject)
            {
                return RackDefinitionCreationResult.Failed(RackDefinitionCreationFailure.TransactionMismatch, mismatch);
            }

            if (invalidPlan)
            {
                return RackDefinitionCreationResult.Failed(RackDefinitionCreationFailure.InvalidPlan, "plan o dibujante ausente");
            }

            if (string.IsNullOrWhiteSpace(requestedBlockName))
            {
                return RackDefinitionCreationResult.Failed(RackDefinitionCreationFailure.InvalidBlockName, "nombre de bloque vacio");
            }

            // The envelope arrives already composed (identity, kind, view, design); it is validated, never altered.
            if (envelope == null
                || string.IsNullOrWhiteSpace(envelope.Id)
                || string.IsNullOrWhiteSpace(envelope.Kind))
            {
                return RackDefinitionCreationResult.Failed(RackDefinitionCreationFailure.InvalidEnvelope, "sobre ausente o sin Id/Kind");
            }

            try
            {
                payloadJson = new RackEmbedStore().Serialize(envelope);
            }
            catch (System.Exception ex)
            {
                return RackDefinitionCreationResult.Failed(RackDefinitionCreationFailure.InvalidEnvelope, ex.Message);
            }

            return null;
        }

        /// <summary>
        /// Write the envelope on the new definition and prove it landed. <see cref="RackBlockData.Write"/> returns
        /// silently on a null id or empty JSON, so the only evidence that the envelope exists is reading it back.
        /// </summary>
        private static RackDefinitionCreationResult Envelope(
            Transaction transaction, ObjectId definitionId, string blockName, string payloadJson)
        {
            RackBlockData.Write(transaction, definitionId, payloadJson);

            if (!string.Equals(RackBlockData.Read(transaction, definitionId), payloadJson, StringComparison.Ordinal))
            {
                return RackDefinitionCreationResult.Failed(
                    RackDefinitionCreationFailure.EnvelopeWriteFailed,
                    "el sobre leido de la definicion no coincide con el escrito");
            }

            return RackDefinitionCreationResult.Created(definitionId, blockName);
        }
    }
}
