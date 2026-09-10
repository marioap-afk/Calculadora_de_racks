using System;

namespace RackCad.Application.ProjectVariables
{
    /// <summary>WHERE a project variable's value comes from. ID22A has one case; a formula would add another.</summary>
    public enum VariableDefinitionKind
    {
        /// <summary>The value is written down.</summary>
        Literal = 1,
    }

    /// <summary>
    /// The definition of a project variable, discriminated by <see cref="Kind"/> from day one.
    ///
    /// <para>
    /// This is the extension point a later initiative occupies: persisting it discriminated now is what lets
    /// an <c>Expression</c> arrive as one more case — additive, without raising the major schema and without
    /// touching how a property references a variable. ID22A implements NONE of that: no formulas, no parser,
    /// no AST, no dependency graph.
    /// </para>
    /// <para>
    /// It is a class, not a struct, so there is no default-constructed instance declaring a kind nobody set.
    /// Equality is by value, because two literals of the same number ARE the same definition.
    /// </para>
    /// </summary>
    public sealed class VariableDefinition : IEquatable<VariableDefinition>
    {
        private readonly double _literal;

        private VariableDefinition(VariableDefinitionKind kind, double literal)
        {
            Kind = kind;
            _literal = literal;
        }

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

            return new VariableDefinition(VariableDefinitionKind.Literal, value);
        }

        public bool Equals(VariableDefinition other)
            => other != null && Kind == other.Kind && _literal.Equals(other._literal);

        public override bool Equals(object obj) => Equals(obj as VariableDefinition);

        public override int GetHashCode() => (Kind, _literal).GetHashCode();

        public override string ToString() => Kind + "(" + _literal.ToString(System.Globalization.CultureInfo.InvariantCulture) + ")";
    }
}
