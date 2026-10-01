namespace I52Ct21d.HostFacts.Core;

// Plain data and thin interfaces that separate the AutoCAD-touching adapters (r0/) from the logic of this library.
// Nothing in this file refers to AutoCAD: the census builders are driven by a fake reader in the tests (design 5.4).

/// <summary>The stored <c>DSTYLE</c> section of the <c>ACAD</c> extended data of a dimension (BA-05 V5 2.3.2): pairs in host order.</summary>
public sealed record DstylePair(int Code, string HostTypeFullName);

public sealed record DstyleInfo(bool HasSection, bool WellFormed, IReadOnlyList<DstylePair> Pairs);

/// <summary>One entity of a block table record as the adapter read it. <see cref="ReadError"/> non-null means UNKNOWN for this entity.</summary>
public sealed record EntityInfo(
    string RxClassName,
    string DxfName,
    bool IsProxyOrCustom,
    string? Layer,
    string? Linetype,
    string? TextStyle,
    string? DimStyle,
    string? ReferencedBlockName,
    bool ReferencesAnonymousNonDynamic,
    IReadOnlyList<double>? Scale,
    DstyleInfo? Dstyle,
    string? ReadError);

/// <summary>One block table record (named, anonymous or layout) of the library file.</summary>
public sealed record BlockRecordInfo(
    string Name,
    bool IsLayout,
    bool IsAnonymous,
    bool IsDynamic,
    IReadOnlyList<EntityInfo> Entities,
    string? ReadError);

/// <summary>Every stored result-buffer entry reachable read-only (Xrecords of the dictionaries, extended data of entities).</summary>
public sealed record StoredBufferScan(IReadOnlyList<StoredEntry> Entries, int BuffersRead, int ReadFailures);

/// <summary>A reader over an in-memory side database that was filled from a private copy. It never saves and never writes.</summary>
public interface IDrawingReader : IDisposable
{
    IReadOnlyList<BlockRecordInfo> ReadBlockRecords();

    StoredBufferScan ReadStoredBuffers();
}

/// <summary>Opens a PRIVATE COPY of a file into an in-memory side database (R0). The adapter lives in r0/ (the only AutoCAD-touching piece).</summary>
public interface IDrawingSource
{
    IDrawingReader OpenPrivateCopy(string privateCopyPath);
}

/// <summary>The value of a system variable as the host returned it, or the error text.</summary>
public sealed record SysVarReading(string Name, string? HostTypeFullName, string? ValueText, string? ValueBitsHex, string? Error);

/// <summary>Reads system variables of the active document (I-4, HF-G3; also DBMOD before/after). Read only.</summary>
public interface ISystemVariables
{
    SysVarReading Read(string name);
}

/// <summary>Process and clock facts. Volatile values only; they never enter a hashed part.</summary>
public interface IRunEnvironment
{
    DateTime UtcNow { get; }

    int ProcessId { get; }

    string ProcessStartUtc { get; }
}
