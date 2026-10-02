#nullable enable
using System;
using System.Collections.Generic;

namespace RackCad.Application.Workspace;

public enum SelectionContextKind
{
    None,
    One,
    Selection,
    NoIdentity,
    Diagnostic,
}

/// <summary>
/// What the capture saw of one selected block reference definition: whether its envelope is
/// interpretable, the Id as written in the envelope, and an Id attributed by probing (diagnostic only).
/// </summary>
public readonly record struct DefinitionFact(bool IsInterpretable, string? EnvelopeId, string? ProbedId = null);

/// <summary>Pure selection context of a session (D-05, D-06). Value equality: an equal context does not navigate.</summary>
public sealed record SelectionContext
{
    private SelectionContext(SelectionContextKind kind, string? rackId, int rackCount, int otherCount)
    {
        Kind = kind;
        RackId = rackId;
        RackCount = rackCount;
        OtherCount = otherCount;
    }

    public SelectionContextKind Kind { get; }

    /// <summary>Set only for <see cref="SelectionContextKind.One"/>; never invented for any other kind.</summary>
    public string? RackId { get; }

    public int RackCount { get; }

    public int OtherCount { get; }

    public static SelectionContext None { get; } = new(SelectionContextKind.None, null, 0, 0);

    public static SelectionContext NoIdentity { get; } = new(SelectionContextKind.NoIdentity, null, 0, 0);

    public static SelectionContext Diagnostic { get; } = new(SelectionContextKind.Diagnostic, null, 0, 0);

    public static SelectionContext One(string rackId)
    {
        if (string.IsNullOrWhiteSpace(rackId))
            throw new ArgumentException("A rack id must not be empty.", nameof(rackId));
        return new SelectionContext(SelectionContextKind.One, rackId, 1, 0);
    }

    public static SelectionContext Selection(int rackCount, int otherCount) =>
        new(SelectionContextKind.Selection, null, rackCount, otherCount);

    /// <summary>
    /// Classifies the selected references. "No Id" is <see cref="string.IsNullOrWhiteSpace"/>; a probed Id
    /// never makes a rack identity, and an uninterpretable definition is a Diagnostic.
    /// </summary>
    public static SelectionContext Classify(IReadOnlyList<DefinitionFact> references, int nonRackEntities)
    {
        return None; // skeleton: behaviour arrives with GREEN
    }
}
