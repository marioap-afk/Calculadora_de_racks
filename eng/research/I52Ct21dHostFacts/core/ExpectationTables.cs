namespace I52Ct21d.HostFacts.Core;

/// <summary>Host type names are compared by their simple form: <c>System.Int16</c> -> <c>Int16</c>, <c>System.Byte[]</c> -> <c>byte[]</c>.</summary>
public static class HostTypeName
{
    public static string Simple(string fullName)
    {
        var dot = fullName.LastIndexOf('.');
        var simple = dot >= 0 ? fullName[(dot + 1)..] : fullName;
        return simple == "Byte[]" ? "byte[]" : simple;
    }
}

/// <summary>One code range of BA-05 V5 section 2.1.2 and the host type(s) its stored value must have.</summary>
public sealed record RbRange(int Low, int High, string Kind, IReadOnlyList<string> HostTypes)
{
    public string Id => Low + "-" + High;

    public bool Contains(int code) => code >= Low && code <= High;
}

/// <summary>
/// The 32 ranges of the table of BA-05 V5 section 2.1.2 (V5:72-105), transcribed. The table is mechanically compared with the
/// BA-05 document by a test (design 5.4, I-2 / I-3 row), so a transcription error fails the build of the tests.
/// This is an EXPECTATION to be confirmed by the host (HF-G1); it decides nothing.
/// </summary>
public static class RbTypeTable
{
    private static readonly string[] Ints = { "Int16", "Int32", "Int64" };
    private static readonly string[] Str = { "String" };
    private static readonly string[] Pt = { "Point3d" };
    private static readonly string[] Dbl = { "Double" };
    private static readonly string[] Bool = { "Boolean" };
    private static readonly string[] Bytes = { "byte[]" };

    public static readonly IReadOnlyList<RbRange> Ranges = new[]
    {
        new RbRange(0, 4, "string", Str),
        new RbRange(6, 9, "string", Str),
        new RbRange(10, 18, "point3d", Pt),
        new RbRange(19, 59, "double", Dbl),
        new RbRange(60, 79, "int", Ints),
        new RbRange(90, 99, "int", Ints),
        new RbRange(100, 100, "string", Str),
        new RbRange(102, 102, "string", Str),
        new RbRange(110, 112, "point3d", Pt),
        new RbRange(113, 149, "double", Dbl),
        new RbRange(160, 169, "int", Ints),
        new RbRange(170, 179, "int", Ints),
        new RbRange(210, 210, "point3d", Pt),
        new RbRange(211, 239, "double", Dbl),
        new RbRange(270, 289, "int", Ints),
        new RbRange(290, 299, "bool", Bool),
        new RbRange(300, 309, "string", Str),
        new RbRange(310, 319, "bytes", Bytes),
        new RbRange(370, 389, "int", Ints),
        new RbRange(400, 409, "int", Ints),
        new RbRange(410, 419, "string", Str),
        new RbRange(420, 429, "int", Ints),
        new RbRange(430, 439, "string", Str),
        new RbRange(440, 459, "int", Ints),
        new RbRange(460, 469, "double", Dbl),
        new RbRange(470, 479, "string", Str),
        new RbRange(999, 1003, "string", Str),
        new RbRange(1004, 1004, "bytes", Bytes),
        new RbRange(1006, 1009, "string", Str),
        new RbRange(1010, 1013, "point3d", Pt),
        new RbRange(1014, 1059, "double", Dbl),
        new RbRange(1060, 1071, "int", Ints),
    };

    public static RbRange? Find(int code) => Ranges.FirstOrDefault(r => r.Contains(code));
}

/// <summary>One stored result-buffer entry as the host returned it: the group code and the full name of the runtime type of its value.</summary>
public sealed record StoredEntry(int Code, string HostTypeFullName);

public sealed record RbHostTypeCount(string HostType, int Count);

public sealed record RbRangeResult(
    RbRange Range,
    IReadOnlyList<int> CodesSeen,
    IReadOnlyList<RbHostTypeCount> HostTypes,
    int Count,
    string Status);

public sealed record RbOutsideTableResult(int Code, IReadOnlyList<RbHostTypeCount> HostTypes, int Count);

public sealed record RbTally(IReadOnlyList<RbRangeResult> Ranges, IReadOnlyList<RbOutsideTableResult> OutsideTable);

/// <summary>
/// HF-G1 (M1): per range of the table, the host types seen in STORED data. A range with no occurrence is NOT_OBSERVED (never filled
/// by a guess and never confirmed). A code outside the table is recorded apart: it is neither confirmed nor corrected here.
/// </summary>
public static class RbTallyBuilder
{
    public static RbTally Build(IEnumerable<StoredEntry> entries)
    {
        var perRange = new Dictionary<string, SortedDictionary<string, int>>(StringComparer.Ordinal);
        var codes = new Dictionary<string, SortedSet<int>>(StringComparer.Ordinal);
        var outside = new SortedDictionary<int, SortedDictionary<string, int>>();

        foreach (var e in entries)
        {
            var range = RbTypeTable.Find(e.Code);
            if (range is null)
            {
                if (!outside.TryGetValue(e.Code, out var o)) outside[e.Code] = o = new SortedDictionary<string, int>(StringComparer.Ordinal);
                o[e.HostTypeFullName] = o.GetValueOrDefault(e.HostTypeFullName) + 1;
                continue;
            }
            if (!perRange.TryGetValue(range.Id, out var t)) perRange[range.Id] = t = new SortedDictionary<string, int>(StringComparer.Ordinal);
            t[e.HostTypeFullName] = t.GetValueOrDefault(e.HostTypeFullName) + 1;
            if (!codes.TryGetValue(range.Id, out var cs)) codes[range.Id] = cs = new SortedSet<int>();
            cs.Add(e.Code);
        }

        var results = new List<RbRangeResult>();
        foreach (var range in RbTypeTable.Ranges)
        {
            if (!perRange.TryGetValue(range.Id, out var types))
            {
                results.Add(new RbRangeResult(range, Array.Empty<int>(), Array.Empty<RbHostTypeCount>(), 0, "NOT_OBSERVED"));
                continue;
            }
            var counts = types.Select(p => new RbHostTypeCount(p.Key, p.Value)).ToList();
            var total = counts.Sum(c => c.Count);
            var allExpected = counts.All(c => range.HostTypes.Contains(HostTypeName.Simple(c.HostType), StringComparer.Ordinal));
            results.Add(new RbRangeResult(range, codes[range.Id].ToList(), counts, total, allExpected ? "OBSERVED" : "OBSERVED_DIFFERS"));
        }

        var outsideResults = outside.Select(p =>
            new RbOutsideTableResult(p.Key, p.Value.Select(x => new RbHostTypeCount(x.Key, x.Value)).ToList(), p.Value.Values.Sum())).ToList();
        return new RbTally(results, outsideResults);
    }
}

/// <summary>An expected context-variable read of BA-05 V5 section 2.1.3 (V5:127-138).</summary>
public sealed record ContextVariable(string Name, string ExpectedHostType, string Kind);

/// <summary>
/// The expectation table of BA-05 V5 section 2.1.3: the host type that <c>Application.GetSystemVariable(name)</c> is expected to
/// return. It is an EXPECTATION confirmed by HF-G3 on the exact build; a different observed type is OBSERVED_DIFFERS, not an error.
/// Mechanically compared with the BA-05 document by a test.
/// </summary>
public static class ContextVariableTable
{
    public static readonly IReadOnlyList<ContextVariable> Variables = new[]
    {
        new ContextVariable("CLAYER", "System.String", "STRING"),
        new ContextVariable("CECOLOR", "System.String", "STRING"),
        new ContextVariable("CELTYPE", "System.String", "STRING"),
        new ContextVariable("CELTSCALE", "System.Double", "DOUBLE"),
        new ContextVariable("CELWEIGHT", "System.Int16", "INT"),
        new ContextVariable("CETRANSPARENCY", "System.String", "STRING"),
        new ContextVariable("CPLOTSTYLE", "System.String", "STRING"),
        new ContextVariable("TEXTSTYLE", "System.String", "STRING"),
        new ContextVariable("DIMSTYLE", "System.String", "STRING"),
        new ContextVariable("PSTYLEMODE", "System.Int16", "INT"),
    };

    public static ContextVariable? Find(string name) => Variables.FirstOrDefault(v => string.Equals(v.Name, name, StringComparison.Ordinal));
}
