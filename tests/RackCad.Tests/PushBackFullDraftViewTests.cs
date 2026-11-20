using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Systems.PushBack.Draft;
using RackCad.Application.Systems.PushBack.Pilot;
using RackCad.Domain.Systems.PushBack;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-64 F4B PB-T3a — the DISPOSITION of the views of a rack after an edition (CT-PBT3A-08): the lateral views that become
    /// obsolete after each edit of the fronts (the cases of CT-PB0-10), the frontal of side B of a one-way rack, a view with no
    /// address and the rack whose only view is an obsolete lateral. The decision is the pure one of RACKEDITAR; nothing here
    /// opens a window or a drawing.
    /// </summary>
    public sealed class PushBackFullDraftViewTests
    {
        private static RackCatalog Catalog => PilotKit.Catalog;

        private static T Paso<T>(List<string> log, Func<T> step)
        {
            try
            {
                return step();
            }
            catch (Exception error)
            {
                log.Add("EXCEPCION " + error.GetType().Name + ": " + error.Message);
                return default(T);
            }
        }

        /// <summary>The system of the editor at one instant, and the lateral cuts it has.</summary>
        private sealed class Moment
        {
            public string Label;
            public List<int> PostIndexes;
            public PushBackSystem System;
        }

        private static Moment Measure(string label, PushBackPilotDraftState state)
        {
            var design = new PushBackCompositeEditorAssembler(Catalog).BuildDesign(state.Editor, state.Inputs);
            var system = new PushBackResolver(Catalog).Resolve(design);
            return new Moment
            {
                Label = label,
                PostIndexes = new PushBackSystemLateralBuilder().Cortes(system, Catalog).Select(corte => corte.PostIndex).ToList(),
                System = system,
            };
        }

        private static RackEmbedDocument Envelope(string view, int section)
            => new RackEmbedDocument { Kind = RackEmbedDocument.KindPushBack, View = view, Section = section };

        private static KeyValuePair<string, RackEmbedDocument> Member(string key, string view, int section)
            => new KeyValuePair<string, RackEmbedDocument>(key, Envelope(view, section));

        private static string Describe(PushBackViewDispositionPlan plan)
            => "Redraw=[" + string.Join(",", plan.Redraw) + "] ObsoleteLaterals=[" + string.Join(",", plan.ObsoleteLaterals)
               + "] FrontalBOfOneWay=[" + string.Join(",", plan.FrontalBOfOneWay) + "] NoAddress=[" + string.Join(",", plan.NoAddress)
               + "] AllObsolete=" + plan.AllObsolete;

        private static PushBackViewDispositionPlan Dispose(List<string> log, IEnumerable<KeyValuePair<string, RackEmbedDocument>> members, PushBackSystem system)
            => Paso(log, () => PushBackViewDispositions.Of(members, system, Catalog));

        [Fact]
        public void CT_PBT3A_08_TheDispositionOfTheViewsIsTheOneOfTheClassicEditor()
        {
            var log = new List<string>();
            var expected = new List<string>();

            // The moments of CT-PB0-10 on pb-01-base: one lateral envelope per cut of BEFORE, the system of AFTER.
            var materialized = PushBackPilotMaterializer.Materialize(PilotKit.Text("pb-01-base"), Catalog);
            var state = materialized.State;
            var editor = state.Editor;
            var count = editor.SlotCount;

            var moments = new List<Moment> { Measure("base", state) };
            editor.SetSlotCount(count + 1);
            moments.Add(Measure("+frente", state));
            editor.SetSlotCount(count);
            moments.Add(Measure("-frente", state));
            editor.SetSlotPresent(PushBackSide.A, 1, false);
            editor.SetSlotPresent(PushBackSide.A, 2, false);
            moments.Add(Measure("frentes 1 y 2 en blanco", state));
            editor.SetSideBPresent(true);
            moments.Add(Measure("declarar B", state));
            editor.SetSlotPresent(PushBackSide.B, 1, true);
            moments.Add(Measure("B ranura 1 activa", state));
            editor.SetSlotPresent(PushBackSide.B, 1, false);
            moments.Add(Measure("B ranura 1 en blanco", state));
            editor.SetSideBPresent(false);
            moments.Add(Measure("retirar B", state));

            // The sets of obsolete laterals of each step are the ones CT-PB0-10 gives: [], [4], [2], [], [], [2], [].
            var obsolete = new[] { string.Empty, "4", "2", string.Empty, string.Empty, "2", string.Empty };
            for (var i = 0; i + 1 < moments.Count; i++)
            {
                var before = moments[i];
                var after = moments[i + 1];
                var members = before.PostIndexes.Select(post => Member(post.ToString(), RackEmbedDocument.ViewLateral, post)).ToList();
                var plan = Dispose(log, members, after.System);
                log.Add(before.Label + " -> " + after.Label + " obsoletas=[" + (plan == null ? "<sin resultado>" : string.Join(",", plan.ObsoleteLaterals)) + "]");
                expected.Add(before.Label + " -> " + after.Label + " obsoletas=[" + obsolete[i] + "]");
            }

            // A one-way rack that keeps a frontal of side B: pb-20-ab with side B retired.
            var abText = PilotKit.Text("pb-20-ab");
            var abState = new PushBackEditableState();
            abState.LoadDesign(
                new RackProjectStore().Deserialize(new RackEmbedStore().Deserialize(abText).Design).PushBackDesign,
                "pb-20-ab",
                new PushBackEditorDesignAssembler(Catalog).Resolver);
            abState.Editor.SetSideBPresent(false);
            var oneWay = new PushBackResolver(Catalog).Resolve(
                new PushBackCompositeEditorAssembler(Catalog).BuildDesign(abState.Editor, abState.BuildInputs(new PushBackEditorDesignAssembler(Catalog))));
            var onlyB = Dispose(log, new[] { Member("FB", RackEmbedDocument.ViewFrontal, 2) }, oneWay);
            log.Add("pb-20-ab-sin-B frontal-B " + (onlyB == null ? "<sin resultado>" : Describe(onlyB)));
            expected.Add("pb-20-ab-sin-B frontal-B Redraw=[] ObsoleteLaterals=[] FrontalBOfOneWay=[FB] NoAddress=[] AllObsolete=False");

            // The frontal of side A and the plan view are always redrawn.
            var always = Dispose(log, new[] { Member("FA", RackEmbedDocument.ViewFrontal, 0), Member("P", RackEmbedDocument.ViewPlanta, -1) }, oneWay);
            log.Add("frontal-A-y-planta " + (always == null ? "<sin resultado>" : Describe(always)));
            expected.Add("frontal-A-y-planta Redraw=[FA,P] ObsoleteLaterals=[] FrontalBOfOneWay=[] NoAddress=[] AllObsolete=False");

            // A lateral with a section of -1 has no address.
            var none = Dispose(log, new[] { Member("N", RackEmbedDocument.ViewLateral, -1) }, moments[0].System);
            log.Add("lateral-sin-direccion " + (none == null ? "<sin resultado>" : Describe(none)));
            expected.Add("lateral-sin-direccion Redraw=[] ObsoleteLaterals=[] FrontalBOfOneWay=[] NoAddress=[N] AllObsolete=False");

            // A rack whose only view is an obsolete lateral: the post 4 does not exist in the system of the base.
            var alone = Dispose(log, new[] { Member("L4", RackEmbedDocument.ViewLateral, 4) }, moments[0].System);
            log.Add("solo-un-lateral-obsoleto " + (alone == null ? "<sin resultado>" : Describe(alone)));
            expected.Add("solo-un-lateral-obsoleto Redraw=[] ObsoleteLaterals=[L4] FrontalBOfOneWay=[] NoAddress=[] AllObsolete=True");

            Assert.Equal(expected, log);
        }
    }
}
