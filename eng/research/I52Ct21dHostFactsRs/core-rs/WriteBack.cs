using System.Globalization;
using System.Text.Json.Nodes;
using I52Ct21d.HostFacts.Core;

namespace I52Ct21d.HostFacts.Rs.Core;

/// <summary>One dimension variable the product writes (BA-05 V5:1115): its name, the DXF group code BA-05 states for it, and its host kind.</summary>
public sealed class OverrideSpec
{
    public string Variable { get; }
    public int Ba05Code { get; }
    public bool IsInt16 { get; }

    public OverrideSpec(string variable, int ba05Code, bool isInt16)
    {
        Variable = variable;
        Ba05Code = ba05Code;
        IsInt16 = isInt16;
    }
}

/// <summary>
/// The eight dimension variables that <c>LateralHeaderDrawer.AppendDimension</c> assigns (<c>LateralHeaderDrawer.cs:362-372</c> at <c>3375aadb</c>),
/// in the product's assignment order, with the group codes BA-05 V5:1115 states ("to be confirmed by the same step") and the values the product
/// assigns for a text height of 3.0. A change here is a change of the test, not of a tolerance.
/// </summary>
public static class WriteBackSpecs
{
    public const double ProductTextHeight = 3.0;

    /// <summary>Product assignment order: Dimscale, Dimtxt, Dimasz, Dimexe, Dimexo, Dimgap, Dimtad, Dimdec.</summary>
    public static readonly IReadOnlyList<OverrideSpec> Product = new[]
    {
        new OverrideSpec("DIMSCALE", 40, false), new OverrideSpec("DIMTXT", 140, false), new OverrideSpec("DIMASZ", 41, false),
        new OverrideSpec("DIMEXE", 44, false), new OverrideSpec("DIMEXO", 42, false), new OverrideSpec("DIMGAP", 147, false),
        new OverrideSpec("DIMTAD", 77, true), new OverrideSpec("DIMDEC", 271, true),
    };

    /// <summary>The product's value for a variable: the arithmetic of the product source with a text height of 3.0.</summary>
    public static double ProductValue(string variable) => variable switch
    {
        "DIMSCALE" => 1.0,
        "DIMTXT" => ProductTextHeight,
        "DIMASZ" => ProductTextHeight * 0.7,
        "DIMEXE" => ProductTextHeight * 0.4,
        "DIMEXO" => ProductTextHeight * 0.4,
        "DIMGAP" => ProductTextHeight * 0.3,
        "DIMTAD" => 1,
        "DIMDEC" => 2,
        _ => throw new ArgumentException("not a product dimension variable: " + variable),
    };

    public static OverrideSpec Find(string variable) =>
        Product.FirstOrDefault(s => s.Variable == variable) ?? throw new ArgumentException("not a product dimension variable: " + variable);
}

/// <summary>The effective value of a variable on a fresh dimension (the style value), as the host returned it.</summary>
public sealed class StyleValue
{
    public string Variable { get; }
    public string HostTypeFullName { get; }
    public double NumericValue { get; }
    public string ValueText { get; }
    public string? ValueBitsHex { get; }

    public StyleValue(string variable, string hostTypeFullName, double numericValue, string valueText, string? valueBitsHex)
    {
        Variable = variable;
        HostTypeFullName = hostTypeFullName;
        NumericValue = numericValue;
        ValueText = valueText;
        ValueBitsHex = valueBitsHex;
    }
}

/// <summary>One assignment of a scenario: the variable and the value the setter receives.</summary>
public sealed class OverrideAssignment
{
    public string Variable { get; }
    public double Value { get; }

    public OverrideAssignment(string variable, double value)
    {
        Variable = variable;
        Value = value;
    }
}

public enum ScenarioKind
{
    None,
    Different,
    EqualToStyle,
    ProductOrder,
}

public sealed class WriteBackScenario
{
    public string Id { get; }
    public ScenarioKind Kind { get; }
    public string? Variable { get; }
    public IReadOnlyList<OverrideAssignment> Assignments { get; }

    public WriteBackScenario(string id, ScenarioKind kind, string? variable, IReadOnlyList<OverrideAssignment> assignments)
    {
        Id = id;
        Kind = kind;
        Variable = variable;
        Assignments = assignments;
    }
}

/// <summary>The probe plan: for each product variable once with a value DIFFERENT from the style and once EQUAL to it, plus the product-order scenario and a none scenario.</summary>
public static class WriteBackPlan
{
    public static IReadOnlyList<WriteBackScenario> Build(IReadOnlyList<StyleValue> style)
    {
        var scenarios = new List<WriteBackScenario> { new("WB-NONE", ScenarioKind.None, null, Array.Empty<OverrideAssignment>()) };
        foreach (var spec in WriteBackSpecs.Product)
        {
            var s = style.FirstOrDefault(x => x.Variable == spec.Variable)
                    ?? throw new ArgumentException("no style value for " + spec.Variable);
            var different = WriteBackSpecs.ProductValue(spec.Variable);
            if (different == s.NumericValue) different += 1.0;
            scenarios.Add(new WriteBackScenario("WB-DIFF-" + spec.Variable, ScenarioKind.Different, spec.Variable, new[] { new OverrideAssignment(spec.Variable, different) }));
            scenarios.Add(new WriteBackScenario("WB-EQ-" + spec.Variable, ScenarioKind.EqualToStyle, spec.Variable, new[] { new OverrideAssignment(spec.Variable, s.NumericValue) }));
        }
        scenarios.Add(new WriteBackScenario("WB-PRODUCT-ORDER", ScenarioKind.ProductOrder, null,
            WriteBackSpecs.Product.Select(p => new OverrideAssignment(p.Variable, WriteBackSpecs.ProductValue(p.Variable))).ToList()));
        return scenarios;
    }
}

/// <summary>One typed value of the <c>ACAD</c> extended data inside the <c>DSTYLE</c> section, as the host returned it.</summary>
public sealed class StoredEntryReading
{
    public int TypeCode { get; }
    public string HostTypeFullName { get; }
    public string ValueText { get; }
    public string? ValueBitsHex { get; }

    public StoredEntryReading(int typeCode, string hostTypeFullName, string valueText, string? valueBitsHex)
    {
        TypeCode = typeCode;
        HostTypeFullName = hostTypeFullName;
        ValueText = valueText;
        ValueBitsHex = valueBitsHex;
    }
}

public sealed class DimensionReading
{
    public string Handle { get; }
    public bool HasAcadXData { get; }
    public bool HasDstyleSection { get; }
    public bool WellFormed { get; }
    public IReadOnlyList<StoredEntryReading> Entries { get; }
    public string? ReadError { get; }

    public DimensionReading(string handle, bool hasAcadXData, bool hasDstyleSection, bool wellFormed, IReadOnlyList<StoredEntryReading> entries, string? readError)
    {
        Handle = handle;
        HasAcadXData = hasAcadXData;
        HasDstyleSection = hasDstyleSection;
        WellFormed = wellFormed;
        Entries = entries;
        ReadError = readError;
    }
}

/// <summary>A stored override: the group code that designates the variable, and the value entry that follows it (type code, host type, text, bits).</summary>
public sealed class StoredOverride
{
    public int Code { get; }
    public int ValueTypeCode { get; }
    public string ValueHostType { get; }
    public string ValueText { get; }
    public string? ValueBitsHex { get; }

    public StoredOverride(int code, int valueTypeCode, string valueHostType, string valueText, string? valueBitsHex)
    {
        Code = code;
        ValueTypeCode = valueTypeCode;
        ValueHostType = valueHostType;
        ValueText = valueText;
        ValueBitsHex = valueBitsHex;
    }
}

public sealed class ParsedDstyle
{
    public bool WellFormed { get; }
    public IReadOnlyList<StoredOverride> Overrides { get; }
    public string Problem { get; }

    public ParsedDstyle(bool wellFormed, IReadOnlyList<StoredOverride> overrides, string problem)
    {
        WellFormed = wellFormed;
        Overrides = overrides;
        Problem = problem;
    }
}

/// <summary>
/// Reads the entries of a <c>DSTYLE</c> section as a sequence of (group code, value) pairs in host order: the group code is an Int16 entry
/// (type code 1070) and the value is the entry that follows it (BA-05 V5 section 3.1.2, "the group code that precedes each value designates
/// its variable"). Anything else is malformed: the pairs are not guessed.
/// </summary>
public static class DstyleOverrideParser
{
    public static ParsedDstyle Parse(IReadOnlyList<StoredEntryReading> entries)
    {
        var result = new List<StoredOverride>();
        var i = 0;
        while (i < entries.Count)
        {
            var codeEntry = entries[i];
            if (codeEntry.TypeCode != 1070 || codeEntry.HostTypeFullName != "System.Int16")
                return new ParsedDstyle(false, result, "ENTRY_" + i + "_IS_NOT_AN_INT16_GROUP_CODE");
            if (!int.TryParse(codeEntry.ValueText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var code))
                return new ParsedDstyle(false, result, "ENTRY_" + i + "_GROUP_CODE_NOT_AN_INTEGER");
            i++;
            if (i >= entries.Count) return new ParsedDstyle(false, result, "GROUP_CODE_" + code + "_HAS_NO_VALUE");
            var value = entries[i];
            result.Add(new StoredOverride(code, value.TypeCode, value.HostTypeFullName, value.ValueText, value.ValueBitsHex));
            i++;
        }
        return new ParsedDstyle(true, result, "");
    }
}

public sealed class ScenarioConstruction
{
    public string ScenarioId { get; }
    public string? Handle { get; }
    public string? Rejection { get; }

    public ScenarioConstruction(string scenarioId, string? handle, string? rejection)
    {
        ScenarioId = scenarioId;
        Handle = handle;
        Rejection = rejection;
    }
}

public interface IWriteBackDatabase : IDisposable
{
    string Id { get; }

    /// <summary>The effective value of each product variable on a fresh <c>RotatedDimension</c> of the drawing's current style.</summary>
    IReadOnlyList<StyleValue> ReadStyleValues();

    /// <summary>T1: one <c>RotatedDimension</c> per scenario, its variables set by the property setters the product uses (and <c>RecomputeDimensionBlock(true)</c> as the product does), COMMITTED.</summary>
    IReadOnlyList<ScenarioConstruction> AppendScenarios(IReadOnlyList<WriteBackScenario> scenarios);

    /// <summary>T2 / T3: reads the <c>ACAD</c> extended data (the <c>DSTYLE</c> section) of every dimension of model space, in a transaction that is never committed.</summary>
    IReadOnlyList<DimensionReading> ReadDimensions();

    ScratchFileEntry SaveToScratch(ScratchTarget target);

    IWriteBackDatabase Reopen(ReadableSource source);
}

public interface IWriteBackHost
{
    IWriteBackDatabase CreateSideDatabase();
}
