#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using RackCad.Application.CustomProperties;
using RackCad.Application.Persistence;
using RackCad.Application.RackFrames;
using RackCad.Application.Systems.Shared;
using RackCad.Application.VentanaRackCad;
using RackCad.Application.Workspace;
using RackCad.Domain.RackFrames;
using RackCad.UI.VentanaRackCad;
using RackCad.UI.Workspace;
using Xunit;
using static RackCad.UI.Tests.SelectiveWindowFixtures;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// T7 cab (I-64, séptima tanda; the Owner on 2026-11-27: «Las dos» and «Abren la ventana»): the cabecera of its own as the other systems
    /// of the window. «Nuevo» › «Cabecera en esta ventana» is a draft («Borrador N») with the defaults of the classic configurator, inserted
    /// with «Insertar en el dibujo…» (RACKCABECERANUEVO, lateral, planta or both, the automatic name of I-60); «Insertar vista…» of a cabecera
    /// of the drawing is RACKCABECERAINSERTAR with the panel of DG-07; the tab «Propiedades» is read, written and, for a new one, written at
    /// its insertion; RACKCABECERA opens «Cabecera en esta ventana»; «Actualizar todo», the veto of the close and «El dibujo cambió» count it
    /// as the other drafts. Nothing here runs AutoCAD: the door is a double, as in the tests of the other systems.
    /// </summary>
    public class RackCadNewCabeceraTests
    {
        private static readonly SessionId Session = new(7);
        private static readonly RackViewAddress Lateral = RackViewAddress.Whole(DimensionViewKind.Lateral);
        private static readonly RackViewAddress Planta = RackViewAddress.Whole(DimensionViewKind.Planta);
        private const string IdA = "6f1c2a3b-0000-4000-8000-0000000000c1";

        // ------------------------------------------------------------------ the door

        private sealed class CabeceraDoor : IRackCadCabeceraDoorPort, IRackCadShortcutPort, IRackCadUpdateAllDoorPort
        {
            public List<UpdateAllWrite> UpdateAlls { get; } = new();

            public event Action<UpdateAllWritten>? UpdateAllWritten { add { } remove { } }

            public RackCadClassicResult RequestUpdateAll(UpdateAllWrite write)
            {
                UpdateAlls.Add(write);
                return RackCadClassicResult.Started();
            }

            public List<RackDoorRequest> CabeceraReads { get; } = new();

            public List<CabeceraDoorWrite> CabeceraWrites { get; } = new();

            public List<long> Abandoned { get; } = new();

            public RackCadClassicResult Answer { get; set; } = RackCadClassicResult.Started();

            public event Action<RackDesignRead>? DesignRead { add { } remove { } }

            public event Action<RackDesignWritten>? DesignWritten;

            public event Action<long>? RequestStarted;

            public event Action<CabeceraDesignRead>? CabeceraRead;

            public event Action<RackCadShortcutAsk>? ShortcutAsked;

            public event Action<RackCadShortcutAsk>? ShortcutDone;

            public RackCadClassicResult RequestDesign(RackDoorRequest request) => RackCadClassicResult.Started();

            public RackCadClassicResult RequestDynamicUpdate(RackDoorWrite write) => RackCadClassicResult.Started();

            public RackCadClassicResult RequestCabeceraDesign(RackDoorRequest request)
            {
                CabeceraReads.Add(request);
                return RackCadClassicResult.Started();
            }

            public RackCadClassicResult RequestCabeceraUpdate(CabeceraDoorWrite write)
            {
                CabeceraWrites.Add(write);
                return Answer;
            }

            public void Abandon(long requestId) => Abandoned.Add(requestId);

            public void Start(long requestId) => RequestStarted?.Invoke(requestId);

            public void Written(RackDesignWritten written) => DesignWritten?.Invoke(written);

            // What RACKCABECERAABRIR answers to the last read: the cabecera of the drawing, with its properties, through the rule of the door.
            public void AnswerLastRead(RackPropertiesRead? properties = null)
            {
                var request = CabeceraReads.Last();
                CabeceraRead?.Invoke(CabeceraDoorRules.AnswerForOpen(request.RequestId, request.RackId, RackEmbedDocument.KindCabecera, "Cabecera 1",
                    Envelope("Cabecera 1"), properties));
            }

            public void Ask(RackCadShortcutAsk ask) => ShortcutAsked?.Invoke(ask);

            public void Done(RackCadShortcutAsk ask) => ShortcutDone?.Invoke(ask);
        }

        private static string Envelope(string name)
        {
            var header = new RackFrameConfigurationFactory(UiSupport.LoadCatalogSafe()).Build(RackFrameTemplateCatalog.FindStandardOrDefault(), PostId, 132.0, 42.0);
            header.Name = name;
            return new RackProjectStore().Serialize(RackProject.ForSelective(header));
        }

        private static RackPropertiesRead Read(params (string Id, string Name, string Value)[] entries)
            => new(true, false, null, entries.Select(e => new CustomPropertyDraftEntry(e.Id, e.Name, e.Value)).ToList(),
                Array.Empty<RackPropertiesViewSummary>(), 2, "fp-cab");

        private static void Change(CabeceraEditorViewModel editor) => Choice(editor.PanelFields, "pArr").SelectedValue = BracingPattern.XBracing.ToString();

        private static void Add(RackPropertiesTabViewModel tab, string name, string value)
        {
            tab.AddProperty();
            tab.Properties[^1].Name = name;
            tab.Properties[^1].Value = value;
            tab.CommitProperties();
        }

        private sealed class Rig
        {
            public Rig(bool withDoor = true)
            {
                Host = new WorkspacePortDouble { Snapshot = WorkspacePanelFixtures.Ready(session: 7) };
                Summary = new WorkspaceBrowserSummaryPortDouble { ListResponder = (_, _) => BrowserRackList.Ready(Listed.ToArray()) };
                Panel = new WorkspacePanelViewModel(Host, _ => { }, new WorkspaceNavigationPortDouble(), Summary, new WorkspacePilotEditPortDouble());
                Window = new RackCadWindowViewModel(Panel, null, null, withDoor ? Door : null, UiSupport.LoadCatalogSafe);
                Window.Refresh();
            }

            public List<BrowserRackListEntry> Listed { get; } = new()
            {
                new BrowserRackListEntry("CAB-1", "Cabecera 1", RackEmbedDocument.KindCabecera, new[] { "lateral" }),
            };

            public WorkspacePortDouble Host { get; }

            public WorkspaceBrowserSummaryPortDouble Summary { get; }

            public WorkspacePanelViewModel Panel { get; }

            public CabeceraDoor Door { get; } = new();

            public RackCadWindowViewModel Window { get; }

            public CabeceraEditorViewModel StartNew()
            {
                var opening = Window.StartNewCabecera();
                Assert.True(opening.Opened, opening.Reason);
                return Window.NewCabeceraEditor!;
            }

            public CabeceraEditorViewModel OpenExisting(RackPropertiesRead? properties = null)
            {
                Window.OpenRack("CAB-1");
                Window.EditCabeceraHere();
                Door.AnswerLastRead(properties);
                Assert.True(Window.ActiveCabeceraEditor != null, Window.ActionMessage);
                return Window.ActiveCabeceraEditor!;
            }

            // The changes of the database and the starts of commands, as the host presents them (the rig of RackCadDrawingChangeTests).
            private long _changes;
            private int _starts;

            public void ForeignCommand(bool changesTheDrawing = true)
            {
                _starts++;
                Present(WorkspaceActionBlock.CommandActive);
                if (changesTheDrawing) _changes++;
                Present(WorkspaceActionBlock.None);
            }

            private void Present(WorkspaceActionBlock block)
            {
                Host.Snapshot = new WorkspaceHostSnapshot(WorkspaceHostStatus.Ready, new SessionId(7), true,
                    new WorkspaceActionAvailability(block == WorkspaceActionBlock.None, false, block), SelectionContext.None, false, false, _changes, _starts);
                Window.Refresh();
            }
        }

        // ------------------------------------------------------------------ «Nuevo» › «Cabecera en esta ventana»

        [Fact]
        public void CabeceraEnEstaVentanaOpensADraftWithTheDefaultsOfTheClassicConfiguratorAndNoName()
        {
            var rig = new Rig();
            Assert.True(rig.Window.CanStartNewCabecera);
            var editor = rig.StartNew();

            // A draft of «Nuevo»: «Borrador 1 · Cabecera», shown in «Nuevo», not in the drawing, not a rack of the list.
            var item = Assert.Single(rig.Window.Drafts);
            Assert.Equal(("Borrador 1", "Cabecera", "Sin insertar"), (item.Label, item.SystemLabel, item.StatusText));
            Assert.True(rig.Window.IsNuevoPage);
            Assert.True(rig.Window.ShowNewCabeceraEditor);
            Assert.False(rig.Window.ShowNuevoCards);
            Assert.Null(rig.Window.ActiveCabeceraEditor);
            Assert.Contains(editor, rig.Window.CabeceraEditors);
            Assert.True(editor.IsNewRack);
            Assert.True(editor.IsStandalone);

            // The defaults of RACKCABECERA (HardcodedStandardRackFrameService), with no name (T7cab-S1: at the insertion, «Cabecera N»).
            var classic = new HardcodedStandardRackFrameService(UiSupport.LoadCatalogSafe()).CreateDefault();
            Assert.Equal(string.Empty, editor.Name);
            Assert.Equal(RackFrameTemplateCatalog.StandardTemplateId, editor.Editor.TemplateId);
            Assert.Equal(classic.Depth, editor.Editor.Configuration.Depth);
            Assert.Equal(classic.Horizontals.Select(h => h.Elevation).ToArray(), editor.Editor.Configuration.Horizontals.Select(h => h.Elevation).ToArray());
            Assert.Equal(classic.LeftPost.PostCatalogId, editor.Editor.Configuration.LeftPost.PostCatalogId);
            Assert.False(editor.IsDirty);
            Assert.Equal("Limpio", editor.StatusText);
            Assert.Empty(editor.Changes); // it is the standard: nothing to restore
            Assert.Null(rig.Window.UnwrittenWorkOf(Session)); // ND-05: a draft with nothing changed does not veto the close

            // «Actualizar» is not offered: a new cabecera is written with «Insertar en el dibujo…».
            Assert.False(editor.CanUpdate);
            Assert.Equal("Una cabecera nueva se escribe en el dibujo con «Insertar en el dibujo…».", editor.UpdateBlockedText);
            Assert.Equal("Insertar en el dibujo…", editor.InsertViewLabel);
            Assert.True(editor.CanOpenInsertPanel);
            Assert.StartsWith("Cabecera nueva · plantilla «", editor.SubLine, StringComparison.Ordinal);
            editor.RequestWrite();
            Assert.Empty(rig.Door.CabeceraWrites);

            // Without the door of the cabecera there is nothing to start.
            var none = new Rig(withDoor: false);
            Assert.False(none.Window.CanStartNewCabecera);
            Assert.False(none.Window.StartNewCabecera().Opened);
            Assert.Equal("Aquí no se puede crear un rack en el dibujo.", none.Window.ActionMessage);
        }

        [Fact]
        public void InsertarEnElDibujoOffersTheViewsOfTheClassicAndSendsACopyAsANewCabecera()
        {
            var rig = new Rig();
            var editor = rig.StartNew();
            Change(editor);
            Assert.Equal("Sin insertar, con cambios", rig.Window.Drafts.Single().StatusText);
            Assert.Contains("Borrador 1 (Cabecera nueva)", rig.Window.UnwrittenWorkOf(Session)); // D-08

            editor.InsertView();
            var panel = Assert.IsType<RackViewInsertPanelViewModel>(editor.InsertPanel);
            Assert.Equal("Insertar la Cabecera nueva en el dibujo", panel.Title);
            Assert.Equal(new[] { Lateral, Planta }, panel.Choice.Options.Select(o => o.Address).ToArray()); // «Insertar en AutoCAD», «Insertar planta»
            Assert.Equal(new[] { Lateral, Planta }, panel.Choice.ToInsert.ToArray()); // DG-02: a new rack, the first view of each group
            Assert.Equal("Insertar con 2 vistas", panel.InsertLabel);
            Assert.True(panel.CanInsert);

            panel.Mark(Planta, Lateral);
            Assert.True(panel.Insert());

            var write = Assert.Single(rig.Door.CabeceraWrites);
            Assert.True(write.NewRack);
            Assert.Equal(new[] { Planta, Lateral }, write.InsertViews!.ToArray());
            Assert.Equal(rig.Window.Drafts.Single().RackId, write.RackId);
            Assert.Equal(Session, write.Session);
            Assert.Equal(string.Empty, write.Name); // the automatic name of I-60 at the insertion
            Assert.NotSame(editor.Editor.Configuration, write.Cabecera);
            Assert.Equal(BracingPattern.XBracing, write.Cabecera.BracingPanels[0].Arrangement);
            Assert.Null(write.Properties); // no rows: nothing to write
            Assert.Null(CabeceraDoorRules.RefusalOf(write));

            // Locked while AutoCAD writes; the panel goes away; the one request of the window waits.
            Assert.True(editor.IsWriting);
            Assert.False(editor.CanEdit);
            Assert.False(editor.CanOpenInsertPanel);
            Assert.Null(editor.InsertPanel);
            Assert.Equal("Escribiendo en el dibujo", rig.Window.Drafts.Single().StatusText);
            Assert.True(rig.Window.IsWaitingForAutoCad);
            rig.Window.StartNewCabecera(); // another draft can be opened meanwhile…
            Assert.True(rig.Window.ShowNewCabeceraEditor);
        }

        [Fact]
        public void ThePlacedCabeceraIsACabeceraOfTheDrawingWithTheNameTheDrawingGaveItAndActualizarWritesItThen()
        {
            var rig = new Rig();
            var editor = rig.StartNew();
            Change(editor);
            editor.InsertView();
            editor.InsertPanel!.Insert();
            var write = rig.Door.CabeceraWrites.Single();
            rig.Door.Start(write.RequestId);
            Assert.Contains("ya tomó la escritura", editor.InsertPanelBlockedText);

            // RACKCABECERANUEVO placed it with two views and the dispatch named it «Cabecera 3» (I-60).
            rig.Door.Written(RackDoorRules.AnswerForNewRack(write.RequestId, write.RackId, 2, "Cabecera 3"));

            Assert.False(editor.IsNewRack);
            Assert.False(editor.IsWriting);
            Assert.Equal("Cabecera 3", editor.Name);
            Assert.False(editor.IsDirty); // what the drawing holds: the cabecera sent, with that name
            Assert.Equal("Limpio", editor.StatusText);
            Assert.Equal("Insertar vista…", editor.InsertViewLabel);
            Assert.Empty(rig.Window.Drafts);
            Assert.StartsWith("AutoCAD insertó el rack nuevo «Cabecera 3» con 2 vistas.", rig.Window.ActionMessage, StringComparison.Ordinal);
            Assert.Null(rig.Window.UnwrittenWorkOf(Session));
            Assert.Contains(editor, rig.Window.CabeceraEditors); // the window keeps its editor

            // Once the list reads it, its page shows the same editor, and «Actualizar» writes it as any cabecera of the drawing.
            rig.Listed.Add(new BrowserRackListEntry(write.RackId, "Cabecera 3", RackEmbedDocument.KindCabecera, new[] { "lateral", "planta" }));
            rig.Window.RackList.Reload();
            rig.Window.OpenRack(write.RackId);
            Assert.Same(editor, rig.Window.ActiveCabeceraEditor);
            Assert.False(rig.Window.ShowCabeceraOffer);
            Choice(editor.PanelFields, "pArr").SelectedValue = BracingPattern.SingleDiagonal.ToString();
            Assert.True(editor.CanUpdate, editor.UpdateBlockedText);
            editor.RequestWrite();
            var update = rig.Door.CabeceraWrites.Last();
            Assert.False(update.NewRack);
            Assert.False(update.Inserts);
            Assert.Equal("Cabecera 3", update.Name);
        }

        [Fact]
        public void ACancelledJigWritesNothingAndTheDraftKeepsItsChanges()
        {
            var rig = new Rig();
            var editor = rig.StartNew();
            Change(editor);
            editor.Name = "Nave A";
            editor.InsertView();
            editor.InsertPanel!.Mark(Lateral);
            editor.InsertPanel!.Insert();
            var first = rig.Door.CabeceraWrites.Single();

            // Esc in the jig: RACKCABECERANUEVO placed nothing.
            rig.Door.Written(RackDoorRules.AnswerForNewRack(first.RequestId, first.RackId, 0, null));
            Assert.True(editor.IsNewRack);
            Assert.False(editor.IsWriting);
            Assert.True(editor.IsDirty);
            Assert.Equal("Nave A", editor.Name);
            Assert.Equal(BracingPattern.XBracing, editor.Editor.Configuration.BracingPanels[0].Arrangement);
            Assert.True(editor.MessageIsError);
            Assert.StartsWith("AutoCAD no insertó el rack nuevo", editor.Message, StringComparison.Ordinal);
            Assert.Equal("Sin insertar, con cambios", Assert.Single(rig.Window.Drafts).StatusText);
            Assert.Contains("Nave A (Borrador 1, Cabecera nueva)", rig.Window.UnwrittenWorkOf(Session));

            // The next «Insertar en el dibujo…» sends it again, with the same identity and its name.
            editor.InsertView();
            Assert.NotNull(editor.InsertPanel);
            editor.InsertPanel!.Insert();
            var second = rig.Door.CabeceraWrites.Last();
            Assert.Equal(2, rig.Door.CabeceraWrites.Count);
            Assert.Equal((first.RackId, "Nave A", true), (second.RackId, second.Name, second.NewRack));
        }

        [Fact]
        public void DuplicarAndDescartarOfACabeceraDraftAreTheOnesOfTheOtherDrafts()
        {
            var rig = new Rig();
            var original = rig.StartNew();
            Change(original);
            original.Name = "Nave A";

            var opening = rig.Window.DuplicateDraft(rig.Window.Drafts.Single());
            Assert.True(opening.Opened, opening.Reason);
            var copy = rig.Window.NewCabeceraEditor!;
            Assert.NotSame(original, copy);
            Assert.Equal(2, rig.Window.Drafts.Count);
            Assert.Equal(string.Empty, copy.Name); // the copy has no name: two cabeceras would share it
            Assert.Equal(BracingPattern.XBracing, copy.Editor.Configuration.BracingPanels[0].Arrangement);
            Assert.True(copy.IsDirty); // its original was «Modificado» (ND-07)
            Assert.StartsWith("Borrador 2: Cabecera nueva en esta ventana, copia de Nave A.", copy.Message, StringComparison.Ordinal);

            // They are their own frames; «Descartar» of the copy goes back to how its original opened (the defaults).
            copy.Name = "Copia";
            Assert.Equal("Nave A", original.Name);
            copy.Discard();
            Assert.False(copy.IsDirty);
            Assert.Equal(string.Empty, copy.Name);
            Assert.NotEqual(BracingPattern.XBracing, copy.Editor.Configuration.BracingPanels[0].Arrangement);
            Assert.Equal(BracingPattern.XBracing, original.Editor.Configuration.BracingPanels[0].Arrangement);
            Assert.Equal(RackCadDraftNaming.CopyDiscardedText, copy.Message);
            Assert.True(copy.IsNewRack);

            original.Discard();
            Assert.Equal(RackCadDraftNaming.DiscardedText, original.Message);
            Assert.False(original.IsDirty);
            Assert.Equal(string.Empty, original.Name);

            // «Descartar el borrador» forgets it: it was not in the drawing.
            rig.Window.DiscardDraft(rig.Window.Drafts.First());
            Assert.Single(rig.Window.Drafts);
            Assert.DoesNotContain(original, rig.Window.CabeceraEditors);
        }

        // ------------------------------------------------------------------ «Insertar vista…» of a cabecera of the drawing

        [Fact]
        public void InsertarVistaOfACabeceraOfTheDrawingWritesItInItsViewsThenInsertsTheNewOnes()
        {
            var rig = new Rig();
            var editor = rig.OpenExisting();
            Assert.Equal("Insertar vista…", editor.InsertViewLabel);
            Change(editor);

            editor.InsertView();
            var panel = editor.InsertPanel!;
            Assert.Equal("Insertar vistas de Cabecera 1", panel.Title);
            Assert.Empty(panel.Choice.ToInsert); // a rack of the drawing opens with nothing marked (DG-02)
            Assert.Contains("En el dibujo ya hay: lateral", panel.ExistingText, StringComparison.Ordinal); // DG-04
            Assert.StartsWith("Hay cambios: AutoCAD escribe antes el borrador en las vistas que ya existen", panel.Note, StringComparison.Ordinal);
            panel.Mark(Planta);
            Assert.True(panel.Insert());

            var write = Assert.Single(rig.Door.CabeceraWrites);
            Assert.False(write.NewRack);
            Assert.Equal(new[] { Planta }, write.InsertViews!.ToArray());
            Assert.Equal(("CAB-1", "Cabecera 1"), (write.RackId, write.Name));
            Assert.Null(write.Properties); // VA-S10: «Insertar vista…» does not write the tab
            Assert.Equal(BracingPattern.XBracing, write.Cabecera.BracingPanels[0].Arrangement);
            Assert.True(editor.IsWriting);
            Assert.Null(editor.InsertPanel);

            // RACKCABECERAINSERTAR redrew the lateral with the cabecera and placed the planta (B-1 in the Plugin, its guards in Core).
            rig.Door.Written(RackDoorRules.AnswerForInsert(write.RequestId, "CAB-1", redrawn: 1, erased: 0, inserted: 1));
            Assert.Equal("Limpio", editor.StatusText);
            Assert.Equal("AutoCAD redibujó 1 vista del rack con el borrador e insertó 1 vista nueva.", editor.Message);
            Assert.Null(editor.InsertPanel); // the panel does not come back after a write AutoCAD took

            // A redraw that did not complete: the views were not placed (T6 b1); the answer says it and the cabecera stays not written.
            Choice(editor.PanelFields, "pArr").SelectedValue = BracingPattern.SingleDiagonal.ToString();
            Assert.True(editor.IsDirty);
            editor.InsertView();
            editor.InsertPanel!.Mark(Lateral);
            editor.InsertPanel!.Insert();
            var again = rig.Door.CabeceraWrites.Last();
            rig.Door.Written(RackDoorRules.AnswerForInsert(again.RequestId, "CAB-1", redrawn: 0, erased: 0, inserted: 0));
            Assert.Equal("Modificado", editor.StatusText);
            Assert.True(editor.MessageIsError);
        }

        [Fact]
        public void TheDoorRefusingTheWriteGivesThePanelBackAndAnotherRequestOrAChangedDrawingSayWhy()
        {
            var rig = new Rig();
            var editor = rig.OpenExisting();

            // AutoCAD is busy: nothing was handed over, the editor can be edited again and the panel comes back with the views chosen.
            rig.Door.Answer = RackCadClassicResult.Without(RackCadClassicStatus.Busy, "orden en curso");
            editor.InsertView();
            editor.InsertPanel!.Mark(Planta);
            editor.InsertPanel!.Insert();
            Assert.Single(rig.Door.CabeceraWrites);
            Assert.False(editor.IsWriting);
            Assert.False(rig.Window.IsWaitingForAutoCad);
            Assert.StartsWith("Hay un comando en curso en AutoCAD", editor.Message, StringComparison.Ordinal);
            Assert.NotNull(editor.InsertPanel);
            Assert.Equal(new[] { Planta }, editor.InsertPanel!.Choice.ToInsert.ToArray());
            rig.Door.Answer = RackCadClassicResult.Started();

            // Another request of the window waits: the panel says why, next to «Insertar N vistas»; «Dejar de esperar» frees it.
            rig.Window.StartNewCabecera();
            var draft = rig.Window.NewCabeceraEditor!;
            draft.InsertView();
            draft.InsertPanel!.Insert();
            rig.Window.OpenRack("CAB-1");
            Assert.False(editor.CanInsertView);
            Assert.NotEqual(string.Empty, editor.InsertViewBlockedText);
            Assert.Equal(editor.InsertViewBlockedText, editor.InsertPanel!.Reason);
            Assert.False(editor.InsertPanel!.CanInsert);
            rig.Window.StopWaiting();
            Assert.True(editor.CanInsertView, editor.InsertViewBlockedText);
            Assert.Equal(string.Empty, editor.InsertPanel!.Reason);
            Assert.False(draft.IsWriting);
            Assert.True(draft.IsNewRack);

            // A drawing marked «El dibujo cambió» stops «Insertar vista…» until it is read again (T6st-S5); a new draft is never marked.
            rig.ForeignCommand();
            Assert.True(editor.DrawingChange.IsChanged);
            Assert.False(draft.DrawingChange.IsChanged);
            Assert.Equal(editor.DrawingChange.InsertBlockedText, editor.InsertViewBlockedText);
            Assert.False(editor.InsertPanel!.CanInsert);
            Assert.Equal(string.Empty, draft.InsertViewBlockedText);
        }

        // ------------------------------------------------------------------ «Propiedades» of a cabecera of its own (VA-07)

        [Fact]
        public void EditarAquiReadsThePropertiesOfTheCabeceraAndActualizarWritesThem()
        {
            var rig = new Rig();
            var editor = rig.OpenExisting(Read((IdA, "Zona", "A")));
            var tab = editor.PropertiesTab;
            Assert.True(tab.IsEditable);
            Assert.False(tab.IsNewRack);
            Assert.Equal(("Zona", "A"), (tab.Properties.Single().Name, tab.Properties.Single().Value));
            Assert.False(editor.IsDirty);

            // A row is part of the draft; a wrong one stops «Actualizar» with its reason.
            tab.Properties[0].Value = "B";
            tab.CommitProperties();
            Assert.True(editor.IsDirty);
            Assert.Equal("Modificado", editor.StatusText);
            Add(tab, "zona", "C");
            Assert.False(editor.CanUpdate);
            Assert.Equal("Propiedades: hay 1 valor no válido.", editor.UpdateBlockedText);
            tab.RemoveProperty(tab.Properties[1]);
            Assert.True(editor.CanUpdate, editor.UpdateBlockedText);

            editor.RequestWrite();
            var write = rig.Door.CabeceraWrites.Single();
            Assert.Equal("fp-cab", write.Properties!.Fingerprint);
            Assert.Equal((IdA, "B"), (write.Properties.Entries.Single().Id, write.Properties.Entries.Single().Value));
            Assert.False(tab.CanEditProperties); // locked while AutoCAD writes

            rig.Door.Written(new RackDesignWritten(write.RequestId, "CAB-1", true, "Escrito. También escribió sus propiedades.", Properties: Read((IdA, "Zona", "B"))));
            Assert.False(editor.IsDirty);
            Assert.True(tab.CanEditProperties);
            Assert.Equal("B", tab.Properties.Single().Value);

            // «Descartar» drops the rows that were not written.
            Add(tab, "Nave", "2");
            Assert.True(editor.IsDirty);
            editor.Discard();
            Assert.False(editor.IsDirty);
            Assert.Single(tab.Properties);
        }

        [Fact]
        public void ANewCabeceraWritesItsPropertiesAtItsInsertion()
        {
            var rig = new Rig();
            var editor = rig.StartNew();
            var tab = editor.PropertiesTab;
            Assert.True(tab.IsNewRack);
            Add(tab, "Zona", "A");
            Assert.True(editor.IsDirty);

            editor.InsertView();
            editor.InsertPanel!.Insert();
            var write = rig.Door.CabeceraWrites.Single();
            Assert.True(write.NewRack);
            Assert.Null(write.Properties!.Fingerprint);
            Assert.Equal("Zona", write.Properties.Entries.Single().Name);

            rig.Door.Written(RackPropertiesRules.AnswerForNewRack(RackDoorRules.AnswerForNewRack(write.RequestId, write.RackId, 2, "Cabecera 2"),
                write.Properties, null, Read((IdA, "Zona", "A"))));
            Assert.False(editor.IsNewRack);
            Assert.False(tab.IsNewRack);
            Assert.False(editor.IsDirty);
            Assert.Equal(IdA, tab.Properties.Single().Id);
        }

        // ------------------------------------------------------------------ RACKCABECERA, RACKPROPIEDADES, «Actualizar todo», D-08

        [Fact]
        public void RackCabeceraOpensCabeceraEnEstaVentanaAndSaysItInTheCommandLine()
        {
            var rig = new Rig();
            rig.Window.ShowPage(RackCadWindowPage.Rack);
            var ask = new RackCadShortcutAsk(RackCadShortcutTarget.NewCabecera, null!, null!);
            rig.Door.Ask(ask);

            Assert.True(ask.Taken);
            Assert.Equal(RackCadShortcutRules.NewCabeceraText("Borrador 1"), ask.Said);
            Assert.True(rig.Window.IsNuevoPage);
            Assert.True(rig.Window.ShowNewCabeceraEditor);
            Assert.True(rig.Window.NewCabeceraEditor!.IsNewRack);
            Assert.Empty(rig.Door.CabeceraReads); // nothing to read: no order leaves the window
            Assert.Empty(rig.Door.CabeceraWrites);

            // Each RACKCABECERA is a click on «Cabecera en esta ventana»: another draft, up to the limit of the drafts.
            var second = new RackCadShortcutAsk(RackCadShortcutTarget.NewCabecera, null!, null!);
            rig.Door.Ask(second);
            Assert.Equal(RackCadShortcutRules.NewCabeceraText("Borrador 2"), second.Said);
            Assert.Equal(2, rig.Window.Drafts.Count);

            var none = new Rig(withDoor: false);
            Assert.False(none.Window.StartNewCabecera().Opened);
        }

        [Fact]
        public void RackPropiedadesOpensTheTabPropiedadesOfACabeceraReadInsideTheCommand()
        {
            var rig = new Rig();
            var ask = new RackCadShortcutAsk(RackCadShortcutTarget.RackProperties, "CAB-1", null!);
            rig.Door.Ask(ask);
            Assert.True(ask.Taken);
            var read = Assert.Single(rig.Door.CabeceraReads); // «Editar aquí», whose read the command runs inside itself
            Assert.Null(ask.Said);

            rig.Door.AnswerLastRead(Read((IdA, "Zona", "A")));
            rig.Door.Done(ask);
            var editor = rig.Window.ActiveCabeceraEditor!;
            Assert.Equal(CabeceraEditorViewModel.PropertiesTabIndex, editor.SelectedTab);
            Assert.Equal("CAB-1", read.RackId);

            // With its editor open, at once and with no read.
            editor.SelectedTab = 1;
            var again = new RackCadShortcutAsk(RackCadShortcutTarget.RackProperties, "CAB-1", null!);
            rig.Door.Ask(again);
            Assert.Equal(CabeceraEditorViewModel.PropertiesTabIndex, editor.SelectedTab);
            Assert.Single(rig.Door.CabeceraReads);
        }

        [Fact]
        public void ActualizarTodoAndTheVetoOfTheCloseCountTheCabecerasAsTheOtherDrafts()
        {
            var rig = new Rig();
            var existing = rig.OpenExisting();
            var draft = rig.StartNew();
            Change(draft);
            Assert.Equal(0, rig.Window.UpdateAllEligibility.Count); // a new one needs its point: it does not go…
            Assert.Empty(rig.Window.UpdateAllEligibility.Blockers); // …and it does not prevent it (its «Actualizar» is not offered)

            rig.Window.OpenRack("CAB-1");
            Change(existing);
            Assert.Equal(1, rig.Window.UpdateAllEligibility.Count);
            Assert.Empty(rig.Window.UpdateAllEligibility.Blockers);
            var names = rig.Window.UnwrittenWorkOf(Session)!;
            Assert.Contains("Cabecera 1", names, StringComparison.Ordinal);
            Assert.Contains("Borrador 1 (Cabecera nueva)", names, StringComparison.Ordinal);

            // A closed drawing forgets both and says what was lost.
            rig.Summary.Dead.Add(Session);
            rig.Host.Snapshot = WorkspacePanelFixtures.Ready(session: 8);
            rig.Window.Refresh();
            Assert.Empty(rig.Window.CabeceraEditors);
            Assert.Contains("Borrador 1 (Cabecera nueva)", rig.Window.ActionMessage, StringComparison.Ordinal);
        }

        // Review of 258c16b1, M-1 and M-5: «Actualizar todo» writes the cabecera with its tab «Propiedades», as its «Actualizar»; and the
        // panel «Insertar vista…» that was open goes away for good once the request is handed over, also after «Dejar de esperar».
        [Fact]
        public void ActualizarTodoWritesTheTabOfTheCabeceraAndItsPanelDoesNotComeBackAfterDejarDeEsperar()
        {
            var rig = new Rig();
            var editor = rig.OpenExisting(Read((IdA, "Zona", "A")));
            Change(editor);
            editor.PropertiesTab.Properties[0].Value = "B";
            editor.PropertiesTab.CommitProperties();
            editor.InsertView();
            Assert.NotNull(editor.InsertPanel);

            rig.Window.RequestUpdateAll();
            var write = Assert.Single(rig.Door.UpdateAlls);
            var item = Assert.Single(write.Items);
            Assert.Equal(UpdateAllSystem.Cabecera, item.System);
            Assert.Equal(("CAB-1", false, false), (item.Cabecera!.RackId, item.Cabecera.NewRack, item.Cabecera.Inserts));
            Assert.Equal("fp-cab", item.Cabecera.Properties!.Fingerprint);
            Assert.Equal((IdA, "B"), (item.Cabecera.Properties.Entries.Single().Id, item.Cabecera.Properties.Entries.Single().Value));
            Assert.Equal(BracingPattern.XBracing, item.Cabecera.Cabecera.BracingPanels[0].Arrangement);
            Assert.True(editor.IsWriting);
            Assert.Null(editor.InsertPanel);

            rig.Window.StopWaiting();
            Assert.False(editor.IsWriting);
            Assert.True(editor.IsDirty);
            Assert.Null(editor.InsertPanel); // handed over: the panel hidden while writing does not come back
        }

        // Review of 258c16b1, M-5: the two writes of the door that are only for a cabecera of the active drawing say so in another drawing
        // and send nothing (the window and the panel say why first).
        [Fact]
        public void ANewCabeceraAndInsertarVistaOfAnotherDrawingAreNotWrittenInTheActiveOne()
        {
            var rig = new Rig();
            var editor = rig.OpenExisting();
            var draft = rig.StartNew();

            rig.Host.Snapshot = WorkspacePanelFixtures.Ready(session: 8);
            rig.Window.Refresh();
            Assert.Empty(rig.Window.Drafts); // the drafts of «Nuevo» follow the drawing shown: the one of the drawing 7 waits for it
            Assert.Equal(RackCadWindowViewModel.OtherDrawingWriteText, editor.InsertViewBlockedText);
            Assert.Equal(RackCadWindowViewModel.OtherDrawingWriteText, draft.InsertViewBlockedText);

            // The panels open and say why next to «Insertar N vistas»; nothing is sent (RACKCABECERAINSERTAR, RACKCABECERANUEVO).
            var existingPanel = editor.InsertWith(Planta)!;
            var newPanel = draft.InsertWith(Lateral)!;
            Assert.Equal(RackCadWindowViewModel.OtherDrawingWriteText, existingPanel.Reason);
            Assert.Equal(RackCadWindowViewModel.OtherDrawingWriteText, newPanel.Reason);
            Assert.False(existingPanel.CanInsert);
            Assert.False(newPanel.CanInsert);
            Assert.Empty(rig.Door.CabeceraWrites);
            Assert.False(editor.IsWriting);
            Assert.False(draft.IsWriting);

            // Back in its drawing, the draft is there and inserts.
            rig.Host.Snapshot = WorkspacePanelFixtures.Ready(session: 7);
            rig.Window.Refresh();
            Assert.Single(rig.Window.Drafts);
            Assert.Equal(string.Empty, newPanel.Reason);
            Assert.True(newPanel.Insert());
            Assert.True(Assert.Single(rig.Door.CabeceraWrites).NewRack);
        }

        // Review of 258c16b1, M-5: the request of a new cabecera that the dispatch of «Nuevo» draws (CabeceraDoorInsertion), by its behaviour.
        [Fact]
        public void TheRequestOfANewCabeceraCarriesTheCabeceraTheIdentityAndEveryViewInItsOrder()
        {
            var cabecera = new RackFrameConfigurationFactory(UiSupport.LoadCatalogSafe()).Build(RackFrameTemplateCatalog.FindStandardOrDefault(), PostId, 132.0, 42.0);
            var request = Assert.IsType<RackCad.UI.Editor.HeaderInsertionRequest>(CabeceraDoorInsertion.NewRackRequest(IdA, cabecera, new[] { Planta, Lateral }));
            Assert.Same(cabecera, request.Configuration);
            Assert.Equal(IdA, request.RackId);
            Assert.Null(request.SourceProject); // a new cabecera: no project of the library (T7cab-S9)
            Assert.Equal(RackCad.Domain.Systems.Shared.RackSystemKind.Selective, request.Kind);
            Assert.Equal(Planta, request.InitialAddress);
            Assert.Equal(new[] { Planta, Lateral }, request.Views.ToArray());

            Assert.Null(CabeceraDoorInsertion.NewRackRequest(IdA, cabecera, Array.Empty<RackViewAddress>()));
            Assert.Null(CabeceraDoorInsertion.NewRackRequest(IdA, cabecera, null!));
            Assert.Null(CabeceraDoorInsertion.NewRackRequest(IdA, null!, new[] { Lateral }));
        }

        // ------------------------------------------------------------------ the view (WPF, STA)

        private sealed class BindingErrors : TraceListener
        {
            public List<string> Messages { get; } = new();

            public override void Write(string? message)
            {
            }

            public override void WriteLine(string? message)
            {
                if (message != null) Messages.Add(message);
            }
        }

        private static void Lay(FrameworkElement view, double width = 1300)
        {
            view.Measure(new Size(width, 900));
            view.Arrange(new Rect(0, 0, width, 900));
            view.UpdateLayout();
        }

        private static IEnumerable<T> Descendants<T>(DependencyObject root) where T : DependencyObject
        {
            var count = System.Windows.Media.VisualTreeHelper.GetChildrenCount(root);
            for (var i = 0; i < count; i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(root, i);
                if (child is T match) yield return match;
                foreach (var nested in Descendants<T>(child)) yield return nested;
            }
        }

        [Fact]
        public void NuevoOffersCabeceraEnEstaVentanaAndItsEditorMovesToThePageOfTheRackOnceInserted()
        {
            StaTestRunner.Run(() =>
            {
                var errors = new BindingErrors();
                PresentationTraceSources.Refresh();
                var source = PresentationTraceSources.DataBindingSource;
                var level = source.Switch.Level;
                source.Listeners.Add(errors);
                source.Switch.Level = SourceLevels.Error;
                try
                {
                    var rig = new Rig();
                    var view = new RackCadWindowView(rig.Window);
                    rig.Window.ShowPage(RackCadWindowPage.Nuevo);
                    Lay(view);

                    // «Cabecera en esta ventana», after «Cantilever en esta ventana», with its own name; the card «Cabecera» stays (the classic).
                    var nuevo = (Panel)view.NuevoPage.Content;
                    Assert.Equal(nuevo.Children.IndexOf(view.NewCantileverHereButton) + 1, nuevo.Children.IndexOf(view.NewCabeceraHereButton));
                    Assert.Equal("Nueva cabecera en esta ventana", AutomationProperties.GetName(view.NewCabeceraHereButton));
                    Assert.True(view.NewCabeceraHereButton.IsEnabled);
                    Assert.Contains(rig.Window.NewSystems, c => c.Action == RackCadClassicAction.NewHeader);
                    var names = Descendants<Button>(view).Select(AutomationProperties.GetName).Where(n => !string.IsNullOrEmpty(n)).ToList();
                    Assert.Equal(names.Count, names.Distinct().Count());

                    view.NewCabeceraHereButton.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
                    Lay(view);
                    var editor = rig.Window.NewCabeceraEditor!;
                    var hosted = Assert.IsType<CabeceraEditorView>(view.NewCabeceraRackHost.Content);
                    Assert.Same(editor, hosted.ViewModel);
                    Assert.Equal(Visibility.Visible, view.NewCabeceraRackHost.Visibility);
                    Assert.Equal("Insertar en el dibujo…", hosted.InsertViewButton.Content);
                    Assert.Equal(CabeceraEditorViewModel.NewRackInsertToolTip, hosted.InsertViewButton.ToolTip); // M-4: no views to write yet
                    Assert.True(hosted.InsertViewButton.IsEnabled);
                    Assert.False(hosted.UpdateButton.IsEnabled);
                    Assert.Equal(Visibility.Visible, hosted.RackPropertiesTabItem.Visibility);
                    Assert.Equal("Propiedades", hosted.RackPropertiesTabItem.Header);

                    hosted.InsertViewButton.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
                    Lay(view);
                    Assert.Equal(Visibility.Visible, hosted.InsertPanelScroll.Visibility);
                    Assert.Equal("panel abierto", AutomationProperties.GetItemStatus(hosted.InsertViewButton));
                    var editorNames = Descendants<Button>(hosted).Select(AutomationProperties.GetName).Where(n => !string.IsNullOrEmpty(n)).ToList();
                    Assert.Equal(editorNames.Count, editorNames.Distinct().Count());

                    editor.InsertPanel!.Insert();
                    var write = rig.Door.CabeceraWrites.Single();
                    rig.Door.Written(RackDoorRules.AnswerForNewRack(write.RequestId, write.RackId, 2, "Cabecera 2"));
                    rig.Listed.Add(new BrowserRackListEntry(write.RackId, "Cabecera 2", RackEmbedDocument.KindCabecera, new[] { "lateral", "planta" }));
                    rig.Window.RackList.Reload();
                    rig.Window.OpenRack(write.RackId);
                    Lay(view);

                    Assert.Null(view.NewCabeceraRackHost.Content); // a view has one parent
                    Assert.Same(hosted, view.RackPageHost.Content);
                    Assert.Equal("Insertar vista…", hosted.InsertViewButton.Content);
                    Assert.Equal(CabeceraEditorViewModel.InsertViewsToolTip, hosted.InsertViewButton.ToolTip);
                    Assert.True(hosted.UpdateButton.Visibility == Visibility.Visible);
                    Assert.Empty(errors.Messages);
                }
                finally
                {
                    source.Listeners.Remove(errors);
                    source.Switch.Level = level;
                }
            });
        }
    }
}
