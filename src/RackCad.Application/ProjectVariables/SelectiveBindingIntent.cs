using System.Collections.Generic;
using RackCad.Application.Persistence;

namespace RackCad.Application.ProjectVariables
{
    /// <summary>The two halves of the binding gesture.</summary>
    public enum SelectiveBindingIntentKind
    {
        Link = 1,

        Unlink = 2,
    }

    /// <summary>
    /// What the Selective editor asks for, addressed by identity (I-47 G17).
    ///
    /// <para>
    /// <see cref="Link"/> carries the <see cref="VariableId"/> the user PICKED, never the name they saw. Two
    /// variables may legitimately share a name — a name is text somebody edits — so a request routed by name
    /// would bind the wrong one the day that happens, silently and permanently.
    /// </para>
    /// <para>
    /// <see cref="Unlink"/> needs no variable: the rack already knows which one governs it, and asking the
    /// editor to name it again would be a second chance to name a different one.
    /// </para>
    /// </summary>
    public sealed class SelectiveBindingIntent
    {
        private SelectiveBindingIntent(
            SelectiveBindingIntentKind kind, string rackId, string propertyId, VariableId variableId)
        {
            Kind = kind;
            RackId = rackId;
            PropertyId = propertyId;
            VariableId = variableId;
        }

        public SelectiveBindingIntentKind Kind { get; }

        public string RackId { get; }

        public string PropertyId { get; }

        /// <summary>The variable to bind to. Default for <see cref="Unlink"/>.</summary>
        public VariableId VariableId { get; }

        public static SelectiveBindingIntent Link(string rackId, string propertyId, VariableId variableId)
            => new SelectiveBindingIntent(SelectiveBindingIntentKind.Link, rackId, propertyId, variableId);

        public static SelectiveBindingIntent Unlink(string rackId, string propertyId)
            => new SelectiveBindingIntent(SelectiveBindingIntentKind.Unlink, rackId, propertyId, default);
    }

    /// <summary>One variable the editor may offer, with everything it needs to SHOW it and nothing else.</summary>
    public sealed class ProjectVariableOption
    {
        public ProjectVariableOption(VariableId id, string name, VariableType type, double value)
        {
            Id = id;
            Name = name;
            Type = type;
            Value = value;
        }

        /// <summary>The authority the selection travels by.</summary>
        public VariableId Id { get; }

        /// <summary>For the user to read. NOT unique, and not identity.</summary>
        public string Name { get; }

        public VariableType Type { get; }

        public double Value { get; }
    }

    /// <summary>
    /// The variables a Selective clearance may be bound to (I-47 G17).
    ///
    /// <para>
    /// Only <see cref="VariableType.Length"/>: offering a variable of a type this property cannot take would
    /// invite a binding that resolves to a number meaning something else. Filtering here — rather than letting
    /// the editor decide what looks compatible — is what keeps that judgement in one place when more types
    /// arrive.
    /// </para>
    /// <para>
    /// Duplicate names are ALLOWED and travel through untouched. The list shows name and value so a person can
    /// tell two homonyms apart; the machine never needed to, because it works from the id.
    /// </para>
    /// </summary>
    public static class SelectiveBindingOptions
    {
        public static IReadOnlyList<ProjectVariableOption> ForLength(ProjectVariablesDocument registry)
        {
            var options = new List<ProjectVariableOption>();

            if (registry == null)
            {
                return options;
            }

            if (registry.Variables == null)
            {
                return options;
            }

            foreach (var entry in registry.Variables)
            {
                // El tipo se lee de lo PERSISTIDO, que es donde apareceria uno futuro. Una entrada cuyo tipo o
                // cuyo id no se puedan interpretar no se ofrece: no se puede vincular a lo que no se sabe leer,
                // y el registro entero ya lo valido su store al leerlo.
                if (entry?.Definition?.Value == null ||
                    !System.Enum.TryParse<VariableType>(entry.Type, ignoreCase: true, out var type) ||
                    !VariableTypes.IsSupported(type) ||
                    type != VariableType.Length ||
                    !VariableId.TryParse(entry.VariableId, out var id))
                {
                    continue;
                }

                options.Add(new ProjectVariableOption(id, entry.Name, type, entry.Definition.Value.Value));
            }

            return options;
        }
    }

    /// <summary>
    /// The single mapping from a binding gesture to the semantics that already exist (I-47 G17).
    ///
    /// <para>
    /// One <c>switch</c>, like G16's. Freezing the literal on link, materialising the effective value on
    /// unlink and refusing to unlink a broken reference are all G6 answers, already proven; re-deriving any of
    /// them next to a button would be a rule nobody can check.
    /// </para>
    /// </summary>
    public static class SelectiveBindingIntentPreflight
    {
        public static VariableMutationPreflightResult Run(
            SelectiveBindingIntent intent,
            ProjectVariablesDocument registry,
            IReadOnlyList<ProjectVariableScanEntry> entries)
        {
            if (intent == null)
            {
                return VariableMutationPreflightResult.Failed("No hay ninguna operación de vínculo que ejecutar.");
            }

            if (!PropertyId.TryParse(intent.PropertyId, out var propertyId) ||
                !ProjectPropertyIds.IsKnown(propertyId))
            {
                return VariableMutationPreflightResult.Failed(
                    "La propiedad '" + (intent.PropertyId ?? "<null>") + "' no es una que esta versión conozca.");
            }

            return intent.Kind == SelectiveBindingIntentKind.Link
                ? ProjectVariableMutationPreflight.Link(registry, intent.RackId, propertyId, intent.VariableId, entries)
                : ProjectVariableMutationPreflight.Unlink(registry, intent.RackId, propertyId, entries);
        }
    }
}
