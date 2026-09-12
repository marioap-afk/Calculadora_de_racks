using System;
using System.Collections.Generic;
using System.Text.Json;
using RackCad.Application.Persistence;
using RackCad.Domain.Systems.Selective;

namespace RackCad.Application.ProjectVariables
{
    /// <summary>What an operation does to the drawing's register.</summary>
    public enum RegistryMutationKind
    {
        /// <summary>Nothing. A blocked operation mutates neither the register nor a rack.</summary>
        None = 0,

        Add = 1,
        Rename = 2,
        ChangeValue = 3,
        Remove = 4,
    }

    /// <summary>
    /// The change one operation makes to the register, described rather than performed.
    ///
    /// <para><see cref="ApplyTo"/> is pure and returns a NEW document: nothing about planning may touch the
    /// register the caller handed in, because a plan that mutated its own input would already have committed
    /// half the operation before anyone decided to run it.</para>
    /// </summary>
    public sealed class RegistryMutation
    {
        private RegistryMutation(
            RegistryMutationKind kind,
            VariableId variableId,
            string name,
            VariableType type,
            VariableDefinition definition)
        {
            Kind = kind;
            VariableId = variableId;
            Name = name;
            Type = type;
            Definition = definition;
        }

        public RegistryMutationKind Kind { get; }

        public VariableId VariableId { get; }

        /// <summary>The name to write. Set for <see cref="RegistryMutationKind.Add"/> and <see cref="RegistryMutationKind.Rename"/>.</summary>
        public string Name { get; }

        public VariableType Type { get; }

        /// <summary>The definition to write. Set for <see cref="RegistryMutationKind.Add"/> and <see cref="RegistryMutationKind.ChangeValue"/>.</summary>
        public VariableDefinition Definition { get; }

        public static RegistryMutation None { get; } =
            new RegistryMutation(RegistryMutationKind.None, default, null, VariableType.Length, null);

        public static RegistryMutation Add(ProjectVariable variable)
            => new RegistryMutation(
                RegistryMutationKind.Add,
                variable.Id,
                variable.Name,
                variable.Type,
                variable.Definition);

        public static RegistryMutation Rename(VariableId id, string name)
            => new RegistryMutation(RegistryMutationKind.Rename, id, name, VariableType.Length, null);

        public static RegistryMutation ChangeValue(VariableId id, VariableDefinition definition)
            => new RegistryMutation(RegistryMutationKind.ChangeValue, id, null, VariableType.Length, definition);

        public static RegistryMutation Remove(VariableId id)
            => new RegistryMutation(RegistryMutationKind.Remove, id, null, VariableType.Length, null);

        /// <summary>
        /// Applies this change to a COPY of <paramref name="registry"/>, leaving the original untouched.
        ///
        /// <para>
        /// INTERNAL since I-48 G4B, and that is the point. A raw read is not an authority: applying a change to
        /// one skips the identity precondition, and with a duplicated <see cref="VariableId"/> a Remove deletes
        /// BOTH entries. Making it internal means the executor cannot reach it at all — the guarantee is a
        /// compile error rather than a review habit. Commits go through
        /// <see cref="RegistryCommit.Prepare"/>, which accredits first.
        /// </para>
        /// </summary>
        internal ProjectVariablesDocument ApplyTo(ProjectVariablesDocument registry)
        {
            var next = ProjectVariableCloning.Clone(registry) ?? ProjectVariablesDocument.CreateNew();

            next.Variables ??= new List<ProjectVariableDocument>();

            switch (Kind)
            {
                case RegistryMutationKind.Add:
                    next.Variables.Add(new ProjectVariableDocument
                    {
                        VariableId = VariableId.Value,
                        Name = Name,
                        Type = Type.ToString(),
                        Definition = new ProjectVariableDefinitionDocument
                        {
                            Kind = "literal",
                            Value = Definition.LiteralValue,
                        },
                    });
                    break;

                case RegistryMutationKind.Rename:
                    var toRename = Find(next);

                    if (toRename != null)
                    {
                        toRename.Name = Name;
                    }

                    break;

                case RegistryMutationKind.ChangeValue:
                    var toChange = Find(next);

                    if (toChange != null)
                    {
                        toChange.Definition = new ProjectVariableDefinitionDocument
                        {
                            Kind = "literal",
                            Value = Definition.LiteralValue,
                        };
                    }

                    break;

                case RegistryMutationKind.Remove:
                    next.Variables.RemoveAll(
                        entry => string.Equals(entry?.VariableId, VariableId.Value, StringComparison.OrdinalIgnoreCase));
                    break;
            }

            return next;
        }

        private ProjectVariableDocument Find(ProjectVariablesDocument registry)
            => registry.Variables.Find(
                entry => string.Equals(entry?.VariableId, VariableId.Value, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>What one rack has to end up looking like.</summary>
    public sealed class RackMutation
    {
        public RackMutation(
            string rackId,
            SelectivePalletDesignDocument authoredOutput,
            SelectivePalletDesign effectiveOutput,
            IReadOnlyList<ProjectVariableScanEntry> destinations)
        {
            RackId = rackId;
            AuthoredOutput = authoredOutput;
            EffectiveOutput = effectiveOutput;
            Destinations = destinations;
        }

        public string RackId { get; }

        /// <summary>The authored document to persist. ONE per rack, not one per view.</summary>
        public SelectivePalletDesignDocument AuthoredOutput { get; }

        /// <summary>The effective design the geometry has to reflect.</summary>
        public SelectivePalletDesign EffectiveOutput { get; }

        /// <summary>
        /// Every present view of the rack. These are LOGICAL destinations: the plan says which views must end
        /// up carrying the result, not how a drawing reaches them.
        /// </summary>
        public IReadOnlyList<ProjectVariableScanEntry> Destinations { get; }
    }

    /// <summary>
    /// The SEMANTIC mutations an operation intends — and nothing else.
    ///
    /// <para>
    /// It carries no <c>ObjectId</c>, no <c>Database</c>, no <c>Transaction</c>, no <c>BlockTableRecord</c>
    /// and no <c>Editor</c>. That is what makes the promise "zero mutation on failure" provable in the Core
    /// suite instead of merely asserted: the plan says WHAT must change, and a later gate decides HOW it is
    /// written.
    /// </para>
    /// <para>
    /// The rule without exception: a failed preflight leaves an EMPTY plan. Not a partial one, not one with
    /// the racks that did pass.
    /// </para>
    /// </summary>
    public sealed class MutationPlan
    {
        private static readonly RackMutation[] NoRacks = new RackMutation[0];

        private MutationPlan(RegistryMutation registryMutation, IReadOnlyList<RackMutation> rackMutations)
        {
            RegistryMutation = registryMutation;
            RackMutations = rackMutations;
        }

        public RegistryMutation RegistryMutation { get; }

        public IReadOnlyList<RackMutation> RackMutations { get; }

        public bool IsEmpty
            => RegistryMutation.Kind == RegistryMutationKind.None && RackMutations.Count == 0;

        public static MutationPlan Empty { get; } = new MutationPlan(RegistryMutation.None, NoRacks);

        public static MutationPlan Of(RegistryMutation registry, IReadOnlyList<RackMutation> racks)
            => new MutationPlan(registry ?? RegistryMutation.None, racks ?? NoRacks);
    }

    /// <summary>
    /// A rack that stands in the way of deleting a variable, named the way a user can act on it. Listing them
    /// is what turns "no puedes borrarla" into something the user can resolve.
    /// </summary>
    public sealed class VariableConsumerSummary
    {
        public VariableConsumerSummary(string rackId, string rackName, IReadOnlyList<string> propertyIds)
        {
            RackId = rackId;
            RackName = rackName;
            PropertyIds = propertyIds;
        }

        public string RackId { get; }

        public string RackName { get; }

        /// <summary>The properties of that rack bound to the variable.</summary>
        public IReadOnlyList<string> PropertyIds { get; }
    }

    /// <summary>
    /// Faithful copies of persisted documents, so planning never mutates what the caller owns.
    ///
    /// <para>It round-trips through the PERSISTED shape rather than the stores on purpose: the selective store
    /// stamps the sticky schema version as a side effect of writing, and a copy must not carry that.</para>
    /// </summary>
    internal static class ProjectVariableCloning
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            WriteIndented = false,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        };

        internal static ProjectVariablesDocument Clone(ProjectVariablesDocument document)
            => document == null
                ? null
                : JsonSerializer.Deserialize<ProjectVariablesDocument>(JsonSerializer.Serialize(document, Options), Options);

        internal static SelectivePalletDesignDocument Clone(SelectivePalletDesignDocument document)
            => document == null
                ? null
                : JsonSerializer.Deserialize<SelectivePalletDesignDocument>(JsonSerializer.Serialize(document, Options), Options);
    }
}
