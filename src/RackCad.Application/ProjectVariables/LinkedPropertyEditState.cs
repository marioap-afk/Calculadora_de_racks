using System;

namespace RackCad.Application.ProjectVariables
{
    /// <summary>
    /// The FINAL state of one linked property, as the editor declares it (Proposal V2 R-03).
    ///
    /// <para>
    /// Two fields and no more. <see cref="CommittedLiteral"/> is the last literal the user actually
    /// COMMITTED: with <see cref="LinkedPropertySourceKind.Literal"/> it is the value in force, and with a
    /// reference it is the literal frozen behind it. That single meaning is what makes the 20.13 rule one rule
    /// instead of a special case — linking freezes <see cref="CommittedLiteral"/>, whatever it happens to be.
    /// </para>
    /// <para>
    /// A draft is NOT here. Drafts belong to the session that is still being edited
    /// (<see cref="LinkedPropertyEditSession"/>); what crosses into Application to be reconciled is only what
    /// was committed. Carrying a draft across would make "the last thing typed" competitive with "the last
    /// thing committed", which is precisely the ambiguity 20.13 resolves.
    /// </para>
    /// </summary>
    public sealed class LinkedPropertyEditState : IEquatable<LinkedPropertyEditState>
    {
        private LinkedPropertyEditState(double committedLiteral, LinkedPropertySource source)
        {
            CommittedLiteral = committedLiteral;
            Source = source;
        }

        /// <summary>The last literal COMMITTED — active when the source is a literal, frozen when it is a reference.</summary>
        public double CommittedLiteral { get; }

        public LinkedPropertySource Source { get; }

        public bool IsReference => Source.IsReference;

        /// <summary>The number belongs to the rack.</summary>
        public static LinkedPropertyEditState Literal(double value) => Create(value, LinkedPropertySource.Literal);

        /// <summary>
        /// A variable governs the property and <paramref name="frozenLiteral"/> stays behind it — the value that
        /// governs again the day the binding is removed.
        /// </summary>
        public static LinkedPropertyEditState Reference(double frozenLiteral, VariableId variableId)
            => Create(frozenLiteral, LinkedPropertySource.Reference(variableId));

        public static LinkedPropertyEditState Create(double committedLiteral, LinkedPropertySource source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (double.IsNaN(committedLiteral) || double.IsInfinity(committedLiteral))
            {
                throw new ArgumentException(
                    "El literal comprometido no es finito, asi que no es un valor de diseno.", nameof(committedLiteral));
            }

            return new LinkedPropertyEditState(committedLiteral, source);
        }

        public bool Equals(LinkedPropertyEditState other)
            => other != null && other.CommittedLiteral.Equals(CommittedLiteral) && other.Source.Equals(Source);

        public override bool Equals(object obj) => Equals(obj as LinkedPropertyEditState);

        public override int GetHashCode() => CommittedLiteral.GetHashCode() ^ Source.GetHashCode();

        public override string ToString() => Source + " / frozen " + CommittedLiteral;
    }
}
