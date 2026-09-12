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
        /// Accredits the register handed in, so every operation below works over a proven identity authority.
        ///
        /// <para>
        /// A null document is the legacy "this drawing has no register" case and maps to an ABSENT read; a
        /// document with a duplicated <see cref="VariableId"/> fails HERE, before any operation could pick one.
        /// </para>
        /// </summary>
        private static bool TryAccredit(
            ProjectVariablesDocument registry,
            out UsableProjectVariablesRegistry usable,
            out VariableMutationPreflightResult failure)
        {
            var accreditation = UsableProjectVariablesRegistry.Accredit(
                registry == null
                    ? ProjectVariablesReadResult.Absent()
                    : ProjectVariablesReadResult.Readable(registry));

            usable = accreditation.Registry;
            failure = accreditation.IsUsable
                ? null
                : VariableMutationPreflightResult.Failed(accreditation.Error);

            return accreditation.IsUsable;
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
            if (!TryAccredit(registry, out var usable, out var failure))
            {
                return failure;
            }

            if (!usable.TryGetTarget(variableId, out var target))
            {
                return Missing(variableId);
            }

            try
            {
                ProjectVariable.Create(
                    variableId, newName, target.VariableType, VariableDefinition.Literal(target.LiteralValue));
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
        ///
        /// <para>
        /// With one variable governing N properties of the same rack this still emits ONE
        /// <see cref="RackMutation"/> per rack: the effective design is resolved once, against the register as
        /// it will be, so every property that variable governs moves together.
        /// </para>
        /// </summary>
        public static VariableMutationPreflightResult ChangeValue(
            ProjectVariablesDocument registry,
            VariableId variableId,
            VariableDefinition definition,
            IReadOnlyList<ProjectVariableScanEntry> entries)
        {
            if (!TryAccredit(registry, out var usable, out var failure))
            {
                return failure;
            }

            if (!usable.TryGetTarget(variableId, out _))
            {
                return Missing(variableId);
            }

            var discovery = ProjectVariableConsumerDiscovery.DiscoverConsumers(entries, variableId);

            if (!discovery.IsSuccess)
            {
                return VariableMutationPreflightResult.Failed(discovery.Error);
            }

            var mutation = RegistryMutation.ChangeValue(variableId, definition);

            if (!TryAccredit(mutation.ApplyTo(registry), out var after, out var afterFailure))
            {
                return afterFailure;
            }

            var racks = new List<RackMutation>();

            foreach (var consumer in discovery.Consumers)
            {
                var authored = ProjectVariableCloning.Clone(consumer.Authored);
                var effective = Resolver.ResolveAgainst(authored, after);

                if (!effective.IsSuccess)
                {
                    return VariableMutationPreflightResult.Failed(effective.Error);
                }

                racks.Add(new RackMutation(consumer.RackId, authored, effective.Design, consumer.Siblings));
            }

            return VariableMutationPreflightResult.Success(MutationPlan.Of(mutation, racks));
        }

        /// <summary>
        /// Deletes a variable, and REFUSES while anything still uses it - listing what does. Nothing broken is
        /// ever created: the user unlinks first, or uses the explicit unlink-all-and-delete.
        /// </summary>
        public static VariableMutationPreflightResult Delete(
            ProjectVariablesDocument registry,
            VariableId variableId,
            IReadOnlyList<ProjectVariableScanEntry> entries)
        {
            if (!TryAccredit(registry, out var usable, out var failure))
            {
                return failure;
            }

            if (!usable.TryGetTarget(variableId, out _))
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
                    "No se puede borrar la variable " + variableId + ": todavia la usan " +
                    discovery.Consumers.Count + " rack(s). Desvinculalos primero, o usa la accion explicita " +
                    "que desvincula todos materializando el valor y despues borra.",
                    Summarize(discovery.Consumers, variableId));
            }

            return VariableMutationPreflightResult.Success(
                MutationPlan.Of(RegistryMutation.Remove(variableId), null));
        }

        /// <summary>
        /// The comfortable path, as ONE logical unit: every consumer is unlinked MATERIALIZING its current
        /// effective value - so no rack changes shape - and only then is the variable removed. All or nothing.
        ///
        /// <para>
        /// <b>N properties of the same rack may point at the same variable</b>, and this is where that used to
        /// break: materialising only one of them left the other pointing at a variable about to disappear, so
        /// the operation FABRICATED a broken binding - the exact damage <see cref="Delete"/> refuses to cause -
        /// and reported success. Now the whole set is derived from the single authored authority, materialised
        /// on ONE clone, and emitted as ONE <see cref="RackMutation"/>.
        /// </para>
        /// </summary>
        public static VariableMutationPreflightResult UnlinkAllAndDelete(
            ProjectVariablesDocument registry,
            VariableId variableId,
            IReadOnlyList<ProjectVariableScanEntry> entries)
        {
            if (!TryAccredit(registry, out var usable, out var failure))
            {
                return failure;
            }

            if (!usable.TryGetTarget(variableId, out _))
            {
                return Missing(variableId);
            }

            var discovery = ProjectVariableConsumerDiscovery.DiscoverConsumers(entries, variableId);

            if (!discovery.IsSuccess)
            {
                return VariableMutationPreflightResult.Failed(discovery.Error);
            }

            var mutation = RegistryMutation.Remove(variableId);

            if (!TryAccredit(mutation.ApplyTo(registry), out var after, out var afterFailure))
            {
                return afterFailure;
            }

            var descriptors = SelectiveLinkedProperties.All;
            var racks = new List<RackMutation>();

            foreach (var consumer in discovery.Consumers)
            {
                // Effective values are read BEFORE the variable goes away, against the register as it is.
                var current = Resolver.ResolveAgainst(consumer.Authored, usable);

                if (!current.IsSuccess)
                {
                    return VariableMutationPreflightResult.Failed(current.Error);
                }

                // P, derived from the ONE authored authority this consumer was already proven to have.
                var properties = SelectiveLinkedPropertyKernel.PropertiesBoundTo(consumer.Authored, variableId);

                if (properties.Count == 0)
                {
                    return VariableMutationPreflightResult.Failed(
                        "El rack " + consumer.RackId + " se detecto como consumidor de " + variableId +
                        " pero no declara ninguna propiedad vinculada a ella.");
                }

                var authored = ProjectVariableCloning.Clone(consumer.Authored);

                foreach (var propertyId in properties)
                {
                    if (!descriptors.TryGetDescriptor(propertyId, out var descriptor))
                    {
                        return VariableMutationPreflightResult.Failed(
                            "La propiedad '" + propertyId + "' del rack " + consumer.RackId +
                            " no es una que esta version conozca.");
                    }

                    descriptor.WriteAuthored(authored, descriptor.ReadEffective(current.Design));
                    authored.PropertyValues?.Remove(propertyId.Value);
                }

                var effective = Resolver.ResolveAgainst(authored, after);

                if (!effective.IsSuccess)
                {
                    return VariableMutationPreflightResult.Failed(effective.Error);
                }

                racks.Add(new RackMutation(consumer.RackId, authored, effective.Design, consumer.Siblings));
            }

            return VariableMutationPreflightResult.Success(MutationPlan.Of(mutation, racks));
        }

        // ------------------------------------------------------------------ target-rack (family B)

        /// <summary>
        /// Binds a property to a variable. The authored literal is FROZEN, not replaced; the schema is
        /// promoted; the effective value is the variable's from this moment on.
        ///
        /// <para>
        /// Compatibility is checked HERE, in Application, and not left to whatever the UI happened to offer: a
        /// filter is a convenience, never a boundary. A target of the wrong type is refused.
        /// </para>
        /// <para>
        /// If ANOTHER property of the same rack is broken the operation fails with an empty plan. That is
        /// deliberate: the executor redraws from a COMPLETE effective design, and while something else is
        /// unresolvable there is none.
        /// </para>
        /// </summary>
        public static VariableMutationPreflightResult Link(
            ProjectVariablesDocument registry,
            string rackId,
            PropertyId propertyId,
            VariableId variableId,
            IReadOnlyList<ProjectVariableScanEntry> entries)
        {
            if (!TryAccredit(registry, out var usable, out var failure))
            {
                return failure;
            }

            if (!SelectiveLinkedProperties.All.TryGetDescriptor(propertyId, out var descriptor))
            {
                return VariableMutationPreflightResult.Failed(
                    "La propiedad '" + propertyId + "' no es una que esta version conozca.");
            }

            if (!usable.TryGetTarget(variableId, out var target))
            {
                return Missing(variableId);
            }

            if (target.VariableType != descriptor.VariableType)
            {
                return VariableMutationPreflightResult.Failed(
                    "La propiedad '" + propertyId + "' exige una variable de tipo " + descriptor.VariableType +
                    ", pero '" + variableId + "' es de tipo " + target.VariableType + ".");
            }

            if (!TryRack(entries, rackId, out var consumer, out var rackFailure))
            {
                return rackFailure;
            }

            var authored = ProjectVariableCloning.Clone(consumer.Authored);

            authored.PropertyValues ??= new Dictionary<string, SelectivePropertyValueDocument>();
            authored.PropertyValues[propertyId.Value] =
                SelectivePropertyValueDocument.ToProjectVariable(variableId.Value);
            authored.SchemaVersion = SelectiveDesignSchema.ResolveWriteVersion(authored.SchemaVersion, true);

            var effective = Resolver.ResolveAgainst(authored, usable);

            return effective.IsSuccess
                ? VariableMutationPreflightResult.Success(
                    MutationPlan.Of(
                        RegistryMutation.None,
                        new[] { new RackMutation(consumer.RackId, authored, effective.Design, consumer.Siblings) }))
                : VariableMutationPreflightResult.Failed(effective.Error);
        }

        /// <summary>
        /// Unbinds a HEALTHY binding by writing the CURRENT EFFECTIVE value into THAT property's literal, which
        /// is what makes the geometric effect nil. Writing the old literal back instead would make the rack
        /// jump, and that is the easy bug this step exists to avoid.
        ///
        /// <para>
        /// The value lands on the DESCRIBED property's field, not on a fixed one: before this gate the
        /// materialisation wrote a single hardcoded field regardless of which property was being unlinked.
        /// </para>
        /// <para>
        /// A broken reference does not come through here: there is no effective value to materialize, and
        /// pretending the frozen literal is one would be the silent fallback the contract forbids. That case is
        /// the rack-scoped repair.
        /// </para>
        /// </summary>
        public static VariableMutationPreflightResult Unlink(
            ProjectVariablesDocument registry,
            string rackId,
            PropertyId propertyId,
            IReadOnlyList<ProjectVariableScanEntry> entries)
        {
            if (!TryAccredit(registry, out var usable, out var failure))
            {
                return failure;
            }

            if (!SelectiveLinkedProperties.All.TryGetDescriptor(propertyId, out var descriptor))
            {
                return VariableMutationPreflightResult.Failed(
                    "La propiedad '" + propertyId + "' no es una que esta version conozca.");
            }

            if (!TryRack(entries, rackId, out var consumer, out var rackFailure))
            {
                return rackFailure;
            }

            if (!consumer.Authored.HasBindingEntry(propertyId))
            {
                return NotBound(rackId, propertyId);
            }

            var current = Resolver.ResolveAgainst(consumer.Authored, usable);

            if (!current.IsSuccess)
            {
                return VariableMutationPreflightResult.Failed(
                    current.Error + " Desvincular exige un valor efectivo que materializar; para un vinculo " +
                    "roto usa la reparacion explicita.");
            }

            var authored = ProjectVariableCloning.Clone(consumer.Authored);

            // The ACTIVE step, now per property. Leaving the old literal here is the same as never having
            // bound the rack; writing another property's field would move the wrong geometry.
            descriptor.WriteAuthored(authored, descriptor.ReadEffective(current.Design));
            authored.PropertyValues.Remove(propertyId.Value);

            var effective = Resolver.ResolveAgainst(authored, usable);

            return effective.IsSuccess
                ? VariableMutationPreflightResult.Success(
                    MutationPlan.Of(
                        RegistryMutation.None,
                        new[] { new RackMutation(consumer.RackId, authored, effective.Design, consumer.Siblings) }))
                : VariableMutationPreflightResult.Failed(effective.Error);
        }

        /// <summary>
        /// Repairs the BROKEN references of a RACK, all of them, as one unit (I-48 G4B, Proposal V8 V3-R01).
        ///
        /// <para>
        /// <b>Repair is rack-scoped, and it has to be.</b> A <see cref="RackMutation"/> carries a COMPLETE
        /// effective design and the executor redraws every view from it, so while any other reference of the
        /// same rack is unresolvable there is nothing to hand it. That is why a per-property repair used to
        /// DEADLOCK a rack with two broken bindings: it removed one and then failed the whole-document check,
        /// leaving the rack neither editable nor repairable.
        /// </para>
        /// <para>
        /// Every guarantee of the accepted contract survives: a broken reference has NO effective value; there
        /// is no automatic fallback; the action is explicit and warned over the COMPLETE set; it uses the
        /// STORED literals and does not touch them; it removes the broken bindings; it keeps the promoted
        /// schema; and only AFTER they are gone do those literals govern again.
        /// </para>
        /// <para>
        /// ANY fatal state - unknown property, malformed reference, incompatible target - blocks the WHOLE
        /// rack. Those are not repairable, and offering a partial remedy would promise what the executor
        /// rejects.
        /// </para>
        /// </summary>
        public static VariableMutationPreflightResult RepairBrokenRack(
            ProjectVariablesDocument registry,
            string rackId,
            IReadOnlyList<ProjectVariableScanEntry> entries,
            bool confirmed)
        {
            if (!TryAccredit(registry, out var usable, out var failure))
            {
                return failure;
            }

            if (!TryRack(entries, rackId, out var consumer, out var rackFailure))
            {
                return rackFailure;
            }

            var descriptors = SelectiveLinkedProperties.All;
            var assessment = SelectiveLinkedPropertyKernel.Assess(consumer.Authored, descriptors, usable);

            if (assessment.IsBlocked)
            {
                return VariableMutationPreflightResult.Failed(
                    "El rack " + rackId + " no se puede reparar: " + assessment.BlockingReason +
                    " Mientras exista ese estado no hay diseno efectivo completo que dibujar.");
            }

            if (assessment.Missing.Count == 0)
            {
                return VariableMutationPreflightResult.Failed(
                    "El rack " + rackId + " no tiene vinculos rotos que reparar. Para quitar un vinculo sano " +
                    "sin cambiar la geometria, desvincula.");
            }

            if (!confirmed)
            {
                var detalle = new List<string>();

                foreach (var missing in assessment.Missing)
                {
                    var literal = descriptors.TryGetDescriptor(missing.PropertyId, out var descriptor)
                        ? descriptor.ReadAuthored(consumer.Authored).ToString(
                            System.Globalization.CultureInfo.InvariantCulture)
                        : "?";

                    detalle.Add(
                        "'" + missing.PropertyId + "' -> '" + missing.RawVariableId +
                        "' (literal almacenado " + literal + ")");
                }

                return VariableMutationPreflightResult.Failed(
                    "El rack " + rackId + " tiene " + assessment.Missing.Count + " vinculo(s) roto(s): " +
                    string.Join("; ", detalle) + ". NO hay valor efectivo que materializar; reparar usara esos " +
                    "literales almacenados y la geometria puede cambiar. Requiere confirmacion explicita del " +
                    "conjunto completo.");
            }

            var authored = ProjectVariableCloning.Clone(consumer.Authored);

            foreach (var missing in assessment.Missing)
            {
                // El literal almacenado NO se toca; lo que se retira es el binding. Solo entonces gobierna.
                authored.PropertyValues.Remove(missing.PropertyId.Value);
            }

            var effective = Resolver.ResolveAgainst(authored, usable);

            return effective.IsSuccess
                ? VariableMutationPreflightResult.Success(
                    MutationPlan.Of(
                        RegistryMutation.None,
                        new[] { new RackMutation(consumer.RackId, authored, effective.Design, consumer.Siblings) }))
                : VariableMutationPreflightResult.Failed(effective.Error);
        }

        // ------------------------------------------------------------------ helpers

        private static readonly SelectiveEffectiveDesignResolver Resolver = new SelectiveEffectiveDesignResolver();

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
