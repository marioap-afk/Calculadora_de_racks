#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Shared;
using RackCad.Application.VentanaRackCad;
using RackCad.Application.Workspace;
using RackCad.Domain.Systems.Shared;
using RackCad.UI.VentanaRackCad;
using RackCad.UI.Workspace;
using Xunit;
using static RackCad.UI.Tests.SelectiveWindowFixtures;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64 A-14, line D, cuarta tanda, part «new» (SD-06 of the Owner: «En lugar de nuevo, podríamos manejar borradores, tipo, borrador 1,
    /// borrador 2 etc, para poder tener varios nuevos, mejora la propuesta»): «Nuevo» holds several drafts of new racks of the drawing at
    /// once, of any system with an editor in the window; each one opens, is discarded or is inserted on its own; inserted, it takes the
    /// automatic name of I-60; a draft of another drawing is not written; the close advisor names all of them; and the one entry point
    /// <see cref="RackCadWindowViewModel.OpenAsNewDraft"/> opens a design (of the library, a copy) as a new draft. Over the double door, as
    /// <c>RackCadSelectiveInsertTests</c>, and the views with real WPF.
    /// </summary>
    public class RackCadNewDraftsTests
    {
        private static readonly SessionId Session = new(7);

        private static readonly RackViewAddress Planta = RackViewAddress.Whole(DimensionViewKind.Planta);

        /// <summary>A door that reads and writes a Selectivo and a Dinámico, as the door of the Plugin does.</summary>
        private sealed class DoorDouble : IRackCadSelectiveDoorPort
        {
            public List<RackDoorWrite> Writes { get; } = new();

            public List<SelectiveDoorWrite> SelectiveWrites { get; } = new();

            public event Action<RackDesignRead>? DesignRead { add { } remove { } }

            public event Action<RackDesignWritten>? DesignWritten;

            public event Action<long>? RequestStarted { add { } remove { } }

            public event Action<SelectiveDesignRead>? SelectiveRead { add { } remove { } }

            public RackCadClassicResult RequestDesign(RackDoorRequest request) => RackCadClassicResult.Started();

            public RackCadClassicResult RequestDynamicUpdate(RackDoorWrite write)
            {
                Writes.Add(write);
                return RackCadClassicResult.Started();
            }

            public RackCadClassicResult RequestSelectiveDesign(RackDoorRequest request) => RackCadClassicResult.Started();

            public RackCadClassicResult RequestSelectiveUpdate(SelectiveDoorWrite write)
            {
                SelectiveWrites.Add(write);
                return RackCadClassicResult.Started();
            }

            public void Abandon(long requestId)
            {
            }

            public void Written(RackDesignWritten written) => DesignWritten?.Invoke(written);
        }

        /// <summary>A door of Dinámico only: it cannot write a Selectivo.</summary>
        private sealed class DynamicOnlyDoor : IRackCadRackDoorPort
        {
            public RackCadClassicResult RequestDesign(RackDoorRequest request) => RackCadClassicResult.Started();

            public RackCadClassicResult RequestDynamicUpdate(RackDoorWrite write) => RackCadClassicResult.Started();

            public void Abandon(long requestId)
            {
            }

            public event Action<RackDesignRead>? DesignRead { add { } remove { } }

            public event Action<RackDesignWritten>? DesignWritten { add { } remove { } }

            public event Action<long>? RequestStarted { add { } remove { } }
        }

        private sealed class Rig
        {
            public Rig(IRackCadRackDoorPort? door = null, bool withDoor = true)
            {
                Host = new WorkspacePortDouble { Snapshot = WorkspacePanelFixtures.Ready(session: 7) };
                Summary = new WorkspaceBrowserSummaryPortDouble { ListResponder = (_, _) => BrowserRackList.Ready(Listed) };
                Panel = new WorkspacePanelViewModel(Host, _ => { }, new WorkspaceNavigationPortDouble(), Summary, new WorkspacePilotEditPortDouble());
                Window = new RackCadWindowViewModel(Panel, null, null, withDoor ? door ?? Door : null, UiSupport.LoadCatalogSafe);
                Window.Refresh();
            }

            public List<BrowserRackListEntry> Listed { get; } = new()
            {
                new BrowserRackListEntry("SEL-1", "Selectivo A", "selective", new[] { "frontal" }),
            };

            public WorkspacePortDouble Host { get; }

            public WorkspaceBrowserSummaryPortDouble Summary { get; }

            public WorkspacePanelViewModel Panel { get; }

            public DoorDouble Door { get; } = new();

            public RackCadWindowViewModel Window { get; }

            public void Present(long session)
            {
                Host.Snapshot = WorkspacePanelFixtures.Ready(session: session);
                Window.Refresh();
            }

            public SelectiveEditorViewModel NewSelective()
            {
                Window.StartNewSelective();
                Assert.True(Window.NewSelectiveEditor != null, Window.ActionMessage);
                return Window.NewSelectiveEditor!;
            }

            public DynamicEditorViewModel NewDynamic()
            {
                Window.StartNewDynamic();
                Assert.True(Window.NewDynamicEditor != null, Window.ActionMessage);
                return Window.NewDynamicEditor!;
            }
        }

        private static void Edit(SelectiveEditorViewModel editor, string text = "72") => Write(Field(editor.CellFields, "cHeight"), text);

        private static void Edit(DynamicEditorViewModel editor, string weight = "1200")
            => Write(editor.Essentials.Single(f => f.Key == "weight"), weight);

        private static string DynamicJson(RackCad.Domain.Systems.Dynamic.DynamicRackDesign? design)
            => new RackProjectStore().Serialize(RackProject.ForDynamic(design!));

        private static RackCad.Application.Catalogs.RackCatalog Catalog() => UiSupport.LoadCatalogSafe();

        // ------------------------------------------------------------------ the rules of the names

        [Fact]
        public void TheNumberOfANewDraftIsTheLowestOneThatNoDraftOfItsDrawingUses()
        {
            Assert.Equal(1, RackCadDraftNaming.NextNumber(Array.Empty<int>()));
            Assert.Equal(1, RackCadDraftNaming.NextNumber(null!));
            Assert.Equal(3, RackCadDraftNaming.NextNumber(new[] { 1, 2 }));
            Assert.Equal(2, RackCadDraftNaming.NextNumber(new[] { 3, 1 }));
            Assert.Equal(1, RackCadDraftNaming.NextNumber(new[] { 2 }));

            Assert.Equal("Borrador 4", RackCadDraftNaming.Label(4));
            Assert.Equal("Borrador 4", RackCadDraftNaming.Title(4, "  "));
            Assert.Equal("Nave A", RackCadDraftNaming.Title(4, " Nave A "));
            Assert.Equal("Borrador 2 (Selectivo nuevo)", RackCadDraftNaming.NoticeName(2, "Selectivo", null));
            Assert.Equal("Nave A (Borrador 2, Dinámico nuevo)", RackCadDraftNaming.NoticeName(2, "Dinámico", "Nave A ")); // point 6 of the review

            Assert.Equal("Selectivo", RackCadDraftNaming.SystemLabel(RackSystemKind.SelectiveRack));
            Assert.Equal("Dinámico", RackCadDraftNaming.SystemLabel(RackSystemKind.PalletFlow));
            Assert.Equal("Cabecera", RackCadDraftNaming.SystemLabel(RackSystemKind.Selective)); // the historical name of the cabecera
            Assert.Equal("Push Back", RackCadDraftNaming.SystemLabel(RackSystemKind.PushBack));
            Assert.Equal("Ya hay 8 borradores sin insertar en este dibujo: inserta o descarta alguno antes de empezar otro.", RackCadDraftNaming.LimitText);
        }

        // ------------------------------------------------------------------ several drafts

        [Fact]
        public void EveryNewOfThisWindowIsADraftOfItsOwnNumberedInItsDrawing()
        {
            var rig = new Rig();
            var first = rig.NewSelective();
            var second = rig.NewSelective();
            var third = rig.NewDynamic();

            Assert.NotSame(first, second);
            Assert.True(first.IsNewRack && second.IsNewRack && third.IsNewRack);
            Assert.Equal(new[] { "Borrador 1", "Borrador 2", "Borrador 3" }, rig.Window.Drafts.Select(d => d.Label).ToArray());
            Assert.Equal(new[] { "Selectivo", "Selectivo", "Dinámico" }, rig.Window.Drafts.Select(d => d.SystemLabel).ToArray());
            Assert.Equal(new[] { first.RackId, second.RackId, third.RackId }, rig.Window.Drafts.Select(d => d.RackId).ToArray());
            Assert.Equal(3, rig.Window.Drafts.Select(d => d.RackId).Distinct().Count());
            Assert.All(rig.Window.Drafts, d => Assert.True(Guid.TryParse(d.RackId, out _)));

            // The last one is open: «Nuevo» shows its editor, and only its item is active.
            Assert.True(rig.Window.IsNuevoPage);
            Assert.Same(third, rig.Window.NewDynamicEditor);
            Assert.Null(rig.Window.NewSelectiveEditor);
            Assert.Same(rig.Window.Drafts[2], rig.Window.ActiveDraft);
            Assert.Equal(new[] { false, false, true }, rig.Window.Drafts.Select(d => d.IsActive).ToArray());
            Assert.False(rig.Window.ShowNuevoCards);
            Assert.True(rig.Window.ShowActiveDraftStrip);
            Assert.False(rig.Window.ShowDraftList);
            Assert.False(rig.Window.ActionFailed);

            // They are editors of the window like the new racks of P3 and of the step 4 of Dinámico.
            Assert.Equal(2, rig.Window.SelectiveEditors.Count);
            Assert.Single(rig.Window.DynamicEditors);
            Assert.Null(rig.Window.ActiveSelectiveEditor);
            Assert.Null(rig.Window.ActiveDynamicEditor);

            // The header counts them (the smoke of the entrega 2 saw «todo escrito» with a new rack «Modificado»).
            Assert.EndsWith(" · 3 borradores sin insertar", rig.Window.HeaderText);
        }

        [Fact]
        public void NuevoGoesToItsHomeAndADraftOpensFromItsItem()
        {
            var rig = new Rig();
            var first = rig.NewSelective();
            var second = rig.NewSelective();
            var raised = new List<string?>();
            rig.Window.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

            // «Nuevo» in the side column: its home, with the systems and the list of the drafts.
            rig.Window.ShowPage(RackCadWindowPage.Nuevo);
            Assert.Null(rig.Window.ActiveDraft);
            Assert.Null(rig.Window.NewSelectiveEditor);
            Assert.True(rig.Window.ShowNuevoCards);
            Assert.True(rig.Window.ShowDraftsBar);
            Assert.True(rig.Window.ShowDraftList);
            Assert.False(rig.Window.ShowActiveDraftStrip);
            Assert.Contains(nameof(RackCadWindowViewModel.NewSelectiveEditor), raised);
            Assert.Equal(2, rig.Window.Drafts.Count);

            rig.Window.OpenDraft(rig.Window.Drafts[0]);
            Assert.Same(first, rig.Window.NewSelectiveEditor);
            Assert.True(rig.Window.Drafts[0].IsActive);
            Assert.False(rig.Window.Drafts[1].IsActive);
            Assert.True(rig.Window.ShowNewSelectiveEditor);

            // Another page keeps the drafts; the item of a draft brings it back.
            rig.Window.ShowPage(RackCadWindowPage.Variables);
            Assert.All(rig.Window.Drafts, d => Assert.False(d.IsActive)); // the column marks a draft only while «Nuevo» is shown (point 7)
            Assert.False(rig.Window.ShowNewSelectiveEditor);
            Assert.False(rig.Window.ShowDraftsBar);
            Assert.Equal(2, rig.Window.Drafts.Count);
            rig.Window.OpenDraft(rig.Window.Drafts[1]);
            Assert.True(rig.Window.Drafts[1].IsActive);
            Assert.True(rig.Window.IsNuevoPage);
            Assert.Same(second, rig.Window.NewSelectiveEditor);
        }

        [Fact]
        public void TheItemOfADraftFollowsItsNameAndItsState()
        {
            var rig = new Rig();
            var editor = rig.NewSelective();
            var item = Assert.Single(rig.Window.Drafts);
            var raised = new List<string?>();
            item.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

            Assert.Equal("Borrador 1", item.Title);
            Assert.Equal("Borrador 1 · Selectivo", item.ColumnText);
            Assert.Equal("Selectivo · Sin insertar", item.DetailText);
            Assert.Equal((RackCadDraftItem.NewKind, "Sin insertar"), (item.StatusKind, item.Tag));
            Assert.Equal("Borrador 1, Selectivo, Sin insertar", item.AccessibleName);

            Edit(editor);
            Assert.Equal((RackCadDraftItem.ModifiedKind, "Modificado", "Sin insertar, con cambios"), (item.StatusKind, item.Tag, item.StatusText));
            Assert.Contains(nameof(RackCadDraftItem.StatusKind), raised);

            editor.Name = "Nave A";
            Assert.Equal("Nave A", item.Title);
            Assert.Equal("Nave A · Selectivo", item.ColumnText);
            Assert.Equal("Selectivo · Borrador 1 · Sin insertar, con cambios", item.DetailText);
            Assert.Equal("Borrador 1 (Nave A), Selectivo, Sin insertar, con cambios", item.AccessibleName);
            Assert.Contains(nameof(RackCadDraftItem.Title), raised);

            editor.InsertWith(Planta); // T4 ins: the panel of «Insertar vista…»
            Assert.True(editor.IsWriting);
            Assert.Equal((RackCadDraftItem.WritingKind, "Escribiendo", "Escribiendo en el dibujo"), (item.StatusKind, item.Tag, item.StatusText));
            Assert.False(item.CanDiscard);
            Assert.False(item.CanDuplicate);

            // The same item lives while the draft lives (D-06): the list is not rebuilt for a change of state.
            Assert.Same(item, Assert.Single(rig.Window.Drafts));
        }

        [Fact]
        public void ADraftIsDiscardedOnItsOwnAndOneWithChangesAsksFirst()
        {
            var rig = new Rig();
            var first = rig.NewSelective();
            var second = rig.NewDynamic();

            // Clean: discarded at once, and said.
            rig.Window.DiscardDraft(rig.Window.Drafts[0]);
            Assert.Equal(new[] { "Borrador 2" }, rig.Window.Drafts.Select(d => d.Label).ToArray());
            Assert.DoesNotContain(first, rig.Window.SelectiveEditors);
            Assert.Equal("Se descartó Borrador 1 (Selectivo nuevo): no estaba en el dibujo.", rig.Window.ActionMessage);
            Assert.False(rig.Window.ActionFailed);
            Assert.Same(second, rig.Window.NewDynamicEditor); // the draft that was open stays open

            // With changes: it asks first, inline; «Cancelar» keeps it.
            Edit(second);
            var item = rig.Window.Drafts[0];
            rig.Window.DiscardDraft(item);
            Assert.True(item.IsConfirmingDiscard);
            Assert.Equal("¿Descartar Borrador 2? Tiene cambios y no está en el dibujo: se pierden.", item.DiscardQuestion);
            Assert.Contains(second, rig.Window.DynamicEditors);
            rig.Window.CancelDiscardDraft(item);
            Assert.False(item.IsConfirmingDiscard);
            rig.Window.ConfirmDiscardDraft(item); // with no question pending it does nothing
            Assert.Contains(second, rig.Window.DynamicEditors);

            // The number of a discarded draft is free again: the lowest one.
            var again = rig.NewSelective();
            Assert.Equal(new[] { "Borrador 1", "Borrador 2" }, rig.Window.Drafts.Select(d => d.Label).ToArray());
            Assert.Same(again, rig.Window.NewSelectiveEditor);

            rig.Window.DiscardDraft(item);
            rig.Window.ConfirmDiscardDraft(item);
            Assert.DoesNotContain(second, rig.Window.DynamicEditors);
            Assert.Equal(new[] { "Borrador 1" }, rig.Window.Drafts.Select(d => d.Label).ToArray());
            Assert.Null(rig.Window.UnwrittenWorkOf(Session));

            // Discarding the draft that is open goes to the home of «Nuevo».
            rig.Window.DiscardDraft(rig.Window.Drafts[0]);
            Assert.Empty(rig.Window.Drafts);
            Assert.True(rig.Window.ShowNuevoCards);
            Assert.False(rig.Window.ShowDraftsBar);
            Assert.DoesNotContain("borrador", rig.Window.HeaderText, StringComparison.Ordinal);
        }

        [Fact]
        public void ADraftThatAutoCadWritesIsNotDiscardedNorCopied()
        {
            var rig = new Rig();
            var editor = rig.NewSelective();
            Edit(editor);
            editor.InsertWith(Planta); // T4 ins: the panel of «Insertar vista…»
            var item = rig.Window.Drafts[0];

            rig.Window.DiscardDraft(item);
            Assert.False(item.IsConfirmingDiscard);
            Assert.Contains(editor, rig.Window.SelectiveEditors);
            Assert.Equal("AutoCAD está escribiendo Borrador 1: espera a que termine, o termina o cancela la orden en AutoCAD.", rig.Window.ActionMessage);
            Assert.True(rig.Window.ActionFailed);

            var copy = rig.Window.DuplicateDraft(item);
            Assert.False(copy.Opened);
            Assert.Equal("AutoCAD está escribiendo Borrador 1: espera a que termine para duplicarlo.", copy.Reason);
            Assert.Single(rig.Window.Drafts);
        }

        [Fact]
        public void DuplicarCopiesTheDesignWithNoNameAndItsOwnIdentity()
        {
            var rig = new Rig();
            var source = rig.NewSelective();
            Edit(source);
            source.Name = "Nave A";

            var opening = rig.Window.DuplicateDraft(rig.Window.Drafts[0]);
            Assert.True(opening.Opened, opening.Reason);
            var copy = rig.Window.NewSelectiveEditor!;
            Assert.NotSame(source, copy);
            Assert.Same(opening.Draft, rig.Window.ActiveDraft);
            Assert.Equal("Borrador 2", opening.Draft!.Label);
            Assert.NotEqual(source.RackId, copy.RackId);
            Assert.True(copy.IsNewRack);
            Assert.Equal(Json(source.Draft.Design), Json(copy.Draft.Design));
            Assert.True(string.IsNullOrEmpty(copy.Name)); // two racks would get the same name
            Assert.True(copy.IsDirty); // point 1 of the review: the copy keeps the base of its original, so it is «Modificado» as it was
            Assert.Equal("Borrador 2: Selectivo nuevo en esta ventana, copia de Nave A. Todavía no está en el dibujo.", copy.Message);

            // The copy does not reach the source.
            Edit(copy, "80");
            Assert.NotEqual(Json(source.Draft.Design), Json(copy.Draft.Design));
            Assert.True(source.IsDirty);
            copy.Discard(); // back to the base of its original: the defaults of a new Selectivo
            Assert.Equal(Json(SelectiveDraftEditor.CreateNew(Catalog()).Design), Json(copy.Draft.Design));
            Assert.False(copy.IsDirty);

            // A Dinámico too.
            var dynamic = rig.NewDynamic();
            Edit(dynamic);
            var dynamicCopy = rig.Window.DuplicateDraft(rig.Window.ActiveDraft!);
            Assert.True(dynamicCopy.Opened, dynamicCopy.Reason);
            var dynamicEditor = rig.Window.NewDynamicEditor!;
            Assert.NotSame(dynamic, dynamicEditor);
            Assert.Equal(DynamicJson(dynamic.Draft.Design), DynamicJson(dynamicEditor.Draft.Design));
            Assert.Equal(new[] { "Borrador 1", "Borrador 2", "Borrador 3", "Borrador 4" }, rig.Window.Drafts.Select(d => d.Label).ToArray());
        }

        // ------------------------------------------------------------------ insertion, drawings and the close

        [Fact]
        public void InsertingOneDraftLeavesTheOthersAndTakesTheAutomaticName()
        {
            var rig = new Rig();
            var first = rig.NewSelective();
            Edit(first);
            var second = rig.NewSelective();
            Edit(second, "80");

            second.InsertWith(Planta); // T4 ins: the panel of «Insertar vista…»
            var write = Assert.Single(rig.Door.SelectiveWrites);
            Assert.True(write.NewRack);
            Assert.Equal(second.RackId, write.RackId);
            Assert.True(string.IsNullOrEmpty(write.Name), write.Name); // «Borrador 2» is the label of the window, never the name of the rack
            Assert.True(SelectiveDoorInsertion.PlanNewRack(write, identityInDrawing: false, Catalog()).CanInsert);

            rig.Door.Written(SelectiveDoorInsertion.AnswerForNewRack(write.RequestId, write.RackId, 1, "Selectivo 1"));
            Assert.Equal("Selectivo 1", second.Name);
            Assert.False(second.IsNewRack);
            Assert.Equal(new[] { "Borrador 1" }, rig.Window.Drafts.Select(d => d.Label).ToArray());
            Assert.Null(rig.Window.ActiveDraft);
            Assert.True(rig.Window.ShowNuevoCards);
            Assert.True(rig.Window.ShowDraftList);
            Assert.Contains(second, rig.Window.SelectiveEditors); // the editor of the rack of the drawing now (D-10)
            Assert.Equal("Borrador 1 (Selectivo nuevo)", rig.Window.UnwrittenWorkOf(Session));
            Assert.EndsWith(" · 1 borrador sin insertar", rig.Window.HeaderText);

            // The other draft is inserted on its own, with its own automatic name.
            rig.Window.OpenDraft(rig.Window.Drafts[0]);
            first.InsertWith(Planta); // T4 ins: the panel of «Insertar vista…»
            var other = rig.Door.SelectiveWrites.Last();
            Assert.Equal(first.RackId, other.RackId);
            rig.Door.Written(SelectiveDoorInsertion.AnswerForNewRack(other.RequestId, other.RackId, 1, "Selectivo 2"));
            Assert.Equal("Selectivo 2", first.Name);
            Assert.Empty(rig.Window.Drafts);
        }

        [Fact]
        public void TheDraftsBelongToTheirDrawingAndADraftOfAnotherOneIsNotWritten()
        {
            var rig = new Rig();
            var first = rig.NewSelective();
            var second = rig.NewDynamic();
            rig.Window.OpenDraft(rig.Window.Drafts[0]);

            rig.Present(8);
            Assert.Empty(rig.Window.Drafts);
            Assert.Null(rig.Window.NewSelectiveEditor);
            Assert.Null(rig.Window.NewDynamicEditor);
            Assert.True(rig.Window.ShowNuevoCards);
            var elsewhere = rig.NewSelective();
            Assert.Equal("Borrador 1", Assert.Single(rig.Window.Drafts).Label); // numbered in its own drawing
            var itemOf8 = rig.Window.Drafts[0];

            // «Nuevo sigue al dibujo»: a draft of the other drawing waits for it.
            first.InsertWith(Planta); // T4 ins: the panel of «Insertar vista…»
            Assert.Empty(rig.Door.SelectiveWrites);
            Assert.Equal("Este borrador es de otro dibujo: vuelve a ese dibujo para escribirlo.", first.InsertViewBlockedText); // T4 ins (correction 7): the reason is next to the button

            // Back in the first drawing: its drafts, and the one it had open.
            rig.Present(7);
            Assert.Equal(new[] { first.RackId, second.RackId }, rig.Window.Drafts.Select(d => d.RackId).ToArray());
            Assert.Same(first, rig.Window.NewSelectiveEditor);
            rig.Window.OpenDraft(itemOf8);
            Assert.Equal("Ese borrador es de otro dibujo: vuelve a ese dibujo para abrirlo.", rig.Window.ActionMessage); // point 5 of the review
            Assert.Equal("Ese borrador es de otro dibujo: vuelve a ese dibujo para duplicarlo.", rig.Window.DuplicateDraft(itemOf8).Reason);
            rig.Window.DiscardDraft(itemOf8);
            Assert.Equal("Ese borrador es de otro dibujo: vuelve a ese dibujo para descartarlo.", rig.Window.ActionMessage);
            Assert.Contains(elsewhere, rig.Window.SelectiveEditors);
            Assert.Same(first, rig.Window.NewSelectiveEditor);
            Assert.Contains(elsewhere, rig.Window.SelectiveEditors);
        }

        [Fact]
        public void TheCloseOfTheDrawingNamesEveryDraftWithWorkAndThePruneSaysWhatWasLost()
        {
            var rig = new Rig();
            Edit(rig.NewSelective());
            Edit(rig.NewDynamic());
            var named = rig.NewSelective();
            named.Name = "Nave A";
            rig.NewSelective(); // «Borrador 4», with nothing changed: nothing to lose, it does not veto

            Assert.Equal("Borrador 1 (Selectivo nuevo), Borrador 2 (Dinámico nuevo), Nave A (Borrador 3, Selectivo nuevo)", rig.Window.UnwrittenWorkOf(Session));
            Assert.Equal("Borrador 2 (Dinámico nuevo)", rig.Window.UnwrittenDynamicWorkOf(Session));

            var lines = new List<string>();
            var advisor = new RackCadWindowCloseAdvisor(new AllowAll(), lines.Add, null!);
            advisor.SetWindowWork(rig.Window.UnwrittenWorkOf);
            Assert.Equal(CloseDecision.Veto, advisor.AdviseClose(Session));
            Assert.Equal(
                "RackCad: el dibujo no se cerró: la ventana RackCad tiene cambios sin escribir en Borrador 1 (Selectivo nuevo), Borrador 2 (Dinámico nuevo), Nave A (Borrador 3, Selectivo nuevo). Escríbelos con «Actualizar» en su editor (un rack nuevo de la ventana, con «Insertar en el dibujo…») o descártalos, y vuelve a cerrar.",
                Assert.Single(lines));

            rig.Summary.Dead.Add(Session);
            rig.Window.Refresh();
            Assert.Empty(rig.Window.Drafts);
            Assert.Empty(rig.Window.SelectiveEditors);
            Assert.Empty(rig.Window.DynamicEditors);
            Assert.StartsWith("Se cerró el dibujo de ", rig.Window.ActionMessage);
            foreach (var name in new[] { "Borrador 1 (Selectivo nuevo)", "Borrador 2 (Dinámico nuevo)", "Nave A (Borrador 3, Selectivo nuevo)" })
                Assert.Contains(name, rig.Window.ActionMessage, StringComparison.Ordinal);
            Assert.DoesNotContain("Borrador 4", rig.Window.ActionMessage, StringComparison.Ordinal);
        }

        private sealed class AllowAll : IWorkspaceLifecycleAdvisor
        {
            public CloseDecision AdviseClose(SessionId session) => CloseDecision.Allow;

            public void SessionDestroyed(SessionId session, bool closeSequenceIntact)
            {
            }
        }

        [Fact]
        public void ADrawingHoldsUpToEightDrafts()
        {
            var rig = new Rig();
            for (var i = 0; i < RackCadDraftNaming.MaxPerDrawing; i++) rig.NewSelective();
            Assert.Equal(8, rig.Window.Drafts.Count);

            rig.Window.StartNewDynamic();
            Assert.Equal(8, rig.Window.Drafts.Count);
            Assert.Empty(rig.Window.DynamicEditors);
            Assert.Equal(RackCadDraftNaming.LimitText, rig.Window.ActionMessage);
            Assert.True(rig.Window.ActionFailed);
            Assert.True(rig.Window.ShowDraftList); // the list, to insert or discard one

            // Another drawing has its own.
            rig.Present(8);
            rig.NewDynamic();
            rig.Present(7);

            rig.Window.DiscardDraft(rig.Window.Drafts[3]);
            var next = rig.NewDynamic();
            Assert.Equal("Borrador 4", rig.Window.ActiveDraft!.Label);
            Assert.Same(next, rig.Window.NewDynamicEditor);
        }

        // ------------------------------------------------------------------ the one entry point

        [Fact]
        public void OpenAsNewDraftOpensADesignOfTheLibraryAsANewDraftWithItsName()
        {
            var rig = new Rig();
            var document = SelectivePalletDesignDocument.From(TwoFondos(), Guid.NewGuid().ToString(), "Nave lib");
            var project = RackProject.ForSelectiveRack(document);

            var opening = rig.Window.OpenAsNewDraft(RackCadDraftSeed.FromProject(project, "del diseño «Nave lib» de la biblioteca"));
            Assert.True(opening.Opened, opening.Reason);
            var editor = rig.Window.NewSelectiveEditor!;
            Assert.Same(opening.Draft, rig.Window.ActiveDraft);
            Assert.Equal("Borrador 1", opening.Draft!.Label);
            Assert.Equal("Nave lib", opening.Draft.Title);
            Assert.Equal("Nave lib", editor.Name); // as the classic editor opened from the library keeps it
            Assert.True(editor.IsNewRack);
            Assert.NotEqual(document.Id, editor.RackId); // a new rack of the window has its own identity
            Assert.False(editor.IsDirty);
            Assert.Equal("Borrador 1: Selectivo nuevo en esta ventana, del diseño «Nave lib» de la biblioteca. Todavía no está en el dibujo.", editor.Message);

            // The design is the one of the library, recomposed as the classic load does it, and a copy: editing does not reach the project.
            var expected = new SelectiveDraftEditor(Catalog());
            Assert.True(expected.LoadFrom(TwoFondos(), null, "Nave lib"), expected.LastError);
            Assert.Equal(Json(expected.Design), Json(editor.Draft.Design));
            var before = Store.Serialize(document);
            Edit(editor);
            Assert.Equal(before, Store.Serialize(document));

            // Inserted, it keeps the name of the design.
            editor.InsertWith(RackViewAddress.Fondo(0)); // T4 ins: the panel of «Insertar vista…»
            var write = Assert.Single(rig.Door.SelectiveWrites);
            Assert.Equal("Nave lib", write.Name);
            Assert.True(SelectiveDoorInsertion.PlanNewRack(write, identityInDrawing: false, Catalog()).CanInsert);

            // A name given by the caller replaces the one of the design; none, and no origin, still opens.
            var renamed = rig.Window.OpenAsNewDraft(RackCadDraftSeed.FromProject(project, name: "Nave B"));
            Assert.Equal("Nave B", rig.Window.NewSelectiveEditor!.Name);
            Assert.Equal("Borrador 2: Selectivo nuevo en esta ventana, con el diseño abierto. Todavía no está en el dibujo.", rig.Window.NewSelectiveEditor.Message);
            Assert.Equal("Borrador 2", renamed.Draft!.Label);
        }

        [Fact]
        public void OpenAsNewDraftOpensADinamicoOfTheLibraryAndSaysWhatTheLoadOfASelectivoChanged()
        {
            var rig = new Rig();
            var source = rig.NewDynamic();
            Edit(source, "1300");
            var project = RackProject.ForDynamic(source.Draft.SnapshotForWrite()!.Design);

            var opening = rig.Window.OpenAsNewDraft(RackCadDraftSeed.FromProject(project, "de la biblioteca"));
            Assert.True(opening.Opened, opening.Reason);
            var editor = rig.Window.NewDynamicEditor!;
            Assert.NotSame(source, editor);
            Assert.Equal(DynamicJson(source.Draft.Design), DynamicJson(editor.Draft.Design));
            Assert.False(editor.IsDirty);
            Assert.Equal("Borrador 2: Dinámico nuevo en esta ventana, de la biblioteca. Todavía no está en el dibujo.", editor.Message);
            Edit(editor, "1400");
            Assert.NotEqual(DynamicJson(source.Draft.Design), DynamicJson(editor.Draft.Design));

            // A Selectivo of the library with holguras bound to variables opens with their value, and says so.
            var bound = Store.Deserialize(Authored(Structure(), bound: true));
            Assert.True(bound.HasPropertyValues);
            var linked = rig.Window.OpenAsNewDraft(RackCadDraftSeed.FromProject(RackProject.ForSelectiveRack(bound), "de la biblioteca"));
            Assert.True(linked.Opened, linked.Reason);
            Assert.EndsWith("Las holguras vinculadas a variables se abren con el valor literal guardado en el diseño, no con el de la variable: un rack nuevo de la ventana no lleva vínculos.",
                rig.Window.NewSelectiveEditor!.Message);
        }

        [Fact]
        public void OpenAsNewDraftRefusesWhatTheWindowCannotOpenAndSaysWhy()
        {
            var rig = new Rig();
            void Refused(RackCadDraftOpening opening, string reason)
            {
                Assert.False(opening.Opened);
                Assert.Null(opening.Draft);
                Assert.Equal(reason, opening.Reason);
                Assert.Equal(reason, rig.Window.ActionMessage);
                Assert.True(rig.Window.ActionFailed);
            }

            Refused(rig.Window.OpenAsNewDraft(null!), "No hay un diseño que abrir como borrador.");
            // T5 pb: Push Back has an editor in the window now; without the door of Push Back (this door is the one of Selectivo) it is refused for the door.
            Refused(rig.Window.OpenAsNewDraft(RackCadDraftSeed.Defaults(RackSystemKind.PushBack)),
                "Aquí no se puede crear un rack en el dibujo.");
            Refused(rig.Window.OpenAsNewDraft(RackCadDraftSeed.Defaults(RackSystemKind.Selective)),
                "Cabecera todavía no tiene editor en la ventana: se abre con su editor clásico.");
            Refused(rig.Window.OpenAsNewDraft(RackCadDraftSeed.FromProject(RackProject.ForSelectiveRack(null!))), "El diseño no tiene un Selectivo que abrir.");
            Refused(rig.Window.OpenAsNewDraft(RackCadDraftSeed.FromProject(RackProject.ForDynamic((RackCad.Domain.Systems.Dynamic.DynamicRackDesign)null!))),
                "El diseño no tiene un Dinámico que abrir.");
            Assert.Empty(rig.Window.Drafts);

            var dynamicOnly = new Rig(door: new DynamicOnlyDoor());
            Assert.False(dynamicOnly.Window.OpenAsNewDraft(RackCadDraftSeed.Defaults(RackSystemKind.SelectiveRack)).Opened);
            Assert.Equal("Aquí no se puede crear un rack en el dibujo.", dynamicOnly.Window.ActionMessage);
            Assert.True(dynamicOnly.Window.OpenAsNewDraft(RackCadDraftSeed.Defaults(RackSystemKind.PalletFlow)).Opened);

            var none = new Rig(withDoor: false);
            Assert.False(none.Window.OpenAsNewDraft(RackCadDraftSeed.Defaults(RackSystemKind.PalletFlow)).Opened);
            Assert.Equal("Aquí no se puede crear un rack en el dibujo.", none.Window.ActionMessage);

            rig.Host.Snapshot = WorkspacePanelFixtures.NoDocument();
            rig.Window.Refresh();
            Assert.False(rig.Window.OpenAsNewDraft(RackCadDraftSeed.Defaults(RackSystemKind.SelectiveRack)).Opened);
            Assert.Equal("No hay un dibujo en la ventana.", rig.Window.ActionMessage);
        }

        // ------------------------------------------------------------------ the review of the part (points 1, 2, 3, 5 and 8)

        // Point 1: B1 with changes, «Duplicar» (B2), B1 discarded. B2 is the only copy of that work, so it is protected as any draft with
        // changes: «Descartar el borrador» asks, «Cerrar el editor» asks, the close is vetoed and the prune names it.
        [Fact]
        public void TheCopyOfADraftWithChangesKeepsItsWorkProtectedWhenTheOriginalIsDiscarded()
        {
            var rig = new Rig();
            var original = rig.NewSelective();
            Edit(original);
            rig.Window.DuplicateDraft(rig.Window.Drafts[0]);
            var copy = rig.Window.NewSelectiveEditor!;
            Assert.True(copy.IsDirty);
            Assert.Equal((RackCadDraftItem.ModifiedKind, "Sin insertar, con cambios"), (rig.Window.Drafts[1].StatusKind, rig.Window.Drafts[1].StatusText));

            rig.Window.DiscardDraft(rig.Window.Drafts[0]);
            rig.Window.ConfirmDiscardDraft(rig.Window.Drafts[0]);
            var item = Assert.Single(rig.Window.Drafts);
            Assert.Same(copy, rig.Window.NewSelectiveEditor);

            Assert.Equal("Borrador 2 (Selectivo nuevo)", rig.Window.UnwrittenWorkOf(Session));
            rig.Window.DiscardDraft(item);
            Assert.True(item.IsConfirmingDiscard);
            rig.Window.CancelDiscardDraft(item);
            copy.RequestClose();
            Assert.True(copy.IsConfirmingClose);
            copy.CancelClose();

            rig.Summary.Dead.Add(Session);
            rig.Window.Refresh();
            Assert.Equal("Se cerró el dibujo de Borrador 2 (Selectivo nuevo) con cambios sin escribir en la ventana: esos cambios se perdieron.", rig.Window.ActionMessage);
        }

        // Point 1, the rest: a copy of a draft with nothing changed has nothing to lose; a copy of a design of the library goes back to it.
        [Fact]
        public void TheCopyOfADraftIsModifiedExactlyWhenItsOriginalWas()
        {
            var rig = new Rig();
            rig.NewSelective();
            var clean = rig.Window.DuplicateDraft(rig.Window.Drafts[0]);
            Assert.True(clean.Opened, clean.Reason);
            Assert.False(rig.Window.NewSelectiveEditor!.IsDirty);

            var library = rig.Window.OpenAsNewDraft(RackCadDraftSeed.FromLibrary(
                RackProject.ForSelectiveRack(SelectivePalletDesignDocument.From(TwoFondos(), null, "Nave lib")), "Nave lib"));
            var fromLibrary = rig.Window.NewSelectiveEditor!;
            Edit(fromLibrary);
            rig.Window.DuplicateDraft(library.Draft!);
            var libraryCopy = rig.Window.NewSelectiveEditor!;
            Assert.True(libraryCopy.IsDirty);
            Assert.True(string.IsNullOrEmpty(libraryCopy.Name));
            libraryCopy.Discard(); // back to the base of its original: the design of the library
            Assert.Equal(Json(new SelectivePalletDesignStore().Deserialize(fromLibrary.BaselineDesignJson)!.ToDomain()), Json(libraryCopy.Draft.Design));

            // A Dinámico too.
            var dynamic = rig.NewDynamic();
            rig.Window.DuplicateDraft(rig.Window.ActiveDraft!);
            Assert.False(rig.Window.NewDynamicEditor!.IsDirty);
            Edit(dynamic);
            rig.Window.OpenDraft(rig.Window.Drafts.Single(d => d.RackId == dynamic.RackId));
            rig.Window.DuplicateDraft(rig.Window.ActiveDraft!);
            var dynamicCopy = rig.Window.NewDynamicEditor!;
            Assert.True(dynamicCopy.IsDirty);
            Assert.Equal(DynamicJson(dynamic.Draft.Design), DynamicJson(dynamicCopy.Draft.Design));
            dynamicCopy.Discard();
            Assert.Equal(DynamicJson(RackCad.Application.VentanaRackCad.DynamicDraftEditor.CreateNew(Catalog()).Design), DynamicJson(dynamicCopy.Draft.Design));
        }

        // Point 2: what AutoCAD answers to a draft that is not on screen is said in the message of the window too, with its label.
        [Fact]
        public void TheAnswerToADraftThatIsNotOnScreenIsSaidInTheWindowWithItsLabel()
        {
            var rig = new Rig();
            var first = rig.NewSelective();
            first.InsertWith(Planta); // T4 ins: the panel of «Insertar vista…»
            var write = rig.Door.SelectiveWrites.Last();
            var second = rig.NewSelective(); // another draft is open now
            Assert.NotSame(first, rig.Window.NewSelectiveEditor);

            rig.Door.Written(SelectiveDoorInsertion.AnswerForNewRack(write.RequestId, write.RackId, 0, null!));
            const string NotPlaced = "AutoCAD no insertó el rack nuevo: se canceló la colocación o no se pudo preparar. El borrador sigue en la ventana; revisa la línea de órdenes de AutoCAD.";
            Assert.Equal(NotPlaced, first.Message);
            Assert.Equal("Borrador 1: " + NotPlaced, rig.Window.ActionMessage);
            Assert.True(rig.Window.ActionFailed);

            // «Dejar de esperar» of a draft that is not on screen.
            first.InsertWith(Planta); // T4 ins: the panel of «Insertar vista…»
            rig.Window.ShowPage(RackCadWindowPage.Bom);
            rig.Window.StopWaiting();
            Assert.Equal("Borrador 1: Se dejó de esperar a AutoCAD: si la orden llega a correr, no escribirá nada. El borrador sigue sin escribir.", rig.Window.ActionMessage);
            Assert.False(rig.Window.ActionFailed);

            // On screen, the editor says it and the window does not repeat it.
            rig.Window.OpenDraft(rig.Window.Drafts[1]);
            rig.Window.DismissMessage();
            second.InsertWith(Planta); // T4 ins: the panel of «Insertar vista…»
            var shown = rig.Door.SelectiveWrites.Last();
            rig.Door.Written(SelectiveDoorInsertion.AnswerForNewRack(shown.RequestId, shown.RackId, 0, null!));
            Assert.Equal(NotPlaced, second.Message);
            Assert.Equal(string.Empty, rig.Window.ActionMessage);

            // A Dinámico too.
            var dynamic = rig.NewDynamic();
            dynamic.InsertWith(RackViewAddress.Post(0)); // T4 ins: the panel of «Insertar vista…»
            var dynamicWrite = rig.Door.Writes.Last();
            rig.Window.OpenDraft(rig.Window.Drafts[0]);
            rig.Door.Written(RackDoorRules.AnswerForNewRack(dynamicWrite.RequestId, dynamicWrite.RackId, 0, null));
            Assert.StartsWith("Borrador 3: ", rig.Window.ActionMessage);
            Assert.EndsWith(dynamic.Message, rig.Window.ActionMessage);
        }

        // Point 3: a design of the library is named as the classic editor opened from the library names it.
        [Fact]
        public void ADesignOfTheLibraryTakesTheNameTheClassicEditorGivesIt()
        {
            var rig = new Rig();

            // A Dinámico keeps no name in its document: it takes the name of the entry of the library (the file), as LoadDesignForNew.
            var source = rig.NewDynamic();
            var dynamicProject = RackProject.ForDynamic(source.Draft.SnapshotForWrite()!.Design);
            Assert.True(rig.Window.OpenAsNewDraft(RackCadDraftSeed.FromProject(dynamicProject)).Opened);
            Assert.True(string.IsNullOrEmpty(rig.Window.NewDynamicEditor!.Name)); // FromProject: the document keeps none
            Assert.True(rig.Window.OpenAsNewDraft(RackCadDraftSeed.FromLibrary(dynamicProject, " Nave din ", "del diseño «Nave din» de la biblioteca")).Opened);
            Assert.Equal("Nave din", rig.Window.NewDynamicEditor!.Name);
            Assert.Equal("Borrador 3: Dinámico nuevo en esta ventana, del diseño «Nave din» de la biblioteca. Todavía no está en el dibujo.",
                rig.Window.NewDynamicEditor.Message);

            // A Selectivo keeps the name of its document, as LoadForNew; with none, the automatic one of I-60 (not the name of the file).
            var named = RackProject.ForSelectiveRack(SelectivePalletDesignDocument.From(Structure(), null, "Nave lib"));
            Assert.True(rig.Window.OpenAsNewDraft(RackCadDraftSeed.FromLibrary(named, "archivo")).Opened);
            Assert.Equal("Nave lib", rig.Window.NewSelectiveEditor!.Name);
            var unnamed = RackProject.ForSelectiveRack(SelectivePalletDesignDocument.From(Structure(), null, null));
            Assert.True(rig.Window.OpenAsNewDraft(RackCadDraftSeed.FromLibrary(unnamed, "archivo")).Opened);
            Assert.True(string.IsNullOrEmpty(rig.Window.NewSelectiveEditor!.Name));
        }

        // Point 8: «Duplicar» is contained as every action of the window.
        [Fact]
        public void DuplicarIsContained()
        {
            var rig = new Rig();
            var editor = rig.NewSelective();
            editor.InsertWith(Planta); // AutoCAD writes it: «Duplicar» refuses and says it // T4 ins: the panel of «Insertar vista…»
            var thrown = false;
            rig.Window.PropertyChanged += (_, e) =>
            {
                if (thrown || e.PropertyName != nameof(RackCadWindowViewModel.ActionMessage)) return;
                thrown = true;
                throw new InvalidOperationException("prueba");
            };

            var opening = rig.Window.DuplicateDraft(rig.Window.Drafts[0]);
            Assert.True(thrown);
            Assert.False(opening.Opened);
            Assert.Equal("La ventana tuvo un fallo contenido: prueba", rig.Window.ActionMessage);
            Assert.Single(rig.Window.Drafts);
        }

        // ------------------------------------------------------------------ the views, with real WPF

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

        private static void WithBindingErrors(Action body)
        {
            var errors = new BindingErrors();
            PresentationTraceSources.Refresh();
            var source = PresentationTraceSources.DataBindingSource;
            var level = source.Switch.Level;
            source.Listeners.Add(errors);
            source.Switch.Level = SourceLevels.Error;
            try
            {
                body();
            }
            finally
            {
                source.Listeners.Remove(errors);
                source.Switch.Level = level;
            }

            Assert.Empty(errors.Messages);
        }

        private static void Lay(FrameworkElement view)
        {
            view.Measure(new Size(1200, 900));
            view.Arrange(new Rect(0, 0, 1200, 900));
            view.UpdateLayout();
        }

        private static void Click(Button button) => button.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));

        private static string Name(UIElement element) => UIElementAutomationPeer.CreatePeerForElement(element).GetName();

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

        private static bool Shown(UIElement element)
        {
            for (DependencyObject? node = element; node != null; node = System.Windows.Media.VisualTreeHelper.GetParent(node))
                if (node is UIElement ui && ui.Visibility != Visibility.Visible) return false;
            return true;
        }

        private static Button ButtonNamed(DependencyObject root, string name)
            => Descendants<Button>(root).Single(b => Shown(b) && Name(b) == name);

        [Fact]
        public void TheSideColumnShowsTheDraftsUnderNuevoWithTheirStateAndOpensThem()
        {
            StaTestRunner.Run(() => WithBindingErrors(() =>
            {
                var rig = new Rig();
                var view = new RackCadWindowView(rig.Window);
                Lay(view);
                Assert.DoesNotContain(Descendants<Button>(view.SectionList), b => Shown(b) && Name(b).StartsWith("Borrador", StringComparison.Ordinal));

                var first = rig.NewSelective();
                rig.NewDynamic();
                Edit(first);
                Lay(view);

                // Under «Nuevo», before «Variables», in order, each one with its name, system and state.
                var order = Descendants<Button>(view.SectionList).Where(Shown).Select(Name).ToList();
                Assert.Equal(new[]
                {
                    "Nuevo",
                    "Borrador 1, Selectivo, Sin insertar, con cambios",
                    "Borrador 2, Dinámico, Sin insertar",
                    "Variables", "BOM y lista de racks", "Almacén", "Biblioteca de diseños", "Configuración", // T5 al: «Almacén» (question 1 of «almacen»)
                }, order.ToArray());
                var texts = Descendants<TextBlock>(view.SectionList).Where(Shown).Select(t => t.Text).ToList();
                Assert.Contains("Borrador 1 · Selectivo", texts);
                Assert.Contains("Modificado", texts);
                Assert.Contains("Sin insertar", texts);

                // A click opens the draft: «Nuevo» shows its editor.
                Click(ButtonNamed(view.SectionList, "Borrador 1, Selectivo, Sin insertar, con cambios"));
                Lay(view);
                Assert.Same(first, rig.Window.NewSelectiveEditor);
                Assert.Same(first, Assert.IsType<SelectiveEditorView>(view.NewSelectiveRackHost.Content).DataContext);
                Assert.Equal(Visibility.Visible, view.NewSelectiveRackHost.Visibility);
                Assert.Equal(Visibility.Collapsed, view.NewRackHost.Visibility);

                // «Nuevo» itself goes to its home.
                Click(ButtonNamed(view.SectionList, "Nuevo"));
                Lay(view);
                Assert.Equal(Visibility.Visible, view.NuevoPage.Visibility);
                Assert.Equal(Visibility.Collapsed, view.NewSelectiveRackHost.Visibility);
            }));
        }

        [Fact]
        public void TheBarOfTheDraftsListsThemOnTheHomeOfNuevoAndShowsTheStripOfTheOneOpen()
        {
            StaTestRunner.Run(() => WithBindingErrors(() =>
            {
                var rig = new Rig();
                var view = new RackCadWindowView(rig.Window);
                rig.Window.ShowPage(RackCadWindowPage.Nuevo);
                Lay(view);
                Assert.Equal(Visibility.Collapsed, view.DraftsBar.Visibility); // no drafts, no bar

                var first = rig.NewSelective();
                Lay(view);
                Assert.Equal(Visibility.Visible, view.DraftsBar.Visibility);
                Assert.Equal(Visibility.Visible, view.ActiveDraftStrip.Visibility);
                Assert.Equal(Visibility.Collapsed, view.DraftListPanel.Visibility);
                Assert.Equal("Borrador 1", view.ActiveDraftTitle.Text);
                Assert.Equal("Selectivo · Sin insertar", view.ActiveDraftDetail.Text);

                // «Duplicar» of the strip: a second draft, open.
                Click(view.DuplicateActiveDraftButton);
                Lay(view);
                Assert.Equal(2, rig.Window.Drafts.Count);
                Assert.Equal("Borrador 2", view.ActiveDraftTitle.Text);

                // «Todos los borradores»: the home of «Nuevo» with the list, and the systems below.
                Click(view.DraftsHomeButton);
                Lay(view);
                Assert.Equal(Visibility.Collapsed, view.ActiveDraftStrip.Visibility);
                Assert.Equal(Visibility.Visible, view.DraftListPanel.Visibility);
                Assert.Equal(Visibility.Visible, view.NuevoPage.Visibility);
                Assert.Equal(RackCadWindowViewModel.DraftsIntro, view.DraftsIntroText.Text);
                var names = Descendants<Button>(view.DraftsBar).Where(Shown).Select(Name).ToList();
                Assert.Equal(new[]
                {
                    "Abrir Borrador 1", "Duplicar Borrador 1", "Descartar Borrador 1",
                    "Abrir Borrador 2", "Duplicar Borrador 2", "Descartar Borrador 2",
                }, names.ToArray());

                // A draft with changes asks first, inline, in its row; «Cancelar» keeps it.
                Edit(first);
                Lay(view);
                Click(ButtonNamed(view.DraftsBar, "Descartar Borrador 1"));
                Lay(view);
                Assert.Contains(Descendants<TextBlock>(view.DraftsBar), t => Shown(t) && t.Text == "¿Descartar Borrador 1? Tiene cambios y no está en el dibujo: se pierden.");
                Click(ButtonNamed(view.DraftsBar, "Cancelar el descarte de Borrador 1"));
                Lay(view);
                Assert.Equal(2, rig.Window.Drafts.Count);
                Click(ButtonNamed(view.DraftsBar, "Descartar Borrador 1"));
                Lay(view);
                Click(ButtonNamed(view.DraftsBar, "Confirmar: descartar Borrador 1"));
                Lay(view);
                Assert.Equal(new[] { "Borrador 2" }, rig.Window.Drafts.Select(d => d.Label).ToArray());

                // «Abrir», then «Descartar el borrador» of the strip of a draft with nothing changed: gone, and the home again.
                Click(ButtonNamed(view.DraftsBar, "Abrir Borrador 2"));
                Lay(view);
                Assert.Equal(Visibility.Visible, view.ActiveDraftStrip.Visibility);
                Assert.IsType<SelectiveEditorView>(view.NewSelectiveRackHost.Content);
                Click(view.DiscardActiveDraftButton);
                Lay(view);
                Assert.Empty(rig.Window.Drafts);
                Assert.Equal(Visibility.Collapsed, view.DraftsBar.Visibility);
                Assert.Equal(Visibility.Visible, view.NuevoPage.Visibility);

                // The strip asks too, with its own names.
                Edit(rig.NewSelective());
                Lay(view);
                Click(view.DiscardActiveDraftButton);
                Lay(view);
                Assert.True(Shown(view.ConfirmDiscardActiveDraftButton));
                Assert.Equal("Confirmar: descartar el borrador abierto", Name(view.ConfirmDiscardActiveDraftButton));
                Click(view.ConfirmDiscardActiveDraftButton);
                Lay(view);
                Assert.Empty(rig.Window.Drafts);

                // On another page there is no bar, drafts or not.
                rig.NewSelective();
                rig.Window.ShowPage(RackCadWindowPage.Bom);
                Lay(view);
                Assert.Equal(Visibility.Collapsed, view.DraftsBar.Visibility);
            }));
        }
    }
}
