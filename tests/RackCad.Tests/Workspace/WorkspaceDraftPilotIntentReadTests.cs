#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.PushBack.Pilot;
using RackCad.Application.Workspace.Drafts;
using Xunit;

namespace RackCad.Tests;

// I-64 F4-T2B1-INTENT-READ: INV-F4T2B1-04..10 and 12 (the result, the 14 rules of the admission in order, INV-16, INV-17,
// the single read, AUTH-13 with the real raw text, the preview witness and the catalog).
public class WorkspaceDraftPilotIntentReadTests
{
    private const string Rack = IntentKit.Rack;

    // A rack Id with letters, so that an Id that differs only in case exists.
    private const string LetterRack = "abcdefab-1111-4111-8111-00000000000a";
    private const string OtherRack = "22222222-2222-4222-8222-000000000009";

    private static string BaseText() => IntentKit.BaseText();

    private static string Lateral(string id = Rack, string fixture = "pb-01-base", int section = 3, string? name = null, string? kind = null) =>
        IntentKit.Variant(fixture, id, view: "lateral", section: section, name: name, kind: kind);

    private static PushBackPilotIntentResult Refuses(PushBackPilotIntentScan scan, string diagnostic, string? detail = null, string? detailContains = null)
    {
        var result = IntentKit.Read(scan);

        Assert.False(result.Read.IsAdmitted);
        Assert.Equal(diagnostic, result.Read.Admission.Diagnostic);
        Assert.Null(result.Read.InitialState);
        Assert.Null(result.Read.StaleBase);
        Assert.NotNull(result.StaleBase);
        Assert.Equal(PushBackPilotIntentReader.ClassicEditorHint, result.ClassicEditorHint);
        Assert.Equal(scan.OriginKey, result.OriginKey);
        if (detail != null) Assert.Equal(detail, result.Detail);
        else if (detailContains != null)
        {
            Assert.NotNull(result.Detail);
            Assert.Contains(detailContains, result.Detail);
        }
        else Assert.Null(result.Detail);

        // The technical detail never goes into the main text, and the main text is always the one of the constant.
        if (result.Detail != null) Assert.DoesNotContain(result.Detail, result.Read.Admission.Diagnostic);
        Assert.StartsWith("No editable aquí: ", result.Read.Admission.Diagnostic);
        return result;
    }

    // ------------------------------------------------------------------ the exact texts (INV-F4T2B1-05)

    [Fact]
    public void TheDiagnosticConstantsHaveTheExactTextsOfThePackage()
    {
        Assert.Equal("No editable aquí: el sobre no tiene identidad", PushBackPilotIntentReader.NoIdentityDiagnostic);
        Assert.Equal("No editable aquí: el origen ya no está en el dibujo", PushBackPilotIntentReader.OriginMissingDiagnostic);
        Assert.Equal("No editable aquí: ilegible o de versión futura", PushBackPilotIntentReader.UnreadableDiagnostic);
        Assert.Equal("No editable aquí: el origen pertenece a otro rack", PushBackPilotIntentReader.OriginNotMemberDiagnostic);
        Assert.Equal("No editable aquí: el origen es una referencia externa", PushBackPilotIntentReader.OriginReadOnlyDiagnostic);
        Assert.Equal("No editable aquí: los miembros del rack son de sistemas distintos", PushBackPilotIntentReader.KindDivergentDiagnostic);
        Assert.Equal("No editable aquí: fuera del piloto (F5)", PushBackPilotIntentReader.OutOfPilotDiagnostic);
        Assert.Equal("No editable aquí: las definiciones del rack no coinciden", PushBackPilotIntentReader.DivergentDiagnostic);
        Assert.Equal("No editable aquí: no se pudo preparar el borrador", PushBackPilotIntentReader.MaterializeDiagnostic);
        Assert.Equal("Usa RACKEDITAR para editarlo con el editor clásico.", PushBackPilotIntentReader.ClassicEditorHint);
    }

    [Fact]
    public void TheUnreadableTextIsTheComposedOneOfTheMaterializerReason()
    {
        // The materializer keeps its own text as the internal reason; the panel only ever shows the composed one.
        var reason = PushBackPilotMaterializer.UnreadableOrFutureDiagnostic;
        var composed = "No editable aquí: " + char.ToLowerInvariant(reason[0]) + reason.Substring(1).TrimEnd('.');

        Assert.Equal(PushBackPilotIntentReader.UnreadableDiagnostic, composed);
        Assert.NotEqual(PushBackPilotIntentReader.UnreadableDiagnostic, reason);
    }

    // ------------------------------------------------------------------ admitted reads

    [Fact]
    public void ASingleMemberOfPb01BaseIsAdmittedWithEverythingOfOneRead()
    {
        var text = BaseText();
        var scan = IntentKit.Scan(Rack, "A1", IntentKit.Member("A1", text));

        var result = IntentKit.Read(scan);

        Assert.True(result.Read.IsAdmitted);
        Assert.True(result.Read.Admission.IsAdmitted);
        Assert.Null(result.Read.Admission.Diagnostic);
        Assert.Null(result.Detail);
        Assert.Null(result.ClassicEditorHint);
        Assert.Equal("A1", result.OriginKey);
        Assert.Equal(PushBackPilotPreviewWitnessStatus.NotSupplied, result.PreviewWitness);
        Assert.NotNull(result.Read.InitialState);
        Assert.Equal(text, result.Read.InitialState!.Origin);
        Assert.Same(result.StaleBase, result.Read.StaleBase);
        Assert.Equal(new[] { "A1" }, result.StaleBase.Entries.Keys.ToArray());
    }

    [Fact]
    public void TwoViewsOfTheSameRackWithTheSameDesignAreAdmitted()
    {
        var result = IntentKit.Read(IntentKit.TwoViews());

        Assert.True(result.Read.IsAdmitted);
        Assert.Equal(2, result.StaleBase.Entries.Count);
        Assert.Equal("lateral", IntentKit.Entry(result.StaleBase, "B2").View);
        Assert.Equal("3", IntentKit.Entry(result.StaleBase, "B2").Section);
    }

    [Fact]
    public void AnOriginThatIsErasedByKindIsAlsoAMutableMemberAndIsAdmitted()
    {
        var result = IntentKit.Read(IntentKit.Scan(Rack, "A1", IntentKit.Member("A1", BaseText(), erase: true)));

        Assert.True(result.Read.IsAdmitted);
        Assert.Equal(DraftMembership.Mutable, IntentKit.Entry(result.StaleBase, "A1").Membership);
    }

    [Theory]
    [InlineData("pb-01-base")]
    [InlineData("pb-02-modulos")]
    [InlineData("pb-03-fondos")]
    [InlineData("pb-04-blanco")]
    [InlineData("pb-05-topes")]
    [InlineData("pb-06-seguridad")]
    [InlineData("pb-07-combinado")]
    [InlineData("pb-08-redondeo")]
    public void EveryCanonicalSingleSidedFixtureIsAdmittedAsTheOnlyMemberOfItsRack(string fixture)
    {
        var text = PilotKit.Text(fixture);
        var id = new RackEmbedStore().Deserialize(text)!.Id!;

        var result = IntentKit.Read(IntentKit.Scan(id, "H1", IntentKit.Member("H1", text)));

        Assert.True(result.Read.IsAdmitted, result.Read.Admission.Diagnostic + " | " + result.Detail);
        Assert.Equal(text, result.Read.InitialState!.Origin);
    }

    [Fact]
    public void KindsThatOnlyDifferInCaseAreTheSameKindForTheMembersOfTheRack()
    {
        var scan = IntentKit.Scan(
            Rack,
            "A1",
            IntentKit.Member("A1", BaseText()),
            IntentKit.Member("B2", Lateral(kind: "PushBack")));

        var result = IntentKit.Read(scan);

        Assert.True(result.Read.IsAdmitted, result.Read.Admission.Diagnostic + " | " + result.Detail);
    }

    // ------------------------------------------------------------------ rules 1 to 6: identity and origin

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData(null)]
    public void Rule1ABlankRackIdIsNoIdentityAndInventsNoRackId(string? rackId)
    {
        var result = Refuses(IntentKit.Scan(rackId, "A1", IntentKit.Member("A1", BaseText())), PushBackPilotIntentReader.NoIdentityDiagnostic);

        Assert.Empty(result.StaleBase.Entries);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    [InlineData("ZZ")]
    public void Rule2AnOriginKeyThatIsBlankOrNotInTheFactsIsMissing(string? originKey)
    {
        var result = Refuses(
            IntentKit.Scan(Rack, originKey, IntentKit.Member("A1", BaseText())),
            PushBackPilotIntentReader.OriginMissingDiagnostic);

        Assert.Equal(originKey ?? string.Empty, result.OriginKey);
        Assert.True(result.StaleBase.Entries.ContainsKey("A1"));
    }

    [Fact]
    public void Rule3AnOriginWhoseEnvelopeIsNotInterpretableIsUnreadable()
    {
        Refuses(IntentKit.Scan(Rack, "A1", IntentKit.Unreadable("A1", Rack)), PushBackPilotIntentReader.UnreadableDiagnostic);
    }

    [Fact]
    public void Rule3AnInterpretableOriginWhoseTextIsMissingOrDoesNotDeserializeIsUnreadable()
    {
        var future = IntentKit.Rewrite(BaseText(), e => e.SchemaVersion = "2.0");

        Refuses(IntentKit.Scan(Rack, "A1", IntentKit.Custom("A1", Rack, "frontal", null)), PushBackPilotIntentReader.UnreadableDiagnostic);
        Refuses(IntentKit.Scan(Rack, "A1", IntentKit.Custom("A1", Rack, "frontal", "{ not json")), PushBackPilotIntentReader.UnreadableDiagnostic);
        Refuses(IntentKit.Scan(Rack, "A1", IntentKit.Custom("A1", Rack, "frontal", "  ")), PushBackPilotIntentReader.UnreadableDiagnostic);
        Refuses(IntentKit.Scan(Rack, "A1", IntentKit.Custom("A1", Rack, "frontal", future)), PushBackPilotIntentReader.UnreadableDiagnostic);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   \t")]
    public void Rule4AnOriginFactWithABlankIdIsNoIdentityAndDoesNotThrow(string id)
    {
        Refuses(
            IntentKit.Scan(Rack, "A1", IntentKit.Custom("A1", id, "frontal", BaseText())),
            PushBackPilotIntentReader.NoIdentityDiagnostic);
    }

    [Fact]
    public void Rule5AnOriginOfAnotherRackIsNotAMember()
    {
        Refuses(
            IntentKit.Scan(Rack, "A1", IntentKit.Member("A1", IntentKit.Variant("pb-02-modulos", OtherRack))),
            PushBackPilotIntentReader.OriginNotMemberDiagnostic);
    }

    [Fact]
    public void Rule5ReadsTheFactAndNotTheDeserializedEnvelope()
    {
        // The text is the one of THIS rack, but the fact attributes the definition to another one.
        Refuses(
            IntentKit.Scan(Rack, "A1", IntentKit.Custom("A1", OtherRack, "frontal", BaseText())),
            PushBackPilotIntentReader.OriginNotMemberDiagnostic);
    }

    [Fact]
    public void Rule6AnOriginFromAnExternalReferenceIsReadOnly()
    {
        Refuses(
            IntentKit.Scan(Rack, "A1", IntentKit.Member("A1", BaseText(), xref: true)),
            PushBackPilotIntentReader.OriginReadOnlyDiagnostic);
    }

    // ------------------------------------------------------------------ rules 7 and 8: the other members

    [Fact]
    public void Rule7ABlockingMemberIsUnreadable()
    {
        var result = Refuses(
            IntentKit.Scan(Rack, "A1", IntentKit.Member("A1", BaseText()), IntentKit.Unreadable("B2", Rack)),
            PushBackPilotIntentReader.UnreadableDiagnostic);

        Assert.Equal(DraftMembership.Blocking, IntentKit.Entry(result.StaleBase, "B2").Membership);
    }

    [Fact]
    public void Rule7AMutableMemberWhoseTextIsMissingOrDoesNotDeserializeIsUnreadable()
    {
        var future = IntentKit.Rewrite(Lateral(), e => e.SchemaVersion = "2.0");

        foreach (var text in new string?[] { null, "{ not json", future })
        {
            Refuses(
                IntentKit.Scan(Rack, "A1", IntentKit.Member("A1", BaseText()), IntentKit.Custom("B2", Rack, "lateral", text)),
                PushBackPilotIntentReader.UnreadableDiagnostic);
        }
    }

    [Fact]
    public void Rule7AReadOnlyMemberDoesNotNeedAReadableText()
    {
        var result = IntentKit.Read(IntentKit.Scan(
            Rack,
            "A1",
            IntentKit.Member("A1", BaseText()),
            IntentKit.Custom("X9", Rack, "lateral", null, xref: true)));

        Assert.True(result.Read.IsAdmitted, result.Read.Admission.Diagnostic);
    }

    [Fact]
    public void Rule8KindsThatDifferBetweenMutableMembersAreDivergentAndNotOutOfPilot()
    {
        var scan = IntentKit.Scan(
            Rack,
            "A1",
            IntentKit.Member("A1", IntentKit.Variant("pb-01-base", Rack, kind: "selective")),
            IntentKit.Member("B2", Lateral()));

        Refuses(scan, PushBackPilotIntentReader.KindDivergentDiagnostic);
    }

    // ------------------------------------------------------------------ rules 9 to 14

    [Theory]
    [InlineData("selective")]
    [InlineData("cama")]
    [InlineData("dynamic")]
    public void Rule9AnOriginThatIsNotPushBackIsOutOfPilotWithItsKindAsTheDetail(string kind)
    {
        Refuses(
            IntentKit.Scan(Rack, "A1", IntentKit.Member("A1", IntentKit.Variant("pb-01-base", Rack, kind: kind))),
            PushBackPilotIntentReader.OutOfPilotDiagnostic,
            detail: kind);
    }

    [Fact]
    public void Rule9TheKindOfTheOriginIsComparedExactlyForThePilot()
    {
        // "PushBack" is the same kind for the members (rule 8), but the path of RACKEDITAR that Materialize follows is exact.
        Refuses(
            IntentKit.Scan(Rack, "A1", IntentKit.Member("A1", IntentKit.Variant("pb-01-base", Rack, kind: "PushBack"))),
            PushBackPilotIntentReader.OutOfPilotDiagnostic,
            detail: "PushBack");
    }

    [Fact]
    public void Rule10TwoMembersWithADifferentDesignAreDivergentWithTheMessageOfTheComparatorAsTheDetail()
    {
        Refuses(
            IntentKit.Scan(Rack, "A1", IntentKit.Member("A1", BaseText()), IntentKit.Member("B2", Lateral(fixture: "pb-02-modulos"))),
            PushBackPilotIntentReader.DivergentDiagnostic,
            detail: "AUTH-13: distinct authored values.");
    }

    [Fact]
    public void Rule10TwoMembersWithADifferentNameAreDivergent()
    {
        Refuses(
            IntentKit.Scan(Rack, "A1", IntentKit.Member("A1", BaseText()), IntentKit.Member("B2", Lateral(name: "ANOTHER NAME"))),
            PushBackPilotIntentReader.DivergentDiagnostic,
            detail: "AUTH-13: distinct authored values.");
    }

    [Fact]
    public void Rule11AnIdThatOnlyDiffersInCaseIsUnreadableWithTheDetailOfTheComparator()
    {
        // Classification attributes without case and AUTH-13 compares the Id exactly: the member is Mutable and then unreadable (M-7).
        var scan = IntentKit.Scan(
            LetterRack,
            "A1",
            IntentKit.Member("A1", IntentKit.Variant("pb-01-base", LetterRack)),
            IntentKit.Member("B2", IntentKit.Variant("pb-01-base", LetterRack.ToUpperInvariant(), view: "lateral", section: 3)));
        Assert.NotEqual(LetterRack, LetterRack.ToUpperInvariant());

        var result = Refuses(scan, PushBackPilotIntentReader.UnreadableDiagnostic, detailContains: "AUTH-13");

        Assert.Equal(DraftMembership.Mutable, IntentKit.Entry(result.StaleBase, "B2").Membership);
    }

    [Fact]
    public void Rule11AnOriginWhoseIdOnlyDiffersInCaseFromTheRackIdIsUnreadable()
    {
        Refuses(
            IntentKit.Scan(LetterRack, "A1", IntentKit.Member("A1", IntentKit.Variant("pb-01-base", LetterRack.ToUpperInvariant()))),
            PushBackPilotIntentReader.UnreadableDiagnostic,
            detailContains: "AUTH-13");
    }

    [Fact]
    public void Rule11RepeatedMembersThatOnlyDifferInCaseAreUnreadableBecauseTheRealRawTextIsPassed()
    {
        // A tolerant deserializer accepts two members "Name" and "name"; the raw reader of AUTH-13 does not (M-5).
        var text = BaseText();
        var repeated = text.Substring(0, text.LastIndexOf('}')) + ",\"name\":\"dup\"}";
        Assert.NotNull(new RackEmbedStore().Deserialize(repeated));

        Refuses(
            IntentKit.Scan(Rack, "A1", IntentKit.Custom("A1", Rack, "frontal", repeated)),
            PushBackPilotIntentReader.UnreadableDiagnostic,
            detailContains: "duplicate or case collision");
    }

    [Fact]
    public void Rule11AnUnknownMemberOfTheEnvelopeIsUnreadable()
    {
        var node = JsonNode.Parse(BaseText())!.AsObject();
        node["Extra"] = 1;
        var withExtra = node.ToJsonString();
        Assert.NotNull(new RackEmbedStore().Deserialize(withExtra));

        Refuses(
            IntentKit.Scan(Rack, "A1", IntentKit.Custom("A1", Rack, "frontal", withExtra)),
            PushBackPilotIntentReader.UnreadableDiagnostic,
            detailContains: "unknown member");
    }

    [Fact]
    public void Rule11AFutureSchemaOfTheInnerProjectIsUnreadable()
    {
        var future = IntentKit.Rewrite(BaseText(), e =>
        {
            var design = JsonNode.Parse(e.Design)!.AsObject();
            design["SchemaVersion"] = "3.0";
            e.Design = design.ToJsonString();
        });

        Refuses(
            IntentKit.Scan(Rack, "A1", IntentKit.Member("A1", future)),
            PushBackPilotIntentReader.UnreadableDiagnostic,
            detailContains: "AUTH-13");
    }

    [Fact]
    public void Rule11TheDetailDoesNotDependOnTheOrderOfTheFacts()
    {
        // A1 fails (Id in another case) and C3 fails (unknown member): the comparator reports the first by key, whatever the order of the facts.
        var node = JsonNode.Parse(Lateral(LetterRack, section: 5))!.AsObject();
        node["Extra"] = 1;
        var items = new[]
        {
            IntentKit.Member("A1", IntentKit.Variant("pb-01-base", LetterRack.ToUpperInvariant(), view: "lateral", section: 4)),
            IntentKit.Member("B2", IntentKit.Variant("pb-01-base", LetterRack)),
            IntentKit.Custom("C3", LetterRack, "lateral", node.ToJsonString()),
        };

        var details = new List<string>();
        foreach (var order in new[] { items, items.Reverse().ToArray(), new[] { items[2], items[0], items[1] }, new[] { items[1], items[2], items[0] } })
        {
            var result = Refuses(IntentKit.Scan(LetterRack, "B2", order), PushBackPilotIntentReader.UnreadableDiagnostic, detailContains: "AUTH-13");
            details.Add(result.Detail!);
        }

        Assert.Single(details.Distinct(StringComparer.Ordinal));
        Assert.Contains("membership or kind mismatch", details[0]);
        Assert.DoesNotContain("unknown member", details[0]);
    }

    [Fact]
    public void Rule12AnABRackIsOutOfPilotWithTheReasonOfTheMaterializer()
    {
        var text = PilotKit.Text("pb-20-ab");
        var id = new RackEmbedStore().Deserialize(text)!.Id!;

        Refuses(
            IntentKit.Scan(id, "H1", IntentKit.Member("H1", text)),
            PushBackPilotIntentReader.OutOfPilotDiagnostic,
            detail: PushBackPilotMaterializer.CompositeOutOfPilotReason);
    }

    [Fact]
    public void Rule12ARackThatParksACompositeTailIsOutOfPilotWithTheReasonOfTheMaterializer()
    {
        var text = PilotKit.Text("pb-21-cola");
        var id = new RackEmbedStore().Deserialize(text)!.Id!;

        Refuses(
            IntentKit.Scan(id, "H1", IntentKit.Member("H1", text)),
            PushBackPilotIntentReader.OutOfPilotDiagnostic,
            detail: PushBackPilotMaterializer.DormantTailOutOfPilotReason);
    }

    [Fact]
    public void Rule14ANullCatalogIsAMaterializeFailureWithTheCatalogReasonAsTheDetail()
    {
        var scan = IntentKit.Scan(Rack, "A1", IntentKit.Member("A1", BaseText()));

        var result = PushBackPilotIntentReader.Read(scan, null);

        Assert.False(result.Read.IsAdmitted);
        Assert.Equal(PushBackPilotIntentReader.MaterializeDiagnostic, result.Read.Admission.Diagnostic);
        Assert.Equal(PushBackPilotMaterializer.CatalogRequiredDiagnostic, result.Detail);
        Assert.Equal(PushBackPilotIntentReader.ClassicEditorHint, result.ClassicEditorHint);
        Assert.NotEmpty(result.StaleBase.Entries);
    }

    /// <summary>A list that throws as soon as anyone looks at it: a catalog that fails in the middle of a load.</summary>
    private sealed class ThrowingList<T> : IReadOnlyList<T>
    {
        public int Count => throw new InvalidOperationException("catalog-boom");

        public T this[int index] => throw new InvalidOperationException("catalog-boom");

        public IEnumerator<T> GetEnumerator() => throw new InvalidOperationException("catalog-boom");

        IEnumerator IEnumerable.GetEnumerator() => throw new InvalidOperationException("catalog-boom");
    }

    [Fact]
    public void Rule14AnExceptionOfTheMaterializerIsAMaterializeFailureWithItsMessageAndIsNeverAnAdmission()
    {
        var real = PilotKit.Catalog;
        var throwing = new RackCatalog
        {
            PostProfiles = new ThrowingList<ProfileCatalogEntry>(),
            BeamProfiles = new ThrowingList<BeamProfileCatalogEntry>(),
            SafetyElements = new ThrowingList<SafetyElementCatalogEntry>(),
            TrussProfiles = real.TrussProfiles,
            BasePlates = real.BasePlates,
            FlowBedProfiles = real.FlowBedProfiles,
            Mensulas = real.Mensulas,
            SpacerProfiles = real.SpacerProfiles,
            ConnectionPoints = real.ConnectionPoints,
            ConnectionLayout = real.ConnectionLayout,
            Views = real.Views,
            Blocks = real.Blocks,
            Defaults = real.Defaults,
        };
        var scan = IntentKit.Scan(Rack, "A1", IntentKit.Member("A1", BaseText()));

        var result = PushBackPilotIntentReader.Read(scan, throwing);

        Assert.False(result.Read.IsAdmitted);
        Assert.Equal(PushBackPilotIntentReader.MaterializeDiagnostic, result.Read.Admission.Diagnostic);
        Assert.NotNull(result.Detail);
        Assert.Contains("catalog-boom", result.Detail);
        Assert.NotEmpty(result.StaleBase.Entries);
    }

    // ------------------------------------------------------------------ the order of the rules (INV-F4T2B1-05, N-3)

    private static IntentItem Blocking() => IntentKit.Unreadable("B2", Rack);

    [Fact]
    public void Rule1ComesBeforeRule2()
    {
        Refuses(IntentKit.Scan(" ", "ZZ", IntentKit.Member("A1", BaseText())), PushBackPilotIntentReader.NoIdentityDiagnostic);
    }

    [Fact]
    public void Rule2ComesBeforeRule7()
    {
        Refuses(IntentKit.Scan(Rack, "ZZ", IntentKit.Member("A1", BaseText()), Blocking()), PushBackPilotIntentReader.OriginMissingDiagnostic);
    }

    [Fact]
    public void Rule4ComesBeforeRule7()
    {
        Refuses(
            IntentKit.Scan(Rack, "A1", IntentKit.Custom("A1", " ", "frontal", BaseText()), Blocking()),
            PushBackPilotIntentReader.NoIdentityDiagnostic);
    }

    [Fact]
    public void Rule5ComesBeforeRule7()
    {
        Refuses(
            IntentKit.Scan(Rack, "A1", IntentKit.Member("A1", IntentKit.Variant("pb-02-modulos", OtherRack)), Blocking()),
            PushBackPilotIntentReader.OriginNotMemberDiagnostic);
    }

    [Fact]
    public void Rule6ComesBeforeRule7()
    {
        Refuses(
            IntentKit.Scan(Rack, "A1", IntentKit.Member("A1", BaseText(), xref: true), Blocking()),
            PushBackPilotIntentReader.OriginReadOnlyDiagnostic);
    }

    [Fact]
    public void Rule7ComesBeforeRule8()
    {
        Refuses(
            IntentKit.Scan(Rack, "A1", IntentKit.Member("A1", BaseText()), Blocking(), IntentKit.Member("C3", Lateral(kind: "selective"))),
            PushBackPilotIntentReader.UnreadableDiagnostic);
    }

    [Fact]
    public void Rule8ComesBeforeRule9AndAMixedRackIsNeverShownAsOutOfPilot()
    {
        var result = Refuses(
            IntentKit.Scan(Rack, "A1", IntentKit.Member("A1", IntentKit.Variant("pb-01-base", Rack, kind: "selective")), IntentKit.Member("B2", Lateral())),
            PushBackPilotIntentReader.KindDivergentDiagnostic);

        Assert.NotEqual(PushBackPilotIntentReader.OutOfPilotDiagnostic, result.Read.Admission.Diagnostic);
    }

    [Fact]
    public void Rule9ComesBeforeRule10()
    {
        // A lone Selective origin is also unreadable for AUTH-13 (it only reads Push Back); the rule 9 verdict wins.
        Refuses(
            IntentKit.Scan(Rack, "A1", IntentKit.Member("A1", IntentKit.Variant("pb-01-base", Rack, kind: "selective"))),
            PushBackPilotIntentReader.OutOfPilotDiagnostic,
            detail: "selective");
    }

    [Fact]
    public void Rule10ComesBeforeRule12()
    {
        // An A/B rack with two members of a different design: it is divergent before it is out of the pilot.
        var ab = PilotKit.Text("pb-20-ab");
        var id = new RackEmbedStore().Deserialize(ab)!.Id!;

        Refuses(
            IntentKit.Scan(id, "H1", IntentKit.Member("H1", ab), IntentKit.Member("H2", IntentKit.Variant("pb-02-modulos", id, view: "lateral", section: 3))),
            PushBackPilotIntentReader.DivergentDiagnostic,
            detail: "AUTH-13: distinct authored values.");
    }

    [Fact]
    public void Rule11ComesBeforeRule12()
    {
        // An A/B rack with a member whose Id only differs in case: it is unreadable before it is out of the pilot.
        var ab = IntentKit.Variant("pb-20-ab", LetterRack);
        var upper = IntentKit.Variant("pb-20-ab", LetterRack.ToUpperInvariant(), view: "lateral", section: 3);

        Refuses(
            IntentKit.Scan(LetterRack, "H1", IntentKit.Member("H1", ab), IntentKit.Member("H2", upper)),
            PushBackPilotIntentReader.UnreadableDiagnostic,
            detailContains: "AUTH-13");
    }

    // ------------------------------------------------------------------ one read (INV-F4T2B1-08)

    [Fact]
    public void TheTextThatIsMaterializedIsTheSameStringThatStaysInTheEntryOfTheOriginOfTheBase()
    {
        // A text with its own instance: only the same reference passes for ReferenceEquals.
        var text = new string(BaseText().AsSpan());
        var lateral = new string(Lateral().AsSpan());
        var scan = IntentKit.Scan(Rack, "A1", IntentKit.Member("A1", text), IntentKit.Member("B2", lateral));

        var result = IntentKit.Read(scan);

        Assert.True(result.Read.IsAdmitted);
        var initial = result.Read.InitialState!;
        Assert.True(ReferenceEquals(initial.Origin, scan.RawEnvelopes["A1"]));
        Assert.True(ReferenceEquals(initial.Origin, IntentKit.Entry(result.StaleBase, "A1").EnvelopeText));
        Assert.True(string.Equals(initial.Origin, IntentKit.Entry(result.StaleBase, "A1").EnvelopeText, StringComparison.Ordinal));
        Assert.True(ReferenceEquals(lateral, IntentKit.Entry(result.StaleBase, "B2").EnvelopeText));
    }

    [Fact]
    public void TheOriginOfTheStateIsTheOriginKeyAndNotTheFirstMember()
    {
        var first = BaseText();
        var second = Lateral();
        var scan = IntentKit.Scan(Rack, "B2", IntentKit.Member("A1", first), IntentKit.Member("B2", second));

        var result = IntentKit.Read(scan);

        Assert.True(result.Read.IsAdmitted);
        Assert.Equal(second, result.Read.InitialState!.Origin);
        Assert.Equal("B2", result.OriginKey);
    }

    // ------------------------------------------------------------------ the preview witness (INV-F4T2B1-10)

    private static PushBackPilotPreviewWitnessStatus Witness(PushBackPilotPreviewWitness? witness, string scanText, string origin = "A1")
    {
        var scan = IntentKit.Scan(Rack, origin, witness, IntentKit.Member("A1", scanText));
        return IntentKit.Read(scan).PreviewWitness;
    }

    [Fact]
    public void WithoutAWitnessTheStatusIsNotSuppliedAndNeverUnchanged()
    {
        Assert.Equal(PushBackPilotPreviewWitnessStatus.NotSupplied, Witness(null, BaseText()));
        Assert.Equal(PushBackPilotPreviewWitnessStatus.NotSupplied, IntentKit.Read(IntentKit.Scan(null, null)).PreviewWitness);
    }

    [Fact]
    public void TheSameKeyAndTheSameRawTextIsUnchangedEvenIfItIsAnotherInstance()
    {
        var text = BaseText();

        Assert.Equal(PushBackPilotPreviewWitnessStatus.Unchanged, Witness(new PushBackPilotPreviewWitness("A1", new string(text.AsSpan())), text));
    }

    [Fact]
    public void AnythingElseIsChanged()
    {
        var text = BaseText();

        // another character
        Assert.Equal(PushBackPilotPreviewWitnessStatus.Changed, Witness(new PushBackPilotPreviewWitness("A1", text.Replace("PB-01", "PB-02", StringComparison.Ordinal)), text));
        // a line ending: CRLF against LF, in both directions
        Assert.Equal(PushBackPilotPreviewWitnessStatus.Changed, Witness(new PushBackPilotPreviewWitness("A1", text + "\n"), text + "\r\n"));
        Assert.Equal(PushBackPilotPreviewWitnessStatus.Changed, Witness(new PushBackPilotPreviewWitness("A1", text + "\r\n"), text + "\n"));
        // a null text
        Assert.Equal(PushBackPilotPreviewWitnessStatus.Changed, Witness(new PushBackPilotPreviewWitness("A1", null), text));
        // another key
        Assert.Equal(PushBackPilotPreviewWitnessStatus.Changed, Witness(new PushBackPilotPreviewWitness("B2", text), text));
        Assert.Equal(PushBackPilotPreviewWitnessStatus.Changed, Witness(new PushBackPilotPreviewWitness("a1", text), text));
        // a text that only differs at the end
        Assert.Equal(PushBackPilotPreviewWitnessStatus.Changed, Witness(new PushBackPilotPreviewWitness("A1", text + " "), text));
    }

    [Fact]
    public void AnOriginThatIsNoLongerThereIsChangedAndSoIsOneWithoutText()
    {
        var text = BaseText();

        // the origin key is not in the scan any more
        var gone = IntentKit.Scan(Rack, "ZZ", new PushBackPilotPreviewWitness("ZZ", text), IntentKit.Member("A1", text));
        Assert.Equal(PushBackPilotPreviewWitnessStatus.Changed, IntentKit.Read(gone).PreviewWitness);

        // the origin is there, but its text could not be read now
        var noText = IntentKit.Scan(Rack, "A1", new PushBackPilotPreviewWitness("A1", text), IntentKit.Custom("A1", Rack, "frontal", null));
        Assert.Equal(PushBackPilotPreviewWitnessStatus.Changed, IntentKit.Read(noText).PreviewWitness);
        var bothNull = IntentKit.Scan(Rack, "A1", new PushBackPilotPreviewWitness("A1", null), IntentKit.Custom("A1", Rack, "frontal", null));
        Assert.Equal(PushBackPilotPreviewWitnessStatus.Changed, IntentKit.Read(bothNull).PreviewWitness);
    }

    [Fact]
    public void TheWitnessIsComputedAlsoWithoutAdmissionAndDoesNotChangeTheBaseNorTheAdmission()
    {
        var text = BaseText();
        var refused = IntentKit.Scan(Rack, "A1", new PushBackPilotPreviewWitness("A1", text), IntentKit.Member("A1", text), IntentKit.Unreadable("B2", Rack));
        var without = IntentKit.Scan(Rack, "A1", IntentKit.Member("A1", text), IntentKit.Unreadable("B2", Rack));

        var result = IntentKit.Read(refused);

        Assert.False(result.Read.IsAdmitted);
        Assert.Equal(PushBackPilotPreviewWitnessStatus.Unchanged, result.PreviewWitness);
        Assert.True(result.StaleBase.CompareWith(IntentKit.Read(without).StaleBase).IsMatch);
        Assert.NotEmpty(result.StaleBase.Entries);

        var admittedChanged = IntentKit.Read(IntentKit.TwoViews(new PushBackPilotPreviewWitness("A1", "{ stale")));
        Assert.True(admittedChanged.Read.IsAdmitted);
        Assert.Equal(PushBackPilotPreviewWitnessStatus.Changed, admittedChanged.PreviewWitness);
    }

    [Fact]
    public void AWitnessWithAnEmptyKeyIsAStructuralError()
    {
        Assert.ThrowsAny<ArgumentException>(() =>
            IntentKit.Scan(Rack, "A1", new PushBackPilotPreviewWitness(string.Empty, "x"), IntentKit.Member("A1", BaseText())));
    }

    // ------------------------------------------------------------------ the result (INV-F4T2B1-04)

    [Fact]
    public void TheClassicEditorHintIsPresentIfAndOnlyIfThereIsNoAdmission()
    {
        var admitted = IntentKit.Read(IntentKit.TwoViews());
        var refused = IntentKit.Read(IntentKit.Scan(Rack, "ZZ", IntentKit.Member("A1", BaseText())));

        Assert.True(admitted.Read.IsAdmitted);
        Assert.Null(admitted.ClassicEditorHint);
        Assert.False(refused.Read.IsAdmitted);
        Assert.Equal("Usa RACKEDITAR para editarlo con el editor clásico.", refused.ClassicEditorHint);
    }

    [Fact]
    public void NoReadOfTheFixturesThrowsAndEveryRefusalCarriesABaseAndTheHint()
    {
        var corpus = new[]
        {
            "pb-01-base", "pb-02-modulos", "pb-03-fondos", "pb-04-blanco", "pb-05-topes", "pb-06-seguridad", "pb-07-combinado", "pb-08-redondeo",
            "pb-10-sin-datum", "pb-11-sin-i41", "pb-12-poste-nulo", "pb-13-sideb-no-presente", "pb-14-peralte-heredado", "pb-15-sin-composite",
            "pb-20-ab", "pb-21-cola", "pb-31-v2-modulos",
        };

        foreach (var fixture in corpus)
        {
            var text = PilotKit.Text(fixture);
            var id = new RackEmbedStore().Deserialize(text)!.Id!;

            var result = IntentKit.Read(IntentKit.Scan(id, "H1", IntentKit.Member("H1", text)));

            Assert.NotNull(result.StaleBase);
            Assert.Equal(1, result.StaleBase.Entries.Count);
            Assert.Equal(result.Read.IsAdmitted, result.ClassicEditorHint == null);
            Assert.Equal(result.Read.IsAdmitted, result.Read.Admission.Diagnostic == null);
        }
    }

    [Fact]
    public void ThePublicTypesOfTheReadAreImmutable()
    {
        foreach (var type in new[] { typeof(PushBackPilotIntentScan), typeof(PushBackPilotIntentResult), typeof(PushBackPilotPreviewWitness) })
        {
            var settable = type.GetProperties()
                .Where(p => p.SetMethod != null && p.SetMethod.IsPublic && !IsInitOnly(p))
                .Select(p => p.Name)
                .ToList();
            var fields = type.GetFields().Where(f => !f.IsStatic && !f.IsInitOnly).Select(f => f.Name).ToList();

            Assert.Empty(settable);
            Assert.Empty(fields);
        }

        Assert.True(typeof(PushBackPilotIntentScan).IsSealed);
        Assert.True(typeof(PushBackPilotIntentResult).IsSealed);
        Assert.True(typeof(PushBackPilotIntentReader).IsAbstract && typeof(PushBackPilotIntentReader).IsSealed);
    }

    private static bool IsInitOnly(System.Reflection.PropertyInfo property) =>
        property.SetMethod!.ReturnParameter.GetRequiredCustomModifiers().Any(m => m.FullName == "System.Runtime.CompilerServices.IsExternalInit");
}
