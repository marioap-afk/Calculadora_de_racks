using System.Collections.Generic;
using RackCad.Application.Persistence;

namespace RackCad.Application.ProjectVariables
{
    /// <summary>What the user asked the central window to do.</summary>
    public enum ProjectVariableIntentKind
    {
        Create = 1,

        Rename = 2,

        ChangeValue = 3,

        Delete = 4,

        UnlinkAllAndDelete = 5,

        RepairBroken = 6,
    }

    /// <summary>
    /// One request from the window, addressed by IDENTITY (I-47 G16).
    ///
    /// <para>
    /// The name never addresses anything. It is the one field a user edits freely, so an operation routed by
    /// name would move the moment somebody renamed a variable — and a rename would stop being the harmless,
    /// registry-only change that the id being independent of the name exists to make it.
    /// </para>
    /// <para>
    /// <see cref="Create"/> carries no id on purpose: minting one is the preflight's job, so the window cannot
    /// invent an identity the register never agreed to.
    /// </para>
    /// </summary>
    public sealed class ProjectVariableIntent
    {
        private ProjectVariableIntent(
            ProjectVariableIntentKind kind,
            VariableId variableId,
            string name,
            double value,
            string rackId,
            string propertyId,
            bool confirmed)
        {
            Kind = kind;
            VariableId = variableId;
            Name = name;
            Value = value;
            RackId = rackId;
            PropertyId = propertyId;
            Confirmed = confirmed;
        }

        public ProjectVariableIntentKind Kind { get; }

        /// <summary>The variable the operation is about. Default for <see cref="Create"/> and repairs.</summary>
        public VariableId VariableId { get; }

        public string Name { get; }

        public double Value { get; }

        /// <summary>The rack, for a repair. Null otherwise.</summary>
        public string RackId { get; }

        /// <summary>The bound property, for a repair. Null otherwise.</summary>
        public string PropertyId { get; }

        /// <summary>Whether the user confirmed an irreversible action. False by default, always.</summary>
        public bool Confirmed { get; }

        public static ProjectVariableIntent Create(string name, double value)
            => new ProjectVariableIntent(ProjectVariableIntentKind.Create, default, name, value, null, null, false);

        public static ProjectVariableIntent Rename(VariableId variableId, string name)
            => new ProjectVariableIntent(ProjectVariableIntentKind.Rename, variableId, name, 0.0, null, null, false);

        public static ProjectVariableIntent ChangeValue(VariableId variableId, double value)
            => new ProjectVariableIntent(ProjectVariableIntentKind.ChangeValue, variableId, null, value, null, null, false);

        public static ProjectVariableIntent Delete(VariableId variableId)
            => new ProjectVariableIntent(ProjectVariableIntentKind.Delete, variableId, null, 0.0, null, null, false);

        public static ProjectVariableIntent UnlinkAllAndDelete(VariableId variableId, bool confirmed)
            => new ProjectVariableIntent(
                ProjectVariableIntentKind.UnlinkAllAndDelete, variableId, null, 0.0, null, null, confirmed);

        public static ProjectVariableIntent RepairBroken(string rackId, string propertyId, bool confirmed)
            => new ProjectVariableIntent(
                ProjectVariableIntentKind.RepairBroken, default, null, 0.0, rackId, propertyId, confirmed);
    }

    /// <summary>
    /// The single mapping from an intent to the semantic preflight that already exists (I-47 G16).
    ///
    /// <para>
    /// It is one <c>switch</c> and nothing else, and that is the whole point: G16 adds a surface, not a second
    /// set of rules. Finding consumers, resolving effective values, materialising an unlink, refusing a delete
    /// and repairing are all G5/G6 answers, and a loop in the Plugin re-deriving any of them would be a rule
    /// that nobody can prove.
    /// </para>
    /// <para>
    /// The one decision made here is the confirmation gate on unlink-all-and-delete: it removes every binding
    /// in the drawing, and the preflight has no way to know whether the user was told.
    /// </para>
    /// </summary>
    public static class ProjectVariableIntentPreflight
    {
        public static VariableMutationPreflightResult Run(
            ProjectVariableIntent intent,
            ProjectVariablesDocument registry,
            IReadOnlyList<ProjectVariableScanEntry> entries)
        {
            if (intent == null)
            {
                return VariableMutationPreflightResult.Failed("No hay ninguna operación que ejecutar.");
            }

            switch (intent.Kind)
            {
                case ProjectVariableIntentKind.Create:
                    return ProjectVariableMutationPreflight.Create(
                        intent.Name, VariableType.Length, VariableDefinition.Literal(intent.Value));

                case ProjectVariableIntentKind.Rename:
                    return ProjectVariableMutationPreflight.Rename(registry, intent.VariableId, intent.Name);

                case ProjectVariableIntentKind.ChangeValue:
                    return ProjectVariableMutationPreflight.ChangeValue(
                        registry, intent.VariableId, VariableDefinition.Literal(intent.Value), entries);

                case ProjectVariableIntentKind.Delete:
                    return ProjectVariableMutationPreflight.Delete(registry, intent.VariableId, entries);

                case ProjectVariableIntentKind.UnlinkAllAndDelete:
                    if (!intent.Confirmed)
                    {
                        return VariableMutationPreflightResult.Failed(
                            "Desvincular todos y eliminar quita el vínculo de TODOS los racks que usan la " +
                            "variable y después la borra. Hace falta confirmarlo explícitamente.");
                    }

                    return ProjectVariableMutationPreflight.UnlinkAllAndDelete(registry, intent.VariableId, entries);

                case ProjectVariableIntentKind.RepairBroken:
                    return ProjectVariableMutationPreflight.RepairBroken(
                        registry,
                        intent.RackId,
                        PropertyId.TryParse(intent.PropertyId, out var propertyId) ? propertyId : default,
                        entries,
                        intent.Confirmed);

                default:
                    return VariableMutationPreflightResult.Failed("Operación no reconocida.");
            }
        }
    }
}
