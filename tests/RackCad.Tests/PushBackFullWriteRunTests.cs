using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Systems.PushBack.Draft;
using RackCad.Application.Systems.PushBack.Pilot;
using RackCad.Application.Workspace.Drafts;
using RackCad.Domain.Systems.PushBack;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-64 F4B PB-T3b — the RUN of the write of the full model (CT-PBT3B-07 to 09 and 12): one applied run with erasures, the failures
    /// of MUTATE with erasures, the comparison of POST and the proof that the run does not mutate the two references D2 declares as
    /// shared (the resolved rack and the module report). It runs against the double of the port that can erase (PilotEraseScopeDouble);
    /// nothing opens a window or a drawing. Every call to the new API goes through <see cref="Paso{T}"/>.
    /// </summary>
    public sealed class PushBackFullWriteRunTests
    {
        private static RackCatalog Catalog => PilotKit.Catalog;

        private static string Rack => IntentKit.Rack;

        // ---------------------------------------------------------------------------------------------- helpers

        /// <summary>The «EXCEPCION» rule: a step that throws becomes a line, and the expected lines never contain one.</summary>
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

        private static void Paso(List<string> log, Action step) => Paso<int>(log, () => { step(); return 0; });

        private static string Keys(IEnumerable<string> keys) => "[" + string.Join(",", keys) + "]";

        private static void Finish(List<string> log, IEnumerable<string> expected)
            => Assert.Equal(string.Join("\n", expected), string.Join("\n", log));

        private static PushBackPilotIntentScan ThreeViews(int lateralPost, string origin)
            => IntentKit.Scan(
                Rack,
                origin,
                IntentKit.Member("A1", PrepareKit.Frontal()),
                IntentKit.Member("B2", PrepareKit.Lateral(lateralPost)),
                IntentKit.Member("C3", PrepareKit.Planta()));

        /// <summary>«- frente»: the rack loses its last front, and its lateral of the post 3 becomes obsolete.</summary>
        private static void LessOneFront(PushBackFullDraftState state) => state.Editable.Editor.SetSlotCount(state.Editable.Editor.SlotCount - 1);

        private static DraftApplyOutcome<PushBackFullDraftState> ExecuteFull(
            List<string> log, DraftSession<PushBackFullDraftState> draft, IPushBackPilotUpdatePort port)
        {
            Assert.True(draft.TryBeginApply().IsStarted);
            return Paso(log, () => PushBackPilotUpdateRun.ExecuteFull(draft, new PushBackPilotUpdateRequest(100, Rack), port));
        }

        private static string Describe(DraftApplyOutcome<PushBackFullDraftState> outcome)
            => outcome == null ? "<sin resultado>" : outcome.Kind.ToString();

        private static string Calls(PilotEraseScopeDouble port) => "[" + string.Join(",", port.Calls) + "]";

        /// <summary>A rack with its drafts, its double and its edit.</summary>
        private sealed class Setup
        {
            public Setup(PushBackPilotIntentScan scan, Action<PushBackFullDraftState> edit)
            {
                Scan = scan;
                Read = FullWriteKit.Read(scan);
                Draft = FullWriteKit.DraftOf(Read);
                FullWriteKit.Edit(Draft, edit);
                Items = FullWriteKit.ItemsOf(scan);
                Port = new PilotEraseScopeDouble(Items);
            }

            public PushBackPilotIntentScan Scan { get; }

            public PushBackFullIntentResult Read { get; }

            public DraftSession<PushBackFullDraftState> Draft { get; }

            public IntentItem[] Items { get; }

            public PilotEraseScopeDouble Port { get; }
        }

        // ------------------------------------------------------------------------------------------------- CT-07

        [Fact]
        public void CT_PBT3B_07_AnAppliedRunErasesTheObsoleteViewBetweenTheStartOfMutateAndTheSealAndRebasesTheDraft()
        {
            var log = new List<string>();

            // (i) the sequence of one run with an erasure: one BeginMutation, one Seal, the EraseUnit between the two.
            Paso(log, () =>
            {
                var setup = new Setup(ThreeViews(3, "A1"), LessOneFront);
                var outcome = ExecuteFull(log, setup.Draft, setup.Port);
                var calls = setup.Port.Calls;
                var begin = calls.IndexOf("BeginMutation");
                var seal = calls.IndexOf("Seal");
                log.Add("llamadas=" + Calls(setup.Port));
                log.Add("resultado=" + Describe(outcome));
                log.Add("un-BeginMutation=" + (calls.Count(c => c == "BeginMutation") == 1) + " un-Seal=" + (calls.Count(c => c == "Seal") == 1)
                    + " un-Post=" + (calls.Count(c => c == "Post") == 1)
                    + " borrados-entre-BeginMutation-y-Seal=" + (setup.Port.Erased.Count > 0
                        && calls.Select((call, index) => (call, index)).Where(item => item.call.StartsWith("EraseUnit:", StringComparison.Ordinal)).All(item => item.index > begin && item.index < seal)));
                if (outcome == null) return;

                var closed = setup.Draft.CompleteApply(outcome);
                log.Add("rebasado=" + closed.Rebased + " base-nueva=" + Keys(setup.Draft.StaleBase.Entries.Keys.OrderBy(key => key, StringComparer.Ordinal)));
            });

            // (ii) the origin of the draft is the lateral that the edition erases: POST reads the first view that is redrawn, in ordinal.
            Paso(log, () =>
            {
                var setup = new Setup(ThreeViews(3, "B2"), LessOneFront);
                var outcome = ExecuteFull(log, setup.Draft, setup.Port);
                log.Add("origen-borrado resultado=" + Describe(outcome));
                if (outcome == null) return;

                setup.Draft.CompleteApply(outcome);
                var anchored = setup.Port.Drawing.First(member => member.Key == "A1").Text;
                log.Add("origen-borrado base-nueva=" + Keys(setup.Draft.StaleBase.Entries.Keys.OrderBy(key => key, StringComparer.Ordinal))
                    + " estado-inicial-es-el-sobre-nuevo-de-A1=" + (setup.Draft.InitialDraftState.Origin == anchored));

                // The next «Actualizar» finds that member by its text: the current state changes, and the draft is Dirty again.
                var scale = setup.Draft.CurrentDraftState.Editable.AnnotationScale == 3.0 ? 3.5 : 3.0;
                FullWriteKit.Edit(setup.Draft, state => state.Editable.AnnotationScale = scale);
                var dirty = setup.Draft.IsDirty;
                var now = FullWriteKit.Read(IntentKit.Scan(Rack, "A1", setup.Port.Drawing.Select(member => IntentKit.Member(member.Key, member.Text)).ToArray()));
                var second = Paso(log, () => PushBackPilotUpdatePlanner.PrepareFull(PushBackFullPrepareInput.From(setup.Draft), now, Catalog));
                log.Add("segundo-prepare dirty=" + dirty + " prepare=" + (second == null ? "<sin resultado>" : second.Kind + (second.Plan == null ? string.Empty : " origen=" + second.Plan.OriginKey)));
            });

            Finish(
                log,
                new[]
                {
                    "llamadas=[Begin,ReadForPrepare,PrepareUnits,BeginMutation,Reread,WriteUnit:A1,WriteUnit:C3,EraseUnit:B2,Seal,Dispose:mutation,Post,Dispose:scope]",
                    "resultado=Applied",
                    "un-BeginMutation=True un-Seal=True un-Post=True borrados-entre-BeginMutation-y-Seal=True",
                    "rebasado=True base-nueva=[A1,C3]",
                    "origen-borrado resultado=Applied",
                    "origen-borrado base-nueva=[A1,C3] estado-inicial-es-el-sobre-nuevo-de-A1=True",
                    "segundo-prepare dirty=True prepare=Ready origen=A1",
                });
        }

        // ------------------------------------------------------------------------------------------------- CT-08

        [Fact]
        public void CT_PBT3B_08_TheFailuresOfMutateWithErasuresAreTheOnesOfTheWriteAndNothingIsWrittenBeforeTheCapacityIsChecked()
        {
            var log = new List<string>();

            // A scope that cannot erase (the original double): the capacity is checked first, before the re-read and before any write.
            Paso(log, () =>
            {
                var setup = new Setup(ThreeViews(3, "A1"), LessOneFront);
                var port = new PilotUpdateScopeDouble(setup.Items);
                var outcome = ExecuteFull(log, setup.Draft, port);
                log.Add("sin-capacidad resultado=" + Describe(outcome) + " causa=[" + (outcome == null ? string.Empty : outcome.Cause) + "] llamadas=[" + string.Join(",", port.Calls) + "]"
                    + " sin-escrituras=" + (port.Written.Count == 0));
            });

            // EraseUnit throws before the seal: the write failed, the abort follows and nothing was sealed.
            Paso(log, () =>
            {
                var setup = new Setup(ThreeViews(3, "A1"), LessOneFront);
                setup.Port.ThrowAt["EraseUnit:B2"] = new InvalidOperationException("boom");
                var outcome = ExecuteFull(log, setup.Draft, setup.Port);
                log.Add("erase-lanza resultado=" + Describe(outcome) + " causa=[" + (outcome == null ? string.Empty : outcome.Cause) + "] llamadas=" + Calls(setup.Port)
                    + " dibujo-intacto=" + setup.Port.Drawing.Select(member => member.Text).SequenceEqual(setup.Items.Select(item => item.Text)));
            });

            // The view to erase is on a locked layer: nothing is written.
            Paso(log, () =>
            {
                var setup = new Setup(ThreeViews(3, "A1"), LessOneFront);
                setup.Port.LockedAtPrepare = new[] { "B2" };
                var outcome = ExecuteFull(log, setup.Draft, setup.Port);
                log.Add("capa-bloqueada-en-PREPARE resultado=" + Describe(outcome) + " causa=[" + (outcome == null ? string.Empty : outcome.Cause) + "] llamadas=" + Calls(setup.Port));
            });
            Paso(log, () =>
            {
                var setup = new Setup(ThreeViews(3, "A1"), LessOneFront);
                setup.Port.LockedAtReread = new[] { "B2" };
                var outcome = ExecuteFull(log, setup.Draft, setup.Port);
                log.Add("capa-bloqueada-en-MUTATE resultado=" + Describe(outcome) + " causa=[" + (outcome == null ? string.Empty : outcome.Cause) + "] llamadas=" + Calls(setup.Port));
            });

            // The view to erase changed between PREPARE and MUTATE.
            Paso(log, () =>
            {
                var setup = new Setup(ThreeViews(3, "A1"), LessOneFront);
                var changed = IntentKit.Member("B2", IntentKit.Rewrite(setup.Items.First(item => item.Fact.DefinitionKey == "B2").Text, e => e.Name = "cambiado"));
                setup.Port.RereadDrawing = setup.Items.Select(item => item.Fact.DefinitionKey == "B2" ? changed : item).ToArray();
                var outcome = ExecuteFull(log, setup.Draft, setup.Port);
                log.Add("vista-cambiada resultado=" + Describe(outcome) + " llamadas=" + Calls(setup.Port));
            });

            Finish(
                log,
                new[]
                {
                    "sin-capacidad resultado=NotAppliedPreflight causa=[El dibujo no admite borrar vistas en esta actualización.] llamadas=[Begin,ReadForPrepare,PrepareUnits,BeginMutation,Dispose:mutation,Dispose:scope] sin-escrituras=True",
                    "erase-lanza resultado=NotAppliedFailure causa=[La escritura falló antes de confirmarse: boom] llamadas=[Begin,ReadForPrepare,PrepareUnits,BeginMutation,Reread,WriteUnit:A1,WriteUnit:C3,EraseUnit:B2,Dispose:mutation,Dispose:scope] dibujo-intacto=True",
                    "capa-bloqueada-en-PREPARE resultado=NotAppliedPreflight causa=[LOCKED_LAYER: B2] llamadas=[Begin,ReadForPrepare,Dispose:scope]",
                    "capa-bloqueada-en-MUTATE resultado=NotAppliedPreflight causa=[LOCKED_LAYER: B2] llamadas=[Begin,ReadForPrepare,PrepareUnits,BeginMutation,Reread,Dispose:mutation,Dispose:scope]",
                    "vista-cambiada resultado=NotAppliedConflict llamadas=[Begin,ReadForPrepare,PrepareUnits,BeginMutation,Reread,Dispose:mutation,Dispose:scope]",
                });
        }

        // ------------------------------------------------------------------------------------------------- CT-09

        private static PushBackFullIntentResult PostRead(IEnumerable<(string Key, string Text)> members, double scale)
        {
            var entries = members.Select(member =>
            {
                var embed = new RackEmbedStore().Deserialize(member.Text);
                return new DraftStaleBaseEntry(member.Key, DraftMembership.Mutable, embed?.View, embed?.Section.ToString(CultureInfo.InvariantCulture), member.Text);
            });
            var staleBase = DraftStaleBase.Create(entries);
            var state = PushBackFullDraftMaterializer.Materialize(PrepareKit.Frontal(), Catalog).State;
            state.Editable.AnnotationScale = scale;
            return new PushBackFullIntentResult(staleBase, DraftIntentRead<PushBackFullDraftState>.Admitted(staleBase, state), "A1", null, null);
        }

        [Fact]
        public void CT_PBT3B_09_ComparePostFullChecksTheErasedKeysTheKeysTheEnvelopesAndTheScaleAndThePilotKeepsItsComparison()
        {
            var log = new List<string>();

            Paso(log, () =>
            {
                var setup = new Setup(ThreeViews(3, "A1"), LessOneFront);
                var prepared = Paso(log, () => PushBackPilotUpdatePlanner.PrepareFull(PushBackFullPrepareInput.From(setup.Draft), setup.Read, Catalog));
                if (prepared == null || prepared.Kind != PushBackPilotPrepareKind.Ready)
                {
                    log.Add("plan prepare=" + (prepared == null ? "<sin resultado>" : prepared.Kind.ToString()));
                    return;
                }

                var plan = prepared.Plan;
                var redrawn = plan.PostExpectation.Envelopes.Select(item => (item.Key, item.Text)).ToList();
                string Compare(PushBackFullIntentResult read) => Paso(log, () => PushBackPilotUpdateRun.ComparePostFull(plan, read)) ?? "<null>";

                log.Add("todo-igual " + Compare(PostRead(redrawn, plan.PostExpectation.AnnotationScale)));
                log.Add("borrada-sigue " + Compare(PostRead(redrawn.Append(("B2", setup.Items.First(item => item.Fact.DefinitionKey == "B2").Text)), plan.PostExpectation.AnnotationScale)));
                log.Add("falta-una-redibujada " + Compare(PostRead(redrawn.Take(1), plan.PostExpectation.AnnotationScale)));
                log.Add("sobre-distinto " + Compare(PostRead(redrawn.Select(item => item.Key == "A1" ? (item.Key, item.Text + " ") : item), plan.PostExpectation.AnnotationScale)));
                log.Add("escala-distinta " + Compare(PostRead(redrawn, 2.0)));
                log.Add("sin-relectura " + Compare(null));
                log.Add("erased-keys=" + Keys(plan.PostExpectation.ErasedKeys));
            });

            // I-64 F4B PB-T4d (owner-pbt4d-substitutes-decision.md): the comparison of the pilot (ComparePost, retired) goes; ComparePostFull stays.

            Finish(
                log,
                new[]
                {
                    "todo-igual <null>",
                    "borrada-sigue " + PushBackPilotUpdateRun.PostErasedStillPresentPrefix + "B2",
                    "falta-una-redibujada " + PushBackPilotUpdateRun.PostKeysDifferReason,
                    "sobre-distinto " + PushBackPilotUpdateRun.PostEnvelopeDiffersPrefix + "A1",
                    "escala-distinta " + PushBackPilotUpdateRun.PostScaleDiffersReason,
                    "sin-relectura " + PushBackPilotUpdateRun.PostRereadMissingReason,
                    "erased-keys=[B2]",
                });
        }

        // ------------------------------------------------------------------------------------------------- CT-12

        private static bool IsProductType(Type type)
            => type.Namespace != null && type.Namespace.StartsWith("RackCad", StringComparison.Ordinal) && !type.IsEnum;

        private static IEnumerable<FieldInfo> InstanceFields(Type type)
        {
            for (var current = type; current != null && current != typeof(object); current = current.BaseType)
            {
                foreach (var field in current.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
                    yield return field;
                }
            }
        }

        private static IEnumerable<object> SharedRoots(PushBackEditableState state)
        {
            foreach (var side in new[] { state.Editor.SideA, state.Editor.SideB })
            {
                if (side.WorkingBaseline != null) yield return side.WorkingBaseline;
                if (side.LastModuleReconciliation != null) yield return side.LastModuleReconciliation;
            }
        }

        /// <summary>The deep fingerprint of an object graph (the one of CT-PBT1B-16): every object with its type and each instance field
        /// with its value; numbers with «R» and the invariant culture, references by their index in the walk.</summary>
        private static string Fingerprint(object root)
        {
            var index = new Dictionary<object, int>(ReferenceEqualityComparer.Instance);
            var order = new List<object>();
            int Idx(object value)
            {
                if (!index.TryGetValue(value, out var position))
                {
                    position = order.Count;
                    index[value] = position;
                    order.Add(value);
                }

                return position;
            }

            string Value(object value)
            {
                if (value == null) return "null";
                if (value is string text) return "\"" + text + "\"";
                if (value is double number) return number.ToString("R", CultureInfo.InvariantCulture);
                if (value is float single) return single.ToString("R", CultureInfo.InvariantCulture);
                if (value is Enum || value.GetType().IsPrimitive) return Convert.ToString(value, CultureInfo.InvariantCulture);
                if (value.GetType().IsValueType)
                {
                    return "{" + string.Join(",", value.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                        .Select(field => field.Name + "=" + Value(field.GetValue(value)))) + "}";
                }

                return "#" + Idx(value);
            }

            var builder = new StringBuilder();
            Idx(root);
            for (var position = 0; position < order.Count; position++)
            {
                var current = order[position];
                var type = current.GetType();
                builder.Append('#').Append(position).Append(' ').Append(type.FullName).Append('\n');
                if (current is IDictionary dictionary)
                {
                    foreach (DictionaryEntry entry in dictionary) builder.Append("  [").Append(Value(entry.Key)).Append("]=").Append(Value(entry.Value)).Append('\n');
                }
                else if (current is IEnumerable items && !(current is string))
                {
                    foreach (var item in items) builder.Append("  ").Append(Value(item)).Append('\n');
                }

                if (!IsProductType(type)) continue;
                foreach (var field in InstanceFields(type))
                {
                    builder.Append("  ").Append(field.Name).Append('=').Append(Value(field.GetValue(current))).Append('\n');
                }
            }

            using (var sha = SHA256.Create())
            {
                return string.Concat(sha.ComputeHash(Encoding.UTF8.GetBytes(builder.ToString())).Select(b => b.ToString("x2", CultureInfo.InvariantCulture)));
            }
        }

        /// <summary>The shared references of the states it watches, each measured from the moment it is captured.</summary>
        private sealed class Watch
        {
            private readonly List<(object Object, string Before)> items = new List<(object, string)>();

            public int Count => items.Count;

            public void Capture(params PushBackFullDraftState[] states)
            {
                foreach (var state in states)
                {
                    foreach (var reference in SharedRoots(state.Editable))
                    {
                        if (items.Any(item => ReferenceEquals(item.Object, reference))) continue;
                        items.Add((reference, Fingerprint(reference)));
                    }
                }
            }

            public bool Intact() => items.All(item => Fingerprint(item.Object) == item.Before);
        }

        [Fact]
        public void CT_PBT3B_12_NeitherThePrepareNorTheRunMutatesTheResolvedRackOrTheModuleReportOfTheDraft()
        {
            var log = new List<string>();
            Paso(log, () =>
            {
                var setup = new Setup(ThreeViews(3, "A1"), LessOneFront);

                // The references are taken BEFORE PrepareFull, and the fingerprint of after is computed over those very objects: after an
                // applied run the draft is rebased and its states are other objects, so reading them again would prove nothing.
                var watch = new Watch();
                watch.Capture(setup.Draft.InitialDraftState, setup.Draft.CurrentDraftState);
                var prepared = Paso(log, () => PushBackPilotUpdatePlanner.PrepareFull(PushBackFullPrepareInput.From(setup.Draft), setup.Read, Catalog));
                var outcome = ExecuteFull(log, setup.Draft, setup.Port);
                log.Add("prepare=" + (prepared == null ? "<sin resultado>" : prepared.Kind.ToString()) + " corrida=" + Describe(outcome)
                    + " huellas-iguales=" + (watch.Count > 0 && watch.Intact()));
            });

            Finish(log, new[] { "prepare=Ready corrida=Applied huellas-iguales=True" });
        }
    }
}
