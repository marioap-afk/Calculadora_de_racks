#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using RackCad.Application.CustomProperties;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using RackCad.Application.VentanaRackCad;
using RackCad.Application.Workspace;
using RackCad.UI.VentanaRackCad;
using RackCad.UI.Workspace;
using Xunit;
using static RackCad.UI.Tests.SelectiveWindowFixtures;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64 A-14, cuarta tanda (va): the page «Variables» of the RackCad window (prototype «variables», VA-01 to VA-10). The page keeps a
    /// draft of the register, its broken references and the custom properties of the project, says what «Actualizar» would write, and
    /// writes it all with ONE request of the door; a rack with an unwritten draft that uses a variable that changes stops it (VA-04).
    /// </summary>
    public class RackCadVariablesPageTests
    {
        private static readonly SessionId Session = new(7);

        /// <summary>A door of the racks and of the page «Variables», as the door of the Plugin is since the cuarta tanda.</summary>
        private sealed class FullDoor : IRackCadSelectiveDoorPort, IRackCadVariablesDoorPort, IRackCadShortcutPort // T6 rl: IRackCadShortcutPort
        {
            // T6 rl: RACKVARIABLES and RACKPROPIEDADES, as the command runs them: it asks the window (the reads the window hands to the door
            // meanwhile are the ones the command runs inside itself), the test answers them, and the command says it is done.
            public event Action<RackCadShortcutAsk>? ShortcutAsked;

            public event Action<RackCadShortcutAsk>? ShortcutDone;

            public RackCadShortcutAsk Ask(RackCadShortcutTarget target, string? rackId = null)
            {
                var ask = new RackCadShortcutAsk(target, rackId!, null!);
                ShortcutAsked?.Invoke(ask);
                return ask;
            }

            public void Done(RackCadShortcutAsk ask) => ShortcutDone?.Invoke(ask);

            public void SelectiveReadAnswer(SelectiveDesignRead read) => SelectiveRead?.Invoke(read);

            public List<RackDoorRequest> SelectiveReads { get; } = new();

            public List<SelectiveDoorWrite> SelectiveWrites { get; } = new();

            public List<VariablesDoorRequest> VariablesReads { get; } = new();

            public List<VariablesDoorWrite> VariablesWrites { get; } = new();

            public List<long> Abandoned { get; } = new();

            public RackCadClassicResult Answer { get; set; } = RackCadClassicResult.Started();

            public event Action<RackDesignRead>? DesignRead { add { } remove { } }

            public event Action<RackDesignWritten>? DesignWritten;

            public event Action<long>? RequestStarted;

            public event Action<SelectiveDesignRead>? SelectiveRead;

            public event Action<VariablesDoorRead>? VariablesRead;

            public event Action<VariablesDoorWritten>? VariablesWritten;

            public RackCadClassicResult RequestDesign(RackDoorRequest request) => Answer;

            public RackCadClassicResult RequestDynamicUpdate(RackDoorWrite write) => Answer;

            public RackCadClassicResult RequestSelectiveDesign(RackDoorRequest request)
            {
                SelectiveReads.Add(request);
                return Answer;
            }

            public RackCadClassicResult RequestSelectiveUpdate(SelectiveDoorWrite write)
            {
                SelectiveWrites.Add(write);
                return Answer;
            }

            public RackCadClassicResult RequestVariables(VariablesDoorRequest request)
            {
                VariablesReads.Add(request);
                return Answer;
            }

            public RackCadClassicResult RequestVariablesUpdate(VariablesDoorWrite write)
            {
                VariablesWrites.Add(write);
                return Answer;
            }

            public void Abandon(long requestId) => Abandoned.Add(requestId);

            public void Start(long requestId) => RequestStarted?.Invoke(requestId);

            public void AnswerVariablesRead(VariablesDoorRead read) => VariablesRead?.Invoke(read);

            public void AnswerVariablesWrite(VariablesDoorWritten written) => VariablesWritten?.Invoke(written);

            public void AnswerLastSelectiveRead(bool bound) => SelectiveRead?.Invoke(SelectiveWindowFixtures.Read(SelectiveReads.Last().RequestId, RackId, null, bound));

            public void AnswerLastSelectiveWrite(bool written) =>
                DesignWritten?.Invoke(new RackDesignWritten(SelectiveWrites.Last().RequestId, RackId, written, written ? "Escrito." : "No."));
        }

        private sealed class Rig
        {
            public Rig(bool withDoor = true)
            {
                Host = new WorkspacePortDouble { Snapshot = WorkspacePanelFixtures.Ready(session: 7) };
                Summary = new WorkspaceBrowserSummaryPortDouble
                {
                    ListResponder = (_, _) => BrowserRackList.Ready(new[]
                    {
                        new BrowserRackListEntry("SEL-1", "Selectivo A", "selective", new[] { "frontal" }),
                    }),
                };
                Panel = new WorkspacePanelViewModel(Host, _ => { }, new WorkspaceNavigationPortDouble(), Summary, new WorkspacePilotEditPortDouble());
                Window = new RackCadWindowViewModel(Panel, null, null, withDoor ? Door : null, UiSupport.LoadCatalogSafe);
                Window.Refresh();
            }

            public WorkspacePortDouble Host { get; }

            public WorkspaceBrowserSummaryPortDouble Summary { get; }

            public WorkspacePanelViewModel Panel { get; }

            public FullDoor Door { get; } = new();

            public RackCadWindowViewModel Window { get; }

            public VariablesPageViewModel ShowAndRead(VariablesDoorRead? read = null)
            {
                Window.ShowPage(RackCadWindowPage.Variables);
                var request = Door.VariablesReads.Last();
                Door.AnswerVariablesRead(read ?? ReadOf(request.RequestId));
                var page = Window.VariablesPage!;
                Assert.True(page.IsLoaded, page.Message);
                return page;
            }
        }

        /// <summary>The drawing: the variable «Holgura estándar» (10 in) and the Selectivo A that uses it (or a broken reference).</summary>
        private static VariablesDoorRead ReadOf(long requestId, double value = 10.0, bool broken = false, CustomPropertiesReadResult? properties = null)
        {
            var authored = Store.Deserialize(Authored(Structure(), bound: true));
            var entries = new List<ProjectVariableScanEntry> { ProjectVariableScanEntry.Selective("D1", RackId, authored, 1) };
            var registry = broken ? ProjectVariablesReadResult.Absent() : Register(value);
            return new VariablesDoorRead(requestId, true, registry, entries,
                properties ?? new CustomPropertiesStore().Read(CustomPropertiesPayload.Absent()), null);
        }

        private static VariablesPageViewModel Page(VariablesDoorRead? read = null, Func<IReadOnlyList<string>, IReadOnlyList<(string, string)>>? conflicts = null)
        {
            var page = new VariablesPageViewModel(Session, conflicts);
            Assert.True(page.Load(read ?? ReadOf(1)));
            return page;
        }

        private static void Define(VariablesPageViewModel page, string text)
        {
            page.InspectorDefinition = text;
            page.CommitInspectorDefinition();
        }

        private static void Name(VariablesPageViewModel page, string text)
        {
            page.InspectorName = text;
            page.CommitInspectorName();
        }

        // ------------------------------------------------------------------ the page

        [Fact]
        public void ThePageShowsTheRegisterWithItsValuesAndTheRacksByNameAndStartsClean()
        {
            var page = Page();

            var row = Assert.Single(page.Rows);
            Assert.Equal("Holgura estándar", row.DisplayName);
            Assert.Equal("10", row.DefinitionShown);
            Assert.Equal("10 in", row.ValueText);
            Assert.Equal("1", row.RacksText);
            Assert.Equal("Variable 1: Holgura estándar", row.AutomationName);
            Assert.Equal("Limpio", page.StatusText);
            Assert.Equal("Sin cambios que escribir.", page.DiffText);
            Assert.False(page.CanUpdate);

            // The inspector: the rack that uses it, by name (VA-09: no GUID, no key of the property), and why «Eliminar» is not offered.
            Assert.Same(row, page.Selected);
            Assert.Equal("Selectivo A · Holgura vertical", Assert.Single(page.Consumers).Text);
            Assert.False(page.CanDelete);
            Assert.True(page.ShowUnlink);
            Assert.Equal("La usa 1 rack, así que «Eliminar» no se ofrece. «Desvincular todos y eliminar…» los desvincula conservando su valor.", page.DeleteWhy);
            Assert.DoesNotContain(VarId, page.DeleteWhy + page.UnlinkText + row.DefinitionShown, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void AChangeGoesToTheDraftSaysWhatActualizarWritesAndDiscardGoesBack()
        {
            var page = Page();
            Define(page, "12");

            Assert.True(page.IsDirty);
            Assert.Equal("Modificado", page.StatusText);
            Assert.Equal("12 in (antes 10)", page.Rows[0].ValueText);
            Assert.Equal("Modificada", page.Rows[0].Tag);
            Assert.Equal(new[] { "«Holgura estándar»: 10 → 12 in." }, page.Changes);
            Assert.Equal("Se redibujan todas las vistas de Selectivo A, en la misma orden y con un solo «U».", page.RedrawText);
            Assert.Equal("1 cambio sin escribir", page.DiffText);
            Assert.True(page.CanUpdate, page.UpdateBlockedText);

            // Leaving the box without changing it changes nothing.
            page.CommitInspectorDefinition();
            Assert.Single(page.Changes);

            page.Discard();
            Assert.False(page.IsDirty);
            Assert.Equal("10 in", page.Rows[0].ValueText);
            Assert.Equal("Borrador descartado: se muestra lo último que se leyó del dibujo o se escribió en él.", page.Message);
        }

        [Fact]
        public void AWrongValueIsSaidInItsFieldAndActualizarSaysWhyItCannotWrite()
        {
            var page = Page();
            Define(page, "6,5");

            Assert.Equal("Usa punto decimal: 6.5.", page.InspectorDefinitionError);
            Assert.Equal("No válida", page.Rows[0].Tag);
            Assert.Equal("Hay 1 valor no válido.", page.UpdateBlockedText);
            Assert.False(page.CanUpdate);
            Assert.Equal("Variables de proyecto (con problemas)", page.VariablesTabHeader);
        }

        [Fact]
        public void ANewVariableFollowsTheOtherByIdentityThroughARenameAndRepeatedNamesAreRefused()
        {
            var page = Page();
            page.NewVariable();
            Name(page, "Alta");
            Define(page, "=Holgura estándar + 2");
            Assert.Equal("12 in", page.InspectorValueText);
            Assert.Equal("Nueva", page.Rows[1].Tag);

            page.Select(page.Rows[0]);
            Name(page, "Base");
            Assert.Equal("=Base + 2", page.Rows[1].DefinitionShown);
            Assert.Equal("«Alta»", page.DependentsText.Split('.')[0]);
            Assert.False(page.CanUnlink);
            Assert.StartsWith("La usa la fórmula de «Alta»: cámbiala antes de eliminar esta variable.", page.DeleteWhy);

            // VA-05.
            page.Select(page.Rows[1]);
            Name(page, "base");
            Assert.Equal("Ya hay otra variable con este nombre.", page.InspectorNameError);
            Assert.False(page.CanUpdate);

            Name(page, "Alta");
            Assert.True(page.CanUpdate, page.UpdateBlockedText);
            Assert.Contains("Renombrar «Holgura estándar» a «Base». No redibuja ningún rack.", page.Changes);
            Assert.Contains("Nueva: «Alta», con la fórmula =Base + 2 (12 in).", page.Changes);
        }

        [Fact]
        public void UnlinkAllAndDeleteAsksInlineWithTheRacksAndTheValueTheyKeepThenGoesToTheDraft()
        {
            var page = Page();
            page.AskUnlink();

            Assert.True(page.IsAskingUnlink);
            Assert.Equal("¿Desvincular todos y eliminar «Holgura estándar»?", page.UnlinkTitle);
            Assert.Equal("El rack que la usa deja de seguirla y conserva su valor de ahora, 10 in: Selectivo A (holgura vertical). Después se elimina la "
                         + "variable. Va al borrador: el dibujo no cambia hasta «Actualizar».", page.UnlinkText);
            Assert.False(page.IsDirty);

            page.ConfirmUnlink();
            Assert.False(page.IsAskingUnlink);
            Assert.Equal("Se eliminará", page.Rows[0].Tag);
            Assert.Equal("Se eliminará al actualizar. Antes, Selectivo A deja de seguirla y conserva 10 in.", page.RemovedText);
            Assert.Equal(new[] { "Desvincular todos y eliminar «Holgura estándar»: Selectivo A deja de seguirla y conserva 10 in." }, page.Changes);
            Assert.True(page.CanUpdate, page.UpdateBlockedText);

            page.UndoDelete();
            Assert.False(page.IsDirty);
        }

        [Fact]
        public void ABrokenReferenceIsRepairedAfterAnInlineQuestionAndOnlyThenGoesToTheDraft()
        {
            var page = Page(ReadOf(1, broken: true));

            var rack = Assert.Single(page.Broken);
            Assert.Equal("Selectivo A", rack.RackName);
            Assert.Equal("Holgura vertical", rack.PropertiesText);
            Assert.True(rack.CanRepair);
            page.AskRepair(rack);
            Assert.True(page.IsAskingRepair);
            Assert.Equal("¿Reparar Selectivo A?", page.RepairTitle);
            Assert.StartsWith("Reparar actúa sobre todo el rack: se quitan todos sus vínculos rotos (1), no solo una fila.", page.RepairText);
            Assert.False(page.IsDirty);

            page.ConfirmRepair();
            Assert.True(rack.IsQueued);
            Assert.StartsWith("Reparar Selectivo A: holgura vertical queda en ", Assert.Single(page.Changes));
            Assert.Equal("Se redibujan todas las vistas de Selectivo A, en la misma orden y con un solo «U».", page.RedrawText);
            Assert.True(page.CanUpdate, page.UpdateBlockedText);
            Assert.Single(page.BuildWrite(9)!.Batch.RepairRacks, RackId);
        }

        [Fact]
        public void ThePropertiesOfTheProjectAreTheSecondTabWithTheSameActualizar()
        {
            // VA-06.
            var page = Page();
            page.AddProperty();
            var row = Assert.Single(page.Properties);
            page.CommitProperties();
            Assert.Equal("El nombre no puede quedar vacío.", row.NameError);
            Assert.Equal("Propiedades del proyecto (con problemas)", page.PropertiesTabHeader);
            Assert.False(page.CanUpdate);

            row.Name = "Revisión";
            row.Value = "A\nB";
            page.CommitProperties();
            Assert.Equal(string.Empty, row.NameError);
            Assert.Equal("Nueva", row.Tag);
            Assert.Equal(new[] { "Propiedades del proyecto: 1 nueva. No redibuja ningún rack." }, page.Changes);
            Assert.True(page.CanUpdate, page.UpdateBlockedText);

            var write = page.BuildWrite(4)!;
            Assert.Equal("Revisión", Assert.Single(write.ProjectProperties!).Name);
            Assert.Equal(VariablesDoorRules.PropertiesFingerprint(ReadOf(1).ProjectProperties), write.PropertiesFingerprint);
            Assert.Equal(VariablesDoorRules.RegistryFingerprint(Register(10.0)), write.RegistryFingerprint);

            page.RemoveProperty(row);
            Assert.False(page.IsDirty);
        }

        // ------------------------------------------------------------------ the window and its door

        [Fact]
        public void ShowingThePageReadsItOnceAndActualizarSendsTheWholeDraftAsOneRequest()
        {
            var rig = new Rig();
            var page = rig.ShowAndRead();
            Assert.Single(rig.Door.VariablesReads);
            Assert.False(rig.Window.IsWaitingForAutoCad);
            rig.Window.ShowPage(RackCadWindowPage.Rack);
            rig.Window.ShowPage(RackCadWindowPage.Variables);
            Assert.Single(rig.Door.VariablesReads);

            Define(page, "12");
            // D-08: the page with changes is work not written of its drawing.
            Assert.Equal("Variables", rig.Window.UnwrittenWorkOf(Session));

            page.RequestWrite();
            var write = Assert.Single(rig.Door.VariablesWrites);
            Assert.Equal("12", Assert.Single(write.Batch.Rows).DefinitionText);
            Assert.True(page.IsWriting);
            Assert.Equal("Escribiendo", page.StatusText);
            Assert.True(rig.Window.IsWaitingForAutoCad);
            Assert.Equal("Esperando a que AutoCAD escriba las variables. Si no responde (por ejemplo, porque había texto a medio escribir en la línea de órdenes), pulsa «Dejar de esperar».",
                rig.Window.WaitingText);

            // One request at a time, for the page and the editors.
            rig.Window.OpenRack("SEL-1");
            rig.Window.EditSelectiveHere();
            Assert.Empty(rig.Door.SelectiveReads);

            rig.Door.Start(write.RequestId);
            Assert.False(rig.Window.CanStopWaiting);
            Assert.Equal("La orden ya corre en AutoCAD: se termina o se cancela allí, y la respuesta siempre llega.", page.WritingText);

            rig.Door.AnswerVariablesWrite(new VariablesDoorWritten(write.RequestId, true, "ok", new[] { RackId }, 1));
            Assert.False(page.IsWriting);
            Assert.Equal("Variables escritas en el dibujo: 1 cambio; se redibujó Selectivo A. Un «U» en AutoCAD lo deshace todo.", page.Message);
            Assert.Null(rig.Window.UnwrittenWorkOf(Session));
            // The drawing is the authority: the page reads it again.
            Assert.Equal(2, rig.Door.VariablesReads.Count);
            Assert.True(page.IsReading);
        }

        // Smoke T4 in AutoCAD (F-2): the read that follows «Actualizar» is asked inside the answer of RACKVARIABLESAPLICAR, while the command
        // still runs, and the real door refuses it as Busy. The page kept a red «Hay un comando en curso…» over a write that went well.
        [Fact]
        public void TheReadAfterTheOwnWriteRefusedAsBusyKeepsTheMessageOfTheWriteAndReadsWhenTheWindowPresentsAgain()
        {
            var rig = new Rig();
            var page = rig.ShowAndRead();
            Define(page, "12");
            page.RequestWrite();
            var write = rig.Door.VariablesWrites.Single();
            rig.Door.Start(write.RequestId);

            rig.Door.Answer = RackCadClassicResult.Without(RackCadClassicStatus.Busy, "Hay un comando en curso.");
            rig.Door.AnswerVariablesWrite(new VariablesDoorWritten(write.RequestId, true, "ok", new[] { RackId }, 1));
            Assert.Equal(2, rig.Door.VariablesReads.Count);
            Assert.False(page.MessageIsError, page.Message);
            Assert.Equal("Variables escritas en el dibujo: 1 cambio; se redibujó Selectivo A. Un «U» en AutoCAD lo deshace todo.", page.Message);
            Assert.True(page.IsNotLoaded);
            Assert.False(page.IsReading);
            Assert.False(rig.Window.IsWaitingForAutoCad);

            // The command ended: the window presents the drawing again and the page reads it.
            rig.Door.Answer = RackCadClassicResult.Started();
            rig.Window.Refresh();
            Assert.Equal(3, rig.Door.VariablesReads.Count);
            rig.Door.AnswerVariablesRead(ReadOf(rig.Door.VariablesReads.Last().RequestId, value: 12.0));
            Assert.True(page.IsLoaded);
            Assert.Equal("12 in", page.Rows[0].ValueText);
            Assert.False(page.MessageIsError, page.Message);

            // A refusal of a read the user asks for does say it, and the next read the user asks for clears it.
            rig.Door.Answer = RackCadClassicResult.Without(RackCadClassicStatus.Busy, "Hay un comando en curso.");
            page.RequestRead();
            Assert.True(page.MessageIsError);
            rig.Door.Answer = RackCadClassicResult.Started();
            page.RequestRead();
            Assert.False(page.MessageIsError, page.Message);
        }

        [Fact]
        public void AnAnswerOfAnotherNumberIsIgnoredAndAFailedWriteKeepsTheDraft()
        {
            var rig = new Rig();
            var page = rig.ShowAndRead();
            Define(page, "12");
            page.RequestWrite();
            var write = rig.Door.VariablesWrites.Single();

            rig.Door.AnswerVariablesWrite(new VariablesDoorWritten(write.RequestId + 50, true, "otra"));
            Assert.True(page.IsWriting);

            rig.Door.AnswerVariablesWrite(new VariablesDoorWritten(write.RequestId, false, "El registro de variables cambió en el dibujo…"));
            Assert.False(page.IsWriting);
            Assert.True(page.IsDirty);
            Assert.True(page.MessageIsError);
            Assert.Equal("El registro de variables cambió en el dibujo…", page.Message);
        }

        [Fact]
        public void PropertiesOfTheProjectThatCouldNotBeWrittenStayInTheDraftAfterTheVariablesAreWritten()
        {
            // Correction 2 of the review (VA-S7): the variables are written and the second transaction, of the properties, fails.
            var rig = new Rig();
            var page = rig.ShowAndRead();
            Define(page, "12");
            page.AddProperty();
            page.Properties[0].Name = "Revisión";
            page.Properties[0].Value = "B";
            page.CommitProperties();
            page.RequestWrite();
            var write = rig.Door.VariablesWrites.Single();

            rig.Door.AnswerVariablesWrite(VariablesDoorRules.AnswerForWrite(write.RequestId, true, new[] { RackId }, 1, false,
                "las propiedades del proyecto cambiaron en el dibujo desde que se leyeron."));
            Assert.True(page.MessageIsError);
            Assert.Contains("las propiedades del proyecto cambiaron en el dibujo desde que se leyeron.", page.Message);

            // The page reads the drawing again: the variables are the written ones, the properties stay in the draft.
            var reread = rig.Door.VariablesReads.Last();
            rig.Door.AnswerVariablesRead(ReadOf(reread.RequestId, value: 12.0));
            Assert.True(page.IsLoaded);
            Assert.Equal("12 in", page.Rows[0].ValueText);
            var row = Assert.Single(page.Properties);
            Assert.Equal(("Revisión", "B"), (row.Name, row.Value));
            Assert.True(page.IsDirty);
            Assert.Equal(new[] { "Propiedades del proyecto: 1 nueva. No redibuja ningún rack." }, page.Changes);
        }

        [Fact]
        public void StoppingTheWaitOfTheWriteKeepsTheDraftAndTellsTheDoor()
        {
            var rig = new Rig();
            var page = rig.ShowAndRead();
            Define(page, "12");
            page.RequestWrite();
            var write = rig.Door.VariablesWrites.Single();

            rig.Window.StopWaiting();
            Assert.Equal(new[] { write.RequestId }, rig.Door.Abandoned);
            Assert.False(page.IsWriting);
            Assert.True(page.IsDirty);
            Assert.StartsWith("Se dejó de esperar a AutoCAD", page.Message);
            rig.Door.AnswerVariablesWrite(new VariablesDoorWritten(write.RequestId, true, "tarde", new[] { RackId }, 1));
            Assert.True(page.IsDirty);
        }

        [Fact]
        public void ARackWithAnUnwrittenDraftThatUsesAChangingVariableStopsActualizar()
        {
            // VA-04.
            var rig = new Rig();
            rig.Window.OpenRack("SEL-1");
            rig.Window.EditSelectiveHere();
            rig.Door.AnswerLastSelectiveRead(bound: true);
            var editor = rig.Window.ActiveSelectiveEditor!;
            Write(Field(editor.CellFields, "cHeight"), "72");
            Assert.True(editor.IsDirty);

            var page = rig.ShowAndRead();
            Define(page, "12");
            var conflict = Assert.Single(page.Conflicts);
            Assert.Equal("Selectivo A tiene cambios sin escribir", conflict.Title);
            Assert.Equal("Ir a Selectivo A", conflict.GoLabel);
            Assert.Equal("Antes, escribe o descarta el borrador de Selectivo A.", page.UpdateBlockedText);
            Assert.False(page.CanUpdate);

            editor.Discard();
            Assert.Empty(page.Conflicts);
            Assert.True(page.CanUpdate, page.UpdateBlockedText);
        }

        [Fact]
        // T6 st (review of 5114345e, A1; the Owner: «Sí, marcarlo»): the clean editor of a rack the write redrew is no longer closed: it is
        // marked «El dibujo cambió», with «Volver a leer del dibujo», which reads it again.
        public void TheCleanEditorOfARackTheWriteRedrewIsMarkedSoItIsReadAgain()
        {
            var rig = new Rig();
            rig.Window.OpenRack("SEL-1");
            rig.Window.EditSelectiveHere();
            rig.Door.AnswerLastSelectiveRead(bound: true);
            var editor = Assert.Single(rig.Window.SelectiveEditors);

            var page = rig.ShowAndRead();
            Define(page, "12");
            page.RequestWrite();
            var write = rig.Door.VariablesWrites.Single();
            rig.Door.AnswerVariablesWrite(new VariablesDoorWritten(write.RequestId, true, "ok", new[] { RackId }, 1));

            Assert.Same(editor, Assert.Single(rig.Window.SelectiveEditors));
            Assert.True(editor.DrawingChange.IsChanged);
            Assert.Equal(RackEditorDrawingChange.CleanText, editor.DrawingChange.Text);
            Assert.EndsWith("El editor de Selectivo A muestra lo que había antes: pulsa en él «Volver a leer del dibujo».", page.Message);

            // «Volver a leer del dibujo» of the editor: the read of «Editar aquí» (the page reads the drawing again meanwhile: one request at a time).
            rig.Door.AnswerVariablesRead(ReadOf(rig.Door.VariablesReads.Last().RequestId, 12.0));
            rig.Window.OpenRack("SEL-1");
            Assert.True(editor.DrawingChange.CanReread);
            editor.DrawingChange.Reread();
            Assert.Equal(2, rig.Door.SelectiveReads.Count);
        }

        [Fact]
        public void WithoutTheDoorOfThePageTheSectionKeepsItsIntermediateStep()
        {
            var rig = new Rig(withDoor: false);
            rig.Window.ShowPage(RackCadWindowPage.Variables);
            Assert.False(rig.Window.HasVariablesDoor);
            Assert.True(rig.Window.ShowVariablesClassicStep);
            Assert.Null(rig.Window.VariablesPage);
        }

        // ------------------------------------------------------------------ the view (WPF, STA)

        // ------------------------------------------------------------------ T6 rl: RACKVARIABLES and RACKPROPIEDADES open the window

        // RACKVARIABLES (or RVA): the page «Variables», shown as by a click; its first read is the only request, and the command runs it
        // inside itself. Again, with the page read: nothing more is read (as when it is shown by hand).
        [Fact]
        public void RackVariablesOpensThePageAndItsFirstReadIsTheOneTheCommandRunsInsideItself()
        {
            var rig = new Rig();
            rig.Window.ShowPage(RackCadWindowPage.Nuevo);

            var ask = rig.Door.Ask(RackCadShortcutTarget.Variables);
            Assert.True(ask.Taken);
            Assert.True(rig.Window.IsVariablesPage);
            var read = Assert.Single(rig.Door.VariablesReads);
            Assert.True(rig.Window.VariablesPage!.IsReading);
            Assert.Null(ask.Said);
            rig.Door.AnswerVariablesRead(ReadOf(read.RequestId)); // RACKVARIABLESLEER, run by RACKVARIABLES
            rig.Door.Done(ask);
            Assert.True(rig.Window.VariablesPage!.IsLoaded);
            Assert.Equal("RackCad: «Variables» en la ventana RackCad.", RackCadShortcutRules.CommandLineText(ask));

            rig.Window.ShowPage(RackCadWindowPage.Nuevo);
            var again = rig.Door.Ask(RackCadShortcutTarget.Variables);
            rig.Door.Done(again);
            Assert.True(rig.Window.IsVariablesPage);
            Assert.Single(rig.Door.VariablesReads);
            Assert.Empty(rig.Door.SelectiveReads);
        }

        // RACKPROPIEDADES with «Proyecto»: the tab «Propiedades del proyecto» of «Variables».
        [Fact]
        public void RackPropiedadesWithProyectoOpensThePropertiesOfTheProject()
        {
            var rig = new Rig();
            var ask = rig.Door.Ask(RackCadShortcutTarget.ProjectProperties);
            rig.Door.AnswerVariablesRead(ReadOf(rig.Door.VariablesReads.Single().RequestId));
            rig.Door.Done(ask);

            Assert.True(rig.Window.IsVariablesPage);
            Assert.Equal(RackCadWindowViewModel.ProjectPropertiesTabIndex, rig.Window.VariablesPage!.SelectedTab);
            Assert.Equal("RackCad: «Variables» › «Propiedades del proyecto» en la ventana RackCad.", RackCadShortcutRules.CommandLineText(ask));
        }

        // RACKPROPIEDADES with a rack: its page and «Editar aquí», whose read the command runs inside itself; when it answered, the tab
        // «Propiedades» of the editor. With the editor open, the tab at once and no read.
        [Fact]
        public void RackPropiedadesOpensTheTabPropiedadesOfTheEditorOfTheRack()
        {
            var rig = new Rig();
            rig.Window.ShowPage(RackCadWindowPage.Nuevo);

            var ask = rig.Door.Ask(RackCadShortcutTarget.RackProperties, RackId);
            Assert.True(ask.Taken);
            Assert.True(rig.Window.IsRackPage);
            Assert.Equal(RackId, rig.Panel.Browser.ActiveRackId);
            Assert.Single(rig.Door.SelectiveReads); // «Editar aquí»: the read the command runs
            rig.Door.AnswerLastSelectiveRead(bound: false);
            rig.Door.Done(ask);
            var editor = rig.Window.ActiveSelectiveEditor!;
            Assert.Equal(SelectiveEditorViewModel.PropertiesTabIndex, editor.SelectedTab);
            Assert.Null(ask.Said);
            Assert.Equal("RackCad: «Propiedades» del rack en la ventana RackCad.", RackCadShortcutRules.CommandLineText(ask));

            editor.SelectedTab = 0;
            rig.Window.ShowPage(RackCadWindowPage.Nuevo);
            var again = rig.Door.Ask(RackCadShortcutTarget.RackProperties, RackId);
            rig.Door.Done(again);
            Assert.True(rig.Window.IsRackPage);
            Assert.Equal(SelectiveEditorViewModel.PropertiesTabIndex, editor.SelectedTab);
            Assert.Single(rig.Door.SelectiveReads);
        }

        // What the window says when it cannot: a rack the list does not have, another request waiting, a read refused.
        [Fact]
        public void RackPropiedadesSaysWhyWhenTheRackCannotBeOpened()
        {
            var rig = new Rig();
            var missing = rig.Door.Ask(RackCadShortcutTarget.RackProperties, "NO-ESTA");
            Assert.Equal(RackCadShortcutRules.NotInTheListText, missing.Said);
            Assert.Empty(rig.Door.SelectiveReads);

            rig.Window.ShowPage(RackCadWindowPage.Variables); // its read waits
            var busy = rig.Door.Ask(RackCadShortcutTarget.RackProperties, RackId);
            Assert.Equal(RackCadShortcutRules.BusyText, busy.Said);
            Assert.Empty(rig.Door.SelectiveReads);
            rig.Door.AnswerVariablesRead(ReadOf(rig.Door.VariablesReads.Single().RequestId));

            var refused = rig.Door.Ask(RackCadShortcutTarget.RackProperties, RackId);
            rig.Door.SelectiveReadAnswer(SelectiveDesignRead.Refused(rig.Door.SelectiveReads.Single().RequestId, RackId, null, null, "El rack no está en el dibujo."));
            rig.Door.Done(refused);
            Assert.Null(rig.Window.ActiveSelectiveEditor);
            // T6rl-S14 (correction 1 of the review): a rack whose editor does not open is one the classic window still serves: said in the
            // command line and in the window.
            Assert.Equal("no se abrió el editor del rack. No se pudo abrir el rack: El rack no está en el dibujo. " + RackCadShortcutRules.ClassicPropertiesText, refused.Said);
            Assert.Equal("No se pudo abrir el rack: El rack no está en el dibujo. " + RackCadShortcutRules.ClassicPropertiesText, rig.Window.ActionMessage);
            Assert.True(rig.Window.ActionFailed);
            Assert.Equal("Usa «Propiedades personalizadas (ventana clásica)» del menú clásico (RACKMENU).", RackCadShortcutRules.ClassicPropertiesText);
        }

        // Correction 4 of the review: «Variables» is shown and not read (its read was refused). The Refresh of the ask reads nothing, so the
        // one read of RACKPROPIEDADES is the one of «Editar aquí» (before, «Variables» took it and the rack said «la ventana espera…»), and
        // RACKVARIABLES with the page already shown reads it once.
        [Fact]
        public void AnUnreadPageOfVariablesTakesNoReadWhileACommandAsks()
        {
            var rig = new Rig();
            rig.Door.Answer = RackCadClassicResult.Without(RackCadClassicStatus.Busy, "x");
            rig.Window.ShowPage(RackCadWindowPage.Variables);
            rig.Door.Answer = RackCadClassicResult.Started();
            Assert.Single(rig.Door.VariablesReads); // the refused one of the click
            Assert.True(rig.Window.VariablesPage!.IsNotLoaded);

            var ask = rig.Door.Ask(RackCadShortcutTarget.RackProperties, RackId);
            Assert.Null(ask.Said);
            Assert.Single(rig.Door.VariablesReads);
            Assert.Single(rig.Door.SelectiveReads);
            rig.Door.AnswerLastSelectiveRead(bound: false);
            rig.Door.Done(ask);
            Assert.Equal(SelectiveEditorViewModel.PropertiesTabIndex, rig.Window.ActiveSelectiveEditor!.SelectedTab);

            rig.Window.ShowPage(RackCadWindowPage.Variables); // shown by hand: it reads (the window was free)
            Assert.Equal(2, rig.Door.VariablesReads.Count);
            rig.Door.AnswerVariablesRead(VariablesDoorRead.Refused(rig.Door.VariablesReads.Last().RequestId, "x"));
            Assert.True(rig.Window.VariablesPage!.IsNotLoaded);
            var variables = rig.Door.Ask(RackCadShortcutTarget.Variables); // the page already shown: read once, by the command
            Assert.Null(variables.Said);
            Assert.Equal(3, rig.Door.VariablesReads.Count);
        }

        // A window whose door does not take these commands (the doubles before T6): nothing is taken, and the command says it.
        [Fact]
        public void WithoutTheDoorTheShortcutIsNotTaken()
        {
            var rig = new Rig(withDoor: false);
            var ask = rig.Door.Ask(RackCadShortcutTarget.Variables);
            Assert.False(ask.Taken);
            Assert.Equal("RackCad: " + RackCadShortcutRules.NotOpenedText, RackCadShortcutRules.CommandLineText(ask));
        }

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

        private static void Lay(FrameworkElement view, double width = 1200, double height = 900)
        {
            view.Measure(new Size(width, height));
            view.Arrange(new Rect(0, 0, width, height));
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

        private static void Click(Button button) => button.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));

        [Fact]
        public void TheViewIsBuiltWithItsTwoTabsWithNoBindingErrorAndItsButtonsReachTheDraft()
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
                    var page = Page();
                    page.AddProperty();
                    var view = new VariablesPageView(page);
                    Lay(view);

                    Assert.Equal(new[] { "Variables de proyecto", "Propiedades del proyecto" },
                        view.Tabs.Items.Cast<TabItem>().Select(t => System.Windows.Automation.AutomationProperties.GetName(t)).ToArray());
                    var rowButton = Descendants<Button>(view.VariableRows).Single();
                    Assert.Equal("Variable 1: Holgura estándar", System.Windows.Automation.AutomationProperties.GetName(rowButton));
                    // Smoke T4 (F-3, seen in AutoCAD): the row fills the table and its name starts under «Nombre», not centred.
                    var rowName = Descendants<TextBlock>(rowButton).First(t => t.Text == "Holgura estándar");
                    var heading = Descendants<TextBlock>(view).First(t => t.Text == "Nombre");
                    var rowX = rowName.TranslatePoint(new Point(0, 0), view).X;
                    var headingX = heading.TranslatePoint(new Point(0, 0), view).X;
                    Assert.True(Math.Abs(rowX - headingX) < 12, "nombre de la fila en x=" + rowX + ", encabezado en x=" + headingX);
                    Assert.True(rowButton.ActualWidth > view.VariableRows.ActualWidth - 4, "ancho de la fila " + rowButton.ActualWidth + " de " + view.VariableRows.ActualWidth);
                    Assert.False(view.DeleteButton.IsEnabled);
                    Assert.Equal(Visibility.Visible, view.UnlinkButton.Visibility);

                    // The inline question of «Desvincular todos y eliminar…», from its button.
                    Click(view.UnlinkButton);
                    Lay(view);
                    Assert.Equal(Visibility.Visible, view.UnlinkAskPanel.Visibility);
                    Click(view.UnlinkYesButton);
                    Assert.Equal("Se eliminará", page.Rows[0].Tag);

                    view.Tabs.SelectedItem = view.PropertiesTab;
                    Lay(view);
                    Assert.Contains(Descendants<TextBox>(view.ProjectPropertiesTable), b => System.Windows.Automation.AutomationProperties.GetName(b) == "Valor de la propiedad 1"
                                                                                       && b.AcceptsReturn);

                    // Narrow: the inspector goes below the list.
                    view.PlaceInspector(true);
                    Assert.True(view.InspectorBelow);
                    Assert.Equal(1, Grid.GetRow(view.InspectorPanel));
                    Assert.Empty(errors.Messages);
                }
                finally
                {
                    source.Listeners.Remove(errors);
                    source.Switch.Level = level;
                }
            });
        }

        [Fact]
        public void TheWindowHostsThePageOfItsDrawing()
        {
            StaTestRunner.Run(() =>
            {
                var rig = new Rig();
                var view = new RackCadWindowView(rig.Window);
                rig.ShowAndRead();
                Lay(view);

                Assert.NotNull(view.VariablesView);
                Assert.Same(rig.Window.VariablesPage, view.VariablesView!.ViewModel);
                Assert.Equal(Visibility.Visible, view.VariablesPageHost.Visibility);
                Assert.Equal(Visibility.Collapsed, view.VariablesPage.Visibility);
            });
        }
    }
}
