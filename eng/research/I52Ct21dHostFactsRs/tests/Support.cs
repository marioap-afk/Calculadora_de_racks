using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using I52Ct21d.HostFacts.Core;
using I52Ct21d.HostFacts.Rs.Core;

namespace I52Ct21d.HostFacts.Rs.Tests;

/// <summary>Helpers shared by the tests. Tests may write temporary files; the instruments (core-rs, rs, tools-rs) may not (the scan proves it).</summary>
internal static class Support
{
    public static string RepoRoot()
    {
        var d = new DirectoryInfo(AppContext.BaseDirectory);
        while (d is not null && !File.Exists(Path.Combine(d.FullName, "global.json"))) d = d.Parent;
        return d?.FullName ?? throw new InvalidOperationException("repository root (global.json) not found above " + AppContext.BaseDirectory);
    }

    public static string RsFolder() => Path.Combine(RepoRoot(), "eng", "research", "I52Ct21dHostFactsRs");

    public static string R0Folder() => Path.Combine(RepoRoot(), "eng", "research", "I52Ct21dHostFacts");

    /// <summary>SHA-256 computed here, independently of <see cref="Sha256Hex"/>.</summary>
    public static string Sha(byte[] b) => Convert.ToHexString(SHA256.HashData(b)).ToLowerInvariant();

    public static string Sha(string path) => Sha(File.ReadAllBytes(path));

    public static string Z64 => new('0', 64);
}

internal sealed class TempDir : IDisposable
{
    public string Path { get; }

    public TempDir()
    {
        Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "i52ct21d-rs-tests-" + Guid.NewGuid().ToString("N"));
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

    /// <summary>Called on every read; a test uses it to change a variable DURING a run.</summary>
    public Action<string, FakeVars>? OnRead { get; set; }

    public static FakeVars Default()
    {
        var v = new FakeVars();
        v.Set("DBMOD", "System.Int16", "0");
        v.Set("SECURELOAD", "System.Int16", "1");
        v.Set("TRUSTEDPATHS", "System.String", "C:\\trusted");
        v.Set("CPROFILE", "System.String", "<<Unnamed Profile>>");
        v.Set("DWGPREFIX", "System.String", "C:\\drawings\\");
        v.Set("DWGNAME", "System.String", "Drawing1.dwg");
        return v;
    }

    public FakeVars Set(string name, string type, string text)
    {
        Values[name] = new SysVarReading(name, type, text, null, null);
        return this;
    }

    public SysVarReading Read(string name)
    {
        OnRead?.Invoke(name, this);
        return Values.TryGetValue(name, out var r) ? r : new SysVarReading(name, null, null, null, "NOT_FOUND");
    }
}

/// <summary>A complete on-disk setup for one RS run: evidence root, scratch root, library, private copy, product folder, designation.</summary>
internal sealed class Rig : IDisposable
{
    public const string TolRunId = "HGP-H4-20260930T120000Z-01";
    public const string OtherRunId = "HGP-H3-20260930T120000Z-01";

    public TempDir Temp { get; } = new();
    public string Evidence { get; }
    public string Scratch { get; }
    public string Library { get; }
    public string PrivateCopy { get; }
    public string Product { get; }
    public byte[] LibraryBytes { get; } = Encoding.ASCII.GetBytes("AC1032-fake-library-drawing-bytes");
    public RsDesignation D { get; private set; }

    public static InstrumentInfo Match => new("I52Ct21d.HostFacts.Rs", new string('a', 64), InstrumentInfo.PinMatch);

    public Rig(string runId = TolRunId, bool scripted = true, bool otherAcad = false, string governed = "")
    {
        Evidence = Temp.Sub("evidence");
        Scratch = Temp.Sub("scratch");
        Product = Temp.Sub("product");
        var lib = Temp.Sub("library");
        var priv = Temp.Sub("private");
        Library = Path.Combine(lib, "library.dwg");
        PrivateCopy = Path.Combine(priv, "library-copy.dwg");
        File.WriteAllBytes(Library, LibraryBytes);
        File.WriteAllBytes(PrivateCopy, LibraryBytes);
        D = Build(runId, scripted, otherAcad, governed);
    }

    private RsDesignation Build(string runId, bool scripted, bool otherAcad, string governed)
    {
        var sha = Support.Sha(LibraryBytes);
        return new RsDesignation(
            runId, 1, "session-1", Evidence, Scratch, PrivateCopy, sha, Library, sha, governed, new[] { Product },
            new HostChecksDeclaration(otherAcad, scripted), "MC-0123456789ab", new string('1', 64), new string('2', 64), new string('3', 64),
            new string('4', 40), "b74af94ece4f0901a071ef5176f3c58bff01cfdc", new[] { new DeclaredFile("I52Ct21d.HostFacts.Rs.dll", new string('5', 64)) });
    }

    public RsDesignation With(Func<RsDesignation, RsDesignation> change)
    {
        D = change(D);
        return D;
    }

    /// <summary>The designation as the JSON file the CAD manager would place next to the DLL.</summary>
    public string DesignationJson() => DesignationJson(D);

    public static string DesignationJson(RsDesignation d) => new JsonObject
    {
        ["schema"] = "ct21d.designation.rs.v1",
        ["runId"] = d.RunId,
        ["attempt"] = d.Attempt,
        ["sessionId"] = d.SessionId,
        ["evidenceRoot"] = d.EvidenceRoot,
        ["scratchRoot"] = d.ScratchRoot,
        ["privateCopyPath"] = d.PrivateCopyPath,
        ["privateCopySha256"] = d.PrivateCopySha256,
        ["libraryPath"] = d.LibraryPath,
        ["libraryFileSha256"] = d.LibraryFileSha256,
        ["governedDocumentPath"] = d.GovernedDocumentPath,
        ["productPaths"] = RecordJson.Strings(d.ProductPaths),
        ["hostChecks"] = new JsonObject { ["otherAcadProcess"] = d.HostChecks.OtherAcadProcess, ["loadRouteScripted"] = d.HostChecks.LoadRouteScripted },
        ["declaredSet"] = RecordJson.Arr(d.DeclaredSet.Select(f => (JsonNode?)new JsonObject { ["path"] = f.Path, ["sha256"] = f.Sha256 })),
        ["tupleBinding"] = new JsonObject
        {
            ["machineClassLabel"] = d.MachineClassLabel, ["buildTupleDigest"] = d.BuildTupleDigest, ["packageManifestSha256"] = d.PackageManifestSha256,
            ["declaredSetSha256"] = d.DeclaredSetSha256, ["designBlob"] = d.DesignBlob, ["ba05Blob"] = d.Ba05Blob,
        },
    }.ToJsonString();

    public EvidenceWriter Writer() => new(Evidence);

    public IReadOnlyList<string> EvidenceFiles() => Directory.EnumerateFiles(Evidence).Select(p => Path.GetFileName(p)!).OrderBy(n => n, StringComparer.Ordinal).ToList();

    public IReadOnlyList<string> ScratchEntries() => Directory.EnumerateFileSystemEntries(Scratch).Select(p => Path.GetFileName(p)!).OrderBy(n => n, StringComparer.Ordinal).ToList();

    public JsonNode ReadEvidence(string name) => Jcs.ParseStrict(File.ReadAllText(Path.Combine(Evidence, name)));

    public void Dispose() => Temp.Dispose();
}

// ---------------------------------------------------------------------------------------------------------------------------------------
// Fake TOL_SCALE host

internal sealed class FakeTolHost : ITolScaleHost
{
    public ScratchRootGuard Guard { get; }
    public SideDbLedger Ledger { get; }

    /// <summary>What the host reads back for a row at OP1 (point 1) or OP2 (point 2). Default: the intended bits.</summary>
    public Func<ProbeRow, int, IReadOnlyList<string>> Readback { get; set; } = (r, _) => r.Intended;

    /// <summary>A host rejection of a row's construction (the error text), or null.</summary>
    public Func<ProbeRow, string?> Reject { get; set; } = _ => null;

    public Func<ProbeRow, int, string> Rotation { get; set; } = (_, _) => DoubleBits.Hex(0.0);

    public Func<ProbeRow, int, IReadOnlyList<string>> Normal { get; set; } = (_, _) => new[] { DoubleBits.Hex(0.0), DoubleBits.Hex(0.0), DoubleBits.Hex(1.0) };

    /// <summary>Column lengths of BlockTransform; default is |scale| computed exactly.</summary>
    public Func<ProbeRow, int, IReadOnlyList<string>, IReadOnlyList<string>> Columns { get; set; } =
        (_, _, scale) => scale.Select(h => DoubleBits.Hex(Math.Abs(DoubleBits.FromHex(h)))).ToList();

    public bool ThrowOnRead { get; set; }
    public bool ThrowOnSave { get; set; }
    public bool LeaveUndeclaredFile { get; set; }
    public bool DoNotDispose { get; set; }
    public Action? OnSaved { get; set; }
    public Action? OnCreate { get; set; }
    public int Creates { get; private set; }
    public List<string> Calls { get; } = new();

    public FakeTolHost(ScratchRootGuard guard, SideDbLedger ledger)
    {
        Guard = guard;
        Ledger = ledger;
    }

    public ITolScaleDatabase CreateSideDatabase()
    {
        Creates++;
        OnCreate?.Invoke();
        Calls.Add("create");
        return new Db(this, Ledger.Register("SCRATCH_WRITE", "new Database(true, true)", "NONE", "TOLSCALE_PROBE"), 1);
    }

    private sealed class Db : ITolScaleDatabase
    {
        private readonly FakeTolHost _host;
        private readonly int _point;
        private List<(ProbeRow Row, string? Handle)> _rows = new();

        public Db(FakeTolHost host, string id, int point)
        {
            _host = host;
            Id = id;
            _point = point;
        }

        public string Id { get; }

        public IReadOnlyList<RowConstruction> AppendReferences(IReadOnlyList<ProbeRow> rows)
        {
            _host.Calls.Add("append");
            var result = new List<RowConstruction>();
            _rows = new List<(ProbeRow, string?)>();
            for (var i = 0; i < rows.Count; i++)
            {
                var rejection = _host.Reject(rows[i]);
                var handle = rejection is null ? (0x100 + i).ToString("X") : null;
                _rows.Add((rows[i], handle));
                result.Add(new RowConstruction(rows[i].Id, handle, rejection));
            }
            return result;
        }

        public IReadOnlyList<ReferenceReading> ReadReferences()
        {
            _host.Calls.Add("read" + _point);
            if (_host.ThrowOnRead && _point == 2) throw new InvalidOperationException("read failed at OP2");
            var list = new List<ReferenceReading>();
            foreach (var (row, handle) in _rows)
            {
                if (handle is null) continue;
                var scale = _host.Readback(row, _point);
                list.Add(new ReferenceReading(handle, scale, _host.Rotation(row, _point), _host.Normal(row, _point), _host.Columns(row, _point, scale), null));
            }
            return list;
        }

        public ScratchFileEntry SaveToScratch(ScratchTarget target)
        {
            _host.Calls.Add("save");
            if (_host.ThrowOnSave) throw new IOException("save failed");
            _host.Guard.AssertCreatable(target);
            File.WriteAllBytes(target.FullPath, Encoding.ASCII.GetBytes("AC1032-fake-saved-side-database:" + target.Name));
            if (_host.LeaveUndeclaredFile) File.WriteAllText(Path.Combine(_host.Guard.Root, "leftover.bak"), "x");
            _host.OnSaved?.Invoke();
            return _host.Guard.RecordSaved(target);
        }

        public ITolScaleDatabase Reopen(ReadableSource source)
        {
            _host.Calls.Add("reopen");
            _host.Guard.AssertReadable(source);
            return new Db(_host, _host.Ledger.Register("SAVED_REOPEN_READ", "new Database(false, true)", "ReadDwgFile", Path.GetFileName(source.FullPath)), 2) { _rows = _rows };
        }

        public void Dispose()
        {
            if (_host.DoNotDispose) return;
            _host.Ledger.MarkDisposed(Id);
        }
    }
}

internal static class TolHelpers
{
    /// <summary>Rounds each component to a multiple of 2^-bits (what a host that stores a fixed-point scale would do).</summary>
    public static string RoundTo(string hex, int bits)
    {
        var d = DoubleBits.FromHex(hex);
        var q = Math.Pow(2.0, -bits);
        return DoubleBits.Hex(Math.Round(d / q) * q);
    }
}
