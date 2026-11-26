#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using RackCad.Application.Persistence;
using RackCad.Application.RackFrames;
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
    /// I-64, fourth batch, part cb (CB-01 A): «Editar aquí» of a cabecera of its own («Cabecera N» of the list) in the RackCad window, over a
    /// door that reads and writes a cabecera (<see cref="IRackCadCabeceraDoorPort"/>), with the rules of the doors of Dinámico and Selectivo:
    /// numbered requests, an answer only for its request, «Dejar de esperar», the request the command took, copies of the cabecera, the
    /// editor locked while AutoCAD writes, a draft of another drawing, the prune of a closed drawing and the veto of the close (D-08).
    /// </summary>
    public class RackCadCabeceraWindowTests
    {
        private static readonly SessionId Session = new(7);

        private sealed class CabeceraDoorDouble : IRackCadCabeceraDoorPort
        {
            public List<RackDoorRequest> Reads { get; } = new();

            public List<RackDoorRequest> CabeceraReads { get; } = new();

            public List<CabeceraDoorWrite> CabeceraWrites { get; } = new();

            public List<long> Abandoned { get; } = new();

            public RackCadClassicResult Answer { get; set; } = RackCadClassicResult.Started();

            public event Action<RackDesignRead>? DesignRead;

            public event Action<RackDesignWritten>? DesignWritten;

            public event Action<long>? RequestStarted;

            public event Action<CabeceraDesignRead>? CabeceraRead;

            public RackCadClassicResult RequestDesign(RackDoorRequest request)
            {
                Reads.Add(request);
                return Answer;
            }

            public RackCadClassicResult RequestDynamicUpdate(RackDoorWrite write) => Answer;

            public RackCadClassicResult RequestCabeceraDesign(RackDoorRequest request)
            {
                CabeceraReads.Add(request);
                return Answer;
            }

            public RackCadClassicResult RequestCabeceraUpdate(CabeceraDoorWrite write)
            {
                CabeceraWrites.Add(write);
                return Answer;
            }

            public void Abandon(long requestId) => Abandoned.Add(requestId);

            public void Read(RackDesignRead read) => DesignRead?.Invoke(read);

            public void ReadCabecera(CabeceraDesignRead read) => CabeceraRead?.Invoke(read);

            public void Written(RackDesignWritten written) => DesignWritten?.Invoke(written);

            public void Start(long requestId) => RequestStarted?.Invoke(requestId);

            // What RACKCABECERAABRIR answers to the last read: the cabecera of the drawing, through the rule of the door.
            public void AnswerLastRead(string name = "Cabecera 1")
            {
                var request = CabeceraReads.Last();
                ReadCabecera(CabeceraDoorRules.AnswerForOpen(request.RequestId, request.RackId, RackEmbedDocument.KindCabecera, name, Envelope(name)));
            }

            public void AnswerLastWrite(bool written, string message)
            {
                var write = CabeceraWrites.Last();
                Written(new RackDesignWritten(write.RequestId, write.RackId, written, message));
            }
        }

        // The inner design of a cabecera block: a RackProjectDocument with the header, as RACKCABECERA writes it.
        private static string Envelope(string name)
        {
            var header = new RackFrameConfigurationFactory(UiSupport.LoadCatalogSafe()).Build(RackFrameTemplateCatalog.FindStandardOrDefault(), PostId, 132.0, 42.0);
            header.Name = name;
            return new RackProjectStore().Serialize(RackProject.ForSelective(header));
        }

        private sealed class Rig
        {
            public Rig(IRackCadRackDoorPort? door = null)
            {
                Door = door as CabeceraDoorDouble ?? new CabeceraDoorDouble();
                Host = new WorkspacePortDouble { Snapshot = WorkspacePanelFixtures.Ready(session: 7) };
                Summary = new WorkspaceBrowserSummaryPortDouble
                {
                    ListResponder = (_, _) => BrowserRackList.Ready(new[]
                    {
                        new BrowserRackListEntry("CAB-1", "Cabecera 1", RackEmbedDocument.KindCabecera, new[] { "lateral", "planta" }),
                        new BrowserRackListEntry("DIN-1", "DIN 1", "dynamic", new[] { "lateral" }),
                    }),
                };
                Panel = new WorkspacePanelViewModel(Host, _ => { }, new WorkspaceNavigationPortDouble(), Summary, new WorkspacePilotEditPortDouble());
                Window = new RackCadWindowViewModel(Panel, null, null, door ?? Door, UiSupport.LoadCatalogSafe);
                Window.Refresh();
            }

            public WorkspacePortDouble Host { get; }

            public WorkspaceBrowserSummaryPortDouble Summary { get; }

            public WorkspacePanelViewModel Panel { get; }

            public CabeceraDoorDouble Door { get; }

            public RackCadWindowViewModel Window { get; }

            public void Present(long session)
            {
                Host.Snapshot = WorkspacePanelFixtures.Ready(session: session);
                Window.Refresh();
            }

            public CabeceraEditorViewModel OpenEditor()
            {
                Window.OpenRack("CAB-1");
                Window.EditCabeceraHere();
                Door.AnswerLastRead();
                Assert.True(Window.ActiveCabeceraEditor != null, Window.ActionMessage);
                return Window.ActiveCabeceraEditor!;
            }
        }

        /// <summary>A door of Dinámico only: it cannot read a cabecera, so the window does not offer to edit one.</summary>
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

        private static void Change(CabeceraEditorViewModel editor) => Choice(editor.PanelFields, "pArr").SelectedValue = BracingPattern.XBracing.ToString();

        [Fact]
        public void ACabeceraOffersToBeEditedHereOnlyWithItsDoorAndOnlyTheAnswerToThatRequestOpensTheEditor()
        {
            var rig = new Rig();
            rig.Window.OpenRack("CAB-1");
            Assert.True(rig.Window.IsCabeceraRack);
            Assert.True(rig.Window.ShowCabeceraOffer);
            Assert.False(rig.Window.ShowDynamicOffer);

            rig.Window.EditCabeceraHere();
            var request = Assert.Single(rig.Door.CabeceraReads);
            Assert.Equal("CAB-1", request.RackId);
            Assert.Equal(Session, request.Session);
            Assert.Empty(rig.Door.Reads);
            Assert.True(rig.Window.IsWaitingForAutoCad);

            // Another number, or the read of Dinámico with the same number, is not for this request.
            rig.Door.ReadCabecera(CabeceraDoorRules.AnswerForOpen(request.RequestId + 9, "CAB-1", RackEmbedDocument.KindCabecera, "Cabecera 1", Envelope("Cabecera 1")));
            rig.Door.Read(new RackDesignRead(request.RequestId, "CAB-1", true, "dynamic", "X", "{}", null));
            Assert.Null(rig.Window.ActiveCabeceraEditor);

            rig.Door.AnswerLastRead();

            var editor = Assert.IsType<CabeceraEditorViewModel>(rig.Window.ActiveCabeceraEditor);
            Assert.False(rig.Window.IsWaitingForAutoCad);
            Assert.True(rig.Window.HasRackEditor);
            Assert.False(rig.Window.ShowCabeceraOffer);
            Assert.False(rig.Window.ShowPanel);
            Assert.True(editor.IsStandalone);
            Assert.Equal("Cabecera 1", editor.Name);
            Assert.Equal("Limpio", editor.StatusText);

            var dynamicOnly = new Rig(new DynamicOnlyDoor());
            dynamicOnly.Window.OpenRack("CAB-1");
            Assert.False(dynamicOnly.Window.ShowCabeceraOffer);
            dynamicOnly.Window.EditCabeceraHere();
            Assert.False(dynamicOnly.Window.IsWaitingForAutoCad);
        }

        [Fact]
        public void TheRuleOfTheDoorRefusesWhatIsNotACabecera()
        {
            var refused = CabeceraDoorRules.AnswerForOpen(1, "X", RackEmbedDocument.KindSelective, "S", "{}");
            Assert.False(refused.Opens);
            Assert.Equal("El rack no es una cabecera: este editor no lo abre.", refused.Reason);
            Assert.False(CabeceraDoorRules.AnswerForOpen(1, "X", RackEmbedDocument.KindCabecera, "C", "no es json").Opens);

            var plan = CabeceraDoorRules.PlanApply(RackEmbedDocument.KindCabecera, Envelope("Cabecera 1"), null);
            Assert.False(plan.CanWrite);
        }

        [Fact]
        public void UpdateSendsACopyLocksTheEditorAndOnlyAWrittenAnswerLeavesItClean()
        {
            var rig = new Rig();
            var editor = rig.OpenEditor();
            Change(editor);
            Assert.Equal("Modificado", editor.StatusText);
            Assert.Contains("Cabecera 1", rig.Window.UnwrittenWorkOf(Session)); // D-08

            editor.RequestWrite();

            var write = Assert.Single(rig.Door.CabeceraWrites);
            Assert.Equal("CAB-1", write.RackId);
            Assert.Equal(Session, write.Session);
            Assert.Equal("Cabecera 1", write.Name);
            Assert.NotSame(editor.Editor.Configuration, write.Cabecera);
            Assert.Equal(BracingPattern.XBracing, write.Cabecera.BracingPanels[0].Arrangement);
            Assert.True(editor.IsWriting);
            Assert.False(editor.CanEdit);
            Assert.True(rig.Window.CanStopWaiting);

            // The command took it: «Dejar de esperar» is no longer offered.
            rig.Door.Start(write.RequestId);
            Assert.False(rig.Window.CanStopWaiting);
            Assert.Contains("ya tomó la escritura", editor.UpdateBlockedText);

            rig.Door.AnswerLastWrite(false, "AutoCAD no redibujó ninguna vista.");
            Assert.False(editor.IsWriting);
            Assert.Equal("Modificado", editor.StatusText);
            Assert.True(editor.MessageIsError);

            editor.RequestWrite();
            rig.Door.AnswerLastWrite(true, "AutoCAD redibujó 2 vistas del rack con el borrador.");
            Assert.Equal("Limpio", editor.StatusText);
            Assert.Null(rig.Window.UnwrittenWorkOf(Session));
        }

        [Fact]
        public void StopWaitingForAWriteLeavesTheCabeceraEditableAndNotWritten()
        {
            var rig = new Rig();
            var editor = rig.OpenEditor();
            Change(editor);
            editor.RequestWrite();
            var write = rig.Door.CabeceraWrites.Last();

            rig.Window.StopWaiting();

            Assert.Equal(new[] { write.RequestId }, rig.Door.Abandoned.ToArray());
            Assert.False(editor.IsWriting);
            Assert.True(editor.CanEdit);
            Assert.Equal("Modificado", editor.StatusText);

            // A late answer of the abandoned request changes nothing.
            rig.Door.Written(new RackDesignWritten(write.RequestId, "CAB-1", true, "tarde"));
            Assert.Equal("Modificado", editor.StatusText);
        }

        [Fact]
        public void ADraftOfAnotherDrawingIsNotWrittenInTheActiveOneAndAClosedDrawingForgetsIt()
        {
            var rig = new Rig();
            var editor = rig.OpenEditor();
            Change(editor);

            rig.Present(8);
            editor.RequestWrite();
            Assert.Empty(rig.Door.CabeceraWrites);
            Assert.Equal("Este borrador es de otro dibujo: vuelve a ese dibujo para escribirlo.", editor.Message);

            rig.Summary.Dead.Add(Session);
            rig.Present(8);
            Assert.Empty(rig.Window.CabeceraEditors);
            Assert.Contains("Cabecera 1", rig.Window.ActionMessage);
        }

        [Fact]
        public void CloseTheEditorAsksWithChangesAndForgetsTheDraft()
        {
            var rig = new Rig();
            var editor = rig.OpenEditor();
            Change(editor);
            Assert.Equal("Modificado (editor de la ventana)", rig.Window.RackList.Groups.SelectMany(g => g.Items).Single(i => i.RackId == "CAB-1").StatusText);

            editor.RequestClose();
            Assert.True(editor.IsConfirmingClose);
            Assert.NotNull(rig.Window.ActiveCabeceraEditor);
            editor.ConfirmClose(true);

            Assert.Null(rig.Window.ActiveCabeceraEditor);
            Assert.False(rig.Window.HasRackEditor);
            Assert.True(rig.Window.ShowCabeceraOffer);
            Assert.Null(rig.Window.UnwrittenWorkOf(Session));
        }

        [Fact]
        public void ABlankInnerNameIsNotTouchedAndTheBaseOfCleanIsWhatWasSent()
        {
            // Correction 7 of the review: the rule of the classic editor. The inner name stays blank (AUTH-15 admits it); the blocks take the
            // name of the envelope in the command; the editor and the veto of the close say the name of the list.
            var rig = new Rig();
            rig.Window.OpenRack("CAB-1");
            rig.Window.EditCabeceraHere();
            var request = rig.Door.CabeceraReads.Last();
            rig.Door.ReadCabecera(CabeceraDoorRules.AnswerForOpen(request.RequestId, "CAB-1", RackEmbedDocument.KindCabecera, "Cabecera 1", Envelope(null!)));
            var editor = rig.Window.ActiveCabeceraEditor!;

            Assert.Equal(string.Empty, editor.Name);
            Assert.True(string.IsNullOrEmpty(editor.Editor.Configuration.Name));
            Assert.False(editor.IsDirty);
            Change(editor);
            Assert.Contains("Cabecera 1", rig.Window.UnwrittenWorkOf(Session));

            editor.RequestWrite();
            var write = rig.Door.CabeceraWrites.Last();
            Assert.True(string.IsNullOrEmpty(write.Name));
            Assert.True(string.IsNullOrEmpty(write.Cabecera.Name));

            rig.Door.AnswerLastWrite(true, "AutoCAD redibujó 2 vistas del rack con el borrador.");
            Assert.False(editor.IsDirty);
            Assert.Equal("Limpio", editor.StatusText);
            Assert.Equal(new RackFrameProjectStore().Serialize(write.Cabecera), editor.Editor.Json);
        }

        [Fact]
        public void ThePageOfTheRackHostsTheEditorOfTheCabecera()
        {
            StaTestRunner.Run(() =>
            {
                var rig = new Rig();
                var view = new RackCadWindowView(rig.Window);
                rig.Window.OpenRack("CAB-1");
                view.Measure(new Size(1300, 900));
                view.Arrange(new Rect(0, 0, 1300, 900));
                view.UpdateLayout();
                Assert.Equal(Visibility.Visible, view.CabeceraOfferPanel.Visibility);
                Assert.Equal("Editar la cabecera en la ventana", System.Windows.Automation.AutomationProperties.GetName(view.EditCabeceraHereButton));

                view.EditCabeceraHereButton.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
                rig.Door.AnswerLastRead();
                view.UpdateLayout();

                var hosted = Assert.IsType<CabeceraEditorView>(view.RackPageHost.Content);
                Assert.Same(rig.Window.ActiveCabeceraEditor, hosted.ViewModel);
                Assert.Equal(1, view.CabeceraViewCount);
                Assert.Equal(Visibility.Collapsed, view.CabeceraOfferPanel.Visibility);

                rig.Window.ActiveCabeceraEditor!.RequestClose();
                view.UpdateLayout();
                Assert.Same(view.PanelView, view.RackPageHost.Content);
                Assert.Equal(0, view.CabeceraViewCount);
            });
        }
    }
}
