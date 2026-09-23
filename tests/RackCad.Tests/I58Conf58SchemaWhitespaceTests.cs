using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using RackCad.Application.Systems.Shared;
using Xunit;
using static RackCad.Tests.I58F1Fixtures;
using static RackCad.Tests.I58F1Oracles;
using O = RackCad.Application.Systems.Shared.RackAuthoredComparisonOutcome;

namespace RackCad.Tests;

public sealed record I58SchemaBoundary(string Kind, string Path, string Version = "1.0",
    bool Envelope = false, bool Flat = false, int Global = 0, bool Calculated = true)
{
    public override string ToString() => $"{Kind}/{(Envelope ? "envelope" : Flat ? "flat" : "project")}/{Path}/{Version}/g{Global}/calc{Calculated}";
}

public sealed class I58Conf58SchemaWhitespaceTests
{
    public static IEnumerable<I58SchemaBoundary> Boundaries()
    {
        foreach (string kind in Kinds)
        {
            yield return new(kind, "SchemaVersion", Envelope: true);
            yield return new(kind, "SchemaVersion");
            yield return new(kind, "SchemaVersion", "2.0");
        }
        yield return new("pushback", "PushBack.SchemaVersion");
        yield return new("cantilever", "Cantilever.SchemaVersion");
        yield return new("cabecera", "Header.SchemaVersion");
        yield return new("cabecera", "SchemaVersion", Flat: true);
        foreach (string kind in new[] { "dynamic", "pushback" })
        foreach (int global in new[] { 0, 5 })
        {
            foreach (bool calculated in new[] { true, false })
                yield return new(kind, Structure(kind) + ".Modules.0.Header.SchemaVersion", Global: global, Calculated: calculated);
            yield return new(kind, Structure(kind) + ".HeaderLineOverrides.0.Header.SchemaVersion", Global: global);
        }
    }

    public static IEnumerable<object[]> WhitespaceCases() => Boundaries().SelectMany(b =>
        new[] { "spaces", "tabs-newlines", "unicode-whitespace" }.Select(p => new object[] { b, p }));

    public static IEnumerable<object[]> InvalidCases() => Boundaries().SelectMany(b => new[] {
        "\"\"", "\" \\t\\r\\n \"", "\"1.1\"", "\" 1.1 \"", "\"2.1\"", "\" 2.1 \"", "\"99.0\"",
        "\"+1.0\"", "\"-1.0\"", "\"1.0.0\"", "\"999999999999999999999999.0\"", "1.0", "{}", "[]"
    }.Select(v => new object[] { b, v }));

    [Theory, MemberData(nameof(WhitespaceCases))]
    public void Known_schema_with_exterior_whitespace_is_Single(I58SchemaBoundary boundary, string padding)
    {
        string padded = padding switch {
            "spaces" => " " + boundary.Version + " ",
            "tabs-newlines" => "\t" + boundary.Version + "\r\n",
            _ => "\u00a0" + boundary.Version + "\u2003"
        };
        var a = AtVersion(boundary, JsonSerializer.Serialize(boundary.Version), "A");
        var b = AtVersion(boundary, JsonSerializer.Serialize(padded), "B");
        Assert.Equal(O.Single, Compare(boundary.Kind, a, a with { SourceIdentity = "control" }).Outcome);
        // The only changed input is this SchemaVersion. This assertion must be RED before CONF58-01.
        Assert.Equal(O.Single, Compare(boundary.Kind, a, b).Outcome);
        Assert.Equal(O.Single, Compare(boundary.Kind, b, a).Outcome);
    }

    [Theory, MemberData(nameof(InvalidCases))]
    public void Blank_invalid_and_trimmed_future_schemas_remain_Unreadable(I58SchemaBoundary boundary, string rawVersion)
    {
        var a = AtVersion(boundary, JsonSerializer.Serialize(boundary.Version), "A");
        var b = AtVersion(boundary, rawVersion, "B");
        Assert.Equal(O.Single, Compare(boundary.Kind, a, a with { SourceIdentity = "control" }).Outcome);
        foreach (var siblings in new[] { new[] { a, b }, new[] { b, a } })
        {
            var result = Compare(boundary.Kind, siblings);
            Assert.Equal(O.Unreadable, result.Outcome);
            Assert.Contains("SchemaVersion", result.Diagnostic);
            Assert.Contains("unrecognized schema", result.Diagnostic);
        }
    }

    [Theory]
    [InlineData("dynamic")]
    [InlineData("pushback")]
    [InlineData("cantilever")]
    [InlineData("cabecera")]
    public void Schema_trim_does_not_normalize_authored_names(string kind)
    {
        var a = Sibling(kind, Rich(kind), "A");
        var b = a with { SourceIdentity = "B", RawEnvelope = Set(a.RawEnvelope, "Name", "\" Rack F1 \"") };
        Assert.Equal(O.Divergent, Compare(kind, a, b).Outcome);
    }

    public static IEnumerable<object[]> PrecedenceCases() => I58F1Matrix.Build()
        .Where(c => c.Rows.Intersect(new[] { "CT58-06", "CT58-07", "CT58-08" }).Any())
        .Select(c => new object[] { c });

    [Theory, MemberData(nameof(PrecedenceCases))]
    public void CT58_06_07_08_keep_their_existing_oracles(I58F1Case c) => AssertFuture(c);

    private static I58F1Sibling AtVersion(I58SchemaBoundary boundary, string rawVersion, string source)
    {
        string raw = Rich(boundary.Kind);
        if (boundary.Kind is "dynamic" or "pushback")
        {
            string structure = Structure(boundary.Kind);
            raw = Set(raw, structure + ".PostPeralte", boundary.Global.ToString(System.Globalization.CultureInfo.InvariantCulture));
            raw = Set(raw, structure + ".Modules.0.UseCalculatedHeaderConfiguration", boundary.Calculated ? "true" : "false");
        }
        if (boundary.Flat)
        {
            using var document = JsonDocument.Parse(raw);
            raw = document.RootElement.GetProperty("Header").GetRawText();
        }
        if (!boundary.Envelope) raw = Set(raw, boundary.Path, rawVersion);
        var sibling = Sibling(boundary.Kind, raw, source);
        return boundary.Envelope ? sibling with { RawEnvelope = Set(sibling.RawEnvelope, boundary.Path, rawVersion) } : sibling;
    }

    private static (O Outcome, string Diagnostic) Compare(string kind, params I58F1Sibling[] siblings)
    {
        var input = ProductInput(Input(siblings), kind);
        return kind switch {
            "dynamic" => Result(RackAuthoredComparatorPorts.Dynamic().Compare(input)),
            "pushback" => Result(RackAuthoredComparatorPorts.PushBack().Compare(input)),
            "cantilever" => Result(RackAuthoredComparatorPorts.Cantilever().Compare(input)),
            "cabecera" => Result(RackAuthoredComparatorPorts.Cabecera().Compare(input)),
            _ => throw new ArgumentException(kind)
        };
    }

    private static (O, string) Result<T>(RackAuthoredComparisonResult<T> result)
    {
        if (result.Outcome == O.Single) Assert.NotNull(result.Authored);
        else Assert.Null(result.Authored);
        return (result.Outcome, result.Diagnostic);
    }
}
