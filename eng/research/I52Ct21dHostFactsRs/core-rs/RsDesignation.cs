using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using I52Ct21d.HostFacts.Core;

namespace I52Ct21d.HostFacts.Rs.Core;

/// <summary>Loads and applies the schemas of the RS records (embedded in this assembly).</summary>
public static class RsSchemas
{
    public static JsonSchemaLite Load(string fileName)
    {
        var asm = typeof(RsSchemas).Assembly;
        using var stream = asm.GetManifestResourceStream("rs-schemas/" + fileName)
            ?? throw new FileNotFoundException("embedded RS schema not found: " + fileName);
        using var reader = new StreamReader(stream, new System.Text.UTF8Encoding(false, true));
        return new JsonSchemaLite((JsonObject)Jcs.ParseStrict(reader.ReadToEnd()));
    }

    public static void ValidateOrThrow(string fileName, JsonNode record)
    {
        var errors = Load(fileName).Validate(record);
        if (errors.Count > 0)
            throw new InvalidOperationException(fileName + " validation failed: " + string.Join("; ", errors.Take(10)));
    }
}

/// <summary>Raised when a run is refused BEFORE any side effect (no side database, no file written).</summary>
public sealed class RsRefusedException : Exception
{
    public IReadOnlyList<string> Reasons { get; }

    public RsRefusedException(IReadOnlyList<string> reasons) : base("RS_RUN_REFUSED: " + string.Join(";", reasons))
    {
        Reasons = reasons;
    }
}

/// <summary>What the CAD manager declares about the host that no instrument can observe from inside the process.</summary>
public sealed class HostChecksDeclaration
{
    public bool OtherAcadProcess { get; }

    public bool LoadRouteScripted { get; }

    public HostChecksDeclaration(bool otherAcadProcess, bool loadRouteScripted)
    {
        OtherAcadProcess = otherAcadProcess;
        LoadRouteScripted = loadRouteScripted;
    }
}

/// <summary>
/// The run designation of the RS class (schema <c>ct21d.designation.rs.v1</c>): the file the CAD manager places next to the RS DLL
/// (<c>run-designation-rs.json</c>). It EXTENDS the R0 designation (<c>ct21d.designation.v1</c>) with the scratch root, the governed
/// document path, the product paths and the host checks that only the CAD manager can declare. It is an INPUT: an instrument never writes it.
/// The format is this folder's own design (flagged for the Architect).
/// </summary>
public sealed class RsDesignation
{
    public const string FileName = "run-designation-rs.json";

    private static readonly Regex Tolscale = new(@"^HGP-H4-[0-9]{8}T[0-9]{6}Z-[0-9]{2}\z", RegexOptions.CultureInvariant);

    public string RunId { get; }
    public int Attempt { get; }
    public string SessionId { get; }
    public string EvidenceRoot { get; }
    public string ScratchRoot { get; }
    public string PrivateCopyPath { get; }
    public string PrivateCopySha256 { get; }
    public string LibraryPath { get; }
    public string LibraryFileSha256 { get; }
    public string GovernedDocumentPath { get; }
    public IReadOnlyList<string> ProductPaths { get; }
    public HostChecksDeclaration HostChecks { get; }
    public string MachineClassLabel { get; }
    public string BuildTupleDigest { get; }
    public string PackageManifestSha256 { get; }
    public string DeclaredSetSha256 { get; }
    public string DesignBlob { get; }
    public string Ba05Blob { get; }
    public IReadOnlyList<DeclaredFile> DeclaredSet { get; }

    public RsDesignation(
        string runId, int attempt, string sessionId, string evidenceRoot, string scratchRoot, string privateCopyPath, string privateCopySha256,
        string libraryPath, string libraryFileSha256, string governedDocumentPath, IReadOnlyList<string> productPaths, HostChecksDeclaration hostChecks,
        string machineClassLabel, string buildTupleDigest, string packageManifestSha256, string declaredSetSha256, string designBlob, string ba05Blob,
        IReadOnlyList<DeclaredFile> declaredSet)
    {
        RunId = runId;
        Attempt = attempt;
        SessionId = sessionId;
        EvidenceRoot = evidenceRoot;
        ScratchRoot = scratchRoot;
        PrivateCopyPath = privateCopyPath;
        PrivateCopySha256 = privateCopySha256;
        LibraryPath = libraryPath;
        LibraryFileSha256 = libraryFileSha256;
        GovernedDocumentPath = governedDocumentPath;
        ProductPaths = productPaths;
        HostChecks = hostChecks;
        MachineClassLabel = machineClassLabel;
        BuildTupleDigest = buildTupleDigest;
        PackageManifestSha256 = packageManifestSha256;
        DeclaredSetSha256 = declaredSetSha256;
        DesignBlob = designBlob;
        Ba05Blob = ba05Blob;
        DeclaredSet = declaredSet;
    }

    public static RsDesignation Parse(string json)
    {
        var node = Jcs.ParseStrict(json);
        RsSchemas.ValidateOrThrow("ct21d.designation.rs.v1.json", node);
        var o = (JsonObject)node;
        var t = (JsonObject)o["tupleBinding"]!;
        var h = (JsonObject)o["hostChecks"]!;
        string S(JsonObject x, string k) => x[k]!.GetValue<string>();
        var declared = ((JsonArray)o["declaredSet"]!).Select(n => new DeclaredFile(n!["path"]!.GetValue<string>(), n["sha256"]!.GetValue<string>())).ToList();
        var products = ((JsonArray)o["productPaths"]!).Select(n => n!.GetValue<string>()).ToList();
        var parsed = new RsDesignation(
            S(o, "runId"), o["attempt"]!.GetValue<int>(), S(o, "sessionId"), S(o, "evidenceRoot"), S(o, "scratchRoot"), S(o, "privateCopyPath"),
            S(o, "privateCopySha256"), S(o, "libraryPath"), S(o, "libraryFileSha256"), S(o, "governedDocumentPath"), products,
            new HostChecksDeclaration(h["otherAcadProcess"]!.GetValue<bool>(), h["loadRouteScripted"]!.GetValue<bool>()),
            S(t, "machineClassLabel"), S(t, "buildTupleDigest"), S(t, "packageManifestSha256"), S(t, "declaredSetSha256"), S(t, "designBlob"), S(t, "ba05Blob"),
            declared);
        var problems = parsed.SeparationProblems();
        if (problems.Count > 0) throw new FormatException("run designation refused: " + string.Join("; ", problems));
        return parsed;
    }

    public bool IsTolscaleRunId => Tolscale.IsMatch(RunId);

    /// <summary>
    /// The path rules (the instrument refuses and writes nothing when any holds). Every path is absolute; the evidence root and the scratch
    /// root are two different directories that do not contain one another; neither contains, or lies inside, the library file, the private
    /// copy, the governed document or any declared product path; neither has a <c>src</c> segment; the private copy is not the library.
    /// </summary>
    public IReadOnlyList<string> SeparationProblems()
    {
        var problems = new List<string>();
        var fields = new (string Name, string Path, bool Optional)[]
        {
            ("evidenceRoot", EvidenceRoot, false), ("scratchRoot", ScratchRoot, false), ("privateCopyPath", PrivateCopyPath, false),
            ("libraryPath", LibraryPath, false), ("governedDocumentPath", GovernedDocumentPath, true),
        };
        foreach (var (name, path, optional) in fields)
        {
            if (optional && path.Length == 0) continue;
            var why = PathRegions.TextProblem(path);
            if (why.Length > 0) problems.Add("PATH_" + why + ":" + name);
        }
        for (var i = 0; i < ProductPaths.Count; i++)
        {
            var why = PathRegions.TextProblem(ProductPaths[i]);
            if (why.Length > 0) problems.Add("PATH_" + why + ":productPaths[" + i + "]");
        }
        if (problems.Count > 0) return problems; // the comparisons below need normalizable absolute paths

        if (PathRegions.Same(PrivateCopyPath, LibraryPath)) problems.Add("PRIVATE_COPY_PATH_EQUALS_LIBRARY_PATH");
        if (PathRegions.Overlaps(EvidenceRoot, ScratchRoot)) problems.Add("EVIDENCE_ROOT_AND_SCRATCH_ROOT_OVERLAP");

        var data = new List<(string Name, string Path)> { ("library", LibraryPath), ("privateCopy", PrivateCopyPath) };
        if (GovernedDocumentPath.Length > 0) data.Add(("governedDocument", GovernedDocumentPath));
        foreach (var (rootName, root) in new[] { ("EVIDENCE_ROOT", EvidenceRoot), ("SCRATCH_ROOT", ScratchRoot) })
        {
            foreach (var (name, path) in data)
                if (PathRegions.Overlaps(root, path)) problems.Add(rootName + "_OVERLAPS_" + name.ToUpperInvariant());
            for (var i = 0; i < ProductPaths.Count; i++)
                if (PathRegions.Overlaps(root, ProductPaths[i])) problems.Add(rootName + "_OVERLAPS_PRODUCT_PATH[" + i + "]");
            if (PathRegions.HasSrcSegment(root)) problems.Add(rootName + "_HAS_A_SRC_SEGMENT");
        }
        return problems;
    }

    /// <summary>Called by every runner before any side effect.</summary>
    public void EnsureSeparation()
    {
        var problems = SeparationProblems();
        if (problems.Count > 0) throw new RsRefusedException(problems);
    }

    /// <summary>The R0 designation shape the shared record builders (<c>RecordJson.Tuple</c>) take. The evidence folder is the evidence root.</summary>
    public RunDesignation ToCore() => new(
        RunId, Attempt, SessionId, EvidenceRoot, PrivateCopyPath, PrivateCopySha256, LibraryPath, LibraryFileSha256, MachineClassLabel,
        BuildTupleDigest, PackageManifestSha256, DeclaredSetSha256, DesignBlob, Ba05Blob, DeclaredSet);
}
