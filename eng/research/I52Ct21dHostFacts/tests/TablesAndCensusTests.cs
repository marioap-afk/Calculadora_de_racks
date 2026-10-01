using System.Text.Json.Nodes;
using I52Ct21d.HostFacts.Core;
using Xunit;

namespace I52Ct21d.HostFacts.Tests;

/// <summary>
/// The expectation tables are compared MECHANICALLY with the BA-05 V5 document (design 5.4: "parse the markdown, compare ranges and
/// types"), and the census builder is driven by a fake reader behind the interface (no AutoCAD).
/// </summary>
public class TablesAndCensusTests
{
    private static string[] Ba05Lines() => File.ReadAllLines(Path.Combine(Support.RepoRoot(), "docs", "initiatives", "I-52-ct21d-baseline-ba-05-fingerprint-specification-v5.md"));

    private static List<string[]> TableRows(string[] lines, string headerStart)
    {
        var h = Array.FindIndex(lines, l => l.StartsWith(headerStart, StringComparison.Ordinal));
        Assert.True(h >= 0, "table header not found: " + headerStart);
        var rows = new List<string[]>();
        for (var i = h + 2; i < lines.Length && lines[i].StartsWith('|'); i++)
            rows.Add(lines[i].Trim().Trim('|').Split('|').Select(c => c.Trim()).ToArray());
        return rows;
    }

    [Fact]
    public void The_RB_range_table_equals_the_table_of_BA05_V5_section_2_1_2()
    {
        var rows = TableRows(Ba05Lines(), "| Codes | Kind | Host type of the value |");
        Assert.Equal(32, rows.Count);
        Assert.Equal(rows.Count, RbTypeTable.Ranges.Count);
        for (var i = 0; i < rows.Count; i++)
        {
            var code = rows[i][0].Split('-');
            var expectedTypes = rows[i][2].Replace(" or ", ", ").Split(", ");
            var r = RbTypeTable.Ranges[i];
            Assert.Equal((int.Parse(code[0]), int.Parse(code[1]), rows[i][1]), (r.Low, r.High, r.Kind));
            Assert.Equal(expectedTypes, r.HostTypes);
        }
    }

    [Fact]
    public void The_context_variable_table_equals_the_table_of_BA05_V5_section_2_1_3()
    {
        var rows = TableRows(Ba05Lines(), "| Variable | Expected host type of the read |");
        Assert.Equal(10, rows.Count);
        Assert.Equal(rows.Count, ContextVariableTable.Variables.Count);
        for (var i = 0; i < rows.Count; i++)
        {
            var v = ContextVariableTable.Variables[i];
            Assert.Equal(rows[i][0].Trim('`'), v.Name);
            Assert.Equal(rows[i][1].Trim('`'), v.ExpectedHostType);
            Assert.Equal(rows[i][2].Trim('`'), v.Kind);
        }
    }

    [Fact]
    public void The_known_class_list_equals_the_closed_map_of_BA05_V5_section_2_3_1()
    {
        var rows = TableRows(Ba05Lines(), "| Host class (`RXClass` name) |");
        Assert.Equal(rows.Select(r => r[0].Trim('`')).ToArray(), CensusBuilder.KnownClasses.ToArray());
    }

    [Theory]
    [InlineData(0, "0-4")]
    [InlineData(4, "0-4")]
    [InlineData(70, "60-79")]
    [InlineData(1071, "1060-1071")]
    [InlineData(1004, "1004-1004")]
    public void Codes_map_to_their_range(int code, string range) => Assert.Equal(range, RbTypeTable.Find(code)!.Id);

    [Theory]
    [InlineData(5)] [InlineData(105)] [InlineData(320)] [InlineData(1005)] [InlineData(1072)] [InlineData(5000)]
    public void Handle_and_unlisted_codes_have_no_range(int code) => Assert.Null(RbTypeTable.Find(code));

    [Fact]
    public void Host_type_names_are_compared_by_their_simple_form()
    {
        Assert.Equal("Int16", HostTypeName.Simple("System.Int16"));
        Assert.Equal("byte[]", HostTypeName.Simple("System.Byte[]"));
        Assert.Equal("Point3d", HostTypeName.Simple("Autodesk.AutoCAD.Geometry.Point3d"));
    }

    [Fact]
    public void RB_tally_reports_not_observed_for_ranges_without_data_and_differs_for_a_wrong_type()
    {
        var tally = RbTallyBuilder.Build(RunnersTestsSupport.Stored().Entries);
        Assert.Equal(32, tally.Ranges.Count);
        string Status(string id) => tally.Ranges.Single(r => r.Range.Id == id).Status;
        Assert.Equal("OBSERVED", Status("0-4"));
        Assert.Equal("OBSERVED", Status("60-79"));            // Int16 and Int32 are both admitted
        Assert.Equal("OBSERVED", Status("19-59"));
        Assert.Equal("OBSERVED", Status("10-18"));
        Assert.Equal("OBSERVED_DIFFERS", Status("290-299"));   // the table says Boolean
        Assert.Equal("NOT_OBSERVED", Status("310-319"));
        Assert.Equal(5, tally.Ranges.Count(r => r.Status != "NOT_OBSERVED")); // 0-4, 10-18, 19-59, 60-79, 290-299
        var r0 = tally.Ranges.Single(r => r.Range.Id == "0-4");
        Assert.Equal(new[] { 1 }, r0.CodesSeen);
        Assert.Equal(2, r0.Count);
        var outside = Assert.Single(tally.OutsideTable);
        Assert.Equal(5, outside.Code);
    }

    // ---- census over a fake drawing reader ------------------------------------------------------------------------------
    private static (JsonObject Body, CensusSummary Summary) Census(IReadOnlyList<BlockRecordInfo>? blocks = null, StoredBufferScan? stored = null) =>
        CensusBuilder.Build(blocks ?? Samples.Library(), stored ?? new StoredBufferScan(Array.Empty<StoredEntry>(), 0, 0), new string('a', 64), "D:\\lib.dwg", RunnersTestsSupport.Instrument());

    [Fact]
    public void Census_builds_blocks_in_host_order_with_explicit_index_and_counts()
    {
        var (body, s) = Census();
        var blocks = (JsonArray)body["blocks"]!;
        Assert.Equal(5, blocks.Count);
        Assert.Equal(new[] { "*Model_Space", "*Paper_Space", "RACK_A", "PIECE", "*U12" }, blocks.Select(b => b!["blockName"]!.GetValue<string>()).ToArray());
        Assert.Equal(Enumerable.Range(0, 5), blocks.Select(b => b!["index"]!.GetValue<int>()));
        var rack = blocks[2]!;
        var classes = ((JsonArray)rack["entityClasses"]!).Select(c => (c!["rxClassName"]!.GetValue<string>(), c["count"]!.GetValue<int>())).ToArray();
        Assert.Equal(new[] { ("AcDbLine", 3), ("AcDbBlockReference", 2), ("AcDbRotatedDimension", 1), ("AcDbText", 1), ("AcDbSolid", 1) }, classes);
        Assert.Equal(new[] { "PIECE" }, ((JsonArray)rack["nestedBlocks"]!).Select(x => x!.GetValue<string>()).ToArray());
        Assert.Equal(new[] { "STD" }, ((JsonArray)rack["symbolRecords"]!["dimStyles"]!).Select(x => x!.GetValue<string>()).ToArray());
        Assert.Equal(new[] { "0", "TEXT" }, ((JsonArray)rack["symbolRecords"]!["layers"]!).Select(x => x!.GetValue<string>()).ToArray());
        Assert.Equal(new[] { "Standard" }, ((JsonArray)rack["symbolRecords"]!["textStyles"]!).Select(x => x!.GetValue<string>()).ToArray());
        Assert.False(rack["referencesAnonymousStatic"]!.GetValue<bool>());
        Assert.True(blocks[4]!["referencesAnonymousStatic"]!.GetValue<bool>());
        Assert.Equal((5, 2, 2, 1, 0), (s.Blocks, s.NamedBlocks, s.LayoutBlocks, s.AnonymousBlocks, s.BlocksUnknown));
        Assert.Equal(1, s.BlocksReferencingAnonymousStatic);
        Assert.Equal(12, s.Entities);
    }

    [Fact]
    public void Census_keeps_the_dstyle_pairs_in_host_order_with_code_and_host_type_only()
    {
        var (body, s) = Census();
        var o = (JsonObject)((JsonArray)body["blocks"]!)[2]!["dimensionOverrides"]![0]!;
        Assert.Equal(5, o["entityIndex"]!.GetValue<int>());
        Assert.True(o["hasDstyleSection"]!.GetValue<bool>());
        var pairs = ((JsonArray)o["pairs"]!).Select(p => (p!["index"]!.GetValue<int>(), p["code"]!.GetValue<int>(), p["hostValueType"]!.GetValue<string>())).ToArray();
        Assert.Equal(new[] { (0, 40, "System.Double"), (1, 41, "System.Double"), (2, 77, "System.Int16") }, pairs);
        Assert.False(((JsonObject)((JsonArray)o["pairs"]!)[0]!).ContainsKey("variable")); // a code and a type, never a variable name (HF-C5 is NOT_OBSERVABLE here)
        Assert.Equal((1, 1, 0, 3), (s.Dimensions, s.DimensionsWithDstyle, s.DimensionsMalformed, s.DstylePairs));
    }

    [Fact]
    public void Census_class_universe_is_sorted_and_classes_without_a_table_are_marked_named_or_layout_only()
    {
        var (body, s) = Census();
        var universe = ((JsonArray)body["classUniverse"]!).Select(c => c!["rxClassName"]!.GetValue<string>()).ToArray();
        Assert.Equal(universe.OrderBy(x => x, StringComparer.Ordinal).ToArray(), universe);
        Assert.Contains("AcDbViewport", universe);
        var without = ((JsonArray)body["classesWithoutTable"]!).ToDictionary(c => c!["rxClassName"]!.GetValue<string>(), c => c!["marking"]!.GetValue<string>());
        Assert.Equal(new Dictionary<string, string> { ["AcDbViewport"] = "anonymousOrLayoutOnly", ["AcDbSolid"] = "named" }, without);
        Assert.Equal(2, s.ClassesWithoutTable);
        // a class seen in BOTH a layout and a named block is "named" (it gets a table)
        var both = new[]
        {
            Samples.Block("*Paper_Space", new[] { Samples.Entity("AcDbHatch", "HATCH") }, layout: true),
            Samples.Block("N", new[] { Samples.Entity("AcDbHatch", "HATCH") }),
        };
        var (b2, _) = Census(both);
        Assert.Equal("named", b2["classesWithoutTable"]![0]!["marking"]!.GetValue<string>());
    }

    [Fact]
    public void Census_marks_a_block_with_an_unreadable_entity_unknown_and_counts_it()
    {
        var blocks = new[] { Samples.Block("A", new[] { Samples.Entity("AcDbLine", "LINE"), Samples.Entity("", "", error: "ENTITY_UNREADABLE:X") }) };
        var (body, s) = Census(blocks);
        Assert.Equal("UNKNOWN", body["blocks"]![0]!["status"]!.GetValue<string>());
        Assert.Equal("ENTITY_UNREADABLE:X", body["blocks"]![0]!["reason"]!.GetValue<string>());
        Assert.Equal(1, s.BlocksUnknown);
        Assert.Single((JsonArray)body["classUniverse"]!); // the unreadable entity contributes no class
    }

    [Fact]
    public void Census_records_proxy_or_custom_presence_and_the_stored_buffer_counts()
    {
        var blocks = new[] { Samples.Block("A", new[] { Samples.Entity("AcDbProxyEntity", "ACAD_PROXY_ENTITY", proxy: true) }) };
        var (body, s) = Census(blocks, RunnersTestsSupport.Stored());
        Assert.True(body["blocks"]![0]!["containsProxyOrCustom"]!.GetValue<bool>());
        Assert.Equal(1, s.BlocksWithProxyOrCustom);
        Assert.Equal(4, body["counts"]!["storedBuffersRead"]!.GetValue<int>());
        Assert.Equal(1, body["counts"]!["storedBufferReadFailures"]!.GetValue<int>());
        Assert.Equal(8, body["counts"]!["storedBufferEntries"]!.GetValue<int>());
        var outside = (JsonArray)body["rbOutsideTable"]!;
        Assert.Equal(5, outside[0]!["code"]!.GetValue<int>());
    }
}
