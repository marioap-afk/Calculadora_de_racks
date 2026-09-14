using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using RackCad.Application.Persistence;

namespace RackCad.Application.CustomProperties
{
    /// <summary>Why a preflight rejected an intent, or a commit returned no plan (I-54 D-09.10, D-10 and D-22.5).</summary>
    public enum CustomPropertiesCommitRefusal
    {
        None = 0,

        /// <summary>The authority is not writable for this operation: not Single for an edit, or read-only for a unify.</summary>
        AuthorityNotWritable = 1,

        /// <summary>The picked definition is no longer in the drawing.</summary>
        SelectionMissing = 2,

        /// <summary>The picked definition now belongs to another rack.</summary>
        RackChanged = 3,

        /// <summary>A member was added or removed since the rack was shown.</summary>
        MemberSetChanged = 4,

        /// <summary>The collection of some member, source or destination, is no longer the one shown.</summary>
        DisplayedStateChanged = 5,

        /// <summary>A unify needs a Divergent rack.</summary>
        NotSafelyDivergent = 6,

        /// <summary>From that source, a unify would overwrite extension data or a higher minor.</summary>
        UnifyUnavailable = 7,

        /// <summary>A unify needs the user's confirmation after seeing every view.</summary>
        NotConfirmed = 8,

        /// <summary>The mutation itself was refused: entry not found, name taken, a limit, and so on.</summary>
        MutationRejected = 9,
    }

    /// <summary>One write of a plan: the definition's handle and the whole envelope to store in it.</summary>
    public sealed class CustomPropertiesPlanEntry
    {
        internal CustomPropertiesPlanEntry(string handle, string payload)
        {
            Handle = handle;
            Payload = payload;
        }

        public string Handle { get; }

        public string Payload { get; }
    }

    /// <summary>
    /// The result of a commit: the complete plan, or a refusal and no plan at all. A plan only exists once EVERY payload has
    /// been prepared, so the edge never receives part of one (D-22.10).
    /// </summary>
    public sealed class CustomPropertiesCommitResult
    {
        private static readonly IReadOnlyList<CustomPropertiesPlanEntry> NoPlan = Array.Empty<CustomPropertiesPlanEntry>();

        private CustomPropertiesCommitResult(
            CustomPropertiesCommitRefusal refusal,
            IReadOnlyList<CustomPropertiesPlanEntry> plan,
            CustomPropertiesDocument document,
            CustomPropertyId createdId,
            CustomPropertiesRejection mutationRejection,
            RackCustomPropertiesAuthorityOutcome? freshOutcome,
            string error)
        {
            Refusal = refusal;
            Plan = plan ?? NoPlan;
            Document = document;
            CreatedId = createdId;
            MutationRejection = mutationRejection;
            FreshOutcome = freshOutcome;
            Error = error;
        }

        public bool IsPlanned => Refusal == CustomPropertiesCommitRefusal.None;

        /// <summary>The writes, in handle order. Empty when refused.</summary>
        public IReadOnlyList<CustomPropertiesPlanEntry> Plan { get; }

        /// <summary>The collection every member holds once the plan is written. Null when refused.</summary>
        public CustomPropertiesDocument Document { get; }

        /// <summary>The identity minted by a create. Empty otherwise.</summary>
        public CustomPropertyId CreatedId { get; }

        public CustomPropertiesCommitRefusal Refusal { get; }

        /// <summary>The mutation's own reason when <see cref="Refusal"/> is <see cref="CustomPropertiesCommitRefusal.MutationRejected"/>.</summary>
        public CustomPropertiesRejection MutationRejection { get; }

        /// <summary>What the authority decided on the fresh read; null when it could not be evaluated.</summary>
        public RackCustomPropertiesAuthorityOutcome? FreshOutcome { get; }

        public string Error { get; }

        internal static CustomPropertiesCommitResult Planned(
            IReadOnlyList<CustomPropertiesPlanEntry> plan, CustomPropertiesDocument document, CustomPropertyId createdId, RackCustomPropertiesAuthorityOutcome freshOutcome)
            => new CustomPropertiesCommitResult(
                CustomPropertiesCommitRefusal.None, plan, document, createdId, CustomPropertiesRejection.None, freshOutcome, null);

        internal static CustomPropertiesCommitResult Refused(
            CustomPropertiesCommitRefusal refusal,
            string error,
            RackCustomPropertiesAuthorityOutcome? freshOutcome,
            CustomPropertiesRejection mutationRejection = CustomPropertiesRejection.None)
            => new CustomPropertiesCommitResult(refusal, null, null, default, mutationRejection, freshOutcome, error);
    }

    /// <summary>
    /// The Application half of a rack write (I-54 D-22 / ADR-0039 §8 and §9). When it is about to write, the edge scans
    /// again and hands over the FRESH projection; this re-evaluates everything from scratch — pick, membership, kind,
    /// authority, writable state and the intent itself — and returns a complete plan or a refusal. It never writes, and it
    /// never falls back on a name.
    ///
    /// <para>
    /// Each payload starts from that member's OWN fresh envelope and changes only its custom properties
    /// (<see cref="RackEmbedComposer.WithCustomProperties"/>). If serializing any of them throws — the pre-existing residual
    /// F-14b in another envelope field, for instance — the exception propagates and there is no plan: nothing half-prepared
    /// reaches the edge (D-22.10). The residual itself is not recovered.
    /// </para>
    /// </summary>
    public static class CustomPropertiesCommit
    {
        /// <summary>Create, rename, change value or delete, by id, on a rack that is still Single with the same members.</summary>
        public static CustomPropertiesCommitResult ForRack(
            IEnumerable<RackCustomPropertiesDefinition> fresh,
            RackCustomPropertiesSelection selection,
            Func<string, bool> isKnownKind,
            RackCustomPropertiesDisplayedState displayed,
            CustomPropertiesIntent intent)
        {
            if (displayed == null)
            {
                throw new ArgumentNullException(nameof(displayed));
            }

            if (intent == null)
            {
                throw new ArgumentNullException(nameof(intent));
            }

            var authority = FreshAuthority(fresh, selection, isKnownKind, out var missing);

            if (missing != null)
            {
                return missing;
            }

            if (authority.Outcome != RackCustomPropertiesAuthorityOutcome.Single)
            {
                return NotWritable(authority);
            }

            var identity = IdentityRefusal(authority, displayed);

            if (identity != null)
            {
                return identity;
            }

            var mutation = CustomPropertiesMutations.Apply(authority.Collection, intent);

            if (!mutation.Succeeded)
            {
                return CustomPropertiesCommitResult.Refused(
                    CustomPropertiesCommitRefusal.MutationRejected, mutation.Error, authority.Outcome, mutation.Rejection);
            }

            // Every member of the rack, from its own fresh envelope: an edit leaves them all canonically equal (INV-05).
            return CustomPropertiesCommitResult.Planned(
                PlanFor(authority.Members, mutation.Document), mutation.Document, mutation.CreatedId, authority.Outcome);
        }

        /// <summary>
        /// Unify from the source the user chose: only on a rack that is still safely Divergent, with the same members, every
        /// collection as it was shown and the user's confirmation. Only members canonically different from the source are
        /// written; the source and the members already equal to it are not rewritten (PR-07).
        /// </summary>
        public static CustomPropertiesCommitResult ForRackUnify(
            IEnumerable<RackCustomPropertiesDefinition> fresh,
            RackCustomPropertiesSelection selection,
            Func<string, bool> isKnownKind,
            RackCustomPropertiesUnifyIntent intent)
        {
            if (intent == null)
            {
                throw new ArgumentNullException(nameof(intent));
            }

            var authority = FreshAuthority(fresh, selection, isKnownKind, out var missing);

            if (missing != null)
            {
                return missing;
            }

            if (authority.Outcome != RackCustomPropertiesAuthorityOutcome.Divergent
                && authority.Outcome != RackCustomPropertiesAuthorityOutcome.Single)
            {
                return NotWritable(authority);
            }

            var refusal = UnifyRefusal(authority, intent, out var error);

            if (refusal != CustomPropertiesCommitRefusal.None)
            {
                return CustomPropertiesCommitResult.Refused(refusal, error, authority.Outcome);
            }

            var source = authority.Members.Single(member => string.Equals(member.Handle, intent.SourceHandle, StringComparison.Ordinal));
            var target = TargetOf(source);
            var destinations = authority.Members
                .Where(member => !string.Equals(member.CanonicalForm, source.CanonicalForm, StringComparison.Ordinal))
                .ToList();

            return CustomPropertiesCommitResult.Planned(PlanFor(destinations, target), target, default, authority.Outcome);
        }

        /// <summary>
        /// What a unify needs besides a writable authority, in the order the checks are reported: same rack, same members,
        /// every collection as shown, a Divergent rack, a safe source and the confirmation. Shared with the preflight.
        /// </summary>
        internal static CustomPropertiesCommitRefusal UnifyRefusal(
            RackCustomPropertiesAuthorityResult authority, RackCustomPropertiesUnifyIntent intent, out string error)
        {
            var identity = IdentityRefusal(authority, intent.Displayed);

            if (identity != null)
            {
                error = identity.Error;
                return identity.Refusal;
            }

            if (!intent.Displayed.ShowsSameForms(authority.Members))
            {
                error = "Las propiedades de alguna vista cambiaron desde que se mostraron: no se unifica nada.";
                return CustomPropertiesCommitRefusal.DisplayedStateChanged;
            }

            if (authority.Outcome != RackCustomPropertiesAuthorityOutcome.Divergent)
            {
                error = "Las vistas de este rack ya no tienen propiedades distintas: no hay nada que unificar.";
                return CustomPropertiesCommitRefusal.NotSafelyDivergent;
            }

            var option = authority.UnifyOptions.Single(candidate => string.Equals(candidate.SourceHandle, intent.SourceHandle, StringComparison.Ordinal));

            if (!option.IsAvailable)
            {
                error = "Unificar desde esa vista sobrescribiría contenido que este build no conoce o una versión más nueva en: "
                        + string.Join(", ", option.BlockingHandles) + ". No se unifica nada.";
                return CustomPropertiesCommitRefusal.UnifyUnavailable;
            }

            if (!intent.Confirmed)
            {
                error = "Unificar exige confirmar el contenido de todas las vistas.";
                return CustomPropertiesCommitRefusal.NotConfirmed;
            }

            error = null;
            return CustomPropertiesCommitRefusal.None;
        }

        private static RackCustomPropertiesAuthorityResult FreshAuthority(
            IEnumerable<RackCustomPropertiesDefinition> fresh,
            RackCustomPropertiesSelection selection,
            Func<string, bool> isKnownKind,
            out CustomPropertiesCommitResult missing)
        {
            if (fresh == null)
            {
                throw new ArgumentNullException(nameof(fresh));
            }

            if (selection == null)
            {
                throw new ArgumentNullException(nameof(selection));
            }

            var definitions = fresh.ToList();

            if (!selection.IsFromExternalReference
                && !definitions.Any(definition => definition != null
                                                  && string.Equals(definition.Handle, selection.DefinitionHandle, StringComparison.Ordinal)))
            {
                missing = CustomPropertiesCommitResult.Refused(
                    CustomPropertiesCommitRefusal.SelectionMissing,
                    "La definición elegida ya no está en el dibujo: no se escribe nada.",
                    null);
                return null;
            }

            missing = null;
            return RackCustomPropertiesAuthority.Evaluate(definitions, selection, isKnownKind);
        }

        private static CustomPropertiesCommitResult NotWritable(RackCustomPropertiesAuthorityResult authority)
            => CustomPropertiesCommitResult.Refused(
                CustomPropertiesCommitRefusal.AuthorityNotWritable,
                "La lectura actual del rack ya no permite esta escritura: " + authority.Error,
                authority.Outcome);

        /// <summary>The same rack, and exactly the members that were shown (no view added or removed).</summary>
        private static CustomPropertiesCommitResult IdentityRefusal(
            RackCustomPropertiesAuthorityResult authority, RackCustomPropertiesDisplayedState displayed)
        {
            if (!string.Equals(authority.RackId, displayed.RackId, StringComparison.OrdinalIgnoreCase))
            {
                return CustomPropertiesCommitResult.Refused(
                    CustomPropertiesCommitRefusal.RackChanged,
                    "El bloque elegido ya no pertenece al rack que se mostró: no se escribe nada.",
                    authority.Outcome);
            }

            if (!displayed.HasSameMembers(authority.Members))
            {
                return CustomPropertiesCommitResult.Refused(
                    CustomPropertiesCommitRefusal.MemberSetChanged,
                    "Las vistas del rack cambiaron desde que se mostraron (se añadió o se quitó alguna): no se escribe nada.",
                    authority.Outcome);
            }

            return null;
        }

        /// <summary>
        /// The document a unify writes (C-5A): the source's collection with its version resolved without downgrading, read
        /// again into a document nobody else holds; or, for an Absent source, the canonical empty document.
        /// </summary>
        private static CustomPropertiesDocument TargetOf(RackCustomPropertiesMember source)
        {
            if (source.Collection.Outcome == CustomPropertiesReadOutcome.Absent)
            {
                return CustomPropertiesDocument.CreateNew();
            }

            var document = new CustomPropertiesStore().ReadElement(source.Definition.Envelope.CustomProperties).Document;
            document.SchemaVersion = SchemaVersionPolicy.ResolveWriteVersion(document.SchemaVersion, CustomPropertiesDocument.CurrentSchemaVersion);
            return document;
        }

        /// <summary>
        /// Serializes the collection once and every member's envelope with it, before anything is returned. Any exception
        /// propagates: there is no partial plan.
        /// </summary>
        private static IReadOnlyList<CustomPropertiesPlanEntry> PlanFor(
            IEnumerable<RackCustomPropertiesMember> members, CustomPropertiesDocument document)
        {
            var text = new CustomPropertiesStore().Serialize(document);
            JsonElement collection;

            using (var parsed = JsonDocument.Parse(text))
            {
                collection = parsed.RootElement.Clone();
            }

            var store = new RackEmbedStore();
            var plan = new List<CustomPropertiesPlanEntry>();

            foreach (var member in members)
            {
                var envelope = RackEmbedComposer.WithCustomProperties(member.Definition.Envelope, collection);
                plan.Add(new CustomPropertiesPlanEntry(member.Handle, store.Serialize(envelope)));
            }

            return plan;
        }
    }

    /// <summary>The early answer of a preflight on the snapshot. Early feedback only: the commit decides (D-22).</summary>
    public sealed class CustomPropertiesPreflightResult
    {
        private CustomPropertiesPreflightResult(CustomPropertiesCommitRefusal refusal, CustomPropertiesRejection mutationRejection, string error)
        {
            Refusal = refusal;
            MutationRejection = mutationRejection;
            Error = error;
        }

        public bool IsAccepted => Refusal == CustomPropertiesCommitRefusal.None;

        public CustomPropertiesCommitRefusal Refusal { get; }

        public CustomPropertiesRejection MutationRejection { get; }

        public string Error { get; }

        internal static CustomPropertiesPreflightResult Accepted()
            => new CustomPropertiesPreflightResult(CustomPropertiesCommitRefusal.None, CustomPropertiesRejection.None, null);

        internal static CustomPropertiesPreflightResult Rejected(
            CustomPropertiesCommitRefusal refusal, string error, CustomPropertiesRejection mutationRejection = CustomPropertiesRejection.None)
            => new CustomPropertiesPreflightResult(refusal, mutationRejection, error);
    }

    /// <summary>
    /// The pure preflight over the snapshot the window was opened with (I-54 D-18.2 / D-22): it tells the user early
    /// whether an intent could go ahead. It is never authoritative and never writes; the commit re-evaluates everything on
    /// the fresh read.
    /// </summary>
    public static class CustomPropertiesPreflight
    {
        public static CustomPropertiesPreflightResult ForRack(RackCustomPropertiesAuthorityResult snapshot, CustomPropertiesIntent intent)
        {
            if (snapshot == null)
            {
                throw new ArgumentNullException(nameof(snapshot));
            }

            if (intent == null)
            {
                throw new ArgumentNullException(nameof(intent));
            }

            if (snapshot.Outcome != RackCustomPropertiesAuthorityOutcome.Single)
            {
                return CustomPropertiesPreflightResult.Rejected(CustomPropertiesCommitRefusal.AuthorityNotWritable, snapshot.Error);
            }

            var mutation = CustomPropertiesMutations.Apply(snapshot.Collection, intent);

            return mutation.Succeeded
                ? CustomPropertiesPreflightResult.Accepted()
                : CustomPropertiesPreflightResult.Rejected(CustomPropertiesCommitRefusal.MutationRejected, mutation.Error, mutation.Rejection);
        }

        public static CustomPropertiesPreflightResult ForRackUnify(RackCustomPropertiesAuthorityResult snapshot, RackCustomPropertiesUnifyIntent intent)
        {
            if (snapshot == null)
            {
                throw new ArgumentNullException(nameof(snapshot));
            }

            if (intent == null)
            {
                throw new ArgumentNullException(nameof(intent));
            }

            if (snapshot.Outcome != RackCustomPropertiesAuthorityOutcome.Divergent)
            {
                return CustomPropertiesPreflightResult.Rejected(
                    CustomPropertiesCommitRefusal.NotSafelyDivergent,
                    snapshot.Error ?? "Las vistas de este rack no tienen propiedades distintas: no hay nada que unificar.");
            }

            var refusal = CustomPropertiesCommit.UnifyRefusal(snapshot, intent, out var error);

            return refusal == CustomPropertiesCommitRefusal.None
                ? CustomPropertiesPreflightResult.Accepted()
                : CustomPropertiesPreflightResult.Rejected(refusal, error);
        }
    }
}
