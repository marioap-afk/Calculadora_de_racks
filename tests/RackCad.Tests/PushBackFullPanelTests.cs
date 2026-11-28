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
    /// I-64 F4B PB-T4b — the panel on the FULL draft (CT-PBT4B-01 to 03, 05 to 11 and 17). Every recipe drives the host core of the workspace
    /// over a simulated bridge and a reader, with the real pure cores; nothing opens AutoCAD, a window or a file. The panel draft is the full
    /// one (D-PBT4-04): the draft is looked for as <see cref="PushBackFullDraftState"/> and never as the state of the pilot. Every call goes
    /// through <see cref="Paso{T}"/> (the «EXCEPCION» rule), so a behavior that is missing fails by ASSERTION on the lines, never by an
    /// escaped exception. Culture: invariant; numbers with «R».
    /// </summary>
    public sealed class PushBackFullPanelTests
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

        /// <summary>The usual realistic rack: the frontal view (the origin), a lateral one and the plan view, with an optional name in the three.</summary>
        private static PushBackPilotIntentScan Realistic(string name)
            => IntentKit.Scan(
                FullPrepareKit.Rack,
                "A1",
                IntentKit.Member("A1", FullPrepareKit.Frontal(name: name)),
                IntentKit.Member("B2", FullPrepareKit.Lateral(FullPrepareKit.ExistingPost(), name: name)),
                IntentKit.Member("C3", FullPrepareKit.Planta(name: name)));

        private static PushBackPilotIntentScan ThreeViews(int lateralPost, string origin)
            => IntentKit.Scan(
                FullPrepareKit.Rack,
                origin,
                IntentKit.Member("A1", FullPrepareKit.Frontal()),
                IntentKit.Member("B2", FullPrepareKit.Lateral(lateralPost)),
                IntentKit.Member("C3", FullPrepareKit.Planta()));

        /// <summary>«- frente»: the rack loses its last front, and its lateral of the post 3 becomes obsolete.</summary>
        private static void LessOneFront(PushBackFullDraftState state) => state.Editable.Editor.SetSlotCount(state.Editable.Editor.SlotCount - 1);

        /// <summary>The reader of these recipes: it answers by a function, records every call and, when asked, evaluates the witness of the
        /// request against the origin of the read as the reader of the Plugin does.</summary>
        private sealed class PanelReader : IWorkspaceEditIntentReader, IWorkspaceEditIntentOriginReader
        {
            public Func<string, EditIntentReadResult> Answer { get; set; }

            public bool EvaluatesWitness { get; set; }

            public List<string> Kinds { get; } = new List<string>();

            public List<string> Origins { get; } = new List<string>();

            public EditIntentReadResult ReadIntent(SessionId session, string rackId, long? chosenReference, EditIntentWitness witness)
            {
                Kinds.Add("ReadIntent");
                Origins.Add(null);
                return Seen(rackId, witness);
            }

            public EditIntentReadResult ReadIntentAtOrigin(SessionId session, string rackId, string originText, EditIntentWitness witness)
            {
                Kinds.Add("ReadIntentAtOrigin");
                Origins.Add(originText);
                return Seen(rackId, witness);
            }

            private EditIntentReadResult Seen(string rackId, EditIntentWitness witness)
            {
                var read = Answer(rackId);
                return EvaluatesWitness && witness != null ? FullHandKit.Seen(read, witness) : read;
            }
        }

        /// <summary>The host of the workspace with a document and a reader.</summary>
        private sealed class Rig
        {
            public Rig(Func<string, EditIntentReadResult> answer, bool evaluatesWitness = false)
            {
                Host = new WorkspaceHostRig().WithDocument(100);
                Host.Host.OpenPanel();
                Session = Host.ActiveSession;
                Host.Idle();
                Host.Reader.Reads.Clear();
                Reader = new PanelReader { Answer = answer, EvaluatesWitness = evaluatesWitness };
                Host.Host.SetEditIntentReader(Reader);
            }

            public WorkspaceHostRig Host { get; }

            public WorkspaceHostCore Core => Host.Host;

            public IWorkspacePilotEditPort Port => Host.Host;

            public PanelReader Reader { get; }

            public SessionId Session { get; }

            public WorkspaceRequest Origin => new WorkspaceRequest(Session);

            public EditIntentRequest Request(string rack = RackA, EditIntentWitness witness = null) => new EditIntentRequest(Session, rack, null, witness);

            public EditIntentOutcome Begin(string rack = RackA) => Core.TryBeginPilotEdit(Request(rack));

            public DraftSession<PushBackFullDraftState> Full(string rack = RackA) => Core.OpenDrafts.Find<PushBackFullDraftState>(Session, rack);

            public DraftSession<PushBackPilotDraftState> Pilot(string rack = RackA) => Core.OpenDrafts.Find<PushBackPilotDraftState>(Session, rack);
        }

        private static bool Same(PushBackFullDraftState a, PushBackFullDraftState b) => PushBackFullDraftStateComparer.Instance.Equals(a, b);

        // ------------------------------------------------------------------------------------------------- CT-01

        [Fact]
        public void CT_PBT4B_01_TheDraftOfThePanelIsTheFullOneAndAReadWithoutTheFullModelBeginsNoDraft()
        {
            var log = new List<string>();

            Paso(log, () =>
            {
                var scan = FullEditIntentKit.TwoViews(RackA);
                var read = FullEditIntentKit.ReadOf(scan);
                var rig = new Rig(rack => read);
                Paso(log, () => rig.Begin());
                var full = rig.Full();
                var pilot = rig.Pilot();
                var expected = PushBackFullDraftReader.Read(scan, PilotKit.Catalog).Read.InitialState;
                log.Add("begin tipo=" + (full == null ? "null" : full.CurrentDraftState.GetType().Name)
                    + " piloto=" + (pilot == null ? "null" : "no-nulo")
                    + " editable-igual=" + (full != null && Same(full.CurrentDraftState, expected))
                    + " origen-igual=" + (full != null && string.Equals(full.CurrentDraftState.Origin, scan.RawEnvelopes["A1"], StringComparison.Ordinal)));
            });

            Paso(log, () =>
            {
                var rig = new Rig(rack => EditIntentReadResult.FromPilot(PushBackPilotIntentReader.Read(FullEditIntentKit.TwoViews(rack), PilotKit.Catalog)));
                var outcome = rig.Begin();
                log.Add("begin-sin-completo estado=" + outcome.Status + " diagnostico=" + outcome.Diagnostic
                    + " pista=" + !string.IsNullOrEmpty(outcome.ClassicEditorHint) + " borradores=" + rig.Core.OpenDrafts.CountOf(rig.Session));
            });

            Finish(
                log,
                new[]
                {
                    "begin tipo=PushBackFullDraftState piloto=null editable-igual=True origen-igual=True",
                    "begin-sin-completo estado=NotAdmitted diagnostico=No editable aquí: la lectura no trae el modelo completo. pista=True borradores=0",
                });
        }

        // ------------------------------------------------------------------------------------------------- CT-02

        [Fact]
        public void CT_PBT4B_02_ABRackOpensItsFullDraftAndTheTailAndTheFutureVersionStayRejected()
        {
            var log = new List<string>();

            Paso(log, () =>
            {
                var text = PilotKit.Text("pb-20-ab");
                var id = new RackEmbedStore().Deserialize(text).Id;
                var read = FullEditIntentKit.ReadOf(IntentKit.Scan(id, "H1", IntentKit.Member("H1", text)));
                var rig = new Rig(rack => read);
                var outcome = rig.Begin(id);
                var draft = rig.Full(id);
                log.Add("pb-20-ab estado=" + outcome.Status + " borrador=" + (draft == null ? "null" : draft.CurrentDraftState.GetType().Name));
            });

            Paso(log, () =>
            {
                var text = PilotKit.Text("pb-21-cola");
                var id = new RackEmbedStore().Deserialize(text).Id;
                var read = FullEditIntentKit.ReadOf(IntentKit.Scan(id, "H1", IntentKit.Member("H1", text)));
                var rig = new Rig(rack => read);
                var outcome = rig.Begin(id);
                log.Add("pb-21-cola estado=" + outcome.Status
                    + " diagnostico-fuera-del-piloto=" + string.Equals(outcome.Diagnostic, PushBackPilotIntentReader.OutOfPilotDiagnostic, StringComparison.Ordinal)
                    + " pista=" + !string.IsNullOrEmpty(outcome.ClassicEditorHint)
                    + " borrador=" + (rig.Full(id) == null ? "null" : "no-nulo"));
            });

            Paso(log, () =>
            {
                var rich = I58F1Fixtures.Rich("pushback");
                var future = IntentKit.Rewrite(IntentKit.BaseText(), e => e.Design = I58F1Fixtures.Set(rich, "PushBack.Future", "1"));
                var read = FullEditIntentKit.ReadOf(IntentKit.Scan(IntentKit.Rack, "A1", IntentKit.Member("A1", future)));
                var rig = new Rig(rack => read);
                var outcome = rig.Begin();
                log.Add("auth-13 estado=" + outcome.Status + " borrador=" + (rig.Full() == null ? "null" : "no-nulo"));
            });

            Finish(
                log,
                new[]
                {
                    "pb-20-ab estado=Admitted borrador=PushBackFullDraftState",
                    "pb-21-cola estado=NotAdmitted diagnostico-fuera-del-piloto=True pista=True borrador=null",
                    "auth-13 estado=NotAdmitted borrador=null",
                });
        }

        // ------------------------------------------------------------------------------------------------- CT-03

        [Fact]
        public void CT_PBT4B_03_TheScaleFieldChangesOnlyTheScaleOfTheFullDraftAndValidatesWithTheOneRule()
        {
            var log = new List<string>();

            Paso(log, () =>
            {
                var rig = new Rig(rack => FullEditIntentKit.Admitted(rack));
                Paso(log, () => rig.Begin());
                var draft = rig.Full();
                var expected = PushBackFullDraftStateComparer.Copy(draft.CurrentDraftState);
                expected.Editable.AnnotationScale = 2.5;
                rig.Port.SetPendingText(rig.Origin, RackA, "2.5");
                var commit = rig.Port.CommitField(rig.Origin, RackA);
                log.Add("campo commit=" + commit.Status + " escala=" + Num(draft.CurrentDraftState.Editable.AnnotationScale)
                    + " resto-igual=" + Same(draft.CurrentDraftState, expected) + " isdirty=" + draft.IsDirty);
            });

            foreach (var text in new[] { string.Empty, "abc", "0", "-1" })
            {
                Paso(log, () =>
                {
                    var rig = new Rig(rack => FullEditIntentKit.Admitted(rack));
                    Paso(log, () => rig.Begin());
                    rig.Port.SetPendingText(rig.Origin, RackA, text);
                    var commit = rig.Port.CommitField(rig.Origin, RackA);
                    log.Add("campo texto=[" + text + "] estado=" + commit.Status + " mensaje=" + commit.Message);
                });
            }

            Finish(
                log,
                new[]
                {
                    "campo commit=Applied escala=2.5 resto-igual=True isdirty=True",
                    "campo texto=[] estado=Invalid mensaje=Escribe un valor.",
                    "campo texto=[abc] estado=Invalid mensaje=Valor no numérico.",
                    "campo texto=[0] estado=Invalid mensaje=El valor debe ser > 0.",
                    "campo texto=[-1] estado=Invalid mensaje=El valor debe ser > 0.",
                });
        }

        // ------------------------------------------------------------------------------------------------- CT-05

        [Fact]
        public void CT_PBT4B_05_UpdateRunsTheFullWriteAndErasesTheViewThatTheDraftLeavesObsolete()
        {
            var log = new List<string>();

            Paso(log, () =>
            {
                var scan = ThreeViews(3, "A1");
                var read = FullEditIntentKit.ReadOf(scan);
                var rig = new Rig(rack => read);
                Paso(log, () => rig.Begin());
                var draft = rig.Full();
                FullWriteKit.Edit(draft, LessOneFront);
                var port = new PilotEraseScopeDouble(FullWriteKit.ItemsOf(scan));
                var outcome = rig.Core.TryUpdatePilot(rig.Request(), port);
                log.Add("actualizar estado=" + outcome.Status + " begin-mutation=" + port.Calls.Count(call => call == "BeginMutation")
                    + " seal=" + port.Calls.Count(call => call == "Seal") + " borrador=" + draft.Status);
                log.Add("borrado " + string.Join(",", port.Calls.Where(call => call.StartsWith("EraseUnit:", StringComparison.Ordinal))));
            });

            Finish(
                log,
                new[]
                {
                    "actualizar estado=Applied begin-mutation=1 seal=1 borrador=Clean",
                    "borrado EraseUnit:B2",
                });
        }

        // ------------------------------------------------------------------------------------------------- CT-06

        /// <summary>A draft of the panel that the user edited (the scale 2.25), marked as possibly stale, with a reader that now answers
        /// another version of the drawing: the three views with another name.</summary>
        private sealed class KeptSetup
        {
            public KeptSetup(bool evaluatesWitness)
            {
                Before = Realistic(null);
                After = Realistic("Otro nombre");
                var read = FullEditIntentKit.ReadOf(Before);
                Rig = new Rig(rack => read, evaluatesWitness);
                Rig.Begin();
                Draft = Rig.Full();
                FullWriteKit.Edit(Draft, state => state.Editable.AnnotationScale = 2.25);
                Draft.MarkPossiblyStale();
                var rereadAfter = FullEditIntentKit.ReadOf(After);
                Rig.Reader.Answer = rack => rereadAfter;
                Reread = rereadAfter;
            }

            public PushBackPilotIntentScan Before { get; }

            public PushBackPilotIntentScan After { get; }

            public Rig Rig { get; }

            public DraftSession<PushBackFullDraftState> Draft { get; }

            public EditIntentReadResult Reread { get; }
        }

        private static PushBackConflictClassification Classification(
            IEnumerable<PushBackChangeEntry> entries, IEnumerable<string> viewsToErase, IEnumerable<string> viewsRejected, bool allViewsObsolete)
            => new PushBackConflictClassification(entries, viewsToErase, viewsRejected, allViewsObsolete);

        private static PushBackChangeEntry Entry(PushBackChangeGroup group, PushBackChangeClass changeClass, bool structural, string path)
            => new PushBackChangeEntry(PushBackDocumentLevel.Payload, path, group, changeClass, structural, "base", "fuera", "borrador");

        [Fact]
        public void CT_PBT4B_06_TheFirstYesOfKeepMineOnlyPresentsTheChangesByGroupsAndTheDraftDoesNotChange()
        {
            var log = new List<string>();

            // The host: «Conservar» without a witness, on a Dirty draft marked as possibly stale.
            Paso(log, () =>
            {
                var setup = new KeptSetup(false);
                var current = PushBackFullDraftStateComparer.Copy(setup.Draft.CurrentDraftState);
                var initial = setup.Draft.InitialDraftState;
                var before = Fingerprint(setup.Draft.CurrentDraftState) + Fingerprint(setup.Draft.InitialDraftState);
                var outcome = setup.Rig.Core.TryKeepPilotEdit(setup.Rig.Request());
                var witness = outcome.KeepWitness;
                log.Add("presentar estado=" + outcome.Status
                    + " testigo-clave=" + (witness == null ? "null" : witness.Key)
                    + " testigo-texto-igual-al-origen=" + (witness != null && string.Equals(witness.Text, setup.After.RawEnvelopes["A1"], StringComparison.Ordinal))
                    + " borrador-igual=" + (Same(setup.Draft.CurrentDraftState, current) && Same(setup.Draft.InitialDraftState, initial))
                    + " huella-igual=" + (before == Fingerprint(setup.Draft.CurrentDraftState) + Fingerprint(setup.Draft.InitialDraftState)));
                foreach (var line in outcome.PresentationLines) log.Add("presentar " + line);
            });

            // The pure function, over a classification built by hand.
            Paso(log, () =>
            {
                var entries = new[]
                {
                    Entry(PushBackChangeGroup.Name, PushBackChangeClass.ChangedExternally, false, "$.Name"),
                    Entry(PushBackChangeGroup.Fronts, PushBackChangeClass.ChangedExternally, false, "$.Fronts[0].Levels"),
                    Entry(PushBackChangeGroup.Fronts, PushBackChangeClass.ChangedExternally, true, "$.Fronts[1]"),
                    Entry(PushBackChangeGroup.Annotations, PushBackChangeClass.ChangedByUser, false, "$.Annotations.AnnotationScale"),
                    Entry(PushBackChangeGroup.Safety, PushBackChangeClass.ChangedByUser, false, "$.SafetySelections[0]"),
                    Entry(PushBackChangeGroup.Modules, PushBackChangeClass.ChangedByUser, true, "$.Modules[0]"),
                    Entry(PushBackChangeGroup.Rack, PushBackChangeClass.Overlapping, false, "$.PalletsDeep"),
                };
                var full = PushBackKeepMinePresentation.Of(Classification(entries, new[] { "B2", "C3" }, new[] { "D4" }, true));
                log.Add("of lineas=" + full.Lines.Count);
                foreach (var line in full.Lines) log.Add("of " + line);
                log.Add("of-singular " + string.Join("|", PushBackKeepMinePresentation.Of(Classification(new PushBackChangeEntry[0], new[] { "B2" }, new string[0], false)).Lines));
                log.Add("of-plural " + string.Join("|", PushBackKeepMinePresentation.Of(Classification(new PushBackChangeEntry[0], new string[0], new[] { "B2", "C3", "D4" }, false)).Lines));
                var empty = PushBackKeepMinePresentation.Of(Classification(new PushBackChangeEntry[0], new string[0], new string[0], false));
                log.Add("of-vacia lineas=" + empty.Lines.Count + " isempty=" + empty.IsEmpty);
            });

            Finish(
                log,
                new[]
                {
                    "presentar estado=Presented testigo-clave=A1 testigo-texto-igual-al-origen=True borrador-igual=True huella-igual=True",
                    "presentar Cambiado fuera: el nombre.",
                    "presentar Cambiado por ti: las anotaciones.",
                    "of lineas=7",
                    "of Cambiado fuera: el nombre y los frentes.",
                    "of Cambiado por ti: las anotaciones, la seguridad y los módulos.",
                    "of Cambiado en los dos: el rack.",
                    "of Cambios estructurales: los frentes y los módulos.",
                    "of Vistas: se borrarán 2 laterales obsoletos.",
                    "of Vistas: 1 no se puede conservar.",
                    "of Todas las vistas quedarían obsoletas.",
                    "of-singular Vistas: se borrará 1 lateral obsoleto.",
                    "of-plural Vistas: 3 no se pueden conservar.",
                    "of-vacia lineas=0 isempty=True",
                });
        }

        // ------------------------------------------------------------------------------------------------- CT-07

        [Fact]
        public void CT_PBT4B_07_TheSecondYesKeepsOnlyWithTheWitnessThatStillHoldsAndOtherwisePresentsAgain()
        {
            var log = new List<string>();

            // A witness that no longer holds: the drawing changed again after the presentation.
            Paso(log, () =>
            {
                var setup = new KeptSetup(true);
                var presented = setup.Rig.Core.TryKeepPilotEdit(setup.Rig.Request());
                var witness = presented.KeepWitness;
                var changedAgain = FullEditIntentKit.ReadOf(Realistic("Tercer nombre"));
                setup.Rig.Reader.Answer = rack => changedAgain;
                var current = PushBackFullDraftStateComparer.Copy(setup.Draft.CurrentDraftState);
                var initial = setup.Draft.InitialDraftState;
                var second = setup.Rig.Core.TryKeepPilotEdit(setup.Rig.Request(RackA, witness));
                log.Add("segundo-si testigo-viejo estado=" + second.Status + " conservado=" + (second.Status == EditIntentStatus.Kept)
                    + " borrador-igual=" + (Same(setup.Draft.CurrentDraftState, current) && Same(setup.Draft.InitialDraftState, initial)));
            });

            // The witness that still holds: «KeepOn» over the reread.
            Paso(log, () =>
            {
                var setup = new KeptSetup(true);
                var presented = setup.Rig.Core.TryKeepPilotEdit(setup.Rig.Request());
                var witness = presented.KeepWitness;
                var current = PushBackFullDraftStateComparer.Copy(setup.Draft.CurrentDraftState);
                var second = setup.Rig.Core.TryKeepPilotEdit(setup.Rig.Request(RackA, witness));
                var reread = setup.Reread.FullRead.Read.InitialState;
                log.Add("segundo-si testigo-vigente estado=" + second.Status
                    + " initial-igual-a-la-relectura=" + Same(setup.Draft.InitialDraftState, reread)
                    + " editable-del-borrador=" + PushBackEditableStateComparer.Instance.Equals(setup.Draft.CurrentDraftState.Editable, current.Editable)
                    + " origen-nuevo=" + string.Equals(setup.Draft.CurrentDraftState.Origin, setup.After.RawEnvelopes["A1"], StringComparison.Ordinal)
                    + " estado-del-borrador=" + setup.Draft.Status);
            });

            Finish(
                log,
                new[]
                {
                    "segundo-si testigo-viejo estado=Presented conservado=False borrador-igual=True",
                    "segundo-si testigo-vigente estado=Kept initial-igual-a-la-relectura=True editable-del-borrador=True origen-nuevo=True estado-del-borrador=Dirty",
                });
        }

        // ------------------------------------------------------------------------------------------------- CT-08

        [Fact]
        public void CT_PBT4B_08_AfterKeepingAndUpdatingEveryMemberCarriesTheDesignOfTheDraftOverTheRestOfTheCurrentRack()
        {
            var log = new List<string>();

            Paso(log, () =>
            {
                var setup = new KeptSetup(true);
                var presented = setup.Rig.Core.TryKeepPilotEdit(setup.Rig.Request());
                setup.Rig.Core.TryKeepPilotEdit(setup.Rig.Request(RackA, presented.KeepWitness));
                var port = new PilotUpdateScopeDouble(FullWriteKit.ItemsOf(setup.After));
                var outcome = setup.Rig.Core.TryUpdatePilot(setup.Rig.Request(), port);
                var store = new RackEmbedStore();
                var design = port.Written.Count > 0;
                var rest = port.Written.Count > 0;
                foreach (var (key, text) in port.Written)
                {
                    var written = store.Deserialize(text);
                    var now = store.Deserialize(setup.After.RawEnvelopes[key]);
                    var materialized = PushBackFullDraftMaterializer.Materialize(text, PilotKit.Catalog);
                    design = design && materialized.IsAdmitted && materialized.State.Editable.AnnotationScale == 2.25;
                    // The name is of the draft, which keeps it (it is an edited field of the design); the rest of the envelope, the identity, the
                    // version, the kind, the view and the section, is the one of the current rack.
                    rest = rest && written.Id == now.Id && written.SchemaVersion == now.SchemaVersion
                        && written.Kind == now.Kind && written.View == now.View && written.Section == now.Section;
                }

                log.Add("tras-conservar-y-actualizar estado=" + outcome.Status + " diseno-del-borrador=" + design + " resto-del-rack-actual=" + rest);
            });

            Finish(log, new[] { "tras-conservar-y-actualizar estado=Applied diseno-del-borrador=True resto-del-rack-actual=True" });
        }

        // ------------------------------------------------------------------------------------------------- CT-09

        [Fact]
        public void CT_PBT4B_09_ASideBDeclaredWithoutFrontsIsWrittenWithoutSideBAndTheDraftKeepsTheDeclarationUntilTheRebase()
        {
            var log = new List<string>();

            Paso(log, () =>
            {
                var scan = Realistic(null);
                var read = FullEditIntentKit.ReadOf(scan);
                var rig = new Rig(rack => read);
                Paso(log, () => rig.Begin());
                var draft = rig.Full();
                FullWriteKit.Edit(draft, state =>
                {
                    state.Editable.AnnotationScale = 2.25;
                    state.Editable.Editor.SetSideBPresent(true);
                });
                var declared = draft.CurrentDraftState.Editable.Editor.SideBPresent;
                var port = new PilotUpdateScopeDouble(FullWriteKit.ItemsOf(scan));
                var outcome = rig.Core.TryUpdatePilot(rig.Request(), port);
                var writtenWithoutSideB = outcome.Status == EditUpdateStatus.Applied && port.Written.Count > 0;
                foreach (var (key, text) in port.Written)
                {
                    var materialized = PushBackFullDraftMaterializer.Materialize(text, PilotKit.Catalog);
                    writtenWithoutSideB = writtenWithoutSideB && materialized.IsAdmitted && !materialized.State.Editable.Editor.SideBPresent;
                }

                log.Add("lado-b-sin-frentes diseno-escrito-sin-lado-b=" + writtenWithoutSideB
                    + " declaracion-hasta-el-rebase=" + (declared && !draft.CurrentDraftState.Editable.Editor.SideBPresent));
            });

            Finish(log, new[] { "lado-b-sin-frentes diseno-escrito-sin-lado-b=True declaracion-hasta-el-rebase=True" });
        }

        // ------------------------------------------------------------------------------------------------- CT-10 and the fingerprint

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
        public void CT_PBT4B_10_NoOperationOfThePanelMutatesTheResolvedRackOrTheModuleReportOfTheDraft()
        {
            var log = new List<string>();

            Paso(log, () =>
            {
                var before = Realistic(null);
                var after = Realistic("Otro nombre");
                var readBefore = FullEditIntentKit.ReadOf(before);
                var readAfter = FullEditIntentKit.ReadOf(after);
                var rig = new Rig(rack => readBefore, true);
                var watch = new Watch();

                // Begin: the references of the read that opens the draft.
                watch.Capture(readBefore.FullRead.Read.InitialState);
                Paso(log, () => rig.Begin());
                var draft = rig.Full();
                log.Add("Begin huellas-iguales=" + (watch.Count > 0 && watch.Intact()));
                watch.Capture(draft.InitialDraftState, draft.CurrentDraftState);

                // CommitField: the field of the scale.
                rig.Port.SetPendingText(rig.Origin, RackA, "2.5");
                rig.Port.CommitField(rig.Origin, RackA);
                log.Add("CommitField huellas-iguales=" + (watch.Count > 0 && watch.Intact()));

                // Presentar: the first Yes of «Conservar», over the reread of the other version of the drawing.
                draft.MarkPossiblyStale();
                rig.Reader.Answer = rack => readAfter;
                watch.Capture(readAfter.FullRead.Read.InitialState);
                var presented = rig.Core.TryKeepPilotEdit(rig.Request());
                log.Add("Presentar huellas-iguales=" + (watch.Count > 0 && watch.Intact()));

                // KeepOn: the second Yes, with the witness of the presentation.
                rig.Core.TryKeepPilotEdit(rig.Request(RackA, presented.KeepWitness));
                log.Add("KeepOn huellas-iguales=" + (watch.Count > 0 && watch.Intact()));

                // Rebase: «Actualizar», which rebases the draft on what it wrote.
                watch.Capture(draft.InitialDraftState, draft.CurrentDraftState);
                var port = new PilotUpdateScopeDouble(FullWriteKit.ItemsOf(after));
                rig.Core.TryUpdatePilot(rig.Request(), port);
                log.Add("Rebase huellas-iguales=" + (watch.Count > 0 && watch.Intact()));
            });

            // Replacing a shared reference never makes the draft dirty: the comparison does not depend on them.
            Paso(log, () =>
            {
                var read = FullEditIntentKit.Admitted(RackA);
                var rig = new Rig(rack => read);
                Paso(log, () => rig.Begin());
                var draft = rig.Full();
                FullWriteKit.Edit(draft, state => state.Editable.Editor.SideA.LastModuleReconciliation =
                    (RackModuleReconciliationResult)RuntimeHelpers.GetUninitializedObject(typeof(RackModuleReconciliationResult)));
                log.Add("sustituir-referencia isdirty=" + draft.IsDirty);
            });

            Finish(
                log,
                new[]
                {
                    "Begin huellas-iguales=True",
                    "CommitField huellas-iguales=True",
                    "Presentar huellas-iguales=True",
                    "KeepOn huellas-iguales=True",
                    "Rebase huellas-iguales=True",
                    "sustituir-referencia isdirty=False",
                });
        }

        // ------------------------------------------------------------------------------------------------- CT-11

        [Fact]
        public void CT_PBT4B_11_AModuleLengthThatIsNaNInTheCurrentStateOfThePanelDraftIsNotEqualToItsOwnCopy()
        {
            var log = new List<string>();

            Paso(log, () =>
            {
                var text = PilotKit.Text("pb-02-modulos");
                var id = new RackEmbedStore().Deserialize(text).Id;
                var read = FullEditIntentKit.ReadOf(IntentKit.Scan(id, "A1", IntentKit.Member("A1", text)));
                var rig = new Rig(rack => read);
                Paso(log, () => rig.Begin(id));
                var draft = rig.Full(id);
                FullWriteKit.Edit(draft, state =>
                {
                    var side = state.Editable.Editor.SideA;
                    side.ModuleSession.SetLength(side.ModuleSession.Modules.First().ModuleId, double.NaN);
                    side.CommitModuleEdits();
                });
                log.Add("nan-longitud-modulo igual-a-su-copia=" + Same(draft.CurrentDraftState, PushBackFullDraftStateComparer.Copy(draft.CurrentDraftState)));
            });

            Finish(log, new[] { "nan-longitud-modulo igual-a-su-copia=False" });
        }

        // ------------------------------------------------------------------------------------------------- CT-17

        [Fact]
        public void CT_PBT4B_17_AfterAnUpdateThatErasesTheViewOfTheOriginTheDraftIsCleanAndTheNextRereadIsAtTheNewOrigin()
        {
            var log = new List<string>();

            Paso(log, () =>
            {
                var scan = ThreeViews(3, "B2");
                var read = FullEditIntentKit.ReadOf(scan);
                var rig = new Rig(rack => read);
                Paso(log, () => rig.Begin());
                var draft = rig.Full();
                FullWriteKit.Edit(draft, LessOneFront);
                var port = new PilotEraseScopeDouble(FullWriteKit.ItemsOf(scan));
                rig.Core.TryUpdatePilot(rig.Request(), port);
                var anchored = port.Drawing.First(member => member.Key == "A1").Text;
                log.Add("borrador estado=" + draft.Status + " origen-es-la-primera-redibujada="
                    + string.Equals(draft.CurrentDraftState.Origin, anchored, StringComparison.Ordinal));

                var before = rig.Reader.Kinds.Count;
                rig.Core.TryConfirmPilotBase(rig.Request());
                var kinds = rig.Reader.Kinds.Skip(before).ToList();
                log.Add("relectura lector=" + (kinds.Count == 1 ? kinds[0] : kinds.Count + " lecturas")
                    + " origen-del-borrador=" + (kinds.Count == 1 && string.Equals(rig.Reader.Origins[before], draft.CurrentDraftState.Origin, StringComparison.Ordinal)));
            });

            Finish(
                log,
                new[]
                {
                    "borrador estado=Clean origen-es-la-primera-redibujada=True",
                    "relectura lector=ReadIntentAtOrigin origen-del-borrador=True",
                });
        }
    }
}
