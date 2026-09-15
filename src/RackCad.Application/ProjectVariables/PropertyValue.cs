using System;
using System.Collections.Generic;
using RackCad.Application.Expressions;

namespace RackCad.Application.ProjectVariables
{
    /// <summary>How a property takes its value: literal, direct project-variable reference or bound expression.</summary>
    public enum PropertyValueKind
    {
        /// <summary>The scalar the user typed.</summary>
        Literal = 1,

        /// <summary>A reference to a project variable, which GOVERNS the effective value.</summary>
        ProjectVariableReference = 2,

        /// <summary>An already-bound expression evaluated against the project-variable snapshot.</summary>
        Expression = 3,
    }

    /// <summary>
    /// The value of a property: a literal, a typed project-variable reference or an already-bound expression.
    ///
    /// <para>
    /// The discrimination is explicit and carried by <see cref="Kind"/>, never inferred from a sentinel. That
    /// is the rule this type exists to enforce: <c>null</c>, a magic string, an empty GUID or a NaN must never
    /// mean "reference" or "absent", because a state nobody can name is a state that later reads as a
    /// successful value. Asking a reference for its literal FAILS instead of handing back <c>default(T)</c> —
    /// a silent zero reaching the geometry is precisely the failure this shape prevents.
    /// </para>
    /// <para>
    /// The expression case is a semantic tree, never source text. A future rack-property reference can still
    /// enter as another case without changing anything already stored.
    /// </para>
    /// <para>
    /// Equality is by value inside each case; values from different cases are never equal.
    /// </para>
    /// </summary>
    public sealed class PropertyValue<T> : IEquatable<PropertyValue<T>>
    {
        private readonly T _literal;
        private readonly VariableId _variableId;
        private readonly BoundExpression _expression;

        private PropertyValue(PropertyValueKind kind, T literal, VariableId variableId, BoundExpression expression)
        {
            Kind = kind;
            _literal = literal;
            _variableId = variableId;
            _expression = expression;
        }

        public PropertyValueKind Kind { get; }

        public bool IsLiteral => Kind == PropertyValueKind.Literal;

        public bool IsProjectVariableReference => Kind == PropertyValueKind.ProjectVariableReference;

        public bool IsExpression => Kind == PropertyValueKind.Expression;

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

        /// <summary>The bound expression. Throws for literal and direct-reference values.</summary>
        public BoundExpression ExpressionValue
            => IsExpression
                ? _expression
                : throw new InvalidOperationException(
                    "El valor de la propiedad no es una expresion (" + Kind + ").");

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
            => new PropertyValue<T>(PropertyValueKind.Literal, value, default, null);

        /// <summary>References a variable. Rejects an empty id: a reference to nothing is not a reference.</summary>
        public static PropertyValue<T> Reference(VariableId variableId)
        {
            if (variableId.IsEmpty)
            {
                throw new ArgumentException(
                    "Una referencia a variable de proyecto necesita un VariableId.",
                    nameof(variableId));
            }

            return new PropertyValue<T>(PropertyValueKind.ProjectVariableReference, default, variableId, null);
        }

        public static PropertyValue<T> Expression(BoundExpression expression)
            => new PropertyValue<T>(
                PropertyValueKind.Expression,
                default,
                default,
                expression ?? throw new ArgumentNullException(nameof(expression)));

        public bool Equals(PropertyValue<T> other)
        {
            if (other == null || Kind != other.Kind)
            {
                return false;
            }

            if (IsLiteral)
            {
                return EqualityComparer<T>.Default.Equals(_literal, other._literal);
            }

            return IsProjectVariableReference
                ? _variableId.Equals(other._variableId)
                : _expression.Equals(other._expression);
        }

        public override bool Equals(object obj) => Equals(obj as PropertyValue<T>);

        public override int GetHashCode()
            => IsLiteral
                ? (Kind, _literal == null ? 0 : EqualityComparer<T>.Default.GetHashCode(_literal)).GetHashCode()
                : IsProjectVariableReference
                    ? (Kind, _variableId).GetHashCode()
                    : (Kind, _expression).GetHashCode();

        public override string ToString()
            => IsLiteral
                ? "Literal(" + _literal + ")"
                : IsProjectVariableReference
                    ? "Reference(" + _variableId + ")"
                    : "Expression(" + _expression + ")";
    }
}
