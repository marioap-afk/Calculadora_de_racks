using System;

namespace RackCad.Application.ProjectVariables
{
    /// <summary>
    /// What a binding needs to know about the variable it points at (I-48 G4A, Proposal V8 R-01/V5-R01).
    ///
    /// <para>
    /// It is deliberately NOT a <see cref="ProjectVariable"/>, and that is the entire reason it exists.
    /// <c>ProjectVariable.Create</c> THROWS on a type this build does not support, so a target carrying an
    /// incompatible type can never be built through it — and a compatibility rule whose failing branch cannot
    /// be constructed is a rule nobody can prove. A snapshot is buildable directly, so the branch is
    /// reachable in a test WITHOUT adding a second supported type, weakening <c>ProjectVariable.Create</c> or
    /// touching the store.
    /// </para>
    /// <para>
    /// It is not a new domain entity and it changes NO persistence: it is an in-memory projection of a
    /// register entry the store already accredited.
    /// </para>
    /// <para>
    /// Its construction checks identity and finiteness — the invariants the store already guarantees for a
    /// readable document — so that a synthetic fixture cannot smuggle in a state production could never
    /// produce. It deliberately does NOT call <see cref="VariableTypes.IsSupported"/>: that is the very check
    /// whose failure has to remain constructible.
    /// </para>
    /// </summary>
    internal sealed class VariableTargetSnapshot
    {
        private VariableTargetSnapshot(
            VariableId variableId, VariableType variableType, VariableDefinition definition, string name)
        {
            VariableId = variableId;
            VariableType = variableType;
            Definition = definition;
            Name = name;
        }

        internal VariableId VariableId { get; }

        internal VariableType VariableType { get; }

        internal VariableDefinition Definition { get; }

        internal double LiteralValue => Definition.LiteralValue;

        /// <summary>
        /// The label a person reads. It travels here so that the surface SHOWING a name and the surface
        /// RESOLVING a value read the same accredited target: an editor displaying one variable while the
        /// drawing takes another's value is exactly the split this authority removes. It is never identity.
        /// </summary>
        internal string Name { get; }

        /// <summary>
        /// Builds a snapshot, or explains why the input is not one. Rejects an empty identity and a non-finite
        /// value; accepts ANY <see cref="ProjectVariables.VariableType"/> value, supported or not.
        /// </summary>
        internal static bool TryCreate(
            VariableId variableId,
            VariableType variableType,
            double literalValue,
            out VariableTargetSnapshot snapshot,
            out string error,
            string name = null)
        {
            snapshot = null;
            error = null;

            if (variableId.IsEmpty)
            {
                error = "Un target de variable de proyecto necesita un VariableId; el id vacio no es identidad.";
                return false;
            }

            if (double.IsNaN(literalValue) || double.IsInfinity(literalValue))
            {
                error = "La variable de proyecto " + variableId + " no declara un valor literal finito.";
                return false;
            }

            snapshot = new VariableTargetSnapshot(
                variableId, variableType, VariableDefinition.Literal(literalValue), name);
            return true;
        }

        internal static bool TryCreate(
            VariableId variableId,
            VariableType variableType,
            VariableDefinition definition,
            out VariableTargetSnapshot snapshot,
            out string error,
            string name = null)
        {
            snapshot = null;
            error = null;

            if (variableId.IsEmpty)
            {
                error = "Un target de variable de proyecto necesita un VariableId; el id vacio no es identidad.";
                return false;
            }

            if (definition == null)
            {
                error = "La variable de proyecto " + variableId + " no declara una definicion.";
                return false;
            }

            snapshot = new VariableTargetSnapshot(variableId, variableType, definition, name);
            return true;
        }

        public override string ToString()
            => VariableId + " (" + VariableType + ") = " + Definition;
    }
}
