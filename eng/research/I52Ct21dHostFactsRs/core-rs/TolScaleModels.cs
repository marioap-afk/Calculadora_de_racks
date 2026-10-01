namespace I52Ct21d.HostFacts.Rs.Core;

// Plain data and thin interfaces between the TOL_SCALE logic (this assembly, no AutoCAD) and the AutoCAD-touching adapters (the RS DLL).
// Nothing in this file refers to AutoCAD: the runner is driven by a fake host in the tests (design 5.4).

/// <summary>What the host did with the construction of one probe row: a handle, or the text of the error it raised (a host rejection).</summary>
public sealed class RowConstruction
{
    public string RowId { get; }
    public string? Handle { get; }
    public string? Rejection { get; }

    public RowConstruction(string rowId, string? handle, string? rejection)
    {
        RowId = rowId;
        Handle = handle;
        Rejection = rejection;
    }
}

/// <summary>
/// What one read of a block reference returned (binary64 patterns, never decimal text): the three scale factors, the rotation, the normal and
/// the three column lengths of <c>BlockTransform</c> (the witness of design 2.4 step S5). Any null member is an unread member.
/// </summary>
public sealed class ReferenceReading
{
    public string Handle { get; }
    public IReadOnlyList<string>? ScaleHex { get; }
    public string? RotationHex { get; }
    public IReadOnlyList<string>? NormalHex { get; }
    public IReadOnlyList<string>? ColumnLengthsHex { get; }
    public string? ReadError { get; }

    public ReferenceReading(string handle, IReadOnlyList<string>? scaleHex, string? rotationHex, IReadOnlyList<string>? normalHex,
        IReadOnlyList<string>? columnLengthsHex, string? readError)
    {
        Handle = handle;
        ScaleHex = scaleHex;
        RotationHex = rotationHex;
        NormalHex = normalHex;
        ColumnLengthsHex = columnLengthsHex;
        ReadError = readError;
    }
}

/// <summary>A side database of the TOL_SCALE probe (in memory, never attached to a document). Disposing it disposes the host database.</summary>
public interface ITolScaleDatabase : IDisposable
{
    string Id { get; }

    /// <summary>T1: constructs one <c>BlockReference</c> per row, assigns <c>ScaleFactors</c>, appends to model space and COMMITS.</summary>
    IReadOnlyList<RowConstruction> AppendReferences(IReadOnlyList<I52Ct21d.HostFacts.Core.ProbeRow> rows);

    /// <summary>T2 / T3: reads every reference of model space in a transaction that is never committed.</summary>
    IReadOnlyList<ReferenceReading> ReadReferences();

    /// <summary>OP2 step 1: saves this side database as a NEW file (the single <c>SaveAs</c> of the RS DLL) and returns its ledger entry.</summary>
    ScratchFileEntry SaveToScratch(ScratchTarget target);

    /// <summary>OP2 step 2: opens the saved file in a SECOND side database (<c>ReadDwgFile</c> of an authorized source).</summary>
    ITolScaleDatabase Reopen(ReadableSource source);
}

public interface ITolScaleHost
{
    /// <summary>Creates the side database and its target block (<c>CT21D_TOLSCALE_TARGET</c> with one line).</summary>
    ITolScaleDatabase CreateSideDatabase();
}
