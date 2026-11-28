#nullable enable
using System;
using System.Collections.Generic;
using RackCad.Application.Systems.PushBack.Draft;
using RackCad.Application.Workspace;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// TEMPORARY measurement (precondition 6 of PB-T4b, package v4.5, I-02 of the review; never committed): the three "presentar" lines of
    /// CT-PBT4B-06 with the real classifier, over the copy with the GREEN of PB-T4b and the kit (WorkspaceFullReadKit.cs). The host case of
    /// CT-06: TryKeepPilotEdit without a witness over a Dirty draft marked as possibly stale, with the scale 2.25 of the user and, outside, the
    /// name of the three views changed. It ends in Assert.Fail(text) so that the lines land in the TRX.
    /// </summary>
    public sealed class TmpCt06PresentarProbe
    {
        [Fact]
        public void Measure_the_presentar_lines_of_ct_pbt4b_06_on_the_green()
        {
            var lines = new List<string> { "copia=<el GREEN de PB-T4b> (sonda de CT-PBT4B-06, paquete v4.5)" };
            var reader = new FullEditIntentFakeReader { Answer = _ => FullEditIntentKit.ReadOf(FullPrepareKit.RealScan()) };
            var rig = new FullReadRig(reader);
            var draft = rig.Begin()!;
            FullPrepareKit.SetScale(draft, 2.25);
            draft.MarkPossiblyStale();
            var before = PushBackFullDraftStateComparer.Copy(draft.CurrentDraftState);
            var fingerprint = draft.CurrentDraftState.Origin;

            var renamed = FullPrepareKit.Scan(
                IntentKit.Member("A1", FullPrepareKit.Frontal(name: "Otro nombre")),
                IntentKit.Member("B2", FullPrepareKit.Lateral(FullPrepareKit.ExistingPost(), name: "Otro nombre")),
                IntentKit.Member("C3", FullPrepareKit.Planta(name: "Otro nombre")));
            reader.Answer = _ => FullEditIntentKit.ReadOf(renamed);
            var reads = reader.Calls.Count;

            var outcome = rig.Core.TryKeepPilotEdit(rig.Request());
            var origin = renamed.RawEnvelopes["A1"];
            lines.Add("presentar estado=" + outcome.Status
                + " testigo-clave=" + (outcome.KeepWitness?.Key ?? "null")
                + " testigo-texto-igual-al-origen=" + string.Equals(outcome.KeepWitness?.Text, origin, StringComparison.Ordinal)
                + " borrador-igual=" + PushBackFullDraftStateComparer.Instance.Equals(before, draft.CurrentDraftState)
                + " origen-igual=" + string.Equals(fingerprint, draft.CurrentDraftState.Origin, StringComparison.Ordinal)
                + " lecturas=" + (reader.Calls.Count - reads)
                + " lineas=" + outcome.PresentationLines.Count);
            foreach (var line in outcome.PresentationLines)
                lines.Add("presentar " + line);

            Assert.Fail(string.Join("\n", lines));
        }
    }
}
