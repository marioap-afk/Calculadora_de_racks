using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using I52Ct21d.HostFacts.Core;

namespace I52Ct21d.HostFacts.Rs.Core;

public enum ScratchGuardError
{
    RootInvalid,
    NameInvalid,
    AlreadyExists,
    AlreadyAuthorized,
    TooManyFiles,
    ForeignTarget,
    AlreadySaved,
    ReparsePoint,
    NotSaved,
    SourceNotAuthorized,
    HashMismatch,
}

public sealed class ScratchGuardException : Exception
{
    public ScratchGuardError Error { get; }

    public ScratchGuardException(ScratchGuardError error, string message) : base(error + ": " + message)
    {
        Error = error;
    }
}

/// <summary>One scratch file the run declared: flat name, byte length, SHA-256 recorded right after the save, and its role.</summary>
public sealed class ScratchFileEntry
{
    public string Name { get; }
    public long Length { get; }
    public string Sha256 { get; }
    public string Role { get; }

    public ScratchFileEntry(string name, long length, string sha256, string role)
    {
        Name = name;
        Length = length;
        Sha256 = sha256;
        Role = role;
    }

    public JsonObject ToJson() => new() { ["name"] = Name, ["length"] = Length, ["sha256"] = Sha256, ["role"] = Role };
}

/// <summary>
/// A NEW scratch path that the guard authorized: a flat file name under the scratch root, absent at authorization time. Only
/// <see cref="ScratchRootGuard.AuthorizeNew"/> creates one (the constructor is internal), and the only type of the RS DLL that may pass it to
/// <c>Database.SaveAs</c> is the <c>ScratchSaver</c> (the scan pins that).
/// </summary>
public sealed class ScratchTarget
{
    internal ScratchRootGuard Owner { get; }
    public string Name { get; }
    public string FullPath { get; }
    public string Role { get; }
    internal bool Saved { get; set; }

    internal ScratchTarget(ScratchRootGuard owner, string name, string fullPath, string role)
    {
        Owner = owner;
        Name = name;
        FullPath = fullPath;
        Role = role;
    }
}

public enum ReadableKind
{
    PrivateCopy,
    SavedScratch,
}

/// <summary>
/// A file the guard allows <c>ReadDwgFile</c> to open: the designated private copy of the library, or a scratch file that the guard's
/// ledger declares (the OP2 reopen). Only <see cref="ScratchRootGuard"/> creates one.
/// </summary>
public sealed class ReadableSource
{
    internal ScratchRootGuard Owner { get; }
    public ReadableKind Kind { get; }
    public string FullPath { get; }
    public string? ExpectedSha256 { get; }

    internal ReadableSource(ScratchRootGuard owner, ReadableKind kind, string fullPath, string? expectedSha256)
    {
        Owner = owner;
        Kind = kind;
        FullPath = fullPath;
        ExpectedSha256 = expectedSha256;
    }
}

/// <summary>
/// THE scratch-root validator (Owner Q-O-1: new files only, under one fixed allowlisted root, resolved and validated paths that do not escape).
/// Flat names only (the design allows no sub-path), a fixed name pattern, create-new semantics (a target that exists, in any letter case, is
/// refused), no reparse point in the root or its ancestors, a bounded number of files, and a ledger with the SHA-256 recorded after each save.
/// It performs NO write: the save itself is the <c>ScratchSaver</c>'s single <c>SaveAs</c>. It re-checks at save time
/// (<see cref="AssertCreatable"/>) and before every read (<see cref="AssertReadable"/>). A check-then-save race with another process is a
/// residual limit (README).
/// </summary>
public sealed class ScratchRootGuard
{
    public const int MaxFiles = 8;

    private static readonly Regex NamePattern = new(@"^CT21D_[A-Za-z0-9][A-Za-z0-9_-]{0,80}\.dwg\z", RegexOptions.CultureInvariant);

    private readonly List<ScratchFileEntry> _ledger = new();
    private readonly HashSet<string> _authorized = new(StringComparer.OrdinalIgnoreCase);
    private readonly string _privateCopyPath;

    public string Root { get; }

    public IReadOnlyList<ScratchFileEntry> Ledger => _ledger;

    public ScratchRootGuard(string scratchRoot, string privateCopyPath)
    {
        var why = PathRegions.TextProblem(scratchRoot);
        if (why.Length > 0) throw new ScratchGuardException(ScratchGuardError.RootInvalid, "the scratch root is not an acceptable absolute path (" + why + ")");
        var full = PathRegions.Norm(scratchRoot);
        if (PathRegions.IsDriveRoot(scratchRoot))
            throw new ScratchGuardException(ScratchGuardError.RootInvalid, "a drive root cannot be the scratch root (use a dedicated directory)");
        if (!Directory.Exists(full))
            throw new ScratchGuardException(ScratchGuardError.RootInvalid, "the scratch root must already exist (the guard creates no directory)");
        var reparse = PathRegions.FirstReparsePoint(full);
        if (reparse.Length > 0)
            throw new ScratchGuardException(ScratchGuardError.ReparsePoint, "the scratch root or an ancestor is a reparse point: " + reparse);
        Root = full;
        _privateCopyPath = PathRegions.Norm(privateCopyPath);
    }

    /// <summary>Validates a scratch file name (flat, <c>CT21D_</c> prefix, <c>.dwg</c>, no reserved device name, no dot sequence).</summary>
    public static void ValidateName(string name)
    {
        if (name is null || !NamePattern.IsMatch(name))
            throw new ScratchGuardException(ScratchGuardError.NameInvalid, "not a scratch file name (CT21D_<id>.dwg, flat): " + name);
        if (name.Contains("..", StringComparison.Ordinal))
            throw new ScratchGuardException(ScratchGuardError.NameInvalid, "dot sequences are not allowed: " + name);
        EvidenceWriter.ValidateName(name); // the reserved device names and the flat-name rules of the evidence writer apply too
    }

    private string Combine(string name)
    {
        var path = Path.Combine(Root, name);
        // defense in depth: the combined path must be a direct child of the root
        if (!string.Equals(Path.GetDirectoryName(Path.GetFullPath(path)), Root, StringComparison.OrdinalIgnoreCase))
            throw new ScratchGuardException(ScratchGuardError.NameInvalid, "path escapes the scratch root: " + name);
        return path;
    }

    private bool ExistsAnyCase(string name)
    {
        foreach (var entry in Directory.EnumerateFileSystemEntries(Root))
            if (string.Equals(Path.GetFileName(entry), name, StringComparison.OrdinalIgnoreCase)) return true;
        return false;
    }

    /// <summary>Authorizes a NEW file. Throws when the name is invalid, was authorized before, exists (any letter case) or the limit is reached.</summary>
    public ScratchTarget AuthorizeNew(string name, string role)
    {
        ValidateName(name);
        if (_authorized.Contains(name)) throw new ScratchGuardException(ScratchGuardError.AlreadyAuthorized, name);
        if (_authorized.Count >= MaxFiles) throw new ScratchGuardException(ScratchGuardError.TooManyFiles, "at most " + MaxFiles + " scratch files per run");
        var path = Combine(name);
        if (ExistsAnyCase(name)) throw new ScratchGuardException(ScratchGuardError.AlreadyExists, name);
        _authorized.Add(name);
        return new ScratchTarget(this, name, path, role);
    }

    /// <summary>The check <c>ScratchSaver</c> makes right before <c>SaveAs</c>: the target is this guard's, unsaved, still absent, the root still clean.</summary>
    public void AssertCreatable(ScratchTarget target)
    {
        if (!ReferenceEquals(target.Owner, this)) throw new ScratchGuardException(ScratchGuardError.ForeignTarget, target.Name);
        if (target.Saved) throw new ScratchGuardException(ScratchGuardError.AlreadySaved, target.Name);
        ValidateName(target.Name);
        if (!string.Equals(Combine(target.Name), target.FullPath, StringComparison.OrdinalIgnoreCase))
            throw new ScratchGuardException(ScratchGuardError.ForeignTarget, "the target path is not the resolved path of its name: " + target.Name);
        var reparse = PathRegions.FirstReparsePoint(Root);
        if (reparse.Length > 0) throw new ScratchGuardException(ScratchGuardError.ReparsePoint, reparse);
        if (ExistsAnyCase(target.Name)) throw new ScratchGuardException(ScratchGuardError.AlreadyExists, target.Name);
    }

    /// <summary>Right after the save: the file must exist as a regular file; records its length and SHA-256 in the ledger.</summary>
    public ScratchFileEntry RecordSaved(ScratchTarget target)
    {
        if (!ReferenceEquals(target.Owner, this)) throw new ScratchGuardException(ScratchGuardError.ForeignTarget, target.Name);
        if (target.Saved) throw new ScratchGuardException(ScratchGuardError.AlreadySaved, target.Name);
        if (!File.Exists(target.FullPath)) throw new ScratchGuardException(ScratchGuardError.NotSaved, target.Name);
        if ((File.GetAttributes(target.FullPath) & FileAttributes.ReparsePoint) != 0) throw new ScratchGuardException(ScratchGuardError.ReparsePoint, target.Name);
        var info = new FileInfo(target.FullPath);
        var entry = new ScratchFileEntry(target.Name, info.Length, Sha256Hex.OfFile(target.FullPath), target.Role);
        target.Saved = true;
        _ledger.Add(entry);
        return entry;
    }

    /// <summary>The designated private copy of the library, for <c>ReadDwgFile</c> (never the library itself).</summary>
    public ReadableSource AuthorizePrivateCopy(string expectedSha256)
    {
        if (!File.Exists(_privateCopyPath)) throw new ScratchGuardException(ScratchGuardError.SourceNotAuthorized, "the private copy does not exist");
        if ((File.GetAttributes(_privateCopyPath) & FileAttributes.ReparsePoint) != 0)
            throw new ScratchGuardException(ScratchGuardError.ReparsePoint, "the private copy is a reparse point (it must be a regular file)");
        return new ReadableSource(this, ReadableKind.PrivateCopy, _privateCopyPath, expectedSha256);
    }

    /// <summary>A scratch file of this guard's ledger, for the OP2 reopen.</summary>
    public ReadableSource AuthorizeReadBack(ScratchFileEntry entry)
    {
        var declared = _ledger.FirstOrDefault(e => ReferenceEquals(e, entry));
        if (declared is null) throw new ScratchGuardException(ScratchGuardError.SourceNotAuthorized, "not a file of this guard's ledger: " + entry.Name);
        return new ReadableSource(this, ReadableKind.SavedScratch, Combine(entry.Name), entry.Sha256);
    }

    /// <summary>The check <c>SideDbWriter</c> makes right before <c>ReadDwgFile</c>: the source is this guard's and the file still has its recorded hash.</summary>
    public void AssertReadable(ReadableSource source)
    {
        if (!ReferenceEquals(source.Owner, this)) throw new ScratchGuardException(ScratchGuardError.SourceNotAuthorized, "foreign source");
        if (!File.Exists(source.FullPath)) throw new ScratchGuardException(ScratchGuardError.SourceNotAuthorized, "the file does not exist");
        if ((File.GetAttributes(source.FullPath) & FileAttributes.ReparsePoint) != 0)
            throw new ScratchGuardException(ScratchGuardError.ReparsePoint, "the source is a reparse point: " + Path.GetFileName(source.FullPath));
        if (source.Kind == ReadableKind.PrivateCopy)
        {
            if (!PathRegions.Same(source.FullPath, _privateCopyPath))
                throw new ScratchGuardException(ScratchGuardError.SourceNotAuthorized, "not the designated private copy");
        }
        else
        {
            if (!PathRegions.IsInside(source.FullPath, Root) || !string.Equals(Path.GetDirectoryName(source.FullPath), Root, StringComparison.OrdinalIgnoreCase))
                throw new ScratchGuardException(ScratchGuardError.SourceNotAuthorized, "a read-back source must be a flat file of the scratch root");
            if (_ledger.All(e => !string.Equals(e.Name, Path.GetFileName(source.FullPath), StringComparison.OrdinalIgnoreCase)))
                throw new ScratchGuardException(ScratchGuardError.SourceNotAuthorized, "not declared in the ledger");
        }
        if (source.ExpectedSha256 is null || !string.Equals(Sha256Hex.OfFile(source.FullPath), source.ExpectedSha256, StringComparison.Ordinal))
            throw new ScratchGuardException(ScratchGuardError.HashMismatch, Path.GetFileName(source.FullPath));
    }
}
