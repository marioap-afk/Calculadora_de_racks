using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace I52Ct21d.HostFacts.Core;

public enum EvidenceWriteError
{
    InvalidName,
    RootInvalid,
    AlreadyExists,
    AlreadyWrittenByThisWriter,
    HashMismatchAfterWrite,
    NotWrittenByThisWriter,
    InvalidText,
    IoFailure,
}

public sealed class EvidenceWriterException : Exception
{
    public EvidenceWriteError Error { get; }

    public EvidenceWriterException(EvidenceWriteError error, string message, Exception? inner = null) : base(error + ": " + message, inner)
    {
        Error = error;
    }
}

/// <summary>One file written by the <see cref="EvidenceWriter"/>: name (flat), byte length and SHA-256 of the bytes written.</summary>
public sealed record WrittenFile(string Name, long Length, string Sha256);

/// <summary>
/// THE single class through which every instrument and every tool of this package writes a file (design 5.3 item 3 rule (c):
/// "File and stream write call sites may appear only inside EvidenceWriter"; the forbidden-API scan enforces it).
///
/// Semantics (Q-O-0(b): new result files under the evidence folder, nothing else):
///  - CREATE-NEW only (<see cref="FileMode.CreateNew"/>, <see cref="FileShare.None"/>): an existing file is never overwritten,
///    truncated or appended to; there is no append API and no delete API;
///  - FLAT names only inside one root folder that must already exist: no path separators, no drive or stream marker, no "..", no
///    reserved device names; the writer creates no directory;
///  - write-once: a name can be written at most once by an instance (the file system refuses it again anyway);
///  - the SHA-256 of every written file is computed from the bytes AND re-verified by reading the file back; the ledger records
///    name, length and hash;
///  - text is UTF-8 without BOM and LF only.
/// </summary>
public sealed class EvidenceWriter
{
    private static readonly Regex NamePattern = new(@"^[A-Za-z0-9][A-Za-z0-9._-]{0,119}\z", RegexOptions.CultureInvariant);

    private static readonly HashSet<string> ReservedDevices = new(StringComparer.OrdinalIgnoreCase)
    {
        "CON", "PRN", "AUX", "NUL", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
        "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9",
    };

    private readonly List<WrittenFile> _ledger = new();
    private readonly HashSet<string> _names = new(StringComparer.OrdinalIgnoreCase);

    public string Root { get; }

    public IReadOnlyList<WrittenFile> Ledger => _ledger;

    public EvidenceWriter(string rootDirectory)
    {
        if (string.IsNullOrWhiteSpace(rootDirectory) || !Path.IsPathFullyQualified(rootDirectory))
            throw new EvidenceWriterException(EvidenceWriteError.RootInvalid, "the evidence folder must be an absolute path");
        var full = Path.GetFullPath(rootDirectory);
        if (!Directory.Exists(full))
            throw new EvidenceWriterException(EvidenceWriteError.RootInvalid, "the evidence folder must already exist (the writer creates no directory)");
        if ((File.GetAttributes(full) & FileAttributes.ReparsePoint) != 0)
            throw new EvidenceWriterException(EvidenceWriteError.RootInvalid, "the evidence folder must not be a reparse point");
        Root = full;
    }

    /// <summary>Validates a flat file name. Throws <see cref="EvidenceWriterException"/> with <see cref="EvidenceWriteError.InvalidName"/>.</summary>
    public static void ValidateName(string name)
    {
        if (name is null || !NamePattern.IsMatch(name))
            throw new EvidenceWriterException(EvidenceWriteError.InvalidName, "not a flat evidence file name: " + name);
        if (name.EndsWith('.') || name.Contains("..", StringComparison.Ordinal))
            throw new EvidenceWriterException(EvidenceWriteError.InvalidName, "dot sequences are not allowed: " + name);
        var stem = name.Split('.')[0];
        if (ReservedDevices.Contains(stem))
            throw new EvidenceWriterException(EvidenceWriteError.InvalidName, "reserved device name: " + name);
    }

    /// <summary>Writes <paramref name="content"/> as a NEW file and records its SHA-256.</summary>
    public WrittenFile WriteNew(string name, byte[] content)
    {
        ValidateName(name);
        if (_names.Contains(name))
            throw new EvidenceWriterException(EvidenceWriteError.AlreadyWrittenByThisWriter, name);
        var path = Path.Combine(Root, name);
        // defense in depth: the combined path must be a direct child of the root
        if (!string.Equals(Path.GetDirectoryName(Path.GetFullPath(path)), Root.TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase))
            throw new EvidenceWriterException(EvidenceWriteError.InvalidName, "path escapes the evidence folder: " + name);
        // a case-insensitive twin (Windows, or a case-sensitive file system) counts as existing: nothing is replaced
        foreach (var existing in Directory.EnumerateFileSystemEntries(Root))
            if (string.Equals(Path.GetFileName(existing), name, StringComparison.OrdinalIgnoreCase))
                throw new EvidenceWriterException(EvidenceWriteError.AlreadyExists, name);

        var expected = Convert.ToHexString(SHA256.HashData(content)).ToLowerInvariant();
        try
        {
            using var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
            stream.Write(content, 0, content.Length);
            stream.Flush(true);
        }
        catch (IOException ex) when (File.Exists(path))
        {
            throw new EvidenceWriterException(EvidenceWriteError.AlreadyExists, name, ex);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new EvidenceWriterException(EvidenceWriteError.IoFailure, name, ex);
        }

        var actual = Sha256Hex.OfFile(path);
        if (!string.Equals(actual, expected, StringComparison.Ordinal))
            throw new EvidenceWriterException(EvidenceWriteError.HashMismatchAfterWrite, name);
        var written = new WrittenFile(name, content.LongLength, actual);
        _names.Add(name);
        _ledger.Add(written);
        return written;
    }

    /// <summary>Writes text as UTF-8 without BOM. A carriage return is refused (LF only).</summary>
    public WrittenFile WriteNewText(string name, string text)
    {
        if (text.Contains('\r'))
            throw new EvidenceWriterException(EvidenceWriteError.InvalidText, "text files are LF only: " + name);
        return WriteNew(name, new UTF8Encoding(false, true).GetBytes(text));
    }

    /// <summary>
    /// Marks an existing file DIRECTLY under the evidence folder as read-only (used by the seal, I-9). It changes one attribute
    /// (read-only) of a flat-named file of the evidence folder and nothing else.
    /// </summary>
    public void MarkReadOnly(string name)
    {
        ValidateName(name);
        var path = Path.Combine(Root, name);
        if (!File.Exists(path))
            throw new EvidenceWriterException(EvidenceWriteError.NotWrittenByThisWriter, "no such file in the evidence folder: " + name);
        File.SetAttributes(path, File.GetAttributes(path) | FileAttributes.ReadOnly);
    }
}
