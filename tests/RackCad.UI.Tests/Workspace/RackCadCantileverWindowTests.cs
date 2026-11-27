#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using RackCad.Application.Catalogs;
using RackCad.Application.CustomProperties;
using RackCad.Application.Persistence;
using RackCad.Application.StructuralSections;
using RackCad.Application.Systems.Cantilever;
using RackCad.Application.Systems.Shared;
using RackCad.Application.VentanaRackCad;
using RackCad.Application.Workspace;
using RackCad.Domain.Systems.Cantilever;
using RackCad.Domain.Systems.Shared;
using RackCad.UI.VentanaRackCad;
using RackCad.UI.Workspace;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>«Insertar vista…» of a Cantilever editor with these views, in this order (as InsertPanelTestSupport does for the others).</summary>
    internal static class CantileverInsertSupport
    {
        public static RackViewInsertPanelViewModel? InsertWith(this CantileverEditorViewModel editor, params RackViewAddress[] views)
        {
            editor.InsertView();
            var panel = editor.InsertPanel;
            if (panel == null) return null;
            panel.Mark(views);
            panel.Insert();
            return panel;
        }
    }

    /// <summary>
    /// T5 ct (I-64, fifth batch; prototype «cantilever»): the editor of a Cantilever line in the RackCad window, over a door that reads and writes
    /// a Cantilever line (<see cref="IRackCadCantileverDoorPort"/>), with the rules of the other doors (numbered requests, an answer only for its
    /// request, «Dejar de esperar», the request the command took, copies, the editor locked while AutoCAD writes, a draft of another drawing, the
    /// prune of a closed drawing, the veto of the close, D-08), and its tabs: Línea, Brazos (the first one), Componentes, Arriostramiento and
    /// Propiedades; «Cantilever en esta ventana» as a draft of «Nuevo», «Insertar vista…» with the panel of the views, the BOM of the draft and
    /// «Guardar en biblioteca…»; and the views with real WPF.
    /// </summary>
    public partial class RackCadCantileverWindowTests // T6 lib: partial (RackCadCantileverWindowTests.Lib.cs: UT-S10)
    {
        private static readonly SessionId Session = new(7);
        private const string ColumnW = "AISC-W-W10X33";
        private const string BaseW = "AISC-W-W12X26";
        private const string ArmHss = "AISC-HSS-RECT-HSS4X4X_250";

        private static readonly StructuralSectionCatalog Catalogue = new CsvStructuralSectionCatalogProvider(CatalogDirectory.Resolve()).Load();

        private sealed class CantileverDoorDouble : IRackCadCantileverDoorPort
        {
            public List<RackDoorRequest> Reads { get; } = new();

            public List<RackDoorRequest> CantileverReads { get; } = new();

            public List<CantileverDoorWrite> CantileverWrites { get; } = new();

            public List<long> Abandoned { get; } = new();

            public RackCadClassicResult Answer { get; set; } = RackCadClassicResult.Started();

            public event Action<RackDesignRead>? DesignRead;

            public event Action<RackDesignWritten>? DesignWritten;

            public event Action<long>? RequestStarted;

            public event Action<CantileverDesignRead>? CantileverRead;

            public RackCadClassicResult RequestDesign(RackDoorRequest request)
            {
                Reads.Add(request);
                return Answer;
            }

            public RackCadClassicResult RequestDynamicUpdate(RackDoorWrite write) => Answer;

            public RackCadClassicResult RequestCantileverDesign(RackDoorRequest request)
            {
                CantileverReads.Add(request);
                return Answer;
            }

            public RackCadClassicResult RequestCantileverUpdate(CantileverDoorWrite write)
            {
                CantileverWrites.Add(write);
                return Answer;
            }

            public void Abandon(long requestId) => Abandoned.Add(requestId);

            public void Read(RackDesignRead read) => DesignRead?.Invoke(read);

            public void ReadCantilever(CantileverDesignRead read) => CantileverRead?.Invoke(read);

            public void Written(RackDesignWritten written) => DesignWritten?.Invoke(written);

            public void Start(long requestId) => RequestStarted?.Invoke(requestId);

            // What RACKCANTILEVERABRIR answers to the last read: the line of the drawing, through the rule of the door.
            public void AnswerLastRead(string name = "Cantilever 1", CantileverLineDesign? design = null)
            {
                var request = CantileverReads.Last();
                ReadCantilever(CantileverDoorRules.AnswerForOpen(request.RequestId, request.RackId, RackEmbedDocument.KindCantilever, name,
                    new RackProjectStore().Serialize(RackProject.ForCantilever(design ?? LineOfTheDrawing()))));
            }

            public void AnswerLastWrite(bool written, string message, string? name = null)
            {
                var write = CantileverWrites.Last();
                Written(new RackDesignWritten(write.RequestId, write.RackId, written, message, name));
            }
        }

        // The line of the test of CantileverLineTests: 3 stations at 96 in, single on +Y, 3 levels, clear 24, W10X33 with a base W12X26 of 48 in,
        // an arm HSS4X4X1/4 of 36 in.
        private static CantileverLineDesign LineOfTheDrawing()
        {
            var draft = new CantileverDraftEditor(Catalogue);
            Assert.Null(draft.Write(CantileverLineField.StationCount, "3"));
            Assert.Null(draft.Write(CantileverLineField.Spacing, "96"));
            Assert.Null(draft.Write(CantileverLineField.LevelCount, "3"));
            Assert.Null(draft.Write(CantileverLineField.ClearHeight, "24"));
            Assert.Null(draft.Write(CantileverLineField.ColumnSection, ColumnW));
            Assert.Null(draft.Write(CantileverLineField.BaseSection, BaseW));
            Assert.Null(draft.Write(CantileverLineField.BaseLength, "48"));
            Assert.Null(draft.WriteArm(null, CantileverArmField.Section, ArmHss));
            Assert.Null(draft.WriteArm(null, CantileverArmField.CutLength, "36"));
            Assert.True(draft.IsResolved);
            draft.SetName("Cantilever 1");
            return draft.Design;
        }

        private sealed class Rig
        {
            public Rig(IRackCadRackDoorPort? door = null)
            {
                Door = door as CantileverDoorDouble ?? new CantileverDoorDouble();
                Host = new WorkspacePortDouble { Snapshot = WorkspacePanelFixtures.Ready(session: 7) };
                Summary = new WorkspaceBrowserSummaryPortDouble
                {
                    ListResponder = (_, _) => BrowserRackList.Ready(new[]
                    {
                        new BrowserRackListEntry("CAN-1", "Cantilever 1", RackEmbedDocument.KindCantilever, new[] { "frontal", "planta", "lateral" }),
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

            public CantileverDoorDouble Door { get; }

            public RackCadWindowViewModel Window { get; }

            public void Present(long session)
            {
                Host.Snapshot = WorkspacePanelFixtures.Ready(session: session);
                Window.Refresh();
            }

            public CantileverEditorViewModel OpenEditor()
            {
                Window.OpenRack("CAN-1");
                Window.EditCantileverHere();
                Door.AnswerLastRead();
                Assert.True(Window.ActiveCantileverEditor != null, Window.ActionMessage);
                return Window.ActiveCantileverEditor!;
            }
        }

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

        private static DynamicFieldItem Field(IEnumerable<object> items, string key) => items.OfType<DynamicFieldItem>().Single(i => i.Key == key);

        private static void Write(IEnumerable<object> items, string key, string text)
        {
            var field = Field(items, key);
            field.Text = text;
            field.Commit();
            Assert.Equal(string.Empty, field.Error);
        }

        private static CantileverChoiceItem Choice(IEnumerable<object> items, string key) => items.OfType<CantileverChoiceItem>().Single(i => i.Key == key);

        private static void Pick(IEnumerable<object> items, string key, string search, string id)
        {
            var picker = items.OfType<CantileverSectionPickerItem>().Single(i => i.Key == key);
            picker.Toggle();
            picker.SearchText = search;
            picker.Choose(picker.Choices.Single(c => c.Id == id));
            Assert.Equal(string.Empty, picker.Error);
        }

        // «Lo esencial» of a new line, through the fields of the editor.
        private static void FillEssentials(CantileverEditorViewModel editor)
        {
            Write(editor.StructureItems, "StationCount", "3");
            Write(editor.StructureItems, "Spacing", "96");
            Write(editor.StructureItems, "LevelCount", "3"); // with one level of 24 in the column is shorter than the bracing (the resolver says it)
            Pick(editor.PieceItems, "ess.col", "W10X33", ColumnW);
            Write(editor.PieceItems, "ess.baseLen", "48");
            Pick(editor.PieceItems, "ess.arm", "HSS4X4X_250", ArmHss);
            Write(editor.PieceItems, "ess.cut", "36");
            Write(editor.LevelItems, "ClearHeight", "24"); // the classic opens with a clear of 0, which the station refuses
        }

        // ------------------------------------------------------------------ the door

        [Fact]
        public void ALineOffersToBeEditedHereOnlyWithItsDoorAndOnlyTheAnswerToThatRequestOpensTheEditor()
        {
            var rig = new Rig();
            rig.Window.OpenRack("CAN-1");
            Assert.True(rig.Window.IsCantileverRack);
            Assert.True(rig.Window.ShowCantileverOffer);
            Assert.False(rig.Window.ShowDynamicOffer);

            rig.Window.EditCantileverHere();
            var request = Assert.Single(rig.Door.CantileverReads);
            Assert.Equal(("CAN-1", Session), (request.RackId, request.Session));
            Assert.Empty(rig.Door.Reads);
            Assert.True(rig.Window.IsWaitingForAutoCad);

            // Another number, or the read of Dinámico with the same number, is not for this request.
            var json = new RackProjectStore().Serialize(RackProject.ForCantilever(LineOfTheDrawing()));
            rig.Door.ReadCantilever(CantileverDoorRules.AnswerForOpen(request.RequestId + 9, "CAN-1", RackEmbedDocument.KindCantilever, "Cantilever 1", json));
            rig.Door.Read(new RackDesignRead(request.RequestId, "CAN-1", true, "dynamic", "X", "{}", null));
            Assert.Null(rig.Window.ActiveCantileverEditor);

            rig.Door.AnswerLastRead();
            var editor = Assert.IsType<CantileverEditorViewModel>(rig.Window.ActiveCantileverEditor);
            Assert.False(rig.Window.IsWaitingForAutoCad);
            Assert.True(rig.Window.HasRackEditor);
            Assert.False(rig.Window.ShowCantileverOffer);
            Assert.False(rig.Window.ShowPanel);
            Assert.Equal("Cantilever 1", editor.Name);
            Assert.Equal("Limpio", editor.StatusText);
            Assert.Equal(CantileverEditorViewModel.ArmsTab, editor.SelectedTab); // «Brazos» is the first tab (decision 9)
            Assert.False(editor.HasMissing);
            Assert.False(editor.CanUpdate);
            Assert.Equal("Sin cambios que escribir.", editor.UpdateBlockedText);

            var dynamicOnly = new Rig(new DynamicOnlyDoor());
            dynamicOnly.Window.OpenRack("CAN-1");
            Assert.False(dynamicOnly.Window.ShowCantileverOffer);
            dynamicOnly.Window.EditCantileverHere();
            Assert.False(dynamicOnly.Window.IsWaitingForAutoCad);
        }

        [Fact]
        public void UpdateSendsCopiesLocksTheEditorAndOnlyAWrittenAnswerLeavesItClean()
        {
            var rig = new Rig();
            var editor = rig.OpenEditor();
            Write(editor.StructureItems, "Spacing", "120");
            Assert.Equal("Modificado", editor.StatusText);
            Assert.Contains("Cantilever 1", rig.Window.UnwrittenWorkOf(Session)); // D-08
            Assert.Equal("Modificado (editor de la ventana)", rig.Window.RackList.Groups.SelectMany(g => g.Items).Single(i => i.RackId == "CAN-1").StatusText);

            editor.RequestWrite();

            var write = Assert.Single(rig.Door.CantileverWrites);
            Assert.Equal(("CAN-1", Session, "Cantilever 1"), (write.RackId, write.Session, write.Name));
            Assert.False(write.Inserts);
            Assert.False(write.NewRack);
            Assert.NotSame(editor.Draft.Design, write.Design);
            Assert.Equal(120.0, write.Design.ColumnCentreSpacing);
            Assert.False(write.Line.IsBlocked);
            Assert.True(editor.IsWriting);
            Assert.False(editor.CanEdit);
            Assert.True(rig.Window.CanStopWaiting);

            // While AutoCAD writes nothing is written in the draft.
            var field = Field(editor.StructureItems, "Spacing");
            field.Text = "140";
            field.Commit();
            Assert.Equal(120.0, editor.Draft.Design.ColumnCentreSpacing);

            rig.Door.Start(write.RequestId);
            Assert.False(rig.Window.CanStopWaiting);
            rig.Door.AnswerLastWrite(false, "AutoCAD no redibujó ninguna vista del rack.");
            Assert.False(editor.IsWriting);
            Assert.Equal("Modificado", editor.StatusText);
            Assert.True(editor.MessageIsError);

            editor.RequestWrite();
            rig.Door.AnswerLastWrite(true, "AutoCAD redibujó 3 vistas del rack con el borrador.");
            Assert.Equal("Limpio", editor.StatusText);
            Assert.Equal(rig.Door.CantileverWrites.Last().DesignJson, editor.Draft.BaselineJson);
            Assert.Null(rig.Window.UnwrittenWorkOf(Session));
        }

        [Fact]
        public void StopWaitingForAWriteLeavesTheLineEditableAndALateAnswerChangesNothing()
        {
            var rig = new Rig();
            var editor = rig.OpenEditor();
            Write(editor.StructureItems, "Spacing", "120");
            editor.RequestWrite();
            var write = rig.Door.CantileverWrites.Last();

            rig.Window.StopWaiting();

            Assert.Equal(new[] { write.RequestId }, rig.Door.Abandoned.ToArray());
            Assert.False(editor.IsWriting);
            Assert.True(editor.CanEdit);
            Assert.Equal("Modificado", editor.StatusText);
            rig.Door.Written(new RackDesignWritten(write.RequestId, "CAN-1", true, "tarde"));
            Assert.Equal("Modificado", editor.StatusText);
        }

        [Fact]
        public void ADraftOfAnotherDrawingIsNotWrittenAndAClosedDrawingForgetsItAndSaysWhatWasLost()
        {
            var rig = new Rig();
            var editor = rig.OpenEditor();
            Write(editor.StructureItems, "Spacing", "120");

            rig.Present(8);
            editor.RequestWrite();
            Assert.Empty(rig.Door.CantileverWrites);
            Assert.Equal("Este borrador es de otro dibujo: vuelve a ese dibujo para escribirlo.", editor.Message);

            rig.Summary.Dead.Add(Session);
            rig.Present(8);
            Assert.Empty(rig.Window.CantileverEditors);
            Assert.Contains("Cantilever 1", rig.Window.ActionMessage);
        }

        [Fact]
        public void CloseTheEditorAsksInlineWithChangesAndForgetsTheDraft()
        {
            var rig = new Rig();
            var editor = rig.OpenEditor();
            Write(editor.StructureItems, "Spacing", "120");

            editor.RequestClose();
            Assert.True(editor.IsConfirmingClose);
            Assert.NotNull(rig.Window.ActiveCantileverEditor);
            editor.CancelClose();
            Assert.False(editor.IsConfirmingClose);
            editor.RequestClose();
            editor.ConfirmClose();

            Assert.Null(rig.Window.ActiveCantileverEditor);
            Assert.True(rig.Window.ShowCantileverOffer);
            Assert.Null(rig.Window.UnwrittenWorkOf(Session));
        }

        // ------------------------------------------------------------------ the fields, the matrix and the inspector

        [Fact]
        public void ARefusedValueStaysInItsFieldWithItsReasonAndTheDraftDoesNotChange()
        {
            var rig = new Rig();
            var editor = rig.OpenEditor();
            var before = editor.Draft.DesignJson;

            foreach (var (key, text) in new[] { ("Spacing", "-5"), ("Spacing", "abc"), ("StationCount", "1") })
            {
                var field = Field(editor.StructureItems, key);
                field.Text = text;
                field.Commit();
                Assert.NotEqual(string.Empty, field.Error);
                Assert.Equal(text, field.Text);
            }

            Assert.Equal(before, editor.Draft.DesignJson);
            Assert.Equal("Limpio", editor.StatusText);

            // The dormant fields say why they cannot be written (dolor 7), and a choice of the user goes to the draft at once.
            Assert.True(Field(editor.FaceItems, "ManualHeight").IsReadOnly);
            Choice(editor.FaceItems, "HeightMode").Choose("Manual");
            Assert.False(Field(editor.FaceItems, "ManualHeight").IsReadOnly);
            Assert.True(editor.HasMissing); // a manual height with no value
            Assert.Contains(editor.MissingItems, m => m.Message.StartsWith("Falta la altura manual", StringComparison.Ordinal));
        }

        [Fact]
        public void TheMatrixSelectsWithClickCtrlShiftHeadsAndTodoAndTheInspectorWritesTheSelection()
        {
            var rig = new Rig();
            var editor = rig.OpenEditor();
            Assert.Equal(new[] { "E1", "E2", "E3" }, editor.ColumnHeads.Select(h => h.Title).ToArray());
            Assert.Equal(new[] { "N3", "N2", "N1" }, editor.MatrixRows.Select(r => r.Label).ToArray());
            Assert.Equal("36", editor.MatrixRows[0].Cells[0].Text); // CT-01: the cut length of the arm

            // The default arm changed in «Componentes»: every cell shows it, changed in the draft («•») and with no exception («E»).
            Write(editor.DefaultArmBodyItems, "def.cut", "38");
            Assert.All(editor.MatrixRows.SelectMany(r => r.Cells), c => Assert.Equal(("38", false, true), (c.Text, c.HasException, c.IsModified)));
            editor.Discard();
            Assert.All(editor.MatrixRows.SelectMany(r => r.Cells), c => Assert.Equal(("36", false, false), (c.Text, c.HasException, c.IsModified)));

            var e3n3 = new CantileverLineCell(2, 2, CantileverArmSide.PositiveY);
            editor.ClickCell(e3n3, control: false, shift: false);
            Assert.Equal(new[] { e3n3 }, editor.Selection.ToArray());
            Assert.Equal("E3 · N3 +Y", editor.InspectorTitle);

            // The example of the inventory: E3 · N3 with an arm of 60 in, as the exception of that cell.
            Write(editor.InspectorItems, "insp.cut", "60");
            var cell = editor.MatrixRows[0].Cells[2];
            Assert.Equal("60", cell.Text);
            Assert.True(cell.HasException);
            Assert.True(cell.IsModified);
            Assert.Equal("1 de 1 celda con excepción.", editor.ExceptionText);

            // Ctrl+click adds; Mayús+click from the anchor is the rectangle; the heads select a station or a level; «Todo» the line.
            editor.ClickCell(new CantileverLineCell(0, 0, CantileverArmSide.PositiveY), control: true, shift: false);
            Assert.Equal(2, editor.Selection.Count);
            Assert.Equal("Varía", Field(editor.InspectorItems, "insp.cut").Hint);
            editor.ClickCell(new CantileverLineCell(1, 1, CantileverArmSide.PositiveY), control: false, shift: true);
            Assert.Equal(4, editor.Selection.Count);
            editor.SelectStation(1);
            Assert.Equal(3, editor.Selection.Count);
            Assert.Equal("Estación 2", editor.InspectorTitle);
            editor.SelectLevel(0);
            Assert.Equal("Nivel 1", editor.InspectorTitle);
            editor.SelectAll();
            Assert.Equal(9, editor.Selection.Count);
            Assert.Equal("Toda la línea", editor.InspectorTitle);

            // CT-06: «Todo» writes the exception in every cell; the default arm does not change.
            Write(editor.InspectorItems, "insp.cut", "40");
            Assert.Equal(9, editor.Draft.Matrix.OverrideCount);
            Assert.Equal(36.0, editor.Draft.Design.DefaultArmTemplate.Body.CutLength);
            Assert.All(editor.MatrixRows.SelectMany(r => r.Cells), c => Assert.Equal("40", c.Text));

            // «Volver al brazo por omisión».
            Assert.True(editor.CanRestoreCells);
            editor.RestoreCells();
            Assert.Equal(0, editor.Draft.Matrix.OverrideCount);
            Assert.Contains("9 celdas vuelven al brazo por omisión", editor.Message, StringComparison.Ordinal);

            // The scope widens from the primary cell: a level of the line.
            editor.ClickCell(e3n3, false, false);
            editor.ScopeChoice.Choose("nivel");
            Assert.Equal(3, editor.Selection.Count);
            Assert.All(editor.Selection, c => Assert.Equal(2, c.LevelIndex));
        }

        [Fact]
        public void TheDoubleFaceHasTwoRowsPerLevelAndTheScopeLado()
        {
            var rig = new Rig();
            var editor = rig.OpenEditor();
            Choice(editor.FaceItems, "FaceMode").Choose("Double");
            Assert.Equal(new[] { "N3 +Y", "N3 −Y", "N2 +Y", "N2 −Y", "N1 +Y", "N1 −Y" }, editor.MatrixRows.Select(r => r.Label).ToArray());
            Assert.True(editor.ShowLadoScope);
            Assert.Contains(editor.ScopeChoice.Options, o => o.Value == "lado");
            Assert.True(Choice(editor.FaceItems, "SingleSide").IsReadOnly);
            Assert.Equal("Brazos: estación × nivel × lado", editor.MatrixTitle);
        }

        [Fact]
        public void TheComponentsArePanelsWithTheirRecipeAndTheSectionIsSearchedInAListOfItsFamilies()
        {
            var rig = new Rig();
            var editor = rig.OpenEditor();
            editor.SelectComponent(CantileverDraftEditor.ComponentColumn);
            Assert.True(editor.ShowColumnPanel);
            var column = editor.ColumnItems.OfType<CantileverSectionPickerItem>().Single();
            Assert.Equal("W10X33", column.ShownText);
            column.SearchText = "HSS";
            Assert.Empty(column.Choices); // CT-04: the family is the one of the piece (W)
            Assert.Contains("Columnas", editor.Recipe.Select(r => r.Key));

            editor.SelectComponent(CantileverDraftEditor.ComponentSeparator);
            var separator = editor.SeparatorItems.OfType<CantileverSectionPickerItem>().Single();
            Assert.Equal("C4X4.5", separator.ShownText); // the designation of the catalogue, not «C4X4_5» (dolor 6)
            Assert.All(separator.Choices, c => Assert.Equal("C", c.Family));
            Assert.Contains("Derivado de la línea actual", editor.SeparatorNote, StringComparison.Ordinal);

            editor.SelectComponent(CantileverDraftEditor.ComponentBrace);
            Choice(editor.BraceItems, "BraceKind").Choose("StructuralSection");
            Assert.True(editor.ComponentTabs.Single(t => t.Key == CantileverDraftEditor.ComponentBrace).HasError);
            Assert.Contains(editor.ComponentDiagnostics, d => d.StartsWith("Falta la sección del tensor estructural", StringComparison.Ordinal));
            Assert.False(editor.CanUpdate);
            Assert.Contains("para que la línea se resuelva", editor.UpdateBlockedText, StringComparison.Ordinal);

            // «Ir a …» of the list of what the line lacks takes the user to its panel.
            editor.SelectedTab = CantileverEditorViewModel.LineTab;
            editor.SelectComponent(CantileverDraftEditor.ComponentColumn);
            editor.GoTo(editor.MissingItems.Single());
            Assert.Equal(CantileverEditorViewModel.ComponentsTab, editor.SelectedTab);
            Assert.Equal(CantileverDraftEditor.ComponentBrace, editor.SelectedComponent);
        }

        [Fact]
        public void TheTableOfTramosIsADraftUntilAppliedAndBlocksUpdateMeanwhile()
        {
            var rig = new Rig();
            var editor = rig.OpenEditor();
            Choice(editor.FaceItems, "HeightMode").Choose("Manual");
            Write(editor.FaceItems, "ManualHeight", "264");
            Assert.False(editor.HasMissing);

            // To «Avanzada» with no tramos kept: the sequence of the rule is materialized.
            editor.LayoutChoice.Choose("Advanced");
            Assert.True(editor.IsAdvanced);
            Assert.NotEmpty(editor.SegmentRows);
            Assert.False(editor.SegmentsPending);

            editor.SegmentRows[0].Braced = !editor.SegmentRows[0].Braced;
            Assert.True(editor.SegmentsPending);
            Assert.False(editor.CanUpdate);
            Assert.Equal("Aplica o descarta la secuencia antes de escribir.", editor.UpdateBlockedText);
            Assert.True(editor.IsDirty);

            editor.ApplySegments();
            Assert.False(editor.SegmentsPending);
            Assert.True(editor.CanUpdate);

            // Back to «Automática» asks inline; going back to «Avanzada» offers to recover the tramos.
            editor.LayoutChoice.Choose("Automatic");
            Assert.True(editor.IsAskingToAutomatic);
            Assert.True(editor.IsAdvanced);
            editor.ConfirmToAutomatic();
            Assert.False(editor.IsAdvanced);
            editor.LayoutChoice.Choose("Advanced");
            Assert.True(editor.IsAskingRecover);
            editor.Recover();
            Assert.True(editor.IsAdvanced);
        }

        [Fact]
        public void RestaurarLayoutOpensItsBoxesMarkedSaysWhatIsLostAndRestoresInTheDraft()
        {
            var rig = new Rig();
            var editor = rig.OpenEditor();
            editor.ClickCell(new CantileverLineCell(2, 2, CantileverArmSide.PositiveY), false, false);
            Write(editor.InspectorItems, "insp.cut", "60");
            Choice(editor.FaceItems, "HeightMode").Choose("Manual");
            Write(editor.FaceItems, "ManualHeight", "264");

            editor.OpenRestore();
            Assert.True(editor.IsRestoreOpen);
            Assert.Equal(4, editor.RestoreBoxes.Count);
            Assert.True(editor.RestoreBoxes.All(b => b.IsChecked));
            Assert.Equal(new[] { true, true, false, false }, editor.RestoreBoxes.Select(b => b.IsEnabled).ToArray());
            Assert.Equal(2, editor.LossLines.Count);
            editor.RestoreBoxes[1].IsChecked = false;
            Assert.Single(editor.LossLines);

            editor.RestoreMarked();
            Assert.False(editor.IsRestoreOpen);
            Assert.Equal(0, editor.Draft.Matrix.OverrideCount);
            Assert.Equal(CantileverStationColumnHeightMode.Manual, editor.Draft.Design.StationTopology.ColumnHeight.Mode);
            editor.Discard();
            Assert.Equal("Limpio", editor.StatusText);
        }

        // T6 ui (point 8): a restore that leaves the line as the drawing has it does not name «Descartar», which is off.
        [Fact]
        public void ARestoreThatLeavesTheLineAsTheDrawingDoesNotNameDiscard()
        {
            var rig = new Rig();
            var editor = rig.OpenEditor();
            editor.ClickCell(new CantileverLineCell(2, 2, CantileverArmSide.PositiveY), false, false);
            Write(editor.InspectorItems, "insp.cut", "60");
            Assert.True(editor.CanDiscard);

            editor.OpenRestore();
            editor.RestoreMarked();

            Assert.False(editor.CanDiscard);
            Assert.Equal("Limpio", editor.StatusText);
            Assert.Equal("Layout restaurado en el borrador: 1 casilla.", editor.Message);
        }

        // ------------------------------------------------------------------ a new line of «Nuevo», «Insertar vista…», the BOM and the library

        [Fact]
        public void ANewLineIsADraftThatSaysWhatItLacksAndIsInsertedWithItsViewsAsANewRack()
        {
            var rig = new Rig();
            Assert.True(rig.Window.CanStartNewCantilever);
            rig.Window.StartNewCantilever();

            var editor = Assert.IsType<CantileverEditorViewModel>(rig.Window.NewCantileverEditor);
            Assert.True(rig.Window.ShowNewCantileverEditor);
            var draft = Assert.Single(rig.Window.Drafts);
            Assert.Equal(("Borrador 1", "Cantilever"), (draft.Title, draft.SystemLabel));
            Assert.True(editor.IsNewRack);
            Assert.Equal("Insertar en el dibujo…", editor.PrimaryLabel);
            Assert.Equal(6, editor.MissingItems.Count); // CT-05: the defaults of the classic editor do not resolve
            Assert.Equal("Ir a «Línea» › «Niveles»", editor.MissingItems.Last().GoLabel);
            Assert.False(editor.CanUpdate);
            Assert.Contains("6 cosas faltan", editor.UpdateBlockedText, StringComparison.Ordinal);
            Assert.False(editor.CanInsertView);
            Assert.Null(rig.Window.UnwrittenWorkOf(Session)); // ND-05: a draft with no change does not veto the close

            FillEssentials(editor);
            Assert.False(editor.HasMissing, string.Join(" | ", editor.MissingItems.Select(m => m.Message)));
            Assert.True(editor.CanUpdate);
            Assert.Contains("Borrador 1 (Cantilever nuevo)", rig.Window.UnwrittenWorkOf(Session));

            var panel = editor.InsertWith(RackViewAddress.Whole(DimensionViewKind.Frontal), RackViewAddress.Station(0));
            Assert.NotNull(panel);
            var write = Assert.Single(rig.Door.CantileverWrites);
            Assert.True(write.NewRack);
            Assert.Null(write.Name); // «Borrador N» is never sent: the drawing gives the automatic name of I-60
            Assert.Equal(new[] { RackViewAddress.Whole(DimensionViewKind.Frontal), RackViewAddress.Station(0) }, write.InsertViews!.ToArray());
            Assert.Null(write.Properties);
            Assert.Equal(write.RackId, editor.RackId);

            rig.Door.Start(write.RequestId);
            rig.Door.AnswerLastWrite(true, "AutoCAD insertó el rack nuevo «Cantilever 3» con 2 vistas.", "Cantilever 3");
            Assert.False(editor.IsNewRack);
            Assert.Equal("Cantilever 3", editor.Name);
            Assert.Equal("Limpio", editor.StatusText);
            Assert.Empty(rig.Window.Drafts);
            Assert.Contains("Cantilever 3", rig.Window.ActionMessage);
        }

        [Fact]
        public void InsertarVistaOffersTheStationsAndWritesTheDraftWithTheViews()
        {
            var rig = new Rig();
            var editor = rig.OpenEditor();
            editor.InsertView();
            var panel = editor.InsertPanel!;
            Assert.Equal(new[] { "Frontal", "Planta", "Lateral · estación 1", "Lateral · estación 2", "Lateral · estación 3" },
                editor.InsertOptions().Select(o => o.Label).ToArray());
            panel.Mark(RackViewAddress.Station(2));
            panel.Insert();

            var write = Assert.Single(rig.Door.CantileverWrites);
            Assert.True(write.Inserts);
            Assert.False(write.NewRack);
            Assert.Equal(new[] { RackViewAddress.Station(2) }, write.InsertViews!.ToArray());
            Assert.Null(write.Properties); // VA-S10: only «Actualizar» writes the tab «Propiedades»
            Assert.False(editor.IsInsertPanelOpen); // hidden while AutoCAD writes
            rig.Door.AnswerLastWrite(true, "AutoCAD redibujó 3 vistas del rack con el borrador e insertó 1 vista nueva.");
            Assert.False(editor.IsInsertPanelOpen);
        }

        [Fact]
        public void TheBomOfTheDraftIsTheOneOfTheClassicEditorAndGuardarEnBibliotecaTakesTheUserToTheLibrary()
        {
            var rig = new Rig();
            var editor = rig.OpenEditor();
            var host = (IRackBomPageHost)rig.Window;
            Assert.Null(host.DraftBomOf(Session, "CAN-1"));
            Assert.False(host.HasUnwrittenDraft(Session, "CAN-1"));

            Write(editor.StructureItems, "StationCount", "4");
            var bom = host.DraftBomOf(Session, "CAN-1");
            Assert.NotNull(bom);
            Assert.Equal(CantileverLineBomBuilder.Build(editor.Draft.SnapshotForWrite()!.Line).Components.Count, bom!.Components.Count);
            Assert.True(host.HasUnwrittenDraft(Session, "CAN-1"));
            Assert.Contains(host.UnwrittenDraftsOf(Session), d => d.RackId == "CAN-1");

            editor.ToggleMore();
            Assert.True(editor.CanSaveToLibrary);
            editor.SaveToLibrary();
            Assert.Equal(RackCadWindowPage.Biblioteca, rig.Window.Page);
            Assert.False(editor.IsMoreOpen);
        }

        [Fact]
        public void ADesignOfTheLibraryOpensAsADraftOfNuevoWithItsNameAndNotInTheDrawing()
        {
            // UT-03 for Cantilever: «Abrir» of the library opens the design as one more «Borrador N» (SD-06), never over a line of the drawing.
            var rig = new Rig();
            var design = LineOfTheDrawing();
            design.Name = null;
            Assert.True(rig.Window.OpenDesignAsNewRack(RackProject.ForCantilever(design), "Linea de la nave"));

            var editor = Assert.IsType<CantileverEditorViewModel>(rig.Window.NewCantileverEditor);
            Assert.True(editor.IsNewRack);
            Assert.Equal("Linea de la nave", editor.Name);
            Assert.False(editor.IsDirty);
            Assert.True(editor.Draft.IsResolved);
            Assert.NotEqual(design.Id.ToString(), editor.RackId); // a new identity
            Assert.Contains("del diseño «Linea de la nave» de la biblioteca", editor.Message, StringComparison.Ordinal);
        }

        // T6 ui, point 10 (decided by the Owner on 2026-11-27) and point 5 (UT-S11): «Descartar» of a draft says the words of every draft, and
        // the line under the name does not say «valores por defecto» for a line opened from the library.
        [Fact]
        public void DiscardingADraftSaysThatItGoesBackToHowItOpenedAndALineOfTheLibraryIsNotCalledTheDefaults()
        {
            var rig = new Rig();
            rig.Window.StartNewCantilever();
            var fresh = rig.Window.NewCantileverEditor!;
            Write(fresh.LevelItems, "ClearHeight", "20");
            Assert.True(fresh.IsDirty);
            fresh.Discard();
            Assert.Equal("Cambios descartados: el borrador vuelve a como se abrió.", fresh.Message);

            var design = LineOfTheDrawing();
            Assert.True(rig.Window.OpenDesignAsNewRack(RackProject.ForCantilever(design), "Linea de la nave"));
            var library = rig.Window.NewCantileverEditor!;
            Assert.Equal("Sin cambios desde que se abrió.", library.DiffText);
            Write(library.StructureItems, "Spacing", "120");
            Assert.Equal("Con cambios desde que se abrió.", library.DiffText);
            library.Discard();
            Assert.Equal("Cambios descartados: el borrador vuelve a como se abrió.", library.Message);
            Assert.DoesNotContain("valores por defecto", library.DiffText, StringComparison.Ordinal);
        }

        // T6 ui, correction 1 of the review (sobre 331f9f18): a copy of a draft compares with the base of its original. A copy of a draft
        // with changes is born «Modificado» without anybody touching it: the line under its name says so, and «Descartar» says where it goes.
        [Fact]
        public void TheCopyOfADraftSaysThatItComparesWithHowItsOriginalOpened()
        {
            var rig = new Rig();
            rig.Window.StartNewCantilever();
            var original = rig.Window.NewCantileverEditor!;
            Assert.True(rig.Window.DuplicateDraft(rig.Window.ActiveDraft!).Opened);
            var clean = rig.Window.NewCantileverEditor!;
            Assert.NotSame(original, clean);
            Assert.False(clean.IsDirty);
            Assert.Equal("Sin cambios respecto de cómo se abrió su original.", clean.DiffText);

            rig.Window.OpenDraft(rig.Window.Drafts.Single(d => d.RackId == original.RackId));
            Write(original.LevelItems, "ClearHeight", "20");
            Assert.True(rig.Window.DuplicateDraft(rig.Window.ActiveDraft!).Opened);
            var copy = rig.Window.NewCantileverEditor!;
            Assert.True(copy.IsDirty);
            Assert.Equal("Con cambios respecto de cómo se abrió su original.", copy.DiffText);

            copy.Discard();
            Assert.Equal("Cambios descartados: la copia vuelve a como se abrió su original.", copy.Message);
            Assert.Equal("Sin cambios respecto de cómo se abrió su original.", copy.DiffText);
        }

        [Fact]
        public void TheTabPropiedadesIsPartOfTheDraftAndOnlyActualizarSendsIt()
        {
            // VA-07: RACKCANTILEVERABRIR reads the properties of the rack; they are part of the draft and «Actualizar» writes them.
            var rig = new Rig();
            rig.Window.OpenRack("CAN-1");
            rig.Window.EditCantileverHere();
            var request = rig.Door.CantileverReads.Last();
            var read = CantileverDoorRules.AnswerForOpen(request.RequestId, "CAN-1", RackEmbedDocument.KindCantilever, "Cantilever 1",
                new RackProjectStore().Serialize(RackProject.ForCantilever(LineOfTheDrawing())));
            rig.Door.ReadCantilever(read with
            {
                Properties = new RackPropertiesRead(true, false, null, new[] { new CustomPropertyDraftEntry("6f1c2a3b-0000-4000-8000-000000000001", "Zona", "A") },
                    Array.Empty<RackPropertiesViewSummary>(), 3, "fp-1"),
            });
            var editor = rig.Window.ActiveCantileverEditor!;
            Assert.Equal("Propiedades", editor.PropertiesTab.TabHeader);
            Assert.False(editor.IsDirty);

            editor.PropertiesTab.Properties[0].Value = "B";
            editor.PropertiesTab.CommitProperties();
            Assert.True(editor.IsDirty);
            Assert.Equal("Modificado", editor.StatusText);
            editor.RequestWrite();
            var write = rig.Door.CantileverWrites.Last();
            Assert.Equal("B", Assert.Single(write.Properties!.Entries).Value);

            rig.Door.AnswerLastWrite(true, "AutoCAD redibujó 3 vistas del rack con el borrador.");
            Assert.False(editor.IsWriting);
        }

        [Fact]
        public void ADuplicateOfACantileverDraftIsAnotherDraftWithTheSameDesign()
        {
            var rig = new Rig();
            rig.Window.StartNewCantilever();
            var editor = rig.Window.NewCantileverEditor!;
            FillEssentials(editor);

            var copy = rig.Window.DuplicateDraft(rig.Window.Drafts.Single());
            Assert.True(copy.Opened, copy.Reason);
            var other = rig.Window.NewCantileverEditor!;
            Assert.NotSame(editor, other);
            Assert.True(other.IsDirty);
            Assert.True(other.Draft.IsResolved);
            Assert.Equal(2, rig.Window.Drafts.Count);
            Assert.NotEqual(editor.Draft.Design.Id, other.Draft.Design.Id); // correction 5: an identity of its own (DuplicateWithNewIdentity)
            Assert.Null(other.Draft.Design.Name);

            // The copy of a draft with no change (a design of the library) is clean, as its original.
            var design = LineOfTheDrawing();
            Assert.True(rig.Window.OpenDesignAsNewRack(RackProject.ForCantilever(design), "Linea A"));
            var library = rig.Window.NewCantileverEditor!;
            Assert.False(library.IsDirty);
            var item = rig.Window.Drafts.Single(d => d.Title == "Linea A");
            Assert.True(rig.Window.DuplicateDraft(item).Opened);
            var copyOfLibrary = rig.Window.NewCantileverEditor!;
            Assert.NotSame(library, copyOfLibrary);
            Assert.False(copyOfLibrary.IsDirty);
            Assert.NotEqual(design.Id, copyOfLibrary.Draft.Design.Id);
        }

        // ------------------------------------------------------------------ corrections of the review

        // Correction 4: the rule of RACKEDITAR: the editor shows and writes the name of the envelope; the inner name is not touched.
        [Fact]
        public void TheEditorShowsAndWritesTheNameOfTheEnvelopeAndLeavesTheInnerNameAlone()
        {
            var rig = new Rig();
            rig.Window.OpenRack("CAN-1");
            rig.Window.EditCantileverHere();
            var inner = LineOfTheDrawing();
            inner.Name = null;
            rig.Door.AnswerLastRead("Cantilever 1", inner);
            var editor = rig.Window.ActiveCantileverEditor!;
            Assert.Equal("Cantilever 1", editor.Name);
            Assert.Null(editor.Draft.Design.Name);
            Assert.False(editor.IsDirty);

            Write(editor.StructureItems, "Spacing", "120");
            editor.RequestWrite();
            var write = rig.Door.CantileverWrites.Last();
            Assert.Equal("Cantilever 1", write.Name);
            Assert.Null(write.Design.Name); // a design with no name is not given one
            rig.Door.AnswerLastWrite(true, "AutoCAD redibujó 3 vistas del rack con el borrador.");
            Assert.False(editor.IsDirty);

            // An inner name that differs from the envelope: the editor shows the one of the envelope, as RACKEDITAR.
            var other = new Rig();
            other.Window.OpenRack("CAN-1");
            other.Window.EditCantileverHere();
            var named = LineOfTheDrawing();
            named.Name = "Interno";
            other.Door.AnswerLastRead("Cantilever 1", named);
            Assert.Equal("Cantilever 1", other.Window.ActiveCantileverEditor!.Name);
            Assert.Equal("Interno", other.Window.ActiveCantileverEditor!.Draft.Design.Name);
        }

        // Correction 1, in the editor: «Empezar desde la regla» with a clear of 20 in (a column of 106.666… in), «Con tensores» changed and
        // «Aplicar secuencia»: the line still resolves, with the exact elevations of the rule.
        [Fact]
        public void ApplyingTheSequenceOfTheRuleInTheTableKeepsTheLineResolved()
        {
            var rig = new Rig();
            var editor = rig.OpenEditor();
            Write(editor.LevelItems, "ClearHeight", "20");
            Assert.False(editor.HasMissing);
            editor.LayoutChoice.Choose("Advanced");
            Assert.True(editor.IsAdvanced);
            var exact = editor.Draft.SavedSegments.Select(s => (s.StartElevation, s.EndElevation)).ToList();

            editor.SegmentRows[0].Braced = !editor.SegmentRows[0].Braced;
            editor.ApplySegments();
            Assert.Equal(string.Empty, editor.SegmentError);
            Assert.False(editor.HasMissing, string.Join(" | ", editor.MissingItems.Select(m => m.Message)));
            Assert.Equal(exact, editor.Draft.SavedSegments.Select(s => (s.StartElevation, s.EndElevation)).ToList());

            // «Dividir» halves the exact tramo, and the halves still meet the next tramo exactly.
            editor.SplitSegment(editor.SegmentRows[editor.SegmentRows.Count - 1]);
            editor.ApplySegments();
            Assert.False(editor.HasMissing, string.Join(" | ", editor.MissingItems.Select(m => m.Message)));
            Assert.Equal(exact[exact.Count - 1].EndElevation, editor.Draft.SavedSegments.Last().EndElevation);
        }

        // Correction 3: the names of UI Automation of every field and button of each tab and panel are unique (and none is empty).
        [Fact]
        public void EveryFieldAndButtonOfEachTabAndPanelHasAUniqueName()
        {
            StaTestRunner.Run(() => WithBindingErrors(errors =>
            {
                var rig = new Rig();
                var editor = rig.OpenEditor();
                var view = new CantileverEditorView(editor);
                Lay(view);

                void Check(string where)
                {
                    foreach (var expander in Descendants<Expander>(view)) expander.IsExpanded = true;
                    Lay(view);
                    var elements = Descendants<Control>(view)
                        .Where(c => c is TextBox || c is CheckBox || c is ComboBox || c is Button)
                        .Where(c => Shown(c, view))
                        .Where(c => !IsInside<RackPropertiesTabView>(c))
                        .ToList();
                    var names = elements.Select(Name).ToList();
                    Assert.DoesNotContain(names, string.IsNullOrWhiteSpace);
                    var repeated = names.GroupBy(n => n).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
                    Assert.True(repeated.Count == 0, where + ": " + string.Join(" | ", repeated));
                }

                foreach (var tab in view.Tabs.Items.OfType<TabItem>().Where(t => t != view.RackPropertiesTabItem))
                {
                    view.Tabs.SelectedItem = tab;
                    Lay(view);
                    if (tab == view.ComponentsTab)
                    {
                        foreach (var component in editor.ComponentTabs.Select(t => t.Key).ToList())
                        {
                            editor.SelectComponent(component);
                            Check("Componentes › " + component);
                        }
                    }
                    else
                    {
                        Check(tab.Header?.ToString() ?? string.Empty);
                    }
                }

                // The inspector: the two thicknesses are told apart (they were two «Espesor»).
                view.Tabs.SelectedItem = view.ArmsTab;
                Lay(view);
                var boxes = Descendants<TextBox>(view.InspectorPanel).Select(Name).ToList();
                Assert.Contains("Placa de montaje: espesor", boxes);
                Assert.Contains("Placa de extremo: espesor", boxes);
                Assert.Empty(errors.Messages);
            }));
        }

        // Whether the element and every element above it is visible (Visibility, not IsVisible: there is no window in the test).
        private static bool Shown(DependencyObject element, DependencyObject root)
        {
            for (var current = element; current != null && current != root; current = System.Windows.Media.VisualTreeHelper.GetParent(current))
            {
                if (current is UIElement ui && ui.Visibility != Visibility.Visible) return false;
            }

            return true;
        }

        private static bool IsInside<T>(DependencyObject element) where T : DependencyObject
        {
            for (var current = System.Windows.Media.VisualTreeHelper.GetParent(element); current != null; current = System.Windows.Media.VisualTreeHelper.GetParent(current))
            {
                if (current is T) return true;
            }

            return false;
        }

        // ------------------------------------------------------------------ the views (real WPF)

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

        private static void Lay(FrameworkElement view, double width = 1400, double height = 900)
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

        private static string Name(UIElement element) => UIElementAutomationPeer.CreatePeerForElement(element).GetName();

        private static void WithBindingErrors(Action<BindingErrors> body)
        {
            var errors = new BindingErrors();
            PresentationTraceSources.Refresh();
            var source = PresentationTraceSources.DataBindingSource;
            var level = source.Switch.Level;
            source.Listeners.Add(errors);
            source.Switch.Level = SourceLevels.Error;
            try
            {
                body(errors);
            }
            finally
            {
                source.Listeners.Remove(errors);
                source.Switch.Level = level;
            }
        }

        [Fact]
        public void TheViewHasItsFiveTabsBrazosFirstAndItsGesturesWithUniqueNamesAndNoBindingError()
        {
            StaTestRunner.Run(() => WithBindingErrors(errors =>
            {
                var rig = new Rig();
                var editor = rig.OpenEditor();
                var view = new CantileverEditorView(editor);
                Lay(view);

                Assert.Equal(new[] { "Línea", "Brazos", "Componentes", "Arriostramiento", "Propiedades" },
                    view.Tabs.Items.OfType<TabItem>().Select(t => t.Header?.ToString()).ToArray());
                Assert.Same(view.ArmsTab, view.Tabs.SelectedItem);

                // A click on a cell of the matrix selects it; the inspector follows.
                var cells = Descendants<Button>(view.MatrixRowList).Where(b => b.DataContext is CantileverCellItem).ToList();
                Assert.Equal(9, cells.Count);
                Assert.Equal(cells.Select(c => ((CantileverCellItem)c.DataContext).AccessibleText).ToArray(), cells.Select(Name).ToArray());
                Click(cells[2]);
                Lay(view);
                Assert.Equal("E3 · N3 +Y", view.InspectorTitle.Text);

                // A segment of a choice writes the draft.
                foreach (TabItem tab in view.Tabs.Items)
                {
                    view.Tabs.SelectedItem = tab;
                    Lay(view);
                }

                view.Tabs.SelectedItem = view.LineTab;
                Lay(view);
                var doble = Descendants<Button>(view.FaceList).Single(b => Name(b) == "Caras: Doble");
                Click(doble);
                Lay(view);
                Assert.Equal(CantileverStationFaceMode.Double, editor.Draft.Design.StationTopology.FaceMode);
                Assert.Equal("Modificado", view.StatusText.Text);

                // The list of sections inside the page (CT-04): «Cambiar…», search, a click on a row.
                view.Tabs.SelectedItem = view.ComponentsTab;
                Lay(view);
                var toggle = Descendants<Button>(view.ColumnPanel).First(b => Name(b) == "Elegir la sección: Sección de columna");
                Click(toggle);
                Lay(view);
                var picker = editor.ColumnItems.OfType<CantileverSectionPickerItem>().Single();
                picker.SearchText = "W12X26";
                Lay(view);
                var row = Descendants<Button>(view.ColumnPanel).First(b => b.DataContext is CantileverSectionChoiceItem c && c.Id == BaseW);
                Click(row);
                Lay(view);
                Assert.Equal(BaseW, editor.Draft.Design.StationTopology.ColumnBaseTemplate.ColumnSectionId);

                var named = new UIElement[]
                {
                    view.UpdateButton, view.InsertNewButton, view.InsertViewButton, view.DiscardButton, view.RestoreButton, view.CloseEditorButton,
                    view.MoreButton, view.SaveToLibraryButton, view.CloseYesButton, view.CloseNoButton, view.RestoreConfirmButton, view.RestoreCancelButton,
                    view.LossToggleButton, view.SelectAllButton, view.GoDefaultArmButton, view.RestoreCellsButton, view.GoSeparatorButton,
                    view.ToAutomaticYesButton, view.ToAutomaticNoButton, view.RecoverButton, view.StartFromRuleButton, view.RecoverNoButton,
                    view.AddSegmentButton, view.ApplySegmentsButton, view.DiscardSegmentsButton,
                }.Select(Name).ToList();
                Assert.Equal(named.Count, named.Distinct().Count());
                Assert.DoesNotContain(named, string.IsNullOrWhiteSpace);
                Assert.Empty(errors.Messages);
            }));
        }

        [Fact]
        public void ANewLineInTheViewListsWhatItLacksAndItsGoButtonsTakeTheUserThere()
        {
            StaTestRunner.Run(() => WithBindingErrors(errors =>
            {
                var rig = new Rig();
                rig.Window.StartNewCantilever();
                var editor = rig.Window.NewCantileverEditor!;
                var view = new CantileverEditorView(editor);
                Lay(view, 700, 900); // a narrow palette: the inspector goes below the matrix
                Assert.True(view.InspectorBelow);
                Assert.Equal(Visibility.Visible, view.MissingPanel.Visibility);
                Assert.Equal(Visibility.Visible, view.InsertNewButton.Visibility);
                Assert.Equal(Visibility.Collapsed, view.UpdateButton.Visibility);
                Assert.Equal(Visibility.Collapsed, view.InsertViewButton.Visibility);
                var go = Descendants<Button>(view.MissingList).ToList();
                Assert.Equal(6, go.Count);
                Click(go[3]);
                Lay(view, 700, 900);
                Assert.Same(view.ComponentsTab, view.Tabs.SelectedItem);
                Assert.Equal(CantileverDraftEditor.ComponentArm, editor.SelectedComponent);
                Assert.Empty(errors.Messages);
            }));
        }

        // T6 ui (O-4 a of the smoke of the fifth batch): «Ir a «Línea» › «Niveles»» opened the group, but the group is at the end of the tab and
        // stayed out of view; the operator had to scroll down to it. Now it is also brought into view.
        [Fact]
        public void GoingToTheLevelsOpensTheGroupAndBringsItIntoView()
        {
            StaTestRunner.Run(() => WithBindingErrors(errors =>
            {
                var rig = new Rig();
                rig.Window.StartNewCantilever();
                var editor = rig.Window.NewCantileverEditor!;
                var view = new CantileverEditorView(editor);
                Lay(view, 1290, 860);
                Assert.Same(view.ArmsTab, view.Tabs.SelectedItem);
                Assert.False(view.LevelsExpander.IsExpanded);

                var go = Descendants<Button>(view.MissingList)
                    .Single(b => b.DataContext is CantileverMissingItem item && item.Component == CantileverDraftEditor.ComponentLevels);
                Click(go);
                Lay(view, 1290, 860);

                Assert.Same(view.LineTab, view.Tabs.SelectedItem);
                Assert.True(view.LevelsExpander.IsExpanded);
                var scroll = Descendants<ScrollViewer>(view.Tabs).First(s => IsAncestorOf(s, view.LevelsExpander));
                Assert.True(scroll.ScrollableHeight > 0, "the tab must be taller than its view for the check to mean anything");
                var top = view.LevelsExpander.TranslatePoint(new Point(0, 0), scroll).Y;
                Assert.True(top >= 0 && top < scroll.ViewportHeight, "the group «Niveles» starts at " + top + " in a view of " + scroll.ViewportHeight);
                Assert.Empty(errors.Messages);
            }));
        }

        private static bool IsAncestorOf(DependencyObject ancestor, DependencyObject element)
        {
            for (var node = System.Windows.Media.VisualTreeHelper.GetParent(element); node != null; node = System.Windows.Media.VisualTreeHelper.GetParent(node))
                if (node == ancestor) return true;
            return false;
        }

        [Fact]
        public void ThePageOfTheRackAndNuevoHostTheEditorOfCantilever()
        {
            StaTestRunner.Run(() =>
            {
                var rig = new Rig();
                var view = new RackCadWindowView(rig.Window);
                rig.Window.OpenRack("CAN-1");
                Lay(view, 1300, 900);
                Assert.Equal(Visibility.Visible, view.CantileverOfferPanel.Visibility);
                Assert.Equal("Editar la línea Cantilever en la ventana", Name(view.EditCantileverHereButton));

                Click(view.EditCantileverHereButton);
                rig.Door.AnswerLastRead();
                view.UpdateLayout();
                var hosted = Assert.IsType<CantileverEditorView>(view.RackPageHost.Content);
                Assert.Same(rig.Window.ActiveCantileverEditor, hosted.ViewModel);
                Assert.Equal(Visibility.Collapsed, view.CantileverOfferPanel.Visibility);

                rig.Window.ActiveCantileverEditor!.RequestClose();
                view.UpdateLayout();
                Assert.Same(view.PanelView, view.RackPageHost.Content);

                // «Nuevo» › «Cantilever en esta ventana»: the draft in «Nuevo».
                rig.Window.ShowPage(RackCadWindowPage.Nuevo);
                Lay(view, 1300, 900);
                Assert.Equal("Nueva línea Cantilever en esta ventana", Name(view.NewCantileverHereButton));
                Click(view.NewCantileverHereButton);
                Lay(view, 1300, 900);
                var hostedNew = Assert.IsType<CantileverEditorView>(view.NewCantileverRackHost.Content);
                Assert.Same(rig.Window.NewCantileverEditor, hostedNew.ViewModel);
                Assert.Equal(Visibility.Visible, view.NewCantileverRackHost.Visibility);
            });
        }
    }
}
