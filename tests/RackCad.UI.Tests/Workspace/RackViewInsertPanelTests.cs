#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.Shared;
using RackCad.Application.VentanaRackCad;
using RackCad.Application.Workspace;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.Shared;
using RackCad.UI.VentanaRackCad;
using RackCad.UI.Workspace;
using Xunit;
using static RackCad.UI.Tests.SelectiveWindowFixtures;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64 A-14, line D, fourth batch, part «ins»: the panel «Insertar vista…» of the editors of the RackCad window (prototype «dialogos»,
    /// DG-01 to DG-08, accepted by the Owner on 2026-11-26), in the Selectivo and the Dinámico editor and over the window: the classic dialog
    /// «Insertar varias vistas» is no longer shown, the order and the starting point (DG-02), once per pass (DG-03), the kinds of view the
    /// drawing has (DG-04), the panel that opens also when it cannot insert and says why (DG-05), no name foreseen (DG-06), the same panel
    /// for every editor (DG-07), and the panel that follows the draft (DG-08); then the views with real WPF.
    /// </summary>
    public class RackViewInsertPanelTests
    {
        private static readonly SessionId Session = new(7);

        private static readonly RackViewAddress Planta = RackViewAddress.Whole(DimensionViewKind.Planta);

        // ------------------------------------------------------------------ doubles

        /// <summary>A door that reads and writes a Selectivo and a Dinámico, as the door of the Plugin does.</summary>
        private sealed class DoorDouble : IRackCadSelectiveDoorPort
        {
            public List<RackDoorRequest> Reads { get; } = new();

            public List<RackDoorRequest> SelectiveReads { get; } = new();

            public List<RackDoorWrite> Writes { get; } = new();

            public List<SelectiveDoorWrite> SelectiveWrites { get; } = new();

            public event Action<RackDesignRead>? DesignRead;

            public event Action<RackDesignWritten>? DesignWritten;

            public event Action<long>? RequestStarted { add { } remove { } }

            public event Action<SelectiveDesignRead>? SelectiveRead;

            public RackCadClassicResult RequestDesign(RackDoorRequest request)
            {
                Reads.Add(request);
                return RackCadClassicResult.Started();
            }

            public RackCadClassicResult RequestDynamicUpdate(RackDoorWrite write)
            {
                Writes.Add(write);
                return RackCadClassicResult.Started();
            }

            public RackCadClassicResult RequestSelectiveDesign(RackDoorRequest request)
            {
                SelectiveReads.Add(request);
                return RackCadClassicResult.Started();
            }

            public RackCadClassicResult RequestSelectiveUpdate(SelectiveDoorWrite write)
            {
                SelectiveWrites.Add(write);
                return RackCadClassicResult.Started();
            }

            public void Abandon(long requestId)
            {
            }

            public void AnswerLastSelectiveRead(Domain.Systems.Selective.SelectivePalletDesign? design = null)
            {
                var request = SelectiveReads.Last();
                SelectiveRead?.Invoke(Read(request.RequestId, request.RackId, design));
            }

            public void AnswerLastDynamicRead()
            {
                var request = Reads.Last();
                var draft = DynamicDraftEditor.CreateNew(UiSupport.LoadCatalogSafe());
                DesignRead?.Invoke(new RackDesignRead(request.RequestId, request.RackId, true, "dynamic", "DIN 1",
                    new RackProjectStore().Serialize(RackProject.ForDynamic(draft.Design)), null));
            }

            public void AnswerLastSelectiveWrite(bool written, string message)
            {
                var write = SelectiveWrites.Last();
                DesignWritten?.Invoke(new RackDesignWritten(write.RequestId, write.RackId, written, message));
            }
        }

        /// <summary>The modal port of the host: it records every dialog it was given.</summary>
        private sealed class ModalPortDouble : IRackCadModalPort
        {
            public List<Window> Shown { get; } = new();

            public bool? ShowModal(Window dialog)
            {
                Shown.Add(dialog);
                return false;
            }
        }

        private sealed class Rig
        {
            public Rig()
            {
                Host = new WorkspacePortDouble { Snapshot = WorkspacePanelFixtures.Ready(session: 7) };
                Summary = new WorkspaceBrowserSummaryPortDouble { ListResponder = (_, _) => BrowserRackList.Ready(Listed) };
                Panel = new WorkspacePanelViewModel(Host, _ => { }, new WorkspaceNavigationPortDouble(), Summary, new WorkspacePilotEditPortDouble());
                Window = new RackCadWindowViewModel(Panel, null, null, Door, UiSupport.LoadCatalogSafe, Modal);
                Window.Refresh();
            }

            public List<BrowserRackListEntry> Listed { get; } = new()
            {
                new BrowserRackListEntry("SEL-1", "Selectivo A", "selective", new[] { "frontal", "lateral" }),
                new BrowserRackListEntry("DIN-1", "DIN 1", "dynamic", new[] { "planta" }),
            };

            public ModalPortDouble Modal { get; } = new();

            public WorkspacePortDouble Host { get; }

            public WorkspaceBrowserSummaryPortDouble Summary { get; }

            public WorkspacePanelViewModel Panel { get; }

            public DoorDouble Door { get; } = new();

            public RackCadWindowViewModel Window { get; }

            public SelectiveEditorViewModel OpenSelective(Domain.Systems.Selective.SelectivePalletDesign? design = null)
            {
                Window.OpenRack("SEL-1");
                Window.EditSelectiveHere();
                Door.AnswerLastSelectiveRead(design);
                Assert.True(Window.ActiveSelectiveEditor != null, Window.ActionMessage);
                return Window.ActiveSelectiveEditor!;
            }

            public DynamicEditorViewModel OpenDynamic()
            {
                Window.OpenRack("DIN-1");
                Window.EditDynamicHere();
                Door.AnswerLastDynamicRead();
                Assert.True(Window.ActiveDynamicEditor != null, Window.ActionMessage);
                return Window.ActiveDynamicEditor!;
            }
        }

        private static string[] Labels(RackViewInsertPanelViewModel panel) => panel.Order.Select(o => o.Label).ToArray();

        // ------------------------------------------------------------------ DG-01, DG-02, DG-04, DG-06: a rack of the drawing and a new one

        [Fact]
        public void ARackOfTheDrawingOpensThePanelWithNothingMarkedAndWhatTheDrawingHas()
        {
            var rig = new Rig();
            var editor = rig.OpenSelective();
            Assert.False(editor.IsInsertPanelOpen);
            Assert.Equal(string.Empty, editor.InsertPanelStatus);

            editor.InsertView();
            Assert.Empty(rig.Modal.Shown); // DG-01: no classic dialog over AutoCAD
            Assert.True(editor.IsInsertPanelOpen);
            Assert.Equal("panel abierto", editor.InsertPanelStatus);
            var panel = editor.InsertPanel!;

            Assert.Equal("Insertar vistas de Selectivo A", panel.Title);
            Assert.Equal("Marca las vistas y ordénalas en «Orden de colocación»: AutoCAD pide el punto de cada una en ese orden. Mientras eliges, AutoCAD y el editor siguen libres.",
                panel.Lead);
            Assert.True(panel.IsOrderEmpty); // DG-02: nothing marked
            Assert.Equal("Elige al menos una vista", panel.InsertLabel);
            Assert.False(panel.CanInsert);
            Assert.False(panel.Insert());
            Assert.Empty(rig.Door.SelectiveWrites);

            // The groups of the prototype, in their order, with their help.
            Assert.Equal(new[] { "Frontal", "Lateral (corte por poste)", "Planta" }, panel.Groups.Select(g => g.Title).ToArray());
            Assert.Equal(new[] { "Una por fondo; este rack tiene uno.", "Un corte por poste; puedes marcar varios.", "Todo el rack." },
                panel.Groups.Select(g => g.Help).ToArray());
            Assert.Equal(new[] { "Fondo 1" }, panel.Groups[0].Options.Select(o => o.Chip).ToArray());
            Assert.Equal(new[] { "Poste 1", "Poste 2", "Poste 3" }, panel.Groups[1].Options.Select(o => o.Chip).ToArray());
            Assert.True(panel.Groups[1].HasCuts);
            Assert.False(panel.Groups[0].HasCuts);

            // DG-04: the kinds the list read for the rack; never which fondo or post.
            Assert.Equal("En el dibujo ya hay: frontal y lateral (según la última lectura de la lista). La ventana todavía no sabe cuál es cada una (de qué fondo o de qué poste): una vista que ya existe se coloca otra vez, enlazada al mismo rack.",
                panel.ExistingText);
            Assert.Equal("Sin cambios: AutoCAD redibuja tal cual las vistas que ya existen y luego coloca las nuevas, enlazadas al rack.", panel.Note);
            Assert.Equal("Si cancelas la colocación en AutoCAD, las vistas que ya existían quedan escritas y no se coloca ninguna nueva.", panel.CancelNote);

            // Asked again while it is open, it stays as it is.
            panel.Toggle(Planta);
            editor.InsertView();
            Assert.Same(panel, editor.InsertPanel);
            Assert.Equal(new[] { "Planta" }, Labels(panel));
            Assert.Equal("Insertar 1 vista", panel.InsertLabel);
            Assert.True(panel.CanInsert);

            // A change of the draft: the note says the draft is written first.
            Write(Field(editor.CellFields, "cHeight"), "72");
            Assert.Equal("Hay cambios: AutoCAD escribe antes el borrador en las vistas que ya existen y luego coloca las nuevas, enlazadas al rack.", panel.Note);

            // «Cancelar»: closed, nothing written; opened again, it starts again with nothing marked.
            panel.Close();
            Assert.False(editor.IsInsertPanelOpen);
            editor.InsertView();
            Assert.True(editor.InsertPanel!.IsOrderEmpty);
            Assert.Empty(rig.Door.SelectiveWrites);
        }

        [Fact]
        public void ANewSelectivoOpensThePanelWithTheThreeViewsOfTheDraftAndNoNameForeseen()
        {
            var rig = new Rig();
            rig.Window.StartNewSelective();
            var editor = rig.Window.NewSelectiveEditor!;
            editor.InsertView();
            var panel = editor.InsertPanel!;

            Assert.Equal("Insertar el Selectivo nuevo en el dibujo", panel.Title);
            Assert.Equal(new[] { "Frontal · fondo 1", "Lateral · poste 1", "Planta" }, Labels(panel)); // DG-02
            Assert.Equal("Insertar con 3 vistas", panel.InsertLabel);
            Assert.Equal(string.Empty, panel.ExistingText); // not in the drawing yet
            Assert.False(panel.HasExistingText);
            // DG-06: no name foreseen; the note says the automatic one of I-60 is taken.
            Assert.Equal("Rack nuevo: se inserta con una identidad nueva y, si no le pones nombre, toma el automático (I-60).", panel.Note);
            Assert.DoesNotContain("Selectivo 1", panel.Title + panel.Note + panel.InsertLabel, StringComparison.Ordinal);
            Assert.Equal("Si cancelas la colocación en AutoCAD, el rack nuevo no se inserta y sigue aquí.", panel.CancelNote);

            panel.Insert();
            var write = Assert.Single(rig.Door.SelectiveWrites);
            Assert.True(write.NewRack);
            Assert.Equal(new[] { RackViewAddress.Fondo(0), RackViewAddress.Post(0), Planta }, write.InsertViews!.ToArray());
            Assert.False(editor.IsInsertPanelOpen); // the write took its views
            Assert.False(editor.CanOpenInsertPanel);
        }

        [Fact]
        public void TheOrderNumbersTheBoxesAndTheRowsAndTheGesturesAtTheEndsAreOff()
        {
            var editor = new Rig().OpenSelective();
            editor.InsertView();
            var panel = editor.InsertPanel!;
            var postBox = panel.Groups[1].Options[1]; // «Poste 2»
            postBox.IsChecked = true; // a click of the box
            panel.Toggle(Planta);
            panel.Toggle(RackViewAddress.Fondo(0));

            Assert.True(postBox.IsChecked);
            Assert.Equal("1", postBox.Number);
            Assert.Equal("puesto 1 del orden", postBox.NumberText);
            Assert.Equal("Lateral · poste 2", postBox.Label);
            Assert.Equal(new[] { "1", "2", "3" }, panel.Order.Select(o => o.Number).ToArray());
            Assert.Equal(new[] { false, true, true }, panel.Order.Select(o => o.CanMoveUp).ToArray());
            Assert.Equal(new[] { true, true, false }, panel.Order.Select(o => o.CanMoveDown).ToArray());
            Assert.Equal("Subir Planta", panel.Order[1].MoveUpName);
            Assert.Equal("Quitar Frontal · fondo 1 del orden", panel.Order[2].RemoveName);

            // The row item moves with its view (its container, and the focus in it, survive the gesture).
            var planta = panel.Order[1];
            Assert.True(panel.MoveUp(1));
            Assert.Same(planta, panel.Order[0]);
            Assert.Equal("1", planta.Number);
            Assert.False(panel.MoveUp(0));
            Assert.Equal("2", postBox.Number);

            // «Todos los postes» / «Ninguno».
            var laterals = panel.Groups[1];
            Assert.Equal("Todos los postes", laterals.MarkAllLabel);
            Assert.Equal("Marcar todos los postes", laterals.MarkAllName);
            Assert.Equal("Quitar todos los postes del orden", laterals.MarkNoneName);
            Assert.True(laterals.CanMarkAll);
            Assert.True(panel.MarkAllCuts());
            Assert.False(laterals.CanMarkAll);
            Assert.Equal(new[] { "Planta", "Lateral · poste 2", "Frontal · fondo 1", "Lateral · poste 1", "Lateral · poste 3" }, Labels(panel));
            Assert.True(panel.UnmarkAllCuts());
            Assert.False(laterals.CanMarkNone);
            Assert.Equal(new[] { "Planta", "Frontal · fondo 1" }, Labels(panel));

            // The box unmarked takes its view out of the order.
            panel.Groups[0].Options[0].IsChecked = false;
            Assert.Equal(new[] { "Planta" }, Labels(panel));
        }

        // ------------------------------------------------------------------ DG-05: it opens anyway and says why

        [Fact]
        public void ThePanelOpensAlsoWhenItCannotInsertAndSaysWhyNextToTheButton()
        {
            var rig = new Rig();
            var editor = rig.OpenSelective();

            // Tramos not applied.
            editor.SelectFront(0);
            editor.StartSegments();
            Assert.True(editor.CanOpenInsertPanel, editor.InsertPanelBlockedText);
            editor.InsertView();
            var panel = editor.InsertPanel!;
            panel.Toggle(Planta);
            Assert.Equal("Aplica los tramos con «Aplicar tramos» o descártalos antes de escribir.", panel.Reason);
            Assert.True(panel.HasReason);
            Assert.False(panel.CanInsert);
            Assert.False(panel.Insert());
            editor.DiscardSegments();
            Assert.Equal(string.Empty, panel.Reason);
            Assert.True(panel.CanInsert);

            // Another request of the window waits for AutoCAD (dolor 9: before, the dialog opened and the write was refused after).
            var other = rig.OpenDynamic();
            rig.Window.OpenRack("SEL-1");
            Assert.Same(editor, rig.Window.ActiveSelectiveEditor);
            Write(Field(other.Essentials, "weight"), "1200");
            other.RequestWrite();
            Assert.True(rig.Window.IsWaitingForAutoCad);
            Assert.StartsWith("AutoCAD todavía no respondió a la petición anterior de la ventana", panel.Reason, StringComparison.Ordinal);
            Assert.False(panel.CanInsert);
            Assert.True(editor.CanOpenInsertPanel); // the button stays on: only the write waits
            Assert.False(editor.CanInsertView);

            // The answer arrives: the panel can insert again, with no gesture.
            rig.Window.StopWaiting();
            Assert.Equal(string.Empty, panel.Reason);
            Assert.True(panel.CanInsert);
            panel.Insert();
            Assert.Single(rig.Door.SelectiveWrites);
        }

        [Fact]
        public void ARackReadOnlyOpensThePanelWithItsReasonAndAnEditorOutOfTheWindowHasNoPanel()
        {
            // S-08: a rack that could not be recomposed when it was opened is read only; the panel opens and says so (DG-05).
            var broken = Structure();
            broken.PostPeralte = 0.0;
            var draft = new SelectiveDraftEditor(UiSupport.LoadCatalogSafe());
            Assert.False(draft.LoadFrom(broken, null, "Selectivo A"));
            var readOnly = new SelectiveEditorViewModel(draft, RackId, _ => { });
            var inserted = new List<IReadOnlyList<RackViewAddress>>();
            readOnly.InsertViewsAction = (_, views) => inserted.Add(views);
            Assert.True(readOnly.CanOpenInsertPanel, readOnly.InsertPanelBlockedText);
            readOnly.InsertView();
            var panel = readOnly.InsertPanel!;
            Assert.StartsWith("No se pudo recomponer el rack al abrirlo:", panel.Reason, StringComparison.Ordinal);
            panel.Toggle(Planta);
            Assert.False(panel.Insert());
            Assert.Empty(inserted);

            // With no action of the window there is nothing to open.
            var loose = SelectiveEditor();
            Assert.False(loose.CanOpenInsertPanel);
            Assert.Equal("Aquí no se pueden insertar vistas del rack.", loose.InsertPanelBlockedText);
            loose.InsertView();
            Assert.False(loose.IsInsertPanelOpen);
            Assert.Equal("Aquí no se pueden insertar vistas del rack.", loose.Message);
        }

        // ------------------------------------------------------------------ DG-08: the panel follows the draft

        [Fact]
        public void ThePanelFollowsTheDraftAViewThatNoLongerExistsStaysMarkedAndIsNotInserted()
        {
            var rig = new Rig();
            var editor = rig.OpenSelective(); // two fronts: posts 1 to 3
            editor.InsertView();
            var panel = editor.InsertPanel!;
            panel.Toggle(RackViewAddress.Post(2));
            panel.Toggle(Planta);

            // With the panel open, the editor stays free: one front less, and «Poste 3» is gone.
            Write(Field(editor.Essentials, "fronts"), "1");
            Assert.Same(panel, editor.InsertPanel);
            Assert.Equal(new[] { "Poste 1", "Poste 2" }, panel.Groups[1].Options.Select(o => o.Chip).ToArray());
            Assert.True(panel.Order[0].IsGone);
            Assert.Equal("Lateral · poste 3", panel.Order[0].Label);
            Assert.Equal("1: Lateral · poste 3; Ya no existe en el borrador: no se insertará", panel.Order[0].AccessibleText);
            Assert.True(panel.HasGone);
            Assert.Equal("Lo tachado ya no existe en el borrador (cambiaron los fondos o los frentes): no se insertará. Quítalo, o devuelve el borrador a como estaba.",
                panel.GoneText);
            Assert.Equal("Insertar 1 vista", panel.InsertLabel);

            panel.Insert();
            Assert.Equal(new[] { Planta }, Assert.Single(rig.Door.SelectiveWrites).InsertViews!.ToArray());
        }

        // ------------------------------------------------------------------ DG-07: the same panel for the Dinámico

        [Fact]
        public void TheDinamicoHasTheSamePanelWithItsEndsAndTheNewDinamicoStartsWithTheFirstOfEachGroup()
        {
            var rig = new Rig();
            var editor = rig.OpenDynamic();
            editor.InsertView();
            Assert.Empty(rig.Modal.Shown);
            var panel = editor.InsertPanel!;
            Assert.Equal(RackSystemKind.PalletFlow, panel.Choice.System);
            Assert.Equal("Insertar vistas de DIN 1", panel.Title);
            Assert.Equal(new[] { "Salida", "Entrada" }, panel.Groups[0].Options.Select(o => o.Chip).ToArray());
            Assert.Equal("Los dos cortes frontales.", panel.Groups[0].Help);
            Assert.Equal("Un corte por poste con frontera: entre dos frentes en blanco no hay (I-33). Puedes marcar varios.", panel.Groups[1].Help);
            Assert.StartsWith("En el dibujo ya hay: planta (según", panel.ExistingText, StringComparison.Ordinal);
            Assert.True(panel.IsOrderEmpty);

            panel.Toggle(RackViewAddress.FlowEnd(RackFlowEnd.Entrance));
            panel.Insert();
            var write = Assert.Single(rig.Door.Writes);
            Assert.Equal(new[] { RackViewAddress.FlowEnd(RackFlowEnd.Entrance) }, write.InsertViews!.ToArray());
            Assert.False(editor.IsInsertPanelOpen);

            // A new Dinámico: «Frontal · salida», the first lateral and «Planta».
            var fresh = new Rig();
            fresh.Window.StartNewDynamic();
            var created = fresh.Window.NewDynamicEditor!;
            created.InsertView();
            Assert.Equal("Insertar el Dinámico nuevo en el dibujo", created.InsertPanel!.Title);
            Assert.Equal(new[] { "Frontal · salida", "Lateral · poste 1", "Planta" }, Labels(created.InsertPanel));
            Assert.Equal("Insertar con 3 vistas", created.InsertPanel.InsertLabel);
        }

        [Fact]
        public void OpeningThePanelClosesTheQuestionOfClosingTheEditor()
        {
            var editor = new Rig().OpenSelective();
            Write(Field(editor.CellFields, "cHeight"), "72");
            editor.RequestClose();
            Assert.True(editor.IsConfirmingClose);
            editor.InsertView();
            Assert.False(editor.IsConfirmingClose);
            Assert.True(editor.IsInsertPanelOpen);
        }

        // ------------------------------------------------------------------ the window no longer shows the classic dialog

        [Fact]
        public void NoFileOfTheWindowShowsTheClassicDialogOfInsertarVariasVistas()
        {
            // DG-01: the classic editors keep «Insertar varias vistas» until they migrate; the window does not show it.
            var folder = Path.Combine(RepoRoot(), "src", "RackCad.UI", "VentanaRackCad");
            var dialog = new Regex(@"\bRackViewBatchDialog(Presenter)?\b");
            foreach (var file in Directory.GetFiles(folder, "*.cs").Concat(Directory.GetFiles(folder, "*.xaml")))
            {
                var code = Regex.Replace(Regex.Replace(File.ReadAllText(file), @"/\*.*?\*/", string.Empty, RegexOptions.Singleline), @"//[^\n]*", string.Empty);
                Assert.False(dialog.IsMatch(code), Path.GetFileName(file) + " muestra el diálogo «Insertar varias vistas».");
            }
        }

        private static string RepoRoot()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "RackCad.sln"))) dir = dir.Parent;
            Assert.True(dir != null, "No se encontró la raíz del repositorio.");
            return dir!.FullName;
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

        private static void Lay(FrameworkElement view, double width = 1200)
        {
            view.Measure(new Size(width, 900));
            view.Arrange(new Rect(0, 0, width, 900));
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

        // Shown: it and every element above it are visible (IsVisible needs a window on the screen).
        private static bool Shown(UIElement element)
        {
            for (DependencyObject? node = element; node != null; node = System.Windows.Media.VisualTreeHelper.GetParent(node))
            {
                if (node is UIElement ui && ui.Visibility != Visibility.Visible) return false;
            }

            return true;
        }

        [Fact]
        public void TheViewOfThePanelInTheSelectivoEditorShowsTheGroupsTheOrderAndTheReasonWithUniqueNames()
        {
            StaTestRunner.Run(() => WithBindingErrors(() =>
            {
                var rig = new Rig();
                var editor = rig.OpenSelective();
                var view = new SelectiveEditorView(editor);
                Lay(view);
                Assert.Equal(Visibility.Collapsed, view.InsertPanelScroll.Visibility);

                Click(view.InsertViewButton);
                Lay(view);
                Assert.Equal(Visibility.Visible, view.InsertPanelScroll.Visibility);
                Assert.Equal("panel abierto", AutomationProperties.GetItemStatus(view.InsertViewButton));
                var panelView = view.InsertPanelView;
                Assert.Same(editor.InsertPanel, panelView.DataContext);
                Assert.Equal("Insertar vistas de Selectivo A", panelView.InsertPanelTitleText.Text);
                Assert.Equal("Elige al menos una vista", panelView.InsertPanelGoButton.Content);
                Assert.False(panelView.InsertPanelGoButton.IsEnabled);
                Assert.Equal(Visibility.Visible, panelView.InsertPanelEmptyBox.Visibility);

                // A box clicked through UI Automation marks its view, with its number.
                var boxes = Descendants<CheckBox>(panelView).ToList();
                Assert.Equal(new[] { "Frontal · fondo 1", "Lateral · poste 1", "Lateral · poste 2", "Lateral · poste 3", "Planta" }, boxes.Select(Name).ToArray());
                var planta = boxes.Single(b => Name(b) == "Planta");
                ((IToggleProvider)new CheckBoxAutomationPeer(planta).GetPattern(PatternInterface.Toggle)).Toggle();
                var post2 = boxes.Single(b => Name(b) == "Lateral · poste 2");
                ((IToggleProvider)new CheckBoxAutomationPeer(post2).GetPattern(PatternInterface.Toggle)).Toggle();
                Lay(view);
                Assert.Equal(new[] { "Planta", "Lateral · poste 2" }, editor.InsertPanel!.Order.Select(o => o.Label).ToArray());
                Assert.Equal("puesto 2 del orden", AutomationProperties.GetItemStatus(post2));
                Assert.Equal("Insertar 2 vistas", panelView.InsertPanelGoButton.Content);
                Assert.True(panelView.InsertPanelGoButton.IsEnabled);
                Assert.Equal(Visibility.Collapsed, panelView.InsertPanelEmptyBox.Visibility);

                // Every button of the panel has its own name; «Bajar» of the first row moves it.
                var buttons = Descendants<Button>(panelView).Where(Shown).ToList();
                var names = buttons.Select(Name).ToList();
                Assert.Equal(names.Count, names.Distinct().Count());
                Assert.Contains("Subir Lateral · poste 2", names);
                Assert.Contains("Quitar Planta del orden", names);
                Assert.Contains("Marcar todos los postes", names);
                Assert.Contains("Cancelar la inserción de vistas", names);
                Click(buttons.Single(b => Name(b) == "Bajar Planta"));
                Lay(view);
                Assert.Equal(new[] { "Lateral · poste 2", "Planta" }, editor.InsertPanel.Order.Select(o => o.Label).ToArray());

                // The reason next to the button (DG-05), and «Insertar 2 vistas» writes once it is gone.
                editor.SelectFront(0);
                editor.StartSegments();
                Lay(view);
                Assert.Equal("Aplica los tramos con «Aplicar tramos» o descártalos antes de escribir.", panelView.InsertPanelReasonText.Text);
                Assert.Equal(Visibility.Visible, panelView.InsertPanelReasonText.Visibility);
                Assert.False(panelView.InsertPanelGoButton.IsEnabled);
                editor.DiscardSegments();
                Lay(view);
                Assert.Equal(Visibility.Collapsed, panelView.InsertPanelReasonText.Visibility);
                Click(panelView.InsertPanelGoButton);
                Assert.Equal(new[] { RackViewAddress.Post(1), Planta }, Assert.Single(rig.Door.SelectiveWrites).InsertViews!.ToArray());
                Lay(view);
                Assert.Equal(Visibility.Collapsed, view.InsertPanelScroll.Visibility);
                Assert.False(view.InsertViewButton.IsEnabled);
            }));
        }

        [Fact]
        public void ThePanelPutsTheOrderUnderTheViewsWhenThePageIsNarrowAndTheDinamicoHostsTheSameView()
        {
            StaTestRunner.Run(() => WithBindingErrors(() =>
            {
                var rig = new Rig();
                var editor = rig.OpenDynamic();
                var view = new DynamicEditorView(editor);
                Lay(view);
                Click(view.InsertViewButton);
                Lay(view);
                Assert.Equal(Visibility.Visible, view.InsertPanelScroll.Visibility);
                var panelView = view.InsertPanelView;
                Assert.Same(editor.InsertPanel, panelView.DataContext);
                Assert.Equal(new[] { "Frontal · salida", "Frontal · entrada" },
                    Descendants<CheckBox>(panelView).Select(Name).Take(2).ToArray());
                Assert.False(panelView.OrderBelow);

                Lay(view, width: 420);
                Assert.True(panelView.OrderBelow);
                Assert.Equal(1, Grid.GetRow(panelView.OrderColumn));
                Assert.Equal(0, Grid.GetColumn(panelView.OrderColumn));
                Lay(view);
                Assert.False(panelView.OrderBelow);

                // «Cancelar» closes it.
                Click(panelView.InsertPanelCancelButton);
                Lay(view);
                Assert.Equal(Visibility.Collapsed, view.InsertPanelScroll.Visibility);
                Assert.False(editor.IsInsertPanelOpen);
            }));
        }
    }
}
