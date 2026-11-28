using System;
using System.Collections.Generic;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Systems.PushBack.Draft;
using RackCad.Application.Systems.PushBack.Pilot;
using RackCad.Domain.Systems.PushBack;
using RackCad.UI.Shell;
using Xunit;
using static RackCad.UI.Tests.PushBackFullEditorCharacterizationSupport;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64 F4B PB-T3a — CT-PBT3A-01: the SEAT of the editable state matches the classic window (D-PBT3-03). For every envelope of
    /// the corpus but <c>pb-21-cola</c>, the full materializer admits it and its editable state is equal, with the comparer of
    /// PB-T1b, to the state of a window after <c>LoadExisting</c> (the seam of PB-T1c), and the persisted design of a recompute
    /// pass over a copy of that state is equal to the one the window computed. <c>pb-21-cola</c> parks a composite tail, which
    /// stays outside of the model: its line is the reason of the pilot.
    ///
    /// <para>No gesture is made: the lines are read right after <c>LoadExisting</c>, which ends with a pass. Every window is created
    /// with <see cref="PushBackFullEditorCharacterizationSupport.Ventana"/>, its load goes inside the <c>try</c> and it is closed in
    /// the <c>finally</c>; none is shown, the keyboard and the real focus are not used, and the whole method runs inside
    /// <see cref="EditorDiscardPrompt.Substitute(System.Func{string,bool})"/>. Every call of the new API goes through <c>Paso</c>,
    /// which turns an exception into an «EXCEPCION» line; the expected lines contain none.</para>
    /// </summary>
    [Collection(EditorDiscardPromptCollection.Name)]
    public sealed class PushBackFullDraftWindowParityTests
    {
        /// <summary>A step that throws becomes a line, and the expected lines never contain one.</summary>
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

        [Fact]
        public void CT_PBT3A_01_TheSeatMatchesTheWindowAfterLoadExisting()
        {
            var log = new List<string>();
            var descartes = 0;
            var names = new List<string>(CorpusNames());
            using (EditorDiscardPrompt.Substitute(_ => { descartes++; return true; }))
            {
                Sta(() =>
                {
                    foreach (var name in names)
                    {
                        var text = Fixture(name);
                        var w = Ventana();
                        try
                        {
                            var catalog = w.Session.Catalog;
                            var materialized = Paso(log, () => PushBackFullDraftMaterializer.Materialize(text, catalog));
                            if (name == "pb-21-cola")
                            {
                                log.Add(name + " " + (materialized == null ? "<sin resultado>" : materialized.Kind + " motivo=" + materialized.Reason));
                                continue;
                            }

                            var embed = new RackEmbedStore().Deserialize(text);
                            var project = new RackProjectStore().Deserialize(embed.Design);
                            w.LoadExisting(project.PushBackDesign, embed.Id, embed.Name, project);

                            var state = materialized != null && materialized.IsAdmitted ? materialized.State : null;
                            var stateEqual = state != null && Paso(log, () => PushBackEditableStateComparer.Instance.Equals(state.Editable, w.EditableStateForTest));
                            var designEqual = state != null && Paso(log, () =>
                            {
                                var copy = PushBackEditableStateComparer.Copy(state.Editable);
                                var computation = PushBackEditableStateSeat.Pass(
                                    copy, new PushBackEditorDesignAssembler(catalog), new PushBackCompositeEditorAssembler(catalog), PushBackSide.A);
                                return computation != null && string.Equals(Persist(computation.Design), Persist(w.LastComputation.Design), StringComparison.Ordinal);
                            });
                            log.Add(name + " estado-igual=" + stateEqual + " diseno-igual=" + designEqual);
                        }
                        finally { Cerrar(w); }
                    }
                });
            }

            log.Add("descartes=" + descartes);

            var expected = new List<string>();
            foreach (var name in names)
            {
                expected.Add(name == "pb-21-cola"
                    ? "pb-21-cola OutOfPilot motivo=" + PushBackPilotMaterializer.DormantTailOutOfPilotReason
                    : name + " estado-igual=True diseno-igual=True");
            }

            expected.Add("descartes=0");
            Assert.Equal(expected, log);
        }
    }
}
