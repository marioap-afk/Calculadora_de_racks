#nullable enable
using System;
using System.Collections.Generic;

namespace RackCad.Application.Workspace
{
    /// <summary>View group of a target. Frontal, lateral and plan come first; everything else the codec does not place follows.</summary>
    public enum NavigationViewOrder
    {
        Frontal = 0,
        Lateral = 1,
        Planta = 2,
        Other = 3,
    }

    /// <summary>One top-level Model Space reference that belongs to the rack.</summary>
    public sealed record NavigationTarget(
        long ReferenceHandle,
        long DefinitionHandle,
        NavigationViewOrder ViewOrder,
        string? Kind,
        string? View,
        int Section,
        string? Name);

    public enum NavigationDiagnosticKind
    {
        UnreadableMember,
    }

    /// <summary>A diagnostic about the rack. A probed member is only ever reported here, never offered as a target.</summary>
    public sealed record NavigationDiagnostic(NavigationDiagnosticKind Kind, long DefinitionHandle);

    /// <summary>The deterministic set of navigation targets of one (session, RackId).</summary>
    public sealed class NavigationTargetSet
    {
        public NavigationTargetSet(RackKey key, IReadOnlyList<NavigationTarget> targets, IReadOnlyList<NavigationDiagnostic> diagnostics)
        {
            Key = key;
            Targets = targets;
            Diagnostics = diagnostics;
        }

        public RackKey Key { get; }

        public IReadOnlyList<NavigationTarget> Targets { get; }

        public IReadOnlyList<NavigationDiagnostic> Diagnostics { get; }

        public bool IsEmpty => Targets.Count == 0;

        /// <summary>First target of the deterministic order, or null when there is none.</summary>
        public NavigationTarget? Primary => Targets.Count == 0 ? null : Targets[0];

        /// <summary>Reference handles in the deterministic order of the targets.</summary>
        public IReadOnlyList<long> ReferenceHandles()
        {
            var handles = new long[Targets.Count];
            for (var i = 0; i < handles.Length; i++) handles[i] = Targets[i].ReferenceHandle;
            return handles;
        }
    }
}
