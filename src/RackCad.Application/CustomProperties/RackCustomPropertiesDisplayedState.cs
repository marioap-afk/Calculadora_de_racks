using System;
using System.Collections.Generic;
using System.Linq;

namespace RackCad.Application.CustomProperties
{
    /// <summary>One member as it was shown: its handle and the canonical form of its collection (D-22.5).</summary>
    public sealed class RackCustomPropertiesDisplayedMember
    {
        internal RackCustomPropertiesDisplayedMember(string handle, string canonicalForm)
        {
            Handle = handle;
            CanonicalForm = canonicalForm;
        }

        public string Handle { get; }

        public string CanonicalForm { get; }
    }

    /// <summary>
    /// What the user saw of a writable rack, captured from the snapshot authority: the rack id and, per member, the
    /// canonical form of its collection. The commit compares it with the fresh read and aborts if the set of members or,
    /// for a unify, the form of ANY member changed (D-09.10, C-5B). Revalidating only the source is not enough.
    /// </summary>
    public sealed class RackCustomPropertiesDisplayedState
    {
        private RackCustomPropertiesDisplayedState(string rackId, IReadOnlyList<RackCustomPropertiesDisplayedMember> members)
        {
            RackId = rackId;
            Members = members;
        }

        public string RackId { get; }

        /// <summary>The members shown, in handle order.</summary>
        public IReadOnlyList<RackCustomPropertiesDisplayedMember> Members { get; }

        /// <summary>
        /// Captures what a Single or Divergent authority shows. Any other result has no write to confirm, so capturing it is a
        /// programming error.
        /// </summary>
        public static RackCustomPropertiesDisplayedState Capture(RackCustomPropertiesAuthorityResult authority)
        {
            if (authority == null)
            {
                throw new ArgumentNullException(nameof(authority));
            }

            if (authority.Outcome != RackCustomPropertiesAuthorityOutcome.Single
                && authority.Outcome != RackCustomPropertiesAuthorityOutcome.Divergent)
            {
                throw new InvalidOperationException(
                    "Solo se captura lo mostrado de un rack Single o Divergent: " + authority.Outcome + " no admite ninguna escritura.");
            }

            return new RackCustomPropertiesDisplayedState(
                authority.RackId,
                authority.Members.Select(member => new RackCustomPropertiesDisplayedMember(member.Handle, member.CanonicalForm)).ToList());
        }

        internal bool HasSameMembers(IReadOnlyList<RackCustomPropertiesMember> members)
            => Members.Select(member => member.Handle).SequenceEqual(members.Select(member => member.Handle), StringComparer.Ordinal);

        internal bool ShowsSameForms(IReadOnlyList<RackCustomPropertiesMember> members)
            => HasSameMembers(members)
               && Members.Select(member => member.CanonicalForm).SequenceEqual(members.Select(member => member.CanonicalForm), StringComparer.Ordinal);
    }

    /// <summary>
    /// A request to unify the rack from one view (D-09.10): the view the user chose explicitly, what every view looked like
    /// when the user confirmed, and whether the user confirmed. There is no default source.
    /// </summary>
    public sealed class RackCustomPropertiesUnifyIntent
    {
        private RackCustomPropertiesUnifyIntent(RackCustomPropertiesDisplayedState displayed, string sourceHandle, bool confirmed)
        {
            Displayed = displayed;
            SourceHandle = sourceHandle;
            Confirmed = confirmed;
        }

        public RackCustomPropertiesDisplayedState Displayed { get; }

        public string SourceHandle { get; }

        /// <summary>The user ticked the box after seeing the content of every view.</summary>
        public bool Confirmed { get; }

        public static RackCustomPropertiesUnifyIntent Create(RackCustomPropertiesDisplayedState displayed, string sourceHandle, bool confirmed)
        {
            if (displayed == null)
            {
                throw new ArgumentNullException(nameof(displayed));
            }

            if (string.IsNullOrWhiteSpace(sourceHandle))
            {
                throw new ArgumentException("Unificar exige elegir explícitamente la vista origen.", nameof(sourceHandle));
            }

            if (!displayed.Members.Any(member => string.Equals(member.Handle, sourceHandle, StringComparison.Ordinal)))
            {
                throw new ArgumentException("La vista origen no es una de las vistas mostradas del rack.", nameof(sourceHandle));
            }

            return new RackCustomPropertiesUnifyIntent(displayed, sourceHandle, confirmed);
        }
    }
}
