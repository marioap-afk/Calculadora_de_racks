using System.Text.Json.Nodes;

namespace I52Ct21d.HostFacts.Core;

/// <summary>One file of the declared set (instrument DLL or dependency): a path relative to the instrument folder and its SHA-256.</summary>
public sealed record DeclaredFile(string Path, string Sha256);

/// <summary>
/// The declared inputs of one run (schema <c>ct21d.designation.v1</c>): the file the CAD manager places next to the instrument DLL
/// (<c>run-designation.json</c>). The format is this folder's own design (flagged for package v2 and the Architect). It carries the tuple bindings that cannot be computed inside the process (the digest of the build
/// tuple, the manifest and declared-set hashes, the label) and the paths. It is an INPUT: an instrument never writes it.
/// </summary>
public sealed record RunDesignation(
    string RunId,
    int Attempt,
    string SessionId,
    string EvidenceFolder,
    string PrivateCopyPath,
    string PrivateCopySha256,
    string LibraryPath,
    string LibraryFileSha256,
    string MachineClassLabel,
    string BuildTupleDigest,
    string PackageManifestSha256,
    string DeclaredSetSha256,
    string DesignBlob,
    string Ba05Blob,
    IReadOnlyList<DeclaredFile>? DeclaredSet = null)
{
    public const string FileName = "run-designation.json";

    public static RunDesignation Parse(string json)
    {
        var node = Jcs.ParseStrict(json);
        var errors = JsonSchemaLite.LoadEmbedded("ct21d.designation.v1.json").Validate(node);
        if (errors.Count > 0)
            throw new FormatException("run designation does not validate: " + string.Join("; ", errors));
        var o = (JsonObject)node;
        var declared = ((JsonArray)o["declaredSet"]!).Select(n => new DeclaredFile(n!["path"]!.GetValue<string>(), n["sha256"]!.GetValue<string>())).ToList();
        var t = (JsonObject)o["tupleBinding"]!;
        string S(JsonObject x, string k) => x[k]!.GetValue<string>();
        var parsed = new RunDesignation(
            S(o, "runId"), o["attempt"]!.GetValue<int>(), S(o, "sessionId"), S(o, "evidenceFolder"), S(o, "privateCopyPath"),
            S(o, "privateCopySha256"), S(o, "libraryPath"), S(o, "libraryFileSha256"), S(t, "machineClassLabel"),
            S(t, "buildTupleDigest"), S(t, "packageManifestSha256"), S(t, "declaredSetSha256"), S(t, "designBlob"), S(t, "ba05Blob"), declared);
        var problems = parsed.SeparationProblems();
        if (problems.Count > 0) throw new FormatException("run designation refused: " + string.Join("; ", problems));
        return parsed;
    }

    private static string Norm(string p) => System.IO.Path.GetFullPath(p).TrimEnd(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar);

    private static string DirOf(string p) => System.IO.Path.GetDirectoryName(Norm(p)) ?? "";

    /// <summary>
    /// The paths that must stay apart (the instrument refuses and writes nothing when any holds): the private copy must not be the
    /// library file, and the evidence folder must be neither the directory of the library nor that of the private copy (nor either file),
    /// otherwise the "private copy" check would not protect the library and the evidence seal would see the data files. Comparison is
    /// ordinal and case-insensitive (Windows paths) on the normalized full path; it is a path check, not a file-identity check.
    /// </summary>
    public IReadOnlyList<string> SeparationProblems()
    {
        var problems = new List<string>();
        var cmp = StringComparison.OrdinalIgnoreCase;
        var copy = Norm(PrivateCopyPath);
        var library = Norm(LibraryPath);
        var evidence = Norm(EvidenceFolder);
        if (string.Equals(copy, library, cmp)) problems.Add("PRIVATE_COPY_PATH_EQUALS_LIBRARY_PATH");
        if (string.Equals(evidence, DirOf(LibraryPath), cmp)) problems.Add("EVIDENCE_FOLDER_IS_THE_LIBRARY_DIRECTORY");
        if (string.Equals(evidence, DirOf(PrivateCopyPath), cmp)) problems.Add("EVIDENCE_FOLDER_IS_THE_PRIVATE_COPY_DIRECTORY");
        if (string.Equals(evidence, library, cmp) || string.Equals(evidence, copy, cmp)) problems.Add("EVIDENCE_FOLDER_IS_A_DATA_FILE");
        return problems;
    }

    /// <summary>Called by every runner before any file is touched: a designation whose paths overlap is refused and nothing is written.</summary>
    public void EnsureSeparation()
    {
        var problems = SeparationProblems();
        if (problems.Count > 0) throw new InvalidOperationException("DESIGNATION_REFUSED: " + string.Join(";", problems));
    }
}

/// <summary>The identity of the running instrument: its own DLL hash and the result of the self-pin comparison (design 5.1).</summary>
public sealed record InstrumentInfo(string Name, string Sha256, string SelfPinStatus)
{
    /// <summary>The only value of <see cref="SelfPinStatus"/> that lets a run be valid.</summary>
    public const string PinMatch = "MATCH";
}

/// <summary>
/// Self-hash pin of design 5.1: compares the instrument file on disk with a sidecar in the same folder. It protects against an
/// accidental mismatch only (it does not defend against a substituted DLL together with its sidecar and checks the file, not the
/// loaded image). The independent pins are the pre-H1 tuple record and the module-list hash.
/// </summary>
public static class SelfPin
{
    public static string PinFileFor(string dllPath) => dllPath + ".pin";

    /// <summary>Returns the instrument identity. A missing or different sidecar gives a status other than <see cref="InstrumentInfo.PinMatch"/>.</summary>
    public static InstrumentInfo Verify(string name, string dllPath)
    {
        var actual = Sha256Hex.OfFile(dllPath);
        var pin = PinFileFor(dllPath);
        if (!File.Exists(pin)) return new InstrumentInfo(name, actual, "PIN_FILE_MISSING");
        var expected = File.ReadAllText(pin).Trim().ToLowerInvariant();
        return new InstrumentInfo(name, actual, string.Equals(expected, actual, StringComparison.Ordinal) ? InstrumentInfo.PinMatch : "PIN_MISMATCH");
    }
}
