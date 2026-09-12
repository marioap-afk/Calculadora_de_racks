using System;

namespace RackCad.Application.ProjectVariables
{
    /// <summary>
    /// What governs a linked property today.
    ///
    /// <para>
    /// It is an enum and not a <c>bool</c> deliberately (Proposal V2 R-03). A boolean admits no third variant,
    /// so the day a formula or a rack-to-rack reference arrives every surface that asked "is it a variable?"
    /// would have to be rewritten — and the ones nobody remembered would silently answer "literal" for a
    /// source they cannot read. Adding a member here makes those call sites a compile-time decision instead.
    /// </para>
    /// <para>
    /// No future kind is modelled now: this initiative persists nothing new.
    /// </para>
    /// </summary>
    public enum LinkedPropertySourceKind
    {
        /// <summary>The number belongs to this rack.</summary>
        Literal = 1,

        /// <summary>A project variable governs it, addressed by <see cref="VariableId"/>.</summary>
        ProjectVariableReference = 2,
    }

    /// <summary>
    /// WHO governs one property, as the editor declares it — never how it got there.
    ///
    /// <para>
    /// It carries no history. There is no <c>PreviousLiteral</c>, no <c>LiteralBeforeReference</c> and no
    /// gesture log: a final state plus the initial authored document is enough to reconcile, and a history
    /// would be a second, weaker description of the same thing that could disagree with it.
    /// </para>
    /// </summary>
    public sealed class LinkedPropertySource : IEquatable<LinkedPropertySource>
    {
        private readonly VariableId _variableId;

        private LinkedPropertySource(LinkedPropertySourceKind kind, VariableId variableId)
        {
            Kind = kind;
            _variableId = variableId;
        }

        public LinkedPropertySourceKind Kind { get; }

        /// <summary>
        /// The variable that governs the property. THROWS when this source is not a reference, rather than
        /// handing back a default that would read as "the empty variable".
        /// </summary>
        public VariableId VariableId
        {
            get
            {
                if (Kind != LinkedPropertySourceKind.ProjectVariableReference)
                {
                    throw new InvalidOperationException(
                        "La fuente de la propiedad no es una referencia a variable de proyecto (" + Kind + ").");
                }

                return _variableId;
            }
        }

        public bool IsReference => Kind == LinkedPropertySourceKind.ProjectVariableReference;

        public static LinkedPropertySource Literal { get; } =
            new LinkedPropertySource(LinkedPropertySourceKind.Literal, default);

        /// <summary>A reference to a concrete variable. An empty id is not an identity and is refused.</summary>
        public static LinkedPropertySource Reference(VariableId variableId)
        {
            if (variableId.IsEmpty)
            {
                throw new ArgumentException(
                    "Una referencia necesita un VariableId; el id vacio no es identidad.", nameof(variableId));
            }

            return new LinkedPropertySource(LinkedPropertySourceKind.ProjectVariableReference, variableId);
        }

        public bool Equals(LinkedPropertySource other)
            => other != null &&
               other.Kind == Kind &&
               (Kind != LinkedPropertySourceKind.ProjectVariableReference || other._variableId.Equals(_variableId));

        public override bool Equals(object obj) => Equals(obj as LinkedPropertySource);

        public override int GetHashCode()
            => Kind == LinkedPropertySourceKind.ProjectVariableReference
                ? _variableId.GetHashCode()
                : (int)Kind;

        public override string ToString()
            => Kind == LinkedPropertySourceKind.ProjectVariableReference
                ? "Reference(" + _variableId + ")"
                : "Literal";
    }
}
