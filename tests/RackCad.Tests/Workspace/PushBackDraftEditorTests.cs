using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Systems.PushBack.Draft;
using RackCad.Application.VentanaRackCad;
using RackCad.Domain.Systems.PushBack;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// T5 pb (I-64, fifth batch): the draft of a Push Back of the RackCad window (<see cref="PushBackDraftEditor"/>) and the pure rules of
    /// its door (<see cref="PushBackDoorRules"/>). The oracle of the loads is the seat of the editable state (the pass and the LoadExisting
    /// of the classic window as product code, tied to the window by CT-PBT3A-01), over the corpus of Fixtures/I64; the commands of AutoCAD
    /// are fixed by their text, as the other doors of the window.
    /// </summary>
    public sealed class PushBackDraftEditorTests
    {
        private static PushBackDesign DesignOf(string fixture)
            => new RackProjectStore().Deserialize(new RackEmbedStore().Deserialize(PilotKit.Text(fixture)).Design).PushBackDesign;

        private static string NameOf(string fixture) => new RackEmbedStore().Deserialize(PilotKit.Text(fixture)).Name;

        private static PushBackDraftEditor Loaded(string fixture)
        {
            var draft = new PushBackDraftEditor(PilotKit.Catalog);
            Assert.True(draft.LoadFrom(DesignOf(fixture), NameOf(fixture)), draft.LastError);
            return draft;
        }

        public static TheoryData<string> Corpus()
        {
            var data = new TheoryData<string>();
            foreach (var name in PilotKit.Canonical.Concat(PilotKit.Legacy).Concat(new[] { "pb-20-ab" })) data.Add(name);
            return data;
        }

        // ------------------------------------------------------------------ the loads

        [Theory]
        [MemberData(nameof(Corpus))]
        public void ALoadIsTheLoadOfRackeditarAndARecompositionWithoutChangesKeepsTheDesign(string fixture)
        {
            var draft = Loaded(fixture);
            var classic = PushBackEditableStateSeat.LoadExisting(DesignOf(fixture), NameOf(fixture), PilotKit.Catalog);

            Assert.True(PushBackEditableStateComparer.Instance.Equals(classic, draft.State));
            Assert.Equal(NameOf(fixture), draft.Name);
            var loaded = draft.DesignJson();
            Assert.NotNull(loaded);

            // A gesture that changes nothing writes back what was read.
            Assert.True(draft.Recompose(), draft.LastError);
            Assert.Equal(loaded, draft.DesignJson());
        }

        [Theory]
        [MemberData(nameof(CanonicalCorpus))]
        public void ACanonicalDesignOpenedAndNotTouchedIsWrittenBackByteForByte(string fixture)
        {
            var draft = Loaded(fixture);
            Assert.Equal(PilotKit.Persist(DesignOf(fixture)), PilotKit.Normalize(draft.DesignJson()));
        }

        public static TheoryData<string> CanonicalCorpus()
        {
            var data = new TheoryData<string>();
            foreach (var name in PilotKit.Canonical) data.Add(name);
            return data;
        }

        [Fact]
        public void ANewRackIsTheNewRackOfTheClassicWindowAndReopensAsItWasWritten()
        {
            var draft = PushBackDraftEditor.CreateNew(PilotKit.Catalog);
            Assert.Null(draft.LastError);
            Assert.NotNull(draft.Design);
            Assert.False(draft.IsTwoSided);
            Assert.Equal(1, draft.SlotCount);
            Assert.Equal(4.0, draft.Side(PushBackSide.A).Structure.Fronts[0].FirstLevelHeight); // PB-012: Push Back opens at 4"
            Assert.True(draft.State.SafetySelections.Count > 0); // PB-VAL-04: the defaults of the catalog

            var reopened = new PushBackDraftEditor(PilotKit.Catalog);
            Assert.True(reopened.LoadFrom(new RackProjectStore().Deserialize(draft.DesignJson()).PushBackDesign, null), reopened.LastError);
            Assert.Equal(draft.DesignJson(), reopened.DesignJson());
        }

        [Fact]
        public void TheSnapshotForAutoCadIsACopyThatNothingAfterwardsReaches()
        {
            var draft = Loaded("pb-01-base");
            draft.Name = "  Nave A  ";
            var snapshot = draft.SnapshotForWrite();

            Assert.Equal("Nave A", snapshot.Name);
            Assert.Equal("Nave A", snapshot.System.Name);
            Assert.NotSame(draft.Design, snapshot.Design);
            Assert.NotSame(draft.System, snapshot.System);
            Assert.Equal(draft.DesignJson(), snapshot.DesignJson);

            var sent = snapshot.DesignJson;
            Assert.Equal(1, draft.WriteCells(new[] { (0, 0) }, (_, cell, __) => cell.PalletFront = 50.0));
            Assert.True(draft.Recompose(), draft.LastError);
            Assert.NotEqual(sent, draft.DesignJson());
            Assert.Equal(sent, new RackProjectStore().Serialize(RackProject.ForPushBack(snapshot.Design)));
        }

        // ------------------------------------------------------------------ the gestures

        [Fact]
        public void ACellWriteReachesTheSidesOfTheSelectionAndOnlyThem()
        {
            var draft = Loaded("pb-20-ab");
            Assert.True(draft.IsTwoSided);
            var a = draft.Side(PushBackSide.A).Structure.Fronts[0].Cells[0];
            var b = draft.Side(PushBackSide.B).Structure.Fronts[0].Cells[0];
            Assert.True(draft.CellExists(PushBackSide.A, 0, 0) && draft.CellExists(PushBackSide.B, 0, 0));

            draft.SetSelection(PushBackSideSelection.A);
            draft.WriteCells(new[] { (0, 0) }, (_, cell, __) => cell.PalletFront = 44.0);
            Assert.Equal(44.0, draft.Side(PushBackSide.A).Structure.Fronts[0].Cells[0].PalletFront);
            Assert.NotEqual(44.0, draft.Side(PushBackSide.B).Structure.Fronts[0].Cells[0].PalletFront);

            draft.SetSelection(PushBackSideSelection.Both);
            Assert.Equal(new[] { PushBackSide.A, PushBackSide.B }, draft.EditSides());
            draft.WriteCells(new[] { (0, 0) }, (_, cell, __) => cell.PalletFront = 46.0);
            Assert.Equal(46.0, draft.Side(PushBackSide.A).Structure.Fronts[0].Cells[0].PalletFront);
            Assert.Equal(46.0, draft.Side(PushBackSide.B).Structure.Fronts[0].Cells[0].PalletFront);
            Assert.True(draft.Recompose(), draft.LastError);

            // «Lo esencial» writes the whole rack, whatever the selector says.
            draft.SetSelection(PushBackSideSelection.B);
            draft.WriteCells(new[] { (0, 0) }, (_, cell, __) => cell.ClearHeight = 8.0, allSides: true);
            Assert.Equal(8.0, draft.Side(PushBackSide.A).Structure.Fronts[0].Cells[0].ClearHeight);
            Assert.Equal(8.0, draft.Side(PushBackSide.B).Structure.Fronts[0].Cells[0].ClearHeight);
            Assert.NotNull(a);
            Assert.NotNull(b);
        }

        [Fact]
        public void TheBedsOfACellAreTheTopologyAndTheDirectionOfTheRack()
        {
            var draft = Loaded("pb-20-ab");
            Assert.Equal(PushBackCellBeds.Encontradas, draft.BedsAt(0, 0));

            draft.SetBeds(new[] { (0, 0) }, PushBackCellBeds.CorridaBToA);
            Assert.Equal(PushBackCellTopology.Corrida, draft.Composite.TopologyAt(0, 0));
            Assert.Equal(PushBackRunDirection.BToA, draft.Composite.DirectionAt(0, 0));
            Assert.Equal(PushBackCellBeds.CorridaBToA, draft.BedsAt(0, 0));
            Assert.True(draft.Recompose(), draft.LastError);

            // A run has ONE rear tope, at the high end of the side it flows to (B → A: the end of A).
            Assert.True(draft.TopeDrawnAt(PushBackSide.A, 0, 0));
            Assert.False(draft.TopeDrawnAt(PushBackSide.B, 0, 0));

            draft.SetBeds(new[] { (0, 0) }, PushBackCellBeds.SoloB);
            Assert.Equal(PushBackCellBeds.SoloB, draft.BedsAt(0, 0));
            Assert.False(draft.TopeDrawnAt(PushBackSide.A, 0, 0));
            Assert.True(draft.TopeIntentAt(PushBackSide.A, 0, 0)); // the intent of A sleeps; it comes back with two beds

            var reopened = new PushBackDraftEditor(PilotKit.Catalog);
            Assert.True(draft.Recompose(), draft.LastError);
            Assert.True(reopened.LoadFrom(new RackProjectStore().Deserialize(draft.DesignJson()).PushBackDesign, null), reopened.LastError);
            Assert.Equal(PushBackCellBeds.SoloB, reopened.BedsAt(0, 0));
        }

        [Fact]
        public void TheRearTopeOfACellGoesToTheDesignAndEveryCellAtOnce()
        {
            var draft = PushBackDraftEditor.CreateNew(PilotKit.Catalog);
            Assert.True(draft.TopeDrawnAt(PushBackSide.A, 0, 0)); // a new rack opens with its topes active

            draft.SetTope(new[] { (0, 1) }, false);
            Assert.True(draft.Recompose(), draft.LastError);
            Assert.False(draft.TopeDrawnAt(PushBackSide.A, 0, 1));
            Assert.Contains(draft.Design.RearTope.OffCells, c => c.Frente == 0 && c.Level == 1);

            draft.SetTopeEverywhere(false);
            Assert.True(draft.Recompose(), draft.LastError);
            Assert.Equal(draft.AllCells().Count(), draft.Design.RearTope.OffCells.Count);

            draft.SetTopeEverywhere(true);
            Assert.True(draft.Recompose(), draft.LastError);
            Assert.Empty(draft.Design.RearTope.OffCells);
        }

        [Fact]
        public void TheOwnFondoAndThePalletOfACellAreWrittenAloneAndTheFondoOfARunGoesToItsBed()
        {
            var draft = Loaded("pb-20-ab");
            draft.SetSelection(PushBackSideSelection.Both);
            var before = draft.Side(PushBackSide.A).Structure.Fronts[0].Cells[0].PalletFront;

            draft.SetCellFondo(new[] { (0, 0) }, 4);
            draft.SetDrawPallet(new[] { (0, 0) }, true);
            Assert.Equal(4, draft.CellFondoAt(PushBackSide.A, 0, 0));
            Assert.Equal(4, draft.CellFondoAt(PushBackSide.B, 0, 0));
            Assert.True(draft.Side(PushBackSide.B).Cell(0, 0).DrawPallet);
            Assert.Equal(before, draft.Side(PushBackSide.A).Structure.Fronts[0].Cells[0].PalletFront); // I-41: one property only
            Assert.True(draft.Recompose(), draft.LastError);

            draft.SetBeds(new[] { (0, 0) }, PushBackCellBeds.CorridaAToB);
            draft.SetCellFondo(new[] { (0, 0) }, 6);
            Assert.Equal(6, draft.Composite.CorridaDepthAt(0, 0));
            Assert.Equal(6, draft.CellFondoAt(PushBackSide.A, 0, 0));
            Assert.Equal(4, draft.Side(PushBackSide.A).Cell(0, 0).PalletsDeepOverride); // the fondo of the side sleeps under the run

            draft.SetCellFondo(new[] { (0, 0) }, null);
            Assert.Null(draft.Composite.CorridaDepthAt(0, 0));
        }

        [Fact]
        public void LevelsAndPositionsGoThroughTheStateOfEachSideAndABlankFrontKeepsItsLevels()
        {
            var draft = PushBackDraftEditor.CreateNew(PilotKit.Catalog);
            draft.SetSlotCount(2);
            Assert.True(draft.Recompose(), draft.LastError);
            draft.SetLevels(new[] { 1 }, 5);
            draft.SetPositions(new[] { 1 }, 3);
            Assert.Equal(5, draft.Side(PushBackSide.A).Structure.Fronts[1].LoadLevels);
            Assert.Equal(3, draft.Side(PushBackSide.A).Structure.Fronts[1].PalletCount);
            Assert.Equal(5, draft.Side(PushBackSide.A).PushFronts[1].Cells.Count); // the rear peraltes and the topes follow
            Assert.True(draft.Recompose(), draft.LastError);

            Assert.Null(draft.SetBlank(PushBackSide.A, 1, true));
            draft.SetLevels(new[] { 1 }, 2);
            Assert.Equal(5, draft.Side(PushBackSide.A).Structure.Fronts[1].LoadLevels); // dormant (I-33)
            Assert.Equal("Al menos un frente debe permanecer activo.", draft.SetBlank(PushBackSide.A, 0, true));
            Assert.True(draft.Recompose(), draft.LastError);
        }

        [Fact]
        public void AModuleLengthGoesToTheDraftAtOnceAndAnEmptyOneGivesTheCalculatedBack()
        {
            var draft = Loaded("pb-01-base");
            var separator = draft.ModuleRows().First(r => !r.IsHeader && r.CanEditLength);
            var calculated = separator.Length;

            Assert.Null(draft.SetModuleLength(separator.ModuleId, calculated + 6.0));
            Assert.True(draft.Recompose(), draft.LastError);
            var edited = draft.ModuleRows().First(r => r.ModuleId == separator.ModuleId);
            Assert.Equal(calculated + 6.0, edited.Length, 6);
            Assert.True(edited.IsManualLength);

            Assert.Null(draft.SetModuleLength(separator.ModuleId, null));
            Assert.True(draft.Recompose(), draft.LastError);
            var restored = draft.ModuleRows().First(r => r.ModuleId == separator.ModuleId);
            Assert.Equal(calculated, restored.Length, 6);
            Assert.False(restored.IsManualLength);

            Assert.NotNull(draft.SetModuleLength("NO-EXISTE", 10.0));
            Assert.NotNull(draft.SetModuleLength(separator.ModuleId, -1.0));
        }

        [Fact]
        public void RestoreLayoutGivesBackOnlyWhatIsMarked()
        {
            var draft = Loaded("pb-01-base");
            var separator = draft.ModuleRows().First(r => !r.IsHeader && r.CanEditLength);
            Assert.Null(draft.SetModuleLength(separator.ModuleId, separator.Length + 6.0));
            draft.SetCellFondo(new[] { (0, 0) }, 5);
            draft.WriteCells(new[] { (0, 0) }, (_, cell, __) => cell.BeamLengthOverride = 100.0);
            draft.State.Advanced.SeparatorCountOverride = 3;
            Assert.True(draft.Recompose(), draft.LastError);
            Assert.Equal(4, draft.LayoutLoss(new PushBackLayoutRestore()).Count);

            draft.RestoreLayout(new PushBackLayoutRestore(ModuleLengths: false, CustomHeaders: false, RackParameters: false, CellFondos: true, BeamLengths: false, SideStructure: false));
            Assert.True(draft.Recompose(), draft.LastError);
            Assert.Null(draft.CellFondoAt(PushBackSide.A, 0, 0));
            Assert.Equal(100.0, draft.Side(PushBackSide.A).Structure.Fronts[0].Cells[0].BeamLengthOverride);
            Assert.Equal(3, draft.State.Advanced.SeparatorCountOverride);
            Assert.True(draft.ModuleRows().First(r => r.ModuleId == separator.ModuleId).IsManualLength);

            draft.RestoreLayout(new PushBackLayoutRestore());
            Assert.True(draft.Recompose(), draft.LastError);
            Assert.Null(draft.Side(PushBackSide.A).Structure.Fronts[0].Cells[0].BeamLengthOverride);
            Assert.Null(draft.State.Advanced.SeparatorCountOverride);
            Assert.False(draft.ModuleRows().First(r => r.ModuleId == separator.ModuleId).IsManualLength);
            Assert.Empty(draft.LayoutLoss(new PushBackLayoutRestore()));
            Assert.Equal(Loaded("pb-01-base").DesignJson(), draft.DesignJson());
        }

        [Fact]
        public void ARefusedRecompositionKeepsTheLastValidComputationAndTheCopyTakesTheStateBack()
        {
            var draft = Loaded("pb-01-base");
            var computation = draft.Computation;
            var json = draft.DesignJson();
            var copy = draft.Capture();

            // No front starts at the position 1: the depth layout of the classic editor refuses it, with its message.
            draft.WriteFronts(new[] { 0 }, (_, state, front) => state.Structure.Fronts[front].DepthStartPosition = 3);
            Assert.False(draft.Recompose());
            Assert.StartsWith("No se pudo generar el sistema: ", draft.LastError);
            Assert.Same(computation, draft.Computation);

            draft.Restore(copy);
            draft.InputsRestoredToLastValid();
            Assert.Null(draft.LastError);
            Assert.True(draft.Recompose(), draft.LastError);
            Assert.Equal(json, draft.DesignJson());
        }

        [Fact]
        public void TheSideBIsDeclaredAbsentInEverySlotAndRemovingItKeepsItDormant()
        {
            var draft = PushBackDraftEditor.CreateNew(PilotKit.Catalog);
            draft.SetSlotCount(2);
            draft.SetSideBPresent(true);
            Assert.True(draft.IsTwoSided);
            Assert.False(draft.IsPresent(PushBackSide.B, 0)); // the side B is born absent: its presence is declared front by front
            Assert.Null(draft.SetBlank(PushBackSide.B, 0, false));
            Assert.True(draft.IsPresent(PushBackSide.B, 0));
            Assert.True(draft.Recompose(), draft.LastError);
            Assert.True(draft.Design.IsComposite);

            draft.SetSideBPresent(false);
            Assert.True(draft.Recompose(), draft.LastError);
            Assert.False(draft.Design.IsComposite);
            draft.SetSideBPresent(true);
            Assert.True(draft.IsPresent(PushBackSide.B, 0)); // dormant, not erased
        }

        // ------------------------------------------------------------------ the rules of the door

        [Fact]
        public void TheReadOfTheDoorOpensOnlyAReadablePushBackInsideTheGuaranteeOfTheEditableState()
        {
            var text = PilotKit.Text("pb-01-base");
            var embed = new RackEmbedStore().Deserialize(text);

            Assert.Equal("El rack no es un Push Back: este editor no lo abre.",
                PushBackDoorRules.AnswerForOpen(1, "X", RackEmbedDocument.KindDynamic, "D", embed.Design).Reason);
            Assert.False(PushBackDoorRules.AnswerForOpen(1, "X", RackEmbedDocument.KindPushBack, "P", "no es json").Found);
            Assert.False(PushBackDoorRules.AnswerForOpen(1, "X", RackEmbedDocument.KindPushBack, "P",
                new RackProjectStore().Serialize(RackProject.ForDynamic(new RackCad.Domain.Systems.Dynamic.DynamicRackDesign()))).Found);

            var tail = new RackEmbedStore().Deserialize(PilotKit.Text("pb-21-cola"));
            var refused = PushBackDoorRules.AnswerForOpen(2, "X", tail.Kind, tail.Name, tail.Design);
            Assert.False(refused.Found);
            Assert.EndsWith(PushBackDoorRules.UseClassicText, refused.Reason);

            var opened = PushBackDoorRules.AnswerForOpen(3, "PB-1", embed.Kind, embed.Name, embed.Design);
            Assert.True(opened.Found);
            Assert.Equal(3, opened.RequestId);
            Assert.Equal("PB-1", opened.RackId);
            Assert.Equal(embed.Design, opened.DesignJson);
            Assert.Null(opened.Reason);
        }

        [Fact]
        public void TheWriteOfTheDoorNeedsADraftAndAReadablePushBackAndTheNameFallsBackToTheEnvelope()
        {
            var embed = new RackEmbedStore().Deserialize(PilotKit.Text("pb-01-base"));
            var snapshot = Loaded("pb-01-base").SnapshotForWrite();
            var write = new PushBackDoorWrite(1, new RackCad.Application.Workspace.SessionId(1), "PB-1", "", snapshot.Design, snapshot.System);

            Assert.False(PushBackDoorRules.PlanApply(embed.Kind, embed.Design, null).CanWrite);
            Assert.False(PushBackDoorRules.PlanApply(RackEmbedDocument.KindSelective, embed.Design, write).CanWrite);
            Assert.False(PushBackDoorRules.PlanApply(embed.Kind, "{", write).CanWrite);
            var plan = PushBackDoorRules.PlanApply(embed.Kind, embed.Design, write);
            Assert.True(plan.CanWrite, plan.Reason);
            Assert.NotNull(plan.Project.PushBackDesign);

            Assert.Null(PushBackDoorRules.RefusalOf(write));
            Assert.Equal("Elige al menos una vista para insertar el rack.", PushBackDoorRules.RefusalOf(write with { NewRack = true }));
            Assert.Equal("No hay un borrador válido que escribir.", PushBackDoorRules.RefusalOf(write with { RackId = " " }));

            Assert.Equal("Nave del sobre", PushBackDoorRules.Named(snapshot.System, " ", "Nave del sobre").Name);
            Assert.Equal("Nave A", PushBackDoorRules.Named(snapshot.System, "Nave A", "Nave del sobre").Name);
        }

        // ------------------------------------------------------------------ the commands of AutoCAD, by their text

        private static string Source(params string[] parts)
            => File.ReadAllText(Path.Combine(new[] { PilotKit.RepoRoot().FullName, "src", "RackCad.Plugin" }.Concat(parts).ToArray()));

        [Fact]
        public void TheReadHasNoUndoMarkerAndTheWritesTakeTheirRequestAndSayItBeforeDrawing()
        {
            var commands = Source("VentanaRackCad", "RackCadPushBackDoorCommands.cs");
            Assert.Contains("[CommandMethod(\"RACKPUSHBACKABRIR\", CommandFlags.NoUndoMarker | CommandFlags.NoHistory)]", commands);
            Assert.Contains("[CommandMethod(\"RACKPUSHBACKAPLICAR\")]", commands);
            Assert.Contains("[CommandMethod(\"RACKPUSHBACKINSERTAR\")]", commands);
            Assert.Contains("[CommandMethod(\"RACKPUSHBACKNUEVO\")]", commands);

            // Every write says that it took the request before anything else, then checks the drawing of the request.
            foreach (var marker in new[] { "private static void Write(", "public void InsertNewPushBack(" })
            {
                var start = commands.IndexOf(marker, StringComparison.Ordinal);
                var started = commands.IndexOf("door?.Started(pending.RequestId);", start, StringComparison.Ordinal);
                var drawing = commands.IndexOf("pending.Document != RackCadRackDoorPort.KeyOf(document)", start, StringComparison.Ordinal);
                Assert.True(start >= 0 && started > start && drawing > started, marker);
            }

            // The update compares the views and plans before the drawing half, and answers with the rule of «written».
            var write = commands.Substring(commands.IndexOf("private static void Write(", StringComparison.Ordinal));
            Assert.True(write.IndexOf("RackSiblingDesigns.Compare(", StringComparison.Ordinal) < write.IndexOf("RackPushBackWindowDrawing.Apply(", StringComparison.Ordinal));
            Assert.True(write.IndexOf("PushBackDoorRules.PlanApply(", StringComparison.Ordinal) < write.IndexOf("RackPushBackWindowDrawing.Apply(", StringComparison.Ordinal));
            Assert.Contains("RackDoorRules.AnswerForApply(", write);
            Assert.Contains("RackDoorRules.AnswerForInsert(", write);
            Assert.Contains("RackMenuCommands.DispatchInsertion(document,", commands);
            Assert.DoesNotContain("LockDocument", commands);
            Assert.DoesNotContain("StartTransaction", commands);
        }

        [Fact]
        public void TheDrawingHalfPreflightsEverythingBeforeTheFirstRedrawAndRegeneratesOnce()
        {
            var drawing = Source("RackPushBackWindowDrawing.cs");
            var firstRedraw = drawing.IndexOf("RedrawInPlace(", StringComparison.Ordinal);
            foreach (var preflight in new[]
                     {
                         "RackEmbedDocument.KindPushBack, StringComparison.OrdinalIgnoreCase",
                         "PreflightInnerSources(blocks, RackSystemKind.PushBack, project)",
                         "!IsValidPushBackDescriptor(viewBlock.Embed)",
                         "batchProducts.PrepareAll(batchRequest",
                     })
            {
                var at = drawing.IndexOf(preflight, StringComparison.Ordinal);
                Assert.True(at >= 0 && at < firstRedraw, preflight);
            }

            Assert.Equal(Regex.Matches(drawing, Regex.Escape("RedrawInPlace(")).Count, Regex.Matches(drawing, Regex.Escape("regen: false")).Count);
            Assert.Equal(1, Regex.Matches(drawing, Regex.Escape("document.Editor.Regen()")).Count);
            Assert.Contains("RackCommandSupport.EraseViewBlocks(document, staleViewBlocks)", drawing);
            Assert.Contains("RackViewBatchExecution.Run(document, batchProducts, batchRequest", drawing);

            // The name of the views comes with the system of the write: no file of the window writes a Name in the Plugin.
            foreach (var file in new[] { Source("RackPushBackWindowDrawing.cs"), Source("VentanaRackCad", "RackCadPushBackDoorCommands.cs"),
                         Source("VentanaRackCad", "RackCadRackDoorPort.PushBack.cs") })
                Assert.DoesNotMatch(@"\.\s*Name\s*=(?![=>])", file);
        }

        [Fact]
        public void TheDoorKeepsTheRequestBeforeHandingTheCommandOverAndAbandonDropsIt()
        {
            var port = Source("VentanaRackCad", "RackCadRackDoorPort.PushBack.cs");
            Assert.Contains("return Send(PushBackReadCommand + \" \",", port);
            Assert.Contains("return Send(command + \" \",", port);
            Assert.Contains("private static void AbandonPushBack(long requestId)", port);
            Assert.Contains("AbandonPushBack(requestId);", Source("VentanaRackCad", "RackCadRackDoorPort.cs"));
            Assert.Contains("PushBackDoorRules.RefusalOf(write)", port);
        }
    }
}
