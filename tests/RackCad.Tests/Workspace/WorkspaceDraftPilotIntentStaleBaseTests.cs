#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.PushBack.Pilot;
using RackCad.Application.Views.Redraw;
using RackCad.Application.Workspace.Drafts;
using Xunit;

namespace RackCad.Tests;

/// <summary>One definition of a scan for the pilot intent tests: the fact of the scan and the raw text read for its key.</summary>
internal sealed record IntentItem(RackSiblingScanFact Fact, string? Text);

/// <summary>
/// I-64 F4-T2B1: shared builders of the intent tests. The envelopes come from the fixtures of F4-T2A (read by path from the
/// repository root); the variants are derived in code through the same store the product uses.
/// </summary>
internal static class IntentKit
{
    public const string Rack = "11111111-1111-4111-8111-000000000001";
    public const string Evidence = "RackSiblingScan/CaptureDrawing";

    /// <summary>pb-01-base, byte for byte: a single-sided Push Back rack of Id <see cref="Rack"/>.</summary>
    public static string BaseText() => PilotKit.Text("pb-01-base");

    public static string Rewrite(string text, Action<RackEmbedDocument> edit)
    {
        var store = new RackEmbedStore();
        var embed = store.Deserialize(text)!;
        edit(embed);
        return store.Serialize(embed);
    }

    /// <summary>The envelope of a fixture, re-stamped with another Id, view, section, name or kind.</summary>
    public static string Variant(string fixture, string id, string? view = null, int? section = null, string? name = null, string? kind = null) =>
        Rewrite(PilotKit.Text(fixture), embed =>
        {
            embed.Id = id;
            if (view != null) embed.View = view;
            if (section != null) embed.Section = section.Value;
            if (name != null) embed.Name = name;
            if (kind != null) embed.Kind = kind;
        });

    /// <summary>A definition whose fact is derived from its own text (the way the scan does for an interpretable envelope).</summary>
    public static IntentItem Member(string key, string text, bool xref = false, bool erase = false)
    {
        var embed = new RackEmbedStore().Deserialize(text)!;
        return new IntentItem(new RackSiblingScanFact(key, embed.View!, false, xref, true, embed.Id!, embed.Id!, 1, 0, erase), text);
    }

    /// <summary>A definition whose envelope cannot be interpreted: the fact only carries the probe of its Id.</summary>
    public static IntentItem Unreadable(string key, string probe, string? text = "{ not an envelope") =>
        new(new RackSiblingScanFact(key, null!, false, false, false, null!, probe, 1, 0, false), text);

    /// <summary>A fact with every field explicit, for the states the scan can produce but no fixture shows.</summary>
    public static IntentItem Custom(string key, string? id, string? view, string? text, bool interpretable = true, bool xref = false, string? probe = null) =>
        new(new RackSiblingScanFact(key, view!, false, xref, interpretable, id!, probe ?? id ?? string.Empty, 1, 0, false), text);

    public static PushBackPilotIntentScan Scan(string? rack, string? origin, params IntentItem[] items) =>
        Scan(rack, origin, null, items);

    public static PushBackPilotIntentScan Scan(string? rack, string? origin, PushBackPilotPreviewWitness? witness, params IntentItem[] items) =>
        new(
            rack,
            origin,
            items.Select(i => i.Fact).ToList(),
            ToDictionary(items),
            Evidence,
            witness);

    public static Dictionary<string, string?> ToDictionary(IEnumerable<IntentItem> items)
    {
        var raw = new Dictionary<string, string?>(StringComparer.Ordinal);
        foreach (var item in items) raw[item.Fact.DefinitionKey] = item.Text;
        return raw;
    }

    /// <summary>The entry of a key of a base; a missing key is an assertion failure, not an exception of the dictionary.</summary>
    public static DraftStaleBaseEntry Entry(DraftStaleBase staleBase, string key)
    {
        Assert.True(staleBase.Entries.ContainsKey(key), "the base has no entry for " + key);
        return staleBase.Entries[key];
    }

    public static PushBackPilotIntentResult Read(PushBackPilotIntentScan scan) => PushBackPilotIntentReader.Read(scan, PilotKit.Catalog);

    /// <summary>The usual admitted rack: two views of the same single-sided design, the origin being the frontal one.</summary>
    public static PushBackPilotIntentScan TwoViews(PushBackPilotPreviewWitness? witness = null) =>
        Scan(Rack, "A1", witness, Member("A1", BaseText()), Member("B2", Variant("pb-01-base", Rack, view: "lateral", section: 3)));
}

// I-64 F4-T2B1-INTENT-READ: INV-F4T2B1-02 (the input), 03 (the total stale base), 04 (the result carries it) and 11 (Check now).
public class WorkspaceDraftPilotIntentStaleBaseTests
{
    // ------------------------------------------------------------------ the membership table (D-T2B-02)

    [Fact]
    public void EveryClassificationBecomesTheMembershipOfTheTableAndANonMemberDisappears()
    {
        var plain = IntentKit.BaseText();
        var lateral = IntentKit.Variant("pb-01-base", IntentKit.Rack, view: "lateral", section: 3);
        var other = IntentKit.Variant("pb-02-modulos", "22222222-2222-4222-8222-000000000009");
        var scan = IntentKit.Scan(
            IntentKit.Rack,
            "A1",
            IntentKit.Member("A1", plain),
            IntentKit.Member("B2", lateral, erase: true),
            IntentKit.Member("C3", plain, xref: true),
            IntentKit.Unreadable("D4", IntentKit.Rack),
            IntentKit.Member("E5", other),
            IntentKit.Unreadable("F6", "22222222-2222-4222-8222-000000000009", "{ other rack, unreadable"),
            IntentKit.Custom("G7", string.Empty, "frontal", plain));

        var built = PushBackPilotIntentReader.BuildStaleBase(scan);

        Assert.Equal(new[] { "A1", "B2", "C3", "D4" }, built.Entries.Keys.OrderBy(k => k, StringComparer.Ordinal).ToArray());

        var a = IntentKit.Entry(built, "A1");
        Assert.Equal(DraftMembership.Mutable, a.Membership);
        Assert.Equal("frontal", a.View);
        Assert.Equal("-1", a.Section);
        Assert.Equal(plain, a.EnvelopeText);

        var b = IntentKit.Entry(built, "B2");
        Assert.Equal(DraftMembership.Mutable, b.Membership);
        Assert.Equal("lateral", b.View);
        Assert.Equal("3", b.Section);
        Assert.Equal(lateral, b.EnvelopeText);

        var c = IntentKit.Entry(built, "C3");
        Assert.Equal(DraftMembership.ReadOnly, c.Membership);
        Assert.Equal("frontal", c.View);
        Assert.Equal("-1", c.Section);
        Assert.Equal(plain, c.EnvelopeText);

        var d = IntentKit.Entry(built, "D4");
        Assert.Equal(DraftMembership.Blocking, d.Membership);
        Assert.Null(d.View);
        Assert.Null(d.Section);
        Assert.Null(d.EnvelopeText);

        foreach (var absent in new[] { "E5", "F6", "G7" })
        {
            Assert.False(built.Entries.ContainsKey(absent), absent);
            Assert.Equal(DraftMembership.NotMember, built.MembershipOf(absent));
        }
    }

    [Fact]
    public void ErasePassesAsMutableLikeRedrawAndTheBaseDoesNotKeepTheSplit()
    {
        var scan = IntentKit.Scan(
            IntentKit.Rack,
            "A1",
            IntentKit.Member("A1", IntentKit.BaseText(), erase: false),
            IntentKit.Member("B2", IntentKit.Variant("pb-01-base", IntentKit.Rack, view: "lateral", section: 1), erase: true));

        var built = PushBackPilotIntentReader.BuildStaleBase(scan);

        Assert.Equal(2, built.Entries.Count);
        Assert.All(built.Entries.Values, entry => Assert.Equal(DraftMembership.Mutable, entry.Membership));
    }

    [Fact]
    public void ThereIsOneEntryPerKeyWithTheExactTextIncludingItsLineEndings()
    {
        var crlf = IntentKit.BaseText() + "\r\n";
        var lf = IntentKit.Variant("pb-01-base", IntentKit.Rack, view: "lateral", section: 2) + "\n";
        var scan = IntentKit.Scan(IntentKit.Rack, "A1", IntentKit.Member("A1", crlf), IntentKit.Member("B2", lf));

        var built = PushBackPilotIntentReader.BuildStaleBase(scan);

        Assert.Equal(2, built.Entries.Count);
        Assert.Equal(crlf, IntentKit.Entry(built, "A1").EnvelopeText);
        Assert.Equal(lf, IntentKit.Entry(built, "B2").EnvelopeText);
        Assert.True(string.Equals(crlf, IntentKit.Entry(built, "A1").EnvelopeText, StringComparison.Ordinal));
        Assert.False(string.Equals(crlf, lf, StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("", -1)]
    [InlineData("es-MX", -1)]
    [InlineData("de-DE", 3)]
    [InlineData("sv-SE", -1)]
    [InlineData("sv-SE", 12)]
    public void TheSectionIsInvariantTextInEveryCulture(string culture, int section)
    {
        var text = IntentKit.Variant("pb-01-base", IntentKit.Rack, section: section);
        var scan = IntentKit.Scan(IntentKit.Rack, "A1", IntentKit.Member("A1", text));

        PilotKit.WithCulture(culture, () =>
        {
            var built = PushBackPilotIntentReader.BuildStaleBase(scan);

            Assert.Equal(section.ToString(System.Globalization.CultureInfo.InvariantCulture), IntentKit.Entry(built, "A1").Section);
        });
    }

    [Fact]
    public void TheViewIsTheOneOfTheFactAndNotTheOneOfTheEnvelope()
    {
        var text = IntentKit.BaseText();
        var scan = IntentKit.Scan(
            IntentKit.Rack,
            "A1",
            IntentKit.Custom("A1", IntentKit.Rack, "planta", text));

        var built = PushBackPilotIntentReader.BuildStaleBase(scan);

        Assert.Equal("planta", IntentKit.Entry(built, "A1").View);
        Assert.Equal("-1", IntentKit.Entry(built, "A1").Section);
    }

    [Fact]
    public void ThePilotDoesNotReadTheVariableRegistry()
    {
        var built = PushBackPilotIntentReader.BuildStaleBase(IntentKit.TwoViews());

        Assert.NotEmpty(built.Entries);
        Assert.Null(built.VariableRegistryText);
    }

    [Fact]
    public void AnInterpretableMemberWhoseTextIsMissingOrDoesNotDeserializeKeepsNoSectionAndTheTextAsItIs()
    {
        var scan = IntentKit.Scan(
            IntentKit.Rack,
            "A1",
            IntentKit.Member("A1", IntentKit.BaseText()),
            IntentKit.Custom("B2", IntentKit.Rack, "lateral", null),
            IntentKit.Custom("C3", IntentKit.Rack, "lateral", "{ not json"));

        var built = PushBackPilotIntentReader.BuildStaleBase(scan);

        Assert.Equal(3, built.Entries.Count);
        Assert.Equal(DraftMembership.Mutable, IntentKit.Entry(built, "B2").Membership);
        Assert.Null(IntentKit.Entry(built, "B2").Section);
        Assert.Null(IntentKit.Entry(built, "B2").EnvelopeText);
        Assert.Equal(DraftMembership.Mutable, IntentKit.Entry(built, "C3").Membership);
        Assert.Null(IntentKit.Entry(built, "C3").Section);
        Assert.Equal("{ not json", IntentKit.Entry(built, "C3").EnvelopeText);
    }

    // ------------------------------------------------------------------ a blank RackId (INV-F4T2B1-03)

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t \r\n")]
    [InlineData(null)]
    public void ABlankRackIdGivesAnEmptyBaseAndNeverThrows(string? rackId)
    {
        var scan = IntentKit.Scan(rackId, "A1", IntentKit.Member("A1", IntentKit.BaseText()), IntentKit.Unreadable("B2", IntentKit.Rack));

        var built = PushBackPilotIntentReader.BuildStaleBase(scan);
        var result = IntentKit.Read(scan);

        Assert.Empty(built.Entries);
        Assert.Empty(result.StaleBase.Entries);
        Assert.True(result.StaleBase.CompareWith(built).IsMatch);
        Assert.False(result.Read.IsAdmitted);
    }

    // ------------------------------------------------------------------ independence (INV-F4T2B1-03, 04)

    private static IEnumerable<(string Name, PushBackPilotIntentScan Scan, bool Admitted)> Scenarios()
    {
        yield return ("admitted", IntentKit.TwoViews(), true);
        yield return ("blocking member", IntentKit.Scan(IntentKit.Rack, "A1", IntentKit.Member("A1", IntentKit.BaseText()), IntentKit.Unreadable("B2", IntentKit.Rack)), false);
        yield return ("divergent design", IntentKit.Scan(
            IntentKit.Rack,
            "A1",
            IntentKit.Member("A1", IntentKit.BaseText()),
            IntentKit.Member("B2", IntentKit.Variant("pb-02-modulos", IntentKit.Rack, view: "lateral", section: 3))), false);
        yield return ("origin missing", IntentKit.Scan(IntentKit.Rack, "ZZ", IntentKit.Member("A1", IntentKit.BaseText())), false);
        yield return ("origin read only", IntentKit.Scan(IntentKit.Rack, "A1", IntentKit.Member("A1", IntentKit.BaseText(), xref: true), IntentKit.Member("B2", IntentKit.BaseText())), false);
    }

    [Fact]
    public void TheBaseOfTheResultIsTheOneOfBuildStaleBaseWhetherOrNotTheReadIsAdmitted()
    {
        foreach (var (name, scan, admitted) in Scenarios())
        {
            var result = IntentKit.Read(scan);
            var rebuilt = PushBackPilotIntentReader.BuildStaleBase(scan);

            Assert.True(admitted == result.Read.IsAdmitted, name);
            Assert.NotNull(result.StaleBase);
            Assert.NotEmpty(result.StaleBase.Entries);
            Assert.True(result.StaleBase.CompareWith(rebuilt).IsMatch, name);
            Assert.True(rebuilt.CompareWith(result.StaleBase).IsMatch, name);
        }
    }

    [Fact]
    public void TheAdmittedReadCarriesTheSameBaseInstanceAsTheResult()
    {
        var result = IntentKit.Read(IntentKit.TwoViews());

        Assert.True(result.Read.IsAdmitted);
        Assert.Same(result.StaleBase, result.Read.StaleBase);
    }

    [Fact]
    public void TheBaseDoesNotDependOnTheOrderOfTheFacts()
    {
        var items = new[]
        {
            IntentKit.Member("A1", IntentKit.BaseText()),
            IntentKit.Member("B2", IntentKit.Variant("pb-01-base", IntentKit.Rack, view: "lateral", section: 3)),
            IntentKit.Member("C3", IntentKit.BaseText(), xref: true),
            IntentKit.Unreadable("D4", IntentKit.Rack),
            IntentKit.Member("E5", IntentKit.Variant("pb-02-modulos", "22222222-2222-4222-8222-000000000009")),
        };
        var reference = PushBackPilotIntentReader.BuildStaleBase(IntentKit.Scan(IntentKit.Rack, "A1", items));
        Assert.Equal(4, reference.Entries.Count);

        var orders = new[]
        {
            items.Reverse().ToArray(),
            items.Skip(2).Concat(items.Take(2)).ToArray(),
            new[] { items[3], items[0], items[4], items[2], items[1] },
        };
        foreach (var order in orders)
        {
            var built = PushBackPilotIntentReader.BuildStaleBase(IntentKit.Scan(IntentKit.Rack, "A1", order));

            Assert.True(reference.CompareWith(built).IsMatch);
            Assert.True(built.CompareWith(reference).IsMatch);
        }
    }

    [Fact]
    public void TheBaseOnlyKeepsTheTextOfTheMembersAndNothingOfOtherRacks()
    {
        var foreignText = IntentKit.Variant("pb-02-modulos", "22222222-2222-4222-8222-000000000009", name: "FOREIGN-RACK-NAME");
        var unreadableForeign = "{ foreign unreadable FOREIGN-RACK-NAME";
        var blockingText = "{ blocking own unreadable BLOCKING-OWN-TEXT";
        var scan = IntentKit.Scan(
            IntentKit.Rack,
            "A1",
            IntentKit.Member("A1", IntentKit.BaseText()),
            IntentKit.Member("E5", foreignText),
            IntentKit.Unreadable("F6", "22222222-2222-4222-8222-000000000009", unreadableForeign),
            IntentKit.Unreadable("D4", IntentKit.Rack, blockingText));

        var result = IntentKit.Read(scan);

        Assert.Equal(new[] { "A1", "D4" }, result.StaleBase.Entries.Keys.OrderBy(k => k, StringComparer.Ordinal).ToArray());
        var texts = result.StaleBase.Entries.Values.Select(e => e.EnvelopeText).ToList();
        Assert.DoesNotContain(foreignText, texts);
        Assert.DoesNotContain(unreadableForeign, texts);
        Assert.DoesNotContain(blockingText, texts);
        Assert.Equal(IntentKit.BaseText(), IntentKit.Entry(result.StaleBase, "A1").EnvelopeText);
        Assert.Null(IntentKit.Entry(result.StaleBase, "D4").EnvelopeText);
    }

    // ------------------------------------------------------------------ the input (INV-F4T2B1-02)

    private static RackSiblingScanFact Plain(string key = "A1") => new(key, "frontal", false, false, true, IntentKit.Rack, IntentKit.Rack, 1, 0, false);

    private static Dictionary<string, string?> Texts() => new(StringComparer.Ordinal) { ["A1"] = IntentKit.BaseText() };

    [Fact]
    public void EveryStructuralViolationOfTheInputThrowsAnArgumentException()
    {
        var cases = new List<(string Name, Func<PushBackPilotIntentScan> Build)>
        {
            ("facts null", () => new PushBackPilotIntentScan(IntentKit.Rack, "A1", null!, Texts(), IntentKit.Evidence)),
            ("a null fact", () => new PushBackPilotIntentScan(IntentKit.Rack, "A1", new RackSiblingScanFact[] { Plain(), null! }, Texts(), IntentKit.Evidence)),
            ("empty fact key", () => new PushBackPilotIntentScan(IntentKit.Rack, "A1", new[] { Plain(string.Empty) }, Texts(), IntentKit.Evidence)),
            ("blank fact key", () => new PushBackPilotIntentScan(IntentKit.Rack, "A1", new[] { Plain(" ") }, Texts(), IntentKit.Evidence)),
            ("repeated fact key", () => new PushBackPilotIntentScan(IntentKit.Rack, "A1", new[] { Plain("A1"), Plain("A1") }, Texts(), IntentKit.Evidence)),
            ("negative layout count", () => new PushBackPilotIntentScan(IntentKit.Rack, "A1",
                new[] { new RackSiblingScanFact("A1", "frontal", false, false, true, IntentKit.Rack, IntentKit.Rack, -1, 0, false) }, Texts(), IntentKit.Evidence)),
            ("negative nested count", () => new PushBackPilotIntentScan(IntentKit.Rack, "A1",
                new[] { new RackSiblingScanFact("A1", "frontal", false, false, true, IntentKit.Rack, IntentKit.Rack, 1, -1, false) }, Texts(), IntentKit.Evidence)),
            ("a selected source", () => new PushBackPilotIntentScan(IntentKit.Rack, "A1",
                new[] { new RackSiblingScanFact("A1", "frontal", true, false, true, IntentKit.Rack, IntentKit.Rack, 1, 0, false) }, Texts(), IntentKit.Evidence)),
            ("raw envelopes null", () => new PushBackPilotIntentScan(IntentKit.Rack, "A1", new[] { Plain() }, null!, IntentKit.Evidence)),
            ("empty raw key", () => new PushBackPilotIntentScan(IntentKit.Rack, "A1", new[] { Plain() },
                new Dictionary<string, string?> { [string.Empty] = "x" }, IntentKit.Evidence)),
            ("evidence null", () => new PushBackPilotIntentScan(IntentKit.Rack, "A1", new[] { Plain() }, Texts(), null!)),
            ("evidence empty", () => new PushBackPilotIntentScan(IntentKit.Rack, "A1", new[] { Plain() }, Texts(), string.Empty)),
            ("evidence blank", () => new PushBackPilotIntentScan(IntentKit.Rack, "A1", new[] { Plain() }, Texts(), "  ")),
            ("witness key empty", () => new PushBackPilotIntentScan(IntentKit.Rack, "A1", new[] { Plain() }, Texts(), IntentKit.Evidence,
                new PushBackPilotPreviewWitness(string.Empty, "x"))),
            ("witness key blank", () => new PushBackPilotIntentScan(IntentKit.Rack, "A1", new[] { Plain() }, Texts(), IntentKit.Evidence,
                new PushBackPilotPreviewWitness(" ", null))),
        };

        foreach (var (name, build) in cases)
        {
            var thrown = Record.Exception(() => build());
            Assert.True(thrown is ArgumentException, name + " -> " + (thrown?.GetType().Name ?? "no exception"));
        }
    }

    [Fact]
    public void NoStateOfTheDrawingMakesTheInputOrTheReadThrowAndEachOneHasItsOwnVerdict()
    {
        // The absence of an exception, the non-null results and "not admitted" would pass against an inert implementation that
        // never throws and answers a provisional text. What discriminates is the EXACT text of the verdict of each state: it is
        // written here as a literal and not through the constants of the reader, so a stub that returns the same placeholder for
        // the constants and for the result (as the skeleton of the RED did) cannot satisfy it.
        const string noIdentity = "No editable aquí: el sobre no tiene identidad";
        const string originMissing = "No editable aquí: el origen ya no está en el dibujo";
        const string unreadable = "No editable aquí: ilegible o de versión futura";
        var states = new List<(PushBackPilotIntentScan Scan, string Diagnostic)>
        {
            (IntentKit.Scan(null, null), noIdentity),
            (IntentKit.Scan(string.Empty, string.Empty), noIdentity),
            (IntentKit.Scan(" ", "A1", IntentKit.Member("A1", IntentKit.BaseText())), noIdentity),
            (IntentKit.Scan(IntentKit.Rack, null, IntentKit.Member("A1", IntentKit.BaseText())), originMissing),
            (IntentKit.Scan(IntentKit.Rack, "A1", IntentKit.Custom("A1", string.Empty, "frontal", IntentKit.BaseText())), noIdentity),
            (IntentKit.Scan(IntentKit.Rack, "A1", IntentKit.Custom("A1", "   ", "frontal", IntentKit.BaseText())), noIdentity),
            (IntentKit.Scan(IntentKit.Rack, "A1", IntentKit.Custom("A1", null, null, null)), unreadable),
            (IntentKit.Scan(IntentKit.Rack, "A1", IntentKit.Unreadable("A1", IntentKit.Rack, null)), unreadable),
            (IntentKit.Scan(IntentKit.Rack, "A1", IntentKit.Unreadable("A1", string.Empty, string.Empty)), unreadable),
        };

        foreach (var (scan, diagnostic) in states)
        {
            var thrown = Record.Exception(() =>
            {
                var result = IntentKit.Read(scan);
                Assert.NotNull(result.StaleBase);
                Assert.NotNull(result.Read);
                Assert.False(result.Read.IsAdmitted);
                Assert.Equal(diagnostic, result.Read.Admission.Diagnostic);
                _ = PushBackPilotIntentReader.BuildStaleBase(scan);
            });
            Assert.Null(thrown);
        }
    }

    [Fact]
    public void ANullTextIsTheSameAsAnAbsentKey()
    {
        var raw = new Dictionary<string, string?> { ["A1"] = IntentKit.BaseText(), ["B2"] = null };
        var scan = new PushBackPilotIntentScan(IntentKit.Rack, "A1", new[] { Plain("A1"), Plain("B2") }, raw, IntentKit.Evidence);

        Assert.True(scan.RawEnvelopes.ContainsKey("A1"));
        Assert.False(scan.RawEnvelopes.ContainsKey("B2"));
        Assert.Equal(1, scan.RawEnvelopes.Count);
    }

    [Fact]
    public void TheRawEnvelopesOfTheInputAreAReadOnlyViewThatCannotBeConvertedToAMutableDictionaryNorChangeTheInput()
    {
        // M-5: the property type says read-only, and the object behind it must say it too.
        var scan = IntentKit.Scan(IntentKit.Rack, "A1", IntentKit.Member("A1", IntentKit.BaseText()));
        var view = scan.RawEnvelopes;
        var original = view["A1"];

        Assert.False(view is Dictionary<string, string>);
        if (view is IDictionary<string, string> generic)
        {
            Assert.True(generic.IsReadOnly);
            Assert.ThrowsAny<NotSupportedException>(() => generic["A1"] = "{ changed");
            Assert.ThrowsAny<NotSupportedException>(() => generic["B2"] = "{ added");
            Assert.ThrowsAny<NotSupportedException>(() => generic.Remove("A1"));
            Assert.ThrowsAny<NotSupportedException>(() => generic.Clear());
        }

        if (view is System.Collections.IDictionary legacy)
        {
            Assert.True(legacy.IsReadOnly);
            Assert.ThrowsAny<NotSupportedException>(() => legacy["A1"] = "{ changed");
        }

        if (view is ICollection<KeyValuePair<string, string>> pairs)
            Assert.ThrowsAny<NotSupportedException>(() => pairs.Add(new KeyValuePair<string, string>("B2", "{ added")));

        Assert.Equal(1, scan.RawEnvelopes.Count);
        Assert.Equal(original, scan.RawEnvelopes["A1"]);
        Assert.False(scan.RawEnvelopes.ContainsKey("B2"));
        Assert.True(IntentKit.Read(scan).Read.IsAdmitted);
    }

    [Fact]
    public void TheInputCopiesItsCollectionsSoLaterChangesOfTheCallerChangeNothing()
    {
        var facts = new List<RackSiblingScanFact> { Plain("A1") };
        var raw = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase) { ["A1"] = IntentKit.BaseText() };
        var scan = new PushBackPilotIntentScan(IntentKit.Rack, "A1", facts, raw, IntentKit.Evidence);
        var before = IntentKit.Read(scan);
        Assert.True(before.Read.IsAdmitted);

        facts.Add(Plain("B2"));
        facts.Clear();
        raw["A1"] = "{ changed";
        raw["B2"] = IntentKit.BaseText();
        raw.Remove("A1");

        Assert.Equal(1, scan.Facts.Count);
        Assert.Equal("A1", scan.Facts[0].DefinitionKey);
        Assert.Equal(IntentKit.BaseText(), scan.RawEnvelopes["A1"]);
        Assert.False(scan.RawEnvelopes.ContainsKey("B2"));
        Assert.False(scan.RawEnvelopes.ContainsKey("a1"));
        Assert.False(scan.Facts is IList<RackSiblingScanFact> { IsReadOnly: false });
        var after = IntentKit.Read(scan);
        Assert.True(after.Read.IsAdmitted);
        Assert.True(after.StaleBase.CompareWith(before.StaleBase).IsMatch);
    }

    [Fact]
    public void ATextWithInvalidUtf16DoesNotThrowAnywhereAndIsNeverAdmitted()
    {
        var loneSurrogate = (char)0xD800;
        var broken = IntentKit.BaseText().Replace("PB-01", "PB-" + loneSurrogate, StringComparison.Ordinal);
        Assert.Contains(loneSurrogate, broken);

        foreach (var scan in new[]
                 {
                     IntentKit.Scan(IntentKit.Rack, "A1", IntentKit.Member("A1", IntentKit.BaseText()), IntentKit.Custom("B2", IntentKit.Rack, "lateral", broken)),
                     IntentKit.Scan(IntentKit.Rack, "A1", IntentKit.Custom("A1", IntentKit.Rack, "frontal", broken)),
                 })
        {
            PushBackPilotIntentResult? result = null;
            var thrown = Record.Exception(() => result = IntentKit.Read(scan));

            Assert.Null(thrown);
            Assert.NotNull(result);
            Assert.False(result!.Read.IsAdmitted);
            Assert.Equal(PushBackPilotIntentReader.UnreadableDiagnostic, result.Read.Admission.Diagnostic);
            var kept = result.StaleBase.Entries.Values.Select(e => e.EnvelopeText).Where(t => t != null && t.Contains(loneSurrogate)).ToList();
            Assert.Equal(broken, Assert.Single(kept));
        }
    }

    // ------------------------------------------------------------------ Check now with the real core (INV-F4T2B1-11, B-1)

    private static DraftSession<PushBackPilotDraftState> UnknownAfterACommitThatThrew(PushBackPilotIntentResult first)
    {
        Assert.True(first.Read.IsAdmitted);
        var begun = new DraftRegistry().Begin(DraftKit.S1, "R1", first.Read, PushBackPilotStateComparer.Instance, PushBackPilotStateComparer.Copy);
        Assert.Equal(DraftBeginStatus.Created, begun.Status);
        var draft = begun.Draft!;
        Assert.True(draft.TryBeginApply().IsStarted);
        draft.CompleteApply(DraftApplyOutcome<PushBackPilotDraftState>.CommitThrew("the commit threw"));
        Assert.Equal(DraftStatus.Unknown, draft.Status);
        Assert.Equal(DraftUnknownOrigin.CommitThrew, draft.UnknownOrigin);
        return draft;
    }

    [Fact]
    public void ARereadThatIsNotAdmittedBecauseAMemberBecameBlockingTakesTheUnknownDraftToConflictWithTheDifferences()
    {
        var draft = UnknownAfterACommitThatThrew(IntentKit.Read(IntentKit.TwoViews()));
        var reread = IntentKit.Read(IntentKit.Scan(
            IntentKit.Rack,
            "A1",
            IntentKit.Member("A1", IntentKit.BaseText()),
            IntentKit.Unreadable("B2", IntentKit.Rack)));
        Assert.False(reread.Read.IsAdmitted);
        Assert.Null(reread.Read.StaleBase);

        var check = draft.CheckNow(reread.StaleBase);

        Assert.Equal(DraftCheckStatus.Conflict, check.Status);
        Assert.Equal(DraftStatus.Conflict, draft.Status);
        Assert.Contains(new DraftDifference("B2", DraftDifferenceKind.Membership), check.Differences);
    }

    [Fact]
    public void ARereadThatIsNotAdmittedBecauseAuth13FindsDivergenceTakesTheUnknownDraftToConflictWithTheDifferences()
    {
        var draft = UnknownAfterACommitThatThrew(IntentKit.Read(IntentKit.TwoViews()));
        var reread = IntentKit.Read(IntentKit.Scan(
            IntentKit.Rack,
            "A1",
            IntentKit.Member("A1", IntentKit.BaseText()),
            IntentKit.Member("B2", IntentKit.Variant("pb-02-modulos", IntentKit.Rack, view: "lateral", section: 3))));
        Assert.Equal(PushBackPilotIntentReader.DivergentDiagnostic, reread.Read.Admission.Diagnostic);

        var check = draft.CheckNow(reread.StaleBase);

        Assert.Equal(DraftCheckStatus.Conflict, check.Status);
        Assert.Equal(DraftStatus.Conflict, draft.Status);
        Assert.Contains(new DraftDifference("B2", DraftDifferenceKind.EnvelopeText), check.Differences);
    }

    [Fact]
    public void ARereadThatIsEqualRecoversTheUnknownDraft()
    {
        // The read before the commit is admitted; the reread has the same base, and it is the base that Check now compares.
        var draft = UnknownAfterACommitThatThrew(IntentKit.Read(IntentKit.TwoViews()));
        var reread = IntentKit.Read(IntentKit.TwoViews());
        Assert.NotEmpty(reread.StaleBase.Entries);

        var check = draft.CheckNow(reread.StaleBase);

        Assert.Equal(DraftCheckStatus.Recovered, check.Status);
        Assert.Empty(check.Differences);
        Assert.NotEqual(DraftStatus.Unknown, draft.Status);
        Assert.NotEqual(DraftStatus.Conflict, draft.Status);
    }

    [Fact]
    public void ARereadThatIsNotAdmittedOnlyForTheCatalogStillRecoversBecauseTheBaseIsTheSame()
    {
        // Two reads of the same facts can differ in admission only through what the base does not hold (the catalog): the base is the same.
        var draft = UnknownAfterACommitThatThrew(IntentKit.Read(IntentKit.TwoViews()));
        var withoutCatalog = PushBackPilotIntentReader.Read(IntentKit.TwoViews(), null);
        Assert.False(withoutCatalog.Read.IsAdmitted);

        var check = draft.CheckNow(withoutCatalog.StaleBase);

        Assert.Equal(DraftCheckStatus.Recovered, check.Status);
    }
}
