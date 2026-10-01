using System.Text.Json.Nodes;

namespace I52Ct21d.HostFacts.Core;

/// <summary>Numbers and flags of a census that the fact records summarize.</summary>
public sealed record CensusSummary(
    int Blocks,
    int NamedBlocks,
    int LayoutBlocks,
    int AnonymousBlocks,
    int BlocksUnknown,
    int Entities,
    int DistinctClasses,
    int ClassesWithoutTable,
    int BlocksWithProxyOrCustom,
    int BlocksReferencingAnonymousStatic,
    int Dimensions,
    int DimensionsWithDstyle,
    int DimensionsMalformed,
    int DstylePairs,
    RbTally Rb);

/// <summary>
/// Builds the census record of BA-05 V5 section 5 (R0 part: HF-C1, HF-C3 and HF-G1 M1) from an abstract reader.
/// The dynamic-property part (HF-C2) is RS (blocked by Q-O-1) and is NOT in this record: <c>dynamicPropertyMethod</c> is the
/// constant <c>NOT_READ_BY_R0</c>.
/// </summary>
public static class CensusBuilder
{
    /// <summary>The 13 host classes of the closed map of BA-05 V5 section 2.3.1 (compared with the document by a test).</summary>
    public static readonly IReadOnlyList<string> KnownClasses = new[]
    {
        "AcDbLine", "AcDbArc", "AcDbCircle", "AcDbPolyline", "AcDbText", "AcDbMText", "AcDbBlockReference",
        "AcDbAttributeDefinition", "AcDbRotatedDimension", "AcDbAlignedDimension", "AcDbAttribute", "AcDbXrecord", "AcDbDictionary",
    };

    public static readonly IReadOnlyList<string> DimensionClasses = new[] { "AcDbRotatedDimension", "AcDbAlignedDimension" };

    public static (JsonObject Body, CensusSummary Summary) Build(
        IReadOnlyList<BlockRecordInfo> blocks,
        StoredBufferScan stored,
        string libraryFileSha256,
        string libraryPath,
        InstrumentInfo instrument)
    {
        var blockNodes = new List<JsonNode?>();
        var classDxf = new SortedDictionary<string, SortedSet<string>>(StringComparer.Ordinal);
        var classNamedBlock = new Dictionary<string, bool>(StringComparer.Ordinal);
        int unknownBlocks = 0, entities = 0, namedBlocks = 0, layoutBlocks = 0, anonBlocks = 0, proxyBlocks = 0, anonRefBlocks = 0;
        int dims = 0, dimsWith = 0, dimsBad = 0, pairs = 0;

        for (var bi = 0; bi < blocks.Count; bi++)
        {
            var b = blocks[bi];
            var firstError = b.ReadError ?? b.Entities.Select(e => e.ReadError).FirstOrDefault(x => x is not null);
            if (firstError is not null) unknownBlocks++;
            if (b.IsLayout) layoutBlocks++;
            if (b.IsAnonymous) anonBlocks++;
            if (!b.IsLayout && !b.IsAnonymous) namedBlocks++;
            var isNamedNonLayout = !b.IsLayout && !b.IsAnonymous;

            var classOrder = new List<(string Rx, string Dxf)>();
            var classCount = new Dictionary<(string, string), int>();
            var nested = new List<string>();
            var layers = new List<string>();
            var lts = new List<string>();
            var tss = new List<string>();
            var dss = new List<string>();
            var overrides = new List<JsonNode?>();
            var anonStatic = false;
            var proxy = false;

            for (var ei = 0; ei < b.Entities.Count; ei++)
            {
                var e = b.Entities[ei];
                entities++;
                if (e.ReadError is not null) continue;
                var key = (e.RxClassName, e.DxfName);
                if (!classCount.ContainsKey(key)) { classCount[key] = 0; classOrder.Add(key); }
                classCount[key]++;
                if (!classDxf.TryGetValue(e.RxClassName, out var ds)) classDxf[e.RxClassName] = ds = new SortedSet<string>(StringComparer.Ordinal);
                ds.Add(e.DxfName);
                classNamedBlock[e.RxClassName] = classNamedBlock.GetValueOrDefault(e.RxClassName) || isNamedNonLayout;
                if (e.ReferencedBlockName is not null && !nested.Contains(e.ReferencedBlockName)) nested.Add(e.ReferencedBlockName);
                if (e.ReferencesAnonymousNonDynamic) anonStatic = true;
                if (e.IsProxyOrCustom) proxy = true;
                AddDistinct(layers, e.Layer);
                AddDistinct(lts, e.Linetype);
                AddDistinct(tss, e.TextStyle);
                AddDistinct(dss, e.DimStyle);
                if (e.Dstyle is not null)
                {
                    dims++;
                    if (e.Dstyle.HasSection) dimsWith++;
                    if (e.Dstyle.HasSection && !e.Dstyle.WellFormed) dimsBad++;
                    pairs += e.Dstyle.Pairs.Count;
                    overrides.Add(new JsonObject
                    {
                        ["entityIndex"] = ei,
                        ["rxClassName"] = e.RxClassName,
                        ["hasDstyleSection"] = e.Dstyle.HasSection,
                        ["wellFormed"] = e.Dstyle.WellFormed,
                        ["pairs"] = RecordJson.Arr(e.Dstyle.Pairs.Select((p, pi) =>
                            (JsonNode?)new JsonObject { ["index"] = pi, ["code"] = p.Code, ["hostValueType"] = p.HostTypeFullName })),
                    });
                }
            }
            if (proxy) proxyBlocks++;
            if (anonStatic) anonRefBlocks++;

            blockNodes.Add(new JsonObject
            {
                ["index"] = bi,
                ["blockName"] = b.Name,
                ["isLayout"] = b.IsLayout,
                ["isAnonymous"] = b.IsAnonymous,
                ["isDynamic"] = b.IsDynamic,
                ["entityClasses"] = RecordJson.Arr(classOrder.Select(c =>
                    (JsonNode?)new JsonObject { ["rxClassName"] = c.Rx, ["dxfName"] = c.Dxf, ["count"] = classCount[c] })),
                ["nestedBlocks"] = RecordJson.Strings(nested),
                ["referencesAnonymousStatic"] = anonStatic,
                ["symbolRecords"] = new JsonObject
                {
                    ["layers"] = RecordJson.Strings(layers),
                    ["linetypes"] = RecordJson.Strings(lts),
                    ["textStyles"] = RecordJson.Strings(tss),
                    ["dimStyles"] = RecordJson.Strings(dss),
                },
                ["dimensionOverrides"] = RecordJson.Arr(overrides),
                ["containsProxyOrCustom"] = proxy,
                ["status"] = firstError is null ? "OBSERVED" : "UNKNOWN",
                ["reason"] = firstError ?? "",
            });
        }

        var universe = RecordJson.Arr(classDxf.Select(p =>
            (JsonNode?)new JsonObject { ["rxClassName"] = p.Key, ["dxfNames"] = RecordJson.Strings(p.Value) }));
        var withoutTable = classDxf.Keys.Where(k => !KnownClasses.Contains(k, StringComparer.Ordinal)).ToList();
        var withoutTableNodes = RecordJson.Arr(withoutTable.Select(k =>
            (JsonNode?)new JsonObject { ["rxClassName"] = k, ["marking"] = classNamedBlock[k] ? "named" : "anonymousOrLayoutOnly" }));

        var rb = RbTallyBuilder.Build(stored.Entries);
        var rbNodes = RecordJson.Arr(rb.Ranges.Select(r => (JsonNode?)new JsonObject
        {
            ["range"] = r.Range.Id,
            ["expectedKind"] = r.Range.Kind,
            ["expectedHostTypes"] = RecordJson.Strings(r.Range.HostTypes),
            ["codesSeen"] = RecordJson.Arr(r.CodesSeen.Select(c => (JsonNode?)JsonValue.Create(c))),
            ["hostTypes"] = HostTypes(r.HostTypes),
            ["count"] = r.Count,
            ["status"] = r.Status,
        }));
        var outsideNodes = RecordJson.Arr(rb.OutsideTable.Select(o => (JsonNode?)new JsonObject
        {
            ["code"] = o.Code,
            ["hostTypes"] = HostTypes(o.HostTypes),
            ["count"] = o.Count,
        }));

        var body = new JsonObject
        {
            ["schema"] = "ct21d.census.v1",
            ["governing"] = false,
            ["gate"] = RecordJson.Gate,
            ["identity"] = new JsonObject
            {
                ["libraryFileSha256"] = libraryFileSha256,
                ["libraryPath"] = libraryPath,
                ["instrumentBuild"] = new JsonObject { ["name"] = instrument.Name, ["sha256"] = instrument.Sha256 },
                ["dynamicPropertyMethod"] = "NOT_READ_BY_R0",
            },
            ["blocks"] = RecordJson.Arr(blockNodes),
            ["classUniverse"] = universe,
            ["classesWithoutTable"] = withoutTableNodes,
            ["rbTypes"] = rbNodes,
            ["rbOutsideTable"] = outsideNodes,
            ["counts"] = new JsonObject
            {
                ["blocks"] = blocks.Count,
                ["blocksUnknown"] = unknownBlocks,
                ["entities"] = entities,
                ["storedBuffersRead"] = stored.BuffersRead,
                ["storedBufferReadFailures"] = stored.ReadFailures,
                ["storedBufferEntries"] = stored.Entries.Count,
            },
        };
        var summary = new CensusSummary(blocks.Count, namedBlocks, layoutBlocks, anonBlocks, unknownBlocks, entities, classDxf.Count,
            withoutTable.Count, proxyBlocks, anonRefBlocks, dims, dimsWith, dimsBad, pairs, rb);
        return (body, summary);
    }

    private static JsonArray HostTypes(IEnumerable<RbHostTypeCount> counts) =>
        RecordJson.Arr(counts.Select(c => (JsonNode?)new JsonObject { ["hostType"] = c.HostType, ["count"] = c.Count }));

    private static void AddDistinct(List<string> list, string? value)
    {
        if (value is not null && !list.Contains(value)) list.Add(value);
    }
}
