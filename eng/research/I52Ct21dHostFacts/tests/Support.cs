using System.Security.Cryptography;
using System.Text.Json.Nodes;
using I52Ct21d.HostFacts.Core;

namespace I52Ct21d.HostFacts.Tests;

/// <summary>Helpers shared by the tests. Tests may write temporary files; the instruments (core, r0, tools) may not (the scan proves it).</summary>
internal static class Support
{
    public static string RepoRoot()
    {
        var d = new DirectoryInfo(AppContext.BaseDirectory);
        while (d is not null && !File.Exists(Path.Combine(d.FullName, "global.json"))) d = d.Parent;
        return d?.FullName ?? throw new InvalidOperationException("repository root (global.json) not found above " + AppContext.BaseDirectory);
    }

    public static string DataFile(string name) => Path.Combine(AppContext.BaseDirectory, "data", name);

    public static JsonNode Data(string name) => JsonNode.Parse(File.ReadAllText(DataFile(name)))!;

    /// <summary>SHA-256 computed here, independently of <see cref="Sha256Hex"/>.</summary>
    public static string Sha(byte[] b) => Convert.ToHexString(SHA256.HashData(b)).ToLowerInvariant();

    public static string Sha(string path) => Sha(File.ReadAllBytes(path));
}

internal sealed class TempDir : IDisposable
{
    public string Path { get; }

    public TempDir()
    {
        Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "i52ct21d-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path);
    }

    public string Sub(string name)
    {
        var p = System.IO.Path.Combine(Path, name);
        Directory.CreateDirectory(p);
        return p;
    }

    public void Dispose()
    {
        try
        {
            foreach (var f in Directory.EnumerateFiles(Path, "*", SearchOption.AllDirectories))
                File.SetAttributes(f, FileAttributes.Normal);
            Directory.Delete(Path, true);
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }
}

internal sealed class FakeEnv : IRunEnvironment
{
    public DateTime UtcNow { get; set; } = new(2026, 9, 30, 12, 0, 0, DateTimeKind.Utc);

    public int ProcessId { get; set; } = 4242;

    public string ProcessStartUtc { get; set; } = "2026-09-30T11:00:00.000Z";
}

internal sealed class FakeVars : ISystemVariables
{
    public Dictionary<string, SysVarReading> Values { get; } = new(StringComparer.Ordinal);

    /// <summary>When set, each read of DBMOD returns the next value (to simulate a change during the run).</summary>
    public Queue<string>? DbmodSequence { get; set; }

    public static FakeVars Default()
    {
        var v = new FakeVars();
        v.Set("DBMOD", "System.Int16", "0");
        v.Set("DWGPREFIX", "System.String", "C:\\drawings\\");
        v.Set("DWGNAME", "System.String", "Drawing1.dwg");
        foreach (var c in ContextVariableTable.Variables) v.Set(c.Name, c.ExpectedHostType, c.ExpectedHostType == "System.String" ? "x" : "1");
        return v;
    }

    public FakeVars Set(string name, string type, string text, string? bits = null)
    {
        Values[name] = new SysVarReading(name, type, text, bits, null);
        return this;
    }

    public SysVarReading Read(string name)
    {
        if (name == "DBMOD" && DbmodSequence is { Count: > 0 })
            return new SysVarReading(name, "System.Int16", DbmodSequence.Dequeue(), null, null);
        return Values.TryGetValue(name, out var r) ? r : new SysVarReading(name, null, null, null, "NOT_FOUND");
    }
}

internal sealed class FakeReader : IDrawingReader
{
    private readonly IReadOnlyList<BlockRecordInfo> _blocks;
    private readonly StoredBufferScan _stored;
    public bool Disposed { get; private set; }
    public Action? OnRead { get; init; }

    public FakeReader(IReadOnlyList<BlockRecordInfo> blocks, StoredBufferScan stored)
    {
        _blocks = blocks;
        _stored = stored;
    }

    public IReadOnlyList<BlockRecordInfo> ReadBlockRecords() { OnRead?.Invoke(); return _blocks; }

    public StoredBufferScan ReadStoredBuffers() => _stored;

    public void Dispose() => Disposed = true;
}

internal sealed class FakeSource : IDrawingSource
{
    public FakeReader? Last { get; private set; }
    public string? OpenedPath { get; private set; }
    public IReadOnlyList<BlockRecordInfo> Blocks { get; init; } = Array.Empty<BlockRecordInfo>();
    public StoredBufferScan Stored { get; init; } = new(Array.Empty<StoredEntry>(), 0, 0);
    public Action? OnRead { get; init; }

    public IDrawingReader OpenPrivateCopy(string privateCopyPath)
    {
        OpenedPath = privateCopyPath;
        return Last = new FakeReader(Blocks, Stored) { OnRead = OnRead };
    }
}

internal static class Samples
{
    public static EntityInfo Entity(string rx, string dxf, string? refBlock = null, double[]? scale = null, DstyleInfo? dstyle = null,
        string layer = "0", bool proxy = false, bool anonStatic = false, string? error = null, string? textStyle = null, string? dimStyle = null) =>
        new(rx, dxf, proxy, layer, "Continuous", textStyle, dimStyle, refBlock, anonStatic, scale, dstyle, error);

    public static BlockRecordInfo Block(string name, IEnumerable<EntityInfo> entities, bool layout = false, bool anon = false, bool dyn = false, string? error = null) =>
        new(name, layout, anon, dyn, entities.ToList(), error);

    /// <summary>A small library: a layout, two named blocks (one with a reference, a dimension, an unknown class), an anonymous block.</summary>
    public static IReadOnlyList<BlockRecordInfo> Library() => new[]
    {
        Block("*Model_Space", Array.Empty<EntityInfo>(), layout: true),
        Block("*Paper_Space", new[] { Entity("AcDbViewport", "VIEWPORT") }, layout: true),
        Block("RACK_A", new[]
        {
            Entity("AcDbLine", "LINE"), Entity("AcDbLine", "LINE"), Entity("AcDbLine", "LINE"),
            Entity("AcDbBlockReference", "INSERT", "PIECE", new[] { 1.0, 1.0, -1.0 }),
            Entity("AcDbBlockReference", "INSERT", "PIECE", new[] { 1.0000000001, 1.0, 1.0 }),
            Entity("AcDbRotatedDimension", "DIMENSION",
                dstyle: new DstyleInfo(true, true, new[] { new DstylePair(40, "System.Double"), new DstylePair(41, "System.Double"), new DstylePair(77, "System.Int16") }),
                dimStyle: "STD"),
            Entity("AcDbText", "TEXT", textStyle: "Standard", layer: "TEXT"),
            Entity("AcDbSolid", "SOLID"),
        }),
        Block("PIECE", new[] { Entity("AcDbLine", "LINE"), Entity("AcDbArc", "ARC") }),
        Block("*U12", new[] { Entity("AcDbBlockReference", "INSERT", "*U7", new[] { 1.0, 1.0, 1.0 }, anonStatic: true) }, anon: true),
    };
}
