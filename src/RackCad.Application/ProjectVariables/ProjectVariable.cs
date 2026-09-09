using System;

namespace RackCad.Application.ProjectVariables
{
    /// <summary>
    /// A value the whole drawing shares BY DECISION of the project, not by coincidence of configuration
    /// (ADR-0034). It is an ENTITY: what makes two instances the same variable is <see cref="Id"/>, never
    /// <see cref="Name"/> and never the value.
    ///
    /// <para>
    /// That is why this type deliberately does NOT implement value equality: comparing two variables field by
    /// field would answer a question nobody asks, and would invite exactly the confusion the identity exists
    /// to prevent. Callers compare <see cref="Id"/>.
    /// </para>
    /// <para>
    /// It is immutable, so a rename produces a NEW instance carrying the SAME id (<see cref="WithName"/>).
    /// That shape is what makes "renaming touches no consumer" checkable instead of merely promised.
    /// </para>
    /// <para>
    /// PURE: no AutoCAD, no persistence, no schema. How this is stored is a separate contract, and the
    /// geometry never learns that a variable existed at all.
    /// </para>
    /// </summary>
    public sealed class ProjectVariable
    {
        private ProjectVariable(VariableId id, string name, VariableType type, VariableDefinition definition)
        {
            Id = id;
            Name = name;
            Type = type;
            Definition = definition;
        }

        /// <summary>The stable identity. Everything resolves through this.</summary>
        public VariableId Id { get; }

        /// <summary>The user-facing label. Changed through <see cref="WithName"/>, and with NO authority.</summary>
        public string Name { get; }

        public VariableType Type { get; }

        public VariableDefinition Definition { get; }

        /// <summary>
        /// Builds a variable, rejecting every state that would be meaningless: no identity, a type this build
        /// does not declare, no definition, or a blank label.
        /// </summary>
        public static ProjectVariable Create(VariableId id, string name, VariableType type, VariableDefinition definition)
        {
            if (id.IsEmpty)
            {
                throw new ArgumentException(
                    "Una variable de proyecto necesita un VariableId; el id vacio no es una identidad.",
                    nameof(id));
            }

            if (!VariableTypes.IsSupported(type))
            {
                throw new ArgumentException(
                    "Tipo de variable de proyecto no soportado (" + type + ").",
                    nameof(type));
            }

            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(
                    "Una variable de proyecto necesita un nombre visible.",
                    nameof(name));
            }

            return new ProjectVariable(id, name, type, definition);
        }

        /// <summary>Renames the variable. The <see cref="Id"/> is carried over untouched — that IS the guarantee.</summary>
        public ProjectVariable WithName(string name) => Create(Id, name, Type, Definition);

        /// <summary>Changes what the variable is worth, keeping its identity and its type.</summary>
        public ProjectVariable WithDefinition(VariableDefinition definition) => Create(Id, Name, Type, definition);

        public override string ToString() => Name + " [" + Id + "]";
    }
}
