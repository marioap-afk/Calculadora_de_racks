#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.PushBack.Pilot;
using RackCad.Application.Views.Redraw;
using RackCad.Application.Workspace.Drafts;
using Xunit;

namespace RackCad.Tests;

// I-64 F4-T4B-MUTATE-POST (INV-F4T4B-01, 02, 03, 05, 06, 10, 16) tested here the run of the update of the pilot, which PB-T4d retires.
// I-64 F4B PB-T4d (recomprobacion 1 de CA-T4d, M-12): what stays is the rule of the names of the views of a composite rack (DesiredNamesOf), over a plan
// built by hand; the run itself is tested over the full model in PushBackRetirePilotRunTests.
public class WorkspacePilotUpdateRunTests
{
    // I-64 F4B PB-T4d (CA-T4d M-03): the tests of the run of the pilot go (29 of them have their substitute over the full model in
    // PushBackRetirePilotRunTests), and with them their helpers and the titles of their sections; what stays is the rule of the names of a
    // composite rack, over a plan built by hand.

    // M-06 (d): the names of the views of a composite rack carry the side of each cut. The pilot refuses a composite rack (its reader says it is
    // out of the pilot), so the rule is exercised on the plan itself, built by hand over the composite fixture, and the names are compared with
    // the rule of the classic editor for the same cuts.
    [Fact]
    public void TheDesiredNamesOfACompositeRackCarryTheSideOfEachCutAndAPosteriorCutKeepsItsOwnName()
    {
        const string fixture = "pb-20-ab";
        var members = new List<PushBackPilotMemberPlan>();
        foreach (var section in new[] { 0, 1, 2, 3 })
        {
            var text = PrepareKit.Frontal(fixture, section, "Mi rack");
            var embed = new RackEmbedStore().Deserialize(text)!;
            members.Add(new PushBackPilotMemberPlan("K" + section, embed.View, embed.Section, embed.Design, text));
        }

        var plan = new PushBackPilotUpdatePlan(
            PrepareKit.Rack, "K0", Array.Empty<DraftStaleBaseEntry>(), null, 2.25, members,
            new PushBackPilotPostExpectation(2.25, Array.Empty<PushBackPilotKeyedText>()));

        var names = PushBackPilotUpdateRun.DesiredNamesOf(plan, PilotKit.Catalog);

        Assert.Equal(members.Count, names.Count);
        Assert.All(names.Values, name => Assert.Matches(@"^Mi rack - frontal (entrada-salida|posterior) [AB]$", name));
        Assert.Contains(names.Values, name => name.EndsWith(" A", StringComparison.Ordinal));
        Assert.Contains(names.Values, name => name.EndsWith(" B", StringComparison.Ordinal));
        Assert.Equal(names.Count, names.Values.Distinct(StringComparer.Ordinal).Count());
    }
}
