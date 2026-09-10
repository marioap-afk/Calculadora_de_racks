using System;
using System.Collections.Generic;

namespace RackCad.Application.ProjectVariables
{
    /// <summary>How a property takes its value. ID22A has two cases; a rack-to-rack reference would add a third.</summary>
    public enum PropertyValueKind
    {
        /// <summary>The scalar the user typed.</summary>
        Literal = 1,

        /// <summary>A reference to a project variable, which GOVERNS the effective value.</summary>
        ProjectVariableReference = 2,
    }

    /// <summary>
    /// The value of a property: either the literal, or a typed reference to a project variable.
    ///
    /// <para>
    /// The discrimination is explicit and carried by <see cref="Kind"/>, never inferred from a sentinel. That
    /// is the rule this type exists to enforce: <c>null</c>, a magic string, an empty GUID or a NaN must never
    /// mean "reference" or "absent", because a state nobody can name is a state that later reads as a
    /// successful value. Asking a reference for its literal FAILS instead of handing back <c>default(T)</c> —
    /// a silent zero reaching the geometry is precisely the failure this shape prevents.
    /// </para>
    /// <para>
    /// The kind is also the extension point: a rack-property reference enters as one more case without
    /// changing the type of anything already stored. ID22A does not implement it.
    /// </para>
    /// <para>
    /// Equality is by value: two references to the same variable are the same value, and a literal is NEVER
    /// equal to a reference regardless of what that variable happens to be worth.
    /// </para>
    /// </summary>
    public sealed class PropertyValue<T> : IEquatable<PropertyValue<T>>
    {
        private readonly T _literal;
        private readonly VariableId _variableId;

        private PropertyValue(PropertyValueKind kind, T literal, VariableId variableId)
        {
            Kind = kind;
            _literal = literal;
            _variableId = variableId;
        }

        public PropertyValueKind Kind { get; }

        public bool IsLiteral => Kind == PropertyValueKind.Literal;

        public bool IsProjectVariableReference => Kind == PropertyValueKind.ProjectVariableReference;

        /// <summary>The literal. Throws when this value is a reference, rather than returning a default.</summary>
        public T LiteralValue
        {
            get
            {
                if (!IsLiteral)
                {
                    throw new InvalidOperationException(
                        "El valor de la propiedad no es un literal (" + Kind + "): no tiene valor propio que devolver.");
                }

                return _literal;
            }
        }

        /// <summary>The referenced variable. Throws when this value is a literal.</summary>
        public VariableId VariableId
        {
            get
            {
                if (!IsProjectVariableReference)
                {
                    throw new InvalidOperationException(
                        "El valor de la propiedad no es una referencia a variable de proyecto (" + Kind + ").");
                }

                return _variableId;
            }
        }

        public static PropertyValue<T> Literal(T value)
            => new PropertyValue<T>(PropertyValueKind.Literal, value, default);

        /// <summary>References a variable. Rejects an empty id: a reference to nothing is not a reference.</summary>
        public static PropertyValue<T> Reference(VariableId variableId)
        {
            if (variableId.IsEmpty)
            {
                throw new ArgumentException(
                    "Una referencia a variable de proyecto necesita un VariableId.",
                    nameof(variableId));
            }

            return new PropertyValue<T>(PropertyValueKind.ProjectVariableReference, default, variableId);
        }

        public bool Equals(PropertyValue<T> other)
        {
            if (other == null || Kind != other.Kind)
            {
                return false;
            }

            return IsLiteral
                ? EqualityComparer<T>.Default.Equals(_literal, other._literal)
                : _variableId.Equals(other._variableId);
        }

        public override bool Equals(object obj) => Equals(obj as PropertyValue<T>);

        public override int GetHashCode()
            => IsLiteral
                ? (Kind, _literal == null ? 0 : EqualityComparer<T>.Default.GetHashCode(_literal)).GetHashCode()
                : (Kind, _variableId).GetHashCode();

        public override string ToString()
            => IsLiteral ? "Literal(" + _literal + ")" : "Reference(" + _variableId + ")";
    }
}
