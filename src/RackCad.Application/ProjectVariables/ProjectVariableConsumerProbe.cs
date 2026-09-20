using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Expressions;
using RackCad.Application.Persistence;

namespace RackCad.Application.ProjectVariables
{
    /// <summary>What a view can be said to answer about one project variable.</summary>
    public enum ConsumerProbeOutcome
    {
        /// <summary>The whole binding map was understood, and at least one reference uses the target.</summary>
        Positive = 1,

        /// <summary>
        /// The whole binding map was understood, and NO reference uses the target. This is a claim, and it is
        /// only made when it can be proven.
        /// </summary>
        Negative = 2,

        /// <summary>
        /// There is something here this build does not understand, so nothing can be claimed either way. It is
        /// not a soft negative: it is the absence of an answer.
        /// </summary>
        Indeterminate = 3,
    }

    /// <summary>
    /// Identifies consumers of a project variable POSITIVELY: by what is found, never by what is missing.
    ///
    /// <para>
    /// The whole point is the meaning of <see cref="ConsumerProbeOutcome.Negative"/>. It says, literally,
    /// "the entire <c>PropertyValues</c> map was understood and none of it references the target". Anything
    /// this build cannot interpret — a property id it does not know, a reference kind from a later version, an
    /// unreadable variable id, a null entry, a payload that does not deserialize — is
    /// <see cref="ConsumerProbeOutcome.Indeterminate"/>.
    /// </para>
    /// <para>
    /// Collapsing the two would turn a drawing written by a newer build into a drawing silently unrelated to
    /// the variable: the propagation would skip views that DO consume it, and leave them showing the old
    /// value while the rest of the drawing moved. That is the failure mode this type exists to make
    /// impossible.
    /// </para>
    /// <para>
    /// It also means a rack is ignored only when it is DEMONSTRABLY unrelated. A rack this build cannot fully
    /// read was never demonstrated to be unrelated, so it never qualified.
    /// </para>
    /// </summary>
    public static class ProjectVariableConsumerProbe
    {
        /// <summary>
        /// Probes one authored design. A null document means the payload could not be read, which is
        /// <see cref="ConsumerProbeOutcome.Indeterminate"/> — never an empty map.
        /// </summary>
        public static ConsumerProbeOutcome Probe(SelectivePalletDesignDocument authored, VariableId target)
            => Probe(authored, new[] { target });

        /// <summary>Probes a complete affected closure in one pass.</summary>
        public static ConsumerProbeOutcome Probe(
            SelectivePalletDesignDocument authored,
            IReadOnlyCollection<VariableId> targets)
        {
            if (authored == null)
            {
                return ConsumerProbeOutcome.Indeterminate;
            }

            if (authored.PropertyValues == null)
            {
                return ConsumerProbeOutcome.Negative;
            }

            var found = false;

            // The map is walked WHOLE before anything can be declared negative, and an entry that cannot be
            // understood wins even when another one matched: nothing can be affirmed about a map that is not
            // fully understood.
            foreach (var entry in authored.PropertyValues)
            {
                if (!PropertyId.TryParse(entry.Key, out var propertyId) ||
                    !ProjectPropertyIds.IsKnown(propertyId))
                {
                    return ConsumerProbeOutcome.Indeterminate;
                }

                var reference = entry.Value;

                if (reference == null)
                {
                    return ConsumerProbeOutcome.Indeterminate;
                }

                if (string.Equals(reference.Kind, SelectivePropertyValueDocument.ProjectVariableKind, StringComparison.Ordinal))
                {
                    if (!VariableId.TryParse(reference.VariableId, out var variableId))
                    {
                        return ConsumerProbeOutcome.Indeterminate;
                    }

                    if (targets.Contains(variableId))
                    {
                        found = true;
                    }
                    continue;
                }

                if (string.Equals(reference.Kind, SelectivePropertyValueDocument.ExpressionKind, StringComparison.Ordinal) &&
                    reference.Expression != null)
                {
                    foreach (var dependency in BoundExpressionDependencies.DirectDependencies(reference.Expression))
                    {
                        if (!VariableId.TryParse(dependency.Key, out var variableId))
                        {
                            return ConsumerProbeOutcome.Indeterminate;
                        }

                        if (targets.Contains(variableId))
                        {
                            found = true;
                        }
                    }
                    continue;
                }

                return ConsumerProbeOutcome.Indeterminate;
            }

            return found ? ConsumerProbeOutcome.Positive : ConsumerProbeOutcome.Negative;
        }

        /// <summary>
        /// Probes one swept view. An entry whose envelope or design could not be read is
        /// <see cref="ConsumerProbeOutcome.Indeterminate"/>: an unreadable sibling is not a sibling that says no.
        /// </summary>
        public static ConsumerProbeOutcome Probe(ProjectVariableScanEntry entry, VariableId target)
            => entry == null || !entry.OuterEnvelopeInterpretable || !entry.AuthoredReadable
                ? ConsumerProbeOutcome.Indeterminate
                : Probe(entry.Authored, target);

        internal static ConsumerProbeOutcome Probe(
            ProjectVariableScanEntry entry, IReadOnlyCollection<VariableId> targets)
            => entry == null || !entry.OuterEnvelopeInterpretable || !entry.AuthoredReadable
                ? ConsumerProbeOutcome.Indeterminate
                : Probe(entry.Authored, targets);
    }
}
