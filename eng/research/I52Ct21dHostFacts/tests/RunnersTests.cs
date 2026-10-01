using System.Text.Json.Nodes;
using I52Ct21d.HostFacts.Core;
using Xunit;

namespace I52Ct21d.HostFacts.Tests;

internal static class RunnersTestsSupport
{
    public const string PinOk = InstrumentInfo.PinMatch;

    public static RunDesignation Designation(string root, string? privateCopy = null, string? privateSha = null)
    {
        var copy = privateCopy ?? Path.Combine(root, "private-copy.dwg");
        if (!File.Exists(copy)) File.WriteAllBytes(copy, new byte[] { 1, 2, 3, 4, 5 });
        var sha = privateSha ?? Support.Sha(copy);
        return new RunDesignation(
            "HGP-H1-20260930T120000Z-01", 1, "session-1", Path.Combine(root, "evidence"), copy, sha, "D:\\lib\\library.dwg", sha,
            "MC-47229d23a125", new string('b', 64), new string('c', 64), new string('d', 64), new string('e', 40), "b74af94ece4f0901a071ef5176f3c58bff01cfdc",
            new[] { new DeclaredFile("I52Ct21d.HostFacts.R0.dll", new string('1', 64)), new DeclaredFile("I52Ct21d.HostFacts.Core.dll", new string('2', 64)) });
    }

    public static string DesignationJson(RunDesignation d) => Jcs.Serialize(new JsonObject
    {
        ["schema"] = "ct21d.designation.v1", ["runId"] = d.RunId, ["attempt"] = d.Attempt, ["sessionId"] = d.SessionId,
        ["evidenceFolder"] = d.EvidenceFolder, ["privateCopyPath"] = d.PrivateCopyPath, ["privateCopySha256"] = d.PrivateCopySha256,
        ["libraryPath"] = d.LibraryPath, ["libraryFileSha256"] = d.LibraryFileSha256,
        ["declaredSet"] = RecordJson.Arr(d.DeclaredSet!.Select(f => (JsonNode?)new JsonObject { ["path"] = f.Path, ["sha256"] = f.Sha256 })),
        ["tupleBinding"] = new JsonObject
        {
            ["machineClassLabel"] = d.MachineClassLabel, ["buildTupleDigest"] = d.BuildTupleDigest,
            ["packageManifestSha256"] = d.PackageManifestSha256, ["declaredSetSha256"] = d.DeclaredSetSha256,
            ["designBlob"] = d.DesignBlob, ["ba05Blob"] = d.Ba05Blob,
        },
    });

    public static InstrumentInfo Instrument(string pin = PinOk) => new("I52Ct21d.HostFacts.R0", new string('f', 64), pin);

    public static JsonObject ReadRecord(string folder, string name) => (JsonObject)Jcs.ParseStrict(File.ReadAllText(Path.Combine(folder, name)));

    public static StoredBufferScan Stored() => new(new[]
    {
        new StoredEntry(1, "System.String"), new StoredEntry(1, "System.String"), new StoredEntry(70, "System.Int16"),
        new StoredEntry(70, "System.Int32"), new StoredEntry(40, "System.Double"),
        new StoredEntry(290, "System.Int16"),                                            // BA-05 expects Boolean: OBSERVED_DIFFERS
        new StoredEntry(10, "Autodesk.AutoCAD.Geometry.Point3d"),
        new StoredEntry(5, "System.String"),                                             // outside the table
    }, 4, 1);
}

public class RunnersTests
{
    [Fact]
    public void Census_run_writes_the_raw_record_and_three_fact_records_through_the_evidence_writer_only()
    {
        using var t = new TempDir();
        var d = RunnersTestsSupport.Designation(t.Path);
        Directory.CreateDirectory(d.EvidenceFolder);
        var copyBytes = File.ReadAllBytes(d.PrivateCopyPath);
        var source = new FakeSource { Blocks = Samples.Library(), Stored = RunnersTestsSupport.Stored() };
        var writer = new EvidenceWriter(d.EvidenceFolder);

        var outcome = Runners.RunCensus(d, RunnersTestsSupport.Instrument(), new FakeEnv(), source, FakeVars.Default(), writer);

        Assert.Empty(outcome.InvalidReasons);
        Assert.Equal("OBSERVED", outcome.FactStatuses["HF-C1"]);   // *U12 is anonymous, not unreadable: the census is complete
        Assert.Equal("OBSERVED", outcome.FactStatuses["HF-C3"]);
        Assert.Equal("OBSERVED_DIFFERS", outcome.FactStatuses["HF-G1"]);
        Assert.True(source.Last!.Disposed);
        Assert.Equal(d.PrivateCopyPath, source.OpenedPath);

        var files = Directory.EnumerateFiles(d.EvidenceFolder).Select(Path.GetFileName).OrderBy(x => x, StringComparer.Ordinal).ToArray();
        Assert.Equal(new[] { "census-raw.json", "hostfact-HF-C1.json", "hostfact-HF-C3.json", "hostfact-HF-G1.json" }, files);
        // every written file is in the ledger with its SHA-256, computed independently here
        Assert.Equal(files.Length, outcome.Written.Count);
        foreach (var w in outcome.Written) Assert.Equal(Support.Sha(Path.Combine(d.EvidenceFolder, w.Name)), w.Sha256);
        // nothing else was touched: the private copy is byte-identical
        Assert.Equal(copyBytes, File.ReadAllBytes(d.PrivateCopyPath));

        var raw = RunnersTestsSupport.ReadRecord(d.EvidenceFolder, "census-raw.json");
        Assert.Empty(JsonSchemaLite.LoadEmbedded("ct21d.census.v1.json").Validate(raw));
        var rawSha = outcome.Written.Single(w => w.Name == "census-raw.json").Sha256;
        foreach (var f in new[] { "HF-C1", "HF-C3", "HF-G1" })
        {
            var fact = RunnersTestsSupport.ReadRecord(d.EvidenceFolder, "hostfact-" + f + ".json");
            Assert.Empty(JsonSchemaLite.LoadEmbedded("ct21d.hostfact.v1.json").Validate(fact));
            Assert.Equal(rawSha, fact["rawRefs"]![0]!["sha256"]!.GetValue<string>());
            Assert.Equal("census-raw.json", fact["rawRefs"]![0]!["path"]!.GetValue<string>());
            Assert.False(fact["governing"]!.GetValue<bool>());
            Assert.Equal(d.MachineClassLabel, fact["tuple"]!["TB-C"]!["machineClassLabel"]!.GetValue<string>());
            Assert.Equal(Support.Sha(d.PrivateCopyPath), fact["tuple"]!["TB-L"]!["privateCopySha256After"]!.GetValue<string>());
        }
        Assert.Equal("NOT_READ_BY_R0", raw["identity"]!["dynamicPropertyMethod"]!.GetValue<string>());
    }

    [Fact]
    public void Census_records_are_deterministic_apart_from_the_volatile_part()
    {
        using var t = new TempDir();
        var d = RunnersTestsSupport.Designation(t.Path);
        JsonObject Run(string folder, DateTime when, int pid)
        {
            var dd = d with { EvidenceFolder = Path.Combine(t.Path, folder) };
            Directory.CreateDirectory(dd.EvidenceFolder);
            Runners.RunCensus(dd, RunnersTestsSupport.Instrument(), new FakeEnv { UtcNow = when, ProcessId = pid },
                new FakeSource { Blocks = Samples.Library(), Stored = RunnersTestsSupport.Stored() }, FakeVars.Default(), new EvidenceWriter(dd.EvidenceFolder));
            return RunnersTestsSupport.ReadRecord(dd.EvidenceFolder, "census-raw.json");
        }
        var a = Run("e1", new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), 1);
        var b = Run("e2", new DateTime(2030, 6, 6, 6, 6, 6, DateTimeKind.Utc), 99999);
        Assert.NotEqual(Jcs.Serialize(a["volatile"]), Jcs.Serialize(b["volatile"]));
        Assert.Equal(a["contentSha256"]!.GetValue<string>(), b["contentSha256"]!.GetValue<string>());
        a.Remove("volatile");
        b.Remove("volatile");
        Assert.Equal(Jcs.Serialize(a), Jcs.Serialize(b));
        Assert.Equal(a["contentSha256"]!.GetValue<string>(), RecordJson.ContentSha256(a));
    }

    [Fact]
    public void Census_marks_an_unreadable_block_unknown_and_the_incomplete_census_invalid()
    {
        using var t = new TempDir();
        var d = RunnersTestsSupport.Designation(t.Path);
        Directory.CreateDirectory(d.EvidenceFolder);
        var blocks = Samples.Library().Concat(new[] { Samples.Block("", Array.Empty<EntityInfo>(), error: "BLOCK_RECORD_UNREADABLE:AccessViolation") }).ToList();
        var outcome = Runners.RunCensus(d, RunnersTestsSupport.Instrument(), new FakeEnv(), new FakeSource { Blocks = blocks }, FakeVars.Default(), new EvidenceWriter(d.EvidenceFolder));
        Assert.Equal("INVALID", outcome.FactStatuses["HF-C1"]);
        var raw = RunnersTestsSupport.ReadRecord(d.EvidenceFolder, "census-raw.json");
        Assert.Equal("UNKNOWN", raw["blocks"]![blocks.Count - 1]!["status"]!.GetValue<string>());
        Assert.Equal(1, raw["counts"]!["blocksUnknown"]!.GetValue<int>());
    }

    [Fact]
    public void Census_without_any_range_occurrence_or_dimension_is_not_observed_never_filled()
    {
        using var t = new TempDir();
        var d = RunnersTestsSupport.Designation(t.Path);
        Directory.CreateDirectory(d.EvidenceFolder);
        var blocks = new[] { Samples.Block("A", new[] { Samples.Entity("AcDbLine", "LINE") }) };
        var outcome = Runners.RunCensus(d, RunnersTestsSupport.Instrument(), new FakeEnv(), new FakeSource { Blocks = blocks }, FakeVars.Default(), new EvidenceWriter(d.EvidenceFolder));
        Assert.Equal("OBSERVED", outcome.FactStatuses["HF-C1"]);
        Assert.Equal("NOT_OBSERVED", outcome.FactStatuses["HF-C3"]);
        Assert.Equal("NOT_OBSERVED", outcome.FactStatuses["HF-G1"]);
        var raw = RunnersTestsSupport.ReadRecord(d.EvidenceFolder, "census-raw.json");
        Assert.All((JsonArray)raw["rbTypes"]!, r => Assert.Equal("NOT_OBSERVED", r!["status"]!.GetValue<string>()));
    }

    [Fact]
    public void A_malformed_dstyle_section_makes_the_fact_unknown()
    {
        using var t = new TempDir();
        var d = RunnersTestsSupport.Designation(t.Path);
        Directory.CreateDirectory(d.EvidenceFolder);
        var blocks = new[] { Samples.Block("A", new[] { Samples.Entity("AcDbAlignedDimension", "DIMENSION", dstyle: new DstyleInfo(true, false, Array.Empty<DstylePair>())) }) };
        var outcome = Runners.RunCensus(d, RunnersTestsSupport.Instrument(), new FakeEnv(), new FakeSource { Blocks = blocks }, FakeVars.Default(), new EvidenceWriter(d.EvidenceFolder));
        Assert.Equal("UNKNOWN", outcome.FactStatuses["HF-C3"]);
    }

    [Fact]
    public void Every_fact_of_a_run_is_invalid_when_a_host_run_check_fails_and_the_records_are_still_retained()
    {
        // 1. the private copy does not have the hash that the designation declares
        using (var t = new TempDir())
        {
            var d0 = RunnersTestsSupport.Designation(t.Path);
            var d = d0 with { PrivateCopySha256 = new string('0', 64) };
            Directory.CreateDirectory(d.EvidenceFolder);
            var o = Runners.RunCensus(d, RunnersTestsSupport.Instrument(), new FakeEnv(), new FakeSource { Blocks = Samples.Library() }, FakeVars.Default(), new EvidenceWriter(d.EvidenceFolder));
            Assert.All(o.FactStatuses.Values, s => Assert.Equal("INVALID", s));
            Assert.Contains("PRIVATE_COPY_HASH_DIFFERS_FROM_DESIGNATION", o.InvalidReasons);
            Assert.Equal(4, Directory.EnumerateFiles(d.EvidenceFolder).Count()); // retained, not discarded
        }
        // 2. the private copy changes while it is read
        using (var t = new TempDir())
        {
            var d = RunnersTestsSupport.Designation(t.Path);
            Directory.CreateDirectory(d.EvidenceFolder);
            var src = new FakeSource { Blocks = Samples.Library(), OnRead = () => File.AppendAllText(d.PrivateCopyPath, "tamper") };
            var o = Runners.RunCensus(d, RunnersTestsSupport.Instrument(), new FakeEnv(), src, FakeVars.Default(), new EvidenceWriter(d.EvidenceFolder));
            Assert.Contains("PRIVATE_COPY_CHANGED", o.InvalidReasons);
            Assert.All(o.FactStatuses.Values, s => Assert.Equal("INVALID", s));
        }
        // 3. DBMOD changes between the reads
        using (var t = new TempDir())
        {
            var d = RunnersTestsSupport.Designation(t.Path);
            Directory.CreateDirectory(d.EvidenceFolder);
            var vars = FakeVars.Default();
            vars.DbmodSequence = new Queue<string>(new[] { "0", "1" });
            var o = Runners.RunInventory(d, RunnersTestsSupport.Instrument(), new FakeEnv(), new FakeSource { Blocks = Samples.Library() }, vars, new EvidenceWriter(d.EvidenceFolder));
            Assert.Contains("DBMOD_CHANGED", o.InvalidReasons);
            Assert.Equal("INVALID", o.FactStatuses["HF-T2"]);
        }
        // 4. the self-pin does not match
        using (var t = new TempDir())
        {
            var d = RunnersTestsSupport.Designation(t.Path);
            Directory.CreateDirectory(d.EvidenceFolder);
            var o = Runners.RunCtxVars(d, RunnersTestsSupport.Instrument("PIN_MISMATCH"), new FakeEnv(), FakeVars.Default(), new EvidenceWriter(d.EvidenceFolder));
            Assert.Equal("INVALID", o.FactStatuses["HF-G3"]);
            Assert.Contains("SELF_PIN_PIN_MISMATCH", o.InvalidReasons);
        }
        // 5. the private copy is not the library file that the designation names
        using (var t = new TempDir())
        {
            var d0 = RunnersTestsSupport.Designation(t.Path);
            var d = d0 with { LibraryFileSha256 = new string('9', 64) };
            Directory.CreateDirectory(d.EvidenceFolder);
            var o = Runners.RunCensus(d, RunnersTestsSupport.Instrument(), new FakeEnv(), new FakeSource { Blocks = Samples.Library() }, FakeVars.Default(), new EvidenceWriter(d.EvidenceFolder));
            Assert.Contains("PRIVATE_COPY_HASH_DIFFERS_FROM_LIBRARY_HASH", o.InvalidReasons);
        }
    }

    [Fact]
    public void Inventory_records_the_scale_bit_patterns_and_Imax_computed_independently()
    {
        using var t = new TempDir();
        var d = RunnersTestsSupport.Designation(t.Path);
        Directory.CreateDirectory(d.EvidenceFolder);
        var o = Runners.RunInventory(d, RunnersTestsSupport.Instrument(), new FakeEnv(), new FakeSource { Blocks = Samples.Library() }, FakeVars.Default(), new EvidenceWriter(d.EvidenceFolder));
        Assert.Equal("OBSERVED", o.FactStatuses["HF-T2"]);
        Assert.Equal(new[] { "hostfact-HF-T2.json", "inventory-raw.json" }, Directory.EnumerateFiles(d.EvidenceFolder).Select(Path.GetFileName).OrderBy(x => x, StringComparer.Ordinal).ToArray());
        var raw = RunnersTestsSupport.ReadRecord(d.EvidenceFolder, "inventory-raw.json");
        Assert.Empty(JsonSchemaLite.LoadEmbedded("ct21d.inventory.v1.json").Validate(raw));
        var summary = raw["summary"]!;
        Assert.Equal(3, summary["count"]!.GetValue<int>());
        Assert.Equal(3, summary["distinctTriples"]!.GetValue<int>());
        // |1.0000000001 - 1.0| as binary64, computed by python: 1.000000082740371e-10 = 0x3DDB7CE000000000
        Assert.Equal("3DDB7CE000000000", summary["ImaxHex"]!.GetValue<string>());
        var refs = (JsonArray)raw["references"]!;
        // 1.0 = 3FF0000000000000, -1.0 = BFF0000000000000
        Assert.Equal("3FF0000000000000", refs[0]!["scale"]![0]!.GetValue<string>());
        Assert.Equal("BFF0000000000000", refs[0]!["scale"]![2]!.GetValue<string>());
        Assert.Equal("-1", refs[0]!["nearestMember"]![2]!.GetValue<string>());
        Assert.Equal("3FF000000006DF38", refs[1]!["scale"]![0]!.GetValue<string>());
    }

    [Fact]
    public void Inventory_marks_a_non_finite_or_unread_scale_unknown_and_an_empty_library_not_observed()
    {
        var blocks = new[] { Samples.Block("A", new[]
        {
            Samples.Entity("AcDbBlockReference", "INSERT", "B", new[] { double.NaN, 1.0, 1.0 }),
            Samples.Entity("AcDbBlockReference", "INSERT", "B", null),
            Samples.Entity("AcDbBlockReference", "INSERT", "B", new[] { 1.0, 1.0, 1.0 }),
            Samples.Entity("AcDbMInsertBlock", "INSERT", "B", new[] { 7.0, 7.0, 7.0 }), // exact class only: never a subclass
        }) };
        var (_, s) = InventoryBuilder.Build(blocks, new string('a', 64), "p", RunnersTestsSupport.Instrument());
        Assert.Equal((3, 1, 2, "UNKNOWN"), (s.Count, s.Observed, s.Unknown, s.Status));
        Assert.Equal(0.0, s.Imax);
        var (_, empty) = InventoryBuilder.Build(new[] { Samples.Block("A", new[] { Samples.Entity("AcDbLine", "LINE") }) }, new string('a', 64), "p", RunnersTestsSupport.Instrument());
        Assert.Equal("NOT_OBSERVED", empty.Status);
        Assert.Null(empty.Imax);
    }

    [Fact]
    public void Context_variables_record_the_host_type_and_compare_it_with_the_expectation()
    {
        using var t = new TempDir();
        var d = RunnersTestsSupport.Designation(t.Path);
        Directory.CreateDirectory(d.EvidenceFolder);
        var vars = FakeVars.Default();
        var o = Runners.RunCtxVars(d, RunnersTestsSupport.Instrument(), new FakeEnv(), vars, new EvidenceWriter(d.EvidenceFolder));
        Assert.Equal("OBSERVED", o.FactStatuses["HF-G3"]);
        Assert.Equal(new[] { "ctxvars-raw.json", "hostfact-HF-G3.json" }, Directory.EnumerateFiles(d.EvidenceFolder).Select(Path.GetFileName).OrderBy(x => x, StringComparer.Ordinal).ToArray());
        var raw = RunnersTestsSupport.ReadRecord(d.EvidenceFolder, "ctxvars-raw.json");
        Assert.Empty(JsonSchemaLite.LoadEmbedded("ct21d.ctxvars.v1.json").Validate(raw));
        Assert.Equal(10, ((JsonArray)raw["ctxVars"]!).Count);
        Assert.True(raw["tuple"]!["TB-L"]!["notApplicable"]!.GetValue<bool>());

        using var t2 = new TempDir();
        var d2 = RunnersTestsSupport.Designation(t2.Path);
        Directory.CreateDirectory(d2.EvidenceFolder);
        var differs = FakeVars.Default().Set("CELWEIGHT", "System.Int32", "25"); // BA-05 expects Int16
        differs.Values["PSTYLEMODE"] = new SysVarReading("PSTYLEMODE", null, null, null, "NOT_FOUND");
        var o2 = Runners.RunCtxVars(d2, RunnersTestsSupport.Instrument(), new FakeEnv(), differs, new EvidenceWriter(d2.EvidenceFolder), new[] { "USERI1" });
        Assert.Equal("UNKNOWN", o2.FactStatuses["HF-G3"]); // an unreadable variable dominates
        var raw2 = RunnersTestsSupport.ReadRecord(d2.EvidenceFolder, "ctxvars-raw.json");
        var list = (JsonArray)raw2["ctxVars"]!;
        Assert.Equal("OBSERVED_DIFFERS", list.Single(x => x!["name"]!.GetValue<string>() == "CELWEIGHT")!["status"]!.GetValue<string>());
        Assert.Equal("UNKNOWN", list.Single(x => x!["name"]!.GetValue<string>() == "USERI1")!["status"]!.GetValue<string>()); // not found: no expectation, no value
        Assert.Equal(11, list.Count);
    }

    [Fact]
    public void A_double_valued_variable_keeps_its_bit_pattern()
    {
        var (list, _, _) = CtxVarsBuilder.Build(new[] { new SysVarReading("CELTSCALE", "System.Double", "1", DoubleBits.Hex(1.0), null) });
        Assert.Equal("3FF0000000000000", list[0]!["valueBitsHex"]!.GetValue<string>());
        Assert.True(list[0]!["matches"]!.GetValue<bool>());
    }

    // ---- the designated paths must stay apart (the instrument refuses and writes nothing) ----------------------------------
    public static IEnumerable<object[]> OverlappingDesignations()
    {
        yield return new object[] { "PRIVATE_COPY_PATH_EQUALS_LIBRARY_PATH" };
        yield return new object[] { "EVIDENCE_FOLDER_IS_THE_LIBRARY_DIRECTORY" };
        yield return new object[] { "EVIDENCE_FOLDER_IS_THE_PRIVATE_COPY_DIRECTORY" };
        yield return new object[] { "EVIDENCE_FOLDER_IS_A_DATA_FILE" };
    }

    private static RunDesignation Overlapping(RunDesignation d, string kind) => kind switch
    {
        "PRIVATE_COPY_PATH_EQUALS_LIBRARY_PATH" => d with { LibraryPath = d.PrivateCopyPath.ToUpperInvariant() },   // case differs: still the same Windows path
        "EVIDENCE_FOLDER_IS_THE_LIBRARY_DIRECTORY" => d with { LibraryPath = Path.Combine(d.EvidenceFolder, "library.dwg") },
        "EVIDENCE_FOLDER_IS_THE_PRIVATE_COPY_DIRECTORY" => d with { EvidenceFolder = Path.GetDirectoryName(d.PrivateCopyPath)! + Path.DirectorySeparatorChar },
        _ => d with { EvidenceFolder = d.PrivateCopyPath },
    };

    [Theory]
    [MemberData(nameof(OverlappingDesignations))]
    public void An_overlapping_designation_is_refused_by_the_parser_and_by_every_runner_and_nothing_is_written(string kind)
    {
        using var t = new TempDir();
        var good = RunnersTestsSupport.Designation(t.Path);
        Directory.CreateDirectory(good.EvidenceFolder);
        Assert.Empty(good.SeparationProblems());
        var d = Overlapping(good, kind);
        Assert.Contains(kind, d.SeparationProblems());

        var ex = Assert.Throws<FormatException>(() => RunDesignation.Parse(RunnersTestsSupport.DesignationJson(d)));
        Assert.Contains(kind, ex.Message);

        var folder = Directory.Exists(d.EvidenceFolder) && !File.Exists(d.EvidenceFolder) ? d.EvidenceFolder : good.EvidenceFolder;
        var before = Directory.EnumerateFileSystemEntries(t.Path, "*", SearchOption.AllDirectories).OrderBy(x => x, StringComparer.Ordinal).Select(f => (f, File.Exists(f) ? Support.Sha(f) : "")).ToArray();
        var source = new FakeSource { Blocks = Samples.Library(), Stored = RunnersTestsSupport.Stored() };
        var writer = new EvidenceWriter(folder);
        Assert.Throws<InvalidOperationException>(() => Runners.RunCensus(d, RunnersTestsSupport.Instrument(), new FakeEnv(), source, FakeVars.Default(), writer));
        Assert.Throws<InvalidOperationException>(() => Runners.RunInventory(d, RunnersTestsSupport.Instrument(), new FakeEnv(), source, FakeVars.Default(), writer));
        Assert.Throws<InvalidOperationException>(() => Runners.RunCtxVars(d, RunnersTestsSupport.Instrument(), new FakeEnv(), FakeVars.Default(), writer));
        Assert.Null(source.OpenedPath);          // the private copy was not even opened
        Assert.Empty(writer.Ledger);             // nothing was written
        var after = Directory.EnumerateFileSystemEntries(t.Path, "*", SearchOption.AllDirectories).OrderBy(x => x, StringComparer.Ordinal).Select(f => (f, File.Exists(f) ? Support.Sha(f) : "")).ToArray();
        Assert.Equal(before, after);
    }

    [Fact]
    public void The_designation_takes_the_declared_set_as_a_plain_list_and_needs_no_generated_manifest()
    {
        using var t = new TempDir();
        var d = RunnersTestsSupport.Designation(t.Path);
        var parsed = RunDesignation.Parse(RunnersTestsSupport.DesignationJson(d));
        Assert.Equal(new[] { "I52Ct21d.HostFacts.R0.dll", "I52Ct21d.HostFacts.Core.dll" }, parsed.DeclaredSet!.Select(f => f.Path).ToArray());
        var node = (JsonObject)Jcs.ParseStrict(RunnersTestsSupport.DesignationJson(d));
        node.Remove("declaredSet");
        Assert.Throws<FormatException>(() => RunDesignation.Parse(Jcs.Serialize(node)));          // required
        node["declaredSet"] = new JsonArray(new JsonObject { ["path"] = "x.dll", ["sha256"] = "zz" });
        Assert.Throws<FormatException>(() => RunDesignation.Parse(Jcs.Serialize(node)));          // each entry is {path, sha256}
        // nothing in the designation schema or the core refers to a manifest generator of this folder
        var schema = new System.IO.StreamReader(typeof(RunDesignation).Assembly.GetManifestResourceStream("schemas/ct21d.designation.v1.json")!).ReadToEnd();
        Assert.DoesNotContain("package-manifest.v1", schema);
    }
}
