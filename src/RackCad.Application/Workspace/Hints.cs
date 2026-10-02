#nullable enable
using System.Collections.Generic;

namespace RackCad.Application.Workspace;

/// <summary>Kind of notification the bridge may queue. A hint only invalidates or marks; it is never authority (D-04).</summary>
public enum HintKind
{
    ImpliedSelectionChanged,
    DatabaseObjectChanged,
    CommandEnded,
}

/// <summary>The only operations a drain may run in F1 (D-04). RefreshIndex arrives with F6 and is not listed.</summary>
public enum DrainOperation
{
    ReadImplicitSelection,
    RecomputeSelectionContext,
    InvalidateNavigationCaches,
    MarkDraftsPossiblyStale,
}

/// <summary>State the host reports at drain time. Drains need a quiescent active document, no RackCad modal and a visible panel.</summary>
public readonly record struct DrainConditions(bool IsQuiescent, bool ModalRackCadActive, bool PanelVisible);

/// <summary>Outcome of one drain attempt.</summary>
public sealed class DrainResult
{
    public DrainResult(bool drained, IReadOnlyList<DrainOperation> operations)
    {
        Drained = drained;
        Operations = operations;
    }

    public bool Drained { get; }

    public IReadOnlyList<DrainOperation> Operations { get; }
}
