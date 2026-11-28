using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Systems.PushBack.Draft;
using RackCad.Application.Systems.PushBack.Pilot;
using RackCad.Application.Workspace;
using RackCad.Application.Workspace.Drafts;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-64 F4B PB-T4e (D-PBT4-22) — the reload of «Editar todo» through the edit port of the host (CT-PBT4E-01, 02 and 04). Before the editor
    /// opens, a clean draft marked as possibly stale is reloaded from the drawing with the lazy reload of «Editar» (the same method of the host,
    /// with its one read); a reload that does not reload it does not open the editor and carries its outcome; a dirty draft, a draft in
    /// conflict or a draft without the mark opens as today, and an undecidable one is refused as today (D-S19). Every recipe drives the host core of the workspace over a simulated bridge and the
    /// reader of the kit, with the real pure cores; the host of the full editor is a double of the class that shows nothing: it puts the modal of
    /// the panel (as the Plugin does), records the launch and the reads already made when it opens, and runs what the recipe gives it. Nothing
    /// opens AutoCAD, a window or a file. Every call goes through <see cref="Paso{T}"/> (the «EXCEPCION» rule), so a behavior that is missing
    /// fails by ASSERTION on the lines, never by an escaped exception. Culture: invariant.
    /// </summary>
    public sealed class PushBackEditAllReloadTests
    {
        private const string RackA = FullEditIntentKit.RackA;

        /// <summary>The name that the drawing carries after the change outside the panel: a field that the full editor shows.</summary>
        private const string OtherName = "Otro nombre";

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

        private static string R(string reason) => reason == null ? "null" : "[" + reason + "]";

        private static string StatusOf(DraftSession<PushBackFullDraftState> draft) => draft == null ? "null" : draft.Status.ToString();

        /// <summary>The host of the full editor of the recipes: it puts the modal of the panel, records what it opens and runs the recipe.</summary>
        private sealed class EditorDouble : IWorkspaceFullEditHost
        {
            private readonly WorkspaceHostCore _core;
            private readonly Func<int> _reads;

            public EditorDouble(WorkspaceHostCore core, Func<int> reads)
            {
                _core = core;
                _reads = reads;
            }

            public Action<PushBackDraftEditorLaunch> Inside { get; set; }

            public int Shown { get; private set; }

            /// <summary>The launch of the last opening, or null.</summary>
            public PushBackDraftEditorLaunch Launch { get; private set; }

            /// <summary>The reads of the drawing already made when the editor opens, and the ones made while it is open.</summary>
            public int ReadsBefore { get; private set; }

            public int ReadsInside { get; private set; }

            public void ShowModal<TLaunch>(TLaunch launch) where TLaunch : class
            {
                Shown++;
                Launch = launch as PushBackDraftEditorLaunch;
                ReadsBefore = _reads();
                _core.SetModalRackCadActive(true);
                try
                {
                    if (Inside != null) Inside(Launch);
                }
                finally
                {
                    _core.SetModalRackCadActive(false);
                    ReadsInside = _reads() - ReadsBefore;
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
                Editor = new EditorDouble(Rig.Core, () => Reader.Calls.Count);
                if (withEditor) Rig.Core.SetFullEditHost(Editor);
            }

            public FullEditIntentFakeReader Reader { get; }

            public FullReadRig Rig { get; }

            public EditorDouble Editor { get; }

            public IWorkspacePilotEditPort Port => Rig.Core;

            /// <summary>The draft begun from the usual read; then the drawing changes outside (the reader answers the renamed read), and the
            /// reads are counted from here.</summary>
            public DraftSession<PushBackFullDraftState> BeginThenChangeOutside()
            {
                var draft = Rig.Begin();
                Reader.Answer = Renamed;
                Reader.Calls.Clear();
                return draft;
            }
        }

        /// <summary>The admitted read of the same two views of the rack after a change outside the panel: another name.</summary>
        private static EditIntentReadResult Renamed(string rackId)
            => FullEditIntentKit.ReadOf(
                IntentKit.Scan(
                    rackId,
                    "A1",
                    IntentKit.Member("A1", IntentKit.Variant("pb-01-base", rackId, name: OtherName)),
                    IntentKit.Member("B2", IntentKit.Variant("pb-01-base", rackId, view: "lateral", section: 3, name: OtherName))));

        /// <summary>The usual three views of a rack (the frontal view, the origin; a lateral one; the plan view), with a name when given.</summary>
        private static PushBackPilotIntentScan ThreeViews(int lateralPost, string name)
            => IntentKit.Scan(
                FullPrepareKit.Rack,
                "A1",
                IntentKit.Member("A1", FullPrepareKit.Frontal(name: name)),
                IntentKit.Member("B2", FullPrepareKit.Lateral(lateralPost, name: name)),
                IntentKit.Member("C3", FullPrepareKit.Planta(name: name)));

        /// <summary>«- frente»: the rack loses its last front, and its lateral of the post 3 becomes obsolete.</summary>
        private static void LessOneFront(PushBackEditableState state) => state.Editor.SetSlotCount(state.Editor.SlotCount - 1);

        private static string NameOf(PushBackDraftEditorLaunch launch) => launch == null ? "null" : "[" + launch.State.Name + "]";

        /// <summary>The outcome of «Editar todo» as the lines of CT-02 read it, and its comparison with the reload of «Editar».</summary>
        private static bool SameAsTheReload(EditIntentOutcome begin, EditIntentOutcome reload)
            => begin.Status == reload.Status
               && string.Equals(begin.Reason, reload.Reason, StringComparison.Ordinal)
               && string.Equals(begin.Diagnostic, reload.Diagnostic, StringComparison.Ordinal)
               && string.Equals(begin.Detail, reload.Detail, StringComparison.Ordinal)
               && string.Equals(begin.ClassicEditorHint, reload.ClassicEditorHint, StringComparison.Ordinal);

        // ------------------------------------------------------------------------------------------------- CT-01

        [Fact]
        public void CT_PBT4E_01_EditAllReloadsACleanDraftMarkedAsPossiblyStaleBeforeTheEditorOpensAndLeavesEveryOtherDraftAsToday()
        {
            var log = new List<string>();

            // The case of the task: a clean draft marked as possibly stale is reloaded, with one read, before the editor opens over it.
            Paso(log, () =>
            {
                var s = new Setup();
                var draft = s.BeginThenChangeOutside();
                draft.MarkPossiblyStale();
                var outcome = s.Port.TryEditAll(s.Rig.Request());
                log.Add("marcado-limpio estado=" + outcome.Status + " abierto=" + (s.Editor.Shown > 0) + " lecturas=" + s.Reader.Calls.Count
                    + " lecturas-antes-de-abrir=" + s.Editor.ReadsBefore + " lecturas-dentro=" + s.Editor.ReadsInside
                    + " nombre-del-lanzamiento=" + NameOf(s.Editor.Launch) + " obsoleto=" + draft.IsPossiblyStale + " borrador=" + StatusOf(draft)
                    + " mismo-borrador=" + ReferenceEquals(draft, s.Rig.Draft()));
            });

            // Without the mark: nothing is read and the editor opens over the draft as it is, as today.
            Paso(log, () =>
            {
                var s = new Setup();
                var draft = s.BeginThenChangeOutside();
                var outcome = s.Port.TryEditAll(s.Rig.Request());
                log.Add("sin-marca estado=" + outcome.Status + " abierto=" + (s.Editor.Shown > 0) + " lecturas=" + s.Reader.Calls.Count
                    + " nombre-nuevo=" + (s.Editor.Launch != null && s.Editor.Launch.State.Name == OtherName) + " obsoleto=" + draft.IsPossiblyStale);
            });

            // A dirty draft with the mark: the reload of «Editar» does not apply to it, so nothing is read and the editor opens as today.
            Paso(log, () =>
            {
                var s = new Setup();
                var draft = s.BeginThenChangeOutside();
                FullPrepareKit.SetScale(draft, 2.5);
                draft.MarkPossiblyStale();
                var outcome = s.Port.TryEditAll(s.Rig.Request());
                log.Add("marcado-sucio estado=" + outcome.Status + " abierto=" + (s.Editor.Shown > 0) + " lecturas=" + s.Reader.Calls.Count
                    + " nombre-nuevo=" + (s.Editor.Launch != null && s.Editor.Launch.State.Name == OtherName) + " obsoleto=" + draft.IsPossiblyStale
                    + " borrador=" + StatusOf(draft));
            });

            // A draft in conflict with the mark: the same, it opens as today without reading.
            Paso(log, () =>
            {
                var s = new Setup();
                var draft = s.BeginThenChangeOutside();
                Assert.True(draft.TryBeginApply().IsStarted);
                draft.CompleteApply(DraftApplyOutcome<PushBackFullDraftState>.NotAppliedConflict(new[] { new DraftDifference("A1", DraftDifferenceKind.EnvelopeText) }));
                draft.MarkPossiblyStale();
                var outcome = s.Port.TryEditAll(s.Rig.Request());
                log.Add("marcado-conflicto estado=" + outcome.Status + " abierto=" + (s.Editor.Shown > 0) + " lecturas=" + s.Reader.Calls.Count
                    + " obsoleto=" + draft.IsPossiblyStale + " borrador=" + StatusOf(draft));
            });

            // An undecidable draft with the mark (its comparer throws, so it cannot tell whether it is dirty; D-S19): the checks of the view
            // answer first, as today, so the mark is never evaluated, nothing is read and nothing is reported as a failure.
            Paso(log, () =>
            {
                var s = new Setup();
                var begun = s.Rig.Core.OpenDrafts.Begin(
                    s.Rig.Session, RackA, FullEditIntentKit.Admitted(RackA).FullRead.Read, new PilotEditThrowingComparerFull(), PushBackFullDraftStateComparer.Copy);
                var draft = begun.Draft;
                draft.MarkPossiblyStale();
                s.Reader.Calls.Clear();
                var outcome = s.Port.TryEditAll(s.Rig.Request());
                log.Add("marcado-sin-decidir alta=" + begun.Status + " estado=" + outcome.Status + " motivo=" + R(outcome.Reason) + " abierto=" + (s.Editor.Shown > 0)
                    + " lecturas=" + s.Reader.Calls.Count + " fallos=[" + string.Join(",", s.Rig.Host.Faults.Select(fault => fault.Context)) + "] obsoleto=" + draft.IsPossiblyStale);
            });

            // Without the host of the editor, nothing is reloaded: the answer of today comes before the reload.
            Paso(log, () =>
            {
                var s = new Setup(withEditor: false);
                var draft = s.BeginThenChangeOutside();
                draft.MarkPossiblyStale();
                var outcome = s.Port.TryEditAll(s.Rig.Request());
                log.Add("marcado-sin-anfitrion estado=" + outcome.Status + " abierto=" + (s.Editor.Shown > 0) + " lecturas=" + s.Reader.Calls.Count
                    + " obsoleto=" + draft.IsPossiblyStale);
            });

            // A request of another session is rejected by the gate before anything is read.
            Paso(log, () =>
            {
                var s = new Setup();
                var draft = s.BeginThenChangeOutside();
                draft.MarkPossiblyStale();
                var outcome = s.Port.TryEditAll(new EditIntentRequest(new SessionId(777), RackA, null, null));
                log.Add("marcado-otra-sesion estado=" + outcome.Status + " abierto=" + (s.Editor.Shown > 0) + " lecturas=" + s.Reader.Calls.Count
                    + " obsoleto=" + draft.IsPossiblyStale);
            });

            Finish(
                log,
                new[]
                {
                    "marcado-limpio estado=Closed abierto=True lecturas=1 lecturas-antes-de-abrir=1 lecturas-dentro=0 nombre-del-lanzamiento=[Otro nombre] obsoleto=False borrador=Clean mismo-borrador=True",
                    "sin-marca estado=Closed abierto=True lecturas=0 nombre-nuevo=False obsoleto=False",
                    "marcado-sucio estado=Closed abierto=True lecturas=0 nombre-nuevo=False obsoleto=True borrador=Dirty",
                    "marcado-conflicto estado=Closed abierto=True lecturas=0 obsoleto=True borrador=Conflict",
                    "marcado-sin-decidir alta=Created estado=Refused motivo=[El borrador no admite el editor completo en su estado actual.] abierto=False lecturas=0 fallos=[] obsoleto=True",
                    "marcado-sin-anfitrion estado=NotAvailable abierto=False lecturas=0 obsoleto=True",
                    "marcado-otra-sesion estado=Rejected abierto=False lecturas=0 obsoleto=True",
                });
        }

        // ------------------------------------------------------------------------------------------------- CT-02

        [Fact]
        public void CT_PBT4E_02_AReloadThatDoesNotReloadTheDraftDoesNotOpenTheEditorAndCarriesWhatTheReloadOfEditarAnswers()
        {
            var log = new List<string>();
            var same = new List<string>();

            // The drawing changed outside and its read is not admitted any more (the origin is not in it).
            Paso(log, () =>
            {
                var s = new Setup();
                var draft = s.BeginThenChangeOutside();
                s.Reader.Answer = FullEditIntentKit.OriginMissing;
                draft.MarkPossiblyStale();
                var outcome = s.Port.TryEditAll(s.Rig.Request());
                log.Add("no-admitida estado=" + outcome.Status + " inicio=" + outcome.Begin.Status + " diagnostico=" + R(outcome.Begin.Diagnostic)
                    + " pista=" + !string.IsNullOrEmpty(outcome.Begin.ClassicEditorHint) + " abierto=" + (s.Editor.Shown > 0)
                    + " lecturas=" + s.Reader.Calls.Count + " obsoleto=" + draft.IsPossiblyStale + " borrador=" + StatusOf(draft));

                var twin = new Setup();
                var other = twin.BeginThenChangeOutside();
                twin.Reader.Answer = FullEditIntentKit.OriginMissing;
                other.MarkPossiblyStale();
                same.Add("no-admitida=" + SameAsTheReload(outcome.Begin, twin.Port.TryReload(twin.Rig.Request())));
            });

            // A command in progress: the reader answers Busy and the reload waits.
            Paso(log, () =>
            {
                var s = new Setup();
                var draft = s.BeginThenChangeOutside();
                s.Reader.Answer = _ => EditIntentReadResult.Busy("Hay un comando en curso.");
                draft.MarkPossiblyStale();
                var outcome = s.Port.TryEditAll(s.Rig.Request());
                log.Add("ocupado estado=" + outcome.Status + " inicio=" + outcome.Begin.Status + " motivo=" + R(outcome.Begin.Reason)
                    + " abierto=" + (s.Editor.Shown > 0) + " lecturas=" + s.Reader.Calls.Count + " obsoleto=" + draft.IsPossiblyStale);

                var twin = new Setup();
                var other = twin.BeginThenChangeOutside();
                twin.Reader.Answer = _ => EditIntentReadResult.Busy("Hay un comando en curso.");
                other.MarkPossiblyStale();
                same.Add("ocupado=" + SameAsTheReload(outcome.Begin, twin.Port.TryReload(twin.Rig.Request())));
            });

            // The reader throws: the reload contains it as Failed, reports it with its own name and the session is not degraded.
            Paso(log, () =>
            {
                var s = new Setup();
                var draft = s.BeginThenChangeOutside();
                s.Reader.Throw = new InvalidOperationException("boom");
                draft.MarkPossiblyStale();
                var outcome = s.Port.TryEditAll(s.Rig.Request());
                log.Add("excepcion estado=" + outcome.Status + " inicio=" + outcome.Begin.Status + " motivo=" + R(outcome.Begin.Reason)
                    + " abierto=" + (s.Editor.Shown > 0) + " obsoleto=" + draft.IsPossiblyStale
                    + " fallos=[" + string.Join(",", s.Rig.Host.Faults.Select(fault => fault.Context)) + "]");

                var twin = new Setup();
                var other = twin.BeginThenChangeOutside();
                twin.Reader.Throw = new InvalidOperationException("boom");
                other.MarkPossiblyStale();
                same.Add("excepcion=" + SameAsTheReload(outcome.Begin, twin.Port.TryReload(twin.Rig.Request())));
            });

            log.Add("igual-que-editar " + string.Join(" ", same));

            Finish(
                log,
                new[]
                {
                    "no-admitida estado=NotBegun inicio=NotAdmitted diagnostico=[No editable aquí: el origen ya no está en el dibujo] pista=True abierto=False lecturas=1 obsoleto=True borrador=Clean",
                    "ocupado estado=NotBegun inicio=Waiting motivo=[Hay un comando en curso.] abierto=False lecturas=1 obsoleto=True",
                    "excepcion estado=NotBegun inicio=Failed motivo=[boom] abierto=False obsoleto=True fallos=[WorkspaceHostCore.TryReloadPilotEdit]",
                    "igual-que-editar no-admitida=True ocupado=True excepcion=True",
                });
        }

        // ------------------------------------------------------------------------------------------------- CT-04

        [Fact]
        public void CT_PBT4E_04_FromTheChangeOutsideAndItsMarkThroughEditAllToTheUpdateWithoutAutoCadTheUpdateAppliesWithoutAConflict()
        {
            var log = new List<string>();

            Paso(log, () =>
            {
                var before = ThreeViews(3, null);
                var after = ThreeViews(3, OtherName);
                var s = new Setup();
                s.Reader.Answer = rack => FullEditIntentKit.ReadOf(before);
                var draft = s.Rig.Begin();

                // The change outside the panel: the drawing is the renamed one, and the hint of the bridge marks the draft at the next drain.
                s.Reader.Answer = rack => FullEditIntentKit.ReadOf(after);
                s.Rig.Host.Bridge.Core.OnSourceEvent(s.Rig.Session, BridgeSource.ObjectModified);
                s.Rig.Host.Idle();
                s.Reader.Calls.Clear();
                log.Add("marca obsoleto=" + draft.IsPossiblyStale + " borrador=" + StatusOf(draft) + " lecturas=" + s.Reader.Calls.Count);

                // «Editar todo», with one gesture of the editor: one front less and another scale, handed to the draft once.
                s.Editor.Inside = launch =>
                {
                    var edited = PushBackEditableStateComparer.Copy(launch.State);
                    LessOneFront(edited);
                    edited.AnnotationScale = 2.5;
                    launch.Port.ApplyToDraft(edited);
                };
                var outcome = s.Port.TryEditAll(s.Rig.Request());
                log.Add("editar-todo estado=" + outcome.Status + " aplicadas=" + outcome.Applied + " lecturas=" + s.Reader.Calls.Count
                    + " nombre-del-lanzamiento=" + NameOf(s.Editor.Launch) + " borrador=" + StatusOf(draft) + " obsoleto=" + draft.IsPossiblyStale);

                // «Actualizar» over the drawing as it is now: the base of the draft is the one of the drawing, so nothing conflicts.
                var port = new PilotEraseScopeDouble(FullWriteKit.ItemsOf(after));
                var update = s.Rig.Core.TryUpdatePilot(s.Rig.Request(), port);
                log.Add("actualizar estado=" + update.Status + " begin-mutation=" + port.Calls.Count(call => call == "BeginMutation")
                    + " seal=" + port.Calls.Count(call => call == "Seal") + " borrador=" + StatusOf(draft));
                log.Add("borrado " + string.Join(",", port.Calls.Where(call => call.StartsWith("EraseUnit:", StringComparison.Ordinal))));
                var store = new RackEmbedStore();
                log.Add("escrito vistas=" + port.Written.Count + " nombre-del-dibujo=" + (port.Written.Count > 0
                    && port.Written.All(written => store.Deserialize(written.Text).Name == OtherName)));
            });

            Finish(
                log,
                new[]
                {
                    "marca obsoleto=True borrador=Clean lecturas=0",
                    "editar-todo estado=Closed aplicadas=1 lecturas=1 nombre-del-lanzamiento=[Otro nombre] borrador=Dirty obsoleto=False",
                    "actualizar estado=Applied begin-mutation=1 seal=1 borrador=Clean",
                    "borrado EraseUnit:B2",
                    "escrito vistas=2 nombre-del-dibujo=True",
                });
        }
    }
}
