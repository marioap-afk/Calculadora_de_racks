using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Systems.PushBack.Draft;
using RackCad.Application.Systems.PushBack.Pilot;
using RackCad.Application.Systems.Shared;
using RackCad.Application.Workspace;
using RackCad.Application.Workspace.Drafts;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-64 F4B PB-T4c — «Editar todo» through the edit port of the host (CT-PBT4C-03, 04, 05, 10, 11 and 12). Every recipe drives the host core
    /// of the workspace over a simulated bridge and the reader of the kit, with the real pure cores; the host of the full editor is a double
    /// of the class that shows nothing: it puts the modal of the panel (as the Plugin does) and runs what the recipe gives it over the launch.
    /// Nothing opens AutoCAD, a window or a file. Every call goes through <see cref="Paso{T}"/> (the «EXCEPCION» rule), so a behavior that is
    /// missing fails by ASSERTION on the lines, never by an escaped exception. Culture: invariant; numbers with «R».
    /// </summary>
    public sealed class PushBackEditAllTests
    {
        private const string RackA = FullEditIntentKit.RackA;

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

        private static void Finish(List<string> log, IEnumerable<string> expected)
            => Assert.Equal(string.Join("\n", expected), string.Join("\n", log));

        private static string Num(double value) => value.ToString("R", CultureInfo.InvariantCulture);

        private static string R(string reason) => reason == null ? "null" : "[" + reason + "]";

        private static string StatusOf(DraftSession<PushBackFullDraftState> draft) => draft == null ? "null" : draft.Status.ToString();

        /// <summary>The host of the full editor of the recipes: it puts the modal of the panel and runs the recipe over the launch.</summary>
        private sealed class EditorDouble : IWorkspaceFullEditHost
        {
            private readonly WorkspaceHostCore _core;

            public EditorDouble(WorkspaceHostCore core)
            {
                _core = core;
            }

            public Action<PushBackDraftEditorLaunch> Inside { get; set; }

            public int Shown { get; private set; }

            public void ShowModal<TLaunch>(TLaunch launch) where TLaunch : class
            {
                Shown++;
                var shown = launch as PushBackDraftEditorLaunch;
                _core.SetModalRackCadActive(true);
                try
                {
                    if (Inside != null) Inside(shown);
                }
                finally
                {
                    _core.SetModalRackCadActive(false);
                }
            }
        }

        /// <summary>The host over a simulated bridge, the reader of the kit and, when asked, the double of the host of the editor.</summary>
        private sealed class Setup
        {
            public Setup(bool withEditor = true)
            {
                Reader = new FullEditIntentFakeReader();
                Rig = new FullReadRig(Reader);
                Editor = new EditorDouble(Rig.Core);
                if (withEditor) Rig.Core.SetFullEditHost(Editor);
            }

            public FullEditIntentFakeReader Reader { get; }

            public FullReadRig Rig { get; }

            public EditorDouble Editor { get; }

            public IWorkspacePilotEditPort Port => Rig.Core;

            public WorkspaceRequest Origin => new WorkspaceRequest(Rig.Session);
        }

        /// <summary>The usual three views of a rack: the frontal view (the origin), a lateral one and the plan view.</summary>
        private static PushBackPilotIntentScan ThreeViews(int lateralPost, string origin)
            => IntentKit.Scan(
                FullPrepareKit.Rack,
                origin,
                IntentKit.Member("A1", FullPrepareKit.Frontal()),
                IntentKit.Member("B2", FullPrepareKit.Lateral(lateralPost)),
                IntentKit.Member("C3", FullPrepareKit.Planta()));

        /// <summary>«- frente»: the rack loses its last front, and its lateral of the post 3 becomes obsolete.</summary>
        private static void LessOneFront(PushBackEditableState state) => state.Editor.SetSlotCount(state.Editor.SlotCount - 1);

        // ------------------------------------------------------------------------------------------------- CT-03

        [Fact]
        public void CT_PBT4C_03_TheHostBeginsTheDraftOpensTheEditorOnlyOverADraftThatAdmitsItAndAnswersTheOtherCasesWithoutOpeningIt()
        {
            var log = new List<string>();

            Paso(log, () =>
            {
                var s = new Setup(withEditor: false);
                var outcome = s.Port.TryEditAll(s.Rig.Request());
                log.Add("sin-anfitrion estado=" + outcome.Status + " motivo=" + R(outcome.Reason) + " abierto=" + (s.Editor.Shown > 0)
                    + " lecturas=" + s.Reader.Calls.Count + " borrador=" + (s.Rig.Draft() != null));
            });

            Paso(log, () =>
            {
                var s = new Setup();
                var outcome = s.Port.TryEditAll(s.Rig.Request());
                log.Add("sin-borrador estado=" + outcome.Status + " abierto=" + (s.Editor.Shown > 0) + " lecturas=" + s.Reader.Calls.Count
                    + " borrador=" + StatusOf(s.Rig.Draft()));
            });

            Paso(log, () =>
            {
                var text = PilotKit.Text("pb-21-cola");
                var id = new RackEmbedStore().Deserialize(text).Id;
                var read = FullEditIntentKit.ReadOf(IntentKit.Scan(id, "H1", IntentKit.Member("H1", text)));
                var s = new Setup();
                s.Reader.Answer = rack => read;
                var outcome = s.Port.TryEditAll(new EditIntentRequest(s.Rig.Session, id, null, null));
                log.Add("cola estado=" + outcome.Status + " inicio=" + outcome.Begin.Status
                    + " diagnostico-fuera-del-piloto=" + string.Equals(outcome.Begin.Diagnostic, PushBackPilotIntentReader.OutOfPilotDiagnostic, StringComparison.Ordinal)
                    + " pista=" + !string.IsNullOrEmpty(outcome.Begin.ClassicEditorHint) + " abierto=" + (s.Editor.Shown > 0)
                    + " borrador=" + (s.Rig.Core.OpenDrafts.Find<PushBackFullDraftState>(s.Rig.Session, id) != null));
            });

            Paso(log, () =>
            {
                var s = new Setup();
                s.Rig.Begin();
                s.Reader.Calls.Clear();
                var outcome = s.Port.TryEditAll(s.Rig.Request());
                log.Add("clean estado=" + outcome.Status + " abierto=" + (s.Editor.Shown > 0) + " lecturas=" + s.Reader.Calls.Count);
            });

            Paso(log, () =>
            {
                var s = new Setup();
                var draft = s.Rig.Begin();
                FullPrepareKit.SetScale(draft, 2.5);
                s.Reader.Calls.Clear();
                var outcome = s.Port.TryEditAll(s.Rig.Request());
                log.Add("dirty estado=" + outcome.Status + " abierto=" + (s.Editor.Shown > 0) + " lecturas=" + s.Reader.Calls.Count);
            });

            Paso(log, () =>
            {
                var s = new Setup();
                var draft = s.Rig.Begin();
                Assert.True(draft.TryBeginApply().IsStarted);
                draft.CompleteApply(DraftApplyOutcome<PushBackFullDraftState>.NotAppliedConflict(new[] { new DraftDifference("A1", DraftDifferenceKind.EnvelopeText) }));
                s.Reader.Calls.Clear();
                var outcome = s.Port.TryEditAll(s.Rig.Request());
                log.Add("conflict estado=" + outcome.Status + " abierto=" + (s.Editor.Shown > 0) + " lecturas=" + s.Reader.Calls.Count);
            });

            Paso(log, () =>
            {
                var s = new Setup();
                var draft = s.Rig.Begin();
                Assert.True(draft.TryBeginApply().IsStarted);
                s.Reader.Calls.Clear();
                var outcome = s.Port.TryEditAll(s.Rig.Request());
                log.Add("applying estado=" + outcome.Status + " motivo=" + R(outcome.Reason) + " abierto=" + (s.Editor.Shown > 0));
            });

            Paso(log, () =>
            {
                var s = new Setup();
                var draft = s.Rig.Begin();
                Assert.True(draft.TryBeginApply().IsStarted);
                draft.CompleteApply(DraftApplyOutcome<PushBackFullDraftState>.CommitThrew("the commit threw"));
                s.Reader.Calls.Clear();
                var outcome = s.Port.TryEditAll(s.Rig.Request());
                log.Add("unknown estado=" + outcome.Status + " motivo=" + R(outcome.Reason) + " abierto=" + (s.Editor.Shown > 0));
            });

            Paso(log, () =>
            {
                var s = new Setup();
                var draft = s.Rig.Begin();
                draft.EnterError("boom");
                s.Reader.Calls.Clear();
                var outcome = s.Port.TryEditAll(s.Rig.Request());
                log.Add("error estado=" + outcome.Status + " motivo=" + R(outcome.Reason) + " abierto=" + (s.Editor.Shown > 0));
            });

            Paso(log, () =>
            {
                var s = new Setup();
                var draft = s.Rig.Begin();
                draft.SetPendingText(PushBackFullFieldAdapter.AnnotationScaleKey, "abc");
                s.Reader.Calls.Clear();
                var outcome = s.Port.TryEditAll(s.Rig.Request());
                log.Add("texto-pendiente estado=" + outcome.Status + " motivo=" + R(outcome.Reason) + " abierto=" + (s.Editor.Shown > 0));
            });

            Paso(log, () =>
            {
                var s = new Setup();
                var outcome = s.Port.TryEditAll(new EditIntentRequest(new SessionId(777), RackA, null, null));
                log.Add("otra-sesion estado=" + outcome.Status + " motivo=" + R(outcome.Reason) + " abierto=" + (s.Editor.Shown > 0)
                    + " lecturas=" + s.Reader.Calls.Count);
            });

            Finish(
                log,
                new[]
                {
                    "sin-anfitrion estado=NotAvailable motivo=[No editable aquí: el panel no tiene el editor completo.] abierto=False lecturas=0 borrador=False",
                    "sin-borrador estado=Closed abierto=True lecturas=1 borrador=Clean",
                    "cola estado=NotBegun inicio=NotAdmitted diagnostico-fuera-del-piloto=True pista=True abierto=False borrador=False",
                    "clean estado=Closed abierto=True lecturas=0",
                    "dirty estado=Closed abierto=True lecturas=0",
                    "conflict estado=Closed abierto=True lecturas=0",
                    "applying estado=Waiting motivo=[El borrador se está aplicando o se retiró: el editor no se abre.] abierto=False",
                    "unknown estado=Refused motivo=[El borrador no admite el editor completo en su estado actual.] abierto=False",
                    "error estado=Refused motivo=[El borrador no admite el editor completo en su estado actual.] abierto=False",
                    "texto-pendiente estado=Refused motivo=[Hay texto sin confirmar en el campo.] abierto=False",
                    "otra-sesion estado=Rejected motivo=[La petición no es del documento activo.] abierto=False lecturas=0",
                });
        }

        // ------------------------------------------------------------------------------------------------- CT-04

        [Fact]
        public void CT_PBT4C_04_TheLaunchCarriesTheDraftAndTheOriginAndThePortHandsEveryEditToTheDraftKeepingTheOrigin()
        {
            var log = new List<string>();

            Paso(log, () =>
            {
                var s = new Setup();
                var draft = s.Rig.Begin();
                var origin = draft.CurrentDraftState.Origin;
                var results = new List<string>();
                s.Editor.Inside = launch =>
                {
                    results.Add("lanzamiento estado-igual=" + PushBackEditableStateComparer.Instance.Equals(launch.State, draft.CurrentDraftState.Editable)
                        + " rack=" + (launch.RackId == RackA) + " proyecto=" + (launch.SourceProject != null)
                        + " proyecto-con-diseno=" + (launch.SourceProject != null && launch.SourceProject.PushBackDesign != null));
                    var edited = PushBackEditableStateComparer.Copy(launch.State);
                    edited.AnnotationScale = 2.5;
                    var first = launch.Port.ApplyToDraft(edited);
                    results.Add("aplicar-1 estado=" + first.Status + " isdirty=" + first.IsDirty + " origen-igual=" + (draft.CurrentDraftState.Origin == origin)
                        + " escala=" + Num(draft.CurrentDraftState.Editable.AnnotationScale));
                    var again = PushBackEditableStateComparer.Copy(launch.State);
                    again.AnnotationScale = 3;
                    var second = launch.Port.ApplyToDraft(again);
                    results.Add("aplicar-2 estado=" + second.Status + " isdirty=" + second.IsDirty + " escala=" + Num(draft.CurrentDraftState.Editable.AnnotationScale));
                };
                var outcome = s.Port.TryEditAll(s.Rig.Request());
                log.AddRange(results);
                log.Add("tras-aplicar estado=" + outcome.Status + " aplicadas=" + outcome.Applied + " borrador=" + StatusOf(draft)
                    + " origen-igual=" + (draft.CurrentDraftState.Origin == origin));
            });

            // The adapter alone, over a draft that does not admit changes (Applying): NotEditable and no call back.
            Paso(log, () =>
            {
                var s = new Setup();
                var draft = s.Rig.Begin();
                var calls = 0;
                var port = new PushBackFullDraftStatePort(draft, () => calls++);
                Assert.True(draft.TryBeginApply().IsStarted);
                var edited = PushBackEditableStateComparer.Copy(draft.CurrentDraftState.Editable);
                edited.AnnotationScale = 2.5;
                var result = port.ApplyToDraft(edited);
                log.Add("adaptador-no-editable estado=" + result.Status + " llamadas=" + calls);
            });

            // The adapter alone, over a clean draft: the edit is taken, and the host is called back once.
            Paso(log, () =>
            {
                var s = new Setup();
                var draft = s.Rig.Begin();
                var calls = 0;
                var port = new PushBackFullDraftStatePort(draft, () => calls++);
                var edited = PushBackEditableStateComparer.Copy(draft.CurrentDraftState.Editable);
                edited.AnnotationScale = 2.5;
                var result = port.ApplyToDraft(edited);
                log.Add("adaptador-aplica estado=" + result.Status + " llamadas=" + calls);
            });

            Finish(
                log,
                new[]
                {
                    "lanzamiento estado-igual=True rack=True proyecto=True proyecto-con-diseno=True",
                    "aplicar-1 estado=Applied isdirty=True origen-igual=True escala=2.5",
                    "aplicar-2 estado=Applied isdirty=True escala=3",
                    "tras-aplicar estado=Closed aplicadas=2 borrador=Dirty origen-igual=True",
                    "adaptador-no-editable estado=NotEditable llamadas=0",
                    "adaptador-aplica estado=Applied llamadas=1",
                });
        }

        // ------------------------------------------------------------------------------------------------- CT-05

        [Fact]
        public void CT_PBT4C_05_WhileTheEditorIsOpenEveryOperationOfThePortAnswersBusyOrWaitingOrFromMemoryAndNoneReadsTheDrawing()
        {
            var log = new List<string>();

            Paso(log, () =>
            {
                var s = new Setup();
                s.Rig.Begin();
                var port = s.Port;
                var origin = s.Origin;
                var request = s.Rig.Request();
                var inside = new List<string>();
                var readsInside = 0;
                s.Editor.Inside = _ =>
                {
                    var before = s.Reader.Calls.Count;
                    var t = port.SetPendingText(origin, RackA, "3"); inside.Add("k1 SetPendingText " + t.Status + " " + R(t.Reason));
                    var c = port.CommitField(origin, RackA); inside.Add("k1 CommitField " + c.Status + " " + R(c.Reason));
                    var b = port.CrossBoundary(origin, RackA, PilotBoundaryKind.TabChange); inside.Add("k1 CrossBoundary " + b.Decision + " " + R(b.Reason));
                    var d = port.DiscardRack(origin, RackA); inside.Add("k1 DiscardRack " + d.Status + " " + R(d.Reason));
                    var dd = port.DiscardDocument(origin); inside.Add("k1 DiscardDocument " + dd.Status + " " + R(dd.Reason));
                    var b1 = port.TryBeginEdit(request); inside.Add("k1 TryBeginEdit " + b1.Status + " " + R(b1.Reason));
                    var b2 = port.TryCheckNow(request); inside.Add("k1 TryCheckNow " + b2.Status + " " + R(b2.Reason));
                    var b3 = port.TryReload(request); inside.Add("k1 TryReload " + b3.Status + " " + R(b3.Reason));
                    var b4 = port.TryConfirmBase(request); inside.Add("k1 TryConfirmBase " + b4.Status + " " + R(b4.Reason));
                    var b5 = port.TryKeepMine(request); inside.Add("k1 TryKeepMine " + b5.Status + " " + R(b5.Reason));
                    var b6 = port.TryDiscardAndReload(request); inside.Add("k1 TryDiscardAndReload " + b6.Status + " " + R(b6.Reason));
                    var b7 = port.TryEditAll(request); inside.Add("k1 TryEditAll " + b7.Status + " " + R(b7.Reason));
                    var u = port.RequestUpdate(request); inside.Add("k1 RequestUpdate " + u.Status + " " + R(u.Reason));
                    var g = port.GetField(origin, RackA); inside.Add("k1 GetField existe=" + g.Exists + " estado=" + g.State);
                    inside.Add("k1 GetRacksAtRisk conocido=" + port.GetRacksAtRisk(origin).IsKnown);
                    inside.Add("k1 TakeLossNotices " + port.TakeLossNotices().Count + " TakeCloseNotices " + port.TakeCloseNotices().Count);
                    inside.Add("k1 TakeUpdateProgress en-vuelo=" + port.TakeUpdateProgress(origin, RackA).InFlight);
                    readsInside = s.Reader.Calls.Count - before;
                };
                var outcome = port.TryEditAll(request);
                log.AddRange(inside);
                log.Add("k1-cerrado estado=" + outcome.Status + " lecturas-dentro=" + readsInside);
                var after = port.SetPendingText(origin, RackA, "3");
                log.Add("despues SetPendingText " + after.Status + " TryBeginEdit " + port.TryBeginEdit(request).Status);
            });

            Finish(
                log,
                new[]
                {
                    "k1 SetPendingText Busy [Hay una lectura del piloto en curso.]",
                    "k1 CommitField Busy [Hay una lectura del piloto en curso.]",
                    "k1 CrossBoundary Busy [Hay una lectura del piloto en curso.]",
                    "k1 DiscardRack Busy [Hay una lectura del piloto en curso.]",
                    "k1 DiscardDocument Busy [Hay una lectura del piloto en curso.]",
                    "k1 TryBeginEdit Waiting [Hay una lectura del piloto en curso.]",
                    "k1 TryCheckNow Waiting [Hay una lectura del piloto en curso.]",
                    "k1 TryReload Waiting [Hay una lectura del piloto en curso.]",
                    "k1 TryConfirmBase Waiting [Hay una lectura del piloto en curso.]",
                    "k1 TryKeepMine Waiting [Hay una lectura del piloto en curso.]",
                    "k1 TryDiscardAndReload Waiting [Hay una lectura del piloto en curso.]",
                    "k1 TryEditAll Waiting [Hay una lectura del piloto en curso.]",
                    "k1 RequestUpdate Busy [Hay una lectura del piloto en curso.]",
                    "k1 GetField existe=True estado=Clean",
                    "k1 GetRacksAtRisk conocido=True",
                    "k1 TakeLossNotices 0 TakeCloseNotices 0",
                    "k1 TakeUpdateProgress en-vuelo=False",
                    "k1-cerrado estado=Closed lecturas-dentro=0",
                    "despues SetPendingText Applied TryBeginEdit AlreadyOpen",
                });
        }

        // ------------------------------------------------------------------------------------------------- CT-10

        [Fact]
        public void CT_PBT4C_10_FromThePortToTheUpdateWithoutAutoCadTheEditThatTheEditorHandsToTheDraftIsTheOneThatUpdateWrites()
        {
            var log = new List<string>();

            Paso(log, () =>
            {
                var scan = ThreeViews(3, "A1");
                var read = FullEditIntentKit.ReadOf(scan);
                var s = new Setup();
                s.Reader.Answer = rack => read;
                s.Rig.Begin();
                var draft = s.Rig.Draft();
                PushBackEditableState edited = null;
                s.Editor.Inside = launch =>
                {
                    edited = PushBackEditableStateComparer.Copy(launch.State);
                    LessOneFront(edited);
                    edited.AnnotationScale = 2.5;
                    launch.Port.ApplyToDraft(edited);
                };
                var outcome = s.Port.TryEditAll(s.Rig.Request());
                log.Add("editar-todo estado=" + outcome.Status + " aplicadas=" + outcome.Applied + " borrador=" + StatusOf(draft));
                log.Add("lo-aplicado-es-el-editado=" + (edited != null && PushBackEditableStateComparer.Instance.Equals(draft.CurrentDraftState.Editable, edited)));

                var port = new PilotEraseScopeDouble(FullWriteKit.ItemsOf(scan));
                var update = s.Rig.Core.TryUpdatePilot(s.Rig.Request(), port);
                log.Add("actualizar estado=" + update.Status + " begin-mutation=" + port.Calls.Count(call => call == "BeginMutation")
                    + " seal=" + port.Calls.Count(call => call == "Seal") + " borrador=" + StatusOf(draft));
                log.Add("borrado " + string.Join(",", port.Calls.Where(call => call.StartsWith("EraseUnit:", StringComparison.Ordinal))));
            });

            Finish(
                log,
                new[]
                {
                    "editar-todo estado=Closed aplicadas=1 borrador=Dirty",
                    "lo-aplicado-es-el-editado=True",
                    "actualizar estado=Applied begin-mutation=1 seal=1 borrador=Clean",
                    "borrado EraseUnit:B2",
                });
        }

        // ------------------------------------------------------------------------------------------------- CT-11 and the fingerprint

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

        /// <summary>The deep fingerprint of an object graph (the one of CT-PBT1B-16): every object with its type and each instance field with
        /// its value; numbers with «R» and the invariant culture, references by their index in the walk.</summary>
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

            public void Capture(params PushBackEditableState[] states)
            {
                foreach (var state in states)
                {
                    foreach (var reference in SharedRoots(state))
                    {
                        if (items.Any(item => ReferenceEquals(item.Object, reference))) continue;
                        items.Add((reference, Fingerprint(reference)));
                    }
                }
            }

            public bool Intact() => items.All(item => Fingerprint(item.Object) == item.Before);
        }

        [Fact]
        public void CT_PBT4C_11_ApplyToDraftNeverMutatesTheTwoSharedReferencesAndReplacingOneNeverMakesTheDraftDirty()
        {
            var log = new List<string>();

            Paso(log, () =>
            {
                var s = new Setup();
                var draft = s.Rig.Begin();
                var watch = new Watch();
                watch.Capture(draft.InitialDraftState.Editable, draft.CurrentDraftState.Editable);
                var lines = new List<string>();
                s.Editor.Inside = launch =>
                {
                    var first = PushBackEditableStateComparer.Copy(launch.State);
                    first.AnnotationScale = 2.5;
                    var one = launch.Port.ApplyToDraft(first);
                    lines.Add("ApplyToDraft-1 estado=" + one.Status + " huellas-iguales=" + (watch.Count > 0 && watch.Intact()));
                    watch.Capture(draft.CurrentDraftState.Editable);
                    var second = PushBackEditableStateComparer.Copy(launch.State);
                    second.AnnotationScale = 3;
                    var two = launch.Port.ApplyToDraft(second);
                    lines.Add("ApplyToDraft-2 estado=" + two.Status + " huellas-iguales=" + (watch.Count > 0 && watch.Intact()));
                };
                s.Port.TryEditAll(s.Rig.Request());
                log.AddRange(lines);
            });

            // Replacing a shared reference of the applied state never makes the draft dirty: the comparison does not depend on them.
            Paso(log, () =>
            {
                var s = new Setup();
                s.Rig.Begin();
                var lines = new List<string>();
                s.Editor.Inside = launch =>
                {
                    var replaced = PushBackEditableStateComparer.Copy(launch.State);
                    replaced.Editor.SideA.LastModuleReconciliation =
                        (RackModuleReconciliationResult)RuntimeHelpers.GetUninitializedObject(typeof(RackModuleReconciliationResult));
                    var result = launch.Port.ApplyToDraft(replaced);
                    lines.Add("sustituir-referencia estado=" + result.Status + " isdirty=" + result.IsDirty);
                };
                s.Port.TryEditAll(s.Rig.Request());
                log.AddRange(lines);
            });

            Finish(
                log,
                new[]
                {
                    "ApplyToDraft-1 estado=Applied huellas-iguales=True",
                    "ApplyToDraft-2 estado=Applied huellas-iguales=True",
                    "sustituir-referencia estado=Applied isdirty=False",
                });
        }

        // ------------------------------------------------------------------------------------------------- CT-12

        [Fact]
        public void CT_PBT4C_12_TheGateOfNoticesForwardsEditAllAndWaitsInsideANotice()
        {
            var log = new List<string>();

            Paso(log, () =>
            {
                var s = new Setup();
                var depth = new WorkspaceNoticeDepth();
                IWorkspacePilotEditPort gate = new WorkspacePilotEditGate(s.Rig.Core, depth);
                depth.Enter();
                var inside = gate.TryEditAll(s.Rig.Request());
                depth.Leave();
                log.Add("compuerta-dentro estado=" + inside.Status + " motivo=" + R(inside.Reason) + " abierto=" + (s.Editor.Shown > 0)
                    + " lecturas=" + s.Reader.Calls.Count);
                var outside = gate.TryEditAll(s.Rig.Request());
                log.Add("compuerta-fuera estado=" + outside.Status + " abierto=" + (s.Editor.Shown > 0) + " lecturas=" + s.Reader.Calls.Count);
            });

            Finish(
                log,
                new[]
                {
                    "compuerta-dentro estado=Waiting motivo=[Se está entregando un aviso del panel.] abierto=False lecturas=0",
                    "compuerta-fuera estado=Closed abierto=True lecturas=1",
                });
        }
    }
}
