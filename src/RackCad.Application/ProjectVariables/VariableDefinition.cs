using System;
using RackCad.Application.Expressions;

namespace RackCad.Application.ProjectVariables
{
    /// <summary>WHERE a project variable's value comes from: a literal or an already-bound expression.</summary>
    public enum VariableDefinitionKind
    {
        /// <summary>The value is written down.</summary>
        Literal = 1,

        /// <summary>The value is computed from the persisted bound semantic tree.</summary>
        Expression = 2,
    }

    /// <summary>
    /// The definition of a project variable, discriminated by <see cref="Kind"/> from day one.
    ///
    /// <para>
    /// Expression is additive over the historical literal case: it does not raise the major schema and does
    /// not change how a property references a variable. The tree is already bound semantic authority; this
    /// union carries no formula text and performs no name lookup.
    /// </para>
    /// <para>
    /// It is a class, not a struct, so there is no default-constructed instance declaring a kind nobody set.
    /// Equality is by value for both closed cases.
    /// </para>
    /// </summary>
    public sealed class VariableDefinition : IEquatable<VariableDefinition>
    {
        private readonly double _literal;
        private readonly BoundExpression _expression;

        private VariableDefinition(VariableDefinitionKind kind, double literal, BoundExpression expression)
        {
            Kind = kind;
            _literal = literal;
            _expression = expression;
        }

        /// <summary>The bound expression. Throws when this definition is a literal.</summary>
        public BoundExpression ExpressionValue
            => Kind == VariableDefinitionKind.Expression
                ? _expression
                : throw new InvalidOperationException(
                    "La definicion de la variable es un literal: no contiene una expresion.");

        public VariableDefinitionKind Kind { get; }

        /// <summary>The literal value. Throws when this definition is not a literal, rather than returning a default.</summary>
        public double LiteralValue
        {
            get
            {
                if (Kind != VariableDefinitionKind.Literal)
                {
                    throw new InvalidOperationException(
                        "La definicion de la variable no es un literal (" + Kind + ").");
                }

                return _literal;
            }
        }

        /// <summary>A definition that IS the number. Rejects NaN and infinity: neither is a length.</summary>
        public static VariableDefinition Literal(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                throw new ArgumentException(
                    "El valor literal de una variable de proyecto debe ser un numero finito.",
                    nameof(value));
            }

            return new VariableDefinition(VariableDefinitionKind.Literal, value, null);
        }

        /// <summary>A definition whose semantic authority is an already-bound expression.</summary>
        public static VariableDefinition Expression(BoundExpression expression)
            => new VariableDefinition(
                VariableDefinitionKind.Expression,
                0,
                expression ?? throw new ArgumentNullException(nameof(expression)));

        public bool Equals(VariableDefinition other)
            => other != null &&
               Kind == other.Kind &&
               (Kind == VariableDefinitionKind.Literal
                   ? _literal.Equals(other._literal)
                   : _expression.Equals(other._expression));

        public override bool Equals(object obj) => Equals(obj as VariableDefinition);

        public override int GetHashCode()
            => Kind == VariableDefinitionKind.Literal
                ? (Kind, _literal).GetHashCode()
                : (Kind, _expression).GetHashCode();

        public override string ToString()
            => Kind == VariableDefinitionKind.Literal
                ? Kind + "(" + _literal.ToString(System.Globalization.CultureInfo.InvariantCulture) + ")"
                : Kind + "(" + _expression + ")";
    }
}
