using System;
using System.Collections.Generic;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Selective;

namespace RackCad.Application.ProjectVariables
{
    /// <summary>
    /// The semantic preflight of the eight confirmed operations. PURE, and it produces either a COMPLETE plan
    /// or nothing at all.
    ///
    /// <para>
    /// The eight are the unit of atomicity — <c>Create</c>, <c>Rename</c>, <c>ChangeValue</c>, <c>Delete</c>,
    /// <c>Link</c>, <c>Unlink</c>, <c>RepairBroken</c>, <c>UnlinkAllAndDelete</c> — but they do not all do the
    /// same thing. Two are REGISTRY-ONLY: creating and renaming touch no rack, need no sweep and cause no
    /// redraw, because references travel by id and a name has no authority. The other six reason over racks,
    /// each through the family that fits it.
    /// </para>
    /// <para>
    /// Whatever fails, the answer is the same: <b>no registry mutation, no rack mutations, empty plan</b>.
    /// Never the racks that happened to pass. That is what makes "cero mutacion" a property this suite can
    /// check rather than a sentence in a document.
    /// </para>
    /// </summary>
    public static class ProjectVariableMutationPreflight
    {
        // ------------------------------------------------------------------ registry-only

        /// <summary>Creates a variable. It can have no consumers — it did not exist a moment ago.</summary>
        public static VariableMutationPreflightResult Create(
            string name,
            VariableType type,
            VariableDefinition definition)
        {
            ProjectVariable variable;

            try
            {
                variable = ProjectVariable.Create(VariableId.New(), name, type, definition);
            }
            catch (ArgumentException ex)
            {
                return VariableMutationPreflightResult.Failed(ex.Message);
            }

            return VariableMutationPreflightResult.Success(
                MutationPlan.Of(RegistryMutation.Add(variable), null));
        }

        /// <summary>
        /// Renames a variable: the <see cref="VariableId"/> is untouched, so no consumer changes, nothing is
        /// swept and nothing is redrawn. That is the entire reason the identity is independent of the name.
        /// </summary>
        public static VariableMutationPreflightResult Rename(
            ProjectVariablesDocument registry,
            VariableId variableId,
            string newName)
        {
            if (!TryFind(registry, variableId, out var variable))
            {
                return Missing(variableId);
            }

            try
            {
                variable.WithName(newName);
            }
            catch (ArgumentException ex)
            {
                return VariableMutationPreflightResult.Failed(ex.Message);
            }

            return VariableMutationPreflightResult.Success(
                MutationPlan.Of(RegistryMutation.Rename(variableId, newName), null));
        }

        // ------------------------------------------------------------------ target-variable (family A)

        /// <summary>
        /// Changes what a variable is worth and plans the redraw of every consumer. The authored literal of a
        /// consumer is NOT touched: propagation redraws, it does not rewrite the intention the user typed.
        /// </summary>
        public static VariableMutationPreflightResult ChangeValue(
            ProjectVariablesDocument registry,
            VariableId variableId,
            VariableDefinition definition,
            IReadOnlyList<ProjectVariableScanEntry> entries)
        {
            if (!TryFind(registry, variableId, out _))
            {
                return Missing(variableId);
            }

            var discovery = ProjectVariableConsumerDiscovery.DiscoverConsumers(entries, variableId);

            if (!discovery.IsSuccess)
            {
                return VariableMutationPreflightResult.Failed(discovery.Error);
            }

            var mutation = RegistryMutation.ChangeValue(variableId, definition);
            var after = mutation.ApplyTo(registry);
            var racks = new List<RackMutation>();

            foreach (var consumer in discovery.Consumers)
            {
                var authored = ProjectVariableCloning.Clone(consumer.Authored);
                var effective = new SelectiveEffectiveDesignResolver().Resolve(authored, after);

                if (!effective.IsSuccess)
                {
                    return VariableMutationPreflightResult.Failed(effective.Error);
                }

                racks.Add(new RackMutation(consumer.RackId, authored, effective.Design, consumer.Siblings));
            }

            return VariableMutationPreflightResult.Success(MutationPlan.Of(mutation, racks));
        }

        /// <summary>
        /// Deletes a variable, and REFUSES while anything still uses it — listing what does. Nothing broken is
        /// ever created: the user unlinks first, or uses the explicit unlink-all-and-delete.
        /// </summary>
        public static VariableMutationPreflightResult Delete(
            ProjectVariablesDocument registry,
            VariableId variableId,
            IReadOnlyList<ProjectVariableScanEntry> entries)
        {
            if (!TryFind(registry, variableId, out _))
            {
                return Missing(variableId);
            }

            var discovery = ProjectVariableConsumerDiscovery.DiscoverConsumers(entries, variableId);

            if (!discovery.IsSuccess)
            {
                return VariableMutationPreflightResult.Failed(discovery.Error);
            }

            if (discovery.Consumers.Count > 0)
            {
                return VariableMutationPreflightResult.Blocked(
                    "No se puede borrar la variable " + variableId + ": todavía la usan " +
                    discovery.Consumers.Count + " rack(s). Desvincúlalos primero, o usa la acción explícita " +
                    "que desvincula todos materializando el valor y después borra.",
                    Summarize(discovery.Consumers, variableId));
            }

            return VariableMutationPreflightResult.Success(
                MutationPlan.Of(RegistryMutation.Remove(variableId), null));
        }

        /// <summary>
        /// The comfortable path, as ONE logical unit: every consumer is unlinked MATERIALIZING its current
        /// effective value — so no rack changes shape — and only then is the variable removed. All or nothing.
        /// </summary>
        public static VariableMutationPreflightResult UnlinkAllAndDelete(
            ProjectVariablesDocument registry,
            VariableId variableId,
            IReadOnlyList<ProjectVariableScanEntry> entries)
        {
            if (!TryFind(registry, variableId, out _))
            {
                return Missing(variableId);
            }

            var discovery = ProjectVariableConsumerDiscovery.DiscoverConsumers(entries, variableId);

            if (!discovery.IsSuccess)
            {
                return VariableMutationPreflightResult.Failed(discovery.Error);
            }

            var mutation = RegistryMutation.Remove(variableId);
            var after = mutation.ApplyTo(registry);
            var racks = new List<RackMutation>();

            foreach (var consumer in discovery.Consumers)
            {
                var materialized = Materialize(consumer.Authored, registry, after, out var failure);

                if (failure != null)
                {
                    return failure;
                }

                racks.Add(new RackMutation(consumer.RackId, materialized.Authored, materialized.Effective, consumer.Siblings));
            }

            return VariableMutationPreflightResult.Success(MutationPlan.Of(mutation, racks));
        }

        // ------------------------------------------------------------------ target-rack (family B)

        /// <summary>
        /// Binds a property to a variable. The authored literal is FROZEN, not replaced; the schema is
        /// promoted; the effective value is the variable's from this moment on. There is no intermediate state
        /// where the binding exists and the drawing still shows the old number.
        /// </summary>
        public static VariableMutationPreflightResult Link(
            ProjectVariablesDocument registry,
            string rackId,
            PropertyId propertyId,
            VariableId variableId,
            IReadOnlyList<ProjectVariableScanEntry> entries)
        {
            if (!TryFind(registry, variableId, out _))
            {
                return Missing(variableId);
            }

            if (!TryRack(entries, rackId, out var consumer, out var failure))
            {
                return failure;
            }

            var authored = ProjectVariableCloning.Clone(consumer.Authored);

            authored.PropertyValues ??= new Dictionary<string, SelectivePropertyValueDocument>();
            authored.PropertyValues[propertyId.Value] =
                SelectivePropertyValueDocument.ToProjectVariable(variableId.Value);
            authored.SchemaVersion = SelectiveDesignSchema.ResolveWriteVersion(authored.SchemaVersion, true);

            var effective = new SelectiveEffectiveDesignResolver().Resolve(authored, registry);

            return effective.IsSuccess
                ? VariableMutationPreflightResult.Success(
                    MutationPlan.Of(
                        RegistryMutation.None,
                        new[] { new RackMutation(consumer.RackId, authored, effective.Design, consumer.Siblings) }))
                : VariableMutationPreflightResult.Failed(effective.Error);
        }

        /// <summary>
        /// Unbinds a HEALTHY binding by writing the CURRENT EFFECTIVE value into the literal — which is what
        /// makes the geometric effect nil. Writing the old literal back instead would make the rack jump, and
        /// that is the easy bug this step exists to avoid.
        ///
        /// <para>A broken reference does not come through here: there is no effective value to materialize,
        /// and pretending the frozen literal is one would be the silent fallback the contract forbids. That
        /// case is <see cref="RepairBroken"/>.</para>
        /// </summary>
        public static VariableMutationPreflightResult Unlink(
            ProjectVariablesDocument registry,
            string rackId,
            PropertyId propertyId,
            IReadOnlyList<ProjectVariableScanEntry> entries)
        {
            if (!TryRack(entries, rackId, out var consumer, out var failure))
            {
                return failure;
            }

            if (!consumer.Authored.HasBindingEntry(propertyId))
            {
                return NotBound(rackId, propertyId);
            }

            var current = new SelectiveEffectiveDesignResolver().Resolve(consumer.Authored, registry);

            if (!current.IsSuccess)
            {
                return VariableMutationPreflightResult.Failed(
                    current.Error + " Desvincular exige un valor efectivo que materializar; para un vínculo " +
                    "roto usa la reparación explícita.");
            }

            var authored = ProjectVariableCloning.Clone(consumer.Authored);

            // The ACTIVE step. Leaving the old literal here is the same as never having bound the rack.
            authored.VerticalClearance = current.Design.VerticalClearance;
            authored.PropertyValues.Remove(propertyId.Value);

            var effective = new SelectiveEffectiveDesignResolver().Resolve(authored, registry);

            return effective.IsSuccess
                ? VariableMutationPreflightResult.Success(
                    MutationPlan.Of(
                        RegistryMutation.None,
                        new[] { new RackMutation(consumer.RackId, authored, effective.Design, consumer.Siblings) }))
                : VariableMutationPreflightResult.Failed(effective.Error);
        }

        /// <summary>
        /// Repairs a BROKEN reference, and only a broken one.
        ///
        /// <para>
        /// The sequence is exactly the accepted one, and the order is the whole point: a broken reference has
        /// NO effective value; there is no automatic fallback to the authored literal; the action is explicit
        /// and warned; it uses the STORED authored literal; it REMOVES the broken binding; it keeps the
        /// promoted schema; and only AFTER the binding is gone does that literal govern again. There is never
        /// a state where the binding is still there and the literal governs.
        /// </para>
        /// <para>
        /// It is repair and not the fallback D-08 forbids because of who starts it and what they were told:
        /// the user, having been warned there is no effective value and that the geometry may change. The
        /// literal is available precisely because binding froze it and no variable change ever touched it.
        /// </para>
        /// </summary>
        public static VariableMutationPreflightResult RepairBroken(
            ProjectVariablesDocument registry,
            string rackId,
            PropertyId propertyId,
            IReadOnlyList<ProjectVariableScanEntry> entries,
            bool confirmed)
        {
            if (!TryRack(entries, rackId, out var consumer, out var failure))
            {
                return failure;
            }

            if (!consumer.Authored.HasBindingEntry(propertyId))
            {
                return NotBound(rackId, propertyId);
            }

            if (new SelectiveEffectiveDesignResolver().Resolve(consumer.Authored, registry).IsSuccess)
            {
                return VariableMutationPreflightResult.Failed(
                    "El vínculo de '" + propertyId + "' en el rack " + rackId + " resuelve correctamente: no " +
                    "hay nada que reparar. Para quitarlo sin cambiar la geometría, desvincula.");
            }

            if (!confirmed)
            {
                return VariableMutationPreflightResult.Failed(
                    "La variable a la que apunta '" + propertyId + "' en el rack " + rackId + " no existe, así " +
                    "que NO hay valor efectivo que materializar. Reparar usará el literal almacenado y la " +
                    "geometría puede cambiar. Requiere confirmación explícita.");
            }

            var authored = ProjectVariableCloning.Clone(consumer.Authored);

            // El literal almacenado NO se toca; lo que se retira es el binding. Solo entonces gobierna.
            authored.PropertyValues.Remove(propertyId.Value);

            var effective = new SelectiveEffectiveDesignResolver().Resolve(authored, registry);

            return effective.IsSuccess
                ? VariableMutationPreflightResult.Success(
                    MutationPlan.Of(
                        RegistryMutation.None,
                        new[] { new RackMutation(consumer.RackId, authored, effective.Design, consumer.Siblings) }))
                : VariableMutationPreflightResult.Failed(effective.Error);
        }

        // ------------------------------------------------------------------ helpers

        /// <summary>Unlinks one consumer by materializing its current effective value. Shared by unlink-all-and-delete.</summary>
        private static (SelectivePalletDesignDocument Authored, Domain.Systems.Selective.SelectivePalletDesign Effective) Materialize(
            SelectivePalletDesignDocument source,
            ProjectVariablesDocument before,
            ProjectVariablesDocument after,
            out VariableMutationPreflightResult failure)
        {
            failure = null;

            var current = new SelectiveEffectiveDesignResolver().Resolve(source, before);

            if (!current.IsSuccess)
            {
                failure = VariableMutationPreflightResult.Failed(current.Error);
                return default;
            }

            var authored = ProjectVariableCloning.Clone(source);
            authored.VerticalClearance = current.Design.VerticalClearance;
            authored.PropertyValues?.Remove(ProjectPropertyIds.SelectiveVerticalClearance.Value);

            var effective = new SelectiveEffectiveDesignResolver().Resolve(authored, after);

            if (!effective.IsSuccess)
            {
                failure = VariableMutationPreflightResult.Failed(effective.Error);
                return default;
            }

            return (authored, effective.Design);
        }

        private static bool TryRack(
            IReadOnlyList<ProjectVariableScanEntry> entries,
            string rackId,
            out ProjectVariableConsumer consumer,
            out VariableMutationPreflightResult failure)
        {
            consumer = null;
            failure = null;

            var resolved = ProjectVariableConsumerDiscovery.ResolveTargetRack(entries, rackId);

            if (!resolved.IsSuccess)
            {
                failure = VariableMutationPreflightResult.Failed(resolved.Error);
                return false;
            }

            consumer = resolved.Consumers[0];
            return true;
        }

        private static bool TryFind(ProjectVariablesDocument registry, VariableId variableId, out ProjectVariable variable)
        {
            variable = null;

            if (registry == null)
            {
                return false;
            }

            foreach (var candidate in registry.ToProjectVariables())
            {
                if (candidate.Id.Equals(variableId))
                {
                    variable = candidate;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// The consumers of a variable, named the way a user can act on them. Public since I-47 G16: the
        /// central window presents exactly this, and building a second version of it there would be a second
        /// answer to the same question.
        /// </summary>
        public static IReadOnlyList<VariableConsumerSummary> Summarize(
            IReadOnlyList<ProjectVariableConsumer> consumers,
            VariableId variableId)
        {
            var summaries = new List<VariableConsumerSummary>();

            foreach (var consumer in consumers)
            {
                var properties = new List<string>();

                if (consumer.Authored.PropertyValues != null)
                {
                    foreach (var entry in consumer.Authored.PropertyValues)
                    {
                        if (entry.Value != null &&
                            VariableId.TryParse(entry.Value.VariableId, out var id) &&
                            id.Equals(variableId))
                        {
                            properties.Add(entry.Key);
                        }
                    }
                }

                summaries.Add(new VariableConsumerSummary(consumer.RackId, consumer.Authored.Name, properties));
            }

            return summaries;
        }

        private static VariableMutationPreflightResult Missing(VariableId variableId)
            => VariableMutationPreflightResult.Failed(
                "La variable de proyecto '" + variableId + "' no existe en este dibujo.");

        private static VariableMutationPreflightResult NotBound(string rackId, PropertyId propertyId)
            => VariableMutationPreflightResult.Failed(
                "La propiedad '" + propertyId + "' del rack " + rackId + " no está vinculada a ninguna variable.");
    }
}
