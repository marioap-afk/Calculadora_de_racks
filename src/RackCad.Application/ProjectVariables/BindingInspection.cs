using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using RackCad.Application.Expressions;
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

        /// <summary>The source itself is statically or numerically invalid.</summary>
        RepairableIntrinsic = 6,

        /// <summary>One or more present variables read by the source failed.</summary>
        RepairableUpstream = 7,

        /// <summary>The source evaluated, but its value violates the property boundary.</summary>
        RepairableDomain = 8,

        /// <summary>The target EXISTS but carries a type this property cannot take. Never repaired silently, never healthy.</summary>
        FatalIncompatibleTarget = 3,

        /// <summary>A property token this build does not declare — including one that is not a parseable id at all.</summary>
        FatalUnknownProperty = 4,

        /// <summary>Unknown reference kind, unreadable <see cref="VariableId"/>, or an absent payload.</summary>
        FatalMalformedReference = 5,
    }

    /// <summary>
    /// Which of the two malformed shapes a reference had. It is NOT a sixth classification: both are
    /// <see cref="BindingInspectionOutcome.FatalMalformedReference"/> with the same disposition. It exists
    /// because the resolver's published outcome vocabulary already distinguished them, and that distinction
    /// cannot be inferred from whether an id string happened to be present.
    /// </summary>
    internal enum MalformedReferenceReason
    {
        None = 0,

        /// <summary>A reference kind this build does not implement.</summary>
        UnknownKind = 1,

        /// <summary>The kind was right but the <see cref="VariableId"/> could not be read.</summary>
        UnreadableVariableId = 2,
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
            string detail,
            MalformedReferenceReason malformed = MalformedReferenceReason.None,
            RepairDecisionReason repairReason = null,
            IReadOnlyList<SymbolId> readSymbols = null,
            double? effectiveValue = null)
        {
            Outcome = outcome;
            Malformed = malformed;
            PropertyToken = propertyToken;
            PropertyId = propertyId;
            RawVariableId = rawVariableId;
            VariableId = variableId;
            Target = target;
            Detail = detail;
            RepairReason = repairReason;
            ReadSymbols = readSymbols ?? Array.Empty<SymbolId>();
            EffectiveValue = effectiveValue;
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

        /// <summary>Which malformed shape it was, when the outcome is the malformed one.</summary>
        internal MalformedReferenceReason Malformed { get; }

        internal RepairDecisionReason RepairReason { get; }

        internal IReadOnlyList<SymbolId> ReadSymbols { get; }

        internal double? EffectiveValue { get; }

        internal bool IsHealthy => Outcome == BindingInspectionOutcome.Healthy;

        internal bool IsRepairable
            => Outcome == BindingInspectionOutcome.RepairableMissingTarget ||
               Outcome == BindingInspectionOutcome.RepairableIntrinsic ||
               Outcome == BindingInspectionOutcome.RepairableUpstream ||
               Outcome == BindingInspectionOutcome.RepairableDomain;

        internal bool IsFatal
            => Outcome == BindingInspectionOutcome.FatalIncompatibleTarget ||
               Outcome == BindingInspectionOutcome.FatalUnknownProperty ||
               Outcome == BindingInspectionOutcome.FatalMalformedReference;

        internal static BindingInspection Healthy(
            string token, PropertyId propertyId, VariableId variableId, VariableTargetSnapshot target,
            double value, IReadOnlyList<SymbolId> reads)
            => new BindingInspection(
                BindingInspectionOutcome.Healthy, token, propertyId, variableId.Value, variableId, target, null,
                readSymbols: reads, effectiveValue: value);

        internal static BindingInspection Missing(
            string token, PropertyId propertyId, VariableId variableId, string detail,
            RepairDecisionReason reason = null, IReadOnlyList<SymbolId> reads = null)
            => new BindingInspection(
                BindingInspectionOutcome.RepairableMissingTarget, token, propertyId, variableId.Value, variableId, null, detail,
                repairReason: reason ?? RepairDecisionReason.MissingTarget(new[] { SymbolId.ProjectVariable(variableId.Value) }),
                readSymbols: reads);

        internal static BindingInspection Semantic(
            BindingInspectionOutcome outcome,
            string token,
            PropertyId propertyId,
            RepairDecisionReason reason,
            IReadOnlyList<SymbolId> reads,
            string detail)
            => new BindingInspection(outcome, token, propertyId, null, default, null, detail,
                repairReason: reason, readSymbols: reads);

        internal static BindingInspection Incompatible(
            string token, PropertyId propertyId, VariableId variableId, VariableTargetSnapshot target, string detail)
            => new BindingInspection(
                BindingInspectionOutcome.FatalIncompatibleTarget, token, propertyId, variableId.Value, variableId, target, detail);

        internal static BindingInspection UnknownProperty(string token, PropertyId propertyId, string detail)
            => new BindingInspection(
                BindingInspectionOutcome.FatalUnknownProperty, token, propertyId, null, default, null, detail);

        internal static BindingInspection MalformedReference(
            string token,
            PropertyId propertyId,
            string rawVariableId,
            string detail,
            MalformedReferenceReason reason)
            => new BindingInspection(
                BindingInspectionOutcome.FatalMalformedReference,
                token, propertyId, rawVariableId, default, null, detail, reason);
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
            => InspectBinding(
                rawPropertyToken,
                rawReference,
                descriptors,
                registry,
                RegistryEvaluation.Evaluate(ProjectVariablesExpressionAdapter.From(registry)));

        internal static BindingInspection InspectBinding(
            string rawPropertyToken,
            SelectivePropertyValueDocument rawReference,
            LinkedPropertyDescriptorSet descriptors,
            UsableProjectVariablesRegistry registry,
            RegistryEvaluation evaluation)
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
                    "declara un vinculo sobre una propiedad que esta version no conoce ('" +
                    (rawPropertyToken ?? "<null>") + "').");
            }

            if (rawReference == null)
            {
                return BindingInspection.MalformedReference(
                    rawPropertyToken,
                    propertyId,
                    rawReference?.VariableId,
                    "propiedad '" + propertyId + "': el vinculo es de una clase que esta version no conoce ('" +
                    (rawReference?.Kind ?? "<null>") + "').",
                    MalformedReferenceReason.UnknownKind);
            }


            if (string.Equals(rawReference.Kind, SelectivePropertyValueDocument.ExpressionKind, StringComparison.Ordinal))
            {
                return InspectExpression(rawPropertyToken, propertyId, rawReference, descriptor, registry, evaluation);
            }

            if (!string.Equals(rawReference.Kind, SelectivePropertyValueDocument.ProjectVariableKind, StringComparison.Ordinal))
            {
                return BindingInspection.MalformedReference(
                    rawPropertyToken,
                    propertyId,
                    rawReference.VariableId,
                    "propiedad '" + propertyId + "': el vinculo es de una clase que esta version no conoce ('" +
                    (rawReference.Kind ?? "<null>") + "').",
                    MalformedReferenceReason.UnknownKind);
            }

            if (!VariableId.TryParse(rawReference.VariableId, out var variableId))
            {
                return BindingInspection.MalformedReference(
                    rawPropertyToken,
                    propertyId,
                    rawReference.VariableId,
                    "propiedad '" + propertyId + "': el vinculo apunta a un id de variable ilegible ('" +
                    (rawReference.VariableId ?? "<null>") + "').",
                    MalformedReferenceReason.UnreadableVariableId);
            }

            if (!registry.TryGetTarget(variableId, out var target))
            {
                return BindingInspection.Missing(
                    rawPropertyToken,
                    propertyId,
                    variableId,
                    "propiedad '" + propertyId + "': la variable de proyecto '" + variableId +
                    "' no existe en este dibujo. No hay valor efectivo que aplicar.");
            }

            if (target.VariableType != descriptor.VariableType)
            {
                return BindingInspection.Incompatible(
                    rawPropertyToken,
                    propertyId,
                    variableId,
                    target,
                    "propiedad '" + propertyId + "': exige una variable de tipo " + descriptor.VariableType +
                    ", pero '" + variableId + "' es de tipo " + target.VariableType + ".");
            }

            var symbol = SymbolId.ProjectVariable(variableId.Value);
            var variableResult = evaluation.Result(symbol);
            if (!variableResult.Succeeded)
            {
                return BindingInspection.Semantic(
                    BindingInspectionOutcome.RepairableUpstream,
                    rawPropertyToken,
                    propertyId,
                    RepairDecisionReason.Upstream(new[] { new KeyValuePair<SymbolId, RegistrySymbolResult>(symbol, variableResult) }),
                    new[] { symbol },
                    "propiedad '" + propertyId + "': la variable referida fallo: " +
                    string.Join(", ", variableResult.RootCauses.Select(root => root.Code.ToString())) + ".");
            }

            if (!AcceptsDomain(variableResult.Value))
            {
                return BindingInspection.Semantic(
                    BindingInspectionOutcome.RepairableDomain,
                    rawPropertyToken,
                    propertyId,
                    RepairDecisionReason.Domain(),
                    new[] { symbol },
                    "propiedad '" + propertyId + "': el valor queda fuera de dominio.");
            }

            return BindingInspection.Healthy(rawPropertyToken, propertyId, variableId, target, variableResult.Value, new[] { symbol });
        }

        private static BindingInspection InspectExpression(
            string token,
            PropertyId propertyId,
            SelectivePropertyValueDocument source,
            SelectiveLinkedPropertyDescriptor descriptor,
            UsableProjectVariablesRegistry registry,
            RegistryEvaluation evaluation)
        {
            if (source.Expression == null)
            {
                return BindingInspection.MalformedReference(
                    token, propertyId, null,
                    "propiedad '" + propertyId + "': la expresion no contiene un arbol enlazado.",
                    MalformedReferenceReason.UnknownKind);
            }

            var reads = BoundExpressionDependencies.DirectDependencies(source.Expression).OrderBy(id => id).ToArray();
            var missing = new List<SymbolId>();
            var targets = new List<VariableTargetSnapshot>();
            foreach (var symbol in reads)
            {
                if (symbol.Namespace != SymbolNamespace.ProjectVariable ||
                    !VariableId.TryParse(symbol.Key, out var variableId) ||
                    !registry.TryGetTarget(variableId, out var target))
                {
                    missing.Add(symbol);
                }
                else
                {
                    targets.Add(target);
                }
            }

            if (missing.Count > 0)
            {
                return BindingInspection.Missing(
                    token, propertyId, default,
                    "propiedad '" + propertyId + "': la expresion refiere variables ausentes.",
                    RepairDecisionReason.MissingTarget(missing), reads);
            }

            var incompatible = targets.FirstOrDefault(target => target.VariableType != descriptor.VariableType);
            if (incompatible != null)
            {
                return BindingInspection.Incompatible(
                    token, propertyId, incompatible.VariableId, incompatible,
                    "propiedad '" + propertyId + "': una variable referida tiene tipo incompatible.");
            }

            var staticFailures = StaticFailures(source.Expression, evaluation.Context.Functions);
            if (staticFailures.Count > 0)
            {
                return BindingInspection.Semantic(
                    BindingInspectionOutcome.RepairableIntrinsic,
                    token,
                    propertyId,
                    RepairDecisionReason.Intrinsic(staticFailures),
                    reads,
                    "propiedad '" + propertyId + "': la expresion tiene un fallo propio: " +
                    string.Join(", ", staticFailures) + ".");
            }

            var failedReads = new List<KeyValuePair<SymbolId, RegistrySymbolResult>>();
            var inputs = new Dictionary<SymbolId, double>();
            foreach (var symbol in reads)
            {
                var result = evaluation.Result(symbol);
                if (result.Succeeded)
                {
                    inputs.Add(symbol, result.Value);
                }
                else
                {
                    failedReads.Add(new KeyValuePair<SymbolId, RegistrySymbolResult>(symbol, result));
                }
            }

            if (failedReads.Count > 0)
            {
                return BindingInspection.Semantic(
                    BindingInspectionOutcome.RepairableUpstream,
                    token,
                    propertyId,
                    RepairDecisionReason.Upstream(failedReads),
                    reads,
                    "propiedad '" + propertyId + "': una variable leida fallo: " + string.Join(", ",
                        failedReads.SelectMany(read => read.Value.RootCauses).Select(root => root.Code.ToString()).Distinct()) + ".");
            }

            var numeric = ExpressionEvaluator.Evaluate(source.Expression, evaluation.Context, inputs);
            if (!numeric.Succeeded)
            {
                return BindingInspection.Semantic(
                    BindingInspectionOutcome.RepairableIntrinsic,
                    token,
                    propertyId,
                    RepairDecisionReason.Intrinsic(numeric.Diagnostics.Select(item => item.Code.ToString())),
                    reads,
                    "propiedad '" + propertyId + "': la expresion tiene un fallo numerico: " +
                    string.Join(", ", numeric.Diagnostics.Select(item => item.Code.ToString())) + ".");
            }

            if (!AcceptsDomain(numeric.Value))
            {
                return BindingInspection.Semantic(
                    BindingInspectionOutcome.RepairableDomain,
                    token,
                    propertyId,
                    RepairDecisionReason.Domain(),
                    reads,
                    "propiedad '" + propertyId + "': el valor queda fuera de dominio.");
            }

            return BindingInspection.Healthy(token, propertyId, default, null, numeric.Value, reads);
        }

        private static IReadOnlyList<string> StaticFailures(BoundExpression expression, FunctionRegistry functions)
        {
            var failures = new SortedSet<string>(StringComparer.Ordinal);
            var pending = new Stack<BoundExpression>();
            pending.Push(expression);
            while (pending.Count > 0)
            {
                switch (pending.Pop())
                {
                    case BoundNegate negate:
                        pending.Push(negate.Operand);
                        break;
                    case BoundBinary binary:
                        pending.Push(binary.Right);
                        pending.Push(binary.Left);
                        break;
                    case BoundCall call:
                        if (!functions.AcceptsArity(call.Function, call.Arguments.Count))
                        {
                            failures.Add("InvalidArguments:" + functions.Token(call.Function) + ":" +
                                call.Arguments.Count.ToString(System.Globalization.CultureInfo.InvariantCulture));
                        }
                        for (var index = call.Arguments.Count - 1; index >= 0; index--)
                        {
                            pending.Push(call.Arguments[index]);
                        }
                        break;
                }
            }

            if (BoundExpressionSemanticValidation.IsNonCanonical(expression))
            {
                failures.Add(ExpressionDiagnosticCode.NonCanonicalForm.ToString());
            }

            return new ReadOnlyCollection<string>(failures.ToArray());
        }

        private static bool AcceptsDomain(double value) => value > 0.0;
    }
}
