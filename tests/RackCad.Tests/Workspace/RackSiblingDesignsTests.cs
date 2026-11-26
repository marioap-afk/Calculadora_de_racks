#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.VentanaRackCad;
using RackCad.Application.Views.Redraw;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests;

// I-64 A-14, line D; M-02 of the review of Selectivo. The door of the RackCad window reads the design of one view of a rack and writes the
// draft in all of them, so it first asks whether the sibling views carry the same design. The rule is generic (any system) and uses the
// authored comparator that already decides AUTHORED_DIVERGENT; it invents no equality of its own.
public class RackSiblingDesignsTests
{
    private const string RackId = "6c1f0a3e-9b2d-4e57-8a41-2f7d3c9b5e10";

    private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

    // Two fronts (3 and 2 levels) with the default safety of a new rack, as the window and the classic editor write it.
    private static DynamicRackDesign Structure()
    {
        var design = new DynamicRackDesign
        {
            Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
            PalletsDeep = 6,
            LoadLevels = 3,
            FirstLevelHeight = 6.0,
            BeamDepth = 4.0,
        };
        design.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 2, LoadLevels = 3, PalletsDeep = 6, DepthStartPosition = 1 });
        design.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 2, PalletsDeep = 4, DepthStartPosition = 3 });
        foreach (var selection in DynamicSafetyDefaults.Build(Catalog)) design.SafetySelections.Add(selection.DeepCopy());
        return design;
    }

    // The inner design of a Dinámico view, recomposed by the draft of the window and serialized as the drawing half of RACKEDITAR does.
    private static string DynamicJson(Action<DynamicRackDesign>? change = null)
    {
        var design = Structure();
        change?.Invoke(design);
        var draft = new DynamicDraftEditor(Catalog);
        draft.LoadFrom(design, "DIN 1");
        Assert.True(draft.Recompose(), draft.LastError);
        return new RackProjectStore().Serialize(RackProject.ForDynamic(draft.Design));
    }

    // One view as the drawing holds it: the envelope composed as a redraw composes it, written and read back as the block finder reads it.
    private static RackSiblingView View(string handle, string kind, string design, string name = "DIN 1",
        string view = RackEmbedDocument.ViewLateral, int section = 0, string? customProperties = null, string? id = null)
    {
        var envelope = RackEmbedComposer.Compose(null, kind, id ?? RackId, name, view, section, design);
        if (customProperties != null) envelope.CustomProperties = JsonDocument.Parse(customProperties).RootElement.Clone();
        var store = new RackEmbedStore();
        return new RackSiblingView(handle, store.Deserialize(store.Serialize(envelope)));
    }

    private static RackSiblingDesignsVerdict Compare(params RackSiblingView[] views) => RackSiblingDesigns.Compare(RackId, views);

    [Fact]
    public void TheViewsOfADinamicoWithTheSameDesignCoincideWhateverViewEachOneDraws()
    {
        var design = DynamicJson();

        var verdict = Compare(
            View("2A1", RackEmbedDocument.KindDynamic, design, view: RackEmbedDocument.ViewLateral, section: 0),
            View("2A2", RackEmbedDocument.KindDynamic, design, view: RackEmbedDocument.ViewLateral, section: 2),
            View("2A3", RackEmbedDocument.KindDynamic, design, view: RackEmbedDocument.ViewFrontal, section: (int)DynamicRackEnd.Entrance),
            View("2A4", RackEmbedDocument.KindDynamic, design, view: RackEmbedDocument.ViewPlanta, section: -1));

        Assert.Equal(RackSiblingDesignsOutcome.Same, verdict.Outcome);
        Assert.True(verdict.Coincide);
        Assert.Null(verdict.Reason);
    }

    [Fact]
    public void TheViewsOfADinamicoWithDifferentDesignsDoNotCoincideInAnyOrder()
    {
        var one = View("2A1", RackEmbedDocument.KindDynamic, DynamicJson());
        var other = View("2A2", RackEmbedDocument.KindDynamic, DynamicJson(d => d.Fronts[0].PalletCount = 3), view: RackEmbedDocument.ViewPlanta, section: -1);

        foreach (var verdict in new[] { Compare(one, other), Compare(other, one) })
        {
            Assert.Equal(RackSiblingDesignsOutcome.Divergent, verdict.Outcome);
            Assert.False(verdict.Coincide);
            Assert.Equal("Las vistas de este rack no tienen el mismo diseño: ábrelo con RACKEDITAR desde la vista que quieras conservar; «Actualizar» las iguala.",
                verdict.Reason);
        }

        // One view of three that differs is enough.
        var third = View("2A3", RackEmbedDocument.KindDynamic, DynamicJson(), section: 1);
        Assert.Equal(RackSiblingDesignsOutcome.Divergent, Compare(one, third, other).Outcome);
    }

    [Fact]
    public void TheNameCountsBecauseTheWindowShowsOneAndWritesItInEveryView()
    {
        var design = DynamicJson();

        var verdict = Compare(
            View("2A1", RackEmbedDocument.KindDynamic, design, name: "DIN 1"),
            View("2A2", RackEmbedDocument.KindDynamic, design, name: "DIN 2", section: 1));

        Assert.Equal(RackSiblingDesignsOutcome.Divergent, verdict.Outcome);
    }

    [Fact]
    public void ASingleViewCoincidesWithoutBeingReadSoTheDoorReadsItAsBefore()
    {
        // Not even a design the comparator would refuse: with one view there is nothing to disagree with.
        Assert.Equal(RackSiblingDesignsOutcome.Same, Compare(View("2A1", RackEmbedDocument.KindDynamic, "{ no es un diseño")).Outcome);
        Assert.Equal(RackSiblingDesignsOutcome.Same, Compare(View("2A1", RackEmbedDocument.KindCama, "{}")).Outcome);
        Assert.True(Compare().Coincide);
        Assert.True(RackSiblingDesigns.Compare(RackId, null!).Coincide);
    }

    [Fact]
    public void WhatTheComparatorIgnoresDoesNotMakeTheViewsDiffer()
    {
        var design = DynamicJson();

        // A documented legacy default: a design written before the annotation scale was persisted reads as scale 1, and the same for the
        // numbering of the fronts (absent = not numbered).
        var legacy = JsonNode.Parse(design)!;
        var system = legacy["DynamicSystem"]!.AsObject();
        Assert.Equal(1.0, system["AnnotationScale"]!.GetValue<double>());
        Assert.False(system["NumberFronts"]!.GetValue<bool>());
        system.Remove("AnnotationScale");
        system.Remove("NumberFronts");
        var legacyDesign = legacy.ToJsonString();
        Assert.NotEqual(design, legacyDesign);

        var verdict = Compare(
            View("2A1", RackEmbedDocument.KindDynamic, design),
            View("2A2", RackEmbedDocument.KindDynamic, legacyDesign, view: RackEmbedDocument.ViewFrontal, section: (int)DynamicRackEnd.Exit,
                // The custom properties of the rack travel with each envelope and are not part of the design.
                customProperties: "{\"SchemaVersion\":\"1.0\",\"Values\":{}}"));

        Assert.Equal(RackSiblingDesignsOutcome.Same, verdict.Outcome);
    }

    [Fact]
    public void AViewTheComparatorCannotReadIsNotEquality()
    {
        var design = DynamicJson();

        // A member the comparator does not know, in the inner design of one view.
        var unknown = JsonNode.Parse(design)!;
        unknown["Extra"] = 1;
        var notRead = Compare(
            View("2A1", RackEmbedDocument.KindDynamic, design),
            View("2A2", RackEmbedDocument.KindDynamic, unknown.ToJsonString(), section: 1));
        Assert.Equal(RackSiblingDesignsOutcome.Unverifiable, notRead.Outcome);
        Assert.False(notRead.Coincide);
        Assert.Equal("No se pudo comprobar que las vistas de este rack tengan el mismo diseño: ábrelo con RACKEDITAR desde la vista que quieras conservar.",
            notRead.Reason);
        Assert.False(string.IsNullOrWhiteSpace(notRead.Diagnostic));

        // Views of different kinds never coincide.
        Assert.Equal(RackSiblingDesignsOutcome.Unverifiable, Compare(
            View("2A1", RackEmbedDocument.KindDynamic, design),
            View("2A2", RackEmbedDocument.KindPushBack, design, section: 1)).Outcome);

        // A kind without a demonstrated comparator is not equality either.
        var cama = new FlowBedConfigurationStore().Serialize(new RackCad.Domain.Systems.FlowBed.FlowBedConfiguration { LaneDepth = 96.0, PalletDepth = 48.0 });
        Assert.Equal(RackSiblingDesignsOutcome.Unverifiable, Compare(
            View("2A1", RackEmbedDocument.KindCama, cama),
            View("2A2", RackEmbedDocument.KindCama, cama, section: 1)).Outcome);

        // Nor a view without an envelope.
        Assert.Equal(RackSiblingDesignsOutcome.Unverifiable, Compare(
            View("2A1", RackEmbedDocument.KindDynamic, design),
            new RackSiblingView("2A2", null!)).Outcome);
    }

    [Fact]
    public void TheRuleIsTheSameForSelectiveWithItsOwnComparator()
    {
        var design = ComputedParametersSummaryKit.SelectiveJson(6.0, 2, 2);

        Assert.Equal(RackSiblingDesignsOutcome.Same, Compare(
            View("3B1", RackEmbedDocument.KindSelective, design, view: RackEmbedDocument.ViewFrontal, section: -1),
            View("3B2", RackEmbedDocument.KindSelective, design, view: RackEmbedDocument.ViewLateral, section: 1)).Outcome);

        var verdict = Compare(
            View("3B1", RackEmbedDocument.KindSelective, design, view: RackEmbedDocument.ViewFrontal, section: -1),
            View("3B2", RackEmbedDocument.KindSelective, ComputedParametersSummaryKit.SelectiveJson(7.0, 2, 2), view: RackEmbedDocument.ViewLateral, section: 1));
        Assert.Equal(RackSiblingDesignsOutcome.Divergent, verdict.Outcome);
        Assert.Equal(RackSiblingDesigns.DivergentText, verdict.Reason);

        Assert.Equal(RackSiblingDesignsOutcome.Unverifiable, Compare(
            View("3B1", RackEmbedDocument.KindSelective, design),
            View("3B2", RackEmbedDocument.KindSelective, ComputedParametersSummaryKit.Garbage, section: 1)).Outcome);
    }

    [Fact]
    public void TheRuleIsTheSameForPushBack()
    {
        var design = ComputedParametersSummaryKit.PushBackJson();

        Assert.Equal(RackSiblingDesignsOutcome.Same, Compare(
            View("4C1", RackEmbedDocument.KindPushBack, design, name: "PB 1"),
            View("4C2", RackEmbedDocument.KindPushBack, design, name: "PB 1", section: 1)).Outcome);
        Assert.Equal(RackSiblingDesignsOutcome.Divergent, Compare(
            View("4C1", RackEmbedDocument.KindPushBack, design, name: "PB 1"),
            View("4C2", RackEmbedDocument.KindPushBack, design, name: "PB 2", section: 1)).Outcome);
    }

    // I-02 of the review of the fix: the door finds the views of the rack as FindRackBlocks finds them, by the identity without case (the list
    // of the window groups them the same way), and the AUTH-13 comparator reads each identity with case. The same rack in other case is the
    // same rack: only the design and the name decide.
    [Fact]
    public void TheIdentityOfTheRackIsMatchedWithoutCaseAsTheDoorFindsItsViews()
    {
        var design = DynamicJson();
        var upper = RackId.ToUpperInvariant();
        Assert.NotEqual(RackId, upper);

        // The rack asked for in other case than its views, and views that differ in case among themselves.
        Assert.Equal(RackSiblingDesignsOutcome.Same, RackSiblingDesigns.Compare(upper, new[]
        {
            View("2A1", RackEmbedDocument.KindDynamic, design),
            View("2A2", RackEmbedDocument.KindDynamic, design, view: RackEmbedDocument.ViewPlanta, section: -1),
        }).Outcome);
        var mixed = View("2A2", RackEmbedDocument.KindDynamic, design, section: 1, id: upper);
        Assert.Equal(RackSiblingDesignsOutcome.Same, Compare(View("2A1", RackEmbedDocument.KindDynamic, design), mixed).Outcome);
        Assert.Equal(upper, mixed.Envelope.Id); // the envelope of the drawing is not changed

        // The design and the name still decide.
        Assert.Equal(RackSiblingDesignsOutcome.Divergent, Compare(
            View("2A1", RackEmbedDocument.KindDynamic, design),
            View("2A2", RackEmbedDocument.KindDynamic, DynamicJson(d => d.Fronts[0].PalletCount = 3), section: 1, id: upper)).Outcome);
        Assert.Equal(RackSiblingDesignsOutcome.Divergent, Compare(
            View("2A1", RackEmbedDocument.KindDynamic, design),
            View("2A2", RackEmbedDocument.KindDynamic, design, name: "DIN 2", section: 1, id: upper)).Outcome);

        // Another identity is another rack, whatever its case: not this rack's view.
        Assert.Equal(RackSiblingDesignsOutcome.Unverifiable, Compare(
            View("2A1", RackEmbedDocument.KindDynamic, design),
            View("2A2", RackEmbedDocument.KindDynamic, design, section: 1, id: "7D2F0A3E-9B2D-4E57-8A41-2F7D3C9B5E10")).Outcome);

        // The same for Push Back, the other kind of AUTH-13 the window may meet.
        var pushBack = ComputedParametersSummaryKit.PushBackJson();
        Assert.Equal(RackSiblingDesignsOutcome.Same, Compare(
            View("4C1", RackEmbedDocument.KindPushBack, pushBack, name: "PB 1"),
            View("4C2", RackEmbedDocument.KindPushBack, pushBack, name: "PB 1", section: 1, id: upper)).Outcome);
    }

    // I-01 of the review of the fix: the definitions an xref brings must not count, as the membership of production leaves them out (ReadOnly,
    // outside MutableMembers, the set AUTHORED_DIVERGENT compares). The door hands the views of FindRackBlocks, whose scan already skips them
    // by the same fact the scan of the membership classifies (BlockTableRecord.IsFromExternalReference), and matches the identity without
    // case as the membership does: the two sets are the same.
    [Fact]
    public void TheViewsOfTheDoorLeaveOutTheDefinitionsOfAnXrefAsTheMembershipOfProductionDoes()
    {
        var facts = new[]
        {
            new RackSiblingScanFact("2A1", RackEmbedDocument.ViewLateral, false, false, true, RackId, RackId, 1, 0, false),
            new RackSiblingScanFact("2A2", RackEmbedDocument.ViewLateral, false, true, true, RackId, RackId, 1, 0, false),
            new RackSiblingScanFact("2A3", RackEmbedDocument.ViewPlanta, false, false, true, RackId.ToUpperInvariant(), RackId.ToUpperInvariant(), 1, 0, false),
        };
        var membership = RackSiblingMembership.Classify(facts, RackId, null, false);
        Assert.Equal(RackSiblingMembershipKind.ReadOnly, membership.Members.Single(m => m.Fact.DefinitionKey == "2A2").Kind);
        Assert.Equal(new[] { "2A1", "2A3" }, membership.MutableMembers.Select(m => m.Fact.DefinitionKey).ToArray());

        var support = WorkspaceHostSource.Code("src/RackCad.Plugin/RackCommandSupport.cs");
        var find = WorkspaceHostSource.BodyOf(support, @"public\s+static\s+[\w.<>(),\s]+\s+FindRackBlocks\s*\(\s*Document\s+document,\s*string\s+rackId\s*\)");
        Assert.Contains("RackBlockFinder.ScanEnvelopes(transaction, document.Database, includeReferenceCount: false)", find, StringComparison.Ordinal);
        Assert.Matches(@"string\.Equals\(envelope\.Embed\.Id,\s*rackId,\s*System\.StringComparison\.OrdinalIgnoreCase\)", find);

        var finder = WorkspaceHostSource.BodyOf(WorkspaceHostSource.Code("src/RackCad.Plugin/RackBlockFinder.cs"),
            @"public\s+static\s+List<RackEnvelopeScan>\s+ScanEnvelopes\s*\(");
        Assert.Matches(@"if\s*\(\s*record\.IsLayout\s*\|\|\s*record\.IsAnonymous\s*\|\|\s*record\.IsFromExternalReference\s*\)\s*\{\s*continue;\s*\}", finder);

        var traverse = WorkspaceHostSource.BodyOf(WorkspaceHostSource.Code("src/RackCad.Plugin/Views/RackSiblingScan.cs"),
            @"internal\s+static\s+RackSiblingDrawingScan\s+TraverseCore\s*\(");
        Assert.Matches(@"new\s+RackSiblingScanFact\(\s*key,\s*envelope\?\.View,\s*definitionId\s*==\s*selectedDefinition,\s*definition\.IsFromExternalReference,", traverse);
    }

    // ------------------------------------------------------------------ the guard of the door

    // RACKDINAMICOABRIR, RACKDINAMICOAPLICAR and RACKDINAMICOINSERTAR compare the views of the rack (every block FindRackBlocks gives, by
    // its handle) before they read the design of the first one or write anything, and answer with the reason of the rule. RACKDINAMICONUEVO
    // inserts a rack that is not in the drawing, so it has no views to compare.
    [Theory]
    [InlineData("RACKDINAMICOABRIR", "AnswerRead", false)]
    [InlineData("RACKDINAMICOAPLICAR", "AnswerWrite", true)]
    [InlineData("RACKDINAMICOINSERTAR", "AnswerWrite", true)]
    public void TheCommandsOfTheDoorCompareTheViewsBeforeReadingOrWriting(string command, string answer, bool writes)
    {
        var body = CommandBody(command);

        var find = body.IndexOf("RackCommandSupport.FindRackBlocks(document, pending.RackId)", StringComparison.Ordinal);
        var compare = body.IndexOf("RackSiblingDesigns.Compare(pending.RackId, Views(blocks))", StringComparison.Ordinal);
        var firstRead = body.IndexOf("blocks[0]", StringComparison.Ordinal);
        Assert.True(find >= 0, command);
        Assert.True(compare > find, command);
        Assert.True(firstRead > compare, command);
        Assert.Equal(1, WorkspaceHostSource.Count(body, @"RackSiblingDesigns\.Compare\("));
        if (writes)
            Assert.True(body.IndexOf("RackDinamicoCommands.ApplyDynamicEdit(", StringComparison.Ordinal) > compare, command);

        Assert.Matches(@"var\s+siblings\s*=\s*RackSiblingDesigns\.Compare\(pending\.RackId,\s*Views\(blocks\)\);\s*if\s*\(\s*!siblings\.Coincide\s*\)\s*\{\s*door\?\."
                       + answer + @"\(new\s+\w+\(pending\.RequestId,\s*pending\.RackId,\s*false,[^;]*siblings\.Reason\)\);\s*return;\s*\}", body);
    }

    // RACKSELECTIVOABRIR, RACKSELECTIVOAPLICAR and RACKSELECTIVOINSERTAR (entrega 2, added when E2 was integrated with this rule) follow
    // the same rule: they compare the views once, after FindRackBlocks and before they read blocks[0] or hand it to the drawing half,
    // and refuse with the reason of the rule.
    [Theory]
    [InlineData("RACKSELECTIVOABRIR", @"AnswerSelectiveRead\(SelectiveDesignRead\.Refused\(pending\.RequestId,\s*pending\.RackId,\s*null,\s*null,\s*siblings\.Reason\)\)", false)]
    [InlineData("RACKSELECTIVOAPLICAR", @"AnswerWrite\(new\s+RackDesignWritten\(pending\.RequestId,\s*pending\.RackId,\s*false,\s*siblings\.Reason\)\)", true)]
    [InlineData("RACKSELECTIVOINSERTAR", @"AnswerWrite\(new\s+RackDesignWritten\(pending\.RequestId,\s*pending\.RackId,\s*false,\s*siblings\.Reason\)\)", false)]
    public void TheSelectiveCommandsOfTheDoorCompareTheViewsBeforeReadingOrWriting(string command, string answer, bool writes)
    {
        var body = CommandBody(command);

        var find = body.IndexOf("RackCommandSupport.FindRackBlocks(document, pending.RackId)", StringComparison.Ordinal);
        var compare = body.IndexOf("RackSiblingDesigns.Compare(pending.RackId, Views(blocks))", StringComparison.Ordinal);
        var firstRead = body.IndexOf("blocks[0]", StringComparison.Ordinal);
        Assert.True(find >= 0, command);
        Assert.True(compare > find, command);
        Assert.True(firstRead > compare, command);
        Assert.Equal(1, WorkspaceHostSource.Count(body, @"RackSiblingDesigns\.Compare\("));
        if (writes)
            Assert.True(body.IndexOf("RackSelectivoCommands.ApplySelectiveEdit(", StringComparison.Ordinal) > compare, command);

        Assert.Matches(@"var\s+siblings\s*=\s*RackSiblingDesigns\.Compare\(pending\.RackId,\s*Views\(blocks\)\);\s*if\s*\(\s*!siblings\.Coincide\s*\)\s*\{\s*door\?\."
                       + answer + @";\s*return;\s*\}", body);
    }

    [Fact]
    public void TheNewRackHasNoViewsToCompareAndTheViewsAreEveryBlockOfTheRack()
    {
        Assert.DoesNotContain("RackSiblingDesigns", CommandBody("RACKDINAMICONUEVO"), StringComparison.Ordinal);

        var code = WorkspaceHostSource.Code("src/RackCad.Plugin/VentanaRackCad/RackCadRackDoorCommands.cs");
        var views = WorkspaceHostSource.BodyOf(code, @"private\s+static\s+[\w.<>]+\s+Views\s*\(");
        Assert.Matches(@"foreach\s*\(\s*var\s*\(\s*blockId\s*,\s*embed\s*\)\s+in\s+blocks\s*\)\s*views\.Add\(new\s+RackSiblingView\(blockId\.Handle\.ToString\(\),\s*embed\)\);", views);
        // 6 since E2 was integrated: RACKSELECTIVOINSERTAR writes the draft in every view before it inserts, so it compares them too.
        Assert.Equal(6, WorkspaceHostSource.Count(code, @"RackSiblingDesigns\.Compare\("));
    }

    private static string CommandBody(string command)
    {
        var code = WorkspaceHostSource.Code("src/RackCad.Plugin/VentanaRackCad/RackCadRackDoorCommands.cs");
        return WorkspaceHostSource.BodyOf(code, @"\[CommandMethod\(""" + command + @"""[^\]]*\]\s*public\s+void\s+\w+\s*\(\s*\)");
    }
}
