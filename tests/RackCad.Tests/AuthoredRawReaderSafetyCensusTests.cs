using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Shared;
using Xunit;
using static RackCad.Tests.I58F1Fixtures;
using static RackCad.Tests.I58F1Oracles;
using O = RackCad.Application.Systems.Shared.RackAuthoredComparisonOutcome;

namespace RackCad.Tests;

// I-64, cuarta tanda (review of «seg», 2026-11-26): the raw reader of AUTH-13 (AuthoredRawReader.Shapes) reads a safety selection with a
// CLOSED list of members and takes any other as «unknown member». DesviadorCellsByPost was added to SafetySelectionDocument without it,
// so every Dinámico with the marker came out unreadable. This census hands the reader, one by one, every member that the safety documents
// write, in the rich fixture of I-58 F1 (Dinámico and Push Back share the shape), so a new field cannot escape the reader again.
public sealed class AuthoredRawReaderSafetyCensusTests
{
    // The documents of a safety selection, and where the rich fixture holds one of each (I58F1Fixtures.Rich).
    private static readonly (Type Document, string Path)[] Shapes =
    {
        (typeof(SafetySelectionDocument), "SafetySelections.0"),
        (typeof(PostSideDocument), "SafetySelections.0.PostSides.0"),
        (typeof(GridCellDocument), "SafetySelections.0.DesviadorOffCells.0"),
        (typeof(PostDefenseDocument), "SafetySelections.0.DefensaPosts.0"),
        (typeof(BootPostDocument), "SafetySelections.0.BotaPosts.0"),
    };

    private static readonly string[] Kinds = { "dynamic", "pushback" };

    // The members a document writes: its public read-write properties, by the name the store writes (it has no naming policy).
    private static IEnumerable<string> Written(Type document)
        => document.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.CanWrite && p.GetIndexParameters().Length == 0
                        && p.GetCustomAttribute<JsonExtensionDataAttribute>() == null
                        && p.GetCustomAttribute<JsonIgnoreAttribute>()?.Condition != JsonIgnoreCondition.Always)
            .Select(p => p.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? p.Name);

    // The AUTH-13 port of the kind over two views of the same raw design.
    private static (O Outcome, string Diagnostic) Read(string kind, string a, string b = null)
    {
        var input = ProductInput(Input(Sibling(kind, a), Sibling(kind, b ?? a, "handle-B")), kind);
        return kind == "dynamic"
            ? Result(RackAuthoredComparatorPorts.Dynamic().Compare(input))
            : Result(RackAuthoredComparatorPorts.PushBack().Compare(input));
    }

    private static (O, string) Result<T>(RackAuthoredComparisonResult<T> result) => (result.Outcome, result.Diagnostic ?? string.Empty);

    [Fact]
    public void EveryMemberThatTheSafetyDocumentsWriteIsKnownToTheRawReader()
    {
        var unknown = new List<string>();
        var members = 0;
        foreach (var kind in Kinds)
        {
            var rich = Rich(kind);
            var baseline = Read(kind, rich);
            Assert.True(baseline.Outcome == O.Single, kind + ": " + baseline.Diagnostic);
            foreach (var (document, path) in Shapes)
            {
                var at = Structure(kind) + "." + path;
                Assert.NotNull(At(JsonNode.Parse(rich), at));
                foreach (var member in Written(document))
                {
                    members++;
                    // The value does not matter: a known member with a value it does not take fails for that value, never as unknown.
                    var (_, diagnostic) = Read(kind, Set(rich, at + "." + member, "null"));
                    if (diagnostic.Contains("unknown member", StringComparison.Ordinal))
                        unknown.Add(kind + " " + document.Name + "." + member + ": " + diagnostic);
                }
            }
        }

        Assert.Contains("DesviadorCellsByPost", Written(typeof(SafetySelectionDocument)));
        Assert.True(members >= 70, members.ToString());
        Assert.Empty(unknown);
    }

    // The census can fail: a member that no safety document writes is unknown at each of those places.
    [Fact]
    public void AMemberThatNoSafetyDocumentWritesIsUnknownAtEachPlace()
    {
        foreach (var kind in Kinds)
        {
            var rich = Rich(kind);
            foreach (var (_, path) in Shapes)
            {
                var (outcome, diagnostic) = Read(kind, Set(rich, Structure(kind) + "." + path + ".CensusFuture", "null"));
                Assert.Equal(O.Unreadable, outcome);
                Assert.Contains("unknown member", diagnostic, StringComparison.Ordinal);
            }
        }
    }

    // The member of the correction itself: absent and false are the historic reading of the desviador and read the same; true is the
    // reading by post, another design.
    [Fact]
    public void DesviadorCellsByPostIsReadAndItsAbsenceIsTheHistoricReading()
    {
        var rich = Rich("dynamic");
        var member = Structure("dynamic") + ".SafetySelections.0.DesviadorCellsByPost";
        Assert.Null(At(JsonNode.Parse(rich), Structure("dynamic") + ".SafetySelections.0")["DesviadorCellsByPost"]);
        var byPost = Set(rich, member, "true");

        Assert.Equal(O.Single, Read("dynamic", byPost).Outcome);
        Assert.Equal(O.Single, Read("dynamic", rich, Set(rich, member, "false")).Outcome);
        Assert.Equal(O.Divergent, Read("dynamic", rich, byPost).Outcome);
        Assert.Equal(O.Unreadable, Read("dynamic", Set(rich, member, "1")).Outcome);
    }
}
