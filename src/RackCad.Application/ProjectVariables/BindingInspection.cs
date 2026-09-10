using System;
using RackCad.Application.Persistence;

namespace RackCad.Application.ProjectVariables
{
    /// <summary>
    /// What ONE persisted binding is, once a usable register and a validated descriptor set are in hand.
    ///
    /// <para>
    /// The five cases name their CAUSE, and that is not cosmetic. A three-way split into healthy / repairable /
    /// fatal, decided by asking whether the document resolves, would classify an existing-but-incompatible
    /// target as HEALTHY — the most dangerous bucket of the three — because resolution never checked the type.
    /// </para>
    /// <para>
    /// Only <see cref="RepairableMissingTarget"/> is repairable. Every FATAL is fail-closed: zero plan, and
    /// never a fall back to the stored literal.
    /// </para>
    /// </summary>
    internal enum BindingInspectionOutcome
    {
        /// <summary>Known property, well-formed reference, target present, and its type is the one the property requires.</summary>
        Healthy = 1,

        /// <summary>Known property, well-formed reference, and the target simply is not in the register.</summary>
        RepairableMissingTarget = 2,

        /// <summary>The target EXISTS but carries a type this property cannot take. Never repaired silently, never healthy.</summary>
        FatalIncompatibleTarget = 3,

        /// <summary>A property token this build does not declare — including one that is not a parseable id at all.</summary>
        FatalUnknownProperty = 4,

        /// <summary>Unknown reference kind, unreadable <see cref="VariableId"/>, or an absent payload.</summary>
        FatalMalformedReference = 5,
    }

    /// <summary>The classification of one binding, with what a caller needs to act on it or explain it.</summary>
    internal sealed class BindingInspection
    {
        private BindingInspection(
            BindingInspectionOutcome outcome,
            string propertyToken,
            PropertyId propertyId,
            string rawVariableId,
            VariableId variableId,
            VariableTargetSnapshot target,
            string detail)
        {
            Outcome = outcome;
            PropertyToken = propertyToken;
            PropertyId = propertyId;
            RawVariableId = rawVariableId;
            VariableId = variableId;
            Target = target;
            Detail = detail;
        }

        internal BindingInspectionOutcome Outcome { get; }

        /// <summary>The key as it was persisted, verbatim — the only thing that can be shown for an unknown token.</summary>
        internal string PropertyToken { get; }

        /// <summary>The parsed property. Default when the token did not parse.</summary>
        internal PropertyId PropertyId { get; }

        /// <summary>The referenced id, RAW: unreadable is not the same as absent.</summary>
        internal string RawVariableId { get; }

        /// <summary>The parsed reference. Default when it did not parse.</summary>
        internal VariableId VariableId { get; }

        /// <summary>
        /// The target this binding resolves to. Non-null ONLY for <see cref="BindingInspectionOutcome.Healthy"/>
        /// and for <see cref="BindingInspectionOutcome.FatalIncompatibleTarget"/>, so a later consumer never
        /// has to look it up a second time — and, if it does, it must not re-decide existence, type or identity.
        /// </summary>
        internal VariableTargetSnapshot Target { get; }

        /// <summary>The visible reason. Null when healthy.</summary>
        internal string Detail { get; }

        internal bool IsHealthy => Outcome == BindingInspectionOutcome.Healthy;

        internal bool IsRepairable => Outcome == BindingInspectionOutcome.RepairableMissingTarget;

        internal bool IsFatal
            => Outcome == BindingInspectionOutcome.FatalIncompatibleTarget ||
               Outcome == BindingInspectionOutcome.FatalUnknownProperty ||
               Outcome == BindingInspectionOutcome.FatalMalformedReference;

        internal static BindingInspection Healthy(
            string token, PropertyId propertyId, VariableId variableId, VariableTargetSnapshot target)
            => new BindingInspection(
                BindingInspectionOutcome.Healthy, token, propertyId, variableId.Value, variableId, target, null);

        internal static BindingInspection Missing(
            string token, PropertyId propertyId, VariableId variableId, string detail)
            => new BindingInspection(
                BindingInspectionOutcome.RepairableMissingTarget, token, propertyId, variableId.Value, variableId, null, detail);

        internal static BindingInspection Incompatible(
            string token, PropertyId propertyId, VariableId variableId, VariableTargetSnapshot target, string detail)
            => new BindingInspection(
                BindingInspectionOutcome.FatalIncompatibleTarget, token, propertyId, variableId.Value, variableId, target, detail);

        internal static BindingInspection UnknownProperty(string token, PropertyId propertyId, string detail)
            => new BindingInspection(
                BindingInspectionOutcome.FatalUnknownProperty, token, propertyId, null, default, null, detail);

        internal static BindingInspection Malformed(
            string token, PropertyId propertyId, string rawVariableId, string detail)
            => new BindingInspection(
                BindingInspectionOutcome.FatalMalformedReference, token, propertyId, rawVariableId, default, null, detail);
    }

    /// <summary>
    /// THE single semantic authority over one persisted binding (I-48 G4A, Proposal V8 R-02/V6-R02).
    ///
    /// <para>
    /// It owns ALL of it: interpreting the property token, resolving the token to a descriptor, validating the
    /// reference kind and payload, parsing the <see cref="VariableId"/>, deciding whether the target exists,
    /// and comparing the target's type against the one the property requires. Taking an already-resolved
    /// descriptor as input would have made <see cref="BindingInspectionOutcome.FatalUnknownProperty"/>
    /// unreachable from here and pushed that decision to whoever did the lookup — splitting the authority on
    /// day one.
    /// </para>
    /// <para>
    /// Duplicate <see cref="VariableId"/> is NOT decided here. It cannot be: the accreditation that produces a
    /// <see cref="UsableProjectVariablesRegistry"/> fails first, so this function cannot be called with an
    /// ambiguous identity.
    /// </para>
    /// <para>
    /// A malformed property token is not a separate case. It resolves to no descriptor, which is exactly what
    /// UNKNOWN means, and both dispositions are identical — the inherited resolver and consumer probe already
    /// collapse them the same way.
    /// </para>
    /// </summary>
    internal static class LinkedPropertyInspection
    {
        internal static BindingInspection InspectBinding(
            string rawPropertyToken,
            SelectivePropertyValueDocument rawReference,
            LinkedPropertyDescriptorSet descriptors,
            UsableProjectVariablesRegistry registry)
        {
            if (descriptors == null)
            {
                throw new ArgumentNullException(nameof(descriptors));
            }

            if (registry == null)
            {
                throw new ArgumentNullException(nameof(registry));
            }

            if (!PropertyId.TryParse(rawPropertyToken, out var propertyId) ||
                !descriptors.TryGetDescriptor(propertyId, out var descriptor))
            {
                return BindingInspection.UnknownProperty(
                    rawPropertyToken,
                    propertyId,
                    "La propiedad '" + (rawPropertyToken ?? "<null>") + "' no es una que esta version conozca.");
            }

            if (rawReference == null ||
                !string.Equals(
                    rawReference.Kind,
                    SelectivePropertyValueDocument.ProjectVariableKind,
                    StringComparison.Ordinal))
            {
                return BindingInspection.Malformed(
                    rawPropertyToken,
                    propertyId,
                    rawReference?.VariableId,
                    "El vinculo de '" + propertyId + "' es de una clase que esta version no conoce ('" +
                    (rawReference?.Kind ?? "<null>") + "').");
            }

            if (!VariableId.TryParse(rawReference.VariableId, out var variableId))
            {
                return BindingInspection.Malformed(
                    rawPropertyToken,
                    propertyId,
                    rawReference.VariableId,
                    "El vinculo de '" + propertyId + "' apunta a un id de variable ilegible ('" +
                    (rawReference.VariableId ?? "<null>") + "').");
            }

            if (!registry.TryGetTarget(variableId, out var target))
            {
                return BindingInspection.Missing(
                    rawPropertyToken,
                    propertyId,
                    variableId,
                    "La variable de proyecto '" + variableId + "' no existe en este dibujo.");
            }

            if (target.VariableType != descriptor.VariableType)
            {
                return BindingInspection.Incompatible(
                    rawPropertyToken,
                    propertyId,
                    variableId,
                    target,
                    "La propiedad '" + propertyId + "' exige una variable de tipo " + descriptor.VariableType +
                    ", pero '" + variableId + "' es de tipo " + target.VariableType + ".");
            }

            return BindingInspection.Healthy(rawPropertyToken, propertyId, variableId, target);
        }
    }
}
