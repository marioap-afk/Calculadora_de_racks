#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RackCad.Application.Workspace.Drafts;
using Xunit;

namespace RackCad.Tests;

// I-64 F4-T1-DRAFT-CORE: INV-F4T1-06 and 07. The stale base only detects external changes (D-12, INV-28, INV-29).
public class WorkspaceDraftStaleBaseTests
{
    private static DraftDifference D(string? key, DraftDifferenceKind kind) => new(key, kind);

    // ------------------------------------------------------------------ what the base stores (INV-F4T1-06)

    [Fact]
    public void TheBaseStoresAFourValuedAttributionWithViewAndSectionPerDefinitionKey()
    {
        var staleBase = DraftKit.Base(
            DraftKit.Mutable("D1", "Front", "S1", "{m}"),
            DraftKit.ReadOnly("X1", "Side", "S2", "{r}"),
            DraftKit.Blocking("B1"),
            DraftKit.NotMember("O1"));

        Assert.Equal(DraftMembership.Mutable, staleBase.MembershipOf("D1"));
        Assert.Equal(DraftMembership.ReadOnly, staleBase.MembershipOf("X1"));
        Assert.Equal(DraftMembership.Blocking, staleBase.MembershipOf("B1"));
        Assert.Equal(DraftMembership.NotMember, staleBase.MembershipOf("O1"));
        Assert.Equal("Front", staleBase.Entries["D1"].View);
        Assert.Equal("S1", staleBase.Entries["D1"].Section);
        Assert.Equal("Side", staleBase.Entries["X1"].View);
        Assert.Equal("S2", staleBase.Entries["X1"].Section);
    }

    [Fact]
    public void TheAttributionHasExactlyFourValuesAndNoRedrawEraseDivision()
    {
        Assert.Equal(
            new[] { "Blocking", "Mutable", "NotMember", "ReadOnly" },
            Enum.GetNames<DraftMembership>().OrderBy(n => n, StringComparer.Ordinal).ToArray());

        var names = typeof(DraftStaleBase).GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
            .Select(m => m.Name)
            .Concat(typeof(DraftStaleBaseEntry).GetMembers(BindingFlags.Public | BindingFlags.Instance).Select(m => m.Name))
            .Concat(Enum.GetNames<DraftMembership>());
        Assert.DoesNotContain(names, n => n.Contains("Redraw", StringComparison.OrdinalIgnoreCase) || n.Contains("Erase", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void AKeyThatWasNotRecordedIsNotAMember()
    {
        var staleBase = DraftKit.Base(DraftKit.Mutable("D1"));

        Assert.Equal(DraftMembership.Mutable, staleBase.MembershipOf("D1"));
        Assert.Equal(DraftMembership.NotMember, staleBase.MembershipOf("never-seen"));
        Assert.False(staleBase.Entries.ContainsKey("never-seen"));
    }

    [Fact]
    public void TheEnvelopeTextOfAMemberIsKeptExactlyAsGivenAndTheTextOfANonMemberIsNeverKept()
    {
        var raw = "  {\"v\":1}\r\n\t";
        var staleBase = DraftKit.Base(
            DraftKit.Mutable("D1", text: raw),
            DraftKit.ReadOnly("X1", text: "{ro}"),
            DraftKit.NotMember("O1", "{secret other rack}"));

        Assert.True(staleBase.Entries.ContainsKey("D1"));
        Assert.True(staleBase.Entries.ContainsKey("X1"));
        Assert.Equal(raw, staleBase.Entries["D1"].EnvelopeText);
        Assert.Equal("{ro}", staleBase.Entries["X1"].EnvelopeText);
        Assert.False(staleBase.Entries.ContainsKey("O1"));
        Assert.DoesNotContain(staleBase.Entries.Values, e => e.EnvelopeText == "{secret other rack}");
    }

    [Fact]
    public void ABaseWithTheSameFactsMatchesWithNoDifferences()
    {
        var one = DraftKit.DefaultBase();
        var two = DraftKit.DefaultBase();
        Assert.Equal(DraftMembership.Mutable, one.MembershipOf("D2"));

        var comparison = one.CompareWith(two);

        Assert.True(comparison.IsMatch);
        Assert.Empty(comparison.Differences);
        Assert.True(one.CompareWith(one).IsMatch);
    }

    [Fact]
    public void ANullOrEmptyKeyIsAProgrammingError()
    {
        var staleBase = DraftKit.Base(DraftKit.Mutable("D1"));

        Assert.Throws<ArgumentException>(() => DraftKit.Base(DraftKit.Mutable("")));
        Assert.Throws<ArgumentNullException>(() => DraftStaleBase.Create(null!));
        Assert.Throws<ArgumentException>(() => staleBase.MembershipOf(""));
        Assert.Throws<ArgumentException>(() => staleBase.IsAmbiguous(""));
        Assert.Throws<ArgumentNullException>(() => staleBase.CompareWith(null!));
    }

    // ------------------------------------------------------------------ a repeated key (INV-F4T1-21, I-3)

    [Fact]
    public void ARepeatedKeyWithTheSameFactsIsOneEntryInEitherOrder()
    {
        var once = DraftKit.Base(DraftKit.Mutable("D1", "Front", "S1", "{a}"));
        var twice = DraftKit.Base(DraftKit.Mutable("D1", "Front", "S1", "{a}"), DraftKit.Mutable("D1", "Front", "S1", "{a}"));

        Assert.Single(twice.Entries);
        Assert.False(twice.IsAmbiguous("D1"));
        Assert.True(once.CompareWith(twice).IsMatch);
        Assert.True(twice.CompareWith(once).IsMatch);
        Assert.True(twice.CompareWith(twice).IsMatch);
    }

    [Fact]
    public void ARepeatedNotMemberKeyWithDifferentTextIsStillJustNotMember()
    {
        var repeated = DraftKit.Base(DraftKit.Mutable("D1"), DraftKit.NotMember("O1", "{one}"), DraftKit.NotMember("O1", "{two}"));
        var plain = DraftKit.Base(DraftKit.Mutable("D1"));

        Assert.False(repeated.IsAmbiguous("O1"));
        Assert.Equal(DraftMembership.NotMember, repeated.MembershipOf("O1"));
        Assert.True(repeated.CompareWith(plain).IsMatch);
        Assert.True(plain.CompareWith(repeated).IsMatch);
    }

    [Fact]
    public void AKeyRepeatedWithDifferentTextNeverMatchesInEitherDirectionAndInEitherOrder()
    {
        var baseline = DraftKit.Base(DraftKit.Mutable("D1", "Front", "S1", "{a}"));
        var externalFirst = DraftKit.Base(DraftKit.Mutable("D1", "Front", "S1", "{b external}"), DraftKit.Mutable("D1", "Front", "S1", "{a}"));
        var externalLast = DraftKit.Base(DraftKit.Mutable("D1", "Front", "S1", "{a}"), DraftKit.Mutable("D1", "Front", "S1", "{b external}"));

        Assert.False(baseline.CompareWith(externalFirst).IsMatch);
        Assert.False(baseline.CompareWith(externalLast).IsMatch);
        Assert.False(externalFirst.CompareWith(baseline).IsMatch);
        Assert.False(externalLast.CompareWith(baseline).IsMatch);
        Assert.Contains(D("D1", DraftDifferenceKind.Membership), baseline.CompareWith(externalFirst).Differences);
        Assert.True(externalFirst.IsAmbiguous("D1"));
        Assert.True(externalLast.IsAmbiguous("D1"));
        Assert.False(externalFirst.Entries.ContainsKey("D1"));
    }

    [Theory]
    [InlineData("view")]
    [InlineData("section")]
    [InlineData("attribution")]
    public void AKeyRepeatedWithADifferentViewSectionOrAttributionIsAmbiguousAndNeverMatches(string fact)
    {
        var first = DraftKit.Mutable("D1", "Front", "S1", "{a}");
        var second = fact switch
        {
            "view" => DraftKit.Mutable("D1", "Back", "S1", "{a}"),
            "section" => DraftKit.Mutable("D1", "Front", "S2", "{a}"),
            _ => DraftKit.ReadOnly("D1", "Front", "S1", "{a}"),
        };
        var ambiguous = DraftKit.Base(first, second);

        Assert.True(ambiguous.IsAmbiguous("D1"));
        Assert.False(ambiguous.CompareWith(DraftKit.Base(first)).IsMatch);
        Assert.False(ambiguous.CompareWith(DraftKit.Base(second)).IsMatch);
        Assert.False(DraftKit.Base(first).CompareWith(ambiguous).IsMatch);
        Assert.False(DraftKit.Base(second).CompareWith(ambiguous).IsMatch);
    }

    [Fact]
    public void AMemberRepeatedAsNotMemberGivesTheSameResultInEitherOrderAndNeverMatches()
    {
        var memberFirst = DraftKit.Base(DraftKit.Mutable("D1"), DraftKit.NotMember("D1"));
        var notMemberFirst = DraftKit.Base(DraftKit.NotMember("D1"), DraftKit.Mutable("D1"));
        var plain = DraftKit.Base(DraftKit.Mutable("D1"));
        var empty = DraftKit.Base();

        Assert.Equal(memberFirst.IsAmbiguous("D1"), notMemberFirst.IsAmbiguous("D1"));
        Assert.True(memberFirst.IsAmbiguous("D1"));
        Assert.Equal(memberFirst.Entries.Keys.ToArray(), notMemberFirst.Entries.Keys.ToArray());
        Assert.Equal(memberFirst.CompareWith(plain).Differences.ToArray(), notMemberFirst.CompareWith(plain).Differences.ToArray());
        Assert.Equal(memberFirst.CompareWith(empty).Differences.ToArray(), notMemberFirst.CompareWith(empty).Differences.ToArray());
        Assert.Equal(plain.CompareWith(memberFirst).Differences.ToArray(), plain.CompareWith(notMemberFirst).Differences.ToArray());
        Assert.False(memberFirst.CompareWith(plain).IsMatch);
        Assert.False(memberFirst.CompareWith(empty).IsMatch);
        Assert.False(plain.CompareWith(memberFirst).IsMatch);
        Assert.False(empty.CompareWith(notMemberFirst).IsMatch);
    }

    [Fact]
    public void AnAmbiguousKeyDoesNotMatchEvenAgainstTheSameAmbiguousRead()
    {
        var one = DraftKit.Base(DraftKit.Mutable("D1", text: "{a}"), DraftKit.Mutable("D1", text: "{b}"));
        var two = DraftKit.Base(DraftKit.Mutable("D1", text: "{b}"), DraftKit.Mutable("D1", text: "{a}"));

        Assert.False(one.CompareWith(two).IsMatch);
        Assert.False(one.CompareWith(one).IsMatch);
        Assert.Equal(one.CompareWith(two).Differences.ToArray(), two.CompareWith(one).Differences.ToArray());
    }

    [Fact]
    public void BuildingABaseWithRepeatedKeysNeverThrowsWhateverTheOrderOrTheFacts()
    {
        var entries = new[]
        {
            DraftKit.Mutable("D1", "Front", "S1", "{a}"), DraftKit.ReadOnly("D1"), DraftKit.Blocking("D1"), DraftKit.NotMember("D1"),
            DraftKit.Mutable("D2"), DraftKit.Mutable("D2"), DraftKit.NotMember("D2"), DraftKit.Blocking("D3"), DraftKit.Blocking("D3"),
        };

        var exception = Record.Exception(() =>
        {
            DraftStaleBase.Create(entries);
            DraftStaleBase.Create(entries.Reverse());
        });

        Assert.Null(exception);
        var forward = DraftStaleBase.Create(entries);
        var backward = DraftStaleBase.Create(entries.Reverse());
        Assert.True(forward.IsAmbiguous("D1"));
        Assert.True(backward.IsAmbiguous("D1"));
        Assert.True(forward.IsAmbiguous("D2"));
        Assert.True(backward.IsAmbiguous("D2"));
        Assert.False(forward.IsAmbiguous("D3"));
        Assert.Equal(DraftMembership.Blocking, backward.MembershipOf("D3"));
        Assert.Equal(forward.CompareWith(backward).Differences.ToArray(), backward.CompareWith(forward).Differences.ToArray());
    }

    // ------------------------------------------------------------------ only the text of interpretable members (INV-F4T1-21, M-2)

    [Fact]
    public void ABlockingEntryKeepsNoEnvelopeTextAndTwoReadsThatOnlyDifferInThatTextMatch()
    {
        var one = DraftKit.Base(new DraftStaleBaseEntry("B1", DraftMembership.Blocking, "Front", "S1", "{garbage1}"));
        var two = DraftKit.Base(new DraftStaleBaseEntry("B1", DraftMembership.Blocking, "Front", "S1", "{garbage2}"));

        Assert.Null(one.Entries["B1"].EnvelopeText);
        Assert.Null(two.Entries["B1"].EnvelopeText);
        Assert.True(one.CompareWith(two).IsMatch);
        Assert.True(two.CompareWith(one).IsMatch);
    }

    [Fact]
    public void ABlockingEntryStillComparesItsViewAndSection()
    {
        var one = DraftKit.Base(new DraftStaleBaseEntry("B1", DraftMembership.Blocking, "Front", "S1", null));
        var two = DraftKit.Base(new DraftStaleBaseEntry("B1", DraftMembership.Blocking, "Back", "S2", null));

        Assert.Equal(
            new[] { D("B1", DraftDifferenceKind.View), D("B1", DraftDifferenceKind.Section) },
            one.CompareWith(two).Differences.ToArray());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void AChangeBetweenBlockingAndMutableIsADifference(bool blockingFirst)
    {
        var blocking = DraftKit.Base(DraftKit.Blocking("D1"));
        var mutable = DraftKit.Base(DraftKit.Mutable("D1"));

        var comparison = blockingFirst ? blocking.CompareWith(mutable) : mutable.CompareWith(blocking);

        Assert.False(comparison.IsMatch);
        Assert.Contains(D("D1", DraftDifferenceKind.Membership), comparison.Differences);
    }

    [Fact]
    public void OnlyMutableAndReadOnlyEntriesKeepTheEnvelopeText()
    {
        var staleBase = DraftKit.Base(
            DraftKit.Mutable("M1", text: "{m}"),
            DraftKit.ReadOnly("R1", text: "{r}"),
            new DraftStaleBaseEntry("B1", DraftMembership.Blocking, null, null, "{b}"),
            DraftKit.NotMember("N1", "{n}"));

        Assert.Equal("{m}", staleBase.Entries["M1"].EnvelopeText);
        Assert.Equal("{r}", staleBase.Entries["R1"].EnvelopeText);
        Assert.Null(staleBase.Entries["B1"].EnvelopeText);
        Assert.False(staleBase.Entries.ContainsKey("N1"));
    }

    // ------------------------------------------------------------------ union of keys, absent is NotMember

    [Fact]
    public void TheComparisonWalksTheUnionOfTheKeysOfTheBaseAndOfTheReread()
    {
        var baseline = DraftKit.Base(DraftKit.Mutable("D1", "Front", "S1", "{a}"));
        var reread = DraftKit.Base(DraftKit.Mutable("D2", "Back", "S2", "{b}"));

        var differences = baseline.CompareWith(reread).Differences;

        Assert.Equal(
            new[]
            {
                D("D1", DraftDifferenceKind.Membership), D("D1", DraftDifferenceKind.View), D("D1", DraftDifferenceKind.Section), D("D1", DraftDifferenceKind.EnvelopeText),
                D("D2", DraftDifferenceKind.Membership), D("D2", DraftDifferenceKind.View), D("D2", DraftDifferenceKind.Section), D("D2", DraftDifferenceKind.EnvelopeText),
            },
            differences.ToArray());
    }

    [Fact]
    public void AnAbsentKeyIsTheSameAsNotMemberInBothDirections()
    {
        var withNotMember = DraftKit.Base(DraftKit.Mutable("D1"), DraftKit.NotMember("O1", "{x}"));
        var without = DraftKit.Base(DraftKit.Mutable("D1"));
        Assert.Equal(DraftMembership.Mutable, withNotMember.MembershipOf("D1"));
        Assert.Equal(DraftMembership.NotMember, withNotMember.MembershipOf("O1"));

        Assert.True(withNotMember.CompareWith(without).IsMatch);
        Assert.True(without.CompareWith(withNotMember).IsMatch);
    }

    [Fact]
    public void AnAbsentKeyDiffersFromAMemberBecauseItIsNotMember()
    {
        var withMember = DraftKit.Base(DraftKit.Mutable("D1"), DraftKit.Mutable("D2", "Back", null, null));
        var without = DraftKit.Base(DraftKit.Mutable("D1"));

        var differences = withMember.CompareWith(without).Differences;

        Assert.Equal(
            new[] { D("D2", DraftDifferenceKind.Membership), D("D2", DraftDifferenceKind.View) },
            differences.ToArray());
    }

    // ------------------------------------------------------------------ the self-inflicted conflicts are gone (D-12)

    [Fact]
    public void AnotherRackBeingUpdatedIsNotAChangeBecauseItsKeysAreNotMembers()
    {
        var baseline = DraftKit.Base(DraftKit.Mutable("D1"), DraftKit.NotMember("B1", "{rack b v1}"));
        var reread = DraftKit.Base(DraftKit.Mutable("D1"), DraftKit.NotMember("B1", "{rack b v2 after its own update}"));
        Assert.Equal(DraftMembership.Mutable, baseline.MembershipOf("D1"));

        Assert.True(baseline.CompareWith(reread).IsMatch);
    }

    [Fact]
    public void AnotherRackBeingInsertedIsNotAChange()
    {
        var baseline = DraftKit.Base(DraftKit.Mutable("D1"));
        var reread = DraftKit.Base(DraftKit.Mutable("D1"), DraftKit.NotMember("C1"), DraftKit.NotMember("C2"));
        Assert.Equal(DraftMembership.Mutable, reread.MembershipOf("D1"));

        Assert.True(baseline.CompareWith(reread).IsMatch);
    }

    [Fact]
    public void TheTextOfANonMemberThatChangesIsNotAChange()
    {
        var baseline = DraftKit.Base(DraftKit.Mutable("D1"), DraftKit.NotMember("O1", "{one}"));
        var reread = DraftKit.Base(DraftKit.Mutable("D1"), DraftKit.NotMember("O1", "{two}"));
        Assert.Equal(DraftMembership.Mutable, baseline.MembershipOf("D1"));

        Assert.True(baseline.CompareWith(reread).IsMatch);
    }

    [Fact]
    public void RemovingAFondoOutsideTheDraftIsReportedByItsKeyAndTheDraftEditsNeverMoveTheBase()
    {
        var baseline = DraftKit.Base(
            DraftKit.Mutable("D1"),
            DraftKit.Mutable("D2", "Back", "S2", "{two}"),
            DraftKit.NotMember("O1", "{other rack}"),
            DraftKit.NotMember("O2", "{another rack}"));
        var draft = DraftKit.Born(staleBase: baseline);
        // The user removes a fondo in the draft: only the current state moves, never the base.
        draft.SetCurrentDraftState(draft.CurrentDraftState with { Name = "one fondo less" });
        Assert.Same(baseline, draft.StaleBase);

        // The re-read comes from a DIFFERENT set of entries: the fondo D2 was removed outside the draft, and the keys of
        // other racks changed (O1 gone, O2 rewritten, O3 inserted).
        var rereadAfterExternalRemoval = DraftKit.Base(
            DraftKit.Mutable("D1"),
            DraftKit.NotMember("O2", "{another rack, rewritten}"),
            DraftKit.NotMember("O3", "{a new rack}"));
        var differences = draft.StaleBase.CompareWith(rereadAfterExternalRemoval).Differences;

        Assert.NotEmpty(differences);
        Assert.All(differences, d => Assert.Equal("D2", d.Key));
        Assert.Contains(D("D2", DraftDifferenceKind.Membership), differences);
        Assert.Contains(D("D2", DraftDifferenceKind.EnvelopeText), differences);
    }

    [Fact]
    public void ReferenceCountsAreNotPartOfTheBaseNorOfItsInput()
    {
        var entryProperties = typeof(DraftStaleBaseEntry).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        Assert.All(entryProperties, p => Assert.True(p.PropertyType == typeof(string) || p.PropertyType == typeof(DraftMembership), p.Name));
        Assert.DoesNotContain(entryProperties, p => p.Name.Contains("Count", StringComparison.OrdinalIgnoreCase) || p.Name.Contains("Reference", StringComparison.OrdinalIgnoreCase));

        var publicNames = typeof(DraftStaleBase).GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).Select(m => m.Name).ToArray();
        Assert.DoesNotContain(publicNames, n => n.Contains("Count", StringComparison.OrdinalIgnoreCase) || n.Contains("Reference", StringComparison.OrdinalIgnoreCase));

        // The counts of references live in the keys of the OTHER racks (NotMember), which come and go and change with each read:
        // the two reads are built from different sets of entries and still match, because no count is part of the base.
        var first = DraftKit.Base(
            DraftKit.Mutable("D1"),
            DraftKit.Mutable("D2", "Back", "S2", "{two}"),
            DraftKit.NotMember("U1", "{used by 3 racks}"),
            DraftKit.NotMember("U2", "{used by 1 rack}"));
        var second = DraftKit.Base(
            DraftKit.Mutable("D1"),
            DraftKit.Mutable("D2", "Back", "S2", "{two}"),
            DraftKit.NotMember("U2", "{used by 2 racks}"),
            DraftKit.NotMember("U3", "{used by 1 rack}"));

        Assert.True(first.CompareWith(second).IsMatch);
        Assert.True(second.CompareWith(first).IsMatch);
    }

    // ------------------------------------------------------------------ each fact that counts

    [Fact]
    public void AChangeOfTheViewOfAMemberIsReportedAsAViewDifferenceOnly()
    {
        var baseline = DraftKit.Base(DraftKit.Mutable("D1", "Front", "S1", "{a}"));
        var reread = DraftKit.Base(DraftKit.Mutable("D1", "Back", "S1", "{a}"));

        Assert.Equal(new[] { D("D1", DraftDifferenceKind.View) }, baseline.CompareWith(reread).Differences.ToArray());
    }

    [Fact]
    public void AChangeOfTheSectionOfAMemberIsReportedAsASectionDifferenceOnly()
    {
        var baseline = DraftKit.Base(DraftKit.Mutable("D1", "Front", "S1", "{a}"));
        var reread = DraftKit.Base(DraftKit.Mutable("D1", "Front", "S2", "{a}"));

        Assert.Equal(new[] { D("D1", DraftDifferenceKind.Section) }, baseline.CompareWith(reread).Differences.ToArray());
    }

    [Theory]
    [InlineData(DraftMembership.Mutable, DraftMembership.ReadOnly)]
    [InlineData(DraftMembership.ReadOnly, DraftMembership.Mutable)]
    public void AChangeOfTheAttributionBetweenInterpretableMembersIsReportedAsAMembershipDifferenceOnly(DraftMembership before, DraftMembership after)
    {
        var baseline = DraftKit.Base(new DraftStaleBaseEntry("D1", before, "Front", "S1", "{a}"));
        var reread = DraftKit.Base(new DraftStaleBaseEntry("D1", after, "Front", "S1", "{a}"));

        Assert.Equal(new[] { D("D1", DraftDifferenceKind.Membership) }, baseline.CompareWith(reread).Differences.ToArray());
    }

    [Theory]
    [InlineData(DraftMembership.Mutable, DraftMembership.Blocking)]
    [InlineData(DraftMembership.Blocking, DraftMembership.ReadOnly)]
    public void AChangeOfTheAttributionToOrFromBlockingAlsoLosesOrGainsTheEnvelopeText(DraftMembership before, DraftMembership after)
    {
        var baseline = DraftKit.Base(new DraftStaleBaseEntry("D1", before, "Front", "S1", "{a}"));
        var reread = DraftKit.Base(new DraftStaleBaseEntry("D1", after, "Front", "S1", "{a}"));

        Assert.Equal(
            new[] { D("D1", DraftDifferenceKind.Membership), D("D1", DraftDifferenceKind.EnvelopeText) },
            baseline.CompareWith(reread).Differences.ToArray());
    }

    [Fact]
    public void AMemberThatBecomesNotMemberIsReportedAsAMembershipDifferenceAmongOthers()
    {
        var baseline = DraftKit.Base(DraftKit.Mutable("D1"));
        var reread = DraftKit.Base(DraftKit.NotMember("D1"));

        Assert.Contains(D("D1", DraftDifferenceKind.Membership), baseline.CompareWith(reread).Differences);
        Assert.False(baseline.CompareWith(reread).IsMatch);
    }

    [Theory]
    [InlineData("{\"v\":1}", "{\"v\":2}")]
    [InlineData("{\"v\":1}", "{\"V\":1}")]
    [InlineData("{\"v\":1}", "{ \"v\":1}")]
    [InlineData("{\"v\":1}", "{\"v\":1}\n")]
    [InlineData("{\"v\":1}\r\n", "{\"v\":1}\n")]
    [InlineData("café", "café")]
    [InlineData("{\"v\":1}", "")]
    public void TheComparisonOfTheEnvelopeTextIsOrdinalAndExactWithNoNormalization(string before, string after)
    {
        var baseline = DraftKit.Base(DraftKit.Mutable("D1", text: before));
        var reread = DraftKit.Base(DraftKit.Mutable("D1", text: after));

        Assert.Equal(new[] { D("D1", DraftDifferenceKind.EnvelopeText) }, baseline.CompareWith(reread).Differences.ToArray());
    }

    [Fact]
    public void ReadOnlyMembersAlsoCompareTheirEnvelopeText()
    {
        var baseline = DraftKit.Base(DraftKit.ReadOnly("X1", text: "{one}"));
        var reread = DraftKit.Base(DraftKit.ReadOnly("X1", text: "{two}"));

        Assert.Equal(new[] { D("X1", DraftDifferenceKind.EnvelopeText) }, baseline.CompareWith(reread).Differences.ToArray());
    }

    [Fact]
    public void DefinitionKeysCompareOrdinally()
    {
        var baseline = DraftKit.Base(DraftKit.Mutable("D1"));
        var reread = DraftKit.Base(DraftKit.Mutable("d1"));

        var differences = baseline.CompareWith(reread).Differences;

        Assert.Contains(D("D1", DraftDifferenceKind.Membership), differences);
        Assert.Contains(D("d1", DraftDifferenceKind.Membership), differences);
    }

    [Fact]
    public void TheDifferencesComeOrderedByKeyThenKind()
    {
        var baseline = DraftKit.Base(
            DraftKit.Mutable("B", "Front", "S1", "{b}"),
            DraftKit.Mutable("A", "Front", "S1", "{a}"));
        var reread = DraftKit.Base(
            DraftKit.Mutable("B", "Back", "S1", "{b2}"),
            DraftKit.Mutable("A", "Back", "S2", "{a}"));

        Assert.Equal(
            new[]
            {
                D("A", DraftDifferenceKind.View), D("A", DraftDifferenceKind.Section),
                D("B", DraftDifferenceKind.View), D("B", DraftDifferenceKind.EnvelopeText),
            },
            baseline.CompareWith(reread).Differences.ToArray());
    }

    // ------------------------------------------------------------------ the raw variable registry

    [Fact]
    public void TheRawVariableRegistryIsKeptWhenTheAdapterSuppliesItAndCompared()
    {
        var one = DraftStaleBase.Create(new[] { DraftKit.Mutable("D1") }, "x = 1\r\ny = 2");
        var same = DraftStaleBase.Create(new[] { DraftKit.Mutable("D1") }, "x = 1\r\ny = 2");
        var changed = DraftStaleBase.Create(new[] { DraftKit.Mutable("D1") }, "x = 1\ny = 2");

        Assert.Equal("x = 1\r\ny = 2", one.VariableRegistryText);
        Assert.True(one.CompareWith(same).IsMatch);
        Assert.Equal(new[] { D(null, DraftDifferenceKind.VariableRegistry) }, one.CompareWith(changed).Differences.ToArray());
    }

    [Fact]
    public void ABaseWithoutARegistryMatchesAnotherWithoutOneAndDiffersFromOneWithIt()
    {
        var none = DraftKit.Base(DraftKit.Mutable("D1"));
        var otherNone = DraftKit.Base(DraftKit.Mutable("D1"));
        var some = DraftStaleBase.Create(new[] { DraftKit.Mutable("D1") }, "x = 1");

        Assert.Null(none.VariableRegistryText);
        Assert.True(none.CompareWith(otherNone).IsMatch);
        Assert.Equal(new[] { D(null, DraftDifferenceKind.VariableRegistry) }, none.CompareWith(some).Differences.ToArray());
    }

    [Fact]
    public void TheRegistryDifferenceComesLastAfterTheKeyedOnes()
    {
        var one = DraftStaleBase.Create(new[] { DraftKit.Mutable("D1", text: "{a}") }, "x = 1");
        var two = DraftStaleBase.Create(new[] { DraftKit.Mutable("D1", text: "{b}") }, "x = 2");

        Assert.Equal(
            new[] { D("D1", DraftDifferenceKind.EnvelopeText), D(null, DraftDifferenceKind.VariableRegistry) },
            one.CompareWith(two).Differences.ToArray());
    }

    // ------------------------------------------------------------------ the base only detects (INV-F4T1-07)

    [Fact]
    public void NoPublicMemberOfTheBaseBuildsAnEditableStateADraftOrWriteData()
    {
        var allowed = new HashSet<Type>
        {
            typeof(DraftStaleBase), typeof(DraftStaleComparison), typeof(DraftMembership), typeof(string), typeof(bool), typeof(int),
            typeof(IReadOnlyDictionary<string, DraftStaleBaseEntry>), typeof(IEnumerable<DraftStaleBaseEntry>), typeof(void),
        };

        foreach (var method in typeof(DraftStaleBase).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
        {
            Assert.False(method.IsGenericMethodDefinition, method.Name);
            Assert.Contains(method.ReturnType, allowed);
            Assert.All(method.GetParameters(), p => Assert.Contains(p.ParameterType, allowed));
        }

        foreach (var property in typeof(DraftStaleBase).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
            Assert.Contains(property.PropertyType, allowed);

        Assert.Empty(typeof(DraftStaleBase).GetConstructors(BindingFlags.Public | BindingFlags.Instance));
        Assert.False(typeof(DraftStaleBase).IsGenericType);
    }

    [Fact]
    public void TheBaseIsImmutableSoItsFactsNeverChangeAfterItIsBuilt()
    {
        var entries = new List<DraftStaleBaseEntry> { DraftKit.Mutable("D1") };
        var staleBase = DraftStaleBase.Create(entries);

        entries.Add(DraftKit.Mutable("D2"));
        entries[0] = DraftKit.Mutable("D1", "Changed");

        Assert.Single(staleBase.Entries);
        Assert.Equal("Front", staleBase.Entries["D1"].View);
        Assert.All(typeof(DraftStaleBase).GetProperties(BindingFlags.Public | BindingFlags.Instance), p => Assert.True(p.SetMethod == null || !p.SetMethod.IsPublic, p.Name));
    }
}
