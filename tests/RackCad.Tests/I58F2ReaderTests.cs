using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.Systems.Dynamic;
using Xunit;
using Xunit.Abstractions;
using static RackCad.Tests.I58F1Fixtures;
using static RackCad.Tests.I58F1Oracles;
using O = RackCad.Application.Systems.Shared.RackAuthoredComparisonOutcome;

namespace RackCad.Tests;

public sealed class I58F2ReaderTests
{
    private readonly ITestOutputHelper output;
    public I58F2ReaderTests(ITestOutputHelper output) => this.output = output;
    public static IEnumerable<object[]> Rejections() => I58F1Matrix.Build().Where(c => c.Expected == O.Unreadable).Select(c => new object[] { c });
    public static IEnumerable<object[]> ReaderControls() => Kinds.SelectMany(k => new[] { "root", "nested", "schema", "duplicate" }.Select(v => new object[] { k, v }));

    private static (O Outcome, string Diagnostic) Compare(string kind, I58F1Input input)
    {
        var p = ProductInput(input, kind);
        return kind switch
        {
            "dynamic" => Result(RackAuthoredComparatorPorts.Dynamic().Compare(p)),
            "pushback" => Result(RackAuthoredComparatorPorts.PushBack().Compare(p)),
            "cantilever" => Result(RackAuthoredComparatorPorts.Cantilever().Compare(p)),
            "cabecera" => Result(RackAuthoredComparatorPorts.Cabecera().Compare(p)),
            _ => throw new ArgumentException(kind)
        };
    }
    private static (O, string) Result<T>(RackAuthoredComparisonResult<T> r)
    {
        if (r.Outcome != O.Single) Assert.Null(r.Authored);
        else Assert.NotNull(r.Authored);
        return (r.Outcome, r.Diagnostic);
    }

    [Theory, MemberData(nameof(Rejections))]
    public void Former_vacuous_rejection_has_a_real_reader_cause_and_supported_control(I58F1Case c)
    {
        var bad = Compare(c.Kind, c.Input);
        Assert.Equal(O.Unreadable, bad.Outcome);
        Assert.Contains("AUTH-13", bad.Diagnostic);
        Assert.DoesNotContain("No authored comparator", bad.Diagnostic);
        string id = c.Id;
        string cause = id.Contains("unknown", StringComparison.Ordinal) || id.Contains("MM-P08", StringComparison.Ordinal) ||
            id.Contains("MM-H08", StringComparison.Ordinal) && !id.Contains("SchemaVersion", StringComparison.Ordinal) ||
            id.Contains("FutureMargin", StringComparison.Ordinal) || id.Contains(".Members/", StringComparison.Ordinal) ? "unknown member" :
            id.Contains("schema", StringComparison.Ordinal) || id.Contains("SchemaVersion", StringComparison.Ordinal) ? "schema" :
            id.Contains("duplicate", StringComparison.Ordinal) || id.Contains("case-collision", StringComparison.Ordinal) ? "duplicate" :
            id.Contains("future-enum", StringComparison.Ordinal) ? "unknown enum" :
            id.Contains("MM-P07", StringComparison.Ordinal) ? "PushBack.enum" :
            id.Contains("mixed-outer", StringComparison.Ordinal) || id.Contains("wrong-kind", StringComparison.Ordinal) ? "membership or kind mismatch" :
            id.Contains("inner-invalid", StringComparison.Ordinal) || id.Contains("inner-absent", StringComparison.Ordinal) ? "identity required" :
            id.Contains("design-disagrees", StringComparison.Ordinal) ? "raw evidence mismatch" :
            id.Contains("cross-payload", StringComparison.Ordinal) ? "wrong payload kind" :
            id.Contains("/null-sibling", StringComparison.Ordinal) ? "missing sibling" :
            id.Contains("null-input", StringComparison.Ordinal) || id.Contains("/empty", StringComparison.Ordinal) || id.Contains("incomplete", StringComparison.Ordinal) ? "incomplete sibling snapshot" :
            id.Contains("MM-D10", StringComparison.Ordinal) ? "DerivedAisles" :
            id.Contains("MM-D08", StringComparison.Ordinal) ? "PostPeralte" :
            id.Contains("malformed", StringComparison.Ordinal) || id.Contains("perm-3", StringComparison.Ordinal) ? "AUTH-13 raw:" : "Header provenance";
        Assert.Contains(cause, bad.Diagnostic);
        string valid = Rich(c.Kind);
        Assert.Equal(O.Single, Compare(c.Kind, Input(Sibling(c.Kind, valid), Sibling(c.Kind, valid, "B"))).Outcome);
        output.WriteLine(c.Id + " -> " + bad.Diagnostic);
    }

    [Theory, MemberData(nameof(ReaderControls))]
    public void Raw_gate_negative_control(string kind, string mutation)
    {
        string a = Rich(kind);
        string b = mutation switch
        {
            "root" => Set(a, "Future", "{}"),
            "nested" => Set(a, Root(kind) + ".Future", "[]"),
            "schema" => Set(a, "SchemaVersion", "\"2.1\""),
            "duplicate" => a.Insert(1, "\"SchemaVersion\":\"2.0\","),
            _ => throw new ArgumentException(mutation)
        };
        Assert.Equal(O.Single, Compare(kind, Input(Sibling(kind, a), Sibling(kind, a, "B"))).Outcome);
        var bad = Compare(kind, Input(Sibling(kind, b), Sibling(kind, b, "B")));
        Assert.Equal(O.Unreadable, bad.Outcome);
        // A recorded temporary production mutation disables these raw gates and must turn this into false Single.
    }

    [Fact]
    public void Carrier_captures_sources_and_does_not_allow_list_replacement()
    {
        string raw = Rich("dynamic");
        var sibling = Sibling("dynamic", raw);
        var list = new List<RackAuthoredSibling> { new(sibling.SourceIdentity, "dynamic", sibling.RawEnvelope, raw) };
        var input = new RackAuthoredInput(Outer, list, true, "all handles captured including unreadable");
        list.Clear();
        Assert.Single(input.Siblings);
        Assert.Equal(O.Single, RackAuthoredComparatorPorts.Dynamic().Compare(input).Outcome);
        Assert.Throws<NotSupportedException>(() => ((IList<RackAuthoredSibling>)input.Siblings).Clear());
    }

    [Fact]
    public void DimensionViews_remains_an_open_Int32_while_Dimensions_is_closed()
    {
        string a = Set(Rich("dynamic"), "DynamicSystem.DimensionViews", "-2147483648");
        var r = RackAuthoredComparatorPorts.Dynamic().Compare(ProductInput(Input(Sibling("dynamic", a)), "dynamic"));
        Assert.Equal(O.Single, r.Outcome);
        Assert.Equal(int.MinValue, (int)r.Authored.DimensionViews.Value);
        string b = Set(a, "DynamicSystem.Dimensions", "999");
        Assert.Equal(O.Unreadable, Compare("dynamic", Input(Sibling("dynamic", b))).Outcome);
    }

    [Fact]
    public void Cabecera_accepts_flat_legacy_and_never_materializes_physical_members()
    {
        string wrapped = Rich("cabecera");
        string flat = JsonDocument.Parse(wrapped).RootElement.GetProperty("Header").GetRawText();
        var r = RackAuthoredComparatorPorts.Cabecera().Compare(ProductInput(Input(Sibling("cabecera", wrapped), Sibling("cabecera", flat, "B")), "cabecera"));
        Assert.Equal(O.Single, r.Outcome);
        Assert.Empty(r.Authored.Members);
    }
}
