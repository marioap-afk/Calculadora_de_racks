#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using RackCad.Application.Persistence;
using RackCad.Application.VentanaRackCad;
using RackCad.Application.Workspace;
using RackCad.Domain.Systems.FlowBed;
using RackCad.Domain.Systems.Shared;
using RackCad.UI.VentanaRackCad;
using RackCad.UI.Workspace;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64, quinta tanda, part cm (prototype «camas»): the camas de rodamiento in the RackCad window, over a door that reads and writes a cama
    /// (<see cref="IRackCadCamaDoorPort"/>), with the rules of the other doors: «Editar aquí» of a cama of the list, numbered requests, an
    /// answer only for its request, «Dejar de esperar», the request the command took, copies of the bed, the editor locked while AutoCAD
    /// writes, a draft of another drawing, the prune of a closed drawing and the veto of the close (D-08); «Cama en esta ventana» as a draft
    /// of «Nuevo» (SD-06) inserted with RACKCAMANUEVO; the library (open as a draft, save the editor) and the BOM of the draft.
    /// </summary>
    public class RackCadCamaWindowTests
    {
        private static readonly SessionId Session = new(7);

        private sealed class CamaDoorDouble : IRackCadCamaDoorPort
        {
            public List<RackDoorRequest> CamaReads { get; } = new();

            public List<CamaDoorWrite> CamaWrites { get; } = new();

            public List<long> Abandoned { get; } = new();

            public RackCadClassicResult Answer { get; set; } = RackCadClassicResult.Started();

            public event Action<RackDesignRead>? DesignRead;

            public event Action<RackDesignWritten>? DesignWritten;

            public event Action<long>? RequestStarted;

            public event Action<CamaDesignRead>? CamaRead;

            public RackCadClassicResult RequestDesign(RackDoorRequest request) => Answer;

            public RackCadClassicResult RequestDynamicUpdate(RackDoorWrite write) => Answer;

            public RackCadClassicResult RequestCamaDesign(RackDoorRequest request)
            {
                CamaReads.Add(request);
                return Answer;
            }

            public RackCadClassicResult RequestCamaWrite(CamaDoorWrite write)
            {
                CamaWrites.Add(write);
                return Answer;
            }

            public void Abandon(long requestId) => Abandoned.Add(requestId);

            public void Read(RackDesignRead read) => DesignRead?.Invoke(read);

            public void ReadCama(CamaDesignRead read) => CamaRead?.Invoke(read);

            public void Written(RackDesignWritten written) => DesignWritten?.Invoke(written);

            public void Start(long requestId) => RequestStarted?.Invoke(requestId);

            // What RACKCAMAABRIR answers to the last read: the cama of the drawing, through the rule of the door.
            public void AnswerLastRead(string? design = null, string name = "Cama 1")
            {
                var request = CamaReads.Last();
                ReadCama(CamaDoorRules.AnswerForOpen(request.RequestId, request.RackId, RackEmbedDocument.KindCama, name, null, -1, design ?? Design(Bed())));
            }

            public void AnswerLastWrite(bool written, string message, string? name = null)
            {
                var write = CamaWrites.Last();
                Written(new RackDesignWritten(write.RequestId, write.RackId, written, message, name));
            }
        }

        private sealed class SettingsDouble : IRackCadSettingsAccess, IRackCadUtilitySettings
        {
            public SettingsDouble(string folder) => Folder = folder;

            public string Folder { get; }

            public string BlockLibraryPath => "C:\\bloques.dwg";

            public bool BlockLibraryOverridden => false;

            public bool BlockLibraryExists => true;

            public string DesignLibraryPath => Folder;

            public void SetBlockLibraryPath(string? path)
            {
            }

            public bool DesignLibraryOverridden => false;

            public void SetDesignLibraryPath(string? path)
            {
            }

            public bool OpenOnStartup => false;

            public void SetOpenOnStartup(bool open)
            {
            }

            public string SettingsFilePath => "C:\\settings.json";

            public RackCadAboutInfo About => new("1.0.0", "C:\\catalogs", "C:\\logs");
        }

        private static FlowBedConfiguration Bed(double lane = 96.0) => new()
        {
            BedType = FlowBedType.Dynamic,
            LaneDepth = lane,
            PalletDepth = 48.0,
            RollerId = FlowBedDefaults.RollerId,
        };

        private static string Design(FlowBedConfiguration bed) => new FlowBedConfigurationStore().Serialize(bed);

        private sealed class Rig : IDisposable
        {
            public Rig(IRackCadRackDoorPort? door = null)
            {
                Root = Path.Combine(Path.GetTempPath(), "RackCadCamaTest_" + Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(Root);
                Door = door as CamaDoorDouble ?? new CamaDoorDouble();
                Host = new WorkspacePortDouble { Snapshot = WorkspacePanelFixtures.Ready(session: 7) };
                Summary = new WorkspaceBrowserSummaryPortDouble { ListResponder = (_, _) => BrowserRackList.Ready(Listed) };
                Panel = new WorkspacePanelViewModel(Host, _ => { }, new WorkspaceNavigationPortDouble(), Summary, new WorkspacePilotEditPortDouble());
                Window = new RackCadWindowViewModel(Panel, null, new SettingsDouble(Root), door ?? Door, UiSupport.LoadCatalogSafe);
                Window.Refresh();
            }

            public string Root { get; }

            public List<BrowserRackListEntry> Listed { get; } = new()
            {
                new BrowserRackListEntry("CAMA-1", "Cama 1", RackEmbedDocument.KindCama, new[] { "lateral" }),
                new BrowserRackListEntry("DIN-1", "DIN 1", RackEmbedDocument.KindDynamic, new[] { "lateral" }),
            };

            public WorkspacePortDouble Host { get; }

            public WorkspaceBrowserSummaryPortDouble Summary { get; }

            public WorkspacePanelViewModel Panel { get; }

            public CamaDoorDouble Door { get; }

            public RackCadWindowViewModel Window { get; }

            public void Present(long session)
            {
                Host.Snapshot = WorkspacePanelFixtures.Ready(session: session);
                Window.Refresh();
            }

            public CamaEditorViewModel OpenEditor(string? design = null)
            {
                Window.OpenRack("CAMA-1");
                Window.EditCamaHere();
                Door.AnswerLastRead(design);
                Assert.True(Window.ActiveCamaEditor != null, Window.ActionMessage);
                return Window.ActiveCamaEditor!;
            }

            public void Dispose()
            {
                try
                {
                    Directory.Delete(Root, true);
                }
                catch (IOException)
                {
                }
                catch (UnauthorizedAccessException)
                {
                }
            }
        }

        /// <summary>A door of Dinámico only: it cannot read a cama, so the window does not offer to edit one, nor to make a new one.</summary>
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

        private static void Write(CamaFieldItem field, string text)
        {
            field.Text = text;
            field.Commit();
        }

        // ------------------------------------------------------------------ a cama of the drawing

        [Fact]
        public void ACamaOffersToBeEditedHereOnlyWithItsDoorAndOnlyTheAnswerToThatRequestOpensTheEditor()
        {
            using var rig = new Rig();
            rig.Window.OpenRack("CAMA-1");
            Assert.True(rig.Window.IsCamaRack);
            Assert.True(rig.Window.ShowCamaOffer);
            Assert.False(rig.Window.ShowDynamicOffer);

            rig.Window.EditCamaHere();
            var request = Assert.Single(rig.Door.CamaReads);
            Assert.Equal(("CAMA-1", Session), (request.RackId, request.Session));
            Assert.True(rig.Window.IsWaitingForAutoCad);

            // Another number, or the read of Dinámico with the same number, is not for this request.
            rig.Door.ReadCama(CamaDoorRules.AnswerForOpen(request.RequestId + 9, "CAMA-1", RackEmbedDocument.KindCama, "Cama 1", null, -1, Design(Bed())));
            rig.Door.Read(new RackDesignRead(request.RequestId, "CAMA-1", true, "dynamic", "X", "{}", null));
            Assert.Null(rig.Window.ActiveCamaEditor);

            rig.Door.AnswerLastRead();

            var editor = Assert.IsType<CamaEditorViewModel>(rig.Window.ActiveCamaEditor);
            Assert.False(rig.Window.IsWaitingForAutoCad);
            Assert.True(rig.Window.HasRackEditor);
            Assert.False(rig.Window.ShowCamaOffer);
            Assert.False(rig.Window.ShowPanel);
            Assert.Equal(("Cama 1", "Limpio", false), (editor.Name, editor.StatusText, editor.IsNewRack));
            Assert.Equal("Abierto en la ventana", rig.Window.RackList.Groups.SelectMany(g => g.Items).Single(i => i.RackId == "CAMA-1").StatusText);

            // A refusal of the door is said, and nothing opens.
            using var refused = new Rig();
            refused.Window.OpenRack("CAMA-1");
            refused.Window.EditCamaHere();
            var r = refused.Door.CamaReads.Last();
            refused.Door.ReadCama(CamaDesignRead.Refused(r.RequestId, "CAMA-1", null, null, "La cama no está en el dibujo."));
            Assert.Null(refused.Window.ActiveCamaEditor);
            Assert.Equal("No se pudo abrir la cama: La cama no está en el dibujo.", refused.Window.ActionMessage);

            using var dynamicOnly = new Rig(new DynamicOnlyDoor());
            dynamicOnly.Window.OpenRack("CAMA-1");
            Assert.False(dynamicOnly.Window.ShowCamaOffer);
            Assert.False(dynamicOnly.Window.CanStartNewCama);
            dynamicOnly.Window.EditCamaHere();
            Assert.False(dynamicOnly.Window.IsWaitingForAutoCad);
        }

        [Fact]
        public void ActualizarSendsACopyLocksTheEditorAndOnlyAWrittenAnswerLeavesItClean()
        {
            using var rig = new Rig();
            var editor = rig.OpenEditor();
            Write(editor.LaneField, "120");
            Assert.Equal("Modificado", editor.StatusText);
            Assert.Equal("Cama 1", rig.Window.UnwrittenWorkOf(Session)); // D-08
            Assert.Equal("Modificado (editor de la ventana)", rig.Window.RackList.Groups.SelectMany(g => g.Items).Single(i => i.RackId == "CAMA-1").StatusText);

            editor.RequestWrite();

            var write = Assert.Single(rig.Door.CamaWrites);
            Assert.Equal(("CAMA-1", Session, "Cama 1", false), (write.RackId, write.Session, write.Name, write.NewRack));
            Assert.Equal(120.0, write.Bed.LaneDepth);
            write.Bed.LaneDepth = 5.0; // the copy is the door's: the draft does not move
            Assert.Equal(120.0, editor.Draft.LaneDepth);
            Assert.True(editor.IsWriting);
            Assert.True(rig.Window.CanStopWaiting);

            // The command took it: «Dejar de esperar» is no longer offered.
            rig.Door.Start(write.RequestId);
            Assert.False(rig.Window.CanStopWaiting);
            Assert.StartsWith("AutoCAD ya tomó la escritura de la cama", editor.PrimaryBlockedText, StringComparison.Ordinal);

            rig.Door.AnswerLastWrite(false, "AutoCAD no redibujó la cama, así que el borrador sigue sin escribir.");
            Assert.False(editor.IsWriting);
            Assert.Equal("Modificado", editor.StatusText);
            Assert.True(editor.MessageIsError);

            editor.RequestWrite();
            rig.Door.AnswerLastWrite(true, "AutoCAD redibujó la cama con el borrador: todas sus copias en el dibujo reflejan el cambio.");
            Assert.Equal("Limpio", editor.StatusText);
            Assert.Null(rig.Window.UnwrittenWorkOf(Session));
            Assert.Equal("120", editor.LaneField.Text);
        }

        [Fact]
        public void DejarDeEsperarLeavesTheCamaEditableAndALateAnswerChangesNothing()
        {
            using var rig = new Rig();
            var editor = rig.OpenEditor();
            Write(editor.LaneField, "100");
            editor.RequestWrite();
            var write = rig.Door.CamaWrites.Last();

            rig.Window.StopWaiting();

            Assert.Equal(new[] { write.RequestId }, rig.Door.Abandoned.ToArray());
            Assert.False(editor.IsWriting);
            Assert.True(editor.CanEdit);
            Assert.Equal("Modificado", editor.StatusText);

            rig.Door.Written(new RackDesignWritten(write.RequestId, "CAMA-1", true, "tarde"));
            Assert.Equal("Modificado", editor.StatusText);
        }

        [Fact]
        public void ADraftOfAnotherDrawingIsNotWrittenInTheActiveOneAndAClosedDrawingForgetsIt()
        {
            using var rig = new Rig();
            var editor = rig.OpenEditor();
            Write(editor.LaneField, "100");

            rig.Present(8);
            editor.RequestWrite();
            Assert.Empty(rig.Door.CamaWrites);
            Assert.Equal("Este borrador es de otro dibujo: vuelve a ese dibujo para escribirlo.", editor.Message);

            rig.Summary.Dead.Add(Session);
            rig.Present(8);
            Assert.Empty(rig.Window.CamaEditors);
            Assert.Equal("Se cerró el dibujo de Cama 1 con cambios sin escribir en la ventana: esos cambios se perdieron.", rig.Window.ActionMessage);
        }

        [Fact]
        public void AClosedDrawingNamesEveryDraftItLostInOneLine()
        {
            // Point 1 of the review: the prune of the cama said its own line and the prune of Dinámico, after it, overwrote it.
            using var rig = new Rig();
            var cama = rig.OpenEditor();
            Write(cama.LaneField, "100");
            rig.Window.StartNewDynamic();
            rig.Window.NewDynamicEditor!.Name = "Din X";

            rig.Summary.Dead.Add(Session);
            rig.Present(8);

            Assert.Empty(rig.Window.CamaEditors);
            Assert.Empty(rig.Window.DynamicEditors);
            Assert.Equal("Se cerró el dibujo de Cama 1, Din X (Borrador 1, Dinámico nuevo) con cambios sin escribir en la ventana: esos cambios se perdieron.",
                rig.Window.ActionMessage);
            Assert.True(rig.Window.ActionFailed);
        }

        [Fact]
        public void PushbackDinamicaPushbackDoesNotVetoTheClose()
        {
            // Point 2 of the review: back to Pushback with the 48 in nobody wrote, the cama has nothing to lose (D-08).
            using var rig = new Rig();
            var bed = Bed();
            bed.BedType = FlowBedType.Pushback;
            bed.PalletDepth = 0.0;
            var editor = rig.OpenEditor(Design(bed));

            editor.TypeField.SelectedValue = "Dynamic";
            Assert.Equal("Cama 1", rig.Window.UnwrittenWorkOf(Session));
            editor.TypeField.SelectedValue = "Pushback";
            Assert.Null(rig.Window.UnwrittenWorkOf(Session));
            Assert.Equal("Abierto en la ventana", rig.Window.RackList.Groups.SelectMany(g => g.Items).Single(i => i.RackId == "CAMA-1").StatusText);
        }

        [Fact]
        public void CerrarElEditorAsksWithChangesAndForgetsTheDraft()
        {
            using var rig = new Rig();
            var editor = rig.OpenEditor();
            Write(editor.LaneField, "100");

            editor.RequestClose();
            Assert.True(editor.IsConfirmingClose);
            Assert.NotNull(rig.Window.ActiveCamaEditor);
            editor.ConfirmClose();

            Assert.Null(rig.Window.ActiveCamaEditor);
            Assert.False(rig.Window.HasRackEditor);
            Assert.True(rig.Window.ShowCamaOffer);
            Assert.Null(rig.Window.UnwrittenWorkOf(Session));
        }

        [Fact]
        public void ACamaReadWithAPitchTheWindowDoesNotAcceptOpensMarkedAndIsNotWrittenUntilCorrected()
        {
            using var rig = new Rig();
            var bed = Bed();
            bed.RollerPitchOverride = 1.0; // smaller than the roller of 1.9 in
            var editor = rig.OpenEditor(Design(bed));

            Assert.Equal("El paso no puede ser menor que el rodillo: mínimo 2 in.", editor.PitchField.Error);
            Write(editor.LaneField, "100");
            editor.RequestWrite();
            Assert.Empty(rig.Door.CamaWrites);
            Assert.Equal("Corrige el valor marcado para actualizar.", editor.Message);

            Write(editor.PitchField, "2");
            editor.RequestWrite();
            Assert.Equal(2.0, Assert.Single(rig.Door.CamaWrites).Bed.RollerPitchOverride);
        }

        // ------------------------------------------------------------------ a new cama of «Nuevo»

        [Fact]
        public void DiscardingADraftOfNuevoSaysThatItGoesBackToHowItOpened() // T6 ui, point 10 (decided by the Owner on 2026-11-27)
        {
            using var rig = new Rig();
            rig.Window.StartNewCama();
            var editor = Assert.IsType<CamaEditorViewModel>(rig.Window.NewCamaEditor);
            Write(editor.LaneField, "110");
            Assert.True(editor.IsDirty);

            editor.Discard();

            Assert.False(editor.IsDirty);
            Assert.Equal("Cambios descartados: el borrador vuelve a como se abrió.", editor.Message);
        }

        [Fact]
        public void CamaEnEstaVentanaIsADraftOfNuevoThatIsInsertedWithItsOwnCommandAndThenIsACamaOfTheDrawing()
        {
            using var rig = new Rig();
            Assert.True(rig.Window.CanStartNewCama);
            rig.Window.StartNewCama();

            var item = Assert.Single(rig.Window.Drafts);
            Assert.Equal((RackSystemKind.Cama, "Borrador 1 · Cama", "Sin insertar"), (item.Kind, item.ColumnText, item.StatusText));
            var editor = Assert.IsType<CamaEditorViewModel>(rig.Window.NewCamaEditor);
            Assert.True(rig.Window.ShowNewCamaEditor);
            Assert.False(rig.Window.ShowNuevoCards);
            Assert.True(editor.IsNewRack);
            Assert.Equal("Insertar en el dibujo…", editor.PrimaryLabel);
            Assert.Contains("1 borrador sin insertar", rig.Window.HeaderText);
            Assert.Null(rig.Window.UnwrittenWorkOf(Session)); // a draft with nothing changed (ND-05)

            Write(editor.LaneField, "110");
            Assert.Equal("Borrador 1 (Cama nueva)", rig.Window.UnwrittenWorkOf(Session)); // D-08, by its label
            Assert.Equal("Sin insertar, con cambios", item.StatusText);

            // Not placed: the draft stays in «Nuevo», with what AutoCAD said.
            editor.RequestWrite();
            var first = Assert.Single(rig.Door.CamaWrites);
            Assert.True(first.NewRack);
            Assert.Equal(item.RackId, first.RackId);
            Assert.Equal(string.Empty, first.Name); // «Borrador N» is never sent: the drawing gives the automatic name (I-60)
            Assert.Null(first.Properties);
            rig.Door.AnswerLastWrite(false, "AutoCAD no insertó la cama nueva: se canceló la colocación o no se pudo preparar.");
            Assert.True(editor.IsNewRack);
            Assert.Single(rig.Window.Drafts);

            editor.RequestWrite();
            rig.Door.AnswerLastWrite(true, "AutoCAD insertó la cama nueva «Cama 3». Para seguir editándola aquí, vuelve a leer la lista y ábrela.", "Cama 3");

            Assert.False(editor.IsNewRack);
            Assert.Equal("Cama 3", editor.Name);
            Assert.Equal("Limpio", editor.StatusText);
            Assert.Empty(rig.Window.Drafts);
            Assert.True(rig.Window.ShowNuevoCards);
            Assert.Equal("AutoCAD insertó la cama nueva «Cama 3». Para seguir editándola aquí, vuelve a leer la lista y ábrela.", rig.Window.ActionMessage);

            // Read again, the list has it, and opening it shows the same editor, with «Actualizar» (D-10).
            rig.Listed.Add(new BrowserRackListEntry(item.RackId, "Cama 3", RackEmbedDocument.KindCama, new[] { "lateral" }));
            rig.Window.RackList.Reload();
            rig.Window.OpenRack(item.RackId);
            Assert.Same(editor, rig.Window.ActiveCamaEditor);
            Assert.Equal("Actualizar", editor.PrimaryLabel);
        }

        [Fact]
        public void DuplicarADraftOfACamaKeepsTheBaseOfItsOriginalAndDescartarElBorradorAsksWithChanges()
        {
            using var rig = new Rig();
            rig.Window.StartNewCama();
            var original = rig.Window.NewCamaEditor!;
            Write(original.LaneField, "130");
            var item = rig.Window.Drafts.Single();

            var copy = rig.Window.DuplicateDraft(item);
            Assert.True(copy.Opened, copy.Reason);
            var copied = rig.Window.NewCamaEditor!;
            Assert.NotSame(original, copied);
            Assert.Equal(130.0, copied.Draft.LaneDepth);
            Assert.Equal("Modificado", copied.StatusText); // its base is the one of its original: the defaults
            Assert.Equal(string.Empty, copied.Name);
            Assert.Equal(new[] { 1, 2 }, rig.Window.Drafts.Select(d => d.Number).ToArray());

            rig.Window.DiscardDraft(copy.Draft);
            Assert.True(copy.Draft.IsConfirmingDiscard);
            rig.Window.ConfirmDiscardDraft(copy.Draft);
            Assert.Equal("Se descartó Borrador 2 (Cama nueva): no estaba en el dibujo.", rig.Window.ActionMessage);
            Assert.Single(rig.Window.Drafts);
        }

        // ------------------------------------------------------------------ the library and the BOM

        [Fact]
        public void ACamaOfTheLibraryOpensAsADraftAndTheEditorIsSavedKeepingTheUnknownFieldsOfItsSource()
        {
            using var rig = new Rig();
            // A cama of the drawing whose document carries a field this build does not know (I-11).
            var editor = rig.OpenEditor(Design(Bed(120.0)).TrimEnd('}') + ",\"Futuro\":7}");
            Write(editor.BrakesField, "6");

            editor.SaveToLibrary();
            Assert.True(rig.Window.IsBibliotecaPage);
            Assert.True(rig.Window.Library.HasSave);
            rig.Window.Library.SaveName = "Cama guardada";
            rig.Window.Library.Save();

            var path = Path.Combine(rig.Root, "Cama guardada" + RackProjectStore.FileExtension);
            Assert.True(File.Exists(path), rig.Window.ActionMessage);
            var saved = new RackProjectStore().Load(path);
            Assert.Equal(RackSystemKind.Cama, saved.Kind);
            Assert.Equal((120.0, (int?)6), (saved.FlowBed.LaneDepth, saved.FlowBed.BrakeEveryRollers));
            Assert.Matches(@"""Futuro""\s*:\s*7", File.ReadAllText(path)); // the library writes it indented

            // «Abrir» of the library: a cama opens here, as a draft of «Nuevo» named as the classic names it (the name of the entry).
            rig.Window.Library.Read();
            var row = rig.Window.Library.Rows.Single(r => r.Path == path);
            rig.Window.Library.SelectPath(row.Path);
            Assert.True(rig.Window.Library.SelectionOpensHere);
            rig.Window.Library.OpenSelected();

            var draft = Assert.IsType<CamaEditorViewModel>(rig.Window.NewCamaEditor);
            Assert.True(draft.IsNewRack);
            Assert.Equal("Cama guardada", draft.Name);
            Assert.Equal((120.0, (int?)6), (draft.Draft.LaneDepth, draft.Draft.BrakeEveryRollers));
            Assert.Equal("Borrador 1: Cama nueva en esta ventana, del diseño «Cama guardada» de la biblioteca. Todavía no está en el dibujo.", draft.Message);

            // Inserted, it carries the document of the library (and its unknown fields) to the drawing.
            draft.RequestWrite();
            var write = rig.Door.CamaWrites.Last();
            Assert.True(write.NewRack);
            Assert.Equal("Cama guardada", write.Name);
            Assert.Contains("\"Futuro\":7", write.SourceDesignJson);
        }

        [Fact]
        public void ThePageOfTheBomShowsTheBomOfTheDraftOfACamaWithChanges()
        {
            using var rig = new Rig();
            var editor = rig.OpenEditor();
            var host = (IRackBomPageHost)rig.Window;
            Assert.False(host.HasUnwrittenDraft(Session, "CAMA-1"));
            Assert.Null(host.DraftBomOf(Session, "CAMA-1"));

            Write(editor.BrakesField, "10");
            Assert.True(host.HasUnwrittenDraft(Session, "CAMA-1"));
            var bom = host.DraftBomOf(Session, "CAMA-1")!;
            Assert.Equal(editor.Layout!.Brakes.Count, bom.Lines.Single(l => l.Category == "Freno").Quantity);
            Assert.Equal(new[] { ("CAMA-1", "Cama 1") }, host.UnwrittenDraftsOf(Session).ToArray());
            Assert.Equal(10, editor.Draft.BrakeEveryRollers); // seeing it does not change the draft

            // Point 4 of the review (CM-S5): with a value marked there is no BOM of the draft; the page shows the one of the drawing.
            Write(editor.LaneField, "0");
            Assert.True(host.HasUnwrittenDraft(Session, "CAMA-1"));
            Assert.Null(host.DraftBomOf(Session, "CAMA-1"));
            Write(editor.LaneField, "96");
            Assert.NotNull(host.DraftBomOf(Session, "CAMA-1"));

            // A new cama is not in the drawing: the page does not show it (BM-S7).
            rig.Window.StartNewCama();
            Write(rig.Window.NewCamaEditor!.LaneField, "50");
            Assert.Single(host.UnwrittenDraftsOf(Session));
        }

        // ------------------------------------------------------------------ the views (WPF, STA)

        [Fact]
        public void ThePageOfTheRackHostsTheEditorOfTheCamaAndNuevoHostsANewOne()
        {
            StaTestRunner.Run(() =>
            {
                using var rig = new Rig();
                var view = new RackCadWindowView(rig.Window);
                rig.Window.OpenRack("CAMA-1");
                Layout(view);
                Assert.Equal(Visibility.Visible, view.CamaOfferPanel.Visibility);
                var offerButton = (Button)view.CamaOfferPanel.FindName("EditCamaHereButton");
                Assert.Equal("Editar la cama en la ventana", System.Windows.Automation.AutomationProperties.GetName(offerButton));

                offerButton.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
                rig.Door.AnswerLastRead();
                Layout(view);

                var hosted = Assert.IsType<CamaEditorView>(view.RackPageHost.Content);
                Assert.Same(rig.Window.ActiveCamaEditor, hosted.ViewModel);
                Assert.Equal(1, view.CamaViewCount);
                Assert.Equal(Visibility.Collapsed, view.CamaOfferPanel.Visibility);

                rig.Window.ActiveCamaEditor!.RequestClose();
                Layout(view);
                Assert.Same(view.PanelView, view.RackPageHost.Content);
                Assert.Equal(0, view.CamaViewCount);

                // «Nuevo» › «Cama en esta ventana».
                rig.Window.ShowPage(RackCadWindowPage.Nuevo);
                Layout(view);
                var newButton = (Button)view.NewCamaHereBlock.FindName("NewCamaHereButton");
                Assert.True(newButton.IsEnabled);
                Assert.Equal("Nueva cama en esta ventana", System.Windows.Automation.AutomationProperties.GetName(newButton));
                newButton.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
                Layout(view);
                var newView = Assert.IsType<CamaEditorView>(view.NewCamaRackHost.Content);
                Assert.Same(rig.Window.NewCamaEditor, newView.ViewModel);
                Assert.Equal(Visibility.Visible, view.NewCamaRackHost.Visibility);
                Assert.Equal(Visibility.Collapsed, view.NuevoPage.Visibility);
            });
        }

        [Fact]
        public void TheEditorShowsItsOnePageWithTheSchemeAndNoTextIsTheNameOfAType()
        {
            StaTestRunner.Run(() =>
            {
                using var rig = new Rig();
                var editor = rig.OpenEditor();
                var view = new CamaEditorView(editor);
                Layout(view);

                Assert.Equal(new[] { "Cama", "Propiedades" }, view.Tabs.Items.Cast<TabItem>().Select(t => t.Header?.ToString()).ToArray());
                Assert.Same(editor.Layout, view.SchemaStrip.Layout);
                Assert.Equal(editor.SchemaText, System.Windows.Automation.AutomationProperties.GetName(view.SchemaStrip));
                Assert.Equal(12.0, view.SchemaStrip.TickStep);
                Assert.True(view.SchemaStrip.ActualWidth > 100);
                Assert.True(view.SchemaStrip.XOf(96.0) > view.SchemaStrip.XOf(0.0));
                Assert.Equal("Actualizar", view.PrimaryButton.Content);
                Assert.False(view.PrimaryButton.IsEnabled);

                // F-1: no text shows the name of a type; every button has a name of its own.
                var texts = Descendants<TextBlock>(view).Select(t => t.Text ?? string.Empty).ToList();
                Assert.DoesNotContain(texts, t => t.Contains("RackCad.", StringComparison.Ordinal));
                Assert.Contains("Profundidad de la cama", texts);
                Assert.Contains("Calculado", texts);
                var names = Descendants<Button>(view).Select(b => System.Windows.Automation.AutomationProperties.GetName(b)).Where(n => !string.IsNullOrEmpty(n)).ToList();
                Assert.Equal(names.Count, names.Distinct().Count());

                // A value written in its box goes to the draft when the box loses the focus (Enter does the same).
                var lane = Descendants<TextBox>(view).Single(t => System.Windows.Automation.AutomationProperties.GetName(t) == "Profundidad de la cama");
                lane.Text = "0";
                lane.RaiseEvent(new RoutedEventArgs(UIElement.LostFocusEvent));
                Layout(view);
                Assert.Equal(CamaDraftRules.LaneDepthText, editor.LaneField.Error);
                Assert.Equal(Visibility.Visible, view.SchemaEmptyText.Visibility);
                Assert.Equal("Corrige el valor marcado para actualizar.", view.PrimaryBlockedText.Text);

                lane.Text = "120";
                lane.RaiseEvent(new RoutedEventArgs(UIElement.LostFocusEvent));
                Write(editor.PitchField, "4");
                Layout(view);
                Assert.True(view.PrimaryButton.IsEnabled);
                Assert.Equal(Visibility.Collapsed, view.SchemaEmptyText.Visibility);

                // «Restaurar layout…» opens inline with its two boxes.
                view.RestoreButton.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
                Layout(view);
                Assert.Equal(Visibility.Visible, view.RestorePanel.Visibility);
                Assert.True(view.RestorePitchBox.IsChecked == true && view.RestorePitchBox.IsEnabled);
                Assert.False(view.RestoreBrakesBox.IsEnabled);
                view.RestoreYesButton.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
                Layout(view);
                Assert.Equal(Visibility.Collapsed, view.RestorePanel.Visibility);
                Assert.Null(editor.Draft.RollerPitch);
            });
        }

        private static void Layout(FrameworkElement view)
        {
            view.Measure(new Size(1300, 1400));
            view.Arrange(new Rect(0, 0, 1300, 1400));
            view.UpdateLayout();
        }

        private static IEnumerable<T> Descendants<T>(DependencyObject root) where T : DependencyObject
        {
            var count = VisualTreeHelper.GetChildrenCount(root);
            for (var i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                if (child is T match) yield return match;
                foreach (var nested in Descendants<T>(child)) yield return nested;
            }
        }
    }
}
