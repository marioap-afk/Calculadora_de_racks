#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Systems.PushBack.Draft;
using RackCad.Application.Systems.PushBack.Pilot;
using RackCad.Application.Workspace;
using RackCad.Application.Workspace.Drafts;
using Xunit;

namespace RackCad.Tests;

// I-64 F4B PB-T4b (D-PBT4-11.3; «Saltar y duplicar», owner-pbt4-decisions.md, pregunta 3): the kit of the copies of the nine classes of the
// panel of F4. Its three types have the members of the three types of the kit of the pilot that they replace (EditIntentFakeReader,
// EditIntentKit and PrepareKit), with the same names and parameters; the draft and its comparer are the ones of the full model (M-04, M-05),
// and the reads keep the types of the reads of the pilot (D-S3). Every read that the reader or the kit hands to the host is
// EditIntentReadResult.FromReads over the same scan, or over the same base, as the read of the pilot. It uses only IntentKit, PilotKit and
// DraftKit, which it does not edit, and it does not depend on the kit of the pilot.

/// <summary>The reader of the copies: it records every call and answers with the read of the pilot and the full read of the same scan.</summary>
internal sealed class FullEditIntentFakeReader : IWorkspaceEditIntentReader
{
    public List<(SessionId Session, string RackId, long? ChosenReference, EditIntentWitness? Witness)> Calls { get; } = new();

    /// <summary>The answer by rack; by default the admitted read of two views of the rack.</summary>
    public Func<string, EditIntentReadResult> Answer { get; set; } = FullEditIntentKit.Admitted;

    public Exception? Throw { get; set; }

    public EditIntentReadResult ReadIntent(SessionId session, string rackId, long? chosenReference, EditIntentWitness? witness)
    {
        Calls.Add((session, rackId, chosenReference, witness));
        if (Throw != null) throw Throw;
        return Answer(rackId);
    }
}

internal static class FullEditIntentKit
{
    public const string RackA = IntentKit.Rack;
    public const string RackB = "22222222-2222-4222-8222-000000000009";

    /// <summary>Two views of the same single-sided design for the rack: admitted by the real readers.</summary>
    public static PushBackPilotIntentScan TwoViews(string rackId, PushBackPilotPreviewWitness? witness = null) =>
        IntentKit.Scan(
            rackId,
            "A1",
            witness,
            IntentKit.Member("A1", IntentKit.Variant("pb-01-base", rackId)),
            IntentKit.Member("B2", IntentKit.Variant("pb-01-base", rackId, view: "lateral", section: 3)));

    public static EditIntentReadResult Admitted(string rackId) => ReadOf(TwoViews(rackId));

    /// <summary>The read of the pilot and the full read of the SAME scan, with the catalog of the kit.</summary>
    public static EditIntentReadResult ReadOf(PushBackPilotIntentScan scan) =>
        EditIntentReadResult.FromReads(
            PushBackPilotIntentReader.Read(scan, PilotKit.Catalog),
            PushBackFullDraftReader.Read(scan, PilotKit.Catalog),
            PilotKit.Catalog);

    /// <summary>A read that is not admitted (the origin is not in the scan) but carries its base.</summary>
    public static EditIntentReadResult OriginMissing(string rackId) =>
        ReadOf(IntentKit.Scan(rackId, "ZZ", IntentKit.Member("A1", IntentKit.Variant("pb-01-base", rackId))));

    /// <summary>The same rack, but a member became unreadable: the base differs from the one of <see cref="Admitted"/>.</summary>
    public static EditIntentReadResult MemberBecameBlocking(string rackId) =>
        ReadOf(IntentKit.Scan(rackId, "A1", IntentKit.Member("A1", IntentKit.Variant("pb-01-base", rackId)), IntentKit.Unreadable("B2", rackId)));
}

/// <summary>
/// The builders of the scans, the full drafts and the rereads of the copies of the tests of PREPARE. The scans and the bases are the ones of the
/// kit of the pilot; a draft is begun with the full read of the same base; PREPARE is the full one (PrepareFull), over the full read of the base
/// of the reread that the test hands in.
/// </summary>
internal static class FullPrepareKit
{
    public const string Rack = IntentKit.Rack;

    public static string Frontal(string fixture = "pb-01-base", int section = 0, string? name = null) =>
        IntentKit.Variant(fixture, Rack, view: "frontal", section: section, name: name);

    public static string Lateral(int post, string fixture = "pb-01-base", string? name = null) =>
        IntentKit.Variant(fixture, Rack, view: "lateral", section: post, name: name);

    public static string Planta(string fixture = "pb-01-base", string? name = null) =>
        IntentKit.Variant(fixture, Rack, view: "planta", section: -1, name: name);

    /// <summary>The first post that has a lateral cut in the system of pb-01-base: a lateral view there is available.</summary>
    public static int ExistingPost()
    {
        var state = PilotKit.State("pb-01-base");
        var design = new PushBackCompositeEditorAssembler(PilotKit.Catalog).BuildDesign(state.Editor, state.Inputs);
        var system = new PushBackResolver(PilotKit.Catalog).Resolve(design);
        return new PushBackSystemLateralBuilder().Cortes(system, PilotKit.Catalog).First().PostIndex;
    }

    public const int MissingPost = 999;

    public static PushBackPilotIntentScan Scan(params IntentItem[] members) => IntentKit.Scan(Rack, "A1", members);

    /// <summary>The usual realistic rack: the frontal view (the origin), a lateral one and the plan view, all of the same design.</summary>
    public static PushBackPilotIntentScan RealScan(string fixture = "pb-01-base") =>
        Scan(
            IntentKit.Member("A1", Frontal(fixture)),
            IntentKit.Member("B2", Lateral(ExistingPost(), fixture)),
            IntentKit.Member("C3", Planta(fixture)));

    public static DraftStaleBase BaseOf(params (string Key, string Text)[] members) =>
        DraftStaleBase.Create(members.Select(m =>
        {
            var embed = new RackEmbedStore().Deserialize(m.Text)!;
            return new DraftStaleBaseEntry(m.Key, DraftMembership.Mutable, embed.View, embed.Section.ToString(CultureInfo.InvariantCulture), m.Text);
        }));

