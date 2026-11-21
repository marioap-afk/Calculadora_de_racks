using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Systems.PushBack.Pilot;
using Xunit;

namespace RackCad.Tests;

// I-64 F4B PB-T4b, DIAGNOSTICO (no entra en ningun commit): lo que responde el PREPARE del piloto y el del modelo completo a los casos de
// WPP InvalidE_AMutableMemberThatIsNotAvailable... y ThePilotNeverErasesAViewAPhantom..., con los mismos kits que el original y que su copia.
public class TmpPhantomProbe
{
    [Fact]
    public void ThePrepareOfThePilotAndOfTheFullModelOverTheCasesOfTheTwoCopies()
    {
        var lines = new List<string>();
        var originFrontal = PrepareKit.Frontal();
        var cases = new (string Name, string Text)[]
        {
            ("phantom-lateral", PrepareKit.Lateral(PrepareKit.MissingPost)),
            ("side-b", PrepareKit.Frontal(section: 2)),
            ("corrupt-descriptor", PilotKit.Text("pb-01-base")),
            ("section-out-of-range", PrepareKit.Frontal(section: 9)),
            ("existing-lateral", PrepareKit.Lateral(PrepareKit.ExistingPost())),
        };
        foreach (var (name, text) in cases)
        {
            var members = PrepareKit.BaseOf(("A1", originFrontal), ("B2", text));
            var pilot = PrepareKit.DraftOf(members, originFrontal);
            PrepareKit.SetScale(pilot, 2.25);
            lines.Add("piloto   " + name + " " + Describe(PrepareKit.Prepare(pilot, PrepareKit.Reread(members))));

            var fullMembers = FullPrepareKit.BaseOf(("A1", originFrontal), ("B2", text));
            var full = FullPrepareKit.DraftOf(fullMembers, originFrontal);
            FullPrepareKit.SetScale(full, 2.25);
            lines.Add("completo " + name + " " + Describe(FullPrepareKit.Prepare(full, FullPrepareKit.Reread(fullMembers))));
        }

        lines.Add("prefijos ViewUnavailable=[" + PushBackPilotUpdatePlanner.ViewUnavailablePrefix + "] FrontalBOfOneWay=[" + PushBackPilotUpdatePlanner.FrontalBOfOneWayPrefix
                  + "] ViewAddress=[" + PushBackPilotUpdatePlanner.ViewAddressPrefix + "]");
        Assert.Fail(string.Join("\n", lines));
    }

    private static string Describe(PushBackPilotPrepareResult r) =>
        "kind=" + r.Kind
        + " plan=" + (r.Plan == null ? "null" : "miembros=" + string.Join(",", r.Plan.Members.Select(m => m.Key)) + " borrados=" + string.Join(",", r.Plan.Erasures))
        + " reason=" + (r.Reason ?? "null");
}
