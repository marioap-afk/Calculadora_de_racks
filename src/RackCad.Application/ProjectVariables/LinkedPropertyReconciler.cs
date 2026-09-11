using System;
using System.Collections.Generic;
using RackCad.Application.Persistence;
using RackCad.Domain.Systems.Selective;

namespace RackCad.Application.ProjectVariables
{
    /// <summary>Why a reconciliation did or did not produce a final state.</summary>
    public enum LinkedPropertyReconcileOutcome
    {
        /// <summary>A complete authored document and a complete effective design.</summary>
        Reconciled = 1,

        /// <summary>The register could not be used as an authority of identity.</summary>
        RegistryNotUsable = 2,

        /// <summary>A declared final state names something this build cannot honour.</summary>
        InvalidFinalState = 3,

        /// <summary>The rack does not resolve completely once reconciled. Zero mutation, no fallback.</summary>
        NotResolvable = 4,
    }

    /// <summary>The reconciled rack, or the visible reason there is none.</summary>
    public sealed class LinkedPropertyReconciliation
    {
        private LinkedPropertyReconciliation(
            LinkedPropertyReconcileOutcome outcome,
            SelectivePalletDesignDocument authored,
            SelectivePalletDesign effective,
            string error)
        {
            Outcome = outcome;
            Authored = authored;
            Effective = effective;
            Error = error;
        }

        public LinkedPropertyReconcileOutcome Outcome { get; }

        /// <summary>The document to persist. Null on every failure — there is no partial result.</summary>
        public SelectivePalletDesignDocument Authored { get; }

        /// <summary>The design the geometry must reflect. Null on every failure.</summary>
        public SelectivePalletDesign Effective { get; }

        public string Error { get; }

        public bool IsSuccess => Outcome == LinkedPropertyReconcileOutcome.Reconciled;

        internal static LinkedPropertyReconciliation Success(
            SelectivePalletDesignDocument authored, SelectivePalletDesign effective)
            => new LinkedPropertyReconciliation(
                LinkedPropertyReconcileOutcome.Reconciled, authored, effective, null);

        internal static LinkedPropertyReconciliation Failed(LinkedPropertyReconcileOutcome outcome, string error)
            => new LinkedPropertyReconciliation(outcome, null, null, error);
    }

    /// <summary>
    /// Turns the editor's FINAL declared state into an authored document and an effective design
    /// (I-48 G4C, Proposal V2 R-03/R-10/R-14).
    ///
    /// <para>
    /// It receives a final state, never a sequence. The editor does NOT hand over
    /// <c>Unlink → SetLiteral → Link</c>: a session where the user links, unlinks and relinks would persist
    /// three intermediate documents, and the second of them freezes a literal nobody asked for. What the user
    /// expressed is where they ENDED, so that is what travels.
    /// </para>
    /// <para>
    /// The transitions it must honour, and each one is a test:
    /// </para>
    /// <list type="bullet">
    /// <item><c>Literal(a) → Literal(b)</c>: literal <c>b</c>, no binding, effective <c>b</c>.</item>
    /// <item><c>Literal(a) → Reference(X)</c>: binding X, frozen literal = the last COMMITTED literal,
    /// effective = value(X).</item>
    /// <item><c>Reference(X) → Reference(Y)</c>: binding Y, frozen literal preserved.</item>
    /// <item><c>Reference(X) → Reference(X)</c>: idempotent NO-OP for that property; the frozen literal is not
    /// rewritten.</item>
    /// <item><c>Reference(X) → Literal(b)</c>: binding removed, literal <c>b</c>, effective <c>b</c>.</item>
    /// </list>
    /// <para>
    /// It is ATOMIC and fail-closed. If ANY property of the rack — including one nobody edited — leaves the
    /// document unable to produce a complete effective design, nothing is returned at all. There is no
    /// fallback to a frozen literal, because a literal frozen when a binding was created is not the value in
    /// force (ADR-0034 §7, §8).
    /// </para>
    /// <para>
    /// A binding that was ALREADY broken when the editor opened never reaches here:
    /// <c>SelectiveEditorOpen</c> refuses to open, and repairing is an explicit rack-scoped operation.
    /// </para>
    /// </summary>
    public static class LinkedPropertyReconciler
    {
        /// <summary>
        /// Reconciles <paramref name="finalStates"/> onto <paramref name="initialAuthored"/> and resolves the
        /// result.
        /// </summary>
        /// <param name="initialAuthored">The document as it was when the editor opened.</param>
        /// <param name="editedDesign">
        /// The design the rest of the editor produced. Its value for a linked property is IGNORED: the final
        /// state is the authority for that property, so a control still showing the effective value cannot
        /// overwrite a frozen literal.
        /// </param>
        /// <param name="finalStates">The final declared state per property.</param>
        /// <param name="read">The register, which is accredited here.</param>
        /// <param name="id">The rack's identity. Null keeps the document's own, like the carrier's convention.</param>
        /// <param name="name">
        /// The rack's name, which the user can change while editing. Null keeps the document's own. It travels
        /// HERE rather than being patched onto the result, so the reconciled document is final and nobody has to
        /// run the carrier a second time — a second pass would be a second interpretation of the same value.
        /// </param>
        public static LinkedPropertyReconciliation Reconcile(
            SelectivePalletDesignDocument initialAuthored,
            SelectivePalletDesign editedDesign,
            IReadOnlyDictionary<PropertyId, LinkedPropertyEditState> finalStates,
            ProjectVariablesReadResult read,
            string id = null,
            string name = null)
        {
            if (initialAuthored == null)
            {
                throw new ArgumentNullException(nameof(initialAuthored));
            }

            if (editedDesign == null)
            {
                throw new ArgumentNullException(nameof(editedDesign));
            }

            var accreditation = UsableProjectVariablesRegistry.Accredit(read);

            if (!accreditation.IsUsable)
            {
                // Includes the ambiguous identity. Without an authority of identity there is no way to say what
                // a reference resolves to, so nothing is written.
                return LinkedPropertyReconciliation.Failed(
                    LinkedPropertyReconcileOutcome.RegistryNotUsable, accreditation.Error);
            }

            var registry = accreditation.Registry;
            var descriptors = SelectiveLinkedProperties.All;

            // The authored carrier is UPDATED, never rebuilt: the trip through the domain would drop the schema
            // version, the bindings and any field a later build wrote.
            var authored = initialAuthored.WithDesign(editedDesign, id, name);

            foreach (var pair in finalStates ?? new Dictionary<PropertyId, LinkedPropertyEditState>())
            {
                if (!descriptors.TryGetDescriptor(pair.Key, out var descriptor))
                {
                    return LinkedPropertyReconciliation.Failed(
                        LinkedPropertyReconcileOutcome.InvalidFinalState,
                        "La propiedad '" + pair.Key + "' no es vinculable en esta version.");
                }

                if (pair.Value == null)
                {
                    return LinkedPropertyReconciliation.Failed(
                        LinkedPropertyReconcileOutcome.InvalidFinalState,
                        "La propiedad '" + pair.Key + "' no declara un estado final.");
                }

                if (!Apply(authored, descriptor, pair.Value, out var error))
                {
                    return LinkedPropertyReconciliation.Failed(
                        LinkedPropertyReconcileOutcome.InvalidFinalState, error);
                }
            }

            // The rack has to resolve COMPLETELY. A property nobody edited can still make it unresolvable, and
            // then there is no design to draw and nothing to persist — no fallback to a frozen literal.
            var resolution = Resolver.ResolveAgainst(authored, registry);

            return resolution.IsSuccess
                ? LinkedPropertyReconciliation.Success(authored, resolution.Design)
                : LinkedPropertyReconciliation.Failed(
                    LinkedPropertyReconcileOutcome.NotResolvable, resolution.Error);
        }

        private static readonly Systems.Selective.SelectiveEffectiveDesignResolver Resolver =
            new Systems.Selective.SelectiveEffectiveDesignResolver();

        /// <summary>
        /// Writes ONE property's final state onto the carrier.
        ///
        /// <para>
        /// The frozen literal is written through the descriptor in BOTH branches, which is what keeps the
        /// reconciler generic: the special case it replaces wrote <c>VerticalClearance</c> by name, so no other
        /// property could ever have been reconciled by it.
        /// </para>
        /// </summary>
        private static bool Apply(
            SelectivePalletDesignDocument authored,
            SelectiveLinkedPropertyDescriptor descriptor,
            LinkedPropertyEditState state,
            out string error)
        {
            error = null;

            // The committed literal is the authority for this property, whatever the edited design carries: the
            // control shows the EFFECTIVE value of a bound property, so trusting the design here would copy a
            // variable's value over the literal the user froze.
            descriptor.WriteAuthored(authored, state.CommittedLiteral);

            if (!state.IsReference)
            {
                authored.PropertyValues?.Remove(descriptor.PropertyId.Value);
                return true;
            }

            authored.PropertyValues ??= new Dictionary<string, SelectivePropertyValueDocument>();
            authored.PropertyValues[descriptor.PropertyId.Value] =
                SelectivePropertyValueDocument.ToProjectVariable(state.Source.VariableId.Value);

            // A document that carries a binding is on the promoted line: an older build must refuse it rather
            // than read it as an unbound rack (I-47 G10).
            authored.SchemaVersion = SelectivePalletDesignDocument.PromotedSchemaVersion;
            return true;
        }
    }
}
