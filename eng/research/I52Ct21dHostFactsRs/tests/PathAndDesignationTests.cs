using System.Text.Json.Nodes;
using I52Ct21d.HostFacts.Core;
using I52Ct21d.HostFacts.Rs.Core;
using Xunit;

namespace I52Ct21d.HostFacts.Rs.Tests;

public class PathRegionsTests
{
    [Theory]
    [InlineData("C:\\a\\b", "")]
    [InlineData("C:\\a b\\c.d", "")]
    [InlineData("D:\\x\\y\\", "")]
    [InlineData("relative\\path", "NOT_ABSOLUTE")]
    [InlineData(".\\x", "NOT_ABSOLUTE")]
    [InlineData("", "EMPTY")]
    [InlineData("   ", "EMPTY")]
    [InlineData("\\\\?\\C:\\x", "DEVICE_PATH")]
    [InlineData("\\\\.\\COM1", "DEVICE_PATH")]
    [InlineData("C:\\a\\..\\b", "PARENT_SEGMENT")]
    [InlineData("C:\\a\\x.txt:stream", "ALTERNATE_STREAM_MARKER")]
    [InlineData("C:\\a\\b.", "TRAILING_DOT_OR_SPACE")]
    [InlineData("C:\\a\\b \\c", "TRAILING_DOT_OR_SPACE")]
    [InlineData("C:\\a\tb", "CONTROL_CHARACTER")]
    public void TextProblem_classifies_declared_paths(string path, string expected) => Assert.Equal(expected, PathRegions.TextProblem(path));

    [Theory]
    [InlineData("C:\\a", "C:\\a", true)]
    [InlineData("C:\\a\\b", "C:\\a", true)]
    [InlineData("C:\\a\\b\\c", "C:\\a", true)]
    [InlineData("C:\\A\\B", "C:\\a", true)]
    [InlineData("C:\\ab", "C:\\a", false)]
    [InlineData("C:\\a", "C:\\a\\b", false)]
    [InlineData("C:\\a\\", "C:\\a", true)]
    [InlineData("D:\\a", "C:\\a", false)]
    public void IsInside_is_a_segment_wise_case_insensitive_prefix(string child, string parent, bool expected) =>
        Assert.Equal(expected, PathRegions.IsInside(child, parent));

    [Theory]
    [InlineData("C:\\a", "C:\\a\\b", true)]
    [InlineData("C:\\a\\b", "C:\\a", true)]
    [InlineData("C:\\a", "C:\\b", false)]
    [InlineData("C:\\ab", "C:\\a", false)]
    public void Overlaps_is_symmetric(string a, string b, bool expected)
    {
        Assert.Equal(expected, PathRegions.Overlaps(a, b));
        Assert.Equal(expected, PathRegions.Overlaps(b, a));
    }

    [Theory]
    [InlineData("C:\\proj\\src\\x", true)]
    [InlineData("C:\\SRC", true)]
    [InlineData("C:\\proj\\source\\x", false)]
    [InlineData("C:\\proj\\xsrc", false)]
    [InlineData("C:\\proj\\evidence", false)]
    public void HasSrcSegment_finds_a_segment_named_src(string path, bool expected) => Assert.Equal(expected, PathRegions.HasSrcSegment(path));

    [Fact]
    public void FirstReparsePoint_is_empty_for_an_ordinary_folder()
    {
        using var t = new TempDir();
        Assert.Equal("", PathRegions.FirstReparsePoint(t.Sub("plain")));
    }

    [Fact]
    public void FirstReparsePoint_finds_a_junction_in_the_path()
    {
        using var t = new TempDir();
        var target = t.Sub("target");
        var junction = Path.Combine(t.Path, "junction");
        var ok = Junction.TryCreate(junction, target);
        if (!ok) return; // the platform refused to create a junction: nothing to test here
        Assert.Equal(PathRegions.Norm(junction), PathRegions.FirstReparsePoint(junction));
        Assert.Equal(PathRegions.Norm(junction), PathRegions.FirstReparsePoint(Path.Combine(junction, "child")));
    }
}

/// <summary>Creates a directory junction with cmd (no administrator right needed). Returns false when it cannot.</summary>
internal static class Junction
{
    public static bool TryCreate(string link, string target)
    {
        try
        {
            var psi = new System.Diagnostics.ProcessStartInfo("cmd.exe", "/c mklink /J \"" + link + "\" \"" + target + "\"")
            {
                UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true,
            };
            using var p = System.Diagnostics.Process.Start(psi)!;
            p.WaitForExit(15000);
            return p.ExitCode == 0 && Directory.Exists(link);
        }
        catch (Exception)
        {
            return false;
        }
    }
}

public class RsDesignationTests
{
    [Fact]
    public void A_valid_designation_round_trips_through_json_and_the_schema()
    {
        using var rig = new Rig();
        var parsed = RsDesignation.Parse(rig.DesignationJson());
        Assert.Equal(rig.D.RunId, parsed.RunId);
        Assert.Equal(rig.D.ScratchRoot, parsed.ScratchRoot);
        Assert.Equal(rig.D.EvidenceRoot, parsed.EvidenceRoot);
        Assert.Equal(rig.D.ProductPaths, parsed.ProductPaths);
        Assert.True(parsed.HostChecks.LoadRouteScripted);
        Assert.False(parsed.HostChecks.OtherAcadProcess);
        Assert.Empty(parsed.SeparationProblems());
    }

    [Fact]
    public void ToCore_maps_the_evidence_root_and_the_tuple_bindings()
    {
        using var rig = new Rig();
        var core = rig.D.ToCore();
        Assert.Equal(rig.D.EvidenceRoot, core.EvidenceFolder);
        Assert.Equal(rig.D.BuildTupleDigest, core.BuildTupleDigest);
        Assert.Equal(rig.D.Ba05Blob, core.Ba05Blob);
        Assert.Equal(rig.D.RunId, core.RunId);
    }

    [Theory]
    [InlineData("schema")]
    [InlineData("runId")]
    [InlineData("evidenceRoot")]
    [InlineData("scratchRoot")]
    [InlineData("privateCopyPath")]
    [InlineData("privateCopySha256")]
    [InlineData("libraryPath")]
    [InlineData("libraryFileSha256")]
    [InlineData("governedDocumentPath")]
    [InlineData("productPaths")]
    [InlineData("hostChecks")]
    [InlineData("declaredSet")]
    [InlineData("tupleBinding")]
    public void A_missing_required_field_fails_the_schema(string field)
    {
        using var rig = new Rig();
        var node = (JsonObject)JsonNode.Parse(rig.DesignationJson())!;
        node.Remove(field);
        Assert.Throws<InvalidOperationException>(() => RsDesignation.Parse(node.ToJsonString()));
    }

    [Fact]
    public void An_unknown_field_fails_the_closed_schema()
    {
        using var rig = new Rig();
        var node = (JsonObject)JsonNode.Parse(rig.DesignationJson())!;
        node["extra"] = 1;
        Assert.Throws<InvalidOperationException>(() => RsDesignation.Parse(node.ToJsonString()));
    }

    [Theory]
    [InlineData("runId", "HGP-4-20260930T120000Z-01")]
    [InlineData("privateCopySha256", "ABC")]
    [InlineData("libraryFileSha256", "zz")]
    public void A_malformed_value_fails_the_schema(string field, string value)
    {
        using var rig = new Rig();
        var node = (JsonObject)JsonNode.Parse(rig.DesignationJson())!;
        node[field] = value;
        Assert.Throws<InvalidOperationException>(() => RsDesignation.Parse(node.ToJsonString()));
    }

    [Fact]
    public void An_empty_product_path_list_fails_the_schema()
    {
        using var rig = new Rig();
        var node = (JsonObject)JsonNode.Parse(rig.DesignationJson())!;
        node["productPaths"] = new JsonArray();
        Assert.Throws<InvalidOperationException>(() => RsDesignation.Parse(node.ToJsonString()));
    }

    [Fact]
    public void The_load_route_and_the_acad_process_must_be_declared()
    {
        using var rig = new Rig();
        var node = (JsonObject)JsonNode.Parse(rig.DesignationJson())!;
        ((JsonObject)node["hostChecks"]!).Remove("loadRouteScripted");
        Assert.Throws<InvalidOperationException>(() => RsDesignation.Parse(node.ToJsonString()));
    }

    [Theory]
    [InlineData("HGP-H4-20260930T120000Z-01", true)]
    [InlineData("HGP-H3-20260930T120000Z-01", false)]
    [InlineData("HGP-H44-20260930T120000Z-01", false)]
    public void Only_an_H4_run_id_is_a_tolscale_run(string runId, bool expected)
    {
        using var rig = new Rig(runId);
        Assert.Equal(expected, rig.D.IsTolscaleRunId);
    }

    // ---- the separation rules (Owner Q-O-1: refuse overlaps with the library, the private copy, the governed document, src/ and any product path) ----

    private static string[] Problems(Rig rig, Func<RsDesignation, RsDesignation> change)
    {
        var d = change(rig.D);
        return d.SeparationProblems().ToArray();
    }

    private static RsDesignation Copy(RsDesignation d, string? evidence = null, string? scratch = null, string? copy = null, string? library = null,
        string? governed = null, IReadOnlyList<string>? products = null) =>
        new(d.RunId, d.Attempt, d.SessionId, evidence ?? d.EvidenceRoot, scratch ?? d.ScratchRoot, copy ?? d.PrivateCopyPath, d.PrivateCopySha256,
            library ?? d.LibraryPath, d.LibraryFileSha256, governed ?? d.GovernedDocumentPath, products ?? d.ProductPaths, d.HostChecks, d.MachineClassLabel,
            d.BuildTupleDigest, d.PackageManifestSha256, d.DeclaredSetSha256, d.DesignBlob, d.Ba05Blob, d.DeclaredSet);

    [Fact]
    public void The_private_copy_must_not_be_the_library()
    {
        using var rig = new Rig();
        Assert.Contains("PRIVATE_COPY_PATH_EQUALS_LIBRARY_PATH", Problems(rig, d => Copy(d, copy: d.LibraryPath)));
    }

    [Fact]
    public void The_scratch_root_must_not_equal_the_evidence_root()
    {
        using var rig = new Rig();
        Assert.Contains("EVIDENCE_ROOT_AND_SCRATCH_ROOT_OVERLAP", Problems(rig, d => Copy(d, scratch: d.EvidenceRoot)));
    }

    [Fact]
    public void The_scratch_root_must_not_lie_inside_the_evidence_root()
    {
        using var rig = new Rig();
        var inner = Path.Combine(rig.Evidence, "scratch");
        Assert.Contains("EVIDENCE_ROOT_AND_SCRATCH_ROOT_OVERLAP", Problems(rig, d => Copy(d, scratch: inner)));
    }

    [Fact]
    public void The_evidence_root_must_not_lie_inside_the_scratch_root()
    {
        using var rig = new Rig();
        var inner = Path.Combine(rig.Scratch, "evidence");
        Assert.Contains("EVIDENCE_ROOT_AND_SCRATCH_ROOT_OVERLAP", Problems(rig, d => Copy(d, evidence: inner)));
    }

    [Theory]
    [InlineData("library")]
    [InlineData("privateCopy")]
    public void The_scratch_root_must_not_be_the_directory_of_a_data_file(string which)
    {
        using var rig = new Rig();
        var dir = which == "library" ? Path.GetDirectoryName(rig.Library)! : Path.GetDirectoryName(rig.PrivateCopy)!;
        var problems = Problems(rig, d => Copy(d, scratch: dir));
        Assert.Contains("SCRATCH_ROOT_OVERLAPS_" + which.ToUpperInvariant(), problems);
    }

    [Theory]
    [InlineData("library")]
    [InlineData("privateCopy")]
    public void The_evidence_root_must_not_be_the_directory_of_a_data_file(string which)
    {
        using var rig = new Rig();
        var dir = which == "library" ? Path.GetDirectoryName(rig.Library)! : Path.GetDirectoryName(rig.PrivateCopy)!;
        var problems = Problems(rig, d => Copy(d, evidence: dir));
        Assert.Contains("EVIDENCE_ROOT_OVERLAPS_" + which.ToUpperInvariant(), problems);
    }

    [Fact]
    public void The_scratch_root_must_not_contain_the_library_even_from_above()
    {
        using var rig = new Rig();
        var above = Path.GetDirectoryName(Path.GetDirectoryName(rig.Library))!;
        Assert.Contains("SCRATCH_ROOT_OVERLAPS_LIBRARY", Problems(rig, d => Copy(d, scratch: above)));
    }

    [Fact]
    public void The_governed_document_is_protected_when_declared()
    {
        using var rig = new Rig(governed: "");
        var doc = Path.Combine(rig.Temp.Sub("governed"), "Drawing1.dwg");
        var withDoc = Copy(rig.D, governed: doc);
        Assert.Empty(withDoc.SeparationProblems());
        Assert.Contains("SCRATCH_ROOT_OVERLAPS_GOVERNEDDOCUMENT", Copy(withDoc, scratch: Path.GetDirectoryName(doc)).SeparationProblems());
        Assert.Contains("EVIDENCE_ROOT_OVERLAPS_GOVERNEDDOCUMENT", Copy(withDoc, evidence: Path.GetDirectoryName(doc)).SeparationProblems());
    }

    [Theory]
    [InlineData("scratch")]
    [InlineData("evidence")]
    public void A_root_must_not_overlap_a_declared_product_path(string which)
    {
        using var rig = new Rig();
        var product = rig.Product;
        var d = which == "scratch" ? Copy(rig.D, scratch: Path.Combine(product, "inside")) : Copy(rig.D, evidence: Path.Combine(product, "inside"));
        Assert.Contains(which.ToUpperInvariant() == "SCRATCH" ? "SCRATCH_ROOT_OVERLAPS_PRODUCT_PATH[0]" : "EVIDENCE_ROOT_OVERLAPS_PRODUCT_PATH[0]", d.SeparationProblems());
        var d2 = which == "scratch" ? Copy(rig.D, products: new[] { rig.Scratch }) : Copy(rig.D, products: new[] { rig.Evidence });
        Assert.Contains(which.ToUpperInvariant() == "SCRATCH" ? "SCRATCH_ROOT_OVERLAPS_PRODUCT_PATH[0]" : "EVIDENCE_ROOT_OVERLAPS_PRODUCT_PATH[0]", d2.SeparationProblems());
    }

    [Theory]
    [InlineData("scratch")]
    [InlineData("evidence")]
    public void A_root_with_a_src_segment_is_refused(string which)
    {
        using var rig = new Rig();
        var src = Path.Combine(rig.Temp.Path, "src", "x");
        Directory.CreateDirectory(src);
        var d = which == "scratch" ? Copy(rig.D, scratch: src) : Copy(rig.D, evidence: src);
        Assert.Contains((which == "scratch" ? "SCRATCH_ROOT" : "EVIDENCE_ROOT") + "_HAS_A_SRC_SEGMENT", d.SeparationProblems());
    }

    [Theory]
    [InlineData("relative\\scratch")]
    [InlineData("C:\\a\\..\\b")]
    [InlineData("C:\\a\\b:stream")]
    public void A_non_absolute_or_odd_root_is_refused_before_any_comparison(string path)
    {
        using var rig = new Rig();
        var problems = Copy(rig.D, scratch: path).SeparationProblems();
        Assert.Single(problems);
        Assert.StartsWith("PATH_", problems[0]);
        Assert.EndsWith(":scratchRoot", problems[0]);
    }

    [Fact]
    public void Parse_refuses_overlapping_paths_with_a_FormatException()
    {
        using var rig = new Rig();
        var d = Copy(rig.D, scratch: rig.D.EvidenceRoot);
        var ex = Assert.Throws<FormatException>(() => RsDesignation.Parse(Rig.DesignationJson(d)));
        Assert.Contains("EVIDENCE_ROOT_AND_SCRATCH_ROOT_OVERLAP", ex.Message);
    }

    [Fact]
    public void EnsureSeparation_throws_RsRefusedException_with_the_reasons()
    {
        using var rig = new Rig();
        var ex = Assert.Throws<RsRefusedException>(() => Copy(rig.D, scratch: rig.D.EvidenceRoot).EnsureSeparation());
        Assert.Contains("EVIDENCE_ROOT_AND_SCRATCH_ROOT_OVERLAP", ex.Reasons);
    }
}
