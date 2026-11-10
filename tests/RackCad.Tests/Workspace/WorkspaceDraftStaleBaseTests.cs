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
    public void ARepeatedKeyKeepsTheLastEntryAndANullOrEmptyKeyIsAProgrammingError()
    {
        var staleBase = DraftKit.Base(DraftKit.Mutable("D1", "Front"), DraftKit.Mutable("D1", "Back"));

        Assert.Single(staleBase.Entries);
        Assert.Equal("Back", staleBase.Entries["D1"].View);
        Assert.Throws<ArgumentException>(() => DraftKit.Base(DraftKit.Mutable("")));
        Assert.Throws<ArgumentNullException>(() => DraftStaleBase.Create(null!));
        Assert.Throws<ArgumentException>(() => staleBase.MembershipOf(""));
        Assert.Throws<ArgumentNullException>(() => staleBase.CompareWith(null!));
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
        var baseline = DraftKit.Base(DraftKit.Mutable("D1"), DraftKit.Mutable("D2", "Back", "S2", "{two}"));
        var draft = DraftKit.Born(staleBase: baseline);
        // The user removes a fondo in the draft: only the current state moves, never the base.
        draft.SetCurrentDraftState(draft.CurrentDraftState with { Name = "one fondo less" });

        Assert.True(baseline.CompareWith(draft.StaleBase).IsMatch);
        Assert.Same(baseline, draft.StaleBase);

        var rereadAfterExternalRemoval = DraftKit.Base(DraftKit.Mutable("D1"));
        var differences = baseline.CompareWith(rereadAfterExternalRemoval).Differences;

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

        // Two reads of the same facts that would differ only in how many references the drawing holds are identical for the base.
        var first = DraftKit.Base(DraftKit.Mutable("D1"), DraftKit.Mutable("D2", "Back", "S2", "{two}"));
        var second = DraftKit.Base(DraftKit.Mutable("D1"), DraftKit.Mutable("D2", "Back", "S2", "{two}"));
        Assert.True(first.CompareWith(second).IsMatch);
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
    [InlineData(DraftMembership.Mutable, DraftMembership.Blocking)]
    [InlineData(DraftMembership.Blocking, DraftMembership.ReadOnly)]
    public void AChangeOfTheAttributionOfAMemberIsReportedAsAMembershipDifference(DraftMembership before, DraftMembership after)
    {
        var baseline = DraftKit.Base(new DraftStaleBaseEntry("D1", before, "Front", "S1", "{a}"));
        var reread = DraftKit.Base(new DraftStaleBaseEntry("D1", after, "Front", "S1", "{a}"));

        Assert.Equal(new[] { D("D1", DraftDifferenceKind.Membership) }, baseline.CompareWith(reread).Differences.ToArray());
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
